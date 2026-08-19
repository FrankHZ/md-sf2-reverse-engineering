# Map 3 to Battle 01 Research Gap Audit

- Status: **OPEN** — durable research-owned gap register; not a readiness report and not a gap-closure
  claim
- Audit date: 2026-08-19
- Audit base: `main` commit `9a7cbcb44322e309ef10d8afac76d9a98be76f98`, tree
  `28c5f9c00a2b095d8b990eb8adc5249ede911704`
- ROM: USA retail, SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- Milestone owner: [ADR 0009](../decisions/0009-first-phase4-playable-slice.md)
- Scope: ADR 0009 pre-entry gate item 1 — the Research lane independently inventories the accepted
  evidence needed for the complete Map 3 through Battle 01-completion scenario, records every evidence
  gap in this durable research-owned artifact, and identifies which gaps must be closed for
  implementation-neutral fidelity.

## Judgment Boundary

This audit is the research-owned counterpart of the Layer B
[Map 3 to Battle 01 Readiness Ledger](../design/synthesis/map3-battle01-readiness.md). It is
written independently from that ledger's closure classification: it inventories the accepted evidence
surface and records gaps using the research evidence labels **Confirmed**, **Inferred**, and
**Unknown**, with named owners, fixtures, and reproduction commands. It does not:

- define the detailed route, starting state, completion endpoint, save scope, assets, or acceptance
  tier (those are audit outputs of the Design lane and explicit product decisions, not recoverable
  original-game facts);
- bulk-associate the 26 aggregate Map 3 index records with any future scenario contract;
- promote a controlled helper or debug seam into a natural-story route;
- treat a static source graph as an observed chronological playthrough;
- weaken any existing fixture, contract, or evidence label;
- change the readiness ledger's **NOT READY** state or authorize Phase 4.

The following normative distinctions are retained exactly as the readiness ledger states them: a
controlled helper or debug seam is not a natural-story route; a static source graph is not an
observed chronological playthrough; a fixture-local H4 surface is not an end-to-end scenario golden;
and an indexed file is not automatically a future design association.

Only accepted `main` evidence contributes. Unmerged branches (including this topic branch before
integration) are collaboration state, not evidence.

## Scenario Dependency Surface

The continuous milestone requires one unbroken chain from an admitted Map 3 start through observable
Battle 01 completion. The audit inventories each segment in source order:

1. admitted Map 3 start and its exact scenario-relevant state;
2. Map 3 setup selection, init execution, and content;
3. exploration loop, player input, and map-event handoffs;
4. dialogue, entity interaction, and area-description behavior on the route;
5. field menu and UI behavior if the route reaches it;
6. map resources, camera, and layout behavior;
7. map-to-battle admission (battle-candidate selection and cutscene handoff);
8. Battle 01 natural encounter setup (roster, stats, items, spells, positions, flags);
9. player turn and battle menu control;
10. AI, navigation, action construction, and resolution through the whole battle;
11. battle scene presentation behavior actually reached;
12. victory, after-battle program execution, and return routing;
13. the observable endpoint state;
14. save/load scope only if the later product decision includes it.

Segments 6 and 11 are largely **Confirmed** private-import corpora plus bounded control contracts;
their remaining gaps are rendered/presentation behavior, which is product-gated under
[ADR 0005](../decisions/0005-remake-value-driven-driver-freeze.md). Segment 14 depends on an
explicit product decision. Segments 1–5, 7–10, 12, and 13 carry the fidelity-blocking research gaps
registered below.

## Exact Accepted-Index Audit

The research index at the audit base contains **1,621 records**, **165 fixtures** (74 H2, 91 H3), and
**2,551 address bindings**. The scenario-relevant denominators are:

| Evidence surface | Exact count | Owner |
| --- | ---: | --- |
| Map 3 source-path records (`data/maps/entries/map03/*`) | 26 | `sf2-map-data-static-v1`, aggregate-only |
| Map 3 setup selector routes | 4 rows (default, flags 506/543/609) in 64 default + 66 flag rows total | `sf2-map-setup-static-v1`, `sf2-map-setup-selection-runtime-v1` |
| Map 3 init dispatch cases runtime-confirmed | default/609/506/543 at `0x47512` | `sf2-map-init-dispatch-runtime-v1` |
| `battle.functions.*` records | 15 | `sf2-battle-functions-static-v1` |
| Battle 01 cutscene data records | 2 (`beforebattle`, `afterbattle`) | `sf2-battle-cutscene-data-static-v1` |
| Battle 01 activation/turn-order records | 2 + 1 (`activate-enemies`, `trigger-regions-and-activate-enemies`, turn-order owner in runtime math) | `sf2-battle01-region-activation-v1`, `sf2-battle01-secondary-activation-v1`, `sf2-battle01-turn-order-v1` |
| Battle control/loop lifecycle records | 9 + 14 | `sf2-battle-control-static-v1`, `sf2-battle-loop-static-v1` |
| New-game/exploration/gameflow records | 13 (`gameflow.start/main/exploration`) | `sf2-gameflow-core-static-v1` |
| Story-state write surface | 56 write flags, 6 read flags, 3 read/write overlaps; battle-unlock base 400 | `sf2-story-state-runtime-v2` |

The 26 Map 3 records remain **unassociated** and each carries only the aggregate
`sf2-map-data-static-v1` owner. This audit does not associate them; a future scenario/data contract
must derive its exact record set from dedicated accepted evidence owners.

## Segment Evidence Inventory

### 1. Admitted Map 3 start

**Confirmed:** a controlled New action exercises slot/difficulty flag paths, calls `SaveGame`, and
transfers to `MainLoop` with current/egress map 3
([story-progression synthesis](../design/synthesis/story-progression.md), save-system/witch H3
rails). `NewGame`'s static initialization order and the three settings clears (global flags, Deals,
Caravan) plus message-speed default are contracted
([new-game-state-initialization contract](../design/contracts/new-game-state-initialization.md)).
Because the settings stage clears global flags, the fresh-start flag bitset is expected to select Map 3
default setup rows; the selector and init dispatch for the default/609/506/543 rows are
runtime-confirmed in isolation.

**Gap (RA-01):** no exact admitted snapshot records every scenario-relevant field — map, position,
facing, flag bitset, party composition, per-ally stats/items/spells, gold, difficulty, RNG seed, and
elapsed-time state — at the first Map 3 exploration entry. The New H3 handoff bypasses naming, menu
selection, and text presentation; the selector/init matrices change only `CURRENT_MAP` and flag bits
from a debug Map Test prompt. Which of the four Map 3 setups is selected by the natural admitted state
is **Inferred** (flag-clear ⇒ default), not **Confirmed** in one natural flow, and the selected
init function/script's state effects are **Unknown**.

### 2. Map 3 setup selection, init execution, and content

**Confirmed:** the 26-record Map 3 aggregate inventory; selector source order and ten-case selection
runtime; Map 3 default/609/506/543 init dispatch (six-case matrix); the complete 90-profile init
inventory; the 47 standalone setup scripts (8,058 statements, 178 labels); the complete 914-program
event corpus (684 entity-event, 150 zone-event, 80 item-event boundaries); nine-case wrapper dispatch
runtime (event bodies replaced with `rts`); the 79-map/1,859-resource canonical import; block-copy
runtime; entity population/reload runtime; Map 3 area-description and item-event tables; and
`map.data.cs-513d6`/`cs-628c8` script containers.

**Gaps:** which Map 3 setup rows the admitted state selects (RA-01 overlap); the ordered
setup → init function → init script → event → description chain executed at admission with real
program bodies (RA-02); the story meaning and state effects of flag operands 506/543/609 in Map 3
setup rows (**Unknown** — they are shared operands across many maps and are not battle/route evidence
by themselves); direct-`rts` entity-event reachability through normal story routes (**Unknown**,
explicit map-data queue); init-script side effects and transition persistence (explicit
`map-init-effects-and-presentation` H3 queue).

### 3. Exploration loop, player input, and map events

**Confirmed:** the 13-file gameflow/exploration static inventory; map-event-before-A/C-action polling
priority; six map-event types; item handoff and refill; entity activation admission (48 candidates,
one-tile radius); chest/vase/barrel/bookshelf area kinds; two-port input sampling with a sixteen-case
H3 matrix; entity movement core (13-case/20-tick matrix); interaction-trigger runtime (six Map 02
handler cases).

**Gaps:** natural inputs and their chronological results along the route (RA-03); VInt-edge event
publication versus input sampling timing (explicit grouped queue); exploration/VDP frame sequencing
(queued); the field-menu branch's natural admission and behavior (RA-08); roof/door/warp/vehicle
transition frames (queued).

### 4. Dialogue, interaction, and area descriptions

**Confirmed:** the dialogue command family (2,883 ordered references; 21-case handler-local H3); the
17-bank/4,267-string Huffman corpus; the 119-row sprite-dialogue property table; portrait window
state; text/font systems; area-description dispatch (75 targets, `d6` condition closed); Map 3
area-description tables.

**Gaps:** which dialogue programs execute on the route, their exact text-line references, speaker
selections, and cursor/state effects (RA-09); rendered text/wait/portrait timing (product-gated);
normal-story reachability of the description/init callers.

### 5. Field menu and UI

**Confirmed:** the 42-file common-menus inventory; shop/church/caravan/blacksmith state machines with
extensive H3; window system (16 stable entries); UI layout and icon corpora; exploration A/C branch
static structure showing the A path into the field menu.

**Gap (RA-08):** no field-menu control contract exists and no natural field-menu admission is
observed. Whether the route requires it is a product/design decision; if included, the field-menu
behavior on the route is a research gap. Explicit exclusion by an accepted product decision would
close this row.

### 6. Map resources, camera, layout

**Confirmed:** complete private-import corpora (tilesets 115 streams, palettes 16, layouts 77, map
sprites 669 payloads, entity data, camera control seven-case H3). Rendered presentation is
product-gated.

### 7. Map-to-battle admission

**Confirmed:** `CheckBattle` static contract (battle-map rows, independent X/Y trigger coordinates with
`-1` wildcards, unlocked-flag admission) in
[map-entry-routing-state](../design/contracts/map-entry-routing-state.md); 45 battle-map rows /
33 distinct map identities; `MainLoop` battle-candidate handoff to `BattleLoop`; new-battle entry order
(before/start cutscenes, region-flag clears, heal, roster init, load); cutscene routing contract
(before-battle checks the intro flag, battle-start sets it); story-state battle-unlock base 400 writes
confirmed in-process; Battle 01 map link (map 57) and trigger metadata.

**Gaps (RA-04):** the natural chronological path from Map 3 to the Battle 01 trigger is unobserved —
the intermediate maps, the trigger coordinates actually reached, the battle-unlock flag state, and the
before/start cutscene execution in a natural flow are all **Unknown**. The accepted Battle 01 H3
entries use Debug Battle Test, which sets the shared intro flag and skips the cutscene scripts; the
two Battle 01 cutscene data files are indexed but their story effects are not reconstructed.

### 8. Battle 01 natural encounter setup

**Confirmed:** map 57, 16×20 area, three allies at `(8,18)`, `(9,18)`, `(7,18)`; six enemy entities
128–133 (enemy 39, GIZMO); `ATTACKER1`/`ATTACKER2` commandsets; three region polygons; no AI points;
terrain corpus; halved-EXP membership; leader-flag 0 (no leader-victory rule); seed-`0x1234` turn
order `0:109, 2:8, 1:6, 128:6, 133:6, 129:4, 130:4, 131:4, 132:4`; region activation semantics
(primary/secondary bits); battlefield grids and movement/range contracts.

**Gap (RA-05):** the natural-entry snapshot (the roster/stats/items/spells/flags/RNG state carried
into the battle from the route) is **Unknown**; all Battle 01 runtime evidence derives from the debug
entry path and controlled seeds. Later-round region-state clearing and natural secondary-region data
remain explicit open questions.

### 9. Player turn and battle menu control

**Confirmed:** the 15-record battle-functions static contract and its design-contract owner; the
complete shared battle-function file inventory; player-control/cursor/menu static decisions; input
surface.

**Gaps (RA-06 part):** natural player input cadence, cancel paths, and menu selection through a real
Battle 01 turn are **Unknown**; no natural multi-round playthrough has been observed.

### 10. AI, navigation, actions, resolution

**Confirmed:** complete battle-AI static contracts (filter, priority, heal, support, action choice,
movement); 14-case action-choice H3 from one natural Battle 01 entry with controlled target lists and
seeds; five-case weighted-propagation matrix; physical-attack-chain, dodge, follow-up-validation,
kill-EXP, final-EXP, gold, enemy-drop, level-up, spell-resolution, status-expiry H3 fixtures; action
construction contracts.

**Gap (RA-06 part):** the complete set of AI/navigation/action/resolution branches actually reached by
an unmodified multi-round Battle 01 playthrough is **Unknown**; existing fixtures are bounded and
controlled. Only branches reached by the accepted playthrough must be closed; unrelated branches must
not be generalized.

### 11. Battle scene presentation

**Confirmed:** the scene-engine static contract (21-command interpreter, 32 setup/update pairs); the
complete sprite/background/effect/terrain/weapon asset corpora; 421 frame entries; graphics-service
state. Rendered frame/VDP/audio timing is product-gated and queued.

### 12. Victory, after-battle, return

**Confirmed:** generic victory mutation order (heal party, after-battle seam, clear unlocked, set
completed at +100, return `D4=1`); defeat path (restore leader HP, halve gold, egress); cutscene
routing (after-battle skips when completed flag set, then joins the per-battle table); Battle 01
after-battle cutscene data indexed; after-battle positions data table.

**Gaps (RA-07):** no natural victory has been observed through the normal controller; the Battle 01
after-battle MAPSCRIPT's effects, the return routing into the following map state, and the exact
observable endpoint state are **Unknown**. The endpoint's observable meaning is a product decision,
but the post-battle state facts that any endpoint will need are a research closure.

### 13. Observable endpoint state

**Gap (RA-12):** the final scenario-relevant state capture at whatever endpoint the product decision
selects. The research closure is a state-fact contract (which fields, in what order, after which
after-battle effects); the endpoint selection is not.

### 14. Save/load scope

**Confirmed:** two-slot SRAM layout, 4,016-byte logical slots, interleaved spans, checksum/flag
transitions, in-process Save/Load/Copy/Delete H3; SaveGame/Church lifecycle rail.

**Gap (RA-10):** cross-process/power-loss durability of every scenario-relevant field is **Unknown**
and only needed if the product decision includes durable save/load; the natural route matrices use
in-process state only.

## Research Gap Register

Each row names the evidence that would close the gap. Fixture IDs are deliberately **not** invented
here; grouped-matrix proposals are named by observation seam and must be scoped by the owning research
slice under ADR 0003 (one launch per coherent matrix).

| ID | Segment | Gap | Required research closure | Evidence label today | Priority |
| --- | --- | --- | --- | --- | --- |
| RA-01 | 1 | Exact admitted Map 3 start state | One grouped matrix from the natural New→MainLoop handoff capturing map, position, facing, flag bitset, party/stats/items/spells, gold, difficulty, RNG, and time fields at first exploration entry; confirm the selected setup row in that flow | **Inferred** (flag-clear ⇒ default) / **Unknown** fields | Blocking |
| RA-02 | 2 | Map 3 setup/init/event chain effects | Observe the ordered setup → init function → init script → event → description chain at natural admission with real program bodies; record state effects and persistence | **Confirmed** dispatch; **Unknown** effects | Blocking |
| RA-03 | 3 | Natural Map 3 route | Scripted-input exploration matrix from the admitted state recording ordered inputs, zone events, entity interactions, area inspections, and the exit sequence; route policy comes from the Design/product audit | **Unknown** | Blocking |
| RA-04 | 7 | Map-to-battle admission | Extend the route matrix to the battle trigger: intermediate maps, trigger coordinates reached, battle-unlock flag state, and before/start cutscene execution in the natural flow | **Confirmed** static; **Unknown** natural | Blocking |
| RA-05 | 8 | Battle 01 natural encounter state | Capture the full scenario-relevant state at first battle-ready state from the natural flow; bind roster/stats/items/spells/positions/flags and later-round region state | **Confirmed** debug-entry; **Unknown** natural | Blocking |
| RA-06 | 9+10 | Complete playable battle trace | Fixed-seed, scripted-input multi-round matrix through victory recording the chronological player/AI/navigation/action/resolution/reward/status trace and every reached branch | **Confirmed** bounded; **Unknown** complete | Blocking |
| RA-07 | 12 | Victory, after-battle, endpoint state | Continue the battle matrix through victory to the after-battle seam: program execution, return routing, and post-battle state capture | **Confirmed** generic order; **Unknown** natural | Blocking |
| RA-08 | 5 | Field menu on route | If the route includes it: natural field-menu admission and behavior matrix; otherwise requires explicit product exclusion | **Confirmed** static only | Route-dependent |
| RA-09 | 4 | Route dialogue chronology | Dialogue programs, text-line references, speakers, and cursor/state effects on the route, recorded by the route matrix | **Confirmed** corpus; **Unknown** chronology | Blocking |
| RA-10 | 14 | Cross-process persistence | Only if product selects durable save/load: prove every scenario field survives the original lifecycle | **Confirmed** in-process; **Unknown** durable | Product-gated |
| RA-11 | 6/11 | Presentation fidelity | Rendered VDP/DMA/audio frames only if the product tier requires original fidelity; frozen by default under ADR 0005 | **Confirmed** static assets; **Unknown** frames | Product-gated |
| RA-12 | 13 | Observable endpoint state | State-fact contract for the final scenario-relevant state at the product-selected endpoint: which fields, in what order, after which after-battle effects | **Unknown** | Blocking (facts) / product (selection) |

Static closures that can proceed without an emulator: the Map 3 event/init programs' operation
inventories (already in the 914-program corpus) can be joined to the 26 Map 3 records by the owning
research slice; the battle-trigger rows for the route maps can be bound statically once the Design
audit names the route; the two Battle 01 cutscene data files can receive full command-shape
inventories (structure already indexed, effects still not).

## Closure Plan (Research-Owned)

The closures follow the static-first, subsystem-batched cadence (ADR 0003) and the
root/worker slice contract (ADR 0004):

1. **Slice R1 — admitted start and setup chain (RA-01, RA-02):** one grouped H3 matrix from the
   accepted New→MainLoop handoff seam; static join of the 26 Map 3 records to their selected rows in
   that flow. No new fixture ID is committed by this audit.
2. **Slice R2 — natural route matrix (RA-03, RA-04, RA-09):** depends on the route policy named by
   the Design audit/product decision; observes exploration, dialogue, and admission chronologies in
   one matrix.
3. **Slice R3 — complete Battle 01 playthrough (RA-05, RA-06):** fixed seed and scripted inputs from
   the natural entry through victory; the largest H3 slice, reusing the accepted battle fixtures as
   authoritative subsystem goldens rather than copying their numbers.
4. **Slice R4 — after-battle and endpoint state (RA-07, RA-12):** continuation of R3 to the after
   program seam plus the endpoint state capture contract.
5. **Conditional slices:** RA-08 (field menu) and RA-10 (durable save) only after the corresponding
   product decisions.

A later readiness update (design-owned) may consume these closures only after they are accepted on
`main`. This audit's own Status remains **OPEN** until the blocking rows are closed; it does not
become a readiness report.

## Evidence Matrix

| Audit statement | Evidence label | Accepted owner | Boundary retained |
| --- | --- | --- | --- |
| Controlled New reaches MainLoop with current/egress map 3 | **Confirmed** bounded runtime | witch/save H3 rails, [story-progression](../design/synthesis/story-progression.md) | Not a natural player-visible New flow or complete start snapshot |
| 26 Map 3 records exist, aggregate-owned, unassociated | **Confirmed** indexed inventory | `sf2-map-data-static-v1`, [map-data research](map-data-inventory.md) | Not route chronology, selection, or effects |
| Map 3 default/609/506/543 init dispatch executes its modeled targets | **Confirmed** bounded runtime | `sf2-map-init-dispatch-runtime-v1`, [map-data research](map-data-inventory.md) | Init-script effects, story reachability, persistence |
| Map 3 setup flag meanings 506/543/609 | **Unknown** | `sf2-map-data-static-v1` | Shared operands across many maps; not route evidence by themselves |
| Battle 01 placement/terrain/activation/turn order under debug entry and fixed seed | **Confirmed** bounded runtime | [battle01-placement](battle01-placement.md), [battle-control lifecycle contract](../design/contracts/battle-control-lifecycle.md) | Natural admission, roster/stats binding, later rounds |
| Generic victory/defeat order and cutscene routing | **Confirmed** static; natural execution **Unknown** | [battle-loop](battle-loop.md), [battle-cutscenes](battle-cutscenes.md), cutscene routing contract | Natural victory, after-program effects, endpoint |
| Natural Map 3 route, Battle 01 admission, complete playthrough, after-battle effects | **Unknown** | none — gap register RA-03/RA-04/RA-06/RA-07 | Blocking research closures; no inference from source labels |

## Reproduction

The audit base counters are reproduced by:

```powershell
uv run sf2 research-index list --summary
uv run sf2 research-index test
uv run sf2 verify
```

The segment inventory cites the existing rails with their own reproduction commands, notably
`uv run sf2 h2 map-data`, `uv run sf2 h2 map-init`, `uv run sf2 h3 map-setup-selection`,
`uv run sf2 h3 map-init-dispatch`, `uv run sf2 h3 map-event-dispatch`, the Battle 01 turn-order/
activation H3 scripts, and the battle-loop/battle-control/combat/spell rails. Generated outputs stay
under ignored `local/derived/`.
