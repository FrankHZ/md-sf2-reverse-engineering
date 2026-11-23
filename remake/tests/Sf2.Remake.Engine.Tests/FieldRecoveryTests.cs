using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.TestSupport;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;
using static Sf2.Remake.Engine.Tests.FieldInventoryTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class FieldRecoveryTests
{
    private static readonly SpellRef Heal = new("heal", 1), Detox = new("detox", 1);
    private static readonly SpellRef Detox2 = new("detox", 2), Detox3 = new("detox", 3);
    private static GameSession Recovery(byte mp = 7, ushort targetHp = 100, ushort targetStatus = 2, byte recoveryItem = 0,
        bool higherRanks = false, ushort casterStatus = 0, ushort casterHp = 50)
    {
        var content = new FieldPreparationDefinition([new(recoveryItem, "Recovery item", false, false), new(3, "Antidote", false, false)],
            [new(recoveryItem, FieldRecoveryEffect.NoEffect), new(3, FieldRecoveryEffect.CurePoison)],
            [new(Heal, "Heal", 3, FieldRecoveryEffect.NoEffect), new(Detox, "Detox", 3, FieldRecoveryEffect.CurePoison),
             new(Detox2, "Detox", 6, FieldRecoveryEffect.CurePoisonAndParalysis),
             new(Detox3, "Detox", 10, FieldRecoveryEffect.CurePoisonAndParalysis)]);
        var loadout = new BattleSourceLoadout([recoveryItem, 3, 3, 127], []);
        return Session(carried: actor => actor.Actor == Giver ? actor with { Hp = casterHp, Mp = mp, Status = casterStatus,
            SourceLoadout = loadout, Progress = new(2, 200, 10, 20, 4, 60, higherRanks ? [Heal, Detox, Detox2, Detox3] : [Heal, Detox], loadout)
            { CurrentStats = higherRanks ? new(8, 20, 3) : null } }
            : actor.Actor == Recipient ? actor with { Hp = targetHp, Status = targetStatus } : actor, preparation: content);
    }

    [Theory]
    [InlineData(2, 0x103, 3, 1)]
    [InlineData(3, 0x101, 1, 0)]
    [InlineData(3, 0x100, 0, 0)]
    public void LearnedHigherFieldDetoxPaysOnceAndRefreshesSelfEvenWithoutAnAilment(
        byte rank, ushort status, ushort cleared, byte remainingMp)
    {
        var session = Recovery(higherRanks: true, casterStatus: status, casterHp: 0);
        var before = session.Current;
        var spell = new SpellRef("detox", rank);
        var choice = Assert.Single(session.QueryFieldActions(Giver).Spells, choice => choice.Spell == spell);
        Assert.Equal(rank == 2 ? 6 : 10, choice.MpCost);
        Assert.Equal(SpellEffect.CurePoisonAndParalysis, choice.Description!.Effect);
        Assert.Equal(SpellTarget.PartyMember, choice.Description.Target);
        Assert.Equal((byte)(7 - remainingMp), choice.Description.FieldPayment);
        Assert.Same(before, session.Current);

        var result = Accept(session, new FieldCast(Giver, spell, Giver));
        var observation = Assert.Single(result.Observations);
        Assert.Equal(Giver, observation.Target);
        Assert.Equal(cleared, observation.FieldCuredStatus);
        Assert.Equal(cleared == 0 ? "field-no-effect" : "field-status-recovered", observation.Kind);
        var party = session.Current.Exploration!.Party;
        var after = party.Actors.Single(actor => actor.Actor == Giver);
        Assert.Equal(remainingMp, after.Mp);
        Assert.Equal((ushort)0x100, after.Status);
        Assert.Equal(0, after.Hp);
        Assert.Null(after.Progress!.CurrentStats);
        var prior = before.Exploration!.Party;
        Assert.Equal(prior.MainSeed, party.MainSeed);
        Assert.Equal(prior.ThinkingSeed, party.ThinkingSeed);
        Assert.Equal(prior.Actors.Single(actor => actor.Actor == Giver).Exp, after.Exp);
        Assert.Equal(prior.Actors.Single(actor => actor.Actor == Giver).SourceLoadout, after.SourceLoadout);
    }

    [Fact]
    public void HigherFieldDetoxAcceptsAFallenReserveAndRejectsAnUnlearnedRankOrCurse()
    {
        var unlearned = Recovery();
        Assert.DoesNotContain(unlearned.QueryFieldActions(Giver).Spells, choice => choice.Spell == Detox2);
        Assert.NotNull(Send(unlearned, new FieldCast(Giver, Detox2, Recipient)).Failure);

        var cursed = Recovery(higherRanks: true, targetStatus: 6);
        var before = cursed.Current;
        Assert.Equal("detox-curse-unsupported", Send(cursed, new FieldCast(Giver, Detox3, Recipient)).Failure!.Code);
        Assert.Same(before, cursed.Current);

        var fallen = Recovery(higherRanks: true, targetHp: 0, targetStatus: 3);
        var state = fallen.Current;
        var world = state.Exploration!;
        var original = world.Party;
        var party = new BattleStartInput(original.Encounter, original.Actors, original.MainSeed, original.ThinkingSeed,
            original.Gold, activeAllies: [Giver], itemCustody: original.ItemCustody);
        var reserve = new SessionSnapshot(state.SessionId, state.Revision, state.ObservationSequence,
            new ActiveExploration(world.WithParty(party)), state.Story, state.StopReason);
        var result = FieldCommands.Submit(fallen.Definition, reserve, new FieldCast(Giver, Detox2, Recipient));
        Assert.Null(result.Failure);
        var resultParty = result.Snapshot.Exploration!.Party;
        Assert.Equal(Giver, Assert.Single(resultParty.ActiveAllies!));
        var cured = resultParty.Actors.Single(actor => actor.Actor == Recipient);
        Assert.Equal(0, cured.Status);
        Assert.Equal(0, cured.Hp);
    }

    [Theory]
    [InlineData(false, 0)] [InlineData(false, 2)] [InlineData(true, 0)]
    public void OrdinaryFieldHealAndRecoveryItemHaveNoEffectAndNoPayment(bool magic, byte recoveryItem)
    {
        var session = Recovery(recoveryItem: recoveryItem); var before = session.Current;
        var query = session.QueryFieldActions(Giver); Assert.Null(query.Error);
        var fieldHeal = Assert.Single(query.Spells, choice => choice.Spell == Heal).Description!;
        Assert.Equal(SpellEffect.NoEffect, fieldHeal.Effect);
        Assert.Equal((byte)0, fieldHeal.FieldPayment);
        Assert.Same(before, session.Current);
        var result = Accept(session, magic ? new FieldCast(Giver, Heal) : new FieldUseItem(Giver, 0, recoveryItem));
        Assert.Equal("field-no-effect", Assert.Single(result.Observations).Kind);
        Assert.Same(before.Exploration!.Party, session.Current.Exploration!.Party);
        Assert.Same(before.Story, session.Current.Story);
        Assert.Equal(before.Revision + 1, session.Current.Revision);
    }

    [Theory]
    [InlineData(false, 2)] [InlineData(true, 2)] [InlineData(false, 0)] [InlineData(true, 0)]
    public void PoisonCurePaysOnlyOnConfirmationIncludingNoEffect(bool magic, ushort status)
    {
        var session = Recovery(targetStatus: status); var before = session.Current;
        FieldCommand command = magic ? new FieldCast(Giver, Detox, Recipient) : new FieldUseItem(Giver, 1, 3, Recipient);
        Assert.Same(before, session.Current);
        var result = Accept(session, command);
        Assert.Equal(status == 2 ? "field-status-recovered" : "field-no-effect", Assert.Single(result.Observations).Kind);
        var party = session.Current.Exploration!.Party;
        var caster = party.Actors.Single(actor => actor.Actor == Giver);
        var target = party.Actors.Single(actor => actor.Actor == Recipient);
        Assert.Equal((ushort)0, target.Status); Assert.Equal((ushort)100, target.Hp);
        Assert.Equal((byte)(magic ? 4 : 7), caster.Mp);
        Assert.Equal(magic ? new ushort[] { 0, 3, 3, 127 } : new ushort[] { 0, 3, 127, 127 }, caster.SourceLoadout!.Items);
        Assert.Same(caster.SourceLoadout, caster.Progress!.SourceLoadout);
        Assert.Equal(before.Exploration!.Party.MainSeed, party.MainSeed);
        Assert.Equal(before.Exploration.Party.ThinkingSeed, party.ThinkingSeed);
        Assert.Equal(before.Exploration.Party.Actors.Select(actor => actor.Exp), party.Actors.Select(actor => actor.Exp));
        Assert.Equal(before.Exploration.Party.ItemCustody, party.ItemCustody);
    }

    [Theory]
    [InlineData(0x0100, "field-no-effect")]
    [InlineData(0x0101, "field-status-recovered")]
    [InlineData(0x0102, "field-status-recovered")]
    [InlineData(0x0103, "field-status-recovered")]
    public void FairyPowderUsesTheSelectedHolderSlotAndClearsOnlyPoisonAndParalysis(ushort status, string observation)
    {
        var session = Session(carried: actor => actor.Actor == Giver ? actor with
        {
            SourceLoadout = new([3, 5, 3, 127], []),
        } : actor.Actor == Recipient ? actor with { Hp = 15, Mp = 2, Status = status } : actor,
            preparation: new([new(3, "Antidote", false, false), new(5, "Fairy Powder", false, false)],
                [new(3, FieldRecoveryEffect.CurePoison), new(5, FieldRecoveryEffect.CurePoisonAndParalysis)]));
        var before = session.Current;
        var choice = Assert.Single(session.QueryFieldActions(Giver).Items, item => item.Slot == 1);
        Assert.Equal(FieldRecoveryEffect.CurePoisonAndParalysis, choice.Use);
        foreach (var command in new[] { new FieldUseItem(Giver, 1, 5),
            new FieldUseItem(Giver, 1, 5, new("unknown")), new FieldUseItem(Giver, 0, 5, Recipient) })
        {
            Assert.NotNull(Send(session, command).Failure);
            Assert.Same(before, session.Current);
        }

        var result = Accept(session, new FieldUseItem(Giver, choice.Slot, choice.Word, Recipient));
        Assert.Equal(observation, Assert.Single(result.Observations).Kind);
        var party = session.Current.Exploration!.Party;
        var giver = party.Actors.Single(actor => actor.Actor == Giver);
        var target = party.Actors.Single(actor => actor.Actor == Recipient);
        Assert.Equal(new ushort[] { 3, 3, 127, 127 }, giver.SourceLoadout!.Items);
        Assert.Equal(before.Exploration!.Party.Actors.Single(actor => actor.Actor == Recipient) with { Status = 0x0100 },
            target);
        Assert.Equal(before.Exploration.Party.Actors.Single(actor => actor.Actor == Giver).Mp, giver.Mp);
        Assert.Same(before.Exploration.Party.ItemCustody, party.ItemCustody);
        Assert.True(session.Current.CanWaitAtInput);
    }

    [Fact]
    public void SelfCureKeepsBothCostAndStatusAndFullHpIsALegalStatusTarget()
    {
        var session = Recovery(targetHp: 200);
        Accept(session, new FieldCast(Giver, Detox, Recipient));
        Assert.Equal(200, session.Current.Exploration!.Party.Actors.Single(actor => actor.Actor == Recipient).Hp);
        var self = Session(carried: actor => actor.Actor == Giver ? actor with { Status = 2,
            SourceLoadout = new([3, 127, 127, 127], []) } : actor,
            preparation: new([new(3, "Antidote", false, false)], [new(3, FieldRecoveryEffect.CurePoison)]));
        Accept(self, new FieldUseItem(Giver, 0, 3, Giver));
        var cured = self.Current.Exploration!.Party.Actors.Single(actor => actor.Actor == Giver);
        Assert.Equal(0, cured.Status); Assert.All(cured.SourceLoadout!.Items, word => Assert.Equal(127, word));
    }

    [Fact]
    public void IllegalTargetsEmptySlotsAndUnsupportedSpellsNeverPay()
    {
        var session = Recovery(mp: 2, targetHp: 0); var before = session.Current;
        foreach (var command in new FieldCommand[] { new FieldUseItem(Giver, 1, 3, new("missing")),
            new FieldUseItem(Giver, 3, 127), new FieldUseItem(Giver, 0, 3), new FieldCast(Giver, new("late", 4)) })
        {
            Assert.NotNull(Send(session, command).Failure);
            Assert.Same(before, session.Current);
        }
    }

    [Fact]
    public void FieldSelectionAllowsLowMpAndDeadMembersWithoutRevivingOrBorrowingBattleLegality()
    {
        var session = Recovery(mp: 0, targetHp: 0);
        var before = session.Current;
        Accept(session, new FieldCast(Giver, Heal));
        Assert.Same(before.Exploration!.Party, session.Current.Exploration!.Party);
        Accept(session, new FieldCast(Giver, Detox, Recipient));
        var target = session.Current.Exploration!.Party.Actors.Single(actor => actor.Actor == Recipient);
        Assert.Equal(0, target.Hp); Assert.Equal(0, target.Status);
        Assert.Equal(0, session.Current.Exploration.Party.Actors.Single(actor => actor.Actor == Giver).Mp);
        var low = Recovery(mp: 2);
        Accept(low, new FieldCast(Giver, Detox, Recipient));
        Assert.Equal(0, low.Current.Exploration!.Party.Actors.Single(actor => actor.Actor == Giver).Mp);
    }

    [PrivateInputFact("SF2_PRIVATE_EXPLORATION_CONTENT", "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD",
        "SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN")]
    public void RealNewGameFieldEquipmentAndNoEffectUseSurviveDetachedSaveAndRestore()
    {
        string Input(string key) => PrivateInputFactAttribute.RequireInput(key);
        var reader = new OpeningNewGameReader(Input("SF2_PRIVATE_EXPLORATION_CONTENT"), Input("SF2_PRIVATE_BATTLE01_DATA"),
            Input("SF2_PRIVATE_BATTLE01_SCENE"), Input("SF2_PRIVATE_BATTLE01_TERRAIN"), Input("SF2_PRIVATE_STATIC_DATA"),
            Input("SF2_PRIVATE_ENEMY_DATA"), Input("SF2_PRIVATE_ENEMY_GOLD"), 0, uint.MaxValue);
        var started = GameSession.Start(reader);
        Assert.True(started is SessionStarted, (started as SessionStartFailed)?.Failure.ToString());
        var session = Assert.IsType<SessionStarted>(started).Session;
        ActorRef leader = new("ally-0");
        var query = session.QueryFieldActions(leader); Assert.Null(query.Error);
        Assert.True(session.Definition.Encounters.Values.First().Deployments.First(row => row.Actor == leader).Definition.Equipment!.AllowChanges);
        Accept(session, new FieldEquip(leader, 4, null));
        var item = session.QueryFieldActions(leader).Items.First(row => row.Use == FieldRecoveryEffect.NoEffect);
        var beforeUse = session.Current;
        Assert.Contains(Accept(session, new FieldUseItem(leader, item.Slot, item.Word)).Observations, row => row.Kind == "field-no-effect");
        Assert.Same(beforeUse.Exploration!.Party, session.Current.Exploration!.Party);
        var saved = session.CaptureSave(session.Current.SessionId, session.Current.Revision); Assert.Null(saved.Error);
        var restored = Assert.IsType<SessionStarted>(GameSession.Restore(session.Definition, saved.Data!, session.Rules)).Session;
        var loaded = restored.QueryPartyDetails()!.Members.Single(row => row.Carried.Actor == leader);
        Assert.All(loaded.Inventory, slot => Assert.False(slot.Equipped));
        Assert.Equal(loaded.Carried.Progress!.BaseAttack, loaded.Attack!.Value);
        Assert.Equal(beforeUse.Exploration.Party.MainSeed, restored.Current.Exploration!.Party.MainSeed);
        Assert.Equal(beforeUse.Exploration.Party.Actors.Single(row => row.Actor == leader).SourceLoadout!.Items, loaded.Carried.SourceLoadout!.Items);
    }
}
