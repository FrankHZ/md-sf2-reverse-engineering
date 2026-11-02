# Controlled-start display audit

## Decision and scope

**Standard rigor:** [#1437](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1437)
resolves the six downstream failures with existing typed commands/receipts, source-ordered counted
membership and actual fade lifecycle coverage. The accepted diagnostic initialization from
[#1435](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1435) remains its dependency.
Scope includes the three private behavior files, focused JoinMessage behavior assertions,
`ProgramRunner`'s field-return publication and their owning documentation. Main-gate authorized
that narrow production boundary after the source-chronology finding. No golden, fixture, source
expectation or private input bytes change. The [current result](#downstream-resolution) is
**9 pass/0 fail/0 skip**; the earlier failures below remain discovery evidence.

The preceding repair corrected the diagnostic display declarations and selected palette dependencies investigated by
[#1433](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1433). The dependency crosses
Content admission, Application startup and transfer; its scope was controlled inputs and their existing
test wiring, with all original behavior/comparison assertions retained. That repair left production,
schemas and goldens unchanged. No aggregate rerun, emulator acquisition, Godot launch, screenshots,
asset export or audio work is required. Its nine corrected-input executions yielded **3 pass,
6 downstream fail, 0 skip**; the [repair result](#corrected-initialization-and-nine-node-results)
distinguishes those failures from the removed null-display defect.

The original investigation reproduced accepted `966728ebee075f208c3af7cd4c8afff46c840adf`, tree
`a43d4393ed9bc16d6a4e284c83e0b4f2229ad145`. Original-source identities remain SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6` and USA ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This audit adds remake execution evidence, not a new original runtime observation.

**Confirmed (investigation base):** all nine historical failures stop at the first ordinary warp because the selected
diagnostic session has no `Story.Display`. There are two missing initialization owners: the R1
checkpoint omits display state; the Map21 helpers construct another start without display state.
The historical world also omits map palettes, a separate content dependency masked by the first
failure. A palette-complete world still leaves the diagnostic display null.

**Disposition:** the bounded repair below supplies the missing declarations and selects existing
palette-complete content. No production fade change or assertion retirement is justified here.
Historical FAIL records remain failures; only actual corrected-input results are reported anew.

## Invariant and failure boundary

[`MapTransfer.ValidateDisplay`](../../src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)
requires a positive byte period, valid BASE/CURRENT logical palette pairs, a defined settled
visibility, black/white CURRENT for those visibility states, and CURRENT equal to BASE otherwise.
`Transitioning` is invalid at this admission boundary; an already running fade has its own typed
wait. These pairs represent palette 1 colors 2 and 3, not the full rendered frame.

An ordinary Move previews its warp in
[`ExplorationDispatcher.Move`](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationDispatcher.cs).
`MapTransfer.Validate` first previews destination construction, then validates the live display
and Rebuild palette. Both observed first warps reach this validation before `BeginWarp` or
`BeginFade`. The failure is `UnsupportedCapability`, code `full-fade-state`, field `story.display`.
No movement/warp observation is emitted and the same exploration world/position remains; a new
snapshot records `StopReason.Unsupported`. This is not a failed palette interpolation or audio fade.

[`ExplorationContentReader`](../../src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)
keeps map `basePalette` and optional `start.display` separate. An embedded map palette or
`start.mapPalettes` normalizes into map definitions; neither initializes live display state.
[`ExplorationDispatcher.Start`](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationDispatcher.cs)
validates a supplied display and passes it into StoryState, but permits null for entries that do
not use these operations. It deliberately invents no period, CURRENT or visibility.
Application owns the invariant and live evolution; Content/the explicit start owns initialization.
The [existing display contract](../verification/field-programs.md#ordinary-warp-visible-return)
and [source proof](../../../docs/research/chapters/grans-opening/map3-controlled-start-egress-transition.md#selected-r1-period-and-initial-display-binding)
retain that boundary.

## Investigated input and start chains

The retained `local/issue1045/inputs.ps1` selections resolve all six required encounter/static inputs
and the world successfully. `PrivateBattleScenarioTests.Selected(controlled)` explicitly replaces
the environment's `SF2_PRIVATE_CONTROLLED_START` for these nine nodes. Changing that environment
variable alone cannot repair them. The Engine.Tests project copies the tracked starts into its
`controlled/` output; there is no hidden alternate checkpoint.

| Chain | Actual declared state and owner | First failing input |
| --- | --- | --- |
| R1 | [`map3-opening-start.json`](../../reference/inputs/map3-opening-start.json), Map3 `(56,3)`, Down, speed32, F0/F32, retained walking phases; [`map3-opening-party.json`](../../reference/inputs/map3-opening-party.json). `StartSource` → `RunOpening` in [`PrivateExplorationTests`](../../tests/Sf2.Remake.Engine.Tests/PrivateExplorationTests.cs). No display or mapPalettes is declared. | R2 logicalInputTrace input 2: Left from Map3 `(55,3)` previews the Preserve warp at `(54,3)` to Map3 `(3,3)`. Failure precedes any Yes/No answer. |
| Bridge | [`map40-intro-start.json`](../../reference/inputs/map40-intro-start.json) is read with [`battle01-player-ready.json`](../../reference/inputs/battle01-player-ready.json); `Bridge` in [`PrivateBattleEntryProgramTests`](../../tests/Sf2.Remake.Engine.Tests/PrivateBattleEntryProgramTests.cs) replaces it with Map21 `(5,15)`, Down, speed32, F0/F1/F2/F32/F33/F34/F401. Its new `ExplorationStartInput` omits display. | player-ready inputPlan input 18: Up from Map21 `(9,2)` previews the Rebuild warp at `(9,1)` to Map40 `(4,30)`. Before-battle code has not started. |
| Membership | Same `Bridge(seen:true, sarah:false)`, flags F0/F2/F32/F34/F401/F451. The pure `BattleTurnFlow.Start` variations execute before `Traverse`; they do not install session display. | Same Map21 input 18, before the session reaches battle. |
| Defeat seam | `Read` in [`PrivateBattleOutcomeProgramTests`](../../tests/Sf2.Remake.Engine.Tests/PrivateBattleOutcomeProgramTests.cs) reads Map40 with opening-party; `StartBattle` constructs Map21 `(5,15)`, Down, speed32, gold123 and F0/F1/F2/F32/F33/F34/F401/F600–605/F607/F608. It also omits display. | Same Map21 input 18 inside `StartBattle` → `Traverse`, before `Play(lose:true)`. |

The baseline world `private-map3-programs` is 103,238,115 bytes, profile
`private-local-controlled-start`. Its six maps (3/19/20/21/40/57) all lack `basePalette`.
The existing `world-services-issue1424.json` is 125,828,417 bytes; its seven retained maps all have
palettes. Both were read in place. The four chains above were observed with both selections:
display remains null, the same input fails, and only map-palette availability changes.

**Confirmed (code):** Map21 helpers omit display even if their upstream `Read()` later supplies one.
Copying a Map40 display would also bind the wrong map's BASE. A Map21 start needs its own binding.
**Inferred next blocker:** adding display alone against the old palette-less world cannot complete
the Rebuild path: `MapTransfer.Apply` requires the target palette when display is present, and
`Validate` independently requires it for Rebuild. This was reviewed directly, not demonstrated by
injecting an invented display state. Preserve warps still need the initial live BASE/CURRENT pair.

## Historical nine-node disposition

All names below are under `Sf2.Remake.Engine.Tests`. Every historical stack in
`local/issue1421/engine-final-01.log` names `full-fade-state`; the nine-node inventory remains
`local/issue1423/inherited-failures.txt`. Stack inspection and the exact common caller establish
each attribution; current individual reproduction is distinguished from a shared-prefix finding.

| Node | Historical failing caller / chain | Current status and decision |
| --- | --- | --- |
| `PrivateBattleOutcomeProgramTests.ActualCommandsCompleteTheWholeVictoryProgramFlagsAndUsableReturn` | `ContinuousBattle` → `RunOpening`, R1 | Not individually rerun. Confirmed shared R1 prefix; victory/reward/return assertions never reached. Diagnostic checkpoint/content correction; retain assertions. |
| `PrivateBattleEntryProgramTests.ContinuousOpeningCastleTowerAndBeforeBattleRetainTheLivePartyAndReachActualBattleInput` | `ContinuousBattle` → `RunOpening`, R1 | Not individually rerun. Confirmed same prefix, before castle navigation or battle admission. Same correction; retain live-party assertions. |
| `PrivateBattleEntryProgramTests.ExplicitH3BridgeRunsTheFullBeforeBodyAndMatchesPlayerReadyWithoutClaimingR1Continuity` | `Traverse` → `Input`, Bridge | Individually reproduced FAIL; confirmed null display and first-warp preflight. Correct Map21 diagnostic construction/content; retain H3 comparison and its no-natural-continuity boundary. |
| `PrivateBattleOutcomeProgramTests.ContinuousOpeningDefeatKeepsOneSessionThroughTheWholeProgramAndReturn` | `ContinuousBattle` → `RunOpening`, R1 | Not individually rerun. Confirmed same prefix, before battle or defeat. Same correction; retain one-session defeat/return assertions. |
| `PrivateExplorationTests.RefusalPreservesUnjoinedStateAndARealSarahInteractionCanLaterJoinTheParty` | `RunOpening(false)`, R1 | Not individually rerun. Confirmed same prefix before any answer; refusal cannot cause this failure. Same correction; retain refusal/retry membership assertions. |
| `PrivateBattleOutcomeProgramTests.OrdinaryLeaderLossKeepsItsOwnRecoveryAndRunsTheRealPostMessengerReload` | `StartBattle` → `Traverse` → `Input`, Defeat seam | Individually reproduced FAIL; confirmed null display before battle, not recovery/reload failure. Correct its separate Map21 constructor/content; retain defeat accounting/reload assertions. |
| `PrivateBattleEntryProgramTests.LiveActiveMembershipPacksFormationWithoutAddingSarahOrReplacingThePartyResources` | `Traverse` → `Input`, Membership | Individually reproduced FAIL; confirmed null display despite F451 and the omitted Sarah. Correct Bridge construction/content; retain changed-membership/resource assertions. |
| `PrivateExplorationTests.FullOriginalOpeningConsumesR1InputsAndMatchesR2AndR2aThroughStableFieldControl` | `RunOpening(true)`, R1 | Individually reproduced FAIL with baseline and palette-complete worlds. Confirmed checkpoint omission at input 2. Correct R1 declaration/content; retain R1/R2/R2a comparisons. |
| `PrivateExplorationTests.CastlePalaceAndAstralRunFromTheLiveOpeningWithFirstRepeatAndChoiceVariants` | `RunOpening` before castle fixture traversal, R1 | Not individually rerun. Confirmed same prefix, before palace/Astral/choice variants. Same correction; retain first/repeat/choice assertions. |

The common immediate cause is established for all nine, not merely inferred from their names.
**Unknown:** downstream results after correct initialization, including exact RNG/phase comparisons.
No nine-node PASS, original display-timing closure or obsolete-test retirement is claimed.

## Product impact

| Entry or transition | Dependency review and impact |
| --- | --- |
| Ordinary New Game | [`GameRoot.BeginNewGame`](../../game/src/GameRoot.cs) uses `OpeningNewGameReader`, not `StartSource`/Bridge. [`PrivateBattleContentReader.Initialize`](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleContentReader.cs) requires palettes on every retained map and explicitly constructs Map3 BASE=CURRENT, BaseRestored, period3. Missing palette rejects startup. It does not read either checkpoint. No shared missing-initialization defect found. |
| Continue | [`OpeningRestoreReader`](../../src/Sf2.Remake.Content/Persistence/OpeningRestoreReader.cs) admits immutable definitions without initializing a new party. [`OpeningRestore.Build`](../../src/Sf2.Remake.Application/Runtime/Persistence/OpeningRestore.cs) validates positive saved FadePeriod, requires the reconstructed map palette, and constructs BASE=CURRENT/BaseRestored. Save capture requires settled restored display. No helper-start dependency found. |
| Ordinary map transfer | Application retains the existing StoryState, fades its CURRENT, updates BASE only for admitted Rebuild, and returns through `ReturnVisible`. It does not construct a nullable diagnostic start. Both valid product entries supply its prerequisite; their accepted actual route exercised this consumer. |
| Battle entry and return | [`BattleEntry`](../../src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [`BattleOutcome`](../../src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs) and `StoryState.Copy` preserve display through battle/field switching; scene/return programs use the same palette guards. The nine failures precede these consumers. Accepted ordinary victory and defeat evidence remains applicable. |
| Explicit diagnostic host entry | `--private-exploration-start` uses the same nullable controlled reader. Selecting either incomplete checkpoint can expose the defect during diagnostic play, even with product world palettes. This is a real diagnostic-input failure; no Godot rendering result is inferred from the engine run. Public authored or arbitrary later starts are outside this audit. |

The [composed ordinary route](opening-loop-acceptance.md),
[Save/Continue and actual defeat/retry](../verification/host.md#opening-save-and-continue), and
[Church defeat/recovery](../verification/host.md#opening-church-services) already retain actual
native observations. No unresolved dependency in this review requires repeating those routes or
launching another host. This is a bounded impact assessment, not universal startup/restore parity.
[#1410 audio lifetime warnings](../verification/host.md#audio-resource-lifetime-observation) concern
different resources and remain outside the display investigation.

The diagnostic repair uses the existing reader and palette helper. It retains the independent
R1 proof, the separate Map21 evidence boundary and an explicit palette-complete content selection.
There is no default display in `GameSession`, relaxed `ValidateDisplay`, Map40 BASE copied into
Map21, skipped node, production-code owner, new schema or startup framework. The following results
retain the investigation's failures and original assertions.

## Corrected initialization and nine-node results

The repair consumes accepted `614185ba9f92bec6aad0aecfd4dfbc40454bfdba`. R1 now declares period3,
BASE=CURRENT `(3716,36)` and `base-restored`, using the independent selected-R1 proof above;
its party, flags, seeds and complete walking phases are unchanged.
[`map21-battle-bridge-start.json`](../../reference/inputs/map21-battle-bridge-start.json)
owns the separate Map21 `(5,15)`, Down, speed32 display binding. Bridge and StartBattle read this
declaration and explicitly retain its display when constructing their different flags/party inputs.

**Confirmed (pinned source/ROM):** Map21's own `00-tilesets.asm` selects palette0, whose words
at offsets4/6 are `(3716,36)`. This is not Map40's pair `(3204,34)`. Reproduction uses the existing
`sf2tool.remake_exploration_content.selected_map_palette_bindings` with the verified pinned
`OriginalPrograms` compiler, map IDs `[3,19,20,21,40,57]` and the identity-verified registered ROM.
The existing helper checks ROM SHA-256 and the map-palette pointer; no asset export is performed.

| Map IDs | BASE colors 2 / 3 (decimal) |
| --- | --- |
| 3, 19, 20, 21 | 3716 / 36 |
| 40 | 3204 / 34 |
| 57 | 3240 / 34 |

**Unknown:** original period/CURRENT/visibility at the separately controlled Map21 bridge.
The player-ready fixture's bridge/deterministic state does not retain them;
`tools/bizhawk/map3_messenger_acceptance_observer.lua:seed_extension_terminal` changes flags,
guard placement, RNG and counters but supplies no display observation. R1 evidence is not extended
to this seam. The Map21 declaration explicitly labels period3 and restored CURRENT/visibility as
**remake-only controls**. Its original-comparison claim stays the existing selected H3 battle
endpoint, not natural continuity or original display/interrupt timing. On actual execution, the
unchanged ready MainSeed, thinking-seed high word, selected actor, turn order, region and flags
assertions pass before the later instruction-coverage failure. This confirms that endpoint subset
under the declared control; it does not prove the Unknown original display values or the whole node.

### Selected content and compatibility boundary

The nine-node commands explicitly select retained `world-services-issue1424.json` after loading
`local/issue1045/inputs.ps1`. Global `accepted-inputs.json`, old worlds and environment defaults are
untouched. Both starts omit `mapPalettes`; embedded palettes are the single binding authority.
The old six-map world still admits the corrected R1 Preserve prefix, but the corrected Map21
prefixes reject input18 with `warp-palette-binding`. It is not a sufficient Rebuild input.

Direct JSON comparison establishes equal layouts, areas, default entities/population, setup
selectors, onLoad and battle routes for the shared six maps, and unchanged `bbcs-01`,
`abcs-battle01` and messenger `cs-51580`. Text/font/member-name/party-flag definitions are equal.
The newer world also adds Battle02/Map66, search and setup variants, Church and supported source
actions; Map3's former x=0 frontier is an admitted warp. None of these commands uses that exit,
Search, Church or later setup flags. `cs-51614`/`cs-51406` lower text447 directly rather than with
a preceding text-cursor write; join/follower/flag operands are unchanged. Reached original
opening text/position/flag assertions still match. This is a reviewed, bounded selection, not an
assertion that the two worlds are interchangeable for all consumers or unexecuted suffixes.

The extra encounter changes the reader's party input to carried allies, including ally3;
per-encounter enemies live in existing `EncounterStarts`. The membership test's four pure battle
starts now select the current encounter's deployments, preferring carried actors and filling from
that encounter's admitted defaults, as production BattleEntry does. Its live session party is
untouched; all original formation/resources/seed/dead-member assertions remain and pass.

The existing `RunUntilStop` driver also now sends `CompletePresentation` for a logically completed
`FullFadeWait`, reusing the typed receipt pattern already used by other engine tests. Its initial
omission was exposed as two `fade-awaiting-presentation` failures, retained in
`local/issue1435/representative.{log,trx}`; no production invariant was changed.

The retained prefix observer from #1433 was adapted only for the corrected declarations/display,
existing typed driver and stopping after the first warp. With the selected world all four chains
return to PlayerInput: R1 Preserve input2 retains BASE=CURRENT `(3716,36)`; Bridge, Membership and
Defeat Rebuild input18 each reach Map40 with BASE=CURRENT `(3204,34)`. All retain period3 and
BaseRestored. `prefix-product.json` and `prefix-baseline.json` distinguish this corrected execution
from #1433's null-display reproduction. They are local engine state observations, not H3 evidence.

### Actual result matrix

This is the historical #1435 corrected-input baseline. Names below retain namespace
`Sf2.Remake.Engine.Tests`. Every row ran with required private inputs; that run claimed no
missing-input skip, expectation change or nine-node PASS. See the [current resolution](#downstream-resolution).

| Node | Actual corrected-input result / next owner |
| --- | --- |
| `PrivateExplorationTests.FullOriginalOpeningConsumesR1InputsAndMatchesR2AndR2aThroughStableFieldControl` | **FAIL**, after R1/R2/messenger texts, positions, flags and joined list match: Active expects `[0,1]`, actual `[0,1,2]`. Main-gate owns assessment of this comparison versus `MapPartyMembership.Rebuild` and flags2/34 already asserted by the same node. No expectation changed. |
| `PrivateBattleEntryProgramTests.ExplicitH3BridgeRunsTheFullBeforeBodyAndMatchesPlayerReadyWithoutClaimingR1Continuity` | **FAIL**, after ready seed/order/actor/flags match: full instruction-range assertion lacks instruction2 (FadeOut). The selected before program is unchanged between worlds. Main-gate owns assessment of entry fade routing/observation coverage versus this assertion. Remaining assertions unexecuted. |
| `PrivateExplorationTests.RefusalPreservesUnjoinedStateAndARealSarahInteractionCanLaterJoinTheParty` | **PASS**, including refusal, real retry interaction, join/follower and final control assertions. |
| `PrivateBattleEntryProgramTests.LiveActiveMembershipPacksFormationWithoutAddingSarahOrReplacingThePartyResources` | **PASS** after the per-encounter input binding correction; earlier `missing-actor` failure retained. |
| `PrivateBattleOutcomeProgramTests.OrdinaryLeaderLossKeepsItsOwnRecoveryAndRunsTheRealPostMessengerReload` | **PASS**, including actual defeat, recovery/gold, real reload program and usable return. |
| `PrivateBattleEntryProgramTests.ContinuousOpeningCastleTowerAndBeforeBattleRetainTheLivePartyAndReachActualBattleInput` | **FAIL**, `explicit-text-wait-required` in `RunUntilStop` through `ContinuousBattle`'s Astral interaction. Main-gate owns the separate typed-text driver boundary; final battle assertions unexecuted. |
| `PrivateBattleOutcomeProgramTests.ActualCommandsCompleteTheWholeVictoryProgramFlagsAndUsableReturn` | **FAIL**, same continuous Astral typed-text boundary before Play; victory/reward/return assertions unexecuted. Same owner. |
| `PrivateBattleOutcomeProgramTests.ContinuousOpeningDefeatKeepsOneSessionThroughTheWholeProgramAndReturn` | **FAIL**, same continuous Astral typed-text boundary before Play; defeat/return assertions unexecuted. Same owner. |
| `PrivateExplorationTests.CastlePalaceAndAstralRunFromTheLiveOpeningWithFirstRepeatAndChoiceVariants` | **FAIL**, `explicit-text-wait-required` at its actual Astral interaction. Same typed-text owner; remaining repeat/choice assertions unexecuted. |

Combined #1435 per-node result: **3 pass / 6 fail / 0 skip**. All nine missing-display paths were
repaired; the six downstream failures were handed to main-gate and #1437 for the bounded resolution
below. No full-route victory/defeat parity or all-world compatibility follows from initialization alone.

Run the exact FullyQualifiedNames above with the existing project command below, selecting only
the intended rows. In this slice the initial two representative nodes ran first, then those two
reran after the fade-receipt correction (`representative-receipt.trx`: 0/2/0); the other seven ran
once (`remaining-seven.trx`: 2/5/0). Only membership reran after its input-binding correction
(`membership-binding.trx`: 1/0/0). The other eight results remain valid for the final diff.
The existing `SelectedR1WalkingPhasesResumeBeforeTheFirstAutonomousDraw` behavior node also passes
on the corrected R1 declaration (`r1-phase-regression.trx`: 1/0/0), preserving its exact phase/RNG assertions.

```powershell
. ./local/issue1045/inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$env:SF2_REQUIRE_PRIVATE_TESTS = '1'
$env:SF2_PRIVATE_EXPLORATION_CONTENT = (Resolve-Path -LiteralPath 'local/issue1045/world-services-issue1424.json').Path
# $filter uses FullyQualifiedName=<namespace + exact matrix node>, joined with | for a bounded group.
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
  --configuration Release --no-restore --filter $filter `
  --logger 'trx;LogFileName=selected-nodes.trx' --results-directory local/issue1435
```

Configured DOTNET_BIN and DOTNET_CLI_HOME are absolute; SDK10.0.204, shared installations and
worktree-local caches/TEMP are reused. Compact results remain in `local/issue1435`, with no new
world, environment, SDK, save slot or native instance. The historical completed aggregate
**1692 pass / 20 fail / 1 skip** and all five #1433 failure reproductions remain unchanged.

## Downstream resolution

**Confirmed (engine execution):** the final nine-node selection completes **9 pass/0 fail/0 skip**
in `local/issue1437/final-nine.trx`. The matrix above retains #1435's **3/6/0** baseline; its six
FAIL rows are now resolved, not erased. Every exact node in that matrix was rerun because the
shared field-return publication and diagnostic reader/driver affect their dependencies.

| Former downstream failure | Final result and retained assertion |
| --- | --- |
| R1/R2/R2a full opening | **PASS**; original Active `[0,1]`, joined list, flags, texts, speakers, guards, followers and stable movement assertions unchanged. |
| Explicit H3 bridge | **PASS**; ready RNG/high thinking word, selected actor, queue, region and flags unchanged; every before-program instruction still required. |
| Continuous castle/tower/battle input | **PASS**; same session, live formation/resources, before-program and actual first battle input. |
| Actual-command victory | **PASS**; actual battle/growth, every after-program instruction, ordered join/flags/return, carried progression and usable field movement. |
| Continuous defeat | **PASS**; same-session actual defeat, whole return program and usable field movement. |
| Castle/palace/Astral variants | **PASS**; both initial follower cases, refusal/acceptance, repeat text579 and terminal control. |

The three prior passing rows (refusal/retry, changed membership, ordinary defeat/reload) also
**PASS** in the same final run. This is an engine diagnostic result; the Map21 display controls
remain remake-only and original display/timing and natural continuity remain **Unknown**.

### Counted membership versus gameplay membership

**Confirmed (pinned source):** `battleparty.asm:JoinForce` (`0x9956`) sets the joined flag,
calls `UpdateForce`, then conditionally calls `JoinBattleParty`; it does not rebuild after the
active write. `mapscriptengine_2.asm:csc08_joinForce` calls Sarah1 then Chester2 for selector128.
The resulting counted Active prefix is `[0,1]`, although flags32/33/34 are all set. The last
`{LEADER}` in `cs_51614` is text536 **before** those joins. Join text447 contains `{NAME;1}` and
`{NAME;2}`, not `{LEADER}`. The remaining follower/guard commands, `ExecuteMapScript:loc_47234`,
`Map3_ZoneEvent8`'s F603 write, `RunMapSetupZoneEvent`,
`ProcessMapEventType6_ZoneEvent` and `WaitForEvent` return path contain no later `UpdateForce`.
All paths are beneath the already pinned SF2DISASM `disasm/` tree; no source or ROM input changed.
This is static source-order support for the retained assertion, not a new original RAM capture.

The incorrect surface was `ProgramRunner`'s ordinary field-control return, which derived gameplay
ActiveAllies from flags **and published that reconstruction as Story.PartyLists**. It now continues
to synchronize carried ActiveAllies but preserves the last actual counted-list state. `Join`,
explicit `{LEADER}` (`ExplorationTextRunner.Begin` / source `symbol_leader`) and battle entry keep
their own real rebuilds; restore remains separately reconstructed. No active flag or expected
R2a value changes. The existing two JoinMessage behavior cases now assert both the cached reserve
prefix and current gameplay ActiveAllies after actual return. Their pre-fix run fails both cache
assertions; the existing LEADER test passes before and after the correction.

### Typed waits, real fade receipts and encounter inputs

`RunUntilStop` advances mandatory FieldText work, supplies actual `CompleteTextReveal`, then
acknowledges only an available text-input boundary. W1 spans use the same existing commands.
Typed text recording reads each actual OpenTextWindow once, retaining speaker operands without
duplicating multi-span text IDs. No state mutation, synthesized input bypass or helper test is added.

**Confirmed (execution and code):** `ProgramRunner` routes black fades through
`MapTransfer.BeginFade` and `FullFadeWait`; that path emits `full-fade-started`, actual
`presentation-completed`, `fade-service` and `full-fade-completed`, without Program coordinates.
The old coordinate-only assertion therefore missed executed fades (first index2, then index10).
The before/after-program assertions now require every non-fade instruction's actual coordinate and
every fade's ordered request/delivery/logical-completion lifecycle **between its two adjacent
instruction coordinates**, including real service work. They neither exclude index2 nor append an
unobserved coordinate. All eight before-program and ten after-program fades are covered.

The selected world carries a later encounter's ally3 as well as the opening three. ContinuousBattle
now checks this encounter's actors and separately checks exact retained off-encounter allies;
it never changes the live party. The pure post-victory BattleTurnFlow comparison similarly selects
the encounter's deployments from carried actors and admitted defaults rather than treating a
travelling ally-only party as a complete enemy-inclusive battle start. Existing growth/resource
assertions remain. Opening diagnostics also pass the already configured
`SF2_PRIVATE_BATTLE_SCENE_CONTENT` to the existing PrivateBattleScenarioReader optional input;
its admitted healing content is required by the actual victory commands. No assets were generated,
copied or reselected globally, and production Content admission is unchanged.

### Verification and preserved discoveries

Reuse the earlier command with the exact nine FullyQualifiedNames, selected world, absolute SDK
selections and required-private flag; change only the results directory to `local/issue1437` and
logger filename to `final-nine.trx`. The local `run-nodes.ps1` records this invocation. Existing
`local/issue1045/inputs.ps1` supplies the scene selection; no implicit optional-input skip counts.

`focused-regression.trx` completes **283 pass/0 fail/0 skip** across Battle01ToBattle02Tests,
MapEntityLifecycleTests, ExplorationTextWaitTests, BattleEntryProgramTests, FieldPlayerReturnTests,
OpeningRestoreTests, OpeningSaveTests and JaroVictoryProgramTests. This checks counted chronology,
LEADER rebuilding, entry/retention and restore/save boundaries affected by the shared publication.
`uv run sf2 verify adapter` completes locked restore and actual adapter build with zero warnings/errors.
No complete engine/private aggregate, native launch, emulator or comparator is run.

Retained completed failures in `local/issue1437` include `typed-text-four` (0/4/0: missing typed
text recording and off-encounter actor assertion), `continuous-candidates` (compile failure:
incorrect Battle.Encounter property), `continuous-candidate-binding` (2/1/0: healing scene input),
`bridge-fade-coverage` (0/1/0: next FadeIn coordinate), `chronology-before-fix` (1/2/0: counted
prefix overwritten), `chronology-after-fix` (4/1/0: after-program fade coordinate) and
`victory-fade-coverage` (0/1/0: incomplete pure re-entry input). Each correction used a focused rerun;
final-nine was justified by the final shared dependency changes. Historical #1433/#1435 receipts and
the completed **1692/20/1** aggregate are untouched. Public CI and exact candidate identity belong
to the PR handoff; neither substitutes for these private results or broadens their fidelity claim.

## Investigation reproduction and retained results

All commands run from the owning repository root with existing inputs/installations/caches, after
loading `local/issue1045/inputs.ps1` in the same process and forcing
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. `DOTNET_BIN` and `DOTNET_CLI_HOME` remain configured absolute
selections; the SDK is pinned to 10.0.204. Set `SF2_REQUIRE_PRIVATE_TESTS=1`; no missing-input skip
can count as execution. Output stays under ignored `local/issue1433`.

```powershell
. ./local/issue1045/inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$env:SF2_REQUIRE_PRIVATE_TESTS = '1'
$r1 = 'Sf2.Remake.Engine.Tests.PrivateExplorationTests.FullOriginalOpeningConsumesR1InputsAndMatchesR2AndR2aThroughStableFieldControl'
$bridge = 'Sf2.Remake.Engine.Tests.PrivateBattleEntryProgramTests.ExplicitH3BridgeRunsTheFullBeforeBodyAndMatchesPlayerReadyWithoutClaimingR1Continuity'
$membership = 'Sf2.Remake.Engine.Tests.PrivateBattleEntryProgramTests.LiveActiveMembershipPacksFormationWithoutAddingSarahOrReplacingThePartyResources'
$defeat = 'Sf2.Remake.Engine.Tests.PrivateBattleOutcomeProgramTests.OrdinaryLeaderLossKeepsItsOwnRecoveryAndRunsTheRealPostMessengerReload'
$project = 'remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj'
& $env:DOTNET_BIN test $project --configuration Release --no-restore `
  --filter "FullyQualifiedName=$r1" --logger 'trx;LogFileName=map3-baseline.trx' --results-directory local/issue1433
& $env:DOTNET_BIN test $project --configuration Release --no-restore `
  --filter "FullyQualifiedName=$bridge|FullyQualifiedName=$membership|FullyQualifiedName=$defeat" `
  --logger 'trx;LogFileName=map21-baseline.trx' --results-directory local/issue1433
$env:SF2_PRIVATE_EXPLORATION_CONTENT = (Resolve-Path -LiteralPath 'local/issue1045/world-services-issue1424.json').Path
& $env:DOTNET_BIN test $project --configuration Release --no-restore `
  --filter "FullyQualifiedName=$r1" --logger 'trx;LogFileName=map3-product-world.trx' --results-directory local/issue1433
```

**Confirmed, completed results:** baseline R1 0 pass/1 fail/0 skip (949 ms); three Map21 nodes
0 pass/3 fail/0 skip (692 ms); palette-complete R1 0 pass/1 fail/0 skip (460 ms). Each test command
exited 1 with the same `full-fade-state` boundary. Logs/TRX retain exact stacks. These are successful
failure reproductions, not passing behavior tests. Only four distinct named nodes were rerun.

The retained scratch `observe-prefix.ps1` directly loads the same Release assemblies, uses actual
production readers and start constructors, sends the existing fixture's Move prefix through
`GameSession.Submit`, and reuses `RunUntilStop` only to deliver its existing typed waits. It sets no
session state and installs no display. `pwsh -NoProfile -File local/issue1433/observe-prefix.ps1`,
then the same command with `-ProductWorld`, both exit 0 and produce `prefix-baseline.json` and
`prefix-product.json`. Their four rows per selection establish the null display, first failure
ordinals/positions, palette distinction and unchanged exploration world described above. The
Membership prefix intentionally observes only the same Bridge start/first warp; its independent
formation assertions are covered by the named node reproduction, not duplicated in the observer.
`input-summary.py` records existence, size, identity and omitted/present keys without copying assets.

The old aggregate remains **completed 1692 pass/20 fail/1 skip**, with its exhaustive dispositions
and subsequent narrow results in `local/issue1421`–`local/issue1425`. Neither it nor a full/private
profile was rerun. New compact outputs occupy under 0.1 MB, with about 3.09 GB free on the execution
drive. No new environment, SDK, asset copy, native instance or save slot was created.

For the corrected diagnostic diff, perform direct ownership, links/anchors, tables/fences,
whitespace, unchanged-assertion and private-boundary checks, then inspect
`uv run sf2 verify plan --base origin/main --head HEAD` on the clean committed candidate.
Exact candidate/remote/CI identity belongs to the PR handoff; public CI does not substitute for
the private nine-node results above. Main-gate retains independent acceptance.
