using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Content;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter;
using Xunit;

namespace Sf2.Remake.Godot.Tests;

public sealed class PublicSyntheticBattlePresenterTests
{
    [Fact]
    public void LegacyMap3SmokeReceiptRemainsByteStableBeforeBattleCommands()
    {
        (GameSession session, ScenarioAdmissionReceipt receipt) = StartSession();
        GameSessionSnapshot before = session.Snapshot;
        GameSessionCommandApplied applied = Assert.IsType<GameSessionCommandApplied>(
            session.Apply(new MoveExplorationCommand(ExplorationDirection.East)));

        string json = PublicSyntheticMap3SmokeDriver.CreateLegacyReceipt(
            before,
            applied,
            receipt);

        Assert.Equal(
            "{\"status\":\"Pass\",\"profile\":\"public-synthetic\"," +
            "\"scenarioId\":\"map3-public-synthetic-smoke\"," +
            "\"exactControlledAdmission\":false," +
            "\"capability\":\"map3-synthetic-exploration-smoke\"," +
            "\"evidenceOwner\":\"sf2-map3-admitted-start-runtime-v1\"," +
            "\"mapId\":\"map3\",\"opaqueStartFacing\":3," +
            "\"before\":{\"x\":56,\"y\":3}," +
            "\"after\":{\"x\":57,\"y\":3},\"outcome\":\"Moved\"," +
            "\"simulationStep\":1," +
            "\"banner\":\"PUBLIC SYNTHETIC \\u2014 NOT ORIGINAL FIDELITY\"}",
            json);
    }

    [Fact]
    public void TypedBattleSnapshotProjectsTheExactProjectAuthoredGrid()
    {
        GameSession session = StartActiveBattle();

        PublicSyntheticBattlePresentationProjection projection =
            PublicSyntheticBattlePresentationProjection.Create(
                session.Snapshot,
                "Project-authored tactical battle admitted");

        Assert.True(projection.Visible);
        Assert.Contains("PROJECT-AUTHORED PUBLIC-SYNTHETIC", projection.Title);
        Assert.Contains("public-synthetic-map3-tactical-battle", projection.Status);
        Assert.Contains("MoveSelection", projection.Status);
        Assert.Equal(6, projection.Cells.Count);
        PublicSyntheticBattleCellProjection actor = Assert.Single(
            projection.Cells,
            cell => cell.HasActor);
        PublicSyntheticBattleCellProjection enemy = Assert.Single(
            projection.Cells,
            cell => cell.HasEnemy);
        PublicSyntheticBattleCellProjection cursor = Assert.Single(
            projection.Cells,
            cell => cell.HasCursor);
        Assert.Equal(new TacticalPosition(0, 1), actor.Position);
        Assert.Equal(new TacticalPosition(2, 1), enemy.Position);
        Assert.Same(actor, cursor);
    }

    [Fact]
    public void CompletionAndReturnProjectOnlyTypedResultWithoutCachedBattleState()
    {
        GameSession session = StartActiveBattle();
        Assert.IsType<GameSessionPublicSyntheticBattleCursorMoved>(session.Apply(
            new MovePublicSyntheticBattleCursorCommand(TacticalDirection.East)));
        Assert.IsType<GameSessionPublicSyntheticBattleSelectionConfirmed>(session.Apply(
            new ConfirmPublicSyntheticBattleSelectionCommand()));
        Assert.IsType<GameSessionPublicSyntheticBattleCursorMoved>(session.Apply(
            new MovePublicSyntheticBattleCursorCommand(TacticalDirection.East)));
        GameSessionPublicSyntheticBattleSelectionConfirmed completed =
            Assert.IsType<GameSessionPublicSyntheticBattleSelectionConfirmed>(session.Apply(
                new ConfirmPublicSyntheticBattleSelectionCommand()));

        PublicSyntheticBattlePresentationProjection completedProjection =
            PublicSyntheticBattlePresentationProjection.Create(
                completed.Snapshot,
                "completed",
                completed);
        Assert.Contains("Completed", completedProjection.Status);
        Assert.Contains("public-synthetic-map3-battle-attack-completed", completedProjection.CueStatus);
        Assert.Contains("public-synthetic-map3-battle-completed", completedProjection.CueStatus);

        GameSessionPublicSyntheticBattleReturned returned =
            Assert.IsType<GameSessionPublicSyntheticBattleReturned>(session.Apply(
                new AcknowledgePublicSyntheticBattleCompletionCommand(
                    completed.Completion!.Battle,
                    completed.Completion.CueSequence)));
        PublicSyntheticBattlePresentationProjection returnedProjection =
            PublicSyntheticBattlePresentationProjection.Create(
                returned.Snapshot,
                "returned",
                returned);

        Assert.Null(returned.Snapshot.PublicSyntheticBattle);
        Assert.True(returnedProjection.Visible);
        Assert.Empty(returnedProjection.Cells);
        Assert.Contains("completed; returned", returnedProjection.Status);
        Assert.Contains(
            "public-synthetic-map3-battle-completion-world-effect",
            returnedProjection.Status);
        Assert.Contains(
            "public-synthetic-map3-battle-completed",
            returnedProjection.Status);
        Assert.Contains("public-synthetic-map3-battle-returned", returnedProjection.CueStatus);

        PublicSyntheticBattlePresentationProjection laterProjection =
            PublicSyntheticBattlePresentationProjection.Create(
                returned.Snapshot,
                "later projection");
        Assert.False(laterProjection.Visible);
        Assert.Empty(laterProjection.Cells);
    }

    private static GameSession StartActiveBattle()
    {
        (GameSession session, _) = StartSession();

        Assert.IsType<GameSessionCommandApplied>(session.Apply(
            new MoveExplorationCommand(ExplorationDirection.East)));
        Assert.IsType<GameSessionContextSelected>(session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        GameSessionEventRequested eventRequested = Assert.IsType<GameSessionEventRequested>(
            session.Apply(new RequestSelectedZoneEventCommand()));
        Assert.IsType<GameSessionEventEffectApplied>(session.Apply(
            new AcknowledgeMapEventRequestCommand(
                eventRequested.Request.Request,
                eventRequested.Cue.Sequence,
                eventRequested.Request.ExpectedEffect)));
        Assert.IsType<GameSessionContextSelected>(session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        Assert.IsType<GameSessionCommandApplied>(session.Apply(
            new MoveExplorationCommand(ExplorationDirection.East)));
        Assert.IsType<GameSessionContextSelected>(session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        GameSessionLocalTransitionRequested local =
            Assert.IsType<GameSessionLocalTransitionRequested>(session.Apply(
                new RequestSelectedLocalTransitionCommand()));
        Assert.IsType<GameSessionLocalTransitionApplied>(session.Apply(
            new AcknowledgeMapLocalTransitionCommand(
                local.Transition.Request,
                local.Transition.CueSequence,
                local.Transition.Transition)));
        Assert.IsType<GameSessionCommandApplied>(session.Apply(
            new MoveExplorationCommand(ExplorationDirection.West)));
        Assert.IsType<GameSessionContextSelected>(session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        GameSessionOutboundTransitionRequested outbound =
            Assert.IsType<GameSessionOutboundTransitionRequested>(session.Apply(
                new RequestSelectedOutboundTransitionCommand()));
        Assert.IsType<GameSessionOutboundTransitionApplied>(session.Apply(
            new AcknowledgeMapOutboundTransitionCommand(
                outbound.Transition.Request,
                outbound.Cue.Sequence,
                outbound.Transition.Transition)));
        Assert.IsType<GameSessionCommandApplied>(session.Apply(
            new MoveExplorationCommand(ExplorationDirection.East)));
        Assert.IsType<GameSessionContextSelected>(session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        GameSessionPublicSyntheticBattleRequested requested =
            Assert.IsType<GameSessionPublicSyntheticBattleRequested>(session.Apply(
                new RequestSelectedPublicSyntheticBattleCommand()));
        Assert.IsType<GameSessionPublicSyntheticBattleAdmitted>(session.Apply(
            new AcknowledgePublicSyntheticBattleEntryCommand(
                requested.Battle.Definition.Request,
                requested.Battle.Definition.Rules.Battle,
                requested.Cue.Sequence)));
        return session;
    }

    private static (GameSession Session, ScenarioAdmissionReceipt Receipt) StartSession()
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
        PublicSyntheticMap3PackageReader source =
            PublicSyntheticMap3PackageReader.FromDocumentBytes(File.ReadAllBytes(contentPath));
        GameSessionStarted started = Assert.IsType<GameSessionStarted>(GameSession.Start(
            source,
            new MapScenarioRequest(
                PublicSyntheticMap3PackageReader.PackageId,
                ContentProfile.PublicSynthetic)));
        return (started.Session, started.Receipt);
    }
}
