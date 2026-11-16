# Elric’s passage and Fairy Cave

**Confirmed** in the private remake. In the registered `handoffs/issue1849` bundle, open `Play-en.cmd` or `Play-zh-CN.cmd` and choose **Continue → Slot 1**. Both parties stand beside the southern chest in Fairy Cave (Map32), with Elric and all four treasures. The bundle’s `HANDOFF.md` also starts a fresh replay from the original English or Chinese pond save. [Harpy Pond rescue and party histories](battle20-elric.md)

## From the pond to the rock

Leave Map51 through its southern exit to arrive on Devil’s Tail at (57,26). One Up input submits the zone at (57,25). Its working layout word is `0x1494`. The zone’s source guards require F751 clear, joined Elric (F13) and nonzero Elric HP. His active-place flag F45 is independent. The pond warp table also lists (57,25), but the tile retains its zone marker after opening. Walking there again returns control on Map67. [Original zone, warp and script](../../../docs/research/chapters/southern-travel-creed/harpy-pond-departure-through-fairy-cave-entry.md)

To inspect first, walk Right, Up, Up, Left from the pond arrival, face the rock to the north and choose **Search**. Both rescued parties saw 424 and 2590 and kept F751 clear. A separate walk from the pre-rescue English landing reached (58,23) on the other side, faced Left and saw Peter’s extra line 2591. The joined, zero-HP source branch selects his hint 2592. The two rescued saves have living Elric. From the south face, stepping Down onto the zone starts the same opening. [Rock description and original HP branches](../../../docs/research/chapters/southern-travel-creed/fairy-cave-exploration-and-devils-tail-return.md)

During 2593 and 2594, (57,23) still held `0xC01F`, a blocked rock. By 2595 the 3×2 region at (56,22) matched the source at (0,0), with `0x1008` at (57,23). F751 was still clear. The script queued Map32 (29,3), Left. The calling zone wrote F751 after the script returned. Both parties gained control in the cave with the flag set. A later fresh Map67 load and a horizontal scroll back from Map68 each reapplied those same six words. Search from (57,24) facing Up then gave 424 and 434. Another visit to (57,25) left the player on the field.

## Walk, search and return

From (29,3) walk Left onto the descending stair at (28,4). Its diagonal facing is 6. The westbound tunnel connects to the north–south passage on column 3. Follow it to row 16 and walk east for the three adjacent boxes. Column 19 leads south to the fourth. Both languages used ordinary movement and Search at each box. [Original chest rows and search precedence](../../../docs/research/chapters/southern-travel-creed/fairy-cave-exploration-and-devils-tail-return.md)

| Chest | Item and saved receipt | Recipient in both runs |
| --- | --- | --- |
| (23,15) | Angel Wing, F160 | Bowie |
| (24,15) | Brave Apple, F161 | Sarah |
| (25,15) | Medical Herb (item 0), F162 | Sarah |
| (19,26) | Mithril, F163 | Sarah |

Each immediate second search gave 403/408 and an empty chest. After cold Continue all four working words were `0xD802`, and repeating the southern search gave the same empty result. The shared item grant used the first available slot in party order. The English party kept F519 clear and 11,614 gold. The Chinese party kept F519 set, its Shining Ball and 15,704 gold. Both kept twelve active members, their stats and equipment, return town Map6, Caravan ownership and raft berth Map69 (28,55).

| Take this exit | Arrival and next ordinary input |
| --- | --- |
| West stair, (2,3) | Map67 (51,23), Down. Up at (51,22) re-enters the west side. Left onto the default zone (50,23) with F521 clear sets F421 and requests the current map. The wildcard row selects Battle21. The retained run stopped at `battle-entry-unavailable / battle-21`. |
| East stair, (29,3) | Map67 (57,24), Down. Moving into the opened (57,23) re-enters the east side. The return also supports the eastern scroll to Map68 and back with the same party. |

The sealed round trips returned to Map32 (19,25), Down. An independent title Continue and immediate re-save matched each complete gameplay payload, including both RNG words, item slots, chest and passage flags, facing, music and transport. The separate east-stair save exercises diagonal facing across a cold load. Run `python -X utf8 compare.py` in the private handoff for the bilingual comparison. [Save and Continue contract](battle-save-and-continue.md)

**Unknown:** natural original-console timing, hardware presentation and the zero-HP hint in a natural playthrough. The terrain and program joins use the registered USA ROM and pinned SF2DISASM revision identified by the original chapters. Battle21 combat and the onward trip to Creed remain beyond this playable route.
