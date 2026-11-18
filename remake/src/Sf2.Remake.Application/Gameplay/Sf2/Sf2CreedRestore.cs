using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Gameplay.Sf2;

// Flags select the next population, not the actors still alive in the current room.
// Preserve this finite room's settled progress without serializing executable actions.
internal static class Sf2CreedRestore
{
    internal static SavedFieldScene? Capture(ScenarioDefinition definition, ExplorationState world, IReadOnlyList<int> flags)
    {
        if (world.Map.Value != "map-33" || !flags.Any(flag => flag is 784 or 785 or 786 or 787)) return null;
        var source = definition.Exploration!.Maps[world.Map];
        var selected = source.Setups.FirstOrDefault(pair => pair.Value.Entities.SequenceEqual(world.Definition.Entities));
        string setup = selected.Key?.Value ?? source.Setup?.DefaultSetup.Value ?? "";
        Require(setup is "ms-map33-flag784" or "ms-map33-flag786", "save-field-scene-population");
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
        IReadOnlyList<int> flags, SavedFieldScene scene, out IReadOnlyList<int> enteredFlags, byte layer)
    {
        var anchor = location.Anchor;
        Require(anchor.Map.Value == "map-33" && location.Recipe == "creed-mansion" && !flags.Contains(22), "save-field-scene-context");
        var source = definition.Exploration!.Maps[anchor.Map];
        Require(scene.Setup is "ms-map33-flag784" or "ms-map33-flag786" &&
            source.Setups.ContainsKey(new(scene.Setup)), "save-field-scene-population");
        var setup = source.Setups[new(scene.Setup)];
        var map = new ExplorationMapDefinition(source.Map, source.Layout, source.Traversal, setup.Entities,
            setup.Events, setup.OnLoad, source.Battle, setup.InputProgram, population: setup.Population,
            layoutEvents: source.LayoutEvents, overlays: source.OverlayOffsets, entryFlags: source.EntryFlags,
            basePalette: source.BasePalette, viewAreas: source.ViewAreas, battles: source.Battles, search: setup.Search);
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
        var identities = setup.Entities.Select(row => row.Entity.Value)
            .Concat(setup.Population.Followers.Select(row => "entity-" + row.Character)).Append("entity-0").ToHashSet();
        Require(scene.Actors.All(actor => identities.Contains(actor.Entity)), "save-field-scene-actor");
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
            // Completed scene actions remain completed. Only a source resident's ordinary
            // ambient action restarts, and never over a follower, hidden or parked actor.
            var ambient = actor.LeaderSlot == -1 && actor.Visible && actor.X < 63 * 384 && actor.Y < 63 * 384
                ? setup.Entities.FirstOrDefault(row => row.Entity.Value == actor.Entity)?.Actions : null;
            return new ExplorationEntity(new(actor.Entity), motion, actor.Visible, ambient, Slot: actor.Slot,
                Sprite: actor.Sprite, Follower: actor.LeaderSlot == -1 ? null :
                    new(actor.LeaderSlot, actor.OffsetX, actor.OffsetY, actor.WaitForDestination),
                Priority: actor.Priority, RenderSize: actor.RenderSize);
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
            population: setup.Population, currentEntities: scene.CurrentEntities);
        Require(map.Traversal.IsWithinActiveArea(anchor.Position) &&
            FieldLayout.Word(world, world.PlayerEntity.Motion, anchor.Position.X, anchor.Position.Y) < 0xC000,
            "save-anchor-unusable");
        return MapEventDispatcher.RoofOnLoad(world);
    }

    private static void Require(bool valid, string code)
    { if (!valid) throw new BattleRuleException(code, "save.fieldScene"); }
}
