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
STAT_OBSERVER = repo_path("tools/bizhawk/stat_gain_observer.lua")
LEVEL_OBSERVER = repo_path("tools/bizhawk/level_up_observer.lua")
BOUNDARY_OBSERVER = repo_path("tools/bizhawk/level_up_boundaries_observer.lua")
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
    return {"stats": stats, "spells": spells}


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
    block = _parse_stats_block(disasm, ally, case["classCode"])
    class_id = equates[f"CLASS_{case['classCode']}"]
    values = case["input"]
    if values["class"] != class_id:
        raise ValueError(f"level-up boundary class identity drift: {case['id']}")
    for stat, growth in block["stats"].items():
        if values[stat] != growth[case["inputBasis"]]:
            raise ValueError(f"level-up boundary {stat} basis drift: {case['id']}")

    promoted = class_id >= equates["CHAR_CLASS_FIRSTPROMOTED"]
    cap = equates["CHAR_LEVELCAP_PROMOTED"] if promoted else equates["CHAR_LEVELCAP_BASE"]
    if values["level"] >= cap:
        return {
            "after": values,
            "capExit": True,
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
            for spell in block["spells"]
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
    for expected, actual in zip(fixture["cases"], observed["results"], strict=True):
        normalized = {
            "id": expected["id"],
            "ally": expected["ally"],
            "seed": expected["seed"],
            "before": expected["input"],
            "after": expected["after"],
            "capExit": expected["capExit"],
            "extraLevelBranch": expected["extraLevelBranch"],
            "levelBeforeExtra": expected["levelBeforeExtra"],
            "effectiveLevel": expected["effectiveLevel"],
            "observedSeed": expected["expectedSeed"],
            "arguments": expected["arguments"],
        }
        if actual != normalized:
            raise ValueError(f"level-up boundary runtime mismatch: {expected['id']}")


def verify_growth(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 60
) -> dict[str, Any]:
    stat = load_json(STAT_FIXTURE)
    level = load_json(LEVEL_FIXTURE)
    boundary = load_json(BOUNDARY_FIXTURE)
    validate_json(stat, STAT_SCHEMA, owner=str(STAT_FIXTURE))
    validate_json(level, LEVEL_SCHEMA, owner=str(LEVEL_FIXTURE))
    validate_json(boundary, BOUNDARY_SCHEMA, owner=str(BOUNDARY_FIXTURE))
    verify_runtime_contract(stat, rom_path)
    verify_runtime_contract(level, rom_path)
    verify_runtime_contract(boundary, rom_path)
    disasm = _verify_upstream(upstream_path)
    curves = _parse_growth_curves(disasm)
    _verify_stat_models(stat, curves)
    _verify_level_models(level, disasm=disasm, curves=curves)
    _verify_boundary_models(boundary, disasm=disasm, curves=curves)

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
    return {
        "StatGainFixture": stat["id"],
        "StatGainCases": len(stat["cases"]),
        "LevelUpFixture": level["id"],
        "LevelUpCases": len(level["cases"]),
        "BoundaryFixture": boundary["id"],
        "BoundaryCases": len(boundary["cases"]),
        "TortBoundaryConfirmed": True,
        "Engine": f"BizHawk {stat['emulator']['version']} / {stat['emulator']['core']}",
        "Status": "PASS",
    }
