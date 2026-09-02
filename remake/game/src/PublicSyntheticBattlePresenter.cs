using System.Collections.ObjectModel;
using Godot;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.GodotAdapter;

internal sealed record PublicSyntheticBattleCellProjection(
    TacticalPosition Position,
    bool HasActor,
    bool HasEnemy,
    bool HasCursor);

internal sealed record PublicSyntheticBattlePresentationProjection
{
    private PublicSyntheticBattlePresentationProjection(
        bool visible,
        string title,
        string instruction,
        string status,
        string cueStatus,
        string legend,
        int actorHitPoints,
        int actorMaxHitPoints,
        int enemyHitPoints,
        int enemyMaxHitPoints,
        IEnumerable<PublicSyntheticBattleCellProjection> cells)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(instruction);
        ArgumentException.ThrowIfNullOrWhiteSpace(status);
        ArgumentException.ThrowIfNullOrWhiteSpace(cueStatus);
        ArgumentException.ThrowIfNullOrWhiteSpace(legend);
        ArgumentNullException.ThrowIfNull(cells);
        Visible = visible;
        Title = title;
        Instruction = instruction;
        Status = status;
        CueStatus = cueStatus;
        Legend = legend;
        ActorHitPoints = actorHitPoints;
        ActorMaxHitPoints = actorMaxHitPoints;
        EnemyHitPoints = enemyHitPoints;
        EnemyMaxHitPoints = enemyMaxHitPoints;
        Cells = new ReadOnlyCollection<PublicSyntheticBattleCellProjection>(
            [.. cells]);
    }

    internal bool Visible { get; }

    internal string Title { get; }

    internal string Instruction { get; }

    internal string Status { get; }

    internal string CueStatus { get; }

    internal string Legend { get; }

    internal int ActorHitPoints { get; }

    internal int ActorMaxHitPoints { get; }

    internal int EnemyHitPoints { get; }

    internal int EnemyMaxHitPoints { get; }

    internal IReadOnlyList<PublicSyntheticBattleCellProjection> Cells { get; }

    internal static PublicSyntheticBattlePresentationProjection Create(
        GameSessionSnapshot snapshot,
        string outcome,
        GameSessionCommandResult? result = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(outcome);
        PublicSyntheticBattleLifecycleSnapshot? lifecycle = snapshot.PublicSyntheticBattle;
        if (lifecycle?.BattleState is TacticalBattleState battleState)
        {
            return Active(
                battleState,
                lifecycle.Definition.Rules.Battle,
                outcome,
                FormatCue(result, lifecycle.LastCueSequence));
        }

        if (result is GameSessionPublicSyntheticBattleReturned returned)
        {
            return new PublicSyntheticBattlePresentationProjection(
                visible: true,
                "PROJECT-AUTHORED PUBLIC-SYNTHETIC TACTICAL MICRO-BATTLE",
                "RETURNED · exploration resumed",
                $"{returned.Completion.Battle} completed; returned to " +
                    $"{returned.Snapshot.Exploration.Map} exploration; " +
                    $"applied {returned.WorldEffect.Effect} / " +
                    $"{returned.WorldEffect.Flag}.",
                $"Cue #{returned.Cue.Sequence} {returned.Cue.Cue}",
                "A actor · E enemy · ▣ cursor",
                0,
                0,
                0,
                0,
                []);
        }

        return new PublicSyntheticBattlePresentationProjection(
            visible: false,
            "PROJECT-AUTHORED PUBLIC-SYNTHETIC TACTICAL MICRO-BATTLE",
            "B request battle · N acknowledge entry",
            "No public-synthetic battle is active.",
            "Battle cue: none.",
            "A actor · E enemy · ▣ cursor",
            0,
            0,
            0,
            0,
            []);
    }

    internal static PublicSyntheticBattlePresentationProjection Create(
        PrivateOriginalMapBattleBridgeSnapshot? bridge,
        string outcome,
        GameSessionCommandResult? result = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outcome);
        if (bridge?.BattleState is TacticalBattleState battleState)
        {
            return Active(
                battleState,
                bridge.Definition.Rules.Battle,
                outcome,
                FormatCue(result, bridge.LastCueSequence));
        }

        if (result is PrivateOriginalMapBattleBridgeReturned returned)
        {
            return new PublicSyntheticBattlePresentationProjection(
                visible: true,
                "PROJECT-AUTHORED PUBLIC-SYNTHETIC TACTICAL MICRO-BATTLE",
                "RETURNED · private traversal resumed",
                $"{returned.Completion.Battle} completed; returned to the same " +
                    $"private {returned.Snapshot.Map} traversal state.",
                $"Cue #{returned.Cue.Sequence} {returned.Cue.Cue}",
                "A actor · E enemy · ▣ cursor",
                0,
                0,
                0,
                0,
                []);
        }

        return new PublicSyntheticBattlePresentationProjection(
            visible: false,
            "PROJECT-AUTHORED PUBLIC-SYNTHETIC TACTICAL MICRO-BATTLE",
            "B request battle · N acknowledge entry",
            "No private battle bridge is active.",
            "Battle cue: none.",
            "A actor · E enemy · ▣ cursor",
            0,
            0,
            0,
            0,
            []);
    }

    private static PublicSyntheticBattlePresentationProjection Active(
        TacticalBattleState battleState,
        TacticalBattleId battle,
        string outcome,
        string cueStatus)
    {
        List<PublicSyntheticBattleCellProjection> cells = [];
        for (int y = 0; y < battleState.Rules.Grid.Height; y++)
        {
            for (int x = 0; x < battleState.Rules.Grid.Width; x++)
            {
                TacticalPosition position = new(x, y);
                cells.Add(new PublicSyntheticBattleCellProjection(
                    position,
                    position == battleState.ActorPosition,
                    position == battleState.EnemyPosition && battleState.EnemyHitPoints > 0,
                    position == battleState.CursorPosition));
            }
        }

        return new PublicSyntheticBattlePresentationProjection(
            visible: true,
            "PROJECT-AUTHORED PUBLIC-SYNTHETIC TACTICAL MICRO-BATTLE",
            InstructionFor(battleState),
            $"{battle}  {battleState.Phase}  " +
                $"Actor HP {battleState.ActorHitPoints}/{battleState.Rules.ActorMaxHitPoints}  " +
                $"Enemy HP {battleState.EnemyHitPoints}/{battleState.Rules.EnemyMaxHitPoints}  " +
                $"Outcome {battleState.Outcome}  {outcome}",
            cueStatus,
            "A actor · E enemy · ▣ cursor · · open cell",
            battleState.ActorHitPoints,
            battleState.Rules.ActorMaxHitPoints,
            battleState.EnemyHitPoints,
            battleState.Rules.EnemyMaxHitPoints,
            cells);
    }

    private static string InstructionFor(TacticalBattleState state) => state.Phase switch
    {
        TacticalBattlePhase.MoveSelection =>
            "MOVE · I/J/K/L cursor · Space confirm destination",
        TacticalBattlePhase.TargetSelection =>
            "TARGET · I/J/K/L cursor · Space attack · Backspace cancel",
        TacticalBattlePhase.Completed when state.Outcome == TacticalBattleOutcome.Victory =>
            "VICTORY · M return to exploration",
        TacticalBattlePhase.Completed when state.Outcome == TacticalBattleOutcome.Defeat =>
            "DEFEAT · M retry battle",
        _ => "Battle state unavailable",
    };

    private static string FormatCue(GameSessionCommandResult? result, long lastCueSequence) =>
        result switch
        {
            GameSessionPublicSyntheticBattleRequested requested =>
                $"Cue #{requested.Cue.Sequence} {requested.Cue.Cue}",
            GameSessionPublicSyntheticBattleAdmitted admitted =>
                $"Cue #{admitted.Cue.Sequence} {admitted.Cue.Cue}",
            GameSessionPublicSyntheticBattleSelectionConfirmed confirmed
                when confirmed.Cues.Count > 0 =>
                FormatConfirmation(confirmed.Cues, confirmed.EnemyResponse),
            GameSessionPublicSyntheticBattleRestarted restarted =>
                $"Cue #{restarted.Cue.Sequence} {restarted.Cue.Cue}",
            PrivateOriginalMapBattleBridgeRequested requested =>
                $"Cue #{requested.Cue.Sequence} {requested.Cue.Cue}",
            PrivateOriginalMapBattleBridgeAdmitted admitted =>
                $"Cue #{admitted.Cue.Sequence} {admitted.Cue.Cue}",
            PrivateOriginalMapBattleBridgeSelectionConfirmed confirmed
                when confirmed.Cues.Count > 0 =>
                FormatConfirmation(confirmed.Cues, confirmed.EnemyResponse),
            PrivateOriginalMapBattleBridgeRestarted restarted =>
                $"Cue #{restarted.Cue.Sequence} {restarted.Cue.Cue}",
            _ => $"Last battle cue sequence #{lastCueSequence}",
        };

    private static string FormatConfirmation(
        IReadOnlyList<PublicSyntheticBattleCue> cues,
        TacticalEnemyResponse? enemyResponse)
    {
        string cueText = "Cues " + string.Join(
            ", ",
            cues.Select(cue => $"#{cue.Sequence} {cue.Cue}"));
        return enemyResponse is null
            ? cueText
            : cueText + $" · Enemy {enemyResponse.Kind} " +
                $"({enemyResponse.EnemyPositionBefore.X},{enemyResponse.EnemyPositionBefore.Y})" +
                $"→({enemyResponse.EnemyPositionAfter.X},{enemyResponse.EnemyPositionAfter.Y}); " +
                $"Actor HP {enemyResponse.ActorHitPointsBefore}" +
                $"→{enemyResponse.ActorHitPointsAfter}";
    }
}

internal sealed class PublicSyntheticBattlePresenter
{
    internal static readonly Vector2 CanvasSize = new(960, 540);
    internal static readonly Rect2 PanelBounds = new(600, 82, 340, 442);
    internal static readonly Rect2 TitleBounds = new(14, 10, 312, 64);
    internal static readonly Rect2 InstructionBounds = new(14, 76, 312, 42);
    internal static readonly Rect2 StatusBounds = new(14, 120, 312, 58);
    internal static readonly Rect2 CueStatusBounds = new(14, 180, 312, 48);
    internal static readonly Rect2 ActorHealthLabelBounds = new(14, 230, 148, 20);
    internal static readonly Rect2 EnemyHealthLabelBounds = new(176, 230, 150, 20);
    internal static readonly Rect2 ActorHealthTrackBounds = new(14, 252, 148, 12);
    internal static readonly Rect2 EnemyHealthTrackBounds = new(176, 252, 148, 12);
    internal static readonly Rect2 TacticalGridBounds = new(14, 286, 190, 124);
    internal static readonly Rect2 LegendBounds = new(224, 286, 102, 92);
    internal static readonly Vector2 TacticalCellSize = new(58, 58);
    internal static readonly Vector2 TacticalCellStride = new(66, 66);
    internal static readonly Vector2 TacticalCellLabelOffset = new(8, 12);
    internal static readonly Vector2 TacticalCellLabelSize = new(42, 32);

    private readonly Control _panel;
    private readonly Label _title;
    private readonly Label _instruction;
    private readonly Label _status;
    private readonly Label _cueStatus;
    private readonly Label _legend;
    private readonly Label _actorHealthLabel;
    private readonly ColorRect _actorHealthFill;
    private readonly Label _enemyHealthLabel;
    private readonly ColorRect _enemyHealthFill;
    private readonly IReadOnlyList<ColorRect> _cells;
    private readonly IReadOnlyList<Label> _cellLabels;
    private ImageTexture? _privateTacticalCursorTexture;
    private TextureRect? _privateTacticalCursorOverlay;

    private PublicSyntheticBattlePresenter(
        Control panel,
        Label title,
        Label instruction,
        Label status,
        Label cueStatus,
        Label legend,
        Label actorHealthLabel,
        ColorRect actorHealthFill,
        Label enemyHealthLabel,
        ColorRect enemyHealthFill,
        IReadOnlyList<ColorRect> cells,
        IReadOnlyList<Label> cellLabels)
    {
        _panel = panel;
        _title = title;
        _instruction = instruction;
        _status = status;
        _cueStatus = cueStatus;
        _legend = legend;
        _actorHealthLabel = actorHealthLabel;
        _actorHealthFill = actorHealthFill;
        _enemyHealthLabel = enemyHealthLabel;
        _enemyHealthFill = enemyHealthFill;
        _cells = cells;
        _cellLabels = cellLabels;
    }

    internal static PublicSyntheticBattlePresenter Attach(Node2D parent)
    {
        ArgumentNullException.ThrowIfNull(parent);
        Control panel = new()
        {
            Position = PanelBounds.Position,
            Size = PanelBounds.Size,
            Visible = false,
        };
        parent.AddChild(panel);
        ColorRect background = new()
        {
            Color = new Color("182034e8"),
            Size = panel.Size,
        };
        panel.AddChild(background);
        Label title = LabelAt(
            panel, TitleBounds.Position, TitleBounds.Size, 16, new Color("ffbd59"));
        Label instruction = LabelAt(
            panel, InstructionBounds.Position, InstructionBounds.Size, 13, new Color("b8f2c2"));
        Label status = LabelAt(
            panel, StatusBounds.Position, StatusBounds.Size, 11, Colors.White);
        Label cueStatus = LabelAt(
            panel, CueStatusBounds.Position, CueStatusBounds.Size, 10, new Color("c6e5ff"));

        Label actorHealthLabel = LabelAt(
            panel,
            ActorHealthLabelBounds.Position,
            ActorHealthLabelBounds.Size,
            11,
            new Color("75c7ff"));
        ColorRect actorHealthTrack = HealthTrack(panel, ActorHealthTrackBounds);
        ColorRect actorHealthFill = HealthFill(actorHealthTrack, new Color("75c7ff"));
        Label enemyHealthLabel = LabelAt(
            panel,
            EnemyHealthLabelBounds.Position,
            EnemyHealthLabelBounds.Size,
            11,
            new Color("ff7d7d"));
        ColorRect enemyHealthTrack = HealthTrack(panel, EnemyHealthTrackBounds);
        ColorRect enemyHealthFill = HealthFill(enemyHealthTrack, new Color("ff7d7d"));

        Label legend = LabelAt(
            panel, LegendBounds.Position, LegendBounds.Size, 11, new Color("ffe2a8"));

        List<ColorRect> cells = [];
        List<Label> cellLabels = [];
        for (int index = 0; index < 6; index++)
        {
            int x = index % 3;
            int y = index / 3;
            ColorRect cell = new()
            {
                Position = TacticalGridBounds.Position +
                    new Vector2(x * TacticalCellStride.X, y * TacticalCellStride.Y),
                Size = TacticalCellSize,
                Color = new Color("31415f"),
            };
            panel.AddChild(cell);
            Label marker = LabelAt(
                panel,
                cell.Position + TacticalCellLabelOffset,
                TacticalCellLabelSize,
                20,
                Colors.White);
            marker.HorizontalAlignment = HorizontalAlignment.Center;
            cells.Add(cell);
            cellLabels.Add(marker);
        }

        return new PublicSyntheticBattlePresenter(
            panel,
            title,
            instruction,
            status,
            cueStatus,
            legend,
            actorHealthLabel,
            actorHealthFill,
            enemyHealthLabel,
            enemyHealthFill,
            cells.AsReadOnly(),
            cellLabels.AsReadOnly());
    }

    internal bool TryAttachPrivateTacticalCursor(
        PrivateLocalPresentationRasterMount mount,
        out PrivateLocalPresentationAssetMountDiagnostic? diagnostic)
    {
        ArgumentNullException.ThrowIfNull(mount);
        diagnostic = null;
        if (_privateTacticalCursorOverlay is not null ||
            !string.Equals(
                mount.Definition.AssetId,
                PrivateLocalPresentationAssetCatalog.TacticalCursorAssetId,
                StringComparison.Ordinal) ||
            mount.Definition.LogicalSize.Width !=
                PrivateLocalPresentationAssetCatalog.TacticalCursorLogicalWidth ||
            mount.Definition.LogicalSize.Height !=
                PrivateLocalPresentationAssetCatalog.TacticalCursorLogicalHeight ||
            mount.Bucket.Width != checked(
                PrivateLocalPresentationAssetCatalog.TacticalCursorLogicalWidth *
                mount.Bucket.Scale) ||
            mount.Bucket.Height != checked(
                PrivateLocalPresentationAssetCatalog.TacticalCursorLogicalHeight *
                mount.Bucket.Scale))
        {
            diagnostic = new PrivateLocalPresentationAssetMountDiagnostic(
                PrivateLocalPresentationAssetMountFailureCode.InvalidBinding,
                "The admitted private tactical cursor binding is incompatible with the battle grid.");
            return false;
        }

        Image image = new();
        Error error = image.LoadPngFromBuffer(mount.CopyPngBytes());
        if (error != Error.Ok ||
            image.GetWidth() != mount.Bucket.Width ||
            image.GetHeight() != mount.Bucket.Height)
        {
            image.Dispose();
            diagnostic = new PrivateLocalPresentationAssetMountDiagnostic(
                PrivateLocalPresentationAssetMountFailureCode.TextureRejected,
                "Godot rejected the admitted private tactical cursor texture.");
            return false;
        }

        ImageTexture texture = ImageTexture.CreateFromImage(image);
        image.Dispose();
        TextureRect overlay = new()
        {
            Name = "PrivateLocalTacticalSelectionCursor",
            Size = TacticalCellSize,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            TextureFilter = string.Equals(
                mount.Bucket.Filter,
                "nearest",
                StringComparison.Ordinal)
                ? CanvasItem.TextureFilterEnum.Nearest
                : CanvasItem.TextureFilterEnum.Linear,
            Texture = texture,
            ZIndex = 1,
            Visible = false,
        };
        _panel.AddChild(overlay);
        _privateTacticalCursorTexture = texture;
        _privateTacticalCursorOverlay = overlay;
        return true;
    }

    internal void Project(
        GameSessionSnapshot snapshot,
        string outcome,
        GameSessionCommandResult? result = null)
    {
        PublicSyntheticBattlePresentationProjection projection =
            PublicSyntheticBattlePresentationProjection.Create(snapshot, outcome, result);
        Project(projection);
    }

    internal void Project(
        PrivateOriginalMapBattleBridgeSnapshot? bridge,
        string outcome,
        GameSessionCommandResult? result = null)
    {
        PublicSyntheticBattlePresentationProjection projection =
            PublicSyntheticBattlePresentationProjection.Create(bridge, outcome, result);
        Project(projection);
    }

    private void Project(PublicSyntheticBattlePresentationProjection projection)
    {
        _panel.Visible = projection.Visible;
        _title.Text = projection.Title;
        _instruction.Text = projection.Instruction;
        _status.Text = projection.Status;
        _cueStatus.Text = projection.CueStatus;
        _legend.Text = projection.Legend;
        _actorHealthLabel.Text = projection.ActorMaxHitPoints == 0
            ? "ACTOR HP —"
            : $"ACTOR HP {projection.ActorHitPoints}/{projection.ActorMaxHitPoints}";
        _enemyHealthLabel.Text = projection.EnemyMaxHitPoints == 0
            ? "ENEMY HP —"
            : $"ENEMY HP {projection.EnemyHitPoints}/{projection.EnemyMaxHitPoints}";
        SetHealthFill(_actorHealthFill, projection.ActorHitPoints, projection.ActorMaxHitPoints);
        SetHealthFill(_enemyHealthFill, projection.EnemyHitPoints, projection.EnemyMaxHitPoints);
        if (_privateTacticalCursorOverlay is not null)
        {
            _privateTacticalCursorOverlay.Visible = false;
        }

        for (int index = 0; index < _cells.Count; index++)
        {
            PublicSyntheticBattleCellProjection? cell =
                index < projection.Cells.Count ? projection.Cells[index] : null;
            _cells[index].Visible = cell is not null;
            _cellLabels[index].Visible = cell is not null;
            if (cell is null)
            {
                continue;
            }

            _cells[index].Color = cell.HasCursor
                ? new Color("b58b2a")
                : new Color("31415f");
            string occupant = cell.HasActor ? "A" : cell.HasEnemy ? "E" : string.Empty;
            _cellLabels[index].Text = cell.HasCursor
                ? $"▣{occupant}"
                : occupant.Length == 0 ? "·" : occupant;
            _cellLabels[index].AddThemeColorOverride(
                "font_color",
                cell.HasActor
                    ? new Color("75c7ff")
                    : cell.HasEnemy
                        ? new Color("ff7d7d")
                        : Colors.White);
            if (cell.HasCursor && _privateTacticalCursorOverlay is not null)
            {
                _privateTacticalCursorOverlay.Position = _cells[index].Position;
                _privateTacticalCursorOverlay.Visible = true;
            }
        }
    }

    private static Label LabelAt(
        Node parent,
        Vector2 position,
        Vector2 size,
        int fontSize,
        Color color)
    {
        Label label = new()
        {
            Position = position,
            Size = size,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            ClipText = true,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", color);
        parent.AddChild(label);
        return label;
    }

    private static ColorRect HealthTrack(Node parent, Rect2 bounds)
    {
        ColorRect track = new()
        {
            Position = bounds.Position,
            Size = bounds.Size,
            Color = new Color("374159"),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        parent.AddChild(track);
        return track;
    }

    private static ColorRect HealthFill(ColorRect track, Color color)
    {
        ColorRect fill = new()
        {
            Size = track.Size,
            Color = color,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        track.AddChild(fill);
        return fill;
    }

    private static void SetHealthFill(ColorRect fill, int hitPoints, int maximum)
    {
        float ratio = maximum == 0 ? 0 : (float)hitPoints / maximum;
        fill.Size = new Vector2(148 * ratio, 12);
    }
}
