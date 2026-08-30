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
    private const string ProfilePrefix = ProfileOption + "=";
    private const string ImportOption = "--canonical-map-import";
    private const string ImportPrefix = ImportOption + "=";
    internal const string PrivateSmokeOption = "--private-map3-smoke";

    private Map3RuntimeProfileSelection(
        Map3RuntimeProfile? requestedProfile,
        bool isAvailable,
        string? canonicalImportPath,
        bool privateSmokeRequested,
        string? diagnostic)
    {
        RequestedProfile = requestedProfile;
        IsAvailable = isAvailable;
        CanonicalImportPath = canonicalImportPath;
        PrivateSmokeRequested = privateSmokeRequested;
        Diagnostic = diagnostic;
    }

    internal Map3RuntimeProfile? RequestedProfile { get; }

    internal bool IsAvailable { get; }

    internal string? CanonicalImportPath { get; }

    internal bool PrivateSmokeRequested { get; }

    internal string? Diagnostic { get; }

    internal static Map3RuntimeProfileSelection Parse(IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        string? profileValue = null;
        string? importPath = null;
        bool profileSeen = false;
        bool importSeen = false;
        bool malformedProfile = false;
        bool malformedImport = false;
        bool privateSmokeRequested = false;

        foreach (string argument in arguments)
        {
            ArgumentNullException.ThrowIfNull(argument);
            if (string.Equals(argument, PrivateSmokeOption, StringComparison.Ordinal))
            {
                privateSmokeRequested = true;
            }
            else if (string.Equals(argument, ProfileOption, StringComparison.Ordinal))
            {
                malformedProfile = true;
                profileSeen = true;
            }
            else if (argument.StartsWith(ProfilePrefix, StringComparison.Ordinal))
            {
                if (profileSeen)
                {
                    return Unavailable(
                        ParseKnownProfile(profileValue),
                        privateSmokeRequested,
                        "The runtime profile option must appear exactly once.");
                }

                profileSeen = true;
                profileValue = argument[ProfilePrefix.Length..];
            }
            else if (string.Equals(argument, ImportOption, StringComparison.Ordinal))
            {
                malformedImport = true;
                importSeen = true;
            }
            else if (argument.StartsWith(ImportPrefix, StringComparison.Ordinal))
            {
                if (importSeen)
                {
                    return Unavailable(
                        ParseKnownProfile(profileValue),
                        privateSmokeRequested,
                        "The canonical import option must appear exactly once.");
                }

                importSeen = true;
                importPath = argument[ImportPrefix.Length..];
            }
        }

        if (malformedProfile || malformedImport)
        {
            return Unavailable(
                ParseKnownProfile(profileValue),
                privateSmokeRequested,
                "Runtime profile options require explicit non-empty values.");
        }

        if (!profileSeen)
        {
            return importSeen || privateSmokeRequested
                ? Unavailable(
                    null,
                    privateSmokeRequested,
                    "Private runtime options require an explicit PrivateLocal profile selection.")
                : Available(Map3RuntimeProfile.PublicSynthetic, null, false);
        }

        Map3RuntimeProfile? profile = ParseKnownProfile(profileValue);
        if (profile is null)
        {
            return Unavailable(
                null,
                privateSmokeRequested,
                "The requested runtime profile is unknown.");
        }

        if (profile == Map3RuntimeProfile.PublicSynthetic)
        {
            return importSeen || privateSmokeRequested
                ? Unavailable(
                    profile,
                    privateSmokeRequested,
                    "PublicSynthetic cannot consume private runtime options.")
                : Available(profile.Value, null, false);
        }

        if (!importSeen || string.IsNullOrWhiteSpace(importPath))
        {
            return Unavailable(
                profile,
                privateSmokeRequested,
                "PrivateLocal requires one fully qualified ignored canonical import path.");
        }

        try
        {
            if (!Path.IsPathFullyQualified(importPath))
            {
                return Unavailable(
                    profile,
                    privateSmokeRequested,
                    "PrivateLocal requires one fully qualified ignored canonical import path.");
            }

            return Available(
                profile.Value,
                Path.GetFullPath(importPath),
                privateSmokeRequested);
        }
        catch (Exception error) when (
            error is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return Unavailable(
                profile,
                privateSmokeRequested,
                "PrivateLocal canonical import path syntax is invalid.");
        }
    }

    private static Map3RuntimeProfileSelection Available(
        Map3RuntimeProfile profile,
        string? canonicalImportPath,
        bool privateSmokeRequested) =>
        new(profile, true, canonicalImportPath, privateSmokeRequested, null);

    private static Map3RuntimeProfileSelection Unavailable(
        Map3RuntimeProfile? profile,
        bool privateSmokeRequested,
        string diagnostic) =>
        new(profile, false, null, privateSmokeRequested, diagnostic);

    private static Map3RuntimeProfile? ParseKnownProfile(string? value) => value switch
    {
        "public-synthetic" => Map3RuntimeProfile.PublicSynthetic,
        "private-local" => Map3RuntimeProfile.PrivateLocal,
        _ => null,
    };
}
