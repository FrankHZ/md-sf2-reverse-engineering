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
            revision = _session?.Current.Revision, availability = offer, progress = capture?.Data, panel = _slots?.State,
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

    private void OpenSavePanel()
    {
        if (_slots is not null) return;
        if (_exploration is not null) _exploration.ProcessMode = ProcessModeEnum.Disabled;
        if (_battle is not null) _battle.ProcessMode = ProcessModeEnum.Disabled;
        if (_entry is not null) _entry.Hide();
        _slots = new SaveSlotView { Name = "SaveSlotView", Cancel = CloseSavePanel };
        AddChild(_slots); DisarmInput();
    }
    private void CloseSavePanel()
    {
        if (_slots is { } slots) { _slots = null; RemoveChild(slots); slots.QueueFree(); }
        if (_entry is not null) _entry.Show();
        else
        {
            if (_exploration is not null) _exploration.ProcessMode = ProcessModeEnum.Inherit;
            if (_battle is not null) _battle.ProcessMode = ProcessModeEnum.Inherit;
        }
        DisarmInput();
    }
    private void SaveError(string code) => _slots!.ShowChoices("error", Explain(code), ("Back", CloseSavePanel),
        ("Quit…", QuitFromMenu));
    private void QuitFromMenu() { CloseSavePanel(); RequestQuit(); }
    private static string Explain(string code) => code switch
    {
        "save-location-unavailable" => "Save after returning to a supported location: opening Granseal or the settled first battle return.",
        "save-incompatible" or "save-version" => "This save needs the matching content/rules. The file has been kept.",
        "save-publication-uncertain" => "The write outcome is uncertain. Progress files have been kept. Reopen Continue to check them before trying again.",
        "save-root-configuration" => "Saving needs an explicitly configured persistent save folder and checkout namespace. No default folder is used.",
        _ => "Save / Continue unavailable: " + code + ". Your existing progress files have been kept.",
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
                (Label(1, store.Read(1)), () => ConfirmSave(1, store, offer)),
                (Label(2, store.Read(2)), () => ConfirmSave(2, store, offer)), ("Cancel", CloseSavePanel), ("Quit…", QuitFromMenu));
        }
        catch (ArgumentException error) { SaveError(error.Message); }
    }

    private static string Label(int slot, SlotRead read) => $"Slot {slot}: " + (read.Primary is { } record
        ? $"{record.SavedAt.UtcDateTime:yyyy-MM-dd HH:mm} UTC — {record.Data.Recipe}"
        : read.Error is not null ? $"{read.Error}" : read.Backup is not null ? "Primary missing — backup available" : "Empty");

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
        OpenSavePanel();
        var read = RestoreSource().Read();
        if (read is not ExplorationReadAccepted admitted)
        { SaveError(read is ScenarioReadRejected rejected ? rejected.Failure.Code : "save-content-required"); return; }
        try
        {
            var store = Store(admitted.Definition);
            _slots!.ShowChoices("continue-slots", "Choose a saved journey.",
                (Label(1, store.Read(1)), () => ContinueSlot(1, store, admitted.Definition)),
                (Label(2, store.Read(2)), () => ContinueSlot(2, store, admitted.Definition)), ("Cancel", CloseSavePanel));
        }
        catch (ArgumentException error) { SaveError(error.Message); }
    }

    private void ContinueSlot(int slot, LocalSaveStore store, ScenarioDefinition definition)
    {
        var read = store.Read(slot);
        if (read.Primary is { } primary) { RestoreRecord(primary, definition); return; }
        if (read.Backup is { } backup && read.Error is not ("save-incompatible" or "save-version" or "save-storage-unavailable" or "save-music-resource"))
        {
            _slots!.ShowChoices("recover-backup", $"Slot {slot}: {read.Error ?? "primary missing"}.\nRecover compatible backup from {backup.SavedAt.UtcDateTime:yyyy-MM-dd HH:mm} UTC?",
                ("Cancel", CloseSavePanel), ("Recover backup", () =>
                {
                    var recovery = store.Recover(slot);
                    if (recovery.Record is null) SaveError(recovery.Error!);
                    else RestoreRecord(recovery.Record, definition);
                }));
            return;
        }
        SaveError(read.Error ?? read.BackupError ?? "save-slot-empty");
    }

    private void RestoreRecord(OpeningSaveRecord record, ScenarioDefinition definition)
    {
        var result = GameSession.Restore(definition, record.Data, RuleCompositions.ForGame());
        if (result is SessionStartFailed failed) { SaveError(failed.Failure.Code); return; }
        _restoring = true;
        try { MountSession((SessionStarted)result); }
        finally { _restoring = false; }
        if (_session?.Current.SessionId == ((SessionStarted)result).Session.Current.SessionId)
        { _savedSession = _session.Current.SessionId; _savedRevision = _session.Current.Revision; }
    }

    private void ConfigurePersistenceButtons()
    {
        _saveButton = HostButton("SaveEntry", "Save… " + _input!.Hint(Input.GameAction.Stay), -540, -240, ShowSave);
        _quitButton = HostButton("QuitEntry", "Quit…", -760, -560, RequestQuit);
    }
    private Button HostButton(string name, string text, float left, float right, Action pressed)
    {
        var button = new Button { Name = name, Text = text, AnchorLeft = 1, AnchorRight = 1,
            OffsetLeft = left, OffsetRight = right, OffsetTop = 12, OffsetBottom = 60,
            Theme = UI.StoryTheme.Create(), FocusMode = Control.FocusModeEnum.None };
        button.Pressed += () => pressed(); AddChild(button); return button;
    }
    private void ReleasePersistenceButtons()
    {
        foreach (var button in new[] { _saveButton, _quitButton })
            if (button is not null) { RemoveChild(button); button.QueueFree(); }
        _saveButton = null; _quitButton = null;
    }

    public override void _Notification(int what)
    { if (what == NotificationWMCloseRequest) RequestQuit(); }
    private void RequestQuit()
    {
        if (_session is null) { GetTree().Quit(); return; }
        if (_slots is not null) return;
        OpenSavePanel();
        bool dirty = _savedSession != _session.Current.SessionId || _savedRevision != _session.Current.Revision;
        _slots!.ShowChoices("confirm-quit", dirty ? "Quit? Progress since your last save will be lost. No automatic save is made." : "Quit to desktop? Your last save is safe.",
            ("Cancel", CloseSavePanel), ("Quit", () => GetTree().Quit()));
    }
}
