# Map and Exploration Contract

- **Confirmed original behavior:** 79 map definitions, shared block/layout ownership, source-form
  areas/events/items/animations, 64x64 decoded layouts, setup selection, and documented first-match
  dispatch rules
- **Unknown original behavior:** transition-time event precedence and persistence, direct-`rts` event
  reachability, exact animation/scroll frame timing, and final VDP-visible rendered parity
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
  `tests/fixtures/h2/canonical-map-import-v1.json`.

The last fixture is the executable serialization of this contract. Its full generated payload stays
private under `local/derived/`; only aggregate structure and provenance are tracked.

The canonical import now resolves all 64 setup routes and 126 setup definitions. A setup definition
references six independently shared resources: entities, entity events, zone events, area
descriptions, item events, and initialization function. The 15 map IDs with no original route keep a
null route. Selection scans every flag variant in source order and retains the last set flag; direct
return handlers remain explicit empty handlers rather than being replaced by guessed defaults.
Initialization resources retain ordered operations and complete branch targets, including the one
confirmed cross-function return edge. A remake importer MAY translate recognized operations into a
typed command IR, but MUST retain unknown operand text and MUST NOT infer script persistence or frame
timing from opcode names alone.
Standalone script resources likewise retain ordered commands, operand text, and resolved references
between all 178 labels. These are importable command graphs, not proof that a modern engine may skip
the original interpreter's state, wait, camera, dialogue, or presentation sequencing.

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
6. initialize the selected setup's entities and init function;
7. evaluate roof state before presenting the settled map.

The implementation MAY cache immutable decoded blocksets and layouts, but flag/step/roof copies act on
a per-map working layout. Cache reuse MUST NOT leak mutated layout state between map loads unless a
later H3 fixture confirms that persistence in the original.

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

Flag, step, and roof records describe rectangular block copies into the working layout. Warp records
retain trigger coordinates, scroll mode, target map, target coordinates, and facing. The exact order
between simultaneously eligible transition events is **Unknown** and MUST remain a queued parity
case rather than an arbitrary priority baked into the data model.

## Animation and Presentation

Animation tables contain one tileset/speed header and ordered replacement entries. The original
advances them from VInt and performs DMA. A modern engine MAY schedule equivalent updates in its frame
loop, but original-fidelity mode needs a shared timing fixture before claiming frame-exact parity.

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

Future H4 tests should reuse compact cases derived from the three H2 fixtures above. Rendered screenshots
or extracted map dumps are not golden fixtures; small indices, state transitions, hashes over
user-local generated output, and placeholder-asset renders are the permitted parity surfaces.
