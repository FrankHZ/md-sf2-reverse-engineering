using Godot;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleTerrainPanel : PanelContainer
{
    internal const int CardGap = 12;
    private readonly Label _reading = new() { Name = "TerrainReading", MouseFilter = MouseFilterEnum.Ignore };

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _reading.AddThemeFontSizeOverride("font_size", StoryTheme.BodySize);
        AddChild(_reading);
    }

    internal void SetReadingSize(bool large) =>
        _reading.AddThemeFontSizeOverride("font_size", ReadingSize.Pixels(StoryTheme.BodySize, large, secondary: true));

    internal void Present(BattleLandEffect land) =>
        Present(land.Position, GameText.Get("battle.inspect.land-percent", land.Percent));

    internal void Present(MapPosition cell, string value)
    {
        _reading.Text = GameText.Get("battle.land-effect", value, cell.X, cell.Y);
        Size = GetCombinedMinimumSize();
    }

    internal object Observe() => new { text = _reading.Text, visible = IsVisibleInTree(),
        rect = BattleMapViewport.Rectangle(GetGlobalRect()) };
}
