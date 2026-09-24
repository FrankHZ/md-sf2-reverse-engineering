using System.Text.Json;
using System.Text.RegularExpressions;
using Godot;
using Sf2.Remake.GodotAdapter.Audio;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Exploration;

public sealed partial class ExplorationSessionView : Control
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
    private Action<SessionResult>? _enterBattle;
    private bool _handedOff;
    private bool _battleMounted;
    private Action? _releaseBattle;
    private Label _title = null!;
    private Label _dialogue = null!;
    private Label _help = null!;
    private Label _failureLabel = null!;
    private GameInput _input = null!;
    private TextWindow? _textWindow;
    private double _revealed;
    private bool _speechSoundToggle;
    private double _tickTime;
    private bool _resumeAutomatic;
    private bool _waitingAtInput;
    private ulong _nextWaitMicros;
    private ulong _lastWaitFrame;
    private ExplorationState? _lastWorld;
    private ExplorationPresentation? _presentation;
    private SessionAudio? _audio;
    private SessionFailure? PresentationFailure => _result?.Failure ??
        (_presentation?.Error is { } error ? new(SessionFailureKind.AdapterError,
            "presentation-unavailable", "presentation", error) : null);

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        GetWindow().ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
        _title = new Label { Name = "Title", Position = new(20, 16) };
        _dialogue = new Label { Name = "Dialogue", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _help = new Label { Name = "Help", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        AddChild(_title); AddChild(_dialogue); AddChild(_help);
        // Failure text must remain readable when the world is correctly held black.
        var failureLayer = new CanvasLayer { Layer = 101 };
        _failureLabel = new Label { Name = "TransitionFailure", Position = new(20, 20), Visible = false };
        AddChild(failureLayer); failureLayer.AddChild(_failureLabel);
        GetViewport().SizeChanged += Present;
        GetWindow().FocusExited += SuspendClock;
        VisibilityChanged += SuspendClock;
    }
    public override void _ExitTree()
    {
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

    private void SuspendClock() { CancelFieldWait(); _tickTime = 0; _resumeAutomatic = true; }

    private void CancelFieldWait()
    {
        _waitingAtInput = false; _nextWaitMicros = 0;
        _input?.DisarmWait();
    }

    private void SubmitFieldWait()
    {
        _lastWaitFrame = Engine.GetProcessFrames();
        _nextWaitMicros = Time.GetTicksUsec() + 16667;
        Send(_session!.Current.CanWaitForText ? new WaitForText(_session.Current.Story.Wait!.Token) : new WaitAtInput());
    }

    private bool TextRevealed => _dialogue.VisibleCharacters < 0 || _dialogue.VisibleCharacters >= _dialogue.GetTotalCharacterCount();
    private bool CanSubmitWait => _session is { } session &&
        (session.Current.CanWaitAtInput || session.Current.CanWaitForText && TextRevealed);

    internal void Begin(GameSession session, SessionResult result, GameInput input, SessionAudio audio, Action<SessionResult> enterBattle,
        Action<SessionResult> prepareBattle, Action? releaseBattle = null)
    {
        _session = session; _result = result; _input = input; _enterBattle = enterBattle;
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
            _title.Hide(); _dialogue.Hide(); _help.Hide();
            QueueRedraw();
        }, audio);
        TextureFilter = TextureFilterEnum.Nearest;
        Present();
    }

    public override void _Process(double delta)
    {
        if (_handedOff || _session is null || _session.Current.StopReason is SessionStopReason.Unsupported or SessionStopReason.Faulted) return;
        if ((_session.Current.Story.Warp is not null || _session.Current.Story.Wait is FullFadeWait) &&
            (!IsVisibleInTree() || !GetWindow().HasFocus())) { SuspendClock(); return; }
        if (_resumeAutomatic) { delta = 0; _resumeAutomatic = false; }
        if (_dialogue.VisibleCharacters >= 0)
        {
            int before = _dialogue.VisibleCharacters;
            _revealed = Math.Min(_dialogue.GetTotalCharacterCount(), _revealed + delta * _input.Settings.CharactersPerSecond);
            _dialogue.VisibleCharacters = (int)_revealed;
            SpeakRevealedCharacters(before, _dialogue.VisibleCharacters);
        }
        if (_presentation is { } presentation)
        {
            var beforePresentation = _session.Current.Story.Wait?.Token;
            foreach (var completion in presentation.Update(delta, _session.Current))
            {
                Send(completion);
                if (_handedOff || _result?.Failure is not null) return;
            }
            QueueRedraw();
            if (presentation.Error is not null) { Present(); return; }
            if (beforePresentation != _session.Current.Story.Wait?.Token) { _tickTime = 0; return; }
        }
        if (_session.Current.CanWaitAtInput || _session.Current.CanWaitForText)
        {
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
                if (_handedOff || token != _session.Current.Story.Wait?.Token || _session.Current.CanWaitAtInput || _session.Current.CanWaitForText || !NeedsTicks(_session.Current)) _tickTime = 0;
            }
        }
        else _tickTime = 0;
    }

    private static bool NeedsTicks(SessionSnapshot current) =>
        current.Story.Wait is FullFadeWait fade ? !fade.LogicalDone :
            current.StopReason == SessionStopReason.SimulationWait ||
            current.Exploration?.AllEntities.Any(entity => entity.Busy || entity.Follower is not null) == true;

    internal void HandleAction(GameAction action)
    {
        if (action != GameAction.Wait) CancelFieldWait();
        if (_handedOff || !IsVisibleInTree() || _session is null || _session.Current.HasBattleControl || _session.Current.BattleScene is not null || PresentationFailure is not null) return;
        var current = _session.Current;
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
            if (action is not (GameAction.Confirm or GameAction.Cancel) || RevealText()) return;
            command = new ChooseDialogue(choice.Token, action == GameAction.Confirm);
        }
        else if (current.Story.Wait is DialogueWait)
        {
            if (action == GameAction.Confirm && !RevealText()) command = new Acknowledge(current.Story.Wait.Token);
        }
        else if (current.StopReason == SessionStopReason.PlayerInput && current.Exploration is { } world)
        {
            command = action switch
            {
                GameAction.Up => new Move(ExplorationDirection.North), GameAction.Right => new Move(ExplorationDirection.East),
                GameAction.Down => new Move(ExplorationDirection.South), GameAction.Left => new Move(ExplorationDirection.West),
                _ => null,
            };
            if (action == GameAction.Confirm)
            {
                var player = world.PlayerEntity;
                (int x, int y) = player.Motion.Facing switch { 0 => (1, 0), 1 => (0, -1), 2 => (-1, 0), _ => (0, 1) };
                var target = world.AllEntities.FirstOrDefault(entity => entity.Entity != world.Player && entity.Visible &&
                    entity.Position.X == player.Position.X + x && entity.Position.Y == player.Position.Y + y);
                if (target is not null) command = new Interact(target.Entity);
            }
        }
        if (command is null) return;
        Send(command);
    }

    private bool RevealText()
    {
        if (_dialogue.VisibleCharacters < 0 || _dialogue.VisibleCharacters >= _dialogue.GetTotalCharacterCount()) return false;
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
        _result = _session.Submit(new(current.SessionId, current.Revision, null, command));
        if (current.Story.Wait?.Token != _session.Current.Story.Wait?.Token) _tickTime = 0;
        if (!CanSubmitWait || current.Story.Wait?.Token != _session.Current.Story.Wait?.Token || _result.Failure is not null) CancelFieldWait();
        else _tickTime = 0;
        PublishResult("submit");
        _audio?.Observe(_result);
        if (_result.Failure is null && command is Acknowledge or ChooseDialogue) _audio?.PlayEffect(67);
        if (_releaseBattle is not null && _result.Observations.Any(row => row.Kind == "map-transferred" ||
            row.Kind == "program-instruction" && row.Detail == "LoadSceneMap"))
        {
            _releaseBattle(); _releaseBattle = null; _battleMounted = false;
        }
        if (_result.Failure is null && (_session.Current.HasBattleControl || _session.Current.BattleScene is not null))
        {
            _handedOff = true;
            _enterBattle!(_result);
            return;
        }
        Present();
    }

    private void Present()
    {
        if (_session is null) return;
        var current = _session.Current;
        if (!_battleMounted) _lastWorld = current.Exploration ?? _lastWorld;
        var size = GetViewportRect().Size;
        _title.Text = _session.Definition.Package.Replace('-', ' ').ToUpperInvariant();
        bool portrait = _session.Definition.Exploration?.Visuals is not null &&
            current.Story.PortraitWindow is OpenPortraitWindow or UnknownPortraitWindow { LegacySpeaker: not null };
        byte portraitFlags = current.Story.PortraitWindow switch
        { OpenPortraitWindow open => open.Flags, UnknownPortraitWindow unknown => unknown.LegacyFlags, _ => 0 };
        bool rightPortrait = (portraitFlags & 0x80) != 0;
        _dialogue.Position = new(portrait && !rightPortrait ? 110 : 20, Mathf.Max(80, size.Y - 175));
        _dialogue.Size = new(Mathf.Max(1, size.X - _dialogue.Position.X - (portrait && rightPortrait ? 110 : 20)), 75);
        _help.Position = new(20, Mathf.Max(130, size.Y - 90));
        _help.Size = new(Mathf.Max(1, size.X - 40), 80);
        string previousText = _dialogue.Text;
        _dialogue.Text = current.Story.Wait switch
        {
            DialogueWait text => _session.Definition.Exploration!.Texts.GetValueOrDefault(text.Text) ?? "",
            ChoiceWait => current.Story.TextWindow is OpenTextWindow question ? _session.Definition.Exploration!.Texts.GetValueOrDefault(question.Text) ?? "" : "Would you like to continue?",
            PresentationWait => current.Story.TextWindow is OpenTextWindow text ? _session.Definition.Exploration!.Texts.GetValueOrDefault(text.Text) ?? "" : "",
            _ => current.Story.TextWindow is OpenTextWindow open ? _session.Definition.Exploration!.Texts.GetValueOrDefault(open.Text) ?? "" : "",
        };
        var names = _session.Definition.Exploration!.MemberNames;
        _dialogue.Text = Regex.Replace(_dialogue.Text.Replace("{N}", "\n"), @"\{W[0-9]+\}", "");
        _dialogue.Text = Regex.Replace(_dialogue.Text, @"\{(?:LEADER|NAME;([0-9]+))\}", match =>
        {
            int member = match.Groups[1].Success ? int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) : 0;
            return member < names.Count ? names[member] : match.Value;
        });
        _help.Text = current.Story.Wait switch
        {
            ChoiceWait => $"{_input.Hint(GameAction.Confirm)}: Yes     {_input.Hint(GameAction.Cancel)}: No",
            DialogueWait => $"{_input.Hint(GameAction.Confirm)}: Reveal / Continue",
            EntityWait or TickWait or PresentationWait or FullFadeWait or WarpLoadWait => "",
            _ => $"{_input.MovementHint}\n{_input.Hint(GameAction.Confirm)}: Talk",
        };
        if (current.CanWaitAtInput || current.CanWaitForText)
            _help.Text += $"\nHold {_input.Hint(GameAction.Wait)} to wait; release to pause field time (including NPCs).";
        if (PresentationFailure is { } failure)
            _dialogue.Text = $"{failure.Message} ({failure.Code})";
        _failureLabel.Visible = PresentationFailure is not null;
        _failureLabel.Text = PresentationFailure is { } visibleFailure ? $"{visibleFailure.Message} ({visibleFailure.Code})" : "";
        // A sound wait can become an input wait on the same already-visible text window.
        // Restart reveal only for changed text or a newly opened window, not that wait handoff.
        if (_dialogue.Text != previousText || !ReferenceEquals(current.Story.TextWindow, _textWindow))
        {
            _revealed = 0;
            _dialogue.VisibleCharacters = _input.Settings.TextMode == "instant" || PresentationFailure is not null ? -1 : 0;
        }
        _textWindow = current.Story.TextWindow;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_lastWorld is not { } world) return;
        if (_presentation?.Draw(world, _session!.Current.Story) == true) return;
        var bounds = world.Definition.Traversal.ActiveAreas;
        int left = bounds.Min(area => area.MinimumX), top = bounds.Min(area => area.MinimumY);
        int right = bounds.Max(area => area.MaximumX), bottom = bounds.Max(area => area.MaximumY);
        var size = GetViewportRect().Size;
        float cell = Mathf.Max(1, Mathf.Min((size.X - 40) / (right - left + 1), (size.Y - 210) / (bottom - top + 1)));
        Vector2 origin = new(20, 55);
        for (int y = top; y <= bottom; y++)
            for (int x = left; x <= right; x++)
            {
                var rectangle = new Rect2(origin + new Vector2((x - left) * cell, (y - top) * cell), new(cell - 1, cell - 1));
                bool blocked = (world.Layout[x, y] & 0xC000) == 0xC000;
                DrawRect(rectangle, blocked ? new Color(0.12f, 0.17f, 0.22f) : new Color(0.24f, 0.33f, 0.32f));
            }
        foreach (var entity in world.AllEntities.Where(entity => entity.Visible))
        {
            var center = origin + new Vector2((entity.Motion.X / 384f - left + 0.5f) * cell,
                (entity.Motion.Y / 384f - top + 0.5f) * cell);
            DrawCircle(center, cell * 0.28f, entity.Entity == world.Player ? new Color(0.9f, 0.76f, 0.36f) : new Color(0.44f, 0.7f, 0.9f));
        }
    }

    public string ReadObservationJson()
    {
        var current = _session?.Current;
        return JsonSerializer.Serialize(new
        {
            sessionId = current?.SessionId, revision = current?.Revision, mode = current?.Mode.ToString(),
            canWaitAtInput = current?.CanWaitAtInput, waitingAtInput = _waitingAtInput,
            canWaitForText = current?.CanWaitForText, inputFirstEntityService = (current?.Story.Wait as DialogueWait)?.InputFirstEntityService,
            portraitWindow = current?.Story.PortraitWindow.GetType().Name,
            portraitId = (current?.Story.PortraitWindow as OpenPortraitWindow)?.Portrait,
            portraitFlags = (current?.Story.PortraitWindow as OpenPortraitWindow)?.Flags,
            portraitProjection = HasMeta("portrait_projection") ? GetMeta("portrait_projection").AsGodotDictionary()
                .ToDictionary(pair => pair.Key.AsString(), pair => pair.Value.AsInt32()) : null,
            waitHeld = _input.WaitHeld, focused = GetWindow().HasFocus(),
            viewport = Battles.BattleMapViewport.Rectangle(GetViewportRect()),
            mapViewport = _presentation is { } presented ? Battles.BattleMapViewport.Rectangle(presented.Screen) : null,
            battleMounted = _battleMounted,
            party = current?.Exploration?.Party.Actors, gold = current?.Exploration?.Party.Gold,
            map = current?.Exploration?.Map.Value, stop = current?.StopReason.ToString(),
            flags = current?.Story.Flags, simulationTick = current?.Story.SimulationTick, cursor = current?.Story.Cursor, wait = current?.Story.Wait?.GetType().Name,
            display = current?.Story.Display, fade = current?.Story.Wait as FullFadeWait,
            warp = current?.Story.Warp, loadServices = (current?.Story.Wait as WarpLoadWait)?.Remaining,
            tickDebt = _tickTime, failureVisible = _failureLabel.Visible,
            token = current?.Story.Wait?.Token.Value, failure = PresentationFailure?.Code, failureField = PresentationFailure?.Field, failureKind = PresentationFailure?.Kind.ToString(), spriteSize = current?.Exploration?.SpriteSize,
            dialogue = _dialogue.Text, help = _help.Text, visibleCharacters = _dialogue.VisibleCharacters,
            totalCharacters = _dialogue.GetTotalCharacterCount(), textMode = _input.Settings.TextMode,
            reducedFlash = _input.Settings.ReducedFlash,
            textId = (current?.Story.Wait as DialogueWait)?.Text,
            speaker = (current?.Story.Wait as DialogueWait)?.Speaker?.Value,
            speakerFlags = (current?.Story.Wait as DialogueWait)?.SpeakerFlags,
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
                priority = entity.Priority,
                follower = entity.Follower, spriteRequest = entity.SpriteRequest, spriteReady = entity.SpriteReady, waitingForSprite = entity.WaitingForSprite,
                x = entity.Motion.X, y = entity.Motion.Y,
                targetX = entity.Motion.XDestination, targetY = entity.Motion.YDestination,
                velocityX = entity.Motion.XVelocity, velocityY = entity.Motion.YVelocity,
                travelX = entity.Motion.XTravel, travelY = entity.Motion.YTravel,
                speedY = entity.Motion.YSpeed, accelerationX = entity.Motion.XAcceleration, accelerationY = entity.Motion.YAcceleration,
                layer = entity.Motion.Layer, animationCounter = entity.Motion.AnimationCounter, waitTimer = entity.Motion.WaitTimer,
                moving = entity.Motion.IsMoving, busy = entity.Busy, entity.Visible, actionCursor = entity.ActionCursor, speedX = entity.Motion.XSpeed, flagsA = entity.Motion.FlagsA, flagsB = entity.Motion.FlagsB,
            }),
            observations = _result?.Observations,
            audio = _audio?.ObservePlayback(),
        });
    }
}
