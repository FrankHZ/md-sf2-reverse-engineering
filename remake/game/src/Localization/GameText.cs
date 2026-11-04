using System.Globalization;
using System.Text.RegularExpressions;
using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Exploration;
using Sf2.Remake.Content.Localization;
using Sf2.Remake.GodotAdapter.UI;
using Sf2.Remake.Content.Scenarios;

namespace Sf2.Remake.GodotAdapter.Localization;

internal static class GameText
{
    internal sealed record RowStatus(string EffectiveLocale, string? Reason);
    internal sealed record Candidate(string Language, IReadOnlyDictionary<string, string> Rows, IReadOnlyDictionary<string, RowStatus> Status);
    private static readonly Dictionary<string, int> Arguments = new()
    {
        ["battle.result-continue"] = 0,
        ["battle.result-more"] = 0,
        ["battle.give-result"] = 3,
        ["battle.exchange-result"] = 4,
        ["battle.give-target"] = 4,
        ["battle.give-confirm"] = 2,
        ["battle.exchange-confirm"] = 3,
        ["battle.give-title"] = 0,
        ["battle.exchange-title"] = 1,
        ["battle.transaction-empty"] = 0,
        ["battle.transaction-item"] = 4,
        ["battle.give-item"] = 0,
        ["battle.give-recipient-help"] = 1,
        ["battle.discard-result"] = 2,
        ["battle.discard-confirm"] = 1,
        ["battle.discard-title"] = 0,
        ["battle.discard-equipped"] = 0,
        ["battle.discard-protected"] = 0,
        ["battle.discard-rare"] = 0,
        ["battle.retreat-spell"] = 3,
        ["battle.retreat-item"] = 2,
        ["battle.command.attack"] = 0,
        ["battle.command.magic"] = 0,
        ["battle.command.item"] = 0,
        ["battle.command.stay"] = 0,
        ["battle.acting"] = 0,
        ["battle.target"] = 0,
        ["battle.context"] = 2,
        ["battle.position"] = 0,
        ["battle.magic"] = 0,
        ["battle.items"] = 0,
        ["battle.choose-target"] = 0,
        ["battle.stay-here"] = 0,
        ["battle.move-help"] = 2,
        ["battle.target-help"] = 3,
        ["battle.choice-help"] = 1,
        ["battle.command-help"] = 1,
        ["battle.confirm-help"] = 2,
        ["battle.extra-help"] = 3,
        ["battle.move-detail"] = 0,
        ["battle.no-target"] = 0,
        ["battle.stay-detail"] = 0,
        ["battle.no-magic"] = 0,
        ["battle.no-items"] = 0,
        ["battle.empty"] = 0,
        ["battle.unavailable-tag"] = 0,
        ["battle.range"] = 2,
        ["battle.previous"] = 0,
        ["battle.next"] = 0,
        ["battle.confirm"] = 0,
        ["battle.back"] = 0,
        ["battle.defeated"] = 0,
        ["battle.low-hp"] = 0,
        ["battle.unavailable"] = 2,
        ["battle.stopped"] = 2,
        ["battle.recovery.movement"] = 0,
        ["battle.recovery.occupied"] = 0,
        ["battle.recovery.range"] = 0,
        ["battle.recovery.mp"] = 0,
        ["battle.recovery.empty"] = 0,
        ["battle.recovery.attack"] = 0,
        ["battle.recovery.target"] = 0,
        ["battle.recovery.unsupported"] = 0,
        ["battle.recovery.general"] = 0,
        ["battle.recovery.stop"] = 0,
        ["battle.in-progress"] = 0,
        ["battle.status.position"] = 0,
        ["battle.status.action"] = 0,
        ["battle.status.target"] = 0,
        ["battle.status.equipment"] = 0,
        ["battle.status.confirm"] = 0,
        ["battle.start-unavailable"] = 0,
        ["battle.equipment-current"] = 5,
        ["battle.equipment-title"] = 0,
        ["battle.no-equipment"] = 0,
        ["battle.equipment-row"] = 6,
        ["battle.aux-help"] = 4,
        ["battle.read-more"] = 1,
        ["prepare.recovery.custody"] = 0,
        ["prepare.recovery.settle"] = 0,
        ["prepare.recovery.member"] = 0,
        ["prepare.recovery.unsupported"] = 0,
        ["prepare.recovery.exchange"] = 0,
        ["prepare.recovery.retry"] = 0,
        ["party.prepare"] = 0,
        ["party.close"] = 0,
        ["party.kills"] = 0,
        ["party.defeats"] = 0,
        ["party.absent"] = 0,
        ["party.defeated"] = 0,
        ["party.unavailable"] = 0,
        ["party.value-unavailable"] = 0,
        ["party.gold"] = 1,
        ["party.magic"] = 0,
        ["party.items"] = 0,
        ["party.member"] = 0,
        ["party.name-unavailable"] = 0,
        ["party.portrait-unavailable"] = 0,
        ["party.class-unavailable"] = 0,
        ["party.none-learned"] = 0,
        ["party.spell-unavailable"] = 1,
        ["party.empty"] = 0,
        ["party.inventory-unavailable"] = 0,
        ["party.item-unavailable"] = 0,
        ["party.equipped"] = 0,
        ["party.broken"] = 0,
        ["party.stat.level"] = 0,
        ["party.stat.attack"] = 0,
        ["party.stat.defense"] = 0,
        ["party.stat.agility"] = 0,
        ["party.stat.exp"] = 0,
        ["party.stat.move"] = 0,
        ["party.hint"] = 7,
        ["prepare.empty"] = 0,
        ["prepare.title"] = 1,
        ["prepare.equipment"] = 1,
        ["prepare.equip"] = 0,
        ["prepare.give"] = 0,
        ["prepare.use"] = 0,
        ["prepare.magic"] = 0,
        ["prepare.drop"] = 0,
        ["prepare.back"] = 0,
        ["prepare.unarmed"] = 0,
        ["prepare.equipment-row"] = 3,
        ["prepare.confirm-equip"] = 2,
        ["prepare.give-item"] = 0,
        ["prepare.give-target"] = 0,
        ["prepare.exchange"] = 0,
        ["prepare.confirm-exchange"] = 2,
        ["prepare.confirm-give"] = 1,
        ["prepare.use-item"] = 0,
        ["prepare.use-target"] = 0,
        ["prepare.use-unsupported"] = 0,
        ["prepare.confirm-no-effect-item"] = 1,
        ["prepare.confirm-use"] = 1,
        ["prepare.choose-magic"] = 0,
        ["prepare.spell-row"] = 2,
        ["prepare.magic-target"] = 0,
        ["prepare.confirm-no-effect-spell"] = 1,
        ["prepare.confirm-cast"] = 2,
        ["prepare.drop-item"] = 0,
        ["prepare.cannot-drop"] = 0,
        ["prepare.drop-refused"] = 0,
        ["prepare.confirm-drop"] = 1,
        ["prepare.confirm"] = 0,
        ["prepare.cancel"] = 0,
        ["prepare.done"] = 0,
        ["prepare.no-effect"] = 0,
        ["prepare.unavailable"] = 2,
        ["prepare.failure"] = 4,
        ["prepare.hint"] = 4,
        ["entry.new-menu"] = 0,
        ["entry.more"] = 0,
        ["entry.more-hint"] = 1,
        ["entry.new"] = 0,
        ["entry.cancel"] = 0,
        ["entry.continue"] = 0,
        ["entry.quit"] = 0,
        ["entry.back"] = 0,
        ["entry.save"] = 0,
        ["entry.quit-menu"] = 0,
        ["entry.save-menu"] = 0,
        ["entry.retry"] = 0,
        ["entry.return"] = 0,
        ["entry.recover"] = 0,
        ["entry.inspect"] = 0,
        ["entry.reveal"] = 0,
        ["entry.press-start"] = 0,
        ["entry.normal"] = 0,
        ["entry.replacement"] = 0,
        ["entry.unavailable"] = 0,
        ["entry.assets"] = 0,
        ["entry.new-hint"] = 0,
        ["entry.journey-hint"] = 0,
        ["entry.retry-help"] = 0,
        ["entry.launch-help"] = 0,
        ["name.title"] = 0,
        ["name.empty"] = 0,
        ["name.help"] = 0,
        ["name.use"] = 0,
        ["name.hint"] = 0,
        ["name.too-long"] = 0,
        ["name.character"] = 0,
        ["name.default"] = 0,
        ["name.bowie-default"] = 0,
        ["slot.title.save"] = 0,
        ["slot.title.confirm"] = 0,
        ["slot.title.saved"] = 0,
        ["slot.title.quit"] = 0,
        ["slot.title.error"] = 0,
        ["slot.title.recover"] = 0,
        ["slot.title.default"] = 0,
        ["slot.matching"] = 0,
        ["slot.unreadable"] = 0,
        ["slot.unavailable"] = 0,
        ["slot.empty"] = 0,
        ["slot.primary-missing"] = 0,
        ["slot.backup-review"] = 0,
        ["slot.backup"] = 0,
        ["save.destination.new-game"] = 0,
        ["save.destination.map3-opening"] = 0,
        ["save.destination.map3-joined"] = 0,
        ["save.destination.battle01-return"] = 0,
        ["save.destination.unavailable"] = 0,
        ["save.choose"] = 0,
        ["save.quit-dirty"] = 0,
        ["save.quit-clean"] = 0,
        ["new.slots"] = 0,
        ["new.uncertain"] = 0,
        ["save.error.location"] = 0,
        ["save.error.compatibility"] = 0,
        ["save.error.uncertain"] = 0,
        ["save.error.storage"] = 0,
        ["save.error.empty"] = 0,
        ["save.error.busy"] = 0,
        ["save.error.stale"] = 0,
        ["save.error.config"] = 0,
        ["save.error.other"] = 0,
        ["entry.start"] = 1,
        ["entry.choose-hint"] = 2,
        ["entry.choose-tooltip"] = 2,
        ["entry.reveal-hint"] = 1,
        ["entry.read-hint"] = 2,
        ["entry.fail"] = 3,
        ["entry.fallback"] = 1,
        ["save.details"] = 1,
        ["save.offer"] = 1,
        ["slot.record"] = 4,
        ["slot.status"] = 3,
        ["save.overwrite"] = 1,
        ["save.confirm"] = 1,
        ["save.saved"] = 2,
        ["save.recovery"] = 4,
        ["save.tooltip"] = 1,
        ["slot.tooltip"] = 2,
        ["new.confirm"] = 2,
        ["new.start"] = 1,
        ["new.error"] = 1,
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
        ["church.menu"] = 1,
        ["church.offer"] = 4,
        ["church.raise"] = 0,
        ["church.poison"] = 0,
        ["church.stun"] = 0,
        ["church.cure"] = 0,
        ["church.promote"] = 0,
        ["church.save"] = 0,
        ["church.leave"] = 0,
        ["church.confirm"] = 0,
        ["church.decline"] = 0,
        ["church.continue"] = 0,
        ["church.none"] = 0,
        ["church.declined"] = 0,
        ["church.insufficient"] = 0,
        ["church.paid.raise"] = 1,
        ["church.paid.poison"] = 1,
        ["church.paid.stun"] = 1,
        ["church.curse"] = 0,
        ["church.promotion"] = 0,
        ["church.failure"] = 3,
        ["church.hint"] = 4,
        ["search.item"] = 2,
        ["search.full"] = 1,
        ["search.gold"] = 1,
        ["search.unavailable"] = 1,
        ["search.continue"] = 2,
        ["field.reveal"] = 1,
        ["field.more"] = 1,
        ["field.controls"] = 3,
        ["field.talk"] = 1,
        ["field.search"] = 1,
        ["localization.names"] = 0,
        ["localization.fallback"] = 1,
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
        ["ui.settings.partial"] = "Chinese display: entry, naming help, slots, Save/Quit, settings, Party/Prepare, battle commands/targets/equipment/status, Give/Drop/retreat, authored field text, HEAL, Church and Search. Missing source rows and untranslated names use English.",
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
    internal static DisplayCatalogAdmission EntryCatalog(EntryPresentation source, string? path)
    {
        try
        {
            if (string.IsNullOrEmpty(path) || !System.IO.Path.IsPathFullyQualified(path) ||
                new System.IO.FileInfo(path).Length > 256 * 1024) return new(null, "overlay-unavailable");
            return DisplayCatalogReader.Read(System.IO.File.ReadAllBytes(path), source);
        }
        catch (Exception error) when (error is System.IO.IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        { return new(null, "overlay-unavailable"); }
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
    internal static string Fallback(string prefix, bool includeKey = false)
    {
        var row = _current?.Status.FirstOrDefault(pair => pair.Key.StartsWith(prefix, StringComparison.Ordinal) && pair.Value.Reason is not null);
        return row?.Value?.Reason is { } reason ? Get("localization.fallback", includeKey ? reason + ": " + row.Value.Key : reason) : "";
    }
    internal static DisplayResult Search(FieldSearchResult result, GameSession session)
    {
        var source = session.Definition.Exploration!;
        var rows = new List<DisplayResult>();
        foreach (int id in result.Texts)
        {
            var names = new List<DisplayNameBinding>();
            if (source.TextTokens.TryGetValue(id, out var tokens))
                for (int i = 0; i < tokens.Count; i++)
                    if (tokens[i].Kind == ExplorationTextTokenKind.MemberName)
                    {
                        int member = tokens[i].Member ?? 0;
                        names.Add(new(i, member, CharacterNames.Member(session.Definition, session.Current.Story, member), null));
                    }
            var row = Language == "en" || _overlay is null ? DisplayCatalog.Canonical(id, source, names) : _overlay.Resolve(id, source, names);
            if (Language != "en" && _overlay is null && row.Reason is null) row = row with { Reason = "overlay-unavailable" };
            if (row.Reason is { } reason) Diagnose(reason, $"world.text/{id}");
            rows.Add(row);
        }
        string? key = result.Kind switch
        {
            FieldSearchKind.Item => "search.item", FieldSearchKind.Full => "search.full",
            FieldSearchKind.Gold => "search.gold", FieldSearchKind.Unavailable => "search.unavailable", _ => null,
        };
        if (key is not null)
        {
            if (result.Kind is FieldSearchKind.Item or FieldSearchKind.Full && (result.ItemWord is null || result.ItemName is null))
            {
                Diagnose("display-operands", key);
                return new("Text unavailable (display-operands).", "en", "display-operands");
            }
            string item = result.ItemName ?? "";
            string value = result.Kind switch
            {
                FieldSearchKind.Item => Get(key, PartyStatusView.MemberName(session.QueryPartyDetails()!.Members.Single(member => member.Carried.Actor == result.Recipient), session), item),
                FieldSearchKind.Full => Get(key, item), FieldSearchKind.Gold => Get(key, result.Gold!.Value),
                _ => Get(key, result.Message ?? ""),
            };
            var status = _current?.Status.GetValueOrDefault(key);
            rows.Add(new(value, status?.EffectiveLocale ?? "en", status?.Reason));
        }
        string? fallback = rows.FirstOrDefault(row => row.Reason is not null)?.Reason;
        return new(string.Join("\n", rows.Select(row => row.Text)), rows.Any(row => row.EffectiveLocale == "zh-CN") ? "zh-CN" : "en", fallback);
    }
    internal static string BattleActionLabel(BattleActionChoice choice) => choice.Empty ? Get("battle.empty") :
        choice.Action.ItemSlot is not null || choice.Action.Spell is not null ? choice.Label :
        choice.Action.Kind switch
        {
            BattleActionKind.Physical => Get("battle.command.attack"),
            BattleActionKind.Stay => Get("battle.command.stay"), _ => choice.Label,
        };
    internal static string BattleEquipmentLabel(BattleEquipmentChoice choice) =>
        choice.Slot == 4 ? Get("prepare.unarmed") : choice.Label;

    internal static string BattleFailure(SessionFailure failure, bool stopped = false)
    {
        string recovery = stopped ? "stop" : failure.Code switch
        {
            "movement-range" => "movement", "occupied-destination" => "occupied",
            "target-range" => "range", "insufficient-mp" => "mp", "empty-item-slot" => "empty",
            "physical-definition" => "attack", "no-healing-target" or "no-action-target" => "target",
            _ => failure.Kind == SessionFailureKind.UnsupportedCapability ? "unsupported" : "general",
        };
        return Get(stopped ? "battle.stopped" : "battle.unavailable", Get("battle.recovery." + recovery),
            $"{failure.Kind}: {failure.Code} ({failure.Field})");
    }

    private static void Diagnose(string reason, string key)
    { if (_diagnostics.Count < 128) _diagnostics.Add(reason + ":" + key); }
}
