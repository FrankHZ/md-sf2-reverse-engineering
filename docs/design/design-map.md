# Remake Design Map

| Level | Design question | Entry |
| --- | --- | --- |
| L0 · Vision | What experience are we preserving, and which changes have been accepted? | [Remake Vision](game-vision.md) |
| L1 · Overall gameplay | How do exploration, party preparation, tactical decisions, outcomes and world return connect? | [Gameplay Overview](synthesis/gameplay-overview.md) |
| L2 · System design | What choices and state does each system own, and how does it hand off to the next? | [Tactical battle](synthesis/themes/tactical-battle-loop.md), [progression and economy](synthesis/themes/progression-and-economy.md), [story progression](synthesis/themes/story-progression.md), [exploration and maps](synthesis/themes/map-design-principles.md), and the accepted design portions of the [shared visual language](../../remake/docs/godot/ui-design-language.md) |
| L3 · Mechanism and behavior specification | Given a player choice and valid state, what is legal, what changes in what order, and where does control go? | Relevant rule sections in the [contract index](../README.md#contracts-combat-turns-and-encounter-decisions) and explicit remake behavior agreements, as in the examples below |

The [roadmap](documentation-roadmap.md#design-and-support-scope) defines section-level publication
scope. The [evidence boundary](documentation-roadmap.md#three-layer-boundary) governs the meaning of
a claim at every level. A **Confirmed** original fact has a named static or observed boundary. An
**Inferred** design reading explains how those facts combine. An accepted remake choice has its own
decision owner. Running code and recorded results establish remake support only for the cases
they cover.

For a change, identify the affected system and behavior below or through the L2 links. Resolve a
missing rule or product choice with its owner before implementing the affected behavior. Follow the
English evidence and tables to check the terms, then use the remake owner to scope implementation
and acceptance. An existing contract can cover multiple callers, each with its own admission and
observation limits. [Campaign feature trace](../../remake/docs/capability-status.md#campaign-feature-trace)

## Equipment and promotion

- **L2.** Promotion changes the member's class and later action options. Possession, eligibility,
  and the contribution from worn gear are separate. [Class progression](synthesis/themes/progression-and-economy.md#definitions-current-class-and-promotion) ·
  [Equipment and carried gear](synthesis/themes/progression-and-economy.md#carried-gear-becomes-an-equipped-contribution)
- **L3.** The rite selects a joined eligible member by current class and stored level. The regular
  branch is free. Only final acceptance of the special branch consumes the chosen holder's item.
  Success sets level 1 and EXP 0, preserves earned stats and vitals, and refreshes class and the
  equipped contribution. SORC replaces the book with DAO 1. MMNK and NINJ clear the weapon equip
  state while retaining the item. The accepted remake decision carries the current class through
  field, battle, return and save. [Selection and custody](../../remake/docs/application/church-promotion-design.md#selection-and-custody) ·
  [Success rules](../../remake/docs/application/church-promotion-design.md#what-changes-on-success) ·
  [Stat refresh order](contracts/party/level-up.md#confirmed-current-and-derived-stat-refresh) ·
  [Remake decision](../../remake/docs/application/church-promotion-design.md#remake-decision)
- **English evidence and tables.** The church agreement links the pinned calls and the
  promotion/class tables. Consult the item-definition owner for equipment class masks and effects,
  the growth owner for ally/class learning entries, and the item index for actual supply custody.
  The complete original promotion and SRAM round trip remains **Unknown**. [Church source](contracts/party/service-interactions.md#church-source-boundary) ·
  [Item records](contracts/party/item-definition-data.md#item-identity-and-fixed-record) ·
  [Class growth data](../research/party/ally-growth.md#storage-model) ·
  [Promotion supply index](../../remake/docs/content/campaign-items.md#promotion-supplies)
- **Remake and acceptance.** Ordinary Pacalon Bowie promotion has a decline branch, HERO with his
  retained Achilles Sword and book, field and battle cold Continue, level-up and paid EGRESS.
  That acceptance covers the named HERO route and existing weapon behavior. [Field service code](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/FieldService.cs) ·
  [Equipment code](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleEquipment.cs) ·
  [HERO evidence and limits](../../remake/docs/application/church-promotion-design.md#playable-hero-and-reproduction) ·
  [Connected equipment status](../../remake/docs/capability-status.md#connected-battle-equipment-agreement)
- **L3 ordinary promotion classes.** [Current families, consumers and implementation order](../../remake/docs/application/ordinary-promotion-classes.md)
- **L3 ring equipment design.** One weapon and one ring can be worn in the same pack. Current
  class determines eligibility. Static ring effects follow status refresh, and the weapon supplies
  physical range. The design specifies separate removal choices, custody, promotion and Continue
  for Power, Protect, Quick and Running Ring. [Ring equipment and acceptance](../../remake/docs/domain/ring-equipment-design.md)
- **L3 equipment Use and Repair.** [Battle effects, damage, merchant Repair and Continue](../../remake/docs/domain/equipment-use-repair-design.md)
- **L3 permanent growth design.** [Field and Caravan growth items, Brave Apple, and persistence](../../remake/docs/domain/permanent-growth-items-design.md)
- **L3 offensive consumables.** [Battle item targeting, damage, consumption and Continue](../../remake/docs/domain/offensive-items-design.md)
- **L3 recovery consumables.** [Battle HP and MP recovery, item custody and Continue](../../remake/docs/domain/recovery-items-design.md)
- **Remaining design and work.** Use the rite owner to admit the next actor/class through its actual
  growth, equipment/book, appearance and save consumers. Static ring implementation and its ordinary
  witnesses precede the dependent Use, Repair, curse and Sandals work.
  [Ordinary classes #1956](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1956) ·
  [Special rites #1957](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1957) ·
  [Ring slot #1946](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1946) ·
  [Use and repair #1952](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1952) ·
  [Black Ring #1926](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1926) ·
  [Chirrup Sandals #1962](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1962)

## Status spells

- **L2.** The learned rank and legal center lead to a committed Magic action, ordered per-target
  outcomes and later turns affected by the new state. [Magic and support](synthesis/themes/tactical-battle-loop.md#magic-healing-and-support) ·
  [Commit and replay](synthesis/themes/tactical-battle-loop.md#action-commit-construction-and-replay)
- **L3.** SLOW and BOOST separate target selection, payment, effect and stat refresh. Canceling
  before confirmation leaves the action uncommitted. Fresh applications and failed recasts have
  different refresh behavior. The accepted remake rule retains the last-refresh operand through
  battle Continue when duration and effective stats diverge. [BOOST](contracts/battle/spell-resolution.md#confirmed-boost-1-fresh-application-and-recast-quirk) ·
  [SLOW](contracts/battle/spell-resolution.md#confirmed-slow-1-status-resistance-matrix) ·
  [After-turn counters](contracts/battle/spell-resolution.md#confirmed-after-turn-counter-transition-matrix) ·
  [Remake behavior](../../remake/docs/domain/support-spell-status-design.md#implementation-decisions)
- **English evidence and tables.** Spell definitions supply rank costs, center range, radius and
  silence eligibility. Ally growth supplies learned ranks. Consult the source and fixture links
  in the support owner for the separate resistance and stat rules. Natural original SLOW 2/recast
  lifetime and reload with an outstanding recast remain **Unknown**.
  [Spell record](contracts/party/spell-definition-data.md#fixed-eight-byte-record) ·
  [Pinned spell and ally rows](../../remake/docs/domain/support-spell-status-design.md#current-reach) ·
  [Source and controlled observations](../../remake/docs/domain/support-spell-status-design.md#original-rules-and-evidence)
- **Remake and acceptance.** Battle26 witnesses Sarah's SLOW 1/2 and Karna's BOOST 1 via ordinary
  Magic, plus cancellation, recast and effective stats through cold battle Continue and EGRESS.
  The owner separates engine comparisons, native input and completed shutdown failures.
  [Rank bindings](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateSupportSpellBindings.cs) ·
  [Action code](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2SupportSpellAction.cs) ·
  [Capability status](../../remake/docs/capability-status.md#authored-aura-agreement) ·
  [Ordinary acceptance and limits](../../remake/docs/domain/support-spell-status-design.md#battle26-ordinary-input-and-reproduction)
- **L3 DETOX ranks.** [Field and battle cures, cursed gear, refresh and Continue](../../remake/docs/domain/detox-ranks-design.md)
- **Remaining design and work.** Scope other status families by actual target eligibility, resistance,
  payment and after-turn/return behavior. Keep DETOX ranks and hostile statuses under their own
  terms and legal consumers. [DETOX 2/3 #1965](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1965) ·
  [MUDDLE, DESOUL and Tyrin SLEEP design](../../remake/docs/domain/hostile-spells-design.md) ·
  [Shutdown follow-up #1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716)

## Exploration services and Save/Continue

- **L2.** A visit selects a population and service caller. Exiting a service, returning from battle
  and resuming a recorded adventure are distinct handoffs. [Map interaction](synthesis/themes/map-design-principles.md#action-and-interaction) ·
  [Service exchanges](synthesis/themes/progression-and-economy.md#source-static-service-exchanges) ·
  [Return and persistence](synthesis/themes/map-design-principles.md#return-and-persistence)
- **L3.** In Elven Village, Fairy state selects the later population, with the shared priest and
  stairs between its two areas. Treat selection of the return anchor separately from save capture.
  The remake closes the service before checking eligibility in settled field play. Cancel keeps
  the existing return anchor. A confirmed church save records the new anchor and the admitted
  scene for Continue. [Church Save seam](contracts/party/service-interactions.md#church-save-runtime-seam) ·
  [Original load/return choices](contracts/system/save-system.md#confirmed-witch-save-menu-action-admission-matrix) ·
  [Remake eligibility and restoration](../../remake/docs/application/opening-session-lifecycle.md#eligibility-and-restoration-recipes)
- **English evidence and tables.** Follow the supplied-F762 caller rows and current-map stair
  conditions to the pinned map tables. The remake chapter owner identifies the original
  `savepointmapcoords.asm` table for the church anchor. Original natural reach and the full local
  church and reload outcome remain **Unknown**. [Population table](../research/chapters/southern-travel-creed/elven-village-conversations-and-local-exploration.md#supplied-f762-table) ·
  [Stairs](../research/chapters/southern-travel-creed/elven-village-conversations-and-local-exploration.md#internal-stairs-and-conditional-map23-control) ·
  [Location lifecycle index](../../remake/docs/content/campaign-locations.md#southern-travel)
- **Remake and acceptance.** The post-Fairy visit covers upstairs Talk, canceled and confirmed Church
  Save, cold Chinese Continue in the later population, stair descent, south exit and paid Battle19
  EGRESS to the priest. The optional wandering resident's Talk has compiled coverage only.
  [Field save](../../remake/src/Sf2.Remake.Application/Runtime/Persistence/OpeningSave.cs) ·
  [Field restore](../../remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OpeningRestore.cs) ·
  [Save status](../../remake/docs/capability-status.md#ordinary-field-time-and-save-entry) ·
  [Village evidence and reproduction](../../remake/docs/application/battle19-village.md#return-after-creed)
- **Remaining design and work.** Check later service and scene recipes at the first affected
  ordinary caller, with settled, canceled and cold-restore branches. Remote service access and
  any wider save permission need a product decision at the modernization owner.
  [Campaign #1818](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1818) ·
  [Local services proposal](../../remake/docs/godot/modernization-design.md#recommended-town-access) ·
  [Shutdown follow-up #1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716)
