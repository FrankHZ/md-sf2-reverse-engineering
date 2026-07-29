# Common Scripting Engines

- Status: **Confirmed** for the pinned 29-file inventory, the complete 90-slot map-script macro/
  dispatcher/handler/source-use contract, the 80-slot entity-script dispatch table, interpreter
  admission/termination rules, text-bank selection, complete context-
  Huffman tree corpus, all 17 compressed text banks/4,267 decoded records, and
  the regular entity map-sprite decode/DMA consumer shape, the complete six-command map-script dialogue
  family/program-reference/handler/consumer contract, the complete five-command map-script transition
  family/program-site/handler/caller contract, the complete six-command map-script roster/death
  family/program-site/handler/caller contract, the complete four-command map-script active-party/AI/
  follower/battle-stat family/program-site/handler/caller contract, the complete three-command map-script
  camera-control family/program-site/handler/caller/service contract, the complete two-command map-script
  block-copy family/program-site/handler/caller contract, the complete four-command map-script entity
  population/reload family/program-site/handler/caller contract, the complete four-command map-script
  entity-placement source/handler/caller contract, the complete 119-row sprite-dialogue
  property table and its lookup/default rules, plus the complete variable-width font, ASCII
  conversion, pointer, and glyph-loader data path, and the complete three-shared/75-distributed
  entity-action source corpus, and the 13-case/20-tick entity movement runtime matrix
- Status: **Inferred** for named helper intent where only call structure is modeled
- Status: **Unknown** for caller-dependent story meaning, text rendering timing,
  and individual script content
- Evidence date: 2026-07-28
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Confirmed Inventory

The recursive `code/common/scripting` boundary contains 29 files, 11,153 source lines, 888 global
labels, and 576 direct call sites across entity, map, text, and end-credit helpers. Twenty-eight files
have a representative global symbol bound to the H1 listing. They now own 35 indexed findings: one
representative record per labeled file plus seven deeper map-setup records in shared scripting
sources. Record count and indexed-file count are intentionally separate denominators. The remaining
`text/unused_textfunctionsdata.asm` is exactly 288 `dc.b` directives over annotated ROM range
`$6D74..$6E94`; because it has no global label, it is verified by the H2 inventory but deliberately
excluded from strict symbol-based file reach.

## Confirmed Interpreters

`ExecuteMapScript` consumes word commands and ends on `$FFFF`. A negative command sleeps for its low
byte; P2 Start under debug mode sets the skip flag, bypassing dialogue and those sleep commands.
Nonnegative commands select one of 90 table slots. Eight slots route to the shared no-op target.
Return waits for outstanding view scroll when a dialogue window is open and clears view speed.

The map-script command boundary is now fully static rather than represented by the table shape alone.
The 90 slots contain 82 non-filler opcodes and eight filler slots at indices 56, 76-79, and 87-89.
They resolve to 83 unique handlers: one shared filler plus one handler for every non-filler opcode.
The 180-byte relative jump table matches the pinned H1 addresses and input ROM; its SHA-256 is
`B128F068249EABC9443A0363BAFAEC1D9B4E06D6BE21B32867C265C7CC405CDE`.

The macro ABI contains 82 primary command macros, eight aliases, and three non-dispatched forms:
`csWait` encodes the negative sleep word, `cscNop` emits no bytes, and `csc_end` emits `$FFFF`.
All 93 forms are scanned across every code/data ASM file. The original corpus has 13,515 invocations
in 169 files and uses 82 forms; eleven defined forms are absent. The catalog retains explicit zero
counts rather than making unused macros disappear. The most common forms are `nextSingleText`
(2,058), `csWait` (1,591), `setFacing` (1,579), `setActscriptWait` (1,015), and
`entityActionsWait` (957).

The 82 primary command layouts contain 133 logical parameters and exactly 133 emitted operand fields
covering 234 bytes. Including each two-byte opcode, 17 commands are two bytes, 27 are four bytes, 24
are six bytes, and 14 are eight bytes. Operand rows retain stream offset, byte width, raw expression,
logical parameter ordinal, and direct-versus-shorthand encoding. This matters for `animEntityFX`:
its entity word plus `defineShorthand.w ENTITY_TRANSITION_` word make the command six bytes; a parser
that counts only `dc.*` directives silently undercounts it by two. Alias layouts substitute constants
without changing physical width: for example, `setF` preserves both `csc10` operand words while
fixing the second to `$FFFF`.

Script-cursor topology is also closed for every unique handler. Seventy-seven return with sequential
cursor ownership, `csc0B_jump` replaces A6 with one absolute target, four condition handlers choose
between that absolute target and a four-byte skip, and `csc14_setEntityActscriptManual` installs the
inline action-program pointer then scans through its `$8080` terminator. This classification concerns
encoded cursor transfer; synchronous sleeps, camera waits, dialogue, and visible effects inside a
handler remain separate timing behavior.

The complete source corpus is now owned at program level. All 13,515 tracked invocations in 169 files
belong to exactly one of 304 programs and 348 program labels. Three hundred three programs terminate
with `csc_end`; `cs_5DE22` is the sole physical tail that terminates by jumping to another program.
Program sizes range from two through 797 tracked commands. Their macros emit 61,020 map-command bytes;
that byte total deliberately excludes entity-action payloads embedded after `customActscript` or
`entityActions` headers because those payloads have their own H2 owner.

Every encoded control target resolves. The eleven unconditional jumps divide into seven same-program
and four cross-program edges; the 51 conditional jumps divide into 35 same-program and sixteen
cross-program edges. All 122 `executeSubroutine` targets resolve to 68000 symbols outside the map-
script program graph. Thus the corpus has 184 explicit transfers with no unowned script target.
This proves graph topology, not which flag-dependent route is reachable in a particular save state.

## Confirmed Map-Script Dialogue Command Family

The `sf2-map-script-engine-static-v1` contract now isolates `nextSingleText`, `nextSingleTextVar`,
`nextText`, `nextTextVar`, `textCursor`, and `hideText` without re-parsing a partial source corpus.
It retains their exact `$00..$04/$09` dispatcher binding, macro operand layout and physical width, and
an ordered compact reference to every one of their 2,883 existing commands: 2,058/0/577/0/234/14 in
that macro order. The reference is grouped by program command index, while zero-inclusive counts retain
all 304 programs. This distinction prevents 2,883 copied command records from becoming a second source
of truth.

The four display handlers compare the packed modifier/entity word with `-1`, call
`csc1D_showPortrait` before `GetEntityPortaitAndSpeechSfx`, call `DisplayText`, then increment
`CUTSCENE_DIALOG_INDEX`. `csc00`/`csc02` alone preserve the skip-flag test and branch; the two single
handlers then close portrait, clear text, and call `Sleep` with source immediate 10, while the
continuing forms do not contain that sequence. The two `*Var` named sections each read two words into
the source-named dialogue-name indices. `textCursor` writes its word to `CUTSCENE_DIALOG_INDEX`, and
`hideText` orders the close call before text clear. These are instruction-order facts from the named
handler sections, not a claim about rendered text, waits, or player-visible timing.

The bounded caller domain retains all six dialogue handlers plus `csc1D_showPortrait` in source order,
including the zero/zero rows for `csc04_setTextIndex` and `csc09_hideDialogueAndPortraitWindows`.
Each row has separate direct-instruction and resolved-effective target maps for
`csc1D_showPortrait` and `GetEntityPortaitAndSpeechSfx`; they are equal for this direct-call corpus but
remain separately recorded. Both total maps are 4/5, while path-derived internal/external totals are
4/0 and 0/5 respectively: the portrait helper's parsed source path is within the bounded map-script
handler surface and the entity consumer's parsed source path is outside it.

`csc1D_showPortrait` reads the packed word and tests bits 15 then 14; those parsed handler use-sites
derive the high-byte `handlerTestedModifierByteMask` `$C0`. Every observed modifier byte outside the
packed-word `$FFFF` sentinel has no bits outside that use-site-derived mask; the raw source domain and
counts are retained as observations rather than the mask's authority. The two macro comments preserve
original modifier labels `$40` `mirrored`, `$80` `display on right`, and `$FF` `undisplayed`; the
contract records those labels separately from both the tested word bits and the full-word sentinel. The
source/ROM-backed text-line domain is contiguous 0..4,266, so all 234 explicit cursor values
(240..4,233) are within it. The
consumer first masks the entity/character value with parsed `COMBATANT_MASK_ALL` 255, then loads its
map-sprite byte. Its only table join is by the sibling 119-row sprite-dialogue contract's identity,
pinned commit, ROM hash, source path, and addresses; no decoded text or sibling fixture data is copied.

The grouped H3 queue has one dialogue item: `dialogue-presentation/runtime-matrix`, covering skip,
packed modifier/entity handling, portrait/speech presentation, input/wait cadence, and close/clear/sleep
observation under shared setup. Story meaning and visible timing remain **Unknown**.

## Confirmed Map-Script Transition Command Family

The same `sf2-map-script-engine-static-v1` fixture now separately retains the source-named primary
forms `warp`, `resetMap`, `loadMapFadeIn`, `reloadMap`, and `mapLoad`. **Confirmed:** their dispatcher
opcodes are `$07/$36/$37/$46/$48`, their macro byte lengths are 6/2/8/6/8, and their complete ordered
source-site counts are 38/7/60/24/17 (146 total). Every site stays in its original program and command
order, while the zero-inclusive per-program totals retain all 304 programs. `warp` retains four byte
operands; `loadMapFadeIn` and `mapLoad` retain three words; `reloadMap` retains two words; and
`resetMap` has no operand. This is a physical encoded-stream fact, not a statement about frame timing.

**Confirmed:** each site keeps the original destination-map operand text and resolved value separately.
The accepted declared-map domain is the sibling 79-map `sf2-map-content-static-v1` identity set
0..78; the only non-member source form accepted here is the exact `MAP_CURRENT` token, resolved from
`sf2enums.asm` to 255 and recorded as `source-map-current`. The contract does not assign a runtime
lifecycle meaning to that sentinel. `warp` additionally retains its source facing value, but no facing
or camera behavior is inferred from the byte/word layout.

**Confirmed** named-section guards preserve the instruction order rather than merely finding symbols
elsewhere in a file. `csc07_warp` writes the parsed `MAP_EVENT_WARP` value 1 to source-named
`MAP_EVENT_TYPE`, clears the following byte, copies its four bytes from A6 in order, then returns.
`csc36_resetMap` saves A6, directly calls `ResetCurrentMap`, restores A6, then returns.
`csc37_loadMapAndFadeIn` has no return in its own named section and its next named section is
`csc48_loadMap`; that fall-through is recorded without claiming a visible fade duration or result.
`csc48_loadMap` reads the first map word before `LoadMapTilesets`, then reads three words, uses parsed
packed-coordinate multiplier 3, calls `LoadMap`, then `EnableDisplayAndInterrupts`. `csc46_reloadMap`
uses parsed D1 immediate -1, reads two words, uses the same independently parsed multiplier 3, calls
`LoadMap`, then `EnableDisplayAndInterrupts`. The shared multiplier is guarded as an equality between
the two named source use sites; changing one operand fails parser construction before a fixture check.

The bounded caller inventory is **Confirmed** and instruction-scoped: it preserves five caller rows,
four target identities, and zero rows. Direct and effective targets are equal because no used call is a
jump-interface alias. The complete effective totals are `ResetCurrentMap` 1, `LoadMapTilesets` 1,
`LoadMap` 2, and `EnableDisplayAndInterrupts` 2; all four are external to the bounded handler surface,
so internal effective totals are explicitly zero. Comment text, operands, labels, and near-miss
mnemonics are not call sites.

The sole grouped H3 question is **Unknown**:
`map-script-transition-presentation-matrix`. One shared launch should observe event consumption,
map/camera state, fade/display timing, and caller-dependent presentation for representative forms;
the static source contract does not establish any of those runtime outcomes.

Provenance: pinned `ShiningForceCentral/SF2DISASM` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`, `master`,
`sf2cutscenemacros.asm`,
`code/common/scripting/map/mapscriptengine_1.asm` (`csc36`, `csc37`, `csc46`, `csc48`),
`mapscriptengine_2.asm` (`csc07` and dispatcher), `sf2enums.asm`, the H1 listing addresses, and the
US ROM SHA-256 in the fixture. Reproduce with `uv run sf2 h2 map-script-engine`; the observed output
is fixture ID `sf2-map-script-engine-static-v1`, transition field `transitionCommandFacts`.

## Confirmed Map-Script Camera-Control Command Family

Evidence date: 2026-07-28.

**Confirmed:** `sf2-map-script-engine-static-v1` field
`expected.mapCameraControlCommandFacts` retains the three source-named macro forms in source order:
`setCameraEntity` opcode `$24` (125 commands, 4 encoded bytes, 2 operand bytes), `setCamDest`
`$32` (247, 6, 4), and `cameraSpeed` `$45` (43, 4, 2). Their raw macro comments remain part
of the physical ABI: `target entity`; `X (left border)` then `Y (top border)`; and
`($8-, $10-, $20-, $28-, $30-, $38-, $40-)`, respectively. The complete ordered source corpus
contains 415 commands in 123 non-empty program source groups, with all 304 declared programs retained
as zero-inclusive total rows. Its compact source-order SHA-256 is
`C285A849AEB914FCCBF0E52D33D84936260120F4AC50DC4D27C2A070031C211A`; its complete program-total
SHA-256 is `E3F10FDFC69E617255D52DF4ED4FF12B42E8DA496C59DE1D948227D9EBB50EA9`.

**Confirmed:** smallest named-section guards retain source instruction order rather than only symbol
presence. At H1 `$46C38`, `csc24_setCameraTargetEntity` advances A6 by one word into D0, has a
`bmi.w loc_46C52` whose parsed target is the later `move.b d0,VIEW_TARGET_ENTITY` write (statement
index 8), then has `tst.b d0` and `bpl.s @Ally` whose parsed target is the `andi.w #BYTE_MASK,d0`
statement (index 6). The guarded path also retains the parsed
`ENTITY_ENEMY_INDEX_DIFFERENCE` value 96, `BYTE_MASK` value 255, `ENTITY_INDEX_LIST` byte lookup,
and the final source-named state write. At `$46506`, `csc32_setCameraDestInTiles` first writes literal
`-1` to `VIEW_TARGET_ENTITY`, then advances A6 by two word reads into D2 and D3, calls
`j_SetCameraDestination`, calls `WaitForViewScrollEnd`, and returns in that order. At `$46700`,
`csc45_cameraSpeed` advances A6 by one word directly into `VIEW_SCROLLING_SPEED`, then has its
source `nop` and return. These are source instruction facts, not evidence for target selection,
coordinate units, motion, or presentation.

**Confirmed:** `j_SetCameraDestination` remains the direct instruction identity while its pinned
jump-interface definition resolves to effective target `SetCameraDestination`; the other direct and
effective identity is `WaitForViewScrollEnd`. Per-handler instruction/effective maps are zero/zero for
`csc24` and `csc45` and one/one for `csc32`; both direct-target totals are one and both effective-target
totals are one. The complete internal maps are zero-inclusive zeros, while the corresponding external
maps hold the one calls. The effective `SetCameraDestination` named section has two parsed
`mulu.w #MAP_TILE_SIZE` use sites, one for each of D2 and D3; both resolve through the authoritative
constants map to 384 before its `SetViewDestination` call. The parsed multiplication records a
source relationship only; it does not establish a coordinate unit or display result.

**Unknown:** `map-script-camera-control/runtime-effects-matrix` is the sole grouped H3 queue. One
shared launch must determine target-selection meaning, destination interpretation and units, speed
effect, scroll/wait timing, normal-story reachability, and VDP-visible presentation. This static slice
does not promote macro or state-symbol names into any of those runtime outcomes.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `sf2cutscenemacros.asm` macro definitions;
`code/common/scripting/map/mapscriptengine_1.asm` named sections `csc24_setCameraTargetEntity`,
`csc32_setCameraDestInTiles`, and `csc45_cameraSpeed`; jump-interface
`code/common/tech/jumpinterfaces/s05_jumpinterface.asm::j_SetCameraDestination`; effective helper
`code/gameflow/battle/battlefunctions/battlefunctions_0.asm::SetCameraDestination`; and named
`WaitForViewScrollEnd`/`SetViewDestination` owner files recorded in the fixture source-identity joins.
The input identity is the local US ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`. Reproduce with
`uv run sf2 h2 map-script-engine`; observed result is
`tests/fixtures/h2/map-script-engine-static-v1.json`, fixture ID
`sf2-map-script-engine-static-v1`, field `expected.mapCameraControlCommandFacts`.

## Confirmed Map-Script Entity-Placement Command Family

Evidence date: 2026-07-28.

**Confirmed:** `sf2-map-script-engine-static-v1` field `entityPlacementCommandFacts` retains four
source-named macro forms in macro source order: `setPos` `$19` (608 sites, 6 encoded bytes, 4 operand
bytes), `setPosFlash` `$17` (2, 6, 4), `setFacing` `$23` (1,579, 4, 2), and `setDest` `$29` (99, 8,
6). Their byte/word operand widths and raw comments remain separate source facts: the two four-byte
forms label `entity to act`, `X`, `Y`, and `facing`; `setFacing` labels `entity to act` and `facing`;
and `setDest` labels three word fields `entity to act`, `X`, and `Y`. These source labels and widths do
not establish a coordinate unit, placement, facing, animation, visibility, or player-visible result.

**Confirmed:** the complete bounded corpus has 2,288 command occurrences in 204 non-empty source
program groups, while its 304 `programTotals` rows are zero-inclusive. The extractor keeps complete
source/program order separately and pins the ordered source-site and program-total corpora with SHA-256
`C451E4B4F2B154D9B01F7321E288D1E9DEC16A656E55730826C9E1800BE64734` and
`5AE7802BB7D93463304AE491B89F136C763AF0E3BAF1EC85877F68E24867B388`. The tracked fixture uses these
compact order/hash constraints rather than copying the large source-site or program-total records.

**Confirmed:** the four named handler sections are H1/ROM `csc19_setEntityPosAndFacing` `$46A32`
(18 statements), `csc17_setEntityPosAndFacingWithFlash` `$469CC` (17), `csc23_setEntityFacing`
`$46C60` (8), and `csc29_setEntityDest` `$46DB8` (29). `csc19` and `csc23` each preserve their
non-advancing A6 selector read, parsed `moveq #4,d7` or `moveq #2,d7`, and ordered
`AdjustScriptPointerByCharacterAliveStatus` call before their advancing operand reads. `csc17` has no
local return: its guarded `bra.w csc19_setEntityPosAndFacing` shared-tail edge retains the target
handler and first instruction, while its two local branches resolve to `loc_469D0`/`add.w d0,d0` and
`loc_469BA`/`move.w d2,(a5)`. `csc29` separately resolves its two `bpl.s` targets and its `bne.s`
return target. These are control-flow records only, not evidence for an effect of the source names.

**Confirmed:** all source reads and writes remain source-shaped operands. `csc19` records its `(a5)`,
`ENTITYDEF_OFFSET_XDEST`, `ENTITYDEF_OFFSET_Y`, `ENTITYDEF_OFFSET_YDEST`, and
`ENTITYDEF_OFFSET_FACING` writes; `csc23` records its facing-offset write; `csc17` records its two
`(a5)` writes; and `csc29` records its `(a5)`/`ENTITYDEF_OFFSET_Y` reads and six destination/travel/
velocity-named writes. The two `mulu.w #MAP_TILE_SIZE` use sites in both `csc19` and `csc29` resolve the
single parsed `sf2enums.asm` equate to 384. The literal `$FE80`, 30, 15, 1, 2, 32, and `$F` records are
kept as parsed source immediates; no lifecycle, unit, or hardware interpretation is assigned to them.

**Confirmed:** the comment-stripping instruction parser retains six direct instruction identities and
zero-inclusive per-handler maps. Aggregate direct/effective totals are
`AdjustScriptPointerByCharacterAliveStatus` 2, `GetEntityAddressFromCharacter` 4,
`UpdateEntitySprite_0` 2, `WaitForVInt` 2, `Sleep` 1, and `WaitForEntityToStopMoving` 1. No current
call instruction is a jump-interface alias, so instruction and effective identities are equal; all
six internal maps are explicitly zero and the external maps retain the totals. Labels, comments,
near-miss mnemonics, and operands do not count as calls. The provenance join independently binds the
`UpdateEntitySprite_0` wrapper's `jsr (ChangeEntityMapsprite).w` to the existing
`sf2-entity-action-scripts-static-v1` fixture, which separately records `UpdateEntityData` and
`ChangeEntityMapsprite`; it does not assert a runtime call edge to `UpdateEntityData`.

### Runtime questions — entity placement

**Unknown:** `map-script-entity-placement/runtime-effects-reachability-matrix` is the sole grouped H3
queue. One shared launch must determine normal-story reachability; alive-status cursor consequences;
the meaning and units of all operands; resulting entity state; animation/visibility/presentation;
wait/sleep/VInt timing; map/collision/pathfinding interaction; and persistence. This static contract
does not promote any of those outcomes from macro, field, or callee names.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` macros `setPos`,
`setPosFlash`, `setFacing`, and `setDest`; `code/common/scripting/map/mapscriptengine_1.asm` named
sections and helper symbols above; `sf2enums.asm::MAP_TILE_SIZE`; H1 listing addresses; and local US
ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`. Reproduce with
`uv run sf2 h2 map-script-engine`; observed result is fixture ID
`sf2-map-script-engine-static-v1` in `tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.entityPlacementCommandFacts`.

## Confirmed Map-Script to Entity-Action Bridge Command Family

Evidence date: 2026-07-28.

**Confirmed:** `sf2-map-script-engine-static-v1` field
`expected.entityActionBridgeCommandFacts` retains six source-named forms in macro source order:
`setActscriptWait` and `setActscript` emit `$15` with 8 encoded bytes (1,015 and 436 sites);
`customActscriptWait` and `customActscript` emit `$14` with 4 encoded bytes (359 and 2); and
`entityActionsWait` and `entityActions` emit `$2D` with 4 encoded bytes (957 and 487). In each
pair, the source control byte is exactly `$FF` for the `Wait` spelling and `0` for the other spelling;
the selector byte remains a separately named first operand. These physical layouts and source names do
not establish a wait result, an entity lifecycle, or any other runtime effect.

**Confirmed:** the complete ordered corpus has 3,256 command occurrences in 196 non-empty source-site
rows and retains all 304 declared programs as zero-inclusive total rows. The compact exact-order
constraints hash those rows as
`7C4BC190E467C5DBEE90092D2443A333AA69F52296352C550BF8A263B4D542F8` and the total rows as
`C22323C27AFC8BD2F6DFAFA721F26F152582A7AF0D9A23B11CDCBCE2DF5D648F`. The raw corpus separates 1,451
no-inline-payload sites, 361 `ac_*`-macro stream sites, and 1,444 entity-action byte-stream sites;
the latter two retain their source terminator spelling and physical cursor advances instead of treating
a terminator word, a payload byte count, and an A6 cursor advance as one quantity. For every
`customActscript*` source site, the guarded `csc14` word compare supplies a two-byte scan transfer;
the extractor derives its exact scan iteration count from the encoded payload bytes, requires word
alignment, records the same payload-byte cursor advance, and records the separate two-byte terminator
advance. This is distinct from csc2D's two-byte interpreted-command read.

**Confirmed:** the bounded named sections are H1/ROM `csc14_setEntityActscriptManual` `$46950` (12
statements), `csc15_setEntityActscript` `$46978` (10), and `csc2D_entityActionSequence` `$467E2`
(18 plus its guarded terminal chunk). The guards retain instruction order, A6 read/capture/skip widths,
branch polarities and resolved target-first-instruction pairs, and direct-call order. `csc14` checks
the exact `$8080` word before the two-byte cursor advance; `csc15` takes a four-byte primary operand
read; and `csc2D` retains the parsed `BYTE_LOWER_NIBBLE_MASK` value 15, its indexed-PC
`rjt_EntityMoveCommands` call, terminal `$34`/`eas_Idle` records, and tail transfer. These are static
source records, not a decoded instruction or behavior claim. The csc2D tail transfer is separately
resolved as `bra.s loc_467FC` to that label's first `move.b (a6)+,d1` instruction.

**Confirmed:** comment-stripping instruction parsing records three direct
`GetEntityAddressFromCharacter` sites and one indexed-PC `rjt_EntityMoveCommands` site. The complete
declared direct/effective-target maps are zero-inclusive: their internal totals are zero, while the
external totals are respectively 3 and 1; no current bridge call is a jump-interface alias. The
source-identity joins preserve the independent `sf2-entity-action-scripts-static-v1` `ac_end` `$8080`
terminator fact and four `sf2-map-events-static-v1` map-44 opener/terminator context pairs. They do not
claim that the map-event or entity-action code runs in a particular runtime context.

### Runtime questions — map-script entity-action bridge

**Unknown:** `map-script-entity-action-bridge/runtime-effects-reachability-matrix` is the sole grouped
H3 queue. One shared launch must determine normal-story reachability; the meaning of the source control
byte; payload interpretation and termination; resulting entity/action state; timing; persistence;
map/collision interaction; and visible presentation. This static contract does not promote any of those
outcomes from macro, state-field, handler, or callee names.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` macros `csc14`, `csc15`,
`csc2D`, the six aliases, and `ac_end`/`endActions`; `code/common/scripting/map/mapscriptengine_1.asm`
named sections above and `rjt_EntityMoveCommands`; the joined fixture source locations; H1 listing
addresses; and local US ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Reproduce with `uv run sf2 h2 map-script-engine`; observed result is fixture ID
`sf2-map-script-engine-static-v1` in `tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.entityActionBridgeCommandFacts`.

## Confirmed Map-Script Entity Lifecycle/Presentation Command Family

Evidence date: 2026-07-28.

**Confirmed:** `sf2-map-script-engine-static-v1` field
`expected.entityLifecyclePresentationCommandFacts` retains eight source-named macro forms in source
order: `hide` `$2E` (141 commands, 4 encoded bytes, 2 operand bytes), `startEntity` `$1B` (70, 4,
2), `stopEntity` `$1C` (107, 4, 2), `waitIdle` `$16` (30, 4, 2), `setSprite` `$1A` (56, 6, 4),
`setPriority` `$53` (51, 6, 4), `removeShadow` `$30` (5, 4, 2), and `setSize` `$50` (4, 6, 4).
The raw macro comments and byte/word widths remain source facts: first operand labels are exactly
`entity to act`, `entity`, or `target entity`, while `setSprite`, `setPriority`, and `setSize` retain
their second source labels. These spellings and physical fields do not establish visibility,
animation, priority, shadow, size, or any other runtime effect.

**Confirmed:** the complete source corpus contains 464 ordered command occurrences in 105 non-empty
source-site rows, and all 304 declared programs remain as zero-inclusive `programTotals` rows. The
extractor separately records command/program order and exact raw operand records; the source-site and
program-total order hashes are respectively
`152416D18046AC324FCF0EBA3F148B82D723FAF03705698B58565F17935E88AD` and
`0ADCBF8A1207FD628CBC63B8BCD028F9426D585E97F29271FB0A23904F05EA3C`.

**Confirmed:** the eight bounded H1/ROM handler sections are `csc2E_hideEntity` `$46E9A` (4
statements), `csc1B_startEntityAnim` `$46A6C` (7), `csc1C_stopEntityAnim` `$46A82` (7),
`csc16_waitUntilEntityIdle` `$4699A` (5), `csc1A_setEntitySprite` `$46A48` (11),
`csc53_setPriority` `$46FBE` (10), `csc30_removeEntityShadow` `$46EC0` (8), and
`csc50_setEntitySize` `$46EE0` (9). Their guards retain complete statement order, A6 transfer versus
non-advancing selector reads, literal use sites, source-shaped read/write operands, branch polarity and
resolved target-first-instruction pairs, direct call order, and return boundaries. In particular,
`setSprite` retains the parsed `COMBATANT_ALLIES_NUMBER` comparison use site, and `setSize` retains
the parsed `%1000` immediate as value 8 and bit index 3; neither record assigns a gameplay meaning to
the source field or bit.

**Confirmed:** comment-stripping instruction parsing retains nine direct instruction targets with
zero-inclusive per-handler, direct/effective, internal/external maps. External totals are
`GetEntityAddressFromCharacter` 8, `HideEntity` 1,
`AdjustScriptPointerByCharacterAliveStatus` 2, `GetAllyMapsprite` 1, `WaitForVInt` 3,
`UpdateEntitySprite_0` 2, `LoadMapsprite` 1, `sub_45A8C` 1, and `DmaMapsprite` 1; every internal
total is zero and no call currently resolves through a jump-interface alias. The provenance-only joins
retain fixture IDs `sf2-entity-action-scripts-static-v1`, `sf2-map-sprite-assignments-static-v1`, and
`sf2-sprite-dialogue-static-v1`; they do not claim that their associated data or code has a particular
runtime effect here.

### Runtime questions — entity lifecycle/presentation

**Unknown:** `map-script-entity-lifecycle-presentation/runtime-effects-reachability-matrix` is the
sole grouped H3 queue. One shared launch must establish normal-story reachability; operand and
state-field meaning; selector handling; resulting entity, animation, visibility, shadow, sprite,
priority, and size outcomes; VInt/wait timing; collision/pathfinding; persistence; and presentation.
No source macro, callee, field, or literal name promotes those runtime behaviors from this static
contract.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` macros `hide`,
`startEntity`, `stopEntity`, `waitIdle`, `setSprite`, `setPriority`, `removeShadow`, and `setSize`;
`code/common/scripting/map/mapscriptengine_1.asm` named sections above; H1 listing symbols/addresses;
and local US ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Reproduce with `uv run sf2 h2 map-script-engine`; observed result is fixture ID
`sf2-map-script-engine-static-v1` in `tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.entityLifecyclePresentationCommandFacts`.

## Confirmed Map-Script Roster/Death Command Family

**Confirmed:** `sf2-map-script-engine-static-v1` separately retains the source-named primary forms
`join`, `jumpIfDefeatedByLastAttack`, `jumpIfDead`, `allyDefeated`, `updateDefeatedAllies`, and
`reviveAlly`. Their dispatcher bindings are `$08/$0E/$0F/$1F/$20/$21`; physical command sizes are
4/8/8/4/2/4 bytes. The six macro layouts retain their byte offsets and widths, while every one of the
304 programs retains a zero-inclusive ordered total. Ordered source-site counts are 34/0/0/5/1/3.
The two zero rows are source-use facts only; they do not claim that either handler is unreachable at
runtime.

**Confirmed:** the source macro labels and handler labels remain distinct: in particular,
`jumpIfDefeatedByLastAttack` dispatches to `csc0E_jumpIfForceMemberInList`, and `jumpIfDead` dispatches
to `csc0F_jumpIfCharacterDead`. Named-section guards cover the exact handler sections at H1/ROM
addresses `$47398`, `$47440`, `$47464`, `$46ADE`, `$46AF0`, and `$46B1A` respectively. They retain A6
word/long cursor reads separately from storage/list accesses, ordered branch mnemonics and polarity,
and mutation/call order; no byte count is promoted into a list capacity.

**Confirmed:** `csc08_joinForce` clears source-named `CURRENT_SPEECH_SFX`, waits for view scrolling,
reads its one word, clears bit 15, then preserves the two music paths in source order. Its numeric
special-selector use site is checked against parsed `COMBATANT_ENEMIES_START` 128; the source-named
`ALLY_SARAH`/`ALLY_CHESTER` calls precede their source text form, while the other path calls
`j_JoinForce` then `j_GetClass` before writing the two source-named dialogue indices. This is an
instruction-order fact, not a claim about music, text, or player-visible presentation.

**Confirmed:** `csc0E_jumpIfForceMemberInList` first decrements the source-named list length and uses
`bcs`, then compares one list byte and uses `beq` to select the A6 long target; the non-match path
loops with `dbf` and skips that long by four bytes. `csc0F_jumpIfCharacterDead` calls
`j_GetCurrentHp`, tests `d1`, and uses `bne`: the fall-through selects the A6 long target and the
branch path skips it. These static branch records preserve source polarity without assigning lifecycle
meaning to either macro name.

**Confirmed:** `csc1F_addDefeatedAlly` indexes from `DEAD_COMBATANTS_LIST_LENGTH`, stores the low byte
of its A6 word operand, then increments that length. `csc21_reviveAlly` separately reads the length,
uses the carry branch for the empty case, compares list bytes, decrements the length on the equality
path, and copies/increments both pointers only on the non-equality path. `csc20_updateDefeatedAllies`
uses the parsed low byte of its `$FFFFFF80` immediate as `COMBATANT_ENEMIES_START` 128, calls
`j_GetCombatantX`, then executes `cmpi.w #-1,d1; beq` before the list write and local length increment.
Thus the write is on the non-equality fall-through path. The nearby source comment is retained as a
source comment only; it is not treated as behavior when it disagrees with this guarded instruction
sequence.

**Confirmed:** the instruction-scoped caller inventory preserves seven direct target identities,
their ordered site counts, and aliases separately from effective targets. The effective totals are
`FadeOut_WaitForP1Input` 1, `GetClass` 1, `GetCombatantX` 1, `GetCurrentHp` 1, `JoinForce` 3, `Sleep`
1, and `WaitForViewScrollEnd` 1. Every six-handler caller row includes zero counts for this complete
target domain. `j_JoinForce` resolves through the pinned `s02_jumpinterface.asm` definition to
`JoinForce`; `j_GetClass`, `j_GetCombatantX`, `j_GetCurrentHp`, and the fade alias retain the same
dual identity. The bounded handler surface contains no effective target implementation, so internal
effective totals are zero and external totals retain the values above. Each resolution records its
parsed `effectiveTargetScope`; the two zero-inclusive scope-total maps are derived from that map and
the effective totals, rather than being fixed all-external constants. Comments, labels, operands,
near-miss mnemonics, and register-indirect calls are not caller sites.

**Confirmed:** the common-stats connection is a provenance identity join, not a copy of another
fixture: `code/common/stats/battleparty.asm`, SHA-256
`670A25075D807BA60B0AA3C6D158DDF80E5248264753361DBC495F7655ED8B37`, exports source labels
`JoinForce` and `UpdateForce` in `sf2-common-stats-static-v1` at the same pinned commit. Only
`JoinForce` is an effective caller target in this six-handler slice; `UpdateForce` remains a retained
source identity for the shared roster boundary, not a fabricated caller count.

**Unknown:** `force-state/roster-death-persistence-visible-outcomes` is the sole grouped H3 queue.
One shared future launch should distinguish roster/list contents, death/revive persistence, and
visible/presentation outcomes across representative caller states. This static slice does not claim
normal-story reachability, save persistence, roster capacity, list capacity, or visible effects.

## Confirmed Map-Script Active-Party/AI/Follower Command Family

Evidence date: 2026-07-27.

**Confirmed:** the same `sf2-map-script-engine-static-v1` field
`forceStateCommandFacts.activePartyCommandFacts` retains the adjacent source-named primary forms
`joinBatParty`, `joinForceAI`, `resetForceBattleStats`, and `addNewFollower`. Their dispatcher bindings
are `$51/$54/$55/$56`; their physical command sizes are 4/6/2/4 bytes; and their complete ordered
source-site counts are 1/4/5/19 (29 total). Every one of the same 304 programs has an ordered,
zero-inclusive total row. These are macro/source-layout facts, not a claim that source comments define
the player-visible meaning of the forms.

**Confirmed:** the four primary macro labels dispatch to `csc51_joinBattleParty` at `$46F02`,
`csc54_joinForceAi` at `$46FDC`, `csc55_resetCharacterBattleStats` at `$47000`, and
`csc56_addFollower` at `$47008`. The four exact section guards preserve each cursor-read width and
the relevant instruction order. `csc51` first writes source literal `-1` to
`DIALOGUE_NAME_INDEX_1`, then reads one word and calls `j_IsInBattleParty`; its guarded mutation/call
summary keeps that initialization before the membership test and the later replacement write before
`LeaveBattleParty` then `JoinBattleParty`. The fall-through updates force state, reads
`BATTLE_PARTY_MEMBERS_NUMBER`, applies source literal `subq.w #2,d7`, and guards the
`GetCurrentHp` zero branch. This keeps the source list symbol, loop literal, state writes, and physical
cursor read distinct; it does not promote any of them into a roster/list capacity claim.

**Confirmed:** `csc54` reads a first word before `j_GetActivationBitfield`, then its second word into
`d2`; the following `bne.s` takes the `ori.w #AIBITFIELD_AI_CONTROLLED,d1; j_JoinForce` path and its
fall-through executes `andi.w #($FFFF-AIBITFIELD_AI_CONTROLLED),d1`. Both paths reach the common
`j_SetActivationBitfield` tail. The parsed `AIBITFIELD_AI_CONTROLLED` source value is 4 and the clear
expression resolves to 65531. The macro comment's “on/off” label is retained only as source text and
is not used as a behavioral interpretation.

**Confirmed:** `csc55` contains exactly `jsr ResetAlliesBattleStats; rts`. `csc56` reads one word,
calls `GetEntityAddressFromCharacter`, initializes `d1`, scans `EXPLORATION_ENTITIES` until the
source literal byte `-1`, and leaves the last observed follower byte in `d1` on the non-sentinel loop
path. Before `AddFollower`, it uses the exact source literals `$FFE8` in `d2` and `0` in `d3`. This is
register/order evidence only; the state lifecycle and user-visible follower behavior remain unassigned.

**Confirmed:** the instruction-scoped caller inventory has 11 direct identities and retains aliases
alongside their effective targets. Every effective target has total 1: `AddFollower`,
`GetActivationBitfield`, `GetCurrentHp`, `GetEntityAddressFromCharacter`, `IsInBattleParty`,
`JoinBattleParty`, `JoinForce`, `LeaveBattleParty`, `ResetAlliesBattleStats`,
`SetActivationBitfield`, and `UpdateForce`. The four per-handler maps retain zero counts across this
complete target domain. Each jump-interface instruction target remains present with its pinned alias
source path; no bounded implementation is an effective target, so derived internal totals are all zero
and external totals retain the values above. Comments, labels, operand text, near-miss mnemonics, and
register-indirect calls do not count as caller sites.

**Confirmed:** the provenance-only source joins retain `sf2-common-stats-static-v1` identities for
`battleparty.asm` (`IsInBattleParty`, `JoinBattleParty`, `JoinForce`, `LeaveBattleParty`, `UpdateForce`),
`combatantstats_1.asm` (`GetActivationBitfield`), and `combatantstats_2.asm`
(`SetActivationBitfield`), plus the direct follower owner
`code/common/scripting/entity/entityfunctions_2.asm` (`AddFollower`) and the direct battle-stats owner
`code/common/scripting/map/resetalliesstats.asm` (`ResetAlliesBattleStats`). The joins carry each
source SHA-256 and do not copy any sibling fixture payload.

**Unknown:** `force-state/active-party-ai-follower-runtime-matrix` is one grouped H3 queue. It must
observe active-party membership, activation-bit effects, reset effects, follower-chain effects, and
their persistence/visible outcomes under shared setup. This static slice claims neither normal-story
reachability nor a lifecycle, capacity, save, hardware, or presentation effect.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `sf2cutscenemacros.asm` forms at lines 474-503;
`code/common/scripting/map/mapscriptengine_1.asm` named sections at lines 1637-1811; the H1 listing
addresses above; `sf2enums.asm::AIBITFIELD_AI_CONTROLLED`; and the follower/battle-stats owner files
named above.
Reproduce with `uv run sf2 h2 map-script-engine`; observed result is fixture
`tests/fixtures/h2/map-script-engine-static-v1.json`, ID `sf2-map-script-engine-static-v1`, nested
field `expected.forceStateCommandFacts.activePartyCommandFacts`.

## Confirmed Map-Script Block-Copy Command Family

Evidence date: 2026-07-27.

**Confirmed:** `sf2-map-script-engine-static-v1` field `mapBlockMutationCommandFacts` retains the
two source-named macro forms in source order: `setBlocks` opcode `$34` (201 sites, 8 encoded bytes)
and `setBlocksVar` opcode `$35` (7 sites, 8 encoded bytes). Both emit a two-byte opcode followed by
six one-byte fields whose unmodified macro-comment labels are `source x`, `source y`, `width`,
`height`, `destination x`, and `destination y`. The 208 source commands retain complete program and
command order through an exact compact order-key sequence plus SHA-256
`063AFC8B1B2FB6B65BB7AA378710F04C95CF7D1FABBF2FCB1A1AD743EFB6B7A7`; all 304 program rows remain
in a zero-inclusive source-total corpus with order/content SHA-256
`71850FBEDC792D65CD15C2B2493E48A3AEA67175A68E6706BF522CB7A180FE57`. The physical six-byte operand
payload, the three two-byte A6 cursor reads, the helper's two-byte `move.w` copy instruction, and the
two 128-byte row-offset additions are recorded as separate quantities; none is a capacity or a
runtime persistence claim.

**Confirmed:** the `$34` dispatcher entry resolves to `csc34_setBlocks` at H1/ROM `$46566` and the
`$35` entry to `csc35_setBlocksVar` at `$46582`. Each exact named section reads three A6 words into
`d0`, `d1`, and `d2`, then directly calls `CopyMapBlocks`. The `$34` section subsequently sets source
bit indices 0 then 1 in source symbol `VIEW_PLANE_UPDATE_TOGGLE_BITFIELD` before returning; the `$35`
section returns immediately after that direct call. This is instruction/cursor/call/bit-set order only:
the target symbol and macro names are retained without interpreting a visual, collision, persistence,
or hardware result.

**Confirmed:** each two-byte cursor read joins exactly one adjacent pair of the parsed byte fields:
`d0` joins `source x`/`source y`, `d1` joins `width`/`height`, and `d2` joins
`destination x`/`destination y`. The called helper `CopyMapBlocks` is at H1 `$03DB0` in
`code/gameflow/exploration/exploration.asm`. Its three parsed `lsr.w #BYTE_SHIFT_COUNT` use sites
resolve `BYTE_SHIFT_COUNT` from `sf2enums.asm` to 8; its two parsed left-shift use sites both use 6;
its paired inner additions both use 2; and its paired outer additions both use 128. The extractor
derives the observed address-row relationship `128 = 2 * 2^6` from those specific use sites and fails
construction if either paired operand, opcode, or order changes. The `d6` and `d7` loop instructions
remain separately recorded as source counters rather than converted into a block count.

**Confirmed:** the comment-stripping instruction parser finds one direct `CopyMapBlocks` call in each
handler. The complete declared target map therefore preserves both handler rows with direct and
effective `CopyMapBlocks` count 1, aggregate direct/effective total 2, internal effective total 0,
and external effective total 2. There is no used jump-interface alias in this two-call boundary.
The only source-identity join is the called helper owner
`code/gameflow/exploration/exploration.asm`, SHA-256
`C38279815C832B5D65B443092048BB92E19FAEE47B81734A3EF0D16AA0E445A0`, symbol `CopyMapBlocks`.
Comments, labels, near-miss mnemonics, and operands do not become caller sites.

**Unknown:** `map-block-mutation/runtime-effects-matrix` is the sole grouped H3 queue. One shared
launch must determine the relevant state mutation, collision/pathfinding interaction, visible-update
timing, and persistence across representative source forms. This static slice does not claim any of
those runtime outcomes.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `sf2cutscenemacros.asm` lines 345-364;
`code/common/scripting/map/mapscriptengine_1.asm` lines 66-91; `sf2enums.asm:9`;
`code/gameflow/exploration/exploration.asm` lines 724-764; H1 addresses `$46566`, `$46582`, and
`$03DB0`; local US-ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`. Reproduce with
`uv run sf2 h2 map-script-engine`; observed result is
`tests/fixtures/h2/map-script-engine-static-v1.json`, ID `sf2-map-script-engine-static-v1`, field
`expected.mapBlockMutationCommandFacts`.

## Confirmed Map-Script Entity Population/Reload Command Family

Evidence date: 2026-07-27.

**Confirmed:** `sf2-map-script-engine-static-v1` field `entityPopulationCommandFacts` retains four
source-named macro forms in source order: `newEntity` opcode `$2B` (18 sites, 8 encoded bytes),
`loadMapEntities` `$42` (69 sites, 6 bytes), `reloadEntities` `$44` (2 sites, 6 bytes), and
`loadEntitiesFromMapSetup` `$49` (7 sites, 8 bytes). `newEntity` emits the two-byte opcode, one
two-byte field whose unmodified source comment is `entity number`, then one-byte fields labeled `X`,
`Y`, `facing`, and `mapsprite`. The `$42` and `$44` forms each emit one four-byte field labeled
`address of entity table`; the three `$49` word fields retain their deliberately blank source comments
rather than receiving invented semantic names. This is a source-layout contract, not a claim that any
name describes a runtime allocation or spawn operation.

**Confirmed:** the complete bounded corpus contains 96 commands in 78 non-empty program groups:
18 `newEntity`, 69 `loadMapEntities`, 2 `reloadEntities`, and 7 `loadEntitiesFromMapSetup`. It retains
the complete ordered command keys and source-site SHA-256
`BE26AD2D93D08929FC28BD451629EC8B275ED3832E24A4D732F033408A0785FD`, plus all 304 program totals,
including zero-use rows, with SHA-256
`45DAE48D41348AE403864F15E2FAD1C30E17637CD3037C03A41BC8105A124F65`. The four named handler
sections are separately guarded: `csc2B_initializeNewEntity` at H1/ROM `$46E38`,
`csc42_loadMapEntities` at `$4668A`, `csc44_reloadEntities` at `$466C8`, and
`csc49_loadEntitiesFromMapSetup` at `$46758`. A source opcode, ordered statement, read size, direct
call, VInt trap record, or source-constant operand mutation makes construction fail before fixture
comparison.

**Confirmed:** `csc2B_initializeNewEntity` consumes A6 reads of 2/1/1/1/1 bytes into D0/D1/D2/D3/D4
after clearing the latter four words, loads source symbol `eas_Init` (H1 `$460CE`) into D5, then directly
calls `InitializeNewEntity`. `csc42_loadMapEntities` uses the source `VINTS_DEACTIVATE` value 3 before
`DisableDisplayAndInterrupts`, consumes one four-byte A6 read into A0 followed by three two-byte A0
reads into D1/D2/D3, calls `InitializeMapEntities`, `LoadEntityMapsprites`, and
`EnableDisplayAndInterrupts` in that order, then uses `VINTS_ACTIVATE` value 4. `csc44_reloadEntities`
has the same paired VInt records, calls `GetEntityAddressFromCharacter` before
`InitializeMapEntities`, and retains its two `divu.w #MAP_TILE_SIZE` use sites (parsed value 384),
`ENTITYDEF_OFFSET_Y` read (2), and `ENTITYDEF_OFFSET_FACING` byte read (16) as separate source facts.
`csc49_loadEntitiesFromMapSetup` brackets its `GetMapSetupEntityList`, three two-byte A6 reads, and
`j_InitializeMapEntities`/`LoadEntityMapsprites` sequence with the same disable/enable and VInt order.
These statements record operands, transfer widths, call order, and labels only; they do not assign a
stored-byte count or a unit to the values.

**Confirmed:** the comment-stripping instruction parser preserves instruction and effective identities.
Across the four handlers, instruction-target totals are `DisableDisplayAndInterrupts` 2,
`EnableDisplayAndInterrupts` 2, `GetEntityAddressFromCharacter` 1, `GetMapSetupEntityList` 1,
`InitializeMapEntities` 2, `InitializeNewEntity` 1, `LoadEntityMapsprites` 2, and
`j_InitializeMapEntities` 1. The last identity resolves through
`code/common/tech/jumpinterfaces/s07_jumpinterface.asm::j_InitializeMapEntities` to effective target
`InitializeMapEntities`, making its effective total 3; all other effective totals match their direct
identity. Each handler retains a zero-inclusive count for the complete declared target domain. No
effective implementation lies inside this four-handler surface, so every internal effective total is
zero and external effective totals equal the effective totals. Comments, labels, near-miss mnemonics,
and operands do not count as callers.

**Unknown:** `entity-population-reload/runtime-effects-matrix` is the sole grouped H3 queue. One shared
launch must determine spawning, slot allocation, capacity, persistence, activation, rendering,
collision/pathfinding, and normal-story reachability for representative forms. The static contract does
not promote source labels, cursor reads, target identity, or VInt records into any of those runtime
claims.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` forms `newEntity`,
`loadMapEntities`, `reloadEntities`, and `loadEntitiesFromMapSetup`; `code/common/scripting/map/mapscriptengine_1.asm`
symbols `csc2B_initializeNewEntity`, `csc42_loadMapEntities`,
`csc44_reloadEntities`, and `csc49_loadEntitiesFromMapSetup`; `sf2enums.asm`;
`code/common/tech/jumpinterfaces/s07_jumpinterface.asm::j_InitializeMapEntities`; the H1 symbols above;
and local US-ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The extractor additionally records the pinned callee-owner paths and SHA-256 values in
`sourceIdentityJoins`. Reproduce with `uv run sf2 h2 map-script-engine`; observed result is
`tests/fixtures/h2/map-script-engine-static-v1.json`, ID `sf2-map-script-engine-static-v1`, field
`expected.entityPopulationCommandFacts`.

## Confirmed Map-Script Map Load/Reload/Reset Command Family

Evidence date: 2026-07-28.

**Confirmed:** `sf2-map-script-engine-static-v1` field `mapLifecycleCommandFacts` keeps four
source-faithful forms in macro source order: `resetMap` opcode `$36` (7 sites, 2 encoded bytes),
`loadMapFadeIn` `$37` (60 sites, 8 bytes), `reloadMap` `$46` (24 sites, 6 bytes), and `mapLoad`
`$48` (17 sites, 8 bytes). The two three-word forms retain the source comments `map`, `camera X`,
and `camera Y`; `reloadMap` retains `camera X` and `camera Y`; and `resetMap` has no operand
field. The contract preserves source text and physical widths, not a claim that these operand labels
describe runtime placement or a persistent map state.

**Confirmed:** the bounded corpus has 108 commands in 81 non-empty source program groups and a
complete 304-row zero-inclusive program inventory. Its ordered source-site SHA-256 is
`4F07DC2BD06A9E326A61CF43867FCBD2027BC35E76FAD9EEE09B622E11DC13A8`; its ordered program-total
SHA-256 is `E553A857B356B1B334DE3C68DAD17D966C46C6788EADB62EAFD6A966D6EBEB84`. The map-comment
operands are independently checked against the existing 79-ID canonical map domain, while their raw
text and the source `MAP_CURRENT` value remain distinct facts.

**Confirmed:** the four exact named handler sections are `csc36_resetMap` at H1/ROM `$4658E`,
`csc37_loadMapAndFadeIn` at `$4659A`, `csc46_reloadMap` at `$46708`, and `csc48_loadMap` at
`$465B6`. `csc36` saves A6, directly calls `ResetCurrentMap`, restores A6, and returns. `csc37`
performs five source-named state writes in order—using parsed `OUT_TO_BLACK` value 2 in its first
write—and has no return before the physical `csc48` section. Its retained continuation is therefore
the exact `csc48` section, not an inferred call. `csc48` retains a non-advancing A6 word probe into
D1 before `LoadMapTilesets`, then `WaitForVInt`, `tst.b` of the source `FADING_SETTING`, and
`bne.s loc_465C4`; the bounded section parser resolves that target label to the immediate first
`jsr (WaitForVInt).w` at normalized statement index 4. It then records VInt deactivate, three
advancing A6 word reads, and the packed D0 sequence
`lsl.w #BYTE_SHIFT_COUNT,d0` (8), `andi.w #BYTE_MASK,d2` (255), `or.w d2,d0`, and `mulu.w #3,d0`.
It saves/restores A6 around `LoadMap`, calls `EnableDisplayAndInterrupts`, activates VInt, waits once,
then returns. `csc46` has its own two advancing word reads, `moveq #-1,d1`, the same parsed
shift/mask/merge/multiply use sites, `LoadMap`/`EnableDisplayAndInterrupts`/`WaitForVInt` order, and
paired VInt records (`VINTS_DEACTIVATE` 3, `VINTS_ACTIVATE` 4). These are guarded instruction,
branch-polarity, cursor-transfer, and call-order records; they do not establish display or fade
behavior.

**Confirmed:** the comment-stripping direct-call parser retains physical instruction identity and a
zero-inclusive declared target domain. Aggregate direct/effective totals are `ResetCurrentMap` 1,
`LoadMapTilesets` 1, `LoadMap` 2, `EnableDisplayAndInterrupts` 2, and `WaitForVInt` 3. Every target
is external to this four-handler surface, so internal effective totals are zero and external totals
equal the effective totals. No direct jump-interface alias occurs here. The source-owner joins retain
`ResetCurrentMap` in `code/gameflow/exploration/exploration.asm`; `LoadMapTilesets` and `LoadMap` in
`code/common/maps/mapload.asm`; `EnableDisplayAndInterrupts` in
`code/common/tech/interrupts/vdpcontrol.asm`; and `WaitForVInt` in
`code/common/tech/interrupts/vintengine_1.asm`, each with the pinned file SHA-256 in the fixture.
Labels, comments, near-miss mnemonics, and operands do not count as calls.

**Unknown:** `map-lifecycle/runtime-effects-matrix` is the sole grouped H3 queue. One shared runtime
matrix must determine map persistence, working-layout replacement, entity reload effects,
camera/player placement, collision/pathfinding refresh, presentation timing, fade visuals, hardware
effects, and normal-story reachability. This static slice does not promote macro names, state-symbol
names, VInt records, or service calls into any of those outcomes.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` macros `resetMap`,
`loadMapFadeIn`, `reloadMap`, and `mapLoad`; `code/common/scripting/map/mapscriptengine_1.asm`
symbols `csc36_resetMap`, `csc37_loadMapAndFadeIn`, `csc46_reloadMap`, and `csc48_loadMap`;
`sf2enums.asm` constants `OUT_TO_BLACK`, `BYTE_SHIFT_COUNT`, `BYTE_MASK`, `VINTS_DEACTIVATE`, and
`VINTS_ACTIVATE`; H1 addresses above; and local US-ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`. Reproduce with
`uv run sf2 h2 map-script-engine`; observed result is
`tests/fixtures/h2/map-script-engine-static-v1.json`, ID `sf2-map-script-engine-static-v1`, field
`expected.mapLifecycleCommandFacts`.

## Confirmed Map-Script Source-Named Trigger Command Family

Evidence date: 2026-07-28.

**Confirmed:** `sf2-map-script-engine-static-v1` field `mapInteractionTriggerCommandFacts` retains
the two source-named macro forms in source order: `roofEvent` opcode `$43` (2 sites, 6 encoded bytes)
and `stepEvent` opcode `$47` (6 sites, 6 encoded bytes). Both emit a two-byte opcode followed by
two advancing two-byte operands at offsets 2 and 4. The source comments `trigger X` and `trigger Y`
are preserved verbatim as labels only; they do not establish a player-visible trigger effect, a map
layer meaning, a collision rule, or a persistence rule.

**Confirmed:** the bounded corpus has 8 commands in 5 non-empty source program groups and a complete
304-row zero-inclusive program inventory. Its exact source-site order is
`cs_62D0E:15:roofEvent`, `cs_5AC58:5:stepEvent`, `cs_5AC58:20:stepEvent`,
`cs_5AF36:37:stepEvent`, `cs_5B016:15:stepEvent`, `cs_540C0:23:roofEvent`,
`cs_540C0:24:stepEvent`, and `cs_540C0:25:stepEvent`. The ordered source-site SHA-256 is
`525013B1AD4B1796BBBD398C063A3F7AF5DDD72D3B062888B1F1E7A26ECF58AB`; the ordered program-total
SHA-256 is `D82DA66400E77E7881A4482AFF5A7541E64DA60E57068306B70102606E72F1E8`.

**Confirmed:** `csc43_RoofEvent` is H1/ROM `$46696` and `csc47_StepEvent` is `$46726`.
Each bounded named section has exactly six normalized statements: advancing `move.w (a6)+,d0` and
`move.w (a6)+,d1`, parsed `mulu.w #MAP_TILE_SIZE,d0` and `mulu.w #MAP_TILE_SIZE,d1`, one direct
call, and `rts`. The one parsed `sf2enums.asm` equate `MAP_TILE_SIZE` has value 384; both multiplier
records resolve that same parsed source symbol rather than duplicating a numeric multiplier. The
direct-call order is `jsr (PerformMapBlockCopyScript).w` for `csc43` and `jsr (OpenDoor).w` for
`csc47`. These are exact source control-flow and operand facts, not a claim about copying, opening,
or a visible interaction result.

**Confirmed:** the comment-stripping call parser keeps the declared direct/effective target domain
zero-inclusive. `csc43_RoofEvent` has one `PerformMapBlockCopyScript` site and zero `OpenDoor` sites;
`csc47_StepEvent` has the inverse. Aggregate direct and effective totals are one for each target;
internal effective totals are zero for both and external effective totals are one for both. Neither
instruction target is a jump-interface alias. The provenance-only owner join records both symbols in
`code/gameflow/exploration/exploration.asm` with its pinned SHA-256; labels, comments, near-miss
mnemonics, and operands do not count as call sites.

**Confirmed:** the independently parsed `sf2-map-content-static-v1` map sections contain 79
`stepEvents` tables and 79 `roofEvents` tables, with 94 and 114 records respectively. The existing
canonical-map-import event-table decoder independently produces 79 `stepEventTables`, 79
`roofEventTables`, and the same 94/114 record counts. This boundary is a provenance join to table
corpora; it does not map either eight-site command corpus to a table record or assert which data is
observed at runtime.

**Unknown:** `map-interaction-trigger/runtime-effects-matrix` is the sole grouped H3 queue. One shared
runtime matrix must determine command reachability under normal play, coordinate interpretation,
callee effects, map/table selection, collision/pathfinding changes, persistence, timing, presentation,
and hardware-visible consequences. No such lifecycle or effect is promoted from this static slice.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` macros `roofEvent` and
`stepEvent`; `code/common/scripting/map/mapscriptengine_1.asm` symbols `csc43_RoofEvent` and
`csc47_StepEvent`; `sf2enums.asm` equate `MAP_TILE_SIZE`; owner source
`code/gameflow/exploration/exploration.asm` symbols `PerformMapBlockCopyScript` and `OpenDoor`; and
local US-ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`. Reproduce with
`uv run sf2 h2 map-script-engine`; observed result is
`tests/fixtures/h2/map-script-engine-static-v1.json`, ID `sf2-map-script-engine-static-v1`, field
`expected.mapInteractionTriggerCommandFacts`.

## Confirmed Map-Script Story-State Branch/Prompt Command Family

Evidence date: 2026-07-27.

**Confirmed:** `sf2-map-script-engine-static-v1` field `storyStateCommandFacts` retains exactly seven
source forms in this order: `jumpIfFlagSet` `$0C` (24 sites, 8 bytes), `jumpIfFlagClear` `$0D` (27,
8), primary `csc10` `$10` (zero, 6), `setF` alias `$10` (37, 6), `clearF` alias `$10` (16, 6),
`yesNo` `$11` (22, 2), and `setStoryFlag` `$13` (20, 4). The zero source count is only an observed
primary-carrier count: `csc10` remains a physical two-word command layout, while `setF` fixes its
second word to `$FFFF` and `clearF` fixes it to `0`. `$0B jump` and `$12 menu` are outside this
bounded family. All 146 source sites retain their program/command order and their references to the
existing 51 conditional-read, 53 direct-write, 22 prompt-write, and 20 battle-unlock `programCorpus`
records; all 304 program-total rows remain zero-inclusive.

**Confirmed:** five named handler sections preserve the source control-flow shape. `csc0C_jumpIfFlagSet`
at H1 `$47418` reads the flag word, calls `j_CheckFlag`, uses `beq.w` to skip the `movea.l (a6),a6`
target replacement, and otherwise skips the four-byte target via `addq.w #4,a6`.
`csc0D_jumpIfFlagClear` at `$4742C` has the same order with `bne.w` polarity. `csc10_toggleFlag` at
`$4747A` reads two words, branches on the second with `bne.s`, and retains the fall-through
`j_ClearFlag` before the branch-target `j_SetFlag` call. `csc11_promptYesNoForStoryFlow` at `$47490`
saves/restores A6 around `j_YesNoPrompt`, loads parsed `FLAG_INDEX_YES_NO_PROMPT` 89, tests D0, calls
`j_SetFlag` on zero and `j_ClearFlag` on nonzero, then performs source `moveq #10,d0; jsr (Sleep).w`.
`csc13_setStoryFlag` at `$474E0` reads a word, adds parsed `BATTLE_UNLOCKED_FLAGS_START` 400, then
calls `j_SetFlag`. These are guarded instruction/polarity/order facts, not a lifecycle or player-visible
meaning inferred from labels.

**Confirmed:** the comment-stripped instruction parser retains five direct identities and resolves the
three jump-interface aliases without collapsing them. Direct totals are `Sleep` 1, `j_CheckFlag` 2,
`j_ClearFlag` 2, `j_SetFlag` 3, and `j_YesNoPrompt` 1; effective totals are `CheckFlag` 2,
`ClearFlag` 2, `SetFlag` 3, `Sleep` 1, and `YesNoPrompt` 1. Every handler row has zero-inclusive
counts for that complete domain. The bounded handler surface contains none of those effective
implementations, so all internal effective totals are zero and external totals equal the effective
totals. The source-identity joins are only `code/common/stats/gameflags.asm` (`CheckFlag`, `SetFlag`,
`ClearFlag`, SHA-256 `1D9BA2EAD0CEA13718D20B0E96D86FD0AC01730E1C6C07A15F9E3EF875A45DD9`) from
`sf2-common-stats-static-v1`, and `code/common/menus/yesnoprompt.asm` (`YesNoPrompt`, SHA-256
`CF54DD1628DB83CA94F4AACA9E854A8356BB2658A5396A32950F5F31219518CA`); no sibling fixture payload is
copied.

**Unknown:** `story-state/branch-prompt-persistence-matrix` is the one grouped H3 queue. A shared
runtime launch must observe the branch target/cursor result, prompt return/value handling, and resulting
flag persistence across representative caller states. This static slice does not claim normal-story
reachability, save persistence, flag lifecycle, UI presentation, or a hardware effect.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` forms
`jumpIfFlagSet` through `setStoryFlag`; `code/common/scripting/map/mapscriptengine_2.asm`
(`csc0C`, `csc0D`, `csc10`, `csc11`, `csc13`); `sf2enums.asm` constants;
`code/common/tech/jumpinterfaces/s02_jumpinterface.asm` and `s03_jumpinterface_1.asm`; the H1
listing addresses above; and the US-ROM-backed extractor. Reproduce with
`uv run sf2 h2 map-script-engine`; observed result is
`tests/fixtures/h2/map-script-engine-static-v1.json`, ID `sf2-map-script-engine-static-v1`, field
`expected.storyStateCommandFacts`.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `sf2cutscenemacros.asm`,
`code/common/scripting/map/mapscriptengine_1.asm` (`csc1F`, `csc20`, `csc21`),
`mapscriptengine_2.asm` (`csc08`, `csc0E`, `csc0F`), `sf2enums.asm`,
`code/common/tech/jumpinterfaces/s02_jumpinterface.asm`,
`code/common/tech/jumpinterfaces/s05_jumpinterface.asm`, `code/common/stats/battleparty.asm`, H1
listing, and the fixture's US ROM SHA-256. Reproduce with `uv run sf2 h2 map-script-engine`; the
tracked fixture is `tests/fixtures/h2/map-script-engine-static-v1.json`, field
`forceStateCommandFacts`.

Two hundred ninety-six programs have H1 entry addresses. Eight remain source-only: the labeled but
unassembled `rbcs_battle01`, one unlabeled unused suspend scene in map 72, and six unlabeled debug
fragments. Seven programs therefore have no entry label. These exceptions remain first-class rows
rather than borrowing an adjacent address or disappearing from the denominator.

A token-exact reference scan over all 2,077 code/data ASM files establishes the static reachability
upper bound. Two hundred ninety-seven programs have at least one reference through any owned label:
187 have a cross-file reference and 110 are same-file-only. Seven programs have no reference. Across
the 348 labels there are 165 same-file and 206 cross-file references; 347 labels are referenced and
`rbcs_battle01` is the sole zero-reference label. One otherwise source-only anonymous debug fragment
is still same-file reachable through an internal label, so source/H1 status and reference status are
deliberately independent axes.

The seven zero-reference programs are `rbcs_battle01`, the unlabeled map 72 suspend scene, and five
unlabeled debug fragments. Static references only prove that source contains an incoming edge. They
do not prove normal-game caller state, flag combinations, save persistence, or rendered sequencing,
which remain runtime/design questions.

The story-state dependency surface is now explicit across 89 programs. Fifty-one conditional reads
touch only six flags: 6 (four reads), 8 (one), 29 (one), 71 (one), 76 (22), and 89 (22). Direct
`setF`/`clearF` commands contribute 53 deterministic writes. Twenty-two `yesNo` commands always write
flag 89 after the prompt: a zero return sets it and a nonzero return clears it. Twenty
`setStoryFlag n` commands add the pinned `BATTLE_UNLOCKED_FLAGS_START` value 400 and set the resulting
battle flag. These commands collectively write 56 unique flags; the read/write domains overlap only
at 71, 76, and 89.

The canonical output retains every program/command index, raw flag operation, prompt-result mapping,
and battle-to-flag translation, plus exact direct-set/direct-clear/battle-unlock domains. This proves
state access and conditional-edge prerequisites. It does not prove a global story order, mutually
exclusive save states, or persistence across transitions.

The 83 handlers contain 955 parsed instructions and group into fourteen source-role families. Their
source-shaped access catalog contains 16 entity fields, 25 global-state symbols, and 62 direct-call
targets, while preserving every script-cursor statement. These facts define opcode topology and
state-touch surfaces, not caller-dependent story meaning. Story-branch reachability, combined
entity/camera/text/wait/transition frame timing, and palette-fade/VDP-visible presentation remain
three grouped runtime questions for future shared observation seams.

The entity VInt skips slots whose coordinate is at least `$7000` or whose actscript pointer is zero,
then dispatches the current word through an 80-slot table. It has 44 unique targets; 37 unused slots
advance directly to the next entity. This establishes table shape and dispatch behavior, not exact
movement duration or rendered sprite behavior.

`DisplayText` selects a text-bank pointer by `(stringIndex / 256) * 4`, uses the low byte within that
bank, reads a one-byte compressed-length prefix, and initializes a stateful Huffman decoder. Decoder
state begins with previous symbol `$FE`, chooses its tree from the previous decoded symbol, and
persists the bit barrel plus previous symbol across calls. Symbols `$EE` and above are control codes;
`$FE` terminates the string.

The context-Huffman payload is now a complete static contract. The offset file is 510 bytes and
therefore contains 255 big-endian entries, not the 256 claimed by its adjacent upstream note. Of
those entries, 86 select a tree and 169 contain `$FFFF`. The 1,952-byte tree payload packs exactly
1,536 reverse-stored leaf symbols and 1,450 non-leaf nodes into 86 contiguous records with no gaps,
overlaps, or nonzero padding bits. Code lengths range from zero through fourteen bits; context 54 is
the sole one-leaf tree and emits symbol 58 without consuming an input bit. Starting at context 254,
all 86 defined contexts are reachable, and the emitted-symbol set equals the defined-context set, so
no valid path selects a `$FFFF` entry. All 2,462 bytes match the H1 addresses and input ROM.

The same decoder now processes the complete original text-bank corpus. Banks 0-15 contain 256
length-prefixed records each and bank 16 contains 171, for 4,267 strings across 79,013 source bytes.
Their 74,746 compressed payload bytes decode to 152,679 symbols, including exactly one terminator 254
per record. Every record leaves 8-15 stored bits after its terminator, every one of the 86 defined
Huffman contexts occurs in the real corpus, and no undefined context is selected. The 17 bank
addresses, 68-byte pointer table, one alignment byte, and top-level pointer give 79,086 bytes of
source/ROM parity. The adjacent `gamescript.txt` independently has contiguous IDs 0-4266; only its
hash/count are tracked, while plaintext and per-string decoded symbol arrays remain under ignored
`local/derived`.

Control symbols 238-252 occur 8,783 times before terminators. Symbol 253, documented by the parser as
the color command, occurs zero times in the complete original banks. That proves base-corpus absence,
not impossibility through a nonstandard direct input or modified script.

The variable-width font has 80 fixed 32-byte glyph records. Each record stores a width field followed
by fifteen rows of twelve usable pixel bits; all header and row padding bits are zero. Stored widths
range from 3 through 9, and `LoadVariableWidthFont` uses `(symbolId - 1) * 32`, then advances zero
pixels for stored zero or `storedWidth + 1` otherwise. The payload, pointer, and loader addresses all
match H1 and ROM.

When `CURRENT_DIALOGUE_ASCII_BYTE_ADDRESS` is nonzero, `GetNextTextSymbol` maps the input byte through
the 256-entry ASCII table. That table reaches 78 of 80 glyph IDs, maps 145 inputs to the default
glyph 1, and never emits IDs 70 or 71. The independent Huffman corpus emits 69 IDs in the 1-80 glyph
range and also omits 70 and 71. Consequently the union of both normal text-input paths still reaches
78/80 glyphs and proves those two IDs unreachable through normal ASCII or compressed dialogue.
Non-regular dialogue calls the glyph loader twice while regular dialogue calls it once; rendered
overlap and timing remain runtime facts.

`ChangeEntityMapsprite` and `DmaEntityMapsprite` convert one regular map-sprite ID plus facing into
one of three pointers in `pt_Mapsprites`, call `LoadBasicCompressedData` into
`FF8002_LOADING_SPACE`, and DMA `0x120` words (`0x240` bytes) to the entity's VRAM slot. IDs 240-255
take the separate special-sprite route. The data contract confirms 669 valid streams of that exact
size, but IDs 237-239 share a raw `0xFFFF` placeholder even though they are below the route cutoff.
A complete symbolic scan finds no source references to those enums, but whether encoded values and
runtime writes exclude the three reserved IDs remains **Unknown** and is owned as one data-flow/
runtime question rather than inferred from name absence.

The combatant-specific route is now closed separately. `GetCombatantMapsprite` reads the enemy-index
byte and performs an unchecked lookup into the 166-byte enemy map-sprite table. Rows 0-102 correspond
to enemy definitions; rows 103-165 contain an NPC-sprite tail. All 627 built battle-spriteset inputs,
the random-upgrade ranges, and the sole named `SetEnemyIndex` caller stay within 0-102, so normal
battle initialization cannot enter the tail. Raw/debug/corrupt enemy-index state remains an explicit
nonstandard reachability question.

The dialogue-property route is also byte-closed. `GetEntityPortaitAndSpeechSfx` masks the character
index, reads the entity map-sprite byte, then linearly scans 119 unique keys in four-byte records.
On a match it sign-extends the portrait byte (`PORTRAIT_NONE` 255 therefore becomes `-1`) and returns
the unsigned speech-SFX byte; the fourth byte is reserved, zero in every source row, and ignored.
After a miss it advances four bytes and tests the next word for the `0xFFFF` sentinel. Exhausting the
table returns portrait `-1` and normal bleep 6 (74). Source reconstruction and the input ROM agree
for all 478 bytes. Which call paths use that fallback and the timing of portrait suppression/audio
remain grouped presentation questions rather than guessed semantics.

The complete original map-sprite assignment surface is now statically classified. Four entity-slot
writers plus the direct player-raft write consume 81 built script macro assignments, 20 callers of
`UpdateEntityProperties`, 980 initial entity records, or the already-verified ally/enemy tables. The
script payloads contain 76 backed regular values and five routed special value 255 assignments; all
20 property-update callers either preserve the current sprite (12), use ally/vehicle derivation (6),
or pass one of two named literals. Ally table values span 1-58 and only decrease during class
selection; enemy table values span 52-229. Consequently no original built input domain writes
237-250. Raw RAM, malformed scripts, and corrupt combatant state remain nonstandard injection cases,
not normal-game reachability unknowns.

All three shared entity-action sources are also parsed as complete command corpora. They contain
732 commands across 61 entry labels, use 34 of 44 defined `ac_*` macro forms, and occupy exactly
2,864 bytes once macro widths and 38 explicit branch displacement words are summed. All relative
targets resolve within the shared corpora; 28 of 29 absolute jumps exit to `eas_Idle`, while the last
jumps internally to `eas_Init`. A complete code/data source scan finds 3,061 external references from
230 files; `eas_ShrinkDisappear` is the only entry without one. This confirms command inventory,
static control flow, and source references, but not frame timing, collision outcomes, or normal-story
reachability of those references.

The same rail now closes every distributed `ac_*` body outside those shared files. The 75 source
files divide into 42 map, 26 battle, six scripting-data, and one code file. All 1,472 commands have
one structural owner: 1,217 commands in 361 `customActscript`/`customActscriptWait` programs and 255
commands in 11 continuous standalone action ranges. Every inline program reaches `ac_end`; together
they encode 4,742 action bytes. The standalone ranges encode another 942 bytes, expose 17 `eas_*`
entry labels, and match their H1 label offsets and ROM range hashes. `byte_45488` is deliberately a
range start even though `eas_4548C` appears four bytes later, proving that physical ranges and
`eas_*` entry counts are different denominators.

Across the distributed corpus, 14 relative branches resolve to five known targets and all 364
absolute jumps resolve to `eas_Idle`. All 17 named entries have at least one definition-excluded
same-file or cross-file reference (38 references from eight files total). This confirms 5,684 action
bytes, command ownership, labeled layout, and static control flow. It does not yet explain the RAM
effects of all 44 defined opcode handlers or prove movement timing, collision results, or story-route
reachability.

The dispatcher boundary is now complete as well. Its 80 slots contain 37 copies of
`esc_goToNextEntity` and 43 real handlers. Of the 44 named `ac_*` macros, `ac_end` is the `$8080`
inline-copy terminator and never indexes the dispatcher; the other 43 macro names collapse to 40
runtime opcodes because the four orientation macros share opcode 27. Three additional handlers at
opcodes 49-51 implement flag-set, flag-clear, and random branches without a matching named macro.
Every runtime macro maps to a non-filler handler, every non-filler handler is owned by either a macro
opcode or one of those three handler-only slots, and all handler labels have H1 addresses.

The complete source corpus uses 38 of the 43 runtime macros. `ac_pass`, `ac_set1Cb4`, `ac_setGhost`,
`ac_setId`, and `ac_waitDestEntity` are defined and mapped but absent from all 2,204 parsed commands.
The handler catalog records every handler's source span, script-parameter reads, direct calls, exit
routes, and source-shaped access modes. Its 18 entity fields include the implicit X coordinate at
`(a0)`; 11 fields are read and 17 are written. Of 15 global-state symbols, ten are read and five are
written. Parameter reads also retain byte/word/long width and instruction provenance. These are
deterministic assembly-access facts, not yet claims about signed meaning or frame-level behavior.
All 43 handlers are grouped into eight source-role families: 16 entity-property, eight movement, six
control-flow, five motion-state, three direct-control, three wait, one audio, and one map-effect.
Twenty-two per-handler entity-bit access records distinguish test/set/clear operations, including
`FLAGS_A` bits 5/6/7 for entity collision, map collision, and obstruction control. The catalog also
records 46 script-pointer actions: fixed advances plus word-relative and long-absolute transfers.
The 44 macro definitions declare 46 dynamic parameters spanning 86 encoded bytes. Forty runtime
macros consume every declared byte. Three are intentionally partial at the handler boundary:
`ac_pass` advances over both payload bytes without reading them, while `ac_setId` and `ac_setSprite`
read only the low byte of their declared word. All four ignored byte positions are preserved in the
contract instead of being silently retyped as byte parameters.
`ac_branch` is the sole named macro with an operand encoded outside its macro body: a trailing signed
word-relative displacement at offset 2. The three handler-only opcodes are absent from all 2,204
parsed source commands but their handler shapes are closed: opcodes 49/50 are six bytes with a
handler-read word followed by a relative displacement; opcode 51 has an ignored word followed by the
same displacement shape. Consequently 35 of 43 handlers occur in the complete source corpus. The
eight absent handlers are the five unused named-macro handlers plus those three handler-only slots.
All direct conditional and unconditional exits, plus the shared movement tail and `ac_pass`
fallthrough, now reduce to two implementation-neutral outcomes. Thirty-nine handlers can redispatch
the next command in the same entity update, eleven can yield to the next entity, and seven can do
either: wait, wait-for-destination, relative/absolute movement when obstructed, random walk, sprite
update when the load queue is full, and wait-for-another-entity. The four yield-only handlers are the
character/follower/raft/Caravan continuous-control paths.
All 46 declared macro parameters now have effective handler-side roles and numeric interpretation:
ten signed numeric values, twenty unsigned numeric values, fifteen booleans, and the one deliberately
ignored `ac_pass` payload. Macro comments own 32 roles; handler data flow supplies the other fourteen.
`ac_randomWalk` is the sole recorded disagreement: its macro calls the first two words X/Y speed,
while the handler uses them as unsigned center tile coordinates and the third word as a radius before
unsigned tile-size multiplication. The raw macro comment remains in the contract beside the effective
role instead of being overwritten.

The seven dual-outcome handlers also carry source-statement predicates. They distinguish the wait
timer threshold, current/other-entity destination deltas, raw CCR results from
`HasSameDestinationAsOtherEntity`, random-walk search exhaustion, and the sprite-load queue limit.
For the helper-CCR cases the contract deliberately says only whether BNE is taken; it does not infer
collision meaning from the helper name or comment. The helper pass below now resolves that boundary
from its actual return instructions.

The command layer now joins directly to the 560-byte `UpdateEntityData` core at `$5D6C..$5F9C`.
Its 190 instructions divide into nine H1-bound phases: destination delta, per-axis acceleration,
velocity update, position integration, facing selection, animation/sprite update, destination snap,
arrival tile state, and animation-counter clamp. The function accesses 15 entity fields (14 read,
nine written), consumes `FLAGS_A` bits 0/1 as X/Y acceleration enables and bits 2/3 as X/Y
deceleration enables, and calls `UpdateEntitySprite`, `ConvertMapPixelCoordinatesToOffset`, and
`ChangeEntityMapsprite`.

The arithmetic shape is static and explicit. Acceleration begins in the outer three quarters of the
original travel distance; deceleration is selected inside the final quarter. Velocity is then added
to position only while the axis travel field is nonzero. A signed velocity-magnitude difference uses
thresholds -8/+8 to index the 16-byte `$5F9C` facing table, whose bytes match the ROM. Animation adds
`(abs(X velocity)+abs(Y velocity)) >> 5` unless its counter is -1, and resets the counter after 30.
Each axis snaps to destination on zero delta or sign crossover. Once both travel words are zero, the
destination tile can change layer/immersed state and trigger a sprite reload when immersed toggles.

Four update helpers add another 434 bytes/135 instructions, eight entity fields, eleven global-state
symbols, eight direct-call targets, and 22 call sites across two source files. The destination-conflict
helper scans 49 slots, skips empty/current entities, and treats Manhattan destination distance below
one map tile (384 pixels) as a conflict when entity-obstruction bit 5 is enabled. Its code sets Z=0 on
conflict (`moveq #-1,d4`) and Z=1 on no conflict (`clr.w d4`); intervening `movem`/`rts` preserve CCR.
This contradicts the source comment claiming “zero-bit set if true.” The relative/absolute movement
handlers' BNE branches therefore yield on a real destination conflict and redispatch when clear.

`UpdateEntitySprite` is a gate, not a normal call wrapper: auto-facing must be enabled, facing must
change, and the sprite-load queue must be below its limit; success falls directly through into
`ChangeEntityMapsprite`. That function bypasses special sprites and entity number 32, otherwise loads
the regular compressed sprite, applies immersed/resize/ghost/orientation transforms, and queues VInt
DMA. `ConvertMapPixelCoordinatesToOffset` conditionally adds exploration layer origins, hashes
`X>>7`/`Y>>7` to six bits, forms `(row<<6)+column`, and doubles it to a word-byte offset.

`map/mapsetupsfunctions_1.asm` and `map/mapfunctions.asm` now have deeper cross-subsystem contracts:
setup selection, six-pointer layout, entity/zone/item/description dispatcher shapes, all selected
entity record streams, the complete entity/zone/item event-table boundary, and all area-description
wrappers/tables, setup initialization callables, and standalone setup scripts are source/H1-verified by
[`map-data-inventory.md`](./map-data-inventory.md). This inventory still owns the file boundary and
general scripting engine; the map-data document owns setup-table semantics.

## Entity Movement Runtime Matrix

One BizHawk launch now replays a stable debug Map Test 0 exploration state across 13 cases and 20
original VInt ticks. Each case clears the other 48 entity slots and replaces only entity 0's 32-byte
record, a small RAM action script, an optional blocker record, and the layout word consumed after
arrival. The original `VInt_UpdateEntities`, `UpdateEntityData`, action dispatcher, destination-
conflict helper, coordinate conversion, and next-entity transition execute unchanged.

The observed state vectors exactly match the independent Python model. **Confirmed:** wait timer 2
yields with counters 1 and 2, then advances and clears on tick 3; relative and absolute moves both
retain their command pointer when a destination is within Manhattan distance 384 of an obstructing
entity, and otherwise install destination/travel/signed velocity before movement begins on the next
tick. Three acceleration ticks produce X velocity/position pairs `(20,20)`, `(24,44)`, `(28,72)`;
two deceleration ticks produce `(24,324)`, `(16,340)`. A 16/64 diagonal selects facing 3 and adds two
animation units, `-1` disables animation, and stationary counter 31 clamps to zero. Crossover snaps
to destination and clears travel; controlled `$2000`, `$2400`, and `$3400` layout flags respectively
select layer 2, layer 0, and immersed state.

The RAM scripts terminate through dispatcher filler opcode `$24`, whose target yields to the next
entity. `ac_pass` is not a terminator: its handler advances four bytes and redispatches. This was
observed during harness development and agrees with its source flow; future synthetic scripts MUST
not use `ac_pass` as a stop instruction.

Story-route reachability remains a separate question. Dialogue typewriter/render timing, control-
code side effects and inserted dynamic values, nonstandard direct symbol injection, end-credit
presentation, and contextual meaning of script commands remain grouped runtime questions. Reserved
map-sprite IDs 237-250 remain excluded from symbolic references, encoded entity records, built script
payloads, combatant-derived tables, direct writers, and every property-update caller. A future shared
entity-sprite matrix is only needed if the project chooses to document deliberately malformed/raw
injection behavior.

## Reproduction

```powershell
uv run sf2 h2 common-scripting
uv run sf2 h2 map-script-engine
uv run sf2 h2 entity-action-scripts
uv run sf2 h3 entity-movement
uv run sf2 h2 map-setup
uv run sf2 h2 map-entities
uv run sf2 h2 map-events
uv run sf2 h2 map-descriptions
uv run sf2 h2 map-init
uv run sf2 h2 map-scripts
uv run sf2 h2 map-sprites
uv run sf2 h2 variable-width-font
uv run sf2 h2 text-huffman
uv run sf2 h2 text-banks
uv run sf2 h2 enemy-map-sprites
uv run sf2 h2 map-sprite-assignments
uv run sf2 h2 sprite-dialogue
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/common-scripting-static.json`,
`local/derived/map-script-engine-static.json`, and
`local/derived/variable-width-font-static.json` and `local/derived/text-huffman-static.json`.
The full decoded symbol corpus stays in ignored `local/derived/text-banks-static.json`.
The full dialogue-property catalog stays in ignored `local/derived/sprite-dialogue-static.json`.
The full assignment/caller catalog stays in ignored `local/derived/map-sprite-assignments-static.json`.
