using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sf2.Remake.GodotAdapter.Input;

internal sealed record ActionBinding(string[] Keys, string[] Buttons, string[] Axes);

// Product settings only; gameplay never reads device bindings or reveal timing.
internal sealed class InputSettings
{
    public required int FormatVersion { get; init; }
    public string ConfirmCancel { get; init; } = "standard";
    public bool ReducedFlash { get; init; }
    public string TextMode { get; init; } = "instant";
    public double CharactersPerSecond { get; init; } = 40;
    public Dictionary<string, ActionBinding> Bindings { get; init; } = [];

    internal static InputSettings Load(string? path)
    {
        var settings = path is null ? new InputSettings { FormatVersion = 1 } :
            JsonSerializer.Deserialize<InputSettings>(System.IO.File.ReadAllText(path), new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            }) ?? throw new ArgumentException("Settings must be an object.");
        if (settings.FormatVersion != 1) throw new ArgumentException("formatVersion must be 1.");
        if (settings.ConfirmCancel is not ("standard" or "swapped"))
            throw new ArgumentException("confirmCancel must be standard or swapped.");
        if (settings.TextMode is not ("instant" or "adjustable"))
            throw new ArgumentException("textMode must be instant or adjustable.");
        if (!double.IsFinite(settings.CharactersPerSecond) || settings.CharactersPerSecond is < 1 or > 240)
            throw new ArgumentException("charactersPerSecond must be between 1 and 240.");
        if (settings.Bindings is null) throw new ArgumentException("bindings must be an object.");
        return settings;
    }
}
