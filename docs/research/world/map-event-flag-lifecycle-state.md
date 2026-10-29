# Map-Event Direct-Flag Lifecycle State

- Status: **Confirmed** for same-program, same-numeric-flag source/H1/ROM-local relations only.
- Evidence date: 2026-08-28.
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Source: `ShiningForceCentral/SF2DISASM` `master` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`.

`sf2-map-event-flag-lifecycle-state-static-v1` fresh-builds the accepted 914-context,
3,579-operation map-events direct-flag mother corpus and retains the accepted direct-state,
direct-control, direct-handoff, predicate-results, common-stats, and technical-interrupt identities.
It selects a program/flag relation only where that one parsed program contains a direct `chkFlg` read
and a direct `setFlg` or `clrFlg` mutation of the same numeric flag. The selection is 117 positive and
797 zero program contexts, with 131 relations over 82 numeric flags.

The relation-local source-order access corpus is 272 accesses: 135 reads, 131 sets, and six clears.
Its exact ordered sequence counts are 121 `[read,set]`, three `[read,set,read,set]`, two
`[read,clear]`, and one each `[read,read,set]`, `[set,read]`, `[read,clear,clear]`,
`[read,set,clear]`, and `[read,clear,set]`. Relations divide into 65 entity, 62 zone, and four item
contexts. All flag accesses in the 117 selected programs total 348; this is intentionally broader than
the relation-local access set.

The selected complete local bodies cover 67 source files, 1,177 contextual operations, 1,137 distinct
physical operation PCs, 339 contextual labels, and 4,216 contextual encoded bytes. Their 79 merged
physical intervals cover 4,066 bytes, leaving 150 contextual overlap bytes explicit rather than
collapsing aliases. Control kinds are 740 ordinary operations, 207 conditional branches, 119 returns,
69 unconditional branches, 41 direct calls, and one direct jump. The source macro definitions retained
are exactly `chkFlg`, `setFlg`, and `clrFlg` in `sf2macros.asm`; their source definitions/emissions and
all selected source, H1, and ROM ranges are guarded before fixture comparison.

The public fixture records only structural source identities, paths/lines, symbol/address identities,
numeric operands, source order, immediate `chkFlg` branch polarity/target/fallthrough, complete local
operation and label shape, and explicit contextual-versus-physical interval accounting. It does not
assert that a conditional path executes or that a lexically later mutation is reached.

Reproduce with:

```powershell
uv run sf2 h2 map-event-flag-lifecycle-state
```

## Grouped H3 Runtime Questions

**Unknown:** `naturalProgramReachability`; `callerEntryFlagState`; `actualFlagValueAtRead`;
`actualConditionalBranchSelection`; `actualMutationReachability`; `runtimeFlagValueAfterMutation`;
`crossProgramLifecycleOrdering`; `mapSetupAndRecordSelection`; `calleeAndScriptEffects`;
`saveLoadAndCrossMapPersistence`; `dialogueAudioPresentationAndTiming`; `storyMeaning`.

No H3 observation is authorized by this source-local relation. Runtime reachability, input flag value,
branch selection, post-mutation value, callee and script effects, cross-program ordering, map selection,
persistence, presentation/timing, and story meaning remain deferred.
