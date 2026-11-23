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
unsettled conditions. The location tables distinguish static review from the linked remake observations.

The initial remake survey compared the [tracked selection](../../content/private/pre-voyage-opening.json),
the existing private Phoenix-wave publication (baseline `33673735` in
[the player report](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456#issuecomment-7069929011)),
and the consumers at the accepted entry baseline. Inspect map/variant IDs, event kinds, Search
receipts and reachable program references in `world.maps` and `world.programs`. Read program
branches and marker sites before classifying a retained `native-call` or missing variant.
The package provides static admission evidence. The linked campaign observations give the
narrower played boundaries.

The initial static survey is bounded by about 25 map layouts including rejected neighbors, each 64 × 64. Treat
each distinct story layout as one local walk from its admitted arrivals. This is linear in the
visited cells and transitions per state, roughly 100,000 cells for one pass over those layouts.
One checked retail image is 2 MiB. The existing world is about 102 MB on disk and was inspected
read-only in memory. Evidence here retains route and gate facts only. The static survey reuses
the existing inputs and decoder. Expand beyond a named neighboring map only when a pre-voyage
arrival reaches it.

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

A Search receipt requires a faced marker, an accessible approach and inventory capacity. The
[Search inventory](campaign-field-search.md#map-and-setup-inventory) records the compiled
placements and descriptions. The Galam and Granseal reviews below give the specific source,
binding and ordinary-input evidence.

| Phase and place | Accessible branches and setup changes | Remake coverage |
| --- | --- | --- |
| **Opening, commission and occupied town**, Map3 Granseal | Home and school CURRENT stairs, town houses and their doors, residents, messenger Yes/No/retry, Church, F600 castle admission and F603 west-exit guard. F609 and F506 change the cast and dock approach; F506 opens Kiwi's cell. | Opening dialogue, Church, school Search F128, F609 shops and F506 Kiwi refusal/revisit/join have play evidence. F506 uses the same two Shop bodies. F220 is an empty chest whose facing square remains occupied. [Town audit](#granseal-and-outlying-interactions) · [Kiwi visit](pre-voyage-campaign.md#battle06-result-through-usable-tower-exploration) |
| **Opening, exorcism, commission and return**, Maps19/20/21 | Map19 courtyard, armory, guard quarters, storeroom chests and west/east lower rooms. Map20 west stairs, royal rooms and east tower. Map21 guard and tower-exterior exit. | F609 armory and both lower-room returns have ordinary-input checks. F130, royal and east CURRENT stairs, the commission, F506/F507 royal chains and guard-room exit retain their accepted observations. [Castle audit](#granseal-and-outlying-interactions) · [Castle Save and Continue](opening-session-lifecycle.md#granseal-castle-save-and-continue-observation) |
| **Tower exterior, return, second descent**, Maps40/57/60/58 | The exterior links the castle and antechamber and permits F132 Mithril. Map40 F506 Lemon approach, F507 default restoration, Map57 first battle/return and F506 stair opening, Map60 connected early chain and Map58 return. Map60/58 have no interaction cast or placements to add; preserve party and exit through the actual connected stairs on the quake return. | Existing main route and controlled exterior Search. Settled traversal uses v6 Save and Continue. The completed-return anchors apply to their legacy slots. Later Map60 frontiers remain behind the conditions above. [Tower route](pre-voyage-campaign.md#battle06-result-through-usable-tower-exploration) · [Exterior Search](opening-session-lifecycle.md#tower-exterior-search-admission). |
| **Open Grans after the commission**, Map66 | Granseal and south Yeel connect through the southern/main component, with Mithril F213, the south shrine and the south end of the Galam drawbridge. Yeel north exit connects the separate Hawel/woods component and reverse Yeel north entrance. Battle02, the marked Battle03 request, Hawel's F404 request and the Battle06 outcome keep their own admission. Battle02 can return on an enclosed tile. | Battle routes, conditional repeat and field EGRESS have existing play evidence. F213's loose placement and southern facing approach are **Confirmed** static. [Search comparison](#granseal-and-outlying-interactions) The [southern shrine visit](#play-the-southern-shrine) reaches the lower chamber and returns to this field. A legal return from the enclosed Battle02 tile uses the existing recovery route. [Enclosed return](pre-voyage-campaign.md#enclosed-battle02-return-and-field-egress). |
| **Yeel before and after Hawel**, Map5 | Both town exits, houses and tavern, bookshelf/room descriptions, aboveground Church, Healing Seed F134, Kazin guiding/F72 and the F650 residents/dialogue after the house scene. F650 changes the entity and Talk tables while retaining Search and Church. | Default input, Kazin introduction and Church Record are observed. **Confirmed** in the published F650 world: a fresh arrival shows the changed residents, one speaks its replacement dialogue, and Kazin stays in the party with the guide flag clear. The aboveground Church records the visit. Search awards the seed once and retains F134 across cold Continue, exit and re-entry. The F530/F28 setups and piano/underground sequence are late. [Pinned F650 setup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map05/mapsetups/pointertable_650.asm) · [#2022 Save and Continue](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2022). |
| **Hawel**, Map43 | House rooms, story, bookshelf diary, well, F135 Antidote and F136 Healing Drop chests, south return (6,6) to Map66. Default scene and F612 quiet revisit. The init has no F503 or F72 predicate. | Recruitment, diary, F612 repeat/exit and ordinary F135 acquisition are observed. Continue keeps the one Antidote receipt. The shared F136 chest binding and legal facing approach are **Confirmed** static. [Search comparison](#granseal-and-outlying-interactions) [Campaign route](pre-voyage-campaign.md#ordinary-hawel-house-and-kazin-recruitment) · [Save observation](opening-session-lifecycle.md#hawel-house-save-and-continue-observation). |
| **Galam first prison escape**, Map17 | F660 wake-up, F661 doors, F662 shortcut and F73 passage staging. F505 changes population and zone dispatch; F663 reconstructs prison doors. | Escape, maid and kitchen stairs have ordinary play evidence. F205 sandals and F206 Mithril each have pickup, custody and empty-repeat observations. [Galam audit](#galam-optional-exploration) · [F205 controls](../content/special-equipment-reach.md#remake-controls-and-reproduction) |
| **Galam town and palace rooms**, Maps16/17 | The throne and royal rooms, guarded treasury vestibule, east gate tower, town residents and services have distinct entry, population or Search conditions. | Ordinary post-Battle05 input reaches and returns from the royal rooms and both east tower landings. The fixed treasury guard keeps the two chests out of early reach. F207 has pickup evidence and F208 has a separate confirmed marker and legal facing tile. [Galam audit](#galam-optional-exploration) |
| **Galam drawbridge**, Map18 | The south shore can be visited with F611 clear. F611 opens the town connection and the upper CURRENT stair. | Ordinary input enters and returns at the south shore with either F611 value. The open visit reaches the upper guard, sign and dialogue-only trader. [Galam audit](#galam-optional-exploration) |
| **Earthquake and departure**, Maps4/44 | F640 selects Map4. F624 reconstructs the quake layout on reentry. F217 and shelf descriptions remain on the surviving streets. The dock setups change their cast and access to the CURRENT hold stairs. | F609 hold/barrel Search and F507 captain No/cancel/Yes are observed. Ordinary F507 input reaches the queue, where fixed residents block the gangplank. The dock west exit follows the active town substitution. The retained malformed zone handler has no active $1400 marker and these routes use other callers. [Dock and Search audit](#granseal-and-outlying-interactions) · [Departure owner](pre-voyage-campaign.md#tower-return-through-first-damaged-ship-control) |
| **Departure handoff**, Maps73/14 | Captain Yes commits departure. Map73 init carries the story to the first stable Map14 below-deck state. The subsequent plank prompt, deck and short Battle08 boundary share the same party and receipts. Map14's copied F139/F140 records have no live item marker. The F458 nonvictory Church and later land connection have their own campaign owner. | Existing departure and first stable control are regression boundaries. Retain NPC/crew dialogue and blocked inventory/menu while the story controls movement. Do not duplicate dock rewards or add arbitrary cabin saves. [Voyage owner](pre-voyage-campaign.md#tower-return-through-first-damaged-ship-control) · [Plank and deck](map14-plank-deck.md#play-and-review). |

## Galam optional exploration

[Issue #2058](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2058) uses Standard
rigor under the pre-voyage agreement. The audit compares the pinned USA source to the #2035
publication and tests the entry, return, interactions and exclusions that depend on active
population, a step copy, a changed layout or a distinctive caller. It reuses the accepted kitchen,
CURRENT stair and F205 evidence. Late Galam, the northern field and the #2059 locations are
outside this slice.

| Question | Original source and layout | Published remake |
| --- | --- | --- |
| Default, same-visit F663 and fresh Map16 | **Confirmed** static. The rally retires the army and sets F663. The same visit keeps its bindings. Fresh Map16 selects the F663 cast. F505 bypasses the Battle05 discovery zone and selects the Map17 replacement cast and zone table on fresh interior entry. Default Map17's list falls through to the seven shared residents before its terminator. | **Confirmed** #2035 observed both Map16 casts and the same-visit transition. The default and F663 priests offer Church. F663 town merchants 135 and 144 use the weapon and supply Shop bindings. The shared Map17 residents keep their Talk bindings, while F663 init retires the maid and rebuilds the prison doors. [Original caller order](../../../docs/research/chapters/grans-opening/battle04-to-battle05-content-route.md#entities-and-persistent-state) · [Accepted Map16 observations](opening-session-lifecycle.md#galam-town-save-and-continue-observation) |
| Throne, royal rooms and treasury | **Confirmed** static. The throne door at Map16 (17,14) reaches Map17 (11,8). The royal stair at (15,1) leads to Map16 (21,3). The separate treasury door at Map16 (10,15) reaches Map17 (5,9), where the fixed guard at (5,8) cuts the vestibule off from the stair at (2,3). His Talk leaves him in place. The guard is in the shared list on both early Map17 setups. The decoded Map16 treasury pocket has no outside walking approach from the audited early arrivals. | **Confirmed** after the controlled Battle05 return. Ordinary input takes the throne door, both royal stair directions and the throne exit. The treasury vestibule admits entry and return. Up from (5,9) is blocked, and Talk reaches the guard and leaves control at the vestibule. F137 Fairy Powder and F138 Healing Water have chest markers and placement rows inside the blocked pocket. [Pinned Map16 records](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map16) · [Pinned Map17 records](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map17) |
| East tower and north gate | **Confirmed** static. The (49,9) door applies the step copy from (54,1) before dispatching the warp. The interior staircase is diagonal. The east landing remains separate from the F999 north exit and F200 approach. | **Confirmed** after Battle05. Walk through (49,9) to Map17 (51,30), then Right twice to Map16 (51,3). Left off the landing and Right back onto it return to Map17 (53,29). The ground door at (51,31) returns to Map16 (49,10). Up from (46,8) remains blocked with F999 clear. [Tower and north-gate controls](../content/special-equipment-reach.md#evil-shot) |
| F206 Mithril, F207/F208 herbs | **Confirmed** static. The script settles Bowie at (29,18) with Slade following after F662 and F73. Search faces (29,16) from (29,17); its word is $DC2B. F207/F208 are separate chest rows with $D801 markers. The F663 supply merchant stands south of F208 at (35,24). The free approach at (36,23) faces Left into F208 from the shop interior. Original recipient selection checks Bowie's capacity first and then eligible force members. | **Confirmed** from controlled settled passage. Up to (29,17), face Up and Search. Bowie's third slot changes from 127 to Mithril 123, F206 commits, repeat Search preserves custody, and the real ladder carries the single copy into the cellar. F207's pickup and empty repeat are in the #2035 witness. F208's binding, marker and side approach are confirmed statically. Its individual pickup remains **Unknown** in ordinary input. [Field Search owner](../../../docs/research/world/field-search-control.md) · [F207 observation](opening-session-lifecycle.md#galam-town-save-and-continue-observation) |
| F611-clear bridge visit | **Confirmed** static. Map66 (31,8) retains its entry warp. Map18 (9,28) can step south to the Map66 return. The upper stair and town exit have base words $C061 and $C074 without warp markers. | **Confirmed** from the southern Map66 approach with F611 clear. Up enters Map18 (9,28), Up reaches the shore (9,27) and two Downs return to Map66 (31,9). The working layout retains those two closed cells. [Pinned Map18 records](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map18) |
| F611-set bridge visit | **Confirmed** static. Map18 applies the drawn bridge and roof copies. The default cast has trader, resident and two guards. The resident and lower guard are dialogue, the upper guard has its own caller, and the sign has a Search description. The trader uses dialogue, portrait and speech operations only. | **Confirmed** with post-Battle06 prerequisites. From the southern approach, take the stair at (2,8), speak to the upper guard from (50,57) facing Right, and descend at (41,55). The actual south exit returns to Map66. A separate ordinary entry from Map16 reaches the trader from (15,12) facing Up and the sign from (12,10) facing Up, with description 4083. The trader returns control with the same party inventory, gold and flags. Resident and lower guard placement and Talk bindings were compared statically. [Original trader and resident callbacks](../../../docs/research/chapters/grans-opening/battle05-to-battle06-content-route.md#dialogue-and-actors) |

All four decoded retail base layout arrays match the #2035 publication. The scoped
check found the named gates and interactions already admitted where the source permits them.
Independent original ordinary-input timing, ambient resident paths and individual original
reward acquisitions remain **Unknown**.

### Reproduce the Galam checks

Verify the registered USA ROM with `uv run --locked sf2 rom verify` after loading the worktree
private-input script. Use the decoder and movement procedure in [Source and method](#source-and-method).
Compare the decoded Map16, Map17, Map18 and Map66 arrays directly to `world.maps[].layout` in
`derived/issue2035/galam-town-v1/world.json`. For the treasury, compare the route from (5,9) to
(2,3) with and without the fixed (5,8) guard. The path exists on empty geometry and fails with the
guard. Check both Map17 setups and the shared Talk program. For F206, the ladder's blocked warp
word $D0D7 still dispatches on an attempted Up from (2,25). For F208, check the path from (37,24)
to (36,23) with the merchant at (35,24).

Use the actual Debug adapter and the #2035 world, entry, scenes and opening inputs together, with
an isolated local save root. Godot 4.7.2 .NET used the retained Debug build
`73b25797fe6c828a3c0f166d99aeb55248c60093`. The following accepted #2057 diff adds a read-only
pickup hint and factors the same marker/placement selection into shared methods. Use ordinary keyboard input and sample field state, projection and
errors at settled control. The [controlled input entry point](../godot/objective-guidance.md#verification-and-direct-replay) supplies only the prerequisite.

- Use #2035 `return-writer.json` for the palace routes. Its F505/F663/F664 state begins at
  Map16 (16,18). Enter and leave each branch through the gates in the table.
- For the closed bridge, stage Map66 (31,9), Up, after the commission with F611 clear.
  For the open bridge use the #2035 `egress-writer.json` post-Battle06 flags, with F406 clear,
  F407 and F611 set. Stage the same southern approach for the stair visit and Map16 (26,30),
  Down, for the trader and sign visit. Take the actual entrances and returns.
- For F206 use the documented Map17 prison start's flags plus F661/F662/F73, at the passage
  script's settled (29,18) Down. Walk to the faced tile and use ordinary Search and dismissal.
  The witness starts after the passage cutscene and checks item custody through the next ordinary transfer.

The closed shore, open stair, east tower, passage receipt and trader/sign cases completed with
native exit 0. The closed-shore interaction checks passed, and Godot reported
`WARNING: 2 ObjectDB instances were leaked at exit` on stderr. A warning-free shutdown remains
unverified for that case. The other successful cases had empty stderr.

The throne round trip completed before the same observation reached the treasury guard and stopped
with `treasury stairs:no-live-layout-path`. The focused guard check confirmed
the intended refusal by ordinary input. Other retained attempts include `field-view-area` with
F406 armed at the Map66 approach or return, a step into a solid neighbor on the east landing, an
early trader read before visible text, a longer prison run's timeout, and an out-of-area
`observation-mount-failed` at the passage script's interim (29,12). The successful controls use
the staged inputs above.

Settled field Save and Continue use the existing v6 snapshot to record the loaded setup, actors,
working layout and committed custody. [Snapshot mechanism checks](#remaining-save-coverage)

## Granseal and outlying interactions

[Issue #2059](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2059) uses Standard
rigor within the High pre-voyage agreement. The comparison covers default and early alternate
setups, the incoming and outgoing room requests, the residents' selected handlers, and the
working-layout conditions for the named Search rewards. It consumes the #2035 publication
and the accepted Galam, Yeel, shrine and campaign observations.

| Question | Source and published remake |
| --- | --- |
| Map3 houses, school and shops | **Confirmed** static. The six step copies cover Bowie's home, three town houses, the Church and school. The home and classroom transfers use CURRENT in both directions. Default, F609 and F506 keep the house connections, Search data and their own casts. The only legal facing cell for F220 is (7,18), occupied by a fixed merchant in all three setups. The row carries item 127 (empty). Default offers Church and has no Shop call. F609 and F506 each select the two shared Shop bodies and the Church body, with different NPC IDs. The opening dock approach has the fixed blocker at (45,22). F609 removes that obstruction by cast replacement. F506 additionally applies the separate cell-door copy. [Opening interactions](../content/opening-branch-inventory.md#offered-branches-and-consumers) · [Shop stock and inputs](../content/profiles-and-trust.md#ordinary-campaign-shops) |
| Map19 doors, chests and lower rooms | **Confirmed** static. The armory and guard-quarters steps at (15,17) and (16,28) copy the open tile from (41,0). The west and east lower-room doors at (19,15) and (35,15) have their own direct warp markers and reverse rows in Map20. The storeroom has three separate $D801 chest markers and different receipt rows F129–131. **Confirmed** from a controlled settled F609 visit. Ordinary input opens the armory door and returns from each lower room. F130 has the accepted pickup and repeat observation. [Castle recovery](opening-session-lifecycle.md#granseal-castle-save-and-continue-observation) |
| Castle floors and staged residents | **Confirmed** static. The diagonal west stair at Map19 (6,2) leads to Map20 (6,37). The separate Map20 (3,36) stair connects the Map21 guard room in both directions. The royal stairs link the two Map20 rooms with CURRENT. The separate Map19 east landing links two Map20 approaches, with the upper east stair using CURRENT. Fresh Map19/20/21 visit selection uses the default, F501, F609 or F506 setup as progress allows. F507 replaces Map19's setup and selects the shared presentation tail on Maps20/21 while those two floors retain F506's empty fixed cast. The royal followers have separate flag bindings. Pre-commission Astral and guard behavior keeps its accepted conditions. [Guard and royal input](pre-voyage-campaign.md#castle-bedroom-cast-after-astral-leaves) · [Tower return](../../../docs/research/chapters/grans-opening/post-tower-return-through-earthquake-entry.md) |
| Map44 dock visits and evacuation queue | **Confirmed** static. F609 supplies five residents, F506 one roaming worker and F507 thirteen fixed residents with ten explicit Talk bindings. The three (12–14,11) CURRENT boarding markers are free of fixed blockers on the F609/F506 setups. F507 stations an obstructing resident on each. The source movement controller checks actor obstruction before reading the warp marker. **Confirmed** with ordinary F506/F507 input. F506 selects the worker on entry from town. A separate facing approach reaches his Talk and returns through the west exit to Map3. In F507, the middle route blocks movement at (13,12), and Talk reaches the queued resident and returns to the same tile with unchanged flags, inventory and gold. The west exit follows F640 to rebuilt Map4; pre-quake it returns to Map3. The F609 hold and barrels and the F507 captain's No/cancel/Yes branches have accepted evidence. [Source dispatch and cast](../../../docs/research/chapters/grans-opening/earthquake-town-through-docks-departure.md#boarding-records-and-actual-entity-interaction) · [Docks and damaged ship](campaign-field-search.md#docks-and-damaged-ship) |

| Search receipt | Confirmed source and remake binding |
| --- | --- |
| F136 · Map43 Healing Drop | Chest (12,3), $D801, faced Up from (12,4). Default and F612 share Search. The settled post-house visit and quiet return lead past the adjacent F135 chest, which already has ordinary grant evidence. |
| F213 · Map66 Mithril | Loose item (30,44), $DDA1, faced Up from (30,45) in the southern component. F611 changes the drawbridge entry cell and leaves this marker in place. |
| F217 · Map4 Mithril | Loose item (35,6), $DC57, faced Up from (35,7) on the surviving street. F640 selects Map4 on the incoming Granseal request. The initial quake's changing copies and F624 reconstruction end at X34 on that side and preserve the marker and southern approach. The neighboring shelves use descriptions. |

The three placements bind existing item identities and independent receipt flags. The
[Search consumer](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldSearch.cs) uses the
faced working marker to select the chest or loose table. It gives the item to the first eligible
joined pack and commits the receipt with the grant. A full pack leaves the placement available.
Repeat Search gives no second item. Description Search leaves inventory and receipt flags intact.
Map14 retains the copied F139/F140 records without matching markers.

### Reproduce the Granseal checks

Load the registered private-input script and run `uv run --locked sf2 rom verify`. Compare the
pinned source's `data/maps/mapsetups.asm`, map-specific pointer tables, doors, warps, populations,
Talk handlers and placement tables to `world.maps` and `world.programs` in the #2035
`galam-town-v1` publication. Use the decoder in [Source and method](#source-and-method). The
Map3/4/19/20/21/43/44/66 base arrays matched the decoded ROM. All selected variants retain the
base Search table. For F624, check both the source init copy destinations and the fresh flag
copies. Marker admission uses the working word masked with $3C00. For the F507 crowd, compare
the three warp coordinates directly to the selected fixed entities and to the field controller's
obstruction check.

The short native checks used Godot 4.7.2 .NET and the retained Debug build from
`73b25797fe6c828a3c0f166d99aeb55248c60093`. The following accepted #2057 changes factor marker
lookups and add guidance glints. The movement, Talk and transfer code for these checks is the same.
Load the #2035 world, entry, scenes and opening inputs together. Use a fresh worktree-local save
root, [controlled start](../godot/objective-guidance.md#verification-and-direct-replay), ordinary
input and live state/projection views.

- **Castle.** Use the #2053 dock prerequisite, with F507/F624/F640 clear and F609/F610 set.
  Stage the Map19 armory approach at (15,18) Up. Step Up, read the copied $080B working cell,
  and step Down. Walk through the west and east lower-room doors from the courtyard and return
  through each room's own exit.
- **F506 docks.** Use the #2035 `egress-writer.json` post-Battle06 party and flags at Map3
  (49,24) Right. Enter east to the single-worker cast. To isolate the ambient timing, start
  the same visit at the reachable Map44 (10,24) Up. Talk while the worker is within reach.
  Dismiss text 626, walk west through (0,25) and confirm the Map3 return.
- **F507 docks.** Use the #2053 dock prerequisite with F74 and the earlier victory/progress flags,
  at Map4 (49,24) Right with F624/F640 held. Enter east. Walk to (13,12), press Up and Talk to
  entity135; its program presents text 1005. Dismiss and take the west exit to active Map4.


The castle, focused F506 Talk/return and F507 queue/return witnesses completed 11, 6 and 10
checks respectively with exit 0 and empty stderr. Earlier F506 attempts confirmed entry before
an ambient actor obstructed the observer's first approach or moved out of its facing range.
The first ended with a no-path assertion and `WARNING: 2 ObjectDB instances were leaked at exit`. Two later
attempts hit the observer time bound. The captured missed Talk fell through to ordinary Search
with the worker outside the interaction range. The short focused approach resolves the Talk
binding. An earlier castle check read the copied door correctly but asserted an empty marker
instead of the source's $080B. The local observations retain those results and the initial
observer parse failures.

## Pre-voyage save coverage

Ordinary Save uses the [settled field snapshot](opening-session-lifecycle.md#unified-settled-field-snapshots)
for supported live visits. The table records the earlier legacy observations and their setup limits.

| Location | Legacy coverage and limits |
| --- | --- |
| Granseal Map3 default/F609 and completed Battle01/07 returns | Legacy Map3 records use the opening or church anchor. `battle01-return` and `battle07-return` keep the recorded outcome anchor under their existing flag/landing gates. [Legacy field recipes](opening-session-lifecycle.md#eligibility-and-restoration-recipes) |
| Yeel Map5 default/F650 | Observed cold Continue retains the loaded visit, live population, follower links, Healing Seed receipt and Church Egress. Later F530/F28 have their own setup boundary. [Yeel observation](opening-session-lifecycle.md#yeel-save-and-continue-observation) |
| Granseal Map3 F506 | Retains the current layout and Kiwi's settled refusal or named join/follow state. Ordinary reentry retires the joined resident. Church Record uses the Map3 refuge. The F543 cast and quake substitution follow their own entry rules. [Granseal observation](opening-session-lifecycle.md#granseal-f506-save-and-continue-observation) |
| Early Galam Map16 | Legacy observations cover default, same-visit post-rally, fresh F663 and settled F505. Both priests bind the Map16 refuge. Ordinary Save now uses the settled v6 snapshot for the loaded visit. [Galam observation](opening-session-lifecycle.md#galam-town-save-and-continue-observation) |
| Hawel's house Map43 | Covers the completed default visit with Kazin following and the quiet F612 revisit. Keeps the diary, observed F135 receipt and ordinary exit. Map43's `(7,5)` Down refuge has no Church caller. [Hawel observation](opening-session-lifecycle.md#hawel-house-save-and-continue-observation) |
| Granseal Castle Maps19/20/21 | Covers the admitted settled default, F501, F609, F506 and Map19 F507 visits, including the loaded cast, royal links, receipt and CURRENT stairs. [Castle observation](opening-session-lifecycle.md#granseal-castle-save-and-continue-observation) |
| Ancient Tower Maps40/57/60/58 | Covers settled exterior control, the F506-open Map57 stairs, first five linked Map60 areas and settled Map58 aftermath. Retains F132, stair copies and physical royal followers. Actual later F999 frontiers, Map59 and Map56 follow their own entry rules. The Ascent refuge belongs to Map56. [Tower observation](opening-session-lifecycle.md#tower-save-and-continue-observation) |

Ordinary Save preserves the incoming Egress and F399. Where the accepted Record flow changes
them, it commits those values with successful publication. A source refuge row alone does not
establish a Church caller. [Church and Egress decisions](opening-session-lifecycle.md#eligibility-and-return-semantics)

<a id="remaining-save-coverage"></a>

### Mechanism observations and limits

[#2053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2053) uses the High recovery
contract and the existing private packages. The witnesses use ordinary save input, a different
process for title Continue, and a subsequent ordinary input. Controlled positions and progress
establish the current state, with natural reach limited to the linked play observations.

| Mechanism | Representatives and result | Observation limit |
| --- | --- | --- |
| Loaded body, working cells and next transfer | **Confirmed.** Native v6 Continue at the F611 drawbridge upper landing approach (42,55) takes Left to the same-map stair destination (2,8). On the F507 dock setup at (1,25), Continue takes Left to fresh post-quake Map4 with F624. The earlier [kitchen cutover](opening-session-lifecycle.md#implemented-field-snapshot-cutover) covers the Galam interior return. Engine controls cover same-visit F663 versus fresh entry, changed working cells and roof backing. | The closed shore, upper guard and palace-room routes use the separate [Galam observations](#galam-optional-exploration). A new setup still needs admitted bindings. |
| Physical state and ambient work | **Confirmed.** Earlier Galam and castle controls preserve resident slots, retirement, following and bound ambient work. The post-voyage pond spot check exercises walking Caravan context and the stationary facing/refresh/timer cycle. Engine comparison covers the next sprite request and wait. | Riding and pending sprite delivery remain outside the save boundary. |
| Committed pickup and pending ownership | **Confirmed.** Map42 Search at (19,23) grants one Power Water and sets F133. Save is unavailable during its receipt. After dismissal, v6 Continue at the chest preserves custody and repeat Search grants no second item. Map17 Search grants Chirrup Sandals and sets F205. v6 restores the worn word and repeat Search stays empty. The engine rejects unsettled transfer and battle ownership. [Southern shrine route](#play-the-southern-shrine) · [Map17 pickup and Continue](../content/special-equipment-reach.md#remake-controls-and-reproduction) | The F206 observation covers pickup and the next ordinary transfer. F217 and arbitrary interrupted scripts remain outside these samples. |
| Open-field movement, encounter and return context | The southern Map66 and separate north-Yeel/Hawel component use the same step and transfer mechanisms. [Settled field and battle return controls](opening-session-lifecycle.md#implemented-settled-field-candidate) compare signed step counter, next movement after victory, current landing and independent Egress. Battle01–07 use existing [battle evidence](battle-save-and-continue.md#implemented-ordinary-host-boundary-c). | No new Map66-specific cold restart, F213 grant or enclosed Battle02 recovery observation. The enclosed cell uses existing EGRESS recovery. |
| Post-voyage field and battle spot check | **Confirmed.** Cold v6 Continue at the Harpy Pond bank (10,9) retains the walking follower cast, prior victory progress, 13,264 gold and the parked raft berth. Left enters Battle20 through the confrontation. Ordinary battle Save writes v2. Cold Continue retains the player turn, RNG words, route and refuge. Stay advances to Bowie, who casts EGRESS for 8 MP and returns to Map6 with the carried gold and unfinished encounter. The battle primary keeps its bytes through Continue and play. | The witness covers one carried party and the Egress branch. Victory, defeat and retry have their existing [Battle20 observations](battle20-elric.md#leave-return-and-win). |

### Reproduce the mechanism checks

Use the [locked Debug adapter workflow](../development-and-verification.md#locked-net-workflow)
and the registered `handoffs/issue2053/snapshot-mechanisms` driver. The driver reads the #2035
published world and selects isolated disposable save namespaces and headless Dummy audio. It
stages only the controlled approaches or a copy of the #1847 pond slot. Each invocation launches
a separate native process. From the configured checkout root:

```powershell
. ./local/private-inputs.ps1
$runner = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs/issue2053/snapshot-mechanisms/Run.ps1'
$checkout = (Get-Location).Path
$stamp = [guid]::NewGuid().ToString('N').Substring(0, 10)
$reports = (New-Item -ItemType Directory -Path "local/issue2053-$stamp").FullName
foreach ($visit in 'bridge', 'shrine', 'dock', 'pond') {
    $namespace = "issue2053-$stamp-$visit"
    $seed = @()
    if ($visit -eq 'pond') { $seed = @('-PublishedSlot', 'pond-zh-CN.json') }
    & pwsh -NoProfile -File $runner -Checkout $checkout -ReportRoot $reports -Case "$visit-write" -Namespace $namespace @seed
    if ($LASTEXITCODE -ne 0) { throw "$visit Save failed" }
    & pwsh -NoProfile -File $runner -Checkout $checkout -ReportRoot $reports -Case "$visit-read" -Namespace $namespace
    if ($LASTEXITCODE -ne 0) { throw "$visit field Continue failed" }
}
& pwsh -NoProfile -File $runner -Checkout $checkout -ReportRoot $reports -Case battle-read -Namespace $namespace
if ($LASTEXITCODE -ne 0) { throw 'Battle20 Continue failed' }
```

Slot 1 starts from a copied older record for the pond case. The ordinary field writer publishes
v6 to slot 2 there. Left after cold Continue leads to Battle20, whose ordinary Save publishes
slot 3. The pre-voyage cases use slot 1 and their controlled starts. See the published handoff
for process results and preserved failures.

## Play the southern shrine

The `derived/issue2020/south-shrine-v1` publication extends the Yeel F650 world at gameplay
revision 4. Map66's southern warp uses the existing fresh-load rule. The Map41 door changes
the working marker before dispatch, and the two Map42 stairs use the current-map transfer.
Search commits the item, F133 and opened chest together. Map41/42 use ordinary field control.
Settled v6 Save and cold Continue are confirmed in the lower chamber. The source's F508, F999,
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

The [southern shrine](#play-the-southern-shrine) closed the missing early Map41/42 route and F133
transaction. The [Galam](#galam-optional-exploration) and [Granseal](#granseal-and-outlying-interactions)
reviews checked the remaining selected rooms, residents, gates and item bindings for #2020. The
review found no unresolved pre-voyage optional exploration defect in that agreed scope. It supports
closing #2020 after independent review. The early inaccessible treasury, evacuation hold,
Yeel underground, northern routes and upper tower follow their named source gates.

**Unknown:** individual ordinary acquisition of F136/F208/F213/F217, every resident and repeat
line in every phase, and a single natural run from New Game through departure that exercises all
admitted choices and visits. The static comparisons and controlled inputs substantiate the
specific connections and consumers above. The [#1456 human playthrough](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456)
retains its independent experience acceptance.
