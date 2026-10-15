# Documentation Roadmap and Governance Boundaries

- Status: **Confirmed repository governance guidance**; this document is not evidence about the
  original game and does not select a remake engine, product, platform, or commercial direction.
- Scope: organize sourced contracts into concise player-facing explanations without changing their
  evidence labels; distinguish original evidence, accepted remake choices and future work at their
  owning decision and acceptance boundaries.

## Authoring Language Policy

**Confirmed repository policy:** during the current design-synthesis phase, English is the canonical
authoring and review language for new or materially revised design-synthesis documents. Preserve
source-faithful identifiers, fixture IDs, evidence labels, and code vocabulary rather than inventing
translated equivalents.

**Confirmed repository decision (2026-08-04):** the project glossary is accepted at
[`glossary.md`](./glossary.md). It is the single binding source for English-to-Chinese terminology
in design-synthesis documents and governs the zh-CN mirror conventions under
`docs/design/zh-CN/`. Do not create ad hoc bilingual terminology; use the glossary's fixed terms and
rules (evidence-label translations, preserved source identifiers, one-term-one-translation, proper
nouns kept in English with suggested annotations, and entry revision process).

Non-English localization from the canonical English source proceeds as a dedicated batch per the
glossary's rules, with terminology consistency, link integrity, evidence-label preservation, and
fixture-trace QA. A zh-CN mirror under `docs/design/zh-CN/` is a derivative; the English source
remains the review baseline unless a localization batch defines another explicit policy.

**Confirmed current sequencing decision:** Chinese synchronization under
[the design-documentation epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514)
is deferred until the selected theme's English source is stable. Stability means its substantive
English revisions have been independently accepted and merged, and no planned rewrite remains for
that theme in this round. Main-gate records that readiness before dispatching a separately scoped
translation batch. [#434](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/434) remains
deferred; completing the baseline calibration alone does not dispatch it or establish stability for
all themes. Chinese translation and continuing synchronization remain eventual epic deliverables.

During this English round, preserve existing Chinese mirrors, glossary and translation metadata,
including known drift and retained diagnostic failures. Do not refresh source anchors to conceal
drift or claim global translation PASS. A later translation batch must translate and review the
substantive content before using the existing source-anchoring checks.

## Design Information Architecture

**Confirmed repository policy:** `docs/design/` uses ownership and evidence role, rather than game
feature alone, as its first-level information architecture:

- `contracts/` contains evidence-bound subsystem contracts. These remain owned by the research slice
  when accepted findings change.
- `synthesis/` contains cross-subsystem or player-facing explanations that consume accepted evidence
  from `main` and remain owned by the design-synthesis lane.
- repository-governance documents, including this roadmap and `glossary.md`, remain at the design
  root because they govern both categories.
- `zh-CN/` preserves the canonical English relative hierarchy: for example,
  `contracts/save-system.md` maps to `zh-CN/contracts/save-system.md`.

This split is an ownership boundary, not a claim about original-game architecture and not a future
engine module layout. Add deeper subdirectories only when a concrete slice establishes a durable
navigation or ownership need; do not create empty category scaffolding.

## Three-Layer Boundary

| Layer | Ownership and allowed content | Prohibited shortcut |
| --- | --- | --- |
| A. Original behavior/data contracts | `docs/research/`, `schemas/`, manifests, H2/H3 fixtures, and sourced design contracts record **Confirmed**, **Inferred**, and **Unknown** facts. | A source macro, field, or symbol name does not automatically establish player-visible meaning. Preserve the source label and explain only interpretations supported by evidence. |
| B. Reconstructed design explanation | `docs/design/` may explain player-facing consequences supported by Layer A, link the local research/fixture owner, and mark gaps as **Unknown**. | Do not promote a static call, address, or plausible reading into original behavior, a campaign conclusion, or a player-experience fact. |
| C. Future remake decisions | Modernization, implementation intent, and product choices belong in explicit decisions and separate expected-deviation/H4 acceptance boundaries. | Do not rewrite modernization as original behavior or use a synthesis document to choose an engine or product direction. |

**Confirmed repository rule:** Layer B is a traceable interpretation of accepted Layer A evidence,
not a second evidence system. **Inferred** interpretations retain that label, and **Unknown** behavior
remains a question rather than being completed as narrative.

## Pre-Synthesis Evidence Review

**Confirmed repository rule:** every Layer B synthesis slice must adversarially review the Layer A
evidence it will explain. A link to an accepted document is necessary but not sufficient. The review
must inspect, where present, the owning research prose, evidence-bound design contract, executable
fixture payload and exact fixture ID, schema/verifier or focused test, and the narrow H2/H3 command
that owns the claimed quantity, unit, order, or state transition.

The review must specifically test for stale question queues, summary prose that is broader than its
fixture, units reused across different lifecycle stages, source-static call order described as a
runtime outcome, and controlled validation seams presented as natural campaign behavior. Record the
surfaces checked and the disposition in the synthesis document or its review record.

When owners disagree, Layer B must not select the convenient answer or silently repair Layer A.
Exclude the disputed conclusion or retain it as **Unknown**, report the exact mismatch to the owning
research lane, and wait for an accepted owner correction before expanding the synthesis. A stale
queue or over-broad summary may be nonblocking only when the executable owner and the stricter claim
boundary agree; the synthesis must use that stricter boundary and keep the discrepancy visible to
reviewers.

## Current Baseline and Near-Term Synthesis

**Current scope agreement — Standard.** The accepted English
[Gameplay Overview](synthesis/gameplay-overview.md) is the entry to the design material.
[Opening through Battle 01](synthesis/opening-through-battle01.md) explains the selected opening,
castle/tower, encounter and usable return;
[Battle 01 return through Battle 02 admission](synthesis/battle01-return-through-battle02.md) continues
through castle return, exorcism/commission, Jaha and conditional admission.
[Battle 02 outcome through Battle 03 admission](synthesis/battle02-outcome-through-battle03.md)
continues through conditional Yeel/Kazin guidance and the separate zone request.
[Battle 03 outcome through Battle 04 admission](synthesis/battle03-outcome-through-battle04.md)
continues through Hawel's house, Kazin recruitment and the separate exit request. These accepted
outcomes retain their evidence and natural-route limits.
[Battle04 outcomes through Battle05 admission](synthesis/battle04-outcomes-through-battle05.md)
extends the accepted route through Galam captivity/escape and the guards' conditional admission.
[Battle05 outcomes through Battle06 admission](synthesis/battle05-outcomes-through-battle06.md)
continues through Slade's explicit recruitment and Galam's exits.
[Battle06 outcomes through Battle07 admission](synthesis/battle06-outcomes-through-battle07.md)
continues through Granseal's changed town/castle content, optional Kiwi recruitment, royal following
and conditional tower admission. These accepted guides retain their evidence and natural-route limits.
[Battle07 outcomes and the exploration handoff](synthesis/battle07-outcomes-and-aftermath.md)
continues through outcome branches, phased tower aftermath, jewel/follower flags and the conditional
Map58 control boundary. These accepted guides retain their own caller, natural-route and service
limits; they do not establish a complete campaign or expand remake acceptance.

[Post-tower return through earthquake-town entry](synthesis/post-tower-return-through-earthquake-entry.md)
continues through selected castle content, independent Astral dialogues and first/repeat town init.
Its conditional control/event boundary retains the caller, follower, walking and presentation limits.

[Earthquake-town exit through docks departure](synthesis/earthquake-town-through-docks-departure.md)
continues through the changed harbor, boarding choice and conditional Map73 request handoff, with
walking, zone, follower and service limits retained.

[Voyage and landing through damaged-ship control](synthesis/voyage-through-damaged-ship-control.md)
continues through Map73 first/repeat selection, optional Kiwi dialogue, landing/crowd instructions,
Jaha/plank staging and the first conditional Map14 control/event read. It retains in-script-load,
actor/follower and service/pending limits.

[Damaged-ship plank choices through Battle08 admission](synthesis/damaged-ship-plank-through-battle08.md)
continues through plank/rescue choices, caller departure writes, Map72 selection and before/start
handoffs, retaining entry, actor, pending and suspended-route limits.

[Battle08 outcomes through the New Granseal castle handoff](synthesis/battle08-outcomes-through-new-granseal-handoff.md)
continues through first/repeat outcomes, worker recovery, conditional construction and the outgoing
castle admission. It retains the separate battle/exploration consumers and stops before Map7 init.

[New Granseal castle: the year-later address and first control handoff](synthesis/new-granseal-castle-year-later-control.md)
continues through setup priority, the rebuilding/trade address, dismissal, ordinary population
replacement and conditional control. It retains actor, service and natural-route limits.

[New Granseal town: Peter's first introduction](synthesis/new-granseal-town-through-peter-introduction.md)
continues through conditional castle departure, town setup/init, independent sailor/guard and Peter
zones, hiding and the first post-event pending/control read. Its own natural-route and service
limits remain applicable.

[Peter's castle audience and the return to town](synthesis/peter-castle-audience-through-town-return.md)
continues through separate castle admission, Peter accompanying the party while Astral stays,
membership/AI and distinct following, the gift discrepancy and conditional town control.
Its caller, actor, service and natural-route limits remain applicable.

[New Granseal departure through Battle09 admission](synthesis/new-granseal-departure-through-battle09.md)
continues through selected overworld connections, distinct scrolling/ordinary entry, partial reload
and empty before/start scripts. Its request, service and natural-route limits remain applicable.

[Battle09 outcomes through Ribble's welcome](synthesis/battle09-outcomes-through-ribble-welcome.md)
continues through variable return, separate town entry, suspicion, Peter's explanation and conditional
restored control. Its independent flags, population/block/roof consumers and natural-route limits
remain applicable.

[Ribble's optional May recruitment through Battle10 admission](synthesis/ribble-may-through-battle10-admission.md)
continues through optional joining/current following, travel advice, selected departure and conditional
before/start admission. It retains membership, actor lifetime, request and natural-route limits.

[Battle10 outcomes through the optional East Shrine visit](synthesis/battle10-outcomes-through-east-shrine-visit.md)
continues through separate shrine entry, mistaken identity, the grandfather's account and invitation,
with independent gates and current/rebuilt following kept distinct. Its conditional-control and
natural-route limits remain applicable.

[East Shrine return through Battle11 cave admission](synthesis/east-shrine-return-through-battle11-admission.md)
continues from optional shrine or direct Battle10 return through selected overworld requests, exact
cave admission, Peter's exchange with the hobgoblin and the before/start battle handoff. Its
interruption, cast, service and darkness-presentation limits remain applicable.

[Battle11 outcomes through Polca's first Oddler arrival](synthesis/battle11-outcomes-through-polca-arrival.md)
continues through separate cave/town admission, the injured boy and villagers' response, rebuilt
residents/following and conditional restored control. Its actor, service and natural-route limits
remain applicable.

[Polca's fairy interaction through Battle12 admission](synthesis/polca-fairy-through-battle12-admission.md)
continues through the optional turtle test, independent east exit, searching-devils confrontation
and ordered battle handoff. Its caller, actor, service and natural-route limits remain applicable.

[Battle12 outcomes through Bedoe's initial guard encounter](synthesis/battle12-outcomes-through-bedoe-entry.md)
continues through leader recognition, first/repeat mountain returns, Peter's response and the initial
guards' challenge, recognition and requests. Its outcome, setup, actor and service limits remain
applicable.

[Bedoe's king address and first audience](synthesis/bedoe-king-address-through-first-audience.md)
continues through independent scenes, dispersing guards, introductions, Zeon warnings and the
invitation to Volcanon. Its shared actors, following and conditional-control limits remain applicable.

The accepted [Bedoe to the Volcanon audience](synthesis/bedoe-to-volcanon-audience.md) explains
selected shrine entry, Peter's introduction, Volcanon's refusal, jewel advice and Peter's commitment
through joining and first conditional Map11 control. Its attribution, capacity/AI/following and
service limits remain applicable.

[Volcanon return through the Bedoe king choice](synthesis/volcanon-return-through-bedoe-king-choice.md)
explains the optional question, refusal/repeat, Creed/river/possible-raft advice and Luke's requested
departure. Its prompt, actor, hiding/re-entry and conditional-control limits remain applicable.

[Bedoe's hatchling flight and rescue](synthesis/bedoe-hatchling-flight-and-rescue.md) explains
separate optional demonstration/repeat and fall/rescue content through its two caller/control
boundaries. Its shared actors, independent predicates and natural-route limits remain applicable.

[Bedoe return through Battle13 admission](synthesis/bedoe-return-through-battle13-admission.md)
explains selected exits, Polca's confrontation, Gerhalt joining and the new-battle start handoff.
Its independent setup, membership, request and natural-route limits remain applicable.

[Battle13 outcomes through Polca's raft grant](synthesis/battle13-outcomes-through-polca-raft-grant.md)
explains Luke accompanying, fresh Polca return and the optional bargain through ordered raft state
and conditional control. Its outcome, actor, service and natural-route limits remain applicable.

[Polca's Oddler choices and following](synthesis/polca-oddler-choice-and-following.md) explains the
independent requests, refusal/convergence and current/future following through its caller/control
boundaries. Its input, actor, service and natural-route limits remain applicable.

[Polca departure and Luke recruitment](synthesis/polca-departure-through-luke-recruitment.md)
explains the selected south request, independent init, Luke volunteering/joining and current hide
through the later unlock and conditional exploration control. Its actor, cache, service and
natural-route limits remain applicable.

[Polca raft boarding and South Parmecia entry](synthesis/polca-raft-boarding-through-south-parmecia-entry.md)
explains separate player boarding, raft control and the selected southern Map72 exits into
South Parmecia69. Its physical-alignment, list-wait, entry-lifecycle and natural-travel limits remain
applicable.

[South Parmecia, the Kraken reveal and Battle16 entry](synthesis/south-parmecia-through-kraken-battle16-entry.md)
explains selected battlefield admission, the complete reveal and fresh combatant rebuilding through
the start wrapper. Its scene identity, service and natural-route limits remain applicable.

[Battle16 outcomes, disembark and conditional Hassan entry](synthesis/battle16-outcomes-through-hassan-entry.md)
explains first/repeat returns, separate landing and flag-selected town population/init. Its tuple,
last-listed-actor wait, transport and usable-control limits remain applicable.

[Hassan ancestry and Rohde's Caravan choice](synthesis/hassan-ancestry-and-rohde-caravan-choice.md)
explains the independent priest and Rohde paths, complete initial/repeat choices and conditional
following/control. Its lore, item/menu, actor and service boundaries remain applicable.

[Ribble's tree opening and the Achilles Sword](synthesis/ribble-tree-opening-and-achilles-sword.md)
explains the selected Panel/tree/Sword puzzle, capacity and conditional custody/return. Its item-use,
admission and natural-path limits remain applicable; acquisition remains a separate Hassan
cross-reference rather than a mandatory chronological prerequisite.

[Hassan departure, Battle17 entry and conditional returns](synthesis/hassan-departure-and-battle17-outcomes.md)
explains ordered admission, walking/raft requests, first/repeat outcomes and conditional returns.
Its mutable request, party/transport and natural-route limits remain applicable.

[Southwest Parmecia, Taros's warning and conditional Battle18 entry](synthesis/southwest-parmecia-through-taros-battle18-entry.md)
explains the selected western transfer, warning/retreat and complete confrontation through the
pre-activation start. Its independent flags, scene/neutral identities and natural-route limits remain.

[Battle18 outcomes and Caravan acquisition](synthesis/battle18-outcomes-and-caravan-acquisition.md)
explains first/repeat outcomes, the complete driver/miniaturization scene and conditional returns.
[Optional Hassan Rick recruitment](synthesis/hassan-rick-recruitment.md) explains separate population,
membership, temporary suppression and current following; neither guide supplies a mandatory travel route.

[Caravan boarding and Southwest Parmecia travel](synthesis/caravan-boarding-and-southwest-parmecia-travel.md)
explains selected boarding, mode1 movement/landing and separate walking Battle19 admission.
Its admission stop and actor, request, service and natural-route limits remain applicable.

[Battle19 outcomes and independent Elven Village entry](synthesis/battle19-outcomes-through-elven-village-entry.md)
explains shared outcomes and independent arrival. Its entry/control stop, supplied population/mode,
savepoint and natural-route limits remain applicable.

[Elven Village conversations and local exploration](synthesis/elven-village-conversations-and-local-exploration.md)
explains optional dialogue, search and internal stairs. Its local stop, inventory/actor/service and
natural-access limits remain applicable.

[Elven Village departure through Harpy Pond entry](synthesis/elven-village-departure-through-harpy-pond-entry.md)
explains selected exits, independent Devil's Tail scroll, call-for-help and the initial pond prompt.
Its conditional-control/admission stop and actor, mode, service and natural-route limits remain applicable.

[Harpy Pond approach through Battle20 entry](synthesis/harpy-pond-approach-through-battle20-entry.md)
explains the selected confrontation, independent warning and fresh/saved admission. Its pre-activation/
individual-turn stops and flag/request/cast/service limits remain applicable.

[Battle20 outcomes and Elric recruitment](synthesis/battle20-outcomes-and-elric-recruitment.md)
explains first/repeat outcomes, force membership, temporary following and immediate conditional pond/
savepoint return. Its stop and supplied-state/service limits remain applicable.

[Fairy Cave exploration and Devil's Tail return](synthesis/fairy-cave-exploration-and-devils-tail-return.md)
explains four chest rewards, both returns and conditional rock inspection. Its local interaction/
control/request or Battle21-admission stop and supplied-state/service limits remain applicable.

[Battle21 opening and initial roster](synthesis/battle21-opening-and-initial-roster.md) explains the
complete confrontation and conditional fresh/saved setup. Its pre-activation/individual-turn stop,
independent flags, crop/cast/roster/item and service limits remain applicable.

[Battle21 outcomes and Devil's Tail return](synthesis/battle21-outcomes-and-devils-tail-return.md)
explains leader cleanup, the complete first after scene and conditional full-map return. Its
Map67 control/pending or supplied loss-continuation stop and cast/service limits remain applicable.

[Creed Mansion entry and the first Goliath obstruction](synthesis/creed-mansion-entry-and-first-goliath-scene.md)
explains separate arrival and the complete first scene through conditional return/repeat. Its
pre-shrinking stop and supplied-state, service, following and live-layout limits remain applicable.

[Creed's magic door and the Desktop transfer](synthesis/creed-magic-door-and-desktop-transfer.md)
explains the independent warning/shrinking/collection scene and conditional request/admission. Its
pre-population/pre-opening stop and supplied-state, sprite, live-copy and pending limits remain.

[Desktop conversations and the Chessboard challenge](synthesis/desktop-kingdom-interactions-through-battle22-entry.md)
explains accepted conversations/services, Ponpei's permission and conditional BattleLoop admission.
Its [#907/PR909 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/909#issuecomment-6174944499)
retains the independent permission/request gates and pre-opening stop.

[Chessboard initialization and initial roster](synthesis/battle22-initialization-and-roster.md)
explains accepted empty hooks, party placement, sixteen enemies and fresh/current-state loading.
Its pre-activation stop and [#911/PR913 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/913#issuecomment-6175260041)
remain intact.

[Chessboard outcomes and Desktop return](synthesis/battle22-outcomes-and-desktop-return.md)
explains accepted outcome/counting, the rebuilt population, announcement and changed callbacks.
Its local-control/pending stop and [#915/PR917 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/917#issuecomment-6175632640)
retain the acquisition and service limits.

[Cotton Balloon acquisition and Floor World departure](synthesis/cotton-balloon-through-floor-world-entry.md)
explains accepted chest delivery, guard clearance, own-tile use and complete departure through
conditional destination admission. Its pre-population stop and
[#919/PR921 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/921#issuecomment-6176035685)
retain inventory, actor/action, request and natural-route limits. The
[corrected per-service ownership](../research/map-event-item-transactions.md#ownership-and-unknown-boundary)
retains #923's caller/callee distinction; accepted #916 owns the selected service algorithms.

[Floor World first arrival and conditional exploration](synthesis/floor-world-first-arrival.md)
explains the fresh population, complete landing/pop and conditional control boundary.
Its [#926 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/926#issuecomment-6176550578)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/929#issuecomment-6176569261)
retain CI38623499458 and the linked arrival/service/departure evidence and failure records.

[Floor World residents, the giant ring and outbound exits](synthesis/floor-world-interactions-and-exits.md)
explains accepted local callbacks, the independent ring attempt and conditional outbound admission.
Its [#930 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/930#issuecomment-6176889129)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/933#issuecomment-6176904018)
retain CI38626011000 and linked #924/#926/#923 corrections and limits.

[Willard Burrow exploration and Battle23 admission](synthesis/willard-burrow-through-battle23-entry.md)
explains accepted exploration, traversal and confrontation through the conditional BattleLoop call.
Its [#934 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/934#issuecomment-6177126852)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/936#issuecomment-6177145045)
retain its actor/alias, coordinate, wait and admission limits.

[Battle23 initial formation and suspended entry](synthesis/battle23-initialization-and-roster.md)
explains accepted fresh/saved setup through its pre-activation/pre-turn stops.
Its [#938 rebase handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/938#issuecomment-6177400955)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/940#issuecomment-6177419250)
retain original/rebase results and predecessor diagnostics.

[Battle23 outcomes and the return to Willard Burrow](synthesis/battle23-outcomes-and-burrow-return.md)
explains accepted leader/count outcomes, current-member victory seed, distinct defeat/escape returns,
fresh/retained burrow state and conditional next-map admission. Its
[#942 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/942#issuecomment-6177635072)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/943#issuecomment-6177652147)
retain exact checks, absent-input limits and linked predecessor failures.

[Creed's restoration and main-room departure](synthesis/creed-restoration-and-basement-departure.md)
explains the accepted restoration/Zeon conversation and conditional main-room handoff.
Its sprite/action, alias/HP, following, Egress-selector and control limits remain applicable.

[Evil Spirit Shrine entry and consultation admission](synthesis/evil-spirit-shrine-entry-and-scene-admission.md)
explains the accepted mansion/shrine route, fresh/retained population and independent
request gate. Its coordinate/layout, actor/allocation, service and natural-route
limits remain applicable.

[Evil Spirit consultation and conditional zone return](synthesis/evil-spirit-consultation-and-zone-return.md)
explains the complete accepted consultation, Zeon interruption and conditional
caller/wrapper return. Its dialogue/transaction, actor/action, dormant hiding,
presentation-service and non-atomic flag/control limits remain applicable.

[Creed return, Fairy following and recruitment choices](synthesis/creed-return-fairy-and-recruitment-choices.md)
explains accepted room interactions, current following and joined/active choices.
Its [retained review](synthesis/creed-return-fairy-and-recruitment-choices.md#evidence-review-and-retained-results)
keeps the accepted predecessor results, failures and Unknowns.

[Oddler stays and the Fairy handoff](synthesis/oddler-stays-and-fairy-handoff.md)
explains the accepted farewell, current actor/following changes, ordered caller
flags and separate fresh-entry populations. Its retained review preserves the
corrected Fairy declaration `(17,10)` Up/eas_InitSlow and both original and
corrected #962 review/CI receipts.

[Creed departure, dwarf recovery and the Cannon gift](synthesis/creed-departure-dwarf-recovery-and-cannon-gift.md)
explains the selected mine route, Fairy's recovery staging, capacity-dependent
Cannon offer and digging through conditional caller returns. Live/fresh layout,
actual actors, repeated gifts and service/control/save limits remain with that owner.

[New Granseal return and Astral following](synthesis/new-granseal-return-and-astral-following.md)
explains the accepted retrospective audience, adviser announcement and distinct
current/fresh following. Its [retained review](synthesis/new-granseal-return-and-astral-following.md#evidence-review-and-retained-results)
preserves predecessor results, corrections, failures and Unknowns.

[New Granseal inventor and Dynamite gift](synthesis/new-granseal-inventor-and-dynamite-gift.md)
explains the accepted inventor conversation, independent permission/item/capacity
checks and ordered gift/local return. Its retained review preserves predecessor
results, corrected inspections, failures and Unknowns.

[New Granseal Janet recruitment and departure](synthesis/new-granseal-janet-recruitment-and-departure.md)
explains accepted scripted acceptance, joined/active membership and temporary
following through local return/fresh-town consequences. Its
[retained review](synthesis/new-granseal-janet-recruitment-and-departure.md#evidence-review-and-retained-results)
preserves predecessor comparisons, corrected inspections, failures and Unknowns.

[New Granseal departure through Battle24 admission](synthesis/new-granseal-departure-through-battle24-admission.md)
explains accepted retained travel, separate request/ordered selection and fresh
formation through the pre-activation boundary. Its retained review preserves
#977's Map68 correction, comparison/CI failures and linked predecessor Unknowns.

[Battle24 outcomes through North Cliff preparation](synthesis/battle24-outcomes-through-north-cliff-preparation.md)
explains actual battle counting, distinct outcome returns and the current-item
warning through local retry/control. Its
[retained review](synthesis/battle24-outcomes-through-north-cliff-preparation.md#evidence-review-and-retained-results)
preserves #981/#982 comparison and CI receipts, #977's Map68 correction and
linked predecessor failures/Unknowns. Actual removal/blast/Rohde and the
separate Battle25 entrance continue in the next two guides; combat/outcomes and later crossing remain separate scope.

[North Cliff opening and Rohde recruitment](synthesis/north-cliff-opening-and-rohde-recruitment.md)
explains actual item removal, complete blast/join staging and local/fresh-map
consequences through the identified next warp. Its
[retained review](synthesis/north-cliff-opening-and-rohde-recruitment.md#evidence-review-and-retained-results)
and #990 handoff preserve #985/#986 and linked #981/#982/#977 findings,
completed failures and Unknowns; content/readability govern the prose.

[North Cliff entry through Battle25 admission](synthesis/north-cliff-entry-through-battle25-admission.md)
explains the separate entrance, ordered admission, complete initial encounter
and crop/view limits before activation. Its
[retained review](synthesis/north-cliff-entry-through-battle25-admission.md#evidence-review-and-retained-results)
and #994 handoff preserve #989/#990 evidence, corrections, CI and unavailable
source/pair boundaries. Actual combat and later travel are separate transitions.

[Battle25 outcomes through Ketto entry](synthesis/battle25-outcomes-through-ketto-entry.md)
explains actual outcomes, separate cave departure/ordered admission and the
initial town population. Its
[retained review](synthesis/battle25-outcomes-through-ketto-entry.md#evidence-review-and-retained-results)
and #998 handoff preserve #993/#994 objects, comparisons, corrections, CI,
completed failures and unavailable source/pair boundaries.

[Ketto interactions and merchant relocation](synthesis/ketto-interactions-and-merchant-relocation.md)
explains residents, merchant movement, services, inspections/items and conditional
departure. Its [retained review](synthesis/ketto-interactions-and-merchant-relocation.md#evidence-review-and-retained-results)
and #1002 handoff preserve accepted actor0 waiting, service/collection boundaries,
predecessor corrections, completed failures and unavailable source/pair limits.

[Battle26 admission and the Geshp confrontation](synthesis/battle26-admission-and-geshp-confrontation.md)
explains ordered admission, actual field actors and the complete confrontation
through before-wrapper return. Its
[retained review](synthesis/battle26-admission-and-geshp-confrontation.md#evidence-review-and-retained-results)
and #1006 handoff preserve #1001/#1002 objects, pair/CI, complete scene/literal
joins, corrections, completed failures and unavailable source/pair limits.

[Battle26 initial formation and neutral Higins](synthesis/battle26-initialization-and-roster.md)
explains before-return through fresh formation, conditional stats/upgrades, neutral
loading and empty start, stopping pre-ActivateEnemies. Its
[retained review](synthesis/battle26-initialization-and-roster.md#evidence-review-and-retained-results)
and #1010 handoff preserve #1005/#1006 objects, source/pair/CI, accepted-table joins,
corrections, completed diagnostics/failures and unavailable source/pair limits.

[Battle26 activation and the first turn handoff](synthesis/battle26-activation-and-first-turn-dispatch.md)
explains accepted region/activation, conditional deals alias, fresh no-spawn and
randomized records through the first ExecuteIndividualTurn call, before its body.
Its [retained review](synthesis/battle26-activation-and-first-turn-dispatch.md#evidence-review-and-retained-results)
and #1014 handoff preserve #1009/#1010 source/pair/CI, schema/table/link checks,
corrections, completed diagnostics/failures and unavailable source/pair limits.

[Battle26 outcomes and Higins recruitment](synthesis/battle26-outcomes-and-higins-recruitment.md)
explains terminal precedence, independent Black Ring transaction, complete
rescue/join/following and the returned handoff. Its
[retained review](synthesis/battle26-outcomes-and-higins-recruitment.md#evidence-review-and-retained-results)
preserves selected source/program/literal joins, public-schema/dialogue checks,
pair provenance, CI, source/pair limits and linked predecessor diagnostics.

[Battle26 return through conditional Pacalon entry](synthesis/battle26-return-through-pacalon-entry.md)
explains fresh return rebuilding, retained eastern scrolling and conditional
Map02 exploration/Battle31 admission through the first event/control read.
Its [retained review](synthesis/battle26-return-through-pacalon-entry.md#changed-states-and-retained-evidence)
preserves the accepted source/pair/optional-layout limits and the corrected
#1022 handoff with distinct original/rebased CI and completed diagnostics.

[Pacalon conversations, supplies and the castle handoff](synthesis/pacalon-interactions-and-castle-handoff.md)
explains both current resident alternatives, shops/priest, descriptions, possible
pickups and outgoing MainLoop requests. Its
[retained review](synthesis/pacalon-interactions-and-castle-handoff.md#changed-states-and-retained-review)
and corrected #1026 handoff preserve accepted public-schema/consumer checks,
pickup music/text chronology, Save service-loop versus post-save suspend,
distinct original/final CI, source/pair limits and linked predecessor failures.

[Pacalon castle: entry and the first drought-state visit](synthesis/pacalon-castle-entry-and-first-visit.md)
explains conditional construction, default cast/conversations, the full optional
centaur scene, examination/pickup/priest boundaries and retained CURRENT stairs.
Its [retained review](synthesis/pacalon-castle-entry-and-first-visit.md#changed-states-and-retained-review)
and #1030 handoff preserve accepted #1025/#1026 results, diagnostics and Unknowns.
Its south endpoint is the starting request for the next guide.

[Pacalon departure: the north scroll and conditional Battle27 admission](synthesis/pacalon-departure-to-battle27-admission.md)
explains fresh town/overworld construction, retained scrolling, candidate zone
writes, partial reload and ordered27/32 admission. Its
[retained review](synthesis/pacalon-departure-to-battle27-admission.md#changed-states-and-retained-evidence-review)
and #1034 handoff/acceptance preserve #1029/#1030 evidence, separate original/
final results and diagnostics, accepted0.083s provenance and natural-route limits.

The current reader outcome explains [Battle27: initial formation and the
restrictions confrontation](synthesis/battle27-initialization-and-opening.md),
scoped by [#1038](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1038).
**Standard** rigor remains appropriate: fresh/resume state, actual membership,
physical identity and intro completion have distinct boundaries. Consume the
complete accepted [#1033 research](../research/battle27-initialization-and-opening.md)
at85016e1c and the accepted #1034 predecessor. Activation research
and UI692 remain unconsumed/offlimits.

Acceptance requires a connected English explanation preserving
**Confirmed**, **Inferred** and **Unknown** boundaries:

- Connect incoming74/27 through the empty before hook, restored selectors,
  region reset, eligible healing, actual membership/formation and enemy load.
  Twelve placements prove no fixed named party; preserve dead/fewer/surplus/
  empty-member behavior and actual HP/class/status/item/difficulty readers.
- Explain all sixteen enemies/six definitions using the owning complete
  tables, equipment/spells/stats and initial order/activation/region/point
  declarations. Preserve no27 random upgrade/neutral group and exclusion of
  Battle32's Jaro exception; counts/names prove no tactics or activation.
- Separate combatants, physical actors, sprites and text speakers. Cover
  rebuilt mappings, position-versus-HP/capacity hazards and actual Map74
  crop/local coordinates/view/terrain/music without a decoded-route claim.
- Explain all nine opening commands and2658–2660 literals/meaning, actual
  Soldier133/Soldier132/Lizardman128 mappings, portrait/speech fallback,
  camera requests and real single-text/W1/skip/terminator consumers. Preserve
  existing portrait, current rather than guaranteed new input, unguarded
  missing-speaker lookup, aliases/dead speakers and477-before-completion.
  Questions create no player choice; no completed camera/rendering is claimed.
- Distinguish fresh pre-ActivateEnemies from F88's supplied-state continuation
  before individual-turn execution, including the possible turn sentinel.
  At this specific74/27 entry d0-preserving helpers carry74 outside the64-byte
  lookup; this proves no natural suspend/resume entry or register state.
- Preserve current-selector/crop/flag/interruption premises and unsupported
  activation/turn/AI/action/outcome/later story. Natural reach, services,
  presentation/control/persistence stay Unknown; no frontier advances.

Own exactly five Markdown paths: new guide, departure predecessor continuation
only, Story Progression selected continuation, this current agreement/
navigation and docs/README.md bounded navigation. No research/contracts/
schema/fixture/tooling/remake/glossary/Chinese writes, runtime, source copy/
acquisition/fresh private comparison, environment changes, generic combat/
service/platform expansion or new maintained verification machinery.
Content/readability governs length; contradictions return to research.

Minimum evidence is complete adversarial accepted Layer A/changed-state
review plus selected actual public fixture/schema/consumer joins and direct
documents. Compact summaries do not prove row contents or natural behavior.
The guide's [retained review](synthesis/battle27-initialization-and-opening.md#evidence-review-and-retained-results)
links #1033 final/acceptance (direct0.202s/independent0.195s, CI38675636173;
original0.207s/CI38675382781 and schema-path/local-$ref failures separate)
and #1034 handoff/acceptance (independent1.433s, CI38674976874; checker
diagnostics retained). Their linked predecessors preserve #1029 and earlier
results/failures/Unknowns.
Reuse accepted0.083s provenance only: source/pair remain unavailable in design
and optional layout limits remain explicit. No completed suite is reset.

Check complete prose/diff/changed states, exact five paths/navigation/links/
anchors/encoding/private boundary and git diff --check. Load retained
local/private-inputs.ps1 in the launching process for clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`; record exact-head
actual scoped CI. Documentation scope skips blanket normal/full/Python/
.NET/Godot/H3/H4/translation suites. Freeze a clean current-main-based
committed/pushed Draft PR fixing #1038, Project Review / Verify and successful
authorized main-gate completion callback. No merge, cleanup, archive or next Issue.

Chinese remains deferred until each selected English theme is independently accepted and stable.
[#638](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/638) remains user-paused.
[#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438) and
[#689](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689) own separate current UI/UX work;
[#617](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/617) retains its architecture boundary.
This segment selects no new art/UI/UX direction, support platform or save/full-game scope.

**Confirmed repository baseline:** existing contracts cover combat, maps, level-up, spells, services,
save/input/window, dialogue, party/roster state, and randomness. They are listed in the
[design index](../README.md#design) and trace back to research and fixture owners. This roadmap does
not merge or replace those contracts.

The following bounded explanations already exist. The ordering is an **Inferred reading priority**,
not a completion claim for their wider subjects. Extensions still require stable accepted evidence
owners and must preserve each document's Unknowns.

| Order | Existing document | Scope and prerequisites | Existing contract/evidence links | Non-goal / stop condition |
| ---: | --- | --- | --- | --- |
| 1 | [Gameplay Overview](synthesis/gameplay-overview.md) | Explain currently supported player actions, state boundaries, and major subsystem handoffs, beginning only from accepted map, dialogue, roster, service, and input facts. | [map exploration](contracts/map-exploration.md), [dialogue](contracts/dialogue-system.md), [party/roster](contracts/party-roster-state.md), [services](contracts/service-interactions.md), [map-script fixture](../../tests/fixtures/h2/map-script-engine-static-v1.json) | Do not promise a complete campaign flow, interface feel, or narrative experience; these remain **Unknown**. |
| 2 | [Tactical Battle Loop](synthesis/tactical-battle-loop.md) | Explain the bounded order from player input/control through battle action, resolution, state replay, and known outcomes while retaining every unresolved branch; requires accepted battle-loop/action/AI research and combat/spell contracts. | [battle-loop research](../research/battle-loop.md), [battle-actions research](../research/battle-actions.md), [combat](contracts/combat-resolution.md), [spell resolution](contracts/spell-resolution.md), [physical-damage fixture](../../tests/fixtures/h3/physical-damage-v1.json) | Do not invent tactics, balance intent, target-selection meaning, or a general simulation from isolated cases. |
| 3 | [Progression and Economy](synthesis/progression-and-economy.md) | Connect growth, EXP/gold/item, and service boundaries into a resource-flow explanation only where inputs, outputs, order, and persistence have evidence. | [ally-growth research](../research/ally-growth.md), [common-stats research](../research/common-stats.md), [level-up](contracts/level-up.md), [services](contracts/service-interactions.md), [level-up fixture](../../tests/fixtures/h3/level-up-boundaries-v1.json) | Do not claim an intended difficulty curve, intended prices, an optimal build, or a long-term economy. |
| 4 | [Story Progression](synthesis/story-progression.md) | Explain Confirmed state/route/dialogue/roster boundaries as a traceable progression map while retaining normal-story reachability and presentation labels; these least-stable dependencies place it after the preceding documents. | [gameflow research](../research/gameflow-core.md), [common-scripting research](../research/common-scripting.md), [dialogue](contracts/dialogue-system.md), [party/roster](contracts/party-roster-state.md), [dialogue runtime fixture](../../tests/fixtures/h3/map-script-dialogue-v1.json) | Do not reconstruct plot beats, player-choice consequences, or a complete story route from source labels or isolated program references. |

This order establishes reader navigation first, then covers the most bounded tactical loop, connected
resource flows, and finally story explanation that depends on reachability. An extension waits when
an active slice is revising its owner contract or when its required answers remain **Unknown**.

[Map Design Principles](synthesis/map-design-principles.md) also exists as a bounded structural
explanation; it does not establish player-route quality or authorial intent. The
[Map 3 to Battle 01 readiness ledger](synthesis/map3-battle01-readiness.md#accepted-current-milestone)
owns the **accepted current private Map 3 → Battle 01 victory → usable 5B return milestone**.
Acceptance composes existing executed continuous-route/return evidence and independently reviewed
source/consumer proofs under default keyboard A, the modern deterministic clock and the 7C/8D/10A
boundary. C is supplemental; gamepad B/D are excluded. The
[verification composition owner](../../remake/docs/evidence/retained-comparisons.md#accepted-composition-review)
retains exact dependencies and reproduction routes. This accepts gameplay and presentation semantics,
including actual consumers, completion and input readiness, without asserting hardware equality,
full-game parity or public-distribution rights.

Original observations retain their own [bounded frontier](synthesis/map3-battle01-readiness.md#accepted-original-frontier).
Historical A10/default/matrix reports remain Unavailable with `milestonePass=false`; scoped outputs
also remain false. No fresh full executable modern report or corrected whole-A trajectory was
produced. Historical seed-latch, HEAL timing, provenance and review failures remain preserved.
Terminal internal completion remains **Inferred**, its delay **Unknown**, and omitted historical
turn-generation operands and original timing retain their owner-defined limits. These scientific
limits do not reopen accepted compositions or create an automatic runtime queue; further evidence
work requires a concrete new defect.

The [Phase 4 bootstrap plan](synthesis/phase4-bootstrap-plan.md) remains a historical pre-start
proposal. Current engine direction and implemented/unsupported capabilities belong to
[ADR 0019](../decisions/0019-state-and-content-driven-remake-engine.md) and the
[remake capability ledger](../../remake/docs/capability-status.md); implementation progress is not
original-game evidence.

## Long-Term Directions

The following broader explanations or extensions remain **Unknown future directions**, not current
commitments. Work may begin only when entry criteria cite accepted local evidence; none authorizes a
new engine design or treats remake functionality as original-game evidence.

| Direction | Entry criteria and evidence dependencies | Non-goal |
| --- | --- | --- |
| experiential extension of map-design principles | The existing structural synthesis is the baseline; additional reachability and interaction-outcome observations must distinguish layout facts from player-route interpretation. | Do not infer authorial intent or redesign levels from 64x64 layout data alone. |
| player roster choice space | Accepted roster, class/promotion, growth, equipment, battle-party, and persistence/capacity boundaries; unresolved lifecycle limits remain visible. | Do not publish a tier list, “best party” advice, or assumed player preferences. |
| player/enemy numerical curves | Complete source-backed numeric tables plus runtime-confirmed application, caps, and level/encounter context sufficient to name units and boundaries. | Do not set remake balance targets or describe mathematical curves as intended difficulty. |
| battle simulation | Complete and mutually compatible battle-loop/action/AI/pathfinding/state contracts plus a bounded H4 adapter acceptance surface. | Do not select a simulation architecture, claim general predictive accuracy, or use a model to fill unresolved branches. |

## Reusable Authoring Structure

Future `docs/design/` synthesis documents may selectively use the following structure. This describes
document shape, not a parallel workspace or a mandatory full GDD template.

**Confirmed authoring guidance:** organize design documents by coherent subject matter, reader flow
and maintainable ownership. Include the explanation needed to make the bounded subject understandable;
do not impose line, word or page caps, split documents solely for length, or remove necessary
explanation to meet a brevity target. The handwritten-source line limit does not apply to design
Markdown. Effort budgets bound work expansion, not document length.

1. **Audience and judgment boundary.** Identify the reader—researcher, fidelity implementer, or
   player-facing explainer—and the supported and unsupported judgments. Original-game claims retain
   **Confirmed**, **Inferred**, or **Unknown** at the source-owner layer.
2. **Player verbs and action-goal alignment.** Begin with evidenced inputs, state changes, and
   outcomes. Keep original source labels separate from neutral player-action phrases. A player goal
   or meaning without local evidence is **Inferred** or **Unknown**.
3. **Loops, state flow, and system dynamics.** Diagram only ordered transitions, resources, and
   feedback relationships with evidence owners. Retain unobserved branches and do not present a
   control-flow graph as engine architecture.
4. **Evidence matrix.** Every substantive entry includes its label, bounded claim, source/research
   owner, contract, fixture ID/path when applicable, and remaining question. Local links such as
   [runtime RNG and battle math](../research/runtime-rng-and-battle-math.md),
   [combat fixture](../../tests/fixtures/h3/physical-damage-v1.json), and
   [combat contract](contracts/combat-resolution.md) are the canonical trace; do not copy another evidence
   ledger.
5. **Original fidelity and modernization.** State the original-fidelity rule first, then mark a
   deliberate deviation as a future decision with a separate expected-deviation fixture. In the
   absence of a decision, do not imply modernization.
6. **H4 acceptance, expansion, and stop conditions.** List adapter-visible parity facts, fixture
   consumers, and the evidence required for expansion. Stop when a gap is a runtime, reachability,
   presentation, or product question rather than silently expanding the contract.

## External Reference Provenance and Selective Adoption

**Confirmed external-reference provenance:** the
[DY-2026/GameDesignOS README](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/README.md),
[contract catalog](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/contracts/README.md), and
[player-promise contract schema](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/contracts/player-promise-contract.schema.json)
were accessed on 2026-08-01 at pinned `main` commit
`d01dfebc6eac7a619b9a18f3cbafa51270d1edba`; the repository uses the
[MIT license](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/LICENSE).
The reproduction command `git ls-remote https://github.com/DY-2026/GameDesignOS.git` observed that
commit at `refs/heads/main`, and requests for each listed pinned raw document/template returned HTTP
200.

The following structural prompts were selectively adopted:
[player-verb inventory](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/player-verb-inventory.md),
[system-dynamics map](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/system-dynamics-map.md),
[game-dissection report](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-experience-analyzer/templates/game-dissection-report.md),
[full design brief](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/full-design-brief.md), and
[reference-game boundary](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/reference-game-boundary.md):
reader/action scope, visible uncertainty, loop mapping, evidence links, scope gates, and validation
conditions. They were adapted to this repository's evidence labels and H4 boundary without copying
template text.

This project explicitly rejects the external project's nine-directory workspace, commercial
pitch/market assumptions, and second evidence/decision system. This repository already owns
`docs/research/`, `docs/design/`, `docs/decisions/`, `schemas/`, `manifests/research-index`, H2/H3
fixtures, and the H4 acceptance boundary. The external reference contributes only selective
authoring perspective; it is not a project dependency or a new source of truth.

## Collaboration and Continuing Hygiene

**Confirmed collaboration rule:** synthesis documents may be added over accepted evidence while
reverse engineering continues, but they must not rewrite a subsystem contract in parallel with its
active worker. When a future finding changes a conclusion, update the owning research note,
fixture/contract, and design explanation together so the trace remains bidirectional.

**Confirmed repository hygiene closure:** [`party-roster-state.md`](contracts/party-roster-state.md) is now
registered in `src/sf2tool/design_contracts.py` with its H2 map-script and H3 active-party fixtures.
The public tracked-input gate validates document path, fixture path, and fixture ID traceability in
both directions. This closure does not change any original-game finding.
