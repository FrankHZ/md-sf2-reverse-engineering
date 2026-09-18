namespace Sf2.Remake.Domain.Maps;

public enum MapViewUpdateChannel
{
    Channel0,
    Channel1,
}

public sealed record MapViewUpdateState(
    bool Channel0Requested,
    bool Channel1Requested);
