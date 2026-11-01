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
    private static GameSession Recovery(byte mp = 7, ushort targetHp = 100, ushort targetStatus = 2)
    {
        var content = new FieldPreparationDefinition([new(0, "Recovery item", false, false), new(3, "Antidote", false, false)],
            [new(0, FieldRecoveryEffect.NoEffect), new(3, FieldRecoveryEffect.CurePoison)],
            [new(Heal, "Heal", 3, FieldRecoveryEffect.NoEffect), new(Detox, "Detox", 3, FieldRecoveryEffect.CurePoison)]);
        var loadout = new BattleSourceLoadout([0, 3, 3, 127], []);
        return Session(carried: actor => actor.Actor == Giver ? actor with { Hp = 50, Mp = mp,
            SourceLoadout = loadout, Progress = new(2, 200, 10, 20, 4, 60, [Heal, Detox], loadout) }
            : actor.Actor == Recipient ? actor with { Hp = targetHp, Status = targetStatus } : actor, preparation: content);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void OrdinaryFieldHealAndRecoveryItemHaveNoEffectAndNoPayment(bool magic)
    {
        var session = Recovery(); var before = session.Current;
        var query = session.QueryFieldActions(Giver); Assert.Null(query.Error);
        Assert.Same(before, session.Current);
        var result = Accept(session, magic ? new FieldCast(Giver, Heal) : new FieldUseItem(Giver, 0, 0));
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
        var session = Assert.IsType<SessionStarted>(GameSession.Start(reader)).Session;
        ActorRef leader = new("ally-0");
        var query = session.QueryFieldActions(leader); Assert.Null(query.Error);
        Assert.False(session.Definition.Encounters.Values.First().Deployments.First(row => row.Actor == leader).Definition.Equipment!.AllowChanges);
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
