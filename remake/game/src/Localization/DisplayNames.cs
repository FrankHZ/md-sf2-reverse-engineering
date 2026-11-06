using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Localization;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.GodotAdapter.Localization;

// Display identity only. Canonical strings, explicit overrides and committed item words stay separate.
internal static class DisplayNames
{
    private static ScenarioDefinition? _source;
    private static DisplayCatalog? _catalog;
    internal static void Clear() { _source = null; _catalog = null; }
    internal static void Bind(ScenarioDefinition source, DisplayCatalog? catalog) { _source = source; _catalog = catalog; }
    internal static string Name(string space, int? index, string canonical, string? custom = null)
    {
        if (custom is not null) return custom;
        if (GameText.Language == "en" || _source?.PrivateDefinitions is null) return canonical;
        var display = _catalog?.ResolveName(space, index, canonical) ?? new(canonical, "en", "overlay-unavailable");
        if (display.Reason is { } reason) GameText.Diagnose(reason, $"{space}/{index?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "unknown"}");
        return display.Text;
    }
    internal static string Member(ScenarioDefinition definition, StoryState story, int member) => Member(definition, story.Names, member);
    internal static string Member(ScenarioDefinition definition, IReadOnlyDictionary<string, string> overrides, int member)
    {
        var actor = definition.Encounters.Values.SelectMany(encounter => encounter.Deployments)
            .FirstOrDefault(row => row.Faction == BattleFaction.Ally && row.Initialization?.AllyPartyMember == member)?.Actor;
        return Name("name.ally", member, CharacterNames.Member(definition, new Dictionary<string, string>(), member),
            actor is { } id ? overrides.GetValueOrDefault(id.Value) : null);
    }
    internal static string DefaultMember(int member) => _source is { } source
        ? Member(source, new Dictionary<string, string>(), member) : GameText.Get("name.default");
    internal static string Battle(StoryState story, BattleActorState actor, BattleSceneDefinition? content)
    {
        if (story.Names.TryGetValue(actor.Actor.Value, out var custom)) return custom;
        string canonical = CharacterNames.Battle(story, actor, content);
        int? index = actor.IsAlly ? actor.Deployment.Initialization?.AllyPartyMember :
            content?.EnemyNameIds?.TryGetValue(actor.Actor, out byte enemy) == true ? enemy : null;
        return Name(actor.IsAlly ? "name.ally" : "name.enemy", index, canonical);
    }
    internal static string Item(int? id, string canonical) => Name("name.item", id, canonical);
    internal static string ItemWord(ushort word, string canonical) => Item(word & 127, canonical);
    internal static string Spell(SpellRef spell, string? canonical = null, bool level = false)
    {
        var definition = _source?.PrivateDefinitions?.Spells.GetValueOrDefault(spell);
        string fallback = canonical ?? spell.Value.ToUpperInvariant();
        if (GameText.Language == "en" || _source?.PrivateDefinitions is null) return fallback;
        string name = Name("name.spell", definition?.BaseId, definition?.DisplayName ?? fallback);
        // Source display names are base names. The caller owns the level, never an English-text parser.
        return level && definition is not null ? $"{name} {spell.Level}" : definition is null ? fallback : name;
    }
    internal static string Class(PartyMemberDetails? member)
    {
        byte? id = member?.Deployment?.Definition.Growth?.ClassId;
        string? expression = id is { } key ? _source?.PrivateDefinitions?.Classes.GetValueOrDefault(key)?.NameExpression : null;
        string canonical = expression is { Length: >= 2 } && expression[0] == '"' && expression[^1] == '"'
            ? expression[1..^1] : GameText.Get("party.class-unavailable");
        return Name("name.class", id, canonical);
    }
    internal static string BattleItem(SessionSnapshot current, int slot, string canonical)
    {
        var actor = current.Selection is { } selection ? current.Battle.GetActor(selection.Actor) : null;
        var items = actor?.SourceLoadout?.Items;
        int? id = items is not null && slot >= 0 && slot < items.Count ? items[slot] & 127 : null;
        return id == 127 ? GameText.Get("battle.empty") : Item(id, canonical);
    }
    internal static string Action(BattleActionChoice choice, SessionSnapshot current) => choice.Empty ? GameText.Get("battle.empty") :
        choice.Action.Spell is { } spell ? Spell(spell, choice.Label, level: true) :
        choice.Action.ItemSlot is { } slot ? BattleItem(current, slot, choice.Label) : GameText.BattleActionLabel(choice);
    internal static string Equipment(BattleEquipmentChoice choice, SessionSnapshot current) => choice.Slot == 4
        ? GameText.Get("prepare.unarmed") : BattleItem(current, choice.Slot, choice.Label);
}
