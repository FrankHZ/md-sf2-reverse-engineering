using System.Numerics;
using static H4Comparison.Operands;
using static H4Comparison.MaterialOperands;

namespace H4Comparison;

internal sealed class AudioMaterials(MaterialReport report, SceneMaterials scene, object? pins)
{
    private object? summary, cue, selected, asset, record, runtime, owner, ownerIndex, begin, end;
    private bool format, cut, valid;
    private int selectedIndex;
    private readonly Dictionary<string, List<object?>> headers = new() { ["audio"] = [], ["library"] = [], ["provenance"] = [] };
    private readonly Dictionary<string, object?> tails = [];
    private void Check(string name, object? value, object? source) =>
        summary = MaterialReport.Combine(summary, report.Check(name, value, source));
    private IEnumerable<(int Index, object? Row)> Source(string kind)
    {
        var rows = headers[kind];
        for (var i = 0; i < rows.Count; i++) yield return (i, rows[i]);
        Fact(tails[kind]);
    }
    public object? Handle(string op, object? message)
    {
        switch (op)
        {
            case "audio-start":
                summary = scene.Initial;
                Check("explicit pinned clean asset checkout", true, At(message, "pins")); return null;
            case "owner":
                var value = At(message, "owner"); var name = Text(At(message, "name"));
                Check("audio source pins " + name,
                    Equal(At(value, "romSha256"), At(pins, "rom")) && Equal(At(value, "sf2disasmCommit"), At(pins, "upstream"))
                    && Equal(At(value, "emulator"), Dict(("name", "BizHawk"), ("version", "2.11.1"), ("core", "Genplus-gx"),
                        ("commit", "bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5"))), "manifests/" + name); return null;
            case "starts-filter":
                return Iterate(At(message, "rows")).Select(row => (object?)Equal(At(At(row, "receipt"), "Operation"), "started")).ToList();
            case "headers":
                headers[Text(At(message, "kind"))].AddRange(Iterate(At(message, "rows"))); return null;
            case "headers-end":
                tails[Text(At(message, "kind"))] = At(message, "tail"); return null;
            case "cue":
                cue = At(message, "cue");
                var audio = Source("audio").Where(p => Equal(At(p.Row, "cue"), cue)).ToList();
                var library = Source("library").Where(p => Equal(At(p.Row, "kind"), "audio") && Equal(At(p.Row, "cue"), cue)).ToList();
                var provenance = Source("provenance").Where(p => Equal(At(At(p.Row, "asset"), "cue"), cue)).ToList();
                var unique = audio.Count == 1 && library.Count == 1 && provenance.Count == 1;
                Check("unique reached audio origin " + Text(cue), audio.Count > 0 && library.Count > 0 && provenance.Count > 0 ? unique : null,
                    "world audio -> library catalog -> provenance record");
                if (!unique) return null;
                selectedIndex = audio[0].Index;
                return new object?[] { new BigInteger(selectedIndex), new BigInteger(library[0].Index), new BigInteger(provenance[0].Index) }.ToList();
            case "cue-detail":
                selected = At(message, "selected"); asset = At(message, "asset"); record = At(message, "record");
                owner = At(message, "owner"); ownerIndex = At(message, "index"); runtime = At(asset, "runtime"); return null;
            case "runtime-format":
                format = Equal(At(message, "format"), new object?[] { new BigInteger(2), At(runtime, "channels"),
                    At(runtime, "sampleRate"), At(runtime, "sampleFrames") }.ToList()); return null;
            case "capture-bytes":
                begin = At(message, "begin"); end = At(message, "end");
                cut = BytesEqual(At(message, "measurement")); return cut;
            case "capture-format":
                cut = Equal(At(message, "format"), new object?[] { new BigInteger(2), At(runtime, "channels"), At(runtime, "sampleRate") }.ToList()); return null;
            case "valid-start": valid = Equal(At(record, "asset"), asset) && format && cut; return valid;
            case "capture-digest":
                valid = Equal(At(message, "digest"), At(At(asset, "source"), "sha256"))
                    && Equal(Fact(At(message, "span")), At(runtime, "sampleFrames")); return valid;
            case "selected-bytes": valid = BytesEqual(At(message, "measurement")); return valid;
            case "pcm-digest":
                valid = Equal(At(message, "digest"), At(selected, "sha256"))
                    && new[] { "sampleRate", "channels", "sampleFrames", "loopBegin", "loopEnd" }.All(k => Equal(At(selected, k), At(runtime, k)))
                    && Equal(new object?[] { At(selected, "command"), At(selected, "timerB") }.ToList(),
                        new object?[] { At(asset, "command"), At(asset, "timerB") }.ToList()); return null;
            case "valid-finish":
                Check("reached original capture cut/runtime PCM " + Text(cue), valid,
                    Dict(("file", "manifests/" + Text(owner)), ("record", ownerIndex), ("assetId", At(asset, "assetId")),
                        ("captureStartSample", begin), ("captureEndSample", end), ("loopBegin", At(runtime, "loopBegin")), ("loopEnd", At(runtime, "loopEnd")))); return null;
            case "receipts":
                foreach (var pair in Iterate(At(message, "rows"))) Receipt(pair); return null;
            case "audio-finish": report.Audio = Truth(At(message, "any")) ? summary : null; return null;
            default: throw new InvalidDataException("Unknown audio material phase");
        }
    }
    private void Receipt(object? pair)
    {
        var receipt = At(pair, "row");
        if (!Equal(At(receipt, "Cue"), cue)) return;
        var index = SceneOperands.Str(At(pair, "index")); var requested = Get(receipt, "RequestedTimerB");
        var choices = Source("audio").Where(p => Equal(At(p.Row, "command"), At(receipt, "Command"))).ToList();
        var exact = choices.Where(p => Equal(At(p.Row, "timerB"), requested)).ToList();
        var finite = choices.Where(p => At(p.Row, "loopBegin") is null).ToList();
        // Unique cue admission proves a different source index cannot equal the selected whole row:
        // equal whole rows have equal cue fields and would have failed that cardinality check.
        var policy = requested is null || exact.Count == 1 && exact[0].Index == selectedIndex
            || exact.Count == 0 && finite.Count == 1 && finite[0].Index == selectedIndex;
        (string, string)[] fields = [("Command", "command"), ("TimerB", "timerB"), ("PcmSha256", "sha256"),
            ("SampleRate", "sampleRate"), ("Channels", "channels"), ("SampleFrames", "sampleFrames"), ("LoopBegin", "loopBegin"), ("LoopEnd", "loopEnd")];
        var matches = policy && fields.All(p => Equal(At(receipt, p.Item1), At(selected, p.Item2)));
        Check($"audio start material selection receipt{index}", matches, "SessionAudio.Select exact timer/unique finite policy");
        report.Join(Dict(("record", $"audioReceipts[{index}].receipt"), ("cue", cue), ("assetId", At(asset, "assetId")),
            ("requestedTimerB", requested), ("assetTimerB", At(asset, "timerB")), ("provenance", owner), ("sourceRecord", ownerIndex),
            ("selection", Equal(requested, At(asset, "timerB")) ? "exact" : requested is null ? "named cue" : "unique finite fallback")));
    }
}
