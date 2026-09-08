// Compile only in an ignored copy of an exact committed Godot project. This is
// controlled seed/route/capture instrumentation, not a production entry point.
using System.Reflection;
using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Content;

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
            await CaptureMiddleTowerArrival();
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
            await CaptureNorthMap40Arrival();
            await CaptureBattle01Pending();

            File.WriteAllText(Path.Combine(_output, "receipt.json"), JsonSerializer.Serialize(new
            {
                status = "Pass",
                scope = "seeded-native-current-runtime-projection; natural-route/H4 Unknown",
                production = new[] { "Map3Root", "PrivateMap3Composition", "PrivateCanonicalMap3ImportReader",
                    "PrivateLocalPresentationAssetCatalog", "PrivateMap3Presenter", "PrivateOriginalMapBaseViewport",
                    "PrivateMap3CameraProjection", "GameSession", "Application-owned player locomotion" },
                instrumentation = new[] { "validated Map19 snapshot seed via existing private state factories",
                    "fixture route driving", "actual Map3Root F and pending-move adapters via test-only reflection", "disabled unsolicited physics/input callbacks", "FramePostDraw/SavePng" },
                assetCommit = PrivateLocalPresentationAssetCatalog.Map3AssetRepositoryCommit,
                assetManifest = PrivateLocalPresentationAssetCatalog.Map3AssetManifestDigest,
                frames = _frames,
            }, new JsonSerializerOptions { WriteIndented = true }));
            GD.Print("SF2_CASTLE_ATLAS_NATIVE_REVIEW Pass frames=12 seeded-projection-only");
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

    private async Task CaptureNorthMap40Arrival()
    {
        var before = _session.PrivateOriginalMapSnapshot;
        var bridge = _session.PrivateOriginalMapBattleBridge;
        using var fixture = JsonDocument.Parse(File.ReadAllText(RequiredPath("SF2_MAP40_REVIEW_FIXTURE")));
        var segments = fixture.RootElement.GetProperty("static").GetProperty("extensionRoute").GetProperty("segments");
        var route = segments[0];
        Require(route.GetProperty("id").GetString() == "map21-terminal-to-north-exit" &&
            route.GetProperty("inputs").GetArrayLength() == 18 && route.GetProperty("points").GetArrayLength() == 19,
            "Fixed first extension segment");
        Require(before.PlayerPosition == new MapPosition(5, 15) &&
            _session.PrivateOriginalMapPlayerLocomotion.OpaqueFacing == 1 && before.MiddleTowerGuard is not null,
            "Ordinary Right/Up guard endpoint");
        for (int index = 0; index < 18; index++)
        {
            var point = route.GetProperty("points")[index];
            Require(_session.PrivateOriginalMapSnapshot.Map == new MapId("map21") &&
                _session.PrivateOriginalMapSnapshot.PlayerPosition == new MapPosition(point[0].GetInt32(), point[1].GetInt32()),
                "Every north route approach");
            var direction = route.GetProperty("inputs")[index].GetString() switch {
                "Up" => ExplorationDirection.North, "Right" => ExplorationDirection.East,
                _ => throw new InvalidOperationException("Unknown north route input."),
            };
            var move = Move(direction);
            var target = route.GetProperty("points")[index + 1];
            Require((index == 17 ? move.CrossMapTransition?.Trigger : move.Snapshot.PlayerPosition) ==
                new MapPosition(target[0].GetInt32(), target[1].GetInt32()), "Every north route target");
        }
        var snapshot = _session.PrivateOriginalMapSnapshot;
        var movement = _session.PrivateOriginalMapPlayerLocomotion;
        var receipt = snapshot.LastCrossMapTransition;
        Require(snapshot.Map == new MapId("map40") && snapshot.PlayerPosition == new MapPosition(4, 30) &&
            snapshot.CurrentArea.OneBasedRecordOrdinal == 1 &&
            OriginalMapRuntimeAdmission.HasExactAcceptedMap40Runtime(snapshot.CurrentRuntime) &&
            ReferenceEquals(snapshot.CurrentRuntime, snapshot.Definition.RuntimeCatalog.Resolve(snapshot.Map)),
            "Exact Map40 runtime arrival");
        Require(receipt?.Capability == OriginalMapRuntimeAdmission.NorthMap40TransitionCapability &&
            receipt.RecordIdentity.SourceMap == new MapId("map21") && receipt.RecordIdentity.OneBasedRecordOrdinal == 2 &&
            receipt.Source == new MapPosition(9, 2) && receipt.Trigger == new MapPosition(9, 1) &&
            receipt.DestinationOpaqueFacing == 1 && movement.OpaqueFacing == 1 &&
            movement.Phase == PrivateOriginalMapPlayerLocomotionPhase.Relocated && !movement.IsMoving &&
            movement.DestinationPosition == snapshot.PlayerPosition, "Map40 source receipt and relocation");
        Require(ReferenceEquals(before.MiddleTowerGuard, snapshot.MiddleTowerGuard) &&
            ReferenceEquals(before.PalaceFirstVisit, snapshot.PalaceFirstVisit) &&
            ReferenceEquals(before.AstralAcceptance, snapshot.AstralAcceptance) &&
            ReferenceEquals(before.Receipt, snapshot.Receipt) && ReferenceEquals(before.Zone601, snapshot.Zone601) &&
            ReferenceEquals(before.Sarah, snapshot.Sarah) && ReferenceEquals(before.Entity142, snapshot.Entity142) &&
            ReferenceEquals(before.MessengerAcceptance, snapshot.MessengerAcceptance) &&
            ReferenceEquals(before.CastleGate, snapshot.CastleGate) && ReferenceEquals(bridge, _session.PrivateOriginalMapBattleBridge),
            "Map40 retains completed route and manual bridge");
        _presenter.Project(snapshot, "Controlled native visual review", movement);
        var traversal = Field<PrivateOriginalMapTraversalViewport>(_presenter, "_viewport");
        var baseViewport = Field<PrivateOriginalMapBaseViewport>(_presenter, "_baseViewport");
        var projection = _presenter.BaseProjection!;
        Require(!traversal.Visible && baseViewport.Visible && projection.Map == snapshot.Map &&
            projection.OriginX == 0 && projection.OriginY == 27 &&
            projection.PlayerColumn == 4 && projection.PlayerRow == 3 &&
            baseViewport.LiveRouteActorProjection is null && snapshot.MiddleTowerGuardPosition is null &&
            _presenter.Entity142DiagnosticProjection is null && !projection.StaticOverlayDiagnostic &&
            !projection.CurrentAreaOverlay, "Map40 base crop and no retained actors or Map3 overlays");
        Require(_presenter.BaseAtlasAssetId == PrivateLocalPresentationAssetCatalog.Map40BaseAtlasAssetId &&
            _presenter.BaseAtlasBucketDigest == (_presenter.BaseAtlasScale == 2
                ? PrivateLocalPresentationAssetCatalog.Map40BaseAtlas2xDigest
                : PrivateLocalPresentationAssetCatalog.Map40BaseAtlas4xDigest) &&
            _presenter.UsesRequiredBaseAtlasSampling, "Exact Map40 family and nearest bucket");
        var camera = projection.Camera!;
        var player = PrivateOriginalMapBaseViewport.PlayerLocomotionRect(projection, movement);
        Require(camera.FocusPixelX == 4 * 24 && camera.FocusPixelY == 30 * 24 &&
            player.Position == new Vector2(4 * 24, 3 * 24) &&
            player.Position == new Vector2(camera.PlayerPixelX, camera.PlayerPixelY),
            "Map40 camera and player use the relocated destination");
        var status = Field<Label>(_presenter, "_status");
        Require(status.Position.Y == 310 &&
            status.Position.Y >= baseViewport.Position.Y + PrivateOriginalMapBaseViewProjection.PixelHeight &&
            status.Text.StartsWith("Map 40 controlled arrival. Base atlas; init not executed.", StringComparison.Ordinal),
            "Map40 atlas status below the base viewport");
        VerifyMap40BucketPixels(snapshot, movement);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        using Image image = GetViewport().GetTexture().GetImage();
        Require(!image.IsEmpty() && image.GetWidth() >= 960 && image.GetHeight() >= 540, "Native Map40 image");
        const string name = "11-map40-base-atlas";
        Require(image.SavePng(Path.Combine(_output, name + ".png")) == Error.Ok, "Map40 PNG write");
        int checkedPixels = 0;
        for (int y = 0; y < PrivateOriginalMapBaseViewProjection.PixelHeight; y++)
        for (int x = 0; x < PrivateOriginalMapBaseViewProjection.PixelWidth; x++)
        {
            if (player.HasPoint(new Vector2(x, y))) continue;
            int offset = ((y * projection.RasterScale * projection.RasterPixelWidth) + x * projection.RasterScale) * 4;
            if (projection.RgbaBytes[offset + 3] != 255) continue;
            Vector2 sample = baseViewport.GetViewportTransform() * baseViewport.GetGlobalTransform() * new Vector2(x + 0.25f, y + 0.25f);
            Color actual = image.GetPixel((int)sample.X, (int)sample.Y);
            Require(Math.Abs(actual.R * 255 - projection.RgbaBytes[offset]) < 1.1f &&
                Math.Abs(actual.G * 255 - projection.RgbaBytes[offset + 1]) < 1.1f &&
                Math.Abs(actual.B * 255 - projection.RgbaBytes[offset + 2]) < 1.1f,
                "Actual Map40 rendered base pixel");
            checkedPixels++;
        }
        Require(checkedPixels > 10000, "Substantial actual Map40 base pixel coverage");
        _frames.Add(new { name, map = snapshot.Map.Value, selectionMap = snapshot.CurrentRuntime.VisualResourceSelection.Map.Value,
            palette = snapshot.CurrentRuntime.VisualResourceSelection.PaletteIndex, tilesets = snapshot.CurrentRuntime.VisualResourceSelection.TilesetSlots,
            area = snapshot.CurrentArea.OneBasedRecordOrdinal, layout = snapshot.CurrentRuntime.DecodedLayoutDigest,
            atlas = _presenter.BaseAtlasAssetId, digest = _presenter.BaseAtlasBucketDigest,
            baseVisible = baseViewport.Visible, traversalVisible = traversal.Visible,
            cameraOriginX = projection.OriginX, cameraOriginY = projection.OriginY,
            cameraFocusX = camera.FocusPixelX, cameraFocusY = camera.FocusPixelY,
            playerPixelX = player.Position.X, playerPixelY = player.Position.Y, playerX = 4, playerY = 30,
            facing = movement.OpaqueFacing, statusY = status.Position.Y, status = status.Text,
            sourceMap = receipt!.RecordIdentity.SourceMap.Value, sourceRecord = receipt.RecordIdentity.OneBasedRecordOrdinal,
            inputCount = 18, guardGlyphs = 0, checkedPixels, verifiedBucketScales = new[] { 2, 4 },
            width = image.GetWidth(), height = image.GetHeight() });
    }

    private async Task CaptureBattle01Pending()
    {
        using var fixture = JsonDocument.Parse(File.ReadAllText(RequiredPath("SF2_MAP40_REVIEW_FIXTURE")));
        var route = fixture.RootElement.GetProperty("static").GetProperty("extensionRoute").GetProperty("segments")[2];
        Require(route.GetProperty("id").GetString() == "map40-entry-to-wildcard-battle-warp" &&
            route.GetProperty("inputs").GetArrayLength() == 28 && route.GetProperty("points").GetArrayLength() == 29,
            "Fixed pending-admission segment");
        var entry = _session.PrivateOriginalMapSnapshot;
        for (int index = 0; index < 27; index++)
        {
            var point = route.GetProperty("points")[index];
            Require(_session.PrivateOriginalMapSnapshot.PlayerPosition == new MapPosition(point[0].GetInt32(), point[1].GetInt32()),
                "Every committed Map40 source point");
            var direction = route.GetProperty("inputs")[index].GetString() switch {
                "Up" => ExplorationDirection.North, "Right" => ExplorationDirection.East,
                "Left" => ExplorationDirection.West, "Down" => ExplorationDirection.South,
                _ => throw new InvalidOperationException("Unknown pending route input."),
            };
            var move = Move(direction);
            var target = route.GetProperty("points")[index + 1];
            Require(move.Battle01Admission is null && move.CrossMapTransition is null &&
                move.Snapshot.PlayerPosition == new MapPosition(target[0].GetInt32(), target[1].GetInt32()),
                "Every committed Map40 target point");
        }
        var source = _session.PrivateOriginalMapSnapshot;
        var animation = _session.PrivateOriginalMapPlayerLocomotion;
        var bridge = _session.PrivateOriginalMapBattleBridge;
        Require(source.Map == new MapId("map40") && source.PlayerPosition == new MapPosition(14, 13) &&
            source.SimulationStep == entry.SimulationStep + 27 && animation.OpaqueFacing == 1 && !animation.IsMoving &&
            route.GetProperty("inputs")[27].GetString() == "Up", "Idle Map40 pre-trigger source");
        void Input(string method, params object[] arguments) => typeof(Map3Root)
            .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(_root, arguments);
        // Exercise the production input-result adapter, including its avoidance of ordinary Traversal.
        Input("ApplyPrivateMoveWhenAvailable", ExplorationDirection.North);
        var pending = _session.PrivateOriginalBattle01Admission;
        var finalPoint = route.GetProperty("points")[28];
        Require(pending is not null && ReferenceEquals(pending.SourceSnapshot, source) &&
            pending.Trigger == new MapPosition(finalPoint[0].GetInt32(), finalPoint[1].GetInt32()) &&
            pending.Definition.DestinationMap == new MapId("map57") && pending.Definition.Destination == new MapPosition(8, 18) &&
            pending.Definition.DestinationOpaqueFacing == 1 && pending.Definition.BattleIndex == 1 &&
            pending.Definition.Preset == OriginalBattle01ControlledPreset.NewBattle, "Typed prospective Battle01 destination");
        var status = Field<Label>(_presenter, "_status");
        string pendingText = status.Text;
        Input("ApplyPrivateMoveWhenAvailable", ExplorationDirection.North);
        Input("ApplyPrivateInteractionRequest");
        Input("ApplyPrivateBattleBridgeRequest");
        _root._PhysicsProcess(1.0 / 60.0);
        Require(ReferenceEquals(source, _session.PrivateOriginalMapSnapshot) &&
            ReferenceEquals(animation, _session.PrivateOriginalMapPlayerLocomotion) &&
            ReferenceEquals(bridge, _session.PrivateOriginalMapBattleBridge) &&
            ReferenceEquals(pending, _session.PrivateOriginalBattle01Admission) && status.Text == pendingText,
            "Pending input/tick rejection preserves source, animation, bridge and display");
        var traversal = Field<PrivateOriginalMapTraversalViewport>(_presenter, "_viewport");
        var baseViewport = Field<PrivateOriginalMapBaseViewport>(_presenter, "_baseViewport");
        var projection = _presenter.BaseProjection!;
        Require(baseViewport.Visible && !traversal.Visible && projection.Map == new MapId("map40") &&
            _presenter.BaseAtlasAssetId == PrivateLocalPresentationAssetCatalog.Map40BaseAtlasAssetId &&
            _presenter.UsesRequiredBaseAtlasSampling && baseViewport.LiveRouteActorProjection is null,
            "Pending retains the Map40 atlas and no Map57 or guard projection");
        Require(status.Position.Y == 310 &&
            status.Position.Y >= baseViewport.Position.Y + PrivateOriginalMapBaseViewProjection.PixelHeight &&
            pendingText.StartsWith("Battle 01 admission pending. Battle not started.", StringComparison.Ordinal) &&
            pendingText.Contains("destination Map 57 (8,18)/UP.", StringComparison.Ordinal) &&
            pendingText.Contains("Map 40 retained", StringComparison.Ordinal) &&
            pendingText.Contains("Restart", StringComparison.Ordinal) && !pendingText.Contains("ENTER", StringComparison.Ordinal),
            "Pending display and restart recovery below retained Map40");
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        using Image image = GetViewport().GetTexture().GetImage();
        Require(status.GetLineCount() == 3 && status.GetVisibleLineCount() == status.GetLineCount(),
            "All pending status lines, including restart recovery, are visible without clipping");
        const string name = "12-battle01-admission-pending";
        Require(image.SavePng(Path.Combine(_output, name + ".png")) == Error.Ok, "Pending PNG write");
        _frames.Add(new { name, map = source.Map.Value, destinationMap = pending!.Definition.DestinationMap.Value,
            playerX = source.PlayerPosition.X, playerY = source.PlayerPosition.Y, facing = animation.OpaqueFacing,
            simulationStep = source.SimulationStep, committedInputCount = 27, prospectiveInputCount = 1,
            triggerX = pending.Trigger.X, triggerY = pending.Trigger.Y, destinationX = 8, destinationY = 18,
            battleIndex = pending.Definition.BattleIndex, battleStarted = false, preset = pending.Definition.Preset,
            atlas = _presenter.BaseAtlasAssetId, digest = _presenter.BaseAtlasBucketDigest,
            cameraOriginX = projection.OriginX, cameraOriginY = projection.OriginY,
            baseVisible = baseViewport.Visible, traversalVisible = traversal.Visible,
            statusY = status.Position.Y, status = pendingText, statusLines = status.GetLineCount(),
            visibleStatusLines = status.GetVisibleLineCount(), identitiesRetained = true,
            width = image.GetWidth(), height = image.GetHeight() });
    }

    private static void VerifyMap40BucketPixels(PrivateOriginalMapSessionSnapshot snapshot,
        PrivateOriginalMapPlayerLocomotionSnapshot movement)
    {
        var reader = new LocalPresentationAssetPackReader(RequiredPath("SF2_PRIVATE_PRESENTATION_ASSET_ROOT"),
            PrivateLocalPresentationAssetCatalog.Map3AssetRepositoryCommit);
        var request = new LocalPresentationAssetPackRequest(LocalPresentationAssetPackAdmission.PackageId,
            ContentProfile.PrivateLocal, LocalPresentationAssetPackAdmission.RepositoryId,
            PrivateLocalPresentationAssetCatalog.Map3AssetRepositoryCommit,
            PrivateLocalPresentationAssetCatalog.Map3AssetManifestDigest);
        var accepted = (LocalPresentationAssetPackAccepted)reader.Admit(request);
        var catalog = new PrivateLocalPresentationAssetCatalog(reader);
        List<PrivateOriginalMapBaseViewProjection> projections = [];
        foreach (int scale in new[] { 2, 4 })
        {
            var mounted = (PrivateLocalPresentationAssetMounted)catalog.MountMap40BaseAtlas(request, accepted, scale);
            using Image atlas = new();
            Require(atlas.LoadPngFromBuffer(mounted.Asset.CopyPngBytes()) == Error.Ok &&
                atlas.GetFormat() == Image.Format.Rgba8 && atlas.GetWidth() == 128 * scale &&
                atlas.GetHeight() == 320 * scale, "Actual accepted Map40 bucket decoded");
            projections.Add(PrivateOriginalMapBaseViewProjection.CreateFromAtlas(snapshot,
                snapshot.CurrentRuntime.VisualResourceSelection, atlas.GetData(), scale, playerLocomotion: movement));
        }
        var two = projections[0];
        var four = projections[1];
        Require(two.OriginX == 0 && two.OriginY == 27 && four.OriginX == 0 && four.OriginY == 27,
            "Both actual buckets retain the destination crop");
        for (int y = 0; y < four.RasterPixelHeight; y++)
        for (int x = 0; x < four.RasterPixelWidth; x++)
        for (int channel = 0; channel < 4; channel++)
            Require(four.RgbaBytes[(y * four.RasterPixelWidth + x) * 4 + channel] ==
                two.RgbaBytes[((y / 2) * two.RasterPixelWidth + x / 2) * 4 + channel],
                "Both accepted Map40 buckets project exact nearest-equivalent pixels");
    }

    private async Task CaptureMiddleTowerArrival()
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
        await CaptureGuard("07-map21-base-atlas", new(3, 16), new(5, 16), completed: false, arrival: true);
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

    private async Task CaptureGuard(
        string name, MapPosition playerPosition, MapPosition guardPosition, bool completed, bool arrival = false)
    {
        var snapshot = _session.PrivateOriginalMapSnapshot;
        var movement = _session.PrivateOriginalMapPlayerLocomotion;
        var traversal = Field<PrivateOriginalMapTraversalViewport>(_presenter, "_viewport");
        var baseViewport = Field<PrivateOriginalMapBaseViewport>(_presenter, "_baseViewport");
        var projection = _presenter.BaseProjection!;
        Require(!traversal.Visible && baseViewport.Visible && projection.Map == new MapId("map21") &&
            snapshot.PlayerPosition == playerPosition && snapshot.MiddleTowerGuardPosition == guardPosition &&
            (snapshot.MiddleTowerGuard is not null) == completed &&
            OriginalMapRuntimeAdmission.HasExactAcceptedMap21Runtime(snapshot.CurrentRuntime),
            "Visible guard runtime and exact Map21 base atlas");
        Require(_presenter.BaseAtlasAssetId == PrivateLocalPresentationAssetCatalog.Map21BaseAtlasAssetId &&
            _presenter.BaseAtlasBucketDigest == (_presenter.BaseAtlasScale == 2
                ? PrivateLocalPresentationAssetCatalog.Map21BaseAtlas2xDigest
                : PrivateLocalPresentationAssetCatalog.Map21BaseAtlas4xDigest) &&
            _presenter.UsesRequiredBaseAtlasSampling, "Map21 family and nearest bucket binding");
        Require(!projection.StaticOverlayDiagnostic && !projection.CurrentAreaOverlay &&
            _presenter.Entity142DiagnosticProjection is null, "Map3 overlays and actors remain scoped");
        var camera = projection.Camera!;
        Require(!movement.IsMoving && camera.FocusPixelX == playerPosition.X * 24 &&
            camera.FocusPixelY == playerPosition.Y * 24 &&
            camera.OriginX == Math.Clamp(playerPosition.X - 6, 0, 64 - 12) &&
            camera.OriginY == Math.Clamp(playerPosition.Y - 3, 0, 64 - 7),
            "Map21 settled camera uses the current player destination");
        var player = PrivateOriginalMapBaseViewport.PlayerLocomotionRect(projection, movement);
        Require(player.Position == new Vector2(camera.PlayerPixelX, camera.PlayerPixelY),
            "Player drawing uses the current camera");
        var guard = baseViewport.LiveRouteActorProjection!.Actors.Single();
        Require(guard.Kind == PrivateMap3LiveRouteActorGlyphKind.MiddleTowerGuardDiamond &&
            guard.LogicalActorId == 128 && guard.Position == guardPosition &&
            guard.SourceRecord == snapshot.Definition.MiddleTowerGuard!.Actor.Identity &&
            guard.DestinationRect.Position == new Vector2(guardPosition.X * 24 - camera.TopLeftPixelX + 5,
                guardPosition.Y * 24 - camera.TopLeftPixelY + 5),
            "Live guard diamond uses current occupancy within the current camera");
        var label = Field<Label>(_presenter, "_status");
        Require(label.Position.Y >= baseViewport.Position.Y + PrivateOriginalMapBaseViewProjection.PixelHeight &&
            label.Text.StartsWith(arrival ? "Middle tower Map 21 reached." :
                completed ? "Guard moved; passage open." : "F apply controlled guard result",
                StringComparison.Ordinal), "Current action/status is readable below the base viewport");
        if (arrival) Require(label.Text.Contains("init not executed", StringComparison.Ordinal) &&
            !label.Text.Contains("F ", StringComparison.Ordinal), "Arrival keeps the controlled boundary");
        Require(snapshot.Definition.MiddleTowerGuard!.Actor.Position == new MapPosition(5, 16) &&
            snapshot.Definition.MiddleTowerGuard.Actor.OpaqueFacing == 3, "Immutable guard source retained");
        if (completed)
            Require(snapshot.MiddleTowerGuard!.HandlerFlag256Set && snapshot.MiddleTowerGuard.ProgramStoryFlag1Set &&
                snapshot.MiddleTowerGuard.ProgramFlag401Set, "Distinct controlled handler/program completion");
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        using Image image = GetViewport().GetTexture().GetImage();
        Require(!image.IsEmpty() && image.GetWidth() >= 960 && image.GetHeight() >= 540, "Native guard image");
        Require(image.SavePng(Path.Combine(_output, name + ".png")) == Error.Ok, "Native guard PNG write");
        // Sample inside the actual rendered diamond, where the old facing line would have appeared.
        Transform2D localToImage = baseViewport.GetViewportTransform() * baseViewport.GetGlobalTransform();
        List<object> guardPixels = [];
        foreach (Vector2 offset in new[] { Vector2.Zero, new Vector2(0, 3) })
        {
            Vector2 sample = localToImage * (guard.DestinationRect.GetCenter() + offset);
            int x = (int)Math.Round(sample.X), y = (int)Math.Round(sample.Y);
            Color color = image.GetPixel(x, y);
            Color purple = new("b5a0ff");
            Require(Math.Abs(color.R - purple.R) < 0.01f && Math.Abs(color.G - purple.G) < 0.01f &&
                Math.Abs(color.B - purple.B) < 0.01f, "Actual guard diamond has no source-facing line");
            guardPixels.Add(new { x, y, rgba = color.ToHtml() });
        }
        var receipt = snapshot.LastCrossMapTransition;
        Require(arrival ? receipt is not null : receipt is null,
            "Only the arrival operation retains the cross-map receipt");
        _frames.Add(new { name, map = snapshot.Map.Value, selectionMap = snapshot.CurrentRuntime.VisualResourceSelection.Map.Value,
            area = snapshot.CurrentArea.OneBasedRecordOrdinal, layout = snapshot.CurrentRuntime.DecodedLayoutDigest,
            atlas = _presenter.BaseAtlasAssetId, digest = _presenter.BaseAtlasBucketDigest,
            cameraOriginX = camera.OriginX, cameraOriginY = camera.OriginY,
            cameraFocusX = camera.FocusPixelX, cameraFocusY = camera.FocusPixelY,
            playerPixelX = player.Position.X, playerPixelY = player.Position.Y,
            playerX = playerPosition.X, playerY = playerPosition.Y, guardX = guard.Position.X, guardY = guard.Position.Y,
            guardKind = guard.Kind.ToString(), guardPixels,
            completed, facing = movement.OpaqueFacing, phase = movement.Phase.ToString(),
            status = label.Text, statusY = label.Position.Y,
            sourceMap = receipt?.RecordIdentity.SourceMap.Value, sourceRecord = receipt?.RecordIdentity.OneBasedRecordOrdinal,
            sourceX = receipt?.Source.X, sourceY = receipt?.Source.Y, triggerX = receipt?.Trigger.X, triggerY = receipt?.Trigger.Y,
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
