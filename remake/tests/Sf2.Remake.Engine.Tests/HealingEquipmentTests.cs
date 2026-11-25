using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.AuraHealingTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class HealingEquipmentTests
{
    private static SessionSnapshot At(string spell = "aura", ushort word = 23, uint mask = uint.MaxValue, ushort status = 0, uint seed = 0)
    {
        var basis = Battle();
        var equipment = new BattleEquipmentDefinition(10, [], rings: [new(23, "Healing gear", mask, RingStat.Defense, 10, DamageOnUse: true)]);
        var rows = basis.Definition.Deployments.Select(row => row.Actor == Caster ? row with {
            Definition = new(Caster, BattleClassRule.Vicar, 5, 100, 80, 10, 5, 100, false, 5, [], equipment: equipment)
        } : row).ToArray();
        HealingSpellDefinition effect = spell switch {
            "aura" => new(new("aura", 2), 11, 15, 0, 3, HealingTargets.AuraRadius, 2),
            "detox" => new(new("detox", 2), 6, 0, 0, 2, CuresPoison: true),
            _ => new(new("healin", 1), 0, 10, 0, 1),
        };
        var old = basis.Definition;
        var definition = new BattleDefinition(old.Encounter, old.Map, old.Width, old.Height, old.Terrain,
            rows, [], new(true), old.Initialization, healingItems:
            [new(23, "Healing gear", effect.Power, effect.MinimumRange, effect.MaximumRange, effect.CuresPoison, EquipmentEffect: effect)], poison: true);
        var owner = basis.GetActor(Caster);
        var caster = new BattleActorState(rows.Single(row => row.Actor == Caster), owner.Hp, 0, 0, owner.Position, 0, 0,
            sourceLoadout: new([23, 0, word, 127], [63, 63, 63, 63]), status: 0x200, activationWord: 0);
        var battle = new EngineBattleState(definition, basis.Actors.Select(a => a.Actor == Caster ? caster :
            a.Actor == Center ? a.With(status: status) : a), seed, basis.ThinkingSeed, basis.Round,
            [new(Caster, 100), new(Center, 60)], 0, 10, basis.StartPolicy, basis.Regions);
        return BattleSaveFixture.At(battle, new StoryState([]));
    }
    private static SessionSnapshot Accept(SessionSnapshot current, SessionCommand command)
    {
        var result = BattleCommandDispatcher.Submit(current, command);
        Assert.Null(result.Failure);
        return result.Snapshot;
    }
    private static SessionSnapshot Select(SessionSnapshot current, ActorRef target)
    {
        current = Accept(current, new Confirm());
        current = Accept(current, new SelectItem(2));
        return Accept(current, new SelectTarget(target));
    }
    private static SessionSnapshot Finish(SessionSnapshot current, List<BattleScenePhase> phases)
    {
        while (current.BattleScene is { } scene)
        {
            Assert.True(phases.Count < 100);
            phases.Add(scene.Phase);
            var result = BattleSceneContinuation.Submit(current, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) :
                new CompletePresentation(scene.Token, scene.CompletionKind));
            Assert.Null(result.Failure);
            current = result.Snapshot;
        }
        return current;
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void AuraUsesRawPowerAllTargetsAndOneSelectedSlotWithoutMpOrBook(bool broken)
    {
        ushort word = (ushort)(23 | 128 | (broken ? BattleInventory.Broken : 0));
        var start = At(word: word);
        var selected = Select(start, Center);
        Assert.Same(start.Battle, Accept(selected, new ReturnToBattleActions()).Battle);
        var result = BattleCommandDispatcher.Submit(selected, new Confirm());
        Assert.Null(result.Failure);
        Assert.Equal(broken ? new[] { "rng-exp-plus", "rng-exp-minus" } : new[] { "rng-item-wear", "rng-exp-plus", "rng-exp-minus" },
            result.Observations.Where(e => e.RandomRange.HasValue).Select(e => e.Kind));
        Assert.Equal(broken ? new ushort[] {23,0,127,127} : new ushort[] {23,0,407,127}, result.Snapshot.Battle.GetActor(Caster).SourceLoadout!.Items);
        Assert.DoesNotContain(result.Observations, e => e.Kind == "item-consumed");
        Assert.Equal(new[] { "a-edge", "b-center", "m-full", "z-priest" }, result.Snapshot.BattleScene!.HealingTargets.Select(a => a.Value));
        List<BattleScenePhase> phases = [];
        var done = Finish(result.Snapshot, phases);
        Assert.Equal(16, done.Battle.GetActor(new("a-edge")).Hp);
        Assert.Equal(start.Battle.GetActor(Caster).Hp + 15, done.Battle.GetActor(Caster).Hp);
        Assert.Equal(0, done.Battle.GetActor(Caster).Mp);
        Assert.Empty(done.Battle.GetActor(Caster).Spells);
        Assert.DoesNotContain(BattleScenePhase.SpellCost, phases);
        Assert.Single(phases, phase => phase == BattleScenePhase.EquipmentWearMessage);
    }
    [Theory]
    [InlineData(0, false)] [InlineData(3, true)]
    public void DetoxUsesBothStatusBitsAndIneffectiveDrawPrecedesWear(ushort status, bool cured)
    {
        var start = At("detox", status: status);
        var result = BattleCommandDispatcher.Submit(Select(start, Center), new Confirm());
        Assert.Null(result.Failure);
        Assert.Equal(cured ? new[] {"rng-item-wear","rng-exp-plus","rng-exp-minus"} :
            new[] {"rng-cure-ineffective","rng-item-wear","rng-exp-plus","rng-exp-minus"},
            result.Observations.Where(e => e.RandomRange.HasValue).Select(e => e.Kind));
        List<BattleScenePhase> phases = [];
        var done = Finish(result.Snapshot, phases);
        Assert.Equal(0, done.Battle.GetActor(Center).Status);
        Assert.Equal(cured, phases.Contains(BattleScenePhase.CureStunMessage));
        Assert.Equal(23, done.Battle.GetActor(Caster).SourceLoadout!.Items[2] & 127);
    }
    [Fact]
    public void SelfHealingUsesTenAndStaleOrWrongClassDoesNotCommit()
    {
        var start = At("healin");
        var selected = Select(start, Caster);
        var changed = selected.Battle.With(actors: selected.Battle.Actors.Select(a => a.Actor == Caster ?
            a.With(sourceLoadout: new([23,0,279,127], [63,63,63,63])) : a));
        var stale = new SessionSnapshot(selected.SessionId, selected.Revision, selected.ObservationSequence,
            new ActiveBattle(changed, selected.Selection), selected.Story, selected.StopReason);
        var rejected = BattleCommandDispatcher.Submit(stale, new Confirm());
        Assert.NotNull(rejected.Failure); Assert.Same(stale, rejected.Snapshot);
        Assert.Empty(rejected.Observations);
        var wrong = Accept(At("healin", mask: 1u << 12), new Confirm());
        Assert.NotNull(BattleCommandDispatcher.Submit(wrong, new SelectItem(2)).Failure);
        var done = Finish(Accept(selected, new Confirm()), []);
        Assert.Equal(start.Battle.GetActor(Caster).Hp + 10, done.Battle.GetActor(Caster).Hp);
    }
}
