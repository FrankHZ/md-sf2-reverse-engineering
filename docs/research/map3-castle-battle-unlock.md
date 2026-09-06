# Map 3 Castle to Battle 01 Unlock — Static Fallback

- Status: **Confirmed** H2 static contract; natural execution **Unknown**
- Fixture: `sf2-map3-castle-battle-unlock-static-v1`
- Command: `uv run sf2 h2 map3-castle-battle-unlock`
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract boundary

This H2 fallback deterministically derives a public-safe source/H1/ROM contract over 53 ordered source
inputs, 23 named functions, and 32 H1 projection fields. It retains only source identities and hashes,
symbols, addresses, flags, coordinates, logical inputs, structural rows, digests, collision topology,
entity occupancy, zones, warps, and program semantics. It contains no source prose or payload bytes,
ROM/H1/layout/block payload, capture, movie, state, log, callback, observer, bootstrap, status,
cleanup, emulator case, runtime golden, or input driver.

The accepted R1, R2, and R2a fixture IDs and digests are preflight dependency guards only. They do not
promote a runtime chronology. In particular, this source-derived legal graph is not an observed
continuous Map 3 → Battle 01 route.

## Confirmed static derivation

**Confirmed:** six source programs are ordered and H1-bound: `cs_51652` at `0x51652`,
`cs_53104` at `0x53104`, `cs_53996` at `0x53996`, `cs_52F0C` at `0x52F0C`, `cs_52F40` at
`0x52F40`, and `cs_53EF4` at `0x53EF4`. The Map 19/20 init and Map 19/21 entity selectors retain
their source guard order. `cs_53EF4` sources `setStoryFlag 1` (F401) and the surrounding Map 21
handler sources F256; that is program/flag semantics only.

**Confirmed:** the selected default zone tables contain 13 rows/52 encoded bytes. Their decoded
zone-cell denominators are Map 3/19/20/21 = 15/7/4/0. The static route has four admitted zone rows:
Map 3 Event 4, then the two Map 19 royal-route default cells, then the Map 19 tower-route default
cell. The table selectors, target kinds, intersections, and two objective-order topology checks are
source-derived.

**Confirmed:** five retained R2 warp predicates are joined to only their navigation terminals,
including the Map 3 wildcard-x predicate. Map 19 owns 13 physical entity records (106 bytes), fixed,
walking, and left/right-loop occupancy domains, plus source-defined phase visibility. Map 20 owns eight
records with the post-`cs_53996` zero-intersection guard; Map 21 retains the two-stage entity-128
interaction metadata.

**Confirmed:** the legal source-derived graph has 16 ordered segments and 110 logical inputs, with
SHA-256 `02C5C3A720F1C61356F7B030BE1E0194BBAE3E241E036CD53E1E0640571393D0`. It models collision,
areas, the Map 3 school-door copy, restored guards, occupancy, zone bits, and exact/wildcard terminal
warp predicates. It is a reproducible static reachability artifact, not replay cadence or proof that a
caller executes the sequence in that order.

**Confirmed:** `map20-to-map19-royal-return.to.facing` is the static destination-facing
annotation sourced from `warpFacing LEFT` in the fifth record of
`data/maps/entries/map20/6-warp-events.asm`. H1 symbol `Map20s6_WarpEvents` is `0xA53DA`;
the 11 eight-byte rows end with a two-byte terminator. Record 5 starts at `0xA53FA` and its
facing field at offset 6 (`0xA5400`) is `2`, resolved through `LEFT` in `sf2enums.asm`.
The record's trigger is `(23,37)`, target operand is `MAP_GRANSEAL_CASTLE_2F` (19), and
destination is `(23,3)`, with no scroll and a zero reserved byte. The `mWarp`,
`warpNoScroll`, `warpMap`, `warpDest`, and `warpFacing` macro order places facing after the
two trigger, one scroll, one map, and two destination bytes; the existing map-content encoder
and canonical map-import decoder are reused to check that relationship.

The parser checks every Map 20 record's ordered field operands against the decoded encoding and
the complete H1/ROM table before selecting record 5. It then derives `static.warps.map20Royal`
and both graph-edge facing annotations from that record; the retained R2 predicate still owns the
map/coordinate join. A mismatched graph consumer is rejected before the route digest or golden
comparison. These annotations describe the warp field, not observed outgoing or incoming player
orientation. Natural init and any later facing changes remain **Unknown**.

## Retained dependency compatibility

The admission, turn-control, action-effect, action-completion, turn-finalization, and victory-return
fixtures retain parent identities. Their existing builders propagate this correction only through
those retained digest fields. Their behavior, terminals, counts, and Unknowns do not change.

The player-ready fixture re-anchors its R2b/R2c/R3a `fixtureSha256` references under `static.retained`
and `expectedObservation.records[0].retained`, plus `static.sourceProjectionSha256`, whose existing
calculation includes R3a's retained R2c projection. This updates the expected comparison contract's
static references; that re-anchoring itself is not a new runtime observation. The completed original
run and its raw observation remain unchanged. Under ADR 0012/0014, this candidate separately passes
one dependency-selected `uv run sf2 h3 map3-battle01-player-ready` run: one case, 46 logical inputs,
one launch, complete status/golden/restoration checks, zero retained callbacks, and deletion of the
session ROM. The Lua Console output is empty and the process exits. The candidate's receipts are
preserved separately; this does not establish any new natural-continuity claim.

A complete structured comparison against accepted base `62718b6b4dcb4ca100aafca734e32e082237353f`
permits only those seven scalar pointers in the H3 fixture. Its cases, controlled Map 21 bridge,
input plan, warps, RAM, functions, source hashes, and every runtime observation field are unchanged.
The observer source is unchanged; rebuilding its configuration changes only the three
`extension.retained` fixture digests. The original natural-continuity Unknowns remain in force.

The original-reference scenario descriptor retains the finalization and victory fixtures by raw
file hash. Its two `/staticFixtures/*/sha256` references were already stale at the accepted base:
preflight completed with `static-fixture-identity`, structured `FAIL`, and `ProcessStarts=0`, despite
CLI exit 0. Only those two hashes are re-anchored; the complete remaining descriptor is unchanged.
The selected `uv run sf2 h3 original-reference-replay-scenario-api --preflight-only` now reports
`PASS`, `ProcessStarts=0`. This is descriptor validation, not original-reference replay execution.

## Unknown runtime boundary

Natural execution and caller order, runtime endpoint, final `WaitForEvent`, F401/F256 runtime
continuity, RA-03/RA-04 continuity, and R2c readiness remain **Unknown**. In particular this owner
does not establish natural Maps 21 → 40 → 57 traversal, Battle 01 admission, cutscene execution,
state persistence, input cadence, or rendered/audio/timing behavior. Those are grouped H3 questions,
not an H3 artifact added by this H2 slice.

## Reproduction

Run the command above with the normal private ROM and pinned upstream inputs. The parser validates
canonical ROM/upstream identity, source/H1/ROM seams, retained-prefix digests, structural closure, and
the checked-in canonical fixture before reporting success.
