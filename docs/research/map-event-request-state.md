# Map-Event Request State

- Status: **Confirmed** for the bounded source/H1/ROM request-state write and caller-local handoff relation.
- Evidence date: 2026-08-26.
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Source: `ShiningForceCentral/SF2DISASM` `master` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`.

`sf2-map-event-request-state-static-v1` fresh-guards the accepted map-events, direct-state,
direct-control, direct-handoff, predicate-results, and dialogue-state owners before selecting the
39 request-state-writing programs from their 914-program / 3,579-operation mother corpus. The
selected zero-inclusive denominator is 30 entity and nine zone programs, with 875 non-selected
program contexts. Its complete selected CFG has 262 contextual/physical operations and 82
contextual/physical labels: 139 ordinary operations, 34 conditional branches, 21 unconditional
branches, 29 direct calls, three direct jumps, and 36 returns.

The six source-defined fixed-RAM write classes are `CURRENT_SHOP_INDEX` (32 writes),
`MAP_EVENT_TYPE` (8), `EGRESS_MAP` (2), `RAFT_MAP` (1), `RAFT_X` (1), and `RAFT_Y` (1). The 45
writes retain source mnemonic/width/operand order and immediate identity. Their 37 unique source
operands comprise 35 `sf2enums.asm` identities plus numeric values 43 and 48. The selected corpus
has exactly 24 event-table source files plus `sf2const.asm`, `sf2enums.asm`, and `sf2macros.asm`.

The static flow starts only at those six source write classes. It computes source-defined may/must
reaching definitions over each selected caller-local CFG and stops at 67 handoff sites: 31 alias-aware
`j_ShopMenu → ShopMenu` transfers (28 returning calls and three tail jumps) and 36 program returns.
There are 69 symbol/handoff relations with at least one source-defined reaching write. Call targets
remain unentered; the retained Shop alias is an instruction/effective-target identity, not a claim
about its callee body or result.

The public fixture contains structural IDs, addresses, opcodes, operands, source/H1/ROM hashes,
source-defined reaching-definition IDs, table identities, counts, and digests only. Provenance is
the pinned upstream source, its `build/sf2build-h1.lst` normalized UTF-8 identity,
the private ROM identity above, and:

```powershell
uv run sf2 h2 map-event-request-state
```

## Grouped H3 Runtime Questions

1. **Unknown:** `normalStoryProgramReachability`; `selectedControlFlowPath`; `callerEntryState`;
   `actualRequestWriteOrder`; `actualDefinitionAtHandoff`.
2. **Unknown:** `actualShopSelection`; `actualShopMenuEntryAndOutcome`; `actualEgressDestination`;
   `actualRaftDestinationAndCoordinates`; `actualMapEventReloadRequestConsumption`;
   `actualProgramReturnState`.
3. **Unknown:** `crossMapStateLifetime`; `saveLoadPersistence`;
   `inputUiMapTransitionAudioTimingAndStoryMeaning`.

No runtime reach, state values, state consumption or persistence, map transition, UI outcome,
audio/timing, story meaning, or callee algorithm is promoted by this H2 relation.
