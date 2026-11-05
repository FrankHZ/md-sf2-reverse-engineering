using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// ApplyPositionsAfterEnemyLeaderDies is a scene-return consumer, independent of
// the completed flag and the defeated wrapper's leader-presence death-list rule.
internal static class BattleEndPositions
{
    internal static EngineBattleState Apply(EngineBattleState battle)
    {
        if (battle.Definition.Outcome is not { DeathPositions: { } positions } route ||
            battle.GetActor(route.Leader).Hp == 0 || battle.GetActor(route.DefeatedGateEnemy).Hp != 0) return battle;
        return battle.With(actors: battle.Actors.Select(actor =>
        {
            int slot = actor.IsAlly ? actor.Deployment.Initialization?.AllyPartyMember ?? actor.ProcessingOrder : actor.ProcessingOrder;
            ushort hp = !actor.IsAlly && slot < 158 ? (ushort)0 : actor.Hp;
            MapPosition? position = slot >= 158 && actor.Position is { } tail ? new(0, tail.Y) : null;
            if (positions.TryGetValue(slot, out var restored) && (hp > 0 || slot == 128)) position = restored;
            return actor.With(hp: hp, position: position, clearPosition: position is null);
        }));
    }
}
