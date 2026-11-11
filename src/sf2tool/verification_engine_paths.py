"""Explicit engine/shared path policy for the existing verification planner."""

REMAKE_PRODUCER_PATHS = frozenset(
    {
        "src/sf2tool/remake_exploration_content.py",
        "src/sf2tool/remake_entry_content.py",
        "src/sf2tool/remake_localization.py",
        "src/sf2tool/remake_battle_scene_content.py",
        "src/sf2tool/remake_authored_audio.py",
        "src/sf2tool/remake_command_menu.py",
        "src/sf2tool/remake_identity_icons.py",
        "src/sf2tool/remake_hd_identity_icons.py",
        "src/sf2tool/remake_exploration_battles.py",
        "src/sf2tool/remake_exploration_actions.py",
        "src/sf2tool/remake_exploration_joins.py",
        "src/sf2tool/remake_exploration_maps.py",
        "src/sf2tool/remake_exploration_search.py",
        "src/sf2tool/remake_exploration_services.py",
        "src/sf2tool/remake_exploration_programs.py",
        "src/sf2tool/remake_exploration_scene.py",
        "src/sf2tool/remake_exploration_view.py",
        "src/sf2tool/remake_exploration_visuals.py",
    }
)

ENGINE_WIRING_PATHS = (
    frozenset(
        {
            "src/sf2tool/cli.py",
            "src/sf2tool/harness.py",
            "src/sf2tool/verification_plan.py",
            "src/sf2tool/verification_engine_paths.py",
            "src/sf2tool/verification_partitions.py",
            "remake/reference/inputs/map3-programs.json",
        }
    )
    | REMAKE_PRODUCER_PATHS
)
ENGINE_EVIDENCE_INPUTS = frozenset(
    {
        "tests/fixtures/h3/battlefield-movement-matrix-v1.json",
        "tests/fixtures/h2/map3-battle01-admission-static-v1.json",
        "tests/fixtures/h2/map3-battle01-victory-return-static-v1.json",
        "tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json",
        "tests/fixtures/h3/battle-exp-level-up-v1.json",
        "tests/fixtures/h3/entity-movement-matrix-v1.json",
        "tests/fixtures/h3/map3-battle01-player-ready-v1.json",
        "tests/fixtures/h3/map3-admitted-start-v1.json",
        "tests/fixtures/h3/map3-battle01-natural-route-v1.json",
        "tests/fixtures/h3/map3-messenger-acceptance-v1.json",
    }
)
RETIRED_ENGINE_TEST_PATHS = frozenset(
    {
        "tests/python/test_dotnet_environment.py",
        "tests/python/test_godot_ai_probe.py",
        "tests/python/test_remake_architecture.py",
        "tests/python/test_remake_godot.py",
        "tests/python/test_verification_plan.py",
    }
)


def _engine_or_document_path(path: str) -> bool:
    if path.endswith(".md"):
        return not path.startswith(("docs/research/", "docs/design/contracts/"))
    return (
        path.startswith("remake/")
        or path in RETIRED_ENGINE_TEST_PATHS
        or path == ".github/workflows/public-checks.yml"
    )


def engine_partitions_for_path(path: str) -> tuple[str, ...]:
    """Product/tool consumers; private admission executes only by explicit local selection."""
    wiring = path in ENGINE_WIRING_PATHS or path == ".github/workflows/public-checks.yml"
    comparison = (
        ("h4-comparison-build",)
        if path in {
            "src/sf2tool/verification_plan.py",
            "src/sf2tool/verification_engine_paths.py",
            "src/sf2tool/verification_partitions.py",
            ".github/workflows/public-checks.yml",
        }
        else ()
    )
    if path.endswith(".md") or path in RETIRED_ENGINE_TEST_PATHS:
        return ()
    if wiring:
        return comparison + (
            "engine-unit", "adapter-build", "research-public", "private-admission-build",
            "live-inspection-build",
        )
    if path.startswith("remake/tools/Sf2.Remake.PrivateAdmission/"):
        return ("private-admission-build",)
    if path.startswith("remake/tools/Sf2.Remake.LiveInspection/"):
        return ("live-inspection-build",)
    if path.startswith("remake/tests/Sf2.Remake.Engine.Tests/"):
        return ("engine-unit",)
    if path.startswith("remake/tests/"):
        return ()  # Retired projects do not run the old solution.
    if path.startswith("remake/reference/inputs/"):
        return ("engine-unit",)
    if path.startswith("remake/game/"):
        return ("adapter-build",)
    return ("engine-unit", "adapter-build", "private-admission-build", "live-inspection-build")
