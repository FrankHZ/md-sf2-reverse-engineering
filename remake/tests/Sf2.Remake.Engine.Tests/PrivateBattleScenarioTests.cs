using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.TestSupport;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class PrivateBattleScenarioTests
{
    private const string Data = "SF2_PRIVATE_BATTLE01_DATA", Scene = "SF2_PRIVATE_BATTLE01_SCENE", Terrain = "SF2_PRIVATE_BATTLE01_TERRAIN",
        Static = "SF2_PRIVATE_STATIC_DATA", Enemy = "SF2_PRIVATE_ENEMY_DATA", Gold = "SF2_PRIVATE_ENEMY_GOLD", StartInput = "SF2_PRIVATE_CONTROLLED_START";
    private static string Input(string name) => PrivateInputFactAttribute.RequireInput(name);
    internal static PrivateBattleScenarioReader Selected(string? controlled = null, string? definitions = null) =>
        new(Input(Data), Input(Scene), Input(Terrain), definitions ?? Input(Static), Input(Enemy), Input(Gold), controlled ?? Input(StartInput));
    private static ActorRef Actor(int index) => new(index < 128 ? "ally-" + index : "enemy-" + (index - 128));

    [PrivateInputFact(Data, Scene, Terrain, Static, Enemy, Gold, StartInput)]
    public void ActualPinnedInputsInitializeTheCommonSessionAndMatchThePlayerReadyBoundary()
    {
        var admitted = Assert.IsType<ScenarioReadAccepted>(Selected().Read());
        var source = admitted.Definition.PrivateDefinitions!;
        Assert.Equal("private-local-controlled-start", admitted.Origin);
        Assert.Equal(new byte[] { 0, 1, 4 }, source.Classes.Keys.Order());
        Assert.Equal(new byte[] { 0, 56, 71, 85, 127 }, source.Items.Keys.Order());
        Assert.Equal("CENTAUR", source.Classes[1].MovementType);
        Assert.Equal((byte)7, source.Enemies["GIZMO"].BaseAttack);
        Assert.Equal("PRIORITYMOD_2", source.Enemies["GIZMO"].AiBitfieldExpression);
        Assert.Equal((byte)39, source.Enemies["GIZMO"].UnknownByte);
        Assert.Equal("WIND_WEAKNESS|ICE_MAJOR|FIRE_WEAKNESS|STATUS_MINOR", source.Enemies["GIZMO"].ResistanceExpression);
        Assert.All(source.Sources, row => Assert.Equal("c834c652b6862bc5679fd7f69a38a7093206efc6", row.Commit));
        Assert.Contains(source.Items[71].EquipEffects, effect => effect.Type == "INCREASE_ATT" && effect.Parameter == 3);
        Assert.Equal((byte)8, source.Spells[new("egress", 1)].MpCost);
        Assert.Equal((byte)3, source.Spells[new("heal", 1)].MpCost);
        var started = Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, admitted.Start));
        var session = started.Session; var battle = session.Current.Battle;
        Assert.Same(admitted.Definition, session.Definition);
        Assert.Equal(new[] { "new-battle-initialized", "regions-tested", "region-program-none", "spawn-modes-admitted", "round-started", "round-rng", "player-control" },
            started.Result.Observations.Select(row => row.Kind));
        using var fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", "player-ready.json")));
        var record = fixture.RootElement.GetProperty("expectedObservation").GetProperty("records")[0];
        Assert.False(record.GetProperty("continuity").GetProperty("naturalR2bContinuity").GetBoolean());
        var ready = record.GetProperty("deterministicState").GetProperty("ready");
        Assert.Equal(ready.GetProperty("randomSeed").GetUInt32(), battle.MainSeed);
        Assert.Equal(ready.GetProperty("randomSeedCopy").GetUInt16(), battle.ThinkingSeed >> 16);
        Assert.Equal(0u, battle.ThinkingSeed & 65535); // Explicit representation padding, not an observed original low word.
        Assert.Equal(1, battle.Round); Assert.Equal(0, battle.Cursor); Assert.Equal(new ActorRef("ally-1"), session.Current.Selection!.Actor);
        var expectedQueue = record.GetProperty("turnState").GetProperty("entries").EnumerateArray().ToArray();
        Assert.Equal(expectedQueue.Select(row => new TurnOrderEntry<ActorRef>(Actor(row.GetProperty("actor").GetInt32()), row.GetProperty("score").GetByte())), battle.Queue.Take(expectedQueue.Length));
        Assert.Equal(64, battle.Queue.Count); Assert.All(battle.Queue.Skip(expectedQueue.Length), row => Assert.Equal(new(null, 255), row));
        Assert.Equal(record.GetProperty("admission").GetProperty("regionFlags90Through105").EnumerateArray().Select(row => row.GetBoolean()), battle.Regions!.Flags);
        Assert.Equal((ushort)7, battle.Regions.Tested);
        foreach (var row in record.GetProperty("scenario").GetProperty("combatants").EnumerateArray())
        {
            int id = row.GetProperty("id").GetInt32(); var actor = battle.GetActor(Actor(id)); var definition = actor.Definition;
            Assert.Equal(row.GetProperty("hpCurrent").GetUInt16(), actor.Hp); Assert.Equal(row.GetProperty("hpMax").GetUInt16(), definition.MaxHp);
            Assert.Equal(row.GetProperty("mpCurrent").GetByte(), actor.Mp); Assert.Equal(row.GetProperty("mpMax").GetByte(), definition.MaxMp);
            Assert.Equal(row.GetProperty("attack").GetByte(), actor.Attack); Assert.Equal(row.GetProperty("defense").GetByte(), definition.Defense);
            Assert.Equal(row.GetProperty("agility").GetByte(), definition.Agility); Assert.Equal(row.GetProperty("move").GetByte(), definition.Move);
            Assert.Equal(row.GetProperty("level").GetByte(), definition.Level); Assert.Equal(row.GetProperty("statusEffects").GetUInt16(), actor.Status);
            Assert.Equal(new MapPosition(row.GetProperty("x").GetInt32(), row.GetProperty("y").GetInt32()), actor.Position);
            Assert.Equal(row.GetProperty("items").EnumerateArray().Select(value => value.GetUInt16()), definition.SourceLoadout!.Items);
            Assert.Equal(row.GetProperty("spells").EnumerateArray().Select(value => value.GetByte()), definition.SourceLoadout.Spells);
            if (id == 1 || id >= 128) Assert.Equal(row.GetProperty("activationBitfield").GetUInt16(), actor.ActivationWord);
            else Assert.Null(actor.ActivationWord); // Original observed zero does not authorize a universal start default.
            if (id < 128) Assert.Equal(row.GetProperty("class").GetByte(), definition.SourceClassId);
            else { Assert.Equal((byte)7, definition.Attack); Assert.Equal(BattleAiStrategy.SourceOrders, actor.AiStrategy); }
            Assert.Null(actor.Exp); Assert.Null(actor.Kills); Assert.Null(actor.Defeats); Assert.Null(actor.LastTarget); Assert.Equal((byte)0, actor.AiMemory);
        }
        Assert.Null(battle.Gold);
        Assert.Equal(new[] { BattleMover.Regular, BattleMover.Healer, BattleMover.Centaur }, battle.Actors.Take(3).Select(a => a.Definition.Mover));
        Assert.All(battle.Actors.Skip(3), a => Assert.Equal(BattleMover.Hovering, a.Definition.Mover));
        Assert.Equal(2304, source.Encounter.Terrain.Count);
        var original = session.Current.Battle; var selected = session.Current.Selection.Actor;
        var moved = Accept(session, new Move(ExplorationDirection.North));
        Assert.Equal(new MapPosition(9, 17), moved.Snapshot.Selection!.Preview.Destination); Assert.Same(original, moved.Snapshot.Battle);
        Accept(session, new Confirm()); var cancelled = Accept(session, new Cancel());
        Assert.Equal(new MapPosition(9, 18), cancelled.Snapshot.Selection!.Preview.Destination); Assert.Same(original, cancelled.Snapshot.Battle);
        Accept(session, new Confirm()); Accept(session, new SelectSpell(new("heal", 1))); Accept(session, new SelectTarget(selected));
        var before = session.Current; var rejected = Send(session, new Confirm());
        Assert.Equal("unspecified-exp", rejected.Failure!.Code); Assert.Same(before, session.Current);
        Accept(session, new Cancel());
        Stay(session); // Actual next entry is the Centaur ally; no per-character controller.
        Assert.Equal(new ActorRef("ally-2"), session.Current.Selection!.Actor);
        Accept(session, new Move(ExplorationDirection.North)); Accept(session, new Cancel());
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.Stay));
        var beforeEnemy = session.Current.Battle; var reached = Send(session, new Confirm());
        Assert.Null(reached.Failure); Assert.Equal(SessionStopReason.PlayerInput, reached.StopReason);
        Assert.Equal(8, reached.Snapshot.Battle.Cursor); Assert.Equal(new ActorRef("ally-0"), reached.Snapshot.Selection!.Actor);
        Assert.Equal(beforeEnemy.MainSeed, reached.Snapshot.Battle.MainSeed); Assert.Equal(0x01340000u, reached.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(new MapPosition(6, 3), reached.Snapshot.Battle.GetActor(new("enemy-0")).Position);
        Assert.Equal((byte)0x14, reached.Snapshot.Battle.GetActor(new("enemy-0")).AiMemory);
        Assert.DoesNotContain(reached.Observations, row => row.Kind == "ai-stay");
        Assert.Equal(6, reached.Observations.Count(row => row.Kind == "source-standby"));

    }

    [PrivateInputFact(Data, Scene, Terrain, Static, Enemy, Gold, StartInput)]
    public void SelectedPrivateFailuresAreTypedAndControlledValuesCannotImportActivatedState()
    {
        var folder = Path.Combine(Path.GetTempPath(), "sf2-private-entry-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        try
        {
            var original = JsonNode.Parse(File.ReadAllText(Input(StartInput)))!;
            var path = Path.Combine(folder, "controlled.json");
            foreach (var (change, code) in new (Action<JsonNode>, string)[]
            {
                (row => row["policy"]!["missingCandidateAllyWord"] = null, "player-control-missingactivationword"),
                (row => row["policy"]!["missingCandidateAllyWord"] = 4, "player-control-aicontrolled"),
                (row => row["policy"]!["difficulty"] = 1, "difficulty"),
                (row => row["policy"]!["allyAutoBattle"] = true, "control-mode"),
                (row => row["allies"]![0]!["activationWord"] = 0, "unknown-or-duplicate-field"),
                // Source initialization retains low three bits; broader refresh remains unsupported.
                (row => row["allies"]![0]!["status"] = 8, "new-battle-status-refresh"),
                (row => row["allies"]![0]!["items"]![0] = 213, "incompatible-equipment"),
            })
            {
                var changed = original.DeepClone(); change(changed); File.WriteAllText(path, changed.ToJsonString());
                var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(Selected(path)));
                Assert.Equal(code, failed.Failure.Code); Assert.DoesNotContain(folder, JsonSerializer.Serialize(failed));
            }
            File.WriteAllText(path, "not an export");
            var trust = Assert.IsType<ScenarioReadRejected>(Selected(definitions: path).Read());
            Assert.Equal("private-definition-input", trust.Failure.Code); Assert.DoesNotContain(folder, JsonSerializer.Serialize(trust));
            Assert.IsType<ScenarioReadRejected>(Selected(Path.Combine(folder, "absent.json")).Read());
        }
        finally { Directory.Delete(folder, true); }
    }

    [Fact]
    public void PartialPrivateSelectionFailsWithoutPathsAndThePublicDefaultRemainsAuthored()
    {
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(new PrivateBattleScenarioReader("", "", "", "", "", "", "")));
        Assert.Equal("private-input-selection", failed.Failure.Code);
        Assert.Equal("public-authored-controlled-start", Start().Definition.Origin);
    }
}
