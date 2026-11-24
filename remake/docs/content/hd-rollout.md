# Pre-voyage HD rollout

[Issue #2172](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2172) defines the
production plan for [#2171](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2171).
**High rigor** applies because production spans several resource families, three content entry
points and a fixed world admission limit. The accepted visual reference is Map3 L1. Keep the
existing Original/HD preference and default, logical view and hit geometry, source selectors,
animation clocks, progress and save compatibility. Start at the title and Witch and stop at first
stable Map14 control after boarding. The existing Battle08 board mounts Map72 and has its own
playable boundary. The two handoffs share the complete package.

Use [the resource trace](asset-inventory.md#resource-families-and-owners) for source, prepared
library, installed selector and observed consumer. The preparation reference is `runtime/retail-v1`
in the local asset repository, with the reviewed indexes and pinned retail provenance. The
production input is the [complete #2156 selection](asset-inventory.md#complete-private-package-and-rebuild-2156).
Reuse the admitted original for the selected palette and frame. Where a caller needs an unmounted
original, join its prepared index to that caller and verify Original first. Use only authorized
materials for derivation and keep the existing private publication and public-asset boundaries.
The [static terrain selection](asset-inventory.md#pre-voyage-static-terrain-2175) carries the Map3
companion and the other nineteen banks. Use this closed package as the input for later families.

## Coverage from the selected package

**Confirmed** by read-only inspection. The complete selection contains 52 map visuals, 13,416
map-local block rows and 25 additional encounter selections alongside the opening Battle01
inputs. Campaign membership follows the [location agreement](../application/pre-voyage-optional-locations.md#selected-locations-and-interactions),
the [southern shrine](../application/pre-voyage-optional-locations.md#southern-shrine-and-excluded-connections)
and scene/encounter consumers. The resulting 20 map banks contain 5,568 block bindings across
16 distinct encoded atlases. Retain each bank's local block IDs even where atlases are identical.

| Order | Map visuals | Necessary selections and witnesses |
| --- | --- | --- |
| First | 3, 4 | Carry forward the accepted Map3 bank. Critical recovery is Map19 south exit → F640 Map4 → Map44 west return → rebuilt Map4. Prepare the first quake's six ordered script copies and F624's two load-time copies, including affected neighbors on both planes. Preserve quake displacement, actor transformations, foreground, collision and F217. [Accepted #2167 route](../application/pre-voyage-campaign.md#castle-to-quake-handoff-from-a-player-save-2167) |
| Grans episode | 19, 20, 21, 40, 57, 60, 58 | Castle optional rooms and stair returns, tower exterior, F506 stair opening, early ascent and Battle07 aftermath. Battle01 and Battle07 draw from Maps57 and 58. Include open chests and battle return layouts as applicable. |
| Overworld, villages and shrine | 66, 5, 43, 41, 42 | Map66 hosts Battles02/03/04/06, the departure ship-and-shore scene, and the southern shrine approach. Include Yeel's aboveground rooms, F650 cast, Hawel's house and repeat, the southern Map41 exterior and both reachable Map42 rooms. Select reachable area variants under the campaign agreement. |
| Galam and departure | 16, 17, 18, 44, 73, 14 | Galam palace, prison and optional rooms, Battle05 on Map16 and the drawbridge in both gate states. Map44 includes the optional hold and changed queue, then the captain's departure. Observe the scripted Map66 layers, Map73 arrival and first Map14 control. |

Publish complete center banks for those map IDs. Plan context coverage for the admitted area
planes and working layouts, including source cells reachable via independent scene offsets.
Count packed late rooms and the second Map73 area as capacity upper bounds. Keep campaign
availability with its existing owners, including the late Map41 head, Map42 sword room, Map59
replacement, additional Map60 ascent, and later Map14 movement.

| Visual family | Prepared and installed | HD generation and actual consumer gap |
| --- | --- | --- |
| Field and tactical terrain | Retail `maps/index.json` joins atlases, tiles, palettes, blocks, layouts and animation operands. The selected world admits original atlases and block words. All twenty selected maps mount L1 banks. | Field drawing, pickup occlusion and tactical terrain resolve the current block and neighborhood. Each tactical plane retains its encounter origin, priority and logical tile footprint. [Static terrain evidence](asset-inventory.md#pre-voyage-static-terrain-2175) |
| Water and other animated tiles | The prepared map index and original [map animation contract](../../../docs/research/world/map-content.md) identify source tables. Map66's DMA witness is in [map-animation-vdp-v1](../../../tests/fixtures/h3/map-animation-vdp-v1.json). Current world maps carry working layout events and the presenter supports plane scroll and palette effects. | **Confirmed** the world visual has a static atlas and no tile-animation field or phase selector. Treat source tile substitution as a separate consumer slice. Static water, scrolling and palette changes each have their own observations. Eight selected maps have referenced animation targets. **Unknown** which phases are drawn during the accepted visits. |
| Field and tactical sprites | Retail `walking/index.json` includes ordinary directions, class appearances and special streams. The current world has 163 sprite IDs, including 252/254/255, and three alternate graphics names on 252. | 85 IDs have installed `hdDirections`. Field selection is gated by HD terrain and excludes shadow-removed and alternate graphics. Tactical sprites currently select Original, including scene-owned death frames. Cover both consumers and their actual special dispatches. |
| Portraits | Retail `portraits/index.json` supplies atlases and replacements. The world mounts 46 portraits, 38 with `hdRaster`. | Bound complete eye/mouth replacement, mirror and member/class identity together. Field story and both Member surfaces presently tie portrait HD to terrain coverage. Resolve the saved mode and the selected portrait's own variant, including special-pose dialogue. |
| Battle close-up bodies and weapons | Retail `bodies` and `weapons` plus the mounted scene documents bind body/palette/weapon, frames and layer order. | Primary scene `rasters` are the current textures in `BattleSceneView`. There is no mode-specific lookup. Preserve idle/reaction/attack geometry, palette-dependent weapon colors, mirroring, offsets and front/back layer. |
| Battle ground and backdrops | Retail `terrain` contains the source variants. Scene defaults and `terrainScenes` select installed pairs. | Derive by distinct selected raster and its display semantics. Keep the existing horizon, ground footprint, wrapping, and encounter-specific selector. |
| Battle effect and field death | Retail `effects/index.json` retains indexed operands. Mounted scenes include healing/fairy, field death and prepared BLAZE1/2 and BLAST1 in Battles04–07. | Cover only original-art consumers already bound in the selected encounter. Scale complete existing draw frames or indexed chunks at their own composition boundary. Keep timing, reaction, skip and Reduced Flash. Placeholder attacks stay with [#2104](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2104). [Current effect status](../godot/spell-presentation.md#coverage-by-family-and-rank) |
| Original UI rasters | Retail `icons` and `ui` distinguish semantic keys and highlight states. World mounts 204 identity states and 32 field/Item/Shop/Church command states. Battle scenes have separate identity, diamond and cursor keys. | `IdentityIconReader.ReadHd` accepts only the two herb states. `PartyStatusView` consumes them. Shops, command menus, the battle identity owner and target/inspection cursor still use their existing Original selectors. Cover admitted callers and preserve the selected state, disabled behavior, anchor and input geometry. Code-drawn feedback, panels, HUD, layouts, licensed text and device glyphs stay with the [UI design owners](presentation-assets.md#fonts-theme-and-input-glyphs). Source pixel glyphs and advances stay at their native grid. |
| Title and Witch | Retail `entry/index.json` identifies preparation. `entry/entry.json` mounts six 256×224 named layers with title/numeral and Witch arrival/base/blink/mouth. | `EntryPresentationReader` and `EntryPresentationAssets` expect Original dimensions. `EntryBackdrop` chooses by the entry clock. Add one optional coherent alternate set and preserve the 256×224 logical box, numeral offset and expression cadence. Other decoded special screens require admitted callers. |

**Confirmed** by the prepared index and admitted block words. Maps 3, 4, 14, 16, 41, 44, 66
and 73 each have two animation entries. Of their declared banks, 37, 24, 24, 20, 9, 16, 160 and
55 blocks respectively refer to an animation target. The five required indexed tileset streams
are present. For this upper bound, intersect each nine-word block's low ten bits with its map's
`targetStartTile`/`tileCount` intervals. The 345 candidate center blocks define the starting
set for affected neighbors. Filter and cache only the selected phase and affected context.
Retain original layout IDs and timing independently of graphics mode.

Shared world counts are capacity bounds for the mounted art, including later-only IDs. Select
early sprites and portraits by base and conditional population, class/member bindings, scripted
sprite and graphics overrides, scene/tactical selectors, and dialogue identity. Use actor/frame
and palette/weapon selectors to locate close-up assets. Use actual command/identity keys
supported by pre-voyage callers. The existence of a prepared row supplies an input candidate.
A package binding records admission. Native projection records use.

## Family rules and content boundary

- **Terrain.** Keep `172662709b31cb296b55f51894754ce810c30f6c` and the
  [accepted producer/filter](../../../src/sf2tool/remake_map3_ramp.py). Apply the density ramp and
  edge-aware 2× within each admitted logical plane, then nearest doubling to 96×96 per 24×24 block.
  Retain tile priority/flip, blank zero and opaque black. Use a base bank and the same row-major
  3×3 live neighborhood keys. Compare whole planes with cropped, clipped patches with a sampling
  halo. For Map4, feed copies in script order from the effective working layout and include the
  separately reconstructed F624 state. Enumerate reachable copy combinations from callers.
  Include door and roof open/restore, chest replacement and battle source footprints. Clip signed
  or out-of-range plane origins to the valid layout with the runtime's missing-neighbor semantics.
  Map73 has a negative overlay offset in the package. Check explicit scene-plane origins, clipped
  edges and repeated keys before publishing. Conflicting outputs for an identical key are a
  producer/admission defect to resolve before expansion. New combinations can use the center L1
  bank under the existing fallback. A known accepted scene should select its intended context.
- **Transparent and animated art.** Begin with an exact-RGBA edge-aware pass followed by nearest
  doubling at native logical dimensions. Isolate the two walking halves and every explicit
  component/pose before filtering. Keep masks, opaque black, canvas size, registration, frame
  indices and palette selection. For portraits, transform the base and authored replacements
  in their tile-local regions before the existing compositing and mirror step. Use nearest 4×
  for tiny glyph-like marks, shared masked pixels, or a sequence whose contour treatment shifts
  features or flickers. One family/profile covers all its related frames and states.
- **Opaque scenery and screen layers.** Evaluate L1 for repeating opaque terrain or backdrop
  regions whose palette and joining edges fit the accepted Map3 rule. For a scroll wrap, compare
  the seam with the actual repeat. Treat title, numeral and Witch expression layers as registered
  sets with exact alpha. Use the conservative edge-aware/nearest treatment for independent
  layers. Keep all draw and crop coordinates in the Original consumer's logical space. Battle bodies
  include installed 192×192 and 256×192 rows already drawn at half scale. Establish the original
  displayed footprint before choosing a 4× target. Do the same for weapon origins, ground, effect
  tiles and UI slots. Texture dimensions supply density and never implicitly set geometry.
- **Selection and lifetime.** Choose an optional variant for the exact original key. Atomic frame
  and normal/selected groups fall back together. Missing optional coverage can fall back to
  Original or the existing public text path. An invalid declared variant is a content/prepare
  error. Extend the existing `SettingsHost` prepare → preference write → publish transaction to
  the active entry, field or battle owners as each slice lands. Prepare textures before exposing
  handles, keep borrowed textures valid, and key caches by the actual variant and content lifetime.
  Scope residency to the active visit/scene and measure a second visit and a toggle round trip.
  Static terrain validates the active bank on HD preparation and retires field textures and
  alpha masks on map changes. The Content reader retains the admitted encoded rasters.

**Payload placement.** Choose a narrow, optional per-map HD terrain bank file beside the ordinary
world inputs for the bulk rollout. Inline shapes remain readable for Map3 and existing packages.
A visual row selects either its inline bank or `hdTerrainFile`, with a conventional relative
path such as `hd/terrain/map-4.json`. Keep the companion keyed to its map ID, with the existing
`hdBlocks` and optional `hdBlockContexts` shapes and checked raster identity, dimensions and PNG
contracts. The reader verifies the resolved package path and checks each bounded bank separately.
Start with an 8 MiB admission bound per companion and check size before reading. Measure the
margin on Map4 and Map66 and the retained encoded bytes across the package.
Keep immutable encoded bytes and the existing map model.
Preserve the bound on each bank, reject a missing declared file, path escapes, mismatched IDs or
duplicate keys, and keep decoding off the draw path except for the existing on-demand texture
creation. Start with the current bounded, sequential prepare behavior and measure it on actual
Apply. If it exceeds the interaction budget, scope preparation to the current mount and validate
remaining banks at their mount boundary before projecting them.

The [world reader](../../src/Sf2.Remake.Content/Scenarios/PrivateExplorationReader.cs)
retains its 128 MiB limit. The closed-package collector now includes each declared terrain file
alongside encounter paths, external WAVs and named entry PNGs. Private Content opens each
declared bank during world admission and passes its checked immutable rasters to the existing
map model. A bank contains exactly `map`, `hdBlocks` and optional `hdBlockContexts`. Paths use
package-relative JSON names with slash-separated components. Links, absolute paths, empty or
traversal components and mixed inline/companion declarations are rejected. A missing declaration
selects the ordinary inline/fallback behavior. The L1 producer preserves a selected companion's
path when replacing its bank. The earlier painted producer recognizes both forms as already mounted.
See the [package and native comparison](asset-inventory.md#map3-terrain-companion-2174) for the
8 MiB bank bound, first Apply timing and handle lifetime. Keep later actor/UI variants inline only while measured
world size has margin. Review any further placement change as a separate interface decision.

## Sample and budgets

**Confirmed.** The closure is 148 files, 324,058,227 bytes (309.05 MiB). `world.json` takes
100,937,361 bytes (96.26 MiB), leaving 31.74 MiB below admission. On this Windows host, the
existing `package_files` discovery took 0.205 s and 261 MiB process peak working set. This is
discovery with JSON parsing and file stat, with no copy, PNG decode or Godot load. The #2156
Map3 bank has 447 base entries, 767 context rasters, 1,389 context keys and 1,187 distinct
encoded payloads across both banks. Both fields total 1,518,441 bytes in compact JSON. Mean
base64 payload is 1,071 bytes, with a 2,276-byte maximum. The [accepted static and native checks](asset-inventory.md#complete-private-package-and-rebuild-2156)
supply the real fixture. This slice read its published metadata.

**Confirmed** bounded authored calibration. The pinned producer's `TerrainFilter.plane` and
`png_raster` ran on 32 independent 24×24 patterned blocks with dither, holes and opaque black.
A 16×16 plane took 0.490 s to filter, 0.079 s for 256 crops/encodes and 54 MiB process peak.
A 64×64 plane took 8.773 s to filter, 0.069 s for 256 crops/encodes and 113 MiB peak. The
intermediates were 768² and 3072² at L1. All delivered envelopes were 96². Mean base64 was
1,233 and 1,226 bytes. The script used Pillow 11.3.0 and measured the Windows process working
set including imports. These numbers bound the sampled process and omit live residency and
package writes.

| Family or stage | Estimate and scope |
| --- | --- |
| Terrain production | 47,315 plane-cell visits across declared areas/overlays before patches and battle crops. Map3 emits about 0.22 contexts per such visit. Use 0.15–0.35 contexts/cell for planning, about 13k–22k rasters including centers. At 1.25–1.5 KiB per encoded/serialized row allow roughly 16–33 MiB including the existing Map3 fields, before atypical scene or animation growth. At 36 KiB decoded per raster, all-at-once upload would cost about 457–773 MiB before driver overhead. Filter/encode cost scales with cells and sampled patches. Allow 3–6 minutes serial for static terrain, including margin for copies and packaging. |
| Field actors, portraits, UI | 489 ordinary strips with 451 distinct encoded inputs, nine alternate strips with three distinct inputs, and 46 distinct atlases. A literal 4× of all unique originals gives about 45 MiB decoded before copies. The 236 world icon states add at most about 6 MiB decoded. Retain only the early selector closure. Measure PNG sizes and toggle/load latency before projecting inline budget. |
| Battle assets | Seven pre-boarding close-up documents contain 2,287 bindings and 376 distinct encoded inputs. Actor-frame selections give 316 body/87 unique, 256 weapon/28, 62 ground/backdrop/15. The other 176 effect/death and 1,477 UI bindings resolve to 35 and 211 distinct payloads within their families. At 4× logical density, 2× pixel dimensions for existing doubled bodies/weapon/scenery imply about 80 MiB unique decoded across those groups. Keep palette identities and frame semantics even when derived bytes can be reused between encounters. Generate once per eligible source and treatment, then measure actual document growth separately. Measure the caller's effect/identity/cursor shapes independently. |
| Entry and package | Six original sibling PNGs total 41,692 bytes. Six 1024×896 RGBA surfaces occupy 21 MiB decoded. Private disk starts at about 34.37 GiB free. Allow an initial 512 MiB for one staged output and 512 MiB for bounded scratch and receipts, with at least 1 GiB free after both. Preserve the prior stable package and existing relative dependencies. One fresh complete candidate per stage, with serial generation and promotion. Cloud calls and service billing are zero. |

**Inferred** planning figures above. Input PNG compression, scripted context density and exact
number of early selectors vary. Allow 1 GiB process peak for the offline terrain pilot. The
8 MiB companion bound, 120 MiB world JSON planning ceiling, one-second first Apply target and
512 MiB staged-output allowance are engineering starting values chosen for this design.
The implementation owner can revise them from the Map4/Map66 measurements within the delegated
scope. Record measured file size, offline process peak and elapsed time, then Godot admission,
first Apply and residency during repeat visits and a toggle round trip. Keep the existing
128 MiB world reader limit. Track where an estimate exceeded its initial range and link the
receipt with the revised value. GPU residency and full-route elapsed time are **Unknown**.

### Reproduce the census and sample

From the repository root, load the [registered private input configuration](../../../docs/operations/local-private-inputs.md).
The tracked collector resolves the closed package and its constituent file sizes.

```powershell
. ./local/private-inputs.ps1
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2156/pre-voyage-map3-l1-v1'
@'
import sys
from pathlib import Path
from time import perf_counter

from sf2tool.remake_private_package import package_files

base = Path(sys.argv[1])
world = base / "world.json"
start = perf_counter()
files = package_files(world, base / "main-scenes.json", base / "entry/entry.json",
                      base / "opening-inputs")
elapsed = perf_counter() - start
print(len(files), sum(path.stat().st_size for path in files.values()), world.stat().st_size)
print(f"closure discovery {elapsed:.3f}s")
'@ | uv run --locked python -B - $base
```

For a process-peak comparison, use the Windows working-set probe in the authored control below
as the final step of the package collector command.

Use these fields for the read-only census. Open one scene document at a time and count distinct
payloads by their declared identities.

- Join `world.maps[].id` to `world.presentation.maps[].map` by the map table above. Sum
  `len(blocks)` and count distinct `atlas.sha256`. For the plane-cell bound, sum each area's
  inclusive width × height, counting it twice when either overlay offset is nonzero.
  For Map3, count `hdBlocks`, `hdBlockContexts` and the sum of each context's `len(neighbors)`.
  Count distinct raster `sha256` values and measure the length of each `data` string in both
  banks. For the serialized sizes, encode the two arrays separately with Python
  `json.dumps(array, separators=(",", ":"))` and count UTF-8 bytes.
- Count sprite `directions`, alternate `graphics` strips, portrait `raster` and the world menu
  and identity maps separately. For Battles01–07, read `main-scenes.json` and the six
  `encounters[].battleScenes` selections paired with Battles02–07. Classify raster keys using
  each actor's `frames` and `weaponFrames`, the default and `terrainScenes` `background` and
  `ground` keys, then the `identity-icon.`, `command-menu.`, `battlefield-menu.` and
  `battle-cursor.` prefixes. The remaining keys are effects and death art in these documents.
  Count bindings before grouping by `(width, height, format, sha256)`. Estimate decoded RGBA
  bytes as `width * height * 4` per unique group. Sum the sizes of the six files selected
  by the entry document's `resources[].file`.
- Within the prepared `runtime/retail-v1/maps/index.json`, join `records.maps[].id` to the
  selected map IDs. For each map, union the half-open target intervals from its
  `animations.entries` (`targetStartTile` through `targetStartTile + tileCount`).
  Count each block once if any of its nine words masked with `0x3ff` is in that union.
  Check that each `animations.tileset` has its corresponding `tileset-<id>.4bpp` sibling.

Set `$gfx` to the existing pinned graphics checkout for the authored control below.
Each run reports its Windows peak working set. Crop encoding stays in memory.

```powershell
foreach ($size in 16, 64) {
@'
import ctypes
import sys
import time
from pathlib import Path
from statistics import mean

from PIL import Image
from sf2tool.remake_map3_ramp import TerrainFilter, load_filter, png_raster

class Memory(ctypes.Structure):
    _fields_ = [("cb", ctypes.c_ulong), ("faults", ctypes.c_ulong)] + [
        (name, ctypes.c_size_t) for name in ("peak", "rss", "peak_paged", "paged",
                                             "peak_nonpaged", "nonpaged", "pagefile", "peak_pagefile")]

def peak_mib():
    counters = Memory()
    counters.cb = ctypes.sizeof(counters)
    ctypes.windll.kernel32.GetCurrentProcess.restype = ctypes.c_void_p
    handle = ctypes.windll.kernel32.GetCurrentProcess()
    if not ctypes.windll.psapi.GetProcessMemoryInfo(ctypes.c_void_p(handle), ctypes.byref(counters),
                                                    counters.cb):
        raise ctypes.WinError()
    return counters.peak / 2**20

colors = ((0, 0, 0, 0), (0, 0, 0, 255), (42, 76, 37, 255), (75, 119, 54, 255),
          (84, 102, 132, 255), (111, 142, 182, 255), (147, 91, 46, 255), (198, 136, 64, 255))

def authored_block(seed):
    pixels = []
    tone = 2 + 2 * (seed % 3)
    for y in range(24):
        for x in range(24):
            if (x - 10 - seed % 5) ** 2 + (y - 12) ** 2 < 26 and seed % 3 == 0:
                color = 0
            elif (x + y + seed) % 37 == 0:
                color = 1
            else:
                color = tone + ((x + y) % 2 if (x + 2 * y + seed) % 11 < 8 else 0)
            pixels.extend(colors[color])
    return Image.frombytes("RGBA", (24, 24), bytes(pixels))

size = int(sys.argv[2])
filter_ = TerrainFilter([authored_block(i) for i in range(32)], load_filter(Path(sys.argv[1])))
layout = [[(x + y * 7 + x * y) % 32 for x in range(size)] for y in range(size)]
start = time.perf_counter()
result = filter_.plane(layout)
filtered = time.perf_counter()
selection = [(i * 97) % (size ** 2) for i in range(min(size ** 2, 256))]
sizes = []
for cell in selection:
    x, y = (cell % size) * 48, (cell // size) * 48
    raster = png_raster(result.crop((x, y, x + 48, y + 48)))
    sizes.append(len(raster["data"]))
end = time.perf_counter()
print(f"{size}x{size} plane, {len(selection)} crop encodes, {result.width}x{result.height} L1, "
      f'{raster["width"]}x{raster["height"]} output')
print(f"filter {filtered-start:.3f} s; crop/encode {end-filtered:.3f} s; "
      f"peak working set {peak_mib():.2f} MiB; mean/max base64 "
      f"{mean(sizes):.1f}/{max(sizes)} bytes")
'@ | uv run --locked --with pillow==11.3.0 python -B - $gfx $size
    if ($LASTEXITCODE -ne 0) { throw 'Terrain control failed' }
}
```

For the baseline production command and native probes, use the [accepted Map3 replay](asset-inventory.md#complete-private-package-and-rebuild-2156),
the [herb replay](hd-art-pilot.md#local-publication-and-replay-1703) and
[closed package selection](presentation-assets.md#reuse-a-closed-private-package).
A future family producer should accept the same explicit base and a fresh private destination.
Publish a closed candidate with producer scratch unavailable and compare unchanged dependencies
to the accepted base. Carry forward the current audio, entry, display catalogs and opening
selections independently of art coverage.

## Implementation order and acceptance

Use the following implementation order. Main-gate assigns one writer per shared path and
selects the accepted package at dispatch. Each ticket owns the listed paths and its matching
probe or design paragraph. Expand an ownership list only after checking the actual caller chain.
The `asset-inventory.md` trace receives consumer status after observed admission and use.

1. **Static field and tactical terrain.** The [complete selection, calibration and replay](asset-inventory.md#pre-voyage-static-terrain-2175)
   cover the twenty named maps, selected working layouts, detached planes and tactical source rectangles.
2. **Tile animation.** [#2180](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2180) owns the map presentation fields in
   `ExplorationDefinition.cs` and `ExplorationAssetReader.cs` and the field/tactical map texture
   owners. Derive only visible, selected tile slots and the admitted cadence. Compare one
   representative full water cycle, a changed-neighbor seam and suspension/scene resume against
   the mechanism owner. Record constant treatment as static coverage until that passes.
3. **Actor and portrait consumers.** Own `src/sf2tool/remake_exploration_visuals.py` and the
   `ExplorationDefinition.cs` / `ExplorationAssetReader.cs` records and readers. Adapter owners
   are `ExplorationPresentation.cs`, `ExplorationRasterImages.cs` and `PartyStatusView.cs` in
   `remake/game/src/Exploration/`, `BattleMapViewport.cs` and `BattleInspectionView.cs` in
   `remake/game/src/Battles/`, and `remake/game/src/UI/MemberRosterView.cs`. Verify all direction
   halves and mirror on one walking family, a 48px alternate/special pose, shadow and occlusion,
   and a full portrait replacement cycle in dialogue and Member. Observe on a map where terrain
   and actor coverage differ and during tactical movement/death.
4. **Battle close-up, then bound effect art.** Reuse `src/sf2tool/remake_battle_scene_content.py`,
   `remake/src/Sf2.Remake.Application/Content/Scenarios/BattleSceneDefinition.cs`,
   `remake/src/Sf2.Remake.Content/Scenarios/BattleSceneContentReader.cs` and
   `remake/game/src/Battles/BattleSceneView.cs`. The effect slice owns only the existing effect
   presenters it binds, with `remake/game/probes/engine_attack_spell_observation.gd` as the
   observation entry point. For body/weapon/scenery use `engine_battle_scene_observation.gd`.
   Observe a sided attack with the correct body palette, layers/contact/target on both variants,
   terrain-based backdrop and return. Separately observe an admitted cast and recovery/death,
   replay their preset timing/skip and verify actual variant use, reduced flash, result and
   ordered release. Keep #2104's placeholders explicit.
5. **Original UI raster consumers, then entry.** UI starts at `src/sf2tool/remake_hd_identity_icons.py`,
   `remake/src/Sf2.Remake.Content/Scenarios/IdentityIconReader.cs` and
   `remake/game/src/UI/IdentityIcons.cs` / `MenuRasterTextures.cs`. Include the actual field,
   Shop/Church and battle menu callers plus `remake/game/src/Battles/BattleInspectionCursor.cs`
   when their variants are wired. The caller paths are `Exploration/PartyStatusView.cs`,
   `Exploration/ShopServiceView.cs`, `Exploration/FieldServiceMenu.cs`,
   `Exploration/ChurchServiceView.cs`, `UI/DiamondMenuView.cs`,
   `Battles/BattleMenuLayout.cs` and `Battles/BattleInspectionOptions.cs` under
   `remake/game/src/`. Check normal/selected identity in inventory and Shop, one
   command family in each menu namespace and target/inspection geometry. Reuse
   `remake/game/probes/engine_menu_assets_observation.gd` and `engine_hd_graphics_observation.gd`.
   Entry owns `src/sf2tool/remake_entry_content.py`, `remake/src/Sf2.Remake.Content/Scenarios/EntryPresentationReader.cs`,
   `remake/game/src/Entry/EntryPresentationAssets.cs` / `EntryBackdrop.cs` / `SessionEntryView.cs`,
   and closure handling for named alternate siblings. Observe actual title and Witch layers,
   transition, blinking/speaking, Reduced Flash, old-entry fallback, and persisted Apply through
   `remake/game/probes/entry_ui_observation.gd`. Schedule `remake/game/src/Entry/SettingsHost.cs`
   and any shared settings coverage labels serially across affected consumer tickets.
6. **Complete private candidate.** Extend only the established closure/mount entry points and
   update the owning trace and package selection. Reopen the staged package from ordinary title
   with a copy of a legal slot. Read actual textures and state at the named representative
   field, battle, UI and departure points. Check Original/HD/Original, fallback and preference
   persistence, package-relative closure, stable slot bytes during Apply and no adapter errors.
   Run the relevant direct probes and use human viewing for presentation acceptance. Keep
   in-game observations to state, input and projection with existing probes. Activation of the
   play checkout follows independent acceptance and its own authorization.
