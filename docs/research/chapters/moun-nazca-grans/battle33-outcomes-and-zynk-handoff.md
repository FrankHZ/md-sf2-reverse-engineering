# Battle33 outcomes and the Zynk handoff

Evidence date: 2026-10-20. **Confirmed (static):** unchanged Battle33 victory
saves its Map31 return seed before the rock/Zynk scene, then clears433 and
sets533. The scene's last story-flag write is833; it neither joins Zynk26 nor
sets his follower flag76. Its moving rock and Zynk are separate physical
actors. Returned exploration rebuilds the population and layout, retaining
the833-controlled copy at16,8 only when that flag is held. Both underground
warp rows are unconditional declarations; reaching them is a separate
question. **Unknown:** natural victory/travel, incoming state, completed
services, presentation, usable control and saved persistence. No playable
Battle33 or completed Moun/underground implementation follows.

Continue with [Moun underground, Gyan joining and Zynk following](./moun-underground-gyan-and-zynk.md).

## Agreement and evidence route

Standard rigor for [Issue #1092](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1092)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
make terminal processing, the complete after scene and conditional exploration
handoff traceable for later content integration. Accepted base is
`faa3178b31e95c9f540b1b758b9ea9c4102c9603`, including
[Battle33 activation/first dispatch](./battle33-activation-and-first-turn-dispatch.md),
[initialization/opening](./battle33-initialization-and-opening.md) and
[Battle32 outcomes/Moun admission](./battle32-outcomes-and-moun-entry.md).
Own only this document and one navigation link in the activation predecessor.
No shared index, stacked dependency or competing UI692 ownership.

Begin after a returning individual turn; do not survey individual-turn AI,
actions or combat. Follow victory/defeat/escape through returned exploration,
then identify Map31's outgoing requests, stopping before Map39 underground
setup, population, interactions or its own events. Map31's setup/init effects
necessary to this boundary are included; a whole-town interaction catalogue,
next encounter, design/translation, engine implementation, new tools/fixtures/
schemas and acquisition are excluded.

Reuse [battle loop](../../battle/battle-loop.md), [battle cutscenes](../../battle/battle-cutscenes.md),
[script data](../../battle/battle-cutscene-data.md), [common scripting](../../world/common-scripting.md),
[common maps](../../world/common-maps.md), [request state](../../world/map-event-request-state.md),
[request consumption](../../world/map-event-request-consumption.md),
[lifecycle](../../../design/contracts/battle/battle-control-lifecycle.md) and
[cutscene routing](../../../design/contracts/battle/battle-cutscene-routing.md).
Original paths below are relative to `disasm/` at clean, read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The accepted USA retail ROM identity is SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM/H1/runtime equality is claimed. Constants/RAM belong to
`sf2enums.asm`/`sf2const.asm`; Fnnn means flag nnn. Ordinals are zero-based,
intervals half-open. `battleloop/` and `cutscenes/` abbreviate children of
`code/gameflow/battle/`; `map/` and `entity/` mean `code/common/scripting/`.

Minimum evidence is complete selected table/command/inline-action/list/text
joins, actual service consumers and changed-state review. Reuse the unchanged
18,285,979-byte init/script pair and accepted0.083s provenance in
`local/issue1005/pair.txt`; no regeneration. Bounded linear reads and one
in-memory scene lowering budget less than one minute/512MiB per direct run,
small ignored receipts in `local/issue1092/`, no expanded corpus. Reassess
contradictions or material supporting machinery before expansion. No runtime,
emulator/Godot/SDK, screenshots, old comparisons or blanket suites. Apply
ADR0004's complete acceptance checklist proportionally to documentation.

## Terminal selection, leader cleanup and reward boundary

**Confirmed:** `battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`) resumes from
ExecuteIndividualTurn, optionally applies debug kill-all, calls the defeated
wrapper, ProcessKilledCombatants and CountRemainingCombatants, in that order.
Ally count0 loses before enemy count0 wins. A nonterminal first pass applies
ProcessAfterTurnEffects to the actual turn-buffer byte, processes deaths and
repeats those ordered counts, without another defeated hook. Only a
nonterminal second pass increments CURRENT_BATTLE_TURN.b by2. Escape unwinds
before this suffix. HP changes, death effects and outcome are separate seams.

| Complete selected owner / extent | Battle33 selection |
| --- | --- |
|`data/battles/cutscenes/battleendcutscenes.asm`,48 relative words, `$47BE8..$47C48`|slot33/byte66 at `$47C2A`: ms_Empty.|
|`data/battles/global/enemyleaderpresence.asm`,45 bytes, `$47C8E..$47CBB`|byte33 at `$47CAF`: -1, nonzero cleanup.|
|`data/battles/cutscenes/afterbattlecutscenes.asm`,48 relative words, `$47CF4..$47D54`|slot33/byte66 at `$47D36`: abcs_battle33, `$4D09A`.|
|`data/battles/cutscenes/afterbattlejoins.asm`,52 bytes, `$47D6A..$47D9E`|byte33 at `$47D8B`:0, the actual JoinForce operand.|
|`data/battles/global/afterbattlepositions.asm`|Only battle5; no33 position override.|
|`data/battles/global/enemyitemdrops.asm`,30 four-byte rows plus terminator, `$BE52..$BECC`|INSIDE_MOUN: enemy ordinal7/combatant135, MAGE_STAFF, droppedFlag11. This is a conditional action-side drop, not an automatic victory reward.|

`battleendcutscenesstart.asm` requires Bowie0 HP.w!=0 and enemy128 HP.w=0.
Clear actual500+CURRENT_BATTLE dispatches ms_Empty; held533 skips it for33.
Both reach `battleendcutscenesend.asm:loc_47C48`, reread the battle and select
the leader byte. Unchanged33's -1 scans all32 enemies128–159, appending each
nonzero-HP slot to the **existing** death list, incrementing its word length
and replacing the next sentinel. No initial clear or X/Y, membership,
activation or neutral test. Sixteen declared enemies do not shorten the sweep.

ms_Empty is one FFFF word. Dispatch still changes interpreter scratch,
clears text-skip on entry, conditionally waits for view when a dialog remains
and clears scrolling speed on return. Skipping it avoids those interpreter
effects, but does not skip the cleanup tail. An empty script proves no service
completion.

ProcessKilledCombatants requests spin/effect/sound waits, credits defeats to
the listed dead ally or enemy kills to supplied BATTLESCENE_FIRST_ALLY, writes X/Y=-1,
clears status/recomputes stats and moves the mapped entity to `$7000,$7000`.
It does not set HP0, clear death-list length or execute the item-drop service.
Living queued enemies can therefore stop counting through X alone. Stale or
duplicate entries, mappings and interrupted cleanup remain supplied state.
CountRemainingCombatants scans30 allies/32 enemies with signed X.b>=0 and
HP.w!=0, ignoring Y, membership, neutral and activation. High-bit nonzero HP
passes. Bowie HP0 forces ally count0; simultaneous exhaustion loses. Off-map
enemies can permit victory without the leader hook, and a second-pass poison
kill of128 does not invoke that hook again there. F88 supplies restored state,
not a fresh-list guarantee. CURRENT_BATTLE is reread, not snapshotted across
all guards, scripts, tables and outcome writes.

The selected135 is NECROMANCER_0 definition101, equipped Mage Staff93 in the
[initialization owner](./battle33-initialization-and-opening.md#complete-encounter-declarations-and-fresh-combatants),
not definition73's Flail. `battleactions/dropenemyitem.asm` requires an allied
attacker, enemy target and targetDies, matches live battle/entity and finds
the actual item slot. Reached flag11 is the one-shot bit in the enemy-drop
bitfield, **not story F11**. It then removes that item and attempts the award;
dead attacker or failed AddItem can route this rare staff to deals. Mage Staff
is not one of the three random special-drop weapons. A missing item, prior
drop bit, different target or incomplete action need not award it. Cleanup,
victory and this document do not establish that the action-side drop occurred.

## Victory seed and after-wrapper completion

**Confirmed:** `battleloop_2.asm:BattleLoop_Victory` (`$23CBA..$23D98` file)
first heals all30 slots: Peter7/Lemon28 even at HP0; other living allies to
maximum HP/MP; other dead allies stay dead. It retains existing
STUN/POISON/CURSE and refreshes stats. Battle44's timer branch is not33.

Before the scene, it saves CURRENT_MAP.b into PARAM2, calls
UpdateForceAndGetFirstBattlePartyMemberIndex, byte-adds battle crop X/Y to
that member's actual combatant X/Y into PARAM3/4, reads its mapped physical
facing into PARAM5 and writes PARAM1=0. It posts no event type. UpdateForce
rebuilds joined/active/reserve prefixes and word counts in ID order without
clearing list tails or writing membership flags. The first active byte is
read without a count/HP test: not necessarily Bowie, alive or placed. An empty
list exposes its retained first byte. With intact Map31/crop `(0,0,32,38)`,
the saved return is31 and that member's X/Y bytes/facing, **not** the scene's
main `(11,11)` or camera `(8,6)`.

GetEntityIndexForCombatant returns an unchecked lookup byte. Mapping0 aliases
physical0; FF is shifted by5 and facing is read from
`ENTITY_DATA+$1FE0+$10`. Neither LoadBattle's negative fallback nor its3F mask
is applied here. Later reconstruction does not repair this earlier seed.

`afterbattlecutscenesstart.asm` checks actual500+CURRENT_BATTLE. Clear533
runs abcs_battle33; held533 skips **the whole scene**, including the copy,
Zynk actions, following and833 write. Both reach EndAfterBattleCutscene:
reread CURRENT_BATTLE and JoinForce the selected byte0. That sets F0,
rebuilds lists, then sets active32 only if active count<12; at capacity an
existing32 remains. A newly set32 is absent from that preceding list snapshot.
This unconditional tail joins Bowie, not Zynk26.

On wrapper return the loop rereads CURRENT_BATTLE, clears400+index and sets
500+index: unchanged33 clears433 then sets533. It clears d0–d3 **words**,
copies PARAM2–5 bytes into them and returns d4=1. Upper register halves are
not established by clr.w; saved PARAM1=0 differs from returned d4=1.
Neither483 nor831/830/76/26/58 is written by this tail. The scene writes833
earlier; an interrupted prefix or preheld533 can separate533 from833.

Scene command1 selects csc37_loadMapAndFadeIn → csc48_loadMap: set black
fade, load Map31 tilesets, wait, disable services/display, pack camera8,6
as `$0806`, multiply by3, LoadMap and restore display/services. CURRENT_MAP
becomes31; CURRENT_BATTLE stays33. LoadMapBlocksAndLayout loads base layout,
flag copies/chest substitutions and the **live battle crop**. With unchanged
crop0,0,32,38, the selected area has local ends `(31*384,37*384)` instead of
ordinary Map31's declared `(31*384,38*384)`. No clipping or crop change is
implied. The camera and block-copy storage row39 are not actor positions or
a return request. No command here changes PARAM2–5, posts a warp, enters
MainLoop/CheckBattle/town init or clears CURRENT_BATTLE.

## Complete rock and Zynk scene

**Confirmed:** `data/battles/entries/battle33/cs_afterbattle.asm` spans
`$4D09A..$4D342`. abcs_battle33 has109 top-level commands,510 encoded bytes
including csc_end. Sixteen movement and six custom streams occupy138
bytes before ce_4D322 at `$4D322`; the32-byte entity list ends at `$4D342`.
Retained script records supply top-level commands; pinned source supplies
the complete inline streams, entity rows, texts and actual consumers.

Here B=FOLLOWER_B31, P=Peter7, Z=Zynk26 and R=logical128, the newly declared
OBJECT2 actor. These are logical lookup identities, not physical slot numbers.

| Index | Complete operation / boundary |
| --- | --- |
|0–2|textCursor2876; loadMapFadeIn31 camera8,6; loadMapEntities ce_4D322.|
|3–7|await Init Bowie0, Init P; set P13,11/Up; await Init B; set B12,9/Down.|
|8–10|setBlocks source3,39 size1,1 →16,8; stopEntity R animation; await R DeactivateAutoFacing.|
|11–18|R asynchronous Down1; wait4; await R StopMoving; Z asynchronous Down1; wait2; await Z StopMoving, then Immersed; fadeInB.|
|19–26|B single2876; wait5; nonawaited Bowie 2xRightLeft; wait5; nonawaited B 2xUpDown; wait5; nonawaited P 2xRightLeft; wait120.|
|27–35|R asynchronous Up1/wait3, Down1/wait3, Up1/wait3, Down1/wait3; await R StopMoving.|
|36–42|face P Right; await P Jump; wait40; P actionsWait Right3; face P Up, Bowie Right, B Right.|
|43–47|R customWait speed24/24, jump Idle; R asynchronous Up1; wait8; await R StopMoving; wait50.|
|48–55|R customWait speed48/48, jump Idle; R asynchronous Down1; wait4; quake2; await R StopMoving; wait10; quake0; wait40.|
|56–61|P actionsWait Up2; R single2877; await P Jump, DeactivateAutoFacing; P customWait speed48/48, jump Idle; P actionsWait Down2.|
|62–68|camera destination10,6 and view wait; R customWait speed24/24, jump Idle; R asynchronous Up1; wait8; await R StopMoving; R continued2878/single2879.|
|69–79|B asynchronous Right3 then Up1; Bowie actionsWait Right4; face B Right, Bowie Up; B2880, R2881, B2882, R2883, B2884, R continued2885/single2886.|
|80–88|R customWait speed40/40, jump Idle; await R JumpRight; R customWait speed56/56, jump Idle; await R DeactivateAutoFacing; R asynchronous Down1; set Z16,8/Down; await Z Init; wait3; await R StopMoving.|
|89–98|quake2; wait20; quake0; B continued2887/single2888; Z single2889/C0; B continued2890/single2891; Z single2892/C0; set Z63,63/Down.|
|99–104|await P Jump; wait30; P actionsWait Up2; face B Down; B continued2893/single2894.|
|105–108|follow B→Bowie offset2; follow P→B offset2; setF833; csc_end.|

Every custom stream has precisely ac_setSpeed with the paired value in the
table, ac_jump eas_Idle, ac_end; those are six streams in source order
24,48,48,24,40,56. Every movement stream ends in endActions; B69 alone has
two moves. Nonawaited installation does not wait for arrival. Replacing R's
script at13/35/46/52/66/88 and Z's at16 interrupts the preceding movement
at the service boundary actually reached, not after a guaranteed tile.
The3/4/8 waits must not be turned into completed one-tile moves or final
integer coordinates. B69 is not joined by Bowie's70; a wait on one actor
is not a wait on every actor. No final R position is established here.

csc2D compiles move macros into action scratch, appends a jump to eas_Idle,
sets the actor read timer from its mapped physical slot, clears flagsA bits5/6
with9F and optionally waits for that actor's Idle. csc14 custom scripts and
csc15 named installations use that same physical-slot timer and optional
Idle polling. None uses the ally-HP skip helper. Inline endActions/ac_end
emit8080; csc14 scans that marker after its optional wait. An Idle pointer
is an action-script state, not a proof that all travel, sprite or view work
completed.

The selected `data/scripting/entity/eas_actions.asm` streams are:

| Stream | Complete effective action sequence before Idle |
| --- | --- |
|Init|accelFactors0/0; speed32/32; acceleration OFF/OFF; deceleration OFF/OFF; obstructable ON; entity/map uncollidable OFF; autoFacing ON; orientUp; immersed OFF; size24; updateSprite.|
|DeactivateAutoFacing|autoFacing OFF.|
|StopMoving|moveRel0,0.|
|Immersed|immersed ON; updateSprite.|
|2xRightLeft / 2xUpDown|four facing/updateSprite/wait30 groups: Right,Left,Right,Left or Up,Down,Up,Down.|
|Jump|autoFacing OFF; moveRel0,-1; wait5; moveRel0,1; wait5; moveRel0,0; autoFacing ON.|
|JumpRight|autoFacing OFF; moveRel1,-1; wait6; moveRel1,1; wait6; moveRel0,0; autoFacing ON.|

Each ends with ac_jump eas_Idle. esc04_moveToRelativeDest computes signed
offset×384 from the **current physical** X/Y, checks competing destinations
and enters travel setup; zero-relative StopMoving requests the then-current
point. Jump/JumpRight use timed waits rather than waitDest, so their names
do not prove a completed arc or exact tile displacement. esc10 copies the
packed speed word. Immersed changes flagsB bit5 and refreshes sprite; later
Init on Z at86 clears it. Neither changes HP, membership or joined flags.

stopEntity is csc1C_stopEntityAnim, setting animation counterFF; it does
not cancel movement. Its **word**128 HP precheck bypasses allies via enemy
bit7. In contrast csc19 setPos and csc23 setFacing peek their **byte** ID:
allies0–29 with HP.w=0 skip, nonzero including high-bit proceeds;31/128
bypass. Thus Z's setPos85/98 can skip when dead while his action scripts and
dialogue still run. Physical placement of Z at63,63 is not HideEntity,
LeaveBattleParty, a combatant-X write or a warp. The list already declared
him at16,8 regardless of membership; interruption/death can change the
visible projection. Quake2/0 directly write amplitude, without high-bit ramp
loops. Source requests/order do not prove VInt/audio/text/view/DMA completion.

Command8 is csc34_setBlocks: three operand words `$0327,$0101,$1008`
feed CopyMapBlocks, copying one working-layout word from3,39 to16,8 and
setting both view-plane update bits. CopyMapBlocks uses64-word/128-byte row
stride, width/height minus1 DBF loops and preserves d0–d7/a2. It writes
layout RAM, not ROM, an event row, story833 or a permanent map edit. The
rendered doorway/rock appearance and collision interpretation of that source
word are not decoded here.833 is only set at107, well after the copy/actions.

## Physical identities, following and logical party state

**Confirmed:** ce_4D322 has a six-byte main header11,11/Up, followed by
three eight-byte rows: Peter13,11/Up/ALLY_PETER/eas_Init;
OBJECT2 at16,8/Up/MAPSPRITE_OBJECT2=179/eas_Init; and Zynk16,8/Down/
ALLY_ZYNK=26/eas_Init, then a two-byte terminator. It is four declared actors
including main, not four additional NPCs. Rock and Zynk share initial X/Y
but are separate declarations with separate scripts and aliases.

csc42 disables services/display, reads the header, calls InitializeMapEntities,
loads mapsprites and restores services/display. ClearEntities resets49
physical slots,64 lookup bytes and walking scratch; battle mappings do not
survive. InitializeFollowerEntities runs before fixed rows and independently
of joined/active membership. Map31 is not overworld, so F65 does not prepend
Caravan here. Held67 can predeclare Peter7, held76 can predeclare Zynk26,
held82 Frayja22. Fixed ally rows with nonzero lookup skip allocation; regular
nonally OBJECT2 uses the next nonally alias byte32, corresponding to logical
128 through GetEntityAddressFromCharacter's enemy -96 translation. It is
not the old Shaman's combatant or sprite retained after battle cleanup.

Let n be successful ordinary follower allocations and p=1 if Peter's fixed
row allocates, else0. Rock is physical `n+p+1`; if76 is clear, fixed Zynk
is `n+p+2`. With76 held, the Zynk row instead retains his earlier follower
mapping/placement until later commands. Main is physical0. Follower scripts
and possible overwrites therefore affect initial Z's partial motion; no
universal physical numbering is inferred from the four-entry list.
Original unchecked capacity/finite action-pointer tables and arbitrary
alias combinations remain supplied-state limits.

FOLLOWER_B31 can be Astral via74, Oddler via68 or later Astral via70, with
last lookup write winning; earlier allocations still consume slots. Absent
31 maps physical0, so its actions/text/following can affect main. The scene
does not establish the incoming follower flags or repair that alias. Actual
class mapsprites determine ally appearance; during scene battle33 is still
live, so the outside-battle dead-ally blue-flame branch is not selected.
Dialogue spoken by128 uses OBJECT2's current physical mapsprite, not Zynk's
ally lookup; NAME26 in that text does not replace the physical speaker.
`data/spritedialogproperties.asm` has no OBJECT2 row, so an intact179 lookup
falls back to portrait DEFAULT=-1 and bleep6. Expected Zynk mapsprite55
instead selects portrait ZYNK26/bleep8; actual changed class/sprite and an
already open portrait can change the reached presentation service.

The two follow macros emit three words each. csc2C peeks only the **high
byte of the first word**:00 for31 and7. GetCurrentHp ultimately masks the
combatant index to a byte, so both prechecks test **Bowie0 HP.w**, not the
named follower's HP. HP0 skips six operand bytes and unwinds this command;
nonzero, including high-bit, permits full-word follower/leader mapping and
offset2=(-24,0). Reads occur separately. Dead Peter alone does not suppress
his following; missing/duplicate aliases can install over main or another
actor. AddFollower copies42-byte eas_Follower1 into walking scratch, patches
physical leader/offsets, advances scratch and appends the follower to
EXPLORATION_ENTITIES only if absent. Two admitted calls consume84 bytes even
if insertion deduplicates. The chain is31→0,7→31. **Zynk is not in it.**

Neither the scene nor these action/follow services calls JoinForce26,
JoinBattleParty26, LeaveBattleParty26 or sets/clears26,58,76. Joined26,
active58 and follower76 are distinct authorities and keep their incoming
values. Text NAME26, a physical actor, relocation63,63, final833 and later
Map39 destinations prove no recruitment. LEADER in2893 can rebuild cached
party lists before107; EndAfterBattleCutscene's JoinForce0 rebuilds again
after the scene, without supplying a new Zynk membership write.

## Keyed text and control dependencies

**Confirmed:** normal cursor progression is2876–2894 (`0B3C..0B4E`) in
`data/scripting/text/gamescript.txt`, nineteen messages; no join446 or choice
prompt occurs. N denotes a line break; W1/W2 retain distinct consumers.
Names below are paraphrases; token arrays are complete and ordered.

| Key / command | Speaker / mode / modifier | Content / tokens |
| --- | --- | --- |
|2876 /19|31 /single /00|Suggests survivors elsewhere; N,W1.|
|2877 /57|128 /single /00|Warns against touching the rock; W1.|
|2878 /67|128 /continued /00|Asks about defeating devils; N,W2.|
|2879 /68|128 /single /00|Explains the sudden quiet; N,W1.|
|2880 /73|31 /single /00|Asks identity and origin; N,N,W1.|
|2881 /74|128 /single /00|Names himself, denies being from Moun; NAME26,N,W1.|
|2882 /75|31 /single /00|Asks his origin; W1.|
|2883 /76|128 /single /00|Answers the past; W1.|
|2884 /77|31 /single /00|Asks meaning; W1.|
|2885 /78|128 /continued /00|Cannot explain; W2.|
|2886 /79|128 /single /00|States his identity is all he is; N,W1.|
|2887 /92|31 /continued /00|Comments on unfamiliar appearance; N,W2.|
|2888 /93|31 /single /00|Asks where Moun's survivors are; N,N,W1.|
|2889 /94|26 /single /C0|Confirms some survived; W1.|
|2890 /95|31 /continued /00|Welcomes the news; W2.|
|2891 /96|31 /single /00|Asks where; W1.|
|2892 /97|26 /single /C0|Says below and asks them to follow; W1.|
|2893 /103|31 /continued /00|Urges LEADER to follow Zynk; LEADER,N,NAME26,W2.|
|2894 /104|31 /single /00|Says Zynk knows the hiding place; NAME26,N,W1.|

csc00/csc02/csc1D consume packed modifier/speaker, derive portrait/bleep
from the actual physical mapsprite, wait for view, DisplayText and increment
cursor. Single closes portrait/text and Sleep10; continued leaves windows.
C0 requests right-side/mirror bits15/14, not an actor change or text variable;
an already open portrait returns early. NAME26 resolves combatant26's name;
LEADER invokes symbol_leader → UpdateForceAndGetFirstBattlePartyMemberIndex
→ UpdateForce before GetCombatantName. That token can rebuild lists and name
a member other than Bowie. A skipped/unreached token has done neither.

ExecuteMapScript clears text-skip on entry; debug P2 Start can latch it.
Skipped nextText/nextSingleText consumes its operand without display or
cursor increment, so the nineteen-key sequence assumes ordinary progression.
That skip does not skip all map/action/flag/follow commands. Reached833 is
therefore not proof that every line was displayed or accepted by player input.
**Inferred:** the dialogue presents Zynk directing the party to survivors
under the rock. Natural entry, player understanding, visible rock motion,
completed text waits and control handback remain Unknown.

## Defeat, Egress and Angel Wing

**Confirmed:** defeat calls UpdateBattleUnlockedFlag, which clears433 only
when533 is already held. It clears dialogue name1, requests sad music/text363
and close, restores **Bowie0** HP=max, halves unsigned gold, selects egress
and returns d4=-1.363's LEADER token can rebuild lists before recovery;
the named member need not be Bowie. Battle4's losable upgrade is not33.

Admitted Egress deducts the selected spell MP cost before274/cast flash;
Angel Wing removes its selected inventory slot and refreshes stats before275
and the common escape tail. That tail closes text, updates unlock, unwinds
the action frame, selects egress and clears d4.w. Both bypass the post-turn
terminal suffix, defeat HP/gold recovery and after/join. None newly sets533
or833/830/76/26/58; reached earlier costs, flags, combat and opening writes
remain. This is not an atomic rollback or proof that retreat services finished.

`battleloop/getegresspositionforbattle.asm` has special cases for16,17,13,31,
32,38/39 flag clears, **no33 override**: unchanged33 reads live EGRESS_MAP.b.
`code/common/maps/egressinit.asm:GetSavepointForMap` first checks399. Reached
BattleLoop sets399; changed clear399 returns initial Map3 `(56,3)` Down3
before raft reset. Held399 scans all23 four-byte savepoints; if live EGRESS_MAP
is31 it selects Moun `(20,8)` Up1, if2 Pacalon `(41,10)` Up1, if36 castle
`(40,5)` Up1. Other entries keep their own selections; no matching map leaves
its map byte and default `(1,1)` Up1. Entry into Moun, victory or scene loading
does not itself prove what EGRESS_MAP was supplied.

Held399/64 then scans the raft-reset table against the selected destination;
31/2/36 all use its default raft Map69 `(42,15)`, without clearing64. Clear64
leaves raft state unchanged. Changed CURRENT_BATTLE can select another special
branch. Returned exploration consequently need not be Map31 on defeat/escape;
no complete survey of every possible supplied egress town is implied here.

## Returned Map31, reload conditions and outgoing requests

**Confirmed:** `mainloop.asm:alt_MainLoopEntry` calls SwitchMap then
ExplorationLoop after every battle return, with **no d4 sign test or
intervening CheckBattle**. Complete switch rows are3/F640→4,71/F530→78,
58/F999→59; unchanged31 has none. Fresh explicit exploration clears pending,
subtracts20000 from STEP_COUNTER with signed BGE (**N==V**, else0), heals
living/immortal allies, fades, sets CURRENT_MAP and CURRENT_BATTLE255, loads
tilesets and reconstructs population/mapsprites. It clears temp256–383,
sets80, loads map/view, installs base VInts and runs the selected town init.

Thus unchanged victory returns at the pre-scene seed, reloads ordinary Map31
area0,0..31,38 and loses the scene's actor positions/action scripts/follow
chain. Base layout is loaded afresh; flag copies recur from current flags,
not from the fact that command8 once ran. `map31/3-flag-events.asm`
(`$AB65E..$AB670`) has exactly two eight-byte rows plus terminator:

| Flag at load | Working-layout mutation |
| --- | --- |
|833 held|source3,39 size1×1 →16,8, matching scene8's explicit copy.|
|831 held|source1,39 size2×2 →26,32, the accepted Frayja gate change.|
|Either clear|Its own copy is skipped; the other row remains independent.|

LoadMapBlocksAndLayout applies those before chest substitutions and any live
battle crop; exploration255 omits the crop. No rendered block/collision decode
or actual transit follows solely from a held flag. Scene8 can have happened
without107, or preheld533 can skip107 with833 still clear. Conversely supplied
833 can reproduce the copy even when this after scene was skipped.831 is
from the before scene, not this after scene; neither requires830.
`map31/4-step-events.asm` separately declares five1×1 door copies from0,39
at triggers/destinations11,8;20,10;27,18;21,26;6,31. None is a new833/831
predicate or either selected underground coordinate. Their later use and
roof/interactions are outside this boundary.

Map setup selection in `data/maps/mapsetups.asm` is default ms_map31,
overridden by held830 to ms_map31_flag830. Each six-long pointer table
(`$5D40E..$5D426` / `$5D426..$5D43E`) selects entity/events/zone/description/
item/init owners; only descriptions are shared. GetCurrentMapSetup rechecks
live map/830 per call, so population and init can select differently if flags
change between them.533 and833 do not select the populated setup.

| Selected setup/init | Necessary returned boundary |
| --- | --- |
|830 clear|Fixed entity list is empty; ordinary flagged followers/main still reconstruct. ms_map31_InitFunction `$5D61C..$5D61E` is RTS. The scene's fixed rock/Zynk do not persist.|
|830 held|Eleven town rows in s1_entities_830, including initially hidden POSE2 at63,63; no fixed Zynk/OBJECT2 declaration. Init `$5D61E..$5D644` tests832: held runs cs_5D63C, setPos136→30,6/Left; clear skips. Then held834 with835 clear clears834; otherwise leaves it. This is independent of533/833 and not a Zynk recruitment effect.|

F76 independently admits ordinary Zynk following in either setup, F67 Peter,
F82 Frayja; joined26 or active58 alone does not. Reconstruction can change
dead allies' exploration appearance. With no new76, the hidden physical26
from the scene is gone on reload. The optional town-init setPos136 is an
enemy-coded nonally alias, not ally26. Do not execute its later zone/item
events here. At first WaitForEvent, pending is tested before A/C; zero
installs mode-selected control/view0, nonzero dispatches an event first.
Stop this construction at the input/event handoff; entry alone proves no
usable input or saved persistence.

`map31/6-warp-events.asm` (`$AB6C4..$AB6EE`) has five eight-byte rows plus
terminator. Rows0/1 are the underground outbound handoffs; remaining rows
are town departures, retained here to distinguish the whole selector table:

| Row / source X,Y | No-scroll destination / X,Y / facing |
| --- | --- |
|0 /16,8|Map39 MOUN_UNDERGROUND /3,21 /Right0.|
|1 /27,7|Map39 MOUN_UNDERGROUND /14,20 /Left2.|
|2 /0,255|Map74 /2,25 /Down3.|
|3 /31,255|Map74 /5,25 /Down3.|
|4 /255,38|Map74 /5,25 /Down3.|

All padding/mode bytes are0. No row tests833,831,830,533,76 or Zynk membership.
FF source bytes are wildcards, not literal255 coordinates. WarpIfSetAtPoint's
ordinary walking caller requires layout kind1000; it sign-extends candidate
coordinates, divides by384, tests the FFFF terminator first, then source bytes
in order. First match wins, no match preserves existing pending. Opening a
layout word, removing a scene actor and admitting a warp are separate facts;
neither rock dialogue nor row0 proves natural collision/walking reach. Row1
is an independent entrance, not gated by the completed rock scene.

Producer writes event word1 **before** PARAM1..4 longword, then facing/padding
word and WARP_SFX. WaitForEvent checks pending before A/C; ProcessMapEvent
clears its stored type before dispatch. ProcessMapEventType1_Warp reads
PARAM1=0, submits/clears WARP_SFX, idles main, unwinds to MainLoop, clears
d0–d4 words, reads destination map and samples actual main X/Y by signed
word division. Nonnegative destination bytes replace only each sampled
**low byte**; negative ones retain its whole sampled word. With intact small
state, selected rows0/1 return39 `(3,21)` Right0 or39 `(14,20)` Left2,d4=0.
Consumption-time coordinates/payloads can differ from trigger-time values;
partial/replaced requests or nonzero changed PARAM1 select other behavior.
**Stop at this outbound MainLoop handoff**, before Map39 setup or exploration.

A later explicit Map31 request takes MainLoop @Start → SwitchMap → CheckBattle.
Its sole matching battle row33 requires433, with wildcard triggers and crop
0,0,32,38. Clear433 enters exploration regardless of533; held433 admits33
even with533 held, clearing433 in that completed-match branch but still
returning33 now. Completion alone does not block readmission. This differs
from the immediate battle-return path, which did not call CheckBattle.
Defeat/escape clear433 only if533 was already held; a subsequent town request
can therefore readmit33 after an unfinished attempt. No next encounter is
executed or assumed here.

## Actual content and execution consumers

**Confirmed about accepted repository code:** production consumes prepared
Content, not research/disassembly on demand. [Capability status](../../../../remake/docs/capability-status.md)
owns actual runtime claims. This slice creates no selected package or running
instance. Lowerable data, admitted content and observed execution are distinct.

| Actual surface | Expressible mechanism / selected boundary |
| --- | --- |
|Existing lifecycle/control/cutscene/global fixtures and immutable script pair|Shared static seams plus109 selected commands. Inline/list/text/action bodies are joined to the pinned source; no outcome, following or persistence observation.|
|[PrivateBattleEncounterReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [ExplorationContentReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)|Pin admits1/2/3, rejects33. Outcome-source also requires EnemyLeaderPresent=false and defeated program only End/Return: empty hook does not admit33's leader sweep. Missing selected map/encounter/party/outcome/script/material bindings; predecessor's Y38/crop-height38 region validation rejection remains.|
|[BattleOutcomeRules](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/BattleOutcomeRules.cs), [BattleActionCommitter](../../../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleActionCommitter.cs), [BattleOutcome](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs)|Loss precedence, positioned living actors, defeated gate, saved return before after, completion skip and ordered flag changes exist. Raw30/32 slots, supplied death lists, live-selector races, full leader cleanup and interrupted original services are not established by these shapes.|
|[Sf2OutcomeReturnPolicy](../../../../remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs), [MapPartyMembership](../../../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)|JoinForce-like rebuild/capacity and outcome return shapes exist. Active.First()/bound positioned actor, guarded nonzero status refresh and egress restrictions for absent399/held64 or640 differ from unchecked original state. No general33 retreat or empty-list/FF-facing parity is claimed.|
|[OriginalPrograms](../../../../src/sf2tool/remake_exploration_programs.py), [ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [EntityActionRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Actual selected lowering yields193 main instructions. Source command8 becomes main instruction11 native-call setBlocks: earliest selected unsupported operation, conditional on admitted content, valid earlier state/resources and returning services. Every selected nested Init/StopMoving/Immersed/2x-facing/Jump/JumpRight, inline movement and six custom speed/Idle streams contains no native-call. There is no later hidden native refusal preceding11. Content maps native to UnsupportedInstruction/UnsupportedEntityAction; runners refuse program-opcode/entity-action, not a successful placeholder.|
|[SceneEntities](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [MapTransfer](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs), [ExplorationViewRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs)|Separate scene-map/entity reconstruction, physical aliases, actions, following, fade/quake/view exist. BindScene requires settled black/closed windows/palette bindings. Prepared followers/capacity guards and entry-area/traversal validation need explicit31/39 assessment. FollowEntity lacks the original high-byte/Bowie-HP precheck; position/face shapes do not encode its ally-HP skip.|
|[MapBlockCopyAction](../../../../remake/src/Sf2.Remake.Domain/Maps/MapBlockCopyAction.cs), [map preparation](../../../../src/sf2tool/remake_exploration_maps.py), MapTransfer.Build|Prepared layout flag copies can express833/831 and door/roof mutations; selected eventMaps choose real setup/init/input instead of native frontier. This does not lower scene command setBlocks, which remains native11. No source-word decoding, selected layout/material binding or rendered-door/collision proof is supplied.|
|[ExplorationTextRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationTextRunner.cs), [ExplorationPresentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs)|PackedC0, names/LEADER rebuild, waits, animation/sprite refresh/fade/quake have consumers. Selected nineteen text/portrait/bleep/mapsprite/audio/palette resources and exact timing/input/persistence are unbound/unobserved.|
|[MapEventDispatcher](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs), [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Positive-coordinate no-scroll warp, ordered battle/setup selection and completed admission shapes exist. No selected31→39 world/route binding or original mutable request/high-byte/live-flag parity. Map39 is an outbound dependency, not a completed underground implementation.|

The isolated lowering is an in-memory query, not preparation, Application
execution, service observation or Godot acceptance. Earlier import/state/
material/service failures can precede11; no universal first runtime failure
is asserted. The current lowering gap is an implementation boundary, not a
reason to omit original command8 or weaken the original flags/geometry.

## Reproduction and remaining boundaries

Load ignored private selections in the same process as each command. Direct
source/table/command/inline/list/text/consumer audit, then committed scope:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1092/review.py <configured-source-disasm-root> --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The ignored audit checks full selected tables and source instruction widths,
branch polarity/order/aliases; all109 commands and inline actions/list/texts;
seed/crop/cleanup/drop/after-join/LEADER/follow-HP joins; defeat/escape/live
egress/raft; Map31 flags/setup/init/reconstruction and all five request rows;
recursive selected lowering and actual Content/Domain/Application/Godot
consumers. Review dead/unjoined/full/empty-party, held533 with clear833,
held833 without scene, supplied76/830/831/832/834/835, overwritten/missing
aliases, F88, stale death/list tails and interrupted requests/services.

Check exact two-path accepted-base→HEAD scope, clean status, navigation-only
predecessor, UTF8/LF/links/table shape/private boundary and staged diff. Record
exact head/tree/base and actual scoped CI in the Issue/PR and local handoff.
Planner selection is interpreted under current documentation-only policy;
its normal/full suite command is not run or relabeled PASS. No tests of this
audit, engine suites, SDK/Godot/emulator, screenshot, comparison or acquisition.

Preserve `local/issue1088/`'s original passing audit that missed the49-versus48
clear and accepted correction: ClearAiMemory writes48 last-target bytes and48
AI bytes, leaving the checked word untouched. Preserve `local/issue1090/`'s
failed helper attempts and accepted passing receipt, earlier nested-action/
LEADER/width/HP/geometry/N==V corrections and twelve completed comparison
failures through their owning handoffs. None is a rerun queue. Each new
receipt/lowering uses its own path; completed failures remain recorded.

**Unknown:** natural terminal/travel reach, actual EGRESS_MAP/flags/party/
HP/stats/classes/items/list/death/crop/alias/request/mode state, completed
cleanup/load/text/input/movement/follow/camera/fade/quake/sprite/DMA/audio
services, collision/visual meaning of selected layout words, interruptions,
timing, usable control and persistence. These do not authorize runtime work.
Done is a clean committed/pushed frozen Draft PR and stopped writer/process
handoff to independent main-gate. No merge, next Issue, cleanup or archive.
