# Battle 04 Outcomes through Battle 05 Admission

Battle04 has two different source paths into Galam captivity: a first-completion victory runs
Lemon's capture scene, while a special defeat returns to Galam after the ordinary loss costs.
Escape follows the selected egress state. In Galam's interiors, prison conversations, opened cells and
Slade's shortcut provide the next authored content; the castle guards supply the actual unlock
and request for Battle05. **Inferred story connection:** these scenes continue the encounter in
[Battle 03 outcome through Battle 04 admission](battle03-outcome-through-battle04.md).
**Confirmed static:** the accepted
[Battle04-to-Battle05 investigation](../../research/battle04-to-battle05-content-route.md) connects
their conditional callers and consumers. **Unknown:** a natural combat outcome, continuous legal
walk, incoming entity state and completed presentation. The source does not establish a forced loss
or a successful walkthrough.

This guide explains locations, actors, dialogue content and state handoffs through Battle05
before/start. Battle04 and Battle05 are zero-based source slots 4 and 5
(`BATTLE_AMBUSHED_BY_GALAM_SOLDIERS` and `BATTLE_GALAM_CASTLE`), not verified original UI numbering.
Galam Castle is Map16; Galam interiors are Map17; Hawel's house is Map43. `Fnnn` denotes a flag.
Original evidence uses the USA retail ROM and `ShiningForceCentral/SF2DISASM` `master` at
`c834c652b6862bc5679fd7f69a38a7093206efc6`, identified by the
[research provenance](../../research/battle04-to-battle05-content-route.md#agreement-and-provenance).
**Confirmed** denotes named static facts or explicitly bounded observations; **Inferred** denotes
the narrative connection; **Unknown** names the missing execution boundary.

The [accepted private remake milestone](map3-battle01-readiness.md#accepted-current-milestone)
ends at Battle01 usable return. This explanation adds no later playable support and uses no remake
implementation as original-game evidence.

## 1. Battle04 Outcomes Reach Different Return Paths

**Confirmed shared outcome rule:** the controller counts combatants after action death processing
and again after turn-end effects/death processing. A nonnegative X and positive HP are required;
Bowie's zero HP forces zero allies. Zero allies selects defeat before zero enemies selects victory.
Battle04's enemy-leader-presence row is zero, and its defeated-enemy program is `ms_Empty`.
Killing slot 128 alone therefore supplies neither its victory rule nor a scripted forced defeat.
The [battle-control contract](../contracts/battle-control-lifecycle.md) owns the shared lifecycle;
the [selected outcome analysis](../../research/battle04-to-battle05-content-route.md#battle04-outcome-branches)
owns the Battle04 composition without claiming winnability.

| Outcome and predicate | Ordered state effects | Return boundary |
| --- | --- | --- |
| Victory, F504 clear | Heal living/immortal allies and prepare the current map plus the first battle-party member's final combatant X/Y plus area origin and entity facing. The after-program replaces those return fields with its capture warp. The shared tail calls `JoinForce(0)`; victory clears F404 and sets F504. | Map17 `(0,0)`, Right, result `d4=1`. MainLoop switches map and enters exploration without a new encounter check. |
| Victory, F504 set | Skip the capture program but still call `JoinForce(0)` and clear F404/set F504. | Prepared final-state operands survive: Map66 for the preceding encounter caller. This repeat branch does not establish a natural repeated battle. |
| Battle04 defeat | Call the unlock helper, display the defeat message, restore Bowie's HP, halve gold by unsigned floor division and obtain the egress tuple. The special tail then clears F404, sets F504, calls `j_UpgradeBattle`, overrides the map to 17 and returns `d4=0`. It executes no after-program or shared after-join. | Only map/result are overridden. With the predecessor's `EGRESS_MAP=43`, the normal F399-set lookup supplies `(7,5)`, Down; those operands survive in the Map17 return. |
| Egress / Angel Wing | Egress spends its MP cost; Angel Wing removes its selected item slot. Both use the unlock helper and egress lookup, returning `d4=0`. Neither performs the special defeat tail, capture program or F504 write. | With unchanged `EGRESS_MAP=43`, Map43 `(7,5)`, Down. F504 clear leaves F404 armed; F504 set clears F404. Escape does not supply the captivity handoff. |

**Confirmed helper limits:** `UpdateBattleUnlockedFlag` clears the current unlock only when its
completion is already set. `GetEgressPositionForBattle` has no Battle04 destination exception;
normal BattleLoop entry sets F399, selecting the map savepoint lookup. A supplied F399-clear helper
call instead uses the game-start tuple; an unlisted egress map has its own default coordinates.
These alternatives prevent treating the preceding Map43 caller as a universal destination.

The special loss's `j_UpgradeBattle` resolves to `UpgradeBattle`, which preserves d0–a6 and scans
the random-battle list. **Confirmed selected-source result:** Battle04 is absent; the call writes
no completion flag, changes no enemies, advances no battle ID and unlocks no Battle05. F504 comes
from the explicit defeat tail. The older fixture name
`controlFacts.outcomes.battle4DefeatUpgradesBattle=true` records this named call, not an effect.

### First victory runs Lemon's capture content

**Confirmed authored scene:** `abcs_battle04` at `0x498EE` binds normal cursor IDs 2319–2322 to
Lemon. He praises the battle, refuses to let the party leave, identifies the leader and orders
the party taken to Galam. Scripted movement, impact sound and poses stage the capture; they do not
write combatant HP to zero. The scene entity table includes companions and soldiers, which does
not prove active-party membership.

Its terminal command 57 requests Map17 `(0,0)`, Right. The warp handler writes event type 1 and
parameters 1–5; victory reads the overwritten parameters as its own return. Exploration clears the
pending event type on entry, so this is not a second later type-1 dispatch. The consumed zero join
table is the one under `data/battles/cutscenes/`, not the unused namesake under `global/`;
zero reaches `JoinForce(0)`, not Slade recruitment. The
[cutscene contract](../contracts/battle-cutscene-routing.md#after-battle-route-and-join-tail)
and [party-membership contract](../contracts/party-membership-state.md) retain the distinct wrapper
and joined/active domains.

## 2. Waking in Galam Restores Battle Stats, Not Resources

**Confirmed setup boundary:** Map17's ordered selectors are default, F505, then F530; the last
matching selector wins. The early content below assumes F505/F530 clear. Neither Map16 nor Map17
has a flag-switched-map override. Later setups are alternative endpoints, not part of an asserted
early-story route. The [map-setup contract](../contracts/map-setup-data.md) owns setup structure;
the [captivity analysis](../../research/battle04-to-battle05-content-route.md#conditional-captivity-and-escape-content)
owns these selected callers.

Default `ms_map17_InitFunction` (`0x52450`) calls `cs_5249E` only with F660 clear, then sets F660.
Afterward, F663 set removes entity 131 and calls the four door/block copies in `cs_5247C`.
The caller always sets F611. F660 set skips wake-up, its reset and its placement: the distinct
victory/loss return coordinates must not be erased for that repeat alternative.

**Confirmed reset order:** `resetForceBattleStats` → `resetMap` →
`loadEntitiesFromMapSetup 0,0,RIGHT`. The first calls `ResetAlliesBattleStats`: all 30 ally slots
have maximum HP/MP restored; their existing status bits are masked with `0x0007`, retaining only
existing STUN/POISON/CURSE bits, then derived stats are refreshed. There is no dead-ally skip.
This differs from the living/immortal heal used around battle. It restores no
gold, recruits nobody and confiscates no inventory. The special defeat's earlier gold reduction
therefore remains. Map reset/load operations have their own consumers and do not imply a new game.
The [party/roster contract](../contracts/party-roster-state.md#handler-boundary) owns the reset call;
the [map contract](../contracts/map-exploration.md#load-and-selection-order) owns load distinctions.

The normal wake-up dialogue uses IDs 880–891, then explicitly resets to 892–893. Sarah, Kazin,
Chester and the two prisoners discuss Galam's war and the Ground Seal; Bowie wakes and joins the
conversation. The program stages Bowie at `(59,4)`, Up, then destination `(59,5)`. Default setup's
132 is an old-man sprite and 133 a soldier sprite; dialogue describes a scholar and an army leader.
Keep sprite identity and dialogue role separate. The named setup entries include Slade at `(48,4)`;
declaration alone does not join him. Incoming follower flags are initialized before setup entities
and can affect actual population/placement.

## 3. Prison Doors, Slade and the Passage

The following **Confirmed static** phases are zone-selected field content, conditional on supplied
coordinates and flags. The zone dispatcher does not prove a legal walk between them.

| Phase and caller | Grounded content | State handoff and repeat alternative |
| --- | --- | --- |
| Prison conversation: Map17 `(53,5)`, F661 clear → `cs_52530` | Slade reacts to the jewels, opens the two cells and faces Sarah's accusation about breaking the seal. Normal IDs 901–931, reset to 931, then 932–946 include companions, prisoners, Slade and narration. The two wire/keyhole narrations reuse ID 931; ID 910's actual actor is 133, despite wording that could suggest Slade. | Four door/block copies; Jaha follows Bowie, Kazin follows Jaha, Chester follows Kazin and Sarah follows Chester. The caller sets F661 after return. F661 set skips the scene. No warp or F405 write. |
| Stairs warning: Map17 `(43,9)`, F662 clear → `cs_528D4`, then `cs_528CA` | IDs 947–951 have Slade warn against the stairs and narration describe a shortcut. | Reveal the tile at `(57,4)`, then move Bowie one step right, then set F662. F662 set instead displays fixed IDs 948/951 and performs only the one-step script. F661 is not an explicit gate. |
| Accompaniment/passage: Map17 `(57,5)`, F662 set → `cs_52938` | IDs 952–957 contain Slade's admission of responsibility, request to accompany, narrative acceptance and passage/kitchen directions; ID 955 is narration. No yes/no branch or join operation. | Reload/hide/stage/follow sequence below; the caller sets F73 after return. F662 clear returns without it. There is no F73 one-shot guard. |

**Confirmed tunnel order:** fade out → reload the current map with camera `(24,14)` → reset action
scripts for the four companions and Slade → hide the four companions → stage Bowie/Slade in the
passage → make Slade follow Bowie. Scripted destinations are Bowie `(29,18)` and Slade `(29,17)`.
`reloadMap` calls `LoadMap` for the current map; it neither recursively runs setup init nor requests
a map exit. It precedes the hide/staging work, rather than rebuilding those entities afterward.

`followEntity` calls `AddFollower`; F73 separately selects Slade during follower initialization.
**Confirmed state distinction:** neither operation writes joined flag F5. Narrative acceptance,
setup presence and following are not force recruitment or active battle membership. Hiding the
companions likewise does not remove them from the force. No investigated captivity/guard script
awards or removes inventory. The jewel conversation is not an item transaction.

Other prison interactions add context without supplying the battle unlock: companion/prisoner
speech and the maid scene `cs_52A3C` (IDs 958–961) describe the situation nearby. That scene is
conditional on entity 131/F256, with F256 set after return and a repeat line; F256 is temporary and
cleared on supplied-map exploration entry. It is not a proven mandatory stop.

### Reentry does not automatically reconstruct every opened route

**Confirmed selected writes:** the four door copies are repeated by init only when F663 is set;
the shortcut copy belongs to the F662-clear warning program. Map17's step table is empty, and its
flag-copy table contains only the F999-controlled north-gate copy. F661/F662 alone therefore do
not establish automatic reconstruction of those script mutations on reload. Working-layout,
occupancy/follower state and reentry persistence remain **Unknown**.

## 4. Castle Content and the Actual Battle05 Producer

**Confirmed candidate connections:** Map17's no-scroll warp records include `(2,24)` → same map
`(32,9)`, Up; `(35,2)` → same map `(16,9)`, Right; and `(17,3)` → Map16 `(23,5)`, Right. The first
two have reverse records. These support the ladder/cellar/kitchen connection suggested by the
dialogue, but prove no continuous walk from the passage or between markers. Other exits exist.
The [map contract](../contracts/map-exploration.md#areas-events-and-items) keeps marker/collision
conditions separate from warp operands.

Map16's selectors are default, F663, then F530. With the early setup selected, its X=17/any-Y zone
calls `cs_51F88` only when F663 is clear and sets F663 afterward. **Confirmed authored content:**
normal IDs 962–985 bind King Galam, Lemon, Slade and crowd narration. Galam alleges Granseal killed
his messenger; Lemon rallies the army, the group departs and Slade discusses leaving. The program
hides Galam/Lemon and army entities 129–144, without combatant death or force-removal writes.
Gate/bridge orders have sound/waits here, not an explicit block-copy command, so changed collision
is not established. This scene does not write F405. F663 selects subsequent alternate setup, which
retains guards 145–147 and shares the zone/init pointers; F530 has later priority and lies outside
this chain.

### The guards unlock and request battle separately

**Confirmed guard caller:** Map16 X=3/any-Y → `Map16_ZoneEvent1`. F505 set returns immediately.
Otherwise it writes `EGRESS_MAP=16` before testing F664. With F664 clear, `cs_521BA` (`0x521BA`)
uses normal IDs 986–996: guard 145 discovers the escapees, Slade and guards 146/147 discuss the
Jewel of Light, and an arrest alarm follows. This field script supplies the actual unlock:

1. `setStoryFlag 5` adds the battle-unlocked base 400 and sets F405.
2. It falls through `cs_5227C` to request Map16 `(0,0)`, Right, event type 1/parameter 1 zero.
3. After script return, the ASM handler sets F664, before exploration dispatches the request.

Neither F663 nor F73 is a prerequisite in this guard handler. With F505 clear/F664 set it calls
only `cs_5227C`, reissuing the warp without dialogue or the F405 producer. A supplied
F664-set/F405-clear state therefore does not rearm Battle05. Map16 init also removes guard 145
on F664-set entry, so arbitrary reentry flags do not guarantee the first scene's actor population.
The [dialogue contract](../contracts/dialogue-system.md) owns cursor/fixed-line semantics; the
[selected guard analysis](../../research/battle04-to-battle05-content-route.md#battle05-admission-and-start-boundary)
owns actor bindings and story-flag arithmetic. F405 is neither joined F5 nor completed F505.

**Confirmed request consumer:** `WaitForEvent` polls; `ProcessMapEvent` clears the pending type;
the zero-parameter-1 warp branch unwinds exploration to MainLoop. SwitchMap leaves Map16 unchanged,
then `CheckBattle` selects row 5 if F405 is set. Nonzero parameter 1 would stay in exploration's
scrolling branch. The [request-consumption owner](../../research/map-event-request-consumption.md)
stops at local dispatch; the accepted research supplies the downstream composition.

Map16 is row 5's map and the only matching map row, so earlier unlocked encounters cannot preempt
it here. Both trigger coordinates are wildcard 255; `(0,14,24,14)` is returned battle-area data,
not an admission rectangle that rejects the request's `(0,0)`. F405/F505 both set makes the selector
clear F405 and still return 5 on that call, even though the guard caller would have returned on
F505. Those are distinct predicates. F663/F73 are not selector requirements.

## 5. Battle05 Before and Start Have Empty Programs

**Confirmed normal preparation:** with F88 clear, BattleLoop installs current map/battle for before,
restores its supplied values afterward, clears region flags 90–105, heals living/immortal allies,
initializes both rosters, clears AI memory, loads battle and then calls start. The
[shared intro gate](../contracts/battle-cutscene-routing.md#shared-intro-gate) checks F455 for
before without setting it; start sets F455 before dispatch. Both row-5 targets are `ms_Empty`.
The arrest dialogue belongs to the preceding field script. Empty termination still has interpreter
wait/scroll cleanup; it is not proof of instantaneous completion.

F455 already set skips both programs; F88's suspended entry bypasses ordinary preparation and
these wrappers. `BattleSpriteset05` (`0x1B35A2`) declares 6 ally placements, 8 enemies, 1 AI region
and 1 AI point; terrain row 5 selects `BattleTerrain05`. Six placements do not prove six recruited
allies or Slade participation. The [encounter contract](../contracts/battle-encounter-definition.md)
and [spriteset owner](../../research/battle-spriteset-data.md) retain the data boundary.
**Unknown:** natural entry, active Battle05 party, completed services/presentation and first turn.
Battle05 resolution and later story are outside this guide.

## Evidence Review and Remaining Limits

This synthesis reviewed the accepted research, predecessor, linked contracts and selected public
payload/verifier surfaces below. Selected-source conclusions remain attributed to the accepted
research owner; this task performed no new extraction or original observation. Normal dialogue
summaries use its script-operand/matching-text review, not fresh rendered speech or independent
off-screen history. Debug/P2-Start skipping can suppress ordinary text and cursor increments;
fixed `txt` calls are separate. Full scripts, layouts and rosters remain private.

| Claim | Reviewed executable owner and reproduction route | Disposition |
| --- | --- | --- |
| Outcomes and shared wrapper | [loop fixture](../../../tests/fixtures/h2/battle-loop-static-v1.json), `sf2-battle-loop-static-v1`, `lifecycleFacts.remainingCombatants`; [control fixture](../../../tests/fixtures/h2/battle-control-static-v1.json), `sf2-battle-control-static-v1`, `controlFacts.outcomes`; [cutscene fixture](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), `sf2-battle-cutscenes-static-v1`, `cutsceneFacts`; [loop](../../../src/sf2tool/h2/battle_loop.py), [control](../../../src/sf2tool/h2/battle_control.py), [cutscene](../../../src/sf2tool/h2/battle_cutscenes.py) verifiers; `uv run sf2 h2 battle-loop`, `battle-control`, `battle-cutscenes` | Shared facts are compared, but the upgrade boolean does not derive list membership/effect. Battle04 capture overwrite, loss's preserved d1–d3 and list miss use accepted selected-source analysis; no composed runtime witness. |
| Init and reset | [init fixture](../../../tests/fixtures/h2/map-init-static-v1.json), `sf2-map-init-static-v1`, route orders 32–37, `primarySourceBodies`, `embeddedPrograms[id=cs_5249E/cs_5247C]`; [verifier](../../../src/sf2tool/h2/map_init.py), `uv run sf2 h2 map-init` | Complete expected-object comparison owns conditional init and caller writes. F660 skips reset/staging; F663 gates copies; F611 is unconditional. |
| Script consumer/state sites | [script fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json), `sf2-map-script-engine-static-v1`; [verifier](../../../src/sf2tool/h2/map_script_engine.py), `uv run sf2 h2 map-script-engine` | Reviewed force-state `$55` handler/source site `cs_5249E:1`; transition sites `abcs_battle04:57` and `cs_521BA:37`; story-state `cs_521BA:36`; lifecycle `cs_52938:14:reloadMap`. Call/provenance joins do not independently derive every downstream reset/follower effect. Exact tunnel order and absence of joins/item transactions use accepted source review. |
| Zone branches and setup population | [zone shard](../../../tests/fixtures/h2/map-events/zone-programs.json), fixture `sf2-map-events-static-v1`, selected `zoneTargetPrograms` Map17 events 0–2/Map16 events 0–1; [verifier](../../../src/sf2tool/h2/map_events.py); [shard loader](../../../src/sf2tool/h2/map_events_fixture.py), `uv run sf2 h2 map-events`; [setup](../../../tests/fixtures/h2/map-setup-static-v1.json) / [entity](../../../tests/fixtures/h2/map-entities-static-v1.json) fixtures and [setup](../../../src/sf2tool/h2/map_setup.py) / [entity](../../../src/sf2tool/h2/map_entities.py) verifiers; `uv run sf2 h2 map-setup`, `map-entities` | Complete zone semantics retain F662-clear suppression, fixed repeat lines, F73/F664 after-return writes and F505 early return. Setup/entity structure supplies no natural invocation or live population proof. |
| Warps, requests and selection | [content](../../../tests/fixtures/h2/map-content-static-v1.json), [request](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), [common-map](../../../tests/fixtures/h2/common-maps-static-v1.json) fixtures; [content](../../../src/sf2tool/h2/map_content.py), [request](../../../src/sf2tool/h2/map_event_request_consumption.py), [map](../../../src/sf2tool/h2/maps.py) verifiers; `uv run sf2 h2 map-content`, `map-event-request-consumption`, `common-maps` | Structure/local dispatch does not prove walking or compose guard → request → row 5. Accepted source review supplies the conditional relation and F664-set/F405-clear, nonzero-parameter-1 and completed-row controls. |
| Entry data | [routing](../../../tests/fixtures/h2/battle-routing-data-static-v1.json), `sf2-battle-routing-data-static-v1`; [spriteset](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json), `sf2-battle-spriteset-data-static-v1`; [routing](../../../src/sf2tool/h2/battle_routing_data.py), [spriteset](../../../src/sf2tool/h2/battle_spriteset_data.py) verifiers; `uv run sf2 h2 battle-routing-data`, `battle-spriteset-data` | `routeTargetsParsed=false` and `rowContentTracked=false` retain inventory limits. Selected row-5 targets/header/order use research source analysis, not active-party or completed-service evidence. |

The controller contract's New Battle overview groups before/start too broadly to witness their
serial placement. The precise order above follows accepted selected-source analysis and the intro
wrapper owner. The old upgrade label is likewise narrowed to its call, with the no-effect list miss
kept explicit. These discrepancies are nonblocking at that stricter boundary; Layer A is unchanged.
Generic runtime queues and proposed existing-owner extensions do not authorize acquisition or new
fixtures. Named H2 commands are reproduction owners, not fresh PASS claims in this synthesis; load
`local/private-inputs.ps1` in the owning process before reproduction.

**Inferred:** the intended captivity/escape connection. **Unknown:** natural outcome and return
state, marker/collision/occupancy-aware walking, incoming followers and actors, reentry reconstruction,
scene/service completion, rendered speech/input/audio, save persistence and the active Battle05 party.
The [research gap disposition](../../research/battle04-to-battle05-content-route.md#evidence-coverage-and-gap-disposition)
retains the missing relations and discriminating controls. The
[roadmap](../documentation-roadmap.md#current-baseline-and-near-term-synthesis) owns future scope;
[Story Progression](story-progression.md) supplies the wider state framework. Stop here before
Battle05 resolution; no later unmerged research is consumed.

Continue with [Battle05 outcomes through Battle06 admission](battle05-outcomes-through-battle06.md)
for the position/HP consumer, explicit Slade recruitment, Galam exits and conditional before/start.
The continuation retains outcome, follower/party and natural-route limits.
