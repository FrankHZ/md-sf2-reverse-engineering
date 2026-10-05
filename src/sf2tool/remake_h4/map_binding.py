"""Compose source, historical applicability and controlled mutable-map delivery."""

import subprocess
from pathlib import Path

from sf2tool.paths import repo_path
from sf2tool.remake_h4.map_cohort import required_regions
from sf2tool.remake_h4.map_history import admit_history, classify_coverage, collect_history
from sf2tool.remake_h4.map_source import source_population, source_regions
from sf2tool.remake_h4.map_witness import compare_witnesses
from sf2tool.remake_h4_reference import UPSTREAM

OWNER = "docs/design/contracts/map3-battle01-continuous-scenario.md"


def map_consumer_binding(actual, context, source_root):
    """Composed mutable-map delivery; never retrofill historical working layouts.

    Source tables define every region and word. Complete selected caller history
    defines applicability (Inferred where Submit operands were omitted). Separate
    controlled sessions supply mechanism and actual draw witnesses. Other map
    source/texture requirements remain the reached-material predicate's job.
    """
    result = dict(
        value=None,
        checks=[],
        witnesses=[],
        coverage=[],
        historical=dict(
            exactWorkingLayout=None, exactCoordinateDraw=None, callerContext="Inferred"
        ),
        sourceRules=dict(upstream=UPSTREAM, owner=OWNER, section="composed-mutable-map-delivery"),
    )

    def check(name, value, label=None):
        result["checks"].append(dict(name=name, value=value, label=label))

    def finish():
        values = [c["value"] for c in result["checks"]]
        result["value"] = (
            False if False in values else None if None in values or not values else True
        )
        return result

    context = context or {}
    history, receipt, session = admit_history(actual, context, check)
    root = (
        (Path(source_root).resolve() if Path(source_root).is_absolute() else repo_path(source_root))
        if source_root
        else None
    )
    maps = {}
    try:
        if root is None:
            raise FileNotFoundError("source root")
        check(
            "pinned original map source",
            subprocess.check_output(
                ["git", "-C", str(root), "rev-parse", "HEAD"], text=True
            ).strip()
            == UPSTREAM
            and subprocess.run(
                ["git", "-C", str(root), "diff", "--quiet", UPSTREAM, "--", "disasm"], check=False
            ).returncode
            == 0,
        )
        maps = {m: source_regions(root, m) for m in ("map-3", "map-19")}
    except (OSError, subprocess.SubprocessError):
        check("source operands available", None)
    except (ValueError, IndexError):
        check("source table shape", False)
    if not maps:
        return finish()

    states, events = collect_history(history, receipt, maps, check)
    school_entities = source_population(root, events, check)
    sources, wanted = required_regions(maps)
    classify_coverage(states, maps, result["coverage"], check)
    result["missingWitnesses"] = compare_witnesses(
        context, maps, sources, wanted, school_entities, session, root, result["witnesses"], check
    )
    return finish()
