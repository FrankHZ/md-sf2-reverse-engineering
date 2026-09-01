using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Content;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter;
using Xunit;

namespace Sf2.Remake.Godot.Tests;

public sealed class Map3PresenterTests
{
    [Fact]
    public void InitialSnapshotBuildsAReadableControlAndStatusDeck()
    {
        GameSession session = StartSession();

        Map3PresentationProjection projection =
            Map3PresentationProjection.Create(session.Snapshot, "Ready");

        Assert.Equal(
            "MAP map3 · TILE 56,3 · FACE → East · STEP 0\nReady",
            projection.Status);
        Assert.Contains("MOVE W/A/S/D · TURN ↑/←/↓/→", projection.ControlGuide);
        Assert.Contains("CONTEXT Enter · EVENT Z / ACK X · LOCAL C / ACK V", projection.ControlGuide);
        Assert.Contains("INTERACT F / ACK G · DIALOGUE H", projection.ControlGuide);
        Assert.Contains("SEARCH Q / ACK E · ACQUIRE R / ACK T · OUTBOUND Y / ACK U", projection.ControlGuide);
        Assert.Equal(
            "MAP SYMBOLS  ▲ player facing · ◆ placeholder entity · × blocked",
            projection.MapLegend);
        Assert.Equal("CONTEXT  Enter\nNot selected", projection.ContextStatus);
        Assert.Equal("EVENT  Z / ACK X\nNo request", projection.EventRequestStatus);
        Assert.Equal("EFFECT\nNone applied", projection.EffectStatus);
        Assert.Equal("LOCAL  C / ACK V\nNo transition", projection.TransitionStatus);
        Assert.Equal(
            "ENTITIES\n◆ 1 current-map\nsynthetic-map3-placeholder-guide@(55,3)",
            projection.EntityStatus);
        Assert.Equal("INTERACT  F / ACK G\nNo request", projection.EntityInteractionStatus);
        Assert.Equal("DIALOGUE  H\nClosed", projection.DialogueStatus);
        Assert.Equal(
            "SEARCH  Q / ACK E\nNo request · discovered 0",
            projection.FieldSearchStatus);
        Assert.Equal(
            "ACQUIRE  R / ACK T\nInventory: empty",
            projection.ItemAcquisitionStatus);
        Assert.Equal("OUTBOUND  Y / ACK U\nNo transition", projection.OutboundTransitionStatus);
    }

    [Fact]
    public void ExplorationPresentationFitsTheFixedCanvasAndUsesOrientedGlyphs()
    {
        Assert.True(Map3Presenter.MapBounds.End.X <= Map3Presenter.CanvasSize.X);
        Assert.True(Map3Presenter.MapBounds.End.Y <= Map3Presenter.CanvasSize.Y);
        Assert.True(Map3Presenter.ActionDeckBounds.End.X <= Map3Presenter.CanvasSize.X);
        Assert.True(Map3Presenter.ActionDeckBounds.End.Y <= Map3Presenter.CanvasSize.Y);
        Assert.Equal(SyntheticMapViewport.CanvasSize, Map3Presenter.MapBounds.Size);
        Assert.Equal(global::Godot.Vector2.Up, SyntheticMapViewport.FacingVector(SemanticFacing.North));
        Assert.Equal(global::Godot.Vector2.Right, SyntheticMapViewport.FacingVector(SemanticFacing.East));
        Assert.Equal(global::Godot.Vector2.Down, SyntheticMapViewport.FacingVector(SemanticFacing.South));
        Assert.Equal(global::Godot.Vector2.Left, SyntheticMapViewport.FacingVector(SemanticFacing.West));
    }

    [Fact]
    public void ContextRequestAndEffectSnapshotsPreserveRichPresentationStrings()
    {
        GameSession session = StartSession();
        Assert.IsType<GameSessionCommandApplied>(
            session.Apply(new MoveExplorationCommand(ExplorationDirection.East)));
        GameSessionContextSelected selected = Assert.IsType<GameSessionContextSelected>(
            session.Apply(
                new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        GameSessionEventRequested requested = Assert.IsType<GameSessionEventRequested>(
            session.Apply(new RequestSelectedZoneEventCommand()));

        Map3PresentationProjection pending = Map3PresentationProjection.Create(
            requested.Snapshot,
            "Event request pending");

        Assert.Equal(
            "MAP map3 · TILE 57,3 · FACE → East · STEP 3\nEvent request pending",
            pending.Status);
        Assert.Equal(
            "CONTEXT  Enter\nSetup ms_map3\nArea text 423/1000 · Zone selected",
            pending.ContextStatus);
        Assert.Equal(
            "EVENT  Z / ACK X\nPending · Cue #1\nsynthetic-map3-east-zone-request",
            pending.EventRequestStatus);

        GameSessionEventEffectApplied applied = Assert.IsType<GameSessionEventEffectApplied>(
            session.Apply(
                new AcknowledgeMapEventRequestCommand(
                    requested.Request.Request,
                    requested.Cue.Sequence,
                    requested.Request.ExpectedEffect)));
        Map3PresentationProjection effected = Map3PresentationProjection.Create(
            applied.Snapshot,
            "Synthetic effect applied; re-select context");

        Assert.Equal("CONTEXT  Enter\nNot selected", effected.ContextStatus);
        Assert.Equal(
            "EVENT  Z / ACK X\nAcknowledged · Cue #1\nsynthetic-map3-east-zone-request",
            effected.EventRequestStatus);
        Assert.Equal(
            "EFFECT\nApplied once · step 4\nFlag synthetic-map3-variant-enabled",
            effected.EffectStatus);
        Assert.Equal(selected.Selection.Map, applied.Snapshot.Exploration.Map);
    }

    private static GameSession StartSession()
    {
        string contentPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "game",
            "content",
            "public-synthetic-map3-smoke-v1.json"));
        byte[] packageBytes = File.ReadAllBytes(contentPath);
        PublicSyntheticMap3PackageReader source =
            PublicSyntheticMap3PackageReader.FromDocumentBytes(packageBytes);
        GameSessionStarted started = Assert.IsType<GameSessionStarted>(
            GameSession.Start(
                source,
                new MapScenarioRequest(
                    PublicSyntheticMap3PackageReader.PackageId,
                    ContentProfile.PublicSynthetic)));
        return started.Session;
    }
}
