using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// Reads the admitted effect and the same target lists used during action construction.
internal sealed record BattleActionArea(int? Radius, IReadOnlyList<BattleActorState> NominalRoster,
    IReadOnlyList<BattleActorState> Recipients)
{
    internal static BattleActionArea Read(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, BattleActionAdmission admission, BattleActorState center)
    {
        var attack = admission.AttackItem?.Effect ?? admission.AttackSpell;
        var support = admission.SupportItem?.Effect ?? admission.SupportSpell;
        var spell = admission.Spell;
        bool allSide = spell?.Targets == HealingTargets.AuraAllAllies || admission.Item?.Effect == HealingItemEffect.FullPartyHp;
        int? radius = allSide ? null : attack?.Radius ?? support?.Radius ?? spell?.Radius ?? 0;
        bool suppressed = SilenceRules.Present(battle.GetActor(actor).Status) && SilenceRules.Cost(action, admission).HasValue;
        if (suppressed && !allSide) return new(radius, [], []);

        IReadOnlyList<BattleActorState> targets;
        if (attack is not null) targets = Sf2AttackSpellAction.ResolveTargets(battle, actor, destination, attack, center.Actor);
        else if (support is not null) targets = Sf2SupportSpellAction.Targets(battle, actor, destination, support, center.Actor);
        else if (spell is not null) targets = HealingTargetRules.Resolve(battle, actor, destination, spell, center.Actor);
        else if (admission.Item is { } recovery) targets = HealingTargetRules.ResolveItem(battle, actor, destination, recovery, center.Actor);
        else if (admission.Retreat is not null || admission.RetreatItem is not null) targets = [];
        else targets = [center];

        return new(radius, allSide ? targets : [], suppressed ? [] : targets);
    }
}
