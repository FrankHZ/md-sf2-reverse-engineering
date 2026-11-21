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
The content parser binds the common destination prowess and the distinct WFBR and NINJ
combinations.
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
  explicitly. WIZ resolves the Black Ring class mask. The ordinary ring interfaces and item
  effects still depend on the supported item's own rules. Curse and BLAZE 2 from the ring
  remain with #1926. Indra remains with #1952. Learned BLAZE is a separate MP action.
  The pinned promoted BLAZE 2 calculation takes base power 10 to 12 before element
  resistance and RNG. Item-origin damage uses the item's base power. Kazin's retained
  book and paid cast were observed after promotion and cold Continue. [Promoted damage calculation](../../../docs/design/contracts/battle/spell-resolution.md#1-resolve-base-power-and-caster-adjustment) ·
  [Kazin growth](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats04.asm) ·
  [Tyrin growth](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats17.asm) ·
  [Current BOLT/HEAL/AURA and limits](church-promotion-design.md#current-battle-spells) ·
  [Black Ring](church-promotion-design.md#interface-for-rings-and-black-ring) ·
  [Hostile status #1967](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1967)
- **VICR.** Sarah at 40 retains HEAL 4, BLAST 4, SLOW 2 and DETOX 3. Karna at 28 retains
  HEAL 3, BLAST 3 and BOOST 1. Karna learns BOOST 2 at effective 30 and AURA 1 at 33.
  VICR uses the shared healer eligibility and EXP rules. HEAL 1, HEAL 3 and mounted
  AURA 1 apply 5/4 to the raw power before the missing-HP cap. HEAL 4 retains its
  full-recovery sentinel. The existing Battle26 bindings supply BLAST 1–4, SLOW 1–2,
  admitted DETOX 1–3 and BOOST 1. Karna learns BOOST 2 and AURA 1 through growth.
  The BOOST 2 effect, higher campaign AURA ranks and DETOX 4 remain with their effect owners. [Sarah growth](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats01.asm) ·
  [Karna growth](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats15.asm) ·
  [Healing power and EXP](../../../docs/design/contracts/battle/spell-resolution.md) ·
  [DETOX #1965](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1965) ·
  [Support status](../domain/support-spell-status-design.md)
- **NINJ.** KATON ranks 1/2/3 are learned at effective 24/29/45, RAIJIN at 34/39/52.
  These are both NEUTRAL damage with distinct costs, range, radius and animation variation
  from the pinned records. The current attack-spell calculation uses neutral resistance and
  the same learned promoted power adjustment as WIZ. Engine cases cover all six ranks and
  effective learning levels. At the rite, THIF's empty book and zero max MP stay as earned.
  The carried knife remains in its original slot with its equip bit cleared. Ordinary acquisition
  and paid casts remain **Unknown**. [Slade growth](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats05.asm) ·
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

### Admitted ordinary classes

**Confirmed in the remake.** The mounted campaign admits SDMN→HERO for Bowie, KNTE→PLDN for
Chester, Rick and Eric, MAGE→WIZ for Kazin and Tyrin, WARR→GLDT for Jaha and Randolf,
BDMN→BDBT for Luke, ACHR→SNIP for Elric and Janet, RNGR→BWNT for May, PRST→VICR for
Sarah and Karna, WFMN→WFBR for Gerhalt, PHNK→PHNX for Peter and THIF→NINJ for Slade.
The three knights have their own growth,
map sprites 6/28/34 and Battle26 sprite 13 palettes 0/1/3. Battle26 mounts seven weapon rows
each for Chester and Rick and six for Eric. The two mages inherit their own spell lists and
use map sprites 12/43 and Battle26 sprite 21 palettes 2/1. Each admits the four mounted
rods/sticks. The two GLDT members use field sprites 9/40 and battle body 18 with palettes
1/2. Luke uses field sprite 25 and battle body 14 palette 0. Their scenes admit the mounted
axes or swords and Luke's newly legal Broad Sword. Elric and Janet use field sprites 31/46
and battle body 31 palettes 0/1. May uses field sprite 21 and battle body 29 palette 0.
Their mounted arrows, shells and Unarmed use the member's body animation with the independent
weapon layer hidden. Sarah and Karna use field sprites 3/37 and battle body 25 palettes 1/2.
Each VICR has four mounted rod/stick bindings and the matching healing cast sequence.
Gerhalt uses field sprite 23, portrait 49 and battle body 30 palette 0. Peter uses field
sprite 19, portrait 48 and battle body 28 palette 0. Both use their own unarmed action.
Slade uses field sprite 15, portrait 50 and battle body 26 palette 0. His unarmed and
equipped sword scenes use the same member body. Earlier loaded encounters share the member bindings.
Higins keeps starting PLDN palette 2 and Rohde keeps starting BRGN. Ordinary promoted
Chester, Rick, Tyrin, Randolf and Slade remain **Unknown** in ordinary promotion play.
MNST still needs its gameplay binding and witness.
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
   shared church flow supports these stages for an admitted ordinary destination. The ordinary
   Pacalon fallback preserves the exact held Wing word and F175. The unsupported special
   acceptance path remains with
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

The shared rite plays finite `MUSIC_PROMOTION` (decimal command 25, Timer B CE) once after
final Yes. Godot observed one start, one Finished and the restart of Pacalon's `MUSIC_CASTLE`
for both Eric and Bowie. The finite cue stays outside field-save music history and the adapter
retains the prior music generation. Engine cases cover cancel, stale answers and the unavailable
special branch. Packages without command 25 retain their previous playback. Exact timing
of the original rite remains **Unknown**.
[Audio lifetime observation](../verification/host.md#audio-resource-lifetime-observation) ·
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
| PLDN | English Eric covered special decline, fallen reserve promotion, Raise, Relief, both physical weapons and field/battle cold Continue. Chester level 1 was refused in Chinese. Rick is absent from the castle force lists and the registered Hassan/Battle17 journeys reach KNTE 19 and 10 EXP. Exact-level-20 native promotion remains **Unknown**. A separate Pacalon copy deposits the Wing before the ordinary offer. [PLDN reproduction](#pldn-ordinary-reproduction) |
| WIZ | Chinese Kazin MAGE 29 became WIZ 1 with his retained book and rod. Independent Battle26 forks exercised a rod strike and paid BLAZE 2, followed by cold Continue and ordinary EGRESS. Tyrin's initial Creed join was played and his promotion has controlled engine coverage. His promoted ordinary route remains **Unknown**. [WIZ reproduction](#wiz-ordinary-reproduction) |
| GLDT, BDBT | English Pacalon Jaha promoted in reserve and used Short Axe with an independent Power Ring. Fallen Luke promoted, was raised, then used Middle Sword, Broad Sword and Unarmed in separate Battle26 forks. Field and battle Continue and EGRESS retained both classes. Randolf's ordinary promoted route and a native Warrior Pride fallback remain **Unknown**. [Melee reproduction](#gladiator-and-bird-battler-ordinary-reproduction) |
| SNIP, BWNT | Chinese Pacalon Elric and Janet kept Steel Arrow. Fallen May became BWNT, was raised and used Wooden Arrow. Battle26 exercised all three body variants, adjacent target rejection, Unarmed and 3-cell Assault Shell and Great Shot attacks. Cold Continue and EGRESS kept all three classes. A native Silver Tank fallback remains **Unknown**. [Ranged reproduction](#sniper-and-bow-knight-ordinary-reproduction) |
| VICR | Chinese Pacalon Sarah and Karna preserved their different books. In Battle26 Sarah used HEAL 1 and full-recovery HEAL 4. Karna used HEAL 3 and attacked with Guardian Staff. Controlled VICR leveling and AURA targeting preserve Karna's later learned ranks. [Vicar reproduction](#vicar-ordinary-reproduction) |
| WFBR, PHNX | Chinese Pacalon Gerhalt and Peter promoted and attacked unarmed in Battle26. Peter had 44 additional reachable destinations on his first turn. Both classes and portraits persisted through cold Continue and EGRESS. [Wolf Baron and Phoenix reproduction](#wolf-baron-and-phoenix-ordinary-reproduction) |
| NINJ | Ordinary English Battle26 pilot reached THIF 12 with 43 EXP. Eligible ordinary promotion and learned casts remain **Unknown**. Controlled engine and native coverage exercises the rite, equipment and spells. [Ninja reproduction](#ninja-controlled-reproduction-and-ordinary-pilot) |
| MNST | Both castles have Kiwi TORT 15. An eligible ordinary witness remains **Unknown**. The deficit is about 490 EXP. Start from a lawful higher-level history if one is registered. Budget any repeat using measured EXP before training. |

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

## PLDN ordinary reproduction

Use the registered `derived/issue1956/pldn-v1` package and its opening/entry files. It has
145 files and 294,393,387 bytes. It mounts the accepted private library at commit
`52435b04f49a84a6a9ed4444620055a070648001` over the registered `issue1966-original`
package. The package adds three growth blocks and class-conditioned field sprite rows from
the pinned source metadata. It is 18,327 bytes larger than the assembled asset-only
`promotion-v1`. Assemble a fresh copy with the existing Mount mode and a pinned source
checkout for progression tables.

```powershell
. ./local/private-inputs.ps1
$root = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2001'
$mount = @('--root', $runtime, '--base', (Join-Path $root 'issue1966-original'),
    '--family', 'all', '--command', '4', '--command', '25', '--command', '87',
    '--command', '99', '--upstream', $pinnedSource, '--mount', $freshPackage)
uv run --locked python -m sf2tool.remake_retail_assets @mount
```

Select the reviewed library's `runtime/retail-v1` as `$runtime` and the source revision in
[the asset owner](../content/asset-inventory.md#retail-original-preparation) as `$pinnedSource`.
Use the English Pacalon castle slot, its own save copy and the ordinary title Continue.
The driver and native receipts are registered under `derived/issue1956/review`. Its
`Boot.ps1` takes an isolated checkout via `-Repo`, a new `-Sample` and a separate `-Journey`.
The companion handoff lists the normal title Continue routes and seeds. The driver reads
state, scene and persistence projections.

| Boundary | Observed result |
| --- | --- |
| Church and custody | Put fallen Eric into reserve through Caravan Purge and join Higins to refill the formation. Return to Pacalon Castle and choose Eric, consent, decline the special offer for Kazin's Wing and confirm Paladin. The rite changes KNTE 27/83 to PLDN 1/0, preserves HP 0, earned bases 36/32/31, the empty book, equipped Steel Lance word 203 and 5,527 gold. Kazin retains the Wing in slot 2 (zero-based), with F175 set. |
| Depot branch | On a separate English castle copy, deposit Kazin's Wing through Caravan and return to the priest. Eric proceeds to ordinary confirmation after consent. Saving retains the Wing in the depot and F175 set. |
| Field Save and Raise | The v3 reserve slot cold-continues with the same allies, flags, formation, loadouts, gold and thinking seed. Four ambient residents advance position or facing, and the main seed advances before the next native Save. Declining the 210-gold Raise keeps Eric fallen. Paying yields 58 HP and 5,317 gold while he remains in reserve. |
| Relief and physical actions | At the overworld Caravan, exchange active Slade for Eric. Enter the supported Battle26 repeat via Ketto. Eric projects centaur movement, MOV 7 and field sprite 34. Steel Lance attacks from one cell. On a separate ordinary fork, Give Higins's Javelin to Eric, Equip it and attack from two cells. Both render palette 3 with the matching weapon and return to battle Movement. The two v4 cold Continues reproduce their complete decoded battle data. |
| Timed v5 and return | On a separate Relief branch also deploy Jaha. He uses his carried Power Ring on Eric. The v5 battle slot restores class 13, ATTACK `0xC000`, effective ATT 65 and packed state, with complete decoded data equality on cold re-save. Bowie casts EGRESS and returns to usable Map36. Cold v3 Continue after return preserves Eric's Paladin card and equipment. The controlled mixed v5 case checks divergent SLOW/ATTACK refresh operands and a retained HERO book. |
| Chinese regression | On the registered Chinese castle history, Chester at KNTE 1 receives the localized level refusal with no rite cue. Bowie's HERO success on a separate copy plays the same finite cue and restarts `MUSIC_CASTLE`. |

The first reserve pilot stopped in the Purge member list, the first Raise launch failed to
parse its driver, and the first Relief selection targeted reserve Chester. Those driver
failures are retained. One timed driver run was stopped before it exercised the intended
action. In a later run three SLOW 2 attempts were resisted before defeat. The Power Ring run
supplies the ordinary v5 witness. The Steel Lance run published both slots before its native
quit exit `0xC0000005`. The Javelin, Power Ring, cold v5, EGRESS and cold field results were
published independently. Some successful exits report ObjectDB teardown warnings. #1716
remains deferred.


## WIZ ordinary reproduction

Use the registered `derived/issue1956/wiz-v1` package. It has 145 files and 294,441,776 bytes,
a 48,389-byte increase over `pldn-v1`. The existing Mount consumes the reviewed retail library
at commit `52435b04f49a84a6a9ed4444620055a070648001` and the pinned source metadata. It binds six
walking strips and eight battle body frames, reuses 16 mounted weapon raster keys and adds two
class growth rows. The admitted rods and sticks share the same body colors in both Mage and
Wizard palettes. One package serves both locales. Allow 1 GiB of free disk for package assembly.
[PowerShell entry and private asset inputs](../content/presentation-assets.md#prepare-and-mount-retail-resources)

Use `Boot.ps1` from the registered `derived/issue1956/wiz-review` handoff. It takes an isolated
checkout via `-Repo`, a fresh `-Sample` and separate `-Journey`. Start with a copy of the Chinese
Pacalon castle slot. Run `promote` and `road` in the same journey, then use the resulting
`battle-ready-slot.json` in separate `rod` and `cast` journeys. Follow the cast with `cast-cold`,
`egress` and `field-return-cold`. The handoff lists the precise seed and checkpoints.

| Boundary | Observed result |
| --- | --- |
| Church and field v3 | Living Kazin MAGE 29/5 became WIZ 1/0. He kept HP 35, MP 55, earned bases 18/29/41, the equipped Wooden Rod and the four packed spell words. The finite command 25 played once and `MUSIC_CASTLE` resumed. No Secret Book was carried in this witness. Field cold Continue preserved the class, resources, book and equipment while the ambient field state and main seed advanced. |
| Battle26 rod and cast | Kazin entered with MAGE terrain movement, MOV 5, field sprite 12 and battle sprite 21 palette 2. On separate forks he attacked with Wooden Rod and cast learned BLAZE 2 on the Lesser Demon. The rod hit for 1 HP. The cast took 6 MP, dealt 12 HP and awarded 15 EXP. Both actions returned to battle input. The cast left the packed BLAZE 3, MUDDLE 2, DISPEL 1 and DESOUL 2 ranks intact. |
| Battle v4 and EGRESS | Complete decoded battle data matched on cold re-save at both the ready and post-cast checkpoints. Bowie cast EGRESS and returned to Map36. A fresh field v3 Continue retained Kazin's Wizard status, rod, spell book and 15 EXP. |
| Tyrin and special fallback | Controlled engine cases use Kazin’s and Tyrin’s packed books and levels with a rod equipped and a Secret Book held elsewhere in the party. Declining the special branch preserves the held word. The Tyrin WIZ growth row retains his own learn schedule. A promoted Tyrin ordinary journey and Kazin's native Secret Book fallback remain **Unknown**. |

The first native rod run reached an AI lookup error (`ai-target-class`). WIZ now uses its
source priority row. The next probe recorded a healing scene as a rod action. Its receipt is
retained and the ordinary rerun identifies Kazin's committed strike and target. Successful
native runs exited 0. Some reported ObjectDB teardown warnings. #1716 remains deferred.


## Gladiator and Bird Battler ordinary reproduction

Mount `melee-v1` from the registered `derived/issue1956/wiz-v1` and the prepared retail
library at commit `52435b04f49a84a6a9ed4444620055a070648001`. The pinned source supplies
the class and growth metadata. The closed package has 145 files and 294,525,207 bytes,
an 83,431-byte increase. It adds nine prepared walking strips and 15 battle body frames.
The 13 member/weapon rows reuse 36 distinct raster keys from the existing axe and sword
layers, including Bowie's Broad Sword layer for Luke. Shared palette slots 1 and 2 were
verified against the registered retail baseline. The same package serves both locales.
[Mount command and private inputs](../content/presentation-assets.md#prepare-and-mount-retail-resources)

The source class, map sprite and battle appearance have separate lookups. GLDT is class 15
with Regular MOV 5. BDBT is class 23 with Flying MOV 6. A rough tile with 15% land effect
costs GLDT three half-move units and BDBT two. BDBT receives 0% land effect. The zero-based
physical AI tie ranks for GLDT and BDBT are respectively 28/10 (Regular), 11/22 (Flying),
28/2 (Archer) and 6/31 (Mage). The two GLDT members retain their distinct growth curves
and all three keep empty spell books. [Class definitions](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/classes/classdefs.asm) ·
[Attack priority](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/global/aipriority.asm) ·
[Weapon eligibility](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/items/itemdefs.asm)

Use the registered `derived/issue1956/melee-review/Boot.ps1` with an isolated checkout.
Start the `jaha`, `luke`, `raise` and `road` modes in one journey from a copy of the
English Pacalon castle slot. `road` cold-saves the field, exchanges Slade for Jaha and
starts the supported Battle26 repeat. Fork the ready slot for `axe`, `sword`, `broad`
and `bare`. Use `battle-cold` on the corresponding after-action slots. From the Broad
Sword fork, run `egress` and `field-return-cold`. The private handoff gives the seed and
receipt names.

| Boundary | Observed result |
| --- | --- |
| Church and Raise | English Jaha WARR 21/88 became GLDT 1/0 in reserve. Active, fallen Luke BDMN 23/52 became BDBT 1/0 and stayed at 0 HP. The rites retained earned stats, packs and empty books. Declining Raise preserved Luke's HP and gold. Paying 210 gold gave Luke 40 HP and left 5,317 gold. The separate Chinese Jaha rite played the same finite cue and restored the Castle music. |
| Relief and axe | Caravan Relief exchanged Slade for Jaha. In Battle26, Jaha projected field sprite 9 and battle body 18 palette 1. Equipping the carried Power Ring kept Short Axe equipped. The one-cell axe hit dealt 2 HP to the Lesser Demon and returned to Movement. |
| Sword and Unarmed | Luke projected Flying MOV 6, field sprite 25 and battle body 14 palette 0. Separate adjacent attacks against the Lesser Demon used Middle Sword for 1 HP and Broad Sword for 13 HP. The Broad Sword fork equipped the carried sword after promotion and kept Middle Sword in the pack. A separate input selected Unarmed and kept both swords carried with no weapon layer. |
| Persistence and return | The complete decoded battle v4 data matched at the ready and post-action cold checkpoints for the axe, both swords and Unarmed. Field v3 cold saves advanced only ambient field state and the main seed. Bowie used EGRESS from the Broad Sword fork and returned to Map36. Fresh Continue displayed both promoted classes and preserved the two packs and Luke's 16 EXP. |

Controlled cases check the different terrain responses, weapon and ring contributions and
the decline of a Warrior Pride held by another member. The registered Pacalon copies lack
Warrior Pride. No ordinary promoted Randolf or native Warrior Pride fallback is claimed.
Some successful Godot exits emitted ObjectDB warnings. #1716 remains deferred.


## Sniper and Bow Knight ordinary reproduction

Mount `ranged-v1` from the registered `derived/issue1956/melee-v1`, the prepared retail
library at commit `52435b04f49a84a6a9ed4444620055a070648001` and the pinned source metadata.
The closed package has 145 files and 294,607,496 bytes, an 82,289-byte increase. It adds nine
walking strips and fifteen battle body frames. All 18 member/weapon bindings use body-only
animation. The source weapon selector is FF for Wooden, Iron, Steel and Robin Arrows, Assault
Shell and Great Shot. One package serves both locales.
[Weapon graphics table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/items/weapongraphics.asm) ·
[Mount command and private inputs](../content/presentation-assets.md#prepare-and-mount-retail-resources)

SNIP is source class 21. ACHR's Archer MOV rises from 5 to 6. BWNT is source class 25 and
keeps RNGR's Centaur Archer MOV 6. On a rough tile with 15% land effect, the two movement
costs are three and five half-move units respectively. The zero-based AI class ranks
SNIP/BWNT are 17/18 (Regular), 29/30 (Flying), 17/18 (Archer) and 18/17 (Mage).
The retained Wooden, Iron and Steel Arrows use range 2–2. Robin Arrow, Assault Shell
and Great Shot become legal and use 2–3. Selecting Unarmed preserves the carried ammunition
and restores range 1–1. [Class definitions](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/classes/classdefs.asm) ·
[Attack priorities](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/global/aipriority.asm) ·
[Item masks and range](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/items/itemdefs.asm)


Use the Chinese Pacalon castle slot and `derived/issue1956/ranged-review/Boot.ps1`.
Run `elric`, `janet`, `may`, `raise` and `road` in one journey. May already has her
formation flag, so Raise restores her live place. At the Caravan, give Rohde's Assault
Shell to Elric. Fork the Battle26 ready slot for `elric-shot`, `janet-shot`, `may-shot`,
`assault` and `bare`. Run `battle-cold` on the resulting slots. Continue the Assault Shell
fork to `egress` and `field-return-cold`. The handoff lists the exact seed and receipt names.

| Boundary | Observed result |
| --- | --- |
| Church and Raise | Elric ACHR 22 and Janet ACHR 26 became SNIP 1/0 and kept Steel Arrow. May RNGR 21 became BWNT 1/0 and stayed at 0 HP with Wooden Arrow. The rites preserved earned stats, packs and empty books. Raise for 210 gold restored 30 HP. |
| Steel Arrow and Wooden Arrow | Elric and Janet dealt 4 and 6 HP at distance two. Their adjacent targets were disabled with `target-range`. May dealt 1 HP at distance two with Centaur Archer MOV 6. All three projected their promoted field sprite and battle body and returned to Movement. |
| Assault Shell and Unarmed | Elric equipped the Assault Shell received from Rohde and dealt 14 HP at distance three. Janet removed her Steel Arrow and kept it carried. Unarmed dealt 1 HP at distance one. The scenes used the appropriate promoted bodies and hid the independent weapon layer. |
| English Great Shot | On the separate English castle history, May was raised and received Rohde's carried Great Shot through Caravan Give. Equipped as BWNT, it dealt 23 HP at distance three with the same body-only presentation. Its ready and post-action v4 data matched on cold re-save. |
| Persistence and return | Complete decoded v4 battle data matched on cold re-save at the ready and post-action checkpoints. Bowie used EGRESS after the Assault Shell fork and returned to Map36. Fresh field v3 Continue preserved the three promoted cards, books and packs. Elric retained his equipped Assault Shell and 17 EXP. Ambient field state and main seed advanced. |

Controlled archer cases decline a Silver Tank held by another member and preserve its
exact slot. Native Silver Tank fallback remains **Unknown**. BRGN special acceptance
stays with [#1957](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1957).

The first two road probes stopped at an unnecessary Relief request and an early Caravan
lookup. Both driver failures are retained. The Assault Shell run reached input and
published its after-action slot before native teardown exited `0xC0000005`. Independent
cold Continue compared complete battle data and completed EGRESS from that slot. Other
successful exits may report ObjectDB teardown warnings. #1716 remains deferred.


## Vicar ordinary reproduction

Mount `vicar-v1` from the registered `derived/issue1956/ranged-v1`, the prepared retail
library at commit `52435b04f49a84a6a9ed4444620055a070648001` and the pinned source metadata.
The closed package has 145 files and 294,653,990 bytes, a 46,494-byte increase. It adds six
prepared walking strips and six battle body frames. Eight member/weapon bindings reuse
the four mounted rod/stick layers for each palette. The selected weapon banks use
shared body palette slots 1 and 2. One package serves both locales.
[Mount command and inputs](../content/presentation-assets.md#prepare-and-mount-retail-resources)

The fixed source defines VICR 19 with HEALER MOV 5, no innate resistance and Counter 1/16.
Its zero-based attack-target ranks are 6 (Regular), 6 (Flying), 9 (Archer) and 26 (Mage).
The healing rule multiplies the raw spell power by 5/4 before capping to missing HP and
applies the full-recovery sentinel first. The existing ally healing contribution uses
the caster's current source class and the target's maximum HP. [Class data](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/classes/classdefs.asm) ·
[AI priority](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/global/aipriority.asm) ·
[Healing power and EXP](../../../docs/design/contracts/battle/spell-resolution.md#confirmed-heal-1-subset)

Start `sarah`, `karna` and `road` in one journey from a copy of the Chinese Pacalon
castle slot, using `derived/issue1956/vicar-review/Boot.ps1`. Fork the Battle26 ready slot
for `heal1`, `heal3` and `heal4`. Karna's physical `rod` witness starts from the
HEAL 3 after-action slot. Run `battle-cold` for each action and continue the HEAL 4
journey through `egress` and `field-return-cold`. The private handoff lists the exact
seed names and checkpoints.

| Boundary | Observed result |
| --- | --- |
| Church | Sarah PRST 40 and Karna PRST 28 became VICR 1 with 0 EXP. Sarah kept Short Rod, HEAL 4, DETOX 3, BLAST 4 and SLOW 2. Karna kept Guardian Staff, HEAL 3, BLAST 3 and BOOST 1. Both retained HP, MP and earned stats. The finite rite cue finished and Castle music resumed. |
| Sarah HEAL 1 and HEAL 4 | HEAL 1 spent 3 MP and restored 18 HP (Bowie 17→35 of 60), earning 10 EXP. The full-recovery HEAL 4 fork spent 20 MP and restored all 43 missing HP (17→60), earning 17 EXP. Sarah projected field sprite 3 and body 25 palette 1 with Short Rod. |
| Karna HEAL 3 and staff | HEAL 3 spent 10 MP and restored 37 HP (Bowie 18→55 of 60), earning 15 EXP. Karna projected field sprite 37 and body 25 palette 2 with Guardian Staff. The separate staff branch committed an adjacent Guardian Staff attack for 6 HP and returned to Movement. |
| Persistence and return | The ready and post-action v4 cold saves reproduced all decoded battle data for HEAL 1, HEAL 3, HEAL 4 and Guardian Staff. EGRESS from the HEAL 4 branch returned to Map36. Field v3 cold Continue kept both Vicar cards, books and loadouts, including Sarah's 17 EXP. |
| Vigor Ball | On the independent Chinese Elven Village Search history, Sarah PRST 37 carried the Vigor Ball in slot 3 (zero-based) with F221 set. Return by Caravan to Hassan, decline the special offer and confirm ordinary VICR. Sarah became VICR 1/0 with the same book and pack. Hassan Save and cold Continue preserved the held word and F221. |

Karna's controlled VICR growth learns BOOST 2 at stored level 10 (effective 30) and
AURA 1 at stored level 13 (effective 33). The controlled AURA 1 has range 0–3 and
radius 1. It restores up to 18 HP per eligible recipient in source-slot order and
uses the shared 25 EXP accumulator cap. Ordinary promoted acquisition and casting of
AURA 1 remain **Unknown**. BOOST 2 learning preserves the packed rank. Its battle effect
and campaign AURA 2–4 remain unbound.

The first staff pilot ended in defeat before Karna could attack. The first Hassan route
stopped at a blocked direct path. The accepted staff fork uses the retained HEAL 3 slot
and the accepted Hassan route retraces the established Caravan crossing. Both failed
runs are retained. Some successful native exits reported ObjectDB warnings. #1716
remains deferred.


## Wolf Baron and Phoenix ordinary reproduction

Mount the prepared retail library at commit
`52435b04f49a84a6a9ed4444620055a070648001` over the registered
`derived/issue1956/vicar-v1` base with the pinned source metadata. The closed `innate-v1`
package has 145 files and 294,702,724 bytes, a 48,734-byte increase. It selects six walking
strips, ten body frames and two portraits and binds two member growth rows. Each battle
body uses its own palette 0 and no weapon layer. One package serves both locales.
[Mount command](../content/presentation-assets.md#prepare-and-mount-retail-resources)

WFBR keeps STEALTH MOV 5 and changes counter odds from 1/32 to 1/8. PHNX moves from
HOVERING MOV 6 to FLYING MOV 7 and gains major FIRE/STATUS resistance. FLYING crosses
high sky and has zero land effect. HOVERING also has zero land effect on desert, while
retaining the source protection on grass. The zero-based enemy attack-target priority
ranks for WFBR/PHNX are 21/12 (Regular), 14/24 (Flying), 21/4 (Archer) and 13/29 (Mage).
[Class data](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/classes/classdefs.asm) ·
[Terrain table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/global/landeffectsettingsandmovecosts.asm) ·
[AI priority](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/global/aipriority.asm)

The PHNX body animation retains the packed selector 61. The current physical scene plays
the body frames. The separate original sound-wave scene effect has no physical playback
binding. [Original phoenix effect](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/phoenixattack.asm)

Start `gerhalt`, `peter`, `portraits` and `road` in one journey from the registered
Chinese Pacalon castle slot. Use `derived/issue1956/innate-review/Boot.ps1` and the `innate-v1`
package. Repeat `road` from the original castle slot for the unpromoted comparison.
Fork both ready slots for `reach`, and the promoted slot for `wolf` and `phoenix`.
The private handoff names the exact slots and the `battle-cold`, `egress` and
`field-return-cold` steps.

| Boundary | Observed result |
| --- | --- |
| Church and field | WFMN 29 and PHNK 31 each became level 1 with 0 EXP. Earned attributes, HP, MP, packed items and empty packed books persisted. Both stayed active. Each finite rite cue finished and Castle music resumed. Cold Continue displayed Gerhalt's portrait 49 and Peter's portrait 48. |
| Movement | From (4,25) on Peter's first Battle26 turn, the real preview exposed 41 destinations at HOVERING MOV 6 and 85 at FLYING MOV 7, including (11,25) in the promoted set. All 41 preceding destinations remained available in this comparison. Distinct high-sky and land-effect behavior has controlled engine coverage. |
| Gerhalt's attack | From one tile away he hit the Lesser Demon for 20 HP and gained 25 EXP. Body 30 palette 0 played attack sequence 30 with the weapon layer hidden. The battle returned to Movement. |
| Peter's attack | From one tile away he first missed, then hit the Lesser Demon for 28 HP. His two attacks earned 36 EXP in total. Body 28 palette 0 played attack sequence 28 with the weapon layer hidden. The battle returned to Movement. |
| Continue and return | Both Battle26 ready and post-action v4 cold saves matched the complete decoded battle data. EGRESS returned to Pacalon Map36 with both promoted classes. At both field v3 cold checkpoints, only ambient field scene state and the main seed advanced. Both portraits were visible after return. |

The first Gerhalt pilot ended early because its driver counted an allied healing reaction
as his attack before verifying an enemy target. The repeat required his committed
target, active attack animation and a matching physical result. The failed pilot log
is retained. Some successful Godot exits logged ObjectDB shutdown warnings. #1716
remains deferred.


## Ninja controlled reproduction and ordinary pilot

Mount `ninja-v1` from the registered `derived/issue1956/innate-v1`, the prepared retail
library at `52435b04f49a84a6a9ed4444620055a070648001` and the pinned source metadata.
The closed package has 145 files and 294,767,297 bytes, a 64,573-byte increase. It adds
three walking strips, six body frames, portrait 50 and both states of the two spell icons.
Eight admitted sword rows reuse the existing sword layers. Steel Sword shares Long Sword's
bank and color pair. The separate unarmed row has the same body 26 palette 0 and no weapon
layer. Later rare and cursed swords retain their existing equipment boundaries.
[Mount command](../content/presentation-assets.md#prepare-and-mount-retail-resources)

NINJ 27 changes REGULAR MOV 7 to STEALTH MOV 7. It has no class resistance and uses
critical 1/8 at the 50% bonus, double 1/16 and counter 1/8. Its zero-based target
priority ranks are 14 (Regular), 26 (Flying), 14 (Archer) and 20 (Mage). The source church
rite clears the worn weapon's equip bit. The remake uses that resulting pack for both
the candidate's appearance check and the commit. The ring stays worn and its stat
contribution is recomputed from the saved base. [Class data](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/classes/classdefs.asm) ·
[AI priority](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/global/aipriority.asm) ·
[Church weapon handling](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/church/churchactions_1.asm#L437-L459)

| Spell | Stored level | MP | Range | Radius | Power | Animation selector |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| KATON 1 | 4 | 6 | 1–2 | 1 | 12 | 33 |
| KATON 2 | 9 | 10 | 1–2 | 1 | 16 | 65 |
| KATON 3 | 25 | 12 | 1–2 | 0 | 42 | 97 |
| RAIJIN 1 | 14 | 15 | 1–3 | 2 | 18 | 39 |
| RAIJIN 2 | 19 | 18 | 1–3 | 2 | 26 | 71 |
| RAIJIN 3 | 32 | 20 | 1–3 | 0 | 58 | 103 |

Both families use NEUTRAL resistance at shift 8. KATON dispatches Blaze's 1/32 spell
critical and RAIJIN dispatches Bolt's 1/8. The existing modern effect playback uses the
pinned flame or lightning selector and variation without changing the damage element.
[Spell records](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spelldefs.asm) ·
[Spell effects dispatch](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/castspell.asm) ·
[Damage elements](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spellelements.asm)

The ordinary English Pacalon pilot raised fallen THIF 12 Slade with 42 EXP and an
equipped Short Knife. In one Battle26 repeat he moved and stayed in round 1, then
attacked the Dark Sniper for 1 HP and earned 1 EXP in round 2. The Sniper defeated him
in round 3. The saved battle returned to usable input with Slade at level 12, 43 EXP
and 0 HP. The pilot then stopped. There were 757 EXP to the rite at that point, with
300 more EXP after the rite to learn KATON 1, which costs 6 current MP. Growth increases
the MP maximum without refilling it. No reliable two-repeat path met the training budget.
Ordinary eligibility, promotion, learning and casting remain **Unknown**.


The ordinary pilot inputs and stopped slot are registered under
`derived/issue1956/ninja-review/ordinary-pilot`. Use the same handoff's separate
`Boot.ps1` for controlled reproduction. The controlled THIF 20 and NINJ 19 fixtures
arrange eligible state and the learned book independently of the recorded ordinary
journey. They use the existing church, movement, action, Save and Continue commands.

| Controlled boundary | Observed result |
| --- | --- |
| Rite and field | THIF 20 became NINJ 1/0. Short Knife changed from word 231 to bare 103 in slot 0. The Power Ring stayed worn as 147 in slot 1, Middle Sword stayed carried and the book remained empty. Slade's earned stats, 32 HP and 0 MP were retained. Cold Continue displayed portrait 50. |
| Unarmed and sword | Separate Battle26 forks used body 26 palette 0 and sequence 26. The unarmed hit dealt 1 HP. Equipping Middle Sword gave ATT 43 from unarmed ATT 35 and preserved the worn Power Ring. The sword layer was visible and the hit dealt 6 HP. Both returned to Movement. |
| Learned Magic | The separately arranged NINJ 19 used KATON 1 at distance 1 and RAIJIN 1 at distance 3. KATON spent 6 MP, caused 14 damage and displayed selector 33. RAIJIN spent 15 MP, caused 18 damage and displayed selector 39. Both presented NEUTRAL and returned to Movement. Controlled engine cases cover the other ranks, radius boundaries and insufficient MP. |
| Persistence | The ready battle checkpoints and the post-action sword, KATON and RAIJIN v4 cold saves matched complete decoded battle data. RAIJIN cold Continue succeeded with the producer's old local directory unavailable. EGRESS from the KATON branch returned to Map36 with NINJ and the packed book. The two field v3 cold saves differed from their loaded slots only in ambient field state and the main seed. |

The first controlled rite reported success but field Save rejected the old equipped
knife word. The commit now changes the carried loadout and progress together. The
first training observer counted empty turn-queue entries against its action budget
and stopped before any turn. Its corrected continuation used the same ready slot.
RAIJIN's pilot saved successfully and then exited with native code 0xC0000005. Its
cold replay passed. Some other exits logged ObjectDB warnings. #1716 remains deferred.
