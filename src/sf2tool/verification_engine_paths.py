"""Explicit engine/shared path policy for the existing verification planner."""

REMAKE_PRODUCER_PATHS = frozenset(
    {
        "src/sf2tool/remake_exploration_content.py",
        "src/sf2tool/remake_battle_scene_content.py",
        "src/sf2tool/remake_authored_audio.py",
        "src/sf2tool/remake_command_menu.py",
        "src/sf2tool/remake_exploration_battles.py",
        "src/sf2tool/remake_exploration_joins.py",
        "src/sf2tool/remake_exploration_maps.py",
        "src/sf2tool/remake_exploration_programs.py",
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
