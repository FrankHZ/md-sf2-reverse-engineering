# Common Menu Engines and Services

- Status: **Confirmed** for the pinned 42-file inventory, 41 layout-owned source files, H1 entry
  addresses, prompt input/return rules, text controls, the nine field-item dispatches, and portrait
  header/palette/tile loading boundaries, plus the complete diamond-menu/yes-no compressed graphics
  and uncompressed item/spell/other icon storage/copy/highlight corpora, and the complete assembled
  UI/window layout, spell-level pointer, diamond-border, and direct menu-tile corpus, plus the
  count-prefixed shop and sequential mithril-selection data contracts, and the complete 17-routine
  shared shop/caravan selection-screen instruction and caller corpus
- Status: **Inferred** for service-level intent named by upstream symbols but not replayed through every
  shop, church, caravan, blacksmith, field, and battle caller
- Status: **Unknown** for exact window/portrait animation timing, visual composition, and caller-state
  transitions that static control flow cannot reproduce
- Evidence date: 2026-07-21
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
pointers, and the menu table are source/H1/ROM identical. The uncompressed main-menu payload is now
independently checked too: seven 576-byte/18-tile icons and its pointer match ROM; the three packed
entries select `[5,1,2,4]`, `[0,1,2,3]`, and `[0,1,2,4]`, leaving icon 6 without a static table
reference. Its dynamic reach, visible timing, and palette composition remain in the presentation
queue.

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

## Complete Static UI Layout Corpus

The vanilla layout assembles 19 graphics/tech ASM owners into 27 leaf layouts. The maintained parser
independently expands `vdpTile`, `vdpBaseTile`, `dc.b`, `dc.l`, and `incbin`, then checks every local
label offset against the H1 listing and all 5,116 assembled source bytes against the ROM. The 27
layouts contain 2,394 VDP words (4,788 bytes), 640 unique attribute words, and 580 unique tile
indices. Their grids include the three 12×9 diamond menus, ten 3×2 spell-level indicators, regular
and mirrored 8×10 portrait windows, the 28×7 alphabet, 26×21 member status, 32×12 battle background,
and all remaining built menu/window layouts.

The sixteen-entry spell-level table resolves to ten unique layouts and its 64 pointer bytes are
source/H1/ROM identical. Four 48-byte diamond-border variants and four direct tile payloads—price-tag
blank, price-tag numbers, shop-item highlight, and alphabet highlight—add 762 bytes. Across layouts,
pointers, borders, and assets the rail covers 5,614 unique original bytes without committing raw tile
or layout data.

`data/graphics/tech/windowborder/entries.asm` and the fighter mini-status layout remain explicit
unassembled alternatives. They receive no borrowed address from the vanilla window resources. Static
layout parity does not prove window allocation, runtime text/tile overwrites, palette selection, DMA
ordering, movement, or final rendered frames; those stay in the shared UI presentation matrix.

## Field Items and Service Boundary

The layout-owned field-item table has nine index/effect pairs, for item indices 3, 5, and 9 through
15, followed by `0xFFFF`. `UseItemOnField` masks status bits from the item entry before dispatch.
Field usability uses a separate byte list ending in `-1` and returns `-1` for an unlisted item.

The item-auxiliary contract independently expands all 30 shop records and byte-compares the complete
265-byte list with ROM. Shop index 0 selects the first record; later indexes walk count-prefixed rows.
It also closes the blacksmith's 9 class groups and 8 weapon rows: groups 0-7 select rows directly,
while BRN/RDBN in group 8 take a two-way random fallback to row 0 or 2 before testing the row's
`16, 8, 4, 1` denominators. Story/debug shop admission and blacksmith persistence/presentation remain
runtime questions; table ordering and selection control flow are no longer inferred.

The inventory binds the top-level `FieldMenu`, `ShopMenu`, `ChurchMenu`, `CaravanMenu`, and
`BlacksmithMenu` entry points, plus the surrounding battle item/magic/equip menus, member screens,
portrait windows, minimap, and ending presentation. The one unbuilt menu alternate remains
`writememberlisttext.asm`; it is not part of the four-service state-machine denominator.

## Static Service-Menu State Machines

The maintained `common-menus` extractor now closes the built eight-source service surface:
blacksmith actions/weapon selection, both caravan files, both church files, shop actions, and the
shared shop/caravan selection screen. It contains 4,286 source lines, 420 direct call sites to 115
unique targets, and 16 indirect call sites; the explicit source-path list prevents the two helper
files from being silently omitted. This is a **Confirmed** source-control-flow contract, backed by
the pinned `master` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`, the H1 entry bindings in
`tests/fixtures/h2/common-menus-static-v1.json`, and source-shape checks in
`src/sf2tool/h2/menus.py`. It adds no new H1 addresses: `BlacksmithMenu` (`0x21A3A`), `CaravanMenu`
(`0x21FD2`), `ChurchMenu` (`0x20A02`), `ShopMenu` (`0x20064`), and `ExecuteShopScreen` (`0x147FA`)
were already bound by this rail.

**Confirmed — Shop static contract.** The Shop parser is pinned to upstream `master`
`c834c652b6862bc5679fd7f69a38a7093206efc6`,
`code/common/menus/shop/shopactions.asm:ShopMenu` (`0x20064`) and
`code/common/menus/shopscreen.asm:ExecuteShopScreen` (`0x147FA`). Its exact fixture is
`sf2-common-menus-static-v1` at `tests/fixtures/h2/common-menus-static-v1.json`, and the parser,
composed output/fixture roots, shared service schemas, and focused source/fixture-mutation tests are
`src/sf2tool/h2/menus.py`, `schemas/h2/common-menus-output.schema.json`,
`schemas/h2/common-menus-fixture.schema.json`, the adjacent `common-menus-*.schema.json` components, and
`tests/python/test_common_menus.py`. Reproduce with `uv run sf2 h2 common-menus`.
Observed result on 2026-07-21: `Status: PASS`, canonical SHA-256
`9D9D1E3B7F847193307DA6E3C0114D33597EE4E7667E99EDFD1C7EF362426DB6`.

`ShopMenu` enters `j_ExecuteDiamondMenu` with `MENU_SHOP`; its local chain compares selector values
`0`, `1`, and `2` for Buy, Sell, and Repair, respectively, and falls through to the Deals section.
This records the actual local branch structure rather than assuming an unobserved selector domain.
The diamond-menu cancel comparison is `-1`; the common shop selection screen tests B, then C, then A
in source order, returns `-1` on B, and confirms on the first C/A branch taken. Every Buy, Sell,
Repair, and Deals source operation is
fixture-pinned as an ordered record of local labels, opcode, operands, direct target, and branch
target; this includes move/add/subtract dataflow as well as calls and branches. Buy/Sell/Repair loop
through their local action labels; Deals includes both the action-loop branch to
`@CheckChoice_Deals` and the cancellation/no-stock branch to `loc_20088`.

Price and eligibility facts are **Confirmed** source dataflow, not runtime amount claims: each of
Buy, Sell, Repair, and Deals has its own `ITEMDEF_OFFSET_PRICE = 6` word-load record. Sell then
executes `ITEMSELLPRICE_MULTIPLIER = 3` followed by `ITEMSELLPRICE_BITSHIFTRIGHT = 2`; Repair
executes a word right shift of `2`. The per-route records prevent one route's load or transform from
validating another. Buy and Deals compare the loaded price with gold before their recipient paths,
and test `COMBATANT_ITEMSLOTS = 4` with `bcs` before their add-item calls. Sell separately tests
`ITEMTYPE_UNSELLABLE` and `ITEMTYPE_RARE`; Repair tests `ITEMENTRY_BIT_BROKEN`. The contract does
not promote a rounding rule, a UI prompt interpretation, or helper-internal resource semantics beyond
those named source calls.

The parser also closes the local list helpers. `PopulateShopInventoryList` copies the count-prefixed
current-shop row into `GENERIC_LIST`; `GetShopInventoryAddress` walks preceding count-prefixed rows
from `CURRENT_SHOP_INDEX`. `DetermineDealsItemsNotInCurrentShop` initializes its `dbf` counter from
`DEALS_ITEMS_COUNTER = 0x7F`, includes only nonzero `j_GetDealsItemAmount` entries whose membership
helper does not find them in the current-shop row, and increments `GENERIC_LIST_LENGTH`. Thus the
stored list count, the inclusive counter value, and the copied entry bytes remain distinct fields.
`ExecuteShopScreen` uses `ITEMS_PER_SHOP_PAGE = 6` for page/selection addressing; its literal
initial-screen multiplier is cross-checked against that parsed constant instead of becoming a second
implementation constant.

**Confirmed — shared selection-screen inventory.** `shopscreen.asm` is parsed as one 1,794-byte
physical source interval (`0x147FA..0x14EFC`) with complete instruction records for its 17 named
routines and separately scoped local labels. The contract retains entry/window creation, directional
input sections, confirm/cancel and cleanup sections, highlight, gold/name, icon/price-tag, selection,
and window-scroll helper records, plus alias-aware zero-inclusive caller totals. This is static source
shape only: input-repeat timing, DMA completion, rendered appearance, and caller admission/lifecycle
remain **Unknown** or **Inferred** in the existing grouped H3 queue.

`code/common/tech/bytecopy.asm:CopyBytes` documents `a0` source, `a1` destination, and `d7.w`
length, so the initial inventory-layout call's stored count is 324 bytes. Separately, the icon/price
routine's `#1599` `dbf` counter yields 1,600 longword clear writes; price-tag blank tiles use 32
longword writes and icon pixels use 48 longword writes. Those counters, derived iteration counts,
longword widths, and the 1,794-byte physical code interval are distinct fixture fields. The VInt DMA
argument records are retained as source operands only; they do not establish a transfer unit, timing,
or rendered result.

The direct-caller inventory is also **Confirmed** instruction-scoped evidence. It resolves the two
pinned jump interfaces while retaining both identities: `j_ShopMenu` in
`code/common/tech/jumpinterfaces/s05_jumpinterface.asm` jumps to `ShopMenu`, and
`j_ExecuteShopScreen` in `code/common/tech/jumpinterfaces/s03_jumpinterface_1.asm` jumps to
`ExecuteShopScreen`. After comment stripping, the complete pinned `code/**/*.asm` scan finds one
`j_ShopMenu` call in `code/common/scripting/map/mapscriptengine_2.asm` and one in
`code/gameflow/special/battletest.asm`, both effective `ShopMenu` sites; it finds three
`j_ExecuteShopScreen` calls in `code/common/menus/caravan/caravanactions_1.asm`, all effective
`ExecuteShopScreen` sites. `shopactions.asm` retains two more `j_ExecuteShopScreen` calls and the
local-helper counts `PopulateShopInventoryList: 1`,
`DetermineDealsItemsNotInCurrentShop: 1`, `DoesCurrentShopContainItem: 1`,
`GetShopInventoryAddress: 2`, and `WaitForMusicResumeAndPlayerInput_Shop: 2`;
`shopscreen.asm` has none of those target calls. Effective external counts are therefore Shop 2 and
selection-screen 3; absence of another direct call remains not an unreachability claim.

**Confirmed — Church static contract.** The Church parser is pinned to upstream `master`
`c834c652b6862bc5679fd7f69a38a7093206efc6`,
`code/common/menus/church/churchactions_1.asm:ChurchMenu` (`0x20A02`), and the directly used
`churchactions_2.asm` helper surface. It uses the same exact `sf2-common-menus-static-v1` fixture,
the composed common-menu schemas, and `tests/python/test_common_menus.py`; reproduce with
`uv run sf2 h2 common-menus`. Observed result on 2026-07-21: `Status: PASS`, canonical SHA-256
`9D9D1E3B7F847193307DA6E3C0114D33597EE4E7667E99EDFD1C7EF362426DB6`.

`ChurchMenu` enters `j_ExecuteDiamondMenu` with `MENU_CHURCH`, compares selector values `0`, `1`,
and `2` for Raise, Cure, and Promote, and falls through to Save. Its `-1` comparison branches to
`@ExitMenu`. The full source statement record corpus preserves labels, opcode, operands, direct target,
and branch target for the four route sections and the complete second action-file helper surface.

Raise iterates the current-force result list with `d7`, skips a member when `j_GetCurrentHp` is above
zero, computes level times `CHURCHMENU_PER_LEVEL_RAISE_COST = 10`, and conditionally adds
`CHURCHMENU_RAISE_COST_EXTRA_WHEN_PROMOTED = 200` after its promotion-data result. After the gold
comparison's `bcc` branch, `routeDerived.raise.mutationCalls` records the **Confirmed** source-derived
mutation-call order `j_DecreaseGold`, then `j_IncreaseCurrentHp` after `move.w #CHAR_STATCAP_HP,d1`,
then `UpdateAllyMapsprite`. This filtered mutation-helper order does not assert that no other calls
intervene; the final current-HP value and caller-visible runtime outcome are **Unknown**, as is prompt
timing.

Cure has separately recorded poison, stun, and curse paths. The parser derives poison cost `10`, stun
cost `20`, and the source masks `STATUSEFFECT_POISON = 2`, `STATUSEFFECT_STUN = 1`,
`STATUSEFFECT_CURSE = 4`, and `STATUSEFFECT_MASK = 0xFFFF`. Poison/stun clear their respective bit
with the source subtraction expression before `j_SetStatusEffects`; curse cost is built from each
cursed held item's price shifted right by two. These are distinct status masks, loop counters, and
price operands, not a claim that all status effects share one lifecycle.

Promotion counts members only after regular-base promotion data and
`CHURCHMENU_MIN_PROMOTABLE_LEVEL = 20` pass their branches. Its structured corpus retains the
regular/special promotion-section searches, special-item loop, SORC branch, item removal path, and
the exact `j_SetClass` then `j_Promote` call order. Save records `SaveGame`, the post-save continue
comparison, and the alternate `WitchSuspend` jump as separate source operands. The runtime meaning of
save continuation, special-promotion availability, and class/UI presentation remains **Unknown**.

The caller inventory is **Confirmed** and alias-aware. `j_ChurchMenu` in
`code/common/tech/jumpinterfaces/s05_jumpinterface.asm` resolves to `ChurchMenu`; direct effective
Church call sites are map-script engine 1, exploration VInt 1, and battle-test 2. It separately
retains five direct `WaitForMusicResumeAndPlayerInput` sites in main-menu actions. This direct-call
inventory does not establish admission reachability or a caller return contract; those remain
**Inferred** in the grouped service H3 queue.

**Confirmed — Caravan static contract.** The Caravan parser is pinned to upstream `master`
`c834c652b6862bc5679fd7f69a38a7093206efc6`,
`code/common/menus/caravan/caravanactions_1.asm:CaravanMenu` (`0x21FD2..0x228A2`) and its direct
helper file `caravanactions_2.asm` (`0x228A8..0x229CA`). The physical spans are 2,256 and 290 bytes,
respectively; they remain separate from the 64-item storage capacity, 4-slot member capacity, word
relative-table entry width, item-definition offsets, and `dbf` loop counters. Reproduce with
`uv run sf2 h2 common-menus`; observed result on 2026-07-21 is `Status: PASS`, canonical SHA-256
`9D9D1E3B7F847193307DA6E3C0114D33597EE4E7667E99EDFD1C7EF362426DB6`.

The top word-relative table is, in source order, `caravanMenu_Join`, `caravanMenu_Depot`,
`caravanMenu_Item`, and `caravanMenu_Purge`; its selector doubles `d0`, its `-1` branch targets
`@ExitCaravan`, and non-exit actions branch to `@RestartCaravan`. Depot and Item repeat that
word-relative selector shape with source orders Look/Deposit/Derive/Drop and Use/Give/Equip/Drop.
These are **Confirmed** control-flow records, not a claim about visible menu timing.

The source also confirms a 12-member battle-party comparison before the Join relief path, Join's
direct mutation-call order (join; then leave/join in its relief path), and Purge's leave call.
Depot deposit compares the parsed 64-item capacity, branches with `bcc.s` to its no-room route, then
calls add-to-caravan before drop-by-slot. Derive compares the parsed four-slot member capacity;
its normal path calls add-item then remove-from-caravan, while its exchange path calls remove-item-
by-slot, remove-from-caravan, add-item, then add-to-caravan. Both depot and member-item drops test
the parsed rare bit 3 after their removal and conditionally call add-to-deals; unsellable bit 4 is a
separate guard. Depot Look's price display separately records the word load at offset 6, multiply by
3, and logical right shift by 2. These static call orders do not establish helper-internal mutation
semantics or runtime persistence.

For Item, Use calls `UseItemOnField` before remove-item-by-slot. Give preserves separate self,
non-full recipient, and exchange call sequences; Equip only confirms its source-selected
`ITEM_SUBMENU_ACTION_EQUIP` handoff. The alias-aware, comment-stripped direct caller inventory
resolves `j_CaravanMenu` in `code/common/tech/jumpinterfaces/s05_jumpinterface.asm` to `CaravanMenu`;
it finds one effective site each in exploration VInt and battle-test, and zero-inclusive internal and
external effective-target totals remain fixture-pinned. Direct callers do not prove service admission,
return state, or reachability, which remain **Inferred** and **Unknown** in the grouped H3 queue.

**Confirmed — Blacksmith static contract.** Pinned upstream `master`
`c834c652b6862bc5679fd7f69a38a7093206efc6` supplies eight complete named-function records across
`blacksmithactions.asm` (`0x21A3A..0x21EB6`) and `pickmithrilweapon.asm`
(`0x21ED6..0x21F62`). The 1,148-byte and 140-byte physical spans remain distinct from the 24-byte
stack frame, four order slots, two-byte order storage width, inclusive counters, and eight-byte weapon
row stride. Reproduce with `uv run sf2 h2 common-menus`; observed 2026-07-21 SHA-256 is
`9D9D1E3B7F847193307DA6E3C0114D33597EE4E7667E99EDFD1C7EF362426DB6`.

The source confirms four visit-local counter clears; the force-copy length transfer from
`TARGETS_LIST_LENGTH` to `GENERIC_LIST_LENGTH`; byte `(a0)+` to `(a1)+` copying; a byte decrement and
`dbf @CopyForceMembersList_Loop`; and the distinct `TARGETS_LIST_LENGTH` word load into `d7` that
supplies that loop counter. The same literal flag 80 appears at the `j_CheckFlag` and later
`j_ClearFlag` use sites. Fulfillment records cancellation/full-inventory/equippability/cursed branches
and the exact add-item, word storage-clear, fulfilled-count increment, optional-equip sequence.
Placement records the source-ordered material-selection cancellation, mithril match, customer-selection
cancellation, promotion, eligible-class, confirmation, and gold gates, followed by decrease-gold,
drop-by-slot, `PickMithrilWeapon`, literal-80 load, and flag-clear. The max-order comparison is a
separate post-placement continuation branch, not an admission gate. Fulfillment separately records
recipient cancellation, inventory capacity, equipment type, equippability, optional-equip confirmation,
weapon/ring cursed-unequip rejection, and newly-equipped curse outcome branches.
The cross-owned class/weapon tables are checked only for count/prefix/row shape: the class list loads a
word prefix, decrements it, and loops with `dbf`; it contains 15 eligible classes, while the item-owned
tables have nine source class groups and eight weapon rows. `PickMithrilWeapon` preserves initial row 0,
the BRN/RDBN fallback RNG bound/zero-to-row-2 branch/convergence, the group-prefix/class-match inner
and outer loops, byte parameter/item reads, parameter-to-RNG-range transfer, result branch, and weighted
loop. Its parameter column is cross-checked to the item-auxiliary-owned `[16, 8, 4, 1]` denominator
sequence. The separate two-byte order-slot search retains empty/occupied polarity, stride load/add,
loop target, and word write.
Persistence, RNG distribution, prompt meaning/timing, caller admission, and direct-call reachability
remain **Inferred** or **Unknown** in the existing grouped H3 queue.

The interaction-level handoff is recorded in
[`service-interactions.md`](../design/contracts/service-interactions.md). It deliberately consumes only the
confirmed action ordering, cancellation boundary, and direct mutation calls; it is not a claim about
the original presentation or persistence lifecycle.

## Concentrated Runtime Queue

One future service-menu H3 launch should use a generated matrix rather than one case per branch:

- enter each vanilla shop, church, caravan, and blacksmith caller; record admission preconditions,
  return state, and cancel behavior;
- for shop/deals, church raise/cure/promotion/save, caravan depot transfer/drop, and blacksmith
  order/fulfillment, snapshot gold, party inventory, caravan storage, order storage, flags, and save
  state before/after both confirm and cancellation paths;
- vary map reload, save/reload, story flag 80, and blacksmith-ready conditions to distinguish
  per-visit stack state from persistent state;
- share VInt/VDP/audio observation points for window/portrait movement, prompt release behavior, and
  post-music continuation timing.

This queue leaves caller-dependent service admission/return intent **Inferred** and persistence,
window/portrait/audio/input timing, and final rendered composition **Unknown**. It starts no
emulator in this static slice.

## Reproduction

```powershell
uv run sf2 h2 common-menus
uv run sf2 h2 portraits
uv run sf2 h2 ui-graphics
uv run sf2 h2 icon-graphics
uv run sf2 h2 ui-layouts
uv run sf2 h2 item-auxiliary
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/common-menus-static.json`,
`local/derived/ui-graphics-decode.json`, `local/derived/icon-graphics-static.json`, and
`local/derived/ui-layout-static.json`.
