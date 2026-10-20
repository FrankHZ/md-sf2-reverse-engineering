# Battle33 initialization, Moun opening and Frayja party admission

Evidence date: 2026-10-20. **Confirmed (static):** admitted Map31/Battle33
runs a temporary gate/confrontation scene, joins Frayja22 and conditionally
replaces an active member, then rebuilds battle actors from combatants.
F831 and follower82 are separate from joined22, active54 and intro483.
Fresh entry stops before ActivateEnemies; F88 stops at the individual-turn
handoff. **Unknown:** natural admission, actual caller state, completed
services, presentation, control and persistence. No playable Battle33 claim.

## Agreement and evidence route

Standard rigor for [Issue #1088](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1088)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect the complete opening, forced-party seam, encounter declarations and
initialization for later content/engine design. Accepted base is
`d15c2f335458c8d8fe4a56353b5dddb51ac8df48`, including the
[Battle32 outcomes/Moun admission owner](./battle32-outcomes-and-moun-entry.md).
Exactly two owned paths: this document and one navigation-only link there.
No shared-index write or stacked dependency; UI692 is independent.
Activation, region geometry/effects, first turns, AI/actions/combat/outcomes,
full Moun exploration/underground/revisit, runtime, asset extraction/decoding,
new maintained tools/fixtures/schemas, engine, UX and translation are deferred.

Reuse [Battle32 initialization](./battle32-initialization-and-opening.md),
[Pacalon/Frayja offer](./pacalon-return-frayja-and-battle31-admission.md),
[spritesets](./battle-spriteset-data.md), [global data](./battle-global-data.md),
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[cutscene data](./battle-cutscene-data.md), [common maps](./common-maps.md)
and [scripting](./common-scripting.md). Their shared derivations remain owners;
selected differences below are directly checked against source and consumers.

Source paths are relative to `disasm/` in clean read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Existing USA retail ROM identity is SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM/H1/runtime equality is claimed. `sf2const.asm`/`sf2enums.asm`
and the cutscene/map macros own constants and widths. Fnnn denotes flag nnn;
ordinals are zero-based and extents half-open. Short `battleloop/` and
`cutscenes/` mean children of `code/gameflow/battle/`; `map/`, `entity/`
and `text/` mean `code/common/scripting/`.

Minimum evidence is complete selected source/table/fixture/consumer joins,
all commands/inline actions/list/native/texts/placement rows and changed-state
review. Reuse the unchanged18,285,979-byte init/script pair with accepted0.083s
provenance in `local/issue1005/pair.txt`. Linear bounded reads/checks budget
less than one minute/512MiB per run, with small ignored receipts under
`local/issue1088/`; no new acquisition or comparison platform. Reassess
contradictions or material supporting machinery before expansion. ADR0004's
complete checklist applies proportionally under documentation-only policy.

## Incoming words, intro latch and two initialization routes

**Confirmed:** the predecessor's CheckBattle row33 is `31,0,0,32,38,255,255`
at `$7B1D`. Held433 writes crop0,0,32,38 and admits33; held533 additionally
clears433 but still admits on this invocation. CheckBattle does not assign
CURRENT_BATTLE. MainLoop replaces d1.w with33 and calls BattleLoop with
resolved map31 in d0.w. For the two unchanged non-scroll requests, d2.w is
the sampled main Y with destination low byte12 or37, d3.w is facing0 or1,
and d4.w=0. Sampled X in d1 is discarded. Replacing a low byte preserves
the sampled word's high byte; these are not scene or formation coordinates.

`code/gameflow/battle/battleloop_1.asm:BattleLoop`, `$23A84..$23BB4`:

1. Clear PLAYER_TYPE.b; set399; test88.
2. Clear88: clear seconds, save incoming d0/d1 **words**, assign their low
   bytes to CURRENT_MAP/CURRENT_BATTLE, set base VInts, execute before.
3. Restore words and reassign current bytes; clear every region flag90–105,
   then HealLivingAndImmortalAllies, InitializeAllAlliesBattlePositions,
   InitializeAllEnemiesBattlePositions and ClearAiMemory, in that order.
4. Clear d0.w; LoadBattle for combatant0; ExecuteBattleStartCutscene; reach
   @Start. This document stops **before ActivateEnemies**.

The before/start tables each contain48 relative words. Slot33/byte66 is
`$47ACA` relative to `$47A88` → bbcs_33 `$4CF0C`, and `$47B6E` relative to
`$47B2C` → ms_Empty. Before checks450+CURRENT_BATTLE=483 and skips when held,
without setting it. Start tests483 and **sets it before dispatching** the
sole FFFF word. Empty dispatch still clears text-skip, conditionally waits
for view when dialogue is open and clears scrolling speed. Held483 skips
both bodies but not fresh healing/placement/load. Interrupted before can
leave483 clear; interrupted start can leave it held. F483 proves no service
completion, gate state, Frayja membership or usable input.

Wrappers reread CURRENT_BATTLE for flag/table selection. Fresh restoration
repairs its two current bytes after before; it does not roll back reached
flags, party, layout, crop or entity changes. Later selectors are live reads.

Held88 restores saved seconds, clears88, clears AI memory and view target,
then calls LoadBattle **without clearing d0**, and branches directly to
@IndividualTurns_Loop. Its following @Start branch is unreachable. No fresh
map/battle assignment, before, heal, region reset, placement or start occurs.
Intact incoming d0.w=31 is therefore **combatant31**, not fresh0. The lookup
consumer treats it as nonnegative and reads alias31 (FOLLOWER_B), not Bowie's
alias. PositionBattleEntities clears lookups and rebuilds combatants; ally
combatants are only0–29, so absent another reached writer alias31 remains0.
LoadBattle uses the returned byte for view selection; negative lookup falls
back to physical0 for coordinate reads, otherwise masks3F. Supplied current
map/battle/crop/roster/turn buffer still control loading. Stop at that direct
individual-turn handoff without reading/executing its turn or activation.

## Complete opening scene and temporary actors

**Confirmed:** `data/battles/entries/battle33/cs_beforebattle.asm` spans
`$4CF0C..$4D09A`. bbcs_33 has58 top-level records/268 encoded bytes; ten
inline streams add48 bytes, reaching ce_4D048 at `$4D048`. The48-byte list
is a six-byte main header, five eight-byte records and two-byte terminator;
it ends at csub_4D078. The native helper occupies34 bytes through `$4D09A`.
Counts distinguish corpus top-level records, inline bytes and native code.

| Record / logical identity | X,Y / facing | Sprite / action |
| --- | --- | --- |
|Main /0|26,37 /Left|Actual Bowie ally sprite /eas_Idle|
|Fixed0 /7|27,38 /Left|ALLY_PETER /eas_Init|
|Fixed1 /22|27,36 /Left|ALLY_FRAYJA /eas_Init|
|Fixed2 /128|10,10 /Down|SHAMAN147 /eas_Init|
|Fixed3 /129|9,10 /Down|DEVIL_SOLDIER87 /eas_Init|
|Fixed4 /130|11,10 /Down|DEVIL_SOLDIER87 /eas_Init|

csc42 disables services/display, InitializeMapEntities and loads mapsprites,
then reenables them. ClearEntities resets49 physical slots,64 lookup bytes
and walking scratch. Ordinary followers allocate before fixed rows, without
joined/active membership tests. Map31 is not overworld: F65 supplies no
Caravan prefix here. F67 can allocate Peter7 and F82 Frayja22; duplicate
fixed ally rows then skip allocation. Other fixed rows get128/129/130 aliases
and are scene actors, not initialized enemy combatants or membership.

FOLLOWER_B31 can be Astral under74, Oddler under68, later Astral under70,
with the last lookup write winning; earlier overwritten physical allocations
remain. Missing31 lookup0 aliases main0. FOLLOWER_A30 can similarly be king74,
old man77 or later king83; this scene does not act on30. Logical31 is not the
physical raft/lookup63 case. Let n be follower allocations and p/f be1 when
Peter/Frayja fixed rows allocate: scene128 is physical `n+p+f+1`, with129/130
next. Arbitrary flag/capacity/action-pointer safety is unproven; original
follower pointer tables are finite. Class/HP affects actual ally appearance;
the outside-battle blue-flame path is not selected while in battle33.

| Index | Complete command / boundary |
| --- | --- |
|0–2|textCursor2866; loadMapFadeIn31 camera22,29; loadMapEntities ce_4D048.|
|3–9|await Init on Bowie, Peter,31,Frayja; set Peter27,38/Left,31 at27,37/Left,Frayja27,36/Left.|
|10–14|setBlocks16,8 size1,1→32,1; setBlocks3,39 size1,1→16,8; play BATTLE_THEME_3; fadeInB; wait30.|
|15–20|face Bowie/Peter/31 Up; Frayja actionsWait Up1/end; face Frayja Down; single2866.|
|21–23|Frayja actionsWait Up1/end; narration single2867 modifierFF/ENTITY_NONE; wait30.|
|24–29|quake2; setBlocks1,39 size2,2→26,32; wait20; quake0; camera entity Bowie; wait50.|
|30|Bowie actionsWait Up1/end.|
|31–34|Frayja asynchronous Up6,Left1,Up1/end; Bowie asynchronous Up7/end;31 asynchronous Up6,Left2,Up1/end; Peter actionsWait Up8/end.|
|35–40|wait30; cameraSpeed48; setCamDest5,8 and wait;128 actionsWait Down1/end;128 continued2868 then single2869.|
|41–46|Frayja continued2870 then single2871; setCamDest21,25 and wait; Frayja actionsWait Down1/end; Frayja single2872; join22.|
|47–50|31 actionsWait Up1/end; face31 Right/Bowie Left;31 single2873.|
|51–54|joinBatParty22; executeSubroutine csub_4D078; textCursor2875;31 single2875.|
|55–57|set831; clear82; csc_end.|

All ten streams end with8080. Eight contain one move; two contain three
moves. More precisely their move counts in order are1,1,1,3,1,3,1,1,1,1:
fourteen two-byte moves plus ten two-byte end markers =48 bytes. csc2D
compiles each move into an eight-byte action and appends a six-byte jump to
eas_Idle, clears flagsA bits5/6 using9F and installs the slot timer. Wait
forms join that actor's Idle; the three asynchronous forms do not join all
three actors when Peter's wait returns. csc15 Init waits similarly; Init
resets acceleration, speed32/32, collision/auto-facing/orientation/size24/
immersed fields and refreshes sprite, not HP, class or membership.

setPos/csc19 and setFacing/csc23 use their own byte-ID HP predicate for
allies0–29: HP.w=0 skips, high-bit nonzero proceeds;31 bypasses it. Init and
csc2D actions have no such skip. With distinct intact aliases, applicable
HP checks passing and **all** movements returning, commanded endpoints
are Bowie26,29/Left, Peter27,30/Up,31 at25,29/Right, Frayja26,28/Down,
128 at10,11/Down;129/130 remain9,10/11,10. These projections do not establish
collision/path legality, concurrency completion or the subsequent formation.
Alias0 can instead serialize actions onto main. Interruptions retain only
the reached prefix. Source waits are requests, not measured elapsed times.

## Keyed texts, join ceremony and actual replacement rule

**Confirmed:** source comments are cross-checked by numeric keys in
`data/scripting/text/gamescript.txt`. N=next line; W1/W2 retain distinct text
wait tokens. Normal cursor progression is2866–2873, reset to2875; join446
and native2874 do not advance this cursor.

| Key / scene index | Speaker / mode / modifier | Paraphrase / complete tokens |
| --- | --- | --- |
|2866 /20|Frayja22 /single /00|Warns before opening Moun; N,N,W1.|
|2867 /22|none /single /FF|Narrates Frayja using gate key; NAME22,N,W1.|
|2868 /39|128 /continued /00|Hostile welcome; W2.|
|2869 /40|128 /single /00|Threatens the same fate as Moun's people; N,N,W1.|
|2870 /41|22 /continued /00|Believes he is too late and responsible; N,W2.|
|2871 /42|22 /single /00|Self-reproach; W1.|
|2872 /45|22 /single /00|Asks leader to let him fight; LEADER,N,N,W1.|
|446 /join46|handler text /current class22|Join announcement; NAME,CLASS,N; no encoded W1/W2.|
|2873 /50|31 /single /00|Recommends Frayja; LEADER,NAME22,N,W1.|
|2874 /native52|handler text /conditional|Announces selected member's return to Caravan; NAME,N,W1.|
|2875 /54|31 /single /00|Agrees with Frayja; LEADER,NAME22,N,W1.|

csc00/csc02 consume packed modifier/speaker and advance cursor after display;
single closes portrait/text and Sleep10, continued retains windows. The
FFFF narration operand bypasses portrait lookup and clears current speech SFX;
ENTITY_NONE is not actor255. Ordinary speaker portraits/bleeps derive from
actual physical mapsprites (SHAMAN147 has no property row: default portrait
FFFF/bleep6; Frayja's actual class and31's actual follower alias remain live),
not enemy-definition
identity. NAME22 resolves named combatant22. LEADER invokes
symbol_leader → UpdateForceAndGetFirstBattlePartyMemberIndex → UpdateForce,
then reads the first active byte without count/HP validation; it need not be
Bowie or alive. Each reached LEADER token refreshes cached prefixes/counts.

ExecuteMapScript clears text-skip on entry; debug P2 Start can latch it.
Skipped nextText/nextSingleText consumes the operand without display or
cursor increment, and skipped negative waits do not Sleep. Explicit cursor
reset2875 still occurs. join/csc08 and native DisplayText do not use this
dialogue skip handler. Skipping a LEADER token also skips its list rebuild.

join22/csc08 waits for view, plays JOIN (bit15 is clear), calls JoinForce22,
GetClass, sets DIALOGUE_NAME_INDEX_1.w=22 and index2=current class, displays446,
then FadeOut_WaitForP1Input/close/Sleep10. JoinForce sets joinedF22, rebuilds
lists, and sets activeF54 only when that snapshot's active count<12; it does
not clear an existing54 at capacity, heal, reposition, or rebuild again after
setting54. No yes/no choice, key inventory removal or key ownership test is
present. The key line is narration; physical follower82 did not itself join22.

`map/mapscriptengine_1.asm:csc51_joinBattleParty`, reached at51, must be read
with `code/common/stats/battleparty.asm`, `gameflags.asm` and jump aliases:

1. Write DIALOGUE_NAME_INDEX_1.w=FFFF; read target **word**22. IsInBattleParty
   checks flag32+target **low byte**. CheckFlag AND is nonzero when held;
   the actual BNE returns immediately for active54. The source comment about
   CCR is not the branch rule. This return does not rebuild cached lists.
2. Otherwise save d0.w in d6, UpdateForce, point a0 at BATTLE_PARTY_MEMBERS,
   load active count **word**N, subtract2 into d7.w, and enter the body with
   **no capacity, zero-count or minimum-count guard**.
3. Read `(a0).b` into d0, GetCurrentHp and test **word**HP. Zero branches
   directly to replacement. Nonzero increments a0 by1, then DBF d7 repeats.
4. At replacement, read the current byte, write that member word to
   DIALOGUE_NAME_INDEX_1; LeaveBattleParty clears its active flag and sets
   combatant X=-1. Copy d6.b to d0 and JoinBattleParty sets54. No joined-flag
   clear, HP/Y change, physical removal, list rebuild or capacity test here.

For N>=2, the loop tests the first N-1 entries for HP0. It selects the first
dead among them, else the last entry **without testing that last HP**.
At N=12 this means test indexes0–10, else choose11. UpdateForce orders joined
and active prefixes by member ID0–29, so “last” means greatest joined active
ID in this rebuilt list, not last recruited or last scene actor. Bowie can
be replaced if he is the first tested dead member. Nonzero high-bit HP is
treated as living. For target22, d0's high byte is0 throughout these byte
member reads; helper preservation does not invent a wider member ID.

| Changed state at this seam | Actual consequence |
| --- | --- |
|22 already active, including at/above capacity|WriteFFFF and return; no replacement,2874 skipped.|
|Count<12 before ordinary join46, target previously inactive|JoinForce sets54; csc51 returns as active. Cached lists can still omit22 if intervening LEADER2873 was skipped.|
|Count>=12, target inactive after join46|No JoinForce auto-admission; csc51 rebuilds then replaces by the above rule even if N>12.|
|Direct inactive-target seam with2<=N<12|Still replaces; csc51 has no “only if full” branch.|
|N=1 or0 at inactive-target seam|Body first reads entry0; DBF startsFFFF orFFFE. If all reached HP values are nonzero and helpers return, it permits65,536 or65,535 HP-test iterations before fallthrough. No bounded empty/short-list behavior is established; stale/out-of-list bytes can be consumed.|
|Dead selected member / dead22|Selection precedes fresh healing. No revive here; another ordinary dead ally stays dead later, whereas Peter/Lemon are immortal-heal exceptions.|
|Interrupted/skip scene|Only reached flags/list/action writes exist; text skip does not skip csc51 or the native helper.|

csub_4D078 compares DIALOGUE_NAME_INDEX_1 **word** to-1; equality returns.
Otherwise CloseDialogueWindow, DisplayText2874, CloseDialogueWindow, RTS.
Thus2874 describes the selected removed member, with no extra membership
operation, player selection or scene-cursor increment. Final2875's LEADER
normally rebuilds the now changed lists; if skipped it leaves the earlier
cached snapshot. These caches do not control the later fresh placement's own
UpdateForce. Finally set831 then clear82; neither immediately removes a
physical Frayja actor. A later entity reconstruction consumes cleared82.
F831 does not replace active54/joined22/intro483 and is not a completion receipt.
**Inferred:** the dialogue presents recruitment and insistence on fighting;
actual assent, completed ceremony and natural party state remain Unknown.

## Map31 layout copies, view and reconstruction

**Confirmed:** `data/maps/entries.asm:Map31` selects palette0, tilesets
0/89/50/53/63, Map31s0_Blocks and Map31s1_Layout (binary pointer metadata only).
The animation pointer isFFFFFFFF; no selected animation payload. The sole
area (`map31/2-areas.asm`, `$AB63E..$AB65E` including terminator) declares
main0,0..31,38, foreground32,0, background0,0, both parallax256/256,
both autoscroll0, **layer0**, MUSIC_TOWN8. It is not Map74's layer254 profile.

loadMapFadeIn1 sets black-fade state then falls into csc48: explicit31
tilesets, wait fade, disable services, pack camera22,29 as `$1600|29`,
multiply by3 and LoadMap. This is a camera origin, not actor placement or
pending map event. LoadMap writes CURRENT_MAP31, not CURRENT_BATTLE; it loads
base blocks/layout, applies flag copies/chests, then battle crop. Camera22,29
selects the sole area before the live battle dimensions override its main
bounds to local0,0..31,37 = ends11904,14208 internal units. Foreground32,0
remains an independent plane origin. LoadBattle later selects its area by
actual view-target position, falling back to the first descriptor if none.

`map31/3-flag-events.asm`, `$AB65E..$AB670`, has two ordered eight-byte rows
thenFFFF: held833 copies source3,39 size1,1→16,8; then held831 copies
source1,39 size2,2→26,32. These are applied **before crop**, each based on its
own live flag read. Crop0,0 size32,38 copies1,216 words/2,432 bytes onto
origin0,0 in the64-wide working layout. It does not clear the remainder,
including source rows39 and foreground columns32 onward. Crop/source/layout
and camera/actor coordinate spaces remain distinct; here crop origin0 makes
battle `(x,y)` equal the base layout `(x,y)` for copied cells only. Area end38
and crop height38 are different declarations, not a walkability proof.

csc34 packs three words `(sourceX<<8|sourceY,sizeX<<8|sizeY,destX<<8|destY)`
and calls CopyMapBlocks: forward word copies in the same working buffer at
FF0000,64 words/128 bytes per row, then requests both plane update bits.
There is no flag write, resource decode or isolated immutable source buffer.
Scene copies10/11 preserve the current16,8 cell at32,1, then replace16,8
from3,39; copy25 replaces the four gate cells at26..27,32..33 from1..2,39..40.
They run even if831/833 already held. The preserved cell's value depends on
the prior833 copy/chest/layout state. Neither values nor rendered gate/roof
meaning are decoded here. setQuake2/0 write the unadorned amplitude word;
no high-bit ramp loop is selected. Music12 requests BATTLE_THEME_3=33;
cameraSpeed36 writes VIEW_SCROLLING_SPEED.w=48. setCameraEntity28 reads
Bowie's actual lookup without HP test; setCamDest37/43 detach following,
set destinations5,8 and21,25 and wait for view. Scene end clears view speed.

After scene return, LoadBattle performs a **new full map load** with
d0=-1 and nonnegative d1=31 (not negative-d1 current-map reload). Base layout
replaces all transient copies, including32,1. Reached831 makes the gate
flag-copy recur on that load;833 independently controls16,8. If before was
skipped by483, its copies/join/831/82 writes never occurred. Interrupted831
or changed flags/crop need not reproduce the normal projection. Neither
scene nor native helper posts a pending warp or runs Moun town initialization.

LoadBattle's suffix waits VInt, loads mapsprites, installs base VInts, loads
terrain, requests music and fade-in. Terrain table slot33/45 points to
BattleTerrain33, incbin `data/battles/entries/battle33/terrain.bin`, decompressed
to BATTLE_TERRAIN_ARRAY. PlayMapMusic changes selected TOWN8 to BATTLE_THEME_3
33 while in battle. Custom background33 is TOWN_1; the30-row enemy-background
switch has no33. No selected33 neutral or random-upgrade table entry exists.
No audible/VDP-visible completion, terrain decoding or public asset binding
is established by these selections.

## Complete encounter declarations and fresh combatants

**Confirmed:** `data/battles/spritesets/entries.asm` slot33 points to
BattleSpriteset33. `spriteset33.asm`, `$1B5A6A..$1B5BCC`, is354 bytes:
four-byte header12 allies/16 enemies/1 region/1 point;28 twelve-byte rows,
one twelve-byte region and a two-byte point. Every row declares placement
item NOTHING, spawn count0 and STARTING0. All allies have HEALER1,
orders NONE/NONE and triggers15/15. Ally numbers below are **sorted active
list placement ordinals**, not fixed member identities.

| Ally ordinal | X,Y | Ally ordinal | X,Y | Ally ordinal | X,Y |
| --- | --- | --- | --- | --- | --- |
|0|28,28|4|27,30|8|25,30|
|1|28,29|5|26,30|9|29,30|
|2|27,29|6|27,28|10|25,29|
|3|28,30|7|26,29|11|26,28|

| Enemy ordinal / combatant | Definition | X,Y | AI | Primary order / trigger | Secondary / trigger |
| --- | --- | --- | --- | --- | --- |
|0 /128|SHAMAN|10,11|LEADER|MOVE_TO\|0 /0|NONE /15|
|1 /129|DEVIL_SOLDIER|9,10|ATTACKER2|FOLLOW_TARGET\|0 /0|NONE /15|
|2 /130|DEVIL_SOLDIER|11,10|ATTACKER2|FOLLOW_TARGET\|0 /0|NONE /15|
|3 /131|MUD_MAN|10,17|ATTACKER3|FOLLOW_ENEMY\|0 /0|NONE /15|
|4 /132|MUD_MAN|7,19|ATTACKER3|FOLLOW_ENEMY\|0 /0|NONE /15|
|5 /133|CERBERUS|5,18|ATTACKER3|FOLLOW_ENEMY\|3 /0|NONE /15|
|6 /134|DEVIL_SOLDIER|18,26|ATTACKER1|NONE /15|NONE /15|
|7 /135|NECROMANCER_0|16,31|ATTACKER3|FOLLOW_ENEMY\|8 /15|NONE /15|
|8 /136|BOW_MASTER|16,32|ATTACKER1|NONE /15|NONE /15|
|9 /137|BOW_MASTER|14,14|SENTRY|NONE /15|NONE /15|
|10 /138|CERBERUS|13,15|SENTRY|NONE /15|NONE /15|
|11 /139|PEGASUS_KNIGHT|14,15|SENTRY|NONE /15|NONE /15|
|12 /140|PEGASUS_KNIGHT|19,14|ATTACKER1|NONE /15|NONE /15|
|13 /141|NECROMANCER|21,16|ATTACKER3|FOLLOW_ENEMY\|14 /15|NONE /15|
|14 /142|BOW_MASTER|22,16|ATTACKER3|FOLLOW_ENEMY\|12 /15|NONE /15|
|15 /143|CERBERUS|12,22|ATTACKER1|FOLLOW_TARGET\|0 /0|NONE /15|

Region0 declares type4, unused byte0, points `(0,0),(0,38),(18,38),(18,0)`,
two trailing zero bytes; point0 is10,11. These are complete declarations.
Their row38 boundary is retained verbatim; polygon inclusion, activation,
AI legality or movement are outside this slice.

Selected `data/stats/enemies/enemydefs.asm` and `enemymapsprites.asm` joins:
stats below are `(level,maxHP,maxMP,baseATT,DEF,AGI,MOV)` before difficulty/
equipment/status refresh. All have ENHANCED spell power, initialStatus NONE,
four item slots and four spell slots; unspecified slots are NOTHING.

| Definition / index / mapsprite | Stats | Move type | Equipped item / spells | Resistance |
| --- | --- | --- | --- | --- |
|SHAMAN83 /147|32,56,46,45,42,40,5|HEALER|WISH_STAFF /AURA2,BLAST3|0|
|DEVIL_SOLDIER23 /87|29,60,16,45,48,43,6|REGULAR|GREAT_AXE /BOOST2|STATUS_MINOR|
|MUD_MAN25 /89|31,55,0,76,51,41,4|REGULAR|none /none|LIGHTNING_MINOR,ICE_MAJOR,FIRE_WEAKNESS,STATUS_IMMUNITY|
|CERBERUS24 /88|30,51,0,70,44,45,6|STEALTH|none /FLAME|ICE_WEAKNESS,FIRE_MAJOR|
|NECROMANCER_0 101 /137|30,47,42,45,44,51,6|MAGE|MAGE_STAFF /FREEZE3|LIGHTNING_MINOR,STATUS_IMMUNITY|
|BOW_MASTER63 /127|30,46,0,50,53,40,5|ARCHER|ASSAULT_SHELL /none|0|
|PEGASUS_KNIGHT48 /112|30,65,0,40,37,47,7|FLYING|CHROME_LANCE /none|STATUS_MINOR|
|NECROMANCER73 /137|30,47,42,45,44,51,6|MAGE|FLAIL /FREEZE3|LIGHTNING_MINOR,STATUS_IMMUNITY|

Prowess: SHAMAN has CRITICAL_MAGIC_DRAIN,DOUBLE_1IN32,COUNTER_1IN32;
Devil Soldier/Pegasus Knight have CRITICAL150_1IN8,DOUBLE_1IN32,COUNTER_1IN16;
Mud Man has CRITICAL150_1IN8,DOUBLE_1IN16,COUNTER_1IN16; Cerberus has
CRITICAL150_1IN32,DOUBLE_1IN16,COUNTER_1IN16; both necromancers have
CRITICAL150_1IN32,DOUBLE_1IN32,COUNTER_1IN16; Bow Master has
CRITICAL150_1IN32,DOUBLE_1IN32,COUNTER_1IN32. Priority is3 for SHAMAN,
2 otherwise. Definition AI extras are BIT5 for Devil/Mud/Cerberus, BIT4
for Bow/Pegasus/NECROMANCER73, none for SHAMAN/NECROMANCER_0. Initialization
retains only the upper priority nibble and overwrites the lower bits from
the placement initialization word, yielding3000/2000 here before activation.

NECROMANCER_0 is **definition101**, not73: same base stats/spells/resistance/
prowess/mapsprite, but unknownByte0 versus73, priority-only versus BIT4,
and equipped Mage Staff93 versus Flail90. Itemdefs gives Mage Staff ATT+27,
WEAPON/RARE/BREAKABLE, BLAZE2 use; Flail ATT+19, WEAPON, no use spell.
Wish Staff94 is ATT+26, WEAPON/RARE/BREAKABLE, ATTACK use. Both staffs and
Flail have range1; Great Axe39 +28/range1, Assault Shell49 +25/range2–3,
Chrome Lance79 +31/range1, all WEAPON/no use spell. Placement NOTHING
**preserves copied definition inventory**, not an empty-inventory claim.

Fresh healing visits all30 allies without membership filtering: Peter7 and
Lemon28 even at HP0, other living allies to maxHP/MP, other dead allies stay
dead. Only STUN/POISON/CURSE are retained before refresh. Replacement chose
against earlier HP, so this heal does not undo LeaveBattleParty or select
another member. Ally initialization resets all30 X=-1, UpdateForce rebuilds
ID-sorted prefixes, then considers up to12 list entries; dead entries consume
an iteration without consuming a placement row. It clears active flags/X
for remaining excess entries. No zero-list guard protects the initial stale
byte read; empty/tail state is not modeled as a safe empty party. Frayja22
gets his resulting **sorted** slot if active, alive and reached, not a fixed
scene endpoint/last placement. Y and unrelated ally fields are not cleared
by the initial X reset.

Enemy initialization visits32 slots128–159. Battle32's Jaro143 exception
requires BATTLE_TO_MOUN=32 and does **not** apply to33. All sixteen declared
STARTING rows receive56-byte definitions (14 longwords), HP/MP maxima, coordinates,
move type/AI command, selected enemy index, packed trigger nibbles and move
orders; NOTHING item early-return retains inventory. Trigger bytes are0F
for rows0–5/15 andFF otherwise; move orders are the primary byte shifted8
OR secondary (MOVE_TO0=40, FOLLOW_TARGET0=00, FOLLOW_ENEMYk=80|k, NONE=FF).
No selected33 random-upgrade entry changes the definition. The difficulty
helper's two consecutive BEQs mean every difficulty applies floor(5*ATT/4),
SUPER applies it again, before UpdateCombatantStats/equipment. BaseATT45
therefore becomes56 (SUPER70) before the distinct staff/flail bonuses.

Absent144–159 take the hidden branch with d1=0: activation0,max/currentHP0,
X=-1; no Y/enemy-index/inventory refresh. Their trigger bytes are read from
the first enemy-row pointer,0/15, not sixteen absent row records. ClearAiMemory
clears49 memory entries, not combatant state. PositionBattleEntities then
clears physical/lookups again and packs placed allies followed by enemies
in ID order, testing signed X/Y bytes and enemy neutral bit, **not HP**.
Coordinates are masked3F, facingDown3; allies use eas_Standing and enemy
scripts derive from the physical loader. With A placed allies, these intact
sixteen enemies map to physical A..A+15. No33 neutral table entry adds Frayja;
held533 separately skips the neutral phase. Scene aliases and endpoints are
discarded; physical Frayja now depends on combatant placement, not follower82.

## Actual lowering and accepted consumer boundaries

**Confirmed about accepted code:** production consumes prepared Content,
not research JSON/disassembly on demand. The isolated
[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py) probe
produces110 main instructions with entitiesRunning=true, plus seven in
csub_4D078. Main native calls are13/14/38 setBlocks,49 cameraSpeed and98
joinBatParty;99 calls the helper. Its0/1/2/3/4/5 are native cmpi.w,beq.s,
jsr,jsr,jsr,@Return:;6 ends. The move.w immediate2874 is compiler register
tracking, not an executed DisplayText representation. No successful native
branch/text/replacement behavior follows from this lowering.

Every selected nested action is inspected recursively: all five list Init
streams, four awaited Init installations and ten movement streams contain
**no nested native-call**. The19 ordinary follower rows have no65 prefix,
which agrees with Map31's non-overworld branch; arbitrary follower/capacity/
class/dead-alias fidelity still needs assessment. Earlier initialization,
resource, fade, sprite or service failures can precede main native13.

| Real consumer | Supported shape / selected missing or refused boundary |
| --- | --- |
|[ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|Native calls become UnsupportedInstruction and reached `program-opcode` refusal; nested native forms would become UnsupportedEntityAction. Known move/flags/speed/size/refresh/idle, quake2/0, text, scene and joins have consumers. No selected csc34/csc51/native2874 implementation is inferred. Position/face shapes do not encode the original ally-HP skip.|
|[EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs), [ExplorationDispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationDispatcher.cs), [MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)|Motion installation sets cursor/timer, optional9F flags and clears follower ownership; waits yield ScriptIdle. Entity service moves actors then executes actions, with sprite-readiness and bounds/collision guards. Awaited Init6–11 precedes13; scene-map3 has missing-map/black/palette/window admissions before that. Script fade/wait service can execute installed actions; installation is not completion, and failures preserve reached state.|
|[SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [MapEntityAllocator](../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs)|Prepared follower allocation, duplicate ally suppression and alias preservation exist with49-slot guards. LoadScene preserves actors until separate entities reload. Neither a selected Map31 package nor unchecked arbitrary original allocation is established.|
|[MapPartyMembership](../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs), [ExplorationTextRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationTextRunner.cs)|JoinForce-like joined/active/count snapshot, explicit NAME/class and LEADER list rebuild exist. No selected count-2/DBF replacement or DIALOGUE_NAME_INDEX_1/2874 seam is supplied by ordinary Join.|
|[PrivateBattleEncounterReader.Pin](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [BattleEncounterDefinition](../../remake/src/Sf2.Remake.Application/Content/Scenarios/BattleEncounterDefinition.cs)|Shared placement/region/point/terrain/fresh-before-load-start shapes exist; Pin admits1/2/3 and rejects33. F88 NewBattle explicitly refuses `suspended-battle-entry`. Missing selected party/eight definitions/items/flags483/433/533/world/content/start/material bindings precede any Battle33 execution claim.|
|[Map preparation](../../src/sf2tool/remake_exploration_maps.py), [ExplorationViewRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs), [BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs)|Preparation exposes layer0 and foreground32,0 as overlay offset32,0. Field view accepts layer0 with background0,0 and unit parallax; foreground32 is allowed. Board unit-parallax/zero-autoscroll guard also passes and shifts foreground by that overlay offset. Board selects area using crop origin; original LoadBattle uses view target/fallback. Representable profiles do not bind selected layout/crop/flag-copy replay or prove rendering. Native cameraSpeed is refused, and nonzero admitted ViewSpeed independently hits `field-view-speed`.|
|[ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs)|Godot sprite/effect/window consumers exist; selected map/terrain/portrait/mapsprite/text/audio/palette resources, interleavings, completed services, exact presentation/timing and control are unassessed. No runtime or screenshot was used.|

Thus native13 is only the first top-level native **conditional on admitted
content/state and returning earlier services**. It is not an absolute first
runtime failure, and later supported instructions are not reached by ignoring
refusals. Prepared block-copy lifecycle shapes do not make setBlocks lowerable.

## Reproduction, acceptance and retained limits

Load ignored selections in the launching process; reuse the pair unchanged:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1088/review.py <configured-source-disasm-root> --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

Direct audit checks the complete58-record/ten-stream/list/native/keyed-text
scene, all28 placement rows/region/point, eight definitions/equipment and
selected tables; actual widths/branch polarity/call and write order/aliases;
fresh/F88/483, join/csc51/LEADER/2874/heal/sorted placement, Map31 copies/crop/
view/terrain/music and recursive lowering/current consumers. Changed states
include active/inactive/dead/over-capacity/short/empty cached lists, skipped
and interrupted scenes, changed selectors/flags/crop and aliased physicals.

Separately compare exact accepted-base→HEAD two paths, clean status and
navigation-only predecessor; verify UTF8/LF/links/private boundary and cached
diff. Retain actual exact-head scoped CI and direct receipts in Issue/PR
handoff. Documentation policy skips blanket normal/full Python/.NET/Godot/
H3/H4, old comparisons and tests of review programs. No maintained parser,
schema or fixture change creates new mutation-test obligations.

Preserve #1086 independent0.430s/CI39496093640, #1084 independent0.294s/
CI39493788988 and #1082 independent0.222s/CI39491194567. Retain #1080's
inventory-NOTHING versus definition-equipment error and clean-head
AssertionError[], focused0.107s/CI39489091772 correction; #1076 nested-action
and earlier LEADER/selector-width/HP/geometry/N==V failures and Unknown chain.
Twelve old completed comparisons remain discovery evidence, not a retry queue.
Preserve previous receipts/lowering before reruns; local failures and focused
corrections belong in this Issue's handoff, without resetting prior evidence.

**Unknown:** natural admission/travel; actual incoming words, flags, HP/class/
equipment/difficulty, joined/active/cached lists/tails, crop/current selectors,
physical aliases/followers/capacity; interrupted prefixes and service ordering;
completed load/text/input/join/actions/camera/quake/audio/terrain/sprite/fade
services, original presentation/timing, usable control and saved persistence.
These are not an automatic runtime queue. Done is a clean committed/pushed
frozen Draft PR with exact head/tree/base, checks/CI/failures/Unknowns and a
stopped writer/process handoff for independent main-gate review. This task
does not merge, clean/archive environments or select another Issue.
