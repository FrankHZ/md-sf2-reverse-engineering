using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class MapTransfer
{
    internal static ExplorationState Build(ExplorationMapDefinition map, EntityRef player, MapPosition position,
        byte facing, ushort speed, BattleStartInput party, IReadOnlyList<int> flags)
    {
        ValidateSetup(map, flags);
        if (!map.Traversal.IsWithinActiveArea(position) || OriginalMapTraversal.IsBlocked(map.Layout, position))
            throw new BattleRuleException("map-entry-position", "map.position");
        var entities = map.Entities.Where(entity => entity.Entity != player).Select(entity =>
            new ExplorationEntity(entity.Entity, EntityMotionState.At(entity.Position, entity.Facing, entity.Speed) with
                { FlagsA = entity.Obstruction ? (byte)0x20 : (byte)0 }, entity.Visible)).ToList();
        entities.Insert(0, new(player, EntityMotionState.At(position, facing, speed), true));
        return new(map, map.Layout, player, entities, party);
    }

    internal static SessionSnapshot Apply(ScenarioDefinition definition, SessionSnapshot current, MapId map,
        MapPosition position, byte facing, MapLoadMode mode, StoryState continuation, List<SessionObservation> observations)
    {
        var world = current.Exploration ?? throw new BattleRuleException("map-transfer-mode", "map", true);
        if (!definition.Exploration!.Maps.TryGetValue(map, out var target))
            throw new BattleRuleException("missing-map", "map");
        ExplorationState next;
        if (mode == MapLoadMode.Preserve)
        {
            ValidateSetup(target, current.Story.Flags);
            if (map != world.Map) throw new BattleRuleException("preserve-other-map", "map.loadMode", true);
            if (!target.Traversal.IsWithinActiveArea(position) || OriginalMapTraversal.IsBlocked(world.Layout, position))
                throw new BattleRuleException("map-entry-position", "map.position");
            next = world.WithEntity(world.PlayerEntity with
            { Motion = EntityMotionState.At(position, facing, world.PlayerEntity.Motion.XSpeed), Actions = null, ActionCursor = 0 });
        }
        else next = Build(target, world.Player, position, facing, world.PlayerEntity.Motion.XSpeed, world.Party, current.Story.Flags);
        var story = continuation.Copy(continuation.Cursor,
            continuation: continuation.EnteringBattle is null ? ProgramContinuation.MapLoaded : continuation.Continuation,
            returnAnchor: continuation.EnteringBattle is null ? new(world.Map, world.PlayerEntity.Position, world.PlayerEntity.Motion.Facing) : null);
        if (target.OnLoad is { } onLoad)
            story = story.Copy(onLoad, callers: story.Cursor is { } caller ? story.Callers.Append(caller) : story.Callers);
        return ProgramRunner.Commit(current, new ActiveExploration(next), story, observations, "map-transferred", map.Value);
    }

    internal static SessionSnapshot Continue(ScenarioDefinition definition, SessionSnapshot current, List<SessionObservation> observations)
    {
        var story = current.Story;
        var world = current.Exploration ?? throw new BattleRuleException("entry-mode", "entry");
        if (story.Continuation == ProgramContinuation.MapLoaded)
        {
            var route = world.Definition.Battle;
            if (route is null || (route.UnlockedFlag is { } unlocked && !story.Flags.Contains(unlocked)) ||
                (route.CompletedFlag is { } completed && story.Flags.Contains(completed)))
                return current.WithStory(story.Copy(null, continuation: ProgramContinuation.FieldInput));
            bool introSeen = route.IntroFlag is { } intro && story.Flags.Contains(intro);
            return ProgramRunner.Commit(current, current.Active,
                story.Copy(introSeen ? null : route.BeforeProgram, continuation: ProgramContinuation.BeforeBattleFinished,
                    enteringBattle: route), observations, "battle-selected", route.Encounter);
        }
        var selected = story.EnteringBattle ?? throw new BattleRuleException("entry-route", "entry");
        if (!definition.Encounters.TryGetValue(selected.Encounter, out var encounter))
            throw new BattleRuleException("missing-encounter", "entry.encounter");
        var input = new BattleStartInput(selected.Encounter, world.Party.Actors, world.Party.MainSeed,
            world.Party.ThinkingSeed, world.Party.Gold,
            world.Party.NewBattle is { } policy ? policy with { SkipIntro = false, BeforeBattleRouted = true } : null);
        var battle = BattleTurnFlow.Start(encounter, input);
        var initialized = ProgramRunner.Commit(current, new ActiveBattle(battle, null), story, observations, "battle-initialized");
        bool skipStart = selected.IntroFlag is { } flag && story.Flags.Contains(flag);
        var startStory = story.Copy(skipStart ? null : selected.StartProgram,
            flags: selected.IntroFlag is { } introFlag ? ProgramRunner.Flags(story, introFlag, true) : story.Flags,
            continuation: ProgramContinuation.BattleStartFinished);
        return ProgramRunner.Commit(initialized, initialized.Active, startStory, observations, "battle-loaded");
    }

    private static void ValidateSetup(ExplorationMapDefinition map, IReadOnlyList<int> flags)
    {
        if (map.Setup is not { } route) return;
        var selected = MapSetupSelector.Select(route, route.DefaultSetup,
            flag => flags.Contains(int.Parse(flag.Value, System.Globalization.CultureInfo.InvariantCulture)));
        if (selected != route.DefaultSetup)
            throw new BattleRuleException("map-setup", selected.Value, true);
    }
}
