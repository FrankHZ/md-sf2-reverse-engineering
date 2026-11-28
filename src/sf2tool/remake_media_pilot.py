"""Publish the numeral HD layer and a prepared Witch cue in a worktree-local selection."""

import argparse
import base64
import contextlib
import filecmp
import hashlib
import inspect
import json
import os
import re
import shutil
import sys
import tempfile
import wave
from pathlib import Path

from sf2tool.paths import REPO_ROOT
from sf2tool.remake_audio import prepare_audio
from sf2tool.remake_entry_content import _audio
from sf2tool.remake_media_metadata import project
from sf2tool.remake_media_selection import (
    DEFAULT_SELECTION,
    package_arguments,
    read_selection,
    shared_root,
)
from sf2tool.remake_windows_package import inputs

LIMIT = 1024**3
IMAGE = "numeral.hd.png"
CUE = "MUSIC_WITCH"
WAV = "audio-0a-c1/command-0a-timer-c1.wav"


def sha(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def load(path):
    return json.loads(path.read_bytes())


def save(path, value):
    path.write_bytes((json.dumps(value, sort_keys=True, separators=(",", ":")) + "\n").encode())


def atomic_text(path, value):
    with tempfile.NamedTemporaryFile(dir=path.parent, delete=False) as out:
        out.write(value.encode())
    os.replace(out.name, path)


def version(root, name):
    if not re.fullmatch(r"[a-z0-9][a-z0-9-]{0,48}", name) or Path(name).is_reserved():
        raise ValueError("use a portable lowercase revision name")
    return root / "versions" / name


def sandbox(path):
    local = (REPO_ROOT / "local").resolve(strict=True)
    root = path.absolute()
    if root != root.resolve() or not root.is_relative_to(local) or root == local:
        raise ValueError("pilot requires a real subdirectory of this checkout's local/")
    return root


def usage(root):
    # Count each NTFS file identity once, including interrupted scratch.
    sizes = {}
    for path in root.rglob("*"):
        if path.is_symlink() or path.is_junction():
            raise ValueError("pilot root contains a linked path")
        if path.is_file():
            info = path.stat()
            sizes[info.st_dev, info.st_ino] = info.st_size
    return sum(sizes.values())


def budget(root, additional):
    if usage(root) + additional > LIMIT or shutil.disk_usage(root).free < additional:
        raise ValueError("planned batch exceeds available pilot space")


@contextlib.contextmanager
def writer(root):
    import msvcrt

    # The kernel releases the lock on exit, including an interrupted publisher.
    with (root / "writer.lock").open("r+b") as lock:
        msvcrt.locking(lock.fileno(), msvcrt.LK_NBLCK, 1)
        try:
            yield
        finally:
            lock.seek(0)
            msvcrt.locking(lock.fileno(), msvcrt.LK_UNLCK, 1)


def initialize(root, source, selection):
    roles = read_selection(selection)
    sources = set(inputs(**package_arguments(roles, source)).values())
    if root.is_relative_to(source.resolve()) or source.resolve().is_relative_to(root):
        raise ValueError("source and disposable pilot roots must be separate")
    root.mkdir(parents=True, exist_ok=False)
    budget(root, sum(p.stat().st_size for p in sources) + 4 * 1024**2)
    for path in sources:
        target = root / path.relative_to(source)
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(path, target)
        if not filecmp.cmp(path, target, shallow=False):
            raise ValueError("pilot seed copy differs")
    initial = version(root, "base")
    initial.mkdir(parents=True)
    save(initial / "selection.json", roles)
    (initial / "current-media.tsv").write_bytes(project(root, roles))
    seal(initial)
    (root / "writer.lock").write_bytes(b"0")
    atomic_text(root / "ACTIVE", "base")
    return len(sources)


def selected(root, name):
    return read_selection(version(root, name) / "selection.json")


def cue(document):
    rows = [r for r in document["presentation"]["audio"] if r["command"] == 10]
    if len(rows) != 1 or (rows[0]["cue"], rows[0]["timerB"]) != (CUE, 193):
        raise ValueError("pilot requires the reviewed Witch command 10 / timer C1 binding")
    return rows[0]


def dependencies(*modules):
    parent = Path(__file__).parent
    return {module: sha(parent / (module + ".py")) for module in modules}


def signature(value):
    return hashlib.sha256(json.dumps(value, sort_keys=True).encode()).hexdigest()


def cached_unit(root, family, identity, generate, outputs):
    parent = root / "units" / family
    parent.mkdir(parents=True, exist_ok=True)
    final = parent / signature(identity)
    if final.exists():
        receipt = load(final / "receipt.json")
        if receipt["identity"] != identity or receipt["files"] != {
            name: sha(final / name) for name in outputs
        }:
            raise ValueError(f"{family} completed output or identity changed")
        return final, False
    with tempfile.TemporaryDirectory(dir=parent) as temp:
        stage = Path(temp) / "unit"
        stage.mkdir()
        generate(stage)
        save(
            stage / "receipt.json",
            {
                "identity": identity,
                "files": {name: sha(stage / name) for name in outputs},
            },
        )
        stage.rename(final)
    return final, True


def assemble_entry(entry, old_entry, changed, image, audio, omit_hd):
    entry.mkdir(parents=True)
    doc = load(old_entry / "entry.json")
    dirty_document = False
    if "image" in changed:
        bank = doc.setdefault("hdResources", [])
        found = [r for r in bank if r["name"] == "numeral"]
        if found and omit_hd:
            bank.remove(found[0])
            dirty_document = True
        elif not omit_hd:
            shutil.copyfile(image / IMAGE, entry / IMAGE)
            if not found:
                bank.append(dict(name="numeral", file=IMAGE, width=1024, height=896))
                dirty_document = True
    if "audio" in changed:
        rows = doc["presentation"]["audio"]
        old_cue = cue(doc)
        new_cue = load(audio / "cue.json")
        if old_cue != new_cue:
            rows[rows.index(old_cue)] = new_cue
            dirty_document = True
    if dirty_document:
        save(entry / "entry.json", doc)
    else:
        os.link(old_entry / "entry.json", entry / "entry.json")
    unchanged = {"entry.zh-CN.json"} | {
        row["file"] for group in ("resources", "hdResources") for row in doc.get(group, [])
    }
    if "image" in changed:
        unchanged.discard(IMAGE)
    for name in sorted(unchanged):
        if Path(name).name != name or not (old_entry / name).is_file():
            raise ValueError("entry reader requires regular sibling inputs")
        os.link(old_entry / name, entry / name)


def run(root, args):
    from PIL import Image
    from PIL import __version__ as pillow

    from sf2tool.remake_hd_actors import frame
    from sf2tool.remake_map3_ramp import GFX_COMMIT, load_filter

    previous = (root / "ACTIVE").read_text()
    if previous != args.base or args.revision == previous:
        raise ValueError("select the current base and a distinct candidate revision")
    old = selected(root, previous)
    if previous != "base":
        verify_seal(version(root, previous))
    original = selected(root, "base")
    base_entry = root / original["entry"]
    old_entry = root / old["entry"]
    if not old_entry.resolve().is_relative_to(root) or old_entry != old_entry.resolve():
        raise ValueError("pilot entry outside the real isolated root")
    source_image = base_entry / "numeral.png"
    source_audio = root / original["gameplay"] / WAV
    original_cue = cue(load(base_entry / "entry.json"))
    # Bind the prepared WAV to the accepted entry PCM before using any interval override.
    with wave.open(str(source_audio), "rb") as sound:
        pcm = sound.readframes(sound.getnframes())
        if (sound.getframerate(), sound.getnchannels(), sound.getsampwidth()) != (
            original_cue["sampleRate"],
            original_cue["channels"],
            2,
        ) or pcm != base64.b64decode(original_cue["pcm16"], validate=True):
            raise ValueError("prepared Witch wave differs from accepted entry")
    del pcm

    def image_unit(stage):
        if not args.omit_hd:
            with Image.open(source_image) as original_image:
                if original_image.size != (256, 224):
                    raise ValueError("invalid numeral source size")
                with original_image.convert("RGBA") as rgba, frame(rgba, scale) as hd:
                    hd.save(stage / IMAGE, compress_level=args.png_compression)

    def audio_unit(stage):
        candidate = stage / "prepared"
        prepare_audio(
            source_audio,
            audio_key["wave"],
            candidate,
            command=10,
            timer_b=193,
            cue=CUE,
            start_sample=0,
            end_sample=audio_key["samples"],
            loop_begin=audio_key["loopBegin"],
            loop_end=audio_key["loopEnd"],
        )
        save(stage / "cue.json", _audio(candidate / "candidate.json", 10))

    reader = REPO_ROOT / "remake/src/Sf2.Remake.Content/Scenarios/EntryPresentationReader.cs"
    common = dict(reader=sha(reader), python=sys.version.split()[0])
    # The pinned filter verifies its implementation checkout and tree on every invocation.
    scale = load_filter(args.gfx_root)[1]
    image_key = dict(
        common=common,
        source=sha(source_image),
        generator=signature(inspect.getsource(image_unit)),
        gfx=GFX_COMMIT,
        pillow=pillow,
        helpers=dependencies("remake_hd_actors", "remake_map3_ramp"),
        compression=args.png_compression,
        enabled=not args.omit_hd,
    )
    audio_key = dict(
        common=common,
        wave=sha(source_audio),
        generator=signature(inspect.getsource(audio_unit)),
        command=10,
        timer=193,
        samples=original_cue["sampleFrames"],
        loopEnd=original_cue["loopEnd"],
        loopBegin=original_cue["loopBegin"] if args.loop_begin is None else args.loop_begin,
        helpers=dependencies("remake_audio", "remake_entry_content"),
    )
    old_keys = {} if previous == "base" else load(version(root, previous) / "units.json")
    keys = {"image": signature(image_key), "audio": signature(audio_key)}
    changed = [k for k in keys if keys[k] != old_keys.get(k)]
    if not changed:
        old_dir = version(root, previous)
        metadata = project(root, old)
        if metadata == (old_dir / "current-media.tsv").read_bytes():
            return {"changed": [], "generated": [], "active": previous}
    budget(root, 5 * (base_entry / "entry.json").stat().st_size + 8 * 1024**2)

    generated = []
    image = audio = None
    if "image" in changed:
        image, made = cached_unit(
            root, "image", image_key, image_unit, [] if args.omit_hd else [IMAGE]
        )
        if made and not args.omit_hd:
            generated.append("image")
    if args.stop_after == "image":
        raise RuntimeError("requested interruption after completed image unit")
    if "audio" in changed:
        audio, made = cached_unit(root, "audio", audio_key, audio_unit, ["cue.json"])
        if made:
            generated.append("audio")

    final = version(root, args.revision)
    roles = dict(old, entry=(final / "entry").relative_to(root).as_posix()) if changed else old
    if final.exists():
        if (
            selected(root, args.revision) != roles
            or load(final / "units.json") != keys
            or (final / "parent").read_text() != previous
        ):
            raise ValueError("candidate revision already belongs to a different publication")
        validate_release(root, args.revision)
    else:
        with tempfile.TemporaryDirectory(dir=root) as temp:
            stage = Path(temp) / "release"
            entry = stage / "entry"
            stage.mkdir()
            if changed:
                assemble_entry(entry, old_entry, changed, image, audio, args.omit_hd)
                metadata = project(root, roles, staged_entry=entry)
            save(stage / "selection.json", roles)
            save(stage / "units.json", keys)
            (stage / "parent").write_text(previous)
            (stage / "current-media.tsv").write_bytes(metadata)
            seal(stage)
            stage.rename(final)
    if args.stop_after == "assembly":
        raise RuntimeError("requested interruption before switching selection and metadata")
    atomic_text(root / "ACTIVE", args.revision)
    return {"active": args.revision, "changed": changed or ["metadata"], "generated": generated}


def release_receipt(directory):
    # Only the pilot's two mutable output bindings and publication records need resume evidence.
    paths = [directory / "selection.json", directory / "current-media.tsv"]
    if directory.name != "base":
        paths.extend([directory / "units.json", directory / "parent"])
    if (directory / "entry").is_dir():
        paths.append(directory / "entry/entry.json")
        if (directory / "entry" / IMAGE).exists():
            paths.append(directory / "entry" / IMAGE)
    return {p.relative_to(directory).as_posix(): sha(p) for p in paths}


def seal(directory):
    save(directory / "seal.json", release_receipt(directory))


def verify_seal(directory):
    if load(directory / "seal.json") != release_receipt(directory):
        raise ValueError("sealed pilot revision changed")


def validate_release(root, name, *, rollback=False):
    directory = version(root, name)
    roles = selected(root, name)
    verify_seal(directory)
    if rollback:
        # A prior reviewed projection can intentionally use older recipe labels.
        inputs(**package_arguments(roles, root))
        return
    if project(root, roles) != (directory / "current-media.tsv").read_bytes():
        raise ValueError("selection and media projection no longer match")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, required=True)
    commands = parser.add_subparsers(dest="operation", required=True)
    init = commands.add_parser("init")
    init.add_argument("--selection", type=Path, default=DEFAULT_SELECTION)
    init.add_argument("--shared-root", type=Path)
    publish = commands.add_parser("publish")
    publish.add_argument("--gfx-root", type=Path, required=True)
    publish.add_argument("--revision", required=True)
    publish.add_argument("--base", required=True)
    publish.add_argument("--png-compression", type=int, choices=range(10), default=6)
    publish.add_argument("--loop-begin", type=int)
    publish.add_argument("--omit-hd", action="store_true")
    publish.add_argument("--stop-after", choices=("image", "assembly"))
    activate = commands.add_parser("activate")
    activate.add_argument("--revision", required=True)
    activate.add_argument("--base", required=True)
    args = parser.parse_args()
    root = sandbox(args.root)
    if args.operation == "init":
        print(
            json.dumps(
                {"seedFiles": initialize(root, shared_root(args.shared_root), args.selection)}
            )
        )
        return
    with writer(root):
        usage(root)
        if args.operation == "publish":
            print(json.dumps(run(root, args)))
        else:
            if (root / "ACTIVE").read_text() != args.base:
                raise ValueError("pilot base changed")
            validate_release(root, args.revision, rollback=True)
            atomic_text(root / "ACTIVE", args.revision)
            print(json.dumps({"active": args.revision}))


if __name__ == "__main__":
    main()
