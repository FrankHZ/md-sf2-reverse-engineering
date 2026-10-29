# Documentation Index

The English index is canonical; [中文入口](./README.zh-CN.md) provides Chinese titles and reading routes.
Research describes original evidence, contracts express behavior, synthesis explains connected rules,
and remake documentation describes implemented capability. Each owner retains its facts and Unknowns.

## Choose a Reading Route

| Read to… | Follow… |
| --- | --- |
| Understand the game | [Gameplay overview](./design/synthesis/gameplay-overview.md) → [themes](#themes-and-design-governance) → a chapter/example → its named contract/evidence |
| Find a location or encounter | [Chapter guides](#chapter-guides-grans-opening-and-departure) and [later story sections](#later-chapters-use-the-connected-story-sections) |
| Check an exact rule | [Combat contracts](#contracts-combat-turns-and-encounter-decisions), [party/resources](#contracts-party-definitions-growth-and-services), [exploration](#contracts-exploration-scripts-and-world-state) or [interface services](#contracts-dialogue-input-windows-and-audio) |
| Run or change the remake | [Local play](../remake/README.md#run-locally) → [capability status](../remake/docs/capability-status.md) → [architecture](../remake/docs/architecture.md) / [verification](../remake/docs/development-and-verification.md#scope) |
| Reproduce an original claim | [Research](#research), [coverage](./research/source-coverage.md) and the owner's command/fixture |
| Resume a task | Inspect live Git/worktrees, apply [AGENTS](../AGENTS.md) once, then use [agent resume](./operations/agent-resume.md) and [governance](./operations/github-project-governance.md#task-lifecycle) |

## Folder Map and Authority

| Folder | What belongs here |
| --- | --- |
| `research/` | Source/ROM/fixture/runtime evidence; chapter evidence is routed by [story coverage](./research/story-content-coverage.md). |
| `design/synthesis/` | Player-facing themes and chapter guides consuming accepted evidence. |
| `design/contracts/` | Implementation-neutral, evidence-bound subsystem contracts. |
| `design/zh-CN/` | Chinese mirrors of the same design-relative paths; English remains canonical. |
| `decisions/`, `operations/` | Decisions, setup, ownership and work/verification procedures. |
| `../remake/docs/` | Current implementation and play/verification instructions, separate from original evidence. |

Keep these existing ownership and mirror boundaries. Themes and chapters below organize reading
without moving their source files or changing existing link destinations. Tracked documents and exact
Git objects are durable state; old chat and launch scripts do not supersede current owners.

## Design

Contracts express implementation-neutral original behavior; synthesis explains accepted research and
separately identified remake choices. Remake implementation does not establish original evidence.
Start with the [roadmap's current agreement](./design/documentation-roadmap.md#current-baseline-and-near-term-synthesis):
the [overview](./design/synthesis/gameplay-overview.md) leads to story/chapters, tactical rules,
party/resources and exploration/continuity. [Story Progression's six chapter groups](./design/synthesis/story-progression.md#read-story-content-in-segments)
connect selected accepted static content through ending/reset, with detailed guides and evidence
at their named boundaries. The [five-theme source disposition](./design/documentation-roadmap.md#selected-english-source-set-and-reconciliation)
keeps integrated source acceptance separate from the bounded playable remake milestone.
The previously accepted design corpus has complete Chinese mirrors at its independently reviewed baselines. The new English battle-analysis pilot has no mirror (`pending`). The [Chinese reader entry](./README.zh-CN.md)
connects themes, chapter guides and English evidence owners. Later chapter sections that link directly
to English research are not pending Chinese design guides. English remains canonical; translation
does not advance original observations or playable remake support.

Some current English sources have changed since their Chinese mirrors' review anchors. Those
outstanding differences require document-by-document semantic review; acceptance at a reviewed
baseline does not establish synchronization with the current English version. Consult the
[translation index](../manifests/zh-translation-index.json) and `uv run --locked sf2 zh-meta list`
for the affected records. Their anchors remain unchanged pending review.

The [continuing synchronization agreement](./design/documentation-roadmap.md#bounded-translation-repair-agreement)
uses the existing glossary, translation index and original `sf2 zh-meta` checks. Future source changes
require stable accepted content, full bilingual review and reader-route acceptance before affected
anchors are updated. Metadata PASS alone is insufficient. Content and navigation changes require
independent semantic and reader-route acceptance; main-gate owns integration and epic completion decisions.

Existing themes explain rules, conditional story flow and selected encounter exceptions. The
[battle-design analysis pilot](./design/synthesis/battle-design-analysis-pilot.md) adds three bounded
examples: Battle01, Taros and Chessboard, with explicit preparation assumptions and simple responses
with tradeoffs. Design readings are Inferred; they do not establish author intent or difficulty.

### Themes and design governance

- [Design Localization Glossary (EN → 中文)](./design/glossary.md)
- [Documentation Roadmap and Governance Boundaries](./design/documentation-roadmap.md)
- [Gameplay Overview and System Boundaries](./design/synthesis/gameplay-overview.md)
- [Tactical Battle Loop and State Handoffs](./design/synthesis/tactical-battle-loop.md)
- [Battle Design Analysis: Battle01, Taros and Chessboard](./design/synthesis/battle-design-analysis-pilot.md) — English pilot; Chinese pending
- [Progression and Economy](./design/synthesis/progression-and-economy.md)
- [Story Progression](./design/synthesis/story-progression.md)
- [Map Design Principles: Exploration, Interaction and World Continuity](./design/synthesis/map-design-principles.md)

### Chapter guides: Grans opening and departure

- [Opening through Battle 01: Locations, People and Return](./design/synthesis/opening-through-battle01.md) — selected locations,
  dialogue/entities, encounter and usable return; original evidence and remake composition stay distinct
- [Battle 01 Return through Battle 02 Admission](./design/synthesis/battle01-return-through-battle02.md) — castle
  return, exorcism/commission, Jaha and conditional admission; static content and natural-route Unknowns
- [Battle 02 Outcome through Battle 03 Admission](./design/synthesis/battle02-outcome-through-battle03.md) — outcome/return,
  conditional Yeel/Kazin guidance and source admission; following, request and natural-route boundaries
- [Battle 03 Outcome through Battle 04 Admission](./design/synthesis/battle03-outcome-through-battle04.md) — conditional return,
  Hawel-house scene, Kazin recruitment and exit/admission; caller, follower and natural-route limits
- [Battle 04 Outcomes through Battle 05 Admission](./design/synthesis/battle04-outcomes-through-battle05.md) — victory/loss/escape,
  Galam captivity, Slade following and guard unlock/admission; conditional content and natural-route limits
- [Battle 05 Outcomes through Battle 06 Admission](./design/synthesis/battle05-outcomes-through-battle06.md) — position/HP,
  Slade recruitment, Galam exits and conditional entry; outcome, party/follower and natural-route limits
- [Battle06 Outcomes, Granseal Content and Battle07 Admission](./design/synthesis/battle06-outcomes-through-battle07.md) — outcomes,
  Granseal/Kiwi/royal following and conditional tower entry; content, request and natural-route limits
- [Battle07 Outcomes and the Exploration Handoff](./design/synthesis/battle07-outcomes-and-aftermath.md) — outcomes,
  phased tower scene, jewel/follower flags and conditional restored exploration; caller and natural-route limits
- [Post-tower Return through Earthquake-town Entry](./design/synthesis/post-tower-return-through-earthquake-entry.md) —
  tower/castle requests, earthquake-town first/repeat entry and conditional control; walking and caller limits
- [Earthquake-town Exit through Docks Departure](./design/synthesis/earthquake-town-through-docks-departure.md) —
  changed harbor, boarding choice, phased departure and conditional Map73 handoff; walking, zone and service limits
- [Voyage and Landing through Damaged-ship Control](./design/synthesis/voyage-through-damaged-ship-control.md) —
  voyage, landing, Jaha/plank staging and conditional Map14 control; actor, init and natural-route limits

### Chapter guides: New Granseal and eastern Parmecia

- [Damaged-ship Plank Choices through Battle08 Admission](./design/synthesis/damaged-ship-plank-through-battle08.md) —
  plank choices, entry alternatives, timber/rescue scene and conditional Battle08 before/start handoff.
- [Battle08 Outcomes through the New Granseal Castle Handoff](./design/synthesis/battle08-outcomes-through-new-granseal-handoff.md) —
  first/repeat outcomes, worker recovery, conditional construction and outgoing castle handoff; caller and service limits.
- [New Granseal Castle: the Year-Later Address and First Control Handoff](./design/synthesis/new-granseal-castle-year-later-control.md) —
  first/repeat castle init, year-later address, dismissal and conditional control; setup, actor and service limits.
- [New Granseal Town: Peter's First Introduction](./design/synthesis/new-granseal-town-through-peter-introduction.md) —
  conditional castle exit, town setup, independent introductions and Peter reception; hiding and control limits.
- [Peter's Castle Audience and the Return to Town](./design/synthesis/peter-castle-audience-through-town-return.md) —
  conditional audience, Peter's join/AI and following, gift discrepancy and town return; caller and control limits.
- [New Granseal Departure through Battle09 Admission](./design/synthesis/new-granseal-departure-through-battle09.md) —
  selected overworld connections, scrolling/ordinary entry, partial reload and empty Battle09 opening; request and readiness limits.
- [Battle09 Outcomes through Ribble's Welcome](./design/synthesis/battle09-outcomes-through-ribble-welcome.md) —
  variable return, separate town entry, suspicion and Peter's reconciliation; reload, roof and conditional-control limits.
- [Ribble's Optional May Recruitment through Battle10 Admission](./design/synthesis/ribble-may-through-battle10-admission.md) —
  optional recruitment, membership/current following, advice and conditional Battle10 entry; request and natural-route limits.
- [Battle10 Outcomes through the Optional East Shrine Visit](./design/synthesis/battle10-outcomes-through-east-shrine-visit.md) —
  battle return, optional shrine visit, independent conversations and current/rebuilt following; caller and natural-route limits.
- [East Shrine Return through Battle11 Cave Admission](./design/synthesis/east-shrine-return-through-battle11-admission.md) —
  optional return, selected overworld connections and cave opening; admission, cast and darkness-presentation limits.
- [Battle11 Outcomes through Polca's First Oddler Arrival](./design/synthesis/battle11-outcomes-through-polca-arrival.md) —
  cave return, selected Polca entry and the injured-boy scene; rebuilt residents/following and conditional-control limits.
- [Polca's Fairy Interaction through Battle12 Admission](./design/synthesis/polca-fairy-through-battle12-admission.md) —
  optional turtle test, independent east departure and searching-devils confrontation; ordered battle admission and presentation limits.
- [Battle12 Outcomes through Bedoe's Initial Guard Encounter](./design/synthesis/battle12-outcomes-through-bedoe-entry.md) —
  first/repeat outcomes, Peter's response and initial Bedoe guard exchange; setup, caller and conditional-control limits.
- [Bedoe's King Address and First Audience](./design/synthesis/bedoe-king-address-through-first-audience.md) —
  independent king address/audience, guard hiding, introductions and invitation; shared actors, following and conditional-control limits.
- [Bedoe to the Volcanon Audience](./design/synthesis/bedoe-to-volcanon-audience.md) — selected shrine entry,
  Volcanon conversation and Peter's joining; actor attribution, capacity/AI/following and conditional Map11 control limits.
- [Volcanon Return through the Bedoe King Choice](./design/synthesis/volcanon-return-through-bedoe-king-choice.md) —
  selected shrine return, optional king choice and Luke's requested departure; prompt, hiding/re-entry and conditional-control limits.
- [Bedoe's Hatchling Flight and Rescue](./design/synthesis/bedoe-hatchling-flight-and-rescue.md) —
  optional demonstration/repeat and independent fall/rescue; shared actor state, hiding/re-entry and two conditional-control limits.
- [Bedoe Return through Battle13 Admission](./design/synthesis/bedoe-return-through-battle13-admission.md) —
  selected return, Polca confrontation, Gerhalt joining and empty battle-start handoff; cast, party, request and readiness limits.
- [Battle13 Outcomes through Polca's Raft Grant](./design/synthesis/battle13-outcomes-through-polca-raft-grant.md) —
  battle outcomes, Luke accompanying, fresh Polca return and optional mayor bargain; ordered raft state and conditional-control limits.
- [Polca's Oddler Choices and Following](./design/synthesis/polca-oddler-choice-and-following.md) —
  two independent requests, refusal/convergence and current/future following; prompt, actor and conditional-control limits.
- [Polca Departure and Luke Recruitment](./design/synthesis/polca-departure-through-luke-recruitment.md) —
  selected departure, independent init and Luke joining; actor, cache, hide/unlock and conditional-control limits.

### Chapter guides: southern travel, Caravan and Creed

- [Polca Raft Boarding and South Parmecia Entry](./design/synthesis/polca-raft-boarding-through-south-parmecia-entry.md) —
  separate boarding request, staged raft mode and selected Map69 exits; actor waits, controller and entry/control limits.
- [South Parmecia, the Kraken Reveal and Battle16 Entry](./design/synthesis/south-parmecia-through-kraken-battle16-entry.md) —
  selected battlefield entry, complete Kraken reveal and fresh Battle16 rebuild; scene identity, start/saved-entry and completion limits.
- [Battle16 Outcomes, Disembark and Conditional Hassan Entry](./design/synthesis/battle16-outcomes-through-hassan-entry.md) —
  first/repeat outcomes, separate disembark and conditional Hassan cast/init; tuple, actor-wait, transport and control limits.
- [Hassan Ancestry Conversation and Rohde's Caravan Choice](./design/synthesis/hassan-ancestry-and-rohde-caravan-choice.md) —
  independent priest/talk/selected-item paths, complete choices and conditional following/control; lore, actor and menu limits.
- [Ribble's Tree Opening and the Achilles Sword](./design/synthesis/ribble-tree-opening-and-achilles-sword.md) —
  selected Panel/tree/Sword puzzle, capacity and conditional custody/return; item-use, admission and natural-path limits.
- [Hassan Departure, Battle17 Entry and Conditional Returns](./design/synthesis/hassan-departure-and-battle17-outcomes.md) —
  ordered Battle9/17 admission, walking/raft and partial reload, fresh/saved entry and conditional returns.
- [Southwest Parmecia, Taros's Warning and Conditional Battle18 Entry](./design/synthesis/southwest-parmecia-through-taros-battle18-entry.md) —
  in-place western scroll, conditional warning/retreat and complete before/start; scene/neutral Caravan and control limits.
- [Battle18 Outcomes and Caravan Acquisition](./design/synthesis/battle18-outcomes-and-caravan-acquisition.md) —
  outcomes, complete driver/miniaturization scene and conditional return; actual flags, actor identity, savepoint and service limits.
- [Hassan Rick Recruitment and Following](./design/synthesis/hassan-rick-recruitment.md) —
  optional population/talk, complete recruitment and current following; separate membership, reentry and control limits.
- [Caravan Boarding and Southwest Parmecia Travel](./design/synthesis/caravan-boarding-and-southwest-parmecia-travel.md) —
  boarding, mode1 movement/landing and separate walking Battle19 request/admission; actor, residual-parameter and natural-route limits.
- [Battle19 Outcomes and Independent Elven Village Entry](./design/synthesis/battle19-outcomes-through-elven-village-entry.md) —
  shared battle outcomes and independent village arrival; supplied population/mode, savepoint and conditional-control limits.
- [Elven Village Conversations and Local Exploration](./design/synthesis/elven-village-conversations-and-local-exploration.md) —
  optional dialogue, local search and current-map stairs; item-kind priority, recipient/rollback and conditional-control limits.
- [Elven Village Departure Through Harpy Pond Entry](./design/synthesis/elven-village-departure-through-harpy-pond-entry.md) —
  selected exits, independent scroll and initial help scenes; separate admission/population/prompt and conditional-control limits.
- [Harpy Pond Approach Through Battle20 Entry](./design/synthesis/harpy-pond-approach-through-battle20-entry.md) —
  confrontation, separate warning/exit and conditional fresh/saved battle entry; flag/request, cast and service limits.
- [Battle20 Outcomes and Elric Recruitment](./design/synthesis/battle20-outcomes-and-elric-recruitment.md) —
  first/repeat outcomes, complete recruitment and conditional pond/savepoint return; force membership versus temporary following and service limits.
- [Harpy Pond Departure through Fairy Cave Entry](./design/synthesis/harpy-pond-departure-through-fairy-cave-entry.md) —
  pond exit, conditional passage and cave entry; actor membership, live blocks/F751, pending request and control limits.
- [Fairy Cave Exploration and Devil's Tail Return](./design/synthesis/fairy-cave-exploration-and-devils-tail-return.md) —
  four rewards, both returns and conditional rock inspection; search/rollback, independent admission and service limits.
- [Battle21 Opening and Initial Roster](./design/synthesis/battle21-opening-and-initial-roster.md) —
  refusal/threat scene and conditional initial placements; independent flags, cropped coordinates, item masking and service limits.
- [Battle21 Outcomes and Devil's Tail Return](./design/synthesis/battle21-outcomes-and-devils-tail-return.md) —
  leader cleanup, complete first after scene and conditional full-map return; repeat seed, follower and service limits.
- [Creed Mansion Entry and the First Goliath Obstruction](./design/synthesis/creed-mansion-entry-and-first-goliath-scene.md) —
  separate arrival and complete first obstruction; setup/cast limits, live door copies, delayed flag and conditional return/repeat.
- [Creed's Magic Door and the Desktop Transfer](./design/synthesis/creed-magic-door-and-desktop-transfer.md) —
  independent door gate, complete shrinking/collection scene and conditional Desktop admission; temporary sprite, live copy and pending/control limits.
- [Desktop Kingdom: First Arrival and Conditional Control](./design/synthesis/desktop-kingdom-first-arrival.md) —
  fresh population, complete landing/discussion/farewell and conditional following/control; supplied-state, temporary sprite and service limits.
- [Desktop Kingdom: Conversations and the Chessboard Challenge](./design/synthesis/desktop-kingdom-interactions-through-battle22-entry.md) —
  conversations/services, King permission and conditional Chessboard admission; actual flags, pending/wrapper and pre-opening limits.
- [Chessboard: Battle22 Initialization and Initial Roster](./design/synthesis/battle22-initialization-and-roster.md) —
  empty hooks, party placement, sixteen chess enemies and rebuilt board/neutral actors; fresh/current-state and pre-activation limits.
- [Chessboard Outcomes and the Return to Desktop Kingdom](./design/synthesis/battle22-outcomes-and-desktop-return.md) —
  leader/count outcomes, rebuilt Desktop, reward announcement and King/Maid repeats; seed, actor, service and acquisition limits.
- [Cotton Balloon Acquisition and the Departure to Floor World](./design/synthesis/cotton-balloon-through-floor-world-entry.md) —
  chest delivery, guard clearance, own-tile use and complete departure through conditional Map26 admission; custody, actor/wait and pre-population limits.
- [Floor World First Arrival and Conditional Exploration](./design/synthesis/floor-world-first-arrival.md) —
  fresh population, complete landing/pop and conditional exploration; independent actions, lookup/park, repeat-EGRESS and service/control limits.
- [Floor World Residents, the Giant Ring and Outbound Exits](./design/synthesis/floor-world-interactions-and-exits.md) —
  all residents/default, giant-ring attempt and three outbound requests; separate waits, hide/parking, coordinate and conditional admission limits.
- [Willard Burrow Exploration and Battle23 Admission](./design/synthesis/willard-burrow-through-battle23-entry.md) —
  fresh/retained population, eight traversal/exit requests and complete confrontation through conditional Battle23 admission; actor/alias, wait, coordinate and pre-initialization limits.
- [Battle23 Initial Formation and Suspended Entry](./design/synthesis/battle23-initialization-and-roster.md) —
  fresh healing/party capacity, complete enemy formation and initialized stats, crop/entity mapping versus suspended current-state load; pre-activation/pre-turn and supplied-state limits.
- [Battle23 Outcomes and the Return to Willard Burrow](./design/synthesis/battle23-outcomes-and-burrow-return.md) —
  leader/count outcomes, current-member victory seed, defeat/escape savepoint selector and fresh/retained burrow returns; independent flags, eight exit requests and pre-destination-init limits.
- [Creed's Restoration and Main-Room Departure](./design/synthesis/creed-restoration-and-basement-departure.md) —
  conditional return population, complete restoration/Zeon conversation and Creed/Fairy departure through main-room init; sprite/action/alias/HP, follower, Egress-selector and conditional-control limits.
- [Evil Spirit Shrine Entry and Consultation Admission](./design/synthesis/evil-spirit-shrine-entry-and-scene-admission.md) —
  mansion/shrine route, three warps, fresh/retained residents and the independent consultation request gate; special actor/effect allocation, coordinate/layout and conditional caller-flag limits.
- [Evil Spirit Consultation and Conditional Zone Return](./design/synthesis/evil-spirit-consultation-and-zone-return.md) —
  complete consultation, Zeon demand and explosion through conditional zone return; dialogue/transaction, actor/action, dormant hides, presentation requests and non-atomic control limits.
- [Creed Return, Fairy Following and Recruitment Choices](./design/synthesis/creed-return-fairy-and-recruitment-choices.md) —
  explicit shrine return, selected mansion population, Fairy current following and eight recruit offers; joined/active capacity, shrink parking, repeat/interruption and conditional-return limits.
- [Oddler Stays and the Fairy Handoff](./design/synthesis/oddler-stays-and-fairy-handoff.md) —
  admitted farewell, dormant hides and current following through ordered zone return; separate fresh F786/F22 populations and service/control limits.
- [Creed Departure, Dwarf Recovery and the Cannon Gift](./design/synthesis/creed-departure-dwarf-recovery-and-cannon-gift.md) —
  selected departure/mine route, Fairy recovery, capacity-dependent Cannon gift and digging; actual actors, repeated/live/fresh state and ordered caller/service/control limits.

### Chapter guides: northern travel and Pacalon

- [New Granseal Return and Astral Following](./design/synthesis/new-granseal-return-and-astral-following.md) —
  conditional mine-to-castle return, retrospective audience and adviser announcement; actual cast, current/fresh following, ordered flags and service/control/save limits.
- [New Granseal Inventor and Dynamite Gift](./design/synthesis/new-granseal-inventor-and-dynamite-gift.md) —
  castle-to-town inventor conversation, actual Cannon search and capacity-dependent Dynamite gift; permission, actor/context, receiving/retry and ordered return limits.
- [New Granseal Janet Recruitment and Departure](./design/synthesis/new-granseal-janet-recruitment-and-departure.md) —
  Janet encounter and scripted acceptance, joined/active membership, temporary following and local/fresh-town return limits.
- [New Granseal Departure through Battle24 Admission](./design/synthesis/new-granseal-departure-through-battle24-admission.md) —
  retained overworld travel, separate request/ordered admission and fresh initial encounter; supplied-state and pre-activation limits.
- [Battle24 Outcomes through North Cliff Preparation](./design/synthesis/battle24-outcomes-through-north-cliff-preparation.md) —
  actual end counting, distinct outcome returns and current-inventory cave warning; cast, movement, temporary retry and conditional local-control limits.
- [North Cliff Opening and Rohde Recruitment](./design/synthesis/north-cliff-opening-and-rohde-recruitment.md) —
  selected item removals, complete blast/join scene and local/fresh-map consequences; actual cast, partial effects and next-warp-only boundary.
- [North Cliff Entry through Battle25 Admission](./design/synthesis/north-cliff-entry-through-battle25-admission.md) —
  separate entrance request and conditional Battle25 admission, complete initial encounter and crop/view limits before enemy activation.
- [Battle25 Outcomes through Ketto Entry](./design/synthesis/battle25-outcomes-through-ketto-entry.md) —
  actual outcome returns, conditional cave departure and later admission, and Ketto's initial population through the controller request.
- [Ketto: Conversations, Shops and the Merchant's Move](./design/synthesis/ketto-interactions-and-merchant-relocation.md) —
  all resident interactions, merchant movement, service entry and inspections/items through conditional departure; choice, state and completion limits.
- [Battle26 Admission and the Geshp Confrontation](./design/synthesis/battle26-admission-and-geshp-confrontation.md) —
  ordered admission, actual field cast and complete Geshp/Higins confrontation through before-wrapper return; narrative, action, alias and fresh-initialization limits.
- [Battle26 Initial Formation and Neutral Higins](./design/synthesis/battle26-initialization-and-roster.md) —
  actual force/enemy formation, conditional stats/upgrades, neutral Higins and crop/load/start; identity, supplied-state and pre-activation limits.
- [Battle26: activation and the first turn handoff](./design/synthesis/battle26-activation-and-first-turn-dispatch.md) —
  region/activation, conditional deals alias, fresh no-spawn and randomized turn records through the first individual-turn call; supplied-state, actor and service limits.
- [Battle26 outcomes and Higins recruitment](./design/synthesis/battle26-outcomes-and-higins-recruitment.md) —
  terminal precedence, independent Black Ring transaction and complete rescue/join/following through the returned handoff; seed, capacity, action, service and later-arrival limits.
- [Battle26 return through conditional Pacalon entry](./design/synthesis/battle26-return-through-pacalon-entry.md) —
  fresh return rebuild, retained eastern scroll and conditional town/Battle31 admission through the first Map02 event/control read; population, coordinate, route and service limits.
- [Pacalon: conversations, supplies and the castle handoff](./design/synthesis/pacalon-interactions-and-castle-handoff.md) —
  both resident alternatives, shops/priest, descriptions and possible pickups through ordered outgoing exits; current selection, partial effects and pre-destination limits.
- [Pacalon castle: entry and the first drought-state visit](./design/synthesis/pacalon-castle-entry-and-first-visit.md) —
  conditional castle entry, drought-state cast/conversations, optional centaur scene, descriptions/pickups/priest and retained CURRENT stairs; south stops before town battle selection, later Frayja deferred.
- [Pacalon departure: the north scroll and conditional Battle27 admission](./design/synthesis/pacalon-departure-to-battle27-admission.md) —
  conditional town/overworld rebuilding, retained north scroll, candidate zone and partial reload through ordered27/32 admission; stops before BattleLoop initialization, with natural-route/control/persistence limits.
- [Battle27: initial formation and the restrictions confrontation](./design/synthesis/battle27-initialization-and-opening.md) —
  actual fresh formation, sixteen enemies, rebuilt crop/view and complete three-speaker opening; supplied F88 state, identity, interrupted-intro and pre-activation/pre-turn limits.
- [Battle27: activation and the first turn handoff](./design/synthesis/battle27-activation-and-first-turn-dispatch.md) —
  checked regions versus flags/activation, fresh no-spawn and randomized turn records through the first call before its body; supplied-state, deals alias, finite-buffer and presentation limits.

### Later chapters: use the connected story sections

The detailed-guide series above ends at Battle27. The same accepted Story Progression document
continues with explanatory sections and English research evidence; these are not missing translations:

- [Tristan, Mitula and restored Pacalon](./design/synthesis/story-progression.md#tristans-people-and-the-cave-route-to-mitula)
- [Moun, Nazca and the return to Grans](./design/synthesis/story-progression.md#moun-nazca-and-the-return-to-grans)
- [Final confrontations and ending](./design/synthesis/story-progression.md#final-confrontations-and-ending)
- [Worked example: Taros and the Caravan](./design/synthesis/story-progression.md#worked-example-taros-and-the-caravan) — Sword custody/equipment, admission, outcome, acquisition and separate boarding

### Milestone acceptance and historical proposals

- [Map 3 to Battle 01 Readiness Ledger](./design/synthesis/map3-battle01-readiness.md#accepted-current-milestone) —
  accepted current private milestone; original Unknowns and historical report failures retain their owners
- [Phase 4 Bootstrap Plan](./design/synthesis/phase4-bootstrap-plan.md) — **Historical** proposal; current implementation guidance is superseded
- [Continuous Map3–Battle01 scenario contract](./design/contracts/map3-battle01-continuous-scenario.md) — bounded private milestone composition

### Contracts: combat, turns and encounter decisions

- [Battle Control and Combatant Lifecycle Contract](./design/contracts/battle-control-lifecycle.md)
- [Battlefield Navigation and Target-Space Contract](./design/contracts/battlefield-navigation.md)
- [Battle Action Construction Contract](./design/contracts/battle-action-construction.md)
- [Physical Combat Resolution Contract](./design/contracts/combat-resolution.md)
- [Spell Damage Resolution Contract](./design/contracts/spell-resolution.md)
- [Battle AI Decision Contract](./design/contracts/battle-ai-decision.md)
- [Battle Encounter Definition Contract](./design/contracts/battle-encounter-definition.md)
- [Battle Functions Control-Flow Contract](./design/contracts/battle-functions-control-flow.md)
- [Battle Cutscene Routing Contract](./design/contracts/battle-cutscene-routing.md)
- [Battle Scene Command and Presentation-Data Contract](./design/contracts/battle-scene-presentation.md)

### Contracts: party, definitions, growth and services

- [Party Membership State Contract](./design/contracts/party-membership-state.md)
- [Party and Roster State Contract](./design/contracts/party-roster-state.md)
- [Combatant State Access Contract](./design/contracts/combatant-state-access.md)
- [Ally Definition and Growth-Data Contract](./design/contracts/ally-definition-data.md)
- [Enemy Definition and Presentation-Index Contract](./design/contracts/enemy-definition-data.md)
- [Item Definition and Auxiliary-Catalog Contract](./design/contracts/item-definition-data.md)
- [Spell Definition Data Contract](./design/contracts/spell-definition-data.md)
- [Spellbook State Contract](./design/contracts/spellbook-state.md)
- [Level-Up and Stat-Growth Contract](./design/contracts/level-up.md)
- [Service Interactions](./design/contracts/service-interactions.md)
- [Caravan and Deals State Contract](./design/contracts/caravan-and-deals-state.md)
- [Stats Null-Return Service Contract](./design/contracts/stats-null-return-service.md)

### Contracts: exploration, scripts and world state

- [Map and Exploration Contract](./design/contracts/map-exploration.md)
- [Exploration Control Flow Contract](./design/contracts/exploration-control-flow.md)
- [Map Entry Routing State Contract](./design/contracts/map-entry-routing-state.md)
- [Map Entity Data Contract](./design/contracts/map-entity-data.md)
- [Map Setup Data Contract](./design/contracts/map-setup-data.md)
- [Map Area-Description Routing Contract](./design/contracts/map-area-description-routing.md)
- [Map Camera Update Control-Flow Contract](./design/contracts/map-camera-update-control-flow.md)
- [Standalone Map-Script Program Data Contract](./design/contracts/standalone-map-script-program-data.md)
- [Global Flag State Contract](./design/contracts/global-flag-state.md)
- [Map 3 Controlled Admission Contract](./design/contracts/map3-controlled-admission.md) — **Proposed** bounded R1 admission contract; current acceptance has separate owners

### Contracts: dialogue, input, windows and audio

- [Dialogue Commands and Reached Text-Wait Contract](./design/contracts/dialogue-system.md)
- [Text Storage, Decoding, and Font Contract](./design/contracts/text-and-font-system.md)
- [Sprite-Dialogue Property Data Contract](./design/contracts/sprite-dialogue-property-data.md)
- [Portrait Window and State Contract](./design/contracts/portrait-window-state.md)
- [Input-System Contract](./design/contracts/input-system.md)
- [Window-System Contract](./design/contracts/window-system.md)
- [UI Layout Data Contract](./design/contracts/ui-layout-data.md)
- [Audio Command and Playback-State Contract](./design/contracts/audio-system.md)
- [Music-Wait Service Contract](./design/contracts/music-wait-service.md)
- [Name-Table Lookup Service Contract](./design/contracts/name-table-lookup-service.md)

### Contracts: graphics and presentation data

- [Map Layout Data Contract](./design/contracts/map-layout-data.md)
- [Map Palette Data Contract](./design/contracts/map-palette-data.md)
- [Map-Sprite Assignment Surface Contract](./design/contracts/map-sprite-assignment-surface.md)
- [Map Sprite Graphics Data Contract](./design/contracts/map-sprite-graphics-data.md)
- [Map Tileset Data Contract](./design/contracts/map-tileset-data.md)
- [Graphics Service and State Contract](./design/contracts/graphics-service-state.md)
- [Portrait Graphics Data Contract](./design/contracts/portrait-graphics-data.md)
- [Special-Sprite Graphics Data Contract](./design/contracts/special-sprite-graphics-data.md)
- [Special-Screen Asset Data Contract](./design/contracts/special-screen-asset-data.md)
- [UI Graphics Asset Data Contract](./design/contracts/ui-graphics-asset-data.md)
- [Battle Background Graphics Data Contract](./design/contracts/battle-background-graphics-data.md)
- [Battle Effect Graphics Data Contract](./design/contracts/battle-effect-graphics-data.md)
- [Battle Sprite Graphics Data Contract](./design/contracts/battle-sprite-graphics-data.md)
- [Battle Weapon and Ground Graphics Data Contract](./design/contracts/battle-weapon-ground-graphics-data.md)

### Contracts: startup, save and technical boundaries

- [New-Game State Initialization Contract](./design/contracts/new-game-state-initialization.md)
- [Startup Control Flow Contract](./design/contracts/startup-control-flow.md)
- [Special-Screen Control-Flow Contract](./design/contracts/special-screen-control-flow.md)
- [Save-System Contract](./design/contracts/save-system.md)
- [Randomness Services](./design/contracts/randomness.md)
- [ROM Header Data Contract](./design/contracts/rom-header-data.md)
- [Byte-Copy Service Contract](./design/contracts/byte-copy-service.md)
- [Interrupt, DMA, and Trap State Contract](./design/contracts/interrupt-dma-and-trap-state.md)
- [Debug Control Flow Contract](./design/contracts/debug-control-flow.md)
- [Unused Mapload Control-Flow Contract](./design/contracts/unused-mapload-control-flow.md)
- [Unused Menu Constant-Write Control-Flow Contract](./design/contracts/unused-menu-constant-write-control-flow.md)
- [Unused Technical Asset Data Contract](./design/contracts/unused-technical-asset-data.md)

## Research

Source, ROM, fixture and runtime provenance live in these owners. The
connection through the ending, with encounter/optional gaps and distinct original-runtime, design
and remake frontiers. The source-coverage owner and research-index commands retain registered
evidence denominators; story prose does not add fixture or index credit.

### Baseline, coverage and indexing

- [Reproducible Original Baseline](./research/reproducible-original.md)
[story-content coverage map](./research/story-content-coverage.md) routes by chapter and conditional
- [Reverse-Engineering Research Index](./research/indexing.md)
- [Source Coverage and Research Cadence](./research/source-coverage.md)
- [Story Content Coverage](./research/story-content-coverage.md)

### Stats, growth and original battle rules

- [Static Core Data Contract](./research/static-core-data.md)
- [Ally Growth and Spell Learning Contract](./research/ally-growth.md)
- [Runtime RNG and Battle-Math Call Chains](./research/runtime-rng-and-battle-math.md)
- [Promotions, Enemy Definitions, and Rewards](./research/enemy-promotions.md)
- [Battle 01 Static Scene Contract](./research/battle01-placement.md)

### Battle decisions, movement, actions and scenes

- [Battle AI Static Inventory and Decision Contracts](./research/battle-ai.md)
- [Battlefield and Pathfinding](./research/battlefield-pathfinding.md)
- [Battle Loop and Combatant Lifecycle](./research/battle-loop.md)
- [Battle Action Script Construction](./research/battle-actions.md)
- [Shared Battle Functions](./research/battle-functions.md)
- [Battle Scene Engine](./research/battle-scene-engine.md)
- [Battle Cutscene Routing](./research/battle-cutscenes.md)

### Shared scripts, exploration, menus and services

- [Common Scripting Engines](./research/common-scripting.md)
- [Common Map Engine](./research/common-maps.md)
- [Common Stats and Inventory Services](./research/common-stats.md)
- [Common Menu Engines and Services](./research/common-menus.md)
- [Field menu control](./research/field-menu-control.md)
- [Field Search Control](./research/field-search-control.md)
- [Field Item Effects](./research/field-item-effects.md)
- [Technical Interrupt, DMA, and Trap Services](./research/technical-interrupts.md)
- [Technical Graphics and Decompression Services](./research/technical-graphics.md)
- [Technical Jump Interfaces and Pointer Tables](./research/technical-interfaces.md)
- [Remaining Technical Services](./research/technical-services.md)
- [Startup, Main Loop, and Exploration Core](./research/gameflow-core.md)
- [Special Screens](./research/special-screens.md)
- [ROM Header, Window Engine, and Special Debug Flows](./research/remaining-core.md)

### Source data inventories

- [Global Battle Data Tables](./research/battle-global-data.md)
- [Ally and Class Data Inventory](./research/ally-data-inventory.md)
- [Item, Spell, and Enemy Data Inventory](./research/core-stats-data-inventory.md)
- [Battle Cutscene Data Inventory](./research/battle-cutscene-data.md)
- [Battle Spriteset Data Inventory](./research/battle-spriteset-data.md)
- [Battle Routing and Terrain Data Inventory](./research/battle-routing-data.md)

### Opening and Battle01: controlled observations and retained limits

- [Map 3 to Battle 01 Research Gap Audit](./research/map3-battle01-audit.md)
- [Map 3 Admitted-Start Evidence](./research/map3-admitted-start.md)
- [Map 3 Controlled-Start Egress Transition](./research/map3-controlled-start-egress-transition.md)
- [Map 3 Original Player Locomotion and Animation](./research/map3-original-player-locomotion-animation.md)
- [Map 3 Natural Route to Messenger Entry](./research/map3-battle01-natural-route.md)
- [Map 3 Messenger Acceptance](./research/map3-messenger-acceptance.md)
- [Map 3 optional interactions: static inventory](./research/map3-optional-interactions.md)
- [Map 3 Castle to Battle 01 Unlock — Static Fallback](./research/map3-castle-battle-unlock.md)
- [Map 3 to Battle 01 Admission — Static and Bounded Player-Ready Contracts](./research/map3-battle01-admission.md)
- [Map 3 to Battle 01 Turn/Control — Static Contract](./research/map3-battle01-turn-control.md)
- [Map 3 to Battle 01 Action/Effect — Static Contract](./research/map3-battle01-action-effect.md)
- [Map 3 to Battle 01 Action Completion — Static Contract](./research/map3-battle01-action-completion.md)
- [Map 3 to Battle 01 Turn Finalization — Static Contract](./research/map3-battle01-turn-finalization.md)
- [Map 3 Battle 01 victory and return](./research/map3-battle01-victory-return.md)
- [Map 3 Original Player Reference Frame](./research/map3-original-player-reference-frame.md)
- [Map 3 Entity 142 Interactable Reference](./research/map3-entity142-interactable-reference.md)
- [Original-reference replay capability](./research/original-reference-replay-capability.md)
- [Original-reference replay scenario API](./research/original-reference-replay-scenario-api.md)

### Map data, event state and caller handoffs

- [Complete Map ASM Inventory](./research/map-data-inventory.md)
- [Map-Event Direct Fixed-RAM State](./research/map-event-direct-state.md)
- [Map-Event Direct-Flag Lifecycle State](./research/map-event-flag-lifecycle-state.md)
- [Map-Event Cross-Program Flag State](./research/map-event-cross-program-flag-state.md)
- [Map-Event Flag Route Selection](./research/map-event-flag-route-selection.md)
- [Map-Event Interaction State](./research/map-event-interaction-state.md)
- [Map-Event Item Transactions](./research/map-event-item-transactions.md)
- [Map-Event Combatant-State Callers](./research/map-event-combatant-state.md)
- [Map-Event Random-Battle State](./research/map-event-random-battle-state.md)
- [Map-Event Tactical-Base Quote State](./research/map-event-tactical-base-quote-state.md)
- [Map-Event Scripted Transition State](./research/map-event-scripted-transition-state.md)
- [Map-Event Direct Control Topology](./research/map-event-direct-control.md)
- [Map-Event Dialogue State](./research/map-event-dialogue-state.md)
- [Map-Event Request State](./research/map-event-request-state.md)
- [Map-event request consumption](./research/map-event-request-consumption.md)
- [Map-Event Direct Handoff](./research/map-event-direct-handoff.md)
- [Map-Event Predicate Results](./research/map-event-predicate-results.md)
- [Map Content Tables and Binary Payload Parity](./research/map-content.md)

### Auxiliary data and sound

- [Auxiliary Graphics, Scripting, and Technical Data Inventory](./research/auxiliary-data-inventory.md)
- [Z80 Music-Bank and Runtime-State Inventory](./research/sound-data-inventory.md)


## Operations

[Remake entry](../remake/README.md), [development and verification](../remake/docs/development-and-verification.md)
and [retained comparisons](../remake/docs/evidence/retained-comparisons.md) own current engine usage,
scoped acceptance and historical failures. Read those owners before applying an old gate recipe.

Current task routing, private-input selection and independent review.

- [Agent Resume Route](./operations/agent-resume.md)
- [Bounded Data Work, Inspection, and Review Runbook](./operations/bounded-inspection-and-review.md)
- [GitHub Project Governance](./operations/github-project-governance.md) — Issues/Project own
  coordination state; tracked documents own evidence and accepted behavior
- [Local Private Inputs and Shared Tools](./operations/local-private-inputs.md)
- [Phase 2 Research and Design Lane Runbook](./operations/phase2-lane-runbook.md)
- [Documentation and Agent Instruction Audit](./operations/documentation-agent-audit.md)

## Decisions

Read scope and status before applying a decision. ADR 0018 supersedes old model/mandatory-worker wording. ADR 0019 records the adopted engine direction, current M0/M1 boundaries and explicit test policy; older blanket gate/retention requirements do not override it.

- [ADR 0001: BizHawk for H3 Runtime Observation](./decisions/0001-bizhawk-for-h3-runtime-observation.md)
- [ADR 0002: Python and uv for Project Tooling](./decisions/0002-python-and-uv-for-project-tooling.md)
- [ADR 0003: Static-First Research with Batched Runtime Observation](./decisions/0003-static-first-batched-runtime-research.md)
- [ADR 0004: One Terra Research Worker with Root Acceptance](./decisions/0004-single-terra-worker-with-root-acceptance.md)
- [ADR 0005: Remake-Value-Driven Driver and Hardware Freeze](./decisions/0005-remake-value-driven-driver-freeze.md)
- [ADR 0006: Parallel Worktrees with Topic-Branch Integration](./decisions/0006-parallel-worktrees-and-topic-branch-integration.md)
- [ADR 0007: Separate Schema Shape from Golden Evidence](./decisions/0007-schema-contract-composition-and-migration.md)
- [ADR 0008: Godot C# with a CLI-First Remake Toolchain](./decisions/0008-godot-csharp-cli-first-remake-tooling.md)
- [ADR 0009: First Phase 4 Playable Slice](./decisions/0009-first-phase4-playable-slice.md)
- [ADR 0010: Map 3 to Battle 01 Product Acceptance Profile](./decisions/0010-map3-battle01-product-acceptance.md)
- [ADR 0011: Phase 4 Remake Runtime Architecture](./decisions/0011-phase4-remake-runtime-architecture.md)
- [ADR 0012: Dependency-Aware Partitioned Verification](./decisions/0012-dependency-aware-partitioned-verification.md)
- [ADR 0013: Token-Efficient Agent Research without Weakening Evidence](./decisions/0013-token-efficient-agent-research-workflow.md)
- [ADR 0014: Static-First Runtime Evidence after Map 3 to Battle 01](./decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
- [ADR 0015: Original-Reference Replay and H4 Boundary](./decisions/0015-original-reference-replay-and-h4-boundary.md)
- [ADR 0016: Remake-Start Evidence Deferral](./decisions/0016-remake-start-evidence-deferral.md)
- [ADR 0017: Heavy Boundaries, Light Internals](./decisions/0017-heavy-boundaries-light-internals.md)
- [ADR 0018: Task Model Routing After the Astra Trial](./decisions/0018-astra-role-routing-trial.md)
- [ADR 0019: State- and Content-Driven Remake Engine](./decisions/0019-state-and-content-driven-remake-engine.md)

## Evidence Vocabulary

- **Confirmed:** reproduced by the owning command or supported by appropriate named original evidence.
- **Inferred:** supported but not independently reproduced at the required boundary.
- **Unknown:** an explicit unresolved question, not an automatic emulator queue.

[README](../README.md) owns project scope, [AGENTS](../AGENTS.md) current working rules, and the linked
topic documents their detailed claims. History stays in Git and retained evidence.
