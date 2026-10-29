# Field Item Effects

- Status: **Confirmed** for the bounded static source/H1/ROM contract; runtime caller and result
  semantics remain **Unknown**
- Evidence date: 2026-08-24
- Upstream: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- ROM: USA retail SHA-256
  `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`

The public H2 fixture `sf2-field-item-effects-static-v1` is reproduced with:

```text
uv run sf2 h2 field-item-effects
```

It inventories exactly eight pinned source identities: `sf2const.asm`, `sf2enums.asm`, the section-05
layout, FieldMenu and Caravan action sources, the field-usability function and data table, and the
field-effects source. The layout canonically includes the three item/menu/data sources in that order;
the unassembled `code/common/stats/items/fielditemeffects.asm` alternate remains excluded.

## Confirmed static contract

The complete comment-stripped direct `bsr.w UseItemOnField` source scan has exactly two sites:
FieldMenu `0x2157C` and Caravan `0x225D0`, each returning four bytes later. Their instruction and
effective targets are both `UseItemOnField`; this source inventory does not establish that either
caller state occurs in ordinary play, which remains **Unknown**.

`IsItemUsableOnField` is `0x229CA..0x229E2`; its table is
`0x229E2..0x229EC`. The ordered usable IDs are `3, 5, 9, 10, 11, 12, 13, 14, 15`, followed by the
byte sentinel `0xFF`. It accepts the item ID in `d1`, reports match `0` or not-found `-1` in `d2`, and
preserves `d1` in this bounded source path.

`UseItemOnField` and its relative dispatch table occupy `0x229EC..0x22A4E`. It masks `d1` with
`ITEMENTRY_MASK_INDEX` (`0x7F`), scans the same ordered IDs with a word `-1` sentinel, and restores
`d0`, `d1`, `d6`, `d7`, and `a0` before return. Its dispatch order is Antidote → CurePoison, Fairy
Powder → CurePoisonAndParalysis, Power Water → IncreaseAtt, Protect Milk → IncreaseDef, Quick Chicken
→ IncreaseAgi, Running Pimento → IncreaseMov, Cheerful Bread → IncreaseHp, Bright Honey → IncreaseMp,
and Brave Apple → LevelUp.

The fourteen H1/ROM-parity intervals exactly cover the two caller instructions, usability function and
table, dispatcher, and the nine effect bodies: `0x2157C..0x21580`, `0x225D0..0x225D4`,
`0x229CA..0x229E2`, `0x229E2..0x229EC`, `0x229EC..0x22A4E`, `0x22A4E..0x22A70`,
`0x22A70..0x22AAE`, `0x22AAE..0x22AD6`, `0x22AD6..0x22AFE`, `0x22AFE..0x22B26`,
`0x22B26..0x22B62`, `0x22B62..0x22B8A`, `0x22B8A..0x22BC2`, and `0x22BC2..0x22C60`.

`CurePoison` clears source bit `STATUSEFFECT_BIT_POISON` (mask `2`) with `bclr`; the clear result's
`beq` branch selects the no-use path before the unconditional `SetStatusEffects` call. `CurePoisonAndParalysis`
clears poison then stun (bits `1`, `0`; masks `2`, `1`), tracks either successful clear with `d2`, and
uses `bne` as the effect-present branch. A zero `d2` falls through to text ID `148` (the no-use path),
then every path calls `SetStatusEffects` followed by `UpdateCombatantStats`.

For Att, Def, Agi, and HP, the guarded use sites set `d6 = 3`, call `GenerateRandomNumber`, add `2` to
`d7`, and therefore statically construct a gain of `2..4`; their helper order is base/max adjustment
then current-stat adjustment. Movement reads base movement first: base `9` yields `0`, base `8` yields
`1`, and every other source branch yields `2`, then calls base and current movement helpers. MP first
checks maximum MP; zero takes the no-use branch, otherwise it uses the same static `2..4` construction
then maximum/current MP helpers.

LevelUp first sets current EXP to zero, calls `LevelUp`, then addresses `LEVELUP_ARGUMENTS`. A first
`-1` branch is the no-use path; zero stat-change bytes take each `beq` skip; the later `-1` spell value
ends processing; and the nonzero level selector takes the level-increase branch. The public fixture
contains only numeric text identifiers `148`, `149`, `150..156`, `244`, `266..272`, and `3523`; it does
not contain dialogue prose.

The fixture records the current fixture-byte IDs and digests from the accepted common-menus, FieldMenu-control,
core-stats-data, item-auxiliary, tech-interface/common-stats, RNG, level-up, and update-stat owners.
Those joins preserve provenance without duplicating their algorithms or golden fixtures.

## Runtime question queue

The following are **Unknown** and intentionally have no H3/BizHawk fixture in this static slice:

- natural-story-field-item-use-reachability; caller-entry-state; selected-item-member-slot;
  actual-dispatch-target; actual-status-clear-result; actual-random-gain;
  actual-movement-cap-result; actual-zero-mp-rejection;
  actual-level-up-result-and-spell-message-branch; caller-item-removal-and-return;
  persistence-across-map-save-story; and input-text-window-audio-vint-rendering.

FieldMenu actual-item-use and RA-08 questions remain with their existing owners unchanged.
