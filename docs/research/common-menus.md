# Common Menu Engines and Services

- Status: **Confirmed** for the pinned 42-file inventory, 41 layout-owned source files, H1 entry
  addresses, prompt input/return rules, text controls, the nine field-item dispatches, and portrait
  header/palette/tile loading boundaries, plus the complete diamond-menu/yes-no compressed graphics
  and uncompressed item/spell/other icon storage/copy/highlight corpora, and the complete assembled
  UI/window layout, spell-level pointer, diamond-border, and direct menu-tile corpus, plus the
  count-prefixed shop and sequential mithril-selection data contracts, the original committed-order
  post-confirmation chronology, the handler-local blacksmith fulfillment pre-commit source contract,
  and the complete 17-routine shared shop/caravan selection-screen instruction and caller corpus
- Status: **Inferred** for service-level intent named by upstream symbols but not replayed through every
  shop, church, caravan, blacksmith, field, and battle caller
- Status: **Unknown** for exact window/portrait animation timing, visual composition, and caller-state
  transitions that static control flow cannot reproduce
- Evidence date: 2026-08-12
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

**Confirmed — PickMithrilWeapon helper H3.** One direct-function BizHawk matrix reproduces the helper
alone after the startup `CheckSram` return, with each case supplying only the stack-local client class,
`RANDOM_SEED`, and four two-byte order slots. The five ordered cases cover ordinary row-0 early choice,
ordinary row-2 final fallback across source-owned `[16, 8, 4, 1]` parameters, both BRN/RDBN fallback
polarities, each first-empty slot, and the all-occupied no-write return. The original helper preserves
the supplied `d0`/`d7` words, consumes the observed generator calls, and writes only the selected item
to the first zero word when one exists.

**Confirmed — committed-order H3 transaction.** At
`BlacksmithAction_PlaceOrder:@PlaceOrder` (`0x21DC2`), the pinned source and H1/ROM guard establish the
post-confirmation chronology: `DecreaseGold(BLACKSMITH_ORDER_COST = 5000)` through `j_DecreaseGold`
(`0x21DC8 → 0x8160 → 0x89B4`, RTS `0x89CC`), `addi.w #1,pendingOrdersNumber(a6)` (`0x21DCE`),
`DropItemBySlot` through its jump interface and original `UpdateCombatantStats` tail
(`0x21DDC → 0x81A0 → 0x8E12 → 0x89CE`, RTS `0x8A24`), `PickMithrilWeapon` (`0x21DE2 → 0x21ED6`, RTS
`0x21F60`), then `ClearFlag(80)` (`0x21DEA → 0x826C → 0x98D4`, RTS `0x98E6`). `DecreaseGold` writes the
`CURRENT_GOLD` longword; `DropItemBySlot` uses the 56-byte `COMBATANT_DATA` stride and four words at item
offset 32; `GetFlag` maps flag 80 to byte `GAME_FLAGS + 10` and mask `0x80`; the picker writes one of four
two-byte order words. These are source/H1/ROM facts, not a claim about another caller or helper behavior.

One grouped BizHawk transaction cohort exercises first-empty order slots 0, 2, and 1 with WIZ,
PLDN, and BRN/RNG variation, without repeating the helper-local cases. Each synthetic continuation jumps
to the original `@PlaceOrder` code with the original helpers intact. At the original `ClearFlag` RTS it
proves that the stack word is exactly the first presentation address `0x21DF0`, replaces only that word
with the generated case continuation, and then allows the original RTS. The observed chronology is
DecreaseGold → pending increment → DropItemBySlot → picker first-empty order write → ClearFlag, with the
three source-derived gold/item/order/flag/RNG outcomes recorded per case. Its restoration claim is exactly
the snapshot/readback set: `CURRENT_GOLD` longword, `RANDOM_SEED` word, four order words, flag-80's owning
byte, and the three selected 56-byte combatant records. Probe code, synthetic frame, stack, and all other
work RAM are explicitly excluded. Reproduce the grouped run with
`uv run sf2 h3 blacksmith-mithril --timeout-seconds 180`.

**Confirmed — direct fulfillment `@AddItem` H3 cohort.** One BizHawk 2.11.1 / Genesis Plus GX launch retains all five helper and
three committed-order results, then uses three accepted-recipient synthetic action frames to enter only
`BlacksmithAction_FulfillOrder:@AddItem` (`0x21BE4`). The source/H1/ROM guards bind
`clientMember=-6`, `itemIndex=-10`, `ordersCounter=-22`, and `fulfilledOrdersNumber=-16`; the original
block calls `j_AddItem` (`0x21BEC → 0x8198 → 0x8CA2`, RTS `0x8CD2`), computes physical order index
`BLACKSMITH_MAX_ORDERS_NUMBER - ordersCounter`, reads and zeroes that exact two-byte word, increments the
synthetic fulfilled counter, then calls `j_IsWeaponOrRingEquippable`
(`0x21C16 → 0x81B4 → 0x8F80`, RTS `0x8F9A`). `AddItem` scans four item words for the first
`ITEM_NOTHING` and returns `d2=0` after its masked write; it does not reach `UpdateCombatantStats` in
this bounded path. The helper reads combatant class byte 10, builds its class bit, and preserves the
resulting carry through its final register restore.

The observed cases vary source-selected physical order slots/counters `3/1`, `2/2`, and `0/4` and first-empty
inventory positions `3`, `2`, and `0`: HERO/Levanter writes slot 3, clears order 3, reaches fulfilled count
1, and has carry set; VICR/Goddess Staff writes slot 2, clears order 2, reaches count 2, and has carry set;
SNIP/Mystery Staff writes slot 0, clears order 0, reaches count 3, and has carry clear. `AddItem` returns
`d2=0` in all three cases. Each record has the same exact 11-event chronology: AddItem call,
instruction-target, effective target, and effective return; order read, clear, and fulfilled-count increment;
then equippability call, instruction-target, effective target, and effective return. Their source
item-definition rows, class masks, weapon-type bit, H1 records, and ROM bytes are independently guarded.
Each target order starts non-empty with its supplied item; none starts with a full inventory. At the original
equippability RTS, the observer observed A7=`0xFFFEFC` and top longword `0x21C1C`, rewrote only that stack
longword to the generated continuation, and let the original RTS execute. It never rewrites PC or executes
the following branch, optional equip prompt, or UI. The final exact readback passed for the existing three
56-byte records and four order words (plus the existing gold, seed, and flag ranges); the synthetic frame,
stack, and probe remain excluded.

**Confirmed — fulfillment pre-commit admission H3.** Static ownership is the
complete `BlacksmithAction_FulfillOrder` section from entry `0x21B42` through the original `@AddItem`
entry `0x21BE4`. Source, H1, and ROM independently guard each pre-commit call, its six-byte `JSR`
encoding, instruction/effective target identity, exact return PC, comparisons, branch polarity, eight
text traps, and the `@AddItem`/`@Done` labels. The four runtime service seams are the member list
(`0x21B68 → 0x10044 → 0x13004`, return `0x21B6E`), held-item count
(`0x21B82 → 0x8174 → 0x8BFA`, return `0x21B88`), equipment type
(`0x21BB0 → 0x8178 → 0x8C28`, return `0x21BB6`), and equippability
(`0x21BC4 → 0x81B4 → 0x8F80`, return `0x21BCA`).

The smallest meaningful fixture-owned admission cohort has five cases: recipient cancel, full inventory,
tool, equippable non-tool, and non-equippable. Runtime begins only at the source selection-loop label
`byte_21B58` (`0x21B58`), not at the handler entry or introductory text. Before the grouped launch, Python verifies
the immutable canonical ROM against its manifest and builds one ignored disposable session copy. It patches
exactly seven source/H1/ROM-bound six-byte spans in that copy: the four service calls above become actual
`JSR` (`4EB9`) calls to the generated work-RAM service stub at `0xFF6D00`, with their original return PCs;
the first excluded presentation instruction for recipient cancel `txt197` (`0x21B74`), full inventory
`txt208` (`0x21B94`), and non-equippable `txt167` (`0x21BD2`) become `JMP` (`4EF9`)
terminal-boundary shims to the generated result stub at `0xFF6D20`. The service stub supplies
only the declared `D0`, `D2`, or CCR result and `RTS`s to the original return PC; this is harness control,
not execution of an original helper. The observer contract only readbacks the already-patched session-ROM
spans and writes the generated work-RAM stubs. A terminal-boundary reach proves only the preceding original branch
reached that source boundary: the patched text instruction/body does not execute. The retained direct-v3
`@AddItem` entry `0x21BE4` is deliberately outside every session-ROM patch span. A deterministic shared-PC
dispatcher records the pre-commit tool/equippable `add-item-boundary` there, before any
`@AddItem` body is evidence. It then runs its already accepted direct-fulfillment block only as harness
cleanup, from the linked source-valid retained frame/order/inventory case, and returns through its established
stack seam to the generated pre-commit result before the optional-equip presentation branch. That cleanup
seam is independently guarded as the distinct direct block call `0x21C16 → 0x81B4 → 0x8F80`, whose
effective RTS is `0x8F9A` and original post-call return is `0x21C1C`; it is not the pre-commit admission
call at `0x21BC4`. The generated pre-commit `JSR` leaves its own return above that direct call return, so
the harness expects the cleanup RTS stack top at `0xFFFEF8` (eight bytes below its `0xFFFF00` setup top),
checks that exact original `0x21C1C` longword, and replaces only that longword with its generated result continuation.
Those cleanup callbacks are not pre-commit chronology or new commit/equip evidence.

One grouped BizHawk 2.11.1 / Genesis Plus GX launch observed all five source-bounded results with one
attempt each: recipient cancel reached `txt197`'s `0x21B74` boundary after the controlled member-list
return and cancel comparison/branch; full inventory reached `txt208`'s `0x21B94` boundary after the
member-list and held-item-count returns plus capacity comparison/branch; the tool and equippable non-tool
cases reached the neutral original `@AddItem` entry `0x21BE4` after their respectively applicable
equipment-type/equippability source branches; and non-equippable reached `txt167`'s `0x21BD2` boundary.
The observed output read back all four patched service calls, all three patched presentation boundaries,
and both generated work-RAM stubs. Its terminal status tail was
`transaction-state-restored` → `callbacks-cleared:0` → `observer-finished`; exact readback restored the
previously named gold, seed, order, flag, combatant-record, dialogue-name, selected-item, and submenu-action
state only.

Accordingly, controlled service results are harness inputs, not claims about original member-list, helper,
prompt, or UI behavior. Full-inventory retry/abort, non-equippable prompt accept/retry, all prompt/UI
execution, loop repetition, retry result, and `@Done` remain **Unknown** because `txt208` and `txt167`
precede their prompt calls. The H3 case and fulfillment-to-first-precommit transition watchdogs use a
180-frame budget solely as harness protocol, not original timing evidence. The canonical ROM remains immutable;
the disposable session copy is deleted after the
launch and is not a restored game state. The only newly claimed game-state readback/restoration is
`DIALOGUE_NAME_INDEX_1`'s word, `SELECTED_ITEM_INDEX`'s word, and `CURRENT_ITEM_SUBMENU_ACTION`'s byte;
generated probe/frame/stack/stub work-RAM is excluded. The direct v3 11-case helper/placement/`@AddItem`
evidence is byte-for-byte retained in v4, for 16 total cases. Reproduce the grouped rail with
`uv run sf2 h3 blacksmith-mithril --timeout-seconds 180`.

**Confirmed — post-`@AddItem` optional-equip H3 cohort.** The bounded continuation starts at the
equippability carry branch `0x21C1C` and ends at `@Done` `0x21CD4`. When carry is clear,
`bcc.w byte_21CD0` reaches the do-not-equip presentation boundary without a prompt. When carry is set,
the source prompt call is `0x21C24 → 0x10074 → 0x1528C`, with original caller return `0x21C2A`; controlled
prompt values are harness inputs only. An accepted controlled value continues through `GetEquipmentType`,
the weapon/ring split, `GetEquippedWeapon` or `GetEquippedRing`, cursed-aware unequip, held-item count,
and `EquipItemBySlot`. The source retains current-cursed `txt176` at `0x21C68`, newly-equipped-cursed
presentation after the music/wait path, noncursed `txt174` at `0x21CC8`, and do-not-equip `txt209` at
`0x21CD0`; the one accepted H3 launch stops at those neutral pre-presentation boundaries and executes
none of their presentation bodies.

The complete eight-by-four Mithril table has 32 choices and 26 distinct IDs. Its source/H1/ROM/table-owner
join shows all 26 item-definition rows have `ITEMTYPE_WEAPON`, with zero `ITEMTYPE_RING` and zero
`ITEMTYPE_CURSED` rows. Therefore the ring and newly-equipped-cursed source branches remain visible static
branches but are outside this Mithril-output runtime domain; this does not claim they are generally
impossible. One grouped BizHawk 2.11.1 / Genesis Plus GX launch ran 21 cases: the exact retained v4
16-record projection plus this compact five-case source-valid cohort. It confirmed, respectively: SNIP
receiving Levanter with carry clear reaches the do-not-equip boundary without a prompt; HERO receiving
Levanter with controlled prompt decline reaches that same boundary; HERO accepting with no equipped weapon
equips the new item and reaches the noncursed boundary; HERO accepting with an equipped uncursed Battle
Sword unequips it, equips the new item, and reaches that boundary; and HERO accepting while an equipped
cursed Dark Sword blocks replacement reaches the current-cursed boundary without a new equip. The last
case records `equipSlot = null`, `equipResult = null`, and status effects `0 → 4` before its boundary.

The accepted launch uses a private session-copy plan with 12 source/H1/ROM-bound non-overlapping spans: the
retained v4 seven six-byte service/terminal spans; the four-byte `0x21C20` `4E45 00AD → 6000 0002`
`BRA.W +2` (the word displacement is relative to the extension-word PC) to the preserved prompt JSR; the six-byte prompt JSR to generated
`0xFF6D40`; and three six-byte JMPs at `0x21C68`, `0x21CC8`, and `0x21CD0` to generated `0xFF6D60`.
The mixed-width plan rejects opcode, width, displacement, target, terminal, overlap, retained-v4-PC,
canonical-readback, and cleanup drift before any write; all 12 patched spans and generated stubs read back
during the accepted launch. It never writes the canonical ROM; the session copy was deleted afterwards.
One physical callback per PC dispatches deterministic roles, and callback/setup/loop/watchdog faults remove
output, clear callbacks, and emit one terminal structured nonzero-exit failure with case, phase, role,
expected/actual PC, and pending-state diagnostics. The successful terminal tail records
`callbacks-cleared:0`; the scoped gold, seed, order, flag, combatant-record, dialogue-name, selected-item,
and submenu-action restoration fields all read back true. The five v5 records are additive to the exact
16-record v4 projection, whose SHA-256 remains
`7F84BB2C8A1E7EF4079C527D672B9A33B60908B0A50A77C8880A632DA5DE5CC2`.
`UnequipItemBySlotIfNotCursed` (`0x8DB2`, `6000 FC1A`) and `EquipItemBySlot` (`0x8D66`,
`6000 FC66`) both tail-branch to `UpdateCombatantStats` (`0x89CE`) and share its RTS at `0x8A24`.
The observer therefore records exactly one source-helper-effective-return role there from the pending
call/target/return triple; it does not treat the shared PC as two calls or as evidence for an uncalled helper.
The no-equipped-weapon case records only `equip-effective-return`; the uncursed replacement case records
`unequip-effective-return → equip-effective-return`; and the cursed-Dark-Sword case records only
`unequip-effective-return` at `0x8A24`.

This does not claim BlacksmithMenu admission or natural story reachability, material/customer selection,
the original member-list/yes-no/helper service behavior, complete UI rendering/input timing, natural prompt
results or retry chronology, text traps, `WaitForVInt`, music/audio, persistence, caller continuation,
player input, audio/VDP/timing/rendering, RNG distribution, ring output, newly-equipped-cursed output, or
hardware behavior; those remain **Unknown**. The ring and newly-equipped-cursed source branches remain
statically excluded from the current Mithril-output domain rather than generally impossible. The grouped
rail confirms only the stated source-bounded decisions and mutations before presentation.

**Confirmed — fulfillment prompt-routing contract and one grouped H3 run.** The same pinned
`BlacksmithAction_FulfillOrder` source gives two adjacent, source-bounded `j_alt_YesNoPrompt` compare/
branch families without assigning a UI or input meaning to their results. At full inventory, the original
call at `0x21B98` returns to `cmpi.w #0,d0` at `0x21B9E`; `beq.s byte_21B58` at `0x21BA2` loops to the
selection label for controlled zero, while the nonzero fallthrough reaches `txt197` and then `@Done`
(`0x21CD4`). At non-equippable, the original call at `0x21BD6` returns to the `0x21BDC` compare; its
`bne.w byte_21B58` at `0x21BE0` loops for controlled nonzero, while controlled zero falls through to
the original `@AddItem` entry `0x21BE4`. Source, H1, ROM, original compare/branch bytes,
and the neutral `@Done` `movem.l (sp)+,d0-a1; rts` bytes (`4CDF03FF4E75`) are independently guarded.

Fixture v6 retains the accepted v5 21-case projection with SHA-256
`A2453765581CA1C8F6DC1D48D9DC1E8CFEB03CF0F6EC882485F3ADB157DA1D1E` and adds exactly four ordered
controlled-result cases: full-inventory zero retry to selection, full-inventory nonzero abort to
`@Done`, non-equippable zero accept to `@AddItem`, and non-equippable nonzero reselect to selection.
One BizHawk 2.11.1 / Genesis Plus GX launch completed the complete 25-case cohort (the retained 21
plus these four) with the exact accepted observation. Its session plan has 13 non-overlapping
source/H1/ROM-bound spans: four retained service JSR shims, the
recipient-cancel JMP, two prompt-boundary JSR shims, a four-byte full-inventory `BRA.W` from the abort
text boundary to the shared terminal/Done redirect, and four separately patched post-`@AddItem`
spans. The joint ten-byte `0x21CD0..0x21CD9` span replaces the overlapping retained do-not-equip
presentation boundary and the `@Done` bytes together: its six-byte JMP reaches a shared RAM dispatcher
and its remaining four bytes are readback-guarded unreachable `ILLEGAL` fill (`4AFC4AFC`). The dispatcher
uses the active deterministic observer mode to write its own next JMP only: accepted v5 do-not-equip
cases target the existing terminal stub, while the v6 abort targets the MOVEM stub. The generated result
stub is instruction-scoped: its `0xFF6D80` entry callback verifies the generated
`move.w #result,d0; jmp compare` bytes and its `0xFF6D84` JMP boundary observes the controlled `d0`
word after the MOVE and before the original compare. The abort route keeps the source `0x03FF`
`MOVEM.L` restore mask (ten registers, 40 bytes). The canonical ROM's six source bytes remain static-only
confirmation inside the jointly guarded ten-byte source span. The private session copy reaches a
dispatcher at `0xFF6D90`, which for the abort writes `jmp 0xFF6DA0`. That RAM stub executes the
source-equivalent `4CDF03FF` `MOVEM.L`, explicitly JMPs to the separate `0xFF6DB0` RAM `4E75` RTS stub,
and has distinct callbacks before the MOVEM and before the RTS. Those callbacks validate respectively
the pre-`MOVEM` synthetic stack seam and post-`MOVEM` `a7 = stack_top - dispatchStack.totalBytes + 40`
with the synthetic harness return. Runtime evidence therefore concerns the generated equivalent
instruction seams, not a claimed original-ROM `@Done+4` callback granularity. The accept route uses the already guarded
direct-`@AddItem` cleanup seam. The shared `0x21CD0` callback has both prompt-routing and retained-v5
equip-decision roles, but deterministic mode guards make the non-owner a no-op: prompt mode records only
the Done route and rewrites the dispatcher to the MOVEM stub; equip mode records only the retained
do-not-equip terminal and rewrites it to the existing terminal stub.

The successful run read back all 13 patched spans, both controlled prompt-boundary sites and the abort
skip, the controlled result stub, the Done redirect, the joint dispatcher write, and both generated
MOVEM/RTS stubs. It also read the synthetic stack before MOVEM and after MOVEM at the separate RTS
boundary, cleared all callbacks (`0`), and restored exactly `CURRENT_GOLD`, `RANDOM_SEED`, order words,
the flag-80 owning byte, and selected combatant records. The observer status ended with the ordered
prompt-routing and equip-decision transition milestones, restoration, `callbacks-cleared:0`, and
`observer-finished`; there was no Lua callback failure. This confirms the four controlled branch
results and the stated instrumented equivalent seams only. Original prompt/text/window/input behavior,
natural Yes/No meaning, timing, persistence, natural reachability, ring/newly-equipped-cursed output,
and every excluded post-`@AddItem` behavior remain **Unknown**.

**Confirmed — service-menu entry/return caller inventory and controlled return cohort.** The accepted
`sf2-common-menus-static-v1` owner establishes the built service-entry set. A separate, complete
source/H1/ROM inventory scans every comment-stripped direct `jsr`/`bsr` returning call and `jmp` tail
transfer to the four pinned jump aliases, retaining each opcode, six-byte instruction, alias and effective
target, and return semantics. It finds 69 direct transfers: 62 returning calls plus seven tail transfers;
Shop 33, Church 29, Caravan 5, and Blacksmith 2. The zero-inclusive caller-family × service × transfer
kind relation uses returning/tail counts: context-menu `1/0,1/0,0/0,1/0`, exploration-VInt
`0/0,1/0,1/0,0/0`, BattleTest `1/0,2/0,1/0,0/0`, and map/entity
`28/3,22/3,3/0,0/1` (Shop/Church/Caravan/Blacksmith order). Thus zeroes and tail transfers are part of
the static boundary rather than omitted or mislabeled call/return sites. The source aliases resolve as `j_ShopMenu → ShopMenu`
(`0x20000 → 0x20064`), `j_ChurchMenu → ChurchMenu` (`0x20004 → 0x20A02`),
`j_CaravanMenu → CaravanMenu` (`0x20010 → 0x21FD2`), and
`j_BlacksmithMenu → BlacksmithMenu` (`0x2000C → 0x21A3A`).

The compact runtime matrix contains the lowest-address direct caller in each positive family ×
service × transfer-kind cell—fourteen cases—and exactly one additional source-shaped case: BattleTest’s
second Church returning caller, whose surrounding `48E7FFFE` save and `4CDF7FFF` restore make its caller
stack frame distinct. The 15 ordered IDs are the prior twelve returning cases (context Church/Shop/
Blacksmith; exploration-VInt Church/Caravan; BattleTest Church main/Shop/Caravan/Church preserved-
registers; map/entity Church/Shop/Caravan) plus map/entity tail-transfer Church/Shop/Blacksmith. Tail
cases do not assert a post-JSR continuation: they observe the source `jmp` admission, outer service RTS,
and harness return instead. It does not claim that all same-family sites are dynamically equivalent or
naturally reachable; the remaining 54 sites remain statically covered only.

One grouped `sf2-service-menu-entry-return-v1` BizHawk 2.11.1 / Genesis Plus GX launch observed all 15
selected cases through their source returning call or tail transfer, alias-resolved outer entry, generated
controlled-return stub, generated outer-return trampoline, and generated result seam. Shop, Church, and
Caravan execute their source saved-frame, controlled `ExecuteDiamondMenu` cancel comparison, and epilogue
while all UI/prompt bodies are skipped.
Blacksmith executes its outer saved frame and a controlled `ProcessBlacksmithOrders` return, not the
action-selection or fulfillment body. The two generated RAM stubs are modelled before launch with exact
addresses, instruction widths, bytes, purposes, and cancel-result ABI: the Diamond path is `MOVEQ #-1,D0;
RTS` and the Blacksmith path is `RTS`. Lua writes and reads back both before the first case, and its
entry callbacks place their exact role and PC between service admission and the generated outer-return
trampoline. At service entry the observer first verifies the source-owned A7/top-longword seam, then
replaces only that RAM return word with the shared `0xFF6D30` six-byte `JMP` trampoline, whose exact
bytes and dynamic original target are written and read back per case. Its two mode-specific callback roles
(`outer-caller-return` and `outer-rts-harness-return`) validate the post-service-RTS A7 and readback before
the generated `JMP` transfers to the original source continuation or tail harness result; no callback at a
source return PC is claimed. The ordinary returning shape is entry `stackTop-4` then trampoline
`stackTop`; context adds the source `move.l a6,-(sp)` frame and is `stackTop-8` then `stackTop-4`; the
BattleTest saved-register case derives `d0-a6` as 15 registers/60 bytes from source, H1, and masks
`48E7FFFE`/`4CDF7FFF`, so it is `stackTop-64` then `stackTop-60`; and a tail transfer is `stackTop` then
`stackTop+4`. This establishes only that the controlled harness bypasses the selected
service bodies; it does not claim a natural transaction or mutation result. The 12 returning records
observed `caller-call-site → service-entry → generated-service-{cancel|blacksmith-return}-stub →
outer-caller-return → caller-result`; the three tail records observed `tail-transfer-site → service-entry
→ generated-service-{cancel|blacksmith-return}-stub → outer-rts-harness-return → caller-result`.
Every outer trampoline read back at `0xFF6D30` with its exact dynamic `JMP` target, including the
BattleTest `0xFFFEC0 → 0xFFFEC4` saved-register seam and the tail `0xFFFF00 → 0xFFFF04` harness seam.
All 17 session-ROM spans and both generated service stubs (`0xFF6D00` `70FF4E75`, `0xFF6D10` `4E75`)
read back exactly. Context-menu cases execute
the source `csc12_executeContextMenu` save/restore and compare sequence; the distinct BattleTest case
executes its original MOVEM save/restore. All 17 disposable session-ROM patches carry source/H1/ROM
original and replacement bytes, are non-overlapping, and are read back before the generated RAM harness
starts. The deterministic one-PC dispatcher reports terminal callback failure with the case plus
expected/actual call, target, return, generated-stub, stack, restoration, and ordered observed/expected
chronology/count state; accepted observation data is not passed to Lua. The Python verifier, after its
temporary-directory context closes, independently asserts that the exact session-ROM path no longer exists
on both success and failure. The successful
status tail was `service-menu-cases-entered → callbacks-cleared:0 → observer-finished`; callback count
was zero and current-portrait/caller-frame restoration read back true. This confirms only the named
controlled admission, bypass, return, restoration, and cleanup seams. Natural UI/input meaning,
transactions/economics, save persistence, Blacksmith order/fulfillment detail, text/window/portrait/
audio/VInt/DMA/timing, rendered output, and story reachability remain **Unknown**.

The interaction-level handoff is recorded in
[`service-interactions.md`](../design/contracts/service-interactions.md). It deliberately consumes only the
confirmed action ordering, cancellation boundary, and direct mutation calls; it is not a claim about
the original presentation or persistence lifecycle.

## Concentrated Runtime Queue

The pending service-entry/return rail above is deliberately narrower than the remaining service-state
questions. After its one grouped launch, the following separate queue must not be conflated with it:

- for shop/deals, church raise/cure/promotion/save, caravan depot transfer/drop, and blacksmith
  menu admission/order fulfillment, snapshot gold, party inventory, caravan storage, order storage,
  flags, and save state before/after both confirm and cancellation paths;
- vary map reload, save/reload, story flag 80, and blacksmith-ready conditions to distinguish
  per-visit stack state from persistent state;
- share VInt/VDP/audio observation points for window/portrait movement, prompt release behavior, and
  post-music continuation timing.

This queue leaves caller-dependent service admission/return intent **Inferred** and persistence,
window/portrait/audio/input timing, and final rendered composition **Unknown**. It starts no
emulator in this static slice.

## Church Raise Transaction Lifecycle

**Confirmed** — pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`churchactions_1.asm:ChurchMenu` / `@CheckRaiseAction` / `@CountDeadMembers_Loop` /
`@ConfirmRaise` / `@CheckRaiseCost` / `@DoRaise` / `@RaiseNextMember`, with H1 and ROM joins at
`ChurchMenu` `0x20A02` and the original Raise route `0x20A64`, define the bounded Raise lifecycle.
The normal menu path is `ChurchMenu → ExecuteDiamondMenu → bra @CheckRaiseAction`; the grouped
runtime fixture controls only the action-selection and Yes/No service seams and callback-observes
both original PCs for every case. It does not enter a local Church label directly.

`Church_GetCurrentForceMemberInfo` builds the source list and seeds `d7 = TARGETS_LIST_LENGTH - 1`.
The original `dbf d7,@CountDeadMembers_Loop` therefore visits members in list order. `j_GetCurrentHp`
returns current HP in `d1`; `tst.w d1; bhi @RaiseNextMember` skips living members, while zero HP
increments `deadMembersCount`. For each dead member, the cost is `GetLevel × 10`; the original
promotion lookup leaves `cannotPromoteFlag` clear for a regular-base class and adds `200` when it is
set. The original `cmp.l d0,d1; bcc @DoRaise` admits equal gold as well as greater gold. The prompt
accept value is zero; nonzero continues the member loop without mutation.

The seven-case `sf2-church-raise-lifecycle-runtime-v1` observation confirms all-alive no prompt,
decline, insufficient-gold, equality success for regular and promoted members, promoted one-below,
and a mixed alive/decline/success order. On each success it callback-observes the original helper
chronology `j_DecreaseGold/DecreaseGold → j_IncreaseCurrentHp/IncreaseCurrentHp` with
`d1 = CHAR_STATCAP_HP (200) → UpdateAllyMapsprite`; a helper entry in a non-success state is a
terminal structured observer failure, not an absence-only assertion. The harness restores only
current gold, touched full combatant records and mapsprite bytes, dialogue name/number scratch,
`TARGETS_LIST_LENGTH`, the three-byte maximum touched `TARGETS_LIST` span, and the read-only
service-entry owner’s current-portrait word, plus generated harness/action/prompt spans and the
18-byte terminal trampoline. That terminal executes real `MOVEA.L` restoration of the captured
CheckSram A6/A7 frame before its callback readback; failures report the bootstrap-frame mismatch
instead of claiming a register write. The static contract source/H1/ROM-derives each observed jump-interface target and
helper RTS return, and guards every session patch against its original bytes before producing a
disposable ROM; Python verifies deletion of that session artifact rather than having Lua claim it.
Presentation, persistence, economic balance meaning, and all-RAM restoration remain **Unknown**.

## Church Cure Transaction Lifecycle

**Confirmed** — the separate `sf2-church-cure-lifecycle-runtime-v1` H3 rail enters original
`ChurchMenu` at `0x20A02` and observes original `@CheckCureAction` at `0x20B58` for every one of
its eleven ordered records. Its pinned evidence is SF2DISASM `master`
`c834c652b6862bc5679fd7f69a38a7093206efc6`, H1, and the canonical USA ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.

The source execution order is poison, then `Church_CureStun`, then curse. Poison uses
`STATUSEFFECT_POISON` (`0x0002`) and `CHURCHMENU_CURE_POISON_COST` (`10`); stun uses
`STATUSEFFECT_STUN` (`0x0001`) and `CHURCHMENU_CURE_STUN_COST` (`20`); curse uses
`STATUSEFFECT_CURSE` (`0x0004`). Each member loop is source-ordered by `dbf`, and the curse
item loop is an independent `dbf d6` scan. The source comparisons are `cmp.l d0,d1; bcc`, so
equal gold is admitted. Controlled zero prompt result reaches the cost test; the nonzero fixture
result takes the no-mutation branch. These are source-bounded outcomes, not a claim about UI input
meaning.

The curse cases use the source/H1/ROM-joined Dark Sword definition: item `70` (`ITEM_DARK_SWORD`),
price `17000`, cursed type bit, and `lsr.w #2` cost derivation of `4250`. The final case starts with
gold `4280` and observes the three successful commits in source order—poison `10`, stun `20`, then
curse `4250`—ending at zero.

Success callbacks observe original helper seams. Poison and stun each record
`j_DecreaseGold → DecreaseGold → j_SetStatusEffects → SetStatusEffects`; curse records
`j_DecreaseGold → DecreaseGold → j_UnequipAllItemsIfNotCursed → UnequipAllItemsIfNotCursed →`
the `UpdateCombatantStats` tail/RTS seam. A helper callback while no transaction is pending is a
terminal failure, including negative, decline, and insufficient-gold records. The private session
copy guards source/H1/canonical original bytes, readbacks its applied spans, and is deleted by
Python. The observer snapshots/restores current gold, the touched complete combatant record,
`TARGETS_LIST_LENGTH` and touched byte, dialogue scratch, current portrait, generated harness spans,
and bootstrap A6/A7 stack state. Its terminal 68K trampoline restores A6/A7 before callback readback;
it does not use same-callback register-setting assumptions. One dispatcher owns every shared PC;
callback/setup/watchdog faults remove output, clear callbacks, record case/family/call/target/return
diagnostics, and produce a nonzero status result.

Normal story reachability and user-input interpretation, text/window/portrait/music/audio/VInt/VDP/DMA
and frame timing, economic meaning, save/load/SRAM/map reload/cross-process persistence, Church
Raise/Promotion/Save, other statuses, inventory-capacity/equipment-selection UI, rendered outcomes,
and remake choices remain **Unknown**.

## Church Save Lifecycle

**Confirmed — bounded Church Save H3 seam.** The five-case
`sf2-church-save-lifecycle-runtime-v1` fixture enters original `ChurchMenu` at `0x20A02` and
observes original `@StartSave` at `0x20FCC` in every case. Source/H1/ROM guards retain selector
comparisons `0`, `1`, and `2` for Raise/Cure/Promotion before action `3` reaches Save; the first
`j_alt_YesNoPrompt` at `0x20FD0` accepts `d0 = 0` through the `0x20FD6` compare and `0x20FDA`
branch, while nonzero follows `@ExitSave` at `0x21028`. Accepted cases copy `CURRENT_MAP` to
`EGRESS_MAP` at `0x20FE6`, load `CURRENT_SAVE_SLOT` at `0x20FEC`, execute the flag-399 trap and
operand at `0x20FF0`/`0x20FF2`, and call original `SaveGame` at `0x20FF4 → 0x6F6A`. Its slot
branches converge at source `@Continue`, whose H1/ROM `rts` at `0x6FAA` is observed before the
original call-site return at `0x20FF8`.

The matrix covers initial decline without SaveGame/Fade/Witch callbacks; slot 1 map 0, slot 2 map
78, and pre-set flag-399 saves that continue through `@ExitMenu`; plus slot 2 save/rest that reaches
the original Fade call/entry/controlled return and then the `WitchSuspend` entry-only tail target
`0x21020 → 0x7034`. Selector zero selects Save slot 1 and nonzero selects slot 2. The 0–78 map
values are the independently validated H2 `sf2-map-content-static-v1` 79-map owner domain, not
Church range checking. `SaveGame`'s
`SAVE_SLOT_REAL_SIZE = 4016` loop stores 4,016 actual SRAM bytes over an 8,032-byte interleaved
address interval; it does not establish 8,032 stored bytes or cross-process durability.

The observer snapshots and readbacks only the touched map/Egress/slot/flag byte, selected slot's
4,016 physical SRAM bytes, checksum, `SAVE_FLAGS`, dialogue/portrait scratch, generated spans, and
bootstrap frame. It uses one deterministic shared-PC dispatcher, reports callback failures through a
structured nonzero status, and deletes rejected output. **Unknown:** ordinary UI/audio/fade/suspend
timing and presentation, normal-story reachability, cross-process or torn-save persistence, invalid
selector behavior, SaveGame payload meaning, and broader WitchSuspend behavior.

## Reproduction

```powershell
uv run sf2 h2 common-menus
uv run sf2 h2 portraits
uv run sf2 h2 ui-graphics
uv run sf2 h2 icon-graphics
uv run sf2 h2 ui-layouts
uv run sf2 h2 item-auxiliary
uv run sf2 h3 blacksmith-mithril --timeout-seconds 180
uv run sf2 h3 service-menu-lifecycle --timeout-seconds 180
uv run sf2 h3 church-raise-lifecycle --timeout-seconds 180
uv run sf2 h3 church-cure-lifecycle --timeout-seconds 180
uv run sf2 h3 church-save-lifecycle --timeout-seconds 180
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/common-menus-static.json`,
`local/derived/ui-graphics-decode.json`, `local/derived/icon-graphics-static.json`, and
`local/derived/ui-layout-static.json`.
