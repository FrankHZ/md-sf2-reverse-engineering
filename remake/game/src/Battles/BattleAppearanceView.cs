using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Battles;

// Authored finite field feedback. Elapsed actual presentation releases the token;
// the view never initializes an actor or generates a turn.
internal sealed partial class BattleAppearanceView : Control
{
    private readonly ContextHint _notice = new() { Name = "AppearanceNotice" };
    private GameSession _session = null!;
    private BattleMapViewport _map = null!;
    private InputSettings _settings = null!;
    private BattleRoundState? _state;
    private double _elapsed;
    private bool _delivered;
    private string Stage => _state is null ? "idle" : _elapsed < 0.6 ? "spin" : _elapsed < 0.8 ? "settle" : "notice";

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); MouseFilter = MouseFilterEnum.Ignore;
        _notice.Text.HorizontalAlignment = HorizontalAlignment.Center;
        AddChild(_notice);
        Hide();
    }
    internal void Configure(GameSession session, BattleMapViewport map, InputSettings settings)
    { _session = session; _map = map; _settings = settings; }
    internal void Present()
    {
        var round = _session.Current.BattleRound;
        if (round?.Actor is null) { Hide(); _state = null; return; }
        if (_state?.Token != round.Token) { _elapsed = 0; _delivered = false; }
        _state = round; Show();
        _notice.Visible = Stage == "notice";
        if (_notice.Visible)
        {
            var actor = _session.Current.Battle.GetActor(round.Actor.Value);
            string name = DisplayNames.Battle(_session.Current.Story, actor, _session.ActiveBattleScenes);
            _notice.Text.Text = GameText.Get("battle.appearance", name);
            _notice.Arrange(Size);
        }
        _map.PresentAppearance(round, _session.Current.Battle, (float)Math.Min(1, _elapsed / 0.6), _settings.ReducedFlash);
    }
    internal SessionCommand? Consume(double delta)
    {
        if (_state is null || _delivered || !IsVisibleInTree() || !CanProcess()) return null;
        _elapsed += delta;
        Present();
        if (_elapsed < 1.5) return null;
        _delivered = true;
        return new CompletePresentation(_state.Token, _state.CompletionKind);
    }
    internal object Observe() => new { active = _state is not null, actor = _state?.Actor?.Value,
        cursor = _state?.Cursor, list = _state?.Appearances.Select(a => a.Value), token = _state?.Token.Value,
        stage = Stage, elapsed = _elapsed, delivered = _delivered, suspended = !CanProcess(),
        text = _notice.Text.Text, noticeVisible = _notice.IsVisibleInTree(), authored = true };
}
