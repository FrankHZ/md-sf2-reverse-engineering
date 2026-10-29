# Battle42 outcomes, Galam aftermath and Zeon admission

Evidence date: 2026-10-22. **Confirmed (static):** defeat has priority;
Galam128's death can sweep other living enemy slots without setting
their HP to zero. Battle42 has no enemy-item-drop row. Its guarded
aftermath rebuilds scene Galam128, Elis129 and special Zeon130, writes
F443, then returns through join0, clear442 and set542. Battle43 is
selected only at a later CheckBattle. The default Map59 setup supplies
the intervening warp when its live flags admit it. Natural completion,
presentation and playable remake support remain **Unknown**.

Continue with [Battle43 initialization](./battle43-initialization.md) for the complete
formation, refreshed values and Zeon/Elis reconstruction before first activation.

## Agreement, provenance and method

Standard rigor for [Issue #1174](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1174)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053).
Accepted base `30c14a926f519eff37c553f83d084b98b879f656`, tree
`4ab5063df3b15448addd0d64065e6bb48c13897a`. This supplies bounded
outcome, inventory, complete Galam-aftermath and Zeon-admission contracts
for later design/content work. Own this document and successor navigation
only in [Battle42 first control](./battle42-activation-and-first-control.md).
No shared owners, schemas, fixtures, importer or engine changes; no
unmerged dependency. Stop at Battle43 selection and initialization/hook
interfaces, before its roster, rounds, defeated/after scenes or ending.

Reuse [Battle42 initialization](./battle42-initialization-and-opening.md),
[Battle41 outcomes](./battle41-outcomes-and-ancient-tower-stairway.md),
[Battle40 outcomes](./battle40-outcomes-and-odd-eye-aftermath.md),
[Map59 arrival](../moun-nazca-grans/nazca-voyage-and-map59-arrival.md),
[item transactions](../../world/map-event-item-transactions.md),
[request consumption](../../world/map-event-request-consumption.md),
[common maps](../../world/common-maps.md) and [scripting](../../world/common-scripting.md).
Selected42 rows, complete aftermath and actual consumers are rechecked
here; earlier geometry, opening lowering and completed suites are reused.

Original paths are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battle/` abbreviates `code/gameflow/battle/`; `map/` abbreviates
`code/common/scripting/map/`. Fnnn means a story flag. Ordinals are
zero-based, intervals half-open, packed words/addresses hexadecimal.
sf2const.asm, sf2enums.asm and the cutscene/entity macros own widths.
USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new source/ROM equality or decoded geometry is claimed.

Method: linear selected source/consumer reads and a few supplied valid
state variations. Reuse the immutable18,285,979-byte init/script pair
with `local/issue1005/pair.txt` provenance for **one** after42 diagnostic
lowering. No all-map/game/terrain export, runtime acquisition, general
comparator or new framework. Ignored material lives in `local/issue1174/`.
Estimate seconds per direct command, selected evidence<2MiB and total
CI/coordination<4MiB; peak-memory target512MiB is **unmeasured**. These
estimates trigger reassessment, not omission of required behavior.

## Terminal dispatcher, leader sweep and no selected drops

**Confirmed:** `battle/battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`)
runs defeated hook→ProcessKilledCombatants→CountRemainingCombatants after
the individual turn/debug branch. Ally count0 branches to defeat before
enemy count0 branches to victory. Only a nonterminal result runs the
actor's after-turn effects, cleanup and the same priority checks. There
is no second defeated hook there. Only another nonterminal result
increments the byte turn cursor by2. Admitted retreat unwinds before
this suffix; no particular action, kill, AI decision or RNG is simulated.

| Complete table / selected42 entry | Exact interface |
| --- | --- |
|battleendcutscenes.asm,48 relative words,base$47BE8|slot42/byte84 at$47C3C→ms_Empty (FFFF).|
|enemyleaderpresence.asm,45 bytes,base$47C8E|byte42 at$47CB8→−1, enables sweep.|
|afterbattlecutscenes.asm,48 relative words,base$47CF4|slot42/byte84 at$47D48→abcs_battle42.|
|cutscenes/afterbattlejoins.asm,52 bytes,base$47D6A|byte42 at$47D94→0.|
|global/afterbattlepositions.asm|Only battle5 has an override; none for42.|

These paths are under `data/battles/`, with `global/` for leader/position
tables. `cutscenes/battleendcutscenesstart.asm` (`$47B92..$47BE8`)
requires Bowie0 HP.w!=0 and enemy128 HP.w=0. spriteset42 names128
Galam97,13,18,STARTING. AI LEADER14 is independent of the leader byte.
Clear542 interprets ms_Empty; held542 skips that interpreter. Both reach
battleendcutscenesend.asm (`$47C48..$47C8E`), reread CURRENT_BATTLE,
and unchanged42's−1 scans all32 IDs128–159. Every HP.w!=0 appends to
the **existing** death list, adds an FF byte sentinel and increments
its word length. No position, roster, activation, neutral or duplicate
check is made. No queue record is removed by this sweep itself.

ProcessKilledCombatants (`$24518..$24642`) requests effects, sprite
spin/sound and waits, credits ally defeats or BATTLESCENE_FIRST_ALLY
kills, then writes each listed X/Y=−1,status0, refreshes stats and parks
its aliased entity at7000,7000. It does **not** write HP0, call the drop
transaction or clear list length. CountRemainingCombatants
(`$23C58..$23CBA`) separately scans30 allies and32 enemies using signed
X.b>=0 and HP.w!=0; Y, membership, neutral and activation are ignored.
Bowie HP0 finally forces ally count0. ApplyPositionsAfterEnemyLeaderDies
has the same Bowie/128 HP gates, but its battle5-only table gives42 no
new coordinates.

| Supplied state at these reads | Result if services return |
| --- | --- |
|Fresh15 STARTING living enemies, four HIDDEN HP0/X−1|15 count even if inactive; unspawned132–135 do not.|
|A hidden132–135 successfully reset and remains positioned/living|It counts normally; its prior HIDDEN declaration does not exempt it.|
|Bowie alive,128 HP0, other living enemies including hidden/stale slots|All nonzero-HP128–159 append; cleanup removes them from X counting without HP0 or drops.|
|Clear542 neutral backing159 HP0,X13/Y15,activation0008|It fails HP count/sweep admission. Its physical Elis/alias31 is not an extra combatant.|
|Held542 fresh159 X−1, or undeclared147–158|Absence alone does not certify all retained fields. A changed living/positioned slot counts; a living off-map slot still sweeps.|
|128 HP!=0 but X<0; every enemy fails X/HP|Ordinary victory can occur without leader hook.|
|128 living/positioned alone|Nonterminal; subordinate deaths are insufficient.|
|Bowie HP0 with enemy count0, or no countable allies|Defeat wins; BowieHP0 also skips defeated hook.|
|128 dies only in after-turn effects|No second hook/sweep that turn; another countable enemy can keep combat running.|
|542 held|Both scene bodies skip at their own guards; sweep and terminal priority remain.|

**Confirmed:** the complete `data/battles/global/enemyitemdrops.asm`
(`$BE52..$BECC`) has30 four-byte rows and a word terminator, **zero**
VERSUS_GALAM42 rows. Neighbors include41/HOLY_STAFF/dropped18 and
43/HEALING_RAIN/dropped19; neither belongs to42. Galam's copied equipped
Demon Rod and other actual inventories do not select a drop by themselves.

The three direct BSR call sites in battleactionsengine_1.asm to
`battleactions/dropenemyitem.asm:battlesceneScript_DropEnemyItem`
(`$BD24..$BE52`) still require allied actor, enemy target and targetDies.b
nonzero. The loop compares live battle byte and full target ID before
looking in its four actual slots. Unchanged42 reaches no matching row:
no selected slot check, special range32 draw, dropped-bit write, removal,
actor capacity test, award, Deals update or pickup text follows.

For a genuinely matching **other** battle row, the reused transaction
order remains: find actual item masked by index; special Taros Sword/
Iron Ball/Counter Sword draw before dropped BSET; held bit stops;
first bit admission removes/arranges item and refreshes holder; actor
HP.w!=0 then four-slot AddItem without class/equip eligibility or
force/Caravan search; success emits396 then395. HP0/full falls back
to RARE check and Deals nibble increment capped15 after bit/removal.
Award uses bare index, without equipped/broken bits. The unreachable
repeat-chance suffix stays unreachable. Independent dropped flags at
FFF7A4 are not F442/F492/F542. These reused branches explain why a
42 sweep or equipment declaration is no award; they do not invent a
42 transaction or carry43's Healing Rain backward.

## Victory seed, defeat, retreat and ordered flags

**Confirmed:** `battle/battleloop_2.asm` (`$23CBA..$23D98`) first heals
living allies and immortal Peter7/Lemon28 even at HP0. The30-slot helper
restores maxima, retains STUN/POISON/CURSE and refreshes stats; other
dead allies skip. It saves CURRENT_MAP.b into PARAM2, rebuilds force/
active lists, reads first active member and adds BATTLE_AREA_X/Y by
**byte** arithmetic to that member's live X/Y for PARAM3/4. Its physical
alias supplies facing PARAM5. PARAM1=0 is written without posting an
event. PARAM1–5 occupy FFA84C..FFA850. Empty membership/stale aliases
have no managed fallback in this native path.

Unchanged42's selected row is Map59,crop0,0/28×38. The saved return is
59,memberX,memberY,alias-facing, independent of scene Bowie13,17/Up,
camera8,13 or later actor motion. The selected active member need not
be Bowie, last actor, alive or positioned. Changed crop wraps low bytes;
later scene commands do not replace the seed.

ExecuteAfterBattleCutscene (`$47CBC..$47CF4`) rereads42/542: clear runs
abcs_battle42; held skips. The completed scene writes43+400=**443**
via csc13_setStoryFlag's word operand and BATTLE_UNLOCKED_FLAGS_START.
It does not set43,493 or543. EndAfterBattleCutscene rereads the selector
and passes join byte0 to JoinForce, including when542 skipped the scene.
That rejoins Bowie: set joined0, rebuild lists, set active32 only below
12 active; existing active32 survives full capacity. A newly set active32
is absent from the just-built list snapshot. No Elis/Lemon/Zynk recruitment.

Only after wrapper return does victory reread CURRENT_BATTLE, clear
400+index and set500+index: unchanged42 gives **clear442 then set542**.
F492 is untouched here. It clears d0–d3 **words**, returns PARAM2–5
in their low bytes and d4=1; upper long halves are not established.
Scene/map/selector mutations are not rolled back by these reads.

Defeat first calls UpdateBattleUnlockedFlag: clear442 only if542 was
held at that read. It clears dialogue-name1, requests sad music/text363/
close, restores BowieHP=max, halves gold with lsr.l1, then calls
GetEgressPositionForBattle (`$23E50..$23EB0`) and returns d4=−1. No
aftermath/join/new542. The battle4 losable exception does not apply.

Egress/Angel Wing admission/payment belongs to the existing action
mechanism. Once admitted, MP/item payment precedes text/flash/sound,
unlock update and action-frame unwind, then the same egress helper
returns d4=0. It bypasses defeat gold/Bowie recovery and victory flags.
Battle42 matches neither forced unlock-clear exceptions38/39 nor fixed
town exceptions16/17/13/31/32: it reads **live EGRESS_MAP**, not a hardcoded
Galam town. The default Map59 earlier setup writes DOJO45 only on its
999-clear branch; that provenance is not a fresh write by this outcome.
Church/savepoint changes can supply another egress source.

GetSavepointForMap (`$75EC..$764E`) with399 held scans the23 four-byte
rows in global/savepointmapcoords.asm (`$7B71..$7BCE`). DOJO45 is
row14→12,25/Up; an absent map keeps supplied map with1,1/Up. Clear399
instead returns3,56,3/Down. Held64 separately updates raft state through
the raft-reset table, including its default row; it does not change the
selected destination. Normal BattleLoop sets399, but changed incoming
flags remain distinct supplied branches.

| Stable supplied flags / outcome | Mutations at this boundary |
| --- | --- |
|542 clear, victory reaches full scene|Scene set443→join0→clear442→set542;492/493/543 unchanged.|
|542 held, victory|Skip scene; join0→clear442→set542. Incoming443 remains, including clear443.|
|542 clear, defeat/admitted retreat|No forced442 clear; incoming442/443/492/493/543 retained; no new542.|
|542 held, defeat/admitted retreat|Clear442;542/443/492/493/543 retained.|
|Interruption before set443 / after443 / afterclear442|Different persistent-memory prefixes can exist; no atomic scene/outcome/save commit is proved.|

Returning to59 after defeat/retreat is conditional on the selected savepoint
and subsequent map requests. If442 is retained, a later matching59
CheckBattle selects42 again;492 can skip its intro while fresh combat
initialization still runs. If542 held caused442 to clear, it does not
automatically restore that unlock. F443 can independently admit43.

## Scene load, complete six-row table and live identities

**Confirmed:** complete `data/battles/entries/battle42/cs_afterbattle.asm`
occupies `$4EF04..$4F358`. Its214 top-level commands contribute914 bytes;
inline motion/custom payloads contribute146; ce_4F328 occupies48 bytes.
Top-level ordinals below refer to this complete source/pair corpus,
including waits and the final terminator. No typed-output count stands
in for these native commands.

`loadMapFadeIn59,8,13` first sets OUT_TO_BLACK fading state, then falls
from csc37 into csc48. It detaches view, loads selected tilesets, waits
VInts until fade setting clears, deactivates services, packs `(8<<8|13)*3`
=1827 hex and calls LoadMap with d1.w=59. LoadMap resets view scroll
state, selects map/palette/area, decodes blocks/layout, applies live flag
copies then open chests and battle crop, and updates view planes/window/
roof services. It enables display/services and waits another VInt before
return. It does **not** run CheckBattle, exploration setup init, combat
initialization, recruitment or a flag reset. CURRENT_BATTLE remains42
through this scene under unchanged input; crop0,0/28×38 still applies.

The next loadMapEntities is a separate csc42 operation: disable services/
display, consume mainEntity's three words, clear49 physical records,
64 aliases and walking scratch, create live followers, read fixed rows,
declare Bowie0, load mapsprites and restore services. This rebuild loses
combat packing and neutral Elis's earlier alias31 role. It preserves
combat records, story/drop flags, membership, saved PARAM2–5 and unrelated
retained state; it is not a battle-state snapshot restore.

| Address / encoded bytes | Complete ce_4F328 record |
| --- | --- |
|4F328 /6|mainEntity13,17,Up→Bowie0, actual ally sprite,eas_Idle.|
|4F32E /8|15,15,Left,ALLY_PETER7,eas_Init.|
|4F336 /8|12,19,Right,ALLY_LEMON28,eas_Init.|
|4F33E /8|13,19,Down,EVIL_GALAM164,eas_Init→scene128.|
|4F346 /8|13,15,Down,ELIS204,eas_Init→scene129.|
|4F34E /8|62,62,Down,ZEON251 special sprite,eas_Init→scene130,physical47.|
|4F356 /2|FFFF cscEntitiesEnd.|

Fixed fields are X/Y/facing/sprite bytes and script long. Native
InitializeMapEntities tests sprite>=MAPSPRITES_SPECIALS_START240;
Zeon uses ENTITY_SPECIAL_SPRITE47, appends47 to the nonally alias list,
sets ENTNUM32, distinct from scene logical130, and does not advance the ordinary allocation
counter. Its layer starts0 through clr.l d6. Galam/Elis are ordinary
nonallies. With n admitted followers and p/l=1 when Peter/Lemon fixed
rows allocate anew, Galam physical n+p+l+1, Elis n+p+l+2, Zeon47,
Bowie0. Arbitrarily many/overlapping follower slots have no capacity
proof from this formula; native declaration does not add a49-slot guard.

Map59 is not overworld: reuse the complete19 ordinary follower rows,
not the Caravan prefix even if65 held. Fixed ally rows reuse existing
nonzero aliases and skip their position/script install. Explicit scene
Init/position then apply separately. FOLLOWER_B31 resolves the last
admitted74/Astral,68/Oddler,70/Astral row in authored order; if none,
its cleared0 alias targets Bowie. Text's Astral meaning does not prove
that producer. F67 Peter/F71 Lemon can allocate before fixed rows.
F76 creates Zynk26; the sole scene flag branch skips his explicit Init/
63,62/Down when clear and runs them when held. It never joins him or
changes76. Other admitted followers remain present without explicit commands.

SetPos/setFacing csc19/23 read the ID **byte** and skip4/2 operand bytes
only for real allies0–29 at HP.w=0. HP1..255 and HP256+ both pass: this
is a full-word zero test, not an HP high-byte threshold. Peter/Lemon
normally healed by victory still use a fresh live test at each command;
Zynk can remain dead and skip position while receiving unguarded Init.
Nonally31/128–130 bypass it. csc1B/1C start/stop read full entity words
before that helper; the scene's128/129 likewise bypass ally HP. Their
high byte00 is **not** the separate csc2C_followEntity defect, where a
pre-read byte of a word tests BowieHP. There is no followEntity command
in this aftermath. No earlier missed high-byte finding is generalized
into an incorrect guard for these selected operations.

Actions/scripts, sprite/priority, shiver, nod, camera and text do not
share that HP guard. Alias resolution masks IDs to a byte, subtracts96
for enemy IDs, then indexes the64-byte table without an index guard.
It uses the fetched byte without a safe missing-alias fallback.
Two aliases to one physical actor share/rewrite that actor's script
pointer/timer; a wait is for that physical slot, not all actors.

## Complete aftermath in narrative order

**Confirmed:** the following covers all214 top-level commands. `W` means
the command waits for that resolved actor's pointer to become eas_Idle;
`NW` returns after installing a script. Direction abbreviations U/D/L/R.
Wait numbers are encoded service requests, not measured elapsed frames.

| Ordinals | Ordered scene operations |
| --- | --- |
|0–12: staging|textCursor3106; map59 camera8,13; table above; InitW Bowie,Peter; Peter15,15/L; InitW31 then11,15/R; InitW Lemon then12,19/R; branch clear76 past InitW Zynk and63,62/D.|
|13–23: defeated Galam|Stop128; customW motionOFF/orientLeft/updateSprite/jumpIdle; DeactivateAutoFacingW128; Stop129; same customW for129; priority31=0,Lemon=0,128=FFFF; fadeInB; cameraSpeed30 hex; wait60.|
|24–43: recognition|Shiver128,3106; Bowie/Peter/31 faceD; shiver128,3107–3108;128 facesU;3109 Lemon; shiver129; PeterL,JumpW,3110; BowieU,31R,LemonU;3111 Elis,3112 follower,3113 Elis.|
|44–65: Elis wakes|31 R1 NW; Lemon U1 W;3114;wait50;InitW129;129D,POSE6,3115;JumpW31,3116;129L,3117;129D/wait30/U/wait30/L,3118;3119 Peter;129U;JumpW Peter,3120.|
|66–88: private moment|Peter D2,L1 W;129D,PeterU,3121;129 spriteELIS,D,start,3122;customW129 speed24,24→Idle;129 D1 W;wait50,3123 Bowie;wait50,3124 Elis;Peter positions13,18/U→wait30→14,16/L→wait30→13,15/D→wait30→14,17/L;31D,3125.|
|89–111: first tremors|Quake2/wait30/quake0/wait5;Lemon 2xRightLeft NW/wait120/facingD;quake2;Bowie L2 NW;129 D1,L1 W;wait5;Bowie2xUpDown NW;Peter R2 W/wait5/2xRightLeft NW;31 R1 W/wait5/2xRightLeft NW;wait120;3126 Peter,3127 Lemon;31U,3128.|
|112–127: approaching the rupture|Bowie U2,R1 NW;Lemon U1,R1,U1 NW;Peter U2,L2 NW;129 U1 NW;waitIdle Peter only;BowieR,31L,3129;nod31;Bowie/Peter/31/LemonU;camera destination8,3 waits;quake3;3130.|
|128–146: opening ground|Battlefield-death sound→copy12,39→10,3/7×4→wait40→3131; sound→copy12,44→same→wait40→3132; sound→copy12,49→same→wait40→3133;wait40→3134;demon-breath sound,quake4,wait30;intro-lightning sound→copy12,54→same.|
|147–164: Zeon rises, Galam leaves|FlashWhite30;130 position13,6/D;tintMap;wait40;quake0;audio fade-out command;wait90;cameraEntity128;customW128 speed48,48→Idle;wait110;Zeon-attack music;128 spriteEFFECT3,D,start;128 U13 W;MOSAIC_OUT128;camera neutral;wait50.|
|165–178: confrontation and faint|3135 Zeon,3136 follower,3137–3138 Zeon;129D,stop,customW motionOFF/orientLeft/updateSprite/jumpIdle;quake3;intro-lightning sound;FlashWhite30;tintMap;quake0;wait50;camera destination8,12 waits.|
|179–201: escape rejected|31L,3139;PeterL,3140;31R,3141;3142 Peter;31D,LemonL;JumpW31,3143;BowieD,JumpW;Lemon D1 W;LemonR/wait30/D/wait30/L/wait30/D/wait30;3144.|
|202–213: battle resolve|wait5;Peter2xRightLeft NW;31L/wait40/U;3145 nextText then3146 single;Bowie/PeterU;nodBowie;setStoryFlag43;FFFF end.|

All14 inline motion streams in installation order are thus:
31 R1 NW;Lemon U1 W;Peter D2,L1 W;Elis D1 W;Bowie L2 NW;
Elis D1,L1 W;Peter R2 W;31 R1 W;Bowie U2,R1 NW;
Lemon U1,R1,U1 NW;Peter U2,L2 NW;Elis U1 NW;Galam U13 W;
Lemon D1 W. Each endActions appends a jump to eas_Idle. csc2D masks
flagsA with AND9F and writes the resolved physical index into that
slot's byte wait timer; movement distances are signed
bytes widened to words, installed as ac_moveRel. Eight W/six NW streams
do not imply all previous NW work finished; ord116 waits only Peter.
Changed aliases or service scheduling can leave other actors moving.

All five custom streams are named in the table: ord14,17,171 each
motionOFF/orientLeft/updateSprite/jumpIdle; ord74 speed24,24/jumpIdle;
ord155 speed48,48/jumpIdle. They end with8080 delimiters. Their wait
form installs inline pointers then waits for Idle before scanning past
that delimiter. Both ordinary/custom installations write the resolved
physical index into the wait timer. Despite its name, Motion OFF writes
animation counterFF, like stopEntity; it does not zero velocity or
acceleration. Motion ON writes1 while startEntity writes0. eas_Init
establishes speed32, zero acceleration factors,
collision/obstruction settings, autofacingON,
orientationUp,size24,refresh,Idle. Galam's explicit DeactivateAutoFacing
persists independently of later facingD; Elis Init restores autofacing
after waking, and the final collapse stops animation again.
eas_Jump temporarily disables autofacing, requests0,−1/wait5→0,1/wait5
→0,0, restores autofacing and jumps Idle. 2xRightLeft/2xUpDown are four
face/update/wait30 steps then Idle, not perpetual oscillators. Their NW
installation can be replaced by later motion/script commands.

With distinct admitted aliases and returned motion, Elis goes13,15→
13,16→12,17→12,16, then collapses there; Galam13,19→13,6 before mosaic.
Peter's explicit placements supersede earlier movement. These are
conditional requested endpoints, not decoded collision or measured poses.
No command deletes Galam, revives/changes Elis combatHP, recruits her,
adds an item or modifies combat159 because of the scene meaning.

## Text, layout and presentation interfaces

**Confirmed:** exactly41 text requests consume IDs3106–3146 in order.
Only3107,3137,3145 use nextText; all others nextSingleText. Modifier C0
(right+mirrored) applies to3106–3108,3111,3113,3115,3117–3118,3122,
3124,3135,3137–3138; all other modifiers0, including Elis3133.
`N` below is `{N}`; W1/W2 are `{W1}`/`{W2}`; bracketed names denote
the exact dynamic token, not literal dialogue. No full text is reproduced.

| ID | Logical speaker / meaning | Ordered tokens |
| --- | --- | --- |
|3106|128 Galam pain|W1|
|3107|128 prior invincibility|N,W2|
|3108|128 disbelief|W1|
|3109|Lemon's lament|N,W1|
|3110|Peter notices Elis|W1|
|3111|129 asleep|W1|
|3112|31 notices waking|W1|
|3113|129 yawns|W1|
|3114|31 calls princess|W1|
|3115|129 startled|W1|
|3116|31 identifies Astral|W1|
|3117|129 asks what happened|N,W1|
|3118|129 asks identities|W1|
|3119|Peter greeting|W1|
|3120|Peter introduces self/leader|{NAME;7},N,W1|
|3121|Peter prompts leader|{LEADER},W1|
|3122|129 recognizes Bowie|W1|
|3123|Bowie hesitation|W1|
|3124|129 hesitation|W1|
|3125|31 asks Peter to leave privacy|{NAME;7},N,W1|
|3126|Peter notices quake|N,W1|
|3127|Lemon senses emergence|N,W1|
|3128|31 recalls other devil|N,W1|
|3129|Bowie names Zeon|W1|
|3130|Peter surprise|W1|
|3131|Lemon alarm|W1|
|3132|31 concern|W1|
|3133|129 fright|W1|
|3134|Bowie alarm|W1|
|3135|130 Zeon on lost minions|N,W1|
|3136|31 names Zeon|W1|
|3137|130 accusation|W2|
|3138|130 denial|W1|
|3139|31 proposes later return|{LEADER},N,W1|
|3140|Peter rejects retreat|W1|
|3141|31 asks why|W1|
|3142|Peter explains Elis fainted|N,W1|
|3143|31 reaction|W1|
|3144|Lemon notes surrounding devils|N,W1|
|3145|31 urges sword/jewel and immediate fight|{LEADER},N,N,W1|
|3146|31 final resolve|W1|

The text engine consumes cursor/modifiers/portrait/name/speech interfaces;
words do not prove alias presence, recruitment, completed acknowledgements
or timed audio. nextText versus single and the dynamic tokens are retained.
ExecuteMapScript initially clears text-skip and resets inline scratch;
debug P2Start can reassert skip. csWait is a negative word decoded to
low-byte Sleep unless skipping; waits are not a measured frame rate.
FFFF conditionally waits for scrolling if dialogue is open, clears
VIEW_SCROLLING_SPEED.w and restores registers; it is not an implicit
warp, battle dispatch, global actor-idle barrier or save commit.

Map59's complete flag-copy table `$B939E..$B93A8` has one8-byte row plus
FFFF: F542,source12,54,size7×4,dest10,3. Packed operands0C36,0704,0A03.
LoadMap applies it **before** chest writes and the selected battle crop.
At ordinary first victory's after-load542 is still clear, so this row
skips; the scene's four copies run regardless of542. If the guard was
clear but542 changes before this load, its row applies first and the
four scene copies still overwrite the same destination in authored order.

| Scene ordinal | Packed source / size / destination | Working-layout effect |
| --- | --- | --- |
|129|0C27 /0704 /0A03|12,39→10,3,7×4 words.|
|133|0C2C /0704 /0A03|12,44→same.|
|137|0C31 /0704 /0A03|12,49→same.|
|146|0C36 /0704 /0A03|12,54→same; matches the eventual F542 copy.|

CopyMapBlocks (`exploration/exploration.asm:CopyMapBlocks`) reads the
**current RAM** layout with64-word rows, copies left→right/top→bottom,
not a fresh immutable source tile array. Each request writes28 words/
56 bytes; four requests write112 words/224 bytes cumulatively to the
same28-word destination, not112 distinct destination words. csc34 also
sets both view-plane update bits. Source39/44/49/54 lies outside the
38-row battle crop but remains in the64×64 working layout; cropping
copies to0,0 rather than deleting those source rows. None is a decoded
block-value, collision, marker, terrain or visible-ground observation.
Rebuilding ordinary59 after set542 reapplies the final row from freshly
decoded layout; it does not persist intermediate scene RAM as a map save.

Camera speed writes word0030. setCamDest detaches target, requests8,3
then8,12 and waits for view scrolling; cameraEntity128 follows its live
physical alias and CAMERA_NEUTRAL detaches. Priority writes physical
byte0 for31/Lemon and1 for nonzeroFFFF on128, not a signed Z depth.
Plain setQuake writes amplitude words directly:2→0→2→3→4→0→3→0,
with waits/text/motion between; no ramp bits selected. Shiver saves
animation/size, sets size21, toggles flagsB bit3 for three Sleep5+5
iterations, then restores them. Nod stops animation, requests Sleep10,
one crop/update cycle with Sleep20+10, then starts animation. Galam's
MOSAIC_OUT uses16 bitmap-mask/DMA iterations with three WaitForVInt
requests each; it does not remove his alias/entity.

Both FlashWhite30 requests compute30>>3=3 then DBF, **four** LaunchFading
calls each, followed by DuplicatePalettes. Each launch uses F,1,
FLASH_QUICKLY_2, not30 observed frames. tintMap twice uses1,6,
HALF_OUT_TO_BLACK. Audio commands are three BATTLEFIELD_DEATH, one
DEMON_BREATH, two INTRO_LIGHTNING, one FADE_OUT and one ZEON_ATTACK
music in the sequence above. fadeInB calls FadeInFromBlack. Calls/waits
are necessary service interfaces; no completed rendering, camera, fade,
DMA, audio, vibration, acknowledgement or input readiness is measured.

## Return loop and conditional Battle43 handoff

**Confirmed:** MainLoop (`$75C4..$75EC`) receives battle return at
alt_MainLoopEntry: SwitchMap→ExplorationLoop, **bypassing immediate
CheckBattle**. Unchanged victory supplies explicit59. SwitchMap's three
rows replace Granseal/F640→Granseal Earthquake, overworld Pacalon drought/
F530→restored overworld Pacalon, and58/F999→59; explicit59
stays59. Coordinates/facing stay the saved member seed.

Exploration clears event type, adjusts/clamps STEP_COUNTER, heals/fades,
sets CURRENT_MAP59/CURRENT_BATTLEFF, loads tilesets, selects setup from
live543, reconstructs followers/entities, clears temporary256–383,
sets80, loads fresh layout with F542 copy, installs services and runs
that setup's init. Neither scene population nor combat neutral159 is
retained as ordinary population. Persistent442/443/492/493/542/543/999
are outside that temporary reset.

With543 clear, default ms_map59 has empty fixed entities; ordinary
followers and Bowie rebuild. Its actual init `$5EAD8..$5EB26` reads999:

- Held999 then clear443 returns without warp. Events/input are an
  interface only; no natural free exploration/collision claim follows.
- Held999/held443 calls cs_5EB26: csub_5EB34 clears32 longwords at
  PALETTE_1_BASE, then **posts** warp59,13,34/Up and returns. It neither
  performs Battle43 initialization nor immediately calls CheckBattle.
- Clear999 calls the five earlier Nazca scripts in order, then sets999,
  EGRESS_MAP45,436,440,442,443 and returns. Those earlier bodies remain
  excluded. This changed-state branch can restore442 and thereby select
  42 before43 on a subsequent admission. A first init here does not
  execute its held999 branch in the same call.

With543 held the distinct ms_map59_flag543 population/init is selected;
its init rereads543 and conditionally calls the ending program cs_615E6.
Stop at that accepted interface, without exporting/lowering the ending
or assuming it reproduces the default warp. Selection and init are
separate reads; flags changing between them retain distinct prefixes.

csc07_warp writes MAP_EVENT_TYPE.w=1 and five parameter bytes in order:
scroll0,map59,X13,Y34,Up. After setup returns and conditional palette/
music/fade work, WaitForEvent sees it. ProcessMapEvent clears type
**before** dispatch; no-scroll handler idles physical0, unwinds the
exploration return frame and returns map/X/Y/facing,d4=0 to MainLoop.
This request can be pending while service work has not completed.
The map59 table's separate Y37 wildcard→Map60,8,52/Right warp needs a
movement-marker caller and is not the setup's immediate handoff.

Now MainLoop repeats SwitchMap→CheckBattle (`$799C..$7A36`). The complete
45×7-byte battle-map table at$7A36 selects rows42 then43, both
`59,0,0,28,38,255,255`, addresses$7B5C and$7B63. Wildcard triggers
require no particular X/Y once the consumer is called. A clear unlock
skips before crop writes. An admitted row writes crop bytes first, then
tests its completion and may clear its unlock, **still returning that
battle on this call**. Completion does not skip the row or fall through.

| Effective map59 / live flags at CheckBattle | Selected boundary |
| --- | --- |
|442 held,443 either,542 clear|42;43 never tested.|
|442 held,542 held,443 held|Clear442 but return42 now; a later call can select43.|
|442 clear,443 held,543 clear|43;crop0,0/28×38; no completion-clear.|
|442 clear,443 held,543 held|Clear443 but return43 now, even though ordinary setup selection used543 separately.|
|442/443 clear|No42/43 battle; CheckBattle returns−1.|
|Supplied58,999 held|SwitchMap replaces58→59 before these tests; clear999 keeps old58/battle7 interface.|

Thus full unchanged first victory with999 held/543 clear yields
scene443→join0→clear442→set542→ordinary59 rebuild→default setup warp→
MainLoop CheckBattle43. This is a conditional source chain, not proof
its services naturally return, nor a user input interval between fights.
Held542 skip with443 clear does not recreate443; held442 at a later
read can still select42 before43. F542 never clears itself here, and
F442 is not a completion flag.

For admitted43 MainLoop passes resolved map59 in d0.w,battle43 in d1.w
to BattleLoop. The existing fresh interface sets399, clears seconds,
installs selectors/services, calls before, restores saved selector
words, clears regions90–105, heals, initializes allies/enemies/AI memory,
loads battle, then calls start. Slot43/byte86 of before table$47A88
(entry$47ADE) and start table$47B2C (entry$47B82) both select ms_Empty.
Before tests493 without setting it; start separately tests493 and sets
it **before** empty interpreter. Held493 skips bodies, not fresh init.
Held88 instead uses the established suspended branch, clearing88 and
AI memory/loading retained state while bypassing those fresh hooks and
initialization. Stop at these interfaces: no43 placement/terrain, queue,
first control, defeated hook, aftermath or ending is investigated.

## Actual final producer and production consumption

**Confirmed about accepted code:** the single diagnostic registers only
after42,scene_maps=[59]. It raises `ValueError: special cutscene entity
not admitted` while building ce_4F328 at Zeon's sprite>=240. Retained
abcs_battle42 contains only five instructions: text-cursor3106,
camera-entity null,FadeOut black,scene-map59 camera8,13,wait1.
eas_Init14 actions were cached while processing earlier fixed rows.
There is no serialized scene-entities instruction, continuation or end,
and no native-call in that five-op prefix. Original214-command corpus
is independently retained; it is not214 executable typed instructions.
The diagnostic catches the exception solely to retain partial compiler
state. It does not run the real final producer or publish a world.

Actual `prepare` in
[remake_exploration_content.py](../../../../src/sf2tool/remake_exploration_content.py)
checks canonical identity/pin, compiles selected programs and battle
routes, compiles maps and requested setups with the same compiler,
binds text/materials and finally writes serialized maps/programs.
An uncaught selected after42 compilation failure prevents reaching
that final write. The retained dictionary is not a usable partial
world or proof of disk rollback; no full producer was invoked here.

| Actual owner | Present capability / selected42→43 limitation |
| --- | --- |
|[OriginalPrograms](../../../../src/sf2tool/remake_exploration_programs.py), [Content reader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)|Typed flag/branch/text,scene-map,ordinary population,actions,position/facing/priority,layout-copy,quake/audio and selected presentation shapes exist. Special ce_4F328 population refuses first; later native/typed coverage was not lowered and must not be invented from an opcode list. native-call elsewhere reads as UnsupportedInstruction.|
|[Map compiler](../../../../src/sf2tool/remake_exploration_maps.py), final producer above|Default59 has a real setup route. Outside eventMaps, onLoad retains a542 guarded native flag-layout-copy then setup frontier; its long default init misses the four-op optimization. eventMaps and selected setup binding are different routes. The accepted conditional shared map-59-setup overwrite by requested543 remains **Inferred**; this task makes no variant/ending preparation or publication claim.|
|[Encounter preparation/routes](../../../../src/sf2tool/remake_exploration_battles.py), [encounter reader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [scenario reader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Preparation admits2/3; private reader/scenario assembly1/2/3.42/43 wildcard rows pass the coordinate-trigger predicate alone, but routes with outcomes then compile the actual after42 and hit the population refusal. Earlier missing selections/sources can fail first. No actual42/43 package, definition, terrain or material admission is supplied.|
|[BattleOutcomeRules](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/BattleOutcomeRules.cs), [BattleOutcome](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs), [Sf2OutcomeReturnPolicy](../../../../remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs)|Leader-defeat priority,represented living/positioned actors,guarded after-program,join→clear→set→return stages exist. Native unchecked32-slot sweep/death-list/alias and byte-wrapping seed are not equivalent to supplied actors and First/Single validation. Configured egress replaces live EGRESS_MAP/Church lookup;399-clear/64/640 variants and nonzero status refresh refuse. No42 action-side drops are required, but this does not close general drops/Deals gaps.|
|[SceneEntities](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [MapEntityAllocator](../../../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs), [ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [EntityActionRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Typed follower-first rebuilding,aliases,motion/actions/waits and sprite readiness have actual consumers. No native special47 population binding arrives from this compiler. Typed setPos/facing do not enforce native selected ally-HP skip. Reached unsupported instruction refuses before advancement; earlier committed field/program prefixes remain distinct from local calculation failure.|
|[MapTransfer](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs), [WorkingMapLayout](../../../../remake/src/Sf2.Remake.Domain/Maps/WorkingMapLayout.cs), [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Typed ordered copy/rebuild/preserve,setup selection and before→initialize→load→start continuations exist. Scene LoadScene does not run native setup init automatically. Declared-map transfers do not bind native58/F999→59 SwitchMap.542's native non-event wrapper,default setup warp and43 route need actual admitted content; one WriteFlag443 does not dispatch a battle.|
|[Adapter presentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs), [session view](../../../../remake/game/src/Exploration/ExplorationSessionView.cs), [capability status](../../../../remake/docs/capability-status.md)|Generic supported field/entity/text/view/effect projection exists. No selected59/42/43 world or complete audiovisual/input observation ran. Operands,typed primitives and table presence do not establish completed presentation,free exploration or playable Zeon admission.|

The preceding [first-control consumer correction](./battle42-activation-and-first-control.md#actual-content-domain-application-and-adapter-boundary)
still applies: Sleep and Stun differ; direct BattleAdvancer.Start's
return boundary differs from field entry's already published init/load/
intro prefixes. No general transaction claim is made for native scene,
compiler partial state or field continuation.

**Inferred:** later content work can reuse outcome staging,typed layout/
program/entity mechanisms after resolving special sprite population,
actual producer/setup/encounter admission and selected presentation.
Widening an allowlist or treating the partial dictionary as success
would not establish these contracts. No engine/tooling expansion follows.

## Reproduction, preserved failures and stopping boundary

Ignored `local/issue1174/review.py` retains selected source and consumer
snapshots, complete commands/inline payloads/tables, ordered guards,
widths/flags/changed-state derivations and link/UTF8/LF/ownership/private
checks. It reads the retained selected observation, without re-lowering.
No maintained verification framework or tests of verification programs.
Tracked prose and exact Git/source objects remain durable evidence.

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1174/review.py <configured-source-disasm-root> --receipt local/issue1174/direct-reproduction-01.json --committed --snapshots local/issue1174/direct-04.json
git diff --check origin/main
uv run sf2 verify plan --base origin/main --head HEAD
```

Every invocation needs a **fresh nonexistent receipt name**; change the
example after use. The source argument ends in disasm. The snapshot
argument reuses direct-04, not its receipt output. The sole selected
lowering exception/partial prefix is retained in observe-01 and is not
a passed import. Its token inventory also retained3153 as an inspection
upper bound; only the source's41 requests3106–3146 belong to after42.

ADR0004 review applies complete provenance/date/shape,widths,polarity,
call/mutation order,aliases,changed states,final output and actual consumer
boundaries. New parser/schema/fixture/mutation-test/H3-callback items are
N/A. Normal/full Python,old comparisons,SDK/Godot/emulator/H3/H4 and
screenshots are **SKIPPED** by explicit documentation-only scope. Clean
committed planner, actual scoped CI, terminal process state, resources
and frozen head/tree belong to the Issue/PR handoff.

Preserve the [predecessor's failures and Unknowns](./battle42-activation-and-first-control.md#reproduction-preserved-results-and-stopping-boundary):
#1162 high-byte followEntity/BowieHP missed-finding PASS and correction;
geometry/read failures, Map54 final overwrite, Map53 failed import/zero
partial, twelve completed old comparison failures; #1170 fresh-output
command-copyability correction and old-head CI/watcher propagation;
#1172 author/main-gate/CI PASS missing Sleep/Stun and direct Start/field
publication distinctions, followed by accepted correction. Those records
are neither erased, called interrupted nor rerun without invalidation.
This task's path-lookup failures and diagnostic refusal remain in its
ignored handoff; a shell/read failure is not original-game behavior.

**Unknown:** natural incoming party/HP/status/items,combat aliases/flags/
death list/queue/RNG, Church/EGRESS_MAP, actual terminal action and scene
reach, interleaved services/interrupted prefixes, decoded blocks/markers/
collision/terrain, actual requested motion/camera/fade/text/audio/DMA/
sprite/input completion, hardware timing, source-ROM equality, save
persistence and original/remake playability of this chain. Static supplied
branches do not close these or create an automatic runtime/repair queue.
Freeze a clean pushed Draft PR, actual scoped CI and complete command/
process result; main-gate independently reviews/merges. Author stops at
delivery: no merge, cleanup, archive, next Issue or43 body expansion.
