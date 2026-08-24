# Map 3 to Battle 01 Research Gap Audit

- Status: **OPEN** — durable research-owned gap register; not a readiness report and not a gap-closure
  claim
- Audit date: 2026-08-20
- Audit base: `main` commit `5fdfa46f1e261825a6a1eadb64aa0c852b46b5c5`, tree
  `eedb7a5fe779ef2e38ca5e1474c9d336b10e3989`
- ROM: USA retail, SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- Milestone owner: [ADR 0009](../decisions/0009-first-phase4-playable-slice.md)
- Accepted product profile: [ADR 0010](../decisions/0010-map3-battle01-product-acceptance.md),
  exact selection `1A + 2A + 3A + 4A + 5B + 6A + 7C + 8C + 9A + 10A`
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

- invent the exact route, admitted-state values, reached action trace, completion-state values, or
  8C comparison tolerances that remain Research/H4 evidence gaps;
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

ADR 0010 now fixes the product boundary that the initial audit left open: a controlled admitted
Map 3 snapshot (1A), the smallest Research-proven natural route (2A), natural battle admission
(3A), manual player control (4A), the first stable controllable post-after-program endpoint (5B),
no milestone save/load/checkpoint/suspend (6A), private-local original assets (7C), exact reached
frame/audio/hardware parity (8C), modern accessible logical controls (9A), and an explicit deviation
ledger (10A). These selections constrain the required evidence; they do not supply the missing route,
state, presentation, or H4 facts and do not change this audit's **OPEN** status.

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
14. the explicit no-save milestone boundary and restart-to-admitted-snapshot behavior;
15. private immutable original inputs and deterministic reference captures for the reached 7C/8C
    comparison domain.

Segments 6 and 11 have largely **Confirmed** private-import corpora plus bounded control contracts,
but selected 8C makes their reached rendered/audio/hardware behavior a mandatory pre-Phase-4
Research/H4 target. This is the bounded reopening condition 3 of
[ADR 0005](../decisions/0005-remake-value-driven-driver-freeze.md): it applies only to observables
reached by this scenario, must reuse existing parser/fixture/observation seams, and does not authorize
an open-ended driver or hardware audit. Segment 14 is closed as a milestone exclusion by 6A;
cross-process persistence remains deferred. Segments 1–5, 7–13, and 15 carry the fidelity-blocking
research gaps registered below.

## Exact Accepted-Index Audit

The research index now contains **1,625 records**, **173 fixtures** (79 H2, 94 H3), and
**2,626 address bindings**. The scenario-relevant denominators are:

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

**Confirmed (controlled R1 seam):**
[`map3-admitted-start`](map3-admitted-start.md) now records the complete scenario-relevant snapshot
at the first original Map 3 `WaitForEvent`: map/egress, position/facing, all joined and active bits,
all ally stats/items/spells, gold, difficulty, RNG, and elapsed-time fields. The observed clear-guard
state selects the default Map 3 setup and crosses the original setup wrapper and init function.

**Remaining RA-01 boundary:** this controlled admission bypasses player naming, menu selection, and
text presentation. It does not establish a natural player-visible New/load state or route
reachability; those remain **Unknown** for R2.

### 2. Map 3 setup selection, init execution, and content

**Confirmed:** the 26-record Map 3 aggregate inventory; selector source order and ten-case selection
runtime; Map 3 default/609/506/543 init dispatch (six-case matrix); the complete 90-profile init
inventory; the 47 standalone setup scripts (8,058 statements, 178 labels); the complete 914-program
event corpus (684 entity-event, 150 zone-event, 80 item-event boundaries); nine-case wrapper dispatch
runtime (event bodies replaced with `rts`); the 79-map/1,859-resource canonical import; block-copy
runtime; entity population/reload runtime; Map 3 area-description and item-event tables; and
`map.data.cs-513d6`/`cs-628c8` script containers.

**Confirmed (controlled R1 seam):** the observed default row traverses the original setup wrapper,
source-derived selection return, indirect init call/return, and `ms_map3_InitFunction` before the
first exploration wait. No guarded script/program request occurs on that selected default path.

**Remaining RA-02 boundary:** natural admission can still select different state, and the ordered
init-script → event → description chain with real program bodies is **Unknown**. The story meanings
and state effects of flags 506/543/609, direct-`rts` event reachability, init effects, and transition
persistence remain separate Map-data/R2 questions.

### 3. Exploration loop, player input, and map events

**Confirmed:** the 13-file gameflow/exploration static inventory; map-event-before-A/C-action polling
priority; six map-event types; item handoff and refill; entity activation admission (48 candidates,
one-tile radius); chest/vase/barrel/bookshelf area kinds; two-port input sampling with a sixteen-case
H3 matrix; entity movement core (13-case/20-tick matrix); interaction-trigger runtime (six Map 02
handler cases).

**Confirmed (R2b static fallback):** the 53-source, 23-function, 32-H1-field H2 contract derives a
16-segment/110-input legal graph through Maps 3/19/20/21, its zones, occupancy, and retained warp
predicates. It is static reachability only, not observed chronology.

**Confirmed (R2c static extension):** the retained R2b terminal model extends through the selected
Map 21 → 40 → 57 links and the source/H1/ROM-checked CheckBattle/BattleLoop/cutscene/LoadBattle
spine. This is source legality only; natural R2a → R2b → R2c continuity remains **Unknown**.

**Gaps:** natural inputs and their chronological results along the route (RA-03); VInt-edge event
publication versus input sampling timing; exploration/VDP frame sequencing; the field-menu branch's
natural admission and behavior when the 2A route requires it (RA-08); and reached
roof/door/warp/vehicle transition frames. Under 8C these reached observable surfaces join RA-11; they
are no longer optional presentation work.

**Confirmed (R2 opening only):** from the R1 admitted wait, one original-controller matrix reaches
the original `cs_5149A` messenger entry before its body.  It observes the house/school doors and
warps, raw-coordinate zone admissions, Sarah/entity-142 actions, entity-142 re-init, and the
source-derived logical input corpus.  This does not observe any continuation beyond that entry.

### 4. Dialogue, interaction, and area descriptions

**Confirmed:** the dialogue command family (2,883 ordered references; 21-case handler-local H3); the
17-bank/4,267-string Huffman corpus; the 119-row sprite-dialogue property table; portrait window
state; text/font systems; area-description dispatch (75 targets, `d6` condition closed); Map 3
area-description tables.

**Gaps:** which dialogue programs execute on the route, their exact text-line references, speaker
selections, and cursor/state effects (RA-09); the exact reached text/window/portrait output and cadence
required by 8C (RA-11); and normal-story reachability of the description/init callers. Original text
and captures remain private-local 7C inputs, not tracked fixture payloads.

**Confirmed (R2 opening only):** the callback chronology identifies reached Map 3 program entries
through `cs_5149A`'s pre-body boundary.  It does not expose dialogue payload, rendered speaker/window
state, timing, or the unexecuted messenger body.

### 5. Field menu and UI

**Confirmed:** the 42-file common-menus inventory; shop/church/caravan/blacksmith state machines with
extensive H3; window system (16 stable entries); UI layout and icon corpora; exploration A/C branch
static structure showing the A path into the field menu.

**Gap (RA-08):** no field-menu control contract exists and no natural field-menu admission is
observed. Accepted 2A excludes unrelated menu coverage: if Research proves the smallest natural route
reaches the field menu, that exact behavior is a route-dependent research gap; otherwise this row
closes as not reached without requiring a new product choice. Selected 9A's remappable actions,
reduced-flash mode, and adjusted text are product/H4 deviation surfaces, never original-fidelity
evidence.

**Confirmed (R2 opening only):** the reached route reports field menu `not-reached`; no menu behavior
or menu admission is claimed.

### 6. Map resources, camera, layout

**Confirmed:** complete private-import corpora (tilesets 115 streams, palettes 16, layouts 77, map
sprites 669 payloads, entity data, camera control seven-case H3). **Gap (RA-11):** 8C requires the
reached scenario's exact pixels, palettes, frame cadence, animation/timing, VInt/DMA/CRAM/VDP state,
and other hardware-observable map behavior. Existing static decoders and bounded runtime seams are
inputs to that closure, not proof of rendered parity.

### 7. Map-to-battle admission

**Confirmed:** `CheckBattle` static contract (battle-map rows, independent X/Y trigger coordinates with
`-1` wildcards, unlocked-flag admission) in
[map-entry-routing-state](../design/contracts/map-entry-routing-state.md); 45 battle-map rows /
33 distinct map identities; `MainLoop` battle-candidate handoff to `BattleLoop`; new-battle entry order
(before/start cutscenes, region-flag clears, heal, roster init, load); cutscene routing contract
(before-battle checks the intro flag, battle-start sets it); story-state battle-unlock base 400 writes
confirmed in-process; Battle 01 map link (map 57) and trigger metadata.

**Confirmed (R2b static fallback):** Map 21 `cs_53EF4` sources F401 via `setStoryFlag 1`, and its
handler sources F256. This confirms program/flag semantics, not a runtime handoff.

**Confirmed (R2c static extension):** Map 21 → 40 → 57, Battle 01 row selection, F401/F501 checks,
the new-battle branch, before/start routing, and LoadBattle order are source/H1/ROM-derived. Natural
admission and caller order remain **Unknown**.

**Gaps (RA-04):** the natural chronological path from Map 3 to the Battle 01 trigger is unobserved —
the intermediate maps, the trigger coordinates actually reached, the battle-unlock flag state, and the
before/start cutscene execution in a natural flow are all **Unknown**. The accepted Battle 01 H3
entries use Debug Battle Test, which sets the shared intro flag and skips the cutscene scripts; the
two Battle 01 cutscene data files are indexed but their story effects are not reconstructed.

**Inferred / partial (R2):** source/H1/ROM deterministically reconstruct the Map 3 → 19 → 20 → 21
→ 40 → 57 topology and Battle 01 routing, but the only runtime observation remains the Map 3 opening
through `cs_5149A` entry.  The exact remaining seam is the messenger body through F401, the Map 57
trigger, before/start cutscenes, and battle-ready state.

### 8. Battle 01 natural encounter setup

**Confirmed:** map 57, 16×20 area, three allies at `(8,18)`, `(9,18)`, `(7,18)`; six enemy entities
128–133 (enemy 39, GIZMO); `ATTACKER1`/`ATTACKER2` commandsets; three region polygons; no AI points;
terrain corpus; halved-EXP membership; leader-flag 0 (no leader-victory rule); seed-`0x1234` turn
order `0:109, 2:8, 1:6, 128:6, 133:6, 129:4, 130:4, 131:4, 132:4`; region activation semantics
(primary/secondary bits); battlefield grids and movement/range contracts.

**Confirmed (R2c static extension):** Battle 01 definitions are `STARTING`; no Battle 01 row appears
in the complete region-cutscene table; first-round source order ends at turn-order generation before
the first individual turn. This is not a natural initialized snapshot or first-actor observation.

**Gap (RA-05):** the R3a static rail adds the post-generation first-consumer/control-dispatch
foundation only. The natural-entry snapshot (the roster/stats/items/spells/flags/RNG state carried into
the battle from the route) is **Unknown**; all Battle 01 runtime evidence derives from the debug entry
path and controlled seeds. Later-round region-state clearing and natural secondary-region data remain
explicit open questions.

### 9. Player turn and battle menu control

**Confirmed:** the 15-record battle-functions static contract and its design-contract owner; the
complete shared battle-function file inventory; player-control/cursor/menu static decisions; input
surface.

**Gaps (RA-06 part):** natural player input cadence, cancel paths, and menu selection through a real
Battle 01 turn are **Unknown**; no natural multi-round playthrough has been observed. R3a/R3b/R3c/R3d add
only the **Confirmed** static control-to-ApplyActionEffect-to-DropEnemyItem-to-action-completion/caller-finalization graph
through the unentered `0x23BB2 -> 0x23B40` backedge; actual branches, loops, and results remain **Unknown**.

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
state. **Gap (RA-11):** selected 8C requires accepted evidence for every reached pixel/palette/frame,
animation/timing, audio waveform/chip/timing, VInt/DMA/CRAM/VDP, and other hardware-observable
surface. The evidence must reuse the existing scene, graphics, sound, timing, and hardware seams and
must not expand into unrelated driver behavior.

### 12. Victory, after-battle, return

**Confirmed:** generic victory mutation order (heal party, after-battle seam, clear unlocked, set
completed at +100, return `D4=1`); defeat path (restore leader HP, halve gold, egress); cutscene
routing (after-battle skips when completed flag set, then joins the per-battle table); Battle 01
after-battle cutscene data indexed; after-battle positions data table.

**Gaps (RA-07):** no natural victory has been observed through the normal controller; the Battle 01
after-battle MAPSCRIPT's effects, the return routing into the following map state, and the exact
observable endpoint state are **Unknown**. ADR 0010 option 5B fixes the endpoint shape as the first
stable player-controllable state after victory mutation, after-battle program execution, and return
handoff; Research still must establish its exact values and the presentation/hardware trace through
that boundary.

### 13. Observable endpoint state

**Gap (RA-12):** the final scenario-relevant state capture at the accepted 5B endpoint. The research
closure is a state-fact contract: which fields have which values, in what order, after which
after-battle effects, at the first stable player-controllable state.

### 14. Save/load scope

**Confirmed:** two-slot SRAM layout, 4,016-byte logical slots, interleaved spans, checksum/flag
transitions, in-process Save/Load/Copy/Delete H3; SaveGame/Church lifecycle rail.

**Deferred boundary (RA-10):** ADR 0010 option 6A excludes user-facing save, load, checkpoint, and
battle suspend from this milestone. Restart returns to the controlled admitted snapshot. Persistence
research is therefore not an implementation blocker for this profile; cross-process/power-loss
durability remains **Unknown** for a separate milestone. Harness reset/setup is test machinery and
must never be described or accepted as a save feature.

### 15. Private immutable inputs and reference captures

**Confirmed policy:** ADR 0010 option 7C selects original assets, dialogue, graphics, music, and sound
only for the private-local milestone/profile. It grants no redistribution right.

**Gap (RA-11):** before any 8C reference run can become accepted evidence, Research must own a
private inventory for every reached immutable input and capture. Each row needs identity, source and
acquisition provenance, cryptographic hash, applicable license/rights boundary, producing tool/core
and version, configuration, deterministic seed/input/timing conditions, and the observable layer it
supports. ROMs, SRAM, states, traces, decoded payloads, audio/video/images, and captures remain
ignored local inputs: none may be tracked, uploaded, embedded in public CI or a public release, or
quoted into a redistributable fixture. Public reports may expose only licensing-safe metadata,
approved hashes, structural/state summaries, tolerances, and comparison results.

## Research Gap Register

Each row names the evidence that would close the gap. Fixture IDs are deliberately **not** invented
here; grouped-matrix proposals are named by observation seam and must be scoped by the owning research
slice under ADR 0003 (one launch per coherent matrix).

| ID | Segment | Gap | Required research closure | Evidence label today | Priority |
| --- | --- | --- | --- | --- | --- |
| RA-01 | 1 | Exact admitted Map 3 start state | R1 controlled matrix now captures map, position, facing, flag bitset, party/stats/items/spells, gold, difficulty, RNG, and time at first exploration entry and observes the default row; natural player-visible admission remains R2 | **Confirmed** controlled default / **Unknown** natural state | R2 dependency |
| RA-02 | 2 | Map 3 setup/init/event chain effects | R1 observes setup → init function and no default guarded program request; R2 must observe a natural real-program/event/description chain and state effects | **Confirmed** controlled default prefix / **Unknown** natural effects | R2 dependency |
| RA-03 | 3 | Natural Map 3 route | R2c confirms only static R2b-terminal extension/source legality; natural R2a → R2b → R2c continuity remains separately unobserved | **Confirmed** R2a bounded runtime / **Confirmed** R2b/R2c static graph / **Unknown** natural continuity | Blocking |
| RA-04 | 7 | Map-to-battle admission | R2c confirms static Map 21 → 40 → 57, CheckBattle/BattleLoop/before/start/LoadBattle spine; natural admission and caller order still require evidence | **Confirmed** static spine / **Unknown** natural admission/caller order | Blocking |
| RA-05 | 8 | Battle 01 natural encounter state | R2c/R3a confirm static definitions/init/turn-generation/first-consumer-control foundation; capture the full natural scenario state and first actor without inferring player readiness | **Confirmed** debug-entry and static foundation; **Unknown** natural snapshot/first actor/player-ready | Blocking |
| RA-06 | 9+10 | Complete playable battle trace | R3a/R3b/R3c/R3d confirm only the static control-to-ApplyActionEffect-to-DropEnemyItem-to-action-completion/caller-finalization graph through unentered `0x23BB2 -> 0x23B40`; fixed-seed, scripted-input H4 reference matrix through victory must record the chronological player/AI/navigation/action/resolution/reward/status trace and every reached branch | **Confirmed** bounded/static graph; **Unknown** complete | Blocking |
| RA-07 | 12 | Victory, after-battle, endpoint state | R3d adds only static unentered victory/defeat edges; continue the battle matrix through victory to the after-battle seam: program execution, return routing, and post-battle state capture | **Confirmed** generic order/static edges; **Unknown** natural | Blocking |
| RA-08 | 5 | Field menu on route | R2 opening records `not-reached`; R2c introduces no menu dependency, while route continuity remains Unknown | **Confirmed** NotReached opening / **Unknown** continuity | Route-dependent |
| RA-09 | 4 | Route dialogue chronology | R2c retains only source command/text IDs and hashes; actual dialogue prose and chronology remain private/Unknown | **Confirmed** structural IDs/hashes / **Unknown** prose and chronology | Blocking |
| RA-10 | 14 | Deferred persistence boundary | Record the accepted 6A exclusion, restart-to-admitted-snapshot behavior, and harness-reset ≠ save; retain cross-process durability as a separate-milestone Unknown | **Confirmed** in-process; **Unknown** durable | Deferred / non-blocking |
| RA-11 | 6/11/15 | Reached 7C/8C presentation, hardware, and private-reference evidence | R2c retains only static presentation owner IDs/hashes; inventory/hash reached immutable inputs and capture pixel/palette/frame/audio/hardware observables in a separately scoped continuation | **Confirmed** static owner IDs/hashes and bounded seams; **Unknown** complete 8C | Blocking |
| RA-12 | 13 | Observable endpoint state | State-fact contract for the final scenario-relevant state at accepted endpoint 5B: which fields, in what order, after which after-battle effects | **Unknown** | Blocking |

RA-11 is the only ADR 0005 condition-3 reopening authorized here. Every question must be selected by
the accepted route or battle trace and attached to an existing parser, fixture, or observation seam;
unrelated Z80, VDP, DMA, controller-electrical, SRAM-failure, or other driver exactness stays frozen.
The 8C reference run uses one declared original-fidelity configuration. Reduced-flash and
instant/adjustable-text modes plus modern remappable input belong to the separate 9A/10A deviation
layer and may share the completion event, but they must not alter or satisfy the exact 8C reference.

Static closures that can proceed without an emulator: the Map 3 event/init programs' operation
inventories (already in the 914-program corpus) can be joined to the 26 Map 3 records by the owning
research slice; the battle-trigger rows for the route maps can be bound statically once the Design
audit names the route; the two Battle 01 cutscene data files can receive full command-shape
inventories (structure already indexed, effects still not).

## Closure Plan (Research-Owned)

The closures follow the static-first, subsystem-batched cadence (ADR 0003) and the
root/worker slice contract (ADR 0004):

1. **Slice R1 — admitted start and setup chain (RA-01, RA-02, RA-11 foundation):** **completed on
   the controlled seam** by `sf2-map3-admitted-start-runtime-v1`. It joins the 26 Map 3 records to
   the observed default row, captures the admitted snapshot, and records private-input identity/tool/
   timing metadata. It does not close natural route/program effects or full 8C capture work.
2. **Slice R2 — smallest natural route matrix (RA-03, RA-04, RA-08, RA-09, reached RA-11):**
   **completed only through the Map 3 opening** by
   `sf2-map3-battle01-natural-route-runtime-v1`: it reaches `cs_5149A` entry-before-body, records
   field menu `not-reached`, and establishes private provenance/callback-state foundations.  It leaves
   Map 19/20/21/40/57 and Battle 01 as static reconstruction, not runtime evidence.
3. **Slice R2a — messenger acceptance continuation (RA-03, RA-04, RA-08, RA-09, reached
   RA-11):** **completed only through the post-messenger follower-ready wait** by
   [map3-messenger-acceptance](map3-messenger-acceptance.md),
   sf2-map3-messenger-acceptance-runtime-v1. It continues strictly from the accepted R2
   entry-before-body boundary, accepts the original default-zero prompt path, observes the bounded
   joins/followers/F603 commit, and stops at the original stable wait. It adds no Castle, later-map,
   CheckBattle, Battle 01, menu, persistence, rendered-prose, timing, or 8C claim.
4. **Slice R2b — static fallback (RA-03, RA-04):** **Confirmed H2 static contract** by
   `sf2-map3-castle-battle-unlock-static-v1`. It preserves R1/R2/R2a as dependency guards and derives
   the Maps 3/19/20/21 legal source graph, zone/warp/occupancy topology, and F401/F256 source
   semantics. Natural continuation, Maps 21 → 40 → 57, Battle 01 admission, and R2c readiness remain
   **Unknown**; this is not an H3 observation or readiness promotion.
5. **Slice R2c — static Battle 01 admission extension (RA-03, RA-04, RA-05, RA-08, RA-09,
   RA-11):** **Confirmed H2 static contract** by
   `sf2-map3-battle01-admission-static-v1`. It begins only from the retained R2b terminal model and
   derives Maps 21/40/57, battle admission/initialization, structural cutscene IDs/hashes, and
   pre-first-turn order. Natural continuity, caller order, initialized state, actor, prose, menus, and
   complete 8C remain **Unknown**.
   R3a/R3b then add only the post-generation control-to-ApplyActionEffect-to-DropEnemyItem H2 spine;
   they do not close natural state, actor/control selection, player input, action/resolution, or any H3 question.
6. **Slice R3 — complete Battle 01 playthrough (RA-05, RA-06, reached RA-11):** fixed seed and
   scripted H4 reference inputs from natural entry through victory; capture every reached logical and
   8C scene/audio/hardware layer while reusing accepted battle, scene, graphics, sound, and timing
   fixtures as authoritative subsystem goldens rather than copying or weakening them.
7. **Slice R4 — after-battle, endpoint, and H4 definition closure (RA-07, RA-11, RA-12):** continue
   R3 through the after-program seam to the accepted 5B endpoint; freeze the final state contract,
   private capture manifest, comparison domains, exact or field-specific tolerances, licensing-safe
   public report, and the separate 9A/10A accessibility/deviation assertions.
8. **Deferred/non-blocking:** RA-10 persistence remains outside the 6A milestone. No save H3 slice is
   part of R1–R4; any later persistence work requires a separate accepted milestone. RA-08 produces a
   dedicated extension only if the Research-proven 2A route actually reaches the field menu.

A later readiness update (design-owned) may consume these closures only after they are accepted on
`main`. This audit's own Status remains **OPEN** until the blocking rows are closed; it does not
become a readiness report.

## Evidence Matrix

| Audit statement | Evidence label | Accepted owner | Boundary retained |
| --- | --- | --- | --- |
| Controlled New reaches MainLoop with current/egress map 3 | **Confirmed** bounded runtime | witch/save H3 rails, [story-progression](../design/synthesis/story-progression.md) | Not a natural player-visible New flow or complete start snapshot |
| Controlled New reaches first Map 3 exploration wait through default setup/init | **Confirmed** bounded runtime | [`map3-admitted-start`](map3-admitted-start.md), `sf2-map3-admitted-start-runtime-v1` | Not natural reachability, route programs/events, or 8C presentation |
| R1 admitted Map 3 opening reaches `cs_5149A` entry-before-body | **Confirmed** bounded runtime | [`map3-battle01-natural-route`](map3-battle01-natural-route.md), `sf2-map3-battle01-natural-route-runtime-v1` | Not a continuous Map 19/20/21/40/57 or Battle 01 runtime route |
| R2a accepts the messenger prompt and reaches follower-ready WaitForEvent | **Confirmed** bounded runtime | [map3-messenger-acceptance](map3-messenger-acceptance.md), sf2-map3-messenger-acceptance-runtime-v1 | Not later-map, battle-admission, persistence, prose/presentation, or 8C evidence |
| 26 Map 3 records exist, aggregate-owned, unassociated | **Confirmed** indexed inventory | `sf2-map-data-static-v1`, [map-data research](map-data-inventory.md) | Not route chronology, selection, or effects |
| Map 3 default/609/506/543 init dispatch executes its modeled targets | **Confirmed** bounded runtime | `sf2-map-init-dispatch-runtime-v1`, [map-data research](map-data-inventory.md) | Init-script effects, story reachability, persistence |
| Map 3 setup flag meanings 506/543/609 | **Unknown** | `sf2-map-data-static-v1` | Shared operands across many maps; not route evidence by themselves |
| Battle 01 placement/terrain/activation/turn order under debug entry and fixed seed | **Confirmed** bounded runtime | [battle01-placement](battle01-placement.md), [battle-control lifecycle contract](../design/contracts/battle-control-lifecycle.md) | Natural admission, roster/stats binding, later rounds |
| Generic victory/defeat order and cutscene routing | **Confirmed** static; natural execution **Unknown** | [battle-loop](battle-loop.md), [battle-cutscenes](battle-cutscenes.md), cutscene routing contract | Natural victory, after-program effects, endpoint |
| Milestone persistence scope | **Accepted product exclusion** | ADR 0010 option 6A | No user save/load/checkpoint/suspend; harness reset is not save; durable persistence remains separate |
| Private-local original inputs | **Accepted product boundary; evidence inventory OPEN** | ADR 0010 option 7C | No tracked/uploaded/public-CI/public-release payloads; identity, provenance, hashes, tools, and capture conditions remain RA-11 |
| Reached frame/audio/hardware parity | **Accepted mandatory target; evidence OPEN** | ADR 0010 option 8C and bounded ADR 0005 condition-3 reopening | Reached observables only; reuse existing seams; unrelated hardware/driver work frozen |
| Accessibility and modern mappings | **Accepted product/H4 deviation surface** | ADR 0010 options 9A/10A | Separate checks; never substitute for or alter the 8C exact reference run |
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

The bounded R2 opening is reproduced by:

```powershell
uv run sf2 h3 map3-battle01-natural-route --timeout-seconds 300
```
