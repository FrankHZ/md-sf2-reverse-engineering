# Battle25 outcomes through Ketto entry

Evidence date: 2026-10-23. **Confirmed:** Battle25's empty outcome scripts
retain the shared death, victory, defeat and escape wrappers. Victory returns
to a member-derived Map48 position without an immediate battle scan. Separate
north-cave exits request Map72, where battle admission can intervene before
a later Ketto request. Ordinary Ketto entry builds eight town actors and
conditionally relocates two under F800. **Inferred:** these transitions form
a candidate continuation from North Cave. **Unknown:** natural traversal,
supplied state, completed services, rendered arrival, usable control and saves.

## Agreement and provenance

Standard rigor for [Issue #993](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/993)
under [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
outcome bookkeeping, mutable return coordinates, later traversal/admission and
town initialization are separate. Starting accepted base is
`7550499bd0a2f60a42da55b5caf37bda33ddbebd`, including #989/PR992. This is the
sole tracked owner; no shared index, registry, schema, fixture, tooling,
design, remake/UI, glossary or Chinese write. Concurrent #994 synthesis and
UI692 remain independent; no unmerged conclusion or stacked dependency.

Consume accepted [Battle25 admission](./north-cliff-entry-through-battle25-admission.md),
the [initialization/first-control continuation](./battle25-initialization-and-first-control.md),
[Battle24 outcomes](./battle24-outcomes-through-north-cliff-preparation.md),
[Battle11 admission](./east-shrine-return-through-battle11-admission.md),
[Battle11 outcomes](./battle11-outcomes-through-polca-arrival.md),
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[request writers](./map-event-request-state.md),
[request consumers](./map-event-request-consumption.md) and
[roster initialization](./battle23-initialization-and-roster.md).
Scope ends after Ketto's initial setup/init returns and the bounded controller
installation request. Battle26 roster/combat, combat simulation, subsequent
town conversations/merchant run/shops/church/houses/items/quests, runtime,
acquisition, regeneration and production implementation are excluded.

Source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The retained read-only checkout is clean at that pin. Fixtures/payloads identify
USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Constants/RAM belong to `sf2enums.asm`/`sf2const.asm`. Fnnn means flag nnn;
rows are zero-based and extents half-open. Short battle paths are under
`code/gameflow/battle/`; script consumers under `code/common/scripting/`.

Minimum evidence is selected source, existing contracts/fixtures and the
retained 18,285,979-byte init/script pair. Linear comparison is budgeted below
one minute/512 MiB with small prose/receipts; the unchanged-pair comparison
took 0.088s. No new maintained checker/platform or installation. Replan a
material contradiction or supporting-tool expansion; Unknowns do not authorize
collection. Direct source/fixture/consumer and changed-state review, document
checks, clean committed planner and exact-head CI are the acceptance profile.

## Actual termination and Battle25 outcome joins

**Confirmed:** `battleloop_1.asm:@IsBattleEnd` calls the defeated hook, then
ProcessKilledCombatants, then CountRemainingCombatants. It tests zero allies
before zero enemies. If both remain, ProcessAfterTurnEffects runs, followed
by death processing and the same ordered tests, without a second defeated
hook. No action/after-turn effect is simulated here.

| Selected source entry | Battle25 value and consumer |
| --- | --- |
|`data/battles/cutscenes/battleendcutscenes.asm`, slot25/byte50, $47C1A|Relative ms_Empty target; ExecuteBattleCutscene_Defeated.|
|`data/battles/global/enemyleaderpresence.asm`, slot25, $47CA7|Byte0; defeated wrapper's loc_47C48 tail.|
|`data/battles/cutscenes/afterbattlecutscenes.asm`, slot25/byte50, $47D26|Relative ms_Empty target; ExecuteAfterBattleCutscene.|
|`data/battles/cutscenes/afterbattlejoins.asm`, slot25, $47D83|Byte0; EndAfterBattleCutscene calls JoinForce(0).|
|`data/battles/global/afterbattlepositions.asm`|Only Battle05 has an override; no25 row.|

The layout include `layout/sf2-07-0x044000-0x064000.asm` selects the
**cutscenes** after-join table. `map/ms_empty.asm` is the word $FFFF at
$47A4E..$47A50. Empty scripts still execute interpreter termination: possible
scroll wait when dialogue is open, then scrolling-speed clear. They do not
cancel their callers or imply completed services.

`cutscenes/battleendcutscenesstart.asm` requires Bowie HP nonzero and enemy128
HP zero. F525 clear dispatches ms_Empty; set525 skips it. Both reach
`battleendcutscenesend.asm:loc_47C48`; byte0 skips the living-enemy append loop.
The accepted fresh roster has Dark Soldier128 and fourteen other starting
combatants129–142; slots143–159 are absent/HP0/X=-1. Soldier128's AI is
ATTACKER1, and other AI/order/region attributes are distinct from the
leader-presence byte. Killing128 alone does not eliminate its companions.
`ApplyPositionsAfterEnemyLeaderDies` also tests Bowie/enemy128, but its table
scan finds no25 row and changes no placement. No fixed exit position results.

`battleloop/countremainingcombatants.asm` scans all30 ally and32 enemy slots.
It counts nonnegative X **byte** and nonzero HP **word**, not membership,
activation, AI role, Y or in-crop position. Bowie HP0 forces ally count0.
Thus defeat wins simultaneous zero counts; Bowie alive does not suffice if
no ally is counted. With positive allies, zero counted enemies wins. Hidden
X=-1 enemies are excluded even with HP; supplied placed/nonmember or unused
slots with nonzero HP can count. Fresh roster assumptions do not govern F88
resumed state or arbitrary changed combatants.

`battleloop/processkilledcombatants.asm` consumes the length-delimited death
list, not a new HP scan. Spin/sound passes precede defeat/kill bookkeeping,
X/Y=-1, status0, stat recomputation and mapped entity position $7000/$7000.
It does not itself zero listed HP or clear list length. Stale/duplicate lists,
inconsistent mappings and unfinished presentation services are not repaired
by the empty Battle25 hook.

## Victory seed, loss and escape

**Confirmed:** `battleloop_2.asm:BattleLoop_Victory` performs this order:

1. HealLivingAndImmortalAllies scans all30 allies: living allies and Peter7/
   Lemon28 even if dead get maximum HP/MP, existing status masked to
   STUN/POISON/CURSE as an intermediate mask. UpdateCombatantStats removes
   incoming CURSE and recomputes final ally CURSE from currently equipped
   cursed gear; the intermediate mask is not final status. Other dead allies
   remain dead;
   joined/active membership is not this predicate. Battle44's timer exception
   does not apply.
2. Seed PARAM2 from CURRENT_MAP. UpdateForceAndGetFirstBattlePartyMemberIndex
   rebuilds joined/active lists in ascending ally ID and returns the first
   active-list byte, without an HP or empty-count guard. Combatant X/Y plus
   BATTLE_AREA_X/Y are added as **bytes** into PARAM3/4. The actual
   GetEntityIndexForCombatant mapping, scaled by32, supplies physical facing
   for PARAM5. Set PARAM1=0; this does not write MAP_EVENT_TYPE.
3. ExecuteAfterBattleCutscene tests525. Clear dispatches ms_Empty, set skips
   it; both reach EndAfterBattleCutscene → JoinForce(0). This sets Bowie's
   joined F0, rebuilds force, and sets active F32 if fewer than12 are active.
   A full party does not evict someone. This is neither Rohde recruitment
   nor a repair to the return seed selected **before** the join.
4. Clear425, set525; zero d0–d3 words and copy PARAM2–5 bytes into them;
   return d4=1. Intro475 is unchanged. There is no unlock426 write.

For the ordinary Map48/Battle25 area `(28,0)`, live local `(x,y)` returns
`((x+28) mod256,y mod256)` and current mapped facing. This is not the initial
formation, necessarily Bowie, either cave exit, or Ketto. An empty active
list leaves a stale first-list byte; a dead/off-map first member, different
map/crop/mapping or later parameter overwrite changes the seed. The wrapper
does not validate these. Interrupted after-script/join/flag writes retain
their reached prefix; there is no transaction or rollback.

Defeat calls UpdateBattleUnlockedFlag, clears DIALOGUE_NAME_INDEX_1, requests
sad music/text363, closes text, restores Bowie's HP maximum, halves unsigned
gold, obtains Egress coordinates and returns d4=-1. Battle04's exception
does not apply. In `battlefunctions/battlefunctions_1.asm`, admitted Egress
deducts the selected spell cost from MP; Angel Wing removes its selected
inventory slot. Their common text/sound/flash tail updates unlock, unwinds
the action frame, obtains Egress coordinates and returns d4=0. Escape does
not apply defeat's gold loss or Bowie restoration. Actual action admission,
inventory/MP and successful text/flash services remain premises.

UpdateBattleUnlockedFlag preserves425 when525 is clear, and clears425 when
525 is set. Defeat/escape do not newly set525, run the victory after/join or
change475. Completed battle admission may already have cleared425. These
paths return registers rather than producing a new event-kind/parameter
tuple; pending-slot resets belong to subsequent exploration. Its shared
healing can refill eligible living/immortal allies' HP/MP after the handler
cost, so consumed MP is not a promise of the eventual exploration MP value.
Consumed item and halved gold are distinct from that healing; dead
non-immortals other than defeat-restored Bowie are not resurrected.

`battleloop/getegresspositionforbattle.asm` has no25 special: it passes
**EGRESS_MAP**, not stored XYZ/facing or automatically Map48/72, into
`code/common/maps/egressinit.asm:GetSavepointForMap`. BattleLoop sets399 on
fresh/resumed entry. With399 set, the savepoint table gives supplied Map6
New Granseal `(11,22)`, Up; supplied25 Ketto `(12,11)`, Up; unmatched48/72
retain that map and default to `(1,1)`, Up. Other selectors use their row or
fallback. No first Ketto visit or saved church selector is established.
Direct399-clear calls instead return initial Map3 `(56,3)`, Down, before
raft reset. With399/F64 set, raft-reset rows select Map72 `(43,48)` for
Polca/Bedoe, Map69 `(28,55)` for Hassan, otherwise Map69 `(42,15)` including
6/25/48/72. F64 clear preserves raft fields; reset is not boarding.

## Immediate exploration and later cave exits

**Confirmed:** all three BattleLoop returns reach
`code/gameflow/mainloop.asm:alt_MainLoopEntry`: SwitchMap, then ExplorationLoop,
without CheckBattle or a sign test on d4. The flag-switch table has no48/72/25
replacement. Ordinary victory's explicit48 therefore does not immediately
re-admit25, even if another producer supplies425.

A later no-scroll request can return to the ordinary scan. On effective48,
only row11 at exact `(15,26)` with411, or row25 at exact `(55,29)` with425,
can qualify; `(52,12)` alone qualifies neither. A held511/525 clears the
selected unlock after selection and still admits that battle on this call.
Victory25 clears425 but not411; neither completion flag alone is admission.
An arbitrary member-derived return point is not automatically a trigger.

`explorationfunctions_2.asm:ExplorationLoop` clears pending type, subtracts
20000 from STEP_COUNTER with signed negative clamp, heals eligible allies,
fades out, and on explicit48 sets current map48/current battle255. It loads
tiles, clears VInts, obtains the setup entity list, rebuilds entities/sprites,
clears temporary F256–383 and sets80 before LoadMap, base VInts and init.
Persistent story/battle/party/follower flags survive. BattleLoop cleared
PLAYER_TYPE; its empty25 hooks do not replace it, so ordinary return uses
mode0. Altered/direct modes still follow their actual shared consumers.

MapSetups has no48 entry. `map/mapsetupsfunctions_1.asm:GetCurrentMapSetup`
returns ms_Void/$FFFF at its sentinel. GetMapSetupEntityList returns that
sentinel; InitializeMapEntities still calls ClearEntities and builds flag
followers, skips fixed NPCs on byte$FF, then declares main physical0 from
the supplied words/facing. RunMapSetupInitFunction sees $FFFF and skips the
call. Absence of a setup directory is not absence of shared behavior.

ClearEntities clears the old actor state/mappings and resets walking scratch;
the former battle entities and scene follow chains are not retained as fixed
exploration actors. Map48 and25 are neither overworld nor follower-excluded:
ordinary follower flags govern declarations, without the F65 Caravan prefix.
In source order these are66/Sarah,67/Peter,72/Kazin,73/Slade,74/B/Astral,
66/Chester,68/B/Oddler,75/Luke,69/C/Fairy,70/B/Astral,74/A/King,
71/Lemon,74/D/Minister,76/Zynk,77/A/Old Man,82/Frayja,83/A/King,
83/D/Minister,84/Rohde. A/B/C/D are logical30/31/158/159 (lookup offsets
30/31/62/63), not guaranteed actors.
Declarations allocate physical1 upward and append physical follower IDs;
later alias declarations can replace a lookup while leaving earlier actors.
Rohde's joined11/active43 flags are independent of follower84. Ally HP/class
affects sprites (dead outside battle → blue flame), not follower admission.

CURRENT255's no-battle partial reload still heals, updates main/raft, loads
and runs init, but retains map/battle, population and temporary flags.
Border scroll stays inside exploration and skips ordinary population/temp
reset/init; direct script LoadMap is also a different boundary. F88 entry
restores supplied battle state rather than fresh formation; its eventual
outcome uses that state. None substitutes for explicit48's lifecycle.

The relevant `data/maps/entries/map48/6-warp-events.asm` rows are:

| Row / address | Exact source X/Y | No-scroll destination |
| --- | --- | --- |
|0 /$B4B16|`(55,30)`|Map72 `(27,34)`, Down3|
|1 /$B4B1E|`(52,13)`|Map72 `(25,29)`, Down3|

Rows2/3 belong to the other cave-side returns `(15,27)` →72 `(58,50)` and
`(3,12)` →72 `(56,46)`, Down. Their presence does not join the cave regions
or prove a navigable route. Neither selected row tests525/425/475,750,
Rohde membership, HP or party size; victory itself invokes neither row.

Ordinary `entity/entityscriptengine_2.asm:esc02_controlCharacter` needs the
masked layout kind$1000 before `exploration.asm:WarpIfSetAtPoint` scans
eight-byte rows. It divides internal coordinates by384, matches source low
bytes, writes type1, PARAM1–4 as a longword, facing/padding as a word and
WARP_SFX. These rows contain fixed destinations. ProcessMapEvent clears type
before dispatch; no-scroll consumption submits/clears SFX, idles physical0,
unwinds exploration, samples its coordinates, replaces nonnegative destination
**low bytes**, applies facing and returns d4=0. Ordinary sampled high bytes
are a premise of the nominal coordinate words. Mutable pending replacement,
changed mode/layout/map or interrupted services can change the result.

## Map72 interruption boundary and Ketto request

**Confirmed:** later no-scroll return reaches MainLoop's ordinary SwitchMap →
CheckBattle path. The complete Map72 battle candidates are8,24,26, in that
order, with unlock408/424/426 and wildcard trigger `(255,255)`. Areas are
respectively `(1,32,32,30)`, `(1,32,32,30)`, `(28,0,36,32)`; they are output
crops, not containment tests. The first unlocked candidate wins even at
`(25,29)` or `(27,34)`. A held matching completion508/524/526 clears its
unlock **after selecting that same battle**, without continuing the scan.
Victory25 alone neither sets426 nor promises unopposed Map72 exploration.

Without battle admission, Map72's only/default setup has an empty fixed list
and the ordinary follower lifecycle; F65 can prepend Caravan on this overworld.
F750's flag-copy row opens the `(27,33)` entrance on load. The init at $4FF2E
tests716 then joined-Luke10: 716 set/10 clear runs the accepted cs_4FF5A;
otherwise returns. It does not test525 or directly request26. Alternate
supplied histories may therefore enter that earlier recruitment scene.

Subsequent walking zone production requires layout kind$1400 and writes
event6/coordinate words; source coordinates alone do not produce the event.
RunMapSetupZoneEvent selects the first matching row by low bytes:

| Ordered Map72 zone rows | Callback boundary |
| --- | --- |
|0–2: X32,33,34, any Y|Map72_ZoneEvent0 calls CheckRandomBattle(26).|
|3–8: X25..30, Y37|Cave callback:750 set,805 clear or256 set returns; otherwise the accepted item/opening branch.|
|9/default|F523 clear selects helper8; set selects helper24.|

A declined specific callback does not fall through to default. Numeric mode1
installs eas_Raft/opcode7; mode2 eas_Caravan/opcode8, so neither is this
ordinary walking producer. A supplied event6 can still invoke the zone.

`CheckRandomBattle` checks **526**, not525: clear526 requests26 immediately
without testing counter or drawing RNG. Set526/nonzero STEP_COUNTER declines;
set526/counter0 draws range8. Zero requests; nonzero draws range4+2 and
sets counter2–5 without requesting. Request sets426, writes longword$000100FF
at MAP_EVENT_TYPE (type1/PARAM1=0/PARAM2=CURRENT255), sets counter30000,
then waits for scroll and requests sound/flash. PARAM3–5 remain supplied.
These writes precede service completion. A later ordinary scan can still
select8/24 first if their unlocks survive. This is an admission discriminator,
not Battle26 roster, combat, victory or a guarantee of a random encounter.

`data/maps/entries/map72/6-warp-events.asm` row2, $C1C0E, independently matches
`(31,26)` and requests `MAP_KETTO=25`, `(16,30)`, Up1, no scroll. It uses the
same producer/mutable-slot/consumer chain above; it has no525/526/800 gate.
The row does not demonstrate a walk from either cave exit around intervening
zone/battle triggers. For an intact explicit25 request, SwitchMap leaves25
unchanged and the **entire** battle-coordinate table contains no Map25 row:
CheckBattle returns -1 regardless of held battle unlocks. Map25 Ketto and
Battle25 North Cave are different indices in different tables.

## Ketto initial population, F800 and control boundary

**Confirmed:** MapSetups selects only default ms_map25, with no flag variant.
Its pointer table at $5D1CA..$5D1E2 binds entities, entity events, zone events,
descriptions, item events and ms_map25_InitFunction in that order. F800 is
an init condition, not a population selector. Map25's flag-copy table is only
endWord. Explicit entry follows the shared exploration order: set current25/
battle255, tiles, entity list/build/sprites, temp reset/80, LoadMap, base VInts,
init, conditional music/fade, then WaitForEvent. Setup selection occurs for
the list and again for init; no town conversation or zone callback is invoked
merely by selecting those other pointer-table entries.

The complete `map25/mapsetups/s1_entities.asm` list at $5D1E2..$5D224 is eight
8-byte records plus a2-byte sentinel. All sprites are ordinary NPC indices
above29 and below240, so each consumes one NPC lookup and one physical slot.
Let **n** be the actual number of preceding admitted ordinary follower rows.
NPC logical128+j uses lookup offset32+j and physical n+1+j; main is physical0,
independently of the battle's first-active-member seed. Party membership does
not insert all active combatants as town actors.

| Row / logical ID | Physical | Declared X/Y, facing | Sprite symbol / value | Action or walking origin/range |
| --- | --- | --- | --- | --- |
|0 /128|n+1|`(17,26)`, Down3|YOUNG_MAN196|origin17,26; range1|
|1 /129|n+2|`(24,25)`, Down3|OLD_MAN198|eas_Init|
|2 /130|n+3|`(20,13)`, Down3|MAN194|origin20,13; range1|
|3 /131|n+4|`(8,21)`, Down3|YOUNG_MAN196|origin8,21; range1|
|4 /132|n+5|`(4,12)`, Down3|MAN194|origin4,12; range1|
|5 /133|n+6|`(11,3)`, Right0|MERCHANT193|eas_Init|
|6 /134|n+7|`(12,10)`, Down3|PRIEST192|eas_Init|
|7 /135|n+8|`(27,17)`, Right0|WOMAN195|eas_Init|

The symbols in the sprite column have prefix MAPSPRITE_. For example only
67/70/84 yields Peter1, Astral2, Rohde3, then NPCs4–11; 133 maps9 and135 maps11.
With no followers NPCs1–8, 133 maps6/135 maps8. Enabled duplicate alias flags
still allocate actors. Source initialization has no capacity check; the
ordinary follower-action pointer sequence has five declared entries before
the adjacent overworld sequence, and walking/entity storage is finite. The
formula describes successful declarations, not safety for arbitrary mass
enablement or corrupt mappings. Separate raft declaration can use physical31
under the actual F64/map/mode conditions; it is not logical FOLLOWER_B31.

`sf2mapsetupmacros.asm` encodes walking parameters as `$FF,originX,originY,range`.
DeclareNewEntity initializes position/destination, clears velocity/travel,
stores facing/sprite/physical number and interprets the negative action longword.
With high byte$FF it calls SetWalkingActscript, copying50 bytes of eas_Walking
and replacing its three words at$22/$24/$26 with originX/originY/range.
Four walkers consume200 bytes of walking scratch; fixed rows store eas_Init.
`esc06_walkRandomly` uses origin±range bounds: nominal rectangles are
X16–18/Y25–27, X19–21/Y12–14, X7–9/Y20–22 and X3–5/Y11–13. These are bounded
random-step requests, not four scripted paths or measured positions. The
consumer attempts up to four random cardinal directions, checks its current
coordinate against the relevant bound and applies layout/other-destination
checks before installing motion. Layout slope handling can adjust Y; it is
not an unconditional rectangular clamp for altered states.

eas_Walking waits30, configures speed/acceleration and collision flags, then
loops random-walk → destination wait → wait20. `eas_Init` configures speed32,
zero acceleration factors, collision/obstruction/auto-facing, Up **orientation**,
size24 and sprite update, then Idle. Orientation is distinct from the declared
facing. Installed scripts and subsequent VInts mean declaration values are
initial requests, not an observation of every actor at the control boundary.

`s6_initfunction.asm:ms_map25_InitFunction`, $5D39C, has four operations:
chkFlg800; beq return_5D3A8; script cs_5D3AA; rts. Clear800 returns without
relocation; set800 executes the complete14-byte program at $5D3AA..$5D3B8:

| Record | Macro operands | Encoding |
| --- | --- | --- |
|0|setPos133,25,18,RIGHT|opcode$19 word + four operand bytes;6 bytes|
|1|setPos135,25,17,RIGHT|same;6 bytes|
|2|csc_end|$FFFF;2 bytes|

`map/mapscriptengine_1.asm:csc19_setEntityPosAndFacing` tests its actor byte
through AdjustScriptPointerByCharacterAliveStatus. NPC133/135 bypass the ally
HP guard. GetEntityAddressFromCharacter masks the index, subtracts96 for
negative actor bytes, reads lookup offsets37/39, then scales the resulting
physical index by32. For each target, csc19 writes X/XDEST=25*384, Y/YDEST=
18*384 or17*384 and facing Right0, then requests sprite update. It does not
walk from the origin, replace action scripts, clear velocity/travel, recruit,
set800 or execute the merchant conversation. Missing zero lookup aliases
physical0; a hidden$FF mapping is not a safe no-op. Fresh normal declarations
supply the intended merchant/woman; retained or corrupt mappings may not.

F800 is supplied state here. The upstream comment names a merchant run, but
its producing conversation and shop behavior are outside this evidence.
The script has no text or sleep command and its setPos operations are not
skipped by the text-skip mechanism. Termination can still wait for open-text
scrolling and clears scroll speed. Interruptions preserve whichever actor
writes completed, without proving the second move or init return.

CURRENT255 on current25 retains actors/temp state but still invokes this init:
set800 reapplies the two positions to current mappings; clear800 does **not**
restore original positions. Fresh explicit25 reconstructs the eight origins
before applying800. Scroll skips init. Supplied CURRENT on another map does
not mean Ketto. Mode2's population tail changes main to raft and hides/stands
followers; mode1/other supplied modes retain their own shared behavior.

After init returns, ExplorationLoop compares palette state and conditionally
requests PlayMapMusic/FadeInFromBlack. WaitForEvent with pending zero selects
camera physical0 and SetControlledEntityActScript(0): mode0 installs
eas_ControlledCharacter, mode1 eas_Raft, other nonzero mode eas_Caravan.
With pending nonzero it skips installation and returns the event instead.
This is the stopping boundary: no observation of usable rendered Ketto,
completed input/sound/palette/DMA/VInt/action services or save persistence.

## Changed-state acceptance and reproduction

| Change or interruption | Required result distinction |
| --- | --- |
|128 dies while another counted enemy survives|No leader append; Battle25 may continue.|
|Zero enemies with Bowie dead or zero counted allies|Defeat has precedence, including simultaneous exhaustion.|
|After-turn death / hidden or supplied slots|Second count follows death processing without defeated-hook replay; count actual X/HP.|
|525 held|Skip empty scripts where gated; victory still joins0/clears425/sets525; defeat/escape clear425.|
|Different/empty active list, HP, map/crop/facing mapping|Victory seed follows actual state; later JoinForce does not repair it.|
|Interrupted death/after/join/escape service|Keep reached writes/costs; no later return or completion claim.|
|Explicit48 versus CURRENT/scroll/F88|Different population/temp/init/roster lifetimes; no immediate postbattle scan.|
|Changed pending map/coordinates or cave exit|Actual mutable request controls the next scan, not a named route.|
|408/424/426 held or completed|Map72's ordered wildcard scan can intercept either exit.|
|526 clear/set, counter/RNG varied|Helper26 cadence depends on526;525 does not authorize26.|
|F750/716/10 or zone history altered|Map72 load/init/zone conditions remain independent of victory25.|
|Explicit25 intact, any unlocks|No Map25 battle row; normal Ketto setup remains independent of Battle25 ID.|
|Party/HP/class/follower84 or alias flags changed|Actual follower prefix/mappings/sprites change; no guaranteed Rohde/healthy cast.|
|F800 clear/set; repeated CURRENT on25|Fresh origins versus relocation; clear800 partial reload does not undo old relocation.|
|Missing/hidden mappings, unsupported capacity, changed mode|No automatic mapping/capacity repair or universal controller claim.|
|Interrupted first/second setPos or pending event|Partial relocation and/or skipped controller installation remain explicit.|

Reproduce after loading ignored `local/private-inputs.ps1` in the same process;
pass the retained pinned upstream read-only and verify its clean HEAD. Use the
[retained-pair recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
for map-init equality, _verify_map_script_engine_fixture and actual
_cutscene_facts, preserving pair size/mtime. No extraction regeneration.

1. Join slot25 end/after/leader/after-join, include and empty target to their
   wrappers; compare complete ordering with battle-loop/control/cutscene
   fixtures and accepted roster. Follow counting/death/healing, first-active
   selection, physical facing, after-position absence, defeat/Egress/savepoint
   and alt_MainLoopEntry directly. Do not infer rows from aggregate counts.
2. Join Map48 rows0/1 and Map72 row2 through constants, macros, request
   production/consumption and complete battle-table candidate scans. Inspect
   Map72 setup/empty entities/init, zone order and helper26 branches against
   `map-init-static-v1.json` and `map-events/routing-setup.json` /
   `map-events/zone-programs.json` under `tests/fixtures/h2/`.
3. Compare all eight Map25 declaration rows and macro/assignment/walking
   consumers. Retained map-init primarySourceBodies selects
   sourceOwnerSymbol=ms_map25_InitFunction; routeJoins/pointerTableJoins bind
   default25 to that target. Compare all operations with source. Retained
   script programCorpus.programs id=cs_5D3AA contains exactly the three
   records above; compare sourceLine/macro/arguments/encodedBytes to source
   lines20–22, then follow csc19, lookup/HP, terminator and control consumers.
4. Review the full prose/diff and changed states, relative links/anchors,
   UTF-8/LF, sole-path and private boundaries; run `git diff --check`.
   On a clean committed head run
   `uv run sf2 verify plan --base origin/main --head HEAD` and record actual
   exact-head CI in the Issue handoff. Documentation-only scope skips blanket
   local normal/full/engine/Godot/H3/H4 suites.

**Unknown:** natural outcome/traversal/layout-event production; supplied flags,
map/request/crop/counter/RNG, party/HP/class/mode/follower/mapping state and
capacity; completed service work, actor settling, usable town control and
save/load persistence. These do not weaken the static conditional findings or
form an automatic runtime queue.

Preserve [#989 correction handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/989#issuecomment-6181049459)
and [PR992 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/992#issuecomment-6181060429):
headb714f581/tree9955d447/base5dbcf49a, CI38656388954(scope21s/research36s),
original18fee1d6/CI38655534808 and reviewed7efb8771/CI38655682054, author0.107s
unchanged-pair receipt, Battle11 words-to-bytes P2 correction and ignored
first-operand assertion correction. Retain all linked predecessor failures,
Unknowns and local source/configuration/pair/results. Leave a clean pushed
frozen Draft PR and Project Review for independent main-gate acceptance;
no merge, cleanup, archive or next Issue.
