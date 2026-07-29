# Special Screens

- Status: **Confirmed** for all 19 layout-owned files, representative H1 addresses, seven screen
  groups, eighteen resource routes, title/logo input structure, the four-row witch save-menu
  dispatcher, page selectors, action call/branch order, suspend/reset flow, ending-effect ownership,
  the complete nine-resource Stack-compressed tile corpus, and the complete witch choice-palette/
  bubble-animation data path, plus all twelve uncompressed palette/layout presentation resources;
  one BizHawk launch additionally confirms the bounded in-process witch Save/Load/Copy/Delete and
  flag-88 Load target matrix, plus a four-case New action slot/difficulty/save/MainLoop matrix below
- Status: **Inferred** for perceived animation pacing and simultaneous skip/cheat input behavior
- Status: **Unknown** for rendered frame parity, exact audio/VDP timing, and five oversized fixed
  transfer tails
- Evidence date: 2026-07-29
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Complete Source Boundary

All 19 files under `code/specialscreens` are included by the main ROM layout. The inventory groups
them as two ending-kiss files, one ending-jewels file, two Sega-logo files, three suspend files,
three title files, five witch files, and three witch-ending files. Together they contain 3,225 lines,
119 global labels, 61 local labels, and 256 direct calls to 68 unique named targets.

Each file has a representative source symbol bound to the H1 listing. The canonical output also
maps eighteen named incbin resources: fifteen standalone screen graphics plus three Sega-logo
resources embedded in the main logo source. Only paths and small metadata are tracked; original
graphic bytes stay in the ignored local checkout.

## Compressed Graphics Corpus

All nine Stack-compressed tile resources consumed by the special-screen code are parsed by one
deterministic rail. Seven call `LoadStackCompressedData` directly; speech-balloon and Sega-logo tiles
use `LoadCompressedDataAndCopy`. Their 23,296 compressed bytes decode to 50,176 bytes. The rail checks
all nine source ranges, six direct source pointers, H1 entry addresses, and ROM bytes while retaining
only hashes, counts, transfer metadata, and codec statistics.

Eight resources have a statically fixed VRAM transfer size. Title tiles (8,192 bytes), title font
(4,096), and Sega logo (6,144) exactly match decoder output. Suspend string decodes 448 bytes but
queues 2,048; ending witch 7,808→16,384; ending jewels 1,856→16,384; witch screen
13,568→16,384; and speech balloon 1,920→2,048. These five overlong transfers total 27,648 bytes beyond
decoder output. The ending-kiss stream decodes 6,144 bytes and is consumed by the pixel-fill path
without a comparable fixed DMA length.

The rail deliberately calls these excess regions transfer tails, not padding. Static evidence does
not prove whether their staging memory is cleared, stable, overwritten, or visibly consumed.

## Uncompressed Presentation Corpus

The complementary rail closes every non-compressed palette and layout payload used by these screens:
seven palettes contain 240 color words (107 unique, 25 zero), and five layouts contain 4,176 words.
The twelve resources total 8,832 bytes, all source/H1/ROM identical. Compressed tiles remain owned by
the nine-resource decoder rail and are not counted again.

Title layouts A and B are the only two assembled from `vdpTile` source rather than a direct `incbin`.
Their 1,792- and 768-byte expansions concatenate to 2,560 bytes and exactly match both upstream
editor binary mirrors. This confirms source and storage shape, not palette upload order, fades,
layout mutations, scrolling, or final frame composition.

## Logo and Title

The Sega-logo path computes the ROM checksum, owns configuration-mode and debug-mode input-sequence
handlers, and can return early when Start is pressed. The second logo file advances the debug input
sequence one byte at a time and activates the debug toggle when the sequence terminates.

The title screen has two distinct scroll-loop functions and a bounded Start-poll helper used at
several phases. Its entry loads/arranges the title resources and its exit reports whether the caller
should reset or continue to the witch screen. Source control flow is confirmed; exact scroll/fade
frames are not.

## Witch, Save, and Suspend

The witch entry builds its screen, checks both SRAM slots, and dispatches exactly four save actions:
new, load, delete, and copy. Those routes call the SRAM functions inventoried in the technical
services batch and re-enter either `MainLoop` or `alt_MainLoopEntry` as appropriate. The US
`j_SoundTest` entry is only an `rts`, matching the source note that the function is absent from this
release.

### Save-menu action state (Confirmed, static)

The `rjt_WitchMenuActions` word-dispatch table in
`code/specialscreens/witch/witchstart.asm` starts at H1 address `0x73FE` and has four ordered rows:
index 0 `witchMenuAction_New` (`0x7406`), 1 `witchMenuAction_Load` (`0x74E2`), 2
`witchMenuAction_Del` (`0x7574`), and 3 `witchMenuAction_Copy` (`0x754C`). The entry doubles the
returned action index before reading that word table and jumping through it. This is a source/H1
dispatch fact, not a statement about a player-visible input sequence.

`StartWitchScreen` calls `CheckSram` at source line 45/H1 `0x72E2`; it tests `d0` and then `d1` with
ordered `bpl.s` branches before reaching the action page. That page masks `SAVE_FLAGS` with `3` and
supplies availability masks `1`, `6`, or `15` to `j_ExecuteWitchMainMenu` before a negative returned
`d0` branches back to the witch text/menu loop. The associated source/H1 service sites are exactly
`CheckSram` (`0x6EA6`, line 45), `SaveGame` (`0x6F6A`, line 220), `LoadGame` (`0x6FAC`, line 259),
`CopySave` (`0x6FDA`, line 294), and `ClearSaveSlotFlag` (`0x6FEC`, line 331). The parser retains
both instruction and effective identities; all five are external to the three-file witch source
surface, so its zero-inclusive internal effective-target totals are zero and its external totals are
one each.

`ExecuteWitchMainMenu` at H1 `0x16658` masks its starting selector with `15`, returns `-1` on its
documented B-button path, wraps navigation with mask `3`, and draws four source-labelled page kinds:
page 0 actions, page 1 new-slot names, page 2 loaded-slot names, and page 3 difficulties. The source
checks available bit positions 0 through 3. This establishes only the static selector/result contract;
it does not establish controller timing, rendered labels, or perceived navigation behavior.

The New action inverts the masked save flags with the same mask, shifts left once, chooses a starting
selector from bit 1, invokes page 1, subtracts one from its returned selector, and writes
`CURRENT_SAVE_SLOT` before `j_NewGame`. Its guarded call order includes naming/configuration and page 3
with availability mask 15. It writes `GAMESTART_MAP` (3) to `CURRENT_MAP`/`EGRESS_MAP`, then calls
`SaveGame`; only afterward does it set map/X/Y/facing/`d4` for the `MainLoop` handoff: map (3),
savepoint X (56), savepoint Y (3), facing (3), and `d4` value 1. The Load and
Delete actions use the non-inverted, once-shifted occupied-slot selector with page 2 and the same
returned-selector/current-slot sequence. Load calls `LoadGame`; its `chkFlg 88` zero branch reaches
`GetSavepointForMap`, while the nonzero path reaches `j_BattleLoop`, and both branch to
`alt_MainLoopEntry`. Copy asks through `j_alt_YesNoPrompt`, branches back on a nonzero result, then
masks the flags with 3, subtracts one, and calls `CopySave`. Delete similarly branches back on a
nonzero prompt result before calling `ClearSaveSlotFlag`. These are branch/call/order facts only; the
meaning of a prompt result beyond that source polarity is not promoted here.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master`
`c834c652b6862bc5679fd7f69a38a7093206efc6`; the three bounded sources
`witchstart.asm`, `witchmainmenu.asm`, and `witchfunctions.asm`; and
`build/sf2build-h1.lst`. Reproduce with `uv run sf2 h2 special-screens` and
`uv run pytest tests/python/test_screens.py -q`. The resulting contract is
`sf2-special-screens-static-v1` at
`tests/fixtures/h2/special-screens-static-v1.json`. Its 118 compact use-site records each preserve a
pinned source path, source line, normalized instruction, opcode, and operand; every record is
referenced by an ordered semantic-summary list. The focused parser and mutation rail reject an
altered source operand, opcode, branch polarity, table order, or call order before fixture comparison.

### Save actions (Confirmed, one in-process H3 launch)

The runtime fixture `sf2-witch-save-actions-runtime-v1` runs all nine direct service cases and both
`witchMenuAction_Load` flag-88 cases in one BizHawk 2.11.1 / Genesis Plus GX launch. It enters the
original `CheckSram`, replaces only that call's post-return continuation with a work-RAM thunk, and
then executes the original `SaveGame`, `LoadGame`, `CopySave`, and `ClearSaveSlotFlag` entries. This
is a bounded service/control-flow observation, not a complete player-driven witch-menu session.

**Confirmed:** source payload seed 19 saved to slot 1 stores and recomputes checksum byte 71; seed 20
saved to slot 2 stores and recomputes checksum byte 247. After poisoning the observed combatant-data
samples, `LoadGame` for each selector restores its four stored sample bytes. `CopySave` selector 0
records source slot 1, destination slot 2, destination selector 1, and checksum 71; after slot 2 is
restored from seed 20, selector 1 records source slot 2, destination slot 1, destination selector 0,
and checksum 247. Both `ClearSaveSlotFlag` calls only change the observed occupied-flag byte (3→2→0);
the selected slot's observed stored sample bytes and checksum 247 remain unchanged. These checksum
bytes are separate from the 4,016 stored physical bytes per slot and the 8,032-byte physical address
interval per slot.

**Confirmed:** with flag 88 clear, the Load branch reaches instruction/effective target
`GetSavepointForMap` at 30188 (`0x75EC`). With flag 88 set, it reaches instruction target
`j_BattleLoop` at 131124 (`0x20034`) and its jump-interface effective target `BattleLoop` at 146052
(`0x23A94`). The fixture preserves both identities; it does not infer a player-facing meaning for
source flag 88.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master`
`c834c652b6862bc5679fd7f69a38a7093206efc6`; sources
`code/common/tech/sram/sramfunctions.asm`,
`code/specialscreens/witch/witchstart.asm`,
`code/common/tech/jumpinterfaces/s05_jumpinterface.asm`, and
`code/common/maps/egressinit.asm`; H1 listing `build/sf2build-h1.lst`; command
`uv run sf2 h3 witch-save-actions`; observed fixture
`tests/fixtures/h3/witch-save-actions-v1.json`. The Python source guard parses the relevant named
sections and rejects a changed opcode, operand, branch polarity, call order, or jump-interface alias
before comparison with the golden fixture.

### New-game lifecycle (Confirmed, one in-process H3 launch)

**Confirmed:** `sf2-witch-new-game-lifecycle-runtime-v1` runs four cases from one core-state
checkpoint at the original `CheckSram` return. The static source guard binds `witchMenuAction_New`,
the `j_ExecuteWitchMainMenu`/`ExecuteWitchMainMenu`, `j_NewGame`/`NewGame`, and
`j_NameAlly` jump-interface identities, the `CheatModeConfiguration` Start-clear branch, `SaveGame`,
and the `MainLoop` branch. It derives the flag mask/XOR mask (3), selector scale (2), page 1/page 3,
difficulty availability (15), source flags 78/79, and the map/X/Y/facing/`d4` handoff values from
their ordered parsed operands rather than a second literal table.

**Confirmed:** the observed page-1 entry registers distinguish source-selected slot from the injected
menu result. `SAVE_FLAGS` 0 reaches selector/page/availability `1/1/6`; flags 1 reaches `2/1/4` and
the injected result 2 saves selector 1 (slot 2); flags 2 reaches `1/1/2` and injected result 1 saves
selector 0 (slot 1). The page-3 entry is `0/3/15` in all four records. Difficulty results 0, 1, 2,
and 3 produce source flags 78/79 respectively clear/clear, set/clear, clear/set, and set/set. Each
case reaches the original `SaveGame` and `MainLoop` handoff with map/egress 3 and D0–D4
`3/56/3/3/1`. The selected slot stores and recomputes checksum bytes 89, 91, 90, and 92 respectively;
the four sampled bytes are `66,79,0,0`. Stored byte count (4,016), physical address interval (8,032),
and sampled physical addresses remain separate contract fields.

**Confirmed harness boundary:** Genesis Plus GX ignores the observer's attempted `M68K BUS` ROM writes.
The fixture therefore records eight per-word readbacks and uses session-only `MD CART` patches for
both menu calls, NameAlly, DisplayText, and MainLoop redirection; no ROM file is modified. The
observer saves/replays core state outside BizHawk input/memory callbacks, bypasses both menu aliases,
NameAlly, and DisplayText, clears `PLAYER_1_INPUT` before the original configuration helper, and
pulses C only to release text-macro waits after original New-action entry. It records that original
`j_NewGame` and `NewGame`, the configuration entry, difficulty code, `SaveGame`, and the MainLoop
handoff execute. The fixture-owned 4,800-frame deadline logs a timeout milestone and exits BizHawk
with failure before the Python-level 120-second observer timeout.

**Unknown:** this harness-controlled observation does not establish player-driven name editing, menu
selection/presentation, controller debounce or input cadence, pixels, audio, text timing,
cross-process SRAM persistence, or power-loss behavior.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master`
`c834c652b6862bc5679fd7f69a38a7093206efc6`; sources
`code/specialscreens/witch/witchstart.asm`, `code/common/stats/newgame.asm`,
`code/gameflow/special/configurationmode.asm`, and the three jump-interface source files named in
the fixture; H1 listing `build/sf2build-h1.lst`; command `uv run sf2 h3 witch-new-game-lifecycle`;
fixture `tests/fixtures/h3/witch-new-game-lifecycle-v1.json`; schemas
`schemas/h3-witch-new-game-lifecycle-observation.schema.json` and
`schemas/h3-witch-new-game-lifecycle-fixture.schema.json`.

### Grouped H3 runtime-question queue

- `witch-save-actions/cross-process-persistence-and-recovery`: **Unknown** whether these in-process
  SRAM writes survive a new emulator process or physical medium cycle, and how interruption, partial
  writes, or power loss affect recovery.
- `witch-save-menu/player-driven-name-entry-and-editing`: **Unknown** player-driven name-entry/editing
  behavior; the New lifecycle matrix returns immediately from the NameAlly alias.
- `witch-save-menu/player-driven-menu-presentation-and-input-cadence`: **Unknown** player-driven menu
  presentation, pixels, audio, controller cadence, and debounce; the matrix injects menu results and
  only pulses C to release text waits.
- `witch-save-menu-suspend/presentation-and-input-timing`: **Unknown** prompt/file rendering,
  pixels, audio, input cadence, blink/bubble timing, and suspend presentation/reset timing.

The witch rendering helpers own screen construction, layout-zone DMA, head updates, blink VInt, and
speech-bubble/menu presentation. The suspend path sleeps 60 frames before presenting its resources.
After the witch dialogue it waits at most 600 frames for Start, fades out, and resets through the
original start vector; Start can end that wait early.

The witch menu's direct presentation data is now independently closed. A 32-byte palette contains
16 colors (15 unique, two zero entries) and is copied to `PALETTE_2_CURRENT` before queued CRAM DMA.
The adjacent 960-byte table contains four option groups, three unique 80-byte frames per option, and
each frame is a 5×8 tile-word grid. Both resources, their two longword pointers, and all 1,000 source
bytes match H1 and ROM; the compressed speech-balloon stream remains owned by the existing graphics
rail rather than being counted twice.

`DrawWitchMenuBubble` applies `-$5D00` to every table word. All 480 adjusted words select palette 2
with priority, half use mirror, half use flip, and their 60 tile indices span 1024-1083. Unselected
options use frame zero. For the selected option, timer states 1-4/5-9/10-14/15-20 select frames
0/1/2/1, then the menu loop resets the timer to 20. This is a confirmed control-flow phase table;
perceived pacing, CRAM timing, window motion, and final pixels remain runtime questions.

## Ending Screens

The ending-witch path owns the falling-jewels and witch-blink VInt functions and connects to the end
game sequence. The ending-kiss path owns a data-driven pixel-filling renderer. Ending-jewel,
ending-witch, and ending-kiss resource labels are all part of the canonical resource map. This
establishes ownership and routing, not visual parity.

## Concentrated Runtime Queue

The witch service/control-flow matrix is now observed above. The remaining non-witch presentation
questions are retained as two shared matrices:

1. Sega logo, title, cheat sequences, and Start timing;
2. ending kiss pixel fill, falling jewels, and ending-witch presentation.

The same launches should sample the five fixed-transfer tails before DMA so their contents and
stability are answered together with rendered parity.

Each matrix should capture compact frame/state hashes for several phases in one launch instead of
creating a separate fixture per animation.

## Reproduction

```powershell
uv run sf2 h2 special-screens
uv run sf2 h2 special-screen-graphics
uv run sf2 h2 special-screen-presentation
uv run sf2 h2 witch-menu-graphics
uv run sf2 h3 witch-save-actions
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/special-screens-static.json` and
`local/derived/special-screen-graphics-decode.json`, plus
`local/derived/special-screen-presentation-static.json` and
`local/derived/witch-menu-graphics-static.json`.
