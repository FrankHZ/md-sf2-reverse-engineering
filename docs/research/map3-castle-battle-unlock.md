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
SHA-256 `D5AAD8F84D72F033012DD769C34FD6FCA8E8BC32EC81912712D9EFFDD5D3C5D4`. It models collision,
areas, the Map 3 school-door copy, restored guards, occupancy, zone bits, and exact/wildcard terminal
warp predicates. It is a reproducible static reachability artifact, not replay cadence or proof that a
caller executes the sequence in that order.

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
