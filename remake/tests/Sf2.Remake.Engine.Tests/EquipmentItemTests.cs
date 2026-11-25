using Sf2.Remake.Application.Gameplay;
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
    private static SessionSnapshot At(ushort[] words, ushort targetStatus = 0, uint seed = 0, uint classMask = uint.MaxValue,
        BattleClassRule? staffClass = null, byte casterMp = 0, byte? enemyMp = null, bool halve = true, SupportSpellDefinition? supportEffect = null, AttackSpellDefinition? attackEffect = null)
    {
        var basis = Battle(doc => doc["encounters"]![0]!["rewards"]!["halvedExperience"] = halve);
        var equipment = new BattleEquipmentDefinition(10,
            [new(92, "Indra Staff", 1u << 17 | 1u << 18 | 1u << 19, 25, 1, 1, DamageOnUse: true),
             new(102, "Iron Ball", 0, 44, 1, 1, DamageOnUse: true),
             new(110, "Taros Sword", 0, 32, 1, 2, DamageOnUse: true)], rings:
            [new(19, "Attack Ring", classMask, RingStat.Attack, 5, DamageOnUse: true),
             new(20, "Defense Ring", classMask, RingStat.Defense, 5, DamageOnUse: true),
             new(21, "Quick Ring", uint.MaxValue, RingStat.Agility, 5),
             new(24, "Black Ring", (1u << 17) | (1u << 18), RingStat.Attack, 10, DamageOnUse: true, Cursed: true)]);
        var deployments = basis.Definition.Deployments.Select(row => row.Actor == Mage
            ? row with { Definition = staffClass is { } selected
                ? new(Mage, selected, 5, 30, 20, 10, 5, 100, false, 5, [],
                    physical: row.Definition.Physical! with { Promoted = selected != BattleClassRule.UnpromotedMage },
                    equipment: equipment)
                : row.Definition.WithEquipment(equipment) }
            : row).ToArray();
        var old = basis.Definition;
        var definition = new BattleDefinition(old.Encounter, old.Map, old.Width, old.Height, old.Terrain,
            deployments, old.Spells.Values, old.Rewards, old.Initialization, old.Outcome, attackSpells: old.AttackSpells.Values,
            supportSpells: old.SupportSpells.Values,
            give: new(new Dictionary<byte, string> { [0] = "Herb", [24] = "Black Ring" }),
            discard: new([new(0, "Herb", false, false), new(24, "Black Ring", true, false)]),
            attackItems: [new(24, "Black Ring", attackEffect ?? new(new("blaze", 2), 0, 10, 1, 2, 1, AttackSpellElement.Fire), Breakable: true),
                new(102, "Iron Ball", new(new("blaze",3),10,15,1,2,1,AttackSpellElement.Fire), Breakable: true),
                new(110, "Taros Sword", new(new("bolt",2),15,16,1,3,2,AttackSpellElement.Lightning), Breakable: true)],
            supportItems:
            [new(19, "Attack Ring", supportEffect ?? new(new("attack", 1), 7, 1, 3, 0)),
             new(20, "Defense Ring", new(new("boost", 1), 2, 0, 1, 1)),
             new(92, "Indra Staff", new(new("spoit", 1), 0, 1, 2, 0))]);
        var original = basis.GetActor(Mage);
        var owner = new BattleActorState(deployments.Single(row => row.Actor == Mage), original.Hp, casterMp, 0,
            original.Position, 0, 0, sourceLoadout: new(words, [63, 63, 63, 63]),
            status: BattleEquipment.RefreshCurse(deployments.Single(row => row.Actor == Mage).Definition, new(words, []), 0x200),
            activationWord: 0);
        var battle = new EngineBattleState(definition, basis.Actors.Select(actor => actor.Actor == Mage ? owner :
            actor.Actor == Priest ? actor.With(status: targetStatus, refreshStats: true) :
            actor.Actor == Smoke ? actor.With(mp: enemyMp) : actor),
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

    [Theory]
    [InlineData(5, 19, 220, 5, 20, 476, BattleClassRule.Wizard)]
    [InlineData(2, 0, 92, 2, 2, 348, BattleClassRule.Vicar)]
    [InlineData(0, 20, 476, 0, 20, 92, BattleClassRule.Wizard)]
    public void IndraConstructsOrderedMpReactionsBeforeWearAndReturnsToControl(
        byte enemyMp, byte casterMp, ushort word, int drained, byte gained, ushort firstWord, BattleClassRule staffClass)
    {
        const uint seed = 0x0DA20000; // RNG(3) = 2, next RNG(4) = 0.
        var start = At([word, 92, 127, 127], seed: seed, staffClass: staffClass,
            casterMp: casterMp, enemyMp: enemyMp, halve: false);
        var actionChoice = Accept(start, new Confirm());
        var targetChoice = Accept(actionChoice, new SelectItem(0));
        var invalid = Send(targetChoice, new SelectTarget(Priest));
        Assert.NotNull(invalid.Failure);
        Assert.Same(targetChoice, invalid.Snapshot);
        Assert.Empty(invalid.Observations);
        Assert.Same(start.Battle, Accept(targetChoice, new ReturnToBattleActions()).Battle);
        var result = Send(Accept(targetChoice, new SelectTarget(Smoke)), new Confirm());
        Assert.Null(result.Failure);

        string[] draws = word == 476
            ? ["rng-mp-absorb", "rng-exp-plus", "rng-exp-minus"]
            : ["rng-mp-absorb", "rng-item-wear", "rng-exp-plus", "rng-exp-minus"];
        Assert.Equal(draws, result.Observations.Where(row => row.RandomRange.HasValue).Select(row => row.Kind));
        Assert.Equal((ushort?)3, result.Observations.First(row => row.Kind == "rng-mp-absorb").RandomRange);
        var current = result.Snapshot;
        Assert.Equal(enemyMp, current.Battle.GetActor(Smoke).Mp);
        Assert.Equal(casterMp, current.Battle.GetActor(Mage).Mp);
        Assert.Equal(firstWord, current.Battle.GetActor(Mage).SourceLoadout!.Items[0]);
        Assert.Equal(word == 476 ? 127 : 92, current.Battle.GetActor(Mage).SourceLoadout!.Items[1]);
        Assert.Equal(word == 220 ? 35 : 10, current.Battle.GetActor(Mage).Attack);
        Assert.Same(start.Battle.ItemCustody, current.Battle.ItemCustody);
        var plus = result.Observations.Single(row => row.Kind == "rng-exp-plus").RandomValue;
        var minus = result.Observations.Single(row => row.Kind == "rng-exp-minus").RandomValue;

        List<(ActorRef? Target, int Amount)> reactions = [];
        int steps = 0;
        while (current.BattleScene is { } scene)
        {
            Assert.True(steps++ < 30);
            Assert.NotEqual(BattleScenePhase.SpellCost, scene.Phase);
            if (scene.Phase == BattleScenePhase.Reaction)
            {
                reactions.Add((scene.Target, scene.Amount));
                Assert.Equal(0x200, current.Battle.GetActor(Mage).Status);
            }
            var advanced = BattleSceneContinuation.Submit(current, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) :
                new CompletePresentation(scene.Token, scene.CompletionKind));
            Assert.Null(advanced.Failure);
            current = advanced.Snapshot;
        }
        Assert.Equal(new (ActorRef?, int)[] { (Smoke, drained), (Mage, drained) }, reactions);
        Assert.Equal(enemyMp - drained, current.Battle.GetActor(Smoke).Mp);
        Assert.Equal(gained, current.Battle.GetActor(Mage).Mp);
        Assert.Equal((byte?)Math.Max(1, 5 + (plus == 0 ? 1 : 0) - (minus == 0 ? 1 : 0)), current.Battle.GetActor(Mage).Exp);
        Assert.True(current.HasBattleControl);
    }

    [Fact]
    public void IndraIsCarriedButCannotBeUsedOrEquippedBeforePromotion()
    {
        var start = At([92, 127, 127, 127]);
        var current = Accept(start, new Confirm());
        var fail = Send(current, new SelectItem(0));
        Assert.NotNull(fail.Failure);
        Assert.Same(current, fail.Snapshot);
        Assert.Empty(fail.Observations);
        Assert.DoesNotContain(BattleEquipment.Choices(start.Battle.GetActor(Mage)), choice => choice.Slot == 0);
    }

    [Fact]
    public void BrokenBoostSelfUseRemovesOneCopyAfterConstructionWithoutWearDrawOrDeals()
    {
        var start = At([404, 20, 19, 127], 0x1000);
        var selection = Select(start, 0, Mage);
        var preview = BattleActionFootprint.Query(selection, RuleCompositions.Sf2(), null, false, Mage)!;
        var battle = start.Battle;
        var owner = battle.GetActor(Mage);
        Assert.Equal(5, preview.Area.Count);
        Assert.Contains(battle.GetActor(Priest).Position!, preview.Area);
        var resolve = BattleActionRules.Prepare(new Sf2ItemAction(), battle, Mage, owner.Position!,
            new(BattleActionKind.Item, ItemSlot: 0), Mage, new Sf2BattleProgressionRule());
        Assert.Equal(new[] { Mage, Priest }, resolve.Reactions.Select(row => row.Target));
        Assert.Equal(preview.Recipients.Select(unit => unit.Actor), resolve.Reactions.Select(row => row.Target));
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
    public void BlackRingChecksCurrentClassAndLocksOnlyItsWornSlot()
    {
        var mage = Accept(At([24, 19, 24, 127], staffClass: BattleClassRule.UnpromotedMage), new Confirm());
        Assert.Equal("equipment-use-class", Send(mage, new SelectItem(0)).Failure!.Code);
        Assert.Equal("equipment-slot", Send(Accept(mage, new BeginEquipment()), new SelectEquipment(0)).Failure!.Code);

        var ready = At([24, 19, 24, 220], staffClass: BattleClassRule.Wizard);
        var acting = Accept(ready, new Confirm());
        var preview = Accept(Accept(acting, new BeginEquipment()), new SelectEquipment(0));
        Assert.Equal(35, preview.Battle.GetActor(Mage).Attack);
        var equipped = Accept(preview, new Confirm());
        var wiz = equipped.Battle.GetActor(Mage);
        Assert.Equal(45, wiz.Attack);
        Assert.Equal(0x204, wiz.Status);
        foreach (int slot in new[] { 1, 2, BattleEquipment.RemoveRingChoice })
        {
            var choice = Accept(Accept(equipped, new BeginEquipment()), new SelectEquipment(slot));
            var rejected = Send(choice, new Confirm());
            Assert.Equal("equipped-curse", rejected.Failure!.Code);
            Assert.Same(choice, rejected.Snapshot);
        }
        var unarmed = Accept(Accept(equipped, new BeginEquipment()), new SelectEquipment(BattleEquipment.UnarmedChoice));
        Assert.Equal(0x204, Accept(unarmed, new Confirm()).Battle.GetActor(Mage).Status);
        var fallen = wiz.With(hp: 0);
        var afterHit = equipped.Battle.With(actors: equipped.Battle.Actors.Select(actor => actor.Actor == Mage ? fallen : actor));
        var cleaned = BattleDeathBatch.Empty.Append(wiz, fallen)
            .Clean(afterHit, Mage, new Sf2BattleProgressionRule()).Battle.GetActor(Mage);
        Assert.Null(cleaned.Position);
        Assert.Equal<ushort?>(1, cleaned.Defeats);
        Assert.Equal(4, cleaned.Status);
        Assert.Equal(wiz.Attack, cleaned.Attack);
        Assert.Equal(wiz.SourceLoadout!.Items, cleaned.SourceLoadout!.Items);
    }

    [Fact]
    public void BlackRingTransferChecksTheSelectedWordOnBothSidesOfAnExchange()
    {
        var battle = At([408, 24, 0, 0], staffClass: BattleClassRule.Wizard).Battle;
        var mage = battle.GetActor(Mage);
        var priest = battle.GetActor(Priest).With(sourceLoadout: new([0, 127, 127, 127], [63, 63, 63, 63]));
        battle = battle.With(actors: battle.Actors.Select(actor => actor.Actor == Priest ? priest : actor));

        Assert.Equal("equipped-curse", Assert.Throws<BattleRuleException>(() =>
            BattleGive.Prepare(battle, Mage, mage.Position!, 0, 408, Priest, null, null)).Code);
        Assert.Equal("equipped-curse", Assert.Throws<BattleRuleException>(() =>
            BattleGive.Prepare(battle, Priest, priest.Position!, 0, 0, Mage, 0, 408)).Code);
        Assert.Equal("equipped-curse", Assert.Throws<BattleRuleException>(() =>
            BattleDiscard.Prepare(battle, Mage, 0, 408)).Code);

        var exchanged = BattleGive.Prepare(battle, Priest, priest.Position!, 0, 0, Mage, 1, 24).Battle;
        Assert.Equal(new ushort[] { 408, 0, 0, 0 }, exchanged.GetActor(Mage).SourceLoadout!.Items);
        Assert.Equal(new ushort[] { 24, 127, 127, 127 }, exchanged.GetActor(Priest).SourceLoadout!.Items);
        Assert.Equal(mage.Status, exchanged.GetActor(Mage).Status);
        var discarded = BattleDiscard.Prepare(battle, Mage, 1, 24).Battle;
        Assert.Equal(new ushort[] { 408, 0, 0, 127 }, discarded.GetActor(Mage).SourceLoadout!.Items);
        Assert.Equal(1, discarded.ItemCustody!.Deals[24]);
    }

    [Theory]
    [InlineData(24, 280, 10, 0x200, true)]
    [InlineData(152, 408, 20, 0x204, true)]
    [InlineData(408, 24, 10, 0x200, false)]
    public void BlackRingPaysItsExactSlotAfterBlazeAndKeepsMagicResources(ushort word, ushort nextFirst,
        int attack, int status, bool rollsWear)
    {
        var start = At([word, 24, 0, 127], seed: 0x00020000, staffClass: BattleClassRule.Wizard);
        var selected = Select(start, 0, Smoke);
        var preview = BattleActionFootprint.Query(selected, RuleCompositions.Sf2(), null, false, Smoke)!;
        Assert.Equal(5, preview.Area.Count);
        var result = Send(selected, new Confirm());
        Assert.Null(result.Failure);
        var scene = result.Snapshot.BattleScene!;
        Assert.Equal(new SpellRef("blaze", 2), scene.AttackSpell!.Spell);
        Assert.Equal(10, scene.AttackSpell.Power);
        Assert.Equal(1, scene.AttackSpell.Radius);
        Assert.Equal(preview.Recipients.Select(unit => unit.Actor), scene.AttackTargets);
        var owner = result.Snapshot.Battle.GetActor(Mage);
        Assert.Equal((ushort)nextFirst, owner.SourceLoadout!.Items[0]);
        Assert.Equal((ushort)(rollsWear ? 24 : 0), owner.SourceLoadout.Items[1]);
        Assert.Equal(attack, owner.Attack);
        Assert.Equal(status, owner.Status);
        Assert.Equal(0, owner.Mp);
        Assert.True(owner.Spells.Count == 0);

        var draws = result.Observations.Where(row => row.RandomRange.HasValue).Select(row => row.Kind).ToArray();
        Assert.Equal(new[] { "rng-spell-critical", "rng-spell-variance", "rng-spell-variance" }, draws.Take(3));
        Assert.Equal(rollsWear, draws.Contains("rng-item-wear"));
        if (rollsWear) Assert.Equal("rng-item-wear", draws[3]);
        SessionSnapshot current = result.Snapshot;
        List<BattleScenePhase> phases = [];
        while (current.BattleScene is { } step)
        {
            Assert.True(phases.Count < 30);
            phases.Add(step.Phase);
            var advanced = BattleSceneContinuation.Submit(current, step.RequiresAcknowledgement ? new Acknowledge(step.Token) :
                new CompletePresentation(step.Token, step.CompletionKind));
            Assert.Null(advanced.Failure);
            current = advanced.Snapshot;
        }
        Assert.DoesNotContain(BattleScenePhase.SpellCost, phases);
        Assert.Contains(BattleScenePhase.EquipmentWearMessage, phases);
        Assert.True(current.HasBattleControl);
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
    [Theory]
    [InlineData("dispel", 5, 0u)] [InlineData("dispel", 5, 0x12340000u)]
    [InlineData("muddle", 6, 0u)] [InlineData("muddle", 6, 0x12340000u)]
    public void HostileEquipmentSharesSpellResistanceBeforeOneWear(string name, byte mp, uint seed)
    {
        var effect = new SupportSpellDefinition(new(name, 1), mp, 1, 2, 1);
        var start = At([19,127,127,127], seed: seed, supportEffect: effect);
        var b = start.Battle;
        var magic = Sf2SupportSpellAction.PrepareEffect(b, Mage, b.GetActor(Mage).Position!, effect, Smoke, new Sf2BattleProgressionRule());
        var item = BattleActionRules.Prepare(new Sf2ItemAction(), b, Mage, b.GetActor(Mage).Position!,
            new(BattleActionKind.Item, ItemSlot: 0), Smoke, new Sf2BattleProgressionRule());
        Assert.Equal(magic.Reactions.Select(r => (r.Target,r.Kind,r.StatusWrite)), item.Reactions.Select(r => (r.Target,r.Kind,r.StatusWrite)));
        Assert.Equal(new[]{"rng-"+name,"rng-item-wear","rng-exp-plus","rng-exp-minus"}, item.ConstructionEffects.Select(e => e.Kind).Where(k => k.StartsWith("rng-")));
        var current = Accept(Select(start, 0, Smoke), new Confirm());
        int steps = 0;
        while (current.BattleScene is { } scene)
        {
            Assert.True(steps++ < 40);
            Assert.False(scene.SwitchesAlly);
            Assert.NotEqual(BattleScenePhase.SpellCost, scene.Phase);
            var advanced = BattleSceneContinuation.Submit(current, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind));
            Assert.Null(advanced.Failure); current = advanced.Snapshot;
        }
        Assert.Equal(0, current.Battle.GetActor(Mage).Mp);
    }

    [Theory]
    [InlineData("blaze",2,10,2,1,AttackSpellElement.Fire)]
    [InlineData("blaze",3,15,2,1,AttackSpellElement.Fire)]
    [InlineData("bolt",1,14,2,1,AttackSpellElement.Lightning)]
    [InlineData("bolt",2,16,3,2,AttackSpellElement.Lightning)]
    [InlineData("freeze",3,18,3,1,AttackSpellElement.Ice)]
    [InlineData("desoul",1,0,2,0,AttackSpellElement.Status)]
    public void OffensiveEquipmentUsesExistingEffectWithOneWearAndNoMagicCost(string spell, byte rank, ushort power, byte range, byte radius, AttackSpellElement element)
    {
        var effect = new AttackSpellDefinition(new(spell,rank), spell == "desoul" ? (byte)8 : (byte)0, power, 1, range, radius, element);
        var start = At([24,127,127,127], staffClass: BattleClassRule.Wizard, attackEffect: effect);
        var b = start.Battle;
        var resolution = BattleActionRules.Prepare(new Sf2ItemAction(), b, Mage, b.GetActor(Mage).Position!,
            new(BattleActionKind.Item, ItemSlot: 0), Smoke, new Sf2BattleProgressionRule());
        Assert.Equal(effect, resolution.AttackSpell);
        Assert.Single(resolution.ConstructionEffects, e => e.Kind == "rng-item-wear");
        int wear = resolution.ConstructionEffects.ToList().FindIndex(e => e.Kind == "rng-item-wear");
        Assert.All(resolution.ConstructionEffects.Take(wear), e => Assert.DoesNotContain("exp",e.Kind));
        Assert.All(resolution.ConstructionEffects.Skip(wear+1).Where(e => e.RandomRange.HasValue), e => Assert.StartsWith("rng-exp",e.Kind));
        Assert.Same(resolution.Prepared, resolution.ApplySpellCost(resolution.Prepared));
        Assert.Equal(0, resolution.Prepared.GetActor(Mage).Mp);
    }

    [Theory]
    [InlineData(102)] [InlineData(110)]
    public void MaskNoneEquipmentRefusesPlayerUseBeforeAnyEffect(byte item)
    {
        var start = Accept(At([item,127,127,127]), new Confirm());
        var rejected = Send(start, new SelectItem(0));
        Assert.Equal("equipment-use-class", rejected.Failure!.Code);
        Assert.Same(start, rejected.Snapshot);
        Assert.Empty(rejected.Observations);
    }

}
