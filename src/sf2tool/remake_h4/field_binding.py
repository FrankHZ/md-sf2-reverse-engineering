"""Compose declared field context, accepted dependencies and bounded native cases."""

import subprocess
from collections import Counter
from pathlib import Path

from sf2tool.paths import repo_path
from sf2tool.remake_h4.admission_seed import admission_seed_binding
from sf2tool.remake_h4.field_case import service_case
from sf2tool.remake_h4.field_evidence import case_input
from sf2tool.remake_h4.field_values import _field_equal
from sf2tool.remake_h4_reference import UPSTREAM, require


def field_service_binding(actual, context, source_root, read_document):
    """Original service rules composed with bounded current input/state/Draw executions.

    Historical callback reads stay Unknown. The only admitted multi-service boundary
    is an unregistered portrait entry with no portrait effect and constrained continuation.
    """
    import xml.etree.ElementTree as ET

    result = dict(
        value=None,
        checks=[],
        cases=[],
        sourceRules=dict(
            commit=UPSTREAM,
            symbols=[
                "GenerateRandomNumber",
                "VInt_PerformPortraitBlinking",
                "VInt_UpdateEntities",
                "esc00_wait",
                "esc01_waitUntilDestination",
                "esc06_walkRandomly",
            ],
            owner="docs/design/contracts/map3-battle01-continuous-scenario.md",
        ),
        unknown=[
            "historical stripped per-callback state and original natural reads/timing",
            "intermediate NPC attempts are Inferred; only the bounded net effect is observed",
            "registered/text multi-service batches and wider NPC/collision domains",
        ],
        historical=dict(result="FAIL", reason="historical A disconnected seed latch unchanged"),
    )

    def check(name, value):
        result["checks"].append(dict(name=name, value=value))

    def merge(values):
        return False if False in values else None if None in values else True

    def local_input(name, limit):
        p = Path(context[name])
        p = p.resolve() if p.is_absolute() else repo_path(p).resolve()
        require(p.is_relative_to(repo_path("local").resolve()), "field evidence outside local/")
        require(p.stat().st_size <= limit, "field evidence input cap")
        return p

    if not isinstance(context, dict):
        check("independent declared field context", None)
        return result
    try:
        root = Path(source_root)
        root = root.resolve() if root.is_absolute() else repo_path(root)
        check(
            "pinned original source",
            subprocess.check_output(
                ["git", "-C", str(root), "rev-parse", "HEAD"], text=True
            ).strip()
            == UPSTREAM,
        )
        check(
            "original source unchanged",
            subprocess.run(
                ["git", "-C", str(root), "diff", "--quiet", UPSTREAM, "--", "disasm"], check=False
            ).returncode
            == 0,
        )
    except (OSError, TypeError, subprocess.CalledProcessError):
        check("original source available", None)
    try:
        if "fieldServiceCases" not in actual:
            sessions = {r.get("state", {}).get("sessionId") for r in actual.get("samples", [])}
            check("historical applicability session", context["historicalSessionId"] in sessions)
            actual = read_document(local_input("actualSupplement", 8 * 1024 * 1024))
        check(
            "bounded current evidence scope",
            actual.get("scope") == "field-service-local-composition-v1",
        )
        # Reviewed PR618 counters/order plus PR621's active seed transport. Compare
        # implementation dependencies directly; unrelated Git advances do not invalidate.
        accepted = "0fe122f548a9a2f882ea3cc524d3937bde7e34b9"
        for path in (
            "remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs",
            "remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationPortraitRunner.cs",
            "remake/src/Sf2.Remake.Domain/Maps/EntityMotion.cs",
            "src/sf2tool/h3/entity_movement.py",
        ):
            old = subprocess.check_output(
                ["git", "-C", str(repo_path(".")), "show", accepted + ":" + path],
                text=True,
                encoding="utf-8",
            )
            check(
                "accepted service dependency " + path,
                True if repo_path(path).read_text(encoding="utf-8") == old else None,
            )
        seed = admission_seed_binding(
            context.get("seedActual") or {}, context.get("seedContext") or {}, source_root
        )
        result["seedMechanism"] = seed
        check("accepted executed current seed transport", seed["value"])
        trx = ET.fromstring(local_input("serviceTrx", 256 * 1024).read_bytes())
        check(
            "accepted service execution identity",
            trx.get("id") == "0f1e82ee-33fd-4aa3-a3c5-af025ad1dd54",
        )
        methods = dict(
            PortraitCountersUseSourceTypewritingAndOrderedIndependentRng=5,
            PollCopySurvivesNpcThenBlinkAndMouthDraws=4,
            HeldCameraUsesCommonLiveEntityWindowPortraitAndRandomService=2,
            NodServicesLiveEntitiesThenWindowAndPortraitWithoutOverwritingPollCopy=2,
            PortraitEventCarriesTextTailAndScriptActivationThroughRealReturn=2,
            PortraitAdmissionUsesLiveSpeakerAndRepeatedFlagBranch=2,
        )
        executions = []
        for method, count in methods.items():
            prefix = "Sf2.Remake.Engine.Tests.ExplorationTextWaitTests." + method + "("
            tests = [
                t
                for t in trx.iter()
                if t.tag.endswith("UnitTestResult") and t.get("testName", "").startswith(prefix)
            ]
            check(
                "accepted executed " + method,
                None
                if len(tests) < count
                else len(tests) == count
                and len({t.get("testName") for t in tests}) == count
                and all(t.get("outcome") == "Passed" for t in tests),
            )
            executions.extend(t.get("executionId") for t in tests)
        result["serviceExecutions"] = executions
    except (KeyError, OSError, ValueError, TypeError, subprocess.CalledProcessError, ET.ParseError):
        check("accepted composition evidence available", None)
    profiles = context.get("profiles") or {}
    supplied = actual.get("fieldServiceCases") or []
    ids = [c.get("id") for c in supplied]
    check(
        "declared case coverage",
        False
        if len(set(ids)) != len(ids) or set(ids) - set(profiles)
        else True
        if set(ids) == set(profiles) and profiles
        else None,
    )
    coverage = Counter()
    sessions = []
    assemblies = None
    for case in supplied:
        if case.get("id") not in profiles:
            continue
        case_checks = []
        try:
            case = case_input(case, read_document)
            profile = profiles[case["id"]]
            check(
                "native source identity:" + case["id"],
                case["launch"]["head"] == context.get("nativeBase"),
            )
            check(
                "independently selected session:" + case["id"],
                case["sessionId"] == context["sessions"][case["id"]],
            )
            check(
                "independently selected build:" + case["id"],
                _field_equal(case["launch"]["assemblies"], context["assemblies"]),
            )
            check("launch case identity:" + case["id"], case["launch"]["case"] == case["id"])
            check(
                "actual observer source:" + case["id"],
                True
                if case["testedView"]
                == repo_path("remake/game/src/Exploration/ExplorationSessionView.cs").read_text(
                    encoding="utf-8"
                )
                else None,
            )
            if assemblies is None:
                assemblies = case["launch"]["assemblies"]
            check(
                "same built runtime across cases:" + case["id"],
                case["launch"]["assemblies"] == assemblies and len(assemblies) == 4,
            )
            check(
                "case profile matches independent declaration:" + case["id"],
                _field_equal(profile, case.get("profile")),
            )
            check(
                "process case identity:" + case["id"],
                case.get("process", {}).get("case") == case["id"],
            )
            sessions.append(case.get("sessionId"))
            binding = service_case(case, profile, case_checks)
            result["cases"].append(binding)
            coverage.update(binding["coverage"])
            check(
                "source/current case:" + case["id"],
                True
                if binding["result"] == "PASS"
                else False
                if binding["result"] == "FAIL"
                else None,
            )
        except (KeyError, OSError, ValueError, TypeError, IndexError, OverflowError):
            case_checks.append(dict(name="required case operand unavailable", value=None))
            check(
                "case operands available:" + str(case.get("id")),
                merge([c["value"] for c in case_checks]),
            )
            result["cases"].append(
                dict(
                    case=case.get("id"),
                    checks=case_checks,
                    coverage={},
                    result="FAIL"
                    if any(c["value"] is False for c in case_checks)
                    else "Unavailable",
                )
            )
    check(
        "distinct actual sessions",
        bool(sessions) and None not in sessions and len(sessions) == len(set(sessions)),
    )
    for branch in (
        "entities-disabled",
        "radius-rejected",
        "entity-rejected",
        "map-rejected",
        "motion-wait",
        "timer-pending",
        "timer-release",
        "walk-accepted",
        "portrait-unregistered",
        "portrait-typing",
        "portrait-not-typing",
        "draw-blink",
        "draw-mouth",
        "eyes-open",
        "eyes-closed",
        "mouth-open",
        "mouth-closed",
        "closed-draw",
    ):
        check("required source branch:" + branch, True if coverage[branch] else None)
    result["coverage"] = dict(coverage)
    result["value"] = merge([c["value"] for c in result["checks"]])
    return result
