using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal sealed record EntityActionTickResult(ExplorationState World, BattleRuleException? Failure = null,
    FieldMoveOutcome? FieldMove = null);

internal static class EntityActionRunner
{
    // eas_ControlledCharacter establishes these settings when ordinary control owns the slot.
    // Keep physical travel and unrelated flags/counters; callers choose the control boundary.
    internal static EntityMotionState ControlledMotion(EntityMotionState motion) => motion with
    {
        XSpeed = 32, YSpeed = 32, XAcceleration = 0, YAcceleration = 0,
        FlagsA = (byte)((motion.FlagsA & 0x10) | 0xEF), WaitTimer = 0,
    };

    internal static EntityActionTickResult Tick(ExplorationState world, ExplorationDirection? fieldMove = null,
        IReadOnlyList<int>? storyFlags = null, bool fieldControl = false)
    {
        var entities = world.AllEntities.ToDictionary(entity => entity.Slot);
        ushort spriteSize = world.SpriteSize;
        uint seed = world.Party.MainSeed;
        FieldMoveOutcome? fieldOutcome = null;
        ExplorationState Publish() => world.WithEntities(entities.Values, spriteSize).WithParty(
            new(world.Party.Encounter, world.Party.Actors, seed, world.Party.ThinkingSeed, world.Party.Gold, world.Party.NewBattle));
        // Source order: movement for this entity, then its script, then the next entity.
        foreach (var original in world.AllEntities)
        {
            var entity = entities[original.Slot];
            var initial = entity.Motion;
            int destinationX = initial.XDestination / 384, destinationY = initial.YDestination / 384;
            ushort? word = destinationX is >= 0 and < 64 && destinationY is >= 0 and < 64
                ? world.Layout[destinationX, destinationY] : null;
            entity = entity with { Motion = EntityMotion.Tick(initial, word) };
            if (fieldControl && world.Population is not null && entity.Slot == world.PlayerEntity.Slot)
                entity = entity with { Motion = ControlledMotion(entity.Motion) };
            entities[entity.Slot] = entity;
            if (fieldMove is not null && entity.Slot == world.PlayerEntity.Slot)
            {
                var resolved = MapEventDispatcher.Move(Publish(), fieldMove.Value, storyFlags!);
                world = resolved.World;
                fieldOutcome = resolved.Outcome;
                entities[entity.Slot] = world.PlayerEntity;
                // esc02 yields to the next slot even after setting MAP_EVENT_TYPE.
                continue;
            }
            if (entity.WaitingForMotion && entity.Motion.IsMoving) continue;
            entity = FollowerMotion.Tick(entity with { WaitingForMotion = false }, entities, world.Layout);
            for (int budget = 0; budget < 1024 && entity.Actions is { } program; budget++)
            {
                if (entity.ActionCursor < 0 || entity.ActionCursor >= program.Actions.Count)
                    throw new BattleRuleException("entity-action-cursor", "entity.actions");
                var motion = entity.Motion;
                int next = entity.ActionCursor + 1;
                bool yield = false, waitingForMotion = false;
                try
                {
                switch (program.Actions[entity.ActionCursor])
                {
                    case MoveEntityRelative relative:
                        int x = motion.X + relative.X * 384, y = motion.Y + relative.Y * 384;
                        if (x is < 0 or > 24192 || y is < 0 or > 24192)
                            throw new BattleRuleException("entity-motion-boundary", "entity.actions", true);
                        motion = EntityMotion.Start(motion, (short)x, (short)y,
                            entities.Values.Where(other => other.Slot != entity.Slot).Select(other => other.Motion));
                        if (motion is null) { next--; motion = entity.Motion; yield = true; }
                        else { waitingForMotion = relative.Wait; yield = relative.Wait; }
                        break;
                    case MoveEntityAbsolute absolute:
                        motion = EntityMotion.Start(motion, (short)(absolute.Position.X * 384), (short)(absolute.Position.Y * 384),
                            entities.Values.Where(other => other.Slot != entity.Slot).Select(other => other.Motion), absolute.FieldInput);
                        if (motion is null) { next--; motion = entity.Motion; }
                        else waitingForMotion = true;
                        yield = true; break;
                    case RandomWalkEntity walk:
                        for (int attempt = 0; attempt < 4; attempt++)
                        {
                            var draw = BattleRandom.NextMain(seed, 4); seed = draw.After;
                            var direction = draw.Value switch { 0 => ExplorationDirection.East, 1 => ExplorationDirection.North,
                                2 => ExplorationDirection.West, _ => ExplorationDirection.South };
                            bool outside = draw.Value switch
                            {
                                0 => motion.X >= (walk.Origin.X + walk.Radius) * 384,
                                1 => motion.Y <= (walk.Origin.Y - walk.Radius) * 384,
                                2 => motion.X <= (walk.Origin.X - walk.Radius) * 384,
                                _ => motion.Y >= (walk.Origin.Y + walk.Radius) * 384,
                            };
                            if (outside) continue;
                            var target = world.Definition.Traversal.ResolveCandidateTarget(world.Layout, entity.Position, direction);
                            if (target is null || ((motion.FlagsA & 0x40) != 0 && OriginalMapTraversal.IsBlocked(world.Layout, target))) continue;
                            var started = EntityMotion.Start(motion, (short)(target.X * 384), (short)(target.Y * 384),
                                entities.Values.Where(other => other.Slot != entity.Slot && other.Visible).Select(other => other.Motion));
                            if (started is null) continue;
                            motion = started; waitingForMotion = true; break;
                        }
                        yield = true; break;
                    case FaceEntity face: motion = motion with { Facing = face.Facing, WaitTimer = 0 }; break;
                    case WaitEntityTicks wait:
                        if (motion.WaitTimer < wait.Ticks)
                        { motion = motion with { WaitTimer = (byte)(motion.WaitTimer + 1) }; next--; yield = true; }
                        else motion = motion with { WaitTimer = 0 };
                        break;
                    case SetEntitySpeed speed: motion = motion with { XSpeed = speed.X, YSpeed = speed.Y, WaitTimer = 0 }; break;
                    case SetEntityAcceleration factors: motion = motion with { XAcceleration = factors.X, YAcceleration = factors.Y, WaitTimer = 0 }; break;
                    case ChangeEntityFlags flags:
                        motion = flags.FlagsB ? motion with { FlagsB = (byte)((motion.FlagsB & ~flags.Mask) | (flags.Value & flags.Mask)), WaitTimer = 0 }
                            : motion with { FlagsA = (byte)((motion.FlagsA & ~flags.Mask) | (flags.Value & flags.Mask)), WaitTimer = 0 };
                        break;
                    case SetGlobalSpriteSize size: spriteSize = size.Size; motion = motion with { WaitTimer = 0 }; break;
                    case RefreshEntitySprite refresh:
                        if (!entity.WaitingForSprite)
                            entity = entity with { SpriteRequest = checked(entity.SpriteRequest + 1), WaitingForSprite = true };
                        if (entity.SpriteReady != entity.SpriteRequest) { next--; yield = true; }
                        else entity = entity with { WaitingForSprite = false };
                        break;
                    case JumpEntityAction jump: next = jump.Instruction; motion = motion with { WaitTimer = 0 }; break;
                    case IdleEntityAction:
                        // eas_Idle's wait1/branch is quiescent ownership, with one existing
                        // slot service. A direct installation has not reset the incoming timer.
                        motion = motion with { WaitTimer = (sbyte)motion.WaitTimer < 1
                            ? unchecked((byte)(motion.WaitTimer + 1)) : (byte)1 };
                        next--; yield = true; break;
                    case UnsupportedEntityAction unsupported:
                        throw new BattleRuleException("entity-action", unsupported.Source + ":" + unsupported.Opcode, true);
                    case StopEntityActions:
                        entity = entity with { Actions = null, ActionCursor = 0, WaitingForMotion = false };
                        break;
                    default: throw new BattleRuleException("entity-action", "entity.actions", true);
                }
                }
                catch (BattleRuleException error)
                {
                    entities[entity.Slot] = entity;
                    return new(Publish(), error);
                }
                if (entity.Actions is not null)
                    entity = entity with { Motion = motion, ActionCursor = next, WaitingForMotion = waitingForMotion };
                entities[entity.Slot] = entity;
                if (yield) break;
            }
            entities[entity.Slot] = entity;
        }
        return new(Publish(), FieldMove: fieldOutcome);
    }
}
