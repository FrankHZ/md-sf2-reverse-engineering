# Ordinary promotion through first Pacalon

[Issue #1956](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1956) extends the
accepted HERO rite to the other ordinary destinations available to the joined force through
Battle26 and first Pacalon. **High rigor** applies because one promotion changes the inputs to
growth, combat, presentation and both save paths. Deliver families in the order below, with each
family independently reviewable. The current route can recruit any one of the four initial Creed
offers. Their admitted branches belong in the inventory even when a particular castle save lacks
them. Higins and Rohde join as PLDN and BRGN, respectively. They exercise starting-class
compatibility. Frayja and later recruits are outside this boundary.
[Progression and economy](../../../docs/design/synthesis/themes/progression-and-economy.md#definitions-current-class-and-promotion) ·
[Campaign roster](../content/campaign-characters.md) ·
[Creed choices](creed-recruitment.md#persistence-and-continuation) ·
[Church and HERO acceptance](church-promotion-design.md#playable-hero-and-reproduction)

## Rules shared by every family

**Confirmed static** against the USA retail baseline and pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`. Join status, base class and stored level ≥20 determine
ordinary eligibility. A fallen or reserve member is eligible. The regular offer is free.
A matching carried item can present an earlier special offer. Declining it reaches a separately
confirmed regular offer with the same inventory. Success publishes level 1 and EXP 0 with the
destination class, keeps earned stats, live HP/MP and the book, and loads class MOV, movement,
resistance and prowess. Refresh status and equipment from these inputs. Only NINJ in this
ordinary set invokes the explicit weapon unequip. The next earned level selects the local
actor/destination growth block, uses the promoted cap of 99 and compares spell thresholds
against stored level +20. The columns for projected growth supply the growth calculation.
The rite leaves the member's accumulated bases in place.
[USA manifest](../../../manifests/roms/sf2-us.json) ·
[Church transaction](church-promotion-design.md#original-rules-that-the-implementation-must-preserve) ·
[Promotion rows](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/classes/promotions.asm) ·
[Growth data and inheritance](../../../docs/research/party/ally-growth.md#storage-model)

The remake keeps the live class in `BattleActorProgress.ClassId`. Validate the legal source pair
and all required operands before publishing the existing church transaction. Resolve that pair
for subsequent field inspection and recovery, Caravan deployment, battle entry/AI targets, growth,
and victory, defeat or EGRESS return. An unavailable binding must give an attributed reason.
Field formats 1/3 carry the live class. For battle saves without timed status operands, keep v2
for unchanged classes and v4 for changed classes. Use the accepted #1966 battle v5 when a
deployed actor has a live SLOW/BOOST counter or a last-applied counter operand. The v5
class-term flag controls class presence for deployed and retained progress. A changed-class
party carries class IDs for both, as in v4. Deployed actors keep the packed status word
and, where it diverges, the sparse `LastRefreshCounters` operand. Capture and restore
reject SLOW/BOOST counters on retained members. These combinations use the existing
versions and save owners.

At restore, validate the exact actor/class pair, the legal packed loadout and the packed book.
Reconstruct derived stats using the saved last-applied counters when present, or the stored
counters otherwise, with equipped effects applied afterward. Preserve the live counters for
the next turn. Only the next real refresh seam applies the then-current counters, including
any preceding after-turn decrement. Compare the detached continuation before publication.
Books can contain base-class ranks learned past effective level 21 before the rite. Preserve
the learned higher rank and the bound lower-rank actions. Reject spells outside the member's
reachable lists and preserve valid learned ranks whose effects still lack support. Restore may
resolve actions from the book without rewriting it.
[Commit and persistence](church-promotion-design.md#commit-and-refresh-boundary) ·
[Battle save codec](battle-save-and-continue.md#implemented-versioned-codec-and-shared-storage-b) ·
[Timed-status save and restore](../domain/support-spell-status-design.md#save-compatibility-and-recovery) ·
[Book contract](../../../docs/design/contracts/party/spellbook-state.md)

Raise reads the resolved current class and stored level. The source price is `10 × level + 200`
for a promoted member and `10 × level` otherwise. For example, fallen Eric goes from KNTE 27
(270 gold) to PLDN 1 (210 gold) and is still fallen until paid Raise. Accepting the rite must
preserve gold and HP. Declining Raise must preserve both as well. Successful Raise restores
HP to the existing cap and exposes the living field appearance. Test an initially promoted
recruit, a newly changed reserve and old-save restoration without inferring class from level.
[Raise source](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/church/churchactions_1.asm) ·
[Source constants](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/sf2enums.asm) ·
[Raise observation](../../../docs/design/contracts/party/service-interactions.md#church-raise-runtime-seam)

[#1946](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1946) owns independent weapon
and ring selection, validation, refresh and round trips of the packed loadout. Every family must
consume those same rules once available. Test legal weapon plus ring, ring only, and no ring
where the loadout changes. NINJ's ring retention gate depends on #1946. Power,
Protect, Quick and Running Ring have ALL masks and keep their static contributions on the new
class. Apply only the surviving equipped slots after status refresh. The physical range comes
from the weapon or the source unarmed rule (1–1 for this ordinary set). Promote neither an
equipped ring into the weapon slot nor a newly legal carried weapon into the equipped slot.
[#1943](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1943) owns permanent growth
persistence and Brave Apple's shared level-up. Preserve its accumulated HP/MP/ATT/DEF/AGI bases
and live vitals at promotion. Class load clears its designed absolute `BaseMove` override.
Refresh MOV from the new base and any retained ring. Route later Apple use through the same
current-class growth path as earned EXP. Early class slices can proceed with the accepted
HERO save shape and supported loadouts. Serialize shared refresh/save edits and verify this
composition when those implementations are accepted.
[Ring custody and stats](../domain/ring-equipment-design.md#custody-promotion-and-continue) ·
[Permanent growth and class load](../domain/permanent-growth-items-design.md#refresh-and-class-load)

## Family inventory and actual consumers

The following pairs and member-specific growth and appearances are **Confirmed static**.
MOV is base MOV, followed by the mover. All destinations have their own growth block. The
common destination prowess is 1/16 critical with the 25% bonus, 1/32 double and 1/16 counter.
WFBR instead counters at 1/8. NINJ uses the 50% critical bonus at 1/8, double at 1/16 and
counter at 1/8. PHNX adds major FIRE/STATUS resistance. MNST adds major NEUTRAL,
UNDEFINED1, UNDEFINED2 and STATUS resistance. Other destinations below have no class
resistance. Use the pinned class and target-priority rows, including for enemies acting
after the player's first move. Compare terrain cost and land effect through the chosen mover.
The accepted content parser already binds the common destination prowess. WFBR and NINJ
need their distinct combinations admitted and used by damage, double and counter selection.
[Class bases](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/classes/classdefs.asm) ·
[Growth selectors](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats) ·
[Terrain table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/global/landeffectsettingsandmovecosts.asm) ·
[Enemy target classes](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/global/aipriority.asm)

| Pair and current member IDs | Movement and combat consumers | Destination field ID / battle palette in member order |
| --- | --- | --- |
| KNTE 1 → PLDN 13. Chester 2, Rick 12, Eric 14 | 7 CENTAUR stays. Empty spell lists. Steel Lance 1–1 stays equipped when held. Javelin 1–2 becomes legal, with a range and weapon-layer check. The ordinary terrain cost and land effect remain centaur. | 6 / 0, 28 / 1, 34 / 3 |
| MAGE 3 → WIZ 17. Kazin 4, Tyrin 17 | 5 MAGE stays, rod mask retains ordinary rods and sticks. Inherit each member's first list. Both keep learned BLAZE and their own other spells. The class enables the Black Ring mask. | 12 / 2, 43 / 1 |
| WARR 2 → GLDT 15. Jaha 3, Randolf 16 | 5 REGULAR stays. Empty book. Retain legal axe and range. Observe a normal attack with its independent ring contribution when admitted. | 9 / 1, 40 / 2 |
| PRST 4 → VICR 19. Sarah 1, Karna 15 | 5 HEALER stays, both retain rods and inherit their first list. Exercise HEAL with promoted power and healer EXP. HEAL 4 retains full-recovery sentinel semantics. Karna's later AURA uses her list and the target rule. | 3 / 1, 37 / 2 |
| ACHR 5 → SNIP 21. Elric 13, Janet 18 | 5 → 6 ARCHER. Empty book. Steel Arrow 2–2 stays equipped. Verify ranged placement and unarmed fallback separately. Source permits promoted-only shells where carried and admitted. | 31 / 0, 46 / 1 |
| RNGR 8 → BWNT 25. May 8 | 6 CENTAUR_ARCHER stays. Empty book. Same Steel Arrow and source shell distinction, with centaur-archer terrain and May's own scene. | 21 / 0 |
| BDMN 6 → BDBT 23. Luke 10 | 6 FLYING stays. Empty book. Retain Middle Sword and select a source-legal carried sword after the rite when available. Confirm flight terrain/land effect. | 25 / 0 |
| WFMN 7 → WFBR 24. Gerhalt 9 | 5 STEALTH stays. Empty book, no weapon. Confirm unarmed body, stealth terrain/land effect and updated counter prowess. | 23 / 0 |
| PHNK 9 → PHNX 26. Peter 7 | 6 HOVERING → 7 FLYING. Empty book, no weapon. Confirm newly reachable flight tiles, flight land effect, major fire/status resistance and unarmed attack. | 19 / 0 |
| THIF 10 → NINJ 27. Slade 5 | 7 REGULAR → 7 STEALTH. Empty THIF book, destination teaches KATON/RAIJIN. Clear Short Knife's equip bit and leave it carried. Confirm unarmed attack then equip a legal sword such as Middle Sword. Preserve a worn ring and refresh its contribution. | 15 / 0 |
| TORT 11 → MNST 28. Kiwi 6 | 5 STEALTH → 7 HOVERING. Empty lists. Check unarmed attack and the source class-28 physical action conversion to KIWI. It is an action result outside the learned book. | 17 / 0 |

For field IDs, use `GetAllyMapsprite` with the current class and preserve the entity's position,
direction, vehicle and fallen presentation. The ordinary class uses the mapsprite table entry
minus one for PLDN/GLDT/WIZ/VICR/SNIP and the table value for BDBT through MNST. Resolve the
pinned actor/class battle sprite and palette independently. PHNX, WFBR, NINJ and MNST also
switch portraits. Retain the normal portrait mapping for the other families. Admit the body
for no weapon and every legal weapon-layer combination reachable in that selection. The
existing scene producer includes each member's starting weapon as a default body donor.
For NINJ, use the source unarmed body/sequence without admitting an equipped THIF-only knife.
A palette or sprite match from another member is insufficient class admission.
[Map appearance table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/allymapsprites.asm) ·
[Mapsprite lookup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/scripting/entity/getallymapsprite.asm) ·
[Battle sprite and palette table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/allybattlesprites.asm) ·
[Portrait remap](../../../docs/design/contracts/interface/portrait-window-state.md#allyclass-remap) ·
[Item masks and range](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/items/itemdefs.asm)

### Spell and attack boundaries

- **WIZ.** The castle Kazin seeds have BLAZE 3, MUDDLE 2 and DISPEL 1. English has DESOUL 1,
  Chinese DESOUL 2. Reuse the currently bound BLAZE 1–3 and DISPEL 1. BLAZE 4 at effective
  level 35 and MUDDLE/DESOUL effects need their own rank/action admission. Tyrin learns
  FREEZE 1–2 and BLAZE 1–2 by recruitment at 24, with SLEEP at effective 25, BOLT 1 at 28
  and later FREEZE/BOLT/BLAZE ranks. Preserve them per member and list unsupported ranks
  explicitly. Kazin's retained BLAZE 2 or 3 is an ordinary first action. Confirm the same
  packed book and rank choices after growth and cold Continue. A carried Black Ring becomes
  class-legal for Equip and player Use, including Use while bare. Actual curse, BLAZE 2
  from the item, breakage and repair remain with #1926 and #1952. Learned BLAZE is a separate
  MP action. Resolve learned damage power from the current source class. The pinned promoted
  BLAZE 2 calculation takes base power 10 to 12 before element resistance and RNG. Check
  that adjustment on a learned cast and preserve the existing HERO witness's narrower
  action claim. [Promoted damage calculation](../../../docs/design/contracts/battle/spell-resolution.md#1-resolve-base-power-and-caster-adjustment) ·
  [Kazin growth](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats04.asm) ·
  [Tyrin growth](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats17.asm) ·
  [Current BOLT/HEAL/AURA and limits](church-promotion-design.md#current-battle-spells) ·
  [Black Ring](church-promotion-design.md#interface-for-rings-and-black-ring) ·
  [Hostile status #1967](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1967)
- **VICR.** Sarah at 40 retains HEAL 4, BLAST 4, SLOW 2 and DETOX 3. Karna at 28 retains
  HEAL 3, BLAST 3 and BOOST 1. Karna learns BOOST 2 at effective 30 and AURA 1 at 33.
  The current healing action, AURA validation and healing EXP have PRST-only checks.
  Extend these consumers together to the source healer classes required by this slice.
  An ordinary promoted HEAL 1 or 3 on missing HP distinguishes its 5/4 power from PRST.
  Also check full-recovery HEAL 4. Reuse BLAST 1–4, SLOW 1–2, DETOX 1, BOOST 1 and mounted
  AURA ranks where available. Record the actual effect support of higher DETOX and BOOST 2
  separately. [Sarah growth](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats01.asm) ·
  [Karna growth](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats15.asm) ·
  [Healing power and EXP](../../../docs/design/contracts/battle/spell-resolution.md) ·
  [DETOX #1965](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1965) ·
  [Support status](../domain/support-spell-status-design.md)
- **NINJ.** KATON ranks 1/2/3 are learned at effective 24/29/45, RAIJIN at 34/39/52.
  These are both NEUTRAL damage with distinct costs, range, radius and animation variation
  from the pinned records. Bind through the current attack-spell calculation, targeting,
  playback and growth book. The current narrow effect validation needs expansion for both families and their neutral
  resistance operand. Apply the same class power adjustment as for learned WIZ spells. Confirm
  learning and a paid KATON 1 cast at or after stored level 4 when MP permits. Use small
  behavior cases for the other thresholds and ranks. At the rite, THIF's empty book and
  zero max MP stay as earned. Keep his current MP and the carried knife in its original slot. [Slade growth](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats05.asm) ·
  [Spell records](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spelldefs.asm) ·
  [Damage elements](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spellelements.asm)
- **MNST.** Use the class-28 attack decision at its source position. It requests RNG(4)
  on a physical attack, converts on zero and chooses KIWI ranks across stored levels
  32/40/50. Keep the action outside Magic and the book, with no MP cost. Verify both
  physical and converted results, RNG continuity and projection through the existing action
  owner. The pinned record uses FIRE and the flame-breath presentation. Keep natural
  conversion/timing claims bounded to the source and later observation.
  [Conversion gate](../../../docs/design/contracts/battle/battle-functions-control-flow.md#kiwi-conversion-gate) ·
  [Spell records](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spelldefs.asm)

### Accepted support before expansion

**Confirmed in the remake.** The accepted HERO package has base/start growth rows for IDs
0–19 and one additional actor/class row, Bowie/HERO. Only Bowie completes the ordinary rite,
through battle growth, field and battle Continue and return. The only PLDN scene palette
mounted in the Battle26 selection is 2 for starting PLDN Higins. His observed Javelin
attack supplies a PLDN scene/range precedent. Chester, Rick and Eric need their own PLDN
rows and assets. Other destination pairs above have pinned source rows without admitted
destination content or ordinary promoted observations. The growth and battle scene readers
accept supplied actor/class rows for SDMN→HERO and KNTE→PLDN through the same finite mapping.
Usable PLDN promotion still requires member-specific field and battle resources and an ordinary
witness. Spell ranks and new class combat rules in the preceding section are separate gaps.
[HERO witness](church-promotion-design.md#playable-hero-and-reproduction) ·
[Higins witness](battle26-higins.md) ·
[Growth binding](../../src/Sf2.Remake.Content/Scenarios/BattleGrowthReader.cs) ·
[Scene admission](../../src/Sf2.Remake.Content/Scenarios/BattleSceneContentReader.cs)

## Implementation order and ownership

1. **PLDN and shared ordinary admission.** Extend the existing private selection
   `promotionClasses` with finite mapped actor/destination rows. Reuse `RegularPromotion` and
   existing class resolution. Generalize private reader, stat and presentation joins to the
   admitted pair, preserving authored profiles and initial PLDN/BRGN. Include source AI target
   lookups, equipment masks, and field v1/v3 and battle v2/v4/v5 restore in the first consumer
   review. Exercise a v5 round trip at settled battle input with a promoted actor carrying
   divergent SLOW/BOOST counters, a retained promoted member, and valid packed books and
   loadouts. Compare class, packed words, live and last-applied counters, derived DEF/AGI and
   the next real refresh against uninterrupted play. Make the offer state distinguish initial
   consent, unavailable special, special decline, regular answer and back to selection. The
   shared church flow supports these stages for an admitted ordinary destination. Declining
   the special offer keeps the held word and its acquisition flag. PLDN's content bindings
   and ordinary Pacalon witness remain open. Preserve the no-item regular path and the special
   acceptance boundary for
   [#1957](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1957).
2. **WIZ and the first retained spell caster.** Reuse the admission path for Kazin and the legal
   Tyrin branch. Exercise rod and cast animation independently. Carry current-class item
   eligibility to the equip/use interface referenced by #1946 and #1926. An unavailable spell
   effect keeps its unsupported status through promotion, growth and Continue.
3. **Remaining armed families.** Group GLDT/BDBT as retained melee weapons, then SNIP/BWNT as
   retained ranged weapons and promoted-only masks. Give VICR its own review for healer
   eligibility, power, EXP and learned ranks. Preserve special WARR/PRST/ACHR fallback wherever
   the corresponding current holder exists. Binding a lawful new class/weapon must also
   admit the animation, including return to an earlier reachable encounter.
4. **Innate and changed weapons.** Group WFBR/PHNX around unarmed projection, movement and
   class bases. Treat NINJ and MNST as independent work within #1956 because of their additional
   action consumers. Keep the relevant row open until the learned damage or attack conversion
   and the required ordinary witness land. If a needed change grows beyond the existing rule
   owner, give main-gate the dependency before taking that shared path.

The first slice owns a single shared rite cue. Source `MUSIC_PROMOTION` is command 25 decimal.
The current package's `audio-25-c6` refers to command 37 (hex 25). The mounted descriptor list
has no command 25. Select one private non-looping cue using the existing audio pipeline, publish
it once on a successful rite and resume the prior church/map music using the existing audio
lifecycle. Check ordinary input through success, playback/return and member selection. Decline,
unavailable and stale input emit no cue. Exact original timing across the whole rite remains
**Unknown**. Keep the existing HERO action as a shared-cue regression case.
[Source cue and wait](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/church/churchactions_1.asm#L432-L479) ·
[Mounted audio baseline](church-promotion-design.md#playable-hero-and-reproduction)

Reserve exact affected files before each implementation PR. The shared edits to serialize with
#1946 and #1943 are in the existing owners below. Reuse the same selection, class table,
growth/loadout resolution and save contract across families.

| Owner | File or consumer |
| --- | --- |
| Source selection and assembly | `remake/content/private/pre-voyage-opening.json`, `src/sf2tool/remake_exploration_content.py`, `remake_exploration_scene.py` and `remake_battle_scene_content.py`. Use the admitted splice of later member visuals into earlier encounter bindings. |
| Private content and class rules | `BattleGrowthReader.cs`, `PrivateBattleDefinitionReader.cs`, `PrivateBattleScenarioReader.cs`, `BattleSceneContentReader.cs`, `PrivateFieldPreparation.cs` and `BattleEquipmentReader.cs` in Content/Scenarios. In Domain, see `Sf2ClassRules.cs`, `EngineBattleState.cs`, `BattleGrowthRules.cs` and `BattleEquipment.cs` in Domain. Review source AI priority tables when adding a class. |
| Existing action owners | `PrivateAttackSpellBindings.cs` and its Domain attack rule for the bound WIZ and NINJ effects. For VICR, see `Sf2HealingRule.cs`, `HealingTargetRules.cs` and `Sf2BattleProgressionRule.cs`. Use current physical action and attack playback for WFBR/PHNX/MNST. |
| Transaction, restore and projection | `FieldService.cs`, `PartyDetailsQuery.cs`, `CaravanRuntime.cs`, `BattleEntry.cs`, `OpeningRestore.cs`, `BattleRestore.cs`, `ChurchServiceView.cs` and the existing EN/zh-CN catalog and `SessionAudio` path. Restore and projection files need edits only where existing current-class resolution fails. |
| Acceptance and publication | Target existing engine behavior projects and update the affected rows in this owner, [campaign characters](../content/campaign-characters.md), [campaign items](../content/campaign-items.md), and [capability status](../capability-status.md#campaign-feature-trace). Expose witnessed actions, unsupported effects and actual restorable appearances. Location/flag rows change only for changed callers or custody. |

## Ordinary witnesses and cost

Decode copies of the registered `handoffs/issue1927/pacalon/slots/castle-en.json` and
`castle-zh-CN.json` using the church owner's read-only procedure. The snapshots cost about
50 KiB per envelope, ~37 KiB decoded, and include a full active formation. Use separate
external player-save namespaces. Preserve the original slots and every completed failure.
A candidate's presence in the package or an engine case is independent of ordinary reach.
[Seed and save procedure](church-promotion-design.md#inputs-commands-and-observation-budget)

| Family | First ordinary witness or gap |
| --- | --- |
| PLDN | English Eric, KNTE 27 and fallen, with Steel Lance and Kazin's separate Wing. Refuse special, accept PLDN, check 210-gold Raise, save/continue, deploy and attack. Save once with PLDN in reserve to exercise the retained class wire path. On a copy, put Wing in depot with ordinary input to exercise the no-carried-item path. Check loan of Higins's carried Javelin and its 2-tile attack. Chester at 1 exercises level refusal. Rick is absent from both castle force lists. Use the registered #1939 Hassan 19 join and earn the next level in Battle17 for a bounded exact-20 witness. |
| WIZ | Living Chinese Kazin MAGE 29 gives retained high ranks and a paid BLAZE cast. English Kazin 27 is fallen and can be raised. Use the legal Battle26 Black Ring holder route only when checking #1926. Tyrin's initial Creed join was played. The corresponding promoted ordinary route remains **Unknown**. |
| GLDT, SNIP, BWNT, BDBT | Jaha WARR 21 (both), Elric ACHR 22 and Janet ACHR 26 (Chinese), May RNGR 21 and fallen (both), Luke BDMN 23 English or 22 Chinese and fallen (both). Play one repeat per coherent group with active or reserve Relief, checking each new pose and legal attack. Randolf's ordinary Creed choice supplies a legal alternate recruit. The promoted continuation remains **Unknown**. |
| VICR | Sarah PRST 40 in both castles and Karna PRST 28 in Chinese. Use one promoted HEAL witness with partial HP, a smaller learned rank and actual award, plus a legal field/book/cold round trip. Preserve the differing books with the same implementation. |
| WFBR, PHNX | Chinese Gerhalt WFMN 29 and Peter PHNK 31 in both castles. Compare the reachable movement cost/land effect against the preceding class, then observe the unarmed action and return. |
| NINJ, MNST | Both castles have Slade THIF 12 and Kiwi TORT 15. An eligible retained witness remains **Unknown**. Start from a lawful pre-Pacalon save with higher levels if one is registered. Otherwise try a bounded Battle26 repeat from a castle copy. Raise Slade, then earn ordinary EXP to 20. The seed deficits are about 760 and 490 EXP respectively. Measure EXP per action and turns on one repeat and agree a finite budget before training both. Keep the native row **Unknown** if the pilot cannot supply an eligible journey within budget. Engine cases at valid levels cover weapon/ring, the first learned rank, resistance and both conversion branches independently. |

Per family, at a normal church compare the authoritative progress/pack/flags/gold/seeds around
the final Yes. Use current field sprite/portrait where projected, ordinary save and fresh title
Continue, then the Battle26 repeat or the bounded earlier recruit case above. Check move range
and terrain, one differentiating action, class/rank growth and active/reserve exchange. Use
a stable-input battle save and cold Continue, then EGRESS or victory and return to field
control. Exercise the affected gameplay rules in small engine cases at levels 19, 20 and late,
with a missing variant, stale answer, different legal equipment and old field/battle saves.
Run `uv run sf2 verify engine`, the affected adapter build and clean-head planner for implementation.
Observe activity using the existing input/state/projection/error surface. Use ordinary English
and Simplified Chinese controls across the group where real seeds exist. Document the separate
seed when reusing one history for UI coverage. Screenshots and whole-campaign runs are outside
this slice. A native shutdown failure after a published slot and the functionality of that
slot are distinct results. #1716 is deferred.
[Existing Godot and save reproduction](church-promotion-design.md#playable-hero-and-reproduction)

The 17 new actor/class pairs add 17 growth blocks and class/presentation joins. At the mounted
growth row sizes of ~1.1–1.9 KiB, budget ~20–35 KiB for the complete growth delta. One pair adds
one field mapsprite, one battle body/palette and compatible carried-weapon bindings. The PLDN
pilot starts with Eric. The mounted Higins palette covers seven legal weapon keys (56, 72–75,
77, 78), at most seven Eric rows for that same selection plus the unarmed body fallback.
WIZ/VICR start with the selected four rod/stick keys per palette. Estimate melee/ranged sets
from the carried-weapon selection and masks, with one body for each unarmed class and four
additional portraits overall. Reuse already mounted weapon layers and count unique raster keys.
The measured HERO pilot added eight bindings and 18 unique rasters for ~47 kB. Until measured,
allow ~50–100 KiB per armed pair and ~20–50 KiB per unarmed pair including a field sprite/portrait
where needed, around 1–2 MiB across all variants before audio. Budget one measured cue using
its WAV duration and format. Verify package and scene admission limits before expanding a
family, including the reader's 4 MiB scene document cap.

Use the registered #1955 HERO world (~102 MB) and the #1939 rebuild entry with the current
selection. Start with one changed-class package per family pilot, preserving stable map,
audio and encounter inputs and reusing the resulting package for both locales. Estimate
~105 MB for the rebuilt world plus the affected scene and shared cue, then measure actual
output after Eric. The current Battle26 scene is ~1.26 MB. Budget under 1 MiB of compact
save copies per family and record only decisive input/state/error checkpoints. This is
linear in selected variants and their legal weapon combinations. Select only legal member/weapon combinations in reachable encounters. Replan on a new scene size limit, a needed unmounted
action family, or a witness that would require replay beyond the supported campaign boundary.
[HERO measured delta and inputs](church-promotion-design.md#playable-hero-and-reproduction) ·
[Package builder](../../../src/sf2tool/remake_battle_scene_content.py)
