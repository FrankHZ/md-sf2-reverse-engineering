using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;

namespace Sf2.Remake.GodotAdapter.Input;

// Adapter timing only. Application still owns movement admission and every event.
internal sealed class ExplorationMovementHold
{
    private bool _armed;
    private ulong _retryAt;
    private WaitToken? _motion;
    internal void Clear(GameInput input) { _armed = false; _retryAt = 0; _motion = null; input.DisarmMovement(); }
    private bool OwnsMotion(SessionSnapshot current) => _motion is not null && current.Story.Cursor is null && current.Story.Wait switch
    {
        EntityWait { AfterMotion: null } wait => wait.Token == _motion && wait.Entity == current.Exploration?.Player,
        ZoneArrivalWait arrival => arrival.FieldMoveToken == _motion,
        _ => false,
    };
    internal bool CanContinue(SessionSnapshot current) => current.CanWaitAtInput || OwnsMotion(current);
    internal void Pressed(ulong now)
    {
        _armed = true;
        _retryAt = _motion is null ? now + 120000 : 0;
    }
    internal GameAction? Repeat(GameInput input, SessionSnapshot current, ulong now)
    {
        if (!_armed || !current.CanWaitAtInput || now < _retryAt || input.MovementDirection is not { } direction) return null;
        _retryAt = now + 120000; // Back off only if this attempt cannot start a step; never catch up.
        return direction;
    }

    internal void Observe(GameInput input, SessionSnapshot before, SessionResult result, SessionCommand command)
    {
        var after = result.Snapshot;
        // A field Move owns this token through its ordinary motion, not arbitrary EntityWaits.
        if (command is Move && after.Story.Wait is EntityWait { AfterMotion: null } started &&
            started.Entity == after.Exploration?.Player && after.Story.Cursor is null)
        {
            _motion = started.Token;
            _retryAt = 0; // Successful motion owns its cadence; no independent repeat deadline.
        }
        bool ownMotion = OwnsMotion(after);
        bool directZoneReturn = ownMotion && after.Story.Wait is ZoneArrivalWait;
        if (result.Failure is not null || before.SessionId != after.SessionId ||
            before.Exploration?.Definition != after.Exploration?.Definition ||
            after.Story.Cursor is not null || after.Story.Callers.Count != 0 ||
            after.Story.EventCaller is not null && !directZoneReturn ||
            after.Story.EnteringBattle is not null || (!after.CanWaitAtInput && !ownMotion) ||
            result.Observations.Any(row => row.Kind is "map-transferred" or "map-input-program" ||
                row.Kind == "program-instruction" && !directZoneReturn))
            Clear(input);
        else if (after.CanWaitAtInput) _motion = null;
    }
}
