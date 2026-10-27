using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Battles;

// Authored finite field feedback. Elapsed actual presentation releases the token;
// the view never initializes an actor or generates a turn.
internal sealed partial class BattleAppearanceView : Control
{
    private Label _notice = null!;
    private GameSession _session = null!;
    private BattleMapViewport _map = null!;
    private InputSettings _settings = null!;
    private BattleRoundState? _state;
    private double _elapsed;
    private bool _delivered, _suspended;
    private string Stage => _state is null ? "idle" : _elapsed < 0.6 ? "spin" : _elapsed < 0.8 ? "settle" : "notice";

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); MouseFilter = MouseFilterEnum.Ignore;
        _notice = new Label { Name = "AppearanceNotice", HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top, MouseFilter = MouseFilterEnum.Ignore };
        AddChild(_notice); _notice.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _notice.AddThemeColorOverride("font_color", Colors.Gold); Hide();
    }
    public override void _EnterTree()
    { GetWindow().FocusExited += Suspend; GetWindow().FocusEntered += Resume; }
    public override void _ExitTree()
    { GetWindow().FocusExited -= Suspend; GetWindow().FocusEntered -= Resume; }
    private void Suspend() => _suspended = true;
    private void Resume() => _suspended = false;
    internal void Configure(GameSession session, BattleMapViewport map, InputSettings settings)
    { _session = session; _map = map; _settings = settings; }
    internal void Present()
    {
        var round = _session.Current.BattleRound;
        if (round?.Actor is null) { Hide(); _state = null; return; }
        if (_state?.Token != round.Token) { _elapsed = 0; _delivered = false; }
        _state = round; Show();
        _notice.Text = $"{round.Actor.Value.Value} emerges!";
        _notice.Visible = Stage == "notice";
        _map.PresentAppearance(round, _session.Current.Battle, (float)Math.Min(1, _elapsed / 0.6), _settings.ReducedFlash);
    }
    internal SessionCommand? Consume(double delta)
    {
        if (_state is null || _delivered || _suspended || !IsVisibleInTree() || !CanProcess()) return null;
        _elapsed += delta;
        Present();
        if (_elapsed < 1.5) return null;
        _delivered = true;
        return new CompletePresentation(_state.Token, _state.CompletionKind);
    }
    internal object Observe() => new { active = _state is not null, actor = _state?.Actor?.Value,
        cursor = _state?.Cursor, list = _state?.Appearances.Select(a => a.Value), token = _state?.Token.Value,
        stage = Stage, elapsed = _elapsed, delivered = _delivered, suspended = _suspended,
        text = _notice.Text, noticeVisible = _notice.Visible, authored = true };
}
