# Map-Event Cross-Program Flag State

- Status: **Confirmed** for static writer-to-reader candidate relationships only.
- Evidence date: 2026-08-28.
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Source: `ShiningForceCentral/SF2DISASM` `master` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`.

`sf2-map-event-cross-program-flag-state-static-v1` reads the complete accepted 914-context,
3,579-operation map-event mother corpus and its 493 contextual direct-flag access sites. It guards
the assigned 90 map-event ASM identities plus `sf2macros.asm`, their H1 rows, and the matching ROM
bytes. The public fixture contains structural source identities rather than source bodies, ROM bytes,
decoded text, private paths, or runtime captures.

The direct access surface contains 316 contextual / 314 physical `chkFlg` reads, 169 / 168 `setFlg`
writes, and 8 / 8 `clrFlg` writes. Each read retains its immediate conditional consumer, yielding
316 contextual / 314 physical consumer PCs. The combined access-and-consumer anchor surface is 809
contextual / 804 physical PCs and 2,610 contextual / 2,592 physical encoded bytes; the 18-byte
contextual overlap remains explicit.

The complete direct domain has 195 positive programs: 190 readers and 135 writers. Its 151 numeric
flags divide into 128 read flags and 114 written flags, with 91 overlaps, 37 read-only values, and
23 write-only values. The retained same-program lifecycle owner remains the sole owner of its 131
relations over 82 flags. This rail verifies that retained join against the direct source surface, then
excludes every same-program pair before creating cross-program candidates.

There are 720 unique cross-program candidates across 49 flags. A candidate is exactly one numeric
flag shared by one writer program and a distinct reader program; it is deduplicated by
`(flag, writer program, reader program)`, not by individual repeated accesses. The category-pair
totals are 374 entity→entity, 2 entity→item, 123 entity→zone, 2 item→entity, 1 item→zone,
182 zone→entity, and 36 zone→zone. No item→item or zone→item candidate exists. Of the 91 direct
read/write-overlap flags, 42 are same-program-only, 40 have both same- and cross-program relations,
and 9 are cross-program-only.

The fixture retains `RunMapSetupEntityEvent`, `RunMapSetupZoneEvent`, `RunMapSetupItemEvent`, and
`Trap4_CheckFlag` only as hash-locked existing-owner joins. It does not re-own their bodies. The
relation proves static source membership and program identity, not an execution order or a value
propagating between programs.

Reproduce with:

```powershell
uv run sf2 h2 map-event-cross-program-flag-state
```

## Grouped H3 Runtime Questions

**Unknown:** `naturalProgramReachability`; `callerEntryFlagState`; `actualFlagValueAtRead`;
`actualConditionalBranchSelection`; `actualMutationReachability`; `runtimeFlagValueAfterMutation`;
`producerConsumerTemporalOrder`; `interveningFlagMutations`; `mapSetupAndRecordSelection`;
`saveLoadAndCrossMapPersistence`; `calleeScriptAndServiceEffects`;
`dialogueAudioPresentationAndStoryMeaning`.

No H3 observation is authorized by this static relationship inventory. Natural reachability,
caller-selected entry state, branch selection, mutation execution, cross-program chronology,
intervening writes, map/setup selection, persistence, callee effects, and player-facing meaning
remain deferred.
