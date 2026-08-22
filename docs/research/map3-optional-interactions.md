# Map 3 optional interactions: static inventory

Status: **Confirmed (static)** for the registered Phase 2 H2 inventory;
findings current at 2026-08-22.

This is useful full-game research, not ADR 0010 selected 2A milestone scope, a
readiness blocker, or a 2B/2C product decision. ADR 0010 2A excludes unproven
optional NPC, menu, item, and unrelated exploration work. That product boundary
does not establish original-game route relevance, so this inventory retains
`Unknown` where the accepted current-main evidence does not prove it.

## Confirmed static contract

`src/sf2tool/h2/map3_optional_interactions.py` parses the complete default-Map
3 surface plus its Map 3 item include-site ownership into
`tests/fixtures/h2/map3-optional-interactions-static-v1.json`. The fixture is
canonical UTF-8 JSON and is recursively exact-value and exact-order closed by
`schemas/h2/map3-optional-interactions-static-fixture.schema.json`. It carries
public structural symbols, source ownership, aggregate classifications, and
the already-public entity-event program/text/flag/menu mapping only. It also
names the two item include-site symbols and public H1/ROM entry addresses. It
does not carry private numeric entity rows, map placements, action payloads,
area-description table entries or text-index sets, dialogue prose, assets, ROM
payload, or emulator captures.

The confirmed denominator is 13 paths: eight default-Map-3 paths, the Map 3
item include-owner path, and four generic macro/consumer paths.

| Inventory | Confirmed count |
| --- | ---: |
| Default Map 3 source paths | 8 |
| Map-entry include-owner paths | 1 |
| Generic macro/consumer paths | 4 |
| Entity definitions | 19 |
| Entity-event routes, including default | 17 |
| Routes callback-observed on the accepted opening | 2 |
| Entity-event routes still route-unknown | 15 |
| Area-description/interactable records | 17 |
| Item placements | 2 |

The parser derives the default `ms_map3` pointer row, source-order variants
`609`, `506`, and `543`, then its six slots. It derives the four-byte entity
event and six-byte item-event record shapes from macros plus their generic
dispatch use sites: `$FD` default selector, target offsets two/four,
respectively, and indirect `jsr (a0)` dispatch. It also derives the six-byte
area-description record, first text base 423, two display-text calls, and the
`j_ChurchMenu` to `ChurchMenu` jump-interface alias.

Every entity-event route includes its table record identity, entity ID/facing,
target program, operation order, numeric text indices, flag check/branch shape,
flag effects, script targets, and any menu-call shape. That surface is already
publicly owned by `sf2-map-events-static-v1`. Comments are stripped before
parsing and cannot create records or operations.

The parser nevertheless parses all 19 entity rows, 17 area-description rows,
and two item rows in memory to derive their exact denominators, macro/consumer
shapes, source-owner/source-kind counts, and route-relevance counts. The
tracked projection deliberately retains only those safe aggregates: entity
macro/action-kind taxonomy; the six-byte area-description/effect shape; and
item macro/terminator/consumer ownership with the two include-site identities
and entry addresses. Exact private values are re-parsed locally against the
pinned source and are neither committed nor hashed in the public fixture. The
detailed rows remain private/local under the accepted entity-data and
area-description-routing owners, and item placements remain private/local under
the accepted Map and Exploration Contract
(`docs/design/contracts/map-exploration.md`) content owner.

## Route relevance labels

**Confirmed:** `Map3_EntityEvent0` and `Map3_EntityEvent15` are classified
`mandatory-observed-opening`. This is strictly the accepted current-main
callback observation from the Map 3 admitted-start/natural-route/messenger
evidence, not a claim about all playthroughs.

**Unknown:** the other 15 entity-event routes (including the default route),
all 17 area descriptions, and both item placements are `unknown` for route
relevance. Static location, text indices, a church-menu call, a flag check, or
a direct-return default handler does not prove original optionality or
mandatory status.

## Pinned provenance and cross-checks

The read-only source baseline is
`https://github.com/ShiningForceCentral/SF2DISASM.git` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; the input identity is the tracked
US ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The bounded default paths are `data/maps/mapsetups.asm`, Map 3 pointer,
entities, entity-events, descriptions, and item-events setup files,
`data/maps/entries.asm`, and the Map 3 chest/other item tables. The two item
include-site symbols are `Map03s7_ChestItems` at ROM `0x9793A` and
`Map03s8_OtherItems` at ROM `0x97940`; their leaf tables remain private-source
provenance rather than public data payload. The generic paths are
`sf2mapsetupmacros.asm`, `sf2mapmacros.asm`,
`code/common/scripting/map/mapsetupsfunctions_1.asm`, and
`code/common/tech/jumpinterfaces/s05_jumpinterface.asm`.

The fixture names the independent accepted-main cross-check owners available
without consuming unmerged work: `sf2-map-data-static-v1`,
`sf2-map-events-static-v1`, `sf2-map3-battle01-natural-route-runtime-v1`, and
`sf2-map3-messenger-acceptance-runtime-v1`. They cross-check shared map table
identity, event-dispatch semantics, and the two bounded runtime observations;
they do not convert any remaining route classification into a fact.

The source checkout and any ROM are read-only private inputs. No private file,
dialogue prose, extracted asset, capture, trace, or ROM byte is copied into the
five tracked outputs.

## Reproduction

With a read-only pinned checkout at `$upstream`, run the focused public suite:

```powershell
uv run pytest tests/python/test_map3_optional_interactions.py
uv run ruff check src/sf2tool/h2/map3_optional_interactions.py tests/python/test_map3_optional_interactions.py
uv run sf2 h2 map3-optional-interactions --upstream-path $upstream
```

The source-backed fixture equality seam is
`verify_map3_optional_interactions(Path($upstream))`; it parses the inventory,
validates the exact fixture schema, and fails on semantic drift. The
registered Phase 2 H2 inventory exposes this parser as the H2-only
`map3-optional-interactions` command; it creates no runtime fixture or product
readiness claim.

## H3 question queue

- **Reachability and route relevance:** which of the remaining entity, area,
  and item records can occur on supported routes, under which state.
- **Flag-dependent state and effects:** observed flag reads/writes, branch
  outcomes, script effects, and persistence for each reachable route.
- **Rendered dialogue/menu/presentation/timing:** rendered text, menu choices,
  presentation, control handoff, and timing; none is inferred from text IDs.
- **Item/default-dispatch outcomes:** chest/hidden-item behavior and the
  runtime result of the otherwise direct-return default item event.

## Candidate registration boundary

The H2 registration uses fixture ID
`sf2-map3-optional-interactions-static-v1`, verifier
`src/sf2tool/h2/map3_optional_interactions.py`, and this owning document. It
adds document/evidence associations to the five established `map.data.ms-map3`
setup-table records and two neutral `map.data.static` include-site records for
`Map03s7_ChestItems` and `Map03s8_OtherItems` in `data/maps/entries.asm`.
The leaf item files remain provenance only. This static registration changes no
H3 queue, design contract, product decision, or readiness boundary.
