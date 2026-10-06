using System.Numerics;
using static H4Comparison.Operands;
using static H4Comparison.MaterialOperands;

namespace H4Comparison;

internal sealed class SceneMaterials(MaterialReport report, object? pins)
{
    private object? binding, identity;
    private object? summary = true, backgrounds = true, actors = true;
    private bool hasBackgrounds, hasActors, valid;
    private object? source, candidate, raster, asset;
    private List<object?> buckets = [];
    private readonly Dictionary<Key, object?> assets = [], rasters = [];
    public object? Initial => MaterialReport.Combine(binding, identity);
    private void Check(string name, object? value, object? owner) =>
        summary = MaterialReport.Combine(summary, report.Check(name, value, owner));
    public object? Handle(string op, object? message)
    {
        switch (op)
        {
            case "selection-start":
                var selected = At(message, "selected");
                string[] keys = ["SF2_PRIVATE_EXPLORATION_CONTENT", "SF2_PRIVATE_BATTLE_SCENE_CONTENT"];
                binding = keys.All(k => Truth(Get(selected, k))) ? true : null; return binding;
            case "selection-path":
                binding = Equal(At(message, "actual"), At(message, "expected")); return binding;
            case "selection-finish":
                Check("same-run explicit world/scene selection", binding, "process.selectedInputs"); return null;
            case "world":
                var provenance = At(message, "provenance");
                identity = Equal(At(provenance, "commit"), At(pins, "upstream"))
                    && Equal(At(provenance, "romSha256"), At(pins, "rom"))
                    && Equal(At(provenance, "repository"), At(pins, "repository"));
                Check("selected world original identity", identity, "world.provenance"); return null;
            case "pins":
                source = At(message, "source"); candidate = At(message, "candidate");
                valid = Equal(At(source, "upstreamCommit"), At(candidate, "upstreamCommit"))
                    && Equal(At(candidate, "upstreamCommit"), At(pins, "upstream"))
                    && Equal(At(source, "romSha256"), At(candidate, "romSha256"))
                    && Equal(At(candidate, "romSha256"), At(pins, "rom"))
                    && Equal(At(source, "upstreamRepository"), At(pins, "repository")); return valid;
            case "source-digest":
                valid = Equal(At(message, "value"), At(candidate, "sourceSha256"))
                    && Equal(At(candidate, "sourceSha256"), At(pins, "sourceSha256")); return valid;
            case "manifest-digest":
                valid = Equal(At(message, "value"), At(candidate, "manifestSha256"))
                    && Equal(At(candidate, "manifestSha256"), At(pins, "manifestSha256")); return valid;
            case "base-size":
                valid = Equal(At(message, "value"), At(candidate, "sceneContentBytes"))
                    && Equal(Fact(At(message, "assetCount")), At(candidate, "assetCount"))
                    && Equal(At(candidate, "assetCount"), new BigInteger(42)); return valid;
            case "pins-finish":
                Check("scene bundle original pins and recorded file identities", valid,
                    "scene-source candidate-report/manifest/source selection");
                var fingerprints = At(message, "fingerprints");
                Check("recorded historical scene extractor fingerprint",
                    Equal(At(fingerprints, "historicalCrlf"), At(candidate, "generatorArtifactSha256")),
                    Dict(("commit", At(pins, "generatorCommit")), ("components", At(pins, "components")),
                        ("representation", "historical CRLF checkout"), ("fingerprints", fingerprints)));
                valid = true; return null;
            case "base-field":
                if (valid) valid = Equal(Get(At(message, "scene"), Text(At(message, "key"))), At(message, "value"));
                return valid;
            case "base-raster":
                if (valid) valid = Equal(Get(At(message, "rasters"), Text(At(message, "key"))), At(message, "value"));
                return valid;
            case "span":
                if (valid) valid = Equal(At(message, "length"), At(At(message, "span"), "byteLength")); return valid;
            case "base-finish":
                Check("selected scene base content equality", valid,
                    "selected scene -> frozen base42 and original ROM spans"); return null;
            case "assets":
                foreach (var row in Iterate(At(message, "rows"))) assets[ResourceOperands.HashKey(At(row, "assetId"))] = row;
                return null;
            case "raster-start":
                var name = Text(At(message, "name")); raster = At(message, "raster");
                asset = Find(assets, "battle.scene." + name.Replace('/', '.'));
                buckets = Truth(asset) ? Iterate(At(asset, "buckets")).Where(b => Equal(At(b, "scale"), new BigInteger(2))).ToList() : [];
                return null;
            case "raster":
                rasters[ResourceOperands.HashKey(At(message, "name"))] = Truth(asset) && buckets.Count == 1
                    && Equal(At(asset, "source"), Dict(("assetId", "source.battle.scene.selection"), ("sha256", At(candidate, "sourceSha256"))))
                    && Equal(At(At(asset, "derivation"), "generatorArtifactSha256"), At(candidate, "generatorArtifactSha256"))
                    && Equal(At(message, "digest"), At(raster, "sha256")) && Equal(At(raster, "sha256"), At(buckets[0], "sha256"))
                    && Equal(At(message, "length"), At(buckets[0], "byteLength"))
                    && Equal(new object?[] { At(raster, "width"), At(raster, "height") }.ToList(),
                        new object?[] { At(buckets[0], "width"), At(buckets[0], "height") }.ToList()); return null;
            case "rasters-finish":
                Check("base42 embedded PNG/manifest identities", rasters.Count == 42 && rasters.Values.All(Truth),
                    "scene rasters -> scale2 buckets/source/derivation"); return null;
            case "mounted-filter":
                return Iterate(At(message, "rows")).Select(row =>
                    (object?)(Truth(Get(At(row, "scene"), "visible")) && !Truth(Get(At(row, "scene"), "fieldDeath")))).ToList();
            case "mounted":
                foreach (var pair in Iterate(At(message, "rows"))) Mount(pair);
                return null;
            case "scene-finish":
                report.Scene = hasBackgrounds ? MaterialReport.Combine(summary, backgrounds) : null;
                report.ActorWeapon = hasActors ? MaterialReport.Combine(summary, actors) : null; return null;
            default: throw new InvalidDataException("Unknown scene material phase");
        }
    }
    private void Mount(object? pair)
    {
        var row = At(pair, "row"); var index = SceneOperands.Str(At(pair, "index"));
        void Join(string key, object? name) => report.Join(Dict(("record", $"sceneObservations[{index}].scene.{key}"),
            ("resource", name), ("source", "scene-source/base42")));
        foreach (var key in new[] { "background", "backgroundWrap", "ground" })
        {
            var node = SceneOperands.Default(row, key, Dict());
            if (!Truth(Get(node, "visible"))) continue;
            var name = Get(node, "resource");
            backgrounds = MaterialReport.Combine(backgrounds, Truth(Get(node, "texturePresent")) && Truth(Find(rasters, name)));
            hasBackgrounds = true; Join(key, name);
        }
        foreach (var key in new[] { "allyResource", "enemyResource", "weaponResource" })
        {
            if (key == "enemyResource" && !Truth(Get(row, "enemyVisible")) || key == "weaponResource" && !Truth(Get(row, "weaponVisible"))) continue;
            var name = Get(row, key);
            if (!Truth(name)) continue;
            actors = MaterialReport.Combine(actors, Truth(Find(rasters, name))); hasActors = true; Join(key, name);
        }
    }
}
