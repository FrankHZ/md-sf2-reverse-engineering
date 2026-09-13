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
}
