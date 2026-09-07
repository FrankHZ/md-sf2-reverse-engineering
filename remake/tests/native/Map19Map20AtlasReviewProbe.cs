// Compile only in an ignored copy of an exact committed Godot project. This is
// controlled seed/route/capture instrumentation, not a production entry point.
using System.Reflection;
using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

public partial class Map19Map20AtlasReviewProbe : Node2D
{
    private GameSession _session = null!;
    private Map3Root _root = null!;
    private PrivateMap3Presenter _presenter = null!;
    private readonly List<object> _frames = [];
    private string _output = null!;
    private JsonDocument _fixture = null!;

    public override async void _Ready()
    {
        try
        {
            _output = RequiredPath("SF2_CASTLE_REVIEW_OUTPUT");
            if (!Directory.Exists(_output) || Directory.EnumerateFileSystemEntries(_output).Any())
                throw new InvalidOperationException("Capture output must be an existing empty directory.");
            _fixture = JsonDocument.Parse(File.ReadAllText(RequiredPath("SF2_CASTLE_REVIEW_FIXTURE")));
            Map3Root root = GD.Load<PackedScene>("res://Main.tscn").Instantiate<Map3Root>();
            _root = root;
            AddChild(root);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            root.ProcessMode = ProcessModeEnum.Disabled;
            _session = Field<GameSession>(root, "_session");
            _presenter = Field<PrivateMap3Presenter>(root, "_privatePresenter");
            await Capture("01-map3", "map3", null);

            SeedMap19();
            await Capture("02-map19-entry", "map19", 0);
            DriveSegment("map19-entry-to-royal-warp", 38);
            Require(_session.PrivateOriginalMapSnapshot.PlayerPosition == new MapPosition(23, 37), "Royal arrival");
            await Capture("03-map20-royal", "map20", 0);
            Require(_session.RequestPrivateOriginalMapInteraction(_session.PrivateOriginalMapSnapshot.SimulationStep)
                is PrivateOriginalMapPalaceFirstVisitApplied, "Controlled palace F");
            Move(ExplorationDirection.North);
            Move(ExplorationDirection.North);
            Require(_session.PrivateOriginalMapSnapshot.Map == new MapId("map19"), "Royal return");
            DriveSegment("map19-royal-return-to-astral", 11);
            Require(Move(ExplorationDirection.North).Traversal.Outcome ==
                OriginalMapTraversalOutcome.BlockedByOccupiedEntity, "Astral occupancy");
            await Capture("04-map19-astral-waiting", "map19", 1);
            Require(_session.RequestPrivateOriginalMapInteraction(_session.PrivateOriginalMapSnapshot.SimulationStep)
                is PrivateOriginalMapAstralAcceptanceApplied, "Controlled Astral F");
            await Capture("05-map19-astral-cleared", "map19", 0);
            Move(ExplorationDirection.North);
            DriveSegment("map19-astral-to-west-tower-warp", 15, skip: 1);
            Require(_session.PrivateOriginalMapSnapshot.PlayerPosition == new MapPosition(6, 37), "West arrival");
            await Capture("06-map20-west", "map20", 0);
            Move(ExplorationDirection.West);
            Require(_session.PrivateOriginalMapSnapshot.PlayerPosition == new MapPosition(5, 37), "Middle route first Left");
            Move(ExplorationDirection.West);
            Require(_session.PrivateOriginalMapSnapshot.PlayerPosition == new MapPosition(4, 37), "Middle route second Left");
            Move(ExplorationDirection.West);
            await CaptureMiddleTowerDiagnostic();
            ApplyGuardF(expectApplied: false); // Actual composition rejection at (3,16).
            Move(ExplorationDirection.East);
            _presenter.Project(_session.PrivateOriginalMapSnapshot, "Controlled guard approach", _session.PrivateOriginalMapPlayerLocomotion);
            await CaptureGuard("08-map21-guard-before", new(4, 16), new(5, 16), completed: false);
            ApplyGuardF(expectApplied: true);
            await CaptureGuard("09-map21-guard-after", new(4, 16), new(6, 16), completed: true);
            ApplyGuardF(expectApplied: false); // Duplicate stays immutable and must not throw in composition.
            Move(ExplorationDirection.East);
            Require(_session.PrivateOriginalMapSnapshot.PlayerPosition == new MapPosition(5, 16), "Guard old occupancy released");
            Move(ExplorationDirection.North);
            Require(_session.PrivateOriginalMapPlayerLocomotion.OpaqueFacing == 1, "Ordinary Up owns final facing");
            _presenter.Project(_session.PrivateOriginalMapSnapshot, "Controlled guard walk endpoint", _session.PrivateOriginalMapPlayerLocomotion);
            await CaptureGuard("10-map21-guard-walk-endpoint", new(5, 15), new(6, 16), completed: true);

            File.WriteAllText(Path.Combine(_output, "receipt.json"), JsonSerializer.Serialize(new
            {
                status = "Pass",
                scope = "seeded-native-current-runtime-projection; natural-route/H4 Unknown",
                production = new[] { "Map3Root", "PrivateMap3Composition", "PrivateCanonicalMap3ImportReader",
                    "PrivateLocalPresentationAssetCatalog", "PrivateMap3Presenter", "PrivateOriginalMapBaseViewport",
                    "PrivateMap3CameraProjection", "GameSession", "Application-owned player locomotion" },
                instrumentation = new[] { "validated Map19 snapshot seed via existing private state factories",
                    "fixture route driving", "actual Map3Root F adapter via test-only reflection", "disabled unsolicited physics/input callbacks", "FramePostDraw/SavePng" },
                assetCommit = PrivateLocalPresentationAssetCatalog.Map3AssetRepositoryCommit,
                assetManifest = PrivateLocalPresentationAssetCatalog.Map3AssetManifestDigest,
                frames = _frames,
            }, new JsonSerializerOptions { WriteIndented = true }));
            GD.Print("SF2_CASTLE_ATLAS_NATIVE_REVIEW Pass frames=10 seeded-projection-only");
            _fixture.Dispose();
            GetTree().Quit();
        }
        catch (Exception error)
        {
            // Paths and input data stay in the private capture directory, not general smoke output.
            if (_output is not null && Directory.Exists(_output))
                File.WriteAllText(Path.Combine(_output, "failure.txt"), error.ToString());
            GD.PushError($"SF2_CASTLE_ATLAS_NATIVE_REVIEW Fail {error.GetType().Name}");
            GetTree().Quit(1);
        }
    }

    private void SeedMap19()
    {
        var initial = _session.PrivateOriginalMapSnapshot;
        var definition = initial.Definition;
        var runtime = definition.RuntimeCatalog.Resolve(new MapId("map19"));
        // The same explicit test-only seed used by the canonical private test.
        // This does not execute the natural Map3 route or any original init.
        var entry = new PrivateOriginalMapSessionSnapshot(definition, initial.Receipt, runtime.WorkingLayout,
            1, new(26, 30), null, false, null,
            zone601: State<PrivateOriginalMapZone601State>("AstralZoneRepositioned", definition.Zone601!, definition.AstralZone!),
            sarah: State<PrivateOriginalMapSarahState>("MessengerFollowerReady", definition.Sarah!, definition.AstralZone!, definition.MessengerAcceptance!),
            entity142: State<PrivateOriginalMapEntity142State>("ReleaseRouteOccupancy", definition.Entity142!,
                State<PrivateOriginalMapEntity142State>("Acknowledged", definition.Entity142!, 1L), definition.MessengerAcceptance!),
            castleGate: State<PrivateOriginalMapCastleGateState>("Completed", definition.CastleGate!),
            currentRuntime: runtime,
            lastCrossMapTransition: (PrivateOriginalMapCrossMapTransitionReceipt)Activator.CreateInstance(
                typeof(PrivateOriginalMapCrossMapTransitionReceipt), BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null, args: [definition.NorthMap19Transition!, 1L], culture: null)!);
        typeof(GameSession).GetField("_privateOriginalMapSnapshot", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(_session, entry);
        var relocated = typeof(PrivateOriginalMapPlayerLocomotionSnapshot).GetMethod(
            "Relocate", BindingFlags.Static | BindingFlags.NonPublic, binder: null,
            types: [typeof(PrivateOriginalMapPlayerLocomotionSnapshot), typeof(PrivateOriginalMapCrossMapTransitionReceipt)],
            modifiers: null)!.Invoke(null, [_session.PrivateOriginalMapPlayerLocomotion, entry.LastCrossMapTransition!]);
        typeof(GameSession).GetField("_privateOriginalMapPlayerLocomotion", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(_session, relocated);
    }

    private PrivateOriginalMapMoveApplied Move(ExplorationDirection direction)
    {
        var started = _session.BeginPrivateOriginalMapPlayerLocomotion(new(direction));
        var animation = started.Animation;
        _presenter.Project(started.Move.Snapshot, "Controlled native route movement", animation);
        int ticks = 0;
        while (animation.IsMoving && ticks++ < 13)
        {
            animation = _session.AdvancePrivateOriginalMapPlayerLocomotion();
            _presenter.Project(started.Move.Snapshot, "Controlled native route tick", animation);
        }
        Require(!animation.IsMoving, "Bounded locomotion completion");
        return started.Move;
    }

    private void DriveSegment(string id, int count, int skip = 0)
    {
        var segment = _fixture.RootElement.GetProperty("static").GetProperty("routeGraph")
            .GetProperty("segments").EnumerateArray().Single(row => row.GetProperty("id").GetString() == id);
        var inputs = segment.GetProperty("inputs");
        Require(inputs.GetArrayLength() == count, "Fixture input count");
        for (int index = skip; index < count; index++)
        {
            var point = segment.GetProperty("points")[index];
            Require(_session.PrivateOriginalMapSnapshot.PlayerPosition ==
                new MapPosition(point[0].GetInt32(), point[1].GetInt32()), "Fixture source position");
            var direction = inputs[index].GetString() switch
            {
                "Up" => ExplorationDirection.North, "Down" => ExplorationDirection.South,
                "Left" => ExplorationDirection.West, "Right" => ExplorationDirection.East,
                _ => throw new InvalidOperationException("Unknown fixture direction."),
            };
            Move(direction);
        }
    }

    private async Task Capture(string name, string map, int? expectedGlyphs)
    {
        var snapshot = _session.PrivateOriginalMapSnapshot;
        // After the seed, let the existing projection resolve the current snapshot focus.
        _presenter.Project(snapshot, "Controlled native visual review", _session.PrivateOriginalMapPlayerLocomotion);
        var projection = _presenter.BaseProjection!;
        var viewport = Field<PrivateOriginalMapBaseViewport>(_presenter, "_baseViewport");
        var glyphs = viewport.LiveRouteActorProjection?.Actors;
        Require(viewport.Visible && snapshot.Map.Value == map && projection.Map == snapshot.Map &&
            snapshot.CurrentRuntime.VisualResourceSelection.Map == snapshot.Map, "Current runtime base view");
        Require(_presenter.BaseAtlasAssetId == PrivateLocalPresentationAssetCatalog.BaseAtlasAssetIdForSelection(
            snapshot.CurrentRuntime.VisualResourceSelection), "Current atlas binding");
        var camera = projection.Camera!;
        Require(!_session.PrivateOriginalMapPlayerLocomotion.IsMoving &&
            camera.FocusPixelX == snapshot.PlayerPosition.X * 24 &&
            camera.FocusPixelY == snapshot.PlayerPosition.Y * 24, "Settled camera uses destination coordinates");
        Require(camera.OriginX == Math.Clamp(snapshot.PlayerPosition.X - 6, 0, 64 - 12) &&
            camera.OriginY == Math.Clamp(snapshot.PlayerPosition.Y - 3, 0, 64 - 7),
            "Settled crop is centered on the destination within layout bounds");
        var player = PrivateOriginalMapBaseViewport.PlayerLocomotionRect(
            projection, _session.PrivateOriginalMapPlayerLocomotion);
        Require(player.Position == new Vector2(camera.PlayerPixelX, camera.PlayerPixelY),
            "Player drawing uses the same destination focus");
        if (expectedGlyphs is not null)
        {
            Require((glyphs?.Count ?? 0) == expectedGlyphs && _presenter.Entity142DiagnosticProjection is null,
                "Map-scoped actor visibility");
            Require(glyphs is null || glyphs.All(glyph => glyph.Kind == PrivateMap3LiveRouteActorGlyphKind.AstralDiamond),
                "No Map3 route glyphs in castle");
        }
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        using Image image = GetViewport().GetTexture().GetImage();
        Require(!image.IsEmpty() && image.GetWidth() >= 960 && image.GetHeight() >= 540, "Native rendered viewport");
        Require(image.SavePng(Path.Combine(_output, name + ".png")) == Error.Ok, "Native PNG write");
        _frames.Add(new { name, map, selectionMap = snapshot.CurrentRuntime.VisualResourceSelection.Map.Value,
            area = snapshot.CurrentArea.OneBasedRecordOrdinal, layout = snapshot.CurrentRuntime.DecodedLayoutDigest,
            atlas = _presenter.BaseAtlasAssetId, digest = _presenter.BaseAtlasBucketDigest,
            glyphs = glyphs?.Select(glyph => glyph.Kind.ToString()).ToArray() ?? [],
            cameraOriginX = camera.OriginX, cameraOriginY = camera.OriginY,
            cameraFocusX = camera.FocusPixelX, cameraFocusY = camera.FocusPixelY,
            playerPixelX = player.Position.X, playerPixelY = player.Position.Y,
            playerX = snapshot.PlayerPosition.X, playerY = snapshot.PlayerPosition.Y,
            width = image.GetWidth(), height = image.GetHeight() });
    }

    private async Task CaptureMiddleTowerDiagnostic()
    {
        var snapshot = _session.PrivateOriginalMapSnapshot;
        var movement = _session.PrivateOriginalMapPlayerLocomotion;
        var receipt = snapshot.LastCrossMapTransition;
        Require(snapshot.Map == new MapId("map21") && snapshot.PlayerPosition == new MapPosition(3, 16) &&
            snapshot.CurrentArea.OneBasedRecordOrdinal == 1 &&
            ReferenceEquals(snapshot.CurrentRuntime, snapshot.Definition.RuntimeCatalog.Resolve(new("map21"))) &&
            OriginalMapRuntimeAdmission.HasExactAcceptedMap21Runtime(snapshot.CurrentRuntime), "Exact Map21 runtime arrival");
        Require(receipt?.Capability == OriginalMapRuntimeAdmission.MiddleTowerMap21TransitionCapability &&
            receipt.RecordIdentity.SourceMap == new MapId("map20") && receipt.RecordIdentity.OneBasedRecordOrdinal == 4 &&
            receipt.Source == new MapPosition(4, 37) && receipt.Trigger == new MapPosition(3, 36) &&
            receipt.DestinationOpaqueFacing == 0 && movement.OpaqueFacing == 0 &&
            movement.Phase == PrivateOriginalMapPlayerLocomotionPhase.Relocated && !movement.IsMoving &&
            movement.DestinationPosition == snapshot.PlayerPosition, "Map21 source receipt and relocation");
        Require(snapshot.PalaceFirstVisit?.CompletionFlag605Set == true &&
            snapshot.AstralAcceptance?.HandlerFlag607Set == true && snapshot.AstralAcceptance.ProgramFlag608Set,
            "Retained controlled route completion");
        _presenter.Project(snapshot, "Controlled native visual review", movement);
        var baseViewport = Field<PrivateOriginalMapBaseViewport>(_presenter, "_baseViewport");
        var traversalViewport = Field<PrivateOriginalMapTraversalViewport>(_presenter, "_viewport");
        var projection = traversalViewport.Projection!;
        Require(!baseViewport.Visible && traversalViewport.Visible && projection.Map == snapshot.Map,
            "Map21 visible diagnostic and hidden castle atlas");
        var player = projection.Cells.Single(cell => cell.IsPlayer);
        Require(player.MapX == 3 && player.MapY == 16 && player.Column == 3 && player.Row == 3 &&
            projection.OriginX == 0 && projection.OriginY == 13, "Map21 visible player and diagnostic crop");
        var statusLabel = Field<Label>(_presenter, "_status");
        Require(statusLabel.Position.Y >= traversalViewport.Position.Y +
            PrivateOriginalMapTraversalViewProjection.RowCount * 48, "Map21 status does not overlap the diagnostic grid");
        string status = statusLabel.Text;
        Require(status.StartsWith("Middle tower Map 21 reached.", StringComparison.Ordinal) &&
            status.Contains("Diagnostic traversal", StringComparison.Ordinal) &&
            status.Contains("init not executed", StringComparison.Ordinal) &&
            !status.Contains("F ", StringComparison.Ordinal), "Map21 diagnostic status");
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        using Image image = GetViewport().GetTexture().GetImage();
        Require(!image.IsEmpty() && image.GetWidth() >= 960 && image.GetHeight() >= 540, "Native Map21 viewport");
        const string name = "07-map21-diagnostic";
        Require(image.SavePng(Path.Combine(_output, name + ".png")) == Error.Ok, "Map21 PNG write");
        _frames.Add(new { name, map = snapshot.Map.Value, selectionMap = snapshot.CurrentRuntime.VisualResourceSelection.Map.Value,
            area = snapshot.CurrentArea.OneBasedRecordOrdinal, layout = snapshot.CurrentRuntime.DecodedLayoutDigest,
            baseVisible = baseViewport.Visible, traversalVisible = traversalViewport.Visible, status, statusY = statusLabel.Position.Y,
            sourceMap = receipt!.RecordIdentity.SourceMap.Value, sourceRecord = receipt.RecordIdentity.OneBasedRecordOrdinal,
            sourceX = receipt.Source.X, sourceY = receipt.Source.Y, triggerX = receipt.Trigger.X, triggerY = receipt.Trigger.Y,
            facing = movement.OpaqueFacing, phase = movement.Phase.ToString(),
            cameraOriginX = projection.OriginX, cameraOriginY = projection.OriginY,
            playerColumn = player.Column, playerRow = player.Row, playerX = player.MapX, playerY = player.MapY,
            width = image.GetWidth(), height = image.GetHeight() });
    }

    private void ApplyGuardF(bool expectApplied)
    {
        var before = _session.PrivateOriginalMapSnapshot;
        var animation = _session.PrivateOriginalMapPlayerLocomotion;
        var bridge = _session.PrivateOriginalMapBattleBridge;
        typeof(Map3Root).GetMethod("ApplyPrivateInteractionRequest", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(_root, null);
        var after = _session.PrivateOriginalMapSnapshot;
        Require(ReferenceEquals(animation, _session.PrivateOriginalMapPlayerLocomotion) &&
            ReferenceEquals(bridge, _session.PrivateOriginalMapBattleBridge), "F retains locomotion and bridge");
        if (!expectApplied)
        {
            Require(ReferenceEquals(before, after), "Rejected or duplicate actual F leaves snapshot unchanged");
            return;
        }
        Require(after.MiddleTowerGuard is not null && after.SimulationStep == before.SimulationStep + 1 &&
            after.PlayerPosition == before.PlayerPosition && animation.OpaqueFacing == 0 &&
            ReferenceEquals(before.PalaceFirstVisit, after.PalaceFirstVisit) &&
            ReferenceEquals(before.AstralAcceptance, after.AstralAcceptance) &&
            ReferenceEquals(before.Sarah, after.Sarah) && ReferenceEquals(before.Entity142, after.Entity142) &&
            ReferenceEquals(before.Zone601, after.Zone601) && ReferenceEquals(before.MessengerAcceptance, after.MessengerAcceptance) &&
            ReferenceEquals(before.CastleGate, after.CastleGate) && ReferenceEquals(before.Receipt, after.Receipt),
            "Actual composition F commits only the controlled guard result");
    }

    private async Task CaptureGuard(string name, MapPosition playerPosition, MapPosition guardPosition, bool completed)
    {
        var snapshot = _session.PrivateOriginalMapSnapshot;
        var traversal = Field<PrivateOriginalMapTraversalViewport>(_presenter, "_viewport");
        var baseViewport = Field<PrivateOriginalMapBaseViewport>(_presenter, "_baseViewport");
        var projection = traversal.Projection!;
        Require(traversal.Visible && !baseViewport.Visible && projection.Map == new MapId("map21") &&
            snapshot.PlayerPosition == playerPosition && snapshot.MiddleTowerGuardPosition == guardPosition &&
            (snapshot.MiddleTowerGuard is not null) == completed, "Visible guard runtime/diagnostic state");
        var player = projection.Cells.Single(cell => cell.IsPlayer);
        var guard = projection.Cells.Single(cell => cell.IsMiddleTowerGuard);
        Require(player.MapX == playerPosition.X && player.MapY == playerPosition.Y &&
            guard.MapX == guardPosition.X && guard.MapY == guardPosition.Y && !guard.IsPlayer && !guard.IsAstral,
            "Actual player and undirected guard marker match Application positions");
        var label = Field<Label>(_presenter, "_status");
        Require(label.Position.Y >= traversal.Position.Y + PrivateOriginalMapTraversalViewProjection.RowCount * 48 &&
            (completed ? label.Text.StartsWith("Guard moved; passage open.", StringComparison.Ordinal) :
                label.Text.StartsWith("F apply controlled guard result", StringComparison.Ordinal)),
            "Actual F action/status is readable below the grid");
        Require(snapshot.Definition.MiddleTowerGuard!.Actor.Position == new MapPosition(5, 16) &&
            snapshot.Definition.MiddleTowerGuard.Actor.OpaqueFacing == 3, "Immutable guard source retained");
        if (completed)
            Require(snapshot.MiddleTowerGuard!.HandlerFlag256Set && snapshot.MiddleTowerGuard.ProgramStoryFlag1Set &&
                snapshot.MiddleTowerGuard.ProgramFlag401Set, "Distinct controlled handler/program completion");
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        using Image image = GetViewport().GetTexture().GetImage();
        Require(!image.IsEmpty() && image.GetWidth() >= 960 && image.GetHeight() >= 540, "Native guard image");
        Require(image.SavePng(Path.Combine(_output, name + ".png")) == Error.Ok, "Native guard PNG write");
        _frames.Add(new { name, map = snapshot.Map.Value, area = snapshot.CurrentArea.OneBasedRecordOrdinal,
            playerX = player.MapX, playerY = player.MapY, guardX = guard.MapX, guardY = guard.MapY,
            completed, facing = _session.PrivateOriginalMapPlayerLocomotion.OpaqueFacing, status = label.Text, statusY = label.Position.Y,
            sourceHandler = snapshot.Definition.MiddleTowerGuard.HandlerIdentity, textId = snapshot.Definition.MiddleTowerGuard.TextId,
            sourceProgram = snapshot.Definition.MiddleTowerGuard.ProgramIdentity,
            handler256 = snapshot.MiddleTowerGuard?.HandlerFlag256Set == true,
            storyFlag1 = snapshot.MiddleTowerGuard?.ProgramStoryFlag1Set == true,
            baseVisible = baseViewport.Visible, traversalVisible = traversal.Visible,
            width = image.GetWidth(), height = image.GetHeight() });
    }

    private static T Field<T>(object instance, string name) => (T)instance.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(instance)!;
    private static T State<T>(string method, params object[] arguments) => (T)typeof(T)
        .GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, arguments)!;
    private static void Require(bool condition, string boundary)
    {
        if (!condition) throw new InvalidOperationException(boundary);
    }
    private static string RequiredPath(string variable)
    {
        string? path = System.Environment.GetEnvironmentVariable(variable);
        return path is not null && Path.IsPathFullyQualified(path) ? path
            : throw new InvalidOperationException("A fully qualified private probe path is required.");
    }
}
