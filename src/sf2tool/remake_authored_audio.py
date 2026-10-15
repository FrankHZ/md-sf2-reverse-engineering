"""Prepare the previously accepted authored defeat cue as explicit local content.

This is a project-authored chord, not a recording or reconstruction of Music 12.
The formula is retained from commit 8a581a82's ExplorationPresentation.JoinCue.
Command 12 / timer C4 supplies only the source command's UI context.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import math
import struct
import subprocess
import wave
from pathlib import Path

from sf2tool.paths import repo_path


def prepare_defeat_cue(output: Path) -> dict:
    output = output.resolve()
    root = repo_path("").resolve()
    if not output.is_relative_to(root / "local") or output.exists():
        raise ValueError("output must be a new worktree-local ignored directory")
    subprocess.run(["git", "check-ignore", "--quiet", str(output)], cwd=root, check=True)
    rate, count = 22050, 44100
    pcm = bytearray()
    for index in range(count):
        time = index / rate
        envelope = min(1, time * 20) * min(1, (2 - time) * 20)
        value = int(
            6500
            * envelope
            * sum(math.sin(math.tau * note * time) for note in (196, 233.08, 293.66))
            / 3
        )
        pcm.extend(struct.pack("<h", value))
    output.mkdir(parents=True)
    runtime = output / "authored-defeat.wav"
    with wave.open(str(runtime), "wb") as sound:
        sound.setnchannels(1)
        sound.setsampwidth(2)
        sound.setframerate(rate)
        sound.writeframes(pcm)
    payload = runtime.read_bytes()
    receipt = {
        "boundary": "Project-authored defeat chord; no original audio or hardware fidelity claim.",
        "asset": {
            "assetId": "audio.authored-defeat",
            "kind": "audio",
            "cue": "MUSIC_SAD_THEME_2",
            "command": 12,
            "timerB": 196,
            "source": {
                "kind": "project-authored",
                "repository": "FrankHZ/md-sf2-reverse-engineering",
                "commit": "8a581a82e297ea2947cc9837e661752163d2806d",
                "path": "remake/game/src/Exploration/ExplorationPresentation.cs",
                "symbol": "JoinCue",
            },
            "derivation": {
                "policyId": "project-authored-defeat-chord-v1",
                "generatorId": "sf2tool.remake_authored_audio",
                "generatorVersion": "1",
            },
            "runtime": {
                "runtimePath": "runtime/audio/authored-defeat.wav",
                "byteLength": len(payload),
                "sha256": hashlib.sha256(payload).hexdigest().upper(),
                "mediaType": "audio/wav",
                "sampleRate": rate,
                "channels": 1,
                "sampleFrames": count,
                "loopBegin": 0,
                "loopEnd": count,
            },
        },
    }
    (output / "candidate.json").write_text(json.dumps(receipt, indent=2) + "\n", encoding="utf-8")
    return receipt


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, required=True)
    result = prepare_defeat_cue(parser.parse_args().output)
    print(json.dumps({"boundary": result["boundary"], "runtime": result["asset"]["runtime"]}))
