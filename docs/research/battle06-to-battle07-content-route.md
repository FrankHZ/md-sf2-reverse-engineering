# Battle 06 outcomes through Granseal to source Battle 07 admission

Evidence date: 2026-10-12. **Confirmed:** Battle06 has empty defeated/after script slots and no
Battle05-style leader-death position override. Victory clears F406, sets F506 and returns to the
current map at the first battle-party member's final position plus battle-area origin. Its already
executed before-program, not victory, supplies F407. F506 selects Granseal's invasion aftermath,
castle events and a tower-entrance block copy. Selected no-scroll warp records connect these maps
to Map58, where F407 admits Battle07 if the request survives map switching. **Inferred:** this is
the intended return to Granseal and pursuit of Galam with Elis into the Ancient Tower. **Unknown:**
natural victory, continuous walking, arbitrary reentry/entity state and completed presentation.
The conditional source connection does not establish a playable bypass or unavoidable chronology.

## Agreement and provenance

This is the Standard-rigor scope agreement for
[Issue #712](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/712), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514). Outcome, map,
party and encounter consumers require explicit cross-system predicates. The sole tracked write is
this document. The accepted starting base is `61fc72637f64638d35f6ddd15c4a320a192c13e8`, including
[the Battle05-to-Battle06 owner](./battle05-to-battle06-content-route.md) from
[PR #710](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/710). Concurrent synthesis has
separate ownership; there is no stacked dependency or shared-path write.

Scope is Battle06 outcome/defeated/after/join/return, immediate Granseal/tower content and conditional
Battle07 admission through selected before/start dependencies. Stop before Battle07 resolution.
Later campaign, full path/layout decoding, winnability, new fixtures/tooling, acquisition, emulator,
engine/UI and Chinese translation are excluded. Acceptance is direct pinned source and existing
fixture/verifier review, conditional content/gap tables, complete prose/source/link/private/diff
checks, clean committed planner and actual final-head CI. A pushed Draft PR goes to independent
main-gate review; this document neither accepts a remake capability nor closes the content epic.

Original-source paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM` at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The reused read-only checkout was clean at that exact pin. Retained `map-init` and `map-script-engine`
payloads bind this revision and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`; their fixture comparisons passed
here, with unchanged source/tool/fixture dependencies. No new extraction, ROM verification or runtime
observation is claimed. Addresses are static source/H1 provenance.

`Fnnn` means flag nnn. `sf2enums.asm` defines battles 6/7 as `BATTLE_TO_GRANSEAL` /
`BATTLE_VERSUS_DARK_SMOKES`; maps 3/19/20/21 as Granseal and its castle 2F/1F/3F, 40/57/60/58 as
Ancient Tower exterior/entrance/stairway/underground room, and 66 as the Granseal kingdom overworld.
Those source floor names are retained rather than inferred from stair comments. Map filenames below
are under `data/maps/entries/mapNN/`; setup filenames are in its `mapsetups/` directory. Battle-code
filenames are under `code/gameflow/battle/`; script handlers are under `code/common/scripting/map/`.

## Battle06 outcome and return consumers

**Confirmed:** `battleloop_1.asm` (`0x23A84`) processes the defeated wrapper and dead list after an
individual turn, then counts combatants; it processes turn-end effects/deaths and counts again.
`battleloop/countremainingcombatants.asm` (`0x23C58`) requires nonnegative X and positive HP, and
forces zero allies when Bowie is dead. Zero allies takes priority over zero enemies.

| Selected consumer | Battle06 result and limit |
| --- | --- |
| `ExecuteBattleCutscene_Defeated`, `cutscenes/battleendcutscenesstart.asm` / `battleendcutscenesend.asm` (`0x47B92..0x47C8E`) | Bowie must be alive and enemy slot 128 dead. With F506 clear, defeated table row 6 selects `ms_Empty`; F506 set skips it. `data/battles/global/enemyleaderpresence.asm` row 6 is zero, so the tail does not queue remaining living enemies merely because slot 128 died. |
| `ApplyPositionsAfterEnemyLeaderDies`, `cutscenes/afterenemyleaderdeathpositions.asm` (`0x47D9E`) | Its current-battle table scan finds only Battle05 in `data/battles/global/afterbattlepositions.asm`. Battle06 reaches the terminator without the position/HP writes. An AI `LEADER` label is not a substitute for this table or the leader-presence boolean. |
| `ExecuteAfterBattleCutscene`, `cutscenes/afterbattlecutscenesstart.asm` / `afterbattlecutscenesend.asm` (`0x47CBC..0x47D6A`) | F506 clear selects row 6 (`0x47D00`) of `data/battles/cutscenes/afterbattlecutscenes.asm`: `ms_Empty`. F506 set skips the script. Both paths execute the shared join-table row 6 (`0x47D70`) from `data/battles/cutscenes/afterbattlejoins.asm`, whose zero calls `JoinForce(0)` (Bowie), not a new story recruit. The similarly named legacy global table is not this consumed table. |

The empty after slot supplies no warp, jewel transaction, Slade recruitment or onward unlock.
There is no Battle06 equivalent of Battle05's scripted knight/Slade aftermath.

| Outcome | Ordered state and resource consequences | Return and preserved boundary |
| --- | --- | --- |
| Victory, F506 clear or already set | `BattleLoop_Victory` in `battleloop_2.asm` (`0x23CBA`) heals living/immortal allies; prepares map-event fields from current map, first active member's final coordinates plus battle-area origin and entity facing, parameter 1 zero; runs the after wrapper/shared join; clears F406 and sets F506. | Empty/skipped after script leaves those fields intact. Ordinary Battle06 gives Map66 with origin `(21,2)` added to that member's battle-relative position, d4=1. There is no fixed town destination. `MainLoop` (`code/gameflow/mainloop.asm`, `0x75C4`, `alt_MainLoopEntry` at `0x75E0`) runs `SwitchMap`, then exploration without `CheckBattle` at this return. Repeat victory does not replay a special scene. |
| Defeat | `UpdateBattleUnlockedFlag` clears F406 only if F506 was already set. Fixed text 363, Bowie HP restored to maximum, gold halved by unsigned shift, `GetEgressPositionForBattle`, d4=-1. No Battle04 capture/upgrade tail. | No new F506 or after/join call. With the predecessor's incoming `EGRESS_MAP=16` and F399 set, the savepoint is Map16 `(27,7)`, Up. Another selected egress map changes this tuple. |
| Egress / Angel Wing | `battlefunctions/battlefunctions_1.asm` charges spell MP / removes the chosen item slot, performs the escape presentation calls, updates the unlock flag and obtains egress coordinates; d4=0. | Same conditional savepoint; no defeat gold penalty or F506 write. F506 clear leaves F406 armed; F506 set clears F406. Escape also bypasses after/join. |

`GetEgressPositionForBattle` (`battleloop/getegresspositionforbattle.asm`, `0x23E50`) has no Battle06
exception. `GetSavepointForMap` (`code/common/maps/egressinit.asm`, `0x75EC`) reads F399 and
`data/maps/global/savepointmapcoords.asm`; F399 clear uses game-start coordinates, and an unlisted
map uses `(1,1)`, Up. BattleLoop itself sets F399. The return paths all subsequently enter
`ExplorationLoop` (`code/gameflow/exploration/explorationfunctions_2.asm`, `0x257C0`), whose heal
restores HP/MP of living allies and the named immortals, skipping other dead allies. The status mask
retains only already-present stun/poison/curse bits; it does not add them or revive every ally.

**Confirmed:** retained `bbcs_06` command 45 is `setStoryFlag 7`; `csc13_setStoryFlag` adds 400.
Thus an executed intro supplies F407 before any outcome. None of the selected outcome paths clears
F407. If F456 was already set or F88 selected suspended entry, the intro was skipped: F407 must
remain an explicit input rather than being assumed from battle number. F506 and F407 serve different
consumers. In particular, escape with F506 clear does not select the post-victory town/castle setups,
even if F407 is set. This is not evidence that the party can physically reach Battle07 after escape.

The predecessor's `battle4DefeatUpgradesBattle=true` limitation is unchanged: it denotes a call to
`UpgradeBattle`, whose random-battle list does not include Battle04/05/06. It does not increment the
current battle, upgrade these enemies or establish an onward unlock.

## Conditional onward map requests

**Confirmed:** the following are selected records in each map's `6-warp-events.asm`. All are
`warpNoScroll`. Each row means **if the player produces that warp request**, not a demonstrated
walk from the preceding destination. Source warp bytes 255 mean a coordinate wildcard here.

| Requesting map / marker | Destination / facing | Relevant consumer |
| --- | --- | --- |
| Map66 `(30,32)` | Map3 `(1,27)`, Right | `SwitchMap` requires F640 clear to retain Map3; otherwise it substitutes the earthquake map. F506 changes Map3 content through setup selection, not this warp record. |
| Map3 `(255,1)` | Map19 `(26,30)`, Up | Enters the castle 2F setup and its conditional events below. |
| Map19 `(6,2)` | Map20 `(6,37)`, Right | Selected west-tower leg; no-scroll exits elsewhere in the castle are not interchangeable. |
| Map20 `(3,36)` | Map21 `(3,16)`, Right | Selected next west-tower leg. |
| Map21 `(9,1)` | Map40 `(4,30)`, Up | Tower exterior with the F506-selected Lemon scene context. |
| Map40 `(255,12)` | Map57 `(8,18)`, Up | Map57 is also Battle01's map: a still-set F401 can select row 1 first, even if F501 is set. An unobstructed no-battle continuation therefore requires F401 clear at this request. |
| Map57 `(255,4)` | Map60 `(53,1)`, Right | Map57's F506 block copy is relevant to the approach; see below. |
| Map60 `(255,51)` | Map58 `(13,36)`, Up | Requires reaching this separate stairway marker. Map60 also contains same-map stair/portal records; their presence alone does not close the path from `(53,1)` to this marker. F999 must be clear to retain Map58 through `SwitchMap`. |

`WaitForEvent` / `ProcessMapEvent` in `explorationfunctions_2.asm` consume the request.
`ProcessMapEventType1_Warp` clears/unwinds the exploration call for parameter 1 zero and supplies
d0–d3 from map/coordinates/facing. Nonzero parameter 1 takes the scrolling path instead.
`MainLoop` then runs `SwitchMap` **before** `CheckBattle`, and only enters the destination's
exploration setup/init if no battle was selected. `code/common/maps/mapinit_0.asm` and
`data/maps/global/flagswitchedmaps.asm` specifically replace Map58 with Map59 when F999 is set.
That is a different admission surface; it cannot be called Battle07 merely because the warp record
originally named Map58.

Map66's init returns directly. Its default zone calls `CheckRandomBattle(3)`; the explicit Y=9/10
and Y=28 zones have later F537/F538/F539-dependent consumers. `CheckRandomBattle` in
`mapsetupsfunctions_1.asm` can set F403 and request map reload depending on F503, `STEP_COUNTER`
and RNG. Consequently the walk from a Battle06 victory position to the Granseal marker is not
proved encounter-free. These consumers are preserved as route conditions, not expanded into later
campaign research.

`map57/3-flag-events.asm` copies a `2 x 1` block from `(17,0)` to `(7,4)` when F506 is set.
The destination overlaps the north warp approach, but the record alone proves neither passability
nor that this is a mandatory gate. Maps19/20/21/40 have empty flag-copy tables; Map19 has two local
step copies, while Maps20/21/40/57/60 have empty step tables. Map60's flag copies depend on F999.
Full working-layout/collision/follower traversal remains **Unknown**, including whether a supplied
pre-victory state can reach the final marker. No layout or portal maze was decoded for this slice.

## Granseal content, recruitment and followers

**Confirmed:** `GetCurrentMapSetup` in `mapsetupsfunctions_1.asm` (`0x4779E`) uses the last set flag
in source order from `data/maps/mapsetups.asm`. For this content segment, select F506 with F543 clear;
Map19 also requires F507 and F982 clear. Earlier F501/F609 do not override F506. Map40's later
F507 explicitly selects its default setup instead of the F506 variant. These predicates matter for
repeat/arbitrary state; victory alone does not erase later flags.

| Selected setup / event | Ordered effect and boundary |
| --- | --- |
| Map3 F506: `pointertable_506.asm`, `s6_initfunction_506.asm` (`0x51360`) | If F6 is set, hides Kiwi; then sad music/fade script. Its zone table is default-only. `3-flag-events.asm` has two F506 copies for the damaged animal-cage area. These and the NPCs express invasion aftermath, without proving the asserted off-screen events. |
| `Map3_1FA_EntityEvent15` (`0x51256`), `s2_entityevents_506.asm` | F6 set returns. With F6 clear, the naming question's nonzero answer returns; zero calls `j_NameAlly(6)`, selected dialogue, then `cs_512BA`. That program's commands 4/5 call `join ALLY_KIWI` and `addNewFollower ALLY_KIWI`. Naming alone is not the join. |
| Kiwi join/follow consumers | `csc08_joinForce` in `mapscriptengine_2.asm` calls `j_JoinForce(6)`, class lookup and fixed join message 446. `code/common/stats/battleparty.asm` sets F6; with fewer than 12 active members, `JoinBattleParty` also sets F38. A full party does not force him active; join does not set HP. `csc56_addFollower` in `mapscriptengine_1.asm` appends the current entity behind the last exploration follower via `AddFollower`; it does not add a persistent Kiwi row to `table_Followers`. Actor placement, membership and current following remain distinct. |
| Map19 F506 zones: `s3_zoneevents_506.asm` | `(255,15)` dispatches `Map19_1FA_ZoneEvent0`; `(25,12)` dispatches `Map19_1FA_ZoneEvent1`. Their implementations are physically in `s3_zoneevents_507.asm` (`0x52DB6` / `0x52DC8`), despite being selected by the F506 table. The filename is not a F507 admission condition. |
| First castle zone, F620 clear | Calls `cs_534D2` in `scripts.asm`: Astral confronts Lemon, Elis cries out, and Astral persuades Lemon to head for the tower. Commands 56–59 hide Astral 128, Lemon and guards 132/133; after the script returns the handler sets F620. F620 set skips it. No `join ALLY_LEMON` occurs. |
| Minister zone, F621 clear | Calls `cs_535FA`: minister 129 reports Elis's abduction, king 130 accompanies Bowie. Commands 5/6 follow king → Bowie and minister → king using `csc2C_followEntity`. On return the handler sets F83, then F621. It does not check F620. F621 set skips the whole script and both writes, so it cannot repair a supplied F621-set/F83-clear state. |
| Map19 F506 init (`s6_initfunction_506.asm`, `0x5311E`) | F620 set moves Lemon, 128, 132, 133 out of the map; F621 set moves 129/130 out. This suppresses the original scene actors on reentry; it does not itself reconstruct arbitrary follower state. |
| F83 across maps | `data/scripting/entity/followers.asm` has two F83 rows, `FOLLOWER_A` king and `FOLLOWER_D` minister. `followersfunctions_1.asm:InitializeFollowerEntities` checks each row flag; `followersfunctions_2.asm:InitializeFollowerActscripts` assigns scripts to admitted rows. Neither is a force join. Multiple/conflicting incoming follower flags and completed movement remain **Unknown**. |
| Selected Map20/21 F506 setup | Map20 points directly into `ms_map20_flag501_InitFunction`, bypassing the old bedroom introduction; Map21 points into `ms_map21_flag609_InitFunction`, bypassing the earlier guard-position script. Both entries only call the shared music/fade script if F507 is set. Their F506 entity-event tables are default-only, so the older Map21 guard conversation/`setStoryFlag 1` is not selected just because its implementation shares the file. |
| Map40 F506 | Places a prone Lemon and two guards; default zone returns, and init only selects the music/fade script if F507 is set. Lemon's `Map40_EntityEvent0` calls `cs_53FE0`: he urges the rescue and describes a black cloud. The script shivers/stops his scene entity; it neither joins him nor writes combat HP/death. No F620 check establishes that his off-screen journey occurred. |

The source actor bindings come from `s1_entities_506.asm` in Maps3/19/40 and the corresponding
entity-event tables. Reusing an `ALLY_*` number for a scene actor does not establish force membership.
The `j_JoinForce` and `j_NameAlly` calls resolve through
`code/common/tech/jumpinterfaces/s02_jumpinterface.asm` and `s03_jumpinterface_1.asm` respectively.
`AddFollower` in `code/common/scripting/entity/entityfunctions_2.asm` installs the follow action and
appends the entity to the exploration list only if absent; it writes no joined/active force flag.
The king/minister relationship can be represented in current entities and later by persistent F83
rows; those are separate consumers. The predecessor's Slade join/follower and prison/drawbridge
walking/reentry Unknowns remain open rather than being re-proved here.

### Selected dialogue

**Confirmed:** operands and the hexadecimal entries in `data/scripting/text/gamescript.txt` agree
on these ordinary unskipped text ranges. Only short content summaries are retained. Debug text
skipping, rendering, speech-SFX timing and truth of NPC assertions are not established.

| Caller / IDs | Brief content and limit |
| --- | --- |
| Map3 F506 entity handlers, 665–673 and 678 (`0x299..0x2A1`, `0x2A6`) | Townspeople describe Galam soldiers entering the castle, violence and broken cages. Several prone actors share the death narration. Dialogue is evidence of what the game says, not independent proof of each off-screen event. |
| Kiwi handler, 674–677 (`0x2A2..0x2A5`), plus fixed 446 | A frightened animal can be named and beckoned; the accepted branch reaches the real join described above. Declining or already joined suppresses it. |
| Map19 F506 entity handlers, 678–689 (`0x2A6..0x2B1`) | Wounded/dead castle occupants describe Galam's magic, Astral's incantation and Elis being taken. They do not perform a battle unlock. |
| `cs_534D2`, 694–712 (`0x2B6..0x2C8`) | Astral challenges Lemon's orders and the king's identity; Elis's cry brings urgency; Astral and the staged group leave. This neither recruits Lemon nor proves arrival at another map. |
| `cs_535FA`, 690–693 (`0x2B2..0x2B5`) | Minister identifies the Ancient Tower; king resolves to rescue his daughter; narration announces king/minister accompaniment. Actual effects are current follows, then F83/F621. |
| `cs_53FE0`, 997–999 (`0x3E5..0x3E7`); guard handler 1000 (`0x3E8`) | Lemon describes the king as a devil and a black cloud attack; guard narration describes death. No force-death or recruitment transaction follows. |
| Battle07 `bbcs_07`, 2357–2384 (`0x935..0x950`) | Galam threatens Elis; Astral and Galam confront and immobilize each other; Elis's escape is blocked; Astral asks Bowie to rescue her and Galam summons attackers. This stages the encounter, not its outcome. |

## Battle07 admission and before/start stopping boundary

**Confirmed:** `code/common/maps/getbattle.asm:CheckBattle` (`0x799C`) scans the coordinate table
in ascending battle order. `data/battles/global/battlemapcoords.asm` row 7 (`0x7A67`) is Map58,
area `(0,0,28,38)`, trigger `(255,255)`. No earlier row uses Map58. With the effective requested map
still 58 and F407 set, it selects battle 7 regardless of the requested X/Y. The area rectangle is
returned battle geometry, not a containment check. F507 set clears F407 **after selecting** row 7
and still returns battle 7 for this call. F506, F620, F621, F83 and Kiwi membership are not checks
in this routine. Their omission from the admission predicate does not establish physical access.

Useful counterexamples are F407 clear with F506 set (no row-7 admission); F999 set (Map58 becomes
Map59 before checking); F401 set at the preceding Map57 request (Battle01 intercepts); and
F507+F407 set (one selection still occurs). These are conditional source states, not claims that
all are naturally attainable in this story segment.

Before-table row 7 (`0x47A96`) in `data/battles/cutscenes/beforebattlecutscenes.asm` selects
`bbcs_07` (`data/battles/entries/battle07/cs_beforebattle.asm`, `0x49CE2`). For a new battle with
F457 clear, command 1 clears F83, command 2 stages Map58 with camera `(8,29)`, and command 3 loads
`ce_49F36`. Camera coordinates are not an exploration-player warp. The scene places Bowie
`(13,35)`, Galam 128, Astral 129, Elis 130, king 131, minister 132 and three staged enemy actors
133–135; the latter begin off-map and are positioned during the program. These actors are not a
combat roster, and the scene does not require a preceding successful F83 follower observation.
It contains no new force join or onward `setStoryFlag`.

`BattleLoop` restores the supplied map/battle after the before-script, clears region flags 90–105,
heals living/immortal allies, initializes ally/enemy battle positions, clears AI memory, loads battle,
then calls the start wrapper. Row 7 (`0x47B3A`) of `data/battles/cutscenes/battlestartcutscenes.asm`
is `ms_Empty`; `ExecuteBattleStartCutscene` sets F457 before dispatch when clear. F457 already set
skips both intro scripts, including the F83 clear. F88 set instead restores the saved seconds,
clears F88 and AI memory, and reloads battle state without new-battle initialization or before/start.

`data/battles/spritesets/spriteset07.asm` (`0x1B3720`) contains 7 ally placements, 10 enemies,
4 AI regions and 1 AI point. Placement initialization consumes the actual joined/active/living
force; seven slots do not force Kiwi into the party or prove seven living allies. Natural admission,
service completion, final actor state, presentation and winnability remain **Unknown**. Stop here:
Battle07 resolution and the later earthquake/campaign are not researched by this document.

## Existing coverage and gap disposition

Fixture paths in this table are under `tests/fixtures/h2/`; commands have prefix `uv run sf2`.
Coverage is the existing admitted boundary, not a claim that an entire rail was freshly executed.

| Claim / owner | Disposition and narrow surface | Missing relation / discriminating counterexample |
| --- | --- | --- |
| Battle06 outcomes, healing, empty scripts and joins | **Covered** by `battle-control-static-v1.json`, `battle-loop-static-v1.json`, `battle-cutscenes-static-v1.json`, `battle-global-data-static-v1.json`; [battle loop](./battle-loop.md), [global data](./battle-global-data.md). Commands: `h2 battle-control`, `battle-loop`, `battle-cutscenes`, `battle-global-data`. | Generic wrapper guards were compared directly to the complete `cutsceneFacts` fixture. Selected row 6 and the absence of a Battle06 position override were read directly. Controls: slot-128 death does not clear the rest; F506-set after skip still joins Bowie; dead non-immortal allies stay dead. |
| F407 producer, selected scripts and join/follow consumers | **Covered** at command/handler/source scope by `map-script-engine-static-v1.json` and common-stats; [common scripting](./common-scripting.md). Command: `h2 map-script-engine`. | Retained payload comparison passed. Source-site anchors: `bbcs_06:45`, `cs_512BA:4/5`, `cs_534D2:56..59`, `cs_535FA:5/6`, `bbcs_07:1..3`. Full-party Kiwi, no naming consent, F456/F457/F88 skips and F621-set/F83-clear discriminate the claims. Current follows do not establish persistence or a join. |
| Setup/init/entity/zone and text callers | **Covered** at table/stream/target scope by `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-init-static-v1.json`, `map-events/entity-programs.json` and `zone-programs.json`; [map inventory](./map-data-inventory.md). Commands: `h2 map-setup`, `map-entities`, `map-init`, `map-events`. | Retained init comparison passed. Zone fixture programs 31/32 retain the F620/F621 handlers and their exact order; entity program 467 retains Kiwi's consent/flag branch. Source-file suffix 507 does not select the F506 handlers; last-set setup priority and default-only Map21 entity table are counterexamples to filename-based reasoning. Natural invocation remains unproved. |
| F407 → selected map requests → Map58 admission | **Reuse/extend** the predecessor's conditional-admission candidate through map-script-engine, `map-event-request-consumption-static-v1.json`, `common-maps-static-v1.json` and [map content](./map-content.md). Narrow commands: `h2 map-script-engine`, `map-event-request-consumption`, `common-maps`, `map-content`. | Missing executable composition: intro write, outcome-preserved flags, selected no-scroll request, SwitchMap and row-7 selection. Existing rails guard separate consumers but not this composed predicate. Minimum public-safe output is source/caller identity, input flags, request tuple, effective map and selected row. Controls: F407 clear; F999 set; F507+F407 still selects; nonzero parameter 1 does not take this return. No standalone per-battle fixture or new command is justified. |
| F506 layout changes and walking through castle/tower/stairway | **Reuse/extend map-content** only if a later accepted static reachability claim needs it; otherwise **deferred runtime Unknown**. Command: `h2 map-content`. | Exact missing relation is legal traversal from the variable Map66 victory position through the named markers, especially Map57 `(8,18)` → north marker with F506 clear/set and Map60 `(53,1)` → Y=51 via the applicable local portals. Existing copy/warp records do not give working-layout collision or actor occupancy. Minimum output would be conditional start/target reachability with selected state, not a published layout. Counterexamples: F401 interception, F506 copy absent, F999 alternate state or blocked follower/collision input. No broad decoding or emulator task is authorized. |
| Battle07 before/start/spriteset | **Covered** by map-script-engine, `battle-cutscene-data-static-v1.json`, `battle-routing-data-static-v1.json`, `battle-spriteset-data-static-v1.json`; [cutscene data](./battle-cutscene-data.md), [routing data](./battle-routing-data.md), [spriteset data](./battle-spriteset-data.md). Commands: `h2 battle-cutscene-data`, `battle-routing-data`, `battle-spriteset-data`. | Routing has `routeTargetsParsed=false`; exact slot targets here come from direct source, not that summary. Seven placements versus optional Kiwi, F457 skip versus F83 clear and F88 resume discriminate the stopping boundary. **Deferred runtime Unknown:** natural entry, service/presentation completion and winnability. |

No new bounded H2 implementation is required. The useful handoff is the conditional request and
admission chain plus explicit F506-dependent content; natural path closure remains a separate claim.
Preserve predecessor walking/reentry, caller actor/follower, presentation and upgrade-summary
limitations. An Unknown is not an automatic runtime work queue.

## Reproduction and review boundary

This used bounded source/fixture/verifier reads and small in-memory selections. The two reused JSON
payloads total 18,285,979 bytes (about 18.3 MB); they were not rewritten. There was no full transcript,
layout or roster publication, map decoding, extraction, capture, installation or new environment.
The predecessor owns the retained ROM and extraction receipts; those commands were not replayed.

Load `local/private-inputs.ps1` in each tool's PowerShell process. `$upstream` below denotes the
verified read-only pinned checkout. From the owning checkout, reproduce the comparisons with:

```powershell
. ./local/private-inputs.ps1
@'
import json
import sys
from pathlib import Path
from sf2tool.h2.battle_cutscenes import _cutscene_facts
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
assert _cutscene_facts(Path(sys.argv[1]) / "disasm") == read(
    "tests/fixtures/h2/battle-cutscenes-static-v1.json")["expected"]["cutsceneFacts"]
print("retained payload and cutscene source comparisons: PASS")
'@ | uv run python -X utf8 - $upstream
```

For direct source review, use the named source paths, labels, table rows and command indices above;
select only the named programs from retained `programCorpus.programs` and map-event fixture target
lists. Dialogue IDs map to four-digit hexadecimal keys followed by `=` in `gamescript.txt`.
Record direct content/reference/link/private-boundary checks, `git diff --check` and clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`, actual CI and any completed failures on the
Issue/PR handoff. Normal/full/engine/H3/H4/Godot suites are not selected under this documentation-only
agreement. Preserve ignored evidence, freeze the clean pushed Draft PR and leave acceptance/merge
to main-gate.
