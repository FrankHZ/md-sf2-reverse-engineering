# Presentation and Local Asset Architecture

- Status: **Proposed**
- Scope: modern high-DPI presentation and local product-asset boundary for the Godot remake
- Applies to: the local product-asset repository, its candidate builders, and presentation data embedded
  in prepared private worlds
- M5 status: the legacy Godot consumers described below (asset catalog, base viewports, HUD preview,
  battle bridge, camera and locomotion projections in `remake/reference/game`) and the `public-synthetic`
  profile were retired with the reference host at
  [M5](../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m5-implementation).
  Sections describing them are historical at base
  [`8a581a82`](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/8a581a82e297ea2947cc9837e661752163d2806d/remake/docs/presentation-and-assets.md). The asset repository contract,
  `sf2tool.remake_asset_build`, `sf2tool.remake_assets` and prepared-world rasters remain current.

## Purpose

The remake is a fan project whose product direction is to run with the user's existing local Shining
Force II graphics, character animation, music, and sound-effect material. The main code repository
does not replace that material with synthetic or substitute art. A separate local-only asset
repository owns the product material and its history, while the main repository owns the deterministic
interfaces and tools that admit, derive, select, and present it.

This document fixes the presentation architecture before the current procedural diagnostic HUD is
migrated. It keeps gameplay state independent from render scale, establishes a modern high-DPI asset
model, and preserves the thin-Godot direction in
[Architecture](./architecture.md). It does not claim original camera, composition, timing, final pixels,
or complete fidelity.

The common host renders map atlases, source sprites and portraits embedded in a prepared private world
(see [Exploration and programs](./exploration-programs.md#presentation-and-private-content)). That
bounded rendering does not reclassify the profile as complete product presentation.

## Decision Summary

1. Keep a **960-by-540 logical presentation grid**. It is not a gameplay, map, collision, or evidence
   coordinate system.
2. Use **4x authored raster masters** for newly authored raster material and deterministic **2x and
   4x runtime buckets**. Existing original raster material remains the canonical local source and is
   deterministically converted into candidate cache entries and then reviewed into the same two
   versioned runtime buckets.
3. Rebuild HUD chrome and semantic input glyphs from project-authored **SVG sources** versioned in the
   local asset repository. Reviewed rasterized/imported derivatives are versioned with the runtime
   assets after they pass local review.
4. Mount the local asset repository or an exported product pack explicitly at the outer Godot
   composition boundary. Missing, incompatible, or stale content fails closed and never silently
   falls back to synthetic presentation while reporting product success.
5. Keep product source material, editable masters, accepted runtime assets, and manifests in the
   local-only asset Git repository. Keep reproducible cache, scratch, previews, Godot import state,
   and machine paths ignored. The main code Git repository owns consumption interfaces, closed
   schemas/tooling, and minimal project-authored test fixtures, not product resource payloads.
6. Keep tracked project-authored packages as test content. They are not the product art, music, or
   presentation source. (The former `public-synthetic` smoke profile was retired at M5.)

## Coordinate and Scale Model

Four coordinate spaces remain explicit:

| Space | Owner | Meaning | Must not own |
| --- | --- | --- | --- |
| simulation | Domain/Application | map IDs, tiles, collision, movement, battle cells, turns, logical time | pixels, DPI, window size, texture paths |
| presentation | Godot adapter | 960-by-540 layout units, anchors, safe areas, camera projection | gameplay mutation or collision rules |
| asset master | local asset repository `source/` or `masters/` | canonical source used to derive render material | live window size or session state |
| output | Godot window/viewport | physical pixels, display DPI, aspect ratio, user UI scale | authoritative game state |

The 960-by-540 grid preserves the current host layout and provides exact integer relationships to
1920-by-1080 and 3840-by-2160. A presentation unit is not automatically one source-art pixel. Domain
positions are projected into this grid by the Godot presenter and never change when the window,
display, UI scale, or asset bucket changes.

The root keeps Godot's `canvas_items` scaling model. A later implementation may use `expand` sizing,
but it must keep a centered 16:9 gameplay/action frame. Additional area on 16:10, 21:9, or taller
displays is project-authored bleed, backdrop, or letterbox space; it does not reveal more map or alter
camera behavior. Godot's design-size, `canvas_items`, aspect, anchor, and accessibility-scale behavior
is described in its [multiple resolutions documentation](https://docs.godotengine.org/en/stable/tutorials/rendering/multiple_resolutions.html).

The default title-safe inset is 48 logical units horizontally and 27 vertically. The effective safe
rectangle is the intersection of that design inset and the platform-reported safe area. Essential
text, commands, meters, focus indicators, and acknowledgements remain inside it. Layout verification
must cover at least 16:9, 16:10, 21:9, and 4:3.

### Product startup window policy

The current product policy applies only to an available explicit `PrivateLocal` profile. The project
keeps a 960-by-540 physical fallback for PublicSynthetic, headless verification, and small displays,
while enabling HiDPI, resizable windows, `canvas_items`, and centered `keep` aspect behavior. A real
window that still has that unmodified fallback at startup measures its actual client area, decorated
window, and current usable screen. It selects the first decorated target that fits from this fixed
client ladder: 1920 by 1080, 1600 by 900, 1280 by 720, then 960 by 540. The selected client is centered,
and the root window receives a runtime minimum of 960 by 540. This avoids requesting a decorated
1920-by-1080 window that cannot fit a normal 1080p usable desktop.

An engine-selected fullscreen or maximized mode, an explicit command-line resolution, or another
already established non-default client size is preserved. A PrivateLocal client below 960 by 540, or
a desktop on which no minimum decorated tier fits, fails that profile without falling back to
PublicSynthetic. Headless verification validates the 960-by-540 virtual content surface and does not
pretend its minimized 64-by-64 root is a desktop client, query a screen, or position a window. Real
window sizes are Godot client/backbuffer pixels; Windows DPI is not applied a second time.

The policy is startup-only. Presentation assets are admitted after the final client is established,
and the existing limiting-dimension rule selects one resident bucket for that session. A later resize
continues to scale or letterbox the fixed logical canvas with the startup bucket. Changing buckets
requires restart until a separate implementation closes stable-size detection, atomic multi-asset
replacement, old-mount retention on failure, and fullscreen/monitor/DPI transitions.

## Master and Runtime Buckets

### Source authority

The word *master* means the highest-authority source for a particular asset family:

- existing world, character, portrait, icon, and other original raster material: the admitted bytes
  versioned under the local asset repository's `source/` are the master;
- newly authored raster additions: a 4x source under `masters/` is the master;
- HUD chrome and semantic input glyphs: the SVG under `masters/` is the master;
- music and sound effects: the admitted local audio file under `source/` is the master.

A 4x image generated from an original low-resolution source is a cache derivative, not a new master
and not a newly synthesized artwork.

### Bucket selection

The image pipeline produces two deterministic local derivatives for renderable raster and SVG
families:

- **2x bucket:** two physical pixels per logical presentation unit;
- **4x bucket:** four physical pixels per logical presentation unit.

For a full-frame equivalent these are 1920-by-1080 and 3840-by-2160. Runtime computes the effective
physical-pixel scale of the safe 16:9 frame, including the user's UI-scale setting, and selects the
smallest available bucket that is not below it:

- effective scale at or below 2: use 2x;
- effective scale above 2 and at or below 4: use 4x;
- effective scale above 4: retain 4x and report the clamped presentation tier until a later tier is
  explicitly accepted.

Generation first writes candidate buckets under ignored `cache/`. After review, accepted 2x and 4x
outputs are promoted together with their manifest update into the local asset repository's tracked
`runtime/` history. Only the selected accepted bucket is resident. A stable display or user-setting
change may replace the bucket only at a later startup. The current runtime does not hot-swap after a
resize; a per-frame or animation-driven bucket switch is forbidden.

This two-bucket policy is preferable to 2x-only because 2x cannot remain crisp at 4K/high DPI. It is
preferable to 4x-only because 1080p and lower-end systems need not carry permanent 4x memory and
bandwidth cost or continuously downsample it. Storing both buckets has 25 percent more uncompressed
pixels than storing only the 4x derivative, but loading only one avoids that same increase in resident
texture memory.

## Local Asset Repository

`md-sf2-remake-assets` is the actual local-only product-art and audio repository. It is a sibling of
the main code repository and of the separate graphics R&D repository; neither sibling is a nested
dependency or a durable substitute for another.

Its bootstrap identity is recorded as an origin point, not as a floating current version:

| Property | Bootstrap value |
| --- | --- |
| branch | `main` |
| root commit | `c82aa7e353e808e4ef12117f247c5c7065839801` |
| root tree | `96d499cabc0d3310b12168cc9c8d392b1862c1b6` |
| remote policy | no remote; tracked pre-push hook rejects pushes |
| binary storage | ordinary local Git history, not Git LFS |

The tracked layout is:

| Path | Authority |
| --- | --- |
| `source/` | original art/audio and other input material in its admitted source form |
| `masters/` | editable HUD SVG and other authored masters |
| `runtime/` | reviewed runtime-ready 2x/4x textures, UI, fonts, music, and sound assets |
| `manifests/` | local identity, semantic mapping, derivation, import policy, and code-compatibility records |

The repository ignores `cache/`, `scratch/`, `previews/`, temporary output, and Godot import state.
Reviewed binary art and audio are intentionally trackable. A local build or verification receipt
records the exact asset-repository commit it consumed; it never records the machine's absolute path.

## Local Product Asset Pack

The product consumes either an explicit checkout root of `md-sf2-remake-assets` or a pack exported from
one exact asset commit through the existing `private-local` outer profile boundary. Presence of a file
or directory never selects the profile implicitly. The composition root receives the fully qualified
local mount and gives only the Content reader the inputs required to validate and load it. The Godot
catalog receives that reader plus path-free semantic selection metadata; it never receives or resolves
a resource path. The admitted descriptor and build receipt carry the asset commit identity; no branch
name or working-tree state substitutes for it.

The implemented v1 pack contract provides stable semantic asset IDs and closed raster records. Future
families must extend that ownership for at least:

- map/world raster sources and their layer or animation grouping;
- character, portrait, icon, and effect animation sources;
- music tracks and sound effects;
- font faces required by the configured theme; and
- reviewed 2x/4x runtime entries and their derivation identities.

Absolute paths stay in the outer composition and Content loader. Domain, Application, Godot catalog,
snapshots,
receipts, status, smoke markers, exceptions, and ordinary logs carry only semantic IDs, capabilities,
and path-free diagnostics. Godot selects presentation tokens from authoritative snapshots and asks
Content for a defensive byte copy; it does not place resource paths or bytes into gameplay state.

Product-profile admission is fail-closed:

- missing or unreadable pack: `Unavailable`;
- malformed, unknown, duplicate, or incompatible record: `InvalidPackage`;
- source or derivative identity drift: `ContentDigestMismatch`;
- stale generator/policy/cache relationship: `CacheStale`;
- requested presentation capability not declared by the pack: `UnsupportedCapability`.

These names describe the prospective diagnostic categories rather than fixing a code API. A failed
product mount may show a path-free project HUD error, but it must not start the tracked synthetic
presentation while claiming that the local product pack is active.

## Deterministic Derivation and Cache

Ownership is deliberately split:

| Surface | Tracked owner |
| --- | --- |
| consumer ports, closed schemas, generator/verifier code, and small project-authored test fixtures | main code Git repository |
| original product material, HUD SVG and authored masters, reviewed runtime buckets, product font/theme/audio data, and manifests | local asset Git repository |
| decoded intermediates, candidate derivatives, font/audio import caches, previews, Godot `.godot/`, exports, captures, logs, receipts, and scratch | ignored local directories |

Each cache entry must bind:

| Field group | Required closure |
| --- | --- |
| source | semantic asset ID, source digest, dimensions/format, pack capability |
| derivation | policy ID, generator version/artifact identity, deterministic parameters |
| output | bucket, dimensions, format, digest, color/alpha policy |
| import | filter, mipmap, repeat, color-space, and Godot resource category |

Generation writes to a fresh ignored `cache/` destination and publishes a candidate receipt only after
the complete cache validates. Promotion copies reviewed outputs into `runtime/` and commits them with
the matching manifest change. The runtime reader matches the supplied mounted commit string plus fixed
manifest and selected-output identities before loading; it does not inspect a Git checkout. The
separate local preflight proves the actual checkout commit/tree/clean/no-remotes identity. A
caller-recomputed output digest cannot convert an incompatible source or policy into an accepted
runtime entry.

HUD SVG candidate derivation uses the official Linebender `resvg` 0.47.0 Windows release archive,
locked by URL, byte length, SHA-256, the exact unique `resvg.exe` member, and exact version output in
`remake/presentation-toolchain.json`. The builder supplies no system fonts, accepts only the closed
static project subset below, uses explicit geometric/image rendering hints, and requires two
independent runs to produce byte-identical 2x and 4x RGBA8 noninterlaced PNGs with valid chunk CRCs.
It writes only a fresh direct child under ignored `cache/` and generates the existing
`manifests/presentation-assets-v1.json` shape inside that candidate. It does not promote or commit.

Private Map 3 base-atlas candidate derivation uses the already accepted fixed ROM, tileset-metadata,
and palette-metadata roots. Actual bytes and caller pins must all match those roots before metadata
parsing. The tool derives, rather than accepts from a caller, palette zero and ordered tileset slots
`[0, 37, 43, 53, 66]`. It reuses the maintained Stack decoder and the accepted pure 4bpp, palette,
tileset-sheet, and deterministic PNG helpers. The ignored candidate contains only the exact selected
private source bundle, one master, two runtime buckets, and the single-asset candidate manifest.

The master is one 128-by-320 atlas: five vertical 128-by-64 segments in accepted slot order, each
containing 128 8-by-8 tiles on a 16-by-8 grid. Runtime buckets are exact nearest-neighbor 2x and 4x
derivatives with no mipmaps or repeat. Palette index zero is transparent and the other indices use
the existing `v << 5 | v << 2 | v >> 1` channel expansion into straight-alpha sRGB RGBA8. This is a
project-authored color-preserving review/runtime candidate policy. It does not prove Mega Drive
analog output, display behavior, colorimetry, hardware chronology, or final-pixel parity.

The separate `build_map19_20_base_atlas_candidate` API and `map19-20-base-atlas-candidate` command
produce one fixed shared Map 19/20 family: palette 0 and ordered slots `[6, 23, 44, 53, 62]`.
Both maps must retain that selection in the complete fixed metadata tables. After the existing
closed-shape and provenance checks, each map's tileset/palette metadata addresses must agree and its
six-byte ROM header must contain the selected palette and five ordered slots. The same header join
also protects the retained Map 3 API. Original data provenance remains owned by
[Technical Graphics](../../docs/research/technical-graphics.md), `sf2-map-tileset-decode-v1` and
`sf2-map-palette-static-v1`; this adds no original rendering conclusion.

The fixed families share the existing Stack/palette/PNG pipeline, two-pass byte comparison,
fresh ignored candidate transaction and zero-remote checkout checks. Callers supply input paths and
identity pins, not arbitrary map indices, palettes or tileset slots. Map 3 keeps its existing
asset/source/policy IDs, source-bundle format, geometry, runtime paths and receipt shape.

The separate `build_map21_base_atlas_candidate` API and `map21-base-atlas-candidate` command
produce only the fixed middle-tower Map 21 family: palette 0 and ordered slots `[6,23,44,53,8]`.
Its metadata address join and actual six-byte ROM header must agree with that selection; the shared
castle family's final slot 62 cannot substitute for 8. The master and nearest 2x/4x buckets retain
the same 128-by-320, 256-by-640 and 512-by-1280 shapes.

Map 21 uses asset `world.map21.base-tileset-atlas`, source
`source.world.map21.base-visual-selection`, policy `private-local-map21-base-nearest-rgba8-v1`,
and capability `private-local-map21-base-tileset-atlas-candidate-build-v1`. Paths use
`world/map21/` under `source/`, `masters/` and `runtime/`. Its source bundle starts with ASCII
`SF2-MAP21-BASE-VISUAL-SELECTION-V1` plus NUL, bytes `[21,0,6,23,44,53,8]`, the 32-byte
effective palette and the five 4,096-byte decoded tilesets. Its receipt explicitly records
`acceptedMapIndices: [21]`, palette 0, the actual selected slots and all three fixed input digests,
alongside the existing source/generator/output identities. It contains no local absolute paths.

The separate `build_map40_base_atlas_candidate` API and `map40-base-atlas-candidate` command
produce only Map 40, palette 3 and ordered slots `[94,95,96,97,58]`. The fixed family owns its
palette index; metadata selection, the six-byte ROM header join, actual palette source range,
source bundle and receipt all use that index. The selected `MapPalette03` source is checked before
the existing index-zero transparency transform. The other fixed families retain palette 0 and
their source format, pixel policy, IDs and receipt shape; callers cannot supply an arbitrary palette.

Map 40 uses asset `world.map40.base-tileset-atlas`, source
`source.world.map40.base-visual-selection`, policy `private-local-map40-base-nearest-rgba8-v1`,
and capability `private-local-map40-base-tileset-atlas-candidate-build-v1`. Its paths use
`world/map40/` under `source/`, `masters/` and `runtime/`. The source bundle starts with ASCII
`SF2-MAP40-BASE-VISUAL-SELECTION-V1` plus NUL, bytes `[40,3,94,95,96,97,58]`, the 32-byte
effective palette and five 4,096-byte decoded tilesets. The receipt records `acceptedMapIndices: [40]`,
`acceptedPaletteIndex: 3`, the fixed slots and input identities. Its master is 128-by-320; buckets are
exact nearest 256-by-640 and 512-by-1280. The existing five-file transaction, two-pass determinism,
zero-remote checks and rollback apply unchanged. This candidate does not admit a Godot mount,
original presentation fidelity, init/F507 execution, later warps, Battle 01 or H3/H4.

#### Map57 candidate policy

`build_map57_base_atlas_candidate` and `map57-base-atlas-candidate` produce only Map57:
palette8, ordered source selection `[94,98,99,255,255]`. The
[reference proof](./map03-playability-plan.md#map57-atlas-candidate-and-unloaded-slot-boundary)
shows no use of slots 3/4 by the fixed layout, while preserving the two unused block records that
do contain slot4 references. This is a controlled storage policy, not proof that original skipped
VRAM banks were zeroed or safe for arbitrary block rendering.

Slots 0/1/2 decode real tilesets 94/98/99 under the existing exact source/Stack/decoded checks.
Physical slots 3/4 retain sentinel255 identity and become separately tagged, project-authored
4096-byte zero buffers and RGBA `[0,0,0,0]` segments. Tileset255 is never read or decoded and
slot indices are never compacted. The master stays 128-by-320, with nearest 256-by-640 and
512-by-1280 buckets; the final 128 logical rows are transparent. Other families retain their
five-real-slot source format, palette, geometry, output pixels and rejection behavior.

The asset/source IDs and paths use the existing `map57` family convention. The policy is
`private-local-map57-base-nearest-rgba8-authored-empty-slots-v1`. Its binary source bundle is:
ASCII `SF2-MAP57-BASE-VISUAL-SELECTION-V1` plus NUL; bytes `[57,8,94,98,99,255,255]`;
ASCII `SF2-PROJECT-AUTHORED-ZERO-SLOTS-V1` plus NUL and bytes `[3,4]`; the 32-byte effective
palette8; then all five ordered 4096-byte slot buffers. The last two are authored, not decoded
source payloads. The receipt's `unloadedSlotPolicy` and `slotSources` state this distinction
alongside the unchanged input/asset/source/generator/master/2x/4x identity fields.

Use the same fixed-input arguments in the recipe below with command
`map57-base-atlas-candidate` and a fresh `--candidate-name 'map57-base-atlas-review'`.
The writable asset root must be a separate ordinary local clone inside this code worktree's
ignored local area, with its origin removed and the existing rejecting hook configuration applied.
An asset linked worktree is incompatible with the maintained verifier. Verify the clone's exact
baseline, clean state and zero remotes before and after generation. The maintained command writes
only its five candidate files below ignored cache and performs both deterministic render passes.
Review the master and both runtime PNGs locally along with the receipt. It does not update the
canonical asset repository, promote a manifest or bind a Godot consumer. Asset acceptance and
the consumer require separate ownership and review.

The castle candidate uses asset `world.map19-20.base-tileset-atlas`, source
`source.world.map19-20.base-visual-selection`, policy `private-local-map19-20-base-nearest-rgba8-v1`
and capability `private-local-map19-20-base-tileset-atlas-candidate-build-v1`. Its master and nearest
2x/4x buckets have the same 128-by-320, 256-by-640 and 512-by-1280 shapes. Paths use the
`world/map19-20/` family under `source/`, `masters/` and `runtime/`; the source bundle starts with
ASCII `SF2-MAP19-20-BASE-VISUAL-SELECTION-V1` plus NUL, bytes `[19,20,0,6,23,44,53,62]`, the 32-byte
effective palette and the five 4,096-byte decoded tilesets. Its receipt records both map indices,
palette, fixed input identities and the existing generator/source/output identities without local
absolute paths.

From the code repository root, with explicit absolute input and asset-root variables and exact Git
pins already selected, generate a fresh ignored candidate:

```powershell
uv run python -m sf2tool.remake_asset_build map19-20-base-atlas-candidate `
    --asset-root $assetRoot --expected-commit $assetCommit --expected-tree $assetTree `
    --rom $romPath --expected-rom-sha256 '9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9' `
    --tileset-metadata $tilesetMetadataPath --expected-tileset-metadata-sha256 '2EA6AB3485CAE4F92F31647C05233F0E1C07E81CCB02806706A51F9F0C1E087F' `
    --palette-metadata $paletteMetadataPath --expected-palette-metadata-sha256 '4F977B4B3EB8E731D2ABB6664F36030487DC186D267E66E9C2DAF3CB211007AB' `
    --candidate-name 'map19-20-base-atlas-review'
```

For Map 21, use `map21-base-atlas-candidate` with the same input options and
`--candidate-name 'map21-base-atlas-review'`. For Map 40, use `map40-base-atlas-candidate` and
`--candidate-name 'map40-base-atlas-review'` with those same fixed input options.
Map57 uses `map57-base-atlas-candidate` and `--candidate-name 'map57-base-atlas-review'`;
its additional authored-slot policy is described above.
Each command writes exactly five files in the named
asset checkout's fresh ignored `cache/` candidate and emits a receipt.
Local image review and a separately accepted source/master/runtime/manifest transaction must precede
consumer binding. This builder does not promote assets or change the current Godot runtime. The
current consumer binds the independently accepted Map 19/20 and Map 21 families described below.

The current reviewed local pack at commit `3bf31fa02c4ca9ee04e06be1efc97bcc2bac5880`, tree
`880d845e1368a87f185cb8bb18b7a3cc6d5882f2`, and manifest SHA-256
`4F0F6BEFE809A3163704C6AAF4DC007A31B30D8DC8B58256DCE5AFF7BBAB0E40` contains the two HUD assets,
the exact Map3, shared Map19/20 and independent Map21/40/57 atlas families, the player initial-reference-frame family, the three controlled-player
locomotion sheets, and one entity-142/Astral UP two-half reference sheet. The explicit
PrivateLocal consumer mounts all four exploration families, plus Map57 when Battle01 inputs are selected, at the selected 2x or 4x scale after Content
rechecks the closed pack; only the current family supplies the rendered base. Godot validates decoded RGBA8 dimensions and projects every physical bucket texel through the
already authoritative working-layout/block/tile/slot/flip selection. A 2x bucket becomes a
576-by-336 ImageTexture and a 4x bucket becomes a 1152-by-672 ImageTexture; `DrawTextureRect` maps
either onto the same 288-by-168 logical crop, and the current project-authored player marker remains
a later logical overlay. Horizontal and vertical flips reverse the complete physical tile axes,
including subpixel coordinates. The reviewed pack is presentation-authoritative at playable startup:
Godot validates that every scale-squared atlas sample is identical before any logical collapse, so the
existing `exact-nearest` identity cannot silently admit arbitrary reviewed art. It does not reopen the
ROM or tileset/palette metadata, never reads private source bundles or masters, and does not embed the asset root in
Application/state/receipts, and does not place private data in a PCK. This is a diagnostic
local presentation policy, not original rendering. The player families are
resolved from the same exact accepted transaction only when the base atlas is selected. Godot decodes
the selected 2x or 4x PNGs and retains them in the base viewport. Session position and locomotion
state remain authoritative; the catalog, presenter, and viewport do not own gameplay, facing,
counter, half, or motion-phase rules.

The sibling private Map 3 player reference-frame builder and bounded consumer remain narrower than an
animation system. The builder fixes the controlled player selection to ally zero, resolves regular map-sprite zero,
selects the accepted DOWN-facing source slot two without horizontal mirroring, and Basic-decodes the
two-half 576-byte source. Only half zero enters the candidate as a 24-by-24
`initial-reference-frame`; the name does not mean standing, idle, or visible at controlled admission.
The ignored candidate holds the exact selected compressed source plus accepted palette words in its
private source bundle, one deterministic master PNG, nearest 2x/4x runtime buckets, and a single-asset
closed manifest. Palette index zero is transparent and the other indices use the existing
project-inferred straight-alpha sRGB channel expansion.

Both actual ROM bytes and the caller pin must match the fixed accepted ROM root before pointer,
decode, or palette work. The receipt identifies the source choice and derivation policy but contains
no private path or payload. The reviewed source/master/runtime/manifest transaction is local only.
Content rechecks its exact mounted commit, manifest, semantic asset, dimensions, bucket digest, and
nearest/no-mipmap/no-repeat policy before returning a defensive runtime-byte copy. Godot decodes and
projects that copy only for the explicit PrivateLocal base-atlas path; source/master material never
enters runtime or a PCK.
Admission rendered pixels, live palette at admission, DMA/cache completion, original rendered-color
parity, wall-clock/display timing, and final pixels remain **Unknown**.

The sibling entity-142 candidate builder is fixture-bound and does not decide original product
selection. It validates the fixed-ROM root, the exact accepted Map 3 record/physical slot/logical entity,
map-sprite 209, UP-facing source, compressed bytes, both decoded-half identities, and palette 3. It
renders both 24-by-24 halves in source order into one 48-by-24 master and deterministic nearest 2x/4x
buckets using the same project-authored transparent-index and channel-expansion policy. The separately
reviewed local transaction retains all five source/master/runtime/manifest objects. The explicit
base-atlas composition now resolves its runtime bucket through the existing Content reader and mounts
it only after the immutable private snapshot retains the exact `ms_map3_Entities` record 17 population.
Godot applies `project-authored-half0-diagnostic-idle-v1` as the fresh-bind frame and places the exact
record relative to the current project-authored camera. The bounded
`project-authored-two-half-diagnostic-cadence-v1` adapter state then retains both cropped source-order
halves and cycles `0 -> 1 -> 0`, holding each half for exactly 30 fixed Godot physics callbacks. It
ignores callback `delta`, wall time, `SimulationStep`, and gameplay state; ordinary snapshot, movement,
and camera reprojection preserve the phase, while a fresh exact bind resets it. Static-overlay mode
projects, advances, and draws no entity. Logical entity 142 and physical slot 17 remain this exact
route-bounded binding, not a general entity or animation model. Which half was visible at interaction
time, the exact original animation counter/cadence, idle meaning, visibility and offscreen/pause
behavior, lifecycle, walking, occupancy, collision, event behavior, and all-entity presentation remain
**Unknown**.

The accepted `sf2-map3-original-player-locomotion-animation-runtime-v1` owner closes the bounded
controlled-player timing gap without turning either half into a universal idle pose. A sibling local
candidate builder keeps the original three source slots as three 48-by-24 two-half sheets: `up`,
`horizontal`, and `down`. RIGHT reuses the horizontal sheet with the accepted horizontal mirror;
there is no duplicate right-facing payload. Masters and nearest 2x/4x buckets retain both halves in
source order. The builder validates the accepted admission counter/half, blocked-attempt ordering,
and successful 13-tick half sequence before deriving the family, but does not itself implement a
runtime clock or publish the private source.

The bounded consumer implements that small state machine in Application. Its admission projection is
DOWN/facing 3, half 1 selected at counter 25, with stored counter 26. A blocked attempt changes facing
before the one sprite selection and settles without motion. A successful attempt owns tick 1 at the
source, advances the remaining 384 source units in twelve 32-unit transitions, and settles on tick 13;
counter selection, wrap, and half choice are computed by the accepted small transition rather than a
hard-coded Godot sequence. The existing grid movement remains the semantic traversal result, while
the separate Application locomotion projection carries the in-cell visual offset.

Godot requests one Application animation transition from `_PhysicsProcess`, selects only the named
`up`, `horizontal`, or `down` sheet and half, mirrors the horizontal sheet only for RIGHT, and maps the
Application offset across the existing 24-pixel logical cell. It never advances animation from a
render-frame count and never derives phase from `SimulationStep` or `LastTraversal`. This engine-native
fixed-tick scheduling does not claim an accepted wall-clock, display, DMA, cache, or final-pixel timing
equivalence. The earlier initial-reference texture remains a distinct, narrower retained asset and is
not renamed as idle or standing. The lower-level viewport can still project it when no locomotion
mount is supplied, but explicit base-atlas composition fails closed when that mount is unavailable;
it does not use the retained texture as a product-startup fallback.

The same Godot projection owns a deliberately modern, project-authored camera follow. It derives its
visual focus only from the authoritative private snapshot and the Application locomotion source plus
offset. Each accepted 32-unit transition becomes two logical pixels; the focus remains at the
existing column 6/row 3 while the 64-by-64 working-layout bounds permit, then clamps at the map edge.
The base compositor samples at that continuous sub-tile origin into the unchanged 288-by-168 visible
surface, using at most one bounded trailing block row or column. Typed payload and exact local-atlas
projection tests and the playable exact local-atlas path share that crop, and edge-scale remains a
post-composition treatment. Blocked movement retains
the current camera; settled movement reaches the same tile-aligned crop as the earlier projection.

This is a 9A presentation choice, not a consumer of the original camera-command runtime fixture. It
does not define original target, dead-zone, clamp, Plane A/B, parallax, autoscroll, layer-switch,
VInt/DMA, display, or final-pixel behavior. The fixed static-overlay diagnostic remains separately
anchored and does not consume the live camera policy.

The earlier `initial-reference-frame` transaction remains a separate, narrower product input. Its
half-zero name and bytes are not rewritten by the locomotion family, and it still carries no
standing, idle, or original-admission-visible claim. The locomotion consumer uses its separate
Application-owned state and does not rewrite the earlier asset's identity or meaning.

The optional named `edge-scale2x` world treatment is an explicit 9A presentation deviation layered
after that exact admission. It is accepted only with the explicit private base-view and base-atlas
selection; exact-nearest remains the default. Godot first requires the selected atlas bucket to be an
exact nearest replication within every scale block, then collapses that admitted atlas to the current
288-by-168 logical base-world crop from the authoritative working layout, block selection, tile slots,
and flips. It never recreates that source by reopening ROM or extraction metadata at runtime.
Standard Scale2x rules compare exact RGBA neighbors across the complete composed crop, with clamped
crop edges, and copy only colors already present. The 2x bucket receives that 576-by-336 result. The
4x bucket deterministically nearest-doubles each accepted Scale2x output pixel, rather than making a
second edge decision. Reprojection repeats the composition and treatment after movement or working-
layout mutation, so atlas-cell boundaries cannot become seams or stale context. The resulting texture
is still drawn into the same logical rectangle before the separate player and HUD overlays. This
bounded base-layer treatment does not admit or describe original layer-2 transparency, camera,
animation, colorimetry, final pixels, H4, or 8C fidelity.

The separately selected `--private-map3-static-overlay` mode is a project-authored **STATIC OVERLAY
DIAGNOSTIC**, not a gameplay camera or original layer-2 renderer. It requires the explicit private
base view and derives the sole non-zero layer offset from the already admitted typed Map 3 area
catalog. The preview anchors at that area's admitted main-layer minimum, reads both passes from the
latest authoritative working layout, and draws the offset pass over the main pass. Palette index zero
retains the main pass; every non-zero palette index replaces it. No player or entity marker is drawn in
the preview.

Both passes deliberately use the already admitted Map 3 palette as a diagnostic policy. That choice
does not establish the original exploration-mode background palette source. The tile priority bit does
not change source pixels in this preview, and the mode makes no claim about priority relative to
players, entities, or sprites. It does not execute roof/setup/init copies, move or fabricate session
state, or imply that the controlled admitted start naturally reaches the previewed area. Exact-nearest
2x/4x atlas projection remains a transport check; `edge-scale2x`, when also selected, runs only after
the complete main-plus-overlay logical crop has been composed. Original camera, parallax, autoscroll,
layer switching, animation, timing, hardware output, and final pixels remain **Unknown**.

The local `md-sf2-gfx-remake` experiments remain useful R&D for comparing nearest, edge-aware, xBRZ,
and color-ramp treatments, but they do not select product art or a general raster upscaler. In
particular, 3x is not a runtime tier and no single upscaler becomes a global default from those
experiments. Its current ignored window SVG is not accepted input because it contains semantic text
and an embedded raster payload forbidden by the source contract.

The first accepted product master is instead a deliberately reduced, project-authored frame that
retains the measured 112-by-24 geometry, five-layer border, and translucent blue fill while removing
the R&D text and embedded original icon. The R&D repository is lineage and comparison material, not a
runtime dependency or authority.

## SVG HUD Source

HUD SVG tracked in the local asset repository's `masters/` is restricted to a deterministic,
reviewable subset:

- explicit `viewBox`, logical dimensions, and stable element IDs;
- paths and basic geometric shapes with declared fills/strokes;
- no scripts, external URLs, machine paths, embedded raster payloads, or local original assets;
- no semantic text embedded as `<text>`; Godot's font system owns localizable text;
- no effect whose rendering is not closed by the selected pinned rasterizer.

The SVG sources reconstruct HUD frames, panels, focus/focus-loss states, cursors, meters, separators,
and semantic input glyphs in the project's visual language. They are not screenshots or extracted UI
payloads. Candidate derivatives are generated into ignored `cache/`; reviewed 2x/4x derivatives are
promoted into tracked `runtime/` with their manifest.

Godot can rasterize SVG at import and can use `DPITexture` for oversampling, but its SVG support is a
bounded ThorVG subset. The local product pipeline therefore requires a pinned, verified derivation rather
than depending on arbitrary runtime SVG parsing. See Godot's
[image import documentation](https://docs.godotengine.org/en/latest/tutorials/assets_pipeline/importing_images.html).

## Texture and Color Policies

Texture policy is declared per asset family instead of globally:

| Family | Default policy | Mipmap boundary |
| --- | --- | --- |
| SVG-derived HUD and glyphs | lossless sRGB RGBA, clamp, linear filter at selected bucket | off at bucket-native scale |
| source-crisp world/character material | color-preserving deterministic integer derivation; nearest sampling where crisp cells are intentional | off unless a verified camera/downscale path needs it |
| reviewed smooth world/character derivative | named, pinned edge treatment; linear sampling only after visual approval | per family, not global |
| large backgrounds | declared lossless or reviewed compression and filtering | on only when material is materially downscaled |

The default original-material derivation preserves source color identity. Dither smoothing, expanded
ramps, interpolation, post-processing, or other visual treatments are explicit presentation deviation
profiles under 9A. They are never silently inferred from the source and never change gameplay or
evidence state.

HUD color tokens are project-owned theme values. They may preserve the original-style visual language
through compact ramps, strong silhouettes, layered frames, and restrained highlights, but they remain
separate from original palette/CRAM evidence and from the private world-material palette.

## Fonts, Theme, and Input Glyphs

The local asset repository tracks product font files, theme data, and their manifests. The main code
repository tracks only the consuming schema/API and fixture configurations. Product configuration owns
logical sizes, weights, line height, outline/shadow tokens, fallback roles, required glyph ranges, and
whether a role uses dynamic or MSDF rendering. Font and theme resources are explicit asset-pack entries
with stable semantic IDs.

MSDF is appropriate for headings or roles that span a large scale range after optical testing. Small
body text and dense CJK text may use hinted dynamic rendering when it is clearer. Required glyphs are
prewarmed to avoid first-use stalls, and the fallback order is deterministic. Semantic text is never
converted into HUD SVG outlines.

Input hints bind semantic actions to project-authored SVG glyph IDs for the current device and binding.
Every glyph has a localized text fallback. Keyboard, controller, and accessibility remapping select a
glyph; glyph filenames and presenter nodes never own command rules.

## Accessibility and Aspect Behavior

The presentation profile supports UI scale values of 100, 125, 150, and 200 percent without changing
simulation coordinates. The chosen value participates in safe-area layout and asset-bucket selection.

Every actionable or stateful distinction must have a non-color channel such as text, shape, icon,
pattern, focus outline, or motion-independent cue. The theme contract will own contrast targets,
reduced-motion behavior, and bounded flash behavior. Accessibility choices remain explicit 9A
presentation deviations and do not rewrite original behavior contracts.

## Audio Boundary

Music and sound effects follow the same explicit local-pack rule as graphics. Their source and accepted
runtime forms are versioned in the local asset repository. The product uses the user's current
material; it does not generate replacement tracks or substitute sound effects. Application emits
semantic audio cues, and the Godot audio adapter resolves those cues to admitted local asset IDs.

The private audio implementation uses the existing version-1 pack with `kind: audio` entries.
Each entry retains its original command, semantic cue, source capture identity, derivation identity,
and one PCM16 WAV runtime payload. `timerB` identifies the original scheduling context. Music commands
are unique; finite SFX may have multiple reviewed `(command, timerB)` variants. Cue IDs remain unique.
Preflight verifies the pinned local Git payload, SHA-256, rate, channel count, sample-frame count and
explicit loop range. Missing content is an error; the former synthesized JOIN chords are removed.

`python -m sf2tool.remake_audio --help` describes the offline candidate materializer. It accepts a
reviewed capture SHA-256, command/timer context, explicit inclusive/exclusive sample-frame interval,
and optional loop boundaries. It creates a new candidate under the worktree's ignored `local/`
directory. It neither launches an emulator nor promotes assets. Original captures, native observer
configuration, failed attempts and runtime WAVs remain private. The source capture and its observed
boundaries must be reviewed before the existing local asset admission transaction.

The compiler embeds the admitted PCM and metadata in the private world. Content checks its identity
again. The Godot session owner creates `AudioStreamWav` resources at their admitted rate and channel
layout, with no gain normalization or pitch change. Streams are resident; resource ownership survives
exploration/battle view replacement. Looping is enabled only when both admitted sample boundaries
are present. Natural `Finished` and deliberate replacement/stop remain distinct observations.
`GameRoot.ReadAudioObservationJson()` exposes the live players and the last 64 ordered start/stop/
finish receipts, including PCM identity, command/timer, loop range, session revision and wait token.
Inspectors must collect these during execution; sequence gaps are not complete playback evidence.

**Confirmed source structure:** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`sounddriver.asm:Load_Music` writes the music header's Timer B value; `Load_SFX` initializes channel
records without replacing it, and `UpdateSound` advances them under that timer. Note-frequency
register writes use separate frequency tables. SFX variants therefore retain the reached timer
context; `pitch_scale` is not a tempo substitute because it would transpose the sampled sound.
The existing [sound inventory](../../docs/research/sound-data-inventory.md) owns the ROM/source
parity, command identities, type-specific channels and stream termination facts.

**Confirmed in a bounded private host observation:** JOIN opens its text window, waits for the
actual finite stream to finish, reissues previous music, and then accepts acknowledgement. The
source `csc08_joinForce`, `FadeOut_WaitForP1Input`, `PlayMusicAfterCurrentOne` and
`ApplyFadingEffectAndZ80BusUpdate` support this order. The source previous operation reissues the
prior command; it does not restore a PCM offset. Map-area field/battle music selection uses imported
area music and `PlayMapMusic` substitutions. Application retains wait tokens and player input
ownership; Godot reports service completion.

The six music resources and 25 finite SFX timer variants occupy 104,374,046 bytes in the generated
private world; its reader permits 100 MiB. Other package limits are unchanged. A WASAPI Godot run observed Town's natural forward-loop
wrap while `Playing` stayed true without `Finished`, then ordinary Map 3 inputs reached JOIN.
An early acknowledgement retained the sound wait token. The finite stream ended naturally;
previous Town restarted at its beginning before acknowledgement returned to the field with flag 603.
The ordered player receipts were start 8, stop 8, start 19, finish 19, start 8. Native exit was zero.
This checks real resource playback and input ownership in that bounded route; it does not establish
original waveform/timing equality. The complete reached resource inventory is admitted at local asset
commit `7219d9c6ac2e72d86b3d62b2042e153e4dbba34f`, tree
`ff1d4f37271cff828f7a0bd1efde083c56915e55`, manifest SHA-256
`82B8E862B7438D44AD65B3C0CEF328B8CFD8E53653FE39D0266590B541B3C448`;
checkout preflight passed. Its tracked
`manifests/audio-town-join-provenance.json` retains original capture identities, sample cuts,
reproduction parameters, loop evidence and the host receipts. Source WAVs and capture observers
remain in that private repository. Use the explicit asset commit/tree/manifest pins with
`python -m sf2tool.remake_assets checkout`; the temporary world is not an implicitly selected pack.

`manifests/audio-reached-inventory-provenance.json` owns the remaining capture protocols, exact
sample cuts and actual host receipts. Music commands 2, 5, 34 and 38 passed actual forward-loop,
continued-playing and clean-release observations through explicitly controlled field-music selections.
That tests their resource consumption, not natural battle-action triggers. The 25 finite SFX variants
passed original driver termination, native lifecycle, complete WAV and unchanged-installation checks.
Their cuts retain unchanged PCM, including 441 samples before and after the selected audible interval;
the measured quiet separation permits at most one PCM16 unit. No normalization, noise gate or waveform
rewrite is applied. Initial-context variants retain their explicit inferred Map 3/Town context.

Ordinary accepted field warps publish `warp-started` before transfer; rejected destinations and script
transfers do not. `WarpIfSetAtPoint`, `ProcessMapEventType1_Warp` and the source `WARP_SFX` reset own
this distinction. Audio consumes that origin marker once, before selecting destination music.
An actual R1 input prefix observed command 89 starting once in Town's inherited Timer B context.
The existing `door-opened` result selects command 92. Reattaching a result does not replay its effects.

Adjustable text consumes the imported sprite speech identity on alternating non-space revealed
characters; acknowledgement and choices consume command 67. Accepted battle spell/item selection
and cancellation use 66, and confirmation uses 67. These are modern UI bindings, not reproduction of
every original menu/window edge. An authored two-text adapter fixture with the original Bowie
sprite/speech binding observed command 73 replacement and natural `Finished`, plus ordinary Enter's
67, under both CB and D2 timer contexts. It collected every audio receipt without sequence gaps.
The full inventory includes the original Woman sprite 195's 72/CB voice; a failed reduced fixture
omitted that entry while incorrectly assuming the entity was Sarah. That fixture failure does not
establish a missing reached asset. The complete pack and source-derived speaker metadata agree.

**Modern text-mode difference:** instant text and reveal-all input suppress incremental speech
bleeps; adjustable text uses the configured modern reveal speed. Adjustable-mode consumption PASS
does not prove speech-cue parity for instant mode, original cadence, or every scene. H4 must retain
suppressed or unobserved cues as differences/unavailable; 9A input/settings acceptance does not grant
an original-audio parity claim. This behavior is not a newly accepted 10A deviation waiver.

Town's inferred PCM loop uses an exact recurrence of all ten original driver channel records at
frames 2757 and 8618 in the controlled export. The reviewed candidate loop is sample frames
1,894,900 through 6,208,289 (exclusive), at 44,100 Hz stereo PCM16. The export bridge's startup
buffer and possible adaptive resampling prevent claiming exact hardware sample phase. Reproduction
requires the retained private capture/configuration, candidate manifest and ordinary-input host
observation; neither the loop interval nor a waveform seam metric alone proves the host result.

### Action-choice menu audio

A successful battle submit that enters or leaves `BattleSelectionStage.ActionChoice` plays command
65 once through the existing UI audio callback. This boundary takes precedence over generic 66/67
selection/confirmation sounds: entry, cancel back to movement, and selection of Stay or a target
action all use 65. Rejected submissions, accepted operations that remain in the same stage, and
redraws do not add a menu-boundary cue. Other existing UI bindings are unchanged. The rule uses
before/after selection state for any actor; it adds no engine event, window RNG or gameplay delay.

**Confirmed source structure:** pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`,
`code/gameflow/battle/battlefunctions/battlefunctions_2.asm:ProcessBattleEntityControlPlayerInput`
selects the outer action menu after movement/stop validation. Its `ExecuteDiamondMenu` caller in
`code/common/menus/diamondmenu.asm` uses `MoveWindowWithSfx` for both opening and A/C-confirm or
B-cancel closing. `code/common/windows/windowengine.asm:MoveWindowWithSfx` submits command 65
before moving the window; directional menu selection's 66 is a separate edge. The retained original
65 request PC `0x48F4` identifies this shared window routine, not all its parent callers. No original
request-count target or cue on every dialogue/window follows from it.

This is the current modern ActionChoice lifetime's source-backed audio mapping, not complete
original diamond-menu/window presentation, nested menu behavior, timing or input-polling parity.
The consumer uses the already admitted original 65 resource and current music timer context.
Set `SF2_BATTLE_SCENE_MENU_AUDIO=1` with the source-world scene observer below to exercise ordinary
entry, cancel/reentry, Stay, target selection, rejected and same-stage inputs for two actual actors.
The observer checks exact per-input starts, the actual 65/BD player, natural `Finished`, legitimate
same-slot replacement and no redraw replay or gameplay work during playback-tail collection.
Normal/reduced-flash runs preserve the same semantic inputs and ordered session observations;
this bounded check does not establish the separate instant/adjustable text or full host Option A
requirements. Missing content or timer support remains an explicit error.

### Finite SFX overlap

The session audio owner uses a modern whole-PCM mixing policy: disjoint and partially overlapping
groups continue playing together; equal or fully covered groups replace the old clip. Partial
overlap preserves the old whole clip and starts the new one, so a Herb tail cannot block ordinary
menu input. Repeated requests replace their previous group instead of accumulating voices.
An admitted command without a
reviewed slot classification fails with `sfx-slots-unavailable`. Existing content/timer errors remain
explicit. No extra action wait or gameplay tick is introduced for a cosmetic effect's tail.

**Confirmed source structure:** pinned `sounddriver.asm:Load_SFX` initializes only non-FF channel
entries, and its type-2 effects have separate temporary YM records. `StopMusic` preserves these
extra records. The existing H2 sound inventory's `sfxModel.entries.activeSlots` supplies the bounded
classification in `SessionAudio`; command names and type numbers alone are insufficient. Commands
83 and 102 occupy YM6 and PSG tone3 respectively; 113 occupies YM4/5 while 67 occupies PSG tone3.
The former single player truncated 83 on level sound 102 and recovery 113 on confirmation 67 in
retained ordinary controlled scene observations. These are independent cue-lifetime defects, not
waveform-fidelity requirements. Classification of PSG noise and tone slots expresses replacement
identity only; it does not prove acoustic independence or reproduce their chip coupling.

`ReadAudioObservationJson()` includes each live effect's cue, command, source slots, actual
`Playing`/position and start receipt sequence in `sounds`. Every voice retains its own actual
`Finished` callback. Already-ended voices await that callback; `Playing == false` alone is not
reported as completion. Replacement and disposal disconnect callbacks and release streams/nodes.
The receipt window remains bounded; inspectors must collect it without sequence gaps.

For the existing controlled physical reward and Herb source-world observation modes below, set
`SF2_BATTLE_SCENE_DISJOINT_AUDIO=1`. The observer samples actual simultaneous players and checks a
pair of independently completed instances (83/102 or 113/67). Rapid later tone input may replace a
tone; it must not truncate the independent reaction sound. After product input release, the observer
collects the playback tail and requires unchanged gameplay state. This is observation latency,
not a product wait. Pair normal/reduced-flash runs using the same semantic inputs and compare their
session observations; no full host Option A conformance follows from an audio overlap check.

**Confirmed, bounded actual consumer observations:** the controlled physical reward and Herb modes
each passed normal/reduced-flash WASAPI runs with concurrent players and independent natural
completion. Each settings pair produced identical ordered session observations and final shared RNG;
playback-tail collection left the input-ready gameplay state unchanged. An independent adapter-only
owner additionally observed reverse arrival, starts after completion, same-slot/full-coverage
replacement, preserved playback on unknown/missing rejection, duplicate-result
suppression and stream/callback disposal. It reused admitted PCM and did not mutate a game session.
PSG noise/tone coverage was a slot-lifetime check only. Earlier observations that ended before the
queued `Finished`, or required a rapid UI tone to survive legitimate same-slot replacement, remain
failed observations; no engine or source rule was changed to satisfy them.

**Accepted modern approximation:** source slots are inexpensive replacement groups, not hardware
output masks. Partial overlap can retain a source voice that the original would replace. Exact
channel masking, chip coupling, sample-perfect mixing and live timer changes within a PCM clip are
outside the current audio goal and do not block the playable milestone. The proposed #554 stem/core
investigation was abandoned as unnecessary under this policy, not delivered.

`SF2_BATTLE_SCENE_PARTIAL_AUDIO=1` observes ordinary Herb-to-menu overlap, actual completion and
unchanged gameplay during tail collection. Independent adapter observations cover partial overlap,
rapid bounded replacement, full/disjoint groups and disposal.

**Incomplete:** Original command 253 fade semantics and complete reached field/UI cue bindings remain
incomplete. The [physical scene consumer](#physical-battle-scenes) implements a bounded battle-action
music/SFX and input-release subset; HEAL spell/fairy consumption remains unfinished. The existing modern
half-second `SoundFade` service is not source-timing evidence. Neither successful resource
requests, counters nor a build proves actual audible consumption or continuous 7C/8D/H4 acceptance.
JOIN still lacks the source logical audio-end/interleaving binding required in addition to actual
PCM completion by accepted Option A. Field-Wait eligibility does not supply it; PCM duration cannot
be converted into logical work or RNG opportunities. The instant/reveal-all speech difference above
still requires its own 10A disposition and same-semantic-Wait/acknowledgement comparison.

## Physical Battle Scenes

The common session separates physical action construction from scene execution. Domain prepares
ordered first/second/counter reactions and the action award. Application owns each continuation and
wait token: initialization, action text, animation, HP reaction, result/death text, EXP credit and
text, growth and its notices, gold text, then scene end. HP changes at reaction entry; EXP credit
precedes its text and growth follows acknowledgement. Kill/defeat accounting, after-turn, outcome
programs and the next actor wait for scene end. Wrong, stale or unrelated input cannot release a
wait. AI passes its prepared action to this same consumer without selecting or rolling twice.

**Confirmed, static source and engine boundary:** the pinned US baseline and SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6` support construction/replay separation in
`disasm/code/gameflow/battle/battleactions/battleactionsengine_1.asm:WriteBattlesceneScript`,
`battleactionsengine_2.asm`, `attack.asm` and `battlescenes/battlesceneengine_0.asm` beneath that battle
source directory. The admitted physical damaging reaction consumes twelve pairs of shared-main RNG
draws (range5 for the ally and range7 for the enemy). Source loops `loc_18E6E`/`loc_19004` follow the
reaction-mode and `d1 == 0x8000` guards; this direct-draw rule is not every hit or scene's total RNG.
Growth consumes the carried main image after
those reactions; an old scalar comparison that omitted scene draws is not final-session parity.
`BattleSceneTests` checks phase boundaries, ordered followups, continuous reaction seeds, growth,
deferred accounting and input release; migrated action tests retain their construction assertions.

The private source content candidate contains the selected SDMN/PRST/KNTE and GIZMO frames,
Wooden Sword/Rod and Wooden Stick, Tower Interior background9 and ground9. The generator composes
source hardware tile chunks and background layout, preserves source frame timing/weapon flags,
and writes the existing pack-v1 master/2x/4x structure plus `battle-scenes.json`. Raw palette
provenance remains intact; displayed base CRAM words use mask `0x0EEE`, including the retail low
bits in words `0x0DB0` and `0x0E50`. Canonical inputs remain read-only and generation only accepts a
new ignored worktree-local output. This is a review candidate, not asset-library promotion.

After loading the [private input/tool selections](../../docs/operations/local-private-inputs.md),
use absolute selected ROM/upstream paths and a fresh ignored destination:

```powershell
uv run python -m sf2tool.remake_battle_scene_content --rom $selectedRom --upstream $pinnedUpstream --output $candidateOutput
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $candidateOutput 'battle-scenes.json'
$env:SF2_BATTLE_SCENE_OBSERVATION_OUTPUT = Join-Path $observationOutput 'observation.json'
& $env:GODOT_BIN --path remake/game --audio-driver WASAPI --script res://probes/engine_battle_scene_observation.gd -- --private-battle-start $env:SF2_PRIVATE_CONTROLLED_START
```

Build the actual adapter first using the [locked SDK workflow](./development-and-verification.md#locked-net-workflow).
The isolated scene consumer/generator uses `uv run sf2 verify plan --scope engine --base origin/main --head HEAD`
on a clean committed head. Its exact generator path selects engine behavior, adapter compilation and
the existing direct public checks; research inputs still reject that explicit scope. This classification
does not extend to other generators or change research verification requirements.
The normal host also consumes this content through `--private-exploration-start`; source private
physical scenes fail explicitly when content is missing. The external observer reads actual node
resources, frame/position state, session effects and audio receipts, and submits ordinary keys.
It accepts `SF2_BATTLE_SCENE_WORLD=1` for the controlled Map40 navigation/warp route with an admitted
source world. `SF2_BATTLE_SCENE_REWARD=1` additionally exercises the adjacent player attack and
requires an explicitly prepared party with Bowie HP40, attack30, defense20 and EXP99. These are
controlled consumer observations, not original natural-entry fixtures. World, party and output
paths remain private. No screenshots or runtime state setters are used.

**Confirmed, bounded host observation:** source actor/weapon/environment textures, the 22-iteration
entrance projection, message waits, enemy and player hits, death/EXP/growth/gold, music2/5, SFX81/83/102,
253 requests and resumed battlefield music execute through the running Godot/WASAPI consumers.
Normal/reduced-flash runs preserve the same reaction draws, final seeds, resources and cursor while
the setting suppresses recoil/flashing. Battle text clears speech bleeps, matching source bsc10/11.
The host uses the existing modern half-second fade and instant/adjustable text settings. Hardware
timing, exact final pixels, continuous opening/outcome acceptance and original audio mixing remain
**Unknown**; these observations grant no H4 or deviation waiver.

### Medical Herb scenes

The admitted item family uses the same typed scene continuation. Inventory compaction and its two
range16 award draws occur during construction; HP/MP remain unchanged until the recovery command.
Recovery applies HP once without spending MP. The actor earns EXP after recovery text and restoration
of the actor's displayed side, then growth follows the EXP acknowledgement. Missing growth rejects
the whole preparation before inventory, movement or RNG can publish. Self-use, another living ally,
full HP and PRST/non-PRST awards share this path. Application supplies the displayed ally and absent
enemy, including outgoing/incoming target and actor phases; Godot never represents an ally as an enemy.

**Confirmed, static caller selection:** at pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, the following chain beneath `disasm/` distinguishes a
Medical Herb from casting the HEAL spell. This consumes the existing
[battle-scene](../../docs/design/contracts/battle-scene-presentation.md) and
[action-construction](../../docs/design/contracts/battle-action-construction.md) contracts; it does
not extend their natural timing claims.

| Source / symbol | Selected behavior |
| --- | --- |
| `data/stats/items/itemdefs.asm`; `code/common/stats/itemstats.asm:GetEquipmentType` | Medical Herb is non-equipment; equipment type is zero. |
| `code/gameflow/battle/battleactions/getspellanimation.asm:battlesceneScript_GetSpellanimation` | Non-equipment skips the use-spell animation field, retaining `SPELLANIMATION_NONE=0`. HEALIN's effect calculation does not imply its fairy animation. |
| `battleactions/createbattlesceneanimation.asm`; `battlescenes/getallyanimation.asm`; `battlescenes/battlesceneengine_0.asm:sub_184B0` (under `code/gameflow/battle/`) | USE_ITEM type2 selects the ordinary class sequence and preserves the caller's selector, rather than substituting its header default. Candidate headers remain raw; item selector zero belongs to the action. |
| `battlescenes/battlesceneengine_2.asm:SetupSpellanimation`; `battlescenes/animation/nothing.asm` | Selector zero dispatches to Nothing, which returns without fairy setup/update. |
| `battleactions/battleactionsengine_1.asm:InitializeActors`; `animateaction.asm:SwitchTargets`; `battleactionsengine_2.asm:battlesceneScript_End` | Begin with the actor alone; switch to another ally before recovery and back to the actor before EXP. Self-use needs neither switch. |
| `battleactions/castspell.asm:spellEffect_Heal`; `battlescenes/battlesceneengine_0.asm:bsc0B_executeAllyReaction` | Mode2 applies recovery then sends SFX113 and returns; no damaging-reaction recoil loop. Message298 follows recovery; item message275 names the consumed item. |

Recheck any named source directly using the selected read-only upstream root, for example:

```powershell
& git -C $pinnedUpstream show 'c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/gameflow/battle/battleactions/getspellanimation.asm'
& git -C $pinnedUpstream show 'c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/gameflow/battle/battlescenes/animation/nothing.asm'
```

The source starting Chester loadout is `WOODEN_STICK|EQUIPPED` in `allystartdefs.asm`:
word184 masked by127 gives item56. `getweaponspriteandpalette.asm:table_WeaponGraphics` at ROM0x1F9E2
selects sprite10/palette19 for item56. The class KNTE sequence is index2, including item type2;
weapon sprite10 does not select the spear-throw specialization. The candidate admits this actual
starting binding. Runtime checks the live equipped item against the candidate and rejects missing
weapon support; it does not assign a class-wide weapon or silently replace equipment. An older
Short Spear candidate does not establish starting Chester presentation. Preserved physical observations
only prove the actor/loadout combinations they actually reached.

**Confirmed, bounded host observations:** ordinary keys consume source item frames/text, one-sided
status/resources, target/actor switches, SFX113, EXP/growth and return to battlefield control. The
Map40 source-world startup heals the party before battle, so that observation covers full-HP use;
a separate ordinary-input battle run first obtains actual enemy damage, then covers wounded
self/other-ally recovery without claiming world audio.
Normal/reduced-flash source-world runs agree on gameplay, inventory and bounded RNG records. A narrow
Chester physical observation consumes the corrected live Wooden Stick resource and sequence2.

The switch projection uses `bscWait $1E`, `bsc07_switchAllies` coordinates and
`SwitchAllyBattlesprite`'s capped16 steps through the existing presentation timing mechanism.
DMA/VInt/input/audio alignment remains **Unknown**. Neither those durations nor recovery sound
playback add invented shared-RNG opportunities. No fairy or24 recoil draws means only those two
branches are absent, not that the whole scene has zero RNG. HEAL/fairy and the pending waiting-policy
decision remain separate; no original timing/pixel/H4 acceptance or asset-library promotion follows.

For the affected engine behavior, select `MedicalHerbTests|BattleSceneTests` with the locked SDK's
`dotnet test --filter` (fully qualified names), then build the adapter. The same committed engine-scope
planner applies. The existing native observer accepts `SF2_BATTLE_SCENE_HERB=1`; it uses Chester
self-use and Sarah-to-Bowie only as controlled observation inputs, not gameplay legality rules.
Select the normal source battle inputs and the fresh candidate as above. For the source-world/audio
pair, copy the tracked opening party to a fresh ignored JSON, retain
all inventory words and set Sarah EXP99. Set `SF2_PRIVATE_CONTROLLED_START` to that absolute path and
`SF2_PRIVATE_EXPLORATION_CONTENT` to the existing world without copying it, and set
`SF2_BATTLE_SCENE_WORLD=1`. Use `--private-exploration-start $explorationStart` with the controlled
Map40 boundary below, plus a fresh output path for each run. The world program's healing is retained.

```json
{"formatVersion":1,"controlledBoundary":"Controlled Map40 scene-consumer observation; not natural entry","start":{"map":"map-40","player":"entity-0","position":{"x":4,"y":30},"facing":1,"speed":32,"flags":[0,1,2,32,33,34,401,451]}}
```

Pass `--input-settings $settingsPath` containing `{"formatVersion":1,"reducedFlash":true}` for the
paired accessibility run. `SF2_BATTLE_SCENE_CHESTER=1` instead selects the narrow physical binding
observation with ordinary Tower entrance movement; its controlled party retains the starting
inventory and gives Chester HP/maxHP40 and defense20. For positive recovery, set `SF2_BATTLE_SCENE_HERB=1` and
`SF2_BATTLE_SCENE_WOUNDED=1` with the same controlled Chester HP/maxHP40/defense20 party,
Sarah EXP0 and `--private-battle-start $partyPath`. The observer walks Chester into actual enemy
attacks, brings Sarah alongside using ordinary movement, then uses Sarah's herbs on Chester and herself.
Each action must begin below live maxHP and increase HP. Supplying a low starting HP alone does not
establish injury: the observed battle initialization restores HP. Chained enemy scenes are identified
by their new initialization token and the preceding scene-ended event, without requiring a hidden
frame between scenes. These modes use the same fresh-output/error
contract and no screenshots, original emulator, runtime state setters or fixed comparison seeds.

**Unfinished healing work:** HEAL spells still use the scalar command path; no healing scene or
fairy RNG is consumed, and #523 remains open. The research owner's
[retained HEAL consumer evidence](../../docs/research/map3-messenger-acceptance.md#retained-heal-consumer-evidence)
binds original self/other-target recovery, actual setup/update RNG callers, saved settings02/00
and the reached timed battle-text branch. The source audit binds scene VInt clear/install/removal
and graphics-before-windows order. Those are available premises, not new runtime-acquisition gaps.

The consumer must preserve MP-before-cast, setup enable after its flash/load/draws, live fairy
updates across target switching/recovery text, the ordinary make-idle wait, and stop-drain before
return-to-actor/reward. The reached `bsc10` timed input loop is distinct from W1/W2: neither text
reveal nor every acknowledgement warrants a range256 draw. `bsc0D` requests stop and waits for the
active toggle; `bsc0C` force-cleans. The dispatcher decrements the lifetime word before updating;
initial `FFFF` is not an immutable sentinel. X-boundary ranges28/32 and periodic dust range12 can
occur in the same update. Last draw, construction-time `Script_End`, animation completion and
runtime spell termination are different boundaries.

**Unknown:** the remaining natural admission of individual logical opportunities, no-draw fairy
states/live gates, actual timed-input reads and stop/cleanup instants. Exact speed2 across every
command is still **Inferred**, as the research owner specifies. The accepted
[Option A contract](../../docs/decisions/0010-map3-battle01-product-acceptance.md) separates mandatory
logical work, explicit gameplay Wait and host delivery; a settled field-Wait consumer does not
establish these battle opportunities. Implement from the named source state/caller rules without
renderer-frame RNG, fixed fairy duration/draw totals or seed padding. This evidence does not make
HEAL playable or close 9A/H4; a partial kernel alone cannot do so.

## Public Synthetic and Test Fixtures

The `public-synthetic` smoke/export profile was retired at M5. Project-authored fixtures may still
exercise dimensions, animation counts, missing assets, bucket selection, cache drift, safe areas, and
cue-to-resource mapping. Fixtures and synthetic presentation are never selected as product content,
and an explicit local product request cannot fall back to them while reporting success.

## Godot Ownership and Migration

The current `Map3Presenter`, `SyntheticMapViewport`, `PublicSyntheticBattlePresenter`, and private
presenters remain thin projections of typed session state. The migration now has one Godot-only
presentation asset catalog; no shared Theme owner exists yet. It does not create one class per panel,
icon, texture family, or scale bucket.

Migration order is intentionally incremental:

1. accept this architecture and close the local manifest, preflight, and HUD SVG candidate-generator
   toolchain contract;
2. accept one actual asset-repository HUD SVG, review its candidate 2x/4x derivatives, and promote the
   exact master/runtime/manifest transaction before any catalog consumption;
3. mount the first reviewed frame through one path-free Godot catalog and one Content-owned semantic
   payload lookup, retain its bounded chrome fallback, and let the project-authored private battle
   entry choice own its first semantic panel migration;
4. append one reviewed 58-by-58 tactical cursor transaction and mount it only as an additive
   `HasCursor` projection without moving tactical state into Godot;
5. build and review one exact local Map 3 base-atlas transaction, mount that bounded world family,
   validate 2x/4x bucket switching, and retain pixel equivalence with the existing typed projection;
6. derive the complete three-sheet controlled-player locomotion family from the accepted
   facing/half/cadence owner, review and promote it as a separate local asset transaction, then bind
   it only after Application owns the runtime phase; do not relabel the current reference frame as
   standing, idle, or a universal admission frame;
7. retain the accepted entity-142 UP source as one reviewed two-half reference sheet without choosing
   a half or adding a runtime consumer until its presentation-time behavior is separately closed;
8. expand asset families only after their source, derivation, cache, and failure rules are closed;
9. close music and sound-effect format/loop/streaming contracts separately.

Godot owns resource loading, Theme application, viewport projection, and disposable scene nodes.
Application owns semantic presentation/audio cues. Domain owns game rules. No asset migration may move
gameplay authority into a scene, resource, filename, or animation callback.

Historical (retired at M5): the shared public-synthetic/private-bridge tactical panel kept its fixed 960-by-540 layout.
Its project-authored title is an exact two-line label with automatic wrapping disabled, so the Godot
fallback font cannot turn the title into a clipped third line. This is a bounded readability fix, not
an admitted product font, Theme, original battle title, or presentation-fidelity claim.

## Fixed, Deferred, and Unknown

| Status | Boundary |
| --- | --- |
| fixed by this proposal | 960-by-540 logical grid; simulation/presentation separation; explicit asset-repository root/exported pack; local-Git source/master/runtime/manifest history; ignored reproducible cache and scratch; fail-closed mount; no synthetic product fallback |
| fixed after acceptance | 4x new-raster authoring; original raster as local master; deterministic 2x/4x buckets; one resident bucket; safe-frame/aspect/accessibility model; thin Godot catalog migration |
| implemented product display policy | PrivateLocal adaptive windowed startup up to a fitting 1920-by-1080 client; runtime 960-by-540 minimum; explicit physical target preservation; HiDPI without double counting; centered `keep` frame; one startup-resident 2x/4x bucket; restart required for bucket reselection |
| implemented tooling prerequisite | exact product manifest path; pinned resvg 0.47.0 Windows archive/version; closed static HUD SVG subset; deterministic ignored-cache 2x/4x candidate build with path-free receipt and no tracked mutation |
| implemented world-family tooling prerequisite | three fixed families: Map 3, shared Map 19/20 and Map 21; fixed private ROM/metadata roots, exact palette/slot selections and each selected map's ROM-header join; five-segment 128-by-320 atlas and deterministic nearest 2x/4x ignored candidate; candidate publication still requires separate local review and asset acceptance before consumer binding; no implicit promotion or update |
| implemented player-reference consumer | fixed private ROM root; exact controlled player, regular map-sprite, DOWN source-slot, no-mirror, and half-zero selection; bounded Basic decode; reviewed 24-by-24 `initial-reference-frame` master and nearest 2x/4x local transaction; exact Content mount and thin Godot logical-cell projection; no standing/idle, animation, or admission-visible claim |
| implemented entity-reference tooling prerequisite and bounded consumer | fixed private ROM root plus accepted entity-142 fixture; exact record/slot/map-sprite/UP source; both decoded halves retained in source order as one reviewed 48-by-24 local sheet and nearest 2x/4x buckets; exact Content mount, Godot-only half-zero fresh bind, and `project-authored-two-half-diagnostic-cadence-v1` viewport state holding each half for 30 fixed physics callbacks; no original selected-half/idle/counter/cadence/visibility/interaction semantics, lifecycle, all-entity renderer, or fidelity claim |
| implemented bounded consumer | reviewed frame/cursor/base-atlas master/runtime/manifest transactions; explicit independent HUD, base-view-plus-atlas, and static-overlay-diagnostic opt-ins; exact semantic lookups; 2x/4x selection; Content-owned contained payload recheck; chrome-only fallback; typed ENTER/STAY and cursor overlays; full selected base-atlas physical raster mapped through the authoritative project-authored logical crop after exact-nearest scale-block validation; no playable ROM/metadata reopen, source/master runtime input, PCK, or fidelity claim |
| implemented modern camera consumer | Godot-only immutable focus projection from authoritative private snapshot plus Application locomotion offset; two logical pixels per accepted transition; column-6/row-3 centering with 64-by-64 map clamp; bounded sub-tile sampling shared by offline payload tests and the playable local atlas; static overlay remains fixed; no original camera/plane/parallax/autoscroll/VInt/final-pixel claim |
| separate implementation decision | live resize bucket remount and fullscreen/monitor transition UX; platform safe-area integration; original or generalized Yes/No behavior; admitted product font/theme/input glyphs; general window chrome/Theme migration; user-selectable UI scale beyond the current 100% limiting-frame calculation; tracked-master rebuild/update transactions; cache retention/review lifecycle beyond one fresh candidate |
| Unknown | original background-layer palette source; camera/layer/priority-with-sprites/animation composition; final-pixel fidelity; natural route and timing; complete UI/text behavior; audio format/loop/streaming; H4 and 8C parity |

The local asset repository and bounded semantic consumers now exist, but this document still does
not authorize a product batch, PCK inclusion, presenter-wide migration, original Yes/No/cursor
interaction, admitted product font/input glyphs, or a general Theme. Those changes require separately
owned, reviewable implementation slices.

## Current runtime castle base view

Historical: this section describes the legacy reference castle views retired at M5.

The required visual selection belongs to each immutable runtime. Application's exact catalog
admission checks Maps 3/19/20/21/40 independently, including custom import-source ports. The base
renderer uses that current selection, the snapshot's working layout, and the current runtime's blocks
and areas. Map 3 uses its own atlas; Maps 19/20 share palette 0 and slots `[6,23,44,53,62]`, but
switching between them still changes the selection's map and the working layout. Missing or mismatched
selections are rejected. The existing camera and player locomotion policies apply across this seam.
These retained reference consumers display immutable snapshots for geometry and asset comparisons.
The common host executes castle/tower transitions and renders its live working layout, player and
source sprites through the [program owner](./exploration-programs.md#castle-palace-astral-and-tower).

Map 3 static/current-area overlays, entity-142 diagnostic, Sarah and Zone601 glyphs are scoped to Map 3.
Retained reference castle views use authored glyphs from their explicit snapshot context; their
former F-key story handlers are removed. The ordinary host uses admitted private source NPC sprites
and actual program state. Traversal-only reference launches retain their geometry diagnostic.
Original hardware rendering/timing, natural caller state and H4 remain outside these comparisons.

Map 21 arrival has palette 0 and slots `[6,23,44,53,8]`, which differ from the shared castle family.
The explicit base-atlas launch binds `world.map21.base-tileset-atlas` from the accepted full pack,
using the exact 2x/4x digests and current runtime's working layout, blocks and area. Camera focus,
crop and player drawing keep the existing destination/locomotion policy. Missing or changed assets
fail closed; the shared castle atlas cannot substitute for the Map 21 selection. Traversal-only
launches retain their diagnostic grid, with status placed below whichever viewport is active.
Reference status directs the migrated guard interaction to the common host. In that host, ordinary
interaction moves guard128 from `(5,16)` to `(6,16)` and source135 resolves through the fresh identity
table to player slot0. Facing down mounts the corresponding player sprite before F401 and caller
F256 publish. Repeat dialogue and ordinary movement through `(5,15)` use the same live session.
Unknown or mismatched visual selections still reject. Original natural timing and hardware sprite
effects remain unclaimed; Map3 reference diagnostics remain scoped to Map3.

Map 40 has its own accepted atlas in the twelve-asset/24-bucket full pack. Its exact binding is
`world.map40.base-tileset-atlas`, palette 3 and slots `[94,95,96,97,58]`; palette 3 is independent of
tile palette-bank bits. The 2x digest is
`40A083FD4AEBF2A13EB93037FDA8D3438AD2B97899560D8077EBDCF2C96B5BD3`, and the 4x digest is
`636A371D7481D63FE845BC2E6C1AB7A2BE99361448626E8553AC2E312110DC59`.
Explicit base-atlas startup requires all four map-family mounts and rejects a missing/changed Map 40
payload or selection. The presenter switches on the current exact runtime; no Map 21/castle fallback
is admitted. Controlled arrival at `(4,30)`/UP/raw 1 uses the existing 64-by-64 camera bounds and crop
`(0,27)`, hides traversal and all retained route glyphs, and keeps status below the base viewport.
Traversal-only startup still shows the diagnostic grid. Both views state that init is unexecuted.
Map 40's area offset `(0,32)`, parallax 128 and main-layer type 255 remain data; this consumer adds
no second-layer execution, player-art policy or NPC extraction. The separate controlled Battle 01
pending result retains this Map 40 base view: it names destination Map 57 and states that battle has
not started. It does not mount Map 57 graphics or relocate the player. Restarting the private launch
creates a fresh Map 3 session.

### Reproduce the bounded native image review

Historical anchor retained for existing links. Screenshot generation, image comparison and visual
frame inspection are prohibited by the [current verification scope](./development-and-verification.md#scope).
The former source-copy/editor-launch/image recipe is available through Git at accepted base
`9fb9727e260416a54aeb3421454e610e00884ed5`; it is not a current command recipe. Completed local images,
logs and process receipts remain evidence of those bounded historical runs.

Use the existing installed Godot and owned ordinary project for current observations. The old
castle atlas source-copy/image probe is removed with its reference execution callers. The
[common-session observer](../game/probes/engine_castle_tower_observation.gd) drives actual input from
the live opening and records session, map, presentation and error state without screenshots.
Its [execution owner](./exploration-programs.md) keeps the source and presentation limits explicit.

The [Battle01 admission observer](../game/probes/engine_battle01_admission_observation.gd) extends that
same live instance through the full before-battle scene and first battle input. Map40 uses its actual
empty source setup plus enabled followers. Scene reload replaces physical records and mounts the
private mist-demon/gizmo/Astral sprites. White fades, coarse-to-fine mosaic and three-cycle shiver
are explicit modern presentation services; their pixels and duration do not claim original hardware
parity. The existing battle board mounts under the fading view while input stays closed, then returns
to the ordinary host after load/start. Use fixed60/30 FPS and inspect state/draw counters and logs;
no screenshots or additional editor/environment installation are needed.

## Diagnostic Battle01 launch and native review

Historical: the private-local arguments and Map40/Map57 diagnostic launch below belonged to the reference
host retired at M5. Current Battle01 play uses `--private-exploration-start` or `--private-battle-start`.

Apply the current [runtime-state acceptance and instance reuse rules](./development-and-verification.md#scope)
before using any recipe below. Earlier image instructions describe retained historical workflows;
do not generate, compare or inspect screenshots for current acceptance. Reuse completed state/input
records when sufficient. A future necessary probe run must disable existing image output in the
owning test, preserve state/input checks and reuse the owned Godot instance unless a named exception applies.

An ordinary private launch can additionally select these three inputs together, alongside its
existing canonical import and optional reviewed exploration atlas options:

```powershell
# Each variable names an explicitly selected, existing fully qualified private input.
$battleArgs = @(
    "--private-battle01-data=$env:SF2_PRIVATE_BATTLE01_DATA"
    "--private-battle01-scene=$env:SF2_PRIVATE_BATTLE01_SCENE"
    "--private-battle01-terrain=$env:SF2_PRIVATE_BATTLE01_TERRAIN"
)
```

Append these arguments after Godot's `--` separator to the private-local user arguments.
The [selected-input owner](./development-and-verification.md#selected-battle01-startup-inputs)
routes their fixed identities and any actually needed reference check. There is no default lookup, fallback
fixture, copied canonical ROM, or new asset transaction. Missing/malformed input diagnostics display
the admission stage without displaying selected paths. Syntax/profile rejection makes startup
unavailable; file or payload rejection at N retains the current Pending.

At the actual Map40 Pending, N starts the controlled Prepare/Initialize/FirstRound/FirstControl chain.
I/J/K/L move its cursor, Space provisionally confirms movement and Backspace returns to the turn
origin. With `--private-map3-base-atlas`, selected Battle01 inputs also require the reviewed Map57
atlas at startup. Its asset/source/derivation policy is fixed by the full manifest above; the existing
catalog independently requires the exact 2x/4x PNG identities. Rehashing a replacement manifest or
payload cannot grant it the accepted identity. Map57 uses source
`source.world.map57.base-visual-selection` and policy
`private-local-map57-base-nearest-rgba8-authored-empty-slots-v1`; its runtime digests are
`3278D7D189D4E1717D212DD59BFBC94779FFDBDFFE171002679A0F3E929A69A3` (2x) and
`FF8C6AD736752A5BEDCE5549EB5D0FCE3C69F7FC3B7A9D3FFECF0D3394F79AA7` (4x).

The current battle's fixed admission supplies the Map57 layout, block catalog, area and selection.
The projection checks that exact definition and scans the whole layout's referenced blocks for
unloaded slots. It preserves the unused seed blocks without drawing them. Shared block sampling
handles index masking/offset, both flips, transparent index zero and the existing background fill.
The 16-by-20 area uses real 24-pixel blocks: a 768-by-960 or 1536-by-1920 physical raster maps to
384-by-480 logical pixels with nearest filtering, without reducing the blocks to the former 20-pixel
diagnostic cells. The map sits left of the status/control column on the existing 960-by-540 canvas.

The heading says `MAP 57 BASE ART + DIAGNOSTIC UNITS`, with reviewed fixed-art/controlled-input
disclosure. All nine live unit markers, reachable/legal-stop cells, actor/cursor/path and costs use
the same 24-pixel grid. Cursor terrain remains visible in the details. Units are authored markers;
Map3 character sheets are not reused as the Battle01 roster. Without a base-art request, the explicit
diagnostic mode retains terrain-ID cells and its graphics-unavailable heading. Missing/wrong requested
Map57 art rejects before startup; a projection rejection displays an unavailable view and closes input.
Action choice offers a separate Space press for controlled no-effect STAY or Backspace cancellation.
STAY retains the selected live position and tries actual next entry once. Player2 receives its own
Centaur range from current occupancy, then can move/cancel/STAY. Its completion at raw offset4
starts one finite relay over the actual six inactive enemies. Each move/STAY uses current occupancy,
chained seed-copy and its own memory, then commits independently. A later failure stops at the last
completed actor with its precise diagnostic. Success reaches actual Bowie0 at offset16 with the
Regular1 budget12 range. His manual move/cancel/STAY reaches the first sentinel at offset18;
current-position activation, empty cutscene/spawn admission and current-main ordering then generate
the next admitted round. The bounded dispatch yields its actual player without choosing a move.
Every completion retains its round number and prior history. Unsupported activation rejects before
installing flags/order/RNG; actor failure retains its last successful state. The view removes stale
movement overlays at rejection, and unrelated keys preserve that reason without retrying.
Its effective-stat policy is explicit in the
[owning plan](./map03-playability-plan.md#implemented-first-player-stay-completion). The complete old
Map40/HUD/synthetic canvas subtrees are hidden; relaunch starts Map3.

Current verification selection belongs to the
[development guide](./development-and-verification.md#scope). The former native
mode-by-mode capture lists and source-copy recipes are retained in Git at
`9fb9727e260416a54aeb3421454e610e00884ed5`, not as instructions to generate more images or replay all
earlier routes. The [Map 3 record](./map03-playability-plan.md) retains the selected reference inputs,
controlled endpoint facts and Unsupported boundaries.

A needed adapter observation should identify the exact input and expected state/projection before
running it, inspect actual errors and bounded cleanup, and distinguish physical Godot input from a
direct API call or a test-seeded state. Use a relevant independent reference expectation where the
claim needs it. Do not add tests of the probe, fixture driver, reports or launch wrapper.

Node/input observations cannot establish original pixel, palette, audio, hardware or timing fidelity.
Those accepted 8C/H4 gaps remain open. Private assets and completed captures remain local and grant no
distribution permission. This documentation change does not implement a new observer, alter the
current game/probe, or authorize engine migration.
