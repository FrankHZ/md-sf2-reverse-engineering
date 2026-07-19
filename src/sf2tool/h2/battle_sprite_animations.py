from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-battle-sprite-animation-static-v1"
MANIFEST = repo_path("manifests/extractions/battle-sprite-animation-static.json")
SCHEMA = repo_path("schemas/battle-sprite-animation-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-sprite-animation-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-sprite-animation-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

ALLY_COUNT = 87
ENEMY_COUNT = 121
POINTER_SOURCE = Path("code/common/tech/pointers/s12_pointers.asm")
ALLY_SOURCE = Path(
    "data/graphics/battles/battlesprites/allies/animations/entries.asm"
)
ENEMY_SOURCE = Path(
    "data/graphics/battles/battlesprites/enemies/animations/entries.asm"
)
ALLY_SELECTOR_SOURCE = Path("code/gameflow/battle/battlescenes/getallyanimation.asm")
ENGINE_SOURCE = Path("code/gameflow/battle/battlescenes/battlesceneengine_0.asm")
ENEMY_SELECTOR_SOURCE = Path(
    "code/gameflow/battle/battlescenes/battlesceneengine_1.asm"
)


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _signed_byte(value: int) -> int:
    return value - 256 if value >= 128 else value


def _equ(source: str, name: str) -> int:
    match = re.search(rf"^{re.escape(name)}:\s*equ\s+(\$?[0-9A-F]+)", source, re.MULTILINE)
    if match is None:
        raise ValueError(f"battle-sprite animation enum missing: {name}")
    token = match.group(1)
    return int(token[1:], 16) if token.startswith("$") else int(token)


def _require_fragments(source: str, owner: str, fragments: tuple[str, ...]) -> None:
    for fragment in fragments:
        if fragment not in source:
            raise ValueError(f"{owner} consumer drift: missing {fragment!r}")


def _parse_side(
    *,
    side: str,
    count: int,
    source_path: Path,
    disasm: Path,
    addresses: dict[str, int],
    rom: bytes,
) -> tuple[dict[str, Any], dict[str, int]]:
    title = "Ally" if side == "ally" else "Enemy"
    symbol_prefix = f"{title}Animation"
    file_prefix = f"{side}animation"
    table_symbol = f"pt_{title}Animations"
    folder = (
        f"data/graphics/battles/battlesprites/{'allies' if side == 'ally' else 'enemies'}"
        "/animations"
    )
    source = read_upstream_text(disasm / source_path)
    pointer_symbols = re.findall(rf"^\s*dc\.l\s+({symbol_prefix}\d{{3}})", source, re.MULTILINE)
    expected_symbols = [f"{symbol_prefix}{index:03}" for index in range(count)]
    if pointer_symbols != expected_symbols:
        raise ValueError(f"{side} animation pointer-table source drift")
    incbins = re.findall(
        rf'^({symbol_prefix}\d{{3}}):\s*incbin\s+"([^"]+)"', source, re.MULTILINE
    )
    expected_incbins = [
        (symbol, f"{folder}/{file_prefix}{index:03}.bin")
        for index, symbol in enumerate(expected_symbols)
    ]
    if incbins != expected_incbins:
        raise ValueError(f"{side} animation payload source drift")

    table_address = addresses[table_symbol]
    table_bytes = b"".join(addresses[symbol].to_bytes(4, "big") for symbol in expected_symbols)
    if rom[table_address : table_address + len(table_bytes)] != table_bytes:
        raise ValueError(f"{side} animation pointer-table ROM parity drift")

    rows = []
    all_frames = []
    for index, (symbol, path) in enumerate(incbins):
        data = (disasm / path).read_bytes()
        frame_count = data[0]
        header_size = 8 if side == "ally" else 4
        frame_size = 8 if side == "ally" else 4
        if len(data) != header_size + frame_count * frame_size:
            raise ValueError(f"{side} animation length formula drift: {symbol}")
        address = addresses[symbol]
        if rom[address : address + len(data)] != data:
            raise ValueError(f"{side} animation payload ROM parity drift: {symbol}")

        frames = []
        for frame_index in range(frame_count):
            offset = header_size + frame_index * frame_size
            raw = data[offset : offset + frame_size]
            if raw[0] > 15 or raw[1] == 0:
                raise ValueError(f"{side} animation frame field drift: {symbol}")
            weapon = None
            if side == "ally":
                if raw[5] not in (1, 2):
                    raise ValueError(f"ally weapon layer drift: {symbol}")
                weapon = {
                    "frame": raw[4],
                    "zIndex": raw[5],
                    "xOffset": _signed_byte(raw[6]),
                    "yOffset": _signed_byte(raw[7]),
                }
            frame = {
                "frameIndex": frame_index,
                "battleSpriteFrame": raw[0],
                "holdPreviousFrame": raw[0] == 15,
                "displayFrames": raw[1],
                "xOffset": _signed_byte(raw[2]),
                "yOffset": _signed_byte(raw[3]),
                "weapon": weapon,
            }
            frames.append(frame)
            all_frames.append(frame)

        idle_weapon = None
        if side == "ally":
            if data[5] not in (1, 2):
                raise ValueError(f"ally idle weapon layer drift: {symbol}")
            idle_weapon = {
                "frame": data[4],
                "zIndex": data[5],
                "xOffset": _signed_byte(data[6]),
                "yOffset": _signed_byte(data[7]),
            }
        rows.append(
            {
                "index": index,
                "symbol": symbol,
                "sourcePath": path,
                "sourceAddress": address,
                "byteCount": len(data),
                "frameCount": frame_count,
                "spellTriggerFrame": data[1],
                "spellAnimation": None if data[2] == 0xFF else data[2],
                "terminateSpellAnimation": data[3] != 0,
                "idleWeapon": idle_weapon,
                "playedAttackFrameCount": frame_count - 1 if side == "ally" else frame_count,
                "sha256": hashlib.sha256(data).hexdigest().upper(),
                "frames": frames,
            }
        )

    summary = {
        "animationCount": len(rows),
        "pointerTableByteCount": len(table_bytes),
        "payloadByteCount": sum(row["byteCount"] for row in rows),
        "frameEntryCount": len(all_frames),
        "playedAttackFrameCount": sum(row["playedAttackFrameCount"] for row in rows),
        "minimumFrameCount": min(row["frameCount"] for row in rows),
        "maximumFrameCount": max(row["frameCount"] for row in rows),
        "holdPreviousFrameCount": sum(frame["holdPreviousFrame"] for frame in all_frames),
        "defaultSpellAnimationCount": sum(row["spellAnimation"] is not None for row in rows),
        "nonzeroTerminateCount": sum(row["terminateSpellAnimation"] for row in rows),
        "triggerEqualsFrameCount": sum(
            row["spellTriggerFrame"] == row["frameCount"] for row in rows
        ),
        "triggerBeforeFrameCount": sum(
            row["spellTriggerFrame"] < row["frameCount"] for row in rows
        ),
        "triggerAfterFrameCount": sum(
            row["spellTriggerFrame"] > row["frameCount"] for row in rows
        ),
        "pointerRomParityCount": len(rows),
        "payloadRomParityCount": len(rows),
    }
    return (
        {
            "side": side,
            "tableSymbol": table_symbol,
            "tableAddress": table_address,
            "summary": summary,
            "animations": rows,
        },
        summary,
    )


def build_battle_sprite_animation_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"battle-sprite animation H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("battle-sprite animation input ROM identity drift")

    enums = read_upstream_text(disasm / "sf2enums.asm")
    constants = {
        name: _equ(enums, name)
        for name in (
            "BATTLEANIMATION_DODGE",
            "ALLYBATTLEANIMATION_DODGES_START",
            "ALLYBATTLEANIMATION_SPECIALS_START",
            "ENEMYBATTLEANIMATION_DODGES_START",
            "ENEMYBATTLEANIMATION_SPECIALS_START",
        )
    }
    if constants != {
        "BATTLEANIMATION_DODGE": 1,
        "ALLYBATTLEANIMATION_DODGES_START": 40,
        "ALLYBATTLEANIMATION_SPECIALS_START": 80,
        "ENEMYBATTLEANIMATION_DODGES_START": 60,
        "ENEMYBATTLEANIMATION_SPECIALS_START": 118,
    }:
        raise ValueError("battle-sprite animation selector constant drift")

    pointer_source = read_upstream_text(disasm / POINTER_SOURCE)
    for pointer, target in (
        ("p_pt_AllyAnimations", "pt_AllyAnimations"),
        ("p_pt_EnemyAnimations", "pt_EnemyAnimations"),
    ):
        if f"{pointer}:" not in pointer_source or f"dc.l {target}" not in pointer_source:
            raise ValueError(f"battle-sprite animation top-level pointer source drift: {pointer}")
        address = addresses[pointer]
        target_address = addresses[target]
        if rom[address : address + 4] != target_address.to_bytes(4, "big"):
            raise ValueError(f"battle-sprite animation top-level pointer ROM drift: {pointer}")

    ally_selector = read_upstream_text(disasm / ALLY_SELECTOR_SOURCE)
    _require_fragments(
        ally_selector,
        "ally animation selector",
        (
            "cmpi.w  #ALLYBATTLEANIMATION_SPECIALS_START,d1",
            "cmpi.w  #BATTLEANIMATION_DODGE,d1",
            "moveq   #ALLYBATTLEANIMATION_DODGES_START,d1",
            "add.w   ((BATTLESCENE_ALLYBATTLEANIMATION-$1000000)).w,d1",
            "movea.l (p_pt_AllyAnimations).l,a0",
            "movea.l (a0,d1.w),a0",
        ),
    )
    enemy_selector = read_upstream_text(disasm / ENEMY_SELECTOR_SOURCE)
    _require_fragments(
        enemy_selector,
        "enemy animation selector",
        (
            "cmpi.w  #ENEMYBATTLEANIMATION_SPECIALS_START,d1",
            "cmpi.w  #BATTLEANIMATION_DODGE,d1",
            "moveq   #ENEMYBATTLEANIMATION_DODGES_START,d1",
            "add.w   ((BATTLESCENE_ENEMYBATTLEANIMATION-$1000000)).w,d1",
            "movea.l (p_pt_EnemyAnimations).l,a0",
            "movea.l (a0,d1.w),a0",
        ),
    )
    engine = read_upstream_text(disasm / ENGINE_SOURCE)
    _require_fragments(
        engine,
        "battle animation reader",
        (
            "subq.w  #1,d7",
            "subq.w  #2,d7",
            "lea     $C(a0),a0",
            "move.l  (a0)+,((WEAPON_FRAME_INDEX-$1000000)).w",
        ),
    )

    ally, ally_summary = _parse_side(
        side="ally",
        count=ALLY_COUNT,
        source_path=ALLY_SOURCE,
        disasm=disasm,
        addresses=addresses,
        rom=rom,
    )
    enemy, enemy_summary = _parse_side(
        side="enemy",
        count=ENEMY_COUNT,
        source_path=ENEMY_SOURCE,
        disasm=disasm,
        addresses=addresses,
        rom=rom,
    )
    combined = {
        "animationCount": ally_summary["animationCount"] + enemy_summary["animationCount"],
        "pointerTableByteCount": ally_summary["pointerTableByteCount"]
        + enemy_summary["pointerTableByteCount"],
        "payloadByteCount": ally_summary["payloadByteCount"]
        + enemy_summary["payloadByteCount"],
        "frameEntryCount": ally_summary["frameEntryCount"]
        + enemy_summary["frameEntryCount"],
        "playedAttackFrameCount": ally_summary["playedAttackFrameCount"]
        + enemy_summary["playedAttackFrameCount"],
        "holdPreviousFrameCount": ally_summary["holdPreviousFrameCount"]
        + enemy_summary["holdPreviousFrameCount"],
        "defaultSpellAnimationCount": ally_summary["defaultSpellAnimationCount"]
        + enemy_summary["defaultSpellAnimationCount"],
        "nonzeroTerminateCount": ally_summary["nonzeroTerminateCount"]
        + enemy_summary["nonzeroTerminateCount"],
        "pointerRomParityCount": ally_summary["pointerRomParityCount"]
        + enemy_summary["pointerRomParityCount"],
        "payloadRomParityCount": ally_summary["payloadRomParityCount"]
        + enemy_summary["payloadRomParityCount"],
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_hash,
        "function": {
            "GetAllyAnimation": addresses["GetAllyAnimation"],
            "GetEnemyAnimation": addresses["GetEnemyAnimation"],
        },
        "table": {
            "p_pt_AllyAnimations": addresses["p_pt_AllyAnimations"],
            "p_pt_EnemyAnimations": addresses["p_pt_EnemyAnimations"],
            "pt_AllyAnimations": addresses["pt_AllyAnimations"],
            "pt_EnemyAnimations": addresses["pt_EnemyAnimations"],
        },
        "constants": constants,
        "summary": {"combined": combined, "ally": ally_summary, "enemy": enemy_summary},
        "selectorRules": [
            "non-special attacks use the combatant base animation index",
            "dodge adds the side-specific dodge-table start to the base animation index",
            "indices at or above the side-specific specials start are direct table indices",
            "ally regular spear attacks remap KNTE, PLDN, and PGNT to direct indices 80-82",
            "ally attack playback skips frame entry zero because it doubles as idle frame two",
            "enemy attack playback consumes every frame entry",
        ],
        "sides": [ally, enemy],
        "runtimeQuestions": [
            "Which base animation indices are reachable for every ally/enemy battlesprite and "
            "weapon combination?",
            "Do display-frame timing, hold-frame value 15, spell triggers, weapon flips/layers/"
            "offsets, and rendered frames reproduce the original presentation?",
        ],
    }


def verify_battle_sprite_animation_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_sprite_animation_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="battle-sprite animation contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("battle-sprite animation provenance drift")
    for field in (
        "function",
        "table",
        "constants",
        "summary",
        "selectorRules",
        "runtimeQuestions",
    ):
        if fixture[field] != output[field]:
            raise ValueError(f"battle-sprite animation {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("battle-sprite animation canonical output drift")
    destination = output_path or repo_path(
        "local/derived/battle-sprite-animation-static.json"
    )
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Animations": output["summary"]["combined"]["animationCount"],
        "FrameEntries": output["summary"]["combined"]["frameEntryCount"],
        "Status": "PASS",
    }
