# Map 3 to Battle 01 Readiness Ledger

- Status: **NOT READY** for continuous-milestone acceptance; not a default blocker for separately authorized implementation.
- Accepted evidence baseline: `58c5a94a4c5349fd221a130a0970222962715528`, including [PR #504](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/504).
- Milestone: [ADR 0009](../../decisions/0009-first-phase4-playable-slice.md); profile: [ADR 0010](../../decisions/0010-map3-battle01-product-acceptance.md).
- Start policy: [ADR 0016](../../decisions/0016-remake-start-evidence-deferral.md); engine direction: [ADR 0019](../../decisions/0019-state-and-content-driven-remake-engine.md).
- Definition owner: [Continuous Scenario Contract](../contracts/map3-battle01-continuous-scenario.md).

## Current acceptance scope

The selected profile is `1A + 2A + 3A + 4A + 5B + 6A + 7C + 8D + 9A + 10A`.
8D compares gameplay and presentation semantics, including actual host consumption, completion,
acknowledgement and input readiness. Pixel/frame/waveform/chip and VInt/DMA/CRAM/VDP equality are
outside scope; timing that changes gameplay, causal order or input availability remains required.
Private original content, natural continuity, 5B and separately reported 9A/10A remain required.

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
and its original owner. Generic window byte 1 is not a blocking dialogue. Original input polls are
observed but neutral: **Unknown / OPEN RA-12** is the next nonneutral input's acceptance and state
effect. The evidence terminal named `controllable-5b` does not establish full RA-12 or milestone PASS;
it is nonresumable. The final segment's 106 audio dispatch/mailbox pairs do not establish full 8D.

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
| Full controllable 5B / RA-12 | OPEN | Research: next nonneutral input and actual effect; no remake-derived expected truth |
| Continuous contract and ten-layer definitions | Accepted definitions; missing bindings OPEN | Linked contract defines fields, sources, actual mappings and failure/unavailable rules; bounded offline bindings available; remaining field gaps and complete definition readiness OPEN |
| Original expected fields complete for every assertion | OPEN | Missing full R1 fields, field-input normalization, thinking RNG/individual draw effects, cancel and required 8D consumption/ack evidence; not a new native authorization |
| Save policy 6A | SELECTED; continuous H4 execution OPEN | Absent user persistence surfaces; restart to admitted state |
| 7C content/provenance | OPEN | Complete reached original scene inventory, especially original audio; authored JoinCue chords or mute cannot pass. Modern HUD/theme/input glyphs/fonts follow accepted authorship/license and 9A, not a ROM-original-font requirement |
| 8D semantic presentation | OPEN | Required identity/order, real host use, completion/ack and readiness; source dispatch pairs alone insufficient. BattlePresentation board markers/status/roster do not implement battle-scene consumers |
| 9A configuration and bounded direct observations | PASS bounded implementation | [9A owner](../../../remake/docs/development-and-verification.md#native-9a-observation); not full continuous variants |
| 9A variants / 10A deviations composed | Accepted definitions; missing bindings and execution OPEN | Contract requires separate baseline/variant results and state/ack equivalence |
| Required reached action support | PASS bounded implementation; continuous comparison OPEN | PR #521 (`78c201c3`) accepts ordinary Medical Herb selection/live inventory and host inventories/itemSlot observations; compare the winning original actions separately |
| All applicable H4 layers executed successfully | OPEN | Remake/harness after definition acceptance; actual continuous session through full 5B |
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
fields leave definition readiness OPEN. Whole-run PASS requires every applicable assertion, variant
and deviation result. Production legality must depend on state/content, not a fixed actor sequence,
round count or reference receipt history. Existing subsystem fixtures remain their own owners.

### Initial deviation inventory mapped to H4

The contract maps all ADR0010 deviations: controlled construction (1A), optional-route exclusion
(2A), fixed evidenced reference seed/trace with manual play (4A), no save (6A), logical remapping and
accessibility (9A), and explicitly identified out-of-domain safe behavior (10A). Every result is
visible separately in layer 10, including passes. 7C private handling is a product boundary, not a
fidelity waiver. Missing evidence/content cannot be recategorized as a deviation.

## Dependency and completion boundaries

| Owner | Work remaining / dependency |
| --- | --- |
| Research | Accepted missing original fields and RA-12 effect; PR #519 is source preparation only, not a native result; later #515 observations await acceptance on main |
| Design | Comparison definitions independently accepted; bounded offline bindings available, precise remaining gaps OPEN; incorporate accepted original corrections in the same outcome |
| Remake/content | Manual Herb accepted in PR #521; original audio/private provenance (#517) and battle-scene consumption (#523) remain OPEN, no unmerged results assumed |
| H4 executor | Bind accepted records, run all ten layers and 9A variants through existing actual state/input/presentation surfaces; preserve failures and Unavailable |
| Main-gate | Independently accept definitions, evidence closures and eventual complete H4 result; serialize integration |

Accepted [outcome implementation](../../../remake/docs/exploration-programs.md#battle01-outcome-after-program-and-return)
and R4a comparison prove their bounded common-session/static-spine behavior, not natural original
expected values or this H4 run. The [capability ledger](../../../remake/docs/capability-status.md)
retains other unsupported consumers. Do not expand scope to EGRESS or unrelated item/menu branches
unless the selected route requires them. The existing 9A observations include remapped/swapped
input and authored paired flash/text checks; physical driver/hot-plug and complete export remain
unverified. Missing continuous variants cannot inherit those bounded passes.

### Conditional runtime questions

Only a named missing original semantic assertion can justify separately admitted observation under
ADRs0014/0016 and [ADR0015](../../decisions/0015-original-reference-replay-and-h4-boundary.md).
Current questions are RA-12 nonneutral effect, unresolved selected input/RNG/state fields and required
8D consumption/ack boundaries. Reuse accepted static rules and bounded observations first. The
[Research presentation audit](../../research/map3-battle01-audit.md#presentation-sufficiency-under-8d)
records DisplayText bypass; a program return does not prove unshimmed delivery. Persistent music
requires start/replacement/stop where reached, not a fictitious end event. This ledger authorizes no
native launch and no automatic per-Unknown observation queue.

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
