# Pre-voyage location coverage

[Issue #2020](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2020) expands the
[pre-voyage campaign agreement](pre-voyage-campaign.md#pre-voyage-location-agreement) to ordinary
optional exploration in Grans. The [location index](../content/campaign-locations.md#grans-and-the-voyage)
owns campaign navigation. This review records the early-phase closure of the map connections and
the work to make their accessible content playable. For item and flag lifetimes, use the
[campaign item index](../content/campaign-items.md) and [Grans flags](../content/campaign-flags-grans.md).

## Source and method

The baseline is the [USA retail manifest](../../../manifests/roms/sf2-us.json) and
[SF2DISASM c834c652](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
For each ordinary entry or battle/scene return, follow the area bounds, warp rows, active setup,
flag copies, step copies and marker/collision words. Setup dispatch retains the last matching
source selector. Read the entity, zone, description, item and init callers when they change the
entry, offer an interaction, displace the player or create a later gate. Check the pin's
[global setup table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/mapsetups.asm)
and [map substitution table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/global/flagswitchedmaps.asm)
before treating an entry coordinate as controlled field state.

The static layout check uses `MapNN` in the pinned H1 listing. Read the two big-endian ROM
pointers at that entry +6 and +10 and check them against `MapNNs0_Blocks` and `MapNNs1_Layout`.
Use `decode_map_blocks` and `decode_map_layout` from [the existing decoder](../../../src/sf2tool/h2/map_layouts.py).
For a map arrival, use the four directional candidates and stair deltas in `_move` from
[the castle route check](../../../src/sf2tool/h2/map3_castle_battle_unlock.py). Apply live flag
copies. At a door candidate with marker `$0400`, apply its matching step copy before re-reading
the marker. A candidate with marker `$1000` and a matching warp row terminates the local walk and
seeds the next arrival. The [field controller](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/scripting/entity/entityscriptengine_2.asm)
queues the warp **before** its `$C000` movement rejection. For this traversal check only, let
such an attempted step emit the warp even if the player cannot occupy the trigger. In particular,
rejecting all `$Dxxx` candidates first hides legitimate cave and wild exits. Use the faced cell's
marker for Search. Generic Search fallback and a declared item table row do not confer a reward.
The [Search controller](../../../docs/research/world/field-search-control.md) and
[map exploration contract](../../../docs/design/contracts/world/map-exploration.md) own the rules.

**Confirmed** static reach here means that the given settled arrival, stated copies and named
source gates admit the connection on the decoded layout. A collision path without population
constraints is an upper bound. Fixed actors and script displacement are included where they
change the conclusion. Ambient movements, ordinary dialog input, and joint routes across multiple
encounters still require their own observations. **Inferred** and **Unknown** below name the
unsettled conditions. No emulator or remake run is part of this design review.

For the remake, compare the [tracked selection](../../content/private/pre-voyage-opening.json),
the existing private Phoenix-wave publication (baseline `33673735` in
[the player report](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456#issuecomment-7069929011)),
and the consumers at the accepted entry baseline. Inspect map/variant IDs, event kinds, Search
receipts and reachable program references in `world.maps` and `world.programs`. Read program
branches and marker sites before classifying a retained `native-call` or missing variant.
The package provides static admission evidence. The linked campaign observations give the
narrower played boundaries.

The work is bounded by about 25 map layouts including rejected neighbors, each 64 × 64. Treat
each distinct story layout as one local walk from its admitted arrivals. This is linear in the
visited cells and transitions per state, roughly 100,000 cells for one pass over those layouts.
One checked retail image is 2 MiB. The existing world is about 102 MB on disk and was inspected
read-only in memory. Evidence here retains route and gate facts only. No new scan/export pipeline,
runtime capture, republished world, or asset build is required. Expand beyond a named neighboring
map only when a pre-voyage arrival reaches it.

Use the [existing Grans route reproduction](../content/special-equipment-reach.md#reproduce-the-static-boundary)
for the Yeel cellar, Galam north gate and Battle02/03/06 variable returns. It checks return areas
from ground-legal battle positions as well as the fixed landings. The direct shrine marker check
below uses the same fixture and decoder. Load the [registered inputs](../../../docs/operations/local-private-inputs.md)
and verify the ROM identity in the same shell before the read.

```powershell
. ./local/private-inputs.ps1
uv run sf2 rom verify
@'
import json
import os
from pathlib import Path
from struct import unpack_from

from sf2tool.h2.map_layouts import decode_map_blocks, decode_map_layout

rom = Path(os.environ["SF2_SHARED_INPUT_ROOT"], "roms", "sf2-us.bin").read_bytes()
table = json.loads(Path("tests/fixtures/h2/map-content-static-v1.json").read_text())["table"]["pt_MapData"]
for map_id, cells in {
    66: {(33, 43): 0xD19F, (35, 28): 0x0141, (7, 54): 0x11C6},
    41: {(23, 4): 0xC43D, (30, 0): 0x100D, (6, 7): 0xC057},
    42: {(19, 24): 0xD801, (26, 18): 0xC058, (4, 3): 0xC001},
    60: {(3, 53): 0x0129},
}.items():
    entry, = unpack_from(">I", rom, table + 4 * map_id)
    blocks, layout = unpack_from(">2I", rom, entry + 6)
    count = len(decode_map_blocks(rom[blocks:layout])[0]) // 9
    words = decode_map_layout(rom[layout:layout + 8192], count)[0]
    for (x, y), expected in cells.items():
        assert words[y * 64 + x] == expected, (map_id, x, y)
print("PASS: early shrine and ascent marker controls")
'@ | uv run --locked python -X utf8 -
```

## Southern shrine and excluded connections

| Source connection | Earliest pre-voyage assessment and remake disposition |
| --- | --- |
| **Southern Devil's Head entrance** · Map66 (33,43), row 8 → Map41 (23,14) Up | **Confirmed** static from the southern Grans component, including from Granseal's west-return seed (29,32) and the drawbridge return with F611. A walk can approach from (33,44). The trigger is `$D19F`. The southern entrance is playable in the [shrine publication](#play-the-southern-shrine). **Inferred** match for the user's south-cave report. The player's exact tile is **Unknown**. |
| **Map41 shrine exterior** · eastern area X17–29/Y0–15 | **Confirmed** static. Both south exits (23,15)/(24,15) return to Map66 (33,44) Down. Approach (23,4) from (23,5). The closed-looking `$C43D` door invokes `OpenDoor`, whose one-cell copy from (30,0) installs `$100D`. The same attempted step queues Map42 (7,14) Up. No story flag or key gates this door. The setup has no NPC or chest. There is no early special description at this door. The Map41 head description and ground weapon are in the separate western area. |
| **Map42 Jewel Cave / Force Sword Shrine** · entry hall and lower chamber | **Confirmed** static. The entrance hall's (7,15) returns to Map41 (23,5) Down. Its (2,9) CURRENT stair reaches (19,17). The lower chamber's (19,16) returns to (2,8). Search the chest at (19,24) from (19,23) Down or (20,24) Left. The `$D801` chest grants Power Water, F133. Map42 has no ordinary NPC. F999 clear makes the (19,21) zone return without attraction, so its F901 copy is absent. At (26,18) the base `$C058` cell has no warp marker. It blocks the sword room and its extraction Search. The Evil Axe row F198 likewise lacks its chest marker at (4,3) until the F901 copy. The unused X15–32/Y0–12 area has no entry in this route. Both early areas, the stairs and F133 are playable in the shrine publication. |
| **Central Devil's Head entrance** · Map66 (35,28), row 7 → Map41 (6,14) Up | **Confirmed** static exclusion. F508 clear gives `$0141` and no warp. The Map66 F508 copy adds the `$11C6` marker after Battle08, even though the player can walk near the tile earlier. Do not enable the western area in the demo. Its (6,15) returns to Map66 (36,28). When that area is later reachable, the head at (6,7) offers Search from (6,8) and the `$DC45` Evil Lance placement (6,5) can be faced. F930 controls the head replacement and access to Map28, with the Force Sword Use caller owning the opening. Neither the lance nor the statue interaction belongs to the early shrine exterior. |
| **North Grans and Magic Tunnel Hub** · Map66 (40,7), row 0 and Y0 scroll row 9 | **Confirmed** static exclusion. The southern field seeded by Granseal/drawbridge and the north-Yeel/Hawel component reach neither row. F611 changes the drawbridge approach but does not join these components to the northern ones. Galam's north return (32,4) on Map66 would reach Y0 at X27–37, but Map16's (45,7)/(46,7) gate cells block that return until F999. Map66 (40,8) is a different component with the Map63 portal and a Y0 marker at X57. The ground-legal Battle02/03/06 return controls also exclude the north row and its connected landings. Leave Map77's Dwarven Village/Prism/Roft routes and Map63's paired portals/Claude/Map50 branch outside the opening. The Map63 entrance area would return at (37,58) → Map66 (40,8) and reach the pair (37,40) ↔ (17,2) if supplied a later arrival. The Claude/Map50 part is separate. |
| **Ancient Tower Ascent** · Map60 Y26 row 22 → Map56 | **Confirmed** static exclusion. The F506-clear Map57 stair row has no active marker. With F506 set, the (7,4) marker reaches Map60 (53,1). Portal pairs at the 61/62,10 → 13/14,2, 1/2,7 → 1/2,29, 13/14,30 → 38,3 and 45/46,8 → 21/22,35 seams lead to Map58 at Y51. The (3,53) row toward the other Map60 component reads `$0129` with F999 clear. F999 adds that portal, changes the lower layout and substitutes Map59 for Map58. Do not join the later (39,26)/(47,19)/(53,17) chain, Map56's priest/upper ascent or Map59 to the pre-voyage return. |
| **Other table-only or late areas** | **Confirmed** static. Yeel's church stair at (11,4) is `$002C` until F940 from the later piano sequence. Its underground F196 Quick Chicken and F197 Evil Knuckles are excluded. The (22,11) row lacks a warp marker and its paired Y64 is outside the 64 × 64 map. The early remade unavailable rows need no destination implementation. On Map14 the two extra layout areas and their dormant rows at Y32/Y36 are not connected to the first controllable deck/stairs. Map73's first-arrival init owns the sea passage to Map14, and its field rows to New Granseal/Parmecia are later movement. |

Check the shrine against the [pinned Map66 warp table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map66/6-warp-events.asm),
[Map66 flag copies](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map66/3-flag-events.asm),
and the [Map41/42 research owner](../../../docs/research/chapters/final-confrontations-ending/battle39-outcomes-and-force-sword-route.md)
for the source records and callback gates. Its late narrative order does not determine when
the lower chamber can be entered or the Power Water collected.
For the separate branches see the [Map16 F999 copies](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map16/3-flag-events.asm),
[Map63 warps](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map63/6-warp-events.asm),
[Map60 layout/warp source](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map60),
and [Yeel equipment reach](../content/special-equipment-reach.md#evil-knuckles).

## Selected locations and interactions

These are the existing map and interaction owners to carry into the expanded acceptance. A
receipt tagged as selected still needs a faced marker, approach and admitted capacity. The
[Search inventory](campaign-field-search.md#map-and-setup-inventory) records the compiled
placements and descriptions. Report unsupported actions at their actual caller or service.

| Phase and place | Accessible branches and setup changes | Remake coverage and remaining work |
| --- | --- | --- |
| **New Game to commission**, Map3 Granseal | Bowie's home and CURRENT stairs, town houses, school and CURRENT classroom stairs, talk to the residents, messenger Yes/No/retry, Church, F600 castle admission and F603 west-exit guard. School herb F128. Shop empty chest F220 has a marker but its valid faced tile is occupied in the default setup. Default fixed entity 137 at (45,22) separates the town from the otherwise marked dock exits. F609 changes that approach and F506 later opens the cell and changes population for Kiwi. | Opening dialogue, Church and cold Continue, school Search, and the F506 Kiwi refusal/revisit/join have play evidence. F609/F506 Church and both supply/weapon Shop callers are selected. Existing [opening branch inventory](../content/opening-branch-inventory.md) and [Kiwi visit](pre-voyage-campaign.md#battle06-result-through-usable-tower-exploration) own the intent. Ask each shop only on an eligible setup. F220 is no additional reward. |
| **Opening, first return, and post-Galam castle**, Maps19/20/21 | Map19 approach, courtyard, storeroom F129–131 chests, west and east lower rooms; Map20 west stair room, king's bedroom, upper royal room and both east tower levels through the Map19 east landing. Map21 guard and exit to tower exterior. Keep separate the F501, F609, F506 and applicable F507 setup changes, bedroom invite/refusal then departure/exorcism, and castle quake warnings. | The rooms and stairs are connected in the selected layout, including the diagonal CURRENT routes. Talk/Search tables and three chest rows are present. Retain the accepted bedroom-cast rule. F507 Map19 is selected and Map21 has no active source-zone marker. **Unknown** ordinary first/revisit control of all optional room approaches with the legal cast, especially transitions around the shared east landing and before/after the exorcism. Use the [castle and stair observations](pre-voyage-campaign.md#same-map-walking-party-arrival) and [opening Search owner](../content/opening-branch-inventory.md#search-surfaces). |
| **Tower exterior, return, second descent**, Maps40/57/60/58 | The exterior links the castle and antechamber and permits F132 Mithril. Map40 F506 Lemon approach, F507 default restoration, Map57 first battle/return and F506 stair opening, Map60 connected early chain and Map58 return. Map60/58 have no interaction cast or placements to add; preserve party and exit through the actual connected stairs on the quake return. | Existing main route and controlled exterior Search. Keep the two existing stable battle-return recipes distinct. Later Map60 frontiers remain behind the conditions above. [Tower route](pre-voyage-campaign.md#battle06-result-through-usable-tower-exploration) · [Exterior Search](opening-session-lifecycle.md#tower-exterior-search-admission). |
| **Open Grans after the commission**, Map66 | Granseal and south Yeel connect through the southern/main component, with Mithril F213, the south shrine and the south end of the Galam drawbridge. Yeel north exit connects the separate Hawel/woods component and reverse Yeel north entrance. Battle02, the marked Battle03 request, Hawel's F404 request and the Battle06 outcome keep their own admission. Battle02 can return on an enclosed tile. | Battle routes, conditional repeat and field EGRESS have existing play evidence. The F213 marker and approach from (30,45) are **Confirmed** static and selected. The [southern shrine visit](#play-the-southern-shrine) reaches the lower chamber and returns to this field. A legal return from the enclosed Battle02 tile uses the existing recovery route. [Enclosed return](pre-voyage-campaign.md#enclosed-battle02-return-and-field-egress). |
| **Yeel before and after Hawel**, Map5 | Both town exits, houses and tavern, bookshelf/room descriptions, aboveground Church, Healing Seed F134, Kazin guiding/F72 and the F650 residents/dialogue after the house scene. F650 changes the entity and Talk tables while retaining Search and Church. | Default input, Kazin introduction and Church Record are observed. **Confirmed** in the published F650 world: a fresh arrival shows the changed residents, one speaks its replacement dialogue, and Kazin stays in the party with the guide flag clear. The aboveground Church records the visit. Search awards the seed once and retains F134 across cold Continue, exit and re-entry. The F530/F28 setups and piano/underground sequence are late. [Pinned F650 setup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map05/mapsetups/pointertable_650.asm) · [#2022 Save and Continue](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2022). |
| **Hawel**, Map43 | House rooms, story, bookshelf diary, well, F135 Antidote and F136 Healing Drop chests, south return (6,6) to Map66. Default scene and F612 quiet revisit. The init has no F503 or F72 predicate. | Recruitment, diary and F612 repeat/exit observed, chest rows selected. **Unknown** ordinary acquisition of each chest on the settled route and carrying both into the next legal return. Existing Search suffices unless a specific approach/receipt fails. [Existing observations](pre-voyage-campaign.md#ordinary-hawel-house-and-kazin-recruitment). |
| **Galam first prison escape**, Map17 | F660 wake-up, F661 doors, north-stair attempt/displacement and F662 shortcut, passage with F206 Mithril, ladder and cellar, kitchen and maid. The Search marker for F205 Chirrup Sandals can be faced from the first prison-control section. F505 selects the altered return population/zone table and F663 owns the door reconstruction. | Escape, maid and kitchen stairs observed. F205/F206 selected. **Inferred** F205 approach with the live cast and F206 approach just after the passage scene settles. **Unknown** ordinary acquisition/carry. Keep Chirrup Sandals Equip/footstep behavior with [#1962](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1962). Door/cutscene behavior should not be replaced with a bare set of progression flags. [Prison source and lifecycle](../../../docs/research/chapters/grans-opening/battle04-to-battle05-content-route.md) · [Equipment reach](../content/special-equipment-reach.md#chirrup-sandals). |
| **Galam town and palace rooms**, Maps16/17 | South court, houses, tavern, shops and Church, kitchen/side stairs, throne room to royal chambers, treasury approach to F137 Fairy Powder and F138 Healing Water, and the east gate tower interior via (49,9) after the door copy. F207/F208 herbs in town. First Map16 rally, changed F663 cast and F505 Battle05 return/revisit. The northern exterior and its F200 Evil Shot stay behind the F999 gates. | Default and F505/F663 consumers and both supply/weapon Shop tables are present on the applicable setups. Rally, paid supplies and named return inputs are observed. **Unknown** ordinary entry/exit and resident/placement reach at the optional treasury, royal rooms and east tower in those live casts. Check them from the settled Battle05 return without bypassing the guards. [Galam routes](pre-voyage-campaign.md#galam-escape-through-battle06-first-control) · [Source transitions](../../../docs/research/chapters/grans-opening/battle05-to-battle06-content-route.md). |
| **Galam drawbridge**, Map18 | South approach from Map66 can enter before F611 and return without crossing. F611 opens both the upper part to town and the CURRENT top stair (2,8) → (41,55), with its reverse. Talk to trader, resident and guards, including the upper guard. Search the sign (12,9). The trader's jewel dialogue is no sale or item removal. | Existing F611 south exit and trader Talk. Top stair/upper guard and sign are installed. **Unknown** ordinary witness for the upper visit and the closed-bridge early turnaround. [Pinned Map18 source](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map18). |
| **Earthquake and departure**, Maps4/44 | F640 changes the Granseal arrival to Map4. First quake, F624 reconstruction, bookshelf and F217 Mithril are accessible on the surviving streets. Three east exits reach Map44. F609 and F506 offer a pre-quake dock visit with their own casts. The deck and lower hold connect by CURRENT stairs. Keep F507 cast, captain No/cancel/Yes, return west through the active town substitution, and F139/F140 shared Search receipts. | Ordinary quake return and docks/ship Search are already covered, including repeat and full-pack control. Map44's retained malformed default handler has no `$1400` marker in either active base area and the map's flag/step tables are empty. The reachable lower hold, ordinary departure and known Search actions do not need its handler. **Unknown** ordinary F217 grant and optional F506/F507 Talk beyond the named departure inputs. [Docks source](../../../docs/research/chapters/grans-opening/earthquake-town-through-docks-departure.md#selected-zone-table-anomaly) · [Search acceptance](campaign-field-search.md#docks-and-damaged-ship). |
| **Departure handoff**, Maps73/14 | Captain Yes commits departure. Map73 init carries the story to the first stable Map14 below-deck state. The subsequent plank prompt, deck and short Battle08 boundary share the same party and receipts. Map14's copied F139/F140 records have no live item marker. The F458 nonvictory Church and later land connection have their own campaign owner. | Existing departure and first stable control are regression boundaries. Retain NPC/crew dialogue and blocked inventory/menu while the story controls movement. Do not duplicate dock rewards or add arbitrary cabin saves. [Voyage owner](pre-voyage-campaign.md#tower-return-through-first-damaged-ship-control) · [Plank and deck](map14-plank-deck.md#play-and-review). |

## Pre-voyage save coverage

[#2029](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2029) owns the remaining
save gaps at existing stable locations. The [field selector](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OpeningRestore.cs)
confirms the recipe gaps. The [save lifecycle](opening-session-lifecycle.md#planned-pre-voyage-settled-visits)
owns the restoration and Church publication contract. Each new location requires its own Save
and cold Continue observation, with Record where a real priest is admitted.

**Confirmed coverage.** Map3 default/F609 retain their anchored recipes. The completed Battle01
Map57 and Battle07 Map58 returns retain their separate anchor gates. Default and F650 Yeel use the
settled-visit recipe. F506 Granseal and the early Map16 Galam visits have separate settled-visit
recipes. Ordinary Save preserves
the incoming Egress and F399. Church Record publishes the town refuge and F399 after slot
readback. [Granseal acceptance](opening-session-lifecycle.md#granseal-f506-save-and-continue-observation) ·
[Yeel acceptance](opening-session-lifecycle.md#yeel-save-and-continue-observation) ·
[Galam acceptance](opening-session-lifecycle.md#galam-town-save-and-continue-observation)

### Ordered implementation batches

Dispatch the remaining batches in order, deferring a batch if its prerequisite has a concrete
unresolved failure. Each batch uses
the [shared acceptance](#save-batch-acceptance). Save-only batches preserve the incoming
Church/Egress state.

1. **Granseal F506, Map3 (implemented).** The finite visit recipe saves the changed layout, actor
   aliases and Kiwi's current follow state. Cold Continue restores the offer after a refusal and
   preserves the completed named join. Ordinary exit and reentry retire the resident. Record uses
   the Map3 refuge and the slot keeps the settled visit's tile. F543, active naming and choices,
   quake Map4 and non-town visits stay outside this recipe.
   [Observation and private inputs](opening-session-lifecycle.md#granseal-f506-save-and-continue-observation)
2. **Galam town, Map16 (implemented).** Save and cold Continue cover the default visit,
   same-visit post-rally, fresh F663 population and settled F505 return. Continue keeps the
   actual cast, layout and Slade's follow or roster state. Record uses the Map16 refuge from
   either admitted priest. F530/F999, active rally/guard/battle work, Map17 and Map18 stay outside
   this recipe. [Observation and private inputs](opening-session-lifecycle.md#galam-town-save-and-continue-observation)
3. **Quiet Hawel visit, Map43 F612.** Save only after the completed house sequence and on the
   concrete F612 revisit. Check the live first-completed cast against the reentry cast and bind
   each through the settled recipe as needed. Preserve F4/F36 as applicable, cleared F72,
   F612/F650, the committed F404 value and house Egress. Cold-continue with one Kazin and a usable
   diary, check a reachable F135/F136 receipt and take the south exit. If that departure selects
   Battle04, reach the normal request boundary once. Exclude the house scene, unfinished
   recruitment and battle handoff. Hawel's source refuge at (7,5) Down has no Church caller.
4. **Castle and palace rooms, Maps19/20/21.** Save only in the existing opening, first-return,
   commission and post-Galam rooms, including settled Map19 F507 return. First verify the
   selected cast and actual CURRENT-stair route. Bind the live setup and scene, including F129–131,
   the bedroom/Astral choices, F620/F621/F83 and F622/F623 effects where applicable. Retain the
   actual royal follower chain independently of combat membership. Cold-continue the distinct
   changed casts, an upper room and the settled royal return. Verify collision, one chest, real
   stairs/exit and no repeat warning or join. Exclude unadmitted rooms, active guard/exorcism and
   Battle01 admission in progress.
5. **Tower traversal, Maps40/57/60/58.** Save only for settled exterior control, the F506-open
   Map57 staircase, the connected early Map60 descent and reachable Map58 return area. Use the
   accepted tower route and reverse. Bind its setup, layout and royal followers through the
   settled-visit mechanism, retaining F132 and the F506 stair copies. Keep current Map57/58
   completed-outcome recipes under their existing gates. Check Lemon's before/after interaction,
   a legal F132 receipt, the Map57 stair and a Map60/Map58 portal after cold Continue. Exclude
   active battle/admission/outcome and the F999 frontiers, Map59 and Map56. The source Ancient Tower
   Ascent refuge belongs to Map56.
6. **Galam interiors and drawbridge, Maps17/18.** Save only. Consume batch 2 and prove ordinary
   reach from the F505 palace return and on the closed/open F611 bridge visits. Retain the actual
   F661/F662 door/shortcut effects, F663 reentry copies and F611 layout. Cold-continue the F505
   room and admitted bridge visits, then use the real stairs and exits and check a reachable
   F137/F138 or F205/F206 receipt. Include a settled early prison visit only after verifying its
   corresponding doors, four-companion or F73 Slade follow state, and next legal movement without
   wake-up replay. Exclude active wake-up, passage staging, battle requests, F530 and the F999
   gate.
7. **Open Grans, Map66.** Save only on settled legal walking ground in the known southern and
   north-Yeel/Hawel components. Consume the town/house return checks above. Scope the recipe to
   admitted pre-voyage phase and traversal data. Retain follower state, F213, route flags, counters,
   RNG and incoming Egress. Cold-continue a position in each component and take real town/house
   transitions. Check an eligible F213 receipt and one applicable battle-request boundary.
   Keep the enclosed Battle02 return on its existing recovery route until the party reaches
   admitted ground. Reject pending battle/warp and leave northern/late areas, vehicles and the
   southern shrine with their existing owners.
8. **Quake town and docks, Maps4/44.** Save only in F640 quake town after F624 settles and in
   ordinary F609/F506 pre-quake and F507 departure dock visits. Use the completed tower/castle
   return or the applicable earlier dock corridor. Retain six quake layout copies, parked guards,
   royal followers, F217 and shared F139/F140 dock receipts. Save the actual dock setup and keep
   CURRENT stairs and the F640 return substitution. Cold-continue Map4, walk the real crack-safe
   exit and return west to Map4. Cold-continue earlier and F507 dock visits, including the captain's
   settled No/cancel, legal hold/deck stairs and a one-time receipt. Confirm Yes starts the existing
   sequence once and refuses capture while it controls the scene. Exclude active quake/boarding,
   Map73 handoff and Map14 cabin. Map4's source refuge supplies no active Church caller.

Map41/42 new location content stays with [#2020](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2020).
Yeel F650 closure stays with [#2022](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2022).
Battle27+ and #1716 are outside these batches. The
[source anchor table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/global/savepointmapcoords.asm)
provides refuge coordinates. A field Continue recipe requires its own admitted landing and Church
Record requires a real caller.

### Save batch acceptance

- Start from a compatible copied slot or controlled legal predecessor with initialized party
  values. Enter through ordinary input. Use the actual Save entry and, where offered, talk to the
  priest and choose Record after the normal service return. Inspect the preview and confirm
  the slot. Use existing native input, state, projection and error observations. Keep inputs short.
- Exit the writer. Use independent title Continue processes for the new ordinary and Record slots,
  with no controlled start on the reader. Compare recorded progress and scene, then take the batch's
  next ordinary input and exit or reentry. Check Record's committed F399/Egress values and the
  refuge binding separately from the Continue landing.
- Load a previously compatible slot in each candidate. Use the default/F609 Map3 anchored and
  Yeel v3 cases across the town batches. Prefer an adjacent legacy recipe for the other batches.
  If shared wire or compatibility handling changes, also load a representative existing battle
  slot. These recipes keep their landing and accept the next ordinary input.
- Seed an occupied disposable slot and compare primary and backup bytes after cancel or a
  representative typed refusal. Check stale/busy capture and missing or mismatched required scene
  admission at the affected boundary. Exercise the Church publication failure in both town batches
  with the existing disposable-slot method. Compare live F399/Egress and verify usable field
  controls after failure. Reuse the accepted storage evidence for wider atomicity and recovery.
  [Storage and restart](../verification/host.md#opening-save-and-continue) · [Church failure witness](../verification/host.md#church-and-shared-save-calibration)

## Play the southern shrine

The `derived/issue2020/south-shrine-v1` publication extends the Yeel F650 world at gameplay
revision 4. Map66's southern warp uses the existing fresh-load rule. The Map41 door changes
the working marker before dispatch, and the two Map42 stairs use the current-map transfer.
Search commits the item, F133 and opened chest together. Map41/42 use ordinary field control.
The next supported Granseal or Yeel savepoint owns persistence. The source's F508, F999,
F901 and F930 conditions retain their original positions. Late head-opening, attraction and
sword-extraction scripts have explicit frontiers at their own calls.

**Confirmed in the remake.** A four-member party continued a compatible Granseal slot from the
preceding Yeel F650 world. Its controlled pre-session input used the post-Battle02 membership
and flags in [the field start](../../reference/inputs/yeel-to-battle03-start.json), four
occupied slots per member, the members' weapons and Medical Herbs in the spare slots.
The prior-world session saved at the supported church approach. Run the following inputs with a
copy of that slot, or prepare the same legal field and inventory conditions before starting.

1. Leave Granseal west. Walk from Map66 (33,44) onto (33,43). From Map41 (23,5) press Up into
   the closed door. At Map42 (7,14) walk to (2,9) and descend to (19,17).
2. Cross (19,21) with F999 and F901 clear. The attraction does not start. From (26,19) press Up
   against the blocked sword-room entrance. The Evil Axe location at (4,3) has no chest marker.
3. At (19,23) face Down and Search. With all four packs full the chest stays closed, F133 stays
   clear and the packs retain their contents. Open Party, choose Drop, confirm one Medical Herb,
   then Search again. Bowie receives Power Water (item 9). A second Search preserves custody.
4. Take (19,16) back to (2,8), leave the hall at (7,15), and return through (23,15).
   Enter the shrine again and take (24,15). Both south exits land at Map66 (33,44).
   Walk onto (35,28) with F508 clear and remain on Map66. Return through (30,32) to Granseal
   and use system Save. Continue in a fresh process. F133, member order and every member's
   item slots match the saved record. Bowie's pack holds his weapon, two herbs and Power Water.
5. A separate byte-identical copy of the preceding publication's six-member F650 Yeel slot
   continued at Map5. Leave through the **south** village exit, take the Map66 southern entrance,
   open the door and descend. The six-member party and F650 survive to the lower chamber.
   The original's [member lists](caravan-services-design.md#evidence-and-the-first-consumers) scan active flags
   by member ID. The two runs exercise different legal memberships in that order.

### Rebuild and select the private inputs

Load the [worktree configuration](../../../docs/operations/local-private-inputs.md). Set
`$upstream` to the registered c834c652 checkout and `$fresh` to a **new absolute directory**
under worktree `local/` or the registered shared `derived/`. Use the existing prepared library
and canonical map import. The producer writes its closed package after validating the source
and binding missing resources.

```powershell
. ./local/private-inputs.ps1
$derived = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived'
uv run --locked python -m sf2tool.remake_retail_assets --family south-shrine --mount $fresh `
  --root (Join-Path $derived 'issue2001/retail-v1') --upstream $upstream `
  --canonical (Join-Path $derived 'canonical-map-import.json') `
  --base (Join-Path $derived 'issue2022/yeel-f650-v1')
```

For ordinary play, use [the existing world/scene/entry and six opening-input selections](opening-session-lifecycle.md#yeel-save-and-continue-observation)
with `$package = Join-Path $derived 'issue2020/south-shrine-v1'`. Use the owned Debug adapter and
an external disposable save namespace for observation.

The prepared Map41 and Map42 atlases are 11,022 and 12,500 bytes. The command 9/C4
`MUSIC_INTRO` WAV adds 19,229,088 bytes outside the world. Item IDs 44 and 84 remain in the
source placement tables behind their early gates. Admission requires their normal and selected
icon bindings in the field and the 26 mounted encounter scenes. Power Water's icons were already
bound. The world grew by 105,737 bytes to 102,142,413 bytes, below the 128 MiB reader limit.
The 146-file package retains 118 dependencies byte for byte. It preserves the preceding package's
[Map23 F762 limitation](../content/campaign-locations.md#find-a-place).

The first candidate failed admission with `identity-icon-binding` before those four icon keys
were mounted. Early observer attempts tried a blocked western approach to (26,18) and a
checkout-local save root. The successful cases used the southern approach and an external
disposable root. Cold Continue compared numeric flags at the JSON wire type. Godot emitted its
two-object shutdown warning on the cold and legacy reads. Both exited successfully.

## Implementation order and acceptance

1. **Southern shrine** ([implemented visit](#play-the-southern-shrine)). Select Map41/42 and their early Search content through the existing
   private package producer. Admit the Map66 row 8 route, Map41 step-copy door and all return rows
   needed by the two early areas of Map42. Use the existing door/marker order, `Preserve` stair
   transfer, fresh-load reconstruction and item receipts. A later row can remain owned by the
   source's existing marker/copy prerequisites. Preserve the hard boundary for unadmitted later
   scripts or maps at their own callers. Prove both Map41 south exits and the round trip from
   Map66, including a blocked-valued warp trigger. Try Search before/after the Power Water grant,
   on a full pack and after making room. Walk the F999/F901-clear gate and check the Evil Axe
   absence and no attraction/extraction. Check F508-clear (35,28) remains an ordinary tile.
   Keep changed membership/order through the actual transfers and carry F133 through the
   next already supported Save and cold Continue. A previous compatible slot can Continue and
   take the new route. Keep the existing gameplay/recipe identity unless the save owner proves
   a change is required.
2. **Existing interiors and service closure.** Use the named Unknowns in the table for small
   walking/interaction checks through the admitted content. Pair Hawel and Yeel around the post-house
   branch. Pair Galam throne/treasury, prison passage/receipts and drawbridge top with F611/F505/F663 as
   applicable. Pair castle rooms, tower return and quake/docks around their actual setups. Check
   opened rooms, valid Search approaches, absent rewards from locked or unmarked cells, resident
   dialogue and service entry/return. A selected room that works needs only its observation.
   Assign a concrete failure to its content or service owner. Use the existing Shop and Church
   contracts for actual offered actions, including eligible promotion/repair and empty/ineligible
   outcomes where available. Separate item use/equip gaps from receipt acquisition and respect
   the existing item/service issues. Give the feature trace and campaign flag/item indexes the
   accepted user-visible changes after the owners integrate their slices.

For an implementation group, begin from valid staged state or a copied compatible slot. Check
ordinary input at the real incoming transition, the relevant interior interactions, outward
transition, expected flags/custody/party or refusal, and the next legal persisted state. Inspect
input, state, projection and errors in a short owned Godot observation. Use affected engine
behavior tests and the adapter compile where code changes. A source-reach ambiguity can warrant
a short original input check at that exact seam. A controlled legal prerequisite is enough to
exercise walking, inventory or an eligible service. No leveling grind, long route replays,
screenshots, arbitrary save locations, extra capture infrastructure or Battle27+ work.

Reuse the [#2001 prepared library](../content/asset-inventory.md#retail-original-preparation).
The first group is two map payloads, 8,192 base layout words, their tables and the already
prepared music, atlas and item families. The raw layout alone is 16 KiB, and the serialized
layout and table delta is expected to be well below 1 MiB before presentation. Read the existing
atlas/cue sizes before mounting, count only missing bindings and verify headroom under the
128 MiB world limit. A ~102 MB input leaves ~32 MB decimal headroom before the new presentation
data. Use one owned incremental candidate. Rebudget if a required new family or the existing
world limit changes the estimate. Keep publication and compatibility proof in the implementation
slice and all generated bytes local.
