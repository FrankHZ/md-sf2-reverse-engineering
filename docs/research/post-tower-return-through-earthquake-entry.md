# Post-tower return through earthquake-town entry

Evidence date: 2026-10-12. **Confirmed:** selected outgoing records connect Map58 through
Maps60/57/40/21/20/19 to a Map3 request at `(29,2)`, Down. With F640 set, `SwitchMap` changes that
request to Map4 before encounter selection. Map4's first init runs `cs_51750`, then sets F624;
repeat init runs only `cs_51742`. Neither program explicitly requests another map or zone event.
The first control boundary is the generic `WaitForEvent` pending-event check after init.
**Inferred:** this is the return from the tower into Granseal's earthquake evacuation.
**Unknown:** legal walking between records, naturally completed scene services and the actual
pending-event value at return. This is conditional static evidence, not an observed continuous escape.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue #720](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/720), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514). Request,
setup, follower and init consumers require explicit ordering across several owners. Sole tracked
ownership is this document, with no shared-path writes. Accepted starting base is
`84be50de6384bff0021b69ae7bf03b85ed2a94f5`, including
[Battle07 outcomes and aftermath](./battle07-outcomes-and-aftermath-handoff.md) and
[the Battle06-to-Battle07 route](./battle06-to-battle07-content-route.md). Concurrent synthesis is
independent; it supplies no unmerged evidence or implementation dependency.

Start at the predecessor's first-victory Map58 control/outgoing boundary. Follow the directly
required tower/castle records and F507 content, effective earthquake-map selection, first/repeat
init and immediate control/event handoff. Stop there. Complete walking/layout proof, town hazards,
docks/ship departure, Battle08, optional interiors/NPC inventories, runtime acquisition, new
fixtures/parsers/comparison platforms, engine/UI and Chinese translation are excluded. Acceptance
is direct pinned source/fixture/verifier and prose/reference/link/private/diff review, clean
committed planner and actual final-head CI. A clean pushed Draft PR freezes for independent
main-gate review; neither delivery nor this document admits a remake capability.

All original paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM` at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The reused read-only checkout is clean at that pin. Retained `map-init` and `map-script-engine`
payloads bind that commit and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`; their existing fixture
comparisons pass. No new ROM verification, extraction or runtime observation is claimed. Addresses
are static source/H1 anchors. Command indices are zero-based retained `programCorpus.programs`
records, not frames; `Fnnn` denotes flag nnn.

## Starting state and outgoing requests

**Confirmed:** the predecessor's first-victory after program requests Map58 `(13,35)`, Down,
installs current-scene follows and sets F74/F640 before the battle caller clears F407 and sets
F507. F999 must be clear to keep that return on Map58. Ordinary before-scene execution cleared
F83; intro-skipped state does not guarantee it. Repeated victory does not freshly establish F74,
F640 or the fixed return position. Those alternate caller states remain separate.

These are selected records in `data/maps/entries/mapNN/6-warp-events.asm`, not a decoded walking
route. Numbers/names resolve through `sf2enums.asm`: Map19 is castle **2F**, Map20 **1F**, and
Map21 **3F**. `255` is a wildcard coordinate. Every row below has `warpNoScroll` (parameter 1 zero).

| Source map / trigger | Requested destination `(map,x,y,facing)` | Source anchor |
| --- | --- | --- |
| Map58 underground room / `(255,37)` | `(60,8,52,RIGHT)` | `map58/6-warp-events.asm`, `0xB8A26` |
| Map60 stairway / `(53,1)` | `(57,7,4,DOWN)` | First record, `map60/6-warp-events.asm`, `0xB9F50` |
| Map57 tower entrance / `(255,19)` | `(40,15,13,DOWN)` | Second record, `map57/6-warp-events.asm`, table `0xB8612` |
| Map40 tower exterior / `(255,31)` | `(21,9,2,DOWN)` | Second record, `map40/6-warp-events.asm`, table `0xB0ABC` |
| Map21 castle 3F / `(3,16)` | `(20,3,36,RIGHT)` | First record, `map21/6-warp-events.asm`, `0xA61F8` |
| Map20 castle 1F / `(6,37)` | `(19,6,2,LEFT)` | Sixth record, `map20/6-warp-events.asm`, table `0xA53DA` |
| Map19 castle 2F / `(255,31)` | `(3,29,2,DOWN)` | Seventh record, `map19/6-warp-events.asm`, table `0xA464E` |

Opposite-direction records were not substituted for these consumers. Map60 also contains same-map
portals and other exits; their presence does not prove `(8,52)` can reach `(53,1)`. Map57's F506
copy in `3-flag-events.asm`, `(17,0)` size `2x1` to `(7,4)`, overlaps the arrival area; neither the
copy nor its absence proves passage. Map60's two flag copies require F999. Maps19/20/21/40 have
empty flag-copy tables; all six maps except Map19 have empty step-copy tables. Map19's local step
copies at `(15,17)` and `(16,28)` do not supply a route. Collision, working layouts, actor occupancy
and follower traversal remain Unknown.

**Confirmed:** `code/common/scripting/entity/entityscriptengine_2.asm:loc_528A` invokes
`WarpIfSetAtPoint` at its warp-type tile check. That function in
`code/gameflow/exploration/exploration.asm` matches coordinates with negative-byte wildcards,
writes event type 1 plus parameters 1–5 and `SFX_WARP`. Naming a trigger coordinate alone does
not establish its actual map/movement caller. `WaitForEvent` and `ProcessMapEvent` in
`explorationfunctions_2.asm` poll and clear the request. Parameter 1 zero selects
`ProcessMapEventType1_Warp`'s no-scroll path: idle the main entity, unwind exploration and return
the destination in d0–d3. Nonzero selects the different scrolling consumer.

Unlike the battle-outcome return, these ordinary warps reach `MainLoop:@Start`
(`code/gameflow/mainloop.asm`, `0x75C4`): `SwitchMap` precedes `CheckBattle`, and exploration init
runs only if no battle is selected. `code/common/maps/getbattle.asm:CheckBattle` (`0x799C`) and
`data/battles/global/battlemapcoords.asm` give the relevant interception:

- Map57 matches battle row 1, trigger `(255,255)`, with F401 set. Even F501 set clears F401 only
  **after selection**, so that call still selects Battle01. F401 clear is required to pass this
  request directly into exploration; F507 alone does not establish it.
- No battle-coordinate row uses Maps60/40/21/20/19/3/4. Their selected requests have no matching
  row in this consumer. This does not prove the entire journey encounter-free.
- Returning to Map58 through another stairway record has its own F407/F999 conditions; that
  detour is not the selected onward chain.

## Selected castle setups and events

**Confirmed:** `code/common/scripting/map/mapsetupsfunctions_1.asm:GetCurrentMapSetup` (`0x4779E`)
starts with a map's default pointer and replaces it for each set flag in source order: the **last**
set flag wins. `data/maps/mapsetups.asm` has no Map57 or Map60 row. Absent interception, these use
`ms_Void`, with no fixed setup entities or setup init; generic flag-selected followers still
initialize. The selected setups below assume the predecessor's F506/F507 state and no later
F543/F982 override where applicable. Battle07 victory does not clear later flags.

| Map / selected setup | Init, actor and event consequences |
| --- | --- |
| Map40 / `ms_map40` | F507 follows F506 and restores the default setup. `s1_entities.asm` is empty: the old F506 Lemon/guard population is not selected. `s6_initfunction.asm:ms_map40_InitFunction` checks F507 and calls `cs_53FD8`; default zone returns. |
| Map21 / `ms_map21_flag506`, F543 clear | `pointertable_506.asm` selects `ms_map21_flag609_InitFunction`, bypassing the older guard-position program. It checks F507 and calls `cs_53FD8`. Selected entity/zone tables are default-only; an older guard conversation is not selected merely by its shared implementation filename. |
| Map20 / `ms_map20_flag506`, F543 clear | `pointertable_506.asm` selects `ms_map20_flag501_InitFunction`, bypassing the bedroom scene. It checks F507 and calls `cs_53FD8`; selected entity/zone tables are default-only. |
| Map19 / `ms_map19_flag507`, F543/F982 clear | `pointertable_507.asm` selects F507 entity/zone/init tables. `s1_entities_507.asm` declares one Astral sprite at `(63,63)`, Down, `eas_Init`; script reference 128 is this setup actor, distinct from `FOLLOWER_B`. Default entity event returns. Init `0x53168` checks F507 and calls `cs_53FD8`. |

`cs_53FD8`, physically in `map40/mapsetups/s6_initfunction.asm` (`0x53FD8`), requests
`MUSIC_BOSS_ATTACK`, runs `fadeInB`, then ends. It writes no progression flag, warp, join or
follower row. Its presence does not establish presentation completion.

Map19's `s3_zoneevents_507.asm` selects these independently guarded events. Older F506 handlers
in that same file are not selected simply because they share its source location.

| Zone / handler | First/repeat order and short text summary |
| --- | --- |
| `(16,5)` / `Map19_1FB_ZoneEvent0` (`0x52DDE`) | F622 clear calls `cs_53620`, then sets F622; F622 set returns. `scripts.asm:315..317`, commands 0–2, text 713: Astral warns about the quake and urges escape. |
| `(25,13)` or `(25,14)` / `Map19_1FB_ZoneEvent1` (`0x52DF0`) | F623 clear calls `cs_5362A`, then sets F623; F623 set returns. `scripts.asm:318..320`, commands 0–2, text 714: Astral declares the group safe. No F622 check precedes it. |

Both scripts address 128. Neither changes F74/F83/F640, moves followers, grants items, alters
combat HP or joins force members. Generic zone dispatch uses supplied coordinates; its producer
still needs the relevant working-map tile/caller. F622/F623 suppress repeat dialogue; they are
not read by the Map19 south warp, `SwitchMap` or Map4 init. Thus they are not explicit progression
prerequisites at those consumers. Whether a legal route necessarily crosses either marker remains
Unknown, not proof that dialogue is physically optional. Text 714 is not proof of everyone's survival.

## Effective Map4 entry and population

**Confirmed:** `code/common/maps/mapinit_0.asm:SwitchMap` (`0x7956`) and
`data/maps/global/flagswitchedmaps.asm` (`0x7988`) replace requested Map3 with Map4 when F640 is
set, retaining `(29,2)`, Down. F640 clear retains Map3; F507/F622/F623/F624 are not substitution
predicates. A direct Map4 request needs no F640 substitution. Map4 has one `ms_map4` setup with no
flag-specific alternative: F624 branches inside init, not at pointer-table selection.

For effective Map4, `ExplorationLoop` (`0x257C0`) clears the pending event on entry, heals living/
immortal allies, fades, assigns current map and `NOT_CURRENTLY_IN_BATTLE`, loads tilesets and
initializes entities. `j_GetMapSetupEntities` resolves to `GetMapSetupEntityList`, and
`j_InitializeMapEntities` to `InitializeMapEntities`, through
`code/common/tech/jumpinterfaces/s07_jumpinterface.asm`. The latter in
`code/common/scripting/map/mapfunctions.asm` (`0x440CC`) clears the old population, initializes
flag-selected followers, reads fixed setup entities, then declares Bowie at the supplied position
with `eas_Idle`.

`map04/mapsetups/s1_entities.asm` (`0x516E4`) supplies guards 128/129 at `(28,7)` / `(29,7)`, Up,
with `eas_Init`. `followersfunctions_1.asm:InitializeFollowerEntities` (`0x44298`) consumes
`data/scripting/entity/followers.asm`: F74 enables Astral (`FOLLOWER_B`), king (`FOLLOWER_A`) and
minister (`FOLLOWER_D`). This is flag-based reconstruction, not the old map's entity IDs surviving.
Selected content does not clear F74. Other follower flags remain inputs, especially intro-skipped
F83 and conflicting `FOLLOWER_B` rows. Initialization does not promise deduplication or a fixed
three-follower total; Map4 init does not validate/repair missing or conflicting follower declarations.

Exploration clears temporary flags 256–383, sets F80, loads the map, installs base VInt functions
and calls `RunMapSetupInitFunction` (`0x474FC`). F74/F507/F622/F623/F624/F640 survive that clear.
`LoadMapBlocksAndLayout` in `code/common/maps/mapload.asm` consumes enabled flag-copy records
**before** init. On F624-set entry, `map04/3-flag-events.asm` supplies the final `5x4` copy
`(51,8)` → `(27,0)` and `11x7` copy `(51,32)` → `(24,3)`. With F624 clear those copies are not
selected; the first script performs its own animation copies. Working-layout writes and
flag-selected reconstruction on later entry are separate mechanisms.

## First and repeat initialization

**Confirmed:** `map04/mapsetups/s6_initfunction.asm:ms_map4_InitFunction` (`0x51728`) checks F624.
Clear takes `script cs_51750`, then `setFlg 624`, then returns. Set takes `script cs_51742` and
returns without the first scene. The init caller writes the flag after script return, not a
`setStoryFlag`, map-entry request or the soldiers' individual action scripts.

| First program `cs_51750` (`0x51750`), commands / source lines | Ordered effects |
| --- | --- |
| 0–8 / 28–36 | Text cursor 715. Place living Bowie `(29,4)`, Down, and follower B/A/D at `(28,3)` / `(29,3)` / `(30,3)`, Down. Quake 1, fade in, wait, quake 1 again. These replace arrival positions; they are not warp destinations. |
| 9–20 / 37–60 | Guards greet the king/Astral. Astral moves down one tile and asks why they remain; they report chasms and people aboard a ship. Astral urges departure; a guard calls for help. Install different guard speeds, then start 128 down and 129 right without the `entityActionsWait` form. |
| 21–32 / 61–72 | Rumble, quake 3, wait; three `11x7` copies from `(51,18)`, `(51,25)`, `(51,32)` to `(24,3)` with waits. Assign `eas_FallInEarthquakeCrack` to 128, wait 5, then assign it to 129 with `setActscriptWait`. |
| 33–45 / 73–85 | Rumble; three `5x4` copies from `(51,0)`, `(51,4)`, `(51,8)` to `(27,0)` with waits; return quake to 1. Astral faces Right and directs Bowie toward the harbor via the east side. Assign `eas_Follower1` to `FOLLOWER_B`; end. The caller then sets F624. |

Six text commands consume IDs 715–720 (`02CB..02D0` in `gamescript.txt`); castle texts are 713/714
(`02C9/02CA`). Source operands/comments and those entries agree. Only short summaries are retained.
The guards' report about people aboard is not an observed docks state.

`mapscriptengine_1.asm:csc19_setEntityPosAndFacing` resolves references and sets position **and
destination** plus facing before updating the sprite. Its liveness helper skips dead force members;
ordinary Bowie's surviving post-victory state matters. Non-force followers depend on initialized
index mappings. `csc15_setEntityActscript` installs the action; the Wait form loops until its
pointer is `eas_Idle`. `map04/mapsetups/scripts.asm:eas_FallInEarthquakeCrack` (`0x51840`) moves,
plays falling audio, turns/shrinks the sprite, waits for destination, sets position `(0,0)`, then
jumps to `eas_Idle`. Guard 129 is explicitly awaited; guard 128 has no equivalent join at the next
command. These actor operations do not write combat HP/death or force membership. Concurrent
motion, wait termination, exact final actor state and perceived deaths remain runtime Unknowns.

`cs_51742` (`0x51742`), commands 0–2 / lines 25–27, sets guards 128/129 to `(0,0)`, Right, then
ends. It does not place Bowie, reposition followers, display text, copy blocks, set quake or request
another event. For the named south-exit request, Bowie retains `(29,2)`, Down before control; other
supplied entries retain their own tuple. F624-set layout copies already ran during load. The lack
of a repeat quake command does not prove zero amplitude: `csc33_setQuakeAmount` writes
`QUAKE_AMPLITUDE`, while caller state/presentation remain separate. F624 set is not proof that
this execution previously completed the first scene.

## Immediate control/event boundary and next owner

**Confirmed:** both scripts end without an explicit warp/zone-event producer. After init returns,
exploration conditionally runs map music/fade and enters `WaitForEvent` (`0x2591C`). With
`MAP_EVENT_TYPE` zero, it follows entity 0 and calls `SetControlledEntityActScript(0)`;
`entityfunctions_2.asm` selects `eas_ControlledCharacter` for `PLAYER_TYPE=0`, the predecessor's
ordinary battle-loop state. Arbitrary raft/caravan input is not silently normalized. With an event
pending, `WaitForEvent` skips control installation and returns the event to `ProcessMapEvent`.
Entry's clear does not prove the later runtime read remains zero. This conditional read is the
stopping boundary; no natural input or service completion is claimed.

The immediate zone owner `map04/mapsetups/s3_zoneevents.asm` selects only
`Map4_DefaultZoneEvent` (`0x516FA`), an `rts`; default entity event also returns at `0x51700`.
`ProcessMapEventType6_ZoneEvent` supplies coordinates to `RunMapSetupZoneEvent`; its shared tail
closes text/portrait windows, waits a VInt and waits for the main entity to stop. There is no further
authored Map4 zone scene behind that default. The next outward event owner is
`map04/6-warp-events.asm` (`0x98A24`): three east-edge triggers share a docks request. That pointer
belongs to a later evacuation slice, not proof that Bowie can cross the chasms, reach the docks,
board a ship or enter Battle08. Stop before consuming that outgoing request.

## Coverage and gap disposition

Fixture paths below are under `tests/fixtures/h2/`; commands have prefix `uv run sf2 h2`.
Coverage names existing admitted boundaries, not fresh full-rail execution.

| Claim / owner | Disposition and narrow surface | Missing relation / counterexample |
| --- | --- | --- |
| Warps, map substitution and encounter scan | **Covered** at separate consumers by `map-content-static-v1.json`, `common-maps-static-v1.json`, `battle-global-data-static-v1.json`, `map-event-request-consumption-static-v1.json`; [map content](./map-content.md), [battle global data](./battle-global-data.md). Commands: `map-content`, `common-maps`, `battle-global-data`, `map-event-request-consumption`. | Request-consumption stops at named accesses, not natural producer/consumer reach. F401+F501 still selects Battle01; F640 clear retains Map3; nonzero parameter 1 selects another consumer. |
| Setup, entities and init dispatch | **Covered** by `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-init-static-v1.json`; [map inventory](./map-data-inventory.md). Commands: `map-setup`, `map-entities`, `map-init`. Retained init comparison passes; selected primary bodies and pointer/route joins were reviewed. | F543/F982 override Map19; F507 selects Map40 default; Map57/60 have no setup; F624 branches inside init. |
| Castle flags and Map4 default events | **Covered** by `map-events/zone-programs.json` programs 33/34/93 and `map-events/entity-programs.json` program 568. Command: `map-events`. | Castle flags are written after script return; F623 does not test F622. Map4 handlers return. Table coordinates alone do not prove invocation. |
| First/repeat scripts and their handlers | **Covered** at command/handler scope by `map-script-engine-static-v1.json`, retained `cs_51750`, `cs_51742`, `cs_53620`, `cs_5362A`, `cs_53FD8`; [common scripting](./common-scripting.md). Command: `map-script-engine`. Retained comparison passes. | Awaited 129 versus non-awaited 128; repeat preserves supplied Bowie position and does not write quake; caller sets F624 after script completion. Command coverage does not prove rendered timing/completed motion. |
| F640 request → Map4 population → F624 init → control/event read | **Reuse/extend existing owners** if executable composition becomes necessary: `map-event-request-consumption`, `map-init`, `map-script-engine` and follower consumers in common scripting. | Missing claim: exact pending state/entity mapping at the post-init read for this caller. Existing rails cover separate parts, not natural composition. Minimal public-safe output: source/caller identity, input flags/player type, request/effective map, selected init, post-return F624 and event/control result. Controls: F640 clear, F624 set, F74 absent/conflicting F83, pending event at the read. No new standalone scene fixture/command is justified. |
| Tower/castle walking and repeat entry into changed layout | **Reuse/extend map-content** only for a separately accepted static reachability claim; otherwise **deferred runtime Unknown**. | Missing relation: legal traversal between the listed arrivals/triggers, especially Map60 portals and Map57's F506 copy. Minimum output would be conditional reachability, not a layout dump. Controls: F401 interception, F506/F999 copy changes, occupancy or incompatible followers. |
| Natural services, actor/follower state, presentation and pending value | **Deferred runtime Unknown**, preserving the predecessor's caller/service limits. | An observation must name its exact entry and consumer. These gaps do not authorize automatic H3 work or evacuation replay. No new bounded H2 implementation candidate is required for this prose boundary. |

## Reproduction and review

Bounded source/fixture/verifier reads and in-memory selections suffice. The two reused JSON
payloads total 18,285,979 bytes (about 18.3 MB) and are not rewritten. There is no new collection,
layout decode, full transcript/roster publication, extraction, installation or supporting platform.
Preserve predecessor natural-combat, service, walking/reentry and actor/follower Unknowns; original
static facts and composition remain separate from remake implementation.

Load the ignored configuration in each tool process. From the owning checkout, compare retained
payloads with the predecessor's existing verifier surface:

```powershell
. ./local/private-inputs.ps1
@'
import json
from pathlib import Path
from sf2tool.h2.map_script_engine import _verify_map_script_engine_fixture
read = lambda p: json.loads(Path(p).read_text(encoding="utf-8"))
f = read("tests/fixtures/h2/map-init-static-v1.json")
o = read("local/derived/map-init-static.json")
assert f["upstreamCommit"] == o["upstream"]["commit"]
assert f["romSha256"] == o["romSha256"] and f["function"] == o["function"]
metadata = {"schemaVersion", "id", "upstream", "romSha256", "function"}
assert f["expected"] == {k: v for k, v in o.items() if k not in metadata}
_verify_map_script_engine_fixture(
    read("tests/fixtures/h2/map-script-engine-static-v1.json"),
    read("local/derived/map-script-engine-static.json"))
print("retained map-init / map-script-engine comparisons: PASS")
'@ | uv run python -X utf8 -
```

Verify the pinned checkout with `git -C $upstream rev-parse HEAD` and
`git -C $upstream status --short`, then read the named relative files/labels/records. Inspect
`map-init`'s `expected.primarySourceBodies` for `ms_map4_InitFunction` and
`ms_map19_flag507_InitFunction`, `map-events`'s named programs and selected
`programCorpus.programs` command indices. Compare text operands with `gamescript.txt` keys
`02C9..02D0`. `$upstream` means a locally selected read-only checkout, never a published machine path.

Record direct prose/reference/link/private checks, `git diff --check`, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`, actual final-head CI and completed failures
on the Issue/PR handoff. Documentation-only scope selects no normal/full/engine/H3/H4/Godot suite.
Preserve local inputs/results, leave a clean pushed Draft PR and freeze the writer for independent
main-gate acceptance. Later evacuation remains a separate outcome.
