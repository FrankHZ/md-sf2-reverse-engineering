using Godot;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Previews;

public partial class DesignPreview
{
    private const int BattleScene = 3;
    private const int ToolbarHeight = 72;
    private static readonly Rect2 BattleBounds = new(0, ToolbarHeight, 1920, 1080 - ToolbarHeight);
    private SubViewport? _battleViewport;
    private TextureRect? _battlePicture;
    private GameRoot? _battleHost;
    private bool BattleSample => _scene is BattleScene or InspectScene;
    private bool SampleCovered => _showMembers || _journey != JourneyPage.Closed;
    private string PreviewLanguage => _english ? "en" : "zh-CN";

    private void StartBattleSample()
    {
        // The board and all production canvases share local viewport coordinates.
        // The review toolbar has its own area above them.
        var viewport = new SubViewport
        {
            Name = "ProductionBattle", Size = (Vector2I)BattleBounds.Size,
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
        };
        AddChild(viewport);
        _battleViewport = viewport;
        _battlePicture = Put(this, new TextureRect
        {
            Name = "ProductionBattlePicture", Texture = viewport.GetTexture(),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, MouseFilter = MouseFilterEnum.Ignore,
        }, BattleBounds);

        string path = ProjectSettings.GlobalizePath("res://../content/authored/practice-yard.json");
        var host = GD.Load<PackedScene>("res://Main.tscn").Instantiate<GameRoot>();
        host.ConfigureBattlePreview(new AuthoredScenarioPackageReader(path),
            new InputSettings { FormatVersion = 1, Language = PreviewLanguage, ReducedFlash = _steadyIcons });
        _battleHost = host;
        viewport.AddChild(host);
        if (_scene == InspectScene && host.GetNodeOrNull<BattleSessionView>("BattleSessionView") is { StartupError: null } battle)
            battle.EnterInspection();
    }

    private void StopBattleSample()
    {
        if (_battleViewport is null) return;
        RemoveChild(_battlePicture!);
        _battlePicture!.QueueFree();
        RemoveChild(_battleViewport);
        _battleViewport.QueueFree();
        _battleViewport = null;
        _battlePicture = null;
        _battleHost = null;
        Localization.GameText.ClearWorld();
    }

    private bool RouteBattleSampleInput(InputEvent input)
    {
        if (_battleViewport is null) return false;
        if (SampleCovered)
        {
            if (input is not InputEventMouse) _battleHost!.TrackCoveredBattlePreviewInput(input);
            return false;
        }
        if (input is InputEventMouse mouse)
        {
            bool overBattle = BattleBounds.HasPoint(mouse.Position);
            // Motion and releases above the board also reach the production gesture
            // owner so a gesture cannot survive a move onto the toolbar.
            if (overBattle || mouse is InputEventMouseMotion or InputEventMouseButton { Pressed: false })
            {
                using var local = input.XformedBy(new Transform2D(0, -BattleBounds.Position));
                _battleViewport.PushInput(local, inLocalCoords: true);
            }
            return overBattle;
        }
        _battleViewport.PushInput(input, inLocalCoords: true);
        return true;
    }
}
