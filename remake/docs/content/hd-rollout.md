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
companion and the other nineteen banks. The [tile-animation selection](asset-inventory.md#pre-voyage-animated-terrain-2180) carries
the six switchable title/Witch layers together with the UI, battle, actor, portrait and static terrain selections.

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
| Water and other animated tiles | The prepared map index and original [map animation contract](../../../docs/research/world/map-content.md) identify source tables. Map66's DMA witness is in [map-animation-vdp-v1](../../../tests/fixtures/h3/map-animation-vdp-v1.json). Current world maps carry working layout events and the presenter supports plane scroll and palette effects. | **Confirmed** eight map visuals admit sparse Original/HD tile substitutions. Field and tactical projection share the phase and neighborhood selector. Map66 has a native two-phase water and shoreline witness in both modes. [Mechanism and evidence](#tile-substitutions-2180) |
| Field and tactical sprites | Retail `walking/index.json` includes ordinary directions, class appearances and special streams. The current world has 163 sprite IDs, including 252/254/255, and three alternate graphics names on 252. | All 163 IDs have complete `hdDirections`; all three alternate names have `hdGraphics`. Ordinary field selection uses the saved graphics mode and the sprite's own HD directions. Shadow-removed and alternate graphics use their selected identity and density. Tactical world sprites resolve HD directions; the eight selected battle banks also provide complete HD scene-owned death strips under #2177. [Focused observations](#focused-observations) cover actual field/tactical selection, alternate/shadow fallback and 48px registration; they do not establish every later special dispatch. |
| Portraits | Retail `portraits/index.json` supplies atlases and replacements. The world mounts 46 portraits, all with `hdRaster`. | Field story and both Member surfaces select HD from the saved mode and the selected portrait's own variant independently of terrain coverage. [Focused observations](#focused-observations) cover complete bank production, distinct eye/mouth tiles, mirror and special-pose dialogue. Member/class identity rules are unchanged; this slice does not claim a new playthrough of every class/member menu. |
| Battle close-up bodies and weapons | Retail `bodies` and `weapons` plus the mounted scene documents bind body/palette/weapon, frames and layer order. | `BattleSceneView` selects optional `hdRasters` in complete frame groups with fixed logical dimensions. Campaign admission carries the paired Original/HD donor for shared appearances. [Actual checks](#private-battle-artwork-acceptance) preserve weapon layering, mirroring and geometry. |
| Battle ground and backdrops | Retail `terrain` contains the source variants. Scene defaults and `terrainScenes` select installed pairs. | Derive by distinct selected raster and its display semantics. Keep the existing horizon, ground footprint, wrapping, and encounter-specific selector. |
| Battle effect and field death | Retail `effects/index.json` retains indexed operands. Mounted scenes include healing/fairy, field death and prepared BLAZE1/2 and BLAST1 in Battles04–07. | Cover only original-art consumers already bound in the selected encounter. Scale complete existing draw frames or indexed chunks at their own composition boundary. Keep timing, reaction, skip and Reduced Flash. Placeholder attacks stay with [#2104](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2104). [Current effect status](../godot/spell-presentation.md#coverage-by-family-and-rank) |
| Original UI rasters | Retail `icons` and `ui` distinguish semantic keys and highlight states. The world has 204 identity states and 32 field/Item/Shop/Church command states. Battle render scenes have identity, command, inspection option and cursor keys. | [UI raster consumers](#ui-raster-consumers-2178) bind those keys through Original/HD. The existing selection, disabled behavior, anchors and input geometry still apply. Code-drawn feedback, panels, HUD, licensed text and device glyphs stay with the [UI design owners](presentation-assets.md#fonts-theme-and-input-glyphs). Source pixel glyphs and advances stay at their native grid. |
| Title and Witch | Retail `entry/index.json` identifies preparation. `entry/entry.json` mounts six 256×224 named layers with title/numeral and Witch arrival/base/blink/mouth. | `EntryPresentationReader` and `EntryPresentationAssets` admit Original and optional 4× `hdResources`. `EntryBackdrop` chooses a complete six-layer bank through the shared preference and the existing entry clock, preserving the 256×224 logical box, numeral offset and expression cadence. Other decoded special screens require admitted callers. |

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
2. **Tile animation.** The [shared projection](#tile-substitutions-2180) consumes the mounted transfer
   table in field and tactical views. The [package and reproduction](asset-inventory.md#pre-voyage-animated-terrain-2180)
   cover sparse substitution, a full cycle, the neighboring shoreline, pause and map return.
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

## Opening terrain and palette banks (#2324)

The independent opening successor selects
`derived/issue2324/opening-hd-v1/opening/opening.json`. Keep the accepted full-game world,
battle scene, Witch and display-catalog selections independent. It supplies Map65's127
96×96 base blocks and166 context rasters,183/187's six192×96 strips, and nine exact
sixteen-word storm/Reduced Flash color banks. Map42 and prior actor/portrait HD resources
are retained. The [mechanism and camera agreement](../application/opening-session-lifecycle.md#opening-hd-completion-2324)
owns current acceptance and Unknowns.

The producer reads the already published indexed #2252 opening and pinned graphics checkout
`172662709b31cb296b55f51894754ce810c30f6c`. Slot-preserving density and edge filtering freezes
geometry under its base palette, then re-evaluates generated OKLCH ramps from their two source
slots for each alternate palette. It does not infer slot identity from equal RGB values,
reclassify contours under lightning colors, or recolor actors/UI with terrain-only changes.
Missing optional exact banks retain the indexed Original fallback. Malformed declared banks
fail Content admission. Runtime matches all sixteen words, and never applies the uniform
fade offset a second time to a selected precolored bank.

From the explicit checkout after loading its private configuration, select the existing clean
graphics checkout as `$gfx`. Use a fresh ignored `$output`, or a fresh versioned destination
under registered `derived/`; immutable published versions are never overwritten:

```powershell
. ./local/private-inputs.ps1
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2252/pre-voyage-opening-v1/opening'
uv run --locked --with pillow==11.3.0 python -m sf2tool.remake_opening_hd `
    --base $base --graphics $gfx --output $output
# Add --pilot for three representative blocks and both complete effect direction sets.
$env:SF2_PRIVATE_OPENING_CONTENT = Join-Path $output 'opening.json'
```

The producer requires no runtime reference to its scratch, ROM, upstream checkout, retail
library or graphics checkout. Registered #2252's original provenance remains attached;
the new bank is a local private derivative, not a new original-game observation or a public
asset license. A full run measured2.446s,77.91MiB peak and about26MiB closed output, including
existing audio. The8MiB terrain companion and128MiB world admission bounds are unchanged.

For a short native witness, run `remake/game/probes/prepare_opening_hd_observation.py` through
the same configured `uv run --locked --with pillow==11.3.0 python`, with `--base` pointing at
the published successor's `opening/` and `--output` a fresh ignored directory. It builds
explicit scene-map/entity/palette/camera operations from the admitted inputs; it is not a
replacement proof of natural four-script timing. `--missing` removes only optional palette
banks to exercise the existing fallback. `--missing-art` also removes Map65 HD terrain and
183/187 HD directions; the rest of the admitted HD resources remain independently selectable.
Select the witness's world, party, catalog and six
`opening-inputs` using the [controlled opening launch](presentation-assets.md#controlled-opening-scene-package-2250).
Use the existing Debug adapter and `engine_opening_hd_observation.gd` with `--headless
--audio-driver Dummy --fixed-fps 60`, the witness's `scene-1.json`, and explicit input settings.
Confine saves to the authorized external disposable #2324 namespace; no saved journey is needed.

`SF2_OPENING_HD_MODE` is `hd`, `original`, `missing` or `missing-art`; settings `graphicsMode`
is `original` only for that named case and otherwise `hd`. Repeat the HD witness with `reducedFlash:true`. Set
`SF2_OPENING_OUTPUT` to a fresh JSON path. `SF2_OPENING_HD_SIZE=small` selects1600×900;
default is1920×1080. `SF2_OPENING_HD_FULL=1` runs the unmodified published world through
all four source scripts with the same controlled start, checking scene order and every
observed plane/actor origin against the draw's logical snapshot plus authored quake.
For the short witness, set `SF2_OPENING_HD_PIXELS` to its `expected-hd-pixels.json` to compare
actual uploaded pixels (normalized losslessly to RGBA8 for comparison) with decoded published
color banks. The expected data are independently read from the published PNGs.
No screenshots are collected. Require exit0, `passed:true`, no native errors, expected HD
and Original dimensions, both effect identities, pulse/restoration, black exterior and
released terminal palette. Human visual acceptance remains **Unknown**.

## Actor and portrait implementation (#2176)

The [registered actor package](asset-inventory.md#pre-voyage-actors-and-portraits-2176) extends
static terrain with the already admitted shared sprite/portrait bank. Preparing that modest bank
as a whole avoids a second conditional story selector registry: it includes later-only resource
IDs, but adds no gameplay admission or claim of later campaign reach. The package contains all
163 sprite definitions, three named alternate groups and 46 portraits.

The transparent-family profile preserves every source pixel's alpha and opaque-black mask.
Within those masks it applies the pinned exact-RGBA edge-aware 2× filter, then nearest doubling.
Each walking half is independent. Each portrait's 8×8 tile, including replacement columns, is
independent before runtime composition. This keeps the native mask and replacement boundaries;
it deliberately does not apply terrain palette expansion to faces or transparent sprites.
A pilot on sprites 0, 59, 252 and portrait 5 confirmed unchanged masks and nontrivial interior
smoothing. Full preparation fits the existing inline world limit, so no additional transport is
needed. Human approval of every derivative remains **Unknown**.

Ordinary and alternate exploration directions follow GraphicsMode and the selected sprite key.
Optional `hdGraphics` uses the same keys as `graphics`, with three 4× strips per key. Missing
coverage falls back to that complete Original direction group. A declared malformed group fails
admission or preparation. Shadow removal uses the Original frame's black-word mask at the chosen
density before orientation/head effects. Story and member portraits select their own optional
atlas, then retain existing mouth/eye tile replacement and mirroring. Neither family requires
HD terrain on the current map. Tactical sprites retain logical extent, animation selection and
48px registration. The existing settings prepare → write → publish transaction owns switching. Field and battle
share target-mode actor preparation in `ExplorationRasterImages`; the field no longer skips
Original or fallback preparation after one successful HD Apply.

Scene-owned death/exit frames belong to the effects family in #2177. Kraken uses its existing
scene-owned Original frames and remains outside this pre-voyage bank delivery. These are explicit
coverage boundaries, not a claim that every presentation resource is now HD.

### Focused observations

The native `HdActorObservation` uses authored data in actual adapter consumers. It covers the
Original → HD → Original tactical geometry, special size/registration, independent portrait
selection and the actual exploration alternate/shadow path on a map without HD terrain.
Distinct authored portrait tiles cover all mouth/eye combinations and mirrored composition; absent
HD variants fall back to Original. Each of the field and battle entry points rejects five malformed-resource cases in preparation and retains
the applied mode, texture and geometry. Run after a locked Debug build:

```powershell
. ./local/private-inputs.ps1
& $env:GODOT_BIN --headless --path remake/game --script res://probes/engine_hd_actor_observation.gd
```

For the private candidate use the closed package environment and writable, isolated settings from
[terrain replay](asset-inventory.md#static-terrain-native-replay), with a fresh external disposable
save root. `engine_hd_actor_campaign_observation.gd` extends that driver: `field` starts Map3 at
(32,13), Right (0), speed32 and flags `[0,1,2,32,33,66,80,600,602,603,604,605]`; `battle6` uses the
existing controlled Battle06 start. Set `SF2_HOST_UI_CASE`, `SF2_HOST_UI_START` and
`SF2_HOST_UI_OUTPUT`; the reused driver expects namespace `issue2175` under the newly isolated
root. This namespace does not select the previous ticket's save root. Both cases apply the three
modes through ordinary Settings and read mounted texture pixels/geometry. All visible normal
actors stay 96×96 logical pixels while source frames change 24 → 96 → 24. The battle driver also
checks paused authoritative state and terrain projection.

For `engine_hd_pose_observation.gd`, reuse the [prison controlled input and normal confirmations](../godot/presentation.md#special-pose-dialogue-portraits-2070),
set `SF2_VIEWPORT_CASE=prison`, `SF2_VIEWPORT_OUTPUT`, and use an isolated input-settings JSON with
`graphicsMode=hd`, English and instant text. The seven source-defined dialogue checks cover
Slade's special/normal pose, Sarah, mirrored Slade, narration without a portrait and final control.
Actual HD portraits are 192×256, with the same destination rectangle. Field actors remain HD.

**Confirmed:** independent pixel comparison of 48 actual field, tactical and portrait samples.
For each actor receipt, decode the selected Original/HD strip and take its square `half`/`frame`
at `direction`. For each portrait receipt, decode `hdRaster`, copy the reported `tiles` from the
immutable atlas at 32px per tile, crop the first six columns and mirror when requested. Compare
SHA-256 of RGBA bytes with the receipt's `rgba`; this observes existing resource identity, not a
new change-detection manifest. Inputs and save bytes are not edited by Apply.

The initial field observer used a compact capture without enabling camera observations, producing
null-camera script errors; its exit0 was not acceptance. A subsequent `check` return-value misuse
caused parse failures before running. Enabling the full observation and using the parent's void
check convention corrected both. The accepted field and Battle06 runs passed 36 and 89 checks.
Prison passed all seven dialogue cases and returned to field control. Battle/prison exits retained
four/two ObjectDB warnings tracked separately by #1716; no production errors were observed.
A cold Battle06 run from the published package, with the producer directories unavailable,
passed the same 89 checks without warnings after the prepare correction. The authored negative
cases deliberately report native PNG decode errors and then assert rejection; these are not
production-package errors. The first authored probe leaked its own Sprite2D; freeing it corrected
that probe-owned warning. The final shared-prepare field observation passed 36 checks: first
HD Apply took 201ms and Original 51ms including input release, with two existing exit warnings.
These are single-run measurements on the review machine, not cross-hardware guarantees.

## Battle raster observation

The battle raster binding and packager deliver #2177. They use the existing
scene resource keys, the 4 MiB scene document bound and the Settings prepare/write/publish
transaction. Packager input can be independently prepared art at 2× or 4× raster dimensions,
or the existing scene rasters through the [derive entry](asset-inventory.md#derive-selected-battle-rasters).
Installed scene keys and the working package retain their existing selection semantics.
[Producer and package scope](asset-inventory.md#battle-raster-variant-binding-2177)

Campaign admission shares member-specific allied appearances across declared scenes. Derive
the selected scene families with `--shared-allies` so the producer also covers the allied
body/weapon donor banks. `CampaignScenes` carries HD together with the Original it actually
inserts. Existing destination resources retain their own variant selection. A render-bank
scan before campaign composition alone does not prove closure at the real consumer.

Build the Debug adapter after its locked restore, then run the short synthetic observation
with the owned Godot installation.

```powershell
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$project = 'remake/game/Sf2.Remake.Godot.csproj'
& $env:DOTNET_BIN restore $project --locked-mode -p:UseSharedCompilation=false -nodeReuse:false
& $env:DOTNET_BIN build $project --configuration Debug --no-restore `
    -p:UseSharedCompilation=false -nodeReuse:false
& $env:GODOT_BIN --headless --path remake/game `
    --script res://probes/engine_hd_battle_raster_observation.gd --quit-after 300
```

**Confirmed** in the battle adapter with the authored `early-ranged` action and generated
textures. The probe presents the actual physical scene, publishes Original → HD → Original
on the same wait token, and observes both native and already doubled ally inputs, a partial
enemy frame group, front and rear weapon layers, both weapon flips, ground and backdrop wrap.
It checks a reparent while textures are borrowed, their release on return, and the scene-owned
tactical exit strip crop with transparent and opaque-black pixels. BLAZE2 and BLAST1 compare
the prepared presenter projections with synthetic textures. The deliberate truncated PNG
should report native decode errors and preserve the current binding. A successful observation
prints `BATTLE-RASTER-OBSERVATION PASS` and exits 0.

Preparation also runs when movement has no mounted close-up sprites. It enumerates the admitted
actor/weapon groups, scenery, healing, field exit and existing effect groups, then decodes the
target mode's selected rasters. HD declarations are checked even for incomplete groups, followed
by the selected Original fallback. UI rasters are outside this consumer check. A fully covered
HD group can prepare with an unusable Original payload; applying Original rejects that payload.
The synthetic observation checks those empty-binding cases and retains the mode, scene state
and empty display on rejection. Decode residency is temporary and repeated bindings share one
decode per preparation call.

### Private battle artwork acceptance

Use the [published complete package](asset-inventory.md#published-battle-package), its world,
main scene, entry and six opening inputs in one launch process. Use an isolated disposable save
root and fresh namespace, never the player's namespace. Build Debug with the command above.
The following inputs are controlled formation/level witnesses, not natural campaign reach.

Write each start JSON below under the current worktree's ignored `local/`. For BLAST1, use
`start: {battle:"battle-4", activeAllies:["ally-0","ally-1"], partyInput:"sarah.json",
actors:[{actor:"ally-1",position:{x:5,y:7}}, {actor:"enemy-1",hp:1,position:{x:6,y:7}}]}`.
For BLAZE1/2 set `activeAllies:["ally-0","ally-4"]`, place `ally-4` at (4,7), retain the same enemy, and use `partyInput:"kazin.json"`;
rank 2 also places enemy-2 at (6,8). Keep the outer `start` object. Use the existing
`remake/reference/inputs/battle01-player-ready.json` shape for the two party files, replacing
`allies` with the single record below and naming the controlled witness in `id`, `evidenceOwner`
and `bridgeBoundary`:

| Input | ID/class/level | HP/maxHP, MP/maxMP | ATT/DEF/AGI/MOV | Packed items | Packed spells |
| --- | --- | --- | --- | --- | --- |
| sarah.json | 1/4/10 | 11/11, 10/10 | 9/5/5/5 | 213,0,0,127 | 64,2,14,63 |
| kazin.json | 4/3/5 | 12/12, 13/13 | 9/6/7/5 | 213,0,127,127 | 75,63,63,63 |

Both use status/EXP/kills/defeats 0. The existing observation adapter imports only the accepted
level/book before session construction; these files do not replace live stats or rules.

Set `SF2_BLAST_START` or `SF2_BLAZE_START` to the corresponding absolute start path and
`SF2_OBSERVATION_OUTPUT` to a fresh ignored JSON. Set `SF2_BLAZE_RANK` to `1` or `2`.
The accepted cases set the matching `SF2_BLAST_*` or `SF2_BLAZE_*` `REDUCED_FLASH`,
`SKIP_MESSAGES` and `HELD_RATE` switches to `0`, and `SF2_BLAZE_ENEMY=0`. Launch with
`--input-settings` selecting `{ "formatVersion":1, "language":"en", "textMode":"instant",
"graphicsMode":"hd" }`. Run the owned Godot with:

```powershell
& $env:GODOT_BIN --headless --audio-driver Dummy --rendering-method gl_compatibility `
    --fixed-fps 60 --resolution 1920x1080 --path remake/game `
    --script res://probes/engine_hd_wind_observation.gd -- --input-settings $settings
# Replace the script with engine_hd_blaze_observation.gd for the two allied lethal cases.
```

For HEAL, reuse the wind formation but omit `partyInput` and the enemy HP override. Set
`SF2_HEAL_START`, use `engine_hd_heal_observation.gd` and omit `--input-settings` so ordinary
Settings owns writable isolated preferences. The adjacent enemy supplies a physical attack;
Sarah then selects HEAL on herself using ordinary input. The observer checks all nine fairy
body/wing/dust keys, unchanged logical footprints, MP and recovery, ordinary HD Apply, later
Original playback, and a final HD preference Apply. That last Apply proves preference/state
publication only; the preceding HD cast is the actual HD playback witness. Actor state excludes
the expected sprite/global-rectangle changes, while both RNG values remain unchanged by Apply.
The published-package observation measured 550ms for first HD Apply, 46ms for Original and
418ms for the subsequent HD Apply, including key release. Those single-machine observations
meet the one-second preparation target; they are not a cross-hardware performance guarantee.

**Confirmed:** all four native cases completed without production errors. BLAST1 observed all
15 effect frames, BLAZE1 both flame frames, and BLAZE2 all six composed frames, with actual MP,
damage, death, EXP/gold and next control. The shared donor correction makes the native 96px
Sarah body select 384px art alongside the already doubled weapon/enemy/scenery inputs.
Each observed close-up texture is four pixels per Original logical pixel. Independent RGBA
comparison matched 290 actual texture samples to the selected package; field-exit sprites
used isolated 96px HD frames at the unchanged tactical footprint. The two published-package
cold runs passed with all producer package directories unavailable. Existing 4–10 ObjectDB
exit warnings remain tracked by #1716, not hidden as clean exits.

The initial eight-bank conversion exposed an actual shared-appearance fallback: campaign
admission imported a later Original body without its HD donor. The paired-donor correction
and shared-allies production close that boundary. Initial observer failures included a settings
file using `version` rather than `formatVersion`, a GDScript type inference error, blocked
castle formation cells, an intentionally weak formation reaching defeat, and comparing sprite
projection as if it were authoritative actor state. These were corrected in the observation
inputs/checks; their failed results were retained through review. No screenshot or bulk frame
collection was used. UI/entry art, map tile animation and placeholder spell families remain
with #2178, #2179, #2180 and #2104 respectively; this slice does not claim their completion.


## UI raster consumers (#2178)

The [prepared UI banks](asset-inventory.md#pre-voyage-ui-rasters-2178) supply density for the
existing draw rectangles. Identity icons switch as individual normal/selected pairs. Field,
Item, Shop, Church, battle command and battlefield option menus switch by complete source family.
Prepare combines Item and field commands, so each family retains its own decision even when the
view displays only one command from that family. Target, inspection and movement share the one
tactical cursor key. A missing member of a group selects the whole Original group. Public
unmounted identity and command namespaces retain their existing text path.

World and scene admission verify every declaration's key, payload identity and 4× dimensions.
The active view decodes its resources at mount, including declared frames from incomplete groups.
A failed decode takes the owning view's presentation error path. The menu buttons and cursor
own their two density sets. Identity rows borrow textures from the identity owner. Mount or
replacement prepares textures before they can be drawn. Settings applies to these owners through
the existing prepare, saved preference and projection sequence. Toggling or temporarily covering
a view keeps its texture handles alive.

| Prepared family | Actual view and bounded input |
| --- | --- |
| Item and field command states, carried item pairs | Map3 Field → Item → member → Use, then Back. Field → Member → Prepare shows the five-command mixed-source list, then Back restores Member. |
| Shop command and item states | Map16 item counter → Buy → stock, then Back to counter. |
| Church command states | Map3 priest → Raise focus → Cure focus, then Back. |
| Battle command, spell, cursor and battlefield options | Battle04 Sarah turn → Main → Magic → BLAST target, then Back to Movement → inspection cursor → empty-ground Options. |

### Native reproduction

Use the [published complete selection](asset-inventory.md#pre-voyage-ui-rasters-2178) and the
[ordinary package selection variables](presentation-assets.md#reuse-a-closed-private-package).
Build the Debug adapter with the [locked SDK](../development-and-verification.md#locked-net-workflow).
Use a fresh `issue2178-*` namespace under the authorized external disposable save root. Keep
APPDATA, LOCALAPPDATA, TEMP and logs worktree-local. Use one native process at a time. No mode
override is needed. The probe uses the normal Settings UI and checks the saved preference and
progress after Apply.

Set `SF2_HOST_UI_CASE` to `field`, `church`, `shop` or `battle` and `SF2_HOST_UI_OUTPUT` to a
fresh ignored JSON. Set `SF2_HOST_UI_START` to the corresponding ignored controlled start:

- Field and Church use `{"start":{"map":"map-3","player":"entity-0","position":{"x":32,"y":13},"facing":1,"speed":32,"flags":[0,1,2,32,33,66,80,600,602,603,604,605]}}`.
- Shop uses `{"start":{"map":"map-16","player":"entity-0","position":{"x":35,"y":26},"facing":1,"speed":32,"flags":[0,1,2,32,33,34,663]}}`.
- Battle uses the [Battle04 Sarah/BLAST formation and party file](#private-battle-artwork-acceptance).
  The probe uses Stay once if the leader acts first. It observes the BLAST target before submitting
  the cast.

```powershell
& $env:GODOT_BIN --headless --audio-driver Dummy --rendering-method gl_compatibility `
    --fixed-fps 60 --resolution 1920x1080 --path remake/game `
    --log-file $nativeLog --script res://probes/engine_hd_ui_observation.gd
```

The probe captures real drawn state keys, raster density and decoded payload identity. It also
checks available/disabled rows, input geometry, cursor overhang and unchanged save projection
across Original → HD → Original. The `field` run leaves HD selected. Launch again with the
same namespace and `reopen` to observe persisted HD in a fresh process. The accepted
[F5 Prepare demo](../godot/ui-scene-review.md#scenes-in-the-playable-demo) uses authored text
commands. The connected Map3 Prepare check supplies the actual private raster bindings.

For `partial` or `invalid`, use the same Map3 start with an isolated copy of the selected package.
For `partial`, remove `hdFieldMenu["member.normal"]` and
`hdIdentityIcons["identity-icon.item.71.selected"]` in world presentation. The whole Field menu
and both states of that item use Original under HD, while the Item menu and other item pairs
use HD. For `invalid`, replace `hdFieldMenu["member.normal"]` with
`fieldMenu["member.normal"]`. Admission rejects the declaration as `ui-hd-raster-shape`.

**Confirmed:** field/Prepare, Shop, Church and Battle04 passed their Original → HD → Original
runs. The independent comparison matched 153 rendered RGBA readbacks to the installed raster
keys, including the partial-selection case. The measured Settings navigation plus first HD Apply
took 303–704 ms over those cases on the review machine. The partial and invalid-dimension
observations passed. With both producer destinations moved aside, a fresh process read the
persisted HD preference and used the registered package for field/item art. The registered
Battle04 run passed the same 135 checks from a fresh namespace. These final native runs logged
no adapter errors.

The observations cover real consumers from controlled preconditions. Subjective visual acceptance
under ordinary play, GPU residency and later campaign UI coverage remain **Unknown**.

## Entry layers (#2179)

The entry slice retains High rigor and consumes the accepted #2178 complete package.
Its boundary is the six optional entry layers, admission, existing settings consumption,
closed-package copying and direct production-entry observation. Title geometry, numeral
motion and the existing Witch clock remain the governing behavior.

Owned paths are EntryPresentationReader, EntryPresentationAssets, EntryBackdrop,
SessionEntryView, remake_private_package, remake_hd_entry, the entry observation and
these content owners. All textures are prepared at mount before Settings can publish a
mode. A complete six-layer alternate group uses HD. A partial group uses Original for
all six layers, and every declared resource still undergoes admission and decoding.

The measured base has 168 files and 359,189,445 bytes. Six source images are 256×224.
Six 1024×896 RGBA images require 21 MiB decoded, in addition to Original. Generation
is serial and linear in pixels plus closure bytes copied. Budget 512 MiB per complete
candidate and 512 MiB process peak, with 1 GiB free after staging. Initial free disk was
34,972,065,792 bytes. A title-only pilot precedes full conversion. Reassess on exceeded
budgets, invalid alpha/black coverage or unexpected dimensions. No network/model cost.

Acceptance uses the actual entry, ordinary Settings Apply and persisted reload, title
and Witch frames, fixed projection geometry and decoded source pixels, Reduced Flash,
missing/partial alternate fallback and malformed declaration rejection. Adapter build,
closed-package comparison and the committed engine-scope gate plan complete the slice.

### Production entry reproduction

Use the registered `derived/issue2179/pre-voyage-entry-v1` complete package and the
selection variables in [presentation-assets](presentation-assets.md#reuse-a-closed-private-package).
Load `local/private-inputs.ps1` in the launching process. Build the Debug adapter with the
locked SDK, `-p:UseSharedCompilation=false -nodeReuse:false`. Set worktree-local APPDATA,
LOCALAPPDATA, TEMP and TMP and an external disposable `SF2_SAVE_ROOT`. Select a fresh
`SF2_SAVE_NAMESPACE=issue2179-<unique-name>`, `SF2_HOST_UI_CASE=switch` and an ignored
`SF2_HOST_UI_OUTPUT` JSON path. Launch from the repository root:

```powershell
& $env:GODOT_BIN --headless --audio-driver Dummy --rendering-method gl_compatibility `
    --fixed-fps 60 --resolution 1920x1080 --path remake/game `
    --log-file $nativeLog --script res://probes/entry_hd_observation.gd
```

The probe navigates ordinary entry and Settings inputs, applies Original → HD → Original
→ HD, switches adjustable text to observe speaking, and applies Reduced Flash. Run `reopen`
with the same namespace to check persisted HD and reduced-flash title projection. All six
selected texture handles expose decoded RGBA hashes for comparison with the package PNGs.
The title numeral offset/opacity comes from the draw call. Witch position is (160,92), title
position is (448,92), and both draw 1024×896 within the 256×224 logical frame.

For `legacy`, select the #2178 complete package with the persisted-HD namespace. For
`partial`, copy the entry JSON/PNG siblings to fresh scratch, remove one `hdResources` row,
and point only `SF2_PRIVATE_ENTRY_CONTENT` to that copy. Both select the full Original bank.
For `invalid`, use a fresh copy with either an HD width of 256 or invalid PNG bytes in a declared
HD sibling. The entry reports `entry-content-unavailable`; console diagnostics distinguish
`entry-image-binding` and `entry-image-decode`. A declaration is checked even under Original.

**Confirmed:** published-package switch passed 312 checks and persisted reopen passed 21.
Legacy and partial groups each passed 20 checks before the final numeral observation addition.
The shape and decode negative cases each passed their rejection check. All selected texture
readbacks matched the prepared source/HD RGBA pixels. Producer pilot and candidate directories
were moved aside before the published-package switch and reload. The six alpha and opaque-black
masks equal nearest-expanded source masks. Original entry fields and 167 other dependencies
compare byte for byte with #2178.

Completed initial failures were `missing-field (entry-image)` from requiring `usedTiles` on
HD rows and a probe using the wrong Reduced Flash ID. Optional admission and the `flash` control
ID corrected those faults. A subsequent observation expected speaking under default instant
text and inspected arrival before advancing its page. Ordinary page navigation and adjustable
text now distinguish arrival, base, blink and mouth. These were completed failing observations.
Native exit logs retain the existing two-ObjectDB shutdown warning. There were no final adapter
errors in positive cases. Peak process/GPU memory and subjective visual acceptance remain
**Unknown**; the 512 MiB process budget is an estimate, not a measured result.


The entry producer consumes already prepared images and changes private remake presentation.
Its path is registered in `REMAKE_PRODUCER_PATHS`. On a clean committed head, select checks
with `uv run sf2 verify plan --base origin/main --head HEAD --scope engine` after reviewing
the engine-only semantic boundary. The PR retains local and CI verification results.
## Tile substitutions (#2180)

The High-rigor scope is the existing twenty-map selection and its field and tactical consumers.
The prepared map index identifies eight tables with two transfers to a common contiguous target
per map. At pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, load seeds
the counter to one. `VInt_UpdateMapAnimations` reads the cache at 32 bytes per replacement
tile, queues 16 words per tile to the absolute target tile and wraps at the terminator.
[The four-case fixture](../../../tests/fixtures/h3/map-animation-vdp-v1.json) confirms the
next-VInt transfer boundary including Map66's second 48-tile entry. Those controlled cache
bytes establish timing and addressing. The prepared retail stream supplies the artwork.
Natural scanline timing and exact phase at each story visit remain **Unknown**.

Select a phase by enabled ticks since map mount. Ticks zero and one use the mounted atlas.
Tick two presents the first transfer, then each entry holds for its own counter. Reuse
field simulation service and tactical board presentation time. Each owner suspends under
the existing pause policy. A new board/map mount starts at the atlas. Resuming a retained
view preserves its position. Settings selects density at the same phase. The phase is
derived presentation state. Saves, plane motion, palettes, block words and collision keep
their existing owners.

Prepare sparse raster substitutions offline using the same pinned L1 filter and layout
footprints as static terrain. The source is the prepared map palette and the selected
4bpp cache range. Preserve tile flips, alpha, opaque black and priority. Resolve Original
by center block, HD by the active 3×3 neighborhood then center. A static center next to
water can receive a changed HD edge. Filter only the changed centers and their halo on
observed layout footprints for the selected transfer. Share identical outputs and use the
installed static bank elsewhere. This keeps the pinned graphics implementation offline and
avoids a second filter in the game. Upload on demand within the current map. A tactical
phase change updates only its affected retained drawing.

Calibration is one Map66 cycle, shoreline and pause/resume at a controlled start before
processing the other selected tables. The base world occupies 97,892,927 bytes, leaving
over 34 MiB to its 128 MiB limit. Budget 1 GiB producer working set, 512 MiB per staged
complete package and 1 GiB disk headroom. Keep a single worker. Cost scales with the
selected source ranges, changed blocks and distinct affected layout neighborhoods.
At 36 KiB per uploaded HD block, 500 distinct selected variants cost about 18 MiB plus
driver overhead. Revise from the Map66 pilot before expanding. No network or model cost.

The Map66 pilot added 957,829 bytes to the world. Its first transfer changed 160 centers
and 375 distinct HD context rasters. Its second transfer exactly reproduced the mounted
center pixels, so it needs no additional raster. Twenty enabled ticks divided the two
states. The native field and Battle06 pilot readbacks matched 24 independent RGBA
comparisons against the prepared atlas and replacement stream, including the static-center
shoreline at (33,49). The return through the southern shrine mounted fresh terrain.
The serial pilot bank took 2.43 seconds. Use the same limits for the eight-map run.
