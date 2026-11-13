# Birdman target and pursuit priority

**Light rigor:** a bounded SF2 class-table correction. `PhysicalTargetRules.ClassRank` binds source
class 6 (BDMN) for both critical physical target cohorts and ordered pursuit. The accepted
[Polca/raft audit](polca-raft-engine-save-audit.zh-CN.md) retains the original `ai-target-class` failure
and its controlled-input limit. Recruitment, story content, roster restoration and generic battle
rules are outside this correction.

**Confirmed (static source):** the US baseline's pinned SF2DISASM revision
[`c834c652b6862bc5679fd7f69a38a7093206efc6`, `aipriority.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/global/aipriority.asm)
defines these zero-based positions in `table_AttackPriority_*`. Lower positions win the critical
class comparison. Physical callers choose the table by their actual mover.

| Table | BDMN (6) | PHNK (9) | SDMN (0) | BDMN versus PHNK in a tied critical cohort |
| --- | --- | --- | --- | --- |
| Regular | 8 | 9 | 0 | BDMN first |
| Flying | 20 | 21 | 0 | BDMN first |
| Archer | 0 | 1 | 11 | BDMN first |
| Mage | 28 | 27 | 0 | PHNK first |

**Confirmed (engine behavior):**
[`BirdmanTargetPriorityTests`](../../tests/Sf2.Remake.Engine.Tests/BirdmanTargetPriorityTests.cs)
checks all twelve admitted unpromoted class positions and BDMN's pairwise critical comparisons
against the other eleven classes at raw priorities 15 and 16, in both candidate orders. Movement
would favor the second candidate, so the test also checks that class comparison precedes movement.

The session test starts typed content through `GameSession.Start`, submits ordinary Stay and
presentation completions, and observes two actual priority-19 candidates, the selected target,
damage only to that target and returned player control. Swapping BDMN/PHNK definitions changes
which actor wins. Regular, Flying/Hovering, Archer/CentaurArcher and Mage/StealthArcher cover all
four tables and their mapped aliases. The public authored JSON class/mover vocabulary is narrower;
the test binds these definitions through the typed boundary also consumed by private readers.

The source-orders pursuit test reaches `SourceEnemyAi` → `SourceMoveOrderAi` → `PursueOrdered`.
Both living targets score 255 outside the order's radius; unrestricted approach still finds a
path and the final stop respects the radius. BDMN/PHNK exchange in the existing single forward
adjacent pass when their processing orders differ by one. BDMN/SDMN do not exchange; a processing
gap of four also prevents exchange. Both target-class arrangements and all four tables are checked,
including destination/path and unchanged RNG streams. This preserves the existing two predicates;
pursuit does not become a global class sort.

Reproduce after loading the worktree's `local/private-inputs.ps1`, following the
[locked environment](../development-and-verification.md#locked-net-workflow):

```powershell
uv sync --locked
uv run sf2 verify engine
uv run sf2 verify adapter
uv run sf2 verify plan --base origin/main --head HEAD
```

The focused engine filter is `FullyQualifiedName~BirdmanTargetPriorityTests`. Use the selected
absolute SDK from `remake/`, locked restore, then `dotnet test` with `--no-restore` and that filter.
The planner runs on a clean committed head. No original-game runtime or natural later-battle claim
is established: that reach remains **Unknown** at this boundary. Luke still joins on the existing
post-Battle13 route; no earlier recruitment or chapter-specific AI condition is added.
