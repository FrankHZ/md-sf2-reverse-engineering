# Hassan to Taros Shrine

The western exit continues the [Kraken and Hassan chapter](kraken-hassan.md). The starting save has eleven retained members and three living. It has neither the Achilles Sword nor the agreement with Rohde. The [source contract](../../../docs/design/synthesis/chapters/southern-travel-creed/hassan-departure-and-battle17-outcomes.md) owns the original behavior. The [southwestern Parmecia contract](../../../docs/design/synthesis/chapters/southern-travel-creed/southwest-parmecia-through-taros-battle18-entry.md) owns the shrine threshold.

## Walk from the Hassan save

In the prepared checkout use `./local/issue1806/play.ps1 en play-en` or `./local/issue1806/play.ps1 zh-CN play-zh`. Choose **Continue → slot 1**. A new sample name gives the player a separate copy. Names starting with `full-` use the eleven-living save obtained by paying the Hassan priest 980 gold.

From the church counter at (8,9), walk Down three, Right four, then Up twelve. If a wandering villager blocks the aisle, wait for them to move. The exit places Bowie at Map69 (26,54) facing Down. One step Left reaches (25,54) and starts Battle17 on the first visit. In battle, arrow keys move the preview, Enter accepts the destination, F chooses Attack, H opens Magic and Space chooses Stay. Protect Bowie. The party can return to the church during training, revive fallen allies and retry with earned experience.

## Observed in the running remake

| Boundary | Observation |
| --- | --- |
| First departure | **Confirmed** from the preceding unedited save. The walking party kept 9,923 gold. F509 and F516 were set, F409, F417 and F517 were clear. The western zone set F417 and the encounter counter to 30000. The preceding package returned `battle-entry-unavailable` for Battle17. |
| First battle input | **Confirmed** with the private Battle17 trial. The 30×32 crop starts at Map69 (0,31). The source has twelve ally formation cells, fourteen enemies of six species, two AI regions and zero points. Dead members stayed in the roster. The three living allies were Bowie (24,23), Peter (24,24) and Luke (24,22). The Arrow Launcher uses its brass-gunner terrain costs. The Soulsower can stun a surviving target. |
| Paid Egress | **Confirmed**. Bowie used his eight MP. The first attempt returned to Map9 (8,9) Up with 9,923 gold. The raft was at Map69 (28,55). F417 remained set and F517 remained clear. |
| Leader defeat | **Confirmed**. Bowie fell through ordinary combat. The return reached the same church and berth with 4,961 gold. On leaving the town again the retained F417 admitted a fresh Battle17, including all fourteen enemies. |
| Victory | **Confirmed** from the eleven-living Hassan save after training, paid resurrections and further departures. Victory over the last Black Monk in round 14 placed Bowie at Map69 (27,55) Down from battle cell (27,24). Gold was 17,363, F517 was set and F417 cleared. |
| Shrine | **Confirmed** after a subsequent ordinary victory. From (27,55), walk Up once and Left 27 to Map68 (62,54), then Left seven and Up eleven. Map12 starts at (12,30) Up. The warning displayed all seven lines in English and Chinese. The Chinese run returned to (55,44) Down and resumed walking. |

## Selection and return contracts

Battle09 precedes Battle17 in Map69's source table. An outstanding F409 therefore takes priority over F417, including the retail completed-battle readmission rule. With neither unlock set, departure admits field input. Walking into the designated zone can grant F417. A raft step uses vehicle dispatch. Egress and ordinary defeat retain an incomplete battle's unlock. Victory clears F417, sets F517 and projects the first active force member's battle position into Map69 using the crop origin and their current facing.

From the western edge of Map69, the scroll crosses to Map68 at X62 with the same Y. The shrine gate at (55,43) enters Map12 at (12,30) Up. The source checks an outstanding Battle18 unlock before map initialization. With F418 and F725 clear the warning uses text IDs 3197–3203 and returns to Map68 (55,44) Down. With F725 set the warning is skipped. The party from the subsequent sword and Rohde chapter has a separate integration acceptance.

## Reproduction and scope

The private package uses the registered USA ROM and SF2DISASM commit `c834c652b6862bc5679fd7f69a38a7093206efc6`. The `BattleSpriteset17` source and table-driven map, background, prowess and movement selections provide the combatants and presentation. The ordinary product loaders, battle rules and Godot main scene handle the observations. Inputs, captures and generated assets stay in the prepared checkout. See the [Battle17 opening research](../../../docs/research/chapters/southern-travel-creed/battle17-initialization-and-first-control.md) for original first-control evidence.

The preparation and runs under `local/issue1806` use isolated trial registrations while the campaign registry and return policy are serialized. The shrine observations use the swordless party. The post-Rohde party and Battle18 need separate acceptance. DETOX level two is outside this chapter’s supported spell actions. `uv run sf2 verify engine` exercises engine behavior and `uv run sf2 verify adapter` compiles the adapter.
