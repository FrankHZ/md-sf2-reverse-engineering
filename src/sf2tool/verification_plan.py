"""Read-only affected verification planning for committed Git ranges."""

from __future__ import annotations

import ast
import json
import subprocess
from collections import defaultdict
from collections.abc import Iterable
from dataclasses import dataclass
from functools import cache
from pathlib import Path
from urllib.parse import urlparse

from sf2tool.h3.bootstrap import COMMAND_LAUNCHES
from sf2tool.paths import repo_path


@dataclass(frozen=True)
class VerificationPartition:
    """One stable scheduling and maintenance boundary."""

    partition_id: str
    layer: str
    description: str
    commands: tuple[str, ...]
    parallel_safe: bool = True
    resource_lock: str | None = None
    external_gates: tuple[str, ...] = ()


def _group_pairs(pairs: Iterable[tuple[str, str]]) -> dict[str, list[str]]:
    grouped: dict[str, list[str]] = defaultdict(list)
    for key, value in pairs:
        grouped[key].append(value)
    return grouped


H2_COMMAND_GROUPS = {
    "h2-battle-logic": (
        "battle-actions",
        "battle-ai",
        "battle-control",
        "battle-cutscene-data",
        "battle-cutscenes",
        "battle-functions",
        "battle-global-data",
        "battle-loop",
        "battle-routing-data",
        "battle-scene-engine",
        "battle-spriteset-data",
        "battlefield",
        "map3-battle01-action-completion",
        "map3-battle01-action-effect",
        "map3-battle01-turn-control",
        "map3-battle01-turn-finalization",
        "map3-battle01-victory-return",
    ),
    "h2-stats-items": (
        "ally-data",
        "common-stats",
        "core-stats-data",
        "enemy-drops",
        "enemy-gold",
        "enemy-map-sprites",
        "item-auxiliary",
    ),
    "h2-map-scripting": (
        "common-maps",
        "common-scripting",
        "entity-action-scripts",
        "map3-battle01-admission",
        "map3-castle-battle-unlock",
        "map3-optional-interactions",
        "map-content",
        "map-data",
        "map-event-direct-control",
        "map-event-dialogue-state",
        "map-event-direct-handoff",
        "map-event-predicate-results",
        "map-event-random-battle-state",
        "map-event-tactical-base-quote-state",
        "map-event-request-state",
        "map-event-request-consumption",
        "map-event-direct-state",
        "map-event-interaction-state",
        "map-event-item-transactions",
        "map-event-combatant-state",
        "map-descriptions",
        "map-entities",
        "map-events",
        "map-import",
        "map-init",
        "map-layouts",
        "map-script-engine",
        "map-scripts",
        "map-setup",
        "map-sprite-assignments",
        "sprite-dialogue",
    ),
    "h2-presentation": (
        "battle-backgrounds",
        "battle-effect-graphics",
        "battle-scene-animations",
        "battle-sprite-animations",
        "battle-sprites",
        "battle-terrain",
        "battle-weapon-ground",
        "compression-consumers",
        "icon-graphics",
        "map-palettes",
        "map-sprites",
        "map-tilesets",
        "portraits",
        "special-screen-graphics",
        "special-screen-presentation",
        "special-screens",
        "special-sprites",
        "text-banks",
        "text-huffman",
        "ui-graphics",
        "ui-layouts",
        "unused-tech-assets",
        "variable-width-font",
        "witch-menu-graphics",
    ),
    "h2-services-state": (
        "auxiliary-data",
        "common-menus",
        "field-item-effects",
        "field-menu-control",
        "field-search-control",
        "gameflow-core",
        "remaining-core",
        "tech-graphics",
        "tech-interfaces",
        "tech-interrupts",
        "tech-services",
    ),
    "h2-sound": ("sound-data",),
}

H3_PROFILE_PARTITIONS = {
    "battle01-intro-skip": "h3-battle01",
    "map-debug-host": "h3-map-debug",
    "direct-function-seam": "h3-direct-seam",
    "witch-menu": "h3-witch",
    "sound-driver": "h3-sound",
}

H2_MODULE_ALIASES = {
    "common-maps": "maps",
    "common-menus": "menus",
    "common-scripting": "scripting",
    "common-stats": "stats",
    "gameflow-core": "gameflow",
    "special-screens": "screens",
    "tech-graphics": "graphics",
    "tech-interfaces": "interfaces",
    "tech-interrupts": "interrupts",
    "tech-services": "services",
    "unused-tech-assets": "unused_technical_assets",
}

H2_COMMAND_PARTITIONS = {
    command: partition_id
    for partition_id, commands in H2_COMMAND_GROUPS.items()
    for command in commands
}
H2_MODULE_COMMANDS = {
    H2_MODULE_ALIASES.get(command, command.replace("-", "_")): command
    for command in H2_COMMAND_PARTITIONS
}
H2_COMMAND_MODULES = {command: module for module, command in H2_MODULE_COMMANDS.items()}
H3_MODULE_COMMANDS: dict[str, tuple[str, ...]] = {
    module: tuple(sorted(commands))
    for module, commands in _group_pairs(
        (launch.dispatch_module.rsplit(".", 1)[1], command)
        for command, launch in COMMAND_LAUNCHES.items()
    ).items()
}
def _commands(prefix: str, commands: tuple[str, ...]) -> tuple[str, ...]:
    return tuple(f"uv run sf2 {prefix} {command}" for command in commands)


PARTITIONS = (
    VerificationPartition(
        "public-core",
        "public",
        "Always-run commit-critical verification and tracked-input boundary.",
        ("uv run sf2 verify",),
        parallel_safe=False,
        external_gates=("GitHub Public / tracked-inputs",),
    ),
    VerificationPartition(
        "tooling-python",
        "affected",
        "Python tooling and focused regression tests.",
        ("uv run pytest",),
    ),
    VerificationPartition(
        "h1-original",
        "affected",
        "Bit-perfect original rebuild and source/toolchain identity.",
        (),
        parallel_safe=False,
        resource_lock="original-rebuild",
    ),
    *(
        VerificationPartition(
            partition_id,
            "affected",
            partition_id.removeprefix("h2-").replace("-", " ").title(),
            _commands("h2", commands),
        )
        for partition_id, commands in H2_COMMAND_GROUPS.items()
    ),
    *(
        VerificationPartition(
            partition_id,
            "affected",
            profile.replace("-", " ").title(),
            _commands(
                "h3",
                tuple(
                    sorted(
                        command
                        for command, launch in COMMAND_LAUNCHES.items()
                        if launch.profile == profile
                    )
                ),
            ),
            parallel_safe=False,
            resource_lock="bizhawk-original-runtime",
        )
        for profile, partition_id in H3_PROFILE_PARTITIONS.items()
    ),
)
PARTITIONS_BY_ID = {partition.partition_id: partition for partition in PARTITIONS}
H2_PARTITION_IDS = tuple(H2_COMMAND_GROUPS)
H3_PARTITION_IDS = tuple(H3_PROFILE_PARTITIONS.values())
EVIDENCE_PARTITION_IDS = ("h1-original", *H2_PARTITION_IDS, *H3_PARTITION_IDS)
ARTIFACT_PREFIXES = (
    "manifests/extractions/",
    "schemas/",
    "tests/fixtures/",
    "tools/bizhawk/",
)
H2_SHARED_ARTIFACT_PARTITIONS = {
    "manifests/extractions/battle01-data.json": ("h2-battle-logic",),
    "manifests/extractions/battle01-scene.json": (
        "h2-battle-logic",
        "h2-presentation",
    ),
    "manifests/extractions/enemy-promotion-rom-layout.json": ("h2-stats-items",),
    "manifests/extractions/rom-static-layout.json": H2_PARTITION_IDS,
}
H3_LEGACY_BATTLE01_ARTIFACT_STEMS = (
    "attack-chain",
    "battle-scene-replay",
    "battle01-region-activation",
    "battle01-secondary-activation",
    "battle01-turn-order",
    "counter-burst-rock",
    "counter-range",
    "counter-same-side",
    "counter-sleep",
    "counter-special-enemies",
    "counter-stun",
    "dodge",
    "double-validation",
    "lethal-followup",
    "physical-damage-application",
    "physical-damage",
    "turn-order-boundaries",
)
H3_SHARED_ARTIFACT_PARTITIONS = {
    "tools/bizhawk/bootstrap.lua": H3_PARTITION_IDS,
    "tools/bizhawk/json.lua": H3_PARTITION_IDS,
    "schemas/h3/observer-callback-contract.schema.json": H3_PARTITION_IDS,
    "schemas/h3/observer-failure-contract.schema.json": H3_PARTITION_IDS,
    **{
        f"tests/fixtures/h3/{stem}-v1.json": ("h3-battle01",)
        for stem in H3_LEGACY_BATTLE01_ARTIFACT_STEMS
    },
    **{
        f"schemas/h3-{stem}-fixture.schema.json": ("h3-battle01",)
        for stem in H3_LEGACY_BATTLE01_ARTIFACT_STEMS
    },
}


def _git(root: Path, *arguments: str) -> str:
    completed = subprocess.run(
        ["git", *arguments],
        cwd=root,
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    )
    return completed.stdout.strip()


def _resolve_commit(root: Path, revision: str) -> str:
    return _git(root, "rev-parse", "--verify", "--end-of-options", f"{revision}^{{commit}}")


def _changed_paths(root: Path, merge_base: str, head: str) -> tuple[str, ...]:
    output = _git(root, "diff", "--name-only", "--diff-filter=ACDMRTUXB", "-z", merge_base, head)
    return tuple(sorted(path.replace("\\", "/") for path in output.split("\0") if path))


def _imports_for_path(root: Path, path: str) -> tuple[str, ...]:
    candidate = root / path
    if candidate.suffix != ".py" or not candidate.is_file():
        return ()
    try:
        tree = ast.parse(candidate.read_text(encoding="utf-8"))
    except (OSError, SyntaxError, UnicodeError):
        return ()
    modules: set[str] = set()
    for node in ast.walk(tree):
        if isinstance(node, ast.ImportFrom) and node.module:
            modules.add(node.module)
            modules.update(f"{node.module}.{alias.name}" for alias in node.names)
        elif isinstance(node, ast.Import):
            modules.update(alias.name for alias in node.names)
    return tuple(sorted(modules))


def _module_for_source_path(path: Path, root: Path) -> str:
    relative = path.relative_to(root / "src").with_suffix("")
    parts = relative.parts
    if parts[-1] == "__init__":
        parts = parts[:-1]
    return ".".join(parts)


def _source_module_paths(root: Path) -> dict[str, Path]:
    return {
        _module_for_source_path(path, root): path
        for path in (root / "src" / "sf2tool").rglob("*.py")
    }


def _source_artifact_literals(root: Path, source: Path) -> set[str]:
    try:
        tree = ast.parse(source.read_text(encoding="utf-8"))
    except (OSError, SyntaxError, UnicodeError):
        return set()
    artifacts = set()
    for node in ast.walk(tree):
        if not isinstance(node, ast.Constant) or not isinstance(node.value, str):
            continue
        normalized = node.value.replace("\\", "/").split("#", 1)[0]
        if normalized.startswith(ARTIFACT_PREFIXES) and (root / normalized).is_file():
            artifacts.add(normalized)
    return artifacts


def _json_artifact_references(root: Path, artifact: str) -> set[str]:
    source = root / artifact
    if source.suffix != ".json" or not source.is_file():
        return set()
    try:
        value = json.loads(source.read_text(encoding="utf-8"))
    except (OSError, UnicodeError, json.JSONDecodeError):
        return set()

    references = set()

    def visit(item: object, *, ref_value: bool = False) -> None:
        if isinstance(item, dict):
            for key, child in item.items():
                visit(child, ref_value=key == "$ref")
            return
        if isinstance(item, list):
            for child in item:
                visit(child)
            return
        if not isinstance(item, str):
            return
        normalized = item.replace("\\", "/").split("#", 1)[0]
        if not normalized:
            return
        parsed = urlparse(normalized)
        if parsed.scheme in {"http", "https"} and parsed.path.startswith("/schemas/"):
            candidate = root / parsed.path.removeprefix("/")
        elif normalized.startswith(ARTIFACT_PREFIXES):
            candidate = root / normalized
        elif ref_value or normalized.endswith(".json"):
            candidate = source.parent / normalized
        else:
            return
        resolved = candidate.resolve()
        resolved_root = root.resolve()
        if not resolved.is_relative_to(resolved_root) or not resolved.is_file():
            return
        relative = resolved.relative_to(resolved_root).as_posix()
        if relative.startswith(ARTIFACT_PREFIXES):
            references.add(relative)

    visit(value)
    return references


def _artifact_closure(root: Path, seeds: set[str]) -> set[str]:
    pending = list(seeds)
    seen = set(seeds)
    while pending:
        artifact = pending.pop()
        for reference in _json_artifact_references(root, artifact):
            if reference not in seen:
                seen.add(reference)
                pending.append(reference)
    return seen


@cache
def _artifact_command_owners(root: Path, layer: str) -> dict[str, tuple[str, ...]]:
    root = root.resolve()
    source_modules = _source_module_paths(root)
    imports = {
        module: {
            imported
            for imported in _imports_for_path(root, path.relative_to(root).as_posix())
            if imported in source_modules
        }
        for module, path in source_modules.items()
    }
    source_artifacts = {
        module: _source_artifact_literals(root, path) for module, path in source_modules.items()
    }
    if layer == "h2":
        command_modules = {
            command: f"sf2tool.h2.{module}" for command, module in H2_COMMAND_MODULES.items()
        }
        extra_seeds: dict[str, set[str]] = {}
    elif layer == "h3":
        command_modules = {
            command: launch.dispatch_module for command, launch in COMMAND_LAUNCHES.items()
        }
        extra_seeds = {
            command: {
                *(launch.observers),
                *(
                    launch.cases_fixture
                    for launch in launch.launches
                    if launch.cases_fixture is not None
                ),
            }
            for command, launch in COMMAND_LAUNCHES.items()
        }
    else:
        raise ValueError(f"unsupported artifact layer: {layer}")

    owners: dict[str, set[str]] = defaultdict(set)
    command_owner_modules = set(command_modules.values())
    for command, seed_module in command_modules.items():
        pending = [seed_module]
        modules = {seed_module}
        while pending:
            module = pending.pop()
            for dependency in imports.get(module, set()):
                same_layer = dependency.startswith(f"sf2tool.{layer}.")
                shared_module = dependency.startswith("sf2tool.") and not dependency.startswith(
                    ("sf2tool.h2.", "sf2tool.h3.")
                )
                if dependency in command_owner_modules and dependency != seed_module:
                    continue
                if (same_layer or shared_module) and dependency not in modules:
                    modules.add(dependency)
                    pending.append(dependency)
        seeds = set(extra_seeds.get(command, set()))
        for module in modules:
            if module == "sf2tool.h3.bootstrap":
                continue
            seeds.update(source_artifacts.get(module, set()))
        for artifact in _artifact_closure(root, seeds):
            if (root / artifact).is_file():
                owners[artifact].add(command)
    return {artifact: tuple(sorted(commands)) for artifact, commands in sorted(owners.items())}


def h2_artifact_commands(root: Path | None = None) -> dict[str, tuple[str, ...]]:
    """Return closed H2 artifact ownership derived from command module declarations."""

    return _artifact_command_owners((repo_path(".") if root is None else root).resolve(), "h2")


def h3_artifact_commands(root: Path | None = None) -> dict[str, tuple[str, ...]]:
    """Return closed H3 artifact ownership derived from bootstrap and module declarations."""

    return _artifact_command_owners((repo_path(".") if root is None else root).resolve(), "h3")


def _selection_entry(
    selected: dict[str, dict[str, set[str]]],
    partition_id: str,
    reason: str,
    commands: tuple[str, ...] | None = None,
) -> None:
    entry = selected.setdefault(partition_id, {"reasons": set(), "commands": set()})
    entry["reasons"].add(reason)
    partition = PARTITIONS_BY_ID[partition_id]
    entry["commands"].update(partition.commands if commands is None else commands)


def _select_h2_command(
    selected: dict[str, dict[str, set[str]]], command: str, reason: str
) -> None:
    partition_id = H2_COMMAND_PARTITIONS[command]
    _selection_entry(selected, partition_id, reason, (f"uv run sf2 h2 {command}",))


def _select_h3_command(
    selected: dict[str, dict[str, set[str]]], command: str, reason: str
) -> None:
    launch = COMMAND_LAUNCHES[command]
    partition_id = H3_PROFILE_PARTITIONS[launch.profile]
    _selection_entry(selected, partition_id, reason, (f"uv run sf2 h3 {command}",))


def _select_all(
    selected: dict[str, dict[str, set[str]]], partition_ids: tuple[str, ...], reason: str
) -> None:
    for partition_id in partition_ids:
        _selection_entry(selected, partition_id, reason)


def _select_imports(
    selected: dict[str, dict[str, set[str]]], root: Path, path: str
) -> bool:
    matched = False
    for module in _imports_for_path(root, path):
        if module.startswith("sf2tool.h2."):
            command = H2_MODULE_COMMANDS.get(module.rsplit(".", 1)[1])
            if command is not None:
                _select_h2_command(selected, command, f"{path} imports {module}")
                matched = True
            else:
                source = (root / "src" / Path(*module.split("."))).with_suffix(".py")
                if source.is_file():
                    relative = source.relative_to(root).as_posix()
                    matched = _select_dependents(selected, root, relative) or matched
        elif module.startswith("sf2tool.h3."):
            commands = H3_MODULE_COMMANDS.get(module.rsplit(".", 1)[1], ())
            for command in commands:
                _select_h3_command(selected, command, f"{path} imports {module}")
                matched = True
            if not commands:
                source = (root / "src" / Path(*module.split("."))).with_suffix(".py")
                if source.is_file():
                    relative = source.relative_to(root).as_posix()
                    matched = _select_dependents(selected, root, relative) or matched
    return matched


def _select_dependents(
    selected: dict[str, dict[str, set[str]]], root: Path, path: str
) -> bool:
    target_path = root / path
    target_module = _module_for_source_path(target_path, root)
    reverse: dict[str, set[str]] = defaultdict(set)
    for source_path in (root / "src" / "sf2tool").rglob("*.py"):
        module = _module_for_source_path(source_path, root)
        relative = source_path.relative_to(root).as_posix()
        for imported in _imports_for_path(root, relative):
            reverse[imported].add(module)

    pending = [target_module]
    seen = {target_module}
    while pending:
        current = pending.pop()
        for dependent in reverse.get(current, set()):
            if dependent not in seen:
                seen.add(dependent)
                pending.append(dependent)

    matched = False
    if "sf2tool.harness" in seen:
        _selection_entry(selected, "h1-original", f"{path} reaches sf2tool.harness")
        matched = True
    for module in sorted(seen):
        if module.startswith("sf2tool.h2."):
            command = H2_MODULE_COMMANDS.get(module.rsplit(".", 1)[1])
            if command is not None:
                _select_h2_command(selected, command, f"{path} reaches {module}")
                matched = True
        elif module.startswith("sf2tool.h3."):
            for command in H3_MODULE_COMMANDS.get(module.rsplit(".", 1)[1], ()):
                _select_h3_command(selected, command, f"{path} reaches {module}")
                matched = True
    return matched


def plan_paths(
    changed_paths: tuple[str, ...],
    *,
    root: Path | None = None,
    include_partitions: tuple[str, ...] = (),
) -> dict[str, object]:
    """Classify normalized repository paths without executing any gate."""

    root = repo_path(".") if root is None else root
    unknown_ids = sorted(set(include_partitions) - PARTITIONS_BY_ID.keys())
    if unknown_ids:
        raise ValueError(f"unknown verification partition(s): {', '.join(unknown_ids)}")

    selected: dict[str, dict[str, set[str]]] = {}
    unclassified: set[str] = set()
    _selection_entry(selected, "public-core", "always-run commit gate")
    for partition_id in include_partitions:
        _selection_entry(selected, partition_id, "explicit --include-partition")

    for path in changed_paths:
        normalized = path.replace("\\", "/")
        if normalized.startswith("src/sf2tool/h2/") and normalized.endswith(".py"):
            module = Path(normalized).stem
            command = H2_MODULE_COMMANDS.get(module)
            if command is not None:
                _select_h2_command(selected, command, normalized)
            elif _select_dependents(selected, root, normalized):
                pass
            else:
                _select_all(
                    selected,
                    H2_PARTITION_IDS,
                    f"shared or unknown H2 module: {normalized}",
                )
                unclassified.add(normalized)
            continue

        if normalized.startswith("src/sf2tool/h3/") and normalized.endswith(".py"):
            module = Path(normalized).stem
            commands = H3_MODULE_COMMANDS.get(module, ())
            if commands:
                for command in commands:
                    _select_h3_command(selected, command, normalized)
            else:
                _select_all(
                    selected,
                    H3_PARTITION_IDS,
                    f"shared or unknown H3 module: {normalized}",
                )
                if module not in {"__init__", "bizhawk", "bootstrap"}:
                    unclassified.add(normalized)
            continue

        if normalized.startswith(ARTIFACT_PREFIXES):
            h2_commands = h2_artifact_commands(root).get(normalized, ())
            h3_commands = h3_artifact_commands(root).get(normalized, ())
            if h2_commands or h3_commands:
                for command in h2_commands:
                    _select_h2_command(selected, command, normalized)
                for command in h3_commands:
                    _select_h3_command(selected, command, normalized)
                continue
            shared_partitions = H2_SHARED_ARTIFACT_PARTITIONS.get(normalized)
            if shared_partitions is not None:
                _select_all(selected, shared_partitions, f"known shared H2 input: {normalized}")
                continue
            shared_partitions = H3_SHARED_ARTIFACT_PARTITIONS.get(normalized)
            if shared_partitions is not None:
                _select_all(selected, shared_partitions, f"known shared H3 input: {normalized}")
                continue
            if normalized.startswith(
                ("tests/fixtures/h2/", "schemas/h2/", "schemas/h2-")
            ) or normalized.startswith("manifests/extractions/"):
                _select_all(selected, H2_PARTITION_IDS, f"unknown H2 input: {normalized}")
            elif normalized.startswith(
                ("tests/fixtures/h3/", "schemas/h3/", "schemas/h3-", "tools/bizhawk/")
            ):
                _select_all(selected, H3_PARTITION_IDS, f"unknown H3 input: {normalized}")
            else:
                _select_all(
                    selected,
                    (*H2_PARTITION_IDS, *H3_PARTITION_IDS),
                    f"unknown shared evidence input: {normalized}",
                )
            unclassified.add(normalized)
            continue

        if normalized.startswith("tests/python/") and normalized.endswith(".py"):
            commands = (
                (f"uv run pytest {normalized}",)
                if (root / normalized).is_file()
                else None
            )
            _selection_entry(
                selected,
                "tooling-python",
                normalized,
                commands,
            )
            _select_imports(selected, root, normalized)
            continue

        if normalized in {"pyproject.toml", "uv.lock", ".python-version"}:
            _selection_entry(selected, "tooling-python", normalized)
            _select_all(selected, EVIDENCE_PARTITION_IDS, f"shared toolchain input: {normalized}")
            continue

        if normalized == "src/sf2tool/harness.py":
            _selection_entry(selected, "tooling-python", normalized)
            _select_all(selected, EVIDENCE_PARTITION_IDS, f"shared harness input: {normalized}")
            continue

        if normalized == "src/sf2tool/cli.py":
            _selection_entry(selected, "tooling-python", normalized)
            _select_all(
                selected,
                EVIDENCE_PARTITION_IDS,
                f"shared CLI input: {normalized}",
            )
            continue

        if normalized.startswith("src/sf2tool/") and normalized.endswith(".py"):
            _selection_entry(selected, "tooling-python", normalized)
            _select_dependents(selected, root, normalized)
            continue

        if normalized.startswith("scripts/") and normalized.endswith(".ps1"):
            _select_all(selected, EVIDENCE_PARTITION_IDS, f"legacy shared rail: {normalized}")
            continue

        if normalized in {"manifests/roms/sf2-us.json", "manifests/toolchain.json"}:
            _select_all(selected, EVIDENCE_PARTITION_IDS, f"shared identity input: {normalized}")
            continue

        if normalized in {
            "manifests/research-index.json",
            "manifests/zh-translation-index.json",
        }:
            continue

        if normalized.startswith(("schemas/", "tests/fixtures/", "manifests/")):
            _select_all(
                selected,
                (*H2_PARTITION_IDS, *H3_PARTITION_IDS),
                f"unowned evidence input: {normalized}",
            )
            unclassified.add(normalized)

    rows = []
    for partition in PARTITIONS:
        entry = selected.get(partition.partition_id)
        if entry is None:
            continue
        rows.append(
            {
                "id": partition.partition_id,
                "layer": partition.layer,
                "description": partition.description,
                "reasons": sorted(entry["reasons"]),
                "commands": sorted(entry["commands"]),
                "parallelSafe": partition.parallel_safe,
                "resourceLock": partition.resource_lock,
                "externalGates": list(partition.external_gates),
            }
        )
    return {"partitions": rows, "unclassifiedPaths": sorted(unclassified)}


def build_verification_plan(
    base: str,
    head: str = "HEAD",
    *,
    root: Path | None = None,
    include_partitions: tuple[str, ...] = (),
) -> dict[str, object]:
    """Build a deterministic plan for a clean, checked-out committed head."""

    root = repo_path(".") if root is None else root
    resolved_base = _resolve_commit(root, base)
    resolved_head = _resolve_commit(root, head)
    checked_out_head = _resolve_commit(root, "HEAD")
    if resolved_head != checked_out_head:
        raise ValueError(
            "verification plan head must resolve to the checked-out HEAD commit "
            f"({checked_out_head}); got {resolved_head}"
        )
    worktree_status = _git(
        root,
        "--no-optional-locks",
        "status",
        "--porcelain=v1",
        "--untracked-files=all",
    )
    if worktree_status:
        status_lines = worktree_status.splitlines()
        status_summary = "; ".join(status_lines[:10])
        if len(status_lines) > 10:
            status_summary += f"; ... ({len(status_lines) - 10} more)"
        raise ValueError(
            "verification plan requires a clean analyzed worktree before classification; "
            f"git status --porcelain: {status_summary}"
        )
    merge_base = _git(root, "merge-base", resolved_base, resolved_head)
    changed_paths = _changed_paths(root, merge_base, resolved_head)
    classified = plan_paths(
        changed_paths,
        root=root,
        include_partitions=include_partitions,
    )
    return {
        "schemaVersion": 1,
        "mode": "read-only-plan",
        "base": resolved_base,
        "head": resolved_head,
        "mergeBase": merge_base,
        "changedPaths": list(changed_paths),
        **classified,
        "executionSemanticsChanged": False,
    }
