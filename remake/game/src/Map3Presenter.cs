using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

internal sealed record Map3PresentationProjection(
    string Status,
    string ContextStatus,
    string EventRequestStatus,
    string EffectStatus,
    string TransitionStatus,
    string EntityStatus,
    string EntityInteractionStatus,
    string DialogueStatus,
    string FieldSearchStatus,
    string ItemAcquisitionStatus,
    string OutboundTransitionStatus)
{
    internal static Map3PresentationProjection Create(
        GameSessionSnapshot snapshot,
        string outcome)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(outcome);
        return new Map3PresentationProjection(
            $"Map {snapshot.Exploration.Map}  " +
                $"Tile ({snapshot.Exploration.PlayerPosition.X}, " +
                $"{snapshot.Exploration.PlayerPosition.Y})  " +
                $"Facing {snapshot.Facing}  Step {snapshot.SimulationStep}  {outcome}  |  " +
                "WASD move / arrows turn / Enter / Z X / C V / F G / H / Q E / R T / Y U",
            snapshot.ContextSelection is null
                ? "Context not selected."
                : FormatContext(snapshot.ContextSelection),
            snapshot.EventRequest is null
                ? "Event request: none."
                : FormatEventRequest(snapshot.EventRequest),
            snapshot.LastEventEffect is null
                ? "Synthetic effect: none."
                : FormatEffect(snapshot),
            snapshot.LocalTransition is null
                ? "Local transition: none."
                : FormatLocalTransition(snapshot.LocalTransition),
            FormatEntities(snapshot.Entities),
            snapshot.EntityInteraction is null
                ? "Placeholder interaction: none."
                : FormatEntityInteraction(snapshot.EntityInteraction),
            snapshot.Dialogue is null
                ? "Placeholder dialogue: none."
                : FormatDialogue(snapshot.Dialogue),
            FormatFieldSearch(snapshot),
            FormatItemAcquisition(snapshot),
            snapshot.OutboundTransition is null
                ? "Outbound transition: none."
                : FormatOutboundTransition(snapshot.OutboundTransition));
    }

    private static string FormatContext(ExplorationContextSelectionSnapshot selection)
    {
        string area = selection.AreaDescription.Kind switch
        {
            AreaDescriptionSelectionKind.NoMatch => "none",
            AreaDescriptionSelectionKind.Text =>
                $"text {selection.AreaDescription.InvestigationTextIndex}/" +
                $"{selection.AreaDescription.DescriptionTextIndex}",
            AreaDescriptionSelectionKind.Function =>
                $"opaque function {selection.AreaDescription.Function}",
            _ => "unknown",
        };
        return $"Setup {selection.SelectedSetup}  Area {area}  " +
            $"Zone {selection.ZoneEvent.Target} (selected only)";
    }

    private static string FormatEventRequest(MapEventRequestSnapshot request) =>
        $"Event request {request.Request}: {request.Status}  " +
        $"Cue #{request.CueSequence}  Effect {request.ExpectedEffect}  " +
        $"Target {request.Target} (opaque)";

    private static string FormatEffect(GameSessionSnapshot snapshot)
    {
        MapEventEffectSnapshot effect = snapshot.LastEventEffect!;
        string setFlags = string.Join(", ", snapshot.SyntheticFlags.SetFlags);
        return $"Synthetic effect {effect.Effect}: applied once at step " +
            $"{effect.AppliedAtStep}; flag {effect.Flag}; setup flags [{setFlags}]";
    }

    private static string FormatLocalTransition(MapLocalTransitionSnapshot transition) =>
        $"Local transition {transition.Transition}: {transition.Status}  " +
        $"Cue #{transition.CueSequence}  ({transition.SourcePosition.X}, " +
        $"{transition.SourcePosition.Y}) -> ({transition.DestinationPosition.X}, " +
        $"{transition.DestinationPosition.Y})  Orientation {transition.DestinationOrientation}";

    private static string FormatEntities(IReadOnlyList<MapEntityDefinition> entities) =>
        entities.Count == 0
            ? "Placeholder entities: none."
            : "Placeholder entities: " + string.Join(
                ", ",
                entities.Select(entity =>
                    $"{entity.Entity}@({entity.Position.X},{entity.Position.Y})"));

    private static string FormatEntityInteraction(MapEntityInteractionSnapshot interaction) =>
        $"Placeholder interaction {interaction.Request}: {interaction.Status}  " +
        $"Cue #{interaction.CueSequence}  Entity {interaction.Entity}  " +
        $"Target {interaction.Target} (uninterpreted)";

    private static string FormatDialogue(MapDialogueSnapshot dialogue) =>
        dialogue.Status == MapDialogueStatus.Open
            ? $"Placeholder dialogue {dialogue.Dialogue}: line " +
                $"{dialogue.CurrentLineIndex + 1}  {dialogue.CurrentLine!.Text}  " +
                $"Cue #{dialogue.CueSequence}  [H advances]"
            : $"Placeholder dialogue {dialogue.Dialogue}: closed  " +
                $"Cue #{dialogue.CueSequence}";

    private static string FormatFieldSearch(GameSessionSnapshot snapshot)
    {
        string discoveries = snapshot.Discoveries.Discoveries.Count == 0
            ? "none"
            : string.Join(", ", snapshot.Discoveries.Discoveries);
        return snapshot.FieldSearch is null
            ? $"Synthetic field search: none. Discoveries [{discoveries}]  [Q search / E ack]"
            : $"Synthetic field search {snapshot.FieldSearch.Context}: " +
                $"{snapshot.FieldSearch.Status}  Result {snapshot.FieldSearch.Result}  " +
                $"Discovery {snapshot.FieldSearch.Discovery}  Discoveries [{discoveries}]";
    }

    private static string FormatItemAcquisition(GameSessionSnapshot snapshot)
    {
        string items = snapshot.Inventory.Items.Count == 0
            ? "empty"
            : string.Join(", ", snapshot.Inventory.Items);
        return snapshot.ItemAcquisition is null
            ? $"Placeholder inventory [{items}]  [R acquire / T ack]"
            : $"Placeholder item acquisition {snapshot.ItemAcquisition.Request}: " +
                $"{snapshot.ItemAcquisition.Status}  Result {snapshot.ItemAcquisition.Result}  " +
                $"Item {snapshot.ItemAcquisition.Item}  Inventory [{items}]";
    }

    private static string FormatOutboundTransition(MapOutboundTransitionSnapshot transition) =>
        $"Outbound transition {transition.Transition}: {transition.Status}  " +
        $"Cue #{transition.CueSequence}  {transition.SourceMap}" +
        $"@({transition.SourcePosition.X},{transition.SourcePosition.Y}) -> " +
        $"{transition.DestinationMap}" +
        $"@({transition.DestinationPosition.X},{transition.DestinationPosition.Y})  " +
        $"Facing {transition.DestinationFacing}";
}

internal sealed class Map3Presenter
{
    private readonly SyntheticMapViewport _viewport;
    private readonly Label _status;
    private readonly Label _contextStatus;
    private readonly Label _eventRequestStatus;
    private readonly Label _effectStatus;
    private readonly Label _transitionStatus;
    private readonly Label _entityStatus;
    private readonly Label _entityInteractionStatus;
    private readonly Label _dialogueStatus;
    private readonly Label _fieldSearchStatus;
    private readonly Label _itemAcquisitionStatus;
    private readonly Label _outboundTransitionStatus;

    private Map3Presenter(
        SyntheticMapViewport viewport,
        Label status,
        Label contextStatus,
        Label eventRequestStatus,
        Label effectStatus,
        Label transitionStatus,
        Label entityStatus,
        Label entityInteractionStatus,
        Label dialogueStatus,
        Label fieldSearchStatus,
        Label itemAcquisitionStatus,
        Label outboundTransitionStatus)
    {
        _viewport = viewport;
        _status = status;
        _contextStatus = contextStatus;
        _eventRequestStatus = eventRequestStatus;
        _effectStatus = effectStatus;
        _transitionStatus = transitionStatus;
        _entityStatus = entityStatus;
        _entityInteractionStatus = entityInteractionStatus;
        _dialogueStatus = dialogueStatus;
        _fieldSearchStatus = fieldSearchStatus;
        _itemAcquisitionStatus = itemAcquisitionStatus;
        _outboundTransitionStatus = outboundTransitionStatus;
    }

    internal static Map3Presenter Attach(Node2D parent)
    {
        ArgumentNullException.ThrowIfNull(parent);
        Label banner = new()
        {
            Text = Map3Root.BannerText,
            Position = new Vector2(24, 18),
        };
        banner.AddThemeFontSizeOverride("font_size", 24);
        banner.AddThemeColorOverride("font_color", new Color("ffbd59"));
        parent.AddChild(banner);

        Label explanation = new()
        {
            Text = "Project-authored selectors, placeholder state, and outbound shell; targets are never interpreted.",
            Position = new Vector2(24, 55),
        };
        explanation.AddThemeFontSizeOverride("font_size", 16);
        parent.AddChild(explanation);

        SyntheticMapViewport viewport = new()
        {
            Position = new Vector2(24, 105),
        };
        parent.AddChild(viewport);

        Label status = new()
        {
            Text = "Admitting synthetic package...",
            Position = new Vector2(24, 450),
        };
        status.AddThemeFontSizeOverride("font_size", 18);
        parent.AddChild(status);

        Label contextStatus = new()
        {
            Text = "Context not selected.",
            Position = new Vector2(24, 480),
        };
        contextStatus.AddThemeFontSizeOverride("font_size", 15);
        parent.AddChild(contextStatus);

        Label eventRequestStatus = new()
        {
            Text = "Event request: none.",
            Position = new Vector2(24, 510),
        };
        eventRequestStatus.AddThemeFontSizeOverride("font_size", 15);
        parent.AddChild(eventRequestStatus);

        Label effectStatus = new()
        {
            Text = "Synthetic effect: none.",
            Position = new Vector2(24, 540),
        };
        effectStatus.AddThemeFontSizeOverride("font_size", 15);
        parent.AddChild(effectStatus);

        Label transitionStatus = new()
        {
            Text = "Local transition: none.",
            Position = new Vector2(24, 570),
        };
        transitionStatus.AddThemeFontSizeOverride("font_size", 15);
        parent.AddChild(transitionStatus);

        Label entityStatus = new()
        {
            Text = "Placeholder entities: none.",
            Position = new Vector2(24, 600),
        };
        entityStatus.AddThemeFontSizeOverride("font_size", 15);
        parent.AddChild(entityStatus);

        Label entityInteractionStatus = new()
        {
            Text = "Placeholder interaction: none.",
            Position = new Vector2(24, 630),
        };
        entityInteractionStatus.AddThemeFontSizeOverride("font_size", 15);
        parent.AddChild(entityInteractionStatus);

        Label dialogueStatus = new()
        {
            Text = "Placeholder dialogue: none.",
            Position = new Vector2(24, 660),
        };
        dialogueStatus.AddThemeFontSizeOverride("font_size", 15);
        dialogueStatus.AddThemeColorOverride("font_color", new Color("c6e5ff"));
        parent.AddChild(dialogueStatus);

        Label fieldSearchStatus = new()
        {
            Text = "Synthetic field search: none.",
            Position = new Vector2(24, 690),
        };
        fieldSearchStatus.AddThemeFontSizeOverride("font_size", 15);
        fieldSearchStatus.AddThemeColorOverride("font_color", new Color("b8f2c2"));
        parent.AddChild(fieldSearchStatus);

        Label itemAcquisitionStatus = new()
        {
            Text = "Placeholder inventory: empty.",
            Position = new Vector2(24, 720),
        };
        itemAcquisitionStatus.AddThemeFontSizeOverride("font_size", 15);
        itemAcquisitionStatus.AddThemeColorOverride("font_color", new Color("ffe2a8"));
        parent.AddChild(itemAcquisitionStatus);

        Label outboundTransitionStatus = new()
        {
            Text = "Outbound transition: none.",
            Position = new Vector2(24, 750),
        };
        outboundTransitionStatus.AddThemeFontSizeOverride("font_size", 15);
        outboundTransitionStatus.AddThemeColorOverride("font_color", new Color("d8c6ff"));
        parent.AddChild(outboundTransitionStatus);

        return new Map3Presenter(
            viewport,
            status,
            contextStatus,
            eventRequestStatus,
            effectStatus,
            transitionStatus,
            entityStatus,
            entityInteractionStatus,
            dialogueStatus,
            fieldSearchStatus,
            itemAcquisitionStatus,
            outboundTransitionStatus);
    }

    internal void Project(GameSessionSnapshot snapshot, string outcome)
    {
        Map3PresentationProjection projection =
            Map3PresentationProjection.Create(snapshot, outcome);
        _viewport.Project(snapshot);
        _status.Text = projection.Status;
        _contextStatus.Text = projection.ContextStatus;
        _eventRequestStatus.Text = projection.EventRequestStatus;
        _effectStatus.Text = projection.EffectStatus;
        _transitionStatus.Text = projection.TransitionStatus;
        _entityStatus.Text = projection.EntityStatus;
        _entityInteractionStatus.Text = projection.EntityInteractionStatus;
        _dialogueStatus.Text = projection.DialogueStatus;
        _fieldSearchStatus.Text = projection.FieldSearchStatus;
        _itemAcquisitionStatus.Text = projection.ItemAcquisitionStatus;
        _outboundTransitionStatus.Text = projection.OutboundTransitionStatus;
    }

    internal void ProjectStatus(string message)
    {
        _status.Text = message;
    }
}
