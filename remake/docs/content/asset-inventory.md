# Demo asset inventory and the Granseal art review

The detailed inventory is `manifests/pre-voyage-asset-inventory.csv` in the local product asset repository. Each row identifies a resource by owner, family and ID. Filter by `map-3` for the map, `shared actors` for walking frames, `shared dialogue` for portraits, or `battle-1` through `battle-7` for a particular encounter. `manifests/map3-cast.csv` joins map setup, entity or party member, sprite and portrait. These tables are produced from the selected demo content. They describe available assets; a conditional follower or unused block can be present without appearing in a playthrough.

| Owner in the selected demo | Identity and grain of a row | Where to look or what the variant covers |
| --- | --- | --- |
| 18 field maps | 5,332 map-local block IDs. Each has nine source tile references, layout-use count, sample coordinates and base/overlay area tags. | Original blocks draw at 24×24. Map3 alone supplies a complete bank of 96×96 HD blocks. Door, roof and story copies use the same block IDs. |
| Shared field actors | 462 poses: 77 sprite IDs × three direction strips × two walking phases. The lateral view is also mirrored. | Map3 selects the 96×96 treatment of each pose. A field cell, foot offset, phase and facing keep their existing meaning. |
| Shared dialogue and member sheet | 38 portrait atlases, including the hidden eye and mouth replacement columns. | Original 64×64 or 256×256 on Map3 in HD. Crop, mirror, animation mapping and panel dimensions are independent of texture density. |
| Field interface | 44 item/spell identity states plus 32 states across four command-menu families. | The herb retains its earlier two field variants. Other field icons keep their existing resources. |
| Battle presentation | 1,046 encounter-scoped raster identities. The ID distinguishes actor/frame, weapon, ground, backdrop, effect and menu art as applicable. | These are installed battle resources. Repetition of a character between battles is expected. |
| Title and witch | Six named entry layers, including the numeral, arrival, face and the two expressions. | Dimensions and used tile count refer to the entry pack. |
| Audio | 78 world cues and two entry cues with command, timer and sample rate. | Listed for navigation. Audio has no graphics variant. |

The CSV has 7,040 rows for this pack. Shared art appears once per logical owner and frame. This is the pre-voyage demo inventory; the rest of the retail ROM has a different scope. The [presentation owner](presentation-assets.md) also identifies the bundled UI font, theme and public-resource policy. Code-drawn windows and text are outside the raster table.

## Retail original preparation

**High rigor.** The scale of the retail source, several independent producers, and the existing
world/scene admission limits require separate acceptance for the source inventory, candidate media,
and a consumer-ready package. Use the registered 2 MiB USA ROM and SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`. This work owns offline Original resources for later
feature tickets. Story, rules, HD, localization work and release are outside the preparation scope.

`sf2tool.remake_private_inventory` reads pinned Git objects and accepted H2 metadata. It also inspects
an explicitly selected package and asset Git manifest. The local asset repository's
`manifests/retail-source-and-coverage.csv` records the source ID, shared payload symbol, source
owner and selected package availability. `present` means the named selection contains that resource
or binding. `missing` means it is absent from the inspected selection. `source-only` means the
source ID is known and reusable output at the same granularity is unverified. `not-applicable`
marks absent slots, sentinel or silent targets, and item weapons without a drawn overlay. Alias
slots retain their referenced symbol. Count distinct payloads by source owner and symbol within
each raw family. Consumer rows may refer to the same source, such as normal and highlighted icons.

| Family | Retail source inventory and valid variants | Production and reuse boundary |
| --- | --- | --- |
| Maps, tiles, blocks, layout, foreground and palettes | 79 map headers, 115 tilesets, 16 palettes. Each map records five tileset slots, palette ID and block/layout/area/roof/animation references. The accepted canonical map import records 19,771 decoded block definitions and 315,392 layout words. Blocks stay map-local and foreground uses area/roof/copy rules. | `sf2 texture extract` and `sf2 texture map` are diagnostic producers with split H1 inputs. Runtime reuses prepared maps via `remake_exploration_content` and the selected world. The stable selection contains 50 map presentations. Full block-by-block output closure and dynamic plane observations remain open. |
| Walking and portraits | 720 walking strip pointers with 669 non-sentinel physical payloads. Each ordinary strip carries two phases. Lateral mirror is a projection. Routed specials use their own table. Class rows retain the ally mapsprite base, with class adjustments owned by `GetAllyMapsprite`. 56 portrait pointers resolve to 52 payloads, including eye and mouth operands. | `sf2 texture assets` prepares ordinary sheets. The world carries selected strips, poses and portrait atlases. The source sentinel does not request an ordinary three-strip export. Complete special/action binding remains open. |
| Battle bodies, weapons, terrain and effects | 86 body slots, 408 indexed frames and 167 sprite palettes. Ally class and enemy species map to a body/palette pair. Item ID maps to a weapon graphic/palette pair. Background and ground each have 30 slots with aliases. Separate animation tables, 23 spell containers, four invocation containers, status and transition resources own their sequences. | `remake_battle_scene_content` publishes selected battle scenes. Prepared scenes now share identical frame/terrain/effect payloads across battle documents. The library preserves each bound body, palette, item and animation selection. Full retail coverage requires the missing prepared inputs. |
| Interface, icons, title and witch | 163 assembled icon slots. Spell storage starts at 130 and includes other-art aliases. Selected icons apply the source highlight mask. Spell level is a text selection. Dedicated UI, special-screen and witch menu resources retain their source symbols. | `remake_command_menu`, `remake_entry_content` and the diagnostic `sf2 texture assets`/`misc` producers have different outputs. The stable world includes 32 mounted field/service command states and an entry with six named rasters. Modern panel chrome has its own [presentation owner](presentation-assets.md). |
| Music and effects | 64 music command slots, 37 non-silent targets, 27 silent/alias slots and 56 finite SFX commands. Track music by command and finite SFX by reached `(command, Timer B)` variant. | [Audio capture and admission](../godot/audio.md#audio-boundary) owns the reviewed source intervals and PCM identity. The selected world/entry and existing asset manifest jointly include rows for 19 music and 31 SFX commands. Variant completeness and original waveform parity remain open. |

The current retail CSV covers resource IDs and source relationships at these grains. It does not
enumerate each map cell, each battle frame/palette output, every possible audio timer context, or
the full UI behavior surface. Every new family publication needs its own variant completeness check
before feature tickets take it as a dependency.

### First consumers

| Ticket | Selected source operand and observed availability | Remaining connection |
| --- | --- | --- |
| #1956 | PLDN class/sprite 13. Chester 2 uses field 6 and palette 0, Rick 12 uses 28 and palette 1, Eric 14 uses 34 and palette 3. Those six field/battle bindings are absent. Higins 19 already has field 48 and palette 2 with seven item bindings in Battle26. | Produce and admit the three missing appearances. The accepted field compiler currently admits Hero as its promotion variation. The current battle producer only selects the KNTE special-spear animation 80. Source `GetAllyAnimation` also has PLDN/PGNT cases 81/82. Candidate output needs review at those interfaces before publication. |
| #1956 | `MUSIC_PROMOTION` is decimal 25, hex `19`, with source Timer B `CE` in `music25.asm`. `audio-25-c6` is decimal command 37. No selected 25 PCM was found. | Reviewed finite PCM and the promotion consumer's actual preceding-music restart. |
| #1950 and #1962 | `SFX_JUMP` 87 (`57`) and `SFX_BLOAB` 99 (`63`) are absent from this selection. | Capture and verify the reached timer contexts. Their `SessionAudio.SoundSlots` classifications and gameplay callers remain with the consumer tickets. |
| #1967 | MUDDLE 7, DESOUL 8 and SLEEP 9 have no normal or selected art in the chosen field and Battle26 selection. The inspected zh-CN catalog has 7 and 8 and lacks 9. | Prepare the missing icon pairs and name row. Learned magic admission remains independent of icon availability. |

**Confirmed:** the registered #1738, #1939, #1941 and #1966 worlds also lacked these promoted field
IDs, the priority icon pairs and audio commands 25/87/99. The three registered Battle26 scenes in
that set had only Higins's PLDN binding. This is a bounded reuse search of four package selections.

### Resource budget and observed pilot

Canonical input is 2 MiB plus pinned source and, where required, a retained 24 MB H1 listing.
An indexed 4bpp tile buffer expands by 8× as RGBA. The 19,771 map blocks would occupy about
43.5 MiB as independent native-size RGBA blocks, or 58 MiB with base64, before map layout data.
The indexed battle frame corpus is about 2.2 MiB, about 17.3 MiB if rendered once per frame before
applying palette variants. Per-frame color variants, scale buckets, PNG compression and JSON affect
the final size. Budget a 1 GiB scratch ceiling and one family at a time for the initial visual
pilots. Verify native size and lifetime before choosing a release budget. The existing 128 MiB
world and 4 MiB battle-scene document ceilings still govern assembly.

Audio is separate from the visual budget. Stereo PCM16 at 44.1 kHz costs 176,400 bytes/second.
At a planning ceiling of 180 seconds per non-silent music target and five seconds per SFX context,
37 music targets would cost about 1.09 GiB and one context for each of the 56 effects about 47 MiB.
Retaining both capture and runtime PCM plus a working derivative needs approximately 3.5 GiB.
Two full cycles at the music ceiling take up to 3.7 hours at real time before setup or review.
These are planning assumptions. Measure the first missing finite cue and loop separately. Replan
if the duration bound or reached context count grows, and retain only reviewed required captures.
No parallel emulator sessions or per-frame campaign capture are required.

**Confirmed:** a representative publication reused the registered #1966 world and Battle01 scene
plus the existing #1857 entry/opening inputs. The closed package is 141 files and 284,132,227 bytes.
Copy and direct byte comparison took about two seconds on the primary volume. A separate byte
check of a copy rebuilt from the stable package took 0.64 seconds at 256 MiB peak working set.
The inventory took 2.47 seconds at 257 MiB peak with the selected 102 MB world and pinned source.
Assembly allocates at most one staged deliverable in addition to the supplied input. Its work is
linear in bytes parsed/copied and the inventory visits the source ID tables once. No ROM decode was
needed for this pilot. Extraction time/memory and missing media quality remain **Unknown**.

**Confirmed** from the stable package in the configured Godot Debug adapter. Ordinary New Game
mounted Map3 at (56,3). Six entry textures loaded. Original title command 28 finished once and the
Witch command 10 wrapped from 35.393 s to 6.347 s while still playing on WASAPI. The process exited
0 with empty stderr. A separate ordinary cold Continue mounted the retained Battle26 deployment
with Higins class 13, level 7, 50 HP and Javelin, and read a live texture for field resource 48.
The four Battle26 command icons were mounted, including the selected Stay raster. It exited 0
with a four-ObjectDB-instance cleanup warning. The witness stopped at ActionChoice. It establishes
deployment and mounted scene availability, while the three missing PLDN appearances, their attack
scenes and natural promotion path remain **Unknown**.

Use the [private preparation commands](presentation-assets.md#reuse-a-closed-private-package) to
reproduce the package without producing-worktree assets and to select it for later consumers.

### Prepared library publication

**Confirmed:** the selected closed package stages 10,651 resource bindings across eleven families,
with 1,516 stored payload files. The 239,729,200-byte library includes 18,229,664 bytes of templates,
bindings and passthrough package files. Shared UI accounts for 5,588 bindings and 233 payloads.
Music accounts for 201,384,940 bytes and SFX for 9,284,756 bytes. The UI pilot took about 2.9 seconds.
Resuming all families took 6.2 seconds and assembling from the library took 7.7 seconds.
Reassembly yielded the same 141-file set, including 28 JSON documents equal by parsed value and
113 remaining files equal by byte. That comparison took 1.03 seconds at 368 MiB peak working set.
The UI-only library refused assembly before publishing a destination.

**Confirmed** at the ordinary Content entry points using the library output. The opening reader
admitted all 26 encounters, 50 maps, 146 sprites and 42 portraits. Battle26 resolved five frame
references for the supplied PLDN sprite 13/palette 2, and the selected Stay and item icons resolved
to valid rasters. The entry reader admitted six PNGs. World command 8 retained a loop and command
19 remained finite. One prepared item icon resolved to one payload across 27 documents.
This reader observation leaves the previous runtime receipts in place.

The published version and code commit are recorded in the local asset repository's
`manifests/prepared-library-v1.json`. Select the stable version and use the tracked
[family staging and assembly commands](presentation-assets.md#assemble-from-the-prepared-resource-library).
This publication covers the available package resources. The missing media and original
waveform, raw layout/tile and complete animation variant coverage remain open.

## Map3 coverage and treatment

The outdoor area is 51×32 cells. Two interior areas each measure 11×10. The exterior's roof/facade plane has a 32-row offset and can be copied, revealed or restored. The player package includes all 447 block definitions, including copy sources and the declaration absent from the initial layout. HD supplies separate textures to the existing layer renderer. Tile priority, camera clipping and actor occlusion continue to use the layout and its nine tile words. The source atlas alpha distinguishes transparent index zero from opaque black ink.

The earlier painted master is `masters/map3-materials/surfaces.png` in the local asset repository at `2e8524dcdd94b1568cf2e15e389d660a1f876a54`. The accompanying README records the text-only GPT Image prompt. It supplies sixteen original material studies. The local painter selects the applicable surfaces for grass, leaves, water, earth, stone, shingles, oak, timber and iron. It applies those painted shapes at display resolution using the original's color regions, guide ink and opacity. Neighboring blocks share a boundary treatment. Walking frames and portraits use local contour, shadow and cloth treatments of their source poses; their silhouettes and character designs are retained. No game pixels were supplied to the image service. The herb uses its earlier Scale2x recipe.

## Map3 density-ramp terrain (#1765)

The current private review uses the Map3 L1 experiment from the project's graphics checkout at
`172662709b31cb296b55f51894754ce810c30f6c`. The publisher runs its own `expand_ramp_dither` and
`edge_aware_2x` implementations directly from that pinned checkout. Only offline production needs
the graphics checkout. The experiment, painted master and derived game pixels remain private.
[Graphics experiment and provenance](hd-art-pilot.md#what-the-earlier-experiment-establishes)

The treatment scans aligned 2×2 checkerboards. Eligible pairs have an OKLCH lightness gap of at
least 0.02 and a hue distance of at most 0.7 radians when both colors have chroma above 0.03.
A 12-pixel radius measures the fraction of lighter pixels among the pair. The closest endpoint
or one of up to four intermediate levels replaces the checkerboard. Other textures and checkers
containing a transparent pixel pass through to edge-aware 2× scaling. The original atlas distinguishes
opaque black from transparent index zero. Lossless nearest doubling fits the 48×48 L1 result into
the existing 96×96 HD slot. Original uses the same admitted 24×24 blocks as before.

**Confirmed:** neighbors affect both local density and edge reconstruction. The producer filters
each area and its foreground separately, then cuts the results into blocks. A 3×3 row-major group
of block IDs contains all the pixels needed for one block. The optional `hdBlockContexts` field
binds groups to 96×96 rasters. The renderer looks up the live working layout on each plane, including
after a roof, door or flag copy. The area edge uses -1 for a missing neighbor. Known layout edits
are prepared from their source rectangles. An unpublished combination selects the center block's
own L1 image. Older worlds continue to select their installed HD bank or original art.

### Rebuild and review

Select a complete ordinary private world and its matching opening scene. The observed review
used the #1759 publication with town Search and shops. The publisher replaces just the Map3
terrain variant. It preserves every original raster, existing character/portrait variant and
gameplay field, and copies the existing scene and dependencies. The #1759 world is 127,169,379
bytes. The review world is 125,267,166 bytes and the complete package with plates is about 191 MB.
The retained 128 MiB world-document limit applies. Allow one output package and 1 GiB scratch on
the output volume. Filtering visits only Map3's 447 blocks, three areas, foreground and bounded
copy patches. Peak memory includes the parsed world and one plane's filter data. No cloud calls.

```powershell
. ./local/private-inputs.ps1
# $world and $scene select the ordinary package; $gfx selects the pinned graphics checkout.
# Choose a fresh ignored directory for $output.
uv run --locked --with pillow==11.3.0 python -B -m sf2tool.remake_map3_ramp --world $world --scene $scene --gfx-root $gfx --out $output
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $output 'world.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $output 'scene.json'
```

Keep the matching entry, display catalogs and battle inputs. Use the ordinary title **Continue →
slot 1** from a separate review namespace with a compatible Map3 save near (32,13). Start in
Original. Open **F10 → Settings → Graphics → HD → Apply**, then switch back at the same position.
The prepared local `local/issue1765/Play Map3.cmd` launches this review. The neighboring
`replay.ps1` runs the same Continue and settings sequence with its own fresh disposable slot.

| Static comparison at the same coordinates | Observation |
| --- | --- |
| Grass and trees at (10,15)–(15,20) | 103 of 82,944 L1 RGB pixels differ from the historical composite. The input already differs at 20 Original pixels. |
| Riverbank at (20,14)–(27,17) | Block-local filtering differed from the old L1 at 501 pixels, including 362 in the boundary band. The final assembled blocks match all 73,728 reference pixels. |
| Wall and roof at (29,8)–(33,14) | The old renderer converted black to transparency and treated palette zero differently. The current Original differs at 1,882 of 20,160 RGB pixels. The candidate differs at 8,148 of 80,640 L1 pixels. Its foreground uses the admitted alpha and remains independent of the ground. |

The complete static exterior reassembled from the actual published 96×96 payloads, at native
L1 density, equals the separately filtered and composited plane image byte for byte. The bank
contains **58 distinct opaque colors** across base and context variants, up from 15. The earlier
84-entry table includes unused levels. Inspect `review/grove.png`, `riverbank.png`, `roof.png` and
`continue.png` for the same crops in Original and HD. These are private asset plates.

**Confirmed:** the [running host witness](../../game/probes/engine_map_art_observation.gd) loaded
the ordinary slot on the integrated UI. In 180 visible ground cells, selected textures used
15 → 42 → 15 opaque colors. Eleven foreground cells used 8 → 12 → 8. Both directions kept the
same tile passes, screen rectangles, four display pixels per logical pixel, paused progress and
slot bytes. Returning to Original reused its texture handles. The existing portrait changed
texture density within the same 144×192 panel. The native exit can still report its known
two-ObjectDB-instance cleanup warning. **Unknown:** player acceptance of the terrain style.

## Earlier painted package

For the optional Map3 HD package, first prepare or reuse the
[complete ordinary menu/identity/Item/Shop world and matching opening battle scene](presentation-assets.md#final-publication-and-ordinary-selection).
Menu publication alone does not generate the painted Map3 bank. Select that world, its scene,
the existing entry pack and the material master above as explicit read-only inputs. Set
`$materialMaster` to that master file and `$output` to a fresh ignored private output directory;
the publisher refuses an existing output or a world already containing painted Map3 blocks.
If a matching HD pack is already prepared, reuse its two outputs and skip regeneration.
To rebuild, after loading the configuration below, set `SF2_PRIVATE_EXPLORATION_CONTENT` and
`SF2_PRIVATE_BATTLE_SCENE_CONTENT` to the complete menu pack without the painted bank.
Run from the repository root. This is an offline Pillow dependency only.

```powershell
. ./local/private-inputs.ps1
# Confirm/select the non-HD menu world and its matching scene before rebuilding.
uv run --locked --with pillow==11.3.0 python -m sf2tool.remake_map3_hd --world $env:SF2_PRIVATE_EXPLORATION_CONTENT --scene $env:SF2_PRIVATE_BATTLE_SCENE_CONTENT --entry $env:SF2_PRIVATE_ENTRY_CONTENT --master $materialMaster --out $output
# Select both outputs in this same launch process:
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $output 'world.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $output 'scene.json'
```

The final pack retains the original content IDs and gameplay revision. Keep the existing entry,
battle/static inputs, display catalogs, save root/namespace and settings selections. Use the
[ordinary launch](../../README.md#run-locally) in the same configured process; New Game or Continue
then reads the matching `world.json` and `scene.json` above. Existing compatible slots retain their
compatibility rules; do not copy or rewrite a player's save to select artwork.

In Settings, choose **Graphics: HD** and apply. The settings transaction prepares the installed HD
content before publishing the change. On Map3, the outdoor and two interior areas use 96×96 blocks
in place of 24×24; actors and portraits gain density while logical geometry and panel size stay the
same. Choose **Graphics: Original** and apply to switch back. Other maps keep their installed art;
a world without the optional bank retains Original even when HD is selected. This makes package
selection distinct from the graphics preference. Human art-direction approval remains separate.

The local `review/` folder contains all three map plates, two interior comparisons, a street detail, a character comparison and the two CSV files. The map plates compose source areas and the static roof copy; a moving actor or an opened roof must be reviewed in the game. These private images belong on the reviewer's machine.

The selected HD world is about 121 MB (decimal), below the unchanged 128 MiB input limit. One release adds roughly 6.5 MB to the herb world. Preparation is bounded by the admitted cells, actor poses and portraits, and makes one set of complete field plates; it does not collect gameplay frames. Derived assets and slots remain private.

For the actual host witness, give `engine_map_art_observation.gd` an isolated `issue1717-check-*` namespace containing a compatible Map3 slot near (32,13), the installed pack, writable settings initially in Original and an explicit report destination. It loads through Continue, changes Settings in both directions and reopens the member sheet. Compare the actual drawn block widths, pass geometry, portrait density and paused progress. Field time can legitimately advance the main RNG between menu visits. Human art-direction approval remains with the player.
