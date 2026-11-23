using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Previews;

public partial class DesignPreview : Control
{
    private enum Page { Root, Question, Reply, Quote, Result }
    private readonly PreviewStage _stage = new() { Name = "Artwork", Size = new(1920, 1080) };
    private readonly List<Choice> _choices = [];
    private Control _surface = null!;
    private Label? _detail;
    private StoryDialoguePanel? _dialogue;
    private readonly MenuPointer _readerPointer = new();
    private Page _page;
    private int _scene, _focus, _rootFocus;
    private bool _english, _steadyIcons;
    private Choice? _animated;
    private double _frameTime;
    private const double IconFrameSeconds = 0.25;
    private static readonly Vector2 CommandSize = new(528, 80);
    private static readonly Vector2 CommandOrigin = new(1312, 312);
    private const int PanelInset = 16, PanelHeader = 64;
    private static readonly Vector2 PanelOrigin = CommandOrigin - new Vector2(PanelInset, PanelHeader);
    private static float PanelWidth => CommandSize.X + 2 * PanelInset;
    private string _result = "";
    private sealed record Choice(Button Hit, Label Name, TextureRect Art, string? Key, bool Enabled, string Reason);
    private string T(string zh, string en) => _english ? en : zh;
    private readonly record struct UnitSummary(string Name, int Hp, int MaxHp, int Mp, int MaxMp);
    private UnitSummary Sarah => new(T("莎拉", "Sarah"), 18, 24, 8, 12);
    private UnitSummary Bowie => new(T("鲍伊", "Bowie"), 16, 24, 0, 0);

    public override void _Ready()
    {
        GetWindow().Title = "SF2 · Design system preview";
        GameText.Initialize("zh-CN");
        Theme = StoryTheme.Create();
        AddChild(_stage);
        _stage.LoadSelection();
        if (OS.GetCmdlineUserArgs().Contains("--battle")) _scene = BattleScene;
        if (OS.GetCmdlineUserArgs().Contains("--shop")) _scene = ShopScene;
        if (OS.GetCmdlineUserArgs().Contains("--field")) _scene = FieldScene;
        if (OS.GetCmdlineUserArgs().Contains("--inspect")) _scene = InspectScene;
        GetWindow().FocusExited += StopIconAnimation;
        if (BattleSample) StartBattleSample();
        Render();
    }

    private static StyleBoxFlat Box(string fill, string edge, int border = 2)
    {
        var style = new StyleBoxFlat { BgColor = new Color(fill), BorderColor = new Color(edge) };
        style.SetBorderWidthAll(border);
        return style;
    }

    private static TNode Put<TNode>(Control parent, TNode child, Rect2 rect) where TNode : Control
    {
        child.Position = rect.Position;
        child.Size = rect.Size;
        parent.AddChild(child);
        return child;
    }

    private Panel Panel(Rect2 rect, bool quiet = false)
    {
        var panel = Put(_surface, new Panel { MouseFilter = MouseFilterEnum.Ignore }, rect);
        if (quiet) panel.AddThemeStyleboxOverride("panel", Box("111d30f5", "53637a", 1));
        return panel;
    }

    private static Label Text(Control parent, Rect2 rect, string value, int size, string color = "f4f5fc")
    {
        var label = new Label
        {
            Text = value,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", new Color(color));
        return Put(parent, label, rect);
    }

    private Button Utility(Rect2 rect, string label, Action action, bool active = false)
    {
        var button = Put(_surface, new Button { Text = label, FocusMode = FocusModeEnum.None }, rect);
        button.AddThemeFontSizeOverride("font_size", 22);
        button.AddThemeStyleboxOverride("normal", Box(active ? "2c4158" : "16243a", active ? "ffe19a" : "53637a", 1));
        button.AddThemeStyleboxOverride("hover", Box("304761", "ffe19a", 1));
        button.Pressed += action;
        return button;
    }

    private void ChooseScene(int scene)
    {
        _giveStep = GiveStep.Closed;
        _showMembers = false;
        _shopStep = ShopStep.Services;
        _journey = JourneyPage.Closed;
        _field = FieldPage.Commands;
        StopBattleSample();
        _reading = 0;
        _scene = scene;
        _page = Page.Root;
        _focus = _rootFocus = 0;
        if (BattleSample) StartBattleSample();
        Render();
    }

    private void ToggleLanguage()
    {
        _english = !_english;
        GameText.Initialize(PreviewLanguage);
        Render();
    }

    private void Show(Page page, int focus = 0)
    {
        _page = page;
        _focus = focus;
        Render();
    }

    private void Render()
    {
        _readerPointer.Cancel();
        if (_surface is not null) { RemoveChild(_surface); _surface.QueueFree(); }
        _surface = new Control { Name = "PreviewControls", Size = new(1920, 1080), MouseFilter = MouseFilterEnum.Ignore };
        AddChild(_surface);
        _choices.Clear();
        _detail = null;
        _dialogue = null;
        _editor = null;
        _battleHost?.RefreshBattlePreview(SampleCovered, PreviewLanguage, _steadyIcons);
        _stage.Visible = !BattleSample;
        if (!BattleSample) _stage.ShowScene(_scene, _facingSign);
        if (_journey != JourneyPage.Closed) DrawJourney();
        else if (Preparing) PrepareGive();
        else if (_showMembers) Members();
        else switch (_scene)
        {
            case 0: Entry(); break;
            case 1: Dialogue(); break;
            case 2: Church(); break;
            case ShopScene: Shop(); break;
            case FieldScene: DrawField(); break;
            case OutcomeScene: DrawOutcome(); break;
        }
        Panel(new(0, 0, 1920, 72), true);
        string[] tabs = [T("女巫", "Witch"), T("对白", "Dialogue"), T("教堂", "Church"), T("战斗", "Battle"), T("队员", "Members"), T("商店", "Shop"),
            T("探索", "Field"), T("查看", "Inspect"), T("战果", "Results"), T("系统", "System")];
        for (int i = 0; i < tabs.Length; i++)
        {
            int tab = i;
            bool active = _journey != JourneyPage.Closed && _fromMenu ? i == 9 : _showMembers ? i == 4 : _scene == i;
            Utility(new(10 + i * 131, 10, 126, 52), $"F{i + 1}  {tabs[i]}", () => SelectTab(tab), active);
        }
        string art = BattleSample ? T("生产战斗", "Production battle") :
            _stage.AssetFailure is { } problem ? T("资源未齐 · ", "Art incomplete · ") + problem :
            (_scene == 0 && !_showMembers ? _stage.HasWitch : _stage.HasWorld) ? T("原版素材 · 示例状态", "Original art · sample state") :
            T("无私有素材 · 线框", "Text-only · schematic");
        Text(_surface, new(1326, 8, 220, 56), art, 18, "c1cfdf");
        Utility(new(1560, 10, 153, 52), _steadyIcons ? T("动效：关", "Motion: off") : T("动效：开", "Motion: on"),
            () => { _steadyIcons = !_steadyIcons; Render(); });
        Utility(new(1722, 10, 170, 52), _english ? "中文   /   L" : "English   /   L", ToggleLanguage);
        Refresh();
    }

    private void Item(Rect2 rect, string name, string? key = null, bool enabled = true, string reason = "", string value = "", bool slot = false)
    {
        int index = _choices.Count;
        var hit = Put(_surface, new Button { Name = "Choice" + index, FocusMode = FocusModeEnum.None }, rect);
        var (caption, icon) = ItemContent(hit, name, key, value, slot || (Preparing && _giveStep is GiveStep.Item or GiveStep.Exchange));
        _choices.Add(new(hit, caption, icon, key, enabled, reason));
        hit.GuiInput += input =>
        {
            if (input is InputEventMouseMotion motion && motion.Relative != Vector2.Zero && ChoiceFocus != index)
            { ChoiceFocus = index; Refresh(); }
        };
        hit.Pressed += () => { ChoiceFocus = index; Refresh(); Activate(); };
    }

    private (Label, TextureRect) ItemContent(Control row, string name, string? key, string value, bool slot)
    {
        var size = row.Size;
        var icon = new TextureRect
        {
            MouseFilter = MouseFilterEnum.Ignore,
            TextureFilter = TextureFilterEnum.Nearest,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
        };
        bool hasArt = key is not null && _stage.Icon(key, false) is not null;
        bool command = !slot && key is not null && !key.StartsWith("identity-icon.", StringComparison.Ordinal);
        float width = command ? 72 : 48, height = 72;
        Put(row, icon, new(20, (size.Y - height) / 2, width, height));
        int left = slot || hasArt ? (int)width + 36 : 20;
        float valueWidth = slot || value.Length > 0 ? 132 : 0;
        var caption = Text(row, new(left, 0, size.X - left - 16 - valueWidth, size.Y), name, 30);
        if (value.Length > 0)
            Text(row, new(size.X - valueWidth - 16, 0, valueWidth, size.Y), value, 30)
                .HorizontalAlignment = HorizontalAlignment.Right;
        return (caption, icon);
    }

    private void Refresh()
    {
        StopIconAnimation();
        if (_scene == ShopScene && !_showMembers && _journey == JourneyPage.Closed) RefreshShopPack();
        if (Preparing && _giveStep == GiveStep.Recipient && _candidateIndex != _giveFocus)
        {
            _candidateIndex = _giveFocus;
            _surface.RemoveChild(_receiverPanel);
            _receiverPanel.QueueFree();
            _receiverPanel = GivePack(ReceiverOrigin, Recipient, RecipientPack, false);
        }
        for (int i = 0; i < _choices.Count; i++)
        {
            var row = _choices[i];
            bool focused = ChoiceFocus == i;
            var style = Box(focused ? "273d56" : "102c50", focused ? "ffe19a" : "536c87", focused ? 3 : 1);
            foreach (var state in new[] { "normal", "hover", "pressed" }) row.Hit.AddThemeStyleboxOverride(state, style);
            row.Name.AddThemeColorOverride("font_color", !row.Enabled ? StoryTheme.Unavailable :
                focused ? StoryTheme.Selected : new Color("f4f5fc"));
            if (row.Key is not null)
            {
                row.Art.Texture = _stage.Icon(row.Key, focused);
                if (focused && row.Enabled && !_steadyIcons && row.Art.Texture is not null &&
                    !row.Key.StartsWith("identity-icon.", StringComparison.Ordinal))
                    _animated = row;
            }
        }
        if (_detail is not null && _choices.Count > 0)
        {
            _detail.Text = _choices[ChoiceFocus].Reason;
            if (!_showMembers && _scene is 2 or FieldScene && _detail.GetParent() is Panel panel)
            {
                _detail.Visible = _detail.Text.Length > 0;
                var last = _choices[^1].Hit;
                float bottom = last.Position.Y + last.Size.Y - panel.Position.Y;
                if (_detail.Visible) bottom = _detail.Position.Y + _detail.GetMinimumSize().Y;
                panel.Size = new(panel.Size.X, bottom + 16);
            }
        }
        if (_page == Page.Question && _dialogue is not null)
            _dialogue.Choices.Present(true, _focus == 0, true, true);
    }

    private void StopIconAnimation()
    {
        if (_animated is { Key: { } key } row) row.Art.Texture = _stage.Icon(key, true);
        _animated = null;
        _frameTime = 0;
    }

    public override void _Process(double delta)
    {
        if (_animated is not { Key: { } key } row) return;
        bool wasSelectedFrame = _frameTime < IconFrameSeconds;
        _frameTime = (_frameTime + delta) % (2 * IconFrameSeconds);
        bool selectedFrame = _frameTime < IconFrameSeconds;
        if (selectedFrame != wasSelectedFrame) row.Art.Texture = _stage.Icon(key, selectedFrame);
    }

    private static Rect2 CommandRow(int index) =>
        new(CommandOrigin + new Vector2(0, index * (CommandSize.Y + 8)), CommandSize);

    private Panel SidePage(string title, int rows, string context = "")
    {
        float height = PanelHeader + rows * CommandSize.Y + (rows - 1) * 8 + PanelInset;
        var panel = Panel(new(PanelOrigin, new(PanelWidth, height)));
        float width = context.Length == 0 ? CommandSize.X : 320;
        Text(panel, new(PanelInset, 6, width, 52), title, 32);
        if (context.Length > 0)
            Text(panel, new(PanelInset + CommandSize.X - 200, 6, 200, 52), context, 24, "cad6e5").HorizontalAlignment = HorizontalAlignment.Right;
        return panel;
    }

    private void Vitals(Control parent, Vector2 origin, UnitSummary unit)
    {
        void Gauge(float x, string type, int current, int maximum)
        {
            var at = origin + new Vector2(x, 0);
            Text(parent, new(at, new(240, 54)), maximum == 0 ? $"{type}  —" : $"{type}  {current}/{maximum}", 36);
            Put(parent, new BattleGaugeImage(type) { Fraction = maximum == 0 ? 0 : (float)current / maximum },
                new(at + new Vector2(0, 55), new(232, 28)));
        }
        Gauge(0, "HP", unit.Hp, unit.MaxHp);
        Gauge(264, "MP", unit.Mp, unit.MaxMp);
    }

    private void ReadingActions(Panel panel, string message, string first, string? second = null)
    {
        var body = Text(panel, new(CommandOrigin - panel.Position, CommandSize), message, 30);
        var extra = new Vector2(0, Mathf.Max(0, body.GetMinimumSize().Y - CommandSize.Y));
        panel.Size += extra;
        Item(new(CommandRow(1).Position + extra, CommandSize), first);
        if (second is not null) Item(new(CommandRow(2).Position + extra, CommandSize), second);
    }

    private void Footer(string hint)
    {
        var panel = Panel(new(32, 997, 1856, 60), true);
        Text(panel, new(18, 0, 1816, 60), hint, 24, "cad6e5");
    }

    private void Entry()
    {
        if (!_stage.HasWitch)
            Text(_surface, new(440, 462, 460, 100), T("女巫 · 原画区域", "Witch artwork"), 40);
        var frame = Panel(new(1216, 144, 640, 252));
        Text(frame, new(26, 16, 584, 54), T("女巫", "The Witch"), 32, "ffe19a");
        Text(frame, new(26, 78, 584, 146), T("准备好出发了吗？请选择你的旅程。", "Are you ready to set out? Choose your journey."), 30);
        Item(new(1216, 444, 640, 78), GameText.Get("entry.new"));
        Item(new(1216, 534, 640, 78), GameText.Get("entry.continue"));
        Footer(T("↑ ↓ 选择     Enter 确认     Esc 返回", "↑ ↓ Select     Enter Confirm     Esc Back"));
    }

    private void Dialogue()
    {
        _dialogue = new StoryDialoguePanel { Name = "StoryReader" };
        _surface.AddChild(_dialogue);
        var portrait = _stage.Portrait();
        if (portrait is not null) _dialogue.MountPortrait(portrait, 0);
        _dialogue.Body.Text = _page switch
        {
            Page.Question => T("准备好和我一起去广场了吗？", "Are you ready to walk to the square with me?"),
            Page.Reply => _result == "yes" ? T("那我们走吧。", "Then let's go.") : T("好，我在这里等你。", "All right. I'll wait here."),
            _ => T("你听见钟声了吗？镇上的人都去了广场。我们也过去看看吧。", "Did you hear the bell? Everyone in town has gone to the square. Let's go and see.")
        };
        _dialogue.Choices.Present(_page == Page.Question, _focus == 0, true, true);
        _dialogue.Present(true, portrait is not null, false, new(864, 480, 96, 96), _page == Page.Question ?
            T("← → 选择     Enter 回答", "← → Select     Enter Answer") : T("Enter 或点击继续", "Enter or click to continue"));
    }

    private void Church()
    {
        Put(_surface, new ColorRect { Color = new(0, 0, 0, 0.35f), MouseFilter = MouseFilterEnum.Ignore }, new(0, 72, 1920, 1008));
        string service = _rootFocus switch { 0 => T("复活", "Raise"), 1 => T("治疗", "Cure"),
            3 => T("记录冒险", "Record adventure"), _ => T("离开", "Leave") };
        string title = T("教堂", "Church") + (_page == Page.Root ? "" : " / " + service);
        var panel = SidePage(title, _page == Page.Root ? 5 : _page == Page.Quote ? 3 : 2,
            _page == Page.Result ? "" : T("金币   150", "Gold   150"));
        if (_page == Page.Root)
        {
            string[] names = [T("复活", "Raise"), T("治疗", "Cure"), T("转职 · 不可用", "Promotion · Unavailable"), T("记录冒险", "Record adventure")];
            string[] keys = ["raise", "cure", "promote", "save"];
            for (int i = 0; i < 4; i++)
                Item(CommandRow(i), names[i], keys[i], i != 2,
                    i == 2 ? T("这位神父没有提供转职服务。", "Promotion is unavailable from this priest.") : "");
            var leave = CommandRow(4);
            Item(new(leave.Position, new(CommandSize.X, 64)), T("离开", "Leave"));
            _detail = Text(panel, new(leave.Position - panel.Position + new Vector2(0, 72),
                new(CommandSize.X, 0)), "", 30);
        }
        else if (_page == Page.Quote)
        {
            ReadingActions(panel, _rootFocus == 0 ? T("让鲍伊复活需要 50 枚金币。要支付吗？", "Raise Bowie for 50 gold?") :
                T("为莎拉治疗中毒需要 10 枚金币。要支付吗？", "Cure Sarah's poison for 10 gold?"),
                T("确认支付", "Confirm payment"), T("拒绝", "Decline"));
        }
        else
        {
            ReadingActions(panel, _result switch
            {
                "leave" => T("已离开服务菜单。", "You have left the service menu."),
                _ => T("示例服务完成。", "Sample service complete.")
            }, T("返回服务列表", "Return to services"));
        }
        Footer(T("↑ ↓ 选择     Enter 确认     Esc 返回", "↑ ↓ Select     Enter Confirm     Esc Back"));
    }

    private void Activate()
    {
        if (_choices.Count > 0 && !_choices[ChoiceFocus].Enabled) return;
        if (_journey != JourneyPage.Closed) { AdvanceJourney(); return; }
        if (Preparing) { AdvanceGive(); return; }
        if (_scene == FieldScene) { AdvanceField(); return; }
        if (_scene == OutcomeScene) { AdvanceOutcome(); return; }
        if (_scene == ShopScene) { AdvanceShop(); return; }
        if (_scene == 0) { OpenSlots(_focus == 0 ? SlotTask.New : SlotTask.Continue); return; }
        if (_scene == 1)
        {
            if (_page == Page.Question) _result = _focus == 0 ? "yes" : "no";
            Show(_page == Page.Root ? Page.Question : _page == Page.Question ? Page.Reply : Page.Root);
            return;
        }
        if (_scene == 2)
        {
            if (_page == Page.Root)
            {
                _rootFocus = _focus;
                if (_focus == 3) { OpenSlots(SlotTask.Save); return; }
                _result = _focus == 4 ? "leave" : "service";
                Show(_focus < 2 ? Page.Quote : Page.Result, _focus < 2 ? 1 : 0);
            }
            else if (_page == Page.Quote && _focus == 0) Show(Page.Result);
            else Show(Page.Root, _rootFocus);
            return;
        }
    }

    private void Back()
    {
        if (_journey != JourneyPage.Closed) { BackJourney(); return; }
        if (_scene == FieldScene) { BackField(); return; }
        if (_scene == OutcomeScene) { _reading = 0; _focus = _outcome; Render(); return; }
        if (_scene == ShopScene) { BackShop(); return; }
        if (_scene == 1 && _page == Page.Question) { _result = "no"; Show(Page.Reply); }
        else if (_page != Page.Root) Show(Page.Root, _scene >= 2 ? _rootFocus : 0);
    }

    private void SelectTab(int tab)
    {
        if (tab == 9) OpenHost();
        else if (tab == 4) { if (_journey == JourneyPage.Closed) ToggleMembers(); }
        else ChooseScene(tab);
    }

    public override void _Input(InputEvent input)
    {
        if (input is InputEventKey { Pressed: true, Echo: false, Keycode: >= Key.F1 and <= Key.F10 } shortcut)
        { SelectTab((int)shortcut.Keycode - (int)Key.F1); GetViewport().SetInputAsHandled(); return; }
        if (_editor is null && input is InputEventKey { Keycode: Key.L, Pressed: true, Echo: false })
        { ToggleLanguage(); GetViewport().SetInputAsHandled(); return; }
        if (RouteBattleSampleInput(input)) { GetViewport().SetInputAsHandled(); return; }
        if (BattleSample && !SampleCovered) return;
        if (_editor is not null)
        {
            if (input is InputEventKey editingKey) _editor.HandleKeyboard(editingKey);
            return;
        }
        if (_journey == JourneyPage.Closed && MemberInput(input)) { GetViewport().SetInputAsHandled(); return; }
        if (_dialogue is not null && input is InputEventMouseMotion motion) _readerPointer.Moved(motion);
        if (_dialogue is not null && input is InputEventMouseButton { ButtonIndex: MouseButton.Left } mouse)
        {
            var rect = _dialogue.GetGlobalRect();
            var hit = _page == Page.Question ? _dialogue.Choices.Hit(mouse.Position)
                : (Id: rect.HasPoint(mouse.Position) ? "Read" : null, Rect: rect);
            if (_readerPointer.Read(mouse, hit.Id, hit.Rect, _dialogue) is { } selected)
            {
                _focus = selected == "No" ? 1 : 0;
                Activate();
            }
            if (hit.Id is not null) GetViewport().SetInputAsHandled();
            return;
        }
        if (_showMembers) return;
        if (input.IsActionPressed("ui_cancel") || input is InputEventJoypadButton { ButtonIndex: JoyButton.B, Pressed: true }
            || input is InputEventMouseButton { ButtonIndex: MouseButton.Right, Pressed: true }) Back();
        else if (input.IsActionPressed("ui_accept") || input is InputEventJoypadButton { ButtonIndex: JoyButton.A, Pressed: true }) Activate();
        else
        {
            bool sideways = _journey == JourneyPage.Closed && _page == Page.Question;
            int direction = input.IsActionPressed(sideways ? "ui_left" : "ui_up") ? -1 :
                input.IsActionPressed(sideways ? "ui_right" : "ui_down") ? 1 : 0;
            if (direction == 0) return;
            int count = sideways ? 2 : _choices.Count;
            if (count == 0) return;
            _focus = (_focus + count + direction) % count;
            Refresh();
        }
        GetViewport().SetInputAsHandled();
    }
}
