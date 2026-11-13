# Ribble's tree, the Achilles Sword and Rohde

This chapter connects the Wooden Panel from the eastern shrine to the sword in Ribble and Rohde's proposal in Hassan. The [tree and chest contract](../../../docs/design/synthesis/chapters/southern-travel-creed/ribble-tree-opening-and-achilles-sword.md) and the [Hassan conversation contract](../../../docs/design/synthesis/chapters/southern-travel-creed/hassan-ancestry-and-rohde-caravan-choice.md#talking-to-rohde) describe the original. The continuous remake route starts at the [post-Battle15 Panel save](east-shrine-panel.md).

| Player action | Admission and consequence |
| --- | --- |
| Use the Wooden Panel in Ribble | Select the member actually carrying it, then the held slot. Bowie must stand at (15,19) or (16,19) facing Up. The scene opens the tree; after its return F731 is set and one Panel is removed from the joined inventories. With duplicates this can be another member's copy. Repeats and wrong positions/facings cost nothing. |
| Walk into the opening and search | Ribble's warp places Bowie at (42,23) on Map50. The Sword is in the separate chest at (35,6); face it and Search. A full party can make space and try again. Successful receipt sets F147 and leaves the Sword unequipped. The exit at (42,24) returns to Ribble (16,20). |
| Speak to Rohde in Hassan | His first conversation searches the joined inventories, including reserves. With no Sword, he dismisses the party and sets F798. With a Sword, he makes a proposal. Alternatively, Use the Sword at (16,58), facing any direction, before that first proposal completes. Neither entry consumes or equips it. |
| Refuse, then reconsider | No records the completed introduction (F727); the next conversation offers a shorter choice without checking the Sword again. Yes sets F725, the follower flag F84 and the later encounter unlock F418. Rohde follows; he does not join the fighting roster or grant the Caravan. |

Map50 is shared by two shrines. The Panel chest at (14,2), reached from Map61, and the Sword chest, reached from Map8, have separate claim flags. F415 can intercept **either** Map50 entry for Battle15 because its trigger is a wildcard; the battle area rectangle does not restrict that check. F515 alone does not suppress an already-selected battle. There is no direct path assumed between the two chests.

## Observed in the remake

**Confirmed:** independent English and Simplified Chinese title runs resumed the same Ancient Hall save at (27,11) Up, with eleven joined, ten living, 11,463 gold and Bowie's Panel in slot 3. The party walked out to Map61, across Map69 and into Ribble. At (15,19) Up the player used the Panel. Bowie moved back two tiles, the tree opened and the slot was freed. Walking Up entered the other Map50 region at (42,23). From (35,7) facing Up, Search returned the (35,6) Sword chest receipt for Bowie, F147 set. The exit at (42,24) returned to (16,20) in Ribble. Normal Save and fresh Continue placed the party at the town anchor (14,13) with F731, F147, the original Panel chest flag F152, and the unequipped Sword in slot 3.

From that Continue, walk back to the raft, sail toward Hassan and enter town. Talk to Rohde from (16,58) facing Down, choose No, and wait for control. Bowie ends at (16,57). Step Down and talk again; Yes sets the follower and later battle unlock. Wait for the followers to settle before saving. Both fresh Hassan Continues placed Bowie at (8,9) Up with the Sword still in his pack, 11,463 gold, the same eleven fighting members and no F11. The visible Rohde in physical slot 3 followed slot 2. The game had not awarded the Caravan, cleared Battle17 or started Taros combat. The [western road](hassan-taros-road.md) remains a separate encounter route.

In a separate earlier ordinary Hassan save with no Sword, talking to Rohde showed his dismissal and set F798 without starting the proposal. With the Sword already in the saved party, its menu entry skips that dismissal and leads to recognition. The English and Chinese chapter runs cover the talk, refusal and repeat branches; the menu-show branch has its own observation.

**Confirmed (bounded engine checks):** `RibbleItemEventTests` covers wrong tile/facing/item, duplicate Panels, repeat suppression, a full Sword chest with retry after freeing a reserve slot, no-Sword talk, selected Sword with no facing requirement, and reconsideration after the Sword is no longer held. Full-pack retry and duplicate items are controlled variations; the continuous party had room. Original hardware timing and rendered parity remain **Unknown**.

## Reproduce

In the prepared checkout, choose a fresh sample (one per language). The private driver sends ordinary keyboard input, checks the live state at each transfer, and writes only under its own run and disposable save namespace:

```powershell
./local/issue1805/native.ps1 tree en review
./local/issue1805/native.ps1 proposal en review
./local/issue1805/native.ps1 restore en review
```

Repeat with `zh-CN` for the second language. `visit zh-CN` starts from the earlier swordless Hassan save and `show en` uses the separate saved Sword branch. The [private handoff registration](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1807#issuecomment-6861556262) identifies the source of the Taros-ready slot; do not distribute it. Some native runs reported ObjectDB instances still live during shutdown. No save or gameplay success depends on treating shutdown cleanup as verified.
