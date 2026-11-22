using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal abstract record FieldPopulationSource;
internal sealed record MapPopulationSource(FieldMapBody Body) : FieldPopulationSource;
internal sealed record ProgramPopulationSource(ProgramLocation Location) : FieldPopulationSource;

internal abstract record FieldEntityOrigin;
internal sealed record PlayerEntityOrigin : FieldEntityOrigin;
internal sealed record ResidentEntityOrigin(int Row) : FieldEntityOrigin;
internal sealed record FollowerEntityOrigin(int Character) : FieldEntityOrigin;
internal sealed record AllocatedEntityOrigin(ProgramLocation Location) : FieldEntityOrigin;
internal sealed record RaftEntityOrigin : FieldEntityOrigin;

internal abstract record FieldActionSource;
internal sealed record InitialEntityActions : FieldActionSource;
internal sealed record ProgramEntityActions(ProgramLocation Location) : FieldActionSource;
internal sealed record ZoneEntityActions(FieldMapBody Body, int Event) : FieldActionSource;
internal sealed record CaravanFollowerActions(int Order, bool Overworld) : FieldActionSource;
internal sealed record FieldStepActions : FieldActionSource
{
    internal static readonly EntityActionProgram Program = new([new StopEntityActions()]);
}

internal sealed record FieldSpriteGraphic(string Resource, ProgramLocation Source);

// Resolves values into the admitted content. None of these lookups execute a map or program.
internal sealed class FieldVisitBindings(ExplorationDefinition content)
{
    internal ExplorationMapDefinition Map(FieldMapBody body) =>
        content.Maps.TryGetValue(body.Map, out var source) ? source.BindSetup(body.Setup)
            : throw new BattleRuleException("field-visit-map", "fieldVisit.body");

    internal (ExplorationPopulation? Population, IReadOnlyList<ExplorationEntityDefinition> Records) Population(
        FieldPopulationSource source)
    {
        if (source is MapPopulationSource mapSource)
        {
            var map = Map(mapSource.Body);
            return (map.Population, map.Entities);
        }
        return source is ProgramPopulationSource programSource ? Instruction(programSource.Location) switch
        {
            LoadSceneEntities load => (load.Population, load.Entities),
            ReloadDefaultEntities reload when content.Maps.TryGetValue(reload.Map, out var map) =>
                (map.Population, map.BindSetup(reload.Setup ?? map.Body.Setup).Entities),
            _ => throw new BattleRuleException("field-visit-population", "fieldVisit.population"),
        } : throw new BattleRuleException("field-visit-population", "fieldVisit.population");
    }

    internal StoryInstruction Instruction(ProgramLocation source) =>
        source.Program is not null && content.Programs.TryGetValue(source.Program, out var program) &&
        source.Instruction >= 0 && source.Instruction < program.Instructions.Count
            ? program.Instructions[source.Instruction]
            : throw new BattleRuleException("field-visit-program", "fieldVisit.source");

    internal EntityActionProgram? InitialActions(FieldEntityOrigin origin, EntityRef entity, int slot, EntityRef player,
        ExplorationPopulation? population, IReadOnlyList<ExplorationEntityDefinition> records)
    {
        switch (origin)
        {
            case PlayerEntityOrigin when slot == 0 && entity == player:
            case FollowerEntityOrigin follower when population?.Followers.Any(row => row.Character == follower.Character) == true &&
                slot > 0 && entity == SceneEntities.Reference(follower.Character):
            case RaftEntityOrigin when slot == RaftRuntime.Slot && entity == RaftRuntime.FollowerD:
                return null;
            case AllocatedEntityOrigin allocated when population is not null && slot > 0 &&
                Instruction(allocated.Location) is AddMapEntity add && entity == add.Entity:
                return add.Actions;
            case ResidentEntityOrigin resident when resident.Row >= 0 && resident.Row < records.Count && slot > 0:
                var row = records[resident.Row];
                if (population is null && entity == row.Entity) return row.Actions;
                if (population is not null && row.Sprite is { } sprite &&
                    (sprite < 240 || slot == 47))
                {
                    int character = sprite < population.AllyCount ? sprite : population.NonAllyStart +
                        records.Take(resident.Row).Count(prior => prior.Sprite >= population.AllyCount);
                    if (entity == SceneEntities.Reference(character)) return row.Actions;
                }
                break;
        }
        throw new BattleRuleException("field-visit-actor-origin", "fieldVisit.actors");
    }

    internal EntityActionProgram Actions(FieldActionSource source, EntityActionProgram? initial, EntityRef player,
        EntityRef entity, int slot) =>
        (source switch
        {
            InitialEntityActions => initial,
            ProgramEntityActions program => Instruction(program.Location) switch
            {
                // The producing instruction resolved its alias at installation. That alias can change later.
                StartEntityMotion motion => motion.Actions,
                ApplyVehicleFollowerActions followers when entity != player => followers.Actions,
                _ => null,
            },
            ZoneEntityActions zone when entity == player => ZoneActions(zone),
            CaravanFollowerActions caravan when caravan.Order is >= 0 and < 5 && slot == caravan.Order + 1 =>
                CaravanPrograms.Follower(caravan.Order, caravan.Overworld),
            FieldStepActions when entity == player => FieldStepActions.Program,
            _ => null,
        }) ?? throw new BattleRuleException("field-visit-action-origin", "fieldVisit.actors");

    internal static bool IsSettledAction(ExplorationEntity actor, IReadOnlySet<int> slots)
    {
        // The movement/action loop checks physical X, independently of visibility and aliases.
        if (actor.Actions is not { } program) return actor.Motion.X >= 0x7000 || !actor.Motion.IsMoving;
        int cursor = actor.ActionCursor;
        if (cursor < 0 || cursor >= program.Actions.Count) return false;
        if (actor.Motion.X >= 0x7000) return true;
        if (actor.IsScriptIdle) return !actor.Motion.IsMoving;

        // The ordinary vehicle landing producer installs these even in walking mode.
        if (actor.ActionSource is CaravanFollowerActions)
            return program.Actions.OfType<FollowPhysical>().All(follow => slots.Contains(follow.Slot));

        // The content reader appends Stop after a final backward jump. It is
        // unreachable in a continuing resident loop and is not a saved cursor.
        if (actor.ActionSource is not (InitialEntityActions or ProgramEntityActions)) return false;
        var actions = program.Actions;
        int end = actions.Count - (actions[^1] is StopEntityActions ? 2 : 1);
        if (end < 2 || cursor > end ||
            actions[end] is not JumpEntityAction { Instruction: >= 0 } jump || jump.Instruction >= end) return false;
        int loop = jump.Instruction;
        var cycle = actions.Skip(loop).Take(end - loop).ToArray();
        if (!(cycle is [RandomWalkEntity, WaitEntityTicks] || IsClosedPatrol(cycle) || IsFacingCycle(cycle))) return false;
        if (actor.WaitingForMotion || actor.Motion.IsMoving)
            return actor.WaitingForMotion && cursor > loop &&
                actions[cursor - 1] is RandomWalkEntity or MoveEntityRelative { Wait: true };
        // Prefix work with story or presentation effects must have completed.
        return cursor >= loop || actions.Take(loop).All(action => action is
            WaitEntityTicks or SetEntitySpeed or SetEntityAcceleration or ChangeEntityFlags or FaceEntity);
    }

    private static bool IsFacingCycle(IReadOnlyList<EntityAction> cycle)
    {
        if (cycle.Count % 3 != 0) return false;
        for (int i = 0; i < cycle.Count; i += 3)
            if (cycle[i] is not FaceEntity || cycle[i + 1] is not RefreshEntitySprite ||
                cycle[i + 2] is not WaitEntityTicks)
                return false;
        return true;
    }

    private static bool IsClosedPatrol(IReadOnlyList<EntityAction> cycle)
    {
        if (cycle.Count % 2 != 0) return false;
        long x = 0, y = 0;
        for (int i = 0; i < cycle.Count; i += 2)
        {
            if (cycle[i] is not MoveEntityRelative { Wait: true } move || cycle[i + 1] is not WaitEntityTicks)
                return false;
            x += move.X;
            y += move.Y;
        }
        return x == 0 && y == 0;
    }

    private EntityActionProgram? ZoneActions(ZoneEntityActions source)
    {
        var events = Map(source.Body).Events;
        return source.Event >= 0 && source.Event < events.Count &&
            events[source.Event] is { Kind: ExplorationEventKind.SourceZone } zone ? zone.SourceInit : null;
    }
}
