using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.SilenceSpellTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class EquipmentItemTests
{
    private static SessionSnapshot At(ushort[] words, ushort targetStatus = 0, uint seed = 0, uint classMask = uint.MaxValue)
    {
        var basis = Battle(doc => doc["encounters"]![0]!["rewards"]!["halvedExperience"] = true);
        var equipment = new BattleEquipmentDefinition(10, [], rings:
            [new(19, "Attack Ring", classMask, RingStat.Attack, 5, DamageOnUse: true),
             new(20, "Defense Ring", classMask, RingStat.Defense, 5, DamageOnUse: true),
             new(21, "Quick Ring", uint.MaxValue, RingStat.Agility, 5)]);
        var deployments = basis.Definition.Deployments.Select(row => row.Actor == Mage
            ? row with { Definition = row.Definition.WithEquipment(equipment) } : row).ToArray();
        var old = basis.Definition;
        var definition = new BattleDefinition(old.Encounter, old.Map, old.Width, old.Height, old.Terrain,
            deployments, old.Spells.Values, old.Rewards, old.Initialization, old.Outcome, attackSpells: old.AttackSpells.Values,
            supportSpells: old.SupportSpells.Values, supportItems:
            [new(19, "Attack Ring", new(new("attack", 1), 7, 1, 3, 0)),
             new(20, "Defense Ring", new(new("boost", 1), 2, 0, 1, 1))]);
        var original = basis.GetActor(Mage);
        var owner = new BattleActorState(deployments.Single(row => row.Actor == Mage), original.Hp, 0, 0,
            original.Position, 0, 0, sourceLoadout: new(words, [63, 63, 63, 63]), status: 0x200, activationWord: 0);
        var battle = new EngineBattleState(definition, basis.Actors.Select(actor => actor.Actor == Mage ? owner :
            actor.Actor == Priest ? actor.With(status: targetStatus, refreshStats: true) : actor),
            seed, basis.ThinkingSeed, basis.Round, [new(Mage, 100), new(Priest, 60)], 0, 10, basis.StartPolicy, basis.Regions,
            itemCustody: new([4], new Dictionary<byte, byte> { [19] = 1 }));
        return BattleSaveFixture.At(battle, new StoryState([]));
    }

    private static SessionResult Send(SessionSnapshot current, SessionCommand command) => BattleCommandDispatcher.Submit(current, command);
    private static SessionSnapshot Accept(SessionSnapshot current, SessionCommand command)
    {
        var result = Send(current, command);
        Assert.Null(result.Failure);
        return result.Snapshot;
    }

    private static SessionSnapshot Select(SessionSnapshot current, int slot, ActorRef target)
    {
        current = Accept(current, new Confirm());
        current = Accept(current, new SelectItem(slot));
        return Accept(current, new SelectTarget(target));
    }

    [Fact]
    public void AttackItemUsesItsSlotWhileSilencedAndPreservesItsWornBonusOnDamage()
    {
        var start = At([147, 19, 127, 127]);
        var selected = Select(start, 0, Priest);
        var staleBattle = selected.Battle.With(actors: selected.Battle.Actors.Select(actor => actor.Actor == Mage
            ? actor.With(sourceLoadout: new([403, 19, 127, 127], actor.SourceLoadout!.Spells)) : actor));
        var stale = new SessionSnapshot(selected.SessionId, selected.Revision, selected.ObservationSequence,
            new ActiveBattle(staleBattle, selected.Selection), selected.Story, selected.StopReason);
        var rejected = Send(stale, new Confirm());
        Assert.NotNull(rejected.Failure);
        Assert.Same(stale, rejected.Snapshot);
        Assert.Empty(rejected.Observations);
        Assert.Same(start.Battle, Accept(selected, new ReturnToBattleActions()).Battle);

        var result = Send(selected, new Confirm());
        Assert.Null(result.Failure);
        Assert.Equal(new[] { "rng-item-wear", "rng-exp-plus", "rng-exp-minus" },
            result.Observations.Where(row => row.RandomRange.HasValue).Select(row => row.Kind));
        var current = result.Snapshot;
        Assert.Equal(new ushort[] { 403, 19, 127, 127 }, current.Battle.GetActor(Mage).SourceLoadout!.Items);
        Assert.Equal(15, current.Battle.GetActor(Mage).Attack);
        Assert.Equal(10, current.Battle.GetActor(Priest).Attack);
        Assert.Equal(0xC000, current.Battle.GetActor(Priest).Status);
        List<BattleScenePhase> phases = [];
        while (current.BattleScene is { } scene)
        {
            Assert.True(phases.Count < 30);
            phases.Add(scene.Phase);
            current = BattleSceneContinuation.Submit(current, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) :
                new CompletePresentation(scene.Token, scene.CompletionKind)).Snapshot;
        }
        Assert.DoesNotContain(BattleScenePhase.SpellCost, phases);
        Assert.DoesNotContain(BattleScenePhase.SupportDefenseMessage, phases);
        Assert.Contains(BattleScenePhase.EquipmentWearMessage, phases);
        Assert.Equal(0, current.Battle.GetActor(Mage).Mp);
        Assert.Equal(13, current.Battle.GetActor(Priest).Attack);
        Assert.True(current.HasBattleControl);
    }

    [Fact]
    public void BrokenBoostSelfUseRemovesOneCopyAfterConstructionWithoutWearDrawOrDeals()
    {
        var battle = At([404, 20, 19, 127], 0x1000).Battle;
        var owner = battle.GetActor(Mage);
        var resolve = BattleActionRules.Prepare(new Sf2ItemAction(), battle, Mage, owner.Position!,
            new(BattleActionKind.Item, ItemSlot: 0), Mage, new Sf2BattleProgressionRule());
        Assert.Equal(new[] { Mage, Priest }, resolve.Reactions.Select(row => row.Target));
        Assert.Equal(new[] { "rng-boost-recast", "item-destroyed", "rng-exp-plus", "rng-exp-minus" },
            resolve.ConstructionEffects.Select(row => row.Kind));
        Assert.Equal(EquipmentWear.Destroyed, resolve.EquipmentUse!.Wear);
        Assert.Same(battle.ItemCustody, resolve.Prepared.ItemCustody);
        Assert.Equal(new ushort[] { 20, 19, 127, 127 }, resolve.Prepared.GetActor(Mage).SourceLoadout!.Items);
        Assert.Equal(6, resolve.Prepared.GetActor(Mage).Defense); // Removal refreshes constructed self status without old ring.
        var reacted = resolve.Reactions.Aggregate(resolve.Prepared, resolve.ApplyReaction);
        Assert.Equal(6, reacted.GetActor(Mage).Defense);
        Assert.Equal(5, reacted.GetActor(Priest).Defense);
        Assert.Equal((ushort?)0x1000, reacted.GetActor(Priest).LastRefreshBits);
    }

    [Fact]
    public void FailedAttackRecastSpendsOneDrawBeforeWearAndRetainsEffectiveAttackUntilRefresh()
    {
        var battle = At([19, 127, 127, 127], 0x5000).Battle;
        var owner = battle.GetActor(Mage);
        var action = BattleActionRules.Prepare(new Sf2ItemAction(), battle, Mage, owner.Position!,
            new(BattleActionKind.Item, ItemSlot: 0), Priest, new Sf2BattleProgressionRule());
        Assert.Equal(new[] { "rng-attack-recast", "rng-item-wear", "rng-exp-plus", "rng-exp-minus" },
            action.ConstructionEffects.Where(row => row.RandomRange.HasValue).Select(row => row.Kind));
        Assert.Equal(BattleReactionKind.Ineffective, Assert.Single(action.Reactions).Kind);
        var priest = action.Prepared.GetActor(Priest);
        Assert.Equal(0xD000, priest.Status);
        Assert.Equal((ushort?)0x5000, priest.LastRefreshBits);
        Assert.Equal(11, priest.Attack);
        Assert.Equal(13, priest.With(refreshStats: true).Attack);
        Assert.Equal(1, action.Reward!.Amount);
    }

    [Fact]
    public void AttackRejectsSelfOutOfRangeWrongClassAndInertEquipmentWithoutCommitting()
    {
        var start = At([19, 21, 127, 127]);
        var acting = Accept(start, new Confirm());
        var picking = Accept(acting, new SelectItem(0));
        Assert.NotNull(Send(picking, new SelectTarget(Mage)).Failure);
        Assert.NotNull(Send(picking, new SelectTarget(Smoke)).Failure);
        Assert.NotNull(Send(acting, new SelectItem(1)).Failure);
        var wrongClass = Accept(At([19, 127, 127, 127], classMask: 1), new Confirm());
        var result = Send(wrongClass, new SelectItem(0));
        Assert.Equal("equipment-use-class", result.Failure?.Code);
        Assert.Same(wrongClass, result.Snapshot);
        Assert.Empty(result.Observations);
    }

    [Fact]
    public void OwnerTickExpiresSlowAttackAndBoostInOrderThenRestoresAttack()
    {
        var start = At([147, 127, 127, 127]);
        var battle = start.Battle.With(actors: start.Battle.Actors.Select(actor => actor.Actor == Mage
            ? actor.With(status: 0x5400, refreshStats: true) : actor));
        var current = BattleSaveFixture.At(battle, start.Story);
        current = Accept(current, new Confirm());
        current = Accept(current, new ChooseAction(SessionAction.Stay));
        current = Accept(current, new Confirm());
        List<BattleScenePhase> expiries = [];
        while (current.BattleScene is { } scene)
        {
            expiries.Add(scene.Phase);
            Assert.True(expiries.Count < 5);
            var result = BattleSceneContinuation.Submit(current, new CompletePresentation(scene.Token, scene.CompletionKind));
            Assert.Null(result.Failure);
            current = result.Snapshot;
        }
        Assert.Equal(new[] { BattleScenePhase.SlowExpiredMessage, BattleScenePhase.AttackExpiredMessage,
            BattleScenePhase.BoostExpiredMessage }, expiries);
        Assert.Equal(0, current.Battle.GetActor(Mage).Status);
        Assert.Equal(15, current.Battle.GetActor(Mage).Attack);
        Assert.Equal(battle.MainSeed, current.Battle.MainSeed);
    }
}
