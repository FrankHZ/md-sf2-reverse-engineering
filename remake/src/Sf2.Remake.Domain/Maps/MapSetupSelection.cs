using System.Collections.ObjectModel;

namespace Sf2.Remake.Domain.Maps;

public sealed record MapSetupId
{
    public MapSetupId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record FlagId
{
    public FlagId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapSetupFlagVariant
{
    public MapSetupFlagVariant(FlagId flag, MapSetupId setup)
    {
        Flag = flag ?? throw new ArgumentNullException(nameof(flag));
        Setup = setup ?? throw new ArgumentNullException(nameof(setup));
    }

    public FlagId Flag { get; }

    public MapSetupId Setup { get; }
}

public sealed class MapSetupRoute
{
    private readonly ReadOnlyCollection<MapSetupFlagVariant> _flagAlternatives;

    public MapSetupRoute(
        MapSetupId defaultSetup,
        IEnumerable<MapSetupFlagVariant> flagAlternatives)
    {
        DefaultSetup = defaultSetup ?? throw new ArgumentNullException(nameof(defaultSetup));
        ArgumentNullException.ThrowIfNull(flagAlternatives);

        List<MapSetupFlagVariant> copiedAlternatives = [];
        foreach (MapSetupFlagVariant alternative in flagAlternatives)
        {
            copiedAlternatives.Add(
                alternative ?? throw new ArgumentException(
                    "Flag alternatives cannot contain null entries.",
                    nameof(flagAlternatives)));
        }

        _flagAlternatives = copiedAlternatives.AsReadOnly();
    }

    public MapSetupId DefaultSetup { get; }

    public IReadOnlyList<MapSetupFlagVariant> FlagAlternatives => _flagAlternatives;
}

public static class MapSetupSelector
{
    public static MapSetupId Select(
        MapSetupRoute? route,
        MapSetupId voidSetup,
        Func<FlagId, bool> isFlagSet)
    {
        ArgumentNullException.ThrowIfNull(voidSetup);
        ArgumentNullException.ThrowIfNull(isFlagSet);

        if (route is null)
        {
            return voidSetup;
        }

        MapSetupId selected = route.DefaultSetup;
        foreach (MapSetupFlagVariant alternative in route.FlagAlternatives)
        {
            if (isFlagSet(alternative.Flag))
            {
                selected = alternative.Setup;
            }
        }

        return selected;
    }
}
