# Recovery consumables in battle

**Proposed for [#1948](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1948).**
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

## Proposed remake behavior

Reuse the Item origin planned in the accepted offensive-item design for custody, resolved identity,
expected word and presentation. Reuse the friendly recovery reaction and AURA ordering. Use a
finite recovery shape on the existing item definition for the three admitted effects. Running
these items through learned Magic would apply an unrelated class and book gate, payment phase
and fairy animation. Treat target geometry and ordered reactions as reusable behavior with
their own admission. Keep source-specific checks on private content joins and rule consumers.

| Responsibility | Candidate implementation owners |
| --- | --- |
| Bind real content | [PrivateBattleDefinitionReader.cs](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs), [PrivateBattleActionBindings.cs](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs), [PrivateBattleScenarioReader.cs](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs). Admit the three item/effect joins from the existing static input across encounters, including old saves with carried words. Keep the linked rank, range and sentinel checks separate from the use-origin payment. |
| Selection and recovery | [EngineBattleState.cs](../../src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs), [Sf2ItemAction.cs](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2ItemAction.cs), [HealingTargetRules.cs](../../src/Sf2.Remake.Domain/Gameplay/Sf2/HealingTargetRules.cs) and [BattleChoices.cs](../../src/Sf2.Remake.Application/Runtime/Battles/BattleChoices.cs). Use the same center predicate for offers and confirmation from the confirmed stop, then reuse the ordered source-slot AURA4 scan. Apply the source-slot and signed HP-domain checks to admitted Rain even when no Magic AURA is registered. Keep Magic-specific caster checks in their caller. Accept full values, preserve faction and target status, and reject stale words and illegal centers at commit. |
| Settlement | [BattleActionResolution.cs](../../src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs), [BattleActionCommitter.cs](../../src/Sf2.Remake.Application/Runtime/Battles/BattleActionCommitter.cs) and [Sf2BattleProgressionRule.cs](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2BattleProgressionRule.cs). Validate ordered HP or MP deltas against target vitals before publishing movement, removal and its stat refresh. Credit the admitted current class and retain the shared max-HP denominator. Rain uses one award over all recipients. Freeze the selected identity before compaction and use it throughout replay. |
| Scene | [BattleSceneContinuation.cs](../../src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs) and [BattleSceneView.cs](../../game/src/Battles/BattleSceneView.cs). Use the existing Item gesture and friendly recipient transitions, including AURA's return to the actor. Drive action text and the absence of MP payment and overlay from item origin. Read MP before/after for Tear, project a distinct MP result including zero, and use the existing ordered scene and reward continuations. |
| Close implementation | Add focused actual engine behavior tests in [Sf2.Remake.Engine.Tests](../../tests/Sf2.Remake.Engine.Tests/). Update this agreement, [gameplay rules](gameplay-rules.md), affected [item](../content/campaign-items.md), [location](../content/campaign-locations.md), [character](../content/campaign-characters.md) and [flag](../content/campaign-flags.md) entries, and the [capability trace](../capability-status.md#campaign-feature-trace) after observation. Recheck exact file ownership after the queued shared item, ring and class work lands. |

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

## Retained inputs and ordinary acceptance plan

**Confirmed (remake data inspection).** `handoffs/issue1966/content/package/world.json` binds
all four relevant pickups. Its encounter static input already contains all three item/spell joins.
The accepted `issue1941/content/catalog-final` has `name.item` entries 6, 7 and 8. English
source text 300 is present in the world. The Chinese catalog lacks row 300. Supply a scoped
localized MP-result phrase using the current adapter text path or add that one catalog row if
the existing mounted-text path requires it. The item names, sprites, gestures and world need
no new extraction.

| Supply | Retained custody and shortest candidate |
| --- | --- |
| Water | Both `issue1922/ketto/player-saves/review-{en,zh-CN}-main-gate-town01/sf2-opening/slot1.json` record F171 and Water, in English Chester's slot 3 and Chinese Jaha's slot 2 (zero-based). Both holders are reserves in the corresponding #1955 Battle26 starts. Give the Water to a deployed ally through ordinary field/Caravan inventory, or use Relief before admission. The independent Taros F155 cold slot `issue1941/player-saves/taros-en-cold-final/search/sf2-opening/slot1.json` already has Water on Bowie in slot 3, with Battle19 as an earlier alternative. [Ketto Search](../application/ketto-town.md#search-and-departure) · [Taros Search](../application/campaign-field-search.md#hassan-the-shrines-elven-village-and-harpy-pond) |
| Tear | [North Cave's ordinary round trip](../application/battle25-north-cave.md#search-and-exits) granted it to Bowie and set F168. The inherited north-mouth seeds precede collection. The English `issue1918/cave02/player-saves/review-en-main-gate-cold01/sf2-opening/slot1.json` is at Map72 (25,29), F168 clear, with Bowie's last slot empty. Continue there for a short ordinary chest-only witness. For the combined Ketto journey, free one slot on Bowie or another active ally via Give or Depot first, then retrace the north-mouth warp and face Up from (34,5). Verify the actual recipient and repeat Search before departing. |
| Rain | Map33 has the chest at (1,23), F164. In the inspected registered Creed and successor save envelopes, F164 was clear and no checked pack carried Rain. The retained `issue1857/slots/victory-en.json` is the earliest short candidate from the accepted Battle21 victory. On the first mansion entry at (15,30), try the southwestern approach to (1,24) and Search Up before entering the Goliath/door sequence. The packaged raw layout has a 20-step cardinal path to that square. Live pickup and legal custody remain **Unknown**. Make a pack opening by ordinary Give first, verify the receipt if reached, then follow the existing magic-door and Desktop route to Battle22. The nearer-looking post-scene door checkpoint (12,15) is across the closed corridor on the raw layout. Stop on any unreconciled gate and record it with the chest owner. [Mansion entry](../application/creed-mansion-entry.md) · [Desktop and Battle22](../application/desktop-battle22.md) · [Chest row](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map33/7-chest-items.asm#L4) |

1. From a disposable English Ketto copy with F171 set and F168 clear, stage both holders, make the brief cave
   pickup and return to [ordinary Battle26 admission](../application/battle26-higins.md).
   At an early player window, spend MP through an admitted offensive spell or let an ordinary
   enemy drain it. Use Tear on a depleted teammate and Water after a survivable hit. Back
   from one selected center before confirming. Verify both vital deltas and their resource
   labels, exact pack changes and first subsequent player control. A short Chinese repeat
   of the Tear seam covers the missing localized MP result. Retained slots establish the
   route inputs, and the first useful attack/cast windows are **Unknown** until play.
2. If Rain's fresh ordinary pickup succeeds, save its custody and use it in Battle22 with
   more than one injured ally, including one beyond ordinary area reach. Read the ordered
   affected list, no MP charge, one compact removal, final recovery and return to control.
   If pickup is unreachable in the bounded check, retain **Unknown** for that ordinary
   acquisition-to-use witness. A controlled engine scenario can prove execution and cannot
   satisfy the ordinary witness.
3. Exercise Field No Effect and Caravan refusal without consumption in the same journey.
   Battle Save at settled Movement, cold Continue, compare the full decoded `data` and
   accept the next normal input. For retained input checks, base64-decode `payload` and
   verify its existing `sha256`, then read `data.allies[].items` or
   `data.battle.actors[].loadout`, flags 155/164/168/171, deployment, vitals and slot order.
   Use normal state/input/projection/error observations. Record process exit failures
   independently of the saved gameplay outcome. Run the affected engine cases, adapter
   compile, clean-head plan and selected CI for the implementation slice.

Mount the measured 101,835,469-byte world and existing media read-only with the accepted
catalogs. Expect zero generated world/media bytes and at most one small MP text change.
The named field slots are about 7–15 KiB and Battle26 starts about 41 KiB. Budget a few
disposable slots and compact readbacks, at most about 1 MiB beyond incremental build/import.
Reassess only if an admitted input or a needed presentation binding is missing. No new
asset build, world export, broad replay or verification platform is required by this design.
