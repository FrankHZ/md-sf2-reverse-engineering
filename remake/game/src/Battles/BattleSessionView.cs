using System.Text.Json;
using Sf2.Remake.GodotAdapter.Observation;
using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Audio;

namespace Sf2.Remake.GodotAdapter.Battles;

public sealed partial class BattleSessionView : Control
{
    [Signal]
    public delegate void SessionResultObservedEventHandler(string resultJson);

    private static readonly StringName FactsSignal = "SessionFactsObserved";
    public BattleSessionView() => AddUserSignal(FactsSignal,
        [new Godot.Collections.Dictionary { ["name"] = "facts", ["type"] = (int)Variant.Type.Dictionary }]);

    private void PublishResult(string boundary)
    {
        if (!HasConnections(SignalName.SessionResultObserved) && !HasConnections(FactsSignal)) return;
        var result = _result!;
        var facts = new
        {
            boundary, sessionId = result.Snapshot.SessionId, revision = result.Snapshot.Revision,
            observationSequence = result.Snapshot.ObservationSequence, mode = result.Snapshot.Mode.ToString(),
            stopReason = result.StopReason.ToString(), failure = result.Failure, observations = result.Observations,
            programControlReads = result.ProgramControlReads ?? [],
        };
        if (HasConnections(FactsSignal) && ObservationCapture.Find(this)?.IsAccepting() != false) EmitSignal(FactsSignal, ObservationCapture.ToDictionary(facts));
        if (HasConnections(SignalName.SessionResultObserved))
            EmitSignal(SignalName.SessionResultObserved, JsonSerializer.Serialize(facts));
    }

    private GameSession? _session;
    private SessionResult? _result;
    private SessionFailure? _startupFailure;
    private bool _started;
    private string _title = "", _status = "", _roster = "";
    private GameInput _input = null!;
    private BattleMapViewport _map = null!;
    private BattleCommandMenu _menu = null!;
    private BattleSceneView _scene = null!;
    internal SessionAudio? Audio { get; set; }
    internal Action<SessionResult>? LeaveBattle { get; set; }
    internal Action<SessionResult>? ObserveResult { get; set; }
    internal Action<int>? PlayUiSound { get; set; }

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        // This common battle UI responds to the actual window; legacy reference composition keeps its policy.
        GetWindow().ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
        _map = new BattleMapViewport { Name = "MapViewport" };
        AddChild(_map);
        _menu = new BattleCommandMenu { Name = "CommandMenu", Submit = Send };
        AddChild(_menu);
        _menu.LayoutChanged = () => { _map.Position = _menu.MapRect.Position; _map.Size = _menu.MapRect.Size; };
        _scene = new BattleSceneView { Name = "BattleScene" }; AddChild(_scene);
        Arrange();
    }

    internal void ConfigureInput(GameInput input) => _input = input;

    public override void _EnterTree() => GetViewport().SizeChanged += Arrange;
    public override void _ExitTree()
    {
        ObservationCapture.Find(this)?.ViewDetached("battle");
        GetViewport().SizeChanged -= Arrange;
    }

    private void Arrange()
    {
        if (_map is null || _menu is null) return;
        _menu.Arrange(GetViewportRect().Size);
        _map.Position = _menu.MapRect.Position;
        _map.Size = _menu.MapRect.Size;
    }

    internal void Begin(IScenarioSource source)
    {
        if (_started) throw new InvalidOperationException("A view starts one session only.");
        _started = true;
        var outcome = GameSession.Start(source, Sf2.Remake.Application.Gameplay.RuleCompositions.ForGame());
        if (outcome is SessionStarted started)
        {
            Attach(started.Session, started.Result);
        }
        else if (outcome is SessionStartFailed failed) FailStartup(failed.Failure);
    }

    internal void Attach(GameSession session, SessionResult result)
    {
        if (_session is not null && _session != session) throw new InvalidOperationException("A view attaches one session only.");
        bool first = _session is null;
        _started = true;
        _session = session;
        _result = result;
        if (first) _scene.Configure(session, _input.Settings, Audio);
        PublishResult("attach");
        ObserveResult?.Invoke(result);
        if (_startupFailure is not null) return;
        if (first) _map.Build(result.Snapshot.Battle.Definition, session.Definition);
        Show();
        Present();
    }

    internal void FailStartup(SessionFailure failure)
    {
        _startupFailure = failure;
        _title = "BATTLE UNAVAILABLE";
        _status = $"{failure.Kind}: {failure.Code} ({failure.Field})\n{failure.Message}";
        var error = new Label { Text = _status, Position = new(16, 90) }; AddChild(error);
        _menu.Hide();
    }

    public override void _Process(double delta)
    {
        if (_startupFailure is not null || !IsVisibleInTree()) return;
        if (_session?.Current.BattleMovement is not null)
        {
            if (_map.ConsumeMovement(delta, _input.Settings.ReducedFlash) is { } completion) Send(completion);
            return;
        }
        if (_session?.Current.BattleScene is not null)
        {
            try { if (_scene.Consume(delta) is { } completion) Send(completion); }
            catch (InvalidOperationException error) { FailStartup(new(SessionFailureKind.AdapterError, error.Message, "battleScene", "Battle scene playback is unavailable.")); }
            return;
        }
        if (_startupFailure is null && _session?.Current is { HasBattleControl: true, StopReason: SessionStopReason.SimulationWait }) Send(new AdvanceSimulation());
    }

    internal void HandleAction(GameAction action)
    {
        if (_startupFailure is not null || !IsVisibleInTree()) return;
        if (_session?.Current.BattleMovement is not null) return;
        if (_session?.Current.BattleScene is not null)
        { if (_scene.Acknowledge(action) is { } ack) Send(ack); return; }
        if (_session?.Current.HasBattleControl != true) return;
        if (_menu.Handle(action))
        {
            // UI-only target focus may change without a session revision (disabled candidates).
            if (_session.Current.HasBattleControl || _session.Current.BattleScene is not null) Present();
            return;
        }
        SessionCommand? command = action switch
        {
            GameAction.Up => new Move(ExplorationDirection.North),
            GameAction.Right => new Move(ExplorationDirection.East),
            GameAction.Down => new Move(ExplorationDirection.South),
            GameAction.Left => new Move(ExplorationDirection.West),
            GameAction.Confirm => new Confirm(),
            GameAction.Cancel => new Cancel(),
            _ => null,
        };
        if (command is not null) Send(command);
    }

    private void Send(SessionCommand command)
    {
        var current = _session!.Current;
        _result = _session.Submit(new(current.SessionId, current.Revision, current.Selection?.Actor, command));
        PublishResult("submit");
        ObserveResult?.Invoke(_result);
        if (_result.Failure is null)
        {
            // The modern action-choice lifetime maps to the source outer menu's
            // open/close cue, before the generic selection/confirmation bindings.
            bool wasActionChoice = current.Selection?.Stage == BattleSelectionStage.ActionChoice;
            bool isActionChoice = _result.Snapshot.Selection?.Stage == BattleSelectionStage.ActionChoice;
            if (wasActionChoice != isActionChoice) PlayUiSound?.Invoke(65);
            else if (command is SelectSpell or SelectItem or Cancel) PlayUiSound?.Invoke(66);
            else if (command is Confirm) PlayUiSound?.Invoke(67);
        }
        if (_startupFailure is not null) return;
        if (_result.Snapshot.Mode == SessionMode.Exploration)
        {
            LeaveBattle?.Invoke(_result);
            return;
        }
        Present();
    }

    private WaitToken? _movementSoundToken;
    private void Present()
    {
        if (_session!.Current.BattleMovement is { } movement)
        {
            if (_session.Definition.PrivateDefinitions is not null && _session.Definition.BattleScenes?.FieldDeath is null)
            { FailStartup(new(SessionFailureKind.AdapterError, "field-movement-content-required", "battleScenes.fieldDeath", "Movement sprite content is unavailable.")); return; }
            if (_movementSoundToken != movement.Token)
            { _movementSoundToken = movement.Token; Audio?.PlayEffect(79); }
        }
        _menu.Present(_session.Current, _session.QueryBattleChoices(), _result!.Failure, _input);
        var projection = BattleSnapshotProjection.Project(_result, _menu.TargetCandidate, _session.Definition.Origin);
        _title = projection.Title;
        _status = projection.Status;
        _roster = projection.Roster;
        _map.Present(projection);
        _scene.Present();
        if (_scene.Error is { } error) FailStartup(new(SessionFailureKind.AdapterError, error, "battleScene", "Battle scene playback is unavailable."));
    }

    // Explicit adapter observation input; never a gameplay command or logical Wait.
    public string PlayAudioObservation(int command, string resource = "")
    {
        string? failure = null;
        try
        {
            if (Audio is null) throw new InvalidOperationException("audio-observer-unavailable");
            if (resource.Length == 0) Audio.Play(command); else Audio.Play(resource);
        }
        catch (InvalidOperationException error) { failure = error.Message; }
        return JsonSerializer.Serialize(new { failure, audio = Audio?.ObservePlayback() });
    }

    // Read-only native observation of the same semantic result and actual projected nodes.
    public string ReadSceneObservationJson()
    {
        var current = _session?.Current;
        return JsonSerializer.Serialize(new
        {
            revision = current?.Revision, observationSequence = current?.ObservationSequence,
            mainSeed = current?.Battle.MainSeed, thinkingSeed = current?.Battle.ThinkingSeed,
            cursor = current?.Battle.Cursor, actor = current?.Selection?.Actor.Value, round = current?.Battle.Round,
            failure = (_result?.Failure ?? _startupFailure)?.Code, scene = _scene.Observe(),
            hasBattleControl = current?.HasBattleControl, movement = _map.ObserveMovement(), fieldActors = current is null ? null : _map.ObserveActors(current.Battle.Actors),
            actors = current?.Battle.Actors.Select(actor => new { actor = actor.Actor.Value, hp = actor.Hp,
                mp = actor.Mp, exp = actor.Exp, kills = actor.Kills, defeats = actor.Defeats }),
        });
    }

    public string ReadObservationJson() => JsonSerializer.Serialize(ObservationFacts(true));

    public Godot.Collections.Dictionary ReadCaptureState(bool full = false) =>
        ObservationCapture.ToDictionary(ObservationFacts(full));

    private object ObservationFacts(bool full)
    {
        var current = _session?.Current;
        var choices = _session?.QueryBattleChoices();
        if (current?.Active is ActiveExploration)
            return new
            {
                sessionId = current.SessionId, revision = current.Revision,
                observationSequence = current.ObservationSequence,
                pendingFieldReturn = new
                {
                    logicalView = current.Story.LogicalView, simulationTick = current.Story.SimulationTick,
                    cursor = current.Story.Cursor,
                },
            };
        return new
        {
            origin = _session?.Definition.Origin, sessionId = current?.SessionId, storyFlags = current?.Story.Flags,
            rules = _session?.Rules.Identity, battleChoices = choices, healingChoices = choices,
            scene = _scene.Observe(), movement = _map.ObserveMovement(), observationSequence = current?.ObservationSequence,
            initializationPolicy = current?.Battle.StartPolicy,
            regionFlags = current?.Battle.Regions?.Flags, regionsTested = current?.Battle.Regions?.Tested,
            turnOrder = current?.Battle.TurnOrder.Select(entry => new { actor = entry.Actor?.Value, score = entry.AlteredAgility }),
            failure = (_result?.Failure ?? _startupFailure)?.Code,
            failureKind = (_result?.Failure ?? _startupFailure)?.Kind.ToString(),
            round = current?.Battle.Round, revision = current?.Revision,
            mainSeed = current?.Battle.MainSeed, thinkingSeed = current?.Battle.ThinkingSeed, gold = current?.Battle.Gold,
            queueCursor = current?.Battle.Cursor,
            aiMemory = current?.Battle.Actors.Where(a => a.Control == BattleControl.Automatic)
                .Select(a => new { actor = a.Actor.Value, memory = a.AiMemory, lastTarget = a.LastTarget?.Value }).ToArray(),
            map = current?.Battle.Definition.Map.Value, mapWidth = current?.Battle.Definition.Width,
            mapHeight = current?.Battle.Definition.Height, actor = current?.Selection?.Actor.Value,
            terrain = full ? current?.Battle.Definition.Terrain.Select(tile => tile.Surface.ToString()) : null,
            target = current?.Selection?.Target?.Value, candidate = _menu.TargetCandidate?.Value,
            itemSlot = current?.Selection?.ItemSlot,
            inventories = current?.Battle.Actors.Select(a => new { actor = a.Actor.Value, items = a.SourceLoadout?.Items }),
            spell = current?.Selection?.Spell,
            stage = current?.Selection?.Stage.ToString(), stopReason = _result?.StopReason.ToString(),
            previewX = current?.Selection?.Preview.Destination.X, previewY = current?.Selection?.Preview.Destination.Y,
            actors = current is null ? null : _map.ObserveActors(current.Battle.Actors),
            observations = _result?.Observations,
            title = _title, status = _status, roster = _roster, menu = _menu.Observe(),
            boardChildren = _map.BoardChildren, previewNodes = _map.PreviewCount,
            viewport = BattleMapViewport.Rectangle(GetViewportRect()), mapViewport = BattleMapViewport.Rectangle(_map.GetGlobalRect()),
            previewRect = _map.PreviewRectangle, previewInsideMap = _map.PreviewInsideMap, zoom = _map.Zoom,
        };
    }
}
