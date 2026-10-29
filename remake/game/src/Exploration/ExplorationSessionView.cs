using System.Text.Json;
using Sf2.Remake.GodotAdapter.Observation;
using Godot;
using Sf2.Remake.GodotAdapter.Maps;
using Sf2.Remake.GodotAdapter.Audio;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Exploration;

public sealed partial class ExplorationSessionView : Control
{
    [Signal]
    public delegate void SessionResultObservedEventHandler(string resultJson);

    // User signal connections identify the external capture itself, without a managed event bridge.
    private static readonly StringName ResourceDrawSignal = "ResourceDrawObserved";
    private static readonly StringName FactsSignal = "SessionFactsObserved";
    internal WorldViewportLayout WorldViewport { get; set; } = new();
    private bool _legacyCamera;
    public ExplorationSessionView()
    {
        AddUserSignal(ResourceDrawSignal);
        AddUserSignal(FactsSignal, [new Godot.Collections.Dictionary { ["name"] = "facts", ["type"] = (int)Variant.Type.Dictionary }]);
    }

    private void PublishResult(string boundary, CompletePresentation? completion = null)
    {
        if (!HasConnections(SignalName.SessionResultObserved) && !HasConnections(FactsSignal)) return;
        var result = _result!;
        var facts = new
        {
            boundary, sessionId = result.Snapshot.SessionId, revision = result.Snapshot.Revision,
            observationSequence = result.Snapshot.ObservationSequence, mode = result.Snapshot.Mode.ToString(),
            stopReason = result.StopReason.ToString(), failure = result.Failure,
            observations = completion is null ? result.Observations : Array.Empty<SessionObservation>(),
            programControlReads = completion is null ? result.ProgramControlReads ?? [] : [],
            completion = completion is null ? null : new
            {
                token = completion.Wait.Value, kind = completion.Kind.ToString(), processFrame = Engine.GetProcessFrames(),
            },
        };
        if (HasConnections(FactsSignal) && ObservationCapture.Find(this)?.IsAccepting() != false) EmitSignal(FactsSignal, ObservationCapture.ToDictionary(facts));
        if (HasConnections(SignalName.SessionResultObserved))
            EmitSignal(SignalName.SessionResultObserved, JsonSerializer.Serialize(facts));
    }

    private GameSession? _session;
    private SessionResult? _result;
    private Action<SessionResult>? _enterBattle;
    private bool _handedOff;
    private bool _battleMounted;
    private Action? _releaseBattle;
    private ExplorationDialogueView _dialogueView = null!;
    private Label _dialogue => _dialogueView.Panel.Body;
    private Label _help => _dialogueView.ActiveHint;
    private UI.StoryChoiceRow _choice => _dialogueView.Panel.Choices;
    private Label _choiceYes => _choice.Yes;
    private Label _choiceNo => _choice.No;
    private bool _choiceRequiresRelease;
    private Label _failureLabel = null!;
    private GameInput _input = null!;
    private readonly ExplorationMovementHold _movementHold = new();
    private double _revealed { get => _dialogueView.Revealed; set => _dialogueView.Revealed = value; }
    private WaitToken? _textDeliveryToken;
    private ulong _textDeliveryFrame;
    private bool _speechSoundToggle;
    private double _tickTime;
    private bool _resumeAutomatic;
    private bool _waitingAtInput;
    private ulong _nextWaitMicros;
    private ulong _lastWaitFrame;
    private ExplorationState? _lastWorld;
    private ExplorationPresentation? _presentation;
    private sealed record AuthoredEntityDraw(int Slot, int? Sprite, float X, float Y, float Radius,
        ushort Size, bool Visible, long Request, ulong Frame);
    private readonly Dictionary<int, AuthoredEntityDraw> _authoredDraws = [];
    private SessionAudio? _audio;
    private SessionFailure? PresentationFailure => _result?.Failure ??
        (_presentation?.Error is { } error ? new(SessionFailureKind.AdapterError,
            "presentation-unavailable", "presentation", error) : null);

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        GetWindow().ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
        _dialogueView = new ExplorationDialogueView { Name = "StoryDialogue" };
        AddChild(_dialogueView);
        // Keep a stopped presentation readable even when the world is held black.
        var failureLayer = new CanvasLayer { Layer = 101 };
        var failurePanel = new PanelContainer { Name = "FailurePanel", Position = new(240, 32),
            Size = new(1440, 140), Theme = UI.StoryTheme.Create(), MouseFilter = MouseFilterEnum.Ignore };
        _failureLabel = new Label { Name = "TransitionFailure", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        AddChild(failureLayer); failureLayer.AddChild(failurePanel); failurePanel.AddChild(_failureLabel);
        failurePanel.Hide();
        GetViewport().SizeChanged += Present;
        GetWindow().FocusExited += SuspendClock;
        VisibilityChanged += SuspendClock;
    }
    public override void _ExitTree()
    {
        ObservationCapture.Find(this)?.ViewDetached("exploration");
        _movementHold.Clear(_input);
        CancelFieldWait();
        GetViewport().SizeChanged -= Present;
        GetWindow().FocusExited -= SuspendClock;
        VisibilityChanged -= SuspendClock;
        _presentation?.Dispose();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPaused || what == NotificationUnpaused) SuspendClock();
    }

    private void SuspendClock()
    {
        if (_input is not null) _movementHold.Clear(_input);
        SuspendEntrySkip();
        CancelFieldWait();
        _tickTime = 0;
        _resumeAutomatic = true;
        _choiceRequiresRelease = true;
    }

    private void CancelFieldWait()
    {
        _waitingAtInput = false; _nextWaitMicros = 0;
        _input?.DisarmWait();
    }

    private void SubmitFieldWait()
    {
        _lastWaitFrame = Engine.GetProcessFrames();
        _nextWaitMicros = Time.GetTicksUsec() + 16667;
        Send(_session!.Current.CanWaitForChoice ? new PollChoice(_session.Current.Story.Wait!.Token) :
            _session.Current.CanWaitForText ? new WaitForText(_session.Current.Story.Wait!.Token) : new WaitAtInput());
    }

    private bool TextRevealed => _dialogue.VisibleCharacters < 0 || _dialogue.VisibleCharacters >= _dialogue.GetTotalCharacterCount();
    private bool CanSubmitWait => _session is { } session &&
        (session.Current.CanWaitAtInput || session.Current.CanWaitForText && TextRevealed || session.Current.CanWaitForChoice && !_choiceRequiresRelease);

    internal void Begin(GameSession session, SessionResult result, GameInput input, SessionAudio audio, Action<SessionResult> enterBattle,
        Action<SessionResult> prepareBattle, Action? releaseBattle = null)
    {
        _session = session; _result = result; _input = input; _enterBattle = enterBattle;
        _movementHold.Clear(input);
        if (session.Current.Story.Display?.Visibility == Sf2.Remake.Application.Content.Scenarios.FullFadeVisibility.Black)
            Modulate = Colors.Black;
        PublishResult("attach");
        _audio = audio; audio.Observe(result);
        _releaseBattle = releaseBattle;
        _battleMounted = releaseBattle is not null;
        _presentation = new(this, session.Definition.Exploration!, input.Settings.ReducedFlash, () =>
        {
            prepareBattle(_result!);
            _battleMounted = true;
            _lastWorld = null;
            _dialogueView.Hide();
            QueueRedraw();
        }, audio, WorldViewport);
        _presentation.DialoguePanel = _dialogueView.Panel;
        TextureFilter = TextureFilterEnum.Nearest;
        Present();
    }

    public override void _Process(double delta)
    {
        if (_session is null) return;
        if (!IsVisibleInTree() || !GetWindow().HasFocus() || !CanProcess() || PresentationFailure is not null)
        { SuspendClock(); return; }
        if (FinishEntryHandoff()) return;
        if (_handedOff || _session.Current.StopReason is SessionStopReason.Unsupported or SessionStopReason.Faulted) return;
        if (_resumeAutomatic) { delta = 0; _resumeAutomatic = false; }
        if (!_input.GameplayHeld) { _choiceRequiresRelease = false; _skipRequiresRelease = false; }
        if (_session.Current.Story.Wait is ChoiceWait { Work.Phase: ChoicePhase.Entry } entry)
        { Send(new ChoiceHeldInput(entry.Token, _input.GameplayHeld)); return; }
        if (_session.Current.Story.Wait is ChoiceWait { Work.Phase: ChoicePhase.Release } release)
        {
            _tickTime = 0;
            if (!_input.GameplayHeld) Send(new ChoiceHeldInput(release.Token, false));
            return;
        }
        if (_session.Current.Story.EntrySkip is null && _dialogue.VisibleCharacters >= 0)
        {
            int before = _dialogue.VisibleCharacters;
            _revealed = Math.Min(_dialogue.GetTotalCharacterCount(), _revealed + delta * _input.Settings.CharactersPerSecond);
            _dialogue.VisibleCharacters = (int)_revealed;
            SpeakRevealedCharacters(before, _dialogue.VisibleCharacters);
        }
        _dialogueView.RefreshHint(_session, _input, _battleMounted, TextRevealed);
        if (_session.Current.Exploration is { Definition.Caravan: not null } authored &&
            _session.Definition.Exploration!.Visuals is null)
            foreach (var entity in authored.AllEntities.Where(actor => actor.WaitingForSprite && actor.SpriteReady != actor.SpriteRequest))
                if (_authoredDraws.TryGetValue(entity.Slot, out var draw) && draw.Request == entity.SpriteRequest &&
                    draw.Sprite == entity.Sprite && draw.Size == entity.RenderSize)
                    Send(new EntitySpriteReady(entity.Slot, entity.SpriteRequest));
        if (_presentation is { } presentation)
        {
            var beforePresentation = _session.Current.Story.Wait?.Token;
            foreach (var completion in presentation.Update(delta, _session.Current))
            {
                Send(completion);
                if (_handedOff || _battleHandoffPending || _result?.Failure is not null) return;
            }
            QueueRedraw();
            if (presentation.Error is not null) { SuspendEntrySkip(); Present(); return; }
            if (beforePresentation != _session.Current.Story.Wait?.Token) { _tickTime = 0; return; }
        }
        if (_session.Current.Story.EntrySkip is not null)
        {
            Send(new AdvanceBattleEntrySkip());
            _tickTime = 0;
            return;
        }
        WaitToken? deliveryToken = _session.Current.Story.Wait switch
        {
            W1TextWait { Revealed: false } delivery => delivery.Token,
            FieldTextWait { Revealed: false } delivery => delivery.Token,
            _ => null,
        };
        if (deliveryToken is { } deliveredToken && TextRevealed && !_dialogueView.Panel.MoreBelow)
        {
            _tickTime = 0;
            // Leave the completed projection mounted for a frame before a tail can close it.
            // This is delivery latency only, never a simulation opportunity.
            if (_textDeliveryToken != deliveredToken)
            { _textDeliveryToken = deliveredToken; _textDeliveryFrame = Engine.GetProcessFrames(); }
            else if (Engine.GetProcessFrames() > _textDeliveryFrame)
                Send(new CompleteTextReveal(deliveredToken));
            return;
        }
        if (_session.Current.CanWaitAtInput || _session.Current.CanWaitForText || _session.Current.CanWaitForChoice || _session.Current.Story.Wait is W1TextWait or FieldTextWait { LogicalDone: true })
        {
            if (_session.Current.CanWaitAtInput && _movementHold.Repeat(_input, _session.Current, Time.GetTicksUsec()) is { } direction)
            { HandleAction(direction, repeating: true); return; }
            // Idle field time is explicitly paused. Presentation time never becomes field debt.
            _tickTime = 0;
            if (!CanSubmitWait || !GetWindow().HasFocus() || !IsVisibleInTree() || !_input.WaitHeld || PresentationFailure is not null)
                CancelFieldWait();
            else if (_waitingAtInput && Engine.GetProcessFrames() != _lastWaitFrame && Time.GetTicksUsec() >= _nextWaitMicros)
                SubmitFieldWait(); // No batch or catch-up for missed repeat deadlines.
            return;
        }
        if (_waitingAtInput) CancelFieldWait();
        if (NeedsTicks(_session.Current))
        {
            const double tickDuration = 1.0 / 60;
            _tickTime += delta;
            int ticks = (int)Math.Min(600, Math.Floor(_tickTime / tickDuration));
            if (ticks > 0)
            {
                long before = _session.Current.Story.SimulationTick;
                var token = _session.Current.Story.Wait?.Token;
                Send(new AdvanceSimulation(_session.Current.Story.Wait?.Token, ticks));
                long executed = _session.Current.Story.SimulationTick - before;
                // The engine may yield before consuming the batch. Retain its unused time
                // for automatic continuation, but never carry paused input time into a new wait.
                _tickTime = Math.Max(0, _tickTime - executed * tickDuration);
                if (_handedOff || token != _session.Current.Story.Wait?.Token || _session.Current.CanWaitAtInput || _session.Current.CanWaitForText || _session.Current.CanWaitForChoice || !NeedsTicks(_session.Current)) _tickTime = 0;
            }
        }
        else _tickTime = 0;
    }

    private static bool NeedsTicks(SessionSnapshot current) =>
        current.Story.Wait is MusicWait music ? !music.LogicalDone :
        current.Story.TextSettings is not null && current.Story.Wait is PresentationWait
            { Cue.Kind: PresentationCueKind.Sound or PresentationCueKind.PreviousMusic or PresentationCueKind.SoundFade } ? false :
        current.Story.Wait is ChoiceWait { Work: { } choice } ? choice.Automatic :
        current.Story.Wait is NodWait nod ? !nod.LogicalDone :
        current.Story.Wait is FieldTextWait text ? !text.LogicalDone :
        current.Story.Wait is FullFadeWait fade ? !fade.LogicalDone :
            current.StopReason == SessionStopReason.SimulationWait ||
            current.Exploration?.AllEntities.Any(entity => entity.Busy || entity.Follower is not null) == true;

    internal void HandleAction(GameAction action, bool repeating = false)
    {
        if (!GetWindow().HasFocus() || !CanProcess()) { SuspendClock(); return; }
        bool direction = action is GameAction.Up or GameAction.Right or GameAction.Down or GameAction.Left;
        if (!direction) _movementHold.Clear(_input);
        if (action != GameAction.Wait) CancelFieldWait();
        if (_handedOff || !IsVisibleInTree() || _session is null || SessionPresentationOwner.IsBattle(_session.Current) || PresentationFailure is not null) return;
        var current = _session.Current;
        if (direction)
        {
            if (!_movementHold.CanContinue(current)) _movementHold.Clear(_input);
            else if (!repeating) _movementHold.Pressed(Time.GetTicksUsec());
            // Direction changes during our own step take effect at its next input boundary.
            if (!current.CanWaitAtInput && _movementHold.CanContinue(current)) return;
        }
        if (_skipRequiresRelease || _battleHandoffPending || current.Story.EntrySkip is not null) return;
        if (action == GameAction.Cancel && BattleEntrySkip.Eligible(current) && GetWindow().HasFocus())
        {
            _skipRequiresRelease = true; _choiceRequiresRelease = true;
            Send(new SkipBattleEntry()); return;
        }
        if (action == GameAction.Wait)
        {
            if (!CanSubmitWait || !GetWindow().HasFocus() || !CanProcess()) { CancelFieldWait(); return; }
            _waitingAtInput = true;
            SubmitFieldWait();
            return;
        }
        SessionCommand? command = null;
        if (current.Story.Wait is ChoiceWait choice)
        {
            if (action == GameAction.Confirm && RevealText()) return;
            if (choice.Work is not null)
            {
                if (!current.CanWaitForChoice || _choiceRequiresRelease || !GetWindow().HasFocus() || !CanProcess()) return;
                ChoiceInput? input = action switch
                {
                    GameAction.Left => ChoiceInput.Left, GameAction.Right => ChoiceInput.Right,
                    GameAction.Up => ChoiceInput.Up, GameAction.Down => ChoiceInput.Down,
                    GameAction.Confirm => ChoiceInput.ConfirmC, GameAction.Cancel => ChoiceInput.Cancel, _ => null,
                };
                if (input is null) return;
                command = new PollChoice(choice.Token, input.Value);
            }
            else
            {
                if (action is not (GameAction.Confirm or GameAction.Cancel) || _choiceRequiresRelease || RevealText()) return;
                command = new ChooseDialogue(choice.Token, action == GameAction.Confirm);
            }
        }
        else if (current.Story.Wait is FieldTextWait fieldText)
        {
            if (action == GameAction.Confirm && !RevealText() && current.CanWaitForText)
                command = new Acknowledge(fieldText.Token);
        }
        else if (current.Story.Wait is W1TextWait w1)
        {
            if (action == GameAction.Confirm && !RevealText() && w1 is { AtInput: true, Revealed: true })
                command = new Acknowledge(w1.Token);
        }
        else if (current.Story.Wait is DialogueWait)
        {
            if (action == GameAction.Confirm && !RevealText()) command = new Acknowledge(current.Story.Wait.Token);
        }
        else if (current.CanWaitAtInput)
        {
            command = action switch
            {
                GameAction.Up => new Move(ExplorationDirection.North), GameAction.Right => new Move(ExplorationDirection.East),
                GameAction.Down => new Move(ExplorationDirection.South), GameAction.Left => new Move(ExplorationDirection.West),
                _ => null,
            };
            if (action == GameAction.Confirm)
            {
                if (_session.QueryExplorationInteraction() is { } target) command = new Interact(target);
            }
        }
        if (command is null) return;
        Send(command);
    }

    private bool RevealText()
    {
        if (TextRevealed) return _dialogueView.Panel.ReadMore();
        _revealed = _dialogue.GetTotalCharacterCount();
        _dialogue.VisibleCharacters = (int)_revealed;
        return true;
    }

    private void SpeakRevealedCharacters(int before, int after)
    {
        if (_session?.Current is not { Exploration: { } world, Story.TextWindow: OpenTextWindow { Speaker: { } speaker } } ||
            !world.TryResolveEntity(speaker, out var entity) || entity.Sprite is not { } sprite ||
            _session.Definition.Exploration!.Visuals is not { } visuals ||
            !visuals.Sprites.TryGetValue(sprite, out var visual)) return;
        // Source HandleDialogueTypewriting alternates non-space speech and resets at spaces.
        // Modern instant/reveal-all input deliberately skips incremental typewriting sounds.
        foreach (var character in _dialogue.Text.EnumerateRunes().Where(value => value.Value is not ('\r' or '\n'))
            .Skip(before).Take(after - before))
        {
            if (System.Text.Rune.IsWhiteSpace(character)) { _speechSoundToggle = false; continue; }
            _speechSoundToggle = !_speechSoundToggle;
            if (_speechSoundToggle) _audio?.PlayEffect(visual.Speech);
        }
    }

    private void Send(SessionCommand command)
    {
        var current = _session!.Current;
        // Observe the actual adapter handoff before Submit can resume its caller.
        // The last draw keeps its own identity; this signal neither draws nor completes anything.
        if (command is CompletePresentation completion) PublishResult("presentation-completion-before-submit", completion);
        _result = _session.Submit(new(current.SessionId, current.Revision, null, command));
        _movementHold.Observe(_input, current, _result, command);
        if (current.Story.Wait?.Token != _session.Current.Story.Wait?.Token) _tickTime = 0;
        if (!CanSubmitWait || current.Story.Wait?.Token != _session.Current.Story.Wait?.Token || _result.Failure is not null) CancelFieldWait();
        else _tickTime = 0;
        PublishResult("submit");
        bool skipping = current.Story.EntrySkip is not null || command is SkipBattleEntry;
        _audio?.Observe(_result, skipping);
        _audio?.ReconcileEntry(_session.Current);
        if (!skipping && _result.Failure is null && (command is ChooseDialogue || command is Acknowledge && (current.Story.TextSettings is null && current.Story.Wait is not (W1TextWait or FieldTextWait) ||
                _result.Observations.Any(row => row.Kind == "text-w2-accepted"))))
            _audio?.PlayEffect(67);
        if (!skipping && _result.Failure is null)
            foreach (var sound in _result.Observations.Where(row => row.Kind is "choice-sound" or "entity-sound"))
                _audio?.PlayEffect(int.Parse(sound.Detail!, System.Globalization.CultureInfo.InvariantCulture));
        if (!skipping && _result.Failure is null)
            foreach (var window in _result.Observations.Where(row => row.Kind == "portrait-window-moving"))
                _audio?.PlayEffect(65);
        if (_releaseBattle is not null && _result.Observations.Any(row => row.Kind == "map-transferred" ||
            row.Kind == "program-instruction" && row.Detail == "LoadSceneMap"))
        {
            _releaseBattle(); _releaseBattle = null; _battleMounted = false;
        }
        if (SessionPresentationOwner.IsBattle(_session.Current))
        {
            if (skipping || _skipRequiresRelease)
            {
                _battleHandoffPending = true;
                FinishEntryHandoff();
            }
            else { _handedOff = true; _enterBattle!(_result); }
            return;
        }
        Present();
        if (_session.Current.Story.Wait is ChoiceWait { Work.Phase: ChoicePhase.Entry } entry)
            Send(new ChoiceHeldInput(entry.Token, _input.GameplayHeld));
    }

    private void Present()
    {
        if (_session is null) return;
        if (!_battleMounted) _lastWorld = _session.Current.Exploration ?? _lastWorld;
        _dialogueView.Visible = PresentationFailure is null;
        _dialogueView.Present(_session, _input, WorldViewport, _battleMounted, TextRevealed);
        _failureLabel.GetParent<Control>().Visible = PresentationFailure is not null;
        _failureLabel.Text = PresentationFailure is { } failure
            ? $"The scene has stopped.\n{failure.Message} ({failure.Code})" : "";
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_lastWorld is not { } world) return;
        _presentation?.ObserveResources(HasConnections(ResourceDrawSignal) && ObservationCapture.Find(this)?.IsAccepting() != false);
        _presentation?.ObserveCamera(_legacyCamera || HasConnections(ResourceDrawSignal) || HasConnections(FactsSignal));
        if (_presentation?.Draw(world, _session!.Current) == true)
        {
            if (HasConnections(ResourceDrawSignal)) EmitSignal(ResourceDrawSignal);
            return;
        }
        var bounds = WorldViewportLayout.FieldBounds(world);
        int left = (int)(bounds.Position.X / 24), top = (int)(bounds.Position.Y / 24);
        int right = (int)(bounds.End.X / 24) - 1, bottom = (int)(bounds.End.Y / 24) - 1;
        float cell = WorldViewportLayout.CellSize;
        Vector2 origin = (bounds.Position - WorldViewport.Origin) * WorldViewportLayout.PixelScale;
        var screen = WorldViewport.Screen;
        for (int y = top; y <= bottom; y++)
            for (int x = left; x <= right; x++)
            {
                var rectangle = new Rect2(origin + new Vector2((x - left) * cell, (y - top) * cell), new(cell - 1, cell - 1));
                if (!rectangle.Intersects(screen)) continue;
                rectangle = rectangle.Intersection(screen);
                bool blocked = (world.Layout[x, y] & 0xC000) == 0xC000;
                DrawRect(rectangle, world.Definition.Caravan is not null && (world.Layout[x, y] & 0x3C00) == 0x3800
                    ? new Color(0.12f, 0.34f, 0.55f) : (world.Layout[x, y] & 0x3C00) == 0x1400
                    ? new Color(0.3f, 0.55f, 0.25f) : blocked ? new Color(0.12f, 0.17f, 0.22f) : new Color(0.24f, 0.33f, 0.32f));
            }
        _authoredDraws.Clear();
        foreach (var entity in world.AllEntities)
        {
            var center = origin + new Vector2((entity.Motion.X / 384f - left + 0.5f) * cell,
                (entity.Motion.Y / 384f - top + 0.5f) * cell);
            float radius = cell * 0.28f * entity.RenderSize / 24f;
            bool visible = entity.Visible && entity.Motion.X >= left * 384 && entity.Motion.X <= right * 384 &&
                entity.Motion.Y >= top * 384 && entity.Motion.Y <= bottom * 384 &&
                screen.Encloses(new Rect2(center - Vector2.One * radius, Vector2.One * radius * 2));
            _authoredDraws[entity.Slot] = new(entity.Slot, entity.Sprite, center.X, center.Y, radius,
                entity.RenderSize, visible, entity.SpriteRequest, Engine.GetProcessFrames());
            if (!visible) continue;
            var color = entity.Entity == world.Player ? new Color(0.9f, 0.76f, 0.36f) : new Color(0.44f, 0.7f, 0.9f);
            if (entity.Sprite == 62) DrawRect(new(center - new Vector2(radius, radius * 0.7f), new Vector2(radius * 2, radius * 1.4f)), new Color(0.65f, 0.4f, 0.2f));
            else DrawCircle(center, radius, color);
            var facing = entity.Motion.Facing switch { 0 => Vector2.Right, 1 => Vector2.Up, 2 => Vector2.Left, _ => Vector2.Down };
            DrawLine(center, center + facing * radius, Colors.White, 2);
        }
    }

    public string ReadObservationJson()
    {
        _legacyCamera = true;
        _presentation?.ObserveCamera(true);
        return JsonSerializer.Serialize(ObservationFacts(true, true));
    }

    public Godot.Collections.Dictionary ReadCaptureClock() => new()
    {
        ["simulationTick"] = _session!.Current.Story.SimulationTick,
        ["token"] = _session.Current.Story.Wait is { } wait ? Variant.From(wait.Token.Value) : default,
        ["revision"] = _session.Current.Revision,
        ["presentation"] = new Godot.Collections.Dictionary { ["cameraX"] = _presentation?.Camera.X ?? 0, ["cameraY"] = _presentation?.Camera.Y ?? 0 },
    };

    public bool CaptureHasConsumerWait() => _session?.Current.Story.Wait is EntityWait or NodWait or FullFadeWait;
    public bool CaptureNeedsPostDraw(bool rawText, bool choice, bool white, bool battleEntry)
    {
        var current = _session?.Current;
        if (current is null) return false;
        if (CaptureHasConsumerWait() || current.Story.Wait is PresentationWait { Cue.Resource: "nod" or "shiver" or "mosaic-in" or "mosaic-out" or "black" or "white" }) return true;
        if (rawText && current.Story.TextWindow is OpenTextWindow { Text: 447 } || choice && current.Story.Wait is ChoiceWait) return true;
        return !white || current.CanWaitForText || battleEntry && _presentation?.ActiveCue is "EntityEffect" or "Gesture" or "BattleLoad" or "FadeIn" or "FadeOut";
    }
    public Godot.Collections.Dictionary ReadCapturePrefix()
    {
        var current = _session!.Current;
        return ObservationCapture.ToDictionary(new
        {
            sessionId = current.SessionId, revision = current.Revision, simulationTick = current.Story.SimulationTick,
            observationSequence = current.ObservationSequence, map = current.Exploration?.Map.Value, mode = current.Mode.ToString(),
            stop = current.StopReason.ToString(), wait = current.Story.Wait?.GetType().Name, token = current.Story.Wait?.Token.Value,
            cursor = current.Story.Cursor, mainSeed = current.Exploration?.Party.MainSeed, failure = PresentationFailure?.Code,
            focused = GetWindow().HasFocus(), tickDebt = _tickTime,
        });
    }

    public Godot.Collections.Dictionary ReadCaptureState(bool full = false, bool resources = false) =>
        ObservationCapture.ToDictionary(ObservationFacts(full, resources));

    private static Rect2I LayoutRegion(int x, int y, int width, int height)
    {
        if (x < 0 || y < 0 || width < 1 || height < 1 || width > 64 || height > 64 ||
            x > 64 - width || y > 64 - height || width * height > 256)
            throw new ArgumentOutOfRangeException(nameof(width), "A layout observation must fit the map and contain at most 256 cells.");
        return new(x, y, width, height);
    }

    public int ObserveLayoutRegion(int x, int y, int width, int height)
    {
        var region = LayoutRegion(x, y, width, height);
        int request = _presentation!.ObserveLayoutRegion(region);
        QueueRedraw();
        return request;
    }

    public void StopLayoutObservation() => _presentation?.ObserveLayoutRegion(null);

    public Godot.Collections.Dictionary ReadLayoutRegion(int x, int y, int width, int height)
    {
        _ = LayoutRegion(x, y, width, height);
        var current = _session!.Current;
        var world = current.Exploration ?? throw new InvalidOperationException("Layout observation requires active exploration.");
        var words = new List<ushort>(width * height);
        var saved = new List<object>();
        var roof = world.RoofState as MapBlockCopyLifecycleActiveState;
        for (int row = y; row < y + height; row++)
            for (int column = x; column < x + width; column++)
            {
                words.Add(world.Layout[column, row]);
                if (roof is not null && column >= roof.DestinationX && column < roof.DestinationX + roof.Width &&
                    row >= roof.DestinationY && row < roof.DestinationY + roof.Height)
                    saved.Add(new { x = column, y = row, word = roof.SavedWords[(row - roof.DestinationY) * roof.Width + column - roof.DestinationX] });
            }
        return ObservationCapture.ToDictionary(new { sessionId = current.SessionId, revision = current.Revision,
            observationSequence = current.ObservationSequence, map = world.Map.Value, x, y, width, height, words,
            flags = current.Story.Flags, roof = roof is null ? null : new { roof.RecordOrdinal, roof.DestinationX,
                roof.DestinationY, roof.Width, roof.Height, saved }, draw = _presentation?.LayoutDrawObservation });
    }

    // Only callback control operands cross into GDScript. Full evidence is captured
    // below at the same synchronous boundary, without a CLR/Variant/CLR round trip.
    public Godot.Collections.Dictionary ReadCaptureWitness(bool route = false)
    {
        // Route control needs the same actual draw clock as the full observation,
        // without pulling unused resource evidence into each readiness poll.
        if (route) { _legacyCamera = true; _presentation?.ObserveCamera(true); }
        var state = ObservationCapture.FactFields(ObservationFacts(false,false));
        string[] names = ["sessionId","revision","observationSequence","simulationTick","mainSeed","map","mode",
            "stop","wait","token","cursor","failure","focused","tickDebt","canWaitAtInput","canWaitForText",
            "visibleCharacters","totalCharacters","textId","entityWait","nod","fade","presentationWait",
            "presentation","choice","choiceProjection","logicalView"];
        var witness = names.ToDictionary(name=>name,name=>state[name]);
        var projection = ObservationCapture.FactFields(state["cameraProjection"]);
        witness["cameraProjection"] = state["cameraProjection"] is null ? null :
            new[] { "drawSequence","processFrame","sessionId","revision","observationSequence","simulationTick","token" }
                .ToDictionary(name=>name,name=>projection[name]);
        if (route)
        {
            witness["portraitId"] = state["portraitId"];
            witness["portraitWork"] = state["portraitWork"];
            witness["entities"] = _session?.Current.Exploration?.AllEntities.Select(entity => new
            {
                id = entity.Entity.Value, x = entity.Motion.X, y = entity.Motion.Y,
                facing = entity.Motion.Facing, busy = entity.Busy,
            });
        }
        return ObservationCapture.ToDictionary(witness);
    }

    public bool RecordCaptureConsumer(Godot.Collections.Dictionary envelope, Godot.Collections.Dictionary context,
        Godot.Collections.Dictionary fieldLabel)
    {
        var capture = ObservationCapture.Find(this);
        if (capture?.IsAccepting() != true) return false;
        var state = ObservationCapture.FactFields(ObservationFacts(false,false));
        return capture.RecordSnapshotFacts("consumerBoundaries",CaptureConsumerFacts(state,envelope,context,fieldLabel));
    }

    private static object CaptureConsumerFacts(Dictionary<string,object?> state, Godot.Collections.Dictionary envelope,
        Godot.Collections.Dictionary context, Godot.Collections.Dictionary fieldLabel)
    {
        string[] names = ["entityWait","entities","callers","continuation","stop","nod","nodProjection",
            "fade","display","presentation","cameraProjection","presentationWait","presentationCue",
            "canWaitForChoice","waitingAtInput","eventCaller","callerReturning"];
        var consumer = names.ToDictionary(name=>name,name=>state[name]);
        foreach (var pair in context) consumer[pair.Key.AsString()] = pair.Value;
        consumer["fieldLabel"] = fieldLabel;
        var row = envelope.ToDictionary(pair=>pair.Key.AsString(),pair=>(object?)pair.Value);
        row["state"] = consumer;
        row["drawDelivery"] = CaptureDrawDelivery(state);
        return row;
    }

    private static object CaptureDrawDelivery(Dictionary<string,object?> state)
    {
        var projection = ObservationCapture.FactFields(state["cameraProjection"]);
        var wait = state["entityWait"] ?? state["nod"];
        var subject = wait is not null ? ObservationCapture.FactFields(wait).GetValueOrDefault("Entity") :
            ObservationCapture.FactFields(ObservationCapture.FactFields(state["presentationWait"]).GetValueOrDefault("Cue")).GetValueOrDefault("Entity");
        var id = ObservationCapture.FactFields(subject).GetValueOrDefault("Value");
        object? actor = null, visible = null;
        foreach (var item in (System.Collections.IEnumerable?)state["entities"] ?? Array.Empty<object>())
        {
            var entity = ObservationCapture.FactFields(item);
            if (Equals(entity["id"],id)) { visible=entity["Visible"]; break; }
        }
        foreach (var item in (System.Collections.IEnumerable?)projection.GetValueOrDefault("actors") ?? Array.Empty<object>())
        {
            var candidate = ObservationCapture.FactFields(item);
            if (Equals(candidate["entity"],id)) { actor=item; break; }
        }
        return new { processFrame=Engine.GetProcessFrames(), subject=id,
            projectionAvailable=projection.Count!=0, drawSequence=projection.GetValueOrDefault("drawSequence"),
            projectionProcessFrame=projection.GetValueOrDefault("processFrame"), projectionToken=projection.GetValueOrDefault("token"),
            projectionTick=projection.GetValueOrDefault("simulationTick"), projectionSessionId=projection.GetValueOrDefault("sessionId"),
            logicalSubjectVisible=visible,
            sameToken=projection.Count!=0 && Equals(projection.GetValueOrDefault("token"),state["token"]),
            sameTick=projection.Count!=0 && Equals(projection.GetValueOrDefault("simulationTick"),state["simulationTick"]),
            actorStatus=id is null ? "no-subject" : actor is null ? "missing" :
                Equals(ObservationCapture.FactFields(actor)["visible"],true) ? "drawn" : "culled", actor };
    }

    public bool RecordCaptureCamera(Godot.Collections.Dictionary before, Godot.Collections.Dictionary after)
    {
        var capture = ObservationCapture.Find(this);
        if (capture?.IsAccepting() != true) return false;
        var state = ObservationCapture.FactFields(ObservationFacts(false,false));
        return capture.RecordSnapshotFacts("cameraDraws",new { projection=state["cameraProjection"],
            logicalView=state["logicalView"], entities=state["entities"], exposedBefore=before, exposedAfter=after });
    }

    private object ObservationFacts(bool full, bool resources)
    {
        _presentation?.ObserveResources(HasConnections(ResourceDrawSignal) && ObservationCapture.Find(this)?.IsAccepting() != false);
        _presentation?.ObserveCamera(_legacyCamera || HasConnections(ResourceDrawSignal) || HasConnections(FactsSignal));
        var current = _session?.Current;
        return new
        {
            sessionId = current?.SessionId, revision = current?.Revision, observationSequence = current?.ObservationSequence, mode = current?.Mode.ToString(),
            canWaitAtInput = current?.CanWaitAtInput, waitingAtInput = _waitingAtInput,
            canWaitForChoice = current?.CanWaitForChoice, choice = current?.Story.Wait as ChoiceWait,
            movementDirection = _input.MovementDirection?.ToString(),
            gameplayHeld = _input.GameplayHeld, choiceRequiresRelease = _choiceRequiresRelease,
            entrySkip = current?.Story.EntrySkip, skipRequiresRelease = _skipRequiresRelease, battleHandoffPending = _battleHandoffPending,
            choiceProjection = new { tick = current?.Story.SimulationTick, token = (current?.Story.Wait as ChoiceWait)?.Token.Value,
                visible = _choice.Visible, x = _choice.Position.X, y = _choice.Position.Y, width = _choice.Size.X, height = _choice.Size.Y,
                yes = _choiceYes.Text, no = _choiceNo.Text, yesAlternate = _choiceYes.Modulate == UI.StoryTheme.Selected, noAlternate = _choiceNo.Modulate == UI.StoryTheme.Selected },
            canWaitForText = current?.CanWaitForText, inputFirstEntityService = (current?.Story.Wait as DialogueWait)?.InputFirstEntityService,
            fieldText = current?.Story.Wait as FieldTextWait, logicalText = current?.Story.LogicalText, logicalView = current?.Story.LogicalView,
            music = current?.Story.Music, musicWait = current?.Story.Wait as MusicWait,
            windowFixPending = current?.Story.WindowFixPending,
            nod = current?.Story.Wait as NodWait, nodProjection = _presentation?.NodProjection,
            entityWait = current?.Story.Wait as EntityWait,
            presentationWait = current?.Story.Wait as PresentationWait, presentationCue = _presentation?.CueProjection,
            cameraProjection = resources ? _presentation?.CameraProjection : _presentation?.CameraCompactProjection,
            portraitResourceProjection = _presentation?.PortraitResourceProjection,
            textSettings = current?.Story.TextSettings, w1 = current?.Story.Wait as W1TextWait,
            randomSeedCopy = current?.CurrentRandomSeedCopy, lastTextSeedCopy = current?.Story.RandomSeedCopy,
            entityEvent = current?.Story.EntityEvent,
            eventCaller = current?.Story.EventCaller?.GetType().Name,
            callerReturning = current?.Story.EventCaller?.Returning,
            entitiesRunning = current?.Story.EntityServices ?? (current?.Story.Cursor is { } location ? _session!.Definition.Exploration!.Programs[location.Program].EntitiesRunning : (bool?)null),
            typewriting = current?.Story.Typewriting,
            textWindow = current?.Story.TextWindow.GetType().Name,
            portraitWindow = current?.Story.PortraitWindow.GetType().Name,
            portraitId = (current?.Story.PortraitWindow as OpenPortraitWindow)?.Portrait,
            portraitFlags = (current?.Story.PortraitWindow as OpenPortraitWindow)?.Flags,
            portraitWork = (current?.Story.PortraitWindow as OpenPortraitWindow)?.Work,
            portraitProjection = HasMeta("portrait_projection") ? GetMeta("portrait_projection").AsGodotDictionary()
                .ToDictionary(pair => pair.Key.AsString(), pair => pair.Value.VariantType switch
                {
                    Variant.Type.Array => (object)pair.Value.AsGodotArray().Select(value => value.AsInt32()).ToArray(),
                    Variant.Type.Bool => pair.Value.AsBool(),
                    Variant.Type.Float => pair.Value.AsDouble(),
                    _ => pair.Value.AsInt32(),
                }) : null,
            waitHeld = _input.WaitHeld, focused = GetWindow().HasFocus(),
            viewport = Battles.BattleMapViewport.Rectangle(GetViewportRect()),
            mapViewport = _presentation is { } presented ? Battles.BattleMapViewport.Rectangle(GetViewportRect()) : null,
            worldGeometry = new { cellSize = WorldViewportLayout.CellSize, pixelScale = WorldViewportLayout.PixelScale,
                cameraX = WorldViewport.Origin.X, cameraY = WorldViewport.Origin.Y,
                bounds = Battles.BattleMapViewport.Rectangle(WorldViewport.Bounds),
                legalScreen = Battles.BattleMapViewport.Rectangle(WorldViewport.Screen) },
            battleMounted = _battleMounted,
            party = current?.Exploration?.Party.Actors, gold = current?.Exploration?.Party.Gold,
            itemCustody = current?.Exploration?.Party.ItemCustody,
            admittedParty = full && current?.Exploration is { } world ? new
            {
                package = _session!.Definition.Package, origin = _session.Definition.Origin,
                provenance = _session.Definition.Exploration?.Provenance,
                encounter = world.Party.Encounter,
                // Definition operands, not an instantiated battle at field startup.
                encounters = _session.Definition.Encounters.Select(pair => new
                {
                    encounter = pair.Key,
                    deployments = pair.Value.Deployments.Where(deployment =>
                        world.Party.Actors.Any(input => input.Actor == deployment.Actor)).Select(deployment => new
                    {
                        actor = deployment.Actor.Value, member = deployment.Initialization?.AllyPartyMember,
                        processingOrder = deployment.ProcessingOrder,
                        definition = new
                        {
                            classRule = deployment.Definition.ClassRule.ToString(),
                            level = deployment.Definition.Level, maxHp = deployment.Definition.MaxHp, maxMp = deployment.Definition.MaxMp,
                            attack = deployment.Definition.Attack, defense = deployment.Definition.Defense,
                            agility = deployment.Definition.Agility, extraRoundAction = deployment.Definition.ExtraRoundAction,
                            move = deployment.Definition.Move, spells = deployment.Definition.Spells,
                            sourceLoadout = deployment.Definition.SourceLoadout,
                        },
                    }),
                }),
            } : null,
            travelMode = current?.Exploration?.TravelMode.ToString(), currentEntities = current?.Exploration?.CurrentEntities,
            authoredDraws = _authoredDraws.Values,
            map = current?.Exploration?.Map.Value, stop = current?.StopReason.ToString(),
            flags = current?.Story.Flags, simulationTick = current?.Story.SimulationTick, cursor = current?.Story.Cursor, wait = current?.Story.Wait?.GetType().Name,
            continuation = current?.Story.Continuation.ToString(), enteringBattle = current?.Story.EnteringBattle, callers = current?.Story.Callers,
            stepCounter = current?.Story.StepCounter, battleRequest = current?.Story.BattleRequest,
            egressMap = current?.Story.EgressMap?.Value, thinkingSeed = current?.Exploration?.Party.ThinkingSeed,
            display = current?.Story.Display, fade = current?.Story.Wait as FullFadeWait,
            warp = current?.Story.Warp, loadServices = (current?.Story.Wait as WarpLoadWait)?.Remaining,
            tickDebt = _tickTime, failureVisible = _failureLabel.IsVisibleInTree(),
            token = current?.Story.Wait?.Token.Value, failure = PresentationFailure?.Code, failureField = PresentationFailure?.Field, failureKind = PresentationFailure?.Kind.ToString(), spriteSize = current?.Exploration?.SpriteSize,
            dialoguePanel = _dialogueView.Panel.Observe(), dialogue = _dialogue.Text, help = _help.Text, helpVisible = _help.IsVisibleInTree(), visibleCharacters = _dialogue.VisibleCharacters,
            totalCharacters = _dialogue.GetTotalCharacterCount(), textMode = _input.Settings.TextMode,
            reducedFlash = _input.Settings.ReducedFlash,
            textId = (current?.Story.TextWindow as OpenTextWindow)?.Text,
            speaker = (current?.Story.TextWindow as OpenTextWindow)?.Speaker?.Value,
            speakerFlags = (current?.Story.TextWindow as OpenTextWindow)?.SpeakerFlags,
            partyLists = current?.Story.PartyLists, mainSeed = current?.Exploration?.Party.MainSeed,
            presentation = new { spriteMounts = _presentation?.SpriteMounts, gestureDraws = _presentation?.GestureDraws,
                nodDraws = _presentation?.NodDraws, restoredGestureDraws = _presentation?.RestoredGestureDraws,
                soundStarts = _presentation?.SoundStarts, soundFades = _presentation?.SoundFades,
                soundCompletions = _presentation?.SoundCompletions, soundStops = _presentation?.SoundStops,
                musicCue = _presentation?.MusicCue, musicPlaying = _presentation?.MusicPlaying,
                musicFinished = _presentation?.MusicFinished, musicPosition = _presentation?.MusicPosition,
                mosaicOutDraws = _presentation?.MosaicOutDraws,
                paletteFades = _presentation?.PaletteFades, paletteBrightness = _presentation?.PaletteBrightness,
                whiteOpacity = _presentation?.WhiteOpacity, suppressedWhiteCues = _presentation?.SuppressedWhiteCues,
                completedCueToken = _presentation?.CompletedCueToken, completedCueKind = _presentation?.CompletedCueKind,
                mosaicDraws = _presentation?.MosaicDraws,
                shiverDraws = _presentation?.ShiverDraws, battleLoads = _presentation?.BattleLoads,
                cameraX = _presentation?.Camera.X, cameraY = _presentation?.Camera.Y, activeCue = _presentation?.ActiveCue,
                error = _presentation?.Error },
            entities = current?.Exploration?.AllEntities.Select(entity => new
            {
                id = entity.Entity.Value, slot = entity.Slot, sprite = entity.Sprite, facing = entity.Motion.Facing,
                priority = entity.Priority, renderSize = entity.RenderSize,
                follower = entity.Follower, spriteRequest = entity.SpriteRequest, spriteReady = entity.SpriteReady, waitingForSprite = entity.WaitingForSprite,
                x = entity.Motion.X, y = entity.Motion.Y,
                targetX = entity.Motion.XDestination, targetY = entity.Motion.YDestination,
                velocityX = entity.Motion.XVelocity, velocityY = entity.Motion.YVelocity,
                travelX = entity.Motion.XTravel, travelY = entity.Motion.YTravel,
                speedY = entity.Motion.YSpeed, accelerationX = entity.Motion.XAcceleration, accelerationY = entity.Motion.YAcceleration,
                layer = entity.Motion.Layer, animationCounter = entity.Motion.AnimationCounter, waitTimer = entity.Motion.WaitTimer,
                moving = entity.Motion.IsMoving, busy = entity.Busy, isScriptIdle = entity.IsScriptIdle,
                entity.Visible, actionCursor = entity.ActionCursor, speedX = entity.Motion.XSpeed, flagsA = entity.Motion.FlagsA, flagsB = entity.Motion.FlagsB,
                waitingForMotion = entity.WaitingForMotion,
                actionProgram = entity.Actions?.Actions.Select(action => new
                {
                    kind = action.GetType().Name, operands = (object)action,
                }),
            }),
            observations = _result?.Observations,
            audio = full ? _audio?.ObservePlayback() : _audio?.ObserveState(),
        };
    }
}
