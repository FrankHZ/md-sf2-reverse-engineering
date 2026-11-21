# Permanent growth items

This design covers the seven permanent growth consumables in Field and Caravan Item → Use through
the current Pacalon and Battle26 campaign boundary. A joined member spends a specific carried item
on a selected joined member, and the result travels with that member into later play and Continue.
Use the [L2 progression and equipment agreement](../../../docs/design/synthesis/themes/progression-and-economy.md#party-development)
and the [L3 level-up contract](../../../docs/design/contracts/party/level-up.md#confirmed-original-sequence)
for earned bases, current stats and learned spell ranks. This document supplies the item callers,
payment, mutation and save decisions for [#1943](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1943).
Battle Item Use has its own spell/target/payment path. The scope also excludes Field Magic,
new supplies or chapters, and wider class and spell admission.

High rigor applies because the same spend changes carried resources, battle stats and two save
paths. The acceptance below checks the transaction and its next consumers through existing slots.

## Original rules and evidence

**Confirmed** for the pinned USA static paths below and for the individual runtime seams named in
the level-up contract. Read the assembled `code/common/menus/item/fielditemeffects.asm` from
`ShiningForceCentral/SF2DISASM` at `c834c652b6862bc5679fd7f69a38a7093206efc6`. The H2 owner
records the ROM identity, fixture, layout and two direct call sites. The `itemdefs` rows mark IDs
9–15 consumable with no equipment effects and give them separate battle spell operands. Field
admission and dispatch come from `IsItemUsableOnField` and `UseItemOnField`.
[USA manifest](../../../manifests/roms/sf2-us.json) ·
[Field effects fixture and ROM anchors](../../../docs/research/world/field-item-effects.md#confirmed-static-contract) ·
[Item rows](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/items/itemdefs.asm#L113-L181) ·
[Canonical effects](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/item/fielditemeffects.asm)

### Selection, payment and return

- Field builds the joined-force list. Caravan requests all force members. Both select a holder
  and slot, check field usability, and use the same main-summary list for the target. The list
  builder checks joined flags, and main-summary confirmation adds no HP or deployment check.
  Self-use, a different holder, a fallen member and a reserve are legal. The depot is outside
  the selection. [Field caller](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/main/mainactions.asm#L205-L285) ·
  [Caravan caller](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/caravan/caravanactions_1.asm#L711-L777) ·
  [Force list](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/battleparty.asm) ·
  [Confirmation](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/memberslistscreen.asm#L248-L350)
- Cancel before target confirmation and the effect never runs. Field target cancel restarts holder
  selection. Caravan reports the cancel then restarts Use, and cancel at the holder stage exits
  Use. On confirmation, apply the target effect first. Both callers then unconditionally remove
  the holder's selected slot, compact the other slots and refresh the holder. That last refresh
  reaches the target on self-use. No-effect text in an admitted effect still leads to payment,
  including zero-max-MP Honey and capped Apple. Field goes back to the Item submenu. Caravan
  offers another use. [Removal and refresh](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/itemstats.asm#L507-L530)

### Effects and feedback

Each of Water, Milk, Chicken and Bread draws once on the main RNG with range 3, adds 2, then
sets the amount for the source message before writing the base or maximum and then the current
value. An eligible Honey takes the same path. Pimento draws nothing. These amounts are the
requested increments, including when the wrappers clamp a smaller actual change. No stat-cap
precheck prevents payment. Stat and seed changes below are scoped to the effect itself. Input,
text and ordinary field simulation have their own clocks. [RNG routine](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/tech/randomnumbergenerator.asm#L10-L27) ·
[Stat wrappers](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/combatantstats_2.asm#L438-L633) ·
[Caps](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/sf2enums.asm#L263-L272)

| Item | Effect before holder removal | Result |
| --- | --- | --- |
| 9 Power Water | Base ATT +2–4, then current ATT + the rolled amount. Both clamp at 200. | 150 |
| 10 Protect Milk | Base DEF +2–4, then current DEF + the rolled amount. Both clamp at 200. | 151 |
| 11 Quick Chicken | Base AGI +2–4 up to 100, then current AGI + the rolled amount up to 127. Each wrapper preserves that byte's bit 7. | 152 |
| 12 Running Pimento | Read stored base MOV. Base 9 gives +0, 8 gives +1, every other base gives +2, including a supplied base above 9. Add the same amount to base and current MOV. Both helpers cap at 200. | 153, including +0 |
| 13 Cheerful Bread | Max HP +2–4 up to 200, then current HP + the rolled amount up to the new maximum. No zero-current-HP guard. A fallen target gains positive HP without a separate Raise action. | 154 |
| 14 Bright Honey | Max MP zero gives no change and no RNG. Otherwise increase max MP +2–4 up to 200, then current MP + the rolled amount up to the new maximum, including from current MP zero. | 148 or 155 |
| 15 Brave Apple | Clear target EXP first, then call one `LevelUp`, even at the class cap. | 148 or 244, then applicable gains/spell, followed by wait 3523 |

For Brave Apple, caps are 40 below class ID 12 and 99 from ID 12. A cap or the original terminal
missing-class scan returns level `0xFF` with no growth draws. EXP has already become zero. On
success, grow max HP, max MP, base ATT, base DEF and base AGI in that order, using the existing
growth formulas. Each active curve draws twice at range 128, and a NONE curve draws nothing.
Current HP and MP keep their values, even at zero. Increment stored level, check the exact
effective spell threshold (including the class-11 comparison and first-list inheritance), learn or
upgrade into the existing four-slot book where allowed, and refresh target stats. Use the level-up
result order for feedback. Show the level, nonzero raw stat gains in stat order, and a successful
new spell or rank increase. A full book or a same-or-higher rank can suppress a book change and its message while
level-up still succeeds. [LevelUp and raw result bytes](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/levelup.asm#L11-L151) ·
[Ally and class growth data](../../../docs/research/party/ally-growth.md#storage-model)

### Refresh and class load

`InitializeCurrentStats` copies stored ATT, DEF, packed AGI and **stored MOV_BASE**. Status effects
apply before worn equipment. Direct stat consumables do their two writes without refreshing the
target. Removal refreshes the holder, and actual later equipment/status refreshes and successful
level-up replace the previous current values using the new bases. This can distinguish a different
holder from self-use. For example, base AGI 4 under STUN gives current AGI 0. A roll of 2 from
another holder gives base 6 and current 2 until the target refresh, which gives current 1.
Preserve the existing high-bit representation separately from numerical AGI.

`Promote` reads the new class after `SetClass`, calls `LoadAllyClassData` and refreshes.
`LoadAllyClassData` writes the new row's MOV into MOV_BASE even if both classes have the same row
value. Thus a successful class load replaces the earlier Pimento base. ATT/DEF/AGI and HP/MP growth
remain earned. Decline leaves movement untouched. Ordinary equipment changes, battle return and
Continue carry the stored base through their normal lifetimes. Original `SaveGame` and `LoadGame`
copy the saved block starting at `COMBATANT_DATA`, which includes the stored base and current
bytes. **Unknown** remains the natural original use-to-reload sequence, including runtime menu
timing, use reach and later refreshes reached by a particular story route.
[Refresh and base copies](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/updatecombatantstats.asm#L10-L95) ·
[InitializeCurrentStats](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/updatecombatantstats.asm#L293-L305) ·
[Class load](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/newgame.asm#L109-L144) ·
[SRAM copy](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/tech/sram/sramfunctions.asm#L96-L149) ·
[Ring agreement](ring-equipment-design.md) ·
[Church agreement](../application/church-promotion-design.md#what-changes-on-success)

## Remake decision and implementation boundary

- **One command and result.** The admitted `PrivateFieldPreparation` item table binds all seven
  effects. The shared transaction resolves the holder, exact full word/slot and target with `FieldInventory`
  from the current revision or Caravan wait token. Both callers use the same prepared result from
  `FieldRecovery`. Validate the class/growth and book resources before publishing. Compute with the
  party's local main-seed value using `BattleRandom.NextMain`, apply the target, then compact and
  refresh the holder. For self-use remove from the changed actor/loadout, including Apple's newly
  learned packed book. Publish actor changes, slot, seed and localized result together using the
  existing exploration commit. Stale input, cancel, illegal/unavailable actors and unsupported
  resources leave the snapshot untouched. A valid no-effect still spends the slot. Use
  `FieldCommands` and `CaravanItemService` to project the outcome and return through their menu
  waits. Reuse FieldActionView and CaravanServiceView with bilingual messages for stat amounts,
  no-effect and level/book results. Acknowledgement or repeat input cannot pay again.
- **One level-up rule.** The EXP threshold path and Apple share growth, book and refresh in
  `BattleGrowthRules`. Apple clears EXP and
  calls it directly. Preserve the battle threshold's residual EXP, RNG and observation order with
  its existing caller. Return raw gain and spell-change operands for Apple feedback alongside the
  updated values. The remake resolves admitted ally/class pairs explicitly. A missing binding
  is unsupported before commit. A supported capped target gets the paid original no-level result.
  An unrelated ally's growth row is outside this slice even though the original scan can cross rows.
- **One persisted base.** An optional absolute `BaseMove` lives in `BattleActorProgress`. An absent value
  resolves to the current class definition's MOV. Use the existing class-resolved actor and
  carried progress across Field, reserves, battle entry, return and save. An absolute base models
  the source byte and avoids rebasing an increment when class content changes. A class-only value
  loses Pimento at every refresh. An inventory-derived or separate party-wide growth ledger would
  give consumed items another state authority. Successful church promotion clears the override
  when setting the new class. All other progress copies, including ordinary LevelUp, carry it.
- **Current values between refreshes.** Reuse the equipment/status stat projection for the
  ordinary refreshed result and `PartyDetailsQuery` for display. `CurrentStats` carries the
  DEF, packed AGI and MOV writes that differ from the refreshed result. For admitted static
  ATT contributions, the clamped projection from the new base equals the direct current write.
  This represents the observed current bytes until
  refresh and keeps neutral cases compact. A different-holder Quick Chicken on the stunned actor
  above is a test of this need. An immediate full refresh would change the result. Use these
  current values for the actual consumers, including field details and battle admission/turn
  stats wherever no refresh has occurred. Clear them on the actor's real full refreshes
  (equipment and pack removal, applicable status refresh, LevelUp success, class load, and
  existing battle/return resets). Preserve them through ordinary movement and saves. Respect the
  existing battle last-refresh-counter representation for status timing. HP and MP already have
  separate current fields, and numerical AGI keeps its established separation from the extra-turn
  flag.
- **Compatible saves.** Write optional values in field `SavedAlly` capture/restore (v1/v3) and
  `BattleSaveWire.Progress` (v2/v4/v5) for active and retained allies. Preserve their omission in
  older payloads and keep the existing class-version rules independent of growth. Resolve the
  old absent base only after resolving class, and default absent current overrides to the
  normal projection. Validate explicit MOV 0–200, current DEF/MOV 0–200 and packed AGI with
  its numerical and flag semantics at field and battle entry/restore. Keep progress copy,
  continuation equality aware of these fields. Check
  ordinary, previously promoted and reserve slots. Existing save identity and checksum rules
  apply. A save stores the applied values, remaining inventory and advanced seed, so Continue
  performs neither a draw nor an effect. Compatibility covers reading earlier slots with the
  new reader. Earlier binaries may reject new payload fields.

[Field command and result](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldCommands.cs) ·
[Shared field effects](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldRecovery.cs) ·
[Caravan Use](../../src/Sf2.Remake.Application/Runtime/Exploration/CaravanItemService.cs) ·
[Content binding](../../src/Sf2.Remake.Content/Scenarios/PrivateFieldPreparation.cs) ·
[Progress](../../src/Sf2.Remake.Domain/Battles/State/BattleGrowthDefinition.cs) ·
[Growth](../../src/Sf2.Remake.Domain/Gameplay/Sf2/BattleGrowthRules.cs) ·
[Actor stats](../../src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs) ·
[Field projection](../../src/Sf2.Remake.Application/Runtime/Exploration/PartyDetailsQuery.cs) ·
[Field save and restore](../../src/Sf2.Remake.Application/Runtime/Persistence/OpeningSave.cs) ·
[Battle wire](../../src/Sf2.Remake.Content/Persistence/BattleSaveWire.cs) ·
[Action atomicity](../../../docs/decisions/0019-state-and-content-driven-remake-engine.md#provisional-selection-atomicity-and-stale-input)

The implementation builds on the accepted Fairy Powder caller and static ring refresh.
Later ordinary promotion destinations use the same class-versus-earned-stat boundary. Battle
recovery consumables continue to use the existing live HP/MP fields.
[#1942](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1942) ·
[#1946](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1946) ·
[Promotion families](../application/ordinary-promotion-classes.md#family-inventory-and-actual-consumers) ·
[Battle recovery items](recovery-items-design.md)

## Acceptance and inputs

Existing receipts establish **custody**. Use copies of retained ordinary slots and check the
live holder/slot at selection. A receipt flag alone does not prove an item is still held.

| Item | Short ordinary starting point |
| --- | --- |
| Power Water | Battle15 Golem 143 kill witness gives it to Peter's slot 1, drop bit 29. Use the paid exit/return to enter Field with the item. F151 only records the trap. [Battle15 witness](../application/east-shrine-panel.md#optional-battle-chest) |
| Protect Milk and Quick Chicken | Use the observed Polca F148 pickup route and F609 church → Map44 F139 deck barrel route. Read the actual recipient after Search. Cold-save at the next admitted local recipe. [Search inventory and routes](../application/campaign-field-search.md#map-and-setup-inventory) |
| Running Pimento | Hassan Map9 (23,22), F153, with an ordinary receipt. Use a retained Hassan route or collect at the existing marker. Read holder and target before choosing. [Hassan Search](../application/campaign-field-search.md#hassan-the-shrines-elven-village-and-harpy-pond) |
| Cheerful Bread | Pacalon F172, carried by Jaha in both retained #1927 castle slots. The slots have initialized fallen members available for the zero-HP case. [Pacalon receipts](../application/pacalon-first-visit.md#church-and-continue) |
| Bright Honey | Ketto F170, held by Chester (ally 2) with max MP zero, including in the Pacalon descendants. Use sibling copies to check the zero-max-MP outcome and ordinary use on an MP-capable ally. [Ketto custody](../application/ketto-town.md#search-and-departure) |
| Brave Apple | Fairy Cave F161, held by Sarah. The Chinese #1849 cold slot has living Kazin/MAGE 24 with MUDDLE 1. Apple's 24→25 learns rank 2. Pacalon Sarah/PRST 40 has Apple and nonzero EXP in the English castle slot for the cap branch. The #1872 Caravan refusal is the unsupported baseline. [Fairy Cave pickup](../application/fairy-cave.md#walk-search-and-return) · [Kazin threshold](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats04.asm) · [Caravan route](../application/caravan-services-design.md#current-item-implementation) |

Exercise each ordinary Use from available custody through result, return and the next supported
Save and fresh title Continue. Reuse the Floor World/Caravan Apple route for the second caller.
Use sibling copies for paid failure and cancel, and read gameplay state before/after the commit.
Observe the actual compacted word slot, changed base and current stats or book, EXP, RNG and
selection control, with English and Chinese results. Learn/book persistence can be accepted
independently of MUDDLE casting, which is covered by
[#1967](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1967). Use a supported battle
to exercise the Pimento movement budget and a weapon/ring change on the same actor. Equip and
remove a legally held eligible ring where available, and check promotion's class reload on a
separate eligible copy. Use actual worn items and battle actions for native claims. If a retained
item is absent and its short ordinary pickup route is unavailable, mark that native witness
pending and cover the rule with an engine behavior case.

Use small engine cases for numerical caps and raw gains, packed AGI, the different-holder refresh
case, base MOV 8/9/supplied >9, HP and MP zero, old omitted fields and battle/retained-allies
round trips, Apple's exact threshold/rank/no-slot/cap and unchanged ordinary EXP growth. Include
canceled and stale requests with a duplicate item elsewhere in the pack, and compare same-actor
use with distinct holder and target. Keep supply flags and other party rows intact. Bound
native evidence to ordinary input, running state, projection, errors and cold round trips.
[Save modes and recipes](../application/battle-save-and-continue.md#manual-save-history-and-developer-files) ·
[Pacalon input and budget owner](../application/church-promotion-design.md#inputs-commands-and-observation-budget)

The retained sample field slots range from about 7 to 50 KiB. Budget up to 16 disposable copies at
128 KiB and up to 2 MiB of compact observations, about 4 MiB before build output. Measure the first
run and reuse the registered world and scene resources. Expect only a few KiB of definition and
message changes. If a content dependency requires rebuilding, make one
checkout-local package for the affected selection and reuse it. No additional original runtime
capture is needed for this design, and #1716 stays parked. Record any completed native exit failure
separately from observed Save/Continue behavior.

### Ordinary input observations

**Confirmed in the remake.** Mount the built Debug adapter, registered `issue1966/content/package`
world and scenes, and `issue1941/content/catalog-final` via the
[Pacalon setup](../application/pacalon-first-visit.md#continue-after-worktree-cleanup). Use a fresh
external player namespace for each read-only seed copy. Start through title Continue → slot 1.
Field Use selects Item, holder, Use, slot, target and Confirm. Caravan Item → Use confirms on
target selection. Dismiss the result, leave the menu and save at settled input.

| Starting slot under the registered handoffs | Ordinary input and result |
| --- | --- |
| `issue1927/pacalon/slots/castle-en.json` | Cancel Bread's target selection, then confirm Jaha's third slot on fallen Kazin. HP 0→4, maximum HP 35→39. Use Chester's Honey on himself with maximum MP zero. It consumes the slot with no RNG draw. Sarah's self-Apple at PRST 40 consumes the slot and clears 88 EXP with the same level, seed and spellbook. Both ineffective confirmations identify item payment in their English feedback. |
| `issue1849/slots/loot-zh-CN.json` | Use Sarah's Apple on Kazin. MAGE 24→25, EXP 44→0, HP/MP remain 32/48, and the packed MUDDLE word changes from 7 to 71. Chinese feedback reports the level, gains and rank increase. F161 remains set. |
| `issue1927/pacalon/slots/departure-zh-CN.json` | Approach the visible Caravan on Map71. Choose Chester's fourth slot, cancel at the target list and reopen it. On Sarah, Honey raises current and maximum MP 56→59 with one main RNG draw. The Chinese result displays +3. Chester's pack changes from `[184,1,115,14]` to `[184,1,115,127]` and F170 stays set. |
| `issue1941/player-saves/hassan-en-final/search/sf2-opening/slot1.json` | Use Sarah's Pimento on Bowie. Base/current MOV 6→8 with no item RNG draw. Open Bowie's Equip menu and confirm Unarmed. The sword becomes bare and MOV stays 8. Save at Hassan's church position with F153 set. |

The ordinary runs establish consumption, feedback and control return for these items. Power Water,
Protect Milk and Quick Chicken have their existing acquisition evidence and controlled engine Use
cases. This pass did not find a registered short retained Use seed for them. The effect-specific
ordinary Use observations remain **Unknown**. The new tests also cover direct-current stat
retention versus real refresh, Pimento in the movement grid, an ordinary class load, and old field
and battle formats including a retained ally. [Engine behavior cases](../../tests/Sf2.Remake.Engine.Tests/PermanentGrowthItemTests.cs)

Fresh processes re-saved the entire same decoded `data` for the castle (sequence 5→6), cave
(65→66) and Caravan return (6→7). All three reached the next field input. A Caravan rehearsal
selected the wrong holder after returning to a remembered cursor and recorded a refusal. The
corrected input verifies the holder, word and target wait before payment, and reads the actual
growth result. Growth and cold-Continue runs exited 0. Some recorded ObjectDB shutdown
warnings after reaching the functional check. [Paused shutdown follow-up #1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716)

### Continue with producing scratch unavailable

Take the external Hassan post-Unequip slot from the journey above. Temporarily move the entire
producing `local/` aside, copy the machine configuration template into a fresh `local/`, and
use the [registered Pacalon Continue setup and command](../application/pacalon-first-visit.md#continue-after-worktree-cleanup)
with the Hassan slot as its seed and a fresh save namespace. The registered `Pacalon.gd` driver in
`continue` mode saves immediately and takes a field movement input after dismissing the result.
Use `python -X utf8` to decode the slot envelope's base64 `payload` and compare the entire
`data` object against `immediate-resave-slot.json`. Check that `sequence` increases once and
that the `loaded.json` and `after-continue.json` player positions differ with settled input.

The two complete data objects matched. The record sequence moved 60→61, with base MOV 8,
Bowie's `[71,1,61,123]` pack, and F153 retained. The next input moved from (8,9) to (7,9)
on Map9 with no projected failure. Godot exited 0 with an ObjectDB shutdown warning. The
mounted 101,835,469-byte world and 635,539-byte scene selection were reused without regeneration.
The observed field save is about 50 KiB, within the existing budget.

### Engine and adapter checks

`uv run sf2 verify engine` completed 2,930 passes and 49 configured private-input skips.
The initial focused run found `save-null-value` in
`ChickenWritesLiveAgilityBeforeRefreshingOnlyTheHolder(selfUse: false)` and both variants of
`BattleContinueKeepsMoveAndDirectValuesForDeployedAndRetainedMembers`. The codec now omits
absent sparse components and the affected checks pass. The actual adapter built in Debug with
zero warnings and errors before the cleanup check.
