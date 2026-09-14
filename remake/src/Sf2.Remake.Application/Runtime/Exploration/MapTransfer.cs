using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class MapTransfer
{
    internal static ExplorationState Build(ExplorationMapDefinition map, EntityRef player, MapPosition position,
        byte facing, ushort speed, BattleStartInput party, IReadOnlyList<int> flags, IReadOnlyList<int>? layoutFlags = null)
    {
        ValidateSetup(map, flags);
        var layout = map.Layout;
        foreach (var copy in map.LayoutEvents?.Flags ?? [])
            if ((layoutFlags ?? flags).Contains(copy.Flag)) layout = layout.ApplyBlockCopy(copy.Copy);
        if (!map.Traversal.IsWithinActiveArea(position) || OriginalMapTraversal.IsBlocked(layout, position))
            throw new BattleRuleException("map-entry-position", "map.position");
        if (map.Population is { } population)
        {
            return MapEventDispatcher.RoofOnLoad(SceneEntities.Build(map, layout, player, position, facing,
                speed, party, flags, population, map.Entities));
        }
        var entities = map.Entities.Where(entity => entity.Entity != player).Select(entity =>
            new ExplorationEntity(entity.Entity, EntityMotionState.At(entity.Position, entity.Facing, entity.Speed) with
                { FlagsA = entity.Obstruction ? (byte)0x20 : (byte)0 }, entity.Visible, entity.Actions, Sprite: entity.Sprite)).ToList();
        entities.Insert(0, new(player, EntityMotionState.At(position, facing, speed), true));
        return MapEventDispatcher.RoofOnLoad(new(map, layout, player, entities, party));
    }

    internal static ExplorationState LoadScene(ExplorationDefinition definition, ExplorationState world, LoadSceneMap load)
    {
        if (!definition.Maps.TryGetValue(load.Map, out var map)) throw new BattleRuleException("missing-map", "scene.map");
        // csc48 changes the displayed layout/view; entity allocation, map init and battle selection
        // are separate calls. Keep the running program, party and physical entities intact here.
        return new(map, map.Layout, world.Player, world.AllEntities, world.Party, world.SpriteSize,
            world.Aliases, population: world.Population);
    }

    internal static SessionSnapshot Apply(ScenarioDefinition definition, SessionSnapshot current, MapId map,
        MapPosition position, byte facing, MapLoadMode mode, StoryState continuation, List<SessionObservation> observations)
    {
        var world = current.Exploration ?? throw new BattleRuleException("map-transfer-mode", "map", true);
        if (!definition.Exploration!.Maps.TryGetValue(map, out var target))
            throw new BattleRuleException("missing-map", "map");
        // MainLoop selects BattleLoop before ExplorationLoop. The before-battle script owns
        // its map load and entity replacement; retain the actual field scene until those calls.
        if (world.Party.NewBattle is not null && target.Battle is { } battle &&
            (battle.UnlockedFlag is null || current.Story.Flags.Contains(battle.UnlockedFlag.Value)) &&
            (battle.CompletedFlag is null || !current.Story.Flags.Contains(battle.CompletedFlag.Value)))
            return BattleEntry.Select(current, continuation.Copy(continuation.Cursor,
                returnAnchor: new(world.Map, world.PlayerEntity.Position, world.PlayerEntity.Motion.Facing)), battle, observations);
        ExplorationState next;
        var flags = current.Story.Flags;
        if (mode == MapLoadMode.Preserve)
        {
            ValidateSetup(target, current.Story.Flags);
            if (map != world.Map) throw new BattleRuleException("preserve-other-map", "map.loadMode", true);
            if (!target.Traversal.IsWithinActiveArea(position) || OriginalMapTraversal.IsBlocked(world.Layout, position))
                throw new BattleRuleException("map-entry-position", "map.position");
            next = world.WithEntity(world.PlayerEntity with
            { Motion = world.PlayerEntity.Motion with { X = (short)(position.X * 384), Y = (short)(position.Y * 384),
                XDestination = (short)(position.X * 384), YDestination = (short)(position.Y * 384), Facing = facing }, Actions = null, ActionCursor = 0 });
            next = MapEventDispatcher.RoofOnLoad(next);
        }
        else
        {
            var enteredFlags = flags.ToHashSet();
            foreach (var write in target.EntryFlags)
                if (write.Value) enteredFlags.Add(write.Flag); else enteredFlags.Remove(write.Flag);
            flags = enteredFlags.Order().ToArray();
            // Source chooses/populates the setup before clearing its temporary flags, then loads layout/init.
            next = Build(target, world.Player, position, facing, world.PlayerEntity.Motion.XSpeed, world.Party, current.Story.Flags, flags);
        }
        var story = continuation.Copy(continuation.Cursor,
            flags: flags,
            continuation: continuation.EnteringBattle is null ? ProgramContinuation.MapLoaded : continuation.Continuation,
            returnAnchor: continuation.EnteringBattle is null ? new(world.Map, world.PlayerEntity.Position, world.PlayerEntity.Motion.Facing) : null);
        if (target.OnLoad is { } onLoad)
            story = story.Copy(onLoad, callers: story.Cursor is { } caller ? story.Callers.Append(caller) : story.Callers);
        return ProgramRunner.Commit(current, new ActiveExploration(next), story, observations, "map-transferred", map.Value);
    }

    internal static SessionSnapshot Continue(ScenarioDefinition definition, SessionSnapshot current, List<SessionObservation> observations)
        => BattleEntry.Continue(definition, current, observations);

    private static void ValidateSetup(ExplorationMapDefinition map, IReadOnlyList<int> flags)
    {
        if (map.Setup is not { } route) return;
        var selected = MapSetupSelector.Select(route, route.DefaultSetup,
            flag => flags.Contains(int.Parse(flag.Value, System.Globalization.CultureInfo.InvariantCulture)));
        if (selected != route.DefaultSetup)
            throw new BattleRuleException("map-setup", selected.Value, true);
    }
}
