using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.GodotAdapter.Entry;
using Sf2.Remake.GodotAdapter.Persistence;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class GameRoot
{
    private SaveSlotView? _slots;
    private Button? _saveButton;
    private Button? _quitButton;
    private Guid? _savedSession;
    private long _savedRevision = -1;
    private bool _restoring;

    public string ReadPersistenceObservationJson()
    {
        var offer = _session?.QuerySave();
        var capture = offer?.Allowed == true ? _session!.CaptureSave(offer.SessionId, offer.Revision) : null;
        var world = _session?.Current.Exploration;
        return System.Text.Json.JsonSerializer.Serialize(new { sessionId = _session?.Current.SessionId,
            revision = _session?.Current.Revision, names = _session?.Current.Story.Names, availability = offer, progress = capture?.Data, panel = _slots?.State,
            entityWork = world?.AllEntities.Where(entity => entity.Visible && entity.Actions is not null && !entity.IsScriptIdle)
                .Select(entity => new { entity.Entity, entity.ActionCursor, entity.Motion.IsMoving, entity.WaitingForMotion,
                    actions = entity.Actions!.Actions.Select(action => action.GetType().Name),
                    ambient = world.Definition.Entities.Any(row => row.Actions is { } actions &&
                        actions.Actions.SequenceEqual(entity.Actions!.Actions)) }) });
    }

    private OpeningRestoreReader RestoreSource() => new(Selected("SF2_PRIVATE_EXPLORATION_CONTENT"),
        Selected("SF2_PRIVATE_BATTLE01_DATA"), Selected("SF2_PRIVATE_BATTLE01_SCENE"), Selected("SF2_PRIVATE_BATTLE01_TERRAIN"),
        Selected("SF2_PRIVATE_STATIC_DATA"), Selected("SF2_PRIVATE_ENEMY_DATA"), Selected("SF2_PRIVATE_ENEMY_GOLD"),
        System.Environment.GetEnvironmentVariable("SF2_PRIVATE_BATTLE_SCENE_CONTENT"));

    private LocalSaveStore Store(ScenarioDefinition definition) => new(Selected("SF2_SAVE_ROOT"), Selected("SF2_SAVE_NAMESPACE"),
        System.IO.Path.GetFullPath(System.IO.Path.Combine(ProjectSettings.GlobalizePath("res://"), "..", "..")),
        data => GameSession.Restore(definition, data, RuleCompositions.ForGame()) is SessionStartFailed failed ? failed.Failure.Code : null);

    private void OpenSavePanel(bool overEntry = false)
    {
        if (_slots is not null) return;
        if (_entry is not null && !overEntry) _entry.Hide();
        _slots = new SaveSlotView { Name = "SaveSlotView", Cancel = CloseSavePanel, OverEntry = overEntry, Input = _input };
        if (overEntry) _entry?.Setup(true);
        AddChild(_slots); ApplyHostPause(); DisarmInput();
    }
    private void CloseSavePanel()
    {
        if (_slots is { } slots) { _slots = null; RemoveChild(slots); slots.QueueFree(); }
        if (_entry is not null) { _entry.Show(); _entry.ReturnFromSetup(); }
        ApplyHostPause();
        DisarmInput();
    }
    private void SaveError(string code)
    {
        _slots!.ShowChoices("error", Explain(code), ("Back", CloseSavePanel), ("Quit…", QuitFromMenu));
        _slots.ShowDetail("Details: " + code);
    }
    private void QuitFromMenu() { CloseSavePanel(); RequestQuit(); }
    private static string Explain(string code) => code switch
    {
        "save-location-unavailable" => "Save after returning to a supported location: opening Granseal or the settled first battle return.",
        "save-incompatible" or "save-version" => "This save needs the matching content/rules. The file has been kept.",
        "save-publication-uncertain" => "The write outcome is uncertain. Progress files have been kept. Reopen Continue to check them before trying again.",
        "save-storage-unavailable" => "The save folder could not be read or written. Check available disk space and folder access. Your existing saves have been kept.",
        "save-slot-empty" => "This slot is empty. Choose another slot or start a new journey.",
        "save-busy" or "save-player-unsettled" or "save-entity-unsettled" or "save-story-motion" =>
            "Finish the current action or conversation, then try Save again. Your existing saves have been kept.",
        "save-stale-revision" => "Your journey changed while this window was open. Return to the game and open Save again. Your existing saves have been kept.",
        "save-root-configuration" or "save-root-inside-worktree" or "save-root-inside-worktree-or-link" =>
            "The save folder is not configured safely. Check the launch settings. Your existing saves have been kept.",
        _ => "This save is unavailable. Your existing progress files have been kept. Check the details below before trying again.",
    };

    private void ShowSave()
    {
        if (_session is null || _slots is not null) return;
        if (_audio?.ObserveFieldHistory() is { } music)
        {
            var before = _session.Current;
            var result = _session.Submit(new(before.SessionId, before.Revision, null, music));
            if (result.Failure is { } failure) { OpenSavePanel(); SaveError(failure.Code); return; }
        }
        var offer = _session.QuerySave();
        OpenSavePanel();
        if (!offer.Allowed) { SaveError(offer.Error!); return; }
        try
        {
            var store = Store(_session.Definition);
            _slots!.ShowChoices("save-slots", $"Save current progress. Continue returns to {offer.Location!.Description}.\n" +
                "NPC positions and music restart; your party and earned progress are preserved.",
                (Label(1, store.Read(1), _session.Definition), () => ConfirmSave(1, store, offer)),
                (Label(2, store.Read(2), _session.Definition), () => ConfirmSave(2, store, offer)), ("Cancel", CloseSavePanel), ("Quit…", QuitFromMenu));
        }
        catch (ArgumentException error) { SaveError(error.Message); }
    }

    private static string Label(int slot, SlotRead read, ScenarioDefinition definition, bool forContinue = false)
    {
        if (read.Primary is { } record)
            return $"Slot {slot} · {CharacterNames.Member(definition, record.Data.Names, 0)}\n" +
                $"{SavedDestination(record.Data)} · {record.SavedAt.UtcDateTime:yyyy-MM-dd HH:mm} UTC";
        string status = read.Error switch
        {
            "save-incompatible" or "save-version" => "Needs matching content / rules",
            "save-storage-unavailable" => "Cannot read this slot",
            not null => "Save unavailable — existing file kept",
            _ => read.Backup is null ? "Empty" : "Primary missing",
        };
        return $"Slot {slot} · {status}" + (read.Backup is null ? "" : forContinue && CanRecover(read)
            ? "\nBackup available — choose to review recovery" : "\nBackup present");
    }

    private static bool CanRecover(SlotRead read) => read.Backup is not null &&
        read.Error is not ("save-incompatible" or "save-version" or "save-storage-unavailable" or "save-music-resource");

    // Display names for the existing finite recipes, not a new savepoint selection policy.
    private static string SavedDestination(OpeningSaveData data) => data.Recipe switch
    {
        "new-game" => "Granseal, beginning of the opening",
        "map3-opening" => "Granseal, home",
        "map3-joined" => "Granseal, outside the church",
        "battle01-return" => "Ancient Tower, completed battle return",
        _ => "Return location unavailable",
    };

    private void ConfirmSave(int slot, LocalSaveStore store, SaveAvailability offer)
    {
        var existing = store.Read(slot);
        _slots!.ShowChoices("confirm-save", existing.Primary is not null ? $"Overwrite Slot {slot}? The previous valid save becomes its backup."
            : $"Save current progress to Slot {slot}?", ("Cancel", CloseSavePanel), ("Save", () =>
            {
                var capture = _session!.CaptureSave(offer.SessionId, offer.Revision);
                if (capture.Data is null) { SaveError(capture.Error!); return; }
                var result = store.Write(slot, capture.Data);
                if (!result.Success) { SaveError(result.Error!); return; }
                _savedSession = offer.SessionId; _savedRevision = offer.Revision;
                _slots.ShowChoices("saved", $"Saved to Slot {slot}. Continue returns to {offer.Location!.Description}.", ("Return to game", CloseSavePanel));
            }));
    }

    private void ShowContinue()
    {
        OpenSavePanel(overEntry: _entry is not null);
        var read = RestoreSource().Read();
        if (read is not ExplorationReadAccepted admitted)
        { SaveError(read is ScenarioReadRejected rejected ? rejected.Failure.Code : "save-content-required"); return; }
        try
        {
            var store = Store(admitted.Definition);
            _slots!.ShowChoices("continue-slots", "Choose a saved journey.",
                (Label(1, store.Read(1), admitted.Definition, forContinue: true), () => ContinueSlot(1, store, admitted.Definition)),
                (Label(2, store.Read(2), admitted.Definition, forContinue: true), () => ContinueSlot(2, store, admitted.Definition)), ("Cancel", CloseSavePanel));
        }
        catch (ArgumentException error) { SaveError(error.Message); }
    }

    private void ContinueSlot(int slot, LocalSaveStore store, ScenarioDefinition definition)
    {
        if (!EnsureWindowSupport()) return;
        var read = store.Read(slot);
        if (read.Primary is { } primary) { RestoreRecord(primary, definition); return; }
        if (read.Backup is { } backup && CanRecover(read))
        {
            _slots!.ShowChoices("recover-backup", $"Slot {slot}'s main save is missing or unavailable.\n" +
                $"Recover {CharacterNames.Member(definition, backup.Data.Names, 0)}'s compatible backup?\n" +
                $"{SavedDestination(backup.Data)} · {backup.SavedAt.UtcDateTime:yyyy-MM-dd HH:mm} UTC",
                ("Cancel", CloseSavePanel), ("Recover backup", () =>
                {
                    if (!EnsureWindowSupport()) return;
                    var recovery = store.Recover(slot);
                    if (recovery.Record is null) SaveError(recovery.Error!);
                    else RestoreRecord(recovery.Record, definition);
                }));
            _slots.ShowDetail("Details: " + (read.Error ?? "primary missing"));
            return;
        }
        SaveError(read.Error ?? read.BackupError ?? "save-slot-empty");
    }

    private void RestoreRecord(OpeningSaveRecord record, ScenarioDefinition definition)
    {
        if (!EnsureWindowSupport()) return;
        var result = GameSession.Restore(definition, record.Data, RuleCompositions.ForGame());
        if (result is SessionStartFailed failed) { SaveError(failed.Failure.Code); return; }
        // The admitted initial recipe runs ordinary on-load and starts fresh-map playback.
        // Settled savepoints instead restore their saved music/history generation.
        _restoring = record.Data.Recipe != "new-game";
        try { MountSession((SessionStarted)result); }
        finally { _restoring = false; }
        if (_session?.Current.SessionId == ((SessionStarted)result).Session.Current.SessionId)
        { _savedSession = _session.Current.SessionId; _savedRevision = _session.Current.Revision; }
    }

    private void ConfigurePersistenceButtons()
    {
        _saveButton = HostButton("SaveEntry", "Save…", -540, -240, ShowSave);
        _saveButton.TooltipText = "Save: " + _input!.Hint(Input.GameAction.Stay);
        _quitButton = HostButton("QuitEntry", "Quit…", -760, -560, RequestQuit);
    }
    private Button HostButton(string name, string text, float left, float right, Action pressed)
    {
        var button = new Button { Name = name, Text = text, AnchorLeft = 1, AnchorRight = 1,
            OffsetLeft = left, OffsetRight = right, OffsetTop = 12, OffsetBottom = 60,
            Theme = UI.StoryTheme.CreateEntry(), FocusMode = Control.FocusModeEnum.None };
        button.Pressed += () => pressed(); AddChild(button); return button;
    }
    private void ReleasePersistenceButtons()
    {
        foreach (var button in new[] { _saveButton, _quitButton })
            if (button is not null) { RemoveChild(button); button.QueueFree(); }
        _saveButton = null; _quitButton = null;
    }

    public override void _Notification(int what)
    { SettingsNotification(what); }
    private void RequestQuit()
    {
        if (_session is null) { GetTree().Quit(); return; }
        if (_windowRecovery is not null) { ShowWindowQuit(); return; }
        if (_slots is not null) return;
        OpenSavePanel(); PresentQuitChoices(_slots!, CloseSavePanel);
    }
    private void PresentQuitChoices(SaveSlotView view, Action cancel)
    {
        bool dirty = _savedSession != _session!.Current.SessionId || _savedRevision != _session.Current.Revision;
        view.ShowChoices("confirm-quit", dirty ? "Quit? Progress since your last save will be lost. No automatic save is made." : "Quit to desktop? Your last save is safe.",
            ("Cancel", cancel), ("Quit", () => GetTree().Quit()));
    }
}
