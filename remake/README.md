# SF2 Remake

This directory contains the independently maintained Godot 4.7.2 .NET/C# remake. It consumes
accepted repository contracts and uses project-authored or caller-supplied local content without
making engine code an evidence owner.

## Current Status

Phase 4 implementation is active through bounded Map 3 slices. The current runtime supports a
tracked public-synthetic exploration shell, one project-authored 3-by-2 tactical micro-battle with a
deterministic enemy response, typed victory/defeat, same-definition retry, an atomic once-only
synthetic victory effect, and exploration return. A separate private-local traversal shell with an
opt-in project-authored Map 3 base view can explicitly enter the same tactical reducer at the
controlled start; defeat/retry and victory preserve the exact private traversal snapshot and never
apply the public synthetic world effect. Its optional local HUD panel can acknowledge entry or decline
the one-shot project-authored request before movement resumes. A separate explicit base-atlas option
can replace only that view's transient decoded-tile sampling with the reviewed local 2x/4x nearest
atlas while retaining the same Application snapshot, block/tile selection, crop, and player marker.
The selected bucket remains a full 576-by-336 or 1152-by-672 physical raster mapped into the fixed
288-by-168 logical crop; Godot does not collapse it to a one-pixel-per-logical-unit intermediate.
This manual bridge and diagnostic atlas are not natural
battle admission, Battle 01, an original after-battle program, or the accepted continuous Map
3-through-Battle 01 milestone, which remains **NOT READY**.

The two persistent runtime disclosures are part of the product boundary:

- `PUBLIC SYNTHETIC — NOT ORIGINAL FIDELITY`
- `PRIVATE LOCAL — NOT FULL ORIGINAL FIDELITY`

Current capabilities and retained Unknowns are summarized in
[Capability Status](./docs/capability-status.md).

## Runtime Profiles

| Profile | Input boundary | Current purpose |
| --- | --- | --- |
| `public-synthetic` | tracked, project-authored content only | default interactive shell, logic tests, local Godot gate, and redistribution-safe export smoke |
| `private-local` | explicit caller-selected ignored inputs with fixed admission checks | bounded original Map 3 traversal, local diagnostics, an optional project-authored base view, and a manual project-authored battle bridge; not a full-original runtime |

Private execution is never inferred from a file's presence and never silently falls back while
reporting private success. See [Runtime Profiles and Trust](./docs/runtime-profiles-and-trust.md).

## Local Presentation Asset Preflight

The local-only `md-sf2-remake-assets` checkout remains a separate product-art repository. Before a
checkout or exported pack can be offered to the accepted Content reader, the repository-owned
preflight verifies an explicitly pinned commit, tree, and manifest digest, a clean local-only Git
state, the closed manifest, and every referenced runtime payload:

```powershell
uv run python -m sf2tool.remake_assets checkout `
  --asset-root <fully-qualified-asset-checkout> `
  --expected-commit <40-lowercase-hex> `
  --expected-tree <40-lowercase-hex> `
  --expected-manifest-sha256 <64-uppercase-hex>

uv run python -m sf2tool.remake_assets export `
  --asset-root <fully-qualified-asset-checkout> `
  --expected-commit <40-lowercase-hex> `
  --expected-tree <40-lowercase-hex> `
  --expected-manifest-sha256 <64-uppercase-hex> `
  --destination <fully-qualified-new-export-directory>
```

The path-free descriptor carries the exact asset commit, tree, manifest digest, capability, and
bounded pack totals required by outer composition. Export writes only the manifest and its referenced
`runtime/` payloads to a fresh sibling staging directory, verifies the copy, writes the descriptor
last, and atomically promotes it without modifying the source checkout or overwriting an earlier
export. It never copies `.git`, `source/`, `masters/`, ignored caches, previews, or Godot import state.

This is transport and checkout preflight, not asset generation or product admission by itself. A
separate candidate builder closes the deterministic HUD SVG derivation boundary:

```powershell
uv run python -m sf2tool.remake_asset_build hud-svg-candidate `
  --asset-root <fully-qualified-asset-checkout> `
  --expected-commit <40-lowercase-hex> `
  --expected-tree <40-lowercase-hex> `
  --asset-id hud.<name> `
  --expected-master-sha256 <64-uppercase-hex> `
  --resvg-archive <fully-qualified-resvg-win64.zip> `
  --candidate-name <fresh-cache-child>
```

The builder admits exactly one nonignored untracked `masters/ui/<name>.svg` over an otherwise exact
local-only checkout, verifies the pinned resvg archive and executable version, renders deterministic
2x/4x RGBA8 PNGs twice, validates the existing pack schema, and atomically publishes only a fresh
direct child under ignored `cache/`. It never stages, promotes, commits, or prints a local path.

The same tooling host also closes the first private Map 3 world-family candidate boundary without
turning the candidate into an accepted asset transaction:

```powershell
uv run python -m sf2tool.remake_asset_build map3-base-atlas-candidate `
  --asset-root <fully-qualified-asset-checkout> `
  --expected-commit <40-lowercase-hex> `
  --expected-tree <40-lowercase-hex> `
  --rom <fully-qualified-accepted-rom> `
  --expected-rom-sha256 <accepted-uppercase-sha256> `
  --tileset-metadata <fully-qualified-accepted-tileset-metadata> `
  --expected-tileset-metadata-sha256 <accepted-uppercase-sha256> `
  --palette-metadata <fully-qualified-accepted-palette-metadata> `
  --expected-palette-metadata-sha256 <accepted-uppercase-sha256> `
  --candidate-name <fresh-cache-child>
```

Actual bytes and caller pins must both match the fixed accepted ROM and metadata roots before either
metadata document is parsed. The builder derives the accepted Map 3 palette and ordered five-slot
selection, decodes only those five 4,096-byte buffers, and emits one ignored private source bundle,
one 128-by-320 master atlas, nearest-neighbor 2x/4x buckets, and a single-asset candidate manifest.
Each of the five vertical 128-by-64 segments is exactly 128 8-by-8 tiles on a 16-by-8 grid. Palette
index zero is transparent; the existing deterministic Mega Drive three-bit channel expansion maps
the other colors to straight-alpha sRGB RGBA8. That mapping is a project-authored review/runtime
candidate policy, not hardware, display, colorimetric, final-pixel, or original-fidelity parity.

The builder result remains under ignored `cache/`; it does not stage, promote, commit, export, mount,
enter a PCK, or authorize public redistribution. Promotion remains a separate reviewed local
asset-repository transaction.

The player reference-frame candidate uses the same fixed ROM boundary without claiming a live
admission frame:

```powershell
uv run python -m sf2tool.remake_asset_build map3-player-reference-frame-candidate `
  --asset-root <fully-qualified-asset-checkout> `
  --expected-commit <40-lowercase-hex> `
  --expected-tree <40-lowercase-hex> `
  --rom <fully-qualified-accepted-rom> `
  --expected-rom-sha256 <accepted-uppercase-sha256> `
  --candidate-name <fresh-cache-child>
```

It derives the accepted controlled player selection, regular map-sprite zero, DOWN source slot two,
no horizontal mirror, and the first of the two decoded 24-by-24 halves. That half is named only
`initial-reference-frame`: admission animation counter, admission-visible frame, movement-facing
timing, DMA/cache completion, and the live palette at admission remain Unknown. The output is one
ignored source bundle, a 24-by-24 master, and nearest-neighbor 2x/4x buckets under the existing closed
manifest schema. Palette index zero is transparent; the same project-inferred channel expansion is
used without a hardware, final-pixel, standing, idle, or original-visible-frame claim. As with the
atlas builder, review and promotion are a separate local-only asset-repository transaction.

The reviewed local asset history now owns `hud.yes-no-window-frame`,
`hud.tactical-selection-cursor`, `world.map3.base-tileset-atlas`, and
`world.map3.player.initial-reference-frame`. The current four-asset checkpoint is local commit
`f7a351f24e328c47b10a892613edeac07a07635a`, tree
`9cc4c0959ebbe067f22adae5c079a65bcfd1f06d`, with manifest SHA-256
`56382461FAA5168939A264FC37ABC8A7590A0D099C19DFB54DC0DC6F96F5DCB6`. The atlas and player runtime
buckets are reviewed nearest 2x/4x outputs; source and master material remain review/provenance
inputs, not runtime files. The player asset is consumed only by the explicit base-atlas path as a
project-authored 24-by-24 logical-cell projection of `initial-reference-frame`.

An explicit PrivateLocal launch may opt into HUD assets with `--private-hud-preview`, or into the
atlas diagnostic with both `--private-map3-base-view` and `--private-map3-base-atlas`. Either asset
selection requires one fully qualified asset root, the exact lowercase mounted commit string, and the
exact uppercase manifest SHA-256. Atlas selection also requires the existing private ROM and metadata
inputs because the authoritative typed visual binding remains required. The Content reader admits the
complete pack before `GameSession` starts; HUD plus atlas share that one admission. The thin Godot
catalog resolves only requested semantic IDs and accepted 2x/4x buckets. It asks the same Content
reader to reopen the fixed manifest, resolve and recheck each contained
path/length/digest, and return a defensive byte copy before Godot decodes it. No runtime path crosses
into Application or Godot. Partial
values, an implicit mount, or a failed private mount never fall back while reporting private success.
The base-atlas selection also requires the exact player reference asset from the same accepted local
transaction. Godot replaces only the base view's project-authored player marker with that texture,
keeps semantic position in the private session, and reprojects after movement. It does not select
animation halves, call the frame standing/idle, or claim that it was visible at original admission.

The explicit PrivateLocal product profile now applies one adaptive windowed startup policy before any
presentation payload is selected. The logical canvas and project fallback remain 960 by 540. On a real
desktop, an unmodified fallback window chooses the largest client tier from 1920 by 1080, 1600 by 900,
1280 by 720, and 960 by 540 whose measured decorations also fit the current usable screen, then centers
that window and sets the runtime minimum to 960 by 540. Fullscreen, maximized, command-line resolution,
and other already established non-default physical targets remain unchanged. HiDPI is enabled, but the
final Godot client size is used directly and is never multiplied by the Windows DPI scale again. The
selected 2x or 4x asset bucket remains resident until restart; resizing during a session scales or
letterboxes the unchanged logical canvas and does not reread private content or hot-swap a bucket.
PublicSynthetic startup and both stable smoke receipts remain unchanged.

The runtime reader does not inspect a Git checkout or infer its current `HEAD`; it matches the
caller-supplied mounted commit string and the fixed manifest/payload identities. The repository-owned
local preflight above separately proves the actual asset checkout's commit, tree, clean state, lack of
remotes, and manifest before launch or export.

The current consumer uses the limiting physical dimension of the centered 16:9 frame at 100% UI scale;
user-selectable UI scaling remains deferred rather than being guessed by this slice.

Without the optional base-view battle bridge, the explicit mount remains the existing chrome-only
diagnostic preview. With that bridge, Godot projects its typed Pending state as a bounded ENTER/STAY
panel: `N` sends the exact existing entry acknowledgement and `Backspace` sends an exact one-shot
decline through `GameSession`. Decline is terminal for that session's bridge, restores movement, and
does not mutate the private map snapshot. The built-in-font labels are project-authored diagnostic
copy, not an admitted product font, original Yes/No text, an icon, a Theme, or original UI behavior.
While the bridge is Active, the verified transparent cursor raster is an additive overlay positioned
only from the existing typed `HasCursor` cell; the gold cell highlight and `▣`/occupant fallback remain
visible and authoritative state remains outside Godot. Missing or drifting frame/cursor payloads fail
the requested private mount closed. Atlas-only selection does not imply either HUD consumer, and
HUD-only behavior is unchanged. The atlas consumer uses only the selected runtime PNG, validates its
Godot-decoded RGBA8 shape, and samples nearest pixels through the existing authoritative block/tile,
slot, and flip projection. Every selected bucket texel survives into a physical ImageTexture, full
physical tile axes are flipped together, and `DrawTextureRect` maps that raster back to the unchanged
logical crop before the logical player marker is drawn. Current startup admission still requires
every scale-squared atlas sample to exactly repeat the typed 1x payload; experimental edge-aware,
xBRZ, and color-ramp variants therefore remain rejected pending separate asset and policy review.
Its additive local-only marker is emitted last only when explicitly selected; the marker payload,
the four earlier private markers, and the public marker remain byte-stable. The separate
`md-sf2-gfx-remake` repository remains
non-authoritative R&D: its measurements informed the product-authored frame, but its ignored
experimental SVG contains forbidden text and an embedded raster and is not an admitted product master
or runtime input.
Because the product asset repository intentionally has no remote, an exact local commit proves
identity but does not provide off-machine recovery; source/master backup remains a separate local
operational responsibility. Rollback selects a prior reachable local commit or an immutable prior
export and never rewrites or overwrites accepted history.

## Build and Test

Run the locked .NET workflow from this directory:

```powershell
dotnet restore Sf2.Remake.sln --locked-mode
dotnet build Sf2.Remake.sln --configuration Release --no-restore
dotnet test Sf2.Remake.sln --configuration Release --no-build --no-restore
```

The pinned SDK, package versions, and NuGet source are tracked beside the solution. Relevant runtime
changes also use the repository-maintained local official Godot gate. GitHub Public intentionally
remains lightweight and does not download or run Godot.

See [Development and Verification](./docs/development-and-verification.md) for planner routing,
local Godot validation, private-smoke boundaries, and process cleanup requirements.

## Repository Layout

```text
remake/
  Sf2.Remake.sln                 locked whole-remake solution
  src/
    Sf2.Remake.Domain/           deterministic state, values, and reducers
    Sf2.Remake.Application/      GameSession, commands, ports, and observations
    Sf2.Remake.Content/          validated public/private input adapters
  game/                          Godot project and thin host adapters
  tests/                         Domain, Application, Content, and Godot tests
  docs/                          implementation architecture, profiles, status, and workflow
  global.json                    pinned .NET SDK
  toolchain.json                 official Godot artifact identity and bounded timeouts
```

The dependency and delegation map is documented in [Architecture](./docs/architecture.md).

## Boundaries

- `GameSession` is the sole logical gameplay mutation facade.
- Domain and Application code do not depend on Godot, JSON, machine paths, or original payloads.
- Content validates external identities and structure before constructing admitted definitions.
- Godot maps input to semantic commands and projects authoritative observations; it does not own
  gameplay rules.
- The public-synthetic tactical micro-battle is deterministic project-authored content. Its actor,
  enemy, grid, movement/attack ranges, hit points, damage, fixed north/east/south/west enemy movement
  tie-break, victory/defeat, retry, completion flag/effect/setup, cues, and return state make no claim
  about the original game. Only victory applies completion state once through `GameSession`; defeat
  applies no world effect and exact acknowledgement restarts the same definition.
- The private-local battle bridge reuses only that tactical definition, commands, cues, and Domain
  reducer. It pauses private traversal while active, preserves the same private snapshot through
  defeat/retry and victory, and does not import the public completion flag, effect, setup, facing, or
  return-map state. Its project-authored pending choice either acknowledges entry or records a
  one-shot terminal decline without changing that snapshot.
- Private ROMs, canonical imports, decoded payloads, captures, tools, and generated exports remain
  ignored and local. None is committed, uploaded, or embedded in the public package.
- Public-synthetic import/export success grants no right to distribute original content.
- Natural route, Battle 01 continuity, original rendering/audio/assets, persistence, complete H4,
  and 8C fidelity remain incomplete, deferred, Unsupported, or Unknown at their existing owners.

## Architecture Decisions

- [ADR 0008](../docs/decisions/0008-godot-csharp-cli-first-remake-tooling.md) fixes Godot 4.7.2
  .NET/C# and the CLI-first, plain-C# core.
- [ADR 0011](../docs/decisions/0011-phase4-remake-runtime-architecture.md) fixes the four assemblies,
  state ownership, Content ports, Godot adapter, and H4 layering.
- [ADR 0017](../docs/decisions/0017-heavy-boundaries-light-internals.md) retains heavy trust,
  mutation, versioned-port, and observation boundaries while keeping same-process internals light.

Historical implementation slices and review chronology remain in Git and merged pull requests rather
than this entry document.
