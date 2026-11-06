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

    public override void _ExitTree() { ReleaseWindowSupport(); GetWindow().FocusExited -= SettingsFocusLost; _audio?.Dispose(); ClearPreparedNewGame(); }
    public string ReadAudioObservationJson() => System.Text.Json.JsonSerializer.Serialize(_audio?.ObservePlayback());

    public override void _Input(InputEvent input)
    {
        if (BlockSettingsRelease(input)) { GetViewport().SetInputAsHandled(); return; }
        if (_releaseBarrier && input is not InputEventMouse)
        {
            _input?.Resolve(input); _input?.DisarmMovement();
            if (_input?.GameplayHeld == false) _releaseBarrier = false;
            GetViewport().SetInputAsHandled(); return;
        }
        if (RouteWindowInput(input)) return;
        if (_settings is not null && input is InputEventMouse)
        { _settings.HandlePointer(input); GetViewport().SetInputAsHandled(); return; }
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
        if (input is InputEventMouse)
        {
            if (_entry is not null || _slots is not null) return;
            if (_menuButton is { } button && button.IsVisibleInTree() && button.GetGlobalRect().HasPoint(((InputEventMouse)input).Position)) return;
            if (_exploration is { } field && GodotObject.IsInstanceValid(field) && field.IsVisibleInTree())
                field.HandlePointer(input);
            else if (_battle?.IsVisibleInTree() == true) _battle.HandlePointer(input);
            GetViewport().SetInputAsHandled();
            return;
        }
        var action = _input?.Resolve(input);
        if (action is null) return;
        if (_exploration?.MenuEntryPending == true)
        {
            if (action is GameAction.Cancel or GameAction.Menu) _exploration.CancelMenuEntry();
            GetViewport().SetInputAsHandled(); return;
        }
        if (_settings is not null) HandleSettingsAction(action.Value, input);
        else if (_slots is not null) _slots.Handle(action.Value);
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
        GetTree().AutoAcceptQuit = false;
        GetWindow().FocusExited += SettingsFocusLost;
        var options = new Dictionary<string, string>(StringComparer.Ordinal);
        var arguments = OS.GetCmdlineUserArgs();
        for (int index = 0; index < arguments.Length; index++)
        {
            string option = arguments[index];
            if (option is not ("--authored-package" or "--private-battle-start" or "--private-exploration-start" or "--input-settings"))
            { Fail("unknown-startup-option", "Unknown game option."); return; }
            if (options.ContainsKey(option))
            { Fail("duplicate-startup-option", "Each game option may appear only once."); return; }
            if (option != "--input-settings" && options.Keys.Any(key => key != "--input-settings"))
            { Fail("conflicting-startup-options", "Select one session entry."); return; }
            if (++index >= arguments.Length || string.IsNullOrWhiteSpace(arguments[index]) || arguments[index].StartsWith("--", StringComparison.Ordinal))
            { Fail("missing-startup-path", "The selected game option requires a path."); return; }
            options.Add(option, arguments[index]);
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
        else ShowEntry();
    }

    private static string Selected(string name) => System.Environment.GetEnvironmentVariable(name) ?? "";
    private static PrivateBattleScenarioReader PrivateBattle(string selectedStart) => new(
        Selected("SF2_PRIVATE_BATTLE01_DATA"), Selected("SF2_PRIVATE_BATTLE01_SCENE"), Selected("SF2_PRIVATE_BATTLE01_TERRAIN"),
        Selected("SF2_PRIVATE_STATIC_DATA"), Selected("SF2_PRIVATE_ENEMY_DATA"), Selected("SF2_PRIVATE_ENEMY_GOLD"), selectedStart,
        System.Environment.GetEnvironmentVariable("SF2_PRIVATE_BATTLE_SCENE_CONTENT"));

    private void ShowEntry()
    {
        if (_entry is not null) return;
        _entry = new SessionEntryView { Name = "SessionEntryView", Replacement = _session is not null,
            NewGame = BeginNewGame, Cancel = CloseEntry, Continue = ShowContinue, Quit = RequestQuit, Settings = ShowSettings };
        _hostUi.AddChild(_entry);
        if (_input is not null) _entry.ConfigureHints(_input);
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
        _input?.DisarmMovement(); _input?.DisarmWait();
        _releaseBarrier = _input?.GameplayHeld == true;
    }

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

    private void BeginSource(IScenarioSource source)
    {
        if (!EnsureWindowSupport()) { _windowStartup = () => BeginSource(source); return; }
        var outcome = GameSession.Start(source, Sf2.Remake.Application.Gameplay.RuleCompositions.ForGame());
        if (outcome is SessionStartFailed failed) { Fail(failed.Failure); return; }
        var started = (SessionStarted)outcome;
        if (started.Result.Failure is { } startFailure) { Fail(startFailure); return; }
        MountSession(started);
    }

    private void MountSession(SessionStarted started, SessionAudio? preparedAudio = null)
    {
        GameText.Bind(started.Session.Definition, System.Environment.GetEnvironmentVariable("SF2_DISPLAY_CATALOG"));
        SessionAudio? candidateAudio = preparedAudio;
        if (candidateAudio is null && started.Session.Definition.Exploration is { } candidateDefinition)
        {
            candidateAudio = new SessionAudio(this, candidateDefinition);
            if (candidateAudio.Error is { } error)
            {
                candidateAudio.Dispose(); Fail(error, "Selected private audio content is unavailable."); return;
            }
        }
        if (_restoring && candidateAudio is not null)
        {
            candidateAudio.Restore(started.Result.Snapshot);
            candidateAudio.SetPaused(true);
            if (candidateAudio.Error is { } restoreAudioError)
            { candidateAudio.Dispose(); SaveError(restoreAudioError); return; }
        }
        BattleSessionView? preparedBattle = null;
        if (_restoring && SessionPresentationOwner.IsBattle(started.Result.Snapshot))
        {
            // Build the actual battle resources while the previous session is still owned.
            // Attach only projects this already restored player boundary; it never advances it.
            var previousScale = GetWindow().ContentScaleMode;
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
                GetWindow().ContentScaleMode = previousScale;
                SaveError(error.Message); return;
            }
        }
        // Admission and required audio preparation succeed before the old owner is released.
        // A failed replacement leaves the old session paused behind the cancellable entry.
        ReleaseSession();
        CloseEntry();
        CloseSavePanel();
        _session = started.Session;
        _audio = candidateAudio;
        var view = preparedBattle ?? new BattleSessionView { WorldViewport = _worldViewport };
        view.Name = "BattleSessionView";
        if (preparedBattle is not null) _worldViewport = preparedBattle.WorldViewport;
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
        view.LeaveBattle = result => ShowExploration(result, returning: true);
        if (SessionPresentationOwner.IsBattle(started.Result.Snapshot)) EnterBattle(started.Result);
        else { view.Hide(); ShowExploration(started.Result, returning: false); }
        _menuButton = new Button { Name = "SystemMenuEntry", Text = GameText.Get("system.entry"), AnchorLeft = 1, AnchorRight = 1,
            OffsetLeft = -180, OffsetRight = -20, OffsetTop = 12, OffsetBottom = 68,
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
        void ShowExploration(SessionResult initial, bool returning)
        {
            var exploration = new ExplorationSessionView { Name = "ExplorationSessionView", WorldViewport = _worldViewport,
                RequestServiceSave = ShowSave };
            _exploration = exploration; AddChild(exploration);
            if (returning) { view.Reparent(exploration, false); exploration.MoveChild(view, 0); }
            exploration.Begin(started.Session, initial, _input!, _audio!, EnterBattle,
                result => { view.Reparent(exploration, false); view.Attach(started.Session, result); },
                returning ? () => { view.Reparent(this, false); view.Hide(); } : null);
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
            _menuButton.TooltipText = _input!.Hint(GameAction.Menu);
            if (!_input.MenuPadAvailable) _menuButton.TooltipText += "\n" + GameText.Get("system.no-pad");
        }
        if (_systemMenu is null) return;
        // Inspection Menu entry is pure, but Save must be reachable before its real
        // music observation is published. Validate that live observation only for the offer.
        var save = SaveOffer(previewMusic: _battle?.Inspecting == true);
        string saveReason = save.Error is "save-busy" or "save-player-unsettled" or "save-entity-unsettled" or "save-story-motion"
            ? Explain(save.Error) : GameText.Get("system.save-reason");
        if (_session!.Current.Active is ActiveBattle && save.Error is { } battleError) saveReason = Explain(battleError);
        _systemMenu.Present(save.Error is null && _fieldEntryFailure is null,
            _fieldEntryFailure is { } error ? Explain(error) : save.Error is null ? "" : saveReason, CanOpenSettings,
            _session.Current.CanWaitAtInput && _exploration?.PartyMenuOpen != true,
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
