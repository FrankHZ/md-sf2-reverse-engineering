using System.Text.Json;
using Sf2.Remake.GodotAdapter.Observation;
using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.Maps;
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
    internal string? StartupError => _startupFailure?.Code;
    private bool _started;
    private bool _inputSuspended;
    private void SuspendInput() { _inputSuspended = true; ClearNavigation(); _menu?.SuspendPointer(); }
    private void ResumeInput() => _inputSuspended = false;
    private string _title = "";
    private string _status = "";
    private string _roster = "";
    private string _spells = "";
    private string _help = "";
    private UI.ContextHint _feedback = null!;
    private GameInput _input = null!;
    private SpellRef? _spellCandidate;
    private int? _itemCandidate;
    private string _items = "";
    private string _equipment = "";
    private string _giveMenu = "";
    private BattleGiveView _give = null!;
    private string _discardMenu = "";
    private BattleDiscardView _discard = null!;
    private BattleSearchView _search = null!;
    private BattleMapViewport _map = null!;
    private BattleCommandMenu _menu = null!;
    private BattleDefinition? _mapDefinition;
    private BattleSceneDefinition? _mapScenes;
    private readonly UI.StoryDialoguePanel _hud = new() { Name = "AuxiliaryHud" };
    private readonly UI.IdentityIcon _equipmentIcon = new() { Name = "EquipmentIcon" };
    private Rect2 _hudSubject;
    private string _battleHelp = "";
    private BattleSceneView _scene = null!;
    private BattleRetreatView _retreat = null!;
    private BattleAppearanceView _appearance = null!;
    private BattleStoryView _story = null!;
    internal WorldViewportLayout WorldViewport { get; set; } = new();
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
        _map = new BattleMapViewport { Name = "MapViewport", WorldViewport = WorldViewport };
        _map.SubjectChanged = rectangle =>
        { _hudSubject = rectangle; _menu?.GetNodeOrNull<BattleMenuLayout>("Layout")?.AvoidSubject(rectangle); };
        AddChild(_map);
        var ui = UI.PresentationCanvas.Mount(this, "BattleUi", UI.PresentationCanvas.Battle);
        ui.AddChild(_hud); _hud.AddChild(_equipmentIcon); _hud.Choices.Hide();
        _menu = new BattleCommandMenu { Name = "CommandMenu", Submit = Send };
        ui.AddChild(_menu);
        _scene = new BattleSceneView { Name = "BattleScene", PresentationRoot = ui }; AddChild(_scene);
        _give = new BattleGiveView { Name = "BattleGive" }; ui.AddChild(_give);
        _discard = new BattleDiscardView { Name = "BattleDiscard" }; ui.AddChild(_discard);
        _search = new BattleSearchView { Name = "BattleSearch" }; ui.AddChild(_search);
        _retreat = new BattleRetreatView { Name = "BattleRetreat" }; ui.AddChild(_retreat);
        _story = new BattleStoryView { Name = "BattleStory" }; ui.AddChild(_story);
        _appearance = new BattleAppearanceView { Name = "BattleAppearance" }; ui.AddChild(_appearance);
        _feedback = new UI.ContextHint { Name = "BattleFeedback", ZIndex = 100 }; ui.AddChild(_feedback);
        _feedback.Hide();
        _inspection = new BattleInspectionView { Name = "Inspection", RequestSpeed = () => RequestSpeed?.Invoke(),
            RequestQuit = () => RequestQuit?.Invoke() }; ui.AddChild(_inspection);
        Arrange();
    }

    internal void ConfigureInput(GameInput input)
    {
        _input = input;
        _help = $"{input.MovementHint}\n{input.Hint(GameAction.Confirm)}: choose action / commit\n" +
            $"{input.Hint(GameAction.Spell)}: open magic list from diamond\n" +
            $"{input.Hint(GameAction.Item)}: open item list from diamond\n" +
            $"{input.Hint(GameAction.Discard)}: Drop; select slot, confirm, cancel returns\n" +
            $"{input.Hint(GameAction.Give)}: Give; directions select item/recipient, confirm, cancel returns\n" +
            $"{input.Hint(GameAction.Equipment)}: equipment; directions cycle, confirm equips, cancel returns\n" +
            $"{input.Hint(GameAction.Attack)}: physical attack\n{input.Hint(GameAction.Target)}: cycle living targets\n" +
            $"{input.Hint(GameAction.Stay)}: STAY\n{input.Hint(GameAction.Cancel)}: undo Movement, then inspect; menu Back returns one level\n" +
            $"{input.Hint(GameAction.Wait)}: wait at a healing message\n\nAI and rounds advance automatically.";
        _battleHelp = _help;
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
        // Map framing is independent of overlays; the UI canvas owns their draw boundary.
        _map.Position = Vector2.Zero;
        _map.Size = size;
        _menu?.Arrange(size);
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
            _scene.Configure(session, _input, Audio); _retreat.Configure(session, _input.Settings);
            _give.Configure(session, _input.Settings); _discard.Configure(session, _input.Settings); _search.Configure(session, _input.Settings); _appearance.Configure(session, _map, _input.Settings);
            _story.Configure(session, _map, _input);
        }
        if (!ReferenceEquals(_mapDefinition, result.Snapshot.Battle.Definition) ||
            !ReferenceEquals(_mapScenes, session.ActiveBattleScenes))
        {
            try
            {
                _map.Build(result.Snapshot.Battle.Definition, session.Definition);
                _menu.ConfigureAssets(session.ActiveBattleScenes, session.Definition);
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
        RetireWindows(); Arrange();
        _title = GameText.Get("battle.start-unavailable");
        _status = $"{failure.Kind}: {failure.Code} ({failure.Field})\n{failure.Message}";
        _feedback.Text.Text = GameText.BattleFailure(failure, stopped: true);
        _feedback.Arrange(GetViewportRect().Size);
    }

    // Keep only the battlefield beneath the field-owned after/return effects.
    internal void RetireWindows()
    {
        ClearNavigation(); RetireInspection();
        _map.Show();
        _menu.Hide(); _hud.Hide(); _scene.Hide(); _story.Hide(); _retreat.Hide();
        _appearance.Hide(); _give.Hide(); _discard.Hide(); _search.Hide(); _feedback.Hide();
    }

    public override void _Process(double delta)
    {
        if (_inspectionRelease && !_input.GameplayHeld) _inspectionRelease = false;
        if (_inspection.Active)
        {
            if (!_inspection.Refresh(_session)) { InspectionBarrier(); Present(); }
            else { ServiceNavigation(); return; }
        }
        ServiceNavigation();
        if (_startupFailure is not null || !IsVisibleInTree()) return;
        if (_session?.Current.StopReason is SessionStopReason.Unsupported or SessionStopReason.Faulted) return;
        if (_session?.Current.BattleSearch is not null)
        {
            if (!_inputSuspended && _search.Consume(delta) is { } completion) Send(completion);
            return;
        }
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

    internal void HandlePointer(InputEvent input)
    {
        ClearNavigation();
        if (_inspection.Active || _inspectionRelease)
        {
            if (!_inspectionRelease && !_inputSuspended && GetWindow().HasFocus() && CanProcess() && IsVisibleInTree())
                _inspection.HandlePointer(input);
            return;
        }
        if (_startupFailure is not null || _inputSuspended || !GetWindow().HasFocus() || !CanProcess() || !IsVisibleInTree() ||
            _session?.Current.StopReason is SessionStopReason.Unsupported or SessionStopReason.Faulted ||
            _session?.Current.HasBattleControl != true || _session.Current.BattleMovement is not null)
        { _menu.SuspendPointer(); return; }
        if (_menu.HandlePointer(input)) Present();
    }

    private void HandleActionCore(GameAction action)
    {
        _menu.SuspendPointer(keyboard: true);
        if (_startupFailure is not null || _inputSuspended || !GetWindow().HasFocus() || !CanProcess() || !IsVisibleInTree()) return;
        if (_session?.Current.StopReason is SessionStopReason.Unsupported or SessionStopReason.Faulted) return;
        if (InspectionInput(action)) return;
        if (_session?.Current.BattleSearch is not null)
        { if (_search.Acknowledge(action) is { } ack) Send(ack); return; }
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
        if (action == GameAction.Confirm && _hud.IsVisibleInTree() && _hud.ReadMore())
        { _hud.SetHint(_hud.MoreBelow ? GameText.Get("battle.read-more", _input.CompactHint(GameAction.Confirm)) : _help); return; }
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
        if (MovementInspectionEntry(action)) return;
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
        if (_inspection.Active) return;
        var current = _session!.Current;
        if (command is SelectTarget target) _targetCandidate = target.Target;
        _result = _session.Submit(new(current.SessionId, current.Revision, current.Selection?.Actor, command));
        if (_result.Failure is not null) { _navigationRejected = true; ClearNavigation(); }
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
            RetireWindows();
            LeaveBattle?.Invoke(_result);
            return;
        }
        Present();
        CompleteInspectionEntry();
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
        _menu.Present(_session.Current, _session.QueryBattleChoices(), _result!.Failure, _input, _session.QueryBattleActingStatus());
        Arrange();
        var projection = BattleSnapshotProjection.Project(_result, _menu.Visible ? _menu.TargetCandidate : _targetCandidate, _session.Definition.Origin, _session.QueryBattleMovementRange(), _session.ActiveBattleScenes);
        _title = projection.Title;
        _status = projection.Status;
        _feedback.Text.Text = projection.Status;
        _feedback.Arrange(GetViewportRect().Size);
        _feedback.Visible = (_result.Failure is { } failure &&
            (failure.Kind != SessionFailureKind.IllegalCommand || !_menu.Visible)) || (!_menu.Visible && _session.Current.Selection is null &&
            _session.Current.BattleScene is null && _session.Current.BattleRound is null &&
            _session.Current.BattleRetreat is null && _session.Current.BattleGive is null && _session.Current.BattleDiscard is null && _session.Current.BattleSearch is null);
        _roster = projection.Roster;
        var selection = _session.Current.Selection;
        var equipmentChoices = _session.QueryBattleChoices().Equipment;
        var currentActor = selection is null ? null : _session.Current.Battle.GetActor(selection.Actor);
        _equipment = currentActor?.Definition.Equipment is null ? "" :
            GameText.Get("battle.equipment-current", _input.Hint(GameAction.Equipment),
                equipmentChoices.FirstOrDefault(o => o.Equipped) is { } equipped ? DisplayNames.Equipment(equipped, _session.Current) : GameText.Get("prepare.unarmed"),
                currentActor.Attack, BattleEquipment.Physical(currentActor)!.MinimumRange, BattleEquipment.Physical(currentActor)!.MaximumRange);
        _spells = selection is null ? "" : $"Spells ({_input.Hint(GameAction.Spell)} to cycle):\n" + string.Join("\n",
            _session.QueryBattleChoices().Spells.Select(option =>
                DisplayNames.Spell(option.Spell, option.Label, level: true) + (option.Spell == selection.Spell ? " · selected" : "") +
                (option.Enabled ? "" : $" · unavailable: {option.Reason!.Code}")));
        _items = selection is null ? "" : $"Items ({_input.Hint(GameAction.Item)} to cycle):\n" + string.Join("\n",
            _session.QueryBattleChoices().Items.Select(option => $"{option.Slot + 1}: {DisplayNames.BattleItem(_session.Current, option.Slot, option.Label)}" +
                (option.Slot == selection.ItemSlot ? " · selected" : "") +
                (option.Enabled || option.Empty ? "" : $" · unavailable: {option.Reason!.Code}")));
        _giveMenu = BattleGiveView.Menu(_session);
        _give.Present();
        _discardMenu = BattleDiscardView.Menu(_session);
        _discard.Present(); _search.Present();
        bool give = BattleGiveView.Selecting(selection?.Stage), discard = BattleDiscardView.Selecting(selection?.Stage);
        bool equipment = selection?.Stage == BattleSelectionStage.EquipmentChoice;
        bool auxiliary = give || discard || equipment;
        if (equipment)
        {
            int index = equipmentChoices.ToList().FindIndex(option => option.Slot == selection!.EquipmentSlot);
            _equipment = GameText.Get("battle.equipment-title") + "\n" + (index < 0 ? GameText.Get("battle.no-equipment") :
                GameText.Get("battle.equipment-row", index + 1, equipmentChoices.Count, DisplayNames.Equipment(equipmentChoices[index], _session.Current),
                    equipmentChoices[index].Attack, equipmentChoices[index].MinimumRange, equipmentChoices[index].MaximumRange));
            if (_session.QueryBattleChoices().EquipmentFailure is { } equipmentFailure)
                _equipment += "\n" + GameText.BattleFailure(equipmentFailure);
            string fallback = GameText.Fallback("battle.", includeKey: true);
            if (fallback.Length > 0) _equipment += "\n" + fallback;
        }
        _help = auxiliary ? GameText.Get("battle.aux-help", _input.CompactHint(GameAction.Up), _input.CompactHint(GameAction.Down),
            _input.CompactHint(GameAction.Confirm), _input.CompactHint(GameAction.Cancel)) +
            (give ? "\n" + GameText.Get("battle.give-recipient-help", _input.CompactHint(GameAction.Target)) : "") : _battleHelp;
        // One readable current choice; directions still visit every typed offer. Reuse story reading
        // before Confirm rather than requiring a mouse wheel to reach long choice text.
        string body = _status + "\n\n" + (give ? _giveMenu : discard ? _discardMenu : equipment ? _equipment :
            _equipment + "\n" + _spells + "\n" + _items);
        if (_hud.Body.Text != body) { _hud.Body.Text = body; _hud.ResetReading(); }
        bool hudVisible = !_menu.Visible && selection is not null;
        _hud.Present(hudVisible, equipment, false, _hudSubject, _help, lifetimeOpen: hudVisible);
        var selectedEquipment = equipmentChoices.FirstOrDefault(choice => choice.Slot == selection?.EquipmentSlot);
        string? equipmentIdentity = !equipment || selectedEquipment is null ? null : selectedEquipment.Slot == 4
            ? UI.IdentityIcons.Unarmed : UI.IdentityIcons.Item(currentActor!.SourceLoadout!.Items[selectedEquipment.Slot]);
        _equipmentIcon.Present(_menu.Icons, equipmentIdentity, true); _equipmentIcon.Position = new(24, 24); _equipmentIcon.Size = new(64, 96);
        _hud.SetHint(_hud.MoreBelow ? GameText.Get("battle.read-more", _input.CompactHint(GameAction.Confirm)) : _help);
        _map.Present(projection, _session.Current, _menu.Visible ? _menu.TargetCandidate : _targetCandidate);
        // Map children have their own Z priorities; a later sibling's black backdrop
        // cannot occlude them. Full close-ups own rendering, while field notices/deaths keep the map.
        _map.Visible = _session.Current.BattleScene is not { IsFieldDeath: false, IsAfterTurnNotice: false };
        _story.Present();
        _scene.Present();
        _retreat.Present();
        _appearance.Present();
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
            inspection = _inspection.Observe(), inspectionRelease = _inspectionRelease, inspectionEntryPending = _inspectionAfterReturn is not null,
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
            statuses = current?.Battle.Actors.Select(a => new { actor = a.Actor.Value, status = a.Status }),
            itemCustody = current?.Battle.ItemCustody,
            deathCredit = current?.Battle.DeathCredit,
            queueCursor = current?.Battle.Cursor,
            aiMemory = current?.Battle.Actors.Where(a => a.Control == BattleControl.Automatic)
                .Select(a => new { actor = a.Actor.Value, memory = a.AiMemory, lastTarget = a.LastTarget?.Value }).ToArray(),
            map = current?.Battle.Definition.Map.Value, mapWidth = current?.Battle.Definition.Width,
            mapHeight = current?.Battle.Definition.Height, actor = current?.Selection?.Actor.Value,
            terrain = full ? current?.Battle.Definition.Terrain.Select(tile => tile.Surface.ToString()) : null,
            target = current?.Selection?.Target?.Value, candidate = (_menu.Visible ? _menu.TargetCandidate : _targetCandidate)?.Value,
            itemSlot = current?.Selection?.ItemSlot, itemCandidate = _itemCandidate, itemChoices = _items,
            inventories = current?.Battle.Actors.Select(a => new { actor = a.Actor.Value, items = a.SourceLoadout?.Items }),
            search = _search.Observe(), searchChests = current?.Battle.Definition.Search?.Chests.Select(c => new { x = c.Position.X, y = c.Position.Y, flag = c.Flag, opened = current.Story.Flags.Contains(c.Flag) }), discard = _discard.Observe(), discardMenu = _discardMenu, custody = current?.Battle.ItemCustody,
            give = _give.Observe(), giveMenu = _giveMenu, returnSlot = current?.Selection?.ReturnSlot,
            equipmentIcon = _equipmentIcon.Observe(),
            equipmentSlot = current?.Selection?.EquipmentSlot, equipmentText = _equipment, equipmentVisible = _hud.IsVisibleInTree() && current?.Selection?.Stage == BattleSelectionStage.EquipmentChoice,
            equipmentActors = current?.Battle.Actors.Select(a => new { actor = a.Actor.Value, baseAttack = a.BaseAttack,
                attack = a.Attack, equippedSlot = BattleEquipment.EquippedSlot(a.Definition, a.SourceLoadout),
                minimumRange = BattleEquipment.Physical(a)?.MinimumRange, maximumRange = BattleEquipment.Physical(a)?.MaximumRange }),
            spell = current?.Selection?.Spell, spellCandidate = _spellCandidate, spellChoices = _spells,
            selection = current?.Selection,
            stage = current?.Selection?.Stage.ToString(), stopReason = _result?.StopReason.ToString(),
            previewX = current?.Selection?.Preview.Destination.X, previewY = current?.Selection?.Preview.Destination.Y,
            actors = current is null || _startupFailure is not null ? null : _map.ObserveActors(current.Battle.Actors),
            observations = _result?.Observations,
            title = _title, status = _status, roster = _roster, help = _help,
            feedbackVisible = _feedback.IsVisibleInTree(), feedback = _feedback.Text.Text,
            boardChildren = _map.BoardChildren, previewNodes = _map.PreviewCount,
            viewport = BattleMapViewport.Rectangle(GetViewportRect()), mapViewport = BattleMapViewport.Rectangle(_map.GetGlobalRect()),
            hud = BattleMapViewport.Rectangle(_hud.GetGlobalRect()), hudPanel = _hud.Observe(),
            previewRect = _map.PreviewRectangle, previewInsideMap = _map.PreviewInsideMap, zoom = _map.Zoom,
        };
    }
}
