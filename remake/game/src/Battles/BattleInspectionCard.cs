using Godot;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleInspectionCard : PanelContainer
{
    private readonly Label _name = new(), _classLevel = new(), _land = new(), _location = new();
    private readonly BattleVitalGauge _hp = new("HP", miniStatus: true), _mp = new("MP", miniStatus: true);
    private BattleInspectionUnit? _unit;
    public override void _Ready()
    {
        Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Ignore;
        TextureFilter = TextureFilterEnum.Nearest;
        AddThemeStyleboxOverride("panel", StoryTheme.PanelFrame(18, 14));
        var column = new VBoxContainer(); AddChild(column);
        var heading = new HBoxContainer(); column.AddChild(heading); heading.AddChild(_name); heading.AddChild(_classLevel);
        _name.SizeFlagsHorizontal = SizeFlags.ExpandFill; _name.CustomMinimumSize = new(220, 0);
        _name.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _classLevel.AddThemeColorOverride("font_color", StoryTheme.Selected);
        _classLevel.AddThemeFontSizeOverride("font_size", 18);
        column.AddChild(_hp); column.AddChild(_mp);
        var terrain = new HBoxContainer(); column.AddChild(terrain); terrain.AddChild(_location); terrain.AddChild(_land);
        _location.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _land.HorizontalAlignment = HorizontalAlignment.Right;
        foreach (var text in new[] { _name, _land, _location }) text.AddThemeFontSizeOverride("font_size", 24);
    }
    internal void Present(BattleInspectionUnit? unit, string name, string land, MapPosition cell)
    {
        _unit = unit; _name.Text = name; _land.Text = land;
        _hp.Visible = _mp.Visible = unit is not null;
        _classLevel.Text = BattleStatusCard.ClassAndLevel(unit?.ClassId, unit is { Ally: true } ? unit.Level : null);
        _classLevel.Visible = _classLevel.Text.Length > 0;
        _location.Text = $"({cell.X}, {cell.Y})";
        _hp.Present(unit?.Hp, unit?.MaxHp); _mp.Present(unit?.Mp, unit?.MaxMp);
        _hp.AlignValueColumn(_mp);
        Size = new(560, 0);
    }
    internal object Observe() => new { unit = _unit?.Actor.Value, name = _name.Text, land = _land.Text, location = _location.Text,
        classLevel = _classLevel.Text, frame = GetThemeStylebox("panel") is StyleBoxTexture,
        rect = BattleMapViewport.Rectangle(GetGlobalRect()), nameRect = BattleMapViewport.Rectangle(_name.GetGlobalRect()),
        landRect = BattleMapViewport.Rectangle(_land.GetGlobalRect()), hp = "HP " + _hp.Reading, mp = "MP " + _mp.Reading,
        hpGauge = _hp.ObserveImage(), mpGauge = _mp.ObserveImage() };
}
