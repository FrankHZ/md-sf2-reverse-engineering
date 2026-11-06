using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleInspectionQueryTests
{
    [Fact]
    public void RepeatedPublicReadsPreserveTheWholeLiveSnapshotAndExposeNoCombatInternals()
    {
        var session = Start(); var before = session.Current;
        var query = session.QueryBattleInspection();
        Assert.True(query.Available, query.Reason); Assert.Equal(before.Selection!.Actor, query.ActiveActor);
        Assert.Equal(before.SessionId, query.SessionId); Assert.Equal(before.Revision, query.Revision);
        Assert.Equal(before.Battle.Definition.Width, query.Width);
        Assert.Equal(query.Width * query.Height, query.Terrain.Count);
        for (int y = 0; y < query.Height; y++)
            for (int x = 0; x < query.Width; x++)
                Assert.Equal(before.Battle.Definition.Terrain[y * 48 + x].Surface, query.Terrain[y * query.Width + x]);
        Assert.True(query.Terrain.Distinct().Count() > 1);
        Assert.Throws<NotSupportedException>(() => ((IList<TerrainSurface>)query.Terrain).Clear());
        Assert.All(query.Units, unit =>
        {
            var detail = session.QueryBattleInspection(query.SessionId, query.Revision, unit.Actor);
            Assert.True(detail.Available); Assert.NotNull(detail.Unit);
            var actor = before.Battle.GetActor(unit.Actor);
            Assert.Equal(actor.Hp, detail.Unit.Hp); Assert.Equal(actor.Mp, detail.Unit.Mp);
            Assert.Equal(actor.Attack, detail.Unit.Attack); Assert.Equal(actor.Defense, detail.Unit.Defense);
            Assert.Equal(actor.Spells, detail.Unit.Spells.Select(s => s.Spell));
            if (!unit.Ally) { Assert.Null(detail.Unit.Level); Assert.Null(detail.Unit.Exp); Assert.Null(detail.Unit.ClassId); }
        });
        Assert.Same(before, session.Current); // includes seeds, sequence, queue, story, resources and selection/path
        Assert.Throws<NotSupportedException>(() => ((IList<BattleInspectionMarker>)query.Units).Clear());
        var own = session.QueryBattleInspection(unit: query.ActiveActor).Unit!;
        Assert.Throws<NotSupportedException>(() => ((IList<string>)own.Status).Clear());
    }

    [Fact]
    public void ActualMovementAndUndoMustFinishBeforeInspectionAndOldBindingsReject()
    {
        var session = Start(); var initial = session.QueryBattleInspection();
        var step = Accept(session, new Move(ExplorationDirection.East));
        Assert.False(session.QueryBattleInspection().Available);
        FinishMovement(session, step);
        Assert.Equal("inspection-unsettled", session.QueryBattleInspection().Reason);
        var returning = Accept(session, new Cancel());
        Assert.NotNull(session.Current.BattleMovement); Assert.False(session.QueryBattleInspection().Available);
        FinishMovement(session, returning);
        Assert.True(session.QueryBattleInspection().Available);
        var baseline = session.Current;
        Assert.Equal("stale-inspection", session.QueryBattleInspection(initial.SessionId, initial.Revision).Reason);
        Assert.Equal("stale-inspection", session.QueryBattleInspection(Guid.NewGuid(), baseline.Revision).Reason);
        Assert.Equal("unit-not-disclosed", session.QueryBattleInspection(unit: new("missing")).Reason);
        Assert.Same(baseline, session.Current);
        Accept(session, new Confirm()); Assert.False(session.QueryBattleInspection().Available);
        Accept(session, new Cancel()); Assert.True(session.QueryBattleInspection().Available);
    }

    [Fact]
    public void LaterReadsUseChangedLiveVitalsStatusAndLoadoutRatherThanInitialDefinitions()
    {
        var session = Start("battle-equipment"); var before = session.QueryBattleInspection();
        var actor = session.Current.Battle.GetActor(before.ActiveActor!.Value);
        var changed = actor.With(hp: (ushort)(actor.Hp - 1), mp: (byte)Math.Max(0, actor.Mp - 1),
            status: 0x142, sourceLoadout: new([127, 127, 127, 127], []));
        var current = session.Current;
        var battle = current.Battle.With(actors: current.Battle.Actors.Select(a => a.Actor == actor.Actor ? changed : a));
        var live = new SessionSnapshot(current.SessionId, current.Revision + 1, current.ObservationSequence,
            ((ActiveBattle)current.Active) with { Battle = battle }, current.Story, current.StopReason);
        var detail = BattleInspectionQuery.Query(session.Definition, live, requested: actor.Actor).Unit!;
        Assert.Equal(changed.Hp, detail.Hp); Assert.Equal(changed.Mp, detail.Mp);
        Assert.Contains("poison", detail.Status); Assert.Contains("sleep", detail.Status); Assert.Contains("silence", detail.Status);
        Assert.All(detail.Inventory, slot => Assert.True(slot.Empty));
        Assert.Same(current, session.Current);
        Assert.NotEqual(before.Revision, live.Revision);
    }

    [Fact]
    public void ANewPublicReadReflectsAnActuallyCommittedEquipmentChange()
    {
        var session = Start("battle-equipment"); var initial = session.QueryBattleInspection();
        var before = session.QueryBattleInspection(unit: initial.ActiveActor).Unit!;
        Assert.True(before.Inventory[0].Equipped); Assert.False(before.Inventory[1].Equipped);
        Accept(session, new Confirm()); Accept(session, new BeginEquipment()); Accept(session, new SelectEquipment(1));
        Accept(session, new Confirm()); FinishMovement(session, Accept(session, new Cancel()));
        var baseline = session.Current;
        var after = session.QueryBattleInspection(unit: initial.ActiveActor).Unit!;
        Assert.Equal(30, after.Attack); Assert.NotEqual(before.Attack, after.Attack);
        Assert.False(after.Inventory[0].Equipped); Assert.True(after.Inventory[1].Equipped);
        Assert.Equal("stale-inspection", session.QueryBattleInspection(initial.SessionId, initial.Revision).Reason);
        Assert.Same(baseline, session.Current);
    }

    [Theory]
    [InlineData(BattleMover.Regular, TerrainProtection.Heavy, 30)]
    [InlineData(BattleMover.Flying, TerrainProtection.Heavy, 0)]
    [InlineData(BattleMover.Hovering, TerrainProtection.Light, 15)]
    public void LandEffectIsQualifiedByInspectedActorMoverAndActualTile(BattleMover mover, TerrainProtection protection, int expected)
    {
        var session = Start(); var current = session.Current; var original = current.Battle.GetActor(current.Selection!.Actor);
        var def = original.Definition;
        // Replace immutable mover content without a UI percentage fixture.
        var actor = new BattleActorState(original.Deployment with { Definition = new BattleActorDefinition(def.Actor, def.ClassRule, def.Level, def.MaxHp, def.MaxMp,
            def.Attack, def.Defense, def.Agility, def.ExtraRoundAction, def.Move, def.Spells, def.Physical,
            mover, def.SourceLoadout, def.Growth, def.Equipment, def.Resistance) },
            original.Hp, original.Mp, original.Exp, original.Position, original.Kills, original.Defeats);
        var terrain = current.Battle.Definition.Terrain.ToArray();
        terrain[original.Position!.Y * 48 + original.Position.X] = new(TerrainSurface.Brush, protection);
        var d = current.Battle.Definition;
        var definition = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, terrain, d.Deployments, d.Spells.Values);
        var battle = new EngineBattleState(definition, current.Battle.Actors.Select(a => a.Actor == actor.Actor ? actor : a),
            current.Battle.MainSeed, current.Battle.ThinkingSeed, current.Battle.Round, current.Battle.Queue, current.Battle.Cursor, current.Battle.Gold);
        var live = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            battle, current.Selection, current.StopReason);
        var query = BattleInspectionQuery.Query(session.Definition, live, requested: actor.Actor);
        Assert.True(query.Available); Assert.Equal(expected, query.Land!.Percent);
        Assert.Equal(actor.Actor, query.Land.Actor); Assert.Equal(mover, query.Land.Mover); Assert.Equal(actor.Position, query.Land.Position);
    }

    [Fact]
    public void HiddenAndPendingAppearancesAreNotPublishedAndBecomeLiveOnlyAfterDelivery()
    {
        var session = Start("hidden-spawns"); var first = session.QueryBattleInspection();
        Assert.True(first.Available);
        Assert.DoesNotContain(first.Units, a => a.Actor == HiddenSpawnTests.Hidden || a.Actor == HiddenSpawnTests.Returner);
        Assert.Equal("unit-not-disclosed", session.QueryBattleInspection(unit: HiddenSpawnTests.Hidden).Reason);
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.North)));
        HiddenSpawnSessionTests.Settle(session, Stay(session), stopAtAppearance: true);
        Assert.NotNull(session.Current.BattleRound);
        Assert.False(session.QueryBattleInspection().Available);
        Assert.Empty(session.QueryBattleInspection().Units);
        HiddenSpawnSessionTests.Settle(session, new(session.Current, [], session.Current.StopReason));
        Assert.Contains(session.QueryBattleInspection().Units, a => a.Actor == HiddenSpawnTests.Hidden);
        Assert.Contains(session.QueryBattleInspection().Units, a => a.Actor == HiddenSpawnTests.Returner);
    }

    [Fact]
    public void FaultStoryOutcomeAutomaticAndFieldBoundariesRejectWithoutDisclosingRoster()
    {
        var session = Start(); var c = session.Current;
        foreach (var stop in new[] { SessionStopReason.Faulted, SessionStopReason.Unsupported, SessionStopReason.SimulationWait, SessionStopReason.PresentationWait })
        {
            var state = new SessionSnapshot(c.SessionId, c.Revision, c.ObservationSequence, c.Active, c.Story, stop);
            var query = BattleInspectionQuery.Query(session.Definition, state); Assert.False(query.Available); Assert.Empty(query.Units);
        }
        var story = new SessionSnapshot(c.SessionId, 0, 0, c.Active, new StoryState(cursor: new("control", 0)), c.StopReason);
        Assert.False(BattleInspectionQuery.Query(session.Definition, story).Available);
        var outcome = new SessionSnapshot(c.SessionId, 0, 0, ((ActiveBattle)c.Active) with { Outcome = BattleOutcomeKind.Victory }, c.Story, c.StopReason);
        Assert.False(BattleInspectionQuery.Query(session.Definition, outcome).Available);
        Assert.False(Start("harbor-arrival").QueryBattleInspection().Available);
    }
}
