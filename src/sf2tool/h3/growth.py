from __future__ import annotations

import re
import subprocess
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

STAT_FIXTURE = repo_path("tests/fixtures/h3/stat-gain-v1.json")
STAT_SCHEMA = repo_path("schemas/h3-stat-gain-fixture.schema.json")
LEVEL_FIXTURE = repo_path("tests/fixtures/h3/level-up-v1.json")
LEVEL_SCHEMA = repo_path("schemas/h3-level-up-fixture.schema.json")
BOUNDARY_FIXTURE = repo_path("tests/fixtures/h3/level-up-boundaries-v1.json")
BOUNDARY_SCHEMA = repo_path("schemas/h3-level-up-boundaries-fixture.schema.json")
REFRESH_FIXTURE = repo_path("tests/fixtures/h3/level-up-refresh-v1.json")
REFRESH_SCHEMA = repo_path("schemas/h3-level-up-refresh-fixture.schema.json")
PROWESS_FIXTURE = repo_path("tests/fixtures/h3/ally-initialization-prowess-v1.json")
PROWESS_SCHEMA = repo_path("schemas/h3-ally-initialization-prowess-fixture.schema.json")
STAT_OBSERVER = repo_path("tools/bizhawk/stat_gain_observer.lua")
LEVEL_OBSERVER = repo_path("tools/bizhawk/level_up_observer.lua")
BOUNDARY_OBSERVER = repo_path("tools/bizhawk/level_up_boundaries_observer.lua")
REFRESH_OBSERVER = repo_path("tools/bizhawk/level_up_refresh_observer.lua")
PROWESS_OBSERVER = repo_path("tools/bizhawk/ally_initialization_prowess_observer.lua")
TOOLCHAIN_MANIFEST = repo_path("manifests/toolchain.json")

CURVE_NAMES = ("LINEAR", "LATE", "EARLY", "MIDDLE", "EARLYANDLATE")
STAT_MACROS = {
    "hp": "hpGrowth",
    "mp": "mpGrowth",
    "attack": "attGrowth",
    "defense": "defGrowth",
    "agility": "agiGrowth",
}


def _verify_upstream(upstream_path: Path) -> Path:
    upstream_path = upstream_path.resolve(strict=True)
    expected = load_json(TOOLCHAIN_MANIFEST)["sf2disasm"]["commit"]
    completed = subprocess.run(
        ["git", "rev-parse", "HEAD"],
        cwd=upstream_path,
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    )
    actual = completed.stdout.strip()
    if actual != expected:
        raise ValueError(f"growth H3 requires SF2DISASM {expected}, got {actual}")
    return upstream_path / "disasm"


def _parse_growth_curves(disasm: Path) -> dict[int, dict[int, tuple[int, int]]]:
    lines = (disasm / "data/stats/allies/growthcurves.asm").read_text(encoding="utf-8").splitlines()
    curves: dict[int, dict[int, tuple[int, int]]] = {}
    current = 0
    heading = re.compile(r"^\s*;\s*(Linear|Late|Early|Middle|Early and late)\s*$")
    row = re.compile(r"^\s*dc\.w\s+(\d+)\s*,\s*(\d+)\s*;\s*level\s+(\d+)")
    for line in lines:
        if heading.match(line):
            current += 1
            curves[current] = {}
            continue
        match = row.match(line)
        if current and match:
            curves[current][int(match.group(3))] = (int(match.group(1)), int(match.group(2)))
    if len(curves) != 5 or any(len(levels) != 29 for levels in curves.values()):
        raise ValueError("unexpected stat-growth curve shape in pinned source")
    return curves


def _parse_equates(disasm: Path) -> dict[str, int]:
    text = (disasm / "sf2enums.asm").read_text(encoding="utf-8")
    pattern = r"^([A-Z0-9_]+):\s+equ\s+(\$[0-9A-Fa-f]+|\d+)"
    result = {}
    for name, value in re.findall(pattern, text, re.MULTILINE):
        result[name] = int(value[1:], 16) if value.startswith("$") else int(value)
    return result


def _parse_ally_starts(disasm: Path) -> list[tuple[str, int]]:
    text = (disasm / "data/stats/allies/allystartdefs.asm").read_text(encoding="utf-8")
    classes = re.findall(r"^\s*startClass\s+([A-Z0-9_]+)", text, re.MULTILINE)
    levels = [int(value) for value in re.findall(r"^\s*startLevel\s+(\d+)", text, re.MULTILINE)]
    if len(classes) != 32 or len(levels) != 32:
        raise ValueError("unexpected ally-start definition count in pinned source")
    return list(zip(classes, levels, strict=True))


def _parse_ally_codes(disasm: Path) -> list[str]:
    text = (disasm / "data/stats/allies/allynames.asm").read_text(encoding="utf-8")
    codes = re.findall(r'(?:^|:)\s*allyName\s+"([A-Z]+)"', text, re.MULTILINE)
    if len(codes) != 30:
        raise ValueError("unexpected ally-name count in pinned source")
    return codes


def _parse_class_prowess(disasm: Path, equates: dict[str, int]) -> list[int]:
    text = (disasm / "data/stats/allies/classes/classdefs.asm").read_text(encoding="utf-8")
    expressions = re.findall(r"^\s*prowess\s+([A-Z0-9_|]+)", text, re.MULTILINE)
    if len(expressions) != 32:
        raise ValueError("unexpected class-prowess definition count in pinned source")
    values = []
    for expression in expressions:
        value = 0
        for token in expression.split("|"):
            value |= equates[f"PROWESS_{token}"]
        values.append(value)
    return values


def _parse_class_bases(disasm: Path, equates: dict[str, int]) -> list[dict[str, int]]:
    text = (disasm / "data/stats/allies/classes/classdefs.asm").read_text(encoding="utf-8")
    pattern = re.compile(
        r"^\s*mov\s+(\d+).*?^\s*resistance\s+([A-Z0-9_|]+).*?"
        r"^\s*movetype\s+[A-Z0-9_]+.*?^\s*prowess\s+([A-Z0-9_|]+)",
        re.MULTILINE | re.DOTALL,
    )
    bases = []
    for move, resistance, prowess in pattern.findall(text):
        resistance_value = 0
        for token in resistance.split("|"):
            resistance_value |= equates[f"RESISTANCE_{token}"]
        prowess_value = 0
        for token in prowess.split("|"):
            prowess_value |= equates[f"PROWESS_{token}"]
        bases.append(
            {
                "move": int(move),
                "resistance": resistance_value,
                "prowess": prowess_value,
            }
        )
    if len(bases) != 32:
        raise ValueError("unexpected class-base definition count in pinned source")
    return bases


def _parse_item_equip_effects(
    disasm: Path, item: int, equates: dict[str, int]
) -> tuple[list[tuple[str, int]], bool]:
    text = (disasm / "data/stats/items/itemdefs.asm").read_text(encoding="utf-8")
    marker = re.search(rf"^\s*;\s*{item}:\s+.+$", text, re.MULTILINE)
    if marker is None:
        raise ValueError(f"missing item definition {item}")
    following = text[marker.end() :]
    next_item = re.search(r"^\s*;\s*\d+:\s+.+$", following, re.MULTILINE)
    block = following[: next_item.start()] if next_item else following
    effects = re.search(r"^\s*equipEffects\s+(.+)$", block, re.MULTILINE)
    if effects is None:
        raise ValueError(f"missing equip effects for item {item}")
    item_type = re.search(r"^\s*itemType\s+([A-Z0-9_|]+)", block, re.MULTILINE)
    if item_type is None:
        raise ValueError(f"missing item type for item {item}")
    effect_text = block[effects.start() :]
    pairs = re.findall(r"([A-Z_]+)\s*,\s*(\d+)", effect_text)
    if len(pairs) != 3:
        raise ValueError(f"unexpected equip-effect count for item {item}")
    for code, _ in pairs:
        if f"EQUIPEFFECT_{code}" not in equates:
            raise ValueError(f"unknown equip effect {code} for item {item}")
    return (
        [(code, int(value)) for code, value in pairs],
        "CURSED" in item_type.group(1).split("|"),
    )


def _parse_stats_block(disasm: Path, ally: int, class_code: str) -> dict[str, Any]:
    path = disasm / f"data/stats/allies/stats/allystats{ally:02d}.asm"
    text = path.read_text(encoding="utf-8")
    marker = re.search(rf"(?:^|:)\s*forClass\s+{re.escape(class_code)}\s*$", text, re.MULTILINE)
    if marker is None:
        raise ValueError(f"missing {class_code} stats block for ally {ally}")
    def class_block(source_marker: re.Match[str]) -> str:
        following = text[source_marker.end() :]
        next_block = re.search(r"(?:^|:)\s*forClass\s+", following, re.MULTILINE)
        return following[: next_block.start()] if next_block else following

    block = class_block(marker)
    stats: dict[str, dict[str, Any]] = {}
    for stat, macro in STAT_MACROS.items():
        match = re.search(
            rf"^\s*{macro}\s+(\d+)\s*,\s*(\d+)\s*,\s*([A-Z]+)", block, re.MULTILINE
        )
        if match is None:
            raise ValueError(f"missing {macro} in ally {ally} {class_code}")
        curve_code = match.group(3)
        curve = 0 if curve_code == "NONE" else CURVE_NAMES.index(curve_code) + 1
        stats[stat] = {
            "start": int(match.group(1)),
            "projected": int(match.group(2)),
            "curve": curve,
        }
    spell_source = block
    if "useFirstSpellList" in block:
        first_marker = re.search(r"(?:^|:)\s*forClass\s+[A-Z0-9_]+\s*$", text, re.MULTILINE)
        if first_marker is None:
            raise ValueError(f"missing first spell-list block for ally {ally}")
        spell_source = class_block(first_marker)
    spell_block = (
        spell_source[spell_source.find("spellList") :] if "spellList" in spell_source else ""
    )
    spell_pattern = r"(\d+)\s*,\s*([A-Z][A-Z0-9_]*(?:\|LV[1-4])?)"
    spells = [
        {"level": int(level), "expression": expression}
        for level, expression in re.findall(spell_pattern, spell_block)
    ]
    return {
        "stats": stats,
        "spells": spells,
        "inheritsFirstSpellList": "useFirstSpellList" in block,
    }


def _stats_blocks_in_rom_order(disasm: Path) -> list[dict[str, Any]]:
    blocks = []
    for ally in range(30):
        path = disasm / f"data/stats/allies/stats/allystats{ally:02d}.asm"
        text = path.read_text(encoding="utf-8")
        range_match = re.search(r"; 0x([0-9A-F]+)\.\.0x([0-9A-F]+)", text)
        if range_match is None:
            raise ValueError(f"missing ROM range for ally stats {ally}")
        address = int(range_match.group(1), 16)
        end = int(range_match.group(2), 16)
        class_codes = re.findall(r"(?:^|:)\s*forClass\s+([A-Z0-9_]+)\s*$", text, re.MULTILINE)
        for class_code in class_codes:
            block = _parse_stats_block(disasm, ally, class_code)
            blocks.append(
                {
                    "ownerAlly": ally,
                    "classCode": class_code,
                    "address": address,
                    "block": block,
                }
            )
            spell_bytes = 1 if block["inheritsFirstSpellList"] else len(block["spells"]) * 2 + 1
            address += 16 + spell_bytes
        if address != end:
            raise ValueError(f"ally stats encoded-size drift for ally {ally}")
    return blocks


def _resolve_stats_block(disasm: Path, ally: int, class_code: str) -> dict[str, Any] | None:
    return next(
        (
            entry
            for entry in _stats_blocks_in_rom_order(disasm)
            if entry["ownerAlly"] >= ally and entry["classCode"] == class_code
        ),
        None,
    )


def _rng_step(seed: int, range_: int = 128) -> tuple[int, int]:
    updated = (seed * 13 + 7) & 0xFFFF
    return updated, (updated * range_) // 0x10000


def _calculate_gain(
    *,
    current: int,
    start: int,
    projected: int,
    curve: int,
    level: int,
    first: int,
    second: int,
    curves: dict[int, dict[int, tuple[int, int]]],
) -> tuple[int, bool]:
    if level >= 30:
        randomized = (384 + first - second + 128) // 256
        expected_minimum = projected
    else:
        total256, gain256 = curves[curve][level + 1]
        projection = projected - start
        randomized = (projection * gain256 + first - second + 128) // 256
        expected_minimum = (projection * total256 + 128) // 256 + start
    pity = current + randomized < expected_minimum
    return randomized + int(pity), pity


def _verify_stat_models(
    fixture: dict[str, Any], curves: dict[int, dict[int, tuple[int, int]]]
) -> None:
    for case in fixture["cases"]:
        for rng in case["rng"]:
            seed, value = _rng_step(rng["seed"])
            if (seed, value) != (rng["expectedSeed"], rng["expectedValue"]):
                raise ValueError(f"stat-gain RNG golden disagrees with model: {case['id']}")
        values = case["input"]
        if values["curve"] == 0:
            modeled = ("none", False, 0)
        else:
            if len(case["rng"]) != 2:
                raise ValueError(f"growth case requires two RNG inputs: {case['id']}")
            gain, pity = _calculate_gain(
                current=values["current"],
                start=values["start"],
                projected=values["projected"],
                curve=values["curve"] & 7,
                level=values["level"],
                first=case["rng"][0]["expectedValue"],
                second=case["rng"][1]["expectedValue"],
                curves=curves,
            )
            modeled = ("growth", pity, gain)
        expected = (case["expectedPath"], case["expectedPity"], case["expectedGain"])
        if modeled != expected:
            raise ValueError(f"stat-gain golden disagrees with source model: {case['id']}")


def _verify_stat_observation(fixture: dict[str, Any], observed: dict[str, Any]) -> None:
    if observed.get("system") != "GEN" or observed.get("core") != fixture["emulator"]["core"]:
        raise ValueError("unexpected stat-gain execution system/core")
    if len(observed.get("results", [])) != len(fixture["cases"]):
        raise ValueError("stat-gain observation count mismatch")
    for expected, actual in zip(fixture["cases"], observed["results"], strict=True):
        if actual["id"] != expected["id"] or actual["input"] != expected["input"]:
            raise ValueError(f"stat-gain input mismatch: {expected['id']}")
        result = (actual["path"], actual["pity"], actual["gain"])
        golden = (expected["expectedPath"], expected["expectedPity"], expected["expectedGain"])
        if result != golden or len(actual["rng"]) != len(expected["rng"]):
            raise ValueError(f"stat-gain result mismatch: {expected['id']}")
        for rng_expected, rng_actual in zip(expected["rng"], actual["rng"], strict=True):
            if rng_actual != {
                "observedSeed": rng_expected["expectedSeed"],
                "observedValue": rng_expected["expectedValue"],
            }:
                raise ValueError(f"stat-gain RNG mismatch: {expected['id']}")


def _model_level_case(
    case: dict[str, Any],
    *,
    disasm: Path,
    curves: dict[int, dict[int, tuple[int, int]]],
    equates: dict[str, int],
    starts: list[tuple[str, int]],
    ally_codes: list[str],
) -> dict[str, Any]:
    ally = case["ally"]
    class_code, start_level = starts[ally]
    if ally_codes[ally] != case["allyCode"] or class_code != case["classCode"]:
        raise ValueError(f"level-up source identity drift: {case['id']}")
    block = _parse_stats_block(disasm, ally, class_code)
    class_id = equates[f"CLASS_{class_code}"]
    last_nonpromoted = equates["CHAR_CLASS_LASTNONPROMOTED"]
    extra = equates["CHAR_CLASS_EXTRALEVEL"]
    bug_branch = class_id >= last_nonpromoted
    before = {"class": class_id, "level": 1, "exp": 0}
    before.update({stat: values["start"] for stat, values in block["stats"].items()})
    after = dict(before)
    after["level"] = 2
    seed = case["seed"]
    gains: list[int] = []
    for stat in STAT_MACROS:
        values = block["stats"][stat]
        if values["curve"] == 0:
            gain = 0
        else:
            seed, first = _rng_step(seed)
            seed, second = _rng_step(seed)
            gain, _ = _calculate_gain(
                current=before[stat],
                start=values["start"],
                projected=values["projected"],
                curve=values["curve"],
                level=1,
                first=first,
                second=second,
                curves=curves,
            )
        gains.append(gain)
        after[stat] += gain
    effective_level = 2 + (extra if bug_branch else 0)
    if any(spell["level"] == effective_level for spell in block["spells"]):
        raise ValueError(
            f"modeled level-up spell requires an encoded spell assertion: {case['id']}"
        )
    return {
        "initialization": {
            "startLevel": start_level,
            "extraLevelBranch": bug_branch,
            "levelBeforeExtra": start_level if bug_branch else -1,
            "effectiveLevel": start_level + (extra if bug_branch else 0),
        },
        "before": before,
        "after": after,
        "levelUpExtraBranch": bug_branch,
        "levelBeforeExtra": 2 if bug_branch else -1,
        "effectiveLevel": effective_level,
        "expectedSeed": seed,
        "arguments": [2, *gains, 255],
    }


def _verify_level_models(
    fixture: dict[str, Any], *, disasm: Path, curves: dict[int, dict[int, tuple[int, int]]]
) -> None:
    equates = _parse_equates(disasm)
    starts = _parse_ally_starts(disasm)
    ally_codes = _parse_ally_codes(disasm)
    modeled_fields = (
        "initialization",
        "before",
        "after",
        "levelUpExtraBranch",
        "levelBeforeExtra",
        "effectiveLevel",
        "expectedSeed",
        "arguments",
    )
    for case in fixture["cases"]:
        model = _model_level_case(
            case,
            disasm=disasm,
            curves=curves,
            equates=equates,
            starts=starts,
            ally_codes=ally_codes,
        )
        if any(case[field] != model[field] for field in modeled_fields):
            raise ValueError(f"level-up golden disagrees with source model: {case['id']}")


def _verify_level_observation(fixture: dict[str, Any], observed: dict[str, Any]) -> None:
    if observed.get("system") != "GEN" or observed.get("core") != fixture["emulator"]["core"]:
        raise ValueError("unexpected level-up execution system/core")
    if len(observed.get("results", [])) != len(fixture["cases"]):
        raise ValueError("level-up observation count mismatch")
    for expected, actual in zip(fixture["cases"], observed["results"], strict=True):
        normalized = {
            "id": expected["id"],
            "ally": expected["ally"],
            "seed": expected["seed"],
            "initialization": expected["initialization"],
            "before": expected["before"],
            "after": expected["after"],
            "levelUpExtraBranch": expected["levelUpExtraBranch"],
            "levelBeforeExtra": expected["levelBeforeExtra"],
            "effectiveLevel": expected["effectiveLevel"],
            "observedSeed": expected["expectedSeed"],
            "arguments": expected["arguments"],
        }
        if actual != normalized:
            raise ValueError(f"level-up runtime mismatch: {expected['id']}")


def _encode_spell(expression: str, equates: dict[str, int]) -> int:
    tokens = expression.split("|")
    encoded = equates[f"SPELL_{tokens[0]}"]
    if len(tokens) == 2:
        encoded |= equates[f"SPELL_{tokens[1]}"]
    return encoded


def _model_boundary_case(
    case: dict[str, Any],
    *,
    disasm: Path,
    curves: dict[int, dict[int, tuple[int, int]]],
    equates: dict[str, int],
    ally_codes: list[str],
) -> dict[str, Any]:
    ally = case["ally"]
    if ally_codes[ally] != case["allyCode"]:
        raise ValueError(f"level-up boundary ally identity drift: {case['id']}")
    class_id = equates[f"CLASS_{case['classCode']}"]
    values = case["input"]
    if values["class"] != class_id:
        raise ValueError(f"level-up boundary class identity drift: {case['id']}")
    resolved = _resolve_stats_block(disasm, ally, case["classCode"])
    if resolved is None:
        if case["inputBasis"] != "custom":
            raise ValueError(f"level-up boundary unexpectedly lacks class block: {case['id']}")
        return {
            "after": values,
            "capExit": True,
            "missingClassExit": True,
            "matchedStatsAddress": -1,
            "extraLevelBranch": False,
            "levelBeforeExtra": -1,
            "effectiveLevel": -1,
            "expectedSeed": case["seed"],
            "arguments": [255, 0, 0, 0, 0, 0, 255],
        }
    owner_ally = resolved["ownerAlly"]
    block = resolved["block"]
    if case["inputBasis"] == "custom":
        if owner_ally == ally:
            raise ValueError(
                f"level-up boundary custom input has a local class block: {case['id']}"
            )
    else:
        if owner_ally != ally:
            raise ValueError(f"level-up boundary unexpectedly borrowed class block: {case['id']}")
        for stat, growth in block["stats"].items():
            if values[stat] != growth[case["inputBasis"]]:
                raise ValueError(f"level-up boundary {stat} basis drift: {case['id']}")

    spell_block = block
    if block["inheritsFirstSpellList"] and owner_ally != ally:
        spell_block = next(
            entry["block"]
            for entry in _stats_blocks_in_rom_order(disasm)
            if entry["ownerAlly"] == ally
        )

    promoted = class_id >= equates["CHAR_CLASS_FIRSTPROMOTED"]
    cap = equates["CHAR_LEVELCAP_PROMOTED"] if promoted else equates["CHAR_LEVELCAP_BASE"]
    if values["level"] >= cap:
        return {
            "after": values,
            "capExit": True,
            "missingClassExit": False,
            "matchedStatsAddress": resolved["address"],
            "extraLevelBranch": False,
            "levelBeforeExtra": -1,
            "effectiveLevel": -1,
            "expectedSeed": case["seed"],
            "arguments": [255, 0, 0, 0, 0, 0, 255],
        }

    after = {
        **values,
        "spells": list(values["spells"]),
        "level": values["level"] + 1,
    }
    seed = case["seed"]
    gains: list[int] = []
    for stat in STAT_MACROS:
        growth = block["stats"][stat]
        if growth["curve"] == 0:
            gain = 0
        else:
            seed, first = _rng_step(seed)
            seed, second = _rng_step(seed)
            gain, _ = _calculate_gain(
                current=values[stat],
                start=growth["start"],
                projected=growth["projected"],
                curve=growth["curve"],
                level=values["level"],
                first=first,
                second=second,
                curves=curves,
            )
        gains.append(gain)
        after[stat] += gain

    bug_branch = class_id >= equates["CHAR_CLASS_LASTNONPROMOTED"]
    effective = after["level"] + (
        equates["CHAR_CLASS_EXTRALEVEL"] if bug_branch else 0
    )
    learned = next(
        (
            _encode_spell(spell["expression"], equates)
            for spell in spell_block["spells"]
            if spell["level"] == effective
        ),
        255,
    )
    if learned != 255:
        base_spell = learned & 0x3F
        slot = next(
            (
                index
                for index, spell in enumerate(after["spells"])
                if spell != 255 and spell & 0x3F == base_spell
            ),
            None,
        )
        if slot is None:
            slot = after["spells"].index(255)
        after["spells"][slot] = learned
    return {
        "after": after,
        "capExit": False,
        "missingClassExit": False,
        "matchedStatsAddress": resolved["address"],
        "extraLevelBranch": bug_branch,
        "levelBeforeExtra": after["level"] if bug_branch else -1,
        "effectiveLevel": effective,
        "expectedSeed": seed,
        "arguments": [after["level"], *gains, learned],
    }


def _verify_boundary_models(
    fixture: dict[str, Any], *, disasm: Path, curves: dict[int, dict[int, tuple[int, int]]]
) -> None:
    equates = _parse_equates(disasm)
    ally_codes = _parse_ally_codes(disasm)
    fields = (
        "after",
        "capExit",
        "missingClassExit",
        "matchedStatsAddress",
        "extraLevelBranch",
        "levelBeforeExtra",
        "effectiveLevel",
        "expectedSeed",
        "arguments",
    )
    for case in fixture["cases"]:
        model = _model_boundary_case(
            case,
            disasm=disasm,
            curves=curves,
            equates=equates,
            ally_codes=ally_codes,
        )
        if any(case[field] != model[field] for field in fields):
            raise ValueError(f"level-up boundary golden disagrees with source model: {case['id']}")


def _verify_boundary_observation(fixture: dict[str, Any], observed: dict[str, Any]) -> None:
    if observed.get("system") != "GEN" or observed.get("core") != fixture["emulator"]["core"]:
        raise ValueError("unexpected level-up boundary execution system/core")
    if len(observed.get("results", [])) != len(fixture["cases"]):
        raise ValueError("level-up boundary observation count mismatch")
    actual_by_id = {result["id"]: result for result in observed["results"]}
    if len(actual_by_id) != len(observed["results"]):
        raise ValueError("duplicate level-up boundary observation ID")
    for expected in fixture["cases"]:
        actual = actual_by_id.get(expected["id"])
        normalized = {
            "id": expected["id"],
            "ally": expected["ally"],
            "seed": expected["seed"],
            "before": expected["input"],
            "after": expected["after"],
            "capExit": expected["capExit"],
            "missingClassExit": expected["missingClassExit"],
            "matchedStatsAddress": expected["matchedStatsAddress"],
            "extraLevelBranch": expected["extraLevelBranch"],
            "levelBeforeExtra": expected["levelBeforeExtra"],
            "effectiveLevel": expected["effectiveLevel"],
            "observedSeed": expected["expectedSeed"],
            "arguments": expected["arguments"],
        }
        if actual != normalized:
            raise ValueError(f"level-up boundary runtime mismatch: {expected['id']}")


def _model_refresh_case(
    case: dict[str, Any],
    *,
    disasm: Path,
    curves: dict[int, dict[int, tuple[int, int]]],
) -> dict[str, Any]:
    ally = case["ally"]
    equates = _parse_equates(disasm)
    ally_codes = _parse_ally_codes(disasm)
    if ally_codes[ally] != case["allyCode"]:
        raise ValueError("level-up refresh ally identity drift")
    class_code = case["classCode"]
    class_id = equates[f"CLASS_{class_code}"]
    before = case["input"]
    if before["class"] != class_id:
        raise ValueError("level-up refresh class identity drift")

    block = _parse_stats_block(disasm, ally, class_code)
    source_fields = {
        "hp": "maxHp",
        "mp": "maxMp",
        "attack": "baseAttack",
        "defense": "baseDefense",
        "agility": "baseAgility",
    }
    for stat, field in source_fields.items():
        if before[field] != block["stats"][stat]["projected"]:
            raise ValueError(f"level-up refresh {field} source basis drift")

    class_bases = _parse_class_bases(disasm, equates)[class_id]
    if (
        before["baseMove"],
        before["baseResistance"],
        before["baseProwess"],
    ) != (
        class_bases["move"],
        class_bases["resistance"],
        class_bases["prowess"],
    ):
        raise ValueError("level-up refresh class-base source drift")

    after = {**before, "items": list(before["items"]), "spells": list(before["spells"])}
    after["level"] += 1
    seed = case["seed"]
    for stat, field in source_fields.items():
        growth = block["stats"][stat]
        if growth["curve"] == 0:
            gain = 0
        else:
            seed, first = _rng_step(seed)
            seed, second = _rng_step(seed)
            gain, _ = _calculate_gain(
                current=before[field],
                start=growth["start"],
                projected=growth["projected"],
                curve=growth["curve"],
                level=before["level"],
                first=first,
                second=second,
                curves=curves,
            )
        after[field] += gain

    after["currentAttack"] = after["baseAttack"]
    after["currentDefense"] = after["baseDefense"]
    after["currentAgility"] = after["baseAgility"]
    after["currentMove"] = after["baseMove"]
    after["currentResistance"] = after["baseResistance"]
    after["currentProwess"] = after["baseProwess"]

    status_mask = 0
    for name in (
        "STATUSEFFECT_STUN",
        "STATUSEFFECT_POISON",
        "STATUSEFFECT_MUDDLE2",
        "STATUSEFFECT_MUDDLE",
        "STATUSEFFECT_SLEEP",
        "STATUSEFFECT_SILENCE",
        "STATUSEFFECT_SLOW",
        "STATUSEFFECT_BOOST",
        "STATUSEFFECT_ATTACK",
    ):
        status_mask |= equates[name]
    status = before["status"] & status_mask
    after["status"] = status
    attack_setting = (status & equates["STATUSEFFECT_ATTACK"]) >> 14
    boost_setting = (status & equates["STATUSEFFECT_BOOST"]) >> 12
    slow_setting = (status & equates["STATUSEFFECT_SLOW"]) >> 10
    after["currentAttack"] += after["baseAttack"] * attack_setting >> 3
    after["currentDefense"] += after["baseDefense"] * boost_setting >> 3
    after["currentAgility"] += after["baseAgility"] * boost_setting >> 3
    after["currentDefense"] -= after["baseDefense"] * slow_setting >> 3
    after["currentAgility"] -= after["baseAgility"] * slow_setting >> 3
    if status & equates["STATUSEFFECT_STUN"]:
        after["currentMove"] = max(0, after["currentMove"] - 1)
        after["currentAgility"] = max(0, after["currentAgility"] - 5)

    item_mask = equates["ITEMENTRY_MASK_INDEX"]
    equipped_bit = equates["ITEM_EQUIPPED"]
    for item_entry in before["items"]:
        item = item_entry & item_mask
        if item == equates["ITEM_NOTHING"] or item_entry & equipped_bit == 0:
            continue
        effects, cursed = _parse_item_equip_effects(disasm, item, equates)
        for effect, value in effects:
            if effect == "NONE":
                continue
            field = {
                "INCREASE_ATT": "currentAttack",
                "INCREASE_DEF": "currentDefense",
                "INCREASE_AGI": "currentAgility",
                "INCREASE_MOV": "currentMove",
                "DECREASE_ATT": "currentAttack",
                "DECREASE_DEF": "currentDefense",
                "DECREASE_AGI": "currentAgility",
                "DECREASE_MOV": "currentMove",
            }.get(effect)
            if field is not None:
                delta = -value if effect.startswith("DECREASE_") else value
                cap = {
                    "currentAttack": equates["CHAR_STATCAP_ATT"],
                    "currentDefense": equates["CHAR_STATCAP_DEF"],
                    "currentAgility": equates["CHAR_STATCAP_AGI_CURRENT"],
                    "currentMove": equates["CHAR_STATCAP_MOV"],
                }[field]
                after[field] = max(0, min(cap, after[field] + delta))
                continue

            prowess = after["currentProwess"]
            critical_mask = equates["PROWESS_MASK_CRITICAL"]
            double_mask = equates["PROWESS_MASK_DOUBLE"]
            counter_mask = equates["PROWESS_MASK_COUNTER"]
            lower_mask = equates["PROWESS_MASK_LOWER_DOUBLE_OR_COUNTER"]
            double_shift = equates["PROWESS_LOWER_DOUBLE_SHIFT_COUNT"]
            counter_shift = equates["PROWESS_LOWER_COUNTER_SHIFT_COUNT"]
            if effect == "INCREASE_CRITICAL":
                critical = prowess & critical_mask
                if critical < equates["PROWESS_CRITICAL_NONE"]:
                    critical = min(7, critical + value)
                after["currentProwess"] = prowess & (double_mask | counter_mask) | critical
            elif effect == "INCREASE_DOUBLE":
                double = min(3, ((prowess >> double_shift) & lower_mask) + value)
                after["currentProwess"] = prowess & critical_mask | double << double_shift
            elif effect == "INCREASE_COUNTER":
                counter = min(3, ((prowess >> counter_shift) & lower_mask) + value)
                after["currentProwess"] = (
                    prowess & (critical_mask | double_mask) | counter << counter_shift
                )
            elif effect == "SET_CRITICAL":
                after["currentProwess"] = (
                    prowess & (double_mask | counter_mask) | value & critical_mask
                )
            elif effect == "SET_DOUBLE":
                after["currentProwess"] = (
                    prowess & (critical_mask | counter_mask)
                    | (value & lower_mask) << double_shift
                )
            elif effect == "SET_COUNTER":
                after["currentProwess"] = (
                    prowess & (critical_mask | double_mask)
                    | (value & lower_mask) << counter_shift
                )
            else:
                raise ValueError(f"unmodeled level-up refresh equip effect: {effect}")
        if cursed:
            after["status"] |= equates["STATUSEFFECT_CURSE"]

    return {
        "after": after,
        "updateStatsCallObserved": True,
        "updateStatsEntryObserved": True,
    }


def _verify_refresh_model(
    fixture: dict[str, Any], *, disasm: Path, curves: dict[int, dict[int, tuple[int, int]]]
) -> None:
    itemdefs = (disasm / "data/stats/items/itemdefs.asm").read_text(encoding="utf-8")
    expected_prowess_effect_counts = fixture["itemTableProwessEffectCounts"]
    for effect, expected_count in expected_prowess_effect_counts.items():
        actual_count = len(re.findall(rf"\b{effect}\s*,", itemdefs))
        if actual_count != expected_count:
            raise ValueError(
                f"level-up refresh {effect} item-table count drift: "
                f"expected {expected_count}, got {actual_count}"
            )

    for case in fixture["cases"]:
        model = _model_refresh_case(case, disasm=disasm, curves=curves)
        if any(case[field] != value for field, value in model.items()):
            raise ValueError(f"level-up refresh golden disagrees with source model: {case['id']}")


def _verify_refresh_observation(
    fixture: dict[str, Any], case: dict[str, Any], observed: dict[str, Any]
) -> None:
    if observed.get("system") != "GEN" or observed.get("core") != fixture["emulator"]["core"]:
        raise ValueError("unexpected level-up refresh execution system/core")
    expected = {
        "id": case["id"],
        "ally": case["ally"],
        "seed": case["seed"],
        "before": case["input"],
        "after": case["after"],
        "updateStatsCallObserved": case["updateStatsCallObserved"],
        "updateStatsEntryObserved": case["updateStatsEntryObserved"],
    }
    if observed.get("result") != expected:
        raise ValueError("level-up refresh runtime mismatch")


def _run_refresh_observations(
    fixture: dict[str, Any], rom_path: Path, *, timeout_seconds: int
) -> None:
    for case in fixture["cases"]:
        observed = run_observer(
            rom_path=rom_path,
            observer_path=REFRESH_OBSERVER,
            config={
                "function": fixture["function"],
                "ram": fixture["ram"],
                "case": {
                    "id": case["id"],
                    "ally": case["ally"],
                    "seed": case["seed"],
                    "input": case["input"],
                },
            },
            output_name=f"level-up-refresh-{case['id']}",
            timeout_seconds=timeout_seconds,
        )
        _verify_refresh_observation(fixture, case, observed)


def verify_level_up_refresh(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 60
) -> dict[str, Any]:
    fixture = load_json(REFRESH_FIXTURE)
    validate_json(fixture, REFRESH_SCHEMA, owner=str(REFRESH_FIXTURE))
    verify_runtime_contract(fixture, rom_path)
    disasm = _verify_upstream(upstream_path)
    _verify_refresh_model(fixture, disasm=disasm, curves=_parse_growth_curves(disasm))
    _run_refresh_observations(fixture, rom_path, timeout_seconds=timeout_seconds)
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"]),
        "ProwessItemEffects": [
            "INCREASE_CRITICAL",
            "INCREASE_DOUBLE",
            "INCREASE_COUNTER",
            "SET_CRITICAL",
        ],
        "UnusedProwessItemEffects": fixture["unreferencedProwessEffects"],
        "Status": "PASS",
    }


def _verify_prowess_models(fixture: dict[str, Any], *, disasm: Path) -> None:
    equates = _parse_equates(disasm)
    starts = _parse_ally_starts(disasm)
    ally_codes = _parse_ally_codes(disasm)
    source_case = fixture["cases"][0]
    ally = source_case["ally"]
    class_code, starting_level = starts[ally]
    if (
        ally_codes[ally] != source_case["allyCode"]
        or class_code != source_case["classCode"]
    ):
        raise ValueError("Karna prowess source identity drift")
    if starting_level != source_case["startingLevel"]:
        raise ValueError("Karna prowess starting-level drift")

    class_id = equates[f"CLASS_{class_code}"]
    initial = _parse_class_prowess(disasm, equates)[class_id]
    block = _parse_stats_block(disasm, ally, class_code)
    heal3 = equates["SPELL_HEAL"] | equates["SPELL_LV3"]
    matching = [
        spell
        for spell in block["spells"]
        if _encode_spell(spell["expression"], equates) == heal3
    ]
    if len(matching) != 1 or matching[0]["level"] > starting_level:
        raise ValueError("Karna HEAL 3 source contract drift")

    cases = fixture["cases"]
    critical = initial & equates["PROWESS_MASK_CRITICAL"]
    expected_inputs = [(high_nibble << 4) | critical for high_nibble in range(16)]
    if len(cases) != len(expected_inputs):
        raise ValueError("Karna HEAL 3 prowess matrix size drift")

    for high_nibble, (case, expected_before) in enumerate(
        zip(cases, expected_inputs, strict=True)
    ):
        if any(
            case[field] != source_case[field]
            for field in ("ally", "allyCode", "classCode", "startingLevel")
        ):
            raise ValueError("synthetic HEAL 3 case source identity drift")
        expected_id = f"karna-prst-heal3-high-{high_nibble:x}"
        if high_nibble == 0:
            expected_id += "-natural"
        if case["id"] != expected_id:
            raise ValueError("Karna HEAL 3 prowess matrix ordering drift")
        before = case["injectedBaseProwess"]
        if high_nibble == 0:
            if before is not None:
                raise ValueError("natural HEAL 3 case must not inject prowess")
            before = initial
        elif before != expected_before:
            raise ValueError("synthetic HEAL 3 prowess matrix input drift")
        if before != expected_before:
            raise ValueError("Karna HEAL 3 prowess matrix critical nibble drift")
        shifted = (before >> equates["PROWESS_LOWER_DOUBLE_SHIFT_COUNT"]) + 1
        if shifted == 8:
            shifted = 7
        after = (before & equates["PROWESS_MASK_CRITICAL"]) | (
            shifted << equates["PROWESS_LOWER_DOUBLE_SHIFT_COUNT"]
        )
        modeled = {
            "startingLevel": starting_level,
            "effectiveLevel": starting_level,
            "spell": heal3,
            "baseProwessBefore": before,
            "baseProwessAfter": after & 0xFF,
        }
        if any(case[field] != value for field, value in modeled.items()):
            raise ValueError("Karna HEAL 3 prowess golden disagrees with source model")


def _verify_prowess_observation(
    fixture: dict[str, Any], case: dict[str, Any], observed: dict[str, Any]
) -> None:
    if observed.get("system") != "GEN" or observed.get("core") != fixture["emulator"]["core"]:
        raise ValueError("unexpected Karna prowess execution system/core")
    expected = {
        "id": case["id"],
        "ally": case["ally"],
        "startingLevel": case["startingLevel"],
        "effectiveLevel": case["effectiveLevel"],
        "spell": case["spell"],
        "baseProwessBefore": case["baseProwessBefore"],
        "baseProwessAfter": case["baseProwessAfter"],
    }
    if observed.get("result") != expected:
        raise ValueError("Karna HEAL 3 prowess runtime mismatch")


def _run_prowess_observations(
    fixture: dict[str, Any], rom_path: Path, *, timeout_seconds: int
) -> None:
    for case in fixture["cases"]:
        observed = run_observer(
            rom_path=rom_path,
            observer_path=PROWESS_OBSERVER,
            config={
                "function": fixture["function"],
                "ram": fixture["ram"],
                "case": {
                    "id": case["id"],
                    "ally": case["ally"],
                    "spell": case["spell"],
                    "injectedBaseProwess": case["injectedBaseProwess"],
                },
            },
            output_name=f"ally-initialization-prowess-{case['id']}",
            timeout_seconds=timeout_seconds,
        )
        _verify_prowess_observation(fixture, case, observed)


def verify_initialization_prowess(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 60
) -> dict[str, Any]:
    fixture = load_json(PROWESS_FIXTURE)
    validate_json(fixture, PROWESS_SCHEMA, owner=str(PROWESS_FIXTURE))
    verify_runtime_contract(fixture, rom_path)
    _verify_prowess_models(fixture, disasm=_verify_upstream(upstream_path))
    _run_prowess_observations(fixture, rom_path, timeout_seconds=timeout_seconds)
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"]),
        "Transitions": [
            f"0x{case['baseProwessBefore']:02X}->0x{case['baseProwessAfter']:02X}"
            for case in fixture["cases"]
        ],
        "Status": "PASS",
    }


def verify_growth(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 60
) -> dict[str, Any]:
    stat = load_json(STAT_FIXTURE)
    level = load_json(LEVEL_FIXTURE)
    boundary = load_json(BOUNDARY_FIXTURE)
    refresh = load_json(REFRESH_FIXTURE)
    prowess = load_json(PROWESS_FIXTURE)
    validate_json(stat, STAT_SCHEMA, owner=str(STAT_FIXTURE))
    validate_json(level, LEVEL_SCHEMA, owner=str(LEVEL_FIXTURE))
    validate_json(boundary, BOUNDARY_SCHEMA, owner=str(BOUNDARY_FIXTURE))
    validate_json(refresh, REFRESH_SCHEMA, owner=str(REFRESH_FIXTURE))
    validate_json(prowess, PROWESS_SCHEMA, owner=str(PROWESS_FIXTURE))
    verify_runtime_contract(stat, rom_path)
    verify_runtime_contract(level, rom_path)
    verify_runtime_contract(boundary, rom_path)
    verify_runtime_contract(refresh, rom_path)
    verify_runtime_contract(prowess, rom_path)
    disasm = _verify_upstream(upstream_path)
    curves = _parse_growth_curves(disasm)
    _verify_stat_models(stat, curves)
    _verify_level_models(level, disasm=disasm, curves=curves)
    _verify_boundary_models(boundary, disasm=disasm, curves=curves)
    _verify_refresh_model(refresh, disasm=disasm, curves=curves)
    _verify_prowess_models(prowess, disasm=disasm)

    stat_observed = run_observer(
        rom_path=rom_path,
        observer_path=STAT_OBSERVER,
        config={
            "function": stat["function"],
            "cases": [
                {"id": case["id"], "seeds": [rng["seed"] for rng in case["rng"]]}
                for case in stat["cases"]
            ],
        },
        output_name="stat-gain",
        timeout_seconds=timeout_seconds,
    )
    _verify_stat_observation(stat, stat_observed)
    level_observed = run_observer(
        rom_path=rom_path,
        observer_path=LEVEL_OBSERVER,
        config={
            "function": level["function"],
            "ram": level["ram"],
            "cases": [
                {"id": case["id"], "ally": case["ally"], "seed": case["seed"]}
                for case in level["cases"]
            ],
        },
        output_name="level-up",
        timeout_seconds=timeout_seconds,
    )
    _verify_level_observation(level, level_observed)
    boundary_observed = run_observer(
        rom_path=rom_path,
        observer_path=BOUNDARY_OBSERVER,
        config={
            "function": boundary["function"],
            "ram": boundary["ram"],
            "cases": [
                {
                    "id": case["id"],
                    "ally": case["ally"],
                    "seed": case["seed"],
                    "input": case["input"],
                }
                for case in boundary["cases"]
            ],
        },
        output_name="level-up-boundaries",
        timeout_seconds=timeout_seconds,
    )
    _verify_boundary_observation(boundary, boundary_observed)
    _run_refresh_observations(refresh, rom_path, timeout_seconds=timeout_seconds)
    _run_prowess_observations(prowess, rom_path, timeout_seconds=timeout_seconds)
    return {
        "StatGainFixture": stat["id"],
        "StatGainCases": len(stat["cases"]),
        "LevelUpFixture": level["id"],
        "LevelUpCases": len(level["cases"]),
        "BoundaryFixture": boundary["id"],
        "BoundaryCases": len(boundary["cases"]),
        "RefreshFixture": refresh["id"],
        "RefreshCases": len(refresh["cases"]),
        "ProwessFixture": prowess["id"],
        "ProwessCases": len(prowess["cases"]),
        "TortBoundaryConfirmed": True,
        "Engine": f"BizHawk {stat['emulator']['version']} / {stat['emulator']['core']}",
        "Status": "PASS",
    }
