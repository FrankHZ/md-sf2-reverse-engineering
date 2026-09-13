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
uses shared damage-EXP/gold/kill-cap functions. Their fixed profile and history guards are reachable
only from `Sessions/Battle01/PrivateOriginalBattle01EnemyPhysicalAttack.cs` and
`PrivateOriginalBattle01PlayerPhysicalAttack.cs`, the old `Application.Sessions.GameSession` and
Godot `PrivateBattle01Composition`, plus selected reference behavior tests. These wrappers still own
legacy enemy AI/ally counter and controlled completion histories; the first authored physical slice
does not replace those startup consumers. The next corresponding M2 private battle admission/AI/
reaction migration must move those calls to common commands and remove these wrappers with their
last caller. M3 owns map-start/program consumers; M4 owns outcome/return. None is deferred wholesale
to M5, and no production project acquires a reference dependency.

Reusable map/layout/item reducers remain in Domain; their existence is not a claim that common-session
exploration is implemented. Shared battle rules and the authored Content reader live in production
responsibility directories. Fixed receipt counts, round positions, kill order, expected terminal
snapshots and comparison IDs have no role in the authored engine's admission or command path.

Use existing selected reference commands only when their comparison is affected. Do not impose the
legacy whole-solution suites on new-engine work or write tests of reference runners. New engine
behavior is checked through `Sf2.Remake.Engine.Tests`; actual adapter input/state observations are
executed directly without screenshots. Original 8C/H4 remains incomplete.
