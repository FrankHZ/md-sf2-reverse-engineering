using Godot;
using Sf2.Remake.GodotAdapter.Battles;

namespace Sf2.Remake.GodotAdapter.UI;

// F4 decision-only composition; existing full/field/close-up gauges keep their own layout.
internal sealed partial class BattleDecisionVitals : HBoxContainer
{
    private readonly Reading _hp = new("HP"), _mp = new("MP");
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        AddThemeConstantOverride("separation", 32);
        AddChild(_hp); AddChild(_mp);
    }
    internal void Present(int? hp, int? maxHp, int? mp, int? maxMp)
    { _hp.Present(hp, maxHp); _mp.Present(mp, maxMp); }
    internal object ObserveHp() => _hp.Observe();
    internal object ObserveMp() => _mp.Observe();

    private sealed partial class Reading : VBoxContainer
    {
        private readonly string _label;
        private readonly Label _value = new() { Name = "Value", MouseFilter = MouseFilterEnum.Ignore,
            CustomMinimumSize = new(232, 54) };
        private readonly BattleGaugeImage _image;
        private int? _current, _maximum;
        private string _text = "";
        internal Reading(string label)
        {
            Name = label; _label = label;
            MouseFilter = MouseFilterEnum.Ignore;
            AddThemeConstantOverride("separation", 1);
            _value.AddThemeFontSizeOverride("font_size", 36);
            _image = new(label) { Name = "Image", CustomMinimumSize = new(232, 28),
                CustomMaximumSize = new(232, 28), SizeFlagsHorizontal = SizeFlags.ShrinkBegin };
        }
        public override void _Ready() { AddChild(_value); AddChild(_image); }
        internal void Present(int? current, int? maximum)
        {
            _current = current; _maximum = maximum;
            _text = BattleVitalGauge.Value(current, maximum);
            _value.Text = _label + "  " + _text;
            _image.Fraction = current is null || maximum is null or 0 ? 0 : Mathf.Clamp((float)current.Value / maximum.Value, 0, 1);
            _image.QueueRedraw();
        }
        internal object Observe() => new { label = _label, current = _current, maximum = _maximum,
            text = _text, display = _value.Text, fontSize = _value.GetThemeFontSize("font_size"),
            valueMinimumWidth = _value.GetMinimumSize().X, valueRect = BattleMapViewport.Rectangle(_value.GetGlobalRect()),
            image = _image.Observe() };
    }
}
