"""Prepare selected private original exploration content; never read comparison fixtures.

The registered canonical export supplies actual layouts, entity records and ordered program
operations. This offline boundary verifies that export and the pinned source checkout. Runtime
receives normalized immutable definitions; controlled starts are a separate reference input.
Unimplemented native operations remain executable stop instructions at their source location.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import subprocess
from pathlib import Path
from typing import Any

from sf2tool.h2.ally_data import _growth_facts
from sf2tool.h2.battle_global_data import _arguments, _tokens
from sf2tool.h2.map_import import MANIFEST, _canonical_bytes
from sf2tool.jsonio import load_json
from sf2tool.private_inputs import ROM_INPUT_IDENTITY, private_input_path
from sf2tool.remake_asset_build import (
    ACCEPTED_ROM_SHA256,
)
from sf2tool.remake_exploration_battles import compile_routes
from sf2tool.remake_exploration_maps import compile_maps
from sf2tool.remake_exploration_programs import OriginalPrograms as OriginalPrograms
from sf2tool.remake_exploration_visuals import prepare_font, prepare_visuals


def _location(program: str, instruction: int = 0) -> dict[str, Any]:
    return {"program": program.lower().replace("_", "-"), "instruction": instruction}


def selected_map_palette_bindings(
    compiler: OriginalPrograms, maps: list[int], rom_path: Path | None = None
) -> list[dict[str, Any]]:
    """Read only the two logical words used by ExplorationLoop's return comparison.

    The caller supplies the already verified pinned compiler source. This small
    metadata entry does not export assets, select a period, or read a checkpoint.
    Sources join the compiler's existing provenance collection.
    """
    if len(set(maps)) != len(maps) or any(not 0 <= map_id <= 255 for map_id in maps):
        raise ValueError("unique supported map ids required")
    rom = rom_path.read_bytes() if rom_path is not None else None
    if rom is not None and (
        hashlib.sha256(rom).hexdigest().upper() != ACCEPTED_ROM_SHA256
        or int.from_bytes(rom[409604:409608], "big") != 608586
    ):
        raise ValueError("map palette ROM/pointer identity mismatch")
    result = []
    for map_id in maps:
        source = f"disasm/data/maps/entries/map{map_id:02d}/00-tilesets.asm"
        palette_ids = _arguments(
            (compiler.upstream / source).read_text(encoding="utf-8"), "mapPalette"
        )
        if len(palette_ids) != 1:
            raise ValueError("map palette selection must be singular")
        palette_id = compiler.number(palette_ids[0])
        palette = f"disasm/data/graphics/maps/mappalettes/mappalette{palette_id:02d}.bin"
        if rom is None:
            data = (compiler.upstream / palette).read_bytes()
            compiler.sources.add(palette)
        else:
            address = int.from_bytes(rom[608586 + palette_id * 4 : 608590 + palette_id * 4], "big")
            data = rom[address : address + 32]
            compiler.sources.add("disasm/data/graphics/maps/mappalettes/entries.asm")
        if len(data) != 32:
            raise ValueError("map palette must contain 16 words")
        pair = {
            "color2": int.from_bytes(data[4:6], "big"),
            "color3": int.from_bytes(data[6:8], "big"),
        }
        if any(value & ~0xEEE for value in pair.values()):
            raise ValueError("unsupported map CRAM word")
        compiler.sources.add(source)
        result.append({"map": f"map-{map_id}", "base": pair})
    return result


def _selected_growth(
    compiler: OriginalPrograms, selections: list[dict[str, Any]]
) -> list[dict[str, Any]]:
    if not selections:
        return []
    curves_path = "disasm/data/stats/allies/growthcurves.asm"
    compiler.sources.update((curves_path, "disasm/code/common/stats/levelup.asm"))
    curves = _growth_facts((compiler.upstream / curves_path).read_text(encoding="utf-8"))["curves"]
    result = []
    for selected in selections:
        path = f"disasm/data/stats/allies/stats/allystats{selected['member']:02d}.asm"
        compiler.sources.add(path)
        text = (compiler.upstream / path).read_text(encoding="utf-8")
        blocks = re.split(
            r"^(?:[A-Za-z_]\w*:[ \t]*)?[ \t]*forClass[ \t]+(\w+)[ \t]*$", text, flags=re.MULTILINE
        )
        classes = dict(zip(blocks[1::2], blocks[2::2], strict=True))
        block = classes[selected["class"]]
        stats = []
        for name in ("hpGrowth", "mpGrowth", "attGrowth", "defGrowth", "agiGrowth"):
            initial, projected, curve = _tokens(_arguments(block, name)[0])
            index = (
                compiler.number("GROWTHCURVE_" + curve) & compiler.equates["GROWTHCURVE_MASK_INDEX"]
            )
            stats.append(
                {
                    "start": compiler.number(initial),
                    "projected": compiler.number(projected),
                    "curve": curves[index - 1] if index else [],
                }
            )
        spell_block = (
            blocks[2] if re.search(r"^\s*useFirstSpellList", block, re.MULTILINE) else block
        )
        expressions = _arguments(spell_block, "spellList")
        tokens = _tokens(expressions[0]) if expressions else []
        spells = []
        for level, expression in zip(tokens[::2], tokens[1::2], strict=True):
            name, *rank = expression.split("|")
            packed = compiler.number("SPELL_" + name) | (
                compiler.number("SPELL_" + rank[0]) if rank else 0
            )
            spells.append(
                {"level": compiler.number(level), "packed": packed, "spell": name.lower()}
            )
        result.append(
            {
                "actor": "ally-" + str(selected["member"]),
                "classId": compiler.number("CLASS_" + selected["class"]),
                "stats": stats,
                "spells": spells,
            }
        )
    return result


def prepare(
    canonical_path: Path,
    upstream: Path,
    selection_path: Path,
    output: Path,
    rom_path: Path | None = None,
    presentation_root: Path | None = None,
    audio_candidates: tuple[Path, ...] = (),
    encounter_inputs: Path | None = None,
) -> dict[str, int]:
    output = output.resolve()
    repository = Path(__file__).resolve().parents[2]
    if (
        not output.is_relative_to(repository / "local")
        or output in (canonical_path.resolve(), selection_path.resolve())
        or output.is_relative_to(upstream.resolve())
    ):
        raise ValueError("output must be ignored scratch distinct from read-only inputs")
    canonical = load_json(canonical_path)
    manifest = load_json(MANIFEST)
    if hashlib.sha256(_canonical_bytes(canonical)).hexdigest().upper() != manifest["outputSha256"]:
        raise ValueError("registered canonical input identity mismatch")
    expected_commit = canonical["upstream"]["commit"]
    completed = subprocess.run(
        ["git", "rev-parse", "HEAD"], cwd=upstream, capture_output=True, text=True, check=True
    )
    if completed.stdout.strip() != expected_commit:
        raise ValueError("pinned source commit mismatch")
    selection = load_json(selection_path)
    compiler = OriginalPrograms(
        canonical, upstream, selection["controlledEntitySpeed"], selection["maps"]
    )
    for selected in selection.get("eventMaps", []):
        folder = upstream / f"disasm/data/maps/entries/map{selected:02d}/mapsetups"
        for path in sorted(folder.glob("*.asm")):
            if path.name.startswith(
                ("s2_entityevents", "s3_zoneevents", "s6_initfunction", "scripts")
            ):
                compiler.register_file(path.relative_to(upstream).as_posix())
    for source in selection["additionalProgramSources"]:
        compiler.raw[source["symbol"]] = {
            "id": source["symbol"],
            "path": source["path"],
            "operations": compiler.source_operations("disasm/" + source["path"], source["symbol"]),
        }
    for symbol in selection["programs"]:
        compiler.compile(symbol)
    battle_routes = compile_routes(compiler, selection, upstream)
    maps = compile_maps(compiler, canonical, selection, battle_routes)
    resources = {
        key: {row["id"]: row for row in rows} for key, rows in canonical["resources"].items()
    }
    for map in maps:
        selected = int(map["id"][4:])
        requested = selection.get("setups", {}).get(str(selected), [])
        if not requested:
            continue
        route = next(row for row in canonical["maps"] if row["id"] == selected)["references"][
            "setupRoute"
        ]
        variants = {
            row["flag"]: row["setup"] for row in resources["setupRoutes"][route]["flagVariants"]
        }
        map["setups"] = []
        for flag in requested:
            setup = variants[flag]
            prepared = compile_maps(
                compiler, canonical, selection, battle_routes, {selected: setup}, [selected]
            )[0]
            map["setups"].append(
                {
                    "id": setup.lower().replace("_", "-"),
                    **{
                        key: prepared[key]
                        for key in ("entities", "events", "onLoad", "input", "population")
                    },
                }
            )
    texts_path = "disasm/data/scripting/text/gamescript.txt"
    names_path = "disasm/data/stats/allies/allynames.asm"
    compiler.sources.add(names_path)
    member_names = re.findall(
        r'allyName\s+"([^"]+)"', (upstream / names_path).read_text(encoding="utf-8")
    )
    if len(member_names) != compiler.equates["COMBATANT_ALLIES_NUMBER"]:
        raise ValueError("source ally name count")
    compiler.sources.add(texts_path)
    # Private text retains source tags; the adapter does not claim original tag/audio timing.
    texts = [
        {"id": int(line[:4], 16), "text": line[5:]}
        for line in (upstream / texts_path).read_text(encoding="utf-8").splitlines()
        if re.match(r"^[0-9A-Fa-f]{4}=.+", line)
    ]
    visuals = None
    if (rom_path is None) != (presentation_root is None):
        raise ValueError("visual preparation requires both registered ROM and presentation root")
    if rom_path is not None:
        visuals = prepare_visuals(
            compiler,
            canonical,
            maps,
            rom_path,
            presentation_root,
            selection["presentationManifestSha256"],
            selection["presentationAtlases"],
            audio_candidates,
            tuple(selection["battles"]),
            tuple(selection["audioCues"]) if "audioCues" in selection else None,
        )
    if audio_candidates and visuals is None:
        raise ValueError("audio candidates require private presentation preparation")
    if visuals is not None:
        # Source playSound names identify commands, not a recording's timer suffix.
        # Resolve only a unique command candidate; runtime still checks live timer policy.
        cues = {asset["cue"] for asset in visuals["audio"]}
        for program in compiler.programs.values():
            for instruction in program["instructions"]:
                resource = instruction.get("resource")
                if (
                    instruction["op"] != "present"
                    or instruction["kind"] != "Sound"
                    or resource in cues
                ):
                    continue
                if resource not in compiler.equates:
                    continue
                command = compiler.number(resource)
                candidates = [asset for asset in visuals["audio"] if asset["command"] == command]
                if len(candidates) == 1:
                    instruction["resource"] = candidates[0]["cue"]
    encounters = None
    if encounter_inputs is not None:
        encounters = load_json(encounter_inputs)
        keys = {
            "placement",
            "scene",
            "terrain",
            "static",
            "enemies",
            "gold",
            "start",
            "battleScenes",
        }
        if not isinstance(encounters, list) or len(encounters) > 1:
            raise ValueError("at most one additional encounter input selection is supported")
        for entry in encounters:
            if not isinstance(entry, dict) or set(entry) != keys:
                raise ValueError("encounter input selection fields")
            for key, value in entry.items():
                if key == "battleScenes" and value is None:
                    continue
                relative = Path(value)
                if relative.is_absolute() or not (output.parent / relative).resolve(
                    strict=True
                ).is_relative_to(output.parent):
                    raise ValueError(
                        "encounter inputs must remain relative to the output directory"
                    )
    growth = _selected_growth(compiler, selection.get("growthClasses", []))
    font_path = "disasm/data/graphics/tech/fonts/variablewidthfont.bin"
    ascii_path = "disasm/data/scripting/text/asciitotextsymbolmap.asm"
    compiler.sources.update(
        (
            ascii_path,
            "disasm/data/graphics/tech/fonts/variablewidthfont.txt",
            "disasm/code/common/tech/incbins/s06_incbins_graphics.asm",
            "disasm/code/common/tech/pointers/s06_pointers.asm",
            "disasm/code/common/maps/camerafunctions.asm",
            "disasm/code/common/maps/animations.asm",
            "disasm/code/common/scripting/text/textfunctions_1.asm",
            "disasm/code/common/scripting/text/textfunctions_2.asm",
        )
    )
    # The split binary is private, not an upstream Git blob. Reuse the existing
    # source/ROM reader (including pointer and ASCII parity); emit only advances.
    text_font, font_digest = prepare_font(
        rom_path or private_input_path(ROM_INPUT_IDENTITY), upstream
    )
    sources = [{"path": font_path, "sha256": font_digest}]
    for relative in sorted(compiler.sources):
        raw = (upstream / relative).read_bytes()
        committed = subprocess.run(
            ["git", "show", f"{expected_commit}:{relative}"],
            cwd=upstream,
            capture_output=True,
            check=True,
        ).stdout
        if raw.replace(b"\r\n", b"\n") != committed.replace(b"\r\n", b"\n"):
            raise ValueError("selected pinned source has local changes: " + relative)
        sources.append({"path": relative, "sha256": hashlib.sha256(raw).hexdigest().upper()})
    result = {
        "formatVersion": 1,
        "profile": "private-local-controlled-start",
        "package": selection["package"],
        "provenance": {
            **canonical["upstream"],
            "romSha256": canonical["romSha256"],
            "sources": sources,
            "controlledBoundary": selection["controlledBoundary"],
        },
        "world": {
            "maps": maps,
            "programs": list(compiler.programs.values()),
            "texts": texts,
            "memberNames": member_names,
            "textFont": text_font,
            **({"growth": growth} if growth else {}),
            **({"presentation": visuals} if visuals else {}),
            **(
                {
                    "partyFlags": {
                        "memberCount": compiler.equates["COMBATANT_ALLIES_NUMBER"],
                        "joinedStart": compiler.equates["FORCEMEMBER_JOINED_FLAGS_START"],
                        "activeStart": compiler.equates["FORCEMEMBER_ACTIVE_FLAGS_START"],
                        "capacity": compiler.equates["FORCE_MAX_SIZE"],
                    }
                }
                if selection.get("eventMaps")
                else {}
            ),
        },
    }
    if encounters is not None:
        result["encounters"] = encounters
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return {"maps": len(maps), "programs": len(compiler.programs), "sources": len(sources)}


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--canonical", required=True, type=Path)
    parser.add_argument("--upstream", required=True, type=Path)
    parser.add_argument("--selection", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--rom-path", type=Path)
    parser.add_argument("--presentation-root", type=Path)
    parser.add_argument(
        "--audio-candidate",
        type=Path,
        action="append",
        default=[],
        help="Reviewed remake_audio candidate.json; repeat for selected additions",
    )
    parser.add_argument(
        "--encounter-inputs",
        type=Path,
        help="Existing encounter input rows; paths resolve from the output world directory",
    )
    args = parser.parse_args()
    destination = args.output.resolve()
    repo = Path(__file__).resolve().parents[2]
    if not destination.is_relative_to(repo / "local"):
        raise SystemExit(
            "private content output must stay in this worktree's ignored local directory"
        )
    try:
        print(
            json.dumps(
                prepare(
                    args.canonical,
                    args.upstream,
                    args.selection,
                    destination,
                    args.rom_path,
                    args.presentation_root,
                    tuple(args.audio_candidate),
                    args.encounter_inputs,
                )
            )
        )
    except (ValueError, KeyError, OSError, subprocess.CalledProcessError) as error:
        # Do not expose local private input paths through an exception traceback.
        raise SystemExit(
            "private exploration preparation failed: " + type(error).__name__
        ) from None


if __name__ == "__main__":
    main()
