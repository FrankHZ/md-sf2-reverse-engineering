using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal sealed record EntityActionTickResult(ExplorationState World, BattleRuleException? Failure = null);

internal static class EntityActionRunner
{
    internal static EntityActionTickResult Tick(ExplorationState world)
    {
        var entities = world.Entities.ToDictionary(pair => pair.Key, pair => pair.Value);
        ushort spriteSize = world.SpriteSize;
        // Source order: movement for this entity, then its script, then the next entity.
        foreach (var original in world.Entities.Values)
        {
            var entity = entities[original.Entity];
            var initial = entity.Motion;
            int destinationX = initial.XDestination / 384, destinationY = initial.YDestination / 384;
            ushort? word = destinationX is >= 0 and < 64 && destinationY is >= 0 and < 64
                ? world.Layout[destinationX, destinationY] : null;
            entity = entity with { Motion = EntityMotion.Tick(initial, word) };
            entities[entity.Entity] = entity;
            if (entity.WaitingForMotion && entity.Motion.IsMoving) continue;
            entity = entity with { WaitingForMotion = false };
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
                            entities.Values.Where(other => other.Entity != entity.Entity).Select(other => other.Motion));
                        if (motion is null) { next--; motion = entity.Motion; }
                        else waitingForMotion = true;
                        yield = true; break;
                    case MoveEntityAbsolute absolute:
                        motion = EntityMotion.Start(motion, (short)(absolute.Position.X * 384), (short)(absolute.Position.Y * 384),
                            entities.Values.Where(other => other.Entity != entity.Entity).Select(other => other.Motion));
                        if (motion is null) { next--; motion = entity.Motion; }
                        else waitingForMotion = true;
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
                        // Full original compressed-sprite/VRAM queue semantics are a reached boundary.
                        // No endpoint or silently successful refresh substitutes for that work.
                        throw new BattleRuleException("entity-sprite-refresh", refresh.Source, true);
                    case JumpEntityAction jump: next = jump.Instruction; motion = motion with { WaitTimer = 0 }; break;
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
                    entities[entity.Entity] = entity;
                    return new(world.WithEntities(entities.Values, spriteSize), error);
                }
                if (entity.Actions is not null)
                    entity = entity with { Motion = motion, ActionCursor = next, WaitingForMotion = waitingForMotion };
                entities[entity.Entity] = entity;
                if (yield) break;
            }
            entities[entity.Entity] = entity;
        }
        return new(world.WithEntities(entities.Values, spriteSize));
    }
}
