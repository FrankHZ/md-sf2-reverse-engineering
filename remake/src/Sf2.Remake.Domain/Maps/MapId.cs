namespace Sf2.Remake.Domain.Maps;

public sealed record MapId
{
    public MapId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}
