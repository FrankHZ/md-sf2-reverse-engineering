# Map-Event Direct Control Topology

- Status: **Confirmed** for the bounded caller-side direct-transfer source/H1/ROM topology.
- Evidence date: 2026-08-25.
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Source: `ShiningForceCentral/SF2DISASM` `master` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`.

`sf2-map-event-direct-control-static-v1` fresh-builds the retained 914-program, 3,579-operation
map-event corpus, then retains only caller-side raw-68000 direct transfers. The zero-inclusive
context denominator is 154 positive and 760 zero contexts. Its 205 contextual sites are 143
returning direct calls (`jsr` or `bsr`) and 62 `jmp` tail transfers over 201 physical PCs. The four
shared physical PCs `0x54C4E`, `0x54C6C`, `0x54C7A`, and `0x54C9A` are retained in both
`Map6_EntityEvent13` and `Map6_DefaultEntityEvent`; contextual records preserve both callers while
physical records deduplicate the PC.

The topology resolves 35 instruction/effective callee identities and 15 jump-interface alias pairs.
For each transfer it records the source instruction identity, effective target identity, owning caller
event-table source, source/H1/ROM anchor, and effective callee entry owner. It does not parse or
attribute a callee algorithm. The 53 positive caller event-table sources divide into 36 entity, 13
zone, and four item sources. Their 25 effective-target files and four alias-definition files yield 81
unique source identities after the one caller/target overlap for `sub_5A278`.

For returning calls, the immediate lexical continuation is source-shaped and has exactly 72 ordinary,
57 return, six direct-call, six unconditional-branch, one conditional-branch, and one direct-jump
kinds. Every `jmp` instead retains its complete lexical suffix, including the two nonempty suffixes;
a tail transfer is not treated as a returning call. The public fixture carries 251 semantic H1/ROM
anchors: 201 transfer instructions, 15 alias entries, and 35 effective callee entries. Its H1 listing
has unresolved relocation operands where appropriate; guards prove the source-resolved ROM target
encoding without claiming the two stored byte forms are identical.

This is caller topology only. It does not establish normal-story reachability, entry registers, call
order in an observed run, callee state or side effects, returned registers/CCR, post-call consumption,
tail-return behavior, persistence, timing, UI, dialogue, audio, or story meaning. Reproduce with:

```powershell
uv run sf2 h2 map-event-direct-control
```

## Grouped H3 Runtime Questions

1. **Unknown:** naturalProgramReachability; callerEntryState; runtimeTransferOrder; preCallRegisterValues.
2. **Unknown:** calleeEntryState; calleeSideEffects; calleeReturnRegistersAndCcr; postCallConsumerSelection.
3. **Unknown:** tailTransferReturnBehavior; crossMapStateLifetime; saveLoadPersistence;
   inputUiDialogueAudioTimingAndStoryMeaning.
