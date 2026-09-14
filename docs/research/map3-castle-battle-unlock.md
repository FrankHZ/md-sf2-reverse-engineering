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

## Map 21 entity 135: unchecked lookup and conditional continuation

This source-only clarification uses the pinned source and ROM identity above. It does not change the
H2 fixture or add a runtime observation. **Confirmed:** an absent logical entry is not a lookup
failure branch in the original `csc23_setEntityFacing` handler. Conversely, neither a remake's
missing-entity rejection nor a reference driver's skipped command proves original behavior.

### Lookup widths and address

**Confirmed:** `code/common/scripting/map/mapscriptengine_1.asm` (paths in this section are relative
to upstream `disasm/`) owns `csc23_setEntityFacing`, `GetEntityAddressFromCharacter`,
`AdjustScriptPointerByCharacterAliveStatus`, and `UpdateEntitySprite_0`. The lookup's twelve
instructions are retained by the existing `map_script_entity_clone.py::_lookup_guard` and
`tests/fixtures/h3/map-script-entity-clone-v1.json`; the facing handler's eight instructions are
retained by `expected.entityPlacementCommandFacts` in the existing map-script-engine H2 fixture.

For incoming D0 low byte `c`, the lookup first masks **D0.w** with `0x00FF`, tests **D0.b** as
signed, and subtracts `ENTITY_ENEMY_INDEX_DIFFERENCE=96` at **byte** width only for bit-7-set
selectors. It reads one byte `s` from `ENTITY_INDEX_LIST[q]`, where `q=c` for `c<128`, otherwise
`q=c-96`. This is direct indexing, not a search or a bounds-checked lookup. It then shifts **D0.w**
left by `ENTITYDEF_SIZE_BITS=5` and adds that word to the short-addressed `ENTITY_DATA` base. The
saved D0 is restored with low word `s`; its high word is not cleared by this routine.

The constants in `sf2const.asm` and `sf2enums.asm` give the following physical 24-bit RAM addresses.
Neither zero nor `0xFF` is tested as an absence sentinel by this lookup.

| Selector/table value | Index byte address | Returned record address | Facing byte address (`+0x10`) |
| --- | --- | --- | --- |
| `135`, `s=0` | `0xFFB140 + 39 = 0xFFB167` | `0xFFA902` | `0xFFA912` |
| `135`, `s=0xFF` | `0xFFB167` | `0xFFC8E2` | `0xFFC8F2` |
| `135`, any byte `s` | `0xFFB167` | `0xFFA902 + 32*s` | `0xFFA912 + 32*s` |

The `0xFF` result is outside the normal 49 records cleared by `ClearEntities`, although still within
the physical RAM address range. It is not evidence for a valid entity 255, a safe no-op, or a general
missing-entity fallback. The returned A5 is an address, not a success/failure result.

`csc23_setEntityFacing` pre-reads the selector, passes a two-byte operand skip to the alive-status
helper, then consumes one selector byte and one facing byte. Selector `135` has bit 7 set, so the
helper returns before any ally HP lookup: the dead-ally skip does not apply. The handler resolves A5,
writes the facing byte (`DOWN=3` here), and calls `UpdateEntitySprite_0`. That helper passes the same
address and facing to `ChangeEntityMapsprite` in
`code/common/scripting/entity/entityscriptengine_2.asm`. No missing-entry skip or error branch is
present at this seam. Full sprite-callee effects and presentation are not established by this
address calculation; continuation below is conditional on its return.

### Why the default population leaves index 39 zero

**Confirmed:** `code/common/scripting/entity/entityfunctions_1.asm::ClearEntities` clears 49
32-byte records and, separately, sixteen longwords of `ENTITY_INDEX_LIST`: all 64 identity bytes
start at **zero**, not `0xFF`. `code/common/scripting/map/mapfunctions.asm::InitializeMapEntities`
initializes followers starting at physical slot 1, writes regular non-ally mappings starting at table
offset 32, and declares the leading/player record at physical slot 0 after the source terminator.

`data/maps/entries/map21/mapsetups/s1_entities.asm::ms_map21_Entities` has one regular non-ally
record, the guard at `(5,16)` facing Down, followed by the terminator. It assigns alias128 at table
offset32; it does not assign alias135 at offset39. The follower allocator in
`code/common/scripting/map/followersfunctions_1.asm` and `data/scripting/entity/followers.asm` uses
ally offsets 0–29 and follower offsets 30, 31, 62, 63; the raft path writes offset63. These paths do
not populate offset39. Thus **after this default population and with no intervening index-table
write**, byte `0xFFB167` remains zero and `setFacing 135,DOWN` writes the leading/player record's
facing, not a separately instantiated guard135. This is a conditional source derivation, not a RAM
sample at a naturally reached `cs_53EF4` PC. Other selected populations or later table mutations
must not be silently assigned this initial value.

### Flag predicates and return boundaries

**Confirmed:** the default `s2_entityevents.asm::ms_map21_EntityEvents` points alias128 at
`Map21_EntityEvent0`. That handler is physically in `s2_entityevents_506.asm`; the filename does not
mean flag506 must be selected. The handler's actual F608/F256 predicates determine this branch:

| Entry F608 | Entry F256 | Handler path, assuming called services return |
| --- | --- | --- |
| Clear | Clear | Text568, set F256, text569; no `cs_53EF4` or F401 write |
| Clear | Set | Text569; no script or flag write |
| Set | Set | Text579, then return; no script or flag write |
| Set | Clear | Text579, call `cs_53EF4`, then set F256 **after that script returns** |

Within `cs_53EF4`, the source order is `entityActionsWait 128` with `moveRight 1`, then
`setFacing 135,DOWN`, then `setStoryFlag 1` (F401), then script end. Therefore absent135 is not itself
a source-defined reason to stop before F401: under the initialized-zero condition above, its facing
command resolves slot0. However, F401 still follows the completed action wait and returned facing
handler, and F256 still follows the returned whole script. Do not replace those dependencies with
unconditional flag writes, nor use the static sequence as proof of natural F401/F256 continuity.

### Bounded applicability to Map 3 entity 142

The accepted [entity142 owner](./map3-entity142-interactable-reference.md) establishes the opening
route's alias142 → table offset46 → physical slot17 join. It does not establish a permanently live
slot17 after later lifecycle commands. In `data/maps/entries/map03/mapsetups/s6_initfunction.asm`,
the F1-set branch runs `cs_513BA` (`hide 142`) before the F603-set branch calls `MoveEntityOutOfMap`
with 142.

**Confirmed static:** `csc2E_hideEntity` resolves the current physical slot through the lookup above.
`code/common/scripting/entity/entityfunctions_2.asm::HideEntity` moves that record off-map and
replaces every identity-table byte matching that slot with `0xFF`. Thus a removed142 entry is
different from Map21's never-assigned zero entry. `MoveEntityOutOfMap`, in
`code/common/scripting/map/mapsetupsfunctions_1.asm`, instead calls
`GetEntityIndexForCombatant` and `SetEntityPosition` in
`code/gameflow/battle/battlefunctions/battlefunctions_0.asm`. The former translates the logical byte
through the table without rejecting `0xFF`; the latter shifts the resulting word by five, adds
`ENTITY_DATA`, and writes four position/destination words. **If offset46 contains `0xFF`**, this
chain writes `0x7000` at physical addresses `0xFFC8E2`, `0xFFC8E4`, `0xFFC8EE`, and `0xFFC8F0`;
it does not resolve the player record or report missing142. This is a conditional memory effect,
not an accepted visible outcome or permission to reproduce unchecked RAM access in a remake.

### Evidence and runtime admission limit

The retained clone rail observes seeded identity-table mappings; the retained placement rail
observes alive/dead ally cursor and field effects. Neither observes naturally missing135 at this
Map21 call. The population rail does not promote this specific natural call state either. Existing
fixtures are retained evidence, not commands rerun for this clarification.

The natural call-time table byte, complete sprite-callee outcome, and continuous F401/F256 endpoint
remain **Unknown**. Static source answers the immediate lookup/width/conditional-continuation
question, so it does not meet the static-insufficiency limb of ADR0014's runtime-admission gate.
No new H3 run or scenario is admitted here. A future proposal must name the remaining behavior
decision, why this static and retained evidence is insufficient, and its smallest observation;
first assess reuse of the existing clone/placement/population rails and obtain main-gate runtime
authorization. The post-F603 comparison does not open a separate Map3 scenario queue.

For this source-only clarification, inspect the named symbols at the exact pinned Git object; this
does not require rebuilding H1 or launching the historical H2/H3 commands. For example, after setting
`$sourceCheckout` to the registered read-only upstream checkout:

```powershell
$sourceCommit = 'c834c652b6862bc5679fd7f69a38a7093206efc6'
git -C $sourceCheckout show "${sourceCommit}:disasm/code/common/scripting/map/mapscriptengine_1.asm"
git -C $sourceCheckout show "${sourceCommit}:disasm/code/common/scripting/entity/entityfunctions_1.asm"
git -C $sourceCheckout show "${sourceCommit}:disasm/data/maps/entries/map21/mapsetups/s2_entityevents_506.asm"
```

Use the other exact source paths above for population and post-hide consumers. The existing lookup
and facing source guards can be invoked independently; importing those helpers does not execute an
emulator. Documentation checks are `uv run sf2 design-contracts test` and `git diff --check`.

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
