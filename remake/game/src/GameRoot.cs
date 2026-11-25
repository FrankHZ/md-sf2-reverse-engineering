using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Exploration;
using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.Maps;
using Sf2.Remake.GodotAdapter.Audio;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Entry;

namespace Sf2.Remake.GodotAdapter;

// Ordinary product entry; authored/controlled starts remain explicit diagnostic options.
public sealed partial class GameRoot : Node
{
    private Control _hostUi = null!;
    private WorldViewportLayout _worldViewport = new();
    private GameInput? _input;
    private GameSession? _session;
    private BattleSessionView? _battle;
    private ExplorationSessionView? _exploration;
    private SessionAudio? _audio;
    private SessionEntryView? _entry;
    private Button? _menuButton;
    private SystemMenuView? _systemMenu;
    private bool _releaseBarrier;
    public override void _ExitTree()
    {
        ReleaseOpening();
        ReleaseWindowSupport(); GetWindow().FocusExited -= SettingsFocusLost;
        GetWindow().FocusEntered -= PacingFocusReturned; Godot.Input.JoyConnectionChanged -= PacingDeviceChanged;
        _audio?.Dispose(); ClearPreparedNewGame();
    }
    public string ReadAudioObservationJson() => System.Text.Json.JsonSerializer.Serialize(_audio?.ObservePlayback());

    public override void _Input(InputEvent input)
    {
        _input?.ObserveHintInput(input, GetWindow().HasFocus());
        _input?.AllowAcceleration(!_releaseBarrier && (_exploration?.CanAccelerate == true || _battle?.CanAccelerate == true));
        _input?.ObserveAccelerationInput(input);
        if (BlockSettingsRelease(input)) { GetViewport().SetInputAsHandled(); return; }
        if (_releaseBarrier && input is not InputEventMouse)
        {
            _input?.Resolve(input); _input?.DisarmMovement();
            if (_input?.GameplayHeld == false) _releaseBarrier = false;
            GetViewport().SetInputAsHandled(); return;
        }
        if (RouteWindowInput(input)) return;
        if (RouteOpeningInput(input)) return;
        if (_settings is not null && !GetWindow().HasFocus()) { GetViewport().SetInputAsHandled(); return; }
        if (_settings?.CaptureInput(input) == true)
        {
            _input?.Resolve(input); _input?.DisarmMovement(); _input?.DisarmWait();
            GetViewport().SetInputAsHandled(); return;
        }
        if (_settings is { } settings)
        {
            if (input is InputEventMouse)
            {
                if (settings.RoutePointer(input)) GetViewport().SetInputAsHandled();
                return;
            }
            var mapped = _input?.Resolve(input);
            if (mapped is { } actionId) HandleSettingsAction(actionId, input);
            GetViewport().SetInputAsHandled();
            return;
        }
        if (_systemMenu?.Visible == true && input is InputEventMouse)
        { _systemMenu.HandlePointer(input); GetViewport().SetInputAsHandled(); return; }
        if (_nameEntry is { } name)
        {
            // Track pad button/axis releases and wait state without routing actions to
            // the name editor or the hidden slot/menu. Only keyboard and pointer GUI remain.
            _input?.Resolve(input); _input?.DisarmMovement(); _input?.DisarmWait();
            if (input is InputEventKey key) name.HandleKeyboard(key);
            else if (input is not InputEventMouse) GetViewport().SetInputAsHandled();
            return;
        }
        var members = _entry is null && _slots is null && _systemMenu is null
            ? _exploration?.NativeMembers ?? _battle?.NativeMembers : null;
        if (members is not null && input is InputEventMouse)
        {
            _input?.DisarmMovement();
            if (members.RoutePointer(input)) GetViewport().SetInputAsHandled();
            return;
        }
        if (input is InputEventMouse)
        {
            if ((_slots?.HandlePointerCancel(input) ?? _entry?.HandlePointerCancel(input)) == true)
            { GetViewport().SetInputAsHandled(); return; }
            if (_entry is not null || _slots is not null) return;
            if (_menuButton is { } button && button.IsVisibleInTree() && button.GetGlobalRect().HasPoint(((InputEventMouse)input).Position)) return;
            if (_exploration is { } field && GodotObject.IsInstanceValid(field) && field.IsVisibleInTree())
                field.HandlePointer(input);
            else if (_battle?.IsVisibleInTree() == true) _battle.HandlePointer(input);
            GetViewport().SetInputAsHandled();
            return;
        }
        bool freshTargetNavigation = _battle?.NeedsFreshDirection == true;
        var action = _input?.Resolve(input, freshTargetNavigation);
        if (action is null)
        {
            if (members is not null) GetViewport().SetInputAsHandled();
            return;
        }
        if (_exploration?.MenuEntryPending == true)
        {
            if (action is GameAction.Cancel or GameAction.Menu) _exploration.CancelMenuEntry();
            GetViewport().SetInputAsHandled(); return;
        }
        if (_slots is not null) _slots.Handle(action.Value);
        else if (_entry is not null) _entry.Handle(action.Value);
        else if (_systemMenu is not null) _systemMenu.Handle(action.Value);
        else if (action == GameAction.Menu) ShowSystemMenu();
        else if (action == GameAction.Stay && _exploration?.CanRequestFieldSave == true) ShowSave();
        else if (_exploration is { } exploration && GodotObject.IsInstanceValid(exploration) && exploration.IsVisibleInTree())
            exploration.HandleAction(action.Value);
        else if (_battle?.IsVisibleInTree() == true) _battle.HandleAction(action.Value);
        GetViewport().SetInputAsHandled();
    }

    public override void _Process(double delta)
    {
        ProcessOpening();
        RefreshPartyName();
        if (_menuButton is not null)
        {
            _menuButton.Visible = _entry is null && _settings is null && _slots is null && _nameEntry is null && _systemMenu is null && _windowRecovery is null;
            _menuButton.Disabled = _exploration?.PartyMenuOpen == true;
        }
    }

    public override void _Ready()
    {
        _hostUi = UI.PresentationCanvas.Mount(this, "HostUi", UI.PresentationCanvas.Host);
        GetWindow().FocusExited += SettingsFocusLost;
        GetWindow().FocusEntered += PacingFocusReturned;
        Godot.Input.JoyConnectionChanged += PacingDeviceChanged;
        if (_preview is { } preview)
        {
            ConfigureInput(preview.Settings);
            if (preview.Source is { } source) BeginSource(source);
            else ShowEntry();
            return;
        }
        GetTree().AutoAcceptQuit = false;
        var options = new Dictionary<string, string>(StringComparer.Ordinal);
        var arguments = OS.GetCmdlineUserArgs();
        for (int index = 0; index < arguments.Length; index++)
        {
            string option = arguments[index];
            if (option is not ("--authored-package" or "--private-battle-start" or "--private-exploration-start" or "--input-settings" or "--load-save" or "--save-output"))
            { Fail("unknown-startup-option", "Unknown game option."); return; }
            if (options.ContainsKey(option))
            { Fail("duplicate-startup-option", "Each game option may appear only once."); return; }
            if (++index >= arguments.Length || string.IsNullOrWhiteSpace(arguments[index]) || arguments[index].StartsWith("--", StringComparison.Ordinal))
            { Fail("missing-startup-path", "The selected game option requires a path."); return; }
            options.Add(option, arguments[index]);
        }
        bool diagnostic = options.Keys.Any(key => key is "--authored-package" or "--private-battle-start" or "--private-exploration-start");
        if (diagnostic && options.Keys.Count(key => key != "--input-settings") != 1)
        { Fail("conflicting-startup-options", "Select one session entry."); return; }
        try { ConfigureSaveFiles(options); }
        catch (Exception error) when (error is ArgumentException or System.IO.IOException or NotSupportedException)
        {
            GameText.Initialize(OS.GetLocaleLanguage() == "zh" ? "zh-CN" : "en");
            Fail("invalid-save-paths", "Use distinct save files outside the player namespace and settings files.");
            return;
        }
        try { LoadPreferences(options.GetValueOrDefault("--input-settings")); }
        catch (Exception error) when (error is System.IO.IOException or UnauthorizedAccessException or
            System.Text.Json.JsonException or ArgumentException or NotSupportedException)
        { Fail("invalid-input-settings", "Cannot read the versioned input settings."); return; }

        InitializeWindowSupport();
        if (options.TryGetValue("--private-battle-start", out string? controlledStart)) BeginSource(PrivateBattle(controlledStart));
        else if (options.TryGetValue("--private-exploration-start", out string? explorationStart))
            BeginSource(new PrivateExplorationReader(Selected("SF2_PRIVATE_EXPLORATION_CONTENT"), explorationStart,
                PrivateBattle(Selected("SF2_PRIVATE_CONTROLLED_START"))));
        else if (options.TryGetValue("--authored-package", out string? authored)) BeginSource(new AuthoredScenarioPackageReader(authored));
        else
        {
            if (_seedFile is not null) { ShowEntry(); ShowContinue(loadSeed: true); }
            else BeginOpening();
        }
    }

    private static string Selected(string name) => System.Environment.GetEnvironmentVariable(name) ?? "";
    private static PrivateBattleScenarioReader PrivateBattle(string selectedStart) => new(
        Selected("SF2_PRIVATE_BATTLE01_DATA"), Selected("SF2_PRIVATE_BATTLE01_SCENE"), Selected("SF2_PRIVATE_BATTLE01_TERRAIN"),
        Selected("SF2_PRIVATE_STATIC_DATA"), Selected("SF2_PRIVATE_ENEMY_DATA"), Selected("SF2_PRIVATE_ENEMY_GOLD"), selectedStart,
        System.Environment.GetEnvironmentVariable("SF2_PRIVATE_BATTLE_SCENE_CONTENT"));

    private void ShowEntry()
    {
        if (_entry is not null) return;
        var entry = new SessionEntryView { Name = "SessionEntryView", Replacement = _session is not null,
            NewGame = BeginNewGame, Cancel = CloseEntry, Quit = RequestQuit, Settings = ShowSettings,
            CheckContinueDefault = SeedOnly || _input is null ? null : CheckContinueDefault };
        entry.Continue = () => ShowContinue(loadSeed: SeedOnly, witch: entry.HasContinueDialogue ? entry : null);
        _entry = entry;
        _hostUi.AddChild(entry);
        if (_input is not null) entry.ConfigureHints(_input);
        CoverSystemMenu();
        ApplyHostPause(); DisarmInput();
    }

    private void CloseEntry()
    {
        if (_entry is { } entry) { _entry = null; _hostUi.RemoveChild(entry); entry.QueueFree(); }
        ReturnToSystemMenu();
        ApplyHostPause();
        DisarmInput();
    }

    private void DisarmInput()
    {
        _input?.AllowAcceleration(false); _exploration?.SuspendPacing(); _battle?.SuspendPacing();
        _input?.DisarmMovement(); _input?.DisarmWait();
        _releaseBarrier = _input?.GameplayHeld == true;
    }

    private void PacingDeviceChanged(long device, bool connected)
    { _input?.HintDeviceChanged((int)device, connected); if (!connected) { _settings?.AbortCapture((int)device); _input?.ReleaseAccelerationDevice((int)device); _exploration?.SuspendPacing(); _battle?.SuspendPacing(); } }

    private void PacingFocusReturned() { _settings?.SeedCaptureControls(); _input?.HintFocus(true, GetViewport().GetMousePosition()); _input?.ReconcileAccelerationNeutral(); }

    private void ReleaseSession()
    {
        // Detach synchronously: old views cannot receive input/ticks or publish old completions.
        // Their existing ExitTree hooks release projections and subscriptions before audio ends.
        var field = _exploration; var battle = _battle;
        _exploration = null; _battle = null; _session = null;
        if (field is not null && GodotObject.IsInstanceValid(field)) { field.GetParent()?.RemoveChild(field); field.Free(); }
        if (battle is not null && GodotObject.IsInstanceValid(battle)) { battle.GetParent()?.RemoveChild(battle); battle.Free(); }
        _audio?.Dispose(); _audio = null;
        CloseSystemMenu();
        if (_menuButton is not null) { _hostUi.RemoveChild(_menuButton); _menuButton.QueueFree(); _menuButton = null; }
        _worldViewport = new();
        DisarmInput();
    }

    private async void BeginSource(IScenarioSource source)
    {
        if (!EnsureWindowSupport()) { _windowStartup = () => BeginSource(source); return; }
        var outcome = GameSession.Start(source, Sf2.Remake.Application.Gameplay.RuleCompositions.ForGame());
        if (outcome is SessionStartFailed failed) { Fail(failed.Failure); return; }
        var started = (SessionStarted)outcome;
        if (started.Result.Failure is { } startFailure) { Fail(startFailure); return; }
        await MountSession(started);
    }

    private async Task MountSession(SessionStarted started, SessionAudio? preparedAudio = null,
        bool restoring = false, bool settledField = false, Func<Task<Persistence.SlotWrite>>? recover = null,
        SessionEntryView? witch = null)
    {
        GameText.Bind(started.Session.Definition, System.Environment.GetEnvironmentVariable("SF2_DISPLAY_CATALOG"));
        SessionAudio? candidateAudio = preparedAudio;
        if (candidateAudio is null && started.Session.Definition.Exploration is { } candidateDefinition)
        {
            candidateAudio = new SessionAudio(this, candidateDefinition);
            if (candidateAudio.Error is { } error)
            {
                candidateAudio.Dispose();
                if (_slots is not null && (restoring || witch is not null)) SaveError(error);
                else Fail(error, "Selected private audio content is unavailable.");
                return;
            }
        }
        if (restoring && candidateAudio is not null)
        {
            candidateAudio.Restore(started.Result, settledField);
            if (candidateAudio.Error is { } restoreAudioError)
            { candidateAudio.Dispose(); SaveError(restoreAudioError); return; }
            candidateAudio.SetPaused(true);
        }
        BattleSessionView? preparedBattle = null;
        if (restoring && SessionPresentationOwner.IsBattle(started.Result.Snapshot))
        {
            // Build the actual battle resources while the previous session is still owned.
            // Attach only projects this already restored player boundary; it never advances it.
            preparedBattle = new BattleSessionView { Name = "PreparedBattle", ProcessMode = ProcessModeEnum.Disabled,
                WorldViewport = new(), Audio = candidateAudio };
            AddChild(preparedBattle); preparedBattle.ConfigureInput(_input!);
            try
            {
                preparedBattle.Attach(started.Session, started.Result); preparedBattle.Hide();
                if (preparedBattle.StartupError is { } resourceError) throw new InvalidOperationException(resourceError);
            }
            catch (Exception error) when (error is InvalidOperationException or ArgumentException)
            {
                RemoveChild(preparedBattle); preparedBattle.Free(); candidateAudio?.Dispose();
                SaveError(error.Message); return;
            }
        }
        var view = preparedBattle ?? new BattleSessionView();
        ExplorationSessionView? preparedField = null;
        if (settledField)
        {
            preparedField = CreateField(new());
            preparedField.ProcessMode = ProcessModeEnum.Disabled;
            preparedField.Hide();
            AddChild(preparedField);
            try
            {
                BeginField(preparedField, started.Result, retainBoard: false, candidateAudio!, preparing: true);
                if (preparedField.PrepareSettledAttachment() is { } error) throw new InvalidOperationException(error);
            }
            catch (Exception error) when (error is InvalidOperationException or ArgumentException or KeyNotFoundException)
            {
                DisposeCandidate();
                SaveError(error.Message);
                return;
            }
        }
        if (recover is not null)
        {
            // An explicit backup promotion waits for the same attachment preflight as Continue.
            bool published = false;
            try
            {
                var result = await recover();
                if (!result.Success) { SaveError(result.Error!); return; }
                published = true;
            }
            finally { if (!published) DisposeCandidate(); }
        }
        if (witch is not null && !await PresentContinueFarewell(witch, started))
        {
            DisposeCandidate();
            return;
        }
        // Only the validated candidate can release the current session.
        ReleaseSession();
        CloseEntry();
        CloseSavePanel();
        _session = started.Session;
        _audio = candidateAudio;
        view.Name = "BattleSessionView";
        _worldViewport = preparedField?.WorldViewport ?? preparedBattle?.WorldViewport ?? _worldViewport;
        view.WorldViewport = _worldViewport;
        _battle = view;
        view.RequestSpeed = ShowBattleSpeed;
        view.RequestQuit = ShowBattleQuit;
        if (view.GetParent() is null) AddChild(view);
        view.ConfigureInput(_input!);
        if (_audio is not null)
        {
            view.Audio = _audio;
            view.ObserveResult = result => { _audio.Observe(result); CheckAudio(); };
            view.PlayUiSound = command => { _audio.PlayEffect(command); CheckAudio(); };
        }
        view.LeaveBattle = (result, retainBoard) =>
        {
            if (!retainBoard) view.Hide();
            ShowExploration(result, retainBoard);
        };
        if (SessionPresentationOwner.IsBattle(started.Result.Snapshot)) EnterBattle(started.Result);
        else
        {
            view.Hide();
            if (preparedField is null) ShowExploration(started.Result, retainBoard: false);
            else
            {
                _exploration = preparedField;
                preparedField.Show();
                preparedField.PublishAttachment();
            }
        }
        _menuButton = new Button { Name = "SystemMenuEntry", Text = GameText.Get("system.entry"), AnchorLeft = 1, AnchorRight = 1,
            OffsetLeft = -UI.StoryTheme.GameplayInset - 160, OffsetRight = -UI.StoryTheme.GameplayInset,
            OffsetTop = UI.StoryTheme.GameplayInset, OffsetBottom = UI.StoryTheme.GameplayInset + UI.StoryTheme.MenuHeight,
            Theme = UI.StoryTheme.CreateEntry(), FocusMode = Control.FocusModeEnum.None };
        _menuButton.AddThemeFontSizeOverride("font_size", 24);
        _menuButton.Pressed += ShowSystemMenu; _hostUi.AddChild(_menuButton); RefreshSystemMenuLabels();
        ApplyHostPause(); DisarmInput();

        void CheckAudio() { if (_audio?.Error is { } failure) Fail(failure, "Required private audio could not be consumed."); }
        void EnterBattle(SessionResult result)
        {
            if (_exploration is { } exploration)
            {
                _exploration = null;
                if (view.GetParent() == exploration) view.Reparent(this, false);
                exploration.QueueFree();
            }
            view.Attach(started.Session, result);
        }
        ExplorationSessionView CreateField(WorldViewportLayout viewport) => new()
        {
            Name = "ExplorationSessionView", WorldViewport = viewport,
            RequestServiceSave = () => RequestFieldEntry(() => ShowSettledSave(church: true)),
        };
        void ShowExploration(SessionResult initial, bool retainBoard)
        {
            var exploration = CreateField(_worldViewport);
            _exploration = exploration; AddChild(exploration);
            BeginField(exploration, initial, retainBoard, _audio!);
        }
        void BeginField(ExplorationSessionView exploration, SessionResult initial, bool retainBoard, SessionAudio audio, bool preparing = false)
        {
            if (retainBoard) { view.Reparent(exploration, false); exploration.MoveChild(view, 0); }
            exploration.Begin(started.Session, initial, _input!, audio, EnterBattle,
                result => { view.Reparent(exploration, false); view.Attach(started.Session, result); },
                retainBoard ? () => { view.Reparent(this, false); view.Hide(); } : null, preparing);
        }
        void DisposeCandidate()
        {
            if (preparedField is not null && GodotObject.IsInstanceValid(preparedField)) preparedField.Free();
            if (GodotObject.IsInstanceValid(view)) view.Free();
            candidateAudio?.Dispose();
        }
    }

    private void ShowSystemMenu()
    {
        if (_session is null || _systemMenu is not null || _entry is not null || _slots is not null ||
            _settings is not null || _windowRecovery is not null || _exploration?.PartyMenuOpen == true) return;
        RequestFieldEntry(OpenSystemMenu);
    }
    private void OpenSystemMenu()
    {
        if (_systemMenu is not null) return;
        _systemMenu = new SystemMenuView { Name = "SystemMenuView", Input = _input!, Resume = CloseSystemMenu };
        _hostUi.AddChild(_systemMenu); ApplyHostPause();
        // Opening/closing a host modal over read-only inspection cannot publish a revision.
        // Explicit Save still captures current music in ShowSettledSave.
        if (_fieldEntryFailure is null && _battle?.Inspecting != true) _fieldEntryFailure = ObserveSaveMusic();
        RefreshSystemMenuLabels(); DisarmInput();
    }
    private void RefreshSystemMenuLabels()
    {
        if (_menuButton is not null)
        {
            _menuButton.Text = GameText.Get("system.entry");
            _menuButton.TooltipText = _input!.CompactHint(GameAction.Menu);
            if (!_input.MenuPadAvailable) _menuButton.TooltipText += "\n" + GameText.Get("system.no-pad");
        }
        if (_systemMenu is null) return;
        // Inspection Menu entry is pure, but Save must be reachable before its real
        // music observation is published. Validate that live observation only for the offer.
        var save = SaveOffer(previewMusic: _battle?.Inspecting == true);
        string saveReason = save.Error switch
        {
            null => "",
            "field-visit-mode" => GameText.Get("system.save-reason"),
            var code => Explain(code),
        };
        _systemMenu.Present(save.Error is null && _fieldEntryFailure is null,
            _fieldEntryFailure is { } error ? Explain(error) : save.Error is null ? "" : saveReason, CanOpenSettings,
            _session!.Current.CanWaitAtInput && _exploration?.PartyMenuOpen != true,
            ShowSave, ShowSettings, ShowEntry, RequestQuit);
    }
    private void CoverSystemMenu() { _systemMenu?.SuspendPointer(); _systemMenu?.Hide(); }
    private void ReturnToSystemMenu()
    {
        if (_systemMenu is null || _entry is not null || _slots is not null || _settings is not null || _windowRecovery is not null) return;
        RefreshSystemMenuLabels(); _systemMenu.Show();
    }
    private void CloseSystemMenu()
    {
        _fieldEntryFailure = null;
        if (_systemMenu is { } menu) { _systemMenu = null; _hostUi.RemoveChild(menu); menu.QueueFree(); }
        ApplyHostPause(); DisarmInput();
    }

    private void Fail(string code, string message) => Fail(new(SessionFailureKind.ContentError, code, "startup", message));
    private void Fail(SessionFailure failure)
    {
        if (_slots is not null) { SaveError(failure.Code); return; }
        ShowEntry(); _entry!.Fail(failure, _input is not null);
    }
}
