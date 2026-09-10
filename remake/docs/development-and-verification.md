# Remake Development and Verification

## Scope

This document owns the remake-specific build and gate route. Global verification selection remains
owned by [ADR 0012](../../docs/decisions/0012-dependency-aware-partitioned-verification.md), the root
repository README, and the committed planner. Local private input layout remains owned by
[Local Private Inputs](../../docs/operations/local-private-inputs.md).

Do not replace planner output with a remembered command list. Run only the affected partitions plus
any explicitly justified semantic dependency.

## Locked .NET Workflow

Run from `remake/` so the pinned SDK is authoritative:

```powershell
dotnet restore Sf2.Remake.sln --locked-mode
dotnet build Sf2.Remake.sln --configuration Release --no-restore
dotnet test Sf2.Remake.sln --configuration Release --no-build --no-restore
```

The solution contains the four production assemblies and their four owning test projects. Package
versions are centrally pinned and each project uses its tracked lock file. Do not install a parallel
SDK or edit lock files merely to make a local build pass.

Use `dotnet format` in verify mode or against explicitly named changed projects when formatting is part
of the owning slice. A documentation-only change does not run .NET unless the committed planner selects
`remake-dotnet`.

## Optional Private .NET Checks

Private checks can consume explicit local inputs. They use the existing xUnit skip mechanism and keep
their production admission and private assertions unchanged. No private input is required by Public CI.

| Test method | Required environment variables |
| --- | --- |
| `AcceptedIgnoredCanonicalImportCanBeCheckedLocallyWithoutBecomingATestInput` | `SF2_PRIVATE_CANONICAL_MAP_IMPORT` |
| `AcceptedIgnoredInputsCloseExactPayloadAndMutationBoundaries` | `SF2_PRIVATE_ROM`, `SF2_PRIVATE_MAP_TILESET_METADATA`, `SF2_PRIVATE_MAP_PALETTE_METADATA` |
| `ExactLocalMap3BaseAtlasMountCanBeCheckedWithoutBecomingATestInput` | `SF2_PRIVATE_PRESENTATION_ASSET_ROOT` |
| `ExactLocalMap21Map40AndMap57BucketsRejectMissingOrChangedPayloads` | `SF2_PRIVATE_PRESENTATION_ASSET_ROOT` |
| `AcceptedMap57DefinitionAndRealBucketsComposeTheFixedAreaAndRejectLayoutDrift` | `SF2_PRIVATE_CANONICAL_MAP_IMPORT`, `SF2_PRIVATE_PRESENTATION_ASSET_ROOT` |
| `AcceptedSelectedBattle01InputsAreRequiredToExerciseTheRealReader` | `SF2_PRIVATE_BATTLE01_DATA`, `SF2_PRIVATE_BATTLE01_SCENE`, `SF2_PRIVATE_BATTLE01_TERRAIN` |
| `AcceptedSelectedInputsInitializeRealNineUnitProjectionFromControlledPending` | the three selected Battle01 variables above plus `SF2_PRIVATE_CANONICAL_MAP_IMPORT` |

For each test, no selected inputs means an explicit **skipped / no private assertions ran** result.
Configuring any of its input variables selects that private check; partial configuration fails instead
of skipping. Complete configuration runs all its existing assertions, including fixed input identity
and admission checks. Empty or whitespace-only values count as missing. Configuration diagnostics
name environment variables without printing their values or local paths.

For an explicitly required private run, also set `SF2_REQUIRE_PRIVATE_TESTS=1`. Only the exact value
`1` requires execution of the selected private tests, so missing configuration produces a test failure
and a nonzero process exit. `--filter` narrows tests but does not itself opt into private execution.
Set the switch before invoking `dotnet test`, since xUnit decides skips during discovery. Use the
existing project and fully qualified method filter, for example after the locked build and after
configuring the canonical input:

```powershell
$env:SF2_REQUIRE_PRIVATE_TESTS = '1'
try {
    dotnet test tests/Sf2.Remake.Content.Tests/Sf2.Remake.Content.Tests.csproj --configuration Release --no-build --no-restore --filter 'FullyQualifiedName~AcceptedIgnoredCanonicalImportCanBeCheckedLocallyWithoutBecomingATestInput'
    if ($LASTEXITCODE -ne 0) { throw 'Required private canonical check failed.' }
} finally {
    Remove-Item Env:SF2_REQUIRE_PRIVATE_TESTS
}
```

The [Map 3 route command](./map03-playability-plan.md#royal-route-acceptance-boundary) includes its
input selection. The visual test belongs to the same Content test project; the asset-mount test
belongs to `tests/Sf2.Remake.Godot.Tests/Sf2.Remake.Godot.Tests.csproj`. Use their tabled method names
with the same required switch and filter. Scope the input variables to the owning run, and keep
payloads and TRX/log output in that worktree's ignored directories. A required run with missing
inputs is a failed availability check, not a private success or product regression. A public run
with skipped private tests is public evidence only.

## Selected Battle01 Startup Inputs

The new Content check actually reads the two fixed selected source exports and the compressed
terrain input, runs the existing C# Stack decoder and checks the exact full deployment/terrain
contract. Its public semantic tests use authored placements, regions and terrain. They bypass only
the transport identity for parser checks and cannot claim admission of the canonical payload.
Application tests use the existing internal projection seam for authored data; the required-private
check exercises real digest computation and the same custom-port admission predicate.

From the repository root, set `SF2_PINNED_UPSTREAM` to the registered read-only upstream checkout
at commit `c834c652b6862bc5679fd7f69a38a7093206efc6`. Use a fresh owning worktree's ignored output
paths. These are the existing selected exporters; no whole battle/map extraction or legacy rail is
required, and the startup reader does not need a ROM.

```powershell
$upstream = $env:SF2_PINNED_UPSTREAM
$env:SF2_PRIVATE_BATTLE01_DATA = Join-Path (Get-Location).Path 'local/derived/battle01-data.json'
$env:SF2_PRIVATE_BATTLE01_SCENE = Join-Path (Get-Location).Path 'local/derived/battle01-scene.json'
$env:SF2_PRIVATE_BATTLE01_TERRAIN = Join-Path $upstream 'disasm/data/battles/entries/battle01/terrain.bin'
& ./scripts/Export-Battle01Data.ps1 -UpstreamPath $upstream -OutputPath $env:SF2_PRIVATE_BATTLE01_DATA
& ./scripts/Export-Battle01Scene.ps1 -UpstreamPath $upstream -OutputPath $env:SF2_PRIVATE_BATTLE01_SCENE
$env:SF2_REQUIRE_PRIVATE_TESTS = '1'
dotnet test remake/tests/Sf2.Remake.Content.Tests/Sf2.Remake.Content.Tests.csproj --configuration Release --no-build --no-restore --filter 'FullyQualifiedName=Sf2.Remake.Content.Tests.PrivateOriginalBattle01StartupReaderTests.AcceptedSelectedBattle01InputsAreRequiredToExerciseTheRealReader'
```

Verify the two export SHA values against `manifests/extractions/battle01-data.json` and
`battle01-scene.json`; the production reader also enforces them. The scene export contains metadata
rather than decoded grid bytes, so the third input is explicitly required. Its compressed SHA is
`A0E6B0D4F656C7BD893923330148B3F9366CA7D839F5A0676272A2C95DAABC4A`, length 284. The decoded
terrain is 2,304 bytes with the accepted scene digest; it is not a 16-by-20 cropped array.
Missing/partial required configuration fails, while an unselected optional run explicitly skips.
Keep these environment variables scoped to the owning command and put TEMP/TMP, logs and generated
outputs in the same worktree. Never mutate the supplied upstream terrain file.

Existing frozen selected exports may be consumed read-only after identity verification; do not rerun
the exporters solely for a new candidate SHA. To exercise final initialization, also supply the
registered read-only canonical import through `SF2_PRIVATE_CANONICAL_MAP_IMPORT`, with SHA
`DDDA4FA05455DDBA9CDAF85497CEE0C1C89C6E625721A8FEAD301044C892E508`. Then use the full Content
filter `FullyQualifiedName~PrivateOriginalBattle01StartupReaderTests` with
`SF2_REQUIRE_PRIVATE_TESTS=1`. The additional required test seeds a completed controlled Map40 route
through accepted test-only factories, issues ordinary moves to exact Pending, reads the selected
inputs and calls the real session initializer followed by `EnterPrivateOriginalBattle01FirstRound`,
`EnterPrivateOriginalBattle01FirstControl`, movement select/confirm/cancel and `CommitPrivateOriginalBattle01Stay`.
It verifies the nine-unit projection, immutable terrain/occupancy, computed enemy ATT, activation bits
and active/tested-region distinction, the complete ordered buffer/current offset and final RNG image,
input preservation and duplicate rejection. The real class/status/MOV and named activation-word policy
feed the control entry; a cost2 move from (9,18) to (9,17), blocked (9,19) rejection, provisional
occupancy, cancel restoration and a subsequent STAY exercise the complete API chain. STAY must retain
nine units and seed0xA4991234 and advance raw byte offset0 to2. Next entry must use actual candidate2,
class1/CENTAUR2, budget14 and current occupancy. Its move to (7,17), cancellation to (7,18) and
second STAY must preserve actor1 at (9,17), both receipts and all64 slots. Offset4 must expose actual
enemy128 / OpponentAi; that classifier retains the exact second completed snapshot. The enemy-standby
facade must then move128 to(6,3), record RNG ranges8/2/1 with results7/0/0 and61/85/1 generator steps,
seed-copy1234 to3934, memory0 to14h and tested-mask7 to0. It preserves mainA4991234, all stats by
identity, both earlier receipts and all64 slots. Continue actual131/133/129/130/132 with evolving
occupancy, own memory and chained seed-copy: 1336 further steps end0134 at offset16. Only then
supply actual Bowie0's missing word0, enter Regular1 budget12, move/cancel/confirm/STAY and retain
all nine linked receipts at offset18/FF. The next-round API then generates round2 from current state,
retains history/memory/copy, and moves round2 Bowie from(8,17) to(11,15) at cost10.
Subsequent explicit origin-STAY choices admit region1 and both active enemies in actual round3,
then continue through round4/5 to enemy132's typed pursuit-API boundary in round6 and the separate
ordinary physical attack. Check priority3,57 further thinking bytes, six main calls, mainAF881234/
copy0134, HP snapshot12→temporary9→restore12→replay9, last-target slot4=0 and receipt51/raw12.
Actual Bowie0 then moves/confirms/cancels from(11,15) via the passable(12,15) tile, manually confirms
origin and attacks132. The named preparation retains HP12 and authored EXP0; live HP remains9,
target HP becomes2, EXP becomes15 and receipt52 advances raw12→14. The eight ordered main calls
must match the owning plan. Actual131 pursuit and133 standby consume the next two turns;133's
range8/3 thinking draws produce7/2 over114/19 steps, copy0234. Round7 mainCF491234, all64 slots
and actual player2 at(7,17)/HP11/budget14 are checked with the persistent HP9/2 and EXP15. All generated64-slot orders and
4498 preceding standby thinking bytes match the independent reduction, including the first131
occupied-cell fallback. Shared player-ready fixture
fields corroborate only their named seam; this is not original natural control/timing/RNG chronology.

Continue this same real preparation with explicit gold0/Bowie kills0: player2 origin STAY,
actual131 pursuit, actual132 physical attack and133 standby leave Bowie HP6/EXP15.
Check real player1 HP11/DEF5 gives potential damage2 (remaining9), Bowie damage3/remaining6,
priorities1/19 and six enemy calls. Manually select Bowie's origin Attack132. Compare all six
lethal/award calls, overkill−3 with HP2→0, cap49/half24/EXP39, gold60/kills1, receipt59/raw10,
first worklist132 and cleared second list, faction3/5 twice, retained dead-row provenance,
eight occupied cells and the same64 slots. Actual player1 HP11/budget10 must move/cancel with
mainF7751234/copy0234 and all awards retained. The real reader also checks empty enemy item rows.

The player action filters are `FullyQualifiedName~Battle01PlayerPhysicalAttackTests` in Domain and
`FullyQualifiedName~PrivateOriginalBattle01PlayerPhysicalAttackTests` in Application. Together with
the existing input/presenter classes they cover manual legal targets, nested cancellation, exact
snapshot admission, distinct receipt roles, real RNG miss/critical/follow-up/level branches, and late
failure after local HP/EXP replay. The generation regression also exercises damaged132's later
actual physical-cohort admission without healing. The required real Content methods above must
execute with `SF2_REQUIRE_PRIVATE_TESTS=1`; an absent selected dependency is not a skip.

Regression boundaries remain executable in the owning tests: unreceipted double/counter seeds
and HP drift fail historical admission before resolution; resolver tests exercise real follow-ups.
Recorded-endpoint generation rejects a forged round7 main image. First-defeat history rejects
removed-cell ghost occupancy, changed placement, missing/duplicate worklists, count transitions,
gold/kills/EXP, independent RNG channels and receipt-role drift. Next-player and later-generation
checks admit a cleaned enemy without resurrection, blocking or repeated rewards. Late Domain
finalization and Application preparation-origin failures retain exact selected snapshots.
Preserve completed suite results and failed node IDs in the handoff; rerun corrections narrowly.
These seeds are explicit comparison inputs, not a natural Map3-continuity claim.

The preparation Application filter is `FullyQualifiedName~PrivateOriginalBattle01StartupTests`. It checks
valid/repeated preparation, exact current Pending, rejection before source reads, custom-port
validation, party/RNG constraints and read-only collection/stride behavior. Initialization belongs to
`FullyQualifiedName~Battle01InitializationTests` in Domain and
`FullyQualifiedName~PrivateOriginalBattle01InitializationTests` in Application. These cover
deterministic initialization, healing without double equipment, late rejection before commit, stale/
foreign/duplicate requests, frozen provenance, closed old commands and reset. First-round ownership
is `FullyQualifiedName~Battle01FirstRoundTests` in Domain and
`FullyQualifiedName~PrivateOriginalBattle01FirstRoundTests` in Application. These use the accepted
RNG, region/secondary activation and turn-order boundary fixtures for word width, zero-range
consumption, edge inclusion, bit preservation, second entries, byte wrapping, signed stable sorting
and sentinel placement. The Application checks include failed local projection without committing
partial flags/RNG. The pure helper uses the 16-bit `rng-v1.json` seed; the controlled battle uses the
distinct four-byte RAM image recorded by the player-ready observer.

First-player ownership is `FullyQualifiedName~Battle01FirstControlTests` and
`FullyQualifiedName~Battle01PlayerMovementTests` in Domain, and the corresponding names prefixed by
`PrivateOriginal` in Application. These cover source status/activation branches, candidate-only
supplemental input, late failure with no partial commit, real weighted terrain/occupancy admission,
cursor versus provisional live position, confirm/cancel/stale/duplicate rejection and fresh-session
reset. The pure propagation tests consume all five cases in `battlefield-movement-matrix-v1.json`,
including LIFO expansion, budget128 bucket wrap and flat row-edge behavior, without unsafe reads.
The source-mask counterexample checks the explicitly controlled preview policy with a complete
cost10 route, direction replay, terminators, return to origin and current/source position separation;
it is not an original move-string or H4 compatibility test. Use the committed planner
for subsequent .NET and official Godot selection.

STAY ownership is `FullyQualifiedName~Battle01TurnCompletionTests` in Domain and
`FullyQualifiedName~PrivateOriginalBattle01TurnCompletionTests` in Application. These cover the
retained provisional position and immutable stats, both continuing-faction results, missing/changed
policy inputs, status/equipment/placement/occupancy rejection, byte-offset selection and next-slot
sentinel preservation. Exact-current application checks close stale/foreign/duplicate/wrong-phase
requests and old movement/control/round entry. Use the required selected-input test above for the
actual full chain; no optional skip substitutes for it.

Next-player ownership is `FullyQualifiedName~Battle01NextPlayerControlTests` in Domain and
`FullyQualifiedName~PrivateOriginalBattle01NextPlayerControlTests` in Application. These cover
current occupancy, independent cancel, preserved completion provenance, once-only phase entry,
exact snapshot/actor guards, status/AI/profile/sentinel boundaries and late range failure without
partial word or state commit. The movement tests compare Centaur, Healer and Regular reachability through
forest/hills/desert and derive the budget from effective MOV. Complete-prefix and sentinel checks
keep Bowie control after all six first-round enemies. The first-round API reaches the actual sentinel;
the separate next-round API and bounded Godot dispatch also admit coherent primary-region activation.
Add the Domain `Battle01EnemyPursuitTests` and Application
`PrivateOriginalBattle01EnemyPursuitTests` filters for source movement/cohort boundaries, mixed
receipt history, exact snapshots and atomic early/late rejection.
Add `Battle01EnemyPhysicalAttackTests` in Domain and
`PrivateOriginalBattle01EnemyPhysicalAttackTests` in Application for the source arithmetic
discriminators, real miss/critical and follow-up seeds, linked HP/main/thinking/last-target history,
strict physical/STAY policy separation and late replay/finalization rejection. Retain the affected
FirstRound, NextPlayerControl, TurnCompletion, Standby and Pursuit owners' HP persistence checks.

Enemy standby ownership is `FullyQualifiedName~Battle01EnemyStandbyTests` in Domain and
`FullyQualifiedName~PrivateOriginalBattle01EnemyStandbyTests` in Application. Cover all immediate-idle
rolls, both tables, previous-index/terrain/occupancy exclusions, no-alternative memory clearing,
source accumulated direction-mask behavior and incomplete paths. Range0/1 and signed high-byte/range
edges retain actual RNG advancement. Missing/drifted seed-copy and late terrain/stat/occupancy/policy
failures must preserve the full second STAY snapshot. The owning plan includes the independent
existing Python helper and raw-grid reduction; no new original runtime observation is implied.

The thin Battle01 Godot consumer adds the focused Godot filters
`Map3RuntimeProfileSelectionTests|Map3InputAdapterTests|PrivateMap3PresenterTests|PrivateBattle01PresenterTests`
(each prefixed by `FullyQualifiedName~`). They cover all-or-none/private-only/path-free admission,
old callback isolation, current phase, a deliberately authored actor2 first candidate, live versus
deployment position, path/cost projection, confirm/cancel/STAY controls and the completed actor versus
undispatched candidate without old range/path/cursor, plus active next-player projection despite
a preserved historical completion receipt.
Pair them with the required selected-input Content class above, the locked solution and official
seven-step Godot gate. Native acceptance uses the existing probe's explicit Battle01 mode:
[launch and bounded recipe](./presentation-and-assets.md#diagnostic-battle01-launch-and-native-review).
It seeds Map40 once, drives the 28-input Pending route and N/I/Space/Backspace through real Godot
physical-key input events, captures Pending/ready/selected/provisional/cancelled/rejected frames,
and checks snapshot/occupancy/RNG/offset retention and old-layer/input isolation. Inspect the images
for overlap and clipping in addition to the receipt. This mode does not replay the old twelve frames
or assert natural Map3 continuity, original scene fidelity or H3/H4 closure. For repeated inactive
rounds, use `round-continuation` after the owning managed gates. Four captures show round2 ready,
provisional, cancelled and actual round3 ready. Require eighteen linked generation-tagged receipts,
both complete64-slot buffers, mainAA861234/9BD71234, retained effective stats/deployment/targets,
all twelve enemy decisions and2450 generated bytes (1483 in round1,967 in round2), final copy0034
and memory04/04/04/24/34/04h. Compare every thinking byte with the existing Python helper, plus
orders/paths/results with the [independent reduction](./map03-playability-plan.md#implemented-repeatable-inactive-round-continuation).
The required selected-input chain proves real terrain/current-origin movement, reachable region1
activation, round6 attack resolution and HP9 Bowie control. Domain/Application cover wrong actor, stale/foreign/duplicate requests, receipt prefix,
nonzero memory and late failures. The native probe additionally checks isolated authored enemy-first
dispatch and failure retention/no retry, then restores its actual round3 snapshot. The former
`first-round`, `enemy-standby`, `next-player` and `stay` names alias this four-frame chain.
For pursuit, use `enemy-pursuit` with four checkpoints: round3 ready, first131 pursuit on a labeled
test copy, actual round4 player1 ready, and actual round6 Bowie0 ready after attack132. The intermediate
copy uses direct Application steps and restores the exact snapshot before physical-key continuation;
its decision must equal the physical relay's actual131 receipt. Assert all six generated orders,
six pursuit receipts,4498 standby thinking bytes plus57 physical-priority bytes, final mainAF881234/
copy0134 and no repeated dispatch on frames/unrelated keys. The pursuit API's independent typed
boundary remains asserted on a copied snapshot.

Use `enemy-physical-attack` for four checkpoints: actual round6 player1 ready, a labeled copied
snapshot after player1 STAY/inactive128/attack132, restored physical-route STAY yielding Bowie0
ready at9 HP, and physical movement/confirm/cancel back to(11,15) at9 HP. Compare copied and
physical attack receipts, all six RNG calls, visible HP/result and source/export identity.
Use `player-physical-attack` for six native checkpoints: Bowie HP9/EXP0, provisional(12,14)
target132, target-cancelled action choice, restored origin target132, labeled exact selected-snapshot
Application copy after receipt52, and physical Space/actual relay through round7 player2. Compare
the full copied/physical player receipt; inspect every image and the persistent HP/EXP result.
Use `first-enemy-defeat` for six checkpoints: round7 player2; actual Bowie HP6/EXP15;
manual origin target132; labeled copy after receipt59; physical Space reaching actual player1;
and player1 movement/cancel. Compare complete receipts and all accounting, position, occupancy,
RNG/AI/flag channels. Inspect corpse removal, cleared reachable tile, five enemies, Bowie HP6/EXP39,
gold60/kills1 and persistent defeated132/+24 EXP/+60 gold alongside usable controls.
Use `chester-enemy-hit` for the six accepted first-defeat frames plus six continuation frames:
R8 Bowie after Chester physically moves to(11,14)/STAY; player1 after Bowie origin STAY;
a labeled exact player1 snapshot copy completing separate Application movement confirmation,
STAY and the actual relay; the restored snapshot completing those two physical Space presses
to R9 Chester; Chester's(12,14) provisional move; cancel back to(11,14). Compare the complete
copied/physical battle snapshots,57 thinking steps,six attack main draws,24 living generation
draws/full64 slots, HP9, previous awards/dead132 and newest enemy result across rounds.
The two original required real Content methods retain their earlier preset/null assertions.
`AcceptedSelectedInputsContinueChesterPlayerAttackFromAuthoredExpZero` supplies the new preset
at preparation/initialization and reuses the complete real prefix, then confirms Chester's origin,
cycles/cancels/reselects131 and commits damage2/EXP10. Actual128/129/131/133 lead to receipt76,
Sarah HP11/budget10 movement/confirm(10,17)/cancel, Bowie HP3 and main25991234/copy0634.
`AcceptedSelectedInputsContinueSecondEnemyDefeatThroughRoundTenChesterControl` reuses that real
Sarah endpoint, then exercises Sarah origin STAY, actual130, Bowie origin target cycle/cancel/reselect,
terrain0 damage4/second cleanup/awards and real R10 Chester move/confirm/cancel. It checks all78 prior
policies/history, receipt79 worklists[131]/[], both3/4 counts, full64 slots and48-entry AI channels,
main9F861234/copy0034/mask7, both corpses and exact preparation/source identities.
Run all four with `SF2_REQUIRE_PRIVATE_TESTS=1` and
`FullyQualifiedName~PrivateOriginalBattle01StartupReaderTests.AcceptedSelected`; require4 pass/0 skip.
Narrow Domain/Application tests reject profile/accounting/history drift, stale or foreign requests,
null/zero preparation-origin mismatches at both physical publish boundaries, and false local
HP/EXP/RNG replay. Unspecified Chester EXP still rejects his player attack.
Use `chester-player-attack` for18 native frames: the twelve Chester-hit frames followed by
origin target131, target cancel, labeled copied receipt72, physical relay to Sarah, Sarah provisional
move and cancel. Compare the complete copied/physical endpoint and unchanged R9 order, every
receipt/stat/AI/random channel, newest enemy result and separately visible Chester EXP10.
Use `second-enemy-defeat` for25 native frames: retain the18 above, then physical Sarah STAY/130
to Bowie, target131, target cancel, labeled copied receipt79 before generation, physical R10 Chester,
provisional(12,14) and cancel. Compare complete copied/physical battle and receipt79, both cleared
enemies, newest +24 EXP/+60 gold versus live EXP63/gold120/kills2, and persistent Chester EXP10.
Recheck `chester-player-attack` (18), `first-enemy-defeat` (6) and `player-physical-attack` (6):
55 inspected frames, with restore/build/native process receipts for each mode.
All native modes now use the named Chester EXP0 preparation; their earlier HP/RNG/accounting
and frame contracts remain. Preserve old null-preset artifacts separately.
The source semantics omit reaction-animation/VInt/menu/text RNG, so these captures do not establish
original post-playback main/copy values. A change to the named preparation or shared view also
requires the affected enemy physical and pursuit modes. Other modes,
including `round-continuation`, run only when their owning contracts change. Do not repeat a
completed full managed suite after a correction; preserve its failed nodes and rerun them narrowly.

For the Map57 base consumer, add focused `PrivateBattle01BaseViewProjectionTests`,
`PrivateOriginalMapBaseViewportTests` and `PrivateLocalPresentationAssetCatalogTests`. They cover
fixed selection and asset identity, caller-rehashed pack rejection, whole-layout unloaded-slot
rejection, physical texel/flip/transparency reuse, unchanged exploration crops and 24-pixel overlay
alignment. Run the real canonical/asset tests with `SF2_REQUIRE_PRIVATE_TESTS=1`; the actual inputs
must not skip. Keep `TEMP` and `TMP` under this worktree's ignored local scratch. The existing required
selected Battle01 Content check supplies the actual startup inputs; native `base-art` mode consumes
those same selections through the real session. Use the one-frame `missing-atlas` boundary and
two-frame `diagnostic` regression described by the native recipe; all private captures stay local.

## Repository Planner

Run from the repository root on a clean committed head:

```powershell
uv run sf2 verify plan --base origin/main --head HEAD
```

The plan reports changed paths, selected partitions, reasons, resource locks, commands, external gates,
unclassified paths, and whether execution semantics changed. Unknown runtime-shaped `remake/**` paths
fan out conservatively; tracked Markdown is public-core documentation.

The normal repository gate is:

```powershell
uv run sf2 verify
```

It may require registered local private inputs after its public stages. Do not copy private inputs into
a worktree merely to convert an unavailable optional local stage into a claimed pass. Follow the owning
slice and [Local Private Inputs](../../docs/operations/local-private-inputs.md).

`uv run sf2 verify --full` is reserved for milestone, release, shared-harness, or explicit full-parity
work. It is not the default remake feature or documentation gate.

## Local Official Godot Gate

Relevant runtime, Application, Content, Domain, Godot project, toolchain, or runner changes use the
maintained local gate from the repository root:

```powershell
uv run python -m sf2tool.remake_godot
```

The runner consumes [the tracked toolchain manifest](../toolchain.json), verifies the official Godot
version and artifact identities, creates fresh ignored scratch, performs locked restore and Debug build,
imports the project, runs the public-synthetic source smoke, exports the public preset, runs the exported
smoke, scans the output boundary, writes an ignored receipt, and cleans its owned process tree.

The gate accepts explicit `--toolchain-root`, `--scratch-parent`, `--manifest-path`, and `--project-path`
overrides when the owning run requires them. Values remain local; do not commit machine-specific paths.

Official Godot validation is local-only. GitHub Public intentionally does not download or run Godot.

## Private-Local Smoke Routing

Private smoke is not part of the public export runner. It requires:

- an explicit `private-local` profile;
- one explicit fully qualified ignored canonical-import path;
- the private smoke option;
- a current committed Debug assembly;
- a bounded process owner with finite launch, termination, and reap timeouts; and
- assertions for the already accepted marker count, ordering, status, path-free output, exit result,
  timeout result, and owned-process cleanup.

The Godot user arguments use exact `--name=value` forms for profile and canonical import. Do not pass a
private path through a split argument form, print it in a receipt, or infer it from the environment.

Private smoke wrappers remain ignored and are removed after the run. A feature must not weaken fixed
Content trust or bypass the accepted Application session merely to make a smoke pass.

## GitHub Public

The sole public workflow job is `tracked-inputs`. It runs:

- locked Python dependency sync and Ruff;
- the shared tracked-input harness plus architecture/planner checks;
- design-contract traceability; and
- locked whole-solution .NET restore, Release build, and tests.

It uses tracked redistribution-safe inputs only. It does not run H3, H4, private profiles, emulator
sessions, original-fidelity checks, or Godot import/export.

## Process, Path, and Artifact Safety

- Launch native tools with argument lists and finite timeouts.
- Cleanup must own the launched process tree; do not kill machine-global processes by name or PID delta.
- Use a fresh ignored scratch directory per gate and verify it is inside the intended local root.
- Keep immutable shared private inputs read-only and writable scratch worktree-local.
- Never place ROMs, canonical imports, decoded assets, engine archives, templates, `.godot/`, `bin/`,
  `obj/`, exports, captures, or receipts in Git.
- Scan staged paths and exported payloads before acceptance.
- Preserve the first failed attempt and its cleanup result; do not enlarge a timeout as the first fix.

Use [Bounded Inspection and Review](../../docs/operations/bounded-inspection-and-review.md) for large
artifacts, cached-diff review, and exact-identity handoff.

## Proportional Gate Guide

| Change | Expected local gates |
| --- | --- |
| Domain or Application behavior | owning focused tests, locked solution, architecture checks, committed planner, selected local Godot smoke, normal verify where inputs are available |
| Content admission or trust | focused adversarial Content/Application tests, locked solution, private scan, committed planner, selected local Godot smoke |
| Godot adapter or project | Godot tests, locked solution, official local import/source/export/export-run gate, process cleanup checks |
| remake documentation only | link/fence/table/private/diff checks, committed planner, public-core verification, lightweight Public |
| shared gate, planner, toolchain, or workflow | owning focused tooling tests plus every partition selected by the committed planner |

H1, H2, H3, H4, full verification, or private-input runs are added only when their accepted owner or the
committed planner requires them. A zero-exit editor launch, manual screenshot, or unbounded local command
is not a substitute for the maintained gate.
