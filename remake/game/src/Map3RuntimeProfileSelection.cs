using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Sf2.Remake.Godot.Tests")]

namespace Sf2.Remake.GodotAdapter;

internal enum Map3RuntimeProfile
{
    PublicSynthetic,
    PrivateLocal,
}

internal enum PrivateMap3WorldTreatment
{
    ExactNearest,
    EdgeScale2x,
}

internal sealed record Map3RuntimeProfileSelection
{
    private const string ProfileOption = "--runtime-profile";
    private const string ImportOption = "--canonical-map-import";
    private const string RomOption = "--original-rom";
    private const string TilesetMetadataOption = "--map-tileset-metadata";
    private const string PaletteMetadataOption = "--map-palette-metadata";
    private const string PresentationAssetRootOption = "--presentation-asset-root";
    private const string PresentationAssetCommitOption = "--presentation-asset-commit";
    private const string PresentationManifestDigestOption =
        "--presentation-manifest-sha256";
    internal const string WorldTreatmentOption = "--private-map3-world-treatment";

    internal const string PrivateSmokeOption = "--private-map3-smoke";
    internal const string PrivateBaseViewOption = "--private-map3-base-view";
    internal const string PrivateBaseAtlasOption = "--private-map3-base-atlas";
    internal const string PrivateStaticOverlayOption = "--private-map3-static-overlay";
    internal const string PrivateHudPreviewOption = "--private-hud-preview";

    private Map3RuntimeProfileSelection(
        Map3RuntimeProfile? requestedProfile,
        bool isAvailable,
        string? canonicalImportPath,
        bool privateSmokeRequested,
        bool privateBaseViewRequested,
        bool privateBaseAtlasRequested,
        bool privateStaticOverlayRequested,
        string? originalRomPath,
        string? tilesetMetadataPath,
        string? paletteMetadataPath,
        bool privateHudPreviewRequested,
        string? presentationAssetRoot,
        string? presentationAssetCommit,
        string? presentationManifestDigest,
        PrivateMap3WorldTreatment worldTreatment,
        string? diagnostic)
    {
        RequestedProfile = requestedProfile;
        IsAvailable = isAvailable;
        CanonicalImportPath = canonicalImportPath;
        PrivateSmokeRequested = privateSmokeRequested;
        PrivateBaseViewRequested = privateBaseViewRequested;
        PrivateBaseAtlasRequested = privateBaseAtlasRequested;
        PrivateStaticOverlayRequested = privateStaticOverlayRequested;
        OriginalRomPath = originalRomPath;
        TilesetMetadataPath = tilesetMetadataPath;
        PaletteMetadataPath = paletteMetadataPath;
        PrivateHudPreviewRequested = privateHudPreviewRequested;
        PresentationAssetRoot = presentationAssetRoot;
        PresentationAssetCommit = presentationAssetCommit;
        PresentationManifestDigest = presentationManifestDigest;
        WorldTreatment = worldTreatment;
        Diagnostic = diagnostic;
    }

    internal Map3RuntimeProfile? RequestedProfile { get; }

    internal bool IsAvailable { get; }

    internal string? CanonicalImportPath { get; }

    internal bool PrivateSmokeRequested { get; }

    internal bool PrivateBaseViewRequested { get; }

    internal bool PrivateBaseAtlasRequested { get; }

    internal bool PrivateStaticOverlayRequested { get; }

    internal string? OriginalRomPath { get; }

    internal string? TilesetMetadataPath { get; }

    internal string? PaletteMetadataPath { get; }

    internal bool PrivateHudPreviewRequested { get; }

    internal string? PresentationAssetRoot { get; }

    internal string? PresentationAssetCommit { get; }

    internal string? PresentationManifestDigest { get; }

    internal PrivateMap3WorldTreatment WorldTreatment { get; }

    internal string? Diagnostic { get; }

    internal static Map3RuntimeProfileSelection Parse(IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        Dictionary<string, string> values = new(StringComparer.Ordinal);
        bool privateSmokeRequested = false;
        bool privateBaseViewRequested = false;
        bool privateBaseAtlasRequested = false;
        bool privateStaticOverlayRequested = false;
        bool privateHudPreviewRequested = false;

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
                        privateHudPreviewRequested,
                        "The private base-view option must appear at most once.");
                }

                privateBaseViewRequested = true;
                continue;
            }

            if (string.Equals(argument, PrivateBaseAtlasOption, StringComparison.Ordinal))
            {
                if (privateBaseAtlasRequested)
                {
                    return Unavailable(
                        ParseKnownProfile(values.GetValueOrDefault(ProfileOption)),
                        privateSmokeRequested,
                        privateHudPreviewRequested,
                        "The private base-atlas option must appear at most once.",
                        privateBaseAtlasRequested: true);
                }

                privateBaseAtlasRequested = true;
                continue;
            }

            if (string.Equals(argument, PrivateStaticOverlayOption, StringComparison.Ordinal))
            {
                if (privateStaticOverlayRequested)
                {
                    return Unavailable(
                        ParseKnownProfile(values.GetValueOrDefault(ProfileOption)),
                        privateSmokeRequested,
                        privateHudPreviewRequested,
                        "The private static-overlay option must appear at most once.",
                        privateBaseAtlasRequested,
                        privateStaticOverlayRequested: true);
                }

                privateStaticOverlayRequested = true;
                continue;
            }

            if (string.Equals(argument, PrivateHudPreviewOption, StringComparison.Ordinal))
            {
                if (privateHudPreviewRequested)
                {
                    return Unavailable(
                        ParseKnownProfile(values.GetValueOrDefault(ProfileOption)),
                        privateSmokeRequested,
                        privateHudPreviewRequested,
                        "The private HUD preview option must appear at most once.");
                }

                privateHudPreviewRequested = true;
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
                    privateHudPreviewRequested,
                    "Runtime profile options require explicit non-empty values.");
            }

            if (!values.TryAdd(option, argument[(option.Length + 1)..]))
            {
                return Unavailable(
                    ParseKnownProfile(values.GetValueOrDefault(ProfileOption)),
                    privateSmokeRequested,
                    privateHudPreviewRequested,
                    $"The {OptionLabel(option)} option must appear exactly once.");
            }
        }

        values.TryGetValue(ProfileOption, out string? profileValue);
        Map3RuntimeProfile? profile = ParseKnownProfile(profileValue);
        bool hasPrivateInputs = values.ContainsKey(ImportOption) ||
            values.ContainsKey(RomOption) ||
            values.ContainsKey(TilesetMetadataOption) ||
            values.ContainsKey(PaletteMetadataOption) ||
            values.ContainsKey(PresentationAssetRootOption) ||
            values.ContainsKey(PresentationAssetCommitOption) ||
            values.ContainsKey(PresentationManifestDigestOption) ||
            values.ContainsKey(WorldTreatmentOption) ||
            privateBaseViewRequested ||
            privateBaseAtlasRequested ||
            privateStaticOverlayRequested ||
            privateHudPreviewRequested ||
            privateSmokeRequested;

        if (!values.ContainsKey(ProfileOption))
        {
            return hasPrivateInputs
                ? Unavailable(
                    null,
                    privateSmokeRequested,
                    privateHudPreviewRequested,
                    "Private runtime options require an explicit PrivateLocal profile selection.")
                : Available(Map3RuntimeProfile.PublicSynthetic, null, false);
        }

        if (profile is null)
        {
            return Unavailable(
                null,
                privateSmokeRequested,
                privateHudPreviewRequested,
                "The requested runtime profile is unknown.");
        }

        if (profile == Map3RuntimeProfile.PublicSynthetic)
        {
            return hasPrivateInputs
                ? Unavailable(
                    profile,
                    privateSmokeRequested,
                    privateHudPreviewRequested,
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
                privateHudPreviewRequested,
                "PrivateLocal requires one fully qualified ignored canonical import path.");
        }

        if (privateBaseAtlasRequested && !privateBaseViewRequested)
        {
            return Unavailable(
                profile,
                privateSmokeRequested,
                privateHudPreviewRequested,
                "Private Map 3 base-atlas presentation requires explicit private base-view selection.",
                privateBaseAtlasRequested: true);
        }

        if (privateStaticOverlayRequested && !privateBaseViewRequested)
        {
            return Unavailable(
                profile,
                privateSmokeRequested,
                privateHudPreviewRequested,
                "Private Map 3 static-overlay diagnostics require explicit private base-view selection.",
                privateBaseAtlasRequested,
                privateStaticOverlayRequested: true);
        }

        PrivateMap3WorldTreatment worldTreatment = PrivateMap3WorldTreatment.ExactNearest;
        if (values.TryGetValue(WorldTreatmentOption, out string? worldTreatmentValue))
        {
            if (!string.Equals(worldTreatmentValue, "edge-scale2x", StringComparison.Ordinal))
            {
                return Unavailable(
                    profile,
                    privateSmokeRequested,
                    privateHudPreviewRequested,
                    "The private Map 3 world treatment is unknown.",
                    privateBaseAtlasRequested);
            }

            if (!privateBaseViewRequested || !privateBaseAtlasRequested)
            {
                return Unavailable(
                    profile,
                    privateSmokeRequested,
                    privateHudPreviewRequested,
                    "The edge-scale2x world treatment requires explicit private base-view and base-atlas selection.",
                    privateBaseAtlasRequested);
            }

            worldTreatment = PrivateMap3WorldTreatment.EdgeScale2x;
        }

        bool hasAnyPresentationValue = values.ContainsKey(PresentationAssetRootOption) ||
            values.ContainsKey(PresentationAssetCommitOption) ||
            values.ContainsKey(PresentationManifestDigestOption);
        bool presentationRequested = privateHudPreviewRequested || privateBaseAtlasRequested;
        string? presentationAssetRoot = null;
        string? presentationAssetCommit = null;
        string? presentationManifestDigest = null;
        if (!presentationRequested && hasAnyPresentationValue)
        {
            return Unavailable(
                profile,
                privateSmokeRequested,
                privateHudPreviewRequested,
                "Private presentation asset values require explicit HUD preview or Map 3 base-atlas selection.");
        }

        if (presentationRequested &&
            (!TryNormalizeRequiredPath(
                values.GetValueOrDefault(PresentationAssetRootOption),
                out presentationAssetRoot) ||
             !TryCanonicalCommit(
                values.GetValueOrDefault(PresentationAssetCommitOption),
                out presentationAssetCommit) ||
             !TryCanonicalSha256(
                values.GetValueOrDefault(PresentationManifestDigestOption),
                out presentationManifestDigest)))
        {
            return Unavailable(
                profile,
                privateSmokeRequested,
                privateHudPreviewRequested,
                "Private presentation requires one fully qualified asset root, canonical commit, and canonical manifest digest.",
                privateBaseAtlasRequested);
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
                    privateHudPreviewRequested,
                    "Private visual inputs require explicit private Map 3 base-view selection.")
                : Available(
                    profile.Value,
                    canonicalImportPath,
                    privateSmokeRequested,
                    privateHudPreviewRequested,
                    privateBaseAtlasRequested,
                    presentationAssetRoot,
                    presentationAssetCommit,
                    presentationManifestDigest);
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
                privateHudPreviewRequested,
                "Private Map 3 base view requires fully qualified ignored ROM, tileset-metadata, and palette-metadata paths.");
        }

        return new Map3RuntimeProfileSelection(
            profile,
            isAvailable: true,
            canonicalImportPath,
            privateSmokeRequested,
            privateBaseViewRequested: true,
            privateBaseAtlasRequested,
            privateStaticOverlayRequested,
            originalRomPath,
            tilesetMetadataPath,
            paletteMetadataPath,
            privateHudPreviewRequested,
            presentationAssetRoot,
            presentationAssetCommit,
            presentationManifestDigest,
            worldTreatment,
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
            PresentationAssetRootOption,
            PresentationAssetCommitOption,
            PresentationManifestDigestOption,
            WorldTreatmentOption,
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
        PresentationAssetRootOption => "presentation asset root",
        PresentationAssetCommitOption => "presentation asset commit",
        PresentationManifestDigestOption => "presentation manifest digest",
        WorldTreatmentOption => "private Map 3 world treatment",
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
        bool privateSmokeRequested,
        bool privateHudPreviewRequested = false,
        bool privateBaseAtlasRequested = false,
        string? presentationAssetRoot = null,
        string? presentationAssetCommit = null,
        string? presentationManifestDigest = null) =>
        new(
            profile,
            true,
            canonicalImportPath,
            privateSmokeRequested,
            privateBaseViewRequested: false,
            privateBaseAtlasRequested,
            privateStaticOverlayRequested: false,
            originalRomPath: null,
            tilesetMetadataPath: null,
            paletteMetadataPath: null,
            privateHudPreviewRequested,
            presentationAssetRoot,
            presentationAssetCommit,
            presentationManifestDigest,
            PrivateMap3WorldTreatment.ExactNearest,
            diagnostic: null);

    private static Map3RuntimeProfileSelection Unavailable(
        Map3RuntimeProfile? profile,
        bool privateSmokeRequested,
        bool privateHudPreviewRequested,
        string diagnostic,
        bool privateBaseAtlasRequested = false,
        bool privateStaticOverlayRequested = false) =>
        new(
            profile,
            false,
            canonicalImportPath: null,
            privateSmokeRequested,
            privateBaseViewRequested: false,
            privateBaseAtlasRequested,
            privateStaticOverlayRequested,
            originalRomPath: null,
            tilesetMetadataPath: null,
            paletteMetadataPath: null,
            privateHudPreviewRequested,
            presentationAssetRoot: null,
            presentationAssetCommit: null,
            presentationManifestDigest: null,
            PrivateMap3WorldTreatment.ExactNearest,
            diagnostic);

    private static bool TryCanonicalCommit(string? value, out string? canonical)
    {
        canonical = null;
        if (value is null || value.Length != 40 ||
            value.Any(character =>
                character is not (>= '0' and <= '9') and
                not (>= 'a' and <= 'f')))
        {
            return false;
        }

        canonical = value;
        return true;
    }

    private static bool TryCanonicalSha256(string? value, out string? canonical)
    {
        canonical = null;
        if (value is null || value.Length != 64 ||
            value.Any(character =>
                character is not (>= '0' and <= '9') and
                not (>= 'A' and <= 'F')))
        {
            return false;
        }

        canonical = value;
        return true;
    }

    private static Map3RuntimeProfile? ParseKnownProfile(string? value) => value switch
    {
        "public-synthetic" => Map3RuntimeProfile.PublicSynthetic,
        "private-local" => Map3RuntimeProfile.PrivateLocal,
        _ => null,
    };
}
