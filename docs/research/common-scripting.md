# Common Scripting Engines

- Status: **Confirmed** for the pinned 29-file inventory, 90-slot map-script and 80-slot entity-script
  dispatch tables, interpreter admission/termination rules, text-bank selection, and Huffman state
- Status: **Inferred** for named helper intent where only call structure is modeled
- Status: **Unknown** for caller-dependent story meaning, entity movement timing, text rendering timing,
  and individual script content
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Confirmed Inventory

The recursive `code/common/scripting` boundary contains 29 files, 11,153 source lines, 888 global
labels, and 576 direct call sites across entity, map, text, and end-credit helpers. Twenty-eight files
have a representative global symbol bound to the H1 listing. The remaining
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

## Runtime Queue

Entity movement timing, dialogue typewriter/render timing, end-credit presentation, and contextual
meaning of script commands remain grouped runtime questions. They will share scenario setup and
observation buffers rather than becoming one emulator launch per opcode. This batch adds no emulator
run.

## Reproduction

```powershell
uv run sf2 h2 common-scripting
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/common-scripting-static.json`.
