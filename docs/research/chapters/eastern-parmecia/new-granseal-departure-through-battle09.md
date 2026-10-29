# New Granseal departure through Battle09 admission

Evidence date: 2026-10-13. **Confirmed:** the selected static connection is Map6 south
exit → Map73 (New Granseal overworld) → Map72 (Bedoe overworld) → Map69 (South Parmecia).
Scrolling edges stay inside exploration; they do not run ordinary entry/init or
`CheckBattle`. At a dispatched Map69 default zone, F509 clear sets F409 and writes a
partial same-map reload request. If consumed with Map69 still current, F409 selects
Battle09 before Battle17, without checking battle-area bounds. Both Battle09 opening
script rows are `ms_Empty`; the start wrapper still sets F459. **Inferred:** this is a
candidate continuation toward the encounter named `BATTLE_TO_RIBBLE`. **Unknown:** a
natural walk, intact request/actor state, completed services and actual battle readiness.
This is original-game static research, not a continuous playthrough or remake capability.

## Agreement and provenance

This is the Standard-rigor scope agreement for
[Issue #752](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/752), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Different transition consumers, partial request state and ordered battle selection need
explicit boundaries. Sole tracked owner: this document. Accepted starting base:
`d6c7d9c6b28c8e1759fa5b8f3e2826bd020cd6dd`; no stacked dependency or shared-path write.
Start at the first Map6 control boundary in
[Peter's castle audience and town return](./peter-castle-audience-through-town-return.md).
Stop at the Battle09 start dispatcher return, before `ActivateEnemies`.

Only the named east-then-south candidate and its discriminators are admitted. Other
world routes, destination town content, Battle09 combat/outcomes, Battle10/11, later
raft/Luke/cannon scenes, translation, engine/UI work and runtime acquisition are excluded.
No parser, fixture, schema, registry or new comparison platform is needed. Acceptance is
pinned source/row/handler/fixture review, applicable retained comparisons, direct document,
link, private-boundary and diff checks, clean committed planner and actual final-head CI.
Freeze a clean pushed Draft PR for independent main-gate review; no self-merge or cleanup.

Source paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM`, pinned `master` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The retained read-only checkout is clean at that pin. Retained `map-init` and
`map-script-engine` payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Their fixture comparisons and the existing cutscene-source comparison pass; no new
extraction or runtime observation is claimed. Fnnn denotes flag nnn; battle table rows
are zero-based source IDs. Map setup filenames are under
`data/maps/entries/mapNN/mapsetups/`; warp files are `mapNN/6-warp-events.asm`.

## Town departure and Map73 entry

**Confirmed:** the completed predecessor carries Peter's joined flag F7, AI-control bit,
F702 and persistent map-follower flag F67. Active flag F39 remains conditional on party
capacity/history; map-following does not prove active battle membership. The accepted
voyage/construction lineage supplies F700/F508/F705 set. Other later story flags, party
state, follower flags, pending requests and service completion remain explicit inputs.
The predecessor's control installation requires pending zero and carried player type 0;
it does not prove the south exit is accessible.

`sf2enums.asm` resolves `MAP_NEW_GRANSEAL=6`,
`MAP_OVERWORLD_NEW_GRANSEAL_KINGDOM=73`, `MAP_OVERWORLD_BEDOE_KINGDOM=72`, and
`MAP_OVERWORLD_SOUTH_PARMECIA=69`. The Map6 warp table (`0x9A3F4`) contains:

| Source row | Request tuple |
| --- | --- |
| South exit `(255,31)` | No scroll, Map73 `(29,53)`, Down. X=255 is a wildcard. |
| Separate row `(31,14)`, commented `unusable warp` | Same destination tuple; neither the comment nor row proves usable movement. |

`WarpIfSetAtPoint` in `code/gameflow/exploration/exploration.asm` scans in source order,
uses negative trigger bytes as wildcards and matches other bytes exactly. Its ordinary
movement caller in `code/common/scripting/entity/entityscriptengine_2.asm:loc_528A`
requires masked layout attribute `$1000`; coordinates alone do not invoke it. The helper
writes type 1, all five parameter bytes (plus the row's trailing byte) and `WARP_SFX`.
No layout, collision or connected path is decoded here.

If the request survives, `WaitForEvent` and `ProcessMapEvent` in
`code/gameflow/exploration/explorationfunctions_2.asm` poll then clear the pending word
before dispatch. The no-scroll handler plays/clears the supplied warp SFX, idles main,
removes the exploration return frame and returns Map73 `(29,53)`, Down to `MainLoop`.
`SwitchMap` has no Map73 substitution; `CheckBattle` has no Map73 row. Ordinary
`ExplorationLoop` therefore clears pending, adjusts the step counter, heals living/immortal
allies, sets current map/not-in-battle, rebuilds entities/sprites, clears temporary flags
256–383, sets F80, loads the map and calls init before control. This is not a full-force
reset. Shared healing and request limits remain with the
[battle-loop](../../battle/battle-loop.md) and [request consumer](../../world/map-event-request-consumption.md).

`data/maps/mapsetups.asm` gives Map73 only its default setup. Its pointer table selects an
empty fixed entity list and `ms_map73_InitFunction` (`0x50342`; retained map-init primary
body 79). Empty fixed entries do not suppress flag-selected followers. The init selector is:

| Supplied history | Result |
| --- | --- |
| F700 clear | Run `cs_503A6`, then set F700 and return; do not test the construction branch. |
| F700 set, F508 clear | Return without a scene. |
| F700/F508 set, F705 clear | Run `cs_50806`, then set F705 and return. |
| F700/F508/F705 set | Return without a scene; this is the stated completed lineage. |

The [voyage owner](../grans-opening/voyage-through-damaged-ship-control.md#map73-admission-and-init-selection)
and [construction owner](./battle08-outcomes-through-new-granseal-handoff.md) own those
alternate scenes and requests; they are not decoded again. The selector does not test
F701/F702 or arrival coordinates. Map73's `s3_zoneevents.asm` (`0x5032C`) has only a default
handler that returns. It does not unlock Battle09. Control installation still depends on
pending being zero after init/services; no unconditional post-init clear guarantees that.

## Selected overworld connections and transition consumers

**Confirmed:** the selected rows, from Map73 `6-warp-events.asm` (`0xC2F6A`) and Map72
`6-warp-events.asm` (`0xC1BFE`), are:

| Connection | Trigger | Encoded destination and transition |
| --- | --- | --- |
| Map73 east → Map72 | `(63,255)` | `(1,255)`, `warpScroll RIGHT`, facing byte Right. |
| Map72 south edge → Map69 | `(255,63)` | `(255,1)`, `warpScroll DOWN`, facing byte Down. |
| Separate Map72 south connection → Map69 | `(40,62)` | `(41,16)`, `warpNoScroll`, facing Down. |

Map73 also has a south connection to Southwest Parmecia, and Map72 has other exits.
Their existence prevents an exclusive-route claim; their content is outside this slice.
Map69's reverse fixed row `(41,15)` returns to Map72 `(40,61)`, Up, while its northern
scroll row `(255,0)` returns to Map72 `(255,62)`, Up. These are connection counterparts,
not evidence that either pair is naturally traversable.

`sf2mapmacros.asm` encodes no-scroll as 0 and scroll as `$10 + direction` (Right=0,
Down=3). `ProcessMapEventType1_Warp:loc_259CC` handles a nonzero parameter 1 by setting
`CURRENT_MAP`, calling `code/common/maps/mapload.asm:ProcessMapTransition`, updating
nonnegative destination coordinate bytes on main's position and destination, calling
`UpdatePlayerPosFromMapEvent`, and declaring the raft before returning to exploration.
`ApplyOverworldMapTransition` uses the low direction bits for scrolling.

For the selected east edge X becomes 1 and negative destination Y leaves the existing
Y unchanged. For the south edge Y becomes 1 and negative destination X leaves the existing
X unchanged. These are coordinate-byte rules at the consumer, not a promise about a
completed movement frame. The scroll branch does not apply parameter 5 as facing; it
reads main's actual facing. It does not remove the exploration return frame, call
`MainLoop`/`CheckBattle`, rebuild map entities, clear temporary flags, heal, or invoke
map init. `ProcessMapTransition` loads tiles/blocks/layout and scrolls; it does not add
those ordinary-entry operations. The row's facing byte remains parameter state even
though the scroll consumer does not apply it.

Consequently the selected Map73→72→69 scrolling composition does not run Map72 or Map69
init and does not automatically start a battle at either border. It retains the prior
entity cohort rather than instantiating each destination's empty fixed list. By contrast,
Map72's `(40,62)` no-scroll row returns the explicit Map69 `(41,16)`, Down tuple to
`MainLoop`: battle selection occurs before ordinary Map69 exploration/init. If no row
is eligible, ordinary entry runs; if F409 is already set, Battle09 can start immediately
without visiting a Map69 zone. No Map69/72 substitution is present in
`data/maps/global/flagswitchedmaps.asm`.

Both Map72 and Map69 have only default setups, shared by all supplied story histories,
and empty fixed lists. Map69 `ms_map69_InitFunction` (`0x4FDE2`, retained body 76) is
just `rts`. Map72's init (`0x4FF2E`, body 78) runs `cs_4FF5A` only if F716 is set and
F10 clear; otherwise it returns. That later Luke branch is an alternate ordinary-entry
control, not a scene reached by this scrolling edge; its body is excluded.

Map72 zone selection can interrupt any attempted walk. Its `s3_zoneevents.asm`
(`0x4FE64`) first matches X32/33/34, wildcard Y, to `CheckRandomBattle(26)`; then
X25–30,Y37 to the later cannon handler (immediate return when F750 set or F805 clear);
otherwise its default calls `CheckRandomBattle(8)` with F523 clear, or `(24)` with F523
set. Do not infer that the default means every ordinary tile runs that handler: the
movement zone producer still needs layout attribute `$1400`. Under carried F508 set,
Battle08's helper is in its completed/random branch, not guaranteed silent.
The [random-battle owner](../../world/map-event-random-battle-state.md) retains the step/RNG/request
contract. Competing Map72 battle rows are ordered 8,24,26, all wildcard triggers;
ordinary `CheckBattle` chooses the first unlocked one, not the geometrically nearest
battle area. No encounter, cannon body or alternate route is audited here.

## Map69 zone, partial reload and Battle09 selection

**Confirmed:** `s3_zoneevents.asm` (`0x4FDA2`) first has X23,24,25 with wildcard Y,
all selecting `Map69_ZoneEvent0`; only the final default selects
`Map69_DefaultZoneEvent`. A matched specific row never falls through to the default
because its own flag checks decline to request a battle.

The specific handler calls `CheckRandomBattle(BATTLE_TO_TAROS_SHRINE=17)`, which can set
F417 and request reload under its F517/step/RNG rules. This is separate from Battle09;
it does not set F409. The default checks only F509: when set, return without writes;
when clear, set F409 then `move.l #MAP_EVENT_RELOADMAP,MAP_EVENT_TYPE` and return.
There is no F702/F700/F705, membership, AI, facing, step-counter or RNG gate in that
short default handler. The early story is context, not an extra unlock predicate.

The ordinary type-6 producer at `entityscriptengine_2.asm:loc_5294` writes type 6 and
word X to parameter 1/2, word Y to parameter 3/4. The type-6 consumer applies main's
init actscript and passes those words to `RunMapSetupZoneEvent` (`0x4751A`) in
`code/common/scripting/map/mapsetupsfunctions_1.asm`. After the selected handler,
that dispatcher closes portrait/text, waits one VInt and waits for physical entity 0
to stop, then returns. It does not clear the newly written request or wait for every
follower. The next exploration poll may consume it; survival across services remains
an explicit condition.

`sf2enums.asm` defines `MAP_EVENT_RELOADMAP=$100FF`. The big-endian long write covers
`0xFFA84A..0xFFA84D`: type word `0001`, parameter 1=`00`, parameter 2=`FF` (`MAP_CURRENT`).
`sf2const.asm` places parameters 3/4/5 at `0xFFA84E/4F/50`; they are untouched.
This is a partial type-1 request, not a special reload event type or complete tuple.
The no-scroll consumer first samples entity 0's X/Y in map tiles, then overrides X/Y
with nonnegative parameter 3/4 bytes and unconditionally takes parameter 5 as facing.
For an intact ordinary type-6 lineage with zone Y in 0–63, parameters 3/4 are `00,Y`:
the returned tuple is therefore `MAP_CURRENT,(0,Y),retained parameter 5`, not necessarily
main's sampled X/Y/facing. Without that lineage, retain the general signed-override rule.
Parameter 5 is not written by type 6 or the default handler; the selected preceding
south warp writes Down, but intervening writers can change it. The pending slot is
mutable, not a queue. Overwrite/interruption can change both dispatch and remaining tuple.

`MainLoop` then calls `SwitchMap` and `CheckBattle` (`code/common/maps/getbattle.asm`,
`0x799C`). The latter resolves `MAP_CURRENT` from `CURRENT_MAP`. In
`data/battles/global/battlemapcoords.asm`, the only Map69 rows are:

| Source battle ID | Area output `(X,Y,width,height)` | Trigger X/Y | Unlock / completion |
| --- | --- | --- | --- |
| 9, `BATTLE_TO_RIBBLE` | `(0,0,48,32)` | `(255,255)` | F409 / F509 |
| 17, `BATTLE_TO_TAROS_SHRINE` | `(0,31,30,32)` | `(255,255)` | F417 / F517 |

`CheckBattle` scans in row order, tests map, unlock and trigger bytes, writes the area
outputs, then checks completion. Area bounds do not constrain admission. Thus:

- F409 set selects 9 even with F417 set or coordinates outside its area.
- F409/F509 both set clears F409 but still returns 9 on that same call. Completion does
  not skip to 17. The default's F509-set return does not erase a pre-existing F409.
- F409 clear can select 17 when F417 is set; neither set returns -1 for Map69.
- F509 clear default production sets F409 before the next selection, so it wins over a
  carried F417 if the request and current map survive.
- A specific X23–25 random request may also select 9 if F409 was already set; the helper's
  requested encounter ID is not a direct `BattleLoop` argument.

No natural first Map69 zone coordinate is claimed. In particular, the scroll edge's
retained X could select a specific row rather than the default, and movement/layout or
another request can prevent this composition entirely.

## Battle09 opening and the stopping boundary

**Confirmed:** `MainLoop` passes selected battle index 9 to
`code/gameflow/battle/battleloop_1.asm:BattleLoop` (`0x23A84`). It clears `PLAYER_TYPE`,
sets F399 and tests F88. With F88 clear, the ordered opening is:

1. Clear elapsed seconds; establish current map/battle and base VInts; call
   `ExecuteBeforeBattleCutscene`.
2. Restore/reassert map/battle, clear region flags 90–105, heal living/immortal allies,
   initialize ally positions, initialize enemy positions, then clear AI memory.
3. Call `LoadBattle` for combatant 0, then `ExecuteBattleStartCutscene`; stop at its
   return, immediately before `ActivateEnemies`.

The source-selected row 9 in `data/battles/cutscenes/beforebattlecutscenes.asm` is
`ms_Empty`; row 9 in `battlestartcutscenes.asm` is also `ms_Empty`. There is no missing
Battle09 before-script dependency. The wrappers in `code/gameflow/battle/cutscenes/`
add `BATTLE_INTRO_CUTSCENE_FLAGS_START=450` to 9: before checks F459 and dispatches only
if clear, without setting it; start checks F459, sets it before dispatch if clear,
and otherwise skips. Empty content therefore still has a real caller flag effect.
F459 set skips both scripts but does not skip new-battle initialization. F509 completion
is not this intro gate.

`code/common/scripting/map/ms_empty.asm` (`0x47A4E`) is the `$FFFF` terminator.
`ExecuteMapScript` in `mapscriptengine_2.asm` reads it and goes to `loc_47234`, which can
wait for view scrolling when a dialogue window is present, clears scrolling speed and
restores registers. Empty means no story-command text, join, warp or story flag; it
does not mean zero interpreter/service effects or proved immediate completion.

The existing [battle lifecycle](../../battle/battle-loop.md), [cutscene routing](../../battle/battle-cutscenes.md)
and [global-data](../../battle/battle-global-data.md) owners bound initialization. Direct source
in `battleloop/initializecombatants.asm` resets ally X, calls `UpdateForce`, places living
active-party members using battle spriteset slots and can remove overflow members from
the active party. It scans enemy slots through the existing initialization/hidden-entry
rules; this document publishes no roster. `battleloop/clearaimemory.asm` resets the 48
last-target bytes to -1 and 48 memory bytes to zero. That is separate from Peter's
combatant AI-control bit. `battlefunctions/loadBattle.asm` positions battle entities,
loads map/sprites/terrain and invokes music/fade services before the start dispatcher.

Peter's F7 membership, conditional F39 active status, AI bit and F67 map follower remain
distinct. The empty Battle09 scripts do not join Peter or install followers. Ordinary
Map73 entry uses `followersfunctions_1.asm:InitializeFollowerEntities` and
`data/scripting/entity/followers.asm` row `follower 67, ALLY_PETER, $FF, 1`. Map69/72/73
are all in `data/maps/global/overworldmaps.asm`; supplied F65 selects the overworld
follower prefix, while other follower flags remain inputs. Scrolling does not rebuild
this cohort. Battle initialization consumes the active-party list, not visible follower
membership; the predecessor's capacity and membership/AI distinctions remain applicable.
No Peter AI action, active roster or turn-readiness observation is claimed.

With F88 set, the alternate entry restores saved seconds, clears F88 and AI memory,
loads battle state and jumps directly to the individual-turn loop. It bypasses this
new-battle before/start sequence and its F459 write. A valid saved-state composition
cannot be inferred merely from a supplied F88. Resume behavior is a discriminator,
not an alternate route proved here. Enemy activation, rounds, combat and outcomes are
beyond the stopping boundary.

## Coverage, controls and reproduction

Existing fixtures are under `tests/fixtures/h2/`; narrow owner commands have prefix
`uv run sf2 h2`. Naming an owner below does not claim its full extraction rail ran here.

| Relation | Existing coverage and concrete missing composition |
| --- | --- |
| Connections and ordinary versus scrolling admission | `map-data-static-v1.json`, `gameflow-core-static-v1.json`, `common-maps-static-v1.json`, `common-scripting-static-v1.json`; [map inventory](../../world/map-data-inventory.md). Direct pinned macro/row/consumer review binds the selected tuples and no-scroll versus in-loop return. Missing: accessible layout, continuous movement, collision and entity/service completion. |
| Setup, init and zone selection | `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-init-static-v1.json`, `map-events/zone-programs.json`; commands `map-setup`, `map-entities`, `map-init`, `map-events`. Retained init bodies 76/78/79 and direct pointer/list/zone reads bind the selectors. Controls: carried versus anomalous F700/F508/F705, F716/F10, F523, F750/F805, specific versus default zone. No later scene body is newly accepted. |
| Partial request and encounter selection | `map-event-request-state-static-v1.json`, `map-event-request-consumption-static-v1.json`, `map-event-random-battle-state-static-v1.json`, `common-maps-static-v1.json`; [request](../../world/map-event-request-consumption.md), [random helper](../../world/map-event-random-battle-state.md), and the [prior partial-request composition](../grans-opening/battle02-to-battle03-content-route.md). Controls: non-zone layout, X23–25, F509-set no-write, absent F409, F417 competition, completed-but-unlocked same-call selection, stale/overwritten parameter 3/4/5, changed current map. Missing: natural producer → service completion → intact poll/clear → selected Battle09. |
| Empty opening, party and AI boundary | `battle-control-static-v1.json`, `battle-loop-static-v1.json`, `battle-cutscenes-static-v1.json`, `battle-global-data-static-v1.json`, `map-script-engine-static-v1.json`; commands `battle-control`, `battle-loop`, `battle-cutscenes`, `battle-global-data`, `map-script-engine`. Retained script and cutscene-source comparisons plus row/handler review bind F459 and the terminator. Controls: F459 set, F88 resume, joined reserve/full party, F67 independent of AI/membership, other followers/F65. Missing: concrete loaded actor/party state and completed services at the return before activation. |

A necessary future executable composition should extend these owners and bind the transition
kind, current map, supplied flags, zone source/coordinates, request-byte lineage, ordered
battle row, party/AI inputs and before/start return. This static design-support decision
needs no new generic comparator or per-scene fixture. Unknowns are not an acquisition queue.

The reused payloads total 18,285,979 bytes (about 18.3 MB). One process performs linear
selected reads/comparisons, estimated under 512 MiB with a one-minute comparison budget;
outputs are this prose and small ignored review/planner receipts. No full transcript,
new layout extraction, capture, environment or installation. Reassess before any route,
scene or battle-semantic expansion.

Load `. ./local/private-inputs.ps1` in the owning checkout. Select the retained read-only
`$upstream`; verify it with `git -C $upstream rev-parse HEAD` and `status --short`.
Run the existing [retained comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
for map-init/map-script-engine and its cutscene-source comparison. Read the exact source
paths, symbols, macro operands and ordered rows named above; compare retained init bodies
76/78/79 and selected zone-program operations to those pinned bodies. Check `sf2const.asm`
request adjacency, the type-6 producer and both type-1 branches together; selecting only
the reload constant is insufficient. Resolve row 9 in both opening tables and read the
wrappers, terminator and BattleLoop call order through the stated stop.

Record source/document/link/private checks, `git diff --check`, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`, actual final-head CI and command
failures in the Issue/PR handoff. Normal/full/engine/H3/H4/Godot runs are not selected
by this documentation-only agreement. Preserve earlier results and private inputs; freeze
the clean pushed Draft PR and leave acceptance/merge to main-gate.
