# Map and Exploration Contract

- **Confirmed original behavior:** 79 map definitions, shared block/layout ownership, source-form
  areas/events/items/animations, 64x64 decoded layouts, setup selection, documented first-match
  dispatch rules, static transition-consumer priority, load-path-specific layout persistence, and
  batched frame-level entity movement/action timing
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
  `tests/fixtures/h3/entity-movement-matrix-v1.json`.

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
Story-state imports MUST preserve conditional flag polarity, direct set/clear operations, the
yes/no result-to-flag-89 mapping, and battle-unlock translation `flag = 400 + battleIndex`. These are
command-graph facts; global story ordering and save persistence remain outside the importer contract.
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
