using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Disposable focus and gestures only. Existing session queries and consumers own every action.
internal sealed partial class FieldMenuView : Control
{
    private readonly Panel _panel = new() { Name = "Panel" };
    private readonly ColorRect _shade = new() { Color = new(0, 0, 0, 0.8f), MouseFilter = MouseFilterEnum.Ignore };
    private readonly Control _diamond = new() { Name = "Diamond", MouseFilter = MouseFilterEnum.Ignore };
    private readonly Label _title = Text("Title", 32), _selection = Text("Selection", 28), _hint = Text("Hint", 24),
        _back = Text("Back", 28), _entry = Text("FieldMenuEntry", 24);
    private readonly DiamondCommandButton[] _commands = [new("Member", "field-menu.member", "field-menu"),
        new("Magic", "field-menu.magic", "field-menu"), new("Item", "field-menu.item", "field-menu"),
        new("Search", "field-menu.search", "field-menu")];
    private readonly MenuPointer _pointer = new();
    private GameInput _input = null!;
    private Action<int> _choose = null!;
    private Action _close = null!, _begin = null!;
    private ExplorationVisuals? _visuals;
    private bool _configured, _requiresRelease = true;
    private int _index;
    private long _generation;
    internal bool Open => _panel.Visible;
    internal string? AssetError { get; private set; }

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Ignore;
        _shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(_shade); AddChild(_panel); AddChild(_entry);
        _panel.AddChild(_title); _panel.AddChild(_selection); _panel.AddChild(_hint); _panel.AddChild(_back); _panel.AddChild(_diamond);
        foreach (var button in _commands) _diamond.AddChild(button);
        _panel.ItemRectChanged += _pointer.Cancel;
        HideMenu(); _entry.Hide();
    }

    internal void Begin() { _generation++; _panel.Show(); _shade.Show(); _entry.Hide(); Suspend(); }
    internal void HideMenu() { _generation++; _panel.Hide(); _shade.Hide(); Suspend(); }
    internal void Present(GameInput input, ExplorationVisuals? visuals, bool entry, bool party, bool preparation, bool search,
        Action begin, Action<int> choose, Action close)
    {
        _input = input; _begin = begin; _choose = choose; _close = close;
        if (!_configured || !ReferenceEquals(_visuals, visuals))
        {
            _configured = true; _visuals = visuals; AssetError = null;
            try { ConfigureArt(visuals?.FieldMenu); }
            catch (Exception error) when (error is InvalidOperationException or ArgumentException) { AssetError = error.Message; }
        }
        _entry.Visible = entry && !Open && AssetError is null;
        _entry.Text = input.CompactHint(GameAction.Cancel) + " · " + GameText.Get("field-menu.title");
        _entry.Size = _entry.GetMinimumSize(); _entry.Position = new(Size.X - _entry.Size.X - 32, Size.Y - _entry.Size.Y - 32);
        _pointer.ReleaseIfNeutral(); if (!input.GameplayHeld) _requiresRelease = false;
        for (int i = 0; i < _commands.Length; i++) _commands[i].Available = i == 3 ? search : i == 0 ? party : preparation;
        if (!Open) return;
        foreach (var command in _commands)
        { command.RefreshCaption(); command.SetSelection(Array.IndexOf(_commands, command) == _index, input.Settings.ReducedFlash); }
        _title.Text = GameText.Get("field-menu.title");
        _selection.Text = GameText.Get("field-menu." + _commands[_index].Identity.ToLowerInvariant() + "-help");
        if (!_commands[_index].Available) _selection.Text += "\n" + GameText.Get("field-menu.unavailable");
        _selection.AddThemeColorOverride("font_color", _commands[_index].Available ? StoryTheme.Selected : StoryTheme.Unavailable);
        _back.Text = input.CompactHint(GameAction.Cancel) + " · " + GameText.Get("prepare.back");
        _hint.Text = GameText.Get("field-menu.hint", input.CompactHint(GameAction.Up), input.CompactHint(GameAction.Left),
            input.CompactHint(GameAction.Right), input.CompactHint(GameAction.Down), input.CompactHint(GameAction.Confirm), input.CompactHint(GameAction.Cancel));
        Arrange();
    }

    private void Arrange()
    {
        float width = Math.Min(1072, Size.X - 64);
        _title.Position = new(32, 24); _title.Size = new(width - 64, 0);
        float top = 48 + _title.GetMinimumSize().Y;
        _diamond.Position = new(32, top); DiamondMenuGeometry.Arrange(_diamond, _commands);
        _selection.Position = new(384, top); _selection.Size = new(width - 416, 0);
        _hint.Position = new(384, top + _selection.GetMinimumSize().Y + 24); _hint.Size = new(width - 416, 0);
        _back.Position = new(32, top + DiamondMenuGeometry.Size.Y + 24); _back.Size = new(304, 0);
        _panel.Size = new(width, Math.Max(_back.Position.Y + _back.GetMinimumSize().Y, _hint.Position.Y + _hint.GetMinimumSize().Y) + 32);
        _panel.Position = ((Size - _panel.Size) / 2).Round();
    }
    public override void _Process(double delta)
    {
        if (!Open) return;
        Arrange(); foreach (var button in _commands) button.AdvanceFocus(delta);
    }
    internal void Suspend() { _requiresRelease = true; _pointer.Suspend(); foreach (var button in _commands) button.StopFocusEmphasis(); }
    internal void Handle(GameAction action)
    {
        _pointer.Cancel(); if (_requiresRelease || !Open) return;
        if (action == GameAction.Cancel) { _close(); return; }
        if (action is GameAction.Up or GameAction.Left or GameAction.Right or GameAction.Down)
            _index = action switch { GameAction.Up => 0, GameAction.Left => 1, GameAction.Right => 2, _ => 3 };
        else if (action == GameAction.Confirm && _commands[_index].Available) { Suspend(); _choose(_index); }
    }
    internal void HandlePointer(InputEvent input)
    {
        if (input is not InputEventMouse mouse) return;
        int hit = Open ? Array.FindIndex(_commands, button => button.GetGlobalRect().HasPoint(mouse.Position)) : -1;
        bool back = Open && _back.GetGlobalRect().HasPoint(mouse.Position);
        bool entry = _entry.Visible && _entry.GetGlobalRect().HasPoint(mouse.Position);
        if (input is InputEventMouseMotion motion)
        { if (_pointer.Moved(motion) && hit >= 0 && !_requiresRelease) _index = hit; return; }
        if (input is not InputEventMouseButton button) return;
        bool right = Open && button.ButtonIndex == MouseButton.Right;
        string? id = back || right ? "back" : entry ? "entry" : hit >= 0 ? hit.ToString() : null;
        Rect2 rect = right ? GetGlobalRect() : back ? _back.GetGlobalRect() : entry ? _entry.GetGlobalRect() : hit >= 0 ? _commands[hit].GetGlobalRect() : default;
        string? clicked = _pointer.Read(button, id, rect, (_generation, Open, _entry.Visible));
        if (_requiresRelease || clicked is null) return;
        if (clicked == "entry") _begin();
        else if (clicked == "back") _close();
        else { _index = int.Parse(clicked); if (_commands[_index].Available) { Suspend(); _choose(_index); } }
    }
    private void ConfigureArt(IReadOnlyDictionary<string, ExplorationRaster>? rasters)
    {
        var textures = new List<ImageTexture>();
        try
        {
            if (rasters is not null)
                foreach (var command in _commands)
                    foreach (string state in new[] { "normal", "selected" })
                    {
                        var raster = rasters[command.Identity.ToLowerInvariant() + "." + state];
                        using var image = raster.Format == "rgba8" ? Image.CreateFromData(24, 24, false, Image.Format.Rgba8, raster.CopyBytes()) : new Image();
                        if (raster.Format != "rgba8" && image.LoadPngFromBuffer(raster.CopyBytes()) != Godot.Error.Ok) throw new InvalidOperationException("field-menu-resource-invalid");
                        textures.Add(ImageTexture.CreateFromImage(image));
                    }
        }
        catch { foreach (var texture in textures) texture.Dispose(); throw; }
        for (int i = 0; i < _commands.Length; i++) _commands[i].SetArt(rasters is null ? null : textures[i * 2], rasters is null ? null : textures[i * 2 + 1]);
    }
    internal object Observe() => new { visible = Open && IsVisibleInTree(), index = _index, requiresRelease = _requiresRelease,
        entryVisible = _entry.IsVisibleInTree(), entryRect = BattleMapViewport.Rectangle(_entry.GetGlobalRect()), selection = _selection.Text,
        panelRect = BattleMapViewport.Rectangle(_panel.GetGlobalRect()), selectionRect = BattleMapViewport.Rectangle(_selection.GetGlobalRect()), selectionMinimumHeight = _selection.GetMinimumSize().Y,
        hint = _hint.Text, hintRect = BattleMapViewport.Rectangle(_hint.GetGlobalRect()), hintMinimumHeight = _hint.GetMinimumSize().Y,
        commands = _commands.Select(button => new { id = button.Identity, selected = button.Selected, enabled = button.Available,
            rect = BattleMapViewport.Rectangle(button.GetGlobalRect()), art = button.ObserveArt() }) };
    private static Label Text(string name, int size)
    { var label = new Label { Name = name, AutowrapMode = TextServer.AutowrapMode.WordSmart, MouseFilter = MouseFilterEnum.Ignore }; label.AddThemeFontSizeOverride("font_size", size); return label; }
}
