using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.TestSupport;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class SilenceSpellTests
{
    internal static readonly ActorRef Mage = new("mage"), Priest = new("priest"), Smoke = new("smoke");
    internal static EngineBattleState Battle(Action<JsonNode>? change = null)
    {
        var document = Document("silence-watch"); change?.Invoke(document);
        var read = Reader(document).Read(); Assert.True(read is ScenarioReadAccepted, read.ToString());
        var content = (ScenarioReadAccepted)read;
        return BattleTurnFlow.Start(content.Definition.Encounters.Values.Single(), content.Start);
    }
    internal static EngineBattleState Status(EngineBattleState battle, ActorRef actor, ushort status) =>
        battle.With(actors: battle.Actors.Select(a => a.Actor == actor ? a.With(status: status) : a));
    internal static BattleActionResolution Dispel(EngineBattleState battle, ActorRef? caster = null, ActorRef? target = null)
    {
        var actor = caster ?? Smoke;
        return BattleActionRules.Prepare(new Sf2SupportSpellAction(), battle, actor, battle.GetActor(actor).Position!,
            new(BattleActionKind.SupportSpell, new("dispel", 1)), target ?? Mage, new Sf2BattleProgressionRule());
    }

    [Theory]
    [InlineData(0, true, true)] [InlineData(1, true, true)] [InlineData(2, true, true)]
    [InlineData(3, true, false)] [InlineData(0, false, false)]
    public void DispelUsesStatusResistanceAndLearnedSpellGate(int resistance, bool known, bool success)
    {
        var battle = Battle(d =>
        {
            d["actors"]![0]!["resistance"] = resistance << 14;
            if (!known) d["actors"]![0]!["spells"] = new JsonArray();
        }).With(mainSeed: 0x12340000); // first draw7 of8
        var action = Dispel(battle); var reaction = action.Reactions.Single(r => r.Target == Mage);
        Assert.Equal(success ? BattleReactionKind.Status : BattleReactionKind.Ineffective, reaction.Kind);
        Assert.Equal(success ? (ushort?)0x300 : null, reaction.StatusWrite);
        Assert.Equal(0, action.Prepared.GetActor(Mage).Status); Assert.Equal(5, action.Prepared.GetActor(Smoke).Mp);
        Assert.Null(action.Reward); Assert.Equal(2, action.ConstructionEffects.Count);
        Assert.All(action.Reactions, r => { Assert.Equal(r.HpBefore, r.HpAfter); Assert.Equal(r.MpBefore, r.MpAfter); });
        var paid = action.ApplySpellCost(action.Prepared);
        Assert.Equal(0, paid.GetActor(Smoke).Mp);
        Assert.Equal(success ? 0x300 : 0, action.ApplyReaction(paid, reaction).GetActor(Mage).Status);
    }

    [Theory]
    [InlineData(0)] [InlineData(0x100)] [InlineData(0x200)] [InlineData(0x300)] [InlineData(0x382)]
    public void FreshOrRecastOrMixedStatusRefreshesOnlySilenceAtPlayback(int status)
    {
        var battle = Status(Battle(d =>
        {
            d["battlePoison"] = "fixed-two-after-turn"; d["start"]!["firstAlly"] = "mage";
            d["encounters"]![0]!["outcome"] = new JsonObject { ["leader"] = "priest", ["defeatedGateEnemy"] = "smoke" };
        }), Mage, (ushort)status)
            .With(mainSeed: 0x12340000);
        var action = Dispel(battle); var hit = action.Reactions.Single(r => r.Target == Mage);
        Assert.Equal(status, action.Prepared.GetActor(Mage).Status);
        Assert.Equal(status | 0x300, action.ApplyReaction(action.Prepared, hit).GetActor(Mage).Status);
    }

    [Fact]
    public void FailedRollAndNoEligibleCenterHaveDifferentResourceBoundaries()
    {
        var battle = Battle().With(mainSeed: 0);
        var action = Dispel(battle);
        Assert.All(action.Reactions, r => Assert.Equal(BattleReactionKind.Ineffective, r.Kind));
        Assert.Equal(0, action.ApplySpellCost(action.Prepared).GetActor(Smoke).Mp);
        var rejected = Assert.Throws<BattleRuleException>(() => Dispel(battle, target: Smoke));
        Assert.Equal("support-spell-target", rejected.Code); Assert.Equal(5, battle.GetActor(Smoke).Mp);
        Assert.Equal(0u, battle.MainSeed);
    }

    [Theory]
    [InlineData("blaze", 2)] [InlineData("heal", 3)] [InlineData("dispel", 5)]
    public void SilencedMarkedSpellsAreAdmittedAndPayWithoutConstructingEffects(string id, int cost)
    {
        var battle = Battle(d =>
        {
            if (id == "dispel") d["actors"]![0]!["spells"]!.AsArray().Add(new JsonObject { ["id"] = id, ["level"] = 1 });
        });
        ActorRef caster = id == "heal" ? Priest : Mage;
        battle = Status(battle, caster, 0x300);
        var rules = RuleCompositions.Sf2(); var kind = id == "heal" ? BattleActionKind.Healing : id == "dispel" ? BattleActionKind.SupportSpell : BattleActionKind.AttackSpell;
        var action = BattleActionRules.Prepare(rules.Action(new(kind)), battle, caster, battle.GetActor(caster).Position!,
            new(kind, new(id, 1)), id == "heal" ? Mage : Smoke, rules.Progression);
        Assert.NotNull(action.Silenced); Assert.Empty(action.Reactions); Assert.Empty(action.ConstructionEffects);
        Assert.Null(action.Reward); Assert.Equal(battle.MainSeed, action.Prepared.MainSeed);
        Assert.Equal(20 - cost, action.ApplySpellCost(action.Prepared).GetActor(caster).Mp);
        Assert.Equal((byte?)0, battle.GetActor(caster).Exp); Assert.Equal(80, action.Prepared.GetActor(Smoke).Hp);
    }

    [Fact]
    public void PlayerDispelAwardsFivePerSuccessAndDefinitionCopiesRetainSupport()
    {
        var battle = Battle(d => d["actors"]![0]!["spells"]!.AsArray().Add(new JsonObject { ["id"] = "dispel", ["level"] = 1 })).With(mainSeed: 0x12340000);
        var action = Dispel(battle, Mage, Smoke);
        Assert.Equal(6, action.Reward!.Amount); //5 contribution, plus source16-way award roll0
        var copied = battle.Definition.WithOutcome(new(Mage, Smoke)).WithDeployments(battle.Definition.Deployments);
        Assert.Equal(battle.Definition.SupportSpells, copied.SupportSpells);
        Assert.Equal(5, copied.SupportSpells[new("dispel", 1)].MpCost);
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void PhysicalSilenceCriticalDoesNotAddDamageAndRequiresKnownSpells(bool spells)
    {
        var battle = Battle(d => { if (!spells) d["actors"]![0]!["spells"] = new JsonArray(); });
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == Smoke ? a.With(position: new(4, 2)) : a));
        var action = PhysicalBattleAction.PrepareSourceDefault(battle.With(mainSeed: 388u << 16), Smoke, new(4, 2), Mage);
        var reaction = action.Reactions[0];
        Assert.False(reaction.Critical); Assert.InRange(reaction.Amount, 1, 5);
        Assert.Equal(spells ? (ushort?)0x300 : null, reaction.InflictedSilence);
        Assert.Null(reaction.InflictedSleep); Assert.Equal(0, action.Prepared.GetActor(Mage).Status);
        Assert.Equal(16, battle.GetActor(Smoke).Definition.Physical!.Critical.DoubleChanceDenominator);
        Assert.Equal(16, battle.GetActor(Smoke).Definition.Physical!.Critical.CounterChanceDenominator);
    }

    [Fact]
    public void SilenceDoesNotBlockUnmarkedHerbOrSpecialEgressButBlocksAura()
    {
        var battle = Battle(d =>
        {
            d["spells"]!.AsArray().Add(new JsonObject { ["id"] = "egress", ["level"] = 1, ["mpCost"] = 8,
                ["minimumRange"] = 0, ["maximumRange"] = 0, ["effect"] = new JsonObject { ["kind"] = "retreat" } });
            d["actors"]![0]!["spells"]!.AsArray().Add(new JsonObject { ["id"] = "egress", ["level"] = 1 });
            d["actors"]![0]!["items"] = new JsonArray(0);
            d["start"]!["actors"]![0]!["hp"] = 20;
            d["items"] = new JsonArray(new JsonObject { ["id"] = 0, ["name"] = "Herb", ["effect"] = "consumable-healing",
                ["power"] = 10, ["minimumRange"] = 0, ["maximumRange"] = 1 });
        });
        battle = Status(battle, Mage, 0x300);
        var rules = RuleCompositions.Sf2();
        var item = BattleActionRules.Prepare(new Sf2ItemAction(), battle, Mage, battle.GetActor(Mage).Position!,
            new(BattleActionKind.Item, ItemSlot: 0), Mage, rules.Progression);
        Assert.Null(item.Silenced); Assert.Equal(30, Assert.Single(item.Reactions).HpAfter);
        Assert.Equal(20, item.Prepared.GetActor(Mage).Mp); Assert.Equal((ushort)127, item.Prepared.GetActor(Mage).SourceLoadout!.Items[0]);
        var retreat = BattleActionRules.Prepare(new Sf2RetreatAction(), battle, Mage, battle.GetActor(Mage).Position!,
            new(BattleActionKind.Retreat, new("egress", 1)), Mage, rules.Progression);
        Assert.Null(retreat.Silenced); Assert.Equal(new SpellRetreatPayment(new("egress", 1), 8), retreat.Retreat);
        var aura = Status(AuraHealingTests.Battle(), AuraHealingTests.Caster, 0x300);
        var suppressed = BattleActionRules.Prepare(rules.Action(new(BattleActionKind.Healing)), aura,
            AuraHealingTests.Caster, aura.GetActor(AuraHealingTests.Caster).Position!,
            new(BattleActionKind.Healing, new("aura", 4)), AuraHealingTests.Caster, rules.Progression);
        Assert.NotNull(suppressed.Silenced); Assert.Empty(suppressed.Reactions); Assert.Null(suppressed.Reward);
    }

    [PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN",
        "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD", "SF2_PRIVATE_CONTROLLED_START")]
    public void PinnedDarkSmokeAdmissionComposesProwessDispelAndExtraEntryWithoutBattle07Story()
    {
        var original = Assert.IsType<ScenarioReadAccepted>(PrivateBattleScenarioTests.Selected().Read()).Definition.PrivateDefinitions!.Encounter;
        var placements = original.Placements.Select(p => p.Kind == EncounterEntityKind.Enemy
            ? p with { IdentityExpression = "DARK_SMOKE", AiCommandsetExpression = "SENTRY", ItemExpression = "NOTHING" } : p).ToArray();
        var encounter = new BattleEncounterDefinition(original.PlacementSource, original.TerrainSource, original.Battle,
            original.PlacementRange, original.Area, original.Scene, placements, original.Regions, original.AiPoints,
            original.CompressedRange, original.CompressedSha256, original.TerrainSha256, original.Terrain,
            original.TerrainValueCounts.ToDictionary(p => p.Key, p => p.Value));
        string Input(string key) => PrivateInputFactAttribute.RequireInput(key);
        var start = ControlledBattleStartReader.Read(Input("SF2_PRIVATE_CONTROLLED_START"));
        var definitions = PrivateBattleDefinitionReader.Read(Input("SF2_PRIVATE_STATIC_DATA"), Input("SF2_PRIVATE_ENEMY_DATA"), Input("SF2_PRIVATE_ENEMY_GOLD"), encounter, start);
        var admitted = PrivateBattleScenarioReader.Assemble(definitions, start);
        var battle = BattleTurnFlow.Start(admitted.Definition.Encounters.Values.Single(), admitted.Start);
        var smoke = battle.GetActor(new("enemy-0"));
        Assert.Equal(13, smoke.Agility); Assert.True(smoke.Definition.ExtraRoundAction);
        Assert.Equal(BattleMover.Hovering, smoke.Definition.Mover); Assert.True(smoke.Definition.Physical!.Critical.InflictsSilence);
        Assert.Equal(4, smoke.Definition.Physical.Critical.ChanceDenominator);
        Assert.True(BattleEquipment.OrdinaryAiLoadout(smoke.Definition, smoke.SourceLoadout, true, true));
        var spell = new Sf2SupportSpellAction().RequireAction(battle, smoke.Actor, new(BattleActionKind.SupportSpell, new("dispel", 1))).SupportSpell!;
        Assert.Equal(5, spell.MpCost); Assert.Equal(1, spell.Radius);
    }
}
