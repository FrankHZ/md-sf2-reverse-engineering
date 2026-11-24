using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Scenarios;

namespace Sf2.Remake.GodotAdapter.Previews;

public partial class DesignPreview
{
    private static IScenarioSource? Source(Tab tab, Sample sample, Outcome outcome) => tab switch
    {
        Tab.Witch => null,
        Tab.Dialogue when sample == Sample.DialogueChoice => new SampleSource("localization-pilot", AddInvitationChoice),
        Tab.Dialogue => Authored("localization-pilot"),
        Tab.Church => new SampleSource("localization-pilot", StartAtChurch),
        Tab.Inspect => InspectionSource(),
        Tab.Battle => Authored("practice-yard"),
        Tab.Members when sample == Sample.Preparation => new SampleSource("localization-pilot", StartAtPreparation),
        Tab.Field when sample == Sample.Caravan => null,
        Tab.Members or Tab.Shop or Tab.Field or Tab.System => new SampleSource("shop-supplies", AddFieldSign),
        Tab.Results when outcome == Outcome.Defeat => Authored("progression-outcome-defeat"),
        Tab.Results when outcome == Outcome.Retreat => new SampleSource("progression-outcome-victory", TeachEgress),
        _ => Authored("progression-outcome-victory"),
    };

    private static IScenarioSource InspectionSource()
    {
        var args = OS.GetCmdlineUserArgs();
        int option = Array.IndexOf(args, "--authored-package");
        return option >= 0 && option + 1 < args.Length
            ? new AuthoredScenarioPackageReader(args[option + 1]) : Authored("practice-yard");
    }

    private static string ContentPath(string name) => ProjectSettings.GlobalizePath($"res://../content/authored/{name}.json");
    private static AuthoredScenarioPackageReader Authored(string name) => new(ContentPath(name));
    private static JsonNode ContentDocument(string name) => JsonNode.Parse(File.ReadAllText(ContentPath(name)))!;

    private sealed class SampleSource(string name, Action<JsonNode> stage) : IScenarioSource
    {
        public ScenarioReadResult Read()
        {
            try
            {
                var document = ContentDocument(name);
                stage(document);
                return AuthoredScenarioPackageReader.FromDocumentBytes(Encoding.UTF8.GetBytes(document.ToJsonString())).Read();
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException)
            {
                return new ScenarioReadRejected(new(SessionFailureKind.ContentError, "preview-package-unavailable",
                    "preview", "The selected authored sample could not be prepared."));
            }
        }
    }

    private static void StartAtChurch(JsonNode document) =>
        document["start"]!["program"] = new JsonObject { ["program"] = "practice-church", ["instruction"] = 0 };

    private static void AddInvitationChoice(JsonNode document)
    {
        var world = document["world"]!;
        var invitation = world["programs"]!.AsArray().Single(program => (string?)program!["id"] == "invitation")!;
        var instructions = invitation["instructions"]!.AsArray();
        var close = instructions.Single(instruction => (string?)instruction!["op"] == "close-text");
        instructions.Insert(instructions.IndexOf(close), new JsonObject { ["op"] = "yes-no", ["flag"] = 10 });

        // The choice window requests command 65 as it opens and closes. Reuse an authored clip.
        var audio = world["presentation"]!["audio"]!.AsArray();
        var choiceWindow = audio.Single(clip => (string?)clip!["cue"] == "pilot-confirm")!.DeepClone();
        choiceWindow["cue"] = "pilot-choice-window";
        choiceWindow["command"] = 65;
        audio.Add(choiceWindow);
    }

    private static void StartAtPreparation(JsonNode document)
    {
        document["start"]!.AsObject().Remove("program");
        var medic = document["battle"]!["actors"]!.AsArray().Single(actor => (string?)actor!["id"] == "medic-a")!;
        // Supply the existing Practice Leaf so the first member can open Give's recipient page.
        medic["items"]!.AsArray().Add(0);
    }

    private static void AddFieldSign(JsonNode document)
    {
        var world = document["world"]!;
        var map = world["maps"]!.AsArray().Single(node => (string?)node!["id"] == "quay")!;
        // The authored shop and sign can share a faced cell. Field Search reads the sign;
        // interacting with its vendor still enters the ordinary shop caller.
        map["search"] = JsonNode.Parse("""
            {
              "chests": [], "otherItems": [], "closedChest": 55296, "openChest": 55297,
              "descriptions": [{"position":{"x":2,"y":1}, "menuOnly":true, "texts":[425,19001]}]
            }
            """);
        var texts = world["texts"]!.AsArray();
        int[] required = [403, 404, 405, 408, 409, 410, 412, 423, 425, 427];
        foreach (var text in ContentDocument("localization-pilot")["world"]!["texts"]!.AsArray())
            if (required.Contains((int)text!["id"]!)) texts.Add(text.DeepClone());
        texts.Add(new JsonObject { ["id"] = 19001, ["text"] = "The sign reads: Supplies for the road ahead." });
    }

    private static void TeachEgress(JsonNode document)
    {
        // Reuse the authored outcome encounter's existing Egress destination.
        var battle = document["battle"]!;
        var leader = battle["actors"]!.AsArray().Single(actor => (string?)actor!["id"] == "leader")!;
        var start = battle["start"]!["actors"]!.AsArray().Single(actor => (string?)actor!["actor"] == "leader")!;
        battle["spells"]!.AsArray().Add(JsonNode.Parse("""
            {"id":"egress","level":1,"mpCost":8,"minimumRange":0,"maximumRange":0,"effect":{"kind":"retreat"}}
            """));
        leader["spells"]!.AsArray().Add(new JsonObject { ["id"] = "egress", ["level"] = 1 });
        leader["maxMp"] = 8;
        start["mp"] = 8;
    }
}
