# Common Menu Engines and Services

- Status: **Confirmed** for the pinned 42-file inventory, 41 layout-owned source files, H1 entry
  addresses, prompt input/return rules, text controls, the nine field-item dispatches, and portrait
  header/palette/tile loading boundaries, plus the complete diamond-menu/yes-no compressed graphics
  and uncompressed item/spell/other icon storage/copy/highlight corpora
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

The nine-entry diamond-menu tile table contains two distinct formats. Its first three entries have
bit 31 set and pack four indices selecting uncompressed main-menu icons. The other six entries point
to pointer words for item, battlefield, church, shop, caravan, and depot Stack streams. Each stream
decodes to 2,304 bytes, matching two animation frames of four 288-byte icon transfers. The separate
yes/no stream decodes to 1,152 bytes, matching two frames of two icons. All seven payloads, their
pointers, and the menu table are source/H1/ROM identical; visible timing and palette composition
remain in the presentation queue.

`NumberPrompt` applies right/left/down/up deltas of `+1/-1/+10/-10`, clamps after each update to the
caller-provided minimum and maximum, returns the selected number on A/C, and returns `-1` on B. Its
idle loop also advances RNG with range 256 once per VInt.

The common text writer has regular and orange entry points, formats numbers through `LOADED_NUMBER`,
and handles move-down, font-toggle, and newline control codes. Presentation timing and exact VDP tile
results remain outside the static claim.

`LoadPortrait` resolves one of 56 pointer slots, copies counted four-byte blink entries, then counted
mouth entries, and copies eight palette longwords into current/base/backup palette state. The
remaining Stack stream deterministically expands to 2,048 bytes before a `0x400`-word VInt VRAM DMA
submission. The complete source/ROM corpus confirms 52 unique payloads and four aliases; blink/mouth
frame timing and visible composition remain presentation questions.

The common icon loaders index a single contiguous block by `index * 192`; no per-icon pointer table
exists. Of 167 available source payloads, 163 are assembled and ROM-identical: 127 item, 30 spell,
and six other icons. `ITEM_NOTHING`/`ICON_NOTHING` selects special slot 127 instead of the unassembled
item-127 payload; item/spell payloads 127 and 16-18 are the four explicit source-only exceptions.
Special slots 146-148 are Jewel of Light, Jewel of Evil, and cracks, while the same arithmetic slots
also equal spell indices 16-18. Slot 129 has neither an enum name nor a symbolic consumer.

The member-list and shop paths copy 192 bytes and OR color 15 into four corner nibbles. The
highlightable path writes a 192-byte base frame and a 192-byte `source AND highlight-mask` frame. The
mask, base pointer, all assembled icon bytes, copy sizes, and mutation offsets are Confirmed; which
reserved/colliding indices can reach these loaders and the visible palette/DMA sequence remain in the
shared UI presentation queue.

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
uv run sf2 h2 portraits
uv run sf2 h2 ui-graphics
uv run sf2 h2 icon-graphics
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/common-menus-static.json`,
`local/derived/ui-graphics-decode.json`, and `local/derived/icon-graphics-static.json`.
