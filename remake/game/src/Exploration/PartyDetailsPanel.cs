using Godot;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.UI;
using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Original member-screen grouping, with modern readable spacing and independent list scrolling.
internal sealed partial class PartyDetailsPanel : Panel
{
    private readonly Label _name = Text("MemberName", 32), _class = Text("Class", 24);
    private readonly string[] _titles = ["LEVEL", "ATTACK", "DEFENSE", "AGILITY", "EXP", "MOVE"];
    private readonly List<(Label Title, Label Value)> _stats = [];
    private readonly BattleVitalGauge _hp = new("HP"), _mp = new("MP");
    private readonly Label _unknownHp = Text("UnknownHP", 28), _unknownMp = Text("UnknownMP", 28);
    private readonly Label _magicTitle = Text("MagicTitle", 30), _itemsTitle = Text("ItemsTitle", 30);
    private readonly ScrollContainer _magic = Scroll("Magic"), _items = Scroll("Items");
    private readonly VBoxContainer _spells = new(), _inventory = new();
    private readonly List<(HBoxContainer Row, Label Text, IdentityIcon Icon, Label? Marker)> _spellRows = [], _itemRows = [];
    private readonly ItemDescriptionView _description = new() { Visible = false };
    internal int ItemSlot { get; private set; }
    private IdentityIcons _icons = null!;
    private ScenarioDefinition _definition = null!;
    internal void ConfigureIcons(IdentityIcons icons, ScenarioDefinition definition) { _icons = icons; _definition = definition; }
    private string? _actor;
    private int _slotCount;
    internal int FocusPane { get; set; }
    internal bool MemberLayout { get; init; }

    public override void _Ready()
    {
        Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Ignore;
        foreach (var control in new Control[] { _name, _class, _hp, _mp, _unknownHp, _unknownMp, _magicTitle, _itemsTitle, _magic, _items }) AddChild(control);
        foreach (string title in _titles)
        {
            var label = Text(title, 28); label.Text = title;
            var value = Text(title + "Value", 30); value.HorizontalAlignment = HorizontalAlignment.Right;
            _stats.Add((label, value)); AddChild(label); AddChild(value);
        }
        _name.AutowrapMode = _class.AutowrapMode = TextServer.AutowrapMode.WordSmart;

        _spells.SizeFlagsHorizontal = _inventory.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _spells.AddThemeConstantOverride("separation", 12); _inventory.AddThemeConstantOverride("separation", 12);
        _magic.AddChild(_spells); _items.AddChild(_inventory);
        AddChild(_description);
    }

    internal void Present(PartyMemberDetails? member, string name, GameSession session)
    {
        _actor = member?.Carried.Actor.Value; _name.Text = name;
        _class.Text = DisplayNames.Class(member) + (member?.Carried.Absent == true ? " · " + GameText.Get("party.absent") : member?.Carried.Hp == 0 ? " · " + GameText.Get("party.defeated") : "");
        string[] values = [Number(member?.Level), Number(member?.Attack), Number(member?.Defense), Number(member?.Agility),
            Number(member?.Carried.Exp), Number(member?.Move)];
        for (int i = 0; i < values.Length; i++)
        { _stats[i].Title.Text = GameText.Get("party.stat." + _titles[i].ToLowerInvariant()); _stats[i].Value.Text = values[i]; }
        _magicTitle.Text = GameText.Get("party.magic"); _itemsTitle.Text = GameText.Get("party.items");
        _hp.Visible = member?.MaxHp is not null; _mp.Visible = member?.MaxMp is not null;
        _unknownHp.Visible = !_hp.Visible; _unknownMp.Visible = !_mp.Visible;
        _unknownHp.Text = $"HP   {member?.Carried.Hp.ToString() ?? GameText.Get("party.value-unavailable")} / {GameText.Get("party.value-unavailable")}";
        _unknownMp.Text = $"MP   {member?.Carried.Mp.ToString() ?? GameText.Get("party.value-unavailable")} / {GameText.Get("party.value-unavailable")}";
        _hp.Present(member?.Carried.Hp, member?.MaxHp);
        _mp.Present(member?.Carried.Mp, member?.MaxMp);
        var spells = member?.Spells is not { } learned ? new[] { GameText.Get("party.value-unavailable") } : learned.Count == 0 ? [GameText.Get("party.none-learned")] :
            learned.Select(spell => spell.Name is { } spellName ? DisplayNames.Spell(spell.Spell, spellName, level: true) : GameText.Get("party.spell-unavailable", spell.Spell.Level)).ToArray();
        Rows(_spells, _spellRows, spells, member?.Spells?.Select(spell => IdentityIcons.Spell(_definition, spell.Spell)).ToArray() ?? []);
        PresentInventory(member?.Inventory);
        PresentDescription(session, member?.Inventory, member?.Deployment?.Definition);
        Arrange();
    }

    // Same detail widgets, projected from live public scalars rather than a carried-party snapshot.
    internal void PresentBattle(BattleInspectionUnit unit, string name, GameSession? memberSession = null, string condition = "")
    {
        _actor = unit.Actor.Value; _name.Text = name;
        _class.Text = (unit.Ally ? DisplayNames.Name("name.class", unit.ClassId,
            unit.ClassName ?? GameText.Get("party.class-unavailable")) : GameText.Get("battle.inspect.not-applicable")) +
            " · " + DisplayNames.InspectionStatus(unit.Status);
        if (condition.Length > 0) _class.Text += " · " + condition;
        string[] values = [Number(unit.Level), Number(unit.Attack), Number(unit.Defense), Number(unit.Agility),
            unit.Ally ? Number(unit.Exp) : GameText.Get("battle.inspect.not-applicable"), Number(unit.Move)];
        if (!unit.Ally) values[0] = GameText.Get("battle.inspect.not-applicable");
        for (int i = 0; i < values.Length; i++)
        { _stats[i].Title.Text = GameText.Get("party.stat." + _titles[i].ToLowerInvariant()); _stats[i].Value.Text = values[i]; }
        _hp.Show(); _mp.Show(); _unknownHp.Hide(); _unknownMp.Hide();
        _hp.Present(unit.Hp, unit.MaxHp); _mp.Present(unit.Mp, unit.MaxMp);
        _magicTitle.Text = GameText.Get("party.magic"); _itemsTitle.Text = GameText.Get("party.items");
        Rows(_spells, _spellRows, unit.Spells.Count == 0 ? [GameText.Get("party.none-learned")] :
            unit.Spells.Select(spell => DisplayNames.Spell(spell.Spell, spell.Name ?? GameText.Get("party.value-unavailable"), level: true)).ToArray(),
            unit.Spells.Select(spell => IdentityIcons.Spell(_definition, spell.Spell)).ToArray());
        PresentInventory(unit.Inventory);
        if (memberSession is not null)
            PresentDescription(memberSession, unit.Inventory, memberSession.Current.Battle.GetActor(unit.Actor).Definition);
        Arrange();
    }

    private void PresentInventory(IReadOnlyList<PartyInventorySlot>? inventory)
    {
        _slotCount = inventory?.Count ?? 0;
        Rows(_inventory, _itemRows, inventory?.Select(slot => $"{slot.Slot + 1}. " +
            (slot.Empty == true ? GameText.Get("party.empty") : slot.Empty is null ? GameText.Get("party.inventory-unavailable") : DisplayNames.Item(slot.ItemId, slot.Name ?? GameText.Get("party.item-unavailable"))) +
            (slot.Equipped ? " · " + GameText.Get("party.equipped") : "") + (slot.Broken ? " · " + GameText.Get("party.broken") : "")).ToArray() ?? [GameText.Get("party.inventory-unavailable")],
            inventory?.Select(slot => slot.ItemId is { } id ? IdentityIcons.Item(id) : null).ToArray() ?? []);
    }

    private void PresentDescription(GameSession session, IReadOnlyList<PartyInventorySlot>? inventory,
        BattleActorDefinition? holder)
    {
        var slot = inventory?.ElementAtOrDefault(ItemSlot);
        string text = FocusPane == 2
            ? ItemDescriptions.Read(session.Definition, session.Current, slot?.ItemId, holder, slot?.Broken == true) : "";
        _description.Present(text, (_actor, ItemSlot));
    }

    private void Rows(VBoxContainer column, List<(HBoxContainer Row, Label Text, IdentityIcon Icon, Label? Marker)> labels, string[] rows, string?[] identities)
    {
        bool selectable = MemberLayout && labels == _itemRows;
        while (labels.Count < rows.Length)
        {
            var label = Text("Entry" + labels.Count, 28);
            label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            if (selectable) label.VerticalAlignment = VerticalAlignment.Center;
            label.SizeFlagsHorizontal = SizeFlags.ExpandFill; label.CustomMinimumSize = new(0, selectable ? 88 : MemberLayout ? 80 : 56);
            var row = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            row.AddThemeConstantOverride("separation", 16);
            var icon = new IdentityIcon(MemberLayout ? 3 : 4);
            var iconCell = new Control { CustomMinimumSize = new(MemberLayout ? 48 : 64, MemberLayout ? 80 : 96), MouseFilter = MouseFilterEnum.Ignore };
            Label? marker = null;
            if (selectable)
            {
                marker = Text("Focus", 30);
                marker.VerticalAlignment = VerticalAlignment.Center;
                marker.CustomMinimumSize = new(20, 0);
                marker.AddThemeColorOverride("font_color", StoryTheme.Selected);
                row.AddChild(marker);
            }
            iconCell.AddChild(icon); row.AddChild(iconCell); row.AddChild(label);
            column.AddChild(row); labels.Add((row, label, icon, marker));
        }
        for (int i = 0; i < labels.Count; i++)
        {
            labels[i].Row.Visible = i < rows.Length;
            bool focused = selectable && i < _slotCount && FocusPane == 2 && i == ItemSlot;
            labels[i].Icon.Present(_icons, i < identities.Length ? identities[i] : null, focused);
            if (labels[i].Marker is { } marker)
            {
                marker.Text = focused ? "›" : "";
                labels[i].Text.Modulate = focused ? StoryTheme.Selected : Colors.White;
            }
            if (i < rows.Length) labels[i].Text.Text = rows[i];
        }
    }

    internal void Arrange()
    {
        if (!MemberLayout) { ArrangeInspection(); return; }
        float width = Size.X;
        float attributes = 280, magic = (width - 96 - attributes) * 0.43f, items = width - 96 - attributes - magic;
        float magicX = 48 + attributes, itemsX = magicX + magic + 24;
        _name.Position = new(208, 24); _name.Size = new(width - 752, 0);
        _class.Position = new(208, 48 + _name.GetMinimumSize().Y); _class.Size = new(width - 752, 0);
        foreach (var (control, y) in new[] { ((Control)_hp, 40), (_mp, 120), (_unknownHp, 40), (_unknownMp, 120) })
        { control.Position = new(width - 504, y); control.Size = new(480, 56); }
        // The list headings clear the vitals. Attributes also clear the portrait.
        float top = Math.Max(232, _class.GetRect().End.Y + 54);
        float statsTop = Math.Max(268, top);
        for (int i = 0; i < _stats.Count; i++)
        {
            _stats[i].Title.Position = new(24, statsTop + 52 * i); _stats[i].Title.Size = new(176, 48);
            _stats[i].Value.Position = new(200, statsTop + 52 * i); _stats[i].Value.Size = new(attributes - 176, 48);
        }
        _magicTitle.Position = new(magicX, top - 54); _magicTitle.Size = new(magic, 44);
        _itemsTitle.Position = new(itemsX, top - 54); _itemsTitle.Size = new(items, 44);
        // Reserve the reader beside the pack so wrapped item rows can use the full column.
        float descriptionTop = Size.Y - 24 - _description.ReadingHeight;
        _magic.Position = new(magicX, top); _magic.Size = new(magic, Math.Max(0, descriptionTop - 12 - top));
        _items.Position = new(itemsX, top); _items.Size = new(items, Math.Max(0, Size.Y - top - 24));
        _description.Position = new(magicX, descriptionTop); _description.Size = new(magic, _description.ReadingHeight);
        _magicTitle.Modulate = FocusPane == 1 ? StoryTheme.Selected : Colors.White;
        _itemsTitle.Modulate = FocusPane == 2 ? StoryTheme.Selected : Colors.White;
    }

    private void ArrangeInspection()
    {
        float half = (Size.X - 72) / 2;
        _name.Position = new(24, 16); _name.Size = new(Size.X - 48, 76);
        float classY = Math.Max(94, _name.GetRect().End.Y + 2);
        _class.Position = new(24, classY); _class.Size = new(Size.X - 48, 36);
        float headerExtra = Math.Max(0, _class.GetRect().End.Y - 130);
        Vector2 belowHeader = new(0, headerExtra);
        Vector2[] positions = [new(24, 146), new(48 + half, 146), new(48 + half, 218),
            new(48 + half, 290), new(24, 362), new(48 + half, 362)];
        for (int i = 0; i < _stats.Count; i++)
        {
            _stats[i].Title.Position = positions[i] + belowHeader; _stats[i].Title.Size = new(152, 48);
            _stats[i].Value.Position = positions[i] + belowHeader + new Vector2(156, 0); _stats[i].Value.Size = new(half - 156, 48);
        }
        foreach (var (control, y) in new[] { ((Control)_hp, 218), (_mp, 290), (_unknownHp, 218), (_unknownMp, 290) })
        { control.Position = new(24, y + headerExtra); control.Size = new(half, 48); }
        _magicTitle.Position = new(24, 442 + headerExtra); _magicTitle.Size = new(half, 44);
        _itemsTitle.Position = new(48 + half, 442 + headerExtra); _itemsTitle.Size = new(half, 44);
        _magic.Position = new(24, 498 + headerExtra); _magic.Size = new(half, Size.Y - 522 - headerExtra);
        _items.Position = new(48 + half, 498 + headerExtra); _items.Size = new(half, Size.Y - 522 - headerExtra);
        _magicTitle.Modulate = FocusPane == 1 ? StoryTheme.Selected : Colors.White;
        _itemsTitle.Modulate = FocusPane == 2 ? StoryTheme.Selected : Colors.White;
    }

    internal void ResetReading(bool keepItemSlot = false)
    {
        _magic.ScrollVertical = 0;
        if (keepItemSlot) return;
        ItemSlot = 0;
        _items.ScrollVertical = 0;
    }

    internal void SelectItemSlot(int slot)
    {
        if (_slotCount == 0) return;
        ItemSlot = (slot + _slotCount) % _slotCount;
        _items.EnsureControlVisible(_itemRows[ItemSlot].Row);
    }

    internal (int Slot, Rect2 Rect) HitItem(Vector2 point)
    {
        if (!MemberLayout || _slotCount == 0 || !_items.GetGlobalRect().HasPoint(point)) return (-1, default);
        int slot = _itemRows.FindIndex(row => row.Row.Visible && row.Row.GetGlobalRect().HasPoint(point));
        return slot < 0 ? (-1, default) : (slot, _itemRows[slot].Row.GetGlobalRect().Intersection(_items.GetGlobalRect()));
    }
    internal void ScrollFocused(int step) { if (FocusPane == 1) _magic.ScrollVertical += step; if (FocusPane == 2) _items.ScrollVertical += step; }
    internal void Wheel(Vector2 position, int step)
    {
        if (MemberLayout && _description.Wheel(position, step)) return;
        if (_magic.GetGlobalRect().HasPoint(position)) _magic.ScrollVertical += step;
        if (_items.GetGlobalRect().HasPoint(position)) _items.ScrollVertical += step;
    }
    internal object Card() => new { actor = _actor, name = _name.Text, visible = IsVisibleInTree(),
        rect = BattleMapViewport.Rectangle(GetGlobalRect()), hp = _hp.Observe(), mp = _mp.Observe() };
    internal object Observe() => new { rect = BattleMapViewport.Rectangle(GetGlobalRect()), name = _name.Text,
        nameRect = BattleMapViewport.Rectangle(_name.GetGlobalRect()), hpRect = BattleMapViewport.Rectangle(_hp.GetGlobalRect()),
        mpRect = BattleMapViewport.Rectangle(_mp.GetGlobalRect()), classText = _class.Text,
        stats = _stats.Select(pair => new { label = pair.Title.Text, value = pair.Value.Text,
            labelRect = BattleMapViewport.Rectangle(pair.Title.GetGlobalRect()), valueRect = BattleMapViewport.Rectangle(pair.Value.GetGlobalRect()) }),
        magic = List(_magic, _spells, _spellRows), items = List(_items, _inventory, _itemRows),
        itemSlot = MemberLayout && FocusPane == 2 && _slotCount > 0 ? ItemSlot : (int?)null, description = MemberLayout ? _description.Observe() : null,
        hpVisible = _hp.IsVisibleInTree(), mpVisible = _mp.IsVisibleInTree() };
    private static object List(ScrollContainer scroll, Control column, List<(HBoxContainer Row, Label Text, IdentityIcon Icon, Label? Marker)> rows) => new
    { rect = BattleMapViewport.Rectangle(scroll.GetGlobalRect()), scroll = scroll.ScrollVertical, height = column.Size.Y,
        entries = rows.Where(row => row.Row.Visible).Select(row => new { text = row.Text.Text,
            minimumHeight = row.Text.GetMinimumSize().Y,
            rect = BattleMapViewport.Rectangle(row.Text.GetGlobalRect()), rowRect = BattleMapViewport.Rectangle(row.Row.GetGlobalRect()),
            icon = row.Icon.Observe() }) };
    private static string Number(object? value) => value?.ToString() ?? GameText.Get("party.value-unavailable");
    private static Label Text(string name, int size)
    { var label = new Label { Name = name, MouseFilter = MouseFilterEnum.Ignore }; label.AddThemeFontSizeOverride("font_size", size); return label; }
    private static ScrollContainer Scroll(string name) => new() { Name = name, HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
        VerticalScrollMode = ScrollContainer.ScrollMode.Auto, FocusMode = FocusModeEnum.None };
}
