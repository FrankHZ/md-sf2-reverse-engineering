using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter.Battles;

public sealed partial class BattleSessionView : Control
{
    private GameSession? _session;
    private SessionResult? _result;
    private SessionFailure? _startupFailure;
    private bool _started;
    private Label _title = null!;
    private Label _status = null!;
    private Label _roster = null!;
    private BattleMapViewport _map = null!;
    private ScrollContainer _hud = null!;
    private VBoxContainer _hudContent = null!;
    private readonly List<Label> _hudLabels = [];
    // Last attempted UI candidate, including rejected attempts. Never the gameplay selected target.
    private ActorRef? _targetCandidate;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        // This authored UI responds to the actual window; legacy reference composition keeps its policy.
        GetWindow().ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
        _map = new BattleMapViewport { Name = "MapViewport" };
        AddChild(_map);
        _hud = new ScrollContainer { Name = "Hud", HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            FocusMode = FocusModeEnum.None };
        AddChild(_hud);
        _hud.GetVScrollBar().FocusMode = FocusModeEnum.None;
        _hudContent = new VBoxContainer { Name = "Content", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _hudContent.AddThemeConstantOverride("separation", 12);
        _hud.AddChild(_hudContent);
        _title = AddLabel("Title");
        _status = AddLabel("Status");
        _roster = AddLabel("Roster");
        var help = AddLabel("Help");
        help.Text = "WASD / arrows: movement preview\nEnter: choose action / commit\nH: HEAL, initially target self\nTab: cycle living allied targets\nSpace: STAY\nEsc: cancel all provisional choices\nX: physical action capability\n\nAI and rounds advance automatically.\nProject-authored controlled start.";
        GetViewport().SizeChanged += Arrange;
        Arrange();
    }

    public override void _ExitTree() => GetViewport().SizeChanged -= Arrange;

    private void Arrange()
    {
        var size = GetViewportRect().Size;
        const int margin = 16;
        if (size.X >= 800)
        {
            float hudWidth = Mathf.Clamp(size.X * 0.32f, 260, 380);
            _map.Position = new(margin, margin);
            _map.Size = new(Mathf.Max(1, size.X - 3 * margin - hudWidth), Mathf.Max(1, size.Y - 2 * margin));
            _hud.Position = new(_map.Position.X + _map.Size.X + margin, margin);
            _hud.Size = new(hudWidth, _map.Size.Y);
        }
        else
        {
            float height = Mathf.Max(2, size.Y - 3 * margin);
            _map.Position = new(margin, margin);
            _map.Size = new(Mathf.Max(1, size.X - 2 * margin), height * 0.6f);
            _hud.Position = new(margin, _map.Position.Y + _map.Size.Y + margin);
            _hud.Size = new(_map.Size.X, height * 0.4f);
        }
        float textWidth = Mathf.Max(1, _hud.Size.X - _hud.GetVScrollBar().GetCombinedMinimumSize().X - 4);
        foreach (var label in _hudLabels) label.CustomMaximumSize = new(textWidth, -1);
    }

    internal void Begin(IScenarioSource source)
    {
        if (_started) throw new InvalidOperationException("A view starts one session only.");
        _started = true;
        var outcome = GameSession.Start(source);
        if (outcome is SessionStarted started)
        {
            _session = started.Session;
            _result = started.Result;
            _map.Build(started.Result.Snapshot.Battle.Definition);
            Present();
        }
        else if (outcome is SessionStartFailed failed) FailStartup(failed.Failure);
    }

    internal void FailStartup(SessionFailure failure)
    {
        _startupFailure = failure;
        _title.Text = "AUTHORED BATTLE UNAVAILABLE";
        _status.Text = $"{failure.Kind}: {failure.Code} ({failure.Field})";
    }

    public override void _Process(double delta)
    {
        if (_session?.Current.StopReason == SessionStopReason.SimulationWait) Send(new AdvanceSimulation());
    }

    public override void _UnhandledInput(InputEvent input)
    {
        if (_session is null || input is not InputEventKey { Pressed: true, Echo: false } key) return;
        SessionCommand? command = key.Keycode switch
        {
            Key.W or Key.Up => new Move(ExplorationDirection.North),
            Key.D or Key.Right => new Move(ExplorationDirection.East),
            Key.S or Key.Down => new Move(ExplorationDirection.South),
            Key.A or Key.Left => new Move(ExplorationDirection.West),
            Key.Enter or Key.KpEnter => new Confirm(),
            Key.Space => new ChooseAction(SessionAction.Stay),
            Key.Escape => new Cancel(),
            Key.X => new ChooseAction(SessionAction.PhysicalAttack),
            _ => null,
        };
        if (command is not null) Send(command);
        else if (key.Keycode == Key.H && _session.Current.Selection is { } selection)
        {
            var spell = _session.Current.Battle.GetActor(selection.Actor).Definition.Spells.FirstOrDefault();
            Send(new SelectSpell(spell));
            if (_result?.Failure is null) Send(new SelectTarget(selection.Actor));
        }
        else if (key.Keycode == Key.Tab && _session.Current.Selection is { Spell: not null } targeting)
        {
            var targets = _session.Current.Battle.Actors.Where(a => a.Hp > 0 && a.Definition.IsAlly).ToArray();
            int selected = Array.FindIndex(targets, a => a.Actor == (_targetCandidate ?? targeting.Target));
            Send(new SelectTarget(targets[(selected + 1) % targets.Length].Actor));
        }
        else return;
        GetViewport().SetInputAsHandled();
    }

    private void Send(SessionCommand command)
    {
        var current = _session!.Current;
        if (command is SelectTarget target) _targetCandidate = target.Target;
        _result = _session.Submit(new(current.SessionId, current.Revision, current.Selection?.Actor, command));
        if (_result.Snapshot.Selection?.Spell is null) _targetCandidate = null;
        Present();
    }

    private void Present()
    {
        var projection = BattleSnapshotProjection.Project(_result!, _targetCandidate);
        _title.Text = projection.Title;
        _status.Text = projection.Status;
        _roster.Text = projection.Roster;
        _map.Present(projection);
    }

    private Label AddLabel(string name)
    {
        var label = new Label { Name = name, AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
        _hudContent.AddChild(label);
        _hudLabels.Add(label);
        return label;
    }

    // Read-only native observation of the same semantic result and actual projected nodes.
    public string ReadObservationJson()
    {
        var current = _session?.Current;
        return JsonSerializer.Serialize(new
        {
            failure = (_result?.Failure ?? _startupFailure)?.Code,
            failureKind = (_result?.Failure ?? _startupFailure)?.Kind.ToString(),
            round = current?.Battle.Round, revision = current?.Revision,
            mainSeed = current?.Battle.MainSeed, thinkingSeed = current?.Battle.ThinkingSeed,
            map = current?.Battle.Definition.Map.Value, mapWidth = current?.Battle.Definition.Width,
            mapHeight = current?.Battle.Definition.Height, actor = current?.Selection?.Actor.Value,
            target = current?.Selection?.Target?.Value, candidate = _targetCandidate?.Value,
            stage = current?.Selection?.Stage.ToString(), stopReason = _result?.StopReason.ToString(),
            previewX = current?.Selection?.Preview.Destination.X, previewY = current?.Selection?.Preview.Destination.Y,
            actors = current is null ? null : _map.ObserveActors(current.Battle.Actors),
            observations = _result?.Observations,
            title = _title.Text, status = _status.Text, roster = _roster.Text,
            boardChildren = _map.BoardChildren, previewNodes = _map.PreviewCount,
            viewport = BattleMapViewport.Rectangle(GetViewportRect()), mapViewport = BattleMapViewport.Rectangle(_map.GetGlobalRect()),
            hud = BattleMapViewport.Rectangle(_hud.GetGlobalRect()), hudClipsContents = _hud.ClipContents,
            hudContentHeight = _hudContent.Size.Y,
            hudLabels = _hudLabels.Select(label => new { name = label.Name.ToString(), rect = BattleMapViewport.Rectangle(label.GetGlobalRect()) }),
            hudScroll = _hud.ScrollVertical, hudScrollMaximum = _hud.GetVScrollBar().MaxValue, hudScrollPage = _hud.GetVScrollBar().Page,
            previewRect = _map.PreviewRectangle, previewInsideMap = _map.PreviewInsideMap, zoom = _map.Zoom,
        });
    }
}
