using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Sf2.Remake.Application.Runtime.Persistence;

namespace Sf2.Remake.Content.Persistence;

public sealed record OpeningSaveRecord(int FormatVersion, int Slot, long Sequence, DateTimeOffset SavedAt, OpeningSaveData Data);
public sealed class SaveFormatException(string code) : Exception(code)
{ public string Code { get; } = code; }

public static class OpeningSaveJson
{
    public const int MaximumBytes = 1024 * 1024;
    private sealed record Envelope(string Payload, string Sha256);
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { info =>
            { foreach (var property in info.Properties) property.IsRequired = true; } } },
        MaxDepth = 32,
    };

    // One encoding: UTF-8 JSON record, base64 payload, SHA-256 over those exact decoded bytes.
    public static byte[] Write(OpeningSaveRecord record)
    {
        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(record, Options);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(new Envelope(Convert.ToBase64String(payload),
            Convert.ToHexString(SHA256.HashData(payload))), Options);
        if (bytes.Length > MaximumBytes) throw new SaveFormatException("save-too-large");
        _ = Read(bytes, record.Slot);
        return bytes;
    }

    public static OpeningSaveRecord Read(byte[] bytes, int slot)
    {
        if (bytes.Length == 0 || bytes.Length > MaximumBytes) throw new SaveFormatException("save-size");
        try
        {
            CheckKeys(bytes);
            var envelope = JsonSerializer.Deserialize<Envelope>(bytes, Options) ?? throw new SaveFormatException("save-empty");
            byte[] payload = Convert.FromBase64String(envelope.Payload);
            if (!string.Equals(envelope.Sha256, Convert.ToHexString(SHA256.HashData(payload)), StringComparison.Ordinal))
                throw new SaveFormatException("save-checksum");
            CheckKeys(payload);
            var record = JsonSerializer.Deserialize<OpeningSaveRecord>(payload, Options) ?? throw new SaveFormatException("save-empty");
            if (record.FormatVersion != 1) throw new SaveFormatException("save-version");
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
