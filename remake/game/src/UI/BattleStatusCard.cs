using Godot;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Battles;

namespace Sf2.Remake.GodotAdapter.UI;

// Disposable projection of live actor state. No resource interpolation or gameplay clock.
internal sealed partial class BattleStatusCard : PanelContainer
{
    internal static readonly Vector2 CardSize = new(500, 192);
    internal Vector2 RequestedSize { get; private set; } = CardSize;
    private readonly Label _role = Text("Role", 18), _name = Text("CharacterName", 26);
    private readonly BattleVitalGauge _hp = new("HP"), _mp = new("MP");
    private ActorRef? _actor;

    public override void _Ready()
    {
        Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Ignore;
        var column = new VBoxContainer(); column.AddThemeConstantOverride("separation", 2);
        AddChild(column); column.AddChild(_role); column.AddChild(_name);
        _name.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _name.CustomMinimumSize = new(0, 64);
        _name.MaxLinesVisible = 2;
        _name.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        column.AddChild(_hp); column.AddChild(_mp);
        // A hidden wrapped label initially measures at width 1. Reset the requested
        // frame when native sorting publishes the real minimum (also on actor changes).
        MinimumSizeChanged += () => Size = RequestedSize;
        Size = RequestedSize;
    }

    internal void Present(BattleActorState? actor, string name, string role)
    {
        _actor = actor?.Actor; Visible = actor is not null;
        PresentName(name);
        bool low = actor is { Hp: > 0, MaxHp: > 0 } && actor.Hp * 4 <= actor.MaxHp;
        _role.Text = role + (actor?.Hp == 0 ? " · Defeated" : low ? " · ! Low HP" : "");
        _role.Modulate = low ? StoryTheme.Selected : Colors.White;
        Size = RequestedSize;
        _hp.Present(actor?.Hp ?? 0, actor?.MaxHp ?? 0);
        _mp.Present(actor?.Mp ?? 0, actor?.MaxMp ?? 0);
    }

    private void PresentName(string name)
    {
        if (_name.Text == name) return;
        _name.Text = name;
        var font = _name.GetThemeFont("font");
        using var shaped = new TextParagraph
        {
            Width = CardSize.X - 28,
            BreakFlags = TextServer.LineBreakFlag.Mandatory | TextServer.LineBreakFlag.WordBound | TextServer.LineBreakFlag.Adaptive,
        };
        // Native smart wrapping also accounts for word boundaries, unlike dividing
        // a single-line width by two. Keep complete long identifiers at >=22px.
        int fontSize = 26;
        for (; fontSize >= 22; fontSize--)
        {
            shaped.Clear(); shaped.AddString(name, font, fontSize);
            if (shaped.GetLineCount() <= 2 || fontSize == 22) break;
        }
        // At most 19 width steps. Ordinary 64-character IDs fit completely;
        // exceptional longer metadata stays within a finite two-line frame.
        const float maximumNameWidth = 800 - 28;
        while (shaped.GetLineCount() > 2 && shaped.Width < maximumNameWidth)
            shaped.Width = Mathf.Min(shaped.Width + 16, maximumNameWidth);
        RequestedSize = new(shaped.Width + 28, CardSize.Y);
        _name.AddThemeFontSizeOverride("font_size", fontSize);
    }

    internal object Observe() => new
    {
        actor = _actor?.Value, visible = IsVisibleInTree(), name = _name.Text, role = _role.Text,
        rect = BattleMapViewport.Rectangle(GetGlobalRect()),
        nameRect = BattleMapViewport.Rectangle(_name.GetGlobalRect()),
        nameLines = _name.GetLineCount(), nameFontSize = _name.GetThemeFontSize("font_size"),
        nameVisibleLines = _name.GetVisibleLineCount(),
        hp = _hp.Observe(), mp = _mp.Observe(),
    };

    private static Label Text(string name, int size)
    {
        var label = new Label { Name = name, MouseFilter = MouseFilterEnum.Ignore };
        label.AddThemeFontSizeOverride("font_size", size); return label;
    }
}

internal sealed partial class BattleVitalGauge : HBoxContainer
{
    private readonly Label _label, _value;
    private readonly BattleGaugeImage _image;
    private int _current, _maximum;
    internal BattleVitalGauge(string label)
    {
        Name = label; MouseFilter = MouseFilterEnum.Ignore;
        AddThemeConstantOverride("separation", 10);
        _label = new Label { Name = "Label", Text = label, CustomMinimumSize = new(42, 30) };
        _value = new Label { Name = "Value", CustomMinimumSize = new(132, 30), HorizontalAlignment = HorizontalAlignment.Right };
        _image = new BattleGaugeImage(label) { Name = "Image", SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkCenter, CustomMinimumSize = new(220, 28) };
        foreach (var text in new[] { _label, _value }) text.AddThemeFontSizeOverride("font_size", 22);
    }
    public override void _Ready() { AddChild(_label); AddChild(_image); AddChild(_value); }
    internal void Present(int current, int maximum)
    {
        _current = current; _maximum = maximum;
        _value.Text = maximum == 0 ? "—" : $"{current}/{maximum}";
        _image.Fraction = maximum == 0 ? 0 : Mathf.Clamp((float)current / maximum, 0, 1);
        _image.QueueRedraw();
    }
    internal object Observe() => new { label = _label.Text, current = _current, maximum = _maximum,
        text = _value.Text, image = _image.Observe(), valueRect = BattleMapViewport.Rectangle(_value.GetGlobalRect()) };
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
