from __future__ import annotations

import ast
import subprocess
from pathlib import Path

import pytest

import sf2tool.cli as cli
import sf2tool.verification_plan as verification_plan
from sf2tool.cli import build_parser
from sf2tool.h3.bootstrap import COMMAND_LAUNCHES
from sf2tool.verification_plan import (
    EVIDENCE_PARTITION_IDS,
    H2_COMMAND_PARTITIONS,
    H2_PARTITION_IDS,
    H2_SHARED_ARTIFACT_PARTITIONS,
    H3_PARTITION_IDS,
    H3_PROFILE_PARTITIONS,
    H3_SHARED_ARTIFACT_PARTITIONS,
    PARTITIONS,
    PARTITIONS_BY_ID,
    build_verification_plan,
    h2_artifact_commands,
    h3_artifact_commands,
    plan_paths,
)

ROOT = Path(__file__).resolve().parents[2]
CLI_PATH = ROOT / "src" / "sf2tool" / "cli.py"


def _registered_commands(group_name: str) -> set[str]:
    tree = ast.parse(CLI_PATH.read_text(encoding="utf-8"))
    return {
        node.args[0].value
        for node in ast.walk(tree)
        if isinstance(node, ast.Call)
        and isinstance(node.func, ast.Attribute)
        and node.func.attr == "add_parser"
        and isinstance(node.func.value, ast.Name)
        and node.func.value.id == group_name
        and node.args
        and isinstance(node.args[0], ast.Constant)
        and isinstance(node.args[0].value, str)
    }


def _partition_ids(plan: dict[str, object]) -> set[str]:
    return {row["id"] for row in plan["partitions"]}  # type: ignore[index, union-attr]


def _partition(plan: dict[str, object], partition_id: str) -> dict[str, object]:
    return next(  # type: ignore[return-value]
        row for row in plan["partitions"] if row["id"] == partition_id  # type: ignore[index, union-attr]
    )


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


def _initialize_git_repo(root: Path) -> str:
    root.mkdir()
    _git(root, "init", "--initial-branch=main")
    _git(root, "config", "user.name", "Verification Plan Test")
    _git(root, "config", "user.email", "verification-plan@example.invalid")
    (root / "README.md").write_text("base\n", encoding="utf-8")
    _git(root, "add", "README.md")
    _git(root, "commit", "-m", "base")
    return _git(root, "rev-parse", "HEAD")


def _tracked_h2_artifacts() -> set[str]:
    return set(
        _git(
            ROOT,
            "ls-files",
            "--",
            "tests/fixtures/h2/**",
            "schemas/h2*",
            "schemas/h2/**",
            "manifests/extractions/**",
        ).splitlines()
    )


def _tracked_h3_artifacts() -> set[str]:
    return set(
        _git(
            ROOT,
            "ls-files",
            "--",
            "tests/fixtures/h3/**",
            "schemas/h3*",
            "schemas/h3/**",
            "tools/bizhawk/**",
        ).splitlines()
    )


def _expected_artifact_commands(
    path: str,
    h2_owners: dict[str, tuple[str, ...]],
    h3_owners: dict[str, tuple[str, ...]],
) -> dict[str, set[str]]:
    expected = {"public-core": {"uv run sf2 verify"}}
    for command in h2_owners.get(path, ()):
        partition_id = H2_COMMAND_PARTITIONS[command]
        expected.setdefault(partition_id, set()).add(f"uv run sf2 h2 {command}")
    for command in h3_owners.get(path, ()):
        partition_id = H3_PROFILE_PARTITIONS[COMMAND_LAUNCHES[command].profile]
        expected.setdefault(partition_id, set()).add(f"uv run sf2 h3 {command}")
    for partition_id in (
        *H2_SHARED_ARTIFACT_PARTITIONS.get(path, ()),
        *H3_SHARED_ARTIFACT_PARTITIONS.get(path, ()),
    ):
        expected.setdefault(partition_id, set()).update(PARTITIONS_BY_ID[partition_id].commands)
    return expected


def test_partition_registry_owns_every_cli_evidence_command_once() -> None:
    assert set(H2_COMMAND_PARTITIONS) == _registered_commands("h2_commands")
    assert set(COMMAND_LAUNCHES) == _registered_commands("h3_commands")
    assert len(H2_COMMAND_PARTITIONS) == 80
    assert len(COMMAND_LAUNCHES) == 74
    assert len({partition.partition_id for partition in PARTITIONS}) == len(PARTITIONS)
    assert len(H2_PARTITION_IDS) == 6
    assert len(H3_PARTITION_IDS) == 5


def test_existing_verify_parse_semantics_remain_available() -> None:
    parser = build_parser()
    normal = parser.parse_args(["verify"])
    full = parser.parse_args(["verify", "--full", "--skip-runtime"])

    assert normal.verify_command is None
    assert normal.full is False
    assert normal.quick is False
    assert full.verify_command is None
    assert full.full is True
    assert full.skip_runtime is True


def test_verify_plan_parser_requires_base_and_accepts_explicit_partition() -> None:
    args = build_parser().parse_args(
        [
            "verify",
            "plan",
            "--base",
            "origin/main",
            "--head",
            "topic",
            "--include-partition",
            "h2-sound",
        ]
    )

    assert args.verify_command == "plan"
    assert args.base == "origin/main"
    assert args.head == "topic"
    assert args.include_partition == ["h2-sound"]


@pytest.mark.parametrize(
    ("arguments", "option"),
    (
        (["--full"], "--full"),
        (["--quick"], "--quick"),
        (["--skip-rebuild"], "--skip-rebuild"),
        (["--skip-extraction"], "--skip-extraction"),
        (["--skip-runtime"], "--skip-runtime"),
        (["--rom-path", "elsewhere.bin"], "--rom-path"),
        (["--upstream-path", "elsewhere-upstream"], "--upstream-path"),
    ),
)
def test_verify_plan_dispatch_rejects_execution_modifiers_before_planning(
    monkeypatch: pytest.MonkeyPatch,
    capsys: pytest.CaptureFixture[str],
    arguments: list[str],
    option: str,
) -> None:
    def must_not_plan(*_args: object, **_kwargs: object) -> dict[str, object]:
        raise AssertionError("planner ran before incompatible option rejection")

    monkeypatch.setattr(cli, "build_verification_plan", must_not_plan)

    assert cli.main(["verify", *arguments, "plan", "--base", "origin/main"]) == 1
    assert option in capsys.readouterr().err


def test_verify_and_full_dispatch_keep_existing_execution_semantics(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    calls: list[dict[str, object]] = []
    monkeypatch.setattr(cli, "verify", lambda **kwargs: calls.append(kwargs))
    parser = build_parser()

    cli.dispatch(parser.parse_args(["verify"]))
    cli.dispatch(parser.parse_args(["verify", "--full"]))

    assert [call["full"] for call in calls] == [False, True]


def test_verify_plan_dispatch_still_builds_and_prints_read_only_plan(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    built: list[tuple[str, str, tuple[str, ...]]] = []
    printed: list[object] = []

    def fake_plan(
        base: str, head: str, *, include_partitions: tuple[str, ...]
    ) -> dict[str, object]:
        built.append((base, head, include_partitions))
        return {"mode": "read-only-plan"}

    monkeypatch.setattr(cli, "build_verification_plan", fake_plan)
    monkeypatch.setattr(cli, "print_json", printed.append)

    cli.dispatch(
        build_parser().parse_args(
            [
                "verify",
                "plan",
                "--base",
                "origin/main",
                "--head",
                "HEAD",
                "--include-partition",
                "h2-sound",
            ]
        )
    )

    assert built == [("origin/main", "HEAD", ("h2-sound",))]
    assert printed == [{"mode": "read-only-plan"}]


def test_h2_manifest_selects_its_declared_owner_command() -> None:
    plan = plan_paths(("manifests/extractions/map-events-static.json",), root=ROOT)

    assert _partition_ids(plan) == {"public-core", "h2-map-scripting"}
    assert _partition(plan, "h2-map-scripting")["commands"] == [
        "uv run sf2 h2 map-event-direct-state",
        "uv run sf2 h2 map-events"
    ]
    assert plan["unclassifiedPaths"] == []


def test_map_event_direct_state_artifacts_select_only_the_direct_state_command() -> None:
    plan = plan_paths(
        (
            "src/sf2tool/h2/map_event_direct_state.py",
            "schemas/h2/map-event-direct-state-static-fixture.schema.json",
            "tests/fixtures/h2/map-event-direct-state-static-v1.json",
        ),
        root=ROOT,
    )

    assert _partition_ids(plan) == {"public-core", "h2-map-scripting"}
    assert _partition(plan, "h2-map-scripting")["commands"] == [
        "uv run sf2 h2 map-event-direct-state"
    ]
    assert plan["unclassifiedPaths"] == []


def test_field_menu_control_artifacts_select_only_the_services_state_command() -> None:
    plan = plan_paths(
        (
            "src/sf2tool/h2/field_menu_control.py",
            "schemas/h2/field-menu-control-static-fixture.schema.json",
            "tests/fixtures/h2/field-menu-control-static-v1.json",
        ),
        root=ROOT,
    )

    assert _partition_ids(plan) == {"public-core", "h2-services-state"}
    assert _partition(plan, "h2-services-state")["commands"] == [
        "uv run sf2 h2 field-item-effects",
        "uv run sf2 h2 field-menu-control",
        "uv run sf2 h2 field-search-control",
    ]
    assert plan["unclassifiedPaths"] == []


def test_field_item_effects_artifacts_select_only_the_services_state_command() -> None:
    plan = plan_paths(
        (
            "src/sf2tool/h2/field_item_effects.py",
            "schemas/h2/field-item-effects-static-fixture.schema.json",
            "tests/fixtures/h2/field-item-effects-static-v1.json",
        ),
        root=ROOT,
    )

    assert _partition_ids(plan) == {"public-core", "h2-services-state"}
    assert _partition(plan, "h2-services-state")["commands"] == ["uv run sf2 h2 field-item-effects"]
    assert plan["unclassifiedPaths"] == []


def test_field_search_control_artifacts_select_only_their_bounded_command() -> None:
    plan = plan_paths(
        (
            "src/sf2tool/h2/field_search_control.py",
            "schemas/h2/field-search-control-static-fixture.schema.json",
            "tests/fixtures/h2/field-search-control-static-v1.json",
        ),
        root=ROOT,
    )

    assert _partition_ids(plan) == {"public-core", "h2-services-state"}
    assert _partition(plan, "h2-services-state")["commands"] == [
        "uv run sf2 h2 field-search-control"
    ]
    assert plan["unclassifiedPaths"] == []


def test_map3_optional_interaction_artifacts_select_only_their_bounded_command() -> None:
    plan = plan_paths(
        (
            "tests/fixtures/h2/map3-optional-interactions-static-v1.json",
            "schemas/h2/map3-optional-interactions-static-fixture.schema.json",
        ),
        root=ROOT,
    )

    assert _partition_ids(plan) == {"public-core", "h2-map-scripting"}
    assert _partition(plan, "h2-map-scripting")["commands"] == [
        "uv run sf2 h2 map3-optional-interactions"
    ]
    assert plan["unclassifiedPaths"] == []


def test_map3_castle_static_artifacts_select_only_their_bounded_command() -> None:
    plan = plan_paths(
        (
            "tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json",
            "schemas/h2/map3-castle-battle-unlock-static-fixture.schema.json",
        ),
        root=ROOT,
    )

    assert _partition_ids(plan) == {"public-core", "h2-map-scripting"}
    assert _partition(plan, "h2-map-scripting")["commands"] == [
        "uv run sf2 h2 map3-castle-battle-unlock"
    ]
    assert plan["unclassifiedPaths"] == []


def test_map3_battle01_admission_artifacts_select_only_their_bounded_command() -> None:
    assert not hasattr(verification_plan, "H2_DEPENDENCY_ONLY_ARTIFACTS")
    plan = plan_paths(
        (
            "tests/fixtures/h2/map3-battle01-admission-static-v1.json",
            "schemas/h2/map3-battle01-admission-static-fixture.schema.json",
        ),
        root=ROOT,
    )

    assert _partition_ids(plan) == {"public-core", "h2-map-scripting"}
    assert _partition(plan, "h2-map-scripting")["commands"] == [
        "uv run sf2 h2 map3-battle01-admission"
    ]
    assert plan["unclassifiedPaths"] == []


def test_map3_battle01_turn_control_artifacts_select_only_their_bounded_command() -> None:
    plan = plan_paths(
        (
            "tests/fixtures/h2/map3-battle01-turn-control-static-v1.json",
            "schemas/h2/map3-battle01-turn-control-static-fixture.schema.json",
        ),
        root=ROOT,
    )

    assert _partition_ids(plan) == {"public-core", "h2-battle-logic"}
    assert _partition(plan, "h2-battle-logic")["commands"] == [
        "uv run sf2 h2 map3-battle01-turn-control"
    ]
    assert plan["unclassifiedPaths"] == []


def test_map3_battle01_action_effect_artifacts_select_only_h2_without_h3() -> None:
    plan = plan_paths(
        (
            "src/sf2tool/h2/map3_battle01_action_effect.py",
            "tests/fixtures/h2/map3-battle01-action-effect-static-v1.json",
            "schemas/h2/map3-battle01-action-effect-static-fixture.schema.json",
        ),
        root=ROOT,
    )

    assert _partition_ids(plan) == {"public-core", "h2-battle-logic"}
    assert _partition(plan, "h2-battle-logic")["commands"] == [
        "uv run sf2 h2 map3-battle01-action-effect"
    ]
    assert not any(partition_id.startswith("h3-") for partition_id in _partition_ids(plan))
    assert plan["unclassifiedPaths"] == []


def test_map3_battle01_action_completion_artifacts_select_only_h2_without_h3() -> None:
    plan = plan_paths(
        (
            "src/sf2tool/h2/map3_battle01_action_completion.py",
            "tests/fixtures/h2/map3-battle01-action-completion-static-v1.json",
            "schemas/h2/map3-battle01-action-completion-static-fixture.schema.json",
        ),
        root=ROOT,
    )

    assert _partition_ids(plan) == {"public-core", "h2-battle-logic"}
    assert _partition(plan, "h2-battle-logic")["commands"] == [
        "uv run sf2 h2 map3-battle01-action-completion"
    ]
    assert not any(partition_id.startswith("h3-") for partition_id in _partition_ids(plan))
    assert plan["unclassifiedPaths"] == []


def test_map3_battle01_turn_finalization_artifacts_select_only_h2_without_h3() -> None:
    plan = plan_paths(
        (
            "src/sf2tool/h2/map3_battle01_turn_finalization.py",
            "tests/fixtures/h2/map3-battle01-turn-finalization-static-v1.json",
            "schemas/h2/map3-battle01-turn-finalization-static-fixture.schema.json",
        ),
        root=ROOT,
    )
    assert _partition_ids(plan) == {"public-core", "h2-battle-logic"}
    assert _partition(plan, "h2-battle-logic")["commands"] == [
        "uv run sf2 h2 map3-battle01-turn-finalization"
    ]
    assert not any(partition_id.startswith("h3-") for partition_id in _partition_ids(plan))
    assert plan["unclassifiedPaths"] == []


def test_map3_battle01_victory_return_artifacts_select_only_h2_without_h3() -> None:
    plan = plan_paths(
        (
            "src/sf2tool/h2/map3_battle01_victory_return.py",
            "tests/fixtures/h2/map3-battle01-victory-return-static-v1.json",
            "schemas/h2/map3-battle01-victory-return-static-fixture.schema.json",
        ),
        root=ROOT,
    )
    assert _partition_ids(plan) == {"public-core", "h2-battle-logic"}
    assert _partition(plan, "h2-battle-logic")["commands"] == [
        "uv run sf2 h2 map3-battle01-victory-return"
    ]
    assert not any(partition_id.startswith("h3-") for partition_id in _partition_ids(plan))
    assert plan["unclassifiedPaths"] == []


def test_every_tracked_h2_h3_artifact_has_closed_exact_ownership() -> None:
    h2_owners = h2_artifact_commands(ROOT)
    h3_owners = h3_artifact_commands(ROOT)
    h2_surface = _tracked_h2_artifacts()
    h3_surface = _tracked_h3_artifacts()

    assert h2_surface <= h2_owners.keys() | H2_SHARED_ARTIFACT_PARTITIONS.keys()
    assert h3_surface <= h3_owners.keys() | H3_SHARED_ARTIFACT_PARTITIONS.keys()

    for path in sorted(h2_surface | h3_surface):
        plan = plan_paths((path,), root=ROOT)
        expected = _expected_artifact_commands(path, h2_owners, h3_owners)
        actual = {
            row["id"]: set(row["commands"])  # type: ignore[index, union-attr]
            for row in plan["partitions"]  # type: ignore[union-attr]
        }
        assert actual == expected, path
        assert plan["unclassifiedPaths"] == [], path


def test_h3_module_with_two_dispatches_selects_both_commands() -> None:
    plan = plan_paths(("src/sf2tool/h3/spell_damage.py",), root=ROOT)

    assert _partition_ids(plan) == {"public-core", "h3-battle01"}
    assert _partition(plan, "h3-battle01")["commands"] == [
        "uv run sf2 h3 spell-damage",
        "uv run sf2 h3 spell-summon",
    ]


def test_h3_observer_selects_its_exact_command() -> None:
    plan = plan_paths(
        ("tools/bizhawk/map_script_entity_presentation_fx_observer.lua",),
        root=ROOT,
    )

    assert _partition_ids(plan) == {"public-core", "h3-map-debug"}
    assert _partition(plan, "h3-map-debug")["commands"] == [
        "uv run sf2 h3 map-script-entity-presentation-fx"
    ]


def test_map3_messenger_artifacts_select_only_their_bounded_command() -> None:
    plan = plan_paths(
        ("src/sf2tool/h3/map3_messenger_acceptance.py",),
        root=ROOT,
    )

    assert _partition_ids(plan) == {"public-core", "h3-witch"}
    assert _partition(plan, "h3-witch")["commands"] == [
        "uv run sf2 h3 map3-messenger-acceptance"
    ]
    assert plan["unclassifiedPaths"] == []


def test_shared_python_module_uses_transitive_reverse_dependencies() -> None:
    plan = plan_paths(("src/sf2tool/compression.py",), root=ROOT)

    assert _partition_ids(plan) == {
        "public-core",
        "tooling-python",
        "h2-presentation",
    }
    commands = _partition(plan, "h2-presentation")["commands"]
    assert "uv run sf2 h2 map-tilesets" in commands  # type: ignore[operator]
    assert "uv run sf2 h2 battle-terrain" in commands  # type: ignore[operator]


def test_shared_cli_fans_out_to_every_evidence_partition() -> None:
    plan = plan_paths(("src/sf2tool/cli.py",), root=ROOT)

    assert _partition_ids(plan) == {
        "public-core",
        "tooling-python",
        *EVIDENCE_PARTITION_IDS,
    }


def test_legacy_launcher_transitively_selects_h1_rebuild() -> None:
    plan = plan_paths(("src/sf2tool/legacy.py",), root=ROOT)

    assert "h1-original" in _partition_ids(plan)


@pytest.mark.parametrize(
    "path", ("manifests/roms/sf2-us.json", "manifests/toolchain.json")
)
def test_shared_identity_manifest_selects_every_evidence_partition(path: str) -> None:
    plan = plan_paths((path,), root=ROOT)

    assert _partition_ids(plan) == {"public-core", *EVIDENCE_PARTITION_IDS}
    assert plan["unclassifiedPaths"] == []


def test_unknown_h2_module_fails_conservatively_to_all_h2_partitions() -> None:
    path = "src/sf2tool/h2/future_shared_helper.py"
    plan = plan_paths((path,), root=ROOT)

    assert _partition_ids(plan) == {"public-core", *H2_PARTITION_IDS}
    assert plan["unclassifiedPaths"] == [path]


def test_docs_only_plan_keeps_only_always_run_public_core() -> None:
    plan = plan_paths(("docs/research/example.md",), root=ROOT)

    assert _partition_ids(plan) == {"public-core"}
    assert plan["unclassifiedPaths"] == []
    assert _partition(plan, "public-core")["externalGates"] == [
        "GitHub Public / tracked-inputs"
    ]


def test_aggregate_indexes_are_owned_by_the_always_run_public_core() -> None:
    plan = plan_paths(
        ("manifests/research-index.json", "manifests/zh-translation-index.json"),
        root=ROOT,
    )

    assert _partition_ids(plan) == {"public-core"}
    assert plan["unclassifiedPaths"] == []


def test_deleted_python_test_falls_back_to_complete_python_suite() -> None:
    plan = plan_paths(("tests/python/test_deleted.py",), root=ROOT)

    assert _partition(plan, "tooling-python")["commands"] == ["uv run pytest"]


def test_explicit_partition_is_included_and_unknown_id_is_rejected() -> None:
    plan = plan_paths((), root=ROOT, include_partitions=("h2-sound",))

    assert _partition_ids(plan) == {"public-core", "h2-sound"}
    assert _partition(plan, "h2-sound")["reasons"] == ["explicit --include-partition"]
    with pytest.raises(ValueError, match="unknown verification partition"):
        plan_paths((), root=ROOT, include_partitions=("missing",))


def test_git_range_plan_is_read_only_and_resolves_exact_commits(tmp_path: Path) -> None:
    root = tmp_path / "repo"
    base = _initialize_git_repo(root)
    test_path = root / "tests" / "python" / "test_example.py"
    test_path.parent.mkdir(parents=True)
    test_path.write_text("def test_example():\n    assert True\n", encoding="utf-8")
    _git(root, "add", "tests/python/test_example.py")
    _git(root, "commit", "-m", "head")
    head = _git(root, "rev-parse", "HEAD")
    status_before = _git(root, "status", "--short")
    index_before = (root / ".git" / "index").read_bytes()

    plan = build_verification_plan(base, "HEAD", root=root)
    index_after = (root / ".git" / "index").read_bytes()

    assert plan["base"] == base
    assert plan["head"] == head
    assert plan["mergeBase"] == base
    assert plan["changedPaths"] == ["tests/python/test_example.py"]
    assert _partition_ids(plan) == {"public-core", "tooling-python"}
    assert _partition(plan, "tooling-python")["commands"] == [
        "uv run pytest tests/python/test_example.py"
    ]
    assert plan["executionSemanticsChanged"] is False
    assert index_after == index_before
    assert _git(root, "status", "--short") == status_before == ""


def test_git_range_plan_rejects_a_committed_head_other_than_checked_out_head(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    root = tmp_path / "repo"
    base = _initialize_git_repo(root)
    (root / "README.md").write_text("head\n", encoding="utf-8")
    _git(root, "add", "README.md")
    _git(root, "commit", "-m", "head")
    checked_out_head = _git(root, "rev-parse", "HEAD")

    monkeypatch.setattr(
        verification_plan,
        "plan_paths",
        lambda *_args, **_kwargs: pytest.fail("classification must not run"),
    )

    with pytest.raises(
        ValueError,
        match=rf"must resolve to the checked-out HEAD commit \({checked_out_head}\); got {base}",
    ):
        build_verification_plan(base, base, root=root)


@pytest.mark.parametrize(
    ("path", "tracked"),
    (
        ("src/sf2tool/owner.py", True),
        ("tests/python/test_untracked_owner.py", False),
    ),
)
def test_git_range_plan_rejects_dirty_or_untracked_ownership_inputs_before_classification(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    path: str,
    tracked: bool,
) -> None:
    root = tmp_path / "repo"
    base = _initialize_git_repo(root)
    candidate = root / path
    candidate.parent.mkdir(parents=True)
    candidate.write_text("original = True\n", encoding="utf-8")
    if tracked:
        _git(root, "add", path)
        _git(root, "commit", "-m", "add ownership input")
        candidate.write_text("dirty = True\n", encoding="utf-8")

    monkeypatch.setattr(
        verification_plan,
        "plan_paths",
        lambda *_args, **_kwargs: pytest.fail("classification must not run"),
    )

    with pytest.raises(
        ValueError,
        match=rf"requires a clean analyzed worktree before classification; .*{path}",
    ):
        build_verification_plan(base, "HEAD", root=root)
