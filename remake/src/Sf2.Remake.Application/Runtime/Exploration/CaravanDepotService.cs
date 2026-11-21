using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

public enum DepotAction { Look, Deposit, Derive, Drop }
public sealed record ChooseDepot(WaitToken Wait, DepotAction Action) : CaravanCommand(Wait);
public sealed record SelectDepotItem(WaitToken Wait, int Index, byte ExpectedItem) : CaravanCommand(Wait);
public sealed record SelectDepotSlot(WaitToken Wait, int Slot, ushort ExpectedWord) : CaravanCommand(Wait);

internal static class CaravanDepotService
{
    internal static SessionResult Submit(ScenarioDefinition definition, SessionSnapshot current,
        CaravanCommand command, CaravanWait wait)
    {
        var field = current.Exploration!;
        var party = field.Party;
        var next = wait with { Token = new(current.ObservationSequence + 1) };
        if (command is BackCaravan && wait.Phase == CaravanPhase.DepotMenu)
        {
            List<SessionObservation> returned = [];
            var closed = ProgramRunner.Commit(current, current.Active,
                current.Story.Copy(current.Story.Cursor, new CaravanWait(next.Token)), returned, "caravan-selection");
            return ProgramRunner.Result(closed, returned);
        }
        try
        {
            var custody = party.ItemCustody ?? throw new BattleRuleException("depot-unavailable", "party.itemCustody", true);
            switch (command)
            {
                case ChooseDepot choose when wait.Phase == CaravanPhase.DepotMenu && Enum.IsDefined(choose.Action):
                    next = Start(next.Token, choose.Action, custody);
                    break;
                case SelectCaravanMember selected when wait.Phase == CaravanPhase.DepotMember:
                    var donor = Member(definition, current, selected.Member);
                    next = next with { Member = selected.Member, Phase = CaravanPhase.DepotSlot };
                    if (Loadout(donor).Items.All(word => (word & 127) == 127))
                        next = next with { Phase = CaravanPhase.DepotResult, DepotResult = "empty-pack" };
                    break;
                case SelectDepotSlot selected when wait.Phase == CaravanPhase.DepotSlot:
                    var member = Member(definition, current, wait.Member!.Value);
                    _ = CaravanDepotQuery.Terms(definition, (byte)(selected.ExpectedWord & 127));
                    var deposited = CaravanInventory.Deposit(Loadout(member), custody, selected.Slot,
                        selected.ExpectedWord, CaravanDepotQuery.Cursed(definition, selected.ExpectedWord));
                    party = FieldInventory.Replace(party, [FieldInventory.WithLoadout(member, deposited.Pack).RefreshStats()], deposited.Custody);
                    next = next with { Phase = CaravanPhase.DepotResult, DepotResult = "deposited" };
                    break;
                case SelectDepotItem selected when wait.Phase == CaravanPhase.DepotIndex:
                    byte item = CaravanInventory.Selected(custody, selected.Index, selected.ExpectedItem);
                    var terms = CaravanDepotQuery.Terms(definition, item);
                    next = next with { DepotIndex = selected.Index, Item = item };
                    next = wait.DepotAction switch
                    {
                        DepotAction.Look => next with { Phase = CaravanPhase.DepotLook },
                        DepotAction.Derive => next with { Phase = CaravanPhase.DepotRecipient },
                        DepotAction.Drop when terms.Unsellable => next with { Phase = CaravanPhase.DepotResult, DepotResult = "unsellable" },
                        DepotAction.Drop => next with { Phase = CaravanPhase.DepotConfirm },
                        _ => throw new BattleRuleException("depot-action", "depot"),
                    };
                    break;
                case SelectCaravanMember selected when wait.Phase == CaravanPhase.DepotRecipient:
                    var recipient = Member(definition, current, selected.Member);
                    next = next with { Member = selected.Member };
                    if (Loadout(recipient).Items.All(word => (word & 127) != 127))
                        next = next with { Phase = CaravanPhase.DepotExchange };
                    else
                        Retrieve(recipient);
                    break;
                case SelectDepotSlot selected when wait.Phase == CaravanPhase.DepotExchange:
                    Retrieve(Member(definition, current, wait.Member!.Value), selected.Slot, selected.ExpectedWord);
                    break;
                case AnswerCaravan answer when wait.Phase == CaravanPhase.DepotConfirm:
                    if (answer.Accept)
                    {
                        var removed = CaravanInventory.Drop(custody, wait.DepotIndex!.Value, wait.Item!.Value,
                            CaravanDepotQuery.Terms(definition, wait.Item.Value));
                        party = FieldInventory.Replace(party, [], removed);
                        next = next with { Phase = CaravanPhase.DepotResult, DepotResult = "dropped" };
                    }
                    else
                        next = Start(next.Token, DepotAction.Drop, custody);
                    break;
                case ContinueCaravan when wait.Phase is CaravanPhase.DepotResult or CaravanPhase.DepotLook:
                    next = wait.DepotResult is "empty" or "full"
                        ? Menu(next.Token) : Start(next.Token, wait.DepotAction!.Value, custody);
                    break;
                case BackCaravan:
                    next = wait.Phase switch
                    {
                        CaravanPhase.DepotSlot => Start(next.Token, DepotAction.Deposit, custody),
                        CaravanPhase.DepotRecipient or CaravanPhase.DepotConfirm or CaravanPhase.DepotLook => Start(next.Token, wait.DepotAction!.Value, custody),
                        CaravanPhase.DepotExchange => next with { Phase = CaravanPhase.DepotRecipient, Member = null },
                        CaravanPhase.DepotResult => wait.DepotResult is "empty" or "full"
                            ? Menu(next.Token) : Start(next.Token, wait.DepotAction!.Value, custody),
                        _ => Menu(next.Token),
                    };
                    break;
                default:
                    return BattleCommandDispatcher.Reject(current, "invalid-service-selection", "depot");
            }
            List<SessionObservation> observations = [];
            var changed = ProgramRunner.Commit(current, new ActiveExploration(field.WithParty(party)),
                current.Story.Copy(current.Story.Cursor, next), observations,
                ReferenceEquals(party, field.Party) ? "caravan-selection" : "caravan-depot");
            return ProgramRunner.Result(changed, observations);

            void Retrieve(PartyMemberDetails recipient, int? slot = null, ushort? word = null)
            {
                _ = CaravanDepotQuery.Terms(definition, wait.Item!.Value);
                if (word is { } returned)
                    _ = CaravanDepotQuery.Terms(definition, (byte)(returned & 127));
                var retrieved = CaravanInventory.Derive(Loadout(recipient), custody, wait.DepotIndex!.Value,
                    wait.Item.Value, slot, word, word is { } outgoing && CaravanDepotQuery.Cursed(definition, outgoing));
                party = FieldInventory.Replace(party, [FieldInventory.WithLoadout(recipient, retrieved.Pack)], retrieved.Custody);
                next = next with { Phase = CaravanPhase.DepotResult, DepotResult = "derived" };
            }
        }
        catch (BattleRuleException error)
        {
            return new(current, [], error.Unsupported ? SessionStopReason.Unsupported : SessionStopReason.Rejected,
                new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.IllegalCommand,
                    error.Code, error.Field, error.Code.Replace('-', ' ')));
        }
    }

    private static CaravanWait Menu(WaitToken token) => new(token, CaravanPhase.DepotMenu, CaravanAction.Depot);

    private static CaravanWait Start(WaitToken token, DepotAction action, BattleItemCustody custody)
    {
        var wait = Menu(token) with { DepotAction = action };
        if (action == DepotAction.Deposit)
            return custody.Depot.Count == 64
                ? wait with { Phase = CaravanPhase.DepotResult, DepotResult = "full" }
                : wait with { Phase = CaravanPhase.DepotMember };
        return custody.Depot.Count == 0
            ? wait with { Phase = CaravanPhase.DepotResult, DepotResult = "empty" }
            : wait with { Phase = CaravanPhase.DepotIndex };
    }

    private static PartyMemberDetails Member(ScenarioDefinition definition, SessionSnapshot current, int member) =>
        PartyDetailsQuery.Query(definition, current)!.Members.SingleOrDefault(row =>
            row.Deployment?.Initialization?.AllyPartyMember == member && row.Carried.Initialized != false)
        ?? throw new BattleRuleException("caravan-member-unavailable", "party.member");

    private static BattleSourceLoadout Loadout(PartyMemberDetails member)
    {
        var pack = FieldInventory.Loadout(member);
        if (pack.Items.Count != 4 || pack.Items.Any(word => word > 511))
            throw new BattleRuleException("depot-loadout", "member.items");
        return pack;
    }
}
