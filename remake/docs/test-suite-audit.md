# Remake Test Suite Audit

## Finding and scope

At the audited accepted base, the whole .NET solution reports **758 expanded cases from 508 test
methods**, of which **189 cases belong to the Godot test project**. The count alone does not justify
reducing the suite. The stronger improvement opportunities are repeated digest-only rejection cases,
tests that freeze internal shape, large coupled setup code, and private checks that return successfully
without exercising their private assertions.

This bounded investigation changes no code, tests, fixtures, schemas, golden observations, or gate
selection. Recommendations below require separate ownership and review; no deletion quota or new
statistics, telemetry, coverage, or benchmark platform is proposed. Priorities rank follow-up value,
not confirmed product defects. No product defect was reproduced by this audit.

The interpretation follows [ADR 0017](../../docs/decisions/0017-heavy-boundaries-light-internals.md):
protect trust, authoritative mutation, versioned ports, and durable observations; allow disposable
internals to change. Current product acceptance remains owned by the
[Map 3 plan](./map03-playability-plan.md) and [verification guide](./development-and-verification.md).

## Reproducible baseline and counting

**Confirmed:** audited commit `f9dbfd5e5366a46fd855e36ee55dd5a1c1e3dd90`, tree
`75a540f3238cd565e8fed0accc87d2c83dfadf0b`. Counts describe that base, not later Map 3 slices.
The [solution](../Sf2.Remake.sln) contains four production assemblies and four xUnit projects.
The completed [Public run 34066578540](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/34066578540)
names the same commit and reports all four project totals below.

| Owning test project | Test C# files | Facts | Theories | Methods | InlineData rows | Reported cases | Test source lines |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Domain | 12 | 129 | 17 | 146 | 63 | 192 | 2,970 |
| Application | 7 | 141 | 7 | 148 | 27 | 168 | 9,600 |
| Content | 5 | 66 | 22 | 88 | 143 | 209 | 4,861 |
| Godot | 10 | 99 | 27 | 126 | 90 | 189 | 5,878 |
| Total | 34 | 435 | 73 | 508 | 323 | 758 | 23,309 |

Methods = Facts + Theories; cases = Facts + InlineData rows at this base. There are no MemberData,
ClassData, or explicit `Skip =` attributes in these projects. Loops and multiple assertions inside a
Fact still count once; a theory row is not necessarily an independent behavior. Source lines include
blank lines, comments, helpers, and test data. They indicate maintenance surface, not a quality ratio.
Python architecture/tooling tests and native Godot source/export smoke executions are outside 758.
The Godot .NET project count is not a count of scenes or native engine acceptance runs.

Read-only reproduction, from a checkout of the audited base in PowerShell:

```powershell
git rev-parse HEAD
git rev-parse 'HEAD^{tree}'
gh run view 34066578540 --json headSha,jobs,url
gh run view 34066578540 --log | Select-String 'Passed!  - Failed:'
gh api repos/FrankHZ/md-sf2-reverse-engineering/actions/runs/34066578540/artifacts
$files = git ls-files 'remake/tests/*.cs' 'remake/tests/**/*.cs'
$rows = foreach ($file in $files) {
    $source = Get-Content -LiteralPath $file -Raw
    [pscustomobject]@{
        Layer = ($file -split '/')[2]
        Facts = [regex]::Matches($source, '(?m)^\s*\[Fact(?:\(|\])').Count
        Theories = [regex]::Matches($source, '(?m)^\s*\[Theory(?:\(|\])').Count
        Inline = [regex]::Matches($source, '(?m)^\s*\[InlineData\(').Count
        Lines = [IO.File]::ReadAllLines((Join-Path $PWD $file)).Count
    }
}
$rows | Group-Object Layer | ForEach-Object {
    $_.Name
    $_.Group | Measure-Object -Property Facts,Theories,Inline,Lines -Sum
    $_.Count
}
rg -n 'MemberData|ClassData|Skip\s*=|Environment.GetEnvironmentVariable' remake/tests
```

The source scan is a base-specific attribute census, cross-checked against completed runner results,
not a general C# discovery implementation. No .NET restore, build, discovery, test execution, Godot,
H3, or old full suite was needed or run to obtain these counts.

## Execution cost and the meaning of green

**Confirmed:** the base CI job ran about 238 seconds. Its .NET restore/build/test step timestamps give
32/14/10 seconds respectively; the build itself reports 12.88 seconds. Runner-reported project test
durations were Domain 707 ms, Application 1 s, Content 3 s, and Godot 3 s. Projects overlap, durations
are rounded, and neither their sum nor an average per case measures serial execution cost.

Two additional completed runs,
[34065381803](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/34065381803) and
[34064921016](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/34064921016), report
the same totals and approximately 10-11 seconds from the test command to the final project summary.
`git diff --name-only 6bcb0f7 f9dbfd5 -- remake` is empty. These samples support a small current CI
.NET execution cost, not a performance trend. In the base run, the separate Python
architecture/planner/asset-preflight step took 125 seconds; its cost cannot be assigned to the 758 C#
cases or any one Python file from these aggregate results.

**Confirmed:** all 758 cases report passed, with zero reported skips. However, three Facts return
before private assertions when their environment settings are absent:

| Test file under `remake/tests/` | Method | Required setting |
| --- | --- | --- |
| `Sf2.Remake.Content.Tests/PrivateCanonicalMap3ImportReaderTests.cs` | `AcceptedIgnoredCanonicalImportCanBeCheckedLocallyWithoutBecomingATestInput` | `SF2_PRIVATE_CANONICAL_MAP_IMPORT` |
| `Sf2.Remake.Content.Tests/PrivateOriginalMap3VisualPayloadReaderTests.cs` | `AcceptedIgnoredInputsCloseExactPayloadAndMutationBoundaries` | `SF2_PRIVATE_ROM`, `SF2_PRIVATE_MAP_TILESET_METADATA`, `SF2_PRIVATE_MAP_PALETTE_METADATA` |
| `Sf2.Remake.Godot.Tests/PrivateLocalPresentationAssetCatalogTests.cs` | `ExactLocalMap3BaseAtlasMountCanBeCheckedWithoutBecomingATestInput` | `SF2_PRIVATE_PRESENTATION_ASSET_ROOT` |

The [Public workflow](../../.github/workflows/public-checks.yml) supplies neither those settings nor
private inputs. Public success is not evidence of private execution; its zero-skip summary conceals
these three non-executed private bodies. Do not subtract them and call the remaining 755 cases
independent coverage either.

**Unknown:** per-case timings, local Windows iteration cost, native Godot gate cost, coverage/mutation
adequacy, flaky-run frequency, and engineer time spent repairing setup. The base CI artifact API returns
zero artifacts, its workflow requests no TRX logger, and this audit worktree had no existing TRX.
Existing aggregate logs do not answer these questions. No measurements were invented or new runs
commissioned to fill them.

## Behavior and boundary value

These are targeted samples, not a complete classification of every assertion. All file/method
references below resolve in the audited Git object.

| Layer and example | Protected failure and assessment |
| --- | --- |
| Domain: [WorkingMapLayoutTests](../tests/Sf2.Remake.Domain.Tests/Maps/WorkingMapLayoutTests.cs), `ForwardHorizontalOverlapCascadesWhenDestinationFollowsSource`, its reverse and vertical companions | Fixed small expected arrays distinguish ordered cascading copy from buffered copy. Similar inputs cover materially different overlap semantics. Retain these and source immutability/span boundaries. |
| Application: [OriginalMapGameSessionTests](../tests/Sf2.Remake.Application.Tests/OriginalMapGameSessionTests.cs), `RoyalReturnCommitsExactDestinationAndRetainsControlledState`, `RoyalReturnBusyAndInvalidMovementPreserveBothAuthorities` | Both movement entry points must select the exact destination runtime/facing, retain route and bridge state, clear transient receipts, and preserve snapshot/locomotion/bridge on rejection. Cross-layer reuse of coordinates is valuable here because transaction authority is different from parsing. |
| Content: [LocalPresentationAssetPackReaderTests](../tests/Sf2.Remake.Content.Tests/LocalPresentationAssetPackReaderTests.cs), `SemanticRasterLookupRejectsMissingSelectionAndPostAdmissionDrift`, `FakeApplicationAdmissionCannotRedirectSemanticPayloadSelection`, reparse-point tests | Reopening admitted files, forged port data, path escape, and post-admission drift are distinct trust failures. Keep adversarial cases and defensive-copy checks; a happy-path smoke cannot replace them. |
| Godot: [PrivateOriginalMapBaseViewportTests](../tests/Sf2.Remake.Godot.Tests/PrivateOriginalMapBaseViewportTests.cs), `CurrentAreaOverlayUsesLatestShiftedLayoutAndZeroBlockHoles`; [PrivateMap3CameraProjectionTests](../tests/Sf2.Remake.Godot.Tests/PrivateMap3CameraProjectionTests.cs), clamp and locomotion tests | Explicit green/red/blue pixels, mutable-layout reprojection, 2x/4x buckets, bounds, and movement offsets catch adapter regressions. Keep distinct scale/boundary cases. These pure projections do not prove native draw output, input polling, DPI behavior, or original rendering. |
| Shared synthetic battle: [PrivateOriginalMapBattleBridgeTests](../tests/Sf2.Remake.Application.Tests/PrivateOriginalMapBattleBridgeTests.cs) and [TacticalBattleTests](../tests/Sf2.Remake.Domain.Tests/Battles/TacticalBattleTests.cs) | Deterministic battle behavior and private snapshot isolation protect the reducer currently reused by the manual bridge. Synthetic provenance limits fidelity claims, not regression value. |

### Concentrated lower-value or brittle coverage

**Confirmed:** [PublicSyntheticMap3PackageReaderTests](../tests/Sf2.Remake.Content.Tests/PublicSyntheticMap3PackageReaderTests.cs)
contains **64 cases in 13 methods** that call `AssertDigestMismatch`. Its helper changes a tracked JSON
string, checks that bytes changed, writes a temporary package, then expects only `ContentDigestMismatch`
at `contentDigest`. In [the reader](../src/Sf2.Remake.Content/PublicSyntheticMap3PackageReader.cs),
`Admit` returns that result before `AdmitDocument` parses or validates any field. For example,
`TacticalBattleIdentityMutationFailsRawDigestAdmission` contributes 13 rows;
`FieldSearchIdentityLocationOrCrossReferenceByteMutationFailsDigestAdmission` contributes 10.
Their varied semantic names do not establish those semantic validators. This accounts for 64 of that
file's 98 cases, not 64 independently demonstrated defects or automatically removable tests.

**Inferred, P2:** reduce this family in a separately owned change to representative malformed-byte,
valid-shape mutation, and whitespace mutation cases that retain exact digest-before-parse behavior.
Keep profile/package admission and actual semantic rejection tests. Where semantic coverage matters,
use the existing internal semantic seam with an independently chosen expected diagnostic. Do not
weaken the production pin or golden package; do not rename a digest test as semantic proof.

**Confirmed:** [PrivateMap3PresenterTests](../tests/Sf2.Remake.Godot.Tests/PrivateMap3PresenterTests.cs),
`BaseAtlasConsumerIsAnExplicitBoundedPresenterSurface`, reflects three private bind methods and pins
their signatures. It also forbids private property names containing `Path`, `Root`, or `Pixel` and
particular root field names. These checks constrain spelling and decomposition without exercising a
bind or proving that data cannot escape. The same test also pins externally parsed smoke markers;
those observations have a different, durable justification.

**Inferred, P2:** when this presenter is next refactored, retain marker compatibility at serialization
and mount/projection behavior, but replace private signature/name constraints with tests of the actual
boundary. Apply the same distinction to source-string assertions such as
`_battlePresenter?.Project(...)` and private method declarations in
[test_remake_architecture.py](../../tests/python/test_remake_architecture.py); assembly direction and
forbidden dependency checks remain valuable. That Python file is outside the 758 count.

**Confirmed:** [Map3PresenterTests](../tests/Sf2.Remake.Godot.Tests/Map3PresenterTests.cs) pins complete
project-authored status/guide strings. [Map3InputAdapterTests](../tests/Sf2.Remake.Godot.Tests/Map3InputAdapterTests.cs),
`BindingsPreserveExactActionAndKeyOrder`, also checks uniqueness of its own constant `expected` array.
Its companion `EveryBindingDispatchesItsExactSemanticAction` exercises real dispatch.
**Inferred, P2:** protect readable state, disclosures, action mappings, and layout bounds; avoid treating
punctuation, disposable copy, private names, or fixture self-checks as independent behavioral coverage.
Exact smoke bytes and intentional input-priority ordering still require exact assertions.

## Setup coupling and the next Map 3 slices

**Confirmed:** this is not universally a suite of private-method tests. The literal
`BindingFlags.NonPublic` occurs 28 times in seven test files: Domain 0, Application 1, Content 3,
Godot 24 occurrences. Occurrences include setup and adversarial fake-port construction, not just shape
assertions. Friend-assembly access already exists for Application, Content, and Godot tests.

The Content readers expose three internal `*ForTests` semantic entry points; their production
admission checks fixed raw identities before parsing. Testing malformed semantic documents through
these seams is justified, provided outer trust checks remain independently covered. Making them public
to simplify tests would weaken the boundary. Reflection used to forge invalid port values likewise
has a stronger reason than pinning the name of a disposable presenter helper.

Specific change-amplification surfaces are:

- `OriginalMapGameSessionTests.cs` has 3,852 lines, including route-driving helpers and the
  `AcceptedOriginalMapRuntimeCatalog` also used by visual-session and bridge tests. Several reducers
  reconstruct `PrivateOriginalMapSessionSnapshot` with explicit retained fields; state-retention
  assertions are necessary while that mechanism remains. The Map 3 plan already identifies the
  repeated pass-through work. Deleting those assertions would conceal that risk.
- `PrivateCanonicalMap3ImportReaderTests.cs` has 2,450 lines. `SampleDocument`,
  `AddSyntheticRoyalPassage`, and `EntitySourceRecords` construct broad synthetic inputs and reuse
  `OriginalMapRuntimeAdmission` constants. That supports admission wiring and drift rejection, but a
  wrongly changed constant can move both sample and implementation together. The focused
  `RoyalReturnRawFacingAgreesWithTheSourceDerivedGraphAnnotation` joins a literal facing to the
  accepted research fixture; `PalaceProjectionBindsUnconsumedTokensWithoutClaimingSourceTextIdentity`
  explicitly preserves the synthetic/source-identity distinction. Retain such independent checks.
- Godot camera setup invokes Application's private `ControlledAdmission`, `Begin`, and `Advance`;
  `PublicSyntheticBattlePresenterTests` reflects `Pending`, `Lifecycle`, `Admit`, and `Update` to
  construct bridge state. A behavior-preserving internal rename can break these presentation tests.
  Prefer an existing admitted session/typed observation when practical; keep exceptional forging
  local to trust-adversarial tests. Do not add production constructors or a generic fixture framework.
- Public scaffolding remains substantive: `GameSessionTests` has 32 Facts across 2,577 lines;
  the public Content file has 98 cases, and public presentation has dedicated status and battle tests.
  It supports redistribution-safe CI/export and the shared manual battle reducer. Keep it as a bounded
  regression fixture; a new private Map 3 route step does not automatically need a synthetic counterpart,
  another capability/receipt family, or more exact diagnostic copy tests.

**Inferred, P2:** when an owning change reaches this setup, reuse its existing local helpers, separate
arrange code from the behavior under review where needed, and stop expanding unrelated synthetic
features. Improve constant-oracle independence only for the consumed contract under change, using
existing minimal tracked evidence. This audit supplies no measured time-saving estimate and does not
authorize a test-data platform or broad snapshot refactor.

## Acceptance gaps and minimal priorities

1. **P1 — distinguish private execution from public success.** The three early-return Facts need an
   explicit private-not-run result in the existing verification/handoff path. An explicitly requested
   private check with missing configuration must report unavailable/failure; ordinary public CI must
   remain independent of private inputs. Fix the reporting/selection boundary in a separate slice,
   preserving the private checks and normal public behavior.
2. **P1 — preserve an independent consumed-route check.** The optional canonical test seeds a validated
   Map 19 entry by reflection into `_privateOriginalMapSnapshot`, then consumes the accepted 38-input
   route, controlled palace result, and return. Its seed bypasses earlier Map 3 continuity. Public
   Application helpers exercise broader routes on synthetic layouts. Neither establishes a continuous
   actual-input route from controlled Map 3 through the next milestone. Extend the existing accepted-input
   scenario only when that milestone is owned, with explicit starting state, semantic commands, retained
   flags/runtime identity, and typed outcome. This is a bounded product acceptance gap, not authorization
   to rerun H3 or turn natural-caller Unknowns into an emulator queue.
3. **P2 — reduce duplicate refusal and internal-shape obligations first.** Start with the named 64-case
   digest family or the private presenter shape test, one separately owned change at a time. Preserve
   positive behavior, each materially distinct trust/state boundary, actual semantic validators, and
   stable observations. A smaller count alone is not acceptance.
4. **P2 — connect projection tests to the existing native gate.** Public CI builds/tests .NET but never
   imports, draws, exports, or runs Godot. Keep the existing local official Godot gate and relevant
   private smoke for owned runtime changes. Pure pixel arrays, exact guide strings, and zero-exit
   launch alone cannot prove interactive presentation. No new native run was performed in this audit.

**Unknown:** natural Battle 01 continuity, original timing/presentation, and full H4 remain at their
existing owners. The suite's size or public green result cannot promote them. Investigation stops at
this report and its review; recommendations are not implemented.
