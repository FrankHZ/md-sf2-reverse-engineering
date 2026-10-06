"""Selected material IO and fixed-phase transport; C# owns origin judgments."""

from __future__ import annotations

import base64
import hashlib
import json
import os
import subprocess
import wave

from sf2tool.h4_dotnet import ResourceComparison, ResourceProcessError, _batches, _selected
from sf2tool.paths import repo_path
from sf2tool.remake_assets import AssetPreflightError

_ERRORS = {
    "AttributeError": AttributeError,
    "KeyError": KeyError,
    "TypeError": TypeError,
    "ValueError": ValueError,
    "IndexError": IndexError,
    "OverflowError": OverflowError,
}


def validate_reply(operation, result):
    def count(value):
        return type(value) is int and value >= 0

    def bad():
        raise ResourceProcessError(f"Malformed H4 material reply for {operation}")

    if not isinstance(result, dict) or set(result) != {
        "sequence",
        "offset",
        "total",
        "rows",
        "done",
        "scene",
        "audio",
        "actorWeapon",
        "control",
        "failure",
    }:
        bad()
    if (
        not all(count(result[k]) for k in ("sequence", "offset", "total"))
        or not isinstance(result["done"], bool)
        or not isinstance(result["rows"], list)
        or len(result["rows"]) > 256
        or any(
            result[k] is not None and type(result[k]) is not bool
            for k in ("scene", "audio", "actorWeapon")
        )
    ):
        bad()
    for item in result["rows"]:
        if (
            not isinstance(item, dict)
            or set(item) != {"channel", "row"}
            or item["channel"] not in ("checks", "joins")
            or not isinstance(item["row"], dict)
        ):
            bad()
        row = item["row"]
        if item["channel"] == "checks" and (
            set(row) != {"name", "value", "source"}
            or not isinstance(row["name"], str)
            or row["value"] is not None
            and type(row["value"]) is not bool
        ):
            bad()
        if item["channel"] == "joins" and (
            set(row)
            not in (
                {"record", "resource", "source"},
                {
                    "record",
                    "cue",
                    "assetId",
                    "requestedTimerB",
                    "assetTimerB",
                    "provenance",
                    "sourceRecord",
                    "selection",
                },
            )
            or not isinstance(row["record"], str)
        ):
            bad()
    failure = result["failure"]
    if failure is not None and (
        not isinstance(failure, dict)
        or not (
            set(failure) == {"fact"}
            and count(failure["fact"])
            or set(failure) == {"kind", "message"}
            and isinstance(failure["kind"], str)
            and failure["kind"] in _ERRORS
            and (failure["message"] is None or isinstance(failure["message"], str))
        )
    ):
        bad()
    if (failure is not None or operation in ("materials-start", "materials-error")) and result[
        "control"
    ] is not None:
        bad()


class _Materials:
    def __init__(self, child, result):
        self.child, self.result = child, result
        self.errors = []
        self.sequence = 0

    def fact(self, operation):
        try:
            return {"value": operation()}
        except Exception as error:
            self.errors.append(error)
            return {"error": len(self.errors) - 1}

    def send(self, operation, **fields):
        reply = self.child.exchange(dict(op="materials-" + operation, **fields))
        self.sequence += 1
        metadata = {k: v for k, v in reply.items() if k not in ("rows", "offset", "done")}
        offset = 0
        while True:
            if (
                reply["sequence"] != self.sequence
                or reply["offset"] != offset
                or {k: v for k, v in reply.items() if k not in ("rows", "offset", "done")}
                != metadata
                or offset + len(reply["rows"]) > reply["total"]
                or reply["done"] != (offset + len(reply["rows"]) == reply["total"])
                or not reply["done"]
                and not reply["rows"]
            ):
                raise ResourceProcessError("Material result sequence/count changed during drain")
            for item in reply["rows"]:
                self.result[item["channel"]].append(item["row"])
            offset += len(reply["rows"])
            if reply["done"]:
                break
            reply = self.child.exchange(dict(op="materials-drain"))
        for key in ("scene", "audio", "actorWeapon"):
            self.result[key] = reply[key]
        failure = reply["failure"]
        if failure is not None:
            if "fact" in failure:
                if failure["fact"] >= len(self.errors):
                    raise ResourceProcessError("Unknown material factual error")
                raise self.errors[failure["fact"]]
            raise _ERRORS[failure["kind"]](
                failure["message"] or "H4 material operand: " + failure["kind"]
            )
        return reply["control"]

    def scene(self, phase, **fields):
        value = self.send("scene", phase=phase, **fields)
        if phase in {
            "selection-start",
            "selection-path",
            "pins",
            "source-digest",
            "manifest-digest",
            "base-size",
            "base-field",
            "base-raster",
            "span",
        }:
            if type(value) is not bool and not (phase == "selection-start" and value is None):
                raise ResourceProcessError("Invalid material scene continuation")
        elif phase != "mounted-filter" and value is not None:
            raise ResourceProcessError("Unexpected material scene control")
        return value

    def audio(self, phase, **fields):
        value = self.send("audio", phase=phase, **fields)
        if phase in {"capture-bytes", "valid-start", "capture-digest", "selected-bytes"}:
            if type(value) is not bool:
                raise ResourceProcessError("Invalid material audio continuation")
        elif phase not in {"cue", "starts-filter"} and value is not None:
            raise ResourceProcessError("Unexpected material audio control")
        return value

    def admitted(self, rows, family, phase, key):
        # Complete eager selection before the caller can emit any joins.
        for batch in _batches(dict(index=i, row=r) for i, r in enumerate(rows)):

            def project(row):
                value = _selected(row, (key,))
                if isinstance(value, dict) and key in value:
                    value[key] = _selected(
                        value[key], ("visible", "fieldDeath") if key == "scene" else ("Operation",)
                    )
                return value

            masks = getattr(self, family)(phase, rows=[project(p["row"]) for p in batch])
            if (
                not isinstance(masks, list)
                or len(masks) != len(batch)
                or any(type(v) is not bool for v in masks)
            ):
                raise ResourceProcessError("Invalid material admission vector")
            for pair, admitted in zip(batch, masks, strict=True):
                if admitted:
                    yield pair["index"], pair["row"][key]

    def headers(self, kind, source, projection):
        # Retain source iteration failure until C# reaches this source in cue order.
        rows = []

        def collect():
            for row in source():
                rows.append(row)
                yield projection(row)

        def send():
            for batch in _batches(collect(), flush_on_error=True):
                self.audio("headers", kind=kind, rows=batch)

        tail = self.fact(send)
        # Protocol/transport errors are never evidence or deferred source failures.
        if "error" in tail and isinstance(self.errors[tail["error"]], ResourceProcessError):
            raise self.errors[tail["error"]]
        self.audio("headers-end", kind=kind, tail=tail)
        return rows


def _digest(data):
    return hashlib.sha256(data).hexdigest().upper()


def _measure(left, right):
    return dict(
        leftBytes=len(left),
        rightBytes=len(right),
        firstDifference=next(
            (i for i, (a, b) in enumerate(zip(left, right, strict=False)) if a != b), None
        ),
    )


def _relative(root, name):
    path = (root / name).resolve()
    if not path.is_relative_to(root):
        raise ValueError("material file escapes explicit selected root")
    return path


def run(
    actual,
    selection,
    result,
    *,
    read,
    bounded_list,
    bounded_sorted,
    inspect,
    fingerprint,
    pins,
    budget=None,
):
    with ResourceComparison({}, {}, {}, {}, budget=budget, defer_start=True) as child:
        bridge = _Materials(child, result)
        try:
            _run(
                bridge,
                actual,
                selection,
                read,
                bounded_list,
                bounded_sorted,
                inspect,
                fingerprint,
                pins,
            )
        except (FileNotFoundError, subprocess.CalledProcessError) as error:
            failure = dict(
                kind="FileNotFoundError"
                if isinstance(error, FileNotFoundError)
                else "CalledProcessError"
            )
        except AssetPreflightError as error:
            failure = dict(kind="AssetPreflightError", code=error.code, field=error.field)
        except KeyError:
            failure = dict(kind="KeyError")
        except (ValueError, wave.Error) as error:
            failure = dict(
                kind="WaveError" if isinstance(error, wave.Error) else "ValueError",
                name=type(error).__name__,
            )
        else:
            return result
        bridge.send("error", error=failure)
    return result


def _run(b, actual, selection, read, bounded_list, bounded_sorted, inspect, fingerprint, pins):
    world_path, scene_path, process_path, scene_root, asset_root, commit, tree, manifest_pin = (
        selection
    )
    world_path, scene_path, process_path, scene_root, asset_root = (
        p.resolve() if p.is_absolute() else repo_path(p)
        for p in (world_path, scene_path, process_path, scene_root, asset_root)
    )
    process = read(process_path)
    selected = process.get("selectedInputs", {})
    b.send("start", pins=pins)
    if b.scene(
        "selection-start",
        selected=_selected(
            selected, ("SF2_PRIVATE_EXPLORATION_CONTENT", "SF2_PRIVATE_BATTLE_SCENE_CONTENT")
        ),
    ):
        for key, path in (
            ("SF2_PRIVATE_EXPLORATION_CONTENT", world_path),
            ("SF2_PRIVATE_BATTLE_SCENE_CONTENT", scene_path),
        ):
            if not b.scene(
                "selection-path",
                actual=os.path.normcase(str(repo_path(selected[key]).resolve())),
                expected=os.path.normcase(str(path)),
            ):
                break
    b.scene("selection-finish")
    world, scene = read(world_path), read(scene_path)
    b.scene(
        "world", provenance=_selected(world["provenance"], ("commit", "romSha256", "repository"))
    )
    manifest = read(scene_root / "manifests/presentation-assets-v1.json")
    source_path = scene_root / "source/battle-scenes/selection.json"
    source, report = read(source_path), read(scene_root / "candidate-report.json")
    base = read(scene_root / "battle-scenes.json")
    historical = {
        name: subprocess.check_output(
            ["git", "show", f"{pins['generatorCommit']}:{name}"], cwd=repo_path("")
        )
        for name in pins["components"]
    }
    fingerprints = dict(
        gitLf=fingerprint(historical),
        historicalCrlf=fingerprint(
            {name: data.replace(b"\n", b"\r\n") for name, data in historical.items()}
        ),
        current=fingerprint({name: repo_path(name).read_bytes() for name in pins["components"]}),
    )
    if (
        b.scene(
            "pins",
            source=_selected(source, ("upstreamCommit", "romSha256", "upstreamRepository")),
            candidate=_selected(
                report,
                (
                    "upstreamCommit",
                    "romSha256",
                    "sourceSha256",
                    "manifestSha256",
                    "sceneContentBytes",
                    "assetCount",
                    "generatorArtifactSha256",
                ),
            ),
        )
        and b.scene("source-digest", value=_digest(source_path.read_bytes()))
        and b.scene(
            "manifest-digest",
            value=_digest((scene_root / "manifests/presentation-assets-v1.json").read_bytes()),
        )
    ):
        b.scene(
            "base-size",
            value=(scene_root / "battle-scenes.json").stat().st_size,
            assetCount=b.fact(lambda: len(manifest["assets"])),
        )
    b.scene("pins-finish", fingerprints=fingerprints)
    matched = True
    for key, value in base.items():
        if key != "rasters":
            matched = b.scene("base-field", key=key, scene=_selected(scene, (key,)), value=value)
            if not matched:
                break
    if matched:
        for key, value in base["rasters"].items():
            if not b.scene(
                "base-raster", key=key, rasters=_selected(scene["rasters"], (key,)), value=value
            ):
                matched = False
                break
    if matched:
        for span in source["spans"]:
            length = len(base64.b64decode(span["data"], validate=True))
            if not b.scene("span", length=length, span=_selected(span, ("byteLength",))):
                break
    b.scene("base-finish")
    for batch in _batches(
        _selected(a, ("assetId", "buckets", "source", "derivation")) for a in manifest["assets"]
    ):
        b.scene("assets", rows=batch)
    for name, raster in base["rasters"].items():
        b.scene("raster-start", name=name, raster=_selected(raster, ("sha256", "width", "height")))
        payload = base64.b64decode(raster["data"], validate=True)
        b.scene("raster", name=name, digest=_digest(payload), length=len(payload))
    b.scene("rasters-finish")
    mounted = bounded_list(
        b.admitted(actual.get("sceneObservations", []), "scene", "mounted-filter", "scene")
    )
    for batch in _batches(
        dict(
            index=i,
            row=_selected(
                row,
                (
                    "background",
                    "backgroundWrap",
                    "ground",
                    "allyResource",
                    "enemyResource",
                    "weaponResource",
                    "enemyVisible",
                    "weaponVisible",
                ),
            ),
        )
        for i, row in mounted
    ):
        b.scene("mounted", rows=batch)
    b.scene("scene-finish")
    _audio(
        b,
        actual,
        world,
        asset_root,
        commit,
        tree,
        manifest_pin,
        read,
        bounded_list,
        bounded_sorted,
        inspect,
    )


def _audio(
    b,
    actual,
    world,
    asset_root,
    commit,
    tree,
    manifest_pin,
    read,
    bounded_list,
    bounded_sorted,
    inspect,
):
    inspection = inspect(
        str(asset_root),
        expected_commit=commit,
        expected_tree=tree,
        expected_manifest_sha256=manifest_pin,
    )
    catalog = json.loads(inspection.manifest_bytes)
    b.audio("audio-start", pins=dict(commit=commit, tree=tree, manifestSha256=manifest_pin))
    provenance = []
    for name, key in (
        ("audio-reached-inventory-provenance.json", "records"),
        ("audio-town-join-provenance.json", "assets"),
    ):
        owner = read(asset_root / "manifests" / name)
        b.audio(
            "owner", name=name, owner=_selected(owner, ("romSha256", "sf2disasmCommit", "emulator"))
        )
        provenance.extend((name, i, row) for i, row in enumerate(owner[key]))
    audio = world["world"]["presentation"]["audio"]
    starts = bounded_list(
        b.admitted(actual.get("audioReceipts", []), "audio", "starts-filter", "receipt")
    )
    ready = False
    for cue in bounded_sorted({row["Cue"] for _, row in starts}):
        if not ready:
            audio_rows = b.headers(
                "audio",
                lambda: audio,
                lambda row: _selected(row, ("cue", "command", "timerB", "loopBegin")),
            )
            library = b.headers(
                "library", lambda: catalog["assets"], lambda row: _selected(row, ("kind", "cue"))
            )
            b.headers(
                "provenance",
                lambda: provenance,
                lambda row: (
                    {"asset": _selected(row[2]["asset"], ("cue",))}
                    if isinstance(row[2], dict) and "asset" in row[2]
                    else _selected(row[2], ("asset",))
                ),
            )
            ready = True
        indices = b.audio("cue", cue=cue)
        if indices is None:
            continue
        if (
            not isinstance(indices, list)
            or len(indices) != 3
            or any(
                type(i) is not int or i < 0 or i >= len(rows)
                for i, rows in zip(indices, (audio_rows, library, provenance), strict=True)
            )
        ):
            raise ResourceProcessError("Invalid unique material source indices")
        selected_audio, asset = audio_rows[indices[0]], library[indices[1]]
        owner_name, owner_index, record = provenance[indices[2]]
        b.audio(
            "cue-detail",
            selected=_selected(
                selected_audio,
                (
                    "sha256",
                    "sampleRate",
                    "channels",
                    "sampleFrames",
                    "loopBegin",
                    "loopEnd",
                    "command",
                    "timerB",
                ),
            ),
            asset=asset,
            record=record,
            owner=owner_name,
            index=owner_index,
        )
        runtime = asset["runtime"]
        with wave.open(str(_relative(asset_root, runtime["runtimePath"])), "rb") as wav:
            pcm = wav.readframes(wav.getnframes())
            b.audio(
                "runtime-format",
                format=[
                    wav.getsampwidth(),
                    wav.getnchannels(),
                    wav.getframerate(),
                    wav.getnframes(),
                ],
            )
        capture_path = _relative(asset_root, record["sourcePath"])
        with wave.open(str(capture_path), "rb") as capture:
            begin, end = record["captureStartSample"], record["captureEndSample"]
            capture.setpos(begin)
            cut = capture.readframes(end - begin)
            if b.audio(
                "capture-bytes", begin=begin, end=end, measurement={"value": _measure(cut, pcm)}
            ):
                b.audio(
                    "capture-format",
                    format=[capture.getsampwidth(), capture.getnchannels(), capture.getframerate()],
                )
        if b.audio("valid-start") and b.audio(
            "capture-digest",
            digest=_digest(capture_path.read_bytes()),
            span=b.fact(lambda end=end, begin=begin: end - begin),
        ):
            decoded = base64.b64decode(selected_audio["pcm16"], validate=True)
            if b.audio("selected-bytes", measurement={"value": _measure(decoded, pcm)}):
                b.audio("pcm-digest", digest=_digest(pcm))
        b.audio("valid-finish")
        for batch in _batches(
            dict(
                index=i,
                row=_selected(
                    row,
                    (
                        "Cue",
                        "RequestedTimerB",
                        "Command",
                        "TimerB",
                        "PcmSha256",
                        "SampleRate",
                        "Channels",
                        "SampleFrames",
                        "LoopBegin",
                        "LoopEnd",
                    ),
                ),
            )
            for i, row in starts
        ):
            b.audio("receipts", rows=batch)
    b.audio("audio-finish", any=bool(starts))
