using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;

namespace Sf2.Remake.GodotAdapter.Input;

// Adapter timing only. Application still owns movement admission and every event.
internal sealed class ExplorationMovementHold
{
    private ulong _next;
    private WaitToken? _motion;
    internal void Clear(GameInput input) { _next = 0; _motion = null; input.DisarmMovement(); }
    internal bool CanContinue(SessionSnapshot current) => current.CanWaitAtInput ||
        _motion is not null && current.Story.Wait is EntityWait { AfterMotion: null } wait &&
        wait.Token == _motion && wait.Entity == current.Exploration?.Player && current.Story.Cursor is null;
    internal void Pressed(ulong now) => _next = now + 220000;
    internal GameAction? Repeat(GameInput input, SessionSnapshot current, ulong now)
    {
        if (_next == 0 || !current.CanWaitAtInput || now < _next || input.MovementDirection is not { } direction) return null;
        _next = now + 120000; // One attempt; missed deadlines never become catch-up commands.
        return direction;
    }

    internal void Observe(GameInput input, SessionSnapshot before, SessionResult result, SessionCommand command)
    {
        var after = result.Snapshot;
        // A field Move owns this token through its ordinary motion, not arbitrary EntityWaits.
        if (command is Move && after.Story.Wait is EntityWait { AfterMotion: null } started &&
            started.Entity == after.Exploration?.Player && after.Story.Cursor is null)
            _motion = started.Token;
        bool ownMotion = _motion is not null && after.Story.Wait is EntityWait { AfterMotion: null } motion &&
            motion.Token == _motion && motion.Entity == after.Exploration?.Player;
        if (result.Failure is not null || before.Exploration?.Definition != after.Exploration?.Definition ||
            after.Story.Cursor is not null || after.Story.Callers.Count != 0 || after.Story.EventCaller is not null ||
            after.Story.EnteringBattle is not null || (!after.CanWaitAtInput && !ownMotion) ||
            result.Observations.Any(row => row.Kind is "program-instruction" or "map-transferred" or "map-input-program"))
            Clear(input);
        else if (after.CanWaitAtInput) _motion = null;
    }
}
