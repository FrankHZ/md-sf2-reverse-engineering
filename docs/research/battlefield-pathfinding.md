# Battlefield and Pathfinding

- Status: **Confirmed** for the pinned 17-file source inventory, representative entry symbols,
  source hashes, static call-edge counts, core grid/RAM layout, initialization, occupancy rules,
  and movement-neighbor admission
- Status: **Inferred** for algorithm names and roles that currently rely on upstream labels/comments
- Status: **Unknown** for full propagation/tie-break behavior, range/target construction, move-string
  reconstruction, and runtime edge cases until later focused models and the queued H3 matrix are complete
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

## Evidence Limits

- **Confirmed:** directory/file set, source metrics, named entry addresses, source hashes, and
  syntactic direct-call relationships reproduced by the Python rail.
- **Inferred:** broad grouping above, because it follows instruction flow and the pinned upstream
  symbol vocabulary but is not yet represented as a project-owned behavioral model.
- **Unknown:** grid dimensions and memory ownership at every caller, sentinel meanings, neighbor
  visitation order, equal-cost tie-breaking, overflow/signedness edges, and whether late helpers
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
`E4CB5515B404D16700DB1FD2A4759DAB7EA7924A081BB3FFEAFC4AB54721B3D5`.

## Next Static Batches

The next passes will model the complete bucket propagation and tie-break behavior, target/range
construction, and move-string reconstruction. Runtime questions remain a queue until those models
expose a compact branch matrix worth launching together.
