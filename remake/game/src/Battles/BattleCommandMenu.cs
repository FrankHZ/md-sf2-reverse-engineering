using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Battles;

// Only menu focus lives here. The session remains authoritative for actions, targets and legality.
internal sealed partial class BattleCommandMenu : Control
{
    private enum Page { Main, Magic, Items, Target, Commit }
    private static readonly BattleActionKind[] Kinds = [BattleActionKind.Physical, BattleActionKind.Healing, BattleActionKind.Item, BattleActionKind.Stay];
    private readonly BattleMenuLayout _layout = new() { Name = "Layout" };
    private Page _page;
    private int _command, _row;
    private ActorRef? _actor;
    private SessionSnapshot? _current;
    private BattleChoicesSnapshot? _choices;
    private GameInput _input = null!;
    private SessionFailure? _failure;
    internal Action<SessionCommand> Submit { get; set; } = null!;
    internal ActorRef? TargetCandidate { get; private set; }
    internal Rect2 MapRect => _layout.MapRect;
    internal Action? LayoutChanged { get => _layout.LayoutChanged; set => _layout.LayoutChanged = value; }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore; AddChild(_layout);
    }
    internal void Arrange(Vector2 size) { Size = size; _layout.Arrange(size); }
    internal void ConfigureAssets(BattleSceneDefinition? content) => _layout.ConfigureAssets(content);
    internal void Reset() { _actor = null; _page = Page.Main; _row = _command = 0; TargetCandidate = null; }

    internal void Present(SessionSnapshot current, BattleChoicesSnapshot choices, SessionFailure? failure, GameInput input)
    {
        _current = current; _choices = choices; _failure = failure; _input = input;
        var selection = current.Selection;
        if (_actor != selection?.Actor || selection?.Stage == BattleSelectionStage.Movement || selection is null)
        {
            _page = Page.Main; _command = _row = 0; TargetCandidate = null;
        }
        _actor = selection?.Actor;
        Visible = current.BattleScene is null && current.BattleRetreat is null && current.BattleGive is null &&
            current.BattleDiscard is null && current.BattleRound is null && selection?.Stage is
                BattleSelectionStage.Movement or BattleSelectionStage.ActionChoice or BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady;
        if (selection?.Stage is BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady)
            _page = selection.ActionReference?.Kind == BattleActionKind.Stay ? Page.Commit : Page.Target;
        Refresh();
    }

    internal bool Handle(GameAction action)
    {
        if (!Visible || _current?.Selection is not { } selection || _choices is null || selection.Stage == BattleSelectionStage.Movement) return false;
        if (_page == Page.Main && action is GameAction.Equipment or GameAction.Give or GameAction.Discard) return false;
        if (action == GameAction.Cancel)
        {
            if (_page is Page.Target or Page.Commit)
            {
                var previous = selection.ActionReference!.Value;
                Submit(new ReturnToBattleActions());
                if (_current!.Selection?.Stage != BattleSelectionStage.ActionChoice) return true;
                TargetCandidate = null;
                _page = previous.ItemSlot is not null ? Page.Items : previous.Spell is not null ? Page.Magic : Page.Main;
                _row = Mathf.Clamp(_row, 0, Math.Max(0, Options().Length - 1));
            }
            else if (_page != Page.Main) { _page = Page.Main; _row = 0; }
            else { Submit(new Cancel()); return true; }
            Refresh(); return true;
        }
        if (_page == Page.Main)
        {
            int next = action switch
            {
                GameAction.Up or GameAction.Attack => 0, GameAction.Left or GameAction.Spell => 1,
                GameAction.Right or GameAction.Item => 2, GameAction.Down or GameAction.Stay => 3, _ => -1,
            };
            if (next >= 0) _command = next;
            if (action is GameAction.Confirm or GameAction.Attack or GameAction.Spell or GameAction.Item or GameAction.Stay) Open();
            Refresh(); return true;
        }
        if (_page is Page.Magic or Page.Items)
        {
            var options = Options();
            int delta = action == GameAction.Up ? -1 : action == GameAction.Down ? 1 : 0;
            if (options.Length > 0) _row = (_row + delta + options.Length) % options.Length;
            if (action == GameAction.Confirm && options.Length > 0) Select(options[_row]);
            Refresh(); return true;
        }
        if (_page == Page.Target)
        {
            var option = SelectedAction();
            if (option is null) return true;
            var targets = option.Targets;
            if (action is GameAction.Target or GameAction.Up or GameAction.Down or GameAction.Left or GameAction.Right)
            {
                int index = targets.ToList().FindIndex(target => target.Actor == (TargetCandidate ?? selection.Target));
                int delta = action is GameAction.Up or GameAction.Left ? -1 : 1;
                if (targets.Count > 0) PickTarget(targets[(index + delta + targets.Count) % targets.Count]);
            }
            else if (action == GameAction.Confirm && targets.Any(target => target.Actor == TargetCandidate && target.Enabled)
                && _current.Selection?.Target == TargetCandidate) Submit(new Confirm());
            Refresh(); return true;
        }
        if (action == GameAction.Confirm) Submit(new Confirm());
        Refresh(); return true;
    }

    private static bool InGroup(BattleActionChoice option, int group) => group switch
    {
        1 => option.Action.Spell is not null, 2 => option.Action.ItemSlot is not null,
        _ => option.Action.Kind == Kinds[group],
    };
    private BattleActionChoice[] Options() => _choices!.Actions.Where(option => InGroup(option, _command)).ToArray();
    private BattleActionChoice? SelectedAction() => _choices!.Actions.FirstOrDefault(option => option.Action == _current!.Selection?.ActionReference);

    private void Open()
    {
        _row = 0;
        if (_command is 1 or 2) { _page = _command == 1 ? Page.Magic : Page.Items; return; }
        if (Options().FirstOrDefault() is { } option) Select(option);
    }

    private void Select(BattleActionChoice option)
    {
        if (!option.Enabled) return;
        Submit(new SelectBattleAction(option.Action));
        if (_failure is not null || _current!.Selection?.ActionReference != option.Action) return;
        TargetCandidate = null;
        if (option.Action.Kind == BattleActionKind.Stay) { _page = Page.Commit; return; }
        _page = Page.Target;
        // Prefer self when the engine offers it; otherwise use the first admitted target.
        var target = option.Targets.FirstOrDefault(target => target.Actor == _actor && target.Enabled)
            ?? option.Targets.FirstOrDefault(target => target.Enabled);
        if (target is not null) PickTarget(target);
    }

    private void PickTarget(BattleTargetChoice target)
    {
        TargetCandidate = target.Actor;
        if (target.Enabled) Submit(new SelectTarget(target.Actor));
    }

    private void Refresh()
    {
        if (_current is null || _choices is null) return;
        var selection = _current.Selection;
        bool moving = selection?.Stage == BattleSelectionStage.Movement;
        _layout.Actor.Text = _actor is { } actor ? ActorText(actor) : "Battle";
        _layout.Context.Text = $"Round {_current.Battle.Round}    Gold {_current.Battle.Gold?.ToString() ?? "—"}";
        for (int i = 0; i < Kinds.Length; i++)
        {
            _layout.Commands[i].Selected = !moving && i == _command;
            _layout.Commands[i].Available = _choices.Actions.Any(option => InGroup(option, i) && option.Enabled);
            _layout.Commands[i].QueueRedraw();
        }
        bool list = !moving && _page is Page.Magic or Page.Items;
        var options = list ? Options() : [];
        _row = Mathf.Clamp(_row, 0, Math.Max(0, options.Length - 1));
        _layout.ShowRows(options.Select(OptionLabel).ToArray(), options.Select(option => option.Enabled).ToArray(), _row, list);
        _layout.Diamond.Visible &= selection is not null && !moving && _page == Page.Main;
        _layout.ShowCommands(list || _layout.Diamond.Visible);
        _layout.Heading.Text = moving ? "Choose a position" : _page switch
        {
            Page.Magic => "Magic", Page.Items => "Items", Page.Target => "Choose a target",
            Page.Commit => "Stay here?", _ => _layout.Commands[_command].Caption,
        };
        string confirm = KeyHint(GameAction.Confirm), cancel = KeyHint(GameAction.Cancel);
        string directions = string.Join("/", new[] { GameAction.Up, GameAction.Left, GameAction.Down, GameAction.Right }.Select(KeyHint));
        _layout.Help.Text = moving ? $"{directions}: move\n{confirm}: actions" :
            _page == Page.Target ? $"{KeyHint(GameAction.Target)}: next target\n{confirm}: commit · {cancel}: back" :
            $"{(list ? $"{KeyHint(GameAction.Up)}/{KeyHint(GameAction.Down)}: choice\n" : _page == Page.Main ? directions + ": command\n" : "")}{confirm}: confirm · {cancel}: back";
        if (!moving && _page == Page.Main)
            _layout.Help.Text += $"\n{KeyHint(GameAction.Equipment)}: equip · {KeyHint(GameAction.Give)}: give · {KeyHint(GameAction.Discard)}: drop";
        string description;
        if (moving) description = "Move to a tile, then open the action menu.";
        else if (_page == Page.Target && SelectedAction() is { } selected)
        {
            var target = selected.Targets.FirstOrDefault(target => target.Actor == TargetCandidate);
            description = $"{OptionLabel(selected)}\n{(target is null ? "No available target" : ActorText(target.Actor))}";
            if (target?.Reason is { } reason) description += "\n" + Unavailable(reason);
        }
        else if (_page == Page.Commit) description = "End this unit's action at the selected position.";
        else if (list) description = options.Length == 0 ? (_page == Page.Magic ? "No learned magic." : "No carried items.") : Describe(options[_row]);
        else
        {
            var offers = Options();
            description = _command switch { 0 => "Attack a target in reach.", 1 => "Choose a learned spell and level.", 2 => "Choose an item from this unit's inventory.", _ => "End this unit's action." };
            if (offers.Length == 0) description += _command == 1 ? "\nNo learned magic." : "\nNo carried items.";
            else if (!offers.Any(option => option.Enabled) && offers[0].Reason is { } reason) description += "\n" + Unavailable(reason);
        }
        if (_choices.Failure is { } queryFailure) description = Unavailable(queryFailure);
        if (_failure is not null) description += "\n" + Unavailable(_failure);
        _layout.Detail.Text = description;
    }

    private string ActorText(ActorRef actor)
    {
        var row = _current!.Battle.GetActor(actor);
        return $"{actor.Value}\nHP {row.Hp}/{row.MaxHp}    MP {row.Mp}/{row.MaxMp}";
    }
    private string OptionLabel(BattleActionChoice option)
    {
        string label = option.Action.ItemSlot is { } slot ? $"{slot + 1}. {option.Label}" : option.Label;
        if (option.Action.Spell is { } spell && _current!.Battle.Definition.Spells.TryGetValue(spell, out var definition)) label += $" · {definition.MpCost} MP";
        return label + (option.Enabled ? "" : " · unavailable");
    }
    private string Describe(BattleActionChoice option)
    {
        string text = OptionLabel(option);
        if (option.MinimumRange is { } minimum) text += $"\nRange {minimum}–{option.MaximumRange}";
        if (option.Reason is { } reason) text += "\n" + Unavailable(reason);
        return text;
    }
    private static string Unavailable(SessionFailure reason) => "Unavailable: " + (reason.Code switch
    {
        "target-range" => "Out of range.", "insufficient-mp" => "Not enough MP.",
        "empty-item-slot" => "This slot is empty.",
        "physical-definition" => "Attack is not supported for this unit.",
        _ => reason.Message,
    });
    private string KeyHint(GameAction action) => _input.Hint(action).Split(" / ", 2)[0];

    internal object Observe() => new { page = _page.ToString(), command = _command, row = _row,
        candidate = TargetCandidate?.Value, visible = Visible, layout = _layout.Observe() };
}
