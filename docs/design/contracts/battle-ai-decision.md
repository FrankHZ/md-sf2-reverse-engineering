# Battle AI Decision Contract

- **Confirmed original behavior:** the complete 26-file AI inventory, action filters, static
  priority/healing/support/action/movement/control models, commandset routing, temporary-terrain
  cleanup, and the bounded 14-case final attack action/target runtime matrix described below.
- **Unknown original behavior:** the queued caller-visible filter/heal/support/movement/control
  cases, signed/overflow edges outside the accepted runtime matrix, natural-map path choice, complete
  multi-turn behavior, player-facing fairness or intent, and presentation/timing.
- Remake status: implementation-neutral Phase 3 contract; no AI architecture, difficulty redesign,
  explainability policy, or deliberate compatibility deviation has been selected.
- Evidence date: 2026-08-08
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the original static decision boundaries from AI commandset dispatch through
action-category, target, and movement outputs. It owns:

1. spell/item eligibility scans and their stop/continue asymmetries;
2. potential-damage, target-priority, healing, and support scoring inputs;
3. final attack action-category and target selection;
4. Move, Move Order, temporary terrain, and move-string handoffs;
5. top-level commandsets, activation/standby/swarm, and special-attacker routes;
6. explicitly bounded runtime observations for final attack action/target choice.

It does not own real combat/spell results, battlefield propagation internals, battle action
construction, player tactics, encounter balance, or presentation. Those remain with
[combat resolution](combat-resolution.md), [spell resolution](spell-resolution.md),
[battlefield navigation](battlefield-navigation.md),
[battle action construction](battle-action-construction.md), and
[battle control/lifecycle](battle-control-lifecycle.md).

The executable owners are:

- `sf2-battle-ai-static-v1` in `tests/fixtures/h2/battle-ai-static-v1.json`;
- `sf2-battle-ai-priority-static-v1` in
  `tests/fixtures/h2/battle-ai-priority-static-v1.json`;
- `sf2-battle-ai-healing-static-v1` in
  `tests/fixtures/h2/battle-ai-healing-static-v1.json`;
- `sf2-battle-ai-support-static-v1` in
  `tests/fixtures/h2/battle-ai-support-static-v1.json`;
- `sf2-battle-ai-action-choice-static-v1` in
  `tests/fixtures/h2/battle-ai-action-choice-static-v1.json`;
- `sf2-battle-ai-movement-static-v1` in
  `tests/fixtures/h2/battle-ai-movement-static-v1.json`;
- `sf2-battle-ai-remaining-static-v1` in
  `tests/fixtures/h2/battle-ai-remaining-static-v1.json`;
- `sf2-battle-ai-action-choice-runtime-v1` in
  `tests/fixtures/h3/battle-ai-action-choice-v1.json`.

The research owner is [Battle AI static inventory and decision contracts](../../research/battle-ai.md).

## Canonical Decision State

An adapter MUST preserve distinct ownership for:

| State | Contract role |
| --- | --- |
| commandset and command cursor | ordered command attempts; stop at first success |
| action candidates | physical, spell, and item viability plus selected spell/item entries |
| target candidates | per-family target bytes, priority bytes, and movement bytes |
| thinking RNG | low-byte `RANDOM_SEED_COPY` stream, separate from the base RNG |
| movement output | move string plus action field, including Stay-with-movement cases |
| temporary battlefield state | obstruction bits installed for path selection and cleared on every control exit |
| AI memory | last target, move count, and standby relative-position state |

The AI's potential-damage score is not actual damage, and a successful movement result need not mean
the action field is non-Stay. Collapsing these values into one `decision` object without typed
intermediate state loses confirmed original distinctions.

## Action Getter and Filter Contract

All five action getters scan at most four slots. No spell returns `SPELL_NOTHING = 0x3F`; no item
returns `ITEM_NOTHING = 0x7F`.

### Attack Spells

**Confirmed static:** ally casters and confused enemies accept only BLAZE, FREEZE, BOLT, BLAST,
KATON, and RAIJIN before requiring attack spell type 0. The ally path is forced through this filter
because its local confusion flag is unconditionally set. An unconfused enemy bypasses the spell-name
allowlist but still requires attack type. Rejection advances to the next slot; acceptance calls
`GetHighestUsableSpellLevel`.

The forced ally filter and its natural caller-visible consequences remain **Unknown** until the
queued ally/unconfused-enemy/confused-enemy runtime matrix exists.

### Attack Items

**Confirmed static:** an attack item must pass battle usability. Equipped entries bypass the AI-use
bit; unequipped entries require it. Ally/confused paths use the smaller BLAZE/FREEZE/BOLT/BLAST
allowlist and require attack type.

The rejection policy is asymmetric:

- a genuinely unusable item advances to the next slot;
- a missing AI-use bit, disallowed use spell, or non-attack use spell aborts the whole search with
  `ITEM_NOTHING`;
- healing-item rejection instead continues scanning;
- Healing Rain bypasses the AI-use-bit requirement; other healing items require it.

This is static control flow. Inventory-order consequences and caller retry behavior remain
**Unknown**.

## Priority Is a Scoring Model, Not Resolution

Physical potential damage is `max(current ATT - current DEF, 1)`, then multiplied and floored by
land setting: `256/256`, `230/256`, or `205/256`. Applying the minimum before terrain means a
one-point estimate can become zero. Spell scoring starts from definition power and applies only the
resistance setting: minor subtracts one quarter, major halves, and weakness adds one quarter. Area
spell priorities sum per affected target.

These are target-scoring estimates. They MUST NOT replace the real combat/spell formulas.

`pt_TargetPriorityScripts` contains 16 pointers indexed by difficulty times four plus activation
column. Allies force column 2. Enemy spell scoring masks two activation bits; regular attacks extract
a low nibble even though the table has only four columns. The four script shapes combine bounded
lethality, prior-target, HP-threshold, movement, class, and RNG terms as recorded by the executable
fixture.

Class adjustment applies only to non-confused allies. The previous-target byte equaling Sarah (ally
1) forces the mage adjustment table. Whether natural caller state reaches that condition as the
source names imply remains **Unknown**.

## Healing and Support Decisions

### Healing

**Confirmed static:** a confused caster exits. Healing Rain is tested before spells and is admitted
only when the first enemy combatant is at or below half HP; its action targets the caster because the
item spell is area-based. Otherwise only HEAL or AURA is accepted, with minimum MP gates 3 and 7.
Failure falls back to an ordinary healing item, and an item wins when both spell and item reach action
loading.

Living same-side targets require `3 * currentHP <= 2 * maxHP`; equality at two thirds qualifies. The
separately named half-HP helper also includes equality.

The healing-level helper returns:

| Missing HP | Static result |
| ---: | --- |
| 0-2 | do not cast |
| 3-14 | level 1 |
| 15-28 | level 3 if known, otherwise level 1 |
| 29+ | level 4 if known; otherwise level 3 if known, else level 1 |

Its MP fallback shifts by five instead of six and adds the packed spell entry without masking level
bits. It never returns level 2; a caller may reintroduce level 2 only through its separate override.
These defects are **Confirmed static**; MP/threshold caller-visible results and byte-score overflow
remain queued runtime questions.

### Support

**Confirmed static:** support is enemy-only and confused enemies Stay. Only the first support spell
is considered; if it is not MUDDLE 2 or DISPEL 1, the command does not scan later slots.

- MUDDLE 2 scores an area center by affected target count and removes centers below three.
- DISPEL adds one for each affected target with an attack or healing spell and removes centers below
  two.
- equal byte priority selects the later candidate.
- if the chosen center has no valid attack position, the command Stays instead of trying the next
  ranked center.

ATTACK and BOOST 2 dispatch branches exist after this admission gate but are unreachable through the
normal command entry. Their dormant defects remain source facts, not required reachable behavior.

## Final Attack Action and Target Choice

**Confirmed static and bounded runtime:** the final attack command records whether physical, spell,
and item lists are non-empty.

- no viable category returns Stay;
- physical alone attacks;
- a sole spell or item is selected, although the ordinary path still consumes `RNG(6)` before
  noticing that physical is unavailable;
- physical plus spell gives spell rolls 2 and 4, physical rolls 0, 1, 3, and 5;
- physical plus item gives item rolls 3 and 5, physical the other four;
- AQUA plus physical bypasses that roll and always casts AQUA;
- when both spell and item are viable, physical is ignored even when viable.

Spell-versus-item choice uses the thinking RNG, updating the low seed-copy byte as
`(seed * 541 + 12345) & 0xFF` until below two; 0 selects spell and 1 selects item.

Priorities are compared as signed bytes from an initial maximum of zero, so 128-255 can be ignored
as negative. The returned priority is capped at 15, while every target tied at the original maximum
is retained in reverse input order. Ordinary 0-127 movement tie-breaking selects the largest stored
movement value; equal values select the later collected target.

The 14-case H3 fixture replays one natural Battle 01 entry with controlled target lists and seeds. It
confirms all seven non-empty viability shapes, both RNG families/outcomes, AQUA bypass, ordinary
priority, largest-movement choice, and equal-movement later-target choice. It does **not** close
priority 127/128/255, movement above 127, critical class cohorts, or other caller states.

## Movement and Move-Order Contract

`aiCommand_Move` builds movement with budget 128. A confused unit directly chooses its side's first
index without checking HP or placement; the normal path collects living placed opponents and has no
empty-list guard before its cost loop. Costs sort ascending as unsigned bytes and the first entry is
selected.

Kraken Leg/Arm/Head use their own 16-byte cost table. After a preliminary move string with budget 4,
the command searches attack positions at radii 0 then 1. Failure changes the action to Stay while the
function still returns success.

`aiCommand_MoveOrder` tries Attack before movement. Zero MOV, absent order, dead follow target, or
failed terrain check produces Stay. Movement-only success is also encoded as Stay plus a non-empty
move string. Its builder uses movement-array budget 128, preliminary budget `MOV * 2`, and attack
position radii 0 through 3.

These are static input/output rules. Empty-list behavior, confused invalid targets, Kraken fallback,
ally-mode stack state, and natural path choice remain **Unknown**.

## Temporary Terrain and Quadrant Helpers

**Confirmed static:** move-order quadrant bit 0 means destination left and bit 1 means below, opposite
the upstream comments. Working bounds expand four tiles and clamp to the 48-by-48 terrain domain.

Temporary obstruction helpers use terrain bits 6 and 7 while leaving permanent `0xFF` cells intact.
Block-and-carve first blocks non-permanent terrain, then clears Manhattan rings 0-2, or rings 0-4
when tethered to a last target. Parsed ring tables contain 1, 4, 8, 12, and 16 entries. Every AI
control exit clears temporary obstruction flags.

Map-edge effects and caller-visible route choices have no accepted runtime owner and remain
**Unknown**. A modern pathfinder must not assume temporary terrain is ordinary permanent collision.

## Commandsets, Activation, and Special Attackers

AI-controlled allies use commandset 6. Enemies select among 16 commandsets and stop at the first
successful command. Commandsets 10/11 share Stay; 13/14 share the critical/leader set. The 18-entry
pathfinding-mode table selects regular, block-non-movable, or block-and-carve modes.

Before activation the controller clears newly triggered regions. No trigger regions starts active;
an inactive enemy runs standby and is forced to Stay. A dead primary follow order is replaced by its
secondary order.

Swarm commandset 15 waits only at full HP in battles 16, 20, or 22 and uses battle-specific threshold
tables. `CountDefeatedEnemies` incorrectly uses the ally subsection length while iterating enemies;
the caller-visible consequence is not runtime-closed.

Prism Flower and Zeon Guard bypass commandsets: with a facing target they choose prism-laser action 6
and the first target; otherwise they Stay. Burst Rock explodes only with at least one target and
thinking `RNG(6) == 4`; otherwise it executes Move 1 then forces action Stay while retaining movement.

All of these are **Confirmed static** routes, not statements about tactical purpose, fairness,
selection frequency, or animation.

## Dispatcher, Standby, and Unused Helpers

The dispatcher handles command values 0-7, 10-14, and 16-19. Reserved 8, 9, 15, and unknown values
return without selecting an action. Standby first uses thinking `RNG(8)`: 2, 4, and 6 Stay; the other
five rolls enter eligibility. Its packed memory retains move count and prior relative-position index.

The move-order standby branch copies returned X into both starting X and starting Y, and its initial
bounds check accepts coordinate 48. Downstream rejection and caller-visible effects remain
**Unknown**. The static 16-case eligibility model yields 11 Stay, 4 regular-move, and 1 move-order
configuration; this is not a runtime frequency distribution.

Five explicitly unused helpers have zero direct calls in the AI subtree. Their comparisons and
four-slot lookup contracts remain source facts, not mandatory reachable remake behaviors.

## Fidelity and Modernization Boundary

An original-fidelity AI adapter MUST preserve:

- commandset order, first-success dispatch, and temporary-terrain cleanup;
- getter scan limits, allowlists, rejection asymmetries, and packed-entry behavior;
- scoring units, byte signedness, tie collection, RNG stream ownership, and bounded H3 results;
- healing/support gates and no-fallback branches;
- Move/Move Order output distinctions, budgets, radius order, and temporary obstruction modes;
- swarm/special/standby source-shaped gates and explicitly bounded defects.

A future remake MAY replace commandsets with planners, improve pathfinding, fix defects, expose AI
reasoning, rebalance difficulty, use safer numeric types, or redesign special attackers. These are
product decisions, not original facts. Fixture-visible changes require explicit decisions and H4
expected-deviation records.

## H4 Acceptance Surface

A future H4 adapter should compare:

1. getter results and slot/abort behavior for source-shaped spell/item inventories;
2. priority/healing/support intermediate scores without substituting real damage;
3. all 14 action-choice runtime cases, including RNG consumption and target tie order;
4. Move/Move Order action plus move-string outputs and temporary-terrain cleanup;
5. commandset first-success traces, activation, swarm, special-attacker, and standby outputs;
6. declared deviations for static defects or unsafe/undefined caller states.

H4 MUST keep natural-play frequency, fairness, tactical intent, rendered behavior, and the itemized
grouped runtime queue outside the accepted adapter until separately evidenced or designed.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| inventory and spell/item filters | **Confirmed static** | `sf2-battle-ai-static-v1` ([`battle-ai-static-v1.json`](../../../tests/fixtures/h2/battle-ai-static-v1.json)) | Caller retry/order results and natural reachability |
| priority and potential-damage scoring | **Confirmed static** | `sf2-battle-ai-priority-static-v1` ([`battle-ai-priority-static-v1.json`](../../../tests/fixtures/h2/battle-ai-priority-static-v1.json)) | Signed/overflow edges, script reachability, intent |
| healing and support | **Confirmed static** | `sf2-battle-ai-healing-static-v1` ([`battle-ai-healing-static-v1.json`](../../../tests/fixtures/h2/battle-ai-healing-static-v1.json)) and `sf2-battle-ai-support-static-v1` ([`battle-ai-support-static-v1.json`](../../../tests/fixtures/h2/battle-ai-support-static-v1.json)) | Threshold/MP/overflow and no-fallback runtime effects |
| final attack action/target model | **Confirmed static** | `sf2-battle-ai-action-choice-static-v1` ([`battle-ai-action-choice-static-v1.json`](../../../tests/fixtures/h2/battle-ai-action-choice-static-v1.json)) | Critical cohorts and signed movement/priority edges |
| final attack action/target runtime seam | **Confirmed runtime, bounded** | `sf2-battle-ai-action-choice-runtime-v1` ([`battle-ai-action-choice-v1.json`](../../../tests/fixtures/h3/battle-ai-action-choice-v1.json)) | Other callers, maps, commandsets, and queued edge cases |
| Move and Move Order | **Confirmed static** | `sf2-battle-ai-movement-static-v1` ([`battle-ai-movement-static-v1.json`](../../../tests/fixtures/h2/battle-ai-movement-static-v1.json)) | Natural-map routes, empty/invalid targets, ally-mode stack state |
| dispatcher, terrain helpers, commandsets, swarm, specials, standby, unused | **Confirmed static** | `sf2-battle-ai-remaining-static-v1` ([`battle-ai-remaining-static-v1.json`](../../../tests/fixtures/h2/battle-ai-remaining-static-v1.json)) | Caller-visible defects, frequency, presentation, complete multi-turn behavior |
| tactics, fairness, balance, presentation | **Unknown / product decision** | No aggregate executable owner | Requires user research, design decisions, and separate acceptance |

## Reproduction

```powershell
uv run sf2 h2 battle-ai
uv run sf2 h3 battle-ai-action --timeout-seconds 150
uv run sf2 design-contracts test
uv run sf2 research-index test
```
