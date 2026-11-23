using Godot;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Previews;

public partial class DesignPreview : Control
{
    private enum Tab { Witch, Dialogue, Church, Battle, Members, Shop, Field, Inspect, Results, System }
    private enum Outcome { Victory, Defeat, Retreat }
    private const float ToolbarHeight = 72, DisplayScale = (1080 - ToolbarHeight) / 1080;
    private static readonly Rect2 PictureBounds = new(64, ToolbarHeight, 1792, 1008);
    private static readonly Transform2D InputTransform = new Transform2D(
        new(DisplayScale, 0), new(0, DisplayScale), PictureBounds.Position).AffineInverse();
    private readonly SubViewport _game = new() { Name = "ProductionViewport", Size = new(1920, 1080),
        RenderTargetUpdateMode = SubViewport.UpdateMode.Always };
    private readonly Button[] _tabs = new Button[10];
    private OptionButton _outcomes = null!;
    private Button _motion = null!, _language = null!;
    private GameRoot? _host;
    private PreviewEntry? _entry;
    private ulong _entryDeadline;
    private InputSettings _settings = new() { FormatVersion = 1, Language = "zh-CN" };
    private string _save = "";
    private Tab _tab;
    private Outcome _outcome;
    private bool _menuOpen;
    private string _presentedLanguage = "";
    private bool _presentedMotion;
    private static PackedScene Main => GD.Load<PackedScene>("res://Main.tscn");
    private string T(string zh, string en) => _settings.Language == "en" ? en : zh;

    public override void _Ready()
    {
        GetWindow().Title = "SF2 · Production UI preview";
        GameText.Initialize(_settings.Language);
        _save = ProjectSettings.GlobalizePath($"user://design-preview/{Guid.NewGuid():N}/slot.json");
        AddChild(_game);
        Place(this, new TextureRect { Name = "ProductionPicture", Texture = _game.GetTexture(),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, MouseFilter = MouseFilterEnum.Ignore }, PictureBounds);
        BuildToolbar();

        var args = OS.GetCmdlineUserArgs();
        _outcome = args.Contains("--retreat") ? Outcome.Retreat :
            args.Contains("--defeat") ? Outcome.Defeat : Outcome.Victory;
        var tab = args.Contains("--battle") ? Tab.Battle : args.Contains("--inspect") ? Tab.Inspect :
            args.Contains("--field") ? Tab.Field : args.Contains("--members") ? Tab.Members :
            args.Contains("--shop") ? Tab.Shop : args.Contains("--church") ? Tab.Church :
            args.Contains("--dialogue") ? Tab.Dialogue : args.Contains("--system") ? Tab.System :
            args.Any(arg => arg is "--results" or "--defeat" or "--retreat") ? Tab.Results : Tab.Witch;
        SelectTab(tab);
    }

    private void BuildToolbar()
    {
        var toolbar = Place(this, new Panel { Name = "PreviewToolbar", Theme = StoryTheme.CreateEntry(),
            MouseFilter = MouseFilterEnum.Ignore }, new(0, 0, 1920, ToolbarHeight));
        for (int i = 0; i < _tabs.Length; i++)
        {
            var tab = (Tab)i;
            _tabs[i] = Place(toolbar, new Button { Name = tab.ToString(), FocusMode = FocusModeEnum.None },
                new(16 + i * 130, 12, 124, 48));
            _tabs[i].AddThemeFontSizeOverride("font_size", 22);
            _tabs[i].Pressed += () => SelectTab(tab);
        }
        _outcomes = Place(toolbar, new OptionButton { Name = "Outcome", FocusMode = FocusModeEnum.None }, new(1322, 12, 224, 48));
        _outcomes.ItemSelected += index => { _outcome = (Outcome)index; SelectTab(Tab.Results); };
        _motion = Place(toolbar, new Button { Name = "Motion", FocusMode = FocusModeEnum.None }, new(1558, 12, 154, 48));
        _language = Place(toolbar, new Button { Name = "Language", FocusMode = FocusModeEnum.None }, new(1724, 12, 180, 48));
        _motion.Pressed += () => _host?.SetPreviewAppearance(_settings.Language, !_settings.ReducedFlash);
        _language.Pressed += ToggleLanguage;
    }

    private void ToggleLanguage() =>
        _host?.SetPreviewAppearance(T("en", "zh-CN"), _settings.ReducedFlash);

    private void PaintToolbar()
    {
        _presentedLanguage = _settings.Language;
        _presentedMotion = _settings.ReducedFlash;
        _menuOpen = _host?.PreviewSystemOpen == true;
        string[] names = [T("女巫", "Witch"), T("对白", "Dialogue"), T("教堂", "Church"), T("战斗", "Battle"),
            T("队员", "Members"), T("商店", "Shop"), T("探索", "Field"), T("检视", "Inspect"),
            T("战果", "Results"), T("系统", "System")];
        for (int i = 0; i < _tabs.Length; i++)
        {
            _tabs[i].Text = $"F{i + 1} {names[i]}";
            StoryTheme.Select(_tabs[i], i == (int)(_menuOpen ? Tab.System : _tab));
        }
        _outcomes.Clear();
        _outcomes.AddItem(T("胜利", "Victory"));
        _outcomes.AddItem(T("败北", "Defeat"));
        _outcomes.AddItem(T("撤退", "Retreat"));
        _outcomes.Select((int)_outcome);
        _outcomes.Visible = _tab == Tab.Results && !_menuOpen;
        _motion.Text = _settings.ReducedFlash ? T("动效：减弱", "Motion: low") : T("动效：开", "Motion: on");
        _language.Text = T("English / L", "中文 / L");
    }

    private static TControl Place<TControl>(Control parent, TControl node, Rect2 rect) where TControl : Control
    {
        node.Position = rect.Position;
        node.Size = rect.Size;
        parent.AddChild(node);
        return node;
    }

    private static bool FieldTab(Tab tab) => tab is Tab.Members or Tab.Shop or Tab.Field;

    private void SelectTab(Tab tab)
    {
        if (_host?.PreviewLoading == true) return;
        if (tab == Tab.System && _host is { PreviewHasSession: true } existing)
        {
            if (existing.OpenPreviewSystemMenu()) PaintToolbar();
            return;
        }
        bool reuse = tab != _tab && FieldTab(_tab) && FieldTab(tab) && _host?.PreviewFieldAvailable == true;
        _tab = tab;
        if (!reuse)
        {
            if (_host is not null)
            {
                _settings = _host.PreviewSettings?.Draft() ?? _settings;
                _game.RemoveChild(_host);
                _host.QueueFree();
            }
            var host = Main.Instantiate<GameRoot>();
            host.ConfigurePreview(Source(tab, _outcome), _settings, _save,
                () => Callable.From(() => SelectTab(Tab.Witch)).CallDeferred());
            _host = host;
            _game.AddChild(host);
        }
        _entry = tab switch
        {
            Tab.Members => PreviewEntry.Party, Tab.Shop => PreviewEntry.Shop, Tab.Field => PreviewEntry.FieldMenu,
            Tab.Inspect => PreviewEntry.Inspection, Tab.Results => PreviewEntry.Outcome, Tab.System => PreviewEntry.System,
            _ => null,
        };
        _entryDeadline = Time.GetTicksMsec() + 6000;
        PaintToolbar();
    }

    public override void _Process(double delta)
    {
        if (_host?.PreviewSettings is { } settings) _settings = settings;
        if (_presentedLanguage != _settings.Language || _presentedMotion != _settings.ReducedFlash ||
            _menuOpen != (_host?.PreviewSystemOpen == true)) PaintToolbar();
        if (_entry is { } entry && (_host?.AdvancePreviewEntry(entry) == true || Time.GetTicksMsec() >= _entryDeadline))
            _entry = null;
    }

    public override void _Input(InputEvent input)
    {
        if (input is InputEventKey { Pressed: true, Echo: false, Keycode: >= Key.F1 and <= Key.F10 } shortcut)
        {
            SelectTab((Tab)((int)shortcut.Keycode - (int)Key.F1));
            GetViewport().SetInputAsHandled();
            return;
        }
        if (input is InputEventKey { Keycode: Key.L, Pressed: true, Echo: false } && _host?.PreviewNameEntryOpen != true)
        {
            ToggleLanguage();
            GetViewport().SetInputAsHandled();
            return;
        }
        if (input is InputEventMouse mouse)
        {
            bool inside = PictureBounds.HasPoint(mouse.Position);
            if (!inside && mouse is not (InputEventMouseMotion or InputEventMouseButton { Pressed: false })) return;
            using var local = input.XformedBy(InputTransform);
            _game.PushInput(local, inLocalCoords: true);
            if (!inside) return;
        }
        else _game.PushInput(input, inLocalCoords: true);
        GetViewport().SetInputAsHandled();
    }
}
