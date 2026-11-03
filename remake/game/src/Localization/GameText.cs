using System.Globalization;
using System.Text.RegularExpressions;
using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Localization;

internal static class GameText
{
    internal sealed record RowStatus(string EffectiveLocale, string? Reason);
    internal sealed record Candidate(string Language, IReadOnlyDictionary<string, string> Rows, IReadOnlyDictionary<string, RowStatus> Status);
    private static readonly Dictionary<string, int> Arguments = new()
    {
        ["ui.settings.title"] = 0,
        ["ui.settings.language"] = 1,
        ["ui.settings.text"] = 1,
        ["ui.settings.instant"] = 0,
        ["ui.settings.gradual"] = 0,
        ["ui.settings.speed"] = 1,
        ["ui.settings.confirm"] = 1,
        ["ui.settings.standard"] = 0,
        ["ui.settings.swapped"] = 0,
        ["ui.settings.flash"] = 1,
        ["ui.settings.on"] = 0,
        ["ui.settings.off"] = 0,
        ["ui.settings.defaults"] = 0,
        ["ui.settings.apply"] = 0,
        ["ui.settings.cancel"] = 0,
        ["ui.settings.close"] = 0,
        ["ui.settings.help"] = 0,
        ["ui.settings.staged"] = 0,
        ["ui.settings.notice"] = 0,
        ["ui.settings.hint"] = 2,
        ["ui.settings.entry"] = 1,
        ["ui.settings.readonly"] = 0,
        ["ui.settings.storage"] = 0,
        ["ui.settings.invalid"] = 0,
        ["ui.settings.display"] = 0,
        ["ui.settings.partial"] = 0,
        ["ui.settings.overlay"] = 0,
        ["battle.heal.action"] = 3,
        ["battle.heal.recovery"] = 2,
    };
    private static readonly Dictionary<string, string> Recovery = new()
    {
        ["ui.settings.title"] = "Settings",
        ["ui.settings.language"] = "Language: {0}",
        ["ui.settings.text"] = "Text: {0}",
        ["ui.settings.instant"] = "Instant",
        ["ui.settings.gradual"] = "Gradual",
        ["ui.settings.speed"] = "Text speed: {0} characters/sec · Left/Right −/+10",
        ["ui.settings.confirm"] = "Confirm / Cancel: {0}",
        ["ui.settings.standard"] = "Standard",
        ["ui.settings.swapped"] = "Swapped",
        ["ui.settings.flash"] = "Reduced flash: {0}",
        ["ui.settings.on"] = "On",
        ["ui.settings.off"] = "Off",
        ["ui.settings.defaults"] = "Restore defaults (draft)",
        ["ui.settings.apply"] = "Apply",
        ["ui.settings.cancel"] = "Cancel",
        ["ui.settings.close"] = "Close",
        ["ui.settings.help"] = "Control help (English)",
        ["ui.settings.staged"] = "Defaults staged. Apply saves them; Cancel keeps current settings.",
        ["ui.settings.notice"] = "Changes take effect only after Apply. Text speed is used in Gradual mode. Chinese display keeps the existing seven-character ASCII naming rule. Other screens remain partly English.",
        ["ui.settings.hint"] = "{0}: Choose/change   •   {1}: Cancel/close",
        ["ui.settings.entry"] = "Settings / Controls · {0}",
        ["ui.settings.readonly"] = "This launch uses read-only preferences. Relaunch without --input-settings to save changes.",
        ["ui.settings.storage"] = "Preferences were not changed. Recovery details above remain in English.",
        ["ui.settings.invalid"] = "Preferences could not be validated. Current settings are retained.",
        ["ui.settings.display"] = "Language resources are unavailable (localization-resources). Current settings are retained.",
        ["ui.settings.partial"] = "Chinese pilot: settings, authored field text and HEAL only. Missing rows use English.",
        ["ui.settings.overlay"] = "Field translation unavailable (overlay-unavailable); English fallback. Preference retained.",
    };
    private static Candidate? _current;
    private static DisplayCatalog? _overlay;
    private static string? _overlayError;
    private static readonly HashSet<string> _diagnostics = new(StringComparer.Ordinal);
    internal static bool Ready => _current is not null;
    internal static string Language => _current?.Language ?? "en";
    internal static string? StartupError { get; private set; }
    internal static string Coverage => Language == "en" ? "" : Get(_overlayError is null ? "ui.settings.partial" : "ui.settings.overlay");
    internal static object? Observe() => _current is null ? null : new { language = Language, rows = _current.Status, diagnostics = Diagnostics, coverage = Coverage };
    internal static string[] Diagnostics => _diagnostics.Order(StringComparer.Ordinal).ToArray();
    internal static string Get(string key, params object[] args)
    {
        string? template;
        if (_current is null)
        {
            if (!Recovery.TryGetValue(key, out template)) return "Message unavailable (localization-resources).";
        }
        else if (!_current.Rows.TryGetValue(key, out template)) return "Message unavailable (localization-resources).";
        if (!Arguments.TryGetValue(key, out int count) || args.Length != count || args.Any(arg => arg is null))
        { Diagnose("display-operands", key); return "Message unavailable (display-operands)."; }
        return string.Format(CultureInfo.InvariantCulture, template, args);
    }
    internal static Candidate Prepare(string language)
    {
        if (language is not ("en" or "zh-CN")) throw new ArgumentException();
        var english = GD.Load<Translation>("res://localization/en.po") ?? throw new ArgumentException();
        var selected = language == "en" ? english : GD.Load<Translation>("res://localization/zh_CN.po") ?? throw new ArgumentException();
        var present = selected.GetMessageList().Select(key => key.ToString()).ToHashSet(StringComparer.Ordinal);
        var baseline = english.GetMessageList().Select(key => key.ToString()).ToHashSet(StringComparer.Ordinal);
        var rows = new Dictionary<string, string>(StringComparer.Ordinal);
        var status = new Dictionary<string, RowStatus>(StringComparer.Ordinal);
        foreach (var (key, count) in Arguments)
        {
            if (!baseline.Contains(key) || !Valid(english.GetMessage(key).ToString(), count))
                throw new ArgumentException();
            string value = selected.GetMessage(key).ToString();
            string? reason = !present.Contains(key) ? "missing-translation" : !Valid(value, count) ? "invalid-translation" : null;
            rows.Add(key, reason is null ? value : english.GetMessage(key).ToString());
            status.Add(key, new(reason is null ? language : "en", reason));
        }
        StoryTheme.PrepareFont(string.Concat(rows.Values));
        return new(language, rows, status);
    }
    private static bool Valid(string value, int count)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var indexes = Regex.Matches(value, @"\{([0-9]+)\}").Select(match => int.TryParse(match.Groups[1].Value, System.Globalization.NumberStyles.None, CultureInfo.InvariantCulture, out int index) ? index : -1).ToHashSet();
        return indexes.SetEquals(Enumerable.Range(0, count)) && !Regex.Replace(value, @"\{[0-9]+\}", "").Any(c => c is '{' or '}');
    }
    internal static void Publish(Candidate candidate)
    {
        _current = candidate;
        foreach (var (key, status) in candidate.Status) if (status.Reason is { } reason) Diagnose(reason, key);
        TranslationServer.SetLocale(candidate.Language == "zh-CN" ? "zh_CN" : "en");
    }
    internal static void Initialize(string requested)
    {
        StartupError = null;
        try { Publish(Prepare(requested)); }
        catch (ArgumentException)
        {
            StartupError = "Language resources unavailable (localization-resources); English recovery. Preference retained.";
            try { Publish(Prepare("en")); } catch (ArgumentException) { _current = null; }
        }
    }
    internal static void Bind(ScenarioDefinition source, string? path)
    {
        _overlay = null; _overlayError = null; _diagnostics.Clear();
        if (_current is { } current)
            foreach (var (key, status) in current.Status) if (status.Reason is { } reason) Diagnose(reason, key);
        if (string.IsNullOrEmpty(path)) { _overlayError = "overlay-unavailable"; return; }
        try
        {
            if (!System.IO.Path.IsPathFullyQualified(path) || new System.IO.FileInfo(path).Length > 256 * 1024) throw new ArgumentException();
            var admission = DisplayCatalogReader.Read(System.IO.File.ReadAllBytes(path), source);
            _overlay = admission.Catalog; _overlayError = admission.Error;
            foreach (var issue in _overlay?.Diagnostics ?? []) Diagnose(issue.Code, issue.Key);
            if (Language == "zh-CN" && _overlayError is not null) Diagnose(_overlayError, "world.text");
        }
        catch (Exception error) when (error is System.IO.IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        { _overlayError = "overlay-unavailable"; }
    }
    internal static DisplayResult Field(FieldTextWait wait, ExplorationDefinition definition)
    {
        DisplayResult result;
        if (Language == "en") result = DisplayCatalog.Canonical(wait, definition);
        else if (_overlay is null)
        {
            result = DisplayCatalog.Canonical(wait, definition);
            if (result.Reason is null) result = result with { Reason = "overlay-unavailable" };
        }
        else result = _overlay.Resolve(wait, definition);
        if (result.Reason is { } reason) Diagnose(reason, $"world.text/{wait.Text}");
        return result;
    }
    private static void Diagnose(string reason, string key)
    { if (_diagnostics.Count < 128) _diagnostics.Add(reason + ":" + key); }
}
