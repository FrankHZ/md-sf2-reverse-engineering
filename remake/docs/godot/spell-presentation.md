# Battle Spell Presentation

**Standard scope.** [#2104] tracks every USA spell-table family and rank through gameplay
admission, scene delivery and verification. Battles01–07 through first Map14 control set the
pre-voyage priority. Later private campaigns and controlled authored packages establish their own
boundaries. This inventory uses the existing rules, scene readers, presenters and private resource
publisher. Each implementation slice owns its own acceptance.

## Source and status

**Confirmed** for table membership and selector assignments. The [USA ROM manifest](../../../manifests/roms/sf2-us.json)
and the [byte-identical vanilla rebuild](../../../docs/research/reproducible-original.md) bind
the source to SF2DISASM revision `c834c652b6862bc5679fd7f69a38a7093206efc6` with
`VANILLA_BUILD=1`. Read `table_SpellDefinitions` at `0x176A6–0x1796E`.
It has 89 records in 44 families, including item, innate and special effect records.
`SPELL_NOTHING` is a sentinel. The animation setup/update tables also contain standalone
selectors such as arrows and spear projectiles. Their own consumers govern those effects.
[Spell definitions](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spelldefs.asm) ·
[Selector equates](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/sf2enums.asm)

For a static comparison, run these reads with `$upstream` set to the pinned checkout and
`$pin` to the commit above. Pair each `entry` with its next `animation`.
Read equipment and other action overrides before claiming a caller uses the table value.

```powershell
git -C $upstream show "$($pin):disasm/data/stats/spells/spelldefs.asm" |
    Select-String '^\s+(entry|animation)\s+'
git -C $upstream show "$($pin):disasm/code/gameflow/battle/battleactions/getspellanimation.asm"
git -C $upstream show "$($pin):disasm/data/stats/items/itemdefs.asm" |
    Select-String '^\s*(;\s*\d+:|useSpell\s+)'
```

In the selector column, v1–v4 means the logical variation numbers, in rank order unless stated.
The encoded variation bits are `00/20/40/60` hex in `sf2enums.asm`, and the enemy
origin flag is `80` hex. Use the equates and `sf2macros.asm:animation` for encoding.
The [original scene dispatcher](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/getspellanimation.asm)
reads the rank selector for an unsilenced learned cast or equipment Use. Non-equipment Use
keeps selector NONE. The battle action can override the table, including Prism Laser. Compare
actual caster, target faction and action kind at the caller boundary.

- **Original assets bound** records actual private playback within the named package. **Partial**
  identifies which assets, action pose or feedback the player sees and which effect remains open.
- **Modern presentation** records a deliberate delivery choice with its acceptance boundary.
  **Placeholder** records an effect or scene retained pending presentation work. An authored
  witness can prove real input, resource and result delivery while its effect remains a placeholder.
- **Unavailable** means the named production gameplay or content path is missing. **Unknown**
  marks evidence still needed, including natural reach when only a controlled state was exercised.
  The [prepared retail library](../content/presentation-assets.md#prepare-and-mount-retail-resources)
  has the raw spell graphics. Actual scene admission, composition and playback determine coverage.
  Human visual acceptance remains with [#1456] and the wider presentation review [#1616].

The private [attack bindings](../../src/Sf2.Remake.Content/Scenarios/PrivateAttackSpellBindings.cs),
[support bindings](../../src/Sf2.Remake.Content/Scenarios/PrivateSupportSpellBindings.cs),
[healing and item bindings](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs)
and [encounter reader](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)
establish which source operands enter gameplay. Current book, faction, target, MP, AI command and
encounter admission still control the available action. The
[scene continuation](../../src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs)
owns ordered gameplay delivery. [BattleSceneView](../../game/src/Battles/BattleSceneView.cs) and
[AttackSpellView](../../game/src/Battles/AttackSpellView.cs) establish the adapter paths. Ordinary
placeholder attack Magic hides the scene actors and scenery and draws element-specific shapes.
Ordinary support Magic and suppressed casts use cards and messages on black. Item gestures, number
of recipients and actual reaction results follow the committed action.

## Graphics variants

Prepared BLAZE1, BLAZE2, BLAST1, breath and wave rasters can select a complete optional HD
frame group from the battle scene. The light fairy uses a single group for body, wings and
dust. Partial groups use the complete Original art. Effect positions, mirroring, opacity
and Reduced Flash read the existing playback state. Higher density keeps the Original
draw extents and does not change cast or result delivery. Placeholder effects remain as
listed by rank below. [Binding contract](../content/asset-inventory.md#battle-raster-variant-binding-2177)

## Coverage by family and rank

### Recovery, support and retreat

| Retail rows | Original selector | Gameplay, package and scene | Remaining owner |
| --- | --- | --- | --- |
| HEAL 1–4 | HEALING_FAIRY v1–v4 | **Original assets bound** for private allied self/teammate casts. The private cast/idle, light fairy, live target and 77/113 cues are described in [HEAL spell scenes](#heal-spell-scenes). Native private HEAL1–3 settings cases and Sarah's Battle26 HEAL4 cover allied delivery. Enemy HEAL1 is observed with authored Mage/Cleric content, and a controlled private Battle19 Master Monk uses HEAL3 on itself. **Original assets bound** for the controlled pre-voyage enemy HEAL1 through the shared faction selector and evil-fairy group. | [Enemy healing acceptance](#enemy-healing-family-2227) covers a legal Dark Cleric recovery. Other higher-rank enemy playback and exact timing remain **Unknown** beyond named cases. [Enemy witness](../development-and-verification.md#enemy-spell-observation) · [Battle19 comparison](../application/battle19-repeat.md#controlled-comparisons) |
| AURA 1 | HEALING_FAIRY v1 | **Original assets bound** for admitted private allied content through the HEAL light-fairy path. Karna's allied ordered recovery has engine evidence and ordinary native campaign casting is **Unknown**. The private Battle19 Dark Bishop shares the faction selector and evil-fairy group; actual enemy AURA delivery remains **Unknown**. Authored AURA uses its own pulse, recipient cards and one EXP delivery. | [#2104] recovery follow-up after pre-voyage priorities. Reuse the same enemy fairy decision and verify actual area recipients when needed. [Current spells](../application/church-promotion-design.md#current-battle-spells) · [Battle19 private binding](../application/battle19-repeat.md) · [Authored AURA](battle-scenes.md#authored-aura-recovery) |
| AURA 2–4 | HEALING_FAIRY v2–v4 | **Unavailable** for private Magic. Authored AURA1–4 resolves ordered recovery with a **modern pulse**, with a native AURA4 witness. Healing Rain is already a private ordinary item with whole-side recovery and item presentation. White Ring/Goddess Staff AURA2 equipment Use would select v2 in the original. | [#2016] private Magic ranks, then [#2104] cast playback with actual scope, including AURA4's whole-side scan. [#1818] later equipment consumers. Keep Healing Rain on its item route. [Recovery items](../domain/recovery-items-design.md#ordinary-input-observations) |
| DETOX 1–3 | DETOX v1–v3 | **Partial** private Magic. The caster pose, timed reaction, recipient, cure/no-effect text and eventual turn release are present. DETOX tileset 7 is unmounted in this route. Pre-voyage rank 1 and later ranks 2/3 have action witnesses, including ordinary paid Battle26 no-effect at 2/3 and effective controlled engine cures. Field Magic and Antidote/Fairy Powder have distinct delivery. Rune Axe Use selects DETOX2 in the original. | [#2104] pre-voyage recovery follow-up after BLAZE2/BLAST1. Reuse the cast and result seam and verify a legal cure plus no effect. [#1965] owns the accepted rank-2/3 rules and equipped-curse limit. Effective ordinary battle stun cure remains **Unknown**. [Rank evidence](../domain/detox-ranks-design.md#ordinary-input-and-cold-continue) |
| DETOX 4 | DETOX v4 | **Unavailable** in private Magic. Uses an area in the original. Its area delivery requires a separate multi-recipient check. | [#2016] rule/admission, then [#2104] playback with the actual area recipients. |
| BOOST 1 | BUFF1 v1 | **Partial**. Ordinary private Karna cast has AGI/DEF, no-effect recast, cards and return on the support scene. Protect Ring uses the same effect rules with an equipment gesture, slot wear and result notices. | [#2104] later BUFF effect work. Share the asset setup with ATTACK where source operands agree and verify Magic/Item independently. [Support evidence](../domain/support-spell-status-design.md#battle26-ordinary-input-and-reproduction) · [Ring witness](../domain/equipment-use-repair-design.md#implemented-ring-loop-and-ordinary-reproduction) |
| BOOST 2 | BUFF1 v2 | **Partial**. Private Magic uses the shared cast scene, ordered AGI/DEF and ineffective messages, and return to battle input. Enemy SUPPORT still rejects the packed rank. | [#2104] retail BUFF1 v2 effect. [Battle26 observation](../domain/support-spell-status-design.md#boost-2-admission-and-observation) |
| SLOW 1–2 | DEBUFF1 v1–v2 | **Partial**. Both Sarah ranks have private Battle26 witnesses with actual AGI/DEF reactions, expiry/state rules and the black support scene. The original visual effect is absent. | [#2104] later DEBUFF work with DISPEL. [Support evidence](../domain/support-spell-status-design.md#battle26-ordinary-input-and-reproduction) |
| ATTACK 1 | BUFF1 v1 | **Partial** through Power Ring Use with item gesture, target ATT, durability and subsequent control. The current private Magic binding omits ATTACK. Battle25 Dark Soldier holds the spell and its ordinary AI rejects it. | [#2104] BUFF effect after the primary spell work. [#1818] other eligible equipment or learned callers as admitted. [Item-use rules and evidence](../domain/equipment-use-repair-design.md) · [Battle25](../application/battle25-north-cave.md) |
| DISPEL 1 | DEBUFF1 v1 | **Partial**. The authored silence witness covers ordered area results, success/no-effect, paid suppressed cast, expiry and later effective cast. Private Dark Smoke is a pre-voyage candidate and learned Kazin can use the admitted rank. Magic shows cards/messages on black without a cast gesture or DEBUFF art. Suppressed Magic correctly omits the effect. Physical silence uses its hit and notice. | [#2104] candidate pre-voyage support follow-up. Confirm ordinary legal Dark Smoke/ally use when relevant. Grand Cannon Use is later equipment work under [#1818]. [Silence observation](../development-and-verification.md#silence-observation) |
| MUDDLE 1–2 | DEBUFF1 v1 for both | **Unavailable** for private learned Magic. Each rank needs its own status, targeting and turn behavior. Giant Knuckles Use points to rank 1 in the source. | [#1967] gameplay and private casting, then [#2104] DEBUFF playback. [Hostile spell design](../domain/hostile-spells-design.md) |
| DESOUL 1–2 | DESOUL v1–v2 | **Learned Magic bound.** Controlled Battle26 Kazin casts both ranks with spell-8 icons and localized cast, instant-death, ineffective and reward messages on black. Dark Sword Use selects rank 1 in the source and has no binding here. | [#2104] owns original effects and timing. [DESOUL reproduction](../domain/hostile-spells-design.md#controlled-kazin-input-and-cold-continue) |
| SLEEP 1 | DEBUFF1 v1 | **Partial**. A controlled Tyrin 25 Battle26 slot exercises private Magic, both original icons, localized result/no-effect feedback and cold Continue with an active sleep count. The support scene uses its existing message presentation. Natural Tyrin progression to a cast remains **Unknown**. | [#2104] DEBUFF playback. [SLEEP boundary](../domain/hostile-spells-design.md#sleep-1-implementation-and-evidence) |
| EGRESS 1 | NONE | **Modern presentation**. Eligible retreat uses the same text, pulse, tone and fade for spell and Angel Wing, with different payment. Battle02 native witnesses cover cancellation, return and walking retry. Later encounters supply their own retreat destinations. | Preserve the accepted [retreat delivery](battle-scenes.md#authored-egress-delivery). Encounter admission belongs to [#1818]. Original visual/audio parity is **Unknown**. |

**Confirmed in the pinned setup/update source.** Both sides load healing tileset 6 and its
palette replacements. The enemy branch selects different tile offsets and initial phase 3,
compared with allied phase 7. The phase operand is not a palette selector. The shared source
update controls subsequent movement, mirroring and dust. [Enemy family evidence](#enemy-healing-family-2227)

### Attack Magic

| Retail rows | Original selector | Gameplay, package and scene | Remaining owner |
| --- | --- | --- | --- |
| BLAZE 1 | BLAZE v1 | **Original assets bound** in the prepared Battles04–07 scene mount, with current caster, target, weapon and backdrop. Controlled native Kazin Battle04 and Galam Mage Battle05 witnesses cover both sides. Packages without the mount show the placeholder. | Maintain [prepared BLAZE1](#prepared-blaze1-presentation). Later mounts and naturally reached casts are bounded by their encounter owners. |
| BLAZE 2 | BLAZE v2 | **Original assets bound** in the prepared Battles04–07 mount. Controlled Battle04 Kazin casts into a legal two-target area with the medium stacked flame and ten small meteors. Other packages retain the primitive effect. | [Prepared BLAZE2](#prepared-blaze2-presentation) covers the source side operands and bounded native cast. [#1926] owns Black Ring admission and Use. |
| BLAZE 3 | BLAZE v3 | **Placeholder** in admitted private Magic, including the Battle19 repeat Wizard binding. FBALL1, KATON2 and equipment Use share original art selection with distinct action rules. | [#2104] later flame variants after [#2105]. [#1818] other equipment and encounter callers. |
| BLAZE 4 | BLAZE v4 | **Placeholder** on learned private Magic. The rank hits one opponent. KATON3 shares the original selector and has its own neutral-damage rule. | [#2104] flame playback. [Battle26 witness](../development-and-verification.md#high-rank-elemental-magic-observation) |
| FREEZE 1–2 | FREEZE v1–v2 | **Placeholder** on admitted learned/AI attack paths. Current bindings use ice shapes. Private caster reach and ordinary casts are bounded by each encounter's admitted books. | [#2104] later ice effects. Coordinate with [#2016] player ranks and BREZAD/Freeze Staff only where the original caller shares the selector. |
| FREEZE 3–4 | FREEZE v3–v4 | **Placeholder** on learned private Magic. Both reach three cells. Rank 3 hits within radius 1 and rank 4 hits one opponent. Blizzard's BREZAD1 and Freeze Staff Use share the original v3 selector. | [#2104] ice playback and [#1818] equipment admission. [Battle26 witness](../development-and-verification.md#high-rank-elemental-magic-observation) |
| BOLT 1–2 | BOLT v1–v2 | **Placeholder**. Bowie BOLT1 in Battle26 and Taros Sword BOLT2 enemy Use in Battle18 have actual private combat witnesses. The lightning presenter is primitive. Taros's item-origin attack uses zero MP. Its class mask prevents ordinary player Use. | [#2104] later lightning effects. Keep actual cast versus sword origin, recipient order and payment. [Bowie witness](../application/church-promotion-design.md#current-battle-spells) · [Taros witness](../application/taros-caravan.md) |
| BOLT 3–4 | BOLT v3–v4 | **Placeholder** on learned private Magic. Both reach three cells. Rank 3 hits within radius 2 and rank 4 hits one opponent. RAIJIN2/3 have independent neutral rules and share the original selectors. THUNDR1 uses v3 in the source table. | [#2104] lightning playback. [Battle26 witness](../development-and-verification.md#high-rank-elemental-magic-observation) |
| BLAST 1 | BLAST v1 | **Original assets bound** in the prepared Battles04–07 mount. Controlled Sarah PRST level 10 casts on one adjacent enemy with current actors and backdrop. | [Prepared BLAST1](#prepared-blast1-presentation) owns the tileset-15 fragments, slashes, cues and observations. |
| BLAST 2–4 | BLAST v2–v4 | **Placeholder**. All three ranks bind to the private wind rule. Battle19 repeat admits Dark Bishop BLAST3; Battle22 observes Sarah's BLAST4 single-target use. | [#2104] later wind variants after [#2106]. Compare their actual area/single-target behavior and setup variants. [Battle19](../application/battle19-repeat.md) · [Battle22](../application/desktop-battle22.md) |
| KATON 1–3 | BLAZE v2–v4 | **Placeholder**. All three ranks bind to distinct learned neutral damage. Controlled private NINJ rank-1 input shows selector 33 and primitive fire; other ranks have engine coverage. | [#2104] reuse compatible art after flame variants. Preserve the recorded neutral rules. [Ninja evidence](../application/ordinary-promotion-classes.md#ninja-controlled-reproduction-and-ordinary-pilot) |
| RAIJIN 1–3 | BOLT v2–v4 | **Placeholder**. All three ranks bind to distinct learned neutral damage. Controlled private NINJ rank-1 input shows selector 39 and primitive lightning; other ranks have engine coverage. | [#2104] reuse compatible art after lightning variants. Preserve the recorded neutral rules. [Ninja evidence](../application/ordinary-promotion-classes.md#ninja-controlled-reproduction-and-ordinary-pilot) |
| DAO 1–2 | DAO v1–v2 | **Unavailable**. Separate SORC summon. | [#1957] playable SORC dependency; [#1369] summon gameplay and [#2104] effect at first admitted caller. |
| APOLLO 1–2 | APOLLO v1–v2 | **Unavailable**. Separate SORC summon. | [#1957] playable SORC dependency; [#1369] summon gameplay and [#2104] effect at first admitted caller. |
| NEPTUN 1–2 | NEPTUN v1–v2 | **Unavailable**. Separate SORC summon. | [#1957] playable SORC dependency; [#1369] summon gameplay and [#2104] effect at first admitted caller. |
| ATLAS 1–2 | ATLAS v1–v2 | **Unavailable**. Separate SORC summon. Enemy HEAL rank selection can read an ATLAS operand as part of its original rank-probing behavior. | [#1957] playable SORC dependency; [#1369] summon gameplay and [#2104] effect at first admitted caller. |

### Item, innate and special rows

| Retail rows | Original selector | Gameplay, package and scene | Remaining owner |
| --- | --- | --- | --- |
| SPOIT 1 | ABSORB v1 | **Partial** for admitted Indra Staff Item Use. It shows the gesture and ordered MP drain/gain notices; original absorb playback is absent. | [#2104] later equipment effect with [#1818] for later SPOIT equipment. [Indra witness](../domain/equipment-use-repair-design.md#indra-staff-reproduction) |
| HEALIN 1–3 | HEALING_FAIRY v1–v3 in table, NONE for consumable Use | **Original assets bound** for the ordinary private Herb scene's item gesture and recovery. Seed and Drop share the admitted HP item consumer, with a bounded Seed supply witness. Holy Lance equipment Use reads the actual selector. | Preserve [item recovery](battle-scenes.md#medical-herb-scenes). [#1818] Holy Lance and later item admission, then [#2104] shared fairy playback if applicable. |
| FLAME 1–3 | FLAME_BREATH v1–v3 | **Unavailable** for these enemy spell rows. Kiwi's private converted Attack has separate assets and rules. | [#1818] first enemy caller and [#1369] rule admission, then [#2104] breath playback. |
| SNOW 1–3 | SNOW_BREATH v2–v4 | **Unavailable** for these enemy spell rows. FREEZE already has a separate Magic path. | [#1818] first enemy caller and [#1369] rule admission, then [#2104] breath playback. |
| DEMON 1–2 | DEMON_BREATH v1–v2 | **Unavailable** for these enemy spell rows. | [#1818] first enemy caller and [#1369] rule admission, then [#2104] breath playback. |
| POWER 1, GUARD 1, SPEED 1, IDATEN 1, HEALTH 1, HANNY 1, BRAVE 1 | NONE for each | **Modern delivery** in Field/Caravan, with item result and actual stat/level/growth changes; outside the battle close-up. Ordinary Water, Milk and Chicken Use remain **Unknown**, with rule tests. | Preserve the [permanent growth item scope](../domain/permanent-growth-items-design.md). Any additional caller needs its own evidence under [#1818]. |
| B_ROCK 1 | BURST_ROCK_EXPLOSION v1, also chosen by the action override | **Unavailable** in the connected campaign. The authored laser path excludes special Burst Rock targets and their action. | [#1369] and [#1818] own special action and encounter admission. [#2104] effect at first admitted caller. [Special-mechanics design](../domain/special-mechanics-design.md) |
| LASER 1 | NONE in table, PRISM_LASER by action override | **Modern presentation**, finite authored ray with charge, fire, reactions/deaths and continued control. Private Battle36 and original visual/audio effects are outside that acceptance. | [#1370] and [#1818] private special/encounter admission, then [#2104] review desired private presentation. [Authored laser](battle-scenes.md#authored-prism-laser) |
| POWDER 1 | NONE | **Partial** for private battle item cure with ordinary gesture and notice. Field/Caravan Use has its own status and feedback. | Keep this item separate from DETOX graphics. [Fairy Powder and higher cure boundaries](../domain/detox-ranks-design.md) |
| G_TEAR 1–2 | NONE | Rank 2 **partial** through admitted Fairy Tear item gesture and MP recovery notice, including ordinary Battle26 input. Rank 1 has a table row and no known retail item binding in `itemdefs.asm`. | Rank 1 live caller remains **Unknown**. Reuse the existing item route only if a caller warrants it. [Fairy Tear evidence](../domain/recovery-items-design.md#ordinary-input-observations) |
| FBALL 1 | BLAZE v3 in table, NONE for consumable Use | **Original item gesture bound** for Shining Ball with ordinary private damage, custody and return. | Preserve the distinct item route with [#2104] and the [offensive-item owner](../domain/offensive-items-design.md#ordinary-remake-observations). |
| BREZAD 1 | FREEZE v3 in table, NONE for consumable Use | **Original item gesture bound** for Blizzard with ordinary private damage, custody and return. | Preserve the distinct item route with [#2104] and the [offensive-item owner](../domain/offensive-items-design.md#ordinary-remake-observations). |
| THUNDR 1 | BOLT v3 in table, NONE for consumable Use | **Unavailable** as Holy Thunder private Item Use. | [#1818] item rule/admission. Reuse the ordinary item scene when admitted. |
| AQUA 1–2 | BUBBLE_BREATH v1–v2 | **Placeholder**. Both enemy ranks have private gameplay and primitive bubble playback. Kraken Battle16 and Willard Battle23 cover the changed reach and enemy action in their own accepted scopes. | [#2104] later breath effect. Preserve rank-specific AI and reaction behavior. [Kraken](../application/kraken-battle.md) · [Willard](../application/willard-burrow-battle23.md) |
| KIWI 1–4 | FLAME_BREATH v3 for all | **Partial** original assets. The optional MNST mount supplies the dedicated cast sequence and two prepared Flame Breath tiles. A controlled private Attack conversion projects the modern particle stream. All four ranks differ by level/power and share the visual path. | [#2104] later breath group can reuse the mounted assets when appropriate. Natural MNST promotion/breath in the connected pilot and exact particle motion remain **Unknown**. [Controlled Monster pilot](../application/ordinary-promotion-classes.md#monster-controlled-reproduction-and-ordinary-pilot) |
| SHINE 1 | NONE | **Unavailable**. Right of Hope from the Fairy Woods trial is outside the current campaign. | [#1818] trial/item caller, with the accepted field/item delivery boundary. [Flag and receipt](../content/campaign-flags.md) |
| ODDEYE 1 | ODD_EYE_BEAM v1 | **Unavailable** in the connected campaign. | [#1818] first encounter, [#1369] action rules and [#2104] beam effect. |

## Pre-voyage dispatch

1. **P2 [#2105], BLAZE2.** See the [prepared mount and observations](#prepared-blaze2-presentation).
2. **P2 [#2106], BLAST1.** After [#2105] acceptance, use learned Sarah from a legal pre-voyage
   state (PRST learns BLAST1 at level 10 in `allystats01.asm`), 2 MP and one adjacent opponent.
   Its source radius is zero. Prepare its own tileset-15 wind/dust composition, including the
   v1 side selection in `spellanimationSetup_Blast` and `spellanimationUpdate_Blast`.
   Later enemy BLAST ranks have their own encounter and package checks. Reuse the playback and
   publication interfaces from the accepted slices. The two issues have independent gameplay rules.
   [Blast setup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/blast.asm) ·
   [Blast update](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/update/blast.asm)

For each, use the [prepared BLAZE1](#prepared-blaze1-presentation) lifecycle and an isolated fresh
private package. Read the live projection, token and errors through one short ordinary-input
controlled cast on legal state. Check the mounted art and current scene identities, one payment,
each actual reaction, release, and the existing Reduced Flash, skip and held-rate behavior at their
applicable waits. Use modern timing within the accepted playback contract. Retain exact source
composition and the results of any failed observations. Natural pre-voyage learning/casting and
foreground visual acceptance remain **Unknown** where no named witness covers them.

## Follow-up order

- **P2 after the two child issues.** [#2104] retains first-use review of pre-voyage recovery and
  support. Group enemy HEAL/AURA fairy art with its faction-specific placement and one actual
  enemy recovery. Track DETOX's art separately from its cure semantics. Use DISPEL's actual
  status/suppression consumer for the first DEBUFF decision. These are candidate presentation
  slices. [#1456] governs foreground acceptance and [#1616] the wider design review.
- **P3 for current later content.** [#2104] groups the remaining elemental variants and shared
  KATON/RAIJIN art, then BUFF/DEBUFF and private AURA rank coverage as actual callers need them.
  [#2016], [#1967] and [#1926] own missing gameplay and item admission. Keep item gestures where
  the original selector path calls for them. Schedule equipment effect reuse at its first legal
  consumer.
- **Campaign-dependent.** [#1818], [#1369] and [#1370] own first-use chapter or action admission
  for future breaths, summons and special attacks. [#2104] retains effect coverage until the
  selected caller exists and the accepted original assets or modern decision can be named.

[#1456]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456
[#1616]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1616
[#1369]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1369
[#1370]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1370
[#1818]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1818
[#1926]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1926
[#1957]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1957
[#1965]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1965
[#1967]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1967
[#2016]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2016
[#2104]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2104
[#2105]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2105
[#2106]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2106

## Prepared BLAZE1 presentation

**Standard scope (#2068).** Battles04–07 can mount BLAZE1's small flame from the prepared
private effects library. The production scene shows the caster's cast pose, the current target,
the equipped weapon where applicable, and the selected background and ground. Ally and enemy
casts place the flame on the recipient's side. The cast pose cues the flame and cast sound.
Completion waits for both the pose sequence and the 48-tick flame. Reduced Flash holds one flame
frame with a smooth fade. The existing hit/coin message setting can suppress the coin notice
while the animation and spell damage notice retain their own waits. Session rules still deliver
cost, ordered reactions, death, rewards and next control.

**Confirmed source identity.** USA retail ROM `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
and pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` bind `spelldefs.asm`
BLAZE1 to animation selector 1. `spellanimationSetup_Blaze` loads tileset 2, whose table entry
is `SpellGraphics02` at `0x1BF3E6`. `LoadSpellTileset` replaces indices 9, 13 and 14 of
`palette_BattlesceneBase` (`0x198A8`) with `$022E`, `$028E` and `$08EE`. The registered 7,104-byte
tiles and base palette compare byte-for-byte to the decoded ROM stream and ROM palette.
`table_1A5E4` and `table_1BE2A` select column-major 2×4 sprites at zero-based tiles 33 and 41.
Each variation-1 entry duplicates the other at the same position. The adapter draws a single
16×32 flame at (72,112) or (176,128) on the 256×224 local canvas. The source cast sound is 77.
Fade, frame cadence, reaction motion and delivery on the Godot clock are modern presentation choices.

`createbattlesceneanimation.asm:battlesceneScript_PerformAnimation` selects `BATTLEANIMATION_ATTACK`
for ordinary spellcasters, including Kazin and Galam Mage. `GetAllyAnimation` resolves Kazin's
class sequence 4 and `GetEnemyAnimation` resolves Galam Mage's species sequence 36. Battle04's
Kazin row exposes sequence 4 through `idle`, identical to its `attack` sequence. The registered
Battle05 Galam Mage row exposes sequence 36 through `attack`. Battles06–07 provide a `cast` alias
with exactly the same operands as `attack`, including trigger 2 and the frame 2→3 pose. The adapter
prefers a published `cast` row and otherwise reads the existing ordinary pose. The pose's trigger
cues the modern flame interval.

**Publication.** Select the registered fixed-backdrops package as `$base`, its prepared `retail-v1`
asset library as `$library`, and a fresh ignored absolute output `$candidate`. Load worktree private
selections in the same shell. The mount reads two existing tile frames and reuses the closed-package
publisher. An omitted `blaze-1/` namespace retains the legacy effect. A partial pair, extra frame
or wrong dimensions fails scene admission.

```powershell
uv run sf2 rom verify
uv run python -m sf2tool.remake_retail_assets --root $library --base $base --mount $candidate --family blaze
```

To reproduce the source comparison, follow `p_pt_SpellGraphics` at `0x1B800C` through table slot 2.
Decode its stream at header offset +8 with `decode_stack_compressed`, using
the first header word as expanded length. Compare with `effects/spell-2.4bpp`, then compare the 16
palette words at `0x198A8` with `effects/base-palette.json`. Apply the three replacement words from
the header and use the named tile operands above to compare the two mounted rasters. The selected
package has 142 byte-identical dependencies and four scenes equal to their base values after
removing the two new rasters from each. The logical size increases by 4,496 bytes, and the largest
changed scene is 879,858 bytes against the 4 MiB reader limit. The publisher hardlinks unchanged
inputs on the same volume and copies changed scenes before writing. A cross-volume copy needs about
315 MB. Complexity is linear in the selected package's file closure, with worktree-local scratch.

**Native reproduction.** Select the candidate `world.json`, `main-scenes.json`, `entry/entry.json`
and six `opening-inputs` files in their ordinary `SF2_PRIVATE_*` environment selections. Use a fresh
worktree-local report/runtime and a disposable task-specific external save namespace. Provide
`SF2_BLAZE_START` pointing to an ignored controlled-start JSON with these values:

```json
{"start":{"battle":"battle-4","activeAllies":["ally-0","ally-4"],"actors":[{"actor":"ally-4","position":{"x":4,"y":7}},{"actor":"enemy-1","hp":1,"position":{"x":6,"y":7}}]}}
```

The existing controlled startup mounts Battle04 with Kazin's normal learned BLAZE1 and stages one
legal target in range. The 1 HP target exercises the actual kill and rewards within one cast.
For each run set `SF2_OBSERVATION_OUTPUT` to a fresh file and select a version-1 input settings file
with `suppressBattleHitCoinMessages` and `reducedFlash` false, then only the former true, then only
the latter true. Set `SF2_BLAZE_SKIP_MESSAGES` and `SF2_BLAZE_REDUCED_FLASH` to matching `0`/`1` values
for the observer. Build the locked Debug adapter before launching the installed Godot console.

```powershell
& $env:GODOT_BIN --headless --audio-driver Dummy --rendering-method gl_compatibility --fixed-fps 60 `
    --resolution 1920x1080 --path remake/game --script res://probes/engine_prepared_blaze_observation.gd -- `
    --input-settings $settings
```

**Confirmed in the remake.** All three runs select and confirm BLAZE1 with ordinary input. Normal
and coin-skip play both flames, Reduced Flash plays the first. The read-only projection sees
Kazin's cast frame, Galam Soldier, rod, background, ground and the source flame. Cast cue 77
starts once on the animation token. Early Confirm
retains the animation token. The spell deals 6 damage, MP changes 13→11, Kazin gains 49 EXP and
one kill, and gold changes 60→110. The next actor, Bowie, opens a new action menu in the same
session. The coin notice is visible in normal and Reduced Flash and passes without input in skip.
The spell result remains readable in all three. The recordings cover actual resource binding and
timed playback. Headless exit reported ten ObjectDB instances after control returned.

**Enemy-side reproduction.** With the same private package and observer, set `SF2_BLAZE_ENEMY=1`,
`SF2_BLAZE_SKIP_MESSAGES=0`, `SF2_BLAZE_REDUCED_FLASH=0`, and use normal input settings. Point
`SF2_BLAZE_START` at a fresh ignored controlled-start JSON containing these values:

```json
{"start":{"battle":"battle-5","activeAllies":["ally-0"],"actors":[{"actor":"enemy-2","position":{"x":9,"y":6}}]}}
```

**Confirmed in the remake.** Battle05's SENTRY Galam Mage moves (9,6)→(5,5) and casts BLAZE1
at Bowie (3,5) through the ordinary enemy decision. Both frames draw at (176,128) with
`enemy36/frame3`, `ally0-palette0/frame0`, the equipped sword, `background8` and `ground8` visible.
The session records one cast, one MP payment (5→3), one HP change (12→6) and one enemy action
commit. Early Confirm retains the cast token. Bowie opens a new action menu in the same session.
The observer reports no session, scene or audio error and exits 0. Eight ObjectDB instances are
reported at exit. Original timing and foreground visual acceptance (#1456) remain **Unknown**.

## Prepared BLAZE2 presentation

**Standard scope ([#2105]).** Private Battles04–07 mount BLAZE2 for learned Magic. The same
cast pose, weapon, backdrop, cast cue 77 and completion token used by BLAZE1 now carry a
medium ground flame and ten small falling meteors with impacts. Subsequent reactions show the
actual recipient in order. A missing rank-2 mount uses the existing primitive effect.
Packages with a rank-2 namespace require all six correctly sized rasters.

**Confirmed source composition.** The BLAZE2 row in `spelldefs.asm` uses
`BLAZE|VARIATION2` (selector `0x21`), 6 MP, range 1–2, radius 1 and power 10.
`table_1A594` and `table_1A5BC` select ten small meteors and the following side operands.
The flame is drawn as a 24×48 raster made from a 3×4 top and 3×2 bottom at their original
relative positions. The table's tile indices are zero-based below.

| Branch | Flame origin | Bottom origin | Meteor entry X window start | Meteor direction and art |
| --- | --- | --- | --- | --- |
| Ally casting at enemy | (68,96) | (68,128) | 80 | Leftward, horizontal flip |
| Enemy casting at ally | (172,112) | (172,144) | 48 | Rightward, original orientation |

`table_1BE2A` uses top/bottom tiles 49/61 and 67/79. `table_1BE42` uses tiles 149 and
153 (16×16). `table_1BE52` and `table_1BE5A` use tiles 157 (24×24) and 166 (24×16)
for the small impacts. The second impact frame sits 8 pixels lower. The original update chooses
meteor X within a 96-pixel window and starts impact at or below Y 120 in local coordinates. In the remake, the
projector deterministically spreads ten meteors over 1.35 seconds from the existing cast-pose
trigger. Reduced Flash holds the first flame, meteor and impact frame and retains all ten
trajectories. The adapter uses its own motion, fade and scheduling with no palette flash.
The ally-side rank-2 branch was compared statically with the original side table.
[Blaze setup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/blaze.asm) ·
[Blaze update](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/update/blaze.asm)

**Publication.** Load the private worktree selections and select the prepared #2001 retail library
as `$library` and the accepted #2068 BLAZE1 package as `$base`. Choose a fresh absolute
`$candidate` under worktree `local/` for validation or the registered shared `derived/`
root for handoff. Reuse the package's current `world.json` and scene selections.

```powershell
uv run sf2 rom verify
uv run python -m sf2tool.remake_retail_assets --root $library --base $base --mount $candidate --family blaze-2
```

Compare tiles and palette to the ROM via the [tileset-2 procedure](#prepared-blaze1-presentation).
Compare each mounted raster with the column-major tile operands above. The 146-member package
has 142 byte-identical dependencies and four scenes equal to their base values after removing the
six rank-2 rasters from each. The logical size grows by 14,240 bytes. Allow about 3.3 MB plus small
raster scratch for a same-volume mount and about 315 MB for a cross-volume copy. The largest
changed scene is 883,418 bytes against the 4 MiB reader limit.

**Native reproduction.** Use the [BLAZE1 launch and private selectors](#prepared-blaze1-presentation)
with the mounted rank-2 candidate. Use a fresh runtime and disposable save namespace per run.
Set `SF2_BLAZE_RANK=2` and `SF2_BLAZE_ENEMY=0`. Put this start document in a fresh ignored
directory and point `SF2_BLAZE_START` at it.

```json
{"start":{"battle":"battle-4","activeAllies":["ally-0","ally-4"],"partyInput":"kazin.json","actors":[{"actor":"ally-4","position":{"x":4,"y":7}},{"actor":"enemy-1","hp":1,"position":{"x":6,"y":7}},{"actor":"enemy-2","position":{"x":6,"y":8}}]}}
```

Create the sibling `kazin.json` from the [controlled party template](../../reference/inputs/battle01-player-ready.json).
Set `id` to `blaze-2-controlled-party`, `evidenceOwner` to `issue-2105`, and
`bridgeBoundary` to `Pre-voyage Kazin level and book only`. Replace `allies` with the following array.
The reader decodes the standard private party shape against the opening static inputs and checks
class, equipment and the learned book against the Battle04 actor's admitted growth. MAGE
learns BLAZE2 at level 5. [Kazin's growth table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats04.asm)

Only level and book enter the Battle04 controlled start. The stat fields satisfy the controlled party shape.

```json
[{"id":4,"classId":3,"level":5,"maxHp":12,"hp":12,"maxMp":13,"mp":13,"attack":9,"defense":6,"agility":7,"move":5,"status":0,"items":[213,0,127,127],"spells":[75,63,63,63],"exp":0,"kills":0,"defeats":0}]
```

Run the observer with these settings.

| Case | `SF2_BLAZE_REDUCED_FLASH` | `SF2_BLAZE_SKIP_MESSAGES` | `SF2_BLAZE_HELD_RATE` |
| --- | ---: | ---: | ---: |
| Normal | 0 | 0 | 0 |
| Reduced Flash | 1 | 0 | 0 |
| Held rate and coin skip | 0 | 1 | 1 |

Match the version-1 settings file's `reducedFlash` and `suppressBattleHitCoinMessages` to
the observer selections. In the last case the observer holds Shift during the animation, reads
1.5× playback, and releases it before subsequent input.

**Confirmed in the remake.** Ordinary Magic selects rank 2 around the Galam Soldier at (6,7).
The Huge Bat at (6,8) receives the second reaction. Kazin's cast frame and Wooden Rod,
the live recipients, background and ground remain visible. All ten meteors fall and impact.
Early Confirm preserves the cast token, and cue 77 starts once. The three settings cases retain
the same settlement. Kazin pays 6 MP once (13→7), applies two ordered 9-point hits (HP 1→0
and 11→2), gains 49 EXP and one kill, and gold changes from 60 to 110. Skip passes the coin
notice while retaining damage messages. Bowie opens the next action menu in the same session.
Separate rank-1 ally and Galam Mage regressions preserve the accepted cast and usable control.
Godot reported ObjectDB exit warnings after control returned, with no session, scene or audio error.

**Unknown.** Natural pre-voyage learning and casting, a live mirrored BLAZE2 caller, original
timing, and foreground visual acceptance [#1456]. The equipment Use path remains with [#1926].

## HEAL spell scenes

The bounded HEAL1–4 consumer prepares two award draws, then retains scene/turn ownership through
MP cost, casting, optional ally switch, HP recovery, recovery text, ordinary idle, fairy retirement,
cleanup, actor restoration, EXP/growth and input release. Full HP still casts and awards the healing
minimum; self-healing preserves the already charged caster MP. Growth consumes the seed carried
through the scene, not the construction seed. Private scenes require admitted PRST cast content,
idle parameters and healing rasters; authored packages use an explicitly authored small gesture.
Private player AURA 1 uses the fairy path and checks the actual recovery recipients' scene bindings.
Its engine test checks ordered recovery and turn release. Ordinary player AURA 1 in private content
remains **Unknown**. Public authored AURA and the [finite attack, support and cure spells](battle-scenes.md#which-actions-can-be-reused)
use their own delivery paths. [Battle26 input and save observation](../application/church-promotion-design.md#current-battle-spells)
The historical #523/#437 records do not establish full scene or hardware parity; the
[current composed milestone](../../../docs/design/synthesis/milestones/map3-battle01-readiness.md#accepted-current-milestone)
has its separately bounded acceptance.

`HealingFairy` implements source integer word arithmetic, signed motion residuals, wing/dust state,
level-dependent one/one/two instances, setup ranges32/30/12 and conditional update draws. Lifetime
FFFF decrements before update. X-boundary ranges28/32 and periodic dust12 can co-occur; phase3
retirement under control2, clear-before-cleanup-wait and later restoration remain separate boundaries.
The existing private scene extractor supplies source body/wing/dust rasters and PRST cast sequence
without promoting assets. Godot projects that state and plays source SFX77 at cast setup and113 at
recovery. Reduced flash and text reveal alter delivery only.

The scene-local `HealingSceneCursor` separates mandatory operations, explicit neutral timed-input
reads and host delivery. Its logical message setting is speed2/no-messages0, independent of modern
text settings. `bsc10` tests acknowledgement before the next VInt opportunity; a neutral read advances
one opportunity. Neither reveal nor acknowledgement injects W2/range256 draws. Ordinary field
`WaitAtInput` and plain-dialogue `WaitForText` retain their own eligibility guards. Scene commands
require the current session, revision and scene token, with exactly one logical step per command.
Completion delivery may arrive early or late, but cannot add steps after logical completion.
For HEAL action/recovery messages, `CompletePresentation` reports text readiness independently of
`Acknowledge`. The source speed2 neutral timeout completes without an additional confirmation once
text is ready; if reveal finishes later, its delivery alone releases the expired message, with no
new logical opportunity or RNG. The host reports readiness at the timed-input boundary or after
timeout, and idle/reveal callbacks do not supply neutral Wait. Early acknowledgement still returns
before the next input-loop VInt. Reward/growth messages retain their existing acknowledgement policy.

Source owners are pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`battlescenes/animation/healingfairy.asm`, `animation/update/healingfairy.asm`, `battlesceneengine_0.asm`
(`bsc01`, `bsc05`, `bsc0B`, `bsc0D`, `bsc10`), `battlesceneengine_1.asm`, `common/tech/graphics/graphics_1.asm`
(`sub_179C/table_1840`) and the window/text helpers identified in the
[retained HEAL evidence](../../../docs/research/chapters/grans-opening/map3-messenger-acceptance.md#retained-heal-consumer-evidence).
Paths start below `disasm/code/gameflow/battle/` except the common helpers. PR550's accepted
prepared-04 instrumentation uses execution `3e69c2d49c8105c0e485dcbea70c688b57a86902` and its explicitly
rebound Issue515 prepared-11 parent. It does not reinterpret the incompatible older parent.

### Separate comparison boundaries

**Confirmed — function comparison:** `local/issue523/heal-scene/comparison/Compare.csproj` calls the
production kernel with HEAL1 and the externally observed post-award seed5C0D0000. Setup produces
seed00920000 and the observed properties. It then supplies exactly one update per recorded
`heal:fairy:after` and requests control2 from the trace's stop/control events. All244 returns match
property words, lifetime/control/toggle, dust age/frame/clock and carried seed, including29 conditional
update draws. Counts are comparison outcomes, not production budgets. This proves the update function
for that externally supplied opportunity/control schedule; it does not prove that the application
admits the original schedule, sprite coordinates/pixels, or every possible entry state.

**Confirmed — completed continuous-cursor comparison failure:**
`local/issue523/heal-scene/cursor-comparison/Compare.csproj` runs the production cursor from the same
external post-award seed, admitted PRST cast/idle content, recovery text `Sarah recovered\n6 hit points.`
and neutral timed reads. Subsequent opportunities and control2 come from the cursor, not trace events.
The first named divergence is recovery `bsc0B`, prepared-04 CP2059–2091 (one-based checkpoint lines),
observer frames32364–32367. Entry has actor/target1, HEAL1; the source action supplies HP+6, MP+0,
status0, reaction mode2. At CP2059, a6=FF0022, seed00920000, control1, lifetimeFFFE, toggle1,
fairy age2/phase7/delay23/dustClock1. Source `OpenAllyBattlesceneMiniStatusWindow` contains an explicit
move/VInt and a movement-end wait. The current cursor ends recovery after two opportunities with
lifetimeFFFC, age4, delay21. The observed caller returns after three, with lifetimeFFFB, age5,
delay20 (CP2084/2091). Neither has drawn yet, so both seeds are00920000 at this first divergence.

At recovery text return, the cursor has seed43EE0000/lifetimeFF80; original CP3458/3459 has
D8800000/FF7F. This is an actual intermediate RNG divergence, not a waived display difference.
At make-idle exit the cursor has seedECE20000/lifetimeFF6A, while CP3727 has ECE20000/FF65;
the equal seed does not restore the missing state/opportunity correspondence. At final stop return,
both reach seed8E2D0000/control0; this particular capped recovery and construction-time award remain
HP5→11, MP10→7, EXP+13. That endpoint coincidence does not establish general result equality:
other seeds, content or stop phases may alter conditional draws and later gameplay/growth.

The PRST idle header is20. `bsc05` performs a real frame load on each side of Sleep; each ends in
`WaitForDmaQueueProcessing`, whose helper unconditionally branches to `WaitForVInt`. The cursor
therefore retains those two operations around the content-provided sleep. Weapon queue enable and
`sub_1942` do not themselves wait. The observed26-frame bsc05 interval is not a source sleep value.
An earlier hand-calculated153-versus154 estimate incorrectly used that observed interval; it is
superseded by direct cursor execution:149 active opportunities before stop versus154 observed.
No149/153/154/26 budget or seed padding is used in production.

**Confirmed — bounded opportunity readback:** the
[research attribution](../../../docs/research/chapters/grans-opening/map3-messenger-acceptance.md#recovery-and-make-idle-opportunity-attribution)
records three top-level recovery VInts: the first has A6=`FFFFDE80` (the source number buffer), the
next two have restored script A6=`00FF0028`. Source numeric conversion has no waits; window movement
has an explicit VInt followed by a wait that repeats conditionally until movement ends. Make-idle
has26 top-level VInts: four with stack A6 in two pairs, two with restored script A6/d0=`$900`, and20
with the Sleep counter19…0. The source performs two decompressions and DMA waits around Sleep(20).

**Inferred:** recovery's extra opportunity interrupts numeric window construction; make-idle's four
extra opportunities interrupt the two decompressions. These are CPU-work interruptions outside the
explicit waits, not evidence for fixed +1/+4 waits. The pinned source/register mapping and exact
readback command belong to the research owner. The audited `HealingSceneCursor` opportunity rules
are unchanged between the retained comparison at8e6881ea and accepted PR557 main
`bfcb819fe61cf6b7f2a3a45822f6de51e411e559`; its neutral timeout correction changes completion handling,
not these opportunity rules. The completed continuous-cursor **FAIL** remains.

**Unknown — gameplay-opportunity admission:** exact interrupted PCs, live busy/window/graphics gates,
cross-state/content timing, and a general semantic treatment remain unproven. Conditional window
completion alone does not account for interruptions during CPU work. Wider content/gate coverage
and full continuous scene conformance remain Unknown at the [retained comparison boundary](../evidence/retained-comparisons.md).
[ADR0010 Option A/8D](../../../docs/decisions/0010-map3-battle01-product-acceptance.md#evidenced-gameplay-waits-accepted-option-a)
still controls shared RNG and selected gameplay results. This attribution does not waive the
intermediate RNG difference or establish that hardware simulation is necessary. A next proposal
must identify concrete gameplay effects and feasible semantic handling under that policy; the
observed counts alone supply neither a new product decision nor permission for new capture.

**Confirmed — separate native settings comparison:** actual Godot inputs in the existing scene
observer exercise full-HP HEAL3, wounded other-target HEAL1 and wounded self-target HEAL2, alongside
physical scenes. Each completes real fairy retirement/cleanup and releases input, with actual77/113
audio and live projected nodes. Normal/instant versus reduced-flash/adjustable text uses the same
semantic inputs; final `native-timeout-normal-01` / `native-timeout-reduced-01` records each finish
six scenes and match all2311 ordered semantic events, RNG and final resource/turn state (delivery
metadata excluded). Both action and recovery messages also exhaust65 neutral inputs with zero
Confirms after natural text readiness; their final-input receipts contain no extra RNG or delivery.
Later HEAL cases retain early acknowledgements. This proves bounded settings conformance, not original neutral-input conformance.
The reproduction and completed failure records belong to the
[verification owner](../verification/battles.md#heal-scene-verification).

## Prepared BLAST1 presentation

**Standard scope (#2106).** This bounded adapter/content slice mounts the registered tileset-15
rank-1 fragments and slashes in Battles04–07. It shares the prepared cast clock and completion
token with BLAZE. Sarah's PRST level-10 book supplies the legal adjacent, single-target cast.
Acceptance requires live actors, weapon, backdrop, effect, one payment and settlement, completed
animation, and usable next control under normal, Reduced Flash and held-rate/coin-skip settings.
The implementation owns AttackSpellView, BattleSceneView, BlastOneEffect, the scene reader,
PrivateAttackSpellBindings, the existing retail publisher and its extracted spell-mount helper,
the narrow observation, SessionAudio cue 114 mapping, its producer registration in verification_engine_paths.py,
and these presentation/capability documents.

Publication is linear in the existing package closure. Budget 120 MB of new same-volume package
storage and raster scratch, 400 MB if hardlinks require copying, and 2 GiB publisher memory for
parsing the approximately 102 MB world JSON and its decoded objects.
Four scene JSON files and the world audio binding change. The publisher copies those files before writing and hardlinks
unchanged dependencies. Reassess if the closure or any scene's 4 MiB admission limit is exceeded.


**Confirmed source and identity.** Pinned SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6 and
USA ROM 9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9 select animation 5,
2 MP, power 6, range 1–1 and radius 0. PRST learns BLAST at level 10.
Follow the [existing ROM comparison](#prepared-blaze1-presentation), using slot 15 at
p_pt_SpellGraphics (0x1B800C). Its header is at 0x1C2C56. The decoded 6,816 bytes equal the
registered effects/spell-15.4bpp. Header replacement words are 0EC8, 026C, 0C82. The registered
base palette equals the 16 ROM words at 0x198A8.

Setup initializes eight fragments and one short-slash slot. Rank 1 leaves the even-rank flag clear
and bypasses the rank-3 cyclone and attached dust. The unmirrored/mirrored VDP origins
(208,224)/(312,240) become (80,96)/(184,112) on the local canvas. The six fragment modifiers
and eight slash modifiers stay relative to that origin. Mirroring changes the origin and
preserves those local modifiers.

The publisher uses column-major sprite tiles and the following zero-based operands.

| Frames | Starting tiles | Dimensions in pixels |
| --- | --- | --- |
| Diagonal fragments 0–2 | 0, 1, 5 | 8×8, 16×16, 16×24 |
| Horizontal fragments 3–5 | 23, 25, 29 | 8×16, 16×16, 24×16 |
| Vertical slash 6–8 | 151, 155, 163 | 16×16, 16×32, 16×24 |
| Horizontal slash 9–11 | 169, 173, 181 | 16×16, 32×16, 24×16 |
| Angled slash 12–14 | 187, 191, 207 | 16×16, 32×32, 16×24 |

table_1CBDE adds offsets to VDPTILE_SPELLTILE1, equal to SPELLTILE_START. table_1CC44 supplies
the slash dimensions and tile offsets. The adapter schedules eight fragments and eight short
slashes over 1.4 seconds with deterministic converging motion and fades. Reduced Flash fixes
each family to its middle frame. Timing, sampling and fades are modern approximations.
Cast cue 77 starts at the pose trigger and wind cue 114 starts 0.1 seconds later. The original
requests 114 repeatedly when particles spawn. The adapter uses one whole recording through
the accepted finite mixing policy. sounddriver.asm:sfx_32, driver offset 0x1D08, is type 1
with active channel indices 8/9 pointing at 0x1D1D/0x1D2A, PSG tone 3 and noise.
SessionAudio now admits that combination.

**Publication.** Load the worktree private configuration. Select the registered #2001 library as
$library, accepted #2105 package as $base, and a fresh absolute output as $candidate.
Run the following from the checkout root.

    uv run python -m sf2tool.remake_retail_assets --root $library --base $base --mount $candidate --family blast-1

The stable handoff selection is registered derived/issue2106/pre-voyage-spells-v2. The earlier
v1 retains the visual-only observation. Version 2 adds the registered cue 114 WAV with the existing
audio publisher. Four scenes grow by 20,536 bytes total and the largest is 888,552 bytes.
The final 147-file closure keeps 141 base files byte-identical. The five rewritten files total
105,448,603 bytes and the new WAV is 51,192 bytes. After removing the 15 BLAST rasters from each
scene and cue 114 from world audio, all five parsed documents equal their base documents.

**Native reproduction.** Select the candidate world.json, main-scenes.json, entry/entry.json and
six opening-inputs through the ordinary SF2_PRIVATE_* variables. Use a fresh worktree-local
report/runtime and a task-specific external save namespace. Copy the controlled party template
remake/reference/inputs/battle01-player-ready.json as sarah.json in the report directory.
Set id to blast-1-controlled-party, evidenceOwner to issue-2106 and bridgeBoundary to
Pre-voyage Sarah level and book only. Replace allies with the following array.

    [{"id":1,"classId":4,"level":10,"maxHp":11,"hp":11,"maxMp":10,"mp":10,"attack":9,"defense":5,"agility":5,"move":5,"status":0,"items":[213,0,0,127],"spells":[64,2,14,63],"exp":0,"kills":0,"defeats":0}]

Items and max MP match the opening Sarah baseline. The controlled reader imports only the validated
level/book. HEAL2, DETOX1 and BLAST1 are legal learned entries at level 10. Point SF2_BLAST_START
at an absolute sibling start JSON containing this document.

    {"start":{"battle":"battle-4","activeAllies":["ally-0","ally-1"],"partyInput":"sarah.json","actors":[{"actor":"ally-1","position":{"x":5,"y":7}},{"actor":"enemy-1","hp":1,"position":{"x":6,"y":7}}]}}

Build the Debug adapter using the locked SDK workflow. Set SF2_OBSERVATION_OUTPUT to a fresh report.
Launch the installed Godot with the following arguments and an absolute version-1 settings file.

    & $env:GODOT_BIN --headless --audio-driver Dummy --rendering-method gl_compatibility --fixed-fps 60 --resolution 1920x1080 --path remake/game --script res://probes/engine_private_wind_cast_observation.gd -- --input-settings $settings

Normal uses reducedFlash=false, suppressBattleHitCoinMessages=false and observer switches
SF2_BLAST_REDUCED_FLASH, SF2_BLAST_SKIP_MESSAGES, SF2_BLAST_HELD_RATE all set to 0.
Reduced Flash changes its settings field and observer switch to true/1.
The third case sets suppressBattleHitCoinMessages=true, SF2_BLAST_SKIP_MESSAGES=1 and
SF2_BLAST_HELD_RATE=1 with Reduced Flash off.
The ordinary Magic observer advances three rows because HEAL1 and HEAL2 both appear.

**Confirmed in the remake.** All three cases bind caster, target, weapon, background and ground,
read selector 5 and mounted frames, retain early-Confirm ownership, and cover the full effect.
Normal reads 15 frames, Reduced Flash reads 5, and held Shift reads 1.5× pacing.
Cues 77 and 114 each start once. Sarah pays 2 MP (10→8), delivers one 6-point hit to the 1-HP
enemy, gains one EXP and one kill, and gold changes 60→110. Bowie opens the next action menu.
BLAZE1 ally/enemy and BLAZE2 ally regressions pass on the final package.

**Completed failures.** The initial placeholder probe selected DETOX1 because it omitted HEAL2's
list row. The first prepared observation queried a reset effect target during Reaction; the
corrected assertion reads the displayed enemy. Adding cue 114 exposed sfx-slots-unavailable and
three completed deadline failures. The source-backed slot mapping corrected the failure and all
three affected cases passed. Reports remain under local/issue2106-baseline in the owning worktree.
Some runs report 8–10 ObjectDB instances at exit, covered by paused #1716.

**Unknown.** Natural pre-voyage learning/casting, live enemy BLAST1, exact hardware timing and
human foreground visual acceptance. Mirrored placement has static source comparison only.
Higher ranks remain with their existing owners.

## Enemy healing family #2227

Standard rigor applies to this bounded content/presentation correction. Acceptance covers the
shared HEAL/AURA faction selector, existing Original/HD contract, controlled pre-voyage enemy
HEAL1, an allied counterexample and a closed successor package. Natural enemy reach, actual
enemy AURA, private unbound ranks and hardware-exact timing remain **Unknown**.

**Confirmed** from pinned SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6,
`animation/healingfairy.asm` and `animation/update/healingfairy.asm`. Light body offsets are
0/16, wings 32/40 and dust 52 down through 48. Evil body offsets are 53/69, wings 85/93
and dust 105 down through 101. Both use tileset 6, palette 3 and the same three palette
replacements. Light starts at source sprite X384/phase7 without mirror; evil starts at
X96/phase3 mirrored. Y is 128 plus the existing range32 draw. Ranks 1/2/3/4 allocate
1/1/2/3 fairies with these same banks. Dust spawns at the current fairy position plus (12,12).
[Setup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/healingfairy.asm)
· [Update](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/update/healingfairy.asm)

The existing HealingSceneCursor selects the initial side from the caster for every healing
caller. Domain retains the existing update, lifetime and draw schedule. BattleSceneView uses
source coordinates minus the VDP origin and selects `healing.enemy` for enemy casts. Older
packages without that optional group retain their light artwork. Each selected body/wing/dust
group requires complete HD coverage; one missing alternate falls back to its whole Original
group. Graphics preparation includes both groups before publishing the preference.

**Confirmed** by the native `engine_enemy_healing_art_observation.gd` controlled Battle06
start. Dark Cleric enemy-0 chooses HEAL1 on Galam Soldier enemy-8 through the production AI.
One payment changes MP7→4 and recovery caps HP1→11. All nine evil body/wing/dust resources
are actual Sprite2D textures, with source-coordinate placement and stable logical footprints.
Separate HD, Original and missing-HD-dust4 runs return to ordinary player control with no enemy
EXP. The existing `engine_hd_heal_observation.gd` Battle04 self-HEAL counterexample covers
all nine light resources, one MP payment, HP recovery and ordinary Settings HD/Original changes.
No screenshots or campaign replay are part of these observations.

Reproduce with the complete package below and the existing locked Debug/Godot launch setup.
Set `SF2_HEAL_START` to a worktree-local JSON with `start.battle="battle-6"`,
`activeAllies=["ally-0","ally-1"]`, enemy-1 through enemy-7 HP0, enemy-8 HP1,
ally-0 position (7,25) and ally-1 position (6,25). Each override is an object in `start.actors`
with `actor`, optional `hp` and optional `position:{x,y}`. Retain the source cleric position,
loadouts and spellbook. Run the observer with a fresh disposable namespace and
`SF2_OBSERVATION_OUTPUT`; use `SF2_HEAL_EXPECT_HD=1` with HD settings, or `0` for Original
and a disposable package missing just `healing-enemy/dust4` from the Battle06 HD bank.
The allied recipe is in [HD rollout](../content/hd-rollout.md#private-battle-artwork-acceptance).

The behavior tests cover both source entry sides, rank reuse, motion, dust, retirement and
unchanged random draw setup. The engine suite completed with 3263 passes and 50 skips.
Adapter Release and Debug builds passed. Retained failures include an initial C# local-name
collision, a producer assumption about a nonexistent `scene.json` (the actual opening owner is
`main-scenes.json`), a missing Pillow invocation, a 64×64 observation viewport and a probe field
lookup. Final native runs have no adapter/script errors; some retain a four-ObjectDB exit warning.
Peak native memory and subjective appearance remain **Unknown**.
