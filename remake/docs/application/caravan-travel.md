# Driving the Caravan from Taros Shrine

The original grants the Caravan on the first Taros victory. On a fresh overworld, the unlocked vehicle appears as the first physical follower. Walking into a dry riverbed starts boarding. The next directional input drives the Caravan, and steering onto a passable bank restores walking. The raft has its own water terrain and saved berth. [Original vehicle controllers and landing](../../../docs/design/synthesis/chapters/southern-travel-creed/caravan-boarding-and-southwest-parmecia-travel.md)

## From the shrine road to the far bank

Use one arrow tap per tile and allow boarding and landing to finish. Both languages use the same keys. Continue the post-Taros slot at Map12 (11,8). Walk south through the shrine and leave via the south edge at (12,31). The exit places Bowie on Map68 at (55,44), facing down.

| From | Keys | Result |
| --- | --- | --- |
| Shrine road | Right 16, Up 12 | Cross into South Parmecia on foot and reach Map69 (9,32). |
| South bank | Up | Board the Caravan on the dry riverbed at (9,31). |
| In the riverbed | Left 9 | Drive west across the map boundary, arriving on Map68 at (62,31). |
| Southwest Parmecia | Left 2, Up 3, Left, Up 2, Left 2 | Follow the bends to (57,26). |
| Far bank | Down, Left | Land at (57,27) and walk to (56,27). Right then Up boards again. |

## Controller and population

The importer reads `table_OverworldMaps` and prepends the selected `table_OverworldFollowers` rows to the ordinary follower table. F65 allocates the Caravan with sprite 62 in physical slot 1. Later active rows retain their own logical lookup behavior. The runtime installs the vehicle program once per world and reuses the authored boarding and landing sequence. Its physical slot operands stay distinct from the logical name FOLLOWER_A. The population-backed party list includes the player. Boarding the Caravan applies the shrink program to followers after the vehicle. Boarding the raft applies its program to all followers.

Mode 1 travels on the working-layout marker `$3800`. The crossing to the adjoining overworld retains the riding player and hidden followers, reselects the view and updates the physical raft from its berth when appropriate. A reachable ordinary tile below `$C000` requests landing. Vehicle movement preserves the walking step counter. The raft uses mode 2 and `$3C00`. Ordinary walking after a Caravan landing reinstalls the active overworld formation. Each vehicle obtains its presentation from the private ROM-backed mapsprites.

## Saving and returning to the raft

Once the party and camera settle on the far bank, press F10, select Save and confirm the slot. Continue returns to the saved walking position and direction in Southwest Parmecia. From (56,27), press Right, Up to board again. Drive Right 2, Down 2, Right, Down 3, Right 2 and then Right 9 to return to Map69 (9,31). Down lands on the south bank. The original raft is moored on the river near Hassan. From shore at (27,55), press Right to board and Up to sail.

**Confirmed** in both English and Simplified Chinese, using normal Continue, movement, combat and Save controls. The party crossed Map69 to Map68 while riding sprite 62, landed as Bowie, saved after another walking step, and reboarded in a fresh game process. The eleven characters, names, equipment, resources, 9,174 gold and raft berth were identical in the shrine and far-bank saves. F64 and F65 remained set. Neither boarding nor driving consumed walking steps.

On the walk back to the raft, the western encounter zone triggered a repeat Battle17. The three living allies won in round 18 after 50 player actions. They returned at (17,48) with 12,694 gold, walked to (27,55), boarded the raft at its retained (28,55) berth and sailed north to (28,54) in mode 2. Separate comparisons also checked all four F64/F65 combinations and a different berth at (26,51).

## Continuing the Hassan party

The old Hassan slot held 9,923 gold and eleven members, three living. The church raised eight allies and left 8,943 gold. The same party won the desert battle, took the old man along, returned to the priest, obtained the Wooden Panel and exchanged it for the Achilles Sword. Bringing Rohde along left the same eleven fighting members. The subsequent Battle17 save had 5,586 gold and five living. After Taros, three members were alive, Bowie had the Sword equipped, and the raft was at (28,55). The far-bank Save preserved every ally's record from that shrine slot. The earlier encounters use their existing ordinary-input observations and Save handoffs. [Desert](desert-battle.md) · [Panel](east-shrine-panel.md) · [Sword and Rohde](ribble-sword-rohde.md) · [Taros](taros-caravan.md)

In the prepared checkout, use the private operator script and a new sample name for each language. The first command uses the retained normal post-Taros save. The second opens a new process on the landing save. Set `$shrineSlot` to the operator's `after-caravan.json`.

```powershell
./local/issue1808/run.ps1 travel review01 en -Seed $shrineSlot
./local/issue1808/run.ps1 restore review01 en
./local/issue1808/run.ps1 travel review01 zh-CN -Seed $shrineSlot
./local/issue1808/run.ps1 restore review01 zh-CN
```

For manual play, use the **Drive Caravan** launcher or run `./local/issue1808/run.ps1 play manual01 en -Seed $shrineSlot` in the prepared checkout. The operator runner uses a separate disposable slot for each sample and language. The private handoff retains the file list, inputs, comparison scripts, saves, and compact receipts. Some successful native exits reported four leaked ObjectDB instances.

Caravan storage, the village entrances beyond the river and Battle19 are outside this chapter. **Unknown** includes original hardware timing and a natural original-game runtime traversal of these exact tiles. [Original research and evidence boundaries](../../../docs/research/chapters/southern-travel-creed/caravan-boarding-and-southwest-parmecia-travel.md)
