using Godot;
using Sf2.Remake.GodotAdapter.UI;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Battles;

// Finite authored text/camera delivery. Gameplay PC, aliases and control stay in GameSession.
internal sealed partial class BattleStoryView : Control
{
    private StoryDialoguePanel _panel = null!;
    private Label _text => _panel.Body;
    private GameInput _input = null!;
    private double _revealed;
    private Rect2 _subject;
    private GameSession _session = null!;
    private BattleMapViewport _map = null!;
    private ProgramWait? _wait;
    private ulong _mountedFrame;
    private bool _delivered, _suspended;
    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); MouseFilter = MouseFilterEnum.Ignore;
        _panel = new StoryDialoguePanel { Name = "DialoguePanel" };
        AddChild(_panel); Hide();
    }
    public override void _Process(double delta)
    {
        if (_suspended || !IsVisibleInTree() || !GetWindow().HasFocus() || !CanProcess()) return;
        if (_text.VisibleCharacters >= 0)
        {
            _revealed = Math.Min(_text.GetTotalCharacterCount(), _revealed + delta * _input.Settings.CharactersPerSecond);
            _text.VisibleCharacters = (int)_revealed;
        }
        Hint();
    }
    private void Hint() => _panel.SetHint(_wait is DialogueWait
        ? $"{_input.Hint(GameAction.Confirm)}: {(Revealed ? _panel.MoreBelow ? "Read more" : "Continue ▾" : "Reveal")}" : "Scene in progress…");
    private bool Revealed => _text.VisibleCharacters < 0 || _text.VisibleCharacters >= _text.GetTotalCharacterCount();
    public override void _EnterTree()
    { GetWindow().FocusExited += Suspend; GetWindow().FocusEntered += Resume; }
    public override void _ExitTree()
    { GetWindow().FocusExited -= Suspend; GetWindow().FocusEntered -= Resume; }
    private void Suspend() => _suspended = true;
    private void Resume() => _suspended = false;
    internal void Configure(GameSession session, BattleMapViewport map, GameInput input)
    {
        if (_map is not null) _map.SubjectChanged -= ObserveSubject;
        _session = session; _map = map; _input = input;
        _map.SubjectChanged += ObserveSubject;
    }
    private void ObserveSubject(Rect2 subject) => _subject = subject;
    internal void Present()
    {
        var current = _session.Current;
        if (current.BattleRound is not { Program: true }) { Hide(); _panel.Present(false, false, false, default, "", false); _wait = null; return; }
        Show();
        if (_wait?.Token != current.Story.Wait?.Token)
        { _mountedFrame = Engine.GetProcessFrames(); _delivered = false; _revealed = 0; _panel.ResetReading();
            _text.VisibleCharacters = _input.Settings.TextMode == "instant" ? -1 : 0; }
        _wait = current.Story.Wait;
        _text.Text = _wait is DialogueWait dialogue ? _session.Definition.Texts[dialogue.Text] : "";
        _map.PresentStoryCamera(current);
        _panel.Present(_wait is DialogueWait, false, false, _subject, "", _wait is DialogueWait);
        Hint();
    }
    private bool DeliveryReady => !_suspended && !_delivered && IsVisibleInTree() && CanProcess() && Engine.GetProcessFrames() > _mountedFrame;
    internal SessionCommand? Consume()
    {
        if (!DeliveryReady) return null;
        if (_wait is PresentationWait camera && _map.CameraSettled)
        { _delivered = true; return new CompletePresentation(camera.Token, camera.Cue.Kind); }
        if (_wait is TickWait ticks) return new AdvanceSimulation(ticks.Token);
        if (_wait is null && _session.Current.StopReason == SessionStopReason.SimulationWait) return new AdvanceSimulation();
        return null;
    }
    internal SessionCommand? Acknowledge(GameAction action)
    {
        if (!DeliveryReady || action != GameAction.Confirm || _wait is not DialogueWait dialogue || !_text.IsVisibleInTree()) return null;
        if (!Revealed) { _revealed = _text.GetTotalCharacterCount(); _text.VisibleCharacters = (int)_revealed; Hint(); return null; }
        if (_panel.ReadMore()) return null;
        _delivered = true; return new Acknowledge(dialogue.Token);
    }
    internal object Observe() => new { active = _session.Current.BattleRound is { Program: true },
        cursor = _session.Current.Story.Cursor, callers = _session.Current.Story.Callers, token = _wait?.Token.Value,
        kind = _wait?.GetType().Name, text = _text.Text, visible = _text.IsVisibleInTree(),
        delivered = _delivered, presented = Engine.GetProcessFrames() > _mountedFrame, suspended = _suspended,
        panel = _panel.Observe(), visibleCharacters = _text.VisibleCharacters, totalCharacters = _text.GetTotalCharacterCount(),
        cameraSlot = _session.Current.Story.CameraEntitySlot, lists = _session.Current.Story.PartyLists };
}
