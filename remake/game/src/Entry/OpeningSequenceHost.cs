using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Audio;
using Sf2.Remake.GodotAdapter.Exploration;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class GameRoot
{
    private sealed class OpeningOwner
    {
        internal Task<OpeningSequenceReader.Prepared>? Loading;
        internal GameSession? Session;
        internal ExplorationSessionView? View;
        internal SessionAudio? Audio;
        internal Control Surface = null!;
        internal ColorRect Backdrop = null!;
        internal Button Skip = null!;
        internal Label LoadingLabel = null!;
        internal readonly MenuPointer Pointer = new();
    }
    private OpeningOwner? _opening;
    private bool _openingPointerBarrier;
    private string? _openingResult, _openingError;

    private void BeginOpening()
    {
        string selected = Selected("SF2_PRIVATE_OPENING_CONTENT");
        if (string.IsNullOrWhiteSpace(selected)) { _openingResult = "absent"; ShowEntry(); return; }
        if (!EnsureWindowSupport()) { _windowStartup = BeginOpening; return; }
        var owner = new OpeningOwner();
        _opening = owner;
        owner.Backdrop = new ColorRect { Name = "OpeningBackdrop", Color = Colors.Black,
            MouseFilter = Control.MouseFilterEnum.Ignore };
        owner.Backdrop.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(owner.Backdrop); MoveChild(owner.Backdrop, 0);
        owner.Surface = new Control { Name = "OpeningControls", Theme = UI.StoryTheme.CreateEntry(), MouseFilter = Control.MouseFilterEnum.Ignore };
        owner.Surface.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        owner.LoadingLabel = new Label { Text = GameText.Get("opening.loading"), Position = new(640, 460), Size = new(640, 100),
            HorizontalAlignment = HorizontalAlignment.Center };
        owner.Skip = new Button { Name = "OpeningSkip", Text = GameText.Get("opening.skip", _input!.CompactHint(GameAction.Equipment)),
            Position = new(1510, 32), Size = new(370, 64), Theme = UI.StoryTheme.CreateEntry(),
            FocusMode = Control.FocusModeEnum.None, MouseFilter = Control.MouseFilterEnum.Ignore };
        owner.Surface.AddChild(owner.LoadingLabel); owner.Surface.AddChild(owner.Skip); _hostUi.AddChild(owner.Surface);
        owner.Pointer.Suspend(); DisarmInput();
        owner.Loading = Task.Run(() => OpeningSequenceReader.Read(selected));
        // Cancelled owners have no Godot continuation. Observe faults even after disposal.
        _ = owner.Loading.ContinueWith(task => { _ = task.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
        ApplyHostPause();
    }

    private void RefreshOpeningHints()
    {
        if (_opening is { } owner) owner.Skip.Text = GameText.Get("opening.skip", _input!.CompactHint(GameAction.Equipment));
    }

    private void ProcessOpening()
    {
        if (_openingPointerBarrier && !Godot.Input.IsMouseButtonPressed(MouseButton.Left) &&
            !Godot.Input.IsMouseButtonPressed(MouseButton.Right)) _openingPointerBarrier = false;
        if (_opening is not { } owner) return;
        if (WindowPausesPlayback) return;
        if (GetWindow().HasFocus()) owner.Pointer.ReleaseIfNeutral();
        if (owner.Loading is { IsCompleted: true } loading)
        {
            owner.Loading = null;
            try
            {
                var prepared = loading.GetAwaiter().GetResult();
                if (prepared.Outcome is SessionStartFailed failed) { FinishOpening(owner, "failed", failed.Failure.Code); return; }
                var started = (SessionStarted)prepared.Outcome;
                if (started.Result.Failure is { } failure) { FinishOpening(owner, "failed", failure.Code); return; }
                owner.Session = started.Session;
                GameText.Bind(started.Session.Definition, prepared.Catalog);
                owner.Audio = new SessionAudio(this, started.Session.Definition.Exploration!) { SelectMapMusic = false };
                owner.View = new ExplorationSessionView { Name = "OpeningScene", WorldViewport = new() };
                AddChild(owner.View);
                owner.View.Begin(started.Session, started.Result, _input!, owner.Audio,
                    _ => FinishOpening(owner, "failed", "opening-battle-unavailable"),
                    _ => throw new InvalidOperationException("opening-battle-unavailable"));
                owner.LoadingLabel.Hide(); ApplyHostPause();
            }
            catch (Exception error) when (error is System.IO.IOException or UnauthorizedAccessException or ArgumentException or
                System.Text.Json.JsonException or InvalidOperationException or KeyNotFoundException or FormatException)
            { FinishOpening(owner, "failed", "opening-content-unavailable"); }
        }
        if (_opening != owner) return;
        if (owner.View?.OpeningFailure is not null || owner.Audio?.Error is not null)
        { FinishOpening(owner, "failed", owner.View?.OpeningFailure ?? owner.Audio!.Error); return; }
        if (owner.Session?.Current.Story.Wait is SceneEndWait) FinishOpening(owner, "completed");
    }

    private bool RouteOpeningInput(InputEvent input)
    {
        if (_openingPointerBarrier && input is InputEventMouse) { GetViewport().SetInputAsHandled(); return true; }
        if (_opening is not { } owner) return false;
        if (!GetWindow().HasFocus()) { owner.Pointer.Suspend(); GetViewport().SetInputAsHandled(); return true; }
        if (input is InputEventMouseButton button)
        {
            var rect = owner.Skip.GetGlobalRect();
            string? target = button.ButtonIndex == MouseButton.Left && rect.HasPoint(button.Position) ? "skip" : null;
            if (owner.Pointer.Read(button, target, rect, owner) == "skip") FinishOpening(owner, "skipped");
        }
        else if (input is not InputEventMouse && _input?.Resolve(input) is { } action)
        {
            if (action == GameAction.Equipment) FinishOpening(owner, "skipped");
            else if (action == GameAction.Confirm) owner.View?.HandleAction(action);
        }
        GetViewport().SetInputAsHandled(); return true;
    }

    private void FinishOpening(OpeningOwner owner, string result, string? error = null)
    {
        if (_opening != owner) return;
        _openingResult = result; _openingError = error;
        ReleaseOpening();
        _openingPointerBarrier = true; DisarmInput(); ShowEntry();
        if (error is not null)
        {
            // A bounded visible diagnostic retains the usable title and its ordinary actions.
            var label = new Label { Name = "OpeningDiagnostic", Text = GameText.Get("opening.failed"),
                Position = new(320, 24), Size = new(1280, 80), HorizontalAlignment = HorizontalAlignment.Center,
                MouseFilter = Control.MouseFilterEnum.Ignore };
            _entry!.AddChild(label);
        }
    }

    private void ReleaseOpening()
    {
        var owner = _opening; _opening = null;
        if (owner is null) return;
        owner.Pointer.Suspend();
        if (owner.View is { } view) { view.GetParent()?.RemoveChild(view); view.Free(); }
        owner.Audio?.Dispose();
        GameText.ClearWorld();
        owner.Surface.GetParent()?.RemoveChild(owner.Surface); owner.Surface.Free();
        owner.Backdrop.GetParent()?.RemoveChild(owner.Backdrop); owner.Backdrop.Free();
    }

    public string ReadOpeningObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    {
        active = _opening is not null, loading = _opening?.Loading is not null, result = _openingResult, error = _openingError,
        journey = _session?.Current.SessionId, releaseBarrier = _releaseBarrier, pointerBarrier = _openingPointerBarrier,
        session = _opening?.Session?.Current.SessionId, audio = _opening?.Audio?.ObservePlayback(),
    });
}
