using Godot;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleTerrainPanel : PanelContainer
{
    private readonly Label _reading = new() { Name = "TerrainReading", MouseFilter = MouseFilterEnum.Ignore };

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _reading.AddThemeFontSizeOverride("font_size", StoryTheme.BodySize);
        AddChild(_reading);
    }

    internal void Present(BattleLandEffect land) =>
        Present(land.Position, GameText.Get("battle.inspect.land-percent", land.Percent));

    internal void Present(MapPosition cell, string value)
    {
        _reading.Text = GameText.Get("battle.land-effect", value, cell.X, cell.Y);
        Size = GetCombinedMinimumSize();
    }

    internal void Place(Vector2 corner, Rect2 subject, bool alignRight)
    {
        var position = corner;
        if (alignRight) position.X -= Size.X;
        if (subject.HasArea() && new Rect2(position, Size).Intersects(subject))
            position.Y = subject.End.Y + 12;
        Position = position;
    }

    internal object Observe() => new { text = _reading.Text, visible = IsVisibleInTree(),
        rect = BattleMapViewport.Rectangle(GetGlobalRect()) };
}
