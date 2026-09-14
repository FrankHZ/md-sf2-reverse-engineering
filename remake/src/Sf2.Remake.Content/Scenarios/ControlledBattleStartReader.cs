using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal sealed record ControlledAllyStart(byte Id, byte ClassId, byte Level, ushort MaxHp, ushort Hp,
    byte MaxMp, byte Mp, byte Attack, byte Defense, byte Agility, byte Move, ushort Status,
    IReadOnlyList<ushort> Items, IReadOnlyList<byte> Spells, byte? Exp, ushort? Kills, ushort? Defeats);
internal sealed record ControlledBattleStart(byte Battle, uint MainSeed, uint ThinkingSeed, uint? Gold,
    NewBattleStartPolicy Policy, IReadOnlyList<ControlledAllyStart> Allies);

internal static class ControlledBattleStartReader
{
    internal static ControlledBattleStart Read(string path)
    {
        byte[] bytes;
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            Require(stream.Length is > 0 and <= 65536, "input-length", "controlled-start");
            bytes = new byte[(int)stream.Length]; stream.ReadExactly(bytes);
        }
        catch (IOException) { throw new AdmissionIssue(new(Sf2.Remake.Application.Runtime.SessionFailureKind.ContentError,
            "controlled-start-unavailable", "controlled-start", "The controlled start input is unavailable.")); }
        catch (UnauthorizedAccessException) { throw new AdmissionIssue(new(Sf2.Remake.Application.Runtime.SessionFailureKind.ContentError,
            "controlled-start-unavailable", "controlled-start", "The controlled start input cannot be read.")); }
        using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 16 });
        return Decode(document.RootElement);
    }

    internal static ControlledBattleStart Decode(JsonElement root)
    {
        Object(root, "controlled-start", "formatVersion", "id", "evidenceOwner", "bridgeBoundary", "profile",
            "battle", "policy", "mainSeed", "thinkingSeed", "gold", "allies");
        _ = Number(root, "formatVersion", 1, 1);
        Require(Text(root, "profile") == "private-local-controlled-start", "profile", "controlled-start.profile");
        var policy = root.GetProperty("policy");
        Object(policy, "policy", "beforeBattle", "allyStats", "actorStorage", "difficulty", "allyAutoBattle", "opponentControl", "missingCandidateAllyWord");
        Require(Text(policy, "beforeBattle") == "controlled-skip", "before-battle-policy", "policy.beforeBattle", true);
        Require(Text(policy, "allyStats") == "already-refreshed-and-equipped", "ally-stats-policy", "policy.allyStats", true);
        Require(Text(policy, "actorStorage") == "roster-only", "actor-storage-policy", "policy.actorStorage", true);
        var inputPolicy = new NewBattleStartPolicy(Text(root, "id"), Text(root, "evidenceOwner"), Text(root, "bridgeBoundary"),
            (byte)Number(policy, "difficulty", 0, 255), true, true, true, Boolean(policy, "allyAutoBattle"),
            Boolean(policy, "opponentControl"), (ushort?)OptionalNumber(policy, "missingCandidateAllyWord", 65535));
        var allies = new List<ControlledAllyStart>(); var ids = new HashSet<byte>();
        foreach (var row in Array(root, "allies"))
        {
            Object(row, "controlled-ally", "id", "classId", "level", "maxHp", "hp", "maxMp", "mp", "attack", "defense", "agility", "move", "status", "items", "spells", "exp", "kills", "defeats");
            byte id = (byte)Number(row, "id", 0, 29);
            Require(ids.Add(id), "duplicate-party-actor", "allies.id");
            var items = Slots(row, "items", 255); var spells = Slots(row, "spells", 255);
            allies.Add(new(id, (byte)Number(row, "classId", 0, 31), (byte)Number(row, "level", 1, 255),
                (ushort)Number(row, "maxHp", 1, 65535), (ushort)Number(row, "hp", 0, 65535),
                (byte)Number(row, "maxMp", 0, 255), (byte)Number(row, "mp", 0, 255),
                (byte)Number(row, "attack", 0, 255), (byte)Number(row, "defense", 0, 255),
                (byte)Number(row, "agility", 0, 255), (byte)Number(row, "move", 1, 255),
                (ushort)Number(row, "status", 0, 65535), System.Array.AsReadOnly(items.Select(value => (ushort)value).ToArray()),
                System.Array.AsReadOnly(spells.Select(value => (byte)value).ToArray()),
                (byte?)OptionalNumber(row, "exp", 200), (ushort?)OptionalNumber(row, "kills", 9999), (ushort?)OptionalNumber(row, "defeats", 9999)));
        }
        Require(allies.Count > 0, "missing-party", "allies");
        return new((byte)Number(root, "battle", 0, 255), WordImage(root, "mainSeed"), WordImage(root, "thinkingSeed"),
            (uint?)OptionalNumber(root, "gold", 9999999), inputPolicy, allies.AsReadOnly());
    }
    private static int? OptionalNumber(JsonElement row, string field, int maximum) =>
        row.GetProperty(field).ValueKind == JsonValueKind.Null ? null : Number(row, field, 0, maximum);
    private static int[] Slots(JsonElement row, string field, int maximum)
    {
        var values = Array(row, field).ToArray(); Require(values.Length == 4, "slot-count", field);
        return values.Select(value =>
        {
            Require(value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out _), "integer-required", field);
            int number = value.GetInt32(); Require(number >= 0 && number <= maximum, "numeric-range", field); return number;
        }).ToArray();
    }
}
