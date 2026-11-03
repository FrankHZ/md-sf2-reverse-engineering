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

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Stop;
        var shade = new ColorRect { Color = new(0, 0, 0, 0.8f), MouseFilter = MouseFilterEnum.Ignore };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade); AddChild(_panel);
        _panel.Position = new(424, 160); _panel.Size = new(1072, 824);
        _panel.AddChild(_title); _title.Position = new(32, 24); _title.Size = new(1008, 150);
        _title.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _hint.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _panel.AddChild(_hint); _hint.Position = new(32, 612); _hint.Size = new(1008, 192);
        Hide();
    }

    internal void Present(GameSession session, GameInput input, Func<ChurchCommand, long, SessionResult> submit)
    {
        _session = session; _input = input; _submit = submit;
        if (session.Current.Story.Wait is not ChurchWait wait) { Hide(); _wait = null; return; }
        Show();
        if (_wait?.Token != wait.Token)
        {
            _revision = session.Current.Revision; _index = 0; _requiresRelease = true; _error = null;
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
                    (GameText.Get("church.save"), new LeaveChurch(wait.Token, true)), (GameText.Get("church.leave"), new LeaveChurch(wait.Token))]);
            }
            while (_rows.Count < _options.Count) { var row = Text("Choice" + _rows.Count, 28); _rows.Add(row); _panel.AddChild(row); }
            for (int i = 0; i < _rows.Count; i++)
            { _rows[i].Visible = i < _options.Count; _rows[i].Position = new(32, 208 + i * 76); _rows[i].Size = new(1008, 66); }
        }
        _pointer.ReleaseIfNeutral(); if (!_input.GameplayHeld) _requiresRelease = false;
        for (int i = 0; i < _options.Count; i++)
        {
            _rows[i].Text = (i == _index ? "▸ " : "   ") + _options[i].Label;
            if (_options[i].Command is not null && i != _index) _rows[i].RemoveThemeColorOverride("font_color");
            else _rows[i].AddThemeColorOverride("font_color", _options[i].Command is null ? StoryTheme.Unavailable : StoryTheme.Selected);
        }
        _hint.Text = (_error is null ? "" : _error + "\n") + GameText.Get("church.hint",
            input.CompactHint(GameAction.Up), input.CompactHint(GameAction.Down), input.CompactHint(GameAction.Confirm), input.CompactHint(GameAction.Cancel));
        if (GameText.Language == "zh-CN")
        {
            _hint.Text += "\n" + GameText.Get("localization.names");
            string fallback = GameText.Fallback("church.");
            if (fallback.Length > 0) _hint.Text += "\n" + fallback;
        }
        float hintHeight = Math.Max(192, _hint.GetMinimumSize().Y);
        _hint.Size = new(1008, hintHeight);
        _panel.Size = new(1072, _hint.Position.Y + hintHeight + 20);
    }

    internal void Suspend() { _requiresRelease = true; _pointer.Suspend(); }
    private void Choose(ChurchCommand? command)
    {
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
        if (input is InputEventMouseMotion motion) { _pointer.Moved(motion); return; }
        if (input is not InputEventMouseButton button || _wait is null) return;
        int hit = _rows.FindIndex(row => row.Visible && row.GetGlobalRect().HasPoint(button.Position));
        string? clicked = _pointer.Read(button, hit < 0 ? null : hit.ToString(), hit < 0 ? default : _rows[hit].GetGlobalRect(), _wait.Token);
        if (!_requiresRelease && clicked is not null) { Focus(int.Parse(clicked)); Choose(_options[_index].Command); }
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
        title = _title.Text, hint = _hint.Text, error = _error,
        panelRect = BattleMapViewport.Rectangle(_panel.GetGlobalRect()), titleRect = BattleMapViewport.Rectangle(_title.GetGlobalRect()), titleMinimumHeight = _title.GetMinimumSize().Y,
        hintRect = BattleMapViewport.Rectangle(_hint.GetGlobalRect()), hintMinimumHeight = _hint.GetMinimumSize().Y, requiresRelease = _requiresRelease,
        rows = _rows.Where(row => row.Visible).Select(row => new { text = row.Text, rect = BattleMapViewport.Rectangle(row.GetGlobalRect()) }) };
    private static Label Text(string name, int size)
    { var label = new Label { Name = name, MouseFilter = MouseFilterEnum.Ignore }; label.AddThemeFontSizeOverride("font_size", size); return label; }
}
