using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class AuraHealingTests
{
    internal static readonly ActorRef Caster = new("z-priest"), Center = new("b-center");
    internal static EngineBattleState Battle() => Start("aura-healing").Current.Battle;
    internal static BattleActionResolution Prepare(EngineBattleState b, byte level, ActorRef? center = null, MapPosition? destination = null)
    {
        var spell = new SpellRef("aura", level); var target = center ?? (level == 4 ? Caster : Center);
        var at = destination ?? b.GetActor(Caster).Position!;
        var action = new Sf2HealingRule().Prepare(b, Caster, at, spell, target, new Sf2BattleProgressionRule());
        action.ValidateHealing(b, Caster, at, b.Definition.Spells[spell], target);
        return action;
    }

    [Theory]
    [InlineData(1, 15)] [InlineData(2, 15)] [InlineData(3, 30)]
    public void RadiusTargetsUseSlotsNotRowProcessingOrLexicalOrder(byte level, int power)
    {
        var b = Battle(); var a = Prepare(b, level);
        Assert.Equal(new[] { "a-edge", "b-center", "m-full", "z-priest" }, a.Reactions.Select(r => r.Target.Value));
        Assert.Equal(new[] { power, 10, 0, power }, a.Reactions.Select(r => r.Amount));
        Assert.Equal(new long?[] { 10, 20, 25, 25 }, a.ConstructionEffects.Where(e => e.Kind == "healing-exp-contribution").Select(e => e.After));
        Assert.Equal(2, a.ConstructionEffects.Count(e => e.RandomRange is not null));
        Assert.All(b.Actors, old => {
            var prepared = a.Prepared.GetActor(old.Actor);
            Assert.Equal(old.Hp, prepared.Hp); Assert.Equal(old.Mp, prepared.Mp); Assert.Equal(old.Exp, prepared.Exp);
        });
    }

    [Fact]
    public void Aura4UsesSelfCenterAndFullMissingPerLivingPlacedAllyIncludingNeutralAndFar()
    {
        var b = Battle(); var a = Prepare(b, 4);
        Assert.Equal(new[] { "a-edge", "b-center", "n-neutral", "m-full", "z-priest", "f-far" }, a.Reactions.Select(r => r.Target.Value));
        Assert.Equal(new[] { 99, 10, 60, 0, 30, 590 }, a.Reactions.Select(r => r.Amount));
        Assert.Throws<BattleRuleException>(() => Prepare(b, 4, Center));
        Assert.Equal(80, a.Prepared.GetActor(Caster).Mp);
    }

    [Theory]
    [InlineData(1, 3, 2, false)] // radius2 diagonal from center
    [InlineData(2, 3, 2, true)]
    [InlineData(2, 2, 2, false)]
    [InlineData(3, 6, 3, true)]
    public void InclusiveManhattanEdgesAndChangedCasterPosition(byte level, int x, int y, bool included)
    {
        var b = Battle(); b = b.With(actors: b.Actors.Select(a => a.Actor == Caster ? a.With(position: new(x,y)) : a));
        var action = Prepare(b, level);
        Assert.Equal(included, action.Reactions.Any(r => r.Target == Caster));
    }

    [Fact]
    public void ProvisionalSelfCenterUsesDestinationForGeometryAndLeavesOriginBeforeConfirmation()
    {
        var b = Battle(); var destination = new MapPosition(4,2);
        var action = Prepare(b, 1, Caster, destination);
        Assert.Equal(new[] { Center, Caster }, action.Reactions.Select(r => r.Target));
        Assert.Equal(new MapPosition(3,3), b.GetActor(Caster).Position);
        Assert.Equal(destination, action.Prepared.GetActor(Caster).Position);
        Assert.Throws<BattleRuleException>(() => Prepare(b, 1, new("f-far")));
        Assert.Throws<BattleRuleException>(() => Prepare(b, 1, new("n-neutral")));
        Assert.Throws<BattleRuleException>(() => Prepare(b, 1, new("u-unplaced")));
        Assert.Throws<BattleRuleException>(() => Prepare(b, 1, new("d-dead")));
        Assert.Throws<BattleRuleException>(() => Prepare(b, 1, new("enemy")));
    }

    [Theory]
    [InlineData(1, 7)] [InlineData(2, 11)] [InlineData(3, 15)] [InlineData(4, 20)]
    public void ExactMpWorksAndInsufficientMpRejectsWithoutStateChange(byte level, byte cost)
    {
        var b = Battle(); b = b.With(actors: b.Actors.Select(a => a.Actor == Caster ? a.With(mp: cost) : a));
        Assert.Equal(cost, Prepare(b, level).Spell!.MpCost);
        b = b.With(actors: b.Actors.Select(a => a.Actor == Caster ? a.With(mp: (byte)(cost-1)) : a));
        Assert.Equal("insufficient-mp", Assert.Throws<BattleRuleException>(() => Prepare(b, level)).Code);
        Assert.Equal(cost-1, b.GetActor(Caster).Mp);
    }

    [Fact]
    public void ZeroRecoveryStillContributesTenAndAllFullTargetsSaturateSharedCap()
    {
        var b = Battle(); b = b.With(actors: b.Actors.Select(a => a.Hp > 0 ? a.With(hp: a.MaxHp) : a));
        var a = Prepare(b, 2);
        Assert.All(a.Reactions, r => Assert.Equal(0, r.Amount));
        Assert.Equal(new long?[] {10,20,25,25}, a.ConstructionEffects.Where(e=>e.Kind=="healing-exp-contribution").Select(e=>e.After));
    }

    [Fact]
    public void ReorderedIdsSlotsAndJsonRowsChangeOnlyTheDeclaredOrdering()
    {
        var d = Document("aura-healing");
        // Change every identity, reorder definitions/placements/inputs, and swap source slots.
        string text = d.ToJsonString();
        text = text.Replace("z-priest", "healer-q").Replace("a-edge", "zzz-edge").Replace("b-center", "aaa-center");
        d = JsonNode.Parse(text)!;
        var slots = d["encounters"]![0]!["initialization"]!["sourceCombatants"]!.AsArray();
        slots[1]!["slot"] = 0; slots[2]!["slot"] = 1;
        foreach(var array in new[] {d["actors"]!.AsArray(), d["start"]!["actors"]!.AsArray(), d["encounters"]![0]!["placements"]!.AsArray()})
        { var reversed = array.Reverse().Select(n=>n!.DeepClone()).ToArray(); array.Clear(); foreach(var n in reversed) array.Add(n); }
        var s = Assert.IsType<SessionStarted>(GameSession.Start(Reader(d))).Session;
        var b = s.Current.Battle; var caster = new ActorRef("healer-q");
        var a = new Sf2HealingRule().Prepare(b,caster,b.GetActor(caster).Position!,new("aura",1),new("aaa-center"),new Sf2BattleProgressionRule());
        Assert.Equal(new[] {"aaa-center","zzz-edge","m-full","healer-q"},a.Reactions.Select(r=>r.Target.Value));
    }

    [Theory]
    [InlineData("missing-slots")] [InlineData("duplicate-actor")] [InlineData("wrong-side")]
    [InlineData("power")] [InlineData("radius")] [InlineData("full-recovery")] [InlineData("class")]
    public void ContentRejectsUnsupportedBindingsBeforeSession(string mutation)
    {
        var d = Document("aura-healing"); var spell = d["spells"]![0]!;
        var slots = d["encounters"]![0]!["initialization"]!["sourceCombatants"]!.AsArray();
        switch(mutation)
        {
            case "missing-slots": slots.RemoveAt(0); break;
            case "duplicate-actor": slots[1]!["actor"]="z-priest"; break;
            case "wrong-side": slots[0]!["slot"]=129; break;
            case "power": spell["effect"]!["adjustedPower"]=16; break;
            case "radius": spell["effect"]!["radius"]=0; break;
            case "full-recovery": spell["effect"]!["fullRecovery"]=true; break;
            case "class": d["actors"]![0]!["classRule"]="ordinary"; break;
        }
        Assert.IsType<ScenarioReadRejected>(Reader(d).Read());
    }

    [Theory]
    [InlineData(32767, true)] [InlineData(32768, false)] [InlineData(40000, false)]
    public void AuraEncounterBoundsHpBeforeAnySourceSignedOverflow(int maximum, bool admitted)
    {
        var document = Document("aura-healing");
        document["actors"]![5]!["maxHp"] = maximum;
        document["start"]!["actors"]![5]!["hp"] = 32760;
        if (!admitted)
        {
            Assert.Equal("aura-hp-domain", Assert.IsType<ScenarioReadRejected>(Reader(document).Read()).Failure.Code);
            return;
        }
        var s = Assert.IsType<SessionStarted>(GameSession.Start(Reader(document))).Session;
        var before = s.Current;
        Assert.True(s.QueryBattleChoices().Spells.Single(c => c.Spell.Level == 4).Enabled);
        Assert.Same(before, s.Current);
        Assert.Equal(7, Prepare(before.Battle, 4).Reactions.Last().Amount);
        var near = before.Battle.With(actors: before.Battle.Actors.Select(a => a.Actor.Value == "f-far" ? a.With(position: new(4,2)) : a));
        Assert.Equal(7, Prepare(near, 1).Reactions.Last().Amount);
        var wounded = near.With(actors: near.Actors.Select(a => a.Actor.Value == "f-far" ? a.With(hp: 1) : a));
        Assert.Equal(32766, Prepare(wounded, 4).Reactions.Last().Amount);
        Assert.Equal(80,s.Current.Battle.GetActor(Caster).Mp);
    }

    [Fact]
    public void PreparedBoundaryRejectsMissingDuplicateReorderedOrPrematureRecoveries()
    {
        var b=Battle(); var a=Prepare(b,2); var at=b.GetActor(Caster).Position!;
        void Invalid(BattleActionResolution candidate) => Assert.Throws<InvalidOperationException>(()=>candidate.ValidateHealing(b,Caster,at,a.Spell!,Center));
        Invalid(a with { Reactions=a.Reactions.Skip(1).ToArray() });
        Invalid(a with { Reactions=a.Reactions.Reverse().ToArray() });
        Invalid(a with { Reactions=a.Reactions.Select((r,i)=>i==1?a.Reactions[0]:r).ToArray() });
        Invalid(a with { Prepared=a.ApplyReaction(a.Prepared,a.Reactions[0]) });
    }
}
