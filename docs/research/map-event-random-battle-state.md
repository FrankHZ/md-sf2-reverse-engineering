# Map-Event Random-Battle State

## Static caller and function contract

**Confirmed — static source/H1/ROM structure.** The public fixture
`sf2-map-event-random-battle-state-static-v1` selects seven positive programs (907 zero programs)
from the retained 914-program map-event mother corpus. They contain eight `CheckRandomBattle` caller
contexts in six physical zone-event tables: Map 66 default / battle 3; Map 67 default / battle 21;
Map 68 default / battle 19; Map 69 event 0 / battle 17; Map 70 event 0 / battle 14; Map 72 event 0 /
battle 26; and the source-ordered Map 72 default North Cliff / battle 8 then North Parmecia / battle
24 alternatives. Each caller records its `move.w` setup, direct `jsr`, and lexical continuation.

`CheckRandomBattle` is anchored at `0x47856..0x478C6`. Its 34 source statements produce 35 H1 rows,
because `sndCom SFX_BOOST` expands to `trap #sound_command` and `dc.w SFX_BOOST`. The guarded source
shape first derives `BATTLE_COMPLETED_FLAGS_START + d0` for `j_CheckFlag`; non-completion sets `d1=-1`.
On a nonzero `STEP_COUNTER`, it sets `d1=0`; otherwise it calls `GenerateRandomNumber` with ranges 8
then 4, writes the second result plus two to `STEP_COUNTER`, and selects the request sequence only when
`d1` is nonzero: `j_SetFlag`, `MAP_EVENT_TYPE=$100FF`, `STEP_COUNTER=30000`,
`WaitForViewScrollEnd`, `SFX_BOOST`, and `ExecuteFlashScreenScript`. The fixture records direct and
effective targets for both flag aliases, plus the RNG, camera, and flash entries; it does not infer
their runtime effects.

The bounded corpus is 18 source identities, 58 source operations, 59 H1 rows, 208 caller/body bytes,
seven retained service/jump joins, and 66 source/H1/ROM anchors. Provenance: USA ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`; upstream
`ShiningForceCentral/SF2DISASM` `master` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`; H1 listing
`build/sf2build-h1.lst`; reproduce with `uv run sf2 h2 map-event-random-battle-state`. Evidence date:
2026-08-27.

## H3 runtime-question queue

**Unknown — grouped random-battle execution:** natural caller reachability and selected caller/battle;
completed-flag result, entry and final `STEP_COUNTER`, both RNG results, selected return and register
result; unlocked flag and `MAP_EVENT_TYPE` before/after; wait/sound/flash completion and presentation;
downstream `CheckBattle`/`BattleLoop` admission; and map/save/load persistence. No emulator work is
included in this static slice.
