# Battlefield and Pathfinding

- Status: **Confirmed** for the pinned 17-file source inventory, representative entry symbols,
  source hashes, static call-edge counts, core grid/RAM layout, initialization, occupancy rules,
  weighted movement propagation, Manhattan range rings, target-side/admission rules, and the batched
  runtime movement-boundary matrix
- Status: **Inferred** for algorithm names and roles that currently rely on upstream labels/comments
- Status: **Unknown** for whether shipped map padding exposes flat row crossing to ordinary gameplay,
  overflow/signedness outside normal stats, and late helpers without known original-game callers
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
cross the subsystem boundary. The research index binds the 17 inventory footholds plus supporting
cross-owner battle-loop, terrain, spell-range, and decompression records to this document. That
evidence relationship evolves with accepted work; [`research-index.json`](../../manifests/research-index.json)
and `uv run sf2 research-index test` are the durable current source rather than a copied record total.

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

The 64-byte stack frame is 32 two-byte LIFO bucket heads, not a 32-point movement cap. A queued
cell temporarily stores the previous head's two pointer bytes across movable-grid and total-cost
entries; the bucket head is then replaced with that cell's flat offset. Popping restores the prior
head from those bytes. Bucket selection is `remainingBudget & 31`, so the structure cycles for the
AI's hardcoded budget 128 as well as ordinary `MOV*2`. The processed value is
`initialBudget - remainingBudget`; exact-budget destinations are marked reachable but never queued.

The project-owned model reproduces this linked-list discipline and has deterministic tests for a
uniform cost-2 diamond, LIFO expansion order, occupied/unaffordable rejection, a 41-cell corridor at
budget 128, and flat horizontal boundary crossing. A five-case H3 matrix now reproduces the same
reachable counts, costs, and expansion order in the original ROM. The source uses neighbor offsets
`+1, -1, -48, +48` with only whole-array bounds, so right from X=47 addresses the next row's X=0;
the controlled runtime case confirms that this flat neighbor becomes reachable. Whether shipped
battle terrain and caller setup always mask it in ordinary play remains unknown.

## Shipped Terrain Corpus

The complete 45-entry battle-terrain pointer table resolves to 43 unique Stack-compressed payloads;
battles 4 and 32 alias payloads 3 and 27. A maintained Python decoder verifies the pointer table and
compressed bytes against the ROM, then produces exactly 2,304 bytes per unique payload. Across all
99,072 decoded bytes, the only values are terrain types 0-8 and `0xFF` obstruction. This establishes
the shipped input domain for the 48×48 battlefield array without committing any decoded grid.

This corpus proof closes initial terrain selection/decompression and the alias behavior. It does not
close the existing gameplay-reachability question for flat row crossing: that depends on where the
shipped obstruction boundaries and combatant origins meet during actual battle states.

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

The neighbor helper reads the target byte before its 2,304-byte bounds check. The concentrated H3
matrix confirms that an array-end origin enters the helper twice with out-of-range offsets (right and
down), but both candidates are then rejected and only the origin remains reachable. The budget-128
top-row corridor enters the helper 42 times out of range: once to the left of the origin and once
above each of 41 expanded cells. This confirms the pre-check read order under controlled RAM guards;
it does not establish a gameplay-visible corruption or map-specific effect.

## Concentrated Runtime Matrix

The runtime fixture replays five cases from one saved Battle 01 core state and uses one BizHawk
launch. Each replay enters the natural `InitializeMovementArrays` call, changes only the moving
combatant's position/MOV before the original register loads, and installs controlled terrain and
move-cost bytes at `BuildMovementArrays`. Original pointer setup, stack buckets, propagation, helper
calls, and return execute unchanged.

| Case | Confirmed result |
| --- | --- |
| uniform cost 2, budget 4 | 13 reachable cells; LIFO expansion order matches the Python model |
| mixed weights/two routes | 5 reachable cells; costs preserve the lower-cost route |
| budget 128 corridor | 41 reachable cells; costs 31, 32, and 40 survive the 32-bucket wrap |
| X=47 flat right neighbor | offset 96 is reachable, confirming row crossing in controlled terrain |
| offset 2303 origin | only the origin is reachable; two out-of-range helper entries are observed |

The corridor additionally records all 42 expected out-of-range helper entries. These counts are
derived independently from the Python expansion order, then compared with the ROM observation.

## Move Orders and Trapped Chests

Move-order bit 6 selects the coordinate source. Clear treats the command value as the combatant to
follow; set uses the low nibble as an index into two-byte AI-point coordinates. The terrain helper's
nominal target-type argument is unused: it returns `0xFF` only when current terrain bit 7 is set and
does not test impassable bit 6 separately.

The trapped-chest helper scans 12-byte entries in the battle's enemy subsection starting at combatant
128. A match requires low-byte X/Y equality, activation bitfield exactly `0x0200`, both trigger
regions exactly 15 (“none”), and maximum HP zero. It resets spawning-enemy stats and returns that
combatant index; empty/no-match cases return `0xFFFF`. These are conjunctions, not loose hidden-bit
or dead-enemy tests.

## Evidence Limits

- **Confirmed:** directory/file set, source metrics, named entry addresses, source hashes, and
  syntactic direct-call relationships reproduced by the Python rail; weighted propagation, 32-bucket
  wrap, flat row crossing, and pre-check out-of-range helper entry reproduced by the H3 matrix.
- **Inferred:** broad helper roles that still rely on pinned upstream labels/comments rather than a
  project-owned runtime or data contract.
- **Unknown:** whether ordinary shipped maps expose horizontal row crossing or a caller-visible effect
  from pre-check reads, overflow/signedness edges outside normal stats, and whether late helpers have
  reachable original-game callers.

Static parsing owns those questions first. Only timing, persistence, caller-context, hardware, or
otherwise irreducible ambiguities will enter a shared BizHawk matrix.

## Reproduction

```powershell
uv run sf2 h2 battlefield
uv run sf2 h2 battle-terrain
uv run sf2 h3 battlefield-matrix
uv run sf2 research-index test
```

The H2 command validates the source inventory and fixture schemas, pinned upstream commit, ROM
provenance, representative labels, summary counts, and canonical output hash. Generated JSON is
written only to ignored `local/derived/battlefield-static.json`; the accepted SHA-256 is
`C99514D9191D12D70BD2F99E95A61318D2BAB218B2B6232852D764BBE990FC0B`.

The H3 command validates five generated cases against the project-owned movement model and the
original ROM in one BizHawk launch. The next battlefield ambiguity is the conflicting
attack-position prose versus lower-cost instruction path; it can wait for a future shared grid
matrix while static-first work advances into the next subsystem.
