using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal sealed record OutcomeStart(ProgramLocation? Program, FieldReturnAnchor Return,
    MapPosition ProjectionPosition, BattleStartInput Party);
internal sealed record OutcomeFinish(BattleStartInput Recovered, BattleStartInput Returned,
    int? JoinMember, bool ClearUnlocked, bool SetCompleted, ProgramLocation Program);

internal interface IOutcomeReturnPolicy
{
    string Identity { get; }
    OutcomeStart Start(ScenarioDefinition definition, EngineBattleState battle, StoryState story,
        BattleOutcomeKind kind, BattleStartInput party);
    OutcomeFinish Finish(ScenarioDefinition definition, StoryState story, BattleStartInput party);
    BattleStartInput Heal(ScenarioDefinition definition, BattleStartInput party, bool all);
}

internal static class OutcomeReturnRules
{
    internal static OutcomeStart Start(IOutcomeReturnPolicy rule, ScenarioDefinition definition,
        EngineBattleState battle, StoryState story, BattleOutcomeKind kind, BattleStartInput party) =>
        BattleProgressionRules.Invoke(rule.Identity, "outcome-start", () =>
        {
            var result = rule.Start(definition, battle, story, kind, party);
            Party(definition, party, result.Party, gold: false);
            Program(definition, result.Program);
            Position(definition, result.Return.Map, result.Return.Position);
            Require(result.Return.Facing <= 3);
            Position(definition, story.EnteringBattle!.Outcome!.BattleMap, result.ProjectionPosition);
            return result;
        });

    internal static OutcomeFinish Finish(IOutcomeReturnPolicy rule, ScenarioDefinition definition,
        StoryState story, BattleStartInput party) => BattleProgressionRules.Invoke(rule.Identity, "outcome-finish", () =>
    {
        var result = rule.Finish(definition, story, party);
        Party(definition, party, result.Recovered, gold: true);
        Party(definition, result.Recovered, result.Returned, gold: false);
        Program(definition, result.Program);
        Require(result.JoinMember is null || definition.Exploration!.PartyFlags is { } layout &&
            result.JoinMember >= 0 && result.JoinMember < layout.MemberCount);
        return result;
    });

    internal static BattleStartInput Heal(IOutcomeReturnPolicy rule, ScenarioDefinition definition,
        BattleStartInput party, bool all) => BattleProgressionRules.Invoke(rule.Identity, "party-recovery", () =>
    {
        var result = rule.Heal(definition, party, all);
        Party(definition, party, result, gold: false);
        return result;
    });

    private static void Program(ScenarioDefinition definition, ProgramLocation? location)
    {
        if (location is not { } pc) return;
        Require(definition.Exploration!.Programs.TryGetValue(pc.Program, out var program) &&
            pc.Instruction >= 0 && pc.Instruction < program.Instructions.Count);
    }
    private static void Position(ScenarioDefinition definition, MapId map, MapPosition position) => Require(
        definition.Exploration!.Maps.TryGetValue(map, out var target) && target.Traversal.IsWithinActiveArea(position) &&
        !OriginalMapTraversal.IsBlocked(target.Layout, position));
    private static void Party(ScenarioDefinition definition, BattleStartInput before, BattleStartInput after, bool gold)
    {
        Require(after.Encounter == before.Encounter && after.MainSeed == before.MainSeed && after.ThinkingSeed == before.ThinkingSeed &&
            after.NewBattle == before.NewBattle && (after.ActiveAllies is null ? before.ActiveAllies is null : before.ActiveAllies is not null && after.ActiveAllies.SequenceEqual(before.ActiveAllies)) && after.Actors.Count == before.Actors.Count &&
            (gold ? before.Gold.HasValue == after.Gold.HasValue : before.Gold == after.Gold));
        var encounter = definition.Encounters[before.Encounter];
        for (int index = 0; index < before.Actors.Count; index++)
        {
            var a = before.Actors[index]; var b = after.Actors[index];
            Require(b with { Hp = a.Hp, Mp = a.Mp } == a);
            var deployment = encounter.Deployments.Single(row => row.Actor == a.Actor);
            Require(deployment.Faction == BattleFaction.Ally || b == a);
            Require(b.Hp <= (a.Progress?.MaxHp ?? deployment.Definition.MaxHp) &&
                b.Mp <= (a.Progress?.MaxMp ?? deployment.Definition.MaxMp));
        }
    }
    private static void Require(bool valid) { if (!valid) throw new InvalidOperationException("Invalid outcome return result."); }
}
