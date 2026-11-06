using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Battles;

// Battle Search owns its result wait; the host only delivers localized feedback.
internal sealed partial class BattleSearchView : Control
{
    private Label _text = null!;
    private readonly StoryDialoguePanel _panel = new() { Name = "ResultPanel" };
    private GameSession _session = null!;
    private InputSettings _settings = null!;
    private BattleSearchState? _state;
    private double _characters;
    private bool _delivered;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        var veil = new ColorRect { Color = new(0, 0, 0, 0.85f), MouseFilter = MouseFilterEnum.Ignore };
        AddChild(veil); veil.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(_panel); _panel.Choices.Hide(); _text = _panel.Body;
        Hide();
    }

    internal void Configure(GameSession session, InputSettings settings) { _session = session; _settings = settings; }
    internal void Present()
    {
        var state = _session.Current.BattleSearch;
        if (state is null) { _state = null; Hide(); return; }
        if (_state?.Token != state.Token)
        {
            _characters = 0; _delivered = false;
            _panel.Present(false, false, false, default, "", lifetimeOpen: false);
            _text.Text = ResultText(state);
            _text.VisibleCharacters = _settings.TextMode == "instant" ? -1 : 0;
        }
        _state = state; Show();
        _panel.Present(true, false, false, default, GameText.Get(_panel.MoreBelow ? "battle.result-more" : "battle.result-continue"));
    }

    internal SessionCommand? Consume(double delta)
    {
        if (_state is not { Revealed: false } state || _delivered || !IsVisibleInTree() || !CanProcess()) return null;
        if (_text.VisibleCharacters != -1)
        {
            _characters += delta * _settings.CharactersPerSecond;
            _text.VisibleCharacters = Math.Min(_text.Text.Length, (int)_characters);
            if (_text.VisibleCharacters < _text.Text.Length) return null;
        }
        _delivered = true;
        return new CompleteTextReveal(state.Token);
    }

    internal SessionCommand? Acknowledge(GameAction action)
    {
        if (_state is not { } state || action != GameAction.Confirm || !IsVisibleInTree() || !CanProcess()) return null;
        if (!state.Revealed) { _text.VisibleCharacters = -1; return null; }
        if (_panel.ReadMore())
        { _panel.SetHint(GameText.Get(_panel.MoreBelow ? "battle.result-more" : "battle.result-continue")); return null; }
        return new Acknowledge(state.Token);
    }

    private string ResultText(BattleSearchState state)
    {
        string actor = DisplayNames.Battle(_session.Current.Story, _session.Current.Battle.GetActor(state.Actor), _session.ActiveBattleScenes);
        string item = DisplayNames.Item(state.Chest.ItemId, state.Chest.ItemName ?? "");
        return state.Kind switch
        {
            Sf2.Remake.Domain.Battles.BattleSearchKind.Item => GameText.Get("battle.search-item", actor, item),
            Sf2.Remake.Domain.Battles.BattleSearchKind.Gold => GameText.Get("battle.search-gold", actor, state.Chest.Gold!.Value),
            Sf2.Remake.Domain.Battles.BattleSearchKind.Full => GameText.Get("battle.search-full", actor, item),
            Sf2.Remake.Domain.Battles.BattleSearchKind.Occupied => GameText.Get("battle.search-occupied",
                DisplayNames.Battle(_session.Current.Story, _session.Current.Battle.GetActor(state.Occupant!.Value), _session.ActiveBattleScenes)),
            _ => GameText.Get("battle.search-empty", actor),
        };
    }

    internal object Observe() => new { active = _state is not null, token = _state?.Token.Value,
        kind = _state?.Kind.ToString(), flag = _state?.Chest.Flag, endsTurn = _state?.EndsTurn, revealed = _state?.Revealed, text = _text.Text, visible = IsVisibleInTree(), visibleCharacters = _text.VisibleCharacters,
        panel = _panel.Observe() };

}
