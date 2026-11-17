using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

public enum CaravanAction { Join, Depot, Item, Purge }
public enum CaravanPhase
{
    Menu, Member, ConfirmDead, Relief, Result,
    DepotMenu, DepotMember, DepotSlot, DepotIndex, DepotRecipient,
    DepotExchange, DepotConfirm, DepotLook, DepotResult,
}
public enum CaravanResult { Joined, Purged, LeaderRequired, NoReserve, Unsupported }
public sealed record CaravanWait(WaitToken Token, CaravanPhase Phase = CaravanPhase.Menu,
    CaravanAction? Action = null, int? Member = null, CaravanResult? Result = null,
    DepotAction? DepotAction = null, int? DepotIndex = null, byte? Item = null, string? DepotResult = null) : ProgramWait(Token);
public abstract record CaravanCommand(WaitToken Wait) : SessionCommand;
public sealed record ChooseCaravan(WaitToken Wait, CaravanAction Action) : CaravanCommand(Wait);
public sealed record SelectCaravanMember(WaitToken Wait, int Member) : CaravanCommand(Wait);
public sealed record AnswerCaravan(WaitToken Wait, bool Accept) : CaravanCommand(Wait);
public sealed record BackCaravan(WaitToken Wait) : CaravanCommand(Wait);
public sealed record ContinueCaravan(WaitToken Wait) : CaravanCommand(Wait);

internal static class CaravanService
{
    internal static SessionResult Submit(ScenarioDefinition definition, SessionSnapshot current, CaravanCommand command, SessionRules rules)
    {
        if (current.Exploration is not { } field || current.Story.Wait is not CaravanWait wait || command.Wait != wait.Token)
        {
            return BattleCommandDispatcher.Reject(current, "stale-or-wrong-service", "wait");
        }

        if (wait.Action == CaravanAction.Depot)
        {
            return CaravanDepotService.Submit(definition, current, command, wait);
        }

        try
        {
            var layout = definition.Exploration?.PartyFlags ?? throw new BattleRuleException("party-flag-layout", "world.partyFlags", true);
            var lists = MapPartyMembership.Rebuild(current.Story.Flags, layout);
            var next = wait with { Token = new(current.ObservationSequence + 1) };
            MapPartyJoinResult? changed = null;
            switch (command)
            {
                case ChooseCaravan choose when wait.Phase == CaravanPhase.Menu && Enum.IsDefined(choose.Action):
                    next = Start(next.Token, choose.Action, lists);
                    break;
                case SelectCaravanMember selected when wait.Phase == CaravanPhase.Member:
                    if (wait.Action == CaravanAction.Join)
                    {
                        Require(lists.Reserve.Contains(selected.Member));
                        var actor = Member(definition, current, selected.Member);
                        next = next with { Member = selected.Member };
                        if (actor.Hp == 0)
                        {
                            next = next with { Phase = CaravanPhase.ConfirmDead };
                        }
                        else
                        {
                            Join();
                        }
                    }
                    else if (wait.Action == CaravanAction.Purge)
                    {
                        Require(lists.Active.Contains(selected.Member));
                        _ = Member(definition, current, selected.Member);
                        if (selected.Member == 0)
                        {
                            next = next with { Phase = CaravanPhase.Result, Result = CaravanResult.LeaderRequired };
                        }
                        else
                        {
                            changed = MapPartyMembership.Purge(current.Story.Flags, layout, selected.Member);
                            next = next with { Phase = CaravanPhase.Result, Member = selected.Member, Result = CaravanResult.Purged };
                        }
                    }
                    else
                    {
                        Require(false);
                    }
                    break;
                case AnswerCaravan answer when wait.Phase == CaravanPhase.ConfirmDead:
                    Require(wait.Member is { } dead && lists.Reserve.Contains(dead) && Member(definition, current, dead).Hp == 0);
                    if (!answer.Accept)
                    {
                        next = Start(next.Token, CaravanAction.Join, lists);
                    }
                    else
                    {
                        Join();
                    }
                    break;
                case SelectCaravanMember relief when wait.Phase == CaravanPhase.Relief:
                    Require(wait.Member is { } joiner && lists.Reserve.Contains(joiner) && lists.Active.Count >= layout.Capacity && lists.Active.Contains(relief.Member));
                    _ = Member(definition, current, wait.Member!.Value);
                    _ = Member(definition, current, relief.Member);
                    if (relief.Member == 0)
                    {
                        next = next with { Phase = CaravanPhase.Result, Result = CaravanResult.LeaderRequired };
                    }
                    else
                    {
                        changed = MapPartyMembership.Deploy(current.Story.Flags, layout, wait.Member.Value, relief.Member);
                        next = next with { Phase = CaravanPhase.Result, Result = CaravanResult.Joined };
                    }
                    break;
                case ContinueCaravan when wait.Phase == CaravanPhase.Result:
                    next = wait.Result is CaravanResult.NoReserve or CaravanResult.Unsupported
                        ? new(next.Token) : Start(next.Token, wait.Action!.Value, lists);
                    break;
                case BackCaravan when wait.Phase is CaravanPhase.ConfirmDead or CaravanPhase.Relief:
                    next = Start(next.Token, CaravanAction.Join, lists);
                    break;
                case BackCaravan when wait.Phase == CaravanPhase.Member:
                    next = new(next.Token);
                    break;
                case BackCaravan when wait.Phase == CaravanPhase.Menu:
                    List<SessionObservation> returned = [];
                    var closed = ProgramRunner.Commit(current, current.Active, current.Story.Copy(current.Story.Cursor), returned, "caravan-closed");
                    return ProgramRunner.Run(definition, closed, returned, rules);
                default:
                    return BattleCommandDispatcher.Reject(current, "invalid-service-selection", "service");
            }

            var party = field.Party;
            if (changed is not null)
            {
                var active = changed.Lists.Active.Select(member => Member(definition, current, member).Actor).ToArray();
                party = new(party.Encounter, party.Actors, party.MainSeed, party.ThinkingSeed, party.Gold,
                    party.NewBattle, active, party.ItemCustody, party.StoryFlags, party.Visuals, party.RetainedSourceSlots, party.DeathCredit);
            }
            List<SessionObservation> observations = [];
            current = ProgramRunner.Commit(current, new ActiveExploration(field.WithParty(party)),
                current.Story.Copy(current.Story.Cursor, next, flags: changed?.Flags, partyLists: changed?.Lists),
                observations, changed is null ? "caravan-selection" : "caravan-formation");
            return ProgramRunner.Result(current, observations);

            void Join()
            {
                Require(next.Member is { } member && lists.Reserve.Contains(member));
                _ = Member(definition, current, next.Member!.Value);
                if (lists.Active.Count >= layout.Capacity)
                {
                    next = next with { Phase = CaravanPhase.Relief };
                }
                else
                {
                    changed = MapPartyMembership.Deploy(current.Story.Flags, layout, next.Member.Value);
                    next = next with { Phase = CaravanPhase.Result, Result = CaravanResult.Joined };
                }
            }
        }
        catch (BattleRuleException error)
        {
            return new(current, [], error.Unsupported ? SessionStopReason.Unsupported : SessionStopReason.Rejected,
                new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.IllegalCommand,
                    error.Code, error.Field, error.Code.Replace('-', ' ')));
        }
    }

    private static CaravanWait Start(WaitToken token, CaravanAction action, MapPartyLists lists) => action switch
    {
        CaravanAction.Depot => new(token, CaravanPhase.DepotMenu, action),
        CaravanAction.Item => new(token, CaravanPhase.Result, action, Result: CaravanResult.Unsupported),
        CaravanAction.Join when lists.Reserve.Count == 0 => new(token, CaravanPhase.Result, action, Result: CaravanResult.NoReserve),
        _ => new(token, CaravanPhase.Member, action),
    };

    private static void Require(bool condition)
    {
        if (!condition)
        {
            throw new BattleRuleException("caravan-member-selection", "service.member");
        }
    }

    private static BattleActorStartInput Member(ScenarioDefinition definition, SessionSnapshot current, int member) =>
        PartyDetailsQuery.Query(definition, current)!.Members.SingleOrDefault(row =>
            row.Deployment?.Initialization?.AllyPartyMember == member && row.Carried.Initialized != false)?.Carried
        ?? throw new BattleRuleException("caravan-member-unavailable", "party.member", true);
}
