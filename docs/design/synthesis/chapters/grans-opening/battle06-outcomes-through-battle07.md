# Battle06 Outcomes, Granseal Content and Battle07 Admission

Battle06 victory prepares an overworld return; the selected town and castle content describes
Granseal after Galam's invasion. Kiwi can be named and recruited, castle events depict Astral and
Lemon leaving for the tower and add the king and minister as followers, and tower dialogue leads into
the confrontation with Galam. **Confirmed:** these are conditional source programs and state
consumers. **Inferred:** they form the intended return-to-Granseal and rescue connection.
**Unknown:** a natural continuous journey through them. In particular, Battle06's intro writes
F407, while victory writes F506: encounter admission and the changed local content are separate.

This English guide continues [Battle05 outcomes through Battle06 admission](battle05-outcomes-through-battle06.md).
It consumes the [accepted segment research](../../../../research/chapters/grans-opening/battle06-to-battle07-content-route.md)
and stops at Battle07 before/start, before its resolution. The
[roadmap](../../../documentation-roadmap.md#current-baseline-and-near-term-synthesis) owns the Standard-rigor
agreement for [#715](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/715).
No playable remake capability or new observation is accepted here.

Continue with [Battle07 outcomes and the exploration handoff](battle07-outcomes-and-aftermath.md)
for victory/repeat/defeat/escape, the long tower scene and conditional restored exploration.

Original-source paths, labels, addresses, dialogue bindings and selected numeric values below belong
to that research owner, pinned to `ShiningForceCentral/SF2DISASM`
`c834c652b6862bc5679fd7f69a38a7093206efc6` and the USA retail ROM identity. `Fnnn` means flag nnn;
source paths are relative to `disasm/`. Map3 is Granseal, Maps19/20/21 are castle 2F/1F/3F, Map66 is
the kingdom overworld, and Maps40/57/60/58 are tower exterior/entrance/stairway/underground room.
These are source identities, not inferred floor names from stair comments.

## 1. Battle06 Has No Scripted Victory Destination

**Confirmed source conditions:** the defeated wrapper requires Bowie alive and enemy slot 128 dead.
For Battle06, its uncompleted F506 branch selects `ms_Empty`; the enemy-leader-presence row is zero,
so the shared tail does not queue the other living enemies. The separate leader-death position helper
finds only Battle05 in its table and does no Battle06 position/HP work. Killing slot 128 is therefore
not a proven all-enemy kill or natural victory. Remaining-combatant checks require nonnegative X and
positive HP, force zero allies when Bowie is dead, and prioritize zero allies over zero enemies.
The [battle-control contract](../../../contracts/battle/battle-control-lifecycle.md) owns that lifecycle.

**Confirmed outcome branches:** unlike Battle05's jewel scene, Battle06's after target is empty.
F506 already set skips that script, but either branch reaches the consumed cutscene join-table row 6:
zero calls `JoinForce(0)` for Bowie. It is not a no-call sentinel or a new story recruit. The
[cutscene contract](../../../contracts/battle/battle-cutscene-routing.md#after-battle-route-and-join-tail)
preserves the shared-tail boundary; the namesake legacy global table is not the consumed table.

| Outcome | State/resource work | Return and limit |
| --- | --- | --- |
| Victory, F506 clear or set | Heal living/immortal allies; prepare return fields; execute empty/skipped after and shared Bowie join; clear F406, set F506; d4=1. | Current map and first battle-party member's final battle-relative position plus area origin, with entity facing. Ordinary Battle06 uses Map66 and origin `(21,2)`. Empty after leaves these fields intact: no fixed Granseal warp, jewel transaction, Slade join or onward unlock. |
| Defeat | Update unlock; fixed text 363; restore Bowie to maximum HP; halve gold by unsigned shift; obtain egress position; d4=-1. | No after/join or new F506. With predecessor-selected `EGRESS_MAP=16` and F399 set: Map16 `(27,7)`, Up. Other incoming savepoint state changes the destination. No Battle04 capture/upgrade tail runs. |
| Egress / Angel Wing | Spend spell MP / remove the selected item slot, perform escape presentation calls, update unlock and obtain egress position; d4=0. | Same conditional savepoint; no defeat gold penalty, after/join or F506 write. |

`UpdateBattleUnlockedFlag` clears F406 only when F506 was already set; with F506 clear,
defeat/escape leaves it armed. BattleLoop sets F399 and Battle06 has no special egress exception.
An F399-clear helper call uses game-start coordinates; an unlisted egress map defaults to `(1,1)`, Up.
These source alternatives do not establish a natural repeat or escape route.

Victory's prepared map-event fields are consumed as the battle return. `MainLoop`'s
`alt_MainLoopEntry` (`0x75E0`) runs `SwitchMap` and then exploration without a fresh `CheckBattle`.
Exploration clears the event type at entry and heals eligible living/immortal allies; it does not
revive every dead ally. The retained stun/poison/curse mask preserves existing bits rather than
adding conditions. Later exits produce their own requests and encounter checks.

### F407 comes from entry; F506 comes from victory

**Confirmed:** `bbcs_06:45` is `setStoryFlag 7`, whose handler adds 400 and sets F407. None of the
selected Battle06 outcome paths clears it. F456 already set skips before/start; F88 selects suspended
entry and also bypasses the intro. Thus F407 is an input when entry was skipped, not an assumption
from the battle number. The [preceding guide](battle05-outcomes-through-battle06.md#6-battle06s-before-scene-writes-f407-at-entry)
owns that entry boundary.

| Supplied state | Conditional consequence |
| --- | --- |
| F407 set, F506 clear, including after an executed intro followed by escape | The selected post-victory town/castle setups are absent. Row-7 admission has an unlock input, but physical access remains Unknown. |
| F506 set, F407 clear | Changed local content can be selected, but Map58 does not select Battle07. |
| F506 and F407 set | Changed content and the row-7 unlock can coexist, subject to map/setup/request conditions below. Neither establishes a legal walk. |

The older `battle4DefeatUpgradesBattle=true` fixture phrase records an `UpgradeBattle` call. Its
random-battle list excludes Battle04/05/06, so it proves no upgrade, battle-ID increment or onward
unlock for them. That narrower accepted boundary remains intact.

## 2. Granseal's Changed Content Includes Optional Kiwi Recruitment

**Confirmed setup selection:** `GetCurrentMapSetup` uses the last matching set flag in source order.
The content here selects F506 with F543 clear; Map19 additionally requires F507 and F982 clear.
Earlier F501/F609 do not override F506. Victory does not erase later flags, so arbitrary/repeat
inputs can select another setup. The [map contract](../../../contracts/world/map-exploration.md#load-and-selection-order)
owns load and selection order; filenames and comments are not setup predicates.

On Map3, F506 selects damaged animal-cage block copies, a default-only zone table, changed actors
and the sad-music/fade init script. Init hides Kiwi when joined flag F6 is set. **Confirmed authored
text:** the selected townspeople's IDs 665–673 and 678 describe soldiers entering the castle,
violence and broken cages; prone actors share death narration. These lines establish what the game
says, not independent observation of every off-screen event.

### Naming consent leads to a separate join

**Confirmed handler order:** `Map3_1FA_EntityEvent15` (`0x51256`) returns when F6 is already set.
Otherwise IDs 674–677 present the frightened animal and naming invitation. A nonzero answer returns;
zero calls `j_NameAlly(6)`, continues the dialogue and invokes `cs_512BA`. Naming is not itself the
membership transaction. That program's command 4 is `join ALLY_KIWI`; command 5 is
`addNewFollower ALLY_KIWI`.

| Consumer | State effect and boundary |
| --- | --- |
| `csc08_joinForce` | Ordinary `j_JoinForce(6)`, class lookup and fixed text 446 with name/class substitutions, followed by wait/close. The fixed join message is separate from the scene dialogue cursor. |
| `JoinForce` / `JoinBattleParty` | Set joined F6, rebuild via `UpdateForce`, then check the rebuilt active count against 12. Below capacity, set active F38; a full party does not force Kiwi active. Join does not restore HP. |
| `csc56_addFollower` / `AddFollower` | Append the current entity behind the last exploration follower, installing current follow action if admitted. This does not create a persistent Kiwi row in `table_Followers` or set a force flag. |

The [membership contract](../../../contracts/party/party-membership-state.md#joinforce-chronology) requires the
rebuild-before-activation order. There is no second `UpdateForce` in `JoinForce`; setting F38 does
not prove a synchronized active prefix/count on helper return. The contract retains symbolic ranges
and capacity; numeric Kiwi flags/capacity above are selected-source facts from segment research.
The [script roster contract](../../../contracts/party/party-roster-state.md#handler-boundary) separately owns join
and `addNewFollower` handler chronology.

Kiwi's scene presence, chosen name, joined membership, active flag, rebuilt lists and current following
are separate facts. Natural interaction, post-join HP/party synchronization, visible following and
reentry reconstruction remain **Unknown**. Optional recruitment is not a `CheckBattle` prerequisite.

## 3. The Castle Separates Astral's Departure from Royal Following

**Confirmed authored content:** Map19 F506 NPC IDs 678–689 describe Galam's magic, Astral's
incantation and Elis being taken. They do not unlock Battle07. Its two zones call handlers located
in `s3_zoneevents_507.asm` even though the selected table is `s3_zoneevents_506.asm`.
The `507` suffix does not impose an F507 check.

| Conditional caller | Content and actual ordered effects |
| --- | --- |
| Map19 `(255,15)`, `Map19_1FA_ZoneEvent0`, F620 clear | Run `cs_534D2` (IDs 694–712): Astral challenges Lemon's orders and the king's identity; Elis cries out; Astral persuades Lemon to head for the tower. Commands 56–59 hide Astral 128, Lemon and guards 132/133. After script return, the handler sets F620. No Lemon join occurs. F620 set skips this scene. |
| Map19 `(25,12)`, `Map19_1FA_ZoneEvent1`, F621 clear | Run `cs_535FA` (IDs 690–693): minister 129 identifies the Ancient Tower and king 130 resolves to rescue Elis. Commands 5/6 request king → Bowie, then minister → king following. After script return, write F83, then F621. This handler does not test F620. |
| Same minister caller, F621 already set | Skip the whole script and both writes. A supplied F621-set/F83-clear state is not repaired here. Source-local independence from F620 does not prove that a player can bypass the first zone. |
| Map19 F506 init on reentry | F620 hides Lemon, Astral and the two guards; F621 hides minister/king. These suppress original scene actors; they do not themselves reconstruct arbitrary current followers. |

The following requests use `csc2C_followEntity`'s character-alive gate and `AddFollower` for current
entities. F83 has separate persistent rows in `data/scripting/entity/followers.asm`: `FOLLOWER_A`
king and `FOLLOWER_D` minister. `InitializeFollowerEntities` tests each row flag;
`InitializeFollowerActscripts` assigns scripts to admitted rows. Neither joins the combat force.
A narration of accompaniment, current follow action, F83 admission and completed cross-map following
must therefore remain distinct. Multiple/conflicting incoming follower flags and actual actor/movement
state remain **Unknown**.

**Confirmed later local content:** Map20's F506 init points directly into
`ms_map20_flag501_InitFunction`, bypassing the old bedroom introduction. Map21 similarly enters
`ms_map21_flag609_InitFunction` after the earlier guard-position script. These entries call the shared
music/fade script only if F507 is set. Their F506 entity-event tables are default-only: the older
Map21 guard conversation and `setStoryFlag 1` are not selected merely because implementation shares
a file. A reused entry label does not reinstate its preceding scene.

## 4. The Tower Approach Is a Chain of Conditional Requests

**Confirmed selected exits:** each row below is a `warpNoScroll` record. It states what happens if
that marker produces the request. It does not establish walking from one arrival to the next marker;
255 denotes a coordinate wildcard. The
[request owner](../../../../research/world/map-event-request-consumption.md#confirmed-consumer-topology)
closes the local poll/clear seam, while the
[segment request review](../../../../research/chapters/grans-opening/battle06-to-battle07-content-route.md#conditional-onward-map-requests)
owns the selected source composition.

| Requesting map / marker | Destination / facing | Content or admission condition |
| --- | --- | --- |
| Map66 `(30,32)` | Map3 `(1,27)`, Right | F640 must be clear to retain Map3; otherwise `SwitchMap` substitutes the earthquake map. F506 selects town content rather than changing this record. |
| Map3 `(255,1)` | Map19 `(26,30)`, Up | Castle F506 setup and the independent zone callers above. |
| Map19 `(6,2)` | Map20 `(6,37)`, Right | Selected west-tower leg; other castle exits are separate requests. |
| Map20 `(3,36)` | Map21 `(3,16)`, Right | Separate next west-tower leg. |
| Map21 `(9,1)` | Map40 `(4,30)`, Up | Tower exterior; F506 selects the Lemon context only while later setup flags permit it. |
| Map40 `(255,12)` | Map57 `(8,18)`, Up | F401 clear is needed for this selected no-battle continuation; otherwise row 1 can intercept even with F501 set. |
| Map57 `(255,4)` | Map60 `(53,1)`, Right | F506's entrance block copy overlaps the north approach; legal traversal is unresolved. |
| Map60 `(255,51)` | Map58 `(13,36)`, Up | A separate stairway marker, requiring F999 clear to retain effective Map58. |

`ProcessMapEvent` clears the pending event type. Type-1 parameter 1 zero unwinds exploration and
supplies map/X/Y/facing to MainLoop; nonzero parameter 1 takes the scrolling branch instead.
MainLoop calls `SwitchMap` before `CheckBattle`, then enters the effective destination's exploration
setup/init only if no encounter was selected. This ordering matters at Map57 and Map58.

**Confirmed tower content:** Map40 F506 places a prone Lemon and two guards. Later F507 selects its
default setup instead; the F506 init calls music/fade only if F507 is set and its default zone returns.
Lemon's `cs_53FE0` (IDs 997–999) urges rescue and describes a devil and black cloud; guard ID 1000
uses death narration. Shivering/stopping the scene actor is no combat-HP/death write or force join.
No F620 check here independently proves Lemon's off-screen journey.

**Confirmed layout data, limited consequence:** Map57 copies a `2 x 1` block from `(17,0)` to `(7,4)`
when F506 is set. It overlaps the north warp approach, but proves neither passability nor a mandatory
gate. Map60's flag copies depend on F999 and its local stair/portal records do not close traversal
from `(53,1)` to Y=51. Maps19/20/21/40 have empty flag-copy tables; Map19 has two local step copies.
No complete working layout, collision, occupancy or follower route is reconstructed here.

Map66 itself is also conditional: its default zone calls `CheckRandomBattle(3)`, which can set F403
and request reload according to F503, step counter and RNG. Its other named zones have later
F537/F538/F539-dependent consumers. Walking from a variable Battle06 victory position to Granseal
is therefore not proved encounter-free. These consumers retain route limits without opening later
campaign research.

## 5. Battle07 Admission Tests the Effective Map and F407

**Confirmed selector:** `CheckBattle` (`0x799C`) scans ascending rows. Row 7 (`0x7A67`) uses Map58,
area `(0,0,28,38)` and wildcard trigger `(255,255)`; no earlier row uses Map58. With effective map 58
and F407 set it selects battle 7 regardless of requested X/Y. The rectangle is returned battle
geometry, not a containment test. F507 set clears F407 after selection and still returns battle 7
on that call. The [encounter contract](../../../contracts/battle/battle-encounter-definition.md#map-coordinates-and-trigger-metadata)
preserves the metadata boundary.

| Counterexample input | Conditional source result |
| --- | --- |
| F506 set, F407 clear | No row-7 selection; completion/content does not substitute for unlock. |
| Map58 request, F999 set | `SwitchMap` substitutes Map59 before checking; the originally requested map does not establish Battle07 admission. |
| Earlier Map57 request, F401 set | Row 1 selects Battle01 even with F501 set; its completion clears the unlock after selecting, not before rejecting the battle. |
| Effective Map58, F407 and F507 set | One Battle07 selection still occurs, with F407 then cleared. |

F506, F620, F621, F83 and Kiwi membership are absent from `CheckBattle`'s row-7 predicate.
That omission does not prove physical access, a playable bypass, or that these supplied flag
combinations are naturally attainable. The accepted connection is conditional source composition;
existing rails do not executable-close intro → preserved outcome flags → selected requests →
effective map → encounter as a continuous journey.

## 6. Battle07 Before Clears Royal Following and Stages the Confrontation

**Confirmed selected before/start:** before row 7 targets `bbcs_07` (`0x49CE2`). For new entry with
F457 clear, command 1 clears F83, command 2 stages Map58/camera `(8,29)`, and command 3 loads
`ce_49F36`. Camera coordinates are not a player warp. Bowie is staged at `(13,35)` with Galam,
Astral, Elis, king, minister and three enemy scene actors. Their presence does not require a preceding
successful F83 follower observation and does not establish the combat roster.

**Confirmed authored dialogue:** ordinary IDs 2357–2384 depict Galam threatening Elis, Astral and
Galam confronting and immobilizing each other, Elis's blocked escape, Astral asking Bowie to rescue
her and Galam summoning attackers. This sets up the encounter; the program contains no new force
join or onward `setStoryFlag`, and this guide does not explain its outcome.

BattleLoop restores the supplied map/battle after before, clears region flags 90–105, heals eligible
allies, initializes ally/enemy positions, clears AI memory and loads battle, then calls start.
The row-7 start target is `ms_Empty`; the wrapper sets F457 before dispatch. Empty termination retains
interpreter cleanup/waits rather than proving instant completion. F457 already set skips both
scripts, including the F83 clear. F88 suspended entry restores saved seconds, clears F88/AI memory
and reloads battle state, bypassing new-battle initialization and before/start. The
[intro contract](../../../contracts/battle/battle-cutscene-routing.md#shared-intro-gate) owns the wrapper gates;
the [selected entry review](../../../../research/chapters/grans-opening/battle06-to-battle07-content-route.md#battle07-admission-and-beforestart-stopping-boundary)
owns their serial placement and Battle07 targets.

`BattleSpriteset07` (`0x1B3720`) declares 7 ally placements, 10 enemies, 4 AI regions and 1 AI point.
Initialization consumes the actual joined/active/living force: seven placements do not force Kiwi
active or prove seven living allies. Stop at this selected before/start boundary. Battle07 resolution,
its later long scene and subsequent campaign are separate work.

## Evidence Review and Remaining Limits

The pre-synthesis review inspected accepted research, the predecessor, relevant contracts, selected
public payloads and the verifier comparisons below. No fresh extraction, source acquisition, runtime
observation or H2 rerun occurred. The research owner supplies selected original-source composition,
actor/text meaning, exact records/targets and pinned provenance/reproduction receipts. Ordinary text
summaries assume the unskipped branch; debug/P2-Start can suppress ordinary text/cursor increments,
while fixed `txt`/join messages are separate. Rendered speech, audio and completed input waits are
not witnessed here.

| Claim | Reviewed executable owner and reproduction route | Disposition |
| --- | --- | --- |
| Outcomes, defeated/after and membership tail | [cutscene fixture](../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), ID `sf2-battle-cutscenes-static-v1`, `cutsceneFacts.intro/afterBattle/enemyDefeated/leaderDeathPositions`; [control fixture](../../../../../tests/fixtures/h2/battle-control-static-v1.json), ID `sf2-battle-control-static-v1`, `controlFacts.mainLoop/outcomes`; [cutscene](../../../../../src/sf2tool/h2/battle_cutscenes.py) and [control](../../../../../src/sf2tool/h2/battle_control.py) verifiers. `uv run sf2 h2 battle-cutscenes`, `battle-control`. | Generic guards and outcome facts do not establish selected row 6 or natural victory. Research supplies the empty targets, leader/position-table absence and variable return composition. The older upgrade boolean is a call, not an effect on these battles. |
| F407/F83, Kiwi join and follows | [script fixture](../../../../../tests/fixtures/h2/map-script-engine-static-v1.json), ID `sf2-map-script-engine-static-v1`, `storyStateCommandFacts`, `forceStateCommandFacts`, `entityGestureRelationshipMotionCommandFacts`; [verifier](../../../../../src/sf2tool/h2/map_script_engine.py). [stats fixture](../../../../../tests/fixtures/h2/common-stats-static-v1.json), ID `sf2-common-stats-static-v1`, `statsFacts.party`; [stats verifier](../../../../../src/sf2tool/h2/stats.py). `uv run sf2 h2 map-script-engine`, `common-stats`. | Sites `bbcs_06:45`, `bbcs_07:1`, `cs_512BA:4` and handler guards retain writes/join separately. Current follows and persistent F83 rows use accepted consumer review. Four stats facts do not prove post-activation synchronization. |
| Kiwi consent and castle caller order/reentry | [entity](../../../../../tests/fixtures/h2/map-events/entity-programs.json) and [zone](../../../../../tests/fixtures/h2/map-events/zone-programs.json) shards of `sf2-map-events-static-v1`, program orders 467 and 31/32; [event verifier](../../../../../src/sf2tool/h2/map_events.py). [init fixture](../../../../../tests/fixtures/h2/map-init-static-v1.json), ID `sf2-map-init-static-v1`, `primarySourceBodies`; [init verifier](../../../../../src/sf2tool/h2/map_init.py). `uv run sf2 h2 map-events`, `map-init`. | Complete semantic comparisons retain naming before script, F620 after return, F83 before F621 and init hiding. The F506 table's 507-named implementations are not F507 predicates. Natural invocation and follower reconstruction remain Unknown. |
| Requests, substitution, encounter and block copies | [request fixture](../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), ID `sf2-map-event-request-consumption-static-v1`; [common-map fixture](../../../../../tests/fixtures/h2/common-maps-static-v1.json), ID `sf2-common-maps-static-v1`, `mapFacts.mapSwitch/battleTrigger/egress`; [content fixture](../../../../../tests/fixtures/h2/map-content-static-v1.json), ID `sf2-map-content-static-v1`; [request](../../../../../src/sf2tool/h2/map_event_request_consumption.py), [map](../../../../../src/sf2tool/h2/maps.py) and [content](../../../../../src/sf2tool/h2/map_content.py) verifiers. `uv run sf2 h2 map-event-request-consumption`, `common-maps`, `map-content`. | Local consumers/inventory do not compose a continuous route or establish legal walking. Research supplies selected records and F401/F999 controls; block-copy presence is not collision/passability evidence. |
| Before/start and encounter placement | [routing fixture](../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json), ID `sf2-battle-routing-data-static-v1`, `facts.routeTargetsParsed=false`; [spriteset fixture](../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json), ID `sf2-battle-spriteset-data-static-v1`, `facts.rowContentTracked=false`; [routing](../../../../../src/sf2tool/h2/battle_routing_data.py) and [spriteset](../../../../../src/sf2tool/h2/battle_spriteset_data.py) verifiers. `uv run sf2 h2 battle-routing-data`, `battle-spriteset-data`. | Exact targets/header/staging use research source review rather than these inventories. F457/F88 skips and seven placements versus the actual party delimit the claim. |

The controller contract's New Battle overview groups before/start more broadly than the selected
serial order; use the precise wrapper/segment order above. Membership contracts deliberately retain
symbolic values and do not close synchronized lists. The request owner's historical H3 question
queue and research's possible existing-owner extensions are Unknown dispositions, not authorized
acquisition. These narrower boundaries agree with the executable owners; no Layer A repair or
new-fixture claim follows from this guide.

**Inferred:** the intended rescue journey connecting Granseal and the tower. **Unknown:** natural
outcomes and incoming flags/savepoint/caller/actor/HP/party state; legal walking, random encounters,
working-layout collision and occupancy, tower/stairway access or bypass; reentry and conflicting
followers; completed services and presentation/input timing; persistence and winnability. The
[research gap owner](../../../../research/chapters/grans-opening/battle06-to-battle07-content-route.md#existing-coverage-and-gap-disposition)
retains their discriminating controls. Named H2 commands are reproduction routes, not fresh PASS
claims; load `local/private-inputs.ps1` in the same process before launching one. No unmerged
Battle07-outcome conclusion is consumed, and the guide adds no remake, UI or translation support.
