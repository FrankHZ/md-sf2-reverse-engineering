using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class SceneEntities
{
    internal static ExplorationState Build(ExplorationMapDefinition map, WorkingMapLayout layout, EntityRef player,
        MapPosition position, byte facing, ushort speed, BattleStartInput party, IReadOnlyList<int> flags,
        ExplorationPopulation population, IReadOnlyList<ExplorationEntityDefinition> records,
        MapBlockCopyLifecycleState? roofState = null)
    {
        int AllySprite(int character, int fallback)
        {
            var appearance = population.AllySprites?.FirstOrDefault(row => row.Character == character);
            return appearance is null ? fallback : appearance.JoinedFlag is { } joined && !flags.Contains(joined)
                ? appearance.UnjoinedSprite!.Value : appearance.Sprite;
        }
        var followers = population.Followers.Where(follower => flags.Contains(follower.Flag))
            .Select(follower => new MapFollowerSpawn(follower.Character, AllySprite(follower.Character, follower.Sprite))).ToArray();
        var allocation = MapEntityAllocator.Allocate(records.Select(entity => entity.Sprite ??
            throw new BattleRuleException("entity-sprite", "scene.entities")).ToArray(), followers,
            population.AllyCount, population.NonAllyStart, population.PlayerSprite);
        if (allocation.Slots.Count > 49) throw new BattleRuleException("source-entity-capacity", "scene.entities", true);
        EntityRef Reference(int character) => new("entity-" + character.ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (player != Reference(0)) throw new BattleRuleException("population-player", "start.player");
        var slots = allocation.Slots.Select(slot =>
        {
            var source = slot.SourceRecord is { } index ? records[index] : null;
            var entity = new ExplorationEntity(Reference(slot.Character),
                EntityMotionState.At(source?.Position ?? position, source?.Facing ?? facing, source?.Speed ?? speed) with
                    { FlagsA = 0xE0, AnimationCounter = (byte)(slot.Slot + 1), WaitTimer = (byte)slot.Slot },
                source?.Visible ?? true, source?.Actions, Slot: slot.Slot, Sprite: AllySprite(slot.Character, slot.Sprite));
            return slot.FollowerOrder is { } order ? FollowerMotion.Install(entity, order, -24, 0) : entity;
        });
        // Only fresh allocation initializes zero entries. Copies retain removed-reference tombstones.
        var aliases = Enumerable.Range(0, 64).ToDictionary(index => Reference(index < 32 ? index : index + 96), _ => 0);
        foreach (var alias in allocation.Aliases) aliases[Reference(alias.Key)] = alias.Value;
        return new(map, layout, player, slots, party, aliases: aliases, roofState: roofState, population: population);
    }

    internal static ExplorationState Reload(ExplorationState world, LoadSceneEntities load, IReadOnlyList<int> flags, long request)
    {
        request = Math.Max(request, checked(world.AllEntities.Max(entity => entity.SpriteRequest) + 1));
        var next = Build(world.Definition, world.Layout, world.Player, load.PlayerPosition, load.Facing,
            world.PlayerEntity.Motion.XSpeed, world.Party, flags, load.Population, load.Entities, world.RoofState);
        // LoadEntityMapsprites completes for the new set before the script can use it.
        return next.WithEntities(next.AllEntities.Select(entity => entity with
            { SpriteRequest = request, SpriteReady = 0, WaitingForSprite = true }));
    }
}
