using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.UI;

// Freestanding source diamond plus a separate explanation and Back; callers own selection and dispatch.
internal sealed partial class DiamondMenuView : Control
{
    private readonly Control _diamond = new();
    private readonly Panel _detail = new();
    private readonly DiamondCommandButton[] _commands;
    private readonly string _prefix;
    internal DiamondMenuView(string prefix, params (string Id, string Caption)[] commands)
    {
        _prefix = prefix;
        _commands = commands.Select(command => new DiamondCommandButton(command.Id, command.Caption, prefix)).ToArray();
    }
    private readonly Label _selection = new() { MouseFilter = MouseFilterEnum.Ignore },
        _back = new() { MouseFilter = MouseFilterEnum.Ignore }, _title = new() { MouseFilter = MouseFilterEnum.Ignore },
        _hint = new() { MouseFilter = MouseFilterEnum.Ignore };
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore; AddChild(_diamond); AddChild(_detail);
        foreach (var label in new[] { _title, _selection, _hint, _back })
        { _detail.AddChild(label); label.AutowrapMode = TextServer.AutowrapMode.WordSmart; label.CustomMaximumSize = new(592, -1); }
        _selection.AddThemeFontSizeOverride("font_size", 28); _back.AddThemeFontSizeOverride("font_size", 28);
        _title.AddThemeFontSizeOverride("font_size", 28); _hint.AddThemeFontSizeOverride("font_size", 24);
        foreach (var command in _commands) _diamond.AddChild(command);
        Hide();
    }
    internal void Configure(IReadOnlyDictionary<string, ExplorationRaster>? rasters, string prefix = "") =>
        MenuRasterTextures.Configure(_commands, rasters, prefix, _prefix);
    internal void Present(GameInput input, int index, string title, string hint, string backKey = "prepare.back", IReadOnlyList<bool>? available = null)
    {
        DiamondMenuGeometry.Arrange(_diamond, _commands);
        for (int i = 0; i < _commands.Length; i++)
        { _commands[i].Available = available is null || available[i]; _commands[i].RefreshCaption(); _commands[i].SetSelection(i == index, input.Settings.ReducedFlash); }
        _title.Text = title; _selection.Text = _commands[index].Caption; _hint.Text = hint;
        _back.Text = input.CompactHint(GameAction.Cancel) + " · " + GameText.Get(backKey);
        float y = 24;
        foreach (var label in new[] { _title, _selection, _hint, _back })
        { label.Position = new(24, y); label.Size = new(592, 0); y += label.GetMinimumSize().Y + 16; }
        float height = Math.Max(DiamondMenuGeometry.Size.Y, y + 8);
        _detail.Position = new(320, 0); _detail.Size = new(640, height);
        _diamond.Position = new(0, height - DiamondMenuGeometry.Size.Y);
        Size = new(960, height); Position = new(32, GetViewportRect().Size.Y - height - 32);
    }
    internal int Hit(Vector2 position) => Array.FindIndex(_commands, command => command.GetGlobalRect().HasPoint(position));
    internal Rect2 HitRect(int index) => _commands[index].GetGlobalRect();
    internal bool BackHit(Vector2 position) => _back.GetGlobalRect().HasPoint(position);
    internal Rect2 BackRect => _back.GetGlobalRect();
    internal void Suspend() { foreach (var command in _commands) command.StopFocusEmphasis(); }
    public override void _Process(double delta) { if (IsVisibleInTree()) foreach (var command in _commands) command.AdvanceFocus(delta); }
    internal object Observe() => new { visible = IsVisibleInTree(), selection = _selection.Text, hint = _hint.Text,
        diamondRect = BattleMapViewport.Rectangle(_diamond.GetGlobalRect()), detailRect = BattleMapViewport.Rectangle(_detail.GetGlobalRect()),
        hintRect = BattleMapViewport.Rectangle(_hint.GetGlobalRect()), hintMinimumHeight = _hint.GetMinimumSize().Y,
        backRect = BattleMapViewport.Rectangle(_back.GetGlobalRect()), commands = _commands.Select(command => new
        { id = command.Identity, selected = command.Selected, enabled = command.Available,
            rect = BattleMapViewport.Rectangle(command.GetGlobalRect()), art = command.ObserveArt() }) };
}
