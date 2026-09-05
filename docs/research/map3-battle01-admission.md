# Map 3 to Battle 01 Admission — Static and Bounded Player-Ready Contracts

- Status: **Confirmed** H2 static contract and explicit-bridge H3 player-ready observation; natural
  R2a → R2b continuity remains **Unknown**.
- Fixtures: `sf2-map3-battle01-admission-static-v1` and
  `sf2-map3-battle01-player-ready-runtime-v1`
- Reproduction: `uv run sf2 h2 map3-battle01-admission` and
  `uv run sf2 h3 map3-battle01-player-ready`
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Scope and Boundary

The H2 rail begins only at the accepted terminal projection of
[`map3-castle-battle-unlock`](./map3-castle-battle-unlock.md): Map 21 player `(5,15)`, facing
Down, after `cs_53EF4`'s source-defined F401/F256 semantics and with the retained R2b fixture and
route-graph digests checked. It derives a legal extension through Maps 21 and 40, then the static
Battle 01 admission and initialization spine.

The H3 rail begins at the accepted R2a follower-ready observation and uses one declared,
non-natural harness bridge to seed exactly the R2b Map 21 terminal. The bridge enters the original
`ProcessMapEvent`/warp handler before applying the retained terminal state. From that point, original
field control performs all 46 route inputs, both map warps, Battle 01 admission, cutscene and load
routing, turn generation, first-turn dispatch, and arrival at the player-input seam. This observation
does not promote the omitted R2a → R2b story segment to natural execution.

The public fixtures are recursively closed to licensing-safe identifiers, coordinates, flags,
addresses, state fields, structural command IDs/operands, hashes, topology, input chronology, and
restoration facts. They contain no source prose, raw ROM/H1/layout payload, asset, capture, movie,
save state, or private trace payload.

## Confirmed Static Findings

All Map 21 and Map 40 warp rows are parsed and source/H1/ROM checked. The selected links are Map 21
`(9,1)` → Map 40 `(4,30)`, facing Up, and Map 40 wildcard `y=12` → Map 57 `(8,18)`, facing Up.
Geometry, collision, event-type markers, occupancy, and area bounds derive the shortest selected
extension: 48 route nodes and 46 logical inputs, SHA-256
`68CBEBD2BF8A69054CCCEF7719BAFF5E1B8190B388E849BB091375DBA1D771AB`.
The wildcard row matches X independently, but the original controller calls `WarpIfSetAtPoint` only
for a destination map word whose event-type field is `$1000`. Map 40 therefore has two reachable
warp-event terminals, `(14,12):28` and `(15,12):29`; the selected endpoint is `(14,12)`. Ordinary
walkable cells on the same row are not warp terminals. Map 40's selected setup entity list is empty,
while the retained R2b post-program Map 21 entity occupancy remains `(6,16)`.

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
the second VInt wait, entity sprites, battle VInts, `BattleTerrain01`, music, and fade-in.
`BattleSpriteset01` at `0x1B3282` / 1782498 joins the accepted spriteset owner through its address,
3/6/3/0 counts, nine-entry all-`STARTING` denominator, and structural digest. The complete
region-cutscene table has no Battle 01 row.

The first-round source order is `ActivateEnemies`, `ExecuteBattleRegionCutscene`,
`PopulateTargetsListWithSpawningEnemies`, then `GenerateBattleTurnOrder`. Turn generation clears its
64 entries, admits only placed/living ally then enemy candidates, randomizes AGI before a descending
sort, and resets `CURRENT_BATTLE_TURN` to zero. The static endpoint is immediately after generation
and before the first `BATTLE_TURN_ORDER` read or `ExecuteIndividualTurn`.

## Confirmed Bounded Runtime Findings

The one-case H3 observation reaches Map 57, Battle 1, area `(0,0,16,20)` with F401 and F451 set,
F501 clear, and F90–F105 all clear. Active allies are `0,1,2`; the nine placed combatants are
`0,1,2,128,129,130,131,132,133`. From the declared `0x1234` bridge seed, generated turn order is
`1:6, 2:6, 128:5, 131:5, 133:5, 129:4, 130:4, 132:4, 0:3`. Actor 1 is dispatched at turn offset
zero and is the only actor executed before the stop.

The stop is `ControlBattleEntity` immediately after `WaitForVInt` and before the input read at
`0x22E70` / 142960. Current input, action, targeting, and map-event state are zero; moving and view
target entities are both actor 1; no transfer or cutscene/menu modal is pending; the semantic input
mode is battle-entity movement. The before-battle and battle-start programs have returned and turn
generation has returned. The ready deterministic state is RNG `2761495092`, retained RNG copy
`4660`, frame counter `236`, seconds `32`, and seconds-frame remainder `15`.

The observer's cutscene liveness callbacks only renew the bounded no-progress watchdog during the
original long scene; they are not presentation or timing evidence. Callback cleanup, every declared
state restoration domain, and deletion of the session ROM are required by the fixture.

## Unknown Runtime Question Queue

- Natural R2a → R2b continuity through the omitted castle story segment.
- A wholly natural route-carried Battle 01 snapshot and independently natural first-actor result.
- Inputs after the first stable seam, route-menu relevance, action/resolution, later rounds, and
  victory/return.
- Actual dialogue prose and chronology and complete reached 8C presentation.

The bounded H3 observation is not a full playthrough, Phase 4 claim, or natural-continuity claim.
