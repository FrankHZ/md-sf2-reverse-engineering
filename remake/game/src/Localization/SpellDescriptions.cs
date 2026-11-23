using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.GodotAdapter.Localization;

internal static class SpellDescriptions
{
    internal static string Read(SpellDescriptionFacts facts)
    {
        List<string> lines = [Effect(facts)];
        string? target = facts.Target switch
        {
            SpellTarget.Self => GameText.Get("spell-description.self"),
            SpellTarget.AllAllies => GameText.Get("spell-description.all-allies"),
            SpellTarget.PartyMember => GameText.Get("spell-description.party-member"),
            SpellTarget.Ally => facts.Radius > 0 ? GameText.Get("spell-description.ally-area", facts.Radius) : GameText.Get("spell-description.ally"),
            SpellTarget.Enemy => facts.Radius > 0 ? GameText.Get("spell-description.enemy-area", facts.Radius) : GameText.Get("spell-description.enemy"),
            _ => null,
        };
        if (target is not null)
            lines.Add(facts.MinimumRange is { } minimum
                ? target + " " + GameText.Get("battle.range", minimum, facts.MaximumRange!)
                : target);
        if (facts.Effect is SpellEffect.CurePoison or SpellEffect.CurePoisonAndParalysis && facts.FieldPayment is not null)
            lines.Add(GameText.Get("spell-description.cure-payment"));
        if (facts.Effect != SpellEffect.NoEffect && facts.FieldPayment is { } payment && payment != facts.MpCost)
            lines.Add(GameText.Get("spell-description.clamped-mp", payment));
        if (facts.SuppressedBySilence)
            lines.Add(GameText.Get("spell-description.silenced"));
        if (facts.Reason is { } reason && (facts.Effect is not null || reason.Kind != SessionFailureKind.UnsupportedCapability ||
            reason.Code == "detox-curse-unsupported"))
            lines.Add(Refusal(reason));
        return string.Join("\n", lines);
    }

    internal static string Refusal(SessionFailure reason) => reason.Code switch
    {
        "field-egress-context" or "egress-map" => GameText.Get("spell-description.no-refuge"),
        "detox-curse-unsupported" => GameText.Get("spell-description.curse"),
        _ => GameText.BattleRecovery(reason),
    };

    internal static string Damage(AttackSpellElement element) =>
        GameText.Get("spell-description.damage." + element.ToString().ToLowerInvariant());

    private static string Effect(SpellDescriptionFacts facts) => facts.Effect switch
    {
        SpellEffect.RestoreHp when facts.HealingLimit is { } limit => GameText.Get("spell-description.hp-limit", limit),
        SpellEffect.RestoreHp => GameText.Get("spell-description.hp"),
        SpellEffect.FullHp => GameText.Get("spell-description.full-hp"),
        SpellEffect.CurePoison => GameText.Get("item-description.poison"),
        SpellEffect.CurePoisonAndParalysis => GameText.Get("item-description.poison-paralysis"),
        SpellEffect.Damage when facts.Element is { } element => Damage(element),
        SpellEffect.Silence => GameText.Get("spell-description.inflict-silence"),
        SpellEffect.Sleep => GameText.Get("spell-description.sleep"),
        SpellEffect.LowerDefenseAndAgility => GameText.Get("spell-description.slow"),
        SpellEffect.RaiseDefenseAndAgility => GameText.Get("spell-description.boost"),
        SpellEffect.RaiseAttack => GameText.Get("item-description.raise-attack"),
        SpellEffect.Retreat => GameText.Get("item-description.retreat"),
        SpellEffect.ReturnToRefuge => GameText.Get("spell-description.refuge"),
        SpellEffect.NoEffect => GameText.Get("spell-description.free"),
        _ => GameText.Get("spell-description.unsupported"),
    };
}
