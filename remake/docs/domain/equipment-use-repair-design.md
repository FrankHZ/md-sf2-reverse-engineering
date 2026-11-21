# Equipment Use, damage and Repair through first Pacalon

This High-rigor design for [#1952](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1952)
covers ordinary Battle Item → Use, the exact slot that survives or is destroyed, the merchant
Repair transaction, and cold Continue. Action construction, stat refresh and persistence meet at
the same loadout. A mismatch can lose gear, advance RNG, or charge gold without repairing the
selected copy. Implementation is pending. The accepted [ring slots](ring-equipment-design.md),
[status refresh](support-spell-status-design.md), [offensive item origin](offensive-items-design.md#proposed-remake-boundary),
[promotion and rebind](../application/church-promotion-design.md), and
[Deals and shop return](../godot/field-and-shop.md#hassan-deals-and-cold-continue) supply the dependencies.

## Original equipment and callers

**Confirmed** below means static at the fixed USA baseline unless a retained observation is
named. Reproduce by joining `data/stats/items/itemdefs.asm` weapon/ring rows with nonempty
`useSpell` to `data/stats/spells/spelldefs.asm`, then checking the reached shop inventories,
map items, ally starts and the enemy drop table through Battle26. For each pinned source link,
`git show c834c652b6862bc5679fd7f69a38a7093206efc6:<path>` in the pinned upstream
checkout reproduces its source, including the pre- and post-call branches. The existing spell
and turn fixtures give the ROM, emulator and controlled boundaries for those observations.
[USA identity](../../../manifests/roms/sf2-us.json) ·
[Item definition contract](../../../docs/design/contracts/party/item-definition-data.md) ·
[Drop table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/global/enemyitemdrops.asm#L26-L83) ·
[Shop inventory](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/items/shopinventories.asm#L99-L133)

| Equipment | Source rule and ordinary reach | Design boundary |
| --- | --- | --- |
| Power Ring (19) | ALL classes, ATT +5 worn, ATTACK 1 on Use. Battle10 drop, once bit 4. Jaha's legal receipt and Battle26 equipped Continue are recorded. | First implementation. |
| Protect Ring (20) | ALL classes, DEF +5 worn, BOOST 1 on Use. Battle21 drop, once bit 8. Peter's legal receipt and Battle21 equipped Continue are recorded. | First implementation. |
| Quick Ring (21), Running Ring (22), Chirrup Sandals (126) | Source `NOTHING` Use. Quick's BREAKABLE flag supplies no effect or reachable damage roll. | Retain ordinary Use refusal. Sandals acquisition and audio belong to [#1962](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1962). |
| Taros Sword (110) | Battle18 enemy BOLT 2 and potential rare drop. Mask NONE refuses player Use even from an intact bare slot. Enemy Use bypasses ally breakage. | Preserve the accepted Taros path and player refusal. |
| Black Ring (24) | Battle26 drop, once bit 10. WIZ/SORC only, including bare BLAZE 2 Use. Neither retained Pacalon party has such a class. | [#1926](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1926) owns admission, curse and BLAZE 2. It can consume the common slot-damage operation when admitted. |
| Indra Staff (92) | Pacalon weapon shop 10 sells it for 3,200. WIZ/SORC/VICR, weapon ATT +25, physical range 1, BREAKABLE, SPOIT 1 on Use. A legally promoted Kazin or Sarah can use it in the original at this frontier. | Class-dependent follow-on within this frontier. Current shop stock and bare possession alone establish no Equip or Use. Rebase onto an accepted WIZ or VICR ordinary consumer from [#1956](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1956) before binding its weapon or effect. |

The reached shop/drop/grant join yields three scoped player effects including the conditional
Indra Staff, plus Black Ring under its own issue. Later rows include the White Ring and Demon Rod
beyond the Dry Stone crossing, Valkyrie on the Prism Flowers map, the later Mage Staff drop and
smith weapons. A definition, enemy-held word or future ally start alone grants no player custody.
[Ring rows](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/items/itemdefs.asm#L213-L281) ·
[Indra row](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/items/itemdefs.asm#L943-L951) ·
[Pacalon merchant](../application/pacalon-first-visit.md#town) ·
[Dry Stone crossing](../content/campaign-items.md#story-items-and-mithril) ·
[Retained ring inputs](ring-equipment-design.md#implemented-behavior-and-ordinary-input)

### Selection and effects

**Confirmed.** Battle Item Use carries the full held word and selected slot into the action.
Both hover range and confirmation call `IsItemUsableByCombatant`. A weapon or ring must match the
holder's **current** class. Neither equipped nor broken is a gate. The equipment helper compares
the Use byte against `$FF` even though the no-spell byte is `$3F`. The later item range builder
explicitly skips `SPELL_NOTHING`; the empty target list returns to selection. Preserve refusal
without exposing an inert row as castable. Use takes its targeting terms from the spell row. The
Item scene path bypasses the Magic MP and Silence setup and still passes normal actor-control and
inaction checks. A canceled target returns to the action menu. No plan has yet committed. Use
`PopulateTargetsListForItemUse` and the accepted support target rules for living, placed combatants,
the confirmed movement stop, and source processing order. [Menu slot return](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/itemmenu.asm#L155-L165) ·
[Use selection and cancel](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/battlefunctions_2.asm#L343-L449) ·
[Eligibility](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/itemstats.asm#L667-L704) ·
[Sentinel and source range](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefield/buildactionrangegrids.asm#L73-L151) ·
[Magic and Item setup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/initbattlesceneproperties.asm#L115-L144)

| Effect | Center and recipients | Construction, reaction and refresh |
| --- | --- | --- |
| ATTACK 1 | Living teammate at distance 1–3, radius 0. The holder cannot self-center. | Write ATTACK bits `0xC000` before checking the previous ATTACK counter. Fresh target succeeds without an effect draw, with one full-word reaction and +5 status EXP contribution. An active target consumes one range-8 draw against threshold 8 and always fails the reaction. The write survives. The current ATT still uses its previous applied counter until that target refreshes. |
| BOOST 1 | Teammate or self center at distance 0–1, radius 1 diamond around that center. A neighboring teammate can be outside center range. | Same construction and per-target recast behavior using BOOST's separate `0x3000`. Reuse the existing BOOST support effect, including fresh and failed targets in sorted order. The item does not enter the Magic book. |
| SPOIT 1 | One living opponent at distance 1–2, radius 0. | Roll range 3 then add 3. Drain the lesser of that amount and the target's current MP. The recipient's MP gain is capped at its own max MP. Target then user react, preserving status; zero MP still takes the draw, reactions and +5 status EXP contribution. There is no HP damage, power scaling or status resistance gate in this effect. Implement only with the admitted Indra consumer. |

**Confirmed.** ATTACK's magnitude is `floor(base ATT × applied counter / 8)`. It uses earned
base before weapon and ring contributions and the live ATT cap. For base 41, weapon +10,
Power Ring +5 and three applied counters, current ATT is 71. At one counter it is 61. BOOST
uses the same fraction of base DEF and numeric AGI, then the existing SLOW and equipment
rules apply. Protect Ring's +5 is outside the fraction. ATTACK, BOOST and SLOW coexist, with
independent counters; each living queue owner's completed entry processes SLOW → ATTACK →
BOOST, then recovery/poison and final refresh. ATTACK subtracts `0x4000` without RNG and gives
expiry message 350 at 1 → 0. A self-BOOST may have already reached two by the next input.
Death cleanup and normal return/entry clear the temporary fields through their existing
boundaries. Keep completed battle effects through settled battle Save. [Spell rows](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spelldefs.asm#L122-L160) ·
[ATTACK construction and controlled observation](../../../docs/design/contracts/battle/spell-resolution.md#confirmed-attack-1-construction-subset) ·
[BOOST and after-turn evidence](support-spell-status-design.md#original-rules-and-evidence) ·
[SPOIT row](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spelldefs.asm#L346-L352) ·
[SPOIT reactions](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/castspell.asm#L522-L575) ·
[Original stat refresh](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/updatecombatantstats.asm#L10-L94) ·
[Original tick order](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleloop/processafterturneffects.asm#L118-L162)

### Damage, destruction and custody

**Confirmed.** Construction visits every target and its drop seam before checking the item
once. A valid but ineffective recast still reaches this check. If turn inaction or zero
targets exits before the loop, no durability check runs. On ally Use of breakable equipment,
an intact slot takes one main RNG draw with range 4. Zero sets the broken flag in that slot
and queues smoke feedback. A nonzero draw leaves the word alone. An already broken slot
reaches its effect and then is destroyed without a range-4 roll. The remove helper compacts
the actual slot and refreshes the holder immediately during construction; it does not run
the Discard or Deals helper. Other held words move intact. Prepared target reactions play
after construction, so a self-target's queued reaction sees the already removed item. The
fresh break helper sets only the flag. Worn gear contributes before and after damage since
refresh and Equip check identity, category, class and equipped bit. Destruction removes only
the selected copy's contribution. A failed recast on an untouched target keeps the earlier
applied counter across the scene until that target actually refreshes.

Original pack words use `0x007F` for identity, `0x0080` for equipped and `0x8000` for broken.
For Power Ring, worn intact is `0x0093` and worn damaged is `0x8093`. Direct AddItem keeps
identity and damage while clearing equipped. The original Depot stores identity bytes, so
Deposit or exchange into it loses damage and retrieval is bare intact. Deals counts by
identity too. These existing custody boundaries differ from direct Give or merely carrying
a word through Save. Preserve them explicitly so a Depot round trip does not masquerade
as a Repair witness. Original Sell checks price and curse without a damage-bit gate. The
accepted remake Sell slice covers unbroken gear and currently refuses broken words. Keep
that Sell boundary in this slice and document it with the implemented capability.
[Use dispatch](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/useitem.asm#L8-L22) ·
[Target loop and damage order](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/battleactionsengine_1.asm#L76-L131) ·
[Damage and destruction](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/breakuseditem.asm#L8-L59) ·
[Original pack helpers](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/itemstats.asm#L175-L305) ·
[Remove and refresh](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/itemstats.asm#L507-L527) ·
[Depot storage](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/caravaninventory.asm#L10-L25)

**Confirmed.** Per-target effect draws precede damage RNG. The normal living-ally award follows
the durability branch. Every fresh ring reaction contributes 5 (total capped at 49). The
same-side flag exempts these buffs from halving, including in a marked battle. Then the
range-16 EXP plus and minus draws run in order, with minimum 1 for an admitted no-effect
action. The current generic Item status-award branch halves in a marked battle. Reuse the
friendly support reward terms for the two ring effects. SPOIT targets the other side and
uses the corresponding status award. [Award source](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/giveexpandgold.asm#L32-L73) ·
[Remake award owner](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2BattleProgressionRule.cs)

### Merchant Repair

**Confirmed.** Repair selects a joined member and held slot. Price is the definition's
unsigned list price shifted right two, so 750 per ring and 800 for Indra. An intact item is
refused before confirmation. After Yes, check gold (equality is enough), then test the equipped
curse refusal, then decrease gold and clear only broken on that slot. No stock-membership,
empty-slot or class gate applies. A full pack and bare gear are valid. Cancel, insufficient
gold and refused curse branch before payment. Accepted Repair and refusal loop to the selection
screen; cancel there goes back to the shop menu. `RepairItemBySlot` clears the bit in place and
does not refresh numeric stats, consistent with the passive contribution staying through damage.
The UI must refresh the slot's broken indicator and gold from the new state.

The source compares `GetEquipmentType`'s result with `ITEMTYPE_BIT_WEAPON` rather than
`EQUIPMENTTYPE_WEAPON`. Both constants equal 1 in the pinned source, so that dispatch still
reaches the weapon branch. Curse means the **selected equipped slot** and a cursed definition;
a bare cursed copy is not refused by that branch. Black Ring gameplay remains with #1926.
The original helper's intact branch writes `d1` while its output comment names `d2`.
The ordinary caller gates intact first and does not depend on that helper result.
[Shop Repair source](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/shop/shopactions.asm#L327-L435) ·
[Equipment type constants](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/sf2enums.asm#L1091-L1155) ·
[Damage-bit helper](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/itemstats.asm#L245-L273)

**Inferred.** An out-of-turn ATTACK target at applied count 1 can be recast to stored count
3 and still strike using count 1 until a real refresh. This follows the retained construction
write and the same reaction boundary established for BOOST. A reward level-up or holder
removal can refresh its own actor before the next turn. **Unknown** are a natural original
ring damage → Repair → SRAM round trip and the equivalent remake journey, including a
naturally divergent ATTACK Continue. Use the retained construction and stat-refresh evidence for the design.

## Remake decision and implementation order

1. **Status and stat authority.** Extend the existing `BattleStatCounters` with ATTACK and
   reuse the actor's last-refresh operand for all three timed fields. Factor the friendly
   counter write/recast in the existing support effect; ATTACK and BOOST keep separate masks,
   range, stats and text. Keep `BattleActorState.Attack` as the current resolved value and
   compute it at the shared stat refresh from the actual base, applied ATTACK and worn loadout.
   Preserve explicit initialized enemy ATT where that existing path owns it. Use the same
   calculation for candidate equipment previews, removal, growth and reconstructed saved ATT.
   Applied bits survive construction-only changes and clear at actual refresh. In particular,
   preparing a selection or inspecting a record may not refresh it. Extend the existing
   after-turn expiry, death and return owners with ATTACK in source order.
2. **Item origin and one slot transaction.** Use the Item command and full-word origin
   proposed for offensive consumables. Content joins an explicit finite item effect and
   breakability term to the accepted equipment and spell records; Domain uses these typed
   terms and current class. Carry revision, actor, confirmed position, slot and full expected
   word through validation. Re-resolve the target set on commit. Keep a single prepared action
   with target writes, reactions, wear outcome, reward and seed in that order. Validate it
   before publication and publish exactly once. Model wear as retain, mark damaged or compact
   removal on the resulting pack. Share the existing removal/refresh and status-reaction
   paths. Reuse the existing scene waits for item name, targets, ineffective/wear feedback,
   reward and expiry. A mutation is computed from the validated slot once; a scene
   acknowledgement cannot repeat it. The item origin skips the spell cost stage entirely.
   Folding these effects into Magic would apply book, MP, Silence and cost-refresh behavior.
   A second effect implementation would duplicate the existing BOOST recast and reward paths.
3. **Repair and carry.** Reuse `ShopWait` and its Member → Slot → Confirm → Result path with
   `ShopOffer`'s full word and slot. Add Repair results to the existing view and translations.
   Quote from trusted catalog terms, then on Yes revalidate the wait, member, slot, full
   word and price. Resolve affordable/cursed before preparing the single party replacement.
   Commit payment and clearing the bit in one publication, preserving status, worn state,
   slot order, flags and other custody. A cancelled offer, stale token/word, no damage,
   unsupported term or failed check spends zero gold. Dismissing a completed Repair result
   goes back to Repair selection; leaving the shop resumes its ordinary program. Permit
   damaged admitted gear through Equip, Give and return/restore. Apply the documented
   identity-only Depot and rare Discard custody rules where used. Audit unrelated slots in
   each selector so one damaged word cannot disable acting, swapping or using a different
   item with the same pack.
4. **Indra consumer.** After an accepted ordinary WIZ/VICR route, admit the single weapon
   record and its MP-transfer effect with the same origin, damage and Repair. Test capped
   recipient MP, empty enemy MP, range/refusal and 800 gold repair. A class-independent
   Repair transaction can be shared first. An equipped/unequipped copy remains subject to
   the live class mask for Use. No future weapon, recruit or mainline segment is necessary.

Steps 1–3 build one usable ring loop. Sequence the shared Item owner after the accepted
common origin lands, or explicitly coordinate that small shared change with the pending
offensive and recovery slices before editing. Keep one writer for selection/scene/save,
catalogs and the shop. The Deals result and static rings are accepted dependencies.
[#1943](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1943) has independent live
ownership; implementation uses accepted main.

### Persistence

Retain the remake's existing normalized bit `0x0100` for broken. Original upper bits
are translated at import. The four words remain the pack authority. A worn Power Ring
changes 147 → 403 → 147 across damage and Repair, or is removed on broken reuse.
A worn Protect Ring changes 148 → 404 → 148. Normalize any source import at its boundary;
do not persist an extra wear field. Keep legacy bare and worn words byte-for-byte, and
preserve existing invalid-word bounds. Widen the equipment-specific validator only for
admitted breakable gear and its legal equipped forms. No AI-only flags enter ally words.
Give preserves damage but clears equipped at the receiver, and identity-only custody stays
in its existing representation.

Existing field formats store the four words already. Battle v2/v4 can carry damaged gear
with no active timed effect. Extend v5's timed mask and optional `lastRefreshCounters` to
include ATTACK with the same settled validity rule per field: it either equals stored bits
or represents applied 1/2 with stored 3 after a failed recast. Absence means all stored
counters apply. Reject the timed bits on retained allies and in v2/v4. Validate the existing
saved current `Attack` against the applied operand and loadout without simulating a new
refresh. Keep v5's class-mode term, v2/v4 writers where appropriate, legacy field readers,
recipe and atomic slot publication. Verify an old slot with the new reader. Earlier readers
can refuse newly supported timed or damaged states at their guards; recovery uses the
existing explicit backup path. Full settled battle Continue retains actor and pack, queue,
seeds, class, statuses and effective stats. Field Continue after retreat and after Repair
retains gold, gear and custody with temporary stats cleared.
[Battle save agreement](../application/battle-save-and-continue.md) ·
[Wire recipe](../../src/Sf2.Remake.Content/Persistence/BattleSaveWire.cs) ·
[Detached restore](../../src/Sf2.Remake.Domain/Battles/State/BattleContinuationRestore.cs)

### Candidate owners for implementation

All source paths below are relative to `remake/` except the localization preparation
path. Review ownership and changed valid states on the implementation baseline.

| Slice | Exact candidate files and seam |
| --- | --- |
| Status, stats and save | `src/Sf2.Remake.Domain/Battles/Rules/BattleStatCounters.cs`, `BattleEquipment.cs` in the same directory; `src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs`, `BattleContinuationData.cs`, `BattleContinuationRestore.cs`; `src/Sf2.Remake.Domain/Gameplay/Sf2/SupportSpellDefinition.cs`, `SilenceRules.cs`, `BattleGrowthRules.cs`; `src/Sf2.Remake.Application/Runtime/Battles/BattleAfterTurnContinuation.cs`, `BattleUnitProjection.cs`; `src/Sf2.Remake.Application/Runtime/Persistence/BattleSave.cs`, `BattleSaveStory.cs`; `src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs`, `src/Sf2.Remake.Application/Runtime/Exploration/IOutcomeReturnPolicy.cs`; `src/Sf2.Remake.Content/Persistence/BattleSaveWire.cs`. Check effective ATT existing damage, growth, projection and action validator consumers. |
| Item, Content and scene | `src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2ItemAction.cs`, `Sf2SupportSpellAction.cs`, `Sf2BattleProgressionRule.cs`; `src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs`, `IBattleActionRule.cs`, `BattleInventory.cs`, `BattleGive.cs`, `BattleDiscard.cs`; `src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs`, `PrivateSupportSpellBindings.cs`, `PrivateBattleDefinitionReader.cs`, `PrivateBattleScenarioReader.cs`; `src/Sf2.Remake.Application/Runtime/Battles/BattleChoices.cs`, `BattleCommandDispatcher.cs`, `BattleSceneContinuation.cs`, `BattleActionCommitter.cs`, `BattleAdvancer.cs`; `game/src/Battles/BattleSceneView.cs`. Reuse existing menu projection and effect data. Change a consumer when the accepted path lacks the required behavior. |
| Repair and field carry | `src/Sf2.Remake.Application/Runtime/Exploration/ShopService.cs`, `FieldInventory.cs`; `src/Sf2.Remake.Application/Content/Scenarios/ShopDefinition.cs` and `src/Sf2.Remake.Content/Scenarios/ShopInstructionReader.cs` if the offer requires repair admission terms; `game/src/Exploration/ShopServiceView.cs`; `game/src/Localization/GameText.Keys.cs`, `DisplayNames.cs`, `game/localization/en.po`, `game/localization/zh_CN.po`. Use the stable identity catalogs, with a bounded update via `src/sf2tool/remake_localization.py` only for a missing name. |
| Acceptance | Extend the actual behavior in `tests/Sf2.Remake.Engine.Tests/SupportSpellActionTests.cs`, `RingEquipmentTests.cs`, `ShopServiceTests.cs` and `BattleSaveContinuationTests.cs`, or use one focused equipment-use test file there for the new action. Update [campaign items](../content/campaign-items.md), [capability status](../capability-status.md), [ring owner](ring-equipment-design.md), [gameplay rules](gameplay-rules.md) and [shop owner](../godot/field-and-shop.md) for the admitted behavior. |

## Acceptance and budget

**Controlled engine behavior.** Exercise both ring effects from legal bare and worn items,
including a silenced zero-MP user, a legal moved stop, an out-of-range or self-ATTACK center
and BOOST neighbors outside the center-selection range. Try target cancellation, a stale
full word with unchanged item ID, wrong class, no spell and no target. Compare state and
seeds on rejection. Cover fresh success and active recast, with distinct ATTACK/BOOST/SLOW
bits and applied operands, and living-owner tick/expiry order. Use distinct contributions
to verify the next real strike reads effective ATT, including one recast followed by cold
Continue. Vary weapon and ring occupancy across damage, equip, growth and self-target
destruction. With controlled seeds assert the actual order of recast draws, one range-4
draw for an intact multi-target Use, and award draws, including a miss then smoke. Prove
a broken reuse applies its effect then removes one slot with no wear draw or Deals entry.
Use duplicates and a worn item after the selected slot so removing by identity fails the
case. Repair a worn and a bare copy in a full pack, with exact and insufficient gold,
intact and stale slots, cancellation, cursed-bare and cursed-equipped outcomes using
authored terms, and repeated confirm/dismissal. Assert that no rejection alters unrelated
words, passives, custody, RNG or gold. Include legacy v2/v4, v5 with an ATTACK divergence,
and field/battle reconstruction of a damaged word before repair. These tests use actual
engine entry points and controlled inputs.

**Ordinary remake input.** Reuse the English worn Power Ring Battle26 slot from the
[ring review](ring-equipment-design.md#battle-continue-after-worktree-cleanup), and Peter's
worn Protect Ring Battle21 slot from the same accepted ring slice. Verify live owner and
pack on entry. Use the first reachable holder turn and a valid target. Observe one
cancel/reopen, projected range, localized reaction and control return for each supported
effect across those histories. For the short Repair witness, prefer the Power Ring
Battle26 branch. Target an adjacent ally, and on natural smoke save at settled Movement
and cold Continue. Then use the existing paid EGRESS route to Map36 and walk south into
Pacalon town for the merchant. Confirm a decline then accept 750 gold on the real held
slot, observe the word and passive, exit to ordinary control and Save → Quit → cold
Continue. Check the intact word, gold, class, custody and playable input. Peter's
Battle21 branch checks BOOST and retained item; use it as the alternate ordinary repair
branch if it damages first and a reached merchant is available via its established return.
One successful natural damage-to-Repair sequence is sufficient. Exercise a second Use
after natural damage on a scratch fork of the saved damaged slot if reachable in the
same budget. Do not remove the only copy on the Repair branch. Reuse that legal damaged
checkpoint for the other language's retail-style Repair input and cold Continue, with its
own writable slot; locale change is no evidence of a second independent acquisition.
[Pacalon visit and shop](../application/pacalon-first-visit.md#town) ·
[Church and EGRESS anchors](../application/pacalon-first-visit.md#church-and-continue)

The original generator gives 16,384 of 65,536 next states to each range-4 result. Expect
one damage per four intact completed uses under the uniform-state model. Eight varied
attempts give about 90% under an independent-draw approximation, which is a planning
estimate for deterministic gameplay, not a coverage guarantee. For the damage witness, stop at the first damaged checkpoint or eight intact uses total
across the two legal branches, whichever comes first. Budget one ordinary action per effect
within that cap, including a first use of the other ring if the first action damages gear. Replaying the same seed and inputs gives the same result. Advance through ordinary
turns and vary legal targets; do not edit slots, seeds or RNG to force the player witness.
If the cap yields no natural damage, publish the normal Use observations, controlled
damage and Repair outcomes, and leave the natural loop **Unknown**. No new original H3
run is proposed. [Original generator](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/tech/randomnumbergenerator.asm#L10-L25)

Reuse the stable Pacalon world (~102 MB read-only), battle scenes, names and catalogs, and
the current debug installation. The two retained battle slots are about 41 and 37 KiB.
Budget up to eight copied or generated slots at 64 KiB each plus less than 1 MiB of compact
state/input/projection/error receipts. Load the existing world once per necessary process;
new campaign-package output is zero. Measure the two seeds and check catalog coverage
before running. If an identity or scene is missing, locate the smallest required content
update and re-estimate before generating it. Use one serial debug owner. Observe state,
input, projection and errors without screenshots. Preserve exit results separately from
save verification. [Stable setup and catalog route](../application/pacalon-first-visit.md#continue-after-worktree-cleanup)

Implementation checks use `uv run sf2 verify engine`, `uv run sf2 verify adapter` for the
changed views, and the ordinary suffix above. On a clean committed head use
`uv run sf2 verify plan --base origin/main --head HEAD` and record exact-head CI.
This design slice uses direct source, content, link and changed-path checks with the
documentation-only public CI selection.
