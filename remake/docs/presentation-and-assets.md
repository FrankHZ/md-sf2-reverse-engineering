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

The current reviewed local pack at commit `f7a351f24e328c47b10a892613edeac07a07635a`, tree
`9cc4c0959ebbe067f22adae5c079a65bcfd1f06d`, and manifest SHA-256
`56382461FAA5168939A264FC37ABC8A7590A0D099C19DFB54DC0DC6F96F5DCB6` contains the two HUD assets,
the exact atlas family, and the player initial-reference-frame family. The atlas's explicit
PrivateLocal consumer mounts only one selected 2x or 4x runtime PNG after Content rechecks the closed
pack. Godot validates decoded RGBA8 dimensions and projects every physical bucket texel through the
already authoritative working-layout/block/tile/slot/flip selection. A 2x bucket becomes a
576-by-336 ImageTexture and a 4x bucket becomes a 1152-by-672 ImageTexture; `DrawTextureRect` maps
either onto the same 288-by-168 logical crop, and the current project-authored player marker remains
a later logical overlay. Horizontal and vertical flips reverse the complete physical tile axes,
including subpixel coordinates. Startup binding still proves that every scale-squared atlas sample
exactly repeats the typed 1x payload, so this transport closure does not admit a non-nearest
derivative. It never reads private source bundles or masters, does not embed the asset root in
Application/state/receipts, and does not place private data in a PCK. This is a diagnostic
substitution for transient decoded-tile sampling, not original rendering. The player family is
resolved from the same exact accepted transaction only when the base atlas is selected. Godot decodes
the selected 2x or 4x PNG, retains it in the base viewport, and maps it onto the current typed
player-position cell after every projection. Session position remains authoritative; the catalog,
presenter, and viewport do not own gameplay or facing rules.

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
Admission animation counter, admission-visible frame, live palette at admission, movement-facing
timing, DMA/cache completion, original rendered-color parity, and final pixels remain **Unknown**.

The optional named `edge-scale2x` world treatment is an explicit 9A presentation deviation layered
after that exact admission. It is accepted only with the explicit private base-view and base-atlas
selection; exact-nearest remains the default. Godot first requires the selected atlas bucket to be an
exact nearest replication of the typed payload, then the adapter composes the current 288-by-168
logical base-world crop from the authoritative working layout, block selection, tile slots, and flips.
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
6. expand the player family to animation only after frame choice, facing/movement timing, and runtime
   state ownership are closed; do not relabel the current frame as standing, idle, or admission-live;
7. expand asset families only after their source, derivation, cache, and failure rules are closed;
8. close music and sound-effect format/loop/streaming contracts separately.

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
| implemented world-family tooling prerequisite | fixed private Map 3 ROM/metadata roots; exact palette/slot selection; five-segment 128-by-320 source-crisp atlas; deterministic nearest 2x/4x ignored candidate; no implicit promotion or update |
| implemented player-reference consumer | fixed private ROM root; exact controlled player, regular map-sprite, DOWN source-slot, no-mirror, and half-zero selection; bounded Basic decode; reviewed 24-by-24 `initial-reference-frame` master and nearest 2x/4x local transaction; exact Content mount and thin Godot logical-cell projection; no standing/idle, animation, or admission-visible claim |
| implemented bounded consumer | reviewed frame/cursor/base-atlas master/runtime/manifest transactions; explicit independent HUD, base-view-plus-atlas, and static-overlay-diagnostic opt-ins; exact semantic lookups; 2x/4x selection; Content-owned contained payload recheck; chrome-only fallback; typed ENTER/STAY and cursor overlays; full selected base-atlas physical raster mapped through the authoritative project-authored logical crop with exact-nearest startup parity; the static overlay is a no-player diagnostic using the admitted map palette by explicit policy; no source/master runtime input, PCK, or fidelity claim |
| separate implementation decision | live resize bucket remount and fullscreen/monitor transition UX; platform safe-area integration; original or generalized Yes/No behavior; admitted product font/theme/input glyphs; general window chrome/Theme migration; user-selectable UI scale beyond the current 100% limiting-frame calculation; tracked-master rebuild/update transactions; cache retention/review lifecycle beyond one fresh candidate |
| Unknown | original background-layer palette source; camera/layer/priority-with-sprites/animation composition; final-pixel fidelity; natural route and timing; complete UI/text behavior; audio format/loop/streaming; H4 and 8C parity |

The local asset repository and three bounded semantic consumers now exist, but this document still does
not authorize a product batch, PCK inclusion, presenter-wide migration, original Yes/No/cursor
interaction, admitted product font/input glyphs, or a general Theme. Those changes require separately
owned, reviewable implementation slices.
