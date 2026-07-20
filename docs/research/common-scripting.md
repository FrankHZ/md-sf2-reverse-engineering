# Common Scripting Engines

- Status: **Confirmed** for the pinned 29-file inventory, 90-slot map-script and 80-slot entity-script
  dispatch tables, interpreter admission/termination rules, text-bank selection, complete context-
  Huffman tree corpus, all 17 compressed text banks/4,267 decoded records, and
  the regular entity map-sprite decode/DMA consumer shape, the complete 119-row sprite-dialogue
  property table and its lookup/default rules, plus the complete variable-width font, ASCII
  conversion, pointer, and glyph-loader data path, and the 1,730-byte global entity-action corpus
- Status: **Inferred** for named helper intent where only call structure is modeled
- Status: **Unknown** for caller-dependent story meaning, entity movement timing, text rendering timing,
  and individual script content
- Evidence date: 2026-07-19
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

The shared cutscene entity-action source is also parsed as a complete command corpus. It contains
444 commands across 39 entry labels, uses 28 of 44 defined `ac_*` macro forms, and occupies exactly
1,730 bytes once macro widths and seven explicit branch displacement words are summed. All seven
relative branches stay within the file; 28 of 29 absolute jumps exit to `eas_Idle`, while the last
jumps internally to `eas_Init`. This confirms command inventory and static control flow, but not
frame timing, collision outcomes, or which story routes invoke each entry.

`map/mapsetupsfunctions_1.asm` and `map/mapfunctions.asm` now have deeper cross-subsystem contracts:
setup selection, six-pointer layout, entity/zone/item/description dispatcher shapes, all selected
entity record streams, the complete entity/zone/item event-table boundary, and all area-description
wrappers/tables, setup initialization callables, and standalone setup scripts are source/H1-verified by
[`map-data-inventory.md`](./map-data-inventory.md). This inventory still owns the file boundary and
general scripting engine; the map-data document owns setup-table semantics.

## Runtime Queue

Entity movement timing, dialogue typewriter/render timing, control-code side effects and inserted
dynamic values, nonstandard direct symbol injection, end-credit presentation, and contextual meaning of script
commands remain grouped runtime questions. They will share scenario setup and
observation buffers rather than becoming one emulator launch per opcode. Reserved map-sprite IDs
237-250 are now excluded from symbolic references, encoded entity records, built script payloads,
combatant-derived tables, direct writers, and every property-update caller. A future shared
entity-sprite matrix is only needed if the project chooses to document deliberately malformed/raw
injection behavior. This batch adds no emulator run.

## Reproduction

```powershell
uv run sf2 h2 common-scripting
uv run sf2 h2 entity-action-scripts
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

Generated JSON stays under ignored `local/derived/common-scripting-static.json` and
`local/derived/variable-width-font-static.json` and `local/derived/text-huffman-static.json`.
The full decoded symbol corpus stays in ignored `local/derived/text-banks-static.json`.
The full dialogue-property catalog stays in ignored `local/derived/sprite-dialogue-static.json`.
The full assignment/caller catalog stays in ignored `local/derived/map-sprite-assignments-static.json`.
