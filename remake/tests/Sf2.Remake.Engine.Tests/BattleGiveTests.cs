using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleGiveTests
{
    private static readonly ActorRef Giver = new("champion"), Recipient = new("receiver");
    private static EngineBattleState Battle(Action<JsonNode>? change = null) => Start("battle-give", change).Current.Battle;
    private static EngineBattleState Replace(EngineBattleState battle, BattleActorState actor) =>
        battle.With(actors: battle.Actors.Select(old => old.Actor == actor.Actor ? actor : old));

    [Theory]
    [InlineData(0, 178, 20)] [InlineData(2, 50, 25)]
    public void ExactDuplicateSlotCompactsAndFirstEmptyReceiptNeverEquips(int slot, int word, int attack)
    {
        var before = Battle(d => d["actors"]![1]!["items"] = new JsonArray(127, 189, 127, 0));
        var result = BattleGive.Prepare(before, Giver, new(1, 2), slot, (ushort)word, Recipient, null, null).Battle;
        Assert.Equal(slot == 0 ? new ushort[] { 0, 50, 127, 127 } : new ushort[] { 178, 0, 127, 127 }, result.GetActor(Giver).SourceLoadout!.Items);
        Assert.Equal(new ushort[] { 50, 189, 127, 0 }, result.GetActor(Recipient).SourceLoadout!.Items);
        Assert.Equal(attack, result.GetActor(Giver).Attack); Assert.Equal(25, result.GetActor(Recipient).Attack);
        Assert.Equal(slot == 0 ? 1 : 2, BattleEquipment.Physical(result.GetActor(Giver))!.MaximumRange);
        Assert.Same(before.GetActor(new("reserve")), result.GetActor(new("reserve")));
        Assert.Same(before.Definition, result.Definition); Assert.Same(before.ItemCustody, result.ItemCustody);
        Assert.Equal(before.MainSeed, result.MainSeed); Assert.Equal(before.ThinkingSeed, result.ThinkingSeed);
        Assert.Equal(before.Gold, result.Gold); Assert.Equal(before.Cursor, result.Cursor);
        foreach (var old in before.Actors)
        {
            var next = result.GetActor(old.Actor);
            Assert.Equal(old.Hp, next.Hp); Assert.Equal(old.Mp, next.Mp); Assert.Equal(old.Exp, next.Exp);
            Assert.Equal(old.Status, next.Status); Assert.Equal(old.SourceLoadout!.Spells, next.SourceLoadout!.Spells);
        }
    }

    [Fact]
    public void FullExchangeRemovesBothBeforeAppendingSavedWordsAndRefreshesCurrentBase()
    {
        var before = Battle(d => d["actors"]![1]!["items"] = new JsonArray(0, 189, 0, 50));
        var giver = before.GetActor(Giver); var recipient = before.GetActor(Recipient);
        var progress = new BattleActorProgress(giver.Level, giver.MaxHp, giver.MaxMp, 43, giver.Defense,
            giver.Agility, giver.Spells, giver.SourceLoadout);
        before = Replace(before, giver.With(progress: progress, attack: 48));
        var result = BattleGive.Prepare(before, Giver, new(1, 2), 0, 178, Recipient, 1, 189).Battle;
        var changed = result.GetActor(Giver);
        Assert.Equal(new ushort[] { 0, 50, 61, 127 }, changed.SourceLoadout!.Items);
        Assert.Equal(new ushort[] { 0, 0, 50, 50 }, result.GetActor(Recipient).SourceLoadout!.Items);
        Assert.Equal(43, changed.Attack); Assert.Equal(15, result.GetActor(Recipient).Attack);
        Assert.Equal(43, changed.BaseAttack); Assert.Same(changed.SourceLoadout, changed.Progress!.SourceLoadout);
        Assert.Equal(53, BattleEquipment.Equip(changed, 2).Attack);
        Assert.Equal(20, BattleEquipment.Equip(result.GetActor(Recipient), 3).Attack);
        Assert.Equal(25, giver.Definition.Attack); Assert.Equal(25, recipient.Definition.Attack);
        Assert.Equal(before.Actors.SelectMany(a => a.SourceLoadout!.Items).Where(w => w != 127).Select(w => w & 127).Order(),
            result.Actors.SelectMany(a => a.SourceLoadout!.Items).Where(w => w != 127).Select(w => w & 127).Order());
    }

    [Theory]
    [InlineData(1, 1, true)] [InlineData(1, 3, true)] [InlineData(2, 2, true)]
    [InlineData(1, 2, false)] [InlineData(2, 3, false)] [InlineData(3, 2, false)]
    public void SourceCardinalGeometryUsesProvisionalDestination(int x, int y, bool allowed)
    {
        var battle = Battle(); var recipient = battle.GetActor(Recipient).With(position: new(x, y));
        battle = Replace(battle, recipient);
        if (allowed) Assert.Same(recipient, BattleGive.RequireTarget(battle, Giver, new(1, 2), Recipient));
        else Assert.Throws<BattleRuleException>(() => BattleGive.RequireTarget(battle, Giver, new(1, 2), Recipient));
        Assert.Throws<BattleRuleException>(() => BattleGive.RequireTarget(battle, Giver, new(1, 2), Giver));
        Assert.Throws<BattleRuleException>(() => BattleGive.RequireTarget(battle, Giver, new(1, 2), new("missing")));
    }

    [Theory]
    [InlineData("dead")] [InlineData("unplaced")] [InlineData("neutral")] [InlineData("status")]
    [InlineData("broken")] [InlineData("unknown")] [InlineData("stats")]
    public void UnadmittedLiveRecipientNeverMutates(string change)
    {
        var battle = Battle(); var target = battle.GetActor(Recipient);
        target = change switch
        {
            "dead" => target.With(hp: 0), "unplaced" => target.With(clearPosition: true),
            "neutral" => target.With(activationWord: 8), "status" => target.With(status: 1),
            "broken" => target.With(sourceLoadout: new([189, 256, 0, 127], [63, 63, 63, 63])),
            "unknown" => target.With(sourceLoadout: new([189, 90, 0, 127], [63, 63, 63, 63])),
            _ => target.With(attack: 26),
        };
        battle = Replace(battle, target);
        Assert.Throws<BattleRuleException>(() => BattleGive.Prepare(battle, Giver, new(1, 2), 0, 178, Recipient, null, null));
        Assert.Equal(new ushort[] { 178, 0, 50, 127 }, battle.GetActor(Giver).SourceLoadout!.Items);
        Assert.Throws<BattleRuleException>(() => BattleGive.RequireTarget(battle, Giver, new(6, 5), new("reserve")));
    }

    [Fact]
    public void CarryDoesNotImplyUseOrClassEquipAndCurrentGrowthUsesReceivedLoadout()
    {
        var battle = Battle(d =>
        {
            d["actors"]![1]!["classRule"] = "unpromoted-warrior";
            d["actors"]![1]!["items"] = new JsonArray(0); d["actors"]![1]!["attack"] = 15;
            d["growth"] = Document("battle-equipment")["growth"]!.DeepClone();
        });
        var transferred = BattleGive.Prepare(battle, Giver, new(1, 2), 0, 178, Recipient, null, null).Battle;
        Assert.Equal(50, transferred.GetActor(Recipient).SourceLoadout!.Items[1]);
        Assert.Empty(BattleEquipment.Choices(transferred.GetActor(Recipient)));
        Assert.Throws<BattleRuleException>(() => BattleEquipment.Equip(transferred.GetActor(Recipient), 1));
        Assert.Throws<BattleRuleException>(() => PlayerItemUse.PrepareSourceDefault(transferred, Recipient, new(2, 2), 1, Recipient));
        var grown = BattleProgressionRules.Grow(new Sf2BattleProgressionRule(), transferred.GetActor(Giver).With(exp: 100), 0x1234).Actor;
        Assert.Equal(20, grown.Attack); Assert.Equal(4, BattleEquipment.EquippedSlot(grown.Definition, grown.SourceLoadout));
        Assert.Equal(new ushort[] { 0, 50, 127, 127 }, grown.SourceLoadout!.Items);
    }

    [Theory]
    [InlineData("missing-equipment")] [InlineData("broken")] [InlineData("false")]
    [InlineData("equipped-herb")] [InlineData("curse")] [InlineData("ring")]
    public void OptInRejectsUnsupportedItemContracts(string change)
    {
        var doc = Document("battle-give");
        switch (change)
        {
            case "missing-equipment": doc["actors"]![1]!.AsObject().Remove("equipment"); break;
            case "broken": doc["actors"]![1]!["items"] = new JsonArray(189, 256); break;
            case "false": doc["battleGive"] = false; break;
            case "equipped-herb": doc["actors"]![1]!["items"] = new JsonArray(128); break;
            case "curse": doc["weapons"]![0]!["cursed"] = true; break;
            case "ring": doc["weapons"]![0]!["type"] = "ring"; break;
        }
        Assert.IsType<ScenarioReadRejected>(Reader(doc).Read());
        Assert.Null(Start("battle-equipment").Current.Battle.Definition.Give);
    }
}
