using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.AttackSpellTests;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class AttackItemSessionTests
{
    private static readonly AttackItemDefinition Fire = new(16, "Fire supply",
        new(new("fire-supply", 1), 0, 20, 1, 1, 0, AttackSpellElement.Fire));
    private static readonly AttackItemDefinition Ice = new(17, "Ice supply",
        new(new("ice-supply", 1), 0, 33, 1, 1, 0, AttackSpellElement.Ice));

    [Theory]
    [InlineData(false, 26, 20, 4)]
    [InlineData(true, 28, 22, 5)]
    public void PromotedSilencedHolderUsesItemPowerAndEarnsDamageExperience(
        bool ice, int damage, int experience, ushort varianceRange)
    {
        var item = ice ? Ice : Fire;
        var session = Prepare([item.ItemId, 0, item.ItemId, 127]);
        var initial = session.Current.Battle.GetActor(Caster);
        Assert.Empty(initial.Spells);
        Assert.Equal(0, initial.Mp);
        Assert.NotEqual(0, initial.Status & 0x0300);
        Assert.NotNull(initial.Progress!.CurrentStats);

        ReachNeighbor(session);
        var result = Use(session, 2);
        Assert.Equal(item, session.Current.BattleScene!.AttackItem);
        Assert.Equal(new[] { Center }, session.Current.BattleScene.AttackTargets);
        Assert.True(session.Current.BattleScene.Critical);
        Assert.Equal(damage, session.Current.BattleScene.Amount);
        Assert.Equal(experience, session.Current.BattleScene.RewardAmount);
        Assert.Equal(new ushort?[] { 32, varianceRange, varianceRange, 16, 16 },
            ConstructionRolls(result).Select(row => row.RandomRange));
        var paid = session.Current.Battle.GetActor(Caster);
        Assert.Equal(new ushort[] { item.ItemId, 0, 127, 127 }, paid.SourceLoadout!.Items);
        Assert.Null(paid.Progress!.CurrentStats);
        Assert.Equal(5, paid.Move);
        Assert.Equal(initial.Status, paid.Status);
        Assert.Equal(50, session.Current.Battle.GetActor(Center).Hp);

        var finished = FinishBattleScenes(session, result);
        Assert.Equal(50 - damage, session.Current.Battle.GetActor(Center).Hp);
        Assert.Equal((byte?)experience, session.Current.Battle.GetActor(Caster).Exp);
        Assert.Equal(0, session.Current.Battle.GetActor(Caster).Mp);
        Assert.Single(finished.Observations, e => e.Kind == "item-consumed" && e.Before == item.ItemId && e.After == 2);
        Assert.DoesNotContain(finished.Observations, e => e.Kind is "mp" or "silenced-cast");
        Assert.Equal(new ActorRef("guard"), session.Current.Selection!.Actor);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LethalUseChecksDropCapacityBeforeCompactingSelectedSlot(bool full)
    {
        var session = Prepare([16, 0, 16, full ? (ushort)0 : (ushort)127], lethal: true);
        ReachNeighbor(session);
        var result = Use(session, 2);
        var battle = session.Current.Battle;
        var custody = battle.ItemCustody!;
        Assert.Contains((byte)9, custody.OnceFlags);
        Assert.Equal(full ? 1 : 0, custody.Deals.GetValueOrDefault((byte)17));
        Assert.Equal(full ? new ushort[] { 16, 0, 0, 127 } : new ushort[] { 16, 0, 17, 127 },
            battle.GetActor(Caster).SourceLoadout!.Items);
        Assert.Equal(new ushort[] { 127, 127, 127, 127 }, battle.GetActor(Center).SourceLoadout!.Items);
        var effects = result.Observations.Select(row => row.Kind).ToList();
        int drop = effects.IndexOf(full ? "item-to-deals" : "item-received");
        Assert.InRange(drop, 0, effects.IndexOf("item-consumed") - 1);
        var finished = FinishBattleScenes(session, result);
        Assert.Equal(0, session.Current.Battle.GetActor(Center).Hp);
        Assert.Equal((ushort?)1, session.Current.Battle.GetActor(Caster).Kills);
        Assert.Equal(107u, session.Current.Battle.Gold);
        Assert.Single(finished.Observations, e => e.Kind == "item-consumed");
        Assert.True(session.Current.HasBattleControl);
    }

    [Fact]
    public void CancellationInvalidTargetsAndChangedFullWordKeepTheLiveBattle()
    {
        var session = Prepare([16, 16, 127, 127]);
        Assert.False(Assert.Single(session.QueryBattleChoices().Items, row => row.Slot == 0).Enabled);
        ReachNeighbor(session);
        var initial = session.Current.Battle;
        Accept(session, new SelectItem(1));
        foreach (var target in new[] { Caster, Wing, new ActorRef("guard") })
        {
            var before = session.Current;
            Assert.NotNull(Send(session, new SelectTarget(target)).Failure);
            Assert.Same(before, session.Current);
        }
        Accept(session, new ReturnToBattleActions());
        Assert.Same(initial, session.Current.Battle);
        Accept(session, new SelectItem(1));
        Accept(session, new SelectTarget(Center));
        var selected = session.Current;
        var holder = selected.Battle.GetActor(Caster);
        var changed = holder.With(sourceLoadout: new([16, 272, 127, 127], holder.SourceLoadout!.Spells));
        var changedBattle = selected.Battle.With(actors: selected.Battle.Actors.Select(a => a.Actor == Caster ? changed : a));
        var stale = new SessionSnapshot(selected.SessionId, selected.Revision, selected.ObservationSequence,
            new ActiveBattle(changedBattle, selected.Selection), selected.Story, selected.StopReason);
        var rejected = BattleCommandDispatcher.Submit(stale, new Confirm());
        Assert.Equal("stale-item-selection", rejected.Failure!.Code);
        Assert.Same(stale, rejected.Snapshot);
        Assert.Empty(rejected.Observations);
    }

    [Fact]
    public void RemovingMovementGearDoesNotInvalidateTheConfirmedItemStop()
    {
        var session = Prepare([150, 16, 127, 127], ring: true);
        Assert.Equal(3, session.Current.Battle.GetActor(Caster).Move);
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.East)));
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.South)));
        Accept(session, new Confirm());
        Accept(session, new BeginEquipment());
        Accept(session, new SelectEquipment(BattleEquipment.RemoveRingChoice));
        Accept(session, new Confirm());
        Assert.Equal(1, session.Current.Battle.GetActor(Caster).Move);
        Assert.Equal(new MapPosition(3, 3), session.Current.Selection!.Preview.Destination);
        Assert.True(Assert.Single(session.QueryBattleChoices().Items, row => row.Slot == 1).Enabled);
        Accept(session, new SelectItem(1));
        Accept(session, new SelectTarget(Wing));
        var result = Accept(session, new Confirm());
        Assert.Contains(result.Observations, e => e.Kind == "item-consumed");
        Assert.Equal(new MapPosition(3, 3), session.Current.Battle.GetActor(Caster).Position);
        Assert.Equal(new ushort[] { 22, 127, 127, 127 }, session.Current.Battle.GetActor(Caster).SourceLoadout!.Items);
    }

    [Fact]
    public void MainBackAfterRemovingMovementGearUsesReducedBudgetFromTheOriginalOrigin()
    {
        var session = Prepare([150, 16, 127, 127], ring: true);
        var initial = session.Current.Battle;
        var origin = initial.GetActor(Caster).Position!;
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.East)));
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.South)));
        Accept(session, new Confirm());
        Accept(session, new BeginEquipment());
        Accept(session, new SelectEquipment(BattleEquipment.RemoveRingChoice));
        Accept(session, new Confirm());

        var preview = session.Current.Selection!.Preview;
        Assert.True(preview.Cost > session.Current.Battle.GetActor(Caster).Move * 2);
        var back = Accept(session, new Cancel());
        var movement = Assert.IsType<BattleMovementState>(back.Snapshot.BattleMovement);
        Assert.Equal(BattleMovementPurpose.Return, movement.Purpose);
        Assert.Equal(preview.Path.Reverse(), movement.Path);
        var returned = FinishMovement(session, back).Snapshot;
        var range = session.QueryBattleMovementRange()!;
        Assert.Equal(origin, range.Origin);
        Assert.Equal(origin, returned.Selection!.Preview.Destination);
        Assert.Equal(origin, returned.Battle.GetActor(Caster).Position);
        Assert.Equal(new ushort[] { 22, 16, 127, 127 }, returned.Battle.GetActor(Caster).SourceLoadout!.Items);
        Assert.Equal(initial.MainSeed, returned.Battle.MainSeed);
        Assert.Equal(initial.ThinkingSeed, returned.Battle.ThinkingSeed);
        Assert.DoesNotContain(preview.Destination, range.ReachableCells);
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.East)));
        Assert.Equal("movement-range", Send(session, new Move(ExplorationDirection.South)).Failure!.Code);
    }

    private static GameSession Prepare(ushort[] words, bool lethal = false, bool ring = false)
    {
        var admitted = Admitted("attack-spells");
        var basis = admitted.Definition.Encounters.Values.Single();
        var pack = new BattleSourceLoadout(words, [63, 63, 63, 63]);
        var enemyPack = new BattleSourceLoadout([17, 127, 127, 127], [63, 63, 63, 63]);
        var empty = new BattleSourceLoadout([127, 127, 127, 127], [63, 63, 63, 63]);
        var growth = new BattleGrowthDefinition(12, 0, Enumerable.Repeat(new StatGrowth(1, 1, []), 5).ToArray(),
            [], new Dictionary<byte, SpellRef>());
        var deployments = basis.Deployments.Select(row => row.Actor == Caster
            ? row with { Definition = new(Caster, BattleClassRule.Hero, 5, 30, 20, 10, 5, 80,
                false, ring ? (byte)1 : (byte)5, [], row.Definition.Physical! with { Promoted = true },
                sourceLoadout: pack, growth: growth, equipment: ring ? new(10, [], inertItems: [16],
                    rings: [new(22, "Movement ring", uint.MaxValue, RingStat.Move, 2)]) : null) }
            : row.Actor == Center
                ? row with { Definition = new(Center, BattleClassRule.Ordinary, lethal ? (byte)25 : (byte)22,
                    50, 0, 10, 5, 20, false, 5, [], row.Definition.Physical, sourceLoadout: lethal ? enemyPack : null,
                    equipment: lethal ? new(10, [], allowChanges: false, inertItems: [17]) : null, resistance: 192 | 16) }
                : row);
        var drops = lethal ? new EnemyItemDropDefinition(1, new Dictionary<ActorRef, byte> { [Center] = 128 },
            [new(1, 128, 17, 9)], [new(17, "Ice supply", Rare: true)]) : null;
        var encounter = new BattleDefinition(basis.Encounter, basis.Map, basis.Width, basis.Height,
            basis.Terrain, deployments, [], basis.Rewards, itemDrops: drops, attackItems: [Fire, Ice]);
        var start = admitted.Start;
        var inputs = start.Actors.Select(row => row.Actor == Center ? row with { Hp = lethal ? (ushort)4 : (ushort)50 } :
            row.Actor == Caster ? row with { Mp = 0, Status = 0x200, Progress = new(5, 30, 20, 10, 5, 80, [], pack, 12)
                { CurrentStats = ring ? null : new(8, null, 7) } } : lethal ? row with { SourceLoadout = empty } : row);
        var started = GameSession.Start(new ScenarioDefinition("item-actions", [encounter]),
            new BattleStartInput(start.Encounter, inputs, 62u << 16, start.ThinkingSeed, start.Gold,
                itemCustody: drops is null ? null : new([], new Dictionary<byte, byte>())));
        Assert.False(started is SessionStartFailed, (started as SessionStartFailed)?.Failure.ToString());
        return Assert.IsType<SessionStarted>(started).Session;
    }

    private static void ReachNeighbor(GameSession session)
    {
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.East)));
        Accept(session, new Confirm());
    }

    private static SessionResult Use(GameSession session, int slot)
    {
        Accept(session, new SelectItem(slot));
        Accept(session, new SelectTarget(Center));
        return Accept(session, new Confirm());
    }
}
