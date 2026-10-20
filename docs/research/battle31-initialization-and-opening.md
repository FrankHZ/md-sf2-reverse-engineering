# Battle31 initialization and opening

Evidence date: 2026-10-20. **Confirmed (static):** Map2/Battle31 has separate
temporary opening actors, twelve formation declarations and fifteen battle
enemy declarations. Fresh entry stops at BattleLoop @Start before
ActivateEnemies; F88 entry stops at the individual-turn handoff.
**Unknown:** natural admission, supplied state, completed presentation,
usable control and persistence.

## Agreement and provenance

Standard rigor for [Issue #1076](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1076)
under [epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
deliver source-backed opening, encounter inventory and actual consumer gaps.
Accepted base is `c5168008f5593ff2266264ec0d97ff11e37d0332`.
Owned writes are this document and one navigation link in the
[Pacalon/Frayja admission owner](./pacalon-return-frayja-and-battle31-admission.md).
No shared-path writes or stacked dependencies; UI692 is unrelated.
Activation, AI, turn generation/execution, combat/outcomes, later town
content, runtime acquisition, new extraction/schema/fixture, engine,
design-guide and translation changes are excluded.

Reuse [Battle30 initialization](./battle30-initialization-and-opening.md),
[Battle29 initialization](./battle29-initialization-and-opening.md),
[Battle26 roster consumers](./battle26-initialization-and-roster.md),
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[spritesets](./battle-spriteset-data.md), [global data](./battle-global-data.md),
[maps](./common-maps.md) and [scripting](./common-scripting.md).
Apply their rules to the actual31 selectors and rows below.

Paths below are relative to `disasm/` in clean read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Existing provenance binds USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
this slice does not rebuild or newly observe it. `sf2const.asm`,
`sf2enums.asm`, `sf2macros.asm`, `sf2mapmacros.asm` and
`sf2cutscenemacros.asm` own constants/encoding. Fnnn means flag nnn;
indices are zero-based and address intervals half-open.
Short `cutscenes/` and `battlefunctions/` paths are under
`code/gameflow/battle/`; `map/` and `text/` paths are under
`code/common/scripting/`.

Minimum evidence is complete selected source, existing fixtures/consumers
and the unchanged18,285,979-byte map-init/script pair with accepted0.083s
provenance. Linear bounded reads/joins are budgeted below one minute/512MiB;
small ignored receipts stay under `local/issue1076/`. No generation,
acquisition or binary asset decoding is needed. Reassess contradictions
or material supporting-tool growth before expanding.

## Entry, latch and suspended-state lifetimes

**Confirmed:** the predecessor's explicit Map2 request preserves sampled X
and replaces Y's low byte with1, facing Down. MainLoop's SwitchMap leaves2
unchanged. CheckBattle row31 is `(2,0,0,48,32,255,255)`: held431 admits31;
held531 clears431 but still admits31 on that invocation. No additional
530/811/82/join22/active54 test applies. MainLoop overwrites d1.w with31
and calls j_BattleLoop with map2 in d0. Entry coordinates do not seed the
formation, and wildcard triggers do not establish containment or travel.
For the intact castle-south request, MainLoop receives d1.w=sampled X,
d2.w=sampled Y with only its low byte replaced by1, d3.w=Down3 and d4.w=0.
The d1 overwrite discards that X argument; d2/d3 are not the scene or
formation positions. Retained high coordinate bytes and supplied mutations
remain premises, rather than an assumed literal entry `(X,1)`.

`code/gameflow/battle/battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`):

1. Clear PLAYER_TYPE.b and set399, then check88.
2. Clear88: clear seconds, save incoming d0/d1 **words**, assign their low
   bytes to CURRENT_MAP/CURRENT_BATTLE, install base VInts and execute before.
3. Restore those words and reassign both current bytes; clear all region
   flags90–105, independently of the four local region declarations.
4. HealLivingAndImmortalAllies, InitializeAllAlliesBattlePositions,
   InitializeAllEnemiesBattlePositions, ClearAiMemory, in that order.
5. Clear d0.w, LoadBattle for combatant0, ExecuteBattleStartCutscene,
   reach @Start. Stop **before ActivateEnemies**.

Both hook tables have48 relative **word** slots. In
`data/battles/cutscenes/beforebattlecutscenes.asm`, slot31/byte62 at `$47AC6`
is relative to `$47A88`, targeting bbcs_31 `$4C994`. In
`battlestartcutscenes.asm`, slot31 at `$47B6A` is relative to `$47B2C`,
targeting ms_Empty (`code/common/scripting/map/ms_empty.asm`, `$47A4E`,
sole word `$FFFF`). The start body has no scene commands, but is not a
wrapper no-op.

`cutscenes/beforebattlecutscenesstart.asm` checks current battle+450=481
and skips if held, without setting it. `battlestartcutscenesstart.asm`
checks the same range and sets the selected flag **before** dispatch.
Dispatch saves d0/a0 and wrappers preserve d1; ExecuteMapScript preserves
its interpreter registers. Empty dispatch still resets text-skip and takes
the common terminator tail, conditionally waits for the view if a dialogue
window exists, and clears VIEW_SCROLLING_SPEED. Held481 skips both bodies
but still permits fresh reset/heal/build/load. F481 proves neither completed
before nor completed start services. Interrupted before may leave481 clear;
interrupted start may leave it held before the empty script returns.

Each hook rereads live CURRENT_BATTLE for dispatch after its flag check.
Roster, neutral, terrain, area and music consumers also use their own reads.
Fresh word restoration repairs current bytes after before; it does not
undo scene/cached-list/crop mutations. No frozen encounter snapshot exists.

Held88 instead restores SAVED_SECONDS_COUNTER, clears88/AI memory/view
target, calls LoadBattle **without clearing d0**, and jumps directly to
@IndividualTurns_Loop. The following branch to @Start is unreachable.
There is no fresh assignment, before/start, heal, region reset or roster
construction. Flag traps/ClearAiMemory preserve d0; intact admitted d0=2
therefore chooses **combatant2** for the view, versus fresh0/Bowie.
Current selectors, crop, combatant and turn state remain supplied inputs.
Stop at the individual-turn handoff; a next turn byteFF can later return
to @Start, without proving that a new round was already generated.

## Temporary population and actual identities

**Confirmed:** `data/battles/entries/battle31/cs_beforebattle.asm` spans
`$4C994..$4CBE2`. bbcs_31 ends at ce_4CB92:96 top-level records, including
six sleeps and terminator,474 top-level encoded bytes. Six inline
movement blocks use24 bytes; the custom action block uses12 bytes, for
510 script bytes. ce_4CB92 is80 bytes: six-byte main header, nine eight-byte
fixed records, two-byte end marker. It is separate from spriteset31.

| Record / logical ID | Declared X,Y / facing | Sprite / initial action |
| --- | --- | --- |
|Main /0|21,4 /Down|Bowie's actual ally sprite /eas_Idle|
|Fixed0 /7|20,3 /Down|ALLY_PETER /eas_Init|
|Fixed1 /22|21,3 /Down|ALLY_FRAYJA /eas_Init|
|Fixed2 /128|21,8 /Up|GESHP159 /eas_Init|
|Fixed3 /129|21,11 /Up|CAMEELA156 /eas_Init|
|Fixed4 /130|63,63 /Left|FLAME1=171 /eas_Init|
|Fixed5 /131|63,63 /Up|DEATH_MONK141 /eas_Init|
|Fixed6 /132|63,63 /Up|DARK_BISHOP145 /eas_Init|
|Fixed7 /133|63,63 /Up|ORC_LORD86 /eas_Init|
|Fixed8 /134|63,63 /Up|HARPY_QUEEN111 /eas_Init|

csc42 reads the main header **words**, clears/rebuilds map entities and
mappings, loads mapsprites and restores display/VInts. `mapfunctions.asm`
allocates ordinary enabled followers first, then the fixed records, then
physical main0. Map2 is neither overworld nor follower-excluded; F65 does
not select the Caravan overworld list. Reuse the complete
[ordinary follower owner](./battle26-return-through-pacalon-entry.md#conditional-map02-population-and-first-eventcontrol-read).
F67 can preallocate Peter7; F82 can preallocate Frayja22. For each fixed
ally, nonzero existing lookup suppresses a duplicate allocation and consumes
its action longword; it does not check joined or active-party flags.

Let F be successful ordinary follower allocations and P/R be1 when the
fixed Peter/Frayja record allocates, else0. Scene128–134 map lookup32–38
to physical `F+P+R+1` through `F+P+R+7`. With no followers, Peter1,
Frayja2 and NPCs3–9; with only82, Frayja1, Peter2 and NPCs again3–9.
Neither example supplies31. F74/F70 can declare Astral at logical31;
F68 supplies Oddler, with later enabled aliases overwriting31. Duplicate
follower aliases still allocate physical actors; the later lookup wins.
Missing31's zero lookup can address main0. There is no generic safe
missing-actor or population-capacity check in DeclareNewEntity.
F64/RAFT_MAP can additionally declare the separate physical raft31 at
lookup63 through sub_44404; that is not logical FOLLOWER_B31/lookup31.

Fresh entry cleared PLAYER_TYPE before scene construction. Fixed ally
sprites resolve class; with live CURRENT_BATTLE31, GetAllyMapsprite bypasses
its outside-battle dead-ally blue-flame branch. HP still affects later
setPos/setFacing consumers. Initial eas_Init sets speed32,32, zero
acceleration factors, flags, orientation/size and sprite, then Idle;
it neither heals nor initializes ally stats.

No join22/JoinForce/JoinBattleParty or item grant occurs in bbcs_31.
Frayja's scene presence/F82 does not establish membership22/active54.
LoadBattle later rebuilds physical entities from combatants, without a
Battle31 neutral group. An unjoined Frayja therefore has no automatic
neutral battle deployment. Scene Geshp/Cameela/flame do not persist as
battle enemies. Scene Death Monk131 and battle Master Monk135 at19,18
are **different source identities**, despite the shared position; do not
infer enemy types from narrative/sprite resemblance.

## Complete opening and keyed dialogue

**Confirmed:** the following phases cover every top-level record and all
inline payloads. W means requested action completion by Idle-pointer wait;
it is not measured motion completion. Text IDs below advance from2811 only
when displayed without debug text-skip.

| Top-level indices | Complete ordered work |
| --- | --- |
|0–9|Cursor2811; loadMapFadeIn2 at camera16,2; load ce_4CB92; Init/W Bowie; Init/W Peter; Peter20,3 Down; Init/W31;31 at22,3 Down; Init/W Frayja; Frayja21,3 Down.|
|10–16|MOSAIC_OUT128 then129; install130 eas_AnimSpeedx2 without wait; music BATTLE_THEME_1; fadeInB; cameraSpeed `$30`; text2811 from128/C0.|
|17–29|Camera16,5; MOSAIC_IN128;2812 Peter,2813 Geshp; camera16,2;31 Down1/W;2814–2820 with exact speakers/controls below.|
|30–38|31 Jump/W;2821;31 Up1/W; camera16,0; face31 Left;2822; Frayja Up1/W; flashScreenWhite2;130 at21,1 Left.|
|39–47|Sleep30; Frayja Jump/W; disable Frayja auto-facing/W; Frayja Down1/W;2823; camera16,6; face128 Down;2824–2825.|
|48–57|MOSAIC_IN129;2826–2829;128 Down1/W;2830–2831; stopEntity128; Sleep40.|
|58–64|Custom128/W: ac_setSpeed48,48; ac_jump eas_Idle; ac_end;128 Jump/W twice; disable auto-facing/W;128 Up1/W; startEntity128;2832.|
|65–71|2833–2835 Cameela; MOSAIC_OUT129;2836–2837 Geshp;2838 Peter.|
|72–79|Sleep40; shiver128; Sleep40; face128 Up;2839–2840; camera16,13; Sleep40.|
|80–88|Set131 at19,18,132 at21,19,133 at22,18,134 at23,15, all Up; Jump131/132/133 without wait then Jump134/W; Sleep30.|
|89–95|Camera16,6;2841 Geshp,2842 Peter,2843–2844 Geshp; MOSAIC_OUT128; csc_end.|

With distinct unchanged actors and all necessary movement completed,
Bowie stays21,4, Peter20,3,31 returns22,3 facing Left, Frayja returns21,3
facing Up after backing down with auto-facing disabled, Geshp returns21,8
facing Up, Cameela stays21,11 and flame130 is21,1 Left. Actors131–134 take
the four explicit positions above. These are conditional scene endpoints,
not the new battle formation or proof of completion. Mosaic-out changes
sprite-transfer presentation, not actor coordinates, membership or deletion.

All34 source comments join exact gamescript keys `0AFB..0B1C` /2811–2844.
S means nextSingleText, T nextText; G=128 with modifierC0, C=129/00,
P=Peter7/00, F=FOLLOWER_B31/00. The table paraphrases meaning and retains
terminal controls; no standalone translation is added.

| Text / macro / speaker | Meaning / terminal control |
| --- | --- |
|2811 /S /G|Vicar NAME22 has finally left the castle /W1|
|2812 /S /P|Recognizes Geshp /W1|
|2813 /S /G|Acknowledges being remembered /W1|
|2814 /S /F|Asks his purpose /W1|
|2815 /T /G|Announces plans for the party /W2|
|2816 /S /G|Pleased they left the castle /W1|
|2817 /S /F|Asks why /W1|
|2818 /T /G|Waiting for NAME22 /W2|
|2819 /T /G|Recalls NAME22 locking Moun's gate /W2|
|2820 /S /G|Demands access for his soldiers to Parmecia /W1|
|2821 /S /F|Identifies a trap /W2|
|2822 /S /F|Tells NAME22 to run /W1|
|2823 /S /G|Says escape is impossible /W1|
|2824 /S /G|Calls Cameela repeatedly /W1|
|2825 /S /P|Recalls meeting her in Pangoat Valley /W1|
|2826 /S /C|Answers the summons /W1|
|2827 /T /G|Acknowledges her arrival /W2|
|2828 /S /G|Orders her to obtain the key /W1|
|2829 /S /C|Calls Geshp devious /W1|
|2830 /S /G|Renews the demand /W1|
|2831 /S /C|Questions being assigned /W1|
|2832 /S /G|Calls her name /W1|
|2833 /T /C|Refuses to help him /W2|
|2834 /T /C|Tells him to get the key himself and offers her army /W2|
|2835 /S /C|Takes leave /W1|
|2836 /T /G|Insults her /W2|
|2837 /S /G|Calls after her /W1|
|2838 /S /P|Observes their division /W1|
|2839 /T /G|Rejects Cameela's help /W2|
|2840 /S /G|Orders devils to kill LEADER /W1|
|2841 /S /G|Bids LEADER farewell /W1|
|2842 /S /P|Asks whether he is leaving /W1|
|2843 /T /G|Claims intellectual rather than physical strength /W2|
|2844 /S /G|Promises to return after their defeat /W1|

2821 is a **single-text macro ending W2**, not W1. Macro window cleanup
and string wait control are separate. There is no choice, key-transfer,
gate-opening, damage, new battle unlock or direct event producer here.
**Inferred:** Frayja is threatened over Moun's key, Cameela refuses Geshp
and lends her soldiers, and Geshp withdraws. Those dialogue claims do not
independently establish a key inventory, opened gate or battle outcome.

### Actual script, text and effect consumers

**Confirmed:** csc00/csc02 mask the packed speaker to its logical low byte.
`sf2cutscenemacros.asm` emits modifier byte then selector byte: Geshp's
`$C080` supplies portrait-side bit15 and mirror bit14 to csc1D, plus logical128;
it is not entity192. `data/spritedialogproperties.asm` resolves live159 to
portrait45/bleep6 and156 to44/bleep3. Peter and31 resolve actual current
sprites; Astral209 would use31/bleep6, but is not created unconditionally.
Already-open portrait bypasses csc1D replacement, while speech resolves
again. NextText retains windows; nextSingleText closes both then Sleep10.

ExecuteMapScript clears text-skip initially; debug/P2 Start can later latch
it. Skipped text consumes speaker operands but neither displays/increments
cursor nor executes single-text cleanup. The six csWait sleep words encode
`$8000|ticks`; interpreter masks to a byte and skips those sleeps when text
skip is held. Action waits, mosaics and flash have separate consumers.
Successful unskipped text ends cursor2845; W1/W2 and waits do not prove
fresh button presses or usable control.

NAME22 resolves GetCombatantName22 without joining22. Reached LEADER in
2840/2841 goes through `text/textfunctions_1.asm:symbol_leader` to
UpdateForceAndGetFirstBattlePartyMemberIndex, **j_UpdateForce**, then the
first cached battle member's name. It rebuilds joined/active/reserve
prefixes/counts from current flags, without writing membership flags.
No-text-skip alone does not prove the token was reached: interruption before
token parsing can prevent this effect. These indirect mutations precede the
later explicit fresh placement rebuild; do not label the scene stateless.

In `map/mapscriptengine_1.asm`, csc19 setPos and csc23 setFacing read the
actual selector **byte** for AdjustScriptPointerByCharacterAliveStatus.
Dead ordinary allies skip those commands (four or two operand bytes);
31 and enemy-bit IDs bypass HP checks. Thus Frayja's setPos may skip even
though her scene record exists. csc1B/1C start/stop read the **word**128
and also bypass as enemy-bit128; they write animation counter0/FF, not
movement suspension or deletion. csc14/15, csc2D, text, mosaic and shiver
do not share a universal alive guard. Action installs may still target a
dead/missing/aliased actor. Healing later is not retroactive.

GetEntityAddressFromCharacter masks to a byte, subtracts96 for enemy lookup
and shifts the unsigned physical lookup by5. Zero can alias main; FF can
address ENTITY_DATA+$1FE0. No missing sentinel makes arbitrary supplied
state safe. csc14/15 install the selected action pointer/timer; wait variants
busy-wait until eas_Idle. csc2D constructs action scratch and waits as
requested. The custom block's ac_end is a delimiter, not an extra executed
motion after its unconditional Idle jump. ac_setSpeed48,48 persists for
later Geshp movement; eas_Jump toggles auto-facing OFF/ON around relative
0,-1/wait5/0,1/wait5/0,0. eas_DeactivateAutoFacing disables it again.
Flame's eas_AnimSpeedx2 enables animation-speed doubling then Idle.

cameraSpeed consumes a **word `$30`=48**, stored in VIEW_SCROLLING_SPEED;
camerafunctions.asm uses nonzero override before parallax scaling. This is
not a duration or measured camera speed. setCamDest detaches following,
sets requested tile destination and waits for view-scroll end. The common
terminator conditionally waits for a dialogue-related view, clears the
override and restores registers; it does not roll back scene state.

csc22 MOSAIC_OUT6 starts mask1; MOSAIC_IN7 starts `$7FFF`. Both select the
special mosaic branch (not the23-iteration crop path), with DBF counter15:
16 LoadMapsprite/sub_45E10/DmaMapsprite iterations, each with three explicit
WaitForVInt calls. Out shifts left/adds1; in shifts right. This is48 explicit
wait calls per mosaic, not proof of hardware duration or visible disappearance.
No scene record is removed. csc41 reads flash operand2, shifts right3 to0
and therefore calls LaunchFading **once**, with d0=$F/d1=1/d2=FLASH_QUICKLY_2,
then DuplicatePalettes. It is not two flashes or two frames. LaunchFading
waits for its service flag and another VInt. shiver uses its own sprite,
animation-counter save/restore and Sleep loop, separate from the surrounding
Sleep40 words. No screenshot, timing or completed effect is observed.

## Full spriteset and fresh construction

**Confirmed:** spriteset pointer31 selects BattleSpriteset31,
`data/battles/spritesets/spriteset31.asm`, `$1B5782..$1B58FC`:378 bytes =
4-byte header +27 twelve-byte combatant rows +4 twelve-byte regions +one
two-byte point. Header is12 allies/15 enemies/4 regions/1 point. The accepted
aggregate fixture's `rowContentTracked=false` does not prove exact rows.

All ally declarations use HEALER1, NOTHING, primary NONE/15, secondary
NONE/15, filler0 and STARTING. Identity0–11 declares placement slots,
not a fixed twelve-person party.

| Slot | X,Y | Slot | X,Y |
| --- | --- | --- | --- |
|0|21,4|6|20,4|
|1|21,3|7|19,3|
|2|22,3|8|21,2|
|3|20,3|9|23,4|
|4|22,4|10|20,2|
|5|23,3|11|22,2|

HealLivingAndImmortalAllies scans30 allies: living allies and Peter7/Lemon28
even if dead regain max HP/MP, retain only STUN|POISON|CURSE=`$0007`, and
update stats. Dead Frayja22 remains dead. InitializeAllAlliesBattlePositions
sets all30 X=-1, calls UpdateForce and walks sorted current battle membership
for at most12 member iterations. A dead member consumes an iteration but
does not advance the formation pointer; the next living member uses that
slot. Surplus active members leave the battle party. Unplaced Y/other state
is retained; empty/inconsistent membership is not established safe. Neither
scene22 nor F82 admits a new battle member.

All enemy rows have item NOTHING, filler0 and STARTING. Secondary order
is NONE except7 and12. Combatant ID=128+ordinal; FOLLOW_ENEMY operands
are local ordinals, not scene logical IDs.

| Ordinal / ID | Type | X,Y | Commandset | Primary order / region | Secondary order / region |
| --- | --- | --- | --- | --- | --- |
|0 /128|DEVIL_SOLDIER23|23,27|LEADER|MOVE_TO\|0 /2|NONE /15|
|1 /129|DARK_BISHOP81|21,19|ATTACKER3|FOLLOW_ENEMY\|0 /1|NONE /15|
|2 /130|NECROMANCER73|15,20|HEALER3|FOLLOW_ENEMY\|0 /1|NONE /15|
|3 /131|HARPY_QUEEN47|23,26|SENTRY|NONE /15|NONE /15|
|4 /132|HARPY_QUEEN47|32,23|ATTACKER1|FOLLOW_TARGET\|0 /2|NONE /15|
|5 /133|DARK_KNIGHT21|25,20|SENTRY|NONE /15|NONE /15|
|6 /134|ORC_LORD22|22,18|ATTACKER1|FOLLOW_TARGET\|0 /1|NONE /15|
|7 /135|MASTER_MONK82|19,18|HEALER4|FOLLOW_ENEMY\|6 /1|FOLLOW_ENEMY\|0 /15|
|8 /136|HARPY_QUEEN47|10,16|ATTACKER1|FOLLOW_TARGET\|0 /1|NONE /15|
|9 /137|DARK_KNIGHT21|17,11|ATTACKER1|NONE /15|NONE /15|
|10 /138|ORC_LORD22|29,11|ATTACKER1|NONE /15|NONE /15|
|11 /139|MASTER_MONK82|31,10|HEALER3|FOLLOW_ENEMY\|10 /15|NONE /15|
|12 /140|ORC_LORD22|37,4|HEALER3|FOLLOW_ENEMY\|13 /0|FOLLOW_ENEMY\|13 /3|
|13 /141|DARK_KNIGHT21|39,2|ATTACKER2|NONE /0|NONE /3|
|14 /142|HARPY_QUEEN47|23,15|SENTRY|NONE /15|NONE /15|

InitializeAllEnemiesBattlePositions scans128–159. Battle32/Jaro's enemy15
exception does not apply to31. All fifteen declared rows have spawn nibble0
below HIDDEN2 and receive InitializeEnemyStats without a starting-occupancy
test. There is **no hidden enemy row** here. Absent143–159 get activation0,
max/current HP0 and X=-1, plus trigger **2F from the still-unoffset first
enemy row**. This partial clear does not set Y/type/item/orders or replace
the whole combatant definition; retained fields are not newly initialized.

InitializeEnemyStats copies56-byte enemy definition; restores current HP/MP;
sets X/Y; packs movetype upper/commandset lower nibble; stores enemy index,
trigger nibbles, move-order bytes and items. NOTHING returns before AddItem,
so definition equipment survives. Activation takes definition `$F000` plus
the rotated spawn word masked `$0FFF`; definition BIT4/BIT5 are dropped.
Fresh1/7/11 use `$3000`, all other declared rows `$2000` (including LEADER0).
Active/secondary/neutral/spawn bits are clear. This is initialization,
not an activation/AI outcome. ClearAiMemory independently writes48 last-target
bytesFF and48 memory bytes0; it does not construct turns.

Trigger bytes in row order:
`2F 1F 1F FF 2F FF 1F 1F 1F FF FF FF 03 03 FF`.
Move words:
`40FF 80FF 80FF FFFF 00FF FFFF 00FF 8680 00FF FFFF FFFF 8AFF 8D8D FFFF FFFF`.
NONE=$FF, MOVE_TO=$40, FOLLOW_ENEMY=$80, FOLLOW_TARGET=0.
Region15 is not a fifth local polygon; FOLLOW_TARGET0 is not a fixed point0.
Row12's two region declarations and row7's two orders remain distinct even
where some later consumer might select one; that evaluation is excluded.

### Definitions, equipment and geometric declarations

**Confirmed:** `data/stats/enemies/enemydefs.asm` supplies seven types,
all ENHANCED spell power, initialStatus NONE and four item/spell slots.
Listed item is followed by three NOTHING; spells by NOTHING padding.
These are base/raw definition values, before difficulty/equipment effects.

| Type | Level | Max HP/MP | Base ATT/DEF/AGI/MOV | Movetype | Item | Spells |
| --- | --- | --- | --- | --- | --- | --- |
|Devil Soldier23|29|60/16|45/48/43/6|REGULAR|GREAT_AXE\|EQUIPPED|BOOST\|LV2|
|Dark Bishop81|23|39/38|28/38/27/5|HEALER|FLAIL\|EQUIPPED|AURA, BLAST\|LV3|
|Necromancer73|30|47/42|45/44/51/6|MAGE|FLAIL\|EQUIPPED|FREEZE\|LV3|
|Harpy Queen47|27|53/20|65/39/43/7|FLYING|NOTHING|FREEZE\|LV2|
|Dark Knight21|27|50/0|36/48/40/7|CENTAUR|CHROME_LANCE\|EQUIPPED|none|
|Orc Lord22|27|46/13|40/44/37/5|REGULAR|HEAVY_LANCE\|EQUIPPED|DISPEL|
|Master Monk82|28|47/39|22/45/31/6|HEALER|BRASS_KNUCKLES\|EQUIPPED|HEAL\|LV4, DISPEL|

Resistance: Soldier STATUS_MINOR; Necromancer LIGHTNING_MINOR|STATUS_IMMUNITY;
Harpy WIND_WEAKNESS|FIRE_WEAKNESS; remaining four0. Prowess: Soldier/Orc
CRITICAL150_1IN8; Bishop/Necromancer CRITICAL150_1IN32; Knight
CRITICAL150_1IN4; Harpy CRITICAL_SILENCE; Monk CRITICAL125_1IN4.
All DOUBLE_1IN32 except Monk1IN8; all COUNTER_1IN16 except Bishop1IN32.
No prowess/spell execution is claimed.

The repeated-BEQ difficulty routine applies floor(baseATT*5/4) in every
difficulty, repeats that on SUPER, then updates stats. Item definitions28,
39,77,79,90 are Brass Knuckles/Great Axe/Heavy Lance/Chrome Lance/Flail:
range1 weapons, useSpell NOTHING, INCREASE_ATT39/28/23/31/19 then two NONE
effects. Current-stat updating applies status/equipped effects without a
class-equippability test in this consumer; its enemy branch clears the ally
curse test. Thus Orc12's HEALER3 and Necromancer2's HEALER3 are actual row
commandsets, not claims about their available spells or final behavior.
Battle31 is absent from all eleven random-battle upgrade entries.

Each region has12 encoded bytes: type4, unknown0, four ordered vertices,
two trailing zero bytes. One point is two bytes. No membership/activation
evaluation occurs within this slice.

| Region | Ordered vertices |
| --- | --- |
|0|(29,0), (29,31), (47,31), (47,0)|
|1|(0,7), (0,31), (47,31), (47,7)|
|2|(0,14), (0,31), (47,31), (47,14)|
|3|(0,11), (0,31), (47,31), (47,11)|

Point0 is23,27 for MOVE_TO0. These declarations do not prescribe when
enemies activate, choose orders or act.

## LoadBattle and selected Map2 resources

**Confirmed:** `battlefunctions/loadBattle.asm` selects live CURRENT_MAP,
fades, detaches view, loads tilesets, waits/clears VInts, then calls
PositionBattleEntities. This clears49 physical records/mappings; scans30
allies then32 enemies by nonnegative X/Y, excluding neutral enemies,
without its own HP test. Position uses `(byte & $3F)*384`, Down,
eas_Standing and aerial layer1 for FLYING/HOVERING. Enemy mapsprites for
the seven types above are87/145/137/111/85/86/146, all below special240.
With N placed allies, fresh rows0–14 use physical N..N+14 (aerial3/4/8/14);
N=12 gives12–26. Absent143–159 skip, lookupFF. No31 neutral table entry
exists: clear531 scans without a match, held531 bypasses that scan.
Neither Astral nor unjoined Frayja carries through as a neutral scene actor.

LoadBattle restores incoming view combatant (fresh0, intact F88 map2
operand2), resolves its lookup into VIEW_TARGET_ENTITY and chosen X/Y.
Negative lookup falls back to physical0 for position reads and masks `$3F`;
it does not create a missing Bowie. LoadMap receives **d0=-1/d1=2** under
unchanged current-map selection: full explicit Map2 load with target-based
camera, not the negative-d1 CURRENT branch. Sprite loading, base VInts,
terrain, music and fade follow. No exploration Map2 setup/init, interaction,
warp or battle-admission scan is invoked by this resource load.

Map02 header `$968D8` selects palette10, tilesets `[1,90,50,91,58]`.
`data/maps/entries.asm:Map02` binds Map02s0_Blocks/0-blocks.bin,
Map02s1_Layout/1-layout.bin, area/flag/step/roof/warp/chest/other-item
tables, and animation pointer `$FFFFFFFF` (no animation table).
Reuse the [accepted Map2/F530 resources](./battle26-return-through-pacalon-entry.md)
and predecessor; these metadata references do not supply deployed binaries.

Single30-byte area at `$96906..$96924`, then endWord to `$96926`:
inclusive bounds0,0..47,31; foreground origin0,32; background0,0;
both parallax256,256, both autoscroll0,0; layer0; MUSIC_TOWN8.
Original LoadMap selects the containing area from explicit camera/target,
with first-record fallback if none contains it. Scene camera16,2 and ordinary
fresh Bowie21,4 select this area. Supplied out-of-range targets still fall
back in original, a distinct modern-consumer gap below.
LoadMapArea's battle override sets main bounds0,0 through
`(47*384,31*384)=(18048,11904)`, retaining the other selected fields.
Layer0 makes plane B main; foreground0,32 is the other packed plane,
not a second area or autoscroll restriction.

LoadMapBlocksAndLayout rebuilds base blocks/layout, applies twenty F530
puddle copies (1x1 from49,2 to the accepted twenty destinations) and
opened-chest edits, then crops `(0,0,48,32)` via CopyMapBlocks into64-wide
working rows:1536 words/3072 bytes, not compact48-wide storage.
F530 can differ independently of431/481/531. This load applies flag-copy
data without running exploration callbacks; no collision/path equality or
natural traversal follows from matching metadata. The scene and battle
loads can sample different live flags.

`data/battles/terrainentries.asm` slot31 selects BattleTerrain31's incbin
`data/battles/entries/battle31/terrain.bin`; LoadBattleTerrainData
decompresses into BATTLE_TERRAIN_ARRAY. Logical48x48 terrain is distinct
from48x32 crop and64-wide layout. PlayMapMusic converts battle MUSIC_TOWN8
to BATTLE_THEME_3=33, after the scene explicitly requested THEME_1=34.
Global custom-background31 is TOWN_0; enemy-switch has no31 entry (its
table has30 bytes), leader-present31=-1, and31 is not halved-EXP.
Leader metadata, row0's LEADER and priority `$2000` are distinct facts;
none proves a victory condition or AI decision at this stopping point.

## Changed-state and interruption boundaries

| Changed premise | Result to retain |
| --- | --- |
|88 held/clear|Supplied combatants/turns and view2 versus fresh heal/build/view0; both rebuild physical/display state.|
|481 held or changes between hooks|Bodies skip/check separately; fresh construction continues; start writes latch before empty dispatch.|
|431/531/530/811/82/22/54|Admission/completion, rainfall, offer latch, follower, member and active flag are separate; no new join or key transfer.|
|Follower flags/aliases/raft/population change|Peter/Frayja duplicate suppression and NPC physical positions differ;31 can be absent/another sprite; no arbitrary capacity guarantee.|
|Dead/changed ally HP before heal|Byte-selector setPos/setFacing can skip; other action/text/effect consumers differ; dead22 is not revived by common heal.|
|Membership versus cached lists|Reached LEADER2840/2841 rebuilds caches; later placement rebuilds again; neither creates member22.|
|Dead/fewer/surplus/empty battle members|Sorted membership and living-slot advancement control placement;12 declarations are not a named party or arbitrary-state safety proof.|
|Current selectors/crop/flags change between services|Fresh restores current bytes after before only; later hook/roster/terrain/map/music reads may differ.|
|Absent143–159 versus declared0–14|Partial clear retains old Y/type/item/orders; no hidden-row construction is inferred.|
|Portrait already open, skip or held input|Portrait reuse, cursor, csWait and token-side effects can differ independently of movement/mosaic/flash.|
|Lookup/view/target outside area|Main alias/negative fallback and original first-area fallback can differ from modern refusals; no actor repair.|
|Interrupt at action/text/effect/load/latch|Only reached prefix survives; scene, F481, declared soldiers and offered army do not prove completed presentation, control or persistence.|

## Actual preparation and consumer gaps

**Confirmed about current repository:** research representations, prepared
Content and executing Application/Godot are separate. The
[capability owner](../../remake/docs/capability-status.md) controls actual
runtime scope. No Map2/Battle31 package or execution is delivered here.

| Surface / actual consumer | Existing shape and selected boundary |
| --- | --- |
|Research fixtures and retained pair|Global/spriteset fixtures retain summary/address/provenance facts; script corpus has96 bbcs_31 top-level records. Exact inline actions/list/full roster require the pinned source join. Aggregate equality is not initialized-state or playable-content acceptance.|
|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py)|Actual read-only selected lowering produces248 instructions. Scene-map/population, movement/custom actions, waits, stop/start animation, mosaics/shiver, text/C0, positions/facings and camera destinations have shapes. Native-call cameraSpeed at lowered18 is the first explicit refusal, before2811; flashScreenWhite at94 is another. Raw compiler source-index43 for that flash includes inline records; top-level corpus index is37. Successful lowering does not execute it.|
|[ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|native-call becomes UnsupportedInstruction and reached program-opcode refusal. No selected world/resources or native speed/one-call screen-flash binding is supplied. Existing white fade/FlashWhite is not evidence of consuming this command.|
|[SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs), [ExplorationPortraitRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationPortraitRunner.cs)|Prepared population/ordinary followers and action/text flags have consumers. Selected full cast, class/HP/aliases/capacity and interrupted semantics remain unbound/unassessed. Compiler position/face/animation forms do not carry original alive-helper checks; do not equate their shapes with exact changed-state parity.|
|[ExplorationTextRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationTextRunner.cs), [MapPartyMembership](../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)|LEADER with admitted PartyFlags already rebuilds lists before choosing the first active name and publishes PartyLists. Selected2840/2841 token-time execution and fidelity remain unassessed, and cameraSpeed refusal precedes them.|
|[BattleEncounterDefinition](../../remake/src/Sf2.Remake.Application/Content/Scenarios/BattleEncounterDefinition.cs), [PrivateBattleEncounterReader.Pin](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs)|Placements/expressions/regions/point/terrain/scene shapes exist; private encounter Pin admits1/2/3, explicitly rejects31. Missing31 definitions/live-party/start/policy and resource bindings remain. Scene Frayja is not a supplied deployment.|
|[BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Intro/before/load/start and membership reconstruction mechanisms exist. Correct431/531/481 route and changed-party rules are unbound for31; NewBattle's F88 path rejects suspended-battle-entry. Save/suspend remains Unsupported.|
|[ExplorationViewRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs), [BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs)|Map2's layer0/background0/256 parallax/zero autoscroll passes the field profile; foreground0,32 is permitted for its nonmain plane. Fixed1:1 profile passes the board plane-motion guard. Metadata support is separate from absent tiles/blocks/layout/sprites/portraits/audio/terrain bindings, reached rendering and precise fidelity. Nonzero ViewSpeed also has an explicit field-view-speed guard; native cameraSpeed already blocks this selected stream.|
|[OriginalMapTraversal.SelectActiveArea](../../remake/src/Sf2.Remake.Domain/Maps/OriginalMapTraversal.cs)|Modern absent area returns null, leading to field-view-area or battlefield-map-area-unavailable; original falls back to first record. Board selects by crop origin, original LoadBattle by view target. Ordinary0,0/21,4 match; changed-state fallback remains a separate gap.|
|[ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs)|Godot has effect/sprite/window consumers; selected mosaic/flash/audio/text/material and camera-speed rendering/timing, motion completion and input/control are unassessed. No deployment or Godot observation follows from the static shapes.|

Production consumes prepared Content, not research JSON/disassembly on
demand. Missing resources/bindings, explicit semantic refusal and unassessed
fidelity are different gaps. Supported shapes do not establish runnable31.

## Reproduction and remaining boundaries

Load current ignored selections in each launching process. Reuse the pair
unchanged; the bounded direct recipe checks pinned sources, complete scene/
inline/list/text joins, all27 rows/four regions/point, definitions/equipment,
hook/init/map/text/action consumers and actual248-instruction lowering:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1076/review.py <configured-source-disasm-root>
git diff --check
```

Review every prose phase and changed-state row, UTF-8/LF, links/anchors,
private boundary, exact two owned paths and predecessor navigation-only
change. On clean committed HEAD run
`uv run sf2 verify plan --base origin/main --head HEAD`; record actual
exact-head scoped CI in the Issue/PR handoff. Documentation scope skips
blanket normal/full Python/.NET/Godot/H3/H4 and tests of review programs.
No parser/schema/extractor change creates new mutation-test obligations;
ADR0004's adversarial branch/width/alias/provenance review still applies.

Retain #1074's original0.256s audit/CI39480126515 missed LEADER→UpdateForce
effect and corrected independent0.138s/CI39481060627 result, #1072 receipts,
#1070 N==V, #1068 HP and #1066 projection corrections, prior diagnostics and
Unknown chain. Twelve completed old comparison failures remain evidence,
not a repair/rerun queue. No runtime/process launch is needed for this slice.

**Unknown:** natural admission, actual map/register words/crop/flags/HP/
membership/equipment/difficulty/turn state; aliases/population/capacity;
working layout and interleavings; completed text/token/action/camera/fade/
mosaic/flash/audio and sprite presentation; original timing, usable control
and persistence. Activation/AI/turn/combat/outcomes and later town/gate
content are outside this boundary. These Unknowns are not an acquisition queue.
