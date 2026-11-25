using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Persistence;

public sealed class FieldSnapshotData
{
    internal FieldSnapshotData(SettledFieldSnapshot snapshot) => Snapshot = snapshot;
    internal SettledFieldSnapshot Snapshot { get; }

    public FieldReturnAnchor Landing
    {
        get
        {
            var player = Snapshot.Visit.Actors.Single(actor => actor.Slot == Snapshot.Visit.Aliases[Snapshot.Visit.Player]);
            return new(Snapshot.Visit.Body.Map, new(player.Motion.X / 384, player.Motion.Y / 384), player.Motion.Facing);
        }
    }

    public IReadOnlyDictionary<string, string> Names => Snapshot.Registers.Names;

    public byte? SavedLevel(string actor) => Snapshot.Party.Allies.FirstOrDefault(ally => ally.Actor == actor)?.Level;
}

public sealed record FieldSnapshotOffer(Guid SessionId, long Revision, FieldReturnAnchor? Landing, string? Error)
{
    public bool Allowed => Error is null && Landing is not null;
}
public sealed record FieldSnapshotCapture(FieldSnapshotData? Data, string? Error);
