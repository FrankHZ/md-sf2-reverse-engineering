namespace Sf2.Remake.Domain.Battles;

// Only newly executed positive-HP -> zero-HP reactions enter this ordered worklist.
// Positions/resources stay in the battle; the worklist is not another actor snapshot.
internal sealed record BattleDeathBatch(IReadOnlyList<ActorRef> Actors)
{
    internal static BattleDeathBatch Empty { get; } = new(Array.Empty<ActorRef>());

    internal BattleDeathBatch Append(BattleActorState before, BattleActorState after) =>
        before.Hp > 0 && after.Hp == 0 && !Actors.Contains(after.Actor)
            ? new(Array.AsReadOnly<ActorRef>([.. Actors, after.Actor])) : this;

    internal (EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects) Clean(
        EngineBattleState battle, ActorRef firstAlly)
    {
        List<BattleEffect> effects = [];
        foreach (var id in Actors)
        {
            var dead = battle.GetActor(id);
            // Already removed combatants cannot be awarded a second time.
            if (dead.Hp != 0 || dead.Position is null) continue;
            var owner = dead.IsAlly ? dead : battle.GetActor(firstAlly);
            ushort before = (dead.IsAlly ? owner.Defeats : owner.Kills)
                ?? throw new BattleRuleException("unspecified-death-accounting", "actor", true);
            ushort after = dead.IsAlly ? BattleRewards.Defeats(before) : BattleRewards.Kills(before);
            battle = battle.With(actors: battle.Actors.Select(a => a.Actor != owner.Actor ? a :
                dead.IsAlly ? a.With(defeats: after) : a.With(kills: after)));
            effects.Add(new(dead.IsAlly ? "defeats" : "kills", owner.Actor, before, after));
            // Admitted status-free, ATT-only equipment needs no derived-stat change.
            battle = battle.With(actors: battle.Actors.Select(a => a.Actor == id ? a.With(clearPosition: true, status: 0) : a));
            effects.Add(new("death-cleanup", id, 1, 0));
        }
        return (battle, effects.AsReadOnly());
    }
}
