# Runtime Profiles and Trust

## Purpose

Runtime profiles declare where content comes from, which trust checks are required, and which product
claims are permitted. Profile selection changes outer composition; it does not fork Domain rules or
make Godot an evidence owner.

## Profile Summary

| Profile | Selection | Admitted inputs | Current claim |
| --- | --- | --- | --- |
| `public-synthetic` | default, or explicit public selection | tracked project-authored package and tracked placeholder presentation | redistribution-safe implementation and export smoke; **not original fidelity** |
| `private-local` | explicit profile plus one explicit fully qualified ignored canonical-import path | caller-owned local inputs admitted by fixed identity, provenance, shape, and capability checks | bounded original Map 3 traversal, optional project-authored base composition, and explicit synthetic battle bridge; **not full original fidelity** |

The runtime always displays the appropriate disclosure:

- `PUBLIC SYNTHETIC — NOT ORIGINAL FIDELITY`
- `PRIVATE LOCAL — NOT FULL ORIGINAL FIDELITY`

## Public Synthetic

The default profile reads the tracked `public-synthetic-map3-smoke-v1` package. The package is
project-authored, raw-byte locked, closed in shape, and validated before `GameSession` starts. Its maps,
entities, dialogue, discoveries, item, transitions, cues, and presentation are synthetic test/product
content rather than reconstructed original facts.

PublicSynthetic supports the maintained local source and export smoke. A successful build or export
proves toolchain, adapter, package, and redistribution-boundary behavior only. It grants no permission
to include original assets and makes no 7C, 8C, natural-route, or H4 claim.

PublicSynthetic rejects private profile options. It cannot consume a canonical import or report private
success.

## Private Local

PrivateLocal is selected only when both user arguments are present with explicit values:

```text
--runtime-profile=private-local
--canonical-map-import=<fully-qualified-ignored-path>
```

The profile is never inferred from a discovered file, environment default, or prior run. The caller's
path remains at the outer Godot composition and Content-reader boundary. It does not cross into Domain,
Application snapshots, receipts, status text, smoke output, or committed configuration.

The optional base view is independently explicit and requires all three additional ignored inputs:

```text
--private-map3-base-view
--original-rom=<fully-qualified-ignored-path>
--map-tileset-metadata=<fully-qualified-ignored-path>
--map-palette-metadata=<fully-qualified-ignored-path>
```

Supplying any visual path without the opt-in, omitting one input, or using a relative path makes the
requested profile unavailable. The traversal-only private profile remains valid without these options.

When the base view is admitted, a manual semantic input at the controlled start may request the
project-authored tactical micro-battle from the tracked public-synthetic package. This does not infer
an original battle from the private input. Application pauses private traversal while the bridge is
pending or active, owns the tactical lifecycle, and returns to the exact same private traversal
snapshot. Public-synthetic completion flags, effects, setup, facing, and return-map state do not cross
the bridge.

Missing, relative, unreadable, malformed, or incompatible input makes PrivateLocal **Unavailable**.
The host does not silently start PublicSynthetic while describing the result as private. Unknown profile
names, duplicate options, split options without explicit values, and private-only smoke flags under the
public profile also fail closed.

## Private Admission Layers

The current private boundary has two sibling Content ports:

1. canonical Map 3 import admits the accepted map projection, traversal inputs, bounded source records,
   and visual-resource references;
2. base visual payload admission validates caller-selected local ROM and metadata inputs and maps only
   admitted decoded buffers and palette forms into Application-owned immutable definitions.

Each production reader checks the accepted fixed trust roots and the caller's additional pins before
parsing the corresponding document where required. Application then checks the typed package,
provenance, capability, dimensions, evidence-owner closure, and cross-port Map 3 selection before a
session or visual runtime binding is created.

Content authenticates actual bytes. Application validates the typed protocol and compatibility between
accepted results; it does not claim cryptographic protection against a malicious in-process port that
fabricates bytes behind an otherwise exact typed result.

## Payload and Path Boundary

The following remain ignored and local:

- ROMs, canonical imports, source metadata exports, decoded graphics, palettes, and captures;
- official engine archives, export templates, and other downloaded executables; and
- generated Godot import state, builds, exports, logs, and smoke receipts.

Private compressed source bytes are validated transiently and do not cross into Application. Admitted
decoded buffers remain in memory and are not committed, printed, embedded in a public package, or
exported by the current private runtime. Diagnostics are typed and path-free.

Use the repository's [Local Private Input Layout](../../docs/operations/local-private-inputs.md) for
machine-private input routing. Worktree-local writable state remains isolated even when immutable shared
inputs are registered centrally.

## Fidelity Boundary

PrivateLocal provides semantic traversal, project-authored diagnostics, and an optional Godot base
view. The view consumes the accepted visual runtime binding and reprojects after movement or admitted
working-layout mutation. It may also present the explicitly requested project-authored tactical bridge
without changing ownership of either traversal or battle rules. Its 12-by-7 player-centered crop,
empty-pixel background, Mega Drive channel expansion, Godot image/marker presentation, and manual
battle trigger are explicit project-authored choices. They are not an original screenshot or a
fidelity backend.

The profile therefore does not claim:

- natural Map 3 route, setup/init/event effects, Battle 01 continuity, or story state;
- original camera, second-layer/overlay/priority composition, animation, text, entities, dialogue, UI,
  audio, or final pixels;
- VRAM, CRAM, VInt, DMA, timing, or other 8C hardware observations;
- save/load, persistence, or complete private-content support; or
- H4 or milestone acceptance.

An absent capability remains Unsupported or Unknown at its owning contract. Private bytes alone never
authorize a fidelity claim.

## Redistribution

Tracked code, metadata, tests, and project-authored synthetic content may pass the public build gates.
Original or caller-provided private payloads must not enter Git, a public PCK, CI artifacts, logs, pull
request attachments, or release downloads. A distributable replacement-asset strategy requires a
separate accepted rights and content decision.
