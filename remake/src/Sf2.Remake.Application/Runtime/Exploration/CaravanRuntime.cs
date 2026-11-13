using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

public sealed record PhysicalEntityWait(WaitToken Token, int Slot) : ProgramWait(Token);

internal static class CaravanRuntime
{
    // The overworld follower table falls through into the ordinary rows.
    internal static bool HasOverworldPopulation(ExplorationPopulation? population) =>
        population?.Followers.FirstOrDefault() is { Flag: 65, Character: 30, Sprite: 62 };

    private static IEnumerable<int> FollowerSlots(ExplorationState world, bool skipCaravan) =>
        world.CurrentEntities.Skip((world.Population is null ? 0 : 1) + (skipCaravan ? 1 : 0));

    internal static (ActiveSessionState Active, StoryState Story) Execute(ScenarioDefinition definition, SessionSnapshot current,
        VehicleInstruction instruction, StoryState next, WaitToken token)
    {
        var world = current.Exploration ?? throw new BattleRuleException("caravan-field", "program", true);
        switch (instruction)
        {
            case MoveVehicleOffMap park:
                var parked = ProgramRunner.Entity(current, park.Entity);
                world = world.WithEntity(parked with { Motion = parked.Motion with
                    { X = 0x6000, Y = 0x6000, XDestination = 0x6000, YDestination = 0x6000, Facing = 3 },
                    SpriteRequest = checked(parked.SpriteRequest + 1), WaitingForSprite = true,
                    RenderSize = (parked.Motion.FlagsB & 8) != 0 ? world.SpriteSize : (ushort)24 });
                next = current.Story.Copy(current.Story.Cursor, new EntitySpriteWait(token, parked.Slot, parked.SpriteRequest + 1));
                break;
            case RestoreCaravanPlayer:
                int sprite = WalkingSprite(definition, world, current.Story.Flags);
                var restored = world.PlayerEntity;
                world = world.WithEntity(restored with { Sprite = sprite,
                    SpriteRequest = checked(restored.SpriteRequest + 1), WaitingForSprite = true,
                    RenderSize = (restored.Motion.FlagsB & 8) != 0 ? world.SpriteSize : (ushort)24 });
                next = current.Story.Copy(current.Story.Cursor, new EntitySpriteWait(token, restored.Slot, restored.SpriteRequest + 1));
                break;
            case InstallCaravanFollowers:
                world = InstallFollowers(world, current.Story.Flags);
                break;
            case ApplyVehicleFollowerActions followers:
                // External admission establishes valid slots; reached recheck precedes any writes.
                foreach (int slot in FollowerSlots(world, followers.SkipCaravan)) _ = Physical(world, slot);
                foreach (int slot in FollowerSlots(world, followers.SkipCaravan))
                {
                    var actor = Physical(world, slot);
                    world = world.WithEntity(actor with { Actions = followers.Actions, ActionCursor = 0,
                        WaitingForMotion = false, Follower = null });
                }
                break;
            case WaitForVehicleFollowers wait:
                if (FollowerSlots(world, wait.SkipCaravan).Any())
                    next = current.Story.Copy(current.Story.Cursor,
                        new PhysicalEntityWait(token, world.CurrentEntities[^1]));
                break;
            case SetFieldTravelMode mode:
                world = world.WithTravelMode(mode.Mode);
                break;
            default: throw new BattleRuleException("caravan-operation", "program", true);
        }
        return (new ActiveExploration(world), next);
    }

    private static int WalkingSprite(ScenarioDefinition definition, ExplorationState world, IReadOnlyList<int> flags)
    {
        if (world.Population is { } population && HasOverworldPopulation(population))
        {
            var bowie = world.Party.Actors.SingleOrDefault(member => member.Actor.Value == "ally-0")
                ?? throw new BattleRuleException("caravan-player-identity", "party", true);
            return bowie.Hp == 0 ? 190 : SceneEntities.ResolveAllySprite(population, flags, 0, population.PlayerSprite);
        }
        var appearance = world.Definition.Caravan?.PlayerAppearance
            ?? throw new BattleRuleException("caravan-admission", "map", true);
        var member = world.Party.Actors.SingleOrDefault(actor => actor.Actor == appearance.Actor)
            ?? throw new BattleRuleException("caravan-player-identity", "party", true);
        var identities = definition.Encounters.Values.SelectMany(encounter => encounter.Deployments)
            .Where(actor => actor.Actor == appearance.Actor).ToArray();
        if (identities.Length == 0 || identities.Any(actor => actor.Definition.ClassRule != appearance.Class))
            throw new BattleRuleException("caravan-player-class", "party", true);
        return member.Hp == 0 ? appearance.DefeatedSprite : appearance.Sprite;
    }

    // Flag rows and consecutive action pointers are independent of the current physical list.
    private static readonly int[] NormalFollowerFlags = [66, 67, 72, 73, 74, 66, 68, 75, 69, 70, 74, 71, 74, 76, 77, 82, 83, 83, 84];
    internal static ExplorationState InstallFollowers(ExplorationState world, IReadOnlyList<int> flags)
    {
        bool overworld = flags.Contains(65) && (world.Definition.Caravan?.Overworld == true || HasOverworldPopulation(world.Population));
        int count = world.Population is { } population ? population.Followers.Count(row => flags.Contains(row.Flag))
            : NormalFollowerFlags.Count(flags.Contains) + (overworld ? 1 : 0);
        if (count > 5) throw new BattleRuleException("caravan-follower-action-table", "flags", true);
        for (int index = 0; index < count; index++) _ = Physical(world, index + 1);
        for (int index = 0; index < count; index++)
        {
            var actor = Physical(world, index + 1);
            world = world.WithEntity(!overworld || world.Population is null
                ? actor with { Actions = CaravanPrograms.Follower(index, overworld), ActionCursor = 0, WaitingForMotion = false, Follower = null }
                : InstallPopulationFollower(actor, index, overworld));
        }
        return world;
    }

    internal static ExplorationEntity InstallPopulationFollower(ExplorationEntity actor, int index, bool overworld)
    {
        var (leader, x, y, wait) = CaravanPrograms.Formation(index, overworld);
        return FollowerMotion.Install(actor, leader, x, y, wait);
    }

    internal static void RequireOrdinaryMapOperation(ExplorationState world)
    {
        if (world.Definition.Caravan is not null || world.TravelMode != FieldTravelMode.Walking)
            throw new BattleRuleException("caravan-map-replacement", "map", true);
    }

    internal static (ExplorationState World, FieldMoveOutcome Outcome) Move(ExplorationState world,
        ExplorationDirection direction, IReadOnlyList<int> flags)
    {
        bool raft = world.TravelMode == FieldTravelMode.Raft;
        if (!raft && world.Definition.Caravan is null && !HasOverworldPopulation(world.Population)) throw new BattleRuleException("caravan-admission", "map", true);
        var player = world.PlayerEntity;
        int currentMarker = world.Layout[player.Position.X, player.Position.Y] & 0x3C00;
        if (currentMarker is 0x800 or 0xC00) throw new BattleRuleException(raft ? "raft-copy-marker" : "caravan-copy-marker", "map.layout", true);
        // The authored controller retains its one loaded area after scripted placement.
        var area = world.Definition.Caravan is null ? world.Definition.Traversal.SelectActiveArea(player.Position)?.Area : world.Definition.Traversal.ActiveAreas[0];
        if (area is null) return (world, new(false, false, null));
        var candidate = CaravanTraversal.Candidate(player.Motion, area, direction, false);
        if (candidate is null) return (world, new(false, false, null));
        if ((player.Motion.FlagsA & 0x20) != 0 && CaravanTraversal.Obstructed(candidate.X * 384, candidate.Y * 384,
            world.AllEntities.Where(actor => actor.Slot < 32).Select(actor => actor.Motion)))
            return (world, new(false, false, null));
        ushort word = world.Layout[candidate.X, candidate.Y];
        int marker = word & 0x3C00;
        bool warpTile = world.Definition.Caravan is null && marker == 0x1000;
        if (!warpTile && marker is (0x400 or 0x800 or 0xC00 or 0x1000))
            throw new BattleRuleException(raft ? "raft-unsupported-marker" : "caravan-unsupported-marker", "map.layout", true);
        if (!warpTile && marker != (raft ? 0x3C00 : 0x3800))
        {
            return (world, new(false, false, null, word < 0xC000 ? (raft ? 5 : 4) : null));
        }
        var warp = warpTile ? world.Definition.Events.FirstOrDefault(entry => entry.Kind == ExplorationEventKind.Warp &&
            MapEventDispatcher.Matches(entry, candidate, word, flags)) : null;
        var motion = EntityMotion.Start(player.Motion, (short)(candidate.X * 384), (short)(candidate.Y * 384), [])!;
        return (world.WithEntity(player with { Motion = motion, Actions = new([new StopEntityActions()]),
            ActionCursor = 0, WaitingForMotion = true }), new(true, false, warp));
    }

    internal static ExplorationEntity Physical(ExplorationState world, int slot) =>
        world.AllEntities.FirstOrDefault(actor => actor.Slot == slot) ??
            throw new BattleRuleException("caravan-physical-entity", "slot-" + slot, true);
}
