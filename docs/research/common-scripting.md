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
  entity-placement source/handler/caller contract, the complete seven-command source-named map-script
  entity gesture/relationship/motion source/handler/caller contract, the complete twelve-command
  source-named map-script screen/map-presentation source/handler/caller contract, the complete 119-row sprite-dialogue
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
have a representative global symbol bound to the H1 listing. They now own 126 indexed findings across
those 28 indexed source paths. The H2 fixture keeps the exact ordered record IDs and per-source-path
relation; membership is the general recursive `sourcePath` boundary, without an ID, subsystem,
document, or evidence-level exception. Record count and indexed-file count are intentionally separate
denominators. The remaining
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

### Bounded runtime matrix — dialogue handlers

**Confirmed (H3):** `sf2-map-script-dialogue-runtime-v1` replays 21 handler-local records through one
BizHawk 2.11.1 / Genesis Plus GX Map Test 0 launch. Before deriving cases it joins the existing 205
ordered H2 source-site references to the authoritative 304-program corpus transiently, checks the
2,883-row order digest, and keeps no second copied command corpus. The source-derived partitions cover
all four observed modifier-byte values (0/128/192/255), high-bit-clear/high-bit-set and `$FFFF` sentinel
inputs for both non-`Var` display forms; two skip-flag controls; the two zero-source `*Var` forms; both
explicit `textCursor` bounds 240/4,233; and `hideText`.

**Confirmed (H3):** all six handler entry/RTS PCs, A6 end offsets, pre-RTS four-byte call-frame deltas,
skip branch polarity, ordered direct instruction/effective target/call-site/target-entry/return records,
zero-inclusive target counts, and direct RAM write results match their guarded named H1 sections. The
observer captures each entry, call-site, target-entry, and return PC before resolving an instruction or
effective-target label through its unique guarded source/H1 address map. The session trampoline explicitly
seeds D0/D1/D2; each recorded call-register triple is then derived from the parsed D0 source use-site
(`(a6)`, `CUTSCENE_DIALOG_INDEX`, or immediate 10) and verified RTS shims, rather than treating an
emulator sample as source authority. The service entries are patched only in a checksum-updated session
copy after original-byte preflight; they are not evidence about portrait, text, audio, wait, close, or
sleep service bodies.

**Unknown:** the remaining grouped queues are exactly:

- `map-script-dialogue/normal-story-reachability`: normal script/save route admission;
- `map-script-dialogue/rendered-portrait-speech-and-controller-timing`: visible output, speech playback,
  input, waits, and frame timing;
- `map-script-dialogue/service-body-effects-and-persistence`: unshimmed service execution, completion,
  repeat behavior, and persistence.

Provenance: USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`; pinned
`ShiningForceCentral/SF2DISASM` `master` `c834c652b6862bc5679fd7f69a38a7093206efc6`;
`mapscriptengine_2.asm` named `csc00`/`csc01`/`csc02`/`csc03`/`csc04`/`csc09` sections and
`build/sf2build-h1.lst`; fixture `tests/fixtures/h3/map-script-dialogue-v1.json`; verifier
`src/sf2tool/h3/map_script_dialogue.py`; observed result `21 cases, 6 handlers, 1 launch, PASS` on
2026-07-31. Reproduce with `uv run sf2 h3 map-script-dialogue --timeout-seconds 180`.

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

**Confirmed (bounded H3):** `sf2-map-script-transition-runtime-v1` runs five RAM-owned representative
streams through the unmodified `ExecuteMapScript` loop in one BizHawk 2.11.1 / Genesis Plus GX Map
Test 0 launch. It confirms the five opcode/terminal words, A6 offsets after each handler, handler
entry and return chronology, csc37-to-csc48 fall-through, direct handler service-call/return seams,
and the bounded csc07 event bytes. With explicit initial map/view-target seeds it also records the
resulting `CURRENT_MAP`, `VIEW_TARGET_ENTITY`, and selected View Plane A words. `ResetCurrentMap`
tail-branches into `LoadMap`; that service body's shared `EnableDisplayAndInterrupts` and
`WaitForVInt` entries are deterministically dispatched as nested service roles, not misreported as
additional direct csc36 calls.

The source has csc37 write `OUT_TO_BLACK` value 2 before its fall-through. In this bounded run,
after `LoadMapTilesets` and at the first observed csc48 `WaitForVInt` entry, `FADING_SETTING` is
already 0. The observer reads that value before any optional safety clear and therefore does not
alter it in this run. This establishes neither fade duration/completion nor a player-visible result.

The grouped H3 question remains **Unknown**:
`map-script-transition-presentation-matrix`. Event consumption, map/camera presentation, fade/display
timing, caller-dependent presentation, normal-story reachability, persistence, and unobserved service
effects remain outside this synthetic, bounded matrix.

Provenance: pinned `ShiningForceCentral/SF2DISASM` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`, `master`,
`sf2cutscenemacros.asm`,
`code/common/scripting/map/mapscriptengine_1.asm` (`csc36`, `csc37`, `csc46`, `csc48`),
`mapscriptengine_2.asm` (`csc07` and dispatcher), `sf2enums.asm`, the H1 listing addresses, and the
US ROM SHA-256 in the fixture. Reproduce static evidence with `uv run sf2 h2 map-script-engine`; the
observed output is fixture ID `sf2-map-script-engine-static-v1`, transition field
`transitionCommandFacts`. Reproduce bounded runtime evidence with
`uv run sf2 h3 map-script-transition --timeout-seconds 180`; its fixture is
`tests/fixtures/h3/map-script-transition-v1.json`, its recursively closed contracts are
`schemas/h3/h3-map-script-transition-fixture.schema.json` and
`schemas/h3/h3-map-script-transition-observation.schema.json`, and its verifier is
`src/sf2tool/h3/map_script_transition.py`.

## Confirmed Map-Script Camera-Control Command Family

Evidence date: 2026-07-29.

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

**Confirmed (H3):** `sf2-map-camera-control-runtime-v1` runs seven cases from one BizHawk launch
through the `RunMapSetupInitFunction` session-only seam. The `$FF80` word case takes the guarded
negative direct-write branch and leaves `VIEW_TARGET_ENTITY` byte `$80` without reaching the
`ENTITY_INDEX_LIST` lookup. The word `$0002` takes the nonnegative nonnegative-byte lookup branch at
H1 `$46C4E` and copies its seeded index-2 byte `$2A`; `$00E1` takes the nonnegative negative-byte
path, applies the source `ENTITY_ENEMY_INDEX_DIFFERENCE` use, reaches index 129, and copies seeded
byte `$2B`. These are bounded instruction/state observations; source labels do not establish a
player-facing target-selection interpretation.

**Confirmed (H3):** the destination cases feed source words `(1, 2)` and `(257, 2)` to
`csc32_setCameraDestInTiles`. In source call order they observe its H1 `$46512` direct alias call,
the resolved `SetCameraDestination` entry, its H1 `$2349E` `SetViewDestination` call/entry, then H1
`$46518` `WaitForViewScrollEnd` call/entry before the handler return. The observed transferred D0/D1
words are `(384, 768)` and `(33152, 768)`, respectively, and the handler leaves
`VIEW_TARGET_ENTITY` byte `$FF`. The two source-given speed words 8 and 64 each reach
`VIEW_SCROLLING_SPEED` unchanged before return. These observations confirm the guarded branch,
word-transfer, call-order, and bounded wait-completion facts, not coordinate units, motion, or VDP
presentation.

### Runtime questions — map-script camera control

**Unknown:** normal-story reachability of these seven session-only inputs, including which original
script contexts select the observed branch/data combinations.

**Unknown:** VDP and player-visible camera behavior, including scroll trajectory, composition, timing,
and the visible meaning of the source-named target/destination/speed state.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `sf2cutscenemacros.asm` macro definitions;
`code/common/scripting/map/mapscriptengine_1.asm` named sections `csc24_setCameraTargetEntity`,
`csc32_setCameraDestInTiles`, and `csc45_cameraSpeed`; jump-interface
`code/common/tech/jumpinterfaces/s05_jumpinterface.asm::j_SetCameraDestination`; effective helper
`code/gameflow/battle/battlefunctions/battlefunctions_0.asm::SetCameraDestination`; and named
`WaitForViewScrollEnd`/`SetViewDestination` owner files recorded in the fixture source-identity joins.
The input identity is the local US ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`. Reproduce with
`uv run sf2 h2 map-script-engine`; observed static result is
`tests/fixtures/h2/map-script-engine-static-v1.json`, fixture ID
`sf2-map-script-engine-static-v1`, field `expected.mapCameraControlCommandFacts`. Reproduce the H3
matrix with `uv run sf2 h3 map-camera-control`; its source-derived fixture is
`tests/fixtures/h3/map-camera-control-v1.json`, fixture ID
`sf2-map-camera-control-runtime-v1`, observed by
`tools/bizhawk/map_camera_control_observer.lua` and checked by
`src/sf2tool/h3/map_camera_control.py`.

## Confirmed Map-Script Entity-Placement Command Family

Evidence date: 2026-07-29.

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

**Confirmed:** the four named handler sections are H1/ROM `csc19_setEntityPosAndFacing` `$46A12`
(18 statements), `csc17_setEntityPosAndFacingWithFlash` `$469AC` (17), `csc23_setEntityFacing`
`$46C20` (8), and `csc29_setEntityDest` `$46D98` (29). `csc19` and `csc23` each preserve their
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

**Confirmed (H3):** `sf2-map-script-entity-placement-runtime-v1` runs seven fixed cases from one
BizHawk launch through the session-only `RunMapSetupInitFunction` trampoline. With selected ally
current-HP seed 1, `setPos` writes the seeded entity record's X/XDEST and Y/YDEST words to
`2*384=768` and `3*384=1152`, writes facing byte 1, and reaches its H1 adjust/get/update call sites.
With current-HP seed 0, it reaches the H1 `$47096` `adda.w d7,a6` use site, advances the RAM script
cursor from `$FF4004` to `$FF4008`, returns before the get/update sites, and leaves the seeded entity
fields unchanged. The corresponding `setFacing` cases establish the same bounded alive/dead split:
alive writes facing 1 and calls the sprite wrapper, while dead advances `$FF4004` to `$FF4006` and
leaves facing unchanged. These results establish only the guarded current-HP/cursor/record effects.

**Confirmed (H3):** the one `setPosFlash` case reaches its own get-entity call, exactly 31 repetitions
of local `WaitForVInt`, `WaitForVInt`, `Sleep`, then the H1 `$469DA` shared-tail branch. The unmodified
`csc19` tail then reaches its distinct adjust/get/update sites and leaves the same `(768, 1152, 1)`
record values. The observer retains both get-entity identities and the shared-tail sprite-wrapper
identity separately; it does not interpret the local timing calls as presentation behavior.

**Confirmed (H3):** two `setDest` cases seed `(X,Y)=(768,768)` and observe the source-scaled
destinations and signed travel/velocity writes before the optional wait call. Selector `$0000` with
input words `(3,1)` produces destination words `(1152,384)`, positive-X/negative-Y branches, and the
H1 `$46DE8` wait call. Selector `$8000` with `(1,3)` produces `(384,1152)`, negative-X/positive-Y
branches, and bypasses that wait through the guarded bit-15 branch. These are stored/transferred word
facts and branch/call observations, not coordinate-unit, pathfinding, or motion semantics.

### Runtime questions — entity placement

**Unknown:** `map-script-entity-placement/normal-story-reachability` — which normal scripts and save
states reach these session-only selector/HP/input combinations.

**Unknown:** `map-script-entity-placement/full-animation-visibility-presentation` — the player-visible
meaning, VInt/Sleep cadence, sprite animation, and visibility of these bounded calls.

**Unknown:** `map-script-entity-placement/collision-pathfinding-persistence` — interactions with map
collision/pathfinding and state persistence beyond the observed handler-local record writes.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` macros `setPos`,
`setPosFlash`, `setFacing`, and `setDest`; `code/common/scripting/map/mapscriptengine_1.asm` named
sections and helper symbols above; `sf2enums.asm::MAP_TILE_SIZE`; H1 listing addresses; and local US
ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`. Reproduce with
`uv run sf2 h2 map-script-engine`; observed result is fixture ID
`sf2-map-script-engine-static-v1` in `tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.entityPlacementCommandFacts`. Reproduce the bounded runtime matrix with
`uv run sf2 h3 map-entity-placement`; its fixture is
`tests/fixtures/h3/map-script-entity-placement-v1.json`, fixture ID
`sf2-map-script-entity-placement-runtime-v1`, observed by
`tools/bizhawk/map_entity_placement_observer.lua` and checked by
`src/sf2tool/h3/map_entity_placement.py`.

## Confirmed Map-Script to Entity-Action Bridge Command Family

Evidence date: 2026-07-29.

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

**Confirmed (H3):** `sf2-map-entity-action-bridge-runtime-v1` runs all six source aliases once in
one BizHawk 2.11.1 / Genesis Plus GX Map Test 0 session through the bounded
`RunMapSetupInitFunction` trampoline. Every case reaches its exact handler and
`GetEntityAddressFromCharacter` call site, returns to the trampoline, resolves selector byte 1 through
`ENTITY_INDEX_LIST` to the seeded entity index 0, and leaves the one-byte `ACTSCRIPTWAITTIMER` field
at 0. The `$FF` rows enter their exact guarded compare PC twice and execute the exact back-edge branch
PC twice; only at the second compare does the session harness write `eas_Idle` to the parsed four-byte
`ACTSCRIPTADDR` field. That bounded release is an observation-control action, not a natural wait or
timing result. The zero-control rows do not enter either wait hook.

**Confirmed (H3):** the csc15 rows retain their input pointer on the zero-control path and the injected
`eas_Idle` pointer on the bounded wait path. The csc14 rows reach the exact `$8080` compare, finish at
the source-derived cursor offsets 8, and retain either its captured input pointer (zero control) or the
injected `eas_Idle` pointer (wait). The two csc2D rows reach the indexed call and its exact selected
target `csc2D_8_faceRight` `$468AA`, then the terminal entry `$46928`; their `FLAGS_A` seed `$FF`
becomes `$9F`, their post-handler action-buffer pointer is `$FF4110`, and their separately observed
entity pointer is the input action-buffer base for zero control or injected `eas_Idle` for wait.

**Confirmed (H3):** at the parsed H1 PC `$46932`, immediately after the terminal
`move.l #eas_Idle,(a0)+` write, each csc2D row captures exactly one complete write-time buffer snapshot:
the selected indexed words `[34, 0, 10, 0, 7]`, terminal record word `52`, and the four-byte
`eas_Idle` payload pointer `$451FC`. The indexed words use their independently parsed two-byte write
width; the terminal-record two-byte width and idle-payload four-byte width remain separate fields.
This is intentionally not a stable post-handler buffer-content claim: the ordinary action consumer can
change that RAM after the source write, while the global buffer pointer and entity pointer are checked
after the handler returns.

### Runtime questions — map-script entity-action bridge

**Unknown:** remaining grouped H3 questions are:

- `map-script-entity-action-bridge/normal-story-reachability`: which ordinary map scripts and caller
  states reach these aliases;
- `map-script-entity-action-bridge/full-action-motion-collision-effects`: action payload meaning and
  resulting motion, collision, entity, and action effects outside the bounded buffer write;
- `map-script-entity-action-bridge/presentation-timing-persistence`: natural wait duration, frame/VDP
  presentation, and persistence.

The fixture does not promote macro names, source labels, the injected wait release, callback hits, or
the write-time buffer snapshot into any of those outcomes.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` macros `csc14`, `csc15`,
`csc2D`, the six aliases, and `ac_end`/`endActions`; `code/common/scripting/map/mapscriptengine_1.asm`
named sections above and `rjt_EntityMoveCommands`; the joined fixture source locations; H1 listing
addresses `$467E2`, `$46814`, `$468AA`, `$46928`, `$46932`, `$46944`, `$4694C`, `$46956`, `$4695E`,
`$46966`, `$4696E`, and `$46976`; and local US ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`. Reproduce the static source
contract with `uv run sf2 h2 map-script-engine` (fixture
`tests/fixtures/h2/map-script-engine-static-v1.json`, ID `sf2-map-script-engine-static-v1`, field
`expected.entityActionBridgeCommandFacts`) and the runtime matrix with
`uv run sf2 h3 map-entity-action-bridge --timeout-seconds 120` (fixture
`tests/fixtures/h3/map-entity-action-bridge-v1.json`, ID
`sf2-map-entity-action-bridge-runtime-v1`; verifier
`src/sf2tool/h3/map_entity_action_bridge.py`; observer
`tools/bizhawk/map_entity_action_bridge_observer.lua`). Observed runtime result: 6 cases, 3 handlers,
1 session-only BizHawk launch, PASS.

## Confirmed Map-Script Entity Lifecycle/Presentation Command Family

Evidence date: 2026-07-30.

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

**Confirmed (bounded H3 observer):** one session-only Map Test 0 replay of 11 fixed cases executed
all eight named handler entries and compared the complete record object in
`sf2-map-entity-lifecycle-presentation-runtime-v1`. `hide` reached its two direct callback targets.
The live `startEntity` and `stopEntity` rows reached `AdjustScriptPointerByCharacterAliveStatus` then
`GetEntityAddressFromCharacter`, with guarded animation-counter writes of 0 and `$FF`. The
zero-current-HP `stopEntity` row reached only the adjust helper: its source-local return path left the
seeded counter `$7F` unchanged and left A6 at input offset 6. This confirms a bounded helper-return
boundary, not an alive/dead gameplay lifecycle interpretation.

**Confirmed (bounded H3 observer):** the controlled `waitIdle` row entered both its compare and its
`bne` instruction twice, injecting `eas_Idle` at compare entry 2; this establishes neither natural
wait duration nor a count of taken backedges. `setSprite` input 0 reached `GetAllyMapsprite`,
`WaitForVInt`, and `UpdateEntitySprite_0`, while threshold input 30 skipped only the first callback.
The zero/nonzero `setPriority` rows produced bytes 0 and 1. `removeShadow` reached `LoadMapsprite`,
`sub_45A8C`, `DmaMapsprite`, and `WaitForVInt` after its entity-address callback. The `setSize` row
used source-backed words 16 (`InitializeMapEntities`) and 21 (`csc2A_entityShiver`): its guarded
update saw 16, post-handler storage was 21, and flags-B changed from `$10` to `$18`. These are bounded
state/callback observations; they do not establish sprite dimensions, rendered appearance, or
persistence.

### Runtime questions — entity lifecycle/presentation

**Unknown:** the remaining grouped H3 queue is exactly:

- `map-script-entity-lifecycle-presentation/normal-story-reachability` for ordinary map-script
  admission and story contexts.
- `map-script-entity-lifecycle-presentation/full-entity-state-callback-effects` for unobserved
  selector/state combinations and callback consequences outside this bounded harness.
- `map-script-entity-lifecycle-presentation/player-visible-presentation-timing-collision-persistence`
  for visible output, natural timing, collision/pathfinding, and persistence.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` macros `hide`,
`startEntity`, `stopEntity`, `waitIdle`, `setSprite`, `setPriority`, `removeShadow`, and `setSize`;
`code/common/scripting/map/mapscriptengine_1.asm` named sections,
`code/common/scripting/map/mapfunctions.asm` `InitializeMapEntities`,
`code/common/stats/combatantstats_1.asm` `GetCurrentHp`,
`code/common/stats/combatantstats_3.asm` `GetCombatantWord`, H1 listing symbols/addresses; and local
US ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`. Reproduce the static
contract with `uv run sf2 h2 map-script-engine`; reproduce the bounded runtime observation with
`uv run sf2 h3 map-entity-lifecycle-presentation --timeout-seconds 120`. Observed results are fixture
ID `sf2-map-script-engine-static-v1` in `tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.entityLifecyclePresentationCommandFacts`, and H3 fixture ID
`sf2-map-entity-lifecycle-presentation-runtime-v1` in
`tests/fixtures/h3/map-entity-lifecycle-presentation-v1.json`, with recursively closed schemas
`schemas/h3-map-entity-lifecycle-presentation-fixture.schema.json` and
`schemas/h3-map-entity-lifecycle-presentation-observation.schema.json`; the H3 command reported 11
cases, 8 handlers, 1 session-only launch, PASS.

## Confirmed Map-Script Entity Gesture/Relationship/Motion Command Family

Evidence date: 2026-07-30.

**Confirmed:** `sf2-map-script-engine-static-v1` field
`expected.entityGestureRelationshipMotionCommandFacts` retains seven source-named macro forms in source
order: `shiver` `$2A` (191 commands, 4 encoded bytes, 2 operand bytes), `nod` `$26` (169, 4, 2),
`followEntity` `$2C` (160, 8, 6), `faceEntity` `$52` (15, 6, 4), `moveNextToPlayer` `$28` (7, 6,
4), `fly` `$2F` (2, 6, 4), and `moveEntityAboveAnother` `$31` (1, 6, 4). The raw macro comments
and byte/word widths remain source facts, including the empty first and second comments of
`moveEntityAboveAnother`; source spelling and physical layout do not establish an entity action,
relationship, movement, layer, facing, or presentation effect.

**Confirmed:** the complete source corpus contains 545 ordered command occurrences in 133 non-empty
source-site rows, and all 304 declared programs remain as zero-inclusive `programTotals` rows. The
extractor separately retains compact scalar source-site/program order keys, complete raw rows, and their
exact order hashes: `A8EAB146BD07272B5D63DD1ADE4FF4BCF941B0D169E9FEDB92B0F70DE55DE022` for source sites and
`62D7A6F5A4A7FF8ABA021555F3FF3BAD8B96F6F5A67910FEF257FC7E76CDAFB8` for program totals.

**Confirmed:** the seven bounded H1/ROM handler sections are `csc2A_entityShiver` `$46DEE` (19
statements), `csc26_entityNodHead` `$46C70` (18), `csc2C_followEntity` `$46E58` (19),
`csc52_faceEntity` `$46F58` (33), `csc28_moveEntityNextToPlayer` `$46D10` (44), `csc2F_fly`
`$46EA8` (8), and `csc31_moveEntityAboveEntity` `$47030` (9). Their guards retain complete statement
and direct-call order, A6 transfer versus non-advancing probe widths, branch polarity and resolved
target-first-instruction pairs, loop targets, literal source instructions, and return boundaries.
Parsed constant use-site records retain the four `MAP_TILE_SIZE` operands (value 384), `UP` (1),
`LEFT` (2), `DIRECTION_MASK` (3), and the four `faceEntity` direction-symbol operands; these are
source operand facts, not behavior labels.

**Confirmed:** comment-stripping instruction parsing retains ten direct instruction targets with
zero-inclusive per-handler, direct/effective, internal/external maps. External totals are
`GetEntityAddressFromCharacter` 11, `UpdateEntitySprite_0` 5, `Sleep` 5, `LoadMapsprite` 1,
`sub_45D70` 1, `DmaMapsprite` 1, `AdjustScriptPointerByCharacterAliveStatus` 1, `AddFollower` 2,
`WaitForVInt` 1, and `WaitForEntityToStopMoving` 2; every internal total is zero and no current call
resolves through a jump-interface alias.

**Confirmed (bounded H3 observer):** one session-only Map Test 0 replay of 17 fixed records entered
all seven named handlers and compared each complete record in
`sf2-map-entity-gesture-relationship-motion-runtime-v1`. The `shiver` row observed three guarded
source-local cycles: temporary sprite-size word 21 and animation-counter byte `$FF`, then the seeded
restored size `$1234` and counter `$55`, with three flags-B set and clear writes and the corresponding
post-write bit states. The `nod` row observed its guarded final animation-counter write of 0. These
are handler-seam observations; callback-caused post-handler fields remain exact record data, not a
claim about a rendered or persistent effect.

**Confirmed (bounded H3 observer):** two controlled `followEntity` selector rows reached the parsed
`AddFollower` target order, while its zero-current-HP row used the non-advancing first script-word
high-byte probe (character byte 0 from `$0002`) and returned through the adjustment boundary. Five
`faceEntity` operand/tie rows, four `moveNextToPlayer` operand rows, both `fly` sides, and the one
`moveEntityAboveAnother` row preserve their exact 16-bit source-local words, callback/register records,
and forced wait-exit seam. The guarded fly writes observed layer bytes 0 and 16. These source-named
records establish neither a normal-story context nor a player-visible direction, movement, following,
collision, or presentation interpretation.

### Runtime questions — entity gesture/relationship/motion

**Unknown:** the remaining grouped H3 queue is exactly:

- `map-script-entity-gesture-relationship-motion/normal-story-reachability` for ordinary map-script
  admission and story contexts.
- `map-script-entity-gesture-relationship-motion/full-entity-state-callback-effects` for unseeded
  selector/state combinations and callback consequences outside this controlled matrix.
- `map-script-entity-gesture-relationship-motion/player-visible-presentation-timing-collision-persistence`
  for visible output, natural timing, collision/pathfinding, and persistence.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` macros `shiver`, `nod`,
`followEntity`, `faceEntity`, `moveNextToPlayer`, `fly`, and `moveEntityAboveAnother`;
`code/common/scripting/map/mapscriptengine_1.asm` named sections above; H1 listing symbols/addresses;
and local US ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Reproduce the static contract with `uv run sf2 h2 map-script-engine`; observed result is fixture ID
`sf2-map-script-engine-static-v1` in `tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.entityGestureRelationshipMotionCommandFacts`. Reproduce the bounded runtime observation with
`uv run sf2 h3 map-entity-gesture-relationship-motion --timeout-seconds 180`; it uses
`tools/bizhawk/map_entity_gesture_relationship_motion_observer.lua` and
`src/sf2tool/h3/map_entity_gesture_relationship_motion.py`, fixture ID
`sf2-map-entity-gesture-relationship-motion-runtime-v1` at
`tests/fixtures/h3/map-entity-gesture-relationship-motion-v1.json`, and recursively closed schemas
`schemas/h3-map-entity-gesture-relationship-motion-fixture.schema.json` and
`schemas/h3-map-entity-gesture-relationship-motion-observation.schema.json`. The command reported 17
cases, 7 handlers, 1 session-only launch, PASS.

## Confirmed Map-Script Screen/Map Presentation Command Family

Evidence date: 2026-07-31.

**Confirmed:** `sf2-map-script-engine-static-v1` field
`expected.screenPresentationCommandFacts` retains twelve source-named macro forms in source order:
`setQuake` `$33` (194 commands, 4 encoded bytes, 2 operand bytes), `fadeInB` `$39` (98, 2, 0),
`fadeOutB` `$3A` (10, 2, 0), `slowFadeInB` `$3B` (1, 2, 0), `slowFadeOutB` `$3C` (0, 2, 0),
`tintMap` `$3D` (11, 2, 0), `flickerOnce` `$3E` (5, 2, 0), `mapFadeOutToWhite` `$3F` (15, 2, 0),
`mapFadeInFromWhite` `$40` (15, 2, 0), `flashScreenWhite` `$41` (96, 4, 2),
`fadeInFromBlackHalf` `$4A` (8, 2, 0), and `fadeOutToBlackHalf` `$4B` (6, 2, 0). The only raw
macro comments are `setQuake`'s exact `? ($4000-, $8000-` and `flashScreenWhite`'s `duration`; source
names, comments, and physical layout do not establish a visual, timing, palette, VDP, or reachability
result.

**Confirmed:** the complete source corpus contains 459 ordered command occurrences in 115 non-empty
source-site rows, and all 304 declared programs remain as zero-inclusive `programTotals` rows. The
extractor retains compact scalar source-site/program order keys, complete raw rows, and exact order
hashes: `EE24CB393511FD9640AC96E427815CBC1851B2A6384A9D045FE74CC7E28F0948` for source sites and
`DB8AFFDF9AE1FE4B119CF916EB1F9792A383F5BD7FE6B7F95B7FD7CBE8F3107F` for program totals.

**Confirmed:** the twelve bounded H1/ROM handler sections are `csc33_setQuakeAmount` `$4651E` (23
statements), `csc39_fadeInFromBlack` `$46604` (2), `csc3A_fadeOutToBlack` `$4660A` (2),
`csc3B_slowFadeInFromBlack` `$46610` (5), `csc3C_slowFadeOutToBlack` `$46624` (5),
`csc3D_tintMap` `$46638` (6), `csc3E_FlickerOnce` `$46646` (6), `csc3F_fadeMapOutToWhite` `$46654`
(6), `csc40_fadeMapInFromWhite` `$46662` (6), `csc41_flashScreenWhite` `$46670` (10),
`csc4A_fadeInFromBlackHalf` `$46788` (6), and `csc4B_fadeOutToBlackHalf` `$46796` (6). Their guards
retain complete statement/direct-call order, A6 transfers, source immediate use sites, raw stored-operand
instructions, branch polarity/target identity, loop targets, and return boundaries. The parsed `$3FFF`,
`$F`, `$E`, `$28`, `FLASH_QUICKLY_2`, and other immediate records remain source instruction facts, not
interpreted masks, durations, or visual settings.

**Confirmed:** instruction-scoped caller parsing retains five direct/effective service identities and
their zero-inclusive per-handler maps: `Sleep` 1, `FadeInFromBlack` 2, `FadeOutToBlack` 2,
`LaunchFading` 7, and `DuplicatePalettes` 1. All internal totals are zero. The seven
`LaunchFading` sites retain both instruction target identity and the source `pc-relative` addressing
form; no call currently resolves through a jump-interface alias. This is a service boundary only: the
called implementation is outside this slice.

### Bounded runtime matrix — screen/map presentation

**Confirmed (H3):** `sf2-map-script-screen-presentation-runtime-v1` runs 22 handler-local records in
one BizHawk 2.11.1 / Genesis Plus GX Map Test 0 launch. It rehashes all 459 H2 source command records
and their 304 zero-inclusive program totals before case derivation. The source-backed rows cover all
twelve handler entries: three `setQuake` bit-test paths (unflagged `$0000`, `$4002`, and `$8002`), every
zero-operand form, the nine distinct source-observed `flashScreenWhite` right-shift results from
durations 2, 10, 20, 30, 40, 50, 60, 70, and 90, and one explicitly marked controlled case for the
zero-use `slowFadeOutB` form. It confirms only handler-local facts: actual entry/return PCs, A6 cursor
advance, stack restoration, direct target/call-site/return chronology, the `setQuake` word writes,
slow-fade counter-byte read/write/restore sequence, flash shifted word and loop count, and source-set
register words at direct calls.

**Confirmed (H3 limitation):** `Sleep`, `FadeInFromBlack`, `FadeOutToBlack`, `LaunchFading`, and
`DuplicatePalettes` execute session-only entry `rts` shims after their actual target PCs are observed.
Their original bodies do not execute; no result claims service-body effects, visible output, palettes,
VDP state, timing, completion/repeat, persistence, story reachability, or map/entity interaction.

### Runtime questions — screen/map presentation

**Unknown:** the remaining grouped queues are exactly:

- `map-script-screen-presentation/normal-story-reachability`: normal script and save-state reachability;
- `map-script-screen-presentation/visible-palette-vdp-and-frame-timing`: player-visible output,
  palette/VDP state, and frame timing;
- `map-script-screen-presentation/service-body-completion-repeat-and-persistence`: unshimmed service
  execution, completion/repeat behavior, and persistence;
- `map-script-screen-presentation/map-and-entity-state-interactions`: effects outside the directly
  observed handler instructions.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` macros named above;
`code/common/scripting/map/mapscriptengine_1.asm` named sections above; H1 listing symbols/addresses;
and local US ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Reproduce static facts with `uv run sf2 h2 map-script-engine`; observed result is fixture ID
`sf2-map-script-engine-static-v1` in `tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.screenPresentationCommandFacts`. Reproduce the bounded runtime seam with
`uv run sf2 h3 map-script-screen-presentation --timeout-seconds 180`; its fixture is
`tests/fixtures/h3/map-script-screen-presentation-v1.json` and its recursively closed contracts are
`schemas/h3-map-script-screen-presentation-fixture.schema.json` and
`schemas/h3-map-script-screen-presentation-observation.schema.json`.

## Confirmed Map-Script Entity Presentation-FX Command Family

Evidence date: 2026-08-01.

**Confirmed:** `sf2-map-script-engine-static-v1` field
`expected.entityPresentationFxCommandFacts` retains three source-named macro forms in source order:
`animEntityFX` `$22` (66 commands, 6 encoded bytes, 4 operand bytes), `headshake` `$27` (63, 4,
2), and `entityFlashWhite` `$18` (48, 6, 4). The physical operand annotations are exact: entity-to-act
for all three, transition-type via `defineShorthand.w ENTITY_TRANSITION_` for `animEntityFX`, and
duration for `entityFlashWhite`. These source spellings, comments, and encodings do not establish an
effect, selector meaning, lifetime, or visual result.

**Confirmed:** the bounded corpus has 177 ordered command occurrences in 61 non-empty source-site rows,
with all 304 declared programs retained as zero-inclusive `programTotals` rows. The extractor retains
complete raw rows plus exact compact order keys and hashes:
`A5A1424438C21C3A3B7602F8537851AD559F1193E72B5D998AF184BED04B4738` for source sites and
`921183412DB9E4E0BE1CAE4960A9702CD410BB85886CD92C967EED89AAE2CDB0` for program totals.

**Confirmed:** the three bounded H1/ROM handler sections are
`csc22_animateEntityFadeInOrOut` `$46B42` (31 statements), `csc27_entityShakeHead` `$46CB4` (22),
and `csc18_flashEntityWhite` `$469DE` (14). Their complete guards retain A6 reads and advance widths,
immediate and source-operand instruction records, direct-call order/target form, local loop target
identity, and return boundaries. The two `beq.w loc_46BE2` records in the first section additionally
resolve to the separately marked `loc_46BE2` function chunk and its first `tst.w d1` instruction; this
is a source control-flow target, not a runtime selector interpretation. Parsed numeric records such as
`#3`, `#22`, `#6`, `%100`, and `%11111011` remain source instruction values rather than named scales,
durations, masks, or effects.

**Confirmed:** instruction-scoped caller parsing retains nine direct/effective target identities with
zero-inclusive per-handler maps: `GetEntityAddressFromCharacter` 3, `LoadMapsprite` 4,
`ApplySpriteCropEffect` 1, `DmaMapsprite` 4, `WaitForVInt` 12, `sub_45E10` 1, `sub_45D1C` 1,
`UpdateEntitySprite_0` 4, and `sub_45D46` 1. All internal totals are zero; all nine effective targets
are external and no direct call resolves through a jump-interface alias. The inventory retains call-site
identity only, not a claim about callee effects.

**Confirmed (H3):** fixture `sf2-map-script-entity-presentation-fx-runtime-v1` at
`tests/fixtures/h3/map-script-entity-presentation-fx-v1.json` runs ten cases in one BizHawk 2.11.1 /
Genesis Plus GX launch. It rehashes the complete H2 source-site corpus before fixture comparison, then
uses every source-observed transition selector 2–7, one headshake row with entity word 0, and the
source-observed flash-duration boundaries 10, 57, and 180. The non-aligned 57 input preserves the
`lsr.w #2,d7` source use site; no unit or visual meaning is assigned to the macro's `duration` comment.

**Confirmed:** the session-only seam is guarded inside named `RunMapSetupInitFunction` at H1 `$474FC`:
after `GetCurrentMapSetup`, its `cmpi.w #-1,(a0)`, `bne.s loc_4750E`, and `bra.w loc_47514` retain
their source order and polarity; the non-sentinel path loads `MAPSETUP_OFFSET_INIT_FUNCTION(a0)` into
A0, invokes the indirect `jsr (a0)` at `$47512`, and resumes at `$47514`. The fixture binds both
injection PCs and preflights the source ROM bytes before the observer registers execute callbacks. This
is harness routing evidence, not a claim about ordinary map setup reachability.

**Confirmed (H3):** actual M68K entry PC observations reach all three named handlers. Selectors 2–5
return at H1 `$46BB0`; selector 6 reaches `loc_46BE2` with observed `d1=$0000`, selector 7 reaches
the same chunk with `d1=$FFFF`, and both return at `$46C1E`. Headshake and flash return at `$46D0E`
and `$46A10`. The corresponding local loop counts are 23, 16, 7, and 3/15/46. The selector-4 case is
the only table-loop case that executes its guarded post-loop block; chunk selector 6 records 0 shifts
and 16 adds, while selector 7 records 16 shifts and 0 adds. These are operand, branch, loop, and return
facts only—not a claim that the source labels describe visible transitions.

**Confirmed (H3):** before either selector-6/7 branch enters `loc_46BE2`, the named
`csc22_animateEntityFadeInOrOut` section unconditionally reaches its first direct call at H1 `$46B44`
(`GetEntityAddressFromCharacter`), its effective entry `$4704A`, and its return-resumption `$46B48`.
Each special-selector case therefore retains that one callback before its 16 repeated chunk sequences.
This is source/control-flow chronology only; it does not establish a service-body or presentation effect.

**Confirmed (H3):** all 1,147 direct callback events are internally checked in actual call-site PC,
effective-target-entry PC, and return-resumption order. The fixture represents that exact chronology as
closed callback records plus compact repeated segments, rather than expanding each repeated loop into a
new schema tree. All nine H2 target identities remain direct equals effective and all per-case target maps
are zero-inclusive. Eight effective entries are session-ROM shims after a source/ROM preflight:
`GetEntityAddressFromCharacter` executes only `lea ENTITY_DATA,a5; rts`; `LoadMapsprite`,
`ApplySpriteCropEffect`, `DmaMapsprite`, `sub_45E10`, `sub_45D1C`, `UpdateEntitySprite_0`, and
`sub_45D46` execute only `rts`. Their original bodies do not execute in this matrix. `WaitForVInt`
remains unpatched because it participates in the boot/menu cadence; its original entry, body, and return
execute, but no whole-service, hardware-timing, or presentation effect is inferred. No jump-interface
alias occurs.

**Confirmed (H3):** with the session seed `ENTITYDEF_OFFSET_ANIMCOUNTER=$7F`, headshake observes the
initial `$FF` byte write and final `$00` byte write. With `ENTITYDEF_OFFSET_FLAGS_B=$A1`, all three flash
cases observe `$A5` after the `%100` OR write and `$A1` after the `%11111011` AND write. These are the
only direct entity-state byte writes promoted by this fixture; it does not claim a full-record mutation,
rendering, timing, persistence, or player-visible result.

### Remaining H3 Runtime Questions

**Unknown:** the remaining grouped H3 queue is exactly:

- `map-script-entity-presentation-fx/normal-story-reachability`: normal script/save-state reachability;
- `map-script-entity-presentation-fx/player-visible-output-timing-completion-repeat`: visible output,
  timing, completion, and repeat behavior;
- `map-script-entity-presentation-fx/service-body-map-entity-state-effects`: bypassed-service effects and
  the complete `WaitForVInt` service/hardware path;
- `map-script-entity-presentation-fx/persistence-and-map-entity-interactions`: persistence and unmodeled
  map/entity-state interactions.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` macros named above;
`code/common/scripting/map/mapscriptengine_1.asm` named sections and `loc_46BE2` function chunk above;
`code/common/scripting/map/mapsetupsfunctions_1.asm` `RunMapSetupInitFunction`; H1 listing
symbols/addresses; and local US ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`. Reproduce with
`uv run sf2 h2 map-script-engine`; H3 is reproduced with
`uv run sf2 h3 map-script-entity-presentation-fx --timeout-seconds 180`; the H2 observed result is fixture ID
`sf2-map-script-engine-static-v1` in `tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.entityPresentationFxCommandFacts`; the H3 fixture and recursively closed contracts are
`tests/fixtures/h3/map-script-entity-presentation-fx-v1.json`,
`schemas/h3-map-script-entity-presentation-fx-fixture.schema.json`, and
`schemas/h3-map-script-entity-presentation-fx-observation.schema.json`.

## Confirmed Map-Script UI Primary Command Boundary

Evidence date: 2026-07-31.

**Confirmed:** `sf2-map-script-engine-static-v1` field
`expected.mapScriptUiPrimaryCommandFacts` retains three source-named primary forms in source order:
`showPortrait` `$1D` (4 commands, 4 encoded bytes, 2 operand bytes), `hidePortrait` `$1E` (1, 2,
0), and `menu` `$12` (0, 4, 2). `showPortrait` emits two direct byte operands with its exact source
comments; `menu` emits one direct word whose source comment is exactly empty. These names and comments do
not establish drawing, input, choice, window timing, reachability, or save behavior.

**Confirmed:** the complete corpus contains five ordered command occurrences in four non-empty source-site
rows: four `showPortrait` and one `hidePortrait`; `menu` has no source occurrence. All 304 declared
programs remain zero-inclusive `programTotals` rows, including an exact zero `menu` count. Exact compact
source-site/program order hashes are `FDF32E72E55D28E7EBC57BB5963658F6A4B10DE7C1920A2A69F75D1A90D4CC4A`
and `63EBE7909405F52FAD4D9C4E24050213E107CD9ABCBF7A709A4A7AA9F4F5EA1D`.

**Confirmed:** the bounded H1/ROM handler sections are `csc1D_showPortrait` `$46A98` (20 statements) and
`csc1E_hidePortrait` `$46AD2` (3) in `mapscriptengine_1.asm`, plus
`csc12_executeContextMenu` `$474B6` (13) in `mapscriptengine_2.asm`. Their complete guards retain A6
transfer and advance widths, source immediates/operand instructions, branch polarity/target identity,
direct-call order and return boundaries. The third handler also records its exact source stack-pointer
transfer instructions separately from its 2-byte A6 command read. These are source-control-flow records,
not a model of any callee's behavior.

**Confirmed:** `portraitHelperJoin` is a provenance join to the existing
`dialogueCommandFacts.portraitHelper`, not a second parser for the same handler. It cross-validates
`showPortrait`'s two one-byte operands against that fact's `move.w (a6)+,d0`, address, source path,
tested modifier-byte mask `192`, and two bit-test records. The join does not interpret either packed byte.

**Confirmed:** instruction-scoped caller parsing retains seven direct targets and alias-aware effective
targets with zero-inclusive per-handler maps. Direct totals are `WaitForViewScrollEnd` 2,
`GetEntityPortaitAndSpeechSfx` 1, `j_OpenPortraitWindow` 1, `j_ClosePortraitWindow` 1,
`j_ChurchMenu` 1, `j_ShopMenu` 1, and `j_BlacksmithMenu` 1. Jump-interface aliases resolve to
`OpenPortraitWindow`, `ClosePortraitWindow`, `ChurchMenu`, `ShopMenu`, and `BlacksmithMenu` respectively;
all internal totals are zero. This preserves call identity and alias provenance only.

### Bounded runtime matrix — UI primary commands

**Confirmed:** `sf2-map-script-ui-primary-runtime-v1` executes eleven handler-local cases in one
BizHawk Map Test 0 launch. The four actual `showPortrait` source input rows remain distinct by their
full H2 `sourceOrderKey` and input provenance: `cs_615E6:558:showPortrait` supplies packed word
`$8084`; the three debug rows each supply `$0000`. The H3 builder derives those rows from the complete
H2 `sourceSites` records, then cross-checks the compact fixture's five-key order and SHA-256 before it
compares any H3 golden. The compact H2 hash is not used to invent input rows.

**Confirmed:** the synthetic busy-word row writes nonzero `PORTRAIT_WINDOW_INDEX` `$FFB080`, reaches
`csc1D_showPortrait`, advances its A6 word read, executes no direct callback call site, and returns. A
separate controlled `d1=$FFFF` row reaches the same handler's parsed `cmpi.w #-1,d1` at `$46ABE` after
the direct helper call site and returns without the open-window direct call site. This is a bounded
handler sentinel branch; it does not establish an entity/table meaning for `$FFFF`.

**Confirmed:** the hide row reaches `csc1E_hidePortrait`, observes the parsed call-site identities
`WaitForViewScrollEnd` then `j_ClosePortraitWindow`/`ClosePortraitWindow`, and reaches its handler
return. The four menu rows enter `csc12_executeContextMenu` with selector words `0`, `1`, `2`, and `3`:
the first three observe exactly one respective parsed alias pair `j_ChurchMenu`/`ChurchMenu`,
`j_ShopMenu`/`ShopMenu`, or `j_BlacksmithMenu`/`BlacksmithMenu`; `3` observes none. All four menu rows restore the saved A6 after
its two-byte input cursor advance and restore the stack pointer to its entry value. Instruction target,
effective target, actual call-site PC, actual shim-target PC/role, actual callback-return PC, handler
`rts` PC, and handler-side continuation PC stay distinct fields. The stack observation is made at the
parsed `rts` before that instruction consumes the handler call return address, so it remains distinct
from the trampoline's later return boundary.

**Confirmed boundary:** the session-only ROM retains every actual handler direct-call instruction,
observes its call-site and return-resumption PCs, and replaces only the parsed shim entry span. The
observer separately records the shim entry PC and whether it is the parsed effective target or the
instruction target. `WaitForViewScrollEnd` and the portrait helper execute their effective target shims;
each menu/window jump-interface alias executes its instruction-target shim and returns before its parsed
effective target executes. The `WaitForViewScrollEnd` and alias shims begin with `rts`; the portrait
helper's eight-byte entry returns the case-provided `d1` word from harness RAM and then returns. Those
exact identities, roles, addresses, original bytes, and replacement spans are fixture-pinned and
ROM-verified before launch. The menu shims preserve the handler's selector `d0` instead of fabricating a
menu result. It therefore proves handler-local dispatch, cursor/stack state, and return behavior, but
does not execute or claim any portrait-window, church, shop, or blacksmith service effect; such effects
are unobserved. The observer writes no `runtimeGolden` into its runtime configuration.

### Runtime questions — UI primary commands

**Unknown:** `map-script-ui-command/normal-story-reachability` remains a grouped route question.

**Unknown:** `map-script-ui-command/full-window-animation-vdp-timing` remains a grouped presentation
question.

**Unknown:** `map-script-ui-command/real-user-choice-service-side-effects` remains a grouped service
and input-result question.

**Unknown:** `map-script-ui-command/save-persistence-map-entity-interactions` remains a grouped
lifecycle question. No source macro, comment, handler, literal, field, jump alias, or callee name
promotes these outcomes from the bounded matrix.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` macros named above;
`code/common/scripting/map/mapscriptengine_1.asm` and `mapscriptengine_2.asm` sections above; H1 listing
symbols/addresses; and local US ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`. Reproduce with
`uv run sf2 h2 map-script-engine`; observed static result is fixture ID
`sf2-map-script-engine-static-v1` in `tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.mapScriptUiPrimaryCommandFacts`. Reproduce the runtime boundary with
`uv run sf2 h3 map-script-ui-primary --timeout-seconds 180`; observed result is fixture ID
`sf2-map-script-ui-primary-runtime-v1` in `tests/fixtures/h3/map-script-ui-primary-v1.json`.

## Confirmed Map-Script Control/Audio Macro Boundary

Evidence date: 2026-08-01.

**Confirmed:** `sf2-map-script-engine-static-v1` field
`expected.scriptControlCommandFacts` records seven source-named forms in this exact source order:
`csWait` (2 encoded bytes: literal `$80`, then one byte operand), `playSound` `$05` (4 bytes, one
word operand), `csc06` `$06` (2 bytes, no operand), `executeSubroutine` `$0A` (6 bytes, one long
operand), `jump` `$0B` (6 bytes, one long operand), `cscNop` (a source no-op that emits zero bytes),
and `csc_end` (one `$FFFF` word, 2 bytes). The source labels and `playSound` operand comments do not
establish a duration unit, audible result, player-facing lifecycle, or story meaning.

**Confirmed:** the complete 304-program corpus contains 2,336 occurrences and 5,822 physically
encoded source bytes: 1,591 `csWait`, 309 `playSound`, zero `csc06`, 122 `executeSubroutine`, 11
`jump`, zero `cscNop`, and 303 `csc_end`. Each source-site record retains its program ID, source path,
program address when the source record has one (otherwise explicit null), command index, source line,
source form, and encoded-byte count. The compact,
order-sensitive SHA-256 guards are `5D853AB05645D181B4B2934242817A42DD40547CB4021181E21B01D11D387571`
for all source sites and `9FE27CBCA9C4911C82667541384CD0AF3E5BEFCE7D3AF248602D1146F796F2C7` for all
304 zero-inclusive program totals. These byte counts and addresses are separate source-storage facts;
they are not transfer sizes, timer units, or runtime iteration counts.

**Confirmed:** the bounded `ExecuteMapScript` loop section `loc_47156` at H1 `$47156` reads a word
from A6 with post-increment, compares it with signed `-1`, and branches to `loc_47234`. Its negative
path retains the `bpl.s loc_47174` polarity, the `SKIP_CUTSCENE_TEXT` read/`bne.s loc_47172` gate,
`andi.w #BYTE_MASK,d0` with parsed value 255, the direct `(Sleep).w` call, and its loop branch in
source order. The non-negative path doubles D0, reads the word-offset dispatch table, calls through
that table, then loops. The guard cross-checks the signed comparison against the emitted `$FFFF`
`csc_end` word; it is a static control-flow relationship, not a claim that `Sleep` or the skip gate
has a particular player-visible outcome.

**Confirmed:** four bounded handler sections in
`code/common/scripting/map/mapscriptengine_2.asm` retain exact order and cursor records:
`csc05_playSound` at H1 `$47378` reads one A6 word then executes
`sndCom SOUND_COMMAND_GET_D0_PARAMETER`; `csc06_doNothing` at `$47380` is only `rts` and performs no
A6 read; `csc0A_executeSubroutine` at `$4740A` reads one A6 long into A1, saves A0, calls `(a1)`,
restores A0, and returns; and `csc0B_jump` at `$47414` reads a long from `(a6)` into A6 without
post-increment and returns. The long transfer and zero cursor advance of the last form remain
separate fields.

**Confirmed:** instruction-scoped target inventories retain caller identity rather than raw symbol
counts. `executeSubroutine` has 122 source uses over 47 declared/active instruction and effective
targets; `jump` has 11 uses, a complete 348-label declared target domain, and 10 positive target rows.
Both internal/external instruction/effective maps are zero-inclusive, and neither inventory has an
unresolved operand. Three `executeSubroutine` instruction targets resolve through verified aliases:
`j_FadeOut_WaitForP1Input` → `FadeOut_WaitForP1Input`, `j_PlayEndingKissSequence` →
`PlayEndingKissSequence`, and `j_csub_40F2` → `csub_40F2`. The complete target-inventory hash is
`197FC6AC1D8E4E2CAA82FDC3A00FFF3AE07F2F8D6E64BCBA41A03A8980A084C8`.

**Confirmed:** `playSound` joins only source operands to the maintained sound-data contract
`sf2-sound-data-static-v1`, fixture `tests/fixtures/h2/sound-data-static-v1.json`; it does not
reparse or characterize the sound driver. Its 309 uses are source-enum categories `music` 58, `sfx`
224, and `sound-command` 27 across 51 distinct source symbols. The complete join, including its
fixture identity and `sf2enums.asm` source identity, is guarded by SHA-256
`205C2F9E7CC481B7C5B387D2736635FC40E5763C41544A3A8C9D0D205F4FA654`.

**Confirmed — map-event direct `sndCom` source-reference boundary:** this is a separate static
source/H1 inventory, not an extension of the map-script `playSound` corpus. `sf2macros.asm:27-29`
defines `sndCom` as the ordered source emission `trap #sound_command` then `dc.w \1`. Across the
complete 684 entity, 150 zone, and 80 item target-program corpus, exactly three direct uses occur in
one positive caller, `Map20_21F_ZoneEvent0` at
`data/maps/entries/map20/mapsetups/s3_zoneevents_543.asm:26,40-41`: in order,
`SOUND_COMMAND_FADE_OUT` `$FD`/253, `SOUND_COMMAND_INIT_DRIVER` `$20`/32, and `MUSIC_TOWN`
`$08`/8. The zero-inclusive source computation covers all 914 caller programs and derives one
positive plus 913 zero-use callers without materializing 913 redundant zero records. Each of the
three source/H1 sites has physical-program, physical-record, setup-reference, and route-reference
weight 1, so every aggregate weight is 3. The operand categories are source namespaces only:
`sound-command` 2, `music` 1, `sfx` 0. The join checks `sf2enums.asm` directly against the pinned
`sf2-sound-data-static-v1` domain (`music` 1–64; `sfx` 65–120) but does not reparse or characterize
the sound driver. The compact exact-order site list, one-positive-caller record, and complete
summary are fixture-pinned and rebuilt from the parsed use sites before comparison.

**Unknown:** no H3 run was added for this direct map-event source slice. The static trap emission
and enum join do not establish normal-story reachability, trap/driver dispatch, audible or timed
behavior, Z80/YM/PSG state, persistence, or caller-context lifetime. Provenance: pinned
`ShiningForceCentral/SF2DISASM` `master` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`, the
source/H1 rows above, `sf2enums.asm`, local USA ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`, and
`uv run sf2 h2 map-events`; observed output is `sf2-map-events-static-v1`, fixture
index `tests/fixtures/h2/map-events-static-v1.json`, semantic shard
`tests/fixtures/h2/map-events/sound-commands.json`, recomposed fields `soundCommand*`. Evidence date:
2026-08-02.

**Confirmed — retained map-event direct-control join:**
`sf2-map-event-direct-control-static-v1` consumes the already complete 914-program map-event corpus
without duplicating its map-script or service algorithms. Its caller-side relation retains the 205
raw direct `jsr`/`bsr`/`jmp` contexts, alias-aware effective targets, and only lexical call
continuations or tail suffixes. `ExecuteMapScript`, `DisplayText`, `Sleep`, and other effective
callee identities remain source-owner joins, not a second interpretation of their bodies. Normal
reachability, caller state/order, callee effects, timing, persistence, and presentation remain the
direct-control owner's grouped **Unknown** boundary.

**Confirmed — retained map-event direct-handoff join:**
`sf2-map-event-direct-handoff-static-v1` adds caller-side semantic handoff depth without changing
that topology: the maximal adjacent preceding raw setup group at all 205 direct transfers and the
first lexical consumer after each of 143 returning calls. Its 184 contextual/177 physical setup
operations and 143 contextual/139 physical continuations combine to 327 contextual/299 physical
source/H1/ROM-anchored operations, with 17 shared role PCs; 78 `sf2enums.asm` identities account for
120 contextual immediate uses. It retains direct-state and direct-control references rather than
copying their state, alias, target, or callee facts. Actual preparation path, register/CCR and
fixed-state values, callee entry/effects/returns, continuation selection, reachability, persistence,
timing, UI, dialogue, audio, and story meaning remain the handoff owner's grouped **Unknown**
boundary.

**Confirmed (bounded H3):** `sf2-map-script-control-audio-runtime-v1` runs six RAM-owned streams in
one BizHawk 2.11.1 / Genesis Plus GX Map Test 0 launch through the unmodified `ExecuteMapScript` loop.
The observer confirms the normal `csWait` D0=1 call's one `WaitForVInt` entry and the debug
P2-START/`DEBUG_MODE_TOGGLE` skip-gate path, `$06` dispatch and return, `$05`'s A6 word read and
`SOUND_COMMAND_GET_D0_PARAMETER` trap with `MUSIC_STOP` value 32, `$0A`'s four-byte cursor advance,
save/call/restore stack deltas, and target return, and `$0B`'s A6 replacement before `$FFFF` reaches
the common end/return boundary. The session-only trampoline replaces `$47512` `jsr (a0)` plus its
four-byte `$47514` `movem.l (sp)+,d0-a1` with a six-byte absolute JSR. In separately verified 20-byte
`$FF` padding it supplies the A0 RAM script, calls `ExecuteMapScript`, uses `addq.l #4,sp` to discard
the patched `$47518` return, then executes the original `movem.l (sp)+,d0-a1` and `rts` sequence. The
post-handler callback is the final stub `rts` seam `$FF9A`, after the restored-register epilogue; the
observer replays the saved Map Test prompt after that bounded completion. It does not patch the
interpreter, handlers, trap, or their callees. Every callback is wrapped in `pcall`; a callback exception
writes case plus expected/actual call-site, target, return, and pending state, deletes the observation,
removes all callback IDs, and exits nonzero. The passing run reports zero residual callbacks and no Lua
Console error.

### Grouped H3 runtime-question queue

**Unknown:** the same five IDs retain only their unresolved runtime meaning:

- `map-script-control-audio/wait-normal-and-skip-gate` — durations beyond D0=1 and normal-story input
  sampling;
- `map-script-control-audio/no-op-dispatch-boundary` — normal-story reachability beyond the synthetic
  `$06` dispatch/return seam;
- `map-script-control-audio/sound-dispatch-boundary` — sound-driver, Z80/YM/PSG, audible, and timing
  effects after the observed trap;
- `map-script-control-audio/subroutine-call-return-boundary` — arbitrary target effects, persistence,
  and caller-context behavior beyond the bounded direct-`rts` target;
- `map-script-control-audio/jump-cursor-redirect-and-end-boundary` — normal-story reachability and
  effects of real target streams.

Provenance: pinned `ShiningForceCentral/SF2DISASM` branch `master`, commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` forms named above;
`code/common/scripting/map/mapscriptengine_2.asm` `ExecuteMapScript`, `loc_47156`, and the four
named handlers; `sf2enums.asm`; H1 listing addresses above; and local US ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`. Reproduce with
`uv run pytest tests/python/test_map_script_engine.py -k script_control -q` and
`uv run sf2 h2 map-script-engine`; observed static result is fixture ID
`sf2-map-script-engine-static-v1` at `tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.scriptControlCommandFacts`. Reproduce the bounded runtime result with
`uv run sf2 h3 map-script-control-audio --timeout-seconds 180`; its fixture is
`tests/fixtures/h3/map-script-control-audio-v1.json`, ID
`sf2-map-script-control-audio-runtime-v1`, parsed by
`schemas/h3/h3-map-script-control-audio-observation.schema.json` and observed by
`tools/bizhawk/map_script_control_audio_observer.lua`.

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

**Confirmed runtime boundary:** the one-launch, 14-case Map Test 0 matrix
`sf2-force-state-roster-death-runtime-v1` at
`tests/fixtures/h3/force-state-roster-death-v1.json` enters all six original handlers through the
original `RunMapSetupInitFunction` seam. It records `csc08` absent/already-present membership,
`csc0E` empty/hit/miss cursor choices, `csc0F` dead/live HP choices, `csc1F` append, `csc20`
offscreen-skip/one-onscreen-append, and `csc21` empty/hit-first/hit-middle/miss removal. The fixture
owns the accepted ordered before/after values and A6 results; its verifier is
`src/sf2tool/h3/force_state_roster_death.py`.

**Confirmed scoped persistence boundary:** `GAME_FLAGS` joined membership and current HP are inside
the original `COMBATANT_DATA` logical SaveGame/LoadGame span `$FFE800..$FFF7AF` (4,016 bytes). The
absent `csc08` case executes original `JoinForce`, original `SaveGame` selector 0, narrowly clears
only the independently derived joined-flag byte, executes original `LoadGame`, and uses original
`CheckFlag` to observe the restored nonzero bit. The runtime records its independently derived selected
physical SRAM byte, checksum byte, and `SAVE_FLAGS` occupied bit with the source-derived two-byte
physical stride. The HP cases
are read-only branch observations in that saved logical domain: they do not invent a Save/poison/Load
operation.

**Confirmed handler-local boundary:** `DEAD_COMBATANTS_LIST_LENGTH` `$FFB402` and
`DEAD_COMBATANTS_LIST` `$FFB58A` are outside that logical save span. The list cases are therefore
scoped RAM/list observations with scoped restoration only, not save-persistence evidence. For the
bounded `csc20` seeds, the exact source loop has 32 `GetCombatantX` checks; the observer restores the
one seeded byte plus those 32 potential append positions. This 33-byte touched span is a probe
restoration range, not a list-capacity claim. The source/H1 and runtime results confirm `-1` skips the
write and a single X value of 0 appends `$80`; they do not establish normal caller sequencing.

**Unknown:** normal-story reachability, roster/list capacity, UI-visible roster/death effects,
service-menu/church/witch callers, copy/delete UI, cross-process SRAM durability, and corruption or
power-loss behavior remain outside this bounded rail.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `sf2cutscenemacros.asm`;
`code/common/scripting/map/mapscriptengine_1.asm` (`csc1F`, `csc20`, `csc21`),
`mapscriptengine_2.asm` (`csc08`, `csc0E`, `csc0F`), `mapsetupsfunctions_1.asm`,
`code/common/stats/battleparty.asm`, `code/common/tech/sram/sramfunctions.asm`,
`sf2const.asm`, `sf2enums.asm`, jump-interface aliases, H1 listing, and the verified US ROM hash.
Reproduce static joins with `uv run sf2 h2 map-script-engine`, `uv run sf2 h2 common-stats`, and
`uv run sf2 h2 tech-services`; reproduce the grouped runtime matrix with
`uv run sf2 h3 force-state-roster-death --timeout-seconds 180` (BizHawk 2.11.1, Genesis Plus GX).

## Confirmed Map-Script Active-Party/AI/Follower Command Family

Evidence date: 2026-07-30.

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

**Confirmed runtime boundary:** `sf2-force-state-active-party-runtime-v1` at
`tests/fixtures/h3/force-state-active-party-v1.json` records nine bounded Map Test 0 handler entries
in one matrix: already-active return, dead-active replacement, living-tail replacement, selector-zero
AI clear, selector-nonzero AI set/JoinForce, mixed reset, and empty/existing/duplicate follower inputs.
The fixture preserves exact fixture-owned streams/seeds, handler and effective-service chronology, and
compact before/after state surfaces. It confirms the handler-local `UpdateForce` list snapshot precedes
the replacement flag mutation, selector-zero versus selector-nonzero activation/join outcomes, the
30-ally ordered reset service cycle; its source mask 7 is applied before `UpdateCombatantStats`, and
the measured representative then ends with status 3 and restored HP/MP. The fixture also records follower append/duplicate behavior
with the allocated walking block pointer and three parameter words. It does not claim normal-story
reachability, save/load or capacity lifecycle, or visible presentation.

**Confirmed bounded outcomes:** the already-active input executes only `IsInBattleParty` and leaves the
dialogue index at `-1`. Both replacement paths materialize `UpdateForce`'s list before `LeaveBattleParty`
and `JoinBattleParty`: on handler return the flags have changed but the measured count/list remain the
earlier `2`/`[0,1]`; the removed member is 0 for the dead path and 1 for the living-tail path, with
dialogue indices 0 and 1 respectively. Selector zero has no `JoinForce` call; selector nonzero reaches it
and makes member 4 joined and active while the handler-local count/list remains the prior zero/empty
snapshot. Reset applies source mask 7 before `UpdateCombatantStats`; the representative ends status 3
with HP and MP restored. A duplicate follower leaves the follower list unchanged but still allocates its
42-byte walking block, advances the pointer, and writes the actscript and three parameter words. These
are Map Test handler-local observations, not lifecycle or presentation claims.

**Unknown:** `force-state/active-party-ai-follower/normal-story-reachability`,
`force-state/active-party-ai-follower/save-load-capacity-lifecycle`, and
`force-state/active-party-ai-follower/player-visible-presentation` remain separate queues.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `sf2cutscenemacros.asm` forms at lines 474-503;
`code/common/scripting/map/mapscriptengine_1.asm` named sections at lines 1637-1811; the H1 listing
addresses above; `sf2enums.asm::AIBITFIELD_AI_CONTROLLED`; and the follower/battle-stats owner files
named above.
Reproduce the static contract with `uv run sf2 h2 map-script-engine` and the bounded runtime matrix
with `uv run sf2 h3 force-state-active-party --timeout-seconds 180` (BizHawk 2.11.1, Genesis Plus GX,
9 cases, 4 handlers, 1 launch); observed result is fixture
`tests/fixtures/h2/map-script-engine-static-v1.json`, ID `sf2-map-script-engine-static-v1`, nested
field `expected.forceStateCommandFacts.activePartyCommandFacts`.

## Confirmed Map-Script Block-Copy Command Family

Evidence date: 2026-07-30.

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

**Confirmed (H3):** `sf2-map-block-mutation-runtime-v1` runs seven bounded replays in one BizHawk
launch through a session-only trampoline at the existing `RunMapSetupInitFunction` indirect-call seam.
It observes both original handlers returning after one direct `CopyMapBlocks` call, the three input
words at that call, and every source/destination word offset at the helper copy instruction. The two
one-cell forms, a six-word cross-row rectangle, and both forward horizontal and vertical overlap
directions all match the exact FF0000-layout readback records. This confirms only the observed
forward word-copy chronology and bounded layout values; the independent Python model remains an
expected-result check rather than evidence. For `$34`, the observed post-copy callbacks are bit 0 then
bit 1 at `$46570`/`$46578`, with the update-toggle byte changing according to those writes. For `$35`,
the observed matrices have no such callbacks and retain their seeded toggle byte. The observer records
actual handler/call/copy-site identities, inputs, offsets, bit states, final toggle byte, and layout
readbacks; it does not replace the original routines.

**Unknown:** collision/pathfinding consumer effects remain
`map-block-mutation/collision-pathfinding-consumer-effects`; normal-story reachability and
map-reload/save persistence remain
`map-block-mutation/normal-story-reachability-and-map-reload-save-persistence`; and visible VDP
presentation plus cycle/pixel timing remain
`map-block-mutation/visible-vdp-presentation-and-cycle-pixel-timing`. None is credited by the
bounded direct-layout matrix.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `sf2cutscenemacros.asm` lines 345-364;
`code/common/scripting/map/mapscriptengine_1.asm` lines 66-91; `sf2enums.asm:9`;
`code/gameflow/exploration/exploration.asm` lines 724-764; H1 addresses `$46566`, `$46582`, and
`$03DB0`; local US-ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`. Reproduce with
`uv run sf2 h2 map-script-engine`; the runtime matrix reproduces with
`uv run sf2 h3 map-block-mutation --timeout-seconds 180` (BizHawk 2.11.1, Genesis Plus GX, seven
cases, two handlers, one launch); observed result is
`tests/fixtures/h2/map-script-engine-static-v1.json`, ID `sf2-map-script-engine-static-v1`, field
`expected.mapBlockMutationCommandFacts`, and `tests/fixtures/h3/map-block-mutation-v1.json`, ID
`sf2-map-block-mutation-runtime-v1`. The H3 observer is
`tools/bizhawk/map_block_mutation_observer.lua`; its parsed observation contract is
`schemas/h3-map-block-mutation-observation.schema.json`.

## Confirmed Map-block Copy Show/Hide Lifecycle

Evidence date: 2026-08-13.

**Confirmed (H3):** `sf2-map-block-copy-lifecycle-runtime-v1` is a ten-case, one-launch, session-only
matrix for entity-action opcode `$40` (`ac_checkMapBlockCopy`). It callback-observes the original
`UpdateEntityData` entry `$5D6C` in each of the four dispatcher cases before `$40` dispatch, and
records the actual coordinate conversion call/return plus the selected original helper call/target/return.
Fading skips the probe; a non-`$0800`/`$0C00` masked block flag calls neither helper; `$0800` calls
`PerformMapBlockCopyScript`; and `$0C00` calls `csub_40F2`. The direct helper rows separately confirm
the busy and terminator/miss no-change gates, the positive-source snapshot-then-copy path, the
negative-source snapshot-then-clear path, and inactive/active saved-rectangle restore behavior.
The selected map 03 source rows preserve the source labels and data: trigger `(24,26)`, source `(51,20)`,
size `(9,7)`, destination `(22,51)` for copy; trigger `(4,8)`, source `(255,255)`, size `(7,8)`,
destination `(2,32)` for clear. The busy word receives the one-based matched roof-row scan ordinal
(not a Boolean): the selected sixth positive row leaves `6`, while the selected first negative row
leaves `1`. The four saved metadata words hold destination x/y and decremented width/height; the
sentinel follows the saved rectangle rather than occupying the buffer base. The direct restore rows
read all 63 saved words for the selected `9×7` rectangle. These bounded facts, including the
update-toggle byte, do not claim a full RAM layout or rendered result.

The complete source-use inventory is five `ac_checkMapBlockCopy` uses: three in
`data/scripting/entity/eas_main.asm` (lines 42, 54, 66) and two in
`data/scripting/map/debugscripts.asm` (lines 41, 44). Provenance: pinned
`ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `sf2cutscenemacros.asm` lines 852-854;
`entityscriptengine_2.asm` dispatcher table slot `$40` and `esc40_checkMapBlockCopy`;
`exploration.asm` `PerformMapBlockCopyScript`/`csub_40F2`; H1 `$4F02`, `$5D12`, `$5D6C`,
`$3FEA`, `$40F2`, and `$61FC`; US ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Reproduce with `uv run sf2 h3 map-block-copy-lifecycle --timeout-seconds 180`.

The verifier independently joins those source sections to H1 instruction bytes, widths, and next PCs,
then to the canonical ROM before it creates the session-only VInt probe. The source-named entry and
the canonical ROM's effective BSR entry may differ within the first H1 instruction; that alias is
guarded rather than normalized away. The observer snapshots, restores, and reads back only its selected
FF0000 layout words; saved-rectangle metadata/buffer/sentinel; busy/toggle; entity-0 fields; and generated
probe/action inputs. Each case emits a restoration milestone before the observer clears callbacks and
writes its result; a callback failure restores that same scope, removes output, clears registrations, and
reports its cleanup facts in the structured nonzero status. No all-RAM restoration claim is made.

**Unknown:** `map-block-copy-lifecycle/collision-pathfinding-navigation-effects`,
`map-block-copy-lifecycle/rendered-vdp-fade-audio-timing`, and
`map-block-copy-lifecycle/persistence-reload-normal-story-reachability` remain one grouped H3 queue.

## Confirmed Map-Script Entity Population/Reload Command Family

Evidence date: 2026-07-30.

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

**Confirmed (H3):** `sf2-entity-population-reload-runtime-v1` replays 12 bounded invocations in one
BizHawk 2.11.1 / Genesis Plus GX launch through the existing `RunMapSetupInitFunction` wrapper. A
session-only 16-byte FF-padding trampoline sets A6 to a RAM-owned source-shaped stream and calls each
unmodified handler. The three `newEntity` rows use source key `cs_55242:1:newEntity`; their selected
`ENTITY_INDEX_LIST` offset 46 changes to 1, 6, and 49 for empty, preexisting-5, and preexisting-48
seeds respectively, while the selected `ENTITY_DATA` records retain X/X-destination 4224, Y 0, facing
3, entity-number 1/6/49, and mapsprite 206. This is an observed bounded record/list result, not a
claim about a player-visible spawn lifecycle or capacity beyond the selected high-water seed.

**Confirmed (H3):** the direct-table row `cs_55832:2:loadMapEntities` reaches
`DisableDisplayAndInterrupts`, `InitializeMapEntities`, `LoadEntityMapsprites`, and
`EnableDisplayAndInterrupts` at the guarded H1 sites in that order; its callback input pointer is
`ce_559AE` ROM 350638 and its selected records 0/1 have exact fixture field values. The reload row
`cs_55832:76:reloadEntities` first resolves `ENTITY_INDEX_LIST[0]=5` through
`GetEntityAddressFromCharacter`, then reaches `InitializeMapEntities` with the selected slot-5
X/Y/facing seed (3456/4224/3) and table pointer `ms_map7_Entities` ROM 349126. All seven source rows
for `loadEntitiesFromMapSetup` execute with CURRENT_MAP 17 and cleared GAME_FLAGS; each reaches the
five guarded callback identities in order, including the direct `j_InitializeMapEntities` identity,
and the fixture preserves each exact three-word source input and selected post-handler records. These
are observed handler-local callback, cursor, list, and record facts; they do not assign the source
macro names an inferred gameplay lifecycle.

### Runtime Questions — Entity Population/Reload

**Unknown:** `entity-population-reload/allocation-capacity-beyond-observed-high-water` — the H3 matrix
observes allocation through selected index value 48 / output 49, while the independently parsed
`InitializeNewEntity` scan has 63 items and `ClearEntities` has a separate 49-record loop; no runtime
case isolates the remaining scan interval or its failure behavior.

**Unknown:** `entity-population-reload/normal-story-reachability-and-save-map-reload-persistence` —
the matrix enters handlers through a session-only wrapper and does not establish normal story callers,
save/load behavior, or a later map reload.

**Unknown:** `entity-population-reload/player-visible-rendering-animation-vdp-timing` — callbacks and
RAM records are observed, not sprites, animation, VDP state, or frame timing.

**Unknown:** `entity-population-reload/collision-pathfinding-consumer-effects` — no collision or
pathfinding consumer is observed.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` forms `newEntity`,
`loadMapEntities`, `reloadEntities`, and `loadEntitiesFromMapSetup`; `code/common/scripting/map/mapscriptengine_1.asm`
symbols `csc2B_initializeNewEntity`, `csc42_loadMapEntities`,
`csc44_reloadEntities`, and `csc49_loadEntitiesFromMapSetup`; `sf2enums.asm`;
`code/common/tech/jumpinterfaces/s07_jumpinterface.asm::j_InitializeMapEntities`; the H1 symbols above;
and local US-ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The extractor additionally records the pinned callee-owner paths and SHA-256 values in
`sourceIdentityJoins`. Reproduce static facts with `uv run sf2 h2 map-script-engine`; reproduce the
one-launch observation with `uv run sf2 h3 entity-population-reload --timeout-seconds 180`. The static
result is `tests/fixtures/h2/map-script-engine-static-v1.json`, ID
`sf2-map-script-engine-static-v1`, field `expected.entityPopulationCommandFacts`; the runtime result
is `tests/fixtures/h3/entity-population-reload-v1.json`, ID
`sf2-entity-population-reload-runtime-v1`, parsed by
`schemas/h3-entity-population-reload-observation.schema.json` and observed by
`tools/bizhawk/entity_population_reload_observer.lua`.

## Confirmed Map-Script `cloneEntity` Command Boundary

Evidence date: 2026-08-01.

**Confirmed:** `sf2-map-script-engine-static-v1` field `entityCloneCommandFacts` retains the single
source-named form `cloneEntity`: opcode `$25`, six encoded bytes, and two two-byte operands (four
encoded operand bytes). Its unmodified source comments are `copied entity` and `entity clone`; those
comments remain source text rather than lifecycle or presentation claims. The complete corpus contains
nine commands in two source rows: `bbcs_16` command indexes 7-14 have ordered literal pairs
129/130, 131/132, 131/133, 131/134, 131/135, 131/136, 131/137, and 131/138; `IntroCutscene2`
command index 4 has 132/131. The contract retains all 304 zero-inclusive program totals: only
`bbcs_16` has eight uses and `IntroCutscene2` has one. The compact ordered source-site and
program-total SHA-256 values are respectively
`867E601D639D063120D3A3A5C7B5CE52664A59A1A6D2CC397C8861A896F042A2` and
`36F45DF30945F8AA1883D1982702DE9A7290D4C0E797F52923C90471E85ECE70`.

**Confirmed:** the complete named section `csc25_cloneEntity` is H1/ROM `$46C5A`. It consumes two
advancing `move.w (a6)+,d0` reads, each transferring and advancing two bytes, then calls
`GetEntityAddressFromCharacter` after each read in that exact order. The first lookup result is read
as `move.b ENTITYDEF_OFFSET_ENTNUM(a5),d1`; after the second lookup, the exact post-lookup write is
`move.b d1,ENTITYDEF_OFFSET_ENTNUM(a5)`, then `rts`. The parsed source equate
`ENTITYDEF_OFFSET_ENTNUM` is 18, and each field instruction transfers one byte. The derived
one-byte transfer is tied to those two parsed use sites, while the four script-operand bytes remain
separate from it. The bounded section has no loop instruction, counter, or parsed whole-record span:
the one-byte `ENTITYDEF_OFFSET_ENTNUM` transfer is the only record-field transfer this static slice
confirms. An opcode, operand width, field symbol, lookup order, transfer order, or return mutation
fails parser construction before golden-fixture comparison.

**Confirmed:** instruction-scoped caller parsing retains two `bsr` sites whose instruction and
effective target are both `GetEntityAddressFromCharacter`. Its one-handler, zero-inclusive maps have
direct/effective total 2, internal direct/effective total 0, and external direct/effective total 2;
there is no jump-interface alias. The provenance-only join points to the existing entity-population
callee-owner record, without copying entity population, placement, action, or lifecycle facts.

**Confirmed (bounded H3):** `sf2-map-script-entity-clone-runtime-v1` runs all nine source-ordered
word pairs in one BizHawk 2.11.1 / Genesis Plus GX Map Test 0 launch. Every record reaches actual H1
handler entry `$46C5A` and its `rts` `$46C6E`; A6 begins at the session input offset 4 and ends at 8.
The observed input pairs are exactly 129/130, 131/132, 131/133, 131/134, 131/135, 131/136,
131/137, 131/138, and 132/131 in the H2 source-key order. Each record observes the same ordered
original lookup chronology: call `$46C5C`, entry `$4704A`, return-resumption `$46C60`; then call
`$46C66`, entry `$4704A`, return-resumption `$46C6A`. The lookup body is unpatched and executes in
each of the 18 calls.

**Confirmed (bounded H3):** the session harness controls only its A6 input words, the two
`ENTITY_INDEX_LIST` bytes selected by the original mask/test/subtract path, and the named byte plus its
immediate neighboring bytes in two distinct `ENTITY_DATA` records. At the actual `$46C60` byte-read PC,
the source record index and offset-18 byte are observed; at the actual `$46C6A` byte-write PC, the
destination record index and pre-write byte are observed. After the handler returns, the destination
offset-18 byte equals the observed source byte in every row, while the seeded offset-17 and offset-19
destination bytes retain their exact sentinel values. This is a one-byte handler-local RAM result, not
a statement about any other field or consumer.

**Unknown:** the only remaining grouped H3 queues are
`map-script-entity-clone/further-runtime-state-matrix`,
`map-script-entity-clone/further-runtime-external-consumer-matrix`, and
`map-script-entity-clone/further-runtime-context-matrix`. Whole-record behavior, entity identity,
lifetime, allocation, visibility, collision/pathfinding, persistence, timing, rendering, and
normal-story reachability remain unclaimed. None follows from the macro name, source comments, A6
reads, lookup calls, or the bounded offset-18 byte result.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm::cloneEntity`;
`code/common/scripting/map/mapscriptengine_2.asm` dispatcher slot `$25`;
`code/common/scripting/map/mapscriptengine_1.asm::csc25_cloneEntity` and
`GetEntityAddressFromCharacter`; `sf2enums.asm::ENTITYDEF_OFFSET_ENTNUM`; H1 handler PCs
`$46C5A/$46C5C/$46C60/$46C64/$46C66/$46C6A/$46C6E`; H1 lookup entry `$4704A`; and local US-ROM
SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`. Reproduce static facts
with `uv run sf2 h2 map-script-engine` (fixture
`tests/fixtures/h2/map-script-engine-static-v1.json`, ID `sf2-map-script-engine-static-v1`, field
`expected.entityCloneCommandFacts`) and the bounded runtime result with
`uv run sf2 h3 map-script-entity-clone --timeout-seconds 180` (fixture
`tests/fixtures/h3/map-script-entity-clone-v1.json`, ID
`sf2-map-script-entity-clone-runtime-v1`; verifier
`src/sf2tool/h3/map_script_entity_clone.py`; observer
`tools/bizhawk/map_script_entity_clone_observer.lua`).

## Confirmed Map-Script Map Load/Reload/Reset Command Family

Evidence date: 2026-07-29.

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

**Confirmed (H3):** `sf2-map-lifecycle-runtime-v1` replays five bounded handler cases in one BizHawk
2.11.1 / Genesis Plus GX launch: `reset-current-map`, `fade-then-map-load`,
`reload-current-map`, `map-load-same-current-map-index`, and
`map-load-changed-map-index`. The observer records return from the invoked handler and chronological
callbacks at the exact H1 `jsr` instruction sites, rather than at service entries. The observed direct
call-site target orders are respectively `ResetCurrentMap`; `LoadMapTilesets`, `WaitForVInt`,
`LoadMap`, `EnableDisplayAndInterrupts`, `WaitForVInt`; `LoadMap`,
`EnableDisplayAndInterrupts`, `WaitForVInt`; and that five-call `csc48` order for each of the two
`mapLoad` operands. The reset follow-up is guarded separately as the source `bra.w LoadMap` tail at
H1 `$3E3C`, with D0 `$0000` and D1 `$FFFF` before that transfer. A callback proves that direct JSR
site executed; it does not prove the called service's externally visible effect.

**Confirmed (H3):** every record has `handlerReturned` `true`. In case order, the exact runtime
field vector `(loadMapD0WordAtCall, loadMapD1WordAtCall, tilesetD1WordAtCall,
resetTailLoadMapD0WordAtTransfer, resetTailLoadMapD1WordAtTransfer, viewTargetEntityAfter)` is
`(null, null, null, 0, 65535, 90)`,
`(774, 4, 4, null, null, 255)`, `(2319, 65535, null, null, null, 255)`,
`(4629, 3, 3, null, null, 255)`, and `(4629, 4, 4, null, null, 255)`. These are
post-handler word/state observation fields, not assertions about the called services' effects.

**Confirmed (H3):** with initial `CURRENT_MAP` 3 and `VIEW_TARGET_ENTITY` seed 90, the five cases
respectively leave current-map values 3, 4, 3, 3, and 4. Their post-handler
`VIEW_PLANE_A_PIXEL_X/Y` words are `(0, 12288)`, `(384, 13056)`, `(1152, 14208)`,
`(2304, 14976)`, and `(2304, 14976)`. The observer writes two nonasset 16-bit sentinel words at the
source-derived `ResetCurrentMap` clear-span boundaries. The exact per-case tuples
`(start clear/replace; end clear/replace)` are reset `(true/true; true/true)`, fade
`(true/true; true/true)`, reload `(false/false; false/false)`, same-valued `mapLoad`
`(false/true; true/true)`, and changed-valued `mapLoad` `(true/true; true/true)`. The source guard
parses `MAP_LAYOUT_LONGS_COUNTER` 2047, `clr.l`, and `dbf` as an 8,192-byte physical clear span, but
the H3 sentinels check only its first and final words: they do not establish complete span contents,
decoded layout contents, or asset bytes. The two `mapLoad` cases are distinct tested map operands (3
and 4), not an equality branch: `csc48` has no current-map equality test. Its static
`FADING_SETTING` branch remains separately source-confirmed; for the bounded fade case the harness
releases it by clearing that setting at the first observed `WaitForVInt`, so this result does not prove
ordinary fade duration or presentation.

**Unknown:** the remaining grouped H3 queue is:

- `map-lifecycle/layout-collision-pathfinding-effects`: complete working-layout content and any
  collision/pathfinding consequence;
- `map-lifecycle/entity-reload-player-placement`: entity reload and player-placement consequences;
- `map-lifecycle/presentation-fade-hardware-timing`: VDP-visible presentation, fade, and hardware
  timing beyond the bounded release;
- `map-lifecycle/story-reachability-persistence`: normal-story reachability and persistence.

The H3 fixture does not promote macro names, state-symbol names, VInt records, direct call-site hits,
or two-word marker results into any of those unobserved outcomes.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` macros `resetMap`,
`loadMapFadeIn`, `reloadMap`, and `mapLoad`; `code/common/scripting/map/mapscriptengine_1.asm`
symbols `csc36_resetMap`, `csc37_loadMapAndFadeIn`, `csc46_reloadMap`, and `csc48_loadMap`;
`sf2enums.asm` constants `OUT_TO_BLACK`, `BYTE_SHIFT_COUNT`, `BYTE_MASK`, `VINTS_DEACTIVATE`, and
`VINTS_ACTIVATE`; `code/gameflow/exploration/exploration.asm::ResetCurrentMap` and its H1 `$3E3C`
tail; H1 direct-call instruction sites `$46590`, `$465C0`, `$465C4`, `$465EC`, `$465F2`, `$465FE`,
`$4672E`, `$46734`, and `$46740`; and local US-ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`. Reproduce with
`uv run sf2 h2 map-script-engine` for the static contract and
`uv run sf2 h3 map-lifecycle --timeout-seconds 120` for the one-launch runtime contract; observed
results are `tests/fixtures/h2/map-script-engine-static-v1.json`, ID
`sf2-map-script-engine-static-v1`, field `expected.mapLifecycleCommandFacts`, and
`tests/fixtures/h3/map-lifecycle-v1.json`, ID `sf2-map-lifecycle-runtime-v1`. The latter uses a
session-only copy and re-hashes the original on-disk ROM unchanged before launch.

## Confirmed Map-Script Source-Named Trigger Command Family

Evidence date: 2026-07-29.

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

**Confirmed:** `csc43_RoofEvent` is H1/ROM `$466B6` and `csc47_StepEvent` is `$46746`.
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

**Confirmed:** `sf2-map-interaction-trigger-runtime-v1` runs six bounded Map 02 cases in one BizHawk
launch: roof/step record-0 hit, terminator miss, and their source-named busy/battle gates. It invokes
the original handlers through `RunMapSetupInitFunction`'s configured seam, records the H1 direct-call
sites `$466C2`/`$46752`, and records the pre-callee D0/D1 word pairs. The record-0 cases reach the
source-selected records and the miss cases reach their exact table terminators; the busy and battle
cases bypass their respective scan/callee paths. These results are bounded handler observations, not
normal-story reachability evidence.

**Confirmed:** in this matrix, the roof hit changes only the destination marker and has toggle bit 0
set; the step hit changes the destination marker, matches the seeded source marker, and has toggle bit
1 set. The four non-hit rows leave the destination marker unchanged. All six observed rows retain
`CURRENT_MAP` 2 after the call. The two marker probes do not establish complete layout contents,
collision/pathfinding, visible roof/door behavior, callee service results, timing, audio, or a
hardware-visible effect.

### Runtime-question queue

**Unknown:** `map-interaction-trigger/full-layout-collision-pathfinding-effects` — establish complete
layout mutation and any collision/pathfinding consequence beyond the two marker probes.

**Unknown:** `map-interaction-trigger/presentation-audio-timing-hardware-effects` — establish
presentation, sound, timing, and hardware-visible consequences.

**Unknown:** `map-interaction-trigger/persistence-story-reachability` — establish normal-play caller
reachability and persistence/story consequences.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` macros `roofEvent` and
`stepEvent`; `code/common/scripting/map/mapscriptengine_1.asm` symbols `csc43_RoofEvent` and
`csc47_StepEvent`; `sf2enums.asm` equate `MAP_TILE_SIZE`; owner source
`code/gameflow/exploration/exploration.asm` symbols `PerformMapBlockCopyScript` and `OpenDoor`; Map 02
tables `Map02s5_RoofEvents` and `Map02s4_StepEvents`; H1 direct-call sites `$466C2` and `$46752`; and
local US-ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`. Reproduce the
static source contract with `uv run sf2 h2 map-script-engine` and the six-case runtime matrix with
`uv run sf2 h3 map-interaction-trigger --timeout-seconds 120`. The tracked observations are
`tests/fixtures/h2/map-script-engine-static-v1.json`, ID `sf2-map-script-engine-static-v1`, field
`expected.mapInteractionTriggerCommandFacts`, and
`tests/fixtures/h3/map-interaction-trigger-v1.json`, ID
`sf2-map-interaction-trigger-runtime-v1`.

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
The H1 return after that `jsr (Sleep).w` is `$474A4` (292020), not `$474A6`.
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

**Confirmed (retained H3 v1 projection):** the ten original handler-local cases retain the two
`jumpIfFlagSet` and two `jumpIfFlagClear` records, both flag-result polarities, and their target
replacement versus four-byte cursor-skip outcomes. `setF` and `clearF` write the observed GAME_FLAGS
bit, both `yesNo` return sides write/clear flag 89 then call `Sleep` with D0 word 10, and
`setStoryFlag` observes both base `400` and the word-wrap boundary to flag 0. Every record retains the
chronological parsed A6 use/call trace and checks the final bit from the parsed GAME_FLAGS base, span,
and mask plan. The v2 fixture protects these ten records with the retained-v1 projection digest
`8245C6580A2E7BB118EE713A723DE385D8AB96551D979795FE5131B5EFA8EC12` before configuration and again
at the golden boundary.

**Confirmed (H3):** one BizHawk 2.11.1 / Genesis Plus GX launch runs the retained ten handler-local
rows and six ordered persistence rows. The retained rows use their accepted six-byte input stream at
`$FF4004`–`$FF4009`, immediately after the `$FF4000`–`$FF4003` trampoline pointer, so their exact
v1 A6/cursor golden values remain unchanged. The persistence cases pair original `csc10` set/clear,
`csc11` flag-89 set/clear, and `csc13` base-400/word-wrap mutations with original `SaveGame`, inverse
work-RAM poison, original `LoadGame`, and their source-appropriate original `csc0C`/`csc0D` final
checks. Their source/H1/ROM joins are COMBATANT_DATA `$FFE800` (4,016 logical bytes), GAME_FLAGS
offset `$E86`, selected SRAM slot data/checksum bytes, and SAVE_FLAGS occupied bits. All sixteen
records match the fixture: each persistence row records its source-local mutation, saved byte, inverse
poison, restored byte, and final original-branch result. This establishes only the synthetic
in-process sequence, not normal save UI or durable-media behavior.

**Confirmed (failed attempt root cause; non-evidence):** the earlier v2 observer seeded all 4,016
COMBATANT_DATA bytes at `RunMapSetupInitFunction` entry `$474FC`. For the first seed 33,
CURRENT_MAP `$FFF711` lies at logical offset `$F11` and became `$DC`; the original map setup resolved
the void `$FFFF` row, skipped its indirect init JSR, and the host later timed out at `$0F00`, the
`bne.s @Wait` loop inside `WaitForVInt` (not `Sleep` entry `$0F04`). The former wrapper-entry
`milestone:story-state-probe` was therefore early evidence and the attempt is not credited.

**Confirmed (H3 corrected harness boundary):** wrapper entry snapshots and writes only controlled
instruction data, using the generated program in the independently guarded scratch gap
`[$FF4800,$FF4A00)` and the `$FF4000` pointer.
The retained-v1 stream is separately snapshotted, written, read back, and restored at
`$FF4004`–`$FF4009`; persistence mutation/final streams remain `$FF4840`/`$FF4850`. Wrapper entry
does not seed COMBATANT_DATA, the flag byte, SRAM/checksum, or SAVE_FLAGS until the generated probe
entry `$FF4800`. The original wrapper's `$47514` is the void/bypass epilogue entry, not a call return.
The six-byte patched outer JSR therefore has the exact tuple `$47512 → $FF88 → $47518`; its
trampoline's indirect inner JSR has the separate tuple `$FF96 → $FF4800 → $FF98`. The gap is bounded by the
`byte_FF4000` clear ending at `$FF4800`, next named constant `byte_FF4A00`, and a comment-stripped
pinned-source zero-reference audit; `$FF6000`–`$FF6800` is rejected as live map-layout storage. The
observer read back restoration of logical COMBATANT_DATA, declared SRAM slot/checksum/SAVE_FLAGS,
generated bytes, pointer, retained-v1 stream, prompt bytes, callbacks/output, and observed balanced
call/return stack. Its 19-line success status is the sixteen case milestones, one actual-probe
milestone, and the `callbacks-cleared:0` / `observer-finished` tail.

**Unknown:** `story-state/normal-story-reachability` — which original caller/save combinations reach
these source forms during normal progression.

**Unknown:** normal story reachability; church/witch/service-menu UI, original prompt presentation
and input timing, map reload, save-slot UI, copy/delete, cross-process or physical durability, power
loss/torn writes/corruption, VInt behavior, VDP/audio/rendering, service economics, and remake/design meaning.

**Unknown:** `story-state/player-visible-yes-no-presentation-timing` — the original prompt's visible
choice presentation and timing; the controlled prompt-return seam is not UI evidence.

Provenance: pinned `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `disasm/sf2cutscenemacros.asm` forms
`jumpIfFlagSet` through `setStoryFlag`; `code/common/scripting/map/mapscriptengine_2.asm`
(`csc0C`, `csc0D`, `csc10`, `csc11`, `csc13`); `sf2enums.asm` constants;
`code/common/tech/jumpinterfaces/s02_jumpinterface.asm` and `s03_jumpinterface_1.asm`;
`mapsetupsfunctions_1.asm` (`RunMapSetupInitFunction`, `GetCurrentMapSetup`),
`vintengine_1.asm` (`WaitForVInt`), `sf2const.asm`, and `LoadWeaponsprite`; the H1 listing addresses
above; and the US-ROM-backed extractor. Reproduce the static contract with
`uv run sf2 h2 map-script-engine`; observed result is
`tests/fixtures/h2/map-script-engine-static-v1.json`, ID `sf2-map-script-engine-static-v1`, field
`expected.storyStateCommandFacts`. `tests/fixtures/h3/story-state-v2.json`, ID
`sf2-story-state-runtime-v2`, is observed by `tools/bizhawk/story_state_observer.lua` through one
session-only BizHawk launch and checked by `src/sf2tool/h3/story_state.py`.

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
uv run sf2 h2 map-event-direct-state
uv run sf2 h2 map-event-direct-control
uv run sf2 h2 map-event-direct-handoff
uv run sf2 h2 map-event-predicate-results
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
