using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class BattleEntry
{
    internal static SessionSnapshot Continue(ScenarioDefinition definition, SessionSnapshot current, List<SessionObservation> observations)
    {
        var story = current.Story;
        if (story.Continuation == ProgramContinuation.MapLoaded)
        {
            var route = current.Exploration?.Definition.Battle;
            if (route is null || (route.UnlockedFlag is { } unlocked && !story.Flags.Contains(unlocked)))
                return current.WithStory(story.Copy(null, continuation: ProgramContinuation.FieldInput));
            if (route.CompletedFlag is { } completed && story.Flags.Contains(completed))
                return ProgramRunner.Commit(current, current.Active,
                    story.Copy(null, flags: route.UnlockedFlag is { } flag ? ProgramRunner.Flags(story, flag, false) : story.Flags,
                        continuation: ProgramContinuation.FieldInput), observations, "battle-already-completed", route.Encounter);
            return Select(current, story, route, observations);
        }

        var selected = story.EnteringBattle ?? throw new BattleRuleException("entry-route", "entry");
        if (story.Continuation == ProgramContinuation.BattleLoadFinished)
        {
            bool skipStart = selected.IntroFlag is { } flag && story.Flags.Contains(flag);
            return ProgramRunner.Commit(current, current.Active,
                story.Copy(skipStart ? null : selected.StartProgram,
                    flags: selected.IntroFlag is { } intro ? ProgramRunner.Flags(story, intro, true) : story.Flags,
                    continuation: ProgramContinuation.BattleStartFinished, clearCameraEntity: true, clearCameraTarget: true),
                observations, "battle-loaded");
        }

        var world = current.Exploration ?? throw new BattleRuleException("entry-mode", "entry");
        if (!definition.Encounters.TryGetValue(selected.Encounter, out var encounter))
            throw new BattleRuleException("missing-encounter", "entry.encounter");
        if (world.Party.NewBattle is not null && story.Flags.Contains(88))
            throw new BattleRuleException("suspended-battle-entry", "entry.flags", true);
        MapPartyLists? partyLists = story.PartyLists;
        ActorRef[]? active = null;
        if (world.Party.NewBattle is not null && definition.Exploration!.PartyFlags is { } membership)
        {
            partyLists = MapPartyMembership.Rebuild(story.Flags, membership);
            var members = encounter.Deployments.Where(row => row.Faction == BattleFaction.Ally).ToDictionary(
                row => row.Initialization?.AllyPartyMember ?? throw new BattleRuleException("party-member-binding", "entry.party", true), row => row.Actor);
            active = partyLists.Active.Select(member => members.TryGetValue(member, out var actor) ? actor :
                throw new BattleRuleException("party-member-definition", "entry.party", true)).ToArray();
        }
        var input = new BattleStartInput(selected.Encounter, world.Party.Actors, world.Party.MainSeed,
            world.Party.ThinkingSeed, world.Party.Gold,
            world.Party.NewBattle is { } policy ? policy with { SkipIntro = false, BeforeBattleRouted = true } : null, active);
        // Validate and compute the whole initialization before publishing its flags or actor state.
        var battle = BattleTurnFlow.Start(encounter, input);
        var flags = input.NewBattle is null ? story.Flags : story.Flags.Where(flag => flag is < 90 or > 105);
        return ProgramRunner.Commit(current, new ActiveBattle(battle, null),
            story.Copy(selected.LoadProgram, flags: flags, partyLists: partyLists, continuation: ProgramContinuation.BattleLoadFinished),
            observations, "battle-initialized");
    }

    internal static SessionSnapshot Select(SessionSnapshot current, StoryState story, ExplorationBattleRoute route,
        List<SessionObservation> observations)
    {
        if (current.Exploration!.Party.NewBattle is not null && story.Flags.Contains(88))
            throw new BattleRuleException("suspended-battle-entry", "entry.flags", true);
        bool introSeen = route.IntroFlag is { } intro && story.Flags.Contains(intro);
        return ProgramRunner.Commit(current, current.Active,
            story.Copy(introSeen ? null : route.BeforeProgram, continuation: ProgramContinuation.BeforeBattleFinished,
                flags: current.Exploration.Party.NewBattle is null ? story.Flags : ProgramRunner.Flags(story, 399, true),
                enteringBattle: route), observations, "battle-selected", route.Encounter);
    }
}
