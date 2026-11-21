# DETOX 2 and 3 through first Pacalon

**Proposed design, [#1965](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1965).**
Standard rigor covers a two-caller change with shared status, equipment, presentation and save
dependencies. Deliver learned player DETOX 2 and 3 through ordinary Field and Battle Magic.
Retain the current campaign boundary at first Pacalon and Battle26. Field, battle and cure from
an item each have their own admission, payment and feedback. The design uses the existing party,
battle and reaction owners. DETOX 4, enemy casting, new ailment producers and progression past
Battle26 are outside this agreement.
[Magic and support](../../../docs/design/synthesis/themes/tactical-battle-loop.md#magic-healing-and-support) ·
[Action commit and replay](../../../docs/design/synthesis/themes/tactical-battle-loop.md#action-commit-construction-and-replay) ·
[Campaign agreement](../application/remaining-campaign-risk-baseline.md)

## Original rules and evidence

**Confirmed** static at the [USA retail baseline](../../../manifests/roms/sf2-us.json) and
[SF2DISASM c834c652](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The paths below are relative to `disasm/`. Read them from the registered source checkout with
`git show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/<path>`. The distinct status bits
are STUN `0x0001` (also called paralysis), POISON `0x0002` and CURSE `0x0004`.

| Learned cast | Packed entry / internal rank | MP | Battle center / radius | Bits eligible for clearing |
| --- | --- | ---: | --- | --- |
| DETOX 1 | `0x02` / 0 | 3 | Teammate at distance 0–1 / 0 | POISON |
| DETOX 2 | `0x42` / 1 | 6 | Teammate at distance 0–2 / 0 | STUN, POISON (`0x0003`) |
| DETOX 3 | `0x82` / 2 | 10 | Teammate at distance 0–2 / 0 | CURSE, STUN, POISON (`0x0007`) |

The rank rows in `data/stats/spells/spelldefs.asm` use POWER 0 and
TYPE_HEAL|TARGET_TEAMMATES|AFFECTEDBYSILENCE. Battle uses animation DETOX with variations 2 and 3.
The masks above apply to both Magic callers. Cure result flags in battle use a different encoding
(POISON 1, STUN 2, CURSE 4). Compute the intersection with the target's live word so a partial cure
preserves the bits outside that rank's mask and every unrelated accepted effect.
[Spell definition contract](../../../docs/design/contracts/party/spell-definition-data.md#fixed-eight-byte-record) ·
[Battle effect contract](../../../docs/design/contracts/battle/spell-resolution.md#confirmed-detox-status-clearing-subset)

### Field Magic

**Confirmed** static in `code/common/menus/main/mainactions.asm:FieldMenu`, its
`PopulateGenericListWithCurrentForceMembers`, `code/common/stats/battleparty.asm:UpdateForce`
and `code/common/menus/memberslistscreen.asm:ExecuteExplorationMagicMenu` and
`ValidateMembersListScreenConfirmationInput`. Magic lists joined members, including reserves.
The caster needs a learned book entry. The selector starts at the highest learned rank and allows
lower ranks. The same joined list supplies the target, including self, reserve or HP-zero members.
The DETOX prompt uses the item summary page with Use and the empty item sentinel. Neither selector
checks HP, status, silence, MP sufficiency or map distance. Canceling the recipient returns to
Magic selection before payment or target writes.

On confirmation, announce the selected cast, debit its MP with floor zero
(`code/common/stats/combatantstats_2.asm:DecreaseCurrentMp`), then inspect the recipient. Rank 3
processes and reports CURSE, then STUN, then POISON. Rank 2 starts at STUN. Each message corresponds
to a bit actually removed. If the intersection is empty, report nothing happened. The field
caller writes the resulting status and calls `UpdateCombatantStats` even on that branch. There is
no EXP or RNG call on either result. HP stays where it was, including zero. Control returns to the
field menu, from which the player resumes walking. The cast does not itself save.
[Pinned Field caller](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/main/mainactions.asm#L61-L188) ·
[Remake Field policy](../application/opening-session-lifecycle.md#implemented-field-preparation)

### Battle Magic

**Confirmed** static. The Magic branch in `code/gameflow/battle/battlefunctions/battlefunctions_2.asm`
compares live MP with the selected packed rank's cost before target selection. From the caster's
chosen square, `battlefield/buildactionrangegrids.asm:BuildSpellRangeGrid` uses Manhattan range and
teammate occupancy. `battlefield/populatetargetslist.asm:PopulateTargetsListForSpell` expands the
center by radius zero. Both use the occupancy filter in
`battlefield/buildtargetsarray.asm:BuildTargetsArray`, which skips the neutral activation bit
even for an ally-side combatant. Select one living, positioned, non-neutral ally, including the
caster at distance zero. Fallen, undeployed or reserve members have no battle target. Need for
a cure does not gate targeting. Cancel before confirmation with both seeds, MP, position
commitment and turn unchanged. Insufficient MP returns to the spell selector.

In `battleactions/initbattlesceneproperties.asm`, the base spell supplies the silence property.
`battleactions/createbattlesceneanimation.asm:battlesceneScript_PerformAnimation` emits one caster
MP reaction using the selected rank and caster's pre-effect status. A silenced cast still pays,
shows suppression and skips target processing and reward. Otherwise
`battleactions/castspell.asm:spellEffect_Detox` constructs the following results.

| Live target at effect construction | Effect and messages | Effect draw and EXP contribution |
| --- | --- | --- |
| At least one eligible bit | One zero-HP/MP status reaction carrying the full post-cure word with flag 2. Emit messages for removed bits in POISON → STUN → CURSE order (301, 302, 303). | No effectiveness or resistance draw. Add 5 once for an ally caster, even if three bits clear. |
| No eligible bit, including rank 2 on CURSE alone | Threshold 8 always fails for a normal draw in [0,7]. Emit message 295, unwind the effect and skip target reaction, status write and refresh. | One main RNG draw at range 8. Contribution 0. |

The effect contribution precedes the end-of-action award. For a living ally who completed the
cast, `battleactions/battleactionsengine_2.asm:battlesceneScript_End` calls the common reward owner
even on ineffective. `battleactions/giveexpandgold.asm:battlesceneScript_GiveExpAndGold` skips the
halving table on the same-side path and uses two range-16 main draws (first zero adds one, second
zero subtracts one), minimum 1. With a single friendly target this yields a 4–6 EXP award on
success or 1 on no effect, before any level-up. There is no gold, HP gain, kill or item reward
from this spell. Separate growth and after-turn draws from effect and award draws. There is no cure
draw on a cancel or a suppressed spell. End the action and run the existing caster after-turn
and next-control flow. A cured ally does not take an extra turn or rebuild the current queue.
[Pinned DETOX effect](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/castspell.asm#L123-L198) ·
[Pinned EXP award](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/giveexpandgold.asm#L32-L73) ·
[After-turn contract](../../../docs/design/contracts/battle/spell-resolution.md#confirmed-after-turn-counter-transition-matrix)

**Confirmed** at the controlled original Battle01 effect seam. The tracked
[DETOX fixture](../../../tests/fixtures/h3/spell-detox-v1.json) supplies the target and observes
`spellEffect_Detox` at ROM 45460 through return 45690 or the ineffective unwind. With status
`0x30C7` and worn Black Ring word 152, rank 2 leaves `0x30C4` and 152. Rank 3 leaves `0x30C0`
and bare word 24. Both contribute 5. The rank-1 no-poison control exercises the shared ineffective
branch. `uv run sf2 h3 spell-detox` is the existing reproduction command (BizHawk 2.11.1 /
Genesis Plus GX). This fixture establishes construction effects, while the ordinary Sarah
selector, completed DETOX 2/3 playback and original SRAM round trip remain **Unknown**.
[Research observation](../../../docs/research/battle/runtime-rng-and-battle-math.md#confirmed-detox-level-masks-ineffective-branch-and-curse-unequip)

### Gear, current stats and persistence

**Confirmed** static. On curse removal both callers invoke
`code/common/stats/itemstats.asm:UnequipAllItemsIfNotCursed`. The routine scans all four held
words. It checks the equipped bit, skips the empty sentinel and tests CURSED on the definition.
It clears the equipped bit on each matching word in place. A regular weapon or ring remains worn
in its existing slot. A bare cursed copy stays bare, and item IDs, damage bits, slot positions and
custody survive. Apply the same scan to the two independently wearable categories. There is no
generic Remove, drop, break roll, repair, inventory compaction or extra charge.

The helper tail-calls `UpdateCombatantStats` while the cleared word is still local to the caller.
That first refresh uses the then-stored status and the changed gear. Both callers subsequently
write the post-cure word and refresh again. Field reports the curse before calling the helper and
then processes stun and poison. Battle constructs the reaction and all removal messages before
calling the helper, writes and refreshes at effect completion, and the reaction handler in
`battlescenes/battlesceneengine_0.asm` also writes its complete status operand and refreshes on
playback. The caster MP reaction applies the caster's pre-effect word and refreshes independently.
For a self-cure, payment must precede the persistent cure reaction so that the cost can never
restore the removed bits. On ineffective Battle DETOX against another actor, there is no target
refresh. The caster's own cost, growth or after-turn can still refresh the caster.
[Original unequip helper](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/itemstats.asm#L945-L971) ·
[Support refresh contract](support-spell-status-design.md#implementation-decisions)

`code/common/stats/updatecombatantstats.asm:UpdateCombatantStats` initializes current stats
from bases, applies ATTACK, BOOST, SLOW and then STUN (MOV −1 and AGI −5, subject to normal
clamping), then applies equipped effects. CURSE is derived again from worn cursed definitions.
Removing STUN restores its MOV/AGI contribution without discarding equipped or timed effects.
The cure mask never clears unrelated durations. An effective reaction refreshes from those
durations, so an existing BOOST/SLOW divergence between stored duration and last-applied operand
can end there. Ineffective against a different target preserves that target's divergence.
Disentangle these operations from ordinary caster after-turn, where durations can advance.

Original `code/common/tech/sram/sramfunctions.asm:SaveGame/LoadGame` copies the 4,016-byte
slot beginning at COMBATANT_DATA. The stat records carry the current values, packed status,
spells and gear. Suspended battle Continue takes its existing resume path. At the callers that
run `battleloop/heallivingandimmortalallies.asm:HealLivingAndImmortalAllies`, living and immortal
allies refill HP/MP, keep remaining STUN/POISON/CURSE, and refresh with gear. This reset strips
battle-only counters and does not reinstate a cured bit or re-equip a removed curse. Follow the
actual victory, defeat and EGRESS callers, including their casualty eligibility, before comparing
MP on return. These static paths establish the data dependency, while a complete original
DETOX 2/3 Save → Continue → return observation remains **Unknown**.
[Original save system](../../../docs/design/contracts/system/save-system.md) ·
[Battle26 return](../../../docs/research/chapters/northern-travel-mitula/battle26-outcomes-and-higins-recruitment.md)

## Current remake reach and dependencies

**Confirmed** in the accepted checkout and registered read-only saves. The pinned
`data/stats/allies/stats/allystats01.asm` teaches PRST Sarah DETOX 1/2/3 at 4/13/33. The two
`issue1955/slots/castle-*.json` Map36 saves both contain active PRST40 Sarah with packed byte
130, the learned ceiling for ranks 1–3. Her MP is 59 in English and 56 in Chinese. The Chinese
Battle26 road slot also has the packed entry, 56 MP and a placed Sarah. Only rank 1 currently
binds in `PrivateFieldPreparation` and `PrivateBattleActionBindings.PoisonCure`. To reproduce,
decode each slot envelope's base64 payload and verify its SHA-256 against the envelope. Read
`data.allies` by `actor` and `data.activeAllies`, or `data.battle.actors` by `actorId`
and its position and loadout. In both forms use `ally-1` for Sarah. Rank admission is keyed by
the current book and the selected source record, independent of the actor's name or chapter.
VICR/MMNK borrow Sarah's first spell list in the source. Their broader class/scene admission
belongs to the promotion owners.
[Sarah's growth and campaign consumer](../content/campaign-characters.md) ·
[Private field binding](../../src/Sf2.Remake.Content/Scenarios/PrivateFieldPreparation.cs) ·
[Private battle binding](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs)

| Current source of state | Consequence for this slice |
| --- | --- |
| Connected physical poison and STUN | Actual earlier remake poison survives battle return. Harpy poison and Soulsower stun are source physical operands in supported encounters. Existing battle and field status readers already consume these bits, including STUN AGI/MOV and after-turn. Verify an admitted target in the selected seed rather than assuming the printed enemy class implies a live ailment. |
| Accepted SLOW/BOOST and physical SLEEP, plus admitted silence | Use the existing packed word and applied-stat operand. DETOX preserves these effects. It neither replaces their source nor expands mixed-state admission beyond the affected cure, refresh and continuation. |
| Black Ring [#1926](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1926) | Battle26 drops and carries bare item 24. WIZ/SORC and equipped curse are required for the ordinary curse witness. The retained Pacalon parties supply neither eligible class. #1926 owns curse equipment legality, normalization and actual affected action/return/save consumers. Static weapon/ring separation is already accepted. |
| Equipment Use/Repair [#1952](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1952) | Rank 3 may encounter a damaged worn word once that lifecycle is admitted. Preserve the bit and share equipment refresh and word validation. DETOX does not require item Use/Repair to ship on admitted uncursed states. Protect Ring's BOOST and Power Ring's ATTACK item callers stay with their owners. |
| Hostile spells [#1967](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1967) | Player MUDDLE, DESOUL and Tyrin's spell SLEEP are separate consumers. Their implementation is unnecessary for clearing STUN, POISON or CURSE. When admitted, preserve MUDDLE/SLEEP according to their owning status rules and keep fallen battle actors outside target selection. |

[Harpy Pond poison source](../../../docs/research/chapters/southern-travel-creed/battle20-initialization-and-first-control.md) ·
[Soulsower STUN](../application/hassan-taros-road.md#observed-in-the-running-remake) ·
[Ring slots and custody](ring-equipment-design.md#custody-promotion-and-continue) ·
[Use and Repair](equipment-use-repair-design.md#damage-destruction-and-custody)

Field/Caravan Fairy Powder (item 5) clears STUN and POISON using the item caller's result
and custody. Battle POWDER rank 1 dispatches the poison-only effect. ANTIDOTE and learned DETOX 1
also clear poison only. The shared finite mask calculation can serve these callers with their
actual terms. Field Powder supplies no evidence of high-rank Magic targeting or payment.
[Fairy Powder callers](../application/pacalon-first-visit.md#fairy-powder-use) ·
[Accepted DETOX 1](gameplay-rules.md#finite-purchased-supplies-and-detox1)

## Implementation decision and sequence

1. **Connect both Magic ranks for admitted uncursed states.** Reuse `FieldCast`, the joined-party
   query, the existing friendly battle spell and scene pipeline, and independently validated
   source rows. Extend the finite cure rule with explicit allowed and removed bits. Preserve the
   existing rank-1 and item masks, costs and custody. Test learn ceilings, usable rank/cost/range,
   valid no-effect commits and rejected input through the same query/confirmation owner.
   `HealingTargetRules` currently checks faction, HP and position for the shared single-target
   query and confirmation, while the neutral-bit check is specific to AURA centers. Apply the
   source occupancy guard in the shared target owner so both Magic and item Use exclude a
   positioned ally marked neutral. Preserve AURA's separate area membership rules. For unsupported
   cursed battle states or gear, reject on the existing explicit admission boundary before
   payment. Rank 3 supports its verified STUN/POISON subset on currently admitted targets.
   Full rank-3 completion remains open.
2. **Publish caller-specific reactions.** Field commits payment, recipient and refreshed party
   record once, including self-target payment and no-effect refresh. Add the actual ordered
   removal result to the existing field observation so the view can format recipient notices.
   Battle uses one successful target reaction carrying the full word and removed bits, with
   distinct caster cost, ordered cure text, award and after-turn. Reuse the same reaction
   validation and refresh boundary as the current poison cure. Preserve any old applied operand
   until its own real refresh. The pinned award skips the halving table for a same-side target.
   The current `Sf2BattleProgressionRule` applies the halving flag to Healing and Item status
   awards without checking the target's side. This observed source/implementation difference
   brings the shared award correction into #1965. Gate halving on the actual target's faction
   relative to the recipient for these status actions. DETOX 1/2/3, Antidote and battle Fairy
   Powder use the same rule, with the existing 5/0 contribution, two range-16 award draws and
   minimum 1 EXP.
3. **Complete the equipped-curse branch with #1926.** Once shared custody, refresh and return/save
   semantics admit a worn cursed word, use the equipment operation to clear the equipped bit only on
   cursed definitions. Check the status/gear transition on the detached action before
   publication. Carry the resulting gear and status into the existing refresh and reaction;
   preserve the source ordering if an observable consumer reads between refreshes. Use one
   atomic final gear/status result in Field, and the existing staged cost and target reaction in
   Battle. No second gear authority or persistent intermediate state. When there is no curse
   removal, leave worn words alone. A real supported cursed target with no rank-eligible bits
   follows the normal no-effect branch.

The first two steps form one playable slice. The third can land with the #1926 consumer or in
a short dependent slice. Serialize the shared reaction, save, localization and campaign records
with [#1944](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1944), #1952 and #1967.
A finite direct engine case can express the original curse effect before natural availability.
Content supplies rank terms; Domain owns the effect, target admission and result validation;
Application publishes existing waits and party changes; Godot displays localized semantics and
the admitted DETOX cast assets. Render the chosen rank and MP in both menus and reuse ordinary
DETOX actor/scenery bindings. HEAL fairy admission applies to HP healing.
[Field recovery](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldRecovery.cs) ·
[Battle cure](../../src/Sf2.Remake.Domain/Gameplay/Sf2/PoisonCureRules.cs) ·
[Battle EXP award](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2BattleProgressionRule.cs) ·
[Friendly target admission](../../src/Sf2.Remake.Domain/Gameplay/Sf2/HealingTargetRules.cs) ·
[Battle reaction and payment](../../src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs) ·
[Battle action scene](../../src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs)

Preserve the existing settled field and player Movement save boundaries. The record already
carries status, raw words, book, resources and both seeds, plus the conditional battle v5
last-refresh operand and current queue/cursor. Restore detached, validate the actor so gear and
status agree in the admitted curse model, and compare before publishing. Recompute projection as
the existing save format requires, without triggering a new gameplay refresh. Keep legacy field
and battle reads. New-rank casts alone need no version change. If #1926 finds an actual persistence
gap for a worn cursed word, resolve it in that admission slice. Cast effects, MP payment and
settled return must survive cold Continue without replaying the cure or making a second draw.
[Save and Continue agreement](../application/battle-save-and-continue.md#stable-commit-and-request-ux) ·
[Support save rules](support-spell-status-design.md#save-compatibility-and-recovery)

## Acceptance and input budget

Use focused engine behavior cases at actual Field/Battle entry points. Vary rank, source actor,
target and resource state within those boundaries.

- **Field.** Cover self, another active ally, joined reserve, fallen caster/target with a learned
  book, and unjoined or stale selection. Verify low-MP clamping, no MP on cancel, no distance or
  silence gate, one paid no-effect refresh and ordered partial cure feedback. Assert no change
  to HP, EXP or either seed. Confirm gear/stat changes and walking control before cold field Save.
- **Battle.** Cover range 0 self from a moved destination, range 2 and out-of-range 3, opposite
  side, neutral, fallen and undeployed. Verify insufficient MP and cancellation before commit.
  Separate silenced payment, paid no effect (including range-8 and both award draws), and effective
  cure (no range-8 draw). Compare mana, full target word, unchanged HP, removed bits, status
  contribution and final EXP. Check selective removal and timed-stat refresh with simultaneous
  STUN/POISON and BOOST/SLOW, including a non-caster with a failed-recast operand. Exercise the
  next legal movement or strike and next generated round. Test self-payment before cure and the
  distinction between the cured target and the caster's normal after-turn.
- **Shared battle regressions.** In a halved-EXP encounter, use DETOX 1, Antidote and battle Fairy
  Powder on a poisoned teammate with a seed that makes both award draws nonzero. Assert the
  applied cure and 5 final EXP for each caller, plus both range-16 draws and the resulting seed.
  Cover the clean-target minimum of 1 EXP and its preceding range-8 draw. Verify the shared
  target query and confirmation exclude a living, positioned ally with the neutral activation
  bit for Magic and item Use before any payment or draw. These cases use existing engine action
  entry points within the acceptance budget below.
- **Curse and storage.** At the equipped-curse boundary use a known class-legal target with an
  ordinary weapon and worn cursed ring. Check rank 2 leaves the ring worn and rank 3 clears only
  the cursed equipped bit. Vary bare duplicates, slot position, damage and mixed status.
  Compare effective ATT/DEF/AGI/MOV, custody and partial/no-effect refresh behavior. Exercise
  current cold field/battle reads and an older bare-word slot. Save after the staged actions
  settle, cold Continue and compare the complete decoded gameplay records with uninterrupted
  control. Verify the actual return rule, including MP debit before refill, remaining lasting
  status and persistent pack. Use ordinary paid EGRESS for the Battle26 return witness.

**Shortest ordinary input plan.** Fork the stable seeds into disposable writable slots.

- For effective Field Magic, use the registered `issue1847-formation/slots/rescued-en.json`.
  Its checksum matches. Map51 has active PRST37 Sarah, 53 MP and learned ceiling 130. Bowie
  and Sarah are both living active and poisoned. Cast the two ranks on separate targets
  (including self), cancel one selection first, then attempt a paid clean-target cast and
  save at settled field input. Use either Pacalon castle slot for the current Map36 Magic
  projection and clean-target branch. [Harpy Pond return and save](../application/battle20-elric.md#leave-return-and-win)
- For Battle26 use `issue1955/receipts/zh-CN-road01/battle-ready-slot.json` with the stable
  Pacalon resources. All actors in this slot have zero cure bits. Use the first playable Sarah
  turn, move and cancel, then cast an actual selected rank on a clean legal ally and cold
  Continue. A second suffix from the same source slot can exercise the other rank without
  waiting for a second Sarah turn. Record the no-effect spell, debit and action award.
  Continue to the live Map36 return via Bowie’s paid EGRESS. [Battle26 seed and return route](support-spell-status-design.md#battle26-ordinary-input-and-reproduction)
- For a possible effective battle witness, prefer the already admitted
  `issue1847-formation/slots/repeat19-zh-CN.json`. The verified save begins at Sarah's first
  movement input, has ceiling 130 and 53 MP, and uses the original Soulsower roster at Bowie's
  level 24. Play until a surviving ally has STUN and Sarah can target that ally at her next turn.
  Stop after the first effective cast or four rounds. Availability within this suffix is
  **Unknown**. If needed, use the controlled engine cases for effective and mixed results and
  record the live Battle26 observation as paid no effect. A curse witness awaits the legally
  equipped #1926 caller. No save edits stand in for ordinary infliction.
  [Accepted repeat seed](../application/battle20-elric.md#return-to-battle19-with-elric) ·
  [Original Soulsower prowess](../../../docs/research/chapters/southern-travel-creed/battle19-initialization-and-first-control.md)

The read-only sample slots are 6–49 KiB. Allow up to 12 copied/generated slots at 128 KiB each
and 2 MiB of compact state, input, projection and error receipts (under 4 MiB together).
Cap ordinary observation at the two main suffixes plus the bounded Soulsower attempt, with one
serial debug owner and fresh processes only where cold Continue requires them. Reuse the stable
Pacalon world (~102 MB) and the registered earlier packages in place. Budget zero generated
world/encounter copies and reuse mounted icons and localized catalogs. Check identity, older recipe
compatibility, assets and slot sizes in the first preflight. If a needed resource is missing,
size the smallest affected resource update before generating it. Inspect running state and errors
without screenshots or repeated campaign replay. Record process exit separately from gameplay
and save receipts. Record any completed shutdown failure under the existing [#1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716)
boundary.

For implementation, run `uv run sf2 verify engine` and `uv run sf2 verify adapter` where the
presentation changes. Record the supported ranks and ordinary or controlled results in
[gameplay rules](gameplay-rules.md), [campaign characters](../content/campaign-characters.md)
and [capability status](../capability-status.md) when the implementation lands. This document
uses direct source, content, scope and link checks and the docs-only CI path.
