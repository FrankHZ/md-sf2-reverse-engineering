# Static ring equipment

This High-rigor design for [#1946](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1946)
covers Power, Protect, Quick and Running Ring through first Pacalon and Battle26. A player can
wear a ring and a weapon together, inspect the resulting stats, act with them, and move or remove
either item through the supported menus. The same party and equipment must survive Continue.
Movement selection, class rebind and save reconstruction share the same loadout, so a failure
in any one of them can leave an otherwise legal party unable to act or resume. The
[capability trace](../capability-status.md#connected-battle-equipment-agreement) records current
support and [ordinary input acceptance](#implemented-behavior-and-ordinary-input).

## Original rules and evidence

**Confirmed (fixed USA source and item data).** The four carried words encode identity in bits
0–6 and equipped in bit 7. Item 127 is empty. Type comes from the item definition, with WEAPON
and RING separate type bits. `GetEquippedWeapon` and `GetEquippedRing` independently scan the
four slots for the first equipped nonempty item of their type. `GetEquippableItemsByType` uses
the corresponding type and tests the definition's 32-bit mask against `1 << currentClassId`.
`ALL` is `0xFFFFFFFF`. Eligibility to wear gear and the equipped bit are separate facts.
[Item record and ROM parity](../../../docs/design/contracts/party/item-definition-data.md#item-identity-and-fixed-record) ·
[Pinned item routines](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/itemstats.asm#L79-L169) ·
[Pinned eligibility routines](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/itemstats.asm#L590-L704) ·
[Pinned mask and word constants](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/sf2enums.asm)

| Source ID and ring | Equip mask | Equipped effect | Separate Use metadata |
| --- | --- | --- | --- |
| 19 Power Ring | ALL | ATT +5 | ATTACK 1, BREAKABLE |
| 20 Protect Ring | ALL | DEF +5 | BOOST 1, BREAKABLE |
| 21 Quick Ring | ALL | AGI +5 | No spell, BREAKABLE flag |
| 22 Running Ring | ALL | MOV +2 | No spell |

All four have RING and RARE type flags and zero in both item range fields. Physical range uses
the equipped weapon or the existing unarmed range of one. Bow minimum range, critical prowess,
Achilles Sword effectiveness and weapon graphics keep their weapon-specific inputs.
[Pinned ring rows](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/items/itemdefs.asm#L213-L251) ·
[Existing range and equipment behavior](gameplay-rules.md#authored-battle-weapon-equipment) ·
[Weapon graphics lookup](../../../docs/design/contracts/party/item-definition-data.md#special-caravan-and-weapon-graphics)

**Confirmed (caller and refresh order).** Battle Equip opens separate weapon and ring choices
and replaces or clears the old item in the chosen category before returning to the action menu.
Both lists share the fourth-cell sentinel for an empty category and can fill that cell with a
fourth eligible item. Field and Caravan enter the same member item-summary Equip path, which also
selects weapons and rings independently. Equip, unequip and compacting removal reach `UpdateCombatantStats`. `AddItem` clears equipped on
the moved word and retains the original broken bit (bit 15). The depot stores identity.
[Battle Equip caller](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/battlefunctions_2.asm#L461-L671) ·
[Field caller](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/main/mainactions.asm#L506-L526) ·
[Caravan Equip caller](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/caravan/caravanactions_1.asm#L911-L934) ·
[Member item-summary Equip](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/memberslistscreen.asm#L1849-L1918) ·
[Slot mutation routines](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/itemstats.asm#L180-L206) ·
[Original depot contract](../../../docs/design/contracts/party/service-interactions.md#caravan-source-boundary)

The original refresh copies the combatant's stored base ATT, DEF, packed AGI and MOV to
their current fields. In particular, `InitializeCurrentStats` copies `MOV_BASE` to `MOV_CURRENT`.
Class loading sets base MOV from the class row. Running Pimento can change the stored base and
current MOV before a later refresh. The refresh applies ATTACK to ATT, BOOST then SLOW to DEF
and AGI, and STUN to MOV (−1) and AGI (−5) where present. Floor each fractional delta separately from its
base, including the packed AGI operand. Equipped effects follow in carried-slot order.
Positive ATT, DEF and MOV effects saturate at 200. Quick Ring adds to the effective AGI low
seven bits, saturates them at 127 and restores the effective bit 7. That is the bit left by
the preceding operations, which may differ from the class's initial extra-turn bit. These
operations preserve stored bases.
[Pinned base-to-current copies](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/updatecombatantstats.asm#L293-L304) ·
[Pinned class load](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/newgame.asm#L111-L126) ·
[Running Pimento effect](../../../docs/research/world/field-item-effects.md#confirmed-static-contract) ·
[Pinned refresh and effect dispatch](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/updatecombatantstats.asm#L10-L164) ·
[Pinned clamp wrappers](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/combatantstats_2.asm#L521-L629) ·
[Controlled refresh and clamp observations](../../../docs/design/contracts/party/level-up.md#confirmed-current-and-derived-stat-refresh) ·
[Packed AGI and timed stats](support-spell-status-design.md#when-the-stats-change)

**Unknown:** the complete ordinary original ring pickup, equip, transfer and SRAM round trip,
and original menu presentation and timing along the selected routes. Source inspection and the
existing controlled refresh evidence are sufficient for selecting these remake rules.

## Remake loadout, actions and stats

- **Authority and eligibility.** Keep the existing four compact `BattleSourceLoadout.Items`
  words and current source-class resolution. One equipped weapon and one equipped ring can occupy
  any two carried slots. Keep separate category lookups, with no inferred weapon from the first
  equipped bit. As with weapons, validate actual worn gear against trusted definitions and the
  current class. Duplicate carried copies are selected by slot and expected word. Reject duplicate
  worn items of a category, worn unsupported items and equipped empty words before publishing.
  Bare supported carry identities continue to travel through existing custody rules. The remake's
  existing normalized broken bit is `0x100`. Static ring admission covers intact words.
- **Equip and unequip.** Show eligible carried slots in pack order with an explicit category
  per choice. Preserve the existing weapon slot-4 **Unarmed** action and four-weapon menu behavior.
  Use a distinct **Remove ring** choice when a ring is worn, including with four eligible ring
  items. The explicit choice disambiguates the source's reused menu cell. Confirm revalidates
  the live caller, expected slot word and category, including the old worn slot on a removal.
  A successful change clears only that category's
  old bit and sets the chosen bit, or only clears it for removal. Preview is detached and cancel
  discards it. A committed change refreshes from the live bases and stored status, even if the
  selected item was already worn. Battle returns to action choice in the same entry. Field
  and Caravan return through their existing menus. Equipment changes spend no resources or RNG.
- **Stat readers.** Use the same finite refresh for equip, removal, existing growth and class
  rebind, and the public stat projections. On a real refresh, apply the admitted status
  operands in source order, then the equipped effects. Preview can calculate the result of
  that refresh without updating the actor. Restore and ordinary reads use the *last applied*
  counter when it differs from stored duration. Keep enemy difficulty-adjusted base ATT and
  existing authored profile and initial stat behavior. Effective ATT and DEF feed damage and
  AI estimates. Effective MOV feeds terrain-cost reachability. Effective numeric AGI and bit 7
  feed the next generated turn queue. An existing queue and cursor remain fixed.
  [Refresh and save operand](support-spell-status-design.md#implementation-decisions)
- **Base MOV in the remake.** Today the actor reads its movement basis from `Definition.Move`.
  Static ring equipment applies movement effects on top of that existing basis. When permanent
  growth gains a stored movement basis, equipment refresh and Continue must use the restored
  member's base MOV, including any legal permanent gain. For example, an admitted gain from 6
  to 8 gives current MOV 10 with Running Ring and 8 after removal, including after cold
  Continue with the ring worn and no STUN. Permanent growth owns the representation and save
  binding. The promotion owner governs class loading.
  [Permanent growth #1943](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1943) ·
  [Current actor movement](../../src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs) ·
  [Promotion class load](../application/church-promotion-design.md#what-changes-on-success)
- **A reached battle position survives a gear change.** Original movement writes the chosen
  coordinates before its action menu. Equip returns to that menu. Back to movement restores the
  turn origin and builds a range again. In the remake, carry the existing validated movement
  preview through the action menu. Use its confirmed stop for action and target queries,
  construction, Give/Discard and their validation even when removing Running Ring lowers MOV.
  Keep normal target and stop-occupancy checks and the existing action publication contract.
  Prefer one internal action view with the acting member at the confirmed stop, based on the
  session-owned selection, so each action can reuse its existing validation. Keep the live
  origin for movement undo and retain the validated path as the return route if the reduced
  budget cannot rebuild it. Back to Movement recalculates reach using current MOV and keeps
  the committed equipment change. Direct movement and AI still require their ordinary budget.
  [Original movement and Back](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/battlefunctions_2.asm#L99-L159) ·
  [Remake movement](../../src/Sf2.Remake.Domain/Battles/Rules/BattleMovement.cs) ·
  [Selection dispatcher](../../src/Sf2.Remake.Application/Runtime/Battles/BattleCommandDispatcher.cs)

## Custody, promotion and Continue

| Operation | Remake contract |
| --- | --- |
| Give, exchange and Drop/Discard | Reuse the current caller-specific selection, capacity, refusal, compaction and Deals rules. Allow a valid equipped ring elsewhere in either pack. Refresh every member whose slot is removed, including along the accepted battle removal seam. Add moved items unequipped and preserve other worn words across shifted slots. Keep Field self-exchange and Caravan self-Give distinct. |
| Depot | Deposit removes the actual donor slot and appends the normalized identity. Retrieve adds it bare. Full-pack exchange validates the outgoing member slot and appends its identity at the tail in the same commit. Read-only Look can use the admitted ring mask for current joined members. Existing capacity, duplicate indices and wait tokens govern the transaction. |
| Promotion | Resolve the accepted destination class and preserve earned values, vitals and book under the rite's existing rules. Refresh the candidate and any distinct special-item holder as required by the rite. The four ALL masks permit retention on both sides. For future MMNK/NINJ, clear the actual weapon's equip bit and leave the ring and its contribution intact. Preserve SORC book handling in its owner. |
| Save/Continue | Encode the same words and depot identities using the existing field and battle formats. Old bare words restore bare with zero ring contribution. Equipped words restore both categories with the resolved class, applied stat operand and unchanged queue/RNG. Verify the existing saved battle ATT/readback against the reconstructed state. Retain existing format, checksum, recovery and unknown-version boundaries. An invalid loadout fails detached restore before replacing live state. |

Stale selection, wrong class, invalid slot, cancellation and failed capacity checks leave the
prior snapshot and custody intact. A failure after an accepted inventory commit follows the
current service publication policy. Keep the settled field and battle-Movement save points.
The existing words and last-refresh operand suffice for Continue.
[Field and Caravan transfer](../application/caravan-services-design.md#current-item-implementation) ·
[Depot](../application/caravan-services-design.md#current-depot-implementation) ·
[Promotion](../application/church-promotion-design.md#what-changes-on-success) ·
[Battle continuation data](../../src/Sf2.Remake.Domain/Battles/State/BattleContinuationData.cs)

## Implementation order and ownership

The four rings use three existing boundaries for content, rules and callers.

1. Bind the four trusted item rows and masks in existing Content admission. Share the binding
   through `PrivateFieldPreparation` and actor loadout construction, including promotion and
   automatic allies. Keep the Witch/Dark Madam's bare carry/drop and current enemy decisions.
   Automatic Peter's equipped Power Ring word 147 must pass ordinary AI loadout admission,
   with the same physical action selection and the refreshed ATT. Reuse mounted names and
   icons where available. [Field preparation](../../src/Sf2.Remake.Content/Scenarios/PrivateFieldPreparation.cs) ·
   [Source actor admission](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)
2. Extend `BattleEquipment` and the actor stat refresh. Wire existing entry/growth/after-turn,
   compacting removal, carried-party queries and detached restore to that calculation. The ATT
   cache, readback guards and effective DEF/AGI/MOV readers must agree with it. Coordinate
   with the support spell owner, keeping the applied-counter timing and authored initial stats.
   Audit all consumers of worn words, especially Give/Discard admission, DETOX cure scene,
   Taros effectiveness and Godot battle-scene weapon projection. A ring in an earlier pack
   slot or as the only worn item keeps weapon presentation correct.
   [Equipment](../../src/Sf2.Remake.Domain/Battles/Rules/BattleEquipment.cs) ·
   [Effective stats](../../src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs) ·
   [Detached restore](../../src/Sf2.Remake.Domain/Battles/State/BattleContinuationRestore.cs)
3. Carry the typed choice and confirmed stop through the existing battle dispatcher and
   Field/Caravan transactions. Update party and battle inspection, equipment labels and
   selected/current projections in the existing English and Simplified Chinese UI.
   Keep field/caller waits, input ownership, and known weapon preview and action flows.
   Amend campaign and capability rows only for the lineage and behavior actually exercised.
   [Field inventory](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldInventory.cs) ·
   [Depot queries](../../src/Sf2.Remake.Application/Runtime/Exploration/CaravanDepotQuery.cs) ·
   [Battle choices](../../src/Sf2.Remake.Application/Runtime/Battles/BattleChoices.cs)

[#1952](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1952) depends on this loadout
and owns item ATTACK/BOOST admission and action, damage to the item, removal, Repair and their
persistence. Quick and Running have no Use spell. [#1926](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1926)
adds the Black Ring's WIZ/SORC gate, curse, BLAZE 2 and treatment with a legally admitted caster.
[#1962](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1962) reuses the ring category
for Chirrup Sandals and owns its pickup, footstep sound and special Depot description.
[Promotion caller dependencies](../application/church-promotion-design.md#remake-decision) ·
[Sandals reach](../content/special-equipment-reach.md)

## Acceptance through Battle26

- **Power. Confirmed** remake receipt and carry. Battle10 Witch 129, once bit 4, awarded Jaha
  item 19. The registered first-Pacalon castle seed carries the same legitimate item on Jaha.
  His WARR class admits Short Axe (33) and Power Ring together. Check both words and ATT +5 in
  Field and the overworld Caravan, then use Relief to deploy him in Battle26. Peter's earlier
  automatic turn is covered with a controlled equipped word in the engine.
  [Battle10 receipt](../application/new-granseal-east-shrine.md) ·
  [Pacalon inputs](../application/pacalon-first-visit.md#continue-after-worktree-cleanup) ·
  [Axe row](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/items/itemdefs.asm#L353-L361)
- **Protect. Confirmed** remake receipt. In the English Battle21 fight Gerhalt defeated Dark
  Madam 129 for item 20 under once bit 8, before defeat. From the registered `issue1854`
  Battle21 input, defeat her before leader cleanup with a free recipient slot, then exercise
  Equip and DEF +5 on the holder at a legal turn or after return. With a full pack the rare
  item can go to Deals. Verify custody in the return before selecting a seed for Continue.
  [Battle21 starts, drop and outcomes](../application/battle21-devils-tail.md#held-items-and-battle-outcomes)
- **Quick. Confirmed** fixed placement at Bedoe Map10 (20,48), F149. The selected map and the
  ordinary Battle12–13 visit provide a starting route. Seek it by ordinary Search from an
  uncollected legal Bedoe continuation with a free slot. Otherwise cover effective AGI and
  next-queue behavior with admitted engine inputs. Actual pickup, holder and item-specific
  Continue remain **Unknown** until observed. [Placement and lifecycle](../content/campaign-items.md#special-equipment-and-attack-items) ·
  [Bedoe visit](../application/dark-caves-bedoe.md)
- **Running. Confirmed** fixed placement at Creed Map33 (7,12), F167. A restored mansion visit
  reached the map. Use a legal uncollected Creed continuation if it reaches the tile, then
  confirm pickup and a move at the extended limit. Otherwise prove the stop and Back behavior
  with a controlled legal engine state. Actual pickup, holder and item-specific Continue
  remain **Unknown** until observed. [Creed placement and limits](../application/creed-restoration.md#reachable-interactions-and-remaining-boundary)

Use one ordinary Power lineage for Equip, cancel/reopen, actual attack and return, Give,
ring removal and Caravan Deposit/Retrieve. Keep a copy with the ring worn for field and
settled battle cold-Continue checks. Check the other language's equipment prompt and a cold restore from
an appropriate legal copy. Protect adds one ordinary receipt and DEF witness. Preserve the
prior result if its receipt route ends in defeat. A Pacalon continuation is useful only after
checking its actual ring custody. Existing actor/weapon scenes can supply a ranged check,
for example the English Higins PLDN/Javelin Battle26 repeat after Relief. Use that pair in an
engine case unless the legal party actually carries a ring to that repeat. The promoted
Bowie HERO route is the existing ordinary class/weapon witness. Test a ring across that
class change with a legal carried item or in an engine case. Future class destinations and
Quick/Running placements do not require a campaign replay for engine-rule acceptance.
[Higins ordinary attack](../application/battle26-higins.md) ·
[HERO class witness](../application/church-promotion-design.md#playable-hero-and-reproduction)

Focused actual-engine cases cover category replacement and removal with mixed slot order,
wrong class/invalid/stale selections, compacting transfer and depot exchange, weapon range
and scene identity with a preceding ring, and automatic Peter. Verify Quick Ring AGI saturation
with the effective high bit, Running Ring's extra-cost stop followed by action and cancel to
the new movement range, and preservation of the existing queue until regeneration. For a
Protect Ring wearer with base DEF 40, stored BOOST 3 and applied BOOST 1, cold Continue shows
50 DEF. The next refresh with the ring produces 60. If that refresh removes it, DEF is 55.
Compare the next strike or AI estimate at the restored DEF. Exercise old bare-word and
equipped word round trips, including changed class, and assert the packed words before and
after. Record which results are controlled engine behavior and which came from ordinary
input. [Existing save and status acceptance](support-spell-status-design.md#serial-ownership-and-acceptance)

Reuse the registered world and scene packages, ordinary legal seed copies and existing Godot
state/input/projection/error readers. The Pacalon world is 101,760,092 bytes. Four definition
bindings add only a few KiB of metadata and should reuse that world. Budget about 16 save
copies at up to 128 KiB each plus a similar amount of compact observations, roughly 4 MiB of
new evidence before build output. Measure the first slot and receipt and revise the estimate
if needed. Rebuild at most one worktree-local selection if a trusted content dependency
actually changes, then reuse it. Keep the source packages and slots read-only, and use a
disposable operator save namespace. For implementation, run the affected engine behavior
cases, necessary adapter compilation, ordinary observations and committed planner, and record
actual CI. Keep any completed shutdown result with its functional outcome and leave
[#1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716) paused.
[Inputs and observation budget](../application/church-promotion-design.md#inputs-commands-and-observation-budget) ·
[Save modes](../application/battle-save-and-continue.md#manual-save-history-and-developer-files)

## Implemented behavior and ordinary input

The engine binds the four static effects from admitted item definitions. `RingEquipmentTests`
covers mixed slot order, Power Ring damage estimation, Protect Ring with retained BOOST operands,
Quick Ring and the next queue, and Running Ring at a confirmed stop after removal.
The actual promotion test preserves a worn ring and weapon across class rebind and growth.
The automatic-ally test accepts a worn Power Ring while choosing an ordinary physical action.

- **Power Ring in Field and Caravan.** Continue the English `castle-en.json` from the registered
  first-Pacalon handoff with the [current world and display package](../application/pacalon-first-visit.md#continue-after-worktree-cleanup).
  Jaha starts with `[161,19,13,0]` and ATT 34. Open the member equipment menu, select Jaha's
  Power Ring, cancel the confirmation, then reopen and confirm. The axe word is still 161 and
  the ring word is 147. ATT is 39. A separate run removed the ring, re-equipped it, then gave
  the worn ring to Kiwi. Jaha kept the axe and ATT 34. Kiwi received bare word 19.
  Chinese equipment input displayed the ring name and projected stats and saved successfully.
- **Power Ring in Battle26.** Continue from the English equipped field save and leave the
  castle and town for Map71. Align with the visible Caravan and open its service. Deposit Jaha's
  worn ring, Look at the stored identity and retrieve it to him. The value is 2,250 gold.
  Retrieval produces `[161,13,0,19]`. Caravan Item Equip sets the last word to 147. Use Join
  and Relief to deploy Jaha in the place of the fallen Kazin, then take the
  [Ketto road to Battle26](../application/pacalon-first-visit.md#reproduction). Jaha's
  axe attack targeted a Lesser Demon at range 1 and was dodged. His ATT was 39. His axe
  was visible in the battle scene with the ring still worn. Save at settled Movement. A fresh Continue and immediate
  re-save preserved the complete battle data. Ordinary EGRESS returned to usable Map36 input.
- **Protect Ring in Battle21.** Continue the English `retry-en.json` from the registered
  [Devil's Tail handoff](../application/battle21-devils-tail.md#held-items-and-battle-outcomes)
  with the current package. Approach from the east, clear the enemies blocking the approach and
  prioritize Dark Madam in slot 129 before the leader. In the observed path Peter defeated her
  in round 10 and received bare word 20 under once bit 8. Save at the next settled Movement.
  On Peter's next turn, confirm the stop, open Equip, select the ring and confirm. He remains
  unarmed at range 1, DEF rises from 35 to 40 and his pack reads `[62,148,127,127]`. Return to
  Movement and save. The worn ring and full battle data survived a fresh Continue and re-save.

The field cold re-save retained all ally records, custody, flags, active party and gold. In the
delayed re-save, wandering entity coordinates and the main seed advanced before the capture.
The direct battle comparisons matched the whole decoded data. Quick and Running use
controlled engine witnesses. Ordinary pickup and item-specific Continue remain **Unknown**
for those two rings.

The Power and Protect post-equip battle slots are about 41 and 37 KiB respectively. A Protect
rehearsal recorded the item receipt, then hit an observer assertion. The Godot process exited
with `0xC0000005`. Its stack showed
`Godot.NativeInterop.NativeFuncs.godotsharp_internal_object_get_associated_gchandle`,
`Godot.GodotObject.Dispose`, and `Godot.GodotObject.Finalize`. The corrected Equip, Save and
cold Continue completed with exit 0. [Paused shutdown issue #1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716)

### Battle Continue after worktree cleanup

Temporarily move the producing `local/` aside in the owning checkout. Capture the operator
save root before loading the worktree configuration. Restore `local/private-inputs.ps1` from
the registered template. Run the Pacalon setup block through its `Copy-Item` of the registered
drivers into `./local`. This selects the stable `issue1966` world and scenes, the `issue1941`
display catalogs, and `issue1927` controller settings. Reuse the built Debug adapter.
[Fresh worktree setup](../../../docs/operations/local-private-inputs.md#fresh-worktree-setup) ·
[Pacalon setup](../application/pacalon-first-visit.md#continue-after-worktree-cleanup)

Use the retained external English Battle26 slot from the ordinary Power Ring route above.
The observer's `battle-continue` mode uses the title's Continue slot picker and re-saves from
settled Movement. This scratch copy adds two ordinary Enter inputs, one to dismiss the save
result and one to open Action Choice at the current stop.

```powershell
# Capture this before loading the worktree configuration.
$operatorData = [Environment]::GetFolderPath('LocalApplicationData')
# Run the linked configuration and Pacalon setup through the driver copy here.
$seed = Join-Path $operatorData 'Sf2RingReview/issue1946/battlecold01/ring-review/sf2-opening/slot1.json'
$sampleRoot = Join-Path $operatorData ('Sf2RingReview/issue1946/clean-' + [guid]::NewGuid().ToString('N'))
$receipt = (New-Item -ItemType Directory -Path './local/issue1946-clean').FullName
$driver = Get-Content -LiteralPath "$pacalon/Pacalon.gd" -Raw
$anchor = ' write_json("result", {"issue": issue, "texts": text_ids})'
$check = @'
 if mode == "battle-continue" and issue == "":
  await key(KEY_ENTER)
  await settle("post-save")
  var start = state()
  await key(KEY_ENTER)
  await ticks(6)
  var next = state()
  if start.get("stage") != "Movement" or next.get("stage") != "ActionChoice" or start.get("actor") != next.get("actor") or next.get("failure") != null:
   issue = "ordinary input after settled re-save"
  write_json("post-save-input", {"actor": next.get("actor"), "before": start.get("stage"), "after": next.get("stage"), "failure": next.get("failure")})
'@
if ([regex]::Matches($driver, [regex]::Escape($anchor)).Count -ne 1) { throw 'Pacalon driver result boundary changed' }
Set-Content -LiteralPath "$receipt/Continue.gd" -Value ($driver.Replace($anchor, $check + [char]10 + $anchor)) -Encoding utf8
$env:SF2_DESKTOP_MODE = 'battle-continue'
$env:SF2_REVIEW_LANGUAGE = 'en'
$env:SF2_ROUTE_DIR = $receipt
$env:SF2_SAVE_ROOT = $sampleRoot
$env:SF2_SAVE_NAMESPACE = 'ring-clean-review'
$env:SF2_REVIEW_SLOT_PATH = Join-Path $sampleRoot 'ring-clean-review/sf2-opening/slot1.json'
$env:APPDATA = Join-Path $receipt 'appdata'
$env:LOCALAPPDATA = Join-Path $receipt 'localappdata'
$env:TEMP = Join-Path $receipt 'temp'
$env:TMP = $env:TEMP
foreach ($dir in @($env:APPDATA, $env:LOCALAPPDATA, $env:TEMP, [IO.Path]::GetDirectoryName($env:SF2_REVIEW_SLOT_PATH))) {
    [void][IO.Directory]::CreateDirectory($dir)
}
[IO.File]::Copy($seed, $env:SF2_REVIEW_SLOT_PATH, $false)
$game = (Resolve-Path -LiteralPath './remake/game').Path
Push-Location -LiteralPath $game
try {
    & $env:GODOT_BIN --headless --fixed-fps 60 --audio-driver Dummy --resolution 1920x1080 `
        --path $game --script "$receipt/Continue.gd" --log-file "$receipt/native.log" `
        -- --input-settings "$pacalon/settings-en.json"
    $nativeExit = $LASTEXITCODE
} finally { Pop-Location }
if ($nativeExit -ne 0) { throw 'Godot ordinary Continue check failed' }
```

Compare the entire decoded `data` object and verify that the record sequence increments once.
Check the worn words and ATT explicitly and read the next input's observed stage and failure.

```powershell
$env:SF2_RING_REVIEW_SEED = $seed
@'
import base64, json, os
from pathlib import Path

def load_record(path):
    envelope = json.loads(path.read_text(encoding="utf-8-sig"))
    return json.loads(base64.b64decode(envelope["payload"]))

receipt = Path(os.environ["SF2_ROUTE_DIR"])
before = load_record(Path(os.environ["SF2_RING_REVIEW_SEED"]))
after = load_record(receipt / "immediate-resave-slot.json")
assert after["sequence"] == before["sequence"] + 1
assert before["data"] == after["data"]
battle = after["data"]["battle"]
assert battle["encounter"] == "battle-26"
jaha = next(actor for actor in battle["actors"] if actor["actorId"] == "ally-3")
assert jaha["loadout"]["value"]["items"] == [161, 13, 0, 147] and jaha["attack"] == 39
entry = json.loads((receipt / "post-save-input.json").read_text(encoding="utf-8"))
assert entry["before"] == "Movement" and entry["after"] == "ActionChoice" and entry["failure"] is None
print(f"PASS sequence {before['sequence']} -> {after['sequence']}, complete battle data equal")
print(f"PASS {battle['encounter']} Jaha attack {jaha['attack']} and both equipped words, input {entry['actor']} {entry['before']} -> {entry['after']}")
'@ | python -X utf8 -
if ($LASTEXITCODE -ne 0) { throw 'Decoded battle or input comparison failed' }
```

With the producing `local/` parked, Godot 4.7.2 Mono continued the Battle26 slot. Sequence 8
advanced to 9 with identical complete battle data. Jaha retained `[161,13,0,147]` and ATT 39.
Bowie (`ally-0`) advanced from Movement to Action Choice on the next input with no projected
failure. Godot exited 0. The source slot and stable content remained read-only.
