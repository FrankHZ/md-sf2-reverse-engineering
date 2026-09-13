# Transitional reference implementation

This directory preserves the selected public-synthetic and private Map 3/Battle 01 comparison
entry points while [ADR 0019](../../docs/decisions/0019-state-and-content-driven-remake-engine.md)
migrates their actual capabilities. It is not a second supported production engine.

`Sf2.Remake.Reference` contains the former scenario-bound Domain/Application/Content code. Existing
namespaces are retained for the legacy consumers; assembly ownership is explicit in their project
references. Production `remake/src` projects do not reference this assembly. The new
`Application.Runtime.GameSession` admits typed authored content and cannot invoke a legacy route.
The old `Application.Sessions.GameSession` exists only in this reference assembly and remains a
known legacy monolith. Moving it does not claim to have refactored or completed its story behavior.

| Owner here | Current consumers and retained meaning | Required exit with capability migration |
| --- | --- | --- |
| `Fixtures/Battle01/OriginalBattle01Controlled*` | Legacy startup, arrival and return comparisons; controlled party/vitals/positions and route-specific expectations | M2 maps real actor/encounter definitions through validated Content; controlled starts and expectations remain reference inputs supplied through the common public entry. No production preset class. |
| `Content/Battle01` and `Readers/PrivateOriginalBattle01StartupReader.cs` | Legacy private startup import and source trust checks | M2 retains pinned provenance verification, externalizes encounter/roster definitions, and removes fixed comparison identity from gameplay admission. Private input is still mandatory for the corresponding comparison. |
| `Rules/Battles/Battle01*` and `Sessions/Battle01` | Existing private battle calls in Godot `PrivateBattle01Composition`; legacy behavior tests | M2 replaces each actual AI/physical/reward/turn dependency chain with generic Domain rules and the common Application dispatcher. Delete migrated wrappers and receipt guards with their last caller. HEAL/RNG/range/turn generation/weighted movement already consume shared rules; their old history gates are reference-only. |
| `Content/Maps/OriginalMapRuntimeAdmission.cs`, other map definitions and private map readers | Legacy Map 3 trust/admission, source-shaped map definitions, reached story owners | M3 moves actual maps/entities/events/program definitions into Content-loaded configuration. Preserve provenance and original special rules; a named program is data, not a production class. Retire fixed package/case gates after their common reader and public-entry comparisons exist. |
| `Sessions/Maps/OriginalMap*` and `PrivateOriginalMap*` | Legacy map composition, presentation and controlled story paths | M3 must execute each reached typed program and its waits/effects before replacing the corresponding handler. Existing endpoint assignments are preserved only for legacy comparison; never treat them as a generic implementation or delete required story behavior as a test fixture. |
| `Content/Synthetic`, `Sessions/Synthetic`, `Rules/Battles/TacticalBattle.cs`, remaining readers | Existing synthetic map smoke, simplified tactical comparison, presentation asset trust | M3/M4 migrate actually adopted map/item/asset behavior; retire synthetic-only runtime routes when no comparison needs them. Synthetic combat is not original-game evidence. |
| `Sessions/GameSession.cs` | Legacy Godot compositions and selected old tests | Shrinks as M2/M3 remove each caller family. Remove the legacy start binding with its last admitted capability. M5 handles remaining cleanup, not wholesale deferred migration. |

The ordinary physical scalar body has one owner: `Domain/Battles/Rules/PhysicalStrikeRules.cs`.
`Rules/Battles/Battle01EnemyPhysicalAttack.cs` retains legacy admission/replay projection but invokes
that shared calculation; the duplicate strike body is deleted. `Battle01PlayerPhysicalAttack.cs`
uses shared damage-EXP/award-randomization/gold/kill-cap functions. The enemy counter wrapper also
uses the shared EXP award; its old `MainRoll` helper and both duplicated reward-randomization bodies
are removed. Existing `Battle01MainRandomRoll` is only a reference DTO projection of the core draws,
not a second RNG or reward calculation authority. The legacy turn cleanup delegates its defeat cap
to `BattleRewards`. `Battle01EnemyStandby.ThinkingRoll` now projects the shared
`BattleRandom.NextThinkingWord` draw, including rejected bytes; no separate reference thinking
algorithm remains. `Battle01EnemyPhysicalAttack.Priority` and `SelectTarget` now project shared
`PhysicalTargetRules`: the reference reverses its RNG-ordered DTOs back to reachable-array order and
supplies its admitted Flying table. Duplicate script3, cohort, class-rank and movement-tie calculations
are removed; the old class check remains only a private profile/land-rule admission guard. Authored
regular movement uses the Regular table through that same selector, with named class definitions.
`Battle01EnemyPursuit` delegates stable raw-cost target selection and radius station search to
`AiMovementRules`; `Battle01EnemyStandby.SourceWalk` delegates the source direction-mask walk to that
same owner. `Battle01MovementGrid` projects the shared weighted grid. Duplicate calculation bodies
are removed; reference path bounds16×20, hovering costs, source move-string formatting, commandset7
and history/order guards retain their comparison-specific roles. Authored commandset06 uses its
configured map and regular movement and admits complete raw target costs0–127 only.
Private activation, commandset admission, source move-string projection and startup/history admission retain their
existing consumers and removal points.
Their fixed profile and history guards are reachable
only from `Sessions/Battle01/PrivateOriginalBattle01EnemyPhysicalAttack.cs` and
`PrivateOriginalBattle01PlayerPhysicalAttack.cs`, the old `Application.Sessions.GameSession` and
Godot `PrivateBattle01Composition`, plus selected reference behavior tests. These wrappers still own
legacy enemy AI/ally counter and controlled completion histories; the authored first/second/counter implementation
does not replace those private startup consumers. The next corresponding M2 private battle admission/AI/
reaction migration must move those calls to common commands and remove these wrappers with their
last caller. M3 owns map-start/program consumers; M4 owns outcome/return. None is deferred wholesale
to M5, and no production project acquires a reference dependency.

Reusable map/layout/item reducers remain in Domain; their existence is not a claim that common-session
exploration is implemented. Shared battle rules and the authored Content reader live in production
responsibility directories. Fixed receipt counts, round positions, kill order, expected terminal
snapshots and comparison IDs have no role in the authored engine's admission or command path.

## Private startup callers and removal boundaries

[ADR0019's private-admission dependency decision](../../docs/decisions/0019-state-and-content-driven-remake-engine.md#private-battle-admission-dependency-boundary)
selects lossless encounter import as the first implementation, followed by initialized common entry
and the actual reached AI/action consumers. That plan is unimplemented. The table below identifies
the current calls that prevent deletion; all paths are relative to this assembly unless stated otherwise.

| Current owner and caller | Keep until / removal condition |
| --- | --- |
| `Readers/PrivateOriginalBattle01StartupReader` is constructed by [`Map3Root`](../game/src/Map3Root.cs); its `Admit` is called by `Sessions/Battle01/PrivateOriginalBattle01Startup.PreparePrivateOriginalBattle01Startup` | First import slice delegates file identity, decode and semantic parsing to production Content and removes the duplicate bodies. Keep only the legacy projection/port until the startup binding consumes the common Content/session path; private trust checks survive that removal. |
| `Readers/StackCompressedGraphicsDecoder` serves both startup and `PrivateOriginalMap3VisualPayloadReader` | Move one calculator to production Content with the first importer. Both real consumers use it; the map visual reader remains for M3. Moving assembly ownership does not require copying either consumer. |
| `Content/Battle01/OriginalBattle01StartupDefinition`, including hardcoded `OriginalBattle01GizmoBaseline`, feeds `PrivateOriginalBattle01Initialization.ProjectBattle01Initialization` | First import replaces encounter parsing only. Enemy definitions must later come from pinned data, with source baseline separate from effective stats. Remove the DTO/projection with its final initializer/comparison consumer; retain source identity verification in Content. |
| `Fixtures/Battle01/OriginalBattle01ControlledPartyPreset` and controlled return/arrival inputs feed startup, physical/healing accounting and recovery/return histories | Move controlled setup values and expected trajectories to external reference data supplied through the common entry as the corresponding consumer migrates. IDs, missing-value supplements and receipt/terminal expectations never become Domain admission. The real party inventory, spells, class and stats are preserved. |
| `Rules/Battles/Battle01Initialization`, `Battle01FirstRound`, `Battle01FirstControl` and matching `Sessions/Battle01` wrappers are called by [`PrivateBattle01Ui.Apply`](../game/src/PrivateBattle01Composition.cs) at startup | Initialized common entry replaces this chain only after source startup, pre-round activation/spawn/program boundary and actual first-player selection are supported. Remove migrated calculation bodies immediately; retain comparison projections only while existing callers use them. Shared turn generation alone does not replace the chain. |
| `Battle01PlayerMovement`, `Battle01EnemyStandby`, `Battle01EnemyPursuit`, both physical rules, `Battle01PlayerHealing`, `Battle01TurnCompletion` and corresponding session methods serve later `PrivateBattle01Ui` branches | Migrate actual class/equipment/AI/action dependencies through common commands, then delete each matching Godot scheduling branch and session wrapper with its final consumer. Source movement/strike/reward/RNG calculations already shared stay single-owned; preset/history checks move to reference comparisons. |
| `Sessions/Battle01/PrivateOriginalBattle01Admission` and startup preparation retain a pending original-map snapshot, idle bridge/locomotion and F401 state; recovery/return/arrival wrappers retain early context | M3 must replace the real map/start program and bridge handoff; M4 must replace the reached outcome/recovery/return path. A standalone controlled private battle cannot justify deleting these callers or claiming natural Map3 continuity. Retire the legacy `Sessions.GameSession` and startup selection only with their last actual consumer. |

The first import's stopping boundary is a trusted encounter definition plus the unchanged legacy
projection. It supplies no party growth, equipped-stat refresh, active-enemy snapshot or completed
common private battle. The next initialized-entry slice must preserve source fields and explicit
Unknowns while using `Application.Runtime.GameSession.Start(IScenarioSource)`; it cannot relabel
`public-authored-controlled-start` or empty unsupported input fields to gain admission.

Use existing selected reference commands only when their comparison is affected. Do not impose the
legacy whole-solution suites on new-engine work or write tests of reference runners. New engine
behavior is checked through `Sf2.Remake.Engine.Tests`; actual adapter input/state observations are
executed directly without screenshots. Original 8C/H4 remains incomplete.
