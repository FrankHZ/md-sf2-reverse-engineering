using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleDiscardPersistenceTests
{
    [Theory]
    [InlineData(0)] [InlineData(1)]
    public void EquippedOrRareDropSurvivesV2RecordRestoreVictoryAndFieldSave(int slot)
    {
        var (basis, initial) = BattleSaveFixture.Create(); var old = initial.Battle;
        var encounter = ConnectedBattleDiscardTests.Bind(old.Definition);
        var definition = new ScenarioDefinition(basis.Package,
            basis.Encounters.Values.Select(e => e.Encounter == encounter.Encounter ? encounter : e),
            exploration: basis.Exploration, encounterStarts: basis.EncounterStarts.Values) { Persistence = new Sf2OpeningRestore() };
        var donor = old.Actors[0]; var loadout = new BattleSourceLoadout([178, 123, 50, 127], donor.SourceLoadout!.Spells);
        var actors = old.Actors.Select(a => a.Actor == donor.Actor
            ? new BattleActorState(encounter.Deployments.Single(d => d.Actor == a.Actor), a.Hp, a.Mp, a.Exp, a.Position, a.Kills, a.Defeats,
                progress: a.Progress! with { SourceLoadout = loadout }, sourceLoadout: loadout, activationWord: 0)
            : a.Actor.Value == "enemy" ? a.With(hp: 1, position: new(5, 5), activationWord: 0x2061) : a);
        var battle = new EngineBattleState(encounter, actors, old.MainSeed, old.ThinkingSeed, old.Round, old.Queue, old.Cursor, old.Gold,
            old.StartPolicy, old.Regions, itemCustody: old.ItemCustody, deathCredit: old.DeathCredit);
        var story = initial.Story.Copy(null, fieldPlayer: initial.Story.FieldPlayer! with
            { RetainedAllies = initial.Story.FieldPlayer.RetainedAllies!.Select(a => a with { Absent = false }).ToArray() });
        var data = BattleSaveFixture.Capture(definition, BattleSaveFixture.At(battle, story));
        var session = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition, data, RuleCompositions.Sf2())).Session;
        Accept(session, new Confirm()); Accept(session, new BeginDiscard()); Accept(session, new SelectDiscardItem(slot)); Accept(session, new Confirm());
        Assert.NotNull(session.QueryBattleSave().Error);
        Accept(session, new Confirm()); var wait = session.Current.BattleDiscard!;
        Assert.NotNull(session.QueryBattleSave().Error);
        Accept(session, new CompleteTextReveal(wait.Token)); Accept(session, new Acknowledge(wait.Token));
        FinishMovement(session, Accept(session, new Cancel()));
        var settled = session.Current; Assert.Equal(BattleSelectionStage.Movement, settled.Selection!.Stage);
        var expected = slot == 0 ? new ushort[] { 123, 50, 127, 127 } : [178, 50, 127, 127];
        Assert.Equal(expected, settled.Battle.GetActor(donor.Actor).SourceLoadout!.Items);
        Assert.Equal(slot == 0 ? donor.BaseAttack : donor.BaseAttack + 5, settled.Battle.GetActor(donor.Actor).Attack);
        var expectedDeals = old.ItemCustody!.Deals.ToDictionary(); if (slot == 1) expectedDeals[123] = 1;
        Assert.Equal(expectedDeals, settled.Battle.ItemCustody!.Deals);
        var saved = BattleSaveFixture.Capture(definition, settled);
        // Versioned codec round-trip in memory, not physical slot-store I/O.
        var wire = OpeningSaveJson.WriteRecord(new BattleSaveRecord(1, 1, DateTimeOffset.UnixEpoch, saved));
        var record = Assert.IsType<BattleSaveRecord>(OpeningSaveJson.ReadRecord(wire, 1));
        var restored = Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(definition, record, session.Rules)).Session;
        BattleSaveFixture.EqualBattle(settled.Battle, restored.Current.Battle);
        var returned = BattleSaveReturnTests.End(definition, restored.Current, BattleOutcomeKind.Victory);
        Assert.True(returned.CanWaitAtInput);
        var party = returned.Exploration!.Party;
        var carried = party.Actors.Single(a => a.Actor == donor.Actor);
        Assert.Equal(expected, carried.SourceLoadout!.Items); Assert.Equal(expected, carried.Progress!.SourceLoadout!.Items);
        Assert.Equal(expectedDeals, party.ItemCustody!.Deals); Assert.Equal(old.ItemCustody.OnceFlags, party.ItemCustody.OnceFlags);
        var field = OpeningSave.Capture(definition, returned, session.Rules, returned.SessionId, returned.Revision); Assert.Null(field.Error);
        var reloaded = Assert.IsType<SessionStarted>(GameSession.Restore(definition, field.Data!, session.Rules)).Session;
        Assert.Equal(expected, reloaded.Current.Exploration!.Party.Actors.Single(a => a.Actor == donor.Actor).SourceLoadout!.Items);
        Assert.Equal(expectedDeals, reloaded.Current.Exploration.Party.ItemCustody!.Deals);
        Assert.Equal(old.ItemCustody.OnceFlags, reloaded.Current.Exploration.Party.ItemCustody.OnceFlags);
    }
}
