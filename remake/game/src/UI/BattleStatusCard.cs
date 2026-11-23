using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.Application.Runtime.Battles;

namespace Sf2.Remake.GodotAdapter.UI;

// Disposable projection of the live combatant, used for decisions and close-ups.
internal sealed partial class BattleStatusCard : PanelContainer
{
    private const int BaseWidth = 500, HorizontalInsets = 36, HeaderGap = 14;
    internal bool DecisionHeader { get; init; }
    internal Vector2 RequestedSize { get; private set; } = new(BaseWidth, 152);
    private readonly Label _name = Text("CharacterName", 26), _role = Text("ClassLevel", 18);
    private readonly BattleVitalGauge _hp = new("HP", miniStatus: true), _mp = new("MP", miniStatus: true);
    private ActorRef? _actor;
    private int? _level;
    internal ActorRef? Actor => _actor;

    public override void _Ready()
    {
        Theme = StoryTheme.Create();
        MouseFilter = MouseFilterEnum.Ignore;
        TextureFilter = TextureFilterEnum.Nearest;
        AddThemeStyleboxOverride("panel", StoryTheme.PanelFrame(18, 14));
        var column = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        column.AddThemeConstantOverride("separation", 4);
        AddChild(column);
        var header = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        header.AddThemeConstantOverride("separation", HeaderGap);
        column.AddChild(header);
        _name.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _name.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _name.CustomMinimumSize = new(0, 40);
        _name.MaxLinesVisible = 2;
        _name.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        _role.VerticalAlignment = VerticalAlignment.Top;
        _role.AddThemeColorOverride("font_color", StoryTheme.Selected);
        header.AddChild(_name); header.AddChild(_role);
        column.AddChild(_hp); column.AddChild(_mp);
        if (!DecisionHeader) MinimumSizeChanged += () => Size = RequestedSize;
        Size = RequestedSize;
    }

    internal void Present(BattleActorState? actor, string name) =>
        Present(actor?.Actor, name, actor?.IsAlly == true ? actor.Definition.Growth?.ClassId : null,
            actor?.IsAlly == true ? actor.Level : null, actor?.Hp, actor?.MaxHp, actor?.Mp, actor?.MaxMp);

    internal void PresentActing(BattleActingStatus? status)
    {
        var unit = status?.Unit;
        Present(unit?.Actor, unit is null ? "" : DisplayNames.InspectionName(unit.Ally, unit.NameIndex, unit.Name, unit.CustomName),
            unit?.ClassId, unit?.Level, unit?.Hp, unit?.MaxHp, unit?.Mp, unit?.MaxMp);
    }

    internal void PresentInspected(BattleInspectionUnit? unit, string name) =>
        Present(unit?.Actor, name, unit?.ClassId, unit is { Ally: true } ? unit.Level : null,
            unit?.Hp, unit?.MaxHp, unit?.Mp, unit?.MaxMp);

    private static string ClassAndLevel(byte? classId, int? level)
    {
        if (level is not { } rank) return "";
        string levelText = GameText.Get("battle.card-level", rank);
        return classId is null ? levelText : DisplayNames.Class(classId) + " · " + levelText;
    }

    private void Present(ActorRef? actor, string name, byte? classId, int? level, int? hp, int? maxHp, int? mp, int? maxMp)
    {
        _actor = actor;
        _level = level;
        Visible = actor is not null;
        string role = ClassAndLevel(classId, level);
        _role.Visible = role.Length > 0;
        if (_name.Text != name || _role.Text != role)
        {
            _name.Text = name;
            _role.Text = role;
            MeasureName();
        }
        _role.Modulate = hp > 0 && maxHp > 0 && hp * 4 <= maxHp ? StoryTheme.Selected : Colors.White;
        _hp.Present(hp, maxHp); _mp.Present(mp, maxMp);
        _hp.AlignValueColumn(_mp);
        Size = RequestedSize;
    }

    private void MeasureName()
    {
        float badge = _role.Visible ? _role.GetMinimumSize().X + HeaderGap : 0;
        var font = _name.GetThemeFont("font");
        using var shaped = new TextParagraph
        {
            Width = BaseWidth - HorizontalInsets - badge,
            BreakFlags = TextServer.LineBreakFlag.Mandatory | TextServer.LineBreakFlag.WordBound | TextServer.LineBreakFlag.Adaptive,
        };
        int fontSize = 26;
        for (; fontSize >= 22; fontSize--)
        {
            shaped.Clear(); shaped.AddString(_name.Text, font, fontSize);
            if (shaped.GetLineCount() <= 2 || fontSize == 22) break;
        }
        float maxWidth = 800 - HorizontalInsets - badge;
        while (shaped.GetLineCount() > 2 && shaped.Width < maxWidth)
            shaped.Width = Mathf.Min(shaped.Width + 16, maxWidth);
        _name.AddThemeFontSizeOverride("font_size", fontSize);
        _name.CustomMinimumSize = new(0, Mathf.Max(40, Mathf.Ceil(shaped.GetSize().Y)));
        RequestedSize = new(shaped.Width + badge + HorizontalInsets, _name.CustomMinimumSize.Y + 112);
    }

    internal object Observe() => new
    {
        actor = _actor?.Value, visible = IsVisibleInTree(), name = _name.Text, role = _role.Text,
        level = _level, roleVisible = _role.IsVisibleInTree(), roleRect = BattleMapViewport.Rectangle(_role.GetGlobalRect()),
        rect = BattleMapViewport.Rectangle(GetGlobalRect()),
        frame = GetThemeStylebox("panel") is StyleBoxTexture,
        nameRect = BattleMapViewport.Rectangle(_name.GetGlobalRect()),
        nameLines = _name.GetLineCount(), nameFontSize = _name.GetThemeFontSize("font_size"),
        nameVisibleLines = _name.GetVisibleLineCount(), decisionHeader = DecisionHeader,
        hp = _hp.Observe(), mp = _mp.Observe(),
    };

    private static Label Text(string name, int size)
    {
        var label = new Label { Name = name, MouseFilter = MouseFilterEnum.Ignore };
        label.AddThemeFontSizeOverride("font_size", size);
        return label;
    }
}

internal sealed partial class BattleVitalGauge : HBoxContainer
{
    private readonly Label _label, _value;
    private readonly BattleGaugeImage _image;
    private int? _current, _maximum;
    internal BattleVitalGauge(string label, bool miniStatus = false)
    {
        Name = label; MouseFilter = MouseFilterEnum.Ignore;
        AddThemeConstantOverride("separation", 10);
        _label = new Label { Name = "Label", Text = label, CustomMinimumSize = new(42, miniStatus ? 36 : 30) };
        _value = new Label { Name = "Value", CustomMinimumSize = new(132, 30), HorizontalAlignment = HorizontalAlignment.Right };
        _image = new BattleGaugeImage(label) { Name = "Image", SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkCenter, CustomMinimumSize = new(220, 28) };
        foreach (var text in new[] { _label, _value }) text.AddThemeFontSizeOverride("font_size", miniStatus ? 25 : 22);
    }
    public override void _Ready() { AddChild(_label); AddChild(_image); AddChild(_value); }
    internal void Present(int? current, int? maximum)
    {
        _current = current; _maximum = maximum;
        _value.Text = Value(current, maximum);
        _image.Fraction = current is null || maximum is null or 0 ? 0 : Mathf.Clamp((float)current.Value / maximum.Value, 0, 1);
        _image.QueueRedraw();
    }
    internal static string Value(int? current, int? maximum) => maximum == 0 ? "—" :
        $"{current?.ToString() ?? GameText.Get("party.value-unavailable")}/{maximum?.ToString() ?? GameText.Get("party.value-unavailable")}";
    internal string Reading => _value.Text;
    internal void AlignValueColumn(BattleVitalGauge other)
    {
        float width = Mathf.Max(132, Mathf.Ceil(Mathf.Max(_value.GetMinimumSize().X, other._value.GetMinimumSize().X)));
        _value.CustomMinimumSize = other._value.CustomMinimumSize = new(width, 30);
    }
    internal object ObserveImage() => _image.Observe();
    internal object Observe() => new { label = _label.Text, current = _current, maximum = _maximum,
        text = _value.Text, fontSize = _value.GetThemeFontSize("font_size"),
        valueMinimumWidth = _value.GetMinimumSize().X, valueRect = BattleMapViewport.Rectangle(_value.GetGlobalRect()),
        image = ObserveImage() };
}

// Four small public-authored PNG segments, loaded through Godot's ordinary resource system.
internal sealed partial class BattleGaugeImage : Control
{
    private const string Root = "res://assets/ui/battle-status/";
    private readonly string _fillName;
    private Texture2D _frame = null!, _track = null!, _fill = null!;
    private Rect2 _drawnFill;
    private float _drawnFraction;
    private int _drawCount;
    internal float Fraction { get; set; }
    internal BattleGaugeImage(string vital) { _fillName = vital == "HP" ? "hp-fill.png" : "mp-fill.png"; }
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore; TextureFilter = TextureFilterEnum.Nearest;
        _frame = GD.Load<Texture2D>(Root + "frame.png");
        _track = GD.Load<Texture2D>(Root + "track.png");
        _fill = GD.Load<Texture2D>(Root + _fillName);
    }
    public override void _Draw()
    {
        DrawTextureRect(_frame, new Rect2(Vector2.Zero, Size), false);
        var inside = new Rect2(new Vector2(6, 6), Size - new Vector2(12, 12));
        DrawTextureRect(_track, inside, false);
        _drawnFraction = Fraction;
        _drawnFill = new(inside.Position, new(inside.Size.X * Fraction, inside.Size.Y));
        if (Fraction > 0) DrawTextureRectRegion(_fill, _drawnFill, new Rect2(0, 0, _fill.GetWidth() * Fraction, _fill.GetHeight()));
        _drawCount++;
    }
    internal object Observe() => new { frame = Root + "frame.png", track = Root + "track.png", fill = Root + _fillName,
        fraction = Fraction, drawnFraction = _drawnFraction, drawCount = _drawCount,
        mounted = _frame is not null && _track is not null && _fill is not null,
        rect = BattleMapViewport.Rectangle(GetGlobalRect()), drawnFill = BattleMapViewport.Rectangle(_drawnFill),
        filter = TextureFilter.ToString() };
}
