using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Persistence;

internal sealed record FieldVisitActor(EntityRef Entity, int Slot, FieldEntityOrigin Origin, EntityMotionState Motion,
    EntityFollower? Follower, FieldActionSource? ActionSource, int ActionCursor, bool WaitingForMotion,
    bool Visible, int? Sprite, int? SharedSpriteSlot, ushort RenderSize, bool Priority, bool Shadowless);

internal sealed record FieldVisitRoof(int RecordOrdinal, IReadOnlyList<ushort> SavedWords);

// The physical part of a visit. Carried party and story control are supplied by their owners.
internal sealed record FieldVisitSnapshot(FieldMapBody Body, FieldPopulationSource Population, EntityRef Player,
    IReadOnlyList<FieldVisitActor> Actors, IReadOnlyDictionary<EntityRef, int> Aliases, IReadOnlyList<int> CurrentEntities,
    IReadOnlyList<SavedLayoutCell> Layout, FieldVisitRoof? Roof, ushort SpriteSize,
    IReadOnlyDictionary<int, ProgramLocation> Graphics, RaftBerth? Berth)
{
    internal static FieldVisitSnapshot Capture(SessionSnapshot current)
    {
        if (!current.CanWaitAtInput || !current.HasOrdinaryFieldTime || current.Exploration is not
            { TravelMode: FieldTravelMode.Walking, Quake: 0, Definition.HasBattleSceneArea: false } world)
            throw new BattleRuleException("field-visit-mode", "fieldVisit");

        var actors = world.AllEntities.Select(entity =>
        {
            Require(entity.Origin is not null, "actor-origin");
            Require(!entity.WaitingForSprite, "sprite-pending");
            Require((entity.Actions is null) == (entity.ActionSource is null), "action-origin");
            Require(entity.Actions is null || entity.IsScriptIdle || entity.Motion.X >= 0x7000, "action-pending");
            return new FieldVisitActor(entity.Entity, entity.Slot, entity.Origin!, entity.Motion,
                entity.Follower, entity.ActionSource, entity.ActionCursor, entity.WaitingForMotion,
                entity.Visible, entity.Sprite, entity.SharedSpriteSlot, entity.RenderSize, entity.Priority,
                entity.ShadowRemovedAt == entity.SpriteRequest);
        }).ToArray();

        var changed = Enumerable.Range(0, WorkingMapLayout.WordCount)
            .Where(index => world.Layout.GetWord(index) != world.Definition.Layout.GetWord(index))
            .Select(index => new SavedLayoutCell(index, world.Layout.GetWord(index))).ToArray();
        var roof = world.RoofState is MapBlockCopyLifecycleActiveState active
            ? new FieldVisitRoof(active.RecordOrdinal, Array.AsReadOnly(active.SavedWords.ToArray())) : null;
        return new(world.Definition.Body, world.PopulationSource, world.Player,
            Array.AsReadOnly(actors), world.Aliases, world.CurrentEntities, Array.AsReadOnly(changed), roof,
            world.SpriteSize, new ReadOnlyDictionary<int, ProgramLocation>(
                world.GraphicBindings.ToDictionary(pair => pair.Key, pair => pair.Value.Source)), current.Story.Raft);
    }

    internal ExplorationState Rebuild(ExplorationDefinition content, BattleStartInput party)
    {
        var bindings = new FieldVisitBindings(content);
        var map = bindings.Map(Body);
        var (population, residents) = bindings.Population(Population);
        var slots = Actors.Select(actor => actor.Slot).ToHashSet();
        Require(slots.Count == Actors.Count && slots.Contains(0) &&
            slots.All(slot => slot >= 0 && slot < (population is null ? Math.Max(RaftRuntime.Slot + 1, residents.Count + 1) : 49)), "slots");
        Require(Aliases.TryGetValue(Player, out int playerSlot) && playerSlot == 0 &&
            (population is null || Player == SceneEntities.Reference(0)) &&
            Aliases.All(pair => slots.Contains(pair.Value) && (population is null || SourceAlias(pair.Key))), "aliases");
        Require(CurrentEntities.Distinct().Count() == CurrentEntities.Count && CurrentEntities.All(slots.Contains) &&
            (population is null || CurrentEntities.Count > 0 && CurrentEntities[0] == 0), "current-entities");
        Require(Actors.Count(actor => actor.Origin is RaftEntityOrigin) <= 1 &&
            (Berth is null || content.Maps.ContainsKey(Berth.Map) &&
                Berth.Position.X is >= 0 and < 64 && Berth.Position.Y is >= 0 and < 64), "raft");

        var entities = Actors.Select(actor =>
        {
            var initial = bindings.InitialActions(actor.Origin, actor.Entity, actor.Slot, Player, population, residents);
            Require(actor.Motion.Facing <= 7 && actor.Motion.Layer is 0 or 2 &&
                actor.RenderSize <= 24 && (actor.Sprite is null || actor.Sprite is >= 0 and <= 255 &&
                    content.Visuals?.Sprites.ContainsKey(actor.Sprite.Value) != false) &&
                (actor.SharedSpriteSlot is null or >= 0 and < 49), "actor");
            Require(actor.Origin is not RaftEntityOrigin || actor.Sprite == RaftRuntime.Sprite, "raft");
            var actions = actor.ActionSource is { } source
                ? bindings.Actions(source, initial, Player, actor.Entity, actor.Slot) : null;
            Require(actions is null || actor.ActionCursor >= 0 && actor.ActionCursor < actions.Actions.Count &&
                (actions.Actions[actor.ActionCursor] is IdleEntityAction || actor.Motion.X >= 0x7000), "action-pending");
            return new ExplorationEntity(actor.Entity, actor.Motion, actor.Visible, actions, actor.ActionCursor,
                actor.WaitingForMotion, actor.Slot, actor.Sprite, actor.Follower, Priority: actor.Priority,
                RenderSize: actor.RenderSize, ShadowRemovedAt: actor.Shadowless ? 0 : null, SharedSpriteSlot: actor.SharedSpriteSlot)
            {
                Origin = actor.Origin, ActionSource = actor.ActionSource,
            };
        }).ToDictionary(actor => actor.Slot);
        ValidateFollowers(entities);
        Require(population is not null || Aliases.All(pair => entities[pair.Value].Entity == pair.Key), "aliases");

        var graphics = Graphics.ToDictionary(pair => pair.Key, pair =>
        {
            if (bindings.Instruction(pair.Value) is not SetSharedSpriteGraphics graphic || graphic.Slot != pair.Key ||
                pair.Key is < 0 or >= 49 || entities.Values.Any(actor => actor.Visible && actor.SpriteSlot == pair.Key &&
                    (actor.Sprite is not { } sprite || content.Visuals?.Sprites.GetValueOrDefault(sprite)?.Graphics?
                        .ContainsKey(graphic.Resource) != true)))
                throw new BattleRuleException("field-visit-graphics", "fieldVisit.graphics");
            return new FieldSpriteGraphic(graphic.Resource, pair.Value);
        });
        var world = new ExplorationState(map, RebuildLayout(map), Player, entities.Values, party, SpriteSize,
            Aliases, RebuildRoof(map), population, currentEntities: CurrentEntities, spriteGraphics: graphics, populationSource: Population);
        ValidateLanding(world);
        return world;
    }

    private WorkingMapLayout RebuildLayout(ExplorationMapDefinition map)
    {
        if (Layout.Count == 0) return map.Layout;
        var seen = new HashSet<int>();
        var words = map.Layout.Words.ToArray();
        foreach (var cell in Layout)
        {
            Require(cell.Index is >= 0 and < WorkingMapLayout.WordCount && seen.Add(cell.Index), "layout");
            words[cell.Index] = cell.Value;
        }
        return new(words);
    }

    private MapBlockCopyLifecycleState RebuildRoof(ExplorationMapDefinition map)
    {
        if (Roof is null) return MapBlockCopyLifecycleState.Inactive;
        var records = map.LayoutEvents?.Roofs.Records ?? [];
        Require(Roof.RecordOrdinal > 0 && Roof.RecordOrdinal <= records.Count, "roof");
        var mutation = records[Roof.RecordOrdinal - 1].Mutation;
        Require(Roof.SavedWords.Count == mutation.Width * mutation.Height, "roof");
        return new MapBlockCopyLifecycleActiveState(Roof.RecordOrdinal, mutation.DestinationX, mutation.DestinationY,
            mutation.Width, mutation.Height, Roof.SavedWords);
    }

    private static void ValidateFollowers(IReadOnlyDictionary<int, ExplorationEntity> entities)
    {
        foreach (var actor in entities.Values)
        {
            var seen = new HashSet<int> { actor.Slot };
            for (var follow = actor.Follower; follow is not null; follow = entities[follow.LeaderSlot].Follower)
                Require(entities.ContainsKey(follow.LeaderSlot) && seen.Add(follow.LeaderSlot), "follower");
        }
    }

    private static void ValidateLanding(ExplorationState world)
    {
        var player = world.PlayerEntity;
        var motion = player.Motion;
        var position = player.Position;
        Require(player.Origin is PlayerEntityOrigin && player.Visible && player.Follower is null &&
            !motion.IsMoving && motion.X % 384 == 0 && motion.Y % 384 == 0 &&
            world.Definition.Traversal.IsWithinActiveArea(position) &&
            FieldLayout.Word(world, motion, position.X, position.Y) is < 0xC000, "landing");
        var others = world.AllEntities.Where(actor => actor.Slot != player.Slot &&
            (world.Definition.Caravan is null ? actor.Visible : actor.Slot < 32));
        bool occupied = world.Population is null && world.Definition.Caravan is null
            ? others.Any(actor => actor.Follower is null && actor.Motion.XDestination / 384 == position.X &&
                actor.Motion.YDestination / 384 == position.Y)
            : (motion.FlagsA & 0x20) != 0 && EntityMotion.FieldObstructed(motion.X, motion.Y, others.Select(actor => actor.Motion));
        Require(!occupied, "landing-occupied");
    }

    private static bool SourceAlias(EntityRef reference)
    {
        return reference.Value is { } value && value.StartsWith("entity-", StringComparison.Ordinal) &&
            int.TryParse(value.AsSpan(7), out int number) && number is >= 0 and < 32 or >= 128 and < 160 &&
            SceneEntities.Reference(number) == reference;
    }

    private static void Require(bool condition, string operand)
    {
        if (!condition) throw new BattleRuleException("field-visit-" + operand, "fieldVisit." + operand);
    }
}
