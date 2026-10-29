# Battle32 outcomes, Jaro recruitment and Moun entry

Continue with [Battle33 initialization and Moun opening](./battle33-initialization-and-opening.md).

Evidence date: 2026-10-20. **Confirmed (static):** unchanged victory saves
a Map74 return seed before the complete Jaro scene, clears432 and sets532.
The scene joins23, clears his AI-control bit, then clears active55 and
writes combatant X=-1; its final following is physical, not active-party
membership. The subsequent Map74 return reconstructs exploration actors.
Both Moun warps independently request31; held433 admits Battle33, even if
533 was already held. **Unknown:** natural outcome/travel, incoming state,
completed services, presentation, usable control and persistence. No
Battle32/33 runtime or remake-playability claim follows.

## Agreement and evidence route

Standard rigor for [Issue #1086](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1086)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect terminal processing, complete recruitment, distinct state authorities,
returned exploration and conditional Moun admission for later design.
Accepted base is `ab39ed171d81221e8721554acd98dd3f530633b3`, including
[Battle32 activation/Jaro conversion](./battle32-activation-jaro-and-first-turn-dispatch.md)
and [initialization/opening](./battle32-initialization-and-opening.md).
Exactly two owned paths: this owner and one navigation-only predecessor link.
No shared index or stacked dependency; UI692 is independent. No implementation,
fixture/schema/tool, extraction, asset decoding, translation or design-guide work.

Begin after a returning individual turn, without surveying actions/AI/combat.
Stop returned Map74/Pacalon exploration at its first input/event handoff.
Separately connect the two Moun warps to BattleLoop33 **before initialization**,
or the explicit Map31 exploration setup-selection handoff **before town init**.
Full Moun population/interactions, underground/revisit, Battle33 body and other
Map74 exits remain deferred. This boundary does not establish a walkable route.

Reuse [Battle31 outcomes](../northern-travel-mitula/battle31-outcomes-and-battle32-admission.md),
[Battle30 outcome/scene consumers](../northern-travel-mitula/battle30-outcomes-and-mitula-scene.md),
[Pacalon entry](../northern-travel-mitula/battle26-return-through-pacalon-entry.md),
[Frayja offer](../northern-travel-mitula/pacalon-return-frayja-and-battle31-admission.md),
[battle loop](../../battle/battle-loop.md), [cutscenes](../../battle/battle-cutscenes.md),
[script data](../../battle/battle-cutscene-data.md), [common scripting](../../world/common-scripting.md),
[common maps](../../world/common-maps.md), [request state](../../world/map-event-request-state.md),
[request consumption](../../world/map-event-request-consumption.md),
[lifecycle](../../../design/contracts/battle/battle-control-lifecycle.md) and
[routing](../../../design/contracts/battle/battle-cutscene-routing.md).

Original paths below are relative to `disasm/` at clean, read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Existing USA retail ROM identity is SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no ROM/H1/runtime equality is newly claimed. `sf2const.asm`/`sf2enums.asm`
own constants/RAM; Fnnn denotes flag nnn; ordinals are zero-based, extents
half-open. Short `battleloop/`/`cutscenes/` mean children of
`code/gameflow/battle/`; `map/`/`entity/` mean `code/common/scripting/`.

Minimum evidence is complete selected source/table/fixture/consumer joins,
all scene commands/inline actions/list/texts and bounded changed-state review.
Reuse the unchanged18,285,979-byte init/script pair and accepted0.083s
provenance in `local/issue1005/pair.txt`, without regeneration. Linear bounded
reads/arithmetic budget below one minute/512MiB per direct run; small ignored
receipts live in `local/issue1086/`. No acquisition, runtime/emulator/Godot,
screenshots, seed hunt or comparator work. Reassess contradictory evidence or
material supporting machinery before expansion. ADR0004's complete checklist
applies proportionally under the current documentation-only verification policy.

## Conditional terminal processing and selected tables

**Confirmed:** `battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`) resumes after
ExecuteIndividualTurn, optionally applies debug kill-all, then calls defeated
wrapper, ProcessKilledCombatants and CountRemainingCombatants in order.
Ally count0 loses before enemy count0 wins. If neither terminates, it reads
the actual turn-buffer byte, applies ProcessAfterTurnEffects, processes deaths
and repeats the same ordered counts, without another defeated hook. Only a
nonterminal suffix advances the byte offset by2. Escape unwinds before this
continuation; lethal HP, death processing and outcome are distinct boundaries.

| Complete selected table / extent | Battle32 selection |
| --- | --- |
|`data/battles/cutscenes/battleendcutscenes.asm`,48 relative words, `$47BE8..$47C48`|slot32/byte64 at `$47C28`: ms_Empty.|
|`data/battles/global/enemyleaderpresence.asm`,45 bytes, `$47C8E..$47CBB`|byte32 at `$47CAE`: -1, nonzero cleanup.|
|`data/battles/cutscenes/afterbattlecutscenes.asm`,48 relative words, `$47CF4..$47D54`|slot32/byte64 at `$47D34`: abcs_battle32, `$4CDB4`.|
|`data/battles/cutscenes/afterbattlejoins.asm`,52 bytes, `$47D6A..$47D9E`|byte32 at `$47D8A`:0, actual JoinForce operand.|
|`data/battles/global/afterbattlepositions.asm`|Only battle5; no32 position record.|
|`data/battles/global/enemyitemdrops.asm`,30 four-byte rows plus terminator, `$BE52..$BECC`|No32 row. This does not establish empty enemy inventory or exclude other action-side rewards.|

`battleendcutscenesstart.asm` requires Bowie0 HP.w!=0 and enemy128 HP.w=0.
Clear actual500+CURRENT_BATTLE dispatches ms_Empty; held532 skips it for32.
Both reach `battleendcutscenesend.asm:loc_47C48`, reread the battle, select
leader presence and, for unchanged32's -1, scan all32 enemies128–159.
Every nonzero-HP slot is appended to the **existing** death list; length
increments and the next sentinel is replaced. No initial clear, X, Y,
membership, neutral or activation predicate. Sixteen declared rows do not
shorten this sweep; already-converted143 HP0 is not newly appended there.

ms_Empty is one FFFF word. Executing it still changes interpreter scratch,
clears text-skip on entry, conditionally waits for view if a dialog remains
and clears scrolling speed on return. Skipping dispatch avoids those effects,
but neither wrapper tail. Empty is not a completed-service receipt.

ProcessKilledCombatants requests spin/effect/sound waits, increments ally
defeats or credits enemy kills to supplied BATTLESCENE_FIRST_ALLY, sets X/Y=-1,
clears status/recomputes stats and moves the mapped entity to `$7000,$7000`.
It does not set HP0, clear the list length or execute item-drop award logic.
Living queued enemies can disappear from the count through X alone. Stale/
duplicate list entries or mappings and incomplete cleanup remain supplied state.

CountRemainingCombatants requires signed X.b>=0 and HP.w!=0 over30 allies
and32 enemies, without Y/membership/neutral/activation tests. High-bit nonzero
HP passes. Bowie HP0 forces ally count0 even when others survive; simultaneous
exhaustion loses. Off-map enemies can yield victory without the leader hook;
a second-pass poison kill of128 does not invoke that hook again there.
F88 restores supplied state rather than guaranteeing fresh lists/roster.
Live CURRENT_BATTLE reads can select different guards, scripts, cleanup and
outcome flags; they are not a single snapshot.

## Victory seed, after loading and completion tail

**Confirmed:** `battleloop_2.asm:BattleLoop_Victory` first heals all30 slots
without membership filtering: Peter7/Lemon28 even at HP0; other nonzero-HP
allies to max HP/MP. Other dead allies remain dead. It retains existing
STUN/POISON/CURSE, clears other status bits and refreshes stats. Battle44's
timer branch is not32.

Before the after wrapper, it saves CURRENT_MAP.b to PARAM2; calls
UpdateForceAndGetFirstBattlePartyMemberIndex; reads that member's actual
combatant X/Y, byte-adds crop X/Y to PARAM3/4; reads mapped physical facing
to PARAM5; writes PARAM1=0. It does not post an event type. UpdateForce
rebuilds joined/active/reserve prefixes and word counts in ID0–29 order,
without clearing tails or writing membership flags. The first active byte
is read without a count/HP test: not necessarily Bowie, alive or placed.
Empty lists expose a retained byte. With intact Map74/crop `(2,16,32,48)`,
the seed is74, `(X+2)&255,(Y+16)&255`, actual facing, not scene `(10,25)`.

GetEntityIndexForCombatant returns its lookup byte unchecked. Mapping0 alone
aliases physical0. Unplaced ally FF is shifted by5 and facing read from
`ENTITY_DATA+$1FE0+$10`; victory applies neither LoadBattle's negative fallback
nor3F mask. This is not a valid-entity guarantee. Scene reconstruction later
replaces mappings but does not repair this earlier seed.

`afterbattlecutscenesstart.asm` checks actual500+CURRENT_BATTLE. Clear532
executes abcs_battle32; held532 skips the **whole scene**, including join23,
AI clear, LeaveBattleParty and following. Both reach EndAfterBattleCutscene,
reread the battle and JoinForce the selected table byte0. That sets F0,
rebuilds lists, then sets F32 only if active count<12; at capacity it leaves
an existing F32 intact. A newly set F32 is absent from its preceding list
snapshot. This tail does not join Jaro when his scene was skipped.

After wrapper return, BattleLoop rereads CURRENT_BATTLE, clears400+index,
then sets500+index: unchanged32 clears432 then sets532. It clears d0–d3
**words**, copies PARAM2–5 bytes and moveq1 to d4 before RTS. Upper register
halves are not established by clr.w; PARAM1=0 differs from returned d4=1.
Intro482, region386 and unlock433 are not written by this tail.

Scene command1 calls csc37_loadMapAndFadeIn, which sets black-fade state
then falls into csc48_loadMap. It loads tilesets for explicit74, waits for
fade, disables services, packs camera5,20 as `$0500|20`, multiplies by3,
calls LoadMap and reenables services/display. LoadMap writes CURRENT_MAP74
and loads blocks/layout. CURRENT_BATTLE stays32: LoadMapBlocksAndLayout
copies the existing crop `(2,16,32,48)` and LoadMapArea takes its battle
branch, with main-layer local origin0,0 and ends `(31*384,47*384)`.
Map74's sole area is0,0..63,63, so camera5,20 selects it before that crop
override. The camera request is not actor placement or an outgoing seed.
Changed current battle/crop can select another loading branch/extent.

Neither this load, the remaining scene nor selected native helper writes
PARAM2–5 or pending type. They do not call MainLoop, CheckBattle, town init
or reset CURRENT_BATTLE. Scene/camera/actor coordinates and the saved request
remain separate authorities; interrupted load can leave only reached changes.

## Complete after scene, actions and text

**Confirmed:** `data/battles/entries/battle32/cs_afterbattle.asm` spans
`$4CDB4..$4CF0C`. abcs_battle32 has52 top-level commands,280 encoded bytes,
including csc_end. Six inline movement streams add24 bytes, ending at
csub_4CEE4 `$4CEE4`; its8 bytes precede ce_4CEEC. The32-byte entity list
ends at `$4CF0C`: six-byte main header, three eight-byte rows, two-byte end.
The retained script corpus supplies top-level records; pinned source supplies
all inline/native/list details, not an inference from the command count.

| Index | Complete operation / boundary |
| --- | --- |
|0|textCursor2853.|
|1|loadMapFadeIn Map74, camera5,20; battle crop remains live.|
|2|loadMapEntities ce_4CEEC.|
|3–5|setActscriptWait Bowie/eas_Init, Peter/eas_Init; setPos Peter11,25/Up.|
|6–7|setActscriptWait FOLLOWER_B/eas_Init; setPos31 at9,25/Up.|
|8–9|setActscriptWait Frayja/eas_Init; setPos22 at11,26/Up.|
|10–11|setActscriptWait FOLLOWER_A/eas_Init; setPos30 at10,26/Up.|
|12–14|setActscriptWait Jaro/eas_Init; setPos23 at10,22/Down; fadeInB.|
|15–17|Peter actionsWait Up1/endActions; Peter text2853,31 text2854.|
|18–20|Jaro actionsWait Down1/endActions; continued2855 then single2856.|
|21–24|Peter actionsWait Up1/endActions; face Peter Left, Jaro Right; Peter2857.|
|25–28|Jaro2858; face Jaro Down; Jaro2859;31 text2860.|
|29–31|setActscriptWait Peter/eas_DeactivateAutoFacing; Peter actionsWait Right1/endActions; Peter2861.|
|32–36|31 actionsWait Up1/endActions; face31 Right;31 text2862; face31 Down;31 text2863.|
|37–39|nod Bowie; join23; joinForceAI23,0.|
|40–44|Jaro actionsWait Down1/endActions; Jaro2864; two separately awaited eas_Jump installations on23; Jaro2865.|
|45|executeSubroutine csub_4CEE4: moveq23,d0; jmp j_LeaveBattleParty.|
|46|followEntity30→Bowie0, offset2 =(-24,0).|
|47|followEntity Peter7→30, offset1 =(0,-24).|
|48|followEntity31→30, offset3 =(0,24).|
|49|followEntity Frayja22→30, offset2 =(-24,0).|
|50|followEntity Jaro23→Frayja22, offset2 =(-24,0).|
|51|csc_end: conditional view wait, scrolling-speed clear, interpreter return.|

All six inline streams contain exactly one move command and endActions;
the latter emits8080. Their directions in order are Up, Down, Up, Right,
Up, Down, all distance1. csc2D compiles each into an eight-byte move action
and six-byte jump to eas_Idle in action scratch, consumes the end marker,
installs it, clears flagsA bits5/6 with9F and waits for that actor's Idle.
csc15 setActscriptWait installs the named long pointer
with the physical slot as read timer and waits for Idle. Neither handler
uses the ally-HP skip helper. eas_Init resets acceleration/speed32/32,
collision/auto-facing/orientation/size24/immersed fields and refreshes sprite,
not combatant HP/stats/membership. DeactivateAutoFacing clears that flag
then jumps Idle. eas_Jump disables auto-facing, requests relative0,-1,
wait5,0,1,wait5,0,0, reenables auto-facing and jumps Idle; both installations
are joined separately. Source waits are not measured durations/completion.

csc19 setPos and csc23 setFacing use their own byte-ID HP predicate for
allies0–29: HP.w=0 skips; high-bit nonzero proceeds.30/31 bypass it.
nod's csc26 consumer has no HP test: stop animation, Sleep10, load/crop/DMA
one nod cycle, Sleep20, refresh sprite, Sleep10 and restart animation.
It does not select JoinForce or a choice branch.
With distinct intact aliases, applicable HP predicates passing and movement
services returning, the commanded pre-follow locations are Bowie10,25,
Peter12,23,31 at9,24,30 at10,26,Frayja11,26,Jaro10,24; jumps are relative
motions, not a new saved return. Later following can move these actors.
Missing aliases, changed HP or interrupted waits invalidate that projection.

Normal text progression is2853–2865 (`0B25..0B31`) plus join handler's446
(`01BE`), which does **not** advance this scene cursor. Complete keyed join:

| Key / command | Speaker / mode / modifier | Paraphrase / complete token sequence |
| --- | --- | --- |
|2853 /16|Peter7 /single /00|Surprised at danger; N,N,W1.|
|2854 /17|31 /single /00|Asks why he changed sides; N,W1.|
|2855 /19|Jaro23 /continued /C0|Names himself/from Galam, waited to escape; NAME23,N,N,W2.|
|2856 /20|23 /single /C0|Rejects their wicked ways; N,W1.|
|2857 /24|7 /single /00|Shares that opinion; N,W1.|
|2858 /25|23 /single /C0|Requests joining, offers help; N,N,W2.|
|2859 /27|23 /single /C0|Professional soldier; N,W1.|
|2860 /28|31 /single /00|Welcomes him; W1.|
|2861 /31|7 /single /00|Questions recent enemy allegiance; N,W1.|
|2862 /34|31 /single /00|Dismisses objection, names Peter; NAME7,N,N,W1.|
|2863 /36|31 /single /00|Asks LEADER to admit NAME23; LEADER,N,NAME23,N,W1.|
|446 /join38|handler text, name/class variables23/current class|Join announcement; NAME,CLASS,N; no encoded W1/W2.|
|2864 /41|23 /single /C0|Thanks; W2 despite single.|
|2865 /44|23 /single /C0|Promises effort; W1.|

csc00/csc02 and csc1D consume packed modifier/speaker, use actual physical
mapsprite for portrait/bleep, wait for view, DisplayText and increment cursor.
Single closes portrait/text and Sleep10; continued retains windows. C0 sets
right-side/mirror bits15/14, not a character or text variable; an already
open portrait returns early. Expected Jaro mapsprite52 maps portrait JARO/
bleep5;31's appearance is determined by the actual follower alias, and Peter
by class. NAME23/NAME7 resolve named combatants; LEADER instead invokes
symbol_leader → UpdateForceAndGetFirstBattlePartyMemberIndex → UpdateForce
before GetCombatantName. Thus reaching2863's token rebuilds cached lists
before join38; its name need not be Bowie. A skipped/not-yet-parsed token
has not done that rebuild. N/W1/W2 are text consumers, not movement or joins.

ExecuteMapScript clears text-skip at entry; debug P2 Start can latch it.
Skipped nextText/nextSingleText consumes its operand without display/cursor
increment. The normal key sequence is therefore conditional; joins/actions/
AI/native/following are not all skipped. Join38 calls its own txt446 and
wait/input path; it is not the csc00/csc02 skip branch. No yes/no branch,
inventory/key award, setStoryFlag, warp or extra population occurs here.
**Inferred:** the conversation presents a recruitment ceremony accepting
Jaro's defection; physical completion and player assent are not proven.

## Physical population, aliases and logical Jaro state

**Confirmed:** ce_4CEEC declares main10,25/Up and fixed Peter11,25/Up,
Frayja11,26/Up,Jaro10,22/Down, all ally sprite operands/eas_Init. csc42
disables services/display, reads the main header, InitializeMapEntities,
loads mapsprites and restores services/display. ClearEntities resets49
physical slots,64 lookup bytes and walking scratch; old battle143→23
physical alias does not survive this reconstruction. Stats, joined/active
flags and activation words are not cleared by it.

Ordinary followers allocate first independently of membership. Map74 is
overworld: held65 prepends Caravan30; F67 can declare Peter7, F82 Frayja22.
Fixed ally rows with a nonzero existing lookup skip allocation; otherwise
they allocate next in Peter/Frayja/Jaro order. No dedicated Jaro follower
row exists. Let n be successful follower allocations and p/f be1 when the
fixed Peter/Frayja rows allocate: new Jaro is physical `n+p+f+1`.
Main remains0. Capacity/duplicate-alias/missing-script-pointer safety for
arbitrary flags is not established; original follower action-pointer tables
are finite, not one independent pointer per possible enabled declaration.

FOLLOWER_A30 can be Caravan (65), king (74), old man (77), later king (83),
with last lookup write winning. FOLLOWER_B31 can be Astral (74), Oddler (68),
later Astral (70), in that order. Supplied earlier declarations still consume
physical slots when overwritten. Missing30/31 lookup0 aliases main0; logical31
is distinct from physical raft31/lookup63 under64/RAFT_MAP/mode. No local
script sets these follower flags. F82 came from the accepted Frayja callback,
not joined22/active54. During scene32, GetAllyMapsprite does not select the
outside-battle dead-ally blue flame branch; it uses actual class appearance.

The mid-battle predecessor is conditional: successful conversion kills/hides
enemy143 logically, copies his old X/Y to ally23, sets joined23 and forcibly
active55 even at capacity, copies the physical lookup, then FFFF joinForceAI
sets activation bit2 (`$0004`) and rebuilds lists through JoinForce. It does
not copy HP/stats or clear every old143 field. A retained/skipped143 and a
scene never admitted/interrupted at386 are distinct states, not assumed
success here. Outcome can be reached without that region conversion.

After command38, actual csc08_joinForce clears speech SFX, waits for view,
reads word23, clears sad-music bit15 (clear here), requests MUSIC_JOIN and
calls j_JoinForce.23 is not special operand128/Sarah+Chester. JoinForce sets
F23, rebuilds lists, then sets F55 only if resulting active count<12; it
does not clear a preheld55 at capacity. No capacity or already-joined skip
of ceremony: it reads current class, sets name/class dialogue variables,
requests446, calls FadeOut_WaitForP1Input, closes text and Sleep10. Reaching
later commands requires each service to return; source ordering proves no
completed music/input/presentation. JoinForce does not heal/initialize23.

Command39 csc54 reads23's activation word and operand0. Actual zero branch
ANDs `$FFFB`, clearing **only AI-control bit2**, and SetActivationBitfield;
it does **not** call JoinForce. Nonzero would OR4 and JoinForce. This resolves
the misleading macro comment from the executable branch. Dead23 still joins
and clears AI; his setPos/facing may skip while action/text handlers proceed.

Command45's native tail resolves s02 `j_LeaveBattleParty` to battleparty.asm:
preserve d1, form active32+23, ClearFlag55, d1=-1, SetCombatantX23, restore d1,
RTS through the JSR frame. Only X.b becomesFF: Y/HP/MP/stats/activation,
joined23 and physical entity/lookup are untouched. It does not rebuild lists.
Cached active/reserve prefixes may therefore still include23 until the
after-wrapper's JoinForce0 rebuild, which sees55 clear. A subsequent new32
write for Bowie can itself lag that rebuild. Final unchanged flags are
joined23 held/active55 clear/AI bit2 clear; dead stays dead. Mid-battle
forced-active13 and capacity12 alternatives converge only after this reached
leave/rebuild suffix, not atomically at join38.

Each final follow macro emits **three words**. csc2C first peeks only the
first/high byte of the follower word:00 for30,7,31,22,23. The helper's
GetCurrentHp → GetCombatantWord → GetCombatantEntryAddress masks BYTE_MASK,
so every precheck tests **Bowie0 HP.w**, not that follower's HP. HP0 skips
six operand bytes and unwinds this command; nonzero, including high-bit,
permits full-word follower/leader mapping and offset-table lookup. Reads
are independent, so changing Bowie HP between commands changes the prefix.
Dead Peter/Frayja/Jaro alone does not suppress their follow command.

AddFollower installs a copied42-byte eas_Follower1 stream in walking scratch,
patches physical leader and signed offsets, advances scratch42, and appends
the physical follower to EXPLORATION_ENTITIES only if absent. It does not
join23, set55, repair HP or move the saved seed; duplicate physical aliases
can replace each other's scripts, including main0. Five admitted calls
consume210 scratch bytes even when list insertion deduplicates. The final
chain is30→0;7/31/22→30;23→22, with the exact offsets above. Original
unchecked lookup/slot/list/scratch capacity remains a supplied-state limit.

## Defeat, Egress, Angel Wing and returned exploration

**Confirmed:** defeat calls UpdateBattleUnlockedFlag, which clears432 only
if532 is already held. It clears dialogue name1, requests sad music/text363
and close, restores **Bowie0** HP=max, halves unsigned gold, selects egress
and returns d4=-1.363's LEADER token can rebuild party lists before recovery;
the displayed member need not be Bowie. Battle4's losable upgrade is not32.

Admitted Egress deducts selected spell MP cost before274/cast flash; Angel
Wing removes its selected inventory slot/refreshes stats before275 and the
common tail. That tail closes text, updates unlock, unwinds the action frame,
selects egress and clears d4.w. Both bypass post-turn terminal processing,
defeat HP/gold recovery and after/join. Neither newly sets532 or changes482,
386,433,23,55 or Jaro AI; reached earlier conversion/combat/cost writes stay.
Neither path implies the retreat services completed or the next attempt
has fresh143. No rollback of any earlier prefix is supplied.

`battleloop/getegresspositionforbattle.asm` special-cases32 to moveq Map2,
ignoring live EGRESS_MAP. With reached BattleLoop's F399 held,
GetSavepointForMap's complete23-row table selects Pacalon `(41,10)`, Up1.
This differs from31's Map36 castle `(40,5)`. F399-clear direct entry instead
returns initial3 `(56,3)` Down3 before raft reset. Held399/64 with Map2 uses
default raft Map69 `(42,15)` without clearing64; clear64 leaves raft state.
Changed CURRENT_BATTLE can select another special/live-EGRESS_MAP branch.

MainLoop's `alt_MainLoopEntry` calls SwitchMap then ExplorationLoop for
every battle return, with **no d4 sign test or intervening CheckBattle**.
Complete switch rows are3/F640→4,71/F530→78,58/F999→59: unchanged74 victory
and2 retreat/defeat are not substituted. Fresh explicit exploration clears
pending, subtracts20000 from STEP_COUNTER under signed BGE (**N==V**, else0),
heals living/immortal allies, fades, sets CURRENT_MAP and CURRENT_BATTLE255,
loads tilesets and reconstructs entities/mapsprites. It clears temporary
256–383, sets80, loads map/view, installs base VInts and calls selected init.

Map74 selects ms_map74 and RTS init `$509D6..$509D8`; fresh construction now
uses ordinary full-map area0..63, without battle crop and without retaining
the scene's temporary follow chain. Its follower table contains no Jaro23
row, so scene following alone does not declare him on the new map. Joined23
remains available to later party management, independently of exploration
actors. Pacalon2 reuses its F530-selected populations and RTS init; no
whole-town interaction/church/save survey is repeated here. At first
WaitForEvent, pending is checked before player A/C; zero pending installs
mode-selected control/view0, nonzero dispatches an event first. **Stop at
this input/event handoff**; entry alone proves neither control nor saving.

## Both Moun warps and the Battle33 or exploration handoff

**Confirmed:** successful Map74 return is separate from later movement to
an actually admitted walking warp caller. `data/maps/entries/map74/6-warp-events.asm`
contains eight eight-byte rows plus endWord. Only its first two are owned here:

| Row / address | Source exact X,Y | No-scroll request / destination / facing |
| --- | --- | --- |
|0 /`$C3682`|3,25|Map31 /1,12 /Right0, padding0.|
|1 /`$C368A`|4,25|Map31 /30,37 /Up1, padding0.|

Neither tests532,386,433,Frayja/Jaro membership or an inventory key.
WarpIfSetAtPoint's ordinary walking caller requires loaded layout kind1000;
candidate coordinates are sign-extended and divided by384. It tests the
FFFF terminator before source bytes; any negative source byte is wildcard,
but both selected rows are exact. Source-order first match wins; no match
preserves old pending. These records do not prove natural route/collision reach.

Producer writes event word1 **before** PARAM1..4 longword, then facing/padding
word and WARP_SFX. WaitForEvent reads pending before A/C; ProcessMapEvent
clears stored type before dispatch. `ProcessMapEventType1_Warp` reads PARAM1:
zero submits/clears WARP_SFX, idles main and unwinds to MainLoop. It clears
d0–d4 words, reads map31, samples actual main X/Y by signed word division,
then replaces each nonnegative destination **low byte**, preserving sampled
high bytes, and reads facing. Negative replacement bytes instead preserve
the whole sampled word. Intact small coordinates yield31 `(1,12)` Right0
or31 `(30,37)` Up1,d4=0. Trigger-time coordinates are not consumption-time
coordinates; partial/replaced payloads can change map/mode/position/facing.
Nonzero changed PARAM1 selects the scroll branch, not this MainLoop handoff.

MainLoop @Start calls SwitchMap **then CheckBattle**. There is no31 switch
row. CheckBattle scans45 seven-byte battle records; exactly row33 at
`$7B1D` matches map31: crop `(0,0,32,38)`, wildcard255/255 triggers. It
requires **unlock433**, not532 or joined Frayja/Jaro. Accepted Frayja source
`data/maps/entries/map36/mapsetups/scripts.asm:cs_5DA28` sets story31,33,34
(431/433/434); after return its callback sets811 then82. Battle32 victory
does not supply433. Dialogue about unlocking Moun is not an extra predicate.

| Consumption-time flags | Actual stopping handoff |
| --- | --- |
|433 held,533 clear|Write crop0,0,32,38; return d7=33; MainLoop move.w d7,d1 / j_BattleLoop. Stop before initialization.|
|433 held,533 held|Write the same crop, clear433, **still return33 now**. Completion does not reject this admission. Same stopping call.|
|433 clear, either533|No match, d7=-1, restore original map operand31; no new crop write. Enter explicit Map31 exploration branch.|

CheckBattle does not write CURRENT_BATTLE. Stale427/432 do not compete on
explicit31 because their rows match74 only. A changed map operand can select
those or another map's rows instead; a partial31 request is not guaranteed
to enter Moun. Each later flag/selector read remains live.

For the no-match alternative, stop at explicit ExplorationLoop's
`j_GetMapSetupEntities` → GetCurrentMapSetup selection, before town init,
population/interactions or an input claim. `data/maps/mapsetups.asm` selects
ms_map31 by default, overridden by held830 to ms_map31_flag830. Pointer tables
`$5D40E..$5D426` / `$5D426..$5D43E` each have six longs: entity/events/zone/
description/item/init; only descriptions are shared. This does not assume830
or survey Gyan/underground. GetCurrentMapSetup rechecks CURRENT_MAP/830 per
call, so entity-list and a later init selection need not agree after changes.
Battle33's before/init/activation/outcomes and full Moun remain deferred.

## Actual representations and consumer gaps

**Confirmed about accepted repository code:** production consumes prepared
Content; static evidence, expressible shapes and admitted runtime fidelity
are distinct. [Capability status](../../../../remake/docs/capability-status.md)
owns runtime claims. No selected package or running instance is produced here.

| Actual surface / consumer | Expressible mechanism and remaining boundary |
| --- | --- |
|Existing lifecycle/control/cutscene/global fixtures and retained script pair|Static shared seams plus52 selected top-level records; inline/native/list/texts are pinned-source joins. These do not observe recruitment/returns.|
|[PrivateBattleEncounterReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [ExplorationContentReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)|Pin admits1/2/3, rejects32/33. outcome-source also requires EnemyLeaderPresent=false and defeated-program only End/Return; selected32's leader sweep is not admitted by its empty hook. Missing selected encounter/map/party/outcome/script/material bindings.|
|[BattleOutcomeRules](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/BattleOutcomeRules.cs), [BattleActionCommitter](../../../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleActionCommitter.cs)|Placed living actors, loss precedence and defeated gate exist; raw30/32 slots, supplied death list, full cleanup/live-selector/interruption semantics are not established by those shapes.|
|[BattleOutcome](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs), [Sf2OutcomeReturnPolicy](../../../../remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs), [MapPartyMembership](../../../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)|Save return before after, completion skip, JoinForce-like list snapshot and ordered unlock/completion exist. Policy requires active.First()/bound positioned actor, guards nonzero status refresh and egress absent399/held64 or640. No32 Map2 override binding, general retreat or unchecked FF/empty-list parity claim.|
|[OriginalPrograms](../../../../src/sf2tool/remake_exploration_programs.py), [ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [EntityActionRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Actual recursive lowering:125 main instructions plus one called native instruction. Main102 is native-call joinForceAI; called csub_4CEE4[0] is native-call jmp. Every selected nested Init/DeactivateAutoFacing/Jump and six movement streams has no native-call. Given admitted content, valid earlier state/slots/materials and returning services,102 is the first reached native refusal;118's call is later. Content maps native to UnsupportedInstruction/UnsupportedEntityAction; runners refuse program-opcode/entity-action, not successful placeholders.|
|[SceneEntities](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [MapTransfer](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs), [ExplorationViewRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs)|Scene-map/population/actions/follow/text/join shapes exist. LoadScene preserves actors until separate scene-entities; BindScene requires settled black/closed windows/palette binding. Capacity guards and prepared follower list differ from unchecked original; Map74's Caravan prefix, original battle crop and changed class/dead aliases require bindings/assessment. FollowEntity runner installs a logical follower without original high-byte/Bowie-HP peek. No automatic equivalence for five follow commands.|
|[ExplorationTextRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationTextRunner.cs), [ExplorationPresentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs)|PackedC0, explicit name/class, LEADER list rebuild, waits/gestures/fades have consumers. Missing selected texts/portrait/bleep/mapsprite/audio/material binding and exact token-time/input/rendering/timing/persistence acceptance; lowerable join ceremony does not prove it completed.|
|[MapEventDispatcher](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs), [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [map preparation](../../../../src/sf2tool/remake_exploration_maps.py)|Ordinary positive-coordinate no-scroll warp and ordered battle/setup selection shapes exist, including selected completed admission. No selected31/74 route/world binding or original mutable request/high-byte/flag-race parity. Pin rejection precedes any33 execution; source setup selection is not full Moun readiness.|

The isolated lowering probe constructs in-memory programs and a small ignored
receipt. It is not preparation, Application execution, service observation or
Godot acceptance. Earlier failures/guards can precede102; no absolute first
runtime failure is asserted. Engine changes and tests of this audit remain out
of scope; production does not load research JSON/disassembly on demand.

## Reproduction, acceptance and remaining boundaries

Load ignored `local/private-inputs.ps1` in the same process as each command.
Direct selected source/document/consumer check, on committed clean HEAD:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1086/review.py <configured-source-disasm-root> --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

Audit full phase semantics, widths, branch polarity/order and aliases; selected
tables and all52 commands/six inline streams/native/list/texts; return seed/crop,
join/AI/leave/list/LEADER/follow-HP joins; defeat/escape/Pacalon/raft; both warp
records/request/SwitchMap/CheckBattle and Map31 setup boundary; real recursive
lowering/Content/Domain/Application/Godot consumers. Review dead/already-joined/
capacity/skipped/interrupted scenes, changed selectors/flags/partial requests,
stale143/list tails/aliases and F88 without assuming a natural outcome.

Separately check exact accepted-base→HEAD two paths, clean status,
navigation-only predecessor, UTF8/LF/links/private boundary and cached diff.
Record actual exact-head scoped CI and receipts in Issue/PR handoff. Current
documentation-only policy skips blanket normal/full Python/.NET/Godot/H3/H4,
old comparisons and tests of review programs; planner output is interpreted
under that policy, not used to restore superseded blanket gates.

Retain #1084 independent0.294s/CI39493788988, #1082 independent0.222s/
CI39491194567; #1080 inventory-NOTHING versus definition-equipment mistake,
clean-head AssertionError[] and focused0.107s/CI39489091772; #1076's nested
action correction and earlier LEADER/operand-width/HP/geometry/N==V/Unknown
chain with their owning receipts. Twelve completed old comparison failures
are discovery evidence, not a rerun queue. Preserve each receipt/lowering
before another run; failed commands and focused corrections stay in handoff.

**Unknown:** natural terminal/region/Jaro/travel reach; supplied flags/party/
classes/HP/items/lists/death entries/old143/crop/lookups/mode/selectors/request
bytes; completed load/text/join/input/action/follow/camera/audio/cleanup services;
interruptions/capacity/alias interactions, exact presentation/timing, usable
control and saved persistence. These are not an automatic acquisition queue.
Done is a clean committed/pushed frozen Draft PR with exact head/tree/base,
checks/failures/Unknowns and stopped writer/process handoff to independent
main-gate; no merge, cleanup/archive or next Issue in this task.
