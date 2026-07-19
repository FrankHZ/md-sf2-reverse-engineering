from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-compression-consumers-static-v1"
MANIFEST = repo_path("manifests/extractions/compression-consumers-static.json")
SCHEMA = repo_path("schemas/compression-consumers-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/compression-consumers-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-compression-consumers-static-fixture.schema.json")

TARGETS = (
    "LoadStackCompressedData",
    "LoadBasicCompressedData",
    "ApplyImmediateVramDmaOnCompressedTiles",
)
CALL_RE = re.compile(
    r"^\s*(?:jsr|jmp|bsr\.w)\s+\(?(LoadStackCompressedData|LoadBasicCompressedData|"
    r"ApplyImmediateVramDmaOnCompressedTiles)\)?(?:\.w)?",
    re.MULTILINE,
)
LABEL_RE = re.compile(r"^([A-Za-z_][A-Za-z0-9_]*):")

OWNER_FIXTURES = {
    "battle-backgrounds": "tests/fixtures/h2/battle-background-decode-v1.json",
    "battle-effects": "tests/fixtures/h2/battle-effect-graphics-decode-v1.json",
    "battle-sprites": "tests/fixtures/h2/battle-sprite-decode-v1.json",
    "battle-terrain": "tests/fixtures/h2/battle-terrain-decode-v1.json",
    "battle-weapon-ground": "tests/fixtures/h2/battle-weapon-ground-decode-v1.json",
    "compression-wrapper": "tests/fixtures/h2/tech-interrupts-static-v1.json",
    "map-sprites": "tests/fixtures/h2/map-sprite-decode-v1.json",
    "map-tilesets": "tests/fixtures/h2/map-tileset-decode-v1.json",
    "portraits": "tests/fixtures/h2/portrait-graphics-decode-v1.json",
    "special-screens": "tests/fixtures/h2/special-screen-graphics-decode-v1.json",
    "special-sprites": "tests/fixtures/h2/special-sprite-decode-v1.json",
    "ui-graphics": "tests/fixtures/h2/ui-graphics-decode-v1.json",
}
EXPECTED_OWNER_COUNTS = {
    "battle-backgrounds": 2,
    "battle-effects": 5,
    "battle-sprites": 7,
    "battle-terrain": 1,
    "battle-weapon-ground": 3,
    "compression-wrapper": 1,
    "map-sprites": 4,
    "map-tilesets": 6,
    "portraits": 1,
    "special-screens": 9,
    "special-sprites": 3,
    "ui-graphics": 4,
}


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _owner(path: str, label: str) -> str:
    if path.startswith("specialscreens/"):
        return "special-screens"
    if path in {
        "gameflow/start/basetiles.asm",
        "common/scripting/endcredits.asm",
        "common/menus/diamondmenu.asm",
        "common/menus/yesnoprompt.asm",
    }:
        return "ui-graphics"
    if path in {
        "gameflow/battle/battlefunctions/battlefunctions_0.asm",
        "common/scripting/entity/entityscriptengine_2.asm",
        "common/scripting/map/mapscriptengine_1.asm",
    }:
        return "map-sprites"
    if path == "gameflow/battle/battleloop/loadbattleterraindata.asm":
        return "battle-terrain"
    if path == "common/maps/mapload.asm":
        return "map-tilesets"
    if path == "common/menus/portraitfunctions.asm":
        return "portraits"
    if path == "common/tech/graphics/specialsprites.asm":
        return "special-sprites"
    if path == "common/tech/interrupts/vintengine_3.asm":
        return "compression-wrapper"
    if path in {
        "gameflow/battle/battlescenes/battlesceneengine_0.asm",
        "gameflow/battle/battlescenes/initializebattlescene.asm",
    }:
        return "battle-effects"
    if path == "gameflow/battle/battlescenes/battlesceneengine_1.asm":
        if label in {"LoadBattlesceneBackground"}:
            return "battle-backgrounds"
        if label in {
            "LoadWeaponsprite",
            "LoadBattlesceneGroundToVram",
            "LoadBattlesceneGround",
        }:
            return "battle-weapon-ground"
        if label in {"LoadInvocationSpriteFrameToVram", "LoadSpellTilesetForInvocation"}:
            return "battle-effects"
        return "battle-sprites"
    raise ValueError(f"unowned compression consumer: {path}:{label}")


def build_compression_consumer_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    code_root = disasm / "code"
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"compression-consumer H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rows = []
    for path in sorted(code_root.rglob("*.asm")):
        source = read_upstream_text(path)
        relative = path.relative_to(code_root).as_posix()
        label = ""
        for line_number, line in enumerate(source.splitlines(), 1):
            label_match = LABEL_RE.match(line)
            if label_match and not label_match.group(1).startswith("loc_"):
                label = label_match.group(1)
            call_match = CALL_RE.match(line)
            if call_match:
                owner = _owner(relative, label)
                rows.append(
                    {
                        "sourcePath": relative,
                        "line": line_number,
                        "contextLabel": label,
                        "target": call_match.group(1),
                        "owner": owner,
                        "ownerFixture": OWNER_FIXTURES[owner],
                    }
                )
    target_counts = Counter(row["target"] for row in rows)
    owner_counts = Counter(row["owner"] for row in rows)
    if dict(sorted(owner_counts.items())) != EXPECTED_OWNER_COUNTS:
        raise ValueError(f"compression-consumer owner coverage drift: {owner_counts}")
    for fixture in OWNER_FIXTURES.values():
        if not repo_path(fixture).is_file():
            raise ValueError(f"compression-consumer owner fixture is missing: {fixture}")
    summary = {
        "callSiteCount": len(rows),
        "sourceFileCount": len({row["sourcePath"] for row in rows}),
        "targetCount": len(target_counts),
        "ownerCount": len(owner_counts),
        "ownedCallSiteCount": sum(owner_counts.values()),
        "unownedCallSiteCount": 0,
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "function": {
            "loadBasicAddress": addresses["LoadBasicCompressedData"],
            "loadStackAddress": addresses["LoadStackCompressedData"],
            "applyImmediateCompressedAddress": addresses[
                "ApplyImmediateVramDmaOnCompressedTiles"
            ],
        },
        "summary": summary,
        "targetCounts": {target: target_counts[target] for target in TARGETS},
        "ownerCounts": dict(sorted(owner_counts.items())),
        "ownerFixtures": OWNER_FIXTURES,
        "callSites": rows,
        "runtimeQuestions": [
            "Dynamic indirect decoder entry and self-modifying call targets are outside this "
            "direct named-call denominator and require runtime trace evidence if suspected."
        ],
    }


def verify_compression_consumer_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_compression_consumer_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="compression-consumer inventory")
    if fixture["upstreamCommit"] != output["upstream"]["commit"]:
        raise ValueError("compression-consumer provenance drift")
    for field in ("function", "summary", "targetCounts", "ownerCounts", "runtimeQuestions"):
        if fixture[field] != output[field]:
            raise ValueError(f"compression-consumer {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("compression-consumer canonical output drift")
    destination = output_path or repo_path("local/derived/compression-consumers-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "CallSites": output["summary"]["callSiteCount"],
        "SourceFiles": output["summary"]["sourceFileCount"],
        "Owners": output["summary"]["ownerCount"],
        "Status": "PASS",
    }
