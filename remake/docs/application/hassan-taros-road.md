# Hassan to Taros Shrine

The western exit continues the [Kraken and Hassan chapter](kraken-hassan.md). The starting save has eleven retained members and three living. It has neither the Achilles Sword nor the agreement with Rohde. The [source contract](../../../docs/design/synthesis/chapters/southern-travel-creed/hassan-departure-and-battle17-outcomes.md) owns the original behavior. The [southwestern Parmecia contract](../../../docs/design/synthesis/chapters/southern-travel-creed/southwest-parmecia-through-taros-battle18-entry.md) owns the shrine threshold.

## Walk from the Hassan save

In the prepared checkout use `./local/issue1806/play.ps1 en play-en` or `./local/issue1806/play.ps1 zh-CN play-zh`. Choose **Continue → slot 1**. A new sample name gives the player a separate copy. Names starting with `full-` use the eleven-living save obtained by paying the Hassan priest 980 gold.

From the church counter at (8,9), walk Down three, Right four, then Up twelve. If a wandering villager blocks the aisle, wait for them to move. The exit places Bowie at Map69 (26,54) facing Down. One step Left reaches (25,54) and starts Battle17 on the first visit. In battle, arrow keys move the preview, Enter accepts the destination, F chooses Attack, H opens Magic and Space chooses Stay. Protect Bowie. The party can return to the church during training, revive fallen allies and retry with earned experience.

## Observed in the running remake

| Boundary | Observation |
| --- | --- |
| First departure | **Confirmed** from the preceding unedited save. The walking party kept 9,923 gold. F509 and F516 were set, F409, F417 and F517 were clear. The western zone set F417 and the encounter counter to 30000. The preceding package returned `battle-entry-unavailable` for Battle17. |
| First battle input | **Confirmed** with the private Battle17 package. The 30×32 crop starts at Map69 (0,31). The source has twelve ally formation cells, fourteen enemies of six species, two AI regions and zero points. Dead members stayed in the roster. The three living allies were Bowie (24,23), Peter (24,24) and Luke (24,22). The Arrow Launcher uses its brass-gunner terrain costs. The Soulsower can stun a surviving target. |
| Paid Egress | **Confirmed**. Bowie used his eight MP. The first attempt returned to Map9 (8,9) Up with 9,923 gold. The raft was at Map69 (28,55). F417 remained set and F517 remained clear. |
| Leader defeat | **Confirmed**. Bowie fell through ordinary combat. The return reached the same church and berth with 4,961 gold. On leaving the town again the retained F417 admitted a fresh Battle17, including all fourteen enemies. |
| Victory | **Confirmed** from the eleven-living Hassan save after training, paid resurrections and further departures. Victory over the last Black Monk in round 14 placed Bowie at Map69 (27,55) Down from battle cell (27,24). Gold was 17,363, F517 was set and F417 cleared. |
| Shrine | **Confirmed** after a subsequent ordinary victory. From (27,55), walk Up once and Left 27 to Map68 (62,54), then Left seven and Up eleven. Map12 starts at (12,30) Up. The warning displayed all seven lines in English and Chinese. In both runs the party returned to (55,44) Down and resumed walking. |

## Selection and return contracts

Battle09 precedes Battle17 in Map69's source table. An outstanding F409 therefore takes priority over F417, including the retail completed-battle readmission rule. With neither unlock set, departure admits field input. Walking into the designated zone can grant F417. A raft step uses vehicle dispatch. Egress and ordinary defeat retain an incomplete battle's unlock. Victory clears F417, sets F517 and projects the first active force member's battle position into Map69 using the crop origin and their current facing.

From the western edge of Map69, the scroll crosses to Map68 at X62 with the same Y. The shrine gate at (55,43) enters Map12 at (12,30) Up. The source checks an outstanding Battle18 unlock before map initialization. With F418 and F725 clear the warning uses text IDs 3197–3203 and returns to Map68 (55,44) Down. A controlled copy of the normal victory save with F725 set and F418 clear reached stable input at (12,30) Up without the warning. A second copy with F418 set produced the Battle18 unavailable observation before any warning text. An independent town copy with F409 and F417 set selected the eleven-enemy Battle09, cleared F409 and retained F417. The party from the subsequent sword and Rohde chapter has a separate integration acceptance.

## Reproduction and scope

The private package uses the registered USA ROM and SF2DISASM commit `c834c652b6862bc5679fd7f69a38a7093206efc6`. The `BattleSpriteset17` source and table-driven map, background, prowess and movement selections provide the combatants and presentation. The ordinary product loaders, battle rules and Godot main scene handle the observations. Inputs, captures and generated assets stay in the prepared checkout. See the [Battle17 opening research](../../../docs/research/chapters/southern-travel-creed/battle17-initialization-and-first-control.md) for original first-control evidence.

Generate the encounter operands, scene and field poses with the standard producers and fresh ignored destinations. Load the registered private environment in the same shell. The campaign binds these operands with its existing static data, enemy tables, identity and command icons. The shrine displays Taros in a 48-pixel field pose with portrait 42. Gradual text plays his speech using command 112 at timer 189 on YM4. The pinned `sfx_30` header selects that slot. The initial gradual playback stopped at line 3197 with `sfx-slots-unavailable`; the corrected adapter started the 28,057-frame sound and continued through the warning.

```powershell
uv run python -m sf2tool.remake_exploration_battles --battle 17 --rom $rom --upstream $upstream --output local/road/inputs/battle17
uv run python -m sf2tool.remake_battle_scene_content --battle 17 --rom $rom --upstream $upstream --output local/road/scene
uv run python -m sf2tool.remake_battle_scene_content --field-death-base local/road/scene/battle-scenes.json --rom $rom --upstream $upstream --output local/road/field
```

In the prepared checkout, run `./local/issue1806/check-route.ps1 Egress en`, `Defeat en`, `Win en` or `Win zh-CN`. `Road zh-CN` starts from the ordinary post-victory save and reads the warning with gradual text and audio. The win shortcut starts from a normal church save after training and saves again through the menu after victory. The isolated flag checks are `Priority en`, `Skip en` and `Battle18 en`. The `Repeat26`, `Repeat27` and `First27` checks vary the unpromoted leader level and completion flag on disposable town copies. Level 26 admitted the completed base formation, level 27 stopped at `repeat-enemy-upgrade`, and level 27 with F517 clear admitted the first formation. The local handoff identifies the retained field save and the input, build and observation commands.

The observed party had no Achilles Sword or agreement with Rohde. The later party and Battle18 combat require separate acceptance. On completed Battle17 repeats the base formation is supported until the first possible enemy substitution at unpromoted Bowie level 27. Eligible high-level substitutions and DETOX level two are outside this chapter.
