using Godot;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleInspectionCard : PanelContainer
{
    private readonly Label _name = new(), _land = new(), _location = new();
    private readonly Label _hp = new(), _mp = new();
    private readonly BattleGaugeImage _hpGauge = new("HP"), _mpGauge = new("MP");
    private readonly HBoxContainer _vitals = new();
    private BattleInspectionUnit? _unit;
    public override void _Ready()
    {
        Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Ignore;
        var column = new VBoxContainer(); AddChild(column);
        var heading = new HBoxContainer(); column.AddChild(heading); heading.AddChild(_name); heading.AddChild(_land);
        _name.SizeFlagsHorizontal = SizeFlags.ExpandFill; _name.CustomMinimumSize = new(220, 0);
        _name.AutowrapMode = TextServer.AutowrapMode.WordSmart; _name.CustomMaximumSize = new(260, -1);
        _land.HorizontalAlignment = HorizontalAlignment.Right;
        column.AddChild(_vitals); _vitals.AddThemeConstantOverride("separation", 20);
        foreach (var pair in new[] { (_hp, _hpGauge), (_mp, _mpGauge) })
        {
            var vital = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsStretchRatio = 1 };
            _vitals.AddChild(vital); vital.AddChild(pair.Item1); vital.AddChild(pair.Item2);
            pair.Item2.CustomMinimumSize = new(250, 28);
        }
        column.AddChild(_location);
        foreach (var text in new[] { _name, _land, _hp, _mp, _location }) text.AddThemeFontSizeOverride("font_size", 24);
    }
    internal void Present(BattleInspectionUnit? unit, string name, string land, MapPosition cell)
    {
        _unit = unit; _name.Text = name; _land.Text = land;
        _vitals.Visible = unit is not null;
        _location.Text = $"({cell.X}, {cell.Y})";
        _hp.Text = $"HP {Value(unit?.Hp)}/{Value(unit?.MaxHp)}";
        _mp.Text = unit?.MaxMp == 0 ? "MP —" : $"MP {Value(unit?.Mp)}/{Value(unit?.MaxMp)}";
        _hpGauge.Fraction = Fraction(unit?.Hp, unit?.MaxHp); _mpGauge.Fraction = Fraction(unit?.Mp, unit?.MaxMp);
        _hpGauge.QueueRedraw(); _mpGauge.QueueRedraw(); Size = new(560, 0);
    }
    private static string Value(int? value) => value?.ToString() ?? GameText.Get("party.value-unavailable");
    private static float Fraction(int? value, int? maximum) => value is null || maximum is null or 0 ? 0 : Mathf.Clamp((float)value / maximum.Value, 0, 1);
    internal object Observe() => new { unit = _unit?.Actor.Value, name = _name.Text, land = _land.Text, location = _location.Text,
        rect = BattleMapViewport.Rectangle(GetGlobalRect()), nameRect = BattleMapViewport.Rectangle(_name.GetGlobalRect()),
        landRect = BattleMapViewport.Rectangle(_land.GetGlobalRect()), hp = _hp.Text, mp = _mp.Text,
        hpGauge = _hpGauge.Observe(), mpGauge = _mpGauge.Observe() };
}
