using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleDiscardTests
{
    private static readonly ActorRef Champion = new("champion");
    private static EngineBattleState Battle(Action<JsonNode>? change = null) => Start("battle-discard", change).Current.Battle;
    private static EngineBattleState Replace(EngineBattleState battle, BattleActorState actor) =>
        battle.With(actors: battle.Actors.Select(old => old.Actor == actor.Actor ? actor : old));

    [Theory]
    [InlineData(0)] [InlineData(3)]
    public void ExactDuplicateSlotCompactsOnlyFollowingWordsAndLeavesCustodyAlone(int slot)
    {
        var before = Battle();
        var result = BattleDiscard.Prepare(before, Champion, slot, 0).Battle;
        Assert.Equal(slot == 0 ? new ushort[] { 189, 113, 0, 127 } : new ushort[] { 0, 189, 113, 127 },
            result.GetActor(Champion).SourceLoadout!.Items);
        Assert.Same(before.ItemCustody, result.ItemCustody);
        Assert.Same(before.GetActor(new("receiver")), result.GetActor(new("receiver")));
        AssertUnchanged(before, result);
        Assert.Equal(30, result.GetActor(Champion).Attack);
        Assert.Equal(0, before.GetActor(Champion).SourceLoadout!.Items[slot]);
    }

    [Theory]
    [InlineData(0, 1)] [InlineData(14, 15)] [InlineData(15, 15)]
    public void EquippedRareAlwaysRemovesAndRefreshesFromCurrentBaseEvenWhenDealsSaturated(int count, int expected)
    {
        var before = Battle(d => d["start"]!["itemCustody"]!["deals"]![0]!["count"] = count);
        var actor = before.GetActor(Champion);
        var progress = new BattleActorProgress(actor.Level, actor.MaxHp, actor.MaxMp, 43, actor.Defense,
            actor.Agility, actor.Spells, actor.SourceLoadout);
        before = Replace(before, actor.With(progress: progress, attack: 53));
        var result = BattleDiscard.Prepare(before, Champion, 1, 189).Battle;
        var changed = result.GetActor(Champion);
        Assert.Equal(new ushort[] { 0, 113, 0, 127 }, changed.SourceLoadout!.Items);
        Assert.Equal(43, changed.Attack); Assert.Equal(43, changed.BaseAttack);
        Assert.Equal(1, BattleEquipment.Physical(changed)!.MaximumRange);
        Assert.Same(changed.SourceLoadout, changed.Progress!.SourceLoadout);
        Assert.Equal(30, actor.Definition.Attack);
        Assert.Equal(expected, result.ItemCustody!.Deals[61]); Assert.Equal(2, result.ItemCustody.Deals[113]);
        Assert.Equal(before.ItemCustody!.OnceFlags, result.ItemCustody.OnceFlags);
        Assert.Equal(count, before.ItemCustody.Deals[61]); AssertUnchanged(before, result);
    }

    [Fact]
    public void HoleAndCarriedDuplicatePreserveRemainingEquippedContributionAndLaterEquipUseGrowth()
    {
        var before = Battle(d =>
        {
            d["actors"]![0]!["items"] = new JsonArray(127, 61, 189, 0);
            d["growth"] = Document("battle-equipment")["growth"]!.DeepClone();
        });
        Assert.NotNull(before.Definition.Discard); // growth reconstruction preserves opt-in
        Assert.Equal(new[] { 1, 2, 3 }, BattleDiscard.Choices(before, Champion).Select(c => c.Slot));
        var result = BattleDiscard.Prepare(before, Champion, 1, 61).Battle;
        var actor = result.GetActor(Champion);
        Assert.Equal(new ushort[] { 127, 189, 0, 127 }, actor.SourceLoadout!.Items);
        Assert.Equal(30, actor.Attack); Assert.Equal(1, BattleEquipment.EquippedSlot(actor.Definition, actor.SourceLoadout));
        Assert.Equal(2, BattleEquipment.Physical(actor)!.MaximumRange);
        var stats = actor.Definition.Growth!.Stats.ToArray();
        stats[2] = new(20, 40, [new(128, 128), new(256, 128), .. Enumerable.Repeat(new StatGrowthFraction(256, 0), 27)]);
        var definition = actor.Definition.WithGrowth(actor.Definition.Growth with { Stats = stats, AttackBonus = 77 });
        var growing = new BattleActorState(actor.Deployment with { Definition = definition }, 7, 0, 100,
            actor.Position, 0, 0, sourceLoadout: actor.SourceLoadout);
        var grown = BattleProgressionRules.Grow(new Sf2BattleProgressionRule(), growing, 0x1234).Actor;
        Assert.Equal(30, grown.BaseAttack); Assert.Equal(40, grown.Attack);
        Assert.Equal(30, BattleEquipment.Equip(grown, 4).Attack);
        var unarmed = BattleDiscard.Prepare(result, Champion, 1, 189).Battle;
        var later = unarmed.GetActor(Champion);
        Assert.Equal(20, later.Attack); Assert.Equal(1, BattleEquipment.Physical(later)!.MaximumRange);
        Assert.Throws<BattleRuleException>(() => BattleEquipment.Equip(later, 1));
        var used = PlayerItemUse.PrepareSourceDefault(Replace(unarmed, later.With(hp: 100)), Champion,
            later.Position!, 1, Champion);
        Assert.NotNull(used);
        Assert.Throws<BattleRuleException>(() => BattleDiscard.Prepare(result, Champion, 0, 127));
    }

    [Fact]
    public void RemovedWeaponChangesActualPhysicalRangeAndAbsentDealEntryStartsAtOne()
    {
        var before = Battle(d => d["start"]!["itemCustody"]!["deals"]!.AsArray().RemoveAt(0));
        before = before.With(actors: before.Actors.Select(a => a.Actor == new ActorRef("reserve") ? a.With(position: new(1, 4)) : a));
        Assert.NotNull(PhysicalBattleAction.PrepareSourceDefault(before, Champion, new(1, 2), new("reserve")));
        var result = BattleDiscard.Prepare(before, Champion, 1, 189).Battle;
        Assert.Equal(1, result.ItemCustody!.Deals[61]);
        Assert.Throws<BattleRuleException>(() => PhysicalBattleAction.PrepareSourceDefault(result, Champion, new(1, 2), new("reserve")));
    }

    [Fact]
    public void UnsellableRareRefusesBeforeCustodyAndUnknownCustodyNeverBecomesEmpty()
    {
        var before = Battle(d => d["start"]!.AsObject().Remove("itemCustody"));
        Assert.Null(before.ItemCustody);
        Assert.Equal("discard-unsellable", Assert.Throws<BattleRuleException>(() => BattleDiscard.Prepare(before, Champion, 2, 113)).Code);
        Assert.Equal("discard-custody-unavailable", Assert.Throws<BattleRuleException>(() => BattleDiscard.Prepare(before, Champion, 1, 189)).Code);
        Assert.Null(BattleDiscard.Prepare(before, Champion, 0, 0).Battle.ItemCustody);
        Assert.Equal(new ushort[] { 0, 189, 113, 0 }, before.GetActor(Champion).SourceLoadout!.Items);
    }

    [Theory]
    [InlineData("dead")] [InlineData("unplaced")] [InlineData("neutral")] [InlineData("status")]
    [InlineData("automatic")] [InlineData("uninitialized")] [InlineData("broken")]
    [InlineData("unknown")] [InlineData("stats")]
    public void ChangedActorStateRejectsBeforeRemoval(string variation)
    {
        var before = Battle(); var actor = before.GetActor(Champion);
        actor = variation switch
        {
            "dead" => actor.With(hp: 0), "unplaced" => actor.With(clearPosition: true),
            "neutral" => actor.With(activationWord: 8), "status" => actor.With(status: 1),
            "uninitialized" => actor.With(initialized: false),
            "broken" => actor.With(sourceLoadout: new([256, 189, 113, 0], [63, 63, 63, 63])),
            "unknown" => actor.With(sourceLoadout: new([99, 189, 113, 0], [63, 63, 63, 63])),
            "automatic" => new(actor.Deployment with { Control = BattleControl.Automatic }, actor.Hp, actor.Mp,
                actor.Exp, actor.Position, actor.Kills, actor.Defeats),
            _ => actor.With(attack: 31),
        };
        var changed = Replace(before, actor);
        Assert.Throws<BattleRuleException>(() => BattleDiscard.Prepare(changed, Champion, 0, 0));
        Assert.Same(actor.SourceLoadout, changed.GetActor(Champion).SourceLoadout);
        Assert.Same(before.ItemCustody, changed.ItemCustody);
    }

    [Theory]
    [InlineData("rarity")] [InlineData("unsellable")] [InlineData("unknown")]
    [InlineData("duplicate")] [InlineData("broken")] [InlineData("equipped-orb")]
    [InlineData("weapon-orb")] [InlineData("curse")]
    public void UnsupportedMetadataOrWordsFailAdmission(string variation)
    {
        var d = Document("battle-discard"); var rows = d["battleDiscard"]!["items"]!;
        switch (variation)
        {
            case "rarity": rows[1]!["rare"] = false; break;
            case "unsellable": rows[2]!["unsellable"] = false; break;
            case "unknown": rows[2]!["id"] = 99; break;
            case "duplicate": rows.AsArray().Add(rows[0]!.DeepClone()); break;
            case "broken": d["actors"]![0]!["items"]![0] = 256; break;
            case "equipped-orb": d["actors"]![0]!["items"]![2] = 241; break;
            case "weapon-orb": d["weapons"]![0]!["id"] = 113; break;
            case "curse": d["weapons"]![0]!["cursed"] = true; break;
        }
        Assert.IsType<ScenarioReadRejected>(Reader(d).Read());
        Assert.Null(Start("battle-give").Current.Battle.Definition.Discard);
    }

    [Fact]
    public void OverlappingEnemyCatalogSharesRarityAndIndependentOnceFlags()
    {
        var before = Battle(d => d["dropItems"] = new JsonArray(
            new JsonObject { ["id"] = 61, ["name"] = "Enemy sword", ["rare"] = true }));
        Assert.Equal(15, BattleDiscard.Prepare(before, Champion, 1, 189).Battle.ItemCustody!.Deals[61]);
        var d = Document("battle-discard");
        d["dropItems"] = new JsonArray(new JsonObject { ["id"] = 61, ["name"] = "Enemy sword", ["rare"] = false });
        Assert.IsType<ScenarioReadRejected>(Reader(d).Read());
    }

    private static void AssertUnchanged(EngineBattleState before, EngineBattleState after)
    {
        Assert.Same(before.Definition, after.Definition); Assert.Equal(before.Queue, after.Queue);
        Assert.Equal(before.MainSeed, after.MainSeed); Assert.Equal(before.ThinkingSeed, after.ThinkingSeed);
        Assert.Equal(before.Cursor, after.Cursor); Assert.Equal(before.Round, after.Round); Assert.Equal(before.Gold, after.Gold);
        foreach (var old in before.Actors)
        {
            var actor = after.GetActor(old.Actor);
            Assert.Equal(old.Position, actor.Position); Assert.Equal(old.Hp, actor.Hp); Assert.Equal(old.Mp, actor.Mp);
            Assert.Equal(old.Exp, actor.Exp); Assert.Equal(old.Status, actor.Status); Assert.Equal(old.ActivationWord, actor.ActivationWord);
            Assert.Equal(old.SourceLoadout!.Spells, actor.SourceLoadout!.Spells);
            Assert.Equal(old.Kills, actor.Kills); Assert.Equal(old.Defeats, actor.Defeats);
        }
    }
}
