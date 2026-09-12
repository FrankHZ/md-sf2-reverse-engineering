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
    private PrivateOriginalBattle01SessionSnapshot? _initialBattle01;
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
            if (System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") is "1" or "missing-input" or "base-art" or "diagnostic" or "stay" or "next-player" or "enemy-standby" or "first-round" or "round-continuation" or "enemy-pursuit" or "enemy-physical-attack" or "player-physical-attack" or "first-enemy-defeat" or "chester-enemy-hit" or "chester-player-attack" or "second-enemy-defeat" or "first-ally-defeat" or "leader-defeat-pending" or "chester-counterattack")
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
        bool stayReview = System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") is "stay" or "next-player" or "enemy-standby" or "first-round" or "round-continuation" or "enemy-pursuit" or "enemy-physical-attack" or "player-physical-attack" or "first-enemy-defeat" or "chester-enemy-hit" or "chester-player-attack" or "second-enemy-defeat" or "first-ally-defeat" or "leader-defeat-pending" or "chester-counterattack";
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
        _initialBattle01 = ready;
        Require(ready!.Preparation.ReturnInputs == OriginalBattle01ControlledReturnInputs.GransealFirstAttemptComparison &&
            ready.Battle.ReturnAdmission is not null && ready.Battle.BattleEntryFlag399,
            "N explicitly selected the return comparison before source admission and first control");
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
            if (System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") is "enemy-pursuit" or "enemy-physical-attack" or "player-physical-attack" or "first-enemy-defeat" or "chester-enemy-hit" or "chester-player-attack" or "second-enemy-defeat" or "first-ally-defeat" or "leader-defeat-pending" or "chester-counterattack")
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
        bool physical = System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") == "enemy-physical-attack";
        bool playerPhysical = System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") is "player-physical-attack" or "first-enemy-defeat" or "chester-enemy-hit" or "chester-player-attack" or "second-enemy-defeat" or "first-ally-defeat" or "leader-defeat-pending" or "chester-counterattack";
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
        if (!physical && !playerPhysical) await CaptureControl("01-round3-ready");

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
            if (!physical && !playerPhysical) await CaptureControl("02-first131-test-copy","test-only copied round3 snapshot; direct Application steps; not physical-key progression or original timing");
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
        if (!physical && !playerPhysical) await CaptureControl("03-round4-ready");
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
        await OriginStay(2);
        var beforeAttack=_session.PrivateOriginalBattle01!;
        Require(beforeAttack.Battle.FirstControl?.ActorIndex==1,"Actual round6 player1 precedes inactive128 and attack132");
        if (physical) await CaptureControl("01-round6-player1-ready");
        var copied=(PrivateOriginalBattle01SessionSnapshot)Activator.CreateInstance(
            typeof(PrivateOriginalBattle01SessionSnapshot),BindingFlags.Instance|BindingFlags.NonPublic,null,
            new object?[] {beforeAttack.Preparation,beforeAttack.Battle,beforeAttack.SourceLocomotion,beforeAttack.SourceBridge},null)!;
        Battle01EnemyPhysicalAttackDecision inspectedAttack;
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session,copied);
        try
        {
            var confirm=(PrivateOriginalBattle01PlayerMovementApplied)_session.ConfirmPrivateOriginalBattle01PlayerMovement(copied,1);
            Require(_session.CommitPrivateOriginalBattle01Stay(confirm.Snapshot,1) is PrivateOriginalBattle01StayCommitted,"Copied player1 STAY");
            Require(_session.CompletePrivateOriginalBattle01EnemyStandby(_session.PrivateOriginalBattle01,128) is
                PrivateOriginalBattle01EnemyStandbyCompleted,"Copied actual inactive128");
            var boundary=_session.PrivateOriginalBattle01!;
            Require(boundary.Battle.FirstRound!.CurrentTurnOffset==10 && History(boundary.Battle).Length==50 &&
                _session.CompletePrivateOriginalBattle01EnemyPursuit(boundary,132) is PrivateOriginalBattle01AttackSelectionRequired typed &&
                typed.Targets.SequenceEqual(new[] {new Battle01AttackCandidate(0,new(11,14),8)}) &&
                ReferenceEquals(boundary,_session.PrivateOriginalBattle01),"Pursuit API retains independent physical-cohort boundary");
            Require(_session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(boundary,132) is
                PrivateOriginalBattle01EnemyPhysicalAttackCompleted,"Copied physical attack completes");
            var completed=_session.PrivateOriginalBattle01!.Battle; inspectedAttack=completed.TurnCompletion!.EnemyPhysicalAttack!;
            Require(completed.FirstRound!.CurrentTurnOffset==12 && History(completed).Length==51 &&
                completed.Roster[0].Stats.HpCurrent==9,"Copied attack replay and completion precede next-player admission");
            presenter.Project(completed,"TEST COPY: attack132 complete. Physical route resumes from saved round6 player1.");
            if (physical) await CaptureControl("02-attack132-test-copy","test-only copied round6 snapshot; direct Application STAY/128/132; not physical-key progression or original timing");
        }
        finally
        {
            typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session,beforeAttack);
            presenter.Project(beforeAttack.Battle,"Restored exact physical-route round6 player1; next STAY is manual.");
        }
        await OriginStay(1);
        var ready=_session.PrivateOriginalBattle01!; var end=ready.Battle; var history=History(end);
        var actualAttack=end.TurnCompletion!.EnemyPhysicalAttack!;
        Require(end.FirstControl?.ActorIndex==0 && end.FirstRound!.CurrentTurnOffset==12 && history.Length==51 &&
            end.RandomSeedImage==0xAF881234 && end.RandomSeedCopy==0x0134 && end.Roster[0].Stats.HpCurrent==9 &&
            end.Roster[7].Position==new MapPosition(11,14) && end.AiLastTargets[4]==0 &&
            JsonSerializer.Serialize(inspectedAttack)==JsonSerializer.Serialize(actualAttack),"Physical relay matches copied attack and hands actual Bowie control");
        Require(presenter.Projection!.ActorIndex==0 && presenter.Projection.CanConfirm &&
            presenter.Projection.AttackResult!.Contains("hit 3") && presenter.Projection.AttackResult.Contains("HP 12 -> 9"),
            "Player controls and persistent physical result are visible together");
        string visibleStatus=presenter.Projection.Status;
        // N is a recognized Enter command and may explain that this battle is already initialized.
        foreach(Key key in new[] {Key.W,Key.F,Key.B,Key.M}) await PressBattleKey(key);
        for(int frame=0;frame<12;frame++) await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
        Require(ReferenceEquals(ready,_session.PrivateOriginalBattle01) && presenter.Projection.Status==visibleStatus,
            "Frames and unrelated keys never repeat the physical attack");
        if (!playerPhysical) await CaptureControl(physical ? "03-bowie-ready-hp9" : "04-round6-bowie-hp9");
        if (physical)
        {
            await PressBattleKey(Key.L); await PressBattleKey(Key.Space);
            Require(_session.PrivateOriginalBattle01!.Battle.Roster[0].Position==new MapPosition(12,15) &&
                _session.PrivateOriginalBattle01.Battle.Roster[0].Stats.HpCurrent==9,"Physical Bowie relocation retains damage");
            await PressBattleKey(Key.Backspace);
            Require(_session.PrivateOriginalBattle01!.Battle.Roster[0].Position==new MapPosition(11,15) &&
                _session.PrivateOriginalBattle01.Battle.Roster[0].Stats.HpCurrent==9 &&
                ReferenceEquals(end.TurnCompletion,_session.PrivateOriginalBattle01.Battle.TurnCompletion) &&
                end.Occupancy.SequenceEqual(_session.PrivateOriginalBattle01.Battle.Occupancy),"Bowie cancel restores origin without healing or undoing attack");
            await CaptureControl("04-bowie-cancel-hp9");
        }
        var steps=Enumerable.Range(1,6).Select(number=>history.Where(receipt=>receipt.RoundNumber==number)
            .Sum(receipt=>receipt.EnemyStandby?.Rolls.Sum(roll=>roll.GeneratorSteps) ?? 0)).ToArray();
        Require(steps.SequenceEqual(new[] {1483,967,958,512,380,198}),"All completed thinking-byte counts match the independent reduction");
        Require(actualAttack.Priorities.Sum(p=>p.Roll.GeneratorSteps)==57 &&
            actualAttack.Effect.Rolls.Select(r=>r.Range).SequenceEqual(new ushort[] {32,32,1,1,32,32}) &&
            actualAttack.Effect.Rolls.Select(r=>r.Result).SequenceEqual(new ushort[] {12,30,0,0,11,21}),
            "Physical priority and six main calls match independent reduction");
        Require(end.AiMemory.Skip(6).All(value=>value==0) && end.AiLastTargets.Where((_,i)=>i!=4).All(value=>value==255) &&
            end.Roster.Select((unit,i)=>(i==0 || ReferenceEquals(unit.Stats,initialBattle.Roster[i].Stats)) &&
                ReferenceEquals(unit.Deployment,initialBattle.Roster[i].Deployment)).All(value=>value),
            "Physical action retains other HP/stats, original anchors and unused memory");
        if (playerPhysical) { await ReviewPlayerPhysicalAttack(ready); return; }
        File.WriteAllText(Path.Combine(_output,"receipt.json"),JsonSerializer.Serialize(new {
            status="Pass",scope="controlled Map40 seed; physical Godot keys through first ordinary attack and Bowie HP9 control",
            instrumentation="copied round3 pursuit and round6 attack Application steps are compared with physical relay; exact source snapshots restored; no original timing claim",
            rounds=rounds.Select(b=>new {number=b.FirstRound!.RoundNumber,slots=b.FirstRound.Slots,main=b.RandomSeedImage,copy=b.RandomSeedCopy,
                flags=b.RegionFlags90Through105,words=b.Roster.Skip(3).Select(unit=>unit.AiBitfield),tested=b.NewlyTestedRegionMask}),
            turns=history.Select(receipt=>new {round=receipt.RoundNumber,actor=receipt.CompletedActorIndex,standby=receipt.EnemyStandby,pursuit=receipt.EnemyPursuit,physical=receipt.EnemyPhysicalAttack}),
            thinkingSteps=steps,main=end.RandomSeedImage,copy=end.RandomSeedCopy,memory=end.AiMemory,lastTargets=end.AiLastTargets,
            occupancy=end.Roster.Select(unit=>new {actor=unit.Index,position=unit.Position}),byteOffset=end.FirstRound!.CurrentTurnOffset,
            nextCandidate=0,inputClosed=false,frames=_frames,
        },new JsonSerializerOptions {WriteIndented=true}));
        GD.Print($"SF2_BATTLE01_CONTROL_NATIVE_REVIEW Pass frames={_frames.Count} {(physical ? "enemy-physical-attack" : "enemy-pursuit")}");
    }

    private async Task ReviewPlayerPhysicalAttack(PrivateOriginalBattle01SessionSnapshot ready)
    {
        bool firstDefeat = System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") is "first-enemy-defeat" or "chester-enemy-hit" or "chester-player-attack" or "second-enemy-defeat" or "first-ally-defeat" or "leader-defeat-pending" or "chester-counterattack";
        var presenter=Field<PrivateBattle01Presenter>(_root,"_privateBattle01Presenter");
        Require(ReferenceEquals(ready,_session.PrivateOriginalBattle01) && ready.Battle.Roster[0].Stats.HpCurrent==9 &&
            ready.Battle.Roster[0].Stats.CurrentExp==0 && ready.Battle.FirstControl?.ActorIndex==0 &&
            ready.Preparation.Party.Id==OriginalBattle01ControlledPartyPreset.LeaderDefeatComparisonId,
            "Actual Bowie HP9 control uses the explicitly named EXP0 input");
        if (!firstDefeat) await CaptureControl("01-bowie-ready-hp9-exp0");
        foreach(Key key in new[] {Key.L,Key.I,Key.Space,Key.A}) await PressBattleKey(key);
        var provisional=_session.PrivateOriginalBattle01!;
        Require(provisional.Battle.Phase==Battle01Phase.PlayerAttackTargetSelection &&
            provisional.Battle.Roster[0].Position==new MapPosition(12,14) &&
            provisional.Battle.FirstControl!.Movement.Attack!.Targets.SequenceEqual(new[] {132}) &&
            presenter.Projection!.SelectedTargetIndex==132,"Physical provisional tile chooses its live adjacent target");
        if (!firstDefeat) await CaptureControl("02-provisional-target132");
        await PressBattleKey(Key.Backspace);
        var cancelled=_session.PrivateOriginalBattle01!;
        Require(cancelled.Battle.Phase==Battle01Phase.PlayerActionChoice && cancelled.Battle.Roster[0].Position==new MapPosition(12,14),
            "Target cancellation retains the provisional position");
        if (!firstDefeat) await CaptureControl("03-target-cancel-action-choice");
        await PressBattleKey(Key.Backspace);
        var restored=_session.PrivateOriginalBattle01!;
        Require(restored.Battle.Phase==Battle01Phase.PlayerMovementSelection && restored.Battle.Roster[0].Position==new MapPosition(11,15) &&
            restored.Battle.Occupancy.SequenceEqual(ready.Battle.Occupancy),"Second cancellation restores the true turn origin");
        foreach(var snapshot in new[] {provisional,cancelled,restored})
            Require(snapshot.Battle.RandomSeedImage==ready.Battle.RandomSeedImage && snapshot.Battle.RandomSeedCopy==ready.Battle.RandomSeedCopy &&
                ReferenceEquals(snapshot.Battle.Roster[0].Stats,ready.Battle.Roster[0].Stats) &&
                ReferenceEquals(snapshot.Battle.Roster[7].Stats,ready.Battle.Roster[7].Stats) &&
                ReferenceEquals(snapshot.Battle.TurnCompletion,ready.Battle.TurnCompletion),"Manual noncommitting choices consume no HP/EXP/RNG/history");
        await PressBattleKey(Key.Space); await PressBattleKey(Key.A);
        var selected=_session.PrivateOriginalBattle01!;
        await PressBattleKey(Key.I); await PressBattleKey(Key.L);
        selected=_session.PrivateOriginalBattle01!;
        Require(selected.Battle.FirstControl!.Movement.Attack!.TargetIndex==132 && selected.Battle.Roster[0].Position==new MapPosition(11,15),
            "Explicit origin target selection remains manual and wraps its one-member list");
        if (!firstDefeat) await CaptureControl("04-origin-target132");
        var copied=(PrivateOriginalBattle01SessionSnapshot)Activator.CreateInstance(typeof(PrivateOriginalBattle01SessionSnapshot),
            BindingFlags.Instance|BindingFlags.NonPublic,null,new object?[] {selected.Preparation,selected.Battle,selected.SourceLocomotion,selected.SourceBridge},null)!;
        Battle01TurnCompletionReceipt inspected;
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session,copied);
        try
        {
            Require(_session.ConfirmPrivateOriginalBattle01PlayerAttack(copied,0) is PrivateOriginalBattle01PlayerAttackApplied,
                "Copied exact selected snapshot completes through Application");
            var completed=_session.PrivateOriginalBattle01!.Battle; inspected=completed.TurnCompletion!;
            Require(completed.FirstRound!.CurrentTurnOffset==14 && inspected.PlayerPhysicalAttack!.AwardedExp==15 &&
                completed.Roster[7].Stats.HpCurrent==2 && completed.Roster[0].Stats.CurrentExp==15,"Copied receipt52 replays HP and EXP once");
            presenter.Project(completed,"TEST COPY: receipt52 after direct Application confirmation. Exact physical selection resumes next.");
            if (!firstDefeat) await CaptureControl("05-player-attack-test-copy","test-only copy of exact selected snapshot; direct Application confirmation; no production pause or original playback timing");
        }
        finally
        {
            typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session,selected);
            presenter.Project(selected.Battle,"Restored exact physical target selection. Space confirms the manual attack.");
        }
        Require(ReferenceEquals(selected,_session.PrivateOriginalBattle01),"Exact selected snapshot restored before physical confirmation");
        await PressBattleKey(Key.Space);
        var final=_session.PrivateOriginalBattle01!; var end=final.Battle;
        var history=new List<Battle01TurnCompletionReceipt>();
        for(var receipt=end.TurnCompletion;receipt is not null;receipt=receipt.Previous) history.Add(receipt);
        var actual=history.Single(receipt=>receipt.PlayerPhysicalAttack is not null);
        Require(JsonSerializer.Serialize(actual)==JsonSerializer.Serialize(inspected),"Physical and copied full player completion receipts match");
        Require(history.Count==54 && end.FirstRound!.RoundNumber==7 && end.FirstControl?.ActorIndex==2 &&
            end.FirstControl.Movement.Range.Budget==14 && end.Roster[2].Position==new MapPosition(7,17) && end.Roster[2].Stats.HpCurrent==11 &&
            end.Roster[0].Stats.HpCurrent==9 && end.Roster[0].Stats.CurrentExp==15 && end.Roster[7].Stats.HpCurrent==2 &&
            end.RandomSeedImage==0xCF491234 && end.RandomSeedCopy==0x0234 && end.NewlyTestedRegionMask==7,
            "Production relay reaches actual round7 player2 without making a player choice");
        CheckRoundBuffer(end,[2,131,132,133,0,1,129,128,130],[6,6,6,6,5,5,5,4,4]);
        Require(history[1].CompletedActorIndex==131 && history[1].EnemyPursuit!.Destination==new MapPosition(11,8) &&
            history[0].CompletedActorIndex==133 && history[0].EnemyStandby!.Destination==new MapPosition(7,5) &&
            history[0].EnemyStandby!.Rolls.Select(r=>r.GeneratorSteps).SequenceEqual(new[] {114,19}),"Actual 131/133 follow the player receipt");
        Require(presenter.Projection!.AttackResult!.Contains("HP 5 -> 2") && presenter.Projection.AttackResult.Contains("EXP +15: 0 -> 15"),
            "Latest player result remains visible through new-round generation");
        foreach(Key key in new[] {Key.W,Key.F,Key.B,Key.M,Key.A}) await PressBattleKey(key);
        for(int frame=0;frame<12;frame++) await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
        Require(ReferenceEquals(final,_session.PrivateOriginalBattle01),"No unrequested action or repeated attack at round7 control");
        if (firstDefeat) { await ReviewFirstEnemyDefeat(final); return; }
        await CaptureControl("06-round7-player2-hp9-2-exp15");
        File.WriteAllText(Path.Combine(_output,"receipt.json"),JsonSerializer.Serialize(new {
            status="Pass",scope="controlled manual player physical/EXP construction and award semantics; presentation RNG and VInt omitted; no original post-playback RAM or H4 claim",
            preparation=final.Preparation.Party.Id,playerReceipt=actual,following=history.Take(2),
            round=end.FirstRound,main=end.RandomSeedImage,copy=end.RandomSeedCopy,memory=end.AiMemory,lastTargets=end.AiLastTargets,
            units=end.Roster.Select(u=>new {actor=u.Index,position=u.Position,hp=u.Stats.HpCurrent,exp=u.Stats.CurrentExp}),
            exactCopiedAndPhysicalReceiptMatch=true,bothCancelStages=true,frames=_frames,
        },new JsonSerializerOptions {WriteIndented=true}));
        GD.Print($"SF2_BATTLE01_CONTROL_NATIVE_REVIEW Pass frames={_frames.Count} player-physical-attack");
    }

    private async Task ReviewFirstEnemyDefeat(PrivateOriginalBattle01SessionSnapshot roundSeven)
    {
        var presenter = Field<PrivateBattle01Presenter>(_root, "_privateBattle01Presenter");
        var before = roundSeven.Battle;
        Require(before.FirstControl?.ActorIndex == 2 && before.CurrentGold == 0 && before.Roster[0].Stats.CurrentKills == 0 &&
            before.Roster[0].Stats.CurrentExp == 15 && before.Roster[7].Stats.HpCurrent == 2,
            "Actual round7 player2 retains explicit unspent accounting inputs");
        await CaptureControl("01-round7-player2");
        await PressBattleKey(Key.Space); await PressBattleKey(Key.Space);
        var bowie = _session.PrivateOriginalBattle01!;
        Require(bowie.Battle.FirstControl?.ActorIndex == 0 && bowie.Battle.Roster[0].Stats.HpCurrent == 6 &&
            bowie.Battle.Roster[7].Stats.HpCurrent == 2 && bowie.Battle.Roster[6].Position == new MapPosition(11, 10) &&
            bowie.Battle.Roster[8].Position == new MapPosition(6, 6) && bowie.Battle.RandomSeedImage == 0x18571234 &&
            bowie.Battle.RandomSeedCopy == 0x0234, "Actual131/132/133 relay reaches Bowie HP6 without making his choice");
        var enemy = bowie.Battle.TurnCompletion!.Previous!.EnemyPhysicalAttack!;
        Require(enemy.Priorities.Select(p => p.PotentialDamage).SequenceEqual(new[] { 2, 3 }) &&
            enemy.Priorities.Select(p => p.Priority).SequenceEqual(new[] { 1, 19 }) &&
            enemy.Effect.Rolls.Select(r => r.Result).SequenceEqual(new ushort[] { 16, 26, 0, 0, 2, 3 }),
            "Real HP11/DEF5 player1 priority and six enemy calls match the reduction");
        await CaptureControl("02-bowie-hp6-exp15");
        await PressBattleKey(Key.Space); await PressBattleKey(Key.A);
        var selected = _session.PrivateOriginalBattle01!;
        Require(selected.Battle.Phase == Battle01Phase.PlayerAttackTargetSelection &&
            selected.Battle.Roster[0].Position == new MapPosition(11, 15) &&
            presenter.Projection!.SelectedTargetIndex == 132, "Manual origin Attack selects live enemy132");
        await CaptureControl("03-origin-target132-hp2");
        var copied = (PrivateOriginalBattle01SessionSnapshot)Activator.CreateInstance(typeof(PrivateOriginalBattle01SessionSnapshot),
            BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object?[] { selected.Preparation, selected.Battle, selected.SourceLocomotion, selected.SourceBridge }, null)!;
        Battle01InitializedState inspected;
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session, copied);
        try
        {
            Require(_session.ConfirmPrivateOriginalBattle01PlayerAttack(copied, 0) is PrivateOriginalBattle01PlayerAttackApplied,
                "Exact selected copy completes through Application");
            inspected = _session.PrivateOriginalBattle01!.Battle;
            Require(inspected.FirstRound!.CurrentTurnOffset == 10 && inspected.TurnCompletion!.EnemyDefeat is not null &&
                inspected.CurrentGold == 60 && inspected.Roster[0].Stats.CurrentKills == 1 &&
                inspected.Roster[0].Stats.CurrentExp == 39 && inspected.Roster[7].Position is null,
                "Copied receipt59 commits cleanup and all awards");
            presenter.Project(inspected, "TEST COPY: receipt59 after direct Application confirmation. Exact physical selection resumes next.");
            await CaptureControl("04-defeat-receipt59-test-copy",
                "test-only copy of exact selected snapshot; direct Application confirmation; no production pause or original death timing");
        }
        finally
        {
            typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session, selected);
            presenter.Project(selected.Battle, "Restored exact physical target selection. Space confirms the manual attack.");
        }
        await PressBattleKey(Key.Space);
        var final = _session.PrivateOriginalBattle01!; var end = final.Battle;
        var history = new List<Battle01TurnCompletionReceipt>();
        for (var receipt = end.TurnCompletion; receipt is not null; receipt = receipt.Previous) history.Add(receipt);
        var lethal = end.TurnCompletion!.PlayerPhysicalAttack!; var cleanup = end.TurnCompletion.EnemyDefeat!;
        Require(history.Count == 59 && end.FirstControl?.ActorIndex == 1 && end.FirstControl.Movement.Range.Budget == 10 &&
            end.Roster[1].Position == new MapPosition(9, 17) && end.Roster[1].Stats.HpCurrent == 11 &&
            end.FirstRound!.CurrentTurnOffset == 10 && ReferenceEquals(before.FirstRound!.Slots, end.FirstRound.Slots),
            "Physical Space commits once and returns actual player1 control with the same64 slots");
        Require(JsonSerializer.Serialize(end.TurnCompletion) == JsonSerializer.Serialize(inspected.TurnCompletion) &&
            JsonSerializer.Serialize(end.Roster) == JsonSerializer.Serialize(inspected.Roster) &&
            end.Occupancy.SequenceEqual(inspected.Occupancy) && end.CurrentGold == inspected.CurrentGold &&
            end.RandomSeedImage == inspected.RandomSeedImage && end.RandomSeedCopy == inspected.RandomSeedCopy &&
            end.AiMemory.SequenceEqual(inspected.AiMemory) && end.AiLastTargets.SequenceEqual(inspected.AiLastTargets) &&
            end.RegionFlags90Through105.SequenceEqual(inspected.RegionFlags90Through105) &&
            end.NewlyTestedRegionMask == inspected.NewlyTestedRegionMask,
            "Copied and physical full receipts, placements, stats, awards, RNG, AI and flags match");
        Require(lethal.Effect.Rolls.Select(r => r.Range).SequenceEqual(new ushort[] { 8, 16, 1, 1, 16, 16 }) &&
            lethal.Effect.Rolls.Select(r => r.Result).SequenceEqual(new ushort[] { 1, 1, 0, 0, 14, 15 }) &&
            lethal.Effect.Rolls.Select(r => r.AfterImage).SequenceEqual(new uint[] {
                0x3C721234, 0x11D11234, 0xE7A41234, 0xC35B1234, 0xEBA61234, 0xF7751234 }) &&
            lethal.Effect.Damage == 3 && lethal.Effect.TemporaryHp == 0 && lethal.Effect.RestoredHp == 2 &&
            lethal.Effect.Reaction!.HpDelta == -3 && lethal.AccumulatedExp == 49 && lethal.AwardedExp == 24,
            "Overkill retains full reaction and lethal early return omits double/counter calls");
        Require(cleanup.FirstWorklist.SequenceEqual(new[] { 132 }) && cleanup.AfterTurnWorklist.Count == 0 &&
            cleanup.CreditedAlly == 0 && cleanup.KillsBefore == 0 && cleanup.KillsAfter == 1 &&
            end.TurnCompletion.BeforeAfterTurn == new Battle01FactionCounts(3, 5) &&
            end.TurnCompletion.AfterAfterTurn == new Battle01FactionCounts(3, 5) &&
            end.Occupancy.Count(id => id >= 0) == 8 && end.OccupantAt(new(11, 14)) == -1 &&
            end.Roster.Count == 9 && end.Roster[7].Stats.HpCurrent == 0 && end.Roster[7].Position is null &&
            ReferenceEquals(selected.Battle.Roster[7].Deployment, end.Roster[7].Deployment) &&
            ReferenceEquals(selected.Battle.Roster[7].EnemySource, end.Roster[7].EnemySource) &&
            end.CurrentGold == 60 && end.Roster[0].Stats.CurrentKills == 1 && end.Roster[0].Stats.CurrentExp == 39 &&
            end.RandomSeedImage == 0xF7751234 && end.RandomSeedCopy == 0x0234,
            "Both cleanup passes, retained provenance, cleared placement and one-time awards match");
        Require(presenter.Projection!.Units.Count == 8 && presenter.Projection.Units.All(unit => unit.Index != 132) &&
            presenter.Projection.AttackResult!.Contains("E4 (132) defeated") &&
            presenter.Projection.AttackResult.Contains("EXP +24: 15 -> 39") && presenter.Projection.AttackResult.Contains("Gold +60") &&
            presenter.Projection.Gold == 60 && presenter.Projection.BowieKills == 1,
            "Persistent visible defeat result and gold/kills coexist with player1 controls");
        await CaptureControl("05-player1-after-defeat");
        await PressBattleKey(Key.L); await PressBattleKey(Key.Space);
        Require(_session.PrivateOriginalBattle01!.Battle.Roster[1].Position == new MapPosition(10, 17),
            "Actual player1 can provisionally relocate");
        await PressBattleKey(Key.Backspace);
        var cancelled = _session.PrivateOriginalBattle01!;
        Require(cancelled.Battle.Roster[1].Position == new MapPosition(9, 17) &&
            ReferenceEquals(end.TurnCompletion, cancelled.Battle.TurnCompletion) &&
            cancelled.Battle.CurrentGold == 60 && cancelled.Battle.Roster[0].Stats.CurrentKills == 1 &&
            cancelled.Battle.Roster[0].Stats.CurrentExp == 39 && cancelled.Battle.Occupancy.SequenceEqual(end.Occupancy) &&
            presenter.Projection!.Tiles.Single(tile => tile.Position == new MapPosition(11, 14)).CanStop,
            "Move/cancel retains the cleared enemy tile and never repeats cleanup or awards");
        foreach (Key key in new[] { Key.W, Key.F, Key.B, Key.M, Key.A }) await PressBattleKey(key);
        Require(ReferenceEquals(cancelled, _session.PrivateOriginalBattle01), "Unmapped input cannot repeat the defeat");
        await CaptureControl("06-player1-move-cancel");
        if (System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") is "chester-enemy-hit" or "chester-player-attack" or "second-enemy-defeat" or "first-ally-defeat" or "leader-defeat-pending" or "chester-counterattack")
        {
            await ReviewChesterEnemyHit(cancelled);
            return;
        }
        File.WriteAllText(Path.Combine(_output, "receipt.json"), JsonSerializer.Serialize(new {
            status = "Pass", scope = "controlled first enemy defeat and actual player1 movement; no original animation, natural RNG lifetime or H4 claim",
            preparation = final.Preparation.Party.Id, receipt59 = end.TurnCompletion, secondEnemyStrike = enemy,
            round = end.FirstRound, main = end.RandomSeedImage, copy = end.RandomSeedCopy, memory = end.AiMemory,
            lastTargets = end.AiLastTargets, gold = end.CurrentGold,
            units = end.Roster.Select(u => new { actor = u.Index, position = u.Position, hp = u.Stats.HpCurrent,
                exp = u.Stats.CurrentExp, kills = u.Stats.CurrentKills }),
            exactCopiedAndPhysicalReceiptMatch = true, actualPlayerOneMoveCancel = true, frames = _frames,
        }, new JsonSerializerOptions { WriteIndented = true }));
        GD.Print($"SF2_BATTLE01_CONTROL_NATIVE_REVIEW Pass frames={_frames.Count} first-enemy-defeat");
    }

    private async Task ReviewChesterEnemyHit(PrivateOriginalBattle01SessionSnapshot receipt59)
    {
        var presenter = Field<PrivateBattle01Presenter>(_root, "_privateBattle01Presenter");
        Require(ReferenceEquals(receipt59, _session.PrivateOriginalBattle01) && receipt59.Battle.FirstControl?.ActorIndex == 1,
            "Continue the exact accepted receipt59 player1 snapshot");
        async Task ConfirmAndStay(int actor)
        {
            var movement = _session.PrivateOriginalBattle01!;
            Require(movement.Battle.Phase == Battle01Phase.PlayerMovementSelection && movement.Battle.FirstControl?.ActorIndex == actor,
                "Physical confirmation starts in the actual player's movement stage");
            await PressBattleKey(Key.Space);
            Require(_session.PrivateOriginalBattle01!.Battle.Phase == Battle01Phase.PlayerActionChoice &&
                ReferenceEquals(movement.Battle.TurnCompletion, _session.PrivateOriginalBattle01.Battle.TurnCompletion),
                "First physical Space confirms movement without committing a turn");
            await PressBattleKey(Key.Space);
        }
        await ConfirmAndStay(1);
        var roundEight = _session.PrivateOriginalBattle01!;
        Require(roundEight.Battle.FirstRound!.RoundNumber == 8 && roundEight.Battle.FirstControl?.ActorIndex == 2 &&
            roundEight.Battle.RandomSeedImage == 0xB28D1234 && roundEight.Battle.RandomSeedCopy == 0x5634,
            "Actual R7 candidates lead to R8 Chester without a scripted skip");
        CheckRoundBuffer(roundEight.Battle, [2,133,0,1,129,128,130,131], [8,6,5,5,5,4,4,4]);
        foreach (Key key in new[] { Key.L, Key.L, Key.L, Key.I, Key.L, Key.I, Key.I }) await PressBattleKey(key);
        var chesterSelection = _session.PrivateOriginalBattle01!;
        Require(chesterSelection.Battle.FirstControl!.Movement.Cursor == new MapPosition(11, 14) &&
            chesterSelection.Battle.FirstControl.Movement.GridCost == 14 &&
            chesterSelection.Battle.FirstControl.Movement.Preview.Directions.SequenceEqual(new byte[] { 0,0,0,1,0,1,1,255 }),
            "Physical cursor reaches the exact legal cost14 destination through the production movement API");
        await ConfirmAndStay(2);
        Require(_session.PrivateOriginalBattle01!.Battle.FirstControl?.ActorIndex == 0 &&
            _session.PrivateOriginalBattle01.Battle.Roster[2].Position == new MapPosition(11, 14), "Actual133 yields Bowie control");
        await CaptureControl("07-round8-bowie-after-chester-move");
        await ConfirmAndStay(0);
        var playerOne = _session.PrivateOriginalBattle01!;
        Require(playerOne.Battle.FirstControl?.ActorIndex == 1 && playerOne.Battle.Roster[0].Position == new MapPosition(11, 15) &&
            playerOne.Battle.Roster[0].Stats.HpCurrent == 6, "Bowie origin STAY preserves the selected nonlethal route");
        await CaptureControl("08-round8-player1-before-confirm-stay");
        var copied = (PrivateOriginalBattle01SessionSnapshot)Activator.CreateInstance(typeof(PrivateOriginalBattle01SessionSnapshot),
            BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object?[] { playerOne.Preparation, playerOne.Battle, playerOne.SourceLocomotion, playerOne.SourceBridge }, null)!;
        Battle01InitializedState inspected;
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session, copied);
        try
        {
            Require(_session.ConfirmPrivateOriginalBattle01PlayerMovement(copied, 1) is PrivateOriginalBattle01PlayerMovementApplied,
                "Exact copy confirms player1 movement through Application first");
            var provisional = _session.PrivateOriginalBattle01!;
            Require(_session.CommitPrivateOriginalBattle01Stay(provisional, 1) is PrivateOriginalBattle01StayCommitted,
                "Exact copy separately commits player1 STAY");
            string result = PrivateBattle01Ui.DispatchNext(_session, _session.PrivateOriginalBattle01!);
            inspected = _session.PrivateOriginalBattle01!.Battle;
            Require(inspected.FirstRound!.RoundNumber == 9 && inspected.FirstControl?.ActorIndex == 2,
                "Exact copied Application relay completes actual129/128/130/131 and returns actual Chester");
            presenter.Project(inspected, "TEST COPY: " + result + " Exact physical player1 movement snapshot resumes next.");
            await CaptureControl("09-chester-hit-and-round9-test-copy",
                "exact player1 movement snapshot copy; separate Application confirm/STAY then actual production relay; no production pause");
        }
        finally
        {
            typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session, playerOne);
            presenter.Project(playerOne.Battle, "Restored exact player1 movement snapshot. Confirm movement, then commit STAY.");
        }
        await ConfirmAndStay(1); // Both physical Space presses are required from the restored movement snapshot.
        var ready = _session.PrivateOriginalBattle01!; var end = ready.Battle;
        var receipt = end.TurnCompletion!; var hit = receipt.EnemyPhysicalAttack!; var priority = hit.Priorities.Single();
        var json = new JsonSerializerOptions { MaxDepth = 256 };
        Require(JsonSerializer.Serialize(inspected, json) == JsonSerializer.Serialize(end, json),
            "Copied and physical full battle snapshots match, including all history, placements, stats, accounting, flags and RNG");
        int count = 0; for (var r = receipt; r is not null; r = r.Previous) count++;
        Require(count == 71 && receipt.CompletedActorIndex == 131 && receipt.RoundNumber == 8 &&
            end.FirstRound!.RoundNumber == 9 && end.FirstRound.CurrentTurnOffset == 0 && end.FirstControl?.ActorIndex == 2 &&
            end.FirstControl.Movement.Range.Budget == 14, "Receipt71 returns actual R9 Chester movement control");
        Require(hit.CombatProfile == "battle01-class1-wooden-stick-effective-prowess3-v1" && hit.TargetIndex == 2 &&
            hit.GridCost == 6 && hit.Destination == new MapPosition(11, 13) && hit.MoveString.SequenceEqual(new byte[] { 3,3,3,255 }) &&
            priority.Priority == 7 && priority.PotentialDamage == 2 && priority.RemainingHp == 9 && priority.LandMultiplier == 230 &&
            priority.Roll.GeneratorSteps == 57 && priority.Roll.BeforeSeedCopy == 0x0034 && priority.Roll.AfterSeedCopy == 0x0134 && priority.Roll.Result == 1,
            "Unique actual Chester target, move and thinking-copy priority match the accepted reduction");
        Require(hit.Effect.Rolls.Select(r => r.Range).SequenceEqual(new ushort[] {32,32,1,1,32,32}) &&
            hit.Effect.Rolls.Select(r => r.Result).SequenceEqual(new ushort[] {2,27,0,0,25,13}) &&
            hit.Effect.Rolls.Select(r => r.AfterImage).SequenceEqual(new uint[] {0x11301234,0xDF771234,0x59121234,0x85F11234,0xCD441234,0x6C7B1234}) &&
            !hit.Effect.Dodged && !hit.Effect.Critical && hit.Effect.Damage == 2 && hit.Effect.TemporaryHp == 9 && hit.Effect.RestoredHp == 11 &&
            hit.Effect.Reaction == new Battle01PhysicalReaction(2,-2,0,0,1), "Six main calls and HP construction/restore/replay match");
        CheckRoundBuffer(end, [2,128,129,131,133,1,130,0], [8,6,6,6,5,4,4,3]);
        Require(end.RandomSeedImage == 0x71D31234 && end.RandomSeedCopy == 0x0134 && end.NewlyTestedRegionMask == 7 &&
            end.RegionFlags90Through105.Select((flag, i) => flag == (i == 1)).All(equal => equal) &&
            end.Roster.Skip(3).Select(u => u.AiBitfield).SequenceEqual(new ushort?[] {0x2060,0x2060,0x2060,0x2061,0x2071,0x2070}) &&
            end.AiMemory.Take(6).SequenceEqual(new byte[] {4,52,4,36,52,52}) &&
            end.AiLastTargets.Take(6).SequenceEqual(new byte[] {255,255,255,2,0,255}), "Round generation and all AI/random channels match");
        Require(receipt.BeforeAfterTurn == new Battle01FactionCounts(3,5) && receipt.AfterAfterTurn == new Battle01FactionCounts(3,5) &&
            end.Roster.Select(u => u.Position).SequenceEqual(new MapPosition?[] {new(11,15),new(9,17),new(11,14),new(7,2),new(10,4),new(6,3),new(11,13),null,new(7,5)}) &&
            end.Roster.Select(u => u.Stats.HpCurrent).SequenceEqual(new ushort[] {6,11,9,5,5,5,5,0,5}) && end.Occupancy.Count(id => id >= 0) == 8 &&
            end.CurrentGold == 60 && end.Roster[0].Stats.CurrentExp == 39 && end.Roster[0].Stats.CurrentKills == 1 &&
            end.Roster[2].Stats.CurrentExp == 0 && end.Roster[2].Stats.CurrentKills is null,
            "Earlier death and awards persist; Chester retains the prepared EXP0 and unspecified kills");
        Require(presenter.Projection!.AttackResult == "E3 -> A2: hit 2. HP 11 -> 9." && presenter.Projection.ActorIndex == 2 &&
            presenter.Projection.Gold == 60 && presenter.Projection.BowieKills == 1 && presenter.Projection.Units.All(u => u.Index != 132),
            "Newest enemy result remains visible across R9 instead of receipt59 overwriting it");
        await CaptureControl("10-round9-chester-after-physical-relay");
        await PressBattleKey(Key.L);
        Require(_session.PrivateOriginalBattle01!.Battle.FirstControl!.Movement.GridCost == 2, "Actual Chester preview retains cost2");
        await PressBattleKey(Key.Space);
        var moved = _session.PrivateOriginalBattle01!;
        Require(moved.Battle.Phase == Battle01Phase.PlayerActionChoice && moved.Battle.Roster[2].Position == new MapPosition(12,14) &&
            ReferenceEquals(receipt, moved.Battle.TurnCompletion), "Physical confirmation moves Chester without another action");
        await CaptureControl("11-chester-provisional12-14");
        await PressBattleKey(Key.Backspace);
        var cancelled = _session.PrivateOriginalBattle01!;
        Require(cancelled.Battle.Roster[2].Position == new MapPosition(11,14) && cancelled.Battle.Occupancy.SequenceEqual(end.Occupancy) &&
            ReferenceEquals(receipt, cancelled.Battle.TurnCompletion) && ReferenceEquals(end.FirstRound, cancelled.Battle.FirstRound) &&
            cancelled.Battle.RandomSeedImage == end.RandomSeedImage && cancelled.Battle.RandomSeedCopy == end.RandomSeedCopy &&
            cancelled.Battle.AiMemory.SequenceEqual(end.AiMemory) && cancelled.Battle.AiLastTargets.SequenceEqual(end.AiLastTargets) &&
            cancelled.Battle.CurrentGold == 60 && cancelled.Battle.Roster[0].Stats.CurrentExp == 39 && cancelled.Battle.Roster[0].Stats.CurrentKills == 1 &&
            cancelled.Battle.Roster[2].Stats.HpCurrent == 9 && presenter.Projection!.AttackResult == "E3 -> A2: hit 2. HP 11 -> 9.",
            "Physical movement cancellation preserves the applied result and every earlier award");
        await CaptureControl("12-chester-move-cancel");
        if (System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") is "chester-player-attack" or "second-enemy-defeat" or "first-ally-defeat" or "leader-defeat-pending" or "chester-counterattack")
        {
            await ReviewChesterPlayerAttack(cancelled);
            return;
        }
        File.WriteAllText(Path.Combine(_output, "receipt.json"), JsonSerializer.Serialize(new {
            status = "Pass", scope = "controlled first enemy hit on Chester and actual R9 movement/cancel; original timing and H4 remain Unknown",
            preparation = ready.Preparation.Party.Id, receipt71 = receipt, round = end.FirstRound,
            main = end.RandomSeedImage, copy = end.RandomSeedCopy, memory = end.AiMemory, lastTargets = end.AiLastTargets,
            gold = end.CurrentGold, tested = end.NewlyTestedRegionMask, regions = end.RegionFlags90Through105,
            units = end.Roster.Select(u => new { u.Index, u.Position, u.Stats }), exactCopiedAndPhysicalSnapshotMatch = true,
            separatePhysicalConfirmAndStay = true, actualChesterMoveCancel = true, frames = _frames,
        }, new JsonSerializerOptions { WriteIndented = true, MaxDepth = 256 }));
        GD.Print($"SF2_BATTLE01_CONTROL_NATIVE_REVIEW Pass frames={_frames.Count} chester-enemy-hit");
    }

    private async Task ReviewChesterPlayerAttack(PrivateOriginalBattle01SessionSnapshot ready)
    {
        var presenter = Field<PrivateBattle01Presenter>(_root, "_privateBattle01Presenter");
        Require(ReferenceEquals(ready, _session.PrivateOriginalBattle01) &&
            ready.Preparation.Party.Id == OriginalBattle01ControlledPartyPreset.LeaderDefeatComparisonId &&
            ready.Preparation.Party.Allies[2].CurrentExp == 0 && ready.Preparation.Party.Allies[2].CurrentKills is null,
            "The complete native route starts with the named authored Chester EXP0 input");
        await PressBattleKey(Key.Space); await PressBattleKey(Key.A);
        var selected = _session.PrivateOriginalBattle01!;
        Require(selected.Battle.Phase == Battle01Phase.PlayerAttackTargetSelection &&
            selected.Battle.FirstControl?.ActorIndex == 2 && selected.Battle.Roster[2].Position == new MapPosition(11, 14) &&
            selected.Battle.FirstControl.Movement.Attack!.Targets.SequenceEqual(new[] { 131 }) &&
            presenter.Projection!.SelectedTargetIndex == 131, "Physical origin confirmation and A select actual enemy131");
        await PressBattleKey(Key.I); await PressBattleKey(Key.L);
        Require(_session.PrivateOriginalBattle01!.Battle.FirstControl!.Movement.Attack!.TargetIndex == 131,
            "Physical previous/next wraps the single live target");
        await CaptureControl("13-chester-origin-target131");
        await PressBattleKey(Key.Backspace);
        var cancelled = _session.PrivateOriginalBattle01!;
        Require(cancelled.Battle.Phase == Battle01Phase.PlayerActionChoice &&
            ReferenceEquals(ready.Battle.TurnCompletion, cancelled.Battle.TurnCompletion) &&
            cancelled.Battle.RandomSeedImage == 0x71D31234 && cancelled.Battle.RandomSeedCopy == 0x0134 &&
            cancelled.Battle.Roster[2].Stats.CurrentExp == 0 && cancelled.Battle.Roster[6].Stats.HpCurrent == 5,
            "Target cancel keeps the provisional origin and consumes no effect");
        await CaptureControl("14-chester-target-cancel");
        await PressBattleKey(Key.A); selected = _session.PrivateOriginalBattle01!;
        var copied = (PrivateOriginalBattle01SessionSnapshot)Activator.CreateInstance(typeof(PrivateOriginalBattle01SessionSnapshot),
            BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object?[] { selected.Preparation, selected.Battle, selected.SourceLocomotion, selected.SourceBridge }, null)!;
        Battle01InitializedState inspected; Battle01TurnCompletionReceipt inspectedAttack;
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session, copied);
        try
        {
            Require(_session.ConfirmPrivateOriginalBattle01PlayerAttack(copied, 2) is PrivateOriginalBattle01PlayerAttackApplied,
                "Exact selected snapshot copy confirms through Application");
            var completed = _session.PrivateOriginalBattle01!.Battle; inspectedAttack = completed.TurnCompletion!;
            var d = inspectedAttack.PlayerPhysicalAttack!;
            Require(completed.FirstRound!.CurrentTurnOffset == 2 && completed.FirstRound.CurrentCandidate?.CombatantIndex == 128 &&
                d.ActorIndex == 2 && d.TargetIndex == 131 && d.CombatProfile == "battle01-class1-wooden-stick-effective-prowess3-v1" &&
                d.TargetTerrain == 1 && d.LandMultiplier == 230 && d.MoveString.SequenceEqual(new byte[] { 255 }) &&
                d.Effect.Damage == 2 && d.Effect.TemporaryHp == 3 && d.Effect.RestoredHp == 5 &&
                d.Effect.Reaction == new Battle01PhysicalReaction(131, -2, 0, 0, 1) &&
                !d.Effect.Dodged && !d.Effect.Critical && !d.DefeatedTarget &&
                d.AccumulatedExp == 20 && d.HalvedExp == 10 && d.AwardedExp == 10 &&
                completed.Roster[2].Stats.CurrentExp == 10 && completed.Roster[6].Stats.HpCurrent == 3 &&
                completed.RandomSeedImage == 0xA59B1234 && completed.RandomSeedCopy == 0x0134,
                "Receipt72 uses Chester's profile and persists HP3/EXP10");
            Require(d.Effect.Rolls.Select(r => r.Range).SequenceEqual(new ushort[] {8,16,1,1,32,32,16,16}) &&
                d.Effect.Rolls.Select(r => r.Result).SequenceEqual(new ushort[] {6,2,0,0,24,1,8,10}) &&
                d.Effect.Rolls.Select(r => r.AfterImage).SequenceEqual(new uint[] {0xC7BE1234,0x24AD1234,0xDCD01234,0x36971234,
                    0xC5B21234,0x0A111234,0x82E41234,0xA59B1234}), "Eight player main calls match the source reduction");
            presenter.Project(completed, "TEST COPY: Chester receipt72 after Application confirmation. Exact physical selection resumes next.");
            Require(presenter.Projection!.AttackResult == "A2 -> E3: hit 2. HP 5 -> 3. EXP +10: 0 -> 10.",
                "The immediate copied frame shows the player result before automatic enemy dispatch");
            await CaptureControl("15-chester-attack-receipt72-test-copy",
                "exact selected snapshot copy; Application confirmation before actual production relay; no production pause");
            PrivateBattle01Ui.DispatchNext(_session, _session.PrivateOriginalBattle01!);
            inspected = _session.PrivateOriginalBattle01!.Battle;
            Require(inspected.FirstControl?.ActorIndex == 1, "Copied actual enemy relay returns Sarah control");
        }
        finally
        {
            typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session, selected);
            presenter.Project(selected.Battle, "Restored exact physical Chester target selection. Space confirms.");
        }
        await PressBattleKey(Key.Space);
        var sarah = _session.PrivateOriginalBattle01!; var end = sarah.Battle;
        var json = new JsonSerializerOptions { MaxDepth = 256 };
        Require(JsonSerializer.Serialize(inspected, json) == JsonSerializer.Serialize(end, json),
            "Physical confirmation and copied actual dispatch produce identical complete battle states");
        var history = new List<Battle01TurnCompletionReceipt>();
        for (var receipt = end.TurnCompletion; receipt is not null; receipt = receipt.Previous) history.Add(receipt);
        Require(history.Count == 76 && history.Take(5).Reverse().Select(r => r.CompletedActorIndex).SequenceEqual(new[] {2,128,129,131,133}) &&
            JsonSerializer.Serialize(history[4], json) == JsonSerializer.Serialize(inspectedAttack, json),
            "Receipt72 and all four actual enemy completions match without another player choice");
        Require(end.FirstRound!.RoundNumber == 9 && end.FirstRound.CurrentTurnOffset == 10 &&
            end.FirstControl?.ActorIndex == 1 && end.FirstControl.Movement.Range.Budget == 10 &&
            ReferenceEquals(ready.Battle.FirstRound!.Slots, end.FirstRound.Slots),
            "Same generated R9 order returns actual Sarah movement control");
        CheckRoundBuffer(end, [2,128,129,131,133,1,130,0], [8,6,6,6,5,4,4,3]);
        var enemy = history[1].EnemyPhysicalAttack!;
        Require(enemy.ActorIndex == 131 && enemy.Actor.Stats.HpCurrent == 3 && enemy.TargetIndex == 0 &&
            enemy.Priorities.Select(p => p.Target.Index).SequenceEqual(new[] {2,1,0}) &&
            enemy.Priorities.Select(p => p.Candidate.GridCost).SequenceEqual(new[] {0,10,6}) &&
            enemy.Priorities.Select(p => p.PotentialDamage).SequenceEqual(new[] {2,2,3}) &&
            enemy.Priorities.Select(p => p.RemainingHp).SequenceEqual(new[] {7,9,3}) &&
            enemy.Priorities.Select(p => p.Priority).SequenceEqual(new[] {1,1,7}) &&
            enemy.Priorities.Select(p => p.Roll.GeneratorSteps).SequenceEqual(new[] {66,57,133}) &&
            enemy.MoveString.SequenceEqual(new byte[] {2,3,3,255}) && enemy.Destination == new MapPosition(10,15) &&
            enemy.Effect.Damage == 3 && enemy.Effect.TemporaryHp == 3 && enemy.Effect.RestoredHp == 6 &&
            enemy.Effect.Reaction == new Battle01PhysicalReaction(0,-3,0,0,1) && !enemy.Effect.Dodged && !enemy.Effect.Critical &&
            enemy.Effect.Rolls.Select(r => r.Result).SequenceEqual(new ushort[] {13,10,0,0,12,4}),
            "Damaged131 preserves Chester EXP10 and selects Bowie through the original reverse priority traversal");
        Require(end.RandomSeedImage == 0x25991234 && end.RandomSeedCopy == 0x0634 &&
            end.AiMemory.Take(6).SequenceEqual(new byte[] {52,36,4,36,52,52}) &&
            end.AiLastTargets.Take(6).SequenceEqual(new byte[] {255,255,255,0,0,255}) && end.NewlyTestedRegionMask == 0 &&
            end.RegionFlags90Through105.Select((flag, i) => flag == (i == 1)).All(equal => equal) &&
            end.Roster.Skip(3).Select(u => u.AiBitfield).SequenceEqual(new ushort?[] {0x2060,0x2060,0x2060,0x2061,0x2071,0x2070}),
            "Every final random, thinking, target and activation channel matches");
        Require(end.Roster.Select(u => u.Position).SequenceEqual(new MapPosition?[] {new(11,15),new(9,17),new(11,14),
                new(8,3),new(9,5),new(6,3),new(10,15),null,new(7,5)}) &&
            end.Roster.Select(u => u.Stats.HpCurrent).SequenceEqual(new ushort[] {3,11,9,5,5,5,3,0,5}) &&
            end.Roster.Count == 9 && end.Occupancy.Count(id => id >= 0) == 8 &&
            end.CurrentGold == 60 && end.Roster[0].Stats.CurrentExp == 39 && end.Roster[0].Stats.CurrentKills == 1 &&
            end.Roster[2].Stats.CurrentExp == 10 && end.Roster[2].Stats.CurrentKills is null &&
            history.Take(5).All(r => r.EnemyDefeat is null && r.BeforeAfterTurn == new Battle01FactionCounts(3,5) &&
                r.AfterAfterTurn == new Battle01FactionCounts(3,5)), "No second death or repeated accounting is introduced");
        Require(presenter.Projection!.AttackResult == "E3 -> A0: hit 3. HP 6 -> 3." &&
            presenter.Projection.Units.Single(u => u.Index == 2).Exp == 10 && presenter.Projection.ActorIndex == 1,
            "Newest enemy result replaces the player message while Chester's earned EXP remains visible");
        await CaptureControl("16-sarah-ready-after-chester-attack");
        await PressBattleKey(Key.L);
        Require(_session.PrivateOriginalBattle01!.Battle.FirstControl!.Movement.GridCost == 2, "Sarah preview costs2");
        await PressBattleKey(Key.Space);
        var moved = _session.PrivateOriginalBattle01!;
        Require(moved.Battle.Roster[1].Position == new MapPosition(10,17) &&
            ReferenceEquals(end.TurnCompletion, moved.Battle.TurnCompletion), "Physical Sarah move does not take her turn");
        await CaptureControl("17-sarah-provisional10-17");
        await PressBattleKey(Key.Backspace);
        var final = _session.PrivateOriginalBattle01!;
        Require(final.Battle.Roster[1].Position == new MapPosition(9,17) && final.Battle.Occupancy.SequenceEqual(end.Occupancy) &&
            ReferenceEquals(end.TurnCompletion, final.Battle.TurnCompletion) && ReferenceEquals(end.FirstRound, final.Battle.FirstRound) &&
            final.Battle.RandomSeedImage == end.RandomSeedImage && final.Battle.RandomSeedCopy == end.RandomSeedCopy &&
            final.Battle.Roster.Select(u => u.Stats).SequenceEqual(end.Roster.Select(u => u.Stats)) &&
            ReferenceEquals(ready.Preparation, final.Preparation) && ReferenceEquals(ready.SourceBridge, final.SourceBridge) &&
            ReferenceEquals(ready.SourceLocomotion, final.SourceLocomotion), "Sarah cancel retains the exact history and provenance");
        await CaptureControl("18-sarah-move-cancel");
        if (System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") is "second-enemy-defeat" or "first-ally-defeat" or "leader-defeat-pending" or "chester-counterattack")
        {
            await ReviewSecondEnemyDefeat(final);
            return;
        }
        File.WriteAllText(Path.Combine(_output, "receipt.json"), JsonSerializer.Serialize(new {
            status = "Pass", scope = "controlled Chester player attack through actual R9 Sarah movement/cancel; natural EXP, original playback timing and H4 remain Unknown",
            preparation = final.Preparation.Party.Id, receipt72 = inspectedAttack, finalReceipt = end.TurnCompletion,
            round = end.FirstRound, main = end.RandomSeedImage, copy = end.RandomSeedCopy, memory = end.AiMemory,
            lastTargets = end.AiLastTargets, gold = end.CurrentGold, tested = end.NewlyTestedRegionMask, regions = end.RegionFlags90Through105,
            units = end.Roster.Select(u => new {u.Index,u.Position,u.Stats}), exactCopiedAndPhysicalSnapshotMatch = true,
            exactCopiedAndPhysicalReceiptMatch = true, actualSarahMoveCancel = true, frames = _frames,
        }, new JsonSerializerOptions {WriteIndented = true, MaxDepth = 256}));
        GD.Print($"SF2_BATTLE01_CONTROL_NATIVE_REVIEW Pass frames={_frames.Count} chester-player-attack");
    }


    private async Task ReviewSecondEnemyDefeat(PrivateOriginalBattle01SessionSnapshot sarah)
    {
        var presenter = Field<PrivateBattle01Presenter>(_root, "_privateBattle01Presenter");
        Require(ReferenceEquals(sarah, _session.PrivateOriginalBattle01) && sarah.Battle.FirstControl?.ActorIndex == 1,
            "Continue the actual Sarah control after Chester's attack");
        await PressBattleKey(Key.Space);
        Require(_session.PrivateOriginalBattle01!.Battle.Phase == Battle01Phase.PlayerActionChoice &&
            ReferenceEquals(sarah.Battle.TurnCompletion, _session.PrivateOriginalBattle01.Battle.TurnCompletion),
            "Sarah origin confirmation consumes no turn");
        await PressBattleKey(Key.Space);
        var ready = _session.PrivateOriginalBattle01!; var before = ready.Battle;
        var standby = before.TurnCompletion!.EnemyStandby!;
        Require(before.FirstControl?.ActorIndex == 0 && before.FirstControl.Movement.Range.Budget == 12 &&
            before.FirstRound!.RoundNumber == 9 && before.FirstRound.CurrentTurnOffset == 14 &&
            before.TurnCompletion.CompletedActorIndex == 130 && before.TurnCompletion.Previous!.CompletedActorIndex == 1 &&
            ReferenceEquals(before.TurnCompletion.Previous.Previous, sarah.Battle.TurnCompletion) &&
            standby.Origin == new MapPosition(6,3) && standby.Destination == new MapPosition(5,4) &&
            standby.MoveString.SequenceEqual(new byte[] {3,2,255}) &&
            standby.Rolls.Select(r => r.Range).SequenceEqual(new byte[] {8,2}) &&
            standby.Rolls.Select(r => r.Result).SequenceEqual(new byte[] {5,0}) &&
            standby.Rolls.Select(r => r.GeneratorSteps).SequenceEqual(new[] {11,43}) &&
            standby.Rolls.Select(r => r.AfterSeedCopy).SequenceEqual(new ushort[] {0x0534,0x0034}) &&
            standby.MemoryBefore == 4 && standby.MemoryAfter == 20 &&
            before.RandomSeedImage == 0x25991234 && before.RandomSeedCopy == 0x0034,
            "Physical Sarah STAY dispatches actual130 and returns Bowie after receipt78");
        Require(before.Roster[0].Position == new MapPosition(11,15) && before.Roster[0].Stats.HpCurrent == 3 &&
            before.Roster[0].Stats.CurrentExp == 39 && before.Roster[0].Stats.CurrentKills == 1 && before.CurrentGold == 60 &&
            before.TerrainAt(new(10,15)) == 0 && before.TerrainAt(new(11,15)) == 1,
            "Real target terrain0 and Bowie origin terrain1 are independently retained");
        await CaptureControl("19-bowie-ready-after-sarah-stay");
        await PressBattleKey(Key.Space); await PressBattleKey(Key.A);
        await PressBattleKey(Key.I); await PressBattleKey(Key.L);
        Require(_session.PrivateOriginalBattle01!.Battle.FirstControl!.Movement.Attack!.Targets.SequenceEqual(new[] {131}) &&
            presenter.Projection!.SelectedTargetIndex == 131, "Physical target cycling retains the sole live131");
        await CaptureControl("20-bowie-origin-target131");
        await PressBattleKey(Key.Backspace);
        var cancelled = _session.PrivateOriginalBattle01!;
        Require(cancelled.Battle.Phase == Battle01Phase.PlayerActionChoice &&
            ReferenceEquals(before.TurnCompletion, cancelled.Battle.TurnCompletion) &&
            cancelled.Battle.RandomSeedImage == before.RandomSeedImage && cancelled.Battle.RandomSeedCopy == before.RandomSeedCopy &&
            cancelled.Battle.Roster.Select(u => u.Stats).SequenceEqual(before.Roster.Select(u => u.Stats)) &&
            cancelled.Battle.CurrentGold == before.CurrentGold, "Physical target cancel consumes no award or randomness");
        await CaptureControl("21-bowie-target-cancel");
        await PressBattleKey(Key.A);
        var selected = _session.PrivateOriginalBattle01!;
        var json = new JsonSerializerOptions {MaxDepth = 256};
        string priorHistory = JsonSerializer.Serialize(selected.Battle.TurnCompletion, json);
        var prefix = new List<Battle01TurnCompletionReceipt>();
        for (var r = selected.Battle.TurnCompletion; r is not null; r = r.Previous) prefix.Add(r);
        Require(prefix.Count == 78 && prefix.All(r => !ReferenceEquals(r.Policy,
            Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndSecondDefeat)),
            "All78 earlier receipt policies retain their original meaning");
        var copied = (PrivateOriginalBattle01SessionSnapshot)Activator.CreateInstance(typeof(PrivateOriginalBattle01SessionSnapshot),
            BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object?[] {selected.Preparation, selected.Battle, selected.SourceLocomotion, selected.SourceBridge}, null)!;
        Battle01InitializedState inspected; Battle01TurnCompletionReceipt inspectedAttack;
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session, copied);
        try
        {
            Require(_session.ConfirmPrivateOriginalBattle01PlayerAttack(copied, 0) is PrivateOriginalBattle01PlayerAttackApplied,
                "Exact selected copy confirms through Application");
            var completed = _session.PrivateOriginalBattle01!.Battle; inspectedAttack = completed.TurnCompletion!;
            var d = inspectedAttack.PlayerPhysicalAttack!; var cleanup = inspectedAttack.EnemyDefeat!;
            Require(ReferenceEquals(inspectedAttack.Previous, selected.Battle.TurnCompletion) &&
                JsonSerializer.Serialize(inspectedAttack.Previous, json) == priorHistory &&
                ReferenceEquals(inspectedAttack.Policy, Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndSecondDefeat) &&
                d.ActorIndex == 0 && d.TargetIndex == 131 && d.TargetTerrain == 0 && d.LandMultiplier == 256 &&
                d.CombatProfile == "battle01-class0-wooden-sword-effective-prowess3-v1" &&
                d.MoveString.SequenceEqual(new byte[] {255}) && d.MovementOrigin == new MapPosition(11,15) &&
                d.Target.Position == new MapPosition(10,15) && d.Effect.Damage == 4 &&
                d.Target.Stats.HpCurrent - d.Effect.Damage == -1 && d.Effect.TemporaryHp == 0 && d.Effect.RestoredHp == 3 &&
                d.Effect.AfterStats.HpCurrent == 0 && d.Effect.Reaction == new Battle01PhysicalReaction(131,-4,0,0,1) &&
                !d.Effect.Dodged && !d.Effect.Critical && d.DefeatedTarget &&
                d.AccumulatedExp == 49 && d.HalvedExp == 24 && d.AwardedExp == 24 &&
                d.GoldBefore == 60 && d.GoldAfter == 120 &&
                d.Effect.Rolls.Select(r => r.Range).SequenceEqual(new ushort[] {8,16,1,1,16,16}) &&
                d.Effect.Rolls.Select(r => r.Result).SequenceEqual(new ushort[] {7,13,0,0,9,5}) &&
                d.Effect.Rolls.Select(r => r.AfterImage).SequenceEqual(new uint[] {
                    0xE8CC1234,0xD2631234,0xAF0E1234,0xE3BD1234,0x90A01234,0x58271234}),
                "Terrain0 receipt79 preserves six lethal early-return calls and the exact prior history");
            Require(cleanup.FirstWorklist.SequenceEqual(new[] {131}) && cleanup.AfterTurnWorklist.Count == 0 &&
                cleanup.CreditedAlly == 0 && cleanup.KillsBefore == 1 && cleanup.KillsAfter == 2 &&
                inspectedAttack.BeforeAfterTurn == new Battle01FactionCounts(3,4) &&
                inspectedAttack.AfterAfterTurn == inspectedAttack.BeforeAfterTurn &&
                ReferenceEquals(before.Roster[7], completed.Roster[7]) && completed.Roster[6].Position is null &&
                completed.OccupantAt(new(10,15)) == -1 && completed.FirstRound!.CurrentTurnOffset == 16 &&
                completed.FirstRound.CurrentCandidate is null && ReferenceEquals(before.FirstRound!.Slots, completed.FirstRound.Slots) &&
                completed.RandomSeedImage == 0x58271234 && completed.RandomSeedCopy == 0x0034,
                "Only new131 is cleared and rewarded before the same R9 buffer reaches its sentinel");
            presenter.Project(completed, "TEST COPY: second defeat receipt79 before R10 generation. Exact physical selection resumes next.");
            Require(presenter.Projection!.AttackResult!.Contains("hit 4") &&
                presenter.Projection.AttackResult.Contains("EXP +24: 39 -> 63") &&
                presenter.Projection.AttackResult.Contains("Gold +60") && presenter.Projection.Gold == 120 &&
                presenter.Projection.BowieKills == 2 && presenter.Projection.Units.Count(u => u.Index >= 128) == 4 &&
                !presenter.Projection.Units.Any(u => u.Index is 131 or 132),
                "The copied immediate frame distinguishes this award from live totals and removes both enemy markers");
            await CaptureControl("22-second-defeat-receipt79-test-copy",
                "exact selected snapshot copy; Application confirmation before production R10 generation; no production pause");
            PrivateBattle01Ui.DispatchNext(_session, _session.PrivateOriginalBattle01!);
            inspected = _session.PrivateOriginalBattle01!.Battle;
            Require(inspected.FirstControl?.ActorIndex == 2 && ReferenceEquals(inspectedAttack, inspected.TurnCompletion),
                "Copied production dispatch generates R10 and immediately yields Chester without another completion");
        }
        finally
        {
            typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session, selected);
            presenter.Project(selected.Battle, "Restored exact physical Bowie target selection. Space confirms.");
        }
        await PressBattleKey(Key.Space);
        var chester = _session.PrivateOriginalBattle01!; var end = chester.Battle;
        Require(JsonSerializer.Serialize(inspected, json) == JsonSerializer.Serialize(end, json) &&
            JsonSerializer.Serialize(inspectedAttack, json) == JsonSerializer.Serialize(end.TurnCompletion, json) &&
            ReferenceEquals(selected.Battle.TurnCompletion, end.TurnCompletion!.Previous),
            "Physical Space and copied production dispatch match the complete battle and receipt79 exactly");
        Require(end.FirstRound!.RoundNumber == 10 && end.FirstRound.CurrentTurnOffset == 0 &&
            end.FirstControl?.ActorIndex == 2 && end.FirstControl.Movement.Range.Budget == 14 &&
            !end.FirstControl.Movement.Range.CanStopAt(new(10,15)) &&
            end.RandomSeedImage == 0x9F861234 && end.RandomSeedCopy == 0x0034 && end.NewlyTestedRegionMask == 7,
            "Seven-survivor R10 returns actual Chester; empty LowSky remains impassable to Centaur");
        CheckRoundBuffer(end, [2,1,128,129,130,133,0], [7,6,5,5,5,5,4]);
        Require(end.Roster.Select(u => u.Position).SequenceEqual(new MapPosition?[] {
                new(11,15),new(9,17),new(11,14),new(8,3),new(9,5),new(5,4),null,null,new(7,5)}) &&
            end.Roster.Select(u => u.Stats.HpCurrent).SequenceEqual(new ushort[] {3,11,9,5,5,5,0,0,5}) &&
            end.Roster.Count == 9 && end.Occupancy.Count(id => id >= 0) == 7 &&
            end.CurrentGold == 120 && end.Roster[0].Stats.CurrentExp == 63 && end.Roster[0].Stats.CurrentKills == 2 &&
            end.Roster[2].Stats.CurrentExp == 10 && end.Roster[2].Stats.CurrentKills is null &&
            end.AiMemory.SequenceEqual(new byte[] {52,36,20,36,52,52}.Concat(Enumerable.Repeat((byte)0,42))) &&
            end.AiLastTargets.SequenceEqual(new byte[] {255,255,255,0,0,255}.Concat(Enumerable.Repeat((byte)255,42))) &&
            end.RegionFlags90Through105.Select((flag,i) => flag == (i == 1)).All(equal => equal) &&
            end.Roster.Skip(3).Select(u => u.AiBitfield).SequenceEqual(new ushort?[] {0x2060,0x2060,0x2060,0x2061,0x2071,0x2070}),
            "All identities, positions, HP, awards and48-slot AI channels agree after real R10 generation");
        var resultText = presenter.Projection!.AttackResult;
        await CaptureControl("23-round10-chester-after-physical-defeat");
        await PressBattleKey(Key.L);
        Require(_session.PrivateOriginalBattle01!.Battle.FirstControl!.Movement.GridCost == 2, "Chester preview costs2");
        await PressBattleKey(Key.Space);
        Require(_session.PrivateOriginalBattle01!.Battle.Roster[2].Position == new MapPosition(12,14) &&
            ReferenceEquals(end.TurnCompletion, _session.PrivateOriginalBattle01.Battle.TurnCompletion) &&
            presenter.Projection!.AttackResult == resultText, "Physical Chester movement retains the latest defeat result");
        await CaptureControl("24-round10-chester-provisional12-14");
        await PressBattleKey(Key.Backspace);
        var final = _session.PrivateOriginalBattle01!;
        Require(final.Battle.Roster[2].Position == new MapPosition(11,14) && final.Battle.Occupancy.SequenceEqual(end.Occupancy) &&
            final.Battle.Roster.Select(u => u.Stats).SequenceEqual(end.Roster.Select(u => u.Stats)) &&
            ReferenceEquals(end.TurnCompletion, final.Battle.TurnCompletion) && ReferenceEquals(end.FirstRound, final.Battle.FirstRound) &&
            final.Battle.RandomSeedImage == end.RandomSeedImage && final.Battle.RandomSeedCopy == end.RandomSeedCopy &&
            final.Battle.AiMemory.SequenceEqual(end.AiMemory) && final.Battle.AiLastTargets.SequenceEqual(end.AiLastTargets) &&
            final.Battle.CurrentGold == 120 && presenter.Projection!.AttackResult == resultText &&
            ReferenceEquals(sarah.Preparation, final.Preparation) && ReferenceEquals(sarah.SourceBridge, final.SourceBridge) &&
            ReferenceEquals(sarah.SourceLocomotion, final.SourceLocomotion),
            "Physical cancel restores Chester while preserving both corpses, awards, order, seeds and provenance");
        await CaptureControl("25-round10-chester-move-cancel");
        if (System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") is "first-ally-defeat" or "leader-defeat-pending" or "chester-counterattack")
        {
            if (System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") == "chester-counterattack")
                await ReviewChesterCounterattack(final);
            else
            await ReviewFirstAllyDefeat(final);
            return;
        }
        File.WriteAllText(Path.Combine(_output, "receipt.json"), JsonSerializer.Serialize(new {
            status = "Pass", scope = "controlled second enemy defeat through actual R10 Chester movement/cancel; natural EXP, timing and H4 remain Unknown",
            preparation = final.Preparation.Party.Id, receipt79 = inspectedAttack, battle = final.Battle,
            exactCopiedAndPhysicalSnapshotMatch = true, exactCopiedAndPhysicalReceiptMatch = true,
            actualSarahConfirmStay = true, actualChesterMoveCancel = true, frames = _frames,
        }, new JsonSerializerOptions {WriteIndented = true, MaxDepth = 256}));
        GD.Print($"SF2_BATTLE01_CONTROL_NATIVE_REVIEW Pass frames={_frames.Count} second-enemy-defeat");
    }


    private async Task ReviewChesterCounterattack(PrivateOriginalBattle01SessionSnapshot chester)
    {
        var presenter=Field<PrivateBattle01Presenter>(_root,"_privateBattle01Presenter");
        var json=new JsonSerializerOptions {WriteIndented=true,MaxDepth=256};
        Require(chester.Preparation.Party.Allies[2].CurrentExp==0 && chester.Battle.Roster[2].Stats.CurrentExp==10,
            "The real startup's early Chester EXP0 and its79 receipts remain the only accounting origin");
        string prefix=JsonSerializer.Serialize(chester.Battle.TurnCompletion,json);
        await PressBattleKey(Key.J);await PressBattleKey(Key.J);
        for(int i=0;i<5;i++)await PressBattleKey(Key.I);
        Require(_session.PrivateOriginalBattle01!.Battle.FirstControl!.Movement.Cursor==new MapPosition(9,9),"R10 selected Chester destination");
        await PressBattleKey(Key.Space);await CaptureControl("26-counter-route-r10-chester9-9");
        await PressBattleKey(Key.Space);
        foreach(int actor in new[]{1,0})
        {
            Require(_session.PrivateOriginalBattle01!.Battle.FirstControl?.ActorIndex==actor,"Actual R10 allied origin choice");
            await PressBattleKey(Key.Space);await PressBattleKey(Key.Space);
        }
        Require(_session.PrivateOriginalBattle01!.Battle.FirstControl?.ActorIndex==2 &&
            _session.PrivateOriginalBattle01.Battle.FirstRound!.RoundNumber==11,"Real R11 Chester control");
        // Actual reachable preview skirts the impassable column: (9,9)->(8,9)->(8,4)->(9,4).
        await PressBattleKey(Key.J);
        for(int i=0;i<5;i++)await PressBattleKey(Key.I);
        await PressBattleKey(Key.L);
        Require(_session.PrivateOriginalBattle01!.Battle.FirstControl!.Movement.Cursor==new MapPosition(9,4) &&
            _session.PrivateOriginalBattle01.Battle.FirstControl.Movement.GridCost==14,"Actual cost14 R11 move");
        await PressBattleKey(Key.Space);await PressBattleKey(Key.A);
        Require(presenter.Projection!.SelectedTargetIndex==129,"Manual R11 target129");
        await CaptureControl("27-chester-target129");
        await PressBattleKey(Key.Backspace);await CaptureControl("28-chester-target-cancel");
        await PressBattleKey(Key.A);await PressBattleKey(Key.I);await PressBattleKey(Key.L);
        var selected=_session.PrivateOriginalBattle01!;
        Require(selected.Battle.FirstControl!.Movement.Attack!.TargetIndex==129,"Physical cancel and reselect retain129");
        var copied=(PrivateOriginalBattle01SessionSnapshot)Activator.CreateInstance(typeof(PrivateOriginalBattle01SessionSnapshot),
            BindingFlags.Instance|BindingFlags.NonPublic,null,new object?[]{selected.Preparation,selected.Battle,selected.SourceLocomotion,selected.SourceBridge},null)!;
        Battle01InitializedState inspected;Battle01TurnCompletionReceipt inspectedCounter;
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session,copied);
        try
        {
            Require(_session.ConfirmPrivateOriginalBattle01PlayerAttack(copied,2) is PrivateOriginalBattle01PlayerAttackApplied,"Copied manual attack87");
            foreach(int actor in new[]{128,130})
            {
                var before=_session.PrivateOriginalBattle01!;
                Require(before.Battle.FirstRound!.CurrentCandidate?.CombatantIndex==actor &&
                    _session.CompletePrivateOriginalBattle01EnemyPursuit(before,actor) is PrivateOriginalBattle01AttackSelectionRequired,"Actual copied enemy selection");
                Require(_session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(before,actor) is PrivateOriginalBattle01EnemyPhysicalAttackCompleted,"Copied primary/counter completion");
            }
            var after=_session.PrivateOriginalBattle01!;inspectedCounter=after.Battle.TurnCompletion!;
            var d=inspectedCounter.EnemyPhysicalAttack!;var c=d.Counterattack!;
            Require(inspectedCounter.CompletedActorIndex==130 && ReferenceEquals(inspectedCounter.Policy,Battle01PhysicalCompletionPolicy.ControlledNonlethalChesterCounterAndExp) &&
                d.TargetIndex==2 && c.Actor.Index==2 && c.Target.Index==130 && d.Destination==new MapPosition(8,4) &&
                d.Effect.BeforeStats.HpCurrent==7 && d.Effect.AfterStats.HpCurrent==5 &&
                c.Effect.BeforeStats.HpCurrent==5 && c.Effect.AfterStats.HpCurrent==4 && c.AwardedExp==5 &&
                c.Actor.Stats.CurrentExp==25 && c.ActorAfterStats.CurrentExp==30 &&
                after.Battle.RandomSeedImage==0xA1051234 && after.Battle.RandomSeedCopy==0x0234 &&
                after.Battle.FirstRound!.CurrentCandidate?.CombatantIndex==133,"One enemy receipt89 contains both ordered effects and Chester EXP");
            Require(d.Effect.Rolls.Concat(c.Effect.Rolls).Select(r=>r.Result).SequenceEqual(new ushort[]{10,12,0,0,2,0,3,4,0,0,6,25,4,10}),"All14 real counter/EXP draws");
            presenter.Project(after.Battle,"TEST COPY: counter receipt89. Actual133 is next.");
            Require(presenter.Projection!.AttackResult=="E2 -> A2: hit 2. HP 7 -> 5.\nCounter: A2 -> E2: hit 1. HP 5 -> 4. EXP +5: 25 -> 30." &&
                presenter.Projection.AllyStatus.Contains("A2 (9,4) HP 5 EXP 30") &&
                presenter.Projection.Units.Single(u=>u.Index==130).Hp==4,"Visible primary/counter HP and EXP result");
            await CaptureControl("29-counter-receipt89-test-copy","exact physical selection; Application primary/counter before automatic relay; no production pause");
            PrivateBattle01Ui.DispatchNext(_session,after);
            inspected=_session.PrivateOriginalBattle01!.Battle;
            Require(inspected.FirstControl?.ActorIndex==1 && inspected.Roster[2].Stats.HpCurrent==1 &&
                inspected.TurnCompletion!.CompletedActorIndex==129 && inspected.TurnCompletion.Previous!.CompletedActorIndex==133 &&
                ReferenceEquals(inspectedCounter,inspected.TurnCompletion.Previous.Previous),"Actual133 and129 reach Sarah with91 receipts");
        }
        finally
        {
            typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session,selected);
            presenter.Project(selected.Battle,"Restored exact physical Chester target selection. Space confirms.");
        }
        await PressBattleKey(Key.Space);
        var ready=_session.PrivateOriginalBattle01!;
        Require(JsonSerializer.Serialize(inspected,json)==JsonSerializer.Serialize(ready.Battle,json),"Physical Space and complete API relay match exactly");
        Require(ready.Battle.RandomSeedImage==0xF6711234 && ready.Battle.RandomSeedCopy==0x0134 &&
            ready.Battle.Roster[2].Stats.CurrentExp==30 && ready.Battle.Roster[5].Stats.HpCurrent==4 &&
            ready.Battle.CurrentGold==120 && ready.Battle.Roster[0].Stats.CurrentExp==63 && ready.Battle.Roster[0].Stats.CurrentKills==2 &&
            ready.Battle.Roster[2].Stats.CurrentKills is null,"Physical Sarah endpoint retains counter accounting and two prior corpses");
        await CaptureControl("30-counter-relay-sarah-ready");
        await PressBattleKey(Key.L);await PressBattleKey(Key.Space);
        Require(_session.PrivateOriginalBattle01!.Battle.Roster[1].Position==new MapPosition(10,17),"Actual Sarah movement confirmation");
        await CaptureControl("31-counter-relay-sarah-provisional");
        await PressBattleKey(Key.Backspace);
        var final=_session.PrivateOriginalBattle01!;
        Require(final.Battle.Roster[1].Position==new MapPosition(9,17) && ReferenceEquals(ready.Battle.TurnCompletion,final.Battle.TurnCompletion) &&
            final.Battle.Occupancy.SequenceEqual(ready.Battle.Occupancy) && final.Battle.RandomSeedImage==ready.Battle.RandomSeedImage &&
            final.Battle.Roster.Select(u=>u.Stats).SequenceEqual(ready.Battle.Roster.Select(u=>u.Stats)),"Actual Sarah cancel retains all combat results");
        await CaptureControl("32-counter-relay-sarah-cancel");
        var retained=final.Battle.TurnCompletion;
        for(int i=0;i<12;i++)retained=retained!.Previous;
        Require(ReferenceEquals(retained,chester.Battle.TurnCompletion) && JsonSerializer.Serialize(retained,json)==prefix,"All79 original receipt references and policies persist");
        File.WriteAllText(Path.Combine(_output,"receipt.json"),JsonSerializer.Serialize(new {
            status="Pass",scope="controlled Chester counter and actual R11 Sarah movement/cancel; natural presentation and H4 remain Unknown",
            preparation=final.Preparation.Party.Id,receipt89=inspectedCounter,battle=final.Battle,
            exactCopiedAndPhysicalSnapshotMatch=true,actualTargetCancelReselect=true,actualSarahMoveCancel=true,frames=_frames
        },json));
        GD.Print($"SF2_BATTLE01_CONTROL_NATIVE_REVIEW Pass frames={_frames.Count} chester-counterattack");
    }

    private async Task ReviewFirstAllyDefeat(PrivateOriginalBattle01SessionSnapshot chester)
    {
        var presenter = Field<PrivateBattle01Presenter>(_root, "_privateBattle01Presenter");
        var json = new JsonSerializerOptions { WriteIndented = true, MaxDepth = 256 };
        Require(ReferenceEquals(chester, _session.PrivateOriginalBattle01) &&
            chester.Preparation.Party.Id == OriginalBattle01ControlledPartyPreset.LeaderDefeatComparisonId &&
            chester.Preparation.Party.Allies[2].CurrentDefeats == 0 &&
            chester.Battle.Roster[2].Stats.CurrentDefeats == 0 && chester.Battle.Roster[2].Stats.CurrentKills is null,
            "Defeats0 comes only from the original named preparation");
        await PressBattleKey(Key.J); await PressBattleKey(Key.J);
        for (int i = 0; i < 5; i++) await PressBattleKey(Key.I);
        Require(_session.PrivateOriginalBattle01!.Battle.FirstControl!.Movement.Cursor == new MapPosition(9,9) &&
            _session.PrivateOriginalBattle01.Battle.FirstControl.Movement.GridCost == 14, "Actual Chester north preview costs14");
        await PressBattleKey(Key.Space);
        Require(_session.PrivateOriginalBattle01!.Battle.Roster[2].Position == new MapPosition(9,9), "Physical Chester move confirmation");
        await CaptureControl("26-chester-provisional9-9");
        await PressBattleKey(Key.Space);
        var seenRounds = new HashSet<int>();
        for (int choice = 1; choice < 12; choice++)
        {
            var current = _session.PrivateOriginalBattle01!;
            Require(current.Battle.FirstControl is not null && current.Battle.FirstRound!.RoundNumber is >= 10 and <= 13,
                "Physical relay returns each actual player in the twelve-choice route");
            int round = current.Battle.FirstRound!.RoundNumber;
            if (round > 10 && seenRounds.Add(round))
                await CaptureControl($"{round+16:D2}-round{round}-actual-player-ready");
            await PressBattleKey(Key.Space);
            if (choice < 11) await PressBattleKey(Key.Space);
        }
        var selected = _session.PrivateOriginalBattle01!;
        Require(selected.Battle.FirstControl?.ActorIndex == 0 && selected.Battle.FirstRound!.RoundNumber == 13 &&
            selected.Battle.FirstRound.CurrentTurnOffset == 6 && selected.Battle.Phase == Battle01Phase.PlayerActionChoice,
            "Twelfth actual player is R13 Bowie, confirmed at origin before STAY");
        var copied = (PrivateOriginalBattle01SessionSnapshot)Activator.CreateInstance(typeof(PrivateOriginalBattle01SessionSnapshot),
            BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object?[] {selected.Preparation, selected.Battle, selected.SourceLocomotion, selected.SourceBridge}, null)!;
        Battle01InitializedState boundary, defeated, pursued, inspected;
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session, copied);
        try
        {
            Require(_session.CommitPrivateOriginalBattle01Stay(copied, 0) is PrivateOriginalBattle01StayCommitted,
                "Copied exact physical choice commits Bowie STAY");
            var afterStay = _session.PrivateOriginalBattle01!;
            Require(_session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(afterStay, 129) is PrivateOriginalBattle01EnemyPhysicalAttackCompleted,
                "Actual129 attack reaches the lethal seam");
            var beforeAttack = _session.PrivateOriginalBattle01!;
            boundary = beforeAttack.Battle;
            var prefix = new List<Battle01TurnCompletionReceipt>();
            for (var r = boundary.TurnCompletion; r is not null; r = r.Previous) prefix.Add(r);
            Require(prefix.Count == 105 && prefix.All(r => r.AllyDefeat is null) &&
                boundary.FirstRound!.RoundNumber == 13 && boundary.FirstRound.CurrentTurnOffset == 10 &&
                boundary.FirstRound.CurrentCandidate?.CombatantIndex == 133 &&
                boundary.RandomSeedImage == 0x66531234 && boundary.RandomSeedCopy == 0x0134 &&
                boundary.Roster[2].Stats.HpCurrent == 1 && boundary.Roster[2].Stats.CurrentDefeats == 0 &&
                boundary.Roster[2].Position == new MapPosition(9,9) &&
                boundary.Roster.Skip(3).Select(u => u.Position).SequenceEqual(new MapPosition?[] {
                    new(9,6),new(9,8),new(8,5),null,null,new(8,9)}) &&
                boundary.Roster.Skip(3).Select(u => u.AiBitfield).SequenceEqual(new ushort?[] {
                    0x2061,0x2061,0x2061,0x2061,0x2071,0x2071}) &&
                boundary.AiMemory.SequenceEqual(new byte[]{4,52,20,36,52,4}.Concat(Enumerable.Repeat((byte)0,42))) &&
                boundary.AiLastTargets.SequenceEqual(new byte[]{255,2,255,0,0,2}.Concat(Enumerable.Repeat((byte)255,42))) &&
                boundary.RegionFlags90Through105.Select((flag,i) => flag == (i < 3)).All(equal => equal) &&
                boundary.NewlyTestedRegionMask == 0, "Actual copied route reaches the complete accepted105 boundary");
            CheckRoundBuffer(boundary, [2,1,128,0,129,133,130], [7,6,6,5,5,5,4]);
            try
            {
                Battle01EnemyPhysicalAttack.CompleteNext(boundary,133,Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike);
                throw new InvalidOperationException("Older policy must reject the lethal seam");
            }
            catch (Battle01PhysicalAttackUnsupportedException e) { Require(e.ParamName == "attack.lethal", "Old lethal guard"); }
            Require(ReferenceEquals(beforeAttack,_session.PrivateOriginalBattle01), "Old policy rejection retains the exact105 snapshot");
            presenter.Project(boundary, "TEST COPY: R13 enemy133 before first ally defeat. Physical STAY resumes after inspection.");
            await CaptureControl("30-first-ally-boundary105-test-copy", "exact physical action-choice copy; actual Application relay; no production pause");
            Require(_session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(beforeAttack,133) is PrivateOriginalBattle01EnemyPhysicalAttackCompleted,
                "Explicit prepared defeats input admits the sole first ally cleanup");
            defeated = _session.PrivateOriginalBattle01!.Battle;
            var receipt = defeated.TurnCompletion!; var d = receipt.EnemyPhysicalAttack!; var cleanup = receipt.AllyDefeat!;
            Require(ReferenceEquals(receipt.Previous,boundary.TurnCompletion) &&
                ReferenceEquals(receipt.Policy,Battle01PhysicalCompletionPolicy.ControlledFirstAllyDefeat) &&
                d.ActorIndex == 133 && d.TargetIndex == 2 && d.Origin == new MapPosition(8,9) &&
                d.Destination == d.Origin && d.GridCost == 0 && d.MoveString.SequenceEqual(new byte[]{255}) &&
                d.Priorities.Count == 1 && d.Priorities[0].Target.Index == 2 && d.Priorities[0].LandMultiplier == 230 &&
                d.Priorities[0].PotentialDamage == 2 && d.Priorities[0].RemainingHp == 0 && d.Priorities[0].Priority == 19 &&
                d.Priorities[0].Roll.Range == 3 && d.Priorities[0].Roll.GeneratorSteps == 133 &&
                d.Priorities[0].Roll.BeforeSeedCopy == 0x0134 && d.Priorities[0].Roll.AfterSeedCopy == 0x0234 &&
                d.Priorities[0].Roll.Result == 2 && d.Effect.Damage == 2 && d.Effect.TemporaryHp == 0 && d.Effect.RestoredHp == 1 &&
                d.Effect.Reaction == new Battle01PhysicalReaction(2,-2,0,0,1) && !d.Effect.Dodged && !d.Effect.Critical &&
                d.Effect.Rolls.Select(r => r.Range).SequenceEqual(new ushort[]{32,32,1,1}) &&
                d.Effect.Rolls.Select(r => r.Result).SequenceEqual(new ushort[]{6,17,0,0}) &&
                d.Effect.Rolls.Select(r => r.AfterImage).SequenceEqual(new uint[]{0x323E1234,0x8D2D1234,0x2B501234,0x33171234}) &&
                cleanup.FirstWorklist.SequenceEqual(new[]{2}) && cleanup.AfterTurnWorklist.Count == 0 &&
                cleanup.DefeatedAlly == 2 && cleanup.DefeatsBefore == 0 && cleanup.DefeatsAfter == 1 &&
                receipt.BeforeAfterTurn == new Battle01FactionCounts(2,4) && receipt.AfterAfterTurn == receipt.BeforeAfterTurn &&
                defeated.Roster[2].Position is null && defeated.Roster[2].Stats.HpCurrent == 0 &&
                defeated.Roster[2].Stats.CurrentDefeats == 1 && defeated.OccupantAt(new(9,9)) == -1 &&
                ReferenceEquals(boundary.Roster[6],defeated.Roster[6]) && ReferenceEquals(boundary.Roster[7],defeated.Roster[7]) &&
                ReferenceEquals(boundary.FirstRound!.Slots,defeated.FirstRound!.Slots),
                "Receipt106 preserves four main calls,133 thinking steps, the sole cleanup and both old corpses");
            presenter.Project(defeated,"TEST COPY: enemy133 defeats Chester. Defeats 0 -> 1. Before actual130.");
            Require(presenter.Projection!.Units.Count == 6 && !presenter.Projection.Units.Any(u => u.Index is 2 or 131 or 132) &&
                presenter.Projection.AllyStatus.Contains("A2 defeated HP 0 EXP 10 Defeats 1") &&
                presenter.Projection.AttackResult!.Contains("HP 1 -> 0"), "Visible dead status and removed marker");
            await CaptureControl("31-first-ally-defeat106-test-copy", "exact physical action-choice copy; Application death cleanup; no production pause");
            Require(_session.CompletePrivateOriginalBattle01EnemyPursuit(_session.PrivateOriginalBattle01,130) is PrivateOriginalBattle01EnemyPursuitCompleted,
                "Actual130 continues after the first ally defeat");
            pursued = _session.PrivateOriginalBattle01!.Battle; var pursuit = pursued.TurnCompletion!.EnemyPursuit!;
            Require(pursuit.Origin == new MapPosition(8,5) && pursuit.Destination == new MapPosition(9,5) &&
                pursuit.GridCost == 2 && pursuit.MoveString.SequenceEqual(new byte[]{0,255}) &&
                pursuit.TargetCosts.SequenceEqual(new[]{new Battle01PursuitTargetCost(0,26),new(1,26)}) &&
                pursuit.TargetIndex == 0 && pursuit.PreliminaryDestination == new MapPosition(9,6) &&
                pursuit.PreliminaryMoveString.SequenceEqual(new byte[]{0,3,255}) &&
                pursued.RandomSeedImage == 0x33171234 && pursued.RandomSeedCopy == 0x0234,
                "Receipt107 excludes Chester from pursuit and preserves both random channels");
            presenter.Project(pursued,"TEST COPY: actual130 pursuit, before six-survivor R14 generation.");
            await CaptureControl("32-pursuit107-test-copy", "exact physical action-choice copy; actual Application pursuit before generation");
            PrivateBattle01Ui.DispatchNext(_session,_session.PrivateOriginalBattle01!);
            inspected = _session.PrivateOriginalBattle01!.Battle;
        }
        finally
        {
            typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session, selected);
            presenter.Project(selected.Battle,"Restored exact physical R13 Bowie action choice. Space commits STAY.");
        }
        await PressBattleKey(Key.Space);
        var sarah = _session.PrivateOriginalBattle01!; var end = sarah.Battle;
        Require(JsonSerializer.Serialize(inspected,json) == JsonSerializer.Serialize(end,json) &&
            ReferenceEquals(selected.Battle.TurnCompletion,end.TurnCompletion!.Previous!.Previous!.Previous!.Previous),
            "Physical twelfth STAY and copied relay match the entire107 state and preserve the old prefix");
        Require(end.FirstRound!.RoundNumber == 14 && end.FirstRound.CurrentTurnOffset == 0 &&
            end.FirstControl?.ActorIndex == 1 && end.FirstControl.Movement.Range.Budget == 10 &&
            end.RandomSeedImage == 0x02A11234 && end.RandomSeedCopy == 0x0234 && end.NewlyTestedRegionMask == 7 &&
            end.Roster.Count == 9 && end.Occupancy.Count(i => i >= 0) == 6 &&
            end.Roster[0].Stats.HpCurrent == 3 && end.Roster[0].Stats.CurrentExp == 63 && end.Roster[0].Stats.CurrentKills == 2 &&
            end.Roster[1].Stats.HpCurrent == 11 && end.Roster[2].Stats.CurrentExp == 10 && end.Roster[2].Stats.CurrentKills is null &&
            end.Roster[2].Stats.CurrentDefeats == 1 && end.CurrentGold == 120 &&
            ReferenceEquals(chester.Preparation,sarah.Preparation) && ReferenceEquals(chester.SourceBridge,sarah.SourceBridge) &&
            ReferenceEquals(chester.SourceLocomotion,sarah.SourceLocomotion), "Actual R14 Sarah inherits all107 receipts and prepared origin");
        CheckRoundBuffer(end,[1,129,128,130,0,133],[6,6,5,5,4,4]);
        string? attackText = presenter.Projection!.AttackResult;
        string allyText = presenter.Projection.AllyStatus;
        await CaptureControl("33-round14-sarah-after-physical-relay");
        await PressBattleKey(Key.L);
        Require(_session.PrivateOriginalBattle01!.Battle.FirstControl!.Movement.GridCost == 2,"Sarah preview costs2");
        await PressBattleKey(Key.Space);
        Require(_session.PrivateOriginalBattle01!.Battle.Roster[1].Position == new MapPosition(10,17),"Physical Sarah move");
        await CaptureControl("34-round14-sarah-provisional10-17");
        await PressBattleKey(Key.Backspace);
        var final = _session.PrivateOriginalBattle01!;
        Require(final.Battle.Roster[1].Position == new MapPosition(9,17) &&
            final.Battle.Occupancy.SequenceEqual(end.Occupancy) &&
            ReferenceEquals(final.Battle.TurnCompletion,end.TurnCompletion) &&
            final.Battle.Roster.Select(u => u.Stats).SequenceEqual(end.Roster.Select(u => u.Stats)) &&
            final.Battle.RandomSeedImage == end.RandomSeedImage && final.Battle.RandomSeedCopy == end.RandomSeedCopy &&
            presenter.Projection!.AttackResult == attackText && presenter.Projection.AllyStatus == allyText,
            "Sarah cancel preserves all dead status, accounting, AI and history before her action");
        await CaptureControl("35-round14-sarah-move-cancel");
        if (System.Environment.GetEnvironmentVariable("SF2_BATTLE01_CONTROL_REVIEW") == "leader-defeat-pending")
        { await ReviewLeaderDefeatPending(final); return; }
        File.WriteAllText(Path.Combine(_output,"receipt.json"),JsonSerializer.Serialize(new {
            status="Pass", scope="controlled first Chester defeat through physical R14 Sarah movement/cancel; natural defeats, timing and H4 Unknown",
            preparation=final.Preparation.Party.Id, boundary105=boundary, defeated106=defeated, pursued107=pursued,
            battle=final.Battle, exactCopiedAndPhysicalSnapshotMatch=true, actualPlayerChoices=12,
            actualSarahMoveCancel=true, frames=_frames
        },json));
        GD.Print($"SF2_BATTLE01_CONTROL_NATIVE_REVIEW Pass frames={_frames.Count} first-ally-defeat");
    }

    private async Task ReviewLeaderDefeatPending(PrivateOriginalBattle01SessionSnapshot sarah)
    {
        var presenter = Field<PrivateBattle01Presenter>(_root, "_privateBattle01Presenter");
        var json = new JsonSerializerOptions { MaxDepth = 256 };
        Require(sarah.Preparation.Party.Id == OriginalBattle01ControlledPartyPreset.LeaderDefeatComparisonId &&
            sarah.Preparation.Party.Allies[0].CurrentDefeats == 0 && sarah.Battle.Roster[0].Stats.CurrentDefeats == 0,
            "Bowie defeats0 was supplied before real initialization and all107 receipts");
        var stages = new[] { (14,0,111,"36-round14-bowie"), (15,0,115,"37-round15-bowie"), (15,1,116,"38-round15-sarah") };
        foreach (var (round, actor, count, name) in stages)
        {
            await PressBattleKey(Key.Space); await PressBattleKey(Key.Space);
            var b = _session.PrivateOriginalBattle01!.Battle;
            Require(b.FirstRound!.RoundNumber == round && b.FirstControl?.ActorIndex == actor && ReceiptCount(b) == count,
                "Actual origin STAY reaches the next planned player through complete automatic dispatch");
            await CaptureControl(name);
        }
        await PressBattleKey(Key.Space);
        var selected = _session.PrivateOriginalBattle01!;
        Require(selected.Battle.FirstControl?.ActorIndex == 1 && selected.Battle.Phase == Battle01Phase.PlayerActionChoice,
            "Actual R15 Sarah origin action choice precedes the fourth STAY");
        await CaptureControl("39-round15-sarah-action-choice");
        string frozen = JsonSerializer.Serialize(selected.Battle,json);
        Battle01InitializedState? boundary = null, inspected = null;
        try
        {
            var current = ((PrivateOriginalBattle01StayCommitted)_session.CommitPrivateOriginalBattle01Stay(selected,1)).Snapshot;
            for (int step=0;step<8;step++)
            {
                var order=current.Battle.FirstRound!;
                if(order.RoundNumber==16 && order.CurrentTurnOffset==0) break;
                current=order.CurrentCandidate is not {} candidate
                    ? ((PrivateOriginalBattle01FirstRoundEntered)_session.EnterPrivateOriginalBattle01NextRound(current)).Snapshot
                    : ((PrivateOriginalBattle01EnemyPursuitCompleted)_session.CompletePrivateOriginalBattle01EnemyPursuit(current,candidate.CombatantIndex)).Snapshot;
            }
            boundary=current.Battle;
            Require(boundary.FirstRound!.RoundNumber==16 && boundary.FirstRound.CurrentTurnOffset==0 &&
                boundary.FirstRound.CurrentCandidate?.CombatantIndex==129 && ReceiptCount(boundary)==119 &&
                boundary.RandomSeedImage==0x9B651234 && boundary.RandomSeedCopy==0x0234 && boundary.NewlyTestedRegionMask==7 &&
                boundary.Roster[0].Stats.HpCurrent==3 && boundary.Roster[0].Stats.CurrentDefeats==0,
                "Exact119-prefix leader boundary is produced before the terminal strike");
            CheckRoundBuffer(boundary,[129,130,0,133,1,128],[6,6,5,5,4,4]);
            presenter.Project(boundary,"TEST COPY: first leader-defeat boundary before enemy129.");
            await CaptureControl("40-leader-boundary119-test-copy","exact physical Sarah action-choice copy; unmodified Application continuation");
            foreach(var policy in new[]{Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike,Battle01PhysicalCompletionPolicy.ControlledFirstAllyDefeat})
            {
                try { Battle01EnemyPhysicalAttack.CompleteNext(boundary,129,policy); throw new InvalidOperationException("Old policy must retain lethal guard"); }
                catch(Battle01PhysicalAttackUnsupportedException e) { Require(e.ParamName=="attack.lethal","Both older policies retain lethal guard"); }
            }
            current=((PrivateOriginalBattle01EnemyPhysicalAttackCompleted)_session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(current,129)).Snapshot;
            inspected=current.Battle;
            CheckTerminal(inspected,boundary);
            string terminalStatus=PrivateBattle01Ui.DispatchNext(_session,current);
            Require(ReferenceEquals(current,_session.PrivateOriginalBattle01),"Terminal dispatcher does not reuse historical129");
            presenter.Project(inspected,"TEST COPY: "+terminalStatus);
            await CaptureControl("41-defeat-pending-test-copy","exact physical action-choice copy; terminal effect before actual key dispatch");
        }
        finally
        {
            typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session,selected);
            presenter.Project(selected.Battle,"Restored exact R15 Sarah action choice. Space commits STAY.");
        }
        Require(JsonSerializer.Serialize(selected.Battle,json)==frozen,"Test inspection did not mutate the physical before-image");
        await PressBattleKey(Key.Space);
        var final=_session.PrivateOriginalBattle01!;
        Require(JsonSerializer.Serialize(inspected,json)==JsonSerializer.Serialize(final.Battle,json),
            "Actual physical STAY and automatic dispatch reproduce the complete inspected terminal state");
        CheckTerminal(final.Battle,boundary!);
        Require(PrivateBattle01Ui.DispatchNext(_session,final)=="Bowie defeated. Space applies HP/gold recovery.",
            "Terminal dispatcher stops and advertises the available recovery confirmation");
        await CaptureControl("42-defeat-pending-physical");
        string finalJson=JsonSerializer.Serialize(final.Battle,json);
        var inspectedRecovery=((PrivateOriginalBattle01DefeatRecovered)_session.RecoverPrivateOriginalBattle01Defeat(final)).Snapshot;
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session,final);
        await PressBattleKey(Key.Space);
        var recovered=_session.PrivateOriginalBattle01!;
        Require(JsonSerializer.Serialize(recovered.Battle,json)==JsonSerializer.Serialize(inspectedRecovery.Battle,json),
            "Actual Space exactly matches the independently invoked recovery API");
        Require(ReferenceEquals(recovered.Battle.DefeatRecovery!.Before,final.Battle) &&
            ReferenceEquals(recovered.Battle.DefeatPending,final.Battle.DefeatPending) &&
            ReferenceEquals(recovered.Battle.TurnCompletion,final.Battle.TurnCompletion) &&
            recovered.Battle.Phase==Battle01Phase.DefeatRecoveryPending && recovered.Battle.Roster[0].Stats.HpCurrent==12 &&
            recovered.Battle.Roster[0].Position is null && recovered.Battle.CurrentGold==60 &&
            recovered.Battle.Occupancy.Count(i=>i>=0)==5 && ReceiptCount(recovered.Battle)==119,
            "Recovery preserves the terminal before-image, historical count and absent leader placement");
        var expectedRecovery=JsonSerializer.SerializeToNode(final.Battle,json)!;
        expectedRecovery["Roster"]![0]!["Stats"]!["HpCurrent"]=12;
        expectedRecovery["CurrentGold"]=60; expectedRecovery["Phase"]=(int)Battle01Phase.DefeatRecoveryPending;
        var actualRecovery=JsonSerializer.SerializeToNode(recovered.Battle,json)!;
        actualRecovery["DefeatRecovery"]=null;
        Require(System.Text.Json.Nodes.JsonNode.DeepEquals(expectedRecovery,actualRecovery),
            "Only leader HP and gold change in the complete state, plus the linked recovery facet/phase");
        Require(JsonSerializer.Serialize(final.Battle,json)==finalJson,"Terminal before-image remains frozen after recovery");
        await CaptureControl("43-defeat-recovery-physical");
        string recoveredJson=JsonSerializer.Serialize(recovered.Battle,json);
        Require(recovered.CanRequestDefeatReturn && ReferenceEquals(_initialBattle01!.Preparation,recovered.Preparation) &&
            ReferenceEquals(_initialBattle01.SourceSnapshot,recovered.SourceSnapshot) &&
            ReferenceEquals(_initialBattle01.SourceLocomotion,recovered.SourceLocomotion) &&
            ReferenceEquals(_initialBattle01.SourceBridge,recovered.SourceBridge) &&
            ReferenceEquals(_initialBattle01.Battle.ReturnAdmission,recovered.Battle.ReturnAdmission) &&
            ReferenceEquals(recovered.Battle.ReturnAdmission,final.Battle.ReturnAdmission),
            "The actual initial preparation, source references and return binding survive the entire physical route");
        var inspectedReturn=((PrivateOriginalBattle01DefeatReturnRequested)_session.RequestPrivateOriginalBattle01DefeatReturn(recovered)).Snapshot;
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session,recovered);
        await PressBattleKey(Key.Space);
        var requested=_session.PrivateOriginalBattle01!; var request=requested.DefeatReturn!;
        Require(JsonSerializer.Serialize(request,json)==JsonSerializer.Serialize(inspectedReturn.DefeatReturn,json) &&
            ReferenceEquals(recovered.Battle,requested.Battle) && ReferenceEquals(recovered.Preparation,requested.Preparation) &&
            ReferenceEquals(recovered.SourceSnapshot,requested.SourceSnapshot) && ReferenceEquals(recovered.SourceBridge,requested.SourceBridge) &&
            ReferenceEquals(recovered.SourceLocomotion,requested.SourceLocomotion) && ReferenceEquals(request.Recovery,recovered.Battle.DefeatRecovery),
            "Physical Space reproduces the return API and retains the complete recovered battle and source references");
        Require(request.SavepointMap==new MapId("map3") && request.SavepointPosition==new MapPosition(32,13) &&
            request.SavepointOpaqueFacing==1 && request.DestinationMap==request.SavepointMap &&
            request.DestinationPosition==request.SavepointPosition && request.DestinationOpaqueFacing==1 &&
            !request.RaftWriteExecuted && request.HandlerResultD4==-1 && !request.ExplorationEntered &&
            _session.PrivateOriginalFlowStage==GameFlowStage.Battle && _session.PrivateOriginalCurrentMap==new MapId("map57") &&
            !requested.CanRequestDefeatReturn && presenter.Projection!.Status==PrivateBattle01Presenter.ReturnRequestedStatus,
            "Only the admitted Granseal request is published; live map57/Battle remains before ExplorationLoop");
        await CaptureControl("44-defeat-return-request-physical");
        Require(requested.CanEnterExploration && requested.Preparation.ArrivalInputs ==
            OriginalBattle01ControlledArrivalInputs.GransealFirstAttemptComparison &&
            presenter.Projection!.CanEnterExploration && presenter.Projection.Controls.Contains("Space: enter Granseal"),
            "The original early preparation enables exactly one visible entry confirmation");
        var inspectedEntry=((PrivateOriginalBattle01ExplorationEntered)_session.EnterPrivateOriginalBattle01Exploration(requested)).Snapshot;
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session,requested);
        await PressBattleKey(Key.Space);
        var entered=_session.PrivateOriginalBattle01!; var arrival=entered.Arrival!;
        Require(JsonSerializer.Serialize(arrival,json)==JsonSerializer.Serialize(inspectedEntry.Arrival,json) &&
            ReferenceEquals(arrival.Before,requested) && ReferenceEquals(entered.Battle,requested.Battle) &&
            ReferenceEquals(entered.DefeatReturn,request) && ReferenceEquals(entered.Preparation,requested.Preparation) &&
            ReferenceEquals(entered.SourceSnapshot,requested.SourceSnapshot) &&
            JsonSerializer.Serialize(entered.Battle,json)==recoveredJson,
            "Physical entry equals the direct API in full and preserves the complete historical battle/request/source");
        var arrivalRoof=(MapBlockCopyLifecycleActiveState)arrival.RoofClear.LifecycleState;
        var inspectedRoof=(MapBlockCopyLifecycleActiveState)inspectedEntry.Arrival!.RoofClear.LifecycleState;
        Require(arrivalRoof.RecordOrdinal==inspectedRoof.RecordOrdinal &&
            arrivalRoof.DestinationX==inspectedRoof.DestinationX && arrivalRoof.DestinationY==inspectedRoof.DestinationY &&
            arrivalRoof.Width==inspectedRoof.Width && arrivalRoof.Height==inspectedRoof.Height &&
            arrivalRoof.SavedWords.SequenceEqual(inspectedRoof.SavedWords),
            "The derived roof lifecycle and all30 saved words also match the direct API");
        // Read the exact before-image owners rather than duplicate the entry reducer's flag list.
        var story=requested.SourceSnapshot;
        object[] flagOwners=[story.MiddleTowerGuard!,story.Zone601!,story.Sarah!,story.Entity142!,
            story.MessengerAcceptance!,story.CastleGate!,story.PalaceFirstVisit!,story.AstralAcceptance!,
            requested.Battle,requested.DefeatReturn!.Admission];
        var knownFlagsBefore=new Dictionary<int,bool>();
        foreach(var owner in flagOwners) foreach(var property in owner.GetType().GetProperties()) {
            var match=System.Text.RegularExpressions.Regex.Match(property.Name,@"Flag(\d+)(?:Set)?$");
            if(property.PropertyType!=typeof(bool) || !match.Success)continue;
            int flag=int.Parse(match.Groups[1].Value);bool value=(bool)property.GetValue(owner)!;
            Require(!knownFlagsBefore.TryGetValue(flag,out bool earlier) || earlier==value,"Before-image flag owners agree");
            knownFlagsBefore[flag]=value;
        }
        foreach(var (value,index) in requested.Battle.RegionFlags90Through105.Select((v,i)=>(v,i)))
            knownFlagsBefore.Add(90+index,value);
        var expectedFlags=knownFlagsBefore.ToDictionary(p=>p.Key,p=>p.Value);
        foreach(var pair in requested.Preparation.ArrivalInputs!.Flags)expectedFlags.Add(pair.Key,pair.Value);
        foreach(int flag in Enumerable.Range(256,128))expectedFlags[flag]=false;
        expectedFlags[80]=true;
        Require(expectedFlags.OrderBy(p=>p.Key).SequenceEqual(arrival.Flags.OrderBy(p=>p.Key)) &&
            knownFlagsBefore.Any(p=>p.Value) && knownFlagsBefore.Any(p=>!p.Value) &&
            Enumerable.Range(256,128).All(flag=>!arrival.Flags[flag]) &&
            !arrival.Flags.ContainsKey(89) && !arrival.Flags.ContainsKey(606),
            "Current entry image preserves known true/false before-image values, resets all128 temp flags and leaves unknowns absent");
        await CaptureArrival("45-granseal-entry-physical");
        var returnMovement = await ExerciseReturnMovement(entered);
        File.WriteAllText(Path.Combine(_output,"receipt.json"),JsonSerializer.Serialize(new {
            status="Pass",scope="controlled defeat recovery/return, church doorway cycle and settlement roof actions; original scripts unsupported",
            preparation=final.Preparation.Party,returnInputs=_initialBattle01!.Preparation.ReturnInputs,
            arrivalInputs=_initialBattle01.Preparation.ArrivalInputs,
            initialBattle=_initialBattle01.Battle,start107=sarah.Battle,boundary119=boundary,terminal=final.Battle,battle=entered.Battle,
            defeatReturn=request,arrival,arrivalRoof,knownFlagsBefore,entryFlagImagePreserved=true,returnMovement,
            earlyBindingAndSourceReferencesRetained=true,exactRecoveredBattleReferenceRetained=true,
            exactCopiedAndPhysicalSnapshotMatch=true,exactRecoveryApiAndPhysicalMatch=true,exactReturnApiAndPhysicalMatch=true,
            exactEntryApiAndPhysicalMatch=true,actualPlayerChoices=4,recoveryConfirmations=1,returnConfirmations=1,
            entryConfirmations=1,frames=_frames
        },json));
        GD.Print($"SF2_BATTLE01_CONTROL_NATIVE_REVIEW Pass frames={_frames.Count} leader-defeat-pending");

        static int ReceiptCount(Battle01InitializedState b) { int n=0;for(var r=b.TurnCompletion;r is not null;r=r.Previous)n++;return n; }
        void CheckTerminal(Battle01InitializedState b,Battle01InitializedState prior)
        {
            var t=b.DefeatPending!; var d=t.Attack;
            Require(b.Phase==Battle01Phase.DefeatPending && b.FirstControl is null && ReceiptCount(b)==119 &&
                b.FirstRound!.CurrentTurnOffset==0 && b.FirstRound.CurrentCandidate?.CombatantIndex==129 &&
                JsonSerializer.Serialize(b.TurnCompletion,json)==JsonSerializer.Serialize(prior.TurnCompletion,json) &&
                ReferenceEquals(t.Previous,b.TurnCompletion) && !t.AfterTurnExecuted && !t.TurnAdvanced &&
                t.FirstOutcome==new Battle01FactionCounts(0,4) && t.Cleanup.FirstWorklist.SequenceEqual(new[]{0}) &&
                t.Cleanup.AfterTurnWorklist.Count==0 && t.Cleanup.DefeatsBefore==0 && t.Cleanup.DefeatsAfter==1,
                "Terminal receipt preserves119 prior receipts and stops after first[0] cleanup/count");
            Require(d.ActorIndex==129 && d.TargetIndex==0 && d.Origin==new MapPosition(11,10) && d.Destination==new MapPosition(11,14) &&
                d.GridCost==8 && d.MoveString.SequenceEqual(new byte[]{3,3,3,3,255}) && d.Priorities.Count==1 &&
                d.Priorities[0].Priority==16 && d.Priorities[0].LandMultiplier==230 && d.Priorities[0].Roll.GeneratorSteps==66 &&
                d.Priorities[0].Roll.Result==0 && d.Effect.Damage==3 && d.Effect.TemporaryHp==0 && d.Effect.RestoredHp==3 &&
                d.Effect.Reaction==new Battle01PhysicalReaction(0,-3,0,0,1) && !d.Effect.Dodged && !d.Effect.Critical &&
                d.Effect.Rolls.Select(r=>r.Result).SequenceEqual(new ushort[]{28,18,0,0}) &&
                b.RandomSeedImage==0x10491234 && b.RandomSeedCopy==0x0034 && b.NewlyTestedRegionMask==0,
                "Controlled source-bound movement, priority, rolls, reaction and both RNG channels");
            Require(b.Roster[0].Position is null && b.Roster[0].Stats.HpCurrent==0 && b.Roster[0].Stats.CurrentDefeats==1 &&
                b.Roster[1].Stats.HpCurrent==11 && b.Roster[2].Position is null && b.Roster[2].Stats.CurrentDefeats==1 &&
                b.Roster[2].Stats.CurrentExp==10 && b.CurrentGold==120 && b.Roster[0].Stats.CurrentExp==63 && b.Roster[0].Stats.CurrentKills==2 &&
                b.Occupancy.Count(i=>i>=0)==5 && b.OccupantAt(new(11,15))==-1 && b.OccupantAt(new(11,14))==129,
                "Leader cleanup retains Sarah, prior deaths and pre-handler accounting");
        }
    }

    private static void CheckRoundBuffer(Battle01InitializedState battle, byte[] actors, byte[] scores)
    {
        var slots=battle.FirstRound!.Slots;
        Require(slots.Count==64 && slots.Take(actors.Length).Select(slot=>slot.CombatantIndex).SequenceEqual(actors) &&
            slots.Take(actors.Length).Select(slot=>slot.AlteredAgility).SequenceEqual(scores) &&
            slots.Skip(actors.Length).All(slot=>slot==new Battle01TurnEntry(255,255)), "Complete current 64-slot buffer");
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

    private async Task<object> ExerciseReturnMovement(PrivateOriginalBattle01SessionSnapshot entered)
    {
        var entry = entered.Arrival!; var json = new JsonSerializerOptions { MaxDepth=256 };
        string battleBefore = JsonSerializer.Serialize(entered.Battle,json);
        string partyBefore = JsonSerializer.Serialize(entry.Party,json);
        var layoutBefore = entry.WorkingLayout.Words.ToArray();
        var operations = new List<object>(); int nextFrame=46, callbacks=0, idleCallbacks=0;
        Dictionary<string,byte[]>? lastPixels=null;
        // Retain real physical-key polling. Schedule each production physics callback explicitly
        // on a physics frame so initial/mid/settled captures cannot skip or double-advance a tick.
        _root.SetPhysicsProcess(false);
        void Invariant()
        {
            var current = _session.PrivateOriginalBattle01!; var live = current.Arrival!;
            Require(ReferenceEquals(entered.Battle,current.Battle) && JsonSerializer.Serialize(current.Battle,json)==battleBefore &&
                ReferenceEquals(entered.Preparation,current.Preparation) && ReferenceEquals(entered.DefeatReturn,current.DefeatReturn) &&
                ReferenceEquals(entered.SourceSnapshot,current.SourceSnapshot) && ReferenceEquals(entered.SourceLocomotion,current.SourceLocomotion) &&
                ReferenceEquals(entered.SourceBridge,current.SourceBridge) && ReferenceEquals(entry.Before,live.Before),"Complete battle/source history remains immutable");
            Require(ReferenceEquals(entry.Party,live.Party) && JsonSerializer.Serialize(live.Party,json)==partyBefore &&
                ReferenceEquals(entry.Flags,live.Flags) && layoutBefore.SequenceEqual(entry.WorkingLayout.Words) &&
                ReferenceEquals(entry.RoofClear,live.RoofClear) &&
                ReferenceEquals(entry.LoadDefinition,live.LoadDefinition),"Current party, flags, resources and roof before-image are retained");
            var saved=(MapBlockCopyLifecycleActiveState)entry.RoofLifecycle;
            var expected=layoutBefore.ToArray();
            if(live.DoorCopy is { } door) {
                Require(ReferenceEquals(door.Entry,entry) && ReferenceEquals(door.Definition,entry.LoadDefinition.ChurchDoor) &&
                    door.BeforeWord==0xC48F && door.AfterWord==0x080E && door.Definition.Identity.OneBasedRecordOrdinal==4,
                    "Only the entry-bound row4 door receipt can open the main-layer word");
                expected[15*64+32]=0x080E;
            }
            if(live.RoofLifecycle is MapBlockCopyLifecycleInactiveState)
                for(int y=41;y<47;y++) for(int x=30;x<35;x++) expected[y*64+x]=saved.SavedWords[(y-41)*5+x-30];
            else Require(live.RoofLifecycle is MapBlockCopyLifecycleActiveState active && active.RecordOrdinal==8 &&
                active.SavedWords.SequenceEqual(saved.SavedWords),"The full ordered roof table retains saved30/ordinal8");
            Require(expected.SequenceEqual(live.WorkingLayout.Words),"All4096 current words equal entry plus only the door/roof deltas");
            Require(entry.Entities.Where(e=>e.Id!=0).All(e=>ReferenceEquals(e,live.Entities.Single(r=>r.Id==e.Id))),
                "All non-player declarations, including dead Chester and both entity142 effects, stay exact");
            var player=live.Entities.Single(e=>e.Id==0);
            Require(player.Position==live.PlayerPosition && player.TargetPosition==live.PlayerPosition && player.Facing==live.PlayerOpaqueFacing &&
                player.DeclarationPosition==new MapPosition(32,13),"Current player pose and historical declaration stay distinct");
        }
        async Task Capture(string suffix)
        {
            Invariant(); lastPixels=await CaptureReturnMovement($"{nextFrame++:00}-granseal-{suffix}");
        }
        async Task Direction(Key key, ExplorationDirection direction, MapPosition destination, bool moved, string? unsupported=null,
            bool detailed=false,string? phase=null)
        {
            var before=_session.PrivateOriginalBattle01!;
            var beforePixels=lastPixels; Dictionary<string,byte[]>? preSettlementPixels=null;
            var expectedResult=_session.BeginPrivateOriginalMapReturnMovement(before,new(direction));
            var expectedStart=_session.PrivateOriginalBattle01!;
            var expectedTicks=new List<PrivateOriginalBattle01SessionSnapshot>{expectedStart};
            while(_session.PrivateOriginalMapArrival!.Locomotion.IsMoving)
                expectedTicks.Add(((PrivateOriginalMapReturnMovementApplied)_session.AdvancePrivateOriginalMapReturnMovement(_session.PrivateOriginalBattle01)).Snapshot);
            typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(_session,before);
            await PressBattleKey(key);
            var actual=_session.PrivateOriginalBattle01!;
            Require(JsonSerializer.Serialize(actual.Arrival,json)==JsonSerializer.Serialize(expectedStart.Arrival,json),
                "Physical direction has the exact independently invoked API result from the same current snapshot");
            if(unsupported is not null) {
                Require(expectedResult is PrivateOriginalMapReturnMovementUnsupported denied && denied.Diagnostic.Field==unsupported &&
                    ReferenceEquals(before,actual),"Unsupported target has no traversal, facing, counter or snapshot mutation");
                Require(Field<Label>(_presenter,"_status").Text.Contains("Unsupported"),"Unsupported capability is visible");
                await Capture(unsupported=="return.followers"?"follower-cell-unsupported":"door-or-boundary-unsupported");
            }
            else {
                Require(expectedResult is PrivateOriginalMapReturnMovementApplied && actual.Arrival!.PlayerPosition==destination &&
                    actual.Arrival.Locomotion.IsMoving==moved && actual.Arrival.InputOrdinal==before.Arrival!.InputOrdinal+1,
                    "Supported move/block has the expected position and exactly one input ordinal");
                Require(actual.Arrival!.LastInput!.Traversal.Outcome==(moved?OriginalMapTraversalOutcome.Moved:OriginalMapTraversalOutcome.BlockedByCollision),
                    "Terrain collision remains distinct from unavailable events");
                if(detailed) {
                    await Capture(phase is null ? "move-initial" : phase+"-begin");
                    if(phase is not null && beforePixels is not null) {
                        Require(lastPixels!["roof"].SequenceEqual(beforePixels["roof"]),"Begin cannot change visible roof pixels");
                        if(before.Arrival!.DoorCopy is null && actual.Arrival.DoorCopy is not null)
                            Require(!lastPixels["door"].SequenceEqual(beforePixels["door"]),"Door pixels change at the actual first Begin");
                    }
                }
                for(int index=1;index<expectedTicks.Count;index++) {
                    await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
                    _root._PhysicsProcess(1.0/60.0); callbacks++;
                    Require(JsonSerializer.Serialize(_session.PrivateOriginalMapArrival,json)==JsonSerializer.Serialize(expectedTicks[index].Arrival,json),
                        "Every real production physics callback equals the corresponding API tick");
                    Invariant();
                    if(detailed && _session.PrivateOriginalMapArrival!.Locomotion.Tick==7)
                        await Capture(phase is null ? "move-middle" : phase+"-middle");
                    if(phase is not null && _session.PrivateOriginalMapArrival!.Locomotion.Tick==12) {
                        await Capture(phase+"-before-settlement"); preSettlementPixels=lastPixels;
                    }
                }
                Require(!_session.PrivateOriginalMapArrival!.Locomotion.IsMoving,"Movement is settled before the next input");
                await Capture(phase is not null ? phase+"-settled" : moved?"move-settled":"terrain-blocked-facing");
                if(preSettlementPixels is not null) {
                    var action=_session.PrivateOriginalMapArrival!.LastRoofAction!;
                    bool changes=action.Action.Outcome is MapBlockCopyActionOutcome.Restored or MapBlockCopyActionOutcome.Activated;
                    Require(changes!=lastPixels!["roof"].SequenceEqual(preSettlementPixels["roof"]),
                        "Settlement alone changes the sampled visible roof pixels on restore/activation");
                    Require(lastPixels["door"].SequenceEqual(preSettlementPixels["door"]),"Roof settlement never recloses the doorway art");
                }
            }
            operations.Add(new {key=key.ToString(),direction,destination,moved,unsupported,
                beforeOrdinal=before.Arrival!.InputOrdinal,afterOrdinal=_session.PrivateOriginalMapArrival!.InputOrdinal,
                phase,apiTicks=expectedTicks.Select(t=>new {t.Arrival!.Locomotion,t.Arrival.InputOrdinal,
                    roof=t.Arrival.LastRoofAction?.Action.Outcome,roofInput=t.Arrival.LastRoofAction?.InputOrdinal,
                    activeRoof=(t.Arrival.RoofLifecycle as MapBlockCopyLifecycleActiveState)?.RecordOrdinal,
                    doorInput=t.Arrival.DoorCopy?.Input.Ordinal}).ToArray()});
        }
        await Direction(Key.W,ExplorationDirection.North,new(32,13),false);
        await Direction(Key.A,ExplorationDirection.West,new(31,13),true,detailed:true);
        await Direction(Key.D,ExplorationDirection.East,new(31,13),false,"return.followers");
        await Direction(Key.W,ExplorationDirection.North,new(31,12),true);
        await Direction(Key.D,ExplorationDirection.East,new(31,12),false);
        await Direction(Key.S,ExplorationDirection.South,new(31,13),true);
        await Direction(Key.S,ExplorationDirection.South,new(31,14),true);
        await Direction(Key.S,ExplorationDirection.South,new(31,14),false,"return.region");
        await Direction(Key.D,ExplorationDirection.East,new(32,14),true);
        await Direction(Key.W,ExplorationDirection.North,new(32,14),false,"return.followers");
        await Direction(Key.D,ExplorationDirection.East,new(33,14),true);
        await Direction(Key.W,ExplorationDirection.North,new(33,13),true);
        await Direction(Key.W,ExplorationDirection.North,new(33,12),true);
        await Direction(Key.A,ExplorationDirection.West,new(33,12),false);
        await Direction(Key.S,ExplorationDirection.South,new(33,13),true);
        await Direction(Key.S,ExplorationDirection.South,new(33,14),true);
        await Direction(Key.A,ExplorationDirection.West,new(32,14),true);
        await Direction(Key.A,ExplorationDirection.West,new(31,14),true);
        await Direction(Key.W,ExplorationDirection.North,new(31,13),true);
        await Direction(Key.W,ExplorationDirection.North,new(31,12),true);
        await Direction(Key.S,ExplorationDirection.South,new(31,13),true);
        var retained=_session.PrivateOriginalBattle01!;
        var interactionKeys = new[]{Key.F,Key.G};
        foreach(var key in interactionKeys) {
            await PressBattleKey(key); Require(ReferenceEquals(retained,_session.PrivateOriginalBattle01),"Interaction cannot dispatch old handlers");
            Require(Field<Label>(_presenter,"_status").Text.Contains("Unsupported: interactions"),"Interaction refusal is explicit");
            await Capture("interaction-"+key.ToString().ToLowerInvariant()+"-unsupported");
        }
        var closedKeys = new[]{Key.N,Key.Space,Key.Backspace,Key.I,Key.J,Key.K,Key.L,Key.E,Key.Enter,Key.Escape};
        foreach(var key in closedKeys) await PressBattleKey(key);
        for(int frame=0;frame<60;frame++) {
            await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame); _root._PhysicsProcess(1.0/60.0); callbacks++; idleCallbacks++;
            Require(ReferenceEquals(retained,_session.PrivateOriginalBattle01),"Idle time and closed gameplay keys do not mutate the visit");
        }
        Require(_session.EnterPrivateOriginalBattle01Exploration(retained) is PrivateOriginalBattle01ExplorationEntryRejected,"Entry cannot repeat");
        await Capture("idle-history-and-followers-retained");
        Require(nextFrame==72,"The existing pocket prefix retains71 captures before the doorway continuation");
        await Direction(Key.S,ExplorationDirection.South,new(31,14),true);
        await Direction(Key.D,ExplorationDirection.East,new(32,14),true);
        await Capture("door-before-copy");
        await Direction(Key.S,ExplorationDirection.South,new(32,15),true,detailed:true,phase:"door-copy");
        var firstDoor=_session.PrivateOriginalMapArrival!.DoorCopy;
        await Direction(Key.A,ExplorationDirection.West,new(32,15),false,"return.region");
        await Direction(Key.D,ExplorationDirection.East,new(32,15),false,"return.region");
        await Direction(Key.S,ExplorationDirection.South,new(32,16),true,detailed:true,phase:"outside-roof-restore");
        await Direction(Key.A,ExplorationDirection.West,new(32,16),false,"return.region");
        await Direction(Key.D,ExplorationDirection.East,new(32,16),false,"return.region");
        await Direction(Key.S,ExplorationDirection.South,new(32,16),false,"return.region");
        var outside=_session.PrivateOriginalBattle01!;
        foreach(var key in interactionKeys) {
            await PressBattleKey(key); Require(ReferenceEquals(outside,_session.PrivateOriginalBattle01),"Outside F/G remain unsupported");
            await Capture("outside-interaction-"+key.ToString().ToLowerInvariant()+"-unsupported");
        }
        await Direction(Key.W,ExplorationDirection.North,new(32,15),true,detailed:true,phase:"reentry-roof-clear");
        await Direction(Key.W,ExplorationDirection.North,new(32,14),true);
        for(int cycle=0;cycle<2;cycle++) {
            await Direction(Key.S,ExplorationDirection.South,new(32,15),true);
            await Direction(Key.S,ExplorationDirection.South,new(32,16),true);
            await Direction(Key.W,ExplorationDirection.North,new(32,15),true);
            await Direction(Key.W,ExplorationDirection.North,new(32,14),true);
            Require(ReferenceEquals(firstDoor,_session.PrivateOriginalMapArrival!.DoorCopy),"Repeated cycles retain the one door-copy receipt");
        }
        await Direction(Key.W,ExplorationDirection.North,new(32,14),false,"return.followers");
        var final=_session.PrivateOriginalBattle01!;
        foreach(var key in closedKeys) await PressBattleKey(key);
        for(int frame=0;frame<60;frame++) {
            await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame); _root._PhysicsProcess(1.0/60.0); callbacks++; idleCallbacks++;
            Require(ReferenceEquals(final,_session.PrivateOriginalBattle01),"Idle/closed keys cannot trigger another roof action or reset the cycle");
        }
        Require(_session.EnterPrivateOriginalBattle01Exploration(final) is PrivateOriginalBattle01ExplorationEntryRejected,"Post-cycle entry cannot repeat");
        await Capture("doorway-cycle-idle-history-retained");
        _root.SetPhysicsProcess(true);
        return new {status="Pass",operations,actualPhysicsCallbacks=callbacks,idlePhysicsCallbacks=idleCallbacks,
            unsupportedInteractionKeys=interactionKeys.Select(k=>k.ToString()).ToArray(),
            closedGameplayKeys=closedKeys.Select(k=>k.ToString()).ToArray(),entry=entered.Arrival,
            current=_session.PrivateOriginalMapArrival,followersExecuted=false,originalScriptsExecuted=false,
            controlledDoorCopy=true,controlledRoofSettlement=true};
    }

    private async Task<Dictionary<string,byte[]>> CaptureReturnMovement(string name)
    {
        var arrival=_session.PrivateOriginalMapArrival!;var view=_presenter.BaseProjection!;
        var viewport=Field<PrivateOriginalMapBaseViewport>(_presenter,"_baseViewport");
        var status=Field<Label>(_presenter,"_status");var glyphs=viewport.ArrivalActorGlyphs!;
        Require(!Field<PrivateBattle01Presenter>(_root,"_privateBattle01Presenter").Visible && viewport.Visible &&
            view.Map==new MapId("map3") && view.CurrentAreaOverlay && view.OverlayAreaRecordOrdinal==1 &&
            ReferenceEquals(_presenter.PlayerLocomotion,arrival.Locomotion),"Real current Arrival owns camera, player and opened-roof view");
        Require(glyphs.Single(g=>g.LogicalActorId==1).Position==new MapPosition(32,13) &&
            glyphs.Single(g=>g.LogicalActorId==2).Position==new MapPosition(32,13) &&
            glyphs.Single(g=>g.LogicalActorId==2).Kind==PrivateMap3LiveRouteActorGlyphKind.DeadFollowerDiamond,
            "Both frozen followers keep the same semantic position and Chester's dead marker");
        Require(status.Text.Contains($"Player 0 Bowie: ({arrival.PlayerPosition.X},{arrival.PlayerPosition.Y})") &&
            status.Text.Contains("Follower 1 Sarah: (32,13)/UP") && status.Text.Contains("DEAD / BLUE_FLAME") &&
            status.Text.Contains("original post-script positions Unknown") &&
            status.Text.Contains(arrival.RoofLifecycle is MapBlockCopyLifecycleActiveState ? "roof open" : "roof restored") &&
            status.Text.Contains(arrival.DoorCopy is null ? "Door closed" : "Door open"),
            "Status separates the live door/roof, current player, frozen declarations and Unknowns");
        await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
        using var image=GetViewport().GetTexture().GetImage();
        Require(image.SavePng(Path.Combine(_output,name+".png"))==Error.Ok,"Return movement PNG");
        int viewportClipPixels = CheckMapViewportClipping(image);
        int partialGlyphPixels = 0;
        if (name == "76-granseal-door-copy-middle")
        {
            var partial = glyphs.Single(g => g.LogicalActorId == 141);
            Require(partial.Position == new MapPosition(32, 11) &&
                partial.DestinationRect == new Rect2(149, -7, 14, 14) && arrival.Locomotion.Tick == 7,
                "The real top-edge diagnostic retains its semantic position and unclamped rectangle");
            for (int y = 0; y < 7; y++)
            for (int x = 149; x < 163; x++)
                if (image.GetPixel((int)((viewport.GlobalPosition.X + x) * image.GetWidth() / 960),
                    (int)((viewport.GlobalPosition.Y + y) * image.GetHeight() / 540)).IsEqualApprox(new Color("ff70a6")))
                    partialGlyphPixels++;
            Require(partialGlyphPixels > 0, "The clipped glyph's in-map pink interior remains actually visible");
        }
        Require(status.GetLineCount()==status.GetVisibleLineCount() && status.Position.Y+status.GetMinimumSize().Y<=540 &&
            status.Position.X+status.GetMinimumSize().X<=960,"Return movement status fits the logical canvas");
        var playerRect=PrivateOriginalMapBaseViewport.PlayerLocomotionRect(view,arrival.Locomotion);
        int samples=0;
        for(int row=0;row<5;row++) for(int column=4;column<9;column++) {
            int x=column*24+2,y=row*24+2;var point=new Vector2(x,y);
            if(playerRect.HasPoint(point) || glyphs.Any(g=>g.DestinationRect.HasPoint(point)))continue;
            int at=((y*view.RasterScale)*view.RasterPixelWidth+x*view.RasterScale)*4;
            var expected=new Color(view.RgbaBytes[at]/255f,view.RgbaBytes[at+1]/255f,view.RgbaBytes[at+2]/255f,view.RgbaBytes[at+3]/255f);
            var actual=image.GetPixel((int)((viewport.GlobalPosition.X+x)*image.GetWidth()/960),(int)((viewport.GlobalPosition.Y+y)*image.GetHeight()/540));
            Require(actual.IsEqualApprox(expected),"Visible church texel matches the current moving-camera projection");samples++;
        }
        Require(samples>0,"Church background remains visibly sampled");
        var worldPixels=new Dictionary<string,byte[]>(); var worldSampleCounts=new Dictionary<string,int>();
        foreach(var (label,worldX,worldY) in new[]{("door",32,15),("roof",30,14)}) {
            var rgba=new List<byte>(); int visible=0;
            for(int dy=0;dy<24;dy++) for(int dx=0;dx<24;dx++) {
                int x=worldX*24+dx-view.Camera!.TopLeftPixelX,y=worldY*24+dy-view.Camera.TopLeftPixelY;
                Require(x>=0 && y>=0 && x<288 && y<168,"Named doorway/roof world tile remains in the viewport");
                int at=((y*view.RasterScale)*view.RasterPixelWidth+x*view.RasterScale)*4;
                rgba.AddRange(view.RgbaBytes.Skip(at).Take(4));
                var point=new Vector2(x,y);
                if(playerRect.HasPoint(point) || glyphs.Any(g=>g.DestinationRect.HasPoint(point)))continue;
                var expected=new Color(view.RgbaBytes[at]/255f,view.RgbaBytes[at+1]/255f,view.RgbaBytes[at+2]/255f,view.RgbaBytes[at+3]/255f);
                var actual=image.GetPixel((int)((viewport.GlobalPosition.X+x)*image.GetWidth()/960),(int)((viewport.GlobalPosition.Y+y)*image.GetHeight()/540));
                Require(actual.IsEqualApprox(expected),"Named door/roof world pixel matches actual native rendering"); visible++;
            }
            var tileStart=new Vector2(worldX*24-view.Camera!.TopLeftPixelX,worldY*24-view.Camera.TopLeftPixelY);
            bool doorCoveredByPlayer=label=="door" && playerRect.HasPoint(tileStart) &&
                playerRect.HasPoint(tileStart+new Vector2(23,23));
            Require(visible>0 || doorCoveredByPlayer,
                "Require uncovered samples unless the complete door tile is inside the current player rectangle");
            worldPixels.Add(label,rgba.ToArray()); worldSampleCounts.Add(label,visible);
        }
        _frames.Add(new {name,map=arrival.Map.Value,phase="ReturnMovement",status=status.Text,
            position=arrival.PlayerPosition,facing=arrival.PlayerOpaqueFacing,arrival.InputOrdinal,arrival.Locomotion,
            glyphs,camera=view.Camera,playerRect,samples,worldSampleCounts,viewportClipPixels,partialGlyphPixels,
            doorInput=arrival.DoorCopy?.Input.Ordinal,roofInput=arrival.LastRoofAction?.InputOrdinal,
            roofAction=arrival.LastRoofAction?.Action.Outcome,width=image.GetWidth(),height=image.GetHeight()});
        return worldPixels;
    }

    private int CheckMapViewportClipping(Image image)
    {
        var viewport = Field<PrivateOriginalMapBaseViewport>(_presenter, "_baseViewport");
        if (viewport is null || !viewport.IsVisibleInTree() || _presenter.BaseProjection is null) return 0;
        Require(viewport.GetParent() is Control { ClipContents: true, MouseFilter: Control.MouseFilterEnum.Ignore } clip &&
            clip.GetGlobalRect() == new Rect2(viewport.GlobalPosition, PrivateOriginalMapBaseViewport.LogicalTextureRect.Size) &&
            viewport.GlobalPosition == new Vector2(24, 105), "The clipping host preserves the existing global map rectangle");
        var renderedClip = RenderingServer.DebugCanvasItemGetRect(((Control)viewport.GetParent()).GetCanvasItem());
        Require(renderedClip == PrivateOriginalMapBaseViewport.LogicalTextureRect,
            $"The clipping host's render rectangle matches its control size: {renderedClip}");
        var rectangle = new Rect2(viewport.GlobalPosition, PrivateOriginalMapBaseViewport.LogicalTextureRect.Size);
        int scaleX = image.GetWidth() / 960, scaleY = image.GetHeight() / 540;
        Require(scaleX > 0 && scaleY > 0 && image.GetWidth() == 960 * scaleX && image.GetHeight() == 540 * scaleY,
            "Native clipping samples use exact logical-to-rendered pixel scales");
        int left = (int)rectangle.Position.X * scaleX, top = (int)rectangle.Position.Y * scaleY;
        int right = (int)rectangle.End.X * scaleX, bottom = (int)rectangle.End.Y * scaleY;
        Color background = image.GetPixel(0, 0);
        int samples = 0;
        for (int y = top - 12 * scaleY; y < bottom + 12 * scaleY; y++)
        for (int x = left - 12 * scaleX; x < right + 12 * scaleX; x++)
        {
            if (x >= left && x < right && y >= top && y < bottom) continue;
            Require(image.GetPixel(x, y).IsEqualApprox(background),
                $"Map drawing is clipped on every edge; outside pixel ({x},{y}) retains the canvas background");
            samples++;
        }
        return samples;
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

    private async Task CaptureArrival(string name)
    {
        var arrival=_session.PrivateOriginalMapArrival!; var view=_presenter.BaseProjection!;
        var viewport=Field<PrivateOriginalMapBaseViewport>(_presenter,"_baseViewport");
        var battle=Field<PrivateBattle01Presenter>(_root,"_privateBattle01Presenter");
        var status=Field<Label>(_presenter,"_status");
        Require(!battle.Visible && viewport.Visible && _session.PrivateOriginalFlowStage==GameFlowStage.Exploration &&
            view.Map==new MapId("map3") && _presenter.BaseAtlasAssetId=="world.map3.base-tileset-atlas" &&
            view.CurrentAreaOverlay && view.OverlayAreaRecordOrdinal==1 && view.OriginX==26 && view.OriginY==10 &&
            ReferenceEquals(_presenter.PlayerLocomotion,arrival.Locomotion) && !arrival.Locomotion.IsMoving,
            "Fresh Map3 atlas/area1/idle camera owns the visible arrival; battle and old view are hidden");
        Require(arrival.PlayerPosition==new MapPosition(32,13) && arrival.PlayerOpaqueFacing==1 &&
            arrival.Party.Gold==60 && arrival.Party.Slots.Count==30 && arrival.Party.ProcessedIds.SequenceEqual(new byte[]{0,1,7,28}) &&
            arrival.Entity142Hidden && arrival.Entity142MovedOut && arrival.ExplorationInputAvailable,
            "Current party, default-init effects and bounded movement availability");
        foreach(int id in new[]{0,1,2}) {
            var e=arrival.Entities.Single(e=>e.Id==id);
            Require(e.Position==arrival.PlayerPosition && e.TargetPosition==arrival.PlayerPosition &&
                e.DeclarationPosition==arrival.PlayerPosition && e.Facing==1 && e.VelocityXUnits==0 && e.TravelYUnits==0,
                "Each player/F66 follower identity retains its same-cell current/target declaration and zero travel");
        }
        var glyphs=viewport.ArrivalActorGlyphs!;var sarah=glyphs.Single(e=>e.LogicalActorId==1);var chester=glyphs.Single(e=>e.LogicalActorId==2);
        Require(sarah.Position==chester.Position && sarah.DestinationRect==chester.DestinationRect &&
            chester.Kind==PrivateMap3LiveRouteActorGlyphKind.DeadFollowerDiamond &&
            status.Text.Contains("Player 0 Bowie: (32,13)/UP") && status.Text.Contains("Follower 1 Sarah: (32,13)/UP") &&
            status.Text.Contains("Follower 2 Chester: (32,13)/UP") && status.Text.Contains("DEAD / BLUE_FLAME") &&
            status.Text.Contains(arrival.EntityPolicy),"Panel exposes all overlapping identities without moving their glyphs");
        var roof=(MapBlockCopyLifecycleActiveState)arrival.RoofClear.LifecycleState;
        Require(roof.RecordOrdinal==8 && roof.SavedWords.Count==30,"Church roof clear and before-image");
        for(int y=41;y<47;y++) for(int x=30;x<35;x++) Require(arrival.WorkingLayout[x,y]==0,"All30 church words cleared");
        await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
        using var image=GetViewport().GetTexture().GetImage();
        Require(image.SavePng(Path.Combine(_output,name+".png"))==Error.Ok,"Arrival PNG");
        CheckMapViewportClipping(image);
        Require(status.GetLineCount()==status.GetVisibleLineCount() && status.Position.Y+status.GetMinimumSize().Y<=540 &&
            status.Position.X+status.GetMinimumSize().X<=960,
            $"All arrival text fits the logical canvas: lines={status.GetLineCount()}/{status.GetVisibleLineCount()}, minimum={status.GetMinimumSize()}, size={status.Size}");
        int samples=0;
        for(int row=0;row<5;row++) for(int column=4;column<9;column++) {
            if(column==6 && row==3)continue;
            int x=column*24+2,y=row*24+2;int at=((y*view.RasterScale)*view.RasterPixelWidth+x*view.RasterScale)*4;
            var expected=new Color(view.RgbaBytes[at]/255f,view.RgbaBytes[at+1]/255f,view.RgbaBytes[at+2]/255f,view.RgbaBytes[at+3]/255f);
            var actual=image.GetPixel((int)((viewport.GlobalPosition.X+x)*image.GetWidth()/960),(int)((viewport.GlobalPosition.Y+y)*image.GetHeight()/540));
            Require(actual.IsEqualApprox(expected),"Visible church texel equals current typed arrival projection");samples++;
        }
        _frames.Add(new {name,map=arrival.Map.Value,phase="EntryReady",status=status.Text,entities=arrival.Entities,
            glyphs,camera=view.Camera,roofRecord=roof.RecordOrdinal,roofWords=roof.SavedWords.Count,
            samples,width=image.GetWidth(),height=image.GetHeight(),policy=arrival.EntityPolicy});
    }

    private async Task CaptureControl(string name, string provenance="physical Godot key events from controlled Map40 seed")
    {
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        using Image image = GetViewport().GetTexture().GetImage();
        CheckMapViewportClipping(image);
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
                provenance, pursuitCompleted=view.PursuitCompleted, physicalAttackCompleted=view.PhysicalAttackCompleted, attackResult=view.AttackResult,
                allyStatus=view.AllyStatus });
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
        CheckMapViewportClipping(image);
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
            status.Position.Y >= baseViewport.GlobalPosition.Y + PrivateOriginalMapBaseViewProjection.PixelHeight &&
            status.Text.StartsWith("Map 40 controlled arrival. Base atlas; init not executed.", StringComparison.Ordinal),
            "Map40 atlas status below the base viewport");
        VerifyMap40BucketPixels(snapshot, movement);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        using Image image = GetViewport().GetTexture().GetImage();
        CheckMapViewportClipping(image);
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
            status.Position.Y >= baseViewport.GlobalPosition.Y + PrivateOriginalMapBaseViewProjection.PixelHeight &&
            pendingText.StartsWith("Battle 01 admission pending. Battle not started.", StringComparison.Ordinal) &&
            pendingText.Contains("destination Map 57 (8,18)/UP.", StringComparison.Ordinal) &&
            pendingText.Contains("Map 40 retained", StringComparison.Ordinal) &&
            pendingText.Contains("Restart", StringComparison.Ordinal) && !pendingText.Contains("ENTER", StringComparison.Ordinal),
            "Pending display and restart recovery below retained Map40");
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        using Image image = GetViewport().GetTexture().GetImage();
        CheckMapViewportClipping(image);
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
        Require(label.Position.Y >= baseViewport.GlobalPosition.Y + PrivateOriginalMapBaseViewProjection.PixelHeight &&
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
        CheckMapViewportClipping(image);
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
