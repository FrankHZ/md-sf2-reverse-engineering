# Map 3 to Battle 01 Readiness Ledger

- Status: **NOT READY** for continuous-milestone acceptance; not a default blocker for separately authorized implementation.
- Current acceptance owners: [modern continuous comparison](../../../remake/docs/development-and-verification.md#current-keyboard-comparison-scope) and [capability status](../../../remake/docs/capability-status.md#current-engineering-frontier). Original evidence and completed historical failures retain the owners below.
- Milestone: [ADR 0009](../../decisions/0009-first-phase4-playable-slice.md); profile: [ADR 0010](../../decisions/0010-map3-battle01-product-acceptance.md).
- Start policy: [ADR 0016](../../decisions/0016-remake-start-evidence-deferral.md); engine direction: [ADR 0019](../../decisions/0019-state-and-content-driven-remake-engine.md).
- Definition owner: [Continuous Scenario Contract](../contracts/map3-battle01-continuous-scenario.md).

## Current acceptance scope

The selected profile is `1A + 2A + 3A + 4A + 5B + 6A + 7C + 8D + 9A + 10A`.
8D compares gameplay and presentation semantics, including actual host consumption, completion,
acknowledgement and input readiness. Pixel/frame/waveform/chip and VInt/DMA/CRAM/VDP equality are
outside scope; timing that changes gameplay, causal order or input availability remains required.
Private original content, natural continuity, 5B and separately reported applicable deviations remain
required. [Current keyboard scope](../../decisions/0010-map3-battle01-product-acceptance.md#current-keyboard-scope)
requires default keyboard A; C is supplemental and gamepad B/D are excluded. Existing remapping,
swapped buttons, reduced flash and adjustable text do not create additional full-route requirements.
The separate #517 fast-text speech choice remains required.

The accepted modern deterministic clock can change original whole-history RNG, actor order and
resources. Those histories remain diagnostic; matched-state rule/RNG effects, mandatory route,
manual agency, actual content use and consumer boundaries remain required. Do not infer a causal
explanation for a historical discrepancy merely from the clock decision.

This ledger accounts for evidence and acceptance; the linked contract owns exact comparison rules.
Original evidence, definition delivery/review and actual remake H4 PASS are three distinct states.
No unmerged Research or Remake result contributes to this ledger. The contract does not register
fixtures, schemas or index associations. Godot acceptance reads the running state/input/projection;
screenshots are prohibited. No normal/full/native suites are warranted by this documentation change.

## Accepted original frontier

**Confirmed bounded original observation:** the [final acquisition owner](../../research/map3-messenger-acceptance.md#native-victory-and-stable-field-readiness)
and [audit](../../research/map3-battle01-audit.md) establish controlled R1 through messenger, Map19,
royal/guard, castle/tower, natural Battle01 admission, reached actions and natural victory through
post-after-program stable field readiness. Accepted source is `9c3ea03ac5f5b467ee744f1ac624870da2408443`;
PR #504 at `5102804b` accepts the result. The selected fresh prepared-68..86 lineage verifies 18
parent pairs and uses only the successful winning requests. It is savestate-linked continuity,
not uninterrupted wall time or a natural New/load flow.

Natural first actor is **2**; R2d's seeded actor 1 remains an explicit controlled bridge. The final
chain reaches round **14**, turn **103**, victory frame **58657**, `abcs_battle01` return **60945**,
then shared-tail/enclosing return, F401 clear, F501 set, BattleLoop D4=1, SwitchMap, ExplorationLoop
and Map57 `ms_Void`. The **67** reached operation pairs are not the static corpus's 80 records.

Two completed neutral frames **61009/61010** confirm Map57, player `(5,12)`, DOWN, battle255,
F401=false/F501=true, roster `[0,1,2]`, gold **420**, settled motion/camera and no blocking
script/modal/transfer/battle/consumer. Exact ally HP/MP/level/items/spells/status and RNG readback are
in the [contract endpoint](../contracts/map3-battle01-continuous-scenario.md#exact-observed-endpoint)
and its original owner. Generic window byte 1 is not a blocking dialogue. Original input polls at
this PR #504 terminal are neutral; that terminal remains unchanged and nonresumable. Accepted PR
#526 separately reproduces the boundary, observes one ordinary Down read and its settled Map57
`(5,13)` displacement. This closes the bounded RA-12 input/effect observation, not full 5B or the
milestone. The PR #526 extension is itself terminal/nonresumable. The final segment's 106 audio
dispatch/mailbox pairs do not establish full 8D.

Earlier R1/R2/R2a/R2d fixtures and Map19/first-player-ready observations retain their original limited
projections. They are not silently expanded by the final chain. Exact post-447 wait entry remains
**Inferred**; the public R1 projection omits status and complete item slots. Use the winning lineage's
readback, not another attempt's zero status or R2d's seeded order. Historical failures and cleanup
Unknowns (including attempts 21/41/61/67) remain in the acquisition owner. Completed defeat and
transport/observer failures are not interrupted runs and are not erased by final success.

## Readiness Checklist

| Gate | Current result | Owner / exact remaining boundary |
| --- | --- | --- |
| Milestone, engine and product choices | PASS | ADR0008/0009/0010; no new product decision here |
| Controlled admission | PASS bounded | [Admission contract](../contracts/map3-controlled-admission.md); selected status/full item slots/NPC/RNG now have [offline bindings](../contracts/map3-battle01-continuous-scenario.md#offline-reference-bindings); full R1 flags and other omitted fields remain OPEN |
| Natural mandatory route and encounter admission | PASS bounded original evidence | Final acquisition; actual actor 2; not the R2d bridge |
| Winning actions and consumed results | PASS bounded original evidence | Selected winning chain; committed actions/seed/main-draw records are bound offline; full field-input normalization, individual draw-to-effect and cancel/reselect remain OPEN |
| Victory, after-program, flag and return spine | PASS bounded original evidence | 67 reached operation pairs and selected final chain; not a full presentation claim |
| Exact neutral settled endpoint | PASS bounded original evidence | Contract endpoint and original terminal; remaining full records must be consumed from private evidence |
| Bounded RA-12 ordinary input/effect | PASS bounded original evidence | PR #526: one Down read and settled displacement from Map57 `(5,12)` to `(5,13)`; separate terminal, not resumable and not part of the PR #504 projector binding |
| Full controllable 5B | Bounded actual return accepted; integration review OPEN | Retain the ordinary continuous winning/return and delivered field-control proof; the separate original RA-12 extension keeps its own lineage |
| Continuous contract and ten-layer definitions | Accepted definitions; missing bindings OPEN | Linked contract defines fields, sources, actual mappings and failure/unavailable rules; bounded offline bindings available; remaining field gaps and complete definition readiness OPEN |
| Original expected fields complete for every applicable assertion | Accepted bounded definitions; integration review OPEN | Use the accepted source and controlled compositions below. Historical missing fields stay explicit and are not automatically current acquisition requirements |
| Save policy 6A | SELECTED; continuous H4 execution OPEN | Absent user persistence surfaces; restart to admitted state |
| 7C content/provenance | Local resource proofs accepted; integration review OPEN | Reuse reached visual/audio provenance and mutable-map delivery at their declared scope. Private content remains untracked; authored JoinCue chords or mute cannot substitute for required original audio |
| 8D semantic presentation | Local consumer obligations accepted; integration review OPEN | Accepted text, motion, operation, audio and scene bindings retain their declared composition limits; overall parent coverage is pending review |
| Existing settings and bounded direct observations | PASS bounded implementation; supplemental/history | [Settings owner](../../../remake/docs/development-and-verification.md#native-9a-observation); implementation does not authorize additional required variants |
| Current keyboard scope / 10A deviations | Scope comparison PASS; full H4 OPEN | Required A only; C diagnostic, B/D excluded. A self-equality closes only the declared scope row, not remaining obligations or arbitrary settings equivalence |
| Required reached action support | PASS bounded implementation; continuous comparison OPEN | PR #521 (`78c201c3`) accepts ordinary Medical Herb selection/live inventory and host inventories/itemSlot observations; compare the winning original actions separately |
| Actual continuous comparison | Accepted local compositions; closure audit pending | The thirteen local obligations below have independent acceptance. Retained A10/matrix results remain unchanged; they are not a current combined PASS |
| All applicable H4 layers executed successfully | OPEN integration acceptance | Account for all required parent/child assertions, shared evidence identities and deviations at the accepted composed boundary; individual scoped PASS reports are insufficient |
| Independent milestone readiness acceptance | OPEN | Main-gate; neither Issue closure nor a bounded implementation PASS is sufficient |
| Separate implementation-start authorization | PASS | User authorization in [Remake README](../../../remake/README.md); does not accept this milestone |
| Public distribution | BLOCKED OUTSIDE PRIVATE MILESTONE | Separate rights/licensed replacement decision; private assets remain untracked |

## H4 Composition Rules

The [contract's ten layers](../contracts/map3-battle01-continuous-scenario.md#ten-h4-comparison-layers)
are: (1) admission/provenance, (2) logical input/route, (3) world/story transitions,
(4) encounter admission, (5) turn/action/RNG/consumed effects, (6) victory/after-program/return,
(7) exact endpoint and ordinary input effect, (8) save exclusion/private assets,
(9) presentation identity/order/consumption/completion/ack/readiness, (10) explicit deviations.

Each assertion retains expected source and actual observation separately. Missing original fields or
private inputs yield Unavailable with the missing side/field, never PASS, zero-filled expectations or
implicit exclusion. Mismatches and observed unsupported required actions are FAIL. Unknown original
fields leave definition readiness OPEN. Whole-run PASS requires every assertion and deviation in the
explicit current scope. Supplemental or excluded reports cannot change required totals. Production legality must depend on state/content, not a fixed actor sequence,
round count or reference receipt history. Existing subsystem fixtures remain their own owners.

### Initial deviation inventory mapped to H4

The contract maps all ADR0010 deviations: controlled construction (1A), optional-route exclusion
(2A), fixed admitted seed and manual play with the accepted modern deterministic clock (4A), no save
(6A), current keyboard scope and separately reported settings (9A), and explicitly identified
out-of-domain safe behavior (10A). Every result is
visible separately in layer 10, including passes. 7C private handling is a product boundary, not a
fidelity waiver. Missing evidence/content cannot be recategorized as a deviation.

## Dependency and completion boundaries

| Owner | Work remaining / dependency |
| --- | --- |
| Research | PR #526 independently accepts the bounded RA-12 input/effect. Other selected original fields, input normalization, RNG and presentation gaps remain as defined by their owners |
| Design | Keep current applicability and the keyboard acceptance boundary aligned with the contract; original PR #526 extension remains separate from the PR #504 projector |
| Remake/content | Reuse accepted audio/private resources, mutable-map delivery and scene consumer proofs at their named dependencies. #517 Option A retains its controlled reveal-tail mechanism and historical C limit |
| H4 executor | Reconcile the accepted local obligations with required parent coverage and shared evidence identities; preserve failures and Unavailable. Any additional integration check needs a bounded plan |
| Main-gate | Independently accept definitions, evidence closures and eventual complete H4 result; serialize integration |

Accepted [outcome implementation](../../../remake/docs/exploration-programs.md#battle01-outcome-after-program-and-return)
and R4a comparison prove their bounded common-session/static-spine behavior, not natural original
expected values or this H4 run. The [capability ledger](../../../remake/docs/capability-status.md)
retains other unsupported consumers. Do not expand scope to EGRESS or unrelated item/menu branches
unless the selected route requires them. Existing remapped/swapped input and paired flash/text
observations remain bounded evidence. Their missing device/export coverage does not expand the
current keyboard milestone. No new capture follows from this ledger.

### Current required comparison boundary

**Confirmed:** the thirteen local obligations previously absent from retained A10 now have
independently accepted source/consumer compositions. Their owning contracts define current scope:

| Required child | Accepted owner and boundary |
| --- | --- |
| Admission seed-copy byte | [Seed composition](../contracts/map3-battle01-continuous-scenario.md#admission-seed-copy-composition): non-resume write-before-read plus current copy mechanism |
| Opening mouth/view controls | [Controlled opening](../contracts/map3-battle01-continuous-scenario.md#controlled-opening-control-binding): selected original callback/receipt lineage |
| Turn score/order | [Turn composition](../contracts/map3-battle01-continuous-scenario.md#composed-current-turn-rule-and-queue-consumption): genuine generation and complete retained queue consumption |
| Physical effects | [Physical rules](../contracts/map3-battle01-continuous-scenario.md#selected-physical-rule-and-consumer-binding): reached source operands and actual effect ownership |
| HEAL cost/recovery/fairy | [HEAL](../contracts/map3-battle01-continuous-scenario.md#selected-heal-rule-and-consumer-binding): selected logical opportunities and actual consumers |
| Rewards/after-turn/outcome | [Rewards](../contracts/map3-battle01-continuous-scenario.md#selected-reward-growth-and-outcome-consumer-binding): source rewards, growth, return effects |
| AI decisions/memory/movement | [AI](../contracts/map3-battle01-continuous-scenario.md#selected-ai-rule-and-consumer-binding): source decisions with accepted seed mechanism |
| Field service draw/effect gates | [Field services](../contracts/map3-battle01-continuous-scenario.md#selected-field-service-rule-and-consumer-binding): bounded source and actual service cases |
| Mutable map resources | [Map delivery](../contracts/map3-battle01-continuous-scenario.md#composed-mutable-map-delivery): ordinary-input lineage, working layout and actual draw |
| W1 token/read/service | [W1](../contracts/map3-battle01-continuous-scenario.md#selected-w1-consumer-binding): occurrence, service and input ownership |
| W2 accepting read/token return | [W2](../contracts/map3-battle01-continuous-scenario.md#composed-w2-consumer-binding): selected validation and source continuation |
| Scene command/resource/wait/effect/end | [Scenes](../contracts/map3-battle01-continuous-scenario.md#selected-battle-scene-command-and-consumer-binding): visible consumers, completion ownership and bounded terminal composition |
| Audio dependent consumer edges | [Audio](../contracts/map3-battle01-continuous-scenario.md#composed-reached-audio-consumer-binding): reached playback/replacement/fade/stop/resume and release |

Reuse the accepted continuous winning/return route and its actual field control, plus the
[displayed text](../contracts/map3-battle01-continuous-scenario.md#complete-reached-displayed-text-material-binding),
[motion](../contracts/map3-battle01-continuous-scenario.md#complete-reached-field-motion-and-consumer-binding),
[operation](../contracts/map3-battle01-continuous-scenario.md#complete-reached-operation-flow-binding)
and [resource cohort](../../../remake/docs/development-and-verification.md#caller-and-reached-visual-resource-cohort)
proofs. The separate [Option A speech policy](../../../remake/docs/presentation-and-assets.md#accepted-fast-text-speech-policy)
uses its accepted controlled reveal-tail evidence; historical C's unsampled interval remains Unknown.

**OPEN integration boundary:** the scoped reports retain `milestonePass=false`. Retained A10 and
its current-keyboard matrix still describe their original Unavailable children. No combined report
has been accepted by substituting thirteen PASS labels. Overall acceptance must account for the
required parents/children and shared source, producer, session, occurrence and input/effect joins,
including the accepted controlled compositions and separately reported deviations. A missing
integration check needs one minimum scope/budget/dependency plan; it does not imply another route,
full H4, matrix or native run. Main-gate owns that decision and final readiness acceptance.

Historical seed-latch, HEAL timing, provenance and review failures remain preserved. Terminal
internal completion remains **Inferred** and its delay **Unknown**; historical missing turn-generation
operands and corrected whole-A history remain explicit. These limits do not reopen accepted
compositions. C remains supplemental and B/D excluded under the current keyboard scope.

### Conditional runtime questions

Only a named missing original semantic assertion can justify separately admitted observation under
ADRs0014/0016 and [ADR0015](../../decisions/0015-original-reference-replay-and-h4-boundary.md).
Current questions are unresolved selected input/RNG/state fields and required 8D consumption/ack
boundaries. The bounded RA-12 Down/effect is accepted in PR #526. Reuse accepted static rules and bounded observations first. The
[Research presentation audit](../../research/map3-battle01-audit.md#presentation-sufficiency-under-8d)
records DisplayText bypass; a program return does not prove unshimmed delivery. Persistent music
requires start/replacement/stop where reached, not a fictitious end event. This ledger authorizes no
native launch and no automatic per-Unknown observation queue.

### First actual H4 comparison and correction (PR #528 / #533)

This subsection is historical. It preserves the completed pre-modern-clock comparison and does not
describe the current required gate or authorize the former four-variant scope.

**Confirmed comparison results:** PR #528's completed baseline reported **5,336 PASS / 6 FAIL / 40
Unavailable**; its six failures and report remain historical evidence. PR #533 corrected the
connected selected R1 product inputs to source NewGame gold 60 and complete starting item words,
while preserving the separate controlled R1 fixture's gold 0/four-byte projection. The corrected
actual host run completed the normalized field route, all 73 reached text IDs and natural first actor
2, then stopped at the diagnostic next-actor divergence. It reports **5,340 PASS / 2 FAIL / 40
Unavailable**, `milestonePass=false`. Admission gold is 60. At natural first control, each ally's live
item array matches the starting slots supported by pinned NewGame source: Bowie `[199,0,127,127]`,
Sarah `[213,0,0,127]`, Chester `[184,0,127,127]`. These later inventories do not establish admission
`SourceLoadout`, which remains null. First-round order remains different; after the diagnostic first STAY, original
next actor is Bowie and actual is Sarah. The host's exit 2 is corroborated by that comparison.
The PR #526 post-victory extension was not rebound or reached. NPC phase and timing/RNG mapping remain Unknown. This completed
comparison was not H4 acceptance. Current Battle01/return/endpoint and keyboard obligations are
assessed under the applicability and scope owners above, without erasing these failures.

### Original replay lineage and launch admission

The old replay path remains **DISABLED / NOT REQUIRED** for this milestone. Its
[lineage hard stop](../../research/original-reference-replay-capability.md#current-lineage-hard-stop)
retains consumed diagnostic identities, ordinal-2 completed timeout FAIL, cleanup failure, missing
actual ledger/receipts and ordinal-3/retry/reset launch prohibition. Recovery/transport/hardware APIs
are not H4 prerequisites. No synthetic reconstruction, task rename or new runner resets that history.
Current segmented-acquisition authority is governed by ADR0015, not superseded cumulative ceilings;
this documentation neither spends nor renews runtime authority.

## Evidence Matrix

| Existing owner | What it continues to own |
| --- | --- |
| [Admission](../contracts/map3-controlled-admission.md), [Research audit](../../research/map3-battle01-audit.md#accepted-evidence-and-exact-field-mapping) | R1/R2/R2a/R2b/R2c/R2d/R3a–d/R4a fixture identities, exact fields and bounded/static distinctions |
| [Battle functions](../contracts/battle-functions-control-flow.md), [camera](../contracts/map-camera-update-control-flow.md) | Existing 15 direct battle-function associations and separate camera record; continuous work adds no index association |
| [Battle lifecycle](../contracts/battle-control-lifecycle.md), [cutscene routing](../contracts/battle-cutscene-routing.md) | Local victory/control/program rules; natural chronology belongs to accepted observations |
| [Acquisition](../../research/map3-messenger-acceptance.md) | Exact source/ROM/tool/parent identities, selected winning records, independent review, reproduction, prior failures and costs |
| [Continuous contract](../contracts/map3-battle01-continuous-scenario.md) | Composition and comparison semantics; missing originals stay explicit |

Map3 aggregate inventory is not a scenario association set; do not bulk-associate its 26 rows.
Research evidence is not rewritten from remake output. Retired M5 consumers/aggregate tests are not
restored: preserve the completed #431 `PrivateExplorationTests.PrivatePresentationBytesAndRequiredSpriteLinksAreAdmittedBeforeStartup` failure and separate complete-private-world
rerun with their [verification owner](../../../remake/docs/development-and-verification.md).

## Public and Private Boundary

ROMs, saves, traces, captures, complete text/graphics/audio and generated exports remain private,
ignored and local; do not publish their absolute paths or require them in public CI. Public reporting
contains only licensed/minimal semantic facts and safe provenance/results. The full private inventory
and resource bindings must be checked locally before 7C can pass. Documentation-only acceptance uses
scope/link/bilingual semantic checks; no original or remake output is generated by this slice.
