"""Common pinned invocation tile arrangement and palette loading."""

from sf2tool.remake_battle_scene_content import compose
from sf2tool.texture_extract import md_palette_color


def invocation_sprites(directory, invocation_id, frame_count, palette_hex):
    # Header palette word0 is not copied: LoadInvocationSpriteFrameToVram
    # advances2 bytes and writes the remaining15 into palette1 colors1..15.
    words = bytes.fromhex(palette_hex)[2:]
    palette = [(0, 0, 0)] + [
        md_palette_color(int.from_bytes(words[i : i + 2], "big")) for i in range(0, 30, 2)
    ]
    # layout_Invocation: four columns of 32x32 sprites. The second stream
    # starts at tile144 after the first128 tiles and16 unused VRAM tiles.
    layout = [
        (x // 4) * 32 + (y // 4 % 2) * 16 + (144 if y >= 8 else 0) + (x % 4) * 4 + y % 4
        for y in range(16)
        for x in range(16)
    ]
    sprites = []
    for frame in range(frame_count):
        streams = [
            (directory / f"invocation-{invocation_id}-{frame}-{part}.4bpp").read_bytes()
            for part in range(2)
        ]
        if any(len(stream) != 4096 for stream in streams):
            raise ValueError("registered invocation stream length differs")
        sprites.append(
            (
                f"summon-{frame}",
                128,
                128,
                compose(streams[0] + bytes(512) + streams[1], palette, 128, 128, layout),
            )
        )
    return sprites
