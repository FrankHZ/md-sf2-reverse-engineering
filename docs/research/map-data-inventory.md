# Complete Map ASM Inventory

- Status: **Confirmed** for the complete 1,390-file ASM boundary, build reachability, internal-symbol
  addresses, map/setup file classes, pointer/include counts, global table row counts, all 64 setup
  routing rows, last-set-flag selection, 126 six-pointer setup tables, event dispatcher record shapes,
  the nine-case entity/zone/item runtime dispatch matrix,
  all 125 entity-list sources/980 physical entity records, all 263 entity/zone/item event sources with
  1,134 physical records, all 684 entity-event and 150 zone-event/80 item-event target programs plus
  one explicit raw-expression exclusion, all 75 area-description targets/227 physical entries, and all 84 init
  sources/90 setup-callable entry points with 597 physical operations, 126 pointer-table joins, and
  130 ordered selector-route joins, all 47 standalone setup-script files/8,058 statements,
  all 662 source-form content sections, and all 154 private blocks/layout payloads
- Status: **Inferred** for event-script side effects, follower/entity collision state, and transition persistence
- Status: **Unknown** for direct-`rts` entity-event reachability through normal story routes, sequenced-orientation consumption,
  nonstandard description callers, presentation timing, rendered layout parity, and VDP timing
- Evidence date: 2026-07-23
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Complete Build Graph

`data/maps` contains 1,390 ASM files and 44,977 lines. Eight files are included directly by the
original layout: three root containers/tables and five global tables. Their transitive include graph
has 1,382 edges to 1,382 unique targets and reaches every ASM file in the directory. The graph
contains 79 map directories, 662 map-content files, and 720 map-setup files.

This batch therefore provides 1,390/1,390 deterministic H2 inventory. It does not claim 1,390 strict
symbol bindings:

- 727 files define at least one global label in their own source and receive a representative H1
  address plus research-index record;
- 662 map-content files contain bodies whose labels are attached at their include sites in
  `entries.asm`; they are hashed and graph-checked but do not receive a falsely relocated symbol;
- `mapsetupsstorage.asm` is the sole unlabeled include container.

The distinction keeps strict data-file reach conservative while still closing the complete source
discovery boundary.

## Static Shape

`entries.asm` defines 79 map pointer slots, includes 662 source-form map sections, and references 154
private binary payloads. The setup storage includes 720 internally labeled files. The top-level
setup selector contains 64 map rows, 66 flag rows, and 64 map terminators.

The five global tables contain 57 debug-map slots, three flag-switched map rows, 13 overworld-map
rows, four raft-reset rows, and 23 save-point rows. These are table-shape facts only. The inventory
fixture does not reproduce map layouts, dialogue, entity lists, event scripts, descriptions, item
placements, or binary payloads. The separate map-content rail now re-encodes all 662 source-form
sections and locally byte-compares all 154 private payloads without tracking their content. Full file
hashes and include edges remain in ignored `local/derived/map-data-static.json`.

## Map Content Closure

All 79 map entries are 46-byte records: six palette/tileset bytes followed by ten longword slots at
offsets 6 through 42. All entries and the 79-pointer `pt_MapData` table byte-match the ROM. The 662
source-form sections re-encode to 12,576 matching bytes, while the 77 block and 77 layout payloads
total 193,678 locally verified bytes. The tracked fixture contains only aggregate counts and sizes.

The actual layout pointer is at offset 10. Upstream declares the unused `MAPDATA_OFFSET_LAYOUT` as 8;
the entry encoding and sequential loader both prove 10, and no code references the bad constant.
Record layouts, consumer evidence, content counts, private-data handling, and the remaining grouped
runtime queue are owned by [map-content.md](./map-content.md).

## Setup Selection and ROM Parity

`GetCurrentMapSetup` at ROM `0x4779E` loads the setup table for `CURRENT_MAP`. A map row starts with a
map word plus its default longword pointer, continues through zero or more flag/pointer pairs, and ends
at `$FFFD`; `$FFFF` ends the complete table. A map absent from the table returns `ms_Void`, whose first
word is `$FFFF`.

The selector does not stop at the first true flag. It loads the default pointer, scans every flag row
in source order, and replaces the candidate after each successful `CheckFlag`. Therefore the
**last set flag in source order wins**. Four flag rows intentionally point back to their map's default
setup: map 7/flag 702, map 33/flags 783 and 22, and map 40/flag 507. Those later aliases can undo an
earlier variant; treating variants as an unordered dictionary or first-match chain would be wrong.

The 64 map rows and 66 flag rows expand to 910 bytes at `MapSetups` (`0x4F6E2`) and byte-match the
canonical ROM. Their 130 pointer references resolve to 126 unique setup tables because of the four
aliases. Every table contains these six big-endian longword slots:

| Offset | Owner |
| ---: | --- |
| `0` | entity list |
| `4` | entity events |
| `8` | zone events |
| `12` | area descriptions |
| `16` | item events |
| `20` | initialization function |

All 126 tables, 756 target slots, and 3,024 pointer bytes are derived independently from source/H1
symbols and byte-match the ROM. The complete routes and pointers are written only to ignored
`local/derived/map-setup-static.json`; the tracked fixture retains compact selection cases, counts,
addresses, and rules.

## Event Dispatcher Shapes

The same H2 rail binds the dispatcher entries and checks their source order:

| Dispatcher | Entry | Static matching rule |
| --- | ---: | --- |
| zone | 4 bytes | `$FD` default; `$FF` wildcards for X/Y; first matching entry |
| item | 6 bytes | `$FD` default; `$FF` wildcards for X/Y/facing; masked item index must match; first match |
| entity | 4 bytes | entity byte or `$FD` default; byte 1 bits 0/1 turn toward actor/restore facing |
| area description | 6 bytes | coordinate word; byte 2 gates on `d6`; byte 3 chooses text indices or relative function |

Initialization calls the init-function pointer at byte offset 20 (the sixth four-byte slot) unless the
selector returned `ms_Void`; entity-list lookup returns the pointer at byte offset 0. These control-flow
and record-layout facts are **Confirmed** statically. The meaning of the area
description byte-2/`d6` check remains **Unknown**, as do side effects inside the selected scripts and
their visible timing.

## Entity List Streams

The 126 setup tables contain 126 entity-pointer references but only 125 unique targets:
`ms_map21_Entities` is deliberately reused. The source boundary likewise contains 125
`s1_entities*.asm` files. Decoding from each unique pointer until the consumer's `$FF` first-byte
terminator reaches every source-owned record and no bytes outside that record set.

The physical source contains 980 eight-byte entity records:

| Encoding | Physical records | Per-list references |
| --- | ---: | ---: |
| fixed action pointer | 803 | 808 |
| `$FF` walking payload | 174 | 175 |
| `$FE` sequenced-action pointer | 3 | 4 |
| **Total** | **980** | **987** |

The seven-reference difference is intentional suffix sharing. Nine entity source fragments omit
their own `msEntitiesEnd`: eight fall directly into an adjacent terminator-only variant, while
`ms_map17_Entities` contributes five prefix records and falls into the seven-record
`ms_map17_flag505_Entities` suffix before their shared terminator. A per-file parser would either
invent nine missing terminators or lose this variant composition.

There are 116 unique terminator addresses, 30 empty selected lists, and at most 31 records in one
selected list. `InitializeMapEntities` consumes records in stream order, masks X/Y to six bits,
scales them by `MAP_TILE_SIZE`, and routes special mapsprites through the special-entity declaration
path. The complete numeric records stay in ignored `local/derived/map-entities-static.json`; the
tracked fixture contains only counts, encodings, fallthrough relationships, addresses, and rules.

The same ROM-backed decode now closes the initial map-sprite domain for all 980 physical records.
They use 113 distinct IDs: 977 records are regular IDs below 240, while the three special records use
251, 252, and 255. The high regular range stops at 236, so none of the shared-sentinel regular IDs
237-239 or unbacked special IDs 240-250 occurs in any selected entity-list source. This proves the
initial-map-record boundary only; later cutscene, entity-action, combatant-derived, and direct code
writes remain separate assignment domains.

## Entity, Zone, and Item Event Tables

The event-table rail follows all three event slots from the 126 setup tables, decodes each unique
target from ROM until its `$FD` default record, and checks every physical record against its owning
macro use site: source path/line, macro name, ordered operands, table-relative expression, record
index, decoded address, and resolved target. Relative branch words resolve from the start of the
table, matching the dispatcher rather than the current record address. The parsed macro definitions
in `sf2mapsetupmacros.asm` bind each target operand position, `$FD` default marker, emitted byte
width, and directive order before the source/ROM comparison.

| Category | Source files / unique targets | Decoded tables | Physical records | Pointer-table targets | Pointer-weighted records | Selector-route targets | Route-weighted records |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| entity | 105 | 103 | 850 (747 specific + 103 default) | 126 | 998 | 130 | 1,031 |
| zone | 84 | 84 | 202 (118 specific + 84 default) | 126 | 313 | 130 | 326 |
| item | 74 | 74 | 82 (8 specific + 74 default) | 126 | 140 | 130 | 144 |
| **Total** | **263** | **261** | **1,134** | **378** | **1,451** | **390** | **1,501** |

All 1,134 physical records join to 915 resolved-target profiles. The 378 pointer-table category
joins and 390 ordered selector-route category joins retain table identity, including direct-`rts`
stubs, without duplicating physical records. Every ordinary resolved address has an exact H1/source
label owner in this pinned corpus; same-address labels remain an explicit label array, and a missing
or multi-source owner is a construction failure rather than an inferred target meaning. These source,
H1, ROM, and join facts are **Confirmed**. The evidence is the pinned
`ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`sf2mapsetupmacros.asm` macro definitions, the 263 `s2_entityevents*.asm`,
`s3_zoneevents*.asm`, and `s5_itemevents*.asm` sources, `build/sf2build-h1.lst`, ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`, and
`uv run sf2 h2 map-events` (observed: 1,134 / 915 / 378 / 390 / zero unresolved / zero ambiguous).

The difference between 263 unique targets and 261 decoded tables is explicit: entity-event targets
`ms_map52_EntityEvents` and `ms_map55_EntityEvents` are two-byte direct-`rts` stubs, not `$FD`-ended
record streams. They are referenced three times. The map 52 default setup pairs its stub with four
entities, while map 52/flag 512 and map 55 pair theirs with empty lists. The source/ROM shape and
pairing are **Confirmed**; why the non-empty map 52 setup cannot safely reach this dispatcher remains
partly explainable rather than wholly unknown. `GetActivatedEntity` scans 48 entity slots, rejects the
player and followers, and accepts an entity whose Manhattan distance from the activated point is less
than `MAP_TILE_SIZE`. `ProcessPlayerAction` passes every nonnegative result through
`GetEntityEventIndex` and then `RunMapSetupEntityEvent`. The index scan switches to event index `$80`
after the 32 ally slots. Combined with clean-state stream-order initialization, map 52's four regular
non-ally records therefore receive event indices 128-131. An adjacent non-follower under controlled
or arbitrarily mutated state is **Confirmed** able to reach the wrapper; the direct `rts` bytes are
not a valid `$FD`-ended table. Whether normal story entrances, terrain, the zone cutscene, and flag 512
always prevent that adjacency remains **Unknown** and is the narrower retained runtime question.

Map 44 has the other source exception: its zone default is written as raw `dc.w` values instead of
`msDefaultZoneEvent`, and its relative word `byte_54868+4-ms_map44_ZoneEvents` resolves to `0x5486C`,
four bytes into the cutscene entity list beginning at H1 address `byte_54868 = 0x54868` in
`data/maps/entries/map06/mapsetups/s1_entities.asm:19`. Its profile deliberately retains the raw
expression boundary instead of inventing a label at the interior address. The upstream source labels
this as a bug. The exact bytes, offset 1,044, base owner, and resolved target are **Confirmed**; no
intended behavior is inferred from the bad pointer.

The complete tables and decoded branch targets stay in ignored
`local/derived/map-events-static.json`. The tracked fixture
`tests/fixtures/h2/map-events-static-v1.json` carries the complete structured semantic object; its
closed output/fixture schemas use reusable category record definitions plus compact exact-order arrays.
It preserves macro contracts, source/ROM/owner joins, target profiles, pointer and selector-route
multiplicities, and both exception families without redistributing event content.

The H2 rail now evaluates nine representative queries directly against those complete decoded tables.
Entity cases cover a late specific match and the default; zone cases cover exact coordinates,
`$FF` Y wildcard, an exact row that precedes an overlapping wildcard row, and the default; item cases
cover the `$7F` index mask, a facing mismatch that falls through to default, and `$FF` facing. Every
case retains its selected setup/table, physical record address, record kind, flags where applicable,
and resolved target address. These are **Confirmed** static first-match contracts and the input table
for the grouped H3, not claims about the called script's side effects.

### Entity-Event Target Program Corpus

**Confirmed — source/H1 program boundary:** the 684 `recordTargetProfiles` whose sole category is
`entityEvents` now join one-for-one to source program boundaries. Their 850 physical records, 998
pointer-table-weighted records, and 1,031 selector-route-weighted records remain separate reference
counts; one program is never copied for each reference. The 684 entry labels occupy 87 exact source
paths, while the event-table inventory still retains all 105 entity-event table sources. For every
program, the contract stores the entry symbol/H1 address/source path/line, the first following
source `End of function` boundary, H1 end address, and individual physical encoded span. These program
spans total 8,928 bytes and are distinct from table record byte widths or reference weights.

**Confirmed — non-comment operation/control-flow corpus:** those boundaries contain 1,015 labels and
2,624 source operations. The source-faithful operation records retain order, source line, H1 address,
raw mnemonic, legal `.b`/`.w`/`.l`/`.s` suffix when present, and split operand text without comments.
The static count is 1,486 ordinary operations, 208 conditional branches, 147 unconditional branches,
96 direct calls, 61 direct jumps, and 626 return instructions. Every program has a source/H1-proven
final return or direct jump boundary. This is a control-flow inventory only: it does not name a
macro's gameplay behavior or reproduce dialogue content.

**Confirmed — target and alias identities:** the 512 non-return control-flow sites retain both the
instruction operand identity and the effective identity. The complete declared instruction and
effective sets each contain 332 symbol/address identities. The nine used `j_` interfaces retain their
exact jump-interface source owner, `jmp` definition, target operand, H1 address, and effective source
labels; same-address aliases remain label arrays rather than a chosen canonical semantic name. Both
internal and external target-total maps carry every declared identity with zero-valued branch/call/jump
fields where absent; their observed site split is 355 internal and 157 external. Internal means only
that the effective H1 address lies inside the parsed physical program span, not that the branch is
runtime-reachable.

**Confirmed — provenance and construction guards:** the corpus is reproduced from the USA ROM
SHA-256 above; `ShiningForceCentral/SF2DISASM` `master`
`c834c652b6862bc5679fd7f69a38a7093206efc6`; the 105
`data/maps/entries/*/mapsetups/s2_entityevents*.asm` table sources plus the 87 resolved body-owner
paths; `code/common/tech/jumpinterfaces/*.asm` for used aliases; and `build/sf2build-h1.lst`.
`uv run sf2 h2 map-events` builds a single H1 label/function-end index and validates every parsed
source operation against its ordered H1 use-site before fixture comparison. A source opcode, operand,
or order mutation therefore fails construction instead of merely changing a golden comparison.
The tracked fixture and its recursively closed output/fixture schemas record the full corpus; compact
program/label/operation/target-total order keys constrain exact sequence without expanding a schema
tree per operation. Observed command: `uv run sf2 h2 map-events` (684 / 2,624 / 8,928 / 512 / zero
unresolved program profiles). Evidence date: 2026-07-23.

### Zone- and Item-Event Target Program Corpora

**Confirmed — profile and non-program boundary:** the 151 zone-target profiles split into 150
source/H1 program boundaries and one explicit non-program exclusion. The program records carry 201
physical references, 309 pointer-table-weighted references, and 322 selector-route-weighted
references; the exclusion separately carries 1, 4, and 4. The profiles therefore retain the complete
zone weights of 202 physical, 313 pointer-table-weighted, and 326 selector-route-weighted records
without pretending that the raw expression is a callable program. The 80 item-target profiles all
join to programs and carry 82, 140, and 144 references respectively. These reference weights remain
separate from stored program spans.

**Confirmed — zone corpus:** the 150 zone programs occupy 76 exact source paths, 251 labels, 809
non-comment operations, and 2,934 physical encoded bytes. There are 477 ordinary operations, 123
conditional branches, 18 unconditional branches, 41 direct calls, one direct jump, and 149 returns.
The 183 branch/call/jump sites split into 141 internal and 42 external effective targets; the
zero-inclusive instruction and effective target maps each declare 119 symbol/address identities. The
seven used jump-interface aliases are `j_GetMaxHp`, `j_SetCurrentHp`, `j_GetMaxMp`,
`j_SetCurrentMp`, `j_YesNoPrompt`, `j_GetCurrentHp`, and `j_GetItemInventoryLocation`; every alias
record retains its instruction identity, `jmp` definition, and resolved effective target rather than
assigning it a behavior.

**Confirmed — item corpus:** the 80 item programs occupy 73 exact source paths, 94 labels, 146
non-comment operations, and 414 physical encoded bytes. Their static instruction counts are 47
ordinary operations, 9 conditional branches, 4 unconditional branches, 6 direct calls, no direct
jumps, and 80 returns. The 19 branch/call sites split into 13 internal and 6 external effective
targets. Both zero-inclusive target maps declare 18 instruction/effective symbol-address identities;
the two used jump-interface aliases are `j_GetItemInventoryLocation` and `j_RemoveItemBySlot`.
Internal versus external here is only a physical-span classification, not a reachability claim.

**Confirmed — exceptional source-stream boundary:** `Map21_DefaultZoneEvent` begins at H1 address
`0x545B6` (345,526) in `data/maps/entries/map44/mapsetups/scripts.asm` and its literal source
`csc_end` at line 111 is an explicit terminal operation at `0x54712` (345,874). Its program span ends
exclusively at the next H1 address, `0x54714` (345,876), so it contains 87 operations and 350 bytes
and does not absorb the adjacent `csub_54714` body. This source-stream terminator is a parsed
boundary form, not evidence of an effect, dialogue, timing, or lifecycle. The one raw zone exclusion
is the existing Map 44 expression boundary at target `0x5486C` with base `0x54868`, no H1 target, and
no invented interior label; its source owner and separate 1/4/4 reference counts are exact.

**Confirmed — provenance and construction guards:** the zone corpus is reproduced from all
`data/maps/entries/*/mapsetups/s3_zoneevents*.asm` table sources and 76 resolved body-owner paths;
the item corpus uses the corresponding `s5_itemevents*.asm` sources and 73 body-owner paths. Both use
the USA ROM SHA-256 above, `ShiningForceCentral/SF2DISASM` `master`
`c834c652b6862bc5679fd7f69a38a7093206efc6`, used
`code/common/tech/jumpinterfaces/*.asm` definitions, and `build/sf2build-h1.lst`. The H2 parser
matches every source opcode, operands, and operation order to its H1 use-site before comparing the
golden fixture. It guards ordinary `End of function` boundaries and, for source `csc_end`, the next
H1 address; it also resolves each alias at its pinned `jmp` definition. Source/H1 operand, opcode,
order, boundary, or alias-definition mutations therefore fail construction before fixture comparison.
The same recursively closed output and fixture schemas use shared closed program/exclusion records and
compact exact-order arrays. Observed command: `uv run sf2 h2 map-events` (zone 150 / 809 / 2,934 /
183; item 80 / 146 / 414 / 19; one explicit zone exclusion). Evidence date: 2026-07-23.

### Complete Operation Vocabulary and Definition Join

**Confirmed — complete source-faithful vocabulary:** the 684 entity, 150 zone, and 80 item target
programs contain exactly 2,624, 809, and 146 physical source operations respectively: 3,579 in the
three-category union and 54 normalized mnemonics. Every operation retains its pre-existing source/H1
identity and now carries one of nine neutral families plus either a source definition identity or
`null` for raw 68000/data forms. There are zero unclassified or family/definition-ambiguous operations.
The exact category and independently weighted counts are:

| Family | Entity / zone / item physical operations | Physical-record weighted | Setup-record weighted | Route-record weighted |
| --- | ---: | ---: | ---: | ---: |
| raw 68000 control flow | 1,138 / 332 / 99 | 2,260 | 2,963 | 3,161 |
| raw 68000 instruction | 183 / 70 / 19 | 433 | 496 | 500 |
| event service macro | 1,303 / 318 / 28 | 2,333 | 3,102 | 3,366 |
| map-script macro | 0 / 64 / 0 | 64 | 256 | 256 |
| entity-action wrapper | 0 / 3 / 0 | 3 | 12 | 12 |
| entity-action payload command | 0 / 16 / 0 | 16 | 64 | 64 |
| entity-action command | 0 / 3 / 0 | 3 | 12 | 12 |
| stream terminator | 0 / 1 / 0 | 1 | 4 | 4 |
| data directive | 0 / 2 / 0 | 2 | 2 | 2 |
| **Total** | **2,624 / 809 / 146** | **5,115** | **6,911** | **7,377** |

The weighted columns are sums of each program's separately parsed physical/setup/route reference
counts, not storage spans, macro byte sizes, or a multiplication inferred from a total. Each program
also retains its own four-operation weight tuple, so a changed profile/use relationship fails before
the golden fixture comparison.

**Confirmed — non-CPU definition/use join:** the 34 used definitions are parsed once from pinned
`sf2macros.asm` and `sf2cutscenemacros.asm`. The seven event-service definitions are exactly
`sndCom` (line 27), `chkFlg` (32), `setFlg` (37), `clrFlg` (42), `txt` (52), `clsTxt` (57), and
`script` (62); the contract records only each use's ordered operands, formal ordinal positions, emitted
source statements, and `trap`/`lea` service target. It does not reproduce dialogue or infer a flag,
text, script, sound, or persistence effect. The 16 map-script macros join the maintained
`sf2-map-script-engine-static-v1` macro/handler catalog, and the three `ac_*` commands join the
maintained `sf2-entity-action-scripts-static-v1` handler-binding catalog. The two used wrappers,
five movement/`endActions` payload forms, and `csc_end` retain source definition identity without a
guessed runtime meaning.

For every macro use, the map-event parser substitutes the parsed operand positions into the parsed
definition and compares the complete emitted statement sequence, statement order, H1 macro-expansion
addresses, and emitted span to `build/sf2build-h1.lst`. Thus a smallest source-definition opcode,
operand-position, or emission-order mutation fails construction before fixture comparison. Raw 68000
instructions/control flow and `dc` directives stay separate rather than being assigned an invented
service definition. Provenance is USA ROM SHA-256 above; `ShiningForceCentral/SF2DISASM` `master`
commit `c834c652b6862bc5679fd7f69a38a7093206efc6`; the two named macro files; each operation's
source path/line and H1 address; and `uv run sf2 h2 map-events` (observed: 54 / 3,579 / 34 / zero
unclassified / zero ambiguous). Evidence date: 2026-07-23.

**Confirmed — Map 21 payload boundaries:** `Map21_DefaultZoneEvent` inherits an
`entityActionsWait` payload opened at
`data/maps/entries/map44/mapsetups/scripts.asm:22` and closed by `endActions` at line 29. Its later
same-source segments are `entityActions` lines 65–68, `customActscriptWait` lines 80–83 ending in
`ac_end`, and `entityActions` lines 84–92. The operation contract records the complete ordered stack
of source context identities; `ac_setSpeed`, `ac_jump`, and `ac_end` are therefore command-payload
forms, not false same-level 68000 calls. This is only stream structure: action timing, movement,
wait behavior, and visible results remain unclaimed. Evidence is the pinned source/H1 rows and the
same command above. Evidence date: 2026-07-23.

**Confirmed — payload wrapper/terminator derivation:** the four retained wrapper-to-context pairs
are parsed, not maintained as a second literal map. The maintained
`sf2-map-script-engine-static-v1` catalog identifies `customActscript`/`customActscriptWait` as
`csc14` aliases with the `inline-action-program` handler cursor flow, and
`entityActions`/`entityActionsWait` as `csc2D` aliases with the sequential handler flow. The
maintained `sf2-entity-action-scripts-static-v1` contract supplies the inline terminator macro and
word. The extractor requires that parsed `ac_end` emits that parsed word, and that the named `csc14`
handler compares it at `(a6)+`, branches on not-equal, then returns on equality. Independently, its
`csc2D` guard requires the ordered `move.b (a6)+,d1`, negative `bmi` branch, later
`move.b (a6)+,d2`, and branch-target `addq.l #1,a6`; it therefore records the two-byte
`endActions` sentinel only when the parsed macro definition lies after `csc2D` and before the parsed
stream terminator and emits a high-bit word. Alias identity, cursor-flow, terminator definition, or
either handler use-site mutation fails construction before fixture comparison. This confirms only
stream parsing and cursor movement, not movement, timing, entity lifecycle, or visible behavior.
Provenance: pinned `ShiningForceCentral/SF2DISASM` `master`
`c834c652b6862bc5679fd7f69a38a7093206efc6`,
`sf2cutscenemacros.asm:141-146,304-309,588-596,635-637`,
`code/common/scripting/map/mapscriptengine_1.asm:502-525,751-768,774-793`, and
`uv run sf2 h2 map-events` (observed four context records; 54 / 3,579 / 34 / zero unclassified / zero
ambiguous). Evidence date: 2026-07-23.

### Direct Flag-State Source Surface

**Confirmed — source-label-defined access inventory:** the complete 684 entity, 150 zone, and
80 item target-program corpus contains 493 direct numeric uses of the three parsed event-service
macros: 316 `chkFlg` sites classified as source-label `read`, 169 `setFlg` sites classified as
source-label `set`, and 8 `clrFlg` sites classified as source-label `clear`. The category split is
190 / 80 / 0 entity, 118 / 84 / 8 zone, and 8 / 5 / 0 item (read / set / clear). These classifications
preserve the upstream macro labels and parsed service definitions; they do not establish an in-game
meaning, persistence class, or lifecycle for any numeric operand.

**Confirmed — parsed definition, operand, and consumer relationship:**
`sf2macros.asm:32-45` supplies the three source definitions in order: `chkFlg`, `setFlg`, and
`clrFlg`, each with one formal operand and a two-statement `trap`/`dc.w` emission template. The
fixture records the parsed definition ID, source macro, trap operand, formal ordinal, and exact
emission statements for every direct site, along with its program identity, source/H1 operation
identity, and raw decimal or `$`-hex numeric operand. Every one of the 316 `chkFlg` sites has the
next ordered operation as a conditional branch consumer: 264 `bne.s`, 49 `beq.s`, and 3 `bne.w`.
Each record retains the next-operation source order, mnemonic/suffix, equal/not-equal source
polarity, and complete instruction/effective branch target identity. The static exception counters
are all zero: missing immediate operation, non-conditional operation, non-adjacent operation,
unrecognized conditional mnemonic, and missing target identity.

**Confirmed — domains, weights, and construction guard:** the observed numeric union has 151
operands: 128 occur in `read`, 112 in `set`, 5 in `clear`, 114 in the source-label write union, and
91 in both the read and write unions. The independently retained weighted counts are:

| Source-label access | Physical program occurrences | Physical-record weighted | Setup-reference weighted | Route-reference weighted |
| --- | ---: | ---: | ---: | ---: |
| read | 316 | 513 | 754 | 839 |
| set | 169 | 230 | 373 | 404 |
| clear | 8 | 14 | 21 | 23 |

The parser builds zero-inclusive per-program totals for all 914 target programs and ordered
per-operand totals for the observed union, then re-derives all sites, domains, branch-consumer
records, and four distinct weight kinds from the parsed macro definitions and program use sites
before fixture comparison. A smallest source mutation to a direct macro emission/order, numeric
operand, branch opcode/polarity/target, operation order, or program reference weight fails this
construction guard. Provenance: USA ROM SHA-256 recorded above;
`ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `sf2macros.asm:32-45`; all parsed
`data/maps/entries/*/mapsetups/s2_entityevents*.asm`, `s3_zoneevents*.asm`, and
`s5_itemevents*.asm` sources plus resolved target-program body owners;
`build/sf2build-h1.lst`; and `uv run sf2 h2 map-events` (observed 493 / 151 / 316 immediate branch
consumers). Evidence date: 2026-07-27.

### Direct `script` Source-Reference Graph

**Confirmed — parsed service and target-owner join:** `sf2macros.asm:62-65` defines the one
source macro `script` with formal operand 1 and the exact ordered emission `lea \1(pc),a0` then
`trap #MAPSCRIPT`. Across all 914 target programs, its 147 parsed source/H1 use sites split 52
entity, 87 zone, and 8 item sites. Each record retains the raw operand label, caller program/source
and H1 operation identity, instruction-label H1 address, and the distinct effective map-script
program owner (ID, entry label/address, source path, and termination). This is a source-reference
join; it does not establish that a reference executes, its timing, any script effect, story
reachability, save persistence, or presentation.

**Confirmed — aliases, complete target domains, and weights:** the 147 sites use 138 distinct
operand labels which resolve to 135 distinct effective owners. The zero-inclusive instruction-target
table covers all 348 labels in the maintained map-script `labelOwners` map; the zero-inclusive
effective-target table covers all 304 declared map-script programs. Thus interior labels remain
distinct instruction identities even when they share an owner program; the fixture never collapses
an operand label into its owner entry label. The four retained counts are:

| Caller category | Physical program occurrences | Physical-record weighted | Setup-reference weighted | Route-reference weighted |
| --- | ---: | ---: | ---: | ---: |
| entity | 52 | 57 | 104 | 123 |
| zone | 87 | 138 | 233 | 249 |
| item | 8 | 9 | 15 | 15 |
| **Total** | **147** | **204** | **352** | **387** |

**Confirmed — construction and reconciliation guard:** the rail parses the service definition once,
then re-derives ordered sites, all 914 caller totals, all 348 instruction-label totals, all 304
effective-owner totals, category weights, and source/H1 owner identities before fixture comparison.
A direct macro emission/order, caller operand/order/H1 address, label-owner mapping, target H1
address, effective owner, site identity/order, or any reference weight mutation fails that guard.
Provenance: USA ROM SHA-256 above; `ShiningForceCentral/SF2DISASM` `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; `sf2macros.asm:62-65`; all event target-program
owners; `build/sf2build-h1.lst`; and the maintained
`sf2-map-script-engine-static-v1` `programCorpus.labelOwners`/`programs` records. Observed command:
`uv run sf2 h2 map-events` (147 / 138 / 135 / 348 / 304). Evidence date: 2026-07-27.

### Grouped H3 Runtime Questions

- **Unknown — normal-story direct-`rts` reachability:** observe map 52's non-empty setup across normal
  entry, terrain, cutscene, and flag-512 contexts; this is a caller/state question, not evidence that
  the two-byte target is a record table.
- **Unknown — selected script effects and persistence:** observe state changes, transitions, and
  save/reload persistence after the already-confirmed selection boundary.
- **Unknown — facing and presentation timing:** observe entity-facing restoration, portrait behavior,
  and visible timing after dispatch; static flags and target ownership do not establish these effects.

## Area-Description Wrappers and Tables

The 126 description-slot references resolve to 75 unique callable targets, not directly to record
tables. Thirty-eight targets are two-byte `rts` stubs. The other 37 are identical-shape 16-byte
wrappers: load a per-map description-text base into `d3`, load an internal table into `a0`, execute
`nop`, and jump to `DisplayAreaDescription`. All 75 callable bodies, the wrapper immediates and target
addresses, and the 37 internal `$FD00`-terminated tables byte-match the ROM.

| Payload | Physical entries | Setup-level references |
| --- | ---: | ---: |
| text offsets | 206 | 426 |
| relative function | 18 | 31 |
| `d6`-conditioned relative function | 3 | 4 |
| **Total** | **227** | **461** |

Text entries add byte 4 to global investigation-text base 423 and byte 5 to the wrapper's `d3`
description base. Function entries add their signed word to the internal table base. There are 37
two-byte terminators, 1,436 physical table bytes, and at most 23 entries in one table. Thirty-five
callable targets are reused; across all setups, 67 references select a wrapper and 59 select an
empty direct-return stub.

The formerly unknown byte-2 check is now closed for the normal exploration call graph. The sole
assembled `j_RunMapSetupAreaDescription` call is reached after `explorationvints.asm` executes
`moveq #1,d6` with the source comment “No entity event.” `DisplayAreaDescription` skips a matching
entry when byte 2 is nonzero and `d6` is nonzero. Therefore the three `msDescFunctionD6` entries
(map 31 byte 1, maps 41/42 byte `$FF`) are **Confirmed** unreachable through that one normal call
path. Their behavior under a direct or deliberately mutated `d6=0` caller remains **Unknown**; no
emulator case is justified until such a caller is found or intentionally isolated.

Complete decoded text indices and function targets remain in ignored
`local/derived/map-descriptions-static.json`; the tracked fixture retains counts, the three
conditioned entries, wrapper/dispatcher rules, and call-graph evidence.

## Initialization Callables

**Confirmed — provenance:** the tracked `sf2-map-init-static-v1` fixture is reproduced from the USA
ROM SHA-256 above, `ShiningForceCentral/SF2DISASM` `master`
`c834c652b6862bc5679fd7f69a38a7093206efc6`, the H1 listing,
`data/maps/entries/*/mapsetups/s6_initfunction*.asm`, `data/maps/mapsetups.asm`, the six-pointer
tables, `sf2enums.asm`, `sf2macros.asm`, `sf2cutscenemacros.asm`, and
`RunMapSetupInitFunction` in `code/common/scripting/map/mapsetupsfunctions_1.asm`. Reproduce with
`uv run sf2 h2 map-init`.

**Confirmed — route and profile join:** the selector source order produces 130 route references (64
default plus 66 flag rows). Each joins one of 126 unique six-pointer tables, that table's init-function
pointer at byte offset 20 (the sixth four-byte slot), and one of 90 target profiles; four route
references reuse a pointer-table identity. The 90
profiles retain source path, symbol, H1 address, direct-`rts` shape, physical source-operation
boundary, exact operation-index sequence, flag macro operands, script targets, direct-call targets,
and zero-inclusive family counts. Six profiles begin at internal labels, so the 84 physical source
bodies remain distinct from the 90 callable entry boundaries. There are 56 non-`rts` profiles and 34
direct-`rts` profiles; the 126 pointer-table references select 82 and 44 of those profiles,
respectively. The 597 physical operations expand to 654 profile operations and 973 pointer-table-
weighted operation occurrences; the 130 route references weight to 1,100 occurrences. These are
reference counts, not physical ROM byte spans or runtime execution counts. The exact route-weighted
family map is 203 flag reads, 53 flag writes, 150 scripts, 65 direct calls, 43 entity/position
commands, two warp/transition commands, 22 presentation commands, 102 data-movement operations, 280
branches/jumps, and 180 terminals; its sum is construction-guarded against the 1,100 route-weighted
occurrence count.

**Confirmed — operation inventory:** the 597 physical operations are fully classified by exact parsed
token, macro, or call form: 101 `chkFlg` reads; 36 source-level flag-write macro uses (`setFlg`,
`clrFlg`, or `setStoryFlag`); 80 `script` invocations; 45 direct calls; 12 `setPos` commands; two
`warp` commands; 20 `sndCom`/`txt`/`clsTxt` commands; 68 arithmetic/data-movement operations; 131
branches or jumps; and 102 `rts`/`csc_end` terminals. The canonical unclassified list is empty. The
130 branch targets resolve, including 129 local operation-index targets and the H1-addressed
cross-function target `return_5C4EC` from `ms_map52_InitFunction`; no branch target is silently
downgraded to an unresolved string.

**Confirmed — call and script identities:** the 45 direct-call sites retain both instruction and
effective target identity. `j_alt_YesNoPrompt` resolves to `alt_YesNoPrompt`, and
`j_FadeOut_WaitForP1Input` resolves to `FadeOut_WaitForP1Input`; the instruction and effective maps
are complete exact six-key observed-target maps, not broader declared target domains. All 80 `script`
call sites carry their owning source function and physical operation index. Their 75 targets resolve
as 63 embedded init-source programs and 12 standalone map-setup programs; a missing target definition
now fails contract construction, so the unresolved target count is zero. This is target ownership and
source-form resolution only, not a claim about any call's effect.

**Confirmed — dispatcher use sites:** `MAPSETUP_OFFSET_INIT_FUNCTION` is parsed from `sf2enums.asm`
and cross-checked with the independently parsed `sourceFacts.pointerLayout` row `{ sourceOrder: 5,
name: initFunction, offset: 20 }` and the symbolic `movea.l` load use site. The ordered wrapper record
is save registers, call `GetCurrentMapSetup`, compare `-1`, branch non-missing setups to the pointer
load, branch missing setups to restore/return, indirect `jsr (a0)`, restore, and `rts`. The extractor
rejects enum, layout-row, load-operand, opcode, branch polarity/target, indirect-call, restore, or
order mutation while constructing this record, before fixture comparison. This confirms source control
flow, not a claim that a selected callee has only one observable runtime effect.

**Unknown — grouped H3 queue `map-init-effects-and-presentation`:** (1) init-script side effects and
transition persistence; (2) entity/position mutation order and visibility timing; and (3) fade,
audio, and text presentation sequencing. No lifecycle or presentation meaning is inferred merely from
macro or target names.

The complete source-shaped object, its recursive closed schemas, all source-order constraints, and
the golden fixture are tracked. The reproducible full payload remains
`local/derived/map-init-static.json`; it contains metadata and normalized source facts, not extracted
copyrighted game assets.

## Standalone Setup Scripts

The final 47 non-pointer-table files under `mapsetups` are `scripts*.asm`. They contain 8,398 source
lines, 8,058 normalized statements, 139 distinct command names, and 178 global labels. Every file's
representative label and every global label resolve in the pinned H1 listing. The label families are
141 cutscene labels, six cutscene subroutines, four cutscene-entity blocks, two entity-action scripts,
13 ordinary subroutines, eight local control-flow labels, two palette blocks, and two other labels.

Across the complete 720-file setup source boundary, 127 of those labels are referenced from another
file and 51 only from their defining file. The graph contains 146 cross-file and 92 same-file lexical
references; no label is unreferenced. Of the 75 distinct targets called by init `script` operations,
12 live in these standalone files and 63 live in their owning `s6_initfunction*.asm` sources.

The most frequent commands are text, facing/action, waits, movement, and positioning; the exact
command map includes 122 `csc_end` markers and 16 `rts` statements. These counts and reference edges
are **Confirmed** static structure, not story semantics or frame timing. All 178 labels now own a
non-empty ordered program in the local output, jointly covering all 8,058 operations. Operand symbol
resolution identifies 100 operations with exactly 100 references to another standalone label; the
edge stores both symbol and H1 address without assuming what the command does. Full programs, command
counts, all 178 H1 addresses, body hashes, and reference-source lists remain in ignored
`local/derived/map-scripts-static.json`.

## Concentrated Queue

No emulator was launched for the accepted static fixtures in this document. Setup priority and
dispatcher order are now closed by source/H1/ROM evidence. Remaining questions are grouped as:

1. sequenced-entity orientation, normal-story direct-`rts` entity-event reachability, and description/init functions
   under nonstandard or mutated callers;
2. follower/map-entity collision state, selected event/description-script
   side effects, transition persistence,
   and roof/step/warp precedence;
3. walking/special-sprite and portrait/text/entity-facing presentation timing;
4. rendered block/layout parity and animation VDP frame timing in the later graphics matrix.

Entity streams, init and standalone labeled command flow, every source-form setup/content
family, and both private block/layout bitstreams are now closed statically, including 77 complete
decoder passes. The map script target graph is now statically closed; remaining state persistence,
nonstandard callers, and presentation behavior belong in the prepared grouped runtime matrices.

The first runtime slice now replays ten setup-selection cases from one natural
`GetCurrentMapSetup` entry at ROM `0x4779E` and observes `a0` at the common return seam `0x477E2`.
The case table is derived from the accepted H2 selector model and covers a missing map, default rows,
single and multiple set flags, last-set-flag-wins, and later aliases that restore a default pointer.
Each replay may change only `CURRENT_MAP` and the 128-byte game-flag bitset; the original row scan,
flag tests, pointer overwrites, register restoration, and return execute unchanged. All ten observed
addresses match the H2 model in one BizHawk launch, so missing-map fallback, default selection,
single-flag variants, last-set-flag-wins, and later aliases that restore defaults are **Confirmed**.

The accepted harness saves its shared core state from the debug Map Test 0 number-prompt frame loop,
then reaches the selector through `DebugSetFlag`, `ExplorationLoop`, and the original entity/init
callers on every replay. BizHawk forbids `memorysavestate.savecorestate()` inside an execution
callback; the rejected first attempt exposed that exception and is not part of the evidence. The
observer therefore requests the snapshot from the outer frame loop rather than weakening the
natural-call boundary or invoking the selector synthetically.

The second runtime slice reuses that natural Map Test prompt boundary for six
`RunMapSetupInitFunction` cases. At wrapper entry it changes only `CURRENT_MAP` and the same flag
bitset, observes the original `jsr (a0)` at ROM `0x47512`, and requires the common wrapper return at
`0x47514`. The missing-map case executes no indirect call. Map 0 and map 3 default/609/506/543 cases
each execute exactly one H2-modeled target and return, covering two direct-`rts` targets, three active
targets, and the flag-506 target that runs a map script. These dispatch facts are **Confirmed** in one
BizHawk launch. Story, entity, audio, fade, and persistence effects under the controlled map/flag
combinations remain outside this fixture and are not promoted from opcode names or successful return.

The third runtime slice consumes all nine H2 event queries in one BizHawk launch. A private derived
ROM replaces the init wrapper's indirect call and register restore with an absolute call to a
50-byte trampoline in byte-checked `FF` alignment padding. Lua writes one 16-byte input record per
replay; the trampoline loads the documented `d0`-`d5` inputs, calls the unmodified entity, zone, or
item wrapper, restores the original init-wrapper registers, and returns. Each of the nine expected
script entries is independently checked against its original first word and replaced with `rts`, so
the observer reaches the selected entry but never executes its story or presentation body. The
original input ROM is never modified.

All nine wrappers preserve the H2-modeled record offset in `d7` and reach the modeled target address.
The entity cases also preserve flags 1 and 0; the three item cases expose masked indices 112, 112,
and 125. This **Confirms** late specific/default entity selection, exact/`$FF`/overlapping-first/
default zone selection, and item index-mask/facing-default/`$FF`-facing selection. Direct-`rts`
table reachability, script side effects, portrait/facing behavior, and transition persistence remain
outside the fixture.

## Harness Performance

This batch more than doubles the research-index record count. The index verifier now parses the H1
listing into a symbol-address map once per run instead of rescanning the complete listing for every
record. The provenance checks are unchanged: each indexed symbol must still occur in its claimed
source file and at the fixture/index address in the H1 listing.

## Reproduction

```powershell
uv run sf2 h2 map-data
uv run sf2 h2 map-setup
uv run sf2 h2 map-entities
uv run sf2 h2 map-events
uv run sf2 h2 map-descriptions
uv run sf2 h2 map-init
uv run sf2 h2 map-scripts
uv run sf2 h2 map-content
uv run sf2 h3 map-setup-selection
uv run sf2 h3 map-init-dispatch
uv run sf2 h3 map-event-dispatch
uv run sf2 research-index test
```
