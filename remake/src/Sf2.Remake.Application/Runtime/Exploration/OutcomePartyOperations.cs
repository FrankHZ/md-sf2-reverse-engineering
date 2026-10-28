using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

// Finite after-battle operations on the retained party, not a second roster.
internal static class OutcomePartyOperations
{
    internal static (ActiveSessionState Active, StoryState Story) Apply(ScenarioDefinition definition,
        SessionSnapshot current, OutcomePartyInstruction operation, StoryState next)
    {
        if (current.Exploration is not { } world ||
            current.Story.Continuation != ProgramContinuation.VictoryProgramFinished ||
            current.Story.EnteringBattle?.Outcome is null ||
            definition.Exploration?.PartyFlags != new MapPartyFlagLayout(30, 0, 32, 12))
            throw new BattleRuleException("outcome-party-context", "program.party", true);
        var encounter = definition.Encounters[current.Story.EnteringBattle.Encounter];
        var binding = encounter.Deployments.SingleOrDefault(d => d.Faction == BattleFaction.Ally &&
            d.Initialization?.AllyPartyMember == operation.Member)
            ?? throw new BattleRuleException("outcome-party-member", "program.member", true);
        var actor = world.Party.Actors.SingleOrDefault(a => a.Actor == binding.Actor)
            ?? throw new BattleRuleException("outcome-party-resource", "party.actor", true);
        switch (operation)
        {
            case ClearOutcomePartyAi:
                ushort word = actor.ActivationWord
                    ?? throw new BattleRuleException("missing-activation-word", "party.activation", true);
                var party = world.Party;
                return (new ActiveExploration(world.WithParty(new(party.Encounter,
                    party.Actors.Select(a => a.Actor == actor.Actor ? a with { ActivationWord = (ushort)(word & ~4) } : a),
                    party.MainSeed, party.ThinkingSeed, party.Gold, party.NewBattle, party.ActiveAllies, party.ItemCustody))), next);
            case LeaveOutcomeBattleParty:
                // Source writes only X=-1 after clearing active membership, without rebuilding lists.
                // BattleOutcome already discards battle coordinates; no raw X/Y authority is retained.
                return (current.Active, next.Copy(next.Cursor,
                    flags: ProgramRunner.Flags(current.Story, 32 + operation.Member, false)));
            default: throw new BattleRuleException("outcome-party-instruction", "program.party", true);
        }
    }
}
