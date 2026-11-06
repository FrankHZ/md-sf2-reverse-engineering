using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Persistence;

public sealed record ObserveBattleMusic(MapId Map, int Area, SavedMusic Music) : SessionCommand;

internal static class BattleMusicObservation
{
    internal static SessionResult Submit(ScenarioDefinition definition, SessionSnapshot current, SessionRules rules,
        ObserveBattleMusic observed)
    {
        try
        {
            BattleSave.RequireStable(current, rules);
            var route = current.Story.EnteringBattle!;
            var visuals = definition.Exploration?.Visuals;
            if (observed.Map != route.Outcome!.BattleMap || visuals is null ||
                !visuals.Maps.TryGetValue(observed.Map, out var map) || observed.Area < 0 || observed.Area >= map.Music.Count ||
                observed.Music.Previous.Count > 9 || !observed.Music.Previous.Prepend(observed.Music.Cue)
                    .All(cue => visuals.Audio.TryGetValue(cue, out var audio) && audio.Command < 65) ||
                visuals.Audio[observed.Music.Cue].ModernEndStep is not null || visuals.Audio[observed.Music.Cue].LoopBegin is null)
                throw new BattleRuleException("save-battle-music-observation", "music");
            var old = current.Story.Music;
            if (old is { EndStep: not null } or { ActualDone: true } or { DeliverySkipped: true } or { PreviousEligible: true })
                throw new BattleRuleException("save-battle-music-unknown", "music");
            bool bound = old is { HistoryBound: true, Battle: true } && old.Map == observed.Map && old.Area == observed.Area;
            if (bound && old!.Cue != observed.Music.Cue)
                throw new BattleRuleException("save-battle-music-disagreement", "music");
            if (bound && old!.Previous.SequenceEqual(observed.Music.Previous)) return new(current, [], current.StopReason);
            // Native battle scenes push attack/defence music between player inputs. Their
            // actual bounded stack supersedes the previous observation; it is not a prefix
            // extension of that stack. Keep the known current cue and live logical clock.
            var previous = Array.AsReadOnly(observed.Music.Previous.ToArray());
            var music = bound ? old! with { Previous = previous } :
                new MusicProgress(current.ObservationSequence + 1, observed.Music.Cue, previous, observed.Map,
                    observed.Area, null, Battle: true);
            var snapshot = new SessionSnapshot(current.SessionId, current.Revision + 1, current.ObservationSequence + 1,
                current.Active, current.Story.Copy(null, music: music), current.StopReason);
            return new(snapshot, [new(snapshot.ObservationSequence, snapshot.Revision, "battle-music-observed")], current.StopReason);
        }
        catch (BattleRuleException error)
        { return new(current, [], current.StopReason, new(SessionFailureKind.IllegalCommand, error.Code, error.Field, error.Code)); }
    }
}
