"""Derive a private PCM asset from an explicitly reviewed original capture interval.

This does not launch an emulator, discover loop points, or establish natural scene reach.
The selected capture identity and sample boundaries require their own source/observation review.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import wave
from pathlib import Path

from sf2tool.paths import repo_path


def prepare_audio(
    capture: Path,
    expected_sha256: str,
    output: Path,
    *,
    command: int,
    cue: str,
    start_sample: int,
    end_sample: int,
    loop_begin: int | None = None,
    loop_end: int | None = None,
) -> dict:
    output = output.resolve()
    if not output.is_relative_to(repo_path("local")) or output == repo_path("local"):
        raise ValueError("audio candidates must be beneath this worktree's local directory")
    source_bytes = capture.read_bytes()
    source_digest = hashlib.sha256(source_bytes).hexdigest().upper()
    if source_digest != expected_sha256.upper():
        raise ValueError("reviewed audio capture identity mismatch")
    if not 1 <= command <= 120 or not cue or not cue.replace("_", "").isalnum():
        raise ValueError("invalid audio command or cue identity")
    with wave.open(str(capture), "rb") as source:
        channels, rate, width = source.getnchannels(), source.getframerate(), source.getsampwidth()
        if (
            width != 2
            or channels not in (1, 2)
            or source.getcomptype() != "NONE"
            or not 8000 <= rate <= 192000
        ):
            raise ValueError("capture must contain PCM16 mono or stereo")
        if not 0 <= start_sample < end_sample <= source.getnframes():
            raise ValueError("reviewed capture sample interval is outside the WAV")
        source.setpos(start_sample)
        pcm = source.readframes(end_sample - start_sample)
    frames = end_sample - start_sample
    if len(pcm) != frames * channels * width or not any(pcm):
        raise ValueError("capture interval is truncated or silent")
    if (
        (loop_begin is None) != (loop_end is None)
        or loop_begin is not None
        and not 0 <= loop_begin < loop_end <= frames
    ):
        raise ValueError("reviewed audio loop interval is invalid")
    output.mkdir(parents=True, exist_ok=False)
    runtime = output / f"command-{command:02x}.wav"
    with wave.open(str(runtime), "wb") as target:
        target.setnchannels(channels)
        target.setsampwidth(width)
        target.setframerate(rate)
        target.writeframes(pcm)
    payload = runtime.read_bytes()
    asset = {
        "assetId": f"audio.command-{command:02x}",
        "kind": "audio",
        "cue": cue,
        "command": command,
        "source": {"assetId": f"source.audio.command-{command:02x}", "sha256": source_digest},
        "derivation": {
            "policyId": "private-reviewed-pcm16-sample-interval-v1",
            "generatorId": "sf2tool.remake_audio",
            "generatorVersion": "1",
            "generatorArtifactSha256": hashlib.sha256(Path(__file__).read_bytes())
            .hexdigest()
            .upper(),
        },
        "runtime": {
            "runtimePath": f"runtime/audio/{runtime.name}",
            "byteLength": len(payload),
            "sha256": hashlib.sha256(payload).hexdigest().upper(),
            "mediaType": "audio/wav",
            "sampleRate": rate,
            "channels": channels,
            "sampleFrames": frames,
            "loopBegin": loop_begin,
            "loopEnd": loop_end,
        },
    }
    receipt = {
        "asset": asset,
        "captureStartSample": start_sample,
        "captureEndSample": end_sample,
        "boundary": (
            "Reviewed source identity and interval only; "
            "no automatic loop discovery or natural gameplay claim."
        ),
    }
    (output / "candidate.json").write_text(json.dumps(receipt, indent=2) + "\n", encoding="utf-8")
    return receipt


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--capture", type=Path, required=True)
    parser.add_argument("--expected-sha256", required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--command", type=int, required=True)
    parser.add_argument("--cue", required=True)
    parser.add_argument(
        "--start-sample", type=int, required=True, help="First interleaved sample frame, inclusive"
    )
    parser.add_argument(
        "--end-sample", type=int, required=True, help="Last interleaved sample frame, exclusive"
    )
    parser.add_argument("--loop-begin", type=int)
    parser.add_argument("--loop-end", type=int)
    args = parser.parse_args()
    result = prepare_audio(
        args.capture,
        args.expected_sha256,
        args.output,
        command=args.command,
        cue=args.cue,
        start_sample=args.start_sample,
        end_sample=args.end_sample,
        loop_begin=args.loop_begin,
        loop_end=args.loop_end,
    )
    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    main()
