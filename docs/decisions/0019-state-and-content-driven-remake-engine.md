# ADR 0019: State- and Content-Driven Remake Engine

- Status: **Accepted direction**; M0/M1 and bounded M2 ordinary physical follow-ups implemented; remaining M2–M5 work is bounded below
- Proposal date: 2026-09-13
- Scope: runtime authority, content admission, program execution, verification, and incremental migration
- Accepted evidence base: `41be8d415322769d4f81cef77998fe35707a3e5e`
- Problem owner: [architecture and verification audit](../../remake/docs/architecture-audit.md)

## Direction and implementation boundary

Keep the four-assembly deterministic modular monolith and one `GameSession` facade. Make gameplay
admission a function of the live state, validated content, and explicitly implemented rule capability.
Move fixed walkthrough histories into reference runners. Use typed resumable programs for story
effects and let Application advance exploration, programs, battles, and return flow until actual
player input, a declared presentation/tick boundary, or an attributed failure is required.

This is a behavioral decoupling direction. Merely moving existing guards into smaller files would
leave the audited problem intact. Conversely, removing every guard would discard valid rules,
atomicity, and unsupported boundaries. The migration below separates these cases before changing them.

The user adopted this direction and authorized M0 and the bounded M1 slice. Except for the current M0/M1 boundaries below,
interfaces, normalized records, authored examples, and candidate filenames remain **Proposed
design**, not findings about the original or claims of implemented support. **Confirmed** and
**Unknown** in the evidence table retain the repository evidence meanings. Audit A1–A8 remain open.
Each implementation slice requires declared scope and independent main-gate review; M1 follows
M0 acceptance rather than continuing the old receipt queue.

The user's testing requirement is part of this design: new automated tests cover meaningful new-engine
behavior through small unit tests. Reference replay, probes, comparisons, fixture drivers, gates,
planners, reports, and test helpers are already verification; do not add tests of those verification
programs or a second validation framework. Audit and retire or migrate old tests and CI with the
engine. Keeping every old test unchanged and green is not a migration objective. Original research
evidence and completed results remain durable without requiring those old tests to run forever.

## Responsibility and reference migration boundary

New paths must use actual responsibility directories, and `GameSession` must remain a thin public
entry, lifecycle owner and sole state publisher. Command dispatch, battle advancement and projection
use independent internal collaborators rather than additional giant partial files. Do not use empty
folders, one-class-one-folder rules, line-count targets or architecture/reflection tests as acceptance.

M1 isolates the old scenario-bound classes in an independent
[reference area](../../remake/reference/README.md). Production Domain/Application cannot depend on
reference data or code. Original map, actor, encounter and event definitions belong in Content-loaded
JSON or suitable configuration with generic parsing/validation, including source provenance and real
special rules. Controlled party/start presets, comparison IDs, receipt counts, expected rounds/kill
orders, scripted verification inputs and expected terminals belong in reference verification.

The reference inventory distinguishes these responsibilities and names current consumers. M2 migrates
battle definitions and actual rules; M3 migrates map definitions and executed programs. Delete the old
class/data with its last caller, and retain only useful external comparisons through the engine's public
entry. Never delete required story behavior as validation data or replace program execution with a
terminal assignment. No permanent parallel engine is accepted; M5 handles remaining cleanup only.
M1 itself must admit two supported authored configurations without new production scenario classes,
case branches or 0009 trace fields. Direct dependency inspection and behavior tests establish that
boundary; relocating classes inside Application would not establish it.

## Current M0 implementation

Domain now has small internal [BattleRandom](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleRandom.cs),
[HealingRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/HealingRules.cs),
[TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs), and
[BattleRange](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleRange.cs) rules. Existing Battle01
healing and first-round consumers call them. Their inputs contain no character, round, receipt or
route predicates. Main RNG preserves the low word; turn ordering retains the original slot domain,
word arithmetic, signed comparison, sentinel participation and fixed buffer capacity.

Healing is the scalar ordinary same-side, EXP-eligible priest rule after class and power selection.
`AdjustedPower` is already normalized ordinary power, not a raw spell byte or full-recovery sentinel.
The rule caps recovery, spends the supplied MP cost and computes the two-draw EXP award; it rejects
level-up without publishing state. It does not select spells, classes, targets or sessions. Full-HP
scalar arithmetic is covered, while the existing reference wrapper still admits only an injured
target. Manhattan action range is shared; M1 adds movement and selection/cancellation on the common path.

The ordinary [Engine.Tests project](../../remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj)
references production Domain, Application and Content for actual M1 behavior. Useful RNG, turn-order
and healing arithmetic assertions moved from the legacy Domain tests. Five pure Python structure,
planner, environment, Godot-gate and probe test files retired. Mixed native-harness and asset tests
remain under their existing owners, outside new-engine CI. Original research fixtures, private-input
protections and controlled-route observations remain intact; the legacy wrappers still retain their
unmigrated trace guards. M0 does not implement the common session/content path or complete A1–A8.

The local engine/adapter entries and scoped public workflow below are implemented. Required-check
configuration is a main-gate integration action, verified against the actual candidate CI run.
Independent main-gate review remains required for each implemented slice.

## Current M1 implementation

The [authored reader](../../remake/src/Sf2.Remake.Content/Scenarios/AuthoredScenarioPackageReader.cs)
loads either [practice yard](../../remake/content/authored/practice-yard.json) or
[garden watch](../../remake/content/authored/garden-watch.json) into defensively owned typed definitions.
The packages supply different maps, encounters, actor/slot identities, placements, vitals, stats and
spell references. No package identity, digest, character, receipt or round predicate admits gameplay.
`initialVitals: authored-controlled` explicitly permits authored HP deficits; this is not the original
new-battle initialization policy. The separate private encounter reader imports source data; common
private initialization and play remain unsupported until their M2/M3 dependencies are implemented.

The plain [GameSession](../../remake/src/Sf2.Remake.Application/Runtime/GameSession.cs) reads its source
once, owns the session/revision envelope and publishes one immutable snapshot. The independent
[dispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleCommandDispatcher.cs) owns
selection and atomic action admission. The [advancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs)
owns dead-entry skipping, configured Stay AI, sentinel/new-round generation and next player control.
Automatic work yields `SimulationWait` after a bounded host tick and resumes through the same facade;
there is no player-facing AI/next-round control, RNG reset or arbitrary snapshot injection API.

Movement previews retain committed position until HEAL/STAY succeeds; cancel resets all provisional
choices without changing battle state or seeds. Weighted movement shares the extracted first-admission
cost algorithm. Authored logical maps prevent row-edge wrapping; the legacy wrapper explicitly retains
its flat storage probes for its source comparison. New path projection is an authored deterministic
policy, not an original presentation or natural-reach claim. Occupancy blocks opposing traversal and
all living occupied stopping cells; self-healing range uses the provisional destination.

MP/range/actor/stale-input failures return `IllegalCommand` with no state/RNG publication. Unsupported
spell effects, class/status/item/AI branches and reached level-up report `UnsupportedCapability`;
duplicate/dangling/malformed content reports `ContentError`. Physical attacks, general AI, rewards,
level-up, exploration/story programs, victory/defeat/return and original presentation remain outside M1.
The full invariant/adapter-fault lifecycle from A8 is not claimed complete.

The [Godot view](../../remake/game/src/Battles/BattleSessionView.cs) adapts actual keys to common
commands; its independent [projection](../../remake/game/src/Battles/BattlePresentation.cs) derives HUD
and actor markers from semantic results. No-argument local startup selects the yard package;
`--authored-package <path>` selects another supported authored package. Explicit old profile arguments
retain the temporary reference entry; a running session never switches authorities. Authored package
export/packaging is not yet claimed. The [reference inventory](../../remake/reference/README.md)
records actual legacy consumers, controlled-data ownership and M2/M3 removal points. Production project
dependencies do not include that assembly; Engine.Tests builds only Domain/Application/Content.

The authored adapter separates a clipped, automatically framed map from a scrollable HUD and responds
to actual viewport changes. Framing includes the acting origin, provisional path and selected/attempted
target; valid 48-wide maps and long 48×48 previews remain operable without changing Content admission.
Tab advances an ephemeral candidate cursor even after rejection. Only an accepted command changes the
snapshot's selected target; the HUD identifies rejected candidates and the unchanged accepted target.
These are adapter responsibilities, not another gameplay authority or new original-presentation claim.

The first four acceptance counterexamples below now have connected behavior coverage in
[EngineSessionTests](../../remake/tests/Sf2.Remake.Engine.Tests/EngineSessionTests.cs) and
[AuthoredScenarioTests](../../remake/tests/Sf2.Remake.Engine.Tests/AuthoredScenarioTests.cs).
Round-2 and round-5 histories carry their natural seeds; isolated M0 scalar expectations remain separate.
[Movement tests](../../remake/tests/Sf2.Remake.Engine.Tests/BattleMovementTests.cs) cover weighted cost,
budget, occupation and a full-width row boundary. The M1 dead-entry unit seam is complemented by the connected physical kill-order coverage below. The retained private reader's selected provenance
rejection still passes in its reference project; moving it does not weaken the trust boundary.

Reproduce with `uv run sf2 verify engine` and `uv run sf2 verify adapter` after the existing environment
setup. The [native no-image observation](../../remake/game/probes/engine_battle_observation.gd) was
executed directly under Godot 4.7.2 .NET for both packages: real movement/cancel/HEAL/STAY input,
ordered effects and carried seeds, automatic next control/round, projected HUD/node positions and
attributed range/unsupported failures passed without process errors. The
[verification owner](../../remake/docs/development-and-verification.md#authored-battle-observation)
contains exact commands. This establishes the bounded M1 path, not completion of A1–A8, original
Map 3/Battle 01 continuity, 8C or H4.

### Authored definitions and explicit session starts

The user-approved JSON model modernization precedes the remaining common private initialized-entry
plan. Its first implemented boundary separates reusable content definitions from one session's
controlled start. JSON remains the bounded package format; the four executable authored packages now
use `formatVersion: 2`. The reader accepts that current shape only, without parallel old/new models.

[`ScenarioDefinition`](../../remake/src/Sf2.Remake.Application/Content/Scenarios/ScenarioDefinition.cs)
contains the admitted encounter definitions from one package. Each Domain `BattleDefinition` owns
terrain, actor/rule/spell/reward definitions and ordered `BattleDeploymentDefinition` records. Those
records keep genuine encounter deployment coordinates; no definition stores `BattleActorState`,
current HP/MP/EXP, kills/defeats, initial gold or RNG seeds. Maximum vitals and enemy gold reward remain
actor definitions; experience halving remains an encounter rule.

[`BattleStartInput`](../../remake/src/Sf2.Remake.Domain/Battles/State/BattleStartInput.cs) selects one
encounter and supplies both seeds, current gold and an explicit keyed input for every deployed actor:
HP/MP/EXP, kills, defeats, supported status and nullable `positionOverride`. Null selects the encounter's
deployment; an override is controlled start data. Dead actors still require explicit records, retain
accounting and become unplaced runtime actors. Start-array order never replaces deployment/candidate
order. Missing counters are rejected instead of being supplied by physical/reward definitions.

The real Content reader returns definitions and start input separately in `ScenarioReadAccepted`.
`GameSession.Start(IScenarioSource)` consumes that pair through the same public
`GameSession.Start(ScenarioDefinition, BattleStartInput)` used to reuse admitted definitions with
another explicit start. The shared Domain start boundary validates encounter/actor joins,
duplicate/missing records, resource maxima/caps, supported status, live occupancy, terrain/bounds,
leader/outcome and turn-capacity limits before creating fresh runtime actors. Content performs that
same validation during read; typed reuse repeats it before publication, without a trusted-test bypass.
The thin facade, dispatcher, automatic advancer and single immutable runtime state authority remain.

**Confirmed (engine):**
[`BattleStartStateTests`](../../remake/tests/Sf2.Remake.Engine.Tests/BattleStartStateTests.cs) reuse the
same admitted definition for two different starts, including reordered keyed records, explicit
resources/accounting/seeds and a position override. Natural round generation and HEAL/STAY keep
independent existing RNG/resource expectations, separate session actors and unchanged definitions.
Another selected encounter resolves its own deployment from the same package. Invalid JSON and typed
starts fail at the actual binding boundary. Existing physical/AI/action tests retain their independent
integer, reward/death, ordering and whole-action failure expectations with the migrated input paths.
The [native start observations](../../remake/docs/development-and-verification.md#authored-start-state-observation)
use real input, existing nodes and complete state checkpoints across all four packages and a differing
controlled start; no screenshots, session setters or reference aggregate are required.

Remaining modernization is explicitly separate: faction versus stable order, agility versus extra
action, meaningful rule/capability profiles, control versus AI strategy, terrain semantic references
and original slot/raw mappings. Existing source semantics remain unchanged until their dedicated
migration. No global catalog, lazy loader, generic content manager, per-actor framework or schema
registry is introduced. Resource-on-demand loading needs an actual map/resource consumer. Only after
these bounded content slices does the private initialized-entry dependency chain resume; raw private
encounter expressions/provenance remain lossless, and unspecified private accounting remains Unknown.
M2, A1–A8 and natural8C/H4 are incomplete.

## Current M2 ordinary physical implementation

The M2 capability uses the common session for configured regular-ground physical actions.
[PhysicalBattleAction](../../remake/src/Sf2.Remake.Domain/Battles/Rules/PhysicalBattleAction.cs) validates
living opposing adjacent targets at the provisional destination, constructs at most first, second
and reversed counter hits on temporary HP, then publishes once. Shared
[PhysicalStrikeRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/PhysicalStrikeRules.cs) owns dodge,
integer land reduction, prowess-0/3 critical, counter halving before spread and each strike's natural
double/counter draws. The already-set counter toggle survives a failed draw after the second hit;
a new second-hit success can also request it. Target death cancels the follow-up. Counter-end draws
are consumed when their target survives but cannot dispatch another attack. Actor/target reversal
and each draw's seed images/range/value are explicit semantic observations, not scenario predicates.

[BattleRewards](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleRewards.cs) owns effective-level
damage/kill EXP, the per-action cap, configured halving, ordered two-roll award and gold/kill/defeat
caps. Damage EXP truncates per hit before accumulation. Enemy counter damage does not earn EXP for
the original player. A surviving ally who attacked receives one award, including an ally counter
during an enemy action. An enemy that dies to that counter still grants ally EXP/kill/gold. Enemy
strikes alone grant no ally EXP or award RNG; an ally killed before countering receives no award. No action, reaction or test resets the running seed.
Thinking RNG is unchanged by explicitly configured Stay AI; a counter does not consume the
counterattacker's queued ordinary turn.

Content accepts optional typed actor `physical` and encounter `rewards` definitions. The exact
[schema and capability boundary](../../remake/docs/runtime-profiles-and-trust.md#public-authored)
keeps effective stats explicit and admits no equipment or status branches. Optional authored
`physical.defeats` supplies 0–9999 prior defeats, defaulting to zero. Unsupported movetypes, prowess
and special rules reject at admission. The `stone-court` and `river-post` packages differ in actors,
map/positions, stats, targets, reward data and halving policy. Supported configuration variants use
the same reader/session; existing HEAL packages have no invented physical/reward defaults. These
are controlled authored inputs, not original private admission or source-fidelity starts.

One commit applies movement, ordered HP reactions and survivor rewards. Ordinary enemy death credits
one capped kill/gold award; ordinary ally death increments capped defeats. Dead
actors lose their battlefield position and occupancy, skip pending queue entries and are excluded
from later generated rounds. Application runs configured Stay or the bounded enemy branch below and returns the next living player.
Admitted status-free/equipment-free after-turn has no further resource or random effect, so both
faction checks have the same continuing result. Level-up, leader death/programs (including a dead
leader at admission), and terminal faction/outcome/return reject the whole action with the original
action-start state and no effects from that action. A failed automatic enemy action retains earlier
committed player/AI work and its own unconsumed queue entry; startup failure retains the generated
round. They are not converted to STAY or bypassed by seed retries. General
AI, other movement/prowess, equipment/status/special effects, private import, programs and M4 return
remain outside this capability; M2 as a whole and A1–A8 remain incomplete.

[Physical behavior tests](../../remake/tests/Sf2.Remake.Engine.Tests/PhysicalBattleTests.cs) assert both
legal kill orders, natural double/counter combinations, sticky and newly-set counter requests,
ignored post-counter decisions, a carried second-round counter, per-hit EXP truncation, lethal
second/counter cleanup and late Unsupported rollback. Independent numeric expectations cover changed
actor/configuration/history and actual common commands. Direct no-image Godot observation exercises
the affected input, source-ordered semantic reactions, resources, removed nodes and next control.
These establish bounded engine behavior, not original natural continuity, presentation, 8C or H4.

Original rule provenance remains the accepted [combat](../design/contracts/combat-resolution.md)
and [lifecycle](../design/contracts/battle-control-lifecycle.md) contracts and their research owners.
The implementation reads pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`
`battleactionsengine_1.asm`, `battleactionsengine_2.asm`, `attack.asm`, `determinedoubleandcounter.asm`,
`isabletocounterattack.asm`, `earnexp.asm`, `giveexpandgold.asm`, damage/dodge/critical routines and
critical definitions. Source structure establishes ordering/conditions; existing bounded runtime
seams do not prove arbitrary natural caller states. No new H3 execution or source evidence change
is implied by the remake checks.

The [reference inventory](../../remake/reference/README.md) names actual remaining private startup,
AI/counter/completion consumers and capability-linked removal points. Shared strike and reward
calculations replace duplicate bodies; the old `MainRoll` and duplicate EXP randomization are
deleted, while reference draw DTOs only project computed results. Remaining legacy profile/history
wrappers do not admit authored actions and cannot be claimed migrated before their private consumers
move to common commands. M2/M3/M4 remove their corresponding callers; cleanup is not postponed
wholesale to M5. Production projects remain independent of the reference assembly.

### Enemy physical decision and common action publication

[EnemyPhysicalDecision](../../remake/src/Sf2.Remake.Domain/Battles/Rules/EnemyPhysicalDecision.cs)
implements the already-active, physical-only ATTACK1 success branch with TargetPriorityScript3.
The typed `commandset06-script3` controller names this capability; activation, other commandsets and
spell/item categories remain Unsupported. Regular MOV 1–63 bounds candidate movement below the
source signed-grid 128 boundary. Candidate construction uses the existing weighted grid, blocks
opponents, permits traversal through friends and excludes occupied stopping cells. The radius-one
ring visits north, west, east, south, takes the first strictly lowest movement cost, and immediately
accepts the actor's cost-zero origin. Reachable living allies are stored by ascending slot, while
thinking calls visit that array backwards and store each priority at its original index. No actor
name, receipt, round, seed allowlist or expected target is a gameplay predicate. Zero reachable
attack targets return command failure to the bounded commandset continuation below.

[PhysicalTargetRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/PhysicalTargetRules.cs) owns
script3 byte scoring and final selection. Signed-byte maximum starts at zero; the highest **raw**
priority determines the cohort before the returned priority is capped at15. The cohort is collected
in reverse reachable-array order. A single highest target bypasses class/movement comparison.
Multiple highest targets with raw priority at least15 use the attacker's movement-type class table;
remaining ties select greatest signed movement, with later-collected targets winning equal movement.
Negative priorities or movement values do not become unsigned winners. Authored movement stays
within0–126; byte/signed edges in the shared scalar are tested without claiming their natural reach.
`ai-candidate` observations carry movement cost/raw score and target; `ai-target` carries raw/capped
priority and selected target. All candidate thinking draws retain their actor/target and seed links.

**Confirmed (static):** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`disasm/data/battles/global/aicommandsets.asm` commandset06 starts ATTACK1 and stops on success;
`disasm/code/gameflow/battle/ai/command/attack.asm` selects ordinary weaponless range and records
AI_LAST_TARGET_TABLE. `attack/prioritizetargets.asm` fills priorities backwards; TargetPriorityScript3
draws thinking range3, then chooses lethality16/1 or byte-doubled movement priority. The matching
`attack/determinebattleaction.asm` owns signed maximum, raw cohort, cap, class and movement selection.
`disasm/data/battles/global/aipriority.asm` maps regular movement to the Regular class table and
hovering/flying to Flying. SDMN/PRST/WARR rank0/5/25 in Regular; the actual retained reference's
SDMN/PRST/KNTE rank0/5/15 in Flying. `disasm/sf2enums.asm` supplies those class IDs. The consumed
[AI contract](../design/contracts/battle-ai-decision.md) retains wider category, activation and
runtime-evidence limits. Authored movement is a modern projection, not original move-string animation.

The existing `classRule` is the sole class selector. It admits `unpromoted-swordsman` (SDMN0),
`unpromoted-warrior` (WARR2), and existing `unpromoted-priest` (PRST4); `ordinary` leaves source class
unspecified. Named unpromoted classes cannot declare promoted physical EXP. No independent raw class,
priority rank or movement-table flag is supplied by content. Current regular physical admission
determines the Regular table; no additional movement capability is introduced. Missing class data
rejects only a reached critical multi-target cohort as `ai-target-class`; noncritical or single-highest
selection needs no class lookup. Every scored candidate requires its physical land-rule definition.
Unavailable required data or a late unsupported settlement rejects the entire enemy ACTION.

`BattleRandom.NextThinkingWord` owns the source byte-rejection calculation. In the authored uint
image the source seed-copy word occupies bits31–16; its high byte changes and other24 bits survive.
Script3 consumes one range3 call per reachable candidate; physical-only selection consumes no
category range6 draw. Last-target memory starts unspecified (`null`) and records only a committed
decision; script3 does not consult earlier last-target state. Reference thinking, script3 scoring
and final ranking now project these shared calculations. Its retained Flying table is a scalar
reference input, not authored flying/centaur admission. Private activation, commandset admission, move-string projection,
class-profile guards and startup/history consumers retain named migration points.

[BattleActionCommitter](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleActionCommitter.cs)
publishes player and enemy actions through one movement/effects/RNG/revision/queue mechanism.
The shared physical calculator settles ally rewards and deaths by actual side, including a counter
kill of the original enemy actor. All decision and physical effects remain temporary until the
entire ACTION succeeds; failure preserves earlier commits and the current enemy's queue entry.
`GameSession`, Application publication and C# Godot projection need no new target-selection route.

**Confirmed (engine):** [target selection tests](../../remake/tests/Sf2.Remake.Engine.Tests/TargetSelectionTests.cs)
use two Content packages, changed classes/positions/slots/configuration order, competing targets,
critical and noncritical cohorts, raw/capped and signed-byte boundaries, automatic continued turns,
natural RNG-driven target changes, common counter rewards and whole-action rejection. Existing
[enemy action tests](../../remake/tests/Sf2.Remake.Engine.Tests/EnemyActionTests.cs) retain death/queue,
startup and per-action failure boundaries. Direct native input observes class-driven selection,
farthest-movement ties, carried two-round history and missing-class Unsupported with complete
checkpoints and clean logs. **Unknown:** wider AI, private input migration, natural original
continuity, presentation and8C/H4. These engine checks do not complete M2 or A1–A8.

### Zero-target commandset06 continuation

[EnemyCommandset06](../../remake/src/Sf2.Remake.Domain/Battles/Rules/EnemyCommandset06.cs) owns the
fixed sequence ATTACK1 → HEAL1 → SUPPORT → MOVE1 → STAY for the admitted already-active,
physical-only, empty spellbook/item/status branch. ATTACK1 success stops the sequence. Without an
attack candidate, ATTACK1 and the unavailable HEAL1/SUPPORT return failure without consuming RNG.
MOVE1(mode0) returns success even when its movement result is origin Stay; the later table STAY is
therefore not reached in this subset. Successful MOVE1 ends this ACTION, and only a subsequent actual
turn reevaluates ATTACK1. Rewards data is required when an attack is reached, not for movement.

[AiMovementRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/AiMovementRules.cs) shares three
calculations with actual reference pursuit/standby/physical consumers: stable unsigned raw target-cost
selection, the source decreasing-cost walk retaining accumulated direction bits, and radius station
selection. MOVE1 builds an unblocked permanent-terrain grid with budget128 and scans living opponents
by slot. Every target cost must be present and within0–127; the first equal minimum wins. A target-rooted
grid drives preliminary movement to `max(0, startCost - 4)`. This is fixed movement cost4, not four
tiles or the whole actor MOV budget. The actor's MOV*2 grid blocks opponents and permits friend
traversal; radius0 then1 searches for an unoccupied stopping cell with strict lower-cost precedence
and immediate own-origin zero. No eligible station yields origin Stay, still a successful MOVE1.
The common movement commit and action publisher update position and consume one queue entry once.
HP/MP/EXP/gold/kill/defeat state, both RNG streams and last-target memory remain unchanged.

**Confirmed (static):** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`disasm/data/battles/global/aicommandsets.asm` set06; `code/gameflow/battle/ai/startaicontrol.asm`
stops its loop only on d1 byte0. Under that AI directory, `executeaicommand.asm` supplies MOVE1
parameter0, while `command/attack.asm`, `heal.asm` and `support.asm` return -1 on these empty branches.
`command/move.asm` selects the raw target, constructs the preliminary path and resolves its station,
returning0 even on empty path or destination failure. `code/gameflow/battle/battlefield/battlefieldengine.asm`
`GetMoveCostToEntity` returns cost in d0; MOVE later tests the enemy bit in that retained cost.
Complete costs below128 skip that disputed class pass. High, incomplete or unreachable target costs
remain `ai-move-target-domain` Unsupported; they are not replaced with nearest-target or Stay guesses.
The consumed [AI contract](../design/contracts/battle-ai-decision.md) preserves wider caller/class-pass
Unknowns. Reference source move-string projection and bounds16×20 remain comparison inputs; authored
logical movement does not claim natural source animation or route continuity.

**Confirmed (engine):** [continuation tests](../../remake/tests/Sf2.Remake.Engine.Tests/CommandsetContinuationTests.cs)
use the real Content/session path across both physical packages, enemy-first startup, changed target
positions, weighted costs, MOV/occupancy correction, origin Stay, missing reached rewards and atomic
unreachable rejection. Actual next-round input reaches physical attack/counter with independently
specified RNG and resource results. Existing reference pursuit/standby/physical methods consume the
shared rules. [Direct native observations](../../remake/docs/development-and-verification.md#commandset06-continuation-observation)
drive real input and check complete checkpoints, live state and clean logs. **Unknown:** wider AI,
initialized common private entry, natural original continuity/presentation and8C/H4. M2 and A1–A8 remain incomplete.

### Private battle admission dependency boundary

The M2 private migration follows the [current user direction](../../remake/docs/architecture-audit.md#current-user-direction)
and the [three content boundaries](#typed-content-and-capability-admission). **Confirmed (implementation
inspection):** `Map3Root` selects `PrivateOriginalBattle01StartupReader`; `PrivateBattle01Ui.Apply`
then calls the reference session's Prepare → Initialize → FirstRound → FirstControl chain. Preparation
requires an exact pending Map3 admission and a controlled party preset. Later UI branches select
standby, pursuit, physical and completion APIs from reference history. None calls
`Application.Runtime.GameSession.Start(IScenarioSource)`. That common entry currently consumes an
a reusable `BattleDefinition` plus explicit controlled `BattleStartInput`, starts turn generation
immediately without original new-battle initialization, and labels the authored read origin
`public-authored-controlled-start`. Changing that label cannot supply the missing source startup rules.

This dependency map describes the particular private Battle01 inputs; original facts remain owned
by the linked contracts/research, not by the reference implementation. Paths under `reference/` below
are relative to `remake/reference/Sf2.Remake.Reference/`; the
[reference inventory](../../remake/reference/README.md#private-startup-callers-and-removal-boundaries)
owns caller retirement.

| Dependency and current owner | Accepted behavior/data owner | Common-engine support, gap and intended owner |
| --- | --- | --- |
| Trust and parsing: [`PrivateBattleEncounterReader`](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), consumed by the retained reference startup reader | [Placement/scene provenance](../research/battle01-placement.md), [encounter definition](../design/contracts/battle-encounter-definition.md) | **Implemented:** production Content owns explicit local selection, bounded reads/Stack decode, registered export/input identities, pinned upstream commit, source/range joins, duplicate-property rejection and path-free failures. The legacy exact-nine/identity/scene projection checks remain in Reference. Existing digests select trusted private inputs; generic structural validity follows declared counts, references, bounds and raw-field domains. No new hash registry or runtime ROM extractor. |
| Encounter data: [`BattleEncounterDefinition`](../../remake/src/Sf2.Remake.Application/Content/Scenarios/BattleEncounterDefinition.cs), projected to the retained `OriginalBattle01StartupDefinition` | [Encounter definition](../design/contracts/battle-encounter-definition.md), [navigation](../design/contracts/battlefield-navigation.md) | **Implemented:** the immutable source definition retains the full48×48 terrain independently of the16×20 battle area, map/trigger/background/leader/halving metadata, ordered placements, identity/item/spawn words, both orders/regions, region vertices/unknown/trailing bytes and AI points. Common `BattleDefinition` has terrain/placements/rewards but no source deployment, region or program model. Content loads these source definitions; raw fields without implemented consumers remain explicit rather than being discarded or guessed. |
| Enemy baseline: `OriginalBattle01GizmoBaseline`; `Sessions/Battle01/PrivateOriginalBattle01Initialization.ProjectBattle01Initialization` | [Enemy definitions](../design/contracts/enemy-definition-data.md), [enemy data research](../research/enemy-promotions.md) | The three selected startup files do not contain this hardcoded baseline. Load the selected enemy record through the existing pinned enemy export owner before common new-battle initialization: source level/vitals/base stats/resistance/prowess/status/items/spells, movement6, AI word and unknown fields stay distinct from effective state. Common physical definitions do not represent that closure. Enemy gold is a separate data join, required when a reward is reached. |
| Party and control input: [`OriginalBattle01ControlledPartyPreset`](../../remake/reference/Sf2.Remake.Reference/Fixtures/Battle01/OriginalBattle01ControlledPartyPreset.cs), `Battle01FirstControlPreset` | [Player-control contract](../design/contracts/battle-functions-control-flow.md), [new-game boundary](../design/contracts/new-game-state-initialization.md), [item](../design/contracts/item-definition-data.md) and [spell data](../design/contracts/spell-definition-data.md) | These presets supply already refreshed effective ally stats, packed equipment/items/spells, optional accounting, independent main/thinking seeds and controlled policy. They are external comparison inputs, not original ally growth/new-game initialization. Common state currently requires EXP and defaults missing kills/defeats/gold to zero; private unspecified values must remain unspecified or be supplied explicitly as declared controlled inputs before use. Preserve class0/4/1, EGRESS/HEAL and equipped records; do not empty spellbooks/items to satisfy current admission. |
| New-battle initialization: [`Battle01Initialization.Initialize`](../../remake/reference/Sf2.Remake.Reference/Rules/Battles/Battle01Initialization.cs) | [New battle lifecycle](../design/contracts/battle-control-lifecycle.md#new-battle), [derived-stat research](../research/runtime-rng-and-battle-math.md) | Common starts copy authored current values. Production Domain still needs the admitted new-battle healing/status/derived-stat boundary, enemy spawn transformation, cleared AI memory/last targets/region state and deployment. The reference retains effective equipped ally ATT while restoring HP/MP and uses source GIZMO ATT7→8 once at difficulty0. Do not apply weapon bonuses twice or accept all difficulties from this sample. General refresh, other statuses and spawn/upgrade paths need their named rule coverage. |
| Round/control: [`Battle01FirstRound`](../../remake/reference/Sf2.Remake.Reference/Rules/Battles/Battle01FirstRound.cs), `Battle01FirstControl` | [Round/activation lifecycle](../design/contracts/battle-control-lifecycle.md), [turn-control join](../research/map3-battle01-turn-control.md) | Shared turn scoring/RNG already exists. Common `BattleTurnFlow` omits activation → region-program → spawn admission before generation; state lacks tested-region flags and per-actor activation words. Domain must own those transitions, with Application ordering them before input. Preserve the particular STARTING/no-region-cutscene branch and roster-only alias boundary. The external missing-ally-word0/control-mode policy is not a natural observation or a universal default. |
| Movement and AI: `Battle01MovementProfile`, `Battle01EnemyStandby`, `Battle01EnemyPursuit` | [Navigation](../design/contracts/battlefield-navigation.md), [AI](../design/contracts/battle-ai-decision.md) | Weighted propagation, direction-mask walk, station search, thinking RNG and script3 selection are shared. Common admission still fixes regular movement and active commandset06. Actual inputs need class1 Centaur2, class4 Healer12, enemy Hovering6, inactive standby memory/anchor behavior, then activation and set7's MOVE_ORDER1 failure path. Do not replace inactive enemies with authored Stay or set06. Production Domain rule/state collaborators consume Content-owned profiles; legacy fixed histories remain reference-only. |
| Actions and continuation: `Battle01PlayerHealing`, both physical rules, `Battle01TurnCompletion` and their session wrappers | [Spell resolution](../design/contracts/spell-resolution.md), [combat](../design/contracts/combat-resolution.md), [lifecycle](../design/contracts/battle-control-lifecycle.md) | Common HEAL and physical/reward math are available, but weapon/range/prowess/land/AI-class operands must come from the actual class/item definitions. Retain reached equipment/status/unsupported-spell semantics and nullable accounting boundaries. Source-ordered after-turn, leader/outcome/return/program work remains required as reached. Application keeps one action publisher; no receipt count or named character profile enters it. |

**Confirmed (implementation): lossless private encounter admission with an immediate existing
consumer.** [`PrivateBattleEncounterReader`](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs)
reads the registered `SF2_PRIVATE_BATTLE01_DATA`, `SF2_PRIVATE_BATTLE01_SCENE` and
`SF2_PRIVATE_BATTLE01_TERRAIN` selections unchanged and read-only. It owns file/trust/JSON admission
and returns the immutable Application
[`BattleEncounterDefinition`](../../remake/src/Sf2.Remake.Application/Content/Scenarios/BattleEncounterDefinition.cs).
The retained [`PrivateOriginalBattle01StartupReader.Admit`](../../remake/reference/Sf2.Remake.Reference/Readers/PrivateOriginalBattle01StartupReader.cs)
delegates to that real reader and projects only the old comparison DTO. Its duplicate file, JSON and
decode bodies are removed. The narrow comparison guard and supported numeric symbol resolution remain
in [`OriginalBattle01StartupDefinition`](../../remake/reference/Sf2.Remake.Reference/Content/Battle01/OriginalBattle01StartupDefinition.cs).

The selected trust descriptor remains Battle01-specific. The same semantic parser checks declared
counts, unique IDs, referenced regions, record lengths, closed properties and terrain/source joins;
it does not make the selected exact positions/counts the definition of valid gameplay. Source fields
retain identity/item/AI/order/spawn expressions, scene flags, unknown and trailing bytes, and all
terrain bytes and metadata. `EncounterPoint` stores raw byte coordinates, including255 for unresolved
spawn expressions. Only literal `STARTING` placements assert deployed in-area, nonoverlapping geometry;
regions and AI points retain their format bounds. Importing an unresolved expression does not execute
it. The retained comparison projection requires supported symbols before converting coordinates into
runtime `MapPosition`; no expression evaluator or silent default supplies missing semantics.

The single unchanged [`StackCompressedGraphicsDecoder`](../../remake/src/Sf2.Remake.Content/Decoding/StackCompressedGraphicsDecoder.cs)
now belongs to Content. Both the new encounter reader and retained private-map visual reader consume
it through the existing namespace. Narrow internal access and the Reference→Content project reference
preserve that ownership; production projects do not depend on Reference. Existing Reference, game and
four legacy test lockfiles record the resulting project graph without changing package identities.
There is no second parser or decoder.

**Confirmed (loader checks):**
[`PrivateBattleEncounterTests`](../../remake/tests/Sf2.Remake.Engine.Tests/PrivateBattleEncounterTests.cs)
exercise independent authored semantic records with changed counts/IDs/positions, unresolved
expressions/raw coordinates, immutable collections and preserved unknown bytes. Duplicate/dangling
references, malformed geometry and incomplete terrain reject at their owning field; wrong/missing
private inputs reject without fallback or paths. The semantic seam is internal; the public file port
always verifies the selected private identities. The selected
[`AcceptedSelectedBattle01InputsAreRequiredToExerciseTheRealReader`](../../remake/tests/Sf2.Remake.Content.Tests/PrivateOriginalBattle01StartupReaderTests.cs)
comparison reads all three actual inputs and asserts every source/deployment/region/point/terrain/scene
field against the new definition, plus the unchanged legacy numeric projection and admission digests.
The old semantic matrix now isolates the comparison projection's unsupported symbol/scene boundary.
The affected `StackCompressedGraphicsDecoderTests` methods
`LiteralMoveToFrontWordAndOverlappingCopyAreDecodedInOrder` and
`EmptyTruncatedBadOffsetOverrunAndSizeDriftFailClosed`, plus
`PrivateOriginalMap3VisualPayloadReaderTests.SelectedSourceDecodedAndStackConsumptionDriftFailSemantically`,
exercise the moved calculator's real consumers. Engine behavior and adapter compilation use the
committed planner. No Map3 replay, new H3, ROM rebuild or legacy aggregate is required by this import.

The stopping boundary is a trusted, lossless source encounter definition and the unchanged legacy
startup projection, or an owning typed trust/structure rejection with no payload/path leak. This does
not create a playable common private session. `BattleDefinition` cannot retain the source orders,
regions or provenance; the cohesive encounter definition preserves them for existing import and later
initialization. The three inputs still lack enemy/party/start-rule closure, so an `IScenarioSource`
bridge belongs with the subsequent initialized start. No Godot composition, GameSession, source
schema/exporter, program runner or historical activated-state import changes at this boundary.

After the [content-model modernization](#authored-definitions-and-explicit-session-starts) slices are
independently accepted, the private blocking slices follow these real dependencies:

1. **Definition closure and initialized common entry.** Load the selected enemy/class/item/spell
   records from existing pinned exports and an external controlled party/start document; preserve
   unknown accounting and source fields. Add the bounded new-battle initializer and the required
   pre-round activation/control state, then bridge into the existing `IScenarioSource`/`ScenarioDefinition`
   and `BattleAdvancer` path. The target is the original unactivated placements with the external
   PlayerReady comparison inputs and explicit intro-skip/control policy, yielding the actual generated
   first player and movement/cancel. Compare initialized data and the existing
   [`map3-battle01-player-ready-v1` fixture](../../tests/fixtures/h3/map3-battle01-player-ready-v1.json)
   through that same public session; do not reset either RNG during play. This targets the separately
   observed player-ready seam, not a claim that a natural Map3 start skipped its programs. First-round
   unsupported branches must stop before any partial generation; unknown data is not silently zeroed.
2. **First inactive-enemy continuation through actual commands.** Migrate the actual class movement
   profiles and bounded standby/activation/set7 path, preserving each enemy's source anchor, memory,
   region words and both RNG channels. Use real player commands to reach the first enemy and next
   live control; consume the shared movement/target primitives. Missing movement needed to expose
   first player control belongs in step1; remaining enemy behavior belongs here. No round/receipt gate.
3. **Reached private action operands and completion.** Bind actual equipment/class/spell/reward data
   to the shared physical/HEAL/after-turn rules and compare the affected real actor/target chain.
   Remove the corresponding reference wrappers and Godot scheduling branches as their last callers
   migrate. This does not wait for M5; unsupported EGRESS, broader status, terminal programs and return
   stay with their named future consumers.

Those implementation scopes need fresh exact-path declarations after their predecessor is accepted;
this is a dependency decision, not authorization to implement all three together. **Unknown:** general
spawn/derived-stat coverage beyond the named contracts, naturally carried party/accounting/seed-copy
values, and natural Map3→Battle01 program/presentation continuity. Resolve a needed gap against the
specific data/initialization/control owner with a narrow source or existing-fixture follow-up first.
M3's pending-admission/map programs and M4's recovery/return consumers still prevent deleting the
legacy startup binding/session. M2, A1–A8 and8C/H4 remain incomplete.

## Evidence used and its limits

The route was synthesis first: [tactical battle loop](../design/synthesis/tactical-battle-loop.md),
[story progression](../design/synthesis/story-progression.md),
[map design principles](../design/synthesis/map-design-principles.md), and
[Map 3 readiness](../design/synthesis/map3-battle01-readiness.md), followed by the direct owners below.
The newer admission owner refines the older readiness summary: an explicit-bridge player-ready H3
exists, while the omitted natural story segment remains Unknown.

Original evidence uses the USA baseline and pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`; each linked fixture retains its own ROM identity,
source/function/address, observer boundary, and reproduction owner. The proposal consumes those
tracked records; it does not treat remake output as evidence of original behavior or run new H3 work.

| Subsystem | Contract → research → representative executable fixture | Confirmed reuse and remaining boundary | Design consequence |
| --- | --- | --- | --- |
| Exploration and map entry | [exploration flow](../design/contracts/exploration-control-flow.md), [entry routing](../design/contracts/map-entry-routing-state.md) → [gameflow](../research/gameflow-core.md), [common maps](../research/common-maps.md) → [gameflow H2](../../tests/fixtures/h2/gameflow-core-static-v1.json), [maps H2](../../tests/fixtures/h2/common-maps-static-v1.json) | Main-loop routing, pending-event handling, ordered map/battle selectors are Confirmed static. Natural reach and visible timing are separate. | Application owns ordered routing; map number and route-input count do not choose a scripted endpoint. |
| Map definitions and mutations | [setup data](../design/contracts/map-setup-data.md), [exploration](../design/contracts/map-exploration.md) → [map inventory](../research/map-data-inventory.md), [common scripting](../research/common-scripting.md) → [setup H2](../../tests/fixtures/h2/map-setup-static-v1.json) | Setup uses the last set-flag alternative; events use first match. New-map rebuild and current-map preserving reload differ. Geometry alone does not prove passability or display. | Keep ordered selectors and working layout separate from immutable resources; reuse existing map reducers. |
| Programs, dialogue, and entity actions | [program data](../design/contracts/standalone-map-script-program-data.md), [dialogue](../design/contracts/dialogue-system.md) → [common scripting](../research/common-scripting.md) → [interpreter H2](../../tests/fixtures/h2/map-script-engine-static-v1.json), [dialogue H3](../../tests/fixtures/h3/map-script-dialogue-v1.json), [bridge H3](../../tests/fixtures/h3/map-entity-action-bridge-v1.json) | Full interpreter graph is 304 programs; standalone setup subset is 178. Cursor transfers and bounded handler/wait seams are Confirmed; bridge wait release is harness-controlled. Natural motion, dialogue chronology, and timing remain bounded/Unknown. | Parse typed operations and preserve PC, continuation, and wait state. Neither opcode counts nor injected release prove a complete story interpreter. |
| Battle admission | [entry routing](../design/contracts/map-entry-routing-state.md), [battle lifecycle](../design/contracts/battle-control-lifecycle.md) → [current admission owner](../research/map3-battle01-admission.md) → [player-ready H3](../../tests/fixtures/h3/map3-battle01-player-ready-v1.json) | The declared bridge seeds the R2b terminal; original code then executes 46 inputs through admission and actor-1 control. R2a → R2b natural continuity and later victory are Unknown. | Preserve bridge provenance in reference setup; interactive runtime cannot require that input count or claim a wholly natural start. |
| Actors, enemies, items, spells, terrain | [allies](../design/contracts/ally-definition-data.md), [enemies](../design/contracts/enemy-definition-data.md), [items](../design/contracts/item-definition-data.md), [spells](../design/contracts/spell-definition-data.md), [encounters](../design/contracts/battle-encounter-definition.md) → [ally inventory](../research/ally-data-inventory.md), [core inventory](../research/core-stats-data-inventory.md) → [ally H2](../../tests/fixtures/h2/ally-data-static-v1.json), [core H2](../../tests/fixtures/h2/core-stats-data-static-v1.json), [terrain H2](../../tests/fixtures/h2/battle-terrain-decode-v1.json) | Stored records, joins, aliases, and raw fields are Confirmed static; definitions are not complete consumer semantics. Spell radius 3 is valid stored data with Unknown geometry; enemy sprite tail is not extra enemy definitions. | Normalize identity and references without losing source projection. Distinguish structurally valid content from implemented effects. |
| Movement and AI | [navigation](../design/contracts/battlefield-navigation.md), [AI](../design/contracts/battle-ai-decision.md) → [pathfinding](../research/battlefield-pathfinding.md), [AI research](../research/battle-ai.md) → [movement H3](../../tests/fixtures/h3/battlefield-movement-matrix-v1.json), [AI choice H3](../../tests/fixtures/h3/battle-ai-action-choice-v1.json) | Weighted first-admission propagation, stable order, AI commandsets stopping at first success, separate thinking RNG, and bounded decision matrices are Confirmed. Wider caller states and natural multi-turn tactics remain Unknown. | Reuse algorithms within declared domains; AI scores cannot substitute for real damage, and unsupported AI cannot silently become STAY. |
| Action, arithmetic, replay, rewards | [construction](../design/contracts/battle-action-construction.md), [combat](../design/contracts/combat-resolution.md), [spells](../design/contracts/spell-resolution.md), [RNG](../design/contracts/randomness.md) → [actions](../research/battle-actions.md), [runtime math](../research/runtime-rng-and-battle-math.md) → [actions H2](../../tests/fixtures/h2/battle-actions-static-v1.json), [HEAL H3](../../tests/fixtures/h3/spell-healing-v1.json), [healing boundaries H3](../../tests/fixtures/h3/spell-healing-exp-boundaries-v1.json) | Temporary construction and persistent application differ. HEAL power/cap, eligible-class EXP, MP-first replay, source arithmetic, and RNG order have named bounded evidence. Adjacent unobserved families are not implicitly supported. | Reuse pure calculations and ordered effects; validate the full action's supported branch closure before persistent commit. |
| Scheduling, death, outcome, return | [individual turn](../design/contracts/battle-functions-control-flow.md), [lifecycle](../design/contracts/battle-control-lifecycle.md), [cutscene routing](../design/contracts/battle-cutscene-routing.md) → [battle loop](../research/battle-loop.md), [runtime math](../research/runtime-rng-and-battle-math.md) → [control H2](../../tests/fixtures/h2/battle-control-static-v1.json), [turn H3](../../tests/fixtures/h3/battle01-turn-order-v1.json), [after-turn H3](../../tests/fixtures/h3/after-turn-status-lifecycle-v1.json) | Ordered activation/spawn/scheduling, dead worklists, checks before and after after-turn, and static outcome mutations are Confirmed. Natural complete battle/after-program/return and presentation are not closed. | One Application loop owns continuation; victory requires after-program and return completion before reporting stable exploration. |

For reproduction, the owners retain commands including `uv run sf2 h2 gameflow-core`,
`uv run sf2 h2 common-maps`, `uv run sf2 h2 map-script-engine`,
`uv run sf2 h2 battle-loop`, `uv run sf2 h2 battle-control`,
`uv run sf2 h2 ally-data`, `uv run sf2 h3 spell-healing`,
`uv run sf2 h3 spell-healing-exp`, and `uv run sf2 h3 map3-battle01-player-ready`.
Listing these is not a new execution result. Later original-runtime work still needs ADR 0016's
specific material question and admission; an Unknown is not an automatic research queue.

## Legal behavior and internal responsibilities

A command is admissible exactly when its session/revision is current, its actor and control phase
permit it, its referenced content and targets are valid, its rule prerequisites hold, and the
reachable action branch lies inside implemented capability. A reference case ID, receipt count,
particular round, five-unit position tuple, or named character's kill ordinal is not a rule predicate.

Keep true source conditions even when identity-specific: healer-class EXP eligibility, enemy-leader
outcome routing, the Taros/Achilles Sword condition, and Battle 01's reward table are examples.
Represent each as a named, typed rule/table operand in the relevant rule profile, with an original
identity mapping in the private import. Do not replace them with a vague generic modifier list or
silently discard them as walkthrough coupling. An unimplemented special rule stops explicitly.

| Audit | Root cause | Proposed owner and change | Observable acceptance |
| --- | --- | --- | --- |
| A1 | State legality and one replay's prefix are combined. | Domain battle command admission and turn reducer use live actor/phase/range/resources/status; reference runner owns expected rounds and receipts. | HEAL succeeds in two valid different histories; dead-slot skipping works in any generated round. Wrong actor, stale selection, and unsupported branches still fail. |
| A2 | Import trust, fixed fixture identity, and runtime definitions share one gate. | Content readers separate source verification from typed package validation; Application admits capabilities; reference loader alone chooses a fixed golden package. | Two authored rosters/maps using the same capabilities load without adding IDs/digests to production. Tampered private provenance still fails. |
| A3 | A story program is represented by a terminal state assignment. | Internal Application program runner plus Domain operation reducers retain cursor, motion, dialogue, and continuation. | Changing a branch flag changes executed operations; a dialogue/motion wait prevents later mutation; an unknown operation stops at its own PC. |
| A4 | Godot dispatches individual turns and rounds. | Internal Application battle coordinator drives activation, actor routing, action completion, and next control. | The same command transcript without Godot reaches the same next actor; Godot input code contains no round/AI/death scheduler. |
| A5 | One facade name hides incompatible state/command APIs. | One common command result and snapshot; focused internal coordinators receive the single state and return transitions. | Public authored and private original sessions accept the same semantic command family; reading a snapshot never requires selecting a profile-specific accessor. |
| A6 | Scenario receipts become constructor-wide mutable lifecycle fields. | Typed world/entity/program/battle state owns live facts; observations retain history separately. | Movement preserves inventory, flags, pending waits, and unrelated entity state without copying Sarah/castle/palace fields through every constructor. |
| A7 | New case IDs spread through unrelated action/round/death handlers. | Spell resolver, shared ordered effect application, and generic turn completion replace case-specific integration branches. | A second supported healer or different lethal actor/order requires content and tests only; no new case comparisons in round, attack, or dead-slot code. |
| A8 | Runtime errors use startup exceptions and lose their reason. | Common failure categories at existing boundaries, internal typed reasons, one Application mapping. | Bad MP, stale revision, unsupported effect, bad reference, invariant breach, and presenter error remain distinguishable through session/Godot results. |

The four assemblies keep their existing dependency direction. Domain owns deterministic rules and
typed transitions. Application owns the sole session state, control flow, content port, and observation
projection. Content owns untrusted decoding and immutable definition construction. Godot owns
composition, semantic input, presentation, and platform errors. Internal coordinators are ordinary
focused types and direct calls, not public services or alternative session facades.

## One command, state, and result model

The following is abbreviated interface notation, not a promised source-compatible API:

```text
GameSession.Start(AdmittedPackage, StartDefinition, RunPolicy) -> StartResult
GameSession.Submit(CommandEnvelope) -> SessionResult
GameSession.Snapshot -> SessionSnapshot

CommandEnvelope = { sessionId, expectedRevision, command }
Command = Move(Direction) | Interact(EntityRef) | Search | ChooseAction(ActionKind)
        | SelectSpell(SpellRef, Level) | SelectTarget(ActorRef) | Confirm | Cancel
        | ChooseDialogue(WaitToken, Choice) | Acknowledge(WaitToken)
        | AdvanceSimulation(WaitToken, TickCount)

SessionResult = {
  snapshot, orderedObservations, stopReason, failure?
}
StopReason = PlayerInput | PresentationWait | SimulationWait
           | Unsupported | Rejected | Faulted | Completed
```

`RunPolicy` selects the gameplay rule profile and observation/presentation policy before start;
content trust is established by its reader, not a caller-supplied boolean. A reference runner is an
external consumer of this same API and its observations. There is no `NextRound`, `EnemyAttack`,
`CompletePalace`, or `AcceptReceipt107` user command. `AdvanceSimulation` belongs to the host's
deterministic tick driver and replay runner, not an extra player button. Every accepted automatic
continuation uses the same Application dispatcher.

The state has one canonical value at each commit:

| State owner inside the session | Required typed contents and authority |
| --- | --- |
| Campaign | Ally actor records, party membership/order, inventory/equipment/spellbooks, flags, economy, and persistent counters. Actor IDs index typed records, never arbitrary `object` values. |
| Active world | Current map/resource selection, working layout, field entities and their movement/action state; selectors use live campaign flags. In battle, saved field return context is an anchor, not a second live player position. |
| Active encounter, when present | Encounter definition ref, enemy actor records, ally refs into Campaign, battlefield placements, terrain/occupancy, region/spawn state, ordered turn entries/cursor, AI memory, dead worklist, and outcome stage. Ally HP/MP/EXP are not copied into a competing roster authority. |
| Execution | A typed control phase, program frames/continuations when running, one blocking wait, and the current typed player selection when applicable. Constructors restrict which combinations can coexist. |
| Determinism and publication | Main and thinking RNG state, explicit simulation tick, session ID, revision, observation sequence, and pending acknowledgement identity. Presentation does not own these values. |

An immutable discriminated phase distinguishes exploration input, program execution, map transfer,
battle initialization, turn preparation, player selection, action resolution, after-turn, outcome,
and return. A program frame can carry a typed return continuation such as
`AfterBeforeBattleProgram` or `AfterVictoryProgram`; it does not own another Battle state.
Nested operations receive state explicitly and return a typed transition. Only `GameSession`
publishes it. The common snapshot is a read-only projection with a discriminated active-mode payload,
available actions, pending wait/selection, actor/world facts, and failure context; no null-forgiving
profile split and no scenario-named optional field collection.

### Provisional selection, atomicity, and stale input

Movement/target selection stores the turn origin, candidate destination/path, selected action/target,
and a state revision against which it was derived. It is the authoritative *selection*, not a second
committed actor position. Presenters may show its preview. Cancel restores the earlier selection
stage or turn origin and consumes no gameplay RNG, MP, HP, inventory, reward, or turn. Confirm
rechecks phase, actor, content, occupancy, target side/range, resources, and capability against the
current state. Cancel after action commit is not undo.

A restart creates a new session ID. Every mutation, including selection and wait consumption, advances
the revision. A command carrying an old ID/revision or a previously consumed wait token is rejected
without mutation. This is concurrency protection, not validation of an entire historical receipt chain.
A valid start comes from the admitted loader; arbitrary snapshot injection is not a production API.

Resolve a committed action against a temporary transaction value containing tentative actor changes,
ordered effects, main/thinking RNG changes, and rewards. Do not debit persistent MP or move the
committed actor while still discovering an unsupported counter, item drop, level-up, or after-effect
inside that action. Once the whole action is supported and valid, publish one atomic action commit
and its ordered effect observations. Reuse snapshot/restore evidence as an observation contract,
without reproducing the original command-buffer storage.

A valid but ineffective action is not a rejected command. For example, the confirmed SILENCE gate
can charge MP before suppressing a marked spell effect; failed recasts can mutate status counters.
Such behavior must follow its admitted rule contract. Only command rejection or an unsupported
uncommitted action guarantees unchanged gameplay state and RNG.

Automatic flow can contain multiple commit boundaries. A later unsupported after-turn/program
operation retains earlier successfully committed actions and reports the exact stopped phase and
last committed revision. It cannot report that the original command had no effect. Non-gameplay
failure/observation metadata may advance on that stop; actor resources and RNG for the failed
transaction remain unchanged. Invariant failure freezes the last coherent state with the original
cause; it never silently retries with a simplified rule.

### Ordered observations and deterministic advancement

Snapshots answer what is true now; observations describe ordered changes and presentation requests.
Observations are not a replayable second state authority. Publish stable sequence/revision, actor or
resource refs, effect payloads, and an optional blocking token at meaningful boundaries: movement
commit, dialogue request, MP/HP/status/reward effect, death cleanup, outcome, and map readiness.

A presentation acknowledgement means the requested presentation boundary completed, not permission
to set arbitrary gameplay values. Only the matching wait may resume its continuation. A headless
semantic runner supplies explicit acknowledgements/ticks under a named policy. It cannot satisfy an
8C claim by auto-acknowledging. Godot disposal, redraw, and render callbacks do not replay mutations.

Application runs automatically to the next real stop. Internal work quanta may yield to the host
without exposing a new gameplay command or dropping state. A valid timed wait advances only through
explicit ticks; a non-progressing immediate program cycle is an attributed content failure, not a
successful end. There is no one-round dispatch cap or additional player click just to advance AI.
The RNG implementation keeps the main generator and thinking stream distinct, preserves recorded
integer widths/wrapping/draw order, and uses no platform time or Godot random generator.

## Typed content and capability admission

Use the existing offline extraction owners to verify original bytes, pinned identity, structure, and
references and to produce ignored private packages. Runtime performs no ROM reads, disassembly,
Python extraction, or address-based service dispatch. Preserve original numeric IDs, raw fields,
aliases, and source projection in provenance/diagnostics; runtime references resolve to typed IDs.

Three boundaries stay distinct:

1. **Source and distribution trust:** private-original readers verify registered input identity,
   pinned extraction provenance, schema, and asset references. Public authored readers accept only
   project-authored or properly licensed content under their own format/trust rules. Neither may
   claim the other's provenance or auto-fallback to its profile. Existing private digests remain
   meaningful; no new hash registry is needed merely to notice edits.
2. **Runtime structure and capability:** decode closed records; reject duplicate/dangling refs,
   malformed enums, invalid numeric domains, bad geometry/placements, unsupported schema, and
   missing required assets. Link the start's dependency closure before session creation. Validate
   rule/opcode support separately from structural validity, including unsupported reachable branches.
   Preserve structurally valid original fields such as radius 3 even if its consumer cannot run.
3. **Reference setup and comparison:** the test/replay loader pins the selected fixture package,
   original IDs, seed, bridge/controlled inputs, expected trajectory, and comparison observations.
   Those exact fixture digests and setup identities do not become general runtime admission rules.

Public authored and private original content use the same rule implementations and semantic command
path wherever their capabilities overlap. Their byte readers, trust claims, content availability,
and distribution rights stay separate. Public validation must not be weakened into an unchecked
byte factory to achieve this. The fixed public smoke package remains a trusted regression input,
while the proposed authored-package entry point has its own strict, reviewed validation.

The minimal definition model is finite and typed:

| Definition | Required fields and joins |
| --- | --- |
| Map | Typed map/resource IDs, dimensions, layout and passability/terrain references, area/roof/warp/event lists, ordered setup alternatives, entity placements, init-program ref. Layout word bits and render geometry remain distinct. |
| Entity | Entity ID, actor/resource ref when applicable, initial position/facing, interaction program, action-program ref; mutable motion/cursor/wait belongs to session state. |
| Battle | Battle ID, map/area/terrain refs, ally placements and enemy spawn records, regions/orders, before/start/region/defeated/after program refs, leader/outcome/reward rule operands, explicit return context. |
| Actor/class | Actor and class-rule IDs, level/base stats, maximum vitals, derived-stat rules, inventory/equipment and known spell refs. Current HP/MP/EXP and accounting belong to explicit start/session inputs; authored starts and original growth-derived starts have different policies. |
| Enemy | Enemy-definition ID, spawn baseline stats/status/items/spells, movement and AI definitions; spawn transformation and battle-local placement remain separate. |
| Item/spell | Typed definition refs and supported effect variant/parameters, equip/use restrictions, cost, ranges/targeting and presentation refs. Preserve original packed fields in source projection, not as universal inferred semantics. |
| Terrain | Fixed supported grid shape, typed cell values, movement-cost and land-effect tables. Original 48×48, byte arithmetic, neighbor order, and tie behavior constrain the initial fidelity profile. |
| Program | Program ID, ordered typed operations, validated branch/label refs, entity/text/resource refs; callable native subroutines require an explicitly implemented semantic mapping. |

Capability is a small code-owned catalog of implemented rule families and parameter domains, for
example ordinary healing, player movement, stable turn generation, and specific AI commands. Content
can require a family; it cannot declare its own implementation by setting `supported: true`.
A capability describes status/equipment/reward/target domains as well as the action name. Numeric
bounds must follow the owning rule, not a single sampled stat line. Broadening behavior beyond
evidence can be an explicit authored rule choice; it cannot be labeled Confirmed original behavior.
Do not create one capability per character, receipt, opcode helper, or test case.

### Minimal authored example

The current executable examples are
[`practice-yard`](../../remake/content/authored/practice-yard.json),
[`garden-watch`](../../remake/content/authored/garden-watch.json),
[`stone-court`](../../remake/content/authored/stone-court.json) and
[`river-post`](../../remake/content/authored/river-post.json). Use those complete format-v2 documents
through the real reader; the [profile owner](../../remake/docs/runtime-profiles-and-trust.md#public-authored)
describes their closed fields and supported domains. Their `actors` contain definitions, encounter
`placements` retain deployment coordinates, and `start.actors` bind explicit per-session values by
actor reference. A start override changes one controlled deployment without altering the definition.

A second supported package needs data changes only. Source slot order remains the explicit ordering
key until its separate mapping migration; authored IDs are not cast to source combatant numbers.
Missing map/actor references are ContentError. An unimplemented effect is UnsupportedCapability;
a runtime cast with insufficient MP is IllegalCommand. No loader fabricates missing start records,
replaces an unknown effect with HEAL, or treats an authored package as original-game evidence. These
battle-only examples do not implement the proposed program/return records described below.

## Program execution and connected flow

Use a small interpreter over typed operations inside Application, with effect reducers in Domain.
Reuse map selection, movement, block mutation, party/item, and flag mechanisms. This is not a general
scripting language, plugin runtime, ECS, event bus, service graph, reflection bridge, or network protocol.

The recommended initial operation vocabulary is:

| Operation family | Typed operations and execution rule |
| --- | --- |
| Control | `End`, `Jump(label)`, `BranchFlag(flag, setLabel, clearLabel)`, `Call(program)`, `Return`; retain PC and typed continuations. Source native `executeSubroutine` is not automatically a Program Call. |
| Story state | `SetFlag(flag, value)`, `JoinParty(actor)`, `GiveItem(actor, item)`; each delegates to its admitted domain rule and can fail on a real unsupported inventory/roster branch. |
| Dialogue | `SetTextCursor(ref)`, `ShowText(ref, mode, speaker)`, `ChooseYesNo(resultFlag)`, `CloseText`; preserve cursor and ordered show/close/wait behavior for the mapped source subset. User choice consumes its own wait token. |
| Entity action | `SetFacing(entity, facing)`, `StartMotion(entity, path)`, `WaitEntity(entity)`; movement progresses in explicit ticks and completion wakes the waiting program. No end-position assignment in place of traversal. |
| Map mutation/transfer | `CopyBlocks(region, destination)`, `TransferMap(map, position, facing, loadMode)`; distinguish rebuild, preserving reload, and reset. Transfer completes the ordered loading/setup continuation before field input resumes. |
| Timing/presentation | `WaitTicks(count)`, `Present(cue)`, `WaitPresentation(token)`; the cue is a closed variant such as fade, camera move, or sound request. A requested sound is not proof of audible output. |

A battle is admitted by Application's map/battle selector after a transfer, not by a story operation
that directly writes `Victory` or teleports to post-battle state. Battle hook programs reuse this
runner. Only reached supported operations are executable; the table is the recommended first
vocabulary to implement incrementally, not a claim that every operation listed is already supported.
An absent/malformed target fails content linking; a known but unimplemented operation reports
capability/PC and retains its previous commit boundary. A conditional path cannot skip validation of
its target refs. An admitted subset package may declare an explicit unsupported frontier; general
playable-package admission must not advertise that frontier as a complete flow.

For example, an authored gate program can be:

```text
0 BranchFlag(gateOpen, done, greet)
greet: ShowText(greeting, single, guard)   // wait for text completion
       StartMotion(guard, twoCellsEast)
       WaitEntity(guard)                // PC stays here until motion completes
       SetFlag(gateOpen, true)
done:  End
```

Changing `gateOpen` changes executed operations. Before the dialogue acknowledgement the guard
has not started moving; during the motion wait the flag is still clear; obstruction follows the
motion rule rather than completing the move instantly. Exact source script text/assets are private.
This authored sequence does not claim to implement the existing palace visit or original timing.

The connected Application lifecycle is:

1. **Explore:** process semantic movement/interactions, live setup/event selection, and pending
   events in their owning order. An event pushes its program; free movement pauses while its
   blocking execution owns control.
2. **Program/transfer:** execute operations, yields, and returns. A map transfer resolves the target,
   builds or preserves the working layout as required, applies flags/mutations, selects setup and
   runs init, then evaluates map/battle routing at its owned boundary.
3. **New battle:** use the selected encounter and live party. Preserve the more precise accepted
   Map 3 admission spine: before-battle program, region reset/party and enemy initialization,
   battle load, then battle-start program before first-round scheduling. The shared intro flag is
   checked by both hooks and set by the start wrapper. Other entry paths need their own admitted
   order; an older broad lifecycle summary must not override this named source seam.
4. **Round/turn:** activation → region program → spawn → turn generation. Preserve source signed
   altered agility, byte wrapping, stable ties, and draw order. Skip dead entries using current state;
   route an eligible player to input, and a supported AI/status route directly to its next transition.
   A sentinel/exhausted order generates the next round without Godot choosing it.
5. **Action:** validate/select/confirm, build ordered targets/effects, resolve and atomically apply,
   then deliver required presentation boundaries. Persistent application and presentation are
   distinct; an acknowledgement cannot apply the same effect again.
6. **Completion:** defeated-program seam → death cleanup → faction/outcome check → after-turn
   processing → death cleanup → faction/outcome check → next actor. If an earlier check ends
   battle, do not run later after-turn merely to simplify the reducer.
7. **Outcome/return:** victory heals eligible party, runs the after-battle program, applies the
   completion/unlock mutations, and returns through map loading/routing to stable player control.
   Ordinary defeat applies its own leader/gold/egress path. Battle-4 loss, special joins, EGRESS,
   suspension, or any other unreconciled branch remain explicit capabilities/Unknowns. Reporting
   battle outcome alone cannot mark ADR 0010's 5B return endpoint complete.

The after-battle wrapper's source join tail, including the zero-filled table, remains a separate
admitted service boundary. Do not invent joins or silently drop the call because the table looks
unused. This plan preserves that unresolved integration instead of claiming a complete winning route.

## Failures and diagnostics

Keep five top-level categories, with precise typed reasons and relevant field/actor/program context.
The names below are proposed semantic categories, not five new public exception hierarchies.

| Category | Example and ownership | State/result behavior |
| --- | --- | --- |
| IllegalCommand | Application: wrong actor/phase, stale revision/token, insufficient MP, out-of-range target. | Reject before gameplay commit; preserve specific reason and current snapshot. Available actions may explain the same reason. |
| UnsupportedCapability | Domain/Application: missing effect/status/AI/program/return branch with otherwise meaningful input. | Stop at the last coherent boundary; identify capability and phase/operation. Never silently substitute STAY or an endpoint. |
| ContentError | Content: malformed record, duplicate/dangling ref, invalid numeric field, schema/provenance mismatch. | Reject start/package or the explicit loading boundary; preserve safe field/resource context. No partial package publication. |
| InvariantFailure | Domain/Application: impossible internally constructed state, duplicate effect application, inconsistent actor/control authority. | Freeze and surface original cause with diagnostic context; do not reclassify as user input or startup refusal. |
| AdapterError | Godot/platform: missing scene binding, failed asset presentation, transport/probe error. | Preserve gameplay snapshot; expose the presentation failure. Do not fabricate its acknowledgement or report a passed observation. |

Expected failures are typed results. Map exception messages and causes only at the owning boundary;
do not catch every `ArgumentException` and discard its message into a generic startup diagnostic.
Public diagnostics use logical resource IDs and safe field names, with private details confined to
local output. No new logging service, diagnostic manifest, or per-helper public result family is needed.

## Independent verification and concrete counterexamples

Separate product unit tests from direct verification:

- **New-engine unit tests:** project-owned independent expected values and invariants from the linked
  contracts/fixtures, plus authored state/configuration variation. Compare arithmetic, targets,
  content semantics, command/state transitions, RNG, effects, and atomicity without calling production
  helpers to calculate expected values. A small in-memory GameSession with actual reducers can be a
  useful unit boundary. Generation of legal histories uses commands from admitted starts.
- **Reference verification:** ADR 0009/Map 3 and existing Battle01 comparisons pin exact inputs, seeds,
  controlled bridges, receipts, expected states and refusal boundaries. Preserve original H2/H3
  fixtures unchanged. A reference runner reports divergence; the runtime does not refuse legal
  play merely because the player departed from that transcript. Do not unit-test the runner, its
  fixture driver, comparison/report code, or setup helpers.
- **Direct adapter observation:** a semantic transcript under public/private readers and the Godot
  adapter verifies routing and projection, not original facts. Observe actual
  session/node state, visibility, geometry, focus, input routing, and error output where the gate
  needs them. Expected source values alone are not observations of a running Godot instance.
  Run the needed observation itself; do not add tests of the probe, smoke host, launch wrapper,
  exported marker, or verifier, and do not rebrand those tools as engine features to preserve tests.

At minimum, implement these **authored test specifications** when the corresponding rule domains
are migrated; they are not new original goldens. State/rule rows become ordinary engine unit tests;
the running-adapter row is direct verification, with no additional testing layer:

| Test | Concrete setup and expected behavior |
| --- | --- |
| HEAL positive, changed history (A1/A7) | Admitted healer at HP 95/100, MP 20, EXP 0, unpromoted priest rule, known power-15/cost-3 self HEAL, no unsupported equipment/status. Reach player control once in round 2 and again in a separately generated round-5 history. Use the same resolver seed `0x1234` in isolated rule-seam cases: recovery 5, accumulated EXP 10, award rolls 14/0, ordered MP `20→17`, HP `95→100`, EXP `0→9`. Neither case requires Sarah, raw offset 4, or 102 receipts. Full session tests use naturally carried seeds and independent expected draws, not a runtime seed-reset command. |
| HEAL negative, real legality (A1/A2) | In either history, set the authored start's available MP so it reaches the cast with MP 2, or choose a living ally outside configured range. Refuse the cast/confirmation with the exact resource/range reason; no MP/HP/EXP/turn/RNG commit. A stale selection from the earlier revision also refuses without effect. |
| HEAL positive, different configuration (A2/A7) | A second package has a different actor, encounter, map and legal placement; priest max HP 20/current HP 8, MP 12, EXP 0, power 15/cost 3. Self recovery is 12, accumulated EXP is `floor(25×12/20)=15`; at isolated seed `0x1234`, award is 14, MP becomes 9, HP 20. Different unrelated equipment/status is allowed only once its actual rule capability exists, not by erasing its guard. |
| Capability/content negatives (A2/A8) | Replace that spell's effect by an unimplemented resurrection family: report UnsupportedCapability rather than a case-ID mismatch. Remove the referenced terrain or duplicate an actor ID: ContentError. Tamper with private original provenance: retain the trust rejection even if the JSON shapes otherwise match. |
| Turn and kill order (A1/A7) | Generate two valid histories in which different supported physical actors kill an ordinary enemy in opposite orders. For each, assert cleanup/worklist/reward rules and the next live actor. Death count and named character order never gate legality; preserve actual leader-death and special-enemy rules. Gate this case on physical/death/reward migration, not the HEAL-only first slice. |
| Program wait and branch (A3/A6) | Execute the authored gate example with flag clear/set. Only the clear path emits greeting and motion. Wrong/double ACK does nothing; obstructed motion cannot set the flag early. A supported inventory change survives subsequent movement and dialogue. An unknown next operation stops at its PC without terminal-state injection. |
| Direct Application/Godot observation (A4/A5/A8) | Run the same command sequence headlessly and through the adapter; compare session snapshots, next actor, RNG, and ordered observations. Failed presentation is AdapterError and leaves its wait pending, rather than advancing the scheduler. The actual session's wait/failure behavior belongs in an engine unit test; the live probe is used directly, never tested by another harness. |

Classify each touched test family by the behavior it still protects. Migrate useful illegal-command,
content/trust, invariant, and unsupported assertions to direct new-engine unit tests. Fixed-prefix
assertions belong in selected reference cases, including a historically useful old refusal when it
explains a boundary; obsolete structural refusals can be deleted. If a guard combines rule and trace
conditions, separate them and keep the independent behavior assertions. There is no requirement to
preserve each old test method, run every historical case, or add a replacement test for each deletion.
Do not loosen original research goldens/digests or call general unit-test success original H4 parity.

Required invariants include no resources/RNG consumed by cancellation or rejection, ordered replay,
single application, stable tie behavior, equal semantic result for equivalent state regardless of
reference-history length, and deterministic replay under the same policy/inputs. A valid ineffective
spell is tested separately from rejection. Unsupported post-commit continuation tests must retain
the committed preceding action. Add only unit tests needed to protect those engine behaviors; use no
test-count, coverage-percentage, helper-validation, or meta-test acceptance target.

Screenshots are currently prohibited. State/node probes can validate semantic and adapter claims;
they cannot establish exact pixels, palette, audio waveform/chip behavior, VInt/DMA, or frame timing.
A debug/probe transport is not selected by this ADR. Future observation work must use an explicitly
chosen read-only interface and the existing running Godot instance where applicable, with no hidden
screenshot side effect. An unavailable observation remains unverified, not passed by source review.

## Old-test migration and retirement

The table classifies actual families at the accepted base. Rows can contain mixed assertions; move
the useful behavior, not the old private-field setup or method count. A new ordinary xUnit project,
proposed at `remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj`, references the
existing Domain, Application, and Content projects and reuses central package versions/locks. This
single project permits engine unit tests to run independently of legacy tests and the Godot SDK.
It is not a new test framework, runner, coverage platform, or source of gameplay truth. Do not leave
duplicate assertions running in both projects once that family is migrated.

| Existing directory/family | Keep as new-engine behavior assertions | Reference-only or retire | Migration time and old-gate closure |
| --- | --- | --- | --- |
| `remake/tests/Sf2.Remake.Domain.Tests/Battles/Battle01PlayerHealingTests.cs`, `Battle01PlayerMovementTests.cs`, `Battle01FirstRoundTests.cs` | Healing arithmetic/cost/EXP/RNG, range/occupancy, cancellation, turn ordering and genuine legality. Use small authored inputs through the new engine. | Fixed Sarah/round/offset/receipt assertions become selected reference expectations; reflection into turn internals, exact private object identity and giant copied history setup retire. The current HEAL test mixes valid effect assertions with receipt 103 and round 13: separate those claims. | M0/M1. Stop selecting these legacy families for the new engine once their product behaviors have direct unit assertions and relevant trace facts have an explicit reference home. No preservation of test totals. |
| Remaining Domain `Battles/Battle01*Tests.cs`, including physical, pursuit, standby, completion, defeat | Actual AI choices, physical effects, rewards, death/status/outcome rules and atomic transitions as each capability migrates. | Exact named kill order, generated transcript length and historical frontier positions are reference cases. Internal constructors/receipt-chain identity, reflection and duplicate helper checks retire. | M2/M4 capability by capability. A retired unsupported feature is reported absent rather than retaining a meaningless old passing test. Legacy gate closes for each migrated family; it need not await all battle features. |
| Domain `Maps/*Tests.cs`, `Items/ItemInventoryTests.cs`, `Battles/TacticalBattleTests.cs` | Existing map/flag/layout/inventory rules with real new-engine consumers. | Synthetic micro-battle rules are authored reference behavior unless intentionally adopted; do not import them as original-game rules. Redundant layout-copy and setup assertions retire. | M1/M3/M4 when the consuming engine function moves. Keep one direct assertion per useful behavior, not parallel old and new suites. |
| `remake/tests/Sf2.Remake.Application.Tests/GameSessionTests.cs`, `OriginalMapGameSessionTests.cs`, `OriginalMapVisualGameSessionTests.cs`, `PrivateOriginalBattle01*Tests.cs` | One-authority command admission, stale/cancel/atomicity, content linking, program wait/return, failures, and deterministic continuation. | Private field injection into `_privateOriginalMapSnapshot` and locomotion fields, null profile accessors, constructor shape and repeated whole-snapshot copying retire. Selected scenario endpoints/receipt projections move to reference verification. | M1–M4 with each common-session path. Gate removal depends on the new path's actual behavior, not source-compatible access to old fields or a green entire Application project. |
| `remake/tests/Sf2.Remake.Content.Tests/*ReaderTests.cs`, Application content-definition tests | Product loader semantics: malformed/duplicate/dangling refs, supported variants, numeric/range rules, public/private trust, atomic package admission. | Exact fixed original/smoke package identity belongs in the selected reference/trust verification case; repeated digest mutation matrices and duplicate parse checks retire. Preserve the loader's genuine provenance rejection as a unit behavior. | M1/M2/M3 with reader migration. One strict loader behavior test can replace many equivalent digest-only cases; no test is required solely to justify deleting another. |
| `remake/tests/Sf2.Remake.Godot.Tests/*PresenterTests.cs`, input/viewport/profile/catalog tests | Only actual rule/state logic that was misplaced in the adapter moves with that production logic into engine unit tests. | Needed visible/input/asset-mount behavior is checked directly against the running adapter. Retire old presenter signature/name, exact marker/copy, reflection, duplicate projection and probe/fixture self-check constraints where still present. Do not rename adapter or probe utilities as engine behavior. | M1/M3 as routing/projection moves. Remove this project from mandatory new-engine CI at M0; keep only explicitly needed direct adapter verification for changed surfaces. Prior audit repairs already removed some of these constraints; do not recreate them. |
| Test helpers inside the four `remake/tests/Sf2.Remake.*.Tests/` projects | Small data builders directly used by engine unit tests, with no tests of the builders. | Giant walkthrough constructors, reflection mutators, fixture self-checks, signature/count inventories, and duplicate assertions about verification output retire. Retain necessary selected trace data in existing reference/observation formats. | Delete with the last owning family, without a replacement-helper test suite or a new helper-validation matrix. |
| `tests/python/test_remake_architecture.py` | No migration into engine unit tests merely to preserve source-shape checks. Real inward dependencies remain build/review constraints. | Retire exact four-project discovery, source-string/name/visibility, workflow text, lock/version and helper-placement assertions from new-engine CI. Review dependencies and build the affected product instead. | M0 CI/policy cutover. Remove the old CI step and planner mappings together; no new test of the new workflow. |
| `tests/python/test_verification_plan.py` and engine/gate/CLI portions of `test_native_harness.py` | No new-engine unit assertions: these test verification infrastructure. | Retire engine planner/gate dispatch, preflight, command registration, report/count and driver meta-tests; exercise the needed verification command directly. The native harness also contains original-data/research checks: separate that other owner's responsibilities before deleting mixed content. | M0 with tooling/main-gate ownership. Remove these files from new-engine required checks immediately; research-only content stays isolated until its owner selects its direct verification route. |
| `tests/python/test_dotnet_environment.py`, `test_remake_godot.py`, `test_godot_ai_probe.py` | None: process setup, download/launch/probe wrappers are not engine gameplay. | Remove wrapper, protected-environment, archive, version, timeout, probe transport and artifact-report tests from engine CI; maintain required environment protections in actual launches and observe actual failures directly. Retire obsolete verification tests without replacing them. | M0. Shared process/private-input protections remain owned by tooling; this is not permission to weaken those protections or delete shared code without owner review. |
| `tests/python/test_remake_assets.py` | New-engine Content unit tests cover actual runtime content/asset-reference admission where relevant. | Export/preflight/descriptor/report tests do not become engine tests. Remove from engine CI; asset owner decides retention/retirement of actual asset-product assertions separately from redundant verification tests. | M0 for CI isolation, M2/M3 for runtime admission migration. Do not delete authored assets, licensing checks, private-input protections, or original evidence to shrink this suite. |
| `tests/fixtures/h2/**`, `tests/fixtures/h3/**`, research schemas/manifests and existing research executables | Consume minimal licensed facts as independent engine unit-test inputs where needed. | Preserve original evidence and named reproduction routes. They are neither obsolete engine implementation tests nor a mandate to run every old case for each engine commit. No additional tests of fixture drivers or reference verifiers. | Research/main-gate ownership throughout. Changed research contracts select their own direct verification; engine-only edits do not close or invalidate original 8C/H4 by themselves. |

Deleting a test does not delete its historical Git object or completed run record. Preserve prior
failures, exact failing nodes when any, skipped boundaries, and whether processes completed in the
handoff/owning evidence. The old [test suite audit](../../remake/docs/test-suite-audit.md) is a
historical diagnostic, not a permanent retention rule or numerical acceptance target.

## Remote CI and local verification cutover

The current [.github/workflows/public-checks.yml](../../.github/workflows/public-checks.yml) keeps
the existing PR/main-push trigger and splits its job responsibilities. A small inline PowerShell
`scope` job reads the actual Git changed paths and emits only
the three needed scope booleans; it performs no verification. Conditional jobs then use the explicit
path scopes below. Keep this wiring in the existing YAML, coordinated with the existing local planner;
do not add a generic dispatcher, workflow-validation framework, coverage job, meta-test job, matrix
generator, or test-discovery manifest.

| Job/entry | Trigger scope after cutover | Required execution/environment and exclusions |
| --- | --- | --- |
| New `engine-unit` job | PR/main changes to Domain, Application, Content, the new engine unit-test project and its authored inputs, shared remake build/package/SDK files, or this workflow. Add a genuinely consumed reference input explicitly if it becomes a unit-test dependency. Documentation-only and unrelated research changes do not execute it. | Windows runner initially; existing pinned .NET SDK 10.0.204 and net8 runtime 8.0.424, central xUnit packages and locked restore. Build/test only `Sf2.Remake.Engine.Tests.csproj` and its actual production dependencies (Domain in M0). No Python, native Godot, ROM, upstream checkout, private assets, old solution tests, probe tests, or coverage target. Preserve explicit CLI selection, PATH opt-out and per-run writable state in the real job. |
| New `adapter-build` job | PR/main changes to `remake/game/**`, its shared build/lock configuration, Domain/Application/Content API dependencies, or this workflow. | Locked restore/build of the actual Godot C# project using the tracked Godot.NET.Sdk/4.7.2 and .NET pins. This is compilation, not an extra automated test layer or native visual acceptance. It does not run the old Godot test project or download/launch the native editor. |
| Existing public workflow, narrowed to `research-public` scope | Changes to research/design contracts, owning research source/schema/fixture/manifest/toolchain paths, genuinely shared Python/private-input protections, or its workflow. Engine-only changes no longer trigger whole-solution restore/build/test or the seven old Python test families. | Keep uv locked dependencies and relevant lint/direct public traceability/index checks under repository/tooling ownership. Separate mixed native-harness responsibilities; remove engine/meta-test steps, not research evidence. Private H0/H1/H3 runs cannot become public CI inputs. Ordinary engine unit CI has no dependency on this job when its paths are unchanged. |
| Reference replay and live adapter checks | Only an affected comparison/observation or explicit milestone/user request; use the existing local reference/probe route. | Execute the verification itself with admitted inputs and existing host/instance policy. No tests of runners, fixture drivers, gate/planner/report code or probes. Retain exact results and unsupported/private boundaries; no automatic screenshot or full-route claim. |
| Local new-engine selection | New-engine source/content/test changes map to the same targeted engine project and applicable compile/direct observation. | Remove the engine requirement for normal legacy `verify`, whole-solution tests, blanket native Godot and tooling pytest fanout. Research/shared changes retain their owning selection. The planner can report the selection, but gets no new tests of that selection. |

The implementation enumerates the scope predicates from those concrete roots and actual dependencies,
including the workflow itself, in the same reviewed change as local selection. A missing base or failed
path comparison fails scope determination rather than declaring all jobs irrelevant. Main-gate updates
the required check contexts from the old monolithic job to the new scope/unit/build/research jobs;
the workflow remains triggered so path-irrelevant jobs can report explicitly skipped. An applicable
unit job must have an actual result; neither a missing workflow nor failed scope determination is
accepted as success. Review wiring, required-check settings and the actual first CI run directly;
do not unit-test the scope step, build a GitHub-filter test harness or generate synthetic PR matrices.

After loading the existing protected host/worktree environment, use the repository entries:

```powershell
uv run sf2 verify engine
uv run sf2 verify adapter
uv run sf2 verify plan --base origin/main --head HEAD
```

The first command performs locked restore, build and unit tests of the engine project; the second
performs locked restore/build of the actual adapter. Both run the selected .NET executable from
`remake/` to honor its SDK pin and force the existing PATH protection. CI executes those .NET steps
directly, so the engine job needs no Python. Add Application/Content project references only when
their real engine behavior is consumed; M1 now consumes all three production projects and its authored JSON inputs.

The planner automatically selects the engine scope for remake and non-research documentation paths.
Legacy test retirement does not select the old solution. Changes to shared CLI/harness/planner source
remain conservative research changes by default. For a declared engine-only wiring change in those
three files, `verify plan --scope engine --base origin/main --head HEAD` explicitly selects engine,
adapter and direct research-public checks. That scope rejects research artifacts and other shared
inputs; it is not permission to omit a semantic evidence dependency in an allowed file. Review the
actual diff before selecting it. Normal research `verify` and its evidence dependency handling remain.

Cutover is part of M0/M1, not a backlog item after the engine is declared complete:

1. **Engine owner** creates the small unit project with the first migrated real behavior and removes
   the corresponding duplicated legacy assertions. Keep old runtime entry points only while actual
   unmigrated capabilities still need them; old tests are not an architecture constraint.
2. **Repository/tooling owner**, serialized with the engine owner, changes CI workflows,
   `src/sf2tool/harness.py`, `src/sf2tool/verification_plan.py` and any affected CLI registration
   to distinguish engine execution from research verification. Remove obsolete pytest mappings,
   whole-solution fanout and deleted-test fallback for deliberately retired engine test paths.
   Do not remove conservative unknown-path handling for unrelated research, and do not add tests
   of the revised gate or planner.
3. **Main-gate/guidance owner** synchronizes root `AGENTS.md`, `README.md`,
   [ADR 0012](0012-dependency-aware-partitioned-verification.md),
   [ADR 0017](0017-heavy-boundaries-light-internals.md), `remake/README.md`,
   [development and verification](../../remake/docs/development-and-verification.md),
   [test suite audit](../../remake/docs/test-suite-audit.md), and any engine routing references in
   `docs/operations/agent-resume.md`. Review the Phase 2 runbook's shared gate references but keep
   research lane requirements with their owner. Replace blanket preservation/all-suite/meta-test
   obligations for the new engine and label retired gate recipes as historical, so a resumed agent
   cannot reinstate them from old guidance.
4. **Close each old gate** when its actual migrated behavior has useful engine unit assertions,
   selected trace facts have their reference home, no unmigrated product path depends on that gate,
   CI/local mappings and required checks point to the intended new commands, and guidance agrees.
   Confirm this from the diff, direct selected commands, and actual CI outcome. No test-count parity,
   coverage quota, green old aggregate, extra test of the cutover, or per-deletion replacement test
   is a closure condition. Unrelated research/private-input protections remain in their own scope.

M0 implements the shared code/workflow/guidance changes under serialized ownership. Main-gate owns
remote required-check configuration: replace `tracked-inputs` with `scope`, `engine-unit`,
`adapter-build`, and `research-public` after reviewing the candidate. Keep the workflow name
`Public checks` and inspect the actual emitted check contexts. Completed verification results remain
exactly as observed; cutover does not justify replaying legacy normal/full/.NET/Godot suites.

## Incremental migration and stopping conditions

Migration order is below; M0's implemented subset is recorded above. Other candidate paths name
future responsibility. Each implementation slice starts from accepted main, declares exact paths/tests,
checks competing writers, and obtains independent review. Do not create empty scaffolding for later
rows or execute this entire table as one rewrite.

| Stage | Concrete change and candidate owners | Dependencies, acceptance, stop and rollback |
| --- | --- | --- |
| M0: classify and cut over verification | Implemented subset: internal main RNG, ordinary priest healing scalar resolution, turn-order generation and Manhattan action range, consumed by existing Battle01 wrappers. Dedicated Engine.Tests protects the moved mechanics. Scoped CI/local/planner/guidance replace new-engine legacy gates under serialized ownership. Full movement and cancellation remain for M1. | No unintended behavior change in existing callers; independent arithmetic/boundary expectations and meaningful varied inputs protect the rules. Original fixtures and selected reference observations remain owned. Retire obsolete/meta-tests. Freeze a Draft PR, record direct command/CI results and main-gate required-check action, then stop for independent acceptance before M1. No old aggregate or tests of the cutover. |
| M1: first connected functional vertical slice | **Implemented:** Content `Scenarios/AuthoredScenarioPackageReader.cs` and Application `Content/Scenarios/ScenarioDefinition.cs`; thin `Runtime/GameSession.cs`, independent `Runtime/Battles/BattleCommandDispatcher.cs` and `BattleAdvancer.cs`; Domain `Battles/Rules` and `Battles/State`; Godot `Battles` input/view and independent snapshot projection. Legacy scenario classes and controlled presets live in the separate reference assembly with no reverse production dependency. | Load either authored controlled encounter, reach player control, move/preview/cancel, atomically HEAL/STAY, skip a dead queued entry when present, execute configured Stay AI, generate another round and return to player input automatically. The first four examples have engine unit assertions and actual no-image adapter observation. Physical attack, level-up, general AI, programs and outcomes remain Unsupported. Stop for independent acceptance before M2. Old explicit reference startup remains selectable before creation; no live authority switch. |
| M2: migrate battle rules and private admission | **Ordinary physical first/second/counter capability implemented; remaining M2 incomplete.** The [private-admission dependency boundary](#private-battle-admission-dependency-boundary) records the implemented encounter import; the user-approved content-model modernization precedes initialized-start work. Extract real rules from reference `Rules/Battles/Battle01*` and `Sessions/Battle01` into cohesive production `Battles/Rules` collaborators. Move actual encounters/actors/spells and source-special rules into Content-loaded configuration; evolve the reference private startup reader into real trust/import mapping. Keep controlled party/start presets and comparison histories only as external reference inputs. Route private battle through the common dispatcher. | M1 is accepted. Migrate one actual AI → physical effects → reward/death/after-turn chain at a time, with alternate-actor/kill-order behavior assertions and the affected original comparison. Preserve source provenance and real special rules. Delete each migrated wrapper/data class with its last caller, including its Godot scheduling entry; no production dependency on reference. Report reached unsupported branches precisely. Stop at a coherent capability frontier and roll back affected binding/publication if parity or atomicity fails. |
| M3: programs and exploration on the common session | Replace the reference `Sessions/Maps/OriginalMapGameSession.cs`, reached `PrivateOriginalMap*` and synthetic lifecycle consumers with cohesive production program/exploration collaborators. Move real map/entity/event/program definitions from reference content/admission into JSON or suitable Content-loaded configuration and typed validation. Preserve source-special operations and provenance. | Common facade exists. Execute actual reached operations and waits/effects before replacing each story handler; never treat required story as disposable validation or assign its terminal state. Connect authored exploration/program/transfer/battle admission, then migrate original content. Reuse actual map/layout/entity rules, add behavior assertions and selected public-entry reference comparisons, and delete each old class/data with its last caller. Stop at unsupported native/timing frontiers and roll back any route whose comparison disagrees. |
| M4: outcome and return | Extend the common battle advancer, program runner and transfer collaborators from the reference defeat/recovery/return owners and their after-program content. | Accepted action/death/after-turn and required program operations first. Implement battle outcome → after-program → completion flags → return map → stable input, then compare only admitted original paths. Preserve leader-loss/special-outcome boundaries and delete migrated endpoint implementations. Do not claim ADR 0009 complete without natural continuity, accepted endpoint and required fidelity. Roll back incomplete outcome publication; victory is not return. |
| M5: remaining migration cleanup | Remove remaining superseded fields, dispatch, scheduling entry points and obsolete tests after capability-by-capability M2/M3/M4 removal. Preserve original evidence and useful external comparisons. | All affected callers use the common session and its behavior tests/affected comparisons pass. Remove the temporary legacy start binding and reference runtime with its last admitted capability; do not defer all scenario/data migration to this row. Confirm no production dependency on reference and no permanent parallel engine or mandatory legacy aggregate. |

M1 is deliberately a useful connected battle capability with a small content domain, not a declaration
that its authored direct battle start satisfies ADR 0009. Its configured Stay enemies exercise actual
automatic scheduling and RNG without pretending general enemy behavior exists. Test-only legal-state
builders may isolate a dead-slot or seed seam; end-to-end histories must come from admitted starts
and commands. Starting HP deficits must be an explicit authored controlled-start policy, separate
from original new-battle party healing, so the first HEAL cannot accidentally rely on a bypassed rule.

The migration reuses confirmed arithmetic, navigation order, AI selectors, turn generation,
after-turn/status logic, scene-effect replay, and reward/accounting rules. It does not mechanically
reuse their walkthrough guards. Existing `TacticalBattle.cs` public-synthetic behavior remains a
characterized consumer until migrated; do not treat its simplified rules as an SF2 oracle or install
a second permanent generic battle engine beside it.

For implementation stages, the synchronized CI/local cutover above is part of acceptance. Run the
new engine's useful unit tests and affected product build/direct verification, not every legacy
Domain/Application/Content/Godot project or tests of tooling. Update the committed planner and its
guidance so the selected paths agree; engine-only changes must not resurrect the old always-run
`verify`, whole-solution, full/native, or infrastructure pytest obligations. Research/shared changes
retain their independently owned requirements. Preserve completed failed nodes and process state;
rerun only corrected engine behavior or newly affected direct verification, not an old aggregate
merely to replace its result. This amends the old engine gate route. Documentation-only increments
still require direct document/scope/private-boundary checks, without replaying engine or research suites.

## Existing decisions and user decision items

| Accepted decision | Compatibility and proposed amendment, if adopted |
| --- | --- |
| [ADR 0009](0009-first-phase4-playable-slice.md) | Keep its Map 3 → completed Battle 01 reference milestone and source evidence. Runtime capabilities serve general valid play; that walkthrough becomes an external comparison/acceptance route, never a source of legality predicates. Completing general engine slices does not complete 0009. |
| [ADR 0010](0010-map3-battle01-product-acceptance.md) | Keep the named start/return, control, content, restart-only, access, and deviation boundaries. 4A already permits interactive divergence under deterministic rules. Recommend a separately reported modern semantic playable deliverable while 7C content trust and the accepted 8C target retain their own gates. Any removal/redefinition of 8C or original start/return requires an explicit user decision and owning ADR amendment. |
| [ADR 0011](0011-phase4-remake-runtime-architecture.md) | Preserve four layers, one state authority, semantic commands, deterministic RNG, typed content/programs, and stable observations. This proposal makes those boundaries concrete and repairs their currently incomplete implementation. An exact 8C presentation backend remains undecided. |
| [ADR 0012](0012-dependency-aware-partitioned-verification.md) | Preserve dependency-aware selection for original research. Amend new-engine partitions, always-run public-core/whole-solution assumptions, obsolete-test deletion fanout and planner/tooling-test requirements during M0/M1. The explicit user policy is new-engine unit tests plus needed direct verification, with no tests of verification infrastructure. Update executable mappings and guidance together. |
| [ADR 0016](0016-remake-start-evidence-deferral.md) | Preserve bounded start and static-first deferral. Authored connected slices and accepted static semantics can proceed with explicit Unknowns; no blanket new H3 requirement, natural-route claim, or waiver of eventual evidence. |
| [ADR 0017](0017-heavy-boundaries-light-internals.md) | Preserve heavy trust/mutation/versioned-observation boundaries and lightweight internal calls. Add the explicit behavioral migration path: M0 protects moved product behavior with useful unit assertions; later stages expand valid state/content admission. Amend blanket characterization/old-test retention assumptions; tests of internal structure and verification infrastructure may retire without replacements. This expansion is not a behavior-preserving file split. |

This direction preserves the accepted product and evidence boundaries above. The product approach is
to deliver and measure semantic playability first, while separately reporting original presentation
parity as incomplete. This avoids coupling engine structure to one route without lowering an accepted
fidelity bar by implication.

The new-engine unit-test-only addition policy and consideration of old-test retirement are already
explicit user requirements, not unresolved permission questions. The user has adopted the direction
and M0 starting boundary. Remaining later product decisions, without blocking bounded M0 work, are:

- Confirm whether modern semantic playability is a separately named deliverable while original
  8C/H4 remains accepted but incomplete, or explicitly revise that product target in its owner.
  Recommendation: separate reporting and preserve 8C until an explicit revision.
- Select the later observation interface and permitted hardware/audio/frame evidence when needed.
  Recommendation: deterministic session and actual node/input probes for ordinary engine work,
  with screenshot prohibition retained; do not claim those probes close visual or audio parity.

No user save/load, engine replacement, universal scripting framework, new public asset rights, or
hardware emulation backend is introduced. Each implementation slice stops after a frozen Draft PR
and independent review; unanswered product/fidelity choices and audit findings remain visible.
