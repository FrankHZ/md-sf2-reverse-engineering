# Original-reference replay scenario API

Status: **Confirmed** for the public, data-only descriptor, recursively closed schemas, deterministic
preflight receipt, and passive-observer structural policy. Runtime behavior is **Unknown**: this slice
does not start BizHawk, read a private ROM/movie/capture/receipt, or create a ledger. Evidence date:
2026-08-30.

## Confirmed public protocol

`sf2-original-reference-replay-scenario-api-v1` is a reusable protocol descriptor, not a scenario
observation or a claim of natural continuity. The core schemas accept bounded future identifiers,
addresses, fixture hashes, checkpoint roles, and timeout limits without embedding this sample's
values. They recursively close every object and reject path/raw-payload fields and path-like values.
Production cross-field checks fail closed on duplicate fixture IDs, artifact IDs/roles, or checkpoint
roles; missing fixture references; non-contiguous repeated PCs; role-order drift; and declared limits.
Filesystem catalogs and the observer path are injected only at the outer Python composition boundary.

The tracked sample intentionally names only `generic-protocol-sample` /
`generic-protocol-preflight`, marks itself **Unknown**, and carries no local path or payload. Its two
source-backed H2 anchors are a project-authored generic example, not bindings built into the reusable
schema.

The descriptor's two H2 anchors are source-backed static fixtures:

- `sf2-map3-battle01-turn-finalization-static-v1`, SHA-256
  `4688D88BB52580BCC326DC264F8A86DF4350C0A17912DB4942FFB920001319FD`, declares the turn-finalization
  resume address `0x24106`.
- `sf2-map3-battle01-victory-return-static-v1`, SHA-256
  `3378FEC1EFDFB4FCD0B35AC10E9F5C494F63216B32613D1E41412D61CFDAA1CE`, declares the victory entry
  address `0x23CBA`.

The final two sample roles share `0x23CBA`; their declared dispatch order is `victory-entry` then
`declared-terminal`. A terminal PASS contract requires `callbacksRemaining: 0`, protected finalization,
and `consoleCheckRequired: true`. Console cleanliness is outer-runner evidence only: neither the
observer nor this preflight claims that it has been observed.

The four sample artifact identities are public synthetic hashes, not movie, Input Log, header, or sync
payloads. Each is SHA-256 over UTF-8
`sf2tool/original-reference-replay-scenario-api/public-synthetic-artifact/v1:` followed by its
`artifactId`. The descriptor carries only those hashes; the deterministic derivation is tested without
materializing any artifact.

The generic observer is canonical UTF-8/LF source, SHA-256
`BA35D6F0DEC2DB79856CA1A71998E831BF13DD15120192791A9AEAB9504EDF85`. Its exact API names, bare
calls, and forbidden-capability list are immutable production constants, mirrored as schema constants;
a descriptor cannot broaden them. Static tests reject direct, aliased, and dynamic member access to
input/controller, memory/register, movie, state/SRAM, ROM, shell, and dynamic loading capabilities.
Each callback is protected; a callback or cleanup failure writes typed `caseId` (or null before
configuration), `phase`, `code`, current role/null, observed roles, expected/actual, callback count,
cleanup result, detail, and a non-zero requested exit code. Every observer text value replaces path
separators plus all ASCII controls including DEL, remains valid UTF-8, and is capped at 500 bytes (within the receipt
schema's 500-character maximum) without splitting a UTF-8 code point before JSON escaping. Runtime
failure receipts retain the validated
descriptor, scenario, transport, checkpoint, lineage, and Unknown context; only a preflight failure has
unavailable descriptor context. PASS requires zero callbacks and protected cleanup. The observer removes
every registered callback before its terminal status and emits `consoleCheckRequired: true`, not a
console-clean assertion. This is a contract implementation only; callback cadence and Lua Console
behavior remain **Unknown** until runtime observation.

## Preflight boundary

Run `uv run sf2 h3 original-reference-replay-scenario-api --preflight-only`. The maintained bootstrap
inventory declares exactly zero expected launches for this command, and its receipt reports
`ProcessStarts: 0`. It writes no scenario ledger, private receipt, movie, emulator configuration, or
derived output. Synthetic invalid-descriptor and passive-policy failures also return typed preflight
receipts with `ProcessStarts: 0`.

Lineage is identity-only: a descriptor/receipt uses `ledgerId`, `availability`, `runClass`, a bounded
`launchOrdinal`, and a `priorReceiptSha256` when one exists. Runtime is stop-loss bounded to ordinals
1–3: ordinal 1 carries no predecessor hash, while ordinals 2–3 require one; runtime declares either
`diagnostic` or `frozen-acceptance`. It never carries a filesystem path or the current receipt's
self-hash. The tracked sample and every preflight receipt remain `not-accessed-preflight` with null
run class, ordinal, and prior receipt identity; this slice creates no ledger.

The compatibility wrapper remains separately owned by
`original-reference-replay-capability`; the shared transport extraction does not alter that command's
fixture/candidate/preflight/ledger behavior. The scenario API neither reads nor modifies its private
ledger.

## H3 question queue

- **Unknown — callback and console lifecycle:** a separately admitted private runtime case must prove
  the declared checkpoint reachability, ordered shared-PC dispatch, exception-to-status/non-zero exit,
  callback removal, and clean Lua Console condition.
- **Unknown — scenario evidence:** a separately admitted case must establish any actual continuity,
  victory, endpoint, R2b observation, capture, or H4 result. This generic sample establishes none.

## Reproduction

Run the command above and `uv run pytest tests/python/test_original_reference_scenario.py -q`.
Both use tracked public inputs only. No H3 fixture registration, evidence counter, research-index entry,
or private artifact is added by this slice.
