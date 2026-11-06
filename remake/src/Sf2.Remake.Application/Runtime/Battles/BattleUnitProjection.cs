using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

public sealed record BattleUnitSummary(ActorRef Actor, bool Ally, string Name, int? NameIndex,
    string? CustomName, byte? ClassId, string? ClassName, int? Level,
    int? Hp, int? MaxHp, int? Mp, int? MaxMp, ushort Status);
public sealed record BattleLandEffect(ActorRef Actor, MapPosition Position, BattleMover Mover, int Percent);

// Shared live disclosure for ordinary acting cards and settled-origin inspection.
internal static class BattleUnitProjection
{
    internal static int? NameIndex(BattleActorState actor, BattleSceneDefinition? scenes) => actor.IsAlly
        ? actor.Deployment.Initialization?.AllyPartyMember
        : scenes?.EnemyNameIds?.TryGetValue(actor.Actor, out byte enemy) == true ? enemy : null;

    internal static string Name(ScenarioDefinition definition, SessionSnapshot current, BattleActorState actor)
    {
        var scenes = definition.ScenesFor(current.Battle.Definition.Encounter);
        int member = actor.Deployment.Initialization?.AllyPartyMember ?? actor.ProcessingOrder;
        bool admitted = actor.IsAlly
            ? scenes is not null && member >= 0 && member < scenes.MemberNames.Count && !string.IsNullOrWhiteSpace(scenes.MemberNames[member])
            : scenes?.EnemyNames?.ContainsKey(actor.Actor) == true;
        return definition.PrivateDefinitions is null || admitted || current.Story.Names.ContainsKey(actor.Actor.Value)
            ? CharacterNames.Battle(current.Story, actor, scenes) : "Name unavailable";
    }

    internal static int? Stat(ScenarioDefinition definition, int value) =>
        definition.PrivateDefinitions is not null && value >= 100 ? null : value;

    internal static BattleUnitSummary Summary(ScenarioDefinition definition, SessionSnapshot current, BattleActorState actor)
    {
        byte? classId = actor.IsAlly ? actor.Definition.Growth?.ClassId : null;
        string? expression = classId is { } key ? definition.PrivateDefinitions?.Classes.GetValueOrDefault(key)?.NameExpression : null;
        string? className = expression is { Length: >= 2 } && expression[0] == '"' && expression[^1] == '"' ? expression[1..^1] : null;
        return new(actor.Actor, actor.IsAlly, Name(definition, current, actor),
            NameIndex(actor, definition.ScenesFor(current.Battle.Definition.Encounter)), current.Story.Names.GetValueOrDefault(actor.Actor.Value),
            classId, className, actor.IsAlly ? actor.Level : null,
            Stat(definition, actor.Hp), Stat(definition, actor.MaxHp), Stat(definition, actor.Mp), Stat(definition, actor.MaxMp), actor.Status);
    }

    internal static BattleLandEffect Land(SessionSnapshot current, BattleActorState actor, MapPosition position) =>
        new(actor.Actor, position, actor.Definition.Mover,
            BattleTerrainRules.LandEffect(current.Battle.Definition.Terrain[position.Y * 48 + position.X], actor.Definition.Mover));
}
