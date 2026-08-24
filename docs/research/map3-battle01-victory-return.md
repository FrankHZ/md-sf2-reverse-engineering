# Map 3 Battle 01 victory and return

Status: **Confirmed** for the bounded H2 static spine below. ROM: US SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`; upstream: `ShiningForceCentral/SF2DISASM` `c834c652b6862bc5679fd7f69a38a7093206efc6`.

## Static boundary

The verifier starts at the unentered `BattleLoop_Victory` entry at ROM `0x23CBA` and stops at the `MainLoop` call at `0x75E4` to `ExplorationLoop` (`0x257C0`); it records the call edge but does not enter that target. `BattleLoop_Defeat` at `0x23D44` is an exclusion boundary.

`BattleLoop_Victory` calls the living/immortal-allies helper, reaches `ExecuteAfterBattleCutscene` at `0x23D08`, derives and clears `F401`, derives and sets `F501`, returns `d4 = 1`, then `RTS`. The source/H1/ROM contract guards that clear-before-set ordering and both flag arithmetic steps.

The after-battle routine checks the completed flag, selects Battle 01 row 1 from `rpt_AfterBattleCutscenes` (`0x47CF4`), and statically identifies `abcs_battle01` at `0x496DC`. Its public structural corpus is exactly 80 source operations in 27 command forms, represented only as operation IDs, commands, operands, and hashes. `EndAfterBattleCutscene` consults the Battle 01 join row at `0x47D6B` (value zero) before returning. `MainLoop` calls BattleLoop, resumes at `0x75E0`, calls `SwitchMap`, and then makes the unentered ExplorationLoop call.

This proves a selected static edge, not a natural victory or an executed cutscene program. RA-07 is therefore Confirmed only for this static Victory → after-battle program → `F401`/`F501` → `d4` return → MainLoop SwitchMap/Exploration call spine. RA-06 natural playthrough, RA-11 complete 8C, and RA-12 stable controllable endpoint remain **Unknown**.

## Provenance and reproducer

The 16-source denominator, 46 H1/ROM anchors, exact ranges `0x23CBA..0x23D44`, `0x47CBC..0x47CF4`, `0x47D54..0x47D6A`, `0x496DC..0x4980E`, and `0x75DA..0x75EA`, retained-owner digests, and index bindings are stored in `sf2-map3-battle01-victory-return-static-v1`.

Run `uv run sf2 h2 map3-battle01-victory-return` with the private canonical ROM and pinned upstream checkout.

## H3 question queue

Runtime-only questions remain grouped: natural victory/caller chronology; whether the after-battle cutscene and all 80 operations actually execute and complete; join and flag outcomes; post-battle MainLoop/SwitchMap timing; ExplorationLoop entry and a stable controllable endpoint. R4b/H4 owns any follow-up; this slice adds no runtime claim.
