using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter;

internal enum PreviewEntry { Party, Shop, FieldMenu, Inspection, Outcome, System }

public sealed partial class GameRoot
{
    private (IScenarioSource? Source, InputSettings Settings, Action ToTitle)? _preview;

    // The preview owns the viewport and a fresh, explicit output file. Normal entry and
    // persistence keep their existing owners and never consult the player's save namespace.
    internal void ConfigurePreview(IScenarioSource? source, InputSettings settings, string saveOutput, Action toTitle)
    {
        _preview = (source, settings, toTitle);
        _outputFile = saveOutput;
    }

    internal InputSettings? PreviewSettings => _input?.Settings;
    internal bool PreviewHasSession => _session is not null;
    internal bool PreviewSystemOpen => _systemMenu is not null;
    internal bool PreviewLoading => _entryLoading || _committing;
    internal bool PreviewNameEntryOpen => _nameEntry is not null;
    internal bool PreviewFieldAvailable => _session?.Current.Active is ActiveExploration &&
        _exploration?.IsVisibleInTree() == true && _systemMenu is null && _slots is null && _entry is null;

    internal void SetPreviewAppearance(string language, bool reducedFlash)
    {
        if (_input is null || _settings is not null || PreviewLoading) return;
        var candidate = _input.Settings.Draft();
        candidate.Language = language;
        candidate.ReducedFlash = reducedFlash;
        if (ApplySettings(candidate, false) is null) _battle?.RefreshSettingsProjection();
    }

    internal bool OpenPreviewSystemMenu()
    {
        if (_systemMenu is not null)
        {
            if (_systemMenu.Visible) CloseSystemMenu();
            return true;
        }
        ShowSystemMenu();
        return _systemMenu is not null || _exploration?.MenuEntryPending == true;
    }

    // Each entry uses the real menu owner or an ordinary interaction once the caller is
    // usable. Selecting samples never edits a live selection, party or battle snapshot.
    internal bool AdvancePreviewEntry(PreviewEntry entry)
    {
        if (_input is null || _session is null) return _entry is not null;
        if (_input.GameplayHeld || !GetWindow().HasFocus()) return false;
        if (entry == PreviewEntry.Inspection)
            return _battle is not null && (_battle.StartupError is not null || _battle.EnterInspection());
        if (entry == PreviewEntry.System) return OpenPreviewSystemMenu();
        if (_entry is not null || _settings is not null || _slots is not null || _systemMenu is not null) return true;
        if (_exploration is not { } field || !field.IsVisibleInTree()) return true;
        if (_session.Current.Story.Wait is ShopWait)
        {
            if (entry == PreviewEntry.Shop) return true;
            field.HandleAction(GameAction.Cancel);
            return false;
        }
        if (entry is PreviewEntry.Party or PreviewEntry.FieldMenu)
            return field.OpenPreviewFieldMenu(entry == PreviewEntry.Party);
        if (field.PartyMenuOpen)
        {
            field.HandleAction(GameAction.Cancel);
            return false;
        }
        if (!field.CanOpenHostMenu) return false;
        if (entry != PreviewEntry.Shop || _session.QueryExplorationInteraction()?.Value == "merchant")
            field.HandleAction(GameAction.Confirm);
        return true;
    }

    private void QuitGame()
    {
        if (_preview is { } preview) preview.ToTitle();
        else GetTree().Quit();
    }
}
