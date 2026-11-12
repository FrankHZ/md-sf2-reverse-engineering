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
    private bool _saveNeedsInspection;
    private bool _battleSaveCaller;
    private int _saveSelection;
    private sealed record HostSaveOffer(Guid SessionId, long Revision, string Destination, string? Error);

    private HostSaveOffer SaveOffer(bool previewMusic = false)
    {
        if (_session!.Current.Active is ActiveBattle)
        {
            var offer = _session.QueryBattleSave(previewMusic ? _audio?.ObserveBattleHistory() : null);
            return new(offer.SessionId, offer.Revision, offer.Location is { } location
                ? BattleDestination(_session.Definition, _session.Current.Story.Names, location.Encounter, location.Round, location.Actor) : "", offer.Error);
        }
        var field = _session.QuerySave();
        return new(field.SessionId, field.Revision, field.Location is { } target ? SavedDestination(target.Recipe) : "", field.Error);
    }

    private string? ObserveSaveMusic()
    {
        var before = _session!.Current;
        SessionCommand? music = before.Active is ActiveBattle ? _audio?.ObserveBattleHistory() : _audio?.ObserveFieldHistory();
        if (music is null) return null;
        return _session.Submit(new(before.SessionId, before.Revision, null, music)).Failure?.Code;
    }

    public string ReadPersistenceObservationJson()
    {
        var offer = _session is null ? null : SaveOffer();
        object? progress = offer is not { Error: null } ? null : _session!.Current.Active is ActiveBattle
            ? _session.CaptureBattleSave(offer.SessionId, offer.Revision).Data : _session.CaptureSave(offer.SessionId, offer.Revision).Data;
        var world = _session?.Current.Exploration;
        object? availability = _session is null ? null : _session.Current.Active is ActiveBattle
            ? _session.QueryBattleSave() : _session.QuerySave();
        return System.Text.Json.JsonSerializer.Serialize(new { sessionId = _session?.Current.SessionId,
            revision = _session?.Current.Revision, names = _session?.Current.Story.Names, availability, progress, panel = _slots?.State,
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
        record => SaveRecordRestore.Restore(definition, record, RuleCompositions.ForGame()) is SessionStartFailed failed ? failed.Failure.Code : null);

    private void OpenSavePanel(bool overEntry = false)
    {
        if (_slots is not null) return;
        _saveSelection = 0;
        if (_entry is not null && !overEntry) _entry.Hide();
        _slots = new SaveSlotView { Name = "SaveSlotView", Cancel = CloseSavePanel, OverEntry = overEntry, Input = _input };
        if (overEntry) _entry?.Setup(true);
        _hostUi.AddChild(_slots); CoverSystemMenu(); ApplyHostPause(); DisarmInput();
    }
    private void CloseSavePanel()
    {
        if (_entryLoading) return;
        if (_slots is { } slots) { _slots = null; _hostUi.RemoveChild(slots); slots.QueueFree(); }
        if (_session is not null) GameText.Bind(_session.Definition, System.Environment.GetEnvironmentVariable("SF2_DISPLAY_CATALOG"));
        if (_entry is not null) { _entry.Show(); _entry.ReturnFromSetup(); }
        if (_battleSaveCaller) { _battleSaveCaller = false; _battle?.ReturnFromBattleSave(); }
        ReturnToSystemMenu();
        ApplyHostPause();
        DisarmInput();
    }
    private void SaveError(string code)
    {
        if (code == "save-publication-uncertain") _saveNeedsInspection = true;
        _slots!.Cancel = CloseSavePanel;
        _slots.ShowChoices(_saveNeedsInspection ? "save-uncertain" : "error", Explain(code),
            _saveNeedsInspection ? [(GameText.Get("entry.inspect"), ShowContinue),
                (GameText.Get("entry.back"), CloseSavePanel)] : [(GameText.Get("entry.back"), CloseSavePanel)]);
        _slots.ShowDetail(GameText.Get("save.details", code));
    }
    private void QuitFromMenu() { CloseSavePanel(); RequestQuit(); }
    private static string Explain(string code) => code switch
    {
        "save-location-unavailable" => GameText.Get("save.error.location"),
        "save-profile-unavailable" or "save-battle-profile-unavailable" or "save-battle-route-unavailable" => GameText.Get("save.error.profile"),
        "save-incompatible" or "save-version" or "save-kind" or "save-battle-incompatible" or "save-battle-recipe-version" => GameText.Get("save.error.compatibility"),
        "save-publication-uncertain" => GameText.Get("save.error.uncertain"),
        "save-storage-unavailable" => GameText.Get("save.error.storage"),
        "save-slot-empty" => GameText.Get("save.error.empty"),
        "save-busy" or "save-player-unsettled" or "save-entity-unsettled" or "save-story-motion" =>
            GameText.Get("save.error.busy"),
        "save-stale-revision" => GameText.Get("save.error.stale"),
        "save-battle-cancel-movement" => GameText.Get("save.error.battle-preview"),
        "save-battle-selection-busy" => GameText.Get("save.error.battle-selection"),
        "save-battle-movement-busy" => GameText.Get("save.error.battle-movement"),
        "save-battle-busy" or "save-battle-story-busy" or "save-battle-actor-unavailable" => GameText.Get("save.error.battle-busy"),
        "save-battle-music-unknown" or "save-battle-music-observation" or "save-battle-music-disagreement" => GameText.Get("save.error.battle-music"),
        var battle when battle.StartsWith("save-battle-", StringComparison.Ordinal) => GameText.Get("save.error.battle-context"),
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

    private void ShowBattleQuit()
    {
        if (_session is null || _entry is not null || _systemMenu is not null || _settings is not null ||
            _slots is not null || _nameEntry is not null || _windowRecovery is not null || _windowQuit is not null ||
            _battle?.CanOpenBattleQuit != true) return;
        _battleSaveCaller = true;
        ShowSettledSave();
    }

    private async void ShowSettledSave()
    {
        if (_session is null) return;
        // Freeze dispatch before music publication and the revision-bound query.
        OpenSavePanel();
        _slots!.Cancel = CloseSavePanel;
        if (_saveNeedsInspection) { SaveError("save-publication-uncertain"); return; }
        if (_fieldEntryFailure is { } entryError) { SaveError(entryError); return; }
        if (ObserveSaveMusic() is { } musicError) { SaveError(musicError); return; }
        try
        {
            var offer = SaveOffer();
            if (offer.Error is { } error) { SaveError(error); return; }
            var store = Store(_session.Definition);
            var slots = await LoadEntry(() => (First: store.Read(1), Second: store.Read(2)));
            _slots!.ShowChoices("save-slots", GameText.Get(_session.Current.Active is ActiveBattle ? "save.battle-offer" : "save.offer", offer.Destination),
                (Label(1, slots.First, _session.Definition), () => ConfirmSave(1, store, offer)),
                (Label(2, slots.Second, _session.Definition), () => ConfirmSave(2, store, offer)), (GameText.Get("entry.cancel"), CloseSavePanel));
            _slots.Select(_saveSelection);
        }
        catch (ArgumentException error) { SaveError(error.Message); }
        catch (OperationCanceledException) { }
    }

    private static string Label(int slot, SlotRead read, ScenarioDefinition definition, bool forContinue = false)
    {
        if (read.Primary is OpeningSaveRecord record)
            return GameText.Get("slot.record", slot, DisplayNames.Member(definition, record.Data.Names, 0),
                SavedDestination(record.Data.Recipe), SavedTime(record));
        if (read.Primary is BattleSaveRecord battle)
            return GameText.Get("slot.record", slot, DisplayNames.Member(definition, battle.Data.Story.Names, 0),
                BattleDestination(definition, battle.Data.Story.Names, battle.Data.Battle.Encounter, battle.Data.Battle.Round,
                    battle.Data.Battle.Queue[battle.Data.Battle.Cursor].Actor!), SavedTime(battle));
        string status = read.Error switch
        {
            "save-incompatible" or "save-version" or "save-kind" => GameText.Get("slot.matching"),
            "save-storage-unavailable" => GameText.Get("slot.unreadable"),
            not null => GameText.Get("slot.unavailable"),
            _ => read.Backup is null ? GameText.Get("slot.empty") : GameText.Get("slot.primary-missing"),
        };
        return GameText.Get("slot.status", slot, status, read.Backup is null ? "" : forContinue && CanRecover(read)
            ? GameText.Get("slot.backup-review") : GameText.Get("slot.backup"));
    }

    private static bool CanRecover(SlotRead read) => read.CanRecover;

    // Display names for the existing finite recipes, not a new savepoint selection policy.
    private static string SavedDestination(string recipe) => recipe switch
    {
        "new-game" => GameText.Get("save.destination.new-game"),
        "map3-opening" => GameText.Get("save.destination.map3-opening"),
        "map3-joined" => GameText.Get("save.destination.map3-joined"),
        "battle01-return" => GameText.Get("save.destination.battle01-return"),
        "new-granseal-castle" => GameText.Get("save.destination.new-granseal-castle"),
        "ribble-welcome" => GameText.Get("save.destination.ribble-welcome"),
        "dark-caves" => GameText.Get("save.destination.dark-caves"),
        "mount-volcano" => GameText.Get("save.destination.mount-volcano"),
        "polca-return" => GameText.Get("save.destination.polca-return"),
        "east-shrine" => GameText.Get("save.destination.east-shrine"),
        "parmecia-battle-return" => GameText.Get("save.destination.parmecia-battle-return"),
        "battle07-return" => GameText.Get("save.destination.battle07-return"),
        _ => GameText.Get("save.destination.unavailable"),
    };

    private static string BattleDestination(ScenarioDefinition definition, IReadOnlyDictionary<string, string> names,
        string encounter, int round, string actor)
    {
        var deployment = definition.Encounters[encounter].Deployments.Single(row => row.Actor.Value == actor);
        string name = deployment.Initialization?.AllyPartyMember is { } member
            ? DisplayNames.Member(definition, names, member) : names.GetValueOrDefault(actor) ?? deployment.Definition.Actor.Value;
        return GameText.Get("save.battle-location", encounter.StartsWith("battle-", StringComparison.Ordinal) ? encounter[7..] : encounter,
            round, name);
    }

    private void ConfirmSave(int slot, LocalSaveStore store, HostSaveOffer offer)
    {
        _saveSelection = slot - 1;
        var existing = store.Read(slot);
        _slots!.Cancel = BackToSave;
        _slots.ShowChoices("confirm-save", existing.Primary is not null ? GameText.Get("save.overwrite", slot)
            : GameText.Get("save.confirm", slot), (GameText.Get("entry.cancel"), BackToSave),
            (GameText.Get("save.and-return"), () => PublishSave(false)), (GameText.Get("save.and-quit"), () => PublishSave(true)));

        void BackToSave() { ShowSettledSave(); }
        async void PublishSave(bool quit)
        {
            if (_entryLoading) return;
            try
            {
                var session = _session!;
                var result = await LoadEntry(() =>
                {
                    if (session.Current.Active is ActiveBattle)
                    {
                        var capture = session.CaptureBattleSave(offer.SessionId, offer.Revision);
                        return capture.Data is { } data ? store.Write(slot, data) : new SlotWrite(false, capture.Error);
                    }
                    var field = session.CaptureSave(offer.SessionId, offer.Revision);
                    return field.Data is { } progress ? store.Write(slot, progress) : new SlotWrite(false, field.Error);
                }, publishing: true, publicationObserved: written =>
                { if (written.Error == "save-publication-uncertain") _saveNeedsInspection = true; });
                if (!result.Success) { SaveError(result.Error!); return; }
                _savedSession = offer.SessionId; _savedRevision = offer.Revision;
                if (quit) { GetTree().Quit(); return; }
                void ReturnToPlay() { CloseSavePanel(); CloseSystemMenu(); }
                _slots!.Cancel = ReturnToPlay;
                _slots.ShowChoices("saved", GameText.Get("save.saved", slot, offer.Destination), (GameText.Get("entry.return"), ReturnToPlay));
                DisarmInput();
            }
            catch (OperationCanceledException) { }
            catch (ArgumentException error) { SaveError(error.Message); }
        }
    }

    private async void ShowContinue()
    {
        if (_entryLoading) return;
        OpenSavePanel(overEntry: _entry is not null);
        try
        {
            var source = RestoreSource();
            var read = await LoadEntry(source.Read);
            if (read is not ExplorationReadAccepted admitted)
            {
                if (_session is null) GameText.ClearWorld();
                SaveError(read is ScenarioReadRejected rejected ? rejected.Failure.Code : "save-content-required"); return;
            }
            GameText.Bind(admitted.Definition, System.Environment.GetEnvironmentVariable("SF2_DISPLAY_CATALOG"));
            var store = Store(admitted.Definition);
            var slots = await LoadEntry(() => (First: store.Read(1), Second: store.Read(2)));
            _saveNeedsInspection = false;
            _slots!.ShowChoices("continue-slots", GameText.Get("save.choose"),
                (Label(1, slots.First, admitted.Definition, forContinue: true), () => ContinueSlot(1, store, admitted.Definition)),
                (Label(2, slots.Second, admitted.Definition, forContinue: true), () => ContinueSlot(2, store, admitted.Definition)), (GameText.Get("entry.cancel"), CloseSavePanel));
        }
        catch (ArgumentException error) { SaveError(error.Message); }
        catch (OperationCanceledException) { }
    }

    private async void ContinueSlot(int slot, LocalSaveStore store, ScenarioDefinition definition)
    {
        try
        {
            if (_entryLoading || !EnsureWindowSupport()) return;
            var read = await LoadEntry(() => store.Read(slot));
            if (read.Primary is { } primary) { RestoreRecord(primary, definition); return; }
            if (read.Backup is { } backup && CanRecover(read))
            {
                _slots!.ShowChoices("recover-backup", GameText.Get("save.recovery-record", Label(slot,
                    new(backup, null, null, null), definition)),
                    (GameText.Get("entry.cancel"), CloseSavePanel), (GameText.Get("entry.recover"), async () =>
                    {
                        if (_entryLoading || !EnsureWindowSupport()) return;
                        try
                        {
                            var recovery = await LoadEntry(() => store.Recover(slot), publishing: true,
                                publicationObserved: written =>
                                { if (written.Error == "save-publication-uncertain") _saveNeedsInspection = true; });
                            if (recovery.Record is null) SaveError(recovery.Error!);
                            else RestoreRecord(recovery.Record, definition);
                        }
                        catch (OperationCanceledException) { }
                        catch (ArgumentException error) { SaveError(error.Message); }
                    }));
                _slots.ShowDetail(GameText.Get("save.details", read.Error ?? GameText.Get("slot.primary-missing")));
                return;
            }
            SaveError(read.Error ?? read.BackupError ?? "save-slot-empty");
        }
        catch (OperationCanceledException) { }
        catch (ArgumentException error) { SaveError(error.Message); }
    }

    private async void RestoreRecord(SaveRecord saved, ScenarioDefinition definition)
    {
        try
        {
            if (_entryLoading || !EnsureWindowSupport()) return;
            var result = await LoadEntry(() => SaveRecordRestore.Restore(definition, saved, RuleCompositions.ForGame()));
            if (result is SessionStartFailed failed) { SaveError(failed.Failure.Code); return; }
            // The admitted initial recipe runs ordinary on-load and starts fresh-map playback.
            // Settled savepoints instead restore their saved music/history generation.
            _restoring = saved is not OpeningSaveRecord { Data.Recipe: "new-game" };
            try { MountSession((SessionStarted)result); }
            finally { _restoring = false; }
            if (_session?.Current.SessionId == ((SessionStarted)result).Session.Current.SessionId)
            { _savedSession = _session.Current.SessionId; _savedRevision = _session.Current.Revision; }
        }
        catch (OperationCanceledException) { }
        catch (ArgumentException error) { SaveError(error.Message); }
    }

    private static string SavedTime(SaveRecord record) => record.SavedAt.UtcDateTime.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);

    public override void _Notification(int what)
    { SettingsNotification(what); }
    private void RequestQuit()
    {
        if (_entryLoading)
        {
            _entryQuitRequested = true;
            _slots?.ShowDetail(GameText.Get("entry.quit-pending"));
            return;
        }
        _exploration?.CancelMenuEntry("save-entry-quit");
        if (_saveNeedsInspection)
        {
            if (_session is null && _entry is null) ShowEntry();
            OpenSavePanel(overEntry: _entry is not null);
            SaveError("save-publication-uncertain");
            return;
        }
        if (_session is null) { GetTree().Quit(); return; }
        if (_windowRecovery is not null) { ShowWindowQuit(); return; }
        if (_slots is not null) return;
        OpenSavePanel(); PresentQuitChoices(_slots!, CloseSavePanel);
    }
    private void PresentQuitChoices(SaveSlotView view, Action cancel)
    {
        if (_saveNeedsInspection)
        {
            view.ShowChoices("save-uncertain", Explain("save-publication-uncertain"),
                (GameText.Get("entry.inspect"), () => { cancel(); CloseSavePanel(); ShowContinue(); }),
                (GameText.Get("entry.cancel"), cancel));
            return;
        }
        bool dirty = _savedSession != _session!.Current.SessionId || _savedRevision != _session.Current.Revision;
        view.ShowChoices("confirm-quit", dirty ? GameText.Get("save.quit-dirty") : GameText.Get("save.quit-clean"),
            (GameText.Get("entry.cancel"), cancel), (GameText.Get("entry.quit"), () => GetTree().Quit()));
    }
}
