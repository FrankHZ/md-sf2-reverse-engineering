// Compile only in an ignored copy of an exact committed Godot project. This is
// controlled seed/route/capture instrumentation, not a production entry point.
using System.Reflection;
using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Battles;
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
            if (System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") == "missing-atlas")
            {
                Require(typeof(Map3Root).GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(root) is null, "Missing Map57 asset must fail before session startup");
                var status = Field<Label>(Field<PrivateMap3Presenter>(root, "_privatePresenter"), "_status");
                Require(status.Text.Contains("Map 57 base art unavailable") &&
                    status.GetLineCount() == status.GetVisibleLineCount(), "Missing Map57 art has a visible failure");
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                using var failedImage = GetViewport().GetTexture().GetImage();
                Require(failedImage.SavePng(Path.Combine(_output, "01-map57-asset-rejected.png")) == Error.Ok, "Failure PNG");
                File.WriteAllText(Path.Combine(_output, "receipt.json"), JsonSerializer.Serialize(new {
                    status = "Pass", scope = "explicit base art; missing Map57 runtime payload", sessionStarted = false,
                    fallback = false, message = status.Text, frames = 1 }));
                GD.Print("SF2_BATTLE01_CONTROL_NATIVE_REVIEW Pass frames=1 missing-atlas");
                _fixture.Dispose(); GetTree().Quit(); return;
            }
            _session = Field<GameSession>(root, "_session");
            _presenter = Field<PrivateMap3Presenter>(root, "_privatePresenter");
            if (System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") is "1" or "missing-input" or "base-art" or "diagnostic")
            {
                await ReviewBattle01Control();
                _fixture.Dispose();
                GetTree().Quit();
                return;
            }
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

    private async Task ReviewBattle01Control()
    {
        var initial = _session.PrivateOriginalMapSnapshot;
        var definition = initial.Definition;
        var runtime = definition.RuntimeCatalog.Resolve(new MapId("map40"));
        static T Receipt<T>(params object[] args) => (T)Activator.CreateInstance(typeof(T),
            BindingFlags.Instance | BindingFlags.NonPublic, null, args, null)!;
        // Explicit controlled Map40 entry seed. All subsequent moves and battle operations
        // pass through real Godot Input events, Map3Root polling and Application APIs.
        var entry = new PrivateOriginalMapSessionSnapshot(definition, initial.Receipt, runtime.WorkingLayout,
            5, new(4, 30), runtime.Traversal.TryMove(runtime.WorkingLayout, new(4, 31), ExplorationDirection.North),
            false, null,
            zone601: State<PrivateOriginalMapZone601State>("AstralZoneRepositioned", definition.Zone601!, definition.AstralZone!),
            sarah: State<PrivateOriginalMapSarahState>("MessengerFollowerReady", definition.Sarah!, definition.AstralZone!, definition.MessengerAcceptance!),
            entity142: State<PrivateOriginalMapEntity142State>("ReleaseRouteOccupancy", definition.Entity142!,
                State<PrivateOriginalMapEntity142State>("Acknowledged", definition.Entity142!, 1L), definition.MessengerAcceptance!),
            castleGate: State<PrivateOriginalMapCastleGateState>("Completed", definition.CastleGate!),
            currentRuntime: runtime,
            palaceFirstVisit: Receipt<PrivateOriginalMapPalaceFirstVisitReceipt>(definition.PalaceFirstVisit!, 2L),
            astralAcceptance: Receipt<PrivateOriginalMapAstralAcceptanceState>(definition.AstralAcceptance!, 3L),
            middleTowerGuard: Receipt<PrivateOriginalMapMiddleTowerGuardReceipt>(definition.MiddleTowerGuard!, 4L));
        typeof(GameSession).GetField("_privateOriginalMapSnapshot", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(_session, entry);
        typeof(GameSession).GetField("_privateOriginalMapPlayerLocomotion", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(_session,
            State<PrivateOriginalMapPlayerLocomotionSnapshot>("ControlledAdmission", entry.PlayerPosition));
        _presenter.Project(entry, "Controlled Map40 seed", _session.PrivateOriginalMapPlayerLocomotion);
        _root.ProcessMode = ProcessModeEnum.Inherit;
        using var fixture = JsonDocument.Parse(File.ReadAllText(RequiredPath("SF2_MAP40_REVIEW_FIXTURE")));
        var route = fixture.RootElement.GetProperty("static").GetProperty("extensionRoute").GetProperty("segments")[2];
        Require(route.GetProperty("id").GetString() == "map40-entry-to-wildcard-battle-warp" &&
            route.GetProperty("inputs").GetArrayLength() == 28, "Bounded Map40 route identity");
        for (int index = 0; index < 28; index++)
        {
            var point = route.GetProperty("points")[index];
            Require(_session.PrivateOriginalMapSnapshot.PlayerPosition == new MapPosition(point[0].GetInt32(), point[1].GetInt32()),
                $"Godot movement source point {index}: actual {_session.PrivateOriginalMapSnapshot.PlayerPosition.X},{_session.PrivateOriginalMapSnapshot.PlayerPosition.Y}; expected {point[0].GetInt32()},{point[1].GetInt32()}");
            Key key = route.GetProperty("inputs")[index].GetString() switch {
                "Up" => Key.W, "Right" => Key.D, "Down" => Key.S, "Left" => Key.A,
                _ => throw new InvalidOperationException("Unknown route input."),
            };
            await PressBattleKey(key);
            for (int frame = 0; frame < 120 && _session.PrivateOriginalMapPlayerLocomotion.IsMoving; frame++)
                await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
            Require(!_session.PrivateOriginalMapPlayerLocomotion.IsMoving, $"Movement settles within 120 physics ticks at route input {index}");
        }
        var pending = _session.PrivateOriginalBattle01Admission;
        Require(pending is not null && _session.PrivateOriginalBattle01 is null &&
            _session.PrivateOriginalMapSnapshot.PlayerPosition == new MapPosition(14, 13), "Actual Pending from Godot movement");
        Require(Field<Label>(_presenter, "_status").Text.Contains("N: start controlled diagnostic"), "Visible explicit N admission");
        await CaptureControl("01-pending");
        await PressBattleKey(Key.N);
        if (System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") == "missing-input")
        {
            var status = Field<Label>(_presenter, "_status");
            Require(_session.PrivateOriginalBattle01 is null &&
                ReferenceEquals(pending, _session.PrivateOriginalBattle01Admission) &&
                ReferenceEquals(pending!.SourceSnapshot, _session.PrivateOriginalMapSnapshot) &&
                status.Text.Contains("Prepare rejected:") && status.Text.Contains("input is unavailable") &&
                !status.Text.Contains(RequiredPath("SF2_PRIVATE_BATTLE01_DATA")), "Missing input visibly retains exact Pending without path disclosure");
            await CaptureControl("02-prepare-rejected");
            File.WriteAllText(Path.Combine(_output, "receipt.json"), JsonSerializer.Serialize(new {
                status = "Pass", scope = "controlled Map40 seed; Godot N with an explicitly selected absent data input",
                pendingAndSourceRetained = true, stage = "Prepare rejected", frames = _frames,
            }, new JsonSerializerOptions { WriteIndented = true }));
            GD.Print("SF2_BATTLE01_CONTROL_NATIVE_REVIEW Pass frames=2 missing-input");
            return;
        }
        var ready = _session.PrivateOriginalBattle01!;
        Require(ready is not null && _session.PrivateOriginalBattle01Admission is null &&
            ready.Battle.Phase == Battle01Phase.PlayerMovementSelection, "N reaches real first control");
        var battle = ready!.Battle;
        int actor = battle.FirstControl!.ActorIndex;
        var origin = battle.FirstControl.Movement.Range.Origin;
        var destination = new MapPosition(origin.X, origin.Y - 1);
        Require(battle.Roster.Count == 9 && origin == new MapPosition(9, 18) &&
            battle.FirstRound!.Slots[battle.FirstRound.CurrentTurnOffset].CombatantIndex == actor, "Actual current candidate and nine units");
        await CaptureControl("02-ready");
        if (System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") == "diagnostic")
        {
            Require(Field<PrivateBattle01Presenter>(_root, "_privateBattle01Presenter").BaseView is null,
                "No base-art request retains the explicit diagnostic mode");
            await PressBattleKey(Key.I); await PressBattleKey(Key.Space); await PressBattleKey(Key.Backspace);
            Require(_session.PrivateOriginalBattle01!.Battle.Roster.Single(unit => unit.Index == actor).Position == origin &&
                _session.PrivateOriginalBattle01.Battle.FirstRound!.CurrentTurnOffset == 0 &&
                _session.PrivateOriginalBattle01.Battle.RandomSeedImage == battle.RandomSeedImage,
                "Diagnostic movement confirm and cancel retain origin, RNG and offset");
            File.WriteAllText(Path.Combine(_output, "receipt.json"), JsonSerializer.Serialize(new {
                status = "Pass", scope = "unrequested base art; real N/I/Space/Backspace regression",
                frames = _frames, baseArt = false, actor, origin, rng = battle.RandomSeedImage }));
            GD.Print("SF2_BATTLE01_CONTROL_NATIVE_REVIEW Pass frames=2 diagnostic");
            return;
        }
        await PressBattleKey(Key.I);
        var selected = _session.PrivateOriginalBattle01!;
        Require(!ReferenceEquals(ready, selected) && selected.Battle.FirstControl!.Movement.Cursor == destination &&
            selected.Battle.Roster.Single(unit => unit.Index == actor).Position == origin &&
            selected.Battle.FirstControl.Movement.Preview.Cost == 2, "Godot I selects without relocating");
        await CaptureControl("03-selected");
        await PressBattleKey(Key.Space);
        var moved = _session.PrivateOriginalBattle01!;
        Require(moved.Battle.Phase == Battle01Phase.PlayerActionChoice &&
            moved.Battle.Roster.Single(unit => unit.Index == actor).Position == destination &&
            moved.Battle.OccupantAt(origin) == -1 && moved.Battle.OccupantAt(destination) == actor, "Godot Space provisional live relocation");
        await CaptureControl("04-provisional");
        foreach (Key key in new[] { Key.I, Key.Space, Key.W, Key.F, Key.B, Key.M, Key.N })
        {
            await PressBattleKey(key);
            Require(ReferenceEquals(moved, _session.PrivateOriginalBattle01), "Action-choice boundary and old-input isolation");
        }
        await PressBattleKey(Key.Backspace);
        var cancelled = _session.PrivateOriginalBattle01!;
        Require(cancelled.Battle.Phase == Battle01Phase.PlayerMovementSelection &&
            cancelled.Battle.Roster.Single(unit => unit.Index == actor).Position == origin &&
            cancelled.Battle.Occupancy.SequenceEqual(battle.Occupancy), "Godot Backspace restores origin and occupancy");
        await CaptureControl("05-cancelled");
        await PressBattleKey(Key.K);
        Require(ReferenceEquals(cancelled, _session.PrivateOriginalBattle01) &&
            Field<PrivateBattle01Presenter>(_root, "_privateBattle01Presenter").Projection!.Status.Contains("unreachable"),
            "Illegal cursor input explains immutable rejection");
        await CaptureControl("06-rejected");
        await PressBattleKey(Key.Backspace);
        Require(ReferenceEquals(cancelled, _session.PrivateOriginalBattle01), "Origin cancel rejects without replacement");
        await PressBattleKey(Key.I);
        await PressBattleKey(Key.Backspace);
        var final = _session.PrivateOriginalBattle01!;
        Require(final.Battle.FirstControl!.Movement.Cursor == origin && final.Battle.Occupancy.SequenceEqual(battle.Occupancy) &&
            final.Battle.RandomSeedImage == battle.RandomSeedImage && ReferenceEquals(final.Battle.FirstRound, battle.FirstRound) &&
            final.Battle.FirstRound!.CurrentTurnOffset == 0 && ReferenceEquals(final.Preparation, ready.Preparation) &&
            ReferenceEquals(final.SourceSnapshot, pending!.SourceSnapshot), "Selection cancel, RNG, order, offset and provenance retained");
        var oldCanvas = _root.GetChildren().OfType<CanvasItem>().Where(child => child is not PrivateBattle01Presenter).ToArray();
        Require(oldCanvas.Length > 0 && oldCanvas.All(child => !child.Visible),
            "All Map40, guard, overlay, HUD and synthetic canvas subtrees hidden");
        File.WriteAllText(Path.Combine(_output, "receipt.json"), JsonSerializer.Serialize(new {
            status = "Pass", scope = "controlled Map40 seed; real Godot physical-key Input events and production polling; natural Map3/H4 Unknown",
            production = new[] { "Map3Root", "PrivateBattle01Composition", "Map3InputAdapter", "PrivateBattle01Presenter", "GameSession", "PrivateOriginalBattle01StartupReader" },
            instrumentation = new[] { "existing private state factories for Map40 seed only", "Input.ParseInputEvent physical keys", "bounded ProcessFrame waits", "FramePostDraw/SavePng" },
            committedMap40Inputs = 27, pendingMap40Inputs = 1, actor, origin, destination,
            rng = battle.RandomSeedImage, offset = final.Battle.FirstRound!.CurrentTurnOffset,
            oldCanvasHidden = oldCanvas.Length, inputIsolation = "W F B M and action-choice I Space N preserve exact snapshot",
            provisionalOccupancyChecked = true, bothCancelStagesChecked = true, frames = _frames,
        }, new JsonSerializerOptions { WriteIndented = true }));
        GD.Print("SF2_BATTLE01_CONTROL_NATIVE_REVIEW Pass frames=6 controlled-Map40-seed");
    }

    private async Task PressBattleKey(Key key)
    {
        Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = key, Pressed = true });
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = key, Pressed = false });
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    private async Task CaptureControl(string name)
    {
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        using Image image = GetViewport().GetTexture().GetImage();
        Require(!image.IsEmpty() && image.SavePng(Path.Combine(_output, name + ".png")) == Error.Ok, "Control native PNG");
        if (_session.PrivateOriginalBattle01 is { } current)
        {
            var presenter = Field<PrivateBattle01Presenter>(_root, "_privateBattle01Presenter");
            var view = presenter.Projection!;
            bool baseArt = System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") != "diagnostic";
            Require(baseArt == (presenter.BaseView is not null), "Explicit base-art mode matches actual binding");
            int baseSamples = baseArt ? CheckBattleBasePixels(presenter, current, image) : 0;
            Require(view.Phase == current.Battle.Phase &&
                view.Units.All(unit => unit.Position == current.Battle.Roster.Single(row => row.Index == unit.Index).Position),
                "Visible projection matches live battle");
            var labels = presenter.GetChildren().OfType<Label>().ToArray();
            foreach (var label in labels)
                Require(label.GetLineCount() == label.GetVisibleLineCount() &&
                    label.Position.Y + label.GetMinimumSize().Y <= 540 &&
                    label.Position.X + label.GetMinimumSize().X <= 960, "All battlefield labels fit logical canvas");
            for (int left = 0; left < labels.Length; left++)
            for (int right = left + 1; right < labels.Length; right++)
                Require(!new Rect2(labels[left].Position, labels[left].Size).Intersects(
                    new Rect2(labels[right].Position, labels[right].Size)), "Battlefield text regions must not overlap");
            _frames.Add(new { name, map = current.Map.Value, phase = view.Phase.ToString(), actor = view.ActorIndex,
                cursor = view.Cursor, units = view.Units, path = view.Path, gridCost = view.GridCost, pathCost = view.PathCost,
                budget = view.Budget, status = view.Status, controls = view.Controls, baseArt, baseSamples,
                blockPixels = PrivateBattle01Presenter.TileSize, width = image.GetWidth(), height = image.GetHeight() });
        }
        else
        {
            var status = Field<Label>(_presenter, "_status");
            Require(status.GetLineCount() == status.GetVisibleLineCount(), "Pending admission text visible");
            _frames.Add(new { name, map = _session.PrivateOriginalCurrentMap.Value, phase = "Pending",
                status = status.Text, width = image.GetWidth(), height = image.GetHeight() });
        }
    }

    private static int CheckBattleBasePixels(PrivateBattle01Presenter presenter,
        PrivateOriginalBattle01SessionSnapshot snapshot, Image image)
    {
        var view = presenter.BaseView!;
        var overlay = presenter.Projection!;
        Require(ReferenceEquals(view.Definition, snapshot.Preparation.Pending.Definition) &&
            view.Definition.DestinationMap.Value == "map57" && view.Definition.DecodedLayoutDigest == OriginalBattle01AdmissionDefinition.LayoutDigest,
            "Base projection uses the current battle's fixed admission, never frozen Map40 provenance");
        Require(view.PixelWidth == 384 && view.PixelHeight == 480 && PrivateBattle01Presenter.TileSize == 24 &&
            presenter.TextureFilter == CanvasItem.TextureFilterEnum.Nearest &&
            presenter.TextureRepeat == CanvasItem.TextureRepeatEnum.Disabled, "24px full area and nearest sampling");
        int checkedPixels = 0;
        foreach (var tile in overlay.Tiles.Where(tile => !tile.Reachable &&
            !overlay.Units.Any(unit => unit.Position == tile.Position) && tile.Position != overlay.Cursor))
        {
            int x = tile.Position.X * 24 + 5, y = tile.Position.Y * 24 + 5;
            int offset = ((y * view.RasterScale * view.PixelWidth * view.RasterScale) + x * view.RasterScale) * 4;
            var expected = new Color(view.RgbaBytes[offset] / 255f, view.RgbaBytes[offset + 1] / 255f,
                view.RgbaBytes[offset + 2] / 255f, 1);
            if (expected.IsEqualApprox(new Color(0x12 / 255f, 0x18 / 255f, 0x20 / 255f))) continue;
            var origin = PrivateBattle01Presenter.GridOrigin;
            var actual = image.GetPixel((int)((origin.X + x) * image.GetWidth() / 960),
                (int)((origin.Y + y) * image.GetHeight() / 540));
            Require(actual.IsEqualApprox(expected), "Native visible Map57 texel at real 24px cell coordinate");
            checkedPixels++;
        }
        Require(checkedPixels >= 12, "Enough visible base-art samples outside live overlays");
        Color ScreenPixel(Vector2 point) => image.GetPixel((int)(point.X * image.GetWidth() / 960),
            (int)(point.Y * image.GetHeight() / 540));
        var actor = overlay.Units.Single(unit => unit.Index == overlay.ActorIndex);
        Require(ScreenPixel(PrivateBattle01Presenter.Cell(actor.Position) + Vector2.One * 3)
            .IsEqualApprox(new Color("#f2cc75")), "Current live actor outline aligns with its 24px cell");
        if (overlay.Path.Count > 1 && !overlay.Units.Any(unit => unit.Position == overlay.Path[^1]))
        {
            var end = PrivateBattle01Presenter.Cell(overlay.Path[^1]) + Vector2.One * 12;
            var previous = PrivateBattle01Presenter.Cell(overlay.Path[^2]) + Vector2.One * 12;
            Require(ScreenPixel(end + (previous - end).Normalized()).IsEqualApprox(new Color("#f2cc75")),
                "Preview path reaches the selected cell center without moving the live actor");
        }
        return checkedPixels;
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
