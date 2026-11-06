using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class ConnectedBattleEquipmentTests
{
    // Public authored operands exercise the production private-to-engine binding, not original facts.
    internal static PrivateBattleDefinitions Source(bool cursed = false) => new(null!, [],
        [new(0, "SDMN", "", 6, "", "", "")],
        [new(50, "TEST_BLADE", "Practice blade", "", "SDMN", ["SDMN"], 1, 2, 0,
            cursed ? "WEAPON|CURSED" : "WEAPON", "NOTHING", "NOTHING", 0, [new("INCREASE_ATT", 5)]),
         new(61, "TEST_SWORD", "Practice sword", "", "PRST", ["PRST"], 1, 1, 0,
            "WEAPON", "NOTHING", "NOTHING", 0, [new("INCREASE_ATT", 10)])], [], [], []);

    private static ScenarioReadAccepted Bound(bool allow = true)
    {
        var admitted = Admitted("battle-equipment");
        var battle = admitted.Definition.Encounters.Values.Single();
        var hero = battle.Deployments.Single(d => d.Actor.Value == "champion");
        // Start with the compatible blade; the other sword is deliberately wrong-class carry.
        battle = battle.WithDeployments(battle.Deployments.Select(d => d.Actor == hero.Actor
            ? d with { Definition = d.Definition.WithGrowth(d.Definition.Growth! with { AttackBonus = 5 })
                .WithEquipment(new(20, d.Definition.Equipment!.Weapons.Values, allowChanges: false)) } : d));
        battle = PrivateFieldPreparation.Equipment(battle, Source(), allowBattleChanges: allow);
        return admitted with { Definition = new(admitted.Definition.Package, [battle]) };
    }

    [Fact]
    public void ExplicitConnectedAdmissionReusesSlotsPreviewCancelAndFreeConfirmation()
    {
        var admitted = Bound();
        var session = Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, admitted.Start)).Session;
        Accept(session, new Confirm());
        var before = session.Current;
        var hero = before.Battle.GetActor(new("champion"));
        Assert.Equal(new[] { 0, 2, 4 }, session.QueryBattleChoices().Equipment.Select(c => c.Slot));
        Assert.Equal("equipment-slot", SendAfterOpen(session, 1).Failure!.Code);
        Accept(session, new SelectEquipment(4));
        Assert.Same(before.Battle, session.Current.Battle);
        Accept(session, new Cancel());
        Assert.Same(before.Battle, session.Current.Battle);
        Accept(session, new BeginEquipment()); Accept(session, new SelectEquipment(4)); Accept(session, new Confirm());
        var after = session.Current;
        Assert.Equal(BattleSelectionStage.ActionChoice, after.Selection!.Stage);
        Assert.Equal(before.Selection!.Preview, after.Selection.Preview);
        Assert.Equal(20, after.Battle.GetActor(hero.Actor).Attack);
        Assert.Equal(new ushort[] { 50, 61, 50, 0 }, after.Battle.GetActor(hero.Actor).SourceLoadout!.Items);
        Assert.Equal(before.Battle.Queue, after.Battle.Queue); Assert.Equal(before.Battle.Cursor, after.Battle.Cursor);
        Assert.Equal(before.Battle.MainSeed, after.Battle.MainSeed); Assert.Equal(before.Battle.ThinkingSeed, after.Battle.ThinkingSeed);
        Assert.Equal(before.Battle.Gold, after.Battle.Gold); Assert.Same(before.Battle.ItemCustody, after.Battle.ItemCustody);
        Assert.Equal(hero.Hp, after.Battle.GetActor(hero.Actor).Hp); Assert.Equal(hero.Mp, after.Battle.GetActor(hero.Actor).Mp);
        Assert.Equal(1, session.QueryBattleChoices().Actions.Single(a => a.Action.Kind == BattleActionKind.Physical).MaximumRange!.Value);
        Accept(session, new BeginEquipment()); Accept(session, new SelectEquipment(2)); Accept(session, new Confirm());
        Assert.Equal(new ushort[] { 50, 61, 178, 0 }, session.Current.Battle.GetActor(hero.Actor).SourceLoadout!.Items);
        Assert.Equal(25, session.Current.Battle.GetActor(hero.Actor).Attack);
        Assert.Equal(2, session.QueryBattleChoices().Actions.Single(a => a.Action.Kind == BattleActionKind.Physical).MaximumRange!.Value);
        Assert.Equal("give-unavailable", Send(session, new BeginGive()).Failure!.Code);
        Assert.Equal("discard-unavailable", Send(session, new BeginDiscard()).Failure!.Code);
    }

    private static SessionResult SendAfterOpen(GameSession session, int slot)
    { Accept(session, new BeginEquipment()); return Send(session, new SelectEquipment(slot)); }

    [Fact]
    public void FieldOnlyAndEnemyBindingsDoNotImplicitlyEnableBattleEquipment()
    {
        var admitted = Bound(false);
        var session = Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, admitted.Start)).Session;
        Accept(session, new Confirm());
        Assert.Equal("equipment-unavailable", Send(session, new BeginEquipment()).Failure!.Code);
        var mutable = Bound();
        Assert.All(mutable.Definition.Encounters.Values.Single().Deployments.Where(d => d.Faction == BattleFaction.Enemy),
            d => Assert.NotEqual(true, d.Definition.Equipment?.AllowChanges));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CursedDefinitionsCannotBecomeOffersOrEquippedState(bool equipped)
    {
        var admitted = Admitted("battle-equipment");
        var b = admitted.Definition.Encounters.Values.Single(); var hero = b.Deployments[0];
        var loadout = new BattleSourceLoadout([equipped ? (ushort)178 : (ushort)50, 127, 127, 127], [63, 63, 63, 63]);
        var definition = hero.Definition.WithEquipment(new(20, PrivateFieldPreparation.Weapons(Source(true))));
        var actor = new BattleActorState(hero with { Definition = definition }, 10, 0, 0, hero.Position, 0, 0,
            sourceLoadout: loadout, attack: 20);
        if (equipped) Assert.Equal("equipment-nonweapon", Assert.Throws<BattleRuleException>(() => BattleEquipment.Choices(actor)).Code);
        else Assert.Empty(BattleEquipment.Choices(actor));
    }
}
