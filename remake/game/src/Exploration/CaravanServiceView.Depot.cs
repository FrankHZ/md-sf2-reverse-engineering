using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.Exploration;

internal sealed partial class CaravanServiceView
{
    private void BuildDepot(CaravanWait wait)
    {
        var depot = _session.Current.Exploration!.Party.ItemCustody?.Depot;
        _title = GameText.Get("depot.title", depot?.Count.ToString() ?? "?");
        var members = _session.QueryPartyDetails()!.Members;
        void Add(string key, CaravanCommand command) => _options.Add((GameText.Get(key), command, null, true));
        string ItemName(byte item) => DisplayNames.Item(item,
            CaravanDepotQuery.Name(_session.Definition, item));

        switch (wait.Phase)
        {
            case CaravanPhase.DepotMenu:
                foreach (var action in Enum.GetValues<DepotAction>())
                    Add("depot." + action.ToString().ToLowerInvariant(), new ChooseDepot(wait.Token, action));
                break;
            case CaravanPhase.DepotMember:
            case CaravanPhase.DepotRecipient:
                _title += "\n" + GameText.Get(wait.Phase == CaravanPhase.DepotMember ? "depot.donor" : "shop.recipient");
                foreach (var member in members)
                {
                    if (member.Deployment?.Initialization?.AllyPartyMember is not { } index)
                        continue;
                    _options.Add((PartyStatusView.MemberName(member, _session),
                        new SelectCaravanMember(wait.Token, index), member, member.Carried.Initialized != false));
                }
                break;
            case CaravanPhase.DepotSlot:
            case CaravanPhase.DepotExchange:
                _title += "\n" + GameText.Get(wait.Phase == CaravanPhase.DepotExchange ? "depot.exchange" : "depot.deposit");
                var owner = members.Single(row => row.Deployment?.Initialization?.AllyPartyMember == wait.Member);
                var loadout = owner.Carried.SourceLoadout ?? owner.Carried.Progress?.SourceLoadout ?? owner.Deployment!.Definition.SourceLoadout!;
                for (int slot = 0; slot < loadout.Items.Count; slot++)
                {
                    ushort word = loadout.Items[slot];
                    byte item = (byte)(word & 127);
                    if (item == 127)
                        continue;
                    _options.Add((GameText.Get("shop.slot", slot + 1, ItemName(item),
                        (word & 128) != 0 ? GameText.Get("shop.equipped") : ""),
                        new SelectDepotSlot(wait.Token, slot, word), owner, true));
                }
                break;
            case CaravanPhase.DepotIndex:
                _title += "\n" + GameText.Get("depot." + wait.DepotAction!.Value.ToString().ToLowerInvariant());
                for (int index = 0; index < depot!.Count; index++)
                    _options.Add(($"{index + 1}. {ItemName(depot[index])}",
                        new SelectDepotItem(wait.Token, index, depot[index]), null, true));
                break;
            case CaravanPhase.DepotConfirm:
                _title += "\n" + GameText.Get("depot.confirm-drop", ItemName(wait.Item!.Value));
                Add("shop.confirm", new AnswerCaravan(wait.Token, true));
                Add("shop.back", new AnswerCaravan(wait.Token, false));
                break;
            case CaravanPhase.DepotLook:
                var appraisal = CaravanDepotQuery.Look(_session.Definition, _session.Current, wait.Item!.Value);
                _title += "\n" + ItemName(appraisal.Item) + " · " + GameText.Get("depot.kind." + appraisal.Kind);
                _title += "\n" + GameText.Get("depot.value", appraisal.SaleValue?.ToString() ?? "?");
                if (appraisal.Eligible is { } eligible)
                    _title += "\n" + GameText.Get("depot.eligible", eligible.Count == 0 ? GameText.Get("depot.none") :
                        string.Join(", ", members.Where(row => eligible.Contains(row.Carried.Actor)).Select(row => PartyStatusView.MemberName(row, _session))));
                if (appraisal.WiderAppraisalUnavailable)
                    _title += "\n" + GameText.Get("depot.appraisal-unavailable");
                Add("shop.continue", new ContinueCaravan(wait.Token));
                break;
            case CaravanPhase.DepotResult:
                _title += "\n" + GameText.Get("depot.result." + wait.DepotResult);
                Add("shop.continue", new ContinueCaravan(wait.Token));
                break;
        }
    }
}
