using System.Text.Json;
using System.Text.RegularExpressions;
using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter.Exploration;

public sealed partial class ExplorationSessionView : Control
{
    private GameSession? _session;
    private SessionResult? _result;
    private Action<SessionResult>? _enterBattle;
    private bool _handedOff;
    private Label _title = null!;
    private Label _dialogue = null!;
    private Label _help = null!;
    private double _tickTime;
    private ExplorationState? _lastWorld;
    private ExplorationPresentation? _presentation;
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
        GetViewport().SizeChanged += Present;
    }
    public override void _ExitTree() { GetViewport().SizeChanged -= Present; _presentation?.Dispose(); }

    internal void Begin(GameSession session, SessionResult result, Action<SessionResult> enterBattle)
    {
        _session = session; _result = result; _enterBattle = enterBattle;
        _presentation = new(this, session.Definition.Exploration!);
        TextureFilter = TextureFilterEnum.Nearest;
        Present();
    }

    public override void _Process(double delta)
    {
        if (_handedOff || _session is null || _session.Current.StopReason is SessionStopReason.Unsupported or SessionStopReason.Faulted) return;
        if (_presentation is { } presentation)
        {
            foreach (var completion in presentation.Update(delta, _session.Current))
            {
                Send(completion);
                if (_handedOff || _result?.Failure is not null) return;
            }
            QueueRedraw();
            if (presentation.Error is not null) { Present(); return; }
        }
        if (NeedsTicks(_session.Current))
        {
            const double tickDuration = 1.0 / 60;
            _tickTime += delta;
            int ticks = (int)Math.Min(600, Math.Floor(_tickTime / tickDuration));
            if (ticks > 0)
            {
                long before = _session.Current.Story.SimulationTick;
                Send(new AdvanceSimulation(_session.Current.Story.Wait?.Token, ticks));
                long executed = _session.Current.Story.SimulationTick - before;
                // The engine may yield before consuming the batch. Retain its unused time
                // for automatic continuation, but never carry paused input time into a new wait.
                _tickTime = Math.Max(0, _tickTime - executed * tickDuration);
                if (_handedOff || !NeedsTicks(_session.Current)) _tickTime = 0;
            }
        }
        else _tickTime = 0;
    }

    private static bool NeedsTicks(SessionSnapshot current) =>
        current.StopReason == SessionStopReason.SimulationWait ||
        current.Exploration?.AllEntities.Any(entity => entity.Busy || entity.Follower is not null) == true;

    public override void _UnhandledInput(InputEvent input)
    {
        if (_handedOff || _session is null || input is not InputEventKey { Pressed: true, Echo: false } key) return;
        var current = _session.Current;
        SessionCommand? command = null;
        if (current.Story.Wait is ChoiceWait choice)
            command = key.Keycode switch { Key.Y or Key.Enter => new ChooseDialogue(choice.Token, true),
                Key.N or Key.Escape => new ChooseDialogue(choice.Token, false), _ => null };
        else if (current.Story.Wait is DialogueWait)
        {
            if (key.Keycode is Key.Enter or Key.Space) command = new Acknowledge(current.Story.Wait.Token);
        }
        else if (current.StopReason == SessionStopReason.PlayerInput && current.Exploration is { } world)
        {
            command = key.Keycode switch
            {
                Key.W or Key.Up => new Move(ExplorationDirection.North), Key.D or Key.Right => new Move(ExplorationDirection.East),
                Key.S or Key.Down => new Move(ExplorationDirection.South), Key.A or Key.Left => new Move(ExplorationDirection.West),
                _ => null,
            };
            if (key.Keycode is Key.Enter or Key.C)
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
        GetViewport().SetInputAsHandled();
    }

    private void Send(SessionCommand command)
    {
        var current = _session!.Current;
        _result = _session.Submit(new(current.SessionId, current.Revision, null, command));
        if (_result.Failure is null && _session.Current.HasBattleControl)
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
        _lastWorld = current.Exploration ?? _lastWorld;
        var size = GetViewportRect().Size;
        _title.Text = _session.Definition.Package.Replace('-', ' ').ToUpperInvariant();
        bool portrait = _session.Definition.Exploration?.Visuals is not null && current.Story.TextWindow is OpenTextWindow { Speaker: not null };
        bool rightPortrait = current.Story.TextWindow is OpenTextWindow { SpeakerFlags: var speakerFlags } && (speakerFlags & 0x80) != 0;
        _dialogue.Position = new(portrait && !rightPortrait ? 110 : 20, Mathf.Max(80, size.Y - 140));
        _dialogue.Size = new(Mathf.Max(1, size.X - _dialogue.Position.X - (portrait && rightPortrait ? 110 : 20)), 75);
        _help.Position = new(20, Mathf.Max(130, size.Y - 55));
        _help.Size = new(Mathf.Max(1, size.X - 40), 45);
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
            ChoiceWait => "Y / Enter: Yes     N / Esc: No",
            DialogueWait => "Enter / Space: Continue",
            EntityWait or TickWait or PresentationWait => "",
            _ => "WASD / arrows: Move     Enter / C: Talk",
        };
        if (PresentationFailure is { } failure)
            _dialogue.Text = $"{failure.Message} ({failure.Code})";
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
            map = current?.Exploration?.Map.Value, stop = current?.StopReason.ToString(),
            flags = current?.Story.Flags, simulationTick = current?.Story.SimulationTick, cursor = current?.Story.Cursor, wait = current?.Story.Wait?.GetType().Name,
            token = current?.Story.Wait?.Token.Value, failure = PresentationFailure?.Code, failureField = PresentationFailure?.Field, failureKind = PresentationFailure?.Kind.ToString(), spriteSize = current?.Exploration?.SpriteSize,
            dialogue = _dialogue.Text, help = _help.Text,
            textId = (current?.Story.Wait as DialogueWait)?.Text,
            speaker = (current?.Story.Wait as DialogueWait)?.Speaker?.Value,
            speakerFlags = (current?.Story.Wait as DialogueWait)?.SpeakerFlags,
            partyLists = current?.Story.PartyLists, mainSeed = current?.Exploration?.Party.MainSeed,
            presentation = new { spriteMounts = _presentation?.SpriteMounts, gestureDraws = _presentation?.GestureDraws,
                nodDraws = _presentation?.NodDraws, restoredGestureDraws = _presentation?.RestoredGestureDraws,
                soundStarts = _presentation?.SoundStarts, soundFades = _presentation?.SoundFades,
                paletteFades = _presentation?.PaletteFades, paletteBrightness = _presentation?.PaletteBrightness,
                cameraX = _presentation?.Camera.X, cameraY = _presentation?.Camera.Y, activeCue = _presentation?.ActiveCue,
                error = _presentation?.Error },
            entities = current?.Exploration?.AllEntities.Select(entity => new
            {
                id = entity.Entity.Value, slot = entity.Slot, sprite = entity.Sprite, facing = entity.Motion.Facing,
                priority = entity.Priority,
                follower = entity.Follower, spriteRequest = entity.SpriteRequest, spriteReady = entity.SpriteReady, waitingForSprite = entity.WaitingForSprite,
                x = entity.Motion.X, y = entity.Motion.Y,
                targetX = entity.Motion.XDestination, targetY = entity.Motion.YDestination,
                moving = entity.Motion.IsMoving, busy = entity.Busy, entity.Visible, actionCursor = entity.ActionCursor, speedX = entity.Motion.XSpeed, flagsA = entity.Motion.FlagsA, flagsB = entity.Motion.FlagsB,
            }),
            observations = _result?.Observations,
        });
    }
}
