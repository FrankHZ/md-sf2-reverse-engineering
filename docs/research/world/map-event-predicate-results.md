# Map-Event Predicate Results

- Status: **Confirmed** for the bounded static source/H1/ROM predicate-result relation.
- Evidence date: 2026-08-25.
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Source: `ShiningForceCentral/SF2DISASM` `master` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`.

`sf2-map-event-predicate-results-static-v1` fresh-builds the accepted map-events, direct-state,
direct-control, and direct-handoff owners before narrowing their 914 target-program / 3,579-operation
mother corpus. Its complete immediate conditional corpus has 340 contextual and 336 physical branch
sites. The retained direct-flag owner accounts for 316 contextual / 314 physical `chkFlg` branches;
this slice excludes all of them and retains exactly 24 contextual / 22 physical predicate-result
pairs in 19 positive and 895 zero program contexts.

The pairs divide into 18 entity contexts in 14 programs from ten source tables (670 entity zero
contexts), five zone contexts in four programs (146 zone zero contexts), and one item context in one
program (79 item zero contexts). The result-origin cohorts are `j_YesNoPrompt` 8/8,
`j_GetItemInventoryLocation` 7/6, `EVENT_RELATIVE_POSITION` 5/5, `ReceiveMandatoryItem` 2/1,
`j_GetCurrentHp` 1/1, and `ENTITY_FACING` after its retained wait seam 1/1
(contextual/physical). Producer forms are `cmpi` 12/11, `tst` 8/8, `btst` 3/2, and direct
`jsr`-CCR 1/1. Branches are `bne` 20/18 and `beq` 4/4.

The public fixture confirms only source-shaped condition data: comparison/test width and operand,
`#-1` sentinel or `#0` bit where present, origin instruction/effective target identity and alias,
static return-continuation seam, branch opcode/polarity, target, and lexical fallthrough. Map 6
preserves the two shared physical producer/branch pairs as separate program contexts while deriving
the physical denominator once. It guards 59 physical caller anchors plus eight retained alias/effective
entry seams, across the 15 event-table and nine supporting source identities named by the fixture.
It does not copy callee bodies or algorithms from the menu, stats, or entity-function owners.

Provenance is the fifteen retained event source files, `sf2const.asm`, `sf2enums.asm`, the two jump
interfaces, the four bounded menu/stats/entity source entry owners, `build/sf2build-h1.lst`, and the
private ROM. Reproduce with:

```powershell
uv run sf2 h2 map-event-predicate-results
```

## Grouped H3 Runtime Questions

**Unknown:** `naturalProgramReachability`; `callerEntryRegisterAndState`;
`actualYesNoPromptResult`; `actualInventoryLocationResult`; `actualMandatoryItemResult`;
`actualCurrentHpResult`; `actualEventRelativePosition`; `actualEntityFacing`;
`actualCcrAndPredicateEvaluation`; `actualBranchSelection`;
`successorExecutionAndSideEffects`; `tailAndReturnState`; `crossMapStateLifetime`;
`saveLoadPersistence`; `inputUiDialogueAudioTimingAndStoryMeaning`.

No runtime values, actual branch selection, callee effect, UI/dialogue/audio behavior, timing,
reachability, persistence, or story meaning is claimed by this static fixture.
