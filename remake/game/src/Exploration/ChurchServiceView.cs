using Godot;
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
    private int _index;
    private bool _requiresRelease = true;
    private string? _error;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Stop;
        var shade = new ColorRect { Color = new(0, 0, 0, 0.8f), MouseFilter = MouseFilterEnum.Ignore };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade); AddChild(_panel);
        _panel.Position = new(424, 160); _panel.Size = new(1072, 760);
        _panel.AddChild(_title); _title.Position = new(32, 24); _title.Size = new(1008, 150);
        _title.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _panel.AddChild(_hint); _hint.Position = new(32, 660); _hint.Size = new(1008, 80);
        Hide();
    }

    internal void Present(GameSession session, GameInput input, Func<ChurchCommand, long, SessionResult> submit)
    {
        _session = session; _input = input; _submit = submit;
        if (session.Current.Story.Wait is not ChurchWait wait) { Hide(); _wait = null; return; }
        Show();
        if (_wait?.Token != wait.Token)
        {
            _wait = wait; _revision = session.Current.Revision; _index = 0; _requiresRelease = true; _error = null;
            _pointer.Suspend(); _options.Clear();
            if (wait.Message is not null)
            {
                _title.Text = wait.Message;
                _options.Add(("Continue", new ContinueChurch(wait.Token)));
            }
            else if (wait.Offer is { } offer)
            {
                var member = session.QueryPartyDetails()!.Members.Single(row => row.Carried.Actor == offer.Member);
                _title.Text = $"{offer.Effect} · {PartyStatusView.MemberName(member, session)}\nCost: {offer.Cost} gold. Current gold: {session.Current.Exploration!.Party.Gold}";
                _options.Add(("Confirm treatment", new AnswerChurch(wait.Token, offer, true)));
                _options.Add(("Decline this treatment", new AnswerChurch(wait.Token, offer, false)));
            }
            else
            {
                _title.Text = $"Church · Gold {session.Current.Exploration!.Party.Gold}\nYour desire will be fulfilled.";
                _options.AddRange([("Raise", new ChooseChurch(wait.Token, ChurchAction.Raise)),
                    ("Cure", new ChooseChurch(wait.Token, ChurchAction.Cure)), ("Promote · Unavailable", null),
                    ("Save… · Close Church first", new LeaveChurch(wait.Token, true)), ("Leave", new LeaveChurch(wait.Token))]);
            }
            while (_rows.Count < _options.Count) { var row = Text("Choice" + _rows.Count, 28); _rows.Add(row); _panel.AddChild(row); }
            for (int i = 0; i < _rows.Count; i++)
            { _rows[i].Visible = i < _options.Count; _rows[i].Position = new(32, 208 + i * 76); _rows[i].Size = new(1008, 66); }
        }
        _pointer.ReleaseIfNeutral(); if (!_input.GameplayHeld) _requiresRelease = false;
        for (int i = 0; i < _options.Count; i++)
        { _rows[i].Text = (i == _index ? "▸ " : "   ") + _options[i].Label; _rows[i].Modulate = i == _index ? StoryTheme.Selected : Colors.White; }
        _hint.Text = _error ?? $"{input.Hint(GameAction.Up)} / {input.Hint(GameAction.Down)}: Choose   {input.Hint(GameAction.Confirm)}: Select\n{input.Hint(GameAction.Cancel)}: Decline / Back   Mouse: select a row";
    }

    internal void Suspend() { _requiresRelease = true; _pointer.Suspend(); }
    private void Choose(ChurchCommand? command)
    {
        if (command is null) { _error = "Promotion is unavailable in the admitted opening content."; return; }
        _requiresRelease = true;
        var result = _submit(command, _revision);
        if (result.Failure is { } failure) _error = "Could not complete: " + failure.Message;
        Present(_session, _input, _submit);
    }
    internal void Handle(GameAction action)
    {
        _pointer.Cancel(); if (_requiresRelease || _wait is null) return;
        if (action == GameAction.Cancel)
        {
            Choose(_wait.Message is not null ? new ContinueChurch(_wait.Token) : _wait.Offer is { } offer
                ? new AnswerChurch(_wait.Token, offer, false) : new LeaveChurch(_wait.Token)); return;
        }
        if (action is GameAction.Up or GameAction.Down) _index = (_index + (action == GameAction.Up ? -1 : 1) + _options.Count) % _options.Count;
        else if (action == GameAction.Confirm) Choose(_options[_index].Command);
        Present(_session, _input, _submit);
    }
    internal void HandlePointer(InputEvent input)
    {
        if (input is InputEventMouseMotion motion) { _pointer.Moved(motion); return; }
        if (input is not InputEventMouseButton button || _wait is null) return;
        int hit = _rows.FindIndex(row => row.Visible && row.GetGlobalRect().HasPoint(button.Position));
        string? clicked = _pointer.Read(button, hit < 0 ? null : hit.ToString(), hit < 0 ? default : _rows[hit].GetGlobalRect(), _wait.Token);
        if (!_requiresRelease && clicked is not null) { _index = int.Parse(clicked); Choose(_options[_index].Command); }
    }
    internal object Observe() => new { visible = IsVisibleInTree(), wait = _wait, index = _index, revision = _revision,
        title = _title.Text, error = _error, requiresRelease = _requiresRelease,
        rows = _rows.Where(row => row.Visible).Select(row => new { text = row.Text, rect = BattleMapViewport.Rectangle(row.GetGlobalRect()) }) };
    private static Label Text(string name, int size)
    { var label = new Label { Name = name, MouseFilter = MouseFilterEnum.Ignore }; label.AddThemeFontSizeOverride("font_size", size); return label; }
}
