using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

public sealed record PhysicalEntityWait(WaitToken Token, int Slot) : ProgramWait(Token);

internal static class CaravanRuntime
{
    internal static (ActiveSessionState Active, StoryState Story) Execute(SessionSnapshot current,
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
                    SpriteRequest = checked(parked.SpriteRequest + 1), WaitingForSprite = true });
                next = current.Story.Copy(current.Story.Cursor, new EntitySpriteWait(token, parked.Slot, parked.SpriteRequest + 1));
                break;
            case ApplyCaravanFollowers followers:
                // Apply in current-list order. Validate each reached slot without replacing aliases/list.
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

    internal static ExplorationEntity Physical(ExplorationState world, int slot) =>
        world.AllEntities.FirstOrDefault(actor => actor.Slot == slot) ??
            throw new BattleRuleException("caravan-physical-entity", "slot-" + slot, true);
}
