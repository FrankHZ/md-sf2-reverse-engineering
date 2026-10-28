using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

// Fixed source field service. Story/party snapshots remain the only mutable authorities.
internal static class SourceRandomBattleRunner
{
    internal static SessionSnapshot Check(SessionSnapshot current, SourceRandomBattle instruction,
        ProgramLocation caller, List<SessionObservation> observations)
    {
        var world = current.Exploration ?? throw new BattleRuleException("random-battle-mode", "program.battle", true);
        var story = current.Story;
        var party = world.Party;
        bool request = !story.Flags.Contains(500 + instruction.Battle);
        short? steps = story.StepCounter;
        if (!request)
        {
            if (steps is null) throw new BattleRuleException("random-battle-step-unbound", "story.stepCounter", true);
            if (steps == 0)
            {
                var first = BattleRandom.NextMain(party.MainSeed, 8);
                uint seed = first.After;
                request = first.Value == 0;
                if (!request)
                {
                    var second = BattleRandom.NextMain(seed, 4);
                    seed = second.After;
                    steps = (short)(second.Value + 2);
                }
                party = new(party.Encounter, party.Actors, seed, party.ThinkingSeed, party.Gold, party.NewBattle, party.ActiveAllies, party.ItemCustody);
            }
        }
        var next = ProgramRunner.Next(caller);
        story = story.Copy(request ? instruction.OnRequest : next,
            flags: request ? ProgramRunner.Flags(story, 400 + instruction.Battle, true) : story.Flags,
            stepCounter: request ? (short)30000 : steps, battleRequest: request || story.BattleRequest,
            callers: request ? story.Callers.Append(next) : story.Callers);
        return ProgramRunner.Commit(current, new ActiveExploration(world.WithParty(party)), story, observations,
            request ? "random-battle-requested" : "random-battle-deferred", instruction.Battle.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    internal static SessionSnapshot Consume(ScenarioDefinition definition, SessionSnapshot current,
        List<SessionObservation> observations, SessionRules rules)
    {
        // $000100FF owns only type1/parameter1=0/map=CURRENT. No new X/Y/facing is produced.
        // The admitted wildcard encounter rows do not inspect retained position operands.
        var route = BattleEntry.First(definition, current.Story, current.Exploration!.Definition, rules)
            ?? throw new BattleRuleException("random-battle-request-route", "map.battles", true);
        var story = current.Story.Copy(null, battleRequest: false);
        current = ProgramRunner.Commit(current, current.Active, story, observations, "random-battle-request-consumed");
        return BattleEntry.Select(current, story, route, observations);
    }
}
