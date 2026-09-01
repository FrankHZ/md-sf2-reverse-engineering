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
        string status,
        string cueStatus,
        IEnumerable<PublicSyntheticBattleCellProjection> cells)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(status);
        ArgumentException.ThrowIfNullOrWhiteSpace(cueStatus);
        ArgumentNullException.ThrowIfNull(cells);
        Visible = visible;
        Title = title;
        Status = status;
        CueStatus = cueStatus;
        Cells = new ReadOnlyCollection<PublicSyntheticBattleCellProjection>(
            [.. cells]);
    }

    internal bool Visible { get; }

    internal string Title { get; }

    internal string Status { get; }

    internal string CueStatus { get; }

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
                $"{lifecycle.Definition.Rules.Battle}  {battleState.Phase}  " +
                    $"Enemy HP {battleState.EnemyHitPoints}  {outcome}",
                FormatCue(result, lifecycle.LastCueSequence),
                cells);
        }

        if (result is GameSessionPublicSyntheticBattleReturned returned)
        {
            return new PublicSyntheticBattlePresentationProjection(
                visible: true,
                "PROJECT-AUTHORED PUBLIC-SYNTHETIC TACTICAL MICRO-BATTLE",
                $"{returned.Completion.Battle} completed; returned to " +
                    $"{returned.Snapshot.Exploration.Map} exploration; " +
                    $"applied {returned.WorldEffect.Effect} / " +
                    $"{returned.WorldEffect.Flag}.",
                $"Cue #{returned.Cue.Sequence} {returned.Cue.Cue}",
                []);
        }

        return new PublicSyntheticBattlePresentationProjection(
            visible: false,
            "PROJECT-AUTHORED PUBLIC-SYNTHETIC TACTICAL MICRO-BATTLE",
            "No public-synthetic battle is active.",
            "Battle cue: none.",
            []);
    }

    private static string FormatCue(GameSessionCommandResult? result, long lastCueSequence) =>
        result switch
        {
            GameSessionPublicSyntheticBattleRequested requested =>
                $"Cue #{requested.Cue.Sequence} {requested.Cue.Cue}",
            GameSessionPublicSyntheticBattleAdmitted admitted =>
                $"Cue #{admitted.Cue.Sequence} {admitted.Cue.Cue}",
            GameSessionPublicSyntheticBattleSelectionConfirmed confirmed
                when confirmed.Cues.Count > 0 =>
                "Cues " + string.Join(
                    ", ",
                    confirmed.Cues.Select(cue => $"#{cue.Sequence} {cue.Cue}")),
            _ => $"Last battle cue sequence #{lastCueSequence}",
        };
}

internal sealed class PublicSyntheticBattlePresenter
{
    private readonly Control _panel;
    private readonly Label _title;
    private readonly Label _status;
    private readonly Label _cueStatus;
    private readonly IReadOnlyList<ColorRect> _cells;
    private readonly IReadOnlyList<Label> _cellLabels;

    private PublicSyntheticBattlePresenter(
        Control panel,
        Label title,
        Label status,
        Label cueStatus,
        IReadOnlyList<ColorRect> cells,
        IReadOnlyList<Label> cellLabels)
    {
        _panel = panel;
        _title = title;
        _status = status;
        _cueStatus = cueStatus;
        _cells = cells;
        _cellLabels = cellLabels;
    }

    internal static PublicSyntheticBattlePresenter Attach(Node2D parent)
    {
        ArgumentNullException.ThrowIfNull(parent);
        Control panel = new()
        {
            Position = new Vector2(540, 95),
            Size = new Vector2(700, 360),
            Visible = false,
        };
        parent.AddChild(panel);
        ColorRect background = new()
        {
            Color = new Color("182034e8"),
            Size = panel.Size,
        };
        panel.AddChild(background);
        Label title = LabelAt(panel, new Vector2(18, 14), 18, new Color("ffbd59"));
        Label status = LabelAt(panel, new Vector2(18, 48), 15, Colors.White);
        Label cueStatus = LabelAt(panel, new Vector2(18, 78), 14, new Color("c6e5ff"));
        Label controls = LabelAt(panel, new Vector2(18, 310), 14, new Color("b8f2c2"));
        controls.Text = "B/N enter · IJKL cursor · Space confirm · Backspace cancel · M return";

        List<ColorRect> cells = [];
        List<Label> cellLabels = [];
        for (int index = 0; index < 6; index++)
        {
            int x = index % 3;
            int y = index / 3;
            ColorRect cell = new()
            {
                Position = new Vector2(18 + (x * 72), 120 + (y * 72)),
                Size = new Vector2(62, 62),
                Color = new Color("31415f"),
            };
            panel.AddChild(cell);
            Label marker = LabelAt(
                panel,
                cell.Position + new Vector2(20, 14),
                22,
                Colors.White);
            cells.Add(cell);
            cellLabels.Add(marker);
        }

        return new PublicSyntheticBattlePresenter(
            panel,
            title,
            status,
            cueStatus,
            cells.AsReadOnly(),
            cellLabels.AsReadOnly());
    }

    internal void Project(
        GameSessionSnapshot snapshot,
        string outcome,
        GameSessionCommandResult? result = null)
    {
        PublicSyntheticBattlePresentationProjection projection =
            PublicSyntheticBattlePresentationProjection.Create(snapshot, outcome, result);
        _panel.Visible = projection.Visible;
        _title.Text = projection.Title;
        _status.Text = projection.Status;
        _cueStatus.Text = projection.CueStatus;
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
            _cellLabels[index].Text = cell.HasActor ? "A" : cell.HasEnemy ? "E" : "·";
            _cellLabels[index].AddThemeColorOverride(
                "font_color",
                cell.HasActor
                    ? new Color("75c7ff")
                    : cell.HasEnemy
                        ? new Color("ff7d7d")
                        : Colors.White);
        }
    }

    private static Label LabelAt(
        Node parent,
        Vector2 position,
        int fontSize,
        Color color)
    {
        Label label = new()
        {
            Position = position,
        };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", color);
        parent.AddChild(label);
        return label;
    }
}
