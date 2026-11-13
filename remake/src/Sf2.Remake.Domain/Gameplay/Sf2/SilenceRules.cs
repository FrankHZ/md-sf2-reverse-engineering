using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal sealed record SilencedCast(BattleActionRef Action, byte MpCost);
internal sealed record SilenceTurnResult(ushort Before, ushort After, MainRandomDraw Draw);

// Finite source SILENCE consumer, not a status registry. Sleep and poison keep their owners.
internal static class SilenceRules
{
    internal const ushort Mask = 0x0300;
    internal static bool Present(ushort status) => (status & Mask) != 0;
    internal static bool Supported(ushort status, bool poison = true) =>
        (status & ~(Mask | 0xC0 | 1 | (poison ? 2 : 0))) == 0;
    internal static bool HasSpells(BattleActorState actor) => actor.SourceLoadout is { } packed &&
        packed.Spells.Any(s => (s & 63) != 63) || actor.Spells.Count != 0;

    // HEAL/AURA, selected damage spells and DISPEL all carry AFFECTEDBYSILENCE.
    // Items' HEALIN is unmarked. ExecuteIndividualTurn branches EGRESS before CastSpell.
    internal static byte? Cost(BattleActionRef action, BattleActionAdmission admission) => action.Kind switch
    {
        BattleActionKind.Healing => admission.Spell?.MpCost,
        BattleActionKind.AttackSpell when action.ItemSlot is null && action.Spell?.Value != "aqua" => admission.AttackSpell?.MpCost,
        BattleActionKind.SupportSpell => admission.SupportSpell?.MpCost,
        _ => null,
    };

    internal static BattleActionResolution? Suppress(EngineBattleState battle, ActorRef actor,
        MapPosition destination, BattleActionRef action, BattleActionAdmission admission)
    {
        if (!Present(battle.GetActor(actor).Status) || Cost(action, admission) is not { } cost) return null;
        BattleMovement.RequireStop(battle, actor, destination);
        return new(battle.With(actors: battle.Actors.Select(a => a.Actor == actor ? a.With(position: destination) : a)),
            actor, destination, [], null, [], [], Silenced: new(action, cost));
    }

    internal static SilenceTurnResult? AfterTurn(BattleActorState actor, uint seed)
    {
        ushort counter = (ushort)(actor.Status & Mask);
        if (actor.Hp == 0 || counter == 0) return null;
        if (!Supported(actor.Status)) throw new BattleRuleException("after-turn-status", "actor.status", true);
        var draw = BattleRandom.NextMain(seed, counter);
        ushort after = (ushort)((actor.Status & ~Mask) | ((draw.Value & Mask) == 0 ? 0 : counter - 0x100));
        return new(actor.Status, after, draw);
    }
}
