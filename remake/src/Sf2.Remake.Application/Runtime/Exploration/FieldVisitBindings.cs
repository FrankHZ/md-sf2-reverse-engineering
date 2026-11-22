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

    private EntityActionProgram? ZoneActions(ZoneEntityActions source)
    {
        var events = Map(source.Body).Events;
        return source.Event >= 0 && source.Event < events.Count &&
            events[source.Event] is { Kind: ExplorationEventKind.SourceZone } zone ? zone.SourceInit : null;
    }
}
