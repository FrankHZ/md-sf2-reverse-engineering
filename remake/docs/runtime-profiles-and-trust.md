# Runtime Profiles and Trust

## Purpose

Runtime profiles declare where content comes from, which trust checks are required, and which product
claims are permitted. Profile selection changes outer composition; it does not fork Domain rules or
make Godot an evidence owner. This is the intended trust boundary; the current implementation still
has divergent private/public session APIs and fixed reference admission identified by the
[architecture audit](./architecture-audit.md).
[Current M1](../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m1-implementation)
implements common-session authored admission. The old private/public fixed readers and session paths
are in the [transitional reference assembly](../reference/README.md); their retained source trust
checks are not universal gameplay predicates. The accepted
8C/H4 target remains incomplete.

## Profile Summary

| Profile | Selection | Admitted inputs | Current claim |
| --- | --- | --- | --- |
| `public-authored` | default local start or `--authored-package <path>` | validated immutable battle/actor/rule definitions plus separate explicit controlled start input | implemented semantic movement, HEAL/STAY and ordinary physical first/second/counter subset; explicit authored starting vitals, no original-start/fidelity/export claim |
| `public-synthetic` | explicit legacy public selection | tracked project-authored package and tracked placeholder presentation | redistribution-safe implementation and export smoke; **not original fidelity** |
| `private-local` | explicit profile plus one explicit fully qualified ignored canonical-import path; optional presentation requires the reviewed local asset pack | canonical logical import plus caller-mounted local presentation assets admitted by fixed identity, provenance, shape, and capability checks | bounded original Map 3 traversal, optional project-authored base composition/battle bridge, and optional local HUD frame/entry-choice projection; **not full original fidelity** |

The runtime always displays the appropriate disclosure:

- `AUTHORED BATTLE` with project-authored controlled-start help
- `PUBLIC SYNTHETIC — NOT ORIGINAL FIDELITY`
- `PRIVATE LOCAL — NOT FULL ORIGINAL FIDELITY`

## Public Authored

`AuthoredScenarioPackageReader` accepts the closed `formatVersion: 2` package with map/terrain, actor/spell and encounter
references, resolves all references and validates numeric shape and implemented capability before
creating immutable definitions. A second supported package needs configuration changes only. Duplicate
or missing references and profile forgery are ContentError; unimplemented effects, status/items/classes
or AI are UnsupportedCapability. No raw-byte identity, comparison ID or prior receipt admits play.

The only current start policy is `initialVitals: authored-controlled`; private original new-battle
initialization is not claimed. The immutable definitions contain maximum vitals, actor/rule/spell and
reward data plus genuine encounter `placements`. The separate `start` selects an encounter, supplies
uint `mainSeed`/`thinkingSeed`, `gold` (0–9,999,999) and one `start.actors` record per deployed actor.
Each record requires `actor`, `hp`/`mp` bounded by its definition, `exp` (0–99), `kills`/`defeats`
(0–9999), `status` (currently `none`) and `positionOverride` (null or a closed `{x,y}` object).
Null uses the encounter deployment; an explicit override must be in bounds and traversable. Living
actors cannot overlap. Dead actors remain explicit inputs and become unplaced runtime actors; missing
records/counters and unknown/duplicate references are rejected. Array order does not override the
encounter's stable deployment order.

`ScenarioReadAccepted` returns the package's `ScenarioDefinition` and `BattleStartInput` separately.
The ordinary source entry delegates to `GameSession.Start(definition, start)`, so a previously admitted
definition can serve another explicit start with the same boundary checks and fresh runtime actors.
Definitions never supply hidden session resources. Ordinary spell power remains already adjusted;
full-recovery and level-up remain unsupported. The session reads no files/ROM after admission and
carries its seeds forward. The default loads the tracked yard; `--authored-package <path>` selects
another bounded package, without an authored-export or original-fidelity claim. Further model
modernization and private initialization follow the
[ordered boundary](../../docs/decisions/0019-state-and-content-driven-remake-engine.md#authored-definitions-and-explicit-session-starts).

Physical capability is optional per actor: `physical` is a closed object with `movementType`
(currently `regular`), `prowess` (0 or 3), boolean `promoted` and `leader`, enemy reward `gold`
(0–65535), and `special` (currently `none`). Current kills/defeats are required start inputs, including
for nonphysical actors. Other movetypes, prowess or special rules return
Unsupported; a named unpromoted class cannot declare promoted EXP rules. Current ATT/DEF are explicit
authored effective stats; nonempty equipment and nonzero status remain Unsupported. Ordinary
weaponless range is adjacent Manhattan distance 1. Enemy stored level may be zero.

Encounter `rewards`, when present, is closed to boolean `halvedExperience`. Current gold is supplied
only by `start.gold`. Missing actor physical data or encounter rewards leaves physical commands Unsupported,
without inventing defaults for the HEAL packages. Target death retains identity/accounting and sets
its battlefield position to null. Ordinary first/second/reversed-counter hits resolve temporarily
and publish as one action. Each hit truncates damage EXP before adding to the per-action accumulator;
only a surviving ally who attacked receives EXP and award RNG, including an ally counter during an
enemy action. Killing the original enemy actor by counter still awards that ally. Ordinary ally death
increments capped defeats; enemy death credits one kill/gold award. A reached level-up, leader defeat
or terminal faction outcome returns Unsupported before any hit or movement is published. The two physical configurations
[`stone-court`](../content/authored/stone-court.json) and [`river-post`](../content/authored/river-post.json)
are controlled authored inputs, not original private admission or original enemy reward tables.

The enemy-only `controller: commandset06-script3` selects the already-active source commandset06 with
physical TargetPriorityScript3. It requires a regular `physical` definition, an empty spellbook and MOV 1–63;
status and items remain empty. Encounter rewards are required when the physical action executes.
Living opposing targets must be reachable through legal radius-one attack positions. Candidate
scoring follows reverse slot order; signed raw-priority cohorts are selected before the returned
priority is capped at15. Critical multi-target cohorts use the Regular class table, then the source
largest-movement/later-collected tie rule. Zero attack targets return ATTACK1 failure, followed by
unavailable HEAL1/SUPPORT and MOVE1. Complete raw target costs0–127 select the first lowest-cost living
opponent in slot order. Source preliminary movement uses fixed cost4, then the legal MOV grid and
radius0/1 stopping correction; no legal station yields origin Stay. MOVE1 succeeds and ends the action
in either case, without reaching the later STAY command or attacking again that turn. Movement needs
no rewards definition and draws no RNG. High/incomplete target costs, activation, other commandsets,
mixed action categories and wider movement domains remain explicit Unsupported boundaries.
`stay` remains a separate explicitly authored policy.

The authored `thinkingSeed` uint maps its upper word to the source seed copy, updating only the
high byte through the source rejection loop; the remaining 24 bits are carried unchanged. No command
resets either RNG channel. Enemy last-target state starts unspecified and changes only on a
successful attack; MOVE1 preserves it and script3 does not read initial memory. On late failure the
enemy's temporary movement, candidate draws, HP, rewards, last target and both seeds are discarded together. Prior
player/AI commits remain valid and the failed enemy retains its queue entry.

`classRule` now also accepts `unpromoted-swordsman` (source SDMN0) and `unpromoted-warrior` (WARR2),
alongside `unpromoted-priest` (PRST4) and `ordinary`. This is one class definition, not a separate AI
rank input. These known unpromoted classes require `physical.promoted: false`; `ordinary` supplies
no source class identity. A reached critical multi-target cohort needs known class identities and
otherwise returns `ai-target-class`. Single-highest/noncritical selection never queries unused class
data. Every scored candidate needs a physical definition for potential land damage. No full class,
growth or new movement framework is admitted; the existing regular physical capability owns table
selection. The shared reference Flying-table scalar does not enable flying authored actors.

## Public Synthetic

The explicitly selected legacy public profile reads the tracked `public-synthetic-map3-smoke-v1` package. The package is
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

The optional base view is independently explicit and requires the reviewed local base atlas:

```text
--private-map3-base-view
--private-map3-base-atlas
--presentation-asset-root=<fully-qualified-local-pack>
--presentation-asset-commit=<40-lowercase-hex>
--presentation-manifest-sha256=<64-uppercase-hex>
```

Omitting the atlas or any pack pin, using a relative path, or supplying the retired playable-runtime
options `--original-rom`, `--map-tileset-metadata`, or `--map-palette-metadata` makes the requested
profile unavailable with a path-free diagnostic. Those inputs remain valid only for offline builders,
evidence, and verification. The traversal-only private profile remains valid without presentation.

When the base view is admitted, a manual semantic input at the controlled start may request the
project-authored tactical micro-battle from the tracked public-synthetic package. This does not infer
an original battle from the private input. Application pauses private traversal while the bridge is
pending or active, owns the tactical lifecycle, and returns to the exact same private traversal
snapshot. Public-synthetic completion flags, effects, setup, facing, and return-map state do not cross
the bridge. While entry is Pending, the exact acknowledgement admits the tactical loop; an exact
one-shot decline instead enters a terminal non-busy bridge state and resumes movement without changing
the authoritative private map snapshot.

Missing, relative, unreadable, malformed, or incompatible input makes PrivateLocal **Unavailable**.
The host does not silently start PublicSynthetic while describing the result as private. Unknown profile
names, duplicate options, split options without explicit values, and private-only smoke flags under the
public profile also fail closed.

## Private Admission Layers

The private logical and offline visual boundaries remain separate:

1. canonical Map 3 import admits the accepted map projection, traversal inputs, bounded source records,
   and visual-resource references;
2. base visual payload admission validates caller-selected local ROM and metadata inputs for offline
   tooling/protocol checks and maps decoded buffers and palette forms into immutable definitions; and
3. playable presentation admits the reviewed local asset pack directly and rechecks its exact
   commit, manifest, semantic asset identities, dimensions, bucket policy, and contained payloads.

Each production reader checks the accepted fixed trust roots and the caller's additional pins before
parsing the corresponding document where required. The playable session uses only the canonical import;
the project-authored battle bridge binds to that admitted session/controlled start. The retained visual
runtime binding checks the offline typed protocol but is not invoked by playable Godot startup.

Content authenticates actual bytes. Application validates the typed protocol and compatibility between
accepted results; it does not claim cryptographic protection against a malicious in-process port that
fabricates bytes behind an otherwise exact typed result.

## Payload and Path Boundary

The following remain ignored and local:

- ROMs, canonical imports, source metadata exports, decoded graphics, palettes, and captures;
- official engine archives, export templates, and other downloaded executables; and
- generated Godot import state, builds, exports, logs, and smoke receipts.

Private compressed source bytes and extraction metadata remain offline. The playable profile reads only
the canonical logical import and selected runtime PNGs from the admitted local asset pack; neither is
committed, printed, embedded in a public package, or exported by the current private runtime.
Diagnostics are typed and path-free.

Use the repository's [Local Private Input Layout](../../docs/operations/local-private-inputs.md) for
machine-private input routing. Worktree-local writable state remains isolated even when immutable shared
inputs are registered centrally.

## Fidelity Boundary

PrivateLocal provides semantic traversal, project-authored diagnostics, and an optional Godot base
view. The view consumes the reviewed local asset pack and reprojects after movement or admitted
working-layout mutation. It may also present the explicitly requested project-authored tactical bridge
without changing ownership of either traversal or battle rules. Its 12-by-7 player-centered crop,
empty-pixel background, Mega Drive channel expansion, Godot image/marker presentation, and manual
battle trigger are explicit project-authored choices. They are not an original screenshot or a
fidelity backend.

The optional `--private-hud-preview` shape requires all three of
`--presentation-asset-root=<fully-qualified>`,
`--presentation-asset-commit=<40-lowercase-hex>`, and
`--presentation-manifest-sha256=<64-uppercase-hex>`. Supplying any of those values without the flag,
omitting one, using them under PublicSynthetic, or relying on a checkout's mere presence is
Unavailable. Canonical Map 3 import and the complete local presentation pack both admit before the
existing private session constructor is called. Godot then resolves only
`hud.yes-no-window-frame` for the bounded preview and, when the base-view battle bridge is also
requested, `hud.tactical-selection-cursor`, at the same accepted scale. The same Content reader
reopens the fixed manifest, resolves and rechecks each selected contained PNG, and returns defensive
byte copies for the thin Godot consumers. Either required asset failing identity, shape, bucket, or
payload validation rejects the requested private mount. The absolute root and runtime paths stay in
outer composition and Content; they do not enter Application, the Godot catalog, snapshots, receipts,
status, smoke, or logs.

Without the private base-view battle bridge this remains a chrome-only preview. With the bridge,
Godot shows project-authored diagnostic `ENTER [N]` and `STAY [BACKSPACE]` labels only while the typed
bridge is Pending. `N` sends the existing exact acknowledgement; `Backspace` sends the exact decline
only while Pending and retains its tactical-cancel meaning only while Active. The labels use Godot's
built-in diagnostic font and do not establish an admitted product font, input-glyph, Theme, original
Yes/No text, focus model, window system, or original UI meaning. While the bridge is Active, one
transparent project-authored cursor texture follows only the typed `HasCursor` cell. It is additive:
the existing gold cell highlight and `▣`/occupant glyph remain visible, public-synthetic stays
asset-free, and Godot gains no tactical rule ownership. Existing private and public smoke receipt bytes
and ordering remain unchanged.

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
