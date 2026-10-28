# Mitula return and Skreech recruitment

Evidence date: 2026-10-20. **Confirmed (static):** the accepted return tables
conditionally connect Map1, Map0, Map35 and Tristan15. Tristan's live F530
setup exposes the F724/!F820 zone callback; scene return sets820 after join20,
three follow requests and layer reset. **Inferred:** the dialogue identifies
Skreech as the rescued Bedoe child and presents his offer to accompany Bowie.
**Unknown:** natural traversal, supplied actors/flags/HP, completed movement,
presentation/control and save/load persistence. This is research data for
design/import, not an engine-ready route or recruitment implementation.

## Agreement and provenance

Standard rigor for [Issue #1072](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1072)
under [epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
compose the conditional Map1 -> Map0 -> Map35 -> Map15 return and complete
Skreech scene for later design/import. Setup F530, rescue F724, completion
F820, force/active membership and temporary follower/flight state have separate
authorities. Accepted base is `520f020f6ceee2b1adee5312ccc8de286dfecbbe`.
Ownership is this document, one navigation link from the
[Mitula exploration owner](./mitula-shrine-exploration-and-taya-recruitment.md)
and only the deferred scene/readiness pointers in the
[Tristan owner](./tristan-entry-interactions-and-exits.md). No shared-index,
engine, tooling, schema, fixture, design-guide or translation writes.

The plan is to join the accepted return tables/callers, inspect the complete
scene/text and actual macro/dispatcher/service consumers, review independent
changed states and current preparer/Content/Application/Godot support, then
check prose, UTF-8, links, private boundaries and exact owned paths. Stop at
Tristan's existing south-exit Map74 admission alternatives; no Map74 body,
Pacalon/Battle31/32, Nazca/ship, runtime capture or new collection.

Source paths are relative to `disasm/` at clean read-only pinned
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Existing provenance binds USA ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM/H1/runtime equality is claimed. Constants and RAM come from
`sf2enums.asm`/`sf2const.asm`; encoding from `sf2cutscenemacros.asm`,
`sf2mapmacros.asm` and `sf2mapsetupmacros.asm`.
Fnnn means flag nnn; indices are zero-based and extents half-open.

Reuse the unchanged18,285,979-byte init/script pair and accepted0.083s
provenance in `local/issue1005/pair.txt`. Linear bounded source/consumer reads
and a direct check are budgeted below one minute/512MiB with small ignored
receipts in `local/issue1072/`. No regeneration, binary extraction, new
verification platform or tests of review programs. Reassess contradictions
or material supporting-tool growth. Documentation scope uses direct checks,
a clean committed verification plan and actual exact-head scoped CI; it skips
blanket Python/.NET/Godot/H3/H4 suites. ADR0004 applies proportionally.

## Conditional return and destination boundaries

**Confirmed:** reuse the complete tables and actual request/caller joins in
the [Mitula exploration](./mitula-shrine-exploration-and-taya-recruitment.md#six-warps-and-destination-admission),
[exterior route](./battle29-outcomes-and-shrine-exterior-route.md#four-warp-requests-and-actual-destination-selectors),
[cave route](./tristan-caves-and-shrine-exterior-admission.md#twelve-directed-warp-requests)
and [Tristan exit](./tristan-entry-interactions-and-exits.md#exit-requests-and-ordered-destination-admission)
owners. The following composes their relevant edges in request order; it
does not duplicate their resource/population/table inventories or assert
that arrival coordinates reach the next trigger.

| Source / ordered rows | Trigger -> requested destination / facing | Admission and next setup boundary |
| --- | --- | --- |
|Map1 /4 then5|`(10,32)` ->0 `(14,12)` Down3; `(255,32)` ->0 `(15,12)` Down3|Row4 wins at X10; row5's signed-negative X is wildcard. Map0's only battle row29 needs429; held529 clears429 but still selects29 in that scan. Clear429 admits fresh Map0 exploration.|
|Map0 /2 then3|`(14,35)` ->35 `(15,23)` Down3; `(15,35)` ->35 `(16,23)` Down3|No battle row or flag-switch source35: explicit35 admits fresh cave exploration, including its void setup.|
|Map35 /0,1,2,3|`(15,31)` ->15 `(18,6)`; `(16,31)` ->15 `(19,6)`; `(24,16)` ->15 `(4,10)`; `(4,15)` ->15 `(7,3)`; all Down3|No battle row or flag-switch source15: explicit15 admits fresh Tristan exploration, selecting population and later callbacks by live530.|

All these rows are no-scroll0. The normal character candidate consumer must
reach post-door-copy layout kind `$1000` before WarpIfSetAtPoint. It signed-
divides supplied coordinate words by MAP_TILE_SIZE384, scans the live map's
table in order, tests whole-word FFFF terminator before matching, and treats
any negative source byte as wildcard. It writes event word1 before type/map/
X/Y parameters, then facing/padding word, then WARP_SFX. No match preserves
an older pending request. These are separate writes to one mutable slot;
interruption or another producer can leave/change a prefix.

WaitForEvent polls the event before A/C. ProcessMapEvent clears type before
dispatch. Unchanged type1/parameter0 consumes/clears WARP_SFX, idles main,
unwinds exploration and returns through the ordinary MainLoop
SwitchMap -> CheckBattle chain. Consumption samples main's **current signed
position words**/384; nonnegative destination bytes replace only low bytes.
Thus the nominal table coordinates above do not erase retained high bytes.
Changed slot/current map/actor state changes the result; no route snapshot
or unconditional exploration transfer is implied. SwitchMap has no0/1/15/35
source row. CheckBattle requires the matching unlock before considering
completion; a completed selected row clears its unlock and still returns
that battle on this call. Stale/rearmed429 can therefore interrupt return
with Battle29 even after529/530. Other-map unlocks do not match explicit35/15.

Map1 CURRENT room requests retain effective1; rearmed430 can admit30, even
with530 held (which clears430 during that same selection). With430 clear,
CURRENT re-entry retains population/layout/temp flags and reruns init.
Map35's CURRENT room rows4–9 likewise do not rebuild actors/layout/temp flags;
an unchanged CURRENT_MAP35 has no battle row. A changed live CURRENT_MAP
instead changes CheckBattle's effective map without a preceding substitution
of that resolved map. The cave's asymmetric rows8/9 are not inverse edges.
These branches are alternatives to fresh entry, not mandatory return steps.

Explicit admitted0/35/15 uses fresh ExplorationLoop: reset pending type,
apply the overflow-aware step-counter rule from the
[corrected entry owner](./mitula-shrine-exploration-and-taya-recruitment.md#returned-seed-fresh-entry-and-first-eventcontrol-read),
heal eligible allies, fade, assign the requested map/CURRENT_BATTLE255, load/rebuild actors and
mapsprites, clear temporary256–383/set80, load layout and run selected init.
Persistent530/724/820 and member flags survive that temporary reset.
Map0 uses its Chaos Wizard/default setup and RTS init; arriving at14,12 or
15,12 does **not** automatically invoke its zone callback. If separately
reached, that callback's801 gate and scene840 effects remain in the accepted
exterior owner; this return does not re-audit or assume its completion.
Map35 has void fixed setup but still builds ordinary flag-followers/main;
void init does not omit caller work. Tristan selects its current population
then shared RTS init. Each admitted entry ends at conditional event/control
polling, not a measured usable-input opportunity or a collision route.

## Tristan population, live gate and changed state

**Confirmed:** reuse [Tristan's population/service deltas](./tristan-entry-interactions-and-exits.md#f530-alternate-population-and-event-frontier).
`data/maps/mapsetups.asm` gives15 one530 alternative. With530 held,
`map15/mapsetups/pointertable_530.asm` selects alternate entities/events/zones
and shared descriptions/items/RTS init. GetCurrentMapSetup in
`code/common/scripting/map/mapsetupsfunctions_1.asm` reads current map/flags
on each lookup. Entity allocation and later callback selection are independent.

Alternate source row0 in `s1_entities_530.asm`, `$5CE90..$5CEEA`, is **ally20**
at63,63 Up/eas_Init, with actual class/HP mapsprite. It is not NPC128.
Ordinary flag-followers allocate first; no ordinary row names20. With n such
allocations and initially unmapped20, Skreech is physical n+1 and the ten NPCs
map128–137 from n+2. Existing lookup20 deduplicates that ally allocation while
still consuming its source action operand; NPC mapping advances independently.
Joined20/active52/held820 does not suppress this row, and RTS init does not
hide, join or place him beside Bowie. Default530-clear has no20 source row.
Keep the accepted merchant motion, shifted service indices and priest mask
differences: this scene does not normalize them or re-run their services.

`s3_zoneevents_530.asm`, `$5CFFE..$5D020`, has one exact `(24,18)` row then
the shared default/RTS. `Map15_212_ZoneEvent0` executes in exact order:
chkFlg724; BEQ return; chkFlg820; BNE return; script cs_5D04E; setFlg820; RTS.
F724's accepted producer is the
[Bedoe rescue callback](./bedoe-hatchling-flight-and-rescue.md#callers-and-admission-boundaries),
whose flag write follows its scene return. A supplied724 does not prove that
history. Neither20,52,530 nor841 is tested **inside** this callback;530
selects the callback table. F820 is a post-return latch, not a join result.

Type6 dispatch first installs main's init action, then reads zone X from
PARAM1 and Y from PARAM3. RunMapSetupZoneEvent resolves the live setup,
scans byte coordinates (`FF` alone wildcard), calls the selected callback,
closes portrait/text, waits a VInt and waits physical0 motion. Ordinary zone
production requires reached layout kind `$1400`; supplied entry coordinates
alone do not post this event. It is independent of warp production.

| State at the relevant read | Result / preserved boundary |
| --- | --- |
|530 clear at zone dispatch|Default table returns;724/820 do not select the alternate table.|
|530 held,724 clear|Skip scene and820 write, even with20 already joined.|
|530 held,724 held,820 held|Skip scene; no membership, flight or follower repair.|
|530 held,724 held,820 clear|Call complete scene, then set820 if it returns, including when20 was prejoined.|
|Default population then530 set|Alternate callback can run against no20 mapping and shifted default NPCs. A flag write does not rebuild them.|
|Alternate population then530 cleared|Default zone callback; existing Skreech actor/aliases remain. Once script is selected, a later530 change does not branch this linear scene.|
|724/820 changed after their checks|No recheck inside cs_5D04E; a later clear/set does not undo a reached prefix; post-return set820 still executes.|
|Missing/dead aliases|Cleared lookup resolves0, not a safe no-op. FF or corrupt mappings have no safe-actor guard here. Own-actor HP gates apply only at the consumers named below; death does not universally suppress this scene.|
|Interrupt before join / after join / before820|Actor/camera/layer/membership/ceremony/follower prefixes remain;820 may be clear after20 was set, permitting replay if later admitted.|
|Fresh explicit15 after completion|Rebuilds ordinary followers/fixed20 at63,63 if530 held; no ordinary20 follower row. Temporary flight/actions/list are rebuilt independently of20/52/820.|
|CURRENT15 re-entry|Retains fixed actors/actions/mappings/layout/temp state, updates main/ordinary follower placement under mode/raft rules and reruns RTS init; no scene-state repair or automatic replay.|

## Complete scene, inline actions and text

**Confirmed:** `data/maps/entries/map15/mapsetups/scripts.asm:cs_5D04E`,
`$5D04E..$5D1CA`, contains63 top-level records (59 opcode commands, three
sleeps, one terminator),316 encoded bytes plus13 inline action/end blocks
of64 bytes:380 bytes total. There is no new actor list, conditional branch,
choice, explicit story-flag write, item grant or warp. The existing immutable
script corpus numbers top-level records0–62; source inline bodies are included
below. Compiler source indices use its raw operation stream, a different index.

1. **Staging,0–13:** cursor2006; Bowie Right; awaited setDest Peter24,17;
   Peter Right; setPos Skreech31,33 Up; wait1; awaited camera26,22;
   install/wait Init on Bowie, Peter,31; setPos Peter24,17 Right,
   31 at24,16 Right, Bowie24,18 Right; fly20 FFFF.
2. **Flight,14–35:** nine nonwaited Skreech sequences and ten awaited camera
   commands including staging camera6. The exact interleaving is below.
   After camera20,12, wait60; awaited setDest20 to26,18; face20 Left.
3. **Recognition,36–50:** Skreech text; Peter Right2/wait, face Down;
   Skreech Up; Peter and Skreech texts; shiver Peter; Peter/Skreech texts;
   two separately installed/waited eas_Jump on Peter; Peter continued text,
   Peter single, Skreech continued, Skreech single.
4. **Peter's motion and join,51–62:** three awaited inline sequences:
   Left1/Down1/faceRight40, Up1/Right1/faceDown40, Left1/Down1/faceRight40;
   Skreech Left; Peter text then Skreech text; join20; follow7->0/2;
   follow31->7/2; follow20->0/5; fly20 zero; csc_end.

| Command / inline action | Next command(s) before replacement | Conditional endpoint if each relative leg completes |
| --- | --- | --- |
|14 /Up8|15 wait60;16 camera26,20|31,25|
|17 /Left3|18 camera20,20|28,25|
|19 /Up10|20 camera20,10|28,15|
|21 /Right4|22 camera27,10|32,15|
|23 /Down5|24 camera27,15|32,20|
|25 /Left19|26 camera9,15|13,20|
|27 /Up10|28 camera9,5|13,10|
|29 /Right13|30 camera20,5|26,10|
|31 /Down7|32 camera20,12;33 wait60|26,17|

Every block ends with endActions. The endpoint column is an arithmetic command
projection from admitted31,33, **not** an observed flight or a guarantee that
a camera wait finishes an actor leg. Later installation replaces that actor's
current action pointer. Command34 writes destinations/velocities and waits for
movement, independently of the previous script pointer.

Normal text progression is2006–2016 inclusive (`07D6..07E0` in
`data/scripting/text/gamescript.txt`), nine nextSingleText (S) and two nextText
(C). Every source comment joins its actual keyed text; service446 is separate.

| Text / command / mode | Live speaker / modifier | Paraphrase / terminal control |
| --- | --- | --- |
|2006 /36 /S|Skreech20 /80|Asks if this is LEADER of Granseal; W1.|
|2007 /40 /S|Peter7 /00|Confirms and asks identity; W1.|
|2008 /41 /S|Skreech20 /80|NAME20 of Bedoe; W1.|
|2009 /43 /S|Peter7 /00|Surprised by Bedoe; W1.|
|2010 /44 /S|Skreech20 /80|LEADER saved his life there; W1.|
|2011 /47 /C|Peter7 /00|Begins recalling the rescue; W2.|
|2012 /48 /S|Peter7 /00|Remembers; W1.|
|2013 /49 /C|Skreech20 /80|Asks if remembered; W2.|
|2014 /50 /S|Skreech20 /80|Thanks him and asks to accompany him; W1.|
|2015 /55 /S|Peter7 /00|Remarks on rapid growth; W1.|
|2016 /56 /S|Skreech20 /80|Says he learned to fight; W1.|

No answer/yes-no branch follows that request. csc1D portrait resolution tests
packed bit15/right and bit14/mirror;80 is a modifier, not NPC128. Actual mapped
mapsprite/class/HP owns portrait/bleep, and an already-open portrait can bypass
opening. Text consumers wait for view scroll, display/increment cursor;
S additionally closes windows and sleeps10, C retains continuation. W1/W2
are independent text controls. Debug P2 Start skipping consumes operands
without display/cursor advancement and skips csWait; it does not skip every
action/join/follow effect. No visible/audio dwell or completed presentation
is established by these declarations.

## Actual action, layer, camera and follower consumers

**Confirmed:** `code/common/scripting/map/mapscriptengine_1.asm` and `_2.asm`,
the actual entity engines, `data/scripting/entity/eas_actions.asm` and
`eas_main.asm` distinguish the following operations:

- setFacing/setPos read **byte** IDs before AdjustScriptPointerByCharacterAliveStatus.
  Dead own ally0–29 can skip that command;31 bypasses the HP test. In contrast,
  csc15 setActscriptWait reads actor/wait bytes and script longword without that
  HP precheck. It installs a slot-valued wait timer and waits for eas_Idle.
  Init sets speed32/32, acceleration/collision/orientation/animation properties
  then Idle; it is not ally-stat initialization. eas_Jump disables auto-facing,
  moves relative0,-1/wait5,0,+1/wait5,0,0, reenables auto-facing then Idle.
- csc2D reads actor/wait **bytes**, masks FLAGS_A with9F, builds action scripts
  in FF9004 scratch, appends jump-to-Idle, advances scratch and replaces the
  action pointer. WaitFF means that pointer must become eas_Idle; wait0 returns
  immediately. Direction/count and face/duration are **bytes** in inline encoding;
  the count is sign-extended by the dispatcher. The four Peter blocks wait;
  the nine Skreech blocks do not. No own-HP precheck protects these installs.
- csc29 setDest reads actor/X/Y **words**, multiplies coordinates by384,
  writes destination/travel/velocity (magnitude32 per axis) and waits for
  movement when actor bit15 is clear, as both calls here are. It has no own-HP
  precheck. Motion completion, action-pointer idle and a camera wait are three
  different boundaries; stale pointers/aliases or service failure can alter
  or prevent the nominal sequence.
- csc32 setCamDest reads X/Y **words**, sets VIEW_TARGET_ENTITY=-1, calls
  j_SetCameraDestination -> SetCameraDestination (multiply384 -> SetViewDestination),
  then WaitForViewScrollEnd. It does not wait on Skreech's action pointer.
  The scene's1/60/60 sleeps are requests to Sleep, not measured scene duration.
  Shiver saves/restores Peter's animation counter and global sprite size when
  it returns; the two Jump waits are separate installs.
- csc2F fly reads actor/state **words** with no HP helper: nonzero FFFF writes
  ENTITYDEF_OFFSET_LAYER.b=16; zero clears it. It changes no ally movement type,
  combat flying stat, follower list, position, height or shadow by itself.
  In `entityscriptengine_1.asm:VInt_UpdateSprites`, signed window/layer comparison
  controls VDPTILE_PRIORITY; in `_2.asm:ConvertMapPixelCoordinatesToOffset`,
  nonzero layer during exploration selects the secondary map origin according
  to MAP_AREA_LAYER_TYPE. Flight naming alone proves neither collision bypass
  nor visible altitude. The action install separately clears collision bits.
- csc2C followEntity emits follower/leader/position **words** but peeks only
  `(a6).b` before the HP helper. Encoded0007,001F,0014 all expose00; GetCurrentHp's
  byte mask checks **Bowie0** for all three. Bowie HP0 skips six operand bytes
  and unwinds this command. Peter/Skreech HP0 alone does not skip their follow.
  Each HP read is live and independent. Admitted full IDs resolve current
  physical slots; table_FollowerPositions index2 gives(-24,0),5 gives(-24,-24)
  **pixel offsets**, not map tiles or immutable destinations.

`entityfunctions_2.asm:AddFollower` always installs/copies42-byte eas_Follower1,
advances ENTITY_WALKING_PARAMETERS and patches physical leader/offsets; it
appends the physical follower to EXPLORATION_ENTITIES only if absent. Existing
entries are not reordered and scratch allocation is not deduplicated. Thus
with intact distinct aliases and all HP prechecks admitted, Peter follows
Bowie,31 follows Peter and Skreech follows Bowie diagonally. There is **no
addNewFollower** call or last-list-tail selection, unlike Taya's scene.
Preexisting Taya/other follower entries do not make Skreech follow them.
Three admitted calls consume126 scratch bytes even if all list IDs existed.

GetEntityAddressFromCharacter masks/resolves the current lookup with no missing-
actor rejection. Missing7/20/31 can resolve physical0; aliases can make a
self-follow or replace another actor's action. Missing31 is not guaranteed
Astral; ordinary follower flags can overwrite that alias. SetPos20 may skip
for dead20 while fly/actions/setDest/follow still act on its mapping. No
arbitrary-population/list/scratch capacity safety is established.

With intact aliases, admitted positioning and completed waited actions,
pre-follow commanded positions are Bowie24,18 Right, Peter25,18 Right,
31 at24,16 Right and Skreech26,18 Left. The flight projection requires the
additional completed-leg assumptions above. Following can move actors during
later services; this is not a frozen final state or next map seed. Final fly0
clears only the **currently resolved**20 layer even if all follows skipped;
interruption before that reset can retain16. ExecuteMapScript's FFFF tail
conditionally waits for scroll when a dialogue window is present, clears view
scroll speed and restores interpreter registers, not actor/layer/member state.

## Join20, callback return and independent lifetimes

**Confirmed:** csc08_joinForce clears speech SFX, waits for view scroll,
reads word0014, clears bit15 and selects MUSIC_JOIN; special128 is not taken.
`code/common/stats/battleparty.asm:JoinForce` sets joined20, then UpdateForce
rebuilds counted joined/active/reserve lists in ally order0–29. Active count
less than FORCE_MAX_SIZE12 calls JoinBattleParty20 to set32+20=52. At capacity
it does not clear preheld52. The rebuild precedes the conditional active write:
a newly active20 need not appear in that counted list until a later rebuild.
Preheld52 without20 only becomes eligible after joined20 is written; repeated
joins preserve membership and can promote active52 when capacity permits.

No InitializeAllyStats, item initialization, heal or battle placement is called
by this join. Stats/class/HP/items remain supplied state. csc08 then reads
actual class/name, displays service text446 (NAME/CLASS join announcement),
waits through FadeOut_WaitForP1Input, closes text and sleeps10. That service
does not advance the scene's text cursor. Membership can already have changed
when ceremony/input stalls; this is not an atomic recruitment transaction.
No52 requirement or membership success test guards the three follow calls.

After csc_end returns, the callback sets820 and RTS; the zone wrapper closes
windows, waits a VInt and main motion. WaitForEvent retargets view0 and installs
main control only when pending type is zero: numeric PLAYER_TYPE0 character,
1 raft, other nonzero caravan. No pending-event guarantee or responsive-input
observation is supplied. A reached820 set proves this callback passed script
return, not natural rescue history, all actor movement, active-party inclusion,
saved persistence or successful final field control. Fresh entry discards
scene-specific followers/layer/actions while persistent flags have separate
lifetimes; CURRENT retains them subject to its actual mode/update services.

## South exit and next unresolved prerequisite

**Confirmed:** reuse Tristan's row4 `(255,31)` -> Map74 `(52,21)` Down3,
no-scroll0. Signed-negative X is wildcard; neither20/52/724/820 nor Taya21/841
gates that row. It is a separately reached travel request. At MainLoop,
SwitchMap has no74 source row. CheckBattle's Map74 alternatives are ordered27
then32, both wildcard X/Y and crop `(2,16,32,48)`: held427 selects27; only
clear427 allows held432 to select32; with neither unlock held, exploration74
is selected. Held527/532 clears
the selected unlock but still returns the corresponding battle that scan.
Stop before the resulting BattleLoop or ExplorationLoop body. Neither this
scene nor Taya's Nazca suggestion grants a ship or sets those unlocks.

The next prerequisite is an unresolved route to the existing **Frayja/F811**
producer, not a Skreech narrative trigger. Bounded source frontier:
`map74/mapsetups/s3_zoneevents.asm:Map74_ZoneEvent0`, at wildcard X/Y61,
branches first on527. Its completed27 arm requires811 held and532 clear to
set432 and MAP_EVENT_RELOADMAP. `map36/mapsetups/s2_entityevents_530.asm:Map36_212_EntityEvent0`
is an811 producer after cs_5DA28 returns (then sets follower82). Its reach,
scene and Pacalon/Battle31 route are outside this slice. F811 supplied set is
not evidence that route occurred; no Map74 exploration or next-scene audit
is added here.

Continue with the [Pacalon return, Frayja following and Battle31 admission owner](./pacalon-return-frayja-and-battle31-admission.md).

## Actual representations and consumer gaps

**Confirmed about repository consumers:** retained routing/program fixtures
and schemas express original facts; prepared Content and a reached runtime
consumer are different boundaries. The actual OriginalPrograms compiler was
run read-only on this selected scene into ignored `local/issue1072/lowering.json`.
It does not register/import the return world automatically. No Map0/1/35/15/74
package, Skreech deployment, original-runtime capture or Godot observation is
delivered. The [capability owner](../../remake/docs/capability-status.md) remains
authoritative for runtime scope.

| Boundary | Actual existing path | Refusal/guard, missing binding or unassessed fidelity |
| --- | --- | --- |
|Scene lowering|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py) emits face/position, Init/Jump/inline motion, camera, text/portrait, join and three follows.|Four native-call instructions remain, in order setDest,fly,setDest,fly. No dedicated lowering for those two services. The **first** is command2, before flight or join; normal ProgramRunner execution stops Unsupported there. No complete executed scene is claimed.|
|Content and Application|[ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs) reads native-call as UnsupportedInstruction; [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs) reports program-opcode/Unsupported when reached.|This is an explicit refusal, separate from the missing selected scene/world binding. Motion/action/camera/text expressibility does not bypass it. All inline Init/Jump/Peter/Skreech actions lower without nested native-call.|
|Motion/camera/text|EntityActionRunner and ExplorationViewRunner consume prepared action replacement, waits, camera-target/CameraWait and logical text; Godot [ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs) consumes current view, window/sprite priority and presentation cues.|Mechanisms exist; alias/HP/word-width fidelity, asynchronous replacement, full text/portrait/music resources, area/material/timing and reached consumers for15 remain unassessed. Actor Motion.Layer exists and Godot uses signed layer/window priority; this does not supply a fly script instruction.|
|Follow/list|Compiler emits7->0(-24,0),31->7(-24,0),20->0(-24,-24); ProgramRunner resolves slots and [FollowerMotion](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/FollowerMotion.cs) installs following.|Lowering omits the high-byte Bowie-HP precheck. Install has no original EXPLORATION_ENTITIES append/dedup or42-byte scratch accounting. Original list lifetime and aliases need binding/review; Taya's addNewFollower semantics cannot be transferred.|
|Join/ceremony|Compiler join_service emits member20, service446, music/input waits; [MapPartyMembership](../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs) preserves joined/active/rebuild ordering.|ProgramRunner requires partyFlags; with encounter starts, JoinResources requires one unique20 deployment and start (party-flag-layout/join-member-binding/join-member-start). No20 resource binding is established. Its admitted resource addition is distinct from original join's no-stat-reset behavior.|
|Setup/return/admission|Existing map preparer lowers selected entities/zone callbacks/warps; MapSetupRoute, MapTransfer and BattleEntry consume prepared alternatives.|No new exporter needed. Live setup reselection versus loaded-definition dispatch, CURRENT lifecycle, negative wildcard/coordinate words and ordered destination admission need selected bindings. Map0's declared parallaxBX16/autoscrollBY1 is explicitly refused by ExplorationViewRunner's field-view-profile and Godot BattleMapViewport's battlefield-map-plane-motion-unsupported; it is not merely an absent asset.|
|Private Content boundary|[PrivateExplorationReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateExplorationReader.cs) admits provenance-bound prepared world/start documents.|Production does not read research fixtures/disassembly on demand. PrivateBattleEncounterReader currently pins encounters1–3, not27/29/30/32. Existing service/search/pickup gaps remain with their accepted owners. No engine-ready or travel-ready conclusion follows.|

## Reproduction, adversarial review and remaining Unknowns

Load `local/private-inputs.ps1` in the launching process. Reuse the retained
pair, source pin and existing source/fixture owners; do not regenerate assets.
The ignored bounded direct-check recipe is:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1072/review.py <configured-source-disasm-root>
git diff --check
```

It joins the complete scene and every inline action with the unchanged corpus,
macro widths/dispatchers, all11 keyed texts/comments and ceremony446; checks
the selected route rows, live gate polarity/order, membership/list/layer/HP
consumers, actual lowering and named current consumer guards; then reviews
changed-state/prose, links/anchors, UTF-8 and the exact three-path/private
boundary. These are direct checks, not tests of an audit tool or fresh runtime
proof. Parser/schema/extractor/test-mutation obligations are not newly applicable
because none is changed; branch/order/width and prose discriminators remain
review obligations. Plan on the clean committed head with
`uv run sf2 verify plan --base origin/main --head HEAD`; preserve actual
exact-head scoped CI and command receipts in the public Issue/PR handoff.

Preserve #1070's original author0.739s/main-gate0.743s and successful
CI39464142557 which missed the STEP_COUNTER prose defect; corrected N==V
check main-gate0.104s/CI39466738861 remains the accepted correction. This
owner uses that overflow-aware rule. #1068's follow high-byte HP correction,
#1066's region-projection correction, earlier diagnostic failures/Unknowns
and the twelve completed old comparison failures remain in their existing
owners/handoffs; none is rerun or relabeled here. The unchanged pair is not
new evidence of ROM/H1 equality or natural completion.

**Unknown:** natural collision/travel/zone reach, actual pending requests,
flags/mode/follower population/aliases/HP/stats, movement and ceremony completion,
view/control opportunities, exact rendering/audio/timing, save/load persistence
and the next Frayja/Pacalon route. They are explicit boundaries, not a runtime
acquisition queue. Research provenance stays separate from future remake
design decisions and implementation acceptance.
