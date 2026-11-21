using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.Exploration;

internal sealed partial class CaravanServiceView
{
    private void BuildItems(CaravanWait wait)
    {
        _title = GameText.Get("caravan.item");
        var members = _session.QueryPartyDetails()!.Members;
        void Add(string key, CaravanCommand command) => _options.Add((GameText.Get(key), command, null, true));
        switch (wait.Phase)
        {
            case CaravanPhase.ItemMenu:
                foreach (var action in Enum.GetValues<CaravanItemAction>())
                    Add("prepare." + action.ToString().ToLowerInvariant(), new ChooseCaravanItem(wait.Token, action));
                break;
            case CaravanPhase.ItemMember:
            case CaravanPhase.ItemTarget:
                _title += "\n" + GameText.Get(wait.Phase == CaravanPhase.ItemMember ? "depot.donor" : "shop.recipient");
                foreach (var member in members)
                {
                    if (member.Deployment?.Initialization?.AllyPartyMember is not { } index)
                        continue;
                    _options.Add((PartyStatusView.MemberName(member, _session),
                        new SelectCaravanMember(wait.Token, index), member, member.Carried.Initialized != false));
                }
                break;
            case CaravanPhase.ItemSlot:
            case CaravanPhase.ItemExchange:
                bool exchange = wait.Phase == CaravanPhase.ItemExchange;
                _title += "\n" + GameText.Get(exchange ? "depot.exchange" : "prepare." + wait.ItemAction!.Value.ToString().ToLowerInvariant());
                var owner = members.Single(row => row.Deployment?.Initialization?.AllyPartyMember ==
                    (exchange ? wait.ItemTarget : wait.Member));
                var loadout = owner.Carried.SourceLoadout ?? owner.Carried.Progress?.SourceLoadout ?? owner.Deployment!.Definition.SourceLoadout!;
                if (!exchange && wait.ItemAction == CaravanItemAction.Equip)
                {
                    var choices = CaravanItemQuery.Query(_session.Definition, _session.Current, owner.Carried.Actor);
                    if (choices.Error is not null || choices.Equipment.Count == 0)
                    {
                        _title += "\n" + GameText.Get("caravan.items.equipment-unavailable");
                        Add("shop.back", new BackCaravan(wait.Token));
                    }
                    foreach (var choice in choices.Equipment)
                    {
                        _options.Add((DisplayNames.EquipmentRow(choice),
                            new SelectCaravanItemSlot(wait.Token, choice.Slot, choice.ExpectedWord, choice.SourceSlot), owner, true));
                    }
                }
                else
                {
                    for (int slot = 0; slot < loadout.Items.Count; slot++)
                    {
                        ushort word = loadout.Items[slot];
                        byte item = (byte)(word & 127);
                        if (item == 127)
                            continue;
                        string name = DisplayNames.Item(item, CaravanDepotQuery.Name(_session.Definition, item));
                        _options.Add((GameText.Get("shop.slot", slot + 1, name,
                            (word & 128) != 0 ? GameText.Get("shop.equipped") : ""),
                            new SelectCaravanItemSlot(wait.Token, slot, word), owner, true));
                    }
                }
                break;
            case CaravanPhase.ItemConfirm:
                byte dropped = (byte)(wait.ItemWord!.Value & 127);
                _title += "\n" + GameText.Get("depot.confirm-drop", DisplayNames.Item(dropped,
                    CaravanDepotQuery.Name(_session.Definition, dropped)));
                Add("shop.confirm", new AnswerCaravan(wait.Token, true));
                Add("shop.back", new AnswerCaravan(wait.Token, false));
                break;
            case CaravanPhase.ItemResult:
                _title += "\n" + (wait.FieldGrowth is { } growth ? FieldActionView.GrowthMessage(growth) :
                    GameText.Get("caravan.items.result." + wait.ItemResult));
                Add("shop.continue", new ContinueCaravan(wait.Token));
                break;
        }
    }
}
