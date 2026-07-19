# Common Map Engine

- Status: **Confirmed** for the pinned seven-file inventory, flag-switched maps, battle-trigger
  admission, egress/savepoint selection, 8 KiB layout decompression boundary, and map VInt gates
- Status: **Inferred** for presentation intent in the large camera and loader helpers
- Status: **Unknown** for exact camera/scroll timing, VDP-visible animation results, and map data
  content outside the loader contracts
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Confirmed Map Routing

All seven files under `code/common/maps` are inventoried and H1-bound. `SwitchMap` scans six-byte
records until a negative source-map value and takes the first source-map entry whose flag is set.
`CheckBattle` accepts `-1` as current map, requires the battle-unlocked flag, treats `-1` trigger X/Y
as wildcards, writes the battle rectangle, and returns `-1` when no record matches. A completed match
clears its unlocked flag.

Before flag 399, egress returns the hardcoded game-start map/coordinates/facing. Otherwise it scans
four-byte savepoint records with a `-1` terminator, defaulting to `(1,1,UP)` if absent. Raft reset
requires flag 64 and uses a second four-byte map/coordinate table.

## Confirmed Loading Boundary

`LoadMapLayoutData` clears its history maps and produces exactly `$2000` bytes. Its top-level modes
emit a new block, copy a run, or reuse left/upper history. New-map load clears scroll state, updates
`CURRENT_MAP`, then loads blocks before layout; battle maps additionally apply the battle-area
overlay. This documents decompressor shape and ordering, not the semantics of each map's content.

Map VInt bit 0 updates plane A and refreshes window layout when present; bit 1 updates plane B. Tile
animation requires a positive data pointer, counts down, and performs VInt DMA. The large camera state
machine and unused randomized loader are hash/call inventoried, while exact camera and VDP timing stay
in the grouped presentation runtime queue. This batch adds no emulator run.

## Reproduction

```powershell
uv run sf2 h2 common-maps
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/common-maps-static.json`.
