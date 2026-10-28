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
    private bool _inputSuspended;
    private void SuspendInput() => _inputSuspended = true;
    private void ResumeInput() => _inputSuspended = false;
    private Label _title = null!;
    private Label _status = null!;
    private Label _roster = null!;
    private Label _spells = null!;
    private Label _help = null!;
    private GameInput _input = null!;
    private SpellRef? _spellCandidate;
    private int? _itemCandidate;
    private Label _items = null!;
    private Label _equipment = null!;
    private Label _giveMenu = null!;
    private BattleGiveView _give = null!;
    private Label _discardMenu = null!;
    private BattleDiscardView _discard = null!;
    private BattleMapViewport _map = null!;
    private BattleCommandMenu _menu = null!;
    private BattleDefinition? _mapDefinition;
    private BattleSceneDefinition? _mapScenes;
    private ScrollContainer _hud = null!;
    private VBoxContainer _hudContent = null!;
    private readonly List<Label> _hudLabels = [];
    private BattleSceneView _scene = null!;
    private BattleRetreatView _retreat = null!;
    private BattleAppearanceView _appearance = null!;
    private BattleStoryView _story = null!;
    internal SessionAudio? Audio { get; set; }
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
        _giveMenu = AddLabel("Give");
        _discardMenu = AddLabel("Discard");
        _equipment = AddLabel("Equipment");
        _spells = AddLabel("Spells");
        _items = AddLabel("Items");
        _roster = AddLabel("Roster");
        _help = AddLabel("Help");
        _menu = new BattleCommandMenu { Name = "CommandMenu", Submit = Send };
        AddChild(_menu);
        _scene = new BattleSceneView { Name = "BattleScene" }; AddChild(_scene);
        _give = new BattleGiveView { Name = "BattleGive" }; AddChild(_give);
        _discard = new BattleDiscardView { Name = "BattleDiscard" }; AddChild(_discard);
        _retreat = new BattleRetreatView { Name = "BattleRetreat" }; AddChild(_retreat);
        _story = new BattleStoryView { Name = "BattleStory" }; AddChild(_story);
        _appearance = new BattleAppearanceView { Name = "BattleAppearance" }; AddChild(_appearance);
        Arrange();
    }

    internal void ConfigureInput(GameInput input)
    {
        _input = input;
        _help.Text = $"{input.MovementHint}\n{input.Hint(GameAction.Confirm)}: choose action / commit\n" +
            $"{input.Hint(GameAction.Spell)}: open magic list from diamond\n" +
            $"{input.Hint(GameAction.Item)}: open item list from diamond\n" +
            $"{input.Hint(GameAction.Discard)}: Drop; select slot, confirm, cancel returns\n" +
            $"{input.Hint(GameAction.Give)}: Give; directions select item/recipient, confirm, cancel returns\n" +
            $"{input.Hint(GameAction.Equipment)}: equipment; directions cycle, confirm equips, cancel returns\n" +
            $"{input.Hint(GameAction.Attack)}: physical attack\n{input.Hint(GameAction.Target)}: cycle living targets\n" +
            $"{input.Hint(GameAction.Stay)}: STAY\n{input.Hint(GameAction.Cancel)}: cancel all provisional choices\n" +
            $"{input.Hint(GameAction.Wait)}: wait at a healing message\n\nAI and rounds advance automatically.";
    }

    public override void _EnterTree()
    {
        GetViewport().SizeChanged += Arrange;
        GetWindow().FocusExited += SuspendInput;
        GetWindow().FocusEntered += ResumeInput;
    }
    public override void _ExitTree()
    {
        GetWindow().FocusExited -= SuspendInput;
        GetWindow().FocusEntered -= ResumeInput;
        ObservationCapture.Find(this)?.ViewDetached("battle");
        GetViewport().SizeChanged -= Arrange;
    }

    private void Arrange()
    {
        if (_map is null || _hud is null) return;
        var size = GetViewportRect().Size;
        // Map framing is independent of which overlay is open. Map is the first child;
        // command menus, the supplementary HUD and scene presenters draw above it.
        _map.Position = Vector2.Zero;
        _map.Size = size;
        _menu?.Arrange(size);
        const int margin = 16;
        if (size.X >= 800)
        {
            float hudWidth = Mathf.Clamp(size.X * 0.32f, 260, 380);
            _hud.Position = new(size.X - margin - hudWidth, margin);
            _hud.Size = new(hudWidth, Mathf.Max(1, size.Y - 2 * margin));
        }
        else
        {
            float height = Mathf.Max(2, size.Y - 3 * margin);
            _hud.Position = new(margin, margin * 2 + height * 0.6f);
            _hud.Size = new(Mathf.Max(1, size.X - 2 * margin), height * 0.4f);
        }
        float textWidth = Mathf.Max(1, _hud.Size.X - _hud.GetVScrollBar().GetCombinedMinimumSize().X - 4);
        foreach (var label in _hudLabels) label.CustomMaximumSize = new(textWidth, -1);
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
        if (first)
        {
            _scene.Configure(session, _input.Settings, Audio); _retreat.Configure(session, _input.Settings);
            _give.Configure(session, _input.Settings); _discard.Configure(session, _input.Settings); _appearance.Configure(session, _map, _input.Settings);
            _story.Configure(session, _map);
        }
        if (!ReferenceEquals(_mapDefinition, result.Snapshot.Battle.Definition) ||
            !ReferenceEquals(_mapScenes, session.ActiveBattleScenes))
        {
            try
            {
                _map.Build(result.Snapshot.Battle.Definition, session.Definition);
                _menu.ConfigureAssets(session.ActiveBattleScenes);
                _menu.Reset();
            }
            catch (InvalidOperationException error)
            {
                FailStartup(new(SessionFailureKind.AdapterError, error.Message, "battlefield", "Selected battlefield resources are unavailable."));
                Show();
                // During BattleLoad the field presenter owns the pending token.
                // Its existing error path must retain that wait instead of treating
                // a failed hidden mount as a completed presentation.
                if (GetParent() is Sf2.Remake.GodotAdapter.Exploration.ExplorationSessionView) throw;
                return;
            }
            _mapDefinition = result.Snapshot.Battle.Definition;
            _mapScenes = session.ActiveBattleScenes;
            _targetCandidate = null; _spellCandidate = null; _itemCandidate = null;
        }
        PublishResult("attach");
        ObserveResult?.Invoke(result);
        if (_startupFailure is not null) return;
        Show();
        Present();
    }

    internal void FailStartup(SessionFailure failure)
    {
        _startupFailure = failure;
        _menu.Hide(); _hud.Show(); Arrange();
        _title.Text = "BATTLE UNAVAILABLE";
        _status.Text = $"{failure.Kind}: {failure.Code} ({failure.Field})\n{failure.Message}";
    }

    public override void _Process(double delta)
    {
        if (_startupFailure is not null || !IsVisibleInTree()) return;
        if (_session?.Current.BattleDiscard is not null)
        {
            if (!_inputSuspended && _discard.Consume(delta) is { } completion) Send(completion);
            return;
        }
        if (_session?.Current.BattleGive is not null)
        {
            if (!_inputSuspended && _give.Consume(delta) is { } completion) Send(completion);
            return;
        }
        if (_session?.Current.BattleRound is { Program: true })
        {
            if (_session.Current.StopReason is not (SessionStopReason.Unsupported or SessionStopReason.Faulted) && _story.Consume() is { } completion) Send(completion);
            return;
        }
        if (_session?.Current.BattleRound is not null)
        {
            if (_appearance.Consume(delta) is { } completion) Send(completion);
            return;
        }
        if (_session?.Current.BattleRetreat is not null)
        {
            if (_retreat.Consume(delta) is { } completion) Send(completion);
            return;
        }
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
        if (_startupFailure is not null || _inputSuspended || !IsVisibleInTree()) return;
        if (_session?.Current.BattleDiscard is not null)
        { if (_discard.Acknowledge(action) is { } ack) Send(ack); return; }
        if (_session?.Current.BattleGive is not null)
        { if (_give.Acknowledge(action) is { } ack) Send(ack); return; }
        if (_session?.Current.BattleRound is { Program: true })
        { if (_story.Acknowledge(action) is { } acknowledgement) Send(acknowledgement); return; }
        if (_session?.Current.BattleRound is not null) return;
        if (_session?.Current.BattleRetreat is not null)
        { if (_retreat.Acknowledge(action) is { } ack) Send(ack); return; }
        if (_session?.Current.BattleMovement is not null) return;
        if (_session?.Current.BattleScene is not null)
        { if (_scene.Acknowledge(action) is { } ack) Send(ack); return; }
        if (_session?.Current.HasBattleControl != true) return;
        if (BattleDiscardView.Selecting(_session.Current.Selection?.Stage))
        { if (BattleDiscardView.Select(_session, action) is { } discardCommand) Send(discardCommand); return; }
        if (BattleGiveView.Selecting(_session.Current.Selection?.Stage))
        { if (BattleGiveView.Select(_session, action) is { } giveCommand) Send(giveCommand); return; }
        if (_session.Current.Selection is { Stage: BattleSelectionStage.EquipmentChoice } equipmentSelection &&
            action is GameAction.Up or GameAction.Right or GameAction.Down or GameAction.Left or GameAction.Equipment)
        {
            var options = _session.QueryBattleChoices().Equipment;
            if (options.Count == 0) return;
            int index = options.ToList().FindIndex(option => option.Slot == equipmentSelection.EquipmentSlot);
            int step = action is GameAction.Up or GameAction.Left ? -1 : 1;
            Send(new SelectEquipment(options[(index + step + options.Count) % options.Count].Slot));
            return;
        }
        if (_menu.Handle(action))
        {
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
            GameAction.Stay => new ChooseAction(SessionAction.Stay),
            GameAction.Cancel => new Cancel(),
            GameAction.Attack => new ChooseAction(SessionAction.PhysicalAttack),
            GameAction.Equipment => new BeginEquipment(),
            GameAction.Give => new BeginGive(),
            GameAction.Discard => new BeginDiscard(),
            _ => null,
        };
        if (command is not null) Send(command);
        else if (action == GameAction.Item && _session.Current.Selection is { } itemSelection)
        {
            var held = _session.QueryBattleChoices().Items.Where(option => !option.Empty).ToArray();
            int index = Array.FindIndex(held, option => option.Slot == (_itemCandidate ?? itemSelection.ItemSlot ?? -1));
            _itemCandidate = held.Length == 0 ? 0 : held[(index + 1) % held.Length].Slot;
            Send(new SelectItem(_itemCandidate.Value));
            TryInitialTarget(itemSelection.Actor);
        }
        else if (action == GameAction.Spell && _session.Current.Selection is { } selection)
        {
            var spells = _session.QueryBattleChoices().Spells.Select(option => option.Spell).ToArray();
            if (spells.Length == 0) return;
            int index = spells.ToList().FindIndex(spell => spell == (_spellCandidate ?? selection.Spell));
            _spellCandidate = spells[(index + 1) % spells.Length];
            Send(new SelectSpell(_spellCandidate.Value));
            TryInitialTarget(selection.Actor);
        }
        else if (action == GameAction.Target && _session.Current.Selection?.ActionReference is { } selectedAction)
        {
            var targets = _session.QueryBattleChoices().Actions.Single(option => option.Action == selectedAction)
                .Targets.Select(target => target.Actor).ToArray();
            if (targets.Length == 0) return;
            int selected = Array.FindIndex(targets, a => a == (_targetCandidate ?? _session.Current.Selection.Target));
            Send(new SelectTarget(targets[(selected + 1) % targets.Length]));
        }
        else return;
    }

    private void TryInitialTarget(ActorRef actor)
    {
        if (_result?.Failure is not null || _session!.Current.Selection?.ActionReference is not { } action) return;
        var choice = _session.QueryBattleChoices().Actions.Single(option => option.Action == action);
        if (choice.Targets.Any(target => target.Actor == actor && target.Enabled)) Send(new SelectTarget(actor));
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
            // The modern action-choice lifetime maps to the source outer menu's
            // open/close cue, before the generic selection/confirmation bindings.
            bool wasActionChoice = current.Selection?.Stage == BattleSelectionStage.ActionChoice;
            bool isActionChoice = _result.Snapshot.Selection?.Stage == BattleSelectionStage.ActionChoice;
            if (wasActionChoice != isActionChoice) PlayUiSound?.Invoke(65);
            else if (command is SelectSpell or SelectItem or SelectEquipment or Cancel) PlayUiSound?.Invoke(66);
            else if (command is Confirm) PlayUiSound?.Invoke(67);
        }
        if (_startupFailure is not null) return;
        if (_result.Snapshot.Selection?.Action is not (SessionAction.Heal or SessionAction.Retreat or SessionAction.Item or SessionAction.PhysicalAttack)) _targetCandidate = null;
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

    private WaitToken? _movementSoundToken;
    private void Present()
    {
        if (_session!.Current.BattleMovement is { } movement)
        {
            if (_session.Definition.PrivateDefinitions is not null && _session.ActiveBattleScenes?.FieldDeath is null)
            { FailStartup(new(SessionFailureKind.AdapterError, "field-movement-content-required", "battleScenes.fieldDeath", "Movement sprite content is unavailable.")); return; }
            if (_movementSoundToken != movement.Token)
            { _movementSoundToken = movement.Token; Audio?.PlayEffect(79); }
        }
        _menu.Present(_session.Current, _session.QueryBattleChoices(), _result!.Failure, _input);
        _hud.Visible = !_menu.Visible && _session.Current.Selection is not null;
        Arrange();
        var projection = BattleSnapshotProjection.Project(_result, _menu.Visible ? _menu.TargetCandidate : _targetCandidate, _session.Definition.Origin);
        _title.Text = projection.Title;
        _status.Text = projection.Status;
        _roster.Text = projection.Roster;
        var selection = _session.Current.Selection;
        var equipmentChoices = _session.QueryBattleChoices().Equipment;
        var currentActor = selection is null ? null : _session.Current.Battle.GetActor(selection.Actor);
        _equipment.Text = currentActor?.Definition.Equipment is null ? "" :
            $"Equipment ({_input.Hint(GameAction.Equipment)}): {equipmentChoices.FirstOrDefault(o => o.Equipped)?.Label ?? "Unarmed"} · ATT {currentActor.Attack} · range {BattleEquipment.Physical(currentActor)!.MinimumRange}–{BattleEquipment.Physical(currentActor)!.MaximumRange}\n" +
            (selection?.Stage != BattleSelectionStage.EquipmentChoice ? "" : string.Join("\n", equipmentChoices.Select(option =>
                $"{(option.Slot == selection.EquipmentSlot ? "> " : "  ")}{(option.Slot == 4 ? "" : $"slot {option.Slot + 1}: ")}{option.Label} · ATT {option.Attack} · range {option.MinimumRange}–{option.MaximumRange}")) +
                $"\n{_input.Hint(GameAction.Confirm)} equips · {_input.Hint(GameAction.Cancel)} restores selection");
        _spells.Text = selection is null ? "" : $"Spells ({_input.Hint(GameAction.Spell)} to cycle):\n" + string.Join("\n",
            _session.QueryBattleChoices().Spells.Select(option =>
                option.Label + (option.Spell == selection.Spell ? " · selected" : "") +
                (option.Enabled ? "" : $" · unavailable: {option.Reason!.Code}")));
        _items.Text = selection is null ? "" : $"Items ({_input.Hint(GameAction.Item)} to cycle):\n" + string.Join("\n",
            _session.QueryBattleChoices().Items.Select(option => $"{option.Slot + 1}: {option.Label}" +
                (option.Slot == selection.ItemSlot ? " · selected" : "") +
                (option.Enabled || option.Empty ? "" : $" · unavailable: {option.Reason!.Code}")));
        _giveMenu.Text = BattleGiveView.Menu(_session);
        _give.Present();
        _discardMenu.Text = BattleDiscardView.Menu(_session);
        _discard.Present();
        _map.Present(projection, _session.Current);
        _story.Present();
        _scene.Present();
        _retreat.Present();
        _appearance.Present();
        if (_scene.Error is { } error) FailStartup(new(SessionFailureKind.AdapterError, error, "battleScene", "Battle scene playback is unavailable."));
    }

    private Label AddLabel(string name)
    {
        var label = new Label { Name = name, AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
        _hudContent.AddChild(label);
        _hudLabels.Add(label);
        return label;
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

    // Replay only a completion already emitted by the actual retreat consumer.
    // Native acceptance uses this to verify duplicate delivery at the session boundary.
    public void RepeatRetreatDeliveryObservation()
    { if (_retreat.LastDelivery is { } completion) Send(completion); }

    // Read-only native observation of the same semantic result and actual projected nodes.
    public string ReadSceneObservationJson()
    {
        var current = _session?.Current;
        return JsonSerializer.Serialize(new
        {
            revision = current?.Revision, observationSequence = current?.ObservationSequence,
            mainSeed = current?.Battle.MainSeed, thinkingSeed = current?.Battle.ThinkingSeed,
            cursor = current?.Battle.Cursor, actor = current?.Selection?.Actor.Value, round = current?.Battle.Round,
            failure = (_result?.Failure ?? _startupFailure)?.Code, scene = _scene.Observe(), retreat = _retreat.Observe(), appearance = _appearance.Observe(), story = _session is null ? null : _story.Observe(),
            hasBattleControl = current?.HasBattleControl, movement = _map.ObserveMovement(), fieldActors = current is null || _startupFailure is not null ? null : _map.ObserveActors(current.Battle.Actors),
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
            scene = _scene.Observe(), retreat = _retreat.Observe(), appearance = _appearance.Observe(), story = _session is null ? null : _story.Observe(), observationSequence = current?.ObservationSequence,
            menu = _menu.Observe(), movement = _map.ObserveMovement(),
            visualAliases = current?.Battle.Visuals?.Aliases.Select(p => new { actor = p.Key.Value, slot = p.Value }),
            physicalVisuals = current?.Battle.Visuals?.Visuals.Values, cameraFocus = _map.CameraFocus,
            initializationPolicy = current?.Battle.StartPolicy,
            regionFlags = current?.Battle.Regions?.Flags, regionsTested = current?.Battle.Regions?.Tested,
            turnOrder = current?.Battle.TurnOrder.Select(entry => new { actor = entry.Actor?.Value, score = entry.AlteredAgility }),
            failure = (_result?.Failure ?? _startupFailure)?.Code,
            failureKind = (_result?.Failure ?? _startupFailure)?.Kind.ToString(),
            round = current?.Battle.Round, revision = current?.Revision,
            mainSeed = current?.Battle.MainSeed, thinkingSeed = current?.Battle.ThinkingSeed, gold = current?.Battle.Gold,
            itemCustody = current?.Battle.ItemCustody,
            deathCredit = current?.Battle.DeathCredit,
            queueCursor = current?.Battle.Cursor,
            aiMemory = current?.Battle.Actors.Where(a => a.Control == BattleControl.Automatic)
                .Select(a => new { actor = a.Actor.Value, memory = a.AiMemory, lastTarget = a.LastTarget?.Value }).ToArray(),
            map = current?.Battle.Definition.Map.Value, mapWidth = current?.Battle.Definition.Width,
            mapHeight = current?.Battle.Definition.Height, actor = current?.Selection?.Actor.Value,
            terrain = full ? current?.Battle.Definition.Terrain.Select(tile => tile.Surface.ToString()) : null,
            target = current?.Selection?.Target?.Value, candidate = (_menu.Visible ? _menu.TargetCandidate : _targetCandidate)?.Value,
            itemSlot = current?.Selection?.ItemSlot, itemCandidate = _itemCandidate, itemChoices = _items.Text,
            inventories = current?.Battle.Actors.Select(a => new { actor = a.Actor.Value, items = a.SourceLoadout?.Items }),
            discard = _discard.Observe(), discardMenu = _discardMenu.Text, custody = current?.Battle.ItemCustody,
            give = _give.Observe(), giveMenu = _giveMenu.Text, returnSlot = current?.Selection?.ReturnSlot,
            equipmentSlot = current?.Selection?.EquipmentSlot, equipmentText = _equipment.Text, equipmentVisible = _equipment.IsVisibleInTree(),
            equipmentActors = current?.Battle.Actors.Select(a => new { actor = a.Actor.Value, baseAttack = a.BaseAttack,
                attack = a.Attack, equippedSlot = BattleEquipment.EquippedSlot(a.Definition, a.SourceLoadout),
                minimumRange = BattleEquipment.Physical(a)?.MinimumRange, maximumRange = BattleEquipment.Physical(a)?.MaximumRange }),
            spell = current?.Selection?.Spell, spellCandidate = _spellCandidate, spellChoices = _spells.Text,
            stage = current?.Selection?.Stage.ToString(), stopReason = _result?.StopReason.ToString(),
            previewX = current?.Selection?.Preview.Destination.X, previewY = current?.Selection?.Preview.Destination.Y,
            actors = current is null || _startupFailure is not null ? null : _map.ObserveActors(current.Battle.Actors),
            observations = _result?.Observations,
            title = _title.Text, status = _status.Text, roster = _roster.Text, help = _help.Text,
            boardChildren = _map.BoardChildren, previewNodes = _map.PreviewCount,
            viewport = BattleMapViewport.Rectangle(GetViewportRect()), mapViewport = BattleMapViewport.Rectangle(_map.GetGlobalRect()),
            hud = BattleMapViewport.Rectangle(_hud.GetGlobalRect()), hudClipsContents = _hud.ClipContents,
            hudContentHeight = _hudContent.Size.Y,
            hudLabels = _hudLabels.Select(label => new { name = label.Name.ToString(), rect = BattleMapViewport.Rectangle(label.GetGlobalRect()) }),
            hudScroll = _hud.ScrollVertical, hudScrollMaximum = _hud.GetVScrollBar().MaxValue, hudScrollPage = _hud.GetVScrollBar().Page,
            previewRect = _map.PreviewRectangle, previewInsideMap = _map.PreviewInsideMap, zoom = _map.Zoom,
        };
    }
}
