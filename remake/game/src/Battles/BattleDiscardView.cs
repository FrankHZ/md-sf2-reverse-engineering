using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Battles;

// Authored result delivery; no original text/timing claim.
internal sealed partial class BattleDiscardView : Control
{
    private Label _text = null!;
    private GameSession _session = null!;
    private InputSettings _settings = null!;
    private BattleDiscardState? _state;
    private double _characters;
    private bool _delivered;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        var veil = new ColorRect { Color = new(0, 0, 0, 0.85f), MouseFilter = MouseFilterEnum.Ignore };
        AddChild(veil); veil.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _text = new Label { Name = "DiscardResult", HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = MouseFilterEnum.Ignore };
        AddChild(_text); _text.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        Hide();
    }

    internal void Configure(GameSession session, InputSettings settings) { _session = session; _settings = settings; }
    internal void Present()
    {
        var state = _session.Current.BattleDiscard;
        if (state is null) { _state = null; Hide(); return; }
        if (_state?.Token != state.Token)
        {
            _characters = 0; _delivered = false;
            _text.Text = state.Text + "\nConfirm to continue.";
            _text.VisibleCharacters = _settings.TextMode == "instant" ? -1 : 0;
        }
        _state = state; Show();
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
        return new Acknowledge(state.Token);
    }

    internal object Observe() => new { active = _state is not null, token = _state?.Token.Value,
        revealed = _state?.Revealed, text = _text.Text, visible = IsVisibleInTree(), visibleCharacters = _text.VisibleCharacters };

    internal static bool Selecting(BattleSelectionStage? stage) => stage is BattleSelectionStage.DiscardItemChoice or
        BattleSelectionStage.DiscardCommitReady;

    internal static SessionCommand? Select(GameSession session, GameAction action)
    {
        if (action == GameAction.Confirm) return new Confirm();
        if (action == GameAction.Cancel) return new Cancel();
        if (session.Current.Selection is not { Stage: BattleSelectionStage.DiscardItemChoice } selection ||
            action is not (GameAction.Up or GameAction.Down or GameAction.Left or GameAction.Right or GameAction.Discard)) return null;
        var items = session.QueryBattleChoices().Discard.Items;
        if (items.Count == 0) return null;
        int index = items.ToList().FindIndex(item => item.Slot == selection.ItemSlot);
        int step = action is GameAction.Up or GameAction.Left ? -1 : 1;
        return new SelectDiscardItem(items[(index + step + items.Count) % items.Count].Slot);
    }

    internal static string Menu(GameSession session)
    {
        var selection = session.Current.Selection;
        if (!Selecting(selection?.Stage) || session.Current.BattleDiscard is not null) return "";
        var items = session.QueryBattleChoices().Discard.Items;
        if (selection!.Stage == BattleSelectionStage.DiscardCommitReady)
            return $"Discard {items.FirstOrDefault(item => item.Slot == selection.ItemSlot)?.Item.Name}?\n" +
                "Confirm discards; Cancel keeps it. Your action and movement preview remain available.";
        return "Choose an item to discard:\n" + string.Join("\n", items.Select(item =>
            $"{(item.Slot == selection.ItemSlot ? "> " : "  ")}slot {item.Slot + 1}: {item.Item.Name}" +
            ((item.Word & 128) != 0 ? " · equipped" : "") +
            (item.Item.Unsellable ? " · protected: cannot discard" : item.Item.Rare ? " · rare: enters Deals" : ""))) +
            "\nDirections cycle; Confirm selects; Cancel returns.";
    }
}
