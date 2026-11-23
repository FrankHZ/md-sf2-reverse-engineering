using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.GodotAdapter.Battles;

internal enum BattleDecisionPage { Main, Magic, Items, Target, Commit }
internal enum BattleCommandRow { Attack, Magic, Item, Stay }

// A projection of one live snapshot. Only row, parent list and attempted target
// persist as local navigation. The session still admits and commits commands.
internal sealed class BattleDecision
{
    internal readonly SessionSnapshot Snapshot;
    internal readonly BattleChoicesSnapshot Choices;
    internal readonly BattleDecisionPage Page;
    internal readonly BattleCommandRow Command;
    internal readonly int Row;
    internal readonly bool Visible, Moving;
    internal readonly BattleActionChoice? SelectedAction;
    internal readonly BattleTargetChoice[] Targets;
    internal readonly ActorRef? Candidate;
    private readonly BattleActionChoice[][] _offers;

    internal BattleDecision(SessionSnapshot snapshot, BattleChoicesSnapshot choices, bool show,
        BattleCommandRow command, bool listOpen, int row, ActorRef? attemptedTarget)
    {
        Snapshot = snapshot;
        Choices = choices;
        Command = command;
        Moving = snapshot.Selection?.Stage == BattleSelectionStage.Movement || snapshot.BattleMovement is not null;
        var selection = snapshot.Selection;
        Page = selection?.Stage switch
        {
            BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady =>
                selection.ActionReference?.Kind == BattleActionKind.Stay ? BattleDecisionPage.Commit : BattleDecisionPage.Target,
            BattleSelectionStage.SearchCommitReady => BattleDecisionPage.Commit,
            BattleSelectionStage.ActionChoice when listOpen => command switch
            {
                BattleCommandRow.Magic => BattleDecisionPage.Magic,
                BattleCommandRow.Item => BattleDecisionPage.Items,
                _ => throw new InvalidOperationException("Only Magic and Item own a battle decision list."),
            },
            _ => BattleDecisionPage.Main,
        };
        Visible = show && (Moving || selection?.Stage is BattleSelectionStage.ActionChoice or
            BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady or BattleSelectionStage.SearchCommitReady);
        _offers = [
            choices.Actions.Where(option => option.Action.Kind == BattleActionKind.Physical).ToArray(),
            choices.Actions.Where(option => option.Action.Spell is not null).ToArray(),
            choices.Actions.Where(option => option.Action.ItemSlot is not null).ToArray(),
            choices.Actions.Where(option => option.Action.Kind == BattleActionKind.Stay).ToArray(),
        ];
        Row = Math.Clamp(row, 0, Math.Max(0, Offers(command).Length - 1));
        SelectedAction = Page == BattleDecisionPage.Target ? ActionOffer(choices, selection!.ActionReference) : null;
        Targets = SelectedAction?.Targets.Where(target => target.Enabled).ToArray() ?? [];
        if (attemptedTarget is { } actor && actor == selection?.Target && Targets.Any(target => target.Actor == actor))
            Candidate = actor;
    }

    internal bool IsList => Page is BattleDecisionPage.Magic or BattleDecisionPage.Items;
    internal bool Targeting => Visible && Page == BattleDecisionPage.Target;
    internal bool Search => Choices.Search is not null;
    internal BattleActionChoice[] Offers(BattleCommandRow row) => _offers[(int)row];

    internal static BattleActionChoice? ActionOffer(BattleChoicesSnapshot choices, BattleActionRef? action) =>
        choices.Actions.SingleOrDefault(offer => offer.Action == action);

    internal bool CommandAvailable(BattleCommandRow row) => OpensList(row) || MainActivation(row) is not null;

    internal static bool OpensList(BattleCommandRow row) => row is BattleCommandRow.Magic or BattleCommandRow.Item;

    internal SessionCommand? MainActivation(BattleCommandRow row) =>
        row == BattleCommandRow.Stay && Search ? new BeginBattleSearch() : Activate(Offers(row).FirstOrDefault());

    internal SessionCommand? RowActivation(int row) => Activate(Offers(Command)[row]);

    private static SessionCommand? Activate(BattleActionChoice? offer) => offer is not { Enabled: true } ? null :
        offer.Action.Kind == BattleActionKind.Stay ? new CommitBattleStay() : new SelectBattleAction(offer.Action);

    internal Confirm? Confirmation => Page == BattleDecisionPage.Commit || Candidate is not null ? new() : null;

    internal bool CanActivateFocus => Page switch
    {
        BattleDecisionPage.Main => CommandAvailable(Command),
        BattleDecisionPage.Magic or BattleDecisionPage.Items => Offers(Command).Length > 0 && RowActivation(Row) is not null,
        _ => Confirmation is not null,
    };

    internal SelectTarget? TargetAt(ActorRef? actor) => actor is { } id && Targets.Any(target => target.Actor == id)
        ? new SelectTarget(id) : null;

    internal SelectTarget? StepTarget(int delta)
    {
        if (Targets.Length == 0) return null;
        var from = Candidate ?? Snapshot.Selection?.Target;
        int index = Array.FindIndex(Targets, target => target.Actor == from);
        int next = index < 0 ? (delta < 0 ? Targets.Length - 1 : 0) : (index + delta + Targets.Length) % Targets.Length;
        return new SelectTarget(Targets[next].Actor);
    }

    internal object PointerBoundary => (Snapshot.SessionId, Snapshot.Revision, Snapshot.Selection?.Actor, Page, Candidate);
}
