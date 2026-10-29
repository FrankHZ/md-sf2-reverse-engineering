# Harpy Pond approach through Battle20 entry

Evidence date: 2026-10-23. **Confirmed:** the selected Map51 approach runs the
confrontation, sets F420 inside it, publishes a same-map warp, and sets F470 only
after the script returns. The separate exit warning runs only while F470 is
clear. Ordinary consumption of the retained confrontation request admits
Battle20 before another exploration setup. Its actual before/start table entries
are empty; fresh entry rebuilds the combat roster and conditionally adds a
neutral Elric. **Inferred:** the confrontation introduces a fight over the
trapped elf. **Unknown:** natural reach, supplied caller state, completed
presentation/services, responsive input and persistence. No rescue or recruitment
is established by this entry sequence.

## Agreement and provenance

Standard rigor under [Issue #868](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/868)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
this bounded static account supports the content guide. Accepted starting base
is `447faea443970dcc83c390d4e4f7cce43017d38b`. Sole ownership is this document;
there are no shared edits, stacked dependencies, new fixtures or machinery.
Consume accepted [pond entry/population](./elven-village-departure-through-harpy-pond-entry.md),
[request writes](../../world/map-event-request-state.md),
[request consumption](../../world/map-event-request-consumption.md),
[battle routing](../../battle/battle-cutscenes.md), [lifecycle](../../battle/battle-loop.md) and
[spriteset inventory](../../battle/battle-spriteset-data.md).

The [initialization/SWARM/first-control account](./battle20-initialization-and-first-control.md)
owns the subsequent activation/count/order/dispatch boundary and complete definitions.
This entry account stops at fresh BattleLoop's `@Start` label, before ActivateEnemies, or at the F88
saved-entry branch before individual turns. Stop an unrelated request/admission
at its own boundary. Exclude activation, region/spawn execution, turn generation,
individual turns, tactics, damage, outcomes, after/defeated scenes, Elric rescue
or joining, F751 passage, later travel, terrain reconstruction and runtime
acquisition. Material coupling beyond this agreement requires replanning.

All source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only source checkout is clean at that pin. Retained payloads bind that
revision and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no fresh ROM extraction or runtime observation is claimed. Short map paths start
at `data/maps/entries/`. Fnnn means flag nnn; command, row and slot indices are
zero-based. Positions, wait counts and AI operands describe source inputs, not
observed movement, elapsed time, presentation or decisions.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine pair, existing
fixtures and bounded selected source. Linear comparisons were budgeted below
512 MiB and one minute, with small receipts/prose and no regeneration; they
passed in 0.081 seconds. Reassess before material resource or scope growth.

## Zone selection and the current pond population

**Confirmed:** both `map51/mapsetups/pointertable.asm` and `pointertable_520.asm`
select the same entity-event, zone-event and init functions. F520 selects the
empty fixed-entity alternate; it does not disable those callbacks. The complete
`s3_zoneevents.asm` (`0x5C2D6..0x5C3C0`) orders:

| Rows | Supplied coordinate | Target and condition |
| --- | --- | --- |
| 0–3 | `(9,6)`, `(9,7)`, `(9,8)`, `(9,9)` | `Map51_ZoneEvent0`: F470 set returns; clear executes `cs_5C312`, then sets F470. |
| 4–5 | `(9,24)`, `(10,24)` | `Map51_ZoneEvent4`: F470 set returns through default; clear executes `cs_5C3AA`, then returns. |
| Default | Any other coordinate | `Map51_DefaultZoneEvent`: `rts` only. |

The complete `s2_entityevents.asm` likewise has only a default target containing
`rts`. These return-only targets are bounded context, not new interaction/service
coverage. Neither selected zone tests F420, F520, F13, HP or party membership.

Ordinary walking's `$1400` branch at
`code/common/scripting/entity/entityscriptengine_2.asm:loc_5294` writes pending
event 6 and word X/Y from destination plus candidate displacement divided by
384. Its `$1000` branch calls WarpIfSetAtPoint instead. Working-layout and earlier
movement checks decide whether either producer runs; coordinate tables alone
do not prove terrain access or that these zones form an unavoidable barrier.

`code/gameflow/exploration/explorationfunctions_2.asm:ProcessMapEvent` clears
pending before dispatch. Type 6 applies Init to physical 0 and passes the X/Y
words to `RunMapSetupZoneEvent` in
`code/common/scripting/map/mapsetupsfunctions_1.asm`. That wrapper resolves the
current setup and scans rows in order (255 wildcard, `$FD` default), calls the
selected target, closes portrait/text, waits one VInt and waits for physical 0
position to equal destination. This is not an all-actor completion barrier.

The accepted fresh default population has Elric at `(6,6)`, Down, with
`eas_WhirlsInWater`, plus Harpy, Soulsower and Soulsower at `(63,63)`, Up, with
`eas_Init`. `mapfunctions.asm:InitializeMapEntities` maps the ally row to logical
Elric 13; the three ordinary non-ally sprites consume logical 128–130. Physical
indices depend on preceding followers. For the fresh normal-table example with
only F67/F68 selecting followers, Peter 7 / B31 occupy physical 1/2, Elric 13 occupies 3,
and 128/129/130 occupy 4/5/6; Bowie 0 is physical 0. F65 alone adds no Caravan on
Map51. Supplied later follower flags can change or overwrite bindings; a fixed
ally row can be skipped when that ally lookup is already populated.

The scene uses those **current logical lookups**. It neither rebuilds actors nor
checks that B is Oddler, Peter is present, or Elric belongs to the force.
F520-set fresh setup omits all four fixed actors, while flag-selected followers
can still exist. Changing a flag after population does not itself clear or
create an actor. The accepted initial prompt and its following actions remain
separate; approach Init replaces Peter's and B's current action scripts.

## Complete approach program

**Confirmed:** `cs_5C312` at `0x5C312`, source lines 45–75, has 29 top-level records.
The retained program and complete source body agree, including the inline move.
B below is FOLLOWER_B (31); 128–130 here are exploration logical IDs.

| Records | Ordered operations |
| --- | --- |
| 0–5 | Cursor 2555; wait Peter Init, then B Init; position 128 at `(7,16)`, 129 `(6,17)`, 130 `(8,17)`, all Up. These position writes relocate the parked actors; they do not allocate them. |
| 6–10 | Elric nextText 2555, then single 2556; 128 single 2557; start B Jump, then start/wait Peter Jump. Waiting for Peter does not independently join B. |
| 11–15 | csWait 5; start Peter 2xRightLeft; csWait 120; Peter single 2558; camera destination `(3,12)` and scroll wait. |
| 16–20 | 128 nextText 2559, then single 2560; Elric single 2561; 128 single 2562; 128 inline moveUp 2/endActions, waiting for idle. |
| 21–25 | 128 single 2563; face 128 Down; 128 single 2564; start 129 Jump, then start/wait 130 Jump. No independent wait joins 129. |
| 26–28 | `setStoryFlag 20`; warp Map51 `(0,0)`, Right; end. Only after ExecuteMapScript returns does the zone callback set F470. |

Complete dialogue joins use `data/scripting/text/gamescript.txt`. This table
paraphrases the literal text and retains its controls; 2555/2559 use nextText
and all others use nextSingleText. `$C0` selects mirrored/right portrait bits;
`$0` selects neither. Speaker identity still follows the mapped mapsprite.

| Decimal / hex key | Speaker / modifier | Content and controls |
| --- | --- | --- |
| 2555 / `09FB` | Elric / `$0` | Asks if they came to help and thanks them; `{N}`, `{W2}`. |
| 2556 / `09FC` | Elric / `$0` | Says monsters might have eaten him without their arrival; `{N}`, `{W1}`. |
| 2557 / `09FD` | 128 / `$C0` | Exclaims that its dinner is escaping; `{W1}`. |
| 2558 / `09FE` | Peter / `$0` | Asks who spoke; `{W1}`. |
| 2559 / `09FF` | 128 / `$C0` | Describes the pond as a food trap and addresses the elf; `{N}`, `{W2}`. |
| 2560 / `0A00` | 128 / `$C0` | Claims ownership of anyone stuck in the pond; two `{N}`, `{W1}`. |
| 2561 / `0A01` | Elric / `$0` | Asks whether it ate the missing people; `{N}`, `{W1}`. |
| 2562 / `0A02` | 128 / `$C0` | Replies that they were in its pond; `{W1}`. |
| 2563 / `0A03` | 128 / `$C0` | Claims the elf as tonight's dinner; `{N}`, `{W1}`. |
| 2564 / `0A04` | 128 / `$C0` | Asks the others about ketchup; `{N}`, `{W1}`. |

Under consistent mappings and completed movement, 128 moves from `(7,16)` to
`(7,14)` before the explicit Down facing. No approach command relocates Elric
from the pond, joins him, writes F13/F520/F751, grants an item, or asserts a
completed rescue. Dialogue about escape or missing people is attributed speech,
not independently confirmed history or a recruitment operation.

## Immediate command and partial-execution limits

**Confirmed:** `code/common/scripting/map/mapscriptengine_1.asm` and `_2.asm`,
`sf2cutscenemacros.asm`, `data/scripting/entity/eas_actions.asm` and the immediate
entity consumers establish the selected operations:

- `csc15` maps the byte ID, replaces action/timer and optionally polls for the
  exact `eas_Idle` pointer. Init zeroes acceleration factors, sets speed 32/32,
  disables acceleration/deceleration, enables obstructability/collision and
  autofacing, orients Up, disables immersion, sets size 24, refreshes and idles.
  Jump disables autofacing, applies relative motion `(0,-1)`, wait 5, `(0,1)`,
  wait 5, `(0,0)`, restores autofacing and idles. 2xRightLeft refreshes and waits 30
  after each Right/Left/Right/Left facing, then idles. A separate csWait 120 is
  not an action-pointer join; later writes may replace unfinished actions.
- `csc19` writes current and destination coordinates scaled by 384 plus facing,
  then refreshes; `csc23` writes facing and refreshes. Their alive helper skips
  HP-zero ally IDs 0–29, but bypasses that test for enemy-bit IDs 128–130. The
  approach's three positions and facing 128 therefore have no HP gate. They do
  not validate the lookup or declare missing entities.
- `csc2D` clears action flags 5/6 and builds relative move opcode 4, destination
  wait opcode 1 and a final jump to idle. The selected inline moves have the
  idle-wait modifier. `entityscriptengine_2.asm:esc04_moveToRelativeDest` uses
  current coordinates plus signed tiles and the actor's current speed. Its
  destination-conflict helper returns immediately with bit 5 clear; this path
  bypasses ordinary walking's layout/zone/warp producer. Opcode 1 waits for both
  coordinates to equal destination. Thus a scripted Up move does not itself
  submit another zone merely because it crosses one of the listed coordinates.
- `csc32` disables entity camera following and sets/waits for the camera tile
  destination. The approach does not restore a camera entity before its warp.
  Text waits for scrolling, resolves portrait/speech from the current actor's
  mapsprite, displays/increments the cursor; single text additionally closes
  portrait/text and sleeps 10. NextText leaves that closure to continuation.
  `{N}`, `{W1}`, `{W2}` and `{LEADER}` are text controls/substitution, not a new
  choice. Neither program contains a YesNoPrompt or a consent result.
- Script entry clears SKIP_CUTSCENE_TEXT. Debug mode plus player 2 Start can set
  it; selected text then skips display and cursor advancement, and csWait skips
  Sleep. Ordinary csWait passes its low byte to Sleep. End conditionally waits
  for view scrolling when the dialogue-window index is nonzero, clears camera
  speed and restores registers. These are service dependencies, not measured
  cadence or proof of an all-actor completed scene.

`GetEntityAddressFromCharacter` masks/maps the logical ID without checking force
membership or presence. `entityfunctions_1.asm:ClearEntities` zeroes unassigned
lookups. Missing Peter, B, Elric or NPC 128–130 can therefore alias physical 0;
a lookup value 255 is not a safe absence sentinel for this command consumer.
HP-zero Peter/Elric does not suppress their selected action/text records; the
warning's Bowie nod/inline action also has no HP gate. Population's sprite choice
can differ with supplied class/HP. No command repairs that state. With F520 set
and F470 clear, the same approach can act on missing or independently supplied
actors; it is not automatically skipped, made safe or changed into post-battle
content. Earlier writes persist if later action/text/scroll work never returns;
there is no rollback. These are static counterexamples, not observed failures
or a claim that such callers occur naturally.

## Separate exit warning

**Confirmed:** `cs_5C3AA` (`0x5C3AA`), lines 76–82, contains five top-level records:
cursor 2565; Peter single text 2565 (`0A05`, `$0`), addressing `{LEADER}` and urging
that they save the elf, with `{N}`/`{W1}`; nod Bowie; Bowie inline moveUp 1 followed
by endActions with an idle wait; end. It writes no story flag, pending warp,
force membership, controller mode or recruitment state. Its caller tests only
F470, and does not set it. A later admitted F470-clear invocation can repeat.

`csc26_entityNodHead` maps Bowie, stops animation, sleeps 10, loads/transforms/DMAs
the mapsprite, sleeps 20, refreshes, sleeps 10 and restores animation. It has no HP
or consent test. Type 6's earlier Init applies to physical 0, so with the ordinary
Bowie 0 mapping and completed services the inline movement uses its initialized
speed and moves one tile Up from its then-current position. A zone parameter
Y24 is not itself proof of that actual starting position, nor a new absolute
Y23 placement. The `{LEADER}` substitution reads the first current battle-party
member after UpdateForce, rather than being an unconditional literal Bowie name.

The complete external `map51/6-warp-events.asm` is a **different producer**:
any-X/Y25 requests no-scroll Map67 `(57,26)`, Down. WarpIfSetAtPoint writes
WARP_SFX and has no F470 test. Its no-scroll return would scan Map67's row 21
(F421/F521); stop at that unrelated continuation. The warning's two Y24 cells
therefore do not prove a universal exit lock, every traversable exit route, or
natural movement between the warning and external warp.

The accepted direct entrance from Map67 requests `(10,24)`, Up; the earlier
scripted call for help requests `(11,22)`, Up. Arrival at `(10,24)` does not itself
call this zone: initial setup/init and walking's zone producer are distinct.
The shared F470-clear initial prompt can move the arriving cast before any
subsequent walking event. Neither entry tuple proves the exit warning ran.

## Ordered request publication and Battle20 admission

**Confirmed:** `sf2cutscenemacros.asm:setStoryFlag` emits opcode `$13` and word 20.
`mapscriptengine_2.asm:csc13_setStoryFlag` adds BATTLE_UNLOCKED_FLAGS_START=400
and calls SetFlag: it writes **F420**, not F20 or F470. The next `csc07_warp`
writes pending type 1, parameter1=0 (no scroll), map51, X0, Y0 and Right. It does
not load a map, move the party, leave the script immediately or write WARP_SFX.
End returns before the callback's F470 write; the zone wrapper then closes
windows and waits for physical 0 movement. Thus the order is:

1. F420 set inside the confrontation.
2. Same-map warp published, followed by script termination/services.
3. F470 set after script return, followed by zone-wrapper services.
4. Exploration's SetMoveSfx/WaitForEvent sees pending; with it still nonzero,
   ordinary controller installation is bypassed.
5. ProcessMapEvent clears pending; its no-scroll path submits/clears the
   currently supplied WARP_SFX, idles physical 0, unwinds to MainLoop and returns
   `(map51,X0,Y0,Right,d4=0)`.
6. MainLoop runs SwitchMap, then CheckBattle, before fresh ExplorationLoop.

`data/maps/global/flagswitchedmaps.asm` has no Map51 substitution. The complete
45-row `data/battles/global/battlemapcoords.asm` has only row 20 for Map51:
area origin `(0,0)`, size 18x26, trigger 255/255. `code/common/maps/getbattle.asm`
requires F420 and treats those trigger bytes as wildcards; it does not test
rectangle containment, F470, F13, membership or HP. It writes the battle area,
checks F520, and if F520 is set clears F420 **without canceling this selection**.
MainLoop then supplies map51/battle20 to BattleLoop. The warp's `(0,0)`, Right
is an admission tuple, not a combatant placement or facing.

| Supplied state / change | Conditional source result |
| --- | --- |
| F470 clear, complete approach, request retained, F520 clear | F420 then F470 are set; ordinary return admits 20 and skips fresh pond setup/init. |
| Same completed approach with F520 set | Still admits 20; CheckBattle clears F420 after selection. Fresh battle omits its completion-gated neutral, while normal enemy rows remain selected. The earlier empty exploration population is not retroactively repaired. |
| F470 set, F420 clear | Approach and warning return without writes. No unlock is repaired. A later ordinary scan of Map51 finds no battle and may enter fresh pond exploration under its supplied F520/setup rules. |
| F420 supplied set before any approach, F470 either value | An ordinary Map51 scan can admit 20 directly, at either initial-entry tuple or another coordinate. F470 is not an admission prerequisite. |
| F420 cleared after the scene but before the scan | The retained Map51 warp alone does not admit battle. Fresh exploration uses the accepted population/init owner; a surviving F470 skips its initial prompt. |
| Pending request independently overwritten | The consumer uses the replacement; the earlier F420/F470 writes are not rolled back or retried automatically. A replacement event/scroll need not perform this MainLoop scan. Stop unrelated continuation. |
| Script or wrapper fails to return | No later write/consumer is guaranteed. In particular, F420/pending can already be written while the callback has not yet set F470. |

Ordinary fresh exploration, if selected instead, retains its existing heal,
setup, temporary-flag clear, init and conditional-controller order. It is not
interposed between this successful no-scroll admission and BattleLoop. Nothing
here establishes an intervening playable pause or a durable save.

## Actual before/start selection and fresh entry

**Confirmed:** slot 20 in both `data/battles/cutscenes/beforebattlecutscenes.asm`
and `battlestartcutscenes.asm` selects `ms_Empty` at `0x47A4E`, whose complete
body in `code/common/scripting/map/ms_empty.asm` is the `$FFFF` terminator.
There is no selected Battle20-specific before/start program to reconstruct from
a guessed filename.

The wrappers in `code/gameflow/battle/cutscenes/beforebattlecutscenesstart.asm`
and `battlestartcutscenesstart.asm` add battle 20 to intro base 450, selecting
**the same F470 used by the approach**. Before checks without setting; start
checks and, if clear, sets F470 before executing the selected empty script.
Their corresponding `*end.asm` files restore and return. A completed approach
therefore skips both scripts. Direct supplied F420/F470-clear admission instead
executes the empty before program and later sets F470 before the empty start
program; it does not replay the pond confrontation, reveal the three old NPCs
or run pond init. Empty means no scene records, not bypassed script-engine entry
and termination services.

`code/gameflow/battle/battleloop_1.asm:BattleLoop` first sets PLAYER_TYPE=0 and
F399, then tests F88. On the fresh F88-clear branch it:

1. Clears seconds, saves incoming map/battle, writes CURRENT_MAP=51 and
   CURRENT_BATTLE=20, installs base VInts and invokes the before wrapper.
2. Restores incoming map/battle, clears region flags 90–105, heals living allies
   and immortal Peter 7 / Lemon 28, then initializes ally and enemy positions.
3. Clears AI memory, calls LoadBattle with d0=0, then invokes the start wrapper.
4. Reaches `@Start`; this account stops **before** ActivateEnemies.

`battleloop/heallivingandimmortalallies.asm` restores eligible HP/MP maxima,
initially retains existing STUN/POISON/CURSE (`status & 7`), then updates stats.
That refresh preserves STUN/POISON and recomputes final CURSE from equipped
cursed items; a supplied CURSE bit alone does not survive. It does not revive
other dead allies or join anyone. Initial exploration HP/mapping
problems are not repaired before the earlier approach merely because fresh
battle will heal later. PLAYER_TYPE is normalized at BattleLoop, not by the
approach warp. No saved raft coordinate write is established by that mode clear.

F88 set takes a different branch after mode/F399: restore saved seconds, clear
F88 and AI memory, clear the view target, LoadBattle, then branch directly to
`@IndividualTurns_Loop`. It skips incoming map/battle assignment, before/start,
fresh healing/roster initialization and new turn-order generation. Unlike fresh
entry it does not explicitly clear d0 before LoadBattle. Saved current map,
battle, area, combatants and turn state must already agree; supplied incoming 20
does not repair them. Stop before that individual-turn continuation. The source's
following branch to `@Start` is unreachable on this path.

## Complete selected spriteset and roster consumers

**Confirmed:** slot 20 of `data/battles/spritesets/entries.asm` selects
`BattleSpriteset20` (`0x1B482C..0x1B4950`) in `spriteset20.asm`: twelve ally
placement records, twelve enemy records, zero AI regions and zero AI points.
The complete selected body was read, not inferred from the aggregate inventory.

Ally placements in row order 0–11 are `(6,8)`, `(7,8)`, `(5,8)`, `(6,9)`, `(8,8)`,
`(7,9)`, `(5,9)`, `(4,9)`, `(5,10)`, `(6,10)`, `(8,7)`, `(9,7)`. All carry
HEALER1/NOTHING, primary NONE/15, secondary NONE/15, filler 0 and STARTING.
These are placement-row inputs, not an imposed roster of named allies or new
ally AI/items. `battleloop/initializecombatants.asm:InitializeAllAlliesBattlePositions`
clears X for all 30 allies, calls `code/common/stats/battleparty.asm:UpdateForce`,
and consumes the current joined/active battle-party list and placement X/Y.
A dead member consumes a party/loop slot without advancing the placement row;
remaining members beyond capacity can leave the active party. No twelve-person
living force, Peter, Oddler or Elric membership follows from twelve rows.
The routine reads its first party entry before checking exhausted member count;
an empty/inconsistent list is not guaranteed safe empty initialization.

For enemies, `sf2macros.asm` encodes type/X/Y, commandset/item, primary order/
region, secondary order/region, filler and initialization. Every selected row
has item NOTHING (`$7F`), secondary NONE/15, primary region 15, filler 0 and
STARTING (0); the differing primary orders and commandsets are retained here:

| Row / combatant slot | Enemy type | X/Y | Commandset | Primary order |
| --- | --- | --- | --- | --- |
| 0 / 128 | HARPY | `(15,23)` | SWARM | NONE |
| 1 / 129 | DARK_MADAM | `(9,22)` | ATTACKER3 | FOLLOW_ENEMY\|3 |
| 2 / 130 | HIGH_PRIEST | `(10,21)` | HEALER3 | FOLLOW_ENEMY\|1 |
| 3 / 131 | ORC | `(9,19)` | ATTACKER1 | NONE |
| 4 / 132 | ARROW_LAUNCHER | `(4,20)` | ATTACKER3 | FOLLOW_ENEMY\|5 |
| 5 / 133 | ORC | `(5,19)` | ATTACKER1 | NONE |
| 6 / 134 | HARPY | `(1,16)` | SWARM | NONE |
| 7 / 135 | HIGH_PRIEST | `(6,15)` | HEALER3 | FOLLOW_ENEMY\|8 |
| 8 / 136 | DARK_MADAM | `(8,13)` | ATTACKER1 | NONE |
| 9 / 137 | HARPY | `(11,17)` | ATTACKER1 | NONE |
| 10 / 138 | HARPY | `(14,3)` | ATTACKER1 | NONE |
| 11 / 139 | HARPY | `(4,1)` | SWARM | NONE |

FOLLOW_ENEMY is `$80`, so those operands encode rows 3/1/5/8 respectively, distinct
from exploration NPC logical IDs. They are stored orders, not evidence that any
unit activates, follows, attacks or heals before the stopping boundary. Zero
region records and STARTING do not substitute for reading or executing the
later activation algorithm; that work is excluded.

`InitializeAllEnemiesBattlePositions` scans all 32 slots 128–159. The twelve
STARTING rows call InitializeEnemyStats; remaining slots take its absent/hidden
path with zero max/current HP and X=-1. InitializeEnemyStats copies the selected
enemy definition, fills current HP/MP, writes row position, combines definition
movetype with row commandset, stores type/trigger/order fields, handles the row
item, constructs activation bits, applies shared difficulty adjustment and
updates stats. NOTHING makes InitializeEnemyItems return without adding a row
item; it does **not** assert empty definition inventory or no equipment/spells.
No exact derived stats or combat effectiveness are claimed here.

`battleloop/upgradeenemies.asm:UpgradeRandomBattleEnemies` first asks
IsBattleUpgradable to scan `data/battles/global/randombattles.asm`.
The complete length-prefixed eleven-battle list excludes Battle20. It therefore
returns the original enemy type before upgrade selection, even with F520 set.
The encounter's type is not promoted by assuming every completed battle is an
upgradable random battle. Shared initialization still consumes its ordinary
stats/difficulty inputs.

### Rebuilt actors and the separate neutral Elric

**Confirmed:** `code/gameflow/battle/battlefunctions/loadBattle.asm:LoadBattle`
fades out, loads tilesets, clears VInts and calls
`code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities`.
That routine clears entity data/lookups, creates actors for combatants with
nonnegative X/Y, and excludes neutral-marked enemies from its ordinary enemy
pass. It uses the rebuilt mappings, standing actions and Down facing for ordinary
combat actors. Old exploration physical indices, action scripts and logical 128–130
are not preserved as those battle enemies. In particular, the two Soulsower
exploration sprites do not become enemy types by carrying over their IDs; battle
slots 129/130 select DARK_MADAM/HIGH_PRIEST from their rows.

After ordinary placement the routine tests F500+CURRENT_BATTLE. For Battle20
with **F520 clear**, the actual `table_NeutralBattleEntities` entry in
`data/battles/global/battleneutralentities.asm` supplies one actor: `(6,6)`, Down,
ELRIC_BASE, `eas_WhirlsInWater`. The neutral loop starts at combatant 159, zeroes
max/current HP and status, ORs the neutral activation bit, writes combatant X/Y,
and declares a new physical actor. The actual predecrement write stores that
physical index at lookup31 (`$FFB15F`), while GetEntityIndexForCombatant(159)
reads lookup63 (`$FFB17F`), left FF on this fresh path. DeclareNewEntity stores
the physical cursor as ENTNUM and does not repair that mismatch. Combatant159,
lookup31, the physical actor and ally13 are separate; there is no working159
binding or carried exploration lookup here. The action enables immersion/double animation
speed and loops refreshed Right/Up/Left/Down facings with wait 10 each; initial
Down is not a permanently held facing or an observed drowning animation.

F520 set skips the neutral tail, including when CheckBattle has just cleared
F420 but retained this admission. It does not remove the twelve selected enemy
rows. Neutral Elric's HP 0 does not invoke ordinary dead-ally omission; this is
its own declaration path. Neither the neutral nor the approach adds Elric to
joined/active flags. If those membership flags were independently supplied,
normal ally placement and this separate neutral are not automatically reconciled.

LoadBattle then selects the entity for d0 (fresh entry supplies combatant 0),
initializes sprites, loads map/mapsprites, installs base VInts, loads terrain,
requests map music and fade-in, and returns. Terrain loading is a named existing
service dependency; no terrain reconstruction, capture or movement simulation
is added. The start wrapper then follows the F470/empty-target rule above.
Return to `@Start` is the completed static entry boundary, not the first observed
playable turn, a guaranteed roster snapshot after activation, or a rescue.

## Reproduction and remaining limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands.
Verify the read-only source revision and clean status. Use the unchanged
[retained comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
for map-init equality, `_verify_map_script_engine_fixture` and source
`_cutscene_facts` equality. They passed without rewriting the retained pair.
For direct reads, use the exact source paths, symbols, slots and source ranges
above; strip comments before comparing instructions/operands and resolve enums,
macros and actual jump-interface aliases. Full selected programs and immediate
consumers, rather than guessed filenames or aggregate counts, own the joins.

| Existing evidence under `tests/fixtures/h2/` | Selected reuse and direct review |
| --- | --- |
| `map-script-engine-static-v1.json` and retained `local/derived/map-script-engine-static.json` | `programCorpus.programs` IDs cs_5C312/cs_5C3AA: all 29/5 records, source lines/macros/arguments and both complete inline moves; direct literal text 2555–2565. |
| `map-events/zone-programs.json`, `map-init-static-v1.json` | Programs 94–96 retain the complete selected Map51 callback bodies; actual default/F520 pointers, four versus empty fixed rows and shared F470 init boundary. |
| `map-event-request-consumption-static-v1.json`, `battle-routing-data-static-v1.json` | Shared request/admission context; direct walking/zone/no-scroll/MainLoop joins, complete switched-map/battle-coordinate tables and sole Map51 row 20. |
| `battle-cutscenes-static-v1.json` | Existing wrapper-source comparison plus direct before/start slot 20 and actual empty target. |
| `battle-loop-static-v1.json`, `battle-spriteset-data-static-v1.json`, `common-stats-static-v1.json` | Shared lifecycle/inventory/membership context; complete selected spriteset20, actual neutral/random-upgrade lists and immediate consumers read directly. Aggregate spriteset fixture is not claimed to guard every row's content. |

Discriminating states include independent F470/F420/F520, direct F420 admission,
F88 saved state, dead/missing/mismapped actors, changed followers after setup,
empty/over-capacity party, debug text skip, partial services and overwritten
pending requests. These constrain the static account; they are not executed
scenarios. **Unknown:** natural route and producer selection, actual flags,
HP/class/membership/lookups/mode, completed action/text/camera/fade services,
rendered scene, usable control and persistence. No automatic runtime queue is
admitted under [ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md).

Preserve [PR866's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/864#issuecomment-6170376311),
[rebased results](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/864#issuecomment-6170461179)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/866#issuecomment-6170476818),
including the retained pair and linked predecessor failures. Lookup misses and
truncated output are not absence evidence; locate/reread actual owners. Current
inspection corrections and exact Git/check/CI/process identities belong in this
Issue/PR handoff. Acceptance is complete owned-source/prose/provenance/link/private
review, `git diff --check`, clean committed configured
`uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head CI.
No blanket local normal/full/engine/Godot/H3/H4 suite is selected solely for this
prose. Leave a clean frozen Draft PR for independent review; no integration or
cleanup is delegated here.
