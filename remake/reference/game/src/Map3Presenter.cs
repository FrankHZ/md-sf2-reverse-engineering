using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

internal sealed record Map3PresentationProjection(
    string Status,
    string ControlGuide,
    string MapLegend,
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
            $"MAP {snapshot.Exploration.Map} · " +
                $"TILE {snapshot.Exploration.PlayerPosition.X}," +
                $"{snapshot.Exploration.PlayerPosition.Y} · " +
                $"FACE {FacingGlyph(snapshot.Facing)} {snapshot.Facing} · " +
                $"STEP {snapshot.SimulationStep}\n{outcome}",
            "MOVE W/A/S/D · TURN ↑/←/↓/→\n" +
                "CONTEXT Enter · EVENT Z / ACK X · LOCAL C / ACK V\n" +
                "INTERACT F / ACK G · DIALOGUE H\n" +
                "SEARCH Q / ACK E · ACQUIRE R / ACK T · OUTBOUND Y / ACK U",
            "MAP SYMBOLS  ▲ player facing · ◆ placeholder entity · × blocked",
            snapshot.ContextSelection is null
                ? "CONTEXT  Enter\nNot selected"
                : FormatContext(snapshot.ContextSelection),
            snapshot.EventRequest is null
                ? "EVENT  Z / ACK X\nNo request"
                : FormatEventRequest(snapshot.EventRequest),
            snapshot.LastEventEffect is null
                ? "EFFECT\nNone applied"
                : FormatEffect(snapshot),
            snapshot.LocalTransition is null
                ? "LOCAL  C / ACK V\nNo transition"
                : FormatLocalTransition(snapshot.LocalTransition),
            FormatEntities(snapshot.Entities),
            snapshot.EntityInteraction is null
                ? "INTERACT  F / ACK G\nNo request"
                : FormatEntityInteraction(snapshot.EntityInteraction),
            snapshot.Dialogue is null
                ? "DIALOGUE  H\nClosed"
                : FormatDialogue(snapshot.Dialogue),
            FormatFieldSearch(snapshot),
            FormatItemAcquisition(snapshot),
            snapshot.OutboundTransition is null
                ? "OUTBOUND  Y / ACK U\nNo transition"
                : FormatOutboundTransition(snapshot.OutboundTransition));
    }

    private static string FacingGlyph(SemanticFacing facing) => facing switch
    {
        SemanticFacing.North => "↑",
        SemanticFacing.East => "→",
        SemanticFacing.South => "↓",
        SemanticFacing.West => "←",
        _ => "?",
    };

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
        return $"CONTEXT  Enter\nSetup {selection.SelectedSetup}\n" +
            $"Area {area} · Zone selected";
    }

    private static string FormatEventRequest(MapEventRequestSnapshot request) =>
        $"EVENT  Z / ACK X\n{request.Status} · Cue #{request.CueSequence}\n" +
        $"{request.Request}";

    private static string FormatEffect(GameSessionSnapshot snapshot)
    {
        MapEventEffectSnapshot effect = snapshot.LastEventEffect!;
        return $"EFFECT\nApplied once · step {effect.AppliedAtStep}\n" +
            $"Flag {effect.Flag}";
    }

    private static string FormatLocalTransition(MapLocalTransitionSnapshot transition) =>
        $"LOCAL  C / ACK V\n{transition.Status} · Cue #{transition.CueSequence}\n" +
        $"{transition.SourcePosition.X},{transition.SourcePosition.Y} → " +
        $"{transition.DestinationPosition.X},{transition.DestinationPosition.Y}";

    private static string FormatEntities(IReadOnlyList<MapEntityDefinition> entities) =>
        entities.Count == 0
            ? "ENTITIES\nNone on current map"
            : $"ENTITIES\n◆ {entities.Count} current-map\n" + string.Join(
                ", ",
                entities.Select(entity =>
                    $"{entity.Entity}@({entity.Position.X},{entity.Position.Y})"));

    private static string FormatEntityInteraction(MapEntityInteractionSnapshot interaction) =>
        $"INTERACT  F / ACK G\n{interaction.Status} · Cue #{interaction.CueSequence}\n" +
        $"Entity {interaction.Entity}";

    private static string FormatDialogue(MapDialogueSnapshot dialogue) =>
        dialogue.Status == MapDialogueStatus.Open
            ? $"DIALOGUE  H\nLine {dialogue.CurrentLineIndex + 1} · " +
                $"Cue #{dialogue.CueSequence}\n{dialogue.CurrentLine!.Text}"
            : $"DIALOGUE  H\nClosed · Cue #{dialogue.CueSequence}\n" +
                $"{dialogue.Dialogue}";

    private static string FormatFieldSearch(GameSessionSnapshot snapshot)
    {
        int discoveries = snapshot.Discoveries.Discoveries.Count;
        return snapshot.FieldSearch is null
            ? $"SEARCH  Q / ACK E\nNo request · discovered {discoveries}"
            : $"SEARCH  Q / ACK E\n{snapshot.FieldSearch.Status} · " +
                $"{snapshot.FieldSearch.Result}\nDiscovered {discoveries}";
    }

    private static string FormatItemAcquisition(GameSessionSnapshot snapshot)
    {
        string items = snapshot.Inventory.Items.Count == 0
            ? "empty"
            : string.Join(", ", snapshot.Inventory.Items);
        return snapshot.ItemAcquisition is null
            ? $"ACQUIRE  R / ACK T\nInventory: {items}"
            : $"ACQUIRE  R / ACK T\n{snapshot.ItemAcquisition.Status} · " +
                $"{snapshot.ItemAcquisition.Result}\nInventory: {items}";
    }

    private static string FormatOutboundTransition(MapOutboundTransitionSnapshot transition) =>
        $"OUTBOUND  Y / ACK U\n{transition.Status} · Cue #{transition.CueSequence}\n" +
        $"→ {transition.DestinationMap}@" +
        $"{transition.DestinationPosition.X},{transition.DestinationPosition.Y}";
}

internal sealed class Map3Presenter
{
    internal static readonly Vector2 CanvasSize = new(960, 540);
    internal static readonly Rect2 MapBounds = new(16, 84, 576, 336);
    internal static readonly Rect2 ActionDeckBounds = new(608, 82, 336, 444);

    private readonly SyntheticMapViewport _viewport;
    private readonly Label _status;
    private readonly Label _controls;
    private readonly Label _mapLegend;
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
        Label controls,
        Label mapLegend,
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
        _controls = controls;
        _mapLegend = mapLegend;
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
            Position = new Vector2(16, 12),
        };
        banner.AddThemeFontSizeOverride("font_size", 22);
        banner.AddThemeColorOverride("font_color", new Color("ffbd59"));
        parent.AddChild(banner);

        Label explanation = new()
        {
            Text = "Project-authored playable shell · semantic commands · targets never interpreted",
            Position = new Vector2(16, 46),
        };
        explanation.AddThemeFontSizeOverride("font_size", 14);
        parent.AddChild(explanation);

        SyntheticMapViewport viewport = new()
        {
            Position = MapBounds.Position,
        };
        parent.AddChild(viewport);

        Control deck = new()
        {
            Position = ActionDeckBounds.Position,
            Size = ActionDeckBounds.Size,
        };
        parent.AddChild(deck);
        ColorRect deckBackground = new()
        {
            Color = new Color("101827ef"),
            Size = deck.Size,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        deck.AddChild(deckBackground);

        Label deckTitle = DeckLabel(
            ActionDeckBounds.Position + new Vector2(12, 8),
            new Vector2(312, 24),
            15,
            new Color("ffbd59"));
        deckTitle.Text = "EXPLORATION / ACTIONS";
        parent.AddChild(deckTitle);

        Label status = DeckLabel(
            ActionDeckBounds.Position + new Vector2(12, 32),
            new Vector2(312, 46),
            13,
            Colors.White);
        status.Text = "Admitting synthetic package...";
        parent.AddChild(status);
        Label controls = DeckLabel(
            ActionDeckBounds.Position + new Vector2(12, 80),
            new Vector2(312, 68),
            10,
            new Color("b8f2c2"));
        controls.Text = "Waiting for controls...";
        parent.AddChild(controls);
        Label mapLegend = DeckLabel(
            ActionDeckBounds.Position + new Vector2(12, 150),
            new Vector2(312, 24),
            10,
            new Color("ffe2a8"));
        mapLegend.Text = "Waiting for map symbols...";
        parent.AddChild(mapLegend);

        const float leftX = 12;
        const float rightX = 172;
        const float columnWidth = 152;
        Label contextStatus = DeckLabel(
            ActionDeckBounds.Position + new Vector2(leftX, 178),
            new Vector2(columnWidth, 48),
            10,
            Colors.White);
        parent.AddChild(contextStatus);
        Label eventRequestStatus = DeckLabel(
            ActionDeckBounds.Position + new Vector2(leftX, 226),
            new Vector2(columnWidth, 48),
            10,
            Colors.White);
        parent.AddChild(eventRequestStatus);
        Label effectStatus = DeckLabel(
            ActionDeckBounds.Position + new Vector2(leftX, 274),
            new Vector2(columnWidth, 48),
            10,
            new Color("ffdda1"));
        parent.AddChild(effectStatus);
        Label transitionStatus = DeckLabel(
            ActionDeckBounds.Position + new Vector2(leftX, 322),
            new Vector2(columnWidth, 48),
            10,
            new Color("d8c6ff"));
        parent.AddChild(transitionStatus);

        Label entityStatus = DeckLabel(
            ActionDeckBounds.Position + new Vector2(rightX, 178),
            new Vector2(columnWidth, 48),
            10,
            Colors.White);
        parent.AddChild(entityStatus);
        Label entityInteractionStatus = DeckLabel(
            ActionDeckBounds.Position + new Vector2(rightX, 226),
            new Vector2(columnWidth, 48),
            10,
            Colors.White);
        parent.AddChild(entityInteractionStatus);
        Label dialogueStatus = DeckLabel(
            ActionDeckBounds.Position + new Vector2(rightX, 274),
            new Vector2(columnWidth, 58),
            10,
            new Color("c6e5ff"));
        parent.AddChild(dialogueStatus);
        Label fieldSearchStatus = DeckLabel(
            ActionDeckBounds.Position + new Vector2(rightX, 332),
            new Vector2(columnWidth, 48),
            10,
            new Color("b8f2c2"));
        parent.AddChild(fieldSearchStatus);
        Label itemAcquisitionStatus = DeckLabel(
            ActionDeckBounds.Position + new Vector2(rightX, 380),
            new Vector2(columnWidth, 52),
            10,
            new Color("ffe2a8"));
        parent.AddChild(itemAcquisitionStatus);
        Label outboundTransitionStatus = DeckLabel(
            ActionDeckBounds.Position + new Vector2(leftX, 370),
            new Vector2(columnWidth, 62),
            10,
            new Color("d8c6ff"));
        parent.AddChild(outboundTransitionStatus);

        return new Map3Presenter(
            viewport,
            status,
            controls,
            mapLegend,
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

    private static Label DeckLabel(
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
        return label;
    }

    internal void Project(GameSessionSnapshot snapshot, string outcome)
    {
        Map3PresentationProjection projection =
            Map3PresentationProjection.Create(snapshot, outcome);
        _viewport.Project(snapshot);
        _status.Text = projection.Status;
        _controls.Text = projection.ControlGuide;
        _mapLegend.Text = projection.MapLegend;
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
