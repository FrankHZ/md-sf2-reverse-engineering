using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;

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
            {
                bool sparseStats = info.Type == typeof(BattleCurrentStats);
                foreach (var property in info.Properties)
                {
                    if (sparseStats) property.ShouldSerialize = (_, value) => value is not null;
                    property.IsRequired = !(sparseStats || info.Type == typeof(OpeningSaveData) && property.Name is "names" or "raft" or "layer" or "depot" or "fieldScene" ||
                        info.Type == typeof(BattleSaveWire.Actor) && property.Name == "lastRefreshCounters" ||
                        info.Type == typeof(BattleSaveWire.Data) && property.Name == "hasClassTerms" ||
                        info.Type == typeof(SavedAlly) && property.Name is "baseMove" or "currentStats" ||
                        info.Type == typeof(BattleSaveWire.Progress) && property.Name is "classId" or "baseMove" or "currentStats" ||
                        info.Type == typeof(BattleSaveWire.Custody) && property.Name == "depot" ||
                        info.Type == typeof(BattleSaveWire.Story) && property.Name == "raft" ||
                        info.Type == typeof(BattleSaveWire.Data) && property.Name == "enemyDefinitions");
                }
                if (info.Kind == JsonTypeInfoKind.Object)
                {
                    // Reject unknown members with a compatibility diagnostic, without depending on
                    // JsonException message text. The extension dictionary is never populated.
                    info.UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip;
                    var unknown = info.CreateJsonPropertyInfo(typeof(Dictionary<string, JsonElement>), "unknown");
                    unknown.IsExtensionData = true;
                    unknown.Get = _ => null;
                    unknown.Set = (_, _) => throw new SaveFormatException("save-shape");
                    info.Properties.Add(unknown);
                } } } },
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
            // Populated scenes cannot be understood by pre-Creed v1 readers. Protect new writes at
            // their early version dispatch; continue reading the already accepted scene-bearing v1.
            OpeningSaveRecord field when field.FormatVersion is 1 or 3 => JsonSerializer.SerializeToUtf8Bytes(
                new FieldRecord(field.Data.FieldScene is null ? field.FormatVersion : 3,
                    field.Slot, field.Sequence, field.SavedAt, field.Data), Options),
            BattleSaveRecord battle when battle.FormatVersion is 2 or 4 or 5 => JsonSerializer.SerializeToUtf8Bytes(BattleSaveWire.Encode(battle), BattleOptions),
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

    public static SaveRecord ReadRecord(byte[] bytes, int slot) => ReadRecord(bytes, (int?)slot);

    public static SaveRecord ReadRecord(byte[] bytes, int? slot)
    {
        if (bytes.Length == 0 || bytes.Length > MaximumBytes) throw new SaveFormatException("save-size");
        try
        {
            var envelope = Bind<Envelope>(bytes, Options);
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
            if (version is not (1 or 2 or 3 or 4 or 5)) throw new SaveFormatException("save-version");
            // An unsupported v2 kind is also an unavailable reader, never a corrupt older save.
            if (version is 2 or 4 or 5)
            {
                var kinds = root.EnumerateObject().Where(p => p.Name == "kind").ToArray();
                if (kinds.Length != 1) throw new SaveFormatException(kinds.Length == 0 ? "save-malformed" : "save-duplicate-key");
                if (kinds[0].Value.ValueKind != JsonValueKind.String || kinds[0].Value.GetString() != "battle-player-input")
                    throw new SaveFormatException("save-kind");
            }
            SaveRecord record = version is 1 or 3
                ? Bind<OpeningSaveRecord>(payload, Options)
                : BattleSaveWire.Decode(Bind<BattleSaveWire.Record>(payload, BattleOptions));
            if (record.Slot <= 0 || slot.HasValue && record.Slot != slot || record.Sequence < 1 || record.SavedAt.Offset != TimeSpan.Zero)
                throw new SaveFormatException("save-header");
            return record;
        }
        catch (Exception error) when (error is JsonException or FormatException or ArgumentException)
        { throw new SaveFormatException("save-malformed"); }
    }

    private static T Bind<T>(byte[] bytes, JsonSerializerOptions options) where T : class
    {
        // Identify an unknown member even when its value is null. For known typed members keep
        // the original null diagnostic, including non-nullable numbers rejected by the binder.
        try { CheckKeys(bytes); }
        catch (SaveFormatException error) when (error.Code == "save-null-value")
        {
            try { JsonSerializer.Deserialize<T>(bytes, options); }
            catch (JsonException) { }
            catch (SaveFormatException known) when (known.Code != "save-shape") { }
            throw;
        }
        return JsonSerializer.Deserialize<T>(bytes, options) ?? throw new SaveFormatException("save-empty");
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
