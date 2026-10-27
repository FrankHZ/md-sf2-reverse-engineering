# Runtime Profiles and Trust

Content owns external bytes, paths, source identity, closed formats and validated immutable
definitions. Profiles select admission and permitted claims; they do not fork gameplay authority.
See [architecture](../architecture.md), [rule composition](../domain/gameplay-rules.md) and
[asset preparation](presentation-assets.md).

## Authored leader cleanup binding

Only a v8 authored world with a nested v7 encounter and a valid outcome route admits:

```json
"leaderCleanup": {
  "bowie": "leader",
  "enemySlots": [
    {"actor": "enemy", "slot": 128},
    {"actor": "survivor", "slot": 129}
  ]
}
```

`LeaderCleanupReader` requires the sole allied physical leader as Bowie, every deployed enemy
mapped exactly once, unique source slots128–159 and a slot128 gate. It copies these identities
into immutable `LeaderCleanupDefinition`; input order, processing order, physical leader flags
on enemies and Taros enemy-definition mappings do not select the gate or sweep ordering.
Growth/outcome definition copies retain the binding. A standalone v7 cleanup, missing outcome
route or nonempty defeated program rejects. The explicit binding is the only relaxation of the
authored enemy-leader guard; private leader encounters remain guarded by source admission.

[leader-cleanup.json](../../content/authored/leader-cleanup.json) uses existing authored map/text/
return machinery, an ordinary one-HP gate and a living survivor. It supplies no original assets,
full Battle18, loot/Deals, Caravan aftermath or original natural-start claim. Controlled inputs
begin without a previous scene's first ally; a no-scene cleanup requiring that absent custody is
Unsupported rather than defaulted. The [rule owner](../domain/gameplay-rules.md#finite-enemy-leader-cleanup)
and [continuation owner](../application/session-and-programs.md#ordered-leader-cleanup-and-retained-accounting)
define supported runtime semantics.

## Authored Taros identity binding

A format7 encounter may include the closed optional binding:

```json
"tarosEffectiveness": {
  "bowie": "champion",
  "enemyDefinitions": [
    {"actor": "guardian", "definition": 88},
    {"actor": "reserve", "definition": 9}
  ],
  "weaponItems": [61]
}
```

`TarosEffectivenessReader` resolves the bound ally and enemy `ActorRef`s in that encounter, rejects
duplicate enemy mappings/weapon identities, requires a Taros88 definition and weapon61, and copies
the mapping into immutable `BattleDefinition.TarosEffectiveness`. Unmapped actors do not acquire a
Taros definition. Numeric definitions are explicit source-policy identities; no slot/name/fixture
string implies them. This opt-in binding represents the selected Battle18 gate in authored content;
it does not widen private Battle18 admission. Region/spawn/nonempty defeated-program guards
remain in force; authored leader cleanup requires its separate explicit binding above.

`weaponItems` explicitly classifies packed loadout identities as weapons for the source getter.
The union admits those carried identities during actor decoding; each bound encounter must include
the package's admitted weapon identities so the first-equipped scan has complete weapon types.
A weapon cannot also be a healing/retreat item. No automatic equip-class or ATT modifier is inferred:
authored statistics are controlled inputs, and normal stat refresh/equipment services remain out of
scope. Live `SourceLoadout` at action construction overrides the immutable starting items.
Encounter copies made by growth/outcome binding retain the same immutable effectiveness definition;
existing authored equipped-growth limitations remain explicit.

[taros-effectiveness.json](../../content/authored/taros-effectiveness.json) supplies a small authored
practice encounter. Both allies carry equipped61 but only `champion` binds Bowie; `guardian` binds88
independently of processing order. Enemy AI is Stay and no source Battle18 leader/drop/aftermath is
claimed. The [rule owner](../domain/gameplay-rules.md#finite-taros-physical-effectiveness) and
[native recipe](../development-and-verification.md#taros-authored-observation) own behavior and delivery.

## Purpose

Runtime profiles declare where content comes from, which trust checks are required, and which product
claims are permitted. Profile selection changes outer composition; it does not fork Domain rules or
make Godot an evidence owner. Authored packages, the initialized private battle entry and the connected
private world all use the common session. The divergent public-synthetic/private-local readers and
session paths identified by the [architecture audit](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1c4c786a6e2c630e1cfadf4daa88dbd7687caa19/remake/docs/architecture-audit.md) were retired at
[M5](../../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m5-implementation).
Private source trust checks are not universal gameplay predicates. The [composed private milestone](../../../docs/design/synthesis/map3-battle01-readiness.md#accepted-current-milestone)
is accepted at its stated modern-clock/keyboard-A/8D boundary; full hardware 8C parity remains Unknown.

## Profile Summary

| Profile | Selection | Admitted inputs | Current claim |
| --- | --- | --- | --- |
| `public-authored` | default local start or `--authored-package <path>` | validated immutable battle/actor/rule definitions plus separate explicit controlled start input | implemented semantic movement, HEAL/STAY and ordinary physical first/second/counter subset; explicit authored starting vitals, no original-start/fidelity/export claim |
| `private-local-controlled-start` | `--private-battle-start <absolute controlled JSON>` and six explicit input environment selections, or `--private-exploration-start <absolute start JSON>` with the same six plus `SF2_PRIVATE_EXPLORATION_CONTENT` and `SF2_PRIVATE_CONTROLLED_START` | selected encounter, pinned enemy/static/gold definitions, external controlled party/start; for exploration also the prepared private world | initialized common session through inactive standby, activation, set6/set7 pursuit and ordinary enemy/player/kill/HEAL actions; the connected world runs the Map 3 opening through Battle 01 outcome and return. Required unknown accounting and wider effects remain explicit boundaries |

The runtime always displays the appropriate disclosure:

- `AUTHORED BATTLE` with controlled-start help
- `PRIVATE CONTROLLED BATTLE` with Unknown accounting shown explicitly

The ordinary project is `remake/game`, whose GameRoot accepts `--authored-package <path>`,
`--private-battle-start <path>` or `--private-exploration-start <path>`, once and mutually exclusively,
plus optional `--input-settings <path>`. With no session-entry option it selects the authored yard.
Unknown or positional arguments, duplicate/conflicting options and missing paths return explicit
startup ContentError without creating a session or switching routes. The legacy `public-synthetic` and
`private-local` profile options were retired with the reference host at M5. Diagnostic environment
selections belong only to the external
[observer](../verification/host.md#ordinary-host-startup); they are not game
options and do not change ordinary routing. Runtime state and both RNG channels still begin only
through the same Content/Application entry.

### Authored growth and ordinary outcomes

The existing v8 `{package,battle,world,start}` envelope (nested battle v7) admits optional
`world.growth` through a distinct authored binding. Each row has `actor`, matching source `classId`,
five `stats` rows in HP/MP/base ATT/DEF/AGI order and empty `spells`. Each stat has `start`, `projected`
and `curve`: either empty or29 `[cumulative,increment]` pairs whose deltas agree and end at256.
Every deployment of that actor must be allied, with a supported unpromoted named class, no equipped
item and zero inferred equipment bonus. Authored spell-learning schedules/equipment are explicitly
unsupported; known spells are preserved. Existing private growth keeps its private-definition guard
and admitted source equipment/spell mappings.

An authored `world.maps[].battle.outcome` uses the existing `after`, `defeated`, `defeat`, `return`,
`joinMember`, `victoryFacing` and `egress` fields. It requires exactly one allied physical leader,
an enemy roster without enemy leaders unless explicitly bound above, the existing no-op defeated
program, valid program references,
join membership within `partyFlags`, and traversable in-bounds egress. Private source/provenance
checks remain on the private entry; this authored entry cannot relabel a private package.
Nonempty defeated programs, unbound enemy-leader outcomes and wider egress/status behavior are not admitted.

[`progression-outcome-victory`](../../content/authored/progression-outcome-victory.json) and
[`progression-outcome-defeat`](../../content/authored/progression-outcome-defeat.json) are small authored
examples. Ordinary interaction, before dialogue, one battle action and real after/defeat/return
programs exercise the same session and presenter. Their curves, entities and text are project-authored.
They establish neither original natural continuity nor a successful private outcome observation.
Algorithm selection remains compiled C# composition, separate from this content/trust admission.

### Story content and source policy selection

The public v8 story examples [A](../../content/authored/story-rule-demo-a.json) and
[B](../../content/authored/story-rule-demo-b.json) reuse ordinary typed branch/call/text/tick/return
operations and the existing reader. All call/jump/branch references are checked at admission,
including untaken targets. Their text, flags and entity effects are project-authored and grant no
source-fidelity or private distribution claim.

The Content profile and the compiled rule composition are separate selections. `ForGame()` and
direct default starts retain `RuleCompositions.Sf2()`; they can consume ordinary authored programs.
Every explicitly authored factory, including `AuthoredStory()`, selects `AuthoredStoryPolicy`,
which reports Unsupported for any reached `SourceStoryInstruction`. The SF2 composition selects
the named source policy and enforces the exact existing scratch context/retirement guards. The
reader maps the unchanged `retired-map3-entity-scratch` opcode to its typed source instruction;
it does not select the policy or execute a handler registry. The accepted private source entry
continues to require its own provenance/resources and never selects an authored fallback.

This is trusted project source composition, not a security sandbox for arbitrary mods. A public
file does not prove source reach by choosing source-like IDs or flags; constructed source-context
tests are explicitly SF2-policy tests. No new package format, source capability, algorithm-name
loader or live policy switch is admitted. See the [author workflow](../domain/gameplay-rules.md#rule-author-workflow-and-diagnostics).

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
full-recovery remains unsupported; level-up requires the admitted v8 growth described below. The session reads no files/ROM after admission and
carries its seeds forward. The default loads the tracked yard; `--authored-package <path>` selects
another bounded package, without an authored-export or original-fidelity claim. Further private AI/action migration follows the
[ordered boundary](../../../docs/decisions/0019-state-and-content-driven-remake-engine.md#authored-definitions-and-explicit-session-starts).

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
increments capped defeats; enemy death credits one kill/gold award. Missing required growth or an
unadmitted terminal outcome returns Unsupported before preparation publishes. The two physical configurations
[`stone-court`](../../content/authored/stone-court.json) and [`river-post`](../../content/authored/river-post.json)
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
[M5](../../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m5-implementation).
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

Use the repository's [Local Private Input Layout](../../../docs/operations/local-private-inputs.md) for
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
- full executable H4 report or hardware-parity acceptance. The composed milestone has its separate
  [accepted boundary](../../../docs/design/synthesis/map3-battle01-readiness.md#accepted-current-milestone).

An absent capability remains Unsupported or Unknown at its owning contract. Private bytes alone never
authorize a fidelity claim.

## Redistribution

Tracked code, metadata, tests, and project-authored synthetic content may pass the public build gates.
Original or caller-provided private payloads must not enter Git, a public PCK, CI artifacts, logs, pull
request attachments, or release downloads. A distributable replacement-asset strategy requires a
separate accepted rights and content decision.

## Private Initialized Common Battle

[`PrivateBattleScenarioReader`](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)
reads seven explicit absolute paths. `SF2_PRIVATE_BATTLE01_DATA`, `SF2_PRIVATE_BATTLE01_SCENE` and
`SF2_PRIVATE_BATTLE01_TERRAIN` retain the existing encounter trust boundary. `SF2_PRIVATE_STATIC_DATA`
and `SF2_PRIVATE_ENEMY_DATA` select the existing pinned static-data and enemy-promotion exports.
`SF2_PRIVATE_ENEMY_GOLD` selects the existing enemy-gold export, whose used enemy IDs and ROM addresses
remain distinct from its unused tail. The extraction manifest owns its digest; it is never a public fixture input.
Their bytes must match the authoritative extraction manifests and their source provenance must match
the pinned upstream revision. The controlled JSON path comes from `--private-battle-start`; tests select
it through `SF2_PRIVATE_CONTROLLED_START`. Partial/missing selections, drift or conflicting profiles
reject visibly without fallback or private path leakage. Files are read-only; no active snapshot is imported.

The external [PlayerReady comparison input](../../reference/inputs/battle01-player-ready.json) is a closed
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
[action input](../../reference/inputs/battle01-actions.json) supplies controlled initial accounting; the
PlayerReady input retains Unknown values and rejects only operations that need them. No source bonus
is applied twice, no live accounting is filled, and unsupported spell references stay visible. The
connected world adds Battle01 growth and outcome programs; broader spawn/region programs and other
outcome families remain future work.


## Selected second encounter

The private world may declare at most one additional `encounters` row. Its `placement`, `scene`,
`terrain`, `static`, `enemies`, `gold`, `start` and optional `battleScenes` paths resolve beneath the
world file's directory. Admission reads these once through the existing pinned readers, rejects
repeated encounter IDs or incompatible shared ally baselines, and retains immutable per-encounter
starts/scenes. The core/enemy/gold exports retain their exact accepted identities; selected records
are merged from those authenticated inputs. Production never reads comparison fixtures.

The Battle01-to-Battle02 selection imports seven maps and the source setup variants needed for
501/609. The maintained visual producer gathers enemy map sprites from the selected encounter
spritesets and enemy sprite table; the runtime rejects missing battlefield sprite references at
startup. It selects the active encounter's scene resources and map music, while live party resources,
progress, inventory, gold and RNG cross the transition. A newly joined member obtains its admitted
initial resources once; an already present member keeps its current values. The separately declared
Battle02 party input is an admission baseline, not a second session or a runtime reseed.

The world file is limited to 128 MiB (134,217,728 bytes), checked before full allocation and after
reading. Decoded PCM/rasters, JSON buffers, renderer objects and imports require additional memory;
this bound is not a total-memory promise. The measured selection and native load are recorded in the
[implementation agreement](../capability-status.md#battle01-return-through-battle02-implementation-agreement).

Explicit `--audio-candidate` inputs use the existing WAV/PCM admission format and byte/format checks.
They do not promote assets into the shared library. Captured finite cues retain exact recording,
sample interval and timer provenance. MainTheme1 uses unchanged existing PCM with an inferred modern
loop. `sf2tool.remake_authored_audio` prepares the prior project-authored defeat chord as a distinctly
labeled local candidate; command12/C4 selects the source command context, not original audio fidelity.
No private waveforms or generated content are checked in.

`battle01-to-battle02-programs.json` declares `outcomeBattles: [1]`. Battle02 resolution is deliberately
unbound and reports the existing typed `outcome-route` boundary; it cannot silently enter later story.

## Selected third encounter

The [Yeel selection](../../reference/inputs/yeel-to-battle03-programs.json) imports only Map5/66 and
Battle02/03. The paired [start](../../reference/inputs/yeel-to-battle03-start.json) and
[party](../../reference/inputs/yeel-to-battle03-party.json) are explicit INITIAL remake inputs derived
from the accepted Battle02 return, not proof of natural original reach. Growth is represented by the
matching grown baseline at admission. Later transitions carry actual party resources/accounting and
both seeds; no reference terminal state is injected. Source new-battle healing remains intentional.

Battle03 placement admits only actual controlled allies, retaining twelve formation rows as capacity.
Its six enemies use pinned source definitions. Huge Bat's Flying/sleep prowess is bound to real common
rules; its placement item `$6000` passes through source AddItem's `$807F` mask to ordinary Herb0.
The source action metadata remains distinct from runtime inventory. Private encounter pins retain
placement span1782772–1783004 and compressed terrain span1758636–1758970 (end exclusive), the
selected export/source/ROM identities, and the same closed private path boundary. Field-death visual
capacity does not create combatants. The selected bat scene admits sprite23/palette0, unarmed frames
and text306/338/354; missing required scene content rejects before play.

Optional selection `audioCues` is a unique nonempty list of exact cue identities. Omission retains
existing all-cue preparation; unknown/missing/duplicate selections reject. Selected assets keep their
existing root, length, format, PCM and provenance checks. This is finite content selection, not a
cache or a weaker reader. The selected 94,006,868-byte world fits the unchanged128MiB ceiling.
Boost109/C2 is a separately labeled project-authored 240ms notification, generated before startup
by `sf2tool.remake_authored_audio --cue encounter`. Its three tones and soft edges are not an original
audio reconstruction. Source109 replaces the admitted YM5 whole-clip group; unknown commands remain
rejected and real playback/Finished receipts retain both recording and requested timer contexts.

Map5 source warp rows containing coordinate64 are retained as frontier events with raw byte operands;
they are not clamped into traversable space. Source comments call these two rows unusable; admission
of a frontier does not establish their universal natural unreachability. Other reached unsupported
warps still fail explicitly. Full village services, later setups, other ailments, Battle03 outcomes,
EGRESS gameplay and original timing remain outside this selected capability.
