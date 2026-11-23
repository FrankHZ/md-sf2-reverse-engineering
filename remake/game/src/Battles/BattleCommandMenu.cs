using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Page = Sf2.Remake.GodotAdapter.Battles.BattleDecisionPage;

namespace Sf2.Remake.GodotAdapter.Battles;

// Renders the decision and reports hits against the actual control geometry.
internal sealed partial class BattleCommandMenu : Control
{
    private readonly BattleMenuLayout _layout = new() { Name = "Layout" };
    private readonly MenuPointer _pointer = new();
    private BattleSceneDefinition? _content;
    private ScenarioDefinition? _definition;
    internal bool Covers(Vector2 point) => _layout.Covers(point);
    internal void AvoidSubject(Rect2 rectangle) => _layout.AvoidSubject(rectangle);
    internal void ReadDescription(GameAction direction) => _layout.ReadDescription(direction == GameAction.Left ? -80 : 80);

    public override void _Ready()
    {
        _layout.InvalidatePointer = _pointer.Cancel;
        MouseFilter = MouseFilterEnum.Ignore; AddChild(_layout);
    }
    internal void Arrange(Vector2 size)
    {
        if (Size != size) _pointer.Cancel();
        Size = size;
        _layout.Arrange(size);
    }
    internal void ConfigureAssets(BattleSceneDefinition? content, ScenarioDefinition definition)
    { _content = content; _definition = definition; _layout.ConfigureAssets(content); }
    internal UI.IdentityIcons Icons => _layout.Icons;
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

    internal (string Id, bool Activate)? ReadPointer(InputEventMouse input, BattleDecision decision)
    {
        if (!decision.Visible || decision.Moving) return null;
        if (input is InputEventMouseMotion motion)
        {
            bool moved = _pointer.Moved(motion);
            return moved && _layout.Hit(motion.Position).Id is { } id ? (id, false) : null;
        }
        if (input is not InputEventMouseButton button) return null;
        if (button.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        {
            _pointer.Cancel();
            if (button.Pressed) _layout.Wheel(button.Position, button.ButtonIndex == MouseButton.WheelUp ? -80 : 80);
            return null;
        }
        if (button.ButtonIndex is not (MouseButton.Left or MouseButton.Right))
        {
            _pointer.Cancel();
            return null;
        }
        var hit = button.ButtonIndex == MouseButton.Right ? ("Back", GetGlobalRect()) : _layout.Hit(button.Position);
        return _pointer.Read(button, hit.Item1, hit.Item2, decision.PointerBoundary) is { } activated ? (activated, true) : null;
    }

    internal void Present(BattleDecision decision, SessionFailure? failure, GameInput input, BattleActingStatus? acting)
    {
        Visible = decision.Visible;
        if (!Visible) return;
        var current = decision.Snapshot;
        var page = decision.Page;
        var command = decision.Command;
        bool moving = decision.Moving;
        bool targetPage = decision.Targeting;
        bool mainPage = !moving && page == Page.Main;
        bool list = decision.IsList;
        bool commit = page == Page.Commit;
        bool search = current.Selection?.Stage == BattleSelectionStage.SearchCommitReady;
        var offers = decision.Offers(command);
        int row = decision.Row;

        _layout.Actor.PresentActing(targetPage ? null : acting);
        var candidate = decision.Candidate is { } targetRef ? current.Battle.GetActor(targetRef) : null;
        _layout.Target.Present(candidate, candidate is null ? "" : BattleCharacterNames.Resolve(candidate, _content, current.Story), GameText.Get("battle.target"));
        _layout.Context.Text = GameText.Get("battle.context", current.Battle.Round, current.Battle.Gold?.ToString() ?? "—");
        for (int i = 0; i < _layout.Commands.Length; i++)
        {
            var button = _layout.Commands[i];
            var commandRow = (BattleCommandRow)i;
            button.CaptionOverride = commandRow == BattleCommandRow.Stay && decision.Search ? "battle.command.search" : null;
            button.RefreshCaption();
            button.SetSelection(mainPage && command == commandRow, input.Settings.ReducedFlash);
            button.Available = decision.CommandAvailable(commandRow);
        }
        var rows = list ? offers : [];
        _layout.ShowRows(rows.Select(option => OptionLabel(option, current)).ToArray(),
            rows.Select(option => option.Enabled).ToArray(), row, list, rows.Select(option => OptionIcon(option, current)).ToArray());
        _layout.Heading.Text = moving ? GameText.Get("battle.position") : page switch
        {
            Page.Magic => GameText.Get("battle.magic"),
            Page.Items => GameText.Get("battle.items"),
            Page.Target => GameText.Get("battle.choose-target"),
            Page.Commit => GameText.Get(search ? "battle.search-confirm" : "battle.stay-here"),
            _ => _layout.Commands[(int)command].Caption,
        };
        string confirm = input.CompactHint(GameAction.Confirm), cancel = input.CompactHint(GameAction.Cancel);
        string back = GameText.Get("battle.back-help", cancel);
        bool canCycle = decision.Targets.Length > 0;
        bool canConfirm = decision.Confirmation is not null;
        if (moving) _layout.Help.Text = GameText.Get("battle.move-help", confirm);
        else if (targetPage)
        {
            string next = input.CompactHint(GameAction.Target);
            _layout.Help.Text = canConfirm ? GameText.Get("battle.target-help", next, confirm, cancel) :
                canCycle ? GameText.Get("battle.target-pick-help", next, cancel) : back;
            if (canCycle && input.KeyboardMouseHints) _layout.Help.Text += "\n" + GameText.Get("battle.pointer-target-help");
        }
        else
        {
            string directions = $"{input.CompactHint(GameAction.Up)}/{input.CompactHint(GameAction.Down)}";
            string focus = list ? GameText.Get("battle.choice-help", directions) :
                mainPage ? GameText.Get("battle.command-help", directions) : "";
            _layout.Help.Text = focus + (decision.CanActivateFocus ? GameText.Get("battle.confirm-help", confirm, cancel) : back);
            if (mainPage)
                _layout.Help.Text += GameText.Get("battle.extra-help", input.CompactHint(GameAction.Equipment),
                    input.CompactHint(GameAction.Give), input.CompactHint(GameAction.Discard));
        }

        string description;
        if (moving) description = GameText.Get("battle.move-detail");
        else if (targetPage)
        {
            var selected = decision.SelectedAction;
            description = selected is null ? "" : OptionLabel(selected, current);
            if (!canCycle) description += "\n" + GameText.Get("battle.no-target");
            else if (!canConfirm) description += "\n" + GameText.Get("battle.target-invalid");
        }
        else if (commit) description = GameText.Get(search ? "battle.search-detail" : "battle.stay-detail");
        else if (command == BattleCommandRow.Stay && decision.Search) description = GameText.Get("battle.search-detail");
        else if (list && offers.Length > 0) description = page == Page.Items
            ? DescribeItem(offers[row], current.Selection!.Actor, current) : Describe(offers[row], current);
        else if (offers.Length == 0) description = GameText.Get(command switch
        {
            BattleCommandRow.Magic => "battle.no-magic",
            BattleCommandRow.Item => "battle.no-items",
            _ => "battle.no-target",
        });
        else description = !offers.Any(offer => offer.Enabled) && offers[0].Reason is { } reason ? Unavailable(reason) : "";
        if (!moving && decision.Choices.Failure is { } queryFailure) description = Unavailable(queryFailure);
        if (failure is not null && failure != decision.Choices.Failure) description += "\n" + Unavailable(failure);
        string fallback = GameText.Fallback("battle.", includeKey: true);
        _layout.PresentDetail(description + (targetPage || fallback.Length == 0 ? "" : "\n" + fallback),
            (current.SessionId, current.Selection?.Actor, moving, page, command, row), mainPage || list ? input : null);
        _layout.ShowNavigation(!moving, canCycle, targetPage || commit, canConfirm);
        _layout.ShowDecision(targetPage, mainPage, moving || mainPage || list || targetPage);
    }

    private string? OptionIcon(BattleActionChoice option, SessionSnapshot current) => _definition?.PrivateDefinitions is null ? null : option.Action.Spell is { } spell
        ? UI.IdentityIcons.Spell(_definition!, spell) : option.Action.ItemSlot is { } slot && current.Selection is { } selection
            ? UI.IdentityIcons.Item(current.Battle.GetActor(selection.Actor).SourceLoadout!.Items[slot]) : null;
    private string OptionLabel(BattleActionChoice option, SessionSnapshot current)
    {
        string name = DisplayNames.Action(option, current);
        string label = option.Action.ItemSlot is { } slot ? $"{slot + 1}. {name}" : name;
        if (option.Action.Spell is { } spell)
        {
            var definition = current.Battle.Definition;
            byte? cost = definition.Spells.TryGetValue(spell, out var healing) ? healing.MpCost :
                definition.AttackSpells.TryGetValue(spell, out var attack) ? attack.MpCost :
                definition.SupportSpells.TryGetValue(spell, out var support) ? support.MpCost :
                definition.RetreatSpells.TryGetValue(spell, out var retreat) ? retreat.MpCost : null;
            if (cost is { } mp) label += $" · {mp} MP";
        }
        return label + (option.Enabled ? "" : GameText.Get("battle.unavailable-tag"));
    }
    private string Describe(BattleActionChoice option, SessionSnapshot current)
    {
        string text = OptionLabel(option, current);
        if (option.MinimumRange is { } minimum) text += "\n" + GameText.Get("battle.range", minimum, option.MaximumRange!);
        if (option.Reason is { } reason) text += "\n" + Unavailable(reason);
        return text;
    }
    private string DescribeItem(BattleActionChoice option, ActorRef actor, SessionSnapshot current)
    {
        var owner = current.Battle.GetActor(actor);
        ushort word = owner.SourceLoadout!.Items[option.Action.ItemSlot!.Value];
        var lines = new List<string>();
        string effects = ItemDescriptions.Read(_definition!, current, (byte)(word & 127), owner.Definition, (word & 256) != 0);
        if (effects.Length > 0) lines.Add(effects);
        if (option.MinimumRange is { } minimum) lines.Add(GameText.Get("battle.range", minimum, option.MaximumRange!));
        if (option.Reason is { } reason) lines.Add(GameText.BattleRecovery(reason));
        return string.Join("\n", lines);
    }

    private static string Unavailable(SessionFailure reason) => GameText.BattleFailure(reason);
    internal object Observe(BattleDecision? decision) => new
    {
        page = (decision?.Page ?? Page.Main).ToString(), command = (int)(decision?.Command ?? BattleCommandRow.Attack),
        row = decision?.Row ?? 0, candidate = decision?.Candidate?.Value, visible = Visible, layout = _layout.Observe(),
    };
}
