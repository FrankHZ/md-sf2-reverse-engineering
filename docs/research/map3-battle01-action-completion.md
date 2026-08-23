# Map 3 to Battle 01 Action Completion — Static Contract

- Status: **Confirmed** H2 static contract; it is not an H3 observation, natural-play claim, or
  readiness claim.
- Fixture: `sf2-map3-battle01-action-completion-static-v1`
- Reproduction: `uv run sf2 h2 map3-battle01-action-completion`
- Evidence date: 2026-08-23
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source: `ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Boundary and Provenance

This R3c H2 rail starts exactly at the three accepted R3b DropEnemyItem return resumes:
`0x9CD8`, `0x9D3A`, and `0x9D98`. It statically closes the primary-target completion loop,
item break/idle calls, double-attack and counter-attack decisions and blocks, the explosion
backedge, `battlesceneScript_End`, and the WriteBattlesceneScript return. The caller handoff
is checked at `ExecuteIndividualTurn` call `0x24100` and its unentered resume `0x24106`; this
rail stops there.

The deterministic parser inventories exactly nine source identities:
`battleactionsengine_1.asm`, `battleactionsengine_2.asm`, `animateaction.asm`,
`breakuseditem.asm`, `isabletocounterattack.asm`, `createbattlescenemessage.asm`,
`createbattlesceneanimation.asm`, `giveexpandgold.asm`, and
`battlefunctions/executeindividualturn.asm`. It separately guards the source, H1 listing, and
ROM for exactly 26 control anchors: the main completion range; target-loop backedge; three
primary calls; double decision/block/resume; counter call/decision/block/resume; explosion and
end ranges; End, both validators, SwitchTargets, MakeActorIdle, and BreakUsedItem ranges; four
owner entries; and the WriteBattlesceneScript call/resume pair.

The public fixture is recursively closed. It contains only source identities and hashes,
addresses and symbols, control topology, enums/counts, retained-fixture digests, and **Unknown**
labels. It contains no source prose/comments, ROM/H1 bytes, text, asset, capture, movie,
save-state, input-log, runtime fact, or other private payload.

## Confirmed Static Completion Graph

At `0x9CD8`, the primary target loop advances its target state, establishes the next direction,
and has a DBF backedge to the existing target-selection body. The completion prefix directly
calls MakeActorIdle and BreakUsedItem before ValidateDoubleAttack. A zero validator result skips
the second-attack block; otherwise the block statically sequences SwitchTargets, message,
animation, SwitchTargets, ApplyActionEffect, DropEnemyItem, and MakeActorIdle, returning at
the retained `0x9D3A` resume.

ValidateCounterAttack has the equivalent guarded decision. Its nonzero block has the same
statically ordered call topology and returns through retained resume `0x9D98`. The later
explosion path clears/restores its local state, calls DetermineTargetsByAction, and branches
back into the existing target-selection spine. The end path restores the actor copy, calls
`battlesceneScript_End`, releases its frame, and returns. End itself has the checked static
end-animation, target-switch, reward-gate, GiveExpAndGold, HP-replay, text-hide, end-command,
and return topology. This records call/control edges only; it does not reconstruct any existing
physical, spell, dodge, follow-up, EXP, gold, drop, status, death, action-construction, or
battle-scene replay algorithm or golden.

`ExecuteIndividualTurn` reaches a local `j_WriteBattlesceneScript` instruction target, whose
effective target is WriteBattlesceneScript. The static source/ROM return edge is `0x24106`.
No conclusion is made about executing that continuation or its caller.

The extractor regenerates and digest-guards the complete accepted R3b projection and the fresh
`sf2-battle-actions-static-v1` projection before fixture construction and at the golden boundary.
The retained battle-actions relation stays exactly 47 records across 29 source paths and retains
both `battle.actions.apply-effect-dispatch` and `battle.actions.cast-spell`.

## Runtime Unknown Register

The retained R3b register remains **Unknown**: `naturalContinuity`, `initializedSnapshot`,
`naturalFirstActor`, `actorControlBranch`, `playerInputChronology`, `aiCommandSelected`,
`movementPath`, `target`, `action`, `preResolutionArrival`, `dispatchBranchReached`,
`perTargetResult`, `statusOutcome`, `targetDeath`, `expAward`, `goldAward`, `dropOutcome`,
`followupOutcome`, `postEffectArrival`, `afterTurn`, `multiRoundPlaythrough`, `victory`, and
`playerReady`.

R3c adds these ten **Unknown** questions: `primaryTargetLoopCompletion`, `doubleAttackReached`,
`counterAttackReached`, `explosionReached`, `itemBreakOutcome`, `actionConstructionCompletion`,
`writeBattlesceneReturn`, `battleSceneReplay`, `executeIndividualTurnReturn`, and
`nextTurnDispatch`. The grouped queue therefore contains exactly 33 **Unknown** labels. It is
not authorization for H3, BizHawk, a replay, after-turn work, multi-round work, victory work,
or an R4 claim.

## Audit Impact

RA-06 gains only the **Confirmed** static action-completion graph through the source return edge
`0x24106`. Natural continuity, actor/action/target/result and branch selection, target-loop and
follow-up reachability, reward/drop/status/death/post-effect behavior, actual return,
battle-scene replay, after-turn behavior, next-turn dispatch, and victory remain **Unknown**.
The Map 3 to Battle 01 audit remains **OPEN** and **NOT READY**; R3d/R4 are separate work.
