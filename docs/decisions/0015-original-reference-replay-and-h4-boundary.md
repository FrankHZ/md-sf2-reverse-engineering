# ADR 0015: Original-Reference Replay and H4 Boundary

- Status: **Accepted**
- Decision date: 2026-08-24
- Scope: conditional original-console scenario replay, private reference capture, and remake H4 ownership

## Current acceptance scope

[ADR 0010's current amendment](./0010-map3-battle01-product-acceptance.md#current-acceptance-amendment)
supersedes this document's former 8C hardware-exact requirements with 8D gameplay/presentation
semantics. Natural continuity, the controllable 5B endpoint, private 7C content, 9A/10A and applicable
H4 evidence/execution remain required. Earlier 8C-specific capture, clock/tolerance and exact-backend
requirements below are historical, not implementation or completion prerequisites. The old original
replay path is disabled and is not a mandatory evidence route. Existing failure records, evidence
labels and ADR 0015 launch limits remain binding; no new observation is authorized by this change.

## Context

[ADR 0009](./0009-first-phase4-playable-slice.md) selects one continuous Map 3-through-completion-of-
Battle 01 milestone. [ADR 0010](./0010-map3-battle01-product-acceptance.md) requires a private-local
original-fidelity profile and exact reached frame, audio, and hardware acceptance. [ADR 0011](./0011-phase4-remake-runtime-architecture.md)
separates original evidence, deterministic remake behavior, Godot adapters, and layered H4 results.
Those decisions do not yet name the boundary between replaying the original ROM to acquire reference
evidence and passing H4 against a remake.

That distinction is necessary if an R4b or H4 runner work item is admitted. An original-console replay can prove
what the pinned game did under one controlled input trace. It cannot prove that a remake matches it.
Conversely, a remake H4 runner must consume accepted evidence; it must not generate its golden by
running an original emulator inside the same comparison and must not treat Lua as a second gameplay
implementation.

R4a owns the static victory/after-battle/return spine. The Issue #496 amendment below additionally
records bounded interactive victory, after-program return and two-frame stable field readiness.
Issue515 also records the post-endpoint ordinary input effect described below. Independent result
acceptance remains required; remaining 8D and H4 are not established.
Frozen R4b replay has not been implemented. For R4b original-reference replay and remake H4,
the launch counts are therefore **H3 = 0** and **H4 = 0**. This decision adds no fixture, schema,
research-index record or association, address binding, CLI command, verification partition, or
counter: every such delta is zero in this governance slice.

## Decision

### Post-victory ordinary-input acquisition (Issue #515)

Issue #515 extends the accepted segmented victory rail to observe RA-12's next ordinary input and
its settled state effect. The [acquisition owner](../research/map3-messenger-acceptance.md#post-victory-ordinary-input-preparation-issue-515)
owns the concrete source/preparation boundary, missing fields, shortest compatible method and
retained costs. Prepared-86 remains terminal/nonresumable; old execution sources or raw settings
cannot be migrated to bypass compatibility. A fresh compatible natural chain retains the full
victory/after-program lineage, then delivers one direction through the ordinary field input rail.

Independent main-gate source/preparation admission precedes native execution. Existing stabilization
authorization then continues in the same Issue without an old attempt quota or repeated user approval;
identity, core, callback, I/O, idle/exchange and process containment remain binding. A source Draft PR
is an intermediate checkpoint, not observed input, Issue completion, full 8D or H4. Keep old failures,
later tooling diagnostics and all newly incurred costs; native evidence still needs independent
acceptance and a tracked result handoff.

**Confirmed (bounded native observation, 2026-09-22):** after PR519 source/preparation acceptance,
the [Issue515 result](../research/map3-messenger-acceptance.md#native-post-victory-ordinary-input-result-issue-515)
retains the observed natural victory/after-program spine and pre-input facts, then observes one Down
read and movement acceptance at frame 61011. Map57 player `(5,12)` becomes `(5,13)`, settled at 61023/61024,
with original raw displacement, complete state readbacks and no pending consumers. All 19 new
native invocations complete with cleanup; segmented totals are 136 starts / 15540.144989400113
seconds / 428408 frames / 16188 batches. A local accepted-main guard failed before process launch
and is separately retained; a later account-limit interruption did not interrupt native completion.
No old seal or source/settings compatibility was changed. Host unreviewed status still requires
independent result acceptance; this evidence does not itself close RA-12, remaining 8D, H4 or #437.

### Bounded instrumented HEAL diagnostic (Issue #546)

The [HEAL diagnostic method](../research/map3-messenger-acceptance.md#instrumented-heal-diagnostic-method-issue-546)
may reuse the explicitly named, accepted Issue515 nonterminal parent through the existing acquisition
rail. Its measurement provenance is distinct from the immutable parent's observer/runner identity;
ordinary exact-identity continuation remains unchanged. Only the reviewed observer/runner addition
and separately bound HEAL source observations may differ. ROM, core/tool/settings, existing source
bindings, execution helpers, sealed ancestry and load-before-input checks remain binding. Exposed
RAM/register/frame equality with an older parent does not prove full native-state equivalence.

The output is a nonresumable diagnostic leaf, never an ordinary forward segment or a replay/H4 pass.
It has no save or descendant and stops after the selected scene return and current completed frame.
Typed append-only claims retain the original lineage and all completed costs/failures. Finite
per-attempt containment does not introduce a permanent attempt quota, reset a historical budget or
reinstate retired cumulative ceilings. The user's stabilization authorization and existing failure
accounting continue to control bounded corrections after an explicit native dispatch.

Source implementation and offline preparation do not authorize restore or native observation.
Main-gate independently reviews frozen source/candidate before dispatch; missing coverage, incompatible
load or failed cleanup remains Failed/Unknown. This amendment neither reopens the disabled original
replay nor grants a new replay launch budget. The
[accepted neutral self-HEAL1 diagnostic](../research/map3-messenger-acceptance.md#accepted-neutral-self-heal1-diagnostic)
confirms its named parent restore and live consumer boundary. Compatibility beyond that instance,
other settings/targets, input shortening and broader native-state equivalence remain **Unknown**.

### Bounded first-warp field-return diagnostic (Issue #534)

The [named method](../research/map3-messenger-acceptance.md#first-warp-field-return-diagnostic-method-issue-534)
reuses fresh controlled R1 acquisition to distinguish first post-FadeIn field return
from subsequent neutral player waiting. No accepted pre-warp R1 parent is available.
The leaf retains the first source poll after warp/init with fade clear before ordinary
per-frame deduplication; failed closure is evidence and cannot be skipped for a later
poll. Its declared Left30/neutral120 suffix ends automatically at the last completed
neutral frame through existing snapshot/restoration/callback/status handling. No Right,
later route, save, sealed pair or descendant is admitted. Ordinary forward, HEAL
parent compatibility and abort failure semantics remain unchanged.

Source/offline verification and preparation authorize no native launch or restore.
Independent main-gate review of frozen source and concrete preparation precedes any
native dispatch under the existing stabilization authorization. Inherited137 starts /
15568.554376200133 active seconds /429120 resource frames /16211 batches, separate
tooling/gate costs and21/41/61/67 cleanup Unknowns remain retained. Per-attempt bounds
are containment, not gameplay timing or a new permanent quota. A completed method
does not establish original state, a desired seed/Wait count, #534/#437 or H4 acceptance.

### Distinguish interactive acquisition, frozen replay, and remake H4

**Interactive original acquisition** is Research-owned observation in which an operator or host
selects the next explicit ordinary controller input from facts observed in the original. Lua only
delivers that input and observes source-bound events; it does not choose a route or implement game
mechanics. Record actual delivered frames, including neutral and declared bootstrap frames, rather
than treating a requested batch as an input trace. Report callback-time facts separately from the
paused frame-end state. An acquisition result remains unreviewed original evidence until independent
Research acceptance; it is neither a frozen-replay PASS nor an H4 PASS.

**Frozen original-reference replay** retains the immutable non-adaptive trace and passive observer
rules below. Acquisition may produce an input recording, but freezing/replaying it is a separate
operation only when a determinism claim needs it and that operation is separately admitted. It is
not an automatic sequel to successful acquisition. **Remake H4** consumes independently accepted
Research evidence and cannot generate original truth inside its comparison.

The specific [Issue 473](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/473)
authorization permits governance, minimal implementation and offline preparation with **zero emulator
starts**. Its [Map3 owner](../research/map3-messenger-acceptance.md#bounded-interactive-acquisition)
reuses the existing bridge and controlled R1 start. R1 initialization and final restoration are
declared interventions. Between R1 service/scratch restoration and final cleanup, ordinary controller
input is the only advancing mechanism: no RAM/register/ROM writes, forced flags, prompt results,
warps, or route transitions. This controlled start is not a passive frozen replay.

The method retains **two completed controlled failures**, #460 and #465. After independent review
and merge, main-gate may separately admit **at most one additional acquisition**, bringing that
historical controlled total to **three**, never resetting it. Its limits are one process, 1800 seconds
including paused operator time, 28634 total emulator frames including bootstrap, and at most 2048
batches of 1–120 frames. First typed failure, disconnect/abort, or first source-bound Map19 movement
control after gate/F604/north-warp/init/consumer returns ends it, including the remainder of a batch.
No extra smoke, automatic retry, replay, fourth start or general future acquisition allowance follows.
The disabled original replay ordinal 1/2 lineage and its existing restrictions remain separate and
unchanged. Existing 8D, 7C, 9A/10A and H4 acceptance boundaries remain unchanged.

### Savestate-linked segmented acquisition amendment

For [Issue #485](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485), the user authorized
native savestate collection and then explicitly required continuing until stable, including improving
wrong-facing interaction handling. The [stabilization authorization](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5752405430)
supersedes prior per-run approval stops, fixed cumulative ceilings and failure retirement for this work.
Main-gate retains independent review/integration. Historical failures, consumed starts and resource
costs remain unchanged; disabled frozen replay is not reopened.

The [Map3 owner](../research/map3-messenger-acceptance.md#savestate-linked-segments-issue-485) defines
early closed-field checkpoints, live entity/consumer readiness and original B cancellation recovery.
Ordinary not-ready input/save requests remain paused nonterminal zero-frame results. No direct RAM,
register, actor or RNG repair is allowed. Save/load execute outside callbacks and require coherent
original core plus observer continuation; verified load precedes any input. Forward checkpoints cannot
repeat a parent. Completed failed children may retry from the last complete compatible parent while
retaining unique attempts and all real costs; a successful child becomes the next parent. Source/tool/
observer identity mismatch blocks reuse rather than weakening compatibility checks.

Cumulative time, frames, batches, stage and source-progress values remain observations. Single-step
maximum 120, startup/exchange 60 seconds, operator/disconnection idle 120 seconds and teardown 3+3
seconds retain fault containment, as do identity, I/O, core and irrecoverable callback failures.
Standalone bridge limits are unaffected. Initial accounting includes every failed attempt; children
inherit sealed and failed-child consumption without fabricating observer/emulator clocks.

Acceptance requires an actual early save → verified native load → forward later save, followed by
independent review. The [house → native load → Sarah forward-save chain](../research/map3-messenger-acceptance.md#accepted-early-native-save-and-resume)
is now **Confirmed** by actual execution and independent acceptance. Its
[forward continuation](../research/map3-messenger-acceptance.md#resumed-messenger-and-map19-checkpoints)
includes independently accepted messenger and native Map19 saves.
Later [royal/guard saves and loads](../research/map3-messenger-acceptance.md#royal-and-guard-saves-battleloop-return-stack-failure)
are independently accepted. The [fresh compatible chain](../research/map3-messenger-acceptance.md#corrected-chain-and-player-entry-window-count-failure)
confirms the corrected CheckBattle return and original battle lifecycle. It reaches the first-player
input PC but fails a mistaken final window-count predicate; this is not passed readiness. Pinned
source/H1/ROM shows the normal mini status window contributes to that count while movement input is
available. Removing only that incorrect modal inference retains the actual consumer/input/transfer
guards and requires another fresh compatible chain with all costs retained. No parent-identity waiver
is allowed. The [next compatible chain](../research/map3-messenger-acceptance.md#movement-grid-palette-failure-and-correction)
confirms window-count handling but rejects mode 5 of `FADING_SETTING`, which the original enables for
nonblocking movement-grid pulsation before accepting player input. The final-only correction admits
idle mode 0 or that selected mode 5 and retains the other input/consumer/transfer guards; field/save
fade checks are unchanged. The [final compatible chain 33–37](../research/map3-messenger-acceptance.md#accepted-first-battle01-player-ready-acquisition)
now provides **Confirmed** actual first-player-ready acceptance and an independently reviewed,
nonresumable final evidence pair. At `0x22E70`, actor 2 has window count 2 and palette mode 5 with
the other actual blocking guards clear. Observer frame 22368/emulator frame 22367 at the callback
is distinct from completed save emulator frame 22368. The last batch stops after 6 neutral frames;
no battle action follows. All five segments complete automatically with verified continuity and
cleanup. All historical costs remain charged: 35 starts, 6562.418724000221 seconds, 105007 resource
frames and 2063 advancing batches. This completes the bounded acquisition endpoint, not #437,
Battle01 actions/victory/5B or H4, which remain **Unknown**.
The second idle
failure's final callback/restoration evidence is also **Unknown**; preserve it separately from the
confirmed cleanup of explicit abort and callback failures. Finite local operators may consume the
existing request/response interface with actual readiness and source-closure checks; they neither
change the acquisition into frozen replay nor authorize gameplay beyond the endpoint. This remains
**savestate-linked segmented original acquisition**, not uninterrupted wall-time execution, frozen
replay, natural visible New/load, or H4. Historical stop restrictions below apply to their original
attempts and cannot override this user-directed stabilization.

### Battle01 victory acquisition amendment (Issue #496)

[Issue #496](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/496) is the user's next
authorized #437 outcome after accepted #485. It reuses segmented interactive acquisition for
ordinary player/AI actions, natural victory, the actual after-program and controllable 5B. The
[existing acquisition owner](../research/map3-messenger-acceptance.md#battle01-actions-and-victory-continuation-issue-496)
defines the explicit continuation, source bindings, input consumers, reconstructible battle-return
descriptors, unsupported outcomes and final readiness predicate. No new fixture/transport/framework
or frozen replay is admitted. The previous terminal pair remains nonresumable; changed execution
sources require a fresh compatible chain.

First integrate the offline implementation independently. Main-gate then checks the concrete
source/start/checkpoint preparation and dispatches native collection in the same accountable task.
Ordinary reviewed collection and corrections require no repeated user permission or per-segment
approval. Historical cumulative totals remain charged observations, while identity, core, I/O,
callback, exchange and operator-idle containment remain enforced. A capability Draft PR does not
complete the Issue: actual observation, independent acceptance and tracked result integration are
still required. PR499's accepted source now has bounded native battle save/load/action evidence
ending in original defeat, with a preserved terminal transport **FAIL**: 78571 bytes exceed the
unchanged 65536-byte bridge limit. Subsequent accepted PR500 source collected prepared-17..32:
corrected native replies, original Heal 1 input/effects, recoverable battle checkpoints through
round 8, and three further original defeats with complete cleanup. Those totals were 61 starts /
8837.811812300177 seconds / 192811 frames / 5222 batches, including every failed child;
failed16's transport FAIL and failed21's final cleanup Unknown remain preserved.
Unused original Medical Herbs exposed a concrete input gap. The bounded PR501
extension admits only original Item→Use→Medical Herb→target input, with menu-context, slot,
consumption/effect, cancellation and pending/save checks; Equip/Give/Drop and other manual items
stay explicitly unsupported. Inventory/spell review identifies no other currently necessary
ordinary input for this battle route. After source acceptance, 33..44 confirm original herb use,
consumption/healing and a later original defeat. Totals through that chain were **73 starts /
10247.52403570019 seconds / 229866 frames / 7193 batches**. Segment 41's local operator
readiness/pipe failure retains forced containment and final restoration/callback **Unknown**;
defeat 44 has complete cleanup. It grants no reuse
of old-source parents or normalization of HP/MP/items/RNG. Finite operator tactics may change
within accepted source, while failures and actual input decisions remain retained.

Successful descendants previously locked an earlier parent against tactical recovery, even after
all native processes ended. The bounded runner correction reuses immutable parent links/claims
to reconcile the whole compatible source lineage before selecting an earlier complete save.
Sibling/cousin costs remain charged, with actual increments checked against cumulative prior-cost
order; missing, active, duplicate, cyclic, incompatible or unreconciled attempts block resume.
Terminal pairs are chargeable completed descendants, never resume parents. The one-writer boundary
remains. Accepted PR502 subsequently records actual earlier-parent recovery and a selected branch
through original victory, after-program return, F401-clear/F501-set, BattleLoop D4=1 return and
exploration reentry in prepared-67. The subsequent setup callback fails before stable field control;
its host cleanup does not prove missing final observer restoration/callback cleanup. Abort 61 retains
the same distinct Unknown. Totals through that failed chain were **96 starts / 12405.304031100066 seconds /
304294 frames / 10553 batches**, including every branch and failure.

The [acquisition owner](../research/map3-messenger-acceptance.md#native-victory-after-program-return-and-post-victory-observer-failure)
records the bounded native result and minimal offline correction: source-bound map 57 void setup
selection and the after-cutscene's actual shared function-tail ownership. After independent PR503
source/preparation acceptance, prepared-68 starts a fresh R1 with all historical costs; strict
identities prohibit loading old-source pairs. Prepared-68..86 finish at a nonresumable terminal
pair, with all 19 native exits, entry restoration, callback removal and ROM cleanup confirmed.
The [final result](../research/map3-messenger-acceptance.md#native-victory-and-stable-field-readiness)
records bounded **Confirmed** original victory/after-program return and settled neutral field frames
61009/61010 with actual player-control polls at Map57 (5,12), facing DOWN and battle255. Current
interactive totals are **115 starts / 14197.020479699888 seconds / 365304 frames / 13351 batches**;
all earlier failures remain charged and their unresolved cleanup Unknowns are preserved.

The PR504 collector stopped before any post-endpoint nonneutral ordinary input. Its acceptance and
state effect remained **Unknown** at that boundary; the later Issue515 result above observes it.
Two-frame 5B readiness does not prove full RA-12, remaining presentation consumption, full 8D, H4 or completion of #437. This remains
savestate-linked interactive original acquisition, not uninterrupted wall-time or frozen replay.
Disabled replay boundaries and their zero launch/counter delta are unchanged. No new fixture,
schema, CLI, shared transport or H4 claim is introduced by the result.

### Use distinct names and evidence owners

The following terms are normative:

- **Original-reference replay** means deterministic execution of the pinned original ROM in the
  accepted original-runtime toolchain from declared private-local start inputs, driven by one frozen
  non-adaptive input trace.
- **Original-reference capture** means the private output set produced by that replay, such as a movie,
  save state, SRAM image, frame/audio capture, trace, or bounded state observation.
- **Original-reference receipt** means the typed result that identifies the replay inputs, runner,
  observed completion status, private artifact identities, and cleanup outcome.
- **H4 fixture** means an accepted, implementation-neutral comparison definition that consumes
  Research-owned reference evidence and the product/architecture decisions. It does not execute the
  original ROM as gameplay logic.
- **H4 PASS** means that the remake has executed the accepted H4 profile and satisfied every applicable
  fixture layer, private-reference comparison, cleanup rule, and declared-deviation rule. An
  original-reference replay, matching original capture, emulator exit code, or clean Lua Console is
  never by itself an H4 PASS.

Original-reference replay is a Phase 2 Research/H3 evidence operation. H4 definition is Design-owned
eventual-milestone and conditional acceptance work, and H4 execution is a later remake acceptance
operation after the separate Phase 4 start action. Neither owner may rename original replay as remake
parity or use an H4 label to promote unaccepted runtime observations.

Under [ADR 0016](./0016-remake-start-evidence-deferral.md), original-reference replay, complete 8C
capture, continuous-scenario evidence, and H4 capability are not default prerequisites for a separately
user-authorized implementation start. They remain conditional evidence and acceptance work. A concrete
implementation or acceptance ambiguity may trigger them only through ADR 0014's immediate three-part
H3 gate, with accepted static owners and reusable rails considered first.

### Keep the original observer passive

An original-reference replay uses the original program as the sole route, collision, zone, warp,
battle, and campaign-mechanics implementation. Its Lua observer may:

- register bounded callbacks at source-backed lifecycle checkpoints;
- read registers, memory, bus facts, and emulator-provided frame/audio facts;
- emit typed observations and bounded diagnostics;
- verify callback completion, timeout, exit, and cleanup state; and
- identify or hash private artifacts without embedding their payloads in public output.

For this replay class, Lua must not write RAM or registers, redirect the program counter, patch the
ROM, choose an input based on observed state, synthesize a missing transition, or reconstruct route,
collision, zone, warp, pathfinding, battle, reward, or after-battle mechanics. The frozen input trace
is supplied by the runner or emulator movie/input boundary; Lua does not become an input policy.

A checkpoint table may name what to observe and when to fail. It may not calculate the state that the
original game should have reached and then substitute that calculated state for observation. Host-side
validation may compare the emitted receipt with an accepted fixture only after the original program
has produced the facts.

### Preserve the private and copyright boundary

Original ROMs, movies, save states, SRAM images, input recordings that embed emulator state, frame or
audio captures, screenshots, traces, memory dumps, decoded assets, and detailed replay receipts remain
ignored private/generated inputs under `local/`. They are never committed, uploaded, attached to a PR,
redistributed, required by Public CI, or pasted into a public handoff.

Tracked artifacts may contain only separately reviewed, public-safe schema and fixture definitions,
allowed identity/provenance, non-reconstructive aggregates, and PASS/FAIL/Unknown results. A hash may
identify a private local input when the owning contract explicitly permits that identity; it does not
make the payload public or safe to distribute. Missing private inputs make the private profile
Unavailable, not Passed and not eligible for a silent substitute.

### Give every future artifact one lane owner

This decision reserves ownership classes without creating their paths or commands:

| Surface | Future owner and boundary |
| --- | --- |
| Original-reference runner capability | One conditionally admitted Phase 2 Research tooling slice under the H3/original-runtime boundary. It owns runner preflight, passive-observer enforcement, typed receipts, timeouts, cleanup, and capability tests, but no R4b scenario claim. |
| R4b scenario reference evidence | A later, separately and conditionally admitted Research scenario-evidence slice. It consumes the accepted capability runner and owns the frozen R4b input trace, private captures, public-safe Research projection, evidence labels, provenance, and any justified research-index associations. |
| H3 fixture and counter | The scenario-evidence slice owns any future `schemas/h3/` and `tests/fixtures/h3/` entry plus its H3 registry/native-counter update. Capability-only diagnostics do not increment scenario-fixture counters. |
| H4 fixture and counter | A later Design/H4-definition slice owns future `schemas/h4/` and `tests/fixtures/h4/` definitions and a distinct H4 counter. It consumes accepted Research from `main`; it does not relabel an H3 receipt. |
| Research index | The Research scenario-evidence slice owns reference-evidence records/associations. A later H4-definition slice may add only the accepted H4 association needed by its fixture. Both are serialized shared-file changes with exact parsed-object proofs. |
| CLI | The capability/evidence runner is registered in the H3 original-reference namespace. A future remake comparator is registered in a distinct H4 namespace by the H4 harness owner. Exact leaf command spelling is selected by those implementation slices. |
| Verification planner | Original replay selects an H3 partition serialized on `bizhawk-original-runtime`. H4 receives a distinct future partition and resource lock when its executable harness exists; it is not hidden inside an H2 or H3 partition. |

No later slice may combine these owners merely to avoid a serialized review. A capability result is not
scenario evidence, a scenario reference is not an H4 definition, and an H4 definition is not a remake
PASS.

### Require a deterministic replay receipt and typed cleanup

Every diagnostic or frozen acceptance launch writes to a fresh ignored output directory and emits one
typed receipt. The receipt records, at minimum:

- scenario and run-class identity (`diagnostic` or `frozen-acceptance`);
- exact ROM, emulator/core, runner, observer, configuration, start-state, SRAM/save-state/movie/input-
  trace, and accepted evidence identities as applicable;
- the declared non-adaptive logical input/replay identity and deterministic clock/configuration facts;
- launch ordinal, bounded timeout result, process exit, Lua/parser/callback status, observed terminal
  checkpoint, and typed failure reason;
- private capture inventory by allowed identity and hash, never embedded payload;
- callback unregistration, emulator/process termination, temporary-file disposition, and residual-
  process/callback checks; and
- the canonical receipt identity after excluding explicitly non-semantic local diagnostics such as a
  wall-clock timestamp or absolute machine path.

An acceptance receipt is valid only when the declared frozen inputs were fixed before launch, the
expected original lifecycle produced the terminal observation, the Lua Console and typed callback
status are clean, and cleanup is complete. Timeout, callback exception, missing terminal state,
unexpected process survival, cleanup failure, or input/capture identity drift is a failed receipt. The
runner must not overwrite a prior receipt or delete an arbitrary caller path to recover from a
collision.

### Sequence capability before conditionally admitted scenario evidence

If the gate admits the work, runtime work proceeds in two separately reviewed slices:

1. **Runner capability slice:** establish deterministic input replay, passive observation, receipt
   shape, bounded process lifecycle, private-output isolation, and cleanup against a capability case.
   It makes no R4b natural-route, victory, endpoint, or H4 claim.
2. **Scenario evidence slice:** only after the capability slice is independently accepted and merged,
   consume the unchanged runner for the R4b scenario. Freeze the scenario inputs before its acceptance
   launch, classify every observed and unobserved claim, and land only the reviewed public-safe
   projection plus required associations.

Only accepted scenario evidence on `main` may feed a later H4-definition slice. H4 runner
implementation and remake H4 execution remain later acceptance work after the separate user Phase 4
start action; they are not a default implementation-start gate.

### Enforce a hard launch stop-loss

Each authorized runner-capability or scenario-evidence slice may perform at most **two diagnostic
launches followed by one frozen acceptance launch**. There is no fourth launch. A launch counts when
the emulator process starts; a preflight failure before process creation does not count. A diagnostic
launch may improve observation or identify a runner defect, but it may not change original mechanics
or generate scenario truth through a simulator.

The acceptance inputs, observer, expected checkpoints, and output contract are frozen before the
acceptance process starts. If that run diverges, times out, exposes a missing capability, or fails
cleanup, the result remains Failed or Unknown and the lane stops. It must not add Lua route logic,
collision handling, zone/warp selection, battle simulation, adaptive input, or another launch to make
the golden pass. Opening a nominally new slice only to reset the launch count is prohibited; further
runtime work requires independently accepted new static evidence or a separately corrected runner
capability and fresh main-gate ownership.

Failed R2b and original-reference replay launches or candidates remain non-evidence under their
existing owners. They cannot be resurrected, renamed, reset, or reused to obtain a new launch budget;
this ADR permits no nominal reset or fourth launch.

## Relationship to Existing Decisions

[ADR 0001](./0001-bizhawk-for-h3-runtime-observation.md) remains authoritative for the pinned BizHawk
toolchain, parser/callback failure boundary, and ignored runtime outputs. This decision narrows the new
original-reference replay class to passive observation even though older bounded H3 fixtures may use
controlled RAM inputs.

[ADR 0012](./0012-dependency-aware-partitioned-verification.md) remains authoritative for the current
public core and H1/H2/H3 planner. This decision reserves a future first-class H4 owner; it does not
change the planner. [ADR 0014](./0014-static-first-runtime-evidence-after-map3-battle01.md) continues
to require static-first Research and does not turn a runtime-question register into automatic replay
authorization.

[ADR 0016](./0016-remake-start-evidence-deferral.md) controls the separate user-authorized
implementation-start policy; it does not weaken this decision's passive-observer, receipt, cleanup,
private-boundary, ownership, or stop-loss requirements.

## Consequences

- The original emulator remains an evidence source, not the remake gameplay core.
- A clean original replay can be reviewed without being mislabeled as remake parity.
- Private movies, states, SRAM, and captures remain usable locally without entering Git or Public CI.
- Runner bring-up has a bounded diagnostic budget and cannot grow into a Lua reimplementation.
- H3 and H4 fixtures, counters, indexes, CLI commands, and planner partitions acquire explicit,
  serialized owners when their separately authorized slices begin.
- Frozen R4b replay and H4 remain **Unknown**, with zero launches and zero repository
  evidence-counter delta on those rails. Interactive acquisition has its own observed costs and results.
- This decision does not start R4b, H4, Godot, `remake/`, Phase 4, or any runtime process.

## References

- [ADR 0001: BizHawk for H3 Runtime Observation](./0001-bizhawk-for-h3-runtime-observation.md).
- [ADR 0009: First Phase 4 Playable Slice](./0009-first-phase4-playable-slice.md).
- [ADR 0010: Map 3 to Battle 01 Product Acceptance Profile](./0010-map3-battle01-product-acceptance.md).
- [ADR 0011: Phase 4 Remake Runtime Architecture](./0011-phase4-remake-runtime-architecture.md).
- [ADR 0012: Dependency-Aware Partitioned Verification](./0012-dependency-aware-partitioned-verification.md).
- [ADR 0014: Static-First Runtime Evidence after Map 3 to Battle 01](./0014-static-first-runtime-evidence-after-map3-battle01.md).
- [ADR 0016: Remake-Start Evidence Deferral](./0016-remake-start-evidence-deferral.md).
- [Map 3 Battle 01 Victory and Return Static Evidence](../research/map3-battle01-victory-return.md).
