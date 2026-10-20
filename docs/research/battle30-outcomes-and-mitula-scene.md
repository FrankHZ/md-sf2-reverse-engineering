# Battle30 outcomes and Mitula scene

Evidence date: 2026-10-20. **Confirmed (static):** Battle30 selects the empty
defeated hook with leader cleanup and the complete `abcs_battle30` after scene.
Its Ground Orb actors/effects do not award an inventory item or determine
the return seed. Victory saves the member-derived Map1 seed before the scene,
then joins0, clears430, sets530 and reaches exploration without another battle
scan. **Inferred:** the scene presents Zalbard's destruction, Mitula's release
and directions to storytellers/Grans. **Unknown:** natural outcome/reach,
actual supplied state, completed services/control, presentation/timing and
persistence. This is no Battle30 runtime or remake-readiness claim.

## Agreement and provenance

Standard rigor for [Issue #1068](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1068)
under [epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
terminal state, physical scene actors, narrative, item transactions, flags and
return parameters have distinct authorities. Accepted base is
`774931b594f1715c71823b6c916864cd76bac6ea`, including #1066/PR1067 and
#1064/PR1065. Exact ownership is this document and one continuation link in
the [activation/first-call owner](./battle30-activation-and-first-turn-dispatch.md).
No shared index, schema, fixture, tool, remake, design-guide or translation write.

Begin conditionally after ExecuteIndividualTurn returns. Reuse the checked
shared lifecycle in [Battle29 outcomes](./battle29-outcomes-and-shrine-exterior-route.md#conditional-terminal-and-escape-returns),
[Battle27 outcomes](./battle27-outcomes-through-tristan-admission.md),
[Battle26 outcomes](./battle26-outcomes-and-higins-recruitment.md),
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[lifecycle contract](../design/contracts/battle-control-lifecycle.md),
[cutscene routing](../design/contracts/battle-cutscene-routing.md),
[common scripting](./common-scripting.md), [request state](./map-event-request-state.md)
and [request consumption](./map-event-request-consumption.md).
The [opening owner](./battle30-initialization-and-opening.md) supplies the
Map1 profile, roster and follower-allocation boundary. Stop at MainLoop's
returned ExplorationLoop call, before destination setup, exploration, Taya
recruitment, outbound travel or the next encounter. No combat/AI simulation,
seed search, acquisition, emulator or supporting-platform expansion.

Source paths below are relative to `disasm/` at clean, read-only pinned
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Existing provenance binds USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM/H1/runtime equality is claimed. Constants/RAM are
`sf2enums.asm`/`sf2const.asm`; `sf2cutscenemacros.asm` owns command encoding.
Fnnn means flag nnn, indices are zero-based and address extents half-open.
Short `battleloop/` and `cutscenes/` paths are children of
`code/gameflow/battle/`; `map/` and `entity/` mean `code/common/scripting/`.

Minimum evidence is selected source/tables, the complete scene and existing
fixture/schema/consumer joins. Reuse the unchanged18,285,979-byte init/script
pair and accepted0.083s provenance in `local/issue1005/pair.txt`. Linear
reads/checks are budgeted below one minute/512MiB, with small ignored receipts
under `local/issue1068/`. No extraction, binary assets or API processing;
reassess contradictory semantics or material supporting machinery. ADR0004's
checklist applies proportionally; no parser/schema/test product is added.

## Conditional terminal processing and selected hooks

**Confirmed:** `battleloop_1.asm:BattleLoop` resumes from the individual turn,
optionally executes the debug kill-all branch, then orders defeated hook,
ProcessKilledCombatants and CountRemainingCombatants. Ally count0 loses before
enemy count0 wins. If neither terminates, ProcessAfterTurnEffects acts on the
actual current turn byte, followed by death processing and the same ordered
counts, with no second defeated hook. Only a nonterminal suffix advances the
turn offset by2. This does not execute an action or prove any natural outcome.

Complete selected tables and layout inclusion give:

| Table / complete extent | Battle30 selection |
| --- | --- |
|`data/battles/cutscenes/battleendcutscenes.asm`,48 relative words, `$47BE8..$47C48`|slot30/byte60 at `$47C24` selects ms_Empty.|
|`data/battles/global/enemyleaderpresence.asm`,45 bytes, `$47C8E..$47CBB`|byte30 at `$47CAC` is-1.|
|`data/battles/cutscenes/afterbattlecutscenes.asm`,48 relative words, `$47CF4..$47D54`|slot30/byte60 at `$47D30` selects abcs_battle30, `$4C6A8`.|
|`data/battles/cutscenes/afterbattlejoins.asm`,52 bytes, `$47D6A..$47D9E`|byte30 at `$47D88` is0; the layout includes this owner.|

`cutscenes/battleendcutscenesstart.asm` first requires Bowie HP.w!=0 and
enemy128 HP.w=0. Clear actual500+CURRENT_BATTLE (530 for30) dispatches ms_Empty;
held530 skips dispatch. Both continue through `battleendcutscenesend.asm`:
reread current battle, select leader-presence, then nonzero scans all32
enemies128–159 and appends each nonzero-HP slot to the existing death list,
incrementing length and rewriting its sentinel. The list is not cleared first.
The selected roster's sixteen declarations do not shorten this scan. The
separate `data/battles/global/afterbattlepositions.asm` contains only battle5;
no Battle30 leader-position script is selected.

The shared death consumer runs spin/effect/sound waits, credits ally defeats
or enemy kills to the supplied BATTLESCENE_FIRST_ALLY, writes combatant X/Y=-1,
clears status/recomputes stats and moves its mapped entity to `$7000,$7000`.
It neither writes current HP0 nor clears list length nor calls the item-drop
transaction. Thus cleanup can remove nonzero-HP enemies from the count by X.
Duplicate/stale death entries and mappings remain supplied inputs; an
interrupted list/animation/cleanup prefix has no rollback guarantee.

CountRemainingCombatants tests signed X.b>=0 and HP.w!=0 for30 allies/32
enemies; no Y, membership, activation, neutral or commandset predicate.
Bowie HP0 forces ally count0, including simultaneous exhaustion. High-bit
nonzero HP passes the count. Off-map enemies can yield victory without the
leader hook; poison can kill128 in the second pass without another hook at
that point. F88's supplied-state entry retains these live selectors/lists,
not an assumed fresh roster or scene. Changing CURRENT_BATTLE between reads
can change checked flag, hook, cleanup and later outcome tables independently.

## Enemy15 Mithril is a separate action-side transaction

**Confirmed:** complete `data/battles/global/enemyitemdrops.asm` has30
four-byte records and a word terminator, `$BE52..$BECC`. Record27 at `$BEBE`
declares Battle30, local enemy15, ITEM_MITHRIL123, droppedFlag27. The macro
encodes local15 as target combatant143. This flag is bit3 in byte3 of
ENEMY_ITEM_DROPPED_FLAGS, not story F27 or completion530.

`battleactions/dropenemyitem.asm:battlesceneScript_DropEnemyItem` is called
by action target processing after ApplyActionEffect (including the double/
counter paths), not by leader cleanup or abcs_battle30. Admission requires
an ally-side actor, enemy-side target, targetDies byte nonzero, matching
current battle and exact target, and the declared item actually present in
the target's inventory. `GetItemSlotContainingIndex` masks item identity.
Mithril is outside the Taros Sword/Iron Ball/Counter Sword random branches:
no1/32 roll is selected once this Mithril row qualifies.

The consumer sets droppedFlag27 **before** removing the target item. A held
bit returns without removal/award. A new bit removes the found slot, then a
nonzero-HP actor can AddItem. Success generates dropped/received messages;
dead/full actors take the rare-item-to-deals path. Item123's actual item
definition is RARE, so that fallback can add it to deals. These effects are
ordered mutations and generated messages, not a guaranteed received item or
completed presentation. No successful natural drop was observed.

The [accepted hidden143 boundary](./battle30-activation-and-first-turn-dispatch.md#region-hook-hidden-admission-and-actual-spawn-consumer)
still controls reach: fresh hidden143 has HP/maxHP0, X=-1, triggerFF and no
fresh Mithril inventory. Its unchanged region-gated spawn path cannot pass
the non15 trigger requirement. Conditional full reset can initialize the
row's407B declaration through AddItem as007B, stripping4000. Neither that
conditional initialization nor the drop table proves a natural reveal or
kill. Cleanup merely queues living enemies; it does not execute this action
consumer. The Ground Orb narrative below selects neither Mithril nor this row.

## Victory seed, after guard and final flags

**Confirmed:** `battleloop_2.asm:BattleLoop_Victory` first heals living and
immortal allies under the accepted all30-slot rules: Peter7/Lemon28 even at
HP0, other nonzero-HP allies, max HP/MP, retaining already-held STUN/POISON/
CURSE and recomputing stats. Other dead allies remain dead. No membership
filter; Battle44's timer branch is not selected for30.

Before the after wrapper it saves CURRENT_MAP.b to PARAM2; rebuilds joined/
active/reserve lists by member ID; takes the first active byte without a
length/HP check; adds crop X/Y as bytes to that member's actual X/Y into
PARAM3/4; resolves its mapped entity facing into PARAM5; writes PARAM1=0.
This is not a pending-type write. Empty active lists expose retained first
bytes. The selected member need not be Bowie, alive, placed or the first
turn actor. With unchanged Map1/crop `(0,0,24,34)`, map1 and actual member
coordinate bytes/facing are the seed, not fixed scene `(10,13)`.

Preserve the [unchecked mapping boundary](./battle29-outcomes-and-shrine-exterior-route.md#conditional-terminal-and-escape-returns):
only lookup0 selects physical0. PositionBattleEntities uses FF for an
unplaced ally; GetEntityIndexForCombatant returns it unchecked. Victory
shifts by5 and reads facing at `ENTITY_DATA+$1FE0+$10`. It does not apply
LoadBattle's negative fallback or3F mask. FF is neither zero alias nor a
valid entity guarantee; other changed mapping bytes also require assessment.
This read precedes scene entities clearing/rebuilding the lookup list.

`cutscenes/afterbattlecutscenesstart.asm` checks actual500+CURRENT_BATTLE:
clear530 runs abcs_battle30; held530 skips the entire scene. Both reach
`EndAfterBattleCutscene`, reread current battle, select joins byte0 and call
JoinForce0. That sets F0, rebuilds lists and sets F32 only when active count<12;
at capacity it does not clear an existing F32. A newly set active bit is not
in the already-built list until another rebuild. No Taya join, join ceremony,
stats reset or repair of the earlier return seed is supplied by this tail.

After the wrapper returns, BattleLoop rereads CURRENT_BATTLE, clears
400+index then sets500+index: unchanged30 clears430 then sets530. Intro480
is unchanged. It clears d0–d3 **words**, copies PARAM2–5 bytes and sets d4=1
with moveq before RTS; upper register halves are not established by clr.w.
Neither the checked flag nor selected script/join/final flags form an atomic
snapshot. An interruption after healing, seed, scene, join or430-clear does
not complete the remaining suffix or establish persistence.

## Scene population and live aliases

**Confirmed:** `data/battles/entries/battle30/cs_afterbattle.asm` spans
`$4C6A8..$4C994`. Its136 top-level commands encode618 bytes; ten inline blocks
add90 bytes, making708 bytes through csc_end to ce_4C96C. The40-byte list is
a six-byte main header, four eight-byte records and a two-byte terminator.
The retained corpus preserves top-level commands/source references; full
inline/list bytes are checked against pinned source, not inferred from a summary.

| Record / logical identity | Position / facing | Sprite operand / action |
| --- | --- | --- |
|Main /0|10,13 /Up|actual Bowie ally sprite /eas_Idle|
|Fixed0 /Peter7|12,13 /Up|ALLY_PETER7 /eas_Init|
|Fixed1 /128|11,10 /Down|ZALBARD155 /eas_Init|
|Fixed2 /129|63,63 /Down|MITULA215 /eas_Init|
|Fixed3 /130|63,63 /Up|ORB172 /eas_Init|

csc42_loadMapEntities disables services/display, reads main header words,
calls InitializeMapEntities, loads mapsprites and restores services/display.
ClearEntities replaces battle actors/mappings/walking scratch. Ordinary
flag followers allocate first; fixed Peter skips allocation if7 is already
mapped. With F follower allocations, Zalbard uses physical F+1 if Peter was
already allocated, otherwise F+2; Mitula and Orb follow at+1/+2. Main stays0.
These logical128–130 are new mapsprite actors, not combatant stats or
surviving enemies; no battle HP/items transfer is implied.

loadMapFadeIn is the shared fade/tileset/map-draw service, not MainLoop's
exploration transfer. With unchanged selectors it loads Map1 under battle30's
24x34 crop/area0; it does not run Map1 setup/init, reset current battle or
replace the saved return parameters. Its camera5,8 and the later actor header
are separate operands. No new shrine-exploration entry has occurred here.

The [accepted follower order](./battle26-return-through-pacalon-entry.md#conditional-map02-population-and-first-eventcontrol-read)
applies on Map1: F67 can supply Peter; F74→Astral31, F68→Oddler31 and later
F70→Astral31 can overwrite the same FOLLOWER_B alias. Other enabled followers
still consume slots. There is no fixed Astral record and no local enable of
those flags. Missing31 resolves its cleared zero lookup to physical0; another
live31 actor is not repaired by dialogue naming Astral. Logical31 is distinct
from physical raft31/lookup63 under F64/RAFT_MAP/mode. Arbitrary capacity,
changed selectors or duplicate mappings are not established safe.

## Complete scene phases

**Confirmed:** the ordered phases below account for all commands0–135 and
all six movement/four custom inline blocks. Narrative interpretations rely
on the text; scene operations alone do not establish story truth or completion.

1. **Stage Zalbard and the Orb (0–25).** Cursor2765; loadMapFadeIn Map1 with
   camera request5,8; load the list. Install/wait eas_Init on Bowie/Peter/31,
   set Peter12,13/Up and31 at11,13/Up. Stop128's animation; install/wait
   eas_DeactivateAutoFacing on130 and stop its animation. Set Peter priority0,
   31 priorityFFFF; fadeInB. Zalbard shivers/speaks;31 walks Up1 and asks for
   Mitula; Zalbard shivers/denies knowing. Wait40, face128 Up; custom128
   motion OFF/orientLeft/updateSprite/jump Idle; quake2, wait20, quake0.
2. **Light and Zalbard's disappearance (26–41).** Put130 at11,10/Up; custom
   speed48,48/jump Idle; setDest130 to9,11; Zalbard names the Ground Orb.
   Flash130 operand60; white fade-out, wait20, white fade-in. Face128 Down,
   shiver and light-intolerance text; custom motion OFF/orientUp/updateSprite/
   jump Idle, then install/wait eas_Die; wait60.31 attributes the victory
   to the Orb's holy light across continued/single text2770–2771.
3. **Mitula arrives and the Orb leaves (42–63).** Flash130/60; Peter jumps
   with wait and walks Up2; text2772. Camera request5,0; put129 at11,3/Down,
   flash129/60, welcome2773. Follow camera129; awaited movement Down4,
   Left2, Down4 ends conditionally at9,11. Face Bowie/31/Peter Left; wait50,
   flash129/60. Custom130 speed56,56/jump Idle; cameraSpeed38 hex, camera130;
   setDest130 to11,2, SLIDE_OUT130, camera request5,8; face129 Right.
4. **Identity, danger and the sealed power (64–92).** Mitula thanks them and
   addresses Astral;31 jumps with wait before surprise/request. Peter jumps
   with wait before questioning a goddess who declines foretelling.31 shivers
   at death; Mitula warns of full-power Zeon. Peter walks Left1, concluding
   they must act before full revival. Mitula names the Jewel of Evil;31 jumps
   with wait, understands the jewel demand, Peter suggests keeping it. Mitula
   explains Arc Valley's accumulation of evil, recovery over time, Ancient
   Tower/Granseal naming and its ground seal;31 jumps with wait and responds.
5. **Directions and disappearance (93–112).** Wait60, face129 Left, wait40,
   flash129/50. Submit Bowie and Peter jumps without joins, then31's awaited
   jump: waiting on31 does not join the other two.31 asks where she is going;
   Mitula says to save people, answers the abandonment question with a hint:
   hear storytellers before Grans Island. Flash129/30; she promises help after
   weakening Zeon, identifies the chosen leader/holy jewel, requests the holy
   sword to reseal Zeon, and begins describing its Grans location. MOSAIC_OUT129,
   wait50. No sword, jewel, Orb, flag or travel transaction occurs in these texts.
6. **Followers take their places (113–135).**31 walks Left2/Up1, wait5;
   submit eas_2xUpDown without wait, then csWait120 and call to Mitula. Peter
   walks Left1, wait30, faces Down/Left/Down separated by30 waits and a final
   wait30, then asks what to do.31 faces Right for Arc Valley directions,
   Down for storytellers; Bowie faces Up and nods. Install follow31→Bowie2,
   then Peter→31/2; csc_end. Offset2 resolves `(-24,0)` from the actual
   follower table. This installs following, without moving the victory seed.

With distinct intact aliases and all awaited actions returning, the commanded
pre-follow positions are Bowie10,13,31 at9,11 and Peter10,11; Zalbard's Die
ends at63,63, Mitula's movement at9,11 and the Orb's second destination11,2.
Mitula's mosaic and the Orb's slide modify graphics without relocating them.
Installing following can subsequently move31/Peter, so these are not a frozen
post-script projection, a promise of collision-free movement or an exploration
return tuple. Changed aliases can make these commands act on the same entity.

The main scene has no choice, conditional flag branch, inventory operation,
join, setStoryFlag, warp, native subroutine call or pending-map-event producer.
Its conditional behavior is in the shared consumers/aliases, not an omitted
story branch. The Orb actor is logical130/mapsprite172; no item identity is
encoded by the narrative name.

## Text sequence and portrait/control semantics

**Confirmed:** cursor2765 plus46 text commands selects gamescript IDs2765–2810
(`0ACD..0AFA`) in `data/scripting/text/gamescript.txt`, checked individually
against each source annotation. The complete join below gives text ID / top-level
command, live speaker and S=nextSingleText/T=nextText. Text meaning is paraphrased.

| Text / command | Speaker / mode | Content and encoded terminal control |
| --- | --- | --- |
|2765 /15|128 /S|Zalbard protests dying; W1.|
|2766 /17|31 /S|Where is Mitula; W1.|
|2767 /19|128 /S|Denies seeing her; W1.|
|2768 /29|128 /S|Names the Ground Orb; W1.|
|2769 /36|128 /S|Cannot endure the light; W1.|
|2770 /40|31 /T|Credits the Ground Orb for victory; W2.|
|2771 /41|31 /S|Holy power in its light; W1.|
|2772 /45|Peter7 /S|Look over there; W1.|
|2773 /49|129 /S|Welcome; W1.|
|2774 /64|129 /T|Thanks; W2.|
|2775 /65|129 /S|Returned to the world; W1.|
|2776 /66|31 /S|Recognizes Goddess Mitula; W1.|
|2777 /67|129 /S|Greets Sir Astral; W1.|
|2778 /69|31 /T|Surprised she knows his name; W2.|
|2779 /70|31 /S|Asks what to do; W1.|
|2780 /71|129 /S|Does not know; W1.|
|2781 /73|Peter7 /T|Surprise; W2.|
|2782 /74|Peter7 /S|Asks about foreknowledge; W1.|
|2783 /75|129 /S|Declines telling the future, unlike Volcanon; W1.|
|2784 /77|31 /S|Alarm at death; W1.|
|2785 /78|129 /T|Zeon's strength; W2.|
|2786 /79|129 /S|Full power exceeds both gods; W1.|
|2787 /81|Peter7 /S|Act before full revival; W1.|
|2788 /82|129 /S|Power sealed in Jewel of Evil; W1.|
|2789 /84|31 /S|Explains the jewel demand; W1.|
|2790 /85|Peter7 /S|Suggests keeping the jewel; W1.|
|2791 /86|129 /T|Arc Valley gathers evil power; W2.|
|2792 /87|129 /S|Power recovers there over time; W1.|
|2793 /88|31 /S|Repeats Arc Valley; W1.|
|2794 /89|129 /T|Ancient Tower/Granseal names; W2.|
|2795 /90|129 /S|Tower symbol seals the ground; W1.|
|2796 /92|31 /S|Understands; W1.|
|2797 /100|31 /S|Asks where Mitula is going; W1.|
|2798 /101|129 /S|Surface, to save people; W1.|
|2799 /102|31 /S|Asks if she will abandon them; W1.|
|2800 /103|129 /T|Offers a hint; W2.|
|2801 /104|129 /S|Hear storytellers before Grans; W1.|
|2802 /106|129 /T|Weaken Zeon for the gods' help; W2.|
|2803 /107|129 /T|LEADER chosen, holds holy jewel; W2.|
|2804 /108|129 /S|Reseal Zeon, find holy sword; W1.|
|2805 /109|31 /S|Asks where/how; W1.|
|2806 /110|129 /S|Grans, jewel will lead onward; W1.|
|2807 /117|31 /S|Calls Mitula; W1.|
|2808 /126|Peter7 /S|LEADER, Mitula vanished, what next; W1.|
|2809 /128|31 /S|Arc Valley on Grans Island; W2 despite S.|
|2810 /130|31 /S|LEADER, find storytellers first; W1.|

All129 text from2774 onward uses modifierC0 (packed C081); initial welcome2773
uses0081. All other text uses00 with its selector. csc1D tests packed bits15/14
for right-side/mirror and looks up the **live mapped mapsprite** for portrait/
speech. It returns early when a portrait is already open. C0 is not an entity
number, speaker change or a universally unsupported text mode. Expected intact
Zalbard155 maps to portrait43/bleep7, Mitula215 to36/bleep3, Astral to31/bleep6;
Peter follows his actual ally appearance/promotion. Changed31/missing aliases
can produce another speaker appearance even while the text names Astral.

Both text handlers wait for view scrolling, display the current cursor and
increment it. S closes portrait/text and sleeps10; T leaves windows for
continuation. W1/W2 are encoded text controls, independent of S/T (notably2809).
N and LEADER substitutions remain text-program semantics, not new commands
or writes to membership/jewels. Debug P2 Start can latch text skipping:
skipped S/T consumes its operand without displaying/incrementing, and csWait
is skipped too; it does not skip all actions/effects. No measured dwell,
held-input behavior, complete display or audible speech was observed.

## Action, effect and interruption boundaries

**Confirmed:** `map/mapscriptengine_1.asm`/`_2.asm`,
`entity/entityscriptengine_1.asm`/`_2.asm` and
`data/scripting/entity/eas_actions.asm` provide the actual consumers:

- setActscriptWait/customActscriptWait wait for the actor's script address
  to equal eas_Idle; inline entityActionsWait compiles movement plus an Idle
  jump and joins that actor. Non-Wait installs do not join. eas_Init resets
  speed32/32, collision/auto-facing/orientation/size/motion-related fields,
  not combatant HP, inventory or membership. stopEntity writes animation
  counterFF; it is not a death or movement-clear command.
- setDest130 has bit15 clear both times: write scaled X/Y destinations and
  travel distances, velocities signed32 by direction, then wait for movement
  to stop. Custom ac_setSpeed48/56 writes XSPEED/YSPEED bytes; csc29's velocity
  writes are distinct. Do not derive a48/56-speed trip duration from those
  operands or assume setDest is asynchronous. Camera/obstruction/services
  can affect completion; no walked path or natural uninterrupted trip is proven.
- eas_Die disables animation motion, rotates, requests death sound, switches
  to EFFECT1, waits/faces and moves the physical actor to63,63 before Idle.
  It does not kill combatant128, award a drop or write530. Mosaic/slide FX
  alter sprite graphics through DMA, not HP/inventory or logical deletion.
- setPriority normalizes0 to byte0 and any nonzero (FFFF here) to1 in the
  per-physical-entity priority array. Quake2/0 writes amplitude directly,
  without the ramp-bit branches. shiver temporarily changes sprite-size/
  animation fields through three set/clear cycles and restores them on return.
- entityFlashWhite uses `(operand>>2)+1` DBF cycles:60→16,50→13,30→8,
  each sets/clears sprite flag bit2 with two VInt waits per half. The scene
  has four60 flashes, one50 and one30; these counts are not measured durations.
  White map fades use LaunchFading with palette mask1/period1 and wait for
  the fading service; this scene has no flashScreenWhite command.
- setCamDest clears view target toFF, writes a tile destination and waits
  for scrolling. setCameraEntity follows the live physical mapping.
  cameraSpeed38 hex writes VIEW_SCROLLING_SPEED=56. The csc_end tail waits
  for view scrolling only when DIALOGUE_WINDOW_INDEX is nonzero, clears
  VIEW_SCROLLING_SPEED and restores interpreter registers; it does not
  restore actors, following, priority, crop or PARAM2–5.

setPos/setFacing/stopEntity/followEntity use the live ally-HP skip helper
where selected: allies0–29 at HP0 skip their command;31 and128–130 bypass
that test. Referenced action/text/camera/flash/custom consumers do not all
share that gate. Victory normally revived Peter first, but changed/dead
Peter or Bowie during the scene can still differ. A missing31 aliases main0,
not a skipped command; a supplied FF mapping is an unchecked entity-data
address, not main0. No arbitrary live-alias/capacity safety is established.

The six movement streams and four custom streams, named actions, text waits,
flash/fade/DMA and camera waits are non-atomic. An interrupted prefix can
leave actors replaced, motion/priority/quake/palette/camera/lookup changes,
or open windows without completing later text/FX/following/flag writes.
csWait120 after eas_2xUpDown is a source wait request, not a measured proof
of its completion;31's waited jump likewise does not join Bowie/Peter.

## Defeat, retreat and next exploration entry

**Confirmed:** defeat calls UpdateBattleUnlockedFlag (clear430 only if530
already held), clears dialogue name1, requests sad music/text363/close,
restores Bowie HP=max, halves unsigned gold, gets egress position and
returns d4=-1. Battle04's losable upgrade does not apply. Admitted Egress
deducts its selected MP cost before the cast/escape tail; admitted Angel
Wing removes its selected item slot/recomputes stats before that tail.
Both update unlock, unwind the action frame, select egress and return
d4.w=0, without the defeat gold/Bowie effects. Escape bypasses the post-turn
terminal pass. Neither path newly sets530, runs after/join, or changes480;
earlier combat/cost/inventory/flag writes are not rolled back.

`battleloop/getegresspositionforbattle.asm` special-cases13/16/17/31/32
and38/39 unlocks, **not30**. It copies live EGRESS_MAP.b into d0's low byte;
`code/common/maps/egressinit.asm:GetSavepointForMap` scans the complete
23-row `data/maps/global/savepointmapcoords.asm`, then its terminator.
BattleLoop set399 on fresh and F88 entry. Under that held399 premise:

| Supplied EGRESS_MAP | Actual selected return |
| --- | --- |
|15|Tristan `(12,16)` Up.|
|2|Pacalon `(41,10)` Up.|
|36|Pacalon castle `(40,5)` Up.|
|1,0 or35|No matching row; retain that map byte, default `(1,1)` Up.|

This is configured live egress, not a guaranteed shrine/save/church return.
Matched fields are byte writes; d0's incoming high word-byte is not globally
cleared. F399-clear direct entry instead returns initial3 `(56,3)` Down
before raft reset, outside the ordinary reached premise. Held64 resets raft
from the complete four-row table: Hassan→69 `(28,55)`, Polca/Bedoe→72
`(43,48)`, default→69 `(42,15)`; clear64 leaves raft fields unchanged.
Do not infer an egress destination from the scene or fill supplied EGRESS_MAP
with an assumed most recent church.

At `code/gameflow/mainloop.asm:alt_MainLoopEntry`, every completed battle
return calls SwitchMap then ExplorationLoop, with no d4 sign test or
intervening CheckBattle. The complete switched-map table contains only
3/F640→4,71/F530→78 and58/F999→59, first set matching row with word map
comparison. Unchanged victory1 therefore reaches explicit Map1 exploration;
even newly held530 does not switch1. Changed return71 can switch78 under
that same flag. Defeat/retreat follow their actual returned word/map selection.

abcs_battle30 and all selected actions above contain no PARAM2–5 or pending
write: mapLoad/camera/entity/movement/FX operate their own state. Consequently
intact victory returns the pre-scene member seed, even if the scene main is
at10,13 with a new follower chain. Those scene actors are not an alternative
exploration seed or destination initialization contract. Stop immediately
before ExplorationLoop's body; resetting pending/current battle, selecting
Map1 setup, Taya interactions and usable exploration belong to the next owner.

## Current representations and actual consumer gaps

**Confirmed about the accepted repository:** research evidence, prepared
Content and executed Application/Godot are separate. The
[capability owner](../../remake/docs/capability-status.md) controls runtime
claims. No Battle30/Map1 package or runtime is created here.

| Surface | Actual mechanism | Classification / remaining boundary |
| --- | --- | --- |
|Evidence|Existing battle-loop/control/cutscene/global/drop fixtures and schemas; retained script corpus136 commands and source addresses.|Expresses static seams; full inline/list/text joins are pinned-source checks, not runtime scene/reward acceptance.|
|Private encounter admission|[PrivateBattleEncounterReader.Pin](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) admits1/2/3.|Explicit rejection for30. Missing30 roster/live-party/policy/outcome/script/material bindings; no runnable30 package.|
|Scene and actions|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py) can lower scene load/list, Init/custom speed/orientation, movement/waits, stop/priority, quake, text modifiers, following, shiver/nod/jump; [SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), ProgramRunner and EntityActionRunner consume them.|Expressible mechanisms; exact30 population/aliases, missing/dead/interrupted behavior and physical materials remain missing binding or unassessed. SceneEntities has explicit capacity/population guards, unlike unchecked original allocation.|
|Direct lowering refusal|Isolated source lowering yields native-call for setDest twice (commands28/60), cameraSpeed58 and SLIDE_OUT61. eas_Die38 contains native ac_soundCommand/ac_setSprite/ac_setPos.|[Content reader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs) maps native-call to UnsupportedInstruction/UnsupportedEntityAction; actual runners fail program-opcode/entity-action. First linear top-level refusal is setDest28. No silent successful placeholder or general unsupported-destination-name assumption.|
|FX and text|Entity flash, white fade, MOSAIC_OUT, quake/priority and packedC0 lower to concrete instructions/cues; [Godot presentation](../../remake/game/src/Exploration/ExplorationPresentation.cs) admits flash-white/mosaic-in/out/slide-in and gestures, with white overlay/reduced-flash behavior.|Expressible, needing source-specific material/timing/binding acceptance. SLIDE_OUT is absent from both lowering and effect admission. The opening's flashScreenWhite gap remains, but this after scene does not call it.|
|Map1 profile|[Opening profile survey](./battle30-initialization-and-opening.md#existing-representations-and-actual-consumer-gaps) checked all three Map1 area declarations against ExplorationViewRunner/BattleMapViewport.|Ordinary declared planes pass existing fixed1:1 guards; missing Map1 materials/crop/animation/flag-copy binding and exact presentation remain. Original first-area fallback versus current null/error selection is an explicit changed-state gap; Map0's plane-motion rejection does not transfer.|
|Outcome/return|[BattleOutcome](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs) and [Sf2OutcomeReturnPolicy](../../remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs) select/retain return, skip after on completion, join and publish unlock/completion before return program.|Mechanisms exist; no30 route/after/join/egress binding. Source policy's active.First()/required actor/position handling does not emulate unchecked empty-list/FF reads; defeat flags64/640 or absent399 and nonzero return status have explicit guards. General retreat/suspend, live-selector races and exact30 parity remain unassessed/unsupported under current capability scope.|

The direct lowering probe constructs only in-memory source programs and a
small ignored receipt; it is not content preparation, an admitted scenario,
Application execution or Godot acceptance. Production does not read research
JSON/disassembly on demand. No engine/preparer implementation is changed.

## Reproduction and remaining boundaries

Load `local/private-inputs.ps1` in each launching process. Reuse the retained
pair/provenance without re-extraction. The ignored direct review checks the
clean pin; lifecycle/control/cutscene/global/drop fixtures/schemas and actual
selected consumers; complete hook/join/drop tables;136 commands/ten inline
blocks/list and46 source texts; portrait/action/FX/camera/return boundaries;
actual lowering/Content/Application/Godot guards and full prose scope:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1068/review.py <configured-source-disasm-root>
git diff --check
```

Review every phase/text/changed-state relationship, UTF-8/LF, relative links/
anchors, exact two-path/navigation-only diff and private boundary. On clean
committed HEAD run `uv run sf2 verify plan --base origin/main --head HEAD`;
record actual exact-head scoped CI in the Issue/PR handoff. Documentation
scope skips blanket normal/full Python/.NET/Godot/H3/H4 and tests of review
programs. Retain #1066's original dd04819d/0.344s/CI39457011144 and missed
mixed-axis prose derivation/P2 diagnosis, correction ccf14c4f/0.365s/
CI39457758889; #1064/#1062 mapping correction and earlier failures/Unknowns
remain with their owners. Twelve completed old remake comparison failures
remain failure evidence, not a rerun or repair queue for this slice.

**Unknown:** natural outcome, hidden143 reveal/drop, incoming flags/HP/items/
membership/turn/death-list/mappings/EGRESS_MAP/current selectors and crop,
completed scene/text/action/camera/FX/audio/DMA, interrupts and actual alias/
capacity behavior, presentation/timing, usable return control and persistence.
Missing30 binding and explicit Unsupported do not erase expressible mechanisms;
expressibility does not establish acceptance. Map1 exploration/Taya and onward
travel/encounters remain outside this boundary, not automatic acquisition work.
