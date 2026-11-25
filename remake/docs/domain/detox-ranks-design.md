# Learned DETOX through first Pacalon

Standard rigor covers the Field and Battle callers and their shared status, equipment, presentation
and save dependencies. Learned player DETOX is admitted through first Pacalon and Battle26.
Field, Battle and item cure have caller-specific admission, payment and feedback. The existing
party, battle and reaction owners handle each result. Enemy casting and new ailment producers
are outside this agreement.
[Rank 2/3](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1965) ·
[Rank 4](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2236)
[Magic and support](../../../docs/design/synthesis/themes/tactical-battle-loop.md#magic-healing-and-support) ·
[Action commit and replay](../../../docs/design/synthesis/themes/tactical-battle-loop.md#action-commit-construction-and-replay) ·
[Campaign agreement](../application/remaining-campaign-risk-baseline.md) ·
[Battle art and controlled playback](../godot/spell-presentation.md#prepared-detox-presentation)

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
| DETOX 4 | `0xC2` / 3 | 16 | Teammate at distance 0–3 / 2 | CURSE, STUN, POISON (`0x0007`) |

The rank rows in `data/stats/spells/spelldefs.asm` use POWER 0 and
TYPE_HEAL|TARGET_TEAMMATES|AFFECTEDBYSILENCE. Battle uses the four variations of the shared DETOX animation.
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
(`code/common/stats/combatantstats_2.asm:DecreaseCurrentMp`), then inspect the recipient. Ranks 3 and 4
process and report CURSE, then STUN, then POISON. Rank 2 starts at STUN. Each message corresponds
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
center by the selected rank's radius. Both use the occupancy filter in
`battlefield/buildtargetsarray.asm:BuildTargetsArray`, which skips the neutral activation bit
even for an ally-side combatant. Select a living, positioned, non-neutral ally as the center,
including the caster at distance zero. Fallen, undeployed or reserve members have no battle target. Need for
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

**Confirmed** in the remake and registered read-only saves. The pinned
`data/stats/allies/stats/allystats01.asm` teaches PRST Sarah DETOX 1/2/3 at 4/13/33. The two
`issue1955/slots/castle-*.json` Map36 saves both contain active PRST40 Sarah with packed byte
130, the learned ceiling for ranks 1–3. Her MP is 59 in English and 56 in Chinese. The Chinese
Battle26 road slot also has the packed entry, 56 MP and a placed Sarah.
`PrivateFieldPreparation` and `PrivateBattleActionBindings.PoisonCure` bind all four ranks. Rank 2 clears STUN and POISON. Ranks 3 and 4 also treat admitted CURSE
and unequip cursed gear. To inspect the accepted rank 2 and 3 seeds,
decode each slot envelope's base64 payload and verify its SHA-256 against the envelope. Read
`data.allies` by `actor` and `data.activeAllies`, or `data.battle.actors` by `actorId`
and its position and loadout. In both forms use `ally-1` for Sarah. Rank admission is keyed by
the current book and the selected source record, independent of the actor's name or chapter.
VICR/MMNK borrow each member's first spell list in the source. Both current priest members
now have class/scene admission through the [promotion owner](../application/church-promotion-design.md#master-monk-promotion); learned rank and target checks remain shared.
[Sarah's growth and campaign consumer](../content/campaign-characters.md) ·
[Private field binding](../../src/Sf2.Remake.Content/Scenarios/PrivateFieldPreparation.cs) ·
[Private battle binding](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs)

| Current source of state | Consequence for this slice |
| --- | --- |
| Connected physical poison and STUN | Actual earlier remake poison survives battle return. Harpy poison and Soulsower stun are source physical operands in supported encounters. Existing battle and field status readers already consume these bits, including STUN AGI/MOV and after-turn. Verify an admitted target in the selected seed rather than assuming the printed enemy class implies a live ailment. |
| Accepted SLOW/BOOST and physical SLEEP, plus admitted silence | Use the existing packed word and applied-stat operand. DETOX preserves these effects. It neither replaces their source nor expands mixed-state admission beyond the affected cure, refresh and continuation. |
| Black Ring [#1926](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1926) | Battle26 drops bare item 24. A controlled legally promoted WIZ exercises worn CURSE. DETOX 3 uses `BattleEquipment.RemoveCurse(definition, loadout)`, commits the pack with the status write, and refreshes from retained gear. DETOX 2 preserves the pack. [Worn equipment contract](ring-equipment-design.md#worn-equipment-and-treatment). |
| Equipment Use/Repair [#1952](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1952) | Rank 3 preserves the damage bit on a worn word and shares equipment refresh and word validation. Protect Ring's BOOST and Power Ring's ATTACK item callers stay with their owners. |
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

Ranks 2 and 3 use the following shared cure and persistence decisions.

1. **Connect both Magic ranks.** Reuse `FieldCast`, the joined-party
   query, the existing friendly battle spell and scene pipeline, and independently validated
   source rows. Extend the finite cure rule with explicit allowed and removed bits. Preserve the
   existing rank-1 and item masks, costs and custody. Test learn ceilings, usable rank/cost/range,
   valid no-effect commits and rejected input through the same query/confirmation owner.
   The accepted [recovery item implementation](recovery-items-design.md) put the source occupancy
   guard in `HealingTargetRules`. Magic and item Use share it, and AURA has its separate area
   membership rules. Existing equipment admission still rejects unsupported worn gear.
2. **Publish caller-specific reactions.** Field commits payment, recipient and refreshed party
   record once, including self-target payment and no-effect refresh. Add the actual ordered
   removal result to the existing field observation so the view can format recipient notices.
   Battle uses one successful target reaction carrying the full word and removed bits, with
   distinct caster cost, ordered cure text, award and after-turn. Reuse the same reaction
   validation and refresh boundary as the current poison cure. Preserve any old applied operand
   until its own real refresh. The accepted recovery item implementation also corrected the
   Healing and Item status award in `Sf2BattleProgressionRule` to check the actual target's side.
   DETOX 1/2/3, Antidote and battle Fairy Powder share that calculation, with the 5/0 contribution,
   two range-16 award draws and minimum 1 EXP. The same refresh reads the stored ATTACK and BOOST
   counters when a reaction succeeds. A paid ineffective cast preserves the recipient's existing
   last-refresh operand when the recipient is distinct from the caster.
3. **Treat equipped curse through the existing equipment owner.** Field follows the Church
   treatment path by replacing the pack, refreshing status from the remaining gear, and publishing
   the result with payment. Battle validates the same detached cure reaction as the other ranks.
   Reaction replay applies the pack change to the live recipient after the spell debit, computes
   current ATT from the retained gear and stored counter, and refreshes the other stats from that
   same pack and status. This also preserves a self-target's paid MP. Only the affected caller needed
   a change to replay. The shared `BattleEquipment.RemoveCurse` and `RefreshCurse` rules already own
   worn-curse detection and word edits. Both callers use one final status and pack, which the
   existing save schema can persist. The linked presentation uses curse→stun→poison in Field and
   poison→stun→curse in Battle.

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
and battle reads. The equipped-curse effect uses the current format. Cast effects, MP payment and
settled return must survive cold Continue without replaying the cure or making a second draw.
[Save and Continue agreement](../application/battle-save-and-continue.md#stable-commit-and-request-ux) ·
[Support save rules](support-spell-status-design.md#save-compatibility-and-recovery)

## DETOX 4 area and Field admission

[#2236](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2236) uses Standard rigor for
the fourth learned rank across the existing Battle and Field callers. Sarah learns packed entry
`0xC2` at effective level 45 from her first spell list, which VICR uses at promoted level 25. The
unpromoted level cap is 40. The current scope is Battle26/Pacalon with existing player classes,
effects and assets.

**Confirmed** static at the same pinned USA source. In `spelldefs.asm`, DETOX 4 has power 0,
cost 16, the same teammate and silence flags, center distance 0–3, Manhattan radius 2 and
animation variation 4 (selector 102). `PopulateTargetsListForSpell` uses the center's teammate
occupancy grid and the radius table. `battleactions/battleactionsengine_1.asm:battlesceneScript_DetermineTargetsByAction`
then calls `battleactions/sorttargets.asm:battlesceneScript_SortTargets`. For this friendly spell
the result is ascending combatant slot.
The selected center can appear anywhere in that list. Use the caster's provisional destination
as its live tile for both center and area geometry. The center and recipients must be living,
placed non-neutral allies. Status does not gate inclusion.

`spellEffect_Detox` takes the same `0x0007` mask as rank 3. Each recipient contributes 5 EXP
on any clear. In `battleactions/earnexp.asm`, `battlesceneScript_AddStatusEffectSpellExp`
calls `battlesceneScript_AddExpAndApplyPerActionCap`, whose `PER_ACTION_EXP_CAP` is 49.
`battlesceneScript_CalculateHealingExp` calls `battlesceneScript_AddExpAndApplyHealingCap`,
whose `HEALING_ACTION_EXP_CAP` is 25. An ineffective recipient consumes one range-8 draw
and contributes zero. The cast spends 16 MP
once and pays the same-side award once after all results, using both range-16 draws. Silence
pays once and skips the cast effect, recipient work and award. Cancellation and Battle
insufficient MP leave state and seeds at the pre-commit boundary.

`FieldMenu` chooses one joined recipient from the same list as the other ranks. At ranks
above 2 it shares the CURSE → STUN → POISON path, including gear removal and refresh.
Field pays 16 MP clamped to availability, including no effect, and leaves both seeds and EXP
untouched. It allows low MP and silence, with no Battle geometry or area expansion.

Keep the selected center in the existing command selection. The friendly target rule expands
that center into ordered recipients at preparation and the detached action validates those
reactions. Reuse existing cure, gear refresh and reward authorities. The scene consumes one
cast effect and one finish wait around the ordered results. Its recipient bindings are checked
before commitment. Field uses the same mask with its established single-member caller.
Existing Save and Continue carry the result; no change to the record is required.

Acceptance uses focused engine cases and one bounded ordinary Battle26 cast from a disposable
VICR Sarah save with a source-legal level and learned book, followed by an independent title
Continue comparison. The Field engine case exercises one recipient while another member
also has a curable status.
Reuse the registered DETOX successor in place. Budget up to six 128 KiB envelopes, 2 MiB of
compact receipts and one serial native process. Natural high-level acquisition, enemy DETOX 4,
hardware timing and later campaign progression stay outside this slice.

### Battle26 input and independent Continue

**Confirmed** in the remake with Godot 4.7.2 .NET. A disposable copy of the registered
`derived/issue1956/vicar-review/receipts/vicar-road01/battle-ready-slot.json` sets Sarah to
VICR 25 with packed book `[192,194,206,68]` and seeds poison and stun on six active members.
The book, level and ailments are controlled starting state. Open ordinary Title → Continue,
then Magic → DETOX 4, cycle to Kazin at (5,25) and confirm. Sarah starts at (6,25).

The eight results appear in order for Bowie, Sarah, Kazin, Peter, Gerhalt, Rohde, Elric and
Janet. Kazin receives poison and stun notices. Elric and Janet have no effect. One cast spends
56→40 MP including Sarah's self-cure and awards 30 EXP (0→30) after six 5 EXP contributions.
Two range-8 ineffective draws precede the two range-16 award draws. Selector 102 uses all
11 mounted Original frames with one cast cue and one SpellStop cleanup. Kazin receives the next
usable player action. The engine cases also exercise worn CURSE with retained gear and an
unrelated status, inclusive range and radius, free cancellation, insufficient MP and silence.
Field rank-4 coverage uses the actual engine command.

Save at settled Battle Movement. A fresh Godot process and save namespace load a copy of that
output via Title → Continue. The immediate re-save advances sequence 9→10 with checksums valid
and equal complete decoded battle data, including the cured statuses, learned book, spent MP,
EXP, actor control, queue and both seeds.

Use the private driver registered at `derived/issue2236/review` with the
[locked Debug adapter](../development-and-verification.md#locked-net-workflow). It mounts the
existing `derived/issue2232/pre-voyage-detox-v1` in place. From the executing checkout use the
approved disposable save root and a fresh journey name. Each mode launches its own Godot process.
The cold mode uses that journey's cast save as input.

```powershell
. ./local/private-inputs.ps1
$repo = (Get-Location).Path
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2236/review'
$journey = 'review-' + [guid]::NewGuid().ToString('N').Substring(0, 8)
& pwsh -NoProfile -File "$review/Run.ps1" -Repo $repo -SaveRoot $disposableSaveRoot -Journey $journey -Mode cast
if ($LASTEXITCODE) { throw 'Inspect cast receipt and exit' }
& pwsh -NoProfile -File "$review/Run.ps1" -Repo $repo -SaveRoot $disposableSaveRoot -Journey $journey -Mode cold
if ($LASTEXITCODE) { throw 'Inspect Continue receipt and exit' }
```

**Completed failures.** Early test assertions miscounted the two silence observations as two
prompts and counted the ordinary post-turn silence-expiry draw as part of the suppressed cast.
The final test checks the one presented notice and isolates the post-turn draw. The first
native attempt stopped at commit with `battle-policy-failure` because the award validator
still enforced the 25 EXP recovery cap. The six-target engine case now requires accumulated
30 EXP, and both award validators admit the source status cap. A later observer omitted Janet
from its expected area and therefore undercounted one no-effect draw. The registered reports
retain both attempts. The successful cast and cold processes exited 0 with ObjectDB warnings
(8 and 2 instances). These remain under the paused [#1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716)
boundary.

**Unknown.** Natural late-rank acquisition and casting, original full scene timing and
enemy rank-4 delivery. The remake observation uses the controlled Battle26 input above.

## Acceptance and input budget for ranks 2 and 3

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
  record the live Battle26 observation as paid no effect. The equipped-curse witnesses below use
  the legal #1926 loadout on disposable controlled copies.
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
presentation changes. The supported ranks link from
[gameplay rules](gameplay-rules.md), [campaign characters](../content/campaign-characters.md)
and [capability status](../capability-status.md).

## Ordinary input and cold Continue

**Confirmed** in the remake using Godot 4.7.2 .NET and the registered game package.

| Entry | Input and result |
| --- | --- |
| English Field, Map51 Harpy Pond return | Open Field → Magic, cycle to Sarah, choose the learned rank and select a joined recipient. Cancel at confirmation before the first cast. Rank 2 cured Bowie's poison (Sarah MP 53→47), rank 3 cured Sarah's own poison (47→37), then a clean-target rank 2 reported no effect and still paid (37→31). The result identified the rank and recipient. Both RNG words, HP and EXP were unchanged across each cast. |
| Chinese Battle26, regular Movement | On Sarah's first two turns, accept her current tile, open Magic, select 解毒 2 and then 解毒 3, and target self. Cancel and reopen before each actual cast. Both ranks showed the selected spell icon and nominal MP. The clean-target scenes paid 56→50 and 50→40, respectively, and awarded 1 EXP each (40→42). Control passed to the next actor both times. |
| Field and Battle Save → cold Continue | Fresh processes loaded the produced slots while the producing checkout's `local/` directory was detached. An immediate re-save preserved both complete decoded `data` objects with valid checksums, advancing sequences 62→63 and 7→8. Field resumed walking south on Map51. Battle took ordinary turns until Bowie's paid EGRESS, then regained usable Map36 field input. |
| Battle19 STUN pilot | The registered repeat seed reached round 5 after four rounds of normal turns. No effective STUN cure was witnessed. Natural Battle cure in this suffix remains **Unknown**. |

Use the private `issue1965-review` input driver with the registered SDK, Godot and
`issue1966/content/package` inputs. From the implementation checkout set `$review` to that
external review directory. Use fresh journey names; each cold mode reuses its producing
journey's player slot. The driver sends ordinary keys and records current state, projection and
errors. `-Detach` moves the producing local directory aside for cold startup and restores it on
exit. The check compares entire decoded payloads separately from process exit.

```powershell
& "$review/Run.ps1" -Repo (Get-Location).Path -Mode field -Journey review-field -Language en
& "$review/Run.ps1" -Repo (Get-Location).Path -Mode cold-field -Journey review-field -Language en -Detach
& "$review/Run.ps1" -Repo (Get-Location).Path -Mode battle -Journey review-battle -Language zh-CN
& "$review/Run.ps1" -Repo (Get-Location).Path -Mode cold-battle -Journey review-battle -Language zh-CN -Detach
& "$review/Run.ps1" -Repo (Get-Location).Path -Mode pilot -Journey review-pilot -Language zh-CN
```

The five completed observation runs exited 0. The field cold run logged an ObjectDB leak warning
after the functional checks. Early rehearsals exited 2 while selecting Sarah's roster entry or
checking battle results. The retained Battle26 rehearsal also showed a duplicate Chinese rank
label, corrected in the locale owner before the successful Battle26 run. The retained review
directory including rehearsals is about 1.8 MB, with 13 save envelopes including backups and
receipt copies, one above the planned count. The saves total about 299 KB. No world or scene
package was generated.

[Focused Battle cases](../../tests/Sf2.Remake.Engine.Tests/PoisonCureTests.cs) exercise
rank 2 and 3 together with mixed CURSE, STUN, POISON and divergent applied ATTACK/BOOST counters.
They check the same-side award and draws, target refresh, staged notices, cancellation and
self-payment. [Focused Field cases](../../tests/Sf2.Remake.Engine.Tests/FieldRecoveryTests.cs)
exercise fallen members, MP clamping, no-effect refresh and mixed-status treatment. Both check
the ordinary weapon, damaged ring, bare duplicate, slot order and custody.

### Equipped-curse input and cold Continue

**Confirmed** in English Godot 4.7.2 .NET using the registered
[Black Ring reproduction inputs and launcher](equipment-use-repair-design.md#black-ring-reproduction).
Fork the existing v6 Field and v4 battle WIZ slots for the following controlled starting states.
Update the checksum of the disposable copy and preserve the active PRST40 Sarah with packed
learned entry 130. The casts, saves and cold loads use ordinary player input.

| Entry | Minimal fork and player input | Observed result |
| --- | --- | --- |
| Map36, Field Magic | Keep the original field position. Give Kazin pack `[85,408,24,220]`, status 7 and custody once bit 10. Open Field → Magic → Sarah → DETOX level 3 → Kazin. Cancel once at confirmation, then cast. | The menu showed 10 MP. The result named Kazin and displayed curse, stun, poison in that order. Sarah MP 56→46, Kazin HP 35, status 7→0, ATT 53→43 and AGI 36→41. His pack became `[85,280,24,220]`. The party returned to field control. |
| Battle26, round 2 | Set the queue cursor to Sarah (entry 0). Set Kazin's pack to `[213,408,24,0]` in both loadout copies, ATT 31, status 4 and custody once bit 10. From Sarah's (6,25), wait for each segment as she moves left then up to (5,24). Open Magic, choose DETOX 3 and confirm Kazin at (5,22). | Magic showed 10 MP and the scene acknowledged removal, then 5 EXP for Sarah (40→45). Her MP went 56→46. Kazin HP stayed 35, status went 4→0, ATT fell 31→21 and the pack became `[213,280,24,0]`. Player Movement resumed on Kazin. |

At settled input open System → Save (F10, Up, Enter in the observed layout), select slot 1 and
confirm the overwrite as Save. Copy each published slot to a fresh namespace, start a fresh
Godot process and use Title → Continue, then save again. The Battle re-save preserved the
complete decoded data (sequence 10→11). Field preserved the complete party and custody
(sequence 11→12), with visit motion and the simulation tick advancing before re-save. Both
envelopes verified against their payload checksums. The cold loads were already settled
at input, with the cured pack, HP, ATT and paid MP restored.

Allow two read-only seeds (about 63 KiB total), up to ten disposable slot envelopes, 2 MiB of
compact state/projection/error receipts, and one serial Godot process. The observed review,
including rehearsals, retained 10 envelopes (322,012 bytes) and about 0.5 MB of JSON receipts.
It mounted the existing package in place. Successful casts and cold loads exited 0. Some runs
logged ObjectDB warnings at shutdown. Rehearsals exited 1 on the observer's exact Field row text,
save-menu selection, battle movement segment timing or confirmation stage. The delayed and
immediate Field re-saves both advanced autonomous visit state. The original selector and
save/return remain **Unknown**. The earlier Battle19 pilot and paid EGRESS results stand
on their original inputs.
