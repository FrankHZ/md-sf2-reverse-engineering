# Upgraded Battle19 on the Elric return

**Confirmed** for fresh normal-difficulty Battle19 entry. F519 and Bowie's effective level determine whether the initial enemy definitions can change. The source slot still supplies its actor index, position, commandset, orders, activation regions and placement item. The selected definition supplies the unit's level, HP/MP, movement, combat stats, resistance, prowess, spell and equipment loadout, gold, name, body/palette and map sprite. The reward and drop rules read those ordinary unit and placement values. [Original selection, retry and retail overread](../../../docs/research/chapters/southern-travel-creed/battle19-initialization-and-first-control.md#f519-and-level-dependent-repeat-selection)

At effective level 29–38 the normal package admits all five additional definitions: Bubbling Ooze (16), Skeleton (17), Wizard (72), Dark Bishop (81) and Master Monk (82). Entry uses the original mover for each of the fifteen slots and threads the post-retry MAIN word into the next slot and then battle initialization. Gargoyle and Arrow Launcher each consume two draws in this band and retain their original identities. Clear F519 or effective level below 29 uses the original roster without selection draws. The signed class check adds 20 for promoted classes. The quotient continues to increase every ten levels. An actually selected definition outside the admitted content reports `repeat-enemy-definition` with its slot and ID.

The private world producer accepts the selected IDs through `repeatEnemies` on the Battle19 encounter input. Prepare its scene, field sprites and command-menu icons before deriving the display catalog. Runtime content binds each candidate to the same placement and resolves the selected appearance by species. Wizard BLAZE3, Dark Bishop AURA1/BLAST3, Master Monk HEAL4, Bubbling Ooze AQUA2 and their lower ranks use their private definitions. HEAL4's AI rank selection uses the retail five-bit shift and unmasked spell entry. A selected rank can cost more MP than the packed key tested by the AI. Execution clamps the actual deduction at zero. AURA1 heals the caster's faction in source slot order. Its private scene uses the selected body and healing fairy. Original native timing and audiovisual parity remain **Unknown**.

A suspended battle with changed definitions saves the ordered IDs in battle recipe 2. Continue binds these to the original slots before restoring HP/MP, equipment, statuses, turn queue, cursor and both RNG words. Continue never performs fresh selection. Recipe 1 restores the original definitions, including historical Battle19 suspends. The Battle14 and Battle17 routes retain effective thresholds 24 and 27 and their explicit upgrade-content boundary.

## Player and comparison inputs

The registered private bundle `handoffs/issue1852` owns both player launchers, the trained field and battle slots, observation receipts and the reproduction commands in `HANDOFF.md`. Continue in Slot 1 opens the upgraded battle. The [earlier Battle19 return](battle20-elric.md) and the Fairy Cave handoff retain their original files.

### Natural growth and player branch

**Confirmed** from the sealed Chinese post-Elric repeat suspend. Both Fairy Cave parties had Bowie at level 24. The Chinese party already had F519 and 14 EXP; the English party had 6 EXP and had yet to win Battle19. The available Brave Apple was a pickup with no admitted field-use effect, and these saves had no implemented promotion service. The Chinese Movement slot therefore let the party start earning immediately. The driver used ordinary movement, attack, healing, victory, village return and F10 inputs. Across 84 completed battles Bowie reached level 29 with 3 EXP. This count describes that party and strategy.

On the next natural entry the ROM comparison matched all fifteen saved IDs. Selection took 23 MAIN draws and produced two Dark Bishops, a Wizard and a Skeleton. Sarah received the first Movement. English cold Continue confirmed her destination, reached ActionChoice, cancelled and re-saved the identical gameplay payload, including both RNG words. From that same battle save the Chinese victory branch returned to Map68, accepted a step to (26,38) and saved with 354,144 gold. The English defeat branch returned to New Granseal (11,22) with 174,872 gold from 349,744, then accepted a step to (12,22). These branches are ordinary inputs from one naturally reached slot.

### Controlled comparisons

**Confirmed** with disposable field copies. At E29 with F519 clear and at E28 with F519 set the first player input retained the original fifteen definitions. An old recipe-1 Movement slot cold-restored independently. At E39 one draw requested enemy 20 for slot 128 and entry reported that missing definition. The private route reader also confirmed that Battle14/17 have zero slots bound to repeat content.

For a controlled E29 entry with all five additional definitions, the MAIN image immediately before the requesting zone step was `0x0004BEEF`. The request consumed one draw. Reading the six original definitions' category bytes and exclusion windows from the pinned ROM predicted these fifteen IDs in placement order:

```text
12 82 16 43 81 12 72 11 43 17 60 16 11 60 43
```

Selection consumed 26 draws and left MAIN at `0x566DBEEF`. The first saved player input contained those exact IDs and MAIN `0x2C97BEEF` after ordinary initialization and automatic play. Another seed selected two Dark Bishops and kept the base mage. A separate controlled fight reached a Master Monk with 9 MP and 28 HP missing. It selected HEAL3 on itself and continued through victory. The package's `derivation/rom-compare.py` reads the retail bytes and compares them to the suspended roster. The engine tests also cover other quotients, slot order and both RNG halfwords.

### Enemy AURA presentation handoff

**Confirmed in the remake:** the [bounded enemy AURA observer](../godot/spell-presentation.md#enemy-aura-recipient-handoff-2317)
uses a disposable copy of the published five-species battle, the complete KIWI art package and
ordinary Stay inputs. Existing Dark Bishop AURA1 heals Bubbling Ooze, itself and Gargoyle in
source-slot order for18/0/18 HP, pays7 MP once, restores the caster and returns usable player
control. Actual body/palette, textures, Chinese results and evil fairy placement pass in Original
and HD/Reduced Flash. The shared cursor correction preserves later recipients' source metadata
and their own idle work; direct varied-content engine cases own that timing assertion.
No enemy policy, rank, source selection or real campaign slot changes. The comparison seed and
launch command are documented at that owner. Natural original casting and hardware timing remain
**Unknown**; the earlier Master Monk HEAL3 witness remains separate.
