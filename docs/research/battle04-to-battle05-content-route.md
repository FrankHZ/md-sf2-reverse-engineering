# Battle 04 outcomes through Galam captivity to source Battle 05 admission

Evidence date: 2026-10-12. **Confirmed:** first-completion Battle04 victory and its special defeat
branch have different consumers but both supply Map17/Galam interiors to exploration. Victory's
capture scene replaces the prepared return coordinates; defeat first restores Bowie and halves gold,
then overrides only the egress map. Escape does not perform either capture path. The direct Map17
and Map16 content can conditionally lead to F405 and source Battle05 admission. **Inferred:** their
connected dialogue and staging express the intended captivity/escape story segment. **Unknown:**
natural combat outcomes, continuous walking, caller entity state and completed presentation. This
is neither a proof of unavoidable defeat nor a successful playthrough.

## Agreement and provenance

This is the Standard-rigor agreement for [Issue #704](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/704),
under [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514). Cross-owner
outcome and event consumers require explicit conditional boundaries; they do not require new tooling.
The sole tracked write is this document. The accepted starting base is
`489b3ea88541dac4b92c6f2d4ce42c3ea7c1405b`; the
[Battle03-to-Battle04 owner](./battle03-to-battle04-content-route.md), accepted in
[PR #702](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/702), supplies conditional Battle04
admission. Concurrent synthesis is independent, with no unmerged evidence dependency.

Scope is Battle04 outcome/after/return consumers, the directly connected Galam captivity content,
and Battle05 admission through before/start dispatch. Battle04 balance/winnability, Battle05
resolution, later campaign, engine/UI work, Chinese translation, new fixtures/parsers and runtime
acquisition are excluded. Acceptance is direct source/fixture/verifier inspection, conditional
content and gap tables, complete prose/link/private-boundary review, clean committed planner and
actual CI, followed by a pushed Draft PR and independent main-gate review. No shared-path edits,
new installation, environment copy or process launch is needed.

All original-source paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM` at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The selected read-only checkout was verified clean at that pin. The predecessor's retained
`h2 map-init` and `h2 map-script-engine` outputs bind the registered USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9` and that source revision.
Their dependencies are unchanged and their fixture comparisons were repeated against retained
payloads; this is not a new extraction or ROM verification. Addresses are static provenance.

`Fnnn` denotes a flag. `sf2enums.asm` defines Battle04 as
`BATTLE_AMBUSHED_BY_GALAM_SOLDIERS=4`, Battle05 as `BATTLE_GALAM_CASTLE=5`,
`MAP_GALAM_CASTLE=16`, `MAP_GALAM_INTERIORS=17`, `MAP_HAWEL_HOUSE=43`,
`MAP_OVERWORLD_GRANSEAL_KINGDOM=66`, and `ALLY_SLADE=5`.
Short battle paths below are under `code/gameflow/battle/`. Map16/17 setup filenames are under
`data/maps/entries/map16/mapsetups/` and `map17/mapsetups/`; content filenames such as
`6-warp-events.asm` are in the respective map directory. Script handlers are in
`code/common/scripting/map/mapscriptengine_1.asm` or `mapscriptengine_2.asm`.

## Battle04 outcome branches

**Confirmed** here denotes selected source operands and consumer order, conditional on their caller.
`battleloop_1.asm` checks remaining combatants after action death processing and again after
turn-end effects/death processing. Zero allies selects defeat before zero enemies selects victory;
`battleloop/countremainingcombatants.asm` (`0x23C58`) counts nonnegative X/nonzero HP and forces zero
allies when Bowie has zero HP. Battle04's row in `data/battles/global/enemyleaderpresence.asm` is zero.
Its defeated-enemy script row in `data/battles/cutscenes/battleendcutscenes.asm` is `ms_Empty`.
The shared wrapper dispatches that script only with Bowie alive, slot 128 dead and F504 clear; its zero leader flag does not
queue all remaining enemies. Killing that slot alone is not the victory rule, and an empty defeated
script does not establish forced defeat. No combat simulation or winnability claim is made.

| Outcome | Ordered consumers and state | Returned destination and safe endpoint |
| --- | --- | --- |
| Victory, F504 clear | `BattleLoop_Victory` (`0x23CBA`, `battleloop_2.asm`) heals living/immortal allies, prepares map-event parameters from current map and first battle-party member's final combatant X/Y plus area origin and entity facing, with parameter 1 zero. `ExecuteAfterBattleCutscene` selects after row 4, `abcs_battle04` (`0x498EE`). Its terminal `warp` overwrites parameters 1–5 and sets event type 1. Shared join row 4 is zero and calls `JoinForce(0)`; victory then clears F404, sets F504 and reads the overwritten parameters. | d0–d3 = Map17, `(0,0)`, Right; d4=1. `MainLoop` at `alt_MainLoopEntry` (`0x75E0`) calls `SwitchMap` then exploration without a new `CheckBattle`. `ExplorationLoop` clears the pending event type on entry. This warp's fields are consumed by the victory return, not a second later type-1 dispatch. |
| Victory, F504 already set | The after wrapper skips `abcs_battle04` but still reaches `JoinForce(0)`. Victory still clears F404 and sets F504. | The prepared final-state map/coordinates survive. For the predecessor's Map66 encounter this is Map66, not the Galam capture warp. A source-available repeat branch is not evidence of natural repeated Battle04. |
| Defeat, Battle04 | `BattleLoop_Defeat` first calls `UpdateBattleUnlockedFlag`, displays fixed ID 363, restores Bowie's HP, halves gold by unsigned shift and calls `GetEgressPositionForBattle`; it initially sets d4=-1. Its Battle04-specific tail then clears F404, sets F504, calls `j_UpgradeBattle`, sets d0=Map17 and d4=0. No after-program or shared after-join call occurs on this path. | d1–d3 remain the egress result; only map/result are overridden. With the predecessor's `EGRESS_MAP=43` and F399 set, the intermediate tuple is Map43 `(7,5)`, Down, so the returned tuple is Map17 `(7,5)`, Down, d4=0. First-visit Map17 init subsequently replaces the scene placement. This is not victory's `(0,0)`, Right request. |
| Egress / Angel Wing | `battlefunctions/battlefunctions_1.asm`: Egress decreases MP by the spell cost; Angel Wing removes the selected item slot. Both reach the shared flash/text-close tail, `UpdateBattleUnlockedFlag`, egress-position lookup, and d4=0. No F504 write, after-program or Battle04 defeat tail occurs. | With unchanged predecessor `EGRESS_MAP=43`, return to Map43 `(7,5)`, Down. F504 clear leaves F404 armed; F504 set clears F404. The actual selected egress state and walking back to the encounter remain conditional. Escape is not the captivity handoff. |

`UpdateBattleUnlockedFlag` only clears current unlock when current completion is already set.
`GetEgressPositionForBattle` (`0x23E50`) has no Battle04 destination exception; it reads `EGRESS_MAP`
and calls `code/common/maps/egressinit.asm`, `GetSavepointForMap` (`0x75EC`). The Hawel-house row is
in `data/maps/global/savepointmapcoords.asm`. `BattleLoop` sets F399 on entry, so its normal outcome
caller reaches the savepoint-table branch. Arbitrarily calling the helper with F399 clear instead
returns the game-start tuple; an unlisted selected map defaults to `(1,1)`, Up. The subsequent loss
override still changes only d0 and d4. These controls prevent turning a helper's default into a
fixed Battle04 destination.

`j_UpgradeBattle` resolves through `code/common/tech/jumpinterfaces/s13_jumpinterface.asm` to
`battleloop/upgradeenemies.asm`, `UpgradeBattle`. It preserves d0–a6, scans
`data/battles/global/randombattles.asm`, and sets `500 + matched battle` only on a match. The list
contains Battle03 and ten later encounters, **not Battle04**. For current battle 4 this call writes
no completion flag, increments no battle ID, changes no enemies and unlocks no Battle05. F504 was
already written by the explicit defeat tail. The older `battle-control` fixture's
`controlFacts.outcomes.battle4DefeatUpgradesBattle=true` records the named call, not a demonstrated
upgrade effect; its value and the shared owner's shorthand must not be promoted to those claims.

`data/battles/entries/battle04/cs_afterbattle.asm` provides the actual victory content. Cursor 2319
covers IDs 2319–2322 (`0x90F..0x912`), all bound to `ALLY_LEMON`: praise for the battle, refusal to
let the party leave, identification of the leader, and orders to take the party to Galam. Scripted
Lemon/Bowie motion, impact sound and pose changes stage the capture; there is no combatant HP-zero
write in that program. `ce_49A6A` stages Bowie, Sarah, Chester, Jaha, Kazin, Lemon and three soldier
sprites; staging does not prove force membership. Command 57 is the terminal Map17 warp.
`csc07_warp` writes event type and the five request fields; it does not synchronously enter that map.
After-table slot 4 is `0x47CFC`; the consumed join slot is `0x47D6E` under `data/battles/cutscenes/`,
not the unused namesake under `global/`. Zero reaches ordinary `JoinForce(0)` in
`code/common/stats/battleparty.asm`, not a no-call sentinel or Slade recruitment.

## Conditional captivity and escape content

Neither Map16 nor Map17 has a flag-switched-map override in `data/maps/global/flagswitchedmaps.asm`.
`GetCurrentMapSetup` in `code/common/scripting/map/mapsetupsfunctions_1.asm` keeps the last matching
selector in source order. Map17 is default, then F505, then F530; Map16 is default, then F663, then
F530 (`data/maps/mapsetups.asm`). The early-story chain below assumes F505/F530 clear and states its
other predicates explicitly. Later selector setups are safe alternative endpoints, not silently
included in this early route.

| Stage | Entry predicate and ordered effects | Supported next boundary |
| --- | --- | --- |
| Map17 wake-up | Supplied-map exploration initializes entities, loads the map, then calls setup init. `ms_map17_InitFunction` (`0x52450`, `s6_initfunction.asm`) tests F660; clear calls `cs_5249E`, then sets F660. It next tests F663; set removes entity 131 and calls `cs_5247C` for four door/block copies. It always sets F611 before return, regardless of F663. | First wake-up scene returns to exploration with its own staged position. F660 set skips wake-up and its reset/placement; both outcome coordinates must therefore remain distinct for that alternative. The comment beside F611 is not an additional predicate. |
| Prison conversation/doors | Default Map17 zone `(53,5)` → `Map17_ZoneEvent0` (`s3_zoneevents.asm`); F661 clear calls `cs_52530` then sets F661, set returns. Scene copies blocks for the two prison doors and makes Jaha follow Bowie, Kazin follow Jaha, Chester follow Kazin, Sarah follow Chester. | Player control can resume with that scene's entity state; no map warp or Battle05 unlock. F661 alone does not establish a traversed door. |
| Stairs warning/shortcut | Zone `(43,9)` → `Map17_ZoneEvent1`; F662 clear calls `cs_528D4`, then `cs_528CA`, then sets F662. The first script reveals the tile at `(57,4)` via block copy; the second moves Bowie right one step. F662 set instead displays fixed IDs 948/951 and calls only that one-step script. | Conditional shortcut opening and displacement. The repeat branch does not reproduce the tile-copy operation. F661 is not an explicit gate of this handler; natural arrival after the conversation is a separate claim. |
| Slade accompanies; tunnel staging | Zone `(57,5)` → `Map17_ZoneEvent2`; F662 set calls `cs_52938` and then sets F73. With F662 clear it returns. The script hides the four companions, reloads the current map with camera `(24,14)`, stages Bowie/Slade in the passage, and finally makes Slade follow Bowie. | Same Map17, scripted destinations Bowie `(29,18)` and Slade `(29,17)`, with F73 set after return. No `join`, `JoinForce`, F5 write, inventory award or map-exit warp appears. No F73 one-shot guard exists here. |
| Candidate kitchen/castle connection | Map17 `6-warp-events.asm`: `(2,24)` → current map `(32,9)`, Up; `(35,2)` → current map `(16,9)`, Right; `(17,3)` → Map16 `(23,5)`, Right. All three are no-scroll records. The first two have reverse records. | These are a source-backed ladder/cellar/kitchen candidate after the passage staging. A legal walk to and between the markers is **Unknown**; the source tuple alone is not a continuous path. Other Map17 exits exist and are not forced into this route. |
| Army departure | Map16 zone X=17, any Y → `Map16_ZoneEvent0` (`s3_zoneevents.asm`); F663 clear calls `cs_51F88` then sets F663. Set skips. Script stages Galam/Lemon's departure and hides them plus entities 129–144. | Same map; F663 changes subsequent setup selection. It does not write F405 or itself prove gate collision changed. The guard-trigger zone has no F663 prerequisite. |
| Guard discovery | Map16 zone X=3, any Y → `Map16_ZoneEvent1`; F505 set returns. Otherwise set `EGRESS_MAP=16`, then test F664. Clear calls `cs_521BA`, whose tail sets story flag 5 and falls through `cs_5227C` to warp Map16 `(0,0)`, Right; after script return the ASM handler sets F664. | F405 set, request type 1 / parameter 1 zero, then handler return. F664 is written after the request is produced and before exploration dispatches it. |
| Already discovered | Same zone with F505 clear/F664 set calls only `cs_5227C`. | Reissues the same warp without dialogue or `setStoryFlag 5`. F664 set/F405 clear does not rearm Battle05; retain that counterexample rather than assuming chronological flags are interchangeable. |
| Admission | `WaitForEvent` → `ProcessMapEvent` clears the pending type → zero-parameter-1 `ProcessMapEventType1_Warp` unwinds exploration to `MainLoop`. `SwitchMap` leaves Map16 unchanged; `CheckBattle` (`0x799C`) selects row 5 when F405 is set. | Battle05, area `(0,14,24,14)`, both trigger coordinates wildcard 255. `(0,0)` need not lie in the battle rectangle: the rectangle is output, not a containment gate. Stop after the before/start boundary below. |

The zone dispatcher (`RunMapSetupZoneEvent`, `0x4751A`) compares each supplied coordinate unless
its table byte is 255 and dispatches the matching relative target. It does not itself prove that
movement reached the zone. Map-script `warp` and no-scroll map-content warps both fill a return
request, whereas `reloadMap` calls `LoadMap` for the current map and does not recursively run init
or return to `MainLoop`. Nonzero request parameter 1 selects an in-exploration scrolling branch
instead. These distinctions are essential to the tunnel scene versus the guard's battle request.

### Entities and persistent state

**Confirmed:** Map17 default `s1_entities.asm` declares named Sarah, Chester, Jaha, Kazin and Slade
entries and falls through into `s1_entities_505.asm` before that list's terminator. The shared tail
supplies logical NPCs 128–134: guards, an old man, maids and a soldier. Thus 132 is the old-man sprite
and 133 the soldier sprite in this setup; dialogue characterizes them as a scholar and army leader.
Slade's setup entry is initially at `(48,4)` with `eas_InitFixedSprite`. Entity declaration does not
call `JoinForce`. `InitializeMapEntities` in `code/common/scripting/map/mapfunctions.asm` initializes
flag-selected followers first and avoids declaring an already indexed ally again; actual placement
therefore also depends on incoming follower flags, not solely the setup list.

`cs_5249E` first executes `resetForceBattleStats`, `resetMap`, then
`loadEntitiesFromMapSetup 0,0,RIGHT`. The `$55` consumer calls `ResetAlliesBattleStats`
(`code/common/scripting/map/resetalliesstats.asm`, `0x478C6`): all 30 ally slots get maximum HP/MP,
status masked to STUN/POISON/CURSE (`0x0007`) and derived-stat refresh, with no dead-ally skip.
This differs from the shared living/immortal heal. It is not a new-game reset, gold restoration,
recruitment or confiscation. `resetMap` calls `ResetCurrentMap` and reloads map data; `$49` selects
the current setup and initializes its entities. Wake-up then stages Bowie at `(59,4)`, Up and later
destination `(59,5)`. Defeat's earlier gold reduction is not undone by these consumers.

The four door/block copies in `cs_52530` are repeated by `cs_5247C` only when the init caller sees
F663. The shortcut tile copy is in `cs_528D4`, not the F662-set repeat handler. Map17's
`4-step-events.asm` is empty and its `3-flag-events.asm` contains only an F999-controlled north-gate
copy; Map16's flag copies are also F999-controlled. Therefore F661/F662 alone must not be described
as automatic map-reload reconstruction of those script mutations. Reentry persistence and the
working-layout route need their own specified caller; they remain **Unknown** here.

`followEntity` uses `csc2C_followEntity`/`AddFollower`. F73 separately selects Slade in
`data/scripting/entity/followers.asm` through `InitializeFollowerEntities`; neither mechanism writes
his joined flag F5. The scripts investigated through Battle05 start contain no Slade recruitment or
removal from the force. Hiding companions and the departing army changes scene entities, not force
membership or combatant death. Map16's default setup declares Lemon and King Galam (128), the staged
army and guard actors 145–147. Its F663 alternate keeps 145–147 at the corresponding logical indices;
it shares the zone and init pointers. `ms_map16_InitFunction` removes 145 on F664-set entry. Reentering
with arbitrary flag/entity combinations does not prove that the first guard scene still has all its
actors. F530 has later priority and a different setup, outside this chain.

No investigated captivity/guard script removes or awards inventory. The guards' Jewel of Light
conversation is a dialogue claim, not an `AddItem` transaction for Bowie or Slade. The only item/MP
costs asserted here are those of the explicit Battle04 escape consumers.

### Dialogue and actor limits

These summaries were checked against actual script command operands and matching hexadecimal IDs in
`data/scripting/text/gamescript.txt`. They describe the normal, unskipped dialogue branch, not rendered
speech or historical truth of off-screen claims. `textCursor` installs an index; selected ordinary
text handlers increment it. Debug/P2-Start skipping can suppress those increments. Fixed `txt` IDs
in ASM handlers are separate from that cursor.

| Program | IDs / actor binding | Grounded content and limit |
| --- | --- | --- |
| `cs_5249E` (`s6_initfunction.asm`) | Cursor 880: 880–891 (`0x370..0x37B`); reset to 892: 892–893 (`0x37C..0x37D`). Sarah, 132/133, Kazin, Chester, then Bowie and Chester. | Prisoners discuss Galam's war and Ground Seal; Bowie wakes and is invited into the conversation. Setup sprites and dialogue roles are retained separately. |
| `cs_52530` (`scripts.asm`) | Cursor 901: 901–931 (`0x385..0x3A3`); explicit reset to 931 repeats that ID, then 932–946 (`0x3A4..0x3B2`). Companions, Slade, 132/133 and no-portrait narration. | Slade reacts to the jewels, opens both cells, and Sarah accuses him of breaking the seal. ID 910 is actually bound to entity 133 despite wording about the jewels that might suggest Slade; no actor correction is invented. The two wire/keyhole narrations intentionally reuse 931. |
| `cs_528D4` | Cursor 947: 947–951 (`0x3B3..0x3B7`), Slade and narration. | Warning against the stairs and revealing a shortcut. Repeat zone uses fixed IDs 948/951. |
| `cs_52938` | Cursor 952: 952–957 (`0x3B8..0x3BD`), Slade and narrator at 955. | Admission of responsibility, request to accompany and narrative acceptance, then tunnel/kitchen directions. No yes/no branch or join operation; narrative acceptance is not force recruitment. |
| Nearby Map17 interactions | `s2_entityevents.asm`: Chester 893, Kazin 895, Jaha 896 before F661; 132 uses 894 then 897; 133 uses 898; Slade 945. Entity 131 with F256 clear calls `cs_52A3C`, then sets F256; repeat is 899. That script uses 958–961 (`0x3BE..0x3C1`). | Companion/prisoner context and a maid's account of an argument nearby. The script moves Bowie and the maid; this is not the F405 producer or a proven mandatory conversation. F256 is a map temporary flag, cleared on supplied-map exploration entry. |
| `cs_51F88` (Map16 `scripts.asm`) | Cursor 962: 962–985 (`0x3C2..0x3D9`), King Galam 128, Lemon, Slade and crowd narration. | Galam alleges Granseal killed his messenger, Lemon rallies the army, the group departs, and Slade discusses leaving. Orders to open gates and lower a bridge are accompanied by sound/wait commands here, not an explicit block-copy command; dialogue alone does not establish a collision change. |
| `cs_521BA` | Cursor 986: 986–996 (`0x3DA..0x3E4`), guard 145, Slade, archer 146 and knight 147. | Discovery of the escapees, discussion of the Jewel of Light and an arrest alarm; this program actually unlocks Battle05. It is field content, not a nonempty Battle05 before-program. |

## Battle05 admission and start boundary

**Confirmed:** `csc13_setStoryFlag` reads operand 5, adds `BATTLE_UNLOCKED_FLAGS_START=400` and
sets F405. It does not set F5 or F505, clear earlier unlocks, or enter battle directly. Map16 is the
map in row 5 of `data/battles/global/battlemapcoords.asm` (`0x7A59`), the only matching map row in
that table. Unlike the earlier Map66 handoffs, earlier unlocked rows cannot preempt it on Map16.
If F405/F505 are both set in a supplied state, `CheckBattle` clears F405 but still returns 5 on that
call; the guard zone itself would have returned early on F505. These are different entry predicates.
F663 and F73 are not requirements of `CheckBattle`.

| Dependency | Confirmed selected relation | Limit |
| --- | --- | --- |
| Before | `data/battles/cutscenes/beforebattlecutscenes.asm` slot 5 (`0x47A92`) is `ms_Empty` (`0x47A4E`). The wrapper checks F455 without setting it. | The arrest dialogue belongs to the preceding field script. Empty script termination still uses interpreter wait/scroll cleanup. |
| Normal preparation | With F88 clear, `BattleLoop` installs current map/battle for before dispatch, restores the supplied map/battle afterward, clears region flags 90–105, heals living/immortal allies, initializes both rosters, clears AI memory and loads battle. | Party state comes from the caller, not the number of scene entities. |
| Data | `data/battles/spritesets/entries.asm` row 5 → `BattleSpriteset05` (`0x1B35A2`, `spritesets/spriteset05.asm`): 6 ally placements, 8 enemies, 1 AI region, 1 AI point. `data/battles/terrainentries.asm` row 5 → `BattleTerrain05`. | Placement capacity is not six recruited allies and does not prove Slade participates. No roster/layout dump is published. |
| Start | `data/battles/cutscenes/battlestartcutscenes.asm` slot 5 (`0x47B36`) is `ms_Empty`. The start wrapper sets F455 before dispatch. F455 already set skips both intro programs. | F88's suspended entry bypasses ordinary preparation/wrappers. Natural entry, active roster, completed loading and first turn remain Unknown. |

The [battle-loop](./battle-loop.md), [cutscene](./battle-cutscenes.md),
[cutscene-data](./battle-cutscene-data.md), [spriteset](./battle-spriteset-data.md),
[global-data](./battle-global-data.md), and [routing-data](./battle-routing-data.md) owners retain the
shared mechanics and data boundaries. This document stops at entry; Battle05 defeated-enemy,
after-program, recruitment, victory/loss and subsequent routes are excluded.

## Evidence coverage and gap disposition

Fixture paths below are relative to `tests/fixtures/h2/`; contract IDs use the `sf2-` prefix.
The selected fixture payloads and `src/sf2tool/h2/` verifier surfaces were inspected. A source inventory
or representative symbol is not an executable proof of the entire composed route.

| Claim / gap | Disposition and owning surface | Missing relation and discriminating control |
| --- | --- | --- |
| Outcome selection, shared after and return | **Covered** at shared scope by `battle-loop-static-v1.json` / `lifecycleFacts.remainingCombatants`, `battle-control-static-v1.json` / `controlFacts.outcomes`, `battle-cutscenes-static-v1.json` / `cutsceneFacts`. **Reuse/extend existing owners** for composed Battle04 effects; `h2 battle-loop`, `battle-control`, `battle-cutscenes`, `battle-global-data`. | `battle_control.py` compares controlFacts and inventory digest; its named upgrade boolean is not a list-membership/effect derivation. Missing executable relation: `BattleLoop_Defeat` → aliased `UpgradeBattle` → random-list miss, preserved d1–d3, override d0/d4. Minimal public result: branch predicates, call target, list membership and ordered state/return fields. Counterexample: Battle03 matches that list but Battle04 does not. Victory control: F504 set skips the capture warp while the zero join tail still executes. |
| Map17 init/reset | **Covered** by `map-init-static-v1.json`: `routeJoins` source orders 35–37, `primarySourceBodies` for `ms_map17_InitFunction`, its `scriptCallSites`, and `embeddedPrograms` `cs_5249E`/`cs_5247C`. Map16 init joins are orders 32–34. Retained `h2 map-init` output again matches the complete expected object under `map_init.py`'s comparison shape. | F660 set skips reset and staging; F663 controls the removal/block copies, not the final F611 write. `map-script-engine-static-v1.json` / `forceStateCommandFacts.activePartyCommandFacts` owns `$55` and its all-ally reset consumer; no gold/inventory reset follows from that name. |
| Setup/entity/zone callers | **Covered** as source selection/stream/operation structure by `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-events/zone-programs.json` and `entity-programs.json`. Commands: `h2 map-setup`, `map-entities`, `map-events`. Owners: [map data](./map-data-inventory.md), [common scripting](./common-scripting.md). | Selected `zoneTargetPrograms` are Map17 events 0–2 and Map16 events 0–1; `map_events.py` checks its assembled complete fixture. A program's branch inventory does not prove natural invocation. Controls: F530 takes later priority; Map17 default entity stream falls through; F662 clear suppresses tunnel staging; F664 set skips the only F405 producer in this handler. |
| Script meaning and dialogue/state | **Covered** at handler/source-site scope by `map-script-engine-static-v1.json`: dialogue, transition, force-state, story-state, block-mutation, entity-population and lifecycle families. Retained `h2 map-script-engine` payload passes `_verify_map_script_engine_fixture`. `map-scripts-static-v1.json` inventories standalone setup-script ownership, not full narrative effects. | Exact program anchors include `abcs_battle04` command 57 (warp); `cs_5249E` command 1 (reset); `cs_52530` 93/94/100/101 (door copies); `cs_528D4` 15 (shortcut copy); `cs_52938` 41 (follow); `cs_521BA` 36/37 (F405/warp), with `cs_5227C` an internal label of the same program. Controls: fixed/cursor IDs differ; ID 931 repeats; ID 910 actor is 133; following/hiding do not join/remove; `reloadMap` is not `warp`. |
| Guard request → Battle05 | **Reuse/extend** the predecessor's conditional-admission candidate, through `map-script-engine-static-v1.json`, `map-event-request-consumption-static-v1.json` and `common-maps-static-v1.json`. Narrow owners: `h2 map-script-engine`, `map-event-request-consumption`, `common-maps`. | Missing executable composition: Map16 zone branch, F405 producer/request, post-script F664 write, request consumption and row 5. Minimal output: source/caller addresses, predicates, request fields and selected row. Controls: F664 set/F405 clear only warps; F505 suppresses the producer; nonzero parameter 1 stays in exploration; row rectangle does not reject `(0,0)`. No standalone per-battle fixture or new command is justified. |
| Natural walking and map mutation persistence | **Reuse/extend [map-content](./map-content.md)** only for a separately needed static route; otherwise **deferred runtime Unknown**. Existing command: `h2 map-content`. | Need specified incoming/outgoing entity coordinates, follower flags, working layouts, door/shortcut mutations and map reload boundaries, especially passage `(29,18)` to ladder `(2,24)` and Map16 entry to its zones. Minimal future output is conditional reachability, not original layouts. Counterexample: a warp tuple on an unmarked/unreachable target or F662 set after reentry without the shortcut-copy producer. No automatic H3 queue. |
| Battle05 entry data/presentation | **Covered** as selected source/table structure by battle cutscene/routing/spriteset/global-data fixtures; commands `h2 battle-cutscene-data`, `battle-routing-data`, `battle-spriteset-data`, `battle-global-data`. **Deferred runtime Unknown:** actual party, natural entry and completed services. | `battle-routing-data` has `routeTargetsParsed=false`; table-target claims here are direct source inspection. Controls: F455 skips intro, F88 bypasses initialization, six placements do not recruit Slade. Stop before Battle05 resolution. |

This yields a useful conditional chain without a missing static unlock producer. It does not close a
continuous route, arbitrary reentry state or every alternative setup. These gaps are explicit safe
endpoints for later scoped work, not reasons to grow this slice into campaign traversal or a platform.

## Reproduction and completion boundary

Work consisted of bounded source/fixture/verifier reads and small in-memory selections. The two
retained static JSON outputs total approximately 18.3 MB and remain unchanged; no new large output,
asset extraction, map decoding, capture or emulator/Godot run was produced. The upstream source and
selected fixture/tool dependencies did not change from the predecessor's accepted research base.

Load `local/private-inputs.ps1` in the same PowerShell process as any tool. `$upstream` below denotes
the verified read-only pinned source checkout, not a public machine path. The accepted predecessor
owns the completed PASS receipts and exact output identities for:

```powershell
. ./local/private-inputs.ps1
uv run sf2 rom verify
uv run sf2 h2 map-init --upstream-path $upstream
uv run sf2 h2 map-script-engine --upstream-path $upstream
```

Those commands were not rerun in this slice. The retained `map-init-static.json` was compared directly
with the complete fixture expected object, and retained `map-script-engine-static.json` was checked
by `sf2tool.h2.map_script_engine._verify_map_script_engine_fixture`; both comparisons passed. The
other narrow commands in the gap table identify existing reproduction/extension owners, not new
run claims. Direct source inspection supports the newly composed conditional conclusions.

Before freeze, review this complete prose, source/fixture references, links, exact single tracked
path and private boundary; run `git diff --check` and committed
`uv run sf2 verify plan --base origin/main --head HEAD`. Record exact Git identities, planner result,
actual CI and stopped-writer handoff on Issue/PR. No normal/full/engine/H3/H4 run is selected for this
document-only change. Preserve prior results and environments. Main-gate retains acceptance and merge.
