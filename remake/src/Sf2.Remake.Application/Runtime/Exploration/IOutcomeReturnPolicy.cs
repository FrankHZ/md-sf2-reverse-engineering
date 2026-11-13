using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal sealed record OutcomeStart(ProgramLocation? Program, FieldReturnAnchor Return,
    MapPosition ProjectionPosition, BattleStartInput Party, RaftBerth? ResetRaft = null);
internal sealed record OutcomeFinish(BattleStartInput Recovered, BattleStartInput Returned,
    int? JoinMember, bool ClearUnlocked, bool SetCompleted, ProgramLocation Program);

internal interface IOutcomeReturnPolicy
{
    string Identity { get; }
    OutcomeStart Start(ScenarioDefinition definition, EngineBattleState battle, StoryState story,
        BattleOutcomeKind kind, BattleStartInput party);
    OutcomeFinish Finish(ScenarioDefinition definition, StoryState story, BattleStartInput party);
    // Eligibility for the existing recovery operation; common validation still owns mutation limits.
    bool CanRecover(BattleDeploymentDefinition deployment, BattleActorStartInput actor, bool all);
    BattleStartInput Heal(ScenarioDefinition definition, BattleStartInput party, bool all);
}

internal static class OutcomeReturnRules
{
    internal static OutcomeStart Start(IOutcomeReturnPolicy rule, ScenarioDefinition definition,
        EngineBattleState battle, StoryState story, BattleOutcomeKind kind, BattleStartInput party) =>
        BattleProgressionRules.Invoke(rule.Identity, "outcome-start", () =>
        {
            var result = rule.Start(definition, battle, story, kind, party);
            Party(rule, definition, party, result.Party, gold: false, healing: kind == BattleOutcomeKind.Victory);
            if (kind == BattleOutcomeKind.Retreat)
                Require(result.Program is not null && result.Party.Actors.SequenceEqual(party.Actors));
            Program(definition, result.Program);
            var route = story.EnteringBattle!.Outcome!;
            // Battle terrain and walking collision are independent. Only an actual combatant
            // coordinate (or the leader's deployment when nobody remains) supplies this exception.
            bool fromBoard = route.BattleOrigin is { } origin && IsCombatPosition(battle,
                new(result.ProjectionPosition.X - origin.X, result.ProjectionPosition.Y - origin.Y));
            bool projectedReturn = fromBoard && kind == BattleOutcomeKind.Victory &&
                result.Return.Map == route.BattleMap && result.Return.Position == result.ProjectionPosition;
            Position(definition, result.Return.Map, result.Return.Position, projectedReturn);
            Require(result.Return.Facing <= 3);
            Position(definition, route.BattleMap, result.ProjectionPosition, fromBoard);
            return result;
        });

    internal static OutcomeFinish Finish(IOutcomeReturnPolicy rule, ScenarioDefinition definition,
        StoryState story, BattleStartInput party) => BattleProgressionRules.Invoke(rule.Identity, "outcome-finish", () =>
    {
        var result = rule.Finish(definition, story, party);
        Party(rule, definition, party, result.Recovered, gold: true);
        Party(rule, definition, result.Recovered, result.Returned, gold: false, healing: true);
        Program(definition, result.Program);
        Require(result.JoinMember is null || definition.Exploration!.PartyFlags is { } layout &&
            result.JoinMember >= 0 && result.JoinMember < layout.MemberCount);
        return result;
    });

    internal static BattleStartInput Heal(IOutcomeReturnPolicy rule, ScenarioDefinition definition,
        BattleStartInput party, bool all) => BattleProgressionRules.Invoke(rule.Identity, "party-recovery", () =>
    {
        var result = rule.Heal(definition, party, all);
        Party(rule, definition, party, result, gold: false, healing: true, all: all);
        return result;
    });

    private static void Program(ScenarioDefinition definition, ProgramLocation? location)
    {
        if (location is not { } pc) return;
        Require(definition.Exploration!.Programs.TryGetValue(pc.Program, out var program) &&
            pc.Instruction >= 0 && pc.Instruction < program.Instructions.Count);
    }
    private static bool IsCombatPosition(EngineBattleState battle, MapPosition position)
    {
        var standing = battle.Actors.Where(actor => actor.IsAlly && actor.Hp > 0 && actor.Position is not null).ToArray();
        return standing.Length > 0 ? standing.Any(actor => actor.Position == position)
            : battle.GetActor(battle.Definition.Outcome!.Leader).Deployment.Position == position;
    }

    private static void Position(ScenarioDefinition definition, MapId map, MapPosition position, bool combatPlacement) => Require(
        definition.Exploration!.Maps.TryGetValue(map, out var target) && target.Traversal.IsWithinActiveArea(position) &&
        (combatPlacement || !OriginalMapTraversal.IsBlocked(target.Layout, position)));
    private static void Party(IOutcomeReturnPolicy rule, ScenarioDefinition definition, BattleStartInput before, BattleStartInput after, bool gold,
        bool healing = false, bool all = false)
    {
        Require(after.Encounter == before.Encounter && after.MainSeed == before.MainSeed && after.ThinkingSeed == before.ThinkingSeed &&
            ReferenceEquals(after.ItemCustody, before.ItemCustody) &&
            after.StoryFlags.SequenceEqual(before.StoryFlags) && ReferenceEquals(after.Visuals, before.Visuals) &&
            after.RetainedSourceSlots.SequenceEqual(before.RetainedSourceSlots) && after.DeathCredit == before.DeathCredit &&
            after.NewBattle == before.NewBattle && (after.ActiveAllies is null ? before.ActiveAllies is null : before.ActiveAllies is not null && after.ActiveAllies.SequenceEqual(before.ActiveAllies)) && after.Actors.Count == before.Actors.Count &&
            (gold ? before.Gold.HasValue == after.Gold.HasValue : before.Gold == after.Gold));
        var encounter = definition.Encounters[before.Encounter];
        for (int index = 0; index < before.Actors.Count; index++)
        {
            var a = before.Actors[index]; var b = after.Actors[index];
            var deployment = encounter.Deployments.FirstOrDefault(row => row.Actor == a.Actor) ??
                definition.Encounters.Values.SelectMany(value => value.Deployments)
                    .First(row => row.Faction == BattleFaction.Ally && row.Actor == a.Actor);
            bool canRecover = healing && deployment.Faction == BattleFaction.Ally && rule.CanRecover(deployment, a, all);
            Require(!healing || canRecover || b == a);
            Require(b.Status == a.Status || (canRecover && b.Status == (a.Status & ~(0xC0 | 0x0300))));
            Require(b with { Hp = a.Hp, Mp = a.Mp, Status = a.Status } == a);
            Require(deployment.Faction == BattleFaction.Ally || b == a);
            Require(b.Hp <= (a.Progress?.MaxHp ?? deployment.Definition.MaxHp) &&
                b.Mp <= (a.Progress?.MaxMp ?? deployment.Definition.MaxMp));
        }
    }
    private static void Require(bool valid) { if (!valid) throw new InvalidOperationException("Invalid outcome return result."); }
}
