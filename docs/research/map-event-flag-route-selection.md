# Map-Event Flag Route Selection

- Status: **Confirmed** for static routing topology only.
- Evidence date: 2026-08-28.
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Source: `ShiningForceCentral/SF2DISASM` `master` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`.

`sf2-map-event-flag-route-selection-static-v1` retains the accepted
`sf2-map-event-cross-program-flag-state-static-v1` writer-to-distinct-reader candidate corpus, then
joins each of its 195 participating programs to source-shaped map-event records, category pointers,
and `msMap`/`msFlag` selector rows. The retained cross-program, map-events routing-setup, and map-setup
owners remain hash-locked; this rail does not recreate their corpora or select a route at runtime.

The fresh source surface has 192 unique identities: the accepted 91 cross-program identities; 96
matching event-record owner files, of which 88 overlap the accepted identity set and eight are new;
91 related pointer-table files; `data/maps/mapsetups.asm`; and `sf2mapsetupmacros.asm`. It verifies
the 284 matching records (1,150 bytes) in 96 event tables, 139 category-pointer entries (556 bytes)
in 91 selected pointer tables, and 94 selected map/flag selector rows (564 bytes) across 51 maps.
Together with the retained cross-program 804 PCs/2,592 bytes, those three disjoint cohorts form 1,321
physical source/H1/ROM anchors covering 4,862 bytes.

Candidate classification uses mutually exclusive source-topology precedence:

1. `sameEventTable` when the writer and reader route contexts share an event-table address;
2. `sameSelectedSetupDifferentEventTable` when they do not share an event table but share a selected
   map-setup pointer table;
3. `sameMapDifferentSelector` when neither prior relation holds but their route-map sets overlap; and
4. `crossMapOnly` otherwise.

All 720 accepted candidates classify exactly once: 20 same-event-table, 54 same-selected-setup/different-
event-table, 11 same-map/different-selector, and 635 cross-map-only. The accepted category-pair totals
remain unchanged. Every participating program has at least one event-record, pointer-table, and route-map
context; no route context is missing or ambiguous. The separate numeric writer-to-selector join has 15
relations across 11 flags and 11 writer-program/flag contexts (one source program contributes two flags).

The confirmed relations are structural only: program-to-record membership, record-to-category-pointer
membership, pointer-to-map/flag selector membership, and the ordered classifier above. A source `msFlag`
row is not evidence that the selector is evaluated, that a particular flag has a value, or that either
program executes.

Reproduce with:

```powershell
uv run sf2 h2 map-event-flag-route-selection
```

## Grouped H3 Runtime Questions

**Unknown:** `naturalProgramReachability`; `callerEntryFlagState`; `actualFlagValueAtRead`;
`actualConditionalBranchSelection`; `actualMutationReachability`; `runtimeFlagValueAfterMutation`;
`producerConsumerTemporalOrder`; `interveningFlagMutations`; `actualMapSetupSelectorEvaluation`;
`actualSelectedPointerTable`; `actualSelectedEventRecord`; `saveLoadAndCrossMapPersistence`;
`calleeScriptAndServiceEffects`; `dialogueAudioPresentationAndStoryMeaning`.

No H3 observation is authorized by this static topology. Runtime selector evaluation, caller state,
program reachability/order, mutation/persistence, service effects, and player-facing meaning remain
deferred.
