# From Southwest Parmecia to Harpy Pond

**Confirmed** in the private remake. Both the pre-Battle19 landing slot and the first-victory slot can reach Harpy Pond on foot. At the title, choose **Continue → Slot 1** in the private `issue1845` player package. The party starts on Map51 at (10,9), facing Left. Right moves along the bank. Left steps onto the confrontation zone. The English slot carries the pre-victory party and 9,174 gold. The Chinese slot carries the first-victory party, its Shining Ball and 13,264 gold. [Previous saves](battle19-village.md)

## Two ways into the pond in source

From the western edge of Map68, the walk onto X 0 scrolls to Map67 at X 62 with the same Y. The two runs crossed at Y 30 and 32. Stepping east across the edge returned them to Map68 X 1. Both crossings preserved walking mode, F64/F65, active and joined membership, the follower list (Caravan, Peter, Oddler) and the raft at Map69 (28,55). Neither party entered the village on the route from its supplied slot. The landing party avoided the Battle19 zone and kept F419/F469/F519 clear. [Original scroll and controller](../../../docs/research/chapters/southern-travel-creed/elven-village-departure-through-harpy-pond-entry.md)

| Map67 producer | Source condition and ordinary result |
| --- | --- |
| Walking zone at (56,26), (57,26), (58,24) or (58,25) | A $1400 marker dispatches the zone. With F520 clear both parties triggered (57,26), heard Oddler's call for help (2543–2549) and entered Map51 at (11,22), Up. F520 set skips that scene. |
| Direct warp table row at (57,25) | A warp dispatch would request Map51 (10,24), Up. The imported layout at (57,25) has marker $1400. Walking selects a zone there. The F751 block copy covers Y 22–23. The existing $1000 warp row is retained for its own producer. Neither old slot provides a walking route through that row. |

On fresh pond admission, F420 would select Battle20 before map initialization. With F420/F470/F520 clear, both parties saw the five-line pond prompt (2550–2554). Peter and Oddler followed Bowie again. Elric occupied (6,6) and turned through four facings while the three monster actors stayed at (63,63). Elric was outside the force. The map's Main Theme reused the existing audio resource. The F520 alternate setup omits those four fixed actors. F470 independently skips the prompt. These latter branches have static coverage. [Original population and prompt](../../../docs/research/chapters/southern-travel-creed/elven-village-departure-through-harpy-pond-entry.md#fresh-pond-population-and-shared-initial-prompt)

## Warning, save and confrontation

Both passable southern exits lead through a warning tile at Y 24. At X 9 and X 10 Peter warned the party (2565), Bowie stepped back to Y 23, and ordinary movement resumed in both languages. The source exit at Y 25 would return to Map67 (57,26), Down once the party can pass the warning. The chapter saves precede that return. [Warning and confrontation](../../../docs/research/chapters/southern-travel-creed/harpy-pond-approach-through-battle20-entry.md) · [Later departure and passage](../../../docs/research/chapters/southern-travel-creed/harpy-pond-departure-through-fairy-cave-entry.md)

On settled plain ground before the confrontation, **F10 → Save** keeps the player's actual tile and facing. Cold Continue restores the default population with the followers at the leader, resumes Elric's ambient action and leaves the entry dialogue completed for this visit. Both fresh processes loaded the bank slot, walked Right and saved again. Only X and facing changed in the serialized gameplay payload. Membership, loadouts, HP/MP, names, gold, item custody, story flags, both RNG words, return town and raft berth survived. [Save selection and reconstruction](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OpeningRestore.cs)

On a separate copy, Left from the bank triggered the ten confrontation lines (2555–2564). The archived issue1845 build set F420/F470 and stopped at `battle-entry-unavailable / battle-20`. F13/F520/F751 remained clear. Continue the same two saves in the [Battle20 and Elric package](battle20-elric.md). **Unknown** includes natural original-console traversal, post-victory pond re-entry and playable onward travel to Fairy Cave. [Battle20 outcomes and recruitment](../../../docs/research/chapters/southern-travel-creed/battle20-outcomes-and-elric-recruitment.md)

## Reproduce

The registered private `handoffs/issue1845` contains the player launchers, world, bilingual catalogs, ordinary slots and a snapshot of the compiled Godot project. Run `Play-en.cmd` or `Play-zh-CN.cmd` on the prepared host. Each language uses its own save namespace. Follow the private `HANDOFF.md` to repeat the old-save approaches, cold steps and boundary observation or regenerate the package. The comparison reads compact state observations and complete ordinary save records. [Campaign scope](remaining-campaign-risk-baseline.md#chapter-handoffs)
