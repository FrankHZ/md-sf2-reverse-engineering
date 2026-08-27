# Map-Event Item Transactions

- Status: **Confirmed** for static caller choreography and the FieldMenu/map-setup return seam
- Status: **Unknown** for all runtime inventory, result, mutation, reachability, persistence, and presentation meaning
- Evidence date: 2026-08-26
- ROM: USA retail, SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Confirmed Static Caller Surface

`uv run sf2 h2 map-event-item-transactions` derives fixture
`sf2-map-event-item-transactions-static-v1` from 16 named source identities, accepted map-event
program topology, the deterministic H1 listing, and the pinned ROM. It retains eight contextual
programs from seven event-table sources: Map 6 EntityEvent13 and its distinct
`Map6_DefaultEntityEvent` shared tail, Map 9 EntityEvent0, Map 63 EntityEvent0, Map 72 ZoneEvent3,
and the Map 8, 22, and 63 item events. The contextual corpus is 708 bytes, 190 operations, and 42
labels; seven physical interval unions are 558 bytes, 150 operations, and 34 labels. The Map 6 tail
is one nested/shared physical range, not a second physical program.

The 150 physical operations split into 53 event-service macros, 37 raw instructions, and 60 raw
control-flow operations: 90 ordinary, 20 conditional branch, 12 unconditional branch, 20 direct
call, and eight return operations. The fixture anchors every physical PC plus seven table entries,
six service instruction/effective-entry seams, and four FieldMenu/map-setup seams (167 anchors).

The eight contextual ordered service chains are: Map 6 EntityEvent13 location → mandatory receive;
the Map 6 default-tail context repeats that physical tail; Map 9 location; Map 63 entity location →
remove-by-item; Map 72 location → location → remove-by-item → remove-by-item; Map 8 item
remove-by-item; Map 22 item location → remove-by-slot; and Map 63 item remove-by-item. These are 15
contextual / 13 physical service calls across six unique ordered shapes: six / one
`GetItemInventoryLocation`, two / one `ReceiveMandatoryItem`, one / one `RemoveItemBySlot`, and five
/ five `RemoveItemFromInventory` calls. The aliases are retained as caller-target facts:
`j_GetItemInventoryLocation` `0x81D0` → `GetItemInventoryLocation` `0x9146`, and
`j_RemoveItemBySlot` `0x819C` → `RemoveItemBySlot` `0x8E76`; the two remaining effective entries are
`0x4F4AA` and `0x4F566`.

Six item setup constants are source-enum joined exactly: Achilles Sword `$3D`, Wooden Panel `$70`,
Cannon `$72`, Dynamite `$74`, Arm of Golem `$75`, and Cotton Balloon `$7D`. The retained predicate
join consists of seven contextual / six physical inventory-location `cmpi #-1,d0` branch pairs and
two / one mandatory-receive `btst #0,d0` branch pair. This records the source-shaped sentinel, bit,
branch opcode/polarity, target, and fallthrough only; it does not assert a runtime predicate value.

## Item-Event Return Handoff

The three item-event contexts contain exactly five `d6` writes: Map 22 `0x5962A` `moveq #-1,d6`; Map
63 `0x5CA96` `move.w #-1,d6` and `0x5CA9C` `clr.w d6`; and Map 8 `0x56328` `moveq #0,d6` and
`0x5633E` `move.w #-1,d6`. FieldMenu calls `j_RunMapSetupItemEvent` at `0x2152A`, whose interface
entry is `0x44088` and map-setup entry is `0x47586`; FieldMenu next tests `tst.w d6` at `0x21530`.
These are static caller/return seams only. The fixture does not claim an acquired/consumed item, a
selected event, or a caller-visible `d6` value.

## Ownership and Unknown Boundary

This owner joins accepted map events; direct state/control/handoff; predicate results;
dialogue/request/interaction projections where they overlap; common stats/item inventory/item stats;
map setup; technical interfaces; and FieldMenu control. It neither enters a service body nor republishes
a callee algorithm or golden result. The canonical service-body owner is
`code/common/stats/iteminventory.asm`; its alternate
`code/common/stats/items/itemfunctions_s7_0.asm` is deliberately excluded.

The fixture's 14-field grouped **Unknown** register keeps natural reachability, selected context,
caller state, inventory contents, all service results and mutations, predicate values/branches,
caller `d6`, item acquisition/consumption, flags/map-script effects, persistence, and input/dialogue/
audio/presentation/story timing outside this static contract. No H3 observation or scenario claim is
introduced here.

## Reproduction

```powershell
uv run sf2 h2 map-event-item-transactions
uv run pytest -q tests/python/test_map_event_item_transactions.py
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/map-event-item-transactions-static.json`.
