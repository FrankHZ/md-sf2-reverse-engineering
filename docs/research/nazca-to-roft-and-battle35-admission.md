# Nazca to Roft, Battle35 admission and the town greeting

Evidence date: 2026-10-20. **Confirmed (static):** Map75's west scroll
loads Map76/Roft region with retained population and without battle admission.
Map76's admitted default zone, while F535 is clear, sets F435 and posts a
CURRENT no-scroll reload; the subsequent ordered battle scan can select35.
Its separate town warp selects Map38/Roft, whose initializer conditionally
creates a Zynk actor and runs a five-message greeting. Neither table order
nor these scripts proves that town entry or Battle35 must happen first.
**Stop before Battle35 initialization and after Map38 init return.**
Natural routes, completed services, usable control and persistence remain
**Unknown**.

## Agreement and evidence route

Standard rigor for [Issue #1116](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1116)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
explain the regional transition, battle admission and town-entry dependencies
for later content decisions. Accepted base:
`cb64e45f2860f99e23016c246b38e1f9693dfbe2`. Own this document and navigation
only in the [optional Dojo owner](./dojo-exploration-and-sheela-recruitment.md).
No stacked dependency, registry/index/schema/fixture/tool/engine change.
Battle35 roster/AI/turns/outcomes, Roft interactions/searches/recruitment and
Map77's body are excluded. A successful conversion is not playable acceptance.

Original paths are relative to `disasm/` in read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The baseline USA ROM identity remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
this adds no ROM/runtime equality claim. `sf2enums.asm`/`sf2const.asm`
own constants and RAM widths. `entity/` and `map/` below abbreviate
`code/common/scripting/`; `exploration/` abbreviates
`code/gameflow/exploration/`. Fnnn denotes flag nnn, ordinals are zero-based,
and address intervals are half-open.

Selected sources: Map75 warp row5; all Map76 regional/setup declarations
(`$C4F44..$C4FB6`, animation `$C5D06..$C5D1C`, setup `$50A0A..$50A50`);
Battle35's row `$7B2B..$7B32` in the complete battle-coordinate table;
Map38 areas, complete entity table `$5DAFC..$5DB76` and complete initializer
with scripts `$5DD58..$5DE2E`. Shared consumers are named at each seam.
Reuse the accepted [scroll/request owner](./moun-to-nazca-and-battle34-admission.md),
[partial-reload owner](./pacalon-departure-to-battle27-admission.md#callback-order-and-partial-reload-overwrite),
[post-crash Map75 context](./post-crash-ship-and-nazca-overworld-return.md#map55-initializer-and-the-resulting-map75-context)
and [follower/action owner](./nazca-voyage-and-map59-arrival.md).
These links do not require another whole-route audit.

Reuse the immutable18,285,979-byte init/script pair with provenance
`local/issue1005/pair.txt` and the registered canonical map export, checked
against `manifests/extractions/canonical-map-import.json` before conversion.
No export or source copy is generated. Selected linear reads, in-memory
compilation and unique small ignored receipts have a one minute/512MiB
budget per direct audit; peak memory is unmeasured. No acquisition/network
payload or model batch is needed. Reassess material growth before expanding.

## West scroll and the regional declarations

**Confirmed:** `map75/6-warp-events.asm` row5 `$C490C..$C4914` is
`(0,255)`, scroll Left `$12`, map76, destination `(62,255)`, facing Left2
plus zero padding. `sf2mapmacros.asm` emits eight-byte warp rows.
`entity/entityscriptengine_2.asm:esc02_controlCharacter` calls
`exploration/exploration.asm:WarpIfSetAtPoint` only through the candidate
movement/layout `$1000` marker branch. The helper checks the terminator
first, divides signed pixel coordinates by MAP_TILE_SIZE384, treats negative
source bytes as wildcards, and chooses the first match. Source255 is a
wildcard; destination255 means preserve at its separate consumer. Spawning
or standing at the edge is not itself a trigger or proof of passable terrain.

The producer writes event word1, PARAM1..4=`$12,76,62,255`, PARAM5/facing
word and WARP_SFX. WaitForEvent prioritizes this mutable slot over A/C;
ProcessMapEvent clears the type word before dispatch. With intact nonzero
PARAM1, ProcessMapEventType1_Warp idles physical0 and writes CURRENT_MAP76.
Its special71/F530 substitution does not apply and it does not call SwitchMap.
ProcessMapTransition loads destination tilesets1/3/4 when their signed IDs
are nonnegative (here101/103/104), destination blocks/layout and flag/item
processing, and an existing battle crop if CURRENT_BATTLE is not255.
It reads/scales the first area rectangle, switches VInt scrolling services,
and dispatches the then-live PARAM1&3: intact$12 selects Left.

The Left consumer rebuilds plane columns, plane X counters/positions and
VDP sprite projections through sub_29A2/DMA/VInt services. This is real map
work, not a visual-only transition. It does not call LoadMapArea: declared
music, palette, full layer/parallax/autoscroll/animation and roof setup are
not freshly installed here. No fresh population, mapsprite load, temporary
clear, F80 write, init, healing, step subtraction, CURRENT_BATTLE reset or
MainLoop/CheckBattle occurs. Map76 callbacks can be selected live afterward
despite the retained Map75 actors. Its empty fixed table does not erase them.

After ProcessMapTransition, X is read into d0.b **without clearing its word**.
For return word R, X/XDEST become
`(((R & $FF00) | $3E) * 384) mod 65536`; a clean high byte gives62 tiles.
Negative X would skip both writes. The code then clears d0.w before Y;
FF is negative and retains the actual post-transition Y/YDEST independently.
PARAM5 does not set facing in this branch. MakeEntityIdle only replaces
the script pointer, not speed/velocity fields. No explicit speed reset is
part of the transition; subsequent WaitForEvent can install the selected
control script, whose ordinary/raft/Caravan variants request speed32,32.
Installation and completed execution are distinct.

The caller samples current main position/facing before DeclareRaftEntity.
Numeric player mode2 returns immediately, mode1 skips follower relocation,
and other modes move
the listed physical followers to that position/destination. F64 and stored
RAFT_MAP/X/Y separately govern physical raft31. Aliases/scripts/list order
remain live; logical FOLLOWER_B31 is not that physical slot. WARP_SFX remains
for a later consumer. Interrupted or replaced parameters can change these
results; there is no immutable request snapshot or automatic redispatch if
PARAM1 changes after branch selection.

Map76 has one unflagged msMap setup and the six pointer-table entries in
Entities/EntityEvents/ZoneEvents/AreaDescriptions/Section5/InitFunction order.
Its complete local declarations are:

| Surface | Exact bounded content |
| --- | --- |
|00-tilesets|Palette0; tilesets1..5=101,102,103,104,105.|
|2-areas `$C4F72..$C4F92`|One area: main0,0 through63,63; secondary foreground/background0,0; both parallax256,256; both autoscroll0,0; type254; MUSIC_MAIN_THEME; terminator.|
|3-flag /4-step /5-roof|Each only endWord; no local block-copy rows.|
|7-chest /8-other items|Each only endWord; no declared local rewards.|
|9-animations|Animation111,96; two entries0,48,$350,20 then48,48,$350,20; terminator. Scroll does not install this animation table.|
|s1_entities|Only msEntitiesEnd; no fixed or walking records. Fresh common population can still contain player/followers/raft.|
|s2_entityevents|Only default Map76_DefaultEntityEvent→rts; no explicit entity/services rows.|
|s3_zoneevents|Only default Map76_DefaultZoneEvent, detailed below; no coordinate-specific row. nullsub_50A3C is rts and is not a table-selected zone.|
|s4_descriptions /s5_itemevents /s6_initfunction|RTS description; only default item callback→rts; RTS initializer. No shop/church/scene/recruitment command.|

The entire `6-warp-events.asm` has three rows plus endWord:

| Row / address | Source | Mode / destination / facing |
| --- | --- | --- |
|0 /$C4F98|8,47|No-scroll0 /38,14,30 /Up1|
|1 /$C4FA0|0,255|Left$12 /77,62,255 /Left2|
|2 /$C4FA8|63,255|Right$10 /75,1,255 /Right0|

Row1 stops at the Galam-region77 boundary; row2 returns to the accepted
Map75 layout context. Both scroll branches retain actors/temporary state,
skip battle scanning/init and preserve Y as above; reverse X uses1 and
the same uncleared-high-byte rule. Loading75 can resample its F999 layout
copies without a fresh setup. These rows prove no natural path between
the three markers or connection to Map77's internal events.

## Zone request and the Battle35 admission boundary

**Confirmed:** the complete default callback is chkFlg535; bne return_50A3A;
setFlg435; move.l #MAP_EVENT_RELOADMAP to MAP_EVENT_TYPE; rts. It never
clears435 or writes535, checks no location/party/HP/join/story prerequisite,
and performs no random-battle draw or scene.

The ordinary movement producer requires loaded layout kind `$1400` after
candidate movement/obstruction checks. It posts event **word6**, the
candidate tile X **word** at PARAM1, and tile Y **word** at PARAM3.
Other callers/modes are not established by standing on a coordinate.
ProcessMapEvent clears type6, then ProcessMapEventType6_ZoneEvent installs
eas_Init on physical0, reads these two words and calls RunMapSetupZoneEvent.
That wrapper selects live76 setup; its first FD default row selects the
callback regardless of X/Y. After callback it closes portrait/text, waits
one VInt and for physical0 to stop, restores registers and returns.
Callback return and cleanup do not themselves run CheckBattle.

`MAP_EVENT_TYPE=$FFA84A` is a word; PARAM1..5 begin$FFA84C..$FFA850.
MAP_EVENT_RELOADMAP=$000100FF is a **longword**: it writes event1,
PARAM1=00/no-scroll, PARAM2=FF/CURRENT. It leaves PARAM3/4, PARAM5,
padding and WARP_SFX untouched. With intact ordinary zone Y=y in0..63,
PARAM3/4 retain word00yy: the later warp reads destination **X0,Yy**,
not two preserve sentinels. PARAM5/facing is stale. Changed Y high bytes,
later request writers or an interrupted callback can produce other values.
For example supplied zone(20,42) produces reload X0/Y42, with facing from
whatever remains at PARAM5. This is source arithmetic, not an observed route.

On the next exploration iteration the pending event1 skips control-script
installation. The no-scroll consumer submits/clears WARP_SFX, idles main,
unwinds to MainLoop, samples actual main X/Y, overlays nonnegative PARAM3/4
onto their low bytes and supplies PARAM5 as facing/d4=0. Changed sampled
high coordinate bytes survive. SwitchMap precedes CheckBattle; the complete
switch table has only3/F640→4,71/F530→78,58/F999→59, no38/75/76 replacement.
CURRENT255 resolves to live76 at CheckBattle.

`code/common/maps/getbattle.asm:CheckBattle` scans the complete45 ordered
seven-byte rows (`$7A36..$7B71`). Its **only76 row is index35**:

| Battle | Map / crop X,Y,width,height | Trigger X,Y | Unlock / completed |
| --- | --- | --- | --- |
|35 BATTLE_TO_ROFT /$7B2B|76 /16,12,48,32|255,255|435 /535|

Earlier and later rows cannot win on76: they have other map IDs. Admission
tests map equality, held400+index flag, and trigger low-byte equality or
exactFF wildcard. It does **not** test crop containment. Thus the reload's
X0 does not disqualify35. On match it writes BATTLE_AREA_X/Y/WIDTH/HEIGHT
bytes, checks completed100 above unlock and, if535 is held, clears435;
it **still returns35 on this call**. Failure restores original map operand,
returns d7=−1 and writes no crop. It never writes CURRENT_BATTLE.
MainLoop then moves d7 to d1 and calls j_BattleLoop. **Stop before that
callee's initialization**; no35 roster, activation, turn or outcome claim.

| Supplied state / seam | Conditional consequence |
| --- | --- |
|75→76 scroll with435 held, either535|No CheckBattle on this path; no automatic35, no clear435.|
|76 default zone,535 clear,435 either|Sets435 then overwrites four request bytes; repeats identically while535 stays clear.|
|76 default zone,535 held|Returns without reload or changing435. Existing435 is not repaired/cleared.|
|Explicit76 or CURRENT on live76,435 clear|No35;535 alone does not admit it.|
|Explicit76/CURRENT,435 held,535 clear|Admits35 at any trigger coordinate; crop written;435 remains held.|
|Same,535 held|Admits35 **now**, clears435; next scan with435 still clear admits none.|
|535 changes between zone and scan|Scan rereads it; newly held535 can clear435 while still admitting this35. Clearing535 alone never manufactures435.|
|Interrupt after set435 /after longword write|Unlock without reload /posted reload awaiting cleanup; no transaction, queue or rollback. A later writer may replace it.|
|Town warp while435 held|Explicit destination38 has no battle row; no35 scan on76 is implied.|
|No-battle CURRENT fallback|Exploration's common heal/fade/step subtraction still runs; no fresh population/temp clear; existing working layout retained and live init rerun.|

The CURRENT fallback's typed position/facing/properties/raft updates and
LoadMap negative path are owned by the accepted predecessors. Actual clear535
zone→intact request normally sets435 before this scan, so a no-battle fallback
there needs independently changed state/request. A fresh explicit76 instead
rebuilds common population, clears256..383, sets80 and runs its RTS init
only if admission returned no battle. Neither seam proves mandatory combat
before Roft: marker placement, collision and naturally carried state remain open.

## The separate Roft warp and fresh population

**Confirmed:** Map76 row0 publishes no-scroll38,14,30,Up with no435/535/910/
921/26/76 condition. Its actual `$1000` producer/request consumption uses
the same mutable slot. MainLoop sees explicit38 after SwitchMap; the complete
battle-coordinate table has **no38 row**, so no battle is admitted here.
Fresh exploration clears pending, subtracts20000 from STEP_COUNTER with
negative clamp, heals living/immortal allies and fades; writes CURRENT_MAP38/
CURRENT_BATTLE255, loads tilesets, rebuilds population/mapsprites, clears
temp256..383 and sets80. LoadMap/base VInts precede live init. Healing restores
living HP/MP, revives Peter7/Lemon28, retains other dead allies and does not
join26, reset his class/items or generally cure status; the Dojo owner names
the service and retained status boundary.

Map38 has one unflagged msMap route and the same six setup pointer roles.
`2-areas.asm` `$AFC16..$AFC90` has four30-byte areas plus terminator:

| Area | Main inclusive bounds | Secondary foreground / background | Type |
| --- | --- | --- | --- |
|0|0,0 through25,31|0,32 /0,0|0|
|1|27,0 through37,9|0,0 /0,0|255|
|2|27,11 through37,20|0,0 /0,0|255|
|3|39,0 through49,9|0,0 /0,0|255|

All use MUSIC_TOWN, both parallax256,256 and autoscroll0,0. LoadMap tests
view-target position against ordered bounds, falling back to first area
when unmatched. Ordinary entry14,30 selects0; greeting coordinates also
lie in0. Declaration/selection is not rendered or collision evidence.

InitializeMapEntities builds selected ordinary followers before fixed NPCs,
then player0 and raft/player-type handling. Map38 excludes the overworld
Caravan65 row but reads the complete ordinary follower table. With supplied
67/70/76 held, Peter7→physical1, FOLLOWER_B31/Astral→2, Zynk26→3; F65 alone
does not add Caravan. Other flags can add/replace duplicate aliases; later
mappings win. Missing aliases ordinarily remain0, potentially selecting
player0. Actor names in prose do not validate current alias identity.

The **complete15-record table** plus terminator is below. All fixed rows
use eas_Init except142/eas_InitFixedSprite. Walking rows use eas_Walking
with their listed origin/radius1. With n selected followers, logical128+j
maps ordinarily to physical n+1+j; NPC aliases use lookup32+j, not combatant
HP or joined flags. No table row is a fixed Zynk or a membership operation.

| Logical | X,Y / facing | Sprite / movement |
| --- | --- | --- |
|128|12,27 /Right0|YOUNG_MAN196 /fixed|
|129|22,20 /Down3|WOMAN195 /walk22,20 radius1|
|130|8,19 /Down3|YOUNG_WOMAN197 /walk8,19 radius1|
|131|11,11 /Down3|MAN194 /walk11,11 radius1|
|132|22,10 /Down3|YOUNG_MAN196 /fixed|
|133|30,5 /Right0|WORKER202 /fixed|
|134|21,3 /Down3|MAN194 /walk21,3 radius1|
|135|5,10 /Down3|YOUNG_MAN196 /walk5,10 radius1|
|136|3,16 /Right0|WORKER202 /fixed|
|137|12,3 /Right0|YOUNG_MAN196 /fixed|
|138|13,4 /Up1|YOUNG_WOMAN197 /fixed|
|139|22,27 /Left2|PRIEST192 /fixed|
|140|31,14 /Down3|MERCHANT193 /fixed|
|141|35,4 /Left2|MERCHANT193 /fixed|
|142|13,3 /Down3|MAN194 /fixed sprite|

Walking actions are ongoing random-walk requests, not prescribed trajectories
or observed RNG. eas_Init resets acceleration factors, speed32, acceleration/
deceleration and collision/obstruction/autofacing/orientation/immersion fields,
requests size24/sprite update and jumps idle. InitFixedSprite additionally
disables autofacing/motion. These init dependencies do not establish city
interactions, shops/church, Petro's later scene or recruitment.

## Independent Zynk creation and greeting branches

**Confirmed:** the complete ms_map38_InitFunction runs in this order:

1. Read921. If clear, skip to byte_5DD78. If held, **set911 first**.
2. With921 held, read joined26; held skips creation. Then read temp260;
   held also skips creation. Only921 held/26 clear/260 clear calls cs_5DD8E;
   **after its return** set260.
3. At byte_5DD78 read910. Held returns. Clear calls cs_5DD9C;
   **after its return** set910, then temp258, then return_5DD8C/rts.

No test of911 controls this initializer and no Petro death scene is called
here. 921/911/910 are outside the map temporary range;26 is joined,76 is
following,260/258 are temporary. Their narrative comments are identities,
not proof of how the caller acquired them. Fresh explicit38 clears260/258
before these tests; retained CURRENT reload does not.

cs_5DD8E's entire stream is newEntity26,11,8,Down,NOTHING_BYTE255;
setFacing26,Down; csc_end. csc2B_initializeNewEntity consumes a word selector,
four coordinate/facing/sprite bytes, supplies eas_Init and calls
entityfunctions_1.asm:InitializeNewEntity **without an HP guard**.
For this nonnegative ally26, GetAllyMapsprite replaces the supplied255 using
actual HP/class (Blue Flame at HP0 in exploration). It scans63 alias bytes
for the greatest signed nonnegative physical slot, adds1, overwrites ally
alias26, declares position/destination11,8 with zero velocity/travel/accel,
installs Init and slot-based animation/action timers, and updates properties.
There is no duplicate-alias/capacity rejection or join/stat initializer here.

Ordinary n followers plus15 NPCs gives new physical n+16. If26 already has
a follower alias because76 is held while joined26 is clear, this operation
still creates another slot and redirects26; the old slot/list entry is not
removed by this function. It does not append the new slot as a follower,
set76/26/58, heal, load/save a party or run a recruitment ceremony. The next
setFacing is separately HP-guarded and can skip for dead26. Repeating this
script directly bypasses the260 caller guard and is not a natural entry.

For the greeting, the entire source command/action order follows. B=Bowie0,
P=Peter7, F=FOLLOWER_B31; AW means entityActionsWait, including endActions.
Ordinals span the combined stream, counting nested commands/delimiters.

| Ordinals / source | Ordered commands |
| --- | --- |
|0–6 /cs_5DD9C|setPos128,14,26,Down; textCursor2132; csWait1; setPos B14,28/Up; P13,29/Up; F14,29/Up; jumpIfFlagSet76,cs_5DE22.|
|Optional /cs_5DE22|setPos26,15,29/Up; jump cs_5DDC2. With76 clear, fall through directly to cs_5DDC2.|
|7–11 /cs_5DDC2|playSound MUSIC_TOWN; fadeInB; camera entity128; setActscriptWait128,eas_Jump; single2132/128.|
|12–20|AW128 Left2,end; AW128 Up1,end; AW128 Left2,end.|
|21–26|face B,P,F Left (three calls); single2133/P; face128 Right; single2134/128.|
|27–32|AW128 Down2,end; AW128 Right2,end.|
|33–35|single2135/128; single2136/128; csc_end.|

Each single uses nextSingleText flags00; it opens/resolves the actor portrait/
speech, waits for view, advances the text cursor and closes portrait/text
with the shared10-tick sleep. All five selected text IDs and ordered tokens:

| ID | Speaker/mode | Short paraphrase | Ordered tokens |
| --- | --- | --- | --- |
|2132|128/single|Warns of soldiers|W1|
|2133|P/single|Denies Galam identity|N,W1|
|2134|128/single|Questions correction|W1|
|2135|128/single|Admits mistake|W2|
|2136|128/single|Recognizes honest intent|N,W1|

N/W1/W2 spell `{N}`/`{W1}`/`{W2}` in
`data/scripting/text/gamescript.txt`. No omitted or reordered tokens;
original prose remains private. Zynk/F have no selected dialogue here.

csc19/setPos and csc23/setFacing call the ally HP helper: below30 at HP0
skip their operation/operands;31 and128 bypass that check. Actions/camera/
text do not inherit it. Fresh Peter is revived, but supplied retained dead
Peter/Bowie or dead Zynk can skip guarded operations. No command validates
that F is Astral or manufactures a missing P/F/26 alias for the greeting.
F76 alone selects its optional Zynk position, independently of921/26/260.

csc15 and csc2D resolve the physical alias, seed its action timer from slot
and poll **eas_Idle's address** for waiting forms; csc2D clears FLAGS_A bits5/6
to enable map/entity collision tests. eas_Jump disables autofacing,
requests relative0,−1/wait5/0,1/wait5/0,0, reenables autofacing and jumps idle.
None of these waits HP-skips. Blockage/interruption can prevent completion.
Nominal completed unblocked128 positions are14,26→12,26→12,25→10,25→10,27→12,27;
these are source arithmetic only, not observed motion or guaranteed final facing.

ExecuteMapScript/csc_end restores registers and clears view scroll speed;
it neither joins nor repositions the party into a follower chain. Trap6
activates entity updates. After greeting return the caller writes910 then258;
an interruption can retain earlier actor/text/flag prefixes without rollback.
RunMapSetupInitFunction returns to exploration's conditional music/fade and
WaitForEvent. With no pending request that installs physical0's control script
and restores its view target; a pending request takes priority. This is a
control route, not proof of completed fade/text/audio or usable player control.

| Changed state | Result at the initializer |
| --- | --- |
|921 clear,910 clear|Greeting only; no911/260 write or dynamic26.|
|921 held,26 held|Set911; skip creation regardless260; greeting still governed solely by910.|
|921 held,26 clear,260 clear|Create26 then set260; greeting follows if910 clear. HP0 does not prevent creation.|
|Same but260 held|Set911, skip creation; no automatic repair of missing26.|
|910 held|Suppress greeting/258 write; preceding911/creation work can still occur.|
|76 held,26 absent/dead|Greeting still requests26 position; absent alias may select0, HP0 skips it. No recruitment inferred.|
|Fresh reentry /retained CURRENT reload|Fresh clears260/258, can recreate26 under921/26; retained260 suppresses creation. Persistent910 suppresses greeting in either.|
|Interrupted before910 /between910 and258|Greeting can replay /910 can suppress it with258 still clear. Temp flag and scene completion are not atomic.|

## Actual compiler/content joins and current implementation boundary

**Confirmed (direct offline execution):** fresh OriginalPrograms instances
register every existing defining file for Map76 and38, including all local
continuations/helpers, before compiling each selected root. No failed cache
or synthetic source data is reused. Observed output:

| Actual selected root/conversion | Result and limit |
| --- | --- |
|76 entity/description/item/init roots; return_50A3A|Each complete one end instruction.|
|Map76_DefaultZoneEvent|3 instructions: branch535, set435, **native move.l**; return target one end. Native terminates this compiled block without proving the original write/return.|
|ms_map38_InitFunction|7 instructions with byte_5DD78 five, return_5DD8C one; all local references resolve.|
|cs_5DD8E|3 instructions: **native newEntity**, typed face26, end-map-script. Ally26 fails the compiler's admitted128..159 newEntity range; actual source creation is not implemented by that emission.|
|cs_5DD9C /cs_5DE22 /cs_5DDC2|8 /2 /44 instructions; complete fallthrough/branch/helper labels; no native in these selected bodies. Init/InitFixedSprite/Jump actions lower to14/16/9 operations respectively.|
|Event-enabled compile_maps76, registered canonical|Returns one map, zero fixed entities,7 programs; onLoad=ms-map76-initfunction, default source-zone plus interaction, ordinary rebuild warp38, scroll frontier programs for77/75. No battle route supplied, battle=null.|
|Event-enabled compile_maps38, registered canonical|Returns one map,15 fixed-table entities,43 programs, onLoad=ms-map38-initfunction. Conversion also visits city events; their behavior is outside this audit. The selected native creation remains and no successful playable town package is claimed.|

Counts are compiler instructions, not source commands/frames/completed services.
The production [compiler](../../src/sf2tool/remake_exploration_programs.py)
and [map conversion](../../src/sf2tool/remake_exploration_maps.py) are unchanged.
The registered source/canonical identities reach these real outcomes.

[ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)
turns native-call into UnsupportedInstruction;
[ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)
stops Unsupported at program-opcode. Thus clear535 reaches set435 before an
unsupported reload write;921 held/26 clear/260 clear reaches unsupported
creation before260/greeting. Held535 can return and skipped creation can reach
typed greeting, but neither establishes original timing, resources or guards.
Lowered position/facing lack the source ally HP prechecks. Logical actor
allocation, missing/duplicate aliases, HP/class sprite selection, native
creation and actual service/material binding require independent support.

[MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)
supports ordinary Rebuild and same-map Preserve; it does not implement this
retained cross-map scroll/VInt/resource lifetime. Its ReloadScene replaces
working layout, unlike original negative LoadMap. The ordinary38 warp output
does not express the partial mutable reload packet or original MainLoop scan.
Battle routes can be supplied separately to conversion, but none was supplied
here; no35 binding is established. The
[private battle reader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)
still admits encounters1/2/3 only. No join instruction occurs in either
selected initializer/scene; current join support does not turn actor26 into a
Force member. Godot projection is not evidence of original scene completion.
No SDK/reader/engine/Godot execution occurred.

The minimum later gaps are the horizontal scroll lifetime, actual zone reload
and battle35 admission binding, ally newEntity/HP/alias parity and reached
town services/materials. This research authorizes none of those repairs or a
new general exploration platform.

## Reproduction, preservation and Unknowns

Load the current configuration in the launching process; source root contains
sf2enums.asm/code/data, and canonical selects the registered existing export:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1116/review.py <registered-source-root> --canonical <registered-canonical-export> --committed --receipt local/issue1116/direct-reproduction-01.json
uv run sf2 verify plan --base origin/main --head HEAD
```

Choose an unused ignored receipt; --canonical/--receipt are required and
existing outputs are rejected. The local direct recipe checks full selected
tables/commands, constants/use sites, request/consumer order, source-arithmetic
changed states, text IDs/speakers/tokens and actual compiler/content results;
pair/provenance/canonical unchanged; two paths/navigation-only, links, UTF-8/LF
and private boundary. Clean committed planner and exact-head scoped CI are
separate receipts. Suggested planner gates are not executed passes. No normal/
full/old comparison/SDK/Godot/emulator/H3/H4/screenshots or new maintained
parser/schema/fixture/platform is introduced or run.

Preserve #1114's eas_5FFC4/5FFC8 and compile_maps45 refusal, native/join-resource
limits, original Lemon29 error corrected to28 and offset2=(0,24) error corrected
to(−24,0), including the original missed checks and correction receipts in
its ignored handoff.
No full old audit rerun is implied. Preserve #1112's complete-import @Return
partial, native crew/RNG/mapLoad and ReloadScene working-layout limits;
#1110/#1108/#1106 named failures remain in their accepted owners. Twelve
completed old-comparison failures are retained, not interrupted or a rerun queue.
The new successful conversions do not close any of those distinct boundaries.

**Unknown:** natural edge/zone/town/exit reach and route ordering, binary
layout/collision/markers, actual HP/classes/status/items/party/follower/alias/
player/raft state, request delivery and interference; movement/waits, service
scheduling, VInt/DMA/scroll/palette/sprite/camera/text/portrait/audio/rendering
completion/timing; usable control and save/load persistence. These questions
are not an automatic runtime-acquisition queue. Static evidence does not make
the Dojo mandatory, Roft entry a Battle35 reward or Battle35 a Roft prerequisite.

Freeze a clean committed/pushed Draft PR with exact head/tree/base, direct
checks/planner/actual CI, preserved failures/Unknowns and stopped process state;
Project Review / Verify and successful main-gate notification precede stop.
Independent acceptance/merge stays with main-gate; no next Issue, merge,
cleanup or archive.
