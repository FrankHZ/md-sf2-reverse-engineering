using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;
using Sf2.Remake.GodotAdapter.Battles;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Only focus/gesture state lives here. Offers and transaction progression belong to the session.
internal sealed partial class ChurchServiceView : Control
{
    private readonly Panel _panel = new();
    private readonly Label _title = Text("Title", 32), _hint = Text("Hint", 24);
    private readonly DiamondCommandButton[] _commands = [new("Raise", "church.raise", "church-menu"),
        new("Cure", "church.cure", "church-menu"), new("Promote", "church.icon-promote", "church-menu"), new("Save", "church.icon-save", "church-menu")];
    private CompactCommandList _commandsList = null!;
    private bool IsMenu => _wait?.Phase == ChurchPhase.Menu;
    private readonly List<Label> _rows = [];
    private readonly List<(string Label, ChurchCommand? Command)> _options = [];
    private readonly MenuPointer _pointer = new();
    private GameSession _session = null!;
    private GameInput _input = null!;
    private Func<ChurchCommand, long, SessionResult> _submit = null!;
    private ChurchWait? _wait;
    private long _revision;
    private string? _language;
    private int _index;
    private bool _requiresRelease = true;
    private string? _error;
    internal string? AssetError { get; private set; }

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Stop;
        var shade = new ColorRect { Color = new(0, 0, 0, 0.35f), MouseFilter = MouseFilterEnum.Ignore };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade); AddChild(_panel);
        _commandsList = new(_commands) { Name = "ServiceCommands", InvalidatePointer = _pointer.Cancel };
        _panel.AddChild(_title); _panel.AddChild(_commandsList);
        _panel.ItemRectChanged += _pointer.Cancel;
        _title.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _hint.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _panel.AddChild(_hint); _hint.Position = new(32, 612); _hint.Size = new(1008, 192);
        Hide();
    }

    internal void Present(GameSession session, GameInput input, Func<ChurchCommand, long, SessionResult> submit)
    {
        if (!ReferenceEquals(_session, session))
        {
            AssetError = null;
            try { ConfigureAssets(session.Definition.Exploration?.Visuals?.ChurchMenu); }
            catch (Exception error) when (error is InvalidOperationException or ArgumentException)
            { AssetError = error.Message; }
        }
        _session = session; _input = input; _submit = submit;
        if (AssetError is not null) { Hide(); Suspend(); return; }
        if (session.Current.Story.Wait is not ChurchWait wait) { Hide(); _wait = null; Suspend(); return; }
        Show();
        if (_wait?.Token != wait.Token)
        {
            _revision = session.Current.Revision; _index = wait.Phase == ChurchPhase.Menu ? _serviceIndex : 0; _requiresRelease = true; _error = null;
            _pointer.Suspend();
        }
        if (_wait?.Token != wait.Token || _language != GameText.Language)
        {
            _wait = wait; _language = GameText.Language; _options.Clear();
            if (wait.Phase == ChurchPhase.Result)
            {
                _title.Text = wait.Result!.Kind switch
                {
                    ChurchResultKind.NoApplicableMember => GameText.Get("church.none"),
                    ChurchResultKind.Declined => GameText.Get("church.declined"),
                    ChurchResultKind.InsufficientGold => GameText.Get("church.insufficient"),
                    _ => GameText.Get("church.paid." + EffectKey(wait.Result.Treatment!.Effect), wait.Result.Treatment.Cost),
                };
                if (wait.CurseUnavailable) _title.Text += "\n" + GameText.Get("church.curse");
                _options.Add((GameText.Get("church.continue"), new ContinueChurch(wait.Token)));
            }
            else if (wait.Offer is { } offer)
            {
                var member = session.QueryPartyDetails()!.Members.Single(row => row.Carried.Actor == offer.Member);
                _title.Text = GameText.Get("church.offer", Treatment(offer.Effect), PartyStatusView.MemberName(member, session), offer.Cost, session.Current.Exploration!.Party.Gold?.ToString() ?? "?");
                _options.Add((GameText.Get("church.confirm"), new AnswerChurch(wait.Token, offer, true)));
                _options.Add((GameText.Get("church.decline"), new AnswerChurch(wait.Token, offer, false)));
            }
            else
            {
                _title.Text = GameText.Get("church.menu", session.Current.Exploration!.Party.Gold?.ToString() ?? "?");
                _options.AddRange([(GameText.Get("church.raise"), new ChooseChurch(wait.Token, ChurchAction.Raise)),
                    (GameText.Get("church.cure"), new ChooseChurch(wait.Token, ChurchAction.Cure)), (GameText.Get("church.promote"), null),
                    (GameText.Get("church.save"), new LeaveChurch(wait.Token, true)),
                    (GameText.Get("church.leave"), new LeaveChurch(wait.Token))]);
            }
            while (_rows.Count < Math.Max(1, _options.Count)) { var row = Text("Choice" + _rows.Count, 28); _rows.Add(row); _panel.AddChild(row); }
            for (int i = 0; i < _rows.Count; i++)
            { _rows[i].Visible = IsMenu ? i == 0 : i < _options.Count; }
        }
        _pointer.ReleaseIfNeutral(); if (!_input.GameplayHeld) _requiresRelease = false;
        _commandsList.Visible = IsMenu;
        if (IsMenu)
        {
            for (int i = 0; i < _commands.Length; i++)
            {
                _commands[i].RefreshCaption(); _commands[i].Available = _options[i].Command is not null;
                _commands[i].SetSelection(i == _index, input.Settings.ReducedFlash);
            }
            _commandsList.Present(_options.Take(4).Select(option => option.Label).ToArray());
            _rows[0].AddThemeColorOverride("font_color", _index == 4 ? StoryTheme.Selected : Colors.White);
            _rows[0].Text = (_index == 4 ? "› " : "   ") + GameText.Get("church.leave");
        }
        else for (int i = 0; i < _options.Count; i++)
        {
            _rows[i].Text = (i == _index ? "> " : "   ") + _options[i].Label;
            _rows[i].AddThemeColorOverride("font_color", i == _index ? StoryTheme.Selected : Colors.White);
        }
        string? reason = _error ?? (IsMenu && _index == 2 ? GameText.Get("church.promotion") : null);
        _hint.Text = (reason is null ? "" : reason + "\n") + GameText.Get("church.hint", input.CompactHint(GameAction.Up), input.CompactHint(GameAction.Down),
                input.CompactHint(GameAction.Confirm), input.CompactHint(GameAction.Cancel));
        if (_wait?.Phase == ChurchPhase.Menu && _options[_index].Command is LeaveChurch { Save: true })
            _hint.Text += "\n" + GameText.Get("church.save-hint");
        if (GameText.Language == "zh-CN")
        {
            _hint.Text += "\n" + GameText.Get("localization.names");
            string fallback = GameText.Fallback("church.");
            if (fallback.Length > 0) _hint.Text += "\n" + fallback;
        }
        Arrange();
    }

    private int _serviceIndex;
    private void Arrange()
    {
        const float inset = 16;
        float width = Math.Min(560, Size.X - 64), inner = width - inset * 2;
        _title.Position = new(inset, inset); _title.Size = new(inner, 0);
        float top = _title.Position.Y + _title.GetMinimumSize().Y + 24;
        if (IsMenu)
        {
            _commandsList.Position = new(inset, top); _commandsList.Arrange(inner);
            _rows[0].Position = new(inset, top + _commandsList.RequestedHeight + 8);
            _rows[0].Size = new(inner, 64);
            top = _rows[0].Position.Y + _rows[0].Size.Y;
        }
        else
        {
            for (int i = 0; i < _options.Count; i++)
            { _rows[i].Position = new(inset, top); _rows[i].Size = new(inner, 0); top += Math.Max(64, _rows[i].GetMinimumSize().Y) + 12; }
        }
        _hint.Position = new(inset, top + 12); _hint.Size = new(inner, 0);
        _panel.Size = new(width, _hint.Position.Y + _hint.GetMinimumSize().Y + inset);
        // Focus/refusal can grow downward; the root service hit regions stay at their origin.
        _panel.Position = new(Size.X - 32 - width, 112);
    }

    public override void _Process(double delta)
    {
        if (!IsVisibleInTree()) return;
        Arrange();
        foreach (var command in _commands) command.AdvanceFocus(delta);
    }
    internal void Suspend()
    {
        _requiresRelease = true; _pointer.Suspend();
        foreach (var command in _commands) command.StopFocusEmphasis();
    }
    private void ConfigureAssets(IReadOnlyDictionary<string, Sf2.Remake.Application.Content.Scenarios.ExplorationRaster>? rasters) =>
        MenuRasterTextures.Configure(_commands, rasters, "", "church-menu");
    private void Choose(ChurchCommand? command)
    {
        if (IsMenu) _serviceIndex = _index;
        _requiresRelease = true; _pointer.Suspend();
        if (command is null) { _error = GameText.Get("church.promotion"); Present(_session, _input, _submit); return; }
        _requiresRelease = true;
        var result = _submit(command, _revision);
        if (result.Failure is { } failure) _error = GameText.Get("church.failure", failure.Code, failure.Field, failure.Message);
        Present(_session, _input, _submit);
    }
    internal void Handle(GameAction action)
    {
        _pointer.Cancel(); if (_requiresRelease || _wait is null) return;
        if (action == GameAction.Cancel)
        {
            Choose(_wait.Phase == ChurchPhase.Result ? new ContinueChurch(_wait.Token) : _wait.Offer is { } offer
                ? new AnswerChurch(_wait.Token, offer, false) : new LeaveChurch(_wait.Token)); return;
        }
        if (action is GameAction.Up or GameAction.Down) Focus((_index + (action == GameAction.Up ? -1 : 1) + _options.Count) % _options.Count);
        else if (action == GameAction.Confirm) Choose(_options[_index].Command);
        Present(_session, _input, _submit);
    }
    internal void HandlePointer(InputEvent input)
    {
        if (_wait is null) return;
        Vector2 position = ((InputEventMouse)input).Position;
        int hit = IsMenu ? _commandsList.Hit(position).Index :
            _rows.FindIndex(row => row.Visible && row.GetGlobalRect().HasPoint(position));
        bool back = IsMenu && _rows[0].GetGlobalRect().HasPoint(position);
        if (input is InputEventMouseMotion motion)
        {
            if (_pointer.Moved(motion) && (hit >= 0 || back) && !_requiresRelease) { Focus(back ? 4 : hit); Present(_session, _input, _submit); }
            return;
        }
        if (input is not InputEventMouseButton button) return;
        back |= button.ButtonIndex == MouseButton.Right;
        string? id = back ? "back" : hit < 0 ? null : hit.ToString();
        Rect2 rect = button.ButtonIndex == MouseButton.Right ? GetGlobalRect() : back ? _rows[0].GetGlobalRect() :
            hit < 0 ? default : IsMenu ? _commandsList.Hit(position).Rect : _rows[hit].GetGlobalRect();
        string? clicked = _pointer.Read(button, id, rect, (_wait.Token, _revision, IsMenu));
        if (!_requiresRelease && clicked is not null)
        {
            if (clicked == "back") Handle(GameAction.Cancel);
            else { Focus(int.Parse(clicked)); Choose(_options[_index].Command); }
        }
    }
    private void Focus(int index)
    {
        if (_index != index) _error = null;
        _index = index;
    }
    private static string EffectKey(ChurchEffect effect) => effect switch
    { ChurchEffect.Poison => "poison", ChurchEffect.Stun => "stun", _ => "raise" };
    private static string Treatment(ChurchEffect effect) => GameText.Get("church." + EffectKey(effect));
    internal object Observe() => new { visible = IsVisibleInTree(), wait = _wait, index = _index, revision = _revision,
        title = _title.Text, hint = _hint.Text, error = _error, page = IsMenu ? "services" : _wait?.Phase == ChurchPhase.Offer ? "offer" : "result",
        commandList = _commandsList.Observe(),
        commands = _commands.Select(button => new { id = button.Identity, label = button.Caption, selected = button.Selected, enabled = button.Available,
            rect = BattleMapViewport.Rectangle(button.GetGlobalRect()), art = button.ObserveArt() }),
        panelRect = BattleMapViewport.Rectangle(_panel.GetGlobalRect()), titleRect = BattleMapViewport.Rectangle(_title.GetGlobalRect()), titleMinimumHeight = _title.GetMinimumSize().Y,
        hintRect = BattleMapViewport.Rectangle(_hint.GetGlobalRect()), hintMinimumHeight = _hint.GetMinimumSize().Y, requiresRelease = _requiresRelease,
        rows = _rows.Where(row => row.Visible).Select(row => new { text = row.Text, rect = BattleMapViewport.Rectangle(row.GetGlobalRect()) }) };
    private static Label Text(string name, int size)
    { var label = new Label { Name = name, AutowrapMode = TextServer.AutowrapMode.WordSmart, MouseFilter = MouseFilterEnum.Ignore }; label.AddThemeFontSizeOverride("font_size", size); return label; }
}
