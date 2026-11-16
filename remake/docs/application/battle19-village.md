# Elven Village and the first Battle19 return

**Confirmed** in the private campaign with ordinary Godot input. The accepted post-Taros shrine and Southwest Parmecia landing slots both open in English and Simplified Chinese. The shrine route crosses the river and reaches the same walking fork as the landing slot. The landing witness has eleven joined and active members, three living, 9,174 gold, F64/F65, a Map6 return town and a raft at Map69 (28,55). F419/F469/F519 start clear. [Caravan controls and shrine route](caravan-travel.md)

## From the landing

From (56,27), walk Left three, Down three, Left four. Allow the Caravan to board and land as the party crosses the dry riverbed. At Map68 (49,30) choose a direction.

| Input | Observed result |
| --- | --- |
| Left | Enter Map23 at (22,29) facing Right. The default setup deploys six village residents alongside the field party and selects Elven Town music. Walk Down through the south exit to Map68 (48,31) facing Down. Walking, the raft berth and clear F419/F469/F519 survive the round trip. |
| Up, Left twice | The marked cell at (47,29) sets F419 and admits Battle19. First start sets F469. Content supplies twelve placement templates, fifteen enemy rows, three regions and one point. The inherited party produces eleven ally records with Bowie, Sarah and Peter on the board. Actual equipment, current HP/MP and the learned spell books feed the existing battle rules. |

The private selection imports the village's default setup, warps, music and the encounter. The High Priest's healing command tries wounded allies in priority order. Its local spell level can change between targets. The chosen HEAL level supplies the actual MP cost and casting range. BLAST and FREEZE use their affordable learned levels. A behavior test exercises HEAL3 at distance three, alongside checks for the source's self-target, wound and MP thresholds. [Original selection and initialization](../../../docs/research/chapters/southern-travel-creed/battle19-initialization-and-first-control.md) · [Engine behavior checks](../../tests/Sf2.Remake.Engine.Tests/EnemySpellDecisionTests.cs)

## Victory and the next slot

Defeat the leader Orc in source slot 128 while Bowie lives. The leader outcome clears the surviving opposition. In both runs Peter first killed the High Priest in slot 129, received its held Shining Ball (item 16) into an empty slot and kept it through Continue. The drop requires a qualifying lethal action, the matching held item and clear item-custody bit 6. It makes no rarity roll. A full pack routes this rare item to Deals. Leader cleanup alone gives no receipt. Full-pack and cleanup cases have engine coverage. Using the Ball as an attack item remains outside this chapter. [Drop and cleanup behavior](../../tests/Sf2.Remake.Engine.Tests/EnemyItemDropSessionTests.cs) · [Full-pack and custody rules](../../tests/Sf2.Remake.Engine.Tests/EnemyItemDropTests.cs)

The first active member's final position and facing determine the return. The field adds (5,19) before rejoining the force. F419 clears, F469 stays set and F519 is set. The English and Chinese input sequences produced different positions.

| Run | Final Bowie position | First walking Map68 position | Save after one Right step |
| --- | --- | --- | --- |
| English | (16,14) Down | (21,33) Down | (22,33) Right |
| Simplified Chinese | (19,17) Down | (24,36) Down | (25,36) Right |

Use F10 → Save on the settled tile. Both saves hold 13,264 gold. A new process loaded each through title-screen Continue and walked Right once. The next serialized save changed only X and the step counter (9,999 to 9,998). This comparison includes names, members, levels, HP/MP, packs, equipment, both RNG words, flags, item custody, gold, berth and return town. The slots are the playable handoff for the next chapter.

## Other outcomes and equipment

- **Defeat.** Bowie's defeat returned the party to Map6 (11,22) facing Up with 4,587 gold from 9,174.
- **EGRESS.** Bowie's cast spent eight MP (15 to 7 at payment) and preserved 9,174 gold. The return refreshed his MP to 15.
- **Retry.** Both routes reset the raft to Map69 (42,15), retained F419/F469 and left F519 clear. Walk through Maps73, 72 and 69. The scroll into Map68 retains the field. Enter Taros Shrine, then leave it. Fresh Map68 admission starts Battle19 before another marked step. Both runs reached player Movement again.
- **Equipment.** From the old landing, use the field party's Equip menu to select Unarmed and save. Continue that copy and enter Battle19. Bowie's third pack slot still holds the unequipped Achilles Sword. The ordinary system menu saved this state at the first Movement input. On a separate run the battle Equip menu restored the sword in that slot and the party won. Another run kept Bowie unarmed and lost.

[Original outcome and return selectors](../../../docs/research/chapters/southern-travel-creed/battle19-outcomes-through-elven-village-entry.md) · [Map23 departure](../../../docs/research/chapters/southern-travel-creed/elven-village-departure-through-harpy-pond-entry.md)

## Reproduce

The registered private handoff `handoffs/issue1834` includes the runtime package, bilingual catalogs, both old starting profiles, both victory saves, the unarmed field and battle slots, input drivers and captured state. Follow its `HANDOFF.md` from a prepared isolated checkout. Use a fresh sample name and disposable save namespace for each seeded run. Modes `village`, `win`, `escape`, `loss` and `restore` send input to the game and read its live observations. Compare serialized slots after a cold Continue with the included comparison. The two additional private audio cues use bounded original-driver captures. Their selection checks do not establish original hardware timing.

A [post-rescue repeat with Elric](battle20-elric.md#return-to-battle19-with-elric) reaches Movement and cold Continue with F519 set. **Unknown** for ordinary play includes other return towns, village services and the later F762 setup. The [Harpy Pond chapter](harpy-pond-approach.md) continues from these saves. [Campaign scope and following stages](remaining-campaign-risk-baseline.md#chapter-handoffs)
