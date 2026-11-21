# Recovery consumables in battle

**Implemented for [#1948](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1948).**
Standard rigor applies because the three effects cross battle selection, recovery, inventory,
presentation and Continue. The target is ordinary player Item use through Battle26 and first
Pacalon. Use the existing recovery and item-action owners. Later campaign content, automatic item
selection and wider Magic admission have separate owners.
[L2 healing and support](../../../docs/design/synthesis/themes/tactical-battle-loop.md#magic-healing-and-support) ·
[L2 item custody and use](../../../docs/design/synthesis/themes/tactical-battle-loop.md#items-custody-equipment-and-use-are-separate) ·
[Healing Drop delivery](gameplay-rules.md#finite-purchased-supplies-and-detox1) ·
[Offensive item design](offensive-items-design.md)

## Original rules

**Confirmed (static).** The following rows and callers are from the USA retail baseline and
`ShiningForceCentral/SF2DISASM` at `c834c652b6862bc5679fd7f69a38a7093206efc6`. Reproduce the
operands with `git -C <pinned-checkout> show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/<path>`
for the linked files. The controlled HEAL and AURA observations corroborate their effect routines and
AURA scan. The complete original ordinary Item actions and cold Suspend/Continue remain **Unknown**.
[HEAL boundary matrix](../../../docs/design/contracts/battle/spell-resolution.md#confirmed-heal-1-subset) ·
[AURA matrix](../../../docs/design/contracts/battle/spell-resolution.md#confirmed-aura-target-and-accumulation-subset)

| Item | Item type and use row | Center and recipients | Resolved recovery |
| --- | --- | --- | --- |
| Healing Water 6 | Consumable, HEAL4, range 0–1, radius 0 | User or cardinally adjacent living teammate | All missing HP. Power 255 is a sentinel, including when missing HP exceeds 255. |
| Fairy Tear 7 | Consumable, G_TEAR2, range 0–1, radius 0 | User or cardinally adjacent living teammate | `min(20, maxMP - currentMP)` MP. HP and status stay as they were during the reaction. |
| Healing Rain 8 | Rare consumable, AURA4, range 0–0, radius 0 | User is the only center. AURA4 then scans the center's whole source side. | All missing HP independently for each eligible member, using the same sentinel as Water. |

All three are unequipped consumables with no class restriction. HEAL4 and AURA4 list MP cost 20
and the silence property in their spell rows. G_TEAR2 lists cost 0. The action remains
`USE_ITEM` when it dispatches the linked effect. The MP charge and Silence check belong to
`CAST_SPELL`, so a directly controlled user needs neither MP nor the spell in their book.
Source SLEEP and STUN skip normal input. MUDDLE and the AI-control bit route to automatic
control. Scene inaction rolls for curse and stun occur on the physical-attack branch.
[Item rows](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/items/itemdefs.asm#L83-L111) ·
[HEAL4 row](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spelldefs.asm#L50-L56) ·
[AURA4 row](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spelldefs.asm#L82-L88) ·
[G_TEAR2 row](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spelldefs.asm#L626-L632) ·
[Turn control](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/executeindividualturn.asm#L51-L124) ·
[Scene admission](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/initbattlesceneproperties.asm#L94-L144) ·
[Payment](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/createbattlesceneanimation.asm#L36-L57)

- **Selection and expansion.** The player menu stages the actual pack word and slot and uses the
  linked spell grid from the chosen stop. A center must be alive, placed in bounds and clear of
  the neutral activation bit. The combatant side bit selects allied or enemy slots, independently
  of who controls a unit. Water and Tear expand radius zero through the teammate grid. AURA4
  takes the separate scan of source ally slots 0–29 for a friendly center, with a corresponding
  enemy-bank branch. The scan visits ascending slot order with HP above zero and X different
  from the unplaced sentinel. Distance, the neutral bit and control ownership do not filter
  that scan. A living full-HP member still receives a zero HP reaction. A full-MP recipient,
  including one with max MP zero, can receive a zero MP reaction. Confirming a zero-recovery use
  consumes the item and proceeds through reward and turn handling. Back and absent centers
  return before action construction. There is no resurrection, status cure or MP restoration
  in the two HP reactions. The separate automatic Rain choice has its own source conditions.
  [Player selection](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/battlefunctions_2.asm#L369-L430) ·
  [Range grid](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefield/buildactionrangegrids.asm#L68-L154) ·
  [Center occupancy](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefield/buildtargetsarray.asm#L66-L96) ·
  [Item and AURA4 target expansion](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefield/populatetargetslist.asm#L12-L194) ·
  [Automatic healing choice](../../../docs/design/contracts/battle/battle-ai-decision.md#healing)
- **Construction, custody and replay.** `UseItem` dispatches the aliased effect for each
  target. `spellEffect_Heal` copies missing HP directly for 255, bypassing promoted power
  scaling. `spellEffect_FairyTear` writes only an MP delta and the MP result message 300.
  Each reaction preserves the recipient's status. Construction walks all recipients and builds
  their results, then makes the user idle and removes the chosen word once. Removal shifts the
  remaining words left, appends empty and runs `UpdateCombatantStats` on the user even for a
  bare item. That refresh can apply current timed status operands. The scene end builds the award
  after removal. The item is already gone when scene playback applies the ordered reactions
  and then the award. Recovery does not kill targets or generate drops. There is no
  resistance adjustment, effect variance or equipment-break draw on these three paths.
  [Item dispatch](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/useitem.asm#L8-L20) ·
  [HP reaction](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/castspell.asm#L74-L114) ·
  [MP reaction](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/castspell.asm#L873-L909) ·
  [Target loop and removal order](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/battleactionsengine_1.asm#L108-L131) ·
  [Consumable branch](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/breakuseditem.asm#L8-L53) ·
  [Compaction and refresh](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/itemstats.asm#L482-L527) ·
  [Scene end and award](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/battleactionsengine_2.asm#L32-L68)
- **EXP.** For a living allied PRST, VICR or MMNK user, add
  `max(10, floor(25 * recovered / recipientMaxHP))` per reaction to one accumulator
  capped at 25. A zero max HP skips the contribution. Tear passes recovered **MP** as the
  numerator operand and the shared routine still reads **max HP** as the denominator.
  Recovery of zero still contributes the minimum when the class and max HP qualify.
  Other allied classes start the final award at zero. Same-side awards skip the battle
  halving table and take exactly two range-16 award draws, adding one on the first zero
  and subtracting one on the second zero, with final minimum one. Later growth and turn
  advancement have their existing draws. No award/gold or break logic from attack damage is needed.
  [Healer whitelist and formula](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/earnexp.asm#L8-L42) ·
  [Cumulative cap](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/earnexp.asm#L134-L142) ·
  [Same-side award and draws](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/giveexpandgold.asm#L32-L75)
- **Presentation and field.** The original builds the item-name action message and item-use gesture,
  with the default no-spell overlay for non-equipment. The result message names the actual
  recipient and stat. The field Use table omits all three IDs and the source item-event tables
  supply no matching alternate use. Field tries the event lookup then shows no effect. Caravan
  refuses before target selection or removal. Ordinary Give, storage and receipt custody remain
  available through their existing rules.
  [Item message](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/createbattlescenemessage.asm#L85-L92) ·
  [Use pose](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/createbattlesceneanimation.asm#L127-L135) ·
  [Overlay default](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/getspellanimation.asm#L36-L43) ·
  [Equipment overlay gate](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/getspellanimation.asm#L134-L153) ·
  [Field allowlist](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/items/usableoutsidebattleitems.asm) ·
  [Field caller](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/main/mainactions.asm#L242-L271) ·
  [Caravan caller](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/caravan/caravanactions_1.asm#L723-L758)

## Remake behavior

The Item origin from the offensive-item implementation supplies custody, resolved identity,
expected word and presentation. Reuse the friendly recovery reaction and AURA ordering. Use a
finite recovery shape on the existing item definition for the three admitted effects. Running
these items through learned Magic would apply an unrelated class and book gate, payment phase
and fairy animation. Treat target geometry and ordered reactions as reusable behavior with
their own admission. Keep source-specific checks on private content joins and rule consumers.

| Responsibility | Implementation owners |
| --- | --- |
| Bind real content | [PrivateBattleDefinitionReader.cs](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs), [PrivateBattleActionBindings.cs](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs), [PrivateBattleScenarioReader.cs](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs). Admit the three item/effect joins from the existing static input across encounters, including old saves with carried words. Keep the linked rank, range and sentinel checks separate from the use-origin payment. |
| Selection and recovery | [EngineBattleState.cs](../../src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs), [Sf2ItemAction.cs](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2ItemAction.cs), [HealingTargetRules.cs](../../src/Sf2.Remake.Domain/Gameplay/Sf2/HealingTargetRules.cs) and [BattleChoices.cs](../../src/Sf2.Remake.Application/Runtime/Battles/BattleChoices.cs). Use the same center predicate for offers and confirmation from the confirmed stop, then reuse the ordered source-slot AURA4 scan. Apply the source-slot and signed HP-domain checks to admitted Rain even when no Magic AURA is registered. Keep Magic-specific caster checks in their caller. Accept full values, preserve faction and target status, and reject stale words and illegal centers at commit. |
| Settlement | [BattleActionResolution.cs](../../src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs), [BattleActionCommitter.cs](../../src/Sf2.Remake.Application/Runtime/Battles/BattleActionCommitter.cs) and [Sf2BattleProgressionRule.cs](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2BattleProgressionRule.cs). Validate ordered HP or MP deltas against target vitals before publishing movement, removal and its stat refresh. Credit the admitted current class and retain the shared max-HP denominator. Rain uses one award over all recipients. Freeze the selected identity before compaction and use it throughout replay. |
| Scene | [BattleSceneContinuation.cs](../../src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs) and [BattleSceneView.cs](../../game/src/Battles/BattleSceneView.cs). Use the existing Item gesture and friendly recipient transitions, including AURA's return to the actor. Drive action text and the absence of MP payment and overlay from item origin. Read MP before/after for Tear, project a distinct MP result including zero, and use the existing ordered scene and reward continuations. |
| Field and verification | [PrivateFieldPreparation.cs](../../src/Sf2.Remake.Content/Scenarios/PrivateFieldPreparation.cs) binds the Field No Effect choices. The existing Caravan allowlist refuses these items before target selection. [RecoveryItemSessionTests.cs](../../tests/Sf2.Remake.Engine.Tests/RecoveryItemSessionTests.cs) exercises the actual selection, scene, turn and service rules. |

The shared healing center predicate excludes neutral actors, including single-target HEAL and
DETOX. Rain's later source-side scan still includes neutral and automatic recipients.
Same-side status EXP also bypasses the marked-battle halving table. The higher-rank
[DETOX implementation](detox-ranks-design.md) consumes these two rules.

Save after scene and turn settlement at normal player Movement. Existing battle v2, promoted v4
and timed-state v5 already carry packs, current HP/MP, rewards, custody, seeds and current turn.
Exercise v5 with timed bits and divergent last-refresh operands together with promoted class terms,
and v5 with base classes. Keep the conditional format selection, earlier field v1/v3 reads,
field optional scene/position data, primary/backup checks and detached restore. Cold Continue
must preserve shifted equipment and class terms, the actual refreshed actor, untouched recipients,
and acquisition flags even after consumption. Item-origin and the in-flight reaction list belong
to the scene lifetime. There is no new persistent field.
[Settled Save](../../src/Sf2.Remake.Application/Runtime/Persistence/BattleSave.cs) ·
[Battle wire](../../src/Sf2.Remake.Content/Persistence/BattleSaveWire.cs) ·
[Existing v5 agreement](support-spell-status-design.md#save-compatibility-and-recovery)

### Engine acceptance

- Use ordinary Item command dispatch for Water with more than 255 missing HP, self and orthogonal
  range from the moved stop, and an eligible full-HP recipient. Check out-of-range, diagonal,
  dead, neutral-center, enemy, absent and stale-slot rejection, and Back before commitment.
  Check an admitted zero-MP silenced user with no book, and that skipped or automatic actors
  get no new player action. Verify a duplicate word in another slot and equipped words shifted
  intact, with one stat refresh and no accidental status clear.
- For Tear, check 20 MP when more is missing, a capped gain and zero including max MP zero.
  HP and recipient status stay fixed. A healer restoring 20 MP to a 30-max-HP/60-max-MP target
  contributes 16 EXP before award variance. Check PRST, VICR and MMNK through source class
  resolution and an ordinary-class zero accumulator, followed by the same two award draws.
- For Rain, choose self at the confirmed stop. Mix distant, full, neutral and automatically
  controlled living allies with a fallen ally, a reserve and a living enemy. Resolve only the
  living placed source allies in source slot order, with one consumption and one capped award.
  Include a legal all-full use and a battle scene that returns to ordinary control. Compare
  settled Save/Continue and a next ordinary command under the existing format combinations.
  Preserve the existing Herb/Drop, poison cure, Magic AURA and field/Caravan behavior.

## Ordinary input observations

**Confirmed in the remake.** Both journeys use the mounted
`handoffs/issue1966/content/package` with the `issue1941/content/catalog-final` catalogs.
The English Ketto settled town slot held F171 and Chester's Water in slot 3. Field Use showed
“But nothing happened” and preserved the party. Ordinary Give moved it to active Gerhalt.
The party left Ketto, returned through the north mouth, faced Up at Map48 (34,5) and opened
the F168 chest. Chester received Tear and repeat Search was empty. Ordinary Give transferred
it to Gerhalt beside Water. The party entered Battle26 from the actual Ketto road and saved
at player Movement. [Ketto Search](../application/ketto-town.md#search-and-departure) ·
[North mouth and Tear](../application/battle25-north-cave.md#search-and-exits) ·
[Battle26 admission](../application/battle26-higins.md)

The same Battle26 slot continued in Simplified Chinese. Sarah's ordinary HEAL spent 3 MP.
Gerhalt used Tear from slot 1 to restore her MP from 56 to 59, preserving 58 HP and leaving
Water in slot 0. He later used Water on Sarah after a hit. Back from target selection
preserved the pack, gameplay state and both RNG words. Confirm restored 42 to 58 HP, kept
49 MP and compacted the pack to four empty words. The result messages named Sarah and the
correct resource. Both actions advanced to the next player. After battle Save, Bowie paid
for EGRESS and the party saved back at Ketto.

**Confirmed in the remake.** From `issue1857/slots/victory-en.json` at the Devil's Tail
mansion door, ordinary entry reached Map33 (15,30). The southwest route reaches (1,24)
before Goliath. Search Up sets F164, gives Rain to Chester even though he is fallen, and
returns empty on repeat. A save at the chest retained custody. On Continue, Field Use kept
Rain, then Give moved it to Kazin's slot 1. The party completed the existing Goliath and
magic-door scenes, talked to the Desktop king, and reached Battle22 at the ordinary y16
zone. No recruitment or mainline advance beyond that battle was required.
[Mansion entry](../application/creed-mansion-entry.md) · [Desktop entry](../application/desktop-battle22.md)

Kazin used Rain on himself after two allies took damage. Battle22 presented results for
source ally slots 0, 3, 4, 7, 8, 9 and 13, with HP deltas 8, 0, 17, 0, 0, 0 and 0.
Fallen and undeployed allies were omitted. One consumption removed slot 1 and retained
Kazin's equipped rod. The source-slot sequence, item gesture, result messages, overlay
selector 0 and subsequent player control were observed without a Magic payment phase.
The action kept Kazin at 48 MP.

### Reproduction and cold saves

Use a registered configuration copied into this checkout and the private driver under
`LocalApplicationData/Sf2RecoveryReview/issue1948/driver`. Copy its four files into
`local/issue1948` before launching from the worktree root. Use a fresh sample name per
invocation. The driver sends ordinary keyboard commands and reads input, state, projection
and errors. [Machine configuration](../../../docs/operations/local-private-inputs.md#fresh-worktree-setup)

| Mode | Source and settled result |
| --- | --- |
| `ketto -Language en` | Default input is the retained `issue1922/ketto/player-saves/review-en-main-gate-town01/sf2-opening/slot1.json`. Gives Water, opens F168, gives Tear and writes the Battle26 Movement slot. The observed journey's slot is in `ketto-cave-01/recovery-review/sf2-opening/slot1.json` under the review directory. |
| `creed -Language en` | Default input is the retained Battle21 victory slot. The observation used `creed` through the chest and `creed-carry` from that save to check Field Use and Give before Battle22. The latter's ready slot is retained under `creed-carry-05/recovery-review/sf2-opening/slot1.json`. |
| `use -Language zh-CN -Items '6,7' -Seed <Battle26-slot>` | Reads receipt and recovery for both items, saves after consumption, then saves once more after ordinary EGRESS to Ketto. |
| `use -Language en -Items 8 -Seed <Battle22-slot>` | Reads all seven Rain responses and saves back at battle Movement. |
| `battle-continue` / `continue` | Takes the relevant settled battle / field slot with `-Seed`. Writes `immediate-resave-slot.json` and observes the next ordinary input. |

For the cleanup check, move the entire producing `local/` aside and recreate only the
registered configuration and driver setup. The accepted input package and built adapter
are sufficient. Reproduce slot equality by verifying the existing envelope `sha256`,
base64-decoding `payload` and comparing the entire `data` object. The two consumed-item
battle slots wrote v2 and the Ketto return wrote field v3. Both battle slots re-saved
identically and accepted the next action menu. Field re-save retained F168/F171, empty
Gerhalt slots and full data equality, then accepted walking. Rain re-save retained F164.
The separate `issue1955/receipts/en-road01/battle-ready-slot.json` v4 and
`issue1966/player-saves/zh-CN-gate08/support-spell-actions/sf2-opening/slot1.json` v5
both retained all data, including the v5 last-refresh operand, and accepted the next
battle input. All five cold processes exited 0.

The actual engine cases cover missing HP above 255, MP capped at 20 and zero, PRST/VICR/MMNK
EXP, moved stops, invalid centers, compaction, Rain including a neutral and an automatic
ally, and the same-side cure EXP rule. Field No Effect and the Caravan refusal were
exercised through actual field and service commands for all three IDs. The separate
native Caravan menu witness and original ordinary actions remain **Unknown**.
[Recovery behavior cases](../../tests/Sf2.Remake.Engine.Tests/RecoveryItemSessionTests.cs)

The world and media were reused as mounted files. The top-level trial scripts, saves
and receipts total about 10.8 MiB, above the 1 MiB estimate because the inherited
driver records story-text waits and unsuccessful rehearsals. Completed failures include
the field-menu reopen assumption, the duplicate save observer after the Battle26 ready
slot had already been written, and the Water cancel observer comparing presentation
fields. Two GDScript parse errors preceded their corrected attempts. Inputs and failure
receipts remain separate from the successful saves. Some successful native processes
reported ObjectDB warnings at exit. Their exit codes and functional outcomes are recorded separately.
