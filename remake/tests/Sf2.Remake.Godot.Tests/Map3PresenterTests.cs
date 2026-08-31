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
    public void InitialSnapshotPreservesEveryPublicPresentationString()
    {
        GameSession session = StartSession();

        Map3PresentationProjection projection =
            Map3PresentationProjection.Create(session.Snapshot, "Ready");

        Assert.Equal(
            "Map map3  Tile (56, 3)  Facing East  Step 0  Ready  |  " +
                "WASD move / arrows turn / Enter / Z X / C V / F G / H / Q E / R T / Y U",
            projection.Status);
        Assert.Equal("Context not selected.", projection.ContextStatus);
        Assert.Equal("Event request: none.", projection.EventRequestStatus);
        Assert.Equal("Synthetic effect: none.", projection.EffectStatus);
        Assert.Equal("Local transition: none.", projection.TransitionStatus);
        Assert.Equal(
            "Placeholder entities: synthetic-map3-placeholder-guide@(55,3)",
            projection.EntityStatus);
        Assert.Equal(
            "Placeholder interaction: none.",
            projection.EntityInteractionStatus);
        Assert.Equal("Placeholder dialogue: none.", projection.DialogueStatus);
        Assert.Equal(
            "Synthetic field search: none. Discoveries [none]  [Q search / E ack]",
            projection.FieldSearchStatus);
        Assert.Equal(
            "Placeholder inventory [empty]  [R acquire / T ack]",
            projection.ItemAcquisitionStatus);
        Assert.Equal("Outbound transition: none.", projection.OutboundTransitionStatus);
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
            "Map map3  Tile (57, 3)  Facing East  Step 3  Event request pending  |  " +
                "WASD move / arrows turn / Enter / Z X / C V / F G / H / Q E / R T / Y U",
            pending.Status);
        Assert.Equal(
            "Setup ms_map3  Area text 423/1000  " +
                "Zone synthetic-map3-east-zone (selected only)",
            pending.ContextStatus);
        Assert.Equal(
            "Event request synthetic-map3-east-zone-request: Pending  " +
                "Cue #1  Effect synthetic-map3-east-zone-variant-effect  " +
                "Target synthetic-map3-east-zone (opaque)",
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

        Assert.Equal("Context not selected.", effected.ContextStatus);
        Assert.Equal(
            "Event request synthetic-map3-east-zone-request: Acknowledged  " +
                "Cue #1  Effect synthetic-map3-east-zone-variant-effect  " +
                "Target synthetic-map3-east-zone (opaque)",
            effected.EventRequestStatus);
        Assert.Equal(
            "Synthetic effect synthetic-map3-east-zone-variant-effect: applied once at step 4; " +
                "flag synthetic-map3-variant-enabled; " +
                "setup flags [synthetic-map3-variant-enabled]",
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
