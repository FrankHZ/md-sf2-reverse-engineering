"""Selected encounter preparation and source battle route compilation."""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import subprocess
from collections import Counter
from pathlib import Path

from sf2tool.compression import decode_stack_compressed
from sf2tool.h2.battle_global_data import _arguments, _tokens
from sf2tool.paths import repo_path
from sf2tool.remake_asset_build import (
    ACCEPTED_ROM_SHA256,
    ACCEPTED_ROM_SIZE,
    ACCEPTED_UPSTREAM_COMMIT,
    ACCEPTED_UPSTREAM_REPOSITORY,
)


def prepare_encounter(upstream: Path, rom: bytes, battle: int, output: Path) -> dict:
    """Reuse the selected encounter transport for admitted overworld slots.

    Source macros and the pinned split ranges own placement and terrain. No H1
    rebuild or mutation of the canonical source tree is required.
    """
    if battle not in (2, 3):
        raise ValueError("only selected overworld encounters 2 and 3 are admitted here")
    root = repo_path("").resolve()
    output = output.resolve()
    upstream = upstream.resolve()
    if (
        not output.is_relative_to(root / "local")
        or output.exists()
        or output.is_relative_to(upstream)
    ):
        raise ValueError("output must be a new worktree-local ignored directory")
    subprocess.run(["git", "check-ignore", "--quiet", str(output)], cwd=root, check=True)
    commit = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=upstream, text=True).strip()
    if commit != ACCEPTED_UPSTREAM_COMMIT:
        raise ValueError("pinned source commit mismatch")
    if (
        len(rom) != ACCEPTED_ROM_SIZE
        or hashlib.sha256(rom).hexdigest().upper() != ACCEPTED_ROM_SHA256
    ):
        raise ValueError("ROM identity mismatch")

    def source_text(relative: str) -> str:
        raw = (upstream / relative).read_bytes()
        pinned = subprocess.check_output(["git", "show", f"{commit}:{relative}"], cwd=upstream)
        if raw.replace(b"\r\n", b"\n") != pinned.replace(b"\r\n", b"\n"):
            raise ValueError("selected pinned source has local changes: " + relative)
        return raw.decode("utf-8")

    source_path = f"data/battles/spritesets/spriteset{battle:02d}.asm"
    source_bytes = (upstream / "disasm" / source_path).read_bytes()
    source = source_text("disasm/" + source_path)

    def sha(data: bytes) -> str:
        return hashlib.sha256(data).hexdigest().upper()

    bounds = re.search(r"0x([0-9A-Fa-f]+)\.\.0x([0-9A-Fa-f]+)", source)
    begin, end = (int(value, 16) for value in bounds.groups())
    counts = [int(value) for value in _arguments(source, "dc.b")[:4]]
    if counts != {2: [4, 6, 0, 1], 3: [12, 6, 1, 0]}[battle]:
        raise ValueError("selected spriteset header drift")
    entities = []
    pattern = (
        r"^\s*(allyCombatant|enemyCombatant)\s+([^\n;]+)\s*\n"
        r"\s*combatantAiAndItem\s+([^\n;]+)\s*\n\s*combatantBehavior\s+([^\n;]+)"
    )
    for kind, identity, ai, behavior in re.findall(pattern, source, re.MULTILINE):
        identity, ai, behavior = map(_tokens, (identity, ai, behavior))
        entities.append(
            dict(
                id=len(entities),
                kind="ally" if kind == "allyCombatant" else "enemy",
                identityExpression=identity[0],
                x=int(identity[1]),
                y=int(identity[2]),
                aiCommandsetExpression=ai[0],
                itemExpression=ai[1],
                behavior=dict(
                    primaryOrderExpression=behavior[0],
                    primaryRegion=int(behavior[1]),
                    secondaryOrderExpression=behavior[2],
                    secondaryRegion=int(behavior[3]),
                    filler=int(behavior[4]),
                    spawnExpression=behavior[5],
                ),
            )
        )
    region_bytes = (
        [
            int(value)
            for row in _arguments(source.split("; AI Regions")[1].split("; AI Points")[0], "dc.b")
            for value in _tokens(row)
        ]
        if counts[2]
        else []
    )
    regions = [
        dict(
            id=index,
            vertexCount=values[0],
            unknown=values[1],
            vertices=[dict(x=values[n], y=values[n + 1]) for n in range(2, 10, 2)],
            trailingBytes=values[10:12],
        )
        for index in range(counts[2])
        for values in [region_bytes[index * 12 : (index + 1) * 12]]
    ]
    points = (
        [
            dict(zip(("x", "y"), map(int, _tokens(row)), strict=True))
            for row in _arguments(source.split("; AI Points")[1], "dc.b")
        ]
        if counts[3]
        else []
    )
    if (
        len(entities) != sum(counts[:2])
        or len(points) != counts[3]
        or len(region_bytes) != counts[2] * 12
        or end - begin != 4 + len(entities) * 12 + counts[2] * 12 + counts[3] * 2
    ):
        raise ValueError("selected spriteset record drift")
    provenance = dict(repository=ACCEPTED_UPSTREAM_REPOSITORY, commit=ACCEPTED_UPSTREAM_COMMIT)
    code = {2: "TO_YEEL", 3: "TO_HAWEL_HOUSE"}[battle]
    identity = dict(id=battle, code=code)

    def span(start: int, end: int) -> dict:
        return dict(start=start, endExclusive=end, lengthBytes=end - start)

    placement = dict(
        schemaVersion=1,
        provenance=dict(**provenance, sourcePath=source_path, sourceSha256=sha(source_bytes)),
        battle=identity,
        romRange=span(begin, end),
        counts=dict(zip(("allies", "enemies", "aiRegions", "aiPoints"), counts, strict=True)),
        entities=entities,
        aiRegions=regions,
        aiPoints=points,
    )
    terrain_path = f"data/battles/entries/battle{battle:02d}/terrain.bin"
    split = re.search(
        r"^#split\s+0x([0-9A-Fa-f]+),0x([0-9A-Fa-f]+),[^\n]*" + re.escape(terrain_path) + r"\s*$",
        source_text("split/sf2splits.txt"),
        re.MULTILINE,
    )
    begin, end = (int(value, 16) for value in split.groups())
    compressed = rom[begin:end]
    terrain = decode_stack_compressed(compressed, expected_output_bytes=2304).output
    global_root = "disasm/data/battles/global/"
    coords = list(
        map(
            int,
            _tokens(
                _arguments(
                    source_text(global_root + "battlemapcoords.asm"), "battleMapCoordinates"
                )[battle]
            ),
        )
    )
    scene = dict(
        schemaVersion=1,
        provenance=dict(
            **provenance, terrainSourcePath=terrain_path, terrainSourceSha256=sha(compressed)
        ),
        battle=identity,
        map=dict(
            zip(("id", "x", "y", "width", "height", "triggerX", "triggerY"), coords, strict=True)
        ),
        scene=dict(
            customBackgroundExpression=_arguments(
                source_text(global_root + "custombackgrounds.asm"), "background"
            )[battle],
            enemyLeaderPresent=int(
                _arguments(source_text(global_root + "enemyleaderpresence.asm"), "dc.b")[battle]
            )
            != 0,
            halfExperience=code
            in _arguments(source_text(global_root + "halvedexpearnedbattles.asm"), "battle"),
        ),
        terrain=dict(
            compressedRange=span(begin, end),
            compressedSha256=sha(compressed),
            decompressedWidth=48,
            decompressedHeight=48,
            decompressedLengthBytes=2304,
            decompressedSha256=sha(terrain),
            valueCounts={str(k): v for k, v in sorted(Counter(terrain).items())},
        ),
    )
    output.mkdir(parents=True)
    for name, data in [("placement.json", placement), ("scene.json", scene)]:
        (output / name).write_text(json.dumps(data, separators=(",", ":")) + "\n", encoding="utf-8")
    (output / "terrain.bin").write_bytes(compressed)
    return dict(
        placementSha256=sha((output / "placement.json").read_bytes()),
        sceneSha256=sha((output / "scene.json").read_bytes()),
        terrainSha256=sha(compressed),
        sourceSha256=sha(source_bytes),
        terrainRange=span(begin, end),
        placementRange=placement["romRange"],
    )


def _location(program: str, instruction: int = 0) -> dict:
    return {"program": program.lower().replace("_", "-"), "instruction": instruction}


def compile_routes(compiler, selection, upstream):
    battle_routes = {}
    compiler.sources.update(
        (
            "disasm/code/common/tech/graphics/flashwhite.asm",
            "disasm/code/common/scripting/map/mapscriptengine_1.asm",
        )
    )
    compiler.programs["source-random-battle-presentation"] = {
        "id": "source-random-battle-presentation",
        "source": "disasm/code/common/scripting/map/mapsetupsfunctions_1.asm:CheckRandomBattle",
        "entitiesRunning": True,
        "instructions": [
            {"op": "wait-view"},
            {
                "op": "present",
                "kind": "Sound",
                "resource": "SFX_BOOST",
                "entity": None,
                "position": None,
            },
            {
                "op": "present",
                "kind": "FlashWhite",
                "resource": "white-four-pulses",
                "entity": None,
                "position": None,
                "durationTicks": 32,
            },
            {"op": "end"},
        ],
    }
    load_source = "disasm/code/gameflow/battle/battlefunctions/loadBattle.asm"
    compiler.sources.update((load_source, "disasm/code/gameflow/battle/battleloop_1.asm"))
    compiler.programs["source-battle-load"] = {
        "id": "source-battle-load",
        "source": load_source + ":LoadBattle",
        "entitiesRunning": False,
        "instructions": [
            {
                "op": "present",
                "kind": "FadeOut",
                "resource": "black",
                "entity": None,
                "position": None,
            },
            {
                "op": "present",
                "kind": "BattleLoad",
                "resource": None,
                "entity": None,
                "position": None,
            },
            {
                "op": "present",
                "kind": "FadeIn",
                "resource": "black",
                "entity": None,
                "position": None,
            },
            {"op": "end"},
        ],
    }
    coordinates_path = "disasm/data/battles/global/battlemapcoords.asm"
    compiler.sources.add(coordinates_path)
    coordinates = _arguments(
        (upstream / coordinates_path).read_text(encoding="utf-8"), "battleMapCoordinates"
    )
    for battle_id in selection["battles"]:
        map_id, origin_x, origin_y, _, _, trigger_x, trigger_y = map(
            compiler.number, _tokens(coordinates[battle_id])
        )
        if (trigger_x, trigger_y) != (255, 255):
            raise ValueError("coordinate-triggered battle selection is not admitted")
        hooks = []
        for hook in ("beforebattle", "battlestart"):
            path = "disasm/data/battles/cutscenes/" + hook + "cutscenes.asm"
            compiler.sources.add(path)
            expressions = _arguments((upstream / path).read_text(encoding="utf-8"), "dc.w")
            symbol = re.search(r"[A-Za-z_]\w*", expressions[battle_id]).group(0)
            hooks.append(_location(compiler.compile(symbol)))
        route = {
            "encounter": "battle-" + str(battle_id),
            "unlockedFlag": 400 + battle_id,
            "completedFlag": 500 + battle_id,
            "introFlag": 450 + battle_id,
            "before": hooks[0],
            "start": hooks[1],
            "load": _location("source-battle-load"),
        }
        if "outcomeEgressMap" in selection and battle_id in selection.get(
            "outcomeBattles", selection["battles"]
        ):
            compiler.sources.update(
                (
                    "disasm/code/gameflow/battle/battleloop_2.asm",
                    "disasm/code/gameflow/battle/battlefunctions/executeindividualturn.asm",
                    "disasm/code/gameflow/battle/cutscenes/afterbattlecutscenesstart.asm",
                    "disasm/code/gameflow/battle/cutscenes/afterbattlecutscenesend.asm",
                    "disasm/code/gameflow/battle/battleloop/getegresspositionforbattle.asm",
                    "disasm/code/common/maps/egressinit.asm",
                    "disasm/code/common/maps/mapinit_0.asm",
                    "disasm/code/common/windows/windowengine.asm",
                    "disasm/code/common/tech/interrupts/vintengine_2.asm",
                )
            )
            outcome_hooks = []
            for hook in ("battleend", "afterbattle"):
                path = "disasm/data/battles/cutscenes/" + hook + "cutscenes.asm"
                compiler.sources.add(path)
                expressions = _arguments((upstream / path).read_text(encoding="utf-8"), "dc.w")
                symbol = re.search(r"[A-Za-z_]\w*", expressions[battle_id]).group(0)
                outcome_hooks.append(_location(compiler.compile(symbol)))
            joins_path = "disasm/data/battles/cutscenes/afterbattlejoins.asm"
            compiler.sources.add(joins_path)
            joins = _arguments((upstream / joins_path).read_text(encoding="utf-8"), "dc.b")
            savepoints_path = "disasm/data/maps/global/savepointmapcoords.asm"
            compiler.sources.add(savepoints_path)
            savepoints = [
                list(map(compiler.number, _tokens(row)))
                for row in _arguments(
                    (upstream / savepoints_path).read_text(encoding="utf-8"),
                    "savePointMapCoordinates",
                )
            ]
            egress, ex, ey, facing = next(
                row for row in savepoints if row[0] == selection["outcomeEgressMap"]
            )
            route["outcome"] = {
                "after": outcome_hooks[1],
                "defeated": outcome_hooks[0],
                "joinMember": compiler.number(joins[battle_id]),
                "victoryFacing": compiler.equates["DOWN"],
                "battleOrigin": {"x": origin_x, "y": origin_y},
                "defeat": _location("source-ordinary-defeat"),
                "return": _location("source-outcome-return"),
                "egress": {
                    "map": "map-" + str(egress),
                    "position": {"x": ex, "y": ey},
                    "facing": facing,
                },
            }
        battle_routes.setdefault(map_id, []).append(route)
    if "outcomeEgressMap" in selection:
        compiler.programs["source-ordinary-defeat"] = {
            "id": "source-ordinary-defeat",
            "source": "disasm/code/gameflow/battle/battleloop_2.asm:BattleLoop_Defeat",
            "entitiesRunning": False,
            "instructions": [
                {
                    "op": "present",
                    "kind": "Sound",
                    "resource": "MUSIC_SAD_THEME_2",
                    "entity": None,
                    "position": None,
                },
                {"op": "text-cursor", "text": 363},
                {"op": "show-text", "mode": "single", "speaker": None},
                {"op": "close-text"},
                {"op": "end"},
            ],
        }
        compiler.programs["source-outcome-return"] = {
            "id": "source-outcome-return",
            "source": "disasm/code/gameflow/exploration/explorationfunctions_2.asm:ExplorationLoop",
            "entitiesRunning": False,
            "instructions": [
                {
                    "op": "present",
                    "kind": "FadeOut",
                    "resource": "black",
                    "entity": None,
                    "position": None,
                },
                {"op": "battle-return-map"},
                {
                    "op": "present",
                    "kind": "FadeIn",
                    "resource": "black",
                    "entity": None,
                    "position": None,
                },
                {"op": "end"},
            ],
        }
    return battle_routes


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--upstream", type=Path, required=True)
    parser.add_argument("--rom", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--battle", type=int, choices=(2, 3), default=2)
    args = parser.parse_args()
    print(
        json.dumps(
            prepare_encounter(args.upstream, args.rom.read_bytes(), args.battle, args.output)
        )
    )
