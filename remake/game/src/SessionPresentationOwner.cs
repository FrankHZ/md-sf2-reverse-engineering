using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;

namespace Sf2.Remake.GodotAdapter;

// Presentation routing only; GameSession remains the sole lifecycle authority.
internal static class SessionPresentationOwner
{
    internal static bool IsBattle(SessionSnapshot current) => current.Mode == SessionMode.Battle &&
        // Entry publishes ActiveBattle before field-owned load/start delivery finishes.
        // Once entry finishes, every battle state (including pending rounds and
        // terminal/failure snapshots) belongs to the battle presenter.
        !(current.Story.EnteringBattle is not null && current.Story.Continuation is
            (ProgramContinuation.BeforeBattleFinished or ProgramContinuation.BattleLoadFinished or
             ProgramContinuation.BattleStartFinished));
}
