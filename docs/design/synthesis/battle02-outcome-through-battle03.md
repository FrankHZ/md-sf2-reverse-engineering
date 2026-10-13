# Battle 02 Outcome through Battle 03 Admission

After the encounter outside Granseal, a victory returns the party to the overworld. Yeel supplies
local information about Hawel and Kazin's offer to guide the party to him. A separate overworld
zone-event caller requests the next encounter. **Inferred story connection:** these authored contents
continue the commission described in
[Battle 01 return through Battle 02 admission](battle01-return-through-battle02.md).
**Confirmed static:** the accepted
[Battle02-to-Battle03 investigation](../../research/battle02-to-battle03-content-route.md) connects a
conditional victory return on Map66 to source Battle03 admission. **Unknown:** the naturally executed
walk, whether Yeel is visited or Kazin is consulted before the encounter, and the actual return and entry state.

This guide is for readers following locations, people, interactions and encounters, with evidence
links for review and fidelity work. It stops at Battle03 admission and selected entry dependencies;
Battle03 resolution and Hawel-house scenes need their own scope. The original baseline is the pinned
US ROM and `ShiningForceCentral/SF2DISASM` `master` at
`c834c652b6862bc5679fd7f69a38a7093206efc6`, identified by the
[research provenance](../../research/battle02-to-battle03-content-route.md#agreement-and-provenance).
No remake implementation supplies evidence here. Its
[accepted private milestone](map3-battle01-readiness.md#accepted-current-milestone) ends at Battle01
usable return; this guide grants no later playable support.

Here **Confirmed** means a named static fact or an explicitly bounded original observation;
**Inferred** describes the narrative relationship; **Unknown** preserves missing execution evidence.
“Battle02” and “Battle03” are zero-based source slots 2 and 3, `BATTLE_TO_YEEL` and
`BATTLE_TO_HAWEL_HOUSE`, rather than verified original UI numbering. Map66 is the source-named
Granseal Kingdom overworld; Yeel is Map5. The names explain content identities, not chronology.

## 1. The Outcome Returns Control to the Field

### What ends Battle02?

**Confirmed shared outcome rule:** after action death processing, and again after after-turn effects
and their death processing, the controller counts remaining combatants. A combatant must have a
placed X coordinate and positive HP to count. Zero allies selects defeat before zero enemies selects
victory; Bowie at zero HP forces the ally count to zero. Thus simultaneous zero counts do not select
victory. Battle02's enemy-leader table value is zero, so the defeated wrapper does not use its
leader-present tail to kill the remaining enemies. The supported win condition concerns all remaining
counted enemies, not merely enemy slot 128. The
[battle-control contract](../contracts/battle-control-lifecycle.md) and
[selected outcome analysis](../../research/battle02-to-battle03-content-route.md#conditional-transition-and-content-table)
own that relationship; they supply no winning action sequence for this encounter.

### Victory has an empty story program and a real shared join tail

**Confirmed selected dispatch:** if F502 is clear, Battle02's after-battle slot selects `ms_Empty`.
If F502 is already set, the wrapper skips the program. Both paths reach `EndAfterBattleCutscene`,
read join-table row 2 as zero, and call `JoinForce(0)`. Zero is Bowie's ally index; the consumer has
no zero-index skip. `JoinForce` sets joined membership, rebuilds the force and conditionally calls
`JoinBattleParty` when below capacity. This row names no additional story recruit.

The empty program contains the map-script terminator. It supplies no story-command dialogue, warp,
flag mutation or recruitment, but the interpreter's termination tail can wait for scrolling and clears
scrolling speed. Empty authored content therefore does not erase the wrapper, interpreter or join
consumer. The [cutscene-routing contract](../contracts/battle-cutscene-routing.md),
[party-membership contract](../contracts/party-membership-state.md) and
[accepted selected-source analysis](../../research/battle02-to-battle03-content-route.md#conditional-transition-and-content-table)
keep these effects with their respective owners. The built join table is
`data/battles/cutscenes/afterbattlejoins.asm`; the unused same-named global table is not this consumer's
input.

### The return position comes from the battle's final state

**Confirmed source recipe:** before the after-program wrapper, victory heals living/immortal allies
and calls `UpdateForceAndGetFirstBattlePartyMemberIndex`. It saves Map66 and that member's final
combatant X/Y plus the battle-area origin, together with the member's entity facing, into the
map-event return parameters. Parameter 1 is zero. These are final caller-dependent operands, not
starting placements or a fixed “return to Yeel” point.

After the wrapper returns, victory clears F402, sets F502, restores the saved parameters to `d0`–`d3`
and returns `d4=1`. `MainLoop` resumes at `alt_MainLoopEntry`, applies `SwitchMap` and calls
`ExplorationLoop` without an intervening `CheckBattle`. F403 is not written by this victory sequence.
**Confirmed static consequence:** victory hands control to field exploration; it does not immediately
launch Battle03. **Unknown:** the settled position/facing, naturally available input and onward walk.
The [accepted return owner](../../research/battle02-to-battle03-content-route.md#conditional-transition-and-content-table)
relates `battleloop_2.asm` (`BattleLoop_Victory`, `0x23CBA`) to `mainloop.asm`.

Map66 has one setup, `ms_map66`, with an empty ordinary entity list and an RTS init. Exploration
clears battle mode, initializes map entities/followers, loads the map and runs init. **Confirmed
structure:** an empty ordinary list does not mean an empty field: the leader and followers use their
own initialization. Nor is it a statement about the encounter's battle roster. Neither Map66 nor
Yeel has a flag-switched-map override; their setup selectors still have distinct local content rules.

Defeat and escape are alternate returns. **Confirmed static:** ordinary Battle02 defeat calls
`UpdateBattleUnlockedFlag`, restores Bowie's HP, halves gold, gets an Egress position and returns
`d4=-1`; Battle02 is not the special losable-battle exception. Egress/Angel Wing uses the unlock helper
and returns `d4=0`. That helper clears an unlock only if the battle was already completed; an ordinary
failed Battle02 is not marked complete. Exact destination/state and onward movement for these returns
remain **Unknown** here. They must not inherit the investigated victory's F402-clear/F502-set state.

## 2. Yeel Offers Information and a Guide

**Inferred content reading:** the village gives the commission a more local destination and a guide.
It is a conditional content branch, not a demonstrated prerequisite to Battle03. The actual admission
caller in the next section reads neither a Yeel-visit condition nor Kazin's F72.

**Confirmed warp records:** Map66 `(19,23)` targets Yeel `(21,42)`, Up; `(18,22)` targets Yeel `(7,1)`,
Down. Yeel's wildcard-X exits at Y=43 and Y=0 return to Map66 `(19,24)` and `(18,21)`, both Down.
These records connect locations in the table, not legal walks from the victory endpoint. Map66's
F508/F611 block copies and Yeel's persistent F940 church block copy and four door-step records are
additional working-layout dependencies. Markers, collision and occupied entities also matter. The
[map-content owner](../../research/map-content.md) and
[map-exploration contract](../contracts/map-exploration.md) retain that boundary without publishing a
layout.

### Which village content is selected?

**Confirmed early setup:** default `ms_map5` declares `ALLY_KAZIN` at `(7,6)`, Down, with village
residents, animals and a priest. Its init and default zone program return directly. The early reading
assumes the later selector flags 650, 530 and 28 are clear. When set, these alternatives override
in that source order: last set wins. F650 uses a different entity list/event table while reusing the
default init, zone, descriptions and item handler; it omits the explicit Kazin placement and default
Kazin event binding. F530/F28 select later shared handlers outside this branch.

The default entity-event table binds `ALLY_KAZIN`, with the Down-facing condition, to
`Map5_EntityEvent0`. The program is physically in `s2_entityevents_650.asm`, but the default table calls
it across that file boundary. **Confirmed routing:** the filename suffix is not an F650 requirement.
The [Yeel content investigation](../../research/battle02-to-battle03-content-route.md#yeel-branch-content-without-an-invented-prerequisite)
and [map-setup contract](../contracts/map-setup-data.md) own the selected bindings and shared selector.

### Kazin's offer changes following state

**Confirmed authored content:** fixed `txt` IDs 784–787 offer guidance to Kazin's master Hawel and
locate the house outside the village. Text 786 is an intervening narrative message; all four IDs must
not be assigned to one speaker merely from an actor sprite. These are fixed operands, not a
`textCursor` sequence. The caller invokes `cs_51B14`, then sets F72 after script return.

The script executes `followEntity ALLY_KAZIN,ALLY_BOWIE,2` and ends. `ALLY_KAZIN=4`, but this sequence
does not call `JoinForce` or set joined flag F4. F72 is associated with Kazin in
`data/scripting/entity/followers.asm` and consumed by `InitializeFollowerEntities`. **Confirmed static
relationship:** this establishes following/F72, not recruitment or battle unlock F403. The
[party/roster contract](../contracts/party-roster-state.md) keeps membership commands distinct, while
[common scripting](../../research/common-scripting.md) and the accepted Yeel analysis own the follower
consumer.

Follower initialization precedes setup-entity initialization; an already indexed ally entry is skipped.
Consequently the default Kazin placement does not prove that a second live Kazin appears after reentry.
F650's omission of that placement likewise does not remove a follower supplied by follower state.
**Unknown:** naturally displayed following, actual reentry population and save/load persistence.

### Local interactions give context without unlocking the encounter

| Early Yeel interaction | Confirmed content or branch | Limit |
| --- | --- | --- |
| Entity event 5, fixed IDs 767–769 | Describes Hawel and gives a northwest direction. | Local guidance does not establish a walkable route to the house. |
| Entity event 10, fixed IDs 775–777 | Reports Galam soldiers asking about Hawel. | This supplies authored story context, not a verified chronology of their movements. |
| Entity event 13 | Reads F72 and selects 782–783 or 788–789. | Following state changes the local response; no F403 writer follows from this branch. |
| Entity event 14 | Calls the church service. | Service outcome, presentation and persistence are outside this content observation. |

The default table and bodies in `s2_entityevents_650.asm` supply these **Confirmed static** relations.
The [dialogue contract](../contracts/dialogue-system.md) and
[service contract](../contracts/service-interactions.md) own the shared services. **Unknown:** which
interactions occur, rendered speech and their completion timing in a natural journey. The guide
conversation and village exits support an **Inferred** route toward Hawel, not a mandatory visit
before the encounter.

## 3. The Overworld Zone Caller Requests Battle03

Returning to or remaining on Map66 is not sufficient to invoke its default event. **Confirmed
controller condition:** controlled movement must inspect a target with masked zone marker `$1400`.
The event consumer passes requested X/Y to `RunMapSetupZoneEvent`, which uses the first matching row.
Map66's wildcard-X rows at Y=9, Y=10 and Y=28 precede its default:

- Y=9/10 select `Map66_ZoneEvent0`; F537 set and F538 clear write F438 and request a reload.
- Y=28 selects `Map66_ZoneEvent2`; F538 set and F539 clear write F439 and request a reload.

If one of those rows matches but its flags select a direct return, it still does not fall through to
the default. An ordinary tile is not a default-zone trigger, and matching coordinates alone do not
prove a marker is reached. The
[zone-dispatch investigation](../../research/battle02-to-battle03-content-route.md#conditional-transition-and-content-table)
joins movement, `ProcessMapEventType6_ZoneEvent` and `RunMapSetupZoneEvent` (`0x4751A`).

**Confirmed exact default caller:** `Map66_DefaultZoneEvent` at `0x4FAD4` loads `d0=3` and calls
`CheckRandomBattle` at `0x4FAD8`. The helper distinguishes first and completed encounters:

| Entry condition | Confirmed static request behavior |
| --- | --- |
| F503 clear | Selects the request path without reading `STEP_COUNTER` or drawing RNG. |
| F503 set, nonzero `STEP_COUNTER` | Makes no request. |
| F503 set, zero `STEP_COUNTER` | Draws with range 8. First result zero requests; a nonzero first result draws with range 4 and stores that result +2 as the step counter instead. |

The request path sets F403, writes longword `$000100FF` at `MAP_EVENT_TYPE`, sets
`STEP_COUNTER=30000`, waits for scrolling, requests `SFX_BOOST` and calls the flash service.
**Confirmed static order:** an incomplete encounter is requested deterministically at this selected
caller, despite the helper's random-battle name. F502, F72 and a Yeel visit are not its gates.
The helper preserves the caller's `d1/d6/d7`; its internal `d1` selection is not a returned success
flag. The [exact random-caller owner](../../research/map-event-random-battle-state.md) and
[accepted route investigation](../../research/battle02-to-battle03-content-route.md#conditional-transition-and-content-table)
retain the fixture and source/H1/ROM joins. **Unknown:** natural default-zone reach, wait completion,
sound/flash delivery and input availability.

## 4. The Pending Request Becomes Encounter Admission

**Confirmed conditional consumer order:** after the zone handler returns, `WaitForEvent` polls the
pending type-1 request and `ProcessMapEvent` clears its type before dispatch. Parameter 1 is zero;
parameter 2 is `MAP_CURRENT`. `ProcessMapEventType1_Warp` therefore exits exploration to `MainLoop`,
which applies map selection and `CheckBattle` before entering `BattleLoop` for a returned index.
This separates the unlock/request producer from the later encounter selector.

The long write covers only the type word and parameter bytes 1/2 (`0xFFA84A..0xFFA84D`). Parameter
3/4 bytes remain retained signed coordinate overrides, and parameter 5 remains retained facing.
**Confirmed source boundary:** this request is not a fresh position/facing snapshot. `CheckBattle`
resolves `MAP_CURRENT` from `CURRENT_MAP`; row 3's wildcard X/Y triggers make the unknown retained
coordinates immaterial to that row's selection, but not to a claim about the actual field endpoint.
The [request-consumption owner](../../research/map-event-request-consumption.md) owns polling/dispatch;
the [accepted consumer composition](../../research/battle02-to-battle03-content-route.md#conditional-transition-and-content-table)
owns the downstream type-1 and selector relationship.

With current Map66, F402 clear from the investigated victory and F403 set by the selected request,
**Confirmed conditional admission:** row 3 selects Battle03, writes area `(0,0,22,30)` and returns
index 3. The rectangle is output origin/size, not a player-coordinate eligibility test. This is the
primary stopping boundary, assuming the selected services return successfully.

Earlier eligible rows can win. If an alternate caller has F402 set, row 2 wins on Map66 even if
F502 is set: `CheckBattle` clears F402 but still returns index 2 on that same call. Similarly, if
selected row 3 has F403/F503 both set, it clears F403 and still returns index 3 on that call. A later
call sees the changed flags. With F403 clear, selection may continue to later unlocked Map66 rows;
no universal “no encounter” result is implied. The
[common-map owner](../../research/common-maps.md#confirmed-map-routing) supplies the shared scan,
and the accepted route supplies the selected row-3 operands. No proposed composed fixture is treated
as already implemented.

## 5. What the Selected Battle03 Entry Declares

**Confirmed source declarations:** row 3 of `battlemapcoords.asm` is `[66,0,0,22,30,255,255]`.
Spriteset slot 3 selects `BattleSpriteset03` (`0x1B33F4`), whose header declares 12 ally placements,
6 enemies, 1 AI region and no AI points. Twelve placement rows do not establish twelve joined or
active allies. Terrain slot 3 selects `BattleTerrain03`; its later alias does not establish story
order. The [encounter contract](../contracts/battle-encounter-definition.md),
[spriteset owner](../../research/battle-spriteset-data.md) and
[terrain/routing owner](../../research/battle-routing-data.md) retain data and payload boundaries.
No roster table or terrain grid is needed to explain this admission.

The before-battle slot selects `bbcs_03` (`0x4980E`), which executes `csub_49816` to write
`EGRESS_MAP=MAP_YEEL`, then ends. **Confirmed entry-side assignment:** this chooses a return-destination
value on the selected branch; it does not show that Yeel was previously visited. The program supplies
no dialogue cursor, recruitment or F403 write.

This assignment is conditional. `ExecuteBeforeBattleCutscene` tests F453 without setting it. The
start wrapper also tests F453 and sets it before dispatching slot 3 to `ms_Empty`. If F453 was already
set, both dispatches are skipped; the F88 suspended-battle path bypasses this normal new-battle
preparation as well. **Confirmed shared order:** with F88 clear, `BattleLoop` installs the current
map/battle, runs before-battle, clears region flags, heals/initializes combatants, loads battle content
and runs start. The [cutscene-routing contract](../contracts/battle-cutscene-routing.md#shared-intro-gate)
and [selected Battle03 dependencies](../../research/battle02-to-battle03-content-route.md#source-battle03-entry-dependencies)
own the F453 and Egress conditions. The shared controller's Battle01 evidence does not prove an
initialized Battle03 party, activation or first turn.

## Evidence Review and Remaining Limits

This synthesis reviewed the accepted investigation's pinned-source bindings, the preceding guide and
current readiness boundary, and the selected public payload/verifier surfaces below. Selected source
findings remain attributed to their research owner; aggregate fixtures are not silently specialized.
Commands are reproduction routes, not fresh runs in this synthesis. No new fixture or original
observation was produced.

| Material claim | Reviewed executable owner and narrow reproduction route | Disposition |
| --- | --- | --- |
| Outcome and ordinary return | [battle-loop fixture](../../../tests/fixtures/h2/battle-loop-static-v1.json), `sf2-battle-loop-static-v1`, `remainingCombatants`; [control fixture](../../../tests/fixtures/h2/battle-control-static-v1.json), `sf2-battle-control-static-v1`; [loop](../../../src/sf2tool/h2/battle_loop.py) / [control](../../../src/sf2tool/h2/battle_control.py) verifiers; `uv run sf2 h2 battle-loop`, `uv run sf2 h2 battle-control` | Count/outcome facts and ordered source fragments cover shared structure. The selected final-state return recipe and Map66 handoff are accepted source findings; the reviewed fixtures do not contain a composed Battle02 victory witness. |
| Empty after-program, join and intro gate | [cutscene fixture](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), `sf2-battle-cutscenes-static-v1`; [verifier](../../../src/sf2tool/h2/battle_cutscenes.py), `uv run sf2 h2 battle-cutscenes`; [script fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json), `sf2-map-script-engine-static-v1`, [verifier](../../../src/sf2tool/h2/map_script_engine.py), `uv run sf2 h2 map-script-engine` | Shared completion/join and intro ordering agree. Selected row 2, zero-index JoinForce and terminator consumer use the investigation's named source; the historical Battle01 fixture is not rewritten as Battle02 proof. |
| Map selection and local content structure | [setup](../../../tests/fixtures/h2/map-setup-static-v1.json), `sf2-map-setup-static-v1`; [entities](../../../tests/fixtures/h2/map-entities-static-v1.json), `sf2-map-entities-static-v1`; [content](../../../tests/fixtures/h2/map-content-static-v1.json), `sf2-map-content-static-v1`; [setup](../../../src/sf2tool/h2/map_setup.py), [entity](../../../src/sf2tool/h2/map_entities.py), [content](../../../src/sf2tool/h2/map_content.py) verifiers; `uv run sf2 h2 map-setup`, `uv run sf2 h2 map-entities`, `uv run sf2 h2 map-content` | Shared selector/consumer facts do not prove a legal walk. Individual Map05/Map66 bindings and warp records remain selected-source findings, not invented aggregate fixture cases. |
| Kazin caller and zone caller | [entity programs](../../../tests/fixtures/h2/map-events/entity-programs.json), [zone programs](../../../tests/fixtures/h2/map-events/zone-programs.json) and [script invocation](../../../tests/fixtures/h2/map-events/script-invocation.json), sections of `sf2-map-events-static-v1`; [verifier](../../../src/sf2tool/h2/map_events.py), `uv run sf2 h2 map-events` | Selected `Map5_EntityEvent0:334530` retains fixed text operands, script then F72; `Map66_DefaultZoneEvent:326356` retains battle 3 then the helper call. `followEntity` handler/call structure in the script fixture is distinct from force membership; follower initialization remains with the accepted source consumer. |
| First versus repeat request | [random-battle fixture](../../../tests/fixtures/h2/map-event-random-battle-state-static-v1.json), `sf2-map-event-random-battle-state-static-v1`; [verifier](../../../src/sf2tool/h2/map_event_random_battle_state.py), `uv run sf2 h2 map-event-random-battle-state` | Reviewed `callerSites.map66-default-battle3`, `completionFlagGate`, `stepCounterGate`, `randomCadence`, `requestWriteSequence`, `returnContract`. Complete semantic comparison guards this caller/body structure; downstream admission and presentation are excluded. |
| Request consumption and ordered battle selection | [request fixture](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), `sf2-map-event-request-consumption-static-v1`; [request verifier](../../../src/sf2tool/h2/map_event_request_consumption.py), `uv run sf2 h2 map-event-request-consumption`; [common maps](../../../tests/fixtures/h2/common-maps-static-v1.json), `sf2-common-maps-static-v1`, [verifier](../../../src/sf2tool/h2/maps.py), `uv run sf2 h2 common-maps` | Request fixture stops at poll/clear/dispatch, and common-map fragments do not compose the selected row. Retained parameters and row-2/row-3 same-call returns use accepted source analysis. Its proposed conditional-admission extension remains unimplemented. |
| Entry declarations and selected cutscenes | [spritesets](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json), `sf2-battle-spriteset-data-static-v1`; [routing](../../../tests/fixtures/h2/battle-routing-data-static-v1.json), `sf2-battle-routing-data-static-v1`; [spriteset](../../../src/sf2tool/h2/battle_spriteset_data.py) / [routing](../../../src/sf2tool/h2/battle_routing_data.py) verifiers; `uv run sf2 h2 battle-spriteset-data`, `uv run sf2 h2 battle-routing-data` | `rowContentTracked=false` and `routeTargetsParsed=false` preserve inventory limits. Battle03's selected header, before/start targets and conditional EGRESS_MAP write are sourced by the investigation, not live-entry proof. |

The strict boundaries agree: there is no evidence disagreement requiring an owner correction.
The [research gap disposition](../../research/battle02-to-battle03-content-route.md#evidence-gaps-and-fixture-disposition)
identifies existing owners for the missing composed witnesses. Proposed extensions are not execution
authority, and generic runtime-question queues are not automatic acquisition work.

**Unknown** remains: the naturally attained Battle02 outcome/caller state and settled return endpoint;
marker/collision/occupancy-aware walking; which Yeel content is selected or visited and when; actual
following, dialogue and service delivery; initialized Battle03 membership/activation/first turn; and
save/load persistence. **Inferred** remains the intended journey toward Hawel. Admission, optional
content and entry-side destination assignment must not be folded into a demonstrated continuous
walkthrough. The [roadmap](../documentation-roadmap.md#current-baseline-and-near-term-synthesis) owns
further scope, and [Story Progression](story-progression.md) explains the wider state relationships.
