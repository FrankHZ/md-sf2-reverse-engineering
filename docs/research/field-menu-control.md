# Field menu control

Status: **Confirmed (static control graph only)**
Evidence date: 2026-08-24

The public H2 fixture `sf2-field-menu-control-static-v1` records the bounded FieldMenu
control graph. It is reproduced with:

```text
uv run sf2 h2 field-menu-control
```

## Provenance and boundary

- Upstream: `ShiningForceCentral/SF2DISASM`, commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`.
- ROM: USA retail SHA-256
  `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Primary source denominator: nine pinned files: `sf2const.asm`, `sf2enums.asm`,
  layout `sf2-05-0x020000-0x028000.asm`, the `s03` and `s05` jump-interface
  sources, `mainactions.asm`, `explorationvints.asm`, `battletest.asm`, and
  `explorationfunctions_0.asm`.
- `mainactions.asm` has 696 physical rows, including 679 non-empty rows, and
  guards 481 statements, 12 global labels, 46 local labels, and 69 direct calls
  to 33 targets.
- H1/ROM parity guards 23 named intervals. The FieldMenu interval is
  `0x2127E..0x219EC` (1,902 bytes) with SHA-256
  `F160CD3803063AE4E2FAD59389803B2083EE0811CA387258957F68EB11AB69ED`.
  The 42-byte force-list helper, six-byte unused table, and 30-byte unused
  helper tail are independently bounded; the tail is excluded from this control
  contract.

The fixture consumes and fresh-guards the accepted common-menus, gameflow-core,
core-stats-data, item-auxiliary, map-setup, tech-interfaces, and common-stats
owners. It does not restate their algorithms or payloads.

## Confirmed static spine

The target-use call at `0x2157C` is additionally consumed by the separate
[field-item-effects.md](field-item-effects.md) static owner. That owner retains its own two-caller
inventory, dispatcher/effect ranges, and runtime-Unknown queue; this FieldMenu control contract's
actual item-use Unknown remains unchanged.

- `j_FieldMenu` at `0x20008` aliases `FieldMenu` at `0x2127E`. Its only guarded
  direct callers are debug Battle Test at `0x7884` and exploration at `0x25BDC`;
  both instruction return addresses are also retained.
- The main selector takes member `0`, magic `1`, item `2`, falls through to
  Search, and exits for `-1`. Member selection uses the main member-list screen,
  hands off to `BuildMemberScreen`, then restarts member selection on return.
- Magic selection uses the magic member-list screen. The source distinguishes
  Detox, Egress, and other spells. Detox has level-1, level-2, and fallthrough
  branches; Egress admits map IDs 66 through 78 and orders MP decrease, flash,
  savepoint lookup, then the retained event/player writes before convergence.
- The item submenu takes Use `0`, Give `1`, Equip `2`, and Drop by fallthrough;
  `-1` returns to the main selector. The guarded branches cover Angel Wing,
  field usability, map-item events, target item use, give/exchange and cursed
  guards, unsellable/rare/drop/Deals handling, the `j_alt_YesNoPrompt` to
  `alt_YesNoPrompt` (`0x1528C`) prompt target, and restart/exit joins.
- Search clears `d6`, calls `CheckArea`, and exits. The force-list helper calls
  `UpdateForce`, copies `TARGETS_LIST_LENGTH` to the generic-list length, then
  copies target bytes in `DBF` counter order.

The separate [field-search-control.md](field-search-control.md) owner retains
the exact static `CheckArea` coordinate, dispatch, content, and fallback spine;
the FieldMenu runtime `actual-search-area-result` remains **Unknown** here.

These are source/H1/ROM control, branch, address, call-order, and constant
facts. They do not establish natural reachability, caller-visible state,
hardware persistence, input timing, presentation, or observed outcomes.

## H3 question queue

All remaining questions are **Unknown** and are intentionally grouped here;
this static slice creates no H3 fixture or emulator launch.

- Reachability and callers: `natural-story-field-menu-reachability`,
  `caller-entry-state`, `caller-return-state-and-vint-reactivation`.
- Selection and outcomes: `actual-main-choice`, `actual-member-selection-and-return`,
  `actual-magic-selection-and-target`, `actual-detox-status-outcome`,
  `actual-egress-map-and-event-outcome`, `actual-item-submenu-choice`,
  `actual-item-use-selection-target-and-result`, `actual-map-item-event-consumption`,
  `actual-item-give-or-exchange`, `actual-item-equip-result`,
  `actual-item-drop-confirmation-and-result`, `actual-search-area-result`.
- Input, presentation, and persistence: `input-repeat-cancel-and-cadence`,
  `window-cursor-portrait-text-sound-presentation`,
  `persistence-across-map-save-story`.
