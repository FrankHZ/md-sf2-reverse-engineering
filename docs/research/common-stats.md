# Common Stats and Inventory Services

- Status: **Confirmed** for the pinned 20-file inventory, flags/party/caravan/deals routing,
  combatant-type encoding, spell-learning outcomes, new-game order, and alternate-source ownership
- Status: **Inferred** for UI-facing helper intent not reproduced through callers
- Status: **Unknown** for caller-dependent inventory UI and remaining getter/setter edge behavior
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Confirmed Services

The recursive `code/common/stats` boundary contains 20 files, 5,149 lines, 235 global labels, and 292
direct call sites. Seventeen files have independent executable/source evidence. Flags use eight bits
per byte with a bit-7-first mask. Joined and active party state use separate flag ranges; `UpdateForce`
rebuilds force, active, and reserve lists, while `JoinForce` auto-activates only below force capacity.

Caravan additions strip item status bits and ignore a full caravan; removal compacts the list and
writes `ITEM_NOTHING` at the tail. Deals store two four-bit counts per byte, saturate additions, and
ignore removal at zero. The otherwise unused combatant-type encoding sets bit 15 for allies and
combines class type, ally count, and ally index; enemies return their enemy index.

`LearnSpell` returns 0 for success, 1 when the same/higher level is known, and 2 when no slot remains;
a higher level replaces the existing entry. A missing spell definition falls back to entry zero.

`NewGame` clears settings, initializes every ally, assigns starting gold, then joins Bowie. Ally
initialization fills empty spell slots, loads class data, initializes base stats, then recomputes
derived stats. The settings pass clears flags, deals, and caravan and sets message speed to 2.

## Alternate Source Boundary

Three files under `code/common/stats/items` are alternate extractions. `itemfunctions_s7_0.asm`
covers the same annotated ROM range as the included `iteminventory.asm`. `fielditemeffects.asm` and
`itemactions_1.asm` likewise overlap the layout-owned files under `code/common/menus/item`. The paired
sources are not byte-identical, but retain the same annotated ranges and shared entry symbols. All
three alternates remain hash/inventory checked and are excluded from strict H1 file reach instead of
borrowing their canonical files' addresses.

Field-item dispatch and usability therefore belong to the common-menu rail. UI behavior and
caller-sensitive edge cases remain grouped runtime questions; this batch adds no emulator run.

## Reproduction

```powershell
uv run sf2 h2 common-stats
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/common-stats-static.json`.
