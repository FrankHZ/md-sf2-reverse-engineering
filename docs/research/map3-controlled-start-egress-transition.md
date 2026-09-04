# Map 3 Controlled-Start Egress Transition

## Scope and result

This owner closes the transition gap between the controlled Map 3 start at
`(56,3)` and the already accepted natural-route interaction with entity 142.
It joins existing source/H1/ROM and bounded H3 evidence; it does not add a new
runtime observation.

- Canonical private input: US ROM SHA-256
  `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Source baseline: `SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`.
- Retained runtime owners:
  [`map3-admitted-start-v1.json`](../../tests/fixtures/h3/map3-admitted-start-v1.json)
  and
  [`map3-battle01-natural-route-v1.json`](../../tests/fixtures/h3/map3-battle01-natural-route-v1.json).
- Retained static owners:
  [`map-content-static-v1.json`](../../tests/fixtures/h2/map-content-static-v1.json)
  and
  [`map3-entity142-interactable-reference-static-v1.json`](../../tests/fixtures/h2/map3-entity142-interactable-reference-static-v1.json).

**Confirmed:** the required boundary is a two-warp, same-map chain. The first
warp moves the player from Map 3 area ordinal 2 into area ordinal 1; after the
accepted route through the house and school, the second moves the player from
area ordinal 1 into area ordinal 3, which contains the entity 142 interaction
position. Neither warp selects a different Map 3 setup variant.

## Stable records and area relation

Map 3's warp table is `data/maps/entries/map03/6-warp-events.asm`, ROM
`0x978F0..0x9793A`. Each row is eight bytes in the macro-defined order
`trigger X, trigger Y, type, target map, target X, target Y, facing, padding`.
The two forward records are:

| Role | Stable row identity | ROM address and bytes | Trigger target | Destination |
| --- | --- | --- | --- | --- |
| Bowie house stairs down | zero-based 8 / one-based 9 | `0x97930`, `36 03 00 FF 03 03 00 00` | `(54,3)` | current Map 3 `(3,3)`, `RIGHT` (`0`) |
| School stairs down | zero-based 5 / one-based 6 | `0x97918`, `2E 07 00 FF 3B 0C 02 00` | `(46,7)` | current Map 3 `(59,12)`, `LEFT` (`2`) |

The `type` byte is `0` (`warpNoScroll`) and the raw destination-map byte is
`0xFF` (`MAP_CURRENT`) in both records. The table is first-coordinate-match;
the trigger coordinate is the controlled entity's candidate target tile, not
a requirement that the entity first settle on that tile.

The stable consumer chain is `WarpIfSetAtPoint` (`0x42DC`) →
`WaitForEvent` (`0x2591C`) → `ProcessMapEvent` (`0x2594A`) →
`ProcessMapEventType1_Warp` (`0x25978`) →
`UpdatePlayerPosFromMapEvent` (`0x25A2A`) → `MainLoop` (`0x75C4`) →
`ExplorationLoop` (`0x257C0`) → `LoadMap` (`0x2A8C`) / `LoadMapArea`
(`0x2DEC`). The first landing additionally reaches `ToggleRoofOnMapLoad`
(`0x3F2C`). These are H1-bound program identities; the table row, rather than
an inferred scene name, supplies each transition's operands.

Map 3's three 30-byte area rows are
`data/maps/entries/map03/2-areas.asm`, ROM `0x977FE..0x9785A`:

| One-based ordinal | ROM address | Inclusive main-layer bounds | Second foreground/background start | Relevant point |
| --- | --- | --- | --- | --- |
| 1 | `0x977FE` | `(0,0)..(50,31)` | `(0,32)` / `(0,0)` | house landing `(3,3)` and school stair `(46,7)` |
| 2 | `0x9781C` | `(51,0)..(61,9)` | `(0,0)` / `(0,0)` | controlled start `(56,3)` and pre-warp `(55,3)` |
| 3 | `0x9783A` | `(51,10)..(61,19)` | `(0,0)` / `(0,0)` | stair landing `(59,12)`, stand `(55,17)`, entity 142 `(54,17)` |

`LoadMap` scans these rows in source order and selects the first inclusive
bounds containing the player/load coordinate. Thus the coordinate changes,
not a flag-switched layout definition, account for the observed area changes.
Area ordinal 1 is the sole row here with a nonzero second-foreground start.

## First warp: controlled pocket to area 1

The accepted start reaches the first `WaitForEvent` at Map 3 `(56,3)`, facing
`DOWN` (`3`), with the default setup `ms_map3` (`0x50AE8`) and
`ms_map3_InitFunction` (`0x51382`). Selector flags 609, 506, and 543 are clear.

The observed and source-joined chronology is:

1. `Left` moves the player to `(55,3)` and leaves facing `LEFT`.
2. A second `Left` targets `(54,3)`. The target block's warp marker is tested
   before passability; `WarpIfSetAtPoint` chooses warp row 8 and writes map
   event type 1 with parameters `0, 0xFF, 3, 3, RIGHT`.
3. `WaitForEvent` returns the request. `ProcessMapEvent` clears the request and
   dispatches `ProcessMapEventType1_Warp`.
4. Because parameter 1 is zero, the handler idles the controlled entity,
   unwinds the exploration return, and returns the raw current-map sentinel,
   destination `(3,3)`, facing `RIGHT`, and `D4=0` to `MainLoop`.
5. `ExplorationLoop` interprets the low `0xFF` map byte as the current-map
   reload path, updates the player, and calls `LoadMap`. That path preserves
   the already decoded working blocks/layout, selects area ordinal 1 from the
   destination coordinate, runs `ToggleRoofOnMapLoad`, reruns the selected
   Map 3 init, and reaches the next `WaitForEvent`.

**Confirmed (bounded H3):** the natural-route trace records
`map-event:warp:map3-bowie-house-exit`, a new `exploration:3`,
`map-init:ms_map3_InitFunction`, and `route:post-warp-wait-for-event`, in that
order. It records the landing `(3,3)`, working-layout byte offset `390`, and
layout word `20533`; the next `Right` follows the slope to `(4,4)`.

This transition is primarily a relocation plus area reselection. It is not a
setup-variant selection and it does not rebuild the base layout. There is one
separate, deterministic post-reload layout effect: at `(3,3)`, area ordinal
1's `(0,32)` second-layer origin makes the first containing roof record the
Bowie-house `slbc 4,8` record. Its source `(255,255)`, size `7x8`, and
destination `(2,32)` make `PerformMapBlockCopyScript` save and clear that
second-layer rectangle. This mutates the preserved working layout as
roof/presentation state; it is not the mechanism that changes the player's
coordinate or area.

## Required bridge through area 1

The accepted natural route supplies the bridge between the two warps:

1. The forced slope step `(3,3) -> (4,4)` admits `Map3_ZoneEvent6`. With flag
   601 clear it runs `cs_5145C`, moves entity 128, and sets flag 601. It does
   not relocate the player or select a setup/layout variant.
2. The Bowie-house door at `(4,8)` and school door at `(41,13)` are step-event
   block copies. They mutate their door cells; neither is a warp.
3. Entity Sarah initially blocks the school route at `(42,8)`. Interaction
   from `(42,9)`, facing `UP`, reaches `Map3_EntityEvent0`; with flags 603,
   602, and temporary 256 clear, it runs `cs_513D6`, moves Sarah to `(41,7)`,
   and sets temporary flag 256.
4. Movement can then reach `(45,7)` and target the school warp at `(46,7)`.

Flags 601 and 256 describe mandatory events on this accepted route, but they
do not guard either warp record. The only setup-selection predicates remain
flags 609, 506, and 543; none is set by this bridge.

## Second warp: area 1 to the entity 142 region

Targeting `(46,7)` follows the same request and no-scroll handler chronology,
using warp row 5. The destination is current Map 3 `(59,12)`, facing `LEFT`.
The current-map reload again preserves the working base layout, selects area
ordinal 3, retains temporary flag 256 because this path skips the new-map
entity/temp-flag initialization block, and reruns the same default
`ms_map3_InitFunction`. At this point flags 1, 602, and 603 are clear, so that
init requests no script; no Map 3 roof row contains `(59,12)`, so this landing
does not add a roof block-copy effect.

**Confirmed (bounded H3):** the retained trace records
`map-event:warp:map3-school-stairs-down`, `exploration:3`, the same Map 3 init,
and the next exploration wait. From `(59,12)`, `Left` follows the slope to the
area-3 zone at `(58,13)`, after which accepted controller input reaches
`(55,17)`, faces `LEFT`, and dispatches logical entity 142 at `(54,17)` to
`Map3_EntityEvent15`. The independent entity-142 H2 owner binds that logical
entity to source record 17 / physical slot 17 under this accepted route.

The first warp is therefore sufficient to escape the controlled pocket and
activate area ordinal 1. It is not sufficient to reach entity 142's disjoint
area ordinal 3; the school warp is the second required transition in the
accepted chain.

## Evidence labels and retained Unknowns

| Claim | Classification | Owner |
| --- | --- | --- |
| record widths/order, first-match scan, target-marker-before-passability order, current-map reload policy, area scan, and roof-on-load scan | **Confirmed static source/H1/ROM** | `sf2-map-content-static-v1` |
| `(56,3)`, facing 3, Map 3/default setup/init, clear selector flags, first wait | **Confirmed bounded H3** | `sf2-map3-admitted-start-runtime-v1` |
| both warp requests, reload/init/wait chronology, mandatory bridge, area-3 route, and entity 142 dispatch | **Confirmed bounded H3** | `sf2-map3-battle01-natural-route-runtime-v1` |
| entity 142 source/event identity and physical-slot relation | **Confirmed static under the accepted route state** | `sf2-map3-entity142-interactable-reference-static-v1` |
| globally shortest input route or uniqueness under every possible entity/flag state | **Unknown** | not required by this bounded accepted chain |
| exact fade frames, roof appearance, camera transition frames, audio timing, and other rendered presentation | **Unknown** | future admitted presentation evidence |
| behavior when selector flags 609, 506, or 543 choose another Map 3 setup | **Unknown for this route** | outside the controlled default-setup boundary |

Reproduce the static owners with `uv run sf2 h2 map-content` and
`uv run sf2 h2 map3-entity142-interactable-reference`. The retained runtime
owners are reproduced by `uv run sf2 h3 map3-admitted-start --timeout-seconds
180` and `uv run sf2 h3 map3-battle01-natural-route --timeout-seconds 180`.
This slice reuses their accepted observations and does not claim a new H3 run.
