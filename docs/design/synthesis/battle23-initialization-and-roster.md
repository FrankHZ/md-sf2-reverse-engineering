# Battle23 Initial Formation and Suspended Entry

**Confirmed:** fresh Battle23 entry heals eligible allies and builds a formation
from the supplied force, alongside Willard, five Rats and six Bubbling Oozes.
The twelve ally placement slots do not promise twelve living heroes or a named
party. Suspended entry instead loads current state without that fresh setup.
**Inferred:** the named enemy formation stages the burrow encounter.
**Unknown:** natural admission, actual supplied state, completed services,
visible/audible presentation, usable control and persistence.

This English guide continues [Willard Burrow admission](willard-burrow-through-battle23-entry.md)
from its BattleLoop call, using [accepted Battle23 research](../../research/battle23-initialization-and-roster.md)
under [#938](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/938)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
It explains original configuration and conditional consumers. Fresh entry ends
at `BattleLoop:@Start`, **before ActivateEnemies**, region activation and initial
round creation. Suspended entry ends at `@IndividualTurns_Loop`, before reading
or executing an individual turn. Fnnn denotes flag nnn; coordinates, stats and
AI labels below are source data or initializer results, not observed play.

## Admission and Fresh Setup

**Confirmed:** Map27 selects Battle23 only with battle-unlocked F423 set.
Its sole row uses wildcard trigger X/Y, with no crop-containment test, and sets
area origin `(23,14)`, size 27x16. Completed-battle F523 clears F423 when set
but still admits this already selected call. Exploration-scene F781 and
battle-intro F473 remain independent. Supplied F423 can bypass the exploration
confrontation entirely. The predecessor's low-byte coordinate override limit
still applies: wildcard admission does not repair an abnormal high byte.

BattleLoop first clears PLAYER_TYPE and sets F399, then checks suspended F88.
Clearing PLAYER_TYPE does not establish that every retained raft field was erased.
With F88 clear, the conditional sequence is:

1. Clear the seconds counter, save incoming words and assign current map 27 /
   battle 23. Install base VInts and execute the before wrapper.
2. Restore the incoming words and reassign current map/battle. Clear region
   flags 90–105, including F90 for the one declared region.
3. Heal living/immortal allies, initialize ally positions, initialize enemy
   positions and clear AI memory, in that order.
4. Supply combatant 0 to LoadBattle. After it returns, execute the start wrapper.
5. Reach `@Start`; this guide stops before its ActivateEnemies call.

The before wrapper precedes healing and both rosters. An earlier exploration
population or current-map value does not choose the fresh roster after these
assignments. Each later step requires prior services to return; completed
prefix effects have no rollback if a later service does not finish.

### Empty Hooks Still Have Effects

Both complete 48-word tables select `ms_Empty` at Battle23 slot 23, byte offset 46.
Its whole body is the `$FFFF` script terminator. There is no local text,
movement, choice, recruitment or load command in either selected hook.

| Supplied F473 | Before wrapper | Start wrapper |
| --- | --- | --- |
| Clear | Dispatch the empty script without writing F473 | Set F473 **before** dispatching the empty script |
| Set | Skip dispatch | Skip flag write and dispatch |

Skipping both hooks does not skip fresh reset, healing, formation or load.
F473 set proves no previously played intro. Even an empty dispatch initializes
script state, saves registers and clears the text-skip byte; shared debug/input
handling can set it again. The tail conditionally waits for scrolling when a
dialogue window is present, clears scrolling speed, restores registers and
returns. A wait can prevent completion, including after the start wrapper has
already set F473. The [research hook join](../../research/battle23-initialization-and-roster.md#empty-hooks-still-have-wrapper-effects)
owns the table, body and complete wrapper provenance.

## Which Allies Receive the Twelve Slots

**Confirmed:** the spriteset header declares 12 allies, 12 enemies, one region
and one point. Its ally entries provide these battle-local coordinates:

| Placement index | X/Y | Placement index | X/Y |
| --- | --- | --- | --- |
| 0 | `(4,5)` | 6 | `(2,6)` |
| 1 | `(4,4)` | 7 | `(4,6)` |
| 2 | `(3,5)` | 8 | `(4,3)` |
| 3 | `(3,4)` | 9 | `(2,7)` |
| 4 | `(3,6)` | 10 | `(3,8)` |
| 5 | `(3,7)` | 11 | `(3,3)` |

Every complete ally record also declares HEALER1=0, item NOTHING=`$007F`,
primary and secondary NONE=`$FF` with region 15, filler 0 and STARTING=0.
The ally initializer copies **only X/Y**, not that row's index, item or AI fields.
Placement index 0 therefore does not itself mean Bowie or recruit any character.

Before placement, healing scans all thirty allies independently of active
membership. Living allies and always Peter (7)/Lemon (28) receive maximum current
HP/MP. Other dead allies are skipped. The helper masks status to existing
STUN/POISON/CURSE bits (`$0007`), then rebuilds current stats, equipment effects
and curse. The mask is an intermediate step, not a promise that every final
status bit remains unchanged. Class, inventory and joined flags are supplied
inputs; reserves are neither automatically enlisted nor made active by healing.

The position initializer first sets all thirty allies' X=-1. UpdateForce
rebuilds joined/active lists in ascending ally-ID order, replacing stale list
order. A living listed member receives the next unconsumed coordinate pair.
A dead listed member consumes a processed-member slot **without advancing the
placement pointer**. Later living members therefore take earlier unused pairs.
The limit is twelve processed members, with fewer living actors possible.
Surplus members leave the active party and keep X=-1; LeaveBattleParty does not
immediately rebuild the list/count. Fewer eligible members yield fewer placements.
A supplied zero-length list has no guard before its first entry read, so arbitrary
inconsistent empty state is not a proven safe deployment.

The [complete formation and party join](../../research/battle23-initialization-and-roster.md#complete-spriteset-and-ally-formation)
retains the 306-byte spriteset, record decoder and actual membership/healing
consumers. It supports a state-dependent party, not a fixed character sequence.

## Willard and the Complete Enemy Formation

**Confirmed:** the twelve enemy rows become logical combatants 128–139.
All share item NOTHING=`$007F`, secondary order NONE=`$FF` / region 15,
filler 0 and STARTING=0. The table supplies every differing record field;
shared fields apply to every row.

| Row / combatant | Type (ID) | Local X/Y | Commandset (value) | Primary order (byte) | Region |
| --- | --- | --- | --- | --- | --- |
| 0 /128 | WILLARD (90) | `(18,7)` | LEADER (14) | MOVE_TO\|0 (`$40`) | 15 |
| 1 /129 | RAT (15) | `(20,6)` | ATTACKER3 (9) | FOLLOW_ENEMY\|0 (`$80`) | 15 |
| 2 /130 | RAT (15) | `(20,8)` | ATTACKER3 (9) | FOLLOW_ENEMY\|0 (`$80`) | 15 |
| 3 /131 | BUBBLING_OOZE (16) | `(17,1)` | ATTACKER2 (7) | FOLLOW_TARGET\|0 (`$00`) | 0 |
| 4 /132 | RAT (15) | `(16,2)` | ATTACKER1 (6) | FOLLOW_TARGET\|0 (`$00`) | 0 |
| 5 /133 | BUBBLING_OOZE (16) | `(15,2)` | ATTACKER1 (6) | FOLLOW_TARGET\|0 (`$00`) | 0 |
| 6 /134 | RAT (15) | `(21,12)` | ATTACKER2 (7) | FOLLOW_TARGET\|0 (`$00`) | 0 |
| 7 /135 | BUBBLING_OOZE (16) | `(22,13)` | ATTACKER1 (6) | FOLLOW_TARGET\|0 (`$00`) | 0 |
| 8 /136 | BUBBLING_OOZE (16) | `(20,14)` | ATTACKER2 (7) | NONE (`$FF`) | 0 |
| 9 /137 | RAT (15) | `(10,11)` | ATTACKER1 (6) | NONE (`$FF`) | 15 |
| 10 /138 | BUBBLING_OOZE (16) | `(9,10)` | ATTACKER3 (9) | FOLLOW_ENEMY\|9 (`$89`) | 15 |
| 11 /139 | BUBBLING_OOZE (16) | `(5,12)` | ATTACKER1 (6) | NONE (`$FF`) | 15 |

The initializer scans all 32 enemy slots. These twelve STARTING rows initialize
from their selected definitions; absent slots 140–159 receive activation 0,
maximum/current HP 0 and X=-1. The absent path does not clear every field and
still reads trigger bytes; it establishes no extra valid row. Battle32's Jaro
exception does not apply.

For each present row, initialization copies the 56-byte definition, restores
current HP/MP from maxima, assigns position/type, packs movetype above commandset,
and writes trigger regions, orders and item effects. Trigger bytes are `$0F`
for rows 3–8 and `$FF` otherwise. Order words are `$40FF` for row 0, `$80FF` for
rows 1/2, `$00FF` for rows 3–7, `$FFFF` for rows 8/9/11 and `$89FF` for row 10.
Willard's activation word becomes `$0000`; Rats/Oozes become `$2000`, retaining
priority bits but discarding the copied BIT5. Primary/secondary-active and
neutral bits are clear, with initialization mode 0, before ActivateEnemies.
Neither region 15 nor a commandset label means that an enemy is already active.

### Definitions, Equipment and Initialized Stats

All three definitions start with status NONE and four NOTHING item slots, so
there is no equipped weapon. All row item words are the exact NOTHING sentinel;
InitializeEnemyItems returns before AddItem and leaves the copied inventory
intact. NOTHING is not a general inventory-erasure operation.

| Definition | HP/MP maxima and fresh current | Base ATT / DEF / raw AGI / MOV | Movetype | Four spell slots |
| --- | --- | --- | --- | --- |
| WILLARD (90) | 50/0 | 50 /36 /165 /6 | STEALTH=3 | NOTHING, NOTHING, NOTHING, NOTHING |
| RAT (15) | 41/0 | 45 /32 /30 /6 | STEALTH=3 | NOTHING, NOTHING, NOTHING, NOTHING |
| BUBBLING_OOZE (16) | 32/0 | 38 /35 /26 /4 | REGULAR=1 | AQUA\|LV2 (`$68`), NOTHING, NOTHING, NOTHING |

Spell NOTHING is `$3F`; AQUA=`$28` combined with LV2=`$40` gives `$68`.
The Ooze's declared spell despite zero MP proves no later AI choice or cost.
Willard's raw AGI 165 is copied data, not displayed agility or a turn-count result.
Definitions also supply level, resistance, prowess and spell-power fields;
their later action/damage interpretation lies beyond this entry boundary.

Difficulty adjustment then derived-stat rebuilding follow the activation write.
The accepted consecutive-branch analysis means **all difficulties** first apply
floor(baseATT*5/4); SUPER (2) applies it again. Willard/Rat/Ooze therefore receive
initialized base ATT 62/56/47 on NORMAL (0), HARD (1) or OUCH (3), and 77/70/58 on SUPER (2).
These are initialized attack values, not damage. UpdateCombatantStats rebuilds
current stats from base/status/equipment; the selected definitions introduce no
initial status or equipped-item effect. See the [definition/difficulty join](../../research/battle23-initialization-and-roster.md#definitions-items-spells-and-difficulty)
for the complete selected definitions and consumers.

### One Region and One Point Do Not Establish Tactics

The single type 4 region has vertices `(9,0),(9,15),(26,15),(26,0)`.
Its second byte and trailing bytes are zero; the selected geometry consumer
reads type and packed vertices, using two triangles, without reading those
other bytes. Point 0 is `(22,3)`. All coordinates are battle-local.

Fresh entry clears F90. A later region consumer associates region 0 with F90
and writes activation bits separately from the stored trigger-region bytes.
Region 15 is the NONE sentinel, not fifteen polygons. Clearing flags 90–105
does not itself assert that every AI scratch field was reset.
MOVE_TO|0 selects point 0 through the low-nibble point index; FOLLOW_ENEMY|9
encodes combatant 137 (and FOLLOW_ENEMY|0 encodes 128). FOLLOW_TARGET|0 retains target operand 0. These are
consumer relationships, not a chosen route, reached point, target or attack.
No declared region is proven triggered at this guide's stopping boundary.

The complete random-battle table excludes Battle23, so completed F523 does not
upgrade these types. The complete neutral table also has no Battle23 group:
F523 set skips scanning, while clear scans through without a match. Neither
adds a fresh neutral or repurposes 158/159. The [record/activation join](../../research/battle23-initialization-and-roster.md#twelve-enemy-records-and-initialization)
and [region/point join](../../research/battle23-initialization-and-roster.md#region-0-and-point-0-are-inputs-not-fired-triggers)
retain the full bytes and exclusion evidence. No tactical intent is inferred
from LEADER, ATTACKER, STEALTH or the formation's arrangement.

## Loading the Battlefield Rebuilds Actor Identity

**Confirmed:** LoadBattle fades out, disables view tracking, loads tilesets,
waits for fade/VInt work and rebuilds entities. That pass clears 49 physical
records and 64 lookup bytes, then scans all 30 ally and 32 enemy slots.
Nonnegative X/Y admits a record; neutral-marked enemies are excluded. There is
**no additional HP check** here. Fresh initialization supplies positions;
arbitrary saved positions need not have the same relationship to HP.

Ordinary actors use masked local coordinates `(X & $3F)*384`, `(Y & $3F)*384`,
Down (3) and eas_Standing. Ally mapsprites use current class-sensitive mapping,
including Rohde's membership exception; battle mode skips the exploration-only
dead-ally BLUE_FLAME branch. Enemy types select WILLARD (154), RAT (79) and
BUBBLING_OOZE (80). These are declared sprite IDs, not verified rendered sprites.

With N declared allies, fresh enemy row r receives physical slot N+r. Twelve
allies would put enemies at physical 12–23; N need not be twelve. Logical
combatants 128–139 use lookup 32–43; skipped ordinary slots have lookup -1.
Willard's type 90, mapsprite 154, logical 128 and physical N are distinct identities.
Exploration's logical 128 Rat /129 Willard /130 Rat and follower/raft mappings
are replaced, not continued into those battle identities.

Generic script aliases A30/B31 and C158/D159 are not additional characters.
Here lookup 30/31 remain zero after clearing, while absent enemy lookups 62/63
are -1. Inherited D/raft63 alias rules do not preserve a raft actor or make an
invalid lookup safe. Nor are Chessboard's Ponpei/Maid or exploration residents
imported as Battle23 neutrals. On supplied resume, a neutral-marked slot stays
excluded from the ordinary pass; absence of a neutral-table match does not
repair that combatant. Extra nonneutral slots with valid positions can declare
actors even if they are outside the fresh twelve rows.

### Formation Coordinates Already Belong to the Crop

Fresh LoadBattle receives combatant 0 for view selection. It resolves the rebuilt
lookup and reads that entity's coordinates; a negative lookup falls back to
physical 0 for the reads, without placing Bowie or repairing membership.
It then supplies d0=-1 for target-based view placement to LoadMap.
The separate d1 map operand selects full load versus reload: preserved current
Map27 is nonnegative and takes full load, rather than reusing the working layout
through the earlier negative-d1 reloadMap route.

Full load applies map flag copies/chest overlays and crops source origin
`(23,14)`, size 27x16 into working origin `(0,0)`. The battle layer1 extent ends
at `(26,15)*384`; other layer operands remain map-area data. Formation, enemies,
region and point are already local; their helpers do not subtract 23/14 again.
Placement 0 `(4,5)` corresponds to source Map27 `(27,19)`, Willard `(18,7)` to
`(41,21)`, and point 0 `(22,3)` to `(45,17)`. These are crop correspondences,
not observed movement from the incoming `(0,0)` warp or proof of terrain legality.
LoadBattle does not rerun CheckBattle; supplied area bytes can change the crop.

After mapsprites, base VInts and terrain, LoadBattle submits map music and fades
in. Battle23 takes no Battle44 timer-window branch. Only its return permits
the start wrapper and the fresh stop. The [load/entity/crop join](../../research/battle23-initialization-and-roster.md#battle-load-entities-and-coordinate-domains)
separates physical declarations, view arguments and full-load geometry.
Returning calls alone prove neither exact visible timing nor usable control.

## Suspended Entry Uses Supplied Current State

**Confirmed:** with F88 set, after PLAYER_TYPE clear/F399 the branch restores
saved seconds, clears F88, clears AI memory, clears the view target, loads the
battle and goes directly to `@IndividualTurns_Loop`. The following branch to
`@Start` is unreachable. Both fresh and saved AI clears reset 48 last-target
bytes to `$FF` and 48 memory bytes to 0; that creates no fresh turn order.

| Setup or premise | Fresh F88 clear | Suspended F88 set |
| --- | --- | --- |
| Current map/battle | Assign incoming 27/23 before and after before wrapper | Retain actual current values; incoming 27/23 does not replace them |
| Seconds | Clear | Restore saved seconds |
| Region flags, healing and formations | Clear flags 90–105; heal; initialize both sides | Bypass these fresh operations |
| Hooks | Conditional before/start dispatch through F473 | Neither wrapper runs |
| View/load | Explicit combatant 0; rebuilt entity lookup | No explicit d0 clear; supplied argument and current-state load |
| Next boundary | Before ActivateEnemies and initial rounds | Before reading/executing the saved individual turn |

HP/MP/status/items/class, membership, positions/activation, area, map/battle and
saved turn/order remain supplied premises. A changed CURRENT_BATTLE may choose
another battle's data/neutral population despite incoming 27/23. LoadBattle
rebuilds physical actors and loads actual current map/terrain; its view argument
does not select that map. No fresh-health, fixed actor-count, automatic repair
or cross-process persistence guarantee follows. Even the saved-order sentinel
branch lies after this guide's stop. The [saved-entry research](../../research/battle23-initialization-and-roster.md#suspendedcurrent-state-entry-and-changed-premises)
owns these changed-state cases, not a played resume demonstration.

## Evidence Review and Retained Limits

**Confirmed provenance:** accepted main `424b8eaeb2f467f3f714d39b73d255f2a3c9dd27`
contains #932/PR935 and #934/PR936. The research pins
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Its [reproduction owner](../../research/battle23-initialization-and-roster.md#reproduction-retained-results-and-limits)
retains exact symbols/addresses, complete selected records and source joins.
This guide adds no original observation or extraction.

Adversarial review reads the whole accepted research and predecessor, comparing
all placement/enemy rows, shared fields, quantities and lifecycle distinctions.
The selected public fixture/schema/verifier boundaries are:

| Executable owner | Selected claim and limit |
| --- | --- |
| `sf2-battle-loop-static-v1`, tests/fixtures/h2/battle-loop-static-v1.json | rosters 30/32, healing/mask/derived stats and AI clear 48; matching h2-battle-loop-static-fixture schema and battle_loop.py `_build_lifecycle_facts`. It does not identify the actual supplied party. |
| `sf2-battle-control-static-v1`, tests/fixtures/h2/battle-control-static-v1.json | F88/seconds, flags 90–105, 12-byte combatant/region strides and ordered source fragments; matching control fixture schema and battle_control.py `_build_control_facts`. It is not a natural fresh/resume observation. |
| `sf2-battle-cutscenes-static-v1`, tests/fixtures/h2/battle-cutscenes-static-v1.json | Shared intro gate and start flag-before-dispatch; matching cutscenes fixture schema and battle_cutscenes.py `_cutscene_facts`. Research owns the complete slot 23/terminator/wrapper join. |
| `sf2-battle-routing-data-static-v1`, tests/fixtures/h2/battle-routing-data-static-v1.json | Shared table topology/provenance; matching routing fixture schema and battle_routing_data.py. routeTargetsParsed=false does not verify slot 23 semantics. |
| `sf2-battle-spriteset-data-static-v1`, tests/fixtures/h2/battle-spriteset-data-static-v1.json | H1 address/header aggregate provenance; matching spriteset fixture schema and battle_spriteset_data.py. rowContentTracked=false does not validate these 24 record bodies or their meanings. |

The schemas live in `schemas/`; verifiers in `src/sf2tool/h2/`. Existing narrow
routes are `uv run sf2 h2 battle-loop`, `battle-control`, `battle-cutscenes`,
`battle-routing-data` and `battle-spriteset-data` with the same command prefix.
Aggregate equality is not a prose or runtime proof. Retained research directly
joins the full party/heal/initializer/stat/difficulty/load/entity/crop consumers,
all selected records, one region/point and complete random/neutral tables.
The design checkout lacks the pinned upstream/retained payload pair; accepted
source/comparison results are retained, not claimed as fresh reruns or regenerated.

Preserve [#932 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/932#issuecomment-6176988463)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/935#issuecomment-6177066791),
including pair 0.081s, CI38626848120, completed diagnostics and predecessor
corrections; [#934 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/934#issuecomment-6177126852)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/936#issuecomment-6177145045)
retain CI38627848438, absent local inputs, #928 results and their failures.
Direct document/diff, numeric/order/table/link/English/private checks and
`git diff --check` apply, followed by the configured clean committed planner
and actual final-head CI. No blanket local suite is selected solely for prose.

**Unknown:** natural admission; actual flags, party/HP/class/items, mode/aliases,
positions/activation, current/saved area/map/battle and turn/order; completed
script/load/fade/VInt services, visible/audible roster/presentation, usable
control and persistence. Neither this guide nor its configuration proves
executed tactics, displayed agility, outcomes, leader death, postbattle or Creed
progression. The separate [outcome and burrow-return guide](battle23-outcomes-and-burrow-return.md)
continues at the post-action check using corrected accepted #937; it does not
fill the unobserved turns between these boundaries. These gaps remain
bounded under [ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../decisions/0016-remake-start-evidence-deferral.md), without an
automatic runtime queue, new remake acceptance or full-campaign completion claim.
