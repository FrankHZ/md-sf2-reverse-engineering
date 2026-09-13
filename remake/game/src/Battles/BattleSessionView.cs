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
    private Control _board = null!;
    private readonly Dictionary<ActorRef, Label> _markers = [];
    private readonly List<ColorRect> _preview = [];
    private const int CellSize = 40;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        _title = AddLabel("Title", new(20, 12));
        _status = AddLabel("Status", new(20, 42));
        _board = new Control { Name = "Board", Position = new(20, 80), MouseFilter = MouseFilterEnum.Ignore };
        AddChild(_board);
        _roster = AddLabel("Roster", new(540, 100));
        var help = AddLabel("Help", new(540, 240));
        help.Text = "WASD / arrows: movement preview\nEnter: choose action / commit\nH: HEAL, initially target self\nTab: cycle living allied targets\nSpace: STAY\nEsc: cancel all provisional choices\nX: physical action capability\n\nAI and rounds advance automatically.\nProject-authored controlled start.";
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
            BuildBoard(started.Result.Snapshot.Battle);
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
            int selected = Array.FindIndex(targets, a => a.Actor == targeting.Target);
            Send(new SelectTarget(targets[(selected + 1) % targets.Length].Actor));
        }
        else return;
        GetViewport().SetInputAsHandled();
    }

    private void Send(SessionCommand command)
    {
        var current = _session!.Current;
        _result = _session.Submit(new(current.SessionId, current.Revision, current.Selection?.Actor, command));
        Present();
    }

    private void BuildBoard(EngineBattleState battle)
    {
        for (int y = 0; y < battle.Definition.Height; y++)
            for (int x = 0; x < battle.Definition.Width; x++)
            {
                byte tile = battle.Definition.Terrain[y * 48 + x];
                _board.AddChild(new ColorRect { Position = new(x * CellSize, y * CellSize), Size = new(CellSize - 2, CellSize - 2),
                    Color = tile == 255 ? new(0.12f, 0.14f, 0.18f) : tile == 3 ? new(0.19f, 0.34f, 0.24f) : new(0.25f, 0.29f, 0.35f),
                    MouseFilter = MouseFilterEnum.Ignore });
            }
        foreach (var actor in battle.Actors)
        {
            var marker = new Label { Name = "Actor_" + actor.Actor.Value, MouseFilter = MouseFilterEnum.Ignore };
            marker.AddThemeFontSizeOverride("font_size", 10);
            _board.AddChild(marker);
            _markers.Add(actor.Actor, marker);
        }
    }

    private void Present()
    {
        var projection = BattleSnapshotProjection.Project(_result!);
        _title.Text = projection.Title;
        _status.Text = projection.Status;
        _roster.Text = projection.Roster;
        foreach (var marker in _markers.Values) marker.Visible = false;
        foreach (var actor in projection.Markers)
        {
            var node = _markers[actor.Actor];
            node.Visible = true;
            node.Position = new(actor.Position.X * CellSize + 2, actor.Position.Y * CellSize + 4);
            node.Text = actor.Text;
            node.Modulate = actor.Selected ? Colors.Gold : actor.Ally ? Colors.LightSkyBlue : Colors.LightCoral;
        }
        foreach (var node in _preview) { _board.RemoveChild(node); node.QueueFree(); }
        _preview.Clear();
        foreach (var position in projection.Preview)
        {
            var node = new ColorRect { Position = new(position.X * CellSize + 4, position.Y * CellSize + CellSize - 7),
                Size = new(CellSize - 10, 3), Color = Colors.Gold, MouseFilter = MouseFilterEnum.Ignore };
            _board.AddChild(node);
            _preview.Add(node);
        }
    }

    private Label AddLabel(string name, Vector2 position)
    {
        var label = new Label { Name = name, Position = position, MouseFilter = MouseFilterEnum.Ignore };
        AddChild(label);
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
            map = current?.Battle.Definition.Map.Value, actor = current?.Selection?.Actor.Value,
            stage = current?.Selection?.Stage.ToString(), stopReason = _result?.StopReason.ToString(),
            previewX = current?.Selection?.Preview.Destination.X, previewY = current?.Selection?.Preview.Destination.Y,
            actors = current?.Battle.Actors.Select(a => new { id = a.Actor.Value, hp = a.Hp, mp = a.Mp, exp = a.Exp,
                x = a.Position.X, y = a.Position.Y, nodeX = _markers[a.Actor].Position.X,
                nodeY = _markers[a.Actor].Position.Y, visible = _markers[a.Actor].Visible, text = _markers[a.Actor].Text }),
            observations = _result?.Observations,
            title = _title.Text, status = _status.Text, roster = _roster.Text,
            boardChildren = _board.GetChildCount(), previewNodes = _preview.Count,
        });
    }
}
