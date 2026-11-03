using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

public enum ChurchAction { Raise, Cure }
public enum ChurchEffect { Raise, Poison, Stun }
public sealed record ChurchOffer(ActorRef Member, ChurchEffect Effect, uint Cost, ushort Hp, ushort Status);
public sealed record ChurchWait(WaitToken Token, ChurchDefinition Definition, ChurchAction? Action = null,
    IReadOnlyList<ChurchOffer>? Offers = null, int Index = 0, string? Message = null) : ProgramWait(Token)
{
    public ChurchOffer? Offer => Offers is not null && Index < Offers.Count ? Offers[Index] : null;
}
public abstract record ChurchCommand(WaitToken Wait) : SessionCommand;
public sealed record ChooseChurch(WaitToken Wait, ChurchAction Action) : ChurchCommand(Wait);
public sealed record AnswerChurch(WaitToken Wait, ChurchOffer Offer, bool Accept) : ChurchCommand(Wait);
public sealed record ContinueChurch(WaitToken Wait) : ChurchCommand(Wait);
public sealed record LeaveChurch(WaitToken Wait, bool Save = false) : ChurchCommand(Wait);

internal static class FieldService
{
    internal static SessionResult Submit(ScenarioDefinition definition, SessionSnapshot current, ChurchCommand command, SessionRules rules)
    {
        if (current.Exploration is not { } field || current.Story.Wait is not ChurchWait wait || wait.Token != command.Wait)
            return BattleCommandDispatcher.Reject(current, "stale-or-wrong-service", "wait");
        List<SessionObservation> observations = [];
        try
        {
            var next = wait with { Token = new(current.ObservationSequence + 1) };
            var party = field.Party;
            string kind;
            switch (command)
            {
                case ChooseChurch choose when wait.Action is null && Enum.IsDefined(choose.Action):
                    var offers = Offers(definition, current, wait.Definition, choose.Action);
                    next = next with { Action = choose.Action, Offers = offers, Index = 0,
                        Message = offers.Count == 0 ? Completion(definition, current, choose.Action, "No applicable member.") : null };
                    kind = "church-action"; break;
                case AnswerChurch answer when wait.Action is not null && wait.Message is null && wait.Offer == answer.Offer:
                    var offer = wait.Offer!;
                    // Recheck the complete immutable offer against live membership/vitals/cost.
                    if (!Offers(definition, current, wait.Definition, wait.Action.Value).Contains(offer))
                        throw new BattleRuleException("stale-service-offer", "service.offer");
                    int index = wait.Index + 1;
                    string message;
                    if (!answer.Accept) { kind = "church-declined"; message = "Treatment declined."; }
                    else
                    {
                        uint gold = party.Gold ?? throw new BattleRuleException("service-gold-unavailable", "party.gold", true);
                        if (gold < offer.Cost)
                        {
                            kind = "church-insufficient-gold"; message = "Not enough gold. No payment taken.";
                            // Source Cure aborts the remaining members of this status family.
                            if (wait.Action == ChurchAction.Cure)
                                while (index < wait.Offers!.Count && wait.Offers[index].Effect == offer.Effect) index++;
                        }
                        else
                        {
                            var member = PartyDetailsQuery.Query(definition, current)!.Members.Single(row => row.Carried.Actor == offer.Member);
                            var changed = offer.Effect == ChurchEffect.Raise
                                ? member.Carried with { Hp = Math.Min(member.MaxHp!.Value, wait.Definition.RaisedHpCap) }
                                : member.Carried with { Status = (ushort)(member.Carried.Status & ~(offer.Effect == ChurchEffect.Poison ? 2 : 1)) };
                            party = FieldInventory.Replace(party, [changed], gold: gold - offer.Cost);
                            next = next with { Offers = Array.AsReadOnly(wait.Offers!.Select(row => row.Member == changed.Actor
                                ? row with { Hp = changed.Hp, Status = changed.Status } : row).ToArray()) };
                            kind = "church-paid";
                            string outcome = offer.Effect switch
                            {
                                ChurchEffect.Poison => "Poison removed", ChurchEffect.Stun => "Stun removed", _ => "Raise completed",
                            };
                            message = $"{outcome}. Paid {offer.Cost} gold.";
                        }
                    }
                    next = next with { Index = index, Message = message }; break;
                case ContinueChurch when wait.Message is not null:
                    next = wait.Offer is null ? new(next.Token, wait.Definition) : next with { Message = null };
                    kind = "church-continued"; break;
                case LeaveChurch when wait.Action is null:
                    current = ProgramRunner.Commit(current, current.Active, current.Story.Copy(current.Story.Cursor), observations,
                        command is LeaveChurch { Save: true } ? "church-save-requested" : "church-closed");
                    return ProgramRunner.Run(definition, current, observations, rules);
                default: return BattleCommandDispatcher.Reject(current, "invalid-service-selection", "service");
            }
            if (next.Action is { } action && next.Offer is null)
                next = next with { Message = Completion(definition, current, action, next.Message ?? "Finished.") };
            current = ProgramRunner.Commit(current, new ActiveExploration(field.WithParty(party)),
                current.Story.Copy(current.Story.Cursor, next), observations, kind);
            return ProgramRunner.Result(current, observations);
        }
        catch (BattleRuleException error)
        {
            return new(current, [], error.Unsupported ? SessionStopReason.Unsupported : SessionStopReason.Rejected,
                new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.IllegalCommand,
                    error.Code, error.Field, error.Code.Replace('-', ' ')));
        }
    }

    private static string Completion(ScenarioDefinition definition, SessionSnapshot current, ChurchAction action, string message) =>
        action == ChurchAction.Cure && PartyDetailsQuery.Query(definition, current)!.Members.Any(member => (member.Carried.Status & 4) != 0)
            ? message + " Curse removal is unavailable in this content." : message;

    private static IReadOnlyList<ChurchOffer> Offers(ScenarioDefinition definition, SessionSnapshot current, ChurchDefinition terms, ChurchAction action)
    {
        var members = PartyDetailsQuery.Query(definition, current)!.Members;
        // PartyDetails preserves carried order; private source membership uses its member index order.
        var ordered = members.OrderBy(member => member.Deployment?.Initialization?.AllyPartyMember ?? int.MaxValue).ToArray();
        List<ChurchOffer> offers = [];
        foreach (var effect in action == ChurchAction.Raise ? new[] { ChurchEffect.Raise } : new[] { ChurchEffect.Poison, ChurchEffect.Stun })
            foreach (var member in ordered)
            {
                if (member.Carried.Initialized == false || member.Deployment is null)
                    throw new BattleRuleException("service-member-unavailable", "party.member", true);
                var actor = member.Carried;
                bool applicable = effect == ChurchEffect.Raise ? actor.Hp == 0 : (actor.Status & (effect == ChurchEffect.Poison ? 2 : 1)) != 0;
                if (!applicable) continue;
                uint cost;
                if (effect == ChurchEffect.Raise)
                {
                    if (member.Level is null || member.MaxHp is null or 0 || member.Deployment.Definition.Physical is not { } physical)
                        throw new BattleRuleException("service-raise-definition", "party.member", true);
                    cost = checked((uint)(member.Level.Value * terms.RaisePerLevel + (physical.Promoted ? terms.PromotedExtra : 0)));
                }
                else cost = effect == ChurchEffect.Poison ? terms.PoisonCost : terms.StunCost;
                offers.Add(new(actor.Actor, effect, cost, actor.Hp, actor.Status));
            }
        return offers.AsReadOnly();
    }
}
