using System.Collections.ObjectModel;

namespace Sf2.Remake.Application.Content;

public sealed record MapDialogueId
{
    public MapDialogueId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapDialogueLineId
{
    public MapDialogueLineId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapDialogueLineDefinition
{
    public const int MaximumTextLength = 120;

    public MapDialogueLineDefinition(
        MapDialogueLineId line,
        string text,
        PresentationCueId cue)
    {
        Line = line ?? throw new ArgumentNullException(nameof(line));
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        if (text.Length > MaximumTextLength ||
            !string.Equals(text, text.Trim(), StringComparison.Ordinal) ||
            text.Any(char.IsControl))
        {
            throw new ArgumentException(
                $"Synthetic dialogue text must be trimmed, single-line, and at most {MaximumTextLength} characters.",
                nameof(text));
        }

        Text = text;
        Cue = cue ?? throw new ArgumentNullException(nameof(cue));
    }

    public MapDialogueLineId Line { get; }

    public string Text { get; }

    public PresentationCueId Cue { get; }
}

public sealed record MapDialogueDefinition
{
    public const int MaximumLineCount = 3;

    private readonly ReadOnlyCollection<MapDialogueLineDefinition> _lines;

    public MapDialogueDefinition(
        MapDialogueId dialogue,
        MapEntityInteractionTargetId interactionTarget,
        IEnumerable<MapDialogueLineDefinition> lines,
        PresentationCueId closeCue)
    {
        Dialogue = dialogue ?? throw new ArgumentNullException(nameof(dialogue));
        InteractionTarget = interactionTarget ??
            throw new ArgumentNullException(nameof(interactionTarget));
        ArgumentNullException.ThrowIfNull(lines);

        List<MapDialogueLineDefinition> copiedLines = [];
        HashSet<MapDialogueLineId> lineIds = [];
        HashSet<PresentationCueId> cueIds = [];
        foreach (MapDialogueLineDefinition line in lines)
        {
            MapDialogueLineDefinition admitted = line ?? throw new ArgumentException(
                "Dialogue lines cannot contain null values.",
                nameof(lines));
            if (!lineIds.Add(admitted.Line))
            {
                throw new ArgumentException(
                    $"Duplicate dialogue line ID '{admitted.Line}'.",
                    nameof(lines));
            }

            if (!cueIds.Add(admitted.Cue))
            {
                throw new ArgumentException(
                    $"Duplicate dialogue line cue '{admitted.Cue}'.",
                    nameof(lines));
            }

            copiedLines.Add(admitted);
        }

        if (copiedLines.Count is < 1 or > MaximumLineCount)
        {
            throw new ArgumentException(
                $"Synthetic dialogue requires between 1 and {MaximumLineCount} lines.",
                nameof(lines));
        }

        CloseCue = closeCue ?? throw new ArgumentNullException(nameof(closeCue));
        if (!cueIds.Add(CloseCue))
        {
            throw new ArgumentException(
                "The dialogue close cue must be distinct from every line cue.",
                nameof(closeCue));
        }

        _lines = copiedLines.AsReadOnly();
    }

    public MapDialogueId Dialogue { get; }

    public MapEntityInteractionTargetId InteractionTarget { get; }

    public IReadOnlyList<MapDialogueLineDefinition> Lines => _lines;

    public PresentationCueId CloseCue { get; }
}

public sealed class MapDialogueCatalog
{
    private readonly ReadOnlyCollection<MapDialogueDefinition> _definitions;
    private readonly Dictionary<MapDialogueId, MapDialogueDefinition> _byDialogue;
    private readonly Dictionary<MapEntityInteractionTargetId, MapDialogueDefinition> _byTarget;

    public MapDialogueCatalog(IEnumerable<MapDialogueDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        List<MapDialogueDefinition> copiedDefinitions = [];
        HashSet<MapDialogueLineId> lineIds = [];
        HashSet<PresentationCueId> cueIds = [];
        _byDialogue = [];
        _byTarget = [];
        foreach (MapDialogueDefinition definition in definitions)
        {
            MapDialogueDefinition admitted = definition ?? throw new ArgumentException(
                "Dialogue definitions cannot contain null values.",
                nameof(definitions));
            if (!_byDialogue.TryAdd(admitted.Dialogue, admitted))
            {
                throw new ArgumentException(
                    $"Duplicate dialogue ID '{admitted.Dialogue}'.",
                    nameof(definitions));
            }

            if (!_byTarget.TryAdd(admitted.InteractionTarget, admitted))
            {
                throw new ArgumentException(
                    $"Duplicate dialogue interaction target '{admitted.InteractionTarget}'.",
                    nameof(definitions));
            }

            foreach (MapDialogueLineDefinition line in admitted.Lines)
            {
                if (!lineIds.Add(line.Line))
                {
                    throw new ArgumentException(
                        $"Duplicate dialogue line ID '{line.Line}'.",
                        nameof(definitions));
                }

                if (!cueIds.Add(line.Cue))
                {
                    throw new ArgumentException(
                        $"Duplicate dialogue cue '{line.Cue}'.",
                        nameof(definitions));
                }
            }

            if (!cueIds.Add(admitted.CloseCue))
            {
                throw new ArgumentException(
                    $"Duplicate dialogue close cue '{admitted.CloseCue}'.",
                    nameof(definitions));
            }

            copiedDefinitions.Add(admitted);
        }

        _definitions = copiedDefinitions.AsReadOnly();
    }

    public IReadOnlyList<MapDialogueDefinition> Definitions => _definitions;

    public MapDialogueDefinition? FindByDialogue(MapDialogueId dialogue)
    {
        ArgumentNullException.ThrowIfNull(dialogue);
        _byDialogue.TryGetValue(dialogue, out MapDialogueDefinition? definition);
        return definition;
    }

    public MapDialogueDefinition? FindByTarget(MapEntityInteractionTargetId target)
    {
        ArgumentNullException.ThrowIfNull(target);
        _byTarget.TryGetValue(target, out MapDialogueDefinition? definition);
        return definition;
    }
}
