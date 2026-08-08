# Dialogue-Command Static Contract

- **Confirmed original behavior:** the six source command layouts, handler dispatch, cursor/name-index
  state writes, source-labelled modifier bits, close/clear call order, and the bounded
  entity-to-map-sprite dialogue-property seam below.
- **Inferred original behavior:** none promoted here.
- **Unknown original behavior:** normal-story reachability, rendered text/portrait/speech/controller
  timing, and unshimmed service completion/repeat/persistence.
- Evidence date: 2026-07-31
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- Traceability: `sf2-map-script-engine-static-v1` in
  `tests/fixtures/h2/map-script-engine-static-v1.json`; and
  `sf2-map-script-dialogue-runtime-v1` in
  `tests/fixtures/h3/map-script-dialogue-v1.json`; `src/sf2tool/h2/map_script_engine.py`;
  `src/sf2tool/h3/map_script_dialogue.py`; and `docs/research/common-scripting.md`.

## Confirmed Static Contract

The map-script dialogue command family has six primary macro forms: `nextSingleText`,
`nextSingleTextVar`, `nextText`, `nextTextVar`, `textCursor`, and `hideText`. Their source-defined
opcodes are `$00` through `$04` and `$09`; the physical command widths are respectively 4, 6, 4, 8,
4, and 2 bytes. The fixture retains both the emitted operand fields and those physical widths, rather
than treating handler reads as a substitute for stored bytes.

All 2,883 invocations are retained as ordered references to their existing map-script program commands,
and zero-inclusive totals cover all 304 programs. The source corpus contains 2,058 `nextSingleText`,
zero `nextSingleTextVar`, 577 `nextText`, zero `nextTextVar`, 234 `textCursor`, and 14 `hideText`
commands. `textCursor` source operands range from 240 through 4,233; the independently source/ROM
checked text-line domain is contiguous from 0 through 4,266. This is an ID-domain validation, not a
claim about decoded dialogue content or display order.

`csc00_displaySingleTextbox` and `csc02_displayTextbox` test the cutscene-text skip flag before their
display path. The four display handlers compare the packed modifier/entity word with `-1`; all call the
portrait helper before the direct entity dialogue-property consumer, call `DisplayText`, then increment
`CUTSCENE_DIALOG_INDEX`. The two `nextSingle*` handlers subsequently call the portrait-close path,
clear text, and call `Sleep` with source value 10, while the two continuing handlers do not contain that
close/sleep sequence. Both `*Var` handlers contain two word reads into the source-named dialogue name
index states. The zero-use `nextSingleTextVar` macro's four operand bytes and its handler's two word
reads remain distinct static facts; this contract does not invent a runtime interpretation for that
unused form.

The static caller audit keeps all six dialogue handlers and `csc1D_showPortrait`, including the two
zero/zero caller rows. It preserves direct-instruction and resolved-effective target identities even
where they are equal, and classifies the helper as internal versus the entity consumer as external from
their parsed source paths rather than assigning a behavioral role.

`textCursor` writes its one word to `CUTSCENE_DIALOG_INDEX`. `hideText` calls the portrait-close target
before its text-clear macro. `csc1D_showPortrait` reads the same packed word and tests word bits 15 then
14. Those handler use-sites derive high-byte `handlerTestedModifierByteMask` `$C0`; observed modifier
bytes outside the packed-word `$FFFF` sentinel are checked against that use-site-derived mask. The macro
comments label modifier byte `$80` as `display on right` and `$40` as `mirrored`; those original labels
are retained as labels, not recast as a renderer contract. The `$FF` `undisplayed` label remains separate
from the handlers' confirmed full-word `-1` comparison.

The direct entity seam is `GetEntityPortaitAndSpeechSfx` at ROM address 284,216. Its named section masks
`d0` with parsed `COMBATANT_MASK_ALL` (255), then obtains the entity address and loads its map-sprite
byte. The map-script contract joins the existing 119-row, 478-byte sprite-dialogue table only through
the sibling contract's ID, pinned commit, ROM hash, source path, and addresses; it does not copy decoded
text or use the sibling golden fixture as evidence.

## Confirmed Runtime Boundary

The one-launch `sf2-map-script-dialogue-runtime-v1` fixture retains 21 handler-local cases: all six
entry/return PC pairs, A6/stack boundaries, skip admission, source-partitioned packed inputs, the two
controlled zero-source `*Var` layouts, cursor bounds, close path, ordered direct call identities and
zero-inclusive target counts, direct state writes, and session-controlled call register words. The H3
observer captures entry/call/target/return PCs first and resolves call labels only through the unique
guarded source/H1 address map. A remake adapter MUST preserve those facts as command/service seams. The
explicit D0/D1/D2 trampoline inputs and the RTS service shims are harness controls, not original
caller/service behavior.

The remaining original questions are exactly the three `map-script-dialogue/*` queues in
`docs/research/common-scripting.md`; a remake MUST NOT infer visual portrait placement, audio playback,
text completion, controller timing, story reachability, or persistence from this handler-local matrix.

## Remake Boundary

A remake may model command decoding, dialogue-line selection, name-index substitution, portrait lookup,
and presentation scheduling as separate services. Fidelity work can preserve the confirmed command and
state-update order. Rendering, audio scheduling, input waits, accessibility behavior, and any semantic
meaning assigned to the original labels remain explicit modern choices until the grouped runtime matrix
supplies observations.
