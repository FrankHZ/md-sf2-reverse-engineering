# Map 3 Natural Route to Messenger Entry

## Scope and provenance

This R2 owner consumes the accepted controlled R1 boundary in
[`map3-admitted-start.md`](map3-admitted-start.md).  It makes one narrower
claim than its rail name suggests: from R1's first original Map 3
`WaitForEvent`, original controller input reaches the entry of the original
`cs_5149A` messenger program, before that program's body.  It does not claim a
continuous route to Battle 01, battle admission, battle readiness, or a battle
turn.

- ROM: USA retail SHA-256
  `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`.
- Runtime: BizHawk 2.11.1 / Genesis Plus GX, one case in one launch:
  `uv run sf2 h3 map3-battle01-natural-route --timeout-seconds 300`.
- Public owner: fixture
  [`map3-battle01-natural-route-v1.json`](../../tests/fixtures/h3/map3-battle01-natural-route-v1.json),
  deterministic verifier, three closed schemas, and observer.  The fixture
  contains state facts and provenance metadata only; ROMs, session ROMs,
  SRAM, traces, captures, dialogue, graphics, audio, and other payloads remain
  ignored under `local/`.

The verifier hashes the bounded source surface, derives the movement/event
contract from source, H1, ROM, layouts, and map offsets, rejects drift before
launch, and gives the observer no accepted golden.  The observer uses only the
accepted R1 controlled snapshot and original controller input after admission:
it uses no teleport, debug battle, coordinate write, or tracked capture.

## Reached opening route

**Confirmed:** the sole `natural-map3-opening-to-messenger-entry` case starts
at Map 3 `(56,3)`, facing `3`, and reaches original `cs_5149A` at the source
target `(43,10)` before that program body.  The 46-label callback chronology
includes the house-exit warp, the house and school doors, Sarah's action,
Map 3's two stair warps, both required Zone 7 admissions, entity 142's action
and `cs_513A0` re-init, and the final Zone 8/messenger admission.

The five observed Map 3 program entries, in source order, are
`cs_5145C`, `cs_513D6`, `cs_5148C`, `cs_513A0`, and `cs_5149A`.  At the
endpoint F601, F256, F602, and F260 are true; F603 remains false.  This is an
entry-before-body boundary, so it does not claim the messenger program's
effects.

**Confirmed:** the logical controller corpus has 95 source/layout-derived
edges.  Sarah retains one source-adjacent `C`; entity 142 retains the required
Left-facing edge and one `C`.  The four zone admissions are validated by their
original raw-coordinate callbacks, not by fabricated controller `C` inputs.
Release, polling, and scheduler frames are diagnostic-only.  A production-Lua
extraction test rejects the first missing, duplicate, or unmodelled logical
edge, proves that a Zone 7 introduction's Left edge is immediately followed by
entity 142's Down edge, and proves a zone waypoint remains neutral until its
callback advances to the house-door Down edge.

**Confirmed:** actual callback-observed opening owners include `MainLoop`,
`ExplorationLoop`/`WaitForEvent`, `ProcessMapEventType1_Warp`,
`ProcessMapEventType6_ZoneEvent`, `ProcessPlayerAction`, `GetActivatedEntity`,
`RunMapSetupEntityEvent`, `RunMapSetupZoneEvent`, `OpenDoor`, the selected Map
3 init, Map 3 entity/zone targets, and the five program entries above.  The
physical PC `0x051382` is registered once as `map3-init-dispatch`; its R1 and
post-warp roles dispatch deterministically rather than registering aliases.
Callback exceptions become typed status/exit failures with expected/actual
state; the passing launch reported no Lua Console error, zero residual
callbacks, scoped restoration, and deletion of the disposable session ROM.

## Static continuation boundary

**Inferred / partial:** the source/H1/ROM contract reconstructs route topology
across Maps 3, 19, 20, 21, 40, and 57.  It joins exact layout areas, movement
and event ordering, source warps, gate flags, relevant Map 3 programs, the
Map 57 Battle 01 row, and the pre-turn battle lifecycle ordering.  This is a
deterministic static reconstruction only.  It is not continuous runtime
observation of Map 19/20/21/40/57, does not show F401's natural state, does not
reach `CheckBattle`, `BattleLoop`, cutscenes, or a battle-ready state, and does
not observe a Battle 01 turn.

The 26 `data/maps/entries/map03/*` index records remain aggregate static
inventory.  This owner neither associates all 26 with the route nor promotes
them to callback evidence.  The research-index bindings added for this rail
are limited to owners actually callback-observed in the reached opening.

## Gap-register effect

- **RA-03:** **Confirmed only through the reached opening**: original input,
  movement, doors/warps, zone admissions, Sarah/entity 142 actions, re-init,
  and messenger-program entry are observed.  Any continuation after that
  entry remains **Unknown** runtime evidence.
- **RA-04:** **Inferred / partial**: the six-map to Battle 01 source route is
  reconstructed, but the exact remaining seam is natural runtime observation
  from `cs_5149A`'s body through the Map 57 trigger, F401, before/start
  cutscenes, and battle-ready state.
- **RA-08:** **Confirmed NotReached**: the opening route did not enter the
  field menu.  No field-menu behavior is claimed or added.
- **RA-09:** **Confirmed only as the reached callback/program chronology**;
  no dialogue payload, speaker rendering, text timing, or body-after-entry
  meaning is public evidence here.
- **RA-11 foundation:** **Confirmed only for the reached opening's private
  provenance and bounded callback/state facts.**  Pixels, audio, cadence,
  hardware observables, and private captures remain **Unknown**.

## Grouped runtime question queue

One future continuation matrix, if separately scoped, should start at the
unexecuted body of `cs_5149A` and jointly observe the remaining Map 3 program
effects, Map 19/20/21/40/57 transitions, F401, `CheckBattle`, before/start
cutscenes, and first battle-ready state.  It must also determine whether any
later route segment enters the field menu.  That queue is not evidence from
this run and does not authorize an H2, R3/R4, persistence, full 8C, or optional
Map 3-content expansion.
