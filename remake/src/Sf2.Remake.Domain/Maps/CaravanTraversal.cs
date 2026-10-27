namespace Sf2.Remake.Domain.Maps;

// SF2DISASM c834c652, esc02/esc07: opcode7 is mode1 despite its Raft label.
internal static class CaravanTraversal
{
    internal static MapPosition? Candidate(EntityMotionState player, OriginalMapTraversalArea area,
        ExplorationDirection direction, bool walking)
    {
        int margin = walking ? 1 : 0;
        int x = player.X / 384, y = player.Y / 384;
        switch (direction)
        {
            case ExplorationDirection.North: if (player.Y <= (area.MinimumY + margin) * 384) return null; y--; break;
            case ExplorationDirection.South: if (player.Y >= (area.MaximumY - margin) * 384) return null; y++; break;
            case ExplorationDirection.West: if (player.X <= (area.MinimumX + margin) * 384) return null; x--; break;
            case ExplorationDirection.East: if (player.X >= (area.MaximumX - margin) * 384) return null; x++; break;
            default: throw new ArgumentOutOfRangeException(nameof(direction));
        }
        return x is >= 0 and < 64 && y is >= 0 and < 64 ? new(x, y) : null;
    }

    internal static bool Obstructed(int x, int y, IEnumerable<EntityMotionState> physical) =>
        physical.Any(other => (other.FlagsA & 0x80) != 0 && Math.Abs(other.X - x) < 256 && Math.Abs(other.Y - y) < 256);
}
