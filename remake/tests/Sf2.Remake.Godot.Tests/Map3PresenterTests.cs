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

        AssertStatusProjectsSnapshot(session.Snapshot, "Ready", projection.Status);
        Assert.Contains("MOVE W/A/S/D · TURN ↑/←/↓/→", projection.ControlGuide);
        Assert.Contains("CONTEXT Enter · EVENT Z / ACK X · LOCAL C / ACK V", projection.ControlGuide);
        Assert.Contains("INTERACT F / ACK G · DIALOGUE H", projection.ControlGuide);
        Assert.Contains("SEARCH Q / ACK E · ACQUIRE R / ACK T · OUTBOUND Y / ACK U", projection.ControlGuide);
        Assert.Contains("▲ player facing", projection.MapLegend);
        Assert.Contains("◆ placeholder entity", projection.MapLegend);
        Assert.Contains("× blocked", projection.MapLegend);
        Assert.Contains("Not selected", projection.ContextStatus);
        Assert.Contains("No request", projection.EventRequestStatus);
        Assert.Contains("None applied", projection.EffectStatus);
        Assert.Contains("No transition", projection.TransitionStatus);
        MapEntityDefinition entity = Assert.Single(session.Snapshot.Entities);
        Assert.Contains(entity.Entity.Value, projection.EntityStatus);
        Assert.Contains($"{entity.Position.X},{entity.Position.Y}", projection.EntityStatus);
        Assert.Contains("No request", projection.EntityInteractionStatus);
        Assert.Contains("Closed", projection.DialogueStatus);
        Assert.Contains("No request", projection.FieldSearchStatus);
        Assert.Contains("discovered 0", projection.FieldSearchStatus);
        Assert.Contains("Inventory: empty", projection.ItemAcquisitionStatus);
        Assert.Contains("No transition", projection.OutboundTransitionStatus);
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
    public void ContextRequestAndEffectPresentationFollowsAuthoritativeState()
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

        AssertStatusProjectsSnapshot(requested.Snapshot, "Event request pending", pending.Status);
        Assert.Contains(selected.Selection.SelectedSetup.Value, pending.ContextStatus);
        Assert.Contains("423/1000", pending.ContextStatus);
        Assert.Contains("Zone selected", pending.ContextStatus);
        Assert.DoesNotContain("Not selected", pending.ContextStatus);
        Assert.Contains(requested.Request.Status.ToString(), pending.EventRequestStatus);
        Assert.Contains($"Cue #{requested.Cue.Sequence}", pending.EventRequestStatus);
        Assert.Contains(requested.Request.Request.Value, pending.EventRequestStatus);

        GameSessionEventEffectApplied applied = Assert.IsType<GameSessionEventEffectApplied>(
            session.Apply(
                new AcknowledgeMapEventRequestCommand(
                    requested.Request.Request,
                    requested.Cue.Sequence,
                    requested.Request.ExpectedEffect)));
        Map3PresentationProjection effected = Map3PresentationProjection.Create(
            applied.Snapshot,
            "Synthetic effect applied; re-select context");

        Assert.Contains("Not selected", effected.ContextStatus);
        Assert.DoesNotContain("Zone selected", effected.ContextStatus);
        Assert.Contains("Acknowledged", effected.EventRequestStatus);
        Assert.DoesNotContain("Pending", effected.EventRequestStatus);
        Assert.Contains($"Cue #{requested.Cue.Sequence}", effected.EventRequestStatus);
        Assert.Contains(requested.Request.Request.Value, effected.EventRequestStatus);
        Assert.Contains("Applied once", effected.EffectStatus);
        Assert.Contains($"step {applied.Snapshot.LastEventEffect!.AppliedAtStep}", effected.EffectStatus);
        Assert.Contains(applied.Snapshot.LastEventEffect.Flag.Value, effected.EffectStatus);
        Assert.Equal(selected.Selection.Map, applied.Snapshot.Exploration.Map);
    }

    private static void AssertStatusProjectsSnapshot(
        GameSessionSnapshot snapshot, string outcome, string status)
    {
        Assert.Contains(snapshot.Exploration.Map.Value, status);
        Assert.Contains($"{snapshot.Exploration.PlayerPosition.X},{snapshot.Exploration.PlayerPosition.Y}", status);
        Assert.Contains(snapshot.Facing.ToString(), status);
        Assert.Contains($"STEP {snapshot.SimulationStep}", status);
        Assert.Contains(outcome, status);
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
