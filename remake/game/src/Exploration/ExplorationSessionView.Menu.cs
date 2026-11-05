using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Exploration;

public sealed partial class ExplorationSessionView
{
    private FieldMenuView _fieldMenu = null!;
    private bool _fieldMenuActive;
    private bool CanOpenFieldMenu => _session?.Current.CanWaitAtInput == true && !_battleMounted && !_partyStatus.Open &&
        !_handedOff && !_battleHandoffPending && !MenuEntryPending && PresentationFailure is null;
    private void BeginFieldMenu()
    {
        if (!CanOpenFieldMenu) return;
        _fieldMenuActive = true; _movementHold.Clear(_input); CancelFieldWait(); _returnFeedback.Dismiss();
        _tickTime = 0; _resumeAutomatic = true; _fieldMenu.Begin(); Present();
    }
    private void CloseFieldMenu()
    {
        _fieldMenuActive = false; _fieldMenu.HideMenu(); _partyStatus.Hide();
        _tickTime = 0; _resumeAutomatic = true; _skipRequiresRelease = true;
        _movementHold.Clear(_input); CancelFieldWait(); Present();
    }
    private void ChooseFieldMenu(int index)
    {
        if (_session?.Current.CanWaitAtInput != true) { CloseFieldMenu(); return; }
        if (index == 3)
        {
            if (!_session.CanSearch) return;
            CloseFieldMenu(); Send(new Search()); return;
        }
        if (_session.QueryPartyDetails() is not { } party || index != 0 && !party.Members.Any(member => _session.QueryFieldActions(member.Carried.Actor).Error is null)) return;
        _fieldMenu.HideMenu();
        _partyStatus.Begin(_session, _input, index == 1 ? "magic" : index == 2 ? "item" : "actions");
        Present();
    }
    private void PresentFieldMenu()
    {
        if (_fieldMenuActive && (_session!.Current.CanWaitAtInput != true || PresentationFailure is not null))
        { _fieldMenuActive = false; _fieldMenu.HideMenu(); _partyStatus.Hide(); _resumeAutomatic = true; }
        if (_fieldMenuActive && !_partyStatus.Open && !_fieldMenu.Open) _fieldMenu.Begin();
        var party = _session!.QueryPartyDetails();
        bool preparation = party?.Members.Any(member => _session.QueryFieldActions(member.Carried.Actor).Error is null) == true;
        _fieldMenu.Present(_input, _session.Definition.Exploration?.Visuals, CanOpenFieldMenu && !_fieldMenuActive,
            party is not null, preparation, _session.CanSearch, BeginFieldMenu, ChooseFieldMenu, CloseFieldMenu);
    }
}
