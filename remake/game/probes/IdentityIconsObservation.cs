using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter;

// An isolated texture witness. The ordinary host covers actual battle navigation.
public sealed partial class IdentityIconsObservation : Control
{
    private GameSession _session = null!;
    private GameInput _input = null!;
    private readonly BattleCommandMenu _menu = new() { Name = "CommandMenu" };
    private BattleDecision? _decision;

    public string Begin(string controlledStart, string settings)
    {
        string E(string key) => System.Environment.GetEnvironmentVariable("SF2_PRIVATE_" + key)!;
        var source = new PrivateBattleScenarioReader(E("BATTLE01_DATA"), E("BATTLE01_SCENE"), E("BATTLE01_TERRAIN"),
            E("STATIC_DATA"), E("ENEMY_DATA"), E("ENEMY_GOLD"), controlledStart, E("BATTLE_SCENE_CONTENT"));
        var outcome = GameSession.Start(source, RuleCompositions.ForGame());
        if (outcome is SessionStartFailed failed) return failed.Failure.Code;
        _session = ((SessionStarted)outcome).Session;
        var current = _session.Current;
        var result = _session.Submit(new(current.SessionId, current.Revision, current.Selection?.Actor, new Confirm()));
        if (result.Failure is { } failure) return failure.Code;
        _input = new(InputSettings.Load(settings));
        GameText.Initialize(_input.Settings.Language);
        GameText.Bind(_session.Definition, System.Environment.GetEnvironmentVariable("SF2_DISPLAY_CATALOG"));
        AddChild(_menu);
        _menu.Arrange(new(1920, 1080));
        _menu.ConfigureAssets(_session.ActiveBattleScenes, _session.Definition, _input.Settings);
        return _session.Current.Selection?.Stage.ToString() ?? "no-player-selection";
    }

    public void PresentChoices(bool items, int row)
    {
        var acting = _session.QueryBattleActingStatus();
        _decision = new BattleDecision(_session.Current, _session.QueryBattleChoices(), acting is not null,
            items ? BattleCommandRow.Item : BattleCommandRow.Magic, listOpen: true, row, attemptedTarget: null);
        _menu.Present(_decision, null, _input, acting);
    }

    public string ReadIdentityIconPixelsObservationJson() => ReadPixels(this);
    internal static string ReadPixels(Node root) => JsonSerializer.Serialize(root.FindChildren("*", "Control", true, false)
        .OfType<UI.IdentityIcon>().Where(icon => icon.IsVisibleInTree())
        .Select(icon => new { art = icon.Observe(), pixelsSha256 = icon.PixelDigest() }));

    public string ReadObservationJson() => JsonSerializer.Serialize(new
    {
        revision = _session.Current.Revision, stage = _session.Current.Selection?.Stage.ToString(),
        actor = _session.Current.Selection?.Actor.Value,
        actors = _session.Current.Battle.Actors.Select(actor => new { id = actor.Actor.Value,
            items = actor.SourceLoadout?.Items, spells = actor.SourceLoadout?.Spells }), menu = _menu.Observe(_decision),
    });
}

public sealed partial class GameRoot
{
    // Explicit acceptance only. Ordinary state observations never read texture pixels.
    public string ReadIdentityIconPixelsObservationJson() => IdentityIconsObservation.ReadPixels(this);
}
