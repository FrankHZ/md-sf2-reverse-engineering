# Original-reference replay capability

Status: **Confirmed** for the tracked transport contract and deterministic materializer. Runtime
execution is **Unknown**: this capability slice intentionally performed no EmuHawk launch. ROM:
USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Evidence date: 2026-08-30.
Toolchain: BizHawk 2.11.1 with Genesis Plus GX, `lua54.dll` SHA-256
`4786E0DF4CAF120E3BEDF0B6DDA260525DF2187C66DED220A21A53ACE76B0501`.
Source/API provenance: the pinned release archive in `manifests/toolchain.json`, BizHawk's
`ArgParser` command surface, and the official BK2/Lua API documentation reviewed on 2026-08-24.

## Confirmed transport contract

`sf2-original-reference-replay-capability-v1` is a non-semantic transport capability, not an
R4b scenario fixture, research-index record, address binding, or H4 definition. Its public recipe
contains one neutral **warm-up** physical BK2 row (`bk2Row: 0`) followed by 32 unchanged semantic
port-1 rows: neutral for semantic frames 0–7, Right for 8–11, neutral for 12–15, A for 16–19,
neutral for 20–23, Start for 24–27, and neutral for 28–31. The semantic row offset is exactly 1.
The materializer produces only `Header`, `Input Log`, and `SyncSettings` in that fixed ZIP order and
DOS timestamp; it rejects CoreState, SaveRAM/SRAM, extra members, header/readback drift, or a
changed recipe.

The reproduced public identities are recipe SHA-256
`BAD3DE219DBDA1F2FB9A2DA7191351290575CAFA4FF03A00B75A289ADA2BD867`, deterministic BK2
SHA-256 `250F4086E1C1AD08BF64A7CB5C84787E4EE8DA41D83D53630F54DE7FB8085E64`, and Input Log SHA-256
`003DDE951EDCB863AF1AE6EAA9AFA759E4722298D44AD190BFF9E85149C500A1`. The tracked observer is
hashed and materialized as canonical UTF-8/LF transport bytes: a CRLF checkout is reduced only by
CRLF-to-LF conversion, while a lone carriage return or every other byte drift fails the declared hash.
Those exact canonical bytes are syntax-compiled against the pinned Lua runtime and statically limited
to movie queries, input-poll/lifecycle registration, frame advancement, controller reads, client exit,
and bounded status output. Alias/dynamic calls, input/memory/register writes, movie mutation,
save-state/SRAM control, ROM control, process/shell APIs, bootstrap loading, and gameplay mechanics
are rejected before launch.

The native command is exactly `EmuHawk.exe --chromeless --config=<fresh-config>
--movie=<generated.bk2> --lua=<passive-observer.lua> <private-rom>`, supplied through a subprocess
argument list without a shell. BizHawk 2.11.1 source order is recorded as MainForm command-movie
loading before Shown command-Lua scheduling; Lua first resumes in `Tools.UpdateAfter`, after the
warm-up row. The loop is `FrameAdvance → MovieSession.HandleFrameAfter → Tools.UpdateAfter`.
The observer begins at emulator frame 1; input-poll callbacks precede the frame increment, so semantic
row `i` against physical BK2 row `i + 1` records emulator frame `i + 1` (frames 1–32). It records
non-maskable `{semanticIndex,bk2Row,emuFrame,input}` observations and writes terminal status in the
same UpdateAfter that handles row 32/`FINISHED` at frame 33. Callback, frame/movie
diagnostic, unregister, and status-write failures use one-shot protected finalization and a non-zero
exit. Every unavailable JSON number, boolean, and string is emitted as `null`, never Lua `nil` or
the string `"nil"`; a status-less process leaves callback, input-count, and Lua-start facts
**Unknown** (`null`), never fabricated as false, zero, or an empty trace.

## Containment and receipt

Each non-preflight launch is reserved under the previously absent ignored directory
`local/derived/h3/original-reference-replay-capability/<candidate-SHA256>/launch-<ordinal>/`. The
candidate SHA-256 closes the ROM, pristine BizHawk archive identity and 477-member-set digest,
runner/helper, capability fixture, both schemas, observer, recipe/BK2, and deterministic contained
configuration template. Each launch revalidates the hash and member set immediately before
extracting the archive only to
`<launch>/toolchain`; it never executes the mutable host extraction. The closed configuration directs
every Global and Genesis ROM/movie/SRAM/state/log/tool/Lua/watch/temp surface inside that directory,
disables recent/start/persistence/update/RA/sound settings, and sets MovieEndAction to Finish enum 3.
Archive traversal, drive/ADS paths, duplicate or case-colliding members, Unix symlink or Windows
reparse-attribute entries, and mutable config/SRAM/state members fail closed. The runner records the
pristine archive identity before launch and a typed post-termination archive snapshot; any mismatch
is `archive-drift`. It also hashes the
declared host and contained mutable surfaces plus the private ROM before and after launch. Separately,
it inventories every regular file and empty directory in the host toolchain without following
reparse points; canonical relative entries carry kind, file identity, and Unix-epoch UTC
100-nanosecond last-write ticks. Any
full-tree entry difference is the first typed `host-toolchain-drift` identity mismatch, while the
16-surface classification remains separately receipted. Absolute host paths and full-tree entries
are excluded from the replay digest. A failed post-exit snapshot is retained as a closed unavailable
snapshot and first typed isolation failure; host and contained drift remain typed, and host data is
never deleted or restored. The runner
preserves only typed private `receipt.json`; cleanup removes only contained launch artifacts after
their diagnostics are retained in that receipt.

Ordinals 1 and 2 are diagnostics; ordinal 3 is frozen acceptance. Ordinal 1 is already globally
consumed by candidate SHA-256 `9F8417BC1A515FEB5D9466DCC1BC489B981D97741E44518D572E6B0E63380BDF`
with receipt SHA-256 `BDE38876750E51E59CF1D2897495EFFD8EE42955F7FE87C3F12A9DB853C14CA6`.
Before any corrected launch, the runner validates that immutable ledger row's candidate, path, and
receipt bytes. The corrected candidate may use only diagnostic ordinal 2; ordinal 3 requires its
single same-candidate ordinal-2 `PASS`, whose ledger receipt hash matches actual receipt bytes, and
a matching replay digest. There is no nominal reset or fourth launch. `uv run sf2 h3 original-reference-replay-capability
--preflight-only` performs only static/materialization checks and starts no emulator; the
verification planner emits that preflight form for this partition.

## H3 question queue

- **Unknown — runtime compatibility:** a later authorized diagnostic must establish whether the
  pinned 2.11.1 movie/header/sync representation is accepted with the observer's first
  UpdateAfter resume following the neutral warm-up row.
- **Unknown — callback cadence and console lifecycle:** a later authorized diagnostic must confirm the
  exact input-poll trace, callback removal, exception-to-status/non-zero-exit propagation, and Lua
  Console clean state. This capability makes no route, map, zone, battle, victory, endpoint, capture,
  R4b, or H4 claim.

## Reproduction

Run `uv run sf2 h3 original-reference-replay-capability --preflight-only`. The focused public test
is `uv run pytest tests/python/test_original_reference_replay.py`; it patches process creation for
all launch-path tests. No ROM, BK2, state, SRAM, capture, or receipt is tracked.
