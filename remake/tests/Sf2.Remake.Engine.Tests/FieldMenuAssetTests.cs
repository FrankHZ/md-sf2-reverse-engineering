using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class FieldMenuAssetTests
{
    [Fact]
    public void OptionalFieldArtPreservesOlderPackagesAndAdmitsCompleteRasterSet()
    {
        var before = Assert.IsType<ExplorationReadAccepted>(EngineTestContent.Reader(EngineTestContent.Document("localization-pilot")).Read());
        Assert.Null(before.Definition.Exploration!.Visuals!.FieldMenu);
        var doc = WithArt();
        var admitted = Assert.IsType<ExplorationReadAccepted>(EngineTestContent.Reader(doc).Read());
        var icons = admitted.Definition.Exploration!.Visuals!.FieldMenu!;
        Assert.Equal(8, icons.Count);
        Assert.All(icons.Values, icon => { Assert.Equal(24, icon.Width); Assert.Equal(24, icon.Height); });
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("shape")]
    [InlineData("identity")]
    public void FieldArtCannotSilentlyAdmitPartialMalformedOrForeignPixels(string fault)
    {
        var doc = WithArt();
        var menu = doc["world"]!["presentation"]!["fieldMenu"]!.AsObject();
        if (fault == "missing") menu.Remove("member.selected");
        else if (fault == "shape")
        {
            var pixels = new byte[23 * 24 * 4];
            menu["member.selected"]!["width"] = 23;
            menu["member.selected"]!["data"] = Convert.ToBase64String(pixels);
            menu["member.selected"]!["sha256"] = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pixels));
        }
        else menu["member.selected"]!["sha256"] = new string('0', 64);
        var rejected = Assert.IsType<ScenarioReadRejected>(EngineTestContent.Reader(doc).Read());
        if (fault == "shape") Assert.Equal("field-menu-shape", rejected.Failure.Code);
    }

    private static JsonNode WithArt()
    {
        var doc = EngineTestContent.Document("localization-pilot");
        var pixels = new byte[24 * 24 * 4];
        var digest = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pixels));
        var menu = new JsonObject();
        foreach (string name in new[] { "member", "magic", "item", "search" })
            foreach (string state in new[] { "normal", "selected" })
                menu[name + "." + state] = new JsonObject { ["width"] = 24, ["height"] = 24,
                    ["format"] = "rgba8", ["data"] = Convert.ToBase64String(pixels), ["sha256"] = digest };
        doc["world"]!["presentation"]!["fieldMenu"] = menu;
        return doc;
    }
}
