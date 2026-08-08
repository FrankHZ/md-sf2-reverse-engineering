# Battlefield Navigation and Target-Space Contract

- **Confirmed original behavior:** the fixed battlefield grids, coordinate conversion, movement-array
  initialization, occupancy projection, weighted propagation and admission rules, Manhattan range
  rings, target-side filtering, attack-position selection, move-string encoding, and the bounded
  five-case original-runtime movement matrix described below.
- **Unknown original behavior:** whether ordinary shipped battle states expose horizontal row
  crossing or caller-visible effects from pre-check out-of-range reads, arithmetic edges outside the
  observed stat domain, natural-map route choice beyond the accepted seams, and rendered cursor,
  range, movement, or timing behavior.
- Remake status: implementation-neutral Phase 3 contract; no engine, navigation library, or deliberate
  compatibility deviation has been selected.
- Evidence date: 2026-08-08
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the original battlefield-space data and algorithm boundaries that a fidelity
adapter can reproduce without treating source labels as tactics, AI intent, or modern engine
architecture. It separates:

1. terrain and occupancy input;
2. movement-cost and reachable-space state;
3. action-range and target-list state;
4. destination and move-string construction;
5. presentation and player-facing policy, which remain outside the confirmed boundary.

The primary evidence owners are:

- `sf2-battlefield-static-v1` in
  `tests/fixtures/h2/battlefield-static-v1.json`;
- `sf2-battlefield-movement-runtime-v1` in
  `tests/fixtures/h3/battlefield-movement-matrix-v1.json`;
- `sf2-battle-terrain-decode-v1` in
  `tests/fixtures/h2/battle-terrain-decode-v1.json`;
- [battlefield/pathfinding research](../../research/battlefield-pathfinding.md), which owns the
  evidence limits and reproduction context.

These fixtures establish source/ROM structure and bounded runtime observations. They do not establish
encounter-design intent, path desirability, normal-play reachability of controlled edge cases, or
rendered behavior.

## Canonical Battlefield State

**Confirmed:** the core arrays use one fixed 48-by-48 row-major address space. A coordinate maps to
`offset = y * 48 + x`, yielding 2,304 bytes or 576 longwords per byte grid.

| State | Original RAM base | Contract role |
| --- | ---: | --- |
| total move costs | `0xFF4400` | accumulated cost for the origin and admitted destinations |
| movable grid | `0xFF4D00` | processed/reachable-space state and temporary bucket links |
| targets grid | `0xFF5600` | occupying combatant indexes aligned to battlefield offsets |
| battle terrain | `0xFF5F00` | terrain type plus obstruction/occupancy bits |
| current move-cost table | `0xFFB6C2` | sixteen terrain-type costs for the current mover |
| move string | `0xFF9804` | encoded movement directions terminated by `0xFF` |

An adapter MUST keep terrain, occupancy, accumulated cost, reachable-space state, targets, and move
strings as distinguishable values. A single `walkable` or `selected` flag cannot losslessly represent
the original contract.

The shipped terrain corpus contains 45 battle selections resolving to 43 unique decoded 48-by-48
payloads. Battles 4 and 32 alias the payloads owned by battles 3 and 27. Every decoded byte is terrain
type 0 through 8 or `0xFF`. This proves the shipped static input domain, not the runtime reachability of
every coordinate from every battle state.

## Movement Initialization and Admission

**Confirmed static contract:** `InitializeMovementArrays` exposes the core pointers and doubles the
current MOV value to produce the propagation budget. `BuildMovementArrays` clears both 2,304-byte
movement grids to `0xFF`, admits the origin at cost zero, and examines neighbors in this order:
right, left, up, down.

A neighbor is rejected when:

- its flat offset is outside `[0, 2304)`;
- terrain bit 7 marks it occupied or obstructed;
- the selected terrain cost is signed-negative or exceeds the remaining budget.

Spending the budget exactly writes the final total cost and reachable marker without queueing the
destination for another expansion. Otherwise the destination is queued by remaining budget. The
original uses flat offsets `+1`, `-1`, `-48`, and `+48`; it does not independently reject an X-axis
row boundary before applying the whole-array bounds check.

**Confirmed occupancy contract:** occupancy projection scans 30 ally slots or 32 enemy slots. It
skips dead combatants and unsigned coordinates outside `[0, 48)`. Setting occupancy sets terrain bit
7. Clearing occupancy leaves bit 7 set when impassable bit 6 is also set, and the fully obstructed
byte `0xFF` is never changed. Occupancy mutation is therefore a terrain-state transformation, not a
separate proof of visible collision or path-preview behavior.

## Weighted Propagation

**Confirmed static and bounded runtime contract:** propagation uses a 64-byte stack frame as 32
two-byte LIFO bucket heads. The selected bucket is `remainingBudget & 31`; the 32 buckets are not a
32-point movement cap. A queued cell temporarily stores the previous two-byte bucket head across its
movable-grid and total-cost bytes. Popping restores that head. The processed cost is
`initialBudget - remainingBudget`.

The original first-admission discipline does not perform general cost relaxation after a cell has
been admitted. A fidelity implementation MUST preserve the accepted order, byte values, bucket wrap,
and tie behavior at fixture seams rather than substituting an arbitrary shortest-path library and
assuming equivalent intermediate state.

The runtime fixture replays five controlled cases through one original
`InitializeMovementArrays -> BuildMovementArrays` call in a shared Battle 01 in-memory state. It
changes the mover's X, Y, and current MOV before the original register loads, then installs controlled
terrain, move-cost, and guard bytes at the build entry. Original pointer setup, stack buckets,
neighbor probes, propagation, and return still execute.

| Runtime case | Confirmed observation |
| --- | --- |
| uniform cost 2, budget 4 | 13 reachable cells; the expansion prefix is `1176, 1224, 1128, 1175, 1177` |
| mixed weights, budget 4 | 5 reachable cells; probe costs are `4, 1, 0, 3, 4` in fixture order |
| budget-128 corridor | 41 reachable cells; costs 31, 32, and 40 survive bucket-index wrap; 42 out-of-range helper entries are observed |
| X=47 flat right neighbor | with start offset 95 and budget 2, offset 96 is admitted at cost 2 and the result contains 5 reachable cells |
| array-end origin | with start offset 2303 and budget 2, only the origin is reachable; two out-of-range helper entries are observed |

The H2 model separately includes a flat-row example with budget 1 and terrain cost 1. It MUST NOT be
silently merged with the runtime case's budget 2 and cost 2; both demonstrate the same flat-neighbor
shape with different fixture units.

The observed helper enters before rejecting some out-of-range offsets. This is a confirmed chronology
at the controlled seam, not evidence of a player-visible corruption. A memory-safe remake is not
required to perform unsafe reads; any fidelity decision about callback chronology or the resulting
state must be explicit in its H4 adapter or expected-deviation record.

## Range and Target Construction

**Confirmed static contract:** four ordered Manhattan rings at `pt_SpellRanges` contain 1, 4, 8, and
12 signed coordinate pairs for radii 0, 1, 2, and 3. Range builders walk requested rings from maximum
to minimum. The unarmed default attack range is 1-1, with source-owned exceptions for Brass Gunner
1-2, Kraken Arm 1-2, and Kraken Head 1-3; equipped weapons and spells supply their own minimum and
maximum values.

Spell property bit 6 selects the target side:

| Caster side | Bit 6 clear | Bit 6 set |
| --- | --- | --- |
| ally | enemies | allies |
| enemy | allies | enemies |

The ordinary target grid admits placed, living, non-neutral combatants with unsigned coordinates
below 48. When a ring position is obstructed, the routine may suppress the movable-grid mark while
still looking up an occupant; visible range and legal entity targets are not the same state.

Burst Rock builds from both factions and omits its center ring. AURA 4 and SHINE enumerate the
supplied target's whole faction, require placed living combatants, and do not apply the ordinary
neutral-bit filter. Reachable-target enumeration normally scans opponents; confusion first flips
actor affiliation. Each admitted target stores the low byte of its total movement cost in a parallel
48-byte list.

This contract preserves target-side and admission mechanics. It does not define a preferred target,
AI scoring intent, menu ordering, highlight color, or complete runtime behavior for every action.

## Destination and Move-String Construction

**Confirmed static contract:** `DetermineAttackPosition` scans a Manhattan annulus from top to bottom
and left to right. It returns immediately when the actor already occupies an in-range zero-cost
position. Other candidates must be in range, unoccupied, and free of the word-bit-15 obstruction
signal. The selected cost begins at `0xFF` and is replaced only by a strictly lower unsigned low byte;
equal costs retain the first scanned coordinate.

Pinned upstream prose describes a higher-cost preference, but the executable comparison and fixture
record establish the lower-cost rule. An implementation MUST preserve the executable rule and keep
the source-comment disagreement visible rather than choosing the convenient interpretation.

Move strings encode right=0, up=1, left=2, and down=3. `0xFF` terminates replay, and any other
unrecognized code also stops it. Backtracking examines right, left, up, down and chooses the lowest
combined grid cost no greater than current cost minus one. On equal-cost alternatives it avoids
repeating the previous backtrack direction when another candidate exists. The AI form reverses the
constructed string and maps directions to their opposites with XOR 2. A partial path stops at
`max(destinationCost - movementBudget, 0)`.

The source reads a neighbor byte before its 2,304-byte bounds check. As with propagation, this
chronology is an original observation boundary, not authorization to reproduce unsafe memory access
in a modern implementation.

## Adjacent Move-Order and Trapped-Chest Boundaries

The shared static fixture also closes three late battlefield helpers that consume the same spatial
state. They remain separate from the core propagation algorithm:

- move-order bit 6 selects the coordinate source. Clear treats the command value as a combatant to
  follow; set uses the low nibble as an index into two-byte AI-point coordinates;
- the move-order terrain helper ignores its nominal target-type argument and returns `0xFF` only when
  current terrain bit 7 is set; it does not test impassable bit 6 separately;
- a trapped-chest match requires low-byte X/Y equality, activation bitfield exactly `0x0200`, both
  trigger regions exactly 15, and maximum HP zero in a 12-byte enemy entry. A match resets spawning
  enemy stats; no match returns `0xFFFF`.

These are **Confirmed static** conjunctions and selector rules. Their ordinary-game callers,
presentation, timing, and encounter purpose remain **Unknown**; they do not authorize a generic
"hidden enemy" or "chest trap" rule with looser admission.

## Fidelity and Modernization Boundary

An original-fidelity adapter MUST preserve:

- the 48-by-48 flat coordinate domain and independently owned arrays;
- byte/sign/bit semantics at movement admission and occupancy seams;
- ordered neighbor expansion, 32-bucket LIFO behavior, and accepted tie rules;
- ring order, target-side filtering, target-list admission, and low-byte cost storage;
- destination scan order and move-string direction/termination rules;
- the exact inputs, outputs, units, and observation boundaries of the named fixtures.

A future remake MAY deliberately add X-axis boundary checks, use memory-safe neighbor access, replace
internal storage, expose clearer range previews, or adopt a different pathfinding implementation.
Such a change is not evidence about the original. It requires an explicit decision plus H4
expected-deviation coverage showing which externally relevant results remain compatible and which do
not.

## H4 Acceptance Surface

A future H4 adapter should consume the three fixtures named in this contract and expose enough state
to compare:

1. grid dimensions, coordinate conversion, terrain domain, and array initialization;
2. admitted offsets, total costs, reachable markers, and deterministic expansion order for the five
   runtime cases;
3. occupancy set/clear transformations, including bit-6 and `0xFF` boundaries;
4. ring coordinates, side-selection matrix, target admission, and parallel cost bytes;
5. attack-position scan/tie results and move-string encoding/replay;
6. declared expected deviations for flat row crossing or pre-check chronology.

H4 MUST compare canonical facts rather than original RAM addresses when the remake has no equivalent
memory map. Pixel output, input timing, cursor motion, path-preview presentation, AI intent, encounter
balance, and natural-story reachability remain outside this adapter until separately evidenced.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| grid, arrays, occupancy, range, targeting, destination, move strings | **Confirmed static** | `sf2-battlefield-static-v1` ([`battlefield-static-v1.json`](../../../tests/fixtures/h2/battlefield-static-v1.json)) | Complete runtime callers, natural-map route choice, presentation |
| shipped terrain input domain | **Confirmed static** | `sf2-battle-terrain-decode-v1` ([`battle-terrain-decode-v1.json`](../../../tests/fixtures/h2/battle-terrain-decode-v1.json)) | Runtime origin/obstruction combinations and normal-play reachability |
| weighted propagation and controlled edge chronology | **Confirmed runtime, bounded** | `sf2-battlefield-movement-runtime-v1` ([`battlefield-movement-matrix-v1.json`](../../../tests/fixtures/h3/battlefield-movement-matrix-v1.json)) | Shipped exposure, caller-visible pre-check effects, arithmetic outside observed domain |
| tactics, intent, path quality, UI, timing, rendering | **Unknown** | No accepted executable owner | Requires separate research or future product decisions |

## Reproduction

```powershell
uv run sf2 h2 battlefield
uv run sf2 h2 battle-terrain
uv run sf2 h3 battlefield-matrix --timeout-seconds 180
uv run sf2 design-contracts test
uv run sf2 research-index test
```
