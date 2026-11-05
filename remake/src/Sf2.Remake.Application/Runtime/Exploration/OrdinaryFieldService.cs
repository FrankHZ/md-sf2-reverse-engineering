namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class OrdinaryFieldService
{
    // One sprite phase after movement/actions, independent of entity-service enablement.
    // Visible off-camera actors share this modern cadence; special/scripted poses keep their owner.
    internal static ExplorationState Sprites(ExplorationState world) => world.WithEntities(
        world.AllEntities.Select(entity =>
        {
            var motion = entity.Motion;
            if (!entity.Visible || entity.WaitingForSprite || entity.RenderSize != 24 ||
                (motion.FlagsB & 8) != 0 || motion.AnimationCounter == 255) return entity;
            int counter = motion.AnimationCounter + ((motion.FlagsB & 16) != 0 ? 3 : 1);
            return entity with { Motion = motion with { AnimationCounter = (byte)(counter > 30 ? 0 : counter) } };
        }));
}
