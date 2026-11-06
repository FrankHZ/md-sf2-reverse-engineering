using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleGivePersistenceTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void TransferredEquipmentSurvivesV2RecordRestoreAndVictoryFieldCustody(bool exchange)
    {
        var (basis, initial) = BattleSaveFixture.Create(); var old = initial.Battle;
        var reserve = initial.Story.FieldPlayer!.RetainedAllies!.Single();
        var receiver = basis.Encounters["opening"].Deployments.Single(d => d.Actor == reserve.Actor) with
        {
            Position = new(4, 6), Definition = new(reserve.Actor, BattleClassRule.UnpromotedSwordsman, 1, 20, 5, 8, 4, 6, false, 6, [],
                new(PhysicalCriticalRule.OneIn32WithHalfBonus, false, false, 0), sourceLoadout: reserve.SourceLoadout,
                growth: old.Actors[0].Definition.Growth),
        };
        var encounter = ConnectedBattleGiveTests.Bind(old.Definition.WithDeployments(old.Definition.Deployments.Append(receiver)));
        var definition = new ScenarioDefinition(basis.Package,
            basis.Encounters.Values.Select(e => e.Encounter == encounter.Encounter ? encounter : e),
            exploration: basis.Exploration, encounterStarts: basis.EncounterStarts.Values) { Persistence = new Sf2OpeningRestore() };
        var donor = old.Actors[0];
        var giving = new BattleSourceLoadout([178, 50, 127, 127], donor.SourceLoadout!.Spells);
        var receiving = new BattleSourceLoadout(exchange ? [178, 0, 0, 50] : [0, 127, 127, 127], reserve.SourceLoadout!.Spells);
        var actors = old.Actors.Select(a => a.Actor == donor.Actor
            ? new BattleActorState(encounter.Deployments.Single(d => d.Actor == a.Actor), a.Hp, a.Mp, a.Exp, a.Position, a.Kills, a.Defeats,
                progress: a.Progress! with { SourceLoadout = giving }, sourceLoadout: giving, activationWord: 0)
            : a.Actor.Value == "enemy" ? a.With(hp: 1, position: new(5, 5), activationWord: 0x2061) : a).Append(
            new(encounter.Deployments.Single(d => d.Actor == receiver.Actor), 7, 2, 38, receiver.Position, 4, 2,
                progress: reserve.Progress! with { SourceLoadout = receiving }, sourceLoadout: receiving, activationWord: 0));
        var battle = new EngineBattleState(encounter, actors.OrderBy(a => a.ProcessingOrder), old.MainSeed, old.ThinkingSeed, old.Round, old.Queue, old.Cursor, old.Gold,
            old.StartPolicy, old.Regions, itemCustody: old.ItemCustody, deathCredit: old.DeathCredit);
        var story = initial.Story.Copy(null, fieldPlayer: initial.Story.FieldPlayer with { RetainedAllies = [] });
        var data = BattleSaveFixture.Capture(definition, BattleSaveFixture.At(battle, story));
        var session = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition, data, RuleCompositions.Sf2())).Session;
        ConnectedBattleGiveTests.Choose(session, receiver.Actor, returned: exchange ? 0 : null);
        Assert.NotNull(session.QueryBattleSave().Error);
        Accept(session, new Confirm()); var wait = session.Current.BattleGive!;
        Assert.NotNull(session.QueryBattleSave().Error);
        Assert.Equal(14, session.Current.Battle.GetActor(donor.Actor).Attack);
        Assert.Equal(14, session.Current.Battle.GetActor(receiver.Actor).Attack);
        Accept(session, new CompleteTextReveal(wait.Token));
        FinishBattleScenes(session, Accept(session, new Acknowledge(wait.Token)));
        var settled = session.Current; Assert.Equal(BattleSelectionStage.Movement, settled.Selection!.Stage);
        var expectedDonor = exchange ? new ushort[] { 50, 50, 127, 127 } : [50, 127, 127, 127];
        var expectedReceiver = exchange ? new ushort[] { 0, 0, 50, 50 } : [0, 50, 127, 127];
        Assert.Equal(expectedDonor, settled.Battle.GetActor(donor.Actor).SourceLoadout!.Items);
        Assert.Equal(expectedReceiver, settled.Battle.GetActor(receiver.Actor).SourceLoadout!.Items);
        var saved = BattleSaveFixture.Capture(definition, settled);
        // Versioned record codec round-trip in memory; no claim of physical slot I/O.
        var wire = OpeningSaveJson.WriteRecord(new BattleSaveRecord(1, 1, DateTimeOffset.UnixEpoch, saved));
        var record = Assert.IsType<BattleSaveRecord>(OpeningSaveJson.ReadRecord(wire, 1));
        var restored = Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(definition, record, session.Rules)).Session;
        BattleSaveFixture.EqualBattle(settled.Battle, restored.Current.Battle);
        // The intervening ordinary enemy turn approached the recipient; use legal movement
        // to attack from its new adjacent tile rather than assuming the fixture stayed still.
        FinishMovement(restored, Accept(restored, new Move(ExplorationDirection.East)));
        var returned = BattleSaveReturnTests.End(definition, restored.Current, BattleOutcomeKind.Victory);
        Assert.True(returned.CanWaitAtInput);
        foreach (var (actor, words) in new[] { (donor.Actor, expectedDonor), (receiver.Actor, expectedReceiver) })
        {
            var carried = returned.Exploration!.Party.Actors.Single(a => a.Actor == actor);
            Assert.Equal(words, carried.SourceLoadout!.Items); Assert.Equal(words, carried.Progress!.SourceLoadout!.Items);
        }
        Assert.Equal(old.ItemCustody!.OnceFlags, returned.Exploration!.Party.ItemCustody!.OnceFlags);
        Assert.Equal(old.ItemCustody.Deals, returned.Exploration.Party.ItemCustody.Deals);
        var field = OpeningSave.Capture(definition, returned, session.Rules, returned.SessionId, returned.Revision); Assert.Null(field.Error);
        var reloaded = Assert.IsType<SessionStarted>(GameSession.Restore(definition, field.Data!, session.Rules)).Session;
        Assert.Equal(expectedDonor, reloaded.Current.Exploration!.Party.Actors.Single(a => a.Actor == donor.Actor).SourceLoadout!.Items);
        Assert.Equal(expectedReceiver, reloaded.Current.Exploration.Party.Actors.Single(a => a.Actor == receiver.Actor).SourceLoadout!.Items);
    }
}
