# Presentation and Local Asset Architecture

- Status: **Proposed**
- Scope: modern high-DPI presentation and local product-asset boundary for the Godot remake
- Applies to: the existing `public-synthetic` verification profile and the future product-asset
  expansion of the explicit `private-local` profile

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

The currently implemented `private-local` profile remains the bounded traversal/base-view capability
described by [Capability Status](./capability-status.md). It may mount the reviewed local HUD frame and
tactical cursor only through an explicit request. The frame remains a chrome-only diagnostic without
the project-authored battle bridge; with that bridge, it projects one typed pending ENTER/STAY choice
and one additive Active-state cursor ring driven only by typed `HasCursor`. Those narrow consumers do
not reclassify the profile as complete product presentation.

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
6. Keep the tracked `public-synthetic` content as a test and smoke profile. It is not the product art,
   music, or presentation source.

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

Exact container/codec normalization, loop points, streaming versus resident policy, channel layout,
gain normalization, transition timing, and audio cache receipts remain **Unknown** until a separate
audio contract closes them. This document authorizes neither guessed loop metadata nor a new audio
implementation slice.

## Public Synthetic and Test Fixtures

The tracked `public-synthetic` profile remains a redistribution-safe architecture, command, snapshot,
smoke, and export verifier. Project-authored fixtures may exercise dimensions, animation counts,
missing assets, bucket selection, cache drift, safe areas, and cue-to-resource mapping.

Fixtures and synthetic presentation are never selected as product content. Tests must prove that an
explicit local product request cannot fall back to them while reporting success. Existing stable smoke
receipts remain unchanged until a separately accepted compatibility migration.

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

The shared public-synthetic/private-bridge tactical panel keeps its existing fixed 960-by-540 layout.
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

The required visual selection belongs to each immutable runtime. Application's exact catalog
admission checks Maps 3/19/20/21/40 independently, including custom import-source ports. The base
renderer uses that current selection, the snapshot's working layout, and the current runtime's blocks
and areas. Map 3 uses its own atlas; Maps 19/20 share palette 0 and slots `[6,23,44,53,62]`, but
switching between them still changes the selection's map and the working layout. Missing or mismatched
selections are rejected. The existing camera and player locomotion policies apply across this seam.
For the instantaneous `Relocated` phase, both camera focus and player drawing use the destination.
Application retains the old source position as relocation provenance; it is not an interpolated
destination-map position. Walking, stair movement, blocked attempts and the existing crop policy retain
their accepted behavior.

Map 3 static/current-area overlays, entity-142 diagnostic, Sarah and Zone601 glyphs are scoped to Map 3.
Castle views retain the existing authored purple Astral diamond while Application reports its route
tile occupied; accepting via F clears it. No original castle NPC sprite is admitted. Traversal-only
launches still use their existing diagnostic display. General init, original rendering and timing,
natural caller state and H4 remain outside this consumer.

Map 21 arrival has palette 0 and slots `[6,23,44,53,8]`, which differ from the shared castle family.
The explicit base-atlas launch binds `world.map21.base-tileset-atlas` from the accepted full pack,
using the exact 2x/4x digests and current runtime's working layout, blocks and area. Camera focus,
crop and player drawing keep the existing destination/locomotion policy. Missing or changed assets
fail closed; the shared castle atlas cannot substitute for the Map 21 selection. Traversal-only
launches retain their diagnostic grid, with status placed below whichever viewport is active.
The first status line names Map 21, the controlled arrival and unexecuted init until the bounded guard action
is available. At `(4,16)` facing RIGHT, it offers the explicit controlled F result; after F it identifies
the guard's completed move. The existing authored diamond follows guard occupancy from `(5,16)` to
`(6,16)` without a direction line or original-art claim. Its immutable source-facing byte is not drawn.
F preserves player position/facing by remake policy;
ordinary Right/Up supplies the later `(5,15)`/UP endpoint. The real composition adapter handles both
guard success and rejection. Neither source entity135's facing effect nor natural script timing is applied.
Unknown or mismatched runtime/visual selections still reject. Only the fixed Map 21 atlas is newly
consumed; Map 3 overlays and NPC diagnostics remain scoped to Map 3, and no NPC asset is added.

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

The [fixed probe](../tests/native/Map19Map20AtlasReviewProbe.cs) is test-owned instrumentation, excluded
from the production project. It seeds the same validated Map 19 state as the private canonical test,
then drives the accepted fixture through the 38-input royal route, controlled palace F, two-input
return, Astral approach/F, released west route, three Left inputs into Map 21, and the controlled guard
F followed by ordinary Right/Up, the 18-input north exit and the controlled Map 40 pending route. Twelve real Godot
viewport captures cover Map 3,
Map 19 entry, Map 20 royal, Astral before/after F, Map 20 west, the Map 21 base atlas, guard before/after
F, the actual walk endpoint, the Map 40 base arrival and Battle 01 pending admission. Map 3-to-19 is a seeded projection
seam. This does not establish a complete natural player route, original init execution or H4.

Use a clean committed head after the owning tests and official Godot gate. Mount the accepted asset checkout read-only after its checkout preflight, or export only
`manifests` and `runtime` from accepted commit `3bf31fa02c4ca9ee04e06be1efc97bcc2bac5880`
into a fresh ignored directory. Set `SF2_PRIVATE_CANONICAL_MAP_IMPORT` and `SF2_PRIVATE_PRESENTATION_ASSET_ROOT`
to the isolated accepted inputs. Set `SF2_CASTLE_REVIEW_EDITOR` to the official 4.7.2 Mono editor
extracted inside this worktree by the Godot gate, and `SF2_CASTLE_REVIEW_ROOT` to a **fresh** absolute
child of this checkout's ignored `local/`. Run the following with `uv run python` from the checkout
root (a local script or a PowerShell single-quoted here-string piped to `uv run python -X utf8 -`).

The recipe copies production sources verbatim from the recorded Git head. Its only added compiled
source is the tracked probe; scene selection, seed/reflection, deterministic route driving, disabled
unsolicited callbacks and capture are instrumentation. The native process uses the actual Windows
renderer with a hidden startup window; a headless/dummy renderer cannot substitute for these images.
For required local tests, set `SF2_REQUIRE_PRIVATE_TESTS=1` with the canonical and runtime-asset
input variables above. Point both `TEMP` and `TMP` at this worktree's ignored test scratch:
the missing/changed-payload checks copy only manifests and runtime buckets there and never alter
the supplied asset input.

The process receipt records the code head, bounded exit/timeout/cleanup state and each step. Inspect
all twelve PNGs and the per-frame selection/area/layout/atlas/glyph receipt. Each Map 21 frame also samples
the rendered guard diamond's center and downward interior to reject a spurious facing line.
The first six retain the destination camera and player-rectangle checks. The seventh requires a
visible Map 21 base atlas and hidden traversal view, `(3,16)`/area 1, crop `(0,13)`, raw facing 0
and the exact record-4 source receipt. Later operations clear that receipt under the existing
per-operation contract. The status label must sit below the base viewport. Frames eight through ten
check the visible player and undirected guard marker, the real F adapter's prompt/completion text, and
the ordinary walk endpoint. Wrong-position and duplicate F calls use that same adapter and preserve
snapshot/locomotion/bridge without throwing. Frame eleven drives all 18 ordinary north-exit inputs
from (5,15)/UP to exact Map 40 (4,30)/UP, area 1. The accepted Map 40 runtime uses palette 3 and
slots [94,95,96,97,58] and its exact accepted base atlas. It crops at (0,27), draws the player at
column 4/row 3 with UP/raw 1, hides traversal and retained actors, and places controlled-arrival/
base-atlas/init-not-executed status below the base viewport at Y=310. This image is
`11-map40-base-atlas.png`; the first ten capture names and production route remain unchanged.
The probe mounts and decodes both accepted 2x/4x Map 40 PNGs, checks every projected RGBA component
for nearest equivalence, samples opaque base pixels from the actual renderer outside the player,
and checks camera/player coordinates and retained route receipts. Frame twelve then checks all 29
Map 40 fixture points: 27 committed moves reach (14,13)/UP, and the real production move adapter
receives the last Up as prospective (14,12). It captures `12-battle01-admission-pending.png` with
the Map 40 atlas retained, destination Map 57 (8,18)/UP, battle not started and restart recovery
below the base viewport. All three status lines, including the complete restart instruction, must
be visible without clipping. Duplicate movement, F, bridge request and physics tick leave source,
animation, bridge, pending identity and status unchanged. The accepted first eleven PNGs must
remain byte-identical. This does not admit Map 40 init, completed Map57 relocation, original
rendering, camera/parallax execution, battle startup or a controllable turn.
An image failure is a failed
native boundary even if startup markers pass. Images, inputs and process receipts remain private and
ignored; no pixels enter a public PR.

The native review exposed a concrete failure that identity-only checks missed: the Map 20 west arrival
reported `(6,37)` but used the retained Map 19 source `(5,3)` to crop Map 20 at `(0,0)`. This produced
the wrong black/roof region. Map 20 area 2 is `(0,33)..(41,45)` and has equal zero second-layer offsets;
that failure was not evidence of a missing layer or an init fix. The corrected stationary camera
must focus on `(6,37)` and crop at `(0,34)`; the royal arrival `(23,37)` crops at `(17,34)`.
The probe now checks actual focus/crop and the player rectangle, as well as map identities. Earlier
process-success receipts without those assertions do not establish correct destination images.

```python
from pathlib import Path
import json, os, shutil, subprocess, zipfile
from sf2tool import remake_godot as gate

repo = Path.cwd()
review = Path(os.environ["SF2_CASTLE_REVIEW_ROOT"]).resolve()
review.relative_to(repo / "local")
review.mkdir(parents=True, exist_ok=False)
head = subprocess.check_output(["git", "rev-parse", "HEAD"], text=True).strip()
subprocess.run(["git", "archive", "--format=zip", "--output", str(review / "source.zip"),
                head, "remake"], check=True, timeout=60)
workspace = review / "workspace"
with zipfile.ZipFile(review / "source.zip") as archive:
    archive.extractall(workspace)
game = workspace / "remake/game"
shutil.copyfile(workspace / "remake/tests/native/Map19Map20AtlasReviewProbe.cs",
                game / "src/Map19Map20AtlasReviewProbe.cs")
(game / "ReviewProbe.tscn").write_text(
    '[gd_scene load_steps=2 format=3]\n'
    '[ext_resource path="res://src/Map19Map20AtlasReviewProbe.cs" type="Script" id="1"]\n'
    '[node name="AtlasReview" type="Node2D"]\nscript = ExtResource("1")\n', encoding="utf-8")
captures = review / "captures"
captures.mkdir()
environment = gate._gate_environment(review)
environment["SF2_CASTLE_REVIEW_OUTPUT"] = str(captures)
environment["SF2_CASTLE_REVIEW_FIXTURE"] = str(repo / "tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json")
environment["SF2_MAP40_REVIEW_FIXTURE"] = str(repo / "tests/fixtures/h2/map3-battle01-admission-static-v1.json")
# Only window visibility changes; the maintained runner still owns its job/timeout/reap contract.
original_popen = subprocess.Popen
def hidden_popen(*args, **kwargs):
    startup = subprocess.STARTUPINFO()
    startup.dwFlags |= subprocess.STARTF_USESHOWWINDOW
    startup.wShowWindow = subprocess.SW_HIDE
    kwargs["startupinfo"] = startup
    return original_popen(*args, **kwargs)
gate.subprocess.Popen = hidden_popen
steps = [
    ("restore", ["dotnet", "restore", str(game / "Sf2.Remake.Godot.csproj"), "--locked-mode"]),
    ("build", ["dotnet", "build", str(game / "Sf2.Remake.Godot.csproj"), "--configuration", "Debug",
               "--no-restore", "-p:UseSharedCompilation=false", "--disable-build-servers"]),
    ("native", [os.environ["SF2_CASTLE_REVIEW_EDITOR"], "--path", str(game),
                "--resolution", "1920x1080", "res://ReviewProbe.tscn", "--",
                "--runtime-profile=private-local",
                "--canonical-map-import=" + os.environ["SF2_PRIVATE_CANONICAL_MAP_IMPORT"],
                "--private-map3-base-view", "--private-map3-base-atlas",
                "--presentation-asset-root=" + os.environ["SF2_PRIVATE_PRESENTATION_ASSET_ROOT"],
                "--presentation-asset-commit=3bf31fa02c4ca9ee04e06be1efc97bcc2bac5880",
                "--presentation-manifest-sha256=4F0F6BEFE809A3163704C6AAF4DC007A31B30D8DC8B58256DCE5AFF7BBAB0E40"]),
]
if environment.get("SF2_BATTLE01_CONTROL_REVIEW") in {"1", "missing-input", "base-art", "diagnostic", "missing-atlas", "stay", "next-player", "enemy-standby", "first-round", "round-continuation", "enemy-pursuit", "enemy-physical-attack"}:
    steps[-1][1].extend([
        "--private-battle01-data=" + environment["SF2_PRIVATE_BATTLE01_DATA"],
        "--private-battle01-scene=" + environment["SF2_PRIVATE_BATTLE01_SCENE"],
        "--private-battle01-terrain=" + environment["SF2_PRIVATE_BATTLE01_TERRAIN"],
    ])
if environment.get("SF2_BATTLE01_CONTROL_REVIEW") == "diagnostic":
    steps[-1][1][:] = [arg for arg in steps[-1][1] if arg not in {"--private-map3-base-view", "--private-map3-base-atlas"}
                       and not arg.startswith("--presentation-")]
receipts = []
for name, command in steps:
    result = gate.run_bounded_process(name, command, cwd=game, environment=environment,
                                      timeout=120, termination_timeout=15, reap_timeout=15)
    receipts.append(result.as_dict())
    (review / "process.json").write_text(json.dumps(
        {"codeHead": head, "productionCopy": "git archive; production sources unchanged",
         "instrumentation": "one copied tracked probe and ReviewProbe.tscn", "steps": receipts},
        indent=2), encoding="utf-8")
    print(name, result.passed, result.exit_code, result.timed_out, result.cleanup_status, flush=True)
    if not result.passed:
        raise SystemExit(1)
```

## Diagnostic Battle01 launch and native review

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
defines their fixed identities and required Content check. There is no default lookup, fallback
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

For this boundary, reuse the bounded recipe immediately above with
`SF2_BATTLE01_CONTROL_REVIEW=base-art` and the three selected-input environment variables set.
Choose a fresh ignored `SF2_CASTLE_REVIEW_ROOT` and the editor from the owning official toolchain gate.
This mode compiles the same tracked probe into the exact committed source copy, seeds controlled
Map40 entry through existing test-owned factories, and drives the 27 committed moves plus prospective
Pending move through `Input.ParseInputEvent` physical keys and real production polling. It then
injects N/I/Space/Backspace and illegal/old keys through the same path. It does not call Domain
battle transitions directly or replay the unrelated twelve-frame castle recipe.

Require six captures: Pending, ready, selected, provisional, cancelled and rejected. Inspect them
for text clipping, path/actor distinction and retained map/guard/overlay layers. The receipt checks
actual admitted layout and visible base texels outside overlays at 24-pixel cell coordinates,
snapshot/actor/live-position/occupancy changes, both cancel stages, immutable rejection,
unchanged RNG/order/current offset, all old canvas subtrees hidden, and closed exploration inputs.
The existing bounded process runner retains 120-second step limits, job termination/reap evidence
and private failure output. This is controlled seeded native UI evidence; natural Map3 continuity,
original Map57 scene/layer/VRAM/animation fidelity, timing, broader combat resolution, other AI commands, victory and H4 remain Unknown.

For repeated inactive-round acceptance, use a fresh review root and
`SF2_BATTLE01_CONTROL_REVIEW=round-continuation` with the same required canonical, three selected
inputs and accepted runtime/manifest pack. The recipe seeds controlled Map40, uses the existing
28-key route and N, and drives the first-round player decisions and inactive enemies through physical
key polling. Bowie's STAY reaches the sentinel and the same bounded dispatch enters round2 player2.
It captures only these four current boundaries:

1. `01-round2-ready`: actual2 at(7,17), new64-slot order, mainAA861234 and nine prior receipts.
2. `02-round2-provisional`: I/Space provisionally moves2 to(7,16), preserving round1 history.
3. `03-round2-cancelled`: Backspace restores only2 to its current origin(7,17) and entry occupancy.
4. `04-round3-ready`: each actual round2 player2/0/1 explicitly confirms origin and separately presses
   STAY; automatic inactive enemy dispatch and the next real sentinel yield round3 player2.

Require eighteen chronological receipts with generation1/2 tags, the complete round2/3 buffers and
mainAA861234/9BD71234, unchanged effective stats and original deployment anchors. The receipt includes
all twelve enemy decisions:1483 thinking bytes in round1 plus967 in round2, final copy0034 and
memory128..13304/04/04/24/34/04h. Compare every generated byte with the existing Python thinking helper
and the order, paths and results with the [independent reduction](./map03-playability-plan.md#implemented-repeatable-inactive-round-continuation).
Actual players remain manual. Inspect all four images for the current round and actor, range,
provisional path, independent cancellation, legible status/controls and hidden old canvas layers.

After captures, isolated copies exercise authored round2 enemy-first order, late generated-round
control rejection, late enemy occupancy rejection and successful reachable Bowie region1 activation.
These call the existing Domain/Application dispatch directly and are explicitly authored boundary
tests, separate from the physical-key comparison. Each rejected snapshot ignores all subsequent
battle inputs without retry; the probe restores the actual round3 snapshot afterwards.

The former `first-round`, `enemy-standby`, `next-player` and `stay` names alias this four-frame
continuation chain. It does not replay six-frame cancellation or castle modes; captures and bounded
process/code-head receipts remain local. It proves controlled remake behavior, not original after-turn
fidelity, natural seed chronology or H4.

For activated pursuit, select `SF2_BATTLE01_CONTROL_REVIEW=enemy-pursuit` and another fresh review
root with the same inputs. The shared physical Godot key-event chain reaches round2 Bowie, moves
him to(11,15), checks confirm/cancel, then separately confirms and commits STAY. All later players
explicitly choose origin/STAY. Capture these four frames:

1. `01-round3-ready`: actual player2, region1 active, main9BD71234/copy0034.
2. `02-first131-test-copy`: a labeled copied round3 snapshot uses direct Application steps to expose
   the synchronous first131 pursuit. Preliminary(9,5) is occupied; final(9,4) costs2 with move string
   `00 FF`. This is test instrumentation. Restore the exact original snapshot before continuing
   physical Godot key events and compare the resulting actual131 decision with this copy.
3. `03-round4-ready`: actual player1 after both active pursuits, main51DC1234/copy0234.
4. `04-round6-bowie-hp9`: the physical relay moves132 to(11,14), applies3 damage once and yields
   Bowie0 control at(11,15), raw12/receipt51, mainAF881234/copy0134. Visible HP/result and controls
   remain together; the independent pursuit API still classifies the cohort before the copied attack.

The receipt includes all six64-slot orders, typed standby/pursuit/physical decisions,4498 standby
thinking bytes plus57 physical-priority bytes, full main/copy images, last-target slot4 FF→0,
HP12→9 and original deployment provenance. Six pursuit turns consume no thinking bytes.
Inspect every image and its layout/atlas assertions; frame02 remains explicitly marked as a test
copy, with no production pause hook or original-timing claim. Frames and unmapped keys do not
repeat an attack. N remains a recognized initialization command and may show its retained-state
explanation while a player has control.

For the first physical attack, select `SF2_BATTLE01_CONTROL_REVIEW=enemy-physical-attack` with a
fresh review root and the same inputs. The shared route produces four different captures:

1. `01-round6-player1-ready`: the actual player before inactive128 and attack132.
2. `02-attack132-test-copy`: explicitly labeled copied round6 snapshot, direct Application
   player1 STAY/inactive128/attack132; HP snapshot12→temporary9→restore12→replay9, receipt51/raw12.
   Restore the exact physical-route snapshot before the next manual input.
3. `03-bowie-ready-hp9`: physical player1 STAY invokes the production relay and yields actual
   Bowie0; the complete attack decision equals the copied decision. Show hit3 and HP12→9 with controls.
4. `04-bowie-cancel-hp9`: physical L/Space/Backspace moves via the passable(12,15) tile and returns
   to(11,15), retaining9 HP, the attack receipt and occupancy.

The recipe retains its bounded restore/build/native process and source-archive provenance checks.
Both modes use existing accepted base art and diagnostic units. No imported combat animation,
original timing, natural RNG lifetime, general death/reward/victory/return or economy effect is claimed.

Use another fresh review root with `SF2_BATTLE01_CONTROL_REVIEW=diagnostic` for the minimal
unrequested-art regression. The recipe removes both base-view/base-atlas and asset options, captures Pending/ready,
then checks actual I/Space/Backspace restoration and retained RNG/offset without replaying all art frames.
For `missing-atlas`, give the recipe a separate ignored negative-test pack containing copied manifests
and runtime payloads from the accepted read-only pack, with only its Map57 runtime bucket absent.
Do not modify the canonical asset checkout. Keep the accepted commit/manifest pins and selected
startup inputs. The missing bucket fails the existing complete-pack admission first, so the probe
requires the visible `Unavailable: PrivateLocal presentation unavailable (PackageUnavailable).` diagnostic,
no session and no fallback, and writes
one capture. The required managed checks cover missing and changed payloads for both scales.

For the rejected-input boundary, use another fresh review root, set
`SF2_BATTLE01_CONTROL_REVIEW=missing-input`, and point `SF2_PRIVATE_BATTLE01_DATA` at an explicitly
absent absolute path inside that worktree's ignored local directory. Keep scene and terrain selections
valid. The same probe expects Pending plus a visible Prepare rejection, checks exact Pending/source
retention and absence of the selected path in displayed text, and writes two captures. Inspect that
the rejection and restart instruction both remain visible; this is an expected failure-path test,
not a replacement for the six-frame valid-input control review.
