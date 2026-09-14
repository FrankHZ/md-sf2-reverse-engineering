# Runtime Profiles and Trust

## Purpose

Runtime profiles declare where content comes from, which trust checks are required, and which product
claims are permitted. Profile selection changes outer composition; it does not fork Domain rules or
make Godot an evidence owner. This is the intended trust boundary; the current implementation still
has divergent private/public session APIs and fixed reference admission identified by the
[architecture audit](./architecture-audit.md).
[Current M1](../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m1-implementation)
implements common-session authored admission; the initialized private entry now uses that same runtime. The old private/public fixed readers and session paths
are in the [transitional reference assembly](../reference/README.md); their retained source trust
checks are not universal gameplay predicates. The accepted
8C/H4 target remains incomplete.

## Profile Summary

| Profile | Selection | Admitted inputs | Current claim |
| --- | --- | --- | --- |
| `public-authored` | default local start or `--authored-package <path>` | validated immutable battle/actor/rule definitions plus separate explicit controlled start input | implemented semantic movement, HEAL/STAY and ordinary physical first/second/counter subset; explicit authored starting vitals, no original-start/fidelity/export claim |
| `private-local-controlled-start` | `--private-battle-start <absolute controlled JSON>` and five explicit input environment selections | selected encounter, pinned enemy/static definitions, external controlled party/start | initialized common session through generated player control and movement/cancel; source enemy continuation remains Unsupported |
| `public-synthetic` | explicit legacy public selection | tracked project-authored package and tracked placeholder presentation | redistribution-safe implementation and export smoke; **not original fidelity** |
| `private-local` | explicit profile plus one explicit fully qualified ignored canonical-import path; optional presentation requires the reviewed local asset pack | canonical logical import plus caller-mounted local presentation assets admitted by fixed identity, provenance, shape, and capability checks | bounded original Map 3 traversal, optional project-authored base composition/battle bridge, and optional local HUD frame/entry-choice projection; **not full original fidelity** |

The runtime always displays the appropriate disclosure:

- `AUTHORED BATTLE` with controlled-start help
- `PRIVATE CONTROLLED BATTLE` with Unknown accounting shown explicitly
- `PUBLIC SYNTHETIC — NOT ORIGINAL FIDELITY`
- `PRIVATE LOCAL — NOT FULL ORIGINAL FIDELITY`

## Public Authored

`AuthoredScenarioPackageReader` accepts the closed `formatVersion: 7` package with map/terrain, actor/spell and encounter
references, resolves all references and validates numeric shape and implemented capability before
creating immutable definitions. A second supported package needs configuration changes only. Duplicate
or missing references and profile forgery are ContentError; unimplemented effects, status/items/classes
or AI are UnsupportedCapability. No raw-byte identity, comparison ID or prior receipt admits play.

Each terrain layout is a closed `{id, legend, rows}` object. `legend` maps a single non-space printable ASCII
symbol to a closed `{surface, protection}` definition; every row symbol must resolve there, including
`#` if used. Rows retain consistent dimensions 1–48; outside the logical rectangle, storage padding is
blocked. No numeric character implies a terrain type. The resolved immutable terrain references are
separate from dynamic actors/occupancy, accumulated move costs and presentation.

| surface | Current regular/priest movement cost |
| --- | --- |
| `open` | 2 |
| `brush`, `rough` | 3 |
| `deep` | 4 |
| `impassable`, `barrier` | Blocked |

Independent `protection` is `none`, `light` or `heavy`, selecting integer land multipliers 256, 230 or 205
respectively. These values feed the existing scalar before critical, counter and downward spread;
they are not floating-point display percentages. The mover rule owns costs; custom cost/probability
fields and unknown properties are not accepted. Unsupported surface/protection values fail explicitly,
and missing definitions/invalid symbols are ContentError. The existing placeholder view reads surface
only; color does not grant passability or damage protection. Original reference movers keep their own
source cost/land tables and use the same propagation kernel, without admitting new authored profiles.

The authored start policy is `initialVitals: authored-controlled`; the separately selected private
initializer below has its own explicit policy. The immutable definitions contain maximum vitals, actor/rule/spell and
reward data plus genuine encounter `placements`. The separate `start` selects an encounter, supplies
uint `mainSeed`/`thinkingSeed`, `gold` (0–9,999,999) and one `start.actors` record per deployed actor.
Each record requires `actor`, `hp`/`mp` bounded by its definition, `exp` (0–99), `kills`/`defeats`
(0–9999), `status` (currently `none`) and `positionOverride` (null or a closed `{x,y}` object).
Null uses the encounter deployment; an explicit override must be in bounds and traversable. Living
actors cannot overlap. Dead actors remain explicit inputs and become unplaced runtime actors; missing
records/counters and unknown/duplicate references are rejected. Array order does not override the
encounter's stable deployment order.

Each closed placement requires `actor`, `faction` (`ally` or `enemy`), `processingOrder` (unique within
the encounter,0–2,147,483,647), `control`, `aiStrategy`, `x` and `y`. The actor definition has no numeric
slot or controller. Faction, order, control and AI strategy belong to this deployment; runtime state derives them rather than inferring allegiance from order.
Content admits at most30 allies and32 enemies per encounter and requires ally level1–99 / enemy
level0–99. Only player-controlled allies and AI-controlled enemies are supported. Source slots are
mapped solely by reference consumers;255 is a valid authored processing order, not a missing actor.
Actor/deployment/start array reordering cannot change round draws, candidate ties or UI selection.

`control` is `player` or `automatic`, independently of the intrinsic actor's capabilities. Player
requires `aiStrategy: null`; automatic requires an explicit supported strategy. `stay` consumes the
automatic entry without selecting an action. `attack-then-approach` attempts an attack and, if no
attack can be selected, executes the accepted approach/movement continuation and ends the turn.
Missing fields and nonstring strategy values reject as ContentError; null automatic policy, player
with a policy, unknown modes/policies and unsupported faction/control pairs reject explicitly as
UnsupportedCapability. There is no default Stay. Content and reusable session start share the same
Domain deployment compatibility checks, including physical/spellbook/MOV requirements below.

Actors require numerical `agility` (0–127) and boolean `extraRoundAction`. False generates one entry;
true permits one additional entry in each generated round. No number or omitted field implies the
flag. Dead or unplaced actors contribute no entries or draws; living entries including extras must
fit the64-entry turn buffer. Source-adjusted signed scores can still sort behind the real sentinel.
The extra entry uses the existing truncated five-sixths basis and two draws, independently of physical
double/counter rules. Broader speed formulas, arbitrary action counts and new statuses are not admitted.

`ScenarioReadAccepted` returns the package's `ScenarioDefinition` and `BattleStartInput` separately.
The ordinary source entry delegates to `GameSession.Start(definition, start)`, so a previously admitted
definition can serve another explicit start with the same boundary checks and fresh runtime actors.
Definitions never supply hidden session resources. Ordinary spell power remains already adjusted;
full-recovery and level-up remain unsupported. The session reads no files/ROM after admission and
carries its seeds forward. The default loads the tracked yard; `--authored-package <path>` selects
another bounded package, without an authored-export or original-fidelity claim. Further private AI/action migration follows the
[ordered boundary](../../docs/decisions/0019-state-and-content-driven-remake-engine.md#authored-definitions-and-explicit-session-starts).

Physical capability is optional per actor: `physical` is a closed object with `movementType`
(currently `regular`), `critical`, boolean `promoted` and `leader`, enemy reward `gold`
(0–65535), and `special` (currently `none`). Current kills/defeats are required start inputs, including
for nonphysical actors. Other movetypes, critical combinations or special rules return
Unsupported; a named unpromoted class cannot declare promoted EXP rules. Current ATT/DEF are explicit
authored effective stats; nonempty equipment and nonzero status remain Unsupported. Ordinary
weaponless range is adjacent Manhattan distance 1. Enemy stored level may be zero.

`physical.critical` is a closed `{chance, damageBonus}` object. Only these paired values are supported:

| chance | damageBonus | Applied semantics |
| --- | --- | --- |
| `one-in-32` | `half` | Critical succeeds on zero from range32 and adds `floor(damage / 2)`. |
| `one-in-16` | `quarter` | Critical succeeds on zero from range16 and adds `floor(damage / 4)`. |

Land reduction precedes the bonus; counter halving and the two downward spread draws follow it.
The immutable typed rule owns the scalar parameters and every first/second/counter hit consumes it.
Missing/nonstring fields are ContentError; unimplemented values or cross-pairs are Unsupported.
Packed `prowess` is no longer an authored field; original source/reference definitions retain it.
Regular dodge and physical double/counter chances remain fixed at1/32 and distinct from extra-round
eligibility. No arbitrary chance/fraction/equipment/status/critical-effect combinations are admitted.

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

The enemy-only `control: automatic`, `aiStrategy: attack-then-approach` selects the already-active
physical attack and approach capability. Its source mapping remains commandset06 with physical
TargetPriorityScript3; authored policy does not expose either source ID. It requires a regular `physical` definition, an empty spellbook and MOV 1–63;
status and items remain empty. Encounter rewards are required when the physical action executes.
Living opposing targets must be reachable through legal radius-one attack positions. Candidate
scoring follows reverse processing order; signed raw-priority cohorts are selected before the returned
priority is capped at15. Critical multi-target cohorts use the Regular class table, then the source
largest-movement/later-collected tie rule. Zero attack targets return ATTACK1 failure, followed by
unavailable HEAL1/SUPPORT and MOVE1. Complete raw target costs0–127 select the first lowest-cost living
opponent in processing order. Source preliminary movement uses fixed cost4, then the legal MOV grid and
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

## Private Initialized Common Battle

[`PrivateBattleScenarioReader`](../src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)
reads six explicit absolute paths. `SF2_PRIVATE_BATTLE01_DATA`, `SF2_PRIVATE_BATTLE01_SCENE` and
`SF2_PRIVATE_BATTLE01_TERRAIN` retain the existing encounter trust boundary. `SF2_PRIVATE_STATIC_DATA`
and `SF2_PRIVATE_ENEMY_DATA` select the existing pinned static-data and enemy-promotion exports.
Their bytes must match the authoritative extraction manifests and their source provenance must match
the pinned upstream revision. The controlled JSON path comes from `--private-battle-start`; tests select
it through `SF2_PRIVATE_CONTROLLED_START`. Partial/missing selections, drift or conflicting profiles
reject visibly without fallback or private path leakage. Files are read-only; no active snapshot is imported.

The external [PlayerReady comparison input](../reference/inputs/battle01-player-ready.json) is a closed
format1 object. It declares party class/level, effective equipped stats, current/max HP/MP, four packed
item words and spell slots, status, independent seeds, nullable accounting and the controlled policy.
All keys are required; null EXP/kills/defeats/gold means Unknown, never zero. Supported living/status0
allies have already refreshed/equipped stats, so the initializer restores HP/MP without adding equipment
bonuses again. Source GIZMO ATT7 becomes effective8 once at difficulty0. Source class0/4/1, items,
HEAL/EGRESS definitions, orders, source words and unknown bytes remain available to their consumers.
Missing or incompatible definitions reject; unimplemented actions are not erased from the spellbook.

The policy explicitly skips the intro, uses roster-only storage, disables ally auto-battle/opponent
control, and supplies word0 only for the reached player candidate whose ally activation word is absent.
This is a controlled input, not an original global default. `mainSeed`4660 and `thinkingSeed`305397760
encode independent source words: the thinking high word carries source seed-copy0x1234 and its low
word is explicit representation padding. Neither RNG is reseeded during play. H3's non-natural R2a→R2b
bridge remains provenance, not a claim that natural Map3 entry skips its actual programs.

Regular/healer costs retain the authored table. Centaur uses cost5 on rough/deep and3 on brush;
hovering uses cost2 on every admitted non-barrier surface and land multiplier256. Private source
terrain values are interpreted only at Content admission; the original full byte grid is retained.
The common weighted kernel, movement and cancel commands own play. Reached SourceOrders, broader
spawn/status/region programs, equipment actions and outcome/return remain Unsupported or future
bindings; the explicit legacy route keeps its actual unported consumers.
