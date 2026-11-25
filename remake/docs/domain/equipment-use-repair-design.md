# Equipment rules, Use, damage and Repair

## Whole-game equipment agreement (#2268)

**High rigor.** Equipment crosses stat refresh, physical construction, Item effects,
after-turn execution, custody, class admission, presentation and persistence. A permissive
reader alone would silently omit effects or destroy the wrong held copy. The outcome is all
original equipment definitions and their necessary consumers, including late weapons and
rings. The connected campaign still stops at the first Pacalon/Battle26 boundary. Later
recruitment, maps, smithing and other acquisition services remain with the campaign owner;
legal detached inputs can validate their equipment before those routes exist. No real save
or backup is an acceptance workspace. This section owns the implementation sequence;
the [complete item reconciliation](../content/campaign-items.md#whole-game-equipment-reconciliation)
owns individual rows. Slice A implements the static-equipment boundary below; whole-game support is still incomplete
and this does not close [#2268](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2268).

The current dependency is the accepted SORC **v3** package and its catalog, with independent
opening and Witch selections retained. Do not substitute rejected v2, earlier v1, or a removed
producer checkout. Reuse the existing Domain/Application/Content/adapter boundaries and
registered prepared assets. No new equipment platform, second item catalog, save format,
reference runner or per-item scenario framework is proposed. Ordinary source-backed definitions
must remain independent of the controlled witnesses used to test them.

### Audit result and decisions

**Confirmed (static).** All 128 item rows in the registered static input agree with pinned
`c834c652b6862bc5679fd7f69a38a7093206efc6` `data/stats/items/itemdefs.asm` for class mask,
range, price, category flags, Use expression and three ordered effects. There are 85 weapon
and nine ring definitions, including two weapons with mask NONE. The [source/ROM contract](../../../docs/design/contracts/party/item-definition-data.md)
owns their original packing/parity. The downloaded source uses LF while the registered source
digest was made with CRLF; direct parsed-field comparison agrees. No content mismatch was
resolved by changing provenance. The registered USA ROM passes `uv run sf2 rom verify`.

**Confirmed (accepted audit baseline before slice A).** Allied Content admitted 45 weapons and six
rings. Guardian91 and Thieve's Dagger106 were already offered by connected shops but rejected
by the weapon effect filter. Iron Knuckles29 and Great Axe39 pass that filter but have no
item-specific allied row in the selected Battle26 scene. Rare ordinary ATT weapons are also
excluded by the category filter. Equipment data, admission, an appearance row, actual gameplay
and natural custody are different claims; none implies the next.

| Execution boundary | Existing authority and actual missing behavior | Decision |
| --- | --- | --- |
| Equip and refresh | `BattleEquipment` stores weapon ATT/critical only; DEF/AGI/MOV use only `RingBonus`. `WornCurse` and `RefreshCurse` recognize rings only. `PrivateFieldPreparation.Weapons` filters mixed effects and almost all rare weapons. | Extend the existing weapon terms with signed stat adjustments and curse metadata, preserving source order and per-step clamps. Reuse equip, transfer, church cure, Repair, growth and restore. Include equipped weapon and ring in curse derivation/removal; do not add an independent persistent curse authority. |
| Physical action | `PhysicalCriticalRule.IncreaseCritical` exists; double/counter changes and Gisarme cutoff do not. `Sf2PhysicalAction` reads the defender's definition directly for counter chance, so changing only attacker projection is insufficient. | Derive both actors' effective prowess from live equipment. Reuse the physical RNG/reaction/death pipeline. Model SET_CRITICAL separately from Gisarme's item-specific cutoff. |
| After-turn | `BattleActionCommitter.NeedsAfterTurn` has no gear-recovery predicate; `BattleAfterTurnContinuation.AfterCounters` proceeds directly to poison/refresh. | Add recovery terms to admitted equipment and consume them in this existing continuation, including otherwise status-free Stay. Do not put recovery in stat refresh, selection, field walking or save reads. |
| Item Use | Offensive breakable and ATTACK/BOOST/SPOIT equipment paths exist. `Sf2ItemAction`'s healing path always consumes the slot; healing/cure equipment cannot safely be added to its table. | Reuse each spell's targeting/effect implementation with explicit Item origin and one selected-slot wear transaction. Add the missing healing/cure and status/damage bindings; do not invent another spell engine or put these items into the Magic book. |
| Class and appearance | `Sf2ClassRules.SourceClass` stops at MNST28. The current world has no growth records for late members20–29; RBT29/GLM30/RDBN31 have no named class mapping and no corresponding complete scene admission. | Admit the necessary original class/party records and prepared bodies for legal controlled equipment consumers, with correct unarmed behavior. This does not connect recruitment or later chapters. Existing class variants also need a complete member/class/item key check. |

**Confirmed (static original recovery order).**
`code/gameflow/battle/battleloop/processafterturneffects.asm`,
`@ApplyHolyStaffRecovery` through `@UpdateStats`, processes Holy Staff97 HP+3, Mystery
Staff100 MP+2, Life Ring124 HP+5, then poison and final refresh. It selects actual equipped
identities, caps to maxima, and emits message356/357 only for positive recovery. Holy Staff
plus Life Ring is two ordered operations, not one +8 message. The item-table HP_RECOVERY and
MP_RECOVERY labels alone do not establish this timing. Original natural play at these seams
remains **Unknown**; no new H3 run was made for this audit.

**Confirmed (static original cutoff).**
`code/gameflow/battle/battleactions/determinecriticalhit.asm` checks the equipped Gisarme
after a successful ordinary critical draw. DESOUL immunity prevents cutoff; otherwise it sets
cutoff and clears the normal critical marker. This is additional to the item's SET_CRITICAL6.
The implementation slice must follow its damage/death/message consumers and verify immune,
nonimmune, miss and counter cases before claiming closure. Existing DESOUL spell support does
not itself implement this physical branch.

The [growth contract](../../../docs/design/contracts/party/level-up.md) confirms Ninja Katana's
source `0x94 → 0x24` double-increase behavior, including loss of the counter field. The proposed
fidelity choice for this equipment work is to preserve that observed result and document it,
rather than silently correct it while implementing the item. Independent design acceptance
must explicitly retain or revise that choice; a normal probability increment cannot accidentally
decide it. Counter Sword uses `0x94 → 0xD4`; Gisarme uses `0x94 → 0x96`. Unused SET_DOUBLE and
SET_COUNTER handlers are not required by the 94 definitions.

### Finite implementation sequence

Main-gate opens and dispatches these five child outcomes serially after independent design
review. The estimates are engineering ranges, not runtime promises or permission thresholds:
roughly 4–8, 3–6, 3–6, 6–12 and 6–12 working hours respectively, including tests and review fixes.
Reassess at each handoff or if a new rule authority/save representation is required. The parent
stays open until all five outcomes and the final checklist pass. The accepted audit contained no
production implementation; slice A status is recorded below. Each child updates the rows it actually completes.

1. **Static equipment, signed stats and weapon curses.** Independently make current-shop
   Guardian91 add ATT22/DEF5 and Thieve's Dagger106 add ATT17/AGI5, with real appearance,
   removal and cold Continue. Admit ordinary rare ATT/critical gear and mixed positive/negative
   stats, plus White/Evil ring passive terms; keep unimplemented Use explicit until slice4.
   Generalize existing curse custody/cure/Repair to weapons and rings. Do not admit a recovery
   or prowess item while silently dropping its unique term. Dependencies: accepted current
   source/content only. Primary paths: `remake/src/Sf2.Remake.Domain/Battles/Rules/BattleEquipment.cs`,
   `remake/src/Sf2.Remake.Content/Scenarios/PrivateFieldPreparation.cs`,
   `PrivateBattleActionBindings.cs` in that directory, and affected existing growth/field stat
   consumers. Include `EnemyEquipment` and source physical admission without applying an
   ally-only class gate to enemies or adding weapon ATT twice to initialized enemy stats.
   Extend `BattleWeaponDefinition` with optional immutable terms so existing authored
   definitions remain compatible. Apply equipped effects in actual carried-slot order, with
   each operation's source clamp, rather than a fixed weapon-then-ring order or a summed delta.
   **Confirmed (static):** pinned `UpdateCombatantStats` initializes current stats, applies status
   effects, then walks from `COMBATANT_OFFSET_ITEMS` by `ITEMENTRY_SIZE`, calling `ApplyItemOnStats`
   for each nonempty equipped slot. There is no separate weapon/ring phase.
   [Pinned loop](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/updatecombatantstats.asm#L10-L44)
   · [Existing slot-order and clamp contract](ring-equipment-design.md#original-rules-and-evidence).
   Behavior acceptance includes swapping the weapon and ring's carried positions, checking
   each intermediate clamp at zero/cap edges, mixed stats under BOOST/SLOW,
   queue unchanged by AGI equip, MOV preview/undo,
   two cursed slots/cure, failed Give/Sell/Repair, valid removal, growth/promotion and restored
   words. One ordinary Guardian shop purchase/Equip and a legal controlled negative-stat weapon
   witness suffice; ordinary Dagger shop stock must still be admitted. Mount the relevant
   existing prepared weapon selectors through the current producer; do not rework all art here.
2. **Prowess and Gisarme physical behavior.** Depends on slice1's shared weapon definition.
   Own `EngineBattleState.cs`'s `PhysicalCriticalRule`, `BattleEquipment.Physical`,
   `Sf2PhysicalAction.cs` and `PhysicalStrikeRules.cs`, with the existing reaction/message
   consumers only where cutoff requires them. Admit Counter Sword68, Ninja Katana108 and
   Gisarme109, retain ordinary increased-critical swords/arrows. Cover both attacker and
   defender equipment, counter direction, misses, immunity, lethal cutoff, reward/drop/death,
   legal class refusal and unequip/restore. Use small deterministic actual-engine cases and
   one short native cutoff/counter observation; no per-item battles or new RNG harness.
3. **Equipped HP/MP recovery.** Depends on slice1. Own `BattleEquipment` recovery terms,
   `BattleActionCommitter.NeedsAfterTurn`, `BattleAfterTurnContinuation` and its existing
   scene/message wait representation; add a focused Domain recovery rule only if it keeps the
   numerical responsibility out of Application. Admit Holy Staff97, Mystery Staff100 and Life
   Ring124. Test status-free Stay, both slots, full resources, poison ordering, dead/absent/reserve
   actors, multiple queue entries, no RNG/EXP, remove and Continue. One ordinary native turn
   using a legal controlled holder must show ordered resource/message results. Saved settled
   state must not double-apply recovery on load.
4. **All equipment Use effects and durability.** Depends on slice1 and consumes accepted slices2/3
   without modifying their semantics. Own `PrivateBattleActionBindings`, `Sf2ItemAction`,
   `EquipmentItemUse`, `Sf2AttackSpellAction`, `Sf2SupportSpellAction` and the existing healing/
   cure rule and scene consumers. Cover ATTACK1, BOOST1, SPOIT1, BLAZE2/3, BOLT1/2, FREEZE3,
   MUDDLE1, DISPEL1, DESOUL1, HEALIN1, DETOX2 and AURA2. Reuse admitted spell definitions and
   their power/radius/resistance/progression rules. Equipment Use ignores MP and Silence,
   checks current class even when bare or broken, and performs effect before one wear check.
   Healing gear must not pass through unconditional consumption. Cover failed/no-effect,
   self/multi-target, canceled/stale action, wrong class, zero MP, damaged destruction and exact
   slot compaction; preserve effect RNG → wear → reward ordering. Retain mask-NONE player
   refusal for Iron Ball102/Taros Sword110 and enemy no-wear behavior. Field Use must follow
   the original field-item table/refusal owner rather than exposing every battle spell. Native
   witnesses: one healing/cure, one status, one offensive equipment use plus Repair and cold
   Continue; existing Black/Power/Protect/Indra witnesses are regression references. #2104
   retains unimplemented original spell choreography, never an excuse to omit the Use rule.
5. **Complete class/resource consumers and final reconciliation.** Depends on slices1–4.
   Own `Sf2ClassRules`, the source party/class binding in Content, `BattleSceneContentReader`
   and existing growth/appearance consumers needed for legal late equipment holders. Bind only
   the source combat operands and minimal detached records necessary for equipment, rather than
   connecting full late-character growth journeys. Reuse
   prepared assets via `src/sf2tool/remake_retail_assets.py` and its current family modules;
   `remake_private_package` owns closure publication. Do not implement future joins, smithing
   or maps. Verify all legal source member/class/item tuples, including explicit unarmed,
   weapon/body separation and innate no-layer classes. This includes late members and current
   members' missing legitimate variants, not just RDBN weapon use. Check all existing scene
   consumers, field/battle save restoration and late-class controlled ordinary input. Publish
   one complete reusable successor from the accepted package; preserve independent opening,
   Witch, catalogs, maps and audio. The [asset plan](../content/asset-inventory.md#whole-game-equipment-resource-plan-2268)
   supplies the resource bound and pilot conditions.

Paths above describe responsibilities for dispatch, not permission for concurrent writers or
an unchecked path glob. Each child declares its exact files after tracing the affected consumer.
The same `BattleEquipment`, Content bindings, engine test files, capability and item/asset owners
are shared serial surfaces. No slice consumes unmerged conclusions; rebase on accepted main.
Tests belong in the existing equipment, physical, after-turn, item, promotion and save behavior
test owners, with at most one focused new file per genuinely new behavior. Do not test parsers,
observation drivers or planners. Run affected engine behavior tests and `uv run sf2 verify adapter`
for implementation, then the clean-head dependency plan interpreted under the current engine
policy. Documentation-only design uses document checks, not those runtime gates.

### Whole-game acceptance and compatibility

- Reproduce the 94-row source reconciliation and use the final **production reader** to inspect
  every definition, exact mask/effect/Use term and all 439 legal class/item pairs (ALL expands
  to 32 classes; NONE to zero). Inert mask-NONE definitions are retained and player-refused,
  not silently omitted from the reconciliation. Count is a completeness cross-check, never
  a substitute for actual values or behavior. Add the legal member/class mapping before
  checking appearance keys; raw scene-row presence is insufficient.
- From the final mounted package, compare every expected member/class/item key and an explicit
  unarmed key against actual reader results in all 26 loaded encounter scenes. Rings must retain
  body/weapon identity; fists and other source no-layer items must not invent a weapon overlay.
  Verify required frames, palettes, animation selectors and projectile families. Preserve
  predecessor assets and compare complete dependency closure without producer-local inputs.
- Use short ordinary input for current acquisition callers and legal detached controlled
  setups for future items/classes. Observe new effect families, damage/destruction/unarmed
  refresh, actual body/weapon projection, Save and a fresh-process Continue. Also preserve a
  prior accepted field and battle save, both languages and the independent opening/Witch.
  No real saves, screenshots, grinding or whole-campaign replay are needed.
- Existing save words encode identity/equipped/broken and existing growth/applied-status fields
  encode stat bases; definitions should reconstruct new derived effects without a format change.
  If actual evidence requires a new durable field, stop that child at the interface decision,
  explain old-save compatibility and amend this owner before implementation. Do not loosen
  validation or mutate historical saves to make the comparison pass.
- Each child records completed failures, actual CI, exact head/tree, remaining original-runtime
  Unknowns and any unavailable original spell art. The final review must leave no equipment
  rule or necessary consumer deferred under the parent. Future acquisition remains a named
  campaign boundary; original animation-only gaps remain explicitly with #2104. Neither
  boundary licenses claiming natural acquisition or original animation from controlled play.

## Static equipment and weapon curses (#2270)

**Implemented slice A; independent integration pending.** The connected source catalog retains
all 94 definitions and their complete class masks without requiring present custody. The passive
reader admits 80 weapons and eight rings, including player-ineligible Iron Ball102/Taros Sword110.
Counter Sword68, Ninja Katana108 and Gisarme109 remain excluded until B; Holy Staff97, Mystery
Staff100 and Life Ring124 remain excluded until C. The source metadata is retained, not used to
silently discard their unique effects. New equipment Use remains D, and full legal class/appearance
closure remains E. No future acquisition, class roster, recruitment or main campaign advance is added.

`BattleWeaponDefinition.Stats` is an optional immutable ordered list of signed operations. Legacy
hand-authored weapon definitions still use `AttackBonus`. `BattleEquipment` applies statuses first,
then each worn slot's operations in actual pack order. ATT/DEF/MOV clamp after every operation;
AGI increase preserves the current high bit and caps the low seven bits, while decrease uses packed
subtraction/clamping. Bases, live queue and save format do not change. Initial enemy ATT receives
its difficulty adjustment then its equipped operations once; growth, removal and restore use the
same calculation. Weapon range and critical increase remain in the existing physical projection.

A worn cursed weapon or ring derives the existing CURSE status. Swapping/unequipping either cursed
category is refused. Cure removes the equipped bits from both categories while preserving damage;
Give, exchange, Drop, Sell and Repair reuse their existing selected-word checks. Bare damaged
admitted gear remains repairable, including equipment whose Use is deferred. The source reader
retains all metadata only for the connected field catalog; standalone early opening inputs keep
their bounded definitions and independent #2252 opening continues to work.

**Confirmed (remake).** Small engine cases cover both pack orders at zero/200 for ATT/DEF/MOV,
multiple operations on the same stat, BOOST/SLOW ordering, packed AGI, two cursed slots and Cure
payment/refusal, and weapon Repair. Existing equipment, ring, growth, promotion, persistence,
transfer and service behavior cases cover queue retention, movement preview/undo and refresh.
The final selected run passes 402 cases, with seven private-input tests explicitly skipped.
Production-reader observation checks 80/8 admitted definitions, six deferred unique effects,
all six WIZ/SORC/VICR rod consumers and THIF Dagger keys across 26 scenes, and older accepted field
and battle saves. THIF Dagger Equip changes ATT5→22 and AGI6→11; Unarmed restores both.

**Confirmed (ordinary remake input, controlled history).** The field witness copies the registered
SORC castle save and clears Kazin's last Medical Herb solely to make one purchase slot. It retains
his class, bases, flags, gold and position. Ordinary walking reaches the Pacalon merchant, buys
Guardian91 for 2,380 gold (8,747→6,367), and Equip writes `[85,0,17,219]`. ATT is40, DEF34 and AGI41.
A fresh Chinese Continue shows those values; Unarmed restores ATT18/DEF29 and keeps the carried
Guardian. A separate registered battle-ready copy replaces the bare Indra slot with Guardian,
then ordinary Equip/attack projects SORC body `ally15-palette0/frame0` and `weapon91/frame0`.
A legal controlled Demon Rod101 copy equips through the same input: ATT68, AGI31, CURSE4 and
`[85,0,17,229]`, then returns to settled Movement and saves. It does not claim a successful
Demon Rod attack or natural custody. Independent field, Guardian battle and Demon Rod battle
Continue/re-save preserve the entire decoded data, with sequence advancing exactly once.
The published package also passes a fresh-process battle Continue with no producer selection.

### Slice A reproduction and retained failures

Load the worktree-local private-input configuration and build the current Debug adapter with
`UseSharedCompilation=false` and `nodeReuse:false`. Select `derived/issue2270/equipment-v1`, the
unchanged `derived/issue2266/catalogs-v1/world.zh-CN.json`, independent #2252 opening and #2255 Witch.
The registered `derived/issue2270/review-v1` contains the bounded ordinary-input runner, labeled
controlled seeds, consumer source and exact receipts. Its Boot copies drivers into checkout-local
scratch and requires an explicit disposable save root; it does not write the registered inputs.
Choose fresh sample/journey names. This uses existing native input/state/projection, no screenshots.

```powershell
. ./local/private-inputs.ps1
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2270/review-v1'
& "$review/consumer/Verify.ps1" -Repo (Get-Location).Path
& "$review/Boot.ps1" -Repo (Get-Location).Path -Mode guardian -Sample $freshSample `
    -Journey $freshJourney -Seed "$review/seeds/field-seed.json" -SaveRoot $disposableSaveRoot
# Same journey, new sample/process, no seed: -Mode field-cold -Language zh-CN.
# Independent battle: -Mode strike -Item 91 (or 101) with battle-91-seed-v2.json
# (or battle-101-seed-v2.json), then -Mode battle-cold on that journey.
# Guardian removal: -Mode remove with the saved guardian-field-slot.json in a new journey.
```

Retained unsuccessful work is not acceptance: the initial build referenced retired `Contribution`
from three test owners; the first selected run had five failures (four obsolete cursed-item
rejection assertions and one wrong new AGI expectation). Corrected narrow runs pass. The initial
consumer caught absent late source definitions, then missing icon23, then an early independent
opening `source-indra-effect` dependency; the current catalog/asset boundaries correct these.
Native `guardian01` had a driver `Stock` lookup error despite exit0; `guardian-battle01` rejected
an inconsistent controlled loadout/progress copy before gameplay. `demon-battle01` did not produce
the requested successful-hit observation and exhausted its short route; `demon-battle02` explicitly
verifies Equip/settled save instead. Successful native runs exit0, but several retain ObjectDB
shutdown leak warnings (2–6 objects); this does not close or resume #1716. No screenshots were taken.
Original natural reach/timing and the unobserved late native equipment combinations remain **Unknown**.

## Accepted Use and Repair through first Pacalon

This High-rigor design for [#1952](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1952)
covers ordinary Battle Item → Use, the exact slot that survives or is destroyed, the merchant
Repair transaction, and cold Continue. Action construction, stat refresh and persistence meet at
the same loadout. A mismatch can lose gear, advance RNG, or charge gold without repairing the
selected copy. Power Ring, Protect Ring and Indra Staff use the implemented battle and Repair loop.
Indra Staff uses the admitted WIZ/VICR and [SORC progression](../application/church-promotion-design.md#current-rod-consumers). The accepted [ring slots](ring-equipment-design.md),
[status refresh](support-spell-status-design.md), [offensive item origin](offensive-items-design.md#remake-item-action),
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
| Black Ring (24) | Battle26 drop, once bit 10. WIZ/SORC only, including bare BLAZE 2 Use. Ordinary WIZ promotion is available. | Equip, BLAZE 2 Item Use, wear and Repair are admitted. [Worn equipment and physical curse](ring-equipment-design.md#black-ring-admission-and-physical-curse). |
| Indra Staff (92) | Pacalon weapon shop 10 sells it for 3,200. WIZ/SORC/VICR, weapon ATT +25, physical range 1, BREAKABLE, SPOIT 1 on Use. A legally promoted Kazin or Sarah can use it in the original at this frontier. | The remake admits Equip and battle Use from WIZ/VICR/SORC. The [SORC successor](../application/church-promotion-design.md#current-rod-consumers) supplies the shared current-rod appearance closure and its separate Equip/physical/cold witness; the Use/Repair observations below retain their original WIZ scope. |

**Open current-caller gap: Guardian Staff (91).** The current SORC v3 world stocks it in
`loc-54b1a` (Map06 `s2_entityevents_701.asm`), `loc-5d2c4` (Map25
`s2_entityevents.asm`) and `map2-entityevent7` (Map02 `s2_entityevents.asm`). It is not
merely a future source definition. Inspection of this package's world, ally start rows
and encounter placements found no initial held 91; arbitrary player saves are not covered
by that inspection. The retained Karna rite/action/cold witness instead holds Power Stick
89, equipped word 217. [Exact receipt evidence](../application/ordinary-promotion-classes.md#vicar-ordinary-reproduction)

The static definition is WIZ/SORC/VICR, ATT +22 and DEF +5. Current
`PrivateFieldPreparation.Weapons` filters it out through
`PrivateBattleActionBindings.HasSupportedWeaponEffects`, which accepts ATT/critical
weapon effects but not `INCREASE_DEF`. Thus `BattleEquipment.Choices` omits its slot;
a forced field/battle Equip command rejects with `equipment-slot`. An already equipped
loadout rejects with `equipment-nonweapon` in `BattleEquipment.ValidateLoadout`; direct
source physical admission uses `source-equipment-effects`. These are code-path findings,
not a new native Guardian observation. Mixed-effect weapon admission and its stat/save
closure belong to [#2268 current equipment reconciliation](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2268)
under this owner; #2266 does not implement it.

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
| SPOIT 1 | One living opponent at distance 1–2, radius 0. | Roll range 3 then add 3. Drain the lesser of that amount and the target's current MP. The recipient's MP gain is capped at its own max MP. Target then user react, preserving status; zero MP still takes the draw, reactions and +5 status EXP contribution. There is no HP damage, power scaling or status resistance gate in this effect. Bound to Indra Item Use. |

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
a bare cursed copy is not refused by that branch. Black Ring uses the same merchant transaction.
The original helper's intact branch writes `d1` while its output comment names `d2`.
The ordinary caller gates intact first and does not depend on that helper result.
[Shop Repair source](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/shop/shopactions.asm#L327-L435) ·
[Equipment type constants](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/sf2enums.asm#L1091-L1155) ·
[Damage-bit helper](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/itemstats.asm#L245-L273)

**Inferred.** An out-of-turn ATTACK target at applied count 1 can be recast to stored count
3 and still strike using count 1 until a real refresh. This follows the retained construction
write and the same reaction boundary established for BOOST. A reward level-up or holder
removal can refresh its own actor before the next turn. The natural original
ring damage → Repair → SRAM round trip remains **Unknown**. See the remake observations
below for the separate native and controlled boundaries.

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
4. **Indra consumer.** Use the accepted WIZ/VICR progression and bind the single weapon
   and its MP-transfer effect. Share Item admission, target centers, wear and Repair.
   Carry the ordered MP reactions through the existing scene and status EXP, including
   an empty enemy or a full recipient. Equip and Use check the live class mask. Repair
   uses the existing held-slot transaction at 800 gold.

The ring and staff slices use the accepted Item origin, Deals transaction, permanent
growth, shared support awards and target centers.

### Black Ring attack item and wear boundary

**Confirmed (fixed USA source).** BLAZE 2 targets at range 1–2 with radius 1 and base power 10.
Item Use takes these spell terms but skips learned-book, MP and Silence admission and uses the
base spell power. The ordinary Use class check applies to both intact and broken words, worn
or bare. The source scene visits its ordered targets and drop helper before applying breakability
once to the selected slot, before EXP. On the fresh branch a zero draw on range 4 marks the slot
broken. A successful use of an already broken copy removes that slot and refreshes the holder.
Broken equipped gear can be treated at Church and then repaired. For the one selected damaged
Black Ring, Repair charges 1,250 gold, clears only damage on the bare word, and refuses while it
is worn. Preserve the accepted battle carrier drop versus leader-cleanup distinction.
[BLAZE 2 row](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spelldefs.asm#L226-L232) ·
[Item spell dispatch](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/useitem.asm) ·
[Spell power adjustment](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/calculatespelldamage.asm) ·
[Post-target wear call](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/battleactionsengine_1.asm#L108-L130) ·
[Slot damage and destruction](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/breakuseditem.asm#L5-L45) ·
[Battle26 custody](../application/battle26-higins.md)

The existing attack consumable admitted only adjacent single-target damage and always removed
its slot. Support equipment embedded its effect definition in the wear result, which would
have attached support-scene behavior to BLAZE 2. Extracting the slot, word, and wear result into
`EquipmentItemUse` gives both callers one eligibility and wear rule. BLAZE 2 owns targets,
damage and drop order. Its item payment runs wear once after the ordered drop handling and
before reward. The same validator checks the full live word and slot using the shared wear
rule. Support effects keep their own scene routing, and both origins reuse the existing
equipment-wear message. Ordinary consumables still compact their selected slot after damage.

**Confirmed (controlled remake).** WIZ can use a bare or worn copy with zero MP, an empty book
and Silence. An unpromoted MAGE is refused at Use and Equip. With duplicate rings, only the
selected word gains damage or is destroyed. Reuse of a damaged ring spends no wear draw,
compacts the slot, and refreshes ATT and CURSE if it was worn. The existing Repair transaction
clears the damaged bit for 1,250 gold and refuses a worn cursed selection. Natural original
Black Ring wear and exact original presentation timing remain **Unknown**.

**Confirmed (current presentation).** Black Ring uses the normal item gesture and delivers the
BLAZE 2 damage, results and wear notices with zero MP cost. The equipment attack binding has no
source animation selector and `BattleSceneView` excludes `AttackItem` from the spell overlay.
The original BLAZE 2 overlay is a remaining presentation task under
[#2104](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2104).
[Equipment effect reuse](../godot/spell-presentation.md#follow-up-order) ·
[Scene consumer](../../game/src/Battles/BattleSceneView.cs)
[Attack-item admission](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2ItemAction.cs) ·
[Damage and payment ordering](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2AttackSpellAction.cs) ·
[Shared wear computation](../../src/Sf2.Remake.Domain/Gameplay/Sf2/EquipmentItemUse.cs) ·
[Repair transaction](../../src/Sf2.Remake.Application/Runtime/Exploration/ShopService.cs)

### Persistence

Retain the remake's existing normalized bit `0x0100` for broken. Original upper bits
are translated at import. The four words remain the pack authority. A worn Power Ring
changes 147 → 403 → 147 across damage and Repair, or is removed on broken reuse.
A worn Protect Ring changes 148 → 404 → 148. A worn Indra Staff uses 220 → 476 → 220.
Bare Indra uses 92 or 348. Normalize any source import at its boundary;
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
The first implementation uses those checks, with affected engine tests in
[`EquipmentItemTests.cs`](../../tests/Sf2.Remake.Engine.Tests/EquipmentItemTests.cs),
[`RingEquipmentTests.cs`](../../tests/Sf2.Remake.Engine.Tests/RingEquipmentTests.cs) and
[`ShopServiceTests.cs`](../../tests/Sf2.Remake.Engine.Tests/ShopServiceTests.cs).

## Implemented ring loop and ordinary reproduction

Register the [existing private game inputs](../application/pacalon-first-visit.md#continue-after-worktree-cleanup),
use the `issue1966/content/package` world and scene handoff with the accepted bilingual catalogs,
and copy one slot per attempt into an independent normal save namespace. Use the Battle26
`hit01` and Battle21 `protectcold01` lineages from the
[ring review](ring-equipment-design.md#implemented-behavior-and-ordinary-input). Read the running
host's state, input, projection and errors. Drive the normal command, target and acknowledgement
pages using the installed Confirm, Cancel and direction bindings.

- **Power Battle26, English.** From `hit01`, advance to Jaha's round 3 turn. Open Item → Use
  on his worn Power Ring and cancel at the target page before reopening and choosing Sarah.
  The ordinary item scene applied ATTACK1, projected Sarah's ATT 69 and +17 change, and gave
  Jaha 5 EXP. Jaha's MP held at 0 and the word stayed 147. The action returned to Movement.
- **Protect Battle21, Chinese input.** From `protectcold01`, advance to Peter's round 11
  turn. Use the worn ring on Peter and cancel/reopen before confirming the center. The
  ordinary item scene displayed AGI +10, DEF +13 and equipment damage, with 5 EXP and
  unchanged 0 MP. The first completed use changed slot 2 from 148 to 404. Save from
  settled Movement. Fresh v5 Continue restored 404 in slot 2, BOOST `0x2000` and resolved
  DEF 48 and AGI 35. The two histories used two intact actions in total. On a separate
  copy of the damaged slot, Peter reused it in round 12. The recast's ineffective message
  preceded the destruction message and 1 EXP. Slot 2 became 127 and play returned to
  Movement. Keep the original damaged checkpoint for Repair.
- **Repair, English and Chinese.** Cold Continue the damaged Battle21 slot and advance
  through ordinary turns until Bowie can cast EGRESS. Arrive in New Granseal (Map 6), face
  up at (16,11) and talk to the item merchant. Open Repair → Peter → slot 2. The quote is
  750 gold. Cancel returns to member selection at 13,244 gold. Reopen and confirm, leaving
  12,494 gold. Dismiss the result, Back out and leave to field control.
- **Save and cold Continue.** New Granseal in this return has no settled save anchor. Exit
  south via (20,31) to Map 73, board the Caravan at (55,61) and cross to Map 68. From
  (55,61), take Down 6, Right 2, Down 2, Right 1, Down 2, Right 1, Down 17, Left 2 and
  Down 1 to disembark at (57,27). Move left to (56,27) and use the host's Save → Save and
  Quit. For fresh Continue, detach the producing scratch, load the copied repaired
  slot with the registered inputs and driver dependencies, re-save, then move left.
  The field record retained 12,494 gold, Peter's class 9 and pack `[62,148,127,127]`,
  with the temporary effects cleared on EGRESS. English and Chinese Continue and
  re-save prompts matched their launch languages. Each fresh process accepted ordinary
  movement to (55,27). Re-saves preserved the allies, active roster, flags, Deals and Depot.

The controlled engine tests cover destruction after the effect with no extra wear draw,
last-applied ATTACK across v5 Continue, and Repair refusal/atomicity. A naturally divergent
ATTACK Continue remains **Unknown**. This loop uses the installed assets and existing save
formats. Black Ring's curse/Use and ordinary class promotion keep their respective owners.

## Indra Staff reproduction

Use the registered `derived/issue1952/indra-v1` package and the existing English/Chinese
catalog and input settings. Take the WIZ field seed
`derived/issue1956/wiz-review/receipts/kazin-rite01/promoted-slot.json`. Start via title
Continue in a fresh save namespace. This reuses the accepted promotion and leaves its
source slot untouched. [WIZ reproduction](../application/ordinary-promotion-classes.md#wiz-ordinary-reproduction)

```powershell
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$inputs = $env:SF2_SHARED_INPUT_ROOT
$package = Join-Path $inputs 'derived/issue1952/indra-v1'
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $package 'world.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $package 'main-scenes.json'
$env:SF2_PRIVATE_ENTRY_CONTENT = Join-Path $package 'entry/entry.json'
Get-ChildItem -LiteralPath (Join-Path $package 'opening-inputs') -Filter '*.json' -File |
    ForEach-Object { [Environment]::SetEnvironmentVariable($_.BaseName, $_.FullName, 'Process') }
```

Use the [private launcher and settings route](../application/pacalon-first-visit.md#continue-after-worktree-cleanup)
for the remaining registered inputs, Debug host and separate writable slot. Select the
Indra package paths above after the base input settings. The package mount adds four
prepared weapon rasters and eight appearance rows to the 1,654,870-byte Battle26 scene.
The resulting scene is 1,669,930 bytes. Other package files stay identical. The observed
slot copies total about 270 KiB. [Mount command and prepared inputs](../content/presentation-assets.md#prepare-and-mount-retail-resources)

1. Leave the castle south via (20,31). Face up at (9,10) in Pacalon town to speak to the
   weapon merchant. Sell one of Kazin's Herbs to free a slot, then Buy Indra Staff for
   him. The quote is 3,200 and the resulting pack is `[213,17,0,92]`. The English stock
   selection shows the MP-absorption description.
2. Take the [Ketto repeat route](../application/ordinary-promotion-classes.md#wiz-ordinary-reproduction)
   into Battle26 and save at Movement. In the observed suffix Sarah and the other
   allies hold near the starting area and Kazin advances north. On his round 2 turn,
   use Equip at the legal stop (7,22). The worn word is 220, physical range is 1,
   and ATT is 43. Choose Item → Indra Staff, cancel the target selection, reopen
   and confirm enemy-12 one tile away. The English scene displays WIZ body 21 palette 2
   and the staff. The enemy's MP goes 11 → 8. Kazin is already at his maximum of 55,
   and the message reports the 3 MP removed. The first intact use naturally damages
   the staff to 476, awards 5 EXP and returns to Movement.
3. Save at Movement and cold Continue in a fresh process. Re-save and compare the
   decoded v4 data. The observed data matched completely, including 476 and the enemy's
   current MP. Use a separate copy of the pre-repair slot to repeat Item → Indra Staff
   on the next Kazin turn. The Chinese scene showed enemy-13 MP 38 → 34, followed by
   the destruction message. The slot becomes 127, Kazin's unarmed body remains visible,
   ATT falls to 18, and the action returns to Movement.
4. On the Repair copy, advance to Bowie's normal EGRESS and return to Map36. The
   field v6 record still contains the worn damaged word. Return to the same Pacalon
   merchant, choose Repair → Kazin → slot 4, cancel the 800-gold quote once, then
   confirm it. The Chinese shop projected 5,914 → 5,114 and the repaired word 220.
   Leave, take the north warp at (19,0) to the castle, save and quit. Fresh Chinese
   Continue and re-save retained WIZ, the complete party packs, gold, formation and
   custody. A subsequent move crossed back into town.

The [engine tests](../../tests/Sf2.Remake.Engine.Tests/EquipmentItemTests.cs) cover bare
and worn legality, unpromoted refusal, cancellation, ordered MP reactions for a capped
caster and empty target, one damage draw and exact-slot destruction with no wear draw.
The [shop engine test](../../tests/Sf2.Remake.Engine.Tests/ShopServiceTests.cs) covers
the 800-gold transaction at exact funds with a duplicate staff. Battle Use was
observed on WIZ. VICR has controlled engine coverage, and SORC is a retained source
mask bit. Other SPOIT equipment and a learned Magic action are outside this admission.

## Black Ring reproduction

Use the same registered `indra-v1` and display catalog as above. The read-only sources are
the accepted Battle26 receipt for item 24 with once bit 10, the `wiz-review` post-promotion
battle slot, and the `Sf2IndraReview` final Map36 field slot. The following forks place that
receipt in a legal WIZ pack under controlled conditions. The title Continue, merchant,
Church and battle actions run through the ordinary player inputs.

| `SF2_RING_CASE` | Read-only starting record and changes on the disposable copy |
| --- | --- |
| `battle-damage` | `derived/issue1956/wiz-review/receipts/kazin-cast01/after-action-slot.json` (v4). Set `data.battle.cursor` to the round 2 queue entry for ally-4, already at (5,22). Set `mainSeed` to 131072. Set Kazin's MP to 0, ATT to 21, status to 0 and item slot 2 to bare word 24 in both `loadout.value.items` and `progress.value.sourceLoadout.value.items`. Include 10 in `data.battle.custody.onceFlags`. |
| `battle-destroy` | Same v4 source and changes, using worn damaged word 408, ATT 31 and status 4. |
| `battle-cold` | Byte copy of the actual `battle-damage` output. |
| `battle-description` | Copy either v4 starting fork. Open Item and highlight slot 2. The observer checks damage and target area, the equipped ATT bonus and the existing bilingual destruction warning when the damaged bit is set. It stops before using the item. |
| `field-church` | The final v6 `slot1.json` from [Indra Staff reproduction](#indra-staff-reproduction), registered under `derived/disposable-saves/Sf2IndraReview/6c5f4a5f1dd84a5b8333d9f0ccf03f1a/indra-review/sf2-opening`. Set ally-4's `data.party.allies` slot 2 to 408 and status to 4. Set `data.party.gold` to 8747 and include 10 in `data.party.onceFlags`. Move the settled `data.visit.actors` player slot 0 to (40,5), copying the 384-unit tile position into `motion.x`, `motion.y` and their respective destinations. Keep facing up. |
| `field-repair` | Same v6 input, gold and once flag as above, with bare damaged word 280 and status 0. Keep the original (20,30) landing, one tile from the castle's south exit. |
| `field-repair-worn` | Same landing and gold as the Repair fork, with word 408 and status 4. |
| `field-cold` | Byte copy of the actual `field-church` output. |

Use the [private launcher route](../application/pacalon-first-visit.md#continue-after-worktree-cleanup)
and the [Indra package selection](#indra-staff-reproduction). Mount the registered
`derived/issue2001/catalogs-v1/world.zh-CN.json` through `SF2_DISPLAY_CATALOG`.
For each fork use a fresh ignored receipt directory and an external disposable save
root and namespace. Copy the chosen source into slot 1 there. For a changed record, decode
the save envelope's base64 payload and edit the listed fields on that copy. Preserve the
UTC `savedAt` string (PowerShell 7.5 `ConvertFrom-Json -AsHashtable -DateKind String`).
Encode the edited UTF-8 payload and update the envelope's uppercase SHA-256 from those
exact bytes, using the [existing format](../../src/Sf2.Remake.Content/Persistence/OpeningSaveJson.cs).
For the cold cases start a fresh process and namespace using the published slot from
the preceding run. Keep app data, logs and temporary outputs local to the worktree.

Build the Debug adapter with the [locked SDK workflow](../development-and-verification.md#locked-net-workflow).
Use `{"formatVersion":1,"textMode":"instant","language":"en"}` or replace `en` with `zh-CN` in
the ignored settings file. With the isolated paths and the case selected above, launch:

```powershell
$env:SF2_RING_LANGUAGE = 'en' # Match the settings file. Use 'zh-CN' for Chinese.
$env:SF2_SAVE_CASE = $env:SF2_RING_CASE
$env:SF2_SAVE_REPORT = Join-Path $receipt 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --audio-driver Dummy --resolution 1920x1080 `
    --path remake/game --script res://probes/engine_black_ring_observation.gd -- --input-settings $inputSettings
```

The [observer](../../game/probes/engine_black_ring_observation.gd) checks the native exit, state
and projected menu and scene messages. Allow 45 seconds per serial process. Inspect the report
and stderr because a script error may occur before a check. No screenshots are used.

**Confirmed (bounded Godot input).** In Battle26, Kazin's English and Chinese Equip previews
showed Black Ring and ATT 31. Confirmed Equip changed 24 → 152 and ATT 21 → 31. Removing the worn
ring produced the localized refusal. BLAZE 2 Item Use dealt 12 HP to the Lesser Demon at zero MP,
then changed 152 → 408, awarded EXP, and returned control to the next ally. A fresh Continue
retained the worn damaged word and CURSE. In the separate broken-ring fork the Chinese cast
showed the destruction message, removed that slot, and cleared CURSE before the next input.
The Church quoted and charged 1,250, retained the damaged word as 280, and cleared CURSE.
After leaving the Church and saving at settled field input, cold Continue retained Kazin's
slot, status and the 7,497 gold balance. At the Pacalon weapon shop, the Chinese Repair
quote for a bare damaged copy charged 1,250 and restored word 24. The English worn-copy quote
and result refused Repair, retaining word 408, CURSE and the entire balance. Item detail shows
fire damage and the one-tile target area. A damaged copy shows the existing destruction warning
once in each language. An intact copy omits it. The earlier English detail observation failed
only its missing-warning assertion.

Observed attempts retained the harness failures for date normalization, an unsafe temporary slot
selection, packed-word comparison and outdated menu inputs. The legacy v3 promotion record's
Continue had already reported `field-visit-actor-origin` before the first player input. It
allowed Cure and later refused the v6 write for the same binding reason. This is the existing
legacy reader boundary for a reconstructed visit whose actor origin cannot be established for
the current snapshot. The v6 field fork allowed treatment followed by settled Save.
[Legacy reader and writer cutover](../application/opening-session-lifecycle.md#reader-and-writer-cutover) ·
[#2049](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2049)

Attempts to save immediately inside the Church could still meet pending ambient motion. The first shop path reached the correct merchant, but its observer
read the wrong projection key for the owner row. Successful processes exited zero. Some emitted ObjectDB shutdown
warnings under the existing [exit issue](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716).
