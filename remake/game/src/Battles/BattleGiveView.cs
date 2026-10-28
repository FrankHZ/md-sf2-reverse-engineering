using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Battles;

// Authored result text; source Give has no battle scene or result-message timing claim.
internal sealed partial class BattleGiveView : Control
{
    private Label _text = null!;
    private GameSession _session = null!;
    private InputSettings _settings = null!;
    private BattleGiveState? _state;
    private double _characters;
    private bool _delivered;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        var veil = new ColorRect { Color = new(0, 0, 0, 0.85f), MouseFilter = MouseFilterEnum.Ignore };
        AddChild(veil); veil.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _text = new Label { Name = "GiveResult", HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = MouseFilterEnum.Ignore };
        AddChild(_text); _text.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        Hide();
    }

    internal void Configure(GameSession session, InputSettings settings) { _session = session; _settings = settings; }
    internal void Present()
    {
        var state = _session.Current.BattleGive;
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

    internal static bool Selecting(BattleSelectionStage? stage) => stage is BattleSelectionStage.GiveItemChoice or
        BattleSelectionStage.GiveTargetChoice or BattleSelectionStage.GiveExchangeChoice or BattleSelectionStage.GiveCommitReady;

    internal static SessionCommand? Select(GameSession session, GameAction action)
    {
        if (action == GameAction.Confirm) return new Confirm();
        if (action == GameAction.Cancel) return new Cancel();
        if (action is not (GameAction.Up or GameAction.Down or GameAction.Left or GameAction.Right or GameAction.Target or GameAction.Give)) return null;
        var selection = session.Current.Selection!;
        var choices = session.QueryBattleChoices().Give;
        int step = action is GameAction.Up or GameAction.Left ? -1 : 1;
        if (selection.Stage == BattleSelectionStage.GiveTargetChoice)
        {
            if (choices.Targets.Count == 0) return null;
            int index = choices.Targets.ToList().IndexOf(selection.Target ?? default);
            return new SelectTarget(choices.Targets[(index + step + choices.Targets.Count) % choices.Targets.Count]);
        }
        bool exchange = selection.Stage == BattleSelectionStage.GiveExchangeChoice;
        if (!exchange && selection.Stage != BattleSelectionStage.GiveItemChoice) return null;
        var items = exchange ? choices.Returns : choices.Items;
        if (items.Count == 0) return null;
        int selected = items.ToList().FindIndex(item => item.Slot == (exchange ? selection.ReturnSlot : selection.ItemSlot));
        int slot = items[(selected + step + items.Count) % items.Count].Slot;
        return exchange ? new SelectGiveReturn(slot) : new SelectGiveItem(slot);
    }

    internal static string Menu(GameSession session)
    {
        var selection = session.Current.Selection;
        if (!Selecting(selection?.Stage)) return "";
        var choices = session.QueryBattleChoices().Give;
        string given = choices.Items.FirstOrDefault(item => item.Slot == selection!.ItemSlot)?.Name ?? "item";
        if (selection!.Stage == BattleSelectionStage.GiveTargetChoice)
            return $"Give {given}: choose an adjacent teammate\n" + string.Join("\n", choices.Targets.Select(actor =>
                (actor == selection.Target ? "> " : "  ") + actor.Value)) + "\nDirections / Target cycle; Confirm selects; Cancel returns.";
        if (selection.Stage == BattleSelectionStage.GiveCommitReady)
            return $"Give {given} to {selection.Target?.Value}" + (selection.ReturnSlot is null ? "" :
                $" for {choices.Returns.FirstOrDefault(item => item.Slot == selection.ReturnSlot)?.Name}") +
                "?\nConfirm transfers and ends this action; Cancel returns.";
        bool exchange = selection.Stage == BattleSelectionStage.GiveExchangeChoice;
        return (exchange ? $"{selection.Target?.Value}'s hands are full. Choose an item to return:" : "Choose an item to give:") +
            "\n" + string.Join("\n", (exchange ? choices.Returns : choices.Items).Select(item =>
                $"{(item.Slot == (exchange ? selection.ReturnSlot : selection.ItemSlot) ? "> " : "  ")}slot {item.Slot + 1}: {item.Name}")) +
            "\nDirections cycle; Confirm selects; Cancel returns.";
    }
}
