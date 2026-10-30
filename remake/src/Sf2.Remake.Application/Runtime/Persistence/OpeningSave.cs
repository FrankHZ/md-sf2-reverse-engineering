using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Persistence;

internal static class OpeningSave
{
    internal static SaveCompatibility Compatibility(ScenarioDefinition definition, SessionRules rules)
    {
        var source = definition.Exploration?.Provenance;
        if (definition.Persistence is null || source is null || rules.Identity != "sf2")
            throw new BattleRuleException("save-profile-unavailable", "save.profile", true);
        return new("sf2-opening", definition.Package, 1, rules.Identity, 1, 1,
            source.Repository, source.Commit, source.RomSha256);
    }

    internal static SaveAvailability Query(ScenarioDefinition definition, SessionSnapshot current, SessionRules rules)
    {
        try
        {
            _ = Compatibility(definition, rules);
            RequireStable(definition, current);
            var location = definition.Persistence!.Select(definition, current.Exploration!.Map, current.Story.Flags, current.Story.SaveAnchor);
            var data = CaptureValues(definition, current, rules, location);
            // Validate the detached progress and the recipe's landing before offering a slot.
            _ = OpeningRestore.Build(definition, data, rules);
            return new(current.SessionId, current.Revision, location, null);
        }
        catch (BattleRuleException error) { return new(current.SessionId, current.Revision, null, error.Code); }
    }

    internal static SaveCapture Capture(ScenarioDefinition definition, SessionSnapshot current, SessionRules rules, Guid session, long revision)
    {
        if (current.SessionId != session || current.Revision != revision) return new(null, "save-stale-revision");
        var availability = Query(definition, current, rules);
        return availability.Allowed ? new(CaptureValues(definition, current, rules, availability.Location!), null) : new(null, availability.Error);
    }

    private static void RequireStable(ScenarioDefinition definition, SessionSnapshot current)
    {
        var story = current.Story;
        if (!current.CanWaitAtInput || story.FieldPlayer is not null || story.EntrySkip is not null ||
            story.Warp is not null || story.ScriptWarp is not null || story.BattleRequest ||
            story.PortraitWindow is not ClosedPortraitWindow || story.LogicalText is not { Open: false } ||
            story.LogicalView is not { Scrolling: false } || story.Typewriting ||
            story.Display is not { Visibility: FullFadeVisibility.BaseRestored } display || display.Base != display.Current)
            throw new BattleRuleException("save-busy", "save.boundary");
        var world = current.Exploration!;
        var player = world.PlayerEntity;
        if (world.Quake != 0 || world.TravelMode != FieldTravelMode.Walking ||
            player.Motion.X % 384 != 0 || player.Motion.Y % 384 != 0 ||
            !world.Definition.Traversal.IsWithinActiveArea(player.Position) || OriginalMapTraversal.IsBlocked(world.Layout, player.Position))
            throw new BattleRuleException("save-player-unsettled", "save.boundary");
        foreach (var entity in world.AllEntities.Where(entity => entity.Visible))
        {
            if (entity.WaitingForSprite || entity.Follower is not null && (entity.Motion.IsMoving || entity.WaitingForMotion))
                throw new BattleRuleException("save-entity-unsettled", "save.boundary");
            // Completed story scripts and autonomous ambient wandering can be discarded.
            if (entity.Actions is not null && !entity.IsScriptIdle &&
                definition.Persistence?.CanDiscardEntityProgram(world, entity) != true)
                throw new BattleRuleException("save-story-motion", "save.boundary");
        }
    }

    internal static SessionResult ObserveMusic(ScenarioDefinition definition, SessionSnapshot current, ObserveFieldMusic observed)
    {
        try
        {
            RequireStable(definition, current);
            var world = current.Exploration!;
            int area = world.Definition.Traversal.SelectActiveArea(world.PlayerEntity.Position)!.OneBasedRecordOrdinal - 1;
            var visuals = definition.Exploration!.Visuals;
            if (observed.Map != world.Map || observed.Area != area || observed.Music.Previous.Count > 9 || visuals is null ||
                !observed.Music.Previous.Prepend(observed.Music.Cue).All(cue => visuals.Audio.TryGetValue(cue, out var audio) && audio.Command < 65))
                throw new BattleRuleException("save-music-observation", "music");
            var old = current.Story.Music;
            if (old is { HistoryBound: true, Battle: false } && old.Map == world.Map && old.Area == area &&
                (old.Cue != observed.Music.Cue || !observed.Music.Previous.Take(old.Previous.Count).SequenceEqual(old.Previous)))
                throw new BattleRuleException("save-music-disagreement", "music");
            if (old is { HistoryBound: true, Battle: false } && old.Map == world.Map && old.Area == area &&
                old.Previous.SequenceEqual(observed.Music.Previous))
                return new(current, [], current.StopReason);
            var previous = Array.AsReadOnly(observed.Music.Previous.ToArray());
            var music = old is { HistoryBound: true, Battle: false } && old.Map == world.Map && old.Area == area
                ? old with { Previous = previous }
                : new MusicProgress(current.ObservationSequence + 1, observed.Music.Cue, previous, world.Map, area, null);
            // Recording native history never resets an already known live clock/completion state.
            // Fresh playback at step zero belongs exclusively to reconstruction.
            var snapshot = new SessionSnapshot(current.SessionId, current.Revision + 1, current.ObservationSequence + 1,
                current.Active, current.Story.Copy(null, music: music), current.StopReason);
            return new(snapshot, [new(snapshot.ObservationSequence, snapshot.Revision, "field-music-observed")], current.StopReason);
        }
        catch (BattleRuleException error)
        { return new(current, [], current.StopReason, new(SessionFailureKind.IllegalCommand, error.Code, error.Field, error.Code)); }
    }

    private static OpeningSaveData CaptureValues(ScenarioDefinition definition, SessionSnapshot current, SessionRules rules, SaveLocation location)
    {
        var world = current.Exploration!;
        var party = world.Party;
        var story = current.Story;
        var actors = party.Actors.Select(actor =>
        {
            var progress = actor.Progress;
            var loadout = actor.SourceLoadout ?? progress?.SourceLoadout;
            var source = definition.Encounters.Values.SelectMany(e => e.Deployments).FirstOrDefault(row => row.Actor == actor.Actor)?.Definition;
            if (progress is null || loadout is null || source?.Growth is null || actor.Exp is null ||
                actor.Kills is null || actor.Defeats is null || actor.ActivationWord is null || actor.Initialized is null)
                throw new BattleRuleException("save-party-unknown", "save.allies");
            return new SavedAlly(actor.Actor.Value, source.Growth.ClassId, actor.Hp, actor.Mp, actor.Exp.Value,
                actor.Kills.Value, actor.Defeats.Value, actor.Status, actor.ActivationWord.Value, actor.Initialized.Value, actor.Absent,
                progress.Level, progress.MaxHp, progress.MaxMp, progress.BaseAttack, progress.Defense, progress.Agility,
                Array.AsReadOnly(loadout.Items.ToArray()), Array.AsReadOnly(loadout.Spells.ToArray()));
        }).ToArray();
        if (party.Gold is null || party.ItemCustody is null || party.ActiveAllies is null || party.NewBattle is null ||
            story.StepCounter is null || story.EgressMap is null || story.TextSettings is null)
            throw new BattleRuleException("save-progress-unknown", "save.party");
        var music = Music(definition, current);
        var anchor = location.Anchor;
        return new(Compatibility(definition, rules), location.Recipe, anchor.Map.Value, anchor.Position.X, anchor.Position.Y,
            anchor.Facing, party.Encounter, Array.AsReadOnly(actors), Array.AsReadOnly(party.ActiveAllies.Select(actor => actor.Value).ToArray()),
            party.MainSeed, party.ThinkingSeed, party.Gold.Value, Array.AsReadOnly(party.ItemCustody.OnceFlags.ToArray()),
            Array.AsReadOnly(party.ItemCustody.Deals.Select(pair => new SavedDeal(pair.Key, pair.Value)).ToArray()),
            Array.AsReadOnly(story.Flags.ToArray()), story.StepCounter.Value, story.EgressMap.Value, story.TextCursor,
            story.TextSettings with { }, story.Display!.Period, music, party.NewBattle.Difficulty,
            party.NewBattle.AllyAutoBattle, party.NewBattle.OpponentControl);
    }

    private static SavedMusic Music(ScenarioDefinition definition, SessionSnapshot current)
    {
        var world = current.Exploration!;
        int area = world.Definition.Traversal.SelectActiveArea(world.PlayerEntity.Position)!.OneBasedRecordOrdinal - 1;
        if (current.Story.Music is { HistoryBound: true, Battle: false } music && music.Map == world.Map && music.Area == area)
            return new(music.Cue, Array.AsReadOnly(music.Previous.ToArray()));
        if (current.Story.Music is { HistoryBound: false } unknown && unknown.Map == world.Map && unknown.Area == area && !unknown.Battle)
            throw new BattleRuleException("save-music-history-unknown", "save.music");
        // No explicit cue in this context: the host uses the admitted map music. Its older context
        // history is not live history and is not carried into the savepoint's new playback generation.
        var visuals = definition.Exploration!.Visuals;
        if (visuals is null || !visuals.Maps.TryGetValue(world.Map, out var map) || area >= map.Music.Count)
            throw new BattleRuleException("save-music-unknown", "save.music");
        var cues = visuals.Audio.Where(pair => pair.Value.Command == map.Music[area].Field).ToArray();
        if (cues.Length != 1) throw new BattleRuleException("save-music-unknown", "save.music");
        return new(cues[0].Key, Array.AsReadOnly(Array.Empty<string>()));
    }
}
