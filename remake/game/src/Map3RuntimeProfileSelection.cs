using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Sf2.Remake.Godot.Tests")]

namespace Sf2.Remake.GodotAdapter;

internal enum Map3RuntimeProfile
{
    PublicSynthetic,
    PrivateLocal,
}

internal sealed record Map3RuntimeProfileSelection
{
    private const string ProfileOption = "--runtime-profile";
    private const string ImportOption = "--canonical-map-import";
    private const string RomOption = "--original-rom";
    private const string TilesetMetadataOption = "--map-tileset-metadata";
    private const string PaletteMetadataOption = "--map-palette-metadata";

    internal const string PrivateSmokeOption = "--private-map3-smoke";
    internal const string PrivateBaseViewOption = "--private-map3-base-view";

    private Map3RuntimeProfileSelection(
        Map3RuntimeProfile? requestedProfile,
        bool isAvailable,
        string? canonicalImportPath,
        bool privateSmokeRequested,
        bool privateBaseViewRequested,
        string? originalRomPath,
        string? tilesetMetadataPath,
        string? paletteMetadataPath,
        string? diagnostic)
    {
        RequestedProfile = requestedProfile;
        IsAvailable = isAvailable;
        CanonicalImportPath = canonicalImportPath;
        PrivateSmokeRequested = privateSmokeRequested;
        PrivateBaseViewRequested = privateBaseViewRequested;
        OriginalRomPath = originalRomPath;
        TilesetMetadataPath = tilesetMetadataPath;
        PaletteMetadataPath = paletteMetadataPath;
        Diagnostic = diagnostic;
    }

    internal Map3RuntimeProfile? RequestedProfile { get; }

    internal bool IsAvailable { get; }

    internal string? CanonicalImportPath { get; }

    internal bool PrivateSmokeRequested { get; }

    internal bool PrivateBaseViewRequested { get; }

    internal string? OriginalRomPath { get; }

    internal string? TilesetMetadataPath { get; }

    internal string? PaletteMetadataPath { get; }

    internal string? Diagnostic { get; }

    internal static Map3RuntimeProfileSelection Parse(IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        Dictionary<string, string> values = new(StringComparer.Ordinal);
        bool privateSmokeRequested = false;
        bool privateBaseViewRequested = false;

        foreach (string argument in arguments)
        {
            ArgumentNullException.ThrowIfNull(argument);
            if (string.Equals(argument, PrivateSmokeOption, StringComparison.Ordinal))
            {
                privateSmokeRequested = true;
                continue;
            }

            if (string.Equals(argument, PrivateBaseViewOption, StringComparison.Ordinal))
            {
                if (privateBaseViewRequested)
                {
                    return Unavailable(
                        ParseKnownProfile(values.GetValueOrDefault(ProfileOption)),
                        privateSmokeRequested,
                        "The private base-view option must appear at most once.");
                }

                privateBaseViewRequested = true;
                continue;
            }

            string? option = ValueOption(argument);
            if (option is null)
            {
                continue;
            }

            if (string.Equals(argument, option, StringComparison.Ordinal) ||
                !argument.StartsWith(option + "=", StringComparison.Ordinal))
            {
                return Unavailable(
                    ParseKnownProfile(values.GetValueOrDefault(ProfileOption)),
                    privateSmokeRequested,
                    "Runtime profile options require explicit non-empty values.");
            }

            if (!values.TryAdd(option, argument[(option.Length + 1)..]))
            {
                return Unavailable(
                    ParseKnownProfile(values.GetValueOrDefault(ProfileOption)),
                    privateSmokeRequested,
                    $"The {OptionLabel(option)} option must appear exactly once.");
            }
        }

        values.TryGetValue(ProfileOption, out string? profileValue);
        Map3RuntimeProfile? profile = ParseKnownProfile(profileValue);
        bool hasPrivateInputs = values.ContainsKey(ImportOption) ||
            values.ContainsKey(RomOption) ||
            values.ContainsKey(TilesetMetadataOption) ||
            values.ContainsKey(PaletteMetadataOption) ||
            privateBaseViewRequested ||
            privateSmokeRequested;

        if (!values.ContainsKey(ProfileOption))
        {
            return hasPrivateInputs
                ? Unavailable(
                    null,
                    privateSmokeRequested,
                    "Private runtime options require an explicit PrivateLocal profile selection.")
                : Available(Map3RuntimeProfile.PublicSynthetic, null, false);
        }

        if (profile is null)
        {
            return Unavailable(
                null,
                privateSmokeRequested,
                "The requested runtime profile is unknown.");
        }

        if (profile == Map3RuntimeProfile.PublicSynthetic)
        {
            return hasPrivateInputs
                ? Unavailable(
                    profile,
                    privateSmokeRequested,
                    "PublicSynthetic cannot consume private runtime options.")
                : Available(profile.Value, null, false);
        }

        if (!TryNormalizeRequiredPath(
                values.GetValueOrDefault(ImportOption),
                out string? canonicalImportPath))
        {
            return Unavailable(
                profile,
                privateSmokeRequested,
                "PrivateLocal requires one fully qualified ignored canonical import path.");
        }

        bool hasAnyVisualPath = values.ContainsKey(RomOption) ||
            values.ContainsKey(TilesetMetadataOption) ||
            values.ContainsKey(PaletteMetadataOption);
        if (!privateBaseViewRequested)
        {
            return hasAnyVisualPath
                ? Unavailable(
                    profile,
                    privateSmokeRequested,
                    "Private visual inputs require explicit private Map 3 base-view selection.")
                : Available(profile.Value, canonicalImportPath, privateSmokeRequested);
        }

        if (!TryNormalizeRequiredPath(
                values.GetValueOrDefault(RomOption),
                out string? originalRomPath) ||
            !TryNormalizeRequiredPath(
                values.GetValueOrDefault(TilesetMetadataOption),
                out string? tilesetMetadataPath) ||
            !TryNormalizeRequiredPath(
                values.GetValueOrDefault(PaletteMetadataOption),
                out string? paletteMetadataPath))
        {
            return Unavailable(
                profile,
                privateSmokeRequested,
                "Private Map 3 base view requires fully qualified ignored ROM, tileset-metadata, and palette-metadata paths.");
        }

        return new Map3RuntimeProfileSelection(
            profile,
            isAvailable: true,
            canonicalImportPath,
            privateSmokeRequested,
            privateBaseViewRequested: true,
            originalRomPath,
            tilesetMetadataPath,
            paletteMetadataPath,
            diagnostic: null);
    }

    private static string? ValueOption(string argument)
    {
        foreach (string option in new[]
        {
            ProfileOption,
            ImportOption,
            RomOption,
            TilesetMetadataOption,
            PaletteMetadataOption,
        })
        {
            if (string.Equals(argument, option, StringComparison.Ordinal) ||
                argument.StartsWith(option + "=", StringComparison.Ordinal))
            {
                return option;
            }
        }

        return null;
    }

    private static string OptionLabel(string option) => option switch
    {
        ProfileOption => "runtime profile",
        ImportOption => "canonical import",
        RomOption => "original ROM",
        TilesetMetadataOption => "map tileset metadata",
        PaletteMetadataOption => "map palette metadata",
        _ => "runtime",
    };

    private static bool TryNormalizeRequiredPath(string? value, out string? normalized)
    {
        normalized = null;
        if (string.IsNullOrWhiteSpace(value) || !Path.IsPathFullyQualified(value))
        {
            return false;
        }

        try
        {
            normalized = Path.GetFullPath(value);
            return true;
        }
        catch (Exception error) when (
            error is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static Map3RuntimeProfileSelection Available(
        Map3RuntimeProfile profile,
        string? canonicalImportPath,
        bool privateSmokeRequested) =>
        new(
            profile,
            true,
            canonicalImportPath,
            privateSmokeRequested,
            privateBaseViewRequested: false,
            originalRomPath: null,
            tilesetMetadataPath: null,
            paletteMetadataPath: null,
            diagnostic: null);

    private static Map3RuntimeProfileSelection Unavailable(
        Map3RuntimeProfile? profile,
        bool privateSmokeRequested,
        string diagnostic) =>
        new(
            profile,
            false,
            canonicalImportPath: null,
            privateSmokeRequested,
            privateBaseViewRequested: false,
            originalRomPath: null,
            tilesetMetadataPath: null,
            paletteMetadataPath: null,
            diagnostic);

    private static Map3RuntimeProfile? ParseKnownProfile(string? value) => value switch
    {
        "public-synthetic" => Map3RuntimeProfile.PublicSynthetic,
        "private-local" => Map3RuntimeProfile.PrivateLocal,
        _ => null,
    };
}
