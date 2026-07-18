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
STAT_OBSERVER = repo_path("tools/bizhawk/stat_gain_observer.lua")
LEVEL_OBSERVER = repo_path("tools/bizhawk/level_up_observer.lua")
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
    pattern = r"^([A-Z0-9_]+):\s+equ\s+(\d+|0x[0-9A-Fa-f]+)"
    return {
        name: int(value, 0)
        for name, value in re.findall(pattern, text, re.MULTILINE)
    }


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
    following = text[marker.end() :]
    next_block = re.search(r"(?:^|:)\s*forClass\s+", following, re.MULTILINE)
    block = following[: next_block.start()] if next_block else following
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
    spell_block = block[block.find("spellList") :] if "spellList" in block else ""
    spell_pattern = r"(\d+)\s*,\s*[A-Z][A-Z0-9_]*(?:\|LV[1-4])?"
    spells = [int(level) for level in re.findall(spell_pattern, spell_block)]
    return {"stats": stats, "spellLevels": spells}


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
    if effective_level in block["spellLevels"]:
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


def verify_growth(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 60
) -> dict[str, Any]:
    stat = load_json(STAT_FIXTURE)
    level = load_json(LEVEL_FIXTURE)
    validate_json(stat, STAT_SCHEMA, owner=str(STAT_FIXTURE))
    validate_json(level, LEVEL_SCHEMA, owner=str(LEVEL_FIXTURE))
    verify_runtime_contract(stat, rom_path)
    verify_runtime_contract(level, rom_path)
    disasm = _verify_upstream(upstream_path)
    curves = _parse_growth_curves(disasm)
    _verify_stat_models(stat, curves)
    _verify_level_models(level, disasm=disasm, curves=curves)

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
    return {
        "StatGainFixture": stat["id"],
        "StatGainCases": len(stat["cases"]),
        "LevelUpFixture": level["id"],
        "LevelUpCases": len(level["cases"]),
        "TortBoundaryConfirmed": True,
        "Engine": f"BizHawk {stat['emulator']['version']} / {stat['emulator']['core']}",
        "Status": "PASS",
    }
