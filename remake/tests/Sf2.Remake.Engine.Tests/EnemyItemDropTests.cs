using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class EnemyItemDropTests
{
    private static readonly ActorRef Ally = new("leader"), Enemy = new("enemy"), Other = new("companion");

    [Theory]
    [InlineData("battle")]
    [InlineData("slot")]
    [InlineData("missing-row")]
    [InlineData("missing-item")]
    [InlineData("not-dead")]
    [InlineData("enemy-actor")]
    [InlineData("ally-target")]
    public void EarlierPredicatesDoNotDrawOrMutate(string fault)
    {
        var battle = Battle(change: d =>
        {
            var binding = d["battle"]!["encounters"]![0]!["itemDrops"]!;
            switch (fault)
            {
                case "battle": binding["battle"] = 17; break;
                case "slot": binding["enemySlots"]![0]!["slot"] = 130; break;
                case "missing-row": binding["rows"]!.AsArray().RemoveAt(0); break;
                case "missing-item": d["battle"]!["actors"]![2]!["items"] = new JsonArray(); break;
            }
        });
        var result = EnemyItemDropRules.Prepare(battle, fault == "enemy-actor" ? Enemy : Ally,
            fault == "ally-target" ? Other : Enemy, fault != "not-dead");
        Assert.Same(battle, result.Battle); Assert.Empty(result.Effects); Assert.Null(result.Receipt);
    }

    [Theory]
    [InlineData(110, true)]
    [InlineData(102, true)]
    [InlineData(68, true)]
    [InlineData(61, false)]
    [InlineData(86, false)]
    public void ChanceIdentitiesAreIndependentOfRarity(int item, bool probabilistic)
    {
        var battle = Battle(item);
        var result = EnemyItemDropRules.Prepare(battle, Ally, Enemy, true);
        Assert.Equal(probabilistic ? BattleRandom.NextMain(battle.MainSeed, 32).After : battle.MainSeed, result.Battle.MainSeed);
        Assert.Equal(probabilistic ? 1 : 0, result.Effects.Count(e => e.Kind == "rng-item-drop"));
        Assert.Equal((ushort)item, result.Battle.GetActor(Ally).SourceLoadout!.Items[0]);
        Assert.Equal(new byte[] { 24 }, result.Battle.ItemCustody!.OnceFlags);
        Assert.NotNull(result.Receipt);
    }

    [Fact]
    public void FailedRollLeavesFlagsSourceAndRecipientUnchanged()
    {
        var battle = Battle().With(mainSeed: 1234u << 16 | 0xABCD);
        var result = EnemyItemDropRules.Prepare(battle, Ally, Enemy, true);
        Assert.True(Assert.Single(result.Effects).RandomValue > 0);
        Assert.Same(battle.ItemCustody, result.Battle.ItemCustody);
        Assert.Same(battle.GetActor(Enemy), result.Battle.GetActor(Enemy));
        Assert.Same(battle.GetActor(Ally), result.Battle.GetActor(Ally));
        Assert.Null(result.Receipt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1234)]
    public void HeldOnceFlagStillConsumesReachedChanceAndNeverRemoves(int seed)
    {
        var battle = Battle(change: d => d["battle"]!["start"]!["itemCustody"]!["onceFlags"]!.AsArray().Add(24))
            .With(mainSeed: (uint)seed << 16 | 0xABCD);
        var result = EnemyItemDropRules.Prepare(battle, Ally, Enemy, true);
        Assert.Equal("rng-item-drop", Assert.Single(result.Effects).Kind);
        Assert.Equal(BattleRandom.NextMain(battle.MainSeed, 32).After, result.Battle.MainSeed);
        Assert.Same(battle.ItemCustody, result.Battle.ItemCustody);
        Assert.Same(battle.GetActor(Enemy), result.Battle.GetActor(Enemy));
        Assert.Null(result.Receipt);
    }

    [Fact]
    public void FirstMatchingHeldSlotIsCompactedAndReceivedStatusIsNormalized()
    {
        var battle = Battle(change: d =>
        {
            d["battle"]!["actors"]![2]!["items"] = new JsonArray(0, 494, 110, 61);
            d["battle"]!["actors"]![0]!["items"] = new JsonArray(0, 383, 61, 127);
        });
        var result = EnemyItemDropRules.Prepare(battle, Ally, Enemy, true);
        Assert.Equal(new ushort[] { 0, 110, 61, 127 }, result.Battle.GetActor(Enemy).SourceLoadout!.Items);
        Assert.Equal(new ushort[] { 0, 110, 61, 127 }, result.Battle.GetActor(Ally).SourceLoadout!.Items);
        Assert.Equal(1L, Assert.Single(result.Effects, e => e.Kind == "enemy-item-removed").After);
        Assert.Equal(new[] { "rng-item-drop", "drop-once-set", "enemy-item-removed", "item-received" }, result.Effects.Select(e => e.Kind));
        Assert.Empty(battle.ItemCustody!.OnceFlags); // Inputs remain immutable.
        Assert.Equal((ushort)494, battle.GetActor(Enemy).SourceLoadout!.Items[1]);
    }

    [Theory]
    [InlineData(110, true, 14, 15)]
    [InlineData(110, true, 15, 15)]
    [InlineData(110, false, 14, 15)]
    [InlineData(86, true, 14, 14)]
    [InlineData(86, false, 14, 14)]
    [InlineData(61, true, 14, 14)]
    public void DeadOrFullRecipientRemovesOnceWithoutSearchingAnotherAlly(int item, bool full, int stock, int expectedStock)
    {
        var battle = Battle(item, d =>
        {
            d["battle"]!["start"]!["itemCustody"]!["deals"]![0]!["count"] = stock;
            if (full) d["battle"]!["actors"]![0]!["items"] = new JsonArray(0, 0, 0, 0);
        });
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == Ally && !full ? a.With(hp: 0) :
            a.Actor == Other ? a.With(hp: 10) : a));
        var result = EnemyItemDropRules.Prepare(battle, Ally, Enemy, true);
        Assert.Null(result.Receipt);
        Assert.All(result.Battle.GetActor(Enemy).SourceLoadout!.Items, word => Assert.Equal(127, word));
        Assert.All(result.Battle.GetActor(Other).SourceLoadout!.Items, word => Assert.Equal(127, word));
        Assert.Equal((byte)expectedStock, result.Battle.ItemCustody!.Deals[110]);
        Assert.Equal(item == 61 ? 4 : 3, result.Battle.ItemCustody.Deals[61]);
        Assert.Contains((byte)24, result.Battle.ItemCustody.OnceFlags);
        Assert.Equal(item == 86 ? "item-discarded" : "item-to-deals", result.Effects[^1].Kind);
    }

    [Fact]
    public void BoundCombatantAndTableOrderWinOverNameDefinitionAndLeader()
    {
        var battle = Battle(61, d =>
        {
            var b = d["battle"]!;
            b["actors"]![2]!["id"] = "renamed-holder";
            b["actors"]![2]!["physical"]!["leader"] = false;
            b["start"]!["actors"]![2]!["actor"] = "renamed-holder";
            b["encounters"]![0]!["placements"]![2]!["actor"] = "renamed-holder";
            b["encounters"]![0]!["leaderCleanup"]!["enemySlots"]![1]!["actor"] = "renamed-holder";
            var drops = b["encounters"]![0]!["itemDrops"]!;
            drops["enemySlots"]![0]!["actor"] = "renamed-holder";
            drops["enemySlots"]![0]!["slot"] = 143; drops["rows"]![0]!["enemySlot"] = 143;
            drops["rows"]![0]!["onceFlag"] = 9;
            // An earlier same-slot row without its held item must not hide the next matching row.
            drops["rows"]!.AsArray().Insert(0, JsonNode.Parse("""{"battle":18,"enemySlot":143,"item":110,"onceFlag":8}"""));
        });
        var result = EnemyItemDropRules.Prepare(battle, Ally, new("renamed-holder"), true);
        Assert.Equal(new byte[] { 9 }, result.Battle.ItemCustody!.OnceFlags);
        Assert.Equal((byte)61, result.Receipt!.Item.ItemId);
        Assert.DoesNotContain(result.Effects, e => e.Kind.StartsWith("rng-"));
    }

    [Theory]
    [InlineData("physical-first")]
    [InlineData("physical-second")]
    [InlineData("physical-counter")]
    public void DropConstructionFollowsTheQualifyingStrikeAndPrecedesRewardAndPlayback(string strike)
    {
        var battle = Battle(110);
        bool counter = strike == "physical-counter";
        // Changed valid HP/ATT select second-hit or allied counter kills without a scenario predicate.
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == Enemy
            ? a.With(hp: strike == "physical-second" ? (ushort)30 : (ushort)1, attack: counter ? (byte)5 : a.Attack) : a));
        BattleActionResolution? found = null; EngineBattleState? input = null;
        for (uint word = 1; word < 65536; word++)
        {
            var candidate = battle.With(mainSeed: word << 16 | 0x1234);
            var actor = counter ? Enemy : Ally; var target = counter ? Ally : Enemy;
            var action = PhysicalBattleAction.PrepareSourceDefault(candidate, actor, candidate.GetActor(actor).Position!, target);
            if (action.Reactions.Any(r => r.Action == strike && r.Receipt is not null)) { found = action; input = candidate; break; }
        }
        Assert.NotNull(found);
        var reaction = Assert.Single(found!.Reactions, r => r.Receipt is not null);
        Assert.Equal(strike, reaction.Action); Assert.Equal(Ally, reaction.Actor);
        Assert.Equal((ushort)110, found.Prepared.GetActor(Ally).SourceLoadout!.Items[0]);
        Assert.Equal(input!.GetActor(Enemy).Hp, found.Prepared.GetActor(Enemy).Hp); // Death is still buffered.
        Assert.Equal(input.GetActor(Ally).Exp, found.Prepared.GetActor(Ally).Exp);
        var kinds = found.ConstructionEffects.Select(e => e.Kind).ToList();
        int mark = kinds.IndexOf("drop-once-set");
        Assert.Equal("rng-item-drop", kinds[mark - 1]); Assert.Equal("hp", kinds[mark - 2]);
        Assert.True(kinds.IndexOf("item-received") < kinds.FindIndex(mark, k => k.StartsWith("rng-exp")));
        found.Validate(input, found.Actor, found.Destination, new(BattleActionKind.Physical), counter ? Ally : Enemy,
            new(1, 1));
        var broken = found with { Prepared = input.With(mainSeed: found.Prepared.MainSeed, gold: found.Prepared.Gold) };
        Assert.Throws<InvalidOperationException>(() => broken.Validate(input, found.Actor, found.Destination,
            new(BattleActionKind.Physical), counter ? Ally : Enemy, new(1, 1)));
    }

    internal static EngineBattleState Battle(int item = 110, Action<JsonNode>? change = null)
    {
        var d = Document("battle-drops");
        var b = d["battle"]!;
        if (item != 110)
        {
            if (!b["dropItems"]!.AsArray().Any(row => row!["id"]!.GetValue<int>() == item))
                b["dropItems"]!.AsArray().Add(new JsonObject { ["id"] = item, ["name"] = "Held item", ["rare"] = item != 86 });
            b["actors"]![2]!["items"] = new JsonArray(item);
            b["encounters"]![0]!["itemDrops"]!["rows"]![0]!["item"] = item;
        }
        change?.Invoke(d);
        var admitted = Assert.IsType<ExplorationReadAccepted>(Reader(d).Read());
        return BattleTurnFlow.Start(admitted.Definition.Encounters[admitted.Start.Party.Encounter], admitted.Start.Party)
            .With(mainSeed: 0x1234);
    }
}
