"""Prepare isolated retail command PCM using the existing local BizHawk and PCM owners."""

import array
import hashlib
import json
import os
import shutil
import subprocess
import tempfile
import wave
from dataclasses import asdict
from pathlib import Path

from sf2tool.h2.sound_data import BANK_ORIGIN, BANK_ROM_OFFSETS
from sf2tool.h3.bizhawk import (
    _lua_literal,
    bizhawk_contract,
    materialize_bizhawk_launch,
    run_native_bizhawk_process,
    validate_lua_syntax,
)
from sf2tool.paths import repo_path
from sf2tool.remake_audio import prepare_audio
from sf2tool.toolchain import verify_bizhawk_installation


def music_header(rom, command):
    bank = BANK_ROM_OFFSETS["bank0" if command <= 32 else "bank1"]
    slot = bank + 2 * ((command - 1) % 32)
    address = bank + int.from_bytes(rom[slot : slot + 2], "little") - BANK_ORIGIN
    return rom[address : address + 24]


def sample(frame):
    return frame * 44100 * 896040 // 53693175 + 441


def crop(capture, events):
    dispatch = int(events["dispatch"][0])
    with wave.open(str(capture), "rb") as src:
        if (src.getframerate(), src.getnchannels(), src.getsampwidth()) != (44100, 2, 2):
            raise ValueError("capture PCM format drift")
        low = sample(dispatch - 2)
        end = sample(int(events.get("tail", events.get("matched"))[0]))
        src.setpos(low)
        pcm = array.array("h", src.readframes(end - low))

    def loud(i):
        return abs(pcm[2 * i]) > 1 or abs(pcm[2 * i + 1]) > 1

    count = len(pcm) // 2
    onset = next((i for i in range(count) if loud(i)), None)
    if onset is None or onset < 441:
        raise ValueError("no isolated PCM onset with 10ms of quiet lead")
    start = low + onset - 441
    if "matched" in events:
        last, first = map(int, events["matched"])
        return start, sample(last), sample(first) - start, sample(last) - start
    # The final two emulator frames can contain post-observation padding.
    limit = min(count, sample(int(events["tail"][0]) - 2) - low)
    last = next(i for i in reversed(range(limit)) if loud(i))
    if limit - last < 22050:
        raise ValueError("finite PCM lacks the observed half-second quiet tail")
    return start, low + last + 442, None, None


def capture(rom_path: Path, rom: bytes, out: Path, command: int, cue: str, context: int):
    music = command <= 64
    header = music_header(rom, command if music else context)
    timer = header[3]
    contract, exe = bizhawk_contract()
    install, _ = verify_bizhawk_installation(contract)

    def inventory():
        return {
            p.relative_to(install): (p.stat().st_size, p.stat().st_mtime_ns)
            for p in install.rglob("*")
            if p.is_file()
        }

    before = inventory()
    processes = subprocess.check_output(
        ["tasklist", "/FI", "IMAGENAME eq EmuHawk.exe", "/FO", "CSV", "/NH"]
    ).lower()
    if b"emuhawk.exe" in processes:
        raise ValueError("registered emulator is already in use")
    scratch = Path(tempfile.mkdtemp(prefix=f"retail-audio-{command}-", dir=repo_path("local")))
    frames = 14500 if music and command not in (25, 19) else 1400
    if command == 15:
        frames = 30000  # the first 14,500-frame export ended before a whole-state match
    wav = scratch / "capture.wav"
    trace = scratch / "boundaries.csv"
    config = scratch / "config.lua"
    config.write_text(
        "return "
        + _lua_literal(
            dict(
                music=music,
                command=command,
                context=context,
                frames=frames - 310,
                trace=str(trace),
                pointers=[int.from_bytes(header[i : i + 2], "little") for i in range(4, 24, 2)],
            )
        ),
        encoding="utf-8",
    )
    observer = repo_path("tools/bizhawk/retail_audio.lua")
    launch = materialize_bizhawk_launch(
        scratch, config={"SoundEnabled": True, "ClockThrottle": False, "Unthrottled": True}
    )
    validate_lua_syntax(observer, exe)
    result = run_native_bizhawk_process(
        executable=exe,
        cwd=Path(launch["cwd"]),
        command=[
            str(exe),
            "--config=" + launch["config"],
            "--lua=" + str(observer),
            "--dump-type=wave",
            "--dump-name=" + str(wav),
            f"--dump-length={frames}",
            "--dump-close",
            "--audiosync=false",
            str(rom_path),
        ],
        environment={**os.environ, **launch["environment"], "SF2_AUDIO_CONFIG": str(config)},
        timeout_seconds=300,
    )
    (scratch / "native.json").write_text(json.dumps(asdict(result)), encoding="utf-8")
    verify_bizhawk_installation(contract)
    if before != inventory():
        raise RuntimeError(f"shared installation changed during capture: {scratch}")
    if result.returncode != 0 or result.timed_out or not trace.is_file():
        raise ValueError(f"native export failed: {scratch}")
    events = {
        parts[0]: parts[1:]
        for line in trace.read_text().splitlines()
        if len(parts := line.split(",", 2)) == 3
    }
    if "PASS" not in events or "FAIL" in events:
        raise ValueError(f"driver boundary failed: {events.get('FAIL')} in {scratch}")
    start, end, first, last = crop(wav, events)
    candidate = scratch / "result"
    receipt = prepare_audio(
        wav,
        hashlib.sha256(wav.read_bytes()).hexdigest(),
        candidate,
        command=command,
        timer_b=timer,
        cue=cue,
        start_sample=start,
        end_sample=end,
        loop_begin=first,
        loop_end=last,
    )
    # Preserve the reviewed source, boundary and candidate together after scratch retirement.
    shutil.copyfile(wav, candidate / "source.wav")
    shutil.copyfile(trace, candidate / trace.name)
    with tempfile.TemporaryDirectory(dir=out.parent) as stage:
        staged = Path(stage) / "candidate"
        shutil.copytree(candidate, staged)
        staged.rename(out)
    return receipt["asset"]


def prepare_commands(family, inventory, root, rom_path, rom, *, commands=()):
    """Only collect commands missing from the fixed selection. Resume by command."""
    failures = {}
    rows = [row for row in inventory if row["family"] == family and row["status"] == "missing"]
    for row in sorted(rows, key=lambda r: (int(r["id"]) not in (25, 87, 99), int(r["id"]))):
        command = int(row["id"])
        if commands and command not in commands:
            continue
        out = root / str(command)
        context = 38 if command == 87 else 8  # Bedoe Map10 Castle; default field pilot Town.
        timer = music_header(rom, command if command <= 64 else context)[3]
        cue = row["source_symbol"] + (f"_TIMER_{timer:02X}" if command > 64 else "")
        try:
            if not out.exists():
                if (
                    min(shutil.disk_usage(root).free, shutil.disk_usage(repo_path("local")).free)
                    < 512 << 20
                ):
                    raise OSError("audio preparation requires 512 MiB free on both volumes")
                capture(rom_path, rom, out, command, cue, context)
            entry = json.loads((out / "candidate.json").read_bytes())["asset"]
            wav = out / Path(entry["runtime"]["runtimePath"]).name
            if (
                (entry["command"], entry["timerB"], entry["cue"]) != (command, timer, cue)
                or hashlib.sha256(wav.read_bytes()).hexdigest().upper()
                != entry["runtime"]["sha256"]
                or hashlib.sha256((out / "source.wav").read_bytes()).hexdigest().upper()
                != entry["source"]["sha256"]
            ):
                raise ValueError("existing audio candidate differs from its receipt")
            print(f"{family} {command} {cue} verified", flush=True)
        except ValueError as error:
            failures[command] = str(error)
            print(f"{family} {command} FAILED: {error}", flush=True)
    if failures:
        raise ValueError(f"unprepared {family} commands: {failures}")
