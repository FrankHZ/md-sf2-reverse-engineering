using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Application.Gameplay.Sf2;

// Saved prefixes in the private opening profile, including the gap between Luke and Elric.
internal static class Sf2HistoricalRoster
{
    private static readonly (int Members, int LastBattle)[] Revisions = [(7, 8), (9, 10), (10, 13), (11, 19), (12, 20)];
    private static readonly int[] MemberIds = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 13];

    internal static SavedBattleStory UpgradeBattle(ScenarioDefinition definition, BattleSaveData data,
        EngineBattleState battle, SaveCompatibility expected)
    {
        var roster = definition.Encounters.Values.SelectMany(encounter => encounter.Deployments)
            .Where(row => row.Faction == BattleFaction.Ally).DistinctBy(row => row.Actor).ToDictionary(row => row.Actor);
        var player = data.Story.Player;
        var saved = battle.Actors.Where(actor => actor.IsAlly).Select(actor => actor.Actor)
            .Concat(player.RetainedAllies.Select(actor => actor.Actor)).ToHashSet();
        // All legacy v2 writers stamped recipe 1, including those with 9, 10 or 11 members.
        int prior = data.Compatibility.RecipeRevision == 1
            ? Array.FindIndex(Revisions, 0, 4, row => row.Members == saved.Count) + 1 : data.Compatibility.RecipeRevision;
        if (prior < 1 || prior > Revisions.Length)
            throw new BattleRuleException("save-battle-retained-roster", "save.story");
        int lastEncounter = data.Compatibility.RecipeRevision == 1 && prior == 4 ? 13 : Revisions[prior - 1].LastBattle;
        if (!Enumerable.Range(1, lastEncounter).Any(id => data.Battle.Encounter == $"battle-{id}"))
            throw new BattleRuleException("save-battle-retained-roster", "save.story");
        var added = Missing(definition, prior, expected, roster, saved, data.Story.Flags, data.Story.Names.Keys,
            data.Battle.MainSeed, "save-battle-retained-roster");
        return data.Story with { Player = player with { RetainedAllies = [.. player.RetainedAllies, .. added] } };
    }

    internal static IReadOnlyList<BattleActorStartInput> Missing(ScenarioDefinition definition, int prior,
        SaveCompatibility expected, IReadOnlyDictionary<ActorRef, BattleDeploymentDefinition> deployments,
        IReadOnlySet<ActorRef> saved, IReadOnlyList<int> flags, IEnumerable<string> claimedActors, uint mainSeed,
        string rosterError)
    {
        int current = expected.RecipeRevision;
        if (expected is not { GameplayRevision: 4, Package: "private-battle01-to-battle02" } ||
            prior < 1 || current > Revisions.Length || prior > current)
            throw new BattleRuleException("save-incompatible", "save.compatibility");

        var rows = deployments.Values.OrderBy(row => row.Initialization?.AllyPartyMember).ToArray();
        var layout = definition.Exploration!.PartyFlags;
        var source = definition.PrivateDefinitions;
        if (source is null || layout is null || rows.Length != Revisions[current - 1].Members ||
            rows.Where((row, id) => row.Initialization?.AllyPartyMember != MemberIds[id] || row.Actor.Value != $"ally-{MemberIds[id]}").Any() ||
            !saved.SetEquals(rows.Take(Revisions[prior - 1].Members).Select(row => row.Actor)))
            throw new BattleRuleException(rosterError, "save.allies");

        var added = new List<BattleActorStartInput>();
        for (int revision = prior; revision < current; revision++)
        {
            // Replay each expansion as its writer did on a single upgrade. Growth draws
            // affect only that expansion, even when Continue crosses several at once.
            uint seed = mainSeed;
            for (int index = Revisions[revision - 1].Members; index < Revisions[revision].Members; index++)
            {
                int id = MemberIds[index];
                var row = rows[index];
                if (flags.Contains(layout.JoinedStart + id) || flags.Contains(layout.ActiveStart + id) ||
                    claimedActors.Contains(row.Actor.Value) || !source.Allies.TryGetValue((byte)id, out var ally))
                    throw new BattleRuleException(rosterError, "save.allies");
                added.Add(OpeningPartyInitialization.Initialize(row, ally.StartingLevel, ref seed));
            }
        }
        return added;
    }
}
