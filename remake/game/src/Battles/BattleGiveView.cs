using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Battles;

// Authored result text; source Give has no battle scene or result-message timing claim.
internal sealed partial class BattleGiveView : Control
{
    private Label _text = null!;
    private readonly StoryDialoguePanel _panel = new() { Name = "ResultPanel" };
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
        AddChild(_panel); _panel.Choices.Hide(); _text = _panel.Body;
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
            _panel.Present(false, false, false, default, "", lifetimeOpen: false);
            string Name(Sf2.Remake.Domain.Battles.ActorRef id) => DisplayNames.Battle(_session.Current.Story,
                _session.Current.Battle.GetActor(id), _session.ActiveBattleScenes);
            _text.Text = state.Returned is null
                ? GameText.Get("battle.give-result", Name(state.Giver), DisplayNames.Item(state.GivenItemId, state.Item), Name(state.Recipient))
                : GameText.Get("battle.exchange-result", Name(state.Giver), DisplayNames.Item(state.GivenItemId, state.Item), Name(state.Recipient), DisplayNames.Item(state.ReturnedItemId, state.Returned));
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

    internal object Observe() => new { active = _state is not null, token = _state?.Token.Value,
        revealed = _state?.Revealed, text = _text.Text, visible = IsVisibleInTree(), visibleCharacters = _text.VisibleCharacters,
        panel = _panel.Observe() };

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
        string Name(Sf2.Remake.Domain.Battles.ActorRef? id) => id is { } actor
            ? DisplayNames.Battle(session.Current.Story, session.Current.Battle.GetActor(actor), session.ActiveBattleScenes) : "";
        var selection = session.Current.Selection;
        if (!Selecting(selection?.Stage)) return "";
        var choices = session.QueryBattleChoices().Give;
        var givenItem = choices.Items.FirstOrDefault(item => item.Slot == selection!.ItemSlot);
        string given = givenItem is null ? GameText.Get("battle.give-item") : DisplayNames.ItemWord(givenItem.Word, givenItem.Name);
        if (selection!.Stage == BattleSelectionStage.GiveTargetChoice)
            return GameText.Get("battle.give-target", given,
                choices.Targets.ToList().IndexOf(selection.Target ?? default) + 1, choices.Targets.Count, Name(selection.Target));
        if (selection.Stage == BattleSelectionStage.GiveCommitReady)
            return selection.ReturnSlot is null ? GameText.Get("battle.give-confirm", given, Name(selection.Target)) :
                GameText.Get("battle.exchange-confirm", given, Name(selection.Target),
                    choices.Returns.FirstOrDefault(item => item.Slot == selection.ReturnSlot) is { } returned ? DisplayNames.ItemWord(returned.Word, returned.Name) : GameText.Get("battle.give-item"));
        bool exchange = selection.Stage == BattleSelectionStage.GiveExchangeChoice;
        var items = exchange ? choices.Returns : choices.Items;
        int index = items.ToList().FindIndex(item => item.Slot == (exchange ? selection.ReturnSlot : selection.ItemSlot));
        return (exchange ? GameText.Get("battle.exchange-title", Name(selection.Target)) : GameText.Get("battle.give-title")) + "\n" +
            (index < 0 ? GameText.Get("battle.transaction-empty") :
                GameText.Get("battle.transaction-item", index + 1, items.Count, items[index].Slot + 1, DisplayNames.ItemWord(items[index].Word, items[index].Name)));
    }
}
