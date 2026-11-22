using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Content.Persistence;

internal static class FieldSnapshotWire
{
    internal sealed record Record(int FormatVersion, int Slot, long Sequence, DateTimeOffset SavedAt, Data Data);
    internal sealed record Data(SaveCompatibility Compatibility, Visit Visit, FieldPartySnapshot Party, SettledFieldRegisters Registers);
    internal sealed record Visit(FieldMapBody Body, Source Population, EntityRef Player, IReadOnlyList<Actor> Actors,
        IReadOnlyDictionary<EntityRef, int> Aliases, IReadOnlyList<int> CurrentEntities, IReadOnlyList<SavedLayoutCell> Layout,
        FieldVisitRoof? Roof, ushort SpriteSize, IReadOnlyDictionary<int, ProgramLocation> Graphics, RaftBerth? Berth);
    internal sealed record Actor(EntityRef Entity, int Slot, Source Origin, EntityMotionState Motion, EntityFollower? Follower,
        Source? ActionSource, int ActionCursor, bool WaitingForMotion, bool Visible, int? Sprite, int? SharedSpriteSlot,
        ushort RenderSize, bool Priority, bool Shadowless);
    internal sealed record Source(string Kind, FieldMapBody? Body = null, ProgramLocation? Location = null,
        int? Index = null, bool? Overworld = null);

    internal static Record Encode(FieldSnapshotRecord record)
    {
        var data = record.Data.Snapshot;
        var visit = data.Visit;
        var actors = visit.Actors.Select(actor => new Actor(actor.Entity, actor.Slot, EncodeOrigin(actor.Origin),
            actor.Motion, actor.Follower, actor.ActionSource is { } action ? EncodeAction(action) : null,
            actor.ActionCursor, actor.WaitingForMotion, actor.Visible, actor.Sprite, actor.SharedSpriteSlot,
            actor.RenderSize, actor.Priority, actor.Shadowless)).ToArray();
        var population = visit.Population switch
        {
            MapPopulationSource map => new Source("map", Body: map.Body),
            ProgramPopulationSource program => new Source("program", Location: program.Location),
            _ => throw new SaveFormatException("save-shape"),
        };
        return new(record.FormatVersion, record.Slot, record.Sequence, record.SavedAt,
            new(data.Compatibility, new(visit.Body, population, visit.Player, actors, visit.Aliases, visit.CurrentEntities,
                visit.Layout, visit.Roof, visit.SpriteSize, visit.Graphics, visit.Berth), data.Party, data.Registers));
    }

    internal static FieldSnapshotRecord Decode(Record record)
    {
        var data = record.Data;
        var visit = data.Visit;
        FieldPopulationSource population = visit.Population switch
        {
            { Kind: "map", Body: { } body, Location: null, Index: null, Overworld: null } => new MapPopulationSource(body),
            { Kind: "program", Body: null, Location: { } location, Index: null, Overworld: null } => new ProgramPopulationSource(location),
            _ => throw new SaveFormatException("save-shape"),
        };
        var actors = visit.Actors.Select(actor => new FieldVisitActor(actor.Entity, actor.Slot, DecodeOrigin(actor.Origin),
            actor.Motion, actor.Follower, actor.ActionSource is { } action ? DecodeAction(action) : null,
            actor.ActionCursor, actor.WaitingForMotion, actor.Visible, actor.Sprite, actor.SharedSpriteSlot,
            actor.RenderSize, actor.Priority, actor.Shadowless)).ToArray();
        return new(record.Slot, record.Sequence, record.SavedAt,
            new(new(data.Compatibility, new(visit.Body, population, visit.Player, actors, visit.Aliases, visit.CurrentEntities,
                visit.Layout, visit.Roof, visit.SpriteSize, visit.Graphics, visit.Berth), data.Party, data.Registers)));
    }

    private static Source EncodeOrigin(FieldEntityOrigin origin) => origin switch
    {
        PlayerEntityOrigin => new("player"),
        ResidentEntityOrigin resident => new("resident", Index: resident.Row),
        FollowerEntityOrigin follower => new("follower", Index: follower.Character),
        AllocatedEntityOrigin allocated => new("allocation", Location: allocated.Location),
        RaftEntityOrigin => new("raft"),
        _ => throw new SaveFormatException("save-shape"),
    };

    private static FieldEntityOrigin DecodeOrigin(Source source) => source switch
    {
        { Kind: "player", Body: null, Location: null, Index: null, Overworld: null } => new PlayerEntityOrigin(),
        { Kind: "resident", Body: null, Location: null, Index: { } row, Overworld: null } => new ResidentEntityOrigin(row),
        { Kind: "follower", Body: null, Location: null, Index: { } member, Overworld: null } => new FollowerEntityOrigin(member),
        { Kind: "allocation", Body: null, Location: { } location, Index: null, Overworld: null } => new AllocatedEntityOrigin(location),
        { Kind: "raft", Body: null, Location: null, Index: null, Overworld: null } => new RaftEntityOrigin(),
        _ => throw new SaveFormatException("save-shape"),
    };

    private static Source EncodeAction(FieldActionSource source) => source switch
    {
        InitialEntityActions => new("initial"),
        ProgramEntityActions program => new("program", Location: program.Location),
        ZoneEntityActions zone => new("zone", Body: zone.Body, Index: zone.Event),
        CaravanFollowerActions caravan => new("caravan", Index: caravan.Order, Overworld: caravan.Overworld),
        FieldStepActions => new("field-step"),
        _ => throw new SaveFormatException("save-shape"),
    };

    private static FieldActionSource DecodeAction(Source source) => source switch
    {
        { Kind: "initial", Body: null, Location: null, Index: null, Overworld: null } => new InitialEntityActions(),
        { Kind: "program", Body: null, Location: { } location, Index: null, Overworld: null } => new ProgramEntityActions(location),
        { Kind: "zone", Body: { } body, Location: null, Index: { } row, Overworld: null } => new ZoneEntityActions(body, row),
        { Kind: "caravan", Body: null, Location: null, Index: { } order, Overworld: { } overworld } =>
            new CaravanFollowerActions(order, overworld),
        { Kind: "field-step", Body: null, Location: null, Index: null, Overworld: null } => new FieldStepActions(),
        _ => throw new SaveFormatException("save-shape"),
    };

    internal static void Configure(JsonTypeInfo info)
    {
        // Reuse value records, but keep calculated accessors out of the wire contract.
        for (int i = info.Properties.Count - 1; i >= 0; i--)
        {
            var property = info.Properties[i];
            if (property.Set is null)
            {
                info.Properties.RemoveAt(i);
                continue;
            }
            property.IsRequired = info.Type switch
            {
                _ when info.Type == typeof(Source) => property.Name == "kind",
                _ when info.Type == typeof(Visit) => property.Name is not ("roof" or "berth"),
                _ when info.Type == typeof(Actor) => property.Name is not ("follower" or "actionSource" or "sprite" or "sharedSpriteSlot"),
                _ when info.Type == typeof(SettledFieldRegisters) => property.Name is not ("entityServices" or "itemReceipt" or
                    "dialogueItem" or "speaker" or "partyLists" or "cameraTarget" or "cameraEntitySlot" or "colors"),
                _ when info.Type == typeof(FieldMapBody) => property.Name != "setup",
                _ when info.Type == typeof(FieldPartySnapshot) => property.Name != "missingCandidateAllyWord",
                _ when info.Type == typeof(LogicalViewAxis) => property.Name != "destination",
                _ when info.Type == typeof(SavedAlly) => property.Name is not ("baseMove" or "currentStats"),
                _ when info.Type == typeof(BattleCurrentStats) => false,
                _ => true,
            };
        }
    }

    internal sealed class MapConverter : JsonConverter<MapId>
    {
        public override MapId Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) => new(ReadId(ref reader));
        public override void Write(Utf8JsonWriter writer, MapId value, JsonSerializerOptions options) => writer.WriteStringValue(value.Value);
    }

    internal sealed class SetupConverter : JsonConverter<MapSetupId>
    {
        public override MapSetupId Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) => new(ReadId(ref reader));
        public override void Write(Utf8JsonWriter writer, MapSetupId value, JsonSerializerOptions options) => writer.WriteStringValue(value.Value);
    }

    internal sealed class EntityConverter : JsonConverter<EntityRef>
    {
        public override EntityRef Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) => new(ReadId(ref reader));
        public override void Write(Utf8JsonWriter writer, EntityRef value, JsonSerializerOptions options) => writer.WriteStringValue(value.Value);
        public override EntityRef ReadAsPropertyName(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) =>
            new(ReadId(ref reader));
        public override void WriteAsPropertyName(Utf8JsonWriter writer, EntityRef value, JsonSerializerOptions options) =>
            writer.WritePropertyName(value.Value);
    }

    private static string ReadId(ref Utf8JsonReader reader) =>
        reader.TokenType is JsonTokenType.String or JsonTokenType.PropertyName && reader.GetString() is { Length: > 0 } id
            ? id : throw new JsonException();
}
