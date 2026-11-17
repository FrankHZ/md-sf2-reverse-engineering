using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Content.Scenarios;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class CaravanDepotPersistenceTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("[61,0,61]", true)]
    [InlineData("null", false)]
    [InlineData("[127]", false)]
    [InlineData("[126]", false)]
    public void AuthoredInitialCustodyAdmitsOnlyKnownOrderedIdentities(string? depot, bool accepted)
    {
        var document = EngineTestContent.Document("battle-drops");
        if (depot is not null)
            document["battle"]!["start"]!["itemCustody"]!["depot"] = JsonNode.Parse(depot);
        var read = EngineTestContent.Reader(document).Read();
        if (accepted)
        {
            var input = Assert.IsType<ExplorationReadAccepted>(read);
            Assert.Equal(depot is null ? Array.Empty<byte>() : new byte[] { 61, 0, 61 }, input.Start.Party.ItemCustody!.Depot);
        }
        else
            Assert.IsType<ScenarioReadRejected>(read);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FieldAndBattleRoundTripOrderedDuplicatesAndAcceptKnownOldAbsence(bool battle)
    {
        var (definition, record) = Record(battle);
        byte[] bytes = OpeningSaveJson.WriteRecord(record);
        var decoded = OpeningSaveJson.ReadRecord(bytes, 1);
        var restored = Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(definition, decoded, RuleCompositions.Sf2())).Session;
        Assert.Equal(new byte[] { 1, 0, 1 }, battle ? restored.Current.Battle.ItemCustody!.Depot : restored.Current.Exploration!.Party.ItemCustody!.Depot);
        if (battle)
        {
            var after = BattleSaveFixture.Stay(restored.Current);
            Assert.Equal(new byte[] { 1, 0, 1 }, after.Battle.ItemCustody!.Depot);
            var captured = BattleSaveFixture.Capture(definition, after);
            Assert.Equal(new byte[] { 1, 0, 1 }, captured.Battle.Custody.Depot);
        }
        else
        {
            var captured = restored.CaptureSave(restored.Current.SessionId, restored.Current.Revision);
            Assert.Null(captured.Error);
            Assert.Equal(new byte[] { 1, 0, 1 }, captured.Data!.Depot);
        }
        var old = Rewrite(bytes, battle, node => node.Remove("depot"));
        var oldSession = Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(definition,
            OpeningSaveJson.ReadRecord(old, 1), RuleCompositions.Sf2())).Session;
        Assert.Empty(battle ? oldSession.Current.Battle.ItemCustody!.Depot : oldSession.Current.Exploration!.Party.ItemCustody!.Depot);
    }

    [Theory]
    [InlineData(false, "null")]
    [InlineData(false, "[127]")]
    [InlineData(false, "[-1]")]
    [InlineData(false, "[1.5]")]
    [InlineData(false, "{}")]
    [InlineData(true, "null")]
    [InlineData(true, "[127]")]
    [InlineData(true, "[-1]")]
    [InlineData(true, "[1.5]")]
    [InlineData(true, "{}")]
    [InlineData(false, "full")]
    [InlineData(true, "full")]
    public void InvalidDepotNeverBecomesAnEmptyCompatibleSave(bool battle, string input)
    {
        var (definition, record) = Record(battle);
        byte[] original = OpeningSaveJson.WriteRecord(record);
        var bad = Rewrite(original, battle, node => node["depot"] = JsonNode.Parse(input == "full" ? "[" + string.Join(',', Enumerable.Repeat(0, 65)) + "]" : input));
        try
        {
            var read = OpeningSaveJson.ReadRecord(bad, 1);
            Assert.IsType<SessionStartFailed>(SaveRecordRestore.Restore(definition, read, RuleCompositions.Sf2()));
        }
        catch (SaveFormatException) { }
        Assert.Equal(original, OpeningSaveJson.WriteRecord(record));
    }

    private static (Application.Content.Scenarios.ScenarioDefinition Definition, SaveRecord Record) Record(bool battle)
    {
        if (battle)
        {
            var (definition, current) = BattleSaveFixture.Create();
            var data = BattleSaveFixture.Capture(definition, current);
            data = data with { Battle = data.Battle with { Custody = data.Battle.Custody.WithDepot([1, 0, 1]) } };
            return (definition, new BattleSaveRecord(1, 1, DateTimeOffset.UnixEpoch, data));
        }
        var (field, saved) = OpeningRestoreTests.Fixture();
        return (field, new OpeningSaveRecord(1, 1, 1, DateTimeOffset.UnixEpoch, saved with { Depot = new byte[] { 1, 0, 1 } }));
    }

    private static byte[] Rewrite(byte[] bytes, bool battle, Action<JsonObject> edit)
    {
        var envelope = JsonNode.Parse(bytes)!;
        var root = JsonNode.Parse(Convert.FromBase64String(envelope["payload"]!.GetValue<string>()))!;
        var node = battle ? root["data"]!["battle"]!["custody"]! : root["data"]!;
        edit(node.AsObject());
        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(root);
        envelope["payload"] = Convert.ToBase64String(payload);
        envelope["sha256"] = Convert.ToHexString(SHA256.HashData(payload));
        return JsonSerializer.SerializeToUtf8Bytes(envelope);
    }
}
