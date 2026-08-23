# Map 3 to Battle 01 Admission — Static Contract

- Status: **Confirmed** H2 static contract; it is not an H3 observation or readiness claim.
- Fixture: `sf2-map3-battle01-admission-static-v1`
- Reproduction: `uv run sf2 h2 map3-battle01-admission`
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Scope and Boundary

This rail begins only at the accepted terminal projection of
[`map3-castle-battle-unlock`](./map3-castle-battle-unlock.md): Map 21 player `(5,15)`, facing
Down, after `cs_53EF4`'s source-defined F401/F256 semantics and with the retained R2b fixture and
route-graph digests checked. It derives a legal extension through Maps 21 and 40, then the static
Battle 01 admission and initialization spine. It does not promote that extension to natural execution.

The public fixture is recursively closed to identifiers, coordinates, flags, addresses, structural
command IDs/operands, hashes, topology, derived route counts, and the stated Unknown boundary. It
contains no source prose, raw ROM/H1/layout payload, asset, capture, movie, state, input log, H3 case,
runtime observation, cadence, callback, restoration, or presentation payload.

## Confirmed Static Findings

All Map 21 and Map 40 warp rows are parsed and source/H1/ROM checked. The selected links are Map 21
`(9,1)` → Map 40 `(4,30)`, facing Up, and Map 40 wildcard `y=12` → Map 57 `(8,18)`, facing Up.
Geometry, collision, occupancy, and area bounds derive the shortest selected extension: 40 route nodes
and 38 logical inputs, SHA-256
`F6D0835A9027D64BD1799735FB0C5460D095DA9474D99CA8C171E972F6386C73`.
Map 40's legal wildcard terminals have input counts `(4,12):20`, `(5,12):21`, `(14,12):28`,
`(15,12):29`, and `(25,12):39`; the selected endpoint is `(4,12)`. Map 40's selected setup entity
list is empty, while the retained R2b post-program Map 21 entity occupancy remains `(6,16)`.

`MainLoop` statically orders `SwitchMap`, `CheckBattle`, conditional `BattleLoop`, then
`ExplorationLoop`. Battle-map row index 1 matches Map 57, area `(0,0,16,20)`, with wildcard trigger
coordinates. `CheckBattle` gates it on F401, checks F501 through the +100 completion offset, writes
the battle area, and returns battle index 1 in `d7`.

The clear-F88 `BattleLoop` branch sets current Map/Battle to 57/1, clears seconds, installs base VInt
functions, runs the before-battle route, clears F90–F105, heals and initializes allies/enemies, clears
AI memory, calls `LoadBattle` with `d0=0`, and routes the battle-start cutscene. The selected
before-battle row targets `bbcs_01` at `0x494BC` / 300220; its public record is limited to structural
command IDs, operands, and a hash. Battle-start row 1 targets `ms_Empty`; it checks and sets F451
before the empty map-script program.

`LoadBattle` at `0x25610` / 153104 structurally loads `CURRENT_MAP`, then orders fade-out, Map 57
tilesets, fade completion, VInt clear/wait, battle entity positioning, sprite initialization, map load,
the second VInt wait, entity sprites, battle VInts, `BattleTerrain01`, music, and fade-in. This is a
source order only, not a runtime execution claim. `BattleSpriteset01` at `0x1B3282` / 1782498 joins
the accepted spriteset owner through its address, 3/6/3/0 counts, nine-entry all-`STARTING` denominator,
and structural digest; it does not duplicate the nine entry identities, positions, or AI rows. The
complete region-cutscene table has no Battle 01 row.

The first-round source order is `ActivateEnemies`, `ExecuteBattleRegionCutscene`,
`PopulateTargetsListWithSpawningEnemies`, then `GenerateBattleTurnOrder`. Turn generation clears its
64 entries, admits only placed/living ally then enemy candidates, randomizes AGI before a descending
sort, and resets `CURRENT_BATTLE_TURN` to zero. The endpoint is immediately after generation and
before the first `BATTLE_TURN_ORDER` read or `ExecuteIndividualTurn`.

## Unknown Runtime Question Queue

- Natural R2a → R2b → R2c continuity, Map 21/40/57 admission, and caller order.
- A natural initialized Battle 01 snapshot, first actor, stable idle/player-ready state, and route menu
  relevance.
- Actual dialogue prose and chronology, complete reached 8C presentation, and any continuous natural
  route or battle-playthrough claim.

These are grouped R3-or-later questions. This H2 rail creates no H3 fixture, observer, emulator run,
or Phase 4 claim.
