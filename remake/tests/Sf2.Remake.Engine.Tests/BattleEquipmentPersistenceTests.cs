using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleEquipmentPersistenceTests
{
    [Theory]
    [InlineData(1)] [InlineData(4)]
    public void FreeEquipmentSurvivesV2RecordRestoreActualAttackAndFieldReturn(int slot)
    {
        var (basis, initial) = BattleSaveFixture.Create();
        var old = initial.Battle;
        var encounter = PrivateFieldPreparation.Equipment(old.Definition, ConnectedBattleEquipmentTests.Source(), allowBattleChanges: true);
        var definition = new ScenarioDefinition(basis.Package,
            basis.Encounters.Values.Select(e => e.Encounter == encounter.Encounter ? encounter : e),
            exploration: basis.Exploration, encounterStarts: basis.EncounterStarts.Values) { Persistence = new Sf2OpeningRestore() };
        var hero = old.Actors[0];
        var loadout = new BattleSourceLoadout([178, 50, 127, 127], hero.SourceLoadout!.Spells);
        var deployment = encounter.Deployments.Single(d => d.Actor == hero.Actor);
        var changed = new BattleActorState(deployment, hero.Hp, hero.Mp, hero.Exp, hero.Position, hero.Kills, hero.Defeats,
            progress: hero.Progress! with { SourceLoadout = loadout }, sourceLoadout: loadout, activationWord: 0);
        var battle = new EngineBattleState(encounter, old.Actors.Select(a => a.Actor == hero.Actor ? changed :
            a.Actor.Value == "enemy" ? a.With(hp: 1, position: new(5, 5), activationWord: 0x2061) : a),
            old.MainSeed, old.ThinkingSeed, old.Round, old.Queue, old.Cursor, old.Gold, old.StartPolicy, old.Regions,
            itemCustody: old.ItemCustody, deathCredit: old.DeathCredit);
        var story = initial.Story.Copy(null, fieldPlayer: initial.Story.FieldPlayer! with
            { RetainedAllies = initial.Story.FieldPlayer.RetainedAllies!.Select(a => a with { Absent = false }).ToArray() });
        var initialData = BattleSaveFixture.Capture(definition, BattleSaveFixture.At(battle, story));
        var session = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition, initialData, RuleCompositions.Sf2())).Session;
        foreach (SessionCommand command in new SessionCommand[] { new Confirm(), new BeginEquipment(), new SelectEquipment(slot), new Confirm(), new Cancel() })
            Accept(session, command);
        var live = session.Current;
        Assert.Equal(slot == 4 ? 14 : 19, live.Battle.GetActor(hero.Actor).Attack);
        Assert.Equal(old.Queue, live.Battle.Queue); Assert.Equal(old.Cursor, live.Battle.Cursor);
        Assert.Equal(old.MainSeed, live.Battle.MainSeed); Assert.Equal(old.ThinkingSeed, live.Battle.ThinkingSeed);
        Assert.Equal(old.ItemCustody!.OnceFlags, live.Battle.ItemCustody!.OnceFlags);
        Assert.Equal(old.ItemCustody.Deals, live.Battle.ItemCustody.Deals);
        var saved = BattleSaveFixture.Capture(definition, live);
        var wire = OpeningSaveJson.WriteRecord(new BattleSaveRecord(1, 1, DateTimeOffset.UnixEpoch, saved));
        var record = Assert.IsType<BattleSaveRecord>(OpeningSaveJson.ReadRecord(wire, 1));
        var restored = Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(definition, record, session.Rules)).Session;
        BattleSaveFixture.EqualBattle(live.Battle, restored.Current.Battle);
        var returned = BattleSaveReturnTests.End(definition, restored.Current, BattleOutcomeKind.Victory);
        Assert.True(returned.CanWaitAtInput);
        var party = returned.Exploration!.Party.Actors.Single(a => a.Actor == hero.Actor);
        var expected = slot == 4 ? new ushort[] { 50, 50, 127, 127 } : [50, 178, 127, 127];
        Assert.Equal(expected, party.SourceLoadout!.Items);
        Assert.Equal(expected, party.Progress!.SourceLoadout!.Items);
        Assert.Equal(old.ItemCustody.OnceFlags, returned.Exploration.Party.ItemCustody!.OnceFlags);
        Assert.Equal(old.ItemCustody.Deals, returned.Exploration.Party.ItemCustody.Deals);
        var field = OpeningSave.Capture(definition, returned, session.Rules, returned.SessionId, returned.Revision);
        Assert.Null(field.Error);
        var reloaded = Assert.IsType<SessionStarted>(GameSession.Restore(definition, field.Data!, session.Rules)).Session;
        Assert.Equal(expected, reloaded.Current.Exploration!.Party.Actors.Single(a => a.Actor == hero.Actor).SourceLoadout!.Items);
    }
}
