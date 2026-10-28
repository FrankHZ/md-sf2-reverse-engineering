using Sf2.Remake.Application.Runtime;

namespace Sf2.Remake.GodotAdapter.Exploration;

public sealed partial class ExplorationSessionView
{
    private bool _skipRequiresRelease;
    private bool _battleHandoffPending;

    private void SuspendEntrySkip()
    {
        _skipRequiresRelease = true;
        if (_session?.Current.Story.EntrySkip is not null) Send(new CancelBattleEntrySkip());
    }

    private bool FinishEntryHandoff()
    {
        if (!_battleHandoffPending) return false;
        if (!_input.GameplayHeld && GetWindow().HasFocus())
        {
            _battleHandoffPending = false; _handedOff = true;
            _enterBattle!(_result!);
        }
        return true;
    }
}
