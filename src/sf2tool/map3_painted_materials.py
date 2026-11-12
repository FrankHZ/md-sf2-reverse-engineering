"""Paint private Map3 cells from semantic material swatches and local silhouettes."""

from __future__ import annotations

from collections import Counter

from PIL import Image, ImageChops, ImageFilter, ImageOps, ImageStat

SCALE = 4
NEAREST = Image.Resampling.NEAREST
SMOOTH = Image.Resampling.LANCZOS
MATERIALS = (
    "grass",
    "leaves",
    "earth",
    "water",
    "stone",
    "cliff",
    "cobbles",
    "plaster",
    "shingles",
    "thatch",
    "oak",
    "timber",
    "slate",
    "iron",
    "brick",
    "cloth",
)


def brushes(sheet: Image.Image) -> dict[str, Image.Image]:
    width, height = sheet.size
    if width != height or width < 384:
        raise ValueError("expected the square sixteen-material master")
    result = {}
    for index, name in enumerate(MATERIALS):
        col, row = index % 4, index // 4
        bounds = (
            col * width // 4 + 3,
            row * height // 4 + 3,
            (col + 1) * width // 4 - 3,
            (row + 1) * height // 4 - 3,
        )
        sample = sheet.crop(bounds).convert("RGB").resize((120, 120), SMOOTH)
        # Crossfade the surplus strip into its opposite edge without introducing mirror axes.
        for _ in range(2):
            length, strip_height = sample.size
            patch = sample.crop((0, 0, 96, strip_height))
            fade = Image.linear_gradient("L").rotate(90).resize((24, strip_height))
            patch.paste(sample.crop((96, 0, length, strip_height)), (0, 0), ImageOps.invert(fade))
            # Meet the next cell at the same paint sample. Ease the final three
            # columns into that sample without mirroring the swatch interior.
            first = patch.crop((0, 0, 1, strip_height))
            for offset, fraction in enumerate((0.25, 0.60, 1.0), start=93):
                last = patch.crop((offset, 0, offset + 1, strip_height))
                patch.paste(Image.blend(last, first, fraction), (offset, 0))
            sample = patch.transpose(Image.Transpose.TRANSPOSE)
        result[name] = sample
    return result


def material_for(color, frequent):
    r, g, b = color[:3]
    if g > r and g > b:
        return "leaves" if (36, 109, 0) in frequent else "grass"
    if b > r and b > g:
        return "water"
    if r > 220 and g < 50:
        return "shingles"
    if max(r, g, b) - min(r, g, b) < 8:
        return "stone" if r > 60 else "iron"
    if r > 2 * max(1, g):
        return "timber"
    if (255, 0, 36) in frequent:
        return "timber"
    if (73, 36, 0) in frequent:
        return "oak"
    return "earth"


def painted_block(
    source: Image.Image, materials: dict[str, Image.Image]
) -> tuple[Image.Image, str]:
    alpha = source.getchannel("A")
    rgb = source.convert("RGB")
    colors = Counter(pixel[:3] for pixel in source.getdata() if pixel[3])
    frequent = {color for color, count in colors.items() if count >= 18}
    families = Counter()
    result = Image.new("RGB", (96, 96))
    # Work per color island. Source value retains architectural modelling and small signage;
    # the color/detail inside each island comes from the painted material at display size.
    interior = Image.new("L", (96, 96))
    interior.putdata(
        [min(255, max(0, min(x, y, 95 - x, 95 - y) - 2) * 32) for y in range(96) for x in range(96)]
    )
    for color, count in colors.items():
        family = material_for(color, frequent)
        families[family] += count
        pattern = materials[family]
        luminance = round(sum(c * w for c, w in zip(color, (0.30, 0.59, 0.11), strict=True)))
        mean = ImageStat.Stat(pattern.convert("L")).mean[0]
        level = min(1.62, max(0.26, (luminance * 0.77 + 28) / max(1, mean)))
        tint = pattern.point([min(255, round(v * level)) for v in range(256)] * 3)
        edge_family = material_for(color, set())
        if edge_family != family:
            border = materials[edge_family]
            average = ImageStat.Stat(border.convert("L")).mean[0]
            gain = min(1.62, max(0.26, (luminance * 0.77 + 28) / max(1, average)))
            border = border.point([min(255, round(v * gain)) for v in range(256)] * 3)
            tint = Image.composite(tint, border, interior)
        mask = Image.new("L", source.size)
        mask.putdata([255 if pixel == color else 0 for pixel in rgb.getdata()])
        mask = mask.resize((96, 96), Image.Resampling.BICUBIC)
        result.paste(tint, (0, 0), mask)
    # Keep guide ink for signs, masonry and small props across painted islands.
    guide = rgb.resize((96, 96), Image.Resampling.BICUBIC)
    result = Image.blend(result, guide, 0.19).convert("RGBA")
    result.putalpha(alpha.resize((96, 96), NEAREST))
    return result, ",".join(name for name, _ in families.most_common()) or "transparent"


def inked_figure(source: Image.Image, fabric: Image.Image) -> Image.Image:
    """Direction-stable local ink/shadow treatment on each discrete pose/portrait tile."""
    size = tuple(axis * SCALE for axis in source.size)
    enlarged = source.convert("RGB").resize(size, Image.Resampling.BICUBIC)
    light = enlarged.convert("L")
    edge = ImageChops.subtract(light, light.filter(ImageFilter.MinFilter(5)))
    shade = ImageChops.subtract(light.filter(ImageFilter.MaxFilter(5)), light)
    grain = fabric.convert("L").resize(size, SMOOTH)
    mid = ImageStat.Stat(grain).mean[0]
    result = Image.new("RGBA", size)
    alpha = source.getchannel("A").resize(size, NEAREST)
    pixels = []
    for rgb, rim, shadow, weave, opacity in zip(
        enlarged.getdata(),
        edge.getdata(),
        shade.getdata(),
        grain.getdata(),
        alpha.getdata(),
        strict=True,
    ):
        value = sum(rgb) / 765
        # Warm lit planes and cool ink shadows; eight-substep relief preserves tiny expressions.
        stroke = rim * 0.36 - shadow * 0.23 + (weave - mid) * 0.13
        gradient = (7 * value, 1, 10 * (1 - value))
        pixels.append(
            tuple(
                max(0, min(255, round(c * 0.92 + stroke + offset)))
                for c, offset in zip(rgb, gradient, strict=True)
            )
            + (opacity,)
        )
    result.putdata(pixels)
    return result
