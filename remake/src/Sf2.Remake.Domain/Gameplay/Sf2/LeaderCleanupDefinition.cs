using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

public sealed class LeaderCleanupDefinition
{
    internal LeaderCleanupDefinition(ActorRef bowie, IReadOnlyDictionary<ActorRef, byte> enemySlots)
    {
        Bowie = bowie;
        EnemySlots = new ReadOnlyDictionary<ActorRef, byte>(new Dictionary<ActorRef, byte>(enemySlots));
        Gate = EnemySlots.Single(row => row.Value == 128).Key;
        Enemies = Array.AsReadOnly(EnemySlots.OrderBy(row => row.Value).Select(row => row.Key).ToArray());
    }
    public ActorRef Bowie { get; }
    public ActorRef Gate { get; }
    public IReadOnlyDictionary<ActorRef, byte> EnemySlots { get; }
    public IReadOnlyList<ActorRef> Enemies { get; }
}
