# Battlefield and Pathfinding

- Status: **Confirmed** for the pinned 17-file source inventory, representative entry symbols,
  source hashes, and static call-edge counts
- Status: **Inferred** for algorithm names and roles that currently rely on upstream labels/comments
- Status: **Unknown** for exact movement-array layouts, propagation/tie-break semantics, and runtime
  edge cases until the focused static models and queued H3 matrix are complete
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
`D6047F2E6968E3B6BA897C5BA934FD3DCA31B2EE880533FFA30B5AE0FE6080B4`.

## Next Static Batches

The next passes will model movement-array initialization and layouts, occupancy updates and grid
propagation, target/range construction, and move-string reconstruction. Runtime questions remain a
queue until those models expose a compact branch matrix worth launching together.
