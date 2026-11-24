using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal static class MuddleRules
{
    internal const ushort CounterMask = 0x0030;
    internal const ushort ConfusionFlag = 0x0008;
    internal const ushort Mask = CounterMask | ConfusionFlag;
    internal static bool Present(ushort status) => (status & CounterMask) != 0;
    internal static bool Confused(ushort status) => Present(status) && (status & ConfusionFlag) != 0;
    internal static bool Valid(ushort status) => (status & ConfusionFlag) == 0 || Present(status);
    internal static bool SupportedRankOne(ushort status, bool poison = true) =>
        !Confused(status) && SilenceRules.SupportedInBattle(status, poison);

    internal static bool CanTurn(EngineBattleState battle, BattleActorState actor, ushort status)
    {
        if (!Present(status) || actor.Hp == 0 || actor.Position is null) return true;
        if (Confused(status)) return CanConfusedTurn(battle, actor);
        if (!actor.IsAlly) return true; // The existing enemy strategy still owns its turn.
        if (actor.SourceLoadout is null || actor.ActivationWord is null || actor.Definition.Physical is null ||
            battle.Definition.Rewards is null || battle.Definition.Initialization is not { } initialization ||
            !initialization.SourceCombatants.Any(slot => slot.Key is >= 0 and < 30 && slot.Value == actor.Actor))
            return false;

        // Commandset 06 needs physical operands for its source opponents, including
        // those that can enter reach later. The live occupancy contract is shared with the selector.
        var opponents = SourceAllyAi.Opponents(battle, initialization.SourceCombatants).ToArray();
        return opponents.Any(SourceAllyAi.IsPlacedOpponent) &&
            opponents.All(other => other.Definition.Physical is not null && SourceAllyAi.SupportsOpponentOccupancy(other));
    }

    private static bool CanConfusedTurn(EngineBattleState battle, BattleActorState actor)
    {
        var slots = battle.Definition.Initialization?.SourceCombatants;
        if (slots is null || actor.ActivationWord is null || actor.SourceLoadout is null ||
            actor.Definition.Physical is null || battle.Definition.Rewards is null ||
            BattleOffense.FixedPursuitTarget(battle, actor) is null ||
            !BattleOffense.OrderedRoster(battle, actor, OffensiveTargetSide.OwnSide).Any(unit => unit.Actor == actor.Actor))
            return false;

        if (actor.IsAlly)
            return (actor.Control == BattleControl.Player ||
                actor is { Control: BattleControl.Automatic, Deployment.SourceAllyControl: true } &&
                actor.AiStrategy == SourceAllyAi.Strategy && SourceAllyAi.PhysicalOnly(actor, actor.SourceLoadout)) &&
                BattleOffense.OrderedRoster(battle, actor, OffensiveTargetSide.OwnSide)
                    .All(SourceAllyAi.SupportsOpponentOccupancy);

        return actor.Control == BattleControl.Automatic && actor.AiStrategy == SourceEnemyAi.Strategy &&
            actor.Deployment.Initialization is { } source && SourceEnemyAi.SupportsCommandset(source.AiCommandset) &&
            actor.PrimaryOrder is not null && actor.SecondaryOrder is not null &&
            BattleEquipment.OrdinaryAiLoadout(actor.Definition, actor.SourceLoadout, allowHerb: true, allowEarlySpells: true) &&
            battle.Actors.Where(unit => unit.IsAlly).All(SourceAllyAi.SupportsOpponentOccupancy);
    }

    internal static (ushort After, MainRandomDraw Draw)? AfterTurn(BattleActorState actor, uint seed)
    {
        ushort count = (ushort)(actor.Status & CounterMask);
        if (actor.Hp == 0 || count == 0) return null;
        if (!SilenceRules.SupportedInBattle(actor.Status))
            throw new BattleRuleException("after-turn-status", "actor.status", true);
        var draw = BattleRandom.NextMain(seed, count);
        ushort after = (draw.Value & CounterMask) == 0 ? (ushort)(actor.Status & ~Mask) : (ushort)(actor.Status - 0x10);
        return (after, draw);
    }
}
