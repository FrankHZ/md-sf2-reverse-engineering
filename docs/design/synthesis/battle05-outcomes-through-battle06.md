# Battle 05 Outcomes through Battle 06 Admission

- **Confirmed original structure:** Battle05's selected position/HP and outcome consumers,
  first-completion jewel/recruitment/return program, conditional Galam exits and Battle06 before/start.
- **Inferred original connection:** these scenes describe an escape from Galam toward Granseal.
- **Unknown original behavior:** natural outcomes, legal walking, actual caller/actor/party state,
  arbitrary reentry, service completion, presentation and persistence.
- Scope: English Layer B content guide for [#711](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/711),
  under [#514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514); no remake capability is admitted.
- Evidence baseline: accepted [Battle05-to-Battle06 research](../../research/battle05-to-battle06-content-route.md),
  evidence date 2026-10-12, pinned `ShiningForceCentral/SF2DISASM` revision
  `c834c652b6862bc5679fd7f69a38a7093206efc6` and the USA retail ROM identity in that owner.

Continue from [Battle04 outcomes through Battle05 admission](battle04-outcomes-through-battle05.md).
That guide explains Galam captivity, Slade following and the guards' F405 unlock/request. Here,
Battle05's first-completion victory supplies the explicit Slade join and F406; a later selected
exit request can admit Battle06 on Map66. Defeat, escape and already-completed alternatives differ.
The numerical sequence alone does not establish a continuous or unavoidable route.

`Fnnn` denotes a flag. Source Battle05 is `BATTLE_GALAM_CASTLE`; Battle06 is `BATTLE_TO_GRANSEAL`.
Map16 is Galam Castle, Map17 its interiors, Map18 the drawbridge and Map66 the Granseal Kingdom
overworld (`MAP_OVERWORLD_GRANSEAL_KINGDOM`). Slade's ally selector is 5; joined F5, active F37,
follower F73, unlock F405/F406 and completed F505/F506 are separate state domains.
All source paths/symbols below refer to the research owner's pinned `disasm/` baseline.

## 1. The Knight's Defeat Has a Separate Position and HP Consumer

**Confirmed static:** the ordinary spriteset's first enemy is a Galam Knight. Its `LEADER` AI label
is distinct from the enemy-leader-presence boolean: Battle05's row in
`data/battles/global/enemyleaderpresence.asm` is zero. That zero suppresses the defeated wrapper's
shared living-enemy dead-list append; it does not disable the earlier Battle05 position helper.

The [cutscene contract](../contracts/battle-cutscene-routing.md#leader-death-position-preparation)
and [selected consumer analysis](../../research/battle05-to-battle06-content-route.md#battle05s-defeated-consumer-and-outcomes)
keep the following steps separate:

| Consumer and condition | Effect and limit |
| --- | --- |
| `executeindividualturn.asm`, after `j_EndBattlescene` | Calls `j_ApplyPositionsAfterEnemyLeaderDies`, then `LoadBattle`, before the top-level defeated-script check. This is a battlescene-return caller; an HP-zero event at the later turn-end check does not establish this earlier call. |
| `ApplyPositionsAfterEnemyLeaderDies` (`0x47D9E`), Bowie HP nonzero and slot 128 HP zero | Scans the battle-position table, whose sole battle is 5. No F505 test occurs. Allies 0–29 and enemies 128–157 receive X=-1; enemies 128–157 also receive HP=0. Slots 158/159 receive X=0 only, with no HP write. Allies are not killed by the X writes. |
| Selected `abp_Battle5` records | Restore battle-relative Bowie `(16,4)`, Slade `(17,4)` and slot 128 `(17,2)`. Allies require nonzero HP; slot 128 is allowed at zero HP. The fourth byte is skipped by the four-byte stride. Area origin `(0,14)` gives map Y 18/18/16. Slade's positioning does not join him or prove participation in Battle05. |
| `ExecuteBattleCutscene_Defeated` (`0x47B92`) | With Bowie alive, slot 128 dead and F505 clear, selects row 5's `edcs_battle5` (`0x49AB2`). F505 set skips this script. Its zero leader-presence row adds no other living enemies to the dead list; earlier HP writes belong to the separate helper. |

**Confirmed authored dialogue:** ordinary cursor IDs 2323–2331 bind the knight, Slade and narration.
The knight recalls Slade's imprisonment; Slade finds the Jewel of Light and learns that King Galam
took the other jewel. This defeated script contains no join, inventory award
or onward unlock. These brief summaries consume the accepted operand/text review, not rendered speech.

`CountRemainingCombatants` (`0x23C58`) counts nonnegative X and positive HP, and forces zero allies
when Bowie is dead. Zero allies takes priority over zero enemies. The position helper explains a
source path toward zero remaining enemies under its caller/input conditions; it does not prove
natural victory, winnability or a universal leader-death rule. The
[battle-control contract](../contracts/battle-control-lifecycle.md) owns the faction-check lifecycle.

## 2. Victory, Defeat and Escape Return Differently

**Confirmed source branches:** outcomes consume the supplied state as follows. The
[outcome owner](../../research/battle05-to-battle06-content-route.md#battle05s-defeated-consumer-and-outcomes)
retains the exact caller and savepoint conditions.

| Outcome | Script/state work | Return and boundary |
| --- | --- | --- |
| Victory, F505 clear | Heal living/immortal allies; prepare current-map/final-position return fields; run `abcs_battle05`; reach shared `JoinForce(0)` tail; clear F405 and set F505. The after-program replaces return fields. | Map16 `(16,18)`, Down, d4=1. Jewel/recruitment/F406 effects are described below. |
| Victory, F505 already set | Skip the whole after-program; still call shared `JoinForce(0)`; clear F405 and set F505. | Prepared current-map/first-battle-party-member final position plus area origin/facing survives. No new F384, Slade join, F73 clear, F406 or fixed castle warp. This is a source alternative, not proven natural repetition. |
| Defeat | Update unlock; fixed ID 363; restore Bowie's maximum HP; halve gold by unsigned shift; obtain egress position, d4=-1. | With predecessor-selected `EGRESS_MAP=16` and F399 set: Map16 `(27,7)`, Up. No after/join program or F505 write; Battle04's special capture/upgrade tail does not run. Later ordinary living/immortal healing does not revive every fallen ally. |
| Egress / Angel Wing | Respectively spend spell MP or remove the chosen item slot, then text/flash cleanup, unlock update and egress lookup, d4=0. | Same conditional castle savepoint; no defeat gold penalty, after-program, Slade join or F406 producer. |

The unlock helper clears F405 only if F505 was already set; otherwise defeat/escape leaves it armed.
BattleLoop sets F399, and Battle05 has no egress exception. A different incoming `EGRESS_MAP` changes
the destination; an F399-clear helper call uses the game-start tuple, while an unlisted map defaults
to `(1,1)`, Up. The castle savepoint is therefore conditional.

The predecessor's older `battle4DefeatUpgradesBattle=true` fixture label records an `UpgradeBattle`
call. Its selected list excludes Battle04/05/06: it proves no enemy upgrade, battle-ID increment or
next unlock. The explicit F406 producer here is the first-completion after-program.

### The victory warp is consumed as a return

**Confirmed order:** `abcs_battle05`'s terminal warp writes event type 1 and parameters 1–5, replacing
the prepared return fields. Victory reads those fields. `MainLoop`'s `alt_MainLoopEntry` (`0x75E0`)
then calls `SwitchMap` and exploration without a fresh `CheckBattle`; exploration clears event type
at entry. This warp supplies the victory return, rather than a second pending exit dispatch.
Later castle/drawbridge exits use their own requests and the encounter check described below. The
[request-consumption owner](../../research/map-event-request-consumption.md#confirmed-consumer-topology)
closes local poll/clear seams; the accepted segment research supplies this conditional composition.

## 3. The Jewel Scene Formally Recruits Slade

**Confirmed authored content:** `abcs_battle05` (`0x49B48`,
`data/battles/entries/battle05/cs_afterbattle.asm`) uses ordinary IDs 2332–2341. Slade offers the
jewel; narration describes its fusion to Bowie's neck. He suggests a later church visit and
accompanying Bowie toward Granseal to find the other jewel. The program calls no mandatory church
event. Its state effects must be read separately from those lines:

| Retained command index | Actual consumer and consequence |
| --- | --- |
| 11: `setF 384` | Writes F384 during jewel dialogue. Neither this nor the defeated program awards an inventory slot or removes a jewel item; narration is not an item transaction. |
| 18: `join ALLY_SLADE` | `csc08_joinForce` selects ordinary join music, calls `j_JoinForce(5)`, obtains class, shows fixed ID 446 with name/class substitutions and waits/closes text. The fixed join message does not advance the script cursor. |
| `JoinForce` helper | Sets joined F5, calls `UpdateForce`, then checks the rebuilt active count against 12. Below capacity, `JoinBattleParty` sets F37; a full party does not force him active. Join does not restore HP. |
| 21/22: `clearF 73`; `setStoryFlag 6` | Clears the persistent follower selector, then adds 400 to the story operand to set F406. It does not change `CURRENT_BATTLE` or synchronously start Battle06. |
| 23/24: `followEntity ALLY_SLADE,ALLY_BOWIE,2`; `warp ...` | Uses the character-alive gate and `AddFollower` for the current entities, then supplies the fixed castle return. This current following relationship is separate from F73 and force membership. |

The [party-membership contract](../contracts/party-membership-state.md#joinforce-chronology)
requires the rebuild-before-activation distinction. `JoinForce` contains no second `UpdateForce`:
newly set F37 does not establish an already synchronized active prefix/count on helper return.
The numeric selected-source flags/capacity above come from the segment research; the general
contract deliberately retains symbolic ranges/capacity. Active-list synchronization and the actual
next battle's force remain **Unknown**.

The wrapper's consumed join-table row 5 (`0x47D6F`, under `data/battles/cutscenes/`) contains zero.
It reaches ordinary `JoinForce(0)` even when F505 skips the script; zero is not a no-call sentinel,
and this is not Slade recruitment. The unused namesake under `global/` is a separate table.
The [party/roster contract](../contracts/party-roster-state.md#handler-boundary) owns script-handler
chronology, rather than the scene's narrative role.

Slade's earlier setup presence, following and F73 did not establish F5. The explicit after-program
join does not retroactively prove he fought in Battle05. Rebuilding exploration entities uses setup
and follower flags, so clearing F73 then adding a current follower does not prove he remains
visibly behind Bowie after the return.

## 4. Leaving Galam Is Conditional on Setup, Layout and Exit Selection

**Confirmed selectors:** no flag-switched-map row remaps Map16/18/66. Setup selection uses the last
matching flag: Map16 default → F663 → F530; Map18 default → F530; Map66 has one setup. The early
content below assumes F530 clear. Battle05 victory does not set F663; the predecessor's army-departure
zone supplies it when invoked. Later setups are alternative endpoints, not this asserted route.
The [map contract](../contracts/map-exploration.md#load-and-selection-order) owns layout/load order.

| Location or request | Conditional handoff and limit |
| --- | --- |
| Castle return on Map16 | Default/F663 init hides guard 145 when F664 is set. The X=3 guard zone returns on F505 before writing `EGRESS_MAP` or reissuing the guard program. X=17's F663 test remains independent. Completion does not establish that every castle route is open. |
| Map16 south record `(255,31)` | No-scroll request → Map18 `(9,10)`, Down. The separate north record `(255,0)` instead supplies Map66 `(32,4)`, Down. Neither record proves that its marker is physically reachable. |
| Map18 flag copies | F611 gates two copies: `(19,0)`, size `(18,30)` → `(0,0)` and `(38,30)`, size `(18,18)` → `(38,0)`. Source labels describe drawbridge-down/new-roof work. Map17 init writes F611 unconditionally after its F660/F663 branches, despite the data comment suggesting an F663 dependency. F611-clear layout is a distinct alternative; copies alone do not establish traversability. |
| Default Map18 content | Init and default zone return directly; step events are empty. Four NPCs provide optional dialogue. No mandatory script here produces F406. |
| Map18 south record `(255,29)` | No-scroll request → Map66 `(31,9)`, Down. North `(255,9)` returns Map16 `(26,30)`, Up; tower-stair records remain in Map18. Walking between the two south markers remains Unknown. |

**Confirmed optional dialogue:** the trader (128, fixed IDs 870–872) admires the jewel and fails to
remove it; handler admission does not test F384 and performs no item transfer. A man (129, 873–874)
worries about his mother in Granseal and the war. Guards (130, 875–876; 131, 877–878) discuss the
order to stop departures, the rusty bridge and restricted tower. Their dialogue does not establish
collision, bridge mutation or mandatory interaction. The
[selected actor/text owner](../../research/battle05-to-battle06-content-route.md#dialogue-and-actors)
keeps these bindings distinct from actual player reach or rendered speech.

Optional Map17 reentry also changes: with F505 set and F530 clear, `ms_map17_flag505` retains the
init/entity-event pointers but its zone table has only the default return target. The earlier
prison/shortcut/tunnel zones do not replay just because the map is revisited. This does not resolve
arbitrary door/shortcut reconstruction, incoming followers or working-layout persistence.

## 5. The Exit Request Can Select Battle06, with Earlier Rows Taking Priority

**Confirmed request order:** a selected no-scroll exit reaches `WaitForEvent` → `ProcessMapEvent`,
which clears the pending type. Type-1 parameter 1 zero unwinds exploration to MainLoop with
map/X/Y/facing; nonzero parameter 1 stays in the scrolling exploration branch. `SwitchMap` leaves
Map66 unchanged, then `CheckBattle` runs before Map66 exploration setup/init/zones. No new Map66
zone event is needed to arm F406.

`CheckBattle` (`0x799C`) scans encounter rows ascending. On Map66, rows 2/3/4 precede row 6 and all
have wildcard triggers. **Confirmed row-6 conditions:** F402/F403/F404 clear and F406 set. Row 6
supplies area `(21,2,20,32)` and trigger `(255,255)`; that rectangle is output data, not a containment
test on the exit tuple. Both selected Map66 arrival tuples can match it, without proving either walk.

Completion F506 does not reject the row: selection clears F406 but still returns battle 6 on that
call. Likewise, F403 set alongside F406 with F402 clear selects row 3 even if F503 is already set;
its completion clears its unlock after selecting. With F505 set/F406 clear, repeat Battle05 victory
skips the after-program producer, and merely arriving on Map66 does not arm Battle06. These controls
prevent converting source numbering into automatic chronology. The
[admission owner](../../research/battle05-to-battle06-content-route.md#direct-exit-content-and-conditional-battle06-selection)
retains the selected-source relation; separate existing rails do not executable-close its composition.

## 6. Battle06's Before Scene Writes F407 at Entry

**Confirmed authored scene:** row-6 before target `bbcs_06` (`0x49BCA`) uses ordinary IDs 2342–2356.
Guards 129/130 stop the group; Slade claims to carry something left by King Galam. Officer 128 offers
to deliver it, recognizes the escapees and calls them Granseal spies. The excuse leads into battle
staging. Bowie is staged but has no text operand in this scene.

`loadMapFadeIn Map66,25,5` supplies map/camera staging, not a warp placing Bowie at `(25,5)`.
`ce_49CBA` stages Bowie `(30,9)`, Slade `(29,8)` and three guard actors. These are scene entities,
rather than the combat roster. The final `setStoryFlag 7` (retained command 45) sets **F407** during
before. It is neither Battle06 completion nor proof of Battle07 admission.

**Confirmed normal-entry order:** with F88 clear, BattleLoop installs map/battle for before, restores
its supplied map/battle afterward, clears region flags 90–105, heals living/immortal allies,
initializes allies/enemies, clears AI memory, loads battle and then calls start. Before checks F456
without setting it; start sets F456 before dispatch, whose row-6 target is `ms_Empty`. F456 already
set skips both scripts, so this F407 write is not repeated. F88 set takes suspended entry: clear F88,
restore saved seconds, clear AI memory and reload battle state, bypassing these new-entry/intro steps.
Empty termination still owns interpreter wait/scroll cleanup; it does not prove instant completion.

The [intro contract](../contracts/battle-cutscene-routing.md#shared-intro-gate) and
[selected entry owner](../../research/battle05-to-battle06-content-route.md#battle06-beforestart-stopping-boundary)
own those gates/order. `BattleSpriteset06` (`0x1B365C`) declares 6 ally placements, 9 enemies,
1 AI region and no AI points. Ally initialization consumes the actual joined/active party and skips
dead members. Six placements and Slade's before-scene actor do not prove a six-person force or his
incoming HP/active membership. The [encounter contract](../contracts/battle-encounter-definition.md)
retains that data-versus-party distinction. Stop here before Battle06 resolution.

## Evidence Review and Remaining Limits

This task reviewed accepted research, the predecessor and relevant contracts against selected public
payloads and verifier comparisons. It performed no fresh extraction, source acquisition, runtime
observation or H2 rerun. Selected original-source composition, actor/text meanings and row operands
are attributed to the accepted segment research, whose pinned source paths/addresses and reproduction
receipts remain the provenance owner. Normal dialogue summaries assume the unskipped branch;
debug/P2-Start can suppress ordinary text/cursor increments, while fixed `txt`/join messages are separate.

| Claim | Reviewed executable surface and reproduction command | Disposition |
| --- | --- | --- |
| Position/HP, faction checks and outcomes | [cutscene fixture](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), ID `sf2-battle-cutscenes-static-v1`, `cutsceneFacts.leaderDeathPositions`, `enemyDefeated`, `afterBattle`, `intro`; [loop](../../../tests/fixtures/h2/battle-loop-static-v1.json), ID `sf2-battle-loop-static-v1`, `lifecycleFacts.remainingCombatants`; [control](../../../tests/fixtures/h2/battle-control-static-v1.json), ID `sf2-battle-control-static-v1`, `controlFacts.outcomes`; [cutscene](../../../src/sf2tool/h2/battle_cutscenes.py), [loop](../../../src/sf2tool/h2/battle_loop.py), [control](../../../src/sf2tool/h2/battle_control.py) verifiers. `uv run sf2 h2 battle-cutscenes`, `battle-loop`, `battle-control`. | HP-zero range and X-only tail are fixture-bound, not an all-enemy kill. Battle05 table/caller and return-field composition use accepted selected-source review. The old upgrade boolean establishes a call only. |
| Jewel/join/follow/unlock/return and F407 | [script fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json), ID `sf2-map-script-engine-static-v1`, `forceStateCommandFacts`, `storyStateCommandFacts`, `transitionCommandFacts` source sites `abcs_battle05:11/18/21/22/24` and `bbcs_06:1/45`; [verifier](../../../src/sf2tool/h2/map_script_engine.py). [stats fixture](../../../tests/fixtures/h2/common-stats-static-v1.json), ID `sf2-common-stats-static-v1`, `statsFacts.party`; [verifier](../../../src/sf2tool/h2/stats.py). `uv run sf2 h2 map-script-engine`, `common-stats`. | Exact sites/guards keep join, flag arithmetic and warp separate. Command 23's current following uses accepted consumer review. Four stats facts do not prove post-activation list synchronization or this natural scene. |
| Castle/drawbridge callers and F611 | [init fixture](../../../tests/fixtures/h2/map-init-static-v1.json), ID `sf2-map-init-static-v1`, Map17 `routeJoins` orders 35–37 and `primarySourceBodies`; [init verifier](../../../src/sf2tool/h2/map_init.py). [entity](../../../tests/fixtures/h2/map-events/entity-programs.json) and [zone](../../../tests/fixtures/h2/map-events/zone-programs.json) shards of `sf2-map-events-static-v1`, Map18 entity events 0–3/Map16 zone events 0–1; [event verifier](../../../src/sf2tool/h2/map_events.py). `uv run sf2 h2 map-init`, `map-events`. | Complete semantic comparisons retain branches and operation order; F611 is outside F663's branch. Entity target structure does not prove reachability; scene summaries retain research text attribution. |
| Exit data, request consumption and encounter selection | [content fixture](../../../tests/fixtures/h2/map-content-static-v1.json), ID `sf2-map-content-static-v1`; [request fixture](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), ID `sf2-map-event-request-consumption-static-v1`; [common-map fixture](../../../tests/fixtures/h2/common-maps-static-v1.json), ID `sf2-common-maps-static-v1`; [content](../../../src/sf2tool/h2/map_content.py), [request](../../../src/sf2tool/h2/map_event_request_consumption.py), [map](../../../src/sf2tool/h2/maps.py) verifiers. `uv run sf2 h2 map-content`, `map-event-request-consumption`, `common-maps`. | Inventory/local consumers do not executable-compose after → victory return → chosen exit → row 6, or establish walking. Accepted conditional source review supplies this relation; earlier-row/completion/parameter controls remain explicit. |
| Intro and encounter data | [routing fixture](../../../tests/fixtures/h2/battle-routing-data-static-v1.json), ID `sf2-battle-routing-data-static-v1`, `facts.routeTargetsParsed=false`; [spriteset fixture](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json), ID `sf2-battle-spriteset-data-static-v1`, `facts.rowContentTracked=false`; [routing](../../../src/sf2tool/h2/battle_routing_data.py), [spriteset](../../../src/sf2tool/h2/battle_spriteset_data.py) verifiers. `uv run sf2 h2 battle-routing-data`, `battle-spriteset-data`. | Selected targets/header/staging remain attributed to research source review; inventories prove neither target semantics nor actual party. F456/F88 skips preserve the entry-side F407 boundary. |

The controller contract's New Battle overview groups before/start too broadly to witness their
serial placement; use the accepted selected order above and precise wrapper contract. The older
upgrade-name limitation and Map18 data-comment/F611 consumer discrepancy likewise remain visible
at their stricter boundaries. Symbolic membership contracts deliberately omit selected numeric
values and do not prove synchronized active lists. None requires silently changing Layer A.
The request owner's historical H3 queue and research's suggested existing-owner extensions are
Unknown dispositions, not an acquisition or new-fixture assignment.

**Inferred:** the intended Galam-to-Granseal escape connection. **Unknown:** natural outcome and
actual return/caller/actor/HP/party state; marker/collision/occupancy-aware walking and F611-dependent
paths; follower/reentry reconstruction; completed services, rendered dialogue/audio/input timing;
and durable state. The [research gap owner](../../research/battle05-to-battle06-content-route.md#existing-coverage-and-gap-disposition)
retains discriminating controls and missing relations. Named H2 commands above are reproduction
routes, not fresh PASS claims; load `local/private-inputs.ps1` in the same process before running one.
The [roadmap](../documentation-roadmap.md#current-baseline-and-near-term-synthesis) owns the current
agreement and [Story Progression](story-progression.md) the wider state framework. Battle06 outcomes
and later campaign content remain outside this guide; no unmerged downstream conclusion is consumed.

Continue with [Battle06 outcomes through Battle07 admission](battle06-outcomes-through-battle07.md)
for the separate outcomes, Granseal content, optional Kiwi join, royal following and conditional
tower request/entry. Its static account retains natural walking, reentry and party Unknowns.
