using System.Collections.ObjectModel;

namespace Sf2.Remake.Domain.Maps;

public sealed record WorkingMapBlockCopy
{
    public WorkingMapBlockCopy(
        int sourceX,
        int sourceY,
        int destinationX,
        int destinationY,
        int width,
        int height)
    {
        ValidateCoordinate(sourceX, nameof(sourceX));
        ValidateCoordinate(sourceY, nameof(sourceY));
        ValidateCoordinate(destinationX, nameof(destinationX));
        ValidateCoordinate(destinationY, nameof(destinationY));
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);

        SourceX = sourceX;
        SourceY = sourceY;
        DestinationX = destinationX;
        DestinationY = destinationY;
        Width = width;
        Height = height;
    }

    public int SourceX { get; }

    public int SourceY { get; }

    public int DestinationX { get; }

    public int DestinationY { get; }

    public int Width { get; }

    public int Height { get; }

    private static void ValidateCoordinate(int value, string parameterName)
    {
        if (value < 0 || value >= WorkingMapLayout.ColumnCount)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}

public sealed class WorkingMapLayout
{
    public const int ColumnCount = 64;
    public const int RowCount = 64;
    public const int WordCount = ColumnCount * RowCount;

    private readonly ushort[] _words;
    private readonly ReadOnlyCollection<ushort> _readOnlyWords;

    public WorkingMapLayout(IEnumerable<ushort> words)
        : this(CopyAndValidate(words))
    {
    }

    private WorkingMapLayout(ushort[] words)
    {
        _words = words;
        _readOnlyWords = Array.AsReadOnly(_words);
    }

    public IReadOnlyList<ushort> Words => _readOnlyWords;

    public ushort this[int x, int y]
    {
        get
        {
            ValidateCoordinate(x, nameof(x));
            ValidateCoordinate(y, nameof(y));
            return _words[(y * ColumnCount) + x];
        }
    }

    public ushort GetWord(int linearIndex)
    {
        if (linearIndex < 0 || linearIndex >= WordCount)
        {
            throw new ArgumentOutOfRangeException(nameof(linearIndex));
        }

        return _words[linearIndex];
    }

    public WorkingMapLayout ApplyBlockCopy(WorkingMapBlockCopy operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ValidateSpan(operation.SourceX, operation.SourceY, operation.Width, operation.Height, "source");
        ValidateSpan(
            operation.DestinationX,
            operation.DestinationY,
            operation.Width,
            operation.Height,
            "destination");

        ushort[] result = [.. _words];
        int sourceRowStart = (operation.SourceY * ColumnCount) + operation.SourceX;
        int destinationRowStart =
            (operation.DestinationY * ColumnCount) + operation.DestinationX;

        for (int row = 0; row < operation.Height; row++)
        {
            for (int column = 0; column < operation.Width; column++)
            {
                result[destinationRowStart + column] = result[sourceRowStart + column];
            }

            sourceRowStart += ColumnCount;
            destinationRowStart += ColumnCount;
        }

        return new WorkingMapLayout(result);
    }

    private static ushort[] CopyAndValidate(IEnumerable<ushort> words)
    {
        ArgumentNullException.ThrowIfNull(words);
        ushort[] copied = [.. words.Take(WordCount + 1)];
        if (copied.Length != WordCount)
        {
            throw new ArgumentException(
                $"A working map layout must contain exactly {WordCount} words.",
                nameof(words));
        }

        return copied;
    }

    private static void ValidateCoordinate(int value, string parameterName)
    {
        if (value < 0 || value >= ColumnCount)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }

    private static void ValidateSpan(
        int x,
        int y,
        int width,
        int height,
        string spanName)
    {
        long firstIndex = ((long)y * ColumnCount) + x;
        long finalRowStart = firstIndex + (((long)height - 1) * ColumnCount);
        long finalIndex = finalRowStart + width - 1;
        if (finalIndex >= WordCount)
        {
            throw new ArgumentOutOfRangeException(
                spanName,
                $"The {spanName} copy span exceeds the working layout.");
        }
    }
}
