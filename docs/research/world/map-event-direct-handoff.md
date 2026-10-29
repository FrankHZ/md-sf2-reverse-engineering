# Map-Event Direct Handoff

- Status: **Confirmed** for bounded static caller-side preparation and first lexical continuation shape.
- Evidence date: 2026-08-25.
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Source: `ShiningForceCentral/SF2DISASM` `master` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`.

`sf2-map-event-direct-handoff-static-v1` fresh-builds and retains the complete map-events,
direct fixed-RAM state, and direct-control owners. Its source identity corpus is exactly the 53
positive direct-control caller-table sources (36 entity, 13 zone, and four item) plus
`sf2enums.asm`. The retained mother denominator remains 914 target-program contexts and 3,579
operations; the retained control relation remains its 205 transfer contexts. It does not re-own
their tables, transfer topology, aliases, effective targets, fixed-state facts, or callee
algorithms.

For each transfer, the handoff owner retains the maximal adjacent preceding raw-instruction group:
56 are empty, 118 have one operation, 29 have two, and two have four. That is 184 contextual
setup operations over 177 physical PCs, with 149 nonempty transfers. For the 143 direct-call
contexts, it retains only the first lexical post-return consumer: 143 contextual records over 139
physical PCs. Their kinds are 72 ordinary, 57 return, six direct-call, six unconditional-branch,
one conditional-branch, and one direct-jump. Together, setup and continuation records are 327
contextual operations over 299 physical PCs, including 17 PCs in both roles.

All 299 physical anchors are independently checked against their source statement, H1 listing row,
and ROM encoding, including opcode, width, operand order, branch polarity, and source-resolved
target where relocation requires it. The source-shaped immediate operands resolve 78
`sf2enums.asm` identities across 120 contextual uses. The fixture carries only structural IDs,
addresses, opcode/operand shapes, enum joins, orders, and hashes. It retains direct-state access-site
IDs and direct-control transfer/effective-target/alias references instead of copying either
owner's facts.

This adds semantic handoff depth, not new topology. Callee bodies remain with their existing
menu, map, stats, text, and technical owners; this contract makes no runtime-value, actual branch,
callee-side-effect, natural-reachability, dialogue, persistence, timing, or rendered-behavior
claim. Reproduce with:

```powershell
uv run sf2 h2 map-event-direct-handoff
```

## Grouped H3 Runtime Questions

**Unknown:** `naturalProgramReachability`; `actualPreparationPath`;
`actualRegisterAndCcrValuesAtTransfer`; `actualFixedStateValuesAtTransfer`; `calleeEntryState`;
`calleeSideEffects`; `calleeReturnRegistersAndCcr`; `actualContinuationAndBranchSelection`;
`tailTransferReturnBehavior`; `crossMapStateLifetime`; `saveLoadPersistence`;
`inputUiDialogueAudioTimingAndStoryMeaning`.
