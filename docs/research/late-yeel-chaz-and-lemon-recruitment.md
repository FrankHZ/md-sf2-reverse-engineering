# Late Yeel, Chaz and Lemon recruitment

Evidence date: 2026-10-22. **Confirmed (static):** Map05 has two late
populations, a piano-controlled church stair, two underground conversations
selected by F930, Chaz's talk recruitment and a separate Lemon zone recruitment.
Membership, active-party admission, current actors, following and reload pose
are different state changes. **Inferred:** these form an optional late Yeel
excursion. **Unknown:** natural itinerary and incoming state, completed services,
presentation, terrain reachability and save durability. This closes the selected
static chapter, not all recruits, revisits, English design or playable content.

## Agreement, provenance and reproduction

**Standard** rigor for [Issue1210](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1210)
under Epic1053: a connected content account for later design/implementation.
Coupled setup, alias, flag, layout and party lifetimes require changed-state
review. Accepted base is `03a970e918af1773543bd823fa70ff30beb7cb10`, with no
stacked dependency. Owned paths are this file, bounded Yeel/Chaz/Lemon/revisit
navigation in [story coverage](./story-content-coverage.md) and a short
[source frontier](./source-coverage.md), preserving its complete pipe tables
and counters. No code, fixture, schema, global index, runtime, lowering or engine
expansion is part of the agreement.

All original paths below are relative to `disasm/` at read-only
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Registered USA ROM identity is SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This investigation reads source and retained evidence, not a new source/ROM or
runtime comparison. `sf2enums.asm`, `sf2const.asm`, `sf2mapsetupmacros.asm` and
`sf2cutscenemacros.asm` own identities, RAM, units and command encodings. Fnnn
denotes flag nnn; IDs/coordinates are decimal, addresses hexadecimal, ranges
half-open. Local paths mean `data/maps/entries/map05/`, setup bodies under
`mapsetups/`; `map/` means `code/common/scripting/map/`. B, P, S, C and L below
mean Bowie0, Peter7, FOLLOWER_B31, Chaz27 and Lemon28, respectively. S is a
logical alias, not a guarantee that an Astral actor exists.

Reuse the [early Yeel owner](./battle02-to-battle03-content-route.md),
[Red Baron aftermath](./battle37-outcomes-and-lemon-aftermath.md),
[Map41/Force Sword owner](./battle39-outcomes-and-force-sword-route.md),
[Claude join/follower distinctions](./magic-tunnel-hub-and-claude-recruitment.md),
[field search](./field-search-control.md),
[request consumption](./map-event-request-consumption.md),
[menus](./common-menus.md), [map content](./map-content.md) and
[common scripting](./common-scripting.md). After37 does not join Lemon;
Map41 is an accepted alternate recruitment entrance, not a prerequisite here.

Finite input is all21 Map05 setup files, 62,685 bytes including older
alternatives; eight local map tables, 4,800 bytes; `scripts_1.asm` is25,959
bytes within that setup total, not an additional input. Selected shared bodies
and retained18,285,979-byte map-init/map-script-engine evidence are reused;
provenance remains `local/issue1005/pair.txt`. Work is linear in selected source
and prose, with direct checks budgeted under60s/512MiB and under4MiB of new
ignored `local/issue1210/` receipts. Peak memory is unmeasured. No corpus/pair
regeneration, download, acquisition, new platform or full service/economy census.
Reassess material contradictions or expanded supporting work before scaling.

Reproduce source review from the configured pinned root, then follow the named
shared consumers and complete selected program bodies below:

```powershell
# $disasm selects the read-only pinned disasm directory.
git -C (Split-Path -LiteralPath $disasm) rev-parse HEAD
git -C (Split-Path -LiteralPath $disasm) status --short
Get-ChildItem -LiteralPath (Join-Path $disasm 'data/maps/entries/map05') -File |
    Sort-Object Name | ForEach-Object { $_.Name; Get-Content -LiteralPath $_.FullName -Encoding utf8 }
Get-ChildItem -LiteralPath (Join-Path $disasm 'data/maps/entries/map05/mapsetups') -File |
    Sort-Object Name | Select-Object Name, Length
# Read selected late pointer/list/event/description/init/scripts files in bounded chunks.
```

`data/maps/mapsetupsstorage.asm` actually includes both late pointer tables,
both lists, s2–s5 late bodies, `scripts_2.asm`, late init and `scripts_1.asm`.
Older default/650 bodies are selector contrast, not replacement evidence for
late callbacks. The local handoff records complete direct-check recipe and
dependencies, semantic audit, committed planner and exact-head scoped CI.
Mechanical checks do not certify the source-to-consumer account.

## Entrances, resources and six exits

**Confirmed:** Map66 `6-warp-events.asm` rows3/4 are `(19,23)` →05 `(21,42)`
Up and `(18,22)` →05 `(7,1)` Down. No row checks F27/F28/F530/F940.
All six05 records in `6-warp-events.asm` (`$99714..$99746`, terminator included)
are no-scroll mode0, padding0:

| Source X,Y | Destination map / X,Y / facing |
| --- | --- |
|11,4|CURRENT255 /4,57 /Right0|
|4,57|CURRENT255 /11,4 /Down3|
|255,43|66 OVERWORLD_GRANSEAL_KINGDOM /19,24 /Down3|
|255,0|66 /18,21 /Down3|
|4,64|CURRENT255 /22,11 /Up1|
|22,11|CURRENT255 /4,64 /Up1|

255 trigger bytes are wildcards. The last reciprocal pair is commented
“unusable” in source, but64 is an actual source/destination byte. A64-wide
layout, declared area bounds or that comment do not prove observed geometry,
natural entry or safe memory access. Do not silently discard these rows or
normalize64 to63/0. The current production preparer refuses their ordinary
transfer admission, as distinguished below.

`code/gameflow/exploration/exploration.asm:WarpIfSetAtPoint` divides sampled
coordinates by384, scans ordered eight-byte rows and writes event1/payload/
facing and WARP_SFX on match. Movement/layout consumers still have to admit the
warp marker `$1000`; zone dispatch requires `$1400`. A coordinate row alone
does not supply terrain, facing, collision or a continuous walking route.

`explorationfunctions_2.asm:ProcessMapEvent` clears the request before dispatch.
No-scroll type1 plays/clears WARP_SFX, idles physical main, samples main0 and
applies nonnegative destination bytes to coordinate **low bytes**; retained
high bytes are not universally cleared. Ordinary tile samples give the table
coordinates; arbitrary supplied words need the actual byte composition.
`MainLoop:@Start` calls SwitchMap then CheckBattle. Only3/F640→4,
71/F530→78 and58/F999→59 are substitutions;05 and66 are unchanged.
CURRENT resolves to the actual current map for battle selection. The accepted
early/late [Map66 owners](./granseal-overworld-return-and-battle38-admission.md)
retain ordered battle interception: returning to66 is not an unconditional
exploration endpoint. Neither joining here nor a warp row unlocks a battle.

After battle selection declines, an explicit05 `ExplorationLoop` initializes
CURRENT_MAP/CURRENT_BATTLE, tilesets, selected entities/followers and sprites,
clears only temporary flags256..383, loads base layout/flag copies, then invokes
live init. CURRENT instead updates main/follower properties and raft without
rebuilding entities or clearing those temporary flags; it reloads the current
working map and still invokes live init. The named late flags are outside that
temporary interval. HealLivingAndImmortalAllies at exploration entry is separate
from the local scene/join commands; direct supplied callbacks need not have run it.

Complete relevant local declarations, reached through `data/maps/entries.asm:Map05`:

| Surface | Complete declaration and lifetime boundary |
| --- | --- |
|Header `$9965A..$99660`|Palette0; tileset slots0,8,43,37,58. Map05s0_Blocks/Map05s1_Layout select0-blocks.bin/1-layout.bin; final animation pointerFFFFFFFF. Private content is not published and no pixel/terrain decoding was performed here.|
|Areas `$99688..$996C6`|Main0,0..31,43 with secondary FG32,0/BG0,0/type0; main0,44..31,63 with FG0,0/BG0,0/type255. Both planes parallax256,256, autoscroll0,0, TOWN music.|
|Flag copy `$996C6..$996D0`|F940 copies32,45 size1×1 to11,4 during base-layout load. Held flag restores the declared stair cell on fresh load; merely setting the flag does not recopy an already loaded layout.|
|Step copies `$996D0..$996F2`|Triggers11,9;10,19;19,19;25,30 each copy32,44 size1×1 to their own trigger. These are four door entries, not four proven walked paths.|
|Roof copies `$996F2..$99714`|Same triggers, source255,255 clear sentinel; sizes/destinations5×7→41,2;7×5→38,14;7×9→49,10;6×6→54,24.|
|Chests `$99746..$99750`|15,48/F196 QUICK_CHICKEN;24,60/F197 EVIL_KNUCKLES, then terminator.|
|Other `$99750..$99758`|24,26/F134 HEALING_SEED, terminator and trailing RTS.|

Chest/search custody still uses actual layout kind, held flags and inventory
capacity through the accepted search owner. These tables do not grant items at
entry, prove acquisition, or establish service UI/presentation or persistence.

## Live setup, complete late populations and local interactions

**Confirmed:** `data/maps/mapsetups.asm` orders05 default →F650 →F530 →F28.
`GetCurrentMapSetup` starts at default and replaces A0 for **each held flag**,
so last matching28 wins, else530, else650, else default. F530 is the literal
flag, despite its “Battle30 completed” comment; no local test of later battles,
Force Sword, F930, prior Yeel or narrative chronology is added. F28 alone selects
the28 profile even with530 clear. Init/entity/zone/description/item wrappers
resolve this again on each call. Changing flags can change handlers immediately
without replacing the already loaded population.

Each pointer table has six long slots in order entities, entity events, zones,
descriptions, item events, init. Default and650 share default s3/s4/s5/init
(RTS), with their own s1/s2. Late tables are `$603AE..$603C6` (530) and
`$603C6..$603DE` (28). Both share
`ms_map5_flag530_EntityEvents`, `_ZoneEvents`, `_AreaDescriptions`, `_Section5`.
Only entities and init differ: `_flag530_Entities` / `_flag530_InitFunction`
versus `_flag28_Entities` / `_flag28_InitFunction` inside the same late init file.

Both complete fixed lists end FFFF; all rows use eas_Init except Orb's
eas_InitFixedSprite:

| Logical identity / sprite |530 list `$603DE..$60410`|28 list `$60410..$6043A`|
| --- | --- | --- |
|L28, ally sprite resolved live|16,32 Right|No fixed row|
|C27, ally sprite resolved live|25,53 Right|25,53 Right|
|128 / PRIEST|11,4 Down|11,4 Down|
|129 / MERCHANT|19,56 Down|19,56 Down|
|130 / ORB|19,33 Down|19,33 Down|
|131 / OBJECT1|63,63 Down|63,63 Down|

`mapfunctions.asm:InitializeMapEntities` clears physical/alias state, creates
flag-selected followers first, then lists, and physical main0 last. Ally sprite
operands27/28 create ally references, not nonallies128/129. A nonzero existing
ally alias suppresses the duplicate fixed record. Ordinary nonallies occupy
successive alias entries128..131 after any follower physical slots; their
logical IDs are not fixed physical slot numbers. F71 creates L as a follower
before either list and can suppress530's fixed L. No ordinary follower row
creates C. FOLLOWER_B may be created by74/68/70 with different sprites;
later declarations can overwrite its alias while earlier physical actors remain.
F71 is independent of joined28/active60 and HP. No fixed party or valid S/P
alias is an implicit requirement of direct supplied source calls.

`ClearEntities` zeroes alias entries. `GetEntityAddressFromCharacter` masks the
character word toFF, maps nonallies by subtracting96 and reads the physical
alias without a missing/FF/capacity check. Missing alias0 can address main0;
overwritten aliases address their actual later actor. These source consequences
must not be replaced by an invented allocation/recovery rule.

`GetAllyMapsprite` reads current HP/class. In exploration (`CURRENT_BATTLE=FF`)
HP0 gives BLUE_FLAME; otherwise class rules transform the ally mapsprite table.
The table's27/28 slots both contain LEMON_SPECIAL57, but C's start WIZ gives
its class transformation, while L's start RDBN retains57. Start records are
WIZ19/MAGE_STAFF|EQUIPPED for C and RDBN19/DARK_SWORD (not marked equipped)
for L. These are new-game definitions, not writes by this init or join.
Changed class, HP and live inventory remain live; this chapter does not reset them.

All late entity records in `s2_entityevents_530.asm` (`$6043A..$604C8`):

| Event / table byte1 | Complete local body |
| --- | --- |
|L / DOWN3|txt3483 then RTS; no flag check/join.|
|C / UP1|script cs_60F18 then RTS; no local27/28/930/940/941/942 guard.|
|128 / DOWN3|jsr j_ChurchMenu→ChurchMenu, return.|
|129 / DOWN3|CURRENT_SHOP_INDEX.b=SHOP_YEEL_UNDERGROUND29, jsr j_ShopMenu→ShopMenu, return.|
|130 / RIGHT0|F28 held: save speech, silence, NAME1=0, txt423 then4082, default RTS. Else if945 held: default RTS. Else cs_6060E, save speech, resolve L portrait/speech, display portrait, txt3483, set945, default RTS.|
|Default /0|RTS. No event for131.|

Byte1 is consumed as bit flags by `RunMapSetupEntityEvent`, not a required
player-facing direction. Bit0 turns the addressed actor opposite supplied d2;
bit1 controls the finish property update. Portrait lookup occurs before the
handler; VInt entity update is disabled around the body and reactivated at
return; portrait/text close in the wrapper. Church/shop are actual service calls,
with shop29 written before entry. Their full UI, price/raise/cure/save outcomes
are not re-proved here. Default item table (`$60606..$6060E`) has only
`Map5_212_DefaultItemEvent0:RTS`: no local item-specific join, Force Sword or
piano action; it does not imply ordinary field-item services never run.

Late area descriptions (`$60548..$60606`) set base D3=$D9C=3484:

| Search X,Y | Record / result |
| --- | --- |
|9,15|investigation429 (paper), description3484 poster instruction.|
|10,16|investigation427 (bookshelves),3485 ancient kingdoms.|
|11,16|427,3486 civilizations.|
|23,50|427,3487 ancient tower.|
|22,11|function DescFunc0: clear940 runs cs_60F64; held940 returns silently.|
|19,33|function DescFunc1: clear945 runs cs_6060E, L portrait/speech,3483,set945 regardless28. Held945/clear28 repeats L3483 without pose script; held945/held28 uses silent423 then4082.|
|Terminator|msDescEnd; trailing RTS/RTS are not more rows.|

This is intentionally different from entity130: there F28 is tested **before**
945; description19,33 tests945 first. A supplied28 held/945 clear therefore
gets hole text through entity130 but the L pose/text/set945 path through the
description. Neither callback joins L. F945's comment is not its complete meaning.

`CheckArea` derives the faced target through actual view entity/position/facing,
then checks chest/vase/barrel/bookshelf/generic item kinds before description
fallback. FieldMenu passes D6=0; no-entity ProcessPlayerAction passes1.
All six rows have byte2=0, so DisplayAreaDescription does not reject either D6;
function records save/restore D6, run the callback and unconditionally report
handled D7=-1 with window cleanup. Thus held940's silent RTS is still handled.
An unmatched row returns D7=0: action D6=1 can remain unhandled, menu D6=0
uses default423/412. Target22,11 in a table is not proof that arbitrary input
reaches this callback instead of another layout-kind branch.

## Piano and the church entrance scene

**Confirmed:** `scripts_1.asm:cs_60F64` starts text cursor3480, narrates a piano
question, calls yesNo and branches to cs_60FA4 if89 clear. The real
`csc11_promptYesNoForStoryFlow` calls j_YesNoPrompt, sets89 on zero result,
clears89 otherwise, waits10. Prompt starts Yes/zero; Left selects zero,
Right and B select nonzero; A/C return the current choice. A preheld89 is not
an authorization bypass if the actual prompt returns differently.

| Selected branch | Complete state/presentation request order |
| --- | --- |
|89 clear|cs_60FA4 sets cursor3482, single narration, end; no sound/copy/940 write.|
|89 set|cursor3481; MUSIC_PIANO_THEME; wait120; quake1; SFX_BIG_DOOR_RUMBLE; wait90; quake0; wait30; single narration3481; setBlocks32,45,1,1,11,4; setF940; end.|

csc34 consumes three packed words, calls CopyMapBlocks on the live64-wide
working layout (two bytes per cell/128 per row), then sets both view-update
bits. This is a scripted native copy, distinct from the static F940 record
applied on fresh base-layout load. An interruption can leave copy done/940
clear, or quake/sound/text prefixes incomplete; there is no transaction or
rollback. Held940 alone can disagree with current layout until a fresh load.
`reloadMap` uses LoadMap with D1=-1 and preserves current working layout;
it does not replay the static copy or fresh entity/init sequence.

Zones are exactly11,8→ZoneEvent0, wildcardX/42→ZoneEvent1, then default RTS
(`$604C8..$6051E`; the following `sub_6051E` raw description-like data is not
the selected s4 pointer). Zone0 requires940 held and941 clear. It runs
cs_6093A, then930 held→cs_60BA6, else cs_60B10; sets941 **after** the selected
script returns. A matching row that returns for a failed flag gate does not
fall through to default/another row.

The complete cs_6093A content/action sequence is:

1. Priority C0/128 nonzero; camera6,2; Init L/P/S; L faces Up;
   destinations B/P/S11,7; P moves Right1/Up, S Left1/Up. Priest128 jumps
   twice, mistakes visitors for devils and produces a holy symbol (3396–3398).
2. Priority131 nonzero/128 zero; LANDSTALKER_ITEM sound;131 at11,4 Down;
   wait60;131 Down1 (nonwaiting),128 Down1 (waiting), priest shivers.
   P/S explain the mistake (3399–3400); hide131, wait20, priest shivers and
   admits failure (3401–3402); S Up1 and tries to explain (3403–3405).
3. Priority C0/128 nonzero; C at11,4 Down,wait30; C addresses priest;
   priest turns Up/jumps twice, explains failure (3406–3408); priority128
   zero; C nods and questions him (3409–3411). S Right1/Up/two Up jumps
   and explains Granseal (3412–3413);128 Down, C shivers,128 Left1/Right;
   wait20; C Down1,wait40,two jumps and recognition questions (3414–3418).
4. S nod; identity exchange3419–3422, S two jumps, Right1/Up1/Left;
   C Right, S shivers;3423–3425 greet Hawel's son and ask his work; end.

Neither B nor the camera transfers to a new map in this program. hide131
calls HideEntity: offmap position/destination, aliases rewrittenFF and follower
list removal. That differs from MoveEntityOutOfMap's position-only behavior.
Full relative paths still depend on current aliases, HP skips and completion.

The two complete continuation alternatives are:

|930|Content and all movement/follow exits|
| --- | --- |
|Clear, cs_60B10 /3426–3433|C explains continuing Hawel's work; S asks questions, C nods, S jumps twice.128 Down1/Right1/Up offers the room; C/S face Down. S Down1/Left,128 Right; thanks, S Down,wait20; C Up1 nonwaiting,128 Up1 waiting; C63,63 Down;128 Up1 then63,63 Down; P follows B place2, S follows P place2; end.|
|Held, cs_60BA6 /3434–3441|S says the secret/tower is solved, C asks about Zeon; S nods,wait30;128 Down1/Right1/Up offers help,wait5,2xUpDown,wait120, repeats offer. S Down1/Left,128 Right; thanks, S Down,wait20. Same C/128 departure/hiding positions; P→B2,S→P2 **and L→S2**; end.|

Dialogue supplies motivation, not an additional gate for joining or entry.
These scripts do not set27/28/71/942/930 or warp underground. After return941
changes later init placement; actual access to the stair/warp still requires
layout/movement. “Priest goes downstairs” is not an independent map transition.

## Late init, cliff scene and underground return

**Confirmed:** `s6_initfunction_530.asm` spans `$6061E..$60708` and has two
entry points.530 first checks944: clear→cs_607DE, set944, shared28 entry;
held→cs_6060E, shared28 entry. The28 pointer enters **after** this whole
test, so joining L can bypass both the first cliff scene and repeated pose.
cs_6060E (`scripts_2.asm:$6060E..$6061E`) only installs eas_Immersed on L
and sets L19,33 Down; it neither joins/heals nor sets944/945 itself.

Complete cs_607DE /3388–3395 cliff sequence:

1. 130 at63,63; Init P/S; B21,39 Up/P22,40 Up/S20,40 Up;
   reload current map camera16,34; fadeInB; L's guilt3388,wait5;
   B2xRightLeft,wait5,P2xUpDown,wait5,S2xRightLeft,wait80.
2. P Up2, B/S Up; P identifies L3389; camera13,28; L's guilt3390,
   face Down/shiver/3391; deactivate autofacing, speed64/64 then idle;
   nod,wait30; L Up5/Right3,Down; custom motion OFF/orientDown/refresh/idle,
   deactivate autofacing; FALLING sound; L Down6.
3. L63,63;13019,33; quake5,wait60,quake0; camera16,34;
   P Left1/Up1/two jumps/Down,3392; S Up2; B/P Left; S explains
   immortality3393–3395, turns Right between lines; B/P nod;
   P→B2,S→P2; Init L; startEntity L; end. Caller then sets944.

No HP/death, combatant conversion, recruitment or guaranteed physical fall is
written by that sequence. Positions are scripted operands. Motion OFF sets
animation counterFF; startEntity writes counter0 after its word-ID HP guard,
not a movement, revive or join command. Waiting requests are not elapsed-time
observations. Interrupted pose/motion/quake prefixes remain possible with944
clear until the caller commits it.

The shared28 init then executes the following exact ordered chain:

|Stage|Predicate and result|
| --- | --- |
|Underground one-time block|If942 clear **and physical main0's first longword equals `$06005580`**, optionally if28 held run cs_60656 (L27,55 Up), then cs_60C42,930 held→cs_60EB2 else cs_60CA4, then set942.|
|C hide|If27 held, MoveEntityOutOfMap(C).|
|Orb hide|If944 clear, MoveEntityOutOfMap(130).|
|Priest placement|If941 held, cs_606AC sets12826,50 Down.|
|L marker|If28 held,set945.|
|Upstairs suffix|If940 held and941 clear, cs_606EA: B/P/S11,10 Up,wait1,reload current camera5,4,wait10,end; if28 held, cs_606E0 additionally L11,10 Up. Return.|

The longword comparison is two physical words: X=$0600=1536 and
Y=$5580=21888; dividing the actual384-unit coordinates gives **4,57**.
It is not a packed pair6,85 or a check of saved warp bytes. The comparison
occurs **after**530's scene/pose prefix. With clear944, a completed cliff
scene can move main away from4,57 before it is tested.28 entry bypasses that
prefix.942 has no local940/941/27 prerequisite; neither dialogue chronology
nor an assumed natural stair route can supply one. Similarly the final
940/clear941 suffix can reposition a supplied underground state upstairs.

cs_60C42 /3442–3445 installs Init L/B/P/S, sets B28,54 Left/P28,55 Left/
S27,53 Left/priest26,50 Down/C25,53 Right; sets camera neutral; reloads
current map camera19,48; fadeInB,wait60; S concludes the journey and C asks
about Zeon's revival. It creates no entities, fresh setup or membership.

|930|Complete underground continuation|
| --- | --- |
|Clear, cs_60CA4 /3446–3467|S nods, seeks Holy Sword;wait20. C recalls a book, shivers/Left/wait40; S shivers/pleads; C Right asks time then Left/wait60. C speed20; two Left2/faceLeft20/faceRight20/Right2/wait20 searches separated by Left/wait20, then Down/wait40/nod. C speed32,Left2/Up2,wait30; shivers twice,wait30,shiver,wait30; wrong book. Speed10,Down2/wait80; Init/two jumps; speed48,Up2,wait30; two shivers,wait40,shiver,wait40,two jumps; found book. S speed64,Up1/Left2; C Right/nod, explains missing passage. Init C,Down1/Right; Init S/JumpRight/wait10/speed48/Left2; S laments,Down/wait40. P Left4/Up1, C Down; P encourages belief, S nods. Init S,Right4/Down; B Up; S asks B to find sword/door, B nods.128 Down1,S Up,C Right; priest advises jewel; P→B2,S→P2;128 Up1/Down; end.|
|Held, cs_60EB2 /3468–3470|S nods, confirms truth,wait40; S Right1/Down; B/P/L Up; S addresses B,B nods;128 Down1,S Up,C Right; priest requests return/world peace; P→B2,S→P2,L→S2;128 Up1/Down; end.|

Only the caller writes942 after the selected continuation completes. No C join
occurs in this scene and no930 write opens the tower. Joined27 is tested for
hiding only **after** the942 scene, so a supplied joined C can still receive
its unguarded action/portrait commands before that hide.

MoveEntityOutOfMap resolves the logical ID with j_GetEntityIndexForCombatant,
then SetEntityPosition to `$7000,$7000`. It preserves alias and follower list;
it is not HideEntity or LeaveForce. C can therefore be moved after being
followed, with later action services still material. Neither table omission nor
offmap placement is a guaranteed permanent actor deletion.

## Chaz join, Lemon join and changed party states

**Confirmed:** Chaz EntityEvent1 runs complete cs_60F18 /3471–3479:
request to travel; S faces C and refuses; C explains his work/Zeon;
P faces C and supports him; C nods; S agrees; **join27**; C excitement,
shiver,wait30; P warns this is no sightseeing; **C follows B place5**; end.
There is no yes/no, inventory cost, F930/941/942 prerequisite, F71 write,
immediate hide, explicit actor allocation or local joined27 guard.
Current talk may run again after join until ordinary caller state prevents it;
direct repeated callback input repeats the text/join/follow sequence.
Fresh init's27 hide is a separate later consumer.

Lemon ZoneEvent1 requires **clear28 and held27**, at anyX/Y42 after zone
marker/caller admission. It does not read530/930/940/941/942/944/945/71.
Complete cs_60708 /3513–3522:

1. L21,33 Down/Init,3513; camera16,28; Init P/S;
   B21,39 Up/P22,39 Up/S20,39 Up;3514; L Down2;
   camera16,33; L requests membership3515;wait50; S3516;wait30;
   L3517–3519 about trust, fighting and his wish;wait40.
2. S Right,B/P Left; S endorses L3520; B/P nod;wait20;
   B Up3 (nonwaiting),P Up4/Left (nonwaiting),S Up4/Right (waiting);
   S invites L3521; **join28**; L two Jump scripts; thanks3522;
   **P→B2,S→P2,L→S2**; end.
3. Only after script return does ZoneEvent1 set **71 then945**. It does not
   set944 or repair an absent actor. A joined28 caller skips the whole zone,
   so interrupted postjoin following/71/945 is not automatically retried.

`csc08_joinForce` clears speech, waits scrolling, reads the selected word,
uses normal MUSIC_JOIN for27/28, calls j_JoinForce, reads current class,
sets NAME/CLASS for fixed446, then fade/music/input service, closes text,
waits10 and returns. The special128 Sarah/Chester and sad-join branches do
not apply. Fixed446 does not consume the scene's next-text cursor.

JoinForce sets membership27/28, calls UpdateForce across30 allies and builds
joined/active/reserve lists, then only below capacity12 writes active59/60
through JoinBattleParty. It does not clear preheld active flags at capacity,
or rebuild the cached lists after its new active write. A full party can gain
a reserve recruit; a supplied preactive member is counted when joined becomes
held. Join does not heal, reset HP/class, equip a weapon, replace inventory,
create the field actor or guarantee deployment/following. Repeated join can
change capacity admission when live state changed, even though membership
was already held.

`followEntity`'s real encoding is three words. csc2C first reads the
**nonadvancing high byte** of follower word00xx for its HP helper: selected
27/28/P/S all test ally0 HP, not the follower's own HP. Zero B HP skips the
command; live B with dead C/L can still install it. It resolves both existing
aliases and indexes table_FollowerPositions; place2→(-24,0), place5→(-24,-24)
in follower offset units, not tile-coordinate differences.

AddFollower installs/copies a42-byte eas_Follower1 into the walking buffer,
patches physical leader/offsets and advances buffer42 **before** scanning
EXPLORATION_ENTITIES to suppress duplicate append. Repeated calls replace
the script even for an existing entry. No missing-actor, list-capacity or buffer
guard guarantees safe rejection. The template's speed32, obstruction/collision
flags and follow/wait loop are action state, not proof of completed movement.
setPos/setFacing instead check their byte actor's HP; startEntity checks its
word actor. Init/custom/entityActions/priority and shiver have their own
consumers and no equivalent blanket HP filter. Init can overwrite following
even when a later dead-actor setPos skips.

The accepted [Map41 alternate](./battle39-outcomes-and-force-sword-route.md#map41-opening-and-lemon-recruitment)
requires its actual selected Force Sword item callback/930 branches and has
its own join/follow sequence. Yeel's join has no sword possession/use check.
Held28 from either route selects Yeel28 and causes init945; held71 remains
a separate follower declaration. No automatic consequence for every other
entrance or full-game recruit census is inferred.

## Text controls and scene services

**Confirmed:** keyed `data/scripting/text/gamescript.txt`, not adjacent comments,
supplies the paraphrases above. Scene cursor ranges are3388–3395,
3396–3425,3426–3433,3434–3441,3442–3445,3446–3467,3468–3470,
3471–3479,3480–3482 and3513–3522. Every scene text in those ranges ends
W1 except the following exact W2 IDs:
3390,3393,3394,3401,3407,3412,3420,3423,3424,3428,3436,3443,
3444,3447,3459,3461,3463,3465,3473,3517,3518.
These W2 records and all W1 records retain their actual keyed N/name controls;
paraphrasing a line does not change the speaker/command or imply a gate.

Other exact controls:3483/0D9B has N,W1;3484–3487 have N,W1;
423/01A7 has NAME,N,W2,CLEAR;427/01AB has NAME,N,W2,CLEAR;
429/01AD has NAME,N,W2,CLEAR;4082/0FF2 has W1;
446/01BE has NAME,CLASS,N and no inline W token. Selected scene names include
NAME;27, NAME;28, NAME;7 and LEADER. Narration usesFF/ENTITY_NONE;
ordinary dialogue flags0/80 are actual operands, not inferred player identity.
nextText leaves the window available; nextSingleText also performs its own
close/wait work. Inline W1/W2 and script-level wait/window commands are distinct.

Action requests preserve their native meanings: eas_Init resets speed/flags/
orientation/immersed/size and idles; eas_Immersed establishes an immersed pose;
InitFixedSprite does not make the orb a gameplay join trigger. csc15 installs
the selected action pointer and physical-slot timer, then waits for eas_Idle
when requested. Custom streams end8080; entityActions lower signed relative
moves and may overlap when nonwaiting. Original relative motion has no0..63
clamp. shiver temporarily alters counter/size/orientation flag, priority is
a sprite ordering bucket, and camera destinations are service operands.
Script reload detaches view, disables/re-enables VInts/display and calls
LoadMap with current-map sentinel; it neither runs CheckBattle nor rebuilds
population/calls ordinary init. Source requests do not establish presentation,
timing, controller completion or natural reachability.

## Revisit, interruption and whole changed-state audit

**Confirmed conditional consequences**, assuming each named reached service
returns; no transaction or save guarantee is added:

| Supplied/change/prefix | Local consequence |
| --- | --- |
|650/530/28 combinations|Last held selector wins on every callback; flag changes alone do not rebuild actors. Default/650 are earlier owners, not a compulsory path into530.|
|940 held/clear89 or repeated piano|Held940 skips prompt; clear940 prompts anew and consumes actual answer89. Refusal writes no940. Copy-before940 interruptions can leave divergent live layout.|
|940 clear or941 held at zone11,8|No church-entry script; no fallthrough to another zone.941 clear after partial scene permits repeat from its current mutated actor state.|
|930 flips between entry and underground init|Each independent selector reads current930 at its own call; two scenes can select different alternatives. Neither writes930.|
|942 held or main not exact4,57|Skip underground story; suffix/hides/placements still run. First530 cliff prefix can change main before this compare.|
|944 clear in530 versus28 entry|530 runs cliff and commits944 after return;28 bypasses it and hides130 position-only if944 remains clear. Neither implies an unobservable narrative prerequisite.|
|27 held before init or repeated direct C talk|Init moves C out after any942 scene; current actor/alias can survive. C handler itself repeats join/text/follow; membership is not an automatic local no-op.|
|28 held/945 clear|Entity130 gives hole; description gives pose/L text/set945; init sets945. Zone42 does not retry join/follow/71.|
|28 clear/945 held|Entity130 silent; description repeats L text; zone42 still admits when27 held.945 does not mean membership.|
|71 held/28 clear or capacity full|Follower L can exist before membership; fixed L may be suppressed. Joined/reserve/active/following remain separate; preactive59/60 and cached list timing retain source behavior.|
|Join completed before text/follow/71/945|Membership and possibly active flag are committed; later effects can be absent. No rollback. C direct replay remains possible; L's28 gate prevents ordinary zone retry.|
|Fresh explicit05|Rebuild followers/fixed list from held flags, clear256..383, reload base layout and940 copy, run selected init. C has no ordinary follower row and27 init hides it; L's ordinary following depends71, not merely28.|
|CURRENT05 warp|Retains population/aliases/follower scripts and working layout through ordinary property/load/init effects; live28 selection can apply to older530 actors. Named init scripts may replace positions/actions/follows.|
|Scene reload|Retains current actors/layout, does not recursively run init or clear flags. Flag-copy restoration needs fresh base-layout load.|
|Missing/overwritten alias, HP0 or changed class|Resolve real physical alias and each handler's distinct guard; no inferred actor allocation, class reset, resurrection or universal follower HP check. Full/corrupt lists/buffers have no safe-capacity guarantee.|

All six exits remain declarations across setups; item/description flags do not
remove their rows. Natural terrain, pending event timing, original services and
Map66 battle selection still govern actual return. No selected callback calls
SaveGame/LoadGame; accepted in-process helper evidence does not establish
physical or cross-process durability for this excursion.

## Actual H2, Python, Content, Domain and Application boundaries

**Confirmed by actual source inspection:** retained
[map init H2](../../src/sf2tool/h2/map_init.py) preserves source bodies,
embedded/standalone script profiles, operations/calls and setup/init route joins;
[map scripts](../../src/sf2tool/h2/map_scripts.py) preserves complete program
operations; [script engine H2](../../src/sf2tool/h2/map_script_engine.py)
preserves macro/opcode/handler facts. Parsed evidence does not execute
descriptions, choose a natural route or validate a playable Yeel package.
No new parser, lowering result or pair reconstruction is claimed.

The [coordinator](../../src/sf2tool/remake_exploration_content.py) prepares
selected verified source/canonical content; runtime Content reads prepared
packages. Current [capability ownership](../../remake/docs/capability-status.md#milestone-and-deferred-surfaces)
keeps later Yeel, item acquisition, area descriptions/setup-event selection and
save/load Unsupported. Early Yeel behavior does not supply these late bindings.

| Actual consumer | Reusable expression and selected limitation |
| --- | --- |
|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py)|Typed flags/calls/text/yesNo/join/follow/actions/camera/priority/quake/reload exist. cmpi.l against physical ENTITY_DATA plus beq/bne lowers to signed raw coordinate words1536/21888 on entity0 (4,57 only after division by384). Recognition does not bind this map or its actors.|
|Same compiler: native boundary|setBlocks has no typed case and reaches native-call; supporting static layout events does not implement the piano command. Church/shop jsr and CURRENT_SHOP_INDEX byte store are not typed service transactions. Description compiler/caller/D6 binding is absent from generic entity/zone conversion. Speech/name clearing must match actual typed cases; arbitrary clr.w is not silently supported.|
|[Map preparation](../../src/sf2tool/remake_exploration_maps.py)|Selects setup at preparation from selected_setups/default; imports entity and zone tables for selected eventMaps, onLoad init, layoutEvents and ordinary warps. It does not reproduce live last-matching05 selection on every callback, s4 function/handled semantics or s5 item routing.64-coordinate warp rows are retained as warp-frontier, not admitted ordinary transfer.|
|[Join producer](../../src/sf2tool/remake_exploration_joins.py)|join27/28 can emit camera wait, JOIN sound, join-party, fixed446/nameMember, sound/previous-music/input/close/wait. This is an expression boundary, not proof of C/L resource, portrait/class or text acceptance.|
|[Content reader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [StoryProgram](../../remake/src/Sf2.Remake.Application/Content/Scenarios/StoryProgram.cs)|Read typed branch-coordinates, yes-no, reload-scene-map, join-party, follow and action shapes. native-call becomes UnsupportedInstruction; native actions become UnsupportedEntityAction. Validated types do not implement original aliases, missing actors or HP quirks.|
|[ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Native raises program-opcode before advancing that instruction; completed prefixes remain. Follow resolves actual admitted entities and installs motion without the original high-byte HP precheck. Counter updates likewise do not inherit startEntity's source HP guard. Ordinary motion/resources/services require real bindings/completion.|
|[MapPartyMembership](../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)|Rebuild/Join model membership, active/reserve, capacity and pre-new-active cached-list timing. ProgramRunner.JoinResources additionally requires one admitted ally binding and start when encounters exist, and adds admitted resources only once. No late C/L admission is established.|
|[MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs), [MapEventDispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs)|Prepared preserve/rebuild and scene reload mechanisms exist for admitted content. They do not establish late setup selection, source low-byte caller states, description/menu routing, offmap/alias consequences or a naturally usable return.|
|[Domain copies](../../remake/src/Sf2.Remake.Domain/Maps/MapBlockCopyAction.cs)|Static flag/door/roof layoutEvents have reducers and0..63 cell validation. They are separate from scripted csc34 working-layout copy/update bits and the declared64 warp pair. No automatic piano support follows.|

Future integration needs explicit trusted map/material/text resources, late
setup/description/menu/zone/init bindings, actor aliases/HP/party behavior,
scripted copy and reload lifetimes, and reached state/control observations.
Neither static parsing nor a typed suffix bypasses unsupported reached work.
This research does not advance the English design/runtime/engine frontier.

## Acceptance and retained Unknowns

The acceptance boundary is the complete selected source→actual consumer→prose
review across six Issue groups and all three owned paths, not a label inventory.
Direct UTF-8/LF, links/anchors/fences, scope/private-boundary and unchanged-ledger
table checks complement that semantic audit. Clean committed planner selection
and actual exact-head scoped CI are recorded in the Issue/local handoff under
the current documentation verification policy. No normal/full, SDK, engine,
Godot, H3/H4, emulator, screenshots, lowering or historical rerun belongs here.

**Unknown:** natural entrance order/terrain/caller states, actual physical aliases,
party/HP/class, prompt/service/wait/presentation completion and interruption,
outgoing battle interception, save/load and physical durability. These remain
bounded questions, not a runtime-acquisition queue. Only late Yeel's selected
static chapter closes; other revisits, all-recruits and Epic1053 remain open.
Retained failure/correction records remain with their prior Issue handoffs;
no completed failure is relabelled interrupted, passed or repaired here.
