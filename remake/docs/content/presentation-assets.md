# Presentation Assets and Private Preparation

Content owns external asset identity and prepared resource admission. The ordinary runtime consumes
prepared private worlds/scene documents; it does not reopen a ROM or reconstruct a legacy Godot
asset catalog. [Profiles and trust](profiles-and-trust.md) owns runtime admission and redistribution.
[Godot](../godot/presentation.md) owns disposable rendering, not source provenance.

## Public localization pilot assets

The game bundles one unmodified OFL 1.1 Noto Sans CJK SC Regular font and its
[exact upstream pin/notices](../../game/fonts/localization/README.md). The adjacent `.otf.import`
records `res://` paths and disables system fallback; generated `.godot` font data stays local.
The original authored localization pilot contains only simple solid diagnostic rasters, a generated
20ms tone and original prose/translation. Its minimal glyph metrics express authored logical work,
not original font evidence. No source-derived script, translated private text or extracted asset is
publicized. Package display overlays are optional separate inputs; `SF2_DISPLAY_CATALOG` is explicit
and absolute, with existing private repository/commit/ROM identity checked by Content when applicable.
See [admission/fallback](../application/localization-design.md#implemented-m1-boundary).

## Master and Runtime Buckets

Original material is the canonical private source. Newly authored raster masters and reviewed
derivatives remain distinct from ignored candidates. Existing tooling supports the versioned 2x/4x
asset buckets; this is not proof that the ordinary host implements the old bucket-selection UI.

### Source authority

Private source art/audio and accepted asset Git objects own material identity. A generated cache,
preview, screenshot or mutable checkout name does not replace that authority. Source color changes,
smoothing and derivative policy require explicit reviewed provenance.

### Bucket selection

The earlier high-DPI product proposal uses 4x new-raster masters, deterministic 2x/4x buckets and one
resident selection. Its retired catalog/window consumers are historical. Keep bucket policies and
derivation identity explicit when preparing material; ordinary runtime resource compatibility must
be checked at the actual prepared-world/scene consumer. [Presentation policy](../godot/presentation.md#presentation-policy-and-current-layout)
records the current layout and deferred product UI boundary.

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

The maintained `sf2tool.remake_assets` checkout/export preflight consumes an explicitly selected
local asset repository or pack and verifies the exact manifest/payload relationships. Selected asset
commit, semantic IDs, source capture/derivation and payload identities remain bound. Presence of a
directory is not profile selection; private inputs cannot fall back to synthetic product material.

Offline `sf2tool.remake_exploration_content` and `sf2tool.remake_battle_scene_content` prepare the
world/scene resource inputs consumed by current Content readers. Runtime reads those selected inputs
once, validates profiles, references, raster/audio shape and digest and admits immutable resources.
Paths stay at the preparation/Content boundary; session observations and ordinary diagnostics remain
path-free. Godot renders admitted bytes and resource selections without authenticating repositories.

The former direct `private-local` Godot catalog/mount route is retired. Its prospective error names
and mount API are not the current `SessionFailure` API. Missing or inconsistent current input returns
typed ContentError/UnsupportedCapability; actual delivery failure is AdapterError and cannot release
a pending engine completion as success. See [private admission](profiles-and-trust.md#private-admission-layers).

## Deterministic Derivation and Cache

Main Git owns schemas, maintained generators/readers and minimum authored fixtures. The separate
private asset Git owns source/master/runtime/manifest history. Decoded intermediates, candidates,
font/audio imports, previews, `.godot`, exports, logs and scratch remain ignored worktree-local state.

Derivation binds source identity, policy/generator version, dimensions/format, semantic selection and
the resulting runtime payload. Reuse accepted assets read-only; a successful candidate/preflight
does not promote it or mutate the asset library. An owned change reviews and publishes the exact
master/runtime/manifest transaction separately. Do not rebuild material merely because a task changes.
The original [cache contract and completed candidate receipts](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1c4c786a6e2c630e1cfadf4daa88dbd7687caa19/remake/docs/presentation-and-assets.md#deterministic-derivation-and-cache)
retain tool versions, commands and detailed identities.

### Map57 candidate policy

Map57 is an explicitly prepared private resource selection, not an inferred ordinary startup map.
Retain its palette/slot/map-header provenance and unloaded-slot boundary; candidate generation does
not establish natural route, palette fidelity or general map loading. The complete
[candidate policy and reproduction](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1c4c786a6e2c630e1cfadf4daa88dbd7687caa19/remake/docs/presentation-and-assets.md#map57-candidate-policy)
preserve the source joins and retained results. Current before-battle projection has its
[Godot owner](../godot/presentation.md#bound-before-battle-scene-presentation).

## Private Command Menu Icons

The current diamond-menu experiment uses the original square attack/magic/item/stay rasters directly
at the four cardinal positions, without another drawn frame or per-icon caption. One shared window
shows the focused command. This is a private presentation choice, separate from the
future SVG pipeline below. No authored icon substitutes for a missing original raster.

Prepare a copy of an admitted battle scene with `uv run python -m sf2tool.remake_command_menu
--rom <registered-rom> --upstream <pinned-upstream-with-H1-listing> --scene <admitted-scene.json>
--out-dir <fresh-ignored-directory>`. The helper reuses `texture_extract` source/ROM verification,
base-palette decoding and tile rendering. `tiles_MainMenu` in
`data/graphics/tech/menus/mainmenutiles.bin` contains seven 576-byte icons; the first four supply the
four commands. Each 288-byte half is nine tiles in the row-major 3×3 arrangement of
`diamondmenulayout.asm`: normal first, selected second. These are source-backed pixel/layout facts,
not a claim about original animation timing or modern visual parity.

The output scene adds eight 24×24 RGBA8 rasters named
`command-menu.{attack|magic|item|stay}.{normal|selected}`, with the existing raster digest fields.
`provenance.json` records pinned upstream/ROM identity, source offsets and raster digests. Select the
new scene through the existing `SF2_PRIVATE_BATTLE_SCENE_CONTENT` input; no new startup switch or
resource loader is involved. Preparation reads source inputs only and refuses to replace an output
directory or an already-populated command-menu namespace. The eight raw rasters total 18KiB, in
addition to the copied scene and JSON encoding.

The ordinary Content reader admits the rasters. The adapter requires all eight when any is present,
requires 24×24 dimensions, and owns/replaces textures with the active scene. A malformed or partial
set fails through the existing adapter-error boundary. Nearest sampling, no mipmaps and integral
icon scale preserve pixel edges. Scenes with no command-menu namespace display text-only squares.
Extracted pixels and prepared scenes stay ignored and must not enter public packages or PR attachments.

## Private church menu icons

**Confirmed:** the optional `world.presentation.churchMenu` contains exactly
`{raise|cure|promote|save}.{normal|selected}` using the existing authenticated raster fields.
All eight must be24×24; absence preserves compatible public/older inputs and renders short authored
text squares. Private original pixels remain ignored, never bundled in public content or PR attachments.
Content validates the set/shape/bytes; native decoding failure stops the host and releases temporary
textures. The [current UI agreement](../godot/presentation.md#church-and-shared-save-agreement-1519)
owns focus, choice and service behavior.

Use the existing preparation entry after same-process input/tool configuration:

```powershell
uv run python -m sf2tool.remake_command_menu --rom $rom --upstream $upstream `
    --world $world --out-dir $freshIgnoredOutput
```

`--world` accepts either a private-world wrapper or a standalone world object; it adds only church
rasters and emits `world.json`/`provenance.json` into a new destination. Preserve the original inputs.
Relative selected encounter files and external audio are copied beneath that destination with their
existing relative paths; escaping/missing dependencies fail before publication. No world rule,
program, event or source-asset substitution is performed. Input/output CLI paths resolve from the
caller's cwd; dependencies resolve from the explicit input world's directory. Use absolute selections
when invoking from another checkout or working directory.

A read-only pinned checkout may omit split binaries and generated H1 output. Select an existing
pinned H1 listing with `--listing $listing` and deliberately add `--rom-assets` to decode the verified
canonical ROM directly. The source identity/parity owner is `sf2tool.graphics_source`; existing imports through
`texture_extract` retain their entry points. The unchanged split-payload parity reader remains mandatory for every split
payload that is present; mismatched bytes/lengths reject. This route verifies the same canonical ROM
identity, pinned upstream, existing accepted H1 menu addresses/pointer and palette address. It does
not build H1, modify upstream or infer permission to redistribute pixels.

Named provenance: pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`menustringspointertable.asm:pt_ChurchMenuNames` gives Raise/Cure/Promo./Save order;
`diamondmenu.asm:dmaDiamondMenuIcon_Up/Left/Right/Down` reads offsets0/0x240/0x480/0x6C0
and adds0x120 when frame counter>=15. Each288-byte half is nine row-major tiles. The modern
normal/selected assignment uses those two halves; it does not reproduce the original15-frame cadence
or inserted side border tiles. `tiles_ChurchMenu` is ROM0x2B072,934 compressed bytes through
`tiles_ShopMenu`0x2B418; the decoder consumes7438 bits into2304 bytes. Existing
`tests/fixtures/h2/ui-graphics-decode-v1.json` binds these menu addresses.
`displayinit.asm:InitializeDisplay` copies `palette_Base`/`CRAM_PALETTE_SIZE` into `PALETTE_3_BASE`;
`diamondmenulayout.asm` selects `PALETTE3` for its menu tiles. The accepted player-frame fixture's
`static.indexBindings.paletteBase`/`static.palettePolicy` binds ROM0x309E,32 bytes/16 big-endian
0x0EEE words. Index0 stays transparent. Preparation records those addresses, consumed bits,
source mapping and each raster's existing content digest.

Eight raw rasters total18,432 bytes (about25KiB base64). One existing world publication is O(n)
time/memory plus its finite selected dependencies. The observed private input is125,828,417 bytes;
publication is125,978,971 bytes plus669,487 dependency bytes and2,490 provenance bytes. Use one world copy, one process,
<3GiB peak and<1GiB total scratch; reassess before larger worlds/dependency sets. Native acceptance
uses semantic samples under5MiB/run, no screenshots or whole-route acquisition.

## Public-authored battle gauges

The shared ordinary-game status card loads four small PNGs directly from
[`game/assets/ui/battle-status`](../../game/assets/ui/battle-status/README.md), using Godot's existing
resource import/packaging. The accompanying source statement dedicates the project-authored
geometric frame, dark track and two shaded fills to CC0 1.0. Frame size is 244×28 RGBA8; track/fills
are 232×16. Lossless imports disable mipmaps; the card uses nearest sampling and crops fill width
from the live current/max ratio. Labels and numbers remain native text. These images are public,
contain no extracted/private pixels, and are not a reconstruction claimed as original art.

Read-only inspection of local asset Git `293c9460818f4768905be809e83488d77f698fba` found admitted
yes/no-frame and tactical-cursor masters but no gauge resource in its manifest/runtime. Unreviewed
SVG candidates therefore supply no runtime mapping. This small public set does not modify that
independent repository, private preparation/admission, source provenance or its license boundary.
Its PNGs are the masters and runtime resources; no new derivation platform or private input is needed.

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

## Public Synthetic and Test Fixtures

The `public-synthetic` smoke/export profile was retired at M5. Project-authored fixtures may still
exercise dimensions, animation counts, missing assets, bucket selection, cache drift, safe areas, and
cue-to-resource mapping. Fixtures and synthetic presentation are never selected as product content,
and an explicit local product request cannot fall back to them while reporting success.

## Product Design and Distribution Limit

The SVG/font/theme specifications above constrain proposed product assets and existing candidate
tooling; they do not claim that the ordinary host has a complete product Theme, font/glyph UI or
general HUD catalog. #438 owns later art/UI/UX choices. No public export or distribution right is
established by private preparation, local asset Git history, successful checks or native play.

## Private exploration menu icons

`presentation.fieldMenu` optionally supplies exactly member/magic/item/search normal/selected
24×24 rasters. The ordinary Content raster identity/shape checks remain authoritative; partial sets
reject. The adapter displays unchanged pixels at nearest4× through the shared diamond control.
Missing art keeps short text captions; native decode failure stops once with
`field-menu-resource-invalid`. No original bitmap is tracked or granted public distribution rights.

The existing `sf2tool.remake_command_menu` world publisher adds `--menu field`; its default remains
Church and its battle scene route is unchanged. Example after loading the local input configuration:

```powershell
uv run python -m sf2tool.remake_command_menu --rom <verified-rom> --upstream <pinned-upstream> `
    --world <existing-world-or-authored-package> --out-dir <new-ignored-directory> `
    --listing <existing-pinned-H1-listing> --rom-assets --menu field
```

Only use explicit `--rom-assets` when the pinned read-only checkout lacks split binaries. Identity,
H1/accepted table addresses, pointer and palette checks precede decoding; present split payloads
still require strict ROM parity and exact size. `tiles_MainMenu` is4032 uncompressed bytes at
0x29A02, `p_tiles_MainMenu` at0x28010 and `pt_tiles_Menu` at0x1115C. The first packed entry
`0x85010204` selects icon records5/1/2/4 (Member/Magic/Item/Search); the accepted
`tests/fixtures/h2/ui-graphics-decode-v1.json` binds those addresses and indices. Palette_Base at
0x309E and its32 bytes follow the existing church palette binding. The pinned revision and source
symbols are in the [gameplay agreement](../godot/presentation.md#exploration-gameplay-diamond-agreement-1588).
A provenance receipt records source offsets, canonical identity, palette and resource bytes.

Minimum work is8×24×24×4=18,432 raw icon bytes, approximately25KiB base64; publication additionally
reads/writes its whole input JSON and selected finite relative encounter/audio dependencies. The
bounded observation reused one126,677,125-byte private publication and tiny authored variants,
with O(input bytes) time/space, <3GiB assumed peak memory and <1GiB scratch. Existing private data
and listing remained read-only. No full asset export, network acquisition or comparison pipeline
was added. Replan for new dependencies, escaping paths or measured volume/memory above that budget.
