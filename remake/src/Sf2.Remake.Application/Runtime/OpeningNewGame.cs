using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime;

public sealed record PreparedNewGame(SessionStarted? Started, OpeningSaveData? Save, SessionFailure? Failure);

// A one-shot candidate, never the live session. No program/input is drained to manufacture a save.
internal static class OpeningNewGame
{
    internal static PreparedNewGame Prepare(IScenarioSource source, NewGameOptions options, SessionRules rules)
    {
        try
        {
            if (NewGameName.Error(options.ProtagonistName) is { } invalid)
                throw new BattleRuleException(invalid, "newGame.name");
            var read = source.Read();
            if (read is ScenarioReadRejected rejected) return new(null, null, rejected.Failure);
            if (read is not ExplorationReadAccepted { Definition.Persistence: not null } admitted)
                throw new BattleRuleException("new-game-content-required", "newGame");
            var old = admitted.Start;
            var leader = admitted.Definition.Encounters.Values.SelectMany(encounter => encounter.Deployments)
                .FirstOrDefault(row => row.Faction == BattleFaction.Ally && row.Initialization?.AllyPartyMember == 0)?.Actor;
            if (leader is null) throw new BattleRuleException("new-game-leader-required", "newGame");
            var names = new Dictionary<string, string>();
            if (options.ProtagonistName.Length > 0) names.Add(leader.Value.Value, options.ProtagonistName);
            var start = new ExplorationStartInput(old.Map, old.Player, old.Position, old.Facing, old.Speed,
                old.Flags, old.Party, old.EntryProgram, old.EntityPhases, old.Display, old.TextSettings, old.StepCounter, old.EgressMap)
                { Names = names };
            var definition = admitted.Definition;
            var location = definition.Persistence!.InitialLocation(definition, start);
            var initial = ExplorationDispatcher.Initialize(definition, start);
            var data = OpeningSave.CaptureValues(definition, initial, rules, location);
            // Restore is also the candidate start: one ordinary on-load execution, no reroll.
            var outcome = GameSession.Restore(definition, data, rules);
            if (outcome is SessionStartFailed failed) return new(null, null, failed.Failure);
            var started = (SessionStarted)outcome;
            return started.Result.Failure is { } failure ? new(null, null, failure) : new(started, data, null);
        }
        catch (BattleRuleException error)
        { return new(null, null, new(SessionFailureKind.ContentError, error.Code, error.Field, error.Code)); }
    }
}
