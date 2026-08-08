# Battle Control and Combatant Lifecycle Contract

- **Confirmed original behavior:** the bounded new/resumed battle entry order, round scheduling,
  combatant admission and cleanup, Battle 01 region/turn-order observations, post-action faction
  checks, one-step after-turn status processing, and static outcome mutations described below.
- **Unknown original behavior:** cross-process suspended-battle persistence, upgrade and Jaro/egress
  edge cases, spawn-reset failure causes, natural behavior outside the accepted Battle 01 runtime
  seams, complete multi-round status evolution, and rendered/audio/input timing.
- Remake status: implementation-neutral Phase 3 contract; no battle-simulation architecture,
  scheduler representation, or deliberate compatibility deviation has been selected.
- Evidence date: 2026-08-08
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the original controller state and ordered lifecycle seams around a tactical
battle. It owns:

1. new and resumed `BattleLoop` entry;
2. round activation, spawn admission, and turn-order handoff;
3. post-action death processing, faction checks, and after-turn handoff;
4. victory, defeat, and the Battle 4 special-loss result;
5. the bounded runtime observations that close Battle 01 activation, turn-order, and one-step
   after-turn behavior.

It does not own player or AI action selection, movement/pathfinding, damage or spell formulas,
battle-scene rendering, campaign meaning, or product-level battle simulation. Those remain with
their own contracts or **Unknown**.

The executable evidence owners are:

- `sf2-battle-loop-static-v1` in
  `tests/fixtures/h2/battle-loop-static-v1.json`;
- `sf2-battle-control-static-v1` in
  `tests/fixtures/h2/battle-control-static-v1.json`;
- `sf2-battle01-turn-order-v1` in
  `tests/fixtures/h3/battle01-turn-order-v1.json`;
- `sf2-turn-order-boundaries-v1` in
  `tests/fixtures/h3/turn-order-boundaries-v1.json`;
- `sf2-battle01-region-activation-v1` in
  `tests/fixtures/h3/battle01-region-activation-v1.json`;
- `sf2-battle01-secondary-activation-v1` in
  `tests/fixtures/h3/battle01-secondary-activation-v1.json`;
- `sf2-after-turn-status-lifecycle-v1` in
  `tests/fixtures/h3/after-turn-status-lifecycle-v1.json`.

The research owners are [battle loop and lifecycle](../../research/battle-loop.md),
[Battle 01 placement](../../research/battle01-placement.md), and the bounded turn/status portions of
[runtime RNG and battle math](../../research/runtime-rng-and-battle-math.md).

## Canonical Controller State

An implementation MUST keep these state domains distinguishable:

| State domain | Confirmed original boundary |
| --- | --- |
| combatant rosters | 30 ally slots and 32 enemy slots; enemy combatant indexes begin at 128 |
| battle regions | new-battle flags 90 through 105; separate newly-tested-region and enemy activation fields |
| turn order | ordered two-byte `(combatant, altered agility)` entries terminated by combatant byte `0xFF` |
| dead-combatant worklist | explicit length plus appended combatant indexes, processed separately from HP writes |
| outcome | signed `D4` result: victory `1`, ordinary defeat `-1`, Battle 4 special loss `0` |
| suspension | flag 88 plus saved/current elapsed seconds and reloaded battle state |

A single `battleState` enum cannot losslessly represent these arrays, flags, worklists, and return
values. The controller coordinates them; it does not collapse their subsystem ownership.

## Entry and Round Scheduling

### New Battle

**Confirmed static order:** a new battle clears elapsed seconds, executes the before-battle and
battle-start cutscene seams, clears battle-region flags 90 through 105, heals the eligible party,
initializes ally and enemy battle state, and loads the battle. The between-battle healing step:

- skips other dead allies but always processes Peter (7) and Lemon (28);
- restores current HP and MP to their maxima;
- preserves only the STUN/POISON/CURSE mask `0x0007` before rebuilding derived stats.

This is a controller lifecycle rule. It is not a healing-spell rule and does not establish the
visible timing of any setup step.

### Resumed Battle

**Confirmed static order:** a suspended entry restores the saved seconds counter, clears flag 88,
clears AI memory, reloads battle state, and resumes the individual-turn loop. AI-memory reset fills
48 last-target bytes with `0xFF` and clears 48 memory bytes to zero.

This proves an in-code handoff, not cross-process SRAM persistence, power-loss recovery, exact resume
UI, or a natural suspended-save lifecycle. Those remain **Unknown** and continue to be bounded by the
[save-system contract](save-system.md).

### Round Order

**Confirmed static order:** every new round performs:

1. enemy activation;
2. the region-cutscene seam;
3. enemy spawn admission and animation;
4. turn-order generation.

An individual-turn entry whose combatant byte is `0xFF` starts the next round. The sentinel and the
four-step order are fidelity facts; frame timing and whether presentation overlaps these calls are
not closed.

## Region Activation and Spawn Admission

**Confirmed static:** new-battle setup clears the sixteen region flags. Spawn admission scans all 32
enemy slots and recognizes initialization modes `0x0100` (respawn), `0x0200`
(hidden/region-triggered), and `0x0300` (both). A successful reset candidate is appended to
`TARGETS_LIST`; reset failure skips that candidate. The reason for a reset failure and complete
spawn animation chronology remain **Unknown**.

**Confirmed runtime, bounded to Battle 01 first-round seams:** the accepted fixtures establish:

- the baseline ally positions trigger none of the three region polygons, while the separate
  newly-tested-region field is `0b111`; that field records polygons tested, not polygons activated;
- moving Bowie from `(8,18)` to `(8,12)` activates all three flags and sets only primary-active bit 0
  on the six naturally primary-region enemies, preserving their other activation bits;
- a controlled enemy with primary region `NONE` and secondary region 2 changes activation
  `0x2060 -> 0x2063`, enabling both primary-active and secondary-active bits; the other five enemies
  retain primary-only activation.

These observations do not establish later-round clearing, natural secondary-region data, region
cutscene timing, or global encounter pacing.

## Turn-Order Construction

**Confirmed static model:** only placed, living combatants enter the list. For each admitted
combatant, the original:

1. masks current agility to the low seven bits;
2. uses two bounded RNG results with range `agility >> 3`, adding the first and subtracting the
   second;
3. adds `RNG(3) - 1`;
4. stores the combatant index and wrapped altered-agility byte;
5. when raw agility is at least 128, adds a second entry whose base is
   `floor((agility & 0x7F) * 5 / 6)` and applies the bounded add/subtract pair without the final
   `RNG(3) - 1` term.

The fixed-size list is stably bubble-sorted by the altered agility interpreted as a signed byte in
descending order, then the current-turn index is cleared.

Two runtime fixtures close only their exact seams:

| Runtime case | Confirmed observation |
| --- | --- |
| natural Battle 01, seed `0x1234` | nine entries; ordered scores are `0:109`, `2:8`, `1:6`, `128:6`, `133:6`, `129:4`, `130:4`, `131:4`, `132:4` |
| controlled boundary, seed `0x0000` | dead ally 2 and unplaced enemy 128 are absent; AGI 128 gives combatant 0 scores 0 and 255; AGI 127 gives combatant 1 score 135, which sorts as signed `-121`; equal positive scores retain insertion order |

An implementation MUST preserve byte wrapping, signed comparison, stable ties, second-turn
construction, and RNG-consumption order at these seams. Status-modified agility, multiple AGI >=128
combatants, overflow beyond the observed cases, and other battles remain **Unknown**.

## Death Worklists and Combatant Cleanup

**Confirmed static:** `CountRemainingCombatants` admits only combatants with non-negative X and
positive current HP. It returns ally and enemy counts separately and forces the ally count to zero
when combatant 0 has zero HP.

`KillRemainingEnemies` first clears the dead-combatant list, then scans placed living enemies. It
appends each index before writing that enemy's current HP to zero.

`ProcessKilledCombatants` returns immediately when the worklist is empty. Otherwise its persistent
cleanup boundary:

- increments defeats for a dead ally or credits an enemy death to `BATTLESCENE_FIRST_ALLY`;
- clears X and Y to `-1`;
- clears status and rebuilds derived stats;
- moves the associated entity to `0x7000,0x7000`.

The source also contains visual passes. Their display, animation, audio, and timing remain
**Unknown**; an adapter must not infer them from the persistent mutation order.

## Post-Action and After-Turn Order

**Confirmed static controller order:** after an action, the loop:

1. runs the defeated-cutscene seam;
2. processes killed combatants;
3. counts both factions and exits if an outcome is reached;
4. processes the acting combatant's after-turn effects;
5. processes killed combatants again;
6. counts both factions again and exits if an outcome is reached;
7. advances the turn index when battle continues.

The repeated death/faction check is part of the contract. A remake MUST NOT postpone every death and
victory/defeat decision until after status processing if it claims fidelity to this controller seam.

**Confirmed runtime, bounded:** five natural Battle 01 `ProcessAfterTurnEffects` calls with controlled
RNG seams reproduce one-step MUDDLE and SILENCE expiry/continuation, deterministic SLOW/ATTACK/BOOST
counter decrements, ordered status writes/messages, and one final `UpdateCombatantStats`
normalization. Empty equipment causes transient CURSE to be removed by that final refresh.

The fixture proves one transition, not a complete multi-round lifetime, naturally carried status,
every after-turn branch, or player-visible message timing. Detailed counter units and exact results
remain owned by the [spell-resolution contract](spell-resolution.md).

## Outcome Contract

**Confirmed static:** the top-level result and persistent mutation boundaries are:

| Outcome | Return | Confirmed mutations |
| --- | ---: | --- |
| victory | `D4 = 1` | heal eligible party; run after-battle cutscene seam; clear unlocked flag; set completed flag at battle offset +100 |
| ordinary defeat | `D4 = -1` | restore leader HP; halve gold by unsigned floor division; obtain the battle egress position |
| Battle 4 defeat | `D4 = 0` | complete/upgrade the hardcoded battle path rather than ordinary defeat |

These return codes do not define campaign meaning, save timing, rendered aftermath, or the full Jaro,
upgrade, and egress special-case surface. An individual-turn EGRESS/Angel Wing also uses a zero exit
result through another owner; it MUST NOT be conflated with the Battle 4 loss reason.

## Adjacent Static Control Boundaries

The accepted control fixture also establishes these supporting facts:

- difficulty is `flag[78] + 2 * flag[79]`, yielding 0 through 3;
- battle spriteset subsections are sizes, allies, enemies, regions, and AI points; entity and region
  entries are 12 bytes, and a missing starting position returns `(-1,-1)`;
- outside battle, map music is preserved; in battle, inputs 0/8/14 select theme 3 and 40/38 select
  theme 1;
- battle VInt setup clears the prior list and installs map planes, entities, view, scrolling,
  sprites, windows, and map animations in that order;
- the laser helper rejects a non-laser battle or facing `-1`; otherwise it marks to the map edge and
  appends occupying combatants.

These are static selector/order boundaries, not evidence for audio timing, rendered VInt output,
laser presentation, difficulty intent, or encounter balance. The source-owned debug self-loop is
excluded from reachable gameplay behavior. `PrintAllActivatedDefCons` and that self-loop remain
indexed inventory/debug-reporting footholds only; neither establishes a player-facing fidelity
requirement.

## Fidelity and Modernization Boundary

An original-fidelity controller MUST preserve:

- distinct roster, region, turn-order, dead-list, suspension, and outcome state;
- new/resumed entry and round call order;
- activation bit polarity and the bounded Battle 01 runtime results;
- turn-order admission, byte/RNG arithmetic, second entries, signed stable sorting, and sentinel;
- the two death/faction checks around after-turn processing;
- cleanup mutation order and static outcome return/mutation boundaries.

A future remake MAY replace RAM layouts, use typed state, expose turn previews, animate setup
differently, speed up transitions, add logs, change failure penalties, or adopt another scheduler.
Those are product decisions, not original facts. Each deviation requires an explicit decision and an
H4 expected-deviation record when it changes a fixture-visible result.

## H4 Acceptance Surface

A future H4 adapter should compare:

1. new/resumed entry traces and the four-step round trace;
2. region flags, tested-region state, and enemy activation bitfields for the two Battle 01 fixtures;
3. exact turn-order entries for both runtime cases, including duplicate combatants and wrapped
   signed scores;
4. dead-list append/HP/cleanup state and both faction-count checkpoints;
5. one-step after-turn state using the existing status fixture rather than a synthesized aggregate;
6. outcome code plus the explicitly confirmed persistent mutations.

H4 SHOULD compare canonical state and ordered events rather than original RAM addresses. Complete
encounters, tactical choices, animation/audio/input timing, save persistence, campaign transitions,
and balance remain outside this adapter until separately evidenced or deliberately designed.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| entry, round order, death checks, outcomes, adjacent selectors | **Confirmed static** | `sf2-battle-control-static-v1` ([`battle-control-static-v1.json`](../../../tests/fixtures/h2/battle-control-static-v1.json)) | Suspend persistence, upgrade/Jaro/egress edges, rendering/timing |
| roster lifecycle, spawns, counts, cleanup, turn/status entry points | **Confirmed static** | `sf2-battle-loop-static-v1` ([`battle-loop-static-v1.json`](../../../tests/fixtures/h2/battle-loop-static-v1.json)) | Reset-failure causes, visual passes, unparsed helper behavior |
| Battle 01 region activation | **Confirmed runtime, bounded** | `sf2-battle01-region-activation-v1` ([`battle01-region-activation-v1.json`](../../../tests/fixtures/h3/battle01-region-activation-v1.json)) and `sf2-battle01-secondary-activation-v1` ([`battle01-secondary-activation-v1.json`](../../../tests/fixtures/h3/battle01-secondary-activation-v1.json)) | Later rounds, other battles, natural secondary data, cutscene timing |
| turn-order construction | **Confirmed runtime, bounded** | `sf2-battle01-turn-order-v1` ([`battle01-turn-order-v1.json`](../../../tests/fixtures/h3/battle01-turn-order-v1.json)) and `sf2-turn-order-boundaries-v1` ([`turn-order-boundaries-v1.json`](../../../tests/fixtures/h3/turn-order-boundaries-v1.json)) | Other AGI/status/caller states and battles |
| one-step after-turn status lifecycle | **Confirmed runtime, bounded** | `sf2-after-turn-status-lifecycle-v1` ([`after-turn-status-lifecycle-v1.json`](../../../tests/fixtures/h3/after-turn-status-lifecycle-v1.json)) | Multi-round natural state, other branches, message timing |
| tactics, AI/player choice, resolution, presentation, campaign meaning | **Unknown / separate owner** | No aggregate executable owner | Consume dedicated contracts and future product decisions |

## Reproduction

```powershell
uv run sf2 h2 battle-loop
uv run sf2 h2 battle-control
pwsh ./scripts/Test-H3Battle01TurnOrderFixture.ps1
pwsh ./scripts/Test-H3TurnOrderBoundariesFixture.ps1
pwsh ./scripts/Test-H3Battle01RegionActivationFixture.ps1
pwsh ./scripts/Test-H3Battle01SecondaryActivationFixture.ps1
uv run sf2 h3 after-turn --timeout-seconds 150
uv run sf2 design-contracts test
uv run sf2 research-index test
```
