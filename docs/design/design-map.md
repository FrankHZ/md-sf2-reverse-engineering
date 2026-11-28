# Remake Design Map

Start here to navigate the remake's design from player experience down to exact rules, original
evidence, and current implementation.

| Level | Question answered | Entry |
| --- | --- | --- |
| L0 · Vision | What experience are we preserving, and which modernizations are accepted? | [Remake Vision](game-vision.md) |
| L1 · Overall gameplay | How do exploration, preparation, battle turns, rewards, and return connect? | [Gameplay Overview](synthesis/gameplay-overview.md) |
| L2 · System design | What choices, costs, and consequences does each system own? | [System map below](#system-map) |
| L3 · Mechanism rules | Given a valid state and player choice, what is legal, in what order, and where does control go? | Subsystem contracts in the [documentation index](../README.md#contracts-combat-turns-and-encounter-decisions) and explicit remake rule owners below |

## System map

Each row connects a game-design area to its current design owner, supporting English evidence or
remake specification, and any missing designer-facing entry. Technical documents are labeled as
support rather than treated as finished design overviews.

| Area | Design entry (L0–L3) | Technical rules, evidence, and implementation support | Current gap / active tracking |
| --- | --- | --- | --- |
| Tactical battle & encounters | [Tactical Battle Loop](synthesis/themes/tactical-battle-loop.md) · [Battle design pilot](synthesis/analyses/battle-design-analysis-pilot.md) · [Battle02 first contact](synthesis/analyses/battle02-first-contact.md) | [Battle lifecycle](contracts/battle/battle-control-lifecycle.md), [navigation](contracts/battle/battlefield-navigation.md), [combat](contracts/battle/combat-resolution.md), [spells](contracts/battle/spell-resolution.md), [AI](contracts/battle/battle-ai-decision.md), [actions](contracts/battle/battle-action-construction.md), [special mechanics](../../remake/docs/domain/special-mechanics-design.md) | Readable numeric/formula quick reference is planned follow-on work (`#2259` Phase C); late special-mechanic delivery under [`#1370`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1370). |
| Progression, classes, gear & economy | [Progression and Economy](synthesis/themes/progression-and-economy.md) | [Roster](contracts/party/party-roster-state.md), [level-up](contracts/party/level-up.md), [spellbook](contracts/party/spellbook-state.md), [services](contracts/party/service-interactions.md), [Caravan & Deals](contracts/party/caravan-and-deals-state.md), [promotion](../../remake/docs/application/church-promotion-design.md), [equipment & rings](../../remake/docs/domain/equipment-use-repair-design.md) | Core equipment, promotion, and consumable rules are implemented within their recorded bounds; natural late acquisition and smithing remain campaign work ([`#1818`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1818)). |
| Exploration, maps & town services | [Map Design Principles](synthesis/themes/map-design-principles.md) | [Map exploration](contracts/world/map-exploration.md), [dialogue](contracts/interface/dialogue-system.md), [input](contracts/interface/input-system.md), [field & shop UI](../../remake/docs/godot/field-and-shop.md), [Caravan services](../../remake/docs/application/caravan-services-design.md) | Later-chapter map setups and optional visits continue under [`#1818`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1818). |
| Story & campaign structure | [Story Progression](synthesis/themes/story-progression.md) | [Story research coverage](../research/story-content-coverage.md), [campaign flags](../../remake/docs/content/campaign-flags.md), [items](../../remake/docs/content/campaign-items.md), [locations](../../remake/docs/content/campaign-locations.md), [characters](../../remake/docs/content/campaign-characters.md), [campaign baseline](../../remake/docs/application/remaining-campaign-risk-baseline.md) | Chronological story-event and chapter overview table is tracked under [`#2366`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2366). |
| UI, UX & modernization | [Remake Vision](game-vision.md#fidelity-and-modernization) (accepted summary) | [Shared visual language](../../remake/docs/godot/ui-design-language.md), [UI scene review](../../remake/docs/godot/ui-scene-review.md), [objective guidance](../../remake/docs/godot/objective-guidance.md), [modernization proposals](../../remake/docs/godot/modernization-design.md) | **Design gap:** no unified L2 UI/UX and original-vs-remake modernization overview yet ([`#2386`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2386), `#2259` Phase C); active UI/UX work under [`#438`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438), [`#1616`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1616), [`#2369`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2369), [`#1578`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1578). |
| Presentation, audio & Original/HD modes | *No standalone L2 design page yet* | [Presentation](../../remake/docs/godot/presentation.md), [audio](../../remake/docs/godot/audio.md), [battle scenes](../../remake/docs/godot/battle-scenes.md), [spell presentation](../../remake/docs/godot/spell-presentation.md), [HD rollout](../../remake/docs/content/hd-rollout.md), [media workflow](../../remake/docs/content/asset-pipeline-workflow.md) | **Design/production gap:** unified artist/audio specs and HD coverage guide tracked under [`#2384`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2384); HD art under [`#1699`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1699). |
| Save, Continue & persistence | [Save contract](contracts/system/save-system.md) · [Map return & persistence](synthesis/themes/map-design-principles.md#return-and-persistence) | [Field save & lifecycle](../../remake/docs/application/opening-session-lifecycle.md), [battle Save & Continue](../../remake/docs/application/battle-save-and-continue.md), [capability status](../../remake/docs/capability-status.md#campaign-feature-trace) | Later-chapter save recipes follow [`#1818`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1818); snapshot audit under [`#2284`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2284). |
| Modern engine & Debug support | *No designer-facing entry yet* | [Remake architecture](../../remake/docs/architecture.md), [ADR 0019](../decisions/0019-state-and-content-driven-remake-engine.md), [local debug port](../../remake/docs/verification/debug-port.md) | **Design gap:** designer-facing architecture and Debug support entry is tracked under [`#2358`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2358). |

## Representative design-to-implementation traces

When changing a rule, trace it from L2 player choices through L3 behavior to the single evidence,
data, and implementation owner.

### Equipment and promotion

- **Player rule (L2/L3):** A member's current class governs what they can equip, what spells they
  learn, and how they move. At Church, any joined member (including reserves or fallen allies) at
  level 20+ with a promotable base class can promote. Regular promotion is free; special promotion
  checks any joined member's inventory for the required item and consumes it only on final
  confirmation. Declining a special branch falls back to regular promotion; declining SORC's
  spellbook overwrite preserves both the item and existing spells. Promotion sets level 1 and EXP
  0, keeps earned stats and current HP/MP, refreshes class base stats and worn equipment, replaces
  the SORC spellbook with DAO 1, and unequips (without removing) MMNK/NINJ weapons. A member may
  wear one weapon and one ring independently; some gear can be Used in battle, may break, and can
  be Repaired at shops. [Class progression](synthesis/themes/progression-and-economy.md#definitions-current-class-and-promotion) ·
  [Carried vs equipped gear](synthesis/themes/progression-and-economy.md#carried-gear-becomes-an-equipped-contribution)
- **Exact owners:** [Church promotion design](../../remake/docs/application/church-promotion-design.md),
  [ordinary classes](../../remake/docs/application/ordinary-promotion-classes.md),
  [ring equipment](../../remake/docs/domain/ring-equipment-design.md),
  [equipment Use & Repair](../../remake/docs/domain/equipment-use-repair-design.md),
  [permanent growth items](../../remake/docs/domain/permanent-growth-items-design.md),
  [offensive consumables](../../remake/docs/domain/offensive-items-design.md),
  [recovery consumables](../../remake/docs/domain/recovery-items-design.md),
  [item records](contracts/party/item-definition-data.md#item-identity-and-fixed-record),
  [stat refresh](contracts/party/level-up.md#confirmed-current-and-derived-stat-refresh).
- **Current boundary:** All 94 source equipment definitions, all 30 members, and their 59 legal
  class bindings have accepted engine and consumer coverage under their named owners (see
  [whole-game equipment boundary](../../remake/docs/capability-status.md#whole-game-equipment-audit-and-implementation-boundary)).
  Original full-transaction promotion and SRAM round-trip remain **Unknown**; natural late
  recruitment, acquisition, and smithing remain campaign work ([`#1818`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1818)).

### Status spells

- **Player rule (L2/L3):** Casting requires a learned spell rank, enough MP, and a valid center;
  canceling before confirmation spends no MP. SLOW and BOOST separate counter updates from stat
  refreshes: a failed recast can change the stored counter while leaving old effective stats active
  until the next refresh (the remake preserves this distinction across battle Continue). DETOX 2/3
  cures poison and stun (and rank 3 also removes curse and unequips cursed gear) with separate
  field and battle rules. Hostile status spells (SLEEP 1, MUDDLE 1/2, DESOUL 1/2) follow their own
  MP, range, resistance, and control/death rules—not one shared countdown formula.
  [Magic, healing & support](synthesis/themes/tactical-battle-loop.md#magic-healing-and-support)
- **Exact owners:** [Spell resolution](contracts/battle/spell-resolution.md),
  [spell definitions](contracts/party/spell-definition-data.md#fixed-eight-byte-record),
  [support spells](../../remake/docs/domain/support-spell-status-design.md),
  [DETOX ranks](../../remake/docs/domain/detox-ranks-design.md),
  [hostile spells](../../remake/docs/domain/hostile-spells-design.md).
- **Current boundary:** SLOW 1/2, BOOST 1, DETOX 2/3, SLEEP 1, MUDDLE 1/2, and DESOUL 1/2 have
  accepted controlled or ordinary battle/field witnesses in their owning docs. Original natural
  SLOW 2 recast lifetimes and reloading with an unrefreshed recast remain **Unknown**; natural late
  campaign progression to these casts remains outside controlled witnesses.

### Exploration services and Save/Continue

- **Player rule (L2/L3):** Visiting a town selects its population and available services from live
  story flags. Leaving a service, returning from battle, and cold-continuing a saved game are three
  distinct handoffs. For example, Elven Village switches residents after the Fairy returns while
  keeping its shared priest and internal stairs. In the remake, Church Record closes the service
  first, then checks settled-field save eligibility: canceling keeps the prior EGRESS return town,
  while confirming records both the new return anchor and the restorable scene.
  [Map return & persistence](synthesis/themes/map-design-principles.md#return-and-persistence) ·
  [Services](synthesis/themes/progression-and-economy.md#shops-church-and-caravan-services)
- **Exact owners:** [Save contract](contracts/system/save-system.md),
  [service interactions](contracts/party/service-interactions.md),
  [field save/restore recipes](../../remake/docs/application/opening-session-lifecycle.md#eligibility-and-restoration-recipes),
  [battle Save & Continue](../../remake/docs/application/battle-save-and-continue.md),
  [Elven Village return](../../remake/docs/application/battle19-village.md#return-after-creed).
- **Current boundary:** Settled field saves and settled player-Movement battle saves cover the
  admitted campaign frontier; original natural full-chapter Save → Load persistence remains
  **Unknown**. Remote town service shortcuts remain a pending product proposal
  ([`modernization-design.md`](../../remake/docs/godot/modernization-design.md#recommended-town-access));
  intermittent native quit finalization remains tracked under [`#1716`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716).

## Basis and boundaries

- **Evidence model:** **Confirmed** facts hold within their linked static or observed boundary;
  **Inferred** readings connect accepted rules; **Unknown** marks unresolved original behavior.
  See [Three-Layer Boundary](documentation-roadmap.md#three-layer-boundary) and
  [Design and Support Scope](documentation-roadmap.md#design-and-support-scope).
- **Remake support:** Running code and recorded observations live in
  [capability status](../../remake/docs/capability-status.md#campaign-feature-trace); design
  coverage never implies full-game playability or public asset redistribution rights.
