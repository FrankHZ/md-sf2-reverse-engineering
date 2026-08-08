# Caravan and Deals State Contract

- Status: **Confirmed static storage and boundary behavior for Caravan and Deals state**
- Evidence date: 2026-08-08
- Scope: implementation-neutral representation of the accepted Caravan item-normalization/compaction
  boundary and packed Deals count boundary, without assigning service, runtime, persistence,
  presentation, or balance meaning

## Judgment Boundary

This contract defines two low-level item-state stores. It does not define how an item is acquired,
sold, dropped, equipped, displayed, saved, or valued.

- **Confirmed**: Caravan additions strip item status bits; an add against a full Caravan is ignored;
  Caravan removal compacts the list and writes `ITEM_NOTHING` at the tail; Deals stores two four-bit
  counts per byte; Deals additions saturate; and Deals removal at zero is ignored.
- **Inferred**: none. Service intent and caller-visible behavior are deliberately not inferred from
  the storage helpers.
- **Separate-owner Confirmed**: the same accepted common-stats owner confirms
  `newGame.clearsFlagsDealsAndCaravan=true`, the source-static `NewGame` clear route for flags, Deals,
  and Caravan. That adjacent ordering/integration fact remains with `stats.new-game` and is not
  consumed or associated by this contract.
- **Unknown**: the numeric item-status mask; Caravan capacity; the Deals item domain, byte count, and
  item-to-packed-field mapping; numeric return values and condition-code use; natural/runtime caller
  reachability; transaction ordering or atomicity; caller-visible/runtime reset outcomes; save/load
  persistence; menus, messages, and other presentation; and economy or balance intent.

The [item-definition-data contract](item-definition-data.md) owns item identities and fixed data, not
mutable ownership. The [service-interactions contract](service-interactions.md) owns bounded
source-static shop, Caravan, church, and blacksmith action order, not the final state of these stores.
The [combat-resolution contract](combat-resolution.md) owns its accepted battle-drop and Deals-routing
subset. The [save-system contract](save-system.md) owns accepted save structures and action replay,
not proof that every Caravan or Deals mutation survives every lifecycle path.

## Evidence Owner

`sf2-common-stats-static-v1`
([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) is the sole
executable owner consumed by this contract. Its verifier is
[`stats.py`](../../../src/sf2tool/h2/stats.py), and its source-backed explanation is
[Common Stats and Inventory Services](../../research/common-stats.md).

The fixture owns the six accepted `expected.statsFacts.inventories` facts and binds the representative
`AddItemToCaravan` address `39484` and `GetDealsItemAmount` address `39390`. The owner prose supplies
the explicit two-four-bit-count wording and the symbolic `ITEM_NOTHING` tail identity. These are
static storage facts, not runtime observations through a menu, battle reward, save, or new-game path.

The common-scripting and battle-functions aggregates are deliberately excluded. The sibling party,
item-inventory, name/item/spell lookup, new-game, and unused records in the common-stats owner are also
outside this contract's research-index boundary. Excluding `stats.new-game` preserves its accepted
static clear fact as separate-owner evidence; it does not make that fact Unknown.

## Caravan Storage Boundary

**Confirmed static:** an item entering the Caravan add helper has its status bits stripped before the
stored item identity is used. The accepted owner does not expose the numeric mask through this
contract. A compatibility adapter must therefore preserve both the caller-supplied value and the
normalized stored identity without inventing the mask or assigning meaning to the removed bits.

An add at the full boundary is ignored. This is a low-level storage outcome only: the evidence does
not define a return code, caller retry, message, alternate destination, refund, or rollback.

Removal preserves an ordered compact list: the remaining entries are compacted and the vacated tail
is written with symbolic `ITEM_NOTHING`. The accepted boundary does not provide a numeric capacity,
the numeric value of `ITEM_NOTHING`, or a caller-visible selected-slot contract. A remake must keep
compaction and tail clearing distinguishable from unordered-set deletion.

| Caravan surface | Accepted contract | Deliberate boundary |
| --- | --- | --- |
| add normalization | strip item status bits before storage | numeric mask and removed-bit meanings are **Unknown** |
| full-boundary add | ignore the add | capacity, return value, message, and fallback route are **Unknown** |
| removal | compact remaining entries | selection provenance and caller-visible ordering are **Unknown** |
| vacated tail | write symbolic `ITEM_NOTHING` | numeric sentinel and total slot count are **Unknown** |

## Deals Packed-Count Boundary

**Confirmed static:** Deals stores two four-bit counts in each byte. The two counts must remain
independently addressable logical fields even if a modern implementation does not use packed bytes.
This contract does not assign an item-ID range, array length, or item-to-field mapping.

An addition at the accepted upper boundary saturates. A removal at zero is ignored. These are static
helper behaviors. The accepted owner does not establish which callers reach either boundary at
runtime, what they return, or whether an enclosing transaction succeeds.

| Deals surface | Accepted contract | Deliberate boundary |
| --- | --- | --- |
| storage packing | two four-bit counts per byte | byte count, item domain, and field mapping are **Unknown** |
| addition boundary | saturate the selected count | caller-visible success and surrounding transaction are **Unknown** |
| zero removal | ignore the removal | return value, message, and retry policy are **Unknown** |
| neighboring count | remain a separate packed field | no item identity or economic relation is implied |

## Cross-System Separation

The low-level stores do not form an economy by themselves:

- item definitions determine which identities and flags exist, not who owns an item;
- service menus may call storage helpers in a source-static order, but their final state and
  cancellation behavior are not runtime-closed here;
- battle-drop evidence owns only its observed recipient and Deals route, not every producer;
- member-held inventory and equipment are separate stores;
- the accepted source-static `NewGame` clear route and ordering/integration remain with their separate
  owner and are not consumed here; caller-visible/runtime reset outcome and persistence are
  **Unknown**;
- UI, dialogue, audio, replacement assets, pricing, rarity, and balance require their own contracts
  or explicit modern design decisions.

No caller path should be promoted into this contract merely because it shares an item identity or a
storage helper name.

## Implementation-Neutral State Model

```text
CaravanStore
  orderedEntries[]
  tailEmptyIdentity: ITEM_NOTHING

CaravanAddOperation
  rawItemValue
  normalizedStoredIdentity
  admissionBoundary: available | full
  fullBoundaryOutcome: ignore

CaravanRemoveOperation
  selectedStoredEntry
  compactRemainingEntries
  writeTailIdentity: ITEM_NOTHING

DealsStore
  packedBytes[]
  countFieldsPerByte: 2
  countFieldWidthBits: 4

DealsMutation
  kind: add | remove
  selectedCountField
  addUpperBoundary: saturate
  removeZeroBoundary: ignore
```

This is a logical parity model, not a required engine memory layout. Array lengths, numeric masks,
sentinel values, item-domain mappings, and caller return shapes are intentionally absent because the
accepted contract does not close them.

A modern engine may store Caravan entries as typed item identities and Deals counts as ordinary
integers. Its compatibility adapter must still reproduce status-bit normalization, ordered compaction,
tail clearing, independent four-bit-field boundaries, saturation, and zero-removal behavior.

## Original Fidelity and Modernization

Original-fidelity mode preserves the six accepted storage facts and the representative owner
identities/addresses. It reports unknown capacity, mapping, caller, and persistence behavior instead
of filling them with assumptions from higher-level menus or economy design.

A modern remake may choose larger storage, explicit result types, unpacked counters, transactional
service commands, event logs, clearer failure messages, or different economy balance. Those choices
are deliberate deviations unless an adapter reproduces the accepted original-facing boundary.

Original item names, descriptions, and other copyrighted content are not needed for this public
contract. Public parity data should retain structural metadata and synthetic values only.

## H4 Acceptance Gates

A future remake Caravan/Deals adapter passes this contract only when:

1. Caravan add preserves the caller-supplied item value separately from the status-stripped stored
   identity without inventing a contract-level numeric mask;
2. a full-boundary Caravan add is ignored without assigning an unconfirmed return, message, or
   fallback destination;
3. Caravan removal preserves ordered compaction and writes symbolic `ITEM_NOTHING` at the vacated
   tail;
4. each Deals storage byte exposes two independent four-bit count fields without inventing an item
   domain or field mapping;
5. Deals addition saturates its selected field and zero removal is ignored;
6. the separately accepted source-static `NewGame` clear ordering/integration remains traced to
   `stats.new-game` and outside this contract, while runtime reachability, service/drop integration,
   caller-visible reset outcomes, persistence, UI, and balance remain separately tested or explicitly
   **Unknown**;
7. public fixtures and tests use structural metadata and synthetic values rather than copyrighted
   item content.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| status-bit stripping and full-add ignore | **Confirmed static** | `sf2-common-stats-static-v1` ([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) | Numeric mask, capacity, return/caller outcomes |
| Caravan compaction and `ITEM_NOTHING` tail write | **Confirmed static** | `sf2-common-stats-static-v1` plus [owner prose](../../research/common-stats.md) | Numeric sentinel, selected-slot ABI, runtime result |
| two four-bit Deals counts per byte | **Confirmed static** | `sf2-common-stats-static-v1` plus [owner prose](../../research/common-stats.md) | Item domain, total bytes, item-to-field mapping |
| Deals addition saturation and zero-removal ignore | **Confirmed static** | `sf2-common-stats-static-v1` ([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) | Runtime reachability, return values, transaction outcome |
| source-static `NewGame` clear of flags, Deals, and Caravan | **Separate-owner Confirmed static** | `sf2-common-stats-static-v1`; `stats.new-game` remains excluded | Ordering/integration is not consumed here; runtime reset outcome and persistence remain **Unknown** |
| service/drop/save/UI/balance and caller-visible reset semantics | **Separate owner / Unknown** | Adjacent contracts and future runtime/synthesis work | Do not infer higher-level behavior from storage helpers |

## Reproduction

```powershell
uv run sf2 h2 common-stats
uv run sf2 design-contracts test
uv run sf2 verify
```
