# Engine, SF2 rules and story-program boundary

The current remake can express another conversation, flag branch, call/wait sequence, supported
map event or recruitment ceremony as typed content. It is not a chapter-independent engine with
every gameplay rule supplied by scripts. Several source services deliberately remain compiled SF2
rules. That distinction is useful: moving Taros's weapon rule or physical-slot semantics into every
chapter script would duplicate rules, not improve authoring.

At the audit base, two inspected seams needed focused correction before dependent work: battle outcomes replace an
authored field player's identity/speed with source defaults (**B1**), and a scripted transfer omits
the selected story policy during early battle selection (**B2**). Neither requires a new scripting
system. The Map3 scratch exception is deliberately isolated compatibility policy; Jaro's ceremony
is already content; Caravan's fixed choreography is an intentional finite source service. The
poison-only standalone terminal boundary and definition-copy risk F3 are conditional follow-up
concerns, not newly authorized implementation.

B1 is implemented under [#1381](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1381);
its current binding lifetime and source rule belong to the
[session owner](../application/session-and-programs.md#field-player-binding-across-connected-battles).
B2 is implemented under [#1382](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1382);
its selected-rule guarantee belongs to the [session owner](../application/session-and-programs.md#selected-rules-during-map-transfer).
The conditional concerns retain their separate scope. Audit source links below remain pinned to the
inspected base; implementation dispositions add bounded engine/native evidence without changing
those historical findings.

## Scope and evidence

**Standard rigor:** bounded investigation and design for [#1379](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1379).
The deliverable supports the decision about what to fix before further chapter/content work.
The original audit changed only this report and an architecture navigation link; production, tests,
content, research and prior audit history were read-only for that investigation. The separately
authorized B1 implementation is recorded in its disposition and owning session agreement. Remaining
proposals do not authorize refactoring or replace the current architecture owners.

Inspected accepted source: **`b4882a0dddd045b44c9110e43b097efd9358d652`**, tree
**`77df88ad880b0365103d9f3b91e5457cf91d16cc`**. Source links below pin that commit. Symbol names
are the reproduction anchors; line links locate the inspected implementation. The prior
[structure review](structure-and-quality-review.md) supplies F1/F2 repair context and deferred F3;
those repairs are already in this base. Their historical observations are not new runs here.

**Confirmed** below means directly established source structure or inspected assertion content.
**Inferred** identifies the predicted consequence of a stated execution path that this audit did
not run. **Unknown** is an unobserved or excluded boundary. No new runtime experiment, suite,
native process, emulator, private extraction or asset acquisition was needed. Existing tests were
read, not rerun. Assertions described below are not reported as fresh PASS results.

The inspection uses selected text files and caller searches, linear in bytes inspected, with no
whole-history/data acquisition. New ignored output is confined to `local/issue1379`, budgeted below
1 MiB. A runtime dispute or a new execution platform would require a focused claim and revised
method; this report does not initiate either. #638 remains paused; #1047's Unknowns and the separate
UI topic are unchanged.

## Responsibility map

Assembly placement is not the classification test. The test is what decides behavior and who may
publish it. The same Application assembly contains both generic continuation and SF2 policy.

| Semantic responsibility | Actual owner and boundary | What an author changes |
| --- | --- | --- |
| Runtime mechanics | [GameSession][session] binds rules once, validates command envelopes and alone assigns `Current`. [ProgramRunner][runner] owns PC, callers, waits, instruction commits and retained prefixes; dispatchers calculate results. | Programs reuse this machinery. A new blocking effect needs an explicit typed operation and completion semantics, not an alternate state owner. |
| SF2 gameplay rules | Domain numerical/action/activation/spawn rules; [Sf2OutcomeReturnPolicy][outcome-policy] chooses recovery/return; [BattleStoryOperations][battle-story] implements ordered conversion and membership writes. Some fixed services remain in `Runtime/Exploration`. | Existing parameters/bindings are data. A different algorithm requires ordinary C# composition where supported, or a separately scoped fixed-service extension. |
| Authored story facts | [ExplorationContentReader][reader] constructs programs, ordered events, map routes, flags, positions and text. [Jaro content][jaro] supplies the recruitment order; [story A][story-a] and [story B][story-b] supply different conditional histories. | Change linked data and restart. No package-name dispatcher chooses the story. Supported operations still have context and operand restrictions. |
| Source compatibility | [Sf2StoryPolicy][source-policy] owns the exact retired Map3 scratch seam. [SourceStoryRules][source-port] validates its narrowly bounded candidate. Source population uses logical aliases and physical slots. | Preserve the named source guard. A different source seam needs evidence and a finite operation; it cannot reuse an unrelated compatibility exception by renaming IDs. |
| Private trust/admission | [PrivateExplorationReader][private-reader] verifies source/ROM provenance against admitted private definitions before shared decoding. Selected private encounter import stays separate from authored admission. | New private bindings require admitted evidence/import support, even if execution needs no new instruction. Authored IDs cannot confer private fidelity. |
| Host presentation | [GameRoot][host] selects source and `ForGame()` composition, wires one existing session into both views and installs return callbacks before initial view choice. Views submit semantic commands and actual completion. | Assets/text/presentation may need work for a new chapter. The host should not decide recruitment, flags or outcomes from chapter IDs. No such chapter branch was found in the inspected root. |

This preserves [ADR 0017](../../../docs/decisions/0017-heavy-boundaries-light-internals.md) and
[ADR 0019](../../../docs/decisions/0019-state-and-content-driven-remake-engine.md): one authority,
heavy trust/mutation boundaries, light internal calls. The current
[finite replacement scope](../domain/gameplay-rules.md#scope-and-fixed-services) explicitly excludes
general refresh, activation/control, movement-cost and field/scene service replacement. These
exclusions do not make every source constant a bug, nor certify every constant as appropriate.

## Findings and smallest corrections

### B1 — implemented: retain field-player binding across battle

**Confirmed at audit base: source mismatch. Confirmed by #1381 reproduction: named-player failure
and corrected authored return/movement.** The following pinned analysis describes the audit base.
[BattleOutcome.Begin:36][outcome] and `ReturnFromRetreat:72` call `MapTransfer.Build` with
`new("entity-0")` and speed `32`. These are shared execution paths, not private-only branches.
[ExplorationContentReader.Read:259][reader] accepts `start.player` and speed1–384;
[ExplorationDispatcher.Start:19][dispatcher] passes them into the initial world.
[MapTransfer.Build:185][transfer] inserts the requested player and, without source population,
excludes only that ID from ordinary map entities. Later `MapTransfer.Apply:293` preserves the
*current* world's player and speed, so the outcome replacement persists through return.

Concrete data/context: [progression-outcome-victory.json:1164][progression] already starts
`traveler` at speed96 and supplies an outcome route. Its default source outcome policy does not
select a replacement field identity or speed. On the last qualifying battle action,
`GameSession.Submit` calls `BattleOutcome.Begin`; the latter builds `entity-0` at32 before running
the configured after-program. The source assignment is certain; this audit did not replay the
package to measure movement or host display.

A credible changed-content trigger is to add `face traveler` to that package's after-program or
returned-map program, while keeping `traveler` out of NPC definitions as at initial construction.
Strict named resolution in `ProgramRunner.Entity` then has no such player. The expected result is
`program-entity` at the reached instruction after the accepted outcome prefix. A map that instead
defines a `traveler` entity can bind that instruction to an NPC, which is also wrong for the
configured player. Movement otherwise continues using the substituted32 rather than96.

The legitimate counterexample is source population: [SceneEntities.Build:45][entities] explicitly
requires logical `entity-0`, and source physical player slot0 is meaningful. Preserve that source
contract. Applying its identity to a strict named authored world is the leak.

**Implemented disposition:** the existing `StoryState` entry/return continuation holds only a
`FieldPlayerBinding` (player ID and applicable speed). `BattleEntry` captures the live values after
the before-program and successful battle validation, including connected startup. Both outcome
projection paths consume it; clearing the completed return route clears the binding. Source
population explicitly retains `entity-0` and its established return speed32; authored fields retain
live speed, including an authored player named `entity-0`. No public Content start change, stale
world snapshot, battle-leader inference or registry is needed.

This is a small cross-continuation correction, with moderate review risk because entry, retreat
and both outcomes share the lifetime. Authoring benefit: a chapter can refer to its declared player
before and after combat without knowing `entity-0` or compensating with script renames.

**Validation:** `FieldPlayerReturnTests` reproduces the pre-fix `program-entity` failure for
`traveler/96` and `scout/192`. Corrected behavior covers victory/defeat/retreat projection,
direct/transfer entry, pre-battle speed changes, named motion/text/presentation waits, subsequent
map transfer, carried resources/RNG/flags, paid retreat, source population, failed initialization,
and a later failure's committed prefix/cursor. The ordinary host witness returns `traveler/96`
after a real victory, then moves x768→864 at y768 in the same session, with one return and no
runtime errors. The [bounded reproduction](../development-and-verification.md#authored-field-player-return)
records commands and limits; no screenshots or original replay establish additional fidelity.

### B2 — implemented: transfer carries selected rules

**Confirmed at audit base: missing propagation. Confirmed by #1382 reproduction: opposite-policy
entry ordering and bypassed throwing policy.** The following pinned analysis describes the audit base.
[ProgramRunner.Run:464][runner-transfer] executes `TransferToMap` by calling `MapTransfer.Apply`
without its final `rules` argument. In contrast, queued warp and `ReturnBattleMap` in that same
runner pass the selected rules. [MapTransfer.Apply:260][transfer-apply] accepts null; if
`world.Party.NewBattle` is non-null it asks `BattleEntry.First` whether to select a battle before
map rebuild/on-load. [BattleEntry.EnterCompleted:16][entry] substitutes `RuleCompositions.Sf2()`
when rules are absent. SF2 then admits a completed encounter iff `PrivateDefinitions` exists.
The session's bound `ISourceStoryPolicy.EnterCompletedBattle` is bypassed at this early decision.

The trigger requires all of: an initialized-party world (`NewBattle != null`), an unlocked but
completed destination route, a reached typed `transfer`, and a selected story policy whose answer
differs from the SF2 default for that definition. A trusted compiled replacement policy or a
source-compatible definition with a different selected replay policy supplies that difference.
The public authored JSON reader uses controlled starts with no `NewBattle`, so simply changing
story A/B or Jaro's JSON does **not** demonstrate this branch. No shipped default-composition
regression is claimed.

For a source-compatible private definition with a selected policy that refuses replay, fallback
can enter the completed battle before the selected policy gets control. For an initialized
definition with a selected policy that admits replay while the fallback refuses it, map rebuild
and on-load effects can run first; a later `BattleEntry.Continue` using the selected rules does not
undo that prefix. Thus the risk is entry ordering and unintended effects, not just a different
final battle flag. A destination on-load flag write makes the distinction observable. This audit
did not execute either constructed composition; the initialized engine tests below now reproduce both.

**Implemented disposition:** `ProgramRunner` passes its existing selected rules into typed transfer.
The policy-sensitive `MapTransfer` validation/warp/load/apply/continuation and `BattleEntry`
selection/continuation parameters are required and non-nullable. Direct comparison callers select
their composition explicitly; named source-default session/runner entries remain available. No new
registry, policy abstraction, source guard or Content trust change is involved.

**Validation:** `MapTransferPolicyTests` starts project-authored initialized definitions through
`GameSession` and typed simulation commands. Before repair, selected repeat loaded the destination
instead of retaining the origin for before-battle; selected refuse retained the origin and entered
before destination on-load; throwing selections produced no policy failure. The four ordinary/default
controls passed. Corrected behavior checks on-load versus before/load/start waits, exact PCs, flags,
unlock/intro handling, selected failure attribution, no speculative publication and committed
prefix/retry. Synthetic source metadata changes only the fallback discriminator and is not private
input/provenance evidence. The [session guarantee and reproduction](../application/session-and-programs.md#selected-rules-during-map-transfer)
own scope and command selection. Existing queued-warp and return behavior remains covered by the
scoped engine suite. No shipped default-composition regression or new original-game claim follows.

### Conditional design work, not present chapter-hardcoding defects

| Boundary | Confirmed current behavior and concrete expansion trigger | Smallest proposed response and acceptance |
| --- | --- | --- |
| Standalone terminal outcome is coupled to poison | [AuthoredScenarioPackageReader:291][authored-reader] reads standalone `outcome` only when `battlePoison` is present. [GameSession.Start:50][session] performs its initial outcome pass only for Poison; [BattleOutcome.Begin:21][outcome] publishes standalone `BattleEnded` only for Poison. Removing poison is therefore not an admitted way to retain the same terminal battle capability. The [poison owner](../domain/gameplay-rules.md#authored-poison-after-turn) explicitly scoped this subset. | When non-poison standalone terminal battles are requested, bind terminal-vs-world-return behavior directly to the encounter/session definition and reuse the outcome checker. Keep poison as after-turn capability. Accept initial dead-leader and ordinary victory/defeat with and without poison, no return program for terminal mode, and unchanged connected outcomes. Moderate admission/lifecycle work; no fix or general outcome framework now. |
| Definition-copy risk F3 | [Prior F3](structure-and-quality-review.md#f3--p2-avoid-silent-defaults-when-reconstructing-definitions) traces the outcome reconstruction omitting later optional fields. Current connected Give/Discard/poison guards prevent the described loss. Removing those guards is the trigger; this is not proof of broken accepted standalone play. | At that integration, use narrow immutable replacement owned by `BattleDefinition` and behavior checks of the newly admitted combination through growth/outcome/re-entry. No repair here and no generic capability bag. |
| Replay convention consults private-definition presence | [Sf2StoryPolicy.EnterCompletedBattle:14][source-policy] intentionally distinguishes private source replay from authored one-shot completion. This is a profile convention inside a policy, not a program-name or chapter check. | Keep the documented convention now. If repeatable authored encounters become a requirement, make replay semantics an explicit route/policy input with compatibility defaults, separately from provenance. Test held completion/unlock and first/repeat hooks. Do not weaken private admission or confuse this prospective change with B2's omitted selected policy. |

## Representative traces and changed-content assessment

### Ordinary branches, calls and waits

[StoryPolicyReplacementTests:19][story-tests] inspects two externally different programs through
one runner: live flag40 chooses a first/repeat branch, nested calls record two return addresses,
text waits for acknowledgement, a timer blocks, then entity/flag effects execute and callers return.
[Reader.Link][reader] resolves even untaken branch/call targets. `ProgramRunner.Run` advances only
the selected target and retains the blocking PC; matching dispatcher completion releases it.
Its256-instruction budget yields rather than defining a legal story length or a named round.

**Confirmed classification:** runtime invariant plus authored story facts. Program IDs, flag IDs,
text, entities and ordering can change within admitted operations without C# changes. Moving a write
before versus after the wait intentionally changes which prefix survives failure. A new arbitrary
condition type or effect is new capability: it needs a finite typed operation, admission and actual
behavior assertions. No interpreter/DSL rewrite is justified by another conversation.

### Opening/chapter source operation

The ordinary Map3/castle story is prepared as data. The inspected
[preparer's MoveEntityOutOfMap lowering:539][lowering] emits ordinary `hide` except for the exact
`map03/mapsetups/s6_initfunction.asm`, `byte_513A8`, entity142 seam, where it emits
`retired-map3-entity-scratch`. [Reader.ReadInstruction][reader] maps that opcode to a typed
`RetiredMap3EntityScratch`; the runner delegates the source family to the selected policy.

[Sf2StoryPolicy.Apply:16][source-policy] checks `byte-513a8:1`, `map-3`, map-loaded/outcome-map-loaded
continuation, closed text, F603, and actual live alias or the accepted removed record with F1 and
X/Y `0x7000`. [SourceStoryRules.Apply][source-port] permits only no change or the exact hide of
an existing physical record. It cannot supply a PC, wait, party, RNG or publication.
[BattleOutcomeProgramTests:255][outcome-tests] varies the map, cursor, window, flags, physical
record and tombstone; the alias test at314 prevents re-resolving a different alias.

**Confirmed classification:** source compatibility guard, intentionally not reusable chapter logic.
Renaming its map/PC or applying it to another NPC should reject. Relaxing those checks would extend
an evidence-limited inactive-scratch abstraction beyond its justification. Normal hide/motion and
conditional programs remain the route for other content. The
[opening owner](../application/session-and-programs.md#original-map-3-opening) and
[source-policy owner](../application/session-and-programs.md#source-story-policy) retain original
provenance and reach limits. This audit establishes the implementation routing, not new original
observations or a general native-call implementation.

### Battle entry, outcome and return

Map data supplies ordered routes with encounter, unlocked/completed/intro flags, before/load/start
and outcome programs. [BattleEntry.Continue/Select][entry] chooses the first eligible route,
runs the configured hooks, rebuilds active party membership where initialized, initializes battle
before publishing, then runs load/start. [BattleOutcome][outcome] asks selected outcome/return rules,
carries battle resources into exploration, executes the after/defeat program, then commits the
join/flag/recovery tail and runs the configured return program. Victory alone is not usable return.

**Confirmed classification:** reusable sequencing mixed with explicit SF2 services. Flags88
(suspended entry unsupported),399 (battle context), and90–105 (region reset) are fixed source
services on initialized paths, not encounter-number tests. Outcome policy's399/64/640 egress checks,
half-gold defeat and special dead-member7/28 recovery are SF2 policy, not universal runtime laws.
The existing [outcome owner](../domain/gameplay-rules.md#implemented-progression-and-outcome-policies)
and [battle admission owner](../application/session-and-programs.md#battle01-admission-and-first-input)
bound their meaning. Nonzero unsupported return status fails explicitly.

Another admitted encounter can change route/map/flag/program data; B1 constrains arbitrary field
player continuity and B2 constrains replacement-policy consistency. New defeat families, non-no-op
defeated programs, suspended battles or new effects need capability work. Renumbering all flags in
an initialized SF2 world is not merely changing a chapter's story flags: it changes the selected
source service's reserved state encoding. Keeping that fixed is acceptable under the current scope.

### Jaro conversion and victory recruitment

[BattleRoundContinuation.Begin:23][round] selects an ordered region-program row from live region
flags and its configured played flag, commits that flag, and runs the shared program before spawn
and turn generation. It contains no Battle32 or Jaro23 dispatch. In battle,
[BattleStoryOperations.Convert/JoinAi][battle-story] consumes explicit enemy/ally source slots,
retains two logical actors, removes the enemy, positions/joins the ally and copies the physical
visual alias in source order. `PrefixFailure` retains earlier writes if the later alias operation
fails. These are finite SF2 operations, not a generic faction switch.

After victory, [jaro-victory.json][jaro] authors join23, text, ten ticks, clear AI, text, leave23 and
another wait. The outcome route owns432/532 and return; common policy skips the ceremony when
completed. [OutcomePartyOperations.Apply][party] accepts a member operand, retained ally resources,
victory continuation and source30/0/32/12 layout. It clears AI bit2 or active32+member without
rebuilding the cached lists; the later common join rebuilds them. [JaroVictoryProgramTests:130][jaro-tests]
changes member23 to5 and the associated source/flag bindings and asserts reserve5 with unchanged
other activation bits. It also examines retained prefixes/retries after join, clear and leave.

**Confirmed classification:** ceremony/order/character operand in content; SF2 slot/layout and
write chronology in a finite typed operation. Different retained characters and dialogue ordering
within that context are data edits. Calling leave in arbitrary field input, changing the roster
layout, inventing raw post-battle X/Y state or implementing actual Battle32/Moun is new capability.
Keep these guards; do not hardcode a new character ceremony in `BattleOutcome` or make every party
write an unconstrained script mutation. The public example does not prove original media/timing.

### Taros, Prism, Fairy Woods and Caravan

| Family and caller trace | Classification and changed-content result | Keep or change |
| --- | --- | --- |
| [TarosEffectiveness.PrepareInitial][taros] → `Sf2PhysicalAction.Prepare` → strike/scene/reward continuation; [reader][taros-reader] binds the Bowie actor, enemy definitions and weapon types. | Definition88 and item61 are real identities in the selected Taros rule. Logical actor names, deployment order and map/package names do not select it. [Tests][taros-tests] vary actor, slot/name, current equipped weapon, double/counter and later failure. Different names/bindings work; a different boss weakness algorithm is not a new Taros chapter. | Keep the named SF2 rule. For a separately requested different weakness, select a narrow rule/binding with its own semantics; do not expose all numeric constants as arbitrary script knobs. Source provenance belongs to the [Taros owner](../domain/gameplay-rules.md#finite-taros-physical-effectiveness). |
| [PrismLaserDecision/PrismLaser][prism] → automatic action → ordinary scene/death continuation. | Direction and explicit source slot are data, independent of display facing/name/order. Construction recomputes current both-side targets and consumes its own RNG; decision focus is not an immutable target list. Source ranges and status/activation restrictions are explicit finite admission. [Tests][prism-tests] examine live and empty shots, ordered deaths and accounting retry. | Keep the algorithm and explicit Unsupported paths. Different positions/directions/actor IDs are data. Non15 activation, ambiguous overlap, new statuses or private Battle36 need implementation/admission work, not a script that bypasses the ray rule. |
| [BattleSpawnRules.Populate][spawn] → [round continuation][round] → actual appearance completion → queue generation. | Explicit source slots, spawn modes, original placement and regions drive reset/reappearance. No Fairy Woods/Battle44 name selects it. The region-program prefix runs first. [HiddenSpawnSessionTests][spawn-tests] examine later reappearance, walking retry and failure after delivered appearances. | Keep reset-before-delivery and delayed queue generation. Changed region/placement/played flags are data within the finite encoding. Fairy trial score/result/session-best and real Battle44 are separate missing capabilities, not evidence that spawn should be scripted per enemy. |
| [MapEventDispatcher.Move:30][events] → reserved boarding/landing programs built by [CaravanPrograms][caravan-programs] → [CaravanRuntime][caravan] → sprite/entity waits → committed travel mode. | Boarding requires source flag65 and terrain marker; source follower flag rows and physical pointers remain compiled. Logical follower, current physical list, player appearance and map geometry are data. [CaravanContentReader.Bind][caravan-reader] admits one authored map and rejects transfer/battle/population combinations; generated IDs are reserved. [CaravanTests][caravan-tests] changes logical names, raw mappings/lists and follower flags. | Keep the finite SF2 vehicle service. The shrink/grow sequence is choreography in C#, but reused source choreography, not a route itinerary. A new acquisition story can use ordinary flags/programs; actual cross-map Caravan integration remains unsupported. Different follower flag tables or choreography require scoped work: first parameterize the concrete table/sequence needed, retaining existing typed operations and physical-slot semantics, not a general vehicle framework. |

The Caravan distinction is especially important for planning. Existing authoring cannot replace its
generated boarding program just by supplying the same ID: admission rejects that. If a future
product requirement makes the sequence authorable, move the actual ordered program into validated
content with explicit bindings and keep mode commit/readiness checks in the runtime. Until then,
its compile requirement is an intentional fixed-service limitation, not a hidden promise of data-only
vehicle customization. Preserve the [Caravan boundary](../application/session-and-programs.md#caravan-boarding-travel-and-landing).

## Decision order and acceptance boundaries

1. **B1's entry/return binding correction is implemented.** Preserve its named-player behavior,
   source-population rule and [bounded acceptance](#b1--implemented-retain-field-player-binding-across-battle).
2. **B2's selected-rule transfer correction is implemented.** Preserve the selected composition at
   early initialized admission and its [bounded guarantee](../application/session-and-programs.md#selected-rules-during-map-transfer).
3. **At the affected capability expansion, address conditional debt.** Separate terminal outcome
   from poison only when broadening terminal battles; resolve F3 before enabling connected fields
   that the copy currently drops. These concerns have different triggers and should not be bundled
   into a broad engine rewrite.
4. **Keep legitimate source rules and finite exclusions.** New chapters reuse operations but still
   need content linking, source import/provenance, required assets and capability coverage. If a new
   mechanic is missing, implement the smallest typed rule/operation at its actual authority boundary.

All proposed changes must preserve one `GameSession`, immutable admitted definitions, deterministic
RNG, real wait completion, stale-input rejection and accepted-prefix failure semantics. Preparation
failure publishes no speculative state; failure after committed instructions cannot rewind the
chapter or replay earlier payment/join effects. New behavior acceptance should vary valid content
and history, not assert package names or mirror implementation structure. Reuse affected comparisons
only where their claim actually changes. No generic event bus, plugin VM, new DSL, global portability
goal, new test infrastructure or automatic refactor queue follows from this audit.

## Reproduction, checks and limits

From an owning repository checkout, inspect the pinned objects rather than a floating branch:

```powershell
git show b4882a0dddd045b44c9110e43b097efd9358d652:remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs
git show b4882a0dddd045b44c9110e43b097efd9358d652:remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs
git show b4882a0dddd045b44c9110e43b097efd9358d652:remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs
```

Follow the symbols and pinned caller/data links above. B1's variant changes only public authored
content before admission; B2 requires the explicitly stated initialized state and selected compiled
policy. The implementation dispositions above identify the subsequent bounded runtime reproductions.
Original-game facts remain with
their existing research/contracts and pinned upstream provenance; this report does not derive them
from remake behavior.

Documentation acceptance is exact two-path scope, `git diff --check`, local link/anchor and pinned
source-path/line checks, balanced fences and private-boundary inspection. On the clean committed
candidate, load the existing worktree environment and run
`uv run sf2 verify plan --base origin/main --head HEAD`; inspect actual docs-scoped CI separately.
Candidate/head/tree, plan and CI outcomes belong in the PR handoff. No engine/research/native suite
is warranted solely by this document. Previously completed failures remain preserved.

Coverage is responsibility-led, not a whole-engine certificate: runner/dispatcher, battle entry/
outcome/return, party operations, selected policies/readers, the six-family paths above, public
examples/assertions and ordinary host composition. Not exhaustively inspected: all opcodes, all
source importers, all malformed inputs, arbitrary rule compositions, save/load, later chapters,
performance, full private media or all presenter branches. No private payload was needed. Actual
changed-content executions, original natural reach/timing and effects outside the admitted profiles
remain **Unknown** here. Independent main-gate review must trace B1/B2 and the no-action
counterexamples before accepting this design artifact; implementation requires its own scope.

[session]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Runtime/GameSession.cs#L22
[runner]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs#L23
[runner-transfer]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs#L464
[outcome]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs#L17
[outcome-policy]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs#L9
[source-policy]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2StoryPolicy.cs#L8
[source-port]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Runtime/Exploration/ISourceStoryPolicy.cs#L10
[battle-story]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Runtime/Battles/BattleStoryOperations.cs#L9
[party]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Runtime/Exploration/OutcomePartyOperations.cs#L10
[reader]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs#L186
[authored-reader]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Content/Scenarios/AuthoredScenarioPackageReader.cs#L291
[private-reader]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Content/Scenarios/PrivateExplorationReader.cs#L12
[dispatcher]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationDispatcher.cs#L12
[transfer]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs#L185
[transfer-apply]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs#L260
[entry]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs#L9
[entities]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs#L29
[host]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/game/src/GameRoot.cs#L93
[progression]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/content/authored/progression-outcome-victory.json#L1164
[jaro]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/content/authored/jaro-victory.json#L476
[story-a]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/content/authored/story-rule-demo-a.json
[story-b]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/content/authored/story-rule-demo-b.json
[story-tests]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/tests/Sf2.Remake.Engine.Tests/StoryPolicyReplacementTests.cs#L19
[outcome-tests]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/tests/Sf2.Remake.Engine.Tests/BattleOutcomeProgramTests.cs#L255
[jaro-tests]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/tests/Sf2.Remake.Engine.Tests/JaroVictoryProgramTests.cs#L130
[lowering]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/src/sf2tool/remake_exploration_programs.py#L539
[round]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Runtime/Battles/BattleRoundContinuation.cs#L23
[taros]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Domain/Gameplay/Sf2/TarosEffectiveness.cs#L25
[taros-reader]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Content/Scenarios/TarosEffectivenessReader.cs#L26
[taros-tests]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/tests/Sf2.Remake.Engine.Tests/TarosEffectivenessTests.cs#L25
[prism]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Domain/Gameplay/Sf2/PrismLaser.cs#L37
[prism-tests]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/tests/Sf2.Remake.Engine.Tests/PrismLaserSessionTests.cs#L16
[spawn]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Domain/Battles/Rules/BattleSpawnRules.cs#L31
[spawn-tests]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/tests/Sf2.Remake.Engine.Tests/HiddenSpawnSessionTests.cs#L19
[events]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs#L10
[caravan-programs]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Runtime/Exploration/CaravanPrograms.cs#L9
[caravan]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Application/Runtime/Exploration/CaravanRuntime.cs#L66
[caravan-reader]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/src/Sf2.Remake.Content/Scenarios/CaravanContentReader.cs#L43
[caravan-tests]: https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/b4882a0dddd045b44c9110e43b097efd9358d652/remake/tests/Sf2.Remake.Engine.Tests/CaravanTests.cs#L107
