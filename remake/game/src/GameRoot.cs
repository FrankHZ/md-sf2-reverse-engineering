using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Exploration;
using Godot;
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
    private WorldViewportLayout _worldViewport = new();
    private GameInput? _input;
    private GameSession? _session;
    private BattleSessionView? _battle;
    private ExplorationSessionView? _exploration;
    private SessionAudio? _audio;
    private SessionEntryView? _entry;
    private Button? _restart;
    private bool _releaseBarrier;

    public override void _ExitTree() => _audio?.Dispose();
    public string ReadAudioObservationJson() => System.Text.Json.JsonSerializer.Serialize(_audio?.ObservePlayback());

    public override void _Input(InputEvent input)
    {
        if (input is InputEventMouse)
        {
            if (_entry is not null) return;
            if (_restart is not null && _restart.GetGlobalRect().HasPoint(((InputEventMouse)input).Position)) return;
            if (_exploration is { } field && GodotObject.IsInstanceValid(field) && field.IsVisibleInTree())
                field.HandlePointer(input);
            else if (_battle?.IsVisibleInTree() == true) _battle.HandlePointer(input);
            GetViewport().SetInputAsHandled();
            return;
        }
        var action = _input?.Resolve(input);
        if (_releaseBarrier)
        {
            _input?.DisarmMovement();
            if (_input?.GameplayHeld == false) _releaseBarrier = false;
            GetViewport().SetInputAsHandled(); return;
        }
        if (action is null) return;
        if (_entry is not null) _entry.Handle(action.Value);
        else if (_exploration is { } exploration && GodotObject.IsInstanceValid(exploration) && exploration.IsVisibleInTree())
            exploration.HandleAction(action.Value);
        else if (_battle?.IsVisibleInTree() == true) _battle.HandleAction(action.Value);
        GetViewport().SetInputAsHandled();
    }

    public override void _Process(double delta)
    {
        if (_restart is not null) _restart.Disabled = _session?.Current.CanWaitAtInput != true;
    }

    public override void _Ready()
    {
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
        try { _input = new GameInput(InputSettings.Load(options.GetValueOrDefault("--input-settings"))); }
        catch (Exception error) when (error is System.IO.IOException or UnauthorizedAccessException or
            System.Text.Json.JsonException or ArgumentException or NotSupportedException)
        { Fail("invalid-input-settings", "Cannot read the versioned input settings."); return; }

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
        if (_exploration is not null) _exploration.ProcessMode = ProcessModeEnum.Disabled;
        if (_battle is not null) _battle.ProcessMode = ProcessModeEnum.Disabled;
        _entry = new SessionEntryView { Name = "SessionEntryView", Replacement = _session is not null,
            NewGame = BeginNewGame, Cancel = CloseEntry };
        AddChild(_entry);
        if (_input is not null) _entry.ConfigureHints(_input);
        DisarmInput();
    }

    private void CloseEntry()
    {
        if (_entry is { } entry) { _entry = null; RemoveChild(entry); entry.QueueFree(); }
        if (_exploration is not null) _exploration.ProcessMode = ProcessModeEnum.Inherit;
        if (_battle is not null) _battle.ProcessMode = ProcessModeEnum.Inherit;
        DisarmInput();
    }

    private void DisarmInput()
    {
        _input?.DisarmMovement(); _input?.DisarmWait();
        _releaseBarrier = _input?.GameplayHeld == true;
    }

    private void BeginNewGame()
    {
        if (_input is null) return;
        // Select each seed once per confirmed start, including the full legal uint domain.
        Span<byte> seeds = stackalloc byte[8]; System.Security.Cryptography.RandomNumberGenerator.Fill(seeds);
        BeginSource(new OpeningNewGameReader(Selected("SF2_PRIVATE_EXPLORATION_CONTENT"),
            Selected("SF2_PRIVATE_BATTLE01_DATA"), Selected("SF2_PRIVATE_BATTLE01_SCENE"), Selected("SF2_PRIVATE_BATTLE01_TERRAIN"),
            Selected("SF2_PRIVATE_STATIC_DATA"), Selected("SF2_PRIVATE_ENEMY_DATA"), Selected("SF2_PRIVATE_ENEMY_GOLD"),
            BitConverter.ToUInt32(seeds[..4]), BitConverter.ToUInt32(seeds[4..]),
            System.Environment.GetEnvironmentVariable("SF2_PRIVATE_BATTLE_SCENE_CONTENT")));
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
        if (_restart is not null) { RemoveChild(_restart); _restart.QueueFree(); _restart = null; }
        _worldViewport = new();
        DisarmInput();
    }

    private void BeginSource(IScenarioSource source)
    {
        var outcome = GameSession.Start(source, Sf2.Remake.Application.Gameplay.RuleCompositions.ForGame());
        if (outcome is SessionStartFailed failed) { Fail(failed.Failure); return; }
        var started = (SessionStarted)outcome;
        if (started.Result.Failure is { } startFailure) { Fail(startFailure); return; }
        SessionAudio? candidateAudio = null;
        if (started.Session.Definition.Exploration is { } candidateDefinition)
        {
            candidateAudio = new SessionAudio(this, candidateDefinition);
            if (candidateAudio.Error is { } error)
            {
                candidateAudio.Dispose(); Fail(error, "Selected private audio content is unavailable."); return;
            }
        }
        // Admission and required audio preparation succeed before the old owner is released.
        // A failed replacement leaves the old session paused behind the cancellable entry.
        ReleaseSession();
        CloseEntry();
        _session = started.Session;
        _audio = candidateAudio;
        var view = new BattleSessionView { Name = "BattleSessionView", WorldViewport = _worldViewport };
        _battle = view; AddChild(view); view.ConfigureInput(_input!);
        if (_audio is not null)
        {
            view.Audio = _audio;
            view.ObserveResult = result => { _audio.Observe(result); CheckAudio(); };
            view.PlayUiSound = command => { _audio.PlayEffect(command); CheckAudio(); };
        }
        view.LeaveBattle = result => ShowExploration(result, returning: true);
        if (SessionPresentationOwner.IsBattle(started.Result.Snapshot)) EnterBattle(started.Result);
        else { view.Hide(); ShowExploration(started.Result, returning: false); }
        // A bounded New Game confirmation, available only at settled field input.
        _restart = new Button { Name = "NewGameEntry", Text = "New Game…", AnchorLeft = 1, AnchorRight = 1,
            OffsetLeft = -220, OffsetRight = -20, OffsetTop = 12, OffsetBottom = 60,
            Theme = UI.StoryTheme.Create(), FocusMode = Control.FocusModeEnum.None };
        _restart.Pressed += ShowEntry; AddChild(_restart);
        DisarmInput();

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
            var exploration = new ExplorationSessionView { Name = "ExplorationSessionView", WorldViewport = _worldViewport };
            _exploration = exploration; AddChild(exploration);
            if (returning) { view.Reparent(exploration, false); exploration.MoveChild(view, 0); }
            exploration.Begin(started.Session, initial, _input!, _audio!, EnterBattle,
                result => { view.Reparent(exploration, false); view.Attach(started.Session, result); },
                returning ? () => { view.Reparent(this, false); view.Hide(); } : null);
        }
    }

    private void Fail(string code, string message) => Fail(new(SessionFailureKind.ContentError, code, "startup", message));
    private void Fail(SessionFailure failure)
    {
        ShowEntry(); _entry!.Fail(failure, _input is not null);
    }
}
