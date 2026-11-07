using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter;

// A bounded independent observation of the actual menu, without a battlefield renderer.
// Declared startup loadouts are admitted by the existing private reader; no active setters.
public sealed partial class IdentityIconsObservation : Control
{
    private GameSession _session = null!;
    private GameInput _input = null!;
    private readonly BattleCommandMenu _menu = new() { Name = "CommandMenu" };
    private SessionFailure? _failure;
    public string Begin(string controlledStart, string settings)
    {
        string E(string key) => System.Environment.GetEnvironmentVariable("SF2_PRIVATE_" + key)!;
        var source = new PrivateBattleScenarioReader(E("BATTLE01_DATA"), E("BATTLE01_SCENE"), E("BATTLE01_TERRAIN"),
            E("STATIC_DATA"), E("ENEMY_DATA"), E("ENEMY_GOLD"), controlledStart, E("BATTLE_SCENE_CONTENT"));
        var outcome = GameSession.Start(source, RuleCompositions.ForGame());
        if (outcome is SessionStartFailed failed) return failed.Failure.Code;
        _session = ((SessionStarted)outcome).Session;
        _input = new(InputSettings.Load(settings));
        GameText.Initialize(_input.Settings.Language);
        GameText.Bind(_session.Definition, System.Environment.GetEnvironmentVariable("SF2_DISPLAY_CATALOG"));
        AddChild(_menu); _menu.Arrange(new(1920, 1080));
        _menu.ConfigureAssets(_session.ActiveBattleScenes, _session.Definition);
        _menu.Submit = Submit; Present();
        return _session.Current.Selection?.Stage.ToString() ?? "no-player-selection";
    }
    private void Submit(SessionCommand command)
    {
        var current = _session.Current;
        _failure = _session.Submit(new(current.SessionId, current.Revision, current.Selection?.Actor, command)).Failure;
        Present();
    }
    private void Present() => _menu.Present(_session.Current, _session.QueryBattleChoices(), _failure, _input, null);
    public override void _Input(InputEvent input)
    {
        if (_input.Resolve(input) is not { } action) return;
        if (action == GameAction.Confirm && _session.Current.Selection?.Stage == BattleSelectionStage.Movement)
            Submit(new Confirm());
        else _menu.Handle(action);
    }
    public string ReadIdentityIconPixelsObservationJson() => ReadPixels(this);
    internal static string ReadPixels(Node root) => JsonSerializer.Serialize(root.FindChildren("*", "Control", true, false)
        .OfType<UI.IdentityIcon>().Where(icon => icon.IsVisibleInTree())
        .Select(icon => new { art = icon.Observe(), pixelsSha256 = icon.PixelDigest() }));
    public string ReadObservationJson() => JsonSerializer.Serialize(new
    {
        revision = _session.Current.Revision, stage = _session.Current.Selection?.Stage.ToString(), failure = _failure,
        actor = _session.Current.Selection?.Actor.Value,
        actors = _session.Current.Battle.Actors.Select(actor => new { id = actor.Actor.Value,
            items = actor.SourceLoadout?.Items, spells = actor.SourceLoadout?.Spells }), menu = _menu.Observe(),
    });
}




public sealed partial class GameRoot
{
    // Explicit acceptance only. Ordinary state observations never read texture pixels.
    public string ReadIdentityIconPixelsObservationJson() => IdentityIconsObservation.ReadPixels(this);
}
