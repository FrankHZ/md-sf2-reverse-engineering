using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Sf2.Remake.Application.Runtime.Persistence;

namespace Sf2.Remake.Content.Persistence;

public sealed class SaveFormatException(string code) : Exception(code)
{ public string Code { get; } = code; }

public static class OpeningSaveJson
{
    public const int MaximumBytes = 1024 * 1024;
    private sealed record Envelope(string Payload, string Sha256);
    // Preserve the original v1 property order despite the shared in-memory record base.
    private sealed record FieldRecord(int FormatVersion, int Slot, long Sequence, DateTimeOffset SavedAt, OpeningSaveData Data);
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { info =>
            { foreach (var property in info.Properties)
                property.IsRequired = !(info.Type == typeof(OpeningSaveData) && property.Name == "names"); } } },
        MaxDepth = 32,
    };
    private static readonly JsonSerializerOptions BattleOptions = CreateBattleOptions();

    private static JsonSerializerOptions CreateBattleOptions()
    {
        var options = new JsonSerializerOptions(Options);
        options.Converters.Add(new BattlePositionConverter());
        options.Converters.Add(new PresenceConverter<byte>());
        options.Converters.Add(new PresenceConverter<ushort>());
        options.Converters.Add(new PresenceConverter<int>());
        options.Converters.Add(new PresenceConverter<bool>());
        options.Converters.Add(new PresenceConverter<string>());
        options.Converters.Add(new PresenceConverter<Sf2.Remake.Domain.Maps.MapPosition>());
        options.Converters.Add(new PresenceConverter<Sf2.Remake.Domain.Battles.SavedBattleRegions>());
        options.Converters.Add(new PresenceConverter<BattleSaveWire.Credit>());
        options.Converters.Add(new PresenceConverter<BattleSaveWire.Visuals>());
        options.Converters.Add(new PresenceConverter<BattleSaveWire.Progress>());
        options.Converters.Add(new PresenceConverter<BattleSaveWire.Loadout>());
        options.Converters.Add(new PresenceConverter<BattleSaveWire.Anchor>());
        return options;
    }

    // One encoding: UTF-8 JSON record, base64 payload, SHA-256 over those exact decoded bytes.
    public static byte[] Write(OpeningSaveRecord record) => WriteRecord(record);

    public static byte[] WriteRecord(SaveRecord record)
    {
        byte[] payload = record switch
        {
            OpeningSaveRecord field when field.FormatVersion == 1 => JsonSerializer.SerializeToUtf8Bytes(
                new FieldRecord(1, field.Slot, field.Sequence, field.SavedAt, field.Data), Options),
            BattleSaveRecord battle when battle.FormatVersion == 2 => JsonSerializer.SerializeToUtf8Bytes(BattleSaveWire.Encode(battle), BattleOptions),
            _ => throw new SaveFormatException("save-version"),
        };
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(new Envelope(Convert.ToBase64String(payload),
            Convert.ToHexString(SHA256.HashData(payload))), Options);
        if (bytes.Length > MaximumBytes) throw new SaveFormatException("save-too-large");
        _ = ReadRecord(bytes, record.Slot);
        return bytes;
    }

    public static OpeningSaveRecord Read(byte[] bytes, int slot) => ReadRecord(bytes, slot) is OpeningSaveRecord field
        ? field : throw new SaveFormatException("save-version");

    public static SaveRecord ReadRecord(byte[] bytes, int slot)
    {
        if (bytes.Length == 0 || bytes.Length > MaximumBytes) throw new SaveFormatException("save-size");
        try
        {
            CheckKeys(bytes);
            var envelope = JsonSerializer.Deserialize<Envelope>(bytes, Options) ?? throw new SaveFormatException("save-empty");
            byte[] payload = Convert.FromBase64String(envelope.Payload);
            if (!string.Equals(envelope.Sha256, Convert.ToHexString(SHA256.HashData(payload)), StringComparison.Ordinal))
                throw new SaveFormatException("save-checksum");
            // Classify the top-level version before visiting or binding its typed payload. A future
            // version's unfamiliar/null data must remain protected from corrupt-primary recovery.
            using var document = JsonDocument.Parse(payload, new JsonDocumentOptions { MaxDepth = 32 });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) throw new SaveFormatException("save-malformed");
            var versions = root.EnumerateObject().Where(p => p.Name == "formatVersion").ToArray();
            if (versions.Length != 1) throw new SaveFormatException(versions.Length == 0 ? "save-malformed" : "save-duplicate-key");
            if (versions[0].Value.ValueKind != JsonValueKind.Number || !versions[0].Value.TryGetInt32(out int version))
                throw new SaveFormatException("save-version");
            if (version is not (1 or 2)) throw new SaveFormatException("save-version");
            // An unsupported v2 kind is also an unavailable reader, never a corrupt older save.
            if (version == 2)
            {
                var kinds = root.EnumerateObject().Where(p => p.Name == "kind").ToArray();
                if (kinds.Length != 1) throw new SaveFormatException(kinds.Length == 0 ? "save-malformed" : "save-duplicate-key");
                if (kinds[0].Value.ValueKind != JsonValueKind.String || kinds[0].Value.GetString() != "battle-player-input")
                    throw new SaveFormatException("save-kind");
            }
            CheckKeys(payload);
            SaveRecord record = version == 1
                ? JsonSerializer.Deserialize<OpeningSaveRecord>(payload, Options) ?? throw new SaveFormatException("save-empty")
                : BattleSaveWire.Decode(JsonSerializer.Deserialize<BattleSaveWire.Record>(payload, BattleOptions) ?? throw new SaveFormatException("save-empty"));
            if (record.Slot is not (1 or 2) || record.Slot != slot || record.Sequence < 1 || record.SavedAt.Offset != TimeSpan.Zero)
                throw new SaveFormatException("save-header");
            return record;
        }
        catch (Exception error) when (error is JsonException or FormatException or ArgumentException)
        { throw new SaveFormatException("save-malformed"); }
    }

    private static void CheckKeys(byte[] bytes)
    {
        using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 32 });
        Visit(document.RootElement);
        static void Visit(JsonElement value)
        {
            if (value.ValueKind == JsonValueKind.Null) throw new SaveFormatException("save-null-value");
            if (value.ValueKind == JsonValueKind.Object)
            {
                var keys = new HashSet<string>(StringComparer.Ordinal);
                foreach (var property in value.EnumerateObject())
                {
                    if (!keys.Add(property.Name)) throw new SaveFormatException("save-duplicate-key");
                    Visit(property.Value);
                }
            }
            else if (value.ValueKind == JsonValueKind.Array)
                foreach (var item in value.EnumerateArray())
                { if (item.ValueKind == JsonValueKind.Null) throw new SaveFormatException("save-null-value"); Visit(item); }
        }
    }
}
