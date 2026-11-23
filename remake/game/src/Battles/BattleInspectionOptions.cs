using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleInspectionOptions : PanelContainer
{
    private readonly DiamondCommandButton[] _icons = [new("Member", "field-menu.member", "battlefield-menu"),
        new("Map", "battle.member.map", "battlefield-menu"), new("Speed", "battle.speed.title", "battlefield-menu"),
        new("Quit", "entry.quit", "battlefield-menu")];
    private CompactCommandList _list = null!;
    internal Action InvalidatePointer { get; set; } = () => { };
    public override void _Ready()
    {
        Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Ignore;
        var column = new VBoxContainer(); column.AddThemeConstantOverride("separation", 12); AddChild(column);
        _list = new(_icons) { InvalidatePointer = () => InvalidatePointer() }; column.AddChild(_list);
        ItemRectChanged += () => InvalidatePointer();
    }
    internal void Configure(IReadOnlyDictionary<string, ExplorationRaster>? rasters, string prefix) =>
        MenuRasterTextures.Configure(_icons, rasters, prefix, "battlefield-menu");
    internal void Present(GameInput input, int index, IReadOnlyList<bool>? available = null)
    {
        for (int i = 0; i < _icons.Length; i++)
        { _icons[i].RefreshCaption(); _icons[i].Available = available is null || available[i]; _icons[i].SetSelection(index == i, input.Settings.ReducedFlash); }
        _list.Present(_icons.Select(i => i.Caption).ToArray()); _list.CustomMinimumSize = new(528, _list.RequestedHeight);
        Size = new(556, 0); Position = new(GetViewportRect().Size.X - 32 - 556, 160);
    }
    internal int Hit(Vector2 p) => _list.Hit(p).Index;
    internal Rect2 HitRect(int index) => _list.Hit(_icons[index].GetGlobalRect().GetCenter()).Rect;
    internal void Suspend() { foreach (var i in _icons) i.StopFocusEmphasis(); InvalidatePointer(); }
    public override void _Process(double delta)
    { if (IsVisibleInTree()) foreach (var i in _icons) i.AdvanceFocus(delta); }
    internal object Observe() => new { visible = IsVisibleInTree(), rect = BattleMapViewport.Rectangle(GetGlobalRect()),
        commandList = _list.Observe() };
}
