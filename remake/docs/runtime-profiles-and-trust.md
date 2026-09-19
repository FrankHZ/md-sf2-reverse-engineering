# Runtime Profiles and Trust

## Purpose

Runtime profiles declare where content comes from, which trust checks are required, and which product
claims are permitted. Profile selection changes outer composition; it does not fork Domain rules or
make Godot an evidence owner. Authored packages, the initialized private battle entry and the connected
private world all use the common session. The divergent public-synthetic/private-local readers and
session paths identified by the [architecture audit](./architecture-audit.md) were retired at
[M5](../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m5-implementation).
Private source trust checks are not universal gameplay predicates. The accepted 8C/H4 target remains
incomplete.

## Profile Summary

| Profile | Selection | Admitted inputs | Current claim |
| --- | --- | --- | --- |
| `public-authored` | default local start or `--authored-package <path>` | validated immutable battle/actor/rule definitions plus separate explicit controlled start input | implemented semantic movement, HEAL/STAY and ordinary physical first/second/counter subset; explicit authored starting vitals, no original-start/fidelity/export claim |
| `private-local-controlled-start` | `--private-battle-start <absolute controlled JSON>` and six explicit input environment selections, or `--private-exploration-start <absolute start JSON>` with the same six plus `SF2_PRIVATE_EXPLORATION_CONTENT` and `SF2_PRIVATE_CONTROLLED_START` | selected encounter, pinned enemy/static/gold definitions, external controlled party/start; for exploration also the prepared private world | initialized common session through inactive standby, activation, set6/set7 pursuit and ordinary enemy/player/kill/HEAL actions; the connected world runs the Map 3 opening through Battle 01 outcome and return. Required unknown accounting and wider effects remain explicit boundaries |

The runtime always displays the appropriate disclosure:

- `AUTHORED BATTLE` with controlled-start help
- `PRIVATE CONTROLLED BATTLE` with Unknown accounting shown explicitly

The ordinary project is `remake/game`, whose GameRoot accepts only `--authored-package <path>`,
`--private-battle-start <path>` or `--private-exploration-start <path>`, once and mutually exclusively. No arguments selects the authored yard.
Unknown or positional arguments, duplicate/conflicting options and missing paths return explicit
startup ContentError without creating a session or switching routes. The legacy `public-synthetic` and
`private-local` profile options were retired with the reference host at M5. Diagnostic environment
selections belong only to the external
[observer](./development-and-verification.md#ordinary-host-startup); they are not game
options and do not change ordinary routing. Runtime state and both RNG channels still begin only
through the same Content/Application entry.

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
only; color does not grant passability or damage protection. Private source movers keep their own
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
decoded solely by private Content;255 is a valid authored processing order, not a missing actor.
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
Packed `prowess` is no longer an authored field; private source definitions retain it.
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
selection. The Flying priority table serves private hovering movers only; it does not enable flying authored actors.

## Retired Legacy Profiles

The `public-synthetic` profile (a tracked project-authored Map 3 package with export smoke) and the
legacy `private-local` profile (canonical-import traversal, project-authored base view and battle
bridge, local HUD preview) belonged to the reference host retired at
[M5](../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m5-implementation).
Their readers, receipts and options no longer exist. The tracked authored packages remain the
redistribution-safe public content.

## Private Admission Layers

Private admission has an offline preparation layer and a runtime Content layer:

1. `sf2tool.remake_exploration_content` verifies the registered canonical import digest and the pinned
   source checkout, rejects local changes to every selected source, and writes an ignored world with
   repository/commit/ROM identity and per-source SHA-256. With `--rom-path` and `--presentation-root`
   it also verifies the ROM identity and the pinned presentation manifest before embedding map atlases,
   sprites and portraits as hashed rasters.
2. At startup, Content checks the battle exports and terrain against fixed digests, requires the world's
   repository and commit to match the encounter source and its ROM identity to match the enemy-gold
   export, validates every raster digest and shape and every required sprite/portrait link, and then
   admits the separate controlled start.

Content authenticates actual bytes. Application validates the typed protocol and compatibility between
accepted results; it does not claim cryptographic protection against a malicious in-process port that
fabricates bytes behind an otherwise exact typed result.

## Payload and Path Boundary

The following remain ignored and local:

- ROMs, canonical imports, source metadata exports, decoded graphics, palettes, and captures;
- official engine archives, export templates, and other downloaded executables; and
- generated Godot import state, builds, exports, logs, and smoke receipts.

Private compressed source bytes and extraction metadata remain offline. The runtime reads only the
selected exports, the prepared world and the controlled start; none is committed, printed, embedded in
a public package, or exported by the current private runtime. Diagnostics are typed and path-free.

Use the repository's [Local Private Input Layout](../../docs/operations/local-private-inputs.md) for
machine-private input routing. Worktree-local writable state remains isolated even when immutable shared
inputs are registered centrally.

## Fidelity Boundary

The private profile executes selected original programs and battle rules through the common session.
Fades, mosaic, shiver, camera scrolling, join music and nod rendering are performed modern services
over prepared rasters; they are not an original screenshot or a fidelity backend. The profile does not
claim:

- natural Map 3 reach before the controlled start or natural continuity across the H3 bridge;
- original camera, layer/priority composition, animation cadence, text layout, audio or final pixels;
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
reads seven explicit absolute paths. `SF2_PRIVATE_BATTLE01_DATA`, `SF2_PRIVATE_BATTLE01_SCENE` and
`SF2_PRIVATE_BATTLE01_TERRAIN` retain the existing encounter trust boundary. `SF2_PRIVATE_STATIC_DATA`
and `SF2_PRIVATE_ENEMY_DATA` select the existing pinned static-data and enemy-promotion exports.
`SF2_PRIVATE_ENEMY_GOLD` selects the existing enemy-gold export, whose used enemy IDs and ROM addresses
remain distinct from its unused tail. The extraction manifest owns its digest; it is never a public fixture input.
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
hovering uses cost2 on every admitted non-barrier surface. Its land multiplier still follows terrain
protection (source0/1 give256/230), independently of its airborne dodge and Flying AI priority. Private source
terrain values are interpreted only at Content admission; the original full byte grid is retained.
The common weighted kernel, movement and cancel commands own play. SourceOrders preserves each
immutable deployment anchor, both decoded source orders, current memory/activation word, last target
and both RNG channels. Inactive standby uses source terrain and evolving occupancy; active set7
retains failed MOVE_ORDER1 before the no-action pursuit shared with set6. Content resolves source
NONE as255 and leaves unresolved order expressions explicit. Nonempty move-order programs, wider
status/neutral occupancy and nonempty enemy action categories stop before partial publication.
Selected class prowess, adjacent ATT-only weapons, unpromoted HEAL1–3 and enemy gold now bind to common
actions, death/rewards and one no-effect after-turn pass. The external
[action input](../reference/inputs/battle01-actions.json) supplies controlled initial accounting; the
PlayerReady input retains Unknown values and rejects only operations that need them. No source bonus
is applied twice, no live accounting is filled, and unsupported spell references stay visible. The
connected world adds Battle01 growth and outcome programs; broader spawn/region programs and other
outcome families remain future work.
