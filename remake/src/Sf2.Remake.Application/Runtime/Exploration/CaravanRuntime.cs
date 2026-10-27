using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

public sealed record PhysicalEntityWait(WaitToken Token, int Slot) : ProgramWait(Token);

internal static class CaravanRuntime
{
    internal static (ActiveSessionState Active, StoryState Story) Execute(ScenarioDefinition definition, SessionSnapshot current,
        CaravanInstruction instruction, StoryState next, WaitToken token)
    {
        var world = current.Exploration ?? throw new BattleRuleException("caravan-field", "program", true);
        _ = world.Definition.Caravan ?? throw new BattleRuleException("caravan-admission", "map", true);
        switch (instruction)
        {
            case ParkCaravanEntity park:
                var parked = ProgramRunner.Entity(current, park.Entity);
                world = world.WithEntity(parked with { Motion = parked.Motion with
                    { X = 0x6000, Y = 0x6000, XDestination = 0x6000, YDestination = 0x6000, Facing = 3 },
                    SpriteRequest = checked(parked.SpriteRequest + 1), WaitingForSprite = true,
                    RenderSize = (parked.Motion.FlagsB & 8) != 0 ? world.SpriteSize : (ushort)24 });
                next = current.Story.Copy(current.Story.Cursor, new EntitySpriteWait(token, parked.Slot, parked.SpriteRequest + 1));
                break;
            case RestoreCaravanPlayer:
                var appearance = world.Definition.Caravan!.PlayerAppearance;
                var member = world.Party.Actors.SingleOrDefault(actor => actor.Actor == appearance.Actor)
                    ?? throw new BattleRuleException("caravan-player-identity", "party", true);
                var identities = definition.Encounters.Values.SelectMany(encounter => encounter.Deployments)
                    .Where(actor => actor.Actor == appearance.Actor).ToArray();
                if (identities.Length == 0 || identities.Any(actor => actor.Definition.ClassRule != appearance.Class))
                    throw new BattleRuleException("caravan-player-class", "party", true);
                var restored = world.PlayerEntity;
                world = world.WithEntity(restored with { Sprite = member.Hp == 0 ? appearance.DefeatedSprite : appearance.Sprite,
                    SpriteRequest = checked(restored.SpriteRequest + 1), WaitingForSprite = true,
                    RenderSize = (restored.Motion.FlagsB & 8) != 0 ? world.SpriteSize : (ushort)24 });
                next = current.Story.Copy(current.Story.Cursor, new EntitySpriteWait(token, restored.Slot, restored.SpriteRequest + 1));
                break;
            case InstallCaravanFollowers:
                world = InstallFollowers(world, current.Story.Flags);
                break;
            case ApplyCaravanFollowers followers:
                // External admission establishes valid slots; reached recheck precedes any writes.
                foreach (int slot in world.CurrentEntities.Skip(1)) _ = Physical(world, slot);
                foreach (int slot in world.CurrentEntities.Skip(1))
                {
                    var actor = Physical(world, slot);
                    world = world.WithEntity(actor with { Actions = followers.Actions, ActionCursor = 0,
                        WaitingForMotion = false, Follower = null });
                }
                break;
            case WaitForCaravanFollowers:
                if (world.CurrentEntities.Count > 1)
                    next = current.Story.Copy(current.Story.Cursor,
                        new PhysicalEntityWait(token, world.CurrentEntities[^1]));
                break;
            case CommitCaravanMode mode:
                world = world.WithTravelMode(mode.Mode);
                break;
            default: throw new BattleRuleException("caravan-operation", "program", true);
        }
        return (new ActiveExploration(world), next);
    }

    // Flag rows and consecutive action pointers are independent of the current physical list.
    private static readonly int[] NormalFollowerFlags = [66, 67, 72, 73, 74, 66, 68, 75, 69, 70, 74, 71, 74, 76, 77, 82, 83, 83, 84];
    internal static ExplorationState InstallFollowers(ExplorationState world, IReadOnlyList<int> flags)
    {
        bool overworld = world.Definition.Caravan!.Overworld && flags.Contains(65);
        int count = NormalFollowerFlags.Count(flags.Contains) + (overworld ? 1 : 0);
        if (count > 5) throw new BattleRuleException("caravan-follower-action-table", "flags", true);
        for (int index = 0; index < count; index++) _ = Physical(world, index + 1);
        for (int index = 0; index < count; index++)
        {
            var actor = Physical(world, index + 1);
            world = world.WithEntity(actor with { Actions = CaravanPrograms.Follower(index, overworld),
                ActionCursor = 0, WaitingForMotion = false, Follower = null });
        }
        return world;
    }

    internal static void RequireOrdinaryMapOperation(ExplorationState world)
    {
        if (world.Definition.Caravan is not null || world.TravelMode != FieldTravelMode.Walking)
            throw new BattleRuleException("caravan-map-replacement", "map", true);
    }

    internal static (ExplorationState World, FieldMoveOutcome Outcome) Move(ExplorationState world, ExplorationDirection direction)
    {
        if (world.Definition.Caravan is null) throw new BattleRuleException("caravan-admission", "map", true);
        var player = world.PlayerEntity;
        int currentMarker = world.Layout[player.Position.X, player.Position.Y] & 0x3C00;
        if (currentMarker is 0x800 or 0xC00) throw new BattleRuleException("caravan-copy-marker", "map.layout", true);
        // This admission has one loaded area; scripted placement does not reselect it.
        var area = world.Definition.Traversal.ActiveAreas[0];
        var candidate = CaravanTraversal.Candidate(player.Motion, area, direction, false);
        if (candidate is null) return (world, new(false, false, null));
        if ((player.Motion.FlagsA & 0x20) != 0 && CaravanTraversal.Obstructed(candidate.X * 384, candidate.Y * 384,
            world.AllEntities.Where(actor => actor.Slot < 32).Select(actor => actor.Motion)))
            return (world, new(false, false, null));
        ushort word = world.Layout[candidate.X, candidate.Y];
        int marker = word & 0x3C00;
        if (marker is 0x400 or 0x800 or 0xC00 or 0x1000)
            throw new BattleRuleException("caravan-unsupported-marker", "map.layout", true);
        if (marker != 0x3800) return (world, new(false, false, null, word < 0xC000 ? 4 : null));
        var motion = EntityMotion.Start(player.Motion, (short)(candidate.X * 384), (short)(candidate.Y * 384), [])!;
        return (world.WithEntity(player with { Motion = motion, Actions = new([new StopEntityActions()]),
            ActionCursor = 0, WaitingForMotion = true }), new(true, false, null));
    }

    internal static ExplorationEntity Physical(ExplorationState world, int slot) =>
        world.AllEntities.FirstOrDefault(actor => actor.Slot == slot) ??
            throw new BattleRuleException("caravan-physical-entity", "slot-" + slot, true);
}
