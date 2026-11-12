using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Exploration;

public sealed partial class ExplorationSessionView
{
    // Prepare both texture sets before preference publication; toggles retain borrowed handles.
    internal void PrepareGraphics() { if (_session is not null) _partyStatus.PrepareGraphics(_session); }
    internal void PublishGraphics() { if (_session is not null) _partyStatus.Present(_session, _input); }

    private FieldMenuView _fieldMenu = null!;
    private FieldGuidanceView _guidance = null!;
    private bool _fieldMenuActive;
    private bool CanOpenFieldMenu => _session?.Current.CanWaitAtInput == true && !_battleMounted && !_partyStatus.Open &&
        !_handedOff && !_battleHandoffPending && !MenuEntryPending && PresentationFailure is null;
    private void BeginFieldMenu()
    {
        if (!CanOpenFieldMenu) return;
        _fieldMenuActive = true; SuspendPacing(); _movementHold.Clear(_input); CancelFieldWait(); _returnFeedback.Dismiss();
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
        var guidance = _input.Settings.ModernGuidance ? _session!.QueryFieldGuidance() : null;
        var player = _session!.Current.Exploration?.PlayerEntity;
        _fieldMenu.ProtectSubject(player is null ? null : new Godot.Rect2(
            (new Godot.Vector2(player.Motion.X, player.Motion.Y) / 16 - WorldViewport.Origin) * 4, new(96, 96)).Grow(24));
        if (_fieldMenuActive && (_session!.Current.CanWaitAtInput != true || PresentationFailure is not null))
        { _fieldMenuActive = false; _fieldMenu.HideMenu(); _partyStatus.Hide(); _resumeAutomatic = true; }
        if (_fieldMenuActive && !_partyStatus.Open && !_fieldMenu.Open) _fieldMenu.Begin();
        var party = _session!.QueryPartyDetails();
        bool preparation = party?.Members.Any(member => _session.QueryFieldActions(member.Carried.Actor).Error is null) == true;
        _fieldMenu.Present(_input, _session.Definition.Exploration?.Visuals, CanOpenFieldMenu && !_fieldMenuActive,
            party is not null, preparation, _session.CanSearch, BeginFieldMenu, ChooseFieldMenu, CloseFieldMenu);
        _guidance.Present(_session, guidance, WorldViewport, _session.Current.Exploration,
            !_partyStatus.Open && !_battleMounted && !MenuEntryPending &&
            !_handedOff && !_battleHandoffPending && PresentationFailure is null && CanProcess(),
            _fieldMenu.OccupiedRect, !_fieldMenuActive);
    }
}
