# Presentation Assets and Private Preparation

Content owns external asset identity and prepared resource admission. The ordinary runtime consumes
prepared private worlds/scene documents; it does not reopen a ROM or reconstruct a legacy Godot
asset catalog. [Profiles and trust](profiles-and-trust.md) owns runtime admission and redistribution.
[Godot](../godot/presentation.md) owns disposable rendering, not source provenance.

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
