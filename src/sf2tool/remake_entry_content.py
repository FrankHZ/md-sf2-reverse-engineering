"""Export the bounded private title/Witch composition; no capture or runtime dependency.

Uses the existing source/ROM provenance and Stack/tile decoders. Only decoded tiles
may be drawn: the original Witch DMA transfer tail is deliberately not synthesized.
"""

from __future__ import annotations

import argparse
import base64
import hashlib
import json
import re
import wave
from pathlib import Path

from sf2tool.compression import decode_stack_compressed
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json
from sf2tool.paths import repo_path
from sf2tool.texture_extract import (
    decode_md_4bpp_tile,
    md_palette_color,
    transform_tile_pixels,
    write_png_rgba,
)


def _audio(candidate: Path, command: int) -> dict:
    reviewed = json.loads(candidate.read_text(encoding="utf-8"))
    asset = reviewed["asset"]
    runtime = asset["runtime"]
    path = candidate.parent / Path(runtime["runtimePath"]).name
    if (
        asset["command"] != command
        or hashlib.sha256(path.read_bytes()).hexdigest().upper() != runtime["sha256"]
    ):
        raise ValueError("entry reviewed music binding drift")
    with wave.open(str(path), "rb") as source:
        if source.getsampwidth() != 2 or source.getnchannels() not in (1, 2):
            raise ValueError("entry music must be PCM16")
        pcm = source.readframes(source.getnframes())
        return {
            "cue": asset["cue"],
            "command": command,
            "timerB": asset["timerB"],
            "sampleRate": source.getframerate(),
            "channels": source.getnchannels(),
            "sampleFrames": source.getnframes(),
            "pcm16": base64.b64encode(pcm).decode("ascii"),
            "sha256": hashlib.sha256(pcm).hexdigest().upper(),
            "loopBegin": runtime["loopBegin"],
            "loopEnd": runtime["loopEnd"],
        }


def export_entry(
    rom: Path,
    upstream: Path,
    output: Path,
    title_audio: Path | None = None,
    witch_audio: Path | None = None,
) -> dict:
    output = output.resolve()
    if not output.is_relative_to(repo_path("local")):
        raise ValueError("entry output must be worktree-local ignored content")
    disasm, commit, _ = _resolve_upstream(upstream)
    identity = load_json(repo_path("manifests/roms/sf2-us.json"))
    rom_bytes = rom.read_bytes()
    if (
        len(rom_bytes) != identity["sizeBytes"]
        or hashlib.sha256(rom_bytes).hexdigest().upper() != identity["hashes"]["sha256"]
    ):
        raise ValueError("entry ROM identity drift")
    # Pinned original resource operands, owned by special-screens.md and its
    # accepted H2 rails. Read only these spans; no H1 rebuild or full ROM split.
    ranges = {
        "tiles_TitleScreen": (1049406, 4514),
        "palette_TitleScreen": (1049278, 128),
        "layout_TitleScreenA": (1053920, 1792),
        "layout_TitleScreenB": (1055712, 768),
        "tiles_Witch": (1855112, 6460),
        "palette_Witch": (1853122, 64),
        "layout_Witch": (1853186, 1926),
        "font_TitleScreen": (187870, 688),
        "palette_TitleScreenFont": (188558, 128),
    }

    def payload(symbol: str) -> bytes:
        address, length = ranges[symbol]
        return rom_bytes[address : address + length]

    def words(data: bytes) -> list[int]:
        return [int.from_bytes(data[i : i + 2], "big") for i in range(0, len(data), 2)]

    def tiles(symbol: str, size: int) -> list[list[int]]:
        decoded = decode_stack_compressed(payload(symbol)).output
        if len(decoded) != size:
            raise ValueError("entry decoded tile size drift")
        return [decode_md_4bpp_tile(decoded[i : i + 32]) for i in range(0, size, 32)]

    title_tiles = tiles("tiles_TitleScreen", 8192)
    font = tiles("font_TitleScreen", 4096)
    witch_tiles = tiles("tiles_Witch", 13568)
    title_palette = [md_palette_color(w) for w in words(payload("palette_TitleScreen"))]
    title_palette[48:64] = [
        md_palette_color(w) for w in words(payload("palette_TitleScreenFont"))[:16]
    ]
    witch_colors = words(payload("palette_Witch"))
    witch_palette = [md_palette_color(w) for w in witch_colors[:16] + [0] * 32 + witch_colors[16:]]
    title_a = words(payload("layout_TitleScreenA"))
    title_b = words(payload("layout_TitleScreenB"))
    witch = words(payload("layout_Witch"))
    if (len(title_a), len(title_b), len(witch)) != (896, 384, 963):
        raise ValueError("entry layout size drift")
    output.mkdir(parents=True, exist_ok=False)
    resources = []

    def render(
        name: str, layout: list[int], pool: list[list[int]], palette: list[tuple[int, int, int]]
    ) -> None:
        pixels = [0] * (256 * 224 * 4)
        used = set()
        for cell, word in enumerate(layout[:896]):
            tile = word & 0x7FF
            if tile == 0:
                continue
            index = tile - 0x100
            if name == "title" and 0x580 <= tile < 0x600:
                pattern = font[tile - 0x580]
            elif 0 <= index < len(pool):
                pattern = pool[index]
            else:
                raise ValueError(
                    f"visible layout references undecoded transfer tail: {name}/{tile}"
                )
            used.add(index)
            transformed = transform_tile_pixels(pattern, word)
            bank = (word >> 13) & 3
            for y in range(8):
                for x in range(8):
                    color = transformed[y * 8 + x]
                    if color == 0:
                        continue
                    offset = (((cell // 32) * 8 + y) * 256 + (cell % 32) * 8 + x) * 4
                    pixels[offset : offset + 4] = [*palette[bank * 16 + color], 255]
        write_png_rgba(output / (name + ".png"), 256, 224, pixels)
        resources.append(
            {
                "name": name,
                "file": name + ".png",
                "width": 256,
                "height": 224,
                "usedTiles": len(used),
            }
        )

    render("title", title_a, title_tiles, title_palette)
    numeral = [0] * 896
    numeral[4 * 32 : 15 * 32] = [w | 0x2000 if w else 0 for w in title_b[: 11 * 32]]
    render("numeral", numeral, title_tiles, title_palette)

    def patch(
        layout: list[int], byte_offset: int, destination: int, width: int, height: int
    ) -> None:
        for row in range(height):
            start = byte_offset // 2 + row * width
            layout[destination + row * 32 : destination + row * 32 + width] = witch[
                start : start + width
            ]

    render("witch-arrival", witch, witch_tiles, witch_palette)
    opened = witch[:]
    patch(opened, 0x700, 0x15C // 2, 7, 7)
    render("witch", opened, witch_tiles, witch_palette)
    blink = opened[:]
    patch(blink, 0x762, 0x21E // 2, 3, 2)
    render("witch-blink", blink, witch_tiles, witch_palette)
    mouth = opened[:]
    patch(mouth, 0x780, 0x29E // 2, 3, 1)
    render("witch-mouth", mouth, witch_tiles, witch_palette)
    script = (disasm / "code/specialscreens/witch/witchstart.asm").read_text(encoding="utf-8")
    dialogue = {}
    for match in re.finditer(r'txt\s+(21[6-9]|220|221)\s*;\s*"([^"]+)"', script):
        dialogue[match[1]] = (
            match[2].replace("{CLEAR}", "").replace("{N}", "\n").replace("{W2}", "")
        )
    if len(dialogue) != 6:
        raise ValueError("entry dialogue source shape drift")
    result = {
        "formatVersion": 1,
        "profile": "private-local-entry",
        "upstreamCommit": commit,
        "romSha256": identity["hashes"]["sha256"],
        "resources": resources,
        "dialogue": dialogue,
        "boundary": (
            "source-informed composition; original timing and DMA transfer tails remain Unknown"
        ),
    }
    if title_audio is not None and witch_audio is not None:
        result["presentation"] = {
            "maps": [],
            "sprites": [],
            "portraits": [],
            "audio": [_audio(title_audio, 28), _audio(witch_audio, 10)],
        }
    (output / "entry.json").write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    return result


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--rom", required=True, type=Path)
    parser.add_argument("--upstream", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument(
        "--title-audio", type=Path, help="Reviewed sf2tool.remake_audio candidate.json"
    )
    parser.add_argument(
        "--witch-audio", type=Path, help="Reviewed sf2tool.remake_audio candidate.json"
    )
    args = parser.parse_args()
    result = export_entry(args.rom, args.upstream, args.output, args.title_audio, args.witch_audio)
    print(json.dumps({key: value for key, value in result.items() if key != "presentation"}))


if __name__ == "__main__":
    main()
