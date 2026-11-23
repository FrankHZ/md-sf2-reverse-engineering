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
    private bool _hostCovered;
    private void SuspendInput() { _inputSuspended = true; _boardPointer.Suspend(); SuspendPacing(); ClearNavigation(); _menu?.SuspendPointer(); _inspection?.SuspendPointer(); }
    private void ResumeInput() { _inputSuspended = false; SuspendPacing(); }
    private string _title = "";
    private string _status = "";
    private string _roster = "";
    private string _spells = "";
    private string _help = "";
    private UI.ContextHint _feedback = null!;
    private GameInput _input = null!;
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
    private readonly UI.IdentityIcon _transactionIcon = new() { Name = "TransactionIcon" }, _returnIcon = new() { Name = "ReturnIcon" };
    private Rect2 _hudSubject;
    private BattleSceneView _scene = null!;
    private BattleRetreatView _retreat = null!;
    private BattleAppearanceView _appearance = null!;
    private BattleStoryView _story = null!;
    internal WorldViewportLayout WorldViewport { get; set; } = new();
    internal SessionAudio? Audio { get; set; }
    private readonly MenuPointer _boardPointer = new();
    internal Action<SessionResult, bool>? LeaveBattle { get; set; }
    internal Action<SessionResult>? ObserveResult { get; set; }
    internal Action<int>? PlayUiSound { get; set; }

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        _map = new BattleMapViewport { Name = "MapViewport", WorldViewport = WorldViewport };
        _map.SubjectChanged = rectangle =>
        { _hudSubject = rectangle; _menu?.AvoidSubject(rectangle); _inspection?.ArrangeHover(rectangle); };
        AddChild(_map);
        var ui = UI.PresentationCanvas.Mount(this, "BattleUi", UI.PresentationCanvas.Battle);
        ui.AddChild(_hud); _hud.AddChild(_equipmentIcon); _hud.AddChild(_transactionIcon); _hud.AddChild(_returnIcon); _hud.Choices.Hide();
        _menu = new BattleCommandMenu { Name = "CommandMenu" };
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
        _help = "";
    }

    private string AuxiliaryHint(bool give, bool auxiliary) => !auxiliary ? "" :
        GameText.Get("battle.aux-help", _input.CompactHint(GameAction.Up), _input.CompactHint(GameAction.Down),
            _input.CompactHint(GameAction.Confirm), _input.CompactHint(GameAction.Cancel)) +
        (give ? "\n" + GameText.Get("battle.give-recipient-help", _input.CompactHint(GameAction.Target)) : "");

    internal void RefreshInputHints()
    {
        if (_session is null) return;
        if (_menu.Visible && _decision is { } decision)
            _menu.Present(decision, _result?.Failure, _input, _session.QueryBattleActingStatus());
        if (_inspection.Active) _inspection.RefreshInputHints();
        if (_hud.Visible)
        {
            var stage = _session.Current.Selection?.Stage;
            bool auxiliary = BattleGiveView.Selecting(stage) || BattleDiscardView.Selecting(stage) || stage == BattleSelectionStage.EquipmentChoice;
            _help = AuxiliaryHint(BattleGiveView.Selecting(stage), auxiliary);
            _hud.SetHint(_hud.MoreBelow ? GameText.Get("battle.read-more", _input.CompactHint(GameAction.Confirm)) : _help);
        }
    }

    internal void RefreshSettingsProjection()
    {
        // Inspection refreshes through RefreshInputHints. Playback retains its wait token.
        if (_result is not null && _startupFailure is null && !_inspection.Active) Present();
    }

    public override void _EnterTree()
    {
        GetViewport().SizeChanged += Arrange;
        GetWindow().FocusExited += SuspendInput;
        GetWindow().FocusEntered += ResumeInput;
    }
    public override void _ExitTree()
    {
        SuspendPacing();
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
        _boardPointer.Cancel();
        _menu?.Arrange(size);
        _inspection?.RefreshInputHints();
        _scene?.RefreshGeometry();
        _hud.Arrange();
        if (_feedback?.IsVisibleInTree() == true) _feedback.Arrange(size);
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
        SuspendPacing();
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
                _map.Build(result.Snapshot.Battle, session.Definition);
                _menu.ConfigureAssets(session.ActiveBattleScenes, session.Definition);
                _inspection.ConfigureIcons(_menu.Icons, session.Definition);
                _inspection.ConfigureOptions(session.ActiveBattleScenes);
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
            RetireDecision();
        }
        PublishResult("attach");
        ObserveResult?.Invoke(result);
        if (_startupFailure is not null) return;
        Show();
        Present();
    }

    internal void FailStartup(SessionFailure failure)
    {
        SuspendPacing();
        _startupFailure = failure;
        RetireWindows(); Arrange();
        _title = GameText.Get("battle.start-unavailable");
        _status = $"{failure.Kind}: {failure.Code} ({failure.Field})\n{failure.Message}";
        _feedback.Text.Text = GameText.BattleFailure(failure, stopped: true);
        _feedback.Arrange(GetViewportRect().Size);
        _feedback.Visible = !_hostCovered;
    }

    // Keep only the battlefield beneath the field-owned after/return effects.
    internal void RetireWindows()
    {
        SuspendPacing();
        ClearNavigation(); _boardPointer.Suspend(); RetireInspection();
        RetireDecision();
        _map.ClearDecision();
        _map.Show();
        _menu.Hide(); _hud.Hide(); _scene.Hide(); _story.Hide(); _retreat.Hide();
        _appearance.Hide(); _give.Hide(); _discard.Hide(); _search.Hide(); _feedback.Hide();
    }

    public override void _Process(double delta)
    {
        _boardPointer.ReleaseIfNeutral();
        ServicePacing(delta);
    }

    private void HandleActionCore(GameAction action)
    {
        _boardPointer.Cancel();
        _menu.SuspendPointer(keyboard: true);
        if (!CanRouteBattleInput) return;
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
        if (HandleDecision(action)) return;

        var stage = _session.Current.Selection?.Stage;
        SessionCommand? command = (stage, action) switch
        {
            (BattleSelectionStage.Movement, GameAction.Up) => new Move(ExplorationDirection.North),
            (BattleSelectionStage.Movement, GameAction.Right) => new Move(ExplorationDirection.East),
            (BattleSelectionStage.Movement, GameAction.Down) => new Move(ExplorationDirection.South),
            (BattleSelectionStage.Movement, GameAction.Left) => new Move(ExplorationDirection.West),
            (BattleSelectionStage.Movement or BattleSelectionStage.EquipmentChoice, GameAction.Confirm) => new Confirm(),
            (BattleSelectionStage.EquipmentChoice, GameAction.Cancel) => new Cancel(),
            (BattleSelectionStage.ActionChoice, GameAction.Equipment) => new BeginEquipment(),
            (BattleSelectionStage.ActionChoice, GameAction.Give) => new BeginGive(),
            (BattleSelectionStage.ActionChoice, GameAction.Discard) => new BeginDiscard(),
            _ => null,
        };
        if (command is not null) Send(command);
    }

    private void Send(SessionCommand command)
    {
        if (_inspection.Active) return;
        var before = _session!.Current;
        Submit(command);
        FinishSubmission(before);
    }

    // Session receipts, audio and pacing see every submission. The caller projects
    // after all commands and local focus changes for this input have settled.
    private SessionResult Submit(SessionCommand command)
    {
        var current = _session!.Current;
        _result = _session.Submit(new(current.SessionId, current.Revision, current.Selection?.Actor, command));
        SyncPacing();
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
            else if (command is SelectEquipment or Cancel) PlayUiSound?.Invoke(66);
            else if (command is Confirm) PlayUiSound?.Invoke(67);
        }
        return _result;
    }

    private void FinishSubmission(SessionSnapshot before)
    {
        if (_startupFailure is not null) return;
        if (_result!.Snapshot.Mode == SessionMode.Exploration)
        {
            RetireWindows();
            // Once a scene stages its actors, the field projection owns the world.
            bool retainBoard = before.BattleFieldScene is null &&
                _result.Snapshot.Story.Continuation != ProgramContinuation.DefeatedProgramFinished;
            LeaveBattle?.Invoke(_result, retainBoard);
            return;
        }
        Present();
        SyncPacing();
        CompleteInspectionEntry();
    }

    private void SetHostCovered(bool covered)
    {
        _hostCovered = covered;
        if (_session is null || _result is null || _result.Snapshot.Mode == SessionMode.Exploration) return;
        if (covered)
        {
            _menu.Hide(); _hud.Hide(); _feedback.Hide();
            _inspection.Hide();
            _map.ClearDecision();
        }
        else if (_startupFailure is not null) _feedback.Show();
        else if (_inspection.Active)
        {
            _inspection.Show();
            _inspection.RefreshInputHints();
        }
        else Present(refreshPlayback: false);
    }

    private WaitToken? _movementSoundToken;
    private void Present(bool refreshPlayback = true)
    {
        if (_session!.Current.BattleMovement is { } movement)
        {
            if (_session.Definition.PrivateDefinitions is not null && _session.ActiveBattleScenes?.FieldDeath is null)
            { FailStartup(new(SessionFailureKind.AdapterError, "field-movement-content-required", "battleScenes.fieldDeath", "Movement sprite content is unavailable.")); return; }
            if (_movementSoundToken != movement.Token)
            { _movementSoundToken = movement.Token; Audio?.PlayEffect(movement.SoundOverride ?? 79); }
        }
        var result = _result!;
        var current = _session.Current;
        var choices = _session.QueryBattleChoices();
        // The acting-status query admits only live player decisions, including the
        // real preview and undo steps. Cover and inspection supersede that decision.
        var acting = !_inspection.Active ? _session.QueryBattleActingStatus() : null;
        var decision = PresentDecision(current, choices, _hostCovered ? null : acting);
        var selection = current.Selection;
        bool movementFocus = decision.Visible && decision.Moving;
        var candidate = decision.Targeting ? decision.Candidate : null;
        var projection = BattleSnapshotProjection.Project(result, candidate, _session.Definition.Origin,
            movementFocus ? _session.QueryBattleMovementRange() : null, _session.ActiveBattleScenes);
        _title = projection.Title;
        _status = projection.Status;
        _roster = projection.Roster;
        var equipmentChoices = choices.Equipment;
        _spells = selection is null ? "" : $"Spells ({_input.CompactHint(GameAction.Spell)} to cycle):\n" + string.Join("\n",
            choices.Spells.Select(option =>
                DisplayNames.Spell(option.Spell, option.Label, level: true) + (option.Spell == selection.Spell ? " · selected" : "") +
                (option.Enabled ? "" : $" · unavailable: {option.Reason!.Code}")));
        _items = selection is null ? "" : $"Items ({_input.CompactHint(GameAction.Item)} to cycle):\n" + string.Join("\n",
            choices.Items.Select(option => $"{option.Slot + 1}: {DisplayNames.BattleItem(_session.Current, option.Slot, option.Label)}" +
                (option.Slot == selection.ItemSlot ? " · selected" : "") +
                (option.Enabled || option.Empty ? "" : $" · unavailable: {option.Reason!.Code}")));
        _giveMenu = BattleGiveView.Menu(_session, choices.Give);
        if (refreshPlayback) _give.Present();
        _discardMenu = BattleDiscardView.Menu(_session, choices.Discard);
        if (refreshPlayback) { _discard.Present(); _search.Present(); }
        bool give = BattleGiveView.Selecting(selection?.Stage), discard = BattleDiscardView.Selecting(selection?.Stage);
        bool equipment = selection?.Stage == BattleSelectionStage.EquipmentChoice;
        bool auxiliary = give || discard || equipment;
        _equipment = "";
        if (equipment)
        {
            int index = equipmentChoices.ToList().FindIndex(option => option.Slot == selection!.EquipmentSlot);
            _equipment = GameText.Get("battle.equipment-title") + "\n" + (index < 0 ? GameText.Get("battle.no-equipment") :
                GameText.Get("battle.equipment-row", index + 1, equipmentChoices.Count, DisplayNames.Equipment(equipmentChoices[index]),
                    equipmentChoices[index].Attack, equipmentChoices[index].MinimumRange, equipmentChoices[index].MaximumRange));
            if (index >= 0 && equipmentChoices[index].Category == BattleEquipmentCategory.Ring)
                _equipment += "\n" + DisplayNames.RingStats(equipmentChoices[index]);
            if (choices.EquipmentFailure is { } equipmentFailure)
                _equipment += "\n" + GameText.BattleFailure(equipmentFailure);
            string fallback = GameText.Fallback("battle.", includeKey: true);
            if (fallback.Length > 0) _equipment += "\n" + fallback;
        }
        _help = AuxiliaryHint(give, auxiliary);
        // One readable current choice; directions still visit every typed offer. Reuse story reading
        // before Confirm rather than requiring a mouse wheel to reach long choice text.
        string body = give ? _giveMenu : discard ? _discardMenu : _equipment;
        if (auxiliary && acting is { } actor)
            body = DisplayNames.InspectionName(actor.Unit.Ally, actor.Unit.NameIndex, actor.Unit.Name, actor.Unit.CustomName) + "\n" + body;
        if (auxiliary && result.Failure is not null) body += "\n" + _status;
        if (_hud.Body.Text != body) { _hud.Body.Text = body; _hud.ResetReading(); }
        bool hudVisible = !_hostCovered && acting is not null && auxiliary;
        _hud.Present(hudVisible, hudVisible, false, _hudSubject, _help,
            lifetimeOpen: auxiliary && _hostCovered || hudVisible);
        _feedback.Text.Text = _status;
        _feedback.Visible = !_hostCovered && !_inspection.Active && !_menu.Visible && !hudVisible &&
            (result.Failure is not null || current.BattleResult is not null && current.BattleScene is null &&
                current.BattleRound is null && current.BattleRetreat is null);
        if (_feedback.Visible) _feedback.Arrange(GetViewportRect().Size);
        var selectedEquipment = equipmentChoices.FirstOrDefault(choice => choice.Slot == selection?.EquipmentSlot);
        string? equipmentIdentity = !equipment || selectedEquipment is null ? null : selectedEquipment.ExpectedWord is { } word
            ? UI.IdentityIcons.Item(word) : UI.IdentityIcons.Unarmed;
        _equipmentIcon.Present(_menu.Icons, equipmentIdentity, true); _equipmentIcon.Position = new(24, 24); _equipmentIcon.Size = new(64, 96);
        int? givenWord = give ? choices.Give.Items.FirstOrDefault(item => item.Slot == selection?.ItemSlot)?.Word :
            discard ? choices.Discard.Items.FirstOrDefault(item => item.Slot == selection?.ItemSlot)?.Word : null;
        int? returnedWord = give ? choices.Give.Returns.FirstOrDefault(item => item.Slot == selection?.ReturnSlot)?.Word : null;
        _transactionIcon.Present(_menu.Icons, givenWord is { } given ? UI.IdentityIcons.Item(given) : null,
            selection?.Stage != BattleSelectionStage.GiveExchangeChoice);
        _returnIcon.Present(_menu.Icons, returnedWord is { } returned ? UI.IdentityIcons.Item(returned) : null, true);
        _transactionIcon.Position = new(24, 24); _returnIcon.Position = new(120, 24);
        _hud.SetHint(_hud.MoreBelow ? GameText.Get("battle.read-more", _input.CompactHint(GameAction.Confirm)) : _help);
        if (!_inspection.Active)
        {
            _map.Present(projection, current, movementFocus, candidate);
            _map.PreviewAction(decision.Targeting && decision.SelectedAction is { Enabled: true } offer ? offer : null,
                current, candidate, _input.Settings.ReducedFlash);
        }
        // A close-up owns the art canvas. Field death and notices retain the board.
        _map.Visible = current.BattleScene is not { IsFieldDeath: false, IsAfterTurnNotice: false };
        if (refreshPlayback)
        {
            _story.Present();
            _scene.Present();
            _retreat.Present();
            _appearance.Present();
        }
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
            revision = current?.Revision, observationSequence = current?.ObservationSequence, pacing = ObservePacing(),
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
        if (_startupFailure is { } failure)
            return new { failure = failure.Code, failureKind = failure.Kind.ToString(), revision = current?.Revision };
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
            pacing = ObservePacing(),
            origin = _session?.Definition.Origin, sessionId = current?.SessionId, storyFlags = current?.Story.Flags,
            rules = _session?.Rules.Identity, battleChoices = choices, healingChoices = choices,
            scene = _scene.Observe(), retreat = _retreat.Observe(), appearance = _appearance.Observe(), story = _session is null ? null : _story.Observe(), observationSequence = current?.ObservationSequence,
            inspection = _inspection.Observe(), inspectionRelease = _inspectionRelease, inspectionEntryPending = _inspectionAfterReturn is not null,
            menu = _menu.Observe(_decision), movement = _map.ObserveMovement(),
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
            target = current?.Selection?.Target?.Value, candidate = _decision?.Candidate?.Value,
            itemSlot = current?.Selection?.ItemSlot, itemChoices = _items,
            inventories = current?.Battle.Actors.Select(a => new { actor = a.Actor.Value, items = a.SourceLoadout?.Items }),
            search = _search.Observe(), searchChests = current?.Battle.Definition.Search?.Chests.Select(c => new { x = c.Position.X, y = c.Position.Y, flag = c.Flag, opened = current.Story.Flags.Contains(c.Flag) }), discard = _discard.Observe(), discardMenu = _discardMenu, custody = current?.Battle.ItemCustody,
            give = _give.Observe(), giveMenu = _giveMenu, returnSlot = current?.Selection?.ReturnSlot,
            equipmentIcon = _equipmentIcon.Observe(),
            transactionIcon = _transactionIcon.Observe(), returnIcon = _returnIcon.Observe(),
            equipmentSlot = current?.Selection?.EquipmentSlot, equipmentText = _equipment, equipmentVisible = _hud.IsVisibleInTree() && current?.Selection?.Stage == BattleSelectionStage.EquipmentChoice,
            equipmentActors = current?.Battle.Actors.Select(a => new { actor = a.Actor.Value, baseAttack = a.BaseAttack,
                attack = a.Attack, defense = a.Defense, agility = a.Agility, move = a.Move,
                equippedSlot = BattleEquipment.EquippedSlot(a.Definition, a.SourceLoadout),
                equippedRingSlot = BattleEquipment.EquippedRingSlot(a.Definition, a.SourceLoadout),
                minimumRange = BattleEquipment.Physical(a)?.MinimumRange, maximumRange = BattleEquipment.Physical(a)?.MaximumRange }),
            spell = current?.Selection?.Spell, spellChoices = _spells,
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
