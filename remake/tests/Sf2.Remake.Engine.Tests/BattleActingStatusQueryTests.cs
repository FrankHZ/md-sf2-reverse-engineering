using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleActingStatusQueryTests
{
    [Theory]
    [InlineData(BattleMover.Regular, 15, 30)]
    [InlineData(BattleMover.Flying, 0, 0)]
    [InlineData(BattleMover.Hovering, 15, 30)]
    public void CardUsesDeliveredTileThroughPreviewAndEveryUndoSegment(BattleMover mover, int light, int heavy)
    {
        var content = Admitted(); var original = content.Definition.Encounters.Values.Single();
        var deployments = original.Deployments.Select(a =>
        {
            var d = a.Definition;
            return a with { Definition = new BattleActorDefinition(d.Actor, d.ClassRule, d.Level, d.MaxHp, d.MaxMp,
                d.Attack, d.Defense, d.Agility, d.ExtraRoundAction, d.Move, d.Spells, d.Physical, mover,
                d.SourceLoadout, d.Growth, d.Equipment, d.Resistance) };
        });
        var battle = new BattleDefinition(original.Encounter, original.Map, original.Width, original.Height,
            original.Terrain, deployments, original.Spells.Values);
        var session = Assert.IsType<SessionStarted>(GameSession.Start(new ScenarioDefinition("status", [battle]), content.Start)).Session;
        var before = session.Current;
        void Read(MapPosition position, int percent)
        {
            var snapshot = session.Current;
            var card = session.QueryBattleActingStatus()!;
            Assert.Equal(snapshot.Selection!.Actor, card.Unit.Actor);
            Assert.Equal(card.Unit.Actor, card.Land.Actor);
            Assert.Equal(position, card.Land.Position); Assert.Equal(percent, card.Land.Percent);
            Assert.Equal(before.Battle.GetActor(card.Unit.Actor).Hp, card.Unit.Hp);
            Assert.Equal(card, session.QueryBattleActingStatus()); Assert.Same(snapshot, session.Current);
        }
        Read(new(3, 3), light);
        var moving = Accept(session, new Move(ExplorationDirection.East));
        Assert.Equal(new MapPosition(4, 3), session.Current.Selection!.Preview.Destination);
        Read(new(3, 3), light); Assert.False(session.QueryBattleInspection().Available);
        FinishMovement(session, moving); Read(new(4, 3), heavy);
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.East))); Read(new(5, 3), heavy);
        Accept(session, new Cancel()); Read(new(5, 3), heavy);
        Assert.Equal(new MapPosition(3, 3), session.Current.Selection!.Preview.Destination);
        var segment = session.Current.BattleMovement!;
        Accept(session, new CompletePresentation(segment.Token, segment.CompletionKind)); Read(new(4, 3), heavy);
        FinishMovement(session, new(session.Current, [], session.Current.StopReason)); Read(new(3, 3), light);
        Assert.Equal(before.Battle.MainSeed, session.Current.Battle.MainSeed);
        Assert.Equal(before.Battle.ThinkingSeed, session.Current.Battle.ThinkingSeed);
        Assert.Equal(before.Battle.Cursor, session.Current.Battle.Cursor);
        Assert.Equal(before.Battle.Round, session.Current.Battle.Round);
        Assert.True(session.QueryBattleInspection().Available);
    }

    [Fact]
    public void PublicSummaryReadsCurrentProgressAndSharesPrivateThresholdAndNamesWithInspection()
    {
        var session = Start("battle-equipment"); var c = session.Current;
        var actor = c.Battle.GetActor(c.Selection!.Actor);
        var source = new EncounterSource("authored-test", "controlled", "synthetic", "not-source-evidence");
        var encounter = new BattleEncounterDefinition(source, source, new(2, "synthetic"), new(0, 0, 0),
            new(66, 0, 0, 16, 16, 255, 255), new("none", false, false), [], [], [], new(0, 0, 0), "", "", [], new Dictionary<byte, int>());
        var definitions = new PrivateBattleDefinitions(encounter, [], [new(0, "CLASS", "\"Live class\"", 5, "", "REGULAR", "")], [], [], [], []);
        var definition = new ScenarioDefinition("status", [c.Battle.Definition], definitions);
        foreach (ushort hp in new ushort[] { 99, 100, 101 })
        {
            var changed = actor.With(hp: hp, mp: 0, progress: new BattleActorProgress(7, 101, actor.MaxMp, actor.BaseAttack, actor.Defense, actor.Agility, actor.Spells, actor.SourceLoadout));
            var battle = c.Battle.With(actors: c.Battle.Actors.Select(a => a.Actor == actor.Actor ? changed : a));
            var live = new SessionSnapshot(c.SessionId, c.Revision, c.ObservationSequence,
                ((ActiveBattle)c.Active) with { Battle = battle }, c.Story, c.StopReason);
            var card = BattleActingStatusQuery.Query(definition, live)!;
            var inspected = BattleInspectionQuery.Query(definition, live, requested: actor.Actor).Unit!;
            Assert.Equal(hp < 100 ? (int?)hp : null, card.Unit.Hp); Assert.Null(card.Unit.MaxHp);
            Assert.Equal(7, card.Unit.Level); Assert.Equal("Live class", card.Unit.ClassName);
            Assert.Equal("Name unavailable", card.Unit.Name); Assert.Equal(inspected.Hp, card.Unit.Hp);
            Assert.Equal(inspected.MaxHp, card.Unit.MaxHp); Assert.Equal(inspected.ClassName, card.Unit.ClassName);
            Assert.Equal(inspected.Level, card.Unit.Level);
            Assert.Equal((int)hp, BattleActingStatusQuery.Query(session.Definition, live)!.Unit.Hp);
        }
    }

    [Fact]
    public void BusyHiddenDeadUninitializedAutomaticAndFieldStatesDoNotDiscloseAnActingCard()
    {
        var session = Start(); var c = session.Current; var active = (ActiveBattle)c.Active;
        var actor = c.Battle.GetActor(c.Selection!.Actor);
        Assert.Null(session.QueryBattleActingStatus()!.Unit.ClassId); // missing authored class is unavailable
        foreach (var changed in new[] { actor.With(clearPosition: true), actor.With(initialized: false), actor.With(hp: 0),
            new BattleActorState(actor.Deployment with { Control = BattleControl.Automatic }, actor.Hp, actor.Mp, actor.Exp, actor.Position, 0, 0) })
        {
            var live = new SessionSnapshot(c.SessionId, 0, 0, active with { Battle = c.Battle.With(actors:
                c.Battle.Actors.Select(a => a.Actor == actor.Actor ? changed : a)) }, c.Story, c.StopReason);
            Assert.Null(BattleActingStatusQuery.Query(session.Definition, live));
        }
        foreach (var stop in new[] { SessionStopReason.Faulted, SessionStopReason.Unsupported, SessionStopReason.SimulationWait })
            Assert.Null(BattleActingStatusQuery.Query(session.Definition, new(c.SessionId, 0, 0, active, c.Story, stop)));
        Assert.Null(BattleActingStatusQuery.Query(session.Definition, new(c.SessionId, 0, 0, active with { Selection = null }, c.Story, c.StopReason)));
        Assert.Null(BattleActingStatusQuery.Query(session.Definition, new(c.SessionId, 0, 0, active with { Outcome = BattleOutcomeKind.Victory }, c.Story, c.StopReason)));
        Assert.Null(BattleActingStatusQuery.Query(session.Definition, new(c.SessionId, 0, 0, active, new StoryState(cursor: new("control", 0)), c.StopReason)));
        var moving = Accept(session, new Move(ExplorationDirection.East)).Snapshot;
        var delivery = moving.BattleMovement!;
        var automatic = new BattleMovementState(delivery.Actor, delivery.Path, delivery.Segment, delivery.Token, BattleMovementPurpose.Automatic);
        Assert.Null(BattleActingStatusQuery.Query(session.Definition, new(c.SessionId, 0, 0,
            ((ActiveBattle)moving.Active) with { Movement = automatic }, c.Story, moving.StopReason)));
        Assert.Null(BattleActingStatusQuery.Query(session.Definition, new(c.SessionId, 0, 0,
            moving.Active, new StoryState(cursor: new("control", 0)), moving.StopReason)));
        Assert.Null(Start("harbor-arrival").QueryBattleActingStatus());
    }
}
