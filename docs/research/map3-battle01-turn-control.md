# Map 3 to Battle 01 Turn/Control — Static Contract

- Status: **Confirmed** H2 static contract; it is not an H3 observation, natural-play claim, or readiness claim.
- Fixture: `sf2-map3-battle01-turn-control-static-v1`
- Reproduction: `uv run sf2 h2 map3-battle01-turn-control`
- Evidence date: 2026-08-23
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source: `ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Boundary and Provenance

This R3a H2 rail begins strictly at `BattleLoop` `0x23B40`, after the retained R2c
`GenerateBattleTurnOrder` call has returned. It ends at the first original
`WriteBattlesceneScript` per-target `battlesceneScript_ApplyActionEffect` edge
`0x9CD0 → 0xA3F4`. That pre-resolution edge is **unentered**: this rail does not parse or promote
effect, resolution, after-turn, multi-round, or victory semantics.

The fixture regenerates and exact-compares the accepted R2c admission fixture before building its
output, then repeats that retained projection/digest check at the fixture comparison boundary. It
inventories exactly ten direct pinned sources and independently hashes 27 named H1/ROM anchors.
The public fixture contains identifiers, addresses, enum/order facts, and hashes only; it contains no
source listing, ROM bytes, asset, capture, state, input, runtime observation, or callback payload.

## Confirmed Static Spine

`BattleLoop` reads `CURRENT_BATTLE_TURN`, bases at `BATTLE_TURN_ORDER`, reads the actor byte, and
compares it with `FF`. The sentinel branches to turn generation; any other value calls
`ExecuteIndividualTurn`.

`ExecuteIndividualTurn` has a dead-actor exit. In both preparation and execution passes, MUDDLE and
the AI-controlled bit route to AI. For an enemy, opponent-control false routes to AI and true routes
to player control; for an ally, auto-battle true routes to AI and false routes to player control.
Player control enters the accepted `ProcessBattleEntityControlPlayerInput` owner at
`0x23FE6 → 0x24662`; AI preparation enters the accepted `StartAiControl`/`ExecuteAiCommand` owners,
and later execution enters `ExecuteAiControl` at `0x24036 → 0x252FA`. Both paths converge at
`0x2403A`. This contract does not select a branch for a natural actor.

Battle 01's three ally rows use `HEALER1` only as the alternate-AI-control source form. Its six enemy
rows assign `ATTACKER1` four times and `ATTACKER2` twice. Pointer entries 6 and 7 resolve to the
accepted ordered command sequences `ATTACK1, HEAL1, SUPPORT, MOVE1, STAY` and
`MOVE_ORDER1, ATTACK1, HEAL1, SUPPORT, MOVE1, STAY`. The sequences are cross-owner joins, not a new
implementation of AI choice, movement, target selection, or action effects.

`StartAiControl` uses the enemy commandset path to the `d5` commandset register, looks up
`pt_AiCommandsets`, and walks the bounded command list through `ExecuteAiCommand` until its first
success. This records only the cross-owner traversal and does not duplicate command-selection,
movement, target, or resolution algorithms.

For an ordinary committed action, source order is two `WaitForVInt` calls, actor reload, and the
`j_WriteBattlesceneScript` alias to `WriteBattlesceneScript`. The common action owner then calls, in
order, `DetermineTargetsByAction`, `InitializeBattlesceneProperties`,
`DetermineIneffectiveAttack`, and `InitializeActors`, before its first per-target ApplyActionEffect
call. Existing battle-functions, battle-AI, battle-actions, and spriteset rails remain the owners of
their algorithms and detailed goldens.

## Runtime Unknown Register

The closed runtime Unknown register is `naturalContinuity`, `initializedSnapshot`, `naturalFirstActor`,
`actorControlBranch`, `playerInputChronology`, `aiCommandSelected`, `movementPath`, `target`,
`action`, `preResolutionArrival`, `resolutionEffects`, `afterTurn`, `multiRoundPlaythrough`,
`victory`, and `playerReady`. All fifteen are **Unknown**. It is not an automatic H3 queue: H3 is
excluded by this slice contract, with no H3 fixture, registration, emulator launch, or runtime result.

## Audit Impact

RA-05 gains only the static first-consumer/control-dispatch foundation. RA-06 gains only the static
control-to-pre-resolution spine. RA-03/RA-04 natural continuity, RA-07, RA-09 prose, and RA-11
complete 8C remain **Unknown**. The Map 3 to Battle 01 audit remains **OPEN** and **NOT READY**.
