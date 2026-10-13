# Battle 03 Outcome through Battle 04 Admission

Battle03 can return the party to the overworld. Hawel's house supplies the next source-connected
scene: Kazin finds his master, gains a reason to join the force and unlocks another encounter.
Leaving through the house's exit then supplies the request from which Battle04 can be selected.
**Inferred story connection:** these authored contents continue the journey toward Hawel in
[Battle 02 outcome through Battle 03 admission](battle02-outcome-through-battle03.md).
**Confirmed static:** the accepted
[Battle03-to-Battle04 investigation](../../research/battle03-to-battle04-content-route.md) connects
these conditional consumers. **Unknown:** the natural walk, actual Battle03 return state, house
caller/follower state and initialized Battle04 party. Battle03 completion is not established as a
mandatory house-scene predicate.

This guide explains locations, people, scene content and state handoffs. It stops at Battle04
admission and selected before/start dependencies. Original evidence uses the USA retail ROM and
`ShiningForceCentral/SF2DISASM` `master` at `c834c652b6862bc5679fd7f69a38a7093206efc6`, identified by
[the research provenance](../../research/battle03-to-battle04-content-route.md#agreement-and-provenance).
**Confirmed** denotes a named static fact or explicitly bounded observation; **Inferred** denotes the
narrative connection; **Unknown** names missing execution evidence. Battle03 and Battle04 are
zero-based source slots 3 and 4 (`BATTLE_TO_HAWEL_HOUSE` and `BATTLE_AMBUSHED_BY_GALAM_SOLDIERS`), not
verified original UI numbering. Hawel's house is Map43; the Granseal Kingdom overworld is Map66.

No remake implementation is evidence about the original. The
[accepted private remake milestone](map3-battle01-readiness.md#accepted-current-milestone) ends at
Battle01 usable return; this guide grants no later playable support.

## 1. Battle03 Hands Control Back to Exploration

**Confirmed shared outcome rule:** the controller counts combatants after action death processing,
and again after after-turn effects and their death processing. A nonnegative X and positive HP are
required to count. Zero allies selects defeat before zero enemies selects victory; Bowie's zero HP
forces the ally count to zero. Battle03's enemy-leader-presence row is zero, so killing enemy slot
128 alone is not its victory condition. The separate defeated-enemy wrapper also selects `ms_Empty`
for this encounter; it supplies neither a house warp nor a substitute outcome rule. The
[battle-control contract](../contracts/battle-control-lifecycle.md) and
[selected outcome owner](../../research/battle03-to-battle04-content-route.md#conditional-transition-table)
retain the shared rule and selected source relationships without supplying a winning action sequence.

### The empty after-program does not recruit Kazin

**Confirmed selected dispatch:** F503 clear selects after-battle row 3's `ms_Empty`; F503 set skips
that program. Both paths reach the shared join tail, read zero from row 3 of the built
`data/battles/cutscenes/afterbattlejoins.asm`, and call `JoinForce(0)`. Zero is Bowie, not a no-call
sentinel. The empty script contributes no story dialogue, warp, flag or join command, although its
interpreter termination can wait for scrolling and clears scrolling speed. The wrapper's real join
call remains. Kazin's recruitment belongs to the house scene below.

The [cutscene-routing contract](../contracts/battle-cutscene-routing.md) and
[party-membership contract](../contracts/party-membership-state.md#joinforce-chronology) keep dispatch,
script effects and force membership distinct. The unused namesake join table under `global/` is not
this wrapper's input. Absence of a per-battle script filename is not evidence of an empty target.

### Victory returns final-state operands, then the field waits for another event

**Confirmed return recipe:** before the after-program wrapper, victory heals living/immortal allies,
updates the force and obtains the first battle-party member. It saves the current map, that member's
final combatant X/Y plus battle-area origin, and the member's entity facing in return parameters
2–5; parameter 1 is zero. The Map66 caller therefore returns to Map66 using final caller-dependent
operands, not starting placements or a fixed house-entry coordinate.

After the wrapper, victory clears F403, sets F503 and returns the saved map/position/facing in
`d0`–`d3`, with `d4=1`. `MainLoop` resumes at `alt_MainLoopEntry`, applies `SwitchMap` and enters
`ExplorationLoop` without first rechecking battles. F404 is not written here. **Confirmed static
consequence:** the next encounter needs its own unlock and request path. **Unknown:** the settled
position/facing, available input and onward walk. The accepted
[return analysis](../../research/battle03-to-battle04-content-route.md#conditional-transition-table)
joins `BattleLoop_Victory` (`0x23CBA`) and the battle-return site in `mainloop.asm` (`0x75E0`).

Defeat and escape remain alternatives. **Confirmed static:** ordinary Battle03 defeat invokes the
unlock helper, restores Bowie, halves gold, gets the egress position and returns `d4=-1`;
Egress/Angel Wing uses that helper and returns `d4=0`. The helper clears F403 only when F503 was
already set; an ordinary failed first attempt or escape does not mark Battle03 complete. The
predecessor's Battle03 before-program can select `EGRESS_MAP=MAP_YEEL`, but only on its unplayed-intro,
unsuspended branch. Actual failed/escaped destinations and later walking remain **Unknown**.

## 2. Entering Hawel's House Selects Initialization Content

**Confirmed warp relation:** Map66's `(6,5)` record requests Map43 `(6,11)`, Up, no-scroll. A
controlled target must supply the warp marker and reach `WarpIfSetAtPoint`. This table connection
does not prove a legal walk from the final Battle03 return position. Map66's F508/F611 block-copy
state and the house's step/roof records and scripted block changes remain walking dependencies.
The [map-exploration contract](../contracts/map-exploration.md) and
[house content owner](../../research/battle03-to-battle04-content-route.md#hawel-house-content-and-caller-state)
keep the working layout, marker, collision and actor conditions separate from the warp tuple.

### The scene runs on default map initialization

**Confirmed setup selection:** Map43's only flag selector is F612. With it clear, the selected init
`ms_map43_InitFunction` (`0x540A6`) calls the embedded `cs_540C0` (`0x540C0`). With F612 set, the
alternate setup selects an RTS init and an empty authored entity list. Both share default entity
and zone event stubs that return directly. The scene is initialization content, not an interaction
with Hawel or a local object; books/well descriptions supply context without writing F404.
Neither F503 nor F72 is an explicit gate in this setup/default-init caller.

**Confirmed actor structure:** default setup declares Hawel (logical entity 128) and two Galam
soldiers (129/130). Hawel starts off-map at `(63,63)` and the program positions him at `(7,4)`.
The stream falls through into the alternate list's terminator. An empty alternate list does not
remove the independently initialized leader and followers. Neither Map43 nor Map66 has a
flag-switched-map override. The [map-setup contract](../contracts/map-setup-data.md) owns table
structure; map exploration and the accepted research own selector/init behavior.

### Kazin's presence has a separate follower path

The default house list does not declare Kazin. **Confirmed source relationship:** exploration
initializes followers before setup entities; the follower table binds F72 to Kazin and its consumer
declares him when F72 is set. The preceding Yeel conversation calls the follow script and then sets
F72 without recruiting him. This supplies a source-backed presence path for the house scene.
Being joined does not itself substitute for that F72 declaration.

**Unknown:** naturally attained F72/entity state on house entry. No F72 gate does not prove the
choreography works with Kazin absent, and it does not prove Yeel was mandatory. The absence of an
F503 gate likewise does not establish a continuous journey from a completed Battle03. These are
caller-state questions, not missing predicates to invent in the scene.

## 3. The House Scene Changes Kazin's Relationship to the Party

**Confirmed authored content:** the following short summaries use `cs_540C0` command/actor operands
and matching IDs in pinned `data/scripting/text/gamescript.txt`, as reviewed by the
[accepted scene analysis](../../research/battle03-to-battle04-content-route.md#hawel-house-content-and-caller-state).
They describe authored content rather than newly observed rendered speech or independent proof of
what happened off-screen. Full dialogue and choreography remain private.

| Scene phase | Dialogue identity and actor | Content relationship |
| --- | --- | --- |
| Arrival and intrusion | Normal cursor IDs 811–816; Kazin, except soldier 129 at 815 | Kazin identifies the house, finds no response and discovers Hawel with soldiers inside. The program repositions Bowie/Kazin, reloads the current map with camera `(0,4)`, and applies explicit roof/step events. |
| Hawel's account | IDs 817–826; Hawel 128, Kazin and Bowie's 822 whisper | After scripted soldier departure, Hawel describes soldiers demanding his Ground Seal notes. Kazin relates the seal's opening and Granseal's disaster. The statements supply the scene's explanation, not independent off-screen evidence. |
| Warning and loss | IDs 827–831; Hawel/Kazin | Hawel explains the sealed danger and jewels and leaves a final request. The program shivers/stops him and moves him off-map with `setPosFlash`; Kazin mourns. This presentation sequence is not a combatant-death write for Hawel. |
| Shared purpose | IDs 832–834; Kazin | Kazin asks to accompany the party to fulfill Hawel's wish, joins between 833 and 834, then points toward the Ancient Shrine. Following and unlock operations come afterward. |

The normal text path installs cursor 811 and advances 24 times through 834. Continuing `nextText`
calls are 829 and 832; the other selected calls close their text. The scene contains no yes/no
branch for Kazin's request. The join handler separately displays fixed message 446 without consuming
this scene cursor. The [dialogue contract](../contracts/dialogue-system.md) owns cursor and display
handler semantics. Debug/P2-Start skipping can suppress ordinary text and its cursor increments;
these ranges describe the normal branch, not every execution.

### Joined membership, following and encounter unlock are distinct

**Confirmed ordered tail:** sad join → final scene text → `followEntity` Kazin/Bowie → `clearF 72`
→ `setStoryFlag 4` → termination. Its state domains must remain distinct:

| Operation | Confirmed consumer effect | Limit |
| --- | --- | --- |
| Sad `join` for Kazin | `csc08_joinForce` reads `0x8004`, clears bit 15 for ally identity, selects sad join music and calls `JoinForce(4)`, setting joined F4. | Active-party activation is conditional on capacity 12. The force rebuild precedes the active-flag set; synchronized active lists and the actual Battle04 roster are not established by this call. |
| `followEntity` and `clearF 72` | The follow consumer resolves entity addresses and calls `AddFollower`; the clear removes the flag used by later follower initialization. | Following is not joining. F72 clear does not remove Kazin from the force; naturally displayed following/reentry remains Unknown. |
| `setStoryFlag 4` | `csc13_setStoryFlag` adds the battle-unlocked base 400 and sets F404. | It does not set F4 or F504, clear earlier encounter unlocks, or request a transition. |

The [party/roster](../contracts/party-roster-state.md), [party-membership](../contracts/party-membership-state.md)
and [common-scripting](../../research/common-scripting.md) owners retain these consumer boundaries.

### Caller writes happen after the scene returns

**Confirmed lifecycle distinction:** the earlier `reloadMap` calls `LoadMap` for the current map.
It neither recursively runs setup init nor enqueues the house exit. After `cs_540C0` returns, its
ASM caller sets F612, sets F650, clears F72 again and writes `EGRESS_MAP=43`, then returns. F612
selects the house's repeat setup; F650 selects Yeel's alternate setup subject to its later selector
priorities. The egress value is separate from a warp request. None is a row-4 admission predicate.

The house does not automatically leave after the scene. A supplied F612-set/F404-clear state selects
the RTS init and does not recreate the missing unlock. **Unknown:** natural scene completion,
settled input, revisit population and save/load persistence of these changes.

## 4. The House Exit Supplies the Request for Battle04

**Confirmed exit producer:** Map43's only warp matches wildcard X and Y=14, requesting Map66
`(6,6)`, Down, no-scroll. On a matching invocation, `WarpIfSetAtPoint` writes event type 1,
parameter 1 zero, destination map/X/Y in parameters 2–4 and facing at parameter 5. These are the
exit's operands, not a Battle03 return snapshot and not the preceding random helper's retained
coordinate request. Walking to the exit marker after the scene remains **Unknown**.

`WaitForEvent` polls the request and `ProcessMapEvent` clears its type before dispatch.
**Confirmed conditional consumer:** parameter 1 zero makes `ProcessMapEventType1_Warp` exit
exploration to `MainLoop`; nonnegative destination coordinates override current player position.
A nonzero parameter 1 would instead scroll within exploration. `MainLoop` then selects the map and
checks encounters. The [request-consumption owner](../../research/map-event-request-consumption.md)
owns poll/clear/local dispatch; the accepted
[exit/admission analysis](../../research/battle03-to-battle04-content-route.md#returned-request-and-battle04-entry-dependencies)
owns the downstream composition.

With Map66 supplied, F402/F403 clear and F404 set, **Confirmed conditional selection:** `CheckBattle`
returns row 4 and area `(0,0,22,30)`. Both trigger coordinates are wildcards (255); the area is output
origin/size, not an admission rectangle. The unlock from the scene and the actual exit producer
are independent steps, assuming intervening services return successfully.

Selection scans source order. Earlier unlocked Map66 row 2 or 3 preempts row 4 even if its completion
flag is set: the selector clears that row's unlock and still returns its index on the same call.
F404/F504 both set similarly returns 4 while clearing F404. Without F404, scanning may reach later
unlocked rows. Map66's default zone helper can also rearm completed Battle03 under its step/RNG
conditions, as owned by the preceding guide. The scene or numeric encounter order therefore cannot
replace the real selector. The [common-map research](../../research/common-maps.md#confirmed-map-routing)
retains the shared scan; a proposed composed admission witness is not already implemented.

## 5. The Selected Battle04 Introduction and Start

**Confirmed selected branch:** with F88 clear and F454 clear, before-table row 4 selects `bbcs_04`
(`0x4981E`). It installs cursor 2311, loads Map66 with camera `(1,5)` and a separate scene entity
table `ce_4989E`. IDs 2311–2318 have Kazin confront the soldiers/Lemon about Hawel and Lemon order
capture. Bowie/Kazin, named companions, Lemon and soldier sprites are staged entities, not proof
of active battle membership. This introduction contains no join or F404 write; the requested future
action in dialogue does not establish an encounter outcome.

On normal preparation the controller installs map/battle for before, restores its supplied values
afterward, clears region flags 90–105, heals, initializes allies/enemies, clears AI memory, loads
the battle and then calls start. Spriteset slot 4 selects `BattleSpriteset04` (`0x1B34DC`), declaring
5 ally placements, 8 enemy entries, 3 AI regions and 1 AI point. Placement capacity is not joined
or active membership. Terrain slot 4 aliases `BattleTerrain03`; sharing terrain does not make
encounter content identical. The [encounter contract](../contracts/battle-encounter-definition.md),
[spriteset research](../../research/battle-spriteset-data.md) and
[selected entry owner](../../research/battle03-to-battle04-content-route.md#returned-request-and-battle04-entry-dependencies)
retain these data boundaries without publishing a roster or grid.

The before wrapper tests F454 without setting it. The start wrapper sets F454 before dispatching
row 4 to `ms_Empty`; F454 already set skips both programs. F88's suspended entry bypasses ordinary
preparation and these wrappers. The [shared intro contract](../contracts/battle-cutscene-routing.md#shared-intro-gate)
therefore does not promise the introduction replays on every admission. **Unknown:** naturally
initialized Battle04 membership, service completion, presentation and first turn. Combat resolution
and all later story remain outside this guide.

## Evidence Review and Remaining Limits

This synthesis reviewed the accepted research, preceding guide, contracts linked above and selected
public payload/verifier semantics below. Original selected-source findings remain attributed to the
research owner. Reproduction routes are not fresh runs in this synthesis; no new fixture or original
observation was produced. Load `local/private-inputs.ps1` in the owning process before using a route.

| Material claim | Reviewed executable owner and reproduction route | Disposition |
| --- | --- | --- |
| Shared outcome/return | [loop fixture](../../../tests/fixtures/h2/battle-loop-static-v1.json), `sf2-battle-loop-static-v1`, `lifecycleFacts.remainingCombatants`; [control fixture](../../../tests/fixtures/h2/battle-control-static-v1.json), `sf2-battle-control-static-v1`, `controlFacts`; [loop](../../../src/sf2tool/h2/battle_loop.py) / [control](../../../src/sf2tool/h2/battle_control.py) verifiers; `uv run sf2 h2 battle-loop`, `uv run sf2 h2 battle-control` | Shared counts and lifecycle facts agree. Battle03's final-state recipe, selected rows and Map66 return are accepted source findings, not a composed battle-3 runtime witness. |
| Empty programs/shared join and intro | [cutscene fixture](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), `sf2-battle-cutscenes-static-v1`, `cutsceneFacts`; [verifier](../../../src/sf2tool/h2/battle_cutscenes.py), `uv run sf2 h2 battle-cutscenes` | Wrapper gates/join chronology are guarded. Exact slot targets and zero-index JoinForce use the research's source consumers. An empty program does not erase its wrapper/interpreter effects. |
| House init and caller | [init fixture](../../../tests/fixtures/h2/map-init-static-v1.json), `sf2-map-init-static-v1`, `expected.routeJoins` 93/94, `scriptCallSites`, `primarySourceBodies`, `embeddedPrograms[id=cs_540C0]`; [verifier](../../../src/sf2tool/h2/map_init.py), `uv run sf2 h2 map-init` | Complete semantic comparison includes the selected init call and post-return writes. The alternate RTS path does not replay the scene. Embedded operation indexes and the script fixture's command indexes use different counting domains. |
| Membership, dialogue, follower and unlock | [script fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json), `sf2-map-script-engine-static-v1`, force/story-state source sites and handlers, map-lifecycle `cs_540C0:8:reloadMap`; [verifier](../../../src/sf2tool/h2/map_script_engine.py), `uv run sf2 h2 map-script-engine` | Reviewed source sites retain join 109, clearF 112 and setStoryFlag 113; reload is command 8. Bit masking, fixed join text and 400-base arithmetic are distinct from natural speech/following. Follow command 111/AddFollower, actor/text pairing and F72 declaration use accepted source analysis. |
| Setup/entities/warp structure | [setup](../../../tests/fixtures/h2/map-setup-static-v1.json), `sf2-map-setup-static-v1`; [entities](../../../tests/fixtures/h2/map-entities-static-v1.json), `sf2-map-entities-static-v1`, Map43 fallthrough; [content](../../../tests/fixtures/h2/map-content-static-v1.json), `sf2-map-content-static-v1`; [setup](../../../src/sf2tool/h2/map_setup.py), [entities](../../../src/sf2tool/h2/map_entities.py), [content](../../../src/sf2tool/h2/map_content.py) verifiers; `uv run sf2 h2 map-setup`, `uv run sf2 h2 map-entities`, `uv run sf2 h2 map-content` | Structural facts do not prove marked, collision/occupancy-aware walking. Exact warp and follower relationships remain with the selected source owner. Empty setup entities do not establish an empty live population. |
| Exit request and encounter selector | [request fixture](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), `sf2-map-event-request-consumption-static-v1`; [common-map fixture](../../../tests/fixtures/h2/common-maps-static-v1.json), `sf2-common-maps-static-v1`; [request](../../../src/sf2tool/h2/map_event_request_consumption.py) / [map](../../../src/sf2tool/h2/maps.py) verifiers; `uv run sf2 h2 map-event-request-consumption`, `uv run sf2 h2 common-maps` | Request evidence stops at local dispatch; inventory does not compose exit → Map66 → row 4. That relationship and earlier/same-call selection counterexamples use accepted source review; the proposed existing-owner extension remains unimplemented. |
| Entry data | [spriteset](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json), `sf2-battle-spriteset-data-static-v1`; [routing](../../../tests/fixtures/h2/battle-routing-data-static-v1.json), `sf2-battle-routing-data-static-v1`; [spriteset](../../../src/sf2tool/h2/battle_spriteset_data.py) / [routing](../../../src/sf2tool/h2/battle_routing_data.py) verifiers; `uv run sf2 h2 battle-spriteset-data`, `uv run sf2 h2 battle-routing-data` | `rowContentTracked=false` and `routeTargetsParsed=false` retain inventory limits. Slot-4 targets/header and before/start order use the selected research source, not live-party evidence. |

The controller contract's New Battle overview groups the before/start seams; it is not used as a
serial witness placing start before initialization. The precise before → preparation → start order
above follows the accepted route's source analysis and shared intro-gate owner. Generic runtime
question queues likewise do not override these stricter local boundaries. No conflicting executable
claim requires expanding this slice. The
[research gap disposition](../../research/battle03-to-battle04-content-route.md#evidence-gaps-and-fixture-disposition)
keeps composed witness proposals with existing owners; proposals do not authorize new implementation
or automatic emulator work.

**Inferred** remains the intended next story segment. **Unknown** remains the naturally attained
outcome/return state, marker/collision/actor-aware walking, house F72/entity state (including
missing-follower choreography), normal scene and service completion, rendered speech/input/audio,
reentry/save persistence and initialized Battle04. These gaps limit a continuous walkthrough and
remake admission claim, while preserving the useful conditional content explanation. The
[roadmap](../documentation-roadmap.md#current-baseline-and-near-term-synthesis) owns future scope;
[Story Progression](story-progression.md) provides the wider state framework.
