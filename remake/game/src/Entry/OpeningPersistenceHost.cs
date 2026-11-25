using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.GodotAdapter.Audio;
using Sf2.Remake.GodotAdapter.Entry;
using Sf2.Remake.GodotAdapter.Persistence;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class GameRoot
{
    private SaveSlotView? _slots;
    private Guid? _savedSession;
    private long _savedRevision = -1;
    private bool _saveNeedsInspection;
    private bool _battleSaveCaller;
    private int _saveSelection;
    private int _historyPage;
    private string? _seedFile, _outputFile;
    private bool SeedOnly => _seedFile is not null && _outputFile is null;
    private sealed record HostSaveOffer(Guid SessionId, long Revision, string Destination, string? Error);

    private HostSaveOffer SaveOffer(bool previewMusic = false, bool church = false)
    {
        if (_session!.Current.Active is ActiveBattle)
        {
            var offer = _session.QueryBattleSave(previewMusic ? _audio?.ObserveBattleHistory() : null);
            return new(offer.SessionId, offer.Revision, offer.Location is { } location
                ? BattleDestination(_session.Definition, _session.Current.Story.Names, location.Encounter, location.Round, location.Actor) : "", offer.Error);
        }
        var field = FieldOffer(church);
        return new(field.SessionId, field.Revision, field.Landing is { } landing ? FieldDestination(landing) : "", field.Error);
    }

    private FieldSnapshotOffer FieldOffer(bool church)
    {
        var offer = _session!.QueryFieldSnapshot(church);
        return offer.Allowed && !SessionAudio.CanRestoreFieldMusic(_session.Current.Story.Music)
            ? offer with { Error = "restore-music-phase" } : offer;
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
            ? _session.CaptureBattleSave(offer.SessionId, offer.Revision).Data : _session.CaptureFieldSnapshot(offer.SessionId, offer.Revision).Data;
        var world = _session?.Current.Exploration;
        object? availability = _session is null ? null : _session.Current.Active is ActiveBattle
            ? _session.QueryBattleSave() : FieldOffer(church: false);
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

    private void ConfigureSaveFiles(Dictionary<string, string> options)
    {
        if (!options.ContainsKey("--load-save") && !options.ContainsKey("--save-output")) return;
        string? FileOption(string key)
        {
            if (!options.TryGetValue(key, out var raw)) return null;
            string path = System.IO.Path.GetFullPath(raw);
            if (!System.IO.Path.IsPathFullyQualified(raw) || System.IO.Path.GetFileName(path).Length == 0)
                throw new ArgumentException();
            // Windows device paths, streams and trailing dots/spaces can give one
            // directory entry a second spelling, bypassing the seed and player-path exclusions.
            if (OperatingSystem.IsWindows() && (path.StartsWith(@"\\?\") || path.StartsWith(@"\\.\") ||
                path[2..].Split('\\').Any(part => part.EndsWith('.') || part.EndsWith(' ') || part.Contains(':') ||
                    System.Text.RegularExpressions.Regex.IsMatch(part, @"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\.|$)",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase))))
                throw new ArgumentException();
            var root = Selected("SF2_SAVE_ROOT");
            var space = Selected("SF2_SAVE_NAMESPACE");
            if (System.IO.Path.IsPathFullyQualified(root))
            {
                var relative = System.IO.Path.GetRelativePath(root, path);
                var parts = relative.Split(System.IO.Path.DirectorySeparatorChar);
                if (!System.IO.Path.IsPathRooted(relative) && parts[0] != ".." &&
                    (parts[0].Equals(space, StringComparison.OrdinalIgnoreCase) || parts.Length >= 2 &&
                        (parts[1].Equals("sf2-opening", StringComparison.OrdinalIgnoreCase) ||
                         parts[1].Equals("sf2-preferences", StringComparison.OrdinalIgnoreCase))))
                    throw new ArgumentException();
            }
            return path;
        }
        _seedFile = FileOption("--load-save");
        _outputFile = FileOption("--save-output");
        if (_outputFile is null) return;
        string[] targets = [_outputFile, _outputFile + ".bak", _outputFile + ".lock"];
        if (targets.Contains(_seedFile, StringComparer.OrdinalIgnoreCase) ||
            options.TryGetValue("--input-settings", out var settings) &&
            targets.Contains(System.IO.Path.GetFullPath(settings), StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException();
    }

    private static string? ValidateRecord(ScenarioDefinition definition, SaveRecord record) =>
        SaveRecordRestore.Restore(definition, record, RuleCompositions.ForGame())
            is SessionStartFailed failed ? failed.Failure.Code : null;

    private LocalSaveStore Store(ScenarioDefinition definition) => Store(record => ValidateRecord(definition, record));

    private LocalSaveStore Store(Func<SaveRecord, string?> validate)
    {
        if (_outputFile is not null) return new(_outputFile, validate);
        if (SeedOnly) throw new ArgumentException("save-seed-read-only");
        return new(Selected("SF2_SAVE_ROOT"), Selected("SF2_SAVE_NAMESPACE"),
            System.IO.Path.GetFullPath(System.IO.Path.Combine(ProjectSettings.GlobalizePath("res://"), "..", "..")), validate);
    }

    private Task<bool> CheckContinueDefault()
    {
        try
        {
            var source = RestoreSource();
            // Empty and malformed records never trigger campaign admission for menu focus.
            var admission = new Lazy<ScenarioReadResult>(source.Read);
            var store = Store(record => admission.Value is ExplorationReadAccepted admitted
                ? ValidateRecord(admitted.Definition, record) : "save-content-required");
            return Task.Run(store.HasContinuablePrimary);
        }
        catch (ArgumentException) { return Task.FromResult(false); }
    }

    private async Task ShowHistory(LocalSaveStore store, ScenarioDefinition definition, string state, string message,
        Action<int, SlotRead> choose, Action cancel, bool continuing = false)
    {
        var page = await LoadEntry(() => store.ReadPage(_historyPage));
        _historyPage = page.Index;
        var choices = new List<(string, Action)>();
        (string Label, Action Choose)? create = !continuing && page.NewSlot is int fresh
            ? (GameText.Get("slot.create", fresh), () => choose(fresh, new(new(null, null), new(null, null)))) : null;
        foreach (var (slot, read) in page.Rows)
            choices.Add((Label(slot, read, definition, continuing), () =>
            {
                _saveSelection = _slots!.SelectedIndex;
                if (!continuing && read.WriteError is { } blocked)
                    ShowBlockedWrite(read, blocked, BackToHistory, create);
                else choose(slot, read);
            }));
        if (create is { } freshChoice)
            choices.Add((freshChoice.Label, () => { _saveSelection = _slots!.SelectedIndex; freshChoice.Choose(); }));
        if (page.Index > 0) choices.Add((GameText.Get("slot.previous"), () => ChangePage(-1)));
        if (page.Index + 1 < page.Count) choices.Add((GameText.Get("slot.next"), () => ChangePage(1)));
        string heading = _outputFile is null ? message : message + "\n" + GameText.Get("slot.output", System.IO.Path.GetFileName(_outputFile));
        _slots!.Cancel = cancel;
        _slots.ShowChoices(state, heading, choices.ToArray());
        _slots.Select(Math.Clamp(_saveSelection, 0, Math.Max(0, choices.Count - 1)));

        void ChangePage(int direction)
        {
            if (_entryLoading) return;
            _historyPage += direction; _saveSelection = 0;
            BackToHistory();
        }
        async void BackToHistory()
        {
            if (_entryLoading) return;
            try { await ShowHistory(store, definition, state, message, choose, cancel, continuing); }
            catch (ArgumentException error) { SaveError(error.Message); }
            catch (OperationCanceledException) { }
        }
    }

    private void ShowBlockedWrite(SlotRead read, string blocked, Action back, (string, Action)? create)
    {
        bool backupBlocks = read.Error is null || read.HasIncompatiblePrimary;
        string reason = SlotStatus(blocked);
        if (backupBlocks) reason = GameText.Get("slot.backup-status", reason);
        string message = GameText.Get(read.CanRecover ? "save.slot-recoverable" : "save.slot-protected", reason);
        var backChoice = (GameText.Get("entry.back"), back);
        _slots!.Cancel = back;
        _slots.ShowChoices("error", message, create is { } fresh ? [backChoice, fresh] : [backChoice]);
        _slots.ShowDetail(GameText.Get("save.details", blocked));
        DisarmInput();
    }

    private void OpenSavePanel(bool overEntry = false)
    {
        if (_slots is not null) return;
        _saveSelection = 0; _historyPage = 0;
        if (_entry is not null && !overEntry) _entry.Hide();
        _slots = new SaveSlotView { Name = "SaveSlotView", Cancel = CloseSavePanel, OverEntry = overEntry, Input = _input };
        if (overEntry) _entry?.Setup(true);
        _hostUi.AddChild(_slots); CoverSystemMenu(); ApplyHostPause(); DisarmInput();
    }
    private void CloseSavePanel()
    {
        if (_entryLoading) return;
        RemoveSavePanel();
        if (_session is not null) GameText.Bind(_session.Definition, System.Environment.GetEnvironmentVariable("SF2_DISPLAY_CATALOG"));
        if (_entry is not null) { _entry.Show(); _entry.ReturnFromSetup(); }
        if (_battleSaveCaller) { _battleSaveCaller = false; _battle?.ReturnFromBattleSave(); }
        ReturnToSystemMenu();
        ApplyHostPause();
        DisarmInput();
    }

    private void RemoveSavePanel()
    {
        if (_slots is not { } slots) return;
        _slots = null; _hostUi.RemoveChild(slots); slots.QueueFree();
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
        "save-record-changed" => GameText.Get("save.error.changed"),
        "save-seed-read-only" => GameText.Get("save.error.read-only"),
        "save-seed-missing" => GameText.Get("save.error.seed-missing"),
        "save-location-unavailable" => GameText.Get("save.error.location"),
        "save-profile-unavailable" or "save-battle-profile-unavailable" or "save-battle-route-unavailable" => GameText.Get("save.error.profile"),
        var incompatible when SlotRead.IsIncompatible(incompatible) => GameText.Get("save.error.compatibility"),
        "save-version" or "save-kind" or "save-shape" or "save-battle-recipe-version" => GameText.Get("slot.unsupported"),
        var corrupt when SlotRead.IsCorrupt(corrupt) => GameText.Get("slot.damaged"),
        "save-publication-uncertain" => GameText.Get("save.error.uncertain"),
        "save-storage-unavailable" => GameText.Get("save.error.storage"),
        "save-slot-empty" => GameText.Get("save.error.empty"),
        "restore-music-phase" => GameText.Get("save.error.music-phase"),
        "field-snapshot-busy" or "field-visit-action-pending" or "field-visit-sprite-pending" or
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
        else RequestFieldEntry(() => ShowSettledSave());
    }

    private void ShowBattleQuit()
    {
        if (_session is null || _entry is not null || _systemMenu is not null || _settings is not null ||
            _slots is not null || _nameEntry is not null || _windowRecovery is not null || _windowQuit is not null ||
            _battle?.CanOpenBattleQuit != true) return;
        _battleSaveCaller = true;
        ShowSettledSave();
    }

    private async void ShowSettledSave(bool church = false)
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
            var offer = SaveOffer(church: church);
            if (offer.Error is { } error) { SaveError(error); return; }
            var store = Store(_session.Definition);
            await ShowHistory(store, _session.Definition, "save-slots",
                GameText.Get(_session.Current.Active is ActiveBattle ? "save.battle-offer" : "save.offer", offer.Destination),
                (slot, seen) => ConfirmSave(slot, seen, store, offer, church), CloseSavePanel);
        }
        catch (ArgumentException error) { SaveError(error.Message); }
        catch (OperationCanceledException) { }
    }

    private static string Label(int slot, SlotRead read, ScenarioDefinition definition, bool forContinue = false)
    {
        string label = read.Primary switch
        {
            OpeningSaveRecord record => GameText.Get("slot.record", slot,
                DisplayNames.Member(definition, record.Data.Names, 0), SavedDestination(record.Data.Recipe), SavedTime(record)),
            FieldSnapshotRecord snapshot => GameText.Get("slot.record", slot,
                DisplayNames.Member(definition, snapshot.Data.Names, 0), FieldDestination(snapshot.Data.Landing), SavedTime(snapshot)),
            BattleSaveRecord battle => GameText.Get("slot.record", slot,
                DisplayNames.Member(definition, battle.Data.Story.Names, 0),
                BattleDestination(definition, battle.Data.Story.Names, battle.Data.Battle.Encounter, battle.Data.Battle.Round,
                    battle.Data.Battle.Queue[battle.Data.Battle.Cursor].Actor!), SavedTime(battle)),
            _ => GameText.Get("slot.status", slot, SlotStatus(read.Error ?? (read.Backup is not null || read.BackupError is not null
                ? "save-primary-missing" : null))),
        };
        if (read.CanRecover) return label + GameText.Get("slot.backup-review");
        if (read.BackupError is { } backupError && (!forContinue || read.Primary is null))
            return label + " · " + GameText.Get("slot.backup-status", SlotStatus(backupError));
        return label;
    }

    private static string SlotStatus(string? error) => error switch
    {
        var incompatible when SlotRead.IsIncompatible(incompatible) => GameText.Get("slot.matching"),
        "save-version" or "save-kind" or "save-shape" or "save-battle-recipe-version" => GameText.Get("slot.unsupported"),
        var corrupt when SlotRead.IsCorrupt(corrupt) => GameText.Get("slot.damaged"),
        "save-storage-unavailable" => GameText.Get("slot.unreadable"),
        "save-primary-missing" => GameText.Get("slot.primary-missing"),
        null => GameText.Get("slot.empty"),
        _ => GameText.Get("slot.unavailable"),
    };

    private static string FieldDestination(FieldReturnAnchor landing)
    {
        string map = landing.Map.Value;
        return GameText.Get("save.field-location", map.StartsWith("map-", StringComparison.Ordinal) ? map[4..] : map,
            landing.Position.X, landing.Position.Y);
    }

    // Display names for the existing finite recipes, not a new savepoint selection policy.
    private static string SavedDestination(string recipe) => recipe switch
    {
        "new-game" => GameText.Get("save.destination.new-game"),
        "map3-opening" => GameText.Get("save.destination.map3-opening"),
        "map3-joined" => GameText.Get("save.destination.map3-joined"),
        "granseal-field" => GameText.Get("save.destination.granseal-field"),
        "galam-field" => GameText.Get("save.destination.galam-field"),
        "hawel-field" => GameText.Get("save.destination.hawel-field"),
        "granseal-castle" => GameText.Get("save.destination.granseal-castle"),
        "tower-field" => GameText.Get("save.destination.tower-field"),
        "battle01-return" => GameText.Get("save.destination.battle01-return"),
        "new-granseal-castle" => GameText.Get("save.destination.new-granseal-castle"),
        "ribble-welcome" => GameText.Get("save.destination.ribble-welcome"),
        "dark-caves" => GameText.Get("save.destination.dark-caves"),
        "mount-volcano" => GameText.Get("save.destination.mount-volcano"),
        "devils-tail" => GameText.Get("save.destination.devils-tail"),
        "desktop-kingdom" => GameText.Get("save.destination.desktop-kingdom"),
        "willard-burrow" => GameText.Get("save.destination.willard-burrow"),
        "creed-mansion" => GameText.Get("save.destination.creed-mansion"),
        "evil-spirit-shrine" => GameText.Get("save.destination.evil-spirit-shrine"),
        "floor-world" => GameText.Get("save.destination.floor-world"),
        "fairy-cave" => GameText.Get("save.destination.fairy-cave"),
        "harpy-pond" => GameText.Get("save.destination.harpy-pond"),
        "taros-shrine" => GameText.Get("save.destination.taros-shrine"),
        "north-parmecia" => GameText.Get("save.destination.north-parmecia"),
        "southwest-parmecia" => GameText.Get("save.destination.southwest-parmecia"),
        "polca-raft" => GameText.Get("save.destination.polca-raft"),
        "polca-victory" => GameText.Get("save.destination.polca-victory"),
        "ketto" => GameText.Get("save.destination.ketto"),
        "pacalon-town" => GameText.Get("save.destination.pacalon-town"),
        "pacalon-castle" => GameText.Get("save.destination.pacalon-castle"),
        "pacalon-road" => GameText.Get("save.destination.pacalon-road"),
        "hassan" => GameText.Get("save.destination.hassan"),
        "polca-return" => GameText.Get("save.destination.polca-return"),
        "ancient-hall" => GameText.Get("save.destination.ancient-hall"),
        "east-shrine" => GameText.Get("save.destination.east-shrine"),
        "parmecia-battle-return" => GameText.Get("save.destination.parmecia-battle-return"),
        "yeel-field" => GameText.Get("save.destination.yeel-field"),
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

    private void ConfirmSave(int slot, SlotRead existing, LocalSaveStore store, HostSaveOffer offer, bool church)
    {
        _slots!.Cancel = BackToSave;
        string message = GameText.Get(existing.HasIncompatiblePrimary ? "save.overwrite-incompatible"
            : existing.Primary is not null ? "save.overwrite" : "save.confirm", slot);
        _slots.ShowChoices("confirm-save", message, (GameText.Get("entry.cancel"), BackToSave),
            (GameText.Get("save.and-return"), () => PublishSave(false)), (GameText.Get("save.and-quit"), () => PublishSave(true)));

        void BackToSave() { ShowSettledSave(church); }
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
                        return capture.Data is { } data ? store.Write(slot, existing, data) : new SlotWrite(false, capture.Error);
                    }
                    if (church)
                    {
                        SlotWrite? written = null;
                        var recorded = session.RecordChurchSnapshot(offer.SessionId, offer.Revision,
                            data => (written = store.Write(slot, existing, data)).Success);
                        return written ?? new SlotWrite(false, recorded.Error);
                    }
                    var field = session.CaptureFieldSnapshot(offer.SessionId, offer.Revision);
                    return field.Data is { } progress ? store.Write(slot, existing, progress) : new SlotWrite(false, field.Error);
                }, publishing: true, publicationObserved: written =>
                { if (written.Error == "save-publication-uncertain") _saveNeedsInspection = true; });
                if (!result.Success) { SaveError(result.Error!); return; }
                _savedSession = offer.SessionId; _savedRevision = session.Current.Revision;
                if (quit) { QuitGame(); return; }
                void ReturnToPlay() { CloseSavePanel(); CloseSystemMenu(); }
                _slots!.Cancel = ReturnToPlay;
                _slots.ShowChoices("saved", GameText.Get("save.saved", slot, offer.Destination), (GameText.Get("entry.return"), ReturnToPlay));
                DisarmInput();
            }
            catch (OperationCanceledException) { }
            catch (ArgumentException error) { SaveError(error.Message); }
        }
    }

    private void ShowContinue() => ShowContinue(loadSeed: SeedOnly);

    private async void ShowContinue(bool loadSeed, SessionEntryView? witch = null)
    {
        if (_entryLoading || _input is null) return;
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
            if (loadSeed)
            {
                var seed = await LoadEntry(() => LocalSaveStore.ReadSeed(_seedFile!));
                if (seed.Record is null) SaveError(seed.Error ?? "save-seed-missing");
                else RestoreRecord(seed.Record, admitted.Definition);
                return;
            }
            var store = Store(admitted.Definition);
            await ShowHistory(store, admitted.Definition, "continue-slots", GameText.Get("save.choose"),
                (slot, seen) => ContinueSlot(slot, seen, store, admitted.Definition, witch), CloseSavePanel, continuing: true);
            _saveNeedsInspection = false;
        }
        catch (ArgumentException error) { SaveError(error.Message); }
        catch (OperationCanceledException) { }
    }

    private async void ContinueSlot(int slot, SlotRead seen, LocalSaveStore store, ScenarioDefinition definition, SessionEntryView? witch)
    {
        try
        {
            if (_entryLoading || !EnsureWindowSupport()) return;
            var read = await LoadEntry(() => store.Read(slot));
            if (!read.Matches(seen)) { SaveError("save-record-changed"); return; }
            if (read.Primary is { } primary) { RestoreRecord(primary, definition, witch: witch); return; }
            if (read.Backup is { } backup && read.CanRecover)
            {
                _slots!.ShowChoices("recover-backup", GameText.Get("save.recovery-record", Label(slot,
                    new(read.Previous, new(null, null)), definition)),
                    (GameText.Get("entry.cancel"), CloseSavePanel), (GameText.Get("entry.recover"), () =>
                        RestoreRecord(backup, definition, () => LoadEntry(() => store.Recover(slot, read), publishing: true,
                            publicationObserved: written =>
                            { if (written.Error == "save-publication-uncertain") _saveNeedsInspection = true; }), witch)));
                _slots.ShowDetail(GameText.Get("save.details", read.Error ?? GameText.Get("slot.primary-missing")));
                return;
            }
            if (read.BackupError is { } backupError && read.Error is null)
            {
                _slots!.Cancel = CloseSavePanel;
                _slots.ShowChoices("error", GameText.Get("slot.backup-status", SlotStatus(backupError)),
                    (GameText.Get("entry.back"), CloseSavePanel));
                _slots.ShowDetail(GameText.Get("save.details", backupError));
                return;
            }
            SaveError(read.Error ?? "save-slot-empty");
        }
        catch (OperationCanceledException) { }
        catch (ArgumentException error) { SaveError(error.Message); }
    }

    private async void RestoreRecord(SaveRecord saved, ScenarioDefinition definition, Func<Task<SlotWrite>>? recover = null,
        SessionEntryView? witch = null)
    {
        try
        {
            if (_entryLoading || !EnsureWindowSupport()) return;
            var result = await LoadEntry(() => SaveRecordRestore.Restore(definition, saved, RuleCompositions.ForGame()));
            if (result is SessionStartFailed failed) { SaveError(failed.Failure.Code); return; }
            // The admitted initial recipe runs ordinary on-load and starts fresh-map playback.
            // Settled savepoints instead restore their saved music/history generation.
            await MountSession((SessionStarted)result, restoring: saved is not OpeningSaveRecord { Data.Recipe: "new-game" },
                settledField: saved is FieldSnapshotRecord, recover: recover, witch: witch);
            if (_session?.Current.SessionId == ((SessionStarted)result).Session.Current.SessionId)
            { _savedSession = _session.Current.SessionId; _savedRevision = _session.Current.Revision; }
        }
        catch (OperationCanceledException) { }
        catch (ArgumentException error) { SaveError(error.Message); }
    }

    private async Task<bool> PresentContinueFarewell(SessionEntryView witch, SessionStarted started)
    {
        if (_entry != witch || !GodotObject.IsInstanceValid(witch) || !witch.IsInsideTree()) return false;
        RemoveSavePanel();
        bool acknowledged = false;
        string hero = DisplayNames.Member(started.Session.Definition, started.Session.Current.Story, 0);
        witch.ShowContinueFarewell(hero, () => acknowledged = true);
        ApplyHostPause(); DisarmInput();
        var tree = GetTree();
        while (_entry == witch && GodotObject.IsInstanceValid(witch) && witch.IsInsideTree())
        {
            if (acknowledged && _windowRecovery is null && _windowQuit is null) return true;
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        }
        return false;
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
        if (_session is null) { QuitGame(); return; }
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
            (GameText.Get("entry.cancel"), cancel), (GameText.Get("entry.quit"), QuitGame));
    }
}
