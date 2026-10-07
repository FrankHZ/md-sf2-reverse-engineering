namespace Sf2.Remake.Domain.Battles;

// Policies calculate only the finite existing publications. They never own a live snapshot.
internal interface IBattleProgressionRule
{
    string Identity { get; }
    int Award(EngineBattleState battle, BattleActorState recipient, BattleActionKind kind,
        IReadOnlyList<BattleReaction> reactions, ref uint seed, List<BattleEffect> effects);
    uint Gold(uint current, uint award);
    ushort Kills(ushort current);
    ushort Defeats(ushort current);
    BattleActorState Credit(BattleActorState actor, int amount, List<BattleEffect> effects);
    BattleActorState Grow(BattleActorState actor, ref uint seed, List<BattleEffect> effects);
}

internal sealed class BattlePolicyFault(string identity, string operation) : Exception
{
    internal string Identity { get; } = identity;
    internal string Operation { get; } = operation;
}

internal static class BattleProgressionRules
{
    internal static T Invoke<T>(string identity, string operation, Func<T> call)
    {
        try { return call(); }
        catch (BattleRuleException) { throw; }
        catch (BattlePolicyFault) { throw; }
        catch (Exception) { throw new BattlePolicyFault(identity, operation); }
    }

    internal static (int Amount, uint Seed, IReadOnlyList<BattleEffect> Effects) Award(
        IBattleProgressionRule rule, EngineBattleState battle, BattleActorState actor,
        BattleActionKind kind, IReadOnlyList<BattleReaction> reactions) => Invoke(rule.Identity, "award", () =>
    {
        if (actor.Exp is null) throw new BattleRuleException("unspecified-exp", "actor.exp", true);
        uint seed = battle.MainSeed;
        List<BattleEffect> effects = [];
        int amount = rule.Award(battle, actor, kind, reactions, ref seed, effects);
        Require(amount > 0);
        ValidateDraws(actor.Actor, battle.MainSeed, seed, effects, randomOnly: true);
        return (amount, seed, effects.AsReadOnly());
    });

    internal static (BattleActorState Actor, IReadOnlyList<BattleEffect> Effects) Credit(
        IBattleProgressionRule rule, BattleActorState before, int amount) => Invoke(rule.Identity, "credit", () =>
    {
        if (before.Exp is null) throw new BattleRuleException("unspecified-exp", "actor.exp", true);
        List<BattleEffect> effects = [];
        var after = rule.Credit(before, amount, effects);
        Unchanged(before, after);
        Require(after.Progress == before.Progress && after.Attack == before.Attack && after.SourceLoadout == before.SourceLoadout);
        Require(after.Exp is not null && effects.Count == 1 && effects[0] == new BattleEffect("exp", before.Actor, before.Exp, after.Exp));
        return (after, effects.AsReadOnly());
    });

    internal static (BattleActorState Actor, uint Seed, IReadOnlyList<BattleEffect> Effects) Grow(
        IBattleProgressionRule rule, BattleActorState before, uint initial) => Invoke(rule.Identity, "growth", () =>
    {
        if (before.Exp is null) throw new BattleRuleException("unspecified-exp", "actor.exp", true);
        List<BattleEffect> effects = [];
        uint seed = initial;
        var after = rule.Grow(before, ref seed, effects);
        Unchanged(before, after);
        Require(after.Exp is not null && after.Level >= before.Level && after.MaxHp > 0 && after.Agility <= 127);
        Require((before.SourceLoadout is null) == (after.SourceLoadout is null));
        if (before.SourceLoadout is { } loadout)
            Require(after.SourceLoadout!.Items.SequenceEqual(loadout.Items) && after.SourceLoadout.Spells.Count == loadout.Spells.Count);
        Require(before.Spells.All(after.Spells.Contains) && after.Spells.Distinct().Count() == after.Spells.Count &&
            after.Spells.All(spell => before.Spells.Contains(spell) || before.Definition.Growth?.SpellDefinitions.Values.Contains(spell) == true));
        ValidateDraws(before.Actor, initial, seed, effects, randomOnly: false);
        var values = new Dictionary<string, (long? Before, long? After)>
        {
            ["exp-threshold"] = (before.Exp, after.Exp), ["level"] = (before.Level, after.Level),
            ["level-max-hp"] = (before.MaxHp, after.MaxHp), ["level-max-mp"] = (before.MaxMp, after.MaxMp),
            ["level-base-attack"] = (before.BaseAttack, after.BaseAttack), ["level-defense"] = (before.Defense, after.Defense),
            ["level-agility"] = (before.Agility, after.Agility),
        };
        var facts = effects.Where(effect => effect.RandomRange is null).ToArray();
        Require(facts.Select(effect => effect.Kind).Distinct().Count() == facts.Length);
        foreach (var fact in facts)
        {
            if (values.TryGetValue(fact.Kind, out var value)) Require(fact.Before == value.Before && fact.After == value.After);
            else if (fact.Kind == "level-cap") Require(fact.Before == before.Level && after.Level == before.Level && fact.After == 255);
            else if (fact.Kind == "spell-learned") Require(fact.Before is null && fact.After is >= 0 and <= 254 &&
                after.SourceLoadout!.Spells.Contains((byte)fact.After.Value) && !before.SourceLoadout!.Spells.Contains((byte)fact.After.Value));
            else Require(false);
        }
        foreach (var value in values)
            if (value.Value.Before != value.Value.After) Require(facts.Any(effect => effect.Kind == value.Key));
        if (before.SourceLoadout is { } original)
            foreach (var packed in after.SourceLoadout!.Spells.Except(original.Spells))
                Require(before.Definition.Growth?.SpellDefinitions.TryGetValue(packed, out var spell) == true &&
                    after.Spells.Contains(spell) && facts.Any(effect => effect.Kind == "spell-learned" && effect.After == packed));
        foreach (var spell in after.Spells.Except(before.Spells))
            Require(after.SourceLoadout is not null && after.SourceLoadout.Spells.Any(packed =>
                before.Definition.Growth!.SpellDefinitions.TryGetValue(packed, out var learned) &&
                learned.Value == spell.Value && learned.Level >= spell.Level));
        return (after, seed, effects.AsReadOnly());
    });

    internal static void Preflight(IBattleProgressionRule rule, BattleActorState actor, int amount, uint seed)
    {
        var credit = Credit(rule, actor, amount);
        _ = Grow(rule, credit.Actor, seed); // Discard; actual growth uses the later live scene seed.
    }

    private static void Unchanged(BattleActorState before, BattleActorState after) => Require(
        ReferenceEquals(before.Deployment, after.Deployment) && after.Hp == before.Hp && after.Mp == before.Mp &&
        after.Position == before.Position && after.Kills == before.Kills && after.Defeats == before.Defeats &&
        after.Status == before.Status && after.LastTarget == before.LastTarget && after.ActivationWord == before.ActivationWord &&
        after.AiMemory == before.AiMemory);

    private static void ValidateDraws(ActorRef actor, uint initial, uint final, IEnumerable<BattleEffect> effects, bool randomOnly)
    {
        uint seed = initial;
        foreach (var effect in effects)
        {
            Require(effect.Actor == actor && effect.Target is null);
            if (effect.RandomRange is not { } range)
            { Require(!randomOnly && effect.RandomValue is null && !effect.Kind.StartsWith("rng-", StringComparison.Ordinal)); continue; }
            var draw = BattleRandom.NextMain(seed, range);
            Require(effect.Kind.StartsWith("rng-", StringComparison.Ordinal) && effect.Before == seed &&
                effect.After == draw.After && effect.RandomValue == draw.Value);
            seed = draw.After;
        }
        Require(seed == final);
    }
    private static void Require(bool valid) { if (!valid) throw new InvalidOperationException("Invalid progression result."); }
}
