namespace Sf2.Remake.Domain.Maps;

public enum ExplorationDirection
{
    North,
    East,
    South,
    West,
}

public sealed record MapPosition
{
    public MapPosition(int x, int y)
    {
        ValidateCoordinate(x, nameof(x));
        ValidateCoordinate(y, nameof(y));
        X = x;
        Y = y;
    }

    public int X { get; }

    public int Y { get; }

    private static void ValidateCoordinate(int value, string parameterName)
    {
        if (value < 0 || value >= WorkingMapLayout.ColumnCount)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
