using Godot;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.UI;

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
    private readonly List<Label> _spellRows = [], _itemRows = [];
    private PartyMemberDetails? _member;
    internal int FocusPane { get; set; }

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
        _name.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _class.Text = "Class: unavailable";
        _magicTitle.Text = "MAGIC"; _itemsTitle.Text = "ITEMS";
        _spells.SizeFlagsHorizontal = _inventory.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _spells.AddThemeConstantOverride("separation", 12); _inventory.AddThemeConstantOverride("separation", 12);
        _magic.AddChild(_spells); _items.AddChild(_inventory);
    }

    internal void Present(PartyMemberDetails? member, string name)
    {
        _member = member; _name.Text = name;
        _class.Text = "Class: unavailable" + (member?.Carried.Absent == true ? " · Absent" : member?.Carried.Hp == 0 ? " · Defeated" : "");
        string[] values = [Number(member?.Level), Number(member?.Attack), Number(member?.Defense), Number(member?.Agility),
            Number(member?.Carried.Exp), Number(member?.Deployment?.Definition.Move)];
        for (int i = 0; i < values.Length; i++) _stats[i].Value.Text = values[i];
        _hp.Visible = member?.MaxHp is not null; _mp.Visible = member?.MaxMp is not null;
        _unknownHp.Visible = !_hp.Visible; _unknownMp.Visible = !_mp.Visible;
        _unknownHp.Text = $"HP   {member?.Carried.Hp.ToString() ?? "Unavailable"} / Unavailable";
        _unknownMp.Text = $"MP   {member?.Carried.Mp.ToString() ?? "Unavailable"} / Unavailable";
        if (member is not null && member.MaxHp is { } hp) _hp.Present(member.Carried.Hp, hp);
        if (member is not null && member.MaxMp is { } mp) _mp.Present(member.Carried.Mp, mp);
        var spells = member?.Spells is not { } learned ? new[] { "Unavailable" } : learned.Count == 0 ? ["None learned"] :
            learned.Select(spell => spell.Name ?? $"Name unavailable · Level {spell.Spell.Level}").ToArray();
        Rows(_spells, _spellRows, spells);
        Rows(_inventory, _itemRows, member?.Inventory.Select(slot => $"{slot.Slot + 1}. " +
            (slot.Empty == true ? "Empty" : slot.Empty is null ? "Inventory unavailable" : slot.Name ?? "Item name unavailable") +
            (slot.Equipped ? "\n    Equipped" : "") + (slot.Broken ? " · Broken" : "")).ToArray() ?? ["Inventory unavailable"]);
        Arrange();
    }

    private static void Rows(VBoxContainer column, List<Label> labels, string[] rows)
    {
        while (labels.Count < rows.Length)
        {
            var label = Text("Entry" + labels.Count, 28);
            label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            label.SizeFlagsHorizontal = SizeFlags.ExpandFill; label.CustomMinimumSize = new(0, 56);
            column.AddChild(label); labels.Add(label);
        }
        for (int i = 0; i < labels.Count; i++) { labels[i].Visible = i < rows.Length; if (i < rows.Length) labels[i].Text = rows[i]; }
    }

    internal void Arrange()
    {
        float half = (Size.X - 72) / 2;
        _name.Position = new(24, 16); _name.Size = new(Size.X - 48, 76);
        _class.Position = new(24, 94); _class.Size = new(Size.X - 48, 36);
        Vector2[] positions = [new(24, 146), new(48 + half, 146), new(48 + half, 218),
            new(48 + half, 290), new(24, 362), new(48 + half, 362)];
        for (int i = 0; i < _stats.Count; i++)
        {
            _stats[i].Title.Position = positions[i]; _stats[i].Title.Size = new(152, 48);
            _stats[i].Value.Position = positions[i] + new Vector2(156, 0); _stats[i].Value.Size = new(half - 156, 48);
        }
        foreach (var (control, y) in new[] { ((Control)_hp, 218), (_mp, 290), (_unknownHp, 218), (_unknownMp, 290) })
        { control.Position = new(24, y); control.Size = new(half, 48); }
        _magicTitle.Position = new(24, 442); _magicTitle.Size = new(half, 44);
        _itemsTitle.Position = new(48 + half, 442); _itemsTitle.Size = new(half, 44);
        _magic.Position = new(24, 498); _magic.Size = new(half, Size.Y - 522);
        _items.Position = new(48 + half, 498); _items.Size = new(half, Size.Y - 522);
        _magicTitle.Modulate = FocusPane == 1 ? StoryTheme.Selected : Colors.White;
        _itemsTitle.Modulate = FocusPane == 2 ? StoryTheme.Selected : Colors.White;
    }

    internal void ResetReading() { _magic.ScrollVertical = 0; _items.ScrollVertical = 0; }
    internal void ScrollFocused(int step) { if (FocusPane == 1) _magic.ScrollVertical += step; if (FocusPane == 2) _items.ScrollVertical += step; }
    internal void Wheel(Vector2 position, int step)
    { if (_magic.GetGlobalRect().HasPoint(position)) _magic.ScrollVertical += step; if (_items.GetGlobalRect().HasPoint(position)) _items.ScrollVertical += step; }
    internal object Card() => new { actor = _member?.Carried.Actor.Value, name = _name.Text, visible = IsVisibleInTree(),
        rect = BattleMapViewport.Rectangle(GetGlobalRect()), hp = _hp.Observe(), mp = _mp.Observe() };
    internal object Observe() => new { rect = BattleMapViewport.Rectangle(GetGlobalRect()), name = _name.Text,
        nameRect = BattleMapViewport.Rectangle(_name.GetGlobalRect()), hpRect = BattleMapViewport.Rectangle(_hp.GetGlobalRect()),
        mpRect = BattleMapViewport.Rectangle(_mp.GetGlobalRect()), classText = _class.Text,
        stats = _stats.Select(pair => new { label = pair.Title.Text, value = pair.Value.Text,
            labelRect = BattleMapViewport.Rectangle(pair.Title.GetGlobalRect()), valueRect = BattleMapViewport.Rectangle(pair.Value.GetGlobalRect()) }),
        magic = List(_magic, _spells, _spellRows), items = List(_items, _inventory, _itemRows),
        hpVisible = _hp.IsVisibleInTree(), mpVisible = _mp.IsVisibleInTree() };
    private static object List(ScrollContainer scroll, Control column, List<Label> rows) => new
    { rect = BattleMapViewport.Rectangle(scroll.GetGlobalRect()), scroll = scroll.ScrollVertical, height = column.Size.Y,
        entries = rows.Where(label => label.Visible).Select(label => new { text = label.Text, rect = BattleMapViewport.Rectangle(label.GetGlobalRect()) }) };
    private static string Number(object? value) => value?.ToString() ?? "Unavailable";
    private static Label Text(string name, int size)
    { var label = new Label { Name = name, MouseFilter = MouseFilterEnum.Ignore }; label.AddThemeFontSizeOverride("font_size", size); return label; }
    private static ScrollContainer Scroll(string name) => new() { Name = name, HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
        VerticalScrollMode = ScrollContainer.ScrollMode.Auto, FocusMode = FocusModeEnum.None };
}
