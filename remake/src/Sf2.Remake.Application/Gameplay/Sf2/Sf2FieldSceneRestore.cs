using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Gameplay.Sf2;

// Shared settled-scene mechanics. Each SF2 consumer admits its own context/population.
internal static class Sf2FieldSceneRestore
{
    internal const string VoidSetup = "ms-void";

    internal static bool MatchesLoadedSetup(ExplorationMapDefinition source, ExplorationMapDefinition loaded, string setup)
    {
        if (setup == source.Setup?.DefaultSetup.Value || setup == VoidSetup && source.Setup is null)
            return Matches(source.Entities, source.Events, source.OnLoad, source.InputProgram, source.Population, source.Search);
        return source.Setups.TryGetValue(new(setup), out var body) &&
            Matches(body.Entities, body.Events, body.OnLoad, body.InputProgram, body.Population, body.Search);

        bool Matches(IReadOnlyList<ExplorationEntityDefinition> entities, IReadOnlyList<ExplorationEvent> events,
            ProgramLocation? onLoad, ProgramLocation? input, ExplorationPopulation? population, FieldSearchDefinition? search) =>
            loaded.OnLoad == onLoad && loaded.InputProgram == input && loaded.Population == population &&
            loaded.Search == search && loaded.Entities.SequenceEqual(entities) && loaded.Events.SequenceEqual(events);
    }

    internal static void RequireUnoccupiedLanding(ExplorationState world)
    {
        var player = world.PlayerEntity;
        if ((player.Motion.FlagsA & 0x20) != 0 && EntityMotion.FieldObstructed(player.Motion.X, player.Motion.Y,
            world.AllEntities.Where(actor => actor.Visible && actor.Slot != player.Slot).Select(actor => actor.Motion)))
            throw new BattleRuleException("save-anchor-occupied", "save.anchor");
    }

    internal static SavedFieldScene Capture(ScenarioDefinition definition, ExplorationState world, string setup)
    {
        var source = definition.Exploration!.Maps[world.Map];
        var actors = world.AllEntities.Select(entity =>
        {
            var motion = entity.Motion;
            var follow = entity.Follower;
            return new SavedFieldActor(entity.Entity.Value, entity.Slot, motion.X, motion.Y, motion.Facing,
                motion.Layer, motion.FlagsA, motion.FlagsB, motion.XSpeed, motion.YSpeed, entity.Visible,
                entity.Sprite ?? -1, entity.RenderSize, entity.Priority, follow?.LeaderSlot ?? -1,
                follow?.OffsetX ?? 0, follow?.OffsetY ?? 0, follow?.WaitForDestination ?? true);
        }).ToArray();
        var changed = world.Layout.Words.Select((word, index) => new SavedLayoutCell(index, word))
            .Where(cell => cell.Value != source.Layout.Words[cell.Index]).ToArray();
        return new(setup, Array.AsReadOnly(actors),
            new ReadOnlyDictionary<string, int>(world.Aliases.ToDictionary(pair => pair.Key.Value, pair => pair.Value)),
            Array.AsReadOnly(world.CurrentEntities.ToArray()), Array.AsReadOnly(changed));
    }

    internal static ExplorationState Rebuild(ScenarioDefinition definition, SaveLocation location, BattleStartInput party,
        IReadOnlyList<int> flags, SavedFieldScene scene, out IReadOnlyList<int> enteredFlags, byte layer,
        StoryProgram? allocationProgram = null)
    {
        var anchor = location.Anchor;
        var source = definition.Exploration!.Maps[anchor.Map];
        var map = source.BindSetup(scene.Setup == VoidSetup && source.Setup is null ? source.Body.Setup : new(scene.Setup));
        var population = map.Population ?? throw new BattleRuleException("save-field-scene-population", "save.fieldScene");
        Require(scene.Actors is { Count: > 0 and <= 49 } && scene.Aliases is { Count: <= 64 } &&
            scene.CurrentEntities is { Count: > 0 and <= 49 } && scene.Layout is { Count: <= 4096 }, "save-field-scene-shape");
        var slots = scene.Actors.Select(actor => actor.Slot).ToHashSet();
        Require(slots.Count == scene.Actors.Count && slots.SetEquals(Enumerable.Range(0, slots.Count)) &&
            scene.CurrentEntities.Distinct().Count() == scene.CurrentEntities.Count && scene.CurrentEntities.All(slots.Contains) &&
            scene.CurrentEntities.Contains(0), "save-field-scene-slots");
        bool Reference(string id) => id.StartsWith("entity-", StringComparison.Ordinal) && int.TryParse(id.AsSpan(7), out int number) &&
            (number is >= 0 and < 32 or >= 128 and < 160) && id == "entity-" + number;
        Require(scene.Aliases.All(pair => Reference(pair.Key) && slots.Contains(pair.Value)) &&
            scene.Aliases.GetValueOrDefault("entity-0", -1) == 0 && scene.Actors.All(actor => Reference(actor.Entity)), "save-field-scene-alias");
        var residents = map.Entities.Select((row, index) => (row, index)).ToLookup(pair => pair.row.Entity);
        var allocations = (allocationProgram?.Instructions ?? []).Select((instruction, index) => (instruction, index))
            .Where(pair => pair.instruction is AddMapEntity).ToLookup(pair => ((AddMapEntity)pair.instruction).Entity);
        var followers = population.Followers.ToLookup(row => SceneEntities.Reference(row.Character));
        var player = scene.Actors.Single(actor => actor.Slot == 0);
        Require(player.Entity == "entity-0" && player.X == anchor.Position.X * 384 && player.Y == anchor.Position.Y * 384 &&
            player.Facing == anchor.Facing && player.Layer == layer && player.Visible && player.LeaderSlot == -1, "save-field-scene-player");
        Require(scene.Layout.Select(cell => cell.Index).Distinct().Count() == scene.Layout.Count &&
            scene.Layout.All(cell => cell.Index is >= 0 and < 4096), "save-field-scene-layout");
        var words = source.Layout.Words.ToArray();
        foreach (var cell in scene.Layout) words[cell.Index] = cell.Value;
        var layout = new WorkingMapLayout(words);
        var entities = scene.Actors.Select(actor =>
        {
            Require(actor.Facing <= 7 && actor.Layer is 0 or 2 && actor.SpeedX <= 256 && actor.SpeedY <= 256 &&
                actor.RenderSize <= 24 && actor.Sprite is >= 0 and <= 255 &&
                actor.LeaderSlot >= -1 && (actor.LeaderSlot == -1 || slots.Contains(actor.LeaderSlot) && actor.LeaderSlot != actor.Slot) &&
                actor.OffsetX is >= -24 and <= 24 && actor.OffsetY is >= -24 and <= 24, "save-field-scene-actor");
            Require(definition.Exploration.Visuals?.Sprites.ContainsKey(actor.Sprite) != false, "save-field-scene-sprite");
            var motion = EntityMotionState.At(new(0, 0), actor.Facing, actor.SpeedX) with
            {
                X = actor.X, Y = actor.Y, XDestination = actor.X, YDestination = actor.Y,
                YSpeed = actor.SpeedY, FlagsA = actor.FlagsA, FlagsB = actor.FlagsB, Layer = actor.Layer,
            };
            var entity = new EntityRef(actor.Entity);
            var resident = residents[entity].ToArray();
            var follower = followers[entity].ToArray();
            var allocated = allocations[entity].ToArray();
            Require(actor.Slot == 0 || resident.Length + follower.Length + allocated.Length > 0, "save-field-scene-actor");
            // Bind the reconstruction we can prove. If the old record is ambiguous, it remains
            // readable but cannot present an invented origin for the next Save.
            FieldEntityOrigin? origin = actor.Slot == 0 ? new PlayerEntityOrigin() :
                (resident.Length, follower.Length, allocated.Length) switch
                {
                    (1, 0, 0) => new ResidentEntityOrigin(resident[0].index),
                    (0, 1, 0) => new FollowerEntityOrigin(follower[0].Character),
                    (0, 0, 1) => new AllocatedEntityOrigin(new(allocationProgram!.Id, allocated[0].index)),
                    _ => null,
                };
            // Keep the old reader's reconstructed ambient program. Its phase begins here;
            // historic current-action cursors do not exist in this wire format.
            var ambient = actor.LeaderSlot == -1 && actor.Visible && actor.X < 63 * 384 && actor.Y < 63 * 384 && resident.Length != 0
                ? resident[0].row.Actions : null;
            return new ExplorationEntity(entity, motion, actor.Visible, ambient, Slot: actor.Slot,
                Sprite: actor.Sprite, Follower: actor.LeaderSlot == -1 ? null :
                    new(actor.LeaderSlot, actor.OffsetX, actor.OffsetY, actor.WaitForDestination),
                Priority: actor.Priority, RenderSize: actor.RenderSize)
            {
                Origin = origin,
                ActionSource = ambient is null ? null : new InitialEntityActions(),
            };
        }).ToArray();
        // An invalid graph cannot be consumed by following, even if its actors are hidden.
        foreach (var actor in scene.Actors)
        {
            var seen = new HashSet<int> { actor.Slot };
            int leader = actor.LeaderSlot;
            while (leader >= 0)
            {
                Require(seen.Add(leader), "save-field-scene-follow-cycle");
                leader = scene.Actors.Single(row => row.Slot == leader).LeaderSlot;
            }
        }
        var entered = flags.ToHashSet();
        foreach (var write in map.EntryFlags)
            if (write.Value) entered.Add(write.Flag); else entered.Remove(write.Flag);
        enteredFlags = Array.AsReadOnly(entered.Order().ToArray());
        var world = new ExplorationState(map, layout, new("entity-0"), entities, party,
            aliases: scene.Aliases.ToDictionary(pair => new EntityRef(pair.Key), pair => pair.Value),
            population: population, currentEntities: scene.CurrentEntities);
        Require(map.Traversal.IsWithinActiveArea(anchor.Position) &&
            FieldLayout.Word(world, world.PlayerEntity.Motion, anchor.Position.X, anchor.Position.Y) < 0xC000,
            "save-anchor-unusable");
        return MapEventDispatcher.RoofOnLoad(world);
    }

    private static void Require(bool valid, string code)
    { if (!valid) throw new BattleRuleException(code, "save.fieldScene"); }
}
