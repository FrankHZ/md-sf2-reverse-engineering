using System.Numerics;
using static H4Comparison.Operands;
using static H4Comparison.ResourceOperands;
using static H4Comparison.SceneOperands;
using static H4Comparison.InventoryOperands;

namespace H4Comparison;

internal sealed class ResourceInventory(CountedChecks checks, ResourceContext visits, object? start)
{
    private readonly InventoryBatch batch = new(checks);
    private readonly HashSet<string> enabled = InventoryItems(At(start, "enabled")).OfType<string>().ToHashSet();
    private readonly int digits = (int)(BigInteger)At(start, "integerDigitLimit")!;
    private readonly object? binding = At(start, "mapBinding");
    private readonly bool hasRequirements = InventoryTruth(At(start, "present"));
    private readonly Dictionary<MapKey, object?> definitions = [];
    private readonly Dictionary<MapKey, object?> visuals = [];
    private readonly Dictionary<MapKey, object?> portraits = [];
    private readonly HashSet<MapKey> mapKeys = InventoryItems(At(start, "maps")).Select(Hash).ToHashSet();
    private int scopeId;

    private readonly struct MapKey(object? value) : IEquatable<MapKey>
    {
        private readonly object? value = value;
        public bool Equals(MapKey other) => ReferenceEquals(value, other.value) || Equal(value, other.value);
        public override bool Equals(object? other) => other is MapKey key && Equals(key);
        public override int GetHashCode() => new Operands.Key(value).GetHashCode();
    }
    private static MapKey Hash(object? value) { HashKey(value); return new MapKey(value); }
    private InventoryScope Scope(string name, string[] families, int parent = -1) => new(++scopeId, families, name, parent);
    private static object? Fetch(Dictionary<MapKey, object?> rows, object? key) =>
        rows.TryGetValue(Hash(key), out var value) ? value : throw new OperandError("KeyError");
    private InventoryStep Write(string kind, object? key, InventoryScope scope) =>
        new(Dict(("kind", kind), ("key", key)), scope);
    private InventoryStep Check(string family, string name, object? value, InventoryScope scope, object? identity = null) =>
        new(Dict(("kind", "check")), scope, _ => checks.Check(family, name, value, BigInteger.One, identity: identity));
    private InventoryStep Member(string kind, object? key, string family, string name, InventoryScope scope) =>
        new(Dict(("kind", kind), ("key", key)), scope, receipt =>
        {
            if (InventoryTruth(At(receipt, "fresh"))) checks.Check(family, name,
                InventoryTruth(At(receipt, "present")) ? true : null, BigInteger.One,
                identity: kind == "layer" ? null : At(receipt, "key"));
        });

    public void Start()
    {
        foreach (var family in new[] { "map", "entity" })
            checks.Check(family, "independent reached requirement channel", hasRequirements ? true : null, BigInteger.One);
    }
    public void Coverage(object? complete) => checks.Check("map", "every reached field map has delivered layer inventory",
        hasRequirements ? complete : null, BigInteger.One);
    public void Finish(object? any)
    {
        checks.Check("entity", "independent reached visible logical inventory", InventoryTruth(any) ? true : null, BigInteger.One);
        definitions.Clear(); visuals.Clear(); portraits.Clear(); visits.Release();
    }
    public void Sources(object? rows)
    {
        foreach (var row in InventoryItems(rows))
        {
            var table = At(row, "kind") switch { "definition" => definitions, "visual" => visuals,
                "portrait" => portraits, _ => throw new InvalidDataException("Inventory source kind") };
            table[Hash(At(row, "key"))] = Prepare(At(row, "value"));
        }
    }
    public object Acknowledge(object? message) => batch.Acknowledge(message);
    public object Rows(string channel, object? rows)
    {
        var scope = Scope("", []);
        return batch.Begin(InventoryBatch.Guard(() => RowsCore(channel, rows, scope), scope));
    }

    private IEnumerable<InventoryStep> RowsCore(string channel, object? rows, InventoryScope outer)
    {
        var index = 0;
        foreach (var record in InventoryItems(rows))
        {
            var row = Prepare(record);
            switch (channel)
            {
                case "fields":
                    if (mapKeys.Contains(Hash(Get(Default(row, "state", Dict()), "map"))))
                        yield return Write("field-map", Field(Field(row, "state"), "map"), outer);
                    break;
                case "observed":
                    if (Equal(Get(row, "kind"), "map")) yield return Write("observed-map", Get(Field(row, "identity"), "map"), outer);
                    break;
                case "phases":
                    var identity = Field(row, "identity");
                    // Keep the two original expression/mutation boundaries separate.
                    yield return Write("phase", Key(Field(identity, "visit"), Field(row, "kind"), Field(identity, "phase")), outer);
                    yield return Write("group", Key(Field(identity, "visit"), Field(row, "kind")), outer);
                    break;
                case "entities":
                    if (Equal(Field(row, "kind"), "entity"))
                    {
                        var i = Field(row, "identity");
                        yield return Write("required-entity", Key(Field(i, "visit"), Field(i, "phase"),
                            Get(row, "subject"), Get(row, "slot"), Field(row, "expected")), outer);
                    }
                    break;
                case "portraits":
                    if (Equal(Field(row, "kind"), "portrait"))
                        foreach (var step in InventoryBatch.Guard(() => RequiredPortrait(row), Scope("portrait inventory", ["entity"]))) yield return step;
                    break;
                case "tiles":
                    if (Equal(Field(row, "kind"), "map"))
                        foreach (var step in InventoryBatch.Guard(() => RequiredTile(row), Scope("tile inventory", ["map"]))) yield return step;
                    break;
                case "definitions":
                    yield return new InventoryStep(Dict(("kind", "definition"), ("key", Field(row, "id")), ("index", new BigInteger(index))), outer);
                    break;
                case "layers":
                    if (Equal(Field(row, "kind"), "map"))
                    {
                        var i = Field(row, "identity");
                        yield return Write("required-layer", Key(Field(i, "visit"), Field(i, "phase"),
                            Get(row, "layer"), Get(row, "highPriority"), Get(row, "subject")), outer);
                    }
                    break;
                case "states":
                    var draw = Scope("logical draw occurrence", ["map", "entity"]);
                    foreach (var step in InventoryBatch.Guard(() => State(Default(row, "state", Dict()), draw), draw)) yield return step;
                    break;
                default: throw new InvalidDataException("Inventory input channel");
            }
            index++;
        }
    }

    private IEnumerable<InventoryStep> RequiredPortrait(object? row)
    {
        var i = Field(row, "identity");
        yield return Write("required-portrait", InventoryGeometry.PortraitKey(Field(i, "visit"), Field(i, "phase"), Field(row, "expected")),
            Scope("portrait inventory", ["entity"]));
    }
    private IEnumerable<InventoryStep> RequiredTile(object? row)
    {
        var i = Field(row, "identity"); var want = Field(row, "expected");
        yield return Write("required-tile", Key(Field(i, "visit"), Field(i, "phase"), Get(row, "layer"),
            Get(row, "highPriority"), Get(row, "subject"), Get(row, "pass"),
            Field(want, "block"), Field(want, "tile"), Field(want, "word")), Scope("tile inventory", ["map"]));
    }

    private IEnumerable<InventoryStep> State(object? state, InventoryScope draw)
    {
        var projection = Or(Get(state, "cameraProjection"), Dict());
        var presentation = Or(Get(state, "presentation"), Dict());
        if (!InventoryTruth(projection) || !Equal(Get(projection, "revision"), Get(state, "revision"))
            || !Equal(Get(projection, "map"), Get(state, "map"))) yield break;
        var visit = visits.LatestSequence(Field(state, "observationSequence"));
        var phase = Str(Or(Get(presentation, "activeCue"), "<null>"));
        var layers = new[] { "background", "foreground", "backgroundHigh", "foregroundHigh" }
            .Select(name => (name, Get(projection, name))).ToList();
        layers.AddRange(InventoryItems(Default(projection, "occlusionDraws", new List<object?>())).Select(row => ("occlusion", row)));
        foreach (var (name, layer) in enabled.Contains("map") ? layers : [])
        {
            if (!InventoryTruth(layer) || !InventoryTruth(Get(layer, "draws"))) continue;
            var key = Key(visit, phase, name, Get(layer, "highPriority"), Get(layer, "subject"));
            yield return Member("layer", key, "map", "independent reached layer/pass/subject requirement", draw);
            var scope = Scope("independent reached tile", ["map"], draw.Id);
            foreach (var step in InventoryBatch.Guard(() => Layer(state, layer, name, key, scope), scope)) yield return step;
        }
        if (enabled.Contains("entity"))
            foreach (var logical in InventoryItems(Or(Get(state, "entities"), new List<object?>())))
            {
                var scope = Scope("logical entity occurrence", ["entity"], draw.Id);
                foreach (var step in InventoryBatch.Guard(() => Entity(state, projection, presentation, logical, visit, phase, scope), scope)) yield return step;
            }
        var portrait = Or(Get(state, "portraitProjection"), Dict());
        if (enabled.Contains("entity") && LE(BigInteger.Zero, Default(portrait, "id", new BigInteger(-1))))
        {
            var scope = Scope("independent portrait pose", ["entity"], draw.Id);
            foreach (var step in InventoryBatch.Guard(() => Portrait(state, visit, phase, scope), scope)) yield return step;
            yield return Check("entity", "drawn portrait has logical source identity",
                state is Dictionary<string, object?> fields && fields.ContainsKey("portraitId")
                    ? Equal(Field(portrait, "id"), Field(state, "portraitId")) : null, draw);
        }
    }

    private IEnumerable<InventoryStep> Layer(object? state, object? layer, string name, object?[] key, InventoryScope scope)
    {
        var map = Field(state, "map");
        var (tiles, mutable) = InventoryGeometry.Layer(layer, name,
            () => Fetch(definitions, map), () => Field(Fetch(visuals, map), "blocks"), digits);
        if (name == "occlusion" && tiles.Count == 0)
            yield return Check("map", "independent occlusion tile operands", null, scope);
        if (mutable)
            yield return Check("map", "reached mutable region composed delivery",
                binding is not null && (Equal(map, "map-3") || Equal(map, "map-19")) ? Field(binding, "value") : null, scope);
        foreach (var tile in tiles)
        {
            var tileKey = key.Concat(new[] { Get(layer, "pass"), Field(tile, "block"), Field(tile, "tile"), Field(tile, "word") }).ToArray();
            yield return Member("tile", tileKey, "map", "independent reached block/tile requirement", scope);
        }
    }

    private IEnumerable<InventoryStep> Entity(object? state, object? projection, object? presentation,
        object? logical, object? visit, string phase, InventoryScope scope)
    {
        var x = Subtract(Real(Field(logical, "x")) / 16, Field(presentation, "cameraX"));
        var y = Subtract(Real(Field(logical, "y")) / 16, Field(presentation, "cameraY"));
        if (!(InventoryTruth(Field(logical, "Visible")) && Less(BigInteger.Zero, Add(x, new BigInteger(24)))
            && Less(BigInteger.Zero, Add(y, new BigInteger(24))) && Less(x, new BigInteger(320)) && Less(y, new BigInteger(192)))) yield break;
        var nod = Or(Get(state, "nod"), Dict()); var subject = Get(nod, "Entity");
        if (subject is Dictionary<string, object?>) subject = Get(subject, "Value");
        var sprite = Field(logical, "sprite");
        var facing = Field(logical, "facing"); var animation = Field(logical, "animationCounter");
        var want = Dict(("sprite", sprite),
            ("direction", new BigInteger(Equal(facing, BigInteger.One) ? 0 : Equal(facing, new BigInteger(3)) ? 2 : 1)),
            ("half", new BigInteger(Less(new BigInteger(15), animation) && Less(animation, new BigInteger(128)) ? 1 : 0)),
            ("nod", Equal(subject, Field(logical, "id")) && InventoryTruth(Get(nod, "Lowered"))));
        var key = Key(visit, phase, Field(logical, "id"), Field(logical, "slot"), want);
        object? generated;
        try { generated = Generated(state, projection, logical, visit, phase, want); }
        catch (OperandError error) { generated = Dict(("error", error.Kind), ("message", error.Detail)); }
        yield return new InventoryStep(Dict(("kind", "entity"), ("key", key), ("generated", generated)), scope, receipt =>
        {
            if (InventoryTruth(At(receipt, "fresh"))) checks.Check("entity", "independent visible logical subject/pose requirement",
                InventoryTruth(At(receipt, "present")) ? true : null, BigInteger.One, identity: At(receipt, "key"));
        });
    }

    private static object? Generated(object? state, object? projection, object? logical, object? visit, string phase, object want)
    {
        var actor = InventoryItems(Default(projection, "actors", new List<object?>())).FirstOrDefault(a =>
            Equal(Get(a, "entity"), Field(logical, "id")) && Equal(Get(a, "slot"), Field(logical, "slot")) && InventoryTruth(Get(a, "visible")));
        if (!InventoryTruth(actor) || Get(actor, "resourceSelector") is null) return null;
        var identity = new[] { "sessionId", "revision", "observationSequence", "simulationTick", "token", "drawSequence" }
            .ToDictionary(name => name, name => Get(projection, name));
        identity.Add("visit", visit); identity.Add("map", Field(state, "map")); identity.Add("phase", phase);
        var required = Dict(("identity", identity), ("kind", "entity"), ("subject", Field(logical, "id")),
            ("slot", Field(logical, "slot")), ("expected", want), ("_captureLocator", Dict(
                ("source", "retained camera projection"), ("state", Get(state, "_captureLocator")),
                ("subject", Field(logical, "id")), ("slot", Field(logical, "slot")))));
        return Dict(("requirement", required), ("used", Field(actor, "resourceSelector")));
    }

    private IEnumerable<InventoryStep> Portrait(object? state, object? visit, string phase, InventoryScope scope)
    {
        var want = InventoryGeometry.Portrait(state, id => Fetch(portraits, id), digits);
        yield return Member("portrait", InventoryGeometry.PortraitKey(visit, phase, want), "entity",
            "independent reached portrait pose requirement", scope);
    }
}
