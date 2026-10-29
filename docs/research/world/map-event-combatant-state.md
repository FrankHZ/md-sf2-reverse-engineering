# Map-Event Combatant-State Callers

## Static caller join

**Confirmed — static source/H1/ROM structure.** The public fixture
`sf2-map-event-combatant-state-static-v1` selects exactly two positive programs from the complete
914-context map-event program corpus (912 zero contexts):
`Map20_1F5_ZoneEvent0` at `0x5376E..0x537AA` (60 bytes, 12 operations) and
`Map67_ZoneEvent0` at `0x4FB32..0x4FB58` (38 bytes, 11 operations). The two physical bodies contain
98 bytes, three labels, five event-service operations, four raw instructions, 14 raw control-flow
operations, and nine stat calls. The fixture anchors all 23 PCs, both event-table entries, and ten
jump-interface/effective-entry seams against the pinned H1 listing and USA ROM.

Map 20's source order selects numeric values 1 then 2, independently joined to
`ALLY_SARAH=1` and `ALLY_CHESTER=2` in `sf2enums.asm`. Each has the same source-shaped sequence:
`GetMaxHp`, `SetCurrentHp`, `GetMaxMp`, `SetCurrentMp`. Map 67 selects
`ALLY_ELRIC=13`, calls `GetCurrentHp`, then has the exact `tst.w d1` / `beq.s return_4FB56` predicate
shape. This records caller structure, aliases, targets, operand/order, and branch polarity only.

Provenance: USA ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`; upstream
`ShiningForceCentral/SF2DISASM` `master` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`;
the six listed source identities; `build/sf2build-h1.lst` SHA-256
`F28FAF604DD8F37AE3EDAA819DD1C9A601863B0596F2C83602CA3D572BB8644D`; reproduce with
`uv run sf2 h2 map-event-combatant-state`. Evidence date: 2026-08-27.

## H3 runtime-question queue

**Unknown — grouped map-event combatant-state effects:** natural program reachability and selected
context; entry registers/state and actual alias/return chronology; ally identity, getter results, HP/MP
writes, and predicate value/branch; script/flag effects and save/load persistence; and input, dialogue,
audio, presentation timing, and story meaning. No emulator run is included in this static slice.
