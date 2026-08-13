# Service Interactions

- **Confirmed original behavior:** static action ordering, cancellation and direct resource-helper
  boundaries for the four service surfaces described below, plus the bounded Church Raise H3
  admission/commit seam and the bounded Church Cure transaction seam.
- **Unknown original behavior:** caller admission/return effects, persistence across map/save reload,
  input/audio/window/portrait timing, and final presentation composition.
- Remake status: implementation-neutral evidence-bound contract; Church Raise and Cure have bounded
  runtime seams while broader service lifecycles remain incomplete.
- Evidence date: 2026-08-13
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- Traceability: `sf2-common-menus-static-v1` in
  `tests/fixtures/h2/common-menus-static-v1.json`; `sf2-church-raise-lifecycle-runtime-v1` in
  `tests/fixtures/h3/church-raise-lifecycle-v1.json`; `sf2-church-cure-lifecycle-runtime-v1` in
  `tests/fixtures/h3/church-cure-lifecycle-v1.json`; `src/sf2tool/h2/menus.py`,
  `src/sf2tool/h3/church_raise_lifecycle.py`, `src/sf2tool/h3/church_cure_lifecycle.py`; and
  `docs/research/common-menus.md`.

## Confirmed Interaction Contract

The service layer exposes four static action surfaces. A remake-facing implementation may model
their selection order and direct resource effects without copying original presentation assets or
timing.

| Surface | Ordered actions | Confirmed static boundary |
| --- | --- | --- |
| Shop | buy, sell, repair, deals | Buy/deals remove gold before granting an item; a deals purchase also removes its deals entry. Selling grants gold, removes the member-held item, and routes rare items to deals. Repair removes gold and repairs the selected item slot. |
| Church | raise, cure, promote, save | The H2 source boundary preserves Raise's mutation-call order `j_DecreaseGold` → `j_IncreaseCurrentHp` (after `move.w #CHAR_STATCAP_HP,d1`) → `UpdateAllyMapsprite`; source alone did not establish the final current-HP value. The bounded H3 Raise seam now observes accepted cases clamped to `min(hpMax, 200)` and the mapsprite helper completion. Caller-visible behavior beyond that seam remains **Unknown**. Cure replaces status bits after payment; promotion is data/member-gated before class change/promotion; save reaches the save operation. |
| Caravan | join, depot, item, purge | The top, depot, and item selectors are source-ordered word-relative tables. Deposit calls storage-add before member-slot drop; derive and give retain distinct normal/exchange call sequences; rare-drop branches can call the deals helper. |
| Blacksmith | fulfill ready orders, then place pending order | No diamond menu. Static source preserves ready/pending counting, fulfillment storage-clear/add/equip order, placement gates and payment/drop/pick/flag order, plus the bounded weapon-row picker. |

Shop, church, caravan, depot, and item surfaces cancel through the common diamond/selection boundary;
the shared selection screen has source-shaped entry, navigation, selection, resource-load, and cleanup
records; its B→C→A test/result order remains a source-level fact rather than a lifecycle guarantee. Shop
and caravan loop back to
their action menus after non-exit actions. The blacksmith sequence is visit-driven rather than a
diamond-menu loop.

### Shop source boundary

The Shop design contract consumes only the **Confirmed** static source boundary in
`sf2-common-menus-static-v1`: Buy and Deals invoke decrease-gold then add-item; Deals then invokes
remove-item-from-deals; Sell invokes increase-gold then drop-item and conditionally reaches the
rare-item Deals helper; Repair invokes decrease-gold then repair-item-by-slot. Its source parser also
pins selector comparisons, price arithmetic inputs, capacity/type/broken-item guards, list routing,
and structured source instruction order. It retains jump-interface caller identity separately from
the effective Shop/selection target. It does not turn the observed helper names, branch labels, or
static input bits into a claim about player-visible timing, caller return state, persistence, or
presentation.

### Church source boundary

The H2 Church design contract consumes the **Confirmed** static route boundary:
`routeDerived.raise.mutationCalls` preserves the source-derived mutation-call order `j_DecreaseGold` →
`j_IncreaseCurrentHp` (after `move.w #CHAR_STATCAP_HP,d1`) → `UpdateAllyMapsprite`. This filtered
mutation-helper order does not assert that no other calls intervene; H2 alone did not establish the final
current-HP value. The bounded **Confirmed** `sf2-church-raise-lifecycle-runtime-v1` H3 seam instead
observes each accepted affordable Raise to clamp current HP to `min(hpMax, 200)` and complete the original
`UpdateAllyMapsprite` helper. Caller-visible continuation, presentation, and persistence remain **Unknown**.
Cure's source/H1/ROM-bounded runtime seam confirms poison → stun → curse status/equipment commits,
including equality-inclusive affordability and the Dark Sword `17000 >> 2 = 4250` cost; broader Cure
admission, UI, persistence, and presentation remain **Unknown**. Promote
preserves its level/data gates and class-then-promotion call order; Save reaches its named save call and
records its separate suspend branch. The contract does not treat selector values, helper names, status
masks, or jump-interface callers as a runtime promise about service admission, persistence, prompt
timing, or rendered presentation.

### Caravan source boundary

The Caravan design contract consumes only the **Confirmed** static source boundary in
`sf2-common-menus-static-v1`: the top Join/Depot/Item/Purge table and its two nested tables preserve
their source order, word selector scaling, cancel branches, and action-loop branches. Join/Purge retain
their named party-helper call order; Depot retains the parsed 64-item storage guard, the four-slot
recipient guard, normal and exchange transfer call order, and separately guarded rare/unsellable
paths. Item Use and Give retain their source call sequences; Equip is modeled only as the named
selection-action handoff, not as a proven equipment lifecycle. Physical ROM spans, word table widths,
item-definition offsets, capacities, and loop counters remain distinct contract fields. Alias-resolved
direct callers record instruction spelling and effective target, but do not imply runtime admission,
return state, persistence, timing, or presentation.

### Blacksmith source boundary

The Blacksmith design contract consumes only the **Confirmed** static source boundary: a 24-byte local
frame clears its four named counters before processing; byte force copying and the paired literal-80
check/clear sites are separately recorded, including the `TARGETS_LIST_LENGTH` `d7` counter-source
load. Fulfillment retains recipient cancellation/capacity/equipment
branches and its add-item, word order-storage clear, count increment, optional-equip sequence. Placement
keeps the source-ordered material/customer cancellation, mithril, promotion/class, confirmation, and gold
gates, then its decrease-gold, drop-by-slot, picker, literal-80 load, flag-clear order; the max-order
comparison is a post-placement continuation branch. The picker retains source-shaped prefixed-class
scanning, initial-row and BRN/RDBN fallback branch, parameter-to-RNG-range/result loop, item-auxiliary
parameter denominators, and two-byte slot search/write loop—not a runtime promise about RNG
distribution, persistence, prompt meaning, caller admission, or presentation.

## Boundaries for a Future Remake

This contract does not establish which maps/NPCs admit each service, whether cancellation has
caller-visible side effects, or when a service returns to exploration. It also does not specify
save/reload persistence for orders, deals, caravan storage, or story flags; input repeat behavior;
window/portrait/audio timing; or final visual composition. Those remain original-runtime questions,
not remake defaults.

Future H4 tests should consume the `serviceStateMachines` fixture object for static action ordering
and direct effect expectations, then add separate behavioral fixtures only after the grouped H3
service matrix resolves those unknowns.

## Church Raise runtime seam

The confirmed `sf2-church-raise-lifecycle-runtime-v1` fixture
`tests/fixtures/h3/church-raise-lifecycle-v1.json` narrows only Raise admission and
commit: a normal `ChurchMenu` entry reaches Raise, living members are skipped, each dead member is
processed in list order, and the original equality-inclusive affordability comparison admits
`level × 10` plus the promoted `200` operand. An accepted, affordable member observes original
`DecreaseGold`, `IncreaseCurrentHp` with HP-cap `200`, then `UpdateAllyMapsprite`; decline and
insufficient-gold paths have no such helper entry. This is an implementation-neutral ordering and
mutation boundary. Observer-scoped restoration covers the selected force-list length and bytes,
current-portrait word, generated harness/action/prompt spans, and an 18-byte terminal trampoline.
That trampoline executes real `MOVEA.L` restoration of the captured CheckSram frame before callback
readback; failure diagnostics report an un-restored bootstrap frame rather than claim a register write.
Python verifies disposal of the patched session ROM. It is not a presentation, persistence,
economy-balance, or all-memory-restoration rule.

## Church Cure runtime seam

The confirmed `sf2-church-cure-lifecycle-runtime-v1` fixture
`tests/fixtures/h3/church-cure-lifecycle-v1.json` narrows only the original Church Cure transaction
seam. It retains source order poison → stun → curse, each source `dbf` member/item iteration, zero
controlled prompt result to the equality-inclusive `cmp.l d0,d1; bcc` affordability path, and
poison/stun/curse costs of `10`/`20`/Dark-Sword-`17000 >> 2 = 4250`. The complete eleven-record
cohort includes no-status, decline, one-below, exact-cost, and the ordered `4280 → 4270 → 4250 → 0`
success sequence. Poison/stun commits observe original `DecreaseGold` followed by `SetStatusEffects`;
curse observes `DecreaseGold`, `UnequipAllItemsIfNotCursed`, and its `UpdateCombatantStats` tail/RTS
completion. This is an implementation-neutral resource/status/equipment ordering boundary. It is not
a rule for text, prompt UI semantics, persistence, economy balance, service admission, or rendered
presentation. Observer restoration is scoped to the touched gold, complete combatant record,
target-list length/byte, dialogue scratch, portrait scratch, generated RAM, and bootstrap frame.
