using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sf2.Remake.GodotAdapter.Input;

internal sealed record ActionBinding(string[] Keys, string[] Buttons, string[] Axes);

// Product settings only; gameplay never reads device bindings or reveal timing.
internal sealed class InputSettings
{
    public required int FormatVersion { get; set; }
    public string ConfirmCancel { get; set; } = "standard";
    public bool ReducedFlash { get; set; }
    public string Language { get; set; } = "en";
    public string TextMode { get; set; } = "instant";
    public double CharactersPerSecond { get; set; } = 40;
    public bool SuppressBattleHitCoinMessages { get; set; }
    public Dictionary<string, ActionBinding> Bindings { get; set; } = [];

    internal static InputSettings Load(string? path)
    {
        var settings = path is null ? new InputSettings { FormatVersion = 1 } :
            Parse(System.IO.File.ReadAllBytes(path));
        Validate(settings);
        return settings;
    }

    internal static InputSettings Parse(byte[] bytes)
    {
        var settings = JsonSerializer.Deserialize<InputSettings>(bytes.AsSpan().StartsWith(new byte[] { 0xef, 0xbb, 0xbf }) ? bytes.AsSpan(3) : bytes.AsSpan(), new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            }) ?? throw new ArgumentException("Settings must be an object.");
        Validate(settings);
        return settings;
    }

    internal static void Validate(InputSettings settings)
    {
        if (settings.FormatVersion != 1) throw new ArgumentException("formatVersion must be 1.");
        if (settings.ConfirmCancel is not ("standard" or "swapped"))
            throw new ArgumentException("confirmCancel must be standard or swapped.");
        if (settings.Language is not ("en" or "zh-CN")) throw new ArgumentException("language must be en or zh-CN.");
        if (settings.TextMode is not ("instant" or "adjustable"))
            throw new ArgumentException("textMode must be instant or adjustable.");
        if (!double.IsFinite(settings.CharactersPerSecond) || settings.CharactersPerSecond is < 1 or > 240)
            throw new ArgumentException("charactersPerSecond must be between 1 and 240.");
        if (settings.Bindings is null) throw new ArgumentException("bindings must be an object.");
    }

    internal InputSettings Draft(bool defaults = false)
    {
        var draft = defaults ? Load(null) : new InputSettings
        {
            FormatVersion = FormatVersion, ConfirmCancel = ConfirmCancel, ReducedFlash = ReducedFlash,
            Language = Language, TextMode = TextMode, CharactersPerSecond = CharactersPerSecond,
            SuppressBattleHitCoinMessages = SuppressBattleHitCoinMessages,
        };
        draft.Bindings = Bindings;
        return draft;
    }

    internal void Publish(InputSettings candidate)
    {
        ConfirmCancel = candidate.ConfirmCancel; ReducedFlash = candidate.ReducedFlash;
        Language = candidate.Language; TextMode = candidate.TextMode; CharactersPerSecond = candidate.CharactersPerSecond;
        Bindings = candidate.Bindings;
        SuppressBattleHitCoinMessages = candidate.SuppressBattleHitCoinMessages;
    }

    // Modern reveal presets, not original VInt timing. Merely opening retains custom cps.
    internal int? BattleSpeed => TextMode == "instant" ? 3 : CharactersPerSecond switch
    { 15 => 0, 30 => 1, 60 => 2, _ => null };

    internal void ChangeBattleSpeed(int direction)
    {
        int speed = BattleSpeed is { } current ? (current + direction + 4) % 4 : direction > 0 ? 0 : 3;
        TextMode = speed == 3 ? "instant" : "adjustable";
        if (speed != 3) CharactersPerSecond = new[] { 15.0, 30, 60 }[speed];
    }
}
