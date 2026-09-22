using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Exploration;
using Godot;
using Sf2.Remake.GodotAdapter.Audio;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter;

// Ordinary game composition only. Reference play has its own project and entry scene.
public sealed partial class GameRoot : Node
{
    private GameInput? _input;
    private BattleSessionView? _battle;
    private ExplorationSessionView? _exploration;
    private SessionAudio? _audio;

    public override void _ExitTree() => _audio?.Dispose();

    public string ReadAudioObservationJson() => System.Text.Json.JsonSerializer.Serialize(_audio?.ObservePlayback());

    public override void _Input(InputEvent input)
    {
        if (_input?.Resolve(input) is not { } action) return;
        // Resolve once, even for releases and inactive modes. A transition cannot dispatch
        // this same event to the newly attached view as another command.
        if (_exploration is { } exploration && GodotObject.IsInstanceValid(exploration) && exploration.IsVisibleInTree())
            exploration.HandleAction(action);
        else if (_battle?.IsVisibleInTree() == true) _battle.HandleAction(action);
        GetViewport().SetInputAsHandled();
    }

    public override void _Ready()
    {
        var view = new BattleSessionView { Name = "BattleSessionView" };
        _battle = view;
        AddChild(view);
        var options = new Dictionary<string, string>(StringComparer.Ordinal);
        var arguments = OS.GetCmdlineUserArgs();
        for (int index = 0; index < arguments.Length; index++)
        {
            string option = arguments[index];
            if (option is not ("--authored-package" or "--private-battle-start" or "--private-exploration-start" or "--input-settings"))
            { Fail("unknown-startup-option", "Unknown game option. Select a session entry and optional input settings."); return; }
            if (options.ContainsKey(option))
            { Fail("duplicate-startup-option", "Each game option may appear only once."); return; }
            if (option != "--input-settings" && options.Keys.Any(key => key != "--input-settings"))
            { Fail("conflicting-startup-options", "Select one session entry."); return; }
            if (++index >= arguments.Length || string.IsNullOrWhiteSpace(arguments[index]) ||
                arguments[index].StartsWith("--", StringComparison.Ordinal))
            { Fail("missing-startup-path", "The selected game option requires a path."); return; }
            options.Add(option, arguments[index]);
        }
        try
        {
            _input = new GameInput(InputSettings.Load(options.GetValueOrDefault("--input-settings")));
            view.ConfigureInput(_input);
        }
        catch (Exception error) when (error is System.IO.IOException or UnauthorizedAccessException or
            System.Text.Json.JsonException or ArgumentException or NotSupportedException)
        {
            // File exceptions may contain private paths; report a bounded startup diagnostic.
            Fail("invalid-input-settings", error is ArgumentException ? error.Message : "Cannot read the versioned input settings.");
            return;
        }
        if (options.TryGetValue("--private-battle-start", out string? controlledStart))
            BeginSource(PrivateBattle(controlledStart));
        else if (options.TryGetValue("--private-exploration-start", out string? explorationStart))
            BeginSource(new PrivateExplorationReader(
                System.Environment.GetEnvironmentVariable("SF2_PRIVATE_EXPLORATION_CONTENT") ?? "", explorationStart,
                PrivateBattle(System.Environment.GetEnvironmentVariable("SF2_PRIVATE_CONTROLLED_START") ?? "")));
        else
        {
            string package = options.GetValueOrDefault("--authored-package") ??
                System.IO.Path.Combine(ProjectSettings.GlobalizePath("res://"), "..", "content", "authored", "practice-yard.json");
            BeginSource(new AuthoredScenarioPackageReader(package));
        }

        PrivateBattleScenarioReader PrivateBattle(string selectedStart) => new(
            System.Environment.GetEnvironmentVariable("SF2_PRIVATE_BATTLE01_DATA") ?? "",
            System.Environment.GetEnvironmentVariable("SF2_PRIVATE_BATTLE01_SCENE") ?? "",
            System.Environment.GetEnvironmentVariable("SF2_PRIVATE_BATTLE01_TERRAIN") ?? "",
            System.Environment.GetEnvironmentVariable("SF2_PRIVATE_STATIC_DATA") ?? "",
            System.Environment.GetEnvironmentVariable("SF2_PRIVATE_ENEMY_DATA") ?? "",
            System.Environment.GetEnvironmentVariable("SF2_PRIVATE_ENEMY_GOLD") ?? "", selectedStart);

        void BeginSource(IScenarioSource source)
        {
            var outcome = GameSession.Start(source);
            if (outcome is SessionStartFailed failed) { view.FailStartup(failed.Failure); return; }
            var started = (SessionStarted)outcome;
            if (started.Session.Definition.Exploration is { } definition)
            {
                _audio = new SessionAudio(this, definition);
                if (_audio.Error is { } error) { Fail(error, "Selected private audio content is unavailable."); return; }
                view.ObserveResult = result =>
                {
                    _audio.Observe(result);
                    if (_audio.Error is { } failure) Fail(failure, "Required private audio could not be consumed.");
                };
            }
            if (started.Result.Snapshot.HasBattleControl) { view.Attach(started.Session, started.Result); return; }
            view.Hide();
            ShowExploration(started.Result, returning: false);
            view.LeaveBattle = result => ShowExploration(result, returning: true);

            void ShowExploration(SessionResult initial, bool returning)
            {
                var exploration = new ExplorationSessionView { Name = "ExplorationSessionView" };
                _exploration = exploration;
                AddChild(exploration);
                if (returning)
                {
                    view.Reparent(exploration, false);
                    exploration.MoveChild(view, 0);
                }
                exploration.Begin(started.Session, initial, _input!, _audio!, result =>
                {
                    _exploration = null;
                    if (view.GetParent() == exploration) view.Reparent(this, false);
                    exploration.QueueFree();
                    view.Attach(started.Session, result);
                }, result =>
                {
                    view.Reparent(exploration, false);
                    view.Attach(started.Session, result);
                }, returning ? () => { view.Reparent(this, false); view.Hide(); } : null);
            }
        }

        void Fail(string code, string message) =>
            view.FailStartup(new(SessionFailureKind.ContentError, code, "startup", message));
    }
}
