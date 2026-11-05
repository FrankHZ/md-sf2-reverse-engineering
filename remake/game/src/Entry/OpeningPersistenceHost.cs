using Godot;
using Sf2.Remake.GodotAdapter.Localization;
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
        _hostUi.AddChild(_slots); CoverSystemMenu(); ApplyHostPause(); DisarmInput();
    }
    private void CloseSavePanel()
    {
        if (_slots is { } slots) { _slots = null; _hostUi.RemoveChild(slots); slots.QueueFree(); }
        if (_entry is not null) { _entry.Show(); _entry.ReturnFromSetup(); }
        ReturnToSystemMenu();
        ApplyHostPause();
        DisarmInput();
    }
    private void SaveError(string code)
    {
        _slots!.Cancel = CloseSavePanel;
        _slots.ShowChoices("error", Explain(code), (GameText.Get("entry.back"), CloseSavePanel), (GameText.Get("entry.quit-menu"), QuitFromMenu));
        _slots.ShowDetail(GameText.Get("save.details", code));
    }
    private void QuitFromMenu() { CloseSavePanel(); RequestQuit(); }
    private static string Explain(string code) => code switch
    {
        "save-location-unavailable" => GameText.Get("save.error.location"),
        "save-profile-unavailable" => GameText.Get("save.error.profile"),
        "save-incompatible" or "save-version" => GameText.Get("save.error.compatibility"),
        "save-publication-uncertain" => GameText.Get("save.error.uncertain"),
        "save-storage-unavailable" => GameText.Get("save.error.storage"),
        "save-slot-empty" => GameText.Get("save.error.empty"),
        "save-busy" or "save-player-unsettled" or "save-entity-unsettled" or "save-story-motion" =>
            GameText.Get("save.error.busy"),
        "save-stale-revision" => GameText.Get("save.error.stale"),
        "save-entry-budget" => GameText.Get("save.error.entry-limit"),
        "save-entry-cancelled" or "save-entry-paused" or "save-entry-quit" => GameText.Get("save.error.entry-cancelled"),
        "save-entry-transition" => GameText.Get("save.error.entry-transition"),
        "save-root-configuration" or "save-root-inside-worktree" or "save-root-inside-worktree-or-link" =>
            GameText.Get("save.error.config"),
        _ => GameText.Get("save.error.other"),
    };

    private void ShowSave()
    {
        if (_session is null || _slots is not null) return;
        if (_systemMenu is not null) ShowSettledSave();
        else RequestFieldEntry(ShowSettledSave);
    }

    private void ShowSettledSave()
    {
        if (_session is null) return;
        // Freeze dispatch before music publication and the revision-bound query.
        OpenSavePanel();
        if (_fieldEntryFailure is { } entryError) { SaveError(entryError); return; }
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
            _slots!.ShowChoices("save-slots", GameText.Get("save.offer", SavedDestination(offer.Location!.Recipe)),
                (Label(1, store.Read(1), _session.Definition), () => ConfirmSave(1, store, offer)),
                (Label(2, store.Read(2), _session.Definition), () => ConfirmSave(2, store, offer)), (GameText.Get("entry.cancel"), CloseSavePanel), (GameText.Get("entry.quit-menu"), QuitFromMenu));
        }
        catch (ArgumentException error) { SaveError(error.Message); }
    }

    private static string Label(int slot, SlotRead read, ScenarioDefinition definition, bool forContinue = false)
    {
        if (read.Primary is { } record)
            return GameText.Get("slot.record", slot, CharacterNames.Member(definition, record.Data.Names, 0),
                SavedDestination(record.Data.Recipe), SavedTime(record));
        string status = read.Error switch
        {
            "save-incompatible" or "save-version" => GameText.Get("slot.matching"),
            "save-storage-unavailable" => GameText.Get("slot.unreadable"),
            not null => GameText.Get("slot.unavailable"),
            _ => read.Backup is null ? GameText.Get("slot.empty") : GameText.Get("slot.primary-missing"),
        };
        return GameText.Get("slot.status", slot, status, read.Backup is null ? "" : forContinue && CanRecover(read)
            ? GameText.Get("slot.backup-review") : GameText.Get("slot.backup"));
    }

    private static bool CanRecover(SlotRead read) => read.Backup is not null &&
        read.Error is not ("save-incompatible" or "save-version" or "save-storage-unavailable" or "save-music-resource");

    // Display names for the existing finite recipes, not a new savepoint selection policy.
    private static string SavedDestination(string recipe) => recipe switch
    {
        "new-game" => GameText.Get("save.destination.new-game"),
        "map3-opening" => GameText.Get("save.destination.map3-opening"),
        "map3-joined" => GameText.Get("save.destination.map3-joined"),
        "battle01-return" => GameText.Get("save.destination.battle01-return"),
        "battle07-return" => GameText.Get("save.destination.battle07-return"),
        _ => GameText.Get("save.destination.unavailable"),
    };

    private void ConfirmSave(int slot, LocalSaveStore store, SaveAvailability offer)
    {
        var existing = store.Read(slot);
        _slots!.Cancel = BackToSave;
        _slots.ShowChoices("confirm-save", existing.Primary is not null ? GameText.Get("save.overwrite", slot)
            : GameText.Get("save.confirm", slot), (GameText.Get("entry.cancel"), BackToSave), (GameText.Get("entry.save"), () =>
            {
                var capture = _session!.CaptureSave(offer.SessionId, offer.Revision);
                if (capture.Data is null) { SaveError(capture.Error!); return; }
                var result = store.Write(slot, capture.Data);
                if (!result.Success) { SaveError(result.Error!); return; }
                _savedSession = offer.SessionId; _savedRevision = offer.Revision;
                _slots.Cancel = CloseSavePanel;
                _slots.ShowChoices("saved", GameText.Get("save.saved", slot, SavedDestination(offer.Location!.Recipe)), (GameText.Get("entry.return"), CloseSavePanel));
            }));

        void BackToSave() { CloseSavePanel(); ShowSave(); }
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
            _slots!.ShowChoices("continue-slots", GameText.Get("save.choose"),
                (Label(1, store.Read(1), admitted.Definition, forContinue: true), () => ContinueSlot(1, store, admitted.Definition)),
                (Label(2, store.Read(2), admitted.Definition, forContinue: true), () => ContinueSlot(2, store, admitted.Definition)), (GameText.Get("entry.cancel"), CloseSavePanel));
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
            _slots!.ShowChoices("recover-backup", GameText.Get("save.recovery", slot, CharacterNames.Member(definition, backup.Data.Names, 0),
                SavedDestination(backup.Data.Recipe), SavedTime(backup)),
                (GameText.Get("entry.cancel"), CloseSavePanel), (GameText.Get("entry.recover"), () =>
                {
                    if (!EnsureWindowSupport()) return;
                    var recovery = store.Recover(slot);
                    if (recovery.Record is null) SaveError(recovery.Error!);
                    else RestoreRecord(recovery.Record, definition);
                }));
            _slots.ShowDetail(GameText.Get("save.details", read.Error ?? GameText.Get("slot.primary-missing")));
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

    private static string SavedTime(OpeningSaveRecord record) => record.SavedAt.UtcDateTime.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);

    public override void _Notification(int what)
    { SettingsNotification(what); }
    private void RequestQuit()
    {
        _exploration?.CancelMenuEntry("save-entry-quit");
        if (_session is null) { GetTree().Quit(); return; }
        if (_windowRecovery is not null) { ShowWindowQuit(); return; }
        if (_slots is not null) return;
        OpenSavePanel(); PresentQuitChoices(_slots!, CloseSavePanel);
    }
    private void PresentQuitChoices(SaveSlotView view, Action cancel)
    {
        bool dirty = _savedSession != _session!.Current.SessionId || _savedRevision != _session.Current.Revision;
        view.ShowChoices("confirm-quit", dirty ? GameText.Get("save.quit-dirty") : GameText.Get("save.quit-clean"),
            (GameText.Get("entry.cancel"), cancel), (GameText.Get("entry.quit"), () => GetTree().Quit()));
    }
}
