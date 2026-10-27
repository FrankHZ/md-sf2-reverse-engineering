using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// Explicit source identities for this admitted encounter. ActorRef is the logical
// combatant; the enemy-definition value is independent of its deployment slot.
public sealed class TarosEffectiveness
{
    internal TarosEffectiveness(ActorRef bowie, IReadOnlyDictionary<ActorRef, byte> enemyDefinitions,
        IEnumerable<byte> weaponItems)
    {
        Bowie = bowie;
        EnemyDefinitions = new ReadOnlyDictionary<ActorRef, byte>(enemyDefinitions.ToDictionary(row => row.Key, row => row.Value));
        WeaponItems = Array.AsReadOnly(weaponItems.ToArray());
    }

    public ActorRef Bowie { get; }
    public IReadOnlyDictionary<ActorRef, byte> EnemyDefinitions { get; }
    public IReadOnlyList<byte> WeaponItems { get; }

    internal bool IsTaros(BattleActorState actor) => actor.Faction == BattleFaction.Enemy &&
        EnemyDefinitions.TryGetValue(actor.Actor, out byte definition) && definition == 88;

    internal bool PrepareInitial(BattleActorState actor, BattleActorState target)
    {
        if (!IsTaros(target)) return false;
        // GetEquippedWeapon: first equipped, non-NOTHING item whose type includes
        // WEAPON. Broken/other bits do not change its masked identity; no class check.
        byte? equipped = actor.SourceLoadout?.Items.Where(word =>
            (word & 128) != 0 && (word & 127) != 127 && WeaponItems.Contains((byte)(word & 127)))
            .Select(word => (byte?)(word & 127)).FirstOrDefault();
        return actor.Actor != Bowie || equipped != 61;
    }
}
