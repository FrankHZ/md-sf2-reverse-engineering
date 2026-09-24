using System.Text;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Scenarios;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

internal static class EngineTestContent
{
    internal static string PathFor(string package) => Path.Combine(AppContext.BaseDirectory, "authored", package + ".json");
    internal static JsonNode Document(string package = "practice-yard") => JsonNode.Parse(File.ReadAllText(PathFor(package)))!;
    internal static AuthoredScenarioPackageReader Reader(JsonNode document) =>
        AuthoredScenarioPackageReader.FromDocumentBytes(Encoding.UTF8.GetBytes(document.ToJsonString()));
    internal static GameSession Start(string package = "practice-yard", Action<JsonNode>? change = null)
    {
        var document = Document(package);
        change?.Invoke(document);
        return Assert.IsType<SessionStarted>(GameSession.Start(Reader(document))).Session;
    }
    internal static ScenarioReadAccepted Admitted(string package = "practice-yard") =>
        Assert.IsType<ScenarioReadAccepted>(new AuthoredScenarioPackageReader(PathFor(package)).Read());
    internal static SessionResult Send(GameSession session, SessionCommand command)
    {
        var current = session.Current;
        return session.Submit(new(current.SessionId, current.Revision, current.Selection?.Actor, command));
    }
    internal static SessionResult Accept(GameSession session, SessionCommand command)
    {
        var result = Send(session, command);
        Assert.Null(result.Failure);
        return result;
    }
    internal static SessionResult Stay(GameSession session)
    {
        Accept(session, new Confirm());
        Accept(session, new ChooseAction(SessionAction.Stay));
        return Accept(session, new Confirm());
    }

    // Explicitly cross the real scene commands when a test owns a completed-action
    // boundary. Stage timing itself is asserted in BattleSceneTests; ordinary
    // Accept/Start never silently consume a presentation or acknowledgement.
    // Completed-action tests choose the earliest legal acknowledgement by default.
    // Neutral timed-poll tests opt in explicitly; reveal/delivery is not player Wait.
    internal static SessionResult FinishBattleScenes(GameSession session, SessionResult result, bool neutralHealingWait = false)
    {
        var observations = result.Observations.ToList();
        while (session.Current.BattleScene is { } scene)
        {
            Assert.False(session.Current.HasBattleControl);
            result = Accept(session, scene.Healing is { LogicalComplete: false } healing &&
                (!healing.AtTimedInput || neutralHealingWait) ? new AdvanceSimulation(scene.Token) :
                scene.IsHealingMessage && neutralHealingWait ? new CompletePresentation(scene.Token, scene.CompletionKind) :
                scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) :
                    new CompletePresentation(scene.Token, scene.CompletionKind));
            observations.AddRange(result.Observations);
        }
        return result with { Observations = observations.AsReadOnly() };
    }

    // Retained combat-construction expectations exclude scene reaction/growth
    // draws. The completed scene's RNG boundary is independently asserted in
    // BattleSceneTests; this value is not a replacement SessionSnapshot seed.
    internal static IEnumerable<SessionObservation> ConstructionRolls(SessionResult result) => result.Observations.Where(row =>
        row.Kind.StartsWith("rng-", StringComparison.Ordinal) && !row.Kind.StartsWith("rng-reaction-", StringComparison.Ordinal) &&
        !row.Kind.StartsWith("rng-growth-", StringComparison.Ordinal));
    internal static uint ConstructionSeed(SessionResult result) => checked((uint)ConstructionRolls(result).Last().After!.Value);
}
