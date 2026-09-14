using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.TestSupport;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;
using static Sf2.Remake.Engine.Tests.PrivateExplorationTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class PrivateBattleEntryProgramTests
{
    private const string World = "SF2_PRIVATE_EXPLORATION_CONTENT";
    private static JsonDocument Fixture(string name) => JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", name + ".json")));
    private static ExplorationDirection Direction(string name) => name switch
    { "Up" => ExplorationDirection.North, "Down" => ExplorationDirection.South, "Left" => ExplorationDirection.West,
        "Right" => ExplorationDirection.East, _ => throw new InvalidOperationException(name) };
    private static void Settle(GameSession session, List<SessionObservation> observations)
    {
        if (session.Current.StopReason != SessionStopReason.PlayerInput)
        {
            var result = RunUntilStop(session, 20000, observations);
            Assert.True(result.Failure is null, result.Failure?.ToString());
        }
    }
    private static void Input(GameSession session, SessionCommand command, List<SessionObservation> observations)
    { observations.AddRange(Accept(session, command).Observations); Settle(session, observations); }

    [PrivateInputFact(World)]
    public void ContinuousOpeningCastleTowerAndBeforeBattleRetainTheLivePartyAndReachActualBattleInput()
        => ContinuousBattle([]);

    internal static GameSession ContinuousBattle(List<SessionObservation> observations)
    {
        var session = RunOpening(true, observations, []);
        var identity = session.Current.SessionId;
        var party = session.Current.Exploration!.Party;
        using var castle = Fixture("castle-tower");
        foreach (var segment in castle.RootElement.GetProperty("static").GetProperty("routeGraph").GetProperty("segments").EnumerateArray())
        {
            var kind = segment.GetProperty("kind").GetString();
            if (kind == "navigation")
                foreach (var input in segment.GetProperty("inputs").EnumerateArray()) Input(session, new Move(Direction(input.GetString()!)), observations);
            if (kind == "entity-interaction")
            {
                Input(session, new Move(ExplorationDirection.North), observations);
                Input(session, new Interact(new("entity-140")), observations);
            }
            if (kind == "entity-terminal")
            {
                foreach (var direction in new[] { ExplorationDirection.East, ExplorationDirection.East })
                    Input(session, new Move(direction), observations);
                Input(session, new Interact(new("entity-128")), observations);
                Input(session, new Move(ExplorationDirection.East), observations);
                Input(session, new Move(ExplorationDirection.East), observations);
                Input(session, new Move(ExplorationDirection.North), observations);
            }
            Assert.Equal(identity, session.Current.SessionId);
        }
        Assert.Contains(401, session.Current.Story.Flags);
        Assert.DoesNotContain(451, session.Current.Story.Flags);
        Assert.Equal(new MapPosition(5, 15), session.Current.Exploration!.PlayerEntity.Position);
        var liveFlags = session.Current.Story.Flags;
        var liveMembers = MapPartyMembership.Rebuild(liveFlags, session.Definition.Exploration!.PartyFlags!).Active;
        Traverse(session, observations);
        Assert.Equal(identity, session.Current.SessionId);
        Assert.True(session.Current.HasBattleControl);
        Assert.Equal(liveMembers.Select(member => "ally-" + member), session.Current.Battle.Actors.Where(actor => actor.IsAlly && actor.Position is not null).Select(actor => actor.Actor.Value));
        Assert.Equal(party.Gold, session.Current.Battle.Gold);
        foreach (var input in party.Actors.Where(input => input.Actor.Value.StartsWith("ally-", StringComparison.Ordinal)))
        {
            var actor = session.Current.Battle.GetActor(input.Actor);
            Assert.Equal(input.Exp, actor.Exp); Assert.Equal(input.Kills, actor.Kills); Assert.Equal(input.Defeats, actor.Defeats);
            Assert.Equal(actor.Definition.MaxHp, actor.Hp); Assert.Equal(actor.Definition.MaxMp, actor.Mp);
        }
        Assert.Contains(observations, row => row.Program?.Program == "bbcs-01");
        Assert.Contains(observations, row => row.Kind == "battle-loaded");
        Assert.Equal(1, session.Current.Battle.Round);
        FirstInput(session);
        return session;
    }

    [PrivateInputFact(World)]
    public void ExplicitH3BridgeRunsTheFullBeforeBodyAndMatchesPlayerReadyWithoutClaimingR1Continuity()
    {
        var session = Bridge();
        List<SessionObservation> observations = [];
        Traverse(session, observations);
        using var fixture = Fixture("player-ready");
        var record = fixture.RootElement.GetProperty("expectedObservation").GetProperty("records")[0];
        Assert.False(record.GetProperty("continuity").GetProperty("naturalR2bContinuity").GetBoolean());
        var ready = record.GetProperty("deterministicState").GetProperty("ready");
        Assert.Equal(ready.GetProperty("randomSeed").GetUInt32(), session.Current.Battle.MainSeed);
        Assert.Equal(ready.GetProperty("randomSeedCopy").GetUInt16(), session.Current.Battle.ThinkingSeed >> 16);
        Assert.Equal("ally-1", session.Current.Selection!.Actor.Value);
        Assert.Equal(new[] { "ally-1", "ally-2", "enemy-0", "enemy-3", "enemy-5", "enemy-1", "enemy-2", "enemy-4", "ally-0" },
            session.Current.Battle.TurnOrder.Where(entry => entry.Actor is not null).Select(entry => entry.Actor!.Value.Value));
        Assert.All(session.Current.Battle.Regions!.Flags, flag => Assert.False(flag));
        Assert.Contains(451, session.Current.Story.Flags);
        Assert.Contains(399, session.Current.Story.Flags);
        Assert.Contains(observations, row => row.Program?.Program == "bbcs-01");
        var beforeProgram = session.Definition.Exploration!.Programs["bbcs-01"];
        Assert.Equal(Enumerable.Range(0, beforeProgram.Instructions.Count), observations.Where(row => row.Program?.Program == "bbcs-01")
            .Select(row => row.Program!.Value.Instruction).Distinct().Order());
        using var admission = Fixture("battle01-admission");
        var sourceCommands = admission.RootElement.GetProperty("static").GetProperty("cutscenes").GetProperty("beforeBattle")
            .GetProperty("program").GetProperty("commands");
        Assert.Equal(sourceCommands.EnumerateArray().Count(row => row.GetProperty("macro").GetString() == "shiver"),
            observations.Count(row => row.Kind == "presentation-completed" && row.Detail == "Gesture"));
        FirstInput(session);
    }

    internal static GameSession Bridge(bool seen = false, bool sarah = true)
    {
        var source = new PrivateExplorationReader(PrivateInputFactAttribute.RequireInput(World),
            Path.Combine(AppContext.BaseDirectory, "controlled", "map40-intro-start.json"),
            PrivateBattleScenarioTests.Selected(Path.Combine(AppContext.BaseDirectory, "controlled", "battle01-player-ready.json")));
        var admitted = Assert.IsType<ExplorationReadAccepted>(source.Read());
        // Explicit H3 control: selected seed and roster, separate from the continuous test above.
        int[] flags = [0, 2, 32, 34, 401, .. sarah ? new[] { 1, 33 } : [], .. seen ? new[] { 451 } : []];
        var start = new ExplorationStartInput(new("map-21"), admitted.Start.Player, new(5, 15), 3, 32, flags, admitted.Start.Party);
        return Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, start)).Session;
    }

    [PrivateInputFact(World)]
    public void LiveActiveMembershipPacksFormationWithoutAddingSarahOrReplacingThePartyResources()
    {
        var session = Bridge(seen: true, sarah: false);
        var source = session.Current.Exploration!.Party;
        var encounter = session.Definition.Encounters[source.Encounter];
        var inputs = source.Actors.Select(input => input.Actor.Value switch
        {
            "ally-0" => input with { Hp = 1, Mp = 0, Exp = 13, Kills = 2, Defeats = 1, Status = 1 },
            "ally-1" => input with { Hp = 0, Mp = 0, Exp = 17, Kills = 3, Defeats = 2 },
            "ally-2" => input with { Hp = 1, Exp = 19, Kills = 5, Defeats = 4 },
            _ => input,
        }).ToArray();
        var initialized = BattleTurnFlow.Start(encounter, new BattleStartInput(source.Encounter, inputs, 0x87654321, 0x12340000, 789,
            source.NewBattle, [new("ally-0"), new("ally-2")]));
        var leader = initialized.GetActor(new("ally-0"));
        var sarah = initialized.GetActor(new("ally-1"));
        var chester = initialized.GetActor(new("ally-2"));
        Assert.Equal(leader.Definition.MaxHp, leader.Hp); Assert.Equal(leader.Definition.MaxMp, leader.Mp);
        Assert.Equal(1, leader.Status); Assert.Equal((byte)13, leader.Exp); Assert.Equal((ushort)2, leader.Kills); Assert.Equal((ushort)1, leader.Defeats);
        Assert.Equal(0, sarah.Hp); Assert.Equal(0, sarah.Mp); Assert.Null(sarah.Position); Assert.Equal((byte)17, sarah.Exp);
        Assert.Equal(encounter.Initialization!.AllyFormation![1], chester.Position);
        Assert.Equal(chester.Definition.MaxHp, chester.Hp);
        Assert.Equal(789u, initialized.Gold); Assert.Equal(0x87654321u, initialized.MainSeed); Assert.Equal(0x12340000u, initialized.ThinkingSeed);
        Assert.All(initialized.Actors, actor => Assert.Same(encounter.Deployments.Single(row => row.Actor == actor.Actor).Definition, actor.Definition));
        var withDeadActiveMember = BattleTurnFlow.Start(encounter, new BattleStartInput(source.Encounter, inputs, source.MainSeed, source.ThinkingSeed,
            source.Gold, source.NewBattle, [new("ally-0"), new("ally-1"), new("ally-2")]));
        Assert.Null(withDeadActiveMember.GetActor(new("ally-1")).Position);
        Assert.Equal(encounter.Initialization.AllyFormation[1], withDeadActiveMember.GetActor(new("ally-2")).Position);
        var standalone = BattleTurnFlow.Start(encounter, new BattleStartInput(source.Encounter, inputs, source.MainSeed, source.ThinkingSeed,
            source.Gold, source.NewBattle));
        Assert.Null(standalone.GetActor(new("ally-1")).Position);
        Assert.Equal(chester.Position, standalone.GetActor(new("ally-2")).Position);
        List<SessionObservation> observations = [];
        Traverse(session, observations);
        Assert.DoesNotContain(observations, row => row.Program?.Program == "bbcs-01");
        Assert.Null(session.Current.Battle.GetActor(new("ally-1")).Position);
        Assert.Equal(encounter.Initialization.AllyFormation[1], session.Current.Battle.GetActor(new("ally-2")).Position);
        Assert.NotEqual("ally-1", session.Current.Selection!.Actor.Value);
    }

    internal static void Traverse(GameSession session, List<SessionObservation> observations)
    {
        using var fixture = Fixture("player-ready");
        foreach (var row in fixture.RootElement.GetProperty("static").GetProperty("inputPlan").EnumerateArray())
        {
            var from = row.GetProperty("from");
            Assert.Equal("map-" + from.GetProperty("map").GetInt32(), session.Current.Exploration!.Map.Value);
            Assert.Equal(new MapPosition(from.GetProperty("x").GetInt32(), from.GetProperty("y").GetInt32()), session.Current.Exploration.PlayerEntity.Position);
            Input(session, new Move(Direction(row.GetProperty("input").GetString()!)), observations);
        }
    }

    [PrivateInputFact(World)]
    public void SuspendedBattleSelectionStopsBeforeTheNewBattleBodyOrEntityReplacement()
    {
        var bridge = Bridge();
        var session = Assert.IsType<SessionStarted>(GameSession.Start(bridge.Definition,
            new ExplorationStartInput(new("map-40"), new("entity-0"), new(14, 13), 1, 32,
                [0, 1, 2, 32, 33, 34, 88, 401], bridge.Current.Exploration!.Party))).Session;
        var world = session.Current.Exploration;
        var flags = session.Current.Story.Flags;
        var result = Send(session, new Move(ExplorationDirection.North));
        Assert.Equal("suspended-battle-entry", result.Failure!.Code);
        Assert.Equal(SessionFailureKind.UnsupportedCapability, result.Failure.Kind);
        Assert.Same(world, session.Current.Exploration);
        Assert.Equal(flags, session.Current.Story.Flags);
        Assert.DoesNotContain(result.Observations, row => row.Program?.Program == "bbcs-01");
    }
    private static void FirstInput(GameSession session)
    {
        Assert.Equal(BattleSelectionStage.Movement, session.Current.Selection!.Stage);
        var battle = session.Current.Battle;
        Accept(session, new Confirm()); Assert.Equal(BattleSelectionStage.ActionChoice, session.Current.Selection.Stage);
        Accept(session, new Cancel()); Assert.Equal(BattleSelectionStage.Movement, session.Current.Selection.Stage);
        Assert.Same(battle, session.Current.Battle);
    }
}
