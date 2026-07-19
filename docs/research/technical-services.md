# Remaining Technical Services

- Status: **Confirmed** for the complete 12-file source boundary, eleven main-ROM representative
  addresses, twenty technical-resource incbin mappings, byte-copy direction, input scan/wait shape,
  SRAM slot/checksum structure, the 68000 music-wait command, and the Z80 driver build chain
- Status: **Inferred** for caller-visible thinking-RNG distribution and perceived delay
- Status: **Unknown** for controller hardware edge cases, SRAM persistence/corruption behavior, and
  rendered/audio timing
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Complete Boundary

This batch closes the twelve `code/common/tech` files not owned by the interrupt, graphics,
jump-interface, or pointer inventories. They contain 6,986 source lines and 550 global labels:

- five layout-owned resource-incbin files;
- `bytecopy.asm`, `input.asm`, `randomnumbergenerator.asm`, and `thinkingairng.asm`;
- the 68000 `sound/music.asm` bridge and standalone Z80 `sound/sounddriver.asm`;
- `sram/sramfunctions.asm`.

Eleven files are included directly by the main 68000 layout. The exception is `sounddriver.asm`: the
pinned build script assembles it separately with ASW into an 8,192-byte `$0000..$1FFF` binary, then
the main layout includes that binary at `SoundDriver` (`0x1EC000`). Its SHA-256 is recorded in the
canonical inventory. The source is an IDA-era single-byte export with C1 bytes in comments, so the
parser uses strict UTF-8 first and a byte-preserving Latin-1 fallback; source hashes always use the
unaltered bytes.

The research index binds the eleven main-layout files. The standalone Z80 source is intentionally
kept as an H2 auxiliary-build fact rather than pretending its `init` label exists in the 68000 H1
listing.

## Static Contracts

The five incbin files contain exactly twenty named entries across sections 3, 6, and 17. Their
canonical mapping covers UI/font/title resources, Huffman text trees, witch-screen resources, and
base tiles. This confirms ROM routing only; extracted copyrighted resource bytes remain local and
their rendered meaning stays with the corresponding presentation subsystem.

`CopyBytes` compares destination and source. It copies backward when the destination address is
higher and forward otherwise, preserving overlapping moves in the normal memmove cases.

`UpdatePlayerInputs` scans both controller data ports and stores two state bytes per controller. The
two bounded input waits use 60 and 180 VInt iterations. Static source proves the port and loop shape;
controller-model quirks and exact repeat perception require runtime observation.

The SRAM service owns two save slots. Logical bytes are stored at every other physical address, the
copy helpers accumulate an eight-bit additive checksum, and `CheckSram` clears the occupied bit for
a slot whose checksum fails. Initialization, save/load/copy, and explicit flag clearing are all in
the same source file. Power-loss ordering and emulator persistence are not inferred from assembly
alone.

`PlayMusicAfterCurrentOne` sends the wait-for-current-music command and parameter command, then polls
the mailbox with three-frame sleeps. The Z80 source statically exposes YM2612, PSG, DAC, bank, music,
and SFX machinery, but audible timing and channel output remain outside this static contract.

The thinking-AI RNG advances the low-byte state with multiplier 541 and increment 12345. The base
68000 RNG file was already covered by dedicated H3 fixtures. For the thinking variant, caller-visible
distribution and variable delay remain inferred until tested as a matrix.

## Concentrated Runtime Queue

No emulator was launched for this batch. Four questions are retained for later grouped runs:

1. controller hardware and repeat timing;
2. SRAM initialization, valid/invalid checksum, slot flags, and persistence in one corruption matrix;
3. 68000-to-Z80 mailbox, channel routing, and audio timing;
4. thinking-RNG caller distribution and delay boundaries.

The SRAM cases should share one initialized save-state matrix, and audio cases should share one sound
driver instrumentation launch. Isolated one-case fixtures are not warranted by the current evidence.

## Reproduction

```powershell
uv run sf2 h2 tech-services
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/tech-services-static.json`.
