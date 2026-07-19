# Special Screens

- Status: **Confirmed** for all 19 layout-owned files, representative H1 addresses, seven screen
  groups, eighteen resource routes, title/logo input structure, witch save actions, suspend/reset
  flow, ending-effect ownership, and the complete nine-resource Stack-compressed tile corpus
- Status: **Inferred** for perceived animation pacing and simultaneous skip/cheat input behavior
- Status: **Unknown** for rendered frame parity, exact audio/VDP timing, and five oversized fixed
  transfer tails
- Evidence date: 2026-07-19
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
new, load, copy, and delete. Those routes call the SRAM functions inventoried in the technical
services batch and re-enter either `MainLoop` or `alt_MainLoopEntry` as appropriate. The US
`j_SoundTest` entry is only an `rts`, matching the source note that the function is absent from this
release.

The witch rendering helpers own screen construction, layout-zone DMA, head updates, blink VInt, and
speech-bubble/menu presentation. The suspend path sleeps 60 frames before presenting its resources.
After the witch dialogue it waits at most 600 frames for Start, fades out, and resets through the
original start vector; Start can end that wait early.

## Ending Screens

The ending-witch path owns the falling-jewels and witch-blink VInt functions and connects to the end
game sequence. The ending-kiss path owns a data-driven pixel-filling renderer. Ending-jewel,
ending-witch, and ending-kiss resource labels are all part of the canonical resource map. This
establishes ownership and routing, not visual parity.

## Concentrated Runtime Queue

No emulator was launched for this inventory. Presentation questions are retained as three shared
matrices:

1. Sega logo, title, cheat sequences, and Start timing;
2. witch save menu, blink/bubble presentation, and suspend/reset timing;
3. ending kiss pixel fill, falling jewels, and ending-witch presentation.

The same launches should sample the five fixed-transfer tails before DMA so their contents and
stability are answered together with rendered parity.

Each matrix should capture compact frame/state hashes for several phases in one launch instead of
creating a separate fixture per animation.

## Reproduction

```powershell
uv run sf2 h2 special-screens
uv run sf2 h2 special-screen-graphics
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/special-screens-static.json` and
`local/derived/special-screen-graphics-decode.json`.
