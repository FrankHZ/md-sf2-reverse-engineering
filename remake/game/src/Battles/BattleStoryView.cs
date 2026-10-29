using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Battles;

// Finite authored text/camera delivery. Gameplay PC, aliases and control stay in GameSession.
internal sealed partial class BattleStoryView : Control
{
    private Label _text = null!;
    private GameSession _session = null!;
    private BattleMapViewport _map = null!;
    private ProgramWait? _wait;
    private ulong _mountedFrame;
    private bool _delivered, _suspended;
    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); MouseFilter = MouseFilterEnum.Ignore;
        _text = new Label { Name = "BattleDialogue", HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom, AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = MouseFilterEnum.Ignore };
        _text.AddThemeColorOverride("font_color", Colors.Gold);
        _text.AddThemeFontSizeOverride("font_size", 30);
        AddChild(_text); _text.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _text.OffsetLeft = 32; _text.OffsetRight = -32; _text.OffsetBottom = -32; Hide();
    }
    public override void _EnterTree()
    { GetWindow().FocusExited += Suspend; GetWindow().FocusEntered += Resume; }
    public override void _ExitTree()
    { GetWindow().FocusExited -= Suspend; GetWindow().FocusEntered -= Resume; }
    private void Suspend() => _suspended = true;
    private void Resume() => _suspended = false;
    internal void Configure(GameSession session, BattleMapViewport map) { _session = session; _map = map; }
    internal void Present()
    {
        var current = _session.Current;
        if (current.BattleRound is not { Program: true }) { Hide(); _wait = null; return; }
        Show();
        if (_wait?.Token != current.Story.Wait?.Token)
        { _mountedFrame = Engine.GetProcessFrames(); _delivered = false; }
        _wait = current.Story.Wait;
        _text.Text = _wait is DialogueWait dialogue ? _session.Definition.Texts[dialogue.Text] + "\nConfirm to continue" : "";
        _text.Visible = _wait is DialogueWait;
        _map.PresentStoryCamera(current);
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
        _delivered = true; return new Acknowledge(dialogue.Token);
    }
    internal object Observe() => new { active = _session.Current.BattleRound is { Program: true },
        cursor = _session.Current.Story.Cursor, callers = _session.Current.Story.Callers, token = _wait?.Token.Value,
        kind = _wait?.GetType().Name, text = _text.Text, visible = _text.IsVisibleInTree(),
        delivered = _delivered, presented = Engine.GetProcessFrames() > _mountedFrame, suspended = _suspended,
        cameraSlot = _session.Current.Story.CameraEntitySlot, lists = _session.Current.Story.PartyLists };
}
