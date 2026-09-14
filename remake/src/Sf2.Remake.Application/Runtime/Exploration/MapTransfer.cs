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
            int AllySprite(int character, int fallback)
            {
                var appearance = population.AllySprites?.FirstOrDefault(row => row.Character == character);
                return appearance is null ? fallback : appearance.JoinedFlag is { } joined && !flags.Contains(joined)
                    ? appearance.UnjoinedSprite!.Value : appearance.Sprite;
            }
            var followers = population.Followers.Where(follower => flags.Contains(follower.Flag))
                .Select(follower => new MapFollowerSpawn(follower.Character, AllySprite(follower.Character, follower.Sprite))).ToArray();
            var allocation = MapEntityAllocator.Allocate(map.Entities.Select(entity => entity.Sprite ??
                throw new BattleRuleException("entity-sprite", "map.entities")).ToArray(), followers,
                population.AllyCount, population.NonAllyStart, population.PlayerSprite);
            EntityRef Reference(int character) => new("entity-" + character.ToString(System.Globalization.CultureInfo.InvariantCulture));
            if (player != Reference(0)) throw new BattleRuleException("population-player", "start.player");
            var slots = allocation.Slots.Select(slot =>
            {
                var source = slot.SourceRecord is { } index ? map.Entities[index] : null;
                var entity = new ExplorationEntity(Reference(slot.Character),
                    EntityMotionState.At(source?.Position ?? position, source?.Facing ?? facing, source?.Speed ?? speed) with
                        { FlagsA = 0xE0, AnimationCounter = (byte)(slot.Slot + 1), WaitTimer = (byte)slot.Slot },
                    source?.Visible ?? true, source?.Actions, Slot: slot.Slot, Sprite: AllySprite(slot.Character, slot.Sprite));
                return slot.FollowerOrder is { } order ? FollowerMotion.Install(entity, order, -24, 0) : entity;
            });
            // Populate a fresh source identity table from its cleared (slot-zero) state.
            // Missing keys after Hide are tombstones; state copies must never refill them.
            var aliases = Enumerable.Range(0, 64).ToDictionary(index => Reference(index < 32 ? index : index + 96), _ => 0);
            foreach (var alias in allocation.Aliases) aliases[Reference(alias.Key)] = alias.Value;
            return MapEventDispatcher.RoofOnLoad(new(map, layout, player, slots, party, aliases: aliases));
        }
        var entities = map.Entities.Where(entity => entity.Entity != player).Select(entity =>
            new ExplorationEntity(entity.Entity, EntityMotionState.At(entity.Position, entity.Facing, entity.Speed) with
                { FlagsA = entity.Obstruction ? (byte)0x20 : (byte)0 }, entity.Visible, entity.Actions, Sprite: entity.Sprite)).ToList();
        entities.Insert(0, new(player, EntityMotionState.At(position, facing, speed), true));
        return MapEventDispatcher.RoofOnLoad(new(map, layout, player, entities, party));
    }

    internal static SessionSnapshot Apply(ScenarioDefinition definition, SessionSnapshot current, MapId map,
        MapPosition position, byte facing, MapLoadMode mode, StoryState continuation, List<SessionObservation> observations)
    {
        var world = current.Exploration ?? throw new BattleRuleException("map-transfer-mode", "map", true);
        if (!definition.Exploration!.Maps.TryGetValue(map, out var target))
            throw new BattleRuleException("missing-map", "map");
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
