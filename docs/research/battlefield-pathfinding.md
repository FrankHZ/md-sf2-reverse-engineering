# Battlefield and Pathfinding

- Status: **Confirmed** for the pinned 17-file source inventory, representative entry symbols,
  source hashes, static call-edge counts, core grid/RAM layout, initialization, occupancy rules,
  movement-neighbor admission, Manhattan range rings, and target-side/admission rules
- Status: **Inferred** for algorithm names and roles that currently rely on upstream labels/comments
- Status: **Unknown** for complete bucket propagation, edge-memory effects, and runtime ambiguities
  until later focused models and the queued H3 matrix are complete
- Evidence date: 2026-07-18
- ROM: USA retail, SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Scope

The first battlefield slice inventories all 17 ASM files under
`code/gameflow/battle/battlefield`. It records every file hash, global/local label count, direct
call site, and one H1-bound representative symbol per file. This establishes complete **file
reach** for the directory; it does not claim that all 126 global labels or 1,167 statements are
semantically understood.

The canonical inventory contains 2,299 source lines, 126 global labels, 94 local labels, 116 direct
call sites, and 45 unique direct targets. Of those targets, 26 resolve inside the directory and 19
cross the subsystem boundary. Eighteen research-index records currently touch the directory: the
17 inventory footholds plus the earlier `PopulateTargetsListForSpell` runtime contract.

## Confirmed Inventory Boundary

The 17 source files divide into five static work groups:

1. coordinate conversion and common battlefield accessors;
2. movement-array initialization, occupancy projection, and movable-grid updates;
3. attack/action range grids, target lists, and reachable-target filtering;
4. movement-array propagation, destination selection, and move-string reconstruction;
5. move-order positioning and trapped-chest handling.

The tracked fixture binds representative ROM entries ranging from
`GetMoveStringDestination` at `0x00C024` through the late-bank
`CheckForTrappedChest` at `0x1B16FE`. The inventory verifier also pins each source file's SHA-256,
so upstream label, call-graph, or file-set drift fails deterministically.

## Core Grid and Movement Contract

The battlefield arrays share a fixed 48×48 row-major grid: offset = `y * 48 + x`, for 2,304 bytes
or 576 longwords per array. The core RAM bases are:

| Array | Address | Initialization/use |
| --- | ---: | --- |
| total move costs | `0xFF4400` | cleared to `0xFF`; origin and accepted destinations receive costs |
| movable grid | `0xFF4D00` | cleared to `0xFF`; non-negative bytes mark processed/reachable spaces |
| targets grid | `0xFF5600` | cleared to `0xFF`; stores occupying combatant indexes |
| battle terrain | `0xFF5F00` | terrain type plus impassable/occupied flag bits |
| current move-cost table | `0xFFB6C2` | 16 terrain-type costs for the moving combatant |

`InitializeMovementArrays` exposes these pointers and doubles current MOV to form the pathfinder's
budget. `BuildMovementArrays` clears both 2,304-byte movement grids, uses 32 remaining-budget
buckets in a 64-byte stack frame, and inspects neighbors in right, left, up, down order. A neighbor
is rejected when its offset is outside the 2,304-byte array, terrain bit 7 is set, or its signed
move cost is negative/greater than the remaining budget. Spending the budget exactly writes the
final cost without queueing another candidate; otherwise the candidate goes into
`(remainingBudget - moveCost) & 0x1F`.

Occupancy updates scan 30 ally or 32 enemy slots, skipping dead combatants and unsigned coordinates
outside `[0, 48)`. Terrain byte `0xFF` is never changed. Setting occupancy sets bit 7; clearing it is
suppressed when impassable bit 6 is set, preserving temporary/combined obstructions. The fixture
contains explicit transformations for ordinary, impassable, and fully obstructed terrain bytes.

## Range and Target Construction

`pt_SpellRanges` at `0x00C590` owns four ordered Manhattan rings: radius 0/1/2/3 contain
1/4/8/12 signed coordinate pairs. Attack or spell range builders walk the requested rings from
maximum down to minimum. The unarmed default is 1–1, with hardcoded exceptions for Brass Gunner
(1–2), Kraken Arm (1–2), and Kraken Head (1–3); equipped weapons and spells read min/max directly
from their definitions.

Spell property bit 6 means “target teammates”; clear means “target opponents.” Combined with caster
affiliation this yields the full ally/enemy × clear/set four-case matrix. The map-aligned target grid
accepts only living, non-neutral combatants whose unsigned coordinates are below 48. When applying a
relative ring, obstructed terrain can suppress the movable-grid mark but does not suppress the
occupant lookup, an important distinction for separating visible range from legal entity targets.

Burst Rock builds the target map from both factions and omits its center ring to avoid self-targeting.
AURA 4 and SHINE bypass rings and enumerate the supplied target's whole faction, requiring a placed, living
combatant but not applying the neutral-bit filter. Reachable-target enumeration normally scans the
opposing roster; confusion flips the actor affiliation first. Each accepted target stores the low
byte of total movement cost in a parallel 48-byte list.

## Attack Position and Move Strings

`DetermineAttackPosition` scans a Manhattan annulus from top to bottom and left to right. It returns
immediately if the actor already stands at an in-range zero-cost position; otherwise it rejects
word-bit-15 obstruction, occupied destinations, and out-of-map candidates. Candidate cost starts at
`0xFF`, and the instruction comparison replaces it only with a strictly lower unsigned low byte, so
equal costs retain the first scanned coordinate. This conflicts with the upstream prose saying that
the function prefers higher move cost; the executable comparison is preserved as the Confirmed fact
and the prose disagreement remains explicit.

Move strings at `0xFF9804` use direction bytes right=0, up=1, left=2, down=3 and `0xFF` as terminator;
any other code also stops destination replay. Backtracking checks right, left, up, down and keeps the
lowest combined grid cost no greater than current minus one. When equal-cost alternatives exist it
avoids repeating the previous backtrack direction. The AI form reverses the constructed string and
maps each byte to its opposite with XOR 2. The partial builder stops at
`max(destinationCost - movementBudget, 0)`.

One static hazard is now queued for concentrated runtime work: neighbor bytes are read before the
2,304-byte bounds check, so border paths can touch the adjacent RAM byte even though the candidate is
then rejected. No runtime effect is claimed yet.

## Evidence Limits

- **Confirmed:** directory/file set, source metrics, named entry addresses, source hashes, and
  syntactic direct-call relationships reproduced by the Python rail.
- **Inferred:** broad grouping above, because it follows instruction flow and the pinned upstream
  symbol vocabulary but is not yet represented as a project-owned behavioral model.
- **Unknown:** full budget-bucket propagation for competing weighted paths, the observable effect of
  pre-check border reads, overflow/signedness edges outside normal stats, and whether late helpers
  have reachable original-game callers.

Static parsing owns those questions first. Only timing, persistence, caller-context, hardware, or
otherwise irreducible ambiguities will enter a shared BizHawk matrix.

## Reproduction

```powershell
uv run sf2 h2 battlefield
uv run sf2 research-index test
```

The H2 command validates the source inventory and fixture schemas, pinned upstream commit, ROM
provenance, representative labels, summary counts, and canonical output hash. Generated JSON is
written only to ignored `local/derived/battlefield-static.json`; the accepted SHA-256 is
`3E22603768377E9585B5F9CBF61463A7CA8230F911D0C067347380D73ED4910A`.

## Next Static Batches

The next passes will model complete bucket propagation, move-order positioning, and trapped-chest
handling. Runtime questions remain a queue until those models expose a compact branch matrix worth
launching together.
