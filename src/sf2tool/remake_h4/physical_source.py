"""Pinned physical operands and original-rule calculation shared by H4 families.

These routines neither select host evidence nor classify a report. The action
calculation consumes source tables and live operands without owning either input.
"""

import re
import subprocess
from pathlib import Path

from sf2tool.h3.rng import _rng_step
from sf2tool.paths import repo_path
from sf2tool.remake_h4_reference import UPSTREAM, require


def source_operands(source_root, item_ids):
    """Read only the selected source tables; never use candidate damage as an oracle."""
    from sf2tool.compression import decode_stack_compressed
    from sf2tool.h2.battle_terrain import _parse_entries
    from sf2tool.h3.growth import (
        _parse_ally_starts,
        _parse_class_prowess,
        _parse_equates,
        _parse_item_equip_effects,
    )

    root = Path(source_root)
    root = root.resolve() if root.is_absolute() else repo_path(root)
    require(
        subprocess.check_output(["git", "-C", str(root), "rev-parse", "HEAD"], text=True).strip()
        == UPSTREAM,
        "physical source pin",
    )
    require(
        subprocess.run(
            ["git", "-C", str(root), "diff", "--quiet", UPSTREAM, "--", "disasm"], check=False
        ).returncode
        == 0,
        "physical source modifications",
    )
    disasm = root / "disasm"

    def source(path):
        return (disasm / path).read_text(encoding="utf-8")

    equates = _parse_equates(disasm)
    starts = _parse_ally_starts(disasm)
    prowess = _parse_class_prowess(disasm, equates)
    movers = re.findall(
        r"^\s*movetype\s+(\w+)", source("data/stats/allies/classes/classdefs.asm"), re.M
    )
    enemies = re.split(
        r"^\s*unknownByte\s+", source("data/stats/enemies/enemydefs.asm"), flags=re.M
    )[1:]
    placements = source("data/battles/spritesets/spriteset01.asm")
    profiles = {}
    for actor, x, y in re.findall(r"^\s*allyCombatant\s+(\d+),\s*(\d+),\s*(\d+)", placements, re.M):
        class_code, _ = starts[int(actor)]
        class_id = equates["CLASS_" + class_code]
        profiles["ally-" + actor] = dict(
            classCode=class_code,
            prowess=prowess[class_id],
            mover=movers[class_id],
            placement=[int(x), int(y)],
        )
    for index, (code, x, y) in enumerate(
        re.findall(r"^\s*enemyCombatant\s+(\w+),\s*(\d+),\s*(\d+)", placements, re.M)
    ):
        enemy_id = equates["ENEMY_" + code]
        block = enemies[enemy_id]
        expression = re.search(r"^\s*baseProwess\s+([^\s;]+)", block, re.M)[1]
        value = 0
        for term in expression.split("|"):
            value |= equates["PROWESS_" + term]
        profiles["enemy-" + str(index)] = dict(
            enemyCode=code,
            enemyId=enemy_id,
            prowess=value,
            mover=re.search(r"^\s*movetype\s+(\w+)", block, re.M)[1],
            placement=[int(x), int(y)],
        )
    references, definitions = _parse_entries(source("data/battles/terrainentries.asm"))
    terrain_path = dict(definitions)[references[1]]
    terrain = decode_stack_compressed(
        (disasm / terrain_path.replace("\\", "/")).read_bytes(), expected_output_bytes=2304
    ).output
    land = re.findall(
        r"^\s*landEffectAndMoveCost\s+([^\s;]+)",
        source("data/battles/global/landeffectsettingsandmovecosts.asm"),
        re.M,
    )
    critical = []
    for chance, shift in re.findall(
        r"^\s*dc.b\s+(\d+),\s+(\w+)", source("data/stats/allies/classes/criticalhitdefs.asm"), re.M
    ):
        critical.append((int(chance), int(shift) if shift.isdigit() else equates[shift]))
    items, text = {}, source("data/stats/items/itemdefs.asm")
    for item in sorted(item_ids):
        marker = re.search(rf"^\s*;\s*{item}:.*$", text, re.M)
        require(marker is not None, "source equipped item")
        tail = text[marker.end() :]
        end = re.search(r"^\s*;\s*\d+:", tail, re.M)
        block = tail[: end.start()] if end else tail
        range_match = re.search(r"^\s*range\s+(\d+),\s*(\d+)", block, re.M)
        effects, cursed = _parse_item_equip_effects(disasm, item, equates)
        require(range_match is not None, "source weapon range")
        items[item] = dict(
            range=tuple(map(int, range_match.groups())),
            effects=effects,
            cursed=cursed,
            weapon="WEAPON" in re.search(r"^\s*itemType\s+([^\s;]+)", block, re.M)[1],
        )
    return dict(
        profiles=profiles,
        terrain=terrain,
        land=land,
        critical=critical,
        items=items,
        equates=equates,
        terrainPath=terrain_path,
    )


def source_action(source, actors, attacker, target, seed):
    """Selected non-ailment, non-cursed source strike arithmetic and follow-up eligibility."""
    hp = {actor: int(row["hp"]) for actor, row in actors.items()}
    draws, strikes = [], []
    profiles, equates = source["profiles"], source["equates"]
    archers = {"ARCHER", "BRASS_GUNNER", "CENTAUR_ARCHER", "STEALTH_ARCHER"}
    airborne = {"FLYING", "HOVERING"}

    def range_for(actor):
        words = [int(word) for word in actors[actor]["items"] if int(word) & 128]
        require(len(words) <= 1, "physical selected equipment multiplicity")
        if not words:
            return (1, 1)
        item = source["items"][words[0] & 127]
        require(
            item["weapon"]
            and not item["cursed"]
            and all(kind in ("NONE", "INCREASE_ATT") for kind, _ in item["effects"]),
            "physical equipment requires a separate source effect",
        )
        return item["range"]

    def in_range(actor, other):
        distance = abs(actors[actor]["x"] - actors[other]["x"]) + abs(
            actors[actor]["y"] - actors[other]["y"]
        )
        low, high = range_for(actor)
        return low <= distance <= high

    def roll(purpose, bound, actor, other):
        nonlocal seed
        before = seed
        word, value = _rng_step(seed >> 16, (bound * 2) & 65535)
        seed = (word << 16) | (seed & 65535)
        value >>= 1
        draws.append(
            dict(
                Kind="rng-" + purpose,
                Actor={"Value": actor},
                Target={"Value": other},
                Before=before,
                After=seed,
                RandomRange=bound,
                RandomValue=value,
            )
        )
        return value

    def strike(kind, actor, other, counter=False):
        attacker_profile, defender_profile = profiles[actor], profiles[other]
        attack, defense = actors[actor], actors[other]
        before_hp = hp[other]
        asleep = int(defense["status"]) & (
            equates["STATUSEFFECT_SLEEP"] | equates["STATUSEFFECT_STUN"]
        )
        muddled = int(attack["status"]) & equates["STATUSEFFECT_MUDDLE"]
        dodge_range = (
            2
            if muddled
            else 8
            if defender_profile["mover"] in airborne and attacker_profile["mover"] not in archers
            else 32
        )
        dodged = False if asleep else roll("dodge", dodge_range, actor, other) == 0
        damage, critical = 0, False
        x, y = int(defense["x"]), int(defense["y"])
        require(0 <= x < 48 and 0 <= y < 48, "physical source terrain coordinate")
        tile = source["terrain"][y * 48 + x]
        require(tile < 16, "physical source terrain category")
        land = source["land"][equates["MOVETYPE_" + defender_profile["mover"]] * 16 + tile]
        require(
            land.startswith(("LE0|", "LE15|", "LE30|")),
            "physical target occupies obstructed terrain",
        )
        multiplier = 256 if land.startswith("LE0|") else 230 if land.startswith("LE15|") else 205
        if not dodged:
            damage = max(1, int(attack["attack"]) - int(defense["defense"])) * multiplier // 256
            if defender_profile["mover"] in airborne and attacker_profile["mover"] in archers:
                damage += damage >> 2
            setting = attacker_profile["prowess"] & 15
            require(setting < 9, "physical ailment prowess requires a separate source effect")
            chance, shift = source["critical"][setting]
            critical = bool(chance and roll("critical", chance, actor, other) == 0)
            if critical:
                damage += damage >> shift
            if counter:
                damage >>= 1
            spread = damage // 8 + 1
            damage -= roll("spread-1", spread, actor, other)
            damage -= roll("spread-2", spread, actor, other)
            damage = max(1, damage)
        hp[other] = max(0, before_hp - damage)
        twice = response = False
        if hp[other]:
            twice = (
                roll("double", (32, 16, 8, 4)[(attacker_profile["prowess"] >> 4) & 3], actor, other)
                == 0
            )
            response = (
                roll(
                    "counter", (32, 16, 8, 4)[(defender_profile["prowess"] >> 6) & 3], actor, other
                )
                == 0
            )
        strikes.append(
            dict(
                kind=kind,
                actor=actor,
                target=other,
                beforeHp=before_hp,
                afterHp=hp[other],
                damage=damage,
                dodge=dodged,
                critical=critical,
                terrain=tile,
                land=land,
                multiplier=multiplier,
            )
        )
        return twice, response

    require(hp[attacker] > 0 and hp[target] > 0, "physical living actor and target")
    legal_range = in_range(attacker, target)
    range_for(target)
    same_side = attacker.startswith("ally-") == target.startswith("ally-")
    muddled = bool(int(actors[attacker]["status"]) & equates["STATUSEFFECT_MUDDLE"])
    twice, counter = strike("physical-first", attacker, target)
    if twice and hp[target] and not muddled and not same_side:
        _, second_counter = strike("physical-second", attacker, target)
        counter |= second_counter
    blocked_enemy = profiles[target].get("enemyId") in {
        equates["ENEMY_BURST_ROCK"],
        equates["ENEMY_KRAKEN_HEAD"],
        equates["ENEMY_PRISM_FLOWER"],
        equates["ENEMY_ZEON_GUARD"],
    }
    counter_eligible = (
        hp[target] > 0
        and not muddled
        and not same_side
        and not blocked_enemy
        and (profiles[attacker].get("enemyId") != equates["ENEMY_TAROS"])
        and not int(actors[target]["status"])
        & (equates["STATUSEFFECT_SLEEP"] | equates["STATUSEFFECT_STUN"])
    )
    if counter and counter_eligible and in_range(target, attacker):
        strike("physical-counter", target, attacker, True)
    return dict(draws=draws, strikes=strikes, rangeLegal=legal_range, seed=seed)
