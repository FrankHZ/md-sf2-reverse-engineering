using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Persistence;

internal static class OpeningRestore
{
    internal static SessionResult Build(ScenarioDefinition definition, OpeningSaveData data, SessionRules rules)
    {
        var expected = OpeningSave.FieldCompatibility(definition, rules);
        Require(data.Depot is { Count: <= 64 } && data.Depot.All(item => item < 127), "save-depot");
        Require(data.Layer is 0 or 2, "save-player-layer");
        definition.Persistence!.ValidateRegisters(data, definition.Exploration);
        Require(data.TextCursor is >= 0 and <= 65535 && data.FadePeriod > 0 &&
            data.TextSettings is { MessageSpeed: <= 3, ViewSpeed: 0 } &&
            data.X is >= 0 and < 64 && data.Y is >= 0 and < 64 && data.Facing <= 7, "save-range");
        var carry = new FieldPartySnapshot(data.Encounter, data.Allies, data.ActiveAllies, data.MainSeed,
            data.ThinkingSeed, data.Gold, data.OnceFlags, data.Deals, data.Depot, data.Difficulty,
            data.AllyAutoBattle, data.OpponentControl);
        var (party, lists) = carry.Rebuild(definition, data.Flags, data.Names, data, expected);
        var anchor = new FieldReturnAnchor(new(data.Map), new(data.X, data.Y), data.Facing);
        var visuals = definition.Exploration!.Visuals;
        Require(data.Music is { Previous.Count: <= 9 } && visuals is not null &&
            data.Music.Previous.Prepend(data.Music.Cue).All(cue => cue is not null && visuals.Audio.TryGetValue(cue, out var audio) && audio.Command < 65), "save-music-resource");
        if (definition.Persistence.InitialStart(definition, data, party) is { } initial)
        {
            _ = rules.Bind(definition.Encounters.Values, party.Actors);
            return ExplorationDispatcher.Start(definition, initial, rules);
        }
        var location = definition.Persistence!.Select(definition, anchor, data.Flags, anchor, data.Recipe);
        Require(location.Recipe == data.Recipe && location.Anchor == anchor, "save-recipe");
        var raft = data.Raft?.Restore(definition.Exploration);
        var world = definition.Persistence.Rebuild(definition, location, party, data.Flags, raft, out var flags, data.Layer, data.FieldScene);
        Require(world.Definition.BasePalette is { Valid: true }, "save-palette-resource");
        int area = world.Definition.Traversal.SelectActiveArea(anchor.Position)!.OneBasedRecordOrdinal - 1;
        var music = new MusicProgress(1, data.Music.Cue, Array.AsReadOnly(data.Music.Previous.ToArray()), anchor.Map, area,
            visuals!.Audio[data.Music.Cue].ModernEndStep);
        var palette = world.Definition.BasePalette!;
        var story = new StoryState(flags, textCursor: data.TextCursor, partyLists: flags == data.Flags ? lists : MapPartyMembership.Rebuild(flags, definition.Exploration!.PartyFlags!),
            display: new(data.FadePeriod, palette, palette, FullFadeVisibility.BaseRestored), textSettings: data.TextSettings,
            stepCounter: data.StepCounter, egressMap: new(data.EgressMap), music: music, saveAnchor: anchor, names: data.Names, raft: raft);
        story = ExplorationTextRunner.Initialize(world, story);
        _ = rules.Bind(definition.Encounters.Values, party.Actors);
        var snapshot = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveExploration(world), story, SessionStopReason.PlayerInput);
        Require(snapshot.CanWaitAtInput, "save-restore-boundary");
        return new(snapshot, Array.Empty<SessionObservation>(), SessionStopReason.PlayerInput);
    }

    private static void Require(bool valid, string code)
    { if (!valid) throw new BattleRuleException(code, "save"); }
}
