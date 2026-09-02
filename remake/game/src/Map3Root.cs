using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Content;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class Map3Root : Node2D
{
    internal static readonly Vector2I MinimumProductClientSize = new(960, 540);
    internal static readonly Vector2I ProjectFallbackClientSize = new(960, 540);

    private static readonly Vector2I[] AdaptiveWindowedClientSizeLadder =
    [
        new(1920, 1080),
        new(1600, 900),
        new(1280, 720),
        new(960, 540),
    ];

    public const string BannerText = "PUBLIC SYNTHETIC — NOT ORIGINAL FIDELITY";
    public const string SmokeMarker = "SF2_MAP3_SMOKE ";
    public const string PublicSyntheticBattleSmokeMarker =
        "SF2_MAP3_PUBLIC_SYNTHETIC_BATTLE_SMOKE ";

    private GameSession? _session;
    private Map3Presenter? _presenter;
    private PublicSyntheticBattlePresenter? _battlePresenter;
    private Map3InputAdapter? _inputAdapter;

    public override void _Ready()
    {
        long readyStarted = System.Diagnostics.Stopwatch.GetTimestamp();
        long selectionStarted = System.Diagnostics.Stopwatch.GetTimestamp();
        Map3RuntimeProfileSelection selection =
            Map3RuntimeProfileSelection.Parse(OS.GetCmdlineUserArgs());
        TracePrivateStage(
            selection.PrivateSmokeRequested,
            "profile-selection",
            selectionStarted);
        _runtimeProfile = selection.RequestedProfile;
        _inputAdapter = Map3InputAdapter.CreateGodot(CreatePublicSyntheticInputActions());
        _inputAdapter.EnsureActionsRegistered();
        BuildSelectedPresentation(selection);
        if (!TryBeginPrivateProductDisplayPolicy(
                selection,
                readyStarted,
                out bool displayCompletionDeferred,
                out string? displayDiagnostic))
        {
            FailPrivateStartup(
                displayDiagnostic!,
                selection.PrivateSmokeRequested,
                "private-local");
            return;
        }

        if (displayCompletionDeferred)
        {
            return;
        }

        StartScenario(selection);
        TracePrivateStage(selection.PrivateSmokeRequested, "godot-ready", readyStarted);
    }

    private bool TryBeginPrivateProductDisplayPolicy(
        Map3RuntimeProfileSelection selection,
        long readyStarted,
        out bool completionDeferred,
        out string? diagnostic)
    {
        completionDeferred = false;
        diagnostic = null;
        if (!selection.IsAvailable ||
            selection.RequestedProfile != Map3RuntimeProfile.PrivateLocal)
        {
            return true;
        }

        Window root = GetWindow();
        bool headless = string.Equals(
            DisplayServer.GetName(),
            "headless",
            StringComparison.Ordinal);
        Vector2I clientSize = SelectObservedClientSize(
            headless,
            root.ContentScaleSize,
            DisplayServer.WindowGetSize());
        if (headless)
        {
            return ValidatePrivateProductClientSize(clientSize, out diagnostic);
        }

        string[] processArguments = System.Environment.GetCommandLineArgs();
        if (ShouldPreservePhysicalStartupTarget(root.Mode, clientSize, processArguments))
        {
            if (!ValidatePrivateProductClientSize(clientSize, out diagnostic))
            {
                return false;
            }

            root.MinSize = MinimumProductClientSize;
            return true;
        }

        Rect2I usableRect = DisplayServer.ScreenGetUsableRect(root.CurrentScreen);
        Vector2I decoratedSize = DisplayServer.WindowGetSizeWithDecorations();
        Vector2I clientOrigin = DisplayServer.WindowGetPosition();
        Vector2I decoratedOrigin = DisplayServer.WindowGetPositionWithDecorations();
        (Vector2I ClientOriginInset, Vector2I OppositeInset) decorationInsets =
            DecorationInsets(
                clientSize,
                decoratedSize,
                clientOrigin,
                decoratedOrigin);
        Vector2I decorationDelta = new(
            checked(decorationInsets.ClientOriginInset.X + decorationInsets.OppositeInset.X),
            checked(decorationInsets.ClientOriginInset.Y + decorationInsets.OppositeInset.Y));
        Vector2I? selected = SelectAdaptiveWindowedClientSize(
            usableRect.Size,
            decorationDelta);
        if (selected is not Vector2I target)
        {
            diagnostic =
                "PrivateLocal display unavailable (no supported windowed client fits the usable screen).";
            return false;
        }

        root.Size = target;
        completionDeferred = true;
        Callable.From(() => CompleteAdaptivePrivateProductDisplaySize(
            selection,
            readyStarted,
            target,
            usableRect,
            decorationInsets.ClientOriginInset)).CallDeferred();
        return true;
    }

    private void CompleteAdaptivePrivateProductDisplaySize(
        Map3RuntimeProfileSelection selection,
        long readyStarted,
        Vector2I target,
        Rect2I usableRect,
        Vector2I clientOriginInset)
    {
        Window root = GetWindow();
        Vector2I finalClientSize = DisplayServer.WindowGetSize();
        if (root.Size != target || finalClientSize != target)
        {
            FailPrivateStartup(
                "PrivateLocal display unavailable (the selected client size was not established).",
                selection.PrivateSmokeRequested,
                "private-local");
            return;
        }

        if (!ValidatePrivateProductClientSize(
                finalClientSize,
                out string? displayDiagnostic))
        {
            FailPrivateStartup(
                displayDiagnostic!,
                selection.PrivateSmokeRequested,
                "private-local");
            return;
        }

        Vector2I finalDecoratedSize = DisplayServer.WindowGetSizeWithDecorations();
        if (!CanFitDecoratedWindow(usableRect.Size, finalDecoratedSize))
        {
            FailPrivateStartup(
                "PrivateLocal display unavailable (the actual decorated window does not fit the usable screen).",
                selection.PrivateSmokeRequested,
                "private-local");
            return;
        }

        Vector2I desiredDecoratedOrigin = CenterDecoratedWindow(
            usableRect,
            finalDecoratedSize);
        DisplayServer.WindowSetPosition(desiredDecoratedOrigin + clientOriginInset);
        Callable.From(() => CompleteAdaptivePrivateProductDisplayPosition(
            selection,
            readyStarted,
            target,
            usableRect,
            clientOriginInset)).CallDeferred();
    }

    private void CompleteAdaptivePrivateProductDisplayPosition(
        Map3RuntimeProfileSelection selection,
        long readyStarted,
        Vector2I target,
        Rect2I usableRect,
        Vector2I clientOriginInset)
    {
        Window root = GetWindow();
        Vector2I finalClientSize = DisplayServer.WindowGetSize();
        Vector2I finalDecoratedSize = DisplayServer.WindowGetSizeWithDecorations();
        Vector2I finalClientOrigin = DisplayServer.WindowGetPosition();
        Vector2I finalDecoratedOrigin = DisplayServer.WindowGetPositionWithDecorations();
        if (root.Size != target || finalClientSize != target ||
            !CanFitDecoratedWindow(usableRect.Size, finalDecoratedSize))
        {
            FailPrivateStartup(
                "PrivateLocal display unavailable (the final window geometry drifted).",
                selection.PrivateSmokeRequested,
                "private-local");
            return;
        }

        Vector2I expectedDecoratedOrigin = CenterDecoratedWindow(
            usableRect,
            finalDecoratedSize);
        if (finalDecoratedOrigin != expectedDecoratedOrigin ||
            finalClientOrigin != expectedDecoratedOrigin + clientOriginInset ||
            !IsDecoratedFrameInside(
                usableRect,
                finalDecoratedOrigin,
                finalDecoratedSize))
        {
            FailPrivateStartup(
                "PrivateLocal display unavailable (the window manager did not establish the centered usable frame).",
                selection.PrivateSmokeRequested,
                "private-local");
            return;
        }

        root.MinSize = MinimumProductClientSize;
        StartScenario(selection);
        TracePrivateStage(selection.PrivateSmokeRequested, "godot-ready", readyStarted);
    }

    internal static bool HasExplicitPhysicalStartupArgument(
        IReadOnlyList<string> processArguments)
    {
        ArgumentNullException.ThrowIfNull(processArguments);
        for (int index = 0; index < processArguments.Count; index++)
        {
            string? argument = processArguments[index];
            if (string.Equals(argument, "--", StringComparison.Ordinal))
            {
                return false;
            }

            if (string.Equals(argument, "--resolution", StringComparison.Ordinal) ||
                argument.StartsWith("--resolution=", StringComparison.Ordinal) ||
                string.Equals(argument, "--fullscreen", StringComparison.Ordinal) ||
                string.Equals(argument, "--maximized", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    internal static bool ShouldPreservePhysicalStartupTarget(
        Window.ModeEnum mode,
        Vector2I currentClientSize,
        IReadOnlyList<string> processArguments) =>
        mode != Window.ModeEnum.Windowed ||
        currentClientSize != ProjectFallbackClientSize ||
        HasExplicitPhysicalStartupArgument(processArguments);

    internal static Vector2I SelectObservedClientSize(
        bool headless,
        Vector2I virtualContentSize,
        Vector2I displayServerSize) =>
        headless ? virtualContentSize : displayServerSize;

    internal static (Vector2I ClientOriginInset, Vector2I OppositeInset) DecorationInsets(
        Vector2I clientSize,
        Vector2I decoratedSize,
        Vector2I clientOrigin,
        Vector2I decoratedOrigin)
    {
        Vector2I total = new(
            Math.Max(0, decoratedSize.X - clientSize.X),
            Math.Max(0, decoratedSize.Y - clientSize.Y));
        Vector2I clientOriginInset = new(
            Math.Clamp(clientOrigin.X - decoratedOrigin.X, 0, total.X),
            Math.Clamp(clientOrigin.Y - decoratedOrigin.Y, 0, total.Y));
        return (
            clientOriginInset,
            new Vector2I(
                total.X - clientOriginInset.X,
                total.Y - clientOriginInset.Y));
    }

    internal static bool CanFitDecoratedWindow(
        Vector2I usableSize,
        Vector2I decoratedSize) =>
        usableSize.X > 0 && usableSize.Y > 0 &&
        decoratedSize.X > 0 && decoratedSize.Y > 0 &&
        decoratedSize.X <= usableSize.X && decoratedSize.Y <= usableSize.Y;

    internal static Vector2I? SelectAdaptiveWindowedClientSize(
        Vector2I usableSize,
        Vector2I decorationDelta)
    {
        if (usableSize.X <= 0 || usableSize.Y <= 0 ||
            decorationDelta.X < 0 || decorationDelta.Y < 0)
        {
            return null;
        }

        foreach (Vector2I candidate in AdaptiveWindowedClientSizeLadder)
        {
            long decoratedWidth = (long)candidate.X + decorationDelta.X;
            long decoratedHeight = (long)candidate.Y + decorationDelta.Y;
            if (decoratedWidth <= usableSize.X && decoratedHeight <= usableSize.Y)
            {
                return candidate;
            }
        }

        return null;
    }

    internal static Vector2I CenterDecoratedWindow(
        Rect2I usableRect,
        Vector2I decoratedSize)
    {
        if (!CanFitDecoratedWindow(usableRect.Size, decoratedSize))
        {
            throw new ArgumentOutOfRangeException(nameof(decoratedSize));
        }

        return new Vector2I(
            usableRect.Position.X + ((usableRect.Size.X - decoratedSize.X) / 2),
            usableRect.Position.Y + ((usableRect.Size.Y - decoratedSize.Y) / 2));
    }

    internal static bool IsDecoratedFrameInside(
        Rect2I usableRect,
        Vector2I decoratedOrigin,
        Vector2I decoratedSize)
    {
        if (!CanFitDecoratedWindow(usableRect.Size, decoratedSize))
        {
            return false;
        }

        long right = (long)decoratedOrigin.X + decoratedSize.X;
        long bottom = (long)decoratedOrigin.Y + decoratedSize.Y;
        long usableRight = (long)usableRect.Position.X + usableRect.Size.X;
        long usableBottom = (long)usableRect.Position.Y + usableRect.Size.Y;
        return decoratedOrigin.X >= usableRect.Position.X &&
            decoratedOrigin.Y >= usableRect.Position.Y &&
            right <= usableRight && bottom <= usableBottom;
    }

    internal static bool ValidatePrivateProductClientSize(
        Vector2I clientSize,
        out string? diagnostic)
    {
        if (clientSize.X >= MinimumProductClientSize.X &&
            clientSize.Y >= MinimumProductClientSize.Y)
        {
            diagnostic = null;
            return true;
        }

        diagnostic =
            "PrivateLocal display unavailable (client area is below the 960-by-540 product minimum).";
        return false;
    }

    public override void _Process(double delta)
    {
        _ = delta;
        if (_session is null)
        {
            return;
        }

        if (_runtimeProfile == Map3RuntimeProfile.PrivateLocal)
        {
            if (_privateBattleBridgeEnabled)
            {
                _inputAdapter?.PollPublicSynthetic();
                return;
            }

            ExplorationDirection? direction =
                _inputAdapter?.PollPrivateOriginalMapMovement();
            if (direction is ExplorationDirection privateMovement)
            {
                ApplyPrivateMove(privateMovement);
            }

            return;
        }

        _inputAdapter?.PollPublicSynthetic();
    }

    private Map3InputActions CreatePublicSyntheticInputActions() =>
        new(
            ApplyMove,
            ApplyContextSelection,
            ApplyEventRequest,
            ApplyEventRequestAcknowledgement,
            ApplyLocalTransitionRequest,
            ApplyLocalTransitionAcknowledgement,
            ApplyTurn,
            ApplyEntityInteractionRequest,
            ApplyEntityInteractionAcknowledgement,
            ApplyDialogueAdvance,
            ApplyFieldSearchRequest,
            ApplyFieldSearchAcknowledgement,
            ApplyItemAcquisitionRequest,
            ApplyItemAcquisitionAcknowledgement,
            ApplyOutboundTransitionRequest,
            ApplyOutboundTransitionAcknowledgement,
            ApplyPublicSyntheticBattleRequest,
            ApplyPublicSyntheticBattleEntryAcknowledgement,
            ApplyPublicSyntheticBattleCursorMove,
            ApplyPublicSyntheticBattleSelectionConfirmation,
            ApplyPublicSyntheticBattleSelectionCancellation,
            ApplyPublicSyntheticBattleCompletionAcknowledgement);

    private void StartScenario(Map3RuntimeProfileSelection selection)
    {
        if (!selection.IsAvailable)
        {
            FailProfileStartup(selection);
            return;
        }

        if (selection.RequestedProfile == Map3RuntimeProfile.PrivateLocal)
        {
            StartPrivateScenario(selection);
            return;
        }

        byte[] packageBytes = Godot.FileAccess.GetFileAsBytes(
            "res://content/public-synthetic-map3-smoke-v1.json");
        PublicSyntheticMap3PackageReader source =
            PublicSyntheticMap3PackageReader.FromDocumentBytes(packageBytes);
        GameSessionStartResult result = GameSession.Start(
            source,
            new MapScenarioRequest(
                PublicSyntheticMap3PackageReader.PackageId,
                ContentProfile.PublicSynthetic));
        if (result is not GameSessionStarted started)
        {
            GameSessionStartRejected rejected = (GameSessionStartRejected)result;
            FailStartup(rejected.Diagnostic.Message);
            return;
        }

        _session = started.Session;
        ProjectSnapshot("Ready");
        if (OS.GetCmdlineUserArgs().Contains("--map3-smoke", StringComparer.Ordinal))
        {
            SceneTree sceneTree = GetTree();
            Map3Presenter presenter = _presenter!;
            PublicSyntheticBattlePresenter battlePresenter = _battlePresenter!;
            Callable.From(() => PublicSyntheticMap3SmokeDriver.Run(
                sceneTree,
                started.Session,
                started.Receipt,
                presenter,
                battlePresenter)).CallDeferred();
        }
    }

    private void ApplyMove(ExplorationDirection direction)
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(new MoveExplorationCommand(direction));
        if (result is GameSessionCommandApplied applied)
        {
            ProjectSnapshot(applied.Outcome.ToString());
            return;
        }

        GameSessionCommandRejected rejected = (GameSessionCommandRejected)result;
        ProjectRejection(rejected);
    }

    private void ApplyContextSelection()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary));
        if (result is GameSessionContextSelected selected)
        {
            ProjectSnapshot("Context selected");
            return;
        }

        GameSessionCommandRejected rejected = (GameSessionCommandRejected)result;
        ProjectRejection(rejected);
    }

    private void ApplyEventRequest()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(new RequestSelectedZoneEventCommand());
        if (result is GameSessionEventRequested)
        {
            ProjectSnapshot("Event request pending");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyEventRequestAcknowledgement()
    {
        if (_session?.Snapshot.EventRequest is not MapEventRequestSnapshot request)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AcknowledgeMapEventRequestCommand(
                request.Request,
                request.CueSequence,
                request.ExpectedEffect));
        if (result is GameSessionEventEffectApplied)
        {
            ProjectSnapshot("Synthetic effect applied; re-select context");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyLocalTransitionRequest()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new RequestSelectedLocalTransitionCommand());
        if (result is GameSessionLocalTransitionRequested)
        {
            ProjectSnapshot("Local transition pending");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyLocalTransitionAcknowledgement()
    {
        if (_session?.Snapshot.LocalTransition is not MapLocalTransitionSnapshot transition)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AcknowledgeMapLocalTransitionCommand(
                transition.Request,
                transition.CueSequence,
                transition.Transition));
        if (result is GameSessionLocalTransitionApplied)
        {
            ProjectSnapshot("Synthetic local transition applied");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyTurn(SemanticFacing facing)
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(new TurnExplorationCommand(facing));
        if (result is GameSessionFacingChanged)
        {
            ProjectSnapshot($"Facing {facing}");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyEntityInteractionRequest()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(new RequestEntityInteractionCommand());
        if (result is GameSessionEntityInteractionRequested)
        {
            ProjectSnapshot("Placeholder entity interaction pending");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyEntityInteractionAcknowledgement()
    {
        if (_session?.Snapshot.EntityInteraction is not MapEntityInteractionSnapshot interaction)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AcknowledgeEntityInteractionCommand(
                interaction.Request,
                interaction.CueSequence,
                interaction.Entity,
                interaction.Target));
        if (result is GameSessionEntityInteractionAcknowledged acknowledged)
        {
            ProjectSnapshot(
                $"Placeholder interaction acknowledged; dialogue {acknowledged.Dialogue.Dialogue} opened");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyDialogueAdvance()
    {
        if (_session?.Snapshot.Dialogue is not MapDialogueSnapshot
            {
                Status: MapDialogueStatus.Open,
                CurrentLine: not null,
            } dialogue)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AdvanceDialogueCommand(
                dialogue.Dialogue,
                dialogue.CueSequence,
                dialogue.CurrentLine.Line));
        switch (result)
        {
            case GameSessionDialogueAdvanced advanced:
                ProjectSnapshot(
                    $"Placeholder dialogue advanced to line {advanced.Dialogue.CurrentLineIndex + 1}");
                return;
            case GameSessionDialogueClosed:
                ProjectSnapshot("Placeholder dialogue closed");
                return;
            default:
                ProjectRejection((GameSessionCommandRejected)result);
                return;
        }
    }

    private void ApplyFieldSearchRequest()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(new RequestFieldSearchCommand());
        if (result is GameSessionFieldSearchRequested requested)
        {
            ProjectSnapshot($"Synthetic field search {requested.Search.Context} pending");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyFieldSearchAcknowledgement()
    {
        if (_session?.Snapshot.FieldSearch is not MapFieldSearchSnapshot
            {
                Status: MapFieldSearchStatus.Pending,
            } search)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AcknowledgeFieldSearchCommand(
                search.Request,
                search.RequestCueSequence,
                search.Result));
        if (result is GameSessionFieldSearchDiscovered discovered)
        {
            ProjectSnapshot($"Placeholder discovery {discovered.Receipt.Discovery} admitted");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyItemAcquisitionRequest()
    {
        if (_session?.Snapshot.FieldSearch is not MapFieldSearchSnapshot
            {
                Status: MapFieldSearchStatus.Discovered,
            } search)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new RequestMapItemAcquisitionCommand(search.Discovery));
        if (result is GameSessionItemAcquisitionRequested requested)
        {
            ProjectSnapshot($"Placeholder item {requested.Acquisition.Item} pending acquisition");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyItemAcquisitionAcknowledgement()
    {
        if (_session?.Snapshot.ItemAcquisition is not MapItemAcquisitionSnapshot
            {
                Status: MapItemAcquisitionStatus.Pending,
            } acquisition)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AcknowledgeMapItemAcquisitionCommand(
                acquisition.Request,
                acquisition.RequestCueSequence,
                acquisition.Result,
                acquisition.Item));
        if (result is GameSessionItemAcquired acquired)
        {
            ProjectSnapshot($"Placeholder item {acquired.Receipt.Item} acquired once");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyOutboundTransitionRequest()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new RequestSelectedOutboundTransitionCommand());
        if (result is GameSessionOutboundTransitionRequested)
        {
            ProjectSnapshot("Outbound transition pending");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyOutboundTransitionAcknowledgement()
    {
        if (_session?.Snapshot.OutboundTransition is not MapOutboundTransitionSnapshot transition)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AcknowledgeMapOutboundTransitionCommand(
                transition.Request,
                transition.CueSequence,
                transition.Transition));
        if (result is GameSessionOutboundTransitionApplied)
        {
            ProjectSnapshot("Public-synthetic outbound transition applied");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyPublicSyntheticBattleRequest()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new RequestSelectedPublicSyntheticBattleCommand());
        if (result is GameSessionPublicSyntheticBattleRequested)
        {
            ProjectSnapshot("Public-synthetic battle entry pending", result);
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyPublicSyntheticBattleEntryAcknowledgement()
    {
        if (_session?.Snapshot.PublicSyntheticBattle is not
            PublicSyntheticBattleLifecycleSnapshot battle)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AcknowledgePublicSyntheticBattleEntryCommand(
                battle.Definition.Request,
                battle.Definition.Rules.Battle,
                battle.EntryCueSequence));
        if (result is GameSessionPublicSyntheticBattleAdmitted)
        {
            ProjectSnapshot("Project-authored tactical battle admitted", result);
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyPublicSyntheticBattleCursorMove(TacticalDirection direction)
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new MovePublicSyntheticBattleCursorCommand(direction));
        if (result is GameSessionPublicSyntheticBattleCursorMoved moved)
        {
            ProjectSnapshot($"Tactical cursor {moved.Outcome}", result);
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyPublicSyntheticBattleSelectionConfirmation()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new ConfirmPublicSyntheticBattleSelectionCommand());
        if (result is GameSessionPublicSyntheticBattleSelectionConfirmed confirmed)
        {
            ProjectSnapshot($"Tactical selection {confirmed.Outcome}", result);
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyPublicSyntheticBattleSelectionCancellation()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new CancelPublicSyntheticBattleSelectionCommand());
        if (result is GameSessionPublicSyntheticBattleSelectionCancelled cancelled)
        {
            ProjectSnapshot($"Tactical selection {cancelled.Outcome}", result);
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyPublicSyntheticBattleCompletionAcknowledgement()
    {
        if (_session?.Snapshot.PublicSyntheticBattle is not
            PublicSyntheticBattleLifecycleSnapshot
            {
                Status: PublicSyntheticBattleLifecycleStatus.Completed,
            } battle)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AcknowledgePublicSyntheticBattleCompletionCommand(
                battle.Definition.Rules.Battle,
                battle.LastCueSequence));
        if (result is GameSessionPublicSyntheticBattleReturned)
        {
            ProjectSnapshot("Public-synthetic battle completed; exploration restored", result);
            return;
        }

        if (result is GameSessionPublicSyntheticBattleRestarted)
        {
            ProjectSnapshot("Public-synthetic battle defeated; retry started", result);
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ProjectRejection(GameSessionCommandRejected rejected)
    {
        _presenter?.ProjectStatus(rejected.Diagnostic.Message);
        ProjectSnapshot(rejected.Diagnostic.Message, rejected);
    }

    private void ProjectSnapshot(
        string outcome,
        GameSessionCommandResult? result = null)
    {
        if (_session is null)
        {
            return;
        }

        _presenter?.Project(_session.Snapshot, outcome);
        _battlePresenter?.Project(_session.Snapshot, outcome, result);
    }

    private void FailStartup(string message)
    {
        GD.PushError(message);
        _presenter?.ProjectStatus(message);

        if (OS.GetCmdlineUserArgs().Contains("--map3-smoke", StringComparer.Ordinal))
        {
            GD.Print(SmokeMarker + JsonSerializer.Serialize(new { status = "Fail", message }));
            GetTree().Quit(1);
        }
    }

    private void BuildPresentation()
    {
        _presenter = Map3Presenter.Attach(this);
        _battlePresenter = PublicSyntheticBattlePresenter.Attach(this);
    }
}
