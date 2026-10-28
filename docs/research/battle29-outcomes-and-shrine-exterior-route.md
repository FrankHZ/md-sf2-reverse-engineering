# Battle29 outcomes and shrine exterior route

Continue the admitted encounter in [Battle30 initialization and opening](./battle30-initialization-and-opening.md).

Evidence date: 2026-10-20. **Confirmed (static):** Battle29's empty defeated
and after scripts retain shared leader cleanup and JoinForce0. Victory returns
a member-derived Map0 seed and enters fresh exploration without another battle
scan. Map0 rebuilds followers plus one Chaos Wizard entity. Its two door-zone
rows conditionally run `cs_5E3C2`; working blocks, F840 and the later F801 write
are separate effects. Four no-scroll warps request Map1 or cave35; only the
later MainLoop selector can admit Battle30. **Inferred:** the dialogue presents
Zalbard's invitation and Astral's distraction over Odd Eye. **Unknown:** natural
reach/outcome, supplied state, completed services/control, presentation/timing
and persistence. No runtime or remake readiness is established.

## Agreement and provenance

Standard rigor for [Issue #1062](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1062)
under [data epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
outcome flags, scene latches, physical actors/aliases and working layout have
different lifetimes. Accepted base is `d8f085773f76e7b7120f0a8811a717809f776799`,
including #1058/PR1059 and #1060/PR1061. This is the sole new owning document;
the [activation owner](./battle29-activation-and-first-turn-dispatch.md) gains
one navigation link. No shared index, schema, fixture, tool, remake, design or
translation changes. UI692 is unrelated.

Start conditionally after an individual turn returns, beyond the predecessor's
first-call boundary. Cover terminal/escape returns, explicit Map0 exploration,
its complete setup/resources, the door scene and outbound admission. Stop before
destination Map1/Battle30 or cave35 bodies. No combat simulation, action/menu/AI
re-audit, shrine-interior narrative, collision route, runtime/capture, extraction
or new comparison platform. Reuse [initialization/opening](./battle29-initialization-and-opening.md),
[Map35/0 admission](./tristan-caves-and-shrine-exterior-admission.md),
[Battle27 outcomes](./battle27-outcomes-through-tristan-admission.md),
[Battle26 outcomes](./battle26-outcomes-and-higins-recruitment.md),
[fresh exploration/followers](./battle26-return-through-pacalon-entry.md),
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[common maps](./common-maps.md), [scripting](./common-scripting.md),
[request state](./map-event-request-state.md),
[request consumption](./map-event-request-consumption.md) and
[item transactions](./map-event-item-transactions.md).

Paths below are relative to `disasm/` at clean, read-only pinned
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Existing provenance binds USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM/H1/runtime equality is claimed. `sf2enums.asm`/`sf2const.asm`
own constants/RAM; `sf2mapmacros.asm`/`sf2mapsetupmacros.asm`/`sf2cutscenemacros.asm` own row/command
widths. Fnnn means flag nnn; rows/commands are zero-based, extents half-open.
Short `battleloop/`/`cutscenes/` paths mean `code/gameflow/battle/`;
`map/`/`entity/` mean `code/common/scripting/`.

Minimum evidence is selected source/tables, one complete scene and existing
fixture/consumer joins. Reuse unchanged 18,285,979-byte init/script pair and
accepted 0.083s provenance; do not re-extract. Linear checks are budgeted below
one minute/512 MiB, small local receipts, no acquisition or API processing.
Contradictory semantics or material supporting-system growth require reassessment.
The checklist applies proportionally: no parser/schema/test machinery is added.

## Conditional terminal and escape returns

**Confirmed:** `battleloop_1.asm:BattleLoop` resumes from ExecuteIndividualTurn,
optionally executes the debug kill-all branch, then at `@IsBattleEnd` calls
ExecuteBattleCutscene_Defeated, ProcessKilledCombatants and CountRemainingCombatants.
Ally count0 selects defeat before enemy count0 selects victory. Otherwise it
reads the actual current turn byte, calls ProcessAfterTurnEffects, then repeats
death processing and ordered counts, without a second defeated hook. Only a
nonterminal suffix advances CURRENT_BATTLE_TURN.b by2. No actual turn is executed here.

The complete selected tables give these records:

| Table / extent | Battle29 selection |
| --- | --- |
|`data/battles/cutscenes/battleendcutscenes.asm`, 48 relative words, `$47BE8..$47C48`|slot29/byte58 at `$47C22` selects ms_Empty.|
|`data/battles/global/enemyleaderpresence.asm`, 45 bytes, `$47C8E..$47CBB`|byte29 at `$47CAB` is -1.|
|`data/battles/cutscenes/afterbattlecutscenes.asm`, 48 relative words, `$47CF4..$47D54`|slot29/byte58 at `$47D2E` selects ms_Empty.|
|`data/battles/cutscenes/afterbattlejoins.asm`, 52 bytes, `$47D6A..$47D9E`|byte29 at `$47D87` is0; this is the layout-included owner.|

`battleendcutscenesstart.asm` first requires Bowie HP.w nonzero and enemy128
HP.w zero. It checks actual completion500+CURRENT_BATTLE; clear529 dispatches
the empty map script, held529 skips it. Both continue through
`battleendcutscenesend.asm`: reread CURRENT_BATTLE, select leader-presence byte,
then for nonzero scan all32 enemies128–159. Every nonzero-HP enemy is appended
to the existing death list, length incremented and sentinel rewritten. This
neither clears the existing length nor grants action-side item drops. The
selected fifteen-row fresh roster does not shorten the32-slot scan. Missing,
stale or duplicate mappings/list entries are consumed as supplied; an interrupted
cleanup retains its reached prefix.

ProcessKilledCombatants consumes the length-driven list through spin/effect/
sound waits, increments ally defeats or credits enemy kills to the supplied
BATTLESCENE_FIRST_ALLY, writes combatant X/Y=-1, clears status/recomputes stats,
and moves the mapped entity to `$7000,$7000`. It does not set that enemy's HP0,
clear the list length or run the item-drop transaction. Count termination can
therefore follow off-map placement while HP is still nonzero. The layout also
includes the after-leader-death positions table, whose sole battle is5, not29;
there is no Battle29 positioning scene to infer from the empty hook.

`CountRemainingCombatants` counts30 allies and32 enemies with signed X.b>=0
and HP.w!=0; Y, membership, AI/activation are not predicates. Bowie HP0 forces
ally count0, so simultaneous exhaustion loses. Off-map enemies can yield victory
without leader cleanup; high-bit nonzero HP passes these tests. The after-turn
consumer clears death-list length and can build a new lethal-poison list. Enemy128
dying there does not immediately invoke a second leader hook. F88 uses supplied
current selectors/list/actors, not an assumed fresh scene or roster.

Complete `data/battles/global/enemyitemdrops.asm` has no Battle29 row. Its
actual current-battle/target consumer cannot match29; Battle30's enemy15 Mithril
row/flag27 is not inherited. No after-scene purse/recruitment is supplied by the
empty program. Ordinary combat gold/experience and earlier item effects remain
with their existing action consumers; this is not a claim that none occurred.

`battleloop_2.asm:BattleLoop_Victory` orders:

1. HealLivingAndImmortalAllies: all30 slots, no membership filter. Peter7 and
   Lemon28 even at HP0, and other allies with nonzero HP, receive max HP/MP,
   retain only already-held STUN/POISON/CURSE, and recompute stats. Other dead
   allies remain dead. Battle44 timer handling is not selected for29.
2. Store CURRENT_MAP.b in PARAM2. UpdateForceAndGetFirstBattlePartyMemberIndex
   rebuilds joined/active/reserve lists in ascending ID and reads the first
   active byte without a length/HP check. Add crop X/Y as **bytes** to that
   combatant's actual X/Y into PARAM3/4. Resolve its physical entity lookup,
   scale by32 and read facing into PARAM5; write PARAM1=0, not pending type.
3. ExecuteAfterBattleCutscene checks actual500+CURRENT_BATTLE: clear529 runs
   ms_Empty, held529 skips it. Both reach EndAfterBattleCutscene, reread the
   current selector and call JoinForce with selected byte0. JoinForce0 sets F0,
   rebuilds lists and sets F32 only when active count<12. At capacity it does
   not clear an existing F32. A newly set active bit is not reflected in the
   already-built lists until another UpdateForce. No stats/healing/follower or
   join presentation is supplied, and the earlier return seed is not repaired.
4. Reread CURRENT_BATTLE, clear unlock400+index then set completion500+index:
   unchanged29 clears429 and sets529. F479 remains supplied. No430/530,801/840
   or destination flag is newly set by this tail.
5. Clear d0–d3 **words**, copy PARAM2–5 bytes, set d4=1 with moveq and return.
   Their upper register halves are not established by clr.w.

With unchanged Map0/crop `(0,0,32,38)`, the victory seed is Map0 and the actual
first active member's coordinate bytes/facing, not fixed `(15,33)` or a door
entrance. It need not be Bowie, alive or onscreen; empty active lists expose
retained first-byte contents. Only a zero lookup selects physical0.
`entity/entityfunctions_1.asm:PositionBattleEntities` writes `$FF` for an
unplaced ally. `battlefunctions/battlefunctions_0.asm:GetEntityIndexForCombatant`
returns that byte without rejecting it; victory directly shifts the result by5
and reads facing at `ENTITY_DATA+$1FE0+$10`. This is an unchecked read, not
physical0 or a valid entity guarantee. Unlike `battlefunctions/loadBattle.asm`,
victory has no negative-lookup fallback or `$3F` mask before the shift. Other
lookup bytes are likewise consumed without validation. Changed map/crop/actor/
PARAM/current-battle values affect their own live reads. Checked completion,
script selection, join tail and final flag writes are not one snapshot. Stopping
after healing, seed, join,429-clear or529-set does not complete the remaining suffix.

| Alternative | Ordered effects / returned state |
| --- | --- |
|Defeat|UpdateBattleUnlockedFlag; clear dialogue name1; sad music/text363/close; Bowie HP=max; unsigned gold shift-right1; GetEgressPositionForBattle; d4=-1. Battle04's losable upgrade is not selected.|
|Admitted Egress|Deduct selected spell MP cost; close battle windows; text274 and common cast sound/flash/close; unlock update; unwind action frame; egress helper; d4.w=0.|
|Admitted Angel Wing|Remove selected item slot/recompute stats before windows/text275; same escape tail, d4.w=0. No defeat gold penalty/Bowie restoration.|

`battlefunctions/battlefunctions_1.asm:UpdateBattleUnlockedFlag` leaves429
unchanged if529 clear, clears429 if529 held. Defeat/retreat do not newly set529,
run the after/join wrapper or change479. Escape bypasses the post-turn terminal
pass. Earlier costs, inventory, flags and combat writes are not rolled back;
subsequent exploration healing can change eligible HP/MP again.

`battleloop/getegresspositionforbattle.asm` special-cases13/16/17/31/32 and
38/39 unlocks, not29. It copies EGRESS_MAP.b to the existing d0 low byte, then
`code/common/maps/egressinit.asm:GetSavepointForMap` scans its actual table.
With F399 held (BattleLoop sets it on fresh and resumed entry), supplied15
returns Tristan `(12,16)` Up;2 returns Pacalon `(41,10)` Up;36 returns castle
`(40,5)` Up. Neither0 nor35 has a savepoint row, so those supplied maps retain
map and default `(1,1)` Up. This is not a prior save/church visit or guaranteed
Map0 return. Matched fields are byte writes; d0's incoming high word-byte is
not globally cleared. F399-clear direct entry instead returns initial3
`(56,3)` Down before raft reset, outside the ordinary reached premise.
Held64 resets raft from its complete table: Polca/Bedoe→72 `(43,48)`, Hassan→69
`(28,55)`, otherwise69 `(42,15)`. Clear64 leaves raft fields. No alternate
savepoint destination body is entered here.

## Fresh Map0 exploration, setup and population

**Confirmed:** `code/gameflow/mainloop.asm:alt_MainLoopEntry` always calls
SwitchMap then ExplorationLoop after a completed battle return, with no d4 sign
test and no intervening CheckBattle. The complete switch table is3/F640→4,
71/F530→78,58/F999→59, first matching set flag with **word** map comparison.
It has no0/1/35 replacement. Victory's returned0 therefore enters exploration;
this path does not test any surviving or changed unlock. Defeat/retreat follow their
actual returned map. The following applies only to explicit0 entry:

1. ExplorationLoop clears pending event **word**, retaining parameter bytes;
   subtracts20000 from STEP_COUNTER.w using signed BGE/zero fallback; heals
   living/immortal allies; requests all-palette fade-out and view target-1.
2. Write CURRENT_MAP0 and CURRENT_BATTLE255; load selected tilesets; wait for
   fade, clear VInts, get setup entities, InitializeMapEntities and mapsprites;
   clear temporary F256–383 and set80.
3. Initialize sprite counters; LoadMap with saved0/view target0 and camera
   argument-1; install base VInts; run setup init; request music/fade-in only
   when compared palette entries differ; set movement sound, WaitForEvent.

Services must return. Pending zero installs physical0 control; a pending event
wins over A/C and bypasses installation. BattleLoop cleared PLAYER_TYPE, and
selected outcomes do not change it; ordinary return is numeric0. Exploration
does not normalize changed modes: control uses0 character/1 raft/other nonzero
Caravan, while raft construction uses enum-valued mode2. Completed control is
unobserved. CURRENT255's distinct reload arm retains actors/layout/current-battle/
temporary flags while updating main/raft and running load/init; none of the four
intact outbound rows requests it.

Complete `data/maps/mapsetups.asm` has one unflagged `msMap 0, ms_map0`, followed
immediately by msMapEnd; there is no529/801/840 alternate. GetCurrentMapSetup
rereads CURRENT_MAP and otherwise uses default/last-held flag semantics, through
the global terminator. Retained map-init route sourceOrder115 joins pointer
`$5E37C` to init `$5E628`; all six pointers are:

| Offset / target | Complete Map0 declaration |
| --- | --- |
|0 /ms_map0_Entities|`$5E394..$5E39E`: fixed `(63,63)` Down, CHAOS_WIZARD138, eas_Init; msEntitiesEnd.|
|4 /ms_map0_EntityEvents|`$5E39E..$5E3A4`: default Map0_DefaultEntityEvent, RTS.|
|8 /ms_map0_ZoneEvents|`$5E3A4..$5E61E`: two exact rows `(14,12)`/`(15,12)` to Map0_ZoneEvent0; default RTS; callback/scene below.|
|12 /ms_map0_AreaDescriptions|`$5E61E..$5E620`: RTS, not a description-row table.|
|16 /ms_map0_Section5|`$5E620..$5E628`: default Map0_DefaultItemEvent0, RTS.|
|20 /ms_map0_InitFunction|`$5E628..$5E62A`: RTS.|

InitializeMapEntities multiplies supplied main X/Y **words** by384, ClearEntities
resets actors/lookups/walking scratch, then allocates ordinary flag followers,
fixed row and main physical0/Bowie with supplied facing. Map0 is neither excluded
nor overworld: no F65 Caravan prefix. The complete ordered follower table is
[shared evidence](./battle26-return-through-pacalon-entry.md#conditional-map02-population-and-first-eventcontrol-read);
its selected meaning here is F67→Peter7; F74→Astral alias31, then F68→Oddler31,
then F70→Astral31, with later enabled aliases overwriting earlier mappings.
Other enabled followers still consume physical slots. Fixed Chaos Wizard gets
logical128/lookup32 at physical F+1 after F ordinary allocations; main stays0.
There is no fixed Peter or Astral row, no recruitment, scene load or flag enabling
those followers. Missing31/7 aliases can resolve physical0. Arbitrary follower
capacity/duplicate state is not proven safe. Raft physical31 and logical
FOLLOWER_B31 are distinct; F64/RAFT_MAP/mode can separately construct raft31.

Battle LoadBattle actors and bbcs_29's priest/devil mappings are replaced.
Logical128 now refers to the exploration Chaos Wizard, not a surviving enemy
combatant128 or the priest's later Dark Knight sprite. The door script's dialogue
names Zalbard; it does not load his battle stats or replace138 with a Zalbard
sprite. Actual portrait/speech lookup uses the mapped actor's **live mapsprite**.
Narrative speaker identity and physical/mapsprite identity must stay distinct.

Empty callbacks retain caller effects in `map/mapsetupsfunctions_1.asm`:
init saves/restores registers and calls RTS; area description clears/tests d7=0,
allowing generic no-description fallback. Item wrapper clears speech SFX, masks
item ID, stores EVENT_RELATIVE_POSITION, sets d6=0, selects default/RTS, closes
portrait/text, then waits one VInt and main motion. Entity wrapper stores
relative position, selects its default, derives portrait/speech and physical
alias, uses entry behavior bits for facing/restoration, clears the entity VInt
flag around RTS, closes windows and reactivates VInt_UpdateEntities. The default
macro has behavior0, so its optional turn/restore branches are not selected.
Zone wrapper selects exact byte coordinates before default; after callback it
closes windows, waits one VInt and main motion. ProcessMapEventType6 first idles
main and reads pending coordinate **words**. Empty callbacks do not disable
shared search, player-menu or event handling, and do not prove usable interaction.

## Map0 resources and independent working blocks

**Confirmed:** `data/maps/entries.asm:Map00` and all `entries/map00/` resources
are reused from the opening owner. Header `$94CC6..$94CCC` is palette0,
tilesets `[5,255,40,41,42]`; signed absent255 skips slot2. Ten long pointers
through `$94CF4` select own blocks/layout, areas, flags, steps, roofs, warps,
chest/other items and a present animation program. No binary decode is repeated.

Sole area `$94CF4..$94D14` is `(0,0)..(31,37)`, foreground origin `(0,0)`,
background `(32,0)`, parallaxes256/256 and16/256, autoscroll0/0 and0/1,
layer byte255, MUSIC_MITULA_SHRINE. Explicit exploration resets CURRENT_BATTLE255,
so full map area replaces Battle29 crop; old crop RAM need not clear. LoadMap
uses actual viewed-actor coordinates, inclusive bounds, first-area fallback.
Layer255 is a nonzero Plane A selector at actual byte consumers, not missing
content. No completed display/DMA/priority/timing is established.

| Resource / extent | Complete records and effect |
| --- | --- |
|Flag events `$94D14..$94D1E`|F840 copies source `(0,38)`, size2x3, destination `(14,10)`; terminator.|
|Step events `$94D1E..$94D20`|endWord only.|
|Roof events `$94D20..$94D22`|endWord only.|
|Warps `$94D22..$94D44`|four eight-byte rows plus terminator, below.|
|Chest `$94D44..$94D4A`|one `(5,19)`, flag179, ITEM_SHINING_BALL16, then terminator.|
|Other items `$94D4A..$94D4C`|endWord only.|
|Animation `$95E86..$95E9C`|mapAnimation46,32; entries `(0,16,$170,20)` and `(16,16,$170,20)`; terminator.|

`LoadMapBlocksAndLayout` on explicit load decodes original blocks/layout, applies
held flag copies and held chest markers. F840 rebuilds the same2x3 rectangle as
the scene, independently of801. F179 sets chest block `$D802` at5,19. CURRENT
reload preserves working layout instead. A changed flag need not immediately
change retained blocks; a script block copy can exist before its flag write.
Empty step/roof/other-item sets do not remove layout-kind producers.

CheckArea in explorationfunctions_0.asm searches the tile ahead by actual layout
kind: chest `$1800` precedes vase/barrel/bookshelf/area-item and setup-description
fallback. GetChestItem/GetItem in exploration.asm read current map's table,
compare coordinate bytes and produce flag/item plus masked64x64 layout offset;
CURRENT_BATTLE255 avoids battle-coordinate adjustment. CheckChestItem additionally
requires live chest layout kind. OpenChest on hit checks179 and substitutes
ITEM_NOTHING when held; both hit paths set179, open working block and mark Plane A update for
nonzero layer. Inventory/text/grant continuation is separate and can fail or stop
after those effects. The row alone proves no approach, successful pickup, item
capacity or persistence. RTS descriptions/items are not a universal search veto.

## Door-zone scene and distinct completion states

**Confirmed:** exact zone rows14,12 and15,12 share Map0_ZoneEvent0. The ordinary
character controller admits the zone producer on layout kind `$1400`, independently
of warp kind `$1000`; posted coordinates are candidate movement coordinates,
not proof of arrival. RunMapSetupZoneEvent matches byte coordinates; `$FF` alone
is its coordinate wildcard, unlike warp's any-negative rule. The callback checks
801: held returns; clear executes script cs_5E3C2, then sets801 **after return**.
Neither callback nor scene checks529/430/840. No automatic init call enters it.

`cs_5E3C2`, `$5E3C2..$5E61E`, has98 top-level commands/490 encoded bytes;
inline movement/action bodies supply the remaining114 bytes of604. The complete
stream is linear, with one setF840 and no choice, join, item/reward, battle unlock,
map transfer, actor load or sprite replacement. Narrative references below are
paraphrases of source comments, not a new text extraction. S means nextSingleText;
T means nextText. Sequential text IDs assume no skip/interleaved cursor change.

| Text / command | Speaker operand / mode | Narrative reference |
| --- | --- | --- |
|2731 /3|128 /S|Welcome to the Granseal force.|
|2732 /8|Peter7 /S|Surprise.|
|2733 /13|FOLLOWER_B31 /S|Voice beyond the door.|
|2734 /20|128 /S|Zalbard identifies himself as a greater devil.|
|2735 /27|128 /S|Invitation, or retreat and abandon Mitula.|
|2736 /29|Peter7 /T|Addresses Astral.|
|2737 /30|Peter7 /S|Bedoe attacker compared with Volcanon.|
|2738 /31|128 /T|Asks whether that was Odd Eye.|
|2739 /32|128 /S|Asks about the Bedoe fight.|
|2740 /34|31 /S|Says Odd Eye lost.|
|2741 /35|128 /T|Disbelief; calls Odd Eye strongest.|
|2742 /36|128 /T|No recent contact with him.|
|2743 /37|128 /T|Jewel of Evil could enable replacement.|
|2744 /38|128 /S|Invites entry; Mitula is inside.|
|2745 /40|Peter7 /S|Asks how to face the waiting devil.|
|2746 /46|31 /S|Repeats Odd Eye's name.|
|2747 /47|Peter7 /S|Calls Astral.|
|2748 /49|31 /S|Thinks the name familiar.|
|2749 /63|Peter7 /S|Calls Astral again.|
|2750 /64|31 /S|Distracted response.|
|2751 /69|Peter7 /S|Louder call.|
|2752 /74|31 /S|Startled; NAME7 substitution addresses Peter.|
|2753 /79|Peter7 /T|Asks what is wrong.|
|2754 /80|Peter7 /S|Asks whether to enter.|
|2755 /81|31 /S|Apology.|
|2756 /86|31 /T|Zalbard compared with Odd Eye.|
|2757 /87|31 /T|Still a greater devil.|
|2758 /88|31 /S|Prepare before entering.|
|2759 /89|Bowie0 /S|Agreement.|

Complete ordered action phases (command ranges include all intervening text):

1. 0–20: cursor2731; init Peter/31; welcome; jump31/Bowie/Peter, waiting only
   for Peter to reach idle; Peter Down, wait5, Peter2xRightLeft, wait120;
   31 Up/voice; Bowie Up; nonwaiting setDest `$801F`→13,12 and waiting Peter→16,12;
   both Up; camera8,9; Zalbard identification.
2. 21–40: deactivate autofacing31/Bowie/Peter; move31 Down2, Bowie Down1,
   Peter Down2 with wait; invitation; Peter Left; Bedoe/Odd Eye exchange,
   Peter Up before31's answer; wait60; Peter's question.
3. 41–57: Peter Left; init31/Peter;31 speed8/8 then idle; Left2 wait,
   name repetition, Right2 wait, familiarity; Peter Down1/Left3 wait;
   Bowie Down, Peter Up, camera9,12,31 Down; deactivate31/Peter autofacing;
   Peter speed8/8 then idle.
4. 58–69:31 Down2; wait16; Peter Down2; waitIdle31, stop Peter;
   call/response;31 Down2 and Peter Down2; waitIdle31, stop Peter; louder call.
5. 70–89:31 speed48/48 then idle, jump wait, deactivate autofacing, Up4 wait;
   startled line, shiver31; Peter speed32/32 then idle, camera9,11, Up3 wait;
   question/apology;31 speed32/32 then idle/init, Right1 wait, Up;
   caution/preparation and Bowie's response.
6. 90–97: nod Peter; init Bowie/Peter; follow31→Bowie at offset selector2,
   then Peter→31 at2; copy2x3 blocks `(0,38)`→`(14,10)`; set840; csc_end.
   Only the enclosing callback then writes801.

`mapscriptengine_2.asm:csc04` writes the text cursor; csc00/csc02 resolve live
speaker alias/portrait/speech, wait for view scroll and DisplayText, then increment
cursor. csc00 additionally closes portrait/text and sleeps10; csc02 leaves the
window open. Text-skip branches consume speaker operands without display/cursor
increment. ExecuteMapScript clears skip on entry, but a changed skip/cursor can
alter later IDs. This static stream proves references, not successful text waits.

`mapscriptengine_1.asm:GetEntityAddressFromCharacter` masks logical identity,
remaps enemy IDs into lookup32+, reads the physical byte and scales32; missing
aliases are not rejected. csc29 setDest masks `$801F` to31 but retains bit15 as
its no-wait choice: writes word destinations scaled384, signed travel and
velocity±32; Peter's unflagged command waits for motion. It does not teleport.
csc14/15 install scripts and physical-slot read timers; waiting variants require
`eas_Idle` address equality, as does csc16 waitIdle. csc2D builds
movement scripts in walking scratch, applies its slot timer/flags and optional
wait; custom scripts set word speeds then jump to idle. Busy/idle completion,
collision-free traversal and elapsed time are not inferred from macro names.
Some facing/follow consumers first apply ally-alive guards, independently of
mapping. csc2C resolves both actors and selector2 into signed offset `(-24,0)`;
these are follower offsets, not two map tiles. Nod/shiver use their concrete
sprite/DMA/wait consumers; actual projection/timing remains unobserved.

csc34 calls CopyMapBlocks, then sets both plane-update bits; csc10 setF840 uses
SetFlag. The matching persistent840 record is consumed on a later explicit load.
These effects and the caller's801 write permit distinct states:

| Supplied/reached state | Consequence at this seam |
| --- | --- |
|801 clear,840 clear|A matching zone starts the scene; no completed-door state is assumed.|
|801 held,840 clear|Callback skips; explicit reload does not apply the840 copy. No consistency repair.|
|801 clear,840 held|Explicit reload applies door copy, but zone still runs the scene.|
|Copy returned, before840|Working rectangle/update bits can change while both flags remain clear.|
|840 set, before script/callback return|Copy and840 reached;801 can remain clear, allowing another later callback.|
|801 set after return|Scene invocation returned; later wrapper/window/motion/control services can still be unfinished.|
|Flags or layout changed after loading|Live blocks can disagree with flags until an actual copy/rebuild;801 never reconstructs them.|
|Follower flags/mappings changed after allocation|Flags alone do not create actors; commands use actual aliases and may affect physical0 or another actor.|

None of these rows proves natural reach, rendered open doors, actual passage,
completed invitation or save-load durability. F529 is battle completion, F479
intro admission, F801 scene-return latch and F840 persistent block selector;
none substitutes for another, and840 is not the Battle30 unlock430.

## Four warp requests and actual destination selectors

**Confirmed:** complete Map0 warp order, each warpNoScroll0 with padding0:

| Row / address | Source X,Y | Map / destination X,Y / facing |
| --- | --- | --- |
|0 /`$94D22`|14,11|Map1 /10,31 /Up1|
|1 /`$94D2A`|15,11|Map1 /12,31 /Up1|
|2 /`$94D32`|14,35|Map35 /15,23 /Down3|
|3 /`$94D3A`|15,35|Map35 /16,23 /Down3|

No row tests801/840/529/530, membership or an item. Ordinary character control
requires live layout kind `$1000` to call WarpIfSetAtPoint; the preceding door
copy may affect layout, but no decoded approach/collision proof is supplied.
The producer sign-extends internal coordinate words, divides384, reads current
map's warp pointer, checks word `$FFFF` first, compares low bytes in order and
accepts any negative source byte as wildcard. These four sources are exact.
First match writes event word1, PARAM1–4 longword, PARAM5/padding word and
WARP_SFX in order. No match leaves pending state intact; no queue or pending
guard snapshots these separate writes. Zone and warp are different requests.

WaitForEvent reads pending before A/C. ProcessMapEvent clears stored type before
dispatching its saved value. Zero PARAM1 selects ProcessMapEventType1_Warp's
no-scroll arm: submit/clear warp sound, idle main, remove the exploration return
address, clear d0–d4 words, read map byte into d0, then sample actual main X/Y
with signed division384 and actual facing. Nonnegative PARAM3/4 replace only
coordinate low bytes; negative bytes retain sampled words. PARAM5 supplies
facing and PARAM1 supplies d4.b. Changed/interleaved pending/service state is
read live. Thus intact ordinary nonnegative samples yield the table seed/d4=0;
a sampled negative high byte can survive as `$FF0A`, for example, rather than
normalized X10. The request does not itself load the destination or place an actor.

This return reaches MainLoop `@Start`: **SwitchMap then CheckBattle**.
Inspection of the complete three-row switch table and45-row BattleMapCoordinates,
not a name-based assumption, gives:

| Actual returned low map byte | Complete relevant battle rows / admission |
| --- | --- |
|1|Only row30: `[1,0,0,24,34,255,255]`. Clear430 gives d7=-1/exploration. Held430 admits30 regardless of X/Y; crop0,0,24,34 is written. Held530 additionally clears430 but still admits30 on this call.|
|35|No battle row; all unlock/completion combinations miss, d7=-1/exploration, no crop write.|
|0 (changed request)|Only row29: `[0,0,0,32,38,255,255]`; held429 admits29, held529 also clears429 without vetoing that admission. Clear429 explores.|

SwitchMap has no0/1/35 row, including high-word-byte variants. CheckBattle tests
map/trigger **bytes**, traverses rows0–44 in order and requires the matched
unlock. `$FF` trigger bytes skip coordinate comparisons, not area containment.
Completion is tested after crop writes; it clears unlock then returns that
battle index. A total miss restores original map operand and leaves crop/flags
unchanged. CURRENT255 resolves the live current map for this scan only; no intact
row here requests it. Other changed maps consume their actual earlier table
matches rather than inheriting these three results.

MainLoop replaces d1 with d7 and calls j_BattleLoop on admission (map1/battle30);
otherwise calls j_ExplorationLoop with returned seed (Map1 or35). **Those calls,
before their destination bodies, are the endpoint.** No430 is set by victory,
Map0 RTS init or cs_5E3C2; it must already exist from an independent accepted
producer or supplied state. No destination story, Battle30 initialization,
interior exploration, cave path, natural entry or successful control is claimed.

## Existing representations and actual consumer gaps

**Confirmed about accepted code:** research/fixture expressibility, prepared
Content and running Application/Godot are separate. This slice prepares no
Map0/1/35 package, encounter29/30, script or observation. The
[capability owner](../../remake/docs/capability-status.md) retains runtime scope.
Production consumers do not read research fixtures/disassembly on demand.

| Boundary | Actual existing join / bounded gap |
| --- | --- |
|Research shape|`battle-loop`, `battle-control`, `battle-cutscenes`, `battle-global-data` fixtures/schemas retain common tails/tables; `map-setup`, `map-content`, `map-init`, `map-script-engine` retain setup/resource/corpus/consumer facts. Retained route115 and full cs_5E3C2 were joined directly. These express evidence, not a playable package.|
|Content admission|[PrivateBattleEncounterReader.Pin](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) admits1/2/3, rejecting29/30. Missing encounter/party/start/outcome/material and map/destination bindings remain; source rows do not admit them.|
|Outcomes and routes|Application [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs) and existing outcome-return policies support bounded battle/field transitions, healing, flags, population and ordered routes. Correct completion-admission policy and member-derived seed/egress/save/selector bindings are required; no general changed-register/pending-slot or all32 cleanup parity follows. F88 suspend is explicitly Unsupported.|
|Scene/population/text/motion|[StoryProgram](../../remake/src/Sf2.Remake.Application/Content/Scenarios/StoryProgram.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), SceneEntities and FollowerMotion already express flags, text/waits, actor aliases, speeds, moves, facing, followers, camera and gestures. [Preparer](../../src/sf2tool/remake_exploration_programs.py) supports many corresponding macros. No complete Map0 binding is supplied: setDest/setBlocks have no selected compiler branch or dedicated StoryInstruction; fallback produces UnsupportedInstruction. Generic MoveEntityAbsolute/layout-copy mechanisms do not automatically implement those exact script consumers. Per-command/entity/portrait/wait/service binding remains unassessed.|
|Working layout/events|Canonical typed flag/step/roof/warp/chest rows, WorkingMapLayout and [MapEventDispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs) express persistent flag block copies and field event subsets. Script block-copy/caller completion must remain distinct; no ready840/801 lifecycle or route is established.|
|Map0 presentation|Declared ParallaxBX16/AutoscrollBY1 is **Unsupported** in both [ExplorationViewRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs) (`field-view-profile`) and Godot [BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs) (`battlefield-map-plane-motion-unsupported`). Resource binding alone cannot admit it. Actual materials/palette/animation/crop/projection and exact timing are separate missing bindings or unassessed behavior.|
|Search/chest/items|[ExplorationDefinition](../../remake/src/Sf2.Remake.Application/Content/Scenarios/ExplorationDefinition.cs) event kinds are Step/Interact/Warp/SourceZone; chest/search/area-item transactions remain Unsupported. A flag/text event cannot replace marker, block, inventory/grant and continuation semantics.|

No engine/preparer/Godot execution, fix or new generic unsupported claim is
introduced. Existing mechanisms are acknowledged without promoting them to
complete source fidelity or runtime acceptance for this route.

## Reproduction and remaining boundaries

Load current `local/private-inputs.ps1` in the launching process. The ignored
one-off review checks complete selected hooks/tables/scene/resources, actual
wrappers/consumers, changed-state cases, fixture relationships, UTF-8/links/
anchors/private/exact-path boundary. It adds no maintained verifier or tests of
review programs and does not regenerate the retained pair.

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1062/review.py <configured-source-disasm-root>
git diff --check
```

Review all prose against supplied flags/current selectors/mode/actor mappings/
positions/pending/layout and stopped prefixes; then on a clean committed head
run `uv run sf2 verify plan --base origin/main --head HEAD`. Keep actual exact-head
scoped CI and receipts in the Issue/PR handoff. Documentation scope skips blanket
normal/full Python/.NET/Godot/H3/H4. Preserve #1060's link-anchor failure/correction
and0.340s/CI/main-gate receipts; #1058's original/P2 correction and #1056/#1054/
predecessor failures/Unknowns; unchanged pair and twelve completed unrelated
remake comparison failures. No completed result is reset or relabeled.

**Unknown:** natural victory/defeat/escape/door reach, actual flags/party/HP/items/
current/saved state/RNG, member-derived seed and physical mapping, pending writes
and service interleaving, working blocks/collision, completed text/action/fade/
VInt/control services, presentation/audio/timing, save-load persistence and
unassessed consumer parity. Unknowns are not an emulator queue. Destination
Map1/Battle30 and cave bodies remain outside the acceptance boundary.
