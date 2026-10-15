# Battle26 outcomes and Higins recruitment

Evidence date: 2026-10-14. **Confirmed:** Battle26 can finish through its
leader-death cleanup or the ordinary zero-enemy count, with defeat taking
precedence. Its separate Black Ring drop requires a qualifying action-side
target and inventory transaction. First victory runs the rescue scene and
joins Higins; the depicted recovery does not itself heal his stats. BattleLoop
returns the map/position/facing seeded before that scene, not its staging
coordinates or the spoken destination Pacalon. **Inferred:** the cloud's
departure represents the threatened possession being averted. **Unknown:**
natural reach, actual outcome/state/reward, completed services, presentation,
usable exploration and persistence.

## Agreement and provenance

Standard rigor for [Issue #1013](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1013)
under [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
terminal predicates, drop handling, scene actors, membership, following and
return registers have different owners and lifetimes. Accepted starting base
is `d2424acb1c698b36746ce04f471e6ffa8f9b28df`. This is the sole tracked path;
no shared index, registry, contract, fixture, schema, tooling, design, remake,
glossary or Chinese write. Independent #1014 synthesis and UI692 supply no
evidence or stacked dependency.

Consume the accepted [first-turn dispatch](./battle26-activation-and-first-turn-dispatch.md),
[initialization and roster](./battle26-initialization-and-roster.md),
[admission and confrontation](./battle26-admission-and-geshp-confrontation.md),
[Battle25 outcomes](./battle25-outcomes-through-ketto-entry.md),
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[battle actions](./battle-actions.md), [drop evidence](./runtime-rng-and-battle-math.md#confirmed-enemy-item-drop-data-and-core-runtime-branches),
[common scripting](./common-scripting.md) and
[party/follower continuation](./ribble-may-through-battle10-admission.md).
Connect shared action/after-turn consumers to outcomes without simulating
ExecuteIndividualTurn, tactics, damage or a combat transcript. Stop at
BattleLoop's return to MainLoop, before exploration reset/load/control or
later town/battle admission. No new runtime, RNG injection, acquisition,
extraction regeneration or generic rewards/menu platform is included.

Source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The retained read-only checkout is clean at that pin. The accepted USA retail
ROM is SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
`sf2enums.asm` and `sf2const.asm` own constants/RAM; Fnnn denotes game flag
nnn, distinct from the enemy-item dropped bitfield. Short battle paths are
under `code/gameflow/battle/`; map/entity/text consumers under
`code/common/scripting/`. Ranges are half-open and command indices zero-based.

Minimum evidence is selected pinned source, existing fixtures and the retained
18,285,979-byte map-init/script pair. Reuse its dependency-valid 0.083s receipt;
new source/program/literal joins are linear, budgeted below one minute/512 MiB,
with small prose/receipts. No new maintained checker or installation. Replan
contradictions or supporting-tool expansion; Unknowns are not a runtime queue.

## Terminal predicates and leader cleanup

**Confirmed:** `battleloop_1.asm` resumes after ExecuteIndividualTurn returns,
optionally applies the debug kill-all branch, then at `@IsBattleEnd` calls
ExecuteBattleCutscene_Defeated, ProcessKilledCombatants and
CountRemainingCombatants. Zero allies selects defeat **before** zero enemies
selects victory. If both counts remain nonzero, it reads the actual current
turn actor, calls ProcessAfterTurnEffects, processes deaths and repeats those
ordered count tests. There is no second defeated hook in that status pass.
Only after neither terminal branch does it advance CURRENT_BATTLE_TURN by2.
`battleloop/processafterturneffects.asm` clears death-list length on entry
and skips its effects for HP0. Its lethal-poison arm increments that cleared
length and writes the current actor into list byte0 before returning through
stat/text cleanup. That is a new status-pass list, not replay of the earlier
action deaths; the shared status/recovery rules remain with the battle-loop owner.

| Selected source row | Value and actual consumer |
| --- | --- |
|`data/battles/cutscenes/battleendcutscenes.asm`, slot26/byte52, `$47C1C`|Relative `ms_Empty`; defeated wrapper.|
|`data/battles/global/enemyleaderpresence.asm`, slot26, `$47CA8`|Byte-1, unlike Battle25's0; nonzero admits living-enemy append.|
|`data/battles/cutscenes/afterbattlecutscenes.asm`, slot26/byte52, `$47D28`|Relative `abcs_battle26` at `$4BCAA`; after wrapper.|
|`data/battles/cutscenes/afterbattlejoins.asm`, slot26, `$47D84`|Byte0; wrapper calls JoinForce(0), not19.|
|`data/battles/global/afterbattlepositions.asm`|Complete table has only Battle05; no26 position override.|

`layout/sf2-07-0x044000-0x064000.asm` selects the **cutscenes** join table,
not the similarly named global copy. Empty script is the `$FFFF` terminator
at `$47A4E`; its interpreter still waits for scrolling if dialogue is open
and clears VIEW_SCROLLING_SPEED before returning.

`cutscenes/battleendcutscenesstart.asm` requires Bowie's HP.w nonzero and
enemy128's HP.w zero. Clear526 executes the empty end script; held526 skips
only that script. Both reach `battleendcutscenesend.asm:loc_47C48`. Its
slot26 byte-1 scans128–159 and appends every **nonzero-HP** slot to the supplied
DEAD_COMBATANTS_LIST, writing a trailing byte-1 and incrementing its word
length after each append. It does not require X>=0, a roster row, activation,
nonneutral state, or an empty/deduplicated list; it does not zero HP.

The actual initial128 is Lizardman, not Geshp or Higins. His AI priority and
the leader-presence table are distinct. If128 dies with Bowie alive, the
other living enemies are queued for cleanup even if they retain HP. Fresh
neutral159 has HP0 and is not appended; supplied nonzero HP changes that.
`ApplyPositionsAfterEnemyLeaderDies` has the same Bowie/128 HP guards but
finds no26 table row, so it performs no relocation here.

`battleloop/processkilledcombatants.asm` consumes the length-delimited list,
performs spin/sound passes, increments ally defeats or kills for
BATTLESCENE_FIRST_ALLY, writes listed combatant X/Y=-1 and status0,
recomputes stats and moves mapped entities to `$7000,$7000`. It neither
zeros listed HP nor clears list length. It calls no enemy-drop transaction.
Thus leader cleanup can eliminate130 from the count without awarding its
ring, and stale/duplicate lists or interrupted services are not repaired.

`battleloop/countremainingcombatants.asm` scans all30 allies and32 enemies.
Nonnegative X.b and nonzero HP.w count; Y, force membership, AI, regions and
neutral bits are not predicates. Bowie HP0 then forces ally count0. With
positive ally count, zero counted enemies wins even without leader death
(for example, supplied offscreen enemies). Bowie death or zero counted
allies wins simultaneous exhaustion. Zero HP/counts alone do not prove every
death handler completed. A leader dying in the post-status pass receives
that pass's death/count handling, without immediately rerunning the leader
append hook. F88-supplied continuation uses actual current slots/lists/flags,
not an assumed fresh fifteen-enemy formation.

## The independent Black Ring drop

**Confirmed:** `data/battles/global/enemyitemdrops.asm` row10 at
`$BE7A..$BE7E` is `OUTSIDE_KETTO, enemyEntity 2, BLACK_RING, droppedFlag 10`.
`sf2macros.asm:enemyEntity` emits ordinal+128, so its four bytes are
`26,130,24,10`. Target130 is the accepted Dark Bishop row2, whose inventory
initialization includes Black Ring. The flags stripped by successful AddItem
during initialization do not disable this separate index-based table rule.

`battleactions/battleactionsengine_1.asm:WriteBattlesceneScript` calls
`battlesceneScript_DropEnemyItem` after ApplyActionEffect for each target and
after admitted second/counter attacks, with their actual actor/target pointers.
The per-target targetDies byte is reset before effect handling;
`inflictdamage.asm:@CheckTargetDies` sets it after DecreaseCurrentHp/GetCurrentHp
returns zero. This is a conditional call-chain join, not a new attack audit
or a claim that every zero-HP enemy went through it.

`battleactions/dropenemyitem.asm` (`$BD24..$BE52`) orders the selected branch:

1. Reject an enemy actor, an ally target, or zero targetDies. Match CURRENT_BATTLE
   and target byte against table rows. `GetItemSlotContainingIndex` scans the
   target's four slots for item index24, masking entry flags; absence continues
   the table scan and eventually returns. Equip, broken, AI-use or drop bits
   are not the match condition.
2. Black Ring is neither Taros Sword, Iron Ball nor Counter Sword, so it takes
   the unconditional drop arm without drop RNG or an upgrade/completion test.
   The upgrade-related instructions after unconditional branches are unreachable.
3. Divide flag10 by8: byte1, bit2 of ENEMY_ITEM_DROPPED_FLAGS (`$FFF7A5`, mask4).
   `bset` occurs **before** removal/delivery; an already-set bit returns without
   either. This is not joined F10 or completion526.
4. Remove the selected target slot through RemoveItemBySlot, shifting later
   items left, filling the last slot with NOTHING and recomputing target stats.
   Then read the acting ally's HP. Zero HP bypasses receipt; otherwise AddItem
   scans that actor's first empty slot, not Bowie, the active party or caravan.
5. With capacity, AddItem stores plain index24 (the table supplied no broken
   bit) and returns d2=0. Only then are dropped/received message commands
   appended for target and actor. Rendering those commands is a later service.
   No automatic equip, gold award or force-wide recipient search occurs here.
6. Dead actor or full inventory goes to the rare-item fallback. The selected
   Black Ring definition is RING|RARE|CURSED|BREAKABLE, so AddItemToDeals
   increments its high-nibble quantity at DEALS_ITEMS+12 (`$FFF610`) unless
   already15. The enemy item and flag are already consumed even at saturation;
   this branch emits neither of the two successful-receipt messages.

The table scan is independent of the ring's usable BLAZE2 or equip class.
While bit10 remains held, respawn, repeat victory, different recipient or a
fresh copy of the item does not permit another transfer. This function has
no rollback: interruption after the bset can suppress a future retry before
removal or receipt; interruption after removal can leave no delivered ring.
Nothing here proves save durability, the supplied bit's history, actual item
possession at death or an observed reward. Leader/list cleanup, after-turn
death and terminal victory are not substitute callers.

The existing `tests/fixtures/h2/enemy-item-drops-v1.json` owns table/consumer
boundaries; its aggregate counts alone do not identify this row. The ten-case
`tests/fixtures/h3/enemy-item-drop-behavior-v1.json` owns controlled Battle01
entry tests with replaced drop inputs, including prior flag, dead/full actor
and deals saturation. It is not a natural Battle26/Black Ring observation.

## Victory seed and after-scene admission

**Confirmed:** `battleloop_2.asm:BattleLoop_Victory` first calls
HealLivingAndImmortalAllies. It scans all30 ally slots, independent of joined
or active flags. Nonzero HP, or immortal Peter7/Lemon28 even at HP0, receives
max HP/MP, status masked to STUN|POISON|CURSE and stat recomputation. Other
dead allies remain dead. In particular, living ally19 can be healed here
before recruitment; dead Higins is not revived by this predicate. The
separate neutral159 does not transfer HP/status into ally19. Battle44's timer
exception does not apply.

It then writes PARAM2=CURRENT_MAP; UpdateForceAndGetFirstBattlePartyMemberIndex
rebuilds joined/active/reserve lists in ascending ID and reads the first active
byte without checking list length or HP. X/Y of that combatant plus the battle
area origin are added as bytes into PARAM3/4. GetEntityIndexForCombatant,
scaled by32, supplies mapped entity facing for PARAM5. PARAM1 becomes0;
MAP_EVENT_TYPE is not written. Ordinary Map72/Battle26 origin `(28,0)` thus
seeds Map72, `((x+28) mod256,y mod256)`, and that actual facing. It is not
necessarily Bowie, a formation coordinate, or a valid/onscreen living member.
An empty active list exposes its retained first byte; altered maps, crops,
HP/positions or mappings change the seed.

`cutscenes/afterbattlecutscenesstart.asm:ExecuteAfterBattleCutscene` tests526.
Clear dispatches exact slot26 to abcs_battle26; held skips it. Both eventually
reach EndAfterBattleCutscene and JoinForce(0). No test of joined19 substitutes
for526, and no526 write precedes the scene. A repeated victory with526 held
does not replay rescue, recruit Higins or rebuild his scene follower.

The accepted admission chain gives the flags their separate roles: request
helper26 sets unlock426; CheckBattle can select26 and clear426 if526 was
already held. Before-scene dispatch checks intro476 without setting it;
the later fresh start wrapper sets476 before its empty start script. Those
are earlier writes, not recruitment/completion. F88 continuation bypasses
fresh before/start dispatch and retains supplied state. This outcome path
neither retroactively proves that introduction completed nor rewrites476.

## Scene load, actors and physical mapping

**Confirmed:** `data/battles/entries/battle26/cs_afterbattle.asm` contains
abcs_battle26 `$4BCAA..$4BE4A` and ce_4BE4A `$4BE4A..$4BE6A`. The retained
`programCorpus.programs[id=abcs_battle26]` covers source lines4–98:72 top-level
commands,348 encoded header/operand bytes,48 inline custom-action bytes and
20 packed movement bytes, totaling416 script bytes. The list is32 bytes:
six-byte main header, three eight-byte declarations and a two-byte terminator.
These counts support the complete ordered comparison below, not its semantics.

csc37 sets fade-to-black state and falls into csc48: disable view target,
load map tilesets, wait for fading, deactivate VInts, LoadMap72 with packed
camera `(13,11)` scaled by3, re-enable display/VInts and wait one VInt.
`code/common/maps/mapload.asm:LoadMap` writes CURRENT_MAP72 and loads its
data/view; it does not set CURRENT_BATTLE255 or rewrite PARAM2–5. This is not
the ExplorationLoop population/temp-flag/healing/setup-init lifecycle. Camera
coordinates are not player coordinates. The later fadeInB calls FadeInFromBlack.

csc42 disables display/VInts, reads the main header, initializes actors and
loads mapsprites, then re-enables them. `map/mapfunctions.asm` first clears
entity state/lookup and resets walking scratch, creates eligible followers
from physical1, processes the fixed list, then declares Bowie at physical0.
This replaces the battle neutral actor/mapping: combatant159 with script B31
from the accepted battle load is not the scene's logical ally19.

Let N be the actual number of admitted follower rows, and P=1 if Peter's
lookup was absent after those rows, otherwise0:

| List record | Initial position/facing | Logical lookup and physical allocation |
| --- | --- | --- |
|mainEntity|19,16 /Up1|Bowie0, physical0; initially eas_Idle|
|Peter7|17,14 /Down3|physical N+1 if absent; otherwise skip fixed row and retain follower|
|Higins19|18,14 /Down3|physical N+P+1; no persistent Higins follower row|
|EVIL_CLOUD106|63,63 /Up1|NPC logical128, lookup offset32, physical N+P+2|

Fixed ally rows deduplicate against any nonzero lookup, without a membership
test; NPC Cloud is an ordinary sprite30..239, not the special physical47
case. InitializeMapEntities scales positions by384; DeclareNewEntity writes
position/destination/facing/sprite, action and ENTNUM/layer, and clears
travel/velocity. Fixed rows use eas_Init.
Neither ally19's actor presence nor its sprite establishes membership.

On Map72, F65 enables the Caravan A30 prefix. The remaining complete follower
table is66/Sarah,67/Peter,72/Kazin,73/Slade,74/B/Astral,66/Chester,68/B/Oddler,
75/Luke,69/C/Fairy,70/B/Astral,74/A/King,71/Lemon,74/D/Minister,76/Zynk,
77/A/Old Man,82/Frayja,83/A/King,83/D/Minister,84/Rohde, in source order.
A/B/C/D are logical30/31/158/159 (lookup offsets30/31/62/63), not fixed
physical IDs. Duplicate alias rows allocate actors and overwrite the lookup;
only fixed ally rows deduplicate. Five follower action pointers and unguarded
finite storage do not guarantee safety under arbitrary mass-enabled flags.
For coherent65/67/70 only, N=3/P=0: A1, Peter2, B3, Higins4, Cloud5.
F64/RAFT_MAP72 can additionally declare raft physical31; it is not B31.

Absent A/B lookup0 aliases physical0; hidden$FF/corrupt mappings are not safe
no-ops. Their setPos commands can therefore move the main actor under altered
flags. PLAYER_TYPE was cleared by BattleLoop; supplied mode2 has its separate
population tail rather than a universal normal-mode guarantee. GetAllyMapsprite
uses actual class; with CURRENT_BATTLE26 its dead-blue-flame branch is bypassed.
SetPos, setFacing and startEntity skip actual dead allies0–29 through
AdjustScriptPointerByCharacterAliveStatus; follower aliases/NPCs bypass it.
Action installation, headshake, nod, priority, text and addNewFollower do not
inherit that HP guard. Dead/missing actors therefore do not produce one
uniform skip behavior.

setPriority19,0 clears its physical byte in byte_FFAFB0;128,$FFFF stores1 in
Cloud's byte. The nonzero operand is Boolean, not a signed layer value-1.
`entity/entityscriptengine_1.asm:sub_4E24` uses those bytes to build VDP sprite
links in two passes: nonzero physical entries first, then zero entries.
These writes select that ordering; they do not change combat priority, HP,
membership or lookup allocation, or alone establish a rendered overlap.

## Complete rescue program and action consumers

**Confirmed:** these ranges exhaust commands0–71. Tn denotes the exact
dialogue ID in the following table. All waits are source service/pointer
conditions, not measured durations or observed completion.

| Commands | Ordered operations including every inline body |
| --- | --- |
|0–12|textCursor2637; loadMapFadeIn72,13,11; list ce_4BE4A; waited Bowie Init; waited Higins custom motion OFF/orientLeft/updateSprite/jump Idle/end; waited Peter Init; setPos Peter17,14,Right; waited B Init; setPos B18,16,Up and A19,17,Up; priority Higins0/Cloud nonzero; fadeInB.|
|13–19|waited Peter Down2/end; face Peter Right; T2637–2638; flash B60; screen flash30; camera13,9.|
|20–29|Cloud SLIDE_OUT; setPos Cloud18,13,Down; SLIDE_IN; face Peter Up; waited Cloud Up3/end; waited Cloud custom speed64,64/jump Idle/end; asynchronous EvilGizmoCircle; csWait200; waited Cloud Die; camera13,11.|
|30–40|waited Higins custom motion OFF/orientUp/updateSprite/jump Idle/end; face Down; headshake; T2639; waited Higins Up1/end; waited Jump; T2640; waited Higins Down2/end; T2641–2643.|
|41–51|T2644–2646; waited Peter Up1/end; face Peter Right; T2647; face Higins Left; T2648–2649; face Higins Down; T2650.|
|52–61|nod Higins; T2651; face Higins Left; T2652–2654; join19; T2655; face Higins Down; T2656.|
|62–71|asynchronous Higins RotateRight; csWait40; waited Jump; face Down; waited Jump; waited Init; startEntity19; T2657; addNewFollower19; csc_end.|

`map/mapscriptengine_1.asm:csc14/csc15` install the inline/referenced action
pointer and seed ACTSCRIPTWAITTIMER with the resolved physical index. Waited
forms poll for the pointer equaling eas_Idle; asynchronous forms return.
csc14 then scans through `$8080`; this is not the map-script `$FFFF` terminator.
The two Higins inline programs are18 bytes each; the Cloud speed program12.

csc2D clears FLAGS_A bits5/6, generates a relative-move/destination-wait pair
of8 bytes for each packed move and a6-byte jump-to-Idle tail. Five one-move
programs generate70 bytes, distinct from their20 packed source bytes.
`entity/entityscriptengine_2.asm:esc04` computes signed tile deltas*384 from
actual position; esc01 waits for actual destination. Peter's nominal waited
path is `(17,14)->(17,16)`, then `(17,15)`. Cloud's is `(18,13)->(18,10)`.
Higins's Up1 begins at the actual position, nominally `(18,14)->(18,13)`;
Down2 starts after a jump, so its endpoint is not guaranteed `(18,15)`.

`data/scripting/entity/eas_main.asm:eas_EvilGizmoCircle` changes speed and
acceleration itself: factors2/1, speed26/0, acceleration ON/ON, deceleration
ON/OFF, relative(-2,+2), wait destination; then factors1/2, speed0/26,
acceleration ON/OFF, deceleration ON/ON, relative(+2,+2), wait; then the
first configuration with relative(+2,-2), wait. Its branch goes to
byte_45086 **before** the entry label, adding the second configuration with
relative(-2,-2), wait, then repeats. It is not an Idle-terminating action.
csWait200 does not join it or prove a completed circuit. The later Die
installation replaces it at whatever progress was reached; speed64 before
the circle does not remain its circle speed.

`data/scripting/entity/eas_actions.asm:eas_Die` disables animation, cycles
Left/Up/Right/Down three times with eleven waits3 and explicit sprite updates,
requests SFX_BATTLEFIELD_DEATH, changes sprite to EFFECT1, faces Up/Left/Down
with three waits8, sets position/destination63,63, clears travel and jumps Idle.
It does not kill a combatant, remove an actor, clear status or recruit anyone.
63*384=`$5E80`, below VInt_UpdateEntities' inactive-X threshold `$7000`, so
the staged/finished cloud is still live entity storage. Slide-out/in also
does not delete it: csc22 modes4/5 run23 crop/transform/DMA iterations with
two VInt waits each; out adds the final masked DMA. Other sprite updates
can interact with those requests; hidden does not mean stopped.

The Higins inline `ac_motion OFF` writes animation counter-1 via esc14;
orientation controls sprite orientation, not facing, movement or possession.
csc18 flash B60 shifts60 right2 then DBFs16 times: set/clear FLAGS_B bit2,
two sprite updates and four explicit VInt waits each (64 total). Screen
flash30 shifts right3 then runs four LaunchFading calls, followed by
DuplicatePalettes;30 is not a measured thirty-frame effect. Camera requests
wait for scrolling. csc27 headshake sets animation counter-1, performs seven
paired sprite-transform cycles with six explicit VInt waits each, then sets0.
csc26 nod similarly brackets one sprite transform with Sleep10/20/10 and
ends counter0. Neither tests/cures an illness or changes HP/status.

eas_Jump disables auto-facing, requests relative(0,-1), waits5, relative(0,+1),
waits5, relative(0,0), enables auto-facing and jumps Idle. Timer waits are
not destination barriers: a waited Jump proves its program reached Idle,
not exact displacement or a full movement stop. RotateRight disables animation
and auto-facing, cycles Left/Up/Right/Down with updates/waits5 and branches
back forever. csWait40 does not prove two rotations; the next Jump replaces
it. Final Init resets acceleration factors/speeds32, disables acceleration/
deceleration, enables obstruction and auto-facing, clears uncollidable and
immersed settings, orients Up, sets size24, updates and reaches Idle. Init
does not reset HP/status or the animation counter. The explicit startEntity
then writes counter0 only if ally19 is alive. Subsequent follower installation
again replaces the action pointer without proving settled motion.

## Dialogue, recruitment and following

**Confirmed:** all21 literals at actual `data/scripting/text/gamescript.txt`
keys `$0A4D..$0A61` match this source program, not just its comments. P is
Peter7, H is Higins19, B is FOLLOWER_B31; S/T mean nextSingleText/nextText.
H uses portrait-control$C0; P/B use$00. The literal controls remain independent
of S/T:2639 is S with W2. Normal uninterrupted cursor flow is2637 through2657,
ending2658; the fixed recruitment message446 does not consume that cursor.

| ID / command | Speaker / form | Actual literal |
| --- | --- | --- |
|2637 /15|P/S|`Is he alright?{W1}`|
|2638 /16|B/S|`We have to save him before{N}he is possessed by an evil...{W1}`|
|2639 /33|H/S|`What happened?{W2}`|
|2640 /36|H/S|`Ah...I was almost possessed{N}by that devil.{W1}`|
|2641 /38|H/S|`Thank you.{N}How can I repay you for{N}your kindness?{W1}`|
|2642 /39|B/T|`Are you OK now?  Good.{W2}`|
|2643 /40|B/S|`May I ask you, where were{N}you going in such a wounded{N}state?{W1}`|
|2644 /41|H/T|`Pacalon, my country.  We{N}are fighting against the{N}devils.{W2}`|
|2645 /42|H/T|`But, no country in North{N}Parmecia has enough power{N}to resist them.{W2}`|
|2646 /43|H/S|`So, I was heading south to{N}find some allies.{W1}`|
|2647 /46|P/S|`You don't have to go any{N}further.{W1}`|
|2648 /48|H/S|`Then, you're from South{N}Parmecia?{W1}`|
|2649 /49|B/S|`Yes, Granseal.{W1}`|
|2650 /51|H/S|`Oh, how lucky!  Please{N}come with me to Pacalon!{W1}`|
|2651 /53|P/S|`But, we have to go to{N}Tristan.{W1}`|
|2652 /55|H/T|`Oh, please...I can't return{N}to Pacalon without allies.{W2}`|
|2653 /56|H/T|`Maybe, when you're done...{W2}`|
|2654 /57|H/S|`OK, I'll go with you.{W1}`|
|2655 /59|B/S|`But, you're injured{N}Mr. {NAME;19}.{W1}`|
|2656 /61|H/S|`No problem.  I don't know{N}why, but I'm much better.{W1}`|
|2657 /69|H/S|`Let's go to Pacalon.{W1}`|

`text/textfunctions_1.asm:symbol_player` resolves `{NAME;19}` through ally19's
actual name, not scene physical19 or an immutable English name. Portrait/speech
lookup resolves the current mapped sprite: Peter base/promoted selects the
matching portrait and bleep4/5, HIGINS_PROMO selects Higins/bleep6, B can be
Astral/bleep6, Oddler/bleep4 or another alias. Class/sprite/mapping changes
therefore matter. csc1D opens only if no portrait window is already open;
bits15/14 of the packed speaker word supply its controls.

csc00/csc02 normal paths prepare portrait/speech, wait for view scroll,
DisplayText and increment the cursor. S additionally closes portrait/text
and sleeps10; T retains windows. ExecuteMapScript clears its skip latch at
entry; debug mode plus P2 Start can set it. Skipped S/T consumes operands but
does not display, increment, close or sleep. Negative csWait words also skip
Sleep, while actions/effects/camera/load/joins still execute. A midscene skip
leaves a prefix cursor; `join` has no skip-latch guard and still requests its
music/fixed text/input wait. No choice/YesNoPrompt, Pacalon/Tristan warp or
conditional route is encoded by the conversation's requests.

At command58, `mapscriptengine_2.asm:csc08_joinForce` clears speech SFX, waits
for view scroll, reads word19, clears bit15 and requests MUSIC_JOIN (no sad
bit). It calls j_JoinForce, reads actual class, writes dialogue-name indices
19/class, displays fixed446 (`{NAME} the {CLASS} {N}has joined the force.`),
calls FadeOut_WaitForP1Input, closes text and sleeps10. The special operand128
Sarah/Chester path is not selected. These presentation services must return
to reach the later conversation/follower call; membership writes precede them.

`code/common/stats/battleparty.asm:JoinForce` sets F19, calls UpdateForce,
then, if its rebuilt active count is below12, calls JoinBattleParty to set
F51. Full party does not evict anyone. Already joined-active remains active;
joined reserve can become active if there is space. Supplied F51 can already
make a newly joined member active during the rebuild. JoinBattleParty only
sets its flag: there is no second immediate list rebuild in JoinForce, so
that newly activated list/count can lag until the next UpdateForce. No HP,
MP, class, level, item initialization, status removal, AI-bit clear or position
repair occurs in these joins. Dead Higins can join while still dead; scene
statements about feeling better do not change this. Repeating the clear526
scene with F19 already held still performs the calls and join presentation.

At command70, `mapscriptengine_1.asm:csc56_addFollower` resolves19 to its
current physical actor, scans EXPLORATION_ENTITIES through byte-1, and supplies
the last follower's physical ID (0 if empty), d2=`$FFE8` (-24), d3=0 to
`entity/entityfunctions_2.asm:AddFollower`. That consumer first installs a
new42-byte copy of eas_Follower1 in walking scratch, advances the scratch
pointer and patches target/offset words at$1E/$20/$22. It then appends the
actor to the follower list only if not already there. Duplicate membership
still replaces the script/uses scratch; a last follower adding itself can
produce a self-target. No capacity, HP, joined/active or safe-mapping guard
precedes this. No persistent Higins follower flag/table row is added.

The resulting esc03_follow uses the supplied physical target and signed
offsets (shifted by4 for internal coordinate units); its distance/layout and
target-facing logic supplies destinations/velocity. This is an installed
following relation, not immediate teleportation or proof of a final follower
position. No join/follower/scene command writes MAP_EVENT_PARAM2–5, so neither
scene Bowie19,16 nor Higins's motion replaces the pre-scene return seed.

## Completion, defeat, escape and the return boundary

**Confirmed:** after csc_end's possible scroll wait/speed clear and register
restore, the after wrapper consumes table byte0 and calls JoinForce(Bowie0),
even on held526's skipped-scene path. That sets F0, rebuilds lists (including
the reached Higins F19/F51 writes) and conditionally sets F32 if active count
is below12. The wrapper is not a second Higins join or a repair of the seed.
Only after it returns does BattleLoop clear426 and set526. Intro476 remains
as supplied from admission; there is no new427 or destination-town write.

BattleLoop clears d0–d3 **words**, copies PARAM2–5 bytes into them and returns
d4=1. Thus nominal victory returns Map72 and member-derived world X/Y/facing,
not camera13,11, main19,16 or a Pacalon entrance. Earlier upper register halves
are not established by clr.w. Changed pending parameters or interrupted scene,
join, service or flag writes preserve their reached prefix, with no rollback
and no completed return claim. Completion526 is not an advance reservation
of a fully rendered rescue.

| Terminal route | State/cost order and returned disposition |
| --- | --- |
|Defeat|UpdateBattleUnlockedFlag; clear dialogue name1; sad music/text363/close; Bowie HP=max; unsigned gold shift-right1; GetEgressPositionForBattle; d4=-1. No Battle04 special tail.|
|Admitted Egress|Selected spell cost deducted from actor MP; close battlefield windows, text274; common sound/flash/close; unlock update; unwind action frame; egress helper; d4.w=0.|
|Admitted Angel Wing|Remove selected actor item slot and recompute stats before text275; same common escape tail and d4.w=0. No defeat gold penalty or Bowie restoration.|

`battlefunctions/battlefunctions_1.asm:UpdateBattleUnlockedFlag` preserves426
if526 is clear; it clears426 if526 is held. Neither defeat nor escape newly
sets526, runs after/join, or changes476. Prior admission may already have
cleared a completed battle's unlock. Neither path reverts previously reached
item/drop/party writes. Later exploration healing is outside this endpoint,
so neither deducted MP nor defeat-restored HP is an assertion about eventual
exploration values.

`battleloop/getegresspositionforbattle.asm` has no26 special: it passes
EGRESS_MAP, not the victory seed or stored egress XYZ/facing, to
`code/common/maps/egressinit.asm:GetSavepointForMap`. BattleLoop sets399 on
fresh/resumed entry. With399 held, selected Ketto25 yields `(12,11)`, Up;
New Granseal6 yields `(11,22)`, Up; an unmatched72 retains72 and defaults to
`(1,1)`, Up. Other supplied selectors use their row/fallback. This does not
establish a prior church visit. The helper's table writes are byte-sized;
do not impose victory's clr.w contract on all escape registers.

With399/F64 held it also applies raft reset: Polca/Bedoe selects72 `(43,48)`,
Hassan selects69 `(28,55)`, otherwise69 `(42,15)`, including6/25/72. Clear64
preserves raft fields. Direct399-clear entry returns initial Map3 `(56,3)`,
Down before that reset; it is not the ordinary reached Battle26 premise.
These are register/raft writes, not boarding or exploration admission.

All BattleLoop returns reach `code/gameflow/mainloop.asm:alt_MainLoopEntry`,
whose next calls are SwitchMap then ExplorationLoop, without a d4 sign test
or intervening CheckBattle. This document stops at that returned handoff:
no subsequent map setup, movement, control availability, Pacalon/Tristan
arrival, battle admission or save is established.

## Changed-state acceptance and reproduction

| Changed premise or interruption | Required distinction |
| --- | --- |
|128 dead versus merely zero counted enemies|Leader append needs Bowie HP and128 HP predicates; ordinary counts can terminate independently.|
|Bowie dead/zero counted allies together with zero enemies|Defeat precedence; no rescue.|
|Leader dies during post-status pass|Death/count pass has no second defeated-hook invocation.|
|Nonzero HP on hidden/unused/neutral slots; stale/duplicate death list|Append scans HP only; counting scans X/HP; processing does not zero HP or deduplicate.|
|130 removed by leader cleanup, no item, wrong actor/target or targetDies0|No inferred ring transfer; actual action caller/table conditions required.|
|Drop bit10 held; actor dead/full; deals15; interruption after bset|Repeat suppression, fallback/saturation and consumed prefix remain separate from reward rendering.|
|526 clear/held, F19 already set, party full or supplied F51|Scene admission differs from membership; join may only reserve, lists can lag and repeat presentation can occur.|
|Dead19, living unjoined19, class/status/item changes|Victory's eligible healing precedes scene; join adds flags without initializing or reviving stats.|
|Follower flags/aliases, empty/duplicate list, missing/hidden mapping, capacity|Actual physical allocation/follow target controls; no automatic alias repair or persistent Higins follower row.|
|Skip text or replace looping actions before their next update|Cursor/wait and animation prefixes differ; no exact circles, rotations or settled endpoint implied.|
|Different/empty active list, crop/map/facing or parameter mutation|Returned seed remains actual pre-scene data; scene placement is no replacement.|
|F88 continuation, defeat/Egress/Wing, different EGRESS_MAP/F64|Use supplied state and path-specific costs/return/raft rules, not fresh-victory assumptions.|
|Any interrupted scene/join/follower/completion service|Only reached writes; neither completion526 nor a rendered/usable return is assumed.|

Load ignored `local/private-inputs.ps1` in the same process as verification;
use the clean pinned source read-only. The
[retained-pair recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
owns map-init equality, `_verify_map_script_engine_fixture` and actual
`_cutscene_facts`. Reuse the accepted pair receipt when its inputs/dependencies
remain unchanged; do not regenerate extraction.

1. Compare exact slot26 end/leader/after/join/position records and include
   targets to their wrappers; use actual `_build_lifecycle_facts`,
   `_build_control_facts` and `_cutscene_facts` against the existing
   `tests/fixtures/h2/{battle-loop,battle-control,battle-cutscenes}-static-v1.json`.
   Follow death/count/healing/seed/flags/egress/MainLoop instructions, including
   the second status pass and the distinct word/byte return writes.
2. Join drop row10 through sf2macros/enums, action caller, complete selected
   drop branch, inventory/removal/deals consumers, ring definition and dropped
   bitfield. Reuse existing enemy-drop fixtures at their scoped boundaries;
   no natural26 or fresh runtime claim follows from their aggregate counts.
3. Select retained program abcs_battle26; compare all72 sourceLine/macro/
   argument/width records, three inline programs and five packed move lists
   with complete source and macros. Join ce_4BE4A's whole list to follower/
   declaration/HP/class/lookup/priority consumers. Follow all selected wait,
   movement, circle/rotation/replacement, flash/slide/die/jump/nod/headshake,
   text/name/skip, join/follower and termination consumers. Compare every
   literal from2637 to2657 by actual four-digit hex key, speaker/form/control,
   and fixed446 separately; inspect source rather than infer from counts.
4. Review full prose and changed states, relative links/anchors, UTF-8/LF,
   sole-path/private boundary and `git diff --check`. On the clean committed
   candidate run `uv run sf2 verify plan --base origin/main --head HEAD` and
   record actual exact-head CI in the Issue handoff. Documentation-only scope
   skips blanket local normal/full/engine/Godot/H3/H4 suites.

**Unknown:** natural terminal reach and supplied combatants/lists/party/HP,
items/drop flags/deals, map/crop/return parameters, followers/mappings/capacity,
timing and action progress; completed text/music/fade/DMA/VInt/input services,
rendered rescue, usable exploration and save persistence. No new runtime
question is automatically admitted by this static result.

Preserve [#1009 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1009#issuecomment-6182172704)
and [PR1012 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1012#issuecomment-6182188584):
headd1853cc9/tree9e6dcb38/base63142601, final CI38664933850(scope15s/research32s),
original CI38664816111, selected joins0.259s/main-gate0.256s and reused pair0.083s.
Retain neutral syntax/coordinate, spawn full-word return and source-endpoint
integer-projection corrections/diagnostics, all linked #1005/#1001/#997/#993/#989
results, failures and Unknowns, and the retained configuration/source/payloads.
Completed suites are not reset. Freeze the clean pushed Draft PR for independent
review; no merge, cleanup, archive or next Issue.
