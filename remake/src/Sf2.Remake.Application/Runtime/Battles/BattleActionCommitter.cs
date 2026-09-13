using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

internal static class BattleActionCommitter
{
    // Called only after an entire player or enemy ACTION has resolved on temporary state.
    internal static SessionSnapshot Publish(SessionSnapshot current, EngineBattleState battle,
        ActorRef actor, MapPosition destination, IReadOnlyList<BattleEffect> effects, List<SessionObservation> observations)
    {
        long revision = checked(current.Revision + 1), sequence = current.ObservationSequence;
        var origin = current.Battle.GetActor(actor).Position;
        if (origin != destination)
            observations.Add(new(++sequence, revision, "movement", actor, From: origin, To: destination));
        foreach (var effect in effects)
            observations.Add(new(++sequence, revision, effect.Kind, effect.Actor, effect.Before, effect.After,
                RandomRange: effect.RandomRange, RandomValue: effect.RandomValue, Target: effect.Target));
        if (battle.MainSeed != current.Battle.MainSeed)
            observations.Add(new(++sequence, revision, "action-rng", actor, current.Battle.MainSeed, battle.MainSeed));
        observations.Add(new(++sequence, revision, "action-committed", actor));
        return new(current.SessionId, revision, sequence, BattleTurnFlow.ConsumeEntry(battle), null, SessionStopReason.SimulationWait);
    }
}
