using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Application.Runtime.Battles;

internal static class DebugBattleVictory
{
    internal static SessionResult Submit(ScenarioDefinition definition, SessionSnapshot current, SessionRules rules)
    {
        if (!current.CanDebugWinBattle)
            return BattleCommandDispatcher.Reject(current, "debug-victory-unavailable", "command");

        List<SessionObservation> observations = [];
        var battle = current.Battle;
        // Remove the opposition without resolving damage, consuming RNG or posting battle rewards.
        battle = battle.With(actors: battle.Actors.Select(actor => actor.IsAlly ? actor : actor.With(hp: 0)));
        var story = current.Story;
        if (story.EnteringBattle?.Outcome is null)
        {
            current = ProgramRunner.Commit(current, new ActiveBattle(battle, null, Outcome: BattleOutcomeKind.Victory),
                story, observations, "debug-battle-victory");
            return ProgramRunner.Result(ProgramRunner.Stop(current, SessionStopReason.BattleEnded), observations);
        }

        var route = story.EnteringBattle;
        if (route?.Outcome?.DefeatedScene == true && !(route.CompletedFlag is { } flag && story.Flags.Contains(flag)))
        {
            // Required stage positions and character identities belong to encounter content.
            battle = BattleEndPositions.Apply(battle);
            var leader = battle.GetActor(battle.Definition.Outcome!.Leader);
            story = story.Copy(null, defeatedScene: new(battle, leader.Actor, leader.Position,
                BattleDeathBatch.Empty, DebugVictory: true));
            current = ProgramRunner.Commit(current, new ActiveBattle(battle, null), story, observations, "debug-battle-victory");
            return BattleDefeatedProgram.Begin(definition, ProgramRunner.Result(current, observations), rules);
        }

        current = ProgramRunner.Commit(current, new ActiveBattle(battle, null), story, observations, "debug-battle-victory");
        return BattleOutcome.Begin(definition, ProgramRunner.Result(current, observations), rules);
    }
}
