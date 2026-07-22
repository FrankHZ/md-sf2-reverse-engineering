# Common Stats and Inventory Services

- Status: **Confirmed** for the pinned 20-file inventory, flags/party/caravan/deals routing,
  combatant-type encoding, spell-learning outcomes, new-game order, alternate-source ownership, and
  the complete 31-entry getter and 53-entry mutation instruction/ABI/caller contracts
- Status: **Inferred** for UI-facing helper intent not reproduced through callers
- Status: **Unknown** for caller-dependent inventory UI, caller-dependent getter edge behavior,
  clamp-helper algorithms, and caller-dependent mutation outcomes
- Evidence date: 2026-07-21
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

## Combatant Getter Contract

**Confirmed — static getter surface.** Pinned upstream `master`
`c834c652b6862bc5679fd7f69a38a7093206efc6` supplies all 31 global entries from
`code/common/stats/combatantstats_1.asm:GetCombatantName` (`0x82D0`) through `GetDefeats`
in one 586-byte physical interval (`0x82D0..0x851A`). The exact fixture
`sf2-common-stats-static-v1`, `src/sf2tool/h2/stats.py`, both mirrored schemas, and
`tests/python/test_common_stats.py` retain every routine's complete instruction/local-label corpus,
H1 address, offset use-site, lower helper, width, terminal, aliases, and instruction-scoped callers.
Reproduce with `uv run sf2 h2 common-stats`; observed 2026-07-21 canonical SHA-256 is
`D979AC018B895D73DF21619E81CB225794CD248420EFAFA6F90C894022BDF09D`.

`combatantstats_3.asm` is consulted only for the ABI used here: its three dependency routines retain
the parsed instruction records, static ally/enemy/error branch polarity, derived 56-byte entry stride,
both `ERRCODE` writes, VInt arguments, and the self-branching error terminal. `GetCombatantByte`'s
`clr.w d1` followed by `move.b` and `GetCombatantWord`'s `move.w` are recorded as source operations,
not semantic booleans. This does not model its setters or clamp helpers. `GetCombatantName` has
separate ally entry/name-length and enemy-index/name-table/`FindName` source paths. The composite
move-type, AI commandset, move-order, and trigger-region getters retain named copy, shift, and mask
operations rather than inferred gameplay behavior. The static invalid-selector error route is
**Confirmed**; caller-visible meaning remains **Inferred**, while runtime outcome, clamp-helper
algorithms, and caller-dependent mutation outcomes remain **Unknown**.

## Combatant Mutation Wrapper Contract

**Confirmed — static wrapper surface.** The same pinned source supplies 53 entries in
`code/common/stats/combatantstats_2.asm`, from `LoadAllyName` through `DecreaseCurrentMov`.
The contract records every complete instruction/local-label corpus, H1 address, selector/value or
delta width, field use-site, lower helper boundary, preservation/terminal order, and alias-aware
caller map. Direct setters, increase/decrease wrappers, `LoadAllyName`, packed move-order/trigger
merges, guarded kills/defeats, and current HP/MP maximum-read paths remain source-shaped forms.
Only the called `combatantstats_3.asm` helper ABI records are included: entry-address call,
read/write access mode and widths, `d0`/`d1`/`d7` roles, clamp `d5`/`d6` arguments, and terminal.
Clamp algorithms themselves remain **Unknown** at this slice boundary.

### H3 Runtime-Question Queue

The next static frontier is the full `combatantstats_3.asm` clamp-algorithm contract. One grouped
later H3 launch can then cover invalid-selector, clamp, caller, UI, and persistence edge cases. This
slice starts no emulator and makes no claim about runtime presentation or caller lifecycle.

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
