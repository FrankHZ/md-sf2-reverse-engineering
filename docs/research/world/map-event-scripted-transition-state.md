# Map-Event Scripted Transition State

Evidence date: 2026-08-28.

## Confirmed static program corpus

**Confirmed:** the complete retained `sf2-map-events-static-v1` mother denominator contains 914
target programs. Exactly one is positive for the selected map-script and entity-action operation
families, leaving 913 zero contexts: `Map21_DefaultZoneEvent` at `$545B6` in
`data/maps/entries/map44/mapsetups/scripts.asm`. Its enclosing map-script entry is `$54578`; the
selected stream ends exclusively at `$54714`, terminates with `csc_end` at `$54712`, and occupies
350 encoded bytes. The one physical record has four setup references and four route references, so
its 87 source/H1/ROM operation rows have weights 87 physical, 348 setup, and 348 route.

**Confirmed:** the ordered stream has 27 distinct source macro/command definitions and 87 public
operation rows. It closes the exact macro counts, numeric and enum operands, four payload contexts
(one inherited at program entry), and five source-resolved pointer targets. The public fixture has
only structural identities, counts, operands, enum values, hashes, addresses, and linkage; it
contains neither decoded dialogue nor source/ROM payloads.

## Confirmed handler joins

**Confirmed:** source/listing resolution finds 19 non-null definition-handler entries before golden
comparison. The public `retainedHandlers` projection contains 17 of those entries plus the two
map/entity dispatcher table joins. The 111 owned address-bearing anchors are exactly 87 operation
rows, 19 definition-handler entries, and five pointer targets; dispatcher joins are retained-owner
links, not extra owned anchors. The contract confirms stream shape, macro encoding, source/H1/ROM
parity, payload nesting, retained handler entry identities, and stream termination only; it does not
infer an effect from a handler name or operand.

## Unknown runtime meaning

**Unknown:** natural reachability; caller entry state; actual execution; entity identity, movement,
and facing; camera progression; map-block and subroutine effects; map/entity load effects; warp
completion/destination; flag lifetime/persistence; dialogue/audio/presentation; timing/cadence; and
post-script control/endpoint remain the ordered 14-key `unknowns` register in fixture
`sf2-map-event-scripted-transition-state-static-v1`. Per ADR 0016 this is a deferred conditional
ambiguity register, not a default blocker or an automatic H3 queue.

## Provenance and reproduction

Pinned source: `ShiningForceCentral/SF2DISASM` `master`
`c834c652b6862bc5679fd7f69a38a7093206efc6`; local US ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`; H1 listing
`build/sf2build-h1.lst`. The complete eight-file source surface and source identities are fixture
pinned in `tests/fixtures/h2/map-event-scripted-transition-state-static-v1.json`. Reproduce with:

```powershell
uv run sf2 h2 map-event-scripted-transition-state
```
