using System.Text.Json;
using System.Text.Json.Serialization;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Content.Persistence;

// Finite version2 wire recipe. No runtime types, paths, computed properties or constructor defaults.
internal static class BattleSaveWire
{
    internal sealed record Record(int FormatVersion, int Slot, long Sequence, DateTimeOffset SavedAt, string Kind, Data Data);
    internal sealed record Data(SaveCompatibility Compatibility, int BattleRecipeRevision, SavedBattleRoute Route, Battle Battle, Story Story);
    internal sealed record Battle(string Encounter, IReadOnlyList<Actor> Actors, uint MainSeed, uint ThinkingSeed,
        int Round, IReadOnlyList<Turn> Queue, int Cursor, uint Gold, Options Options,
        Presence<SavedBattleRegions> Regions, Presence<Credit> DeathCredit, Custody Custody, Presence<Visuals> Visuals);
    internal sealed record Actor(string ActorId, Presence<MapPosition> Position, MapPosition DeploymentPosition,
        byte Facing, ushort Hp, byte Mp, Presence<byte> Exp, Presence<ushort> Kills, Presence<ushort> Defeats,
        ushort Status, bool Initialized, byte Attack, Presence<ushort> ActivationWord, byte AiMemory,
        Presence<byte> PrimaryOrder, Presence<byte> SecondaryOrder, Presence<string> LastTarget,
        Presence<Progress> Progress, Presence<Loadout> Loadout);
    internal sealed record Turn(Presence<string> Actor, byte AlteredAgility);
    internal sealed record Options(byte Difficulty, bool AllyAutoBattle, bool OpponentControl, Presence<ushort> MissingCandidateAllyWord);
    internal sealed record Credit(Presence<string> Ally);
    internal sealed record Custody(IReadOnlyList<byte> OnceFlags, IReadOnlyList<SavedDeal> Deals);
    internal sealed record Visual(int Slot, string Appearance, MapPosition Position, byte Facing);
    internal sealed record Visuals(IReadOnlyDictionary<string, int> Aliases, IReadOnlyList<Visual> Records);
    internal sealed record Loadout(IReadOnlyList<ushort> Items, IReadOnlyList<byte> Spells);
    internal sealed record Progress(byte Level, ushort MaxHp, byte MaxMp, byte BaseAttack, byte Defense, byte Agility,
        IReadOnlyList<SpellRef> Spells, Presence<Loadout> SourceLoadout);
    internal sealed record Retained(string Actor, ushort Hp, byte Mp, Presence<byte> Exp, Presence<ushort> Kills,
        Presence<ushort> Defeats, ushort Status, Presence<MapPosition> PositionOverride, Presence<Progress> Progress,
        Presence<Loadout> SourceLoadout, Presence<ushort> ActivationWord, Presence<bool> Initialized, bool Absent, bool RetainDeadPosition);
    internal sealed record Player(string Entity, ushort Speed, IReadOnlyList<Retained> RetainedAllies);
    internal sealed record Palette(ushort Color2, ushort Color3);
    internal sealed record Display(byte Period, Palette Base, Palette Current, FullFadeVisibility Visibility);
    internal sealed record Text(bool Open, byte X, byte Y, int Row, int AnimationLength, int AnimationCounter,
        bool Moving, int Indicator, bool IndicatorVisible, int OriginY, int DestinationY);
    internal sealed record Axis(int Position, Presence<int> Destination, int Speed);
    internal sealed record View(ExplorationViewArea Area, Presence<int> TargetSlot, Axis AX, Axis AY, Axis BX, Axis BY,
        int FollowCounter, bool HideWindows);
    internal sealed record Anchor(string Map, MapPosition Position, byte Facing);
    internal sealed record Story(IReadOnlyList<int> Flags, IReadOnlyDictionary<string, string> Names, short StepCounter,
        string EgressMap, int TextCursor, ExplorationTextSettings TextSettings, Display Display, Text LogicalText,
        View LogicalView, Presence<string> Speaker, Presence<bool> EntityServices, bool WindowFixPending,
        Presence<Anchor> SaveAnchor, SavedBattleMusic Music, Player Player,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] SavedRaft? Raft = null);

    internal static Record Encode(BattleSaveRecord record) => new(2, record.Slot, record.Sequence, record.SavedAt,
        "battle-player-input", new(record.Data.Compatibility, record.Data.BattleRecipeRevision, record.Data.Route,
            Encode(record.Data.Battle), Encode(record.Data.Story)));
    internal static BattleSaveRecord Decode(Record record)
    {
        if (record.Kind != "battle-player-input") throw new SaveFormatException("save-kind");
        // These two strings become MapId values in A's restore admission. Apply that existing
        // scalar contract here, before handing untrusted bytes to the typed validator.
        _ = new MapId(record.Data.Route.Map);
        _ = new MapId(record.Data.Story.EgressMap);
        return new(record.Slot, record.Sequence, record.SavedAt, new(record.Data.Compatibility, record.Data.BattleRecipeRevision,
            record.Data.Route, Decode(record.Data.Battle), Decode(record.Data.Story)));
    }

    private static Presence<T> Optional<T>(T? value) where T : struct => value.HasValue ? new(value.Value) : default;
    private static Presence<TWire> Optional<T, TWire>(T? value, Func<T, TWire> encode) where T : class =>
        value is null ? default : new(encode(value));
    private static T? Value<T>(Presence<T> value) where T : struct => value.Present ? value.Value : null;
    private static T? Value<TWire, T>(Presence<TWire> value, Func<TWire, T> decode) where T : class =>
        value.Present ? decode(value.Value) : null;
    private static Presence<string> String(string? value) => value is null ? default : new(value);
    private static string? String(Presence<string> value) => value.Present ? value.Value : null;
    private static Loadout Encode(BattleSourceLoadout value) => new(value.Items, value.Spells);
    private static BattleSourceLoadout Decode(Loadout value) => new(value.Items, value.Spells);
    private static Progress Encode(BattleActorProgress p) => new(p.Level, p.MaxHp, p.MaxMp, p.BaseAttack, p.Defense,
        p.Agility, p.Spells, Optional(p.SourceLoadout, Encode));
    private static BattleActorProgress Decode(Progress p) => new(p.Level, p.MaxHp, p.MaxMp, p.BaseAttack, p.Defense,
        p.Agility, p.Spells, Value(p.SourceLoadout, Decode));
    private static Axis Encode(LogicalViewAxis a) => new(a.Position, Optional(a.Destination), a.Speed);
    private static LogicalViewAxis Decode(Axis a) => new(a.Position, Value(a.Destination), a.Speed);
    private static Battle Encode(BattleContinuationData b) => new(b.Encounter, b.Actors.Select(a => new Actor(a.Actor,
        Optional(a.Position, p => p), a.DeploymentPosition, a.Facing, a.Hp, a.Mp, Optional(a.Exp), Optional(a.Kills), Optional(a.Defeats),
        a.Status, a.Initialized, a.Attack, Optional(a.ActivationWord), a.AiMemory, Optional(a.PrimaryOrder), Optional(a.SecondaryOrder),
        String(a.LastTarget), Optional(a.Progress, Encode), Optional(a.Loadout, Encode))).ToArray(),
        b.MainSeed, b.ThinkingSeed, b.Round, b.Queue.Select(t => new Turn(String(t.Actor), t.AlteredAgility)).ToArray(), b.Cursor, b.Gold,
        new(b.Options.Difficulty, b.Options.AllyAutoBattle, b.Options.OpponentControl, Optional(b.Options.MissingCandidateAllyWord)),
        Optional(b.Regions, r => r), Optional(b.DeathCredit, c => new Credit(String(c.Ally?.Value))),
        new(b.Custody.OnceFlags, b.Custody.Deals.Select(p => new SavedDeal(p.Key, p.Value)).ToArray()),
        Optional(b.Visuals, v => new Visuals(v.Aliases, v.Records.Select(r => new Visual(r.Slot, r.Appearance.Value, r.Position, r.Facing)).ToArray())));
    private static BattleContinuationData Decode(Battle b)
    {
        // The custody constructor sorts/deduplicates; reject noncanonical input before it can normalize it.
        if (!b.Custody.OnceFlags.SequenceEqual(b.Custody.OnceFlags.Distinct().Order()) ||
            b.Custody.Deals.Select(d => d.Item).Distinct().Count() != b.Custody.Deals.Count)
            throw new SaveFormatException("save-battle-custody");
        return new(b.Encounter, b.Actors.Select(a => new SavedBattleActor(a.ActorId, Value(a.Position, p => p),
            a.DeploymentPosition, a.Facing, a.Hp, a.Mp, Value(a.Exp), Value(a.Kills), Value(a.Defeats), a.Status,
            a.Initialized, a.Attack, Value(a.ActivationWord), a.AiMemory, Value(a.PrimaryOrder), Value(a.SecondaryOrder),
            String(a.LastTarget), Value(a.Progress, Decode), Value(a.Loadout, Decode))).ToArray(),
            b.MainSeed, b.ThinkingSeed, b.Round, b.Queue.Select(t => new SavedBattleTurn(String(t.Actor), t.AlteredAgility)).ToArray(),
            b.Cursor, b.Gold, new(b.Options.Difficulty, b.Options.AllyAutoBattle, b.Options.OpponentControl, Value(b.Options.MissingCandidateAllyWord)),
            Value(b.Regions, r => r), Value(b.DeathCredit, c => new BattleDeathCredit(c.Ally.Present ? new ActorRef(c.Ally.Value) : null)),
            new(b.Custody.OnceFlags, b.Custody.Deals.ToDictionary(d => d.Item, d => d.Count)),
            Value(b.Visuals, v => new SavedBattleVisuals(v.Aliases, v.Records.Select(r => new BattleVisual(r.Slot, new(r.Appearance), r.Position, r.Facing)).ToArray())));
    }
    private static Story Encode(SavedBattleStory s) => new(s.Flags, s.Names, s.StepCounter, s.EgressMap, s.TextCursor,
        s.TextSettings, new(s.Display.Period, new(s.Display.Base.Color2, s.Display.Base.Color3),
            new(s.Display.Current.Color2, s.Display.Current.Color3), s.Display.Visibility),
        new(s.LogicalText.Open, s.LogicalText.X, s.LogicalText.Y, s.LogicalText.Row, s.LogicalText.AnimationLength,
            s.LogicalText.AnimationCounter, s.LogicalText.Moving, s.LogicalText.Indicator, s.LogicalText.IndicatorVisible,
            s.LogicalText.OriginY, s.LogicalText.DestinationY),
        new(s.LogicalView.Area, Optional(s.LogicalView.TargetSlot), Encode(s.LogicalView.AX), Encode(s.LogicalView.AY),
            Encode(s.LogicalView.BX), Encode(s.LogicalView.BY), s.LogicalView.FollowCounter, s.LogicalView.HideWindows),
        String(s.Speaker?.Value), Optional(s.EntityServices), s.WindowFixPending,
        Optional(s.SaveAnchor, a => new Anchor(a.Map.Value, a.Position, a.Facing)), s.Music,
        new(s.Player.Entity, s.Player.Speed, s.Player.RetainedAllies.Select(a => new Retained(a.Actor.Value, a.Hp, a.Mp,
            Optional(a.Exp), Optional(a.Kills), Optional(a.Defeats), a.Status, Optional(a.PositionOverride, p => p),
            Optional(a.Progress, Encode), Optional(a.SourceLoadout, Encode), Optional(a.ActivationWord), Optional(a.Initialized),
            a.Absent, a.RetainDeadPosition)).ToArray()), s.Raft);
    private static SavedBattleStory Decode(Story s) => new(s.Flags, s.Names, s.StepCounter, s.EgressMap, s.TextCursor,
        s.TextSettings, new(s.Display.Period, new(s.Display.Base.Color2, s.Display.Base.Color3),
            new(s.Display.Current.Color2, s.Display.Current.Color3), s.Display.Visibility),
        new(s.LogicalText.Open, s.LogicalText.X, s.LogicalText.Y, s.LogicalText.Row, s.LogicalText.AnimationLength,
            s.LogicalText.AnimationCounter, s.LogicalText.Moving, s.LogicalText.Indicator, s.LogicalText.IndicatorVisible,
            s.LogicalText.OriginY, s.LogicalText.DestinationY),
        new(s.LogicalView.Area, Value(s.LogicalView.TargetSlot), Decode(s.LogicalView.AX), Decode(s.LogicalView.AY),
            Decode(s.LogicalView.BX), Decode(s.LogicalView.BY), s.LogicalView.FollowCounter, s.LogicalView.HideWindows),
        s.Speaker.Present ? new EntityRef(s.Speaker.Value) : null, Value(s.EntityServices), s.WindowFixPending,
        Value(s.SaveAnchor, a => new FieldReturnAnchor(new(a.Map), a.Position, a.Facing)), s.Music,
        new(s.Player.Entity, s.Player.Speed, s.Player.RetainedAllies.Select(a => new BattleActorStartInput(new(a.Actor), a.Hp, a.Mp,
            Value(a.Exp), Value(a.Kills), Value(a.Defeats), a.Status, Value(a.PositionOverride, p => p),
            Value(a.Progress, Decode), Value(a.SourceLoadout, Decode), Value(a.ActivationWord), Value(a.Initialized),
            a.Absent, a.RetainDeadPosition)).ToArray()), s.Raft);
}

// Every absence is explicit: {"present":false} or {"present":true,"value":...}.
internal readonly record struct Presence<T>
{
    internal Presence(T value) { Present = true; Value = value; }
    internal bool Present { get; }
    internal T Value { get; } = default!;
}

internal sealed class PresenceConverter<T> : JsonConverter<Presence<T>>
{
    public override Presence<T> Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("present", out var present) ||
            present.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw new SaveFormatException("save-presence");
        bool has = present.GetBoolean();
        if (root.EnumerateObject().Count() != (has ? 2 : 1) || has && !root.TryGetProperty("value", out _))
            throw new SaveFormatException("save-presence");
        return has ? new(root.GetProperty("value").Deserialize<T>(options) ?? throw new SaveFormatException("save-null-value")) : default;
    }
    public override void Write(Utf8JsonWriter writer, Presence<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject(); writer.WriteBoolean("present", value.Present);
        if (value.Present) { writer.WritePropertyName("value"); JsonSerializer.Serialize(writer, value.Value, options); }
        writer.WriteEndObject();
    }
}

internal sealed class BattlePositionConverter : JsonConverter<MapPosition>
{
    private sealed record Position(int X, int Y);
    public override MapPosition Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        var value = JsonSerializer.Deserialize<Position>(ref reader, options) ?? throw new SaveFormatException("save-null-value");
        return new(value.X, value.Y);
    }
    public override void Write(Utf8JsonWriter writer, MapPosition value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, new Position(value.X, value.Y), options);
}
