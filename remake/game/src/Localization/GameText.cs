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

internal static partial class GameText
{
    internal sealed record RowStatus(string EffectiveLocale, string? Reason);
    internal sealed record Candidate(string Language, IReadOnlyDictionary<string, string> Rows, IReadOnlyDictionary<string, RowStatus> Status);
    private static readonly Dictionary<string, string> Recovery = new()
    {
        ["guidance.start-day"] = "Start your day.",
        ["guidance.home"] = "Home",
        ["guidance.leave-home"] = "Next: Leave the house.",
        ["guidance.mother"] = "Next: Speak with your mother near home.",
        ["guidance.attend-school"] = "Go to school.",
        ["guidance.school"] = "School",
        ["guidance.school-courtyard"] = "Next: Join your classmates at school.",
        ["guidance.teacher"] = "Next: Speak with your teacher downstairs.",
        ["guidance.join-lesson"] = "Join the lesson.",
        ["guidance.return-class"] = "Next: Return to the classroom for the lesson.",
        ["guidance.visit-castle"] = "Visit Granseal Castle.",
        ["guidance.granseal-castle"] = "Granseal Castle",
        ["guidance.castle-north"] = "Next: Take the town’s north exit to the castle.",
        ["guidance.royal-quarters"] = "The King’s room",
        ["guidance.return-castle"] = "Next: Return to Granseal Castle.",
        ["guidance.join-astral"] = "Join Astral in the castle hall.",
        ["guidance.castle-hall"] = "Castle hall",
        ["guidance.astral-hall"] = "Next: Speak with Astral in the hall.",
        ["guidance.return-hall"] = "Next: Return through the castle to the main hall.",
        ["guidance.follow-astral"] = "Follow Astral to the tower.",
        ["guidance.ancient-tower"] = "Ancient Tower",
        ["guidance.west-stairs"] = "Next: Use the stairs on the west side of the castle.",
        ["guidance.west-tower-stairs"] = "Next: Continue through the western tower stairs.",
        ["guidance.tower-guard"] = "Next: Speak with the guard on the tower route.",
        ["guidance.enter-tower"] = "Enter the Ancient Tower.",
        ["guidance.north-tower-exit"] = "Next: Take the northern exit toward the tower.",
        ["guidance.tower-passage"] = "Next: Follow the passage to the tower entrance.",
        ["guidance.tower-inner"] = "Next: Continue into the tower.",
        ["guidance.return-king"] = "Return to see the King.",
        ["guidance.king-room"] = "Next: Go through the royal quarters to the King’s room.",
        ["guidance.reverse-tower"] = "Next: Retrace the tower passages toward the castle.",
        ["guidance.hear-commission"] = "Speak with the royal party.",
        ["guidance.royal-party"] = "Next: Approach the royal party in the hall.",
        ["guidance.fetch-hawel"] = "Find Hawel near Yeel.",
        ["guidance.find-hawel"] = "Find Hawel near Yeel.",
        ["guidance.yeel-area"] = "Yeel and its surroundings",
        ["guidance.west-town-exit"] = "Next: Leave Granseal by the western exit.",
        ["guidance.leave-castle"] = "Next: Leave the castle through town for the countryside.",
        ["guidance.toward-yeel"] = "Next: Travel toward Yeel outside Granseal.",
        ["guidance.ask-hawel"] = "Next: Ask the villagers about Hawel.",
        ["guidance.visit-yeel"] = "Next: Visit Yeel to ask for directions.",
        ["guidance.return-yeel"] = "Next: Return toward Yeel and Hawel’s house.",
        ["guidance.ancient-shrine-goal"] = "Go to the Ancient Shrine.",
        ["guidance.ancient-shrine"] = "Ancient Shrine",
        ["guidance.leave-house"] = "Next: Leave Hawel’s house to continue your journey.",
        ["guidance.south-house"] = "Next: Continue south from Hawel’s house.",
        ["guidance.return-countryside"] = "Next: Return to the countryside outside Hawel’s house.",
        ["guidance.escape-galam"] = "Find a way out of Galam.",
        ["guidance.galam-exit"] = "Galam Castle exit",
        ["guidance.west-castle-exit"] = "Next: Head toward the western castle exit.",
        ["guidance.return-galam-exit"] = "Next: Return to the castle grounds and look for a way out.",
        ["guidance.leave-cells"] = "Find a way out of the cells.",
        ["guidance.prison-cells"] = "Prison cells",
        ["guidance.other-prisoners"] = "Next: Approach the other prisoners.",
        ["guidance.prison-stairs"] = "Prison stairs",
        ["guidance.approach-stairs"] = "Next: Head toward the stairs outside the cells.",
        ["guidance.opened-shortcut"] = "Known shortcut",
        ["guidance.use-shortcut"] = "Next: Find the shortcut near the cells.",
        ["guidance.castle-kitchen"] = "Castle kitchen",
        ["guidance.tunnel-kitchen"] = "Next: Follow the tunnel to the kitchen.",
        ["guidance.leave-kitchen"] = "Next: Leave the kitchen for the castle grounds.",
        ["guidance.castle-interior-exit"] = "Next: Find the door back to the castle grounds.",
        ["guidance.return-granseal"] = "Return to Granseal.",
        ["guidance.granseal"] = "Granseal",
        ["guidance.south-galam-gate"] = "Next: Leave Galam through the southern gate.",
        ["guidance.south-drawbridge"] = "Next: Cross the drawbridge and take the south exit.",
        ["guidance.toward-granseal"] = "Next: Head for Granseal’s entrance.",
        ["guidance.return-granseal-route"] = "Next: Make your way back toward Granseal.",
        ["guidance.check-castle"] = "Find out what happened at Granseal Castle.",
        ["guidance.castle-people"] = "Next: Speak with the people in the castle hall.",
        ["guidance.rescue-at-tower"] = "Go to the Ancient Tower.",
        ["guidance.tower-portals"] = "Next: Continue through the tower’s connected passages.",
        ["guidance.tower-deep"] = "Next: Continue toward the inner tower.",
        ["guidance.return-from-tower"] = "Leave the tower with the royal party.",
        ["guidance.castle-exit"] = "Castle exit",
        ["guidance.cross-castle"] = "Next: Cross the castle on the way back to town.",
        ["guidance.escape-earthquake"] = "Get everyone to safety.",
        ["guidance.castle-south"] = "Next: Leave the castle through the southern exit.",
        ["guidance.reach-docks"] = "Reach the docks.",
        ["guidance.granseal-docks"] = "Granseal docks",
        ["guidance.east-safe-exit"] = "Next: Use the town’s safer eastern exit.",
        ["guidance.east-exit"] = "East exit",
        ["guidance.board-ship"] = "Depart by ship.",
        ["guidance.dock-man"] = "Next: Speak with the man beside the ship.",

        ["controls.acceleration.note"] = "Hold for exploration and battle at 1.5x; release returns to 1x. NPCs and automatic sequences share the rate. Reading, menu controls and music keep normal speed. After a menu, message, map change or focus loss, release and press again.",
        ["controls.acceleration.hold"] = "Hold: temporary 1.5x gameplay",
        ["ui.settings.title"] = "Settings",
        ["ui.settings.language"] = "Language: {0}",
        ["ui.settings.graphics"] = "Graphics: {0}",
        ["ui.settings.graphics-original"] = "Original",
        ["ui.settings.graphics-hd"] = "HD",
        ["ui.settings.graphics-coverage"] = "HD uses installed artwork for supported scenes and items. Other artwork uses Original.",
        ["ui.settings.graphics-error"] = "Graphics could not be prepared. Previous graphics and preferences are retained.",
        ["host.setting-description-5"] = "Apply switches this game immediately and saves the choice for the next launch.",
        ["guidance.heading"] = "Current objective",
        ["guidance.visit-hawel"] = "Visit Hawel with {0}.",
        ["guidance.hawel-outside-yeel"] = "Hawel’s house · outside Yeel",
        ["guidance.next-north"] = "Next: North exit",
        ["guidance.north-exit"] = "North exit",
        ["guidance.house-entrance"] = "House entrance",
        ["guidance.known-house"] = "Next: Hawel’s house entrance",
        ["guidance.route-unavailable"] = "Route not available",
        ["guidance.reading"] = "{0}: Read · {1}: Commands",
        ["ui.settings.guidance"] = "Modern guidance: {0}",
        ["ui.settings.debug"] = "Debug diagnostics: {0}",
        ["host.setting-description-11"] = "Shows text loading status and catalog counts in Settings. Off by default. Apply saves the choice; errors remain visible either way.",
        ["ui.settings.auto-victory"] = "Debug: auto-win battles: {0}",
        ["host.setting-description-12"] = "Automatically wins at the first available battle turn. Story scenes and field return play normally. Skipped actions give no combat rewards.",
        ["host.setting-description-10"] = "Shows the current objective in the field menu and known destinations in the world. Apply changes both together.",
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
        ["ui.settings.notice"] = "Choose Apply to save your changes.",
        ["ui.settings.hint"] = "{0}: Choose/change   •   {1}: Cancel/close",
        ["ui.settings.entry"] = "Settings / Controls · {0}",
        ["ui.settings.readonly"] = "This launch uses read-only preferences. Relaunch without --input-settings to save changes.",
        ["ui.settings.storage"] = "Preferences were not changed. Recovery details above remain in English.",
        ["ui.settings.invalid"] = "Preferences could not be validated. Current settings are retained.",
        ["ui.settings.display"] = "Language resources are unavailable (localization-resources). Current settings are retained.",
        ["ui.settings.partial"] = "Chinese support is partial. Untranslated dialogue and names remain in English.",
        ["ui.settings.overlay"] = "Field translation unavailable (overlay-unavailable); English fallback. Preference retained.",
    };
    private static Candidate? _current;
    private static DisplayCatalog? _overlay;
    private static string? _overlayError;
    private static object? _entryAdmission;
    private static string? _entryError;
    private static readonly HashSet<string> _diagnostics = new(StringComparer.Ordinal);
    internal static bool Ready => _current is not null;
    internal static string Language => _current?.Language ?? "en";
    internal static string? StartupError { get; private set; }
    internal static string Coverage => string.Join("\n", new[] {
        _overlay is null ? Get(_overlayError is null ? "ui.settings.world-unbound" : "ui.settings.overlay") : Get("ui.settings.catalog", _overlay.MessageCount, _overlay.NameCount),
        Get(_entryError is not null ? "ui.settings.entry-unavailable" : _entryAdmission is null ? "ui.settings.entry-unbound" : "ui.settings.entry-loaded"),
        Get("ui.settings.partial") });
    internal static object? Observe() => _current is null ? null : new { language = Language, rows = _current.Status, diagnostics = Diagnostics, coverage = Coverage,
        worldCatalog = new { messages = _overlay?.MessageCount ?? 0, names = _overlay?.NameCount ?? 0, error = _overlayError },
        entryCatalog = _entryAdmission, entryError = _entryError,
        nameDiagnostics = Diagnostics.Where(issue => issue.Contains(":name.", StringComparison.Ordinal)).ToArray() };
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
    internal static void ClearWorld()
    {
        _overlay = null; _overlayError = "overlay-unavailable"; DisplayNames.Clear();
        _diagnostics.Clear();
        if (_current is { } current)
            foreach (var (key, status) in current.Status) if (status.Reason is { } reason) Diagnose(reason, key);
    }
    internal static void Bind(ScenarioDefinition source, string? path)
    {
        ClearWorld(); _overlayError = null; DisplayNames.Bind(source, null);
        if (string.IsNullOrEmpty(path)) { _overlayError = "overlay-unavailable"; return; }
        try
        {
            if (!System.IO.Path.IsPathFullyQualified(path) || new System.IO.FileInfo(path).Length > 256 * 1024) throw new ArgumentException();
            var admission = DisplayCatalogReader.Read(System.IO.File.ReadAllBytes(path), source);
            _overlay = admission.Catalog; _overlayError = admission.Error;
            DisplayNames.Bind(source, _overlay);
            foreach (var issue in _overlay?.Diagnostics ?? []) Diagnose(issue.Code, issue.Key);
            if (Language == "zh-CN" && _overlayError is not null) Diagnose(_overlayError, "world.text");
        }
        catch (Exception error) when (error is System.IO.IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        { _overlayError = "overlay-unavailable"; }
    }
    internal static DisplayCatalogAdmission EntryCatalog(EntryPresentation source, string? path)
    {
        _entryAdmission = null; _entryError = "overlay-unavailable";
        try
        {
            if (string.IsNullOrEmpty(path) || !System.IO.Path.IsPathFullyQualified(path) ||
                new System.IO.FileInfo(path).Length > 256 * 1024) return new(null, "overlay-unavailable");
            var result = DisplayCatalogReader.Read(System.IO.File.ReadAllBytes(path), source);
            _entryAdmission = result.Catalog is { } catalog ? new { messages = catalog.MessageCount, names = catalog.NameCount } : null;
            _entryError = result.Error;
            return result;
        }
        catch (Exception error) when (error is System.IO.IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        { return new(null, "overlay-unavailable"); }
    }
    internal static DisplayResult Field(FieldTextWait wait, ExplorationDefinition definition)
    {
        DisplayResult result;
        if (Language == "en") result = DisplayCatalog.Canonical(wait, definition, reflow: true);
        else if (_overlay is null)
        {
            result = DisplayCatalog.Canonical(wait, definition, reflow: true);
            if (result.Reason is null) result = result with { Reason = "overlay-unavailable" };
        }
        else result = _overlay.Resolve(wait, definition, reflow: true);
        if (result.Reason is { } reason) Diagnose(reason, $"world.text/{wait.Text}");
        foreach (var issue in result.NameDiagnostics ?? []) Diagnose(issue.Code, issue.Key);
        return result;
    }
    internal static string Fallback(string prefix, bool includeKey = false, bool exact = false)
    {
        var row = _current?.Status.FirstOrDefault(pair => (exact ? pair.Key == prefix : pair.Key.StartsWith(prefix, StringComparison.Ordinal)) && pair.Value.Reason is not null);
        return row?.Value?.Reason is { } reason ? Get("localization.fallback", includeKey ? reason + ": " + row.Value.Key : reason) : "";
    }
    internal static string Unmigrated(string key)
    {
        Diagnose("unmigrated-consumer", key);
        return Get("localization.fallback", "unmigrated-consumer: " + key);
    }
    internal static DisplayResult Dialogue(DialogueWait wait, GameSession session) => WholeRow(wait.Text, session, null, reflow: true);
    private static DisplayResult WholeRow(int id, GameSession session, int? implicitMember, bool reflow = false,
        IReadOnlySet<int>? preservedBreaks = null)
    {
        var source = session.Definition.Exploration!;
        var names = new List<DisplayNameBinding>();
        if (source.TextTokens.TryGetValue(id, out var tokens))
            for (int i = 0; i < tokens.Count; i++)
                if (tokens[i].Kind == ExplorationTextTokenKind.MemberName &&
                    (tokens[i].Member ?? implicitMember) is { } member)
                {
                    var actor = session.Definition.Encounters.Values.SelectMany(encounter => encounter.Deployments)
                        .FirstOrDefault(row => row.Faction == BattleFaction.Ally && row.Initialization?.AllyPartyMember == member)?.Actor;
                    names.Add(new(i, member, CharacterNames.Member(session.Definition, new Dictionary<string, string>(), member),
                        actor is { } idActor ? session.Current.Story.Names.GetValueOrDefault(idActor.Value) : null, Actor: actor));
                }
        // Legacy dialogue has no captured bare NAME/CLASS operand. Missing bindings use display-operands fallback.
        var row = Language == "en" || _overlay is null ? DisplayCatalog.Canonical(id, source, names, reflow, preservedBreaks) :
            _overlay.Resolve(id, source, names, reflow, preservedBreaks);
        if (Language != "en" && _overlay is null && row.Reason is null) row = row with { Reason = "overlay-unavailable" };
        if (row.Reason is { } reason) Diagnose(reason, $"world.text/{id}");
        foreach (var issue in row.NameDiagnostics ?? []) Diagnose(issue.Code, issue.Key);
        return row;
    }
    internal static DisplayResult Search(FieldSearchResult result, GameSession session)
    {
        var rows = result.Texts.Select(id =>
        {
            var preserved = SourceTextLayout.SearchBreaks(id, session.Definition.Exploration!);
            return WholeRow(id, session, 0, reflow: preserved is not null, preservedBreaks: preserved);
        }).ToList();
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
            string item = result.ItemWord is { } word ? DisplayNames.ItemWord(word, result.ItemName ?? "") : result.ItemName ?? "";
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

    internal static void Diagnose(string reason, string key)
    { if (_diagnostics.Count < 128) _diagnostics.Add(reason + ":" + key); }
}
