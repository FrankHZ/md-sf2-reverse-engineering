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

The current reader outcome explains New Granseal's inventor conversation and
capacity-dependent Dynamite gift through local interaction return and necessary
fresh-entry consequences. **Standard** rigor remains appropriate: permission
flags, actual Cannon possession, actor/speech context, receiving capacity and
ordered completion are distinct. The owner is
[New Granseal inventor and Dynamite gift](synthesis/new-granseal-inventor-and-dynamite-gift.md),
scoped by [#974](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/974).
Consume [accepted #969 research](../research/new-granseal-inventor-and-dynamite-gift.md)
and accepted predecessors on main `36b0026bca3f7aaa5dee2b1d366312534000c522`;
Janet research is a separate outcome and is not this guide's evidence dependency.

Acceptance requires a readable castle-to-town inventor story with unchanged
**Confirmed**, **Inferred** and **Unknown** boundaries:

- Explain the selected Map7 south return, current/fresh town setup/init and
  inventor141, distinct from castle139/current-follow and fresh B31. Reuse the
  accepted population owner without a full town catalog or natural-walk claim.
- Preserve F802 priority over old F702, F804 before F805, earlier warning,
  permission dialogue, actual Cannon search, nod/offer and receiving attempt.
  No F761/F70/actual-Astral or living/active-party prerequisite, yes/no prompt,
  Cannon consumption, cave opening or Dynamite uniqueness rule.
- Keep ascending joined-member search (reserve/dead included), count-limited
  descending slots, packed/gapped premise, masked entries/unmasked request and
  full-word sentinel distinct. Receiving counts all four slots and chooses the
  earliest joined member with room; AddItem finds the first empty slot. Empty
  joined-list behavior is not guaranteed.
- Separate unchecked AddItem result, receiving announcement and later helper0,
  caller bit0/F804, wrapper cleanup, visible receipt, usable control and save.
  Option0 permits no forced discard; all-full leaves F804 clear for retry.
- Preserve actual B nod/speech/portrait context, inherited inventor receiving
  context, recipient NAME/item substitution and two sequential held-input
  services without required release. The helper name is not a palette fade;
  direct txt differs from scene skip. Retain repeat/existing-item, changed
  actor/state and interrupted-prefix limits, with no invented repair/rollback.

Own exactly five paths: the new guide, Astral predecessor continuation link only,
Story Progression's connected segment, this agreement and docs/README.md navigation.
No research/contract/schema/fixture/tooling/remake/glossary or Chinese writes.
Stop at local inventor return and relevant fresh-entry consequences; Janet
recruitment, North Cave blast, travel/battle and remake choices remain outside.
Minimum evidence is accepted Layer A plus selected public fixture/schema/consumer
inspection and small document/scope checks. No acquisition, retained-pair
regeneration, runtime/capture, installation or new machinery. Content/readability
governs length. Material evidence contradiction requires replanning with its owner.

The guide's [retained review](synthesis/new-granseal-inventor-and-dynamite-gift.md#evidence-review-and-retained-results)
links #969's complete49/40 operation and14 text joins, distinct author/independent
unchanged-pair receipts and actual CI, plus #970's original/final candidates,
checks and independent review. Linked #965 Battle19/stride, #954 callback and
#962 Fairy corrections and predecessor failures/diagnostics/Unknowns remain
retained. The synthesis checkout lacks the pinned upstream/pair; no fresh
comparison claim, acquisition or completed-failure restart follows.

Check complete prose/diff, links/anchors/navigation, UTF-8/private boundary,
exact ownership and `git diff --check`. Load local/private-inputs.ps1 in the
same process for the clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`;
record actual exact-head CI in the Issue handoff. Documentation-only
policy calls for direct checks, without blanket Python/.NET/Godot/H3/H4 or
translation suites. Freeze a clean pushed Draft PR fixing #974, Project Review /
Verify and successfully notify authorized main-gate for independent acceptance.
No merge, cleanup, archive or next Issue.

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
