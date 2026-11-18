namespace Sf2.Remake.Application.Runtime.Persistence;

// Settled local progress only: no definitions, executable actions or presentation continuations.
// The selected game's finite restore policy admits the population and reconstructs ambient actions.
public sealed record SavedFieldScene(string Setup, IReadOnlyList<SavedFieldActor> Actors,
    IReadOnlyDictionary<string, int> Aliases, IReadOnlyList<int> CurrentEntities,
    IReadOnlyList<SavedLayoutCell> Layout);
public sealed record SavedLayoutCell(int Index, ushort Value);
public sealed record SavedFieldActor(string Entity, int Slot, short X, short Y, byte Facing,
    byte Layer, byte FlagsA, byte FlagsB, ushort SpeedX, ushort SpeedY, bool Visible,
    int Sprite, ushort RenderSize, bool Priority, int LeaderSlot, int OffsetX, int OffsetY,
    bool WaitForDestination);
