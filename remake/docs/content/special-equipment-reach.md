# Special equipment reach through first Pacalon

This Standard audit checks which of three [indexed items](campaign-items.md#special-equipment-and-attack-items)
can reach a player action before [first Pacalon](../application/pacalon-first-visit.md). Setup
selection, physical approach, inventory history and equipment admission each affect that
decision. The audit covers static evidence and the shortest legal starting points for later
observation. The [campaign item index](campaign-items.md) owns item lifecycles.

| Placement | Original route through first Pacalon | Accepted remake and ownership |
| --- | --- | --- |
| Evil Knuckles, Map5 F197 | **Confirmed** closed in early Yeel. The late piano opens the usable entrance to the chest area. | Late Grans return. A selected early chest row supplies a carry identity. The later equip caller depends on MMNK and cursed-weapon rules. |
| Evil Shot, Map16 F200 | **Confirmed** closed to ordinary walking from the audited early entrances. The later north gate or northern overworld approach reaches the search area. | Late Grans return. Rohde's current BRGN class would qualify once a legitimate copy reaches him. |
| Chirrup Sandals, Map17 F205 | **Inferred** ordinary pickup at first prison control. The initial room has a clear approach and the faced item marker. | Current-campaign Search candidate. Ordinary pickup and custody need a bounded witness. Ring Equip, footsteps and the Depot description each have an unbound consumer. |

## Source and layout

The baseline is the verified [USA retail ROM](../../../manifests/roms/sf2-us.json) and
ShiningForceCentral/SF2DISASM at `c834c652b6862bc5679fd7f69a38a7093206efc6`. The
[setup selector](../../../docs/research/world/map-data-inventory.md) uses the last matching flag.
Map headers supply layouts and item tables separately from setup populations and callbacks.
`CheckArea` reads the faced working-layout word masked with `$3C00`. Marker `$1800` calls
`OpenChest` and `$1C00` calls `CheckNonChestItem`. The matching map table and receipt flag
then determine the item. Field movement uses `esc02_controlCharacter`. Entity obstruction
precedes door and warp dispatch. The `$C000` destination solidity test follows dispatch
at `loc_52C0`. A matching stair slope can turn a horizontal input into a diagonal move.
[Field Search evidence](../../../docs/research/world/field-search-control.md) ·
[Pinned movement handler](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/scripting/entity/entityscriptengine_2.asm)

### Evil Knuckles

**Confirmed (static).** Map66 enters early Yeel at (21,42) or (7,1). Map5's main areas
are (0,0)–(31,43) and (0,44)–(31,63). `Map05s7_ChestItems` at `$99746` places F197
at (24,60), word `$D801`, in the lower area. The church stair row at (11,4) targets
(4,57), but the base word `$002C` has marker zero. The (22,11) → (4,64) row has an
unmarked origin (`$C078`) and a destination outside the layout. Default and F650 use a
plain piano description. F530 and higher-priority F28 share `Map5_212_DescFunc0`.
On Yes, `cs_60F64` copies `$1105` from (32,45) to (11,4) and sets F940. The copy's
`$1000` marker enables the stair. `Map05s3_FlagEvents` reapplies it on fresh F940 load.
F530 (Battle30 victory) and F28 (Lemon joined) both fall beyond the first-Pacalon history.
[Yeel entrance and area tables](../../../docs/research/chapters/moun-nazca-grans/late-yeel-chaz-and-lemon-recruitment.md#entrances-resources-and-six-exits) ·
[Piano callback](../../../docs/research/chapters/moun-nazca-grans/late-yeel-chaz-and-lemon-recruitment.md#piano-and-the-church-entrance-scene) ·
[Battle30 victory](../../../docs/research/chapters/northern-travel-mitula/battle30-outcomes-and-mitula-scene.md)

**Inferred (late pickup).** Start from a legal late Yeel visit with the piano callback
selected, F940/F197 clear and a free joined slot. Search from (22,12) facing Up,
accept the piano prompt, enter the church stair and approach (23,60) facing Right.
**Unknown:** actual pickup, holder and restored copy. An eligible equip witness can
use Sarah after PRST → MMNK. That route depends on Vigor Ball acquisition in Map23
and special promotion. Schedule the chest with the [late Grans return](../application/remaining-campaign-risk-baseline.md).
The two promotion dependencies can proceed against their own callers. Reuse the common
curse work when the late chapter has a legitimate copy.
[Map23 Search #1941](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1941) ·
[MMNK promotion #1957](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1957) ·
[Curse work #1926](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1926)

### Evil Shot

**Confirmed (static).** Map16 (43,2) has word `$DC1A` and F200 in
`Map16s8_OtherItems` at `$A167C`. Early default/F663 share the layout and early
init. F505 selects a different Map17 population after Battle05, and F530 is a later
setup on both maps. The lower castle's north gate has solid words `$C008` at
(45,7) and (46,7). Under F999, Map16's flag table at `$A157A` copies `$00A7`
from (27,9) to both cells on map load. Map59's post-shootdown initializer produces
F999. Map66's (32,5) → Map16 (45,1) entrance lies beyond the early Map66 walking
component. Its `$D06E` word carries a warp marker and can dispatch from the north.
The later Map77 scroll reaches that side of Map66.
Battle06 victory can instead return from the first party member's final
position plus (21,2). Its board overlaps the north pocket. The early ground
force cannot bridge the intervening Low Sky or obstructed battle cells. The
reproduction checks every ground-legal Battle02/03 return cell and the
Battle06 ground component from `BattleTerrain06` at `$1AD7F0`. This covers
variable victory positions as well as the fixed field entrances.
[Pinned Map16 flag copies](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map16/3-flag-events.asm) ·
[Nazca flag producer](../../../docs/research/chapters/moun-nazca-grans/nazca-ship-shootdown-and-crash-handoff.md) ·
[Battle06 victory return](../../../docs/research/chapters/grans-opening/battle06-to-battle07-content-route.md) ·
[Battle06 terrain and formation](../../../docs/research/chapters/grans-opening/battle06-opening-and-first-control.md) ·
[Pinned move costs](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/global/landeffectsettingsandmovecosts.asm) ·
[Map77 to Map66](../../../docs/research/chapters/moun-nazca-grans/granseal-overworld-return-and-battle38-admission.md)

**Confirmed (static tower boundary).** From the Battle05 return (16,18), a layout
path via (18,28) and (42,28) reaches the east tower door. The step copy at
(49,9) changes `$C4AF` to warp word `$1007` from (54,1). The warp goes to
Map17 (51,30). Right onto (52,30) `$80EA` and Right again follow the stair
slope to (53,29) `$907A`; that warp returns to Map16 (51,3). This upper pocket
ends at (48,4) `$2048` toward the search area. The geometric diagonal
(48,4) → (47,3) `$0030` has no matching stair slope, and its orthogonal
neighbors (47,4) and (48,3) are solid. The west pocket has an analogous
break at (43,4) → (44,3). The reach checks below allow both upper tower
arrivals even when a particular early script blocks access to one of them.
Neither pocket can reach the north gate exit or the F200 search approach
under the ordinary input rules.
[Map16 step and warp tables](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map16) ·
[Map17 warps](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map17/6-warp-events.asm) ·
[Battle05 return and exits](../application/pre-voyage-campaign.md#galam-escape-through-battle06-first-control)

**Confirmed (ordinary input).** `VInt` calls `ApplyZ80BusUpdates` before
its contextual functions. `@ProcessInputs` polls the pad with `UpdatePlayerInputs`
then filters `PLAYER_1_INPUT` before copying it to `CURRENT_PLAYER_INPUT`.
For an ordinary Up+Left or equivalent two-axis press, removing a held primary
direction leaves the other axis. If both are new, the vertical mask leaves only
vertical input. A nonzero `CONTROLLING_UNIT_CURSOR` bypasses this filter.
Its writers in `ControlCursorEntity` set it for battle cursor input and clear
it on exit. At ordinary exploration `WaitForEvent` installs
`eas_ControlledCharacter` on main entity 0. Its `ac_controlCharacter` reaches
`esc02_controlCharacter` through `VInt_UpdateEntities`; the slot reads
`PLAYER_1_INPUT` at `loc_502C`, after the filter has already modified that byte.
The handler's ability to add two supplied offsets therefore establishes a
controlled geometry case. Ordinary field input reaches one axis or a
slope-driven diagonal. The remake's cardinal `Move` and stair resolver agree
at this boundary. Classify F200 under the [late Grans return](../application/remaining-campaign-risk-baseline.md).
[Pinned interrupt order](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/tech/interrupts/vint.asm) ·
[Pinned input filter](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/tech/interrupts/applyfadingeffectandz80busupdate.asm) ·
[Pinned battle cursor](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/battlefunctions_0.asm) ·
[Pinned exploration caller](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/exploration/explorationfunctions_2.asm) ·
[Pinned controlled actscript](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/scripting/entity/eas_main.asm) ·
[Remake field input](../../game/src/Exploration/ExplorationSessionView.cs) ·
[Remake traversal](../../src/Sf2.Remake.Domain/Maps/OriginalMapTraversal.cs)

**Inferred (late pickup and equip).** Take an ordinary post-Nazca Galam revisit
with F999 held, F200 clear and a free joined slot. Cross the opened gate or
approach from Map66 (32,4), attempt the south warp to Map16 (45,1), and Search
from (44,2) facing Left. **Unknown:** ordinary item-specific pickup, holder,
restored copy and cursed equip outcome. For the later player binding, keep the
actual bare word and Give it to Rohde after the North Cliff join. He starts
BRGN. His observed Great Shot attack from the English Pacalon party establishes
a legal weapon caller at that class. That earlier party supplies no legitimate
F200 copy. Elric and Janet start ACHR. Evil Shot accepts SNIP, BRGN and BWNT
and needs the source 2–3 range, ATT/DEF effects, shared curse transactions and
Shot projectile family. Reuse the curse rules from #1926 when available. All
three audited items have `useSpell NOTHING`, outside #1952's Use, damage and
Repair loop.
[North Cliff join](../application/north-cliff-rohde.md) ·
[Pacalon BRGN attack](../application/pacalon-first-visit.md#town) ·
[Weapon admission](../../src/Sf2.Remake.Content/Scenarios/PrivateFieldPreparation.cs) ·
[Pinned item rules](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/items/itemdefs.asm) ·
[Pinned projectile selector](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/getspellanimation.asm) ·
[Black Ring #1926](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1926) ·
[Ring Use/Repair #1952](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1952)

### Chirrup Sandals

**Confirmed (static).** After Battle04 capture, default `ms_map17_InitFunction` runs
`cs_5249E` and sets F660. Bowie settles at (59,5) in the prison area (38,0)–(62,12).
Before F662, the path Up to (59,4), then Left twice to (57,4), faces the item at (56,4).
Its word `$DC57` has the `$1C00` marker and Map17's other-item table at `$A2F2E` gives
F205. This path avoids the (57,5) shortcut zone. The later `cs_528D4` changes (57,4),
and F505 places a resident at the alternate approach (56,5).
[Prison capture and scripts](../../../docs/research/chapters/grans-opening/battle04-to-battle05-content-route.md) ·
[Pinned F205 table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map17/8-other-items.asm) ·
[Pinned F505 population](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map17/mapsetups/s1_entities_505.asm)

**Inferred (ordinary Search in the original and selected remake).** Start from the
Battle04 capture or a legitimate Battle04 Movement slot followed by victory or
special defeat. Finish wake-up with a free joined slot, Search at the approach, and
check the actual word, holder, F205 and empty repeat. The documented Map57 Battle01
Continue is a fallback start if there is no later legal slot.
Map16/17 field saves were outside the accepted Galam slice. Carry the bare word to a
supported downstream Movement save or the observed settled Battle07 Map58 return and
cold Continue. A first-Pacalon slot whose history skipped the pickup cannot establish
that lineage. **Unknown:** actual normal pickup, recipient and item-specific persistence.
[Observed Galam route](../application/pre-voyage-campaign.md#galam-escape-through-battle06-first-control) ·
[Battle save and Continue](../application/battle-save-and-continue.md#implemented-ordinary-host-boundary-c) ·
[Battle07 return](../application/pre-voyage-campaign.md#battle06-result-through-usable-tower-exploration)

**Confirmed (static consumers).** Item 126 is an all-class `RING|RARE`. `SetMoveSfx`
at `$25790` checks the equipped ring and selects `SFX_BLOAB` (99). For field
movement, `ExplorationLoop` calls it for Bowie at `loc_2586A` before the next wait.
Battle movement uses the acting combatant. The Caravan appraisal picks a stored item
then checks `DisplaySpecialCaravanDescription` before the ordinary type description.
Its table at `$228A2` gives item 126 a two-message entry. After legitimate pickup, use
Bowie for a field Equip and step check. A separate legal carry after Taros can exercise
Deposit → Look → retrieval as soon as appraisal supports the item, independently of
Equip. **Unknown:** audible original behavior along these routes and remake item-specific
Equip, sound and appraisal behavior. #1946 supplies the ring slot and currently covers
four stat rings. The Sandals effects need explicit follow-up ownership.
[Original movement sound contract](../../../docs/design/contracts/battle/battle-functions-control-flow.md) ·
[Pinned special appraisal table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/items/specialcaravandescriptions.asm) ·
[Battle movement audio](../godot/battle-scenes.md) ·
[Remake Depot boundary](../application/caravan-services-design.md#current-depot-implementation) ·
[Ring slot #1946](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1946)

## Reproduce the static boundary

Load the [registered inputs](../../../docs/operations/local-private-inputs.md) in the
same PowerShell process and run `uv run sf2 rom verify`. Read the linked assembly at
the stated pin for the exact selectors, callbacks and copy rules. The command below
uses `pt_MapData` from the tracked [map-content fixture](../../../tests/fixtures/h2/map-content-static-v1.json)
and the terrain table from the [battle-terrain fixture](../../../tests/fixtures/h2/battle-terrain-decode-v1.json).
It reads the verified 2 MiB ROM into memory and decodes four 64×64 layouts and
three 48×48 battle arrays from less than 64 KiB of compressed slices. Each search visits
at most 4096 cells. It bounds closed routes generously by ignoring residents and
allowing passage across event markers. The Map66 exclusion also allows free diagonals.
The positive Map16 two-axis control demonstrates geometry with an input the ordinary
field filter suppresses. These checks write only a terminal verdict.

```powershell
. ./local/private-inputs.ps1
@'
import json
import os
from collections import deque
from pathlib import Path
from struct import unpack_from

from sf2tool.compression import decode_stack_compressed
from sf2tool.h2.map_layouts import decode_map_blocks, decode_map_layout

rom = Path(os.environ["SF2_SHARED_INPUT_ROOT"], "roms", "sf2-us.bin").read_bytes()
fixtures = Path("tests/fixtures/h2")

def read_table(name, field):
    return json.loads((fixtures / name).read_text(encoding="utf-8"))["table"][field]

table = read_table("map-content-static-v1.json", "pt_MapData")
terrain_table = read_table("battle-terrain-decode-v1.json", "terrainTableAddress")

def decode(map_id):
    entry, = unpack_from(">I", rom, table + 4 * map_id)
    blocks, layout = unpack_from(">2I", rom, entry + 6)
    count = len(decode_map_blocks(rom[blocks:layout])[0]) // 9
    words = decode_map_layout(rom[layout:layout + 8192], count)[0]
    return {(x, y): words[y * 64 + x] for y in range(64) for x in range(64)}

def battle_ground(battle, ox, oy, width, height):
    address, = unpack_from(">I", rom, terrain_table + 4 * battle)
    grid = decode_stack_compressed(rom[address:address + 2048], expected_output_bytes=48 * 48).output
    return {(x + ox, y + oy): 0 if 1 <= grid[y * 48 + x] <= 6 else 0xC000
            for y in range(height) for x in range(width)}

def reach(words, bounds, starts, diagonals=False):
    left, top, right, bottom = bounds
    pending = deque(starts)
    reached = set(starts)
    moves = [(0, -1), (0, 1), (-1, 0), (1, 0)]
    if diagonals:
        moves += [(dx, dy) for dx in (-1, 1) for dy in (-1, 1)]
    while pending:
        x, y = pending.popleft()
        collision = words[x, y] & 0xC000
        for dx, dy in moves:
            if dx and not dy and collision in (0x4000, 0x8000):
                stair_y = y + (dx if collision == 0x4000 else -dx)
                if words.get((x + dx, stair_y), 0) & 0xC000 == collision:
                    dy = stair_y - y
            target = x + dx, y + dy
            if (left <= target[0] <= right and top <= target[1] <= bottom
                    and words[target] < 0xC000 and target not in reached):
                reached.add(target)
                pending.append(target)
    return reached

yeel, castle, prison, field = (decode(map_id) for map_id in (5, 16, 17, 66))
assert [yeel[p] for p in ((11, 4), (32, 45), (22, 11), (24, 60))] == [
    0x002C, 0x1105, 0xC078, 0xD801]
assert (23, 60) in reach(yeel, (0, 44, 31, 63), [(4, 57)])
assert [prison[p] for p in ((59, 5), (59, 4), (58, 4), (57, 4), (56, 4))] == [
    0x0050, 0x005C, 0x000F, 0x005D, 0xDC57]
assert [prison[p] for p in ((52, 30), (53, 29))] == [0x80EA, 0x907A]
assert [castle[p] for p in ((49, 9), (54, 1), (51, 3), (48, 4), (47, 3),
                            (47, 4), (48, 3), (44, 2), (43, 2))] == [
    0xC4AF, 0x1007, 0x1033, 0x2048, 0x0030, 0xC046, 0xC032, 0x0005, 0xDC1A]
assert castle[45, 7] == castle[46, 7] == 0xC008
for door in ((27, 22), (35, 14), (38, 14), (37, 25), (44, 19), (45, 28)):
    castle[door] = castle[54, 0]
castle[49, 9] = castle[54, 1]
area = (0, 0, 53, 31)
early = [(16, 18), (23, 5), (27, 7), (26, 30), (17, 15), (10, 16), (8, 5),
         (21, 3), (49, 10), (39, 3), (51, 3)]
early_tiles = reach(castle, area, early)
assert not early_tiles.intersection({(44, 2), (45, 1)})
assert all(y != 0 for _, y in early_tiles)
assert (49, 9) in reach(castle, area, [(16, 18)])
assert (48, 4) in reach(castle, area, [(51, 3)])
assert (44, 2) in reach(castle, area, [(51, 3)], diagonals=True)
for gate in ((45, 7), (46, 7)):
    castle[gate] = castle[27, 9]
assert (44, 2) in reach(castle, area, [(16, 18)])
assert field[32, 5] == 0xD06E
field[31, 7] = field[59, 4]  # F611, opens an early cell
field_starts = [(1, 27), (19, 24), (18, 21), (31, 9), (6, 6)]
for battle, bounds in ((2, (16, 19, 22, 18)), (3, (0, 0, 22, 30))):
    field_starts.extend(p for p, word in battle_ground(battle, *bounds).items() if word == 0)
battle06 = battle_ground(6, 21, 2, 20, 32)
formation = [(x, y) for x in (30, 31, 32) for y in (8, 9)]
field_starts.extend(reach(battle06, (21, 2, 40, 33), formation))
north_approach = {(x, y) for x in range(31, 34) for y in range(4, 7)}
north_approach.update((x, 1) for x in range(64))  # Neighbors of the north scroll row
assert not north_approach & reach(field, (0, 0, 63, 63), field_starts, diagonals=True)
for x in range(10):  # F508 comparison after first voyage
    for y in range(15):
        field[x + 28, y + 23] = field[x, y + 49]
assert not north_approach & reach(field, (0, 0, 63, 63), field_starts, diagonals=True)
print("PASS: Yeel stair and chest, Galam gate and tower, prison Search approach")
'@ | uv run --locked python -X utf8 -
```
