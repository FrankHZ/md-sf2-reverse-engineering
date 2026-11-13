# Driving the Caravan from Taros Shrine

The original grants the Caravan on the first Taros victory. On a fresh overworld, the unlocked vehicle appears as the first physical follower. Walking into a dry riverbed starts boarding. The next directional input drives the Caravan, and steering onto a passable bank restores walking. The raft has its own water terrain and saved berth. [Original vehicle controllers and landing](../../../docs/design/synthesis/chapters/southern-travel-creed/caravan-boarding-and-southwest-parmecia-travel.md)

## From the shrine road to the far bank

Use one arrow tap per tile and allow boarding and landing to finish. Both languages use the same keys. From the shrine exit on Map68 at (55,44), facing down, follow these directions.

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

**Confirmed** in the remake with the private shrine-road comparison in English and Chinese. A controlled start at (55,44) with F64/F65/F67/F68 followed the route above using the normal Godot movement controls. Sprite 62 changed from a follower to the driver and back. Physical followers reappeared at the bank. In a separate controlled field save with a parked raft, a fresh Chinese Continue used ordinary inputs to board slot 31 on Map69 (26,51) and sail to (27,51). That comparison supplied the berth explicitly.

Caravan storage, the village entrances beyond the river and Battle19 are outside this chapter. **Unknown** includes original hardware timing and a natural original-game runtime traversal of these exact tiles. [Original research and evidence boundaries](../../../docs/research/chapters/southern-travel-creed/caravan-boarding-and-southwest-parmecia-travel.md)
