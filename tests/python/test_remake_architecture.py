from __future__ import annotations

import json
import re
import subprocess
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


def test_public_and_private_input_polling_share_one_internal_godot_adapter() -> None:
    root_source = (REMAKE / "game/src/Map3Root.cs").read_text(encoding="utf-8")
    adapter_source = (REMAKE / "game/src/Map3InputAdapter.cs").read_text(
        encoding="utf-8"
    )
    private_source = (REMAKE / "game/src/PrivateMap3Composition.cs").read_text(
        encoding="utf-8"
    )

    assert "Map3InputAdapter.CreateGodot" in root_source
    assert "PollPublicSynthetic" in root_source
    assert "PollPrivateOriginalMapMovement" in root_source
    assert "Input.IsActionJustPressed" not in root_source
    assert "InputMap." not in root_source
    assert "RegisterInputMap" not in root_source

    assert "internal sealed class Map3InputAdapter" in adapter_source
    assert "internal sealed record Map3InputActions" in adapter_source
    assert "internal sealed record Map3InputBinding" in adapter_source
    assert "Map3InputIntent" not in adapter_source
    assert "internal void PollPublicSynthetic()" in adapter_source
    assert "internal ExplorationDirection? PollPrivateOriginalMapMovement()" in (
        adapter_source
    )
    assert "public " not in adapter_source
    # Exact action names, keys, and ordering remain owned by Map3InputAdapterTests.
    # This characterization closes only the internal dispatch count and authority boundary.
    assert adapter_source.count("static actions =>") == 31
    for forbidden in (
        "GameSession",
        "GameFlowStage",
        "TacticalBattleReducer",
        "TacticalBattleState",
        "PublicSyntheticMap3PackageReader",
        "PrivateCanonicalMap3ImportReader",
        "SmokeMarker",
        "SF2_MAP3_SMOKE",
    ):
        assert forbidden not in adapter_source

    assert "Input.IsActionJustPressed" not in private_source
    assert "ProcessPrivateInput" not in private_source
    assert "ProcessPrivateInput" not in root_source
    assert "ApplyPrivateMove(privateMovement);" in root_source
    assert "_session.ApplyPrivateOriginalMap(" in private_source


def test_public_synthetic_smoke_is_an_internal_godot_driver() -> None:
    root_source = (REMAKE / "game/src/Map3Root.cs").read_text(encoding="utf-8")
    driver_source = (
        REMAKE / "game/src/PublicSyntheticMap3SmokeDriver.cs"
    ).read_text(encoding="utf-8")
    battle_presenter_source = (
        REMAKE / "game/src/PublicSyntheticBattlePresenter.cs"
    ).read_text(encoding="utf-8")
    private_source = (REMAKE / "game/src/PrivateMap3Composition.cs").read_text(
        encoding="utf-8"
    )

    assert "_admissionReceipt" not in root_source
    assert "RunHeadlessSmoke" not in root_source
    assert "PublicSyntheticMap3SmokeDriver.Run(" in root_source
    assert "SceneTree sceneTree = GetTree();" in root_source
    assert "GameSession.Start(" in root_source
    assert "PublicSyntheticMap3PackageReader.FromDocumentBytes" in root_source
    assert "Godot.FileAccess.GetFileAsBytes" in root_source
    assert "private void FailStartup(string message)" in root_source
    assert "private void ProjectSnapshot(" in root_source
    assert "string outcome," in root_source
    assert "GameSessionCommandResult? result = null" in root_source
    assert "_battlePresenter?.Project(_session.Snapshot, outcome, result);" in root_source
    assert "TacticalBattleState" not in root_source
    assert "PublicSyntheticBattleLifecycleSnapshot?" not in root_source
    assert "GameSessionSnapshot snapshot" in battle_presenter_source
    assert "GameSessionCommandResult? result = null" in battle_presenter_source
    assert "private readonly GameSessionSnapshot" not in battle_presenter_source
    assert "private GameSessionSnapshot" not in battle_presenter_source
    assert "TacticalBattleState?" not in battle_presenter_source

    for moved in (
        "The bounded synthetic movement command did not move.",
        '"synthetic-map3-east-zone-selected"',
        "Public-synthetic outbound shell admitted",
    ):
        assert moved not in root_source
        assert moved in driver_source

    assert "internal static class PublicSyntheticMap3SmokeDriver" in driver_source
    for dependency in (
        "SceneTree sceneTree",
        "GameSession session",
        "ScenarioAdmissionReceipt admissionReceipt",
        "Map3Presenter presenter",
        "PublicSyntheticBattlePresenter battlePresenter",
    ):
        assert dependency in driver_source
    for retained in (
        "session.Apply(",
        "presenter.Project(session.Snapshot,",
        "presenter.ProjectStatus(message);",
        "JsonSerializer.Serialize",
        "Map3Root.SmokeMarker",
        "Map3Root.BannerText",
        "sceneTree.Quit(0);",
        "sceneTree.Quit(1);",
    ):
        assert retained in driver_source
    assert "public " not in driver_source
    assert driver_source.index("Map3Root.SmokeMarker + legacyReceipt") < driver_source.index(
        "Map3Root.PublicSyntheticBattleSmokeMarker"
    )

    for forbidden in (
        "Map3Root owner",
        "Map3RuntimeProfileSelection",
        "OS.GetCmdlineUserArgs",
        "FileAccess",
        "PrivateCanonicalMap3ImportReader",
        "PrivateSmokeMarker",
        "SF2_MAP3_PRIVATE",
        "CanonicalImportPath",
        "Action<",
        "Func<",
        "interface ",
    ):
        assert forbidden not in driver_source

    assert "RunPrivateHeadlessSmoke" in private_source
    assert "SF2_MAP3_PRIVATE_LOCAL_SMOKE" in private_source


def test_private_local_smoke_is_an_internal_godot_driver() -> None:
    private_source = (REMAKE / "game/src/PrivateMap3Composition.cs").read_text(
        encoding="utf-8"
    )
    driver_source = (REMAKE / "game/src/PrivateMap3SmokeDriver.cs").read_text(
        encoding="utf-8"
    )

    for retained in (
        "Map3RuntimeProfileSelection",
        "canonicalImportPath",
        "PrivateCanonicalMap3ImportReader",
        "GameSession.StartPrivateOriginalMap",
        "TimedOriginalMapImportSource",
        "Callable.From(() => RunPrivateHeadlessSmoke(",
        "TracePrivateStage(",
        "private void ApplyPrivateMove(ExplorationDirection direction)",
        "PrivateMap3SmokeDriver.Run(GetTree(), session, presenter, smokeStarted);",
    ):
        assert retained in private_source

    for moved in (
        "No bounded semantic movement was admitted from the controlled start.",
        "The admitted private definition has no controlled step-copy record.",
        "Controlled step-copy diagnostic applied",
    ):
        assert moved not in private_source
        assert moved in driver_source

    assert "internal static class PrivateMap3SmokeDriver" in driver_source
    for dependency in (
        "SceneTree sceneTree",
        "GameSession session",
        "PrivateMap3Presenter presenter",
        "long smokeStarted",
    ):
        assert dependency in driver_source
    for retained in (
        "session.ApplyPrivateOriginalMap(",
        "session.ApplyPrivateOriginalMapLayoutMutation(",
        "presenter.Project(",
        "presenter.ProjectStatus(message);",
        "presenter.Projection",
        "JsonSerializer.Serialize",
        "Map3Root.TracePrivateStage(",
        "sceneTree.Quit(0);",
        "sceneTree.Quit(1);",
    ):
        assert retained in driver_source
    assert "public " not in driver_source

    marker_order = (
        "Map3Root.PrivateSmokeMarker",
        "Map3Root.PrivateViewSmokeMarker",
        "Map3Root.PrivateStepCopySmokeMarker",
        "Map3Root.PrivateAreaSmokeMarker",
    )
    offsets = [driver_source.index(marker) for marker in marker_order]
    assert offsets == sorted(offsets)

    for forbidden in (
        "Map3Root owner",
        "Map3RuntimeProfileSelection",
        "OS.GetCmdlineUserArgs",
        "FileAccess",
        "PrivateCanonicalMap3ImportReader",
        "canonicalImportPath",
        "IOriginalMapImportSource",
        "TimedOriginalMapImportSource",
        "StartPrivateScenario",
        "GameSession.StartPrivateOriginalMap",
        "private static void TracePrivateStage",
        "PrivateStageMarker",
        "ApplyPrivateMove",
        "Action<",
        "Func<",
        "interface ",
        " event ",
    ):
        assert forbidden not in driver_source


def test_public_synthetic_presentation_is_an_internal_godot_adapter() -> None:
    root_source = (REMAKE / "game/src/Map3Root.cs").read_text(encoding="utf-8")
    presenter_source = (REMAKE / "game/src/Map3Presenter.cs").read_text(
        encoding="utf-8"
    )
    private_source = (REMAKE / "game/src/PrivateMap3Composition.cs").read_text(
        encoding="utf-8"
    )

    assert "private Map3Presenter? _presenter;" in root_source
    assert "_presenter = Map3Presenter.Attach(this);" in root_source
    assert "_presenter?.Project(_session.Snapshot, outcome);" in root_source
    assert "_presenter?.ProjectStatus" in root_source
    assert "new Label" not in root_source
    assert "new SyntheticMapViewport" not in root_source
    assert "_status.Text" not in root_source
    for formatter in (
        "FormatContext",
        "FormatEventRequest",
        "FormatEffect",
        "FormatLocalTransition",
        "FormatEntities",
        "FormatEntityInteraction",
        "FormatDialogue",
        "FormatFieldSearch",
        "FormatItemAcquisition",
        "FormatOutboundTransition",
    ):
        assert formatter not in root_source

    assert "internal sealed class Map3Presenter" in presenter_source
    assert "internal sealed record Map3PresentationProjection" in presenter_source
    assert "Map3PresentationProjection.Create(snapshot, outcome)" in presenter_source
    assert "public " not in presenter_source
    for forbidden in (
        ".Apply(",
        "PublicSyntheticMap3PackageReader",
        "PrivateCanonicalMap3ImportReader",
        "Map3RuntimeProfileSelection",
        "JsonSerializer",
        "SmokeMarker",
        "SF2_MAP3_",
        "GetTree().Quit",
    ):
        assert forbidden not in presenter_source

    child_order = (
        "parent.AddChild(banner);",
        "parent.AddChild(explanation);",
        "parent.AddChild(viewport);",
        "parent.AddChild(status);",
        "parent.AddChild(contextStatus);",
        "parent.AddChild(eventRequestStatus);",
        "parent.AddChild(effectStatus);",
        "parent.AddChild(transitionStatus);",
        "parent.AddChild(entityStatus);",
        "parent.AddChild(entityInteractionStatus);",
        "parent.AddChild(dialogueStatus);",
        "parent.AddChild(fieldSearchStatus);",
        "parent.AddChild(itemAcquisitionStatus);",
        "parent.AddChild(outboundTransitionStatus);",
    )
    offsets = [presenter_source.index(token) for token in child_order]
    assert offsets == sorted(offsets)
    for layout_contract in (
        "internal static readonly Vector2 CanvasSize = new(960, 540);",
        "internal static readonly Rect2 MapBounds = new(16, 84, 576, 336);",
        "internal static readonly Rect2 ActionDeckBounds = new(608, 82, 336, 444);",
        "Position = MapBounds.Position",
        "Position = ActionDeckBounds.Position",
        "AutowrapMode = TextServer.AutowrapMode.WordSmart",
        "ClipText = true",
    ):
        assert layout_contract in presenter_source
    for projection_field in (
        "string ControlGuide",
        "string MapLegend",
        "_controls.Text = projection.ControlGuide;",
        "_mapLegend.Text = projection.MapLegend;",
    ):
        assert projection_field in presenter_source
    for obsolete_off_canvas_position in (
        "new Vector2(24, 540)",
        "new Vector2(24, 570)",
        "new Vector2(24, 600)",
        "new Vector2(24, 630)",
        "new Vector2(24, 660)",
        "new Vector2(24, 690)",
        "new Vector2(24, 720)",
        "new Vector2(24, 750)",
    ):
        assert obsolete_off_canvas_position not in presenter_source
    for color in ("ffbd59", "c6e5ff", "b8f2c2", "ffe2a8", "d8c6ff"):
        assert f'new Color("{color}")' in presenter_source

    assert "BuildPresentation();" in private_source
    assert "Map3PresentationProjection" not in private_source
    assert "_presenter" not in private_source


def test_private_diagnostic_presentation_is_an_internal_godot_adapter() -> None:
    root_source = (REMAKE / "game/src/Map3Root.cs").read_text(encoding="utf-8")
    private_source = (REMAKE / "game/src/PrivateMap3Composition.cs").read_text(
        encoding="utf-8"
    )
    presenter_source = (REMAKE / "game/src/PrivateMap3Presenter.cs").read_text(
        encoding="utf-8"
    )
    driver_source = (REMAKE / "game/src/PrivateMap3SmokeDriver.cs").read_text(
        encoding="utf-8"
    )

    assert "private Label? _status;" not in root_source
    assert "private PrivateMap3Presenter? _privatePresenter;" in private_source
    assert "_privatePresenter = PrivateMap3Presenter.Attach(this, plan);" in private_source
    assert "PrivateMap3PresentationPlan.PrivateLocalAvailable()" in private_source
    assert "PrivateMap3PresentationPlan.PrivateLocalUnavailable(" in private_source
    assert "PrivateMap3PresentationPlan.ProfileUnavailable(" in private_source
    assert "_privatePresenter?.Project(" in private_source
    assert "_privatePresenter?.ProjectStatus(message);" in private_source
    assert "_privatePresenter?.Projection" not in private_source
    assert "presenter.Projection" in driver_source
    assert "new Label" not in private_source
    assert "new PrivateOriginalMapTraversalViewport" not in private_source
    assert "_status" not in private_source
    assert "_privateTraversalViewport" not in private_source

    assert "internal sealed record PrivateMap3PresentationPlan" in presenter_source
    assert "internal sealed class PrivateMap3Presenter" in presenter_source
    assert presenter_source.count("internal sealed record ") == 1
    assert "internal static PrivateMap3PresentationPlan PrivateLocalAvailable()" in (
        presenter_source
    )
    assert "internal static PrivateMap3PresentationPlan PrivateLocalUnavailable(" in (
        presenter_source
    )
    assert "internal static PrivateMap3PresentationPlan ProfileUnavailable(" in (
        presenter_source
    )
    assert "internal PrivateOriginalMapTraversalViewProjection? Projection" in (
        presenter_source
    )
    assert "_viewport?.Project(snapshot);" in presenter_source
    assert "PrivateMap3PresentationPlan.FormatStatus(snapshot, outcome)" in (
        presenter_source
    )
    assert "public " not in presenter_source

    for forbidden in (
        ".Apply(",
        "Map3RuntimeProfileSelection",
        "runtime-profile",
        "CanonicalImportPath",
        "PrivateCanonicalMap3ImportReader",
        "ContentProfile",
        "JsonSerializer",
        "SmokeMarker",
        "SF2_MAP3_",
        "Stopwatch",
        "Input.",
        "InputMap.",
        "FileAccess",
        "GD.Print",
        "GetTree().Quit",
    ):
        assert forbidden not in presenter_source

    child_order = (
        "parent.AddChild(banner);",
        "parent.AddChild(explanation);",
        "parent.AddChild(viewport);",
        "parent.AddChild(status);",
    )
    offsets = [presenter_source.index(token) for token in child_order]
    assert offsets == sorted(offsets)
    for position in (
        "new Vector2(24, 18)",
        "new Vector2(24, 55)",
        "new Vector2(24, 105)",
        "new Vector2(24, plan.StatusY)",
    ):
        assert presenter_source.count(position) == 1
    assert presenter_source.count('new Color("ff8f70")') == 1
    assert "StatusY: 450" in presenter_source
    assert "StatusY: 105" in presenter_source

    for retained in (
        "PrivateCanonicalMap3ImportReader",
        "GameSession.StartPrivateOriginalMap",
        "JsonSerializer.Serialize",
        "SF2_MAP3_PRIVATE_LOCAL_SMOKE",
        "SF2_MAP3_PRIVATE_LOCAL_VIEW_SMOKE",
        "SF2_MAP3_PRIVATE_LOCAL_STEP_COPY_SMOKE",
        "SF2_MAP3_PRIVATE_LOCAL_AREA_SMOKE",
        "TracePrivateStage",
        "GetTree().Quit",
    ):
        assert retained in private_source

    assert "Input.IsActionJustPressed" not in private_source


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
            "tests/python/test_verification_plan.py "
            "tests/python/test_remake_assets.py"
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


def test_local_presentation_asset_pack_schema_and_reader_close_private_mount() -> None:
    schema_path = REMAKE / "schemas/local-presentation-asset-pack-v1.schema.json"
    schema = json.loads(schema_path.read_text(encoding="utf-8"))

    def assert_closed_objects(value: object) -> None:
        if isinstance(value, dict):
            if value.get("type") == "object":
                assert value.get("additionalProperties") is False
            for child in value.values():
                assert_closed_objects(child)
        elif isinstance(value, list):
            for child in value:
                assert_closed_objects(child)

    assert_closed_objects(schema)
    properties = schema["properties"]
    assert properties["schemaVersion"] == {"const": 1}
    assert properties["packageId"] == {
        "const": "sf2-local-presentation-asset-pack-v1"
    }
    assert properties["repositoryId"] == {"const": "md-sf2-remake-assets"}
    assert properties["profile"] == {"const": "private-local"}
    assert properties["logicalPresentation"]["properties"] == {
        "width": {"const": 960},
        "height": {"const": 540},
    }
    buckets = schema["$defs"]["rasterAsset"]["properties"]["buckets"]
    assert buckets["minItems"] == buckets["maxItems"] == 2
    assert [
        item["allOf"][1]["properties"]["scale"]["const"]
        for item in buckets["prefixItems"]
    ] == [2, 4]
    assert schema["$defs"]["bucket"]["properties"]["mediaType"] == {
        "const": "image/png"
    }
    semantic_id_pattern = "^[a-z0-9]+(?:[.-][a-z0-9]+)*$"
    assert schema["$defs"]["source"]["properties"]["assetId"]["pattern"] == (
        semantic_id_pattern
    )
    assert schema["$defs"]["rasterAsset"]["properties"]["assetId"][
        "pattern"
    ] == semantic_id_pattern
    assert schema["$defs"]["bucket"]["properties"]["byteLength"][
        "maximum"
    ] == 256 * 1024 * 1024
    assert schema["$defs"]["bucket"]["properties"]["filter"] == {
        "enum": ["nearest", "linear"]
    }
    assert schema["$defs"]["bucket"]["properties"]["colorSpace"] == {
        "const": "srgb"
    }
    assert schema["$defs"]["bucket"]["properties"]["alphaMode"] == {
        "const": "straight"
    }

    reader_source = (
        REMAKE / "src/Sf2.Remake.Content/LocalPresentationAssetPackReader.cs"
    ).read_text(encoding="utf-8")
    assert reader_source.count("public LocalPresentationAssetPackReader(") == 1
    assert "string assetRoot," in reader_source
    assert "string mountedAssetRepositoryCommit)" in reader_source
    assert "File.ReadAllBytes" not in reader_source
    assert reader_source.index("stream.Length != expectedLength") < reader_source.index(
        "SHA256.HashData(stream)"
    )
    assert "LocalPresentationAssetPackAdmission.IsCanonicalSemanticId(assetId)" in (
        reader_source
    )
    assert "LocalPresentationAssetPackAdmission.IsCanonicalSemanticId(sourceAssetId)" in (
        reader_source
    )

    admission_source = (
        REMAKE
        / "src/Sf2.Remake.Application/Content/LocalPresentationAssetPackAdmission.cs"
    ).read_text(encoding="utf-8")
    assert "public const int MaximumManifestBytes = 4 * 1024 * 1024;" in (
        admission_source
    )
    assert "public const long MaximumRasterPayloadBytes = 256L * 1024 * 1024;" in (
        admission_source
    )
    assert "public static bool IsCanonicalSemanticId(string? value)" in admission_source
    for forbidden in (
        "using Godot",
        "System.Diagnostics",
        "Process.Start",
        '"git"',
        '".git"',
        "FromDocumentBytes",
        "FromBytes",
    ):
        assert forbidden not in reader_source

    definition_source = (
        REMAKE
        / "src/Sf2.Remake.Application/Content/LocalPresentationAssetPackDefinition.cs"
    ).read_text(encoding="utf-8")
    for forbidden in (
        "System.IO",
        "System.Text.Json",
        "using Godot",
        "RuntimePath",
        "PayloadBytes",
        "FilePath",
    ):
        assert forbidden not in definition_source

    tracked = subprocess.run(
        ["git", "-C", str(ROOT), "ls-files"],
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    ).stdout.splitlines()
    product_media_extensions = {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".svg",
        ".bmp",
        ".gif",
        ".ase",
        ".aseprite",
        ".psd",
        ".kra",
        ".ogg",
        ".wav",
        ".mp3",
        ".flac",
        ".mp4",
        ".webm",
        ".ttf",
        ".otf",
        ".woff",
        ".woff2",
    }
    assert [
        path
        for path in tracked
        if Path(path).suffix.lower() in product_media_extensions
        and not path.startswith(("remake/tests/", "tests/fixtures/"))
    ] == []


def test_presentation_asset_preflight_is_path_free_local_only_and_atomic() -> None:
    schema_path = REMAKE / "schemas/local-presentation-asset-mount-v1.schema.json"
    schema = json.loads(schema_path.read_text(encoding="utf-8"))
    assert schema["additionalProperties"] is False
    assert set(schema["properties"]) == {
        "schemaVersion",
        "descriptorId",
        "packageId",
        "repositoryId",
        "profile",
        "packCapability",
        "preflightCapability",
        "mountKind",
        "assetRepositoryCommit",
        "assetRepositoryTree",
        "manifestSha256",
        "assetCount",
        "bucketCount",
        "bucketScales",
        "totalPayloadBytes",
        "contentSetSha256",
        "status",
    }
    serialized_schema = json.dumps(schema, sort_keys=True)
    for forbidden in (
        '"assetRoot"',
        '"branch"',
        '"destination"',
        '"runtimePath"',
        '"sourcePath"',
        '"masterPath"',
        '"payload"',
    ):
        assert forbidden not in serialized_schema

    source = (ROOT / "src/sf2tool/remake_assets.py").read_text(encoding="utf-8")
    for required in (
        '"status", "--porcelain=v2", "-z"',
        '"ls-files", "--stage", "-z"',
        '"ls-files", "-z", "--", "manifests", "runtime"',
        '"remote",',
        '"config", "--local", "--get", "core.hooksPath"',
        "shell=False",
        "timeout=timeout",
        "os.path.normcase(os.path.realpath(source_path))",
        "_is_within(destination, asset_root)",
        '"ExportStagingFailed"',
        '"ExportCleanupFailed"',
        "os.rename(temp, export_root)",
        "(temp / DESCRIPTOR_FILE_NAME).write_bytes(descriptor_bytes)",
    ):
        assert required in source
    assert source.index("(temp / DESCRIPTOR_FILE_NAME).write_bytes(descriptor_bytes)") < (
        source.index("os.rename(temp, export_root)")
    )
    for forbidden in (
        "Invoke-Expression",
        "shell=True",
        "md-sf2-gfx-remake",
        "PIL",
        "Godot",
        "source.copy",
        "masters.copy",
        "git fetch",
        "git gc",
        "git push",
    ):
        assert forbidden not in source
