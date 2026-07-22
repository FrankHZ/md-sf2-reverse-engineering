# Common Stats and Inventory Services

- Status: **Confirmed** for the pinned 20-file inventory, flags/party/caravan/deals routing,
  combatant-type encoding, spell-learning outcomes, new-game order, alternate-source ownership, and
  the complete 31-entry getter, 53-entry mutation, and seven-routine clamp-helper
  instruction/ABI/caller contracts
- Status: **Inferred** for UI-facing helper intent not reproduced through callers
- Status: **Unknown** for caller-dependent inventory UI, caller-dependent getter edge behavior, and
  caller-dependent mutation outcomes beyond the existing H3 clamp fixture matrix
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
**Confirmed**; caller-visible meaning remains **Inferred**, while runtime outcome and
caller-dependent mutation outcomes remain **Unknown**.

## Combatant Mutation Wrapper Contract

**Confirmed — static wrapper surface.** The same pinned source supplies 53 entries in
`code/common/stats/combatantstats_2.asm`, from `LoadAllyName` through `DecreaseCurrentMov`.
The contract records every complete instruction/local-label corpus, H1 address, selector/value or
delta width, field use-site, lower helper boundary, preservation/terminal order, and alias-aware
caller map. Direct setters, increase/decrease wrappers, `LoadAllyName`, packed move-order/trigger
merges, guarded kills/defeats, and current HP/MP maximum-read paths remain source-shaped forms.
The wrapper contract retains its source-facing lower-helper ABI: entry-address call, read/write access
mode and widths, `d0`/`d1`/`d7` roles, clamp `d5`/`d6` arguments, and terminal. The helper
algorithms are separately modelled below; this wrapper surface alone does not establish caller meaning.

## Combatant Clamp-Helper Contract

**Confirmed — static algorithm and caller surface.** Pinned upstream `master`
`c834c652b6862bc5679fd7f69a38a7093206efc6`,
`code/common/stats/combatantstats_3.asm:IncreaseAndClampByte` through the byte before
`GetDistanceBetweenCombatants`, is the 268-byte physical interval `0x9312..0x941E`.
`uv run sf2 h2 common-stats` parses the seven routine entries in source order, their H1 addresses,
the complete instruction/local-label corpora, and `BYTE_MASK=255`, `TWO_TURN_THRESHOLD=128`, and
`TURN_AGILITY_MASK=127` from `sf2enums.asm`. The observed canonical extraction digest on
2026-07-21 is `580CE7333A1910A99E7A17D83CAEC169E5D41442B10FAF01872B502A7D8F3E5C`.

The six byte/word/long helpers retain exact entry-call, field access, arithmetic, branch,
comparison/assignment, preservation/restoration, write, normalization, and terminal records. The
seven-bit increase helper separately preserves the `TWO_TURN_THRESHOLD` field bits in `d3`, masks the
working field byte in `d2` with `TURN_AGILITY_MASK`, adds/clamps that low portion, ORs `d3` back,
writes the byte, then masks the result with `BYTE_MASK`. These are source-shaped register and constant
relationships, not a claim about the gameplay meaning of the field bits.

The alias-aware instruction parser finds all 25 direct wrapper sites in
`code/common/stats/combatantstats_2.asm`: `IncreaseAndClampByte` 10,
`IncreaseAndClamp7Bits` 2, `DecreaseAndClampByte` 8, `IncreaseAndClampWord` 4, and
`DecreaseAndClampWord` 1. `IncreaseAndClampLong` and `DecreaseAndClampLong` have zero direct sites
in the complete `code/` caller inventory. This is a **Confirmed** static caller count, not proof that a
zero-site helper cannot be reached indirectly or at runtime.

The output contract is `statsFacts.combatantClampContract` in fixture
`tests/fixtures/h2/common-stats-static-v1.json` under fixture ID `sf2-common-stats-static-v1`; its
strict mirrored output/fixture schemas and focused tests retain every operation, H1 boundary, caller
target identity/site count, and existing H3 boundary. Reproduce with `uv run sf2 h2 common-stats`.

### H3 Runtime-Question Queue

The existing BizHawk fixture `sf2-stat-clamp-boundaries-v1` emulator-confirms a nine-operation matrix:
four increase-byte cases, one increase-word case, one increase-seven-bits case, and three decrease-byte
cases. It does not cover `DecreaseAndClampWord`, `IncreaseAndClampLong`, `DecreaseAndClampLong`, or a
decrease-current-ATT outcome. A future grouped launch must introduce a new fixture/case matrix for those
questions rather than implying that this fixture ID covers them. This slice performs no emulator run and
makes no runtime lifecycle or presentation claim. The next static frontier is
`GetDistanceBetweenCombatants`.

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
