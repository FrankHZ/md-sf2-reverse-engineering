using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

public enum CaravanItemAction { Use, Give, Equip, Drop }
public sealed record ChooseCaravanItem(WaitToken Wait, CaravanItemAction Action) : CaravanCommand(Wait);
public sealed record SelectCaravanItemSlot(WaitToken Wait, int Slot, ushort? ExpectedWord, int? ExpectedSourceSlot = null) : CaravanCommand(Wait);

public static class CaravanItemQuery
{
    public static FieldActionChoices Query(ScenarioDefinition definition, SessionSnapshot current, ActorRef actor) =>
        FieldInventory.Query(definition, current, actor, FieldItemCaller.Caravan);
}

// Service phases select operands; Field's actual mutation owns the prepared party.
internal static class CaravanItemService
{
    internal static SessionResult Submit(ScenarioDefinition definition, SessionSnapshot current,
        CaravanCommand command, CaravanWait wait)
    {
        var field = current.Exploration!;
        var party = field.Party;
        var token = new WaitToken(current.ObservationSequence + 1);
        var next = wait with { Token = token };
        try
        {
            switch (command)
            {
                case ChooseCaravanItem choose when wait.Phase == CaravanPhase.ItemMenu && Enum.IsDefined(choose.Action):
                    next = Start(token, choose.Action);
                    break;
                case SelectCaravanMember selected when wait.Phase == CaravanPhase.ItemMember:
                    var member = Member(definition, current, selected.Member);
                    next = next with { Member = selected.Member, Phase = CaravanPhase.ItemSlot };
                    if (FieldInventory.Loadout(member).Items.All(word => (word & 127) == 127))
                        next = next with { Phase = CaravanPhase.ItemResult, ItemResult = "empty-pack" };
                    break;
                case SelectCaravanItemSlot selected when wait.Phase == CaravanPhase.ItemSlot:
                    var donor = Member(definition, current, wait.Member!.Value);
                    next = next with { ItemSlot = selected.Slot, ItemWord = selected.ExpectedWord };
                    if (wait.ItemAction == CaravanItemAction.Equip)
                    {
                        Apply(new FieldEquip(donor.Carried.Actor, selected.Slot, selected.ExpectedWord, selected.ExpectedSourceSlot), "equipped");
                        break;
                    }
                    if (selected.ExpectedWord is not { } expected)
                        throw new BattleRuleException("field-stale-item", "item.word");
                    if (wait.ItemAction == CaravanItemAction.Use)
                    {
                        _ = FieldInventory.Slot(definition, donor, selected.Slot, expected);
                        _ = FieldRecovery.CaravanEffect(definition.FieldPreparation!, (byte)(expected & 127));
                        next = next with { Phase = CaravanPhase.ItemTarget };
                    }
                    else if (wait.ItemAction == CaravanItemAction.Give)
                    {
                        // Validate the outgoing word/refusal with the real self-Give rule, without publishing.
                        _ = FieldInventory.Prepare(definition, current,
                            new FieldGive(donor.Carried.Actor, selected.Slot, expected, donor.Carried.Actor), FieldItemCaller.Caravan);
                        next = next with { Phase = CaravanPhase.ItemTarget };
                    }
                    else if (wait.ItemAction == CaravanItemAction.Drop)
                    {
                        _ = FieldInventory.Prepare(definition, current,
                            new FieldDrop(donor.Carried.Actor, selected.Slot, expected), FieldItemCaller.Caravan);
                        next = next with { Phase = CaravanPhase.ItemConfirm };
                    }
                    else
                        throw new BattleRuleException("caravan-item-action", "item");
                    break;
                case SelectCaravanMember selected when wait.Phase == CaravanPhase.ItemTarget:
                    var target = Member(definition, current, selected.Member);
                    var source = Member(definition, current, wait.Member!.Value);
                    next = next with { ItemTarget = selected.Member };
                    if (wait.ItemAction == CaravanItemAction.Use)
                    {
                        party = FieldRecovery.Prepare(definition, current,
                            new FieldUseItem(source.Carried.Actor, wait.ItemSlot!.Value, wait.ItemWord!.Value, target.Carried.Actor),
                            out var effect, out bool effective, out var growth, out _, FieldItemCaller.Caravan);
                        string result = effective ? "cured" : "no-effect";
                        next = next with { Phase = CaravanPhase.ItemResult,
                            ItemResult = growth is not null ? "growth" :
                                effect == FieldRecoveryEffect.CurePoisonAndParalysis ? "powder-" + result : result,
                            FieldGrowth = growth };
                    }
                    else if (wait.ItemAction == CaravanItemAction.Give)
                    {
                        if (selected.Member != wait.Member && FieldInventory.Loadout(target).Items.All(word => (word & 127) != 127))
                            next = next with { Phase = CaravanPhase.ItemExchange };
                        else
                            Give();
                    }
                    else
                        throw new BattleRuleException("caravan-item-action", "item");
                    break;
                case SelectCaravanItemSlot selected when wait.Phase == CaravanPhase.ItemExchange:
                    Give(selected.Slot, selected.ExpectedWord);
                    break;
                case AnswerCaravan answer when wait.Phase == CaravanPhase.ItemConfirm:
                    if (answer.Accept)
                        Apply(new FieldDrop(Member(definition, current, wait.Member!.Value).Carried.Actor,
                            wait.ItemSlot!.Value, wait.ItemWord!.Value), "dropped");
                    else
                        next = Start(token, wait.ItemAction!.Value);
                    break;
                case ContinueCaravan when wait.Phase == CaravanPhase.ItemResult:
                    next = Start(token, wait.ItemAction!.Value);
                    break;
                case BackCaravan:
                    next = wait.Phase switch
                    {
                        CaravanPhase.ItemMenu => new(token),
                        CaravanPhase.ItemMember => Menu(token),
                        CaravanPhase.ItemSlot or CaravanPhase.ItemTarget or CaravanPhase.ItemConfirm or CaravanPhase.ItemResult =>
                            Start(token, wait.ItemAction!.Value),
                        CaravanPhase.ItemExchange => next with { Phase = CaravanPhase.ItemTarget, ItemTarget = null },
                        _ => throw new BattleRuleException("invalid-service-selection", "item"),
                    };
                    break;
                default:
                    return BattleCommandDispatcher.Reject(current, "invalid-service-selection", "item");
            }
            List<SessionObservation> observations = [];
            var changed = ProgramRunner.Commit(current, new ActiveExploration(field.WithParty(party)),
                current.Story.Copy(current.Story.Cursor, next), observations,
                ReferenceEquals(party, field.Party) ? "caravan-selection" : "caravan-item");
            return ProgramRunner.Result(changed, observations);

            void Apply(FieldCommand mutation, string result)
            {
                party = FieldInventory.Prepare(definition, current, mutation, FieldItemCaller.Caravan);
                next = next with { Phase = CaravanPhase.ItemResult, ItemResult = result };
            }

            void Give(int? slot = null, ushort? word = null)
            {
                Apply(new FieldGive(Member(definition, current, wait.Member!.Value).Carried.Actor,
                    wait.ItemSlot!.Value, wait.ItemWord!.Value,
                    Member(definition, current, next.ItemTarget!.Value).Carried.Actor, slot, word), "given");
            }
        }
        catch (BattleRuleException error)
        {
            return new(current, [], error.Unsupported ? SessionStopReason.Unsupported : SessionStopReason.Rejected,
                new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.IllegalCommand,
                    error.Code, error.Field, error.Code.Replace('-', ' ')));
        }
    }

    private static CaravanWait Menu(WaitToken token) => new(token, CaravanPhase.ItemMenu, CaravanAction.Item);
    private static CaravanWait Start(WaitToken token, CaravanItemAction action) =>
        Menu(token) with { Phase = CaravanPhase.ItemMember, ItemAction = action };

    private static PartyMemberDetails Member(ScenarioDefinition definition, SessionSnapshot current, int index)
    {
        var member = PartyDetailsQuery.Query(definition, current)!.Members.SingleOrDefault(row =>
            row.Deployment?.Initialization?.AllyPartyMember == index)
            ?? throw new BattleRuleException("caravan-member-unavailable", "party.member");
        return FieldInventory.Member(definition, current, member.Carried.Actor, FieldItemCaller.Caravan);
    }
}
