namespace Sf2.Remake.Domain.Battles;

// Action deaths retain reaction order; the leader hook appends living source slots.
// The batch is consumed once at the field-exit publication, never rebuilt on retry.
// Positions/resources stay in the battle; the worklist is not another actor snapshot.
internal sealed record BattleDeathBatch(IReadOnlyList<ActorRef> Actors, IReadOnlyList<ActorRef>? Forced = null)
{
    internal static BattleDeathBatch Empty { get; } = new(Array.Empty<ActorRef>());

    internal BattleDeathBatch Append(BattleActorState before, BattleActorState after) =>
        before.Hp > 0 && after.Hp == 0 && !Actors.Contains(after.Actor)
            ? new(Array.AsReadOnly<ActorRef>([.. Actors, after.Actor]), Forced) : this;

    internal BattleDeathBatch AppendSurvivors(EngineBattleState battle)
    {
        if (battle.Definition.LeaderCleanup is not { } binding) return this;
        var appended = binding.Enemies.Where(id => battle.GetActor(id).Hp > 0 && !Actors.Contains(id)).ToArray();
        return new(Array.AsReadOnly<ActorRef>([.. Actors, .. appended]),
            Array.AsReadOnly<ActorRef>([.. Forced ?? [], .. appended]));
    }

    internal (EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects) Clean(
        EngineBattleState battle, ActorRef? firstAlly, IBattleProgressionRule rule)
    {
        List<BattleEffect> effects = [];
        foreach (var id in Actors)
        {
            var dead = battle.GetActor(id);
            // Scene return can unplace a casualty before this pass. The retained batch
            // owns the work and FieldExit consumes it once. Forced entries may still have HP.
            if (Forced?.Contains(id) != true && dead.Hp != 0) continue;
            if (dead.IsAlly || firstAlly is not null)
            {
                var owner = dead.IsAlly ? dead : battle.GetActor(firstAlly!.Value);
                ushort before = (dead.IsAlly ? owner.Defeats : owner.Kills)
                    ?? throw new BattleRuleException("unspecified-death-accounting", "actor", true);
                ushort after = BattleProgressionRules.Invoke(rule.Identity, "death-accounting", () =>
                    dead.IsAlly ? rule.Defeats(before) : rule.Kills(before));
                battle = battle.With(actors: battle.Actors.Select(a => a.Actor != owner.Actor ? a :
                    dead.IsAlly ? a.With(defeats: after) : a.With(kills: after)));
                effects.Add(new(dead.IsAlly ? "defeats" : "kills", owner.Actor, before, after));
            }
            battle = battle.With(actors: battle.Actors.Select(a => a.Actor == id ? a.With(clearPosition: true, status: 0, refreshStats: true) : a));
            effects.Add(new("death-cleanup", id, 1, 0));
        }
        return (battle, effects.AsReadOnly());
    }
}
