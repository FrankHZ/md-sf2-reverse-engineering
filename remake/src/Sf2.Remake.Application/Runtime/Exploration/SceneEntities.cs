using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class SceneEntities
{
    internal static EntityRef Reference(int character) =>
        new("entity-" + character.ToString(System.Globalization.CultureInfo.InvariantCulture));

    internal static int ResolveAllySprite(ExplorationPopulation population, IReadOnlyList<int> flags, int character, int fallback, BattleStartInput? party = null)
    {
        var appearance = population.AllySprites?.FirstOrDefault(row => row.Character == character);
        return appearance is null ? fallback : appearance.JoinedFlag is { } joined && !flags.Contains(joined)
            ? appearance.UnjoinedSprite!.Value : appearance.ForClass(party?.Actors.FirstOrDefault(a => a.Actor.Value == $"ally-{character}")?.Progress?.ClassId);
    }

    internal static ExplorationState Add(ExplorationState world, AddMapEntity add, long request, IReadOnlyList<int>? flags = null, ProgramLocation? source = null)
    {
        // InitializeNewEntity scans the first 63 signed alias bytes, then allocates max+1.
        if (world.Population is null || !world.Aliases.ContainsKey(add.Entity))
            throw new BattleRuleException("new-entity-reference", "program.entity", true);
        int slot = world.Aliases.Where(pair => pair.Key != new EntityRef("entity-159"))
            .Select(pair => pair.Value).DefaultIfEmpty(0).Max() + 1;
        if (slot >= 49) throw new BattleRuleException("source-entity-capacity", "program.entity", true);
        int sprite = add.Sprite;
        if (add.Entity.Value.StartsWith("entity-", StringComparison.Ordinal) &&
            int.TryParse(add.Entity.Value.AsSpan("entity-".Length), out int member) && member < 30)
            sprite = world.Party.Actors.FirstOrDefault(row => row.Actor.Value == $"ally-{member}")?.Hp == 0
                ? 190 : ResolveAllySprite(world.Population, flags ?? [], member, member, world.Party);
        var entity = new ExplorationEntity(add.Entity, EntityMotionState.At(add.Position, add.Facing, 0) with
            { FlagsA = 0xE0, AnimationCounter = (byte)(slot + 1), WaitTimer = (byte)slot },
            true, add.Actions, Slot: slot, Sprite: sprite,
            SpriteRequest: request, WaitingForSprite: true)
        {
            Origin = source is { } location ? new AllocatedEntityOrigin(location) : null,
            ActionSource = add.Actions is null || source is null ? null : new InitialEntityActions(),
        };
        var aliases = world.Aliases.ToDictionary(pair => pair.Key, pair => pair.Value);
        aliases[add.Entity] = slot;
        // An older visit can retain a link to the previous occupant of this physical slot.
        bool replacing = world.AllEntities.Any(row => row.Slot == slot);
        var entities = world.AllEntities.Where(row => row.Slot != slot)
            .Select(row => replacing && row.Follower?.LeaderSlot == slot ? row with { Follower = null } : row)
            .Append(entity);
        return new(world.Definition, world.Layout, world.Player, entities, world.Party,
            world.SpriteSize, aliases, world.RoofState, world.Population, world.Quake, world.TravelMode, world.CurrentEntities, populationSource: world.PopulationSource);
    }

    internal static ExplorationState Build(ExplorationMapDefinition map, WorkingMapLayout layout, EntityRef player,
        MapPosition position, byte facing, ushort speed, BattleStartInput party, IReadOnlyList<int> flags,
        ExplorationPopulation population, IReadOnlyList<ExplorationEntityDefinition> records,
        MapBlockCopyLifecycleState? roofState = null, FieldPopulationSource? populationSource = null)
    {
        int AllySprite(int character, int fallback) => ResolveAllySprite(population, flags, character, fallback, party);
        var followers = population.Followers.Where(follower => flags.Contains(follower.Flag))
            .Select(follower => new MapFollowerSpawn(follower.Character, AllySprite(follower.Character, follower.Sprite))).ToArray();
        var allocation = MapEntityAllocator.Allocate(records.Select(entity => entity.Sprite ??
            throw new BattleRuleException("entity-sprite", "scene.entities")).ToArray(), followers,
            population.AllyCount, population.NonAllyStart, population.PlayerSprite);
        if (allocation.Slots.Count > 49) throw new BattleRuleException("source-entity-capacity", "scene.entities", true);
        if (player != Reference(0)) throw new BattleRuleException("population-player", "start.player");
        bool caravan = CaravanRuntime.HasOverworldPopulation(population) && flags.Contains(65);
        var slots = allocation.Slots.Select(slot =>
        {
            var source = slot.SourceRecord is { } index ? records[index] : null;
            var entity = new ExplorationEntity(Reference(slot.Character),
                EntityMotionState.At(source?.Position ?? position, source?.Facing ?? facing, source?.Speed ?? speed) with
                    { FlagsA = 0xE0, AnimationCounter = (byte)(slot.Slot + 1), WaitTimer = (byte)slot.Slot },
                source?.Visible ?? true, source?.Actions, Slot: slot.Slot, Sprite: AllySprite(slot.Character, slot.Sprite),
                SharedSpriteSlot: slot.Sprite >= 240 ? 32 : null)
            {
                Origin = slot.SourceRecord is { } resident ? new ResidentEntityOrigin(resident)
                    : slot.Slot == 0 ? new PlayerEntityOrigin() : new FollowerEntityOrigin(slot.Character),
                ActionSource = source?.Actions is null ? null : new InitialEntityActions(),
            };
            if (slot.FollowerOrder is not { } order) return entity;
            return caravan ? CaravanRuntime.InstallPopulationFollower(entity, order, true)
                : FollowerMotion.Install(entity, order, -24, 0);
        });
        // Only fresh allocation initializes zero entries. Copies retain removed-reference tombstones.
        var aliases = Enumerable.Range(0, 64).ToDictionary(index => Reference(index < 32 ? index : index + 96), _ => 0);
        foreach (var alias in allocation.Aliases) aliases[Reference(alias.Key)] = alias.Value;
        return new(map, layout, player, slots, party, aliases: aliases, roofState: roofState, population: population,
            currentEntities: allocation.Slots.Where(slot => slot.Slot == 0 || slot.FollowerOrder is not null).Select(slot => slot.Slot).ToArray(),
            populationSource: populationSource);
    }

    internal static ExplorationState Reload(ExplorationState world, LoadSceneEntities load, IReadOnlyList<int> flags, long request, FieldPopulationSource? source = null)
    {
        request = Math.Max(request, checked(world.AllEntities.Max(entity => entity.SpriteRequest) + 1));
        var next = Build(world.Definition, world.Layout, world.Player, load.PlayerPosition, load.Facing,
            world.PlayerEntity.Motion.XSpeed, world.Party, flags, load.Population, load.Entities, world.RoofState, source);
        // LoadEntityMapsprites completes for the new set before the script can use it.
        return next.WithEntities(next.AllEntities.Select(entity => entity with
            { SpriteRequest = request, SpriteReady = 0, WaitingForSprite = true }));
    }
}
