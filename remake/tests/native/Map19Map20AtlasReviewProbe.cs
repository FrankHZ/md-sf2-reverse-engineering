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
                Require(status.Text == "Unavailable: PrivateLocal presentation unavailable (PackageUnavailable)." &&
                    status.GetLineCount() == status.GetVisibleLineCount(), "Missing Map57 art fails the complete pack admission visibly");
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                using var failedImage = GetViewport().GetTexture().GetImage();
                Require(failedImage.SavePng(Path.Combine(_output, "01-map57-asset-rejected.png")) == Error.Ok, "Failure PNG");
                File.WriteAllText(Path.Combine(_output, "receipt.json"), JsonSerializer.Serialize(new {
                    status = "Pass", scope = "explicit base art; missing Map57 runtime payload", sessionStarted = false,
                    missingAsset = "world.map57.base-tileset-atlas", failureStage = "pack-admission",
                    fallback = false, message = status.Text, frames = 1 }));
                GD.Print("SF2_BATTLE01_CONTROL_NATIVE_REVIEW Pass frames=1 missing-atlas");
                _fixture.Dispose(); GetTree().Quit(); return;
            }
            _session = Field<GameSession>(root, "_session");
            _presenter = Field<PrivateMap3Presenter>(root, "_privatePresenter");
            if (System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") is "1" or "missing-input" or "base-art" or "diagnostic" or "stay" or "next-player" or "enemy-standby" or "first-round" or "round-continuation" or "enemy-pursuit")
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
        // Earlier STAY mode names now alias the bounded round-continuation chain.
        bool stayReview = System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") is "stay" or "next-player" or "enemy-standby" or "first-round" or "round-continuation" or "enemy-pursuit";
        if (!stayReview) await CaptureControl("01-pending");
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
            battle.FirstRound!.CurrentCandidate?.CombatantIndex == actor, "Actual current candidate and nine units");
        if (!stayReview) await CaptureControl("02-ready");
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
        if (!stayReview) await CaptureControl("03-selected");
        await PressBattleKey(Key.Space);
        var moved = _session.PrivateOriginalBattle01!;
        Require(moved.Battle.Phase == Battle01Phase.PlayerActionChoice &&
            moved.Battle.Roster.Single(unit => unit.Index == actor).Position == destination &&
            moved.Battle.OccupantAt(origin) == -1 && moved.Battle.OccupantAt(destination) == actor, "Godot Space provisional live relocation");
        if (!stayReview) await CaptureControl("04-provisional");
        if (stayReview)
        {
            // A separate physical press commits STAY; the first press only confirmed movement.
            await PressBattleKey(Key.Space);
            var next = _session.PrivateOriginalBattle01!;
            var firstReceipt = next.Battle.TurnCompletion!;
            Require(next.Battle.Phase == Battle01Phase.PlayerMovementSelection &&
                next.Battle.FirstControl?.ActorIndex == 2 && firstReceipt.CompletedActorIndex == actor && actor == 1,
                "First STAY automatically enters the actual next player once");
            var nextControl = next.Battle.FirstControl!;
            Require(next.Battle.FirstRound!.CurrentTurnOffset == 2 &&
                next.Battle.FirstRound.CurrentCandidate == battle.FirstRound!.Slots[1] &&
                ReferenceEquals(next.Battle.FirstRound.Slots, battle.FirstRound.Slots) &&
                nextControl.Movement.Range.Profile == Battle01MovementProfile.Centaur &&
                nextControl.Movement.Range.Budget == next.Battle.Roster[2].Stats.Move * 2 &&
                nextControl.Movement.Range.Budget == 14 && nextControl.Movement.Range.Origin == new MapPosition(7, 18),
                "Actual class1 Centaur uses effective MOV and the retained order");
            Require(nextControl.CandidateWordSupplied && next.Battle.Roster[2].AiBitfield == 0 &&
                moved.Battle.Roster[2].AiBitfield is null && next.Battle.Roster[0].AiBitfield is null &&
                ReferenceEquals(next.Battle.Occupancy, moved.Battle.Occupancy) &&
                ReferenceEquals(nextControl.Movement.Range.OriginOccupancy, moved.Battle.Occupancy) &&
                next.Battle.Roster[1].Position == destination &&
                firstReceipt.BeforeAfterTurn == new Battle01FactionCounts(3, 6) &&
                firstReceipt.AfterAfterTurn == new Battle01FactionCounts(3, 6),
                "Candidate-only supplement and current occupancy preserve the first completed move");
            await PressBattleKey(Key.I); await PressBattleKey(Key.Space);
            var secondMoved = _session.PrivateOriginalBattle01!;
            Require(secondMoved.Battle.Phase == Battle01Phase.PlayerActionChoice &&
                secondMoved.Battle.Roster[2].Position == new MapPosition(7, 17) &&
                secondMoved.Battle.Roster[1].Position == destination &&
                ReferenceEquals(firstReceipt, secondMoved.Battle.TurnCompletion), "Second player's independent provisional movement");
            await PressBattleKey(Key.Backspace);
            var secondCancelled = _session.PrivateOriginalBattle01!;
            Require(secondCancelled.Battle.Roster[2].Position == new MapPosition(7, 18) &&
                secondCancelled.Battle.Roster[1].Position == destination &&
                secondCancelled.Battle.Occupancy.SequenceEqual(next.Battle.Occupancy) &&
                ReferenceEquals(firstReceipt, secondCancelled.Battle.TurnCompletion), "Second cancel preserves the first STAY");
            await PressBattleKey(Key.I); await PressBattleKey(Key.Space); await PressBattleKey(Key.Space);
            var bowie = _session.PrivateOriginalBattle01!; var bowieBattle = bowie.Battle;
            Require(bowieBattle.Phase == Battle01Phase.PlayerMovementSelection && bowieBattle.FirstControl?.ActorIndex == 0 &&
                bowieBattle.FirstRound!.CurrentTurnOffset == 16 && bowieBattle.FirstRound.CurrentCandidate?.CombatantIndex == 0 &&
                bowieBattle.FirstControl.Movement.Range.Profile == Battle01MovementProfile.Regular &&
                bowieBattle.FirstControl.Movement.Range.Budget == 12 && bowieBattle.Roster[0].Position == new MapPosition(8, 18),
                "Finite enemy relay hands actual Bowie his class0 Regular1 range without making his decision");
            var completedPrefix = new List<Battle01TurnCompletionReceipt>();
            for (var receipt = bowieBattle.TurnCompletion; receipt is not null; receipt = receipt.Previous) completedPrefix.Add(receipt);
            completedPrefix.Reverse();
            Require(completedPrefix.Select(receipt => receipt.CompletedActorIndex).SequenceEqual(new[] { 1,2,128,131,133,129,130,132 }) &&
                ReferenceEquals(completedPrefix[0], firstReceipt), "Eight ordered receipts retain both player turns");
            var enemyDecisions = completedPrefix.Skip(2).Select(receipt => receipt.EnemyStandby!).ToArray();
            Require(enemyDecisions.All(decision => decision is not null) && enemyDecisions.SelectMany(decision => decision.Rolls).Sum(roll => roll.GeneratorSteps) == 1483 &&
                bowieBattle.Roster.Skip(3).Select(unit => unit.Position).SequenceEqual(new MapPosition[] { new(6,3),new(10,4),new(6,5),new(8,4),new(9,6),new(6,6) }) &&
                bowieBattle.AiMemory.Take(6).SequenceEqual(new byte[] { 0x14,0x34,0x24,0x24,0x24,0x24 }) &&
                bowieBattle.AiMemory.Skip(6).All(value => value == 0) && bowieBattle.RandomSeedCopy == 0x0134 &&
                bowieBattle.RandomSeedImage == 0xA4991234 && bowieBattle.NewlyTestedRegionMask == 0 &&
                bowieBattle.RegionFlags90Through105.All(flag => !flag) && bowieBattle.AiLastTargets.All(value => value == 255) &&
                ReferenceEquals(bowieBattle.FirstRound!.Slots, battle.FirstRound!.Slots) &&
                bowieBattle.Roster.Select((unit, i) => ReferenceEquals(unit.Stats, battle.Roster[i].Stats)).All(value => value),
                "All six independent standby traces preserve main RNG, effective stats, order and first enemy memory");
            Require(secondCancelled.Battle.Roster[0].AiBitfield is null && bowieBattle.Roster[0].AiBitfield == 0 &&
                bowieBattle.FirstControl!.CandidateWordSupplied && ReferenceEquals(bowie.Preparation, ready.Preparation), "Only current Bowie receives the missing word");
            await PressBattleKey(Key.I); await PressBattleKey(Key.Space); await PressBattleKey(Key.Space);
            var round2 = _session.PrivateOriginalBattle01!; var second = round2.Battle;
            Require(second.FirstRound!.RoundNumber == 2 && second.FirstRound.CurrentTurnOffset == 0 &&
                second.FirstControl?.ActorIndex == 2 && second.Phase == Battle01Phase.PlayerMovementSelection &&
                second.RandomSeedImage == 0xAA861234 && second.RandomSeedCopy == 0x0134 &&
                second.TurnCompletion?.RoundNumber == 1 && second.TurnCompletion.CompletedActorIndex == 0 &&
                ReferenceEquals(second.TurnCompletion.Previous, bowieBattle.TurnCompletion) &&
                second.Roster[0].Position == new MapPosition(8,17) && second.NewlyTestedRegionMask == 7,
                "Actual sentinel generates round2 once and yields its real player without a player decision");
            CheckRoundBuffer(second, new byte[] {2,128,132,131,133,0,1,129,130}, new byte[] {7,6,6,5,5,4,4,4,4});
            if (System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") == "enemy-pursuit")
            {
                await ReviewEnemyPursuit(round2,battle);
                return;
            }
            await CaptureControl("01-round2-ready");
            await PressBattleKey(Key.I); await PressBattleKey(Key.Space);
            var provisional = _session.PrivateOriginalBattle01!;
            Require(provisional.Battle.Phase == Battle01Phase.PlayerActionChoice &&
                provisional.Battle.Roster[2].Position == new MapPosition(7,16) &&
                ReferenceEquals(second.TurnCompletion, provisional.Battle.TurnCompletion), "Round2 player chooses independent provisional movement");
            await CaptureControl("02-round2-provisional");
            await PressBattleKey(Key.Backspace);
            var cancelled2 = _session.PrivateOriginalBattle01!;
            Require(cancelled2.Battle.Roster[2].Position == new MapPosition(7,17) &&
                cancelled2.Battle.Occupancy.SequenceEqual(second.Occupancy) &&
                ReferenceEquals(second.TurnCompletion,cancelled2.Battle.TurnCompletion) &&
                ReferenceEquals(second.FirstRound,cancelled2.Battle.FirstRound), "Round2 cancel restores its current origin and retains round1");
            await CaptureControl("03-round2-cancelled");
            // These physical presses are explicit origin confirm/STAY decisions for each actual player.
            foreach (int player in new[] {2,0,1})
            {
                Require(_session.PrivateOriginalBattle01!.Battle.FirstControl?.ActorIndex == player, "Actual player ready before manual choice");
                await PressBattleKey(Key.Space);
                Require(_session.PrivateOriginalBattle01!.Battle.Phase == Battle01Phase.PlayerActionChoice, "Origin confirmation does not complete a turn");
                await PressBattleKey(Key.Space);
            }
            var round3 = _session.PrivateOriginalBattle01!; var end = round3.Battle;
            Require(end.FirstRound!.RoundNumber == 3 && end.FirstRound.CurrentTurnOffset == 0 &&
                end.FirstControl?.ActorIndex == 2 && end.RandomSeedImage == 0x9BD71234 && end.RandomSeedCopy == 0x0034 &&
                end.Roster.Take(3).Select(unit=>unit.Position).SequenceEqual(new MapPosition[] {new(8,17),new(9,17),new(7,17)}) &&
                end.AiMemory.Take(6).SequenceEqual(new byte[] {4,4,4,0x24,0x34,4}) &&
                end.AiMemory.Skip(6).All(value=>value==0) && end.AiLastTargets.All(value=>value==255) &&
                end.RegionFlags90Through105.All(flag=>!flag) && end.NewlyTestedRegionMask == 7,
                "Actual round3 player retains the composed round2 result without automated player action");
            CheckRoundBuffer(end, new byte[] {2,129,130,131,128,133,0,1,132}, new byte[] {8,6,6,6,5,5,4,4,4});
            var history = new List<Battle01TurnCompletionReceipt>();
            for (var receipt=end.TurnCompletion;receipt is not null;receipt=receipt.Previous) history.Add(receipt);
            history.Reverse();
            Require(history.Count==18 && history.Take(9).All(receipt=>receipt.RoundNumber==1) &&
                history.Skip(9).All(receipt=>receipt.RoundNumber==2) &&
                history.Skip(9).Select(receipt=>receipt.CompletedActorIndex).SequenceEqual(second.FirstRound!.Slots.Take(9).Select(slot=>(int)slot.CombatantIndex)),
                "All eighteen receipts preserve round numbers and actual generation order");
            var secondDecisions=history.Skip(9).Where(receipt=>receipt.EnemyStandby is not null).Select(receipt=>receipt.EnemyStandby!).ToArray();
            Require(secondDecisions.Sum(decision=>decision.Rolls.Sum(roll=>roll.GeneratorSteps))==967 &&
                secondDecisions.Sum(decision=>decision.Rolls.Count)==11 &&
                secondDecisions.Select(decision=>decision.Destination).SequenceEqual(new MapPosition[] {new(7,2),new(10,5),new(8,4),new(6,4),new(9,3),new(6,3)}) &&
                end.Roster.Select((unit,i)=>ReferenceEquals(unit.Stats,battle.Roster[i].Stats) && ReferenceEquals(unit.Deployment,battle.Roster[i].Deployment)).All(value=>value),
                "Round2 thinking and original deployment anchors retain effective stats without reinitialization");
            var hidden = _root.GetChildren().OfType<CanvasItem>().Where(child => child is not PrivateBattle01Presenter).ToArray();
            Require(hidden.Length > 0 && hidden.All(child => !child.Visible), "Old canvas subtrees remain hidden throughout continuation");
            await CaptureControl("04-round3-ready");
            CheckContinuationDispatchBranches(round2,round3);
            File.WriteAllText(Path.Combine(_output, "receipt.json"), JsonSerializer.Serialize(new {
                status = "Pass", scope = "controlled Map40 seed; physical player keys through round2 and actual round3 control",
                completedActors = history.Select(receipt=>receipt.CompletedActorIndex).ToArray(),
                receiptRounds = history.Select(receipt=>receipt.RoundNumber).ToArray(),
                round2Order=second.FirstRound.Slots, round3Order=end.FirstRound.Slots,
                round2Main=second.RandomSeedImage, round3Main=end.RandomSeedImage,
                enemyDecisions, secondDecisions, seedCopy=end.RandomSeedCopy, memory=end.AiMemory,
                byteOffset=end.FirstRound.CurrentTurnOffset, nextCandidate=end.FirstControl!.ActorIndex,
                nextStarted=true, inputClosed=false, oldCanvasHidden=hidden.Length, frames=_frames,
                authoredChecks="isolated copied round2 order starts enemy128; late control and enemy failures retain state without retry; reachable region1 now admits actual round3 player2; original round3 snapshot restored",
            }, new JsonSerializerOptions { WriteIndented = true }));
            GD.Print($"SF2_BATTLE01_CONTROL_NATIVE_REVIEW Pass frames={_frames.Count} round-continuation");

            return;
        }
        foreach (Key key in new[] { Key.I, Key.W, Key.F, Key.B, Key.M, Key.N })
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
            oldCanvasHidden = oldCanvas.Length, inputIsolation = "W F B M and action-choice I N preserve exact snapshot",
            provisionalOccupancyChecked = true, bothCancelStagesChecked = true, frames = _frames,
        }, new JsonSerializerOptions { WriteIndented = true }));
        GD.Print("SF2_BATTLE01_CONTROL_NATIVE_REVIEW Pass frames=6 controlled-Map40-seed");
    }


    private async Task ReviewEnemyPursuit(PrivateOriginalBattle01SessionSnapshot round2, Battle01InitializedState initialBattle)
    {
        var presenter=Field<PrivateBattle01Presenter>(_root,"_privateBattle01Presenter");
        var rounds=new List<Battle01InitializedState> {initialBattle,round2.Battle};
        async Task OriginStay(int player)
        {
            Require(_session.PrivateOriginalBattle01!.Battle.FirstControl?.ActorIndex==player,"Actual player awaits explicit key choices");
            await PressBattleKey(Key.Space);
            Require(_session.PrivateOriginalBattle01!.Battle.Phase==Battle01Phase.PlayerActionChoice,"Confirm remains provisional");
            await PressBattleKey(Key.Space);
        }
        static Battle01TurnCompletionReceipt[] History(Battle01InitializedState b)
        {
            var receipts=new List<Battle01TurnCompletionReceipt>();
            for(var receipt=b.TurnCompletion;receipt is not null;receipt=receipt.Previous) receipts.Add(receipt);
            receipts.Reverse(); return receipts.ToArray();
        }
        await OriginStay(2);
        Require(_session.PrivateOriginalBattle01!.Battle.FirstControl?.ActorIndex==0,"Round2 actual Bowie after inactive prefix");
        var bowie=_session.PrivateOriginalBattle01!;
        foreach(Key key in new[] {Key.L,Key.L,Key.L,Key.I,Key.I}) await PressBattleKey(key);
        Require(_session.PrivateOriginalBattle01!.Battle.FirstControl!.Movement.Cursor==new MapPosition(11,15) &&
            _session.PrivateOriginalBattle01.Battle.FirstControl.Movement.GridCost==10,"Physical cursor enters region1 within budget12");
        await PressBattleKey(Key.Space); await PressBattleKey(Key.Backspace);
        Require(_session.PrivateOriginalBattle01!.Battle.Roster[0].Position==new MapPosition(8,17) &&
            ReferenceEquals(bowie.Battle.TurnCompletion,_session.PrivateOriginalBattle01.Battle.TurnCompletion) &&
            bowie.Battle.Occupancy.SequenceEqual(_session.PrivateOriginalBattle01.Battle.Occupancy),"Round2 Bowie cancel preserves the exact completed prefix");
        foreach(Key key in new[] {Key.L,Key.L,Key.L,Key.I,Key.I}) await PressBattleKey(key);
        await PressBattleKey(Key.Space); await PressBattleKey(Key.Space);
        await OriginStay(1);
        var round3=_session.PrivateOriginalBattle01!; var third=round3.Battle; rounds.Add(third);
        Require(third.FirstRound!.RoundNumber==3 && third.FirstControl?.ActorIndex==2 &&
            third.RegionFlags90Through105.Take(3).SequenceEqual(new[] {false,true,false}) && third.NewlyTestedRegionMask==7 &&
            third.Roster.Skip(3).Select(unit=>unit.AiBitfield).SequenceEqual(new ushort?[] {0x2060,0x2060,0x2060,0x2061,0x2071,0x2070}) &&
            third.Roster[0].Position==new MapPosition(11,15) && third.RandomSeedImage==0x9BD71234 && third.RandomSeedCopy==0x0034,
            "Physical round2 region entry admits the actual activated round3");
        CheckRoundBuffer(third,[2,129,130,131,128,133,0,1,132],[8,6,6,6,5,5,4,4,4]);
        await CaptureControl("01-round3-ready");

        // Test-only copied snapshot and direct Application steps expose the otherwise synchronous intermediate view.
        // Restore the exact physical-route snapshot before any further Godot key event. No production pause hook.
        var instrumented=(PrivateOriginalBattle01SessionSnapshot)Activator.CreateInstance(
            typeof(PrivateOriginalBattle01SessionSnapshot), BindingFlags.Instance | BindingFlags.NonPublic,
            null, new object?[] {round3.Preparation,third,round3.SourceLocomotion,round3.SourceBridge}, null)!;
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session,instrumented);
        Battle01EnemyPursuitDecision inspected;
        try
        {
            var confirmed=(PrivateOriginalBattle01PlayerMovementApplied)_session.ConfirmPrivateOriginalBattle01PlayerMovement(instrumented,2);
            _session.CommitPrivateOriginalBattle01Stay(confirmed.Snapshot,2);
            foreach(int enemy in new[] {129,130})
            {
                Require(_session.PrivateOriginalBattle01!.Battle.FirstRound!.CurrentCandidate?.CombatantIndex==enemy,"Copied branch preserves actual inactive prefix");
                Require(_session.CompletePrivateOriginalBattle01EnemyStandby(_session.PrivateOriginalBattle01,enemy) is
                    PrivateOriginalBattle01EnemyStandbyCompleted,"Copied inactive step completes");
            }
            Require(_session.CompletePrivateOriginalBattle01EnemyPursuit(_session.PrivateOriginalBattle01,131) is
                PrivateOriginalBattle01EnemyPursuitCompleted,"Copied first131 pursuit completes");
            var completed=_session.PrivateOriginalBattle01!.Battle; inspected=completed.TurnCompletion!.EnemyPursuit!;
            Require(inspected.PreliminaryDestination==new MapPosition(9,5) && inspected.Destination==new MapPosition(9,4) &&
                inspected.MoveString.SequenceEqual(new byte[] {0,255}) && inspected.GridCost==2 && completed.FirstRound!.CurrentTurnOffset==8,
                "Copied first131 uses the occupied-cell fallback");
            presenter.Project(completed,"TEST COPY: after first E3 pursuit. Direct Application steps; physical route resumes from saved round3.");
            await CaptureControl("02-first131-test-copy","test-only copied round3 snapshot; direct Application steps; not physical-key progression or original timing");
        }
        finally
        {
            typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session,round3);
            presenter.Project(third,"Round 3. Player 2 ready.");
        }
        Require(ReferenceEquals(round3,_session.PrivateOriginalBattle01),"Exact physical route restored after instrumentation");
        await OriginStay(2);
        var physicalAfterPrefix=_session.PrivateOriginalBattle01!;
        var actual131=History(physicalAfterPrefix.Battle).Single(receipt=>receipt.RoundNumber==3 && receipt.CompletedActorIndex==131).EnemyPursuit!;
        Require(JsonSerializer.Serialize(actual131)==JsonSerializer.Serialize(inspected) &&
            physicalAfterPrefix.Battle.FirstControl?.ActorIndex==0,"Physical synchronous dispatch reproduces the copied first131 decision");
        await OriginStay(0); await OriginStay(1);
        var round4=_session.PrivateOriginalBattle01!; var fourth=round4.Battle; rounds.Add(fourth);
        Require(fourth.FirstRound!.RoundNumber==4 && fourth.FirstControl?.ActorIndex==1 && fourth.RandomSeedImage==0x51DC1234 &&
            fourth.RandomSeedCopy==0x0234 && fourth.AiMemory.Take(6).SequenceEqual(new byte[] {0x14,0x24,0x14,0x24,0x34,0x34}) &&
            fourth.Roster[6].Position==new MapPosition(9,4) && fourth.Roster[7].Position==new MapPosition(11,6),
            "Both physical active pursuits finish and actual round4 player1 receives control");
        CheckRoundBuffer(fourth,[1,2,130,132,129,131,0,128,133],[6,6,6,6,5,5,4,4,4]);
        await CaptureControl("03-round4-ready");
        foreach(int player in new[] {1,2,0}) await OriginStay(player);
        var fifth=_session.PrivateOriginalBattle01!.Battle; rounds.Add(fifth);
        Require(fifth.FirstRound!.RoundNumber==5 && fifth.RandomSeedImage==0xDE251234 && fifth.RandomSeedCopy==0x0234 &&
            fifth.Roster[6].Position==new MapPosition(10,5) && fifth.Roster[7].Position==new MapPosition(11,8),"Round4 pursuit remains repeatable");
        CheckRoundBuffer(fifth,[1,2,131,133,129,132,0,128,130],[6,6,6,6,5,5,4,4,4]);
        foreach(int player in new[] {1,2,0}) await OriginStay(player);
        var sixth=_session.PrivateOriginalBattle01!.Battle; rounds.Add(sixth);
        Require(sixth.FirstRound!.RoundNumber==6 && sixth.RandomSeedImage==0x07821234 && sixth.RandomSeedCopy==0x5634 &&
            sixth.Roster[6].Position==new MapPosition(11,6) && sixth.Roster[7].Position==new MapPosition(11,10),"Round5 pursuit reaches the next physical eligibility seam");
        CheckRoundBuffer(sixth,[2,129,130,1,128,132,0,131,133],[7,6,6,5,5,5,4,4,4]);
        await OriginStay(2); await OriginStay(1);
        var stopped=_session.PrivateOriginalBattle01!; var end=stopped.Battle; var history=History(end);
        Require(end.FirstControl is null && end.FirstRound!.RoundNumber==6 && end.FirstRound.CurrentTurnOffset==10 &&
            end.FirstRound.CurrentCandidate?.CombatantIndex==132 && end.TurnCompletion?.CompletedActorIndex==128 &&
            history.Length==50 && history.Count(receipt=>receipt.EnemyPursuit is not null)==6 && end.RandomSeedImage==0x07821234 &&
            end.RandomSeedCopy==0x0034 && end.AiMemory.Take(6).SequenceEqual(new byte[] {4,0x34,0x24,0x24,0x34,0x24}) &&
            end.NewlyTestedRegionMask==0 && end.Roster[7].Position==new MapPosition(11,10),"Attack selection retains the actual fifty-receipt boundary");
        Require(presenter.Projection!.Status.Contains("Enemy 132: attack selection required") &&
            presenter.Projection.NextCandidateIndex==132 && presenter.Projection.CompletedActorIndex==128 &&
            !presenter.Projection.CanConfirm && presenter.Projection.Controls.Contains("Input closed"),"Visible attack-required boundary closes player input");
        Require(_session.CompletePrivateOriginalBattle01EnemyPursuit(stopped,132) is PrivateOriginalBattle01AttackSelectionRequired typed &&
            typed.Targets.SequenceEqual(new[] {new Battle01AttackCandidate(0,new(11,14),8)}) &&
            ReferenceEquals(stopped,_session.PrivateOriginalBattle01),"Explicit typed rejection independently names the eligible cohort without mutation");
        string visibleStatus=presenter.Projection.Status;
        foreach(Key key in new[] {Key.I,Key.L,Key.Space,Key.Backspace,Key.N,Key.W,Key.F,Key.B,Key.M}) await PressBattleKey(key);
        for(int frame=0;frame<12;frame++) await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
        Require(ReferenceEquals(stopped,_session.PrivateOriginalBattle01) && presenter.Projection.Status==visibleStatus,
            "Frames and unrelated keys never retry or overwrite the rejection");
        await CaptureControl("04-round6-attack-required");
        var steps=Enumerable.Range(1,6).Select(number=>history.Where(receipt=>receipt.RoundNumber==number)
            .Sum(receipt=>receipt.EnemyStandby?.Rolls.Sum(roll=>roll.GeneratorSteps) ?? 0)).ToArray();
        Require(steps.SequenceEqual(new[] {1483,967,958,512,380,198}),"All completed thinking-byte counts match the independent reduction");
        Require(end.AiMemory.Skip(6).All(value=>value==0) && end.AiLastTargets.All(value=>value==255) &&
            end.Roster.Select((unit,i)=>ReferenceEquals(unit.Stats,initialBattle.Roster[i].Stats) && ReferenceEquals(unit.Deployment,initialBattle.Roster[i].Deployment)).All(value=>value),
            "Pursuit preserves effective stats, original anchors and unused memory");
        File.WriteAllText(Path.Combine(_output,"receipt.json"),JsonSerializer.Serialize(new {
            status="Pass",scope="controlled Map40 seed; physical Godot keys through round6 attack-required boundary",
            instrumentation="frame02 uses a copied round3 snapshot and direct Application steps; exact snapshot restored before physical-key continuation; first131 decisions compared",
            rounds=rounds.Select(b=>new {number=b.FirstRound!.RoundNumber,slots=b.FirstRound.Slots,main=b.RandomSeedImage,copy=b.RandomSeedCopy,
                flags=b.RegionFlags90Through105,words=b.Roster.Skip(3).Select(unit=>unit.AiBitfield),tested=b.NewlyTestedRegionMask}),
            turns=history.Select(receipt=>new {round=receipt.RoundNumber,actor=receipt.CompletedActorIndex,standby=receipt.EnemyStandby,pursuit=receipt.EnemyPursuit}),
            thinkingSteps=steps,main=end.RandomSeedImage,copy=end.RandomSeedCopy,memory=end.AiMemory,lastTargets=end.AiLastTargets,
            occupancy=end.Roster.Select(unit=>new {actor=unit.Index,position=unit.Position}),byteOffset=end.FirstRound!.CurrentTurnOffset,
            nextCandidate=132,inputClosed=true,frames=_frames,
        },new JsonSerializerOptions {WriteIndented=true}));
        GD.Print($"SF2_BATTLE01_CONTROL_NATIVE_REVIEW Pass frames={_frames.Count} enemy-pursuit");
    }

    private static void CheckRoundBuffer(Battle01InitializedState battle, byte[] actors, byte[] scores)
    {
        var slots=battle.FirstRound!.Slots;
        Require(slots.Count==64 && slots.Take(9).Select(slot=>slot.CombatantIndex).SequenceEqual(actors) &&
            slots.Take(9).Select(slot=>slot.AlteredAgility).SequenceEqual(scores) &&
            slots.Skip(9).All(slot=>slot==new Battle01TurnEntry(255,255)), "Complete current 64-slot buffer");
    }

    private void CheckContinuationDispatchBranches(PrivateOriginalBattle01SessionSnapshot round2,
        PrivateOriginalBattle01SessionSnapshot actualRound3)
    {
        // Isolated authored copies exercise dispatch boundaries; these are not natural generated order evidence.
        static T Construct<T>(params object?[] args) => (T)Activator.CreateInstance(typeof(T),
            BindingFlags.Instance|BindingFlags.NonPublic,null,args,null)!;
        static Battle01InitializedState Copy(Battle01InitializedState b, Battle01FirstRoundOrder? order=null,
            byte[]? terrain=null,int[]? occupancy=null)
        {
            var initial=Construct<Battle01InitializedState>(b.Roster.ToArray(),b.Regions.ToArray(),terrain ?? b.Terrain.ToArray(),
                occupancy ?? b.Occupancy.ToArray(),b.RandomSeedImage,b.RandomSeedCopy);
            var thinking=Construct<Battle01InitializedState>(initial,initial.Roster.ToArray(),initial.Occupancy.ToArray(),b.AiMemory.ToArray(),b.RandomSeedCopy!.Value);
            var generation=Construct<Battle01InitializedState>(thinking,thinking.Roster.ToArray(),b.RegionFlags90Through105.ToArray(),
                b.NewlyTestedRegionMask,b.RandomSeedImage,order ?? b.FirstRound);
            return Construct<Battle01InitializedState>(generation,generation.FirstRound,b.TurnCompletion);
        }
        PrivateOriginalBattle01SessionSnapshot Install(Battle01InitializedState b)
        {
            var snapshot=Construct<PrivateOriginalBattle01SessionSnapshot>(round2.Preparation,b,round2.SourceLocomotion,round2.SourceBridge);
            typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session,snapshot);
            return snapshot;
        }
        void NoRetry(PrivateOriginalBattle01SessionSnapshot snapshot)
        {
            foreach (var input in Enum.GetValues<PrivateBattle01Input>())
                Require(PrivateBattle01Ui.Apply(_session,null,input) is null &&
                    ReferenceEquals(snapshot,_session.PrivateOriginalBattle01), "Boundary ignores all further inputs without dispatch/reroll");
        }
        var generated=Copy(round2.Battle);
        var slots=generated.FirstRound!.Slots.ToArray(); (slots[0],slots[1])=(slots[1],slots[0]);
        var enemyFirst=Install(Copy(generated,Construct<Battle01FirstRoundOrder>(slots,Array.Empty<int>(),Array.Empty<int>(),2)));
        var outcome=PrivateBattle01Ui.DispatchNext(_session,enemyFirst);
        var ready=_session.PrivateOriginalBattle01!;
        Require(outcome.Contains("Player 2 ready") && ready.Battle.FirstControl?.ActorIndex==2 &&
            ready.Battle.FirstRound!.CurrentTurnOffset==2 && ready.Battle.TurnCompletion?.CompletedActorIndex==128 &&
            ready.Battle.TurnCompletion.RoundNumber==2 && ReferenceEquals(ready.Battle.TurnCompletion.Previous,generated.TurnCompletion) &&
            ready.Battle.TurnCompletion.EnemyStandby!.MemoryBefore==0x14, "Actual authored enemy-first order dispatches enemy then yields player");

        var terrain=generated.Terrain.ToArray(); terrain[16*48+7]=16;
        var invalidControl=Install(Copy(generated,terrain:terrain));
        Require(PrivateBattle01Ui.DispatchNext(_session,invalidControl).Contains("Control rejected: terrain") &&
            ReferenceEquals(invalidControl,_session.PrivateOriginalBattle01) && invalidControl.Battle.Phase==Battle01Phase.RoundGenerated &&
            invalidControl.Battle.RandomSeedImage==0xAA861234, "Late control failure keeps the newly generated round");
        NoRetry(invalidControl);

        static Battle01InitializedState OriginStay(Battle01InitializedState b)
        {
            int actor=b.FirstRound!.CurrentCandidate!.Value.CombatantIndex;
            return actor>=128 ? Battle01EnemyStandby.CompleteNext(b,actor,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats)
                : Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(Battle01NextPlayerControl.Enter(b,actor).State!,actor),
                    actor,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        }
        var after128=OriginStay(OriginStay(generated)); var occupancy=after128.Occupancy.ToArray(); occupancy[17*48+9]=-1;
        var invalidEnemy=Install(Copy(after128,occupancy:occupancy));
        Require(PrivateBattle01Ui.DispatchNext(_session,invalidEnemy).Contains("Enemy 132 standby rejected: occupancy") &&
            ReferenceEquals(invalidEnemy,_session.PrivateOriginalBattle01) &&
            ReferenceEquals(after128.TurnCompletion,invalidEnemy.Battle.TurnCompletion), "Late enemy failure keeps prior enemy commit");
        NoRetry(invalidEnemy);

        var branch=generated;
        while (branch.FirstRound!.CurrentCandidate!.Value.CombatantIndex!=0) branch=OriginStay(branch);
        var selected=Battle01PlayerMovement.SelectDestination(Battle01NextPlayerControl.Enter(branch,0).State!,0,new(11,15));
        Require(selected.FirstControl!.Movement.GridCost==10, "Real terrain admits Bowie region1 destination from current origin");
        branch=Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(selected,0),0,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        while (branch.FirstRound!.CurrentCandidate is not null) branch=OriginStay(branch);
        var regionBoundary=Install(branch);
        Require(PrivateBattle01Ui.DispatchNext(_session,regionBoundary).Contains("Player 2 ready") &&
            _session.PrivateOriginalBattle01!.Battle.FirstRound!.RoundNumber==3 &&
            _session.PrivateOriginalBattle01.Battle.RegionFlags90Through105[1] &&
            _session.PrivateOriginalBattle01.Battle.Roster[6].AiBitfield==0x2061 &&
            _session.PrivateOriginalBattle01.Battle.Roster[7].AiBitfield==0x2071 &&
            _session.PrivateOriginalBattle01.Battle.RandomSeedImage==0x9BD71234 &&
            branch.FirstRound!.RoundNumber==2 && branch.RegionFlags90Through105.All(flag=>!flag) &&
            branch.RandomSeedImage==0xAA861234, "Reachable primary activation installs one new round and actual player control");
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session,actualRound3);
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

    private async Task CaptureControl(string name, string provenance="physical Godot key events from controlled Map40 seed")
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
                round = current.Battle.FirstRound?.RoundNumber, completedActor = view.CompletedActorIndex, nextCandidate = view.NextCandidateIndex,
                cursor = view.Cursor, units = view.Units, path = view.Path, gridCost = view.GridCost, pathCost = view.PathCost,
                budget = view.Budget, status = view.Status, controls = view.Controls, baseArt, baseSamples,
                blockPixels = PrivateBattle01Presenter.TileSize, width = image.GetWidth(), height = image.GetHeight(),
                provenance, pursuitCompleted=view.PursuitCompleted });
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
        if (overlay.ActorIndex is { } actorIndex)
        {
            var actor = overlay.Units.Single(unit => unit.Index == actorIndex);
            Require(ScreenPixel(PrivateBattle01Presenter.Cell(actor.Position) + Vector2.One * 3)
                .IsEqualApprox(new Color("#f2cc75")), "Current live actor outline aligns with its 24px cell");
        }
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
