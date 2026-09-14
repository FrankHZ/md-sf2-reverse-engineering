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

public sealed class PrivateBattleOutcomeProgramTests
{
    private const string World = "SF2_PRIVATE_EXPLORATION_CONTENT";
    internal static ExplorationReadAccepted Read()
    {
        var source = new PrivateExplorationReader(PrivateInputFactAttribute.RequireInput(World),
            Path.Combine(AppContext.BaseDirectory, "controlled", "map40-intro-start.json"),
            PrivateBattleScenarioTests.Selected(Path.Combine(AppContext.BaseDirectory, "controlled", "map3-opening-party.json")));
        var result = source.Read();
        Assert.True(result is ExplorationReadAccepted, result is ScenarioReadRejected failed ? failed.Failure.ToString() : "admission");
        return (ExplorationReadAccepted)result;
    }

    internal static GameSession StartBattle(List<SessionObservation> observations)
    {
        var read = Read();
        var input = read.Start.Party;
        var party = new BattleStartInput(input.Encounter, input.Actors, input.MainSeed, input.ThinkingSeed, 123, input.NewBattle);
        // External controlled comparison at the existing Map21 seam. Native observation separately
        // supplies the uninterrupted opening/castle session; this does not claim original continuity.
        var started = GameSession.Start(read.Definition, new ExplorationStartInput(new("map-21"), new("entity-0"), new(5, 15), 3, 32,
            [0, 1, 2, 32, 33, 34, 401, 600, 601, 602, 603, 604, 605, 607, 608], party));
        var session = Assert.IsType<SessionStarted>(started).Session;
        PrivateBattleEntryProgramTests.Traverse(session, observations);
        return session;
    }

    [PrivateInputFact(World)]
    public void ImportedGrowthMatchesTheAcceptedConnectedBattleExperienceBoundary()
    {
        var source = Read().Definition.Encounters.Values.Single().Deployments.Single(row => row.Actor.Value == "ally-0");
        using var fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", "battle-exp-level-up.json")));
        var input = fixture.RootElement.GetProperty("case").GetProperty("input");
        var expected = fixture.RootElement.GetProperty("expected");
        var profile = source.Definition.Growth! with { AttackBonus = 0 };
        var definition = source.Definition.WithGrowth(profile);
        var progress = new BattleActorProgress(input.GetProperty("level").GetByte(), input.GetProperty("maxHp").GetUInt16(),
            input.GetProperty("maxMp").GetByte(), input.GetProperty("baseAttack").GetByte(), input.GetProperty("baseDefense").GetByte(),
            input.GetProperty("baseAgility").GetByte(), source.Definition.Spells,
            new(input.GetProperty("items").EnumerateArray().Select(value => value.GetUInt16()), source.Definition.SourceLoadout!.Spells));
        var actor = new BattleActorState(source with { Definition = definition }, input.GetProperty("currentHp").GetUInt16(),
            input.GetProperty("currentMp").GetByte(), input.GetProperty("exp").GetByte(), source.Position, 0, 0,
            attack: input.GetProperty("battleAttack").GetByte(), progress: progress);
        uint seed = fixture.RootElement.GetProperty("case").GetProperty("levelUpSeed").GetUInt32() << 16;
        List<BattleEffect> effects = [];
        var result = BattleGrowthRules.Award(actor, expected.GetProperty("award").GetProperty("commandExp").GetInt32(), ref seed, effects);
        var after = expected.GetProperty("final");
        Assert.Equal(after.GetProperty("level").GetByte(), result.Level);
        Assert.Equal(after.GetProperty("maxHp").GetUInt16(), result.MaxHp);
        Assert.Equal(after.GetProperty("maxMp").GetByte(), result.MaxMp);
        Assert.Equal(after.GetProperty("currentHp").GetUInt16(), result.Hp);
        Assert.Equal(after.GetProperty("currentMp").GetByte(), result.Mp);
        Assert.Equal(after.GetProperty("currentAttack").GetByte(), result.Attack);
        Assert.Equal(after.GetProperty("baseDefense").GetByte(), result.Defense);
        Assert.Equal(after.GetProperty("baseAgility").GetByte(), result.Agility);
        Assert.Equal(after.GetProperty("exp").GetByte(), result.Exp);
        Assert.Equal(expected.GetProperty("levelUp").GetProperty("observedSeed").GetUInt32(), seed >> 16);
    }

    [PrivateInputFact(World)]
    public void CompletedFlagSkipsOnlyTheAfterProgramAndRetainsItsJoinTail()
    {
        var read = Read();
        var encounter = read.Definition.Encounters.Values.Single();
        var before = BattleTurnFlow.Start(encounter, read.Start.Party);
        var terminal = before.With(actors: before.Actors.Select(actor => actor.IsAlly ? actor : actor.With(hp: 0)));
        var route = read.Definition.Exploration!.Maps.Values.Single(map => map.Battle is not null).Battle!;
        var story = new StoryState([0, 1, 2, 32, 33, 34, 401, 501], enteringBattle: route);
        var snapshot = new SessionSnapshot(Guid.NewGuid(), 1, 1, new ActiveBattle(terminal, null), story, SessionStopReason.SimulationWait);
        var result = BattleOutcome.Begin(read.Definition, new(snapshot, [], snapshot.StopReason));
        Assert.Null(result.Failure);
        Assert.DoesNotContain(result.Observations, row => row.Program?.Program == "abcs-battle01");
        Assert.Contains(result.Observations, row => row.Kind == "after-battle-join" && row.Detail == "0");
        Assert.DoesNotContain(401, result.Snapshot.Story.Flags);
        Assert.Contains(501, result.Snapshot.Story.Flags);
        Assert.Equal(snapshot.SessionId, result.Snapshot.SessionId);
        Assert.IsType<PresentationWait>(result.Snapshot.Story.Wait);

        var leaderDead = terminal.With(actors: terminal.Actors.Select(actor => actor.Actor == encounter.Outcome!.Leader ? actor.With(hp: 0) : actor));
        Assert.Equal(BattleOutcomeKind.Defeat, BattleOutcomeRules.Check(leaderDead));
        var invalidDefeat = new SessionSnapshot(Guid.NewGuid(), 1, 1, new ActiveBattle(leaderDead, null), story, SessionStopReason.SimulationWait);
        var rejected = BattleOutcome.Begin(read.Definition, new(invalidDefeat, [], invalidDefeat.StopReason));
        Assert.Equal("egress-flags", rejected.Failure!.Code);
        Assert.Same(leaderDead, rejected.Snapshot.Battle);
        Assert.Same(story, rejected.Snapshot.Story);
    }

    [PrivateInputFact(World)]
    public void LivingReturnHealAndAllPartyResetKeepDistinctDeadAllyRules()
    {
        var read = Read();
        var input = read.Start.Party;
        var party = new BattleStartInput(input.Encounter, input.Actors.Select(actor => actor.Actor.Value == "ally-2" ? actor with { Hp = 0 } : actor),
            input.MainSeed, input.ThinkingSeed, input.Gold, input.NewBattle);
        Assert.Equal(0, BattleOutcome.Heal(read.Definition, party, all: false).Actors.Single(actor => actor.Actor.Value == "ally-2").Hp);
        Assert.Equal(11, BattleOutcome.Heal(read.Definition, party, all: true).Actors.Single(actor => actor.Actor.Value == "ally-2").Hp);
        Assert.Equal(0, party.Actors.Single(actor => actor.Actor.Value == "ally-2").Hp);
    }

    [PrivateInputFact(World)]
    public void ContinuousOpeningDefeatKeepsOneSessionThroughTheWholeProgramAndReturn()
    {
        List<SessionObservation> observations = [];
        var session = PrivateBattleEntryProgramTests.ContinuousBattle(observations);
        var identity = session.Current.SessionId;
        Play(session, observations, lose: true);
        Assert.Equal(identity, session.Current.SessionId);
        MoveAfterReturn(session, observations);
        Assert.Contains(observations, row => row.Kind == "battle-outcome" && row.Detail == "Defeat");
    }

    [PrivateInputFact(World)]
    public void ActualCommandsCompleteTheWholeVictoryProgramFlagsAndUsableReturn()
    {
        List<SessionObservation> observations = [];
        var session = PrivateBattleEntryProgramTests.ContinuousBattle(observations);
        var identity = session.Current.SessionId;
        Play(session, observations, lose: false);
        Assert.Equal(identity, session.Current.SessionId);
        Assert.Contains(observations, row => row.Kind == "level");
        Assert.True(observations.Any(observation => observation.Kind == "battle-outcome" && observation.Detail == "Victory"),
            "The actual battle must reach victory.");
        Assert.DoesNotContain(401, session.Current.Story.Flags); Assert.Contains(501, session.Current.Story.Flags);
        var program = session.Definition.Exploration!.Programs["abcs-battle01"];
        Assert.Equal(Enumerable.Range(0, program.Instructions.Count), observations.Where(row => row.Program?.Program == program.Id)
            .Select(row => row.Program!.Value.Instruction).Distinct().Order());
        Assert.True(observations.FindIndex(row => row.Kind == "after-battle-join") < observations.FindIndex(row => row.Kind == "battle-unlock-cleared"));
        Assert.True(observations.FindIndex(row => row.Kind == "battle-unlock-cleared") < observations.FindIndex(row => row.Kind == "battle-completed-set"));
        Assert.Equal("map-57", session.Current.Exploration!.Map.Value);
        Assert.Equal(session.Current.Story.OutcomeReturn!.Position, session.Current.Exploration.PlayerEntity.Position);
        Assert.Equal(3, session.Current.Exploration.PlayerEntity.Motion.Facing);
        Assert.Contains(observations, row => row.Kind == "battle-returned");
        var party = session.Current.Exploration.Party;
        var reentered = BattleTurnFlow.Start(session.Definition.Encounters[party.Encounter], party);
        foreach (var actor in reentered.Actors.Where(actor => actor.IsAlly))
        {
            var saved = party.Actors.Single(input => input.Actor == actor.Actor);
            Assert.Equal(saved.Progress, actor.Progress);
            Assert.Equal(saved.Exp, actor.Exp);
            Assert.Equal(actor.BaseAttack + actor.Definition.Growth!.AttackBonus, actor.Attack);
            Assert.Equal(actor.MaxHp, actor.Hp); Assert.Equal(actor.MaxMp, actor.Mp);
        }
        MoveAfterReturn(session, observations);
    }

    [PrivateInputFact(World)]
    public void OrdinaryLeaderLossKeepsItsOwnRecoveryAndRunsTheRealPostMessengerReload()
    {
        List<SessionObservation> observations = [];
        var session = StartBattle(observations);
        var identity = session.Current.SessionId;
        Play(session, observations, lose: true);
        Assert.Equal(identity, session.Current.SessionId);
        Assert.Contains(observations, row => row.Kind == "battle-outcome" && row.Detail == "Defeat");
        Assert.DoesNotContain(observations, row => row.Kind == "after-battle-join" || row.Program?.Program == "abcs-battle01");
        Assert.Contains(401, session.Current.Story.Flags); Assert.DoesNotContain(501, session.Current.Story.Flags);
        var world = session.Current.Exploration!;
        Assert.Equal("map-3", world.Map.Value); Assert.Equal(new MapPosition(32, 13), world.PlayerEntity.Position);
        Assert.Equal(1, world.PlayerEntity.Motion.Facing);
        long beforeGold = observations.LastOrDefault(row => row.Kind == "gold")?.After ?? 123;
        Assert.Equal((uint)(beforeGold / 2), world.Party.Gold);
        var leader = world.Party.Actors.Single(actor => actor.Actor.Value == "ally-0");
        Assert.Equal((ushort)1, leader.Defeats);
        Assert.Equal(leader.Progress?.MaxHp ?? session.Definition.Encounters[world.Party.Encounter].Deployments[0].Definition.MaxHp, leader.Hp);
        Assert.False(world.TryResolveEntity(new("entity-142"), out _));
        Assert.Contains(observations, row => row.Program == new ProgramLocation("cs-513ba", 0));
        Assert.Contains(observations, row => row.Program == new ProgramLocation("byte-513a8", 1));
        MoveAfterReturn(session, observations);
    }

    private static void MoveAfterReturn(GameSession session, List<SessionObservation> observations)
    {
        var world = session.Current.Exploration!;
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        var previous = world.PlayerEntity.Position;
        var direction = previous.X > 0 && !OriginalMapTraversal.IsBlocked(world.Layout, new(previous.X - 1, previous.Y))
            ? ExplorationDirection.West : ExplorationDirection.East;
        observations.AddRange(Accept(session, new Move(direction)).Observations);
        RunUntilStop(session, 3000, observations);
        Assert.NotEqual(previous, session.Current.Exploration!.PlayerEntity.Position);
    }

    internal static void Play(GameSession session, List<SessionObservation> observations, bool lose)
    {
        for (int action = 0; action < 300 && session.Current.Mode == SessionMode.Battle; action++)
        {
            if (!session.Current.HasBattleControl || session.Current.Selection is null)
            { Settle(session, observations); continue; }
            var battle = session.Current.Battle;
            var actor = battle.GetActor(session.Current.Selection.Actor);
            var enemy = battle.Actors.Where(row => !row.IsAlly && row.Hp > 0 && row.Position is not null).ToArray();
            int Threat(MapPosition p, BattleActorState? attacked = null) => enemy.Count(foe =>
                (foe != attacked || foe.Hp > Math.Max(1, (actor.Attack - foe.Defense) * 3 / 4)) &&
                BattleRange.Contains(p, foe.Position!, 0, foe.Definition.Move + 1));
            var grid = BattleMovement.Grid(battle, actor.Actor);
            var positions = (from y in Enumerable.Range(0, battle.Definition.Height)
                             from x in Enumerable.Range(0, battle.Definition.Width)
                             let p = new MapPosition(x, y)
                             where grid.CostAt(p) is not null && !battle.Actors.Any(row => row.Actor != actor.Actor && row.Hp > 0 && row.Position == p)
                             select p).ToArray();
            MapPosition destination = actor.Position!;
            ActorRef? target = null;
            SpellRef? spell = null;
            if (!lose)
            {
                var heal = actor.Spells.FirstOrDefault(key => battle.Definition.Spells.ContainsKey(key) && key.Level == 1);
                if (heal != default && battle.Definition.Spells.TryGetValue(heal, out var healing) && actor.Mp >= healing.MpCost)
                {
                    var healChoice = (from ally in battle.Actors.Where(row => row.IsAlly && row.Hp > 0 && row.MaxHp - row.Hp >= 6)
                                      from p in positions where BattleRange.Contains(p, ally.Actor == actor.Actor ? p : ally.Position!, healing.MinimumRange, healing.MaximumRange)
                                      orderby ally.Hp, grid.CostAt(p) select (ally, p)).FirstOrDefault();
                    if (healChoice.ally is not null) { destination = healChoice.p; target = healChoice.ally.Actor; spell = heal; }
                }
                if (target is null)
                {
                    var attack = (from foe in enemy from p in positions where BattleRange.Contains(p, foe.Position!, 1, 1)
                                  orderby foe.Hp, Threat(p, foe), grid.CostAt(p), foe.ProcessingOrder select (foe, p)).FirstOrDefault();
                    if (attack.foe is not null) { destination = attack.p; target = attack.foe.Actor; }
                }
            }
            if (target is null && (!lose || actor.Definition.Physical?.Leader == true))
            {
                var costs = battle.Definition.Terrain.Select(tile => BattleTerrainRules.MovementCost(tile, actor.Definition.Mover)).ToArray();
                var distances = enemy.Select(foe => WeightedMovement.Build(costs, foe.Position!.Y * 48 + foe.Position.X, 255)).ToArray();
                destination = positions.Where(p => lose || grid.CostAt(p) <= 4)
                    .OrderBy(p => distances.Min(distance => distance.CostAt(p) ?? int.MaxValue))
                    .ThenBy(p => grid.CostAt(p)).First();
                var support = battle.Actors.FirstOrDefault(row => row.IsAlly && row.Hp > 0 && row.Spells.Any(battle.Definition.Spells.ContainsKey));
                if (!lose && support is not null && support.Actor != actor.Actor &&
                    !BattleRange.Contains(actor.Position!, support.Position!, 0, 3))
                {
                    var toSupport = WeightedMovement.Build(costs, support.Position!.Y * 48 + support.Position.X, 255);
                    destination = positions.OrderBy(p => toSupport.CostAt(p) ?? int.MaxValue).ThenBy(p => grid.CostAt(p)).First();
                }
                if (!lose && support?.Actor == actor.Actor && battle.Actors.Any(row => row.IsAlly && row.Hp > 0 &&
                    !BattleRange.Contains(row.Position!, actor.Position!, 0, 3))) destination = actor.Position!;
            }
            if (!lose && actor.Definition.Physical?.Leader == true && actor.Hp <= 6 &&
                Threat(destination, target is { } attacked ? battle.GetActor(attacked) : null) * 3 >= actor.Hp)
            {
                destination = positions.OrderByDescending(p => enemy.Min(foe => Math.Abs(p.X - foe.Position!.X) + Math.Abs(p.Y - foe.Position.Y)))
                    .ThenBy(p => grid.CostAt(p)).First();
                target = null; spell = null;
            }
            var path = BattleMovement.Preview(battle, actor.Actor, destination).Path;
            for (int i = 1; i < path.Count; i++)
            {
                var delta = (path[i].X - path[i - 1].X, path[i].Y - path[i - 1].Y);
                observations.AddRange(Accept(session, new Move(delta switch { (1, 0) => ExplorationDirection.East, (-1, 0) => ExplorationDirection.West,
                    (0, 1) => ExplorationDirection.South, _ => ExplorationDirection.North })).Observations);
            }
            observations.AddRange(Accept(session, new Confirm()).Observations);
            observations.AddRange(Accept(session, spell is { } chosen ? new SelectSpell(chosen) : new ChooseAction(target is null ? SessionAction.Stay : SessionAction.PhysicalAttack)).Observations);
            if (target is { } selected) observations.AddRange(Accept(session, new SelectTarget(selected)).Observations);
            var result = Send(session, new Confirm()); observations.AddRange(result.Observations);
            Assert.True(result.Failure is null, result.Failure?.ToString());
            Settle(session, observations);
        }
        Assert.True(session.Current.Mode == SessionMode.Exploration, session.Current.Mode == SessionMode.Battle
            ? string.Join("; ", session.Current.Battle.Actors.Select(row => $"{row.Actor.Value} hp{row.Hp} pos{row.Position}")) : null);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    private static void Settle(GameSession session, List<SessionObservation> observations)
    {
        if (session.Current.StopReason == SessionStopReason.PlayerInput) return;
        var result = RunUntilStop(session, 20000, observations);
        Assert.True(result.Failure is null, result.Failure?.ToString());
    }
}
