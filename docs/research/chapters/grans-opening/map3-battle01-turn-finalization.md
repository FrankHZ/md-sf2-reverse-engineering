# Map 3 to Battle 01 Turn Finalization — Static Contract

- Status: **Confirmed** H2 static contract; not a runtime observation or readiness claim.
- Fixture: `sf2-map3-battle01-turn-finalization-static-v1`
- Reproduction: `uv run sf2 h2 map3-battle01-turn-finalization`
- Evidence date: 2026-08-23
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source: `ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Boundary and Confirmed Static Spine

R3d starts at R3c's unentered `ExecuteIndividualTurn` return resume `0x24106`. It guards the
source-defined music/actor selector ranges, the effective targets of InitializeBattlescene,
ExecuteBattlesceneScript, EndBattlescene, ApplyPositionsAfterEnemyLeaderDies, and LoadBattle, and
the enclosing return at `0x24240`. It then guards BattleLoop's defeated-cutscene call, first
cleanup/count and outcome edges, ProcessAfterTurnEffects call/join, second cleanup/count, turn
increment, and the backedge `0x23BB2 -> 0x23B40`. Victory `0x23CBA` and defeat `0x23D44` are
unentered boundary entries only; their bodies are excluded.

The deterministic parser inventories exactly eleven assigned source paths and independently
checks 34 bounded H1/ROM anchors. It checks source ordering, alias-aware call targets, branches,
returns, and backedge targets. The public, recursively closed fixture contains identifiers,
hashes, addresses, symbols, control topology, counts, and **Unknown** labels only. It excludes
source prose/comments, raw ROM/H1 bytes, dialogue, assets, captures, state, input, movies, and
runtime/H3 claims.

R3c and the accepted battle-functions, battle-scene-engine, battle-cutscenes, battle-loop,
battle-control, and after-turn status-lifecycle owners are digest-guarded before construction and
again at the golden boundary. No owner algorithm or golden is duplicated.

## Runtime Unknown Register

The retained R3c 33-label register remains **Unknown**. R3d adds exactly
`battleSceneMusicBranch`, `battleSceneInitialization`, `battleSceneTeardown`, `battlefieldReload`,
`defeatedCutscene`, `preAfterTurnOutcomeGate`, and `postAfterTurnOutcomeGate`, for 40 labels.
Natural continuity, actual actor/action/target/result/branch/replay/after-turn outcome, player
readiness, next turn, multi-round play, victory, and R4 remain **Unknown**. This queue is not H3
authorization.

## Audit Impact

RA-06 gains only the **Confirmed** static caller replay/teardown/reload/after-turn/next-turn
control spine. RA-07 gains only the static, unentered victory/defeat edges. The audit remains
**OPEN** and **NOT READY**. Victory/defeat bodies, caller-side replay outcomes, R4, next turn-order
read, next ExecuteIndividualTurn, H3/8C/H4, and Phase 4 are separate work.
