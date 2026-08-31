from __future__ import annotations

import json
import re
import xml.etree.ElementTree as element_tree
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
REMAKE = ROOT / "remake"
PRODUCTION_PROJECTS = {
    "Sf2.Remake.Domain": REMAKE / "src/Sf2.Remake.Domain/Sf2.Remake.Domain.csproj",
    "Sf2.Remake.Application": REMAKE
    / "src/Sf2.Remake.Application/Sf2.Remake.Application.csproj",
    "Sf2.Remake.Content": REMAKE / "src/Sf2.Remake.Content/Sf2.Remake.Content.csproj",
    "Sf2.Remake.Godot": REMAKE / "game/Sf2.Remake.Godot.csproj",
}
EXPECTED_REFERENCES = {
    "Sf2.Remake.Domain": set(),
    "Sf2.Remake.Application": {"Sf2.Remake.Domain"},
    "Sf2.Remake.Content": {"Sf2.Remake.Application", "Sf2.Remake.Domain"},
    "Sf2.Remake.Godot": {
        "Sf2.Remake.Application",
        "Sf2.Remake.Content",
        "Sf2.Remake.Domain",
    },
}
PUBLIC_WORKFLOW = ROOT / ".github/workflows/public-checks.yml"


def _project_document(path: Path) -> element_tree.Element:
    return element_tree.parse(path).getroot()


def _property(document: element_tree.Element, name: str) -> str | None:
    element = document.find(f".//{name}")
    return None if element is None else element.text


def test_exact_four_production_assemblies_have_closed_inward_references() -> None:
    discovered = {
        path
        for root in (REMAKE / "src", REMAKE / "game")
        for path in root.rglob("*.csproj")
    }
    assert discovered == set(PRODUCTION_PROJECTS.values())

    for assembly, project in PRODUCTION_PROJECTS.items():
        document = _project_document(project)
        assert _property(document, "AssemblyName") == assembly
        references = {
            Path(element.attrib["Include"].replace("\\", "/")).stem
            for element in document.findall(".//ProjectReference")
        }
        assert references == EXPECTED_REFERENCES[assembly]
        assert document.findall(".//PackageReference") == []


def test_godot_project_and_lock_pin_accepted_4_7_2_dotnet_boundary() -> None:
    document = _project_document(PRODUCTION_PROJECTS["Sf2.Remake.Godot"])
    assert document.attrib["Sdk"] == "Godot.NET.Sdk/4.7.2"
    lock = json.loads((REMAKE / "game/packages.lock.json").read_text(encoding="utf-8"))
    dependencies = lock["dependencies"]["net8.0"]

    assert dependencies["Godot.SourceGenerators"]["resolved"] == "4.7.2"
    assert dependencies["GodotSharp"]["resolved"] == "4.7.2"
    assert dependencies["GodotSharpEditor"]["resolved"] == "4.7.2"


def test_public_synthetic_input_polling_is_an_internal_godot_adapter() -> None:
    root_source = (REMAKE / "game/src/Map3Root.cs").read_text(encoding="utf-8")
    adapter_source = (REMAKE / "game/src/Map3InputAdapter.cs").read_text(
        encoding="utf-8"
    )
    private_source = (REMAKE / "game/src/PrivateMap3Composition.cs").read_text(
        encoding="utf-8"
    )

    assert "Map3InputAdapter.CreateGodot" in root_source
    assert "PollPublicSynthetic" in root_source
    assert "Input.IsActionJustPressed" not in root_source
    assert "InputMap." not in root_source
    assert "RegisterInputMap" not in root_source

    assert "internal sealed class Map3InputAdapter" in adapter_source
    assert "internal sealed record Map3InputActions" in adapter_source
    assert "internal sealed record Map3InputBinding" in adapter_source
    assert "Map3InputIntent" not in adapter_source
    assert "internal void PollPublicSynthetic()" in adapter_source
    assert "public " not in adapter_source
    assert adapter_source.count("static actions =>") == 22
    for forbidden in (
        "GameSession",
        "PublicSyntheticMap3PackageReader",
        "PrivateCanonicalMap3ImportReader",
        "SmokeMarker",
        "SF2_MAP3_SMOKE",
    ):
        assert forbidden not in adapter_source

    assert 'Input.IsActionJustPressed("move_north")' in private_source
    assert "ProcessPrivateInput();" in root_source


def test_inner_assemblies_do_not_acquire_outer_or_nondeterministic_apis() -> None:
    forbidden = {
        "Sf2.Remake.Domain": (
            "using Godot",
            "System.IO",
            "System.Random",
            "System.Text.Json",
            "DateTime",
            "Stopwatch",
        ),
        "Sf2.Remake.Application": (
            "using Godot",
            "System.IO",
            "System.Random",
            "System.Text.Json",
            "DateTime",
            "Stopwatch",
        ),
    }
    for assembly, tokens in forbidden.items():
        source_root = PRODUCTION_PROJECTS[assembly].parent
        source = "\n".join(
            path.read_text(encoding="utf-8")
            for path in sorted(source_root.rglob("*.cs"))
            if not {"bin", "obj", ".godot"}.intersection(path.relative_to(source_root).parts)
        )
        for token in tokens:
            assert token not in source


def test_solution_hosts_four_production_projects_and_godot_export_host() -> None:
    root_solution = (REMAKE / "Sf2.Remake.sln").read_text(encoding="utf-8")
    for assembly in PRODUCTION_PROJECTS:
        assert f'= "{assembly}"' in root_solution

    godot_solution = (REMAKE / "game/Sf2.Remake.Godot.sln").read_text(encoding="utf-8")
    assert '= "Sf2.Remake.Godot", "Sf2.Remake.Godot.csproj"' in godot_solution
    assert godot_solution.count("\nProject(") == 1


def test_public_workflow_is_one_lightweight_tracked_input_job() -> None:
    workflow = PUBLIC_WORKFLOW.read_text(encoding="utf-8")
    jobs = workflow.split("\njobs:\n", maxsplit=1)
    assert len(jobs) == 2
    assert re.findall(r"(?m)^  ([a-z0-9-]+):$", jobs[1]) == ["tracked-inputs"]

    expected_run_commands = (
        "uv sync --locked",
        "uv run ruff check src tests/python",
        "uv run pytest tests/python/test_native_harness.py",
        (
            "uv run pytest tests/python/test_remake_architecture.py "
            "tests/python/test_verification_plan.py"
        ),
        "uv run sf2 design-contracts test",
        "dotnet restore Sf2.Remake.sln --locked-mode",
        "dotnet build Sf2.Remake.sln --configuration Release --no-restore",
        "dotnet test Sf2.Remake.sln --configuration Release --no-build --no-restore",
    )
    assert re.findall(r"(?m)^        run: (.+)$", jobs[1]) == list(
        expected_run_commands
    )
    for command in expected_run_commands:
        assert workflow.count(command) == 1

    assert "- name: Build remake solution" in workflow
    assert "- name: Test remake solution" in workflow
    forbidden_fragments = (
        "remake-godot:",
        "godotengine/godot-builds",
        "Download locked official Godot artifacts",
        "Godot_v4.7.2",
        "sf2tool.remake_godot",
        "Import, run, and export",
        "--toolchain-root",
        "--scratch-parent",
        "Build remake domain",
        "Test remake domain",
    )
    for fragment in forbidden_fragments:
        assert fragment not in workflow
