using Sf2.Remake.Application.Content;

namespace Sf2.Remake.Application.Sessions;

public enum MapDialogueStatus
{
    Open,
    Closed,
}

public enum MapDialogueCueKind
{
    LinePresented,
    Closed,
}

public sealed record AdvanceDialogueCommand : IGameSessionCommand
{
    public AdvanceDialogueCommand(
        MapDialogueId dialogue,
        long cueSequence,
        MapDialogueLineId currentLine)
    {
        Dialogue = dialogue ?? throw new ArgumentNullException(nameof(dialogue));
        ArgumentOutOfRangeException.ThrowIfLessThan(cueSequence, 1);
        CueSequence = cueSequence;
        CurrentLine = currentLine ?? throw new ArgumentNullException(nameof(currentLine));
    }

    public MapDialogueId Dialogue { get; }

    public long CueSequence { get; }

    public MapDialogueLineId CurrentLine { get; }
}

public sealed record MapDialogueSnapshot
{
    private MapDialogueSnapshot(
        MapDialogueId dialogue,
        MapEntityInteractionTargetId triggerTarget,
        MapDialogueStatus status,
        int currentLineIndex,
        MapDialogueLineDefinition? currentLine,
        long openedAtStep,
        long lastAdvancedAtStep,
        long cueSequence,
        long? closedAtStep)
    {
        Dialogue = dialogue ?? throw new ArgumentNullException(nameof(dialogue));
        TriggerTarget = triggerTarget ?? throw new ArgumentNullException(nameof(triggerTarget));
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(currentLineIndex);
        ArgumentOutOfRangeException.ThrowIfLessThan(openedAtStep, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(lastAdvancedAtStep, openedAtStep);
        ArgumentOutOfRangeException.ThrowIfLessThan(cueSequence, 1);
        if (status == MapDialogueStatus.Open &&
            (currentLine is null || closedAtStep is not null))
        {
            throw new ArgumentException(
                "An open dialogue requires a current line and cannot have a close step.",
                nameof(currentLine));
        }

        if (status == MapDialogueStatus.Closed &&
            (currentLine is not null || closedAtStep is null || closedAtStep != lastAdvancedAtStep))
        {
            throw new ArgumentException(
                "A closed dialogue requires no current line and an exact terminal step.",
                nameof(closedAtStep));
        }

        Status = status;
        CurrentLineIndex = currentLineIndex;
        CurrentLine = currentLine;
        OpenedAtStep = openedAtStep;
        LastAdvancedAtStep = lastAdvancedAtStep;
        CueSequence = cueSequence;
        ClosedAtStep = closedAtStep;
    }

    public MapDialogueId Dialogue { get; }

    public MapEntityInteractionTargetId TriggerTarget { get; }

    public MapDialogueStatus Status { get; }

    public int CurrentLineIndex { get; }

    public MapDialogueLineDefinition? CurrentLine { get; }

    public long OpenedAtStep { get; }

    public long LastAdvancedAtStep { get; }

    public long CueSequence { get; }

    public long? ClosedAtStep { get; }

    internal static MapDialogueSnapshot Open(
        MapDialogueDefinition definition,
        long openedAtStep,
        long cueSequence) =>
        new(
            definition.Dialogue,
            definition.InteractionTarget,
            MapDialogueStatus.Open,
            currentLineIndex: 0,
            definition.Lines[0],
            openedAtStep,
            openedAtStep,
            cueSequence,
            closedAtStep: null);

    internal MapDialogueSnapshot Advance(
        MapDialogueDefinition definition,
        long advancedAtStep,
        long cueSequence)
    {
        if (definition.Dialogue != Dialogue ||
            definition.InteractionTarget != TriggerTarget ||
            Status != MapDialogueStatus.Open ||
            CurrentLine is null ||
            CurrentLineIndex >= definition.Lines.Count ||
            definition.Lines[CurrentLineIndex].Line != CurrentLine.Line ||
            advancedAtStep <= LastAdvancedAtStep ||
            cueSequence <= CueSequence)
        {
            throw new InvalidOperationException(
                "Dialogue advancement requires the exact admitted open-line definition.");
        }

        int nextLineIndex = checked(CurrentLineIndex + 1);
        if (nextLineIndex < definition.Lines.Count)
        {
            return new MapDialogueSnapshot(
                Dialogue,
                TriggerTarget,
                MapDialogueStatus.Open,
                nextLineIndex,
                definition.Lines[nextLineIndex],
                OpenedAtStep,
                advancedAtStep,
                cueSequence,
                closedAtStep: null);
        }

        return new MapDialogueSnapshot(
            Dialogue,
            TriggerTarget,
            MapDialogueStatus.Closed,
            nextLineIndex,
            currentLine: null,
            OpenedAtStep,
            advancedAtStep,
            cueSequence,
            advancedAtStep);
    }
}

public sealed record MapDialogueCue
{
    private MapDialogueCue(
        PresentationCueId cue,
        MapDialogueId dialogue,
        MapDialogueCueKind kind,
        MapDialogueLineId? line,
        string? text,
        long sequence)
    {
        Cue = cue ?? throw new ArgumentNullException(nameof(cue));
        Dialogue = dialogue ?? throw new ArgumentNullException(nameof(dialogue));
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        if (kind == MapDialogueCueKind.LinePresented &&
            (line is null || string.IsNullOrWhiteSpace(text)))
        {
            throw new ArgumentException(
                "A line-presentation cue requires a typed line and project-authored text.",
                nameof(line));
        }

        if (kind == MapDialogueCueKind.Closed && (line is not null || text is not null))
        {
            throw new ArgumentException(
                "A close cue cannot carry dialogue text.",
                nameof(line));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(sequence, 1);
        Kind = kind;
        Line = line;
        Text = text;
        Sequence = sequence;
    }

    public PresentationCueId Cue { get; }

    public MapDialogueId Dialogue { get; }

    public MapDialogueCueKind Kind { get; }

    public MapDialogueLineId? Line { get; }

    public string? Text { get; }

    public long Sequence { get; }

    public bool RequiresAcknowledgement => Kind == MapDialogueCueKind.LinePresented;

    internal static MapDialogueCue LinePresented(
        MapDialogueDefinition definition,
        MapDialogueLineDefinition line,
        long sequence) =>
        new(
            line.Cue,
            definition.Dialogue,
            MapDialogueCueKind.LinePresented,
            line.Line,
            line.Text,
            sequence);

    internal static MapDialogueCue Closed(
        MapDialogueDefinition definition,
        long sequence) =>
        new(
            definition.CloseCue,
            definition.Dialogue,
            MapDialogueCueKind.Closed,
            line: null,
            text: null,
            sequence);
}

public sealed record GameSessionDialogueAdvanced(
    GameSessionSnapshot Snapshot,
    MapDialogueSnapshot Dialogue,
    MapDialogueCue Cue) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public MapDialogueSnapshot Dialogue { get; } =
        Dialogue ?? throw new ArgumentNullException(nameof(Dialogue));

    public MapDialogueCue Cue { get; } =
        Cue ?? throw new ArgumentNullException(nameof(Cue));
}

public sealed record GameSessionDialogueClosed(
    GameSessionSnapshot Snapshot,
    MapDialogueSnapshot Dialogue,
    MapDialogueCue Cue) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public MapDialogueSnapshot Dialogue { get; } =
        Dialogue ?? throw new ArgumentNullException(nameof(Dialogue));

    public MapDialogueCue Cue { get; } =
        Cue ?? throw new ArgumentNullException(nameof(Cue));
}
