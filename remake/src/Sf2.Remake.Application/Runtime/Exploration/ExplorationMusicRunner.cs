using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

// Accepted modern clock: a step is one admitted common service, not a hardware interval.
internal static class ExplorationMusicRunner
{
    private static int Area(SessionSnapshot current) => current.Exploration!.Definition.Traversal
        .SelectActiveArea(current.Exploration.PlayerEntity.Position)!.OneBasedRecordOrdinal - 1;

    internal static MusicProgress? Request(ExplorationDefinition definition, SessionSnapshot current, string? cue, WaitToken token)
    {
        if (current.Story.Wait is MusicWait)
            throw new BattleRuleException("music-helper-interference", "program.music", true);
        if (cue is null || definition.Visuals is not { } visuals || !visuals.Audio.TryGetValue(cue, out var audio) || audio.Command >= 65)
            return current.Story.Music;
        int area = Area(current);
        var old = current.Story.Music;
        bool battle = current.Story.EnteringBattle is not null;
        if (old is { HistoryBound: true } && old.Map == current.Exploration!.Map && old.Area == area && old.Battle == battle)
        {
            if (old.Cue == cue) return old;
            return new(token.Value, cue, Array.AsReadOnly(new[] { old.Cue }.Concat(old.Previous).Take(9).ToArray()),
                old.Map, area, audio.ModernEndStep, Battle: battle);
        }
        if (old is not null)
            // Several context changes can occur before the host observes a result. Do not
            // infer that its map selection replaced the player, or revive an older stack.
            // This request establishes only the current cue; a later replacement can bind
            // finite progress and a known previous cue again.
            return new(token.Value, cue, Array.AsReadOnly(System.Array.Empty<string>()),
                current.Exploration!.Map, area, null, Battle: battle);
        // The ordinary map music selection is typed content, also consumed by SessionAudio.
        string? previous = null;
        if (definition.Visuals!.Maps.TryGetValue(current.Exploration!.Map, out var map) && area < map.Music.Count)
        {
            int command = current.Story.EnteringBattle is null ? map.Music[area].Field : map.Music[area].Battle;
            previous = definition.Visuals.Audio.FirstOrDefault(pair => pair.Value.Command == command).Key;
        }
        return new(token.Value, cue, Array.AsReadOnly(previous is null || previous == cue ? [] : new[] { previous }),
            current.Exploration.Map, area, audio.ModernEndStep, Battle: battle);
    }

    // SessionAudio selects map/area/battle music at these context changes. Its older stack
    // is not admitted by Application; even a return to the same map cannot revive it.
    internal static StoryState InvalidateContext(ActiveSessionState active, StoryState story)
    {
        if (story.Music is not { HistoryBound: true } music) return story;
        if (active is ActiveExploration exploration && music.Map == exploration.World.Map &&
            music.Area == exploration.World.Definition.Traversal.SelectActiveArea(exploration.World.PlayerEntity.Position)!.OneBasedRecordOrdinal - 1 &&
            music.Battle == (story.EnteringBattle is not null)) return story;
        if (story.Wait is MusicWait)
            throw new BattleRuleException("music-context-replaced", "story.music", true);
        return story.Copy(story.Cursor, story.Wait,
            music: music with { HistoryBound = false, EndStep = null, ActualDone = false, PreviousEligible = false });
    }

    internal static MusicWait Begin(SessionSnapshot current, WaitToken token)
    {
        ExplorationTextRunner.ValidateContext(current);
        var music = current.Story.Music;
        if (music?.EndStep is null || music.Previous.Count == 0)
            throw new BattleRuleException("field-music-progress-unbound", "program.presentation", true);
        ValidateContext(current, music);
        return new(token, music.Generation);
    }

    private static void ValidateContext(SessionSnapshot current, MusicProgress music)
    {
        if (!music.HistoryBound || music.Map != current.Exploration!.Map || music.Area != Area(current) ||
            music.Battle != (current.Story.EnteringBattle is not null))
            throw new BattleRuleException("music-context-replaced", "story.music", true);
    }

    internal static SessionSnapshot BeforeService(SessionSnapshot current, List<SessionObservation> observations)
    {
        if (current.Story.TextSettings is null || current.Story.Music is not { EndStep: { } end } music) return current;
        ValidateContext(current, music);
        music = music with { Step = Math.Min(end, music.Step + 1) };
        ProgramWait? wait = current.Story.Wait;
        string kind = "music-step";
        if (wait is MusicWait helper)
        {
            if (helper.Generation != music.Generation || helper.LogicalDone)
                throw new BattleRuleException("music-helper-interference", "story.music", true);
            bool cleared = helper.Cleared || helper.Armed && music.Step == end;
            int elapsed = helper.Elapsed + 1;
            if (!helper.Armed) kind = "music-wait-armed";
            else if (cleared && !helper.Cleared) kind = "music-previous-eligible";
            wait = helper with { Armed = true, Cleared = cleared, Elapsed = elapsed,
                LogicalDone = cleared && elapsed % 3 == 0 };
            music = music with { PreviousEligible = cleared };
        }
        return ProgramRunner.Commit(current, current.Active, current.Story.Copy(current.Story.Cursor, wait, music: music),
            observations, kind, music.Cue);
    }

    internal static SessionSnapshot Finish(SessionSnapshot current, List<SessionObservation> observations) =>
        current.Story.Wait is MusicWait { LogicalDone: true } wait &&
            current.Story.Music is { ActualDone: true } music && music.Generation == wait.Generation
        ? ProgramRunner.Commit(current, current.Active,
            current.Story.Copy(current.Story.Cursor is { } cursor ? ProgramRunner.Next(cursor) : null),
            observations, "music-wait-returned", music.Cue) : current;

    internal static MusicProgress Previous(ExplorationDefinition definition, SessionSnapshot current, WaitToken token)
    {
        var music = current.Story.Music;
        if (music is null || music.Previous.Count == 0 ||
            music.EndStep is not null && !(music.PreviousEligible && music.ActualDone))
            throw new BattleRuleException("music-previous-unavailable", "program.music", true);
        ValidateContext(current, music);
        string cue = music.Previous[0];
        return new(token.Value, cue, Array.AsReadOnly(music.Previous.Skip(1).ToArray()), music.Map, music.Area,
            definition.Visuals!.Audio[cue].ModernEndStep, Battle: music.Battle);
    }
}
