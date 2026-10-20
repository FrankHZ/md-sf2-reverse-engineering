# Battle34 outcomes and the Sky Orb handoff

Evidence date: 2026-10-20. **Confirmed (static):** unchanged Battle34
victory saves its Map55 return seed before the Cameela/Geshp scene, then
clears434 and sets534. The scene hides its physical Orb before attempting
the distinct Sky Orb inventory award. That award scans the joined force,
including reserves, and enables a mandatory discard loop when everyone is
full. The later explicit55 return reconstructs exploration entities; it
does not retain the scene's Caravan follow arrangement. Map55's7,7 zone
checks534 before dispatching cs_5E27C. **Stop before that ship program's
body.** Natural reach, actual state, service completion, presentation,
usable control and saved persistence remain **Unknown**. No runnable34
or completed ship implementation is established.

## Agreement and evidence route

Standard rigor for [Issue #1104](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1104)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
provide reproducible outcome, after-scene, inventory and exploration facts
for later content and engine decisions. Accepted base is
`1f4bae449250536dfdf59e43b6bd5e6659bc2bb8`, including
[Battle34 initialization](./battle34-initialization-and-opening.md) and
[activation/first dispatch](./battle34-activation-and-first-turn-dispatch.md).
Own this document, one navigation link in that activation owner, and only
the death-counter attribution sentence in each of these accepted owners:

- [Battle23 research](./battle23-outcomes-and-burrow-return.md).
- [Battle26 research](./battle26-outcomes-and-higins-recruitment.md).
- [Battle30 research](./battle30-outcomes-and-mitula-scene.md).
- [Battle31 research](./battle31-outcomes-and-battle32-admission.md).
- [Battle33 research](./battle33-outcomes-and-zynk-handoff.md).
- [Battle23 synthesis](../design/synthesis/battle23-outcomes-and-burrow-return.md).

These eight paths correct the same factual attribution; no other design
scope, shared registry/index/schema/fixture, stacked dependency,
implementation or competing UI692 path is included.

Begin after a returning individual turn. Include terminal selection,
cleanup, victory/defeat/escape, complete selected scene and item helper,
then Map55 setup/init and four outgoing requests. Stop at the534-gated
ship dispatch; F999 revisit programs are named boundaries only. Exclude
individual-turn AI/combat algorithms, ship body/interior/voyage/shootdown,
next encounter, design/translation, runtime acquisition and new platforms.

Reuse [Battle33 outcomes](./battle33-outcomes-and-zynk-handoff.md),
[Moun Arm inventory boundary](./moun-town-painter-and-golem-arm.md),
[battle loop](./battle-loop.md), [battle cutscenes](./battle-cutscenes.md),
[common scripting](./common-scripting.md), [common maps](./common-maps.md),
[request producer](./map-event-request-state.md),
[request consumer](./map-event-request-consumption.md),
[lifecycle](../design/contracts/battle-control-lifecycle.md) and
[routing](../design/contracts/battle-cutscene-routing.md). Their shared
seams do not substitute for the selected34 tables, branches or consumers.

Original paths below are relative to `disasm/` at clean, read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM/H1/runtime equality is claimed. `sf2enums.asm`/`sf2const.asm`
own constants, widths and RAM. `battleloop/` and `cutscenes/` abbreviate
children of `code/gameflow/battle/`; `map/` and `entity/` abbreviate
`code/common/scripting/`. Fnnn means flag nnn; ordinals are zero-based,
intervals half-open. Named text keys are decimal unless marked otherwise.

Minimum evidence is complete selected wrapper/table/command/inline/list/
text/helper joins, actual current consumers and small changed-state checks.
Reuse the unchanged18,285,979-byte init/script pair and0.083s provenance
at `local/issue1005/pair.txt`; do not regenerate. One pair load, bounded
linear reads and one selected in-memory lowering fit one minute/512MiB
per direct audit, with small ignored receipts under `local/issue1104/`.
No bulk scan, runtime, emulator, Godot, SDK, screenshot or old comparison.
Reassess material contradictions or supporting-tool expansion before scaling.

## Terminal selection, cleanup and separate battle drops

**Confirmed:** `battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`) resumes
from ExecuteIndividualTurn, optionally performs debug kill-all, then calls
the defeated wrapper, ProcessKilledCombatants and CountRemainingCombatants.
Zero allies loses before zero enemies wins. A nonterminal first pass calls
ProcessAfterTurnEffects using the actual turn-buffer byte, processes deaths
and repeats the same count order without a second defeated hook. Only a
nonterminal second pass increments CURRENT_BATTLE_TURN.b by2. Admitted
escape unwinds before this suffix. An action-side death, cleanup, after-turn
effect and terminal result are separate boundaries.

| Complete selected owner / extent | Battle34 selection |
| --- | --- |
|`data/battles/cutscenes/battleendcutscenes.asm`,48 relative words, `$47BE8..$47C48`|slot34/byte68 at `$47C2C`: ms_Empty.|
|`data/battles/global/enemyleaderpresence.asm`,45 bytes, `$47C8E..$47CBB`|byte34 at `$47CB0`: -1, nonzero cleanup.|
|`data/battles/cutscenes/afterbattlecutscenes.asm`,48 relative words, `$47CF4..$47D54`|slot34/byte68 at `$47D38`: abcs_battle34, `$4D4B6`.|
|`data/battles/cutscenes/afterbattlejoins.asm`,52 bytes, `$47D6A..$47D9E`|byte34 at `$47D8C`:0, actual JoinForce operand.|
|`data/battles/global/afterbattlepositions.asm`|Only battle5; no34 override.|
|`data/battles/global/enemyitemdrops.asm`,30 four-byte rows plus terminator, `$BE52..$BECC`|34/ordinal2/combatant130: WISH_STAFF94, drop bit12;34/ordinal0/combatant128: IRON_BALL102, drop bit26, random range32. Neither is the Sky Orb award.|

`battleendcutscenesstart.asm` requires Bowie0 HP.w!=0 and enemy128 HP.w=0.
Clear actual500+CURRENT_BATTLE dispatches ms_Empty; held534 skips it for34.
Both enter `battleendcutscenesend.asm:loc_47C48`, reread the selector and
test its leader byte. Unchanged34's -1 scans32 enemy slots128–159 and
appends each nonzero-HP slot to the **existing** death list, updating its
word length and next sentinel. No initial clear or X/Y, membership, neutral
or activation test exists. The17 declared enemies do not shorten this sweep.

ms_Empty is one FFFF word. Its interpreter still changes scratch, clears
text-skip on entry, conditionally waits for view when dialogue remains and
clears scrolling speed on return. Skipping dispatch avoids those effects
but retains leader cleanup. An empty program is not a completed-service
observation. F88 supplies restored state, not fresh-list guarantees.

ProcessKilledCombatants requests spin/effect/sound waits, credits defeats
to the listed dead ally or enemy kills to supplied BATTLESCENE_FIRST_ALLY,
writes combatant X/Y=-1, clears
status/refreshes stats and moves the mapped entity to `$7000,$7000`.
At @CheckKillDefeat_Loop, d0 retains the listed ally for IncreaseDefeats;
only @IncreaseKills temporarily replaces it with BATTLESCENE_FIRST_ALLY,
then restores the dead enemy ID before cleanup. With Jaha3 listed and
FIRST_ALLY=Bowie0, defeat credit goes to3; a listed enemy instead credits0's kills.
It neither sets HP0 nor clears the list length nor calls the drop service.
Living queued enemies can disappear from counts through X alone. Duplicate
or stale entries/mappings and interrupted processing remain supplied state.
CountRemainingCombatants scans30 allies/32 enemies using signed X.b>=0 and
HP.w!=0; Y, membership, neutral and activation do not gate it. High-bit HP
passes. Bowie HP0 forces ally count0; simultaneous exhaustion loses. Off-map
enemies can permit victory without the leader hook, and a second-pass
after-turn kill of128 does not run that hook again in this suffix.

Selected130 is SHAMAN, equipped Wish Staff94;128 is CAMEELA with Iron
Ball102. `battleactions/dropenemyitem.asm:battlesceneScript_DropEnemyItem`
requires an allied actor, enemy target, targetDies, matching live battle/
entity and an actual matching item slot. Iron Ball alone here additionally
requires zero from GenerateRandomOrDebugNumber(range32); Wish Staff does
not. Only then does BSET test the separate enemy-drop bitfield's12/26,
followed by item removal and attempted attacker award. These are not story
F12/F26. A dead attacker or failed AddItem routes a RARE item to deals;
both selected definitions are RARE. Prior drop bit, missing weapon, failed
random draw or unreached action can avoid the award. Victory/leader cleanup
does not run this path or establish it happened. No new RNG observation,
drop-fixture change or combat algorithm survey is included.

## Victory seed, wrapper and completion order

**Confirmed:** `battleloop_2.asm:BattleLoop_Victory` (`$23CBA..$23D98` file)
first HealLivingAndImmortalAllies: all30 slots, Peter7/Lemon28 even at HP0,
other living members to maxHP/MP, other dead members remain dead. Existing
STUN/POISON/CURSE survive and stats refresh. Battle44's timer branch is not34.

Before the after scene, copy CURRENT_MAP.b→PARAM2; call
UpdateForceAndGetFirstBattlePartyMemberIndex; add BATTLE_AREA_X/Y **bytes**
to that member's actual combatant X/Y into PARAM3/4; map its physical
entity and read facing into PARAM5; write PARAM1=0. No event type is posted.
UpdateForce rebuilds joined/active/reserve prefixes and word counts in
ascending member ID without clearing tails or changing membership flags.
The first active byte is read without count/HP/position validation. It need
not be Bowie/alive; an empty active list exposes its retained first byte.
Intact Map55/crop0,0,32,32 saves55 and that member's X/Y/facing, **not**
scene main7,12, camera2,7, later Bowie movement or the Orb's position.

GetEntityIndexForCombatant's unchecked lookup0 aliases physical0; FF is
shifted by5 and reads facing from `ENTITY_DATA+$1FE0+$10`. There is no
LoadBattle negative fallback or3F mask here. Later reconstruction cannot
repair the earlier saved seed. Selector and parameter bytes are reread
across services rather than protected by one immutable selector snapshot.

`afterbattlecutscenesstart.asm:ExecuteAfterBattleCutscene` (`$47CBC..$47CF4`)
checks actual500+CURRENT_BATTLE. Clear534 runs the entire selected scene;
held534 skips it, including Orb display/hide, helper, dialogue and follows.
Both reach `afterbattlecutscenesend.asm:EndAfterBattleCutscene`
(`$47D54..$47D6A`), reread CURRENT_BATTLE and JoinForce selected byte0.
This sets F0, rebuilds lists, then sets active32 only if active count<12;
an existing32 remains at capacity, and a newly set32 is absent from the
preceding list snapshot. The tail joins Bowie, not Zynk26 or Caravan130.

After wrapper return, reread CURRENT_BATTLE, clear400+index, then set
500+index: unchanged34 clears434 then sets534. Clear d0–d3 **words**,
copy PARAM2–5 bytes into them, return d4=1. Upper halves are not established
by clr.w; PARAM1=0 differs from returned d4=1. No local F76/65, Zynk joined26/
active58, ship flag999, inventory-possession check or separate Orb-awarded
story flag is written. Item grant, wrapper completion and534 are distinct.

## Complete Cameela, Geshp and Orb scene

**Confirmed:** `data/battles/entries/battle34/cs_afterbattle.asm` spans
`$4D4B6..$4D6CE`. The83 top-level commands encode422 bytes including
csc_end; six movement and two custom inline streams add56 bytes, ending
at `$4D694`. csub_4D694 is10 bytes to `$4D69E`; the48-byte entity list
ends at `$4D6CE`. Retained script records give top-level declarations;
pinned source supplies complete nested actions/helper/list/text joins.

Here B=FOLLOWER_B31, P=Peter7, Z=Zynk26, C=logical128 (Cameela actor),
G=129 (Geshp actor), V=130 (Caravan actor), O=131 (Orb actor). These are
logical aliases, not physical slots or the old enemy combatants' identity.

| Index | Complete operation / boundary |
| --- | --- |
|0–7|textCursor2911; loadMapFadeIn55 camera2,7; loadMapEntities ce_4D69E; await Init Bowie, Init P; set P9,11/Up; await Init B; set B5,11/Up.|
|8–10|Read76; clear jumps to cs_4D502/11; held awaits Z Init and sets Z8,12/Up.|
|11–16|stop C animation; C customWait motion OFF/orientLeft/updateSprite/jump Idle; stop O animation; await O DeactivateAutoFacing; fadeInB; shiver C.|
|17–22|C single2911; narrator single2912; set O7,10/Left; C continued2913, continued2914, single2915.|
|23–31|tintMap; G single2916/C0; flashScreenWhite2; tintMap; await C Init; face C Left; C sprite OBJECT3; start C animation; install C AnimSpeedx2 without waiting.|
|32–36|install Bowie/P Jump without waiting; reread76, clear skips Z Jump; await B Jump at cs_4D592/36. This wait does not join the other actors.|
|37–45|C2917, G2918/C0, C2919; restore C sprite CAMEELA; await C Die; B2920; G continued2921, continued2922, single2923/C0.|
|46–52|flickerOnce; wait60; face P Left; P continued2924/single2925; face B Right; B2926.|
|53–63|Reread76; clear skips to64. Held: Z customWait speed24/24/jump Idle; Z actionsWait Up1 then Left1; Z2927; B actionsWait Right1; B2928; Z2929; B2930; face Z Up; wait30; Z2931.|
|64–69|textCursor2932 at cs_4D620, independent of skipped optional lines; P actionsWait Left1; P2932; B2933; P actionsWait Up1; face P Left.|
|70–72|Reread76; clear skips to73; held Z actionsWait Right1 then face Left.|
|73–75|nod Bowie at cs_4D654; Bowie actionsWait Up1; hide O.|
|76–79|executeSubroutine csub_4D694; follow V→Bowie offset2; follow P→V offset1; follow B→V offset3.|
|80–82|Reread76; clear jumps to csc_end at cs_4D692; held follows Z→V offset2, then csc_end.|

F76 is independently read at8/34/53/70/80, in addition to follower
construction. It does not freeze one branch choice. Stable clear76 omits
the optional Z sequence; stable held76 requests all of it. A changed flag
can admit later actions without earlier placement/allocation, or omit a
later follow after earlier action. No read joins Zynk or sets76.

Each movement stream ends endActions; the Z55 stream alone contains two
moves. Both custom streams terminate ac_end after ac_jump eas_Idle. csc14,
csc15 and csc2D resolve a mapped physical slot, use its index as read timer
and optionally poll its action pointer for Idle. csc2D builds relative move/
wait-destination commands in scratch, appends an Idle jump and clears flagsA
bits5/6. None uses the ally-HP skip helper. Awaiting Idle is not proof that
every other actor, VInt, sprite, audio or camera service finished.

Named bodies in `data/scripting/entity/eas_actions.asm` are complete at
their first ac_jump eas_Idle, not at the next globally named source label:

| Stream | Effective actions before Idle |
| --- | --- |
|Init|accelFactors0/0; speed32/32; acceleration and deceleration OFF/OFF; obstructable ON; entity/map uncollidable OFF; autoFacing ON; orientUp; immersed OFF; size24; updateSprite.|
|DeactivateAutoFacing|autoFacing OFF.|
|Jump|autoFacing OFF; relative0,-1; wait5; relative0,1; wait5; relative0,0; autoFacing ON.|
|AnimSpeedx2|animSpeedX2 ON.|
|Die|motion OFF; Left/Up/Right/Down, Left/Up/Right/Down, Left/Up/Right/Down with sprite refresh at each orientation; wait3 after all except the first Down; SFX_BATTLEFIELD_DEATH; sprite EFFECT1; Up/refresh/wait8, Left/refresh/wait8, Down/refresh/wait8; setPos63,63.|

Die requests11 waits of3 and three of8; it moves the physical actor off map
without HideEntity, alias removal or a combatant HP write. G is declared at
63,63 and never brought on map here; his remote dialogue is not proof of a
visible Geshp arrival/departure. flickerOnce does not hide G. C's OBJECT3
animation is separate from the later EFFECT1 death sprite.

`map/mapscriptengine_1.asm` and `entity/entityscriptengine_2.asm` own actual
services. Relative movement adds signed tile offsets×384 to the then-current
physical position words;
Jump uses timed waits, so an installed jump is not an exact completed arc.
stopEntity sets animation counterFF, not movement cancellation; startEntity
sets it0. shiver saves size/counter, uses size21, requests three paired
flagsB-bit3 sprite updates/Sleep5 and restores those saved values. nod's
sprite-crop/DMA waits and all sprite replacement updates must return.
tintMap requests LaunchFading(1,6,HALF_OUT_TO_BLACK); flickerOnce uses
(1,6,FLICKER_ONCE). flashScreenWhite2 shifts2 right3 and DBF launches once
with(15,1,FLASH_QUICKLY_2), then DuplicatePalettes. The operand2 does not
mean two completed flashes. No appearance, display duration or sound
completion has been observed.

csc19 setPos/csc23 setFacing peek a **byte** ID: allies0–29 with HP.w=0
skip; nonzero including high-bit proceeds; B31/C128/O131 bypass. Word-ID
stop/start/sprite commands and high-byte follow prechecks have different
boundaries. Z's positions/facing can therefore skip while his actions and
dialogue proceed. Physical actor position, combatant position, membership,
follower flags and inventory are independent authorities.

## Entity aliases, LEADER and following

**Confirmed:** ce_4D69E has a six-byte main header7,12/Up, then five
eight-byte rows and a two-byte terminator:

| Fixed row | Declaration | Logical lookup |
| --- | --- | --- |
|0|P9,11/Up, ALLY_PETER, eas_Init|7; allocate only if no earlier follower mapping.|
|1|C7,9/Down, MAPSPRITE_CAMEELA156, eas_Init|128, lookup byte32.|
|2|G63,63/Down, MAPSPRITE_GESHP159, eas_Init|129, byte33.|
|3|V6,12/Up, MAPSPRITE_CARAVAN62, eas_Init|130, byte34.|
|4|O63,63/Left, MAPSPRITE_ORB172, eas_Init|131, byte35.|

csc42 disables services/display, reads header, InitializeMapEntities,
LoadEntityMapsprites, then reenables. ClearEntities resets49 physical
slots,64 lookup bytes and walking scratch. Ordinary followers are allocated
before fixed rows, independent of force membership. Map55 is neither an
overworld map nor the no-follower ship map: F65 alone does not prepend
Caravan here. F67 can predeclare Peter7, F76 Zynk26, F82 Frayja22 and
F74/68/70 can each declare B31; multiple held rows can overwrite the same
logical mapping while leaving earlier physical actors. The actual follower
table and supplied flags govern, not an assumed one-B party.

Let n be successfully declared ordinary follower rows and p=1 when fixed
Peter must allocate, otherwise0. C/G/V/O are physical n+p+1 through n+p+4;
main is physical0. With clear76 no fixed Zynk row exists. B's setPos/actions
still attempt its alias without a local existence check. Main construction
clears d0 and calls GetAllyMapsprite for **Bowie0**; this differs from the
first-active-member return/name selection. The final sub_44404 also reads
PLAYER_TYPE/F64/RAFT_MAP: raft player type can replace main's sprite and
stand followers; a matching unlocked raft can declare physical31/lookup63.
These supplied-state effects are not a Caravan unlock by the scene. Missing/
stale aliases, empty lists, excessive followers and interruptions are not
safe allocations merely because the declarations fit.

gamescript's LEADER token at2911/2933 independently calls
`text/textfunctions_1.asm:symbol_leader` →
UpdateForceAndGetFirstBattlePartyMemberIndex → GetCombatantName. This
rebuilds lists again and names their first active byte, not necessarily the
scene actor named Bowie. Other item/defeat messages can rebuild through
LEADER too. That is a list/name side effect, not joining or making an actor
controllable. G/C speaker portraits come from their mapped sprite dialogue
properties; a narrator operand uses no actor portrait.

csc2C follow first checks the **high byte of its word operand** through
AdjustScriptPointerByCharacterAliveStatus. All selected follower IDs here
are below256, so that byte is0: Bowie HP0 skips the whole command even for
P/B/Z/V. A live Bowie permits following even if that follower ally is dead.
Then it resolves physical follower and leader and passes offsets from
table_FollowerPositions to AddFollower:2=(-24,0),1=(0,-24),3=(0,24).
These are physical coordinate offsets, not tile offsets. AddFollower copies
42 bytes of walking code, writes leader/offsets, installs it and adds a
physical follower-list entry only if not already listed. It writes no
joined/active/F76/F65 bit. The Caravan image V does not unlock/board a
vehicle or grant an inventory item. No all-actors follow wait exists here.

## Sky Orb mandatory inventory award

**Confirmed:** O's display at19 and hide at75 are physical operations.
csc2E resolves131 and HideEntity moves current/destination words to
`$7000,$7000`, changes every matching lookup byte among64 toFF and removes
its matching physical follower-list entry. No inventory mutation occurs.
It is stronger than Die's setPos63,63 and does not free/recreate the actor.

At76, `map/mapscriptengine_2.asm:csc0A_executeSubroutine` reads its long
pointer, saves a0, JSRs it, restores a0 and returns. csub_4D694 is exactly
moveq ITEM_SKY_ORB113,d0; moveq1,d1; JMP ReceiveMandatoryItem. The included
owner is `code/common/stats/iteminventory.asm` (`$4F48A..$4F6E2`), selected
by `layout/sf2-07-0x044000-0x064000.asm`; the similarly named
`items/itemfunctions_s7_0.asm` is not this layout's owner. The JMP tail
returns through the helper to the map-script call. No return-value branch
or separate award flag exists in the scene.

1. Save d1–d5/a0; store item **word** in d4 and discard-option word in d5.
   UpdateForce rebuilds joined/active/reserve lists. TARGETS_LIST contains
   every joined member in ascending ID, including dead/reserve members;
   active12 capacity, visible B/Z/V and HP do not filter this scan.
2. Read length.w, subtract1, then read a member **byte** before DBF.
   GetItemBySlotAndHeldItemsNumber(slot0) counts all four inventory words
   after masking ITEMENTRY_MASK_INDEX; NOTHING127 is empty regardless of
   other bits. Unsigned heldCount<4 chooses the first non-full member.
3. Room: d1=113; AddItem scans again for first masked-NOTHING slot,
   masks away equipped bits while retaining the permitted broken bit and
   stores the item. Then substitutions, MUSIC_ITEM, txt213, fade/input
   waits, close, and d0.w=0. The caller does not verify AddItem's d2 result.
4. All full: d0=1, but d5=1 is nonzero, so this caller enters txt214 and
   DiscardItem. Supplying d5=0 would return1 without discard; the helper
   name alone does not force it for every caller.
5. DiscardItem rebuilds/copies all joined members to GENERIC_LIST, uses
   the item-summary member selector with GIVE submenu action, and loops
   on selection -1. ITEMTYPE_UNSELLABLE requests txt37 then repeats.
   Otherwise txt44/alt_YesNoPrompt:
   only result0 accepts; No/B repeats. Equipped selected weapons/rings
   that IsItemCursed rejects request txt30 and repeat. A cursed but
   unequipped entry is not rejected by that equipped-slot test.
6. Accepted removal calls RemoveItemBySlot (compacts remaining items and
   refreshes stats). RARE removal calls AddItemToDeals, then loops again;
   it does **not** return that freed slot. Only a non-RARE accepted removal
   returns selected member/slot/old item in d0/d1/d2. Thus discarding a
   rare item may free space but still require another selection; there
   is no re-scan of newly free space in the mandatory helper at this point.
7. The helper restores the incoming113 into d1, substitutes recipient/old
   item, AddItem to that selected member, substitutes new item, MUSIC_ITEM,
   txt215/fade/input/close, then d0.w=2. AddItem's distinct failure result
   is again unchecked. It restores saved registers and returns; the scene
   attempts77–81 follows, wrapper joins Bowie, then434/534 tail can finish.

Sky Orb113's item definition is RARE|UNSELLABLE, price0, no equip flags,
range0,0, useSpell NOTHING and three NONE effects. The ORB mapsprite172
is not this item definition. The helper does not test prior possession,
deduplicate113, route this award to Caravan storage or set a possession
flag. Its discard screen can remain in a nonreturning selection loop; cancel
is not a helper failure1 for this d5=1 call. If only protected/rare choices
remain, return is not guaranteed by static source. AddItemToDeals uses
the selected item's four-bit count and dumps at the maximum; the award
does not promise preservation of every discarded item.

An empty joined list is not a safe zero-iteration scan: length0−1 enters
the body and can execute65536 reads if none appears non-full. Similar
zero-length copy behavior exists in DiscardItem. Declared normal party
state does not authorize replacing this with a safe modern contract.

Changed-state consequences are ordered source facts, not observed crashes,
saves or recovery behavior:

| Supplied condition / interruption | Boundary consequence |
| --- | --- |
|Bowie/full active party, later joined reserve has room|Reserve can receive113 even if dead; first non-full joined ID wins.|
|Inventory already contains113,534 clear|No duplicate guard; another award can be attempted.|
|534 held,113 absent|Whole after scene skipped; no repair award, though join0/completion tail still run.|
|Hide75 returns, helper selection/wait does not|Orb alias is already hidden; following and434/534 tail have not been reached.|
|AddItem mutates, message/input wait interrupted|Item may exist with534 still clear. Reentry into the after wrapper can attempt another grant; ordinary automatic reentry is not established.|
|Helper returns, later follow or wrapper/tail interrupted|Award can precede completed534. There is no rollback.|
|534 becomes held independently of the scene|Zone admission later checks534 alone, not actual113 possession or award completion.|

## Text keys, modes and input boundaries

**Confirmed:** `data/scripting/text/gamescript.txt` keyed values supply
these complete ordered tokens. Public prose is authored paraphrase; raw
game text stays ignored. N is line break, W1/W2 are the source text waits;
LEADER/NAME/ITEM/SPELL/# are substitutions. C=128, G=129, P=7, B=31, Z=26;
S=nextSingleText, Ctd=nextText. C/P/B/Z modes00; G modeC0; narratorFF.

| ID / command | Speaker / form | Authored meaning | Ordered tokens |
| --- | --- | --- | --- |
|2911 /17|C /S|Acknowledges defeat and offers something.|`["{LEADER}","{N}","{W1}"]`|
|2912 /18|ENTITY_NONE /S,FF|Narrates the Orb reveal.|`["{W1}"]`|
|2913 /20|C /Ctd|Explains Orb use for the vessel.|`["{N}","{N}","{W2}"]`|
|2914 /21|C /Ctd|Warns of future danger.|`["{N}","{W2}"]`|
|2915 /22|C /S|Begins a warning about Geshp's flowers.|`["{N}","{W1}"]`|
|2916 /24|G /S,C0|Threatens Cameela.|`["{N}","{W1}"]`|
|2917 /37|C /S|Recognizes the attacker.|`["{W1}"]`|
|2918 /38|G /S,C0|Claims the outcome was planned.|`["{N}","{W1}"]`|
|2919 /39|C /S|Utters a dying fragment.|`["{W1}"]`|
|2920 /42|B /S|Reacts to the killing.|`["{W1}"]`|
|2921 /43|G /Ctd,C0|Justifies punishing betrayal.|`["{N}","{W2}"]`|
|2922 /44|G /Ctd,C0|Invites the newly equipped party to Grans.|`["{N}","{N}","{W2}"]`|
|2923 /45|G /S,C0|Announces hostile preparations.|`["{N}","{N}","{W1}"]`|
|2924 /49|P /Ctd|Expresses anger toward Geshp.|`["{W2}"]`|
|2925 /50|P /S|Asks about the named flowers.|`["{N}","{W1}"]`|
|2926 /52|B /S|Suspects a trap.|`["{N}","{W1}"]`|
|2927 /56|Z /S,76-held branch|Identifies their ancient origin.|`["{W1}"]`|
|2928 /58|B /S,76-held branch|Requests clarification.|`["{W1}"]`|
|2929 /59|Z /S,76-held branch|Associates them with old national borders.|`["{N}","{W1}"]`|
|2930 /60|B /S,76-held branch|Asks what sort they are.|`["{N}","{W1}"]`|
|2931 /63|Z /S,76-held branch|Cannot explain further.|`["{W1}"]`|
|2932 /66|P /S|Expects to learn after reaching Grans.|`["{N}","{W1}"]`|
|2933 /67|B /S|Urges collecting the Orb and departing.|`["{LEADER}","{N}","{N}","{W1}"]`|

Both stable76 paths explicitly reset cursor2932 at64. That does not
guarantee every earlier line displayed. `mapscriptengine_2.asm` nextText/
nextSingleText skip branch consumes its operand without display or cursor
increment.
Single additionally closes portrait/text; Ctd leaves the dialogue open.
ModeFF suppresses actor portrait, while mode bits6/7 route portrait side/
mirror; sprite-derived portrait/speech and existing-window state still
govern. G's remote alias can supply a portrait while its actor stays63,63.
No mode is an inventory recipient or joined-member selector.

| Shared ID | Authored meaning | Ordered tokens |
| --- | --- | --- |
|213|Names the recipient of the new item.|`["{NAME}","{N}","{ITEM}"]`|
|214|Requires a discard because no member has room.|`["{ITEM}","{N}","{N}","{W1}"]`|
|215|Names the replacement recipient and both items.|`["{NAME}","{N}","{ITEM}","{N}","{ITEM}"]`|
|37|Refuses an unsellable discard.|`["{LEADER}","{N}","{ITEM}","{W2}"]`|
|44|Asks to confirm discarding the selected item.|`["{ITEM}","{N}"]`|
|30|Refuses removing selected cursed equipment.|`["{LEADER}","{N}","{ITEM}","{N}","{W2}"]`|
|363|Announces exhaustion before defeat recovery.|`["{LEADER}","{W1}"]`|
|274|Names the retreat spell and level.|`["{NAME}","{N}","{SPELL}","{#}"]`|
|275|Names the retreat item use.|`["{NAME}","{N}","{ITEM}"]`|

213/215 have no W token but explicitly call FadeOut_WaitForP1Input and
WaitForPlayerInput before close/return. txt44 calls alt_YesNoPrompt, then
closes and tests0. Do not infer input absence from token absence, or
mandatory completion from presence. **Inferred:** the dialogue frames
Cameela's warning and Geshp's attack as the narrative reason to take the
vessel. Natural progress, perception and final control handback are Unknown.

## Defeat, Egress and Angel Wing returns

**Confirmed:** defeat calls UpdateBattleUnlockedFlag: clear434 only if534
was already held. It clears dialogue name1, requests sad music/txt363/close,
restores **Bowie0** HP=max, halves unsigned gold, selects egress, returns
d4=-1. LEADER363 may name another first active member before recovery.
Battle4's losable upgrade branch is not34.

Admitted Egress deducts selected spell MP cost before txt274/cast flash;
Angel Wing removes selected inventory slot and refreshes stats before275.
The common tail closes text, updates unlock, unwinds the action frame,
selects egress and clr.w d4. Both bypass the terminal suffix, defeat HP/gold
recovery, after scene and JoinForce tail. Neither newly sets534 or grants
Sky Orb. Earlier costs, battle/opening/flag changes persist in reached state;
this is not an atomic rollback or a completed-retreat observation.

`battleloop/getegresspositionforbattle.asm` (`$23E50..$23EB0`) has special
cases16/17/13/31/32 and flag clears38/39, **no34 override**. Unchanged34
reads live EGRESS_MAP.b. `code/common/maps/egressinit.asm:GetSavepointForMap`
first checks399 (set at BattleLoop entry). Changed clear399 returns initial
Map3(56,3)/Down3 before raft logic. Held399 scans23 four-byte savepoints;
31 selects Moun20,8/Up1;2 Pacalon41,10/Up1;36 castle40,5/Up1. Neither55
nor75 has a row: if supplied as EGRESS_MAP, retain that map byte with
default1,1/Up1. There is no asserted natural EGRESS_MAP value here.

Held399/F64 also resets raft coordinates by selected destination. These
55/75/31/2/36 examples use the default69/42,15 row. Post-battle MainLoop
alt entry calls SwitchMap then ExplorationLoop without CheckBattle or a
d4 branch. F640 can substitute explicit3→4; other switch rows are71/F530→78
and58/F999→59, so55/75 have no substitution. Raft/global state, source map,
coordinates and facing are separate return authorities.

## Map55 reconstruction, exits and ship dispatch boundary

**Confirmed:** scene loadMapFadeIn uses csc37→csc48: black fade, Map55
tilesets, fade wait, services/display disabled, packed camera `$0207`×3,
LoadMap, display/services restored. CURRENT_MAP becomes55; CURRENT_BATTLE
remains34. LoadMapBlocksAndLayout loads base layout, flag/chest changes and
live battle crop0,0,32,32. Map55 has no flag-copy records; ordinary area
ends31,31, type255, default MUSIC_BATTLE_THEME_1. No scene command writes
the saved PARAM2–5 or posts a warp. The crop/camera/header/actor positions
are not interchangeable.

After victory returns d0=55, MainLoop alt entry SwitchMap does not change55;
ExplorationLoop clears pending event, adjusts step counter, heals, fades,
sets CURRENT_MAP55 and CURRENT_BATTLEFF, loads tilesets and **reinitializes**
entities from selected setup. It clears map-temp256–383 and sets80, reloads
map using the saved first-active-member X/Y/facing, then runs setup init
and waits for events. d4=1 does not preserve the scene or select a town branch.

A separately supplied d0.b=FF instead takes MapIndexNotProvided: retains
population, UpdateMainEntityProperties conditionally changes nonnegative
coordinate bytes, updates appearance/facing and declares raft as appropriate.
It still loads the map and runs init. That scene-retained route is **not**
unchanged Battle34 victory's explicit55 return. No source fact turns the
after-scene follow arrangement into the fresh Map55 exploration population.

| Map55 owner / interval | Complete selected declaration |
| --- | --- |
|`data/maps/mapsetups.asm`|msMap55→ms_map55, then msMapEnd; no alternate setup flag.|
|`map55/mapsetups/pointertable.asm`, `$5E22C..$5E244`|Six pointers: entities, entity events, zones, descriptions, item events, init.|
|`s1_entities.asm`, `$5E244..$5E246`|msEntitiesEnd only. Main and flag-driven ordinary followers can still be constructed; no fixed Cameela/Geshp/Caravan/Orb rows.|
|`s2_entityevents.asm`, `$5E246..$5E248`; `s4_descriptions.asm`, `$5E25E..$5E260`|RTS each.|
|`s3_zoneevents.asm`, `$5E248..$5E25E`|One7,7 row→Map55_ZoneEvent0, default RTS. Check534; clear returns, held dispatches cs_5E27C.|
|`s5_itemevents.asm`, `$5E260..$5E268`|Default item callback RTS; no local pickup/search repair for a missing Sky Orb.|
|`s6_initfunction.asm`, `$5E268..$5E27C`|Check999; clear returns; held dispatches cs_5E320 then cs_5E346. Revisit bodies remain excluded.|
|`map55/3-flag-events.asm`, `$B7906..$B7908`|endWord only; no534/Orb layout replacement.|
|`map55/6-warp-events.asm`, `$B790C..$B792E`|Four eight-byte rows below plus FFFF terminator.|

Map-relative filenames in this table begin `data/maps/entries/`.
Fresh population uses the current flags again; earlier F76 reads do not
pin its later Zynk following. Clear999 performs no selected init scene;
held999 is a separately marked revisit, not proof of natural reach, a
repair award or permission to inspect its bodies.

| Ordered row | Source X,Y | Scroll / destination | Facing |
| --- | --- | --- | --- |
|0|0,255|NoScroll; Map75 Nazca46,19|Down3|
|1|31,255|NoScroll; Map75 Nazca46,19|Down3|
|2|255,0|NoScroll; Map75 Nazca46,19|Down3|
|3|255,31|NoScroll; Map75 Nazca46,19|Down3|

No row checks434/534/76/999 or113 inventory. Source255 is a signed-negative
wildcard, not a required coordinate255. `exploration.asm:WarpIfSetAtPoint`
has an ordinary controlled-character caller requiring layout kind1000;
signed physical words divide by384, terminator is checked first and the
first matching row wins. At a corner the earlier row wins, with the same
destination. A declaration does not prove traversal/collision reach.

Producer writes event type1 before PARAM1..4 longword, then facing/padding
word and WARP_SFX. WaitForEvent prioritizes pending over A/C. ProcessMapEvent
clears stored type before dispatch; Type1_Warp consumes NoScroll0, submits/
clears sound, idles main, unwinds to MainLoop, clears d0–d4 words, reads map
and samples then-current physical X/Y by signed division. Nonnegative
destination bytes replace low bytes of those samples. Intact small state
returns75(46,19)/Down3,d4=0; unprotected payload/position changes may differ.
Stop at that outbound handoff, without re-auditing Map75 travel.

Later explicit55 enters MainLoop @Start→SwitchMap→CheckBattle. Sole matching
row34 has crop0,0,32,32 and wildcard trigger bytes255/255, gated by434.
Clear434 enters exploration regardless of534. Held434 admits34 even with
534 held: completed-match clears434 but still returns34 now. This differs
from the immediate battle-return alt entry, which skips CheckBattle.
The ordinary movement producer recognizes layout kind1400 and posts event6
before writing candidate destination-derived X/Y **words** into PARAM1/3.
ProcessMapEvent clears pending before Type6 idles main via ApplyInitActscript,
reads those words and calls RunMapSetupZoneEvent. That dispatcher reselects
the current setup and matches X/Y bytes, then invokes the chosen callback.
Map55_ZoneEvent0 at7,7 tests **only534** before its script trap; spawning or
standing at the coordinate alone does not prove this event was produced.
The underlying layout marker/reach and supplied payload remain Unknown.
No local113/434/999/follower/vehicle check occurs. **Stop at dispatch of
cs_5E27C**, before its first command. Its body, later inventory use or
subsequent flight cannot be inferred from this gate.

## Actual Content, Domain, Application and Godot consumers

**Confirmed about current repository code:** production consumes prepared
Content. [Capability status](../../remake/docs/capability-status.md) owns
runtime claims. No34 content package, running instance or observation is
supplied here; accepted initialization/activation guards remain in force.

| Actual owner | Expressible mechanism / selected boundary |
| --- | --- |
|[PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)|Pin1/2/3 rejects34. Private outcome-source requires EnemyLeaderPresent=false and defeated program End/Return only;34's empty script does not admit its nonzero leader sweep. Selected map/material/sprite/text/party/outcome and inventory bindings remain absent. Predecessor type3/equipment/commandset gaps remain.|
|[BattleOutcomeRules](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/BattleOutcomeRules.cs), [BattleActionCommitter](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleActionCommitter.cs), [BattleOutcome](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs)|Defeat priority, positioned living actors, defeated gate, pre-scene return, completion skip and join→unlock→complete tail have shapes. No raw30/32 slot/death-list/live-selector/interruption parity or selected34 drop/leader-cleanup service is established.|
|[Sf2OutcomeReturnPolicy](../../remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs), [MapPartyMembership](../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)|Rebuild/capacity and selected return policies exist. Active.First()/bound positioned actor, guarded nonzero status refresh and egress refusal with absent399/held64 or640 differ from original unchecked state. Source partial writes are not reproduced by transactional policy refusal.|
|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py)|Selected lowering creates seven programs with instruction counts15/50/95/20/10/1/1 in compiler insertion order abcs_battle34, cs_4D502, cs_4D592, cs_4D620, cs_4D654, csub_4D694, cs_4D692. Branch labels are independently compiled continuations, not a sum of one executed trace. Earliest selected unsupported operation is command23→cs_4D502 instruction31 native tintMap, conditional on admitted bindings and earlier services.|
|[ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Native program rows remain tintMap at cs_4D502[31/39], flashScreenWhite[38], flickerOnce at cs_4D592[39], helper csub_4D694[0] jmp. AnimSpeedx2 has native action0 ac_animSpeedX2; Die actions36/37/47 are native ac_soundCommand/ac_setSprite/ac_setPos. Readers map these to UnsupportedInstruction/UnsupportedEntityAction; runners refuse program-opcode/entity-action. Init, Jump, DeactivateAutoFacing and both custom/six movement streams have no native frontier. No mandatory discard/award implementation follows from compiling moveq inputs before native jmp.|
|[SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs), [ExplorationViewRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs)|Scene-map/entity rebuilds,49-slot capacity, aliases, position/hide/follow/fade/view/gesture shapes exist. Settled-black/window/palette, traversal/entry-area and required actor guards can fail earlier for missing/changed state. Follow lacks original high-byte/Bowie-HP skip; setPos/facing lack original ally-HP skip. Scene load versus explicit fresh entry must be bound deliberately.|
|[Map preparation](../../src/sf2tool/remake_exploration_maps.py), [ExplorationDefinition](../../remake/src/Sf2.Remake.Application/Content/Scenarios/ExplorationDefinition.cs), [MapEventDispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs)|Setup/init/zone/warp data have shapes; selected eventMaps determine actual source binding. Map55's534 branch is expressible, but this slice neither prepares its ship target nor lowers/executes its body. No-scroll/specific destination and flag state do not establish natural access. Current loaded-definition selection differs from original live setup reselection.|
|[ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs), [BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs)|Existing board/sprite/text/effect/gesture services do not prove original tint/flash/death/audio/input timing or Map55 layer255 material binding. No composed34 outcome, reward, exploration or ship control claim.|

Smallest future integration decisions are selected content/party/material
bindings, an explicit34 leader-cleanup/drop policy, admitted original
status/equipment return semantics, and missing palette/entity-action/mandatory
inventory consumers. Identifying those boundaries completes this research
scope without authorizing engine work or an acquisition queue.

## Verification, preserved failures and Unknowns

Use the owning worktree, current ignored selections and bounded recipe:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1104/correction.py <configured-source-disasm-root> --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The preserved original review.py audit joins complete selected tables/wrappers, terminal/count/cleanup/
healing/return/escape consumers,83 commands/56 inline bytes/10-byte helper/
48-byte list, all23 scene and nine shared text-token rows, independent76
reads/aliases/LEADER/follow/HP differences, mandatory item/discard branches,
fresh versus retained entry, Map55 setup/init/four exits/534 zone and actual
lowering/native/content/engine guards. Check changed534/76/party/inventory/
selector/return-mode conditions; no claim is established solely by a fixed
scenario. Apply ADR0004's adversarial checklist to this documentation scope.
No new parser/fixture/schema or tests of verification machinery are added.

The correction-only audit checks the actual list-byte/branch/call/register
dataflow and counter fields, with distinct and changed valid dead/first-ally
IDs. It checks seven attribution sentences, exact sentence-only changes in
the six older owners and unchanged original-candidate content outside the
declared new-owner correction/scope/verification edits. Do not rerun the
complete original audit for this prose correction.

Check exact eight-path ownership, predecessor navigation only, cached diff,
UTF8/no BOM/LF, links/anchors/tables, short paraphrases and no raw/token-stripped
game lines, private/generated paths or assets. Record clean-head planner
and actual exact-head scoped CI separately. Planner suggestions are not
executed passes. Normal/full Python/.NET/Godot/H3/H4, old comparisons and
runtime services are excluded by this documentation policy.

Preserve #1100's original6df1f6c7/direct0.377s/CI39511030878 public-full-text
P2 and corrected51ac0d0f author0.239s/main-gate0.242s/CI39511617705; the
correction changed publication, not game semantics. Preserve #1088's49→48
AI-clear correction/untouched checked word, #1096's withdrawn accepted-Tristan
attribution, #1098 diagnostics, #1102 completed author0.406s/main-gate0.402s/
CI39513028023, and twelve completed old comparison failures. No old result
is overwritten, described as interrupted or made an automatic rerun queue.
The first local1104 probe's no-drop assumption failed; the selected table
instead has both Wish Staff and random Iron Ball rows, now explicitly joined.
This was an author inspection error, not changed original evidence.
Preserve original1104 head d84ecb32/tree8d3518ff, author/main-gate direct
PASS0.290s and CI39514888171 as completed results: they missed the incorrect
defeat-counter prose attribution. The focused correction distinguishes the
listed dead ally from FIRST_ALLY; original behavior and engine scope do not change.

**Unknown:** natural admission/terminal path/reach of exits or7,7; actual
joined/active/follower flags, party lists/tails, checked word, HP/status,
inventory/equipment/drop bits/deals/RNG, selector/egress/parameters/mappings;
empty/overfull/stale/interleaved state, interrupted helper and reload behavior;
completed native/text/input/DMA/sprite/fade/audio/view services, visible
presentation/timing, usable control and saved persistence. Ship body and
F999 revisit bodies remain outside the boundary, not silently complete.

Freeze clean committed/pushed Draft PR with exact base/head/tree, scoped
checks/CI, preserved failures/Unknowns and ignored handoff; set Project
Review / Verify and send the assigned main-gate completion notification,
then stop writer/processes. Independent acceptance/merge belongs to
main-gate. No next Issue, merge, cleanup or archive is authorized here.
