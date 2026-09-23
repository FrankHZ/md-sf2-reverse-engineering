using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Battles;

public sealed partial class BattleSessionView : Control
{
    [Signal]
    public delegate void SessionResultObservedEventHandler(string resultJson);

    private void PublishResult(string boundary)
    {
        if (!HasConnections(SignalName.SessionResultObserved)) return;
        var result = _result!;
        EmitSignal(SignalName.SessionResultObserved, JsonSerializer.Serialize(new
        {
            boundary, sessionId = result.Snapshot.SessionId, revision = result.Snapshot.Revision,
            observationSequence = result.Snapshot.ObservationSequence, mode = result.Snapshot.Mode.ToString(),
            stopReason = result.StopReason.ToString(), failure = result.Failure, observations = result.Observations,
        }));
    }

    private GameSession? _session;
    private SessionResult? _result;
    private SessionFailure? _startupFailure;
    private bool _started;
    private Label _title = null!;
    private Label _status = null!;
    private Label _roster = null!;
    private Label _spells = null!;
    private Label _help = null!;
    private GameInput _input = null!;
    private SpellRef? _spellCandidate;
    private int? _itemCandidate;
    private Label _items = null!;
    private BattleMapViewport _map = null!;
    private ScrollContainer _hud = null!;
    private VBoxContainer _hudContent = null!;
    private readonly List<Label> _hudLabels = [];
    // Last attempted UI candidate, including rejected attempts. Never the gameplay selected target.
    private ActorRef? _targetCandidate;
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
        _hud = new ScrollContainer { Name = "Hud", HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            FocusMode = FocusModeEnum.None };
        AddChild(_hud);
        _hud.GetVScrollBar().FocusMode = FocusModeEnum.None;
        _hudContent = new VBoxContainer { Name = "Content", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _hudContent.AddThemeConstantOverride("separation", 12);
        _hud.AddChild(_hudContent);
        _title = AddLabel("Title");
        _status = AddLabel("Status");
        _spells = AddLabel("Spells");
        _items = AddLabel("Items");
        _roster = AddLabel("Roster");
        _help = AddLabel("Help");
        Arrange();
    }

    internal void ConfigureInput(GameInput input)
    {
        _input = input;
        _help.Text = $"{input.MovementHint}\n{input.Hint(GameAction.Confirm)}: choose action / commit\n" +
            $"{input.Hint(GameAction.Spell)}: select / cycle learned spells and levels; target self\n" +
            $"{input.Hint(GameAction.Item)}: select / cycle carried items; target self\n" +
            $"{input.Hint(GameAction.Attack)}: physical attack\n{input.Hint(GameAction.Target)}: cycle living targets\n" +
            $"{input.Hint(GameAction.Stay)}: STAY\n{input.Hint(GameAction.Cancel)}: cancel all provisional choices\n\nAI and rounds advance automatically.";
    }

    public override void _EnterTree() => GetViewport().SizeChanged += Arrange;
    public override void _ExitTree() => GetViewport().SizeChanged -= Arrange;

    private void Arrange()
    {
        if (_map is null || _hud is null) return;
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
        PublishResult("attach");
        ObserveResult?.Invoke(result);
        if (_startupFailure is not null) return;
        if (first) _map.Build(result.Snapshot.Battle.Definition);
        Show();
        Present();
    }

    internal void FailStartup(SessionFailure failure)
    {
        _startupFailure = failure;
        _title.Text = "BATTLE UNAVAILABLE";
        _status.Text = $"{failure.Kind}: {failure.Code} ({failure.Field})\n{failure.Message}";
    }

    public override void _Process(double delta)
    {
        if (_startupFailure is null && _session?.Current is { HasBattleControl: true, StopReason: SessionStopReason.SimulationWait }) Send(new AdvanceSimulation());
    }

    internal void HandleAction(GameAction action)
    {
        if (_startupFailure is not null || !IsVisibleInTree() || _session?.Current.HasBattleControl != true) return;
        SessionCommand? command = action switch
        {
            GameAction.Up => new Move(ExplorationDirection.North),
            GameAction.Right => new Move(ExplorationDirection.East),
            GameAction.Down => new Move(ExplorationDirection.South),
            GameAction.Left => new Move(ExplorationDirection.West),
            GameAction.Confirm => new Confirm(),
            GameAction.Stay => new ChooseAction(SessionAction.Stay),
            GameAction.Cancel => new Cancel(),
            GameAction.Attack => new ChooseAction(SessionAction.PhysicalAttack),
            _ => null,
        };
        if (command is not null) Send(command);
        else if (action == GameAction.Item && _session.Current.Selection is { } itemSelection)
        {
            var slots = _session.Current.Battle.GetActor(itemSelection.Actor).SourceLoadout?.Items;
            // Even an empty inventory submits a real command, so the player sees its rejection.
            var held = slots is null ? [] : Enumerable.Range(0, slots.Count).Where(slot => (slots[slot] & 127) != 127).ToArray();
            int index = Array.IndexOf(held, _itemCandidate ?? itemSelection.ItemSlot ?? -1);
            _itemCandidate = held.Length == 0 ? 0 : held[(index + 1) % held.Length];
            Send(new SelectItem(_itemCandidate.Value));
            if (_result?.Failure is null) Send(new SelectTarget(itemSelection.Actor));
        }
        else if (action == GameAction.Spell && _session.Current.Selection is { } selection)
        {
            var spells = _session.Current.Battle.GetActor(selection.Actor).Spells;
            if (spells.Count == 0) return;
            int index = spells.ToList().FindIndex(spell => spell == (_spellCandidate ?? selection.Spell));
            _spellCandidate = spells[(index + 1) % spells.Count];
            Send(new SelectSpell(_spellCandidate.Value));
            if (_result?.Failure is null) Send(new SelectTarget(selection.Actor));
        }
        else if (action == GameAction.Target && _session.Current.Selection is { Action: SessionAction.Heal or SessionAction.Item or SessionAction.PhysicalAttack } targeting)
        {
            var targets = _session.Current.Battle.Actors.Where(a => a.Hp > 0 && a.IsAlly == (targeting.Action != SessionAction.PhysicalAttack)).ToArray();
            if (targets.Length == 0) return;
            int selected = Array.FindIndex(targets, a => a.Actor == (_targetCandidate ?? targeting.Target));
            Send(new SelectTarget(targets[(selected + 1) % targets.Length].Actor));
        }
        else return;
    }

    private void Send(SessionCommand command)
    {
        var current = _session!.Current;
        if (command is SelectTarget target) _targetCandidate = target.Target;
        _result = _session.Submit(new(current.SessionId, current.Revision, current.Selection?.Actor, command));
        PublishResult("submit");
        ObserveResult?.Invoke(_result);
        if (_result.Failure is null)
        {
            if (command is SelectSpell or SelectItem or Cancel) PlayUiSound?.Invoke(66);
            else if (command is Confirm) PlayUiSound?.Invoke(67);
        }
        if (_startupFailure is not null) return;
        if (_result.Snapshot.Selection?.Action is not (SessionAction.Heal or SessionAction.Item or SessionAction.PhysicalAttack)) _targetCandidate = null;
        if (command is Cancel || _result.Snapshot.Selection?.Actor != current.Selection?.Actor ||
            _result.Snapshot.Selection?.Action is SessionAction.PhysicalAttack or SessionAction.Stay or SessionAction.Item) _spellCandidate = null;
        if (command is Cancel || _result.Snapshot.Selection?.Actor != current.Selection?.Actor ||
            (_result.Failure is null && _result.Snapshot.Selection?.Action != SessionAction.Item)) _itemCandidate = null;
        if (_result.Snapshot.Mode == SessionMode.Exploration)
        {
            LeaveBattle?.Invoke(_result);
            return;
        }
        Present();
    }

    private void Present()
    {
        var projection = BattleSnapshotProjection.Project(_result!, _targetCandidate, _session!.Definition.Origin);
        _title.Text = projection.Title;
        _status.Text = projection.Status;
        _roster.Text = projection.Roster;
        var selection = _session.Current.Selection;
        _spells.Text = selection is null ? "" : $"Spells ({_input.Hint(GameAction.Spell)} to cycle):\n" + string.Join("\n",
            _session.Current.Battle.GetActor(selection.Actor).Spells.Select(spell =>
                $"{spell.Value.ToUpperInvariant()} {spell.Level}" + (spell == selection.Spell ? " · selected" : "") +
                (spell == _spellCandidate && _result!.Failure is not null ? " · unavailable" : "")));
        var inventory = selection is null ? null : _session.Current.Battle.GetActor(selection.Actor).SourceLoadout?.Items;
        _items.Text = inventory is null ? "" : $"Items ({_input.Hint(GameAction.Item)} to cycle):\n" + string.Join("\n",
            inventory.Select((word, slot) => $"{slot + 1}: " + ((word & 127) == 127 ? "Empty" :
                _session.Current.Battle.Definition.HealingItems.TryGetValue((byte)(word & 127), out var item) ? item.Name : $"Item {word & 127} · unsupported") +
                (slot == selection!.ItemSlot ? " · selected" : "") + (slot == _itemCandidate && _result!.Failure is not null ? " · unavailable" : "")));
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
            origin = _session?.Definition.Origin, sessionId = current?.SessionId, storyFlags = current?.Story.Flags,
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
            terrain = current?.Battle.Definition.Terrain.Select(tile => tile.Surface.ToString()),
            target = current?.Selection?.Target?.Value, candidate = _targetCandidate?.Value,
            itemSlot = current?.Selection?.ItemSlot, itemCandidate = _itemCandidate, itemChoices = _items.Text,
            inventories = current?.Battle.Actors.Select(a => new { actor = a.Actor.Value, items = a.SourceLoadout?.Items }),
            spell = current?.Selection?.Spell, spellCandidate = _spellCandidate, spellChoices = _spells.Text,
            stage = current?.Selection?.Stage.ToString(), stopReason = _result?.StopReason.ToString(),
            previewX = current?.Selection?.Preview.Destination.X, previewY = current?.Selection?.Preview.Destination.Y,
            actors = current is null ? null : _map.ObserveActors(current.Battle.Actors),
            observations = _result?.Observations,
            title = _title.Text, status = _status.Text, roster = _roster.Text, help = _help.Text,
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
