# Hassan to Taros Shrine

The western exit continues the [Kraken and Hassan chapter](kraken-hassan.md). The starting save has eleven retained members and three living. It has neither the Achilles Sword nor the agreement with Rohde. The [source contract](../../../docs/design/synthesis/chapters/southern-travel-creed/hassan-departure-and-battle17-outcomes.md) owns the original behavior. The [southwestern Parmecia contract](../../../docs/design/synthesis/chapters/southern-travel-creed/southwest-parmecia-through-taros-battle18-entry.md) owns the shrine threshold.

## Walk from the Hassan save

Use the private `handoffs/issue1920/repeat17-standard` package for the retained Hassan save. Run
`./Play.ps1 -Language en -Mode interactive -Sample first17 -Seed ./slots/hassan-fresh17.json`
and choose **Continue → slot 1**. Use `zh-CN` for the Chinese interface. Each sample opens a
separate player copy. The trained church save already has F417 and selects the first formation
on leaving Hassan.

From the church counter at (8,9), walk Down three, Right four, then Up twelve. If a wandering
villager blocks the aisle, wait for them to move. With neither encounter unlocked the exit
places Bowie at Map69 (26,54) facing Down. One step Left reaches the Battle17 zone at (25,54).
In battle, arrow keys move the preview, Enter accepts the destination, F chooses Attack, H opens
Magic and Space chooses Stay. The party can return to the church during training, revive fallen
allies and retry with earned experience.

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

The shrine displays Taros in a 48-pixel field pose with portrait 42. Gradual text plays his speech
using command 112 at timer 189 on YM4. The pinned `sfx_30` header selects that slot. The initial
gradual playback stopped at line 3197 with `sfx-slots-unavailable`. The corrected adapter started
the 28,057-frame sound and continued through the warning.

## Upgraded interception on the northern return

**Confirmed** from ordinary Battle25 defeat and paid EGRESS town saves. Reviving at Hassan and
walking the direct road triggered the completed Battle17 at effective level 27. The original
selector uses F517, Bowie's effective level and each original row's movement type, in slot order.
The first band spans effective levels 27–36. It can add Orc, Bubbling Ooze, Dark Madam, High Priest
and Dark Bishop. The USA airborne exclusion overread rejects all three in-band Gargoyle rolls.
The Arrow Launcher uses the melee bounds and falls back to its base definition. See the
[upgrade predicate and tables](../../../docs/research/chapters/southern-travel-creed/battle17-initialization-and-first-control.md#f517-and-actual-repeat-definition-selection).

Both ordinary first-control rosters matched the draws from their preserved request seeds.
The English and Chinese projections resolved the selected identities and field poses. The High
Priest carried its Power Stick and HEAL3/BLAST2 book. Ordinary English play showed BLAST2 and the
High Priest's mounted HEAL3 scene. The Dark Madam carried a Power Stick and FREEZE2 and was observed
using a physical attack. The book alone grants no new AI choice. Recipe 2 stored the chosen
definitions, and a Chinese title Continue and ordinary re-save preserved the entire battle payload.

The standard Battle17 scene and field-death producer outputs passed an independent admission
and ordinary action check. All eleven selected enemy sprites resolved in the prepared world, and
the new English first-control save matched the earlier run's entire gameplay data. The new scene
was exercised by the High Priest's BLAST2 and an Orc's attack before returning to player input.

The English repeat ended in victory in round 10. Bowie's local (9,20) projected to Map69 (9,51),
with F517 set, F417 clear and 6,547 gold. Ordinary walking and Caravan travel crossed Map68 and
Map73 to Map72 and saved at North Cliff (27,34), keeping F425 and F750. The predecessor's direct
route failed at `repeat-enemy-upgrade` on `encounter.placements`; its later town detour reached
the northern route. Both paths retain their own receipts.

## Reproduction and scope

The private package uses the registered USA ROM and SF2DISASM commit `c834c652b6862bc5679fd7f69a38a7093206efc6`. The `BattleSpriteset17` source and table-driven map, background, prowess and movement selections provide the combatants and presentation. The ordinary product loaders, battle rules and Godot main scene handle the observations. Inputs, captures and generated assets stay in the prepared checkout. See the [Battle17 opening research](../../../docs/research/chapters/southern-travel-creed/battle17-initialization-and-first-control.md) for original first-control evidence.

Use the existing encounter, scene and field-pose producers with fresh ignored destinations and the
registered private environment. On the Battle17 row in the world producer's
`--encounter-inputs`, set `repeatEnemies` to the IDs in `BATTLE_REPEAT_ENEMIES[17]` and
point `battleScenes` at the prepared Battle17 scene. Run the existing command-menu and identity
mount and derive the display catalog from that world. They consume `repeatEnemies` for the
selected names and AURA identity. The handoff names the accepted private inputs and startup
commands. Its `Rebuild.ps1` assembles a fresh private package from the supplied standard field
scene, the accepted world and its existing menu, identity, sprite and audio resources.

```powershell
uv run python -m sf2tool.remake_exploration_battles --battle 17 --rom $rom --upstream $upstream --output local/road/inputs/battle17
uv run python -m sf2tool.remake_battle_scene_content --battle 17 --rom $rom --upstream $upstream --output local/road/scene
uv run python -m sf2tool.remake_battle_scene_content --field-death-base local/road/scene/battle-scenes.json --rom $rom --upstream $upstream --output local/road/field
```

The earlier isolated observations used disposable town copies. Level 26 admitted the completed base
formation. Level 27 with F517 set stopped at `repeat-enemy-upgrade` in the earlier package.
Level 27 with F517 clear admitted the first formation. These historical observations are separate
from the new native first-form check and the two ordinary upgrade runs.

The [Taros Shrine chapter](taros-caravan.md) follows the later sword-bearing party through Battle18.
At effective level 37 and above, draws can require additional unmounted definitions, including
Skeleton or Wizard. Those draws retain the `repeat-enemy-definition` boundary. Full higher-band
support, other upgraded outcomes, DETOX level two and human audiovisual acceptance remain open.
