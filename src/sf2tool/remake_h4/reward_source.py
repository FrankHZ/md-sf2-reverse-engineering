"""Pinned reward/growth tables and admission of the source-initial party.

Return only source operands, a newly seeded progress ledger and missing-source
reasons. Partial operands are retained on failure exactly as in the admitted
comparison; source unavailability cannot suppress independent receipt checks.
"""

import re
import subprocess
from pathlib import Path

from sf2tool.h3.growth import _parse_growth_curves, _parse_stats_block
from sf2tool.paths import repo_path
from sf2tool.remake_asset_build import ACCEPTED_UPSTREAM_REPOSITORY
from sf2tool.remake_h4.physical_source import source_operands
from sf2tool.remake_h4.reward_checks import absent, actor
from sf2tool.remake_h4_reference import ROM, UPSTREAM


def load_source(initial, battle, source_root, checks):
    """Return source table mapping, mutable party progress and ordered Unknowns."""
    check, eq, clocks = checks.check, checks.eq, checks.clocks
    unknown = []
    gold_table, after_joins = [], []
    clocks("initial profile", initial)
    clocks("battle admission", battle)
    admitted = initial.get("admittedParty") or {}
    eq(
        "admitted provenance",
        dict(Commit=UPSTREAM, Repository=ACCEPTED_UPSTREAM_REPOSITORY, RomSha256=ROM),
        admitted.get("provenance", absent),
    )
    eq("admitted encounter", "battle-1", admitted.get("encounter", absent))
    eq(
        "initial profile declaration",
        "private-local-map3-r1-party-v1",
        (battle.get("initializationPolicy") or {}).get("Declaration", absent),
    )
    initial_actors = {a.get("id"): a for a in battle.get("actors") or []}
    initial_party = {actor(p): p for p in initial.get("party") or []}
    source = None
    profiles, progress, expected_items, expected_spells = {}, {}, {}, {}
    equates, curves = {}, {}
    enemy_operands = {}
    try:
        item_ids = {
            int(w) & 127 for a in initial_actors.values() for w in a["items"] if int(w) & 128
        }
        source = source_operands(source_root, item_ids) if source_root else None
        check("pinned source operands", True if source else None)
        if source:
            root = Path(source_root)
            root = root.resolve() if root.is_absolute() else repo_path(root)
            disasm = root / "disasm"
            curves = _parse_growth_curves(disasm)
            equates = source["equates"]
            gold_table = [
                int(x)
                for x in re.findall(
                    r"dc\.w\s+(\d+)",
                    (disasm / "data/stats/enemies/enemygold.asm").read_text(encoding="utf-8"),
                )
            ][:103]
            starts = re.split(
                r"^\s*startClass\s+",
                (disasm / "data/stats/allies/allystartdefs.asm").read_text(encoding="utf-8"),
                flags=re.M,
            )[1:]
            definitions = {
                d["actor"]: d["definition"]
                for encounter in admitted.get("encounters", [])
                if encounter.get("encounter") == "battle-1"
                for d in encounter.get("deployments", [])
            }
            enemies = re.split(
                r"^\s*unknownByte\s+",
                (disasm / "data/stats/enemies/enemydefs.asm").read_text(encoding="utf-8"),
                flags=re.M,
            )[1:]
            after_joins = [
                int(x)
                for x in re.findall(
                    r"dc\.b\s+(\d+)",
                    (disasm / "data/battles/global/afterbattlejoins.asm").read_text(
                        encoding="utf-8"
                    ),
                )
            ]
            halved = re.findall(
                r"^\s*battle\s+(\w+)",
                (disasm / "data/battles/global/halvedexpearnedbattles.asm").read_text(
                    encoding="utf-8"
                ),
                re.M,
            )
            check(
                "source Battle01 halves enemy-target EXP",
                1 in [int(x) if x.isdigit() else equates["BATTLE_" + x] for x in halved],
            )
            for who, profile in source["profiles"].items():
                if not who.startswith("ally-"):
                    block = enemies[profile["enemyId"]]
                    enemy_operands[who] = {
                        key: int(re.search(rf"^\s*{key}\s+(\d+)", block, re.M)[1])
                        for key in ("level", "maxHp")
                    }
                    eq(
                        "source enemy admitted reward operands",
                        enemy_operands[who],
                        definitions.get(who, absent),
                        who,
                    )
                    continue
                ally = int(who.split("-")[1])
                block = _parse_stats_block(disasm, ally, profile["classCode"])
                profiles[who] = block
                stats = {k: v["start"] for k, v in block["stats"].items()}
                items = []
                for name, equipped in re.findall(
                    r"^\s+([A-Z_]+)(\|EQUIPPED)?(?:,\s*&)?\s*$", starts[ally], re.M
                ):
                    items.append(equates["ITEM_" + name] | (128 if equipped else 0))
                expected_items[who] = items
                spells = [
                    dict(Value=s["expression"].split("|")[0].lower(), Level=1)
                    for s in block["spells"]
                    if s["level"] == 1
                ]
                words = [
                    equates["SPELL_" + s["expression"]] for s in block["spells"] if s["level"] == 1
                ]
                words += [equates["SPELL_NOTHING"]] * (4 - len(words))
                expected_spells[who] = words
                attack = stats["attack"] + sum(
                    amount
                    for w in items
                    if w & 128
                    for effect, amount in source["items"][w & 127]["effects"]
                    if effect == "INCREASE_ATT"
                )
                eq(
                    "source initial admitted definition",
                    dict(
                        level=1,
                        maxHp=stats["hp"],
                        maxMp=stats["mp"],
                        attack=attack,
                        defense=stats["defense"],
                        agility=stats["agility"],
                        classRule={
                            "SDMN": "UnpromotedSwordsman",
                            "PRST": "UnpromotedPriest",
                            "KNTE": "UnpromotedKnight",
                        }[profile["classCode"]],
                        sourceLoadout=dict(Items=items, Spells=words),
                        spells=spells,
                    ),
                    definitions.get(who, absent),
                    who,
                )
                progress[who] = dict(
                    Level=1,
                    MaxHp=stats["hp"],
                    MaxMp=stats["mp"],
                    BaseAttack=stats["attack"],
                    Defense=stats["defense"],
                    Agility=stats["agility"],
                    Spells=spells,
                    SourceLoadout=dict(Items=items, Spells=words),
                )
                eq(
                    "initial party resources",
                    dict(Exp=0, Kills=0, Defeats=0, Status=0, Progress=None),
                    initial_party.get(who, absent),
                    who,
                )
                eq(
                    "source battle admission resources",
                    dict(
                        exp=0,
                        kills=0,
                        defeats=0,
                        level=1,
                        maxHp=stats["hp"],
                        maxMp=stats["mp"],
                        attack=attack,
                        defense=stats["defense"],
                        status=0,
                        items=items,
                        spells=words,
                        learned=spells,
                    ),
                    initial_actors.get(who, absent),
                    who,
                )
    except (
        OSError,
        ValueError,
        KeyError,
        TypeError,
        IndexError,
        subprocess.SubprocessError,
    ) as error:
        check(
            "pinned source operands",
            False
            if str(error) in ("physical source pin", "physical source modifications")
            else None,
        )
        unknown.append(str(error))
        source = None

    operands = dict(
        physical=source,
        profiles=profiles,
        items=expected_items,
        spells=expected_spells,
        enemies=enemy_operands,
        equates=equates,
        curves=curves,
        gold=gold_table,
        joins=after_joins,
    )
    return operands, progress, unknown
