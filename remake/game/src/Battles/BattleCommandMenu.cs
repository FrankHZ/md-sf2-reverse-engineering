using Godot;
using Sf2.Remake.GodotAdapter.Localization;
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
    private readonly MenuPointer _pointer = new();
    private Page _page;
    private int _command, _row;
    private ActorRef? _actor;
    private SessionSnapshot? _current;
    private BattleChoicesSnapshot? _choices;
    private BattleActingStatus? _actingStatus;
    private GameInput _input = null!;
    private SessionFailure? _failure;
    private BattleSceneDefinition? _content;
    internal Action<SessionCommand> Submit { get; set; } = null!;
    internal ActorRef? TargetCandidate { get; private set; }
    // TargetChoice and CommitReady share this navigation page; choosing a different
    // target must not create a new mode or restart the directional repeat clock.
    internal string NavigationPage => _page.ToString();

    public override void _Ready()
    {
        _layout.InvalidatePointer = _pointer.Cancel;
        MouseFilter = MouseFilterEnum.Ignore; AddChild(_layout);
    }
    internal void Arrange(Vector2 size) { Size = size; _layout.Arrange(size); }
    internal void ConfigureAssets(BattleSceneDefinition? content) { _content = content; _layout.ConfigureAssets(content); }
    internal void Reset() { _layout.StopFocusEmphasis(); _actor = null; _page = Page.Main; _row = _command = 0; TargetCandidate = null; }
    internal void SuspendPointer(bool keyboard = false)
    {
        if (keyboard) _pointer.Cancel();
        else { _pointer.Suspend(); _layout.StopFocusEmphasis(); }
    }
    public override void _Process(double delta) => _pointer.ReleaseIfNeutral();
    public override void _Notification(int what)
    {
        if (what == NotificationPaused || what == NotificationUnpaused || what == NotificationVisibilityChanged && !IsVisibleInTree())
            SuspendPointer();
    }
    public override void _ExitTree() => _layout.StopFocusEmphasis();

    internal bool HandlePointer(InputEvent input)
    {
        if (!IsVisibleInTree() || _current?.Selection is not { } selection ||
            selection.Stage == BattleSelectionStage.Movement) { _pointer.Cancel(); return false; }
        Vector2 position = ((InputEventMouse)input).Position;
        var hit = _layout.Hit(position);
        if (input is InputEventMouseMotion motion)
        {
            if (_pointer.Moved(motion) && hit.Id is not null) Focus(hit.Id);
            return true;
        }
        if (input is not InputEventMouseButton button) return true;
        if (button.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        {
            _pointer.Cancel();
            if (button.Pressed) _layout.Scroll(position, button.ButtonIndex == MouseButton.WheelUp ? -1 : 1);
            return true;
        }
        if (button.ButtonIndex == MouseButton.Right) hit = ("Back", GetGlobalRect());
        object boundary = (_current.SessionId, _current.Revision, _actor, _page);
        if (_pointer.Read(button, hit.Id, hit.Rect, boundary) is not { } activated) return true;
        Focus(activated);
        if (activated.StartsWith("command:", StringComparison.Ordinal)) { Open(); Refresh(); }
        else if (activated.StartsWith("row:", StringComparison.Ordinal)) { Select(Options()[_row]); Refresh(); }
        else if (activated == "PreviousTarget") Handle(GameAction.Left);
        else if (activated == "NextTarget") Handle(GameAction.Right);
        else if (activated == "Confirm") Handle(GameAction.Confirm);
        else if (activated == "Back") Handle(GameAction.Cancel);
        return true;
    }

    private void Focus(string id)
    {
        if (id.StartsWith("command:", StringComparison.Ordinal)) _command = int.Parse(id.AsSpan(8));
        else if (id.StartsWith("row:", StringComparison.Ordinal)) _row = int.Parse(id.AsSpan(4));
        else return;
        Refresh();
    }

    internal void Present(SessionSnapshot current, BattleChoicesSnapshot choices, SessionFailure? failure, GameInput input, BattleActingStatus? actingStatus)
    {
        var selection = current.Selection;
        bool enteringActions = selection?.Stage == BattleSelectionStage.ActionChoice &&
            (_actor != selection.Actor || _current?.SessionId != current.SessionId ||
                _current?.Selection?.Stage == BattleSelectionStage.Movement);
        if (_current?.BattleSearch is not null && current.BattleSearch is null) _page = Page.Main;
        _current = current; _actingStatus = actingStatus; _choices = choices; _failure = failure; _input = input;
        if (_actor != selection?.Actor || selection?.Stage == BattleSelectionStage.Movement || selection is null)
        {
            _page = Page.Main; _command = _row = 0; TargetCandidate = null;
        }
        // Fresh entry consumes the live offer at the confirmed stop. Child Back keeps the chosen focus.
        if (enteringActions)
            _command = choices.Failure is null && choices.Actions.Any(option =>
                option.Action.Kind == BattleActionKind.Physical && option.Enabled &&
                option.Targets.Any(target => target.Enabled)) ? 0 : 3;
        _actor = selection?.Actor;
        Visible = current.BattleScene is null && current.BattleRetreat is null && current.BattleGive is null &&
            current.BattleDiscard is null && current.BattleSearch is null && current.BattleRound is null && selection?.Stage is
                BattleSelectionStage.Movement or BattleSelectionStage.ActionChoice or BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady or BattleSelectionStage.SearchCommitReady;
        if (selection?.Stage is BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady)
            _page = selection.ActionReference?.Kind == BattleActionKind.Stay ? Page.Commit : Page.Target;
        if (selection?.Stage == BattleSelectionStage.SearchCommitReady) _page = Page.Commit;
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
                var previous = selection.ActionReference;
                Submit(new ReturnToBattleActions());
                if (_current!.Selection?.Stage != BattleSelectionStage.ActionChoice) return true;
                TargetCandidate = null;
                _page = previous?.ItemSlot is not null ? Page.Items : previous?.Spell is not null ? Page.Magic : Page.Main;
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
        if (_command == 3 && _choices!.Search is not null) { Submit(new BeginBattleSearch()); return; }
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
        _layout.Actor.PresentActing(_actingStatus);
        var candidate = !moving && _page == Page.Target && TargetCandidate is { } targetRef
            ? _current.Battle.GetActor(targetRef) : null;
        _layout.Target.Present(candidate, candidate is null ? "" : BattleCharacterNames.Resolve(candidate, _content, _current!.Story), GameText.Get("battle.target"));
        _layout.Context.Text = GameText.Get("battle.context", _current.Battle.Round, _current.Battle.Gold?.ToString() ?? "—");
        for (int i = 0; i < Kinds.Length; i++)
        {
            _layout.Commands[i].CaptionOverride = i == 3 && _choices.Search is not null ? "battle.command.search" : null;
            _layout.Commands[i].RefreshCaption();
            _layout.Commands[i].SetSelection(Visible && !moving && _page == Page.Main && i == _command, _input.Settings.ReducedFlash);
            _layout.Commands[i].Available = _choices.Actions.Any(option => InGroup(option, i) && option.Enabled);
        }
        bool list = !moving && _page is Page.Magic or Page.Items;
        var options = list ? Options() : [];
        _row = Mathf.Clamp(_row, 0, Math.Max(0, options.Length - 1));
        _layout.ShowRows(options.Select(OptionLabel).ToArray(), options.Select(option => option.Enabled).ToArray(), _row, list);
        _layout.Diamond.Visible &= selection is not null && !moving && _page == Page.Main;
        _layout.Heading.Text = moving ? GameText.Get("battle.position") : _page switch
        {
            Page.Magic => GameText.Get("battle.magic"), Page.Items => GameText.Get("battle.items"), Page.Target => GameText.Get("battle.choose-target"),
            Page.Commit => GameText.Get(selection?.Stage == BattleSelectionStage.SearchCommitReady ? "battle.search-confirm" : "battle.stay-here"), _ => _layout.Commands[_command].Caption,
        };
        string confirm = KeyHint(GameAction.Confirm), cancel = KeyHint(GameAction.Cancel);
        string directions = string.Join("/", new[] { GameAction.Up, GameAction.Left, GameAction.Down, GameAction.Right }.Select(KeyHint));
        _layout.Help.Text = moving ? GameText.Get("battle.move-help", directions, confirm) :
            _page == Page.Target ? GameText.Get("battle.target-help", KeyHint(GameAction.Target), confirm, cancel) :
            (list ? GameText.Get("battle.choice-help", $"{KeyHint(GameAction.Up)}/{KeyHint(GameAction.Down)}") :
                _page == Page.Main ? GameText.Get("battle.command-help", directions) : "") +
                GameText.Get("battle.confirm-help", confirm, cancel);
        if (!moving && _page == Page.Main)
            _layout.Help.Text += GameText.Get("battle.extra-help", KeyHint(GameAction.Equipment), KeyHint(GameAction.Give), KeyHint(GameAction.Discard));
        string description;
        if (moving) description = GameText.Get("battle.move-detail");
        else if (_page == Page.Target && SelectedAction() is { } selected)
        {
            var target = selected.Targets.FirstOrDefault(target => target.Actor == TargetCandidate);
            description = $"{OptionLabel(selected)}\n{(target is null ? GameText.Get("battle.no-target") : BattleCharacterNames.Resolve(_current.Battle.GetActor(target.Actor), _content, _current.Story))}";
            if (target?.Reason is { } reason) description += "\n" + Unavailable(reason);
        }
        else if (_page == Page.Commit) description = GameText.Get(selection?.Stage == BattleSelectionStage.SearchCommitReady ? "battle.search-detail" : "battle.stay-detail");
        else if (_page == Page.Main && _command == 3 && _choices.Search is not null) description = GameText.Get("battle.search-detail");
        else if (list) description = options.Length == 0 ? (_page == Page.Magic ? GameText.Get("battle.no-magic") : GameText.Get("battle.no-items")) : Describe(options[_row]);
        else
        {
            var offers = Options();
            description = offers.Length == 0 ? (_command == 1 ? GameText.Get("battle.no-magic") : GameText.Get("battle.no-items")) :
                !offers.Any(option => option.Enabled) && offers[0].Reason is { } reason ? Unavailable(reason) : "";
        }
        if (_choices.Failure is { } queryFailure) description = Unavailable(queryFailure);
        if (_failure is not null) description += "\n" + Unavailable(_failure);
        string fallback = GameText.Fallback("battle.", includeKey: true);
        _layout.Detail.Text = description + (fallback.Length == 0 ? "" : "\n" + fallback);
        bool canConfirm = _page == Page.Commit || _page == Page.Target && SelectedAction()?.Targets.Any(target =>
            target.Actor == TargetCandidate && target.Enabled && selection?.Target == TargetCandidate) == true;
        _layout.ShowNavigation(!moving, _page == Page.Target, _page == Page.Commit, canConfirm);
        _layout.ShowCommandHint(!moving && _page == Page.Main);
    }

    private string OptionLabel(BattleActionChoice option)
    {
        string name = DisplayNames.Action(option, _current!);
        string label = option.Action.ItemSlot is { } slot ? $"{slot + 1}. {name}" : name;
        if (option.Action.Spell is { } spell)
        {
            var definition = _current!.Battle.Definition;
            byte? cost = definition.Spells.TryGetValue(spell, out var healing) ? healing.MpCost :
                definition.AttackSpells.TryGetValue(spell, out var attack) ? attack.MpCost :
                definition.SupportSpells.TryGetValue(spell, out var support) ? support.MpCost :
                definition.RetreatSpells.TryGetValue(spell, out var retreat) ? retreat.MpCost : null;
            if (cost is { } mp) label += $" · {mp} MP";
        }
        return label + (option.Enabled ? "" : GameText.Get("battle.unavailable-tag"));
    }
    private string Describe(BattleActionChoice option)
    {
        string text = OptionLabel(option);
        if (option.MinimumRange is { } minimum) text += "\n" + GameText.Get("battle.range", minimum, option.MaximumRange!);
        if (option.Reason is { } reason) text += "\n" + Unavailable(reason);
        return text;
    }
    private static string Unavailable(SessionFailure reason) => GameText.BattleFailure(reason);
    private string KeyHint(GameAction action) => _input.Hint(action).Split(" / ", 2)[0];

    internal object Observe() => new { page = _page.ToString(), command = _command, row = _row,
        candidate = TargetCandidate?.Value, visible = Visible, layout = _layout.Observe() };
}
