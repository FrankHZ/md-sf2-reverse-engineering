# Map and Exploration Contract

- **Confirmed original behavior:** 79 map definitions, shared block/layout ownership, source-form
  areas/events/items/animations, 64x64 decoded layouts, setup selection, documented first-match
  dispatch rules, static transition-consumer priority, load-path-specific layout persistence,
  source-shaped map-script entity population/reload, cloneEntity, camera-control, and entity-placement command records, and batched frame-level entity
  movement/action timing
- **Unknown original behavior:** normal-story
  reachability of the non-empty map 52 direct-`rts` event setup, exact VDP-visible scroll timing,
  hardware-level animation scanline timing, and final VDP-visible rendered parity
- Remake status: implementation-neutral Phase 3 contract; no engine has been selected

## Contract Boundary

A remake map importer MUST construct a `MapDefinition` from research-owned structured output, never
from assumptions embedded in renderer or scene code. Each definition has these independently owned
references:

1. palette and five tileset slots;
2. one blockset and one 64x64 layout;
3. ordered area records;
4. ordered flag, step, roof, and warp event records;
5. ordered chest and other-item records;
6. an optional animation table;
7. a separately selected six-pointer map-setup definition, resolved through the ordered per-map
   default/flag route.

References are identities, not implicit copies. Maps 24 and 46 reuse the blockset/layout identities of
maps 23 and 7. The importer MUST preserve that sharing unless an explicit remake transform requests a
copy. Optional original pointers represented by `$FFFFFFFF` become absent values, not empty invented
tables.

Evidence is executable through:

- `sf2-map-content-static-v1` in
  `tests/fixtures/h2/map-content-static-v1.json`;
- `sf2-map-layout-decode-v1` in
  `tests/fixtures/h2/map-layout-decode-v1.json`;
- `sf2-canonical-map-import-v1` in
  `tests/fixtures/h2/canonical-map-import-v1.json`;
- `sf2-map-events-static-v1` in
  `tests/fixtures/h2/map-events-static-v1.json`;
- `sf2-map-init-static-v1` in
  `tests/fixtures/h2/map-init-static-v1.json`;
- `sf2-map-script-engine-static-v1` in
  `tests/fixtures/h2/map-script-engine-static-v1.json`;
- `sf2-map-setup-selection-runtime-v1` in
  `tests/fixtures/h3/map-setup-selection-v1.json`;
- `sf2-map-init-dispatch-runtime-v1` in
  `tests/fixtures/h3/map-init-dispatch-v1.json`;
- `sf2-map-event-dispatch-runtime-v1` in
  `tests/fixtures/h3/map-event-dispatch-v1.json`;
- `sf2-map-animation-vdp-runtime-v1` in
  `tests/fixtures/h3/map-animation-vdp-v1.json`;
- `sf2-entity-movement-runtime-v1` in
  `tests/fixtures/h3/entity-movement-matrix-v1.json`;
- `sf2-map-lifecycle-runtime-v1` in
  `tests/fixtures/h3/map-lifecycle-v1.json`;
- `sf2-map-script-control-audio-runtime-v1` in
  `tests/fixtures/h3/map-script-control-audio-v1.json`;
- `sf2-map-script-transition-runtime-v1` in
  `tests/fixtures/h3/map-script-transition-v1.json`;
- `sf2-map-interaction-trigger-runtime-v1` in
  `tests/fixtures/h3/map-interaction-trigger-v1.json`.
- `sf2-map-script-ui-primary-runtime-v1` in
  `tests/fixtures/h3/map-script-ui-primary-v1.json`.
- `sf2-map-script-entity-presentation-fx-runtime-v1` in
  `tests/fixtures/h3/map-script-entity-presentation-fx-v1.json`.
- `sf2-map-script-entity-clone-runtime-v1` in
  `tests/fixtures/h3/map-script-entity-clone-v1.json`.
- `sf2-map-script-screen-presentation-runtime-v1` in
  `tests/fixtures/h3/map-script-screen-presentation-v1.json`.
- `sf2-map-entity-lifecycle-presentation-runtime-v1` in
  `tests/fixtures/h3/map-entity-lifecycle-presentation-v1.json`.

The canonical-import fixture is the executable serialization of this contract. Its full generated payload stays
private under `local/derived/`; only aggregate structure and provenance are tracked.

The canonical import now resolves all 64 setup routes and 126 setup definitions. A setup definition
references six independently shared resources: entities, entity events, zone events, area
descriptions, item events, and initialization function. The 15 map IDs with no original route keep a
null route. Selection scans every flag variant in source order and retains the last set flag; direct
return handlers remain explicit empty handlers rather than being replaced by guessed defaults.
The ten-case H3 matrix confirms the selector itself returns the H2-modeled pointer for a missing map,
default routes, single and multiple set flags, last-set-flag-wins, and later aliases that restore a
default pointer. It replays one natural debug Map Test prompt and changes only `CURRENT_MAP` plus the
game-flag bitset at selector entry; the original scan and return execute unchanged.
The six-case init-dispatch matrix separately confirms that a missing map skips the indirect init
call, while five default/flag-selected setups each call their H2-modeled init target exactly once and
return through the original wrapper. It covers active, scripted, and direct-return targets without
claiming that synthetic map/flag combinations reproduce their story side effects.
Initialization resources retain the 130 ordered selector-route joins through 126 setup tables to 90
target profiles. Each profile keeps its physical-source operation boundary, ordered operation indices,
exact flag operands, direct instruction/effective call identities, and script-target resolution; the
597 physical operations, 973 pointer-table-weighted occurrences, and 1,100 route-weighted occurrences
are not interchangeable counts. The parsed dispatcher record retains linked enum, pointer-layout row,
and symbolic load-use-site evidence for the init-function pointer at byte offset 20 (the sixth
four-byte slot), plus the missing-setup comparison and branches, indirect call, restore, and return
order. A remake importer MAY translate recognized source forms into a typed command IR, but MUST retain
unknown operand text and MUST NOT infer script persistence, entity visibility, or frame timing from
opcode or target names alone.
Standalone script resources likewise retain ordered commands, operand text, and resolved references
between all 178 labels. These are importable command graphs, not proof that a modern engine may skip
the original interpreter's state, wait, camera, dialogue, or presentation sequencing.

**Confirmed source contract:** an importer that retains map-script command graphs MUST preserve the
source-faithful control/audio forms from `sf2-map-script-engine-static-v1`,
`tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.scriptControlCommandFacts`: `csWait`, `playSound`, `csc06`, `executeSubroutine`, `jump`,
the zero-byte source form `cscNop`, and the two-byte `$FFFF` `csc_end` form. `playSound` retains its
raw source enum operand and its maintained sound-data identity; it MUST NOT be normalized into an
audible-result, music, effect, or timing field merely from its source label.

**Confirmed source contract:** the imported interpreter model MUST keep the static A6-cursor boundary
distinct from encoded-byte storage. The named dispatch loop reads a word, compares signed `-1` with the
`csc_end` word, has a source-negative branch through the `BYTE_MASK`/`Sleep` sequence, and doubles the
non-negative selector before table dispatch. `executeSubroutine` transfers a four-byte target with
post-increment before its save/call/restore sequence; `jump` transfers a four-byte target into A6 with
zero cursor advance. These source control-flow facts require fidelity tests, but they do not establish
timer units, sound playback, story reachability, persistence, or player-visible presentation.

**Confirmed runtime boundary:** `sf2-map-script-control-audio-runtime-v1` preserves six one-launch
interpreter observations: D0=1 and one `WaitForVInt` entry versus debug P2-START skip, `$06`
dispatch/return, `$05` raw sound word and trap boundary, `$0A` cursor/stack/callee-return boundary,
`$0B` cursor replacement, and the `$FFFF` end/return. Its session-only entry seam preserves the source
wrapper epilogue before the observer records completion; it is an H3 harness boundary, not a gameplay
contract. At shared physical callback PCs, the observer selects failure diagnostics by exact fixture
case role while retaining one registration; this is likewise not adapter behavior. An
original-fidelity adapter MUST retain these control and cursor facts without converting a source enum or
observed trap into an audible, timing, persistence, or player-visible contract.

**Unknown:** the grouped `map-script-control-audio/*` queue in
[`common-scripting.md`](../research/common-scripting.md#confirmed-map-script-controlaudio-macro-boundary)
retains larger-duration timing, sound-driver/audible outcome, arbitrary callee effects, normal-story reachability,
and presentation behavior.

Direct map-event `txt` and operand-free `clsTxt` forms are likewise source identities, not a display
contract. An importer MUST retain each ordered site as either a numeric text-line identifier or the
literal `$FFFF` sentinel source form, plus its caller and independently named physical/setup/route
reference weights. It MUST preserve the complete 0–4,266 declared ID domain, including
zero-reference IDs, without copying or decoding original text into the map contract. This does not
specify shown content, speakers, windows, waits, input, story progression, or presentation behavior;
those remain outside this static import rule.
The importer MUST also retain the five source-named map-script transition forms `warp`, `resetMap`,
`loadMapFadeIn`, `reloadMap`, and `mapLoad` as ordered command records rather than replacing them with
guessed scene operations. Each record MUST keep its physical operand widths, raw destination-map
operand text when present, resolved declared map ID or the distinct `MAP_CURRENT` sentinel, and the
source command/program identity. The named handler boundary MUST retain the source-confirmed A6 cursor
reads, direct service call identity/order, and the `csc37_loadMapAndFadeIn` fall-through to
`csc48_loadMap`; it MUST preserve the parsed map-event value, D1 immediate, and packed-coordinate
multiplier as source facts. A remake MAY map these forms to an engine-specific transition IR only after
it defines its own behavior; the original source contract does not establish event consumption,
camera state, fade timing, display timing, or player-visible transition results.

**Confirmed bounded runtime boundary:** `sf2-map-script-transition-runtime-v1` at
`tests/fixtures/h3/map-script-transition-v1.json` adds one five-case launch through the original
interpreter. An original-fidelity adapter MUST preserve the observed opcode/A6/handler-return order,
csc37-to-csc48 fall-through, direct service seam order, csc07 event bytes, and the explicitly seeded
map/view-target/plane-A state facts as distinct adapter data. The source writes `OUT_TO_BLACK` value 2
at csc37 entry, while this bounded run reads `FADING_SETTING` as 0 at the first csc48
`WaitForVInt` entry after `LoadMapTilesets`; neither fact defines a fade duration or visible result.
The adapter MUST NOT turn these synthetic Map Test outcomes into normal-story reachability,
persistence, collision/pathfinding, camera presentation, or display behavior.
Map-script imports MUST separately retain the source-faithful `setCameraEntity`, `setCamDest`, and
`cameraSpeed` records in `sf2-map-script-engine-static-v1` at
`tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.mapCameraControlCommandFacts`. Each record MUST preserve its source opcode, physical
operand width, raw macro comment, program/command ordering, and zero-inclusive program-total domain.
The import boundary MUST also preserve the exact static handler records: csc24's advancing word read,
two branch polarities and parsed target statements, parsed constants and source-named write; csc32's
literal state write, two advancing words, alias call followed by wait call and return; and csc45's
advancing source word write and return. Direct instruction targets and resolved effective targets MUST
remain distinct identities, including zero-count per-handler rows, and the two parsed
`MAP_TILE_SIZE` use sites in `SetCameraDestination` MUST remain independent source records. A remake
MAY define an engine-specific camera interface independently; this static contract establishes neither
target/destination meaning, coordinate units, speed effect, timing, reachability, nor presentation.
The bounded H3 record `sf2-map-camera-control-runtime-v1` at
`tests/fixtures/h3/map-camera-control-v1.json` adds seven one-launch command observations without
turning source labels into a presentation design: it preserves the negative/ally/enemy target branch
records, destination input-word to transferred-word values, two speed-word values, direct/service/wait
callback order, and handler-return boundary. An original-fidelity adapter MUST keep those measured
state and call-order facts distinct from normal-story reachability and VDP/player-visible behavior,
which remain Unknown.
Map-script imports MUST separately retain the source-named `setPos`, `setPosFlash`, `setFacing`, and
`setDest` records in `sf2-map-script-engine-static-v1` at
`tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.entityPlacementCommandFacts`. Each record MUST preserve its opcode, encoded/operand byte
widths, raw macro comments, program/command identity, and compact complete-source order/hash boundary.
The import boundary MUST also retain the exact named handler records: `csc19`/`csc23` non-advancing
selector read plus alive-status cursor-adjustment call and advancing reads; `csc17`'s local branch
targets and `csc19` shared-tail edge; `csc29`'s three local branch targets; parsed `MAP_TILE_SIZE`
multiplier use sites; source-shaped state read/write operands; and zero-inclusive direct/effective
caller maps. These source records MUST NOT be normalized into a placement, facing, movement,
visibility, animation, coordinate-unit, collision, persistence, timing, or rendering model. The bounded
H3 contract `sf2-map-script-entity-placement-runtime-v1` at
`tests/fixtures/h3/map-script-entity-placement-v1.json` records seven one-launch cases: alive/dead
current-HP cursor outcomes for `setPos` and `setFacing`, source-scaled entity-record words/facing,
the complete 31-iteration local flash callback sequence plus its distinct shared-tail callbacks, and
both signed destination delta polarities with bit-15 wait/bypass. An original-fidelity adapter MUST
retain those measured RAM/cursor/callback facts without promoting them to a presentation or map-motion
design. Normal-story reachability, full animation/visibility/presentation, and collision/pathfinding/
persistence remain Unknown. A remake MAY define its own entity-state interface independently.
Map-script imports MUST separately retain the six source-named bridge forms `setActscriptWait`,
`setActscript`, `customActscriptWait`, `customActscript`, `entityActionsWait`, and `entityActions` in
`sf2-map-script-engine-static-v1` at `tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.entityActionBridgeCommandFacts`. Each record MUST preserve its source opcode, encoded and
operand byte widths, first source selector field, exact `$FF` or zero source control field, program/
command ordering, and compact full-corpus order/hash boundary. Inline payloads MUST retain their
source-form class, ordered command bytes, source terminator spelling, terminator byte count, and each
separately named primary/payload/terminator cursor advance; an importer MUST NOT collapse these physical
quantities or replace them with a semantic action sequence. `customActscript*` records MUST retain the
separate csc14 two-byte scan transfer and encoded-byte-derived, word-aligned scan iteration count;
those source facts are distinct from csc2D's two-byte interpreted-command read. The exact csc14/csc15/
csc2D handler guards, including the csc2D terminal chunk, resolved tail-transfer target, branch/call
order, source constant use site, and zero-inclusive
direct/effective caller identities, remain part of the import boundary. The joins to the map-event and
entity-action fixtures are provenance records only. The bounded H3 contract
`sf2-map-entity-action-bridge-runtime-v1` at
`tests/fixtures/h3/map-entity-action-bridge-v1.json` records all six aliases in one session: exact
handler/callback PCs, source-shaped entity field and cursor results, the csc14 inline-terminator hook,
the csc2D indexed target and terminal entry, and the exact csc2D buffer record at the parsed PC
immediately after its idle-payload write. That snapshot is a write-time record, while its global
buffer-pointer and entity-pointer fields are post-handler observations; an original-fidelity adapter
MUST preserve that observation boundary rather than treating the snapshot as persistent action state.
A remake MAY define its own scripting/action IR, but normal-story reachability, full action/motion/
collision effects, natural timing, persistence, and presentation remain Unknown.
Map-script imports MUST separately retain the eight source-named forms `hide`, `startEntity`,
`stopEntity`, `waitIdle`, `setSprite`, `setPriority`, `removeShadow`, and `setSize` in
`sf2-map-script-engine-static-v1` at `tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.entityLifecyclePresentationCommandFacts`. Each record MUST preserve opcode and physical
operand widths, raw macro comments, complete program/command order, and the zero-inclusive program
domain. The import boundary MUST retain the exact named handler instruction order: advancing versus
non-advancing A6 reads, alive-status pointer-adjustment literals/calls, source-shaped field operands,
parsed `COMBATANT_ALLIES_NUMBER` and `%1000` use sites, branch polarity/target identity, direct
instruction/effective-target caller maps, and return boundaries. These are source-layout and
control-flow records. A remake MUST NOT normalize them into a visibility, animation, sprite, priority,
shadow, size, collision, persistence, timing, or presentation model. The bounded H3 fixture
`sf2-map-entity-lifecycle-presentation-runtime-v1` at
`tests/fixtures/h3/map-entity-lifecycle-presentation-v1.json` additionally preserves 11 exact Map
Test 0 records: callback order, live versus zero-current-HP start/stop cursor boundary, controlled
second-compare idle seam, the two sprite selector sides, priority bytes, remove-shadow callback chain,
and source-backed temporary/restored size words plus flags-B state. An adapter MUST retain that
observation boundary without treating it as a player-visible or persistent model. The remaining
original questions are exactly `map-script-entity-lifecycle-presentation/normal-story-reachability`,
`map-script-entity-lifecycle-presentation/full-entity-state-callback-effects`, and
`map-script-entity-lifecycle-presentation/player-visible-presentation-timing-collision-persistence`.
Map-script imports MUST separately retain the seven source-named forms `shiver`, `nod`,
`followEntity`, `faceEntity`, `moveNextToPlayer`, `fly`, and `moveEntityAboveAnother` in
`sf2-map-script-engine-static-v1` at `tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.entityGestureRelationshipMotionCommandFacts`. Each record MUST preserve opcode and physical
operand widths, raw macro comments (including the two empty `moveEntityAboveAnother` comments), complete
command/program order, and the zero-inclusive program domain. The import boundary MUST retain named
handler instruction order: A6 transfer versus non-advancing probe widths, source operand/literal use
sites, branch polarity and target identity, loop target records, direct instruction/effective target
caller maps, and return boundaries. These are source-layout and control-flow records. A remake MUST NOT
normalize them into a gesture, relationship, position, following, movement, layer, facing, animation,
timing, collision, persistence, or presentation model. The bounded H3 fixture
`sf2-map-entity-gesture-relationship-motion-runtime-v1` at
`tests/fixtures/h3/map-entity-gesture-relationship-motion-v1.json` preserves 17 exact Map Test 0
records: all seven handler entries, direct/effective callback plans and observed callback order,
source-local shiver/nod/fly write seams, the non-advancing follow HP byte probe, face/move word
boundaries, and the `moveEntityAboveAnother` register record. An adapter MUST retain that controlled
observation boundary without treating it as a player-visible, collision, timing, or persistent model.
The remaining original questions are exactly
`map-script-entity-gesture-relationship-motion/normal-story-reachability`,
`map-script-entity-gesture-relationship-motion/full-entity-state-callback-effects`, and
`map-script-entity-gesture-relationship-motion/player-visible-presentation-timing-collision-persistence`.
Map-script imports MUST separately retain the twelve source-named forms `setQuake`, `fadeInB`,
`fadeOutB`, `slowFadeInB`, `slowFadeOutB`, `tintMap`, `flickerOnce`, `mapFadeOutToWhite`,
`mapFadeInFromWhite`, `flashScreenWhite`, `fadeInFromBlackHalf`, and `fadeOutToBlackHalf` in
`sf2-map-script-engine-static-v1` at `tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.screenPresentationCommandFacts`. Each record MUST preserve opcode and physical operand widths,
raw macro comments, complete command/program order, and the zero-inclusive program domain. The import
boundary MUST retain named handler instruction order: A6 transfer widths, source immediate and stored-
operand records, branch polarity/target identity, loop-target records, instruction target plus
PC-relative/direct addressing form, effective target caller maps, and return boundaries. These are
source-layout and control-flow records. A remake MUST NOT normalize them into a screen effect, map
effect, visual, palette, VDP, timing, persistence, or reachability model; all original runtime
consequences remain intentionally outside the H2 import. The bounded H3 fixture
`sf2-map-script-screen-presentation-runtime-v1` at
`tests/fixtures/h3/map-script-screen-presentation-v1.json` preserves all twelve handler entries,
source-derived quake and flash operand partitions, direct target/call/return chronology, A6/stack
boundaries, direct handler-local RAM writes, and source-set call-register words. An adapter MUST retain
that seam without treating its service-entry shims as original service behavior or as a visual/palette,
VDP, timing, persistence, reachability, or map/entity model. The remaining original questions are the
four exact `map-script-screen-presentation/*` queues in `docs/research/common-scripting.md`.
Map-script imports MUST separately retain the three source-named forms `animEntityFX`, `headshake`, and
`entityFlashWhite` in `sf2-map-script-engine-static-v1` at
`tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.entityPresentationFxCommandFacts`. Each record MUST preserve opcode, physical operand widths,
the direct versus `ENTITY_TRANSITION_` shorthand encoding, raw macro comments, complete command/program
order, and the zero-inclusive program domain. The import boundary MUST retain named handler instruction
order: A6 transfer widths, immediate/source-operand records, the separately marked `loc_46BE2` branch
chunk target, loop-target records, instruction/effective target caller maps, and return boundaries.
These are source-layout and control-flow records. The bounded one-launch H3 fixture
`sf2-map-script-entity-presentation-fx-runtime-v1` at
`tests/fixtures/h3/map-script-entity-presentation-fx-v1.json` additionally requires source-observed
transition selectors 2–7, flash duration boundaries 10/57/180, all three handler entry PCs, A6 cursor
boundaries, H1 return PCs, local branch/loop counts, compact exact call-site/target/return chronology,
and the two direct entity-byte-write seams. A remake MUST NOT normalize any of those records into an
entity effect, head motion, color change, transition meaning, visual, timing, persistence, or
reachability model. The remaining original questions are exactly the four grouped
`map-script-entity-presentation-fx/*` queues in `docs/research/common-scripting.md`.
Map-script imports MUST separately retain the three source-named primary forms `showPortrait`,
`hidePortrait`, and `menu` in `sf2-map-script-engine-static-v1` at
`tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.mapScriptUiPrimaryCommandFacts`. Each record MUST preserve opcode, physical operand widths,
raw macro comments including `menu`'s empty comment, complete command/program order, and the
zero-inclusive program domain. The import boundary MUST retain named handler instruction order: A6
transfer widths, source immediate/operand records, branch targets, source stack-pointer transfer records,
instruction/effective target caller maps with aliases, return boundaries, and the provenance join to
`dialogueCommandFacts.portraitHelper`. The bounded runtime fixture
`sf2-map-script-ui-primary-runtime-v1` at
`tests/fixtures/h3/map-script-ui-primary-v1.json` additionally retains eleven handler-local records:
the four exact H2 source-row inputs, the busy-word direct-return boundary, a controlled `d1=$FFFF`
comparison branch, hide's direct call chronology, and selector `0`/`1`/`2`/other menu partitions with
A6/stack restoration. Its parsed instruction/effective identities remain provenance, while its actual
call-site, shim-target role/PC, and callback-return PCs are bounded control-flow observations. The
session-only ROM observes those PCs and replaces only parsed shim entry spans; aliases return from their
instruction-target shim before their parsed effective target executes. Service effects are unobserved. A
remake MUST NOT treat the fixture as
portrait drawing, menu/input behavior, user choice, timing, persistence, save behavior, or reachability
evidence. Those original questions remain explicitly Unknown in
`docs/research/common-scripting.md` under the four `map-script-ui-command/*` queues.
The shared interpreter contract defines 82 primary command layouts with 133 ordered operand fields
over 234 bytes. An importer MUST preserve each field's byte width and stream offset, including
shorthand-encoded words, and MUST represent sequential, absolute-jump, conditional-absolute-jump,
and inline-action-program cursor outcomes explicitly instead of flattening every command into a
linear list.
The original corpus contains 304 such programs and 348 labels. Import validation MUST assign every
tracked command to one program, preserve the 303 `csc_end` and one absolute-jump termination shapes,
and resolve all same-program/cross-program script targets. Assembly-subroutine calls stay explicit
external edges; a modern importer MUST NOT inline or reinterpret them merely from their symbol names.
The importer SHOULD retain reference status separately from address status: 297 programs have an
incoming source reference, seven have none, and eight lack H1 entry addresses. A reference is useful
for reachability planning but MUST NOT be promoted to proof that a normal save-state route executes
the program.
Story-state imports MUST preserve the seven source forms `jumpIfFlagSet`, `jumpIfFlagClear`, primary
`csc10`, `setF`, `clearF`, `yesNo`, and `setStoryFlag` as distinct source identities. The primary
`csc10` carrier and its `setF`/`clearF` aliases MUST retain their separate physical word layouts;
aliases do not erase the primary form merely because its current source-site count is zero. An importer
MUST preserve conditional flag polarity, direct set/clear operations, the yes/no result-to-flag-89
mapping, battle-unlock translation `flag = 400 + battleIndex`, branch target/cursor shape, and direct
versus resolved service target identity. The bounded ten-case H3 fixture additionally preserves each
handler-local A6/call chronology and the final GAME_FLAGS bit for both conditional polarities, both
aliases, both yes/no outcomes, and battle-unlock base/wrap inputs. These are command-graph and
session-local mutation facts; global story ordering, save-load persistence, and player-visible prompt
presentation/timing remain outside the importer contract.
Map-script imports MUST retain `setBlocks` and `setBlocksVar` as two distinct source command forms,
not substitute a guessed map-edit operation. Each record MUST retain its two-byte opcode, six
source-labeled one-byte fields (`source x`, `source y`, `width`, `height`, `destination x`,
`destination y`), source program/command order, and the three paired A6 word reads into `d0`, `d1`,
and `d2`. The static adapter boundary MUST retain the exact direct `CopyMapBlocks` call and the
source-named bit-set sequence present only after `csc34_setBlocks`; it MUST separately retain the
helper's parsed 8-bit shifts, 6-bit row shifts, 2-byte inner offsets, 128-byte outer offsets, and
loop-counter instructions. These are source-layout and instruction-order facts. **Confirmed
(H3):** the bounded runtime contract `sf2-map-block-mutation-runtime-v1` in
`tests/fixtures/h3/map-block-mutation-v1.json` additionally requires exact forward FF0000-layout
word-copy chronology/readbacks for both forms, a cross-row rectangle, and both horizontal/vertical
overlap directions; it also requires `$34`'s observed post-copy toggle-bit order and `$35`'s absent
toggle callback. A remake MUST keep collision/pathfinding consumer effects, normal-story reachability
and map-reload/save persistence, and visible VDP presentation/cycle-pixel timing outside this contract
until the three explicit `map-block-mutation/*` Unknown questions are separately observed.
Map-script imports MUST retain the four source-named forms `newEntity`, `loadMapEntities`,
`reloadEntities`, and `loadEntitiesFromMapSetup` as distinct ordered command records in
`sf2-map-script-engine-static-v1` at `tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.entityPopulationCommandFacts`. Each record MUST preserve its physical opcode/operand widths,
source comment (including a deliberate blank comment), source program/command identity, and exact
handler cursor-read, VInt, direct-call, and source-constant records. Direct instruction target and
resolved effective target identities MUST remain distinct: in particular,
`j_InitializeMapEntities` is not a replacement spelling for the `InitializeMapEntities` effective
target. A remake MAY translate these records into its own entity-loading interface only after defining
that interface independently. **Confirmed (H3):** the bounded runtime contract
`sf2-entity-population-reload-runtime-v1` in
`tests/fixtures/h3/entity-population-reload-v1.json` requires all 12 exact ordered records from one
BizHawk launch: three `newEntity` identity-list high-water seeds, one direct-table load, one reload
through an identity-list-selected record, and all seven `loadEntitiesFromMapSetup` source input rows.
Each observation retains handler identity/return, script cursor offset, direct callback chronology and
register snapshots, selected identity-list/entity fields, and the 49-record clear-span non-empty
count. The fixture is a handler-local RAM/callback contract, not a remake entity-lifecycle, rendering,
or scene model. A remake MUST leave capacity beyond the observed high-water seed, normal-story and
save/map-reload persistence, player-visible rendering/animation/VDP timing, and collision/pathfinding
consumer effects outside this contract until the four explicit
`entity-population-reload/*` Unknown questions are separately observed.
Map-script imports MUST retain source-named `cloneEntity` as a distinct ordered record in
`sf2-map-script-engine-static-v1` at `tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.entityCloneCommandFacts`. The record MUST preserve opcode `$25`, its six-byte physical
layout, the two raw source comments, ordered source-site identity, and the complete 304-row
zero-inclusive program domain through its compact order/hash contract. It MUST also preserve the
complete `csc25_cloneEntity` section: two advancing two-byte A6 reads, ordered
`GetEntityAddressFromCharacter` calls, the source-named `ENTITYDEF_OFFSET_ENTNUM` offset-18 byte read
into D1, the following lookup, the matching byte write from D1, and return. The importer MUST keep the
four operand bytes separate from the one-byte field transfer. It MUST NOT infer a stored record span,
loop/counter, whole-record copy, entity lifecycle, allocation, collision/pathfinding, visibility,
persistence, timing, rendering, or normal-story reachability.

**Confirmed bounded runtime boundary:** `sf2-map-script-entity-clone-runtime-v1` at
`tests/fixtures/h3/map-script-entity-clone-v1.json` preserves all nine source-ordered word pairs from
one Map Test 0 launch. A consumer MUST retain the exact H1 handler entry/RTS, A6 offsets 4/8, both
input-word reads, the two call-site/lookup-entry/return-resumption PC triples, and the offset-18
source-byte/destination-byte before-and-after records. The two adjacent destination-byte sentinels are
also part of every exact record; their preservation proves only the bounded adjacent-byte condition.
The lookup body executes unmodified in this session-only trampoline harness. This fixture does not
create a lifecycle, whole-record, allocation, presentation, collision/pathfinding, persistence, timing,
or story model.

**Unknown:** `map-script-entity-clone/further-runtime-state-matrix`,
`map-script-entity-clone/further-runtime-external-consumer-matrix`, and
`map-script-entity-clone/further-runtime-context-matrix` remain the only grouped queue. A remake MUST
leave the unobserved state, consumer, and context questions outside this contract until independently
observed.
Map-script imports MUST retain the two source-named forms `roofEvent` and `stepEvent` as distinct
ordered records in `sf2-map-script-engine-static-v1` at
`tests/fixtures/h2/map-script-engine-static-v1.json`, field
`expected.mapInteractionTriggerCommandFacts`. Each record MUST preserve its two word-width operands,
the raw `trigger X`/`trigger Y` comments, source program/command identity, the two advancing A6 word
reads, both parsed `MAP_TILE_SIZE` use sites, direct target identity/order, and return boundary.
The zero-inclusive direct/effective caller maps and the source-only link to the 79-table/94-record
step corpus and 79-table/114-record roof corpus MUST remain separate from the eight command sites.
A remake MAY adapt these records only through an independently specified interface.

**Confirmed bounded runtime boundary:** `sf2-map-interaction-trigger-runtime-v1` at
`tests/fixtures/h3/map-interaction-trigger-v1.json` records six Map 02 handler invocations in one
launch. A consumer MUST retain every fixture case and its closed static and runtime record shape,
including its exact handler/call-site identity, D0/D1 word pair, hash coordinates, selected table,
stride/terminator address, `currentMapSeed`, handler return, match/terminator boundary, marker results,
toggle bits, busy word, and battle byte. The record-0 hit, terminator miss, and busy/battle gate rows
are bounded synthetic inputs; `currentMapSeed` is input identity while `currentMapAfter` is an observed
post-handler value. The two marker probes are not a complete-layout, collision/pathfinding, or callee
service-effect contract, and direct H1 JSR-site hits are not service-effect records.

The following original behaviors remain unknown rather than inferred from these checkpoints:
`map-interaction-trigger/full-layout-collision-pathfinding-effects`,
`map-interaction-trigger/presentation-audio-timing-hardware-effects`, and
`map-interaction-trigger/persistence-story-reachability`.
Map-script imports MUST separately retain the source-faithful map lifecycle records `resetMap`,
`loadMapFadeIn`, `reloadMap`, and `mapLoad` in `sf2-map-script-engine-static-v1` at
`tests/fixtures/h2/map-script-engine-static-v1.json`, field `expected.mapLifecycleCommandFacts`.
The four forms MUST remain distinct, preserving opcode/operand widths, source comments, complete
program ordering, and the exact named handler facts: cursor transfer versus non-advancing probe,
VInt operation records, branch polarity/target identity, direct instruction/effective target identity,
call order, and the physical `csc37_loadMapAndFadeIn` continuation into `csc48_loadMap`.

**Confirmed runtime boundary:** `sf2-map-lifecycle-runtime-v1` at
`tests/fixtures/h3/map-lifecycle-v1.json` records five bounded handler replays in one launch. A
consumer MUST retain every exact per-case fixture field: `id`, `handlerAddress`, `handlerReturned`,
`currentMapAfter`, `directCallSiteOrder`, `loadMapD0WordAtCall`, `loadMapD1WordAtCall`,
`tilesetD1WordAtCall`, `resetTailLoadMapD0WordAtTransfer`,
`resetTailLoadMapD1WordAtTransfer`, `viewTargetEntityAfter`, `viewPlaneAPixelX`,
`viewPlaneAPixelY`, `layoutClearStartMarkerCleared`, `layoutClearStartMarkerReplaced`,
`layoutClearEndMarkerCleared`, and `layoutClearEndMarkerReplaced`. The two `mapLoad` rows are distinct
input operands, not an equality-branch model. The marker rows are not a complete-layout or
asset-content contract, and direct JSR-site hits are not service-effect records. The bounded fade row
clears `FADING_SETTING` at its first observed `WaitForVInt`; it is not a timing or visible-fade rule.

A remake MAY introduce its own lifecycle interface only after defining that interface independently.
The following original behaviors remain unknown rather than inferred from these checkpoints:
`map-lifecycle/layout-collision-pathfinding-effects`,
`map-lifecycle/entity-reload-player-placement`,
`map-lifecycle/presentation-fade-hardware-timing`, and
`map-lifecycle/story-reachability-persistence`.
The 201 non-setup labels embedded in init sources use the same representation, so all 75 targets of
an init `script` command resolve across the two resource families.

## Geometry and Block Data

The canonical original layout is an ordered array of 4,096 words addressed as 64 rows by 64 columns.
The low ten bits select a 3x3 map block. The remaining six bits are retained as layout flags. All
decoded references MUST satisfy `blockIndex < blockset.length`; the complete original corpus already
passes this invariant.

A blockset is an ordered array of 3x3 tile-word records. Original block indices 0-2 are built-in empty,
closed-chest, and open-chest blocks constructed by the loader before compressed commands. The import
pipeline MUST expose these as normal indexed blocks after decoding so events and collision logic do
not need special negative or out-of-band identities.

The modern renderer MAY normalize tile and block flags into named fields only when each bit's meaning
is confirmed. Until then the canonical import retains the raw 16-bit word alongside any proven
interpretation. Rendering convenience MUST NOT destroy unknown bits or rewrite source evidence.

Decoded layout and block content remains private/generated. Distributable builds consume user-owned
imports or project-owned replacement assets; the repository stores schemas, aggregate fixtures, and
behavior rules rather than original map data.

## Load and Selection Order

For original-fidelity behavior, a new map load performs these conceptual steps:

1. resolve the map entry and load palette/tileset resources;
2. decode/load the selected blockset, then decode its layout;
3. apply flag-triggered block-copy records to the working layout;
4. select the area containing the requested or current position;
5. load the area's layer origins, parallax, autoscroll, layer type, music, and animation state;
6. evaluate the first matching roof record before the initial plane update;
7. run the selected setup init function after `LoadMap` returns.

The implementation MAY cache immutable decoded blocksets and layouts, but flag/step/roof copies act
on a separate 8 KiB working layout. A nonnegative map argument and a scrolling warp rebuild that
layout from source and replay persistent flag/chest state. A negative current-map reload preserves
the working layout before roof evaluation. The explicit reset operation clears the full working
layout and then uses that preserving reload path. A remake MUST model the selected path explicitly;
cache reuse cannot choose preservation based only on whether the map ID changed.

The upstream enum `MAPDATA_OFFSET_LAYOUT = 8` is not part of this contract. The confirmed entry layout
places the pointer at offset 10; no original code references the defective constant.

## Areas, Events, and Items

Area records are ordered and selected by coordinate bounds. Their canonical fields retain layer-1
bounds, layer-2/background origins, both parallax pairs, both autoscroll pairs, layer type, and default
music. Consumers MUST use the confirmed 30-byte logical shape, not the loader's partial-read cursor as
a replacement schema.

Event arrays preserve source order because original dispatch is order-sensitive:

- setup entity, zone, item, and description dispatch uses the first matching entry;
- map-setup variants scan every set flag and the last set flag in source order wins;
- coordinate `$FF` values remain explicit wildcards where the owning consumer confirms them;
- `$FFFF` terminators are serialization details and do not become gameplay records;
- searchable items keep coordinate, flag, item identity, and chest/non-chest ownership.

The event fixture includes nine executable selection cases over the complete decoded tables:
entity-specific/default, zone exact/wildcard/overlapping-first/default, and item index-mask,
facing-mismatch/default, and wildcard-facing behavior. A remake event selector MUST reproduce those
cases before presentation or story scripts are connected. A single-launch H3 matrix confirms all
nine original wrappers select the same record offsets, target addresses, entity flags, and masked
item values. It uses a private instrumented ROM whose 50-byte trampoline only supplies documented
wrapper inputs; each selected script entry is replaced with `rts`, so script side effects and
presentation remain outside this confirmed selection contract.

The same `sf2-map-events-static-v1` fixture records the source-owned event contract without treating
routes as duplicate records: 1,134 physical macro/ROM records join to 915 target profiles; 378
pointer-table category joins and 390 ordered selector-route category joins refer to their table
profiles. An importer MUST preserve each record's ordered operand text, table-relative target
expression, resolved address, source/H1 owner identity, and the distinction among physical, pointer-
weighted, and route-weighted counts. Same-address labels remain evidence, not a license to choose a
semantic alias. The map44 raw zone-default expression is an explicit exceptional boundary and MUST
remain separate from a labeled target. This contract establishes selection and target identity only;
it MUST NOT be used to infer selected script lifecycle, transition persistence, facing, or visible
presentation.

For the 684 profiles categorized exactly as entity events, the same fixture additionally defines one
source/H1 program boundary per resolved target. An importer or future execution adapter MUST retain the
entry identity, source/H1 span, ordered non-comment operations and labels, final return/direct-jump
form, instruction-versus-effective control-flow target identities, and the independent physical,
pointer-weighted, and route-weighted reference counts. It MUST preserve same-address alias labels and
the nine resolved jump-interface identities without treating an alias as a semantic behavior. The
static internal/external target classification is a physical-span relation only; runtime branch
reachability, operation effects, persistence, dialogue timing, and presentation remain outside this
design contract.

For all entity, zone, and item target programs, an adapter MUST also preserve each operation's raw
source mnemonic, neutral source family, nullable definition identity, and ordered payload-context
identity stack. The fixture's 54-mnemonic vocabulary joins raw 68000/data forms separately from
event-service, map-script, entity-action wrapper/command/payload, and stream-terminator source forms;
this is a provenance/import rule, not permission to assign behavior from a name. It MUST retain the
separate physical/setup/route-weighted operation totals. In particular, Map 21's inherited
`entityActionsWait` payload and its later `entityActions`/`customActscriptWait` segments MUST remain
source nesting, so `ac_*` payload entries are not flattened into same-level calls. A remake MUST NOT
infer macro side effects, persistence, dialogue text, timing, or presentation from this static join.
The context relation itself is imported from source aliases, cursor parsing, and terminator records;
it is not a claim that a wrapper name describes lifecycle or user-visible behavior.

For the same complete target-program corpus, an importer MUST retain each direct numeric
`chkFlg`/`setFlg`/`clrFlg` source use as a source-shaped record: parsed macro/service-definition
identity, raw operand text and numeric value, category/program/source/H1 operation identity, and
separate physical-record/setup-reference/route-reference weights. A `chkFlg` record with its
immediate static conditional consumer MUST also retain source order, raw branch mnemonic/suffix,
source polarity, and instruction/effective target identity. The `read`, `set`, and `clear` labels
remain source classifications only. A remake MUST NOT infer save persistence, story state, flag
lifecycle, operation effect, or presentation from this static contract.

An importer MUST preserve every direct `script` source reference as a distinct source-shaped edge:
the parsed service-definition identity, raw operand label, caller program/source/H1 operation
identity, instruction-label H1 address, effective map-script owner program identity, termination,
and four independent caller/reference weights. Instruction labels and effective owner programs MUST
remain separate identities so aliases are not collapsed. The complete declared label and program
domains, including zero-reference rows, are import data. This static graph MUST NOT be treated as
evidence that a reference executes, nor as an inference about timing, effects, persistence, story,
or presentation.

For zone and item targets, an importer or future execution adapter MUST retain the same
source-shaped program record: entry identity, physical span, ordered non-comment operations and
labels, terminal form, instruction-versus-effective target identity, aliases, and independent
physical/pointer-weighted/route-weighted references. The zone contract has 150 program records plus
one explicit raw-expression exclusion among 151 profiles; the item contract has 80 program records
for 80 profiles. The Map 44 raw boundary remains an unlabeled exclusion, not a fabricated program.
The source `csc_end` boundary for `Map21_DefaultZoneEvent` ends at the next H1 address before
`csub_54714`; it is a source-structure rule only. These records MUST NOT be promoted into claims about
effects, dialogue, timing, persistence, or lifecycle without runtime evidence.

Direct-`rts` entity-event targets are explicit empty handlers, not record arrays. Map 55 and the
flag-512 map 52 setup pair them with empty entity lists. The default map 52 setup instead initializes
four non-ally entities, which receive clean-state event indices 128-131; the original interaction
call chain can reach its wrapper if one is made adjacent and is not a follower. A remake importer
MUST preserve the empty-handler type and MUST NOT parse its `rts` opcode as event data. Whether the
original story route always prevents such adjacency is still Unknown and cannot become a remake
collision rule without a route-level fixture.

Flag, step, and roof records describe rectangular block copies into the working layout. Warp records
retain trigger coordinates, scroll mode, target map, target coordinates, and facing. Their original
consumers do not form one ambiguous priority list: flag copies all run in source order during layout
construction, roof-on-load uses the first containing record, and step/warp scans use the first
coordinate match. Controlled walking checks one mutually exclusive masked marker in the order
enter-caravan, enter-raft, door, warp, zone, then passability. Door processing mutates and re-reads
the target block before the warp and zone checks. A remake MUST preserve these phase and ordering
rules. The load-path rules above determine whether the resulting working-layout mutations survive;
only their VDP-visible frame timing remains outside this static contract.

The exploration wait loop polls a pending map event before A/C, and the outer loop dispatches that
event before a player action. If both are visible in the same poll, the map event wins. Exact
publication versus input-sampling timing at the original VInt boundary is presentation/runtime
evidence and remains outside this static priority rule.

## Entity Movement and Action Timing

An original-fidelity entity update MUST keep position, velocity, original travel distance,
destination, acceleration factors, motion flags, facing, layer, sprite flags, animation counter,
wait timer, and action-script pointer as distinct state. Per enabled VInt, movement updates before
the entity's action command is dispatched. A move command therefore installs its destination,
travel, and signed velocity in one tick and position begins changing on the next.

Acceleration applies in the outer three quarters of the original axis travel and deceleration in
the final quarter. Velocity is integrated only for axes whose travel is nonzero. Dominant-axis
facing uses the confirmed +/-8 magnitude boundary; animation advances by
`(abs(xVelocity)+abs(yVelocity)) >> 5`, preserves byte `-1`, and clears positive counters above 30.
Zero delta or sign crossover snaps an axis to its destination and clears that axis travel.

Entity obstruction compares candidate destinations across all other non-empty slots. With the
obstruction flag enabled, Manhattan distance below one 384-unit map tile blocks both relative and
absolute move commands: the command pointer remains at the move and the entity yields. A clear
destination advances to the following command and installs motion state. Wait commands likewise
retain their pointer while counting and advance only after their threshold is reached. Synthetic or
remake scripts MUST NOT treat `ac_pass` as a stop opcode; it advances four bytes and redispatches.

After both travel words clear, layout marker `$2000` selects layer 2, `$2400` selects layer 0, and
`$3400` sets immersed while other controlled markers clear it. A remake MAY represent those values
as typed flags but MUST preserve the raw layout word and reproduce the 13-case/20-tick state vectors
before adding interpolation or presentation adapters.

## Animation and Presentation

Animation tables contain a tileset plus cached-tile-count header and ordered replacement entries.
The upstream macro's `speed?` comment is not the contract: the consumer multiplies that word by 32
and uses it as the byte length copied into the animation cache. Each entry owns replacement start,
tile count, target start, and a logical counter. Map load initializes the counter to one. Each
enabled base VInt callback decrements it, submits the entry when it reaches zero, and reloads that
entry's counter; `$FFFF` wraps to the first entry of the current map table.

Each submitted tile queues 16 words (32 bytes) from the cache to VRAM. Animation is the last base
contextual callback, while the current VInt's DMA queue was processed earlier, so the transfer's
earliest processing point is the next enabled VInt. A modern engine MUST preserve this logical
cadence in original-fidelity mode. The four-case H3 matrix confirms target VRAM remains unchanged in
the submission frame and matches the selected cache slice after the next enabled VInt. It subtracts
a two-frame animation-disabled control because the other base callbacks naturally queue three DMA
commands per observed frame. Hardware-level scanline differences still belong to the shared
graphics fixture, but do not make the frame-level transfer delay Unknown.

Import validation MUST reject an entry whose replacement source range exceeds its cached tile
count. The original corpus already satisfies this for all 108 entries; cache sizes are 4-96 tiles,
entry transfers are 1-48 tiles, and complete logical cycles are 40, 44, or 80 enabled callbacks.

Camera interpolation, plane composition, palette application, window interaction, and VDP-visible
output are presentation adapters over this contract. They do not own map content or event state.

## Remake Acceptance

The first remake map slice is acceptable when it can:

- import one canonical generated map without committing original assets;
- preserve shared block/layout references and all raw flags;
- instantiate the 64x64 layout with every block index in range;
- select areas and ordered events using the same implementation-neutral records;
- apply a scripted block copy to an isolated working layout;
- report intentional presentation deviations separately from original facts.

Future H4 tests should reuse compact cases derived from the H2 fixtures above. Rendered screenshots
or extracted map dumps are not golden fixtures; small indices, state transitions, hashes over
user-local generated output, and placeholder-asset renders are the permitted parity surfaces.
