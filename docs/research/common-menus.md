# Common Menu Engines and Services

- Status: **Confirmed** for the pinned 42-file inventory, 41 layout-owned source files, H1 entry
  addresses, prompt input/return rules, text controls, and the nine field-item dispatches
- Status: **Inferred** for service-level intent named by upstream symbols but not replayed through every
  shop, church, caravan, blacksmith, field, and battle caller
- Status: **Unknown** for exact window/portrait animation timing, visual composition, and caller-state
  transitions that static control flow cannot reproduce
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Inventory and Layout Ownership

The recursive `code/common/menus` boundary contains 42 files and 14,827 source lines: 32 at the
directory root, two each under blacksmith, caravan, church, and item, and one each under main and
shop. The source parser records 646 global labels, 1,380 direct call sites, and 29 indirect call
sites. The pinned layout directly includes 41 files, each bound to a representative H1 listing
address.

`writememberlisttext.asm` is the sole exception. Its annotated `0x135A6..0x137AC` range starts at the
included `BuildMembersListWindow` function inside `memberslistscreen.asm`, but the two source files
are not byte-identical and use different entry names and some different helper calls. It is therefore
hash/range checked as an alternate extraction and excluded from strict reach instead of borrowing the
canonical container's address.

## Confirmed Input Contracts

`ExecuteDiamondMenu` maps up/left/right/down to choices 0/1/2/3. A or C confirms, B returns `-1`, and
the optional callback runs after opening and selection changes. An idle iteration advances RNG with a
range of 256 and then waits for VInt.

The yes/no prompt starts on Yes. Left returns 0 (Yes), right returns `-1` (No), A/C confirms, and B
also returns `-1`; callers therefore cannot distinguish No from cancellation through this return
value alone. Existing dialogue and gold windows are moved while the prompt is displayed.

`NumberPrompt` applies right/left/down/up deltas of `+1/-1/+10/-10`, clamps after each update to the
caller-provided minimum and maximum, returns the selected number on A/C, and returns `-1` on B. Its
idle loop also advances RNG with range 256 once per VInt.

The common text writer has regular and orange entry points, formats numbers through `LOADED_NUMBER`,
and handles move-down, font-toggle, and newline control codes. Presentation timing and exact VDP tile
results remain outside the static claim.

## Field Items and Service Boundary

The layout-owned field-item table has nine index/effect pairs, for item indices 3, 5, and 9 through
15, followed by `0xFFFF`. `UseItemOnField` masks status bits from the item entry before dispatch.
Field usability uses a separate byte list ending in `-1` and returns `-1` for an unlisted item.

The inventory binds the top-level `FieldMenu`, `ShopMenu`, `ChurchMenu`, `CaravanMenu`, and
`BlacksmithMenu` entry points, plus the surrounding battle item/magic/equip menus, member screens,
portrait windows, minimap, and ending presentation. Detailed service state machines and animation
behavior are intentionally not promoted from names alone.

## Concentrated Runtime Queue

The next UI runtime batch should share one BizHawk launch and generated case table for prompt return
values, held-input release behavior, window displacement/restoration, and idle RNG advancement.
Portrait, minimap, ending, and menu-icon frame timing should form a separate presentation matrix
because they share VInt/VDP observation points. This static batch starts no emulator.

## Reproduction

```powershell
uv run sf2 h2 common-menus
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/common-menus-static.json`.
