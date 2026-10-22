# Battle0 bonus battle and return to reset

Evidence date: 2026-10-22. **Confirmed (static):** EndGame admits the
Versus All Bosses battle on Map63. Its twelve enemy rows include ten
dual-turn bosses and two Prism Flowers. Victory requires no remaining
countable enemies; Zeon's death alone does not clear this battle. Victory,
defeat and admitted Egress/Angel Wing all return to EndGame's start-vector
reset. None of these returns resumes exploration in this caller. These
are conditional source contracts, not an observed or playable battle.

## Agreement, provenance and method

Standard rigor for [Issue #1194](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1194)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
one coherent battle, from the accepted
[ending admission](./ending-credits-finale-and-bonus-admission.md#battle0-admission-and-normal-return-reset)
through formation, regions, first control, the two laser bindings and
all normal returns to reset. Accepted base
`994a4a85a2685916521f2ad542b5230c33feb488`, tree
`162aa611cded3f029d5b588b964223aba2d6f3fa`; no stacked dependency.
Own this document and navigation only in that predecessor. Reuse
[battle loop](./battle-loop.md), [routing data](./battle-routing-data.md),
[Battle43 initialization](./battle43-initialization.md),
[activation/control](./battle43-activation-and-first-control.md),
[outcomes](./battle43-outcomes-and-ending-admission.md),
[Battle36 Flower laser](./battle36-prism-flower-laser.md) and
[Battle43 Guard laser](./battle43-zeon-guard-laser.md).
Selected Battle0 data and real use sites are checked here. Full boss action
selection, new independent special moves, full rounds, post-reset menus,
runtime acquisition and implementation are excluded. No epic completion claim.

Original paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battle/` abbreviates `code/gameflow/battle/`; `stats/` means
`code/common/stats/`. Constants/RAM and encoding come from sf2enums.asm,
sf2const.asm and sf2macros.asm. Addresses and packed words are hexadecimal,
intervals half-open; IDs/positions are decimal and ordinals zero-based.
Registered USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new ROM/source equivalence, decoded map/art or runtime claim.

Minimum evidence is linear selected source/consumer inspection: spriteset00
5,432 bytes, after00 300 bytes, shared laser table 2,372 bytes; twelve ally
placements, twelve enemy records, two regions and two AI points. Read
necessary shared callees rather than extract a new world. Direct-check
reassessment signals: 60 seconds/512MiB, 2MiB per output/4MiB total;
peak memory **unmeasured**. Small one-off recipe/receipts live in ignored
`local/issue1194/`; no maintained parser, model, cache or comparator.
The existing 18,285,979-byte init/script pair and
`local/issue1005/pair.txt` provenance are preserved; no need to read or
rebuild them or perform another selected lowering. Actual accepted
consumer code and retained predecessor native/partial results answer the gap.

## EndGame caller, flags and two entry paths

**Confirmed:** `code/specialscreens/witchend/witchend.asm:EndGame`
clears entities, sets400, supplies D0.W=MAP_MAGIC_TUNNEL_HUB=63, calls
CheckBattle, copies D7.W to D1.W and BSRs BattleLoop. The selected first
seven-byte row in `data/battles/global/battlemapcoords.asm` at `$7A36`
is map63, crop origin0,12, size32,36, triggerFF,FF. CheckBattle
(`code/common/maps/getbattle.asm:$799C..$7A36`) requires unlocked400,
then writes the crop. Clear completed500 retains400; held500 clears400
but still returns battle0. No completion rejection or preceding clear500.
Inherited D1/D2 coordinates do not restrict these two wildcard axes.
EndGame does not test the no-match D7=−1 sentinel; explicit map63 and
set400 are part of this admission, not a generic safe-call guarantee.

Battle-local positions below are relative to crop0,12: map coordinates
are (x,y+12). Tactical storage is48×48, crop32×36, map working layout64×64;
they are different boundaries. Ally slot0 at15,27 maps to15,39; Zeon18,5
maps to18,17. No entity/map pixel offset should be added a second time.

`battle/battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`) clears
PLAYER_TYPE.b, sets399, then tests88. With88 clear:

1. Clear seconds.l; save incoming D0/D1 words; write CURRENT_MAP=63 and
   CURRENT_BATTLE=0; install base VInts; execute before wrapper.
2. Restore/rewrite both selectors; clear region flags90–105 inclusive;
   HealLivingAndImmortalAllies; initialize all ally positions, then all
   enemy positions; ClearAiMemory.
3. Clear D0.W; LoadBattle; start wrapper; then activation/round prefix.

Healing scans all30 allies, independent of membership. Living allies and
Peter7/Lemon28 at HP0 receive maxHP/maxMP; other dead allies skip. Status
AND7 is intermediate: UpdateCombatantStats recomputes CURSE from equipped
cursed items and does not simply preserve old CURSE. A living ally with
old CURSE and no cursed equipment loses it; equipped cursed gear can
reintroduce it. STUN/POISON can survive into first control. This is the
accepted shared refresh, not a new healing-spell rule.

Held88 is an inherited or externally supplied-state conditional path;
the normal Battle0 menu cannot produce it. BattlefieldMenu's @SuspendGame
in `battle/battlefunctions/battlefunctions_2.asm` first tests
CURRENT_BATTLE.b and branches to @StartMenu when zero, before the prompt,
setFlg88, SaveGame or WitchSuspend. If88 is nevertheless held on entry,
BattleLoop instead restores saved seconds, clears88 and AI memory/view target,
calls LoadBattle with incoming D0, then jumps to the individual-turn loop.
It does not install63/0, reset flags/HP/stats/formation, run hooks or generate
the first queue. CURRENT_MAP/BATTLE, crop, combatants, queue/cursor and RNG
remain live inputs. For intact incoming D0.W=63, the alias getter indexes
offset63 of the64-byte alias list, enemy159; it is not ally63. Retained
alias bytes and LoadBattle's negative-alias fallback to physical0 still
apply. A set88 alone does not prove a valid suspended Battle0 or a safe
return. A later FF queue sentinel can reach the round prefix.

### Actual hook tables and zero values

**Confirmed:** four tables each have48 relative-word slots, not sentinel
lookups. Selected slot0 is at each table base:

| Table / base | Battle0 target / gate |
| --- | --- |
|beforebattlecutscenes.asm /47A88|ms_Empty; before wrapper tests450, does not set it|
|battlestartcutscenes.asm /47B2C|ms_Empty; start wrapper tests450, sets450 before body|
|battleendcutscenes.asm /47BE8|ms_Empty; defeated wrapper first requires BowieHP≠0,128HP=0,500 clear|
|afterbattlecutscenes.asm /47CF4|abcs_battle00; after wrapper skips body with500 held|

These paths are under `data/battles/cutscenes/`; corresponding wrapper
start/end files are under `battle/cutscenes/`. `ms_Empty` at47A4E is
FFFF/end only, not a null pointer or missing file. Its interpreter still
resets script scratch/text-skip, can wait for an open-dialogue view, and
clears view-scroll speed on return; skipping the body skips those effects.
Clear450 runs empty before then sets450/runs empty start. Held450 skips
both bodies while ordinary reconstruction/load still occurs. Each wrapper
rereads its live gate; no atomic snapshot/rollback is promised.

The complete region table at47EC8 contains32/0/F386,32/1/F386,
40/0/F387,40/1/F388, then FFFF.W. Its8-byte-row dispatcher finds no
Battle0 row; regions90/91 cause no selected scene. No inference from an
absent battle00 source file is needed. Enemy-leader table at47C8E has45
bytes; slot0 is **0**, so the defeated suffix does not sweep other enemies.
The after-join table actually read at47D6A has52 zero bytes. Its zero
means **JoinForce(0)**, not “no ally”; the separately named global copy is
an unused alternate source file, not another join invocation.

## Complete formation and twelve enemies

**Confirmed:** `data/battles/spritesets/entries.asm` points slot0 to
BattleSpriteset00; source-form spriteset00 occupies `$1B31A2..$1B32E2`,
320 bytes: header4, allies144, enemies144, regions24, points4. Subsections
are counted; there is no terminator between/after these records. Every
12-byte combatant row encodes identity/X/Y/AI bytes, item word, primary
order/region bytes, secondary order/region bytes, filler byte, spawn byte.
All ally rows have HEALER1=0, item007F, NONE/15 in both behavior slots,
filler0 and STARTING0.

| Ally placement ordinal | X,Y |
| --- | --- |
|0|15,27|
|1|15,28|
|2|14,29|
|3|16,29|
|4|13,30|
|5|17,30|
|6|13,29|
|7|17,29|
|8|14,28|
|9|16,28|
|10|12,30|
|11|18,30|

InitializeAllAlliesBattlePositions resets X of all30 slots to−1 and
calls UpdateForce to rebuild joined/active lists from flags in ID order. It consumes actual
active members, advancing the formation pointer only for HP.w≠0. A dead
member therefore does not consume a position; a changed active roster
changes which character receives each ordinal. Extra members beyond the
formation capacity leave the battle party. Twelve placements are neither
a fixed Bowie-through-character11 roster nor proof twelve allies are alive.
An empty/stale list has no managed native safety guarantee.

All enemy rows have placement item NOTHING=007F, secondary NONE=FF with
region15, filler0 and STARTING0. The table includes both behavior slots
through those common fields. Primary values below are hexadecimal:
40/41=MOVE_TO point0/1;80/82/87/89=FOLLOW_ENEMY ordinal0/2/7/9.

| Ordinal / logical ID / address | Kind(ID),X,Y | AI(byte) | Primary order/region |
| --- | --- | --- | --- |
|0 /128 /1B3236|ZEON(98),18,5|SENTRY(8)|40/15|
|1 /129 /1B3242|DARK_SMOKE(86),12,17|ATTACKER3(9)|41/15|
|2 /130 /1B324E|KING(89),22,19|ATTACKER1(6)|FF/15|
|3 /131 /1B325A|WILLARD(90),10,18|ATTACKER3(9)|82/15|
|4 /132 /1B3266|ZALBARD(91),15,14|SENTRY(8)|FF/15|
|5 /133 /1B3272|CAMEELA(92),19,15|ATTACKER3(9)|87/1|
|6 /134 /1B327E|RED_BARON(94),20,6|ATTACKER3(9)|80/15|
|7 /135 /1B328A|GESHP(95),21,11|SENTRY(8)|FF/1|
|8 /136 /1B3296|ODD_EYE(96),12,11|ATTACKER3(9)|89/0|
|9 /137 /1B32A2|GALAM(97),15,7|SENTRY(8)|FF/0|
|10 /138 /1B32AE|PRISM_FLOWER(93),5,20|SENTRY(8)|FF/15|
|11 /139 /1B32BA|PRISM_FLOWER(93),27,13|SENTRY(8)|FF/15|

`battle/battleloop/initializecombatants.asm:$1B1272..$1B15F8` initializes
all32 enemy IDs. Twelve declared rows copy their56-byte definitions,
set HP/MP maxima and X/Y, combine mover and placement AI, write kind,
triggers/orders/items, preserve only the definition's upper priority
nibble and combine filler/spawn, adjust difficulty ATT, refresh stats.
Battle0 is absent from the complete random-upgrade list3,8,9,10,14,17,
19,21,24,26,35; held500 does not upgrade these bosses. Slots140–159
fail the count test before advancing the first-row pointer: activation0,
max/currentHP0, X−1, triggers15/15. Their Y/kind/MP/items/spells/orders
and other unstated fields remain stale, not freshly zeroed definitions.
No selected HIDDEN/RESPAWN declaration; all twelve spawn-mask0F00 values
are0, even after region activation OR1/3. The spawn collector is empty for
this fresh population, not universally for a retained88 state.

### Copied definitions and actual refresh

**Confirmed:** `data/stats/enemies/enemydefs.asm:table_EnemyDefinitions`
starts1B1A66; definition address=base+56×kind, independent of placement
ordinal or unknownByte. All eleven selected kinds have unknownByte0 and
initialStatus NONE. DARK_SMOKE has REGULAR spellPower; all others
ENHANCED (copied class byte99). Unlisted item slots are007F, spell slots3F;
all named equipment below is copied already equipped. LV1 is implicit.

| Kind(ID) | Level,HP,MP,baseATT,DEF,rawAGI,MOV | Mover | Copied equipment / spells |
| --- | --- | --- | --- |
|ZEON(98)|99,500,180,102,50,203,0|REGULAR|none /DEMON2|
|DARK_SMOKE(86)|7,18,20,18,13,141,6|HOVERING|none /DISPEL1|
|KING(89)|24,55,28,29,40,161,6|MAGE|Flail90 /BLAZE2|
|WILLARD(90)|25,50,0,50,36,165,6|STEALTH|none /none|
|ZALBARD(91)|40,80,65,73,50,180,5|MAGE|none /BOLT2|
|CAMEELA(92)|50,99,0,42,52,186,6|REGULAR|Iron Ball102 /none|
|RED_BARON(94)|60,150,0,42,59,190,6|REGULAR|Dark Sword70 /none|
|GESHP(95)|70,180,120,78,51,191,6|MAGE|Flail90 /FREEZE4|
|ODD_EYE(96)|80,200,160,64,54,195,6|MAGE|Counter Sword68 /ODDEYE1|
|GALAM(97)|99,240,180,49,50,208,6|REGULAR|Demon Rod101 /DEMON1|
|PRISM_FLOWER(93)|40,62,0,16,55,44,0|REGULAR|none /LASER1|

| Kind | Resistance | Prowess critical / double / counter | Initial activation / currentAGI |
| --- | --- | --- | --- |
|ZEON|STATUS_IMMUNITY|125_1IN4 /1IN4 /1IN4|1000 /200|
|DARK_SMOKE|WIND_WEAKNESS,ICE_MAJOR,FIRE_WEAKNESS,STATUS_MAJOR|SILENCE /1IN16 /1IN16|0000 /141|
|KING|WIND_MINOR,LIGHTNING_MINOR,ICE_MINOR,FIRE_MINOR,STATUS_IMMUNITY|125_1IN4 /1IN8 /1IN16|1000 /161|
|WILLARD|STATUS_MAJOR|150_1IN4 /1IN16 /1IN16|0000 /165|
|ZALBARD|WIND_MINOR,LIGHTNING_MINOR,FIRE_MAJOR,STATUS_IMMUNITY|125_1IN4 /1IN8 /1IN4|1000 /180|
|CAMEELA|WIND_MINOR,LIGHTNING_MINOR,ICE_MINOR,FIRE_MINOR,STATUS_IMMUNITY|125_1IN4 /1IN4 /1IN4|1000 /186|
|RED_BARON|WIND_MINOR,LIGHTNING_MINOR,ICE_MINOR,FIRE_MINOR,STATUS_IMMUNITY|125_1IN4 /1IN4 /1IN4|1000 /190|
|GESHP|WIND_MAJOR,LIGHTNING_MAJOR,ICE_MAJOR,FIRE_MAJOR,STATUS_IMMUNITY|125_1IN4 /1IN8 /1IN8|1000 /191|
|ODD_EYE|WIND_MINOR,LIGHTNING_MINOR,ICE_MINOR,FIRE_MINOR,STATUS_IMMUNITY|SLEEP /1IN8 /1IN4|1000 /195|
|GALAM|WIND_MAJOR,LIGHTNING_MAJOR,ICE_MAJOR,FIRE_MAJOR,STATUS_IMMUNITY|125_1IN4 /1IN8 /1IN4|1000 /190|
|PRISM_FLOWER|ICE_MAJOR,FIRE_WEAKNESS,STATUS_IMMUNITY|150_1IN32 /1IN32 /1IN32|1000 /44|

Prowess is the **base** declaration; equipment can change it. Source
refresh copies base fields, applies status effects and scans four equipped
items. Difficulty's duplicated conditional applies floor(baseATT×5/4)
once at every difficulty and twice with intermediate truncation at SUPER;
equipped ATT follows. Flail adds19 ATT; Iron Ball44; Dark Sword50 and
−5 DEF; Counter Sword39 and +1 counter effect; Demon Rod50 and −10 AGI.
Copied words are00DA,00E6,00C6,00C4,00E5 respectively. Placement NOTHING
adds **no** item; it does not erase this copied inventory.

Zero Boost invokes the7-bit AGI increase preserving high80; zero Slow
then uses byte-decrease cap200 even for decrement0. Zeon203→200 and
Galam208→200→Demon Rod190. Other selected currentAGI bytes remain as
listed. High80 survives all ten boss bytes, so each offers two queue
records. This is separate from prowess DOUBLE. ApplyItemOnStats's enemy
branch clears the curse-test result: these copied cursed weapons do not
give those enemies CURSE. Ally curse derivation is different.

`data/battles/global/enemyitemdrops.asm:$BE52..$BECC` has30 four-byte
rows plusFFFF, **none for battle0**. No selected drop transaction matches,
including Cameela/Red Baron/Odd Eye's normally notable weapons. Copied
equipment and the global drop table are distinct requirements; NOTHING
in placements alone would not prove no drop. EXP/gold/statistics remain
shared action/cleanup behavior, not an extra Battle0 reward script.

## Physical entities, resources and native load

**Confirmed:** PositionBattleEntities in
`code/common/scripting/entity/entityfunctions_1.asm` clears49 records and
64 aliases, scans30 allies then32 enemies, and packs nonnegative signed
X/Y; enemy NEUTRAL also skips. HP is not this allocation predicate.
Let A be the actual packed ally count (fresh ordinarily0..12). Zeon128
uses special mapsprite251 at physical47 with **ENTNUM32**, not128.
The remaining eleven pack in order at A..A+10; alias138/139 therefore
select physical A+9/A+10, ordinary mapsprite157. Every fresh entity faces
Down3/eas_Standing initially, independent of laser direction.

| Logical IDs | Mapsprites from enemymapsprites.asm |
| --- | --- |
|128,129,130,131|251,150,153,154|
|132,133,134,135|155,156,158,159|
|136,137,138,139|160,164,157,157|

The complete neutral table448C4..4497A contains no battle0; clear500
scans to its final FFFF, held500 skips that scan. Neither creates an Elis
neutral or rewrites159/logical31 here. For the unchanged fresh population,
A+12 occupied records (max24), highest ordinary A+10 (max22), special47
are distinct quantities;48-record mapsprite scan covers them. Retained
positions/neutral/extra slots or special collisions are not covered by
that fresh capacity calculation. A supplied HP0 but positioned slot can
still allocate an entity while failing queue/remaining-count tests.

`battle/battlefunctions/loadBattle.asm:$25610..$256A2` preserves its
combatant word; requests fade out, tilesets, wait, VInt clear/wait,
PositionBattleEntities; restores/resolves that combatant alias for view
and chosen coordinates (divide native position words by384); initializes
64 sprite records; LoadMap(−1), wait, LoadEntityMapsprites, base VInts,
LoadBattleTerrainData, PlayMapMusic and fade in. Fresh D0=0 selects
actual ally0 alias/fallback, not formation ordinal0 by definition.
No Battle44 timer window. No ordinary map setup/entity program is called
by this battle load; it does not establish a new church EGRESS_MAP.

Terrain table1AD104 slot0 points to BattleTerrain00; spriteset table
1B30EE slot0 points to BattleSpriteset00. The legacy top-level binary
spriteset aggregate is not the maintained source-form inclusion. Map63
tileset declaration BAFB0 is palette6, tilesets17,255,34,35,36. Its ordinary
area table BAFDE has rectangles32,36..42,58;0,0..31,11;0,12..31,63,
then endWord. These are not crop32×36. Flag-copy events BB03A contain
endWord only. LoadMap rebuild/crop and tactical terrain48×48 remain
separate; no assets, decoded blocks, collision or rendered fidelity are
published here. VInt/DMA/fade/music/view services are requests, not
observed completions or established free-input intervals.

## Regions, activation and first control

**Confirmed:** region records occupy1B32C6..1B32DE; each is12 bytes.
AI points at1B32DE/1B32E0 are18,5 and15,22.

| Region / flag | Complete bytes grouped as header;V1;V2;V3;V4;tail |
| --- | --- |
|0 /90|4,0;0,4;0,22;13,22;30,4;0,0|
|1 /91|4,0;0,4;19,24;30,24;30,4;0,0|

Both overlap; neither is a sequential story phase. Reuse the exact
[signed integer triangle predicate](./battle43-activation-and-first-control.md#region0-and-the-exact-integer-predicate):
triangles1/2/4 then3/2/4, inclusive bounded projected edges, dominant-axis
projection from B and DIVS truncation toward zero. Do not substitute a
floating polygon or claim general cross-product equivalence. Ally scans
require signed HP.w>0 and signed X/Y≥0 across all30 slots, without
membership/active/neutral/crop tests. All twelve formation points at
Y27..30 miss both selected regions (bottom edges22/24). Shared vertex0,4
can hit both; eligible0,22 hits0, eligible30,24 hits1. Those are supplied
boundary inputs, not observed routes. HP0/8000 or negative coordinates
exclude a supplied ally; an eligible nonmember can trigger.

ActivateEnemies (`$2550C..$25544`) visits128..159 and falls through once
more with160. Its gate is X.b≥0, HP.w≠0 and negative actor byte; Y and
NEUTRAL are not gates. TriggerRegionsAndActivateEnemies uses the checked
word FFB20C: bit0/1 clear permits testing the corresponding region; a
hit sets90/91; hit **or miss** sets its checked bit. Misses never clear
held region flags. ClearAiMemory ends immediately before this word and
does not reset it; enemy non-SWARM StartAiControl clears it before the
activation gate. No selected SWARM. No imagined per-round snapshot.

Primary held non15 region produces activation OR1; otherwise held
secondary produces OR3. All secondary regions here are15; Cameela133
and Geshp135 read91, Odd Eye136 and Galam137 read90. Remaining eight
15/15 rows bypass PRIMARY admission without setting bit0. Thus unchanged
fresh points/clear90/91 leave those four on standby even though queued;
held flags or later eligible positions activate their respective pairs.
Checked bits already held can suppress new geometry but cannot suppress
activation from already held90/91. Leaving a region does not clear it or
previous PRIMARY. These region flags are distinct from unlocked400,
intro450 and completed500. The known extra160/deals alias from
[Battle26](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
remains conditional shared behavior, not a thirteenth enemy or new pickup.

After activation: region dispatcher (no0 row), optional debug printing,
spawn collector/animations (fresh empty), GenerateBattleTurnOrder. The
latter clears64 two-byte records toFFFF; offers allies0..29 then enemies
128..157 ascending. Admission tests X.b≥0 and HP.w≠0, not Y/PRIMARY/
neutral/membership/sleep/stun. For G=currentAGI AND127 and successive
direct RNG draws R, first score is G+R(G>>3)−R(G>>3)+R(3)−1. With current
byte≥128 append B=floor(5G/6), score B+R(B>>3)−R(B>>3). Skips consume0
draws; single records3; dual records5, continuing in offered order.
The main RNG recurrence is(13s+7) AND65535, range result floor(s*n/65536).
Generation does not refresh current stats or use thinking/debug RNG.

The unchanged twelve enemies offer **22 records/56 draws**. With N
eligible allies and T dual-turn allies, totals22+N+T and56+3N+2T;
fresh0≤T≤N≤12 gives22..46 records/56..116 draws. Changed HP/X can remove
any offer; retained slots invalidate the fresh bound. Sort descending
signed score bytes using62 passes/63 adjacent comparisons; equal scores
retain offered order. Wrapped negative scores can sort behind sentinel−1;
there is no universal natural actor or arbitrary-state capacity guarantee.
Cursor resets0; FF actor restarts the round, otherwise ExecuteIndividualTurn;
ordinary continuation increments cursor by2. F88 initially bypasses this.

ExecuteIndividualTurn (`$23EB0..$24242`) rechecks live HP and returns on0.
MUDDLE/AI_CONTROLLED, enemy control-opponent toggle and ally auto-battle
choose preparation; after cursor/view/windows/movement services, Sleep
then Stun can choose NoAction. The same control predicates are reread
before execution/player input. Preparation may already have changed scratch
before a status skip; no fixed first action/control-ready claim.

StartAiControl checks kind93/38 before ordinary commandsets after its
region gate. Only138/139 are selected line attackers. Other Battle0 rows
actually use SENTRY8, ATTACKER1=6 or ATTACKER3=9, not Battle43's LEADER14.
SENTRY commands: ATTACK1,HEAL1,SUPPORT,STAY; ATTACKER1 adds MOVE1 before
STAY. ATTACKER3: MOVE_ORDER3,ATTACK3,HEAL2,SUPPORT,MOVE_ORDER4,MOVE2,STAY,
with block-non-movable pathfinding;6/8 use regular mode. Commands try in
order until success. SENTRY has no movement-order command, so Zeon's
point0 declaration is not an instruction to move his MOV0 body.
Dark Smoke's point1 is15,22; Willard follows logical130 King, Cameela135
Geshp, Red Baron128 Zeon, Odd Eye137 Galam. FOLLOW_ENEMY resolves combat
slots/live positions, not physical entity numbers. A dead follow target
promotes secondaryFF to primary and replaces secondary withFF. Region
standby precedes ordinary command handling; declarations do not prove
those targets can be reached or movement completed.

General attack/spell/item construction remains with
[AI](./battle-ai.md), [actions](./battle-actions.md) and
[RNG/math](./runtime-rng-and-battle-math.md). Full Zeon/Dark Smoke/King/
Willard/Zalbard/Cameela/Red Baron/Geshp/Odd Eye/Galam action choice and
their independent special-move rules are **Unknown at this selected
battle boundary**. Copied spell/prowess fields are not proof each move
executes naturally here. No ten-boss research expansion follows.

## The two Flower lasers

**Confirmed:** laser list1AC9B8 orders battles36,43,0 and three pointers;
Battle0 selects the **third**, twelve-byte table:
`FF,FF,FF,FF,FF,FF,FF,FF,FF,FF,00,02`. GetLaserFacing
(`$1AC8A0..$1AC9B8`) indexes actor byte AND7F and rereads current X/Y,
not entity facing. Ordinal0 Zeon has FF, not a Guard ray.

| Ordinal / logical / initial physical | Origin / direction | First→last local square / count |
| --- | --- | --- |
|10 /138 /A+9|5,20 /Right0|6,20→47,20 /42|
|11 /139 /A+10|27,13 /Left2|26,13→0,13 /27|

Neither fresh formation nor another declared enemy lies on these rays.
Under unchanged positions/no other eligible ally, both prepare Stay.
Future live positions remain inputs. Unlike36's eight flowers/24-byte
table and43's two kind38 Guards/16-byte table, these are two kind93
Flowers, sprite49/palette0 in enemybattlesprites.asm. Reuse the accepted
[Flower pipeline](./battle36-prism-flower-laser.md#occupancy-decision-and-changed-state-examples):
StartAiControl→ProcessLineAttackerAi builds ally-only occupancy, picks
action6 and first near-to-far focus if any; otherwise Stay. Both terminate
the move string withFF. ExecuteAiControl consumes the move service, stores
returned X/Y, rebuilds ray blocks and focuses the saved ID. WriteBattlesceneScript
rebuilds occupancy with **all** combatants and rereads the ray, then sorts
IDs; saved focus does not pin damage targets or guarantee a hit.

| Supplied changed inputs, other slots absent | Preparation → reconstructed sorted targets |
| --- | --- |
|138 unchanged; ally9 at8,20, ally2 at15,20|focus9/action6 →[2,9]|
|Same plus enemy130 at12,20|focus9/action6 →[2,9,130], including friendly fire|
|139 unchanged; ally9 at25,13, ally2 at10,13|focus9/action6 →[2,9]|
|Only enemy130 on the line|Stay; enemy alone cannot trigger preparation|
|Only triggering ally becomes HP0/neutral/negative-coordinate before decision|Stay; HP8000 instead still passes the nonzero laser filter|
|After action6 selection all allies leave/die|Rebuild may hit only enemies or nobody; it does not retroactively Stay|
|Ally2/9 overlap and eligible enemy130 occupies the same square|Later enemy insertion overwrites ally occupancy during reconstruction|
|Move138 to5,21 with ally9 at6,21|Same Right direction, new origin; no initial-row legality restriction|

The shared48×48 ray ignores terrain/walls/crop, penetrates targets,
excludes these interior origins; valid overlapping occupants resolve by
later ID insertion. It is not a clipped safe edge API: processing precedes
the bound check. No general arbitrary-ordinal table guard exists.
Action6 base16 enters InflictDamage per sorted target: two ordinary
range3 variance draws yield12..16; forced debug Down can yield10 without
seed advancement. No ATT−DEF/resistance/MP/learned-spell or ordinary
double/counter calculation substitutes for it. HP is temporary during
construction, restored at End; emitted side-specific reactions later
publish damage/deaths. Enemy actions do not earn ally EXP/gold/drop via
their friendly fire. Forced debug overrides remain separate.

Kind93 selects message319 (Flower laser announcement, no control tokens),
animation15|80=95, enemy sprite49/palette0, and the sprite49 branch of
the shared Prism animation (property word4=40). Guard43 selects message321
and palette1 despite sharing sprite49; Zeon's presence does not change
this. SFX103/108 are requests, distinct from cutscene111. Actual pixels,
audio, animation RNG totals, service timing/replay completion remain
Unknown; no new renderer investigation.

## Outcomes, escape and the actual reset return

**Confirmed:** after an ordinary individual turn, BattleLoop runs defeated
hook→ProcessKilledCombatants→CountRemainingCombatants, tests allies0
before enemies0; if nonterminal, after-turn effects→cleanup→same tests,
then cursor+2. There is no second defeated hook after effects. Counting
scans30/32 slots with X.b≥0 and HP.w≠0; Y/neutral/activation/membership
are not filters. BowieHP0 overrides the ally count to0. The Battle5
position override does not match0. Leader-table0 disables sweep even
when128HP0 admits the empty defeated body; killing Zeon alone leaves
other countable enemies. Conversely enemies with negative X do not count
even with HP≠0. Both sides empty or BowieHP0 gives **Defeat first**.

Victory (`battle/battleloop_2.asm:$23CBA..$23D98`) heals living/immortal
allies again, stores live map in PARAM2, rebuilds party lists and reads
first active member X/Y plus crop by byte arithmetic into PARAM3/4;
physical alias facing into PARAM5; PARAM1=0. These are return parameters,
not a posted exploration event. Clear500 executes the full16-byte script
at `$4F47A..$4F48A`:

| Ordered command | Meaning / interface |
| --- | --- |
|textCursor466|Select dialogue cursor466 (hex01D2)|
|playSound MUSIC_ITEM|Request item music|
|nextSingleText FF,ENTITY_NONE|Display466: author paraphrase “game is over”; ordered tokens `{W1}`|
|hideText|Close text|
|csc_end|Return to wrapper|

No warp, population, join or item grant occurs **inside** abcs_battle00.
Held500 skips that body. Both paths then read actual join byte0 and
JoinForce(0): set Bowie's joined flag0, rebuild lists, set active32 only
when active count<12. Existing32 survives a full party; newly set32 is
absent from the immediately preceding list snapshot. Victory finally
rereads CURRENT_BATTLE, clears400, sets500, clears D0–D3 words, returns
PARAM2–5 low bytes and D4=1. Intro450 is untouched. Selector/gate changes
between services are not rolled back or latched to original0.

Defeat first calls UpdateBattleUnlockedFlag: clear400 only if live500
held. It requests sad music/text363 (`{LEADER}{W1}`), closes, restores
BowieHP=max, halves gold by unsigned long shift1, calls
GetEgressPositionForBattle and returns D4=−1. No new500, after scene/join
or general party resurrection; the special losable battle4 does not match.

Player escape requires reaching a live, actionable control turn. The
actual menu/input path (`battlefunctions_2.asm:ProcessBattleEntityControlPlayerInput`)
requires a learned selected spell, confirmed target and currentMP≥cost
(failure437 returns to selection), or a held selected item slot accepted
by IsItemUsableByCombatant plus target confirmation. Egress1 costs8,
range0/radius0, targets teammates; Angel Wing4 is a consumable using
Egress, range0. Range occupancy has its own HP≠0/neutral/coordinate filter.
Cancel/no target/insufficient MP does not admit a retreat. Sleep/Stun's
earlier skip is distinct. The source menu path has no local Silence gate;
the Egress special branch bypasses ordinary CastSpell's effect handling
despite AFFECTEDBYSILENCE metadata. A cheat/AI-forced action does not
prove those player-selection preconditions were met.

`executeindividualturn.asm` branches, rather than calls, to
ExecuteBattleaction_Egress or ExecuteBattleaction_AngelWing after action
kind/index checks. `battlefunctions_1.asm:$23D98..$23E50` consumes the
selected Wing slot **before** windows/text275 (`{NAME}{N}{ITEM}`), or
decreases MP by selected spell cost **before**274
(`{NAME}{N}{SPELL}{#}`), then common sound/flash/close/unlock update.
No additional MP/learned-spell/item possession check in these handlers.
The common tail performs **UNLK A6; pop one long return address into D0;
BSR GetEgressPositionForBattle; clear D4.W; RTS**. That discarded address
is the return from ExecuteIndividualTurn to BattleLoop. The final RTS
therefore returns to **EndGame's BSR BattleLoop** with D4=0. It bypasses
ordinary post-action/outcome tests, defeat gold/revival and victory flags.
This stack account, not MainLoop analogy, establishes this caller's escape.

GetEgressPositionForBattle (`$23E50..$23EB0`) matches none of0's fixed
town/forced38/39 branches and reads live EGRESS_MAP. GetSavepointForMap
(`code/common/maps/egressinit.asm:$75EC..$764E`) with399 held scans23
rows; unmatched map retains it and defaults1,1/Up. Map63 is absent from
that table. EGRESS_MAP is not necessarily63: battle load did not set it;
church/earlier state remain relevant. Supplied45 yields12,25/Up;
supplied63 yields63/1/1/Up. A changed clear399 yields3/56/3/Down.
Held64 separately resets raft coordinates using its own table. None of
these parameter calculations means EndGame loads the returned map.

| Stable500 / normal outcome | Return and flags before caller suffix |
| --- | --- |
|Clear /victory|Full after00→join0→clear400→set500; D4=1|
|Held /victory|Skip after00→join0→clear400→set500; D4=1|
|Clear /defeat|No forced400 clear; retain500 clear; Bowie revival/gold half; D4=−1|
|Held /defeat|Clear400, retain500; Bowie revival/gold half; D4=−1|
|Clear /admitted Egress or Wing|Pay MP/item; no forced400 clear/new500; D4=0|
|Held /admitted Egress or Wing|Pay MP/item; clear400, retain500; D4=0|

For **every actual return** above EndGame ignores D0–D4, requests
FadeOutToWhite, clears contextual VInts, disables display/interrupts,
writes SR2700, restores SP from InitialStack, loads p_Start and JMP(A0).
There is no return-value dispatch to field exploration or another battle.
Battle0's Suspend menu choice returns to @StartMenu without prompting,
saving, setting88, leaving the battle or entering WitchSuspend/reset.
It is not a selected nonreturning path. An interrupted scene, a waiting service, or a nonreturning
BattleLoop is not evidence that this suffix executed. Stop at this software
reset transfer; do not reopen the post-reset menu investigation.

Battle0 outcome flags/HP/gold/inventory are memory writes, not a SaveGame
call. The earlier EndGame SAVE_FLAGS bit7 write is separately
[SRAM-mapped](./ending-credits-finale-and-bonus-admission.md#witchend-text-and-completion-storage);
it is not a serialization of these later outcomes. Actual hardware/SRAM
retention and resulting menus after reset remain **Unknown**.

## Actual producer and production consumers

**Confirmed by current accepted-code read:** no selected export, lowering,
engine run or adapter observation is supplied here. Existing typed
mechanisms are reusable shapes, not a Battle0 package or native execution.

| Actual owner | Present expression / selected0 boundary |
| --- | --- |
|[Encounter preparation](../../src/sf2tool/remake_exploration_battles.py), [final producer](../../src/sf2tool/remake_exploration_content.py)|prepare_encounter admits2/3; route compilation can express wildcard admission,400/450/500, Empty hooks and after hooks/join0. Typed source-battle-load is fade→BattleLoad→fade, not native entity/stat reconstruction. No0 encounter/material package produced.|
|[Encounter reader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [scenario assembly](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Pin/admission and region-program table restrict1/2/3; SENTRY8 has no assembly mapping. Definitions retain raw metadata, but ENHANCED, equipped/cursed loadouts and Galam/Zeon AGI refresh need real conversion. Raw low7 splitting would use75/80 rather than current72/62.|
|[Initialization](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs), [source AI](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs)|Formation/STARTING/priority/regions exist within admitted roster/difficulty0/unequipped inputs. Source AI permits6/7/9/14 and restricted physical loadouts; no SENTRY8 or kind93 laser bypass. HP/status/region and native stale-slot/extra160 rules are not all bound.|
|[Action state](../../remake/src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs), [advancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs)|Stay/Healing/Physical/Item and automatic movement/action continuation exist; not action6 ally-only preparation/all-slot rebuilt multi-target/friendly-fire laser or all boss spells. Generic reaction arrays do not implement target selection/RNG/presentation.|
|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py), [Content instructions](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|Text cursor/music/dialogue/close/end have typed forms suitable for after00; native-call becomes UnsupportedInstruction and fails when reached. Predecessor EndGame global-label/native truncation and wrapper exception remain; no whole EndGame or reset consumer binding.|
|[Battle entry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [outcome](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs)|Ordered before/load/start and outcome heal/join/flag phases exist. F88 new-entry refuses. Outcome continues through ActiveExploration/return-map programs; it is not this EndGame return-address discard/reset policy. Ordinary return support cannot establish0's caller behavior.|
|[Battle scene view](../../remake/game/src/Battles/BattleSceneView.cs), [presentation](../../remake/game/src/Battles/BattlePresentation.cs), [viewport](../../remake/game/src/Battles/BattleMapViewport.cs), [capability owner](../../remake/docs/capability-status.md)|Existing map/HP/text/audio/scene projections need admitted selected content and laser95/message319/sprite49/palette0/SFX103/108 bindings. No native special47 allocation, ray, reset hardware or0 playability is proved by their presence.|

**Inferred:** future integration can reuse formation/region/turn/outcome
and typed after00 primitives after binding selected content, refreshed
definitions/loadouts, SENTRY and laser action production, physical identities
and this reset caller policy. This is a gap description, not implementation
authorization or an automatic runtime queue.

## Reproduction, retained failures and stop

The original candidate's `local/issue1194/review.py` uses Python standard library only;
there are no imported local helpers. It checks clean source pin, selected
complete rows/definitions and source→use-site→prose joins, critical ordered
instructions/flag/stack branches, changed-input implications, actual consumer
refusals, links/UTF8/LF and exact two-path/navigation-only/private diff.
It writes a fresh receipt and retains bounded source/consumer sections;
it does not simulate the battle or add tests of verification programs.
One-off checks are not maintained mutation coverage or proof every prose
claim is correct; independent source/consumer review remains necessary.

Independent source review found a P2: the original prose did not identify
the Battle0 Suspend rejection, so its generic suspension wording could
imply a normally available exit. Original author direct05 and committed
direct checks, main's reproduction-main-01 PASS and exact-head CI passed
without covering this gate. Those completed PASS results remain preserved
with the omission. The corrected F88 and outcome paragraphs bind the
zero-battle branch to its actual menu boundary. The self-contained
`local/issue1194/focused-correction.py` checks only that source branch,
Battle0's zero constant, corrected prose, retained results and exact owned
delta/links/UTF8/LF/private boundary. It does not rerun full review,
capture, lowering or unrelated checks; no local helpers are imported.

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1194/focused-correction.py <configured-source-disasm-root> --receipt local/issue1194/focused-main-01.json --committed
git diff --check origin/main
uv run sf2 verify plan --base origin/main --head HEAD
```

Source argument ends in disasm; receipt **must not exist**. Each uv launch
loads current private configuration in the same process. Exact committed
head/tree, plan/actual scoped CI (including skips), resources/processes,
self-review corrections and any failed checks belong to the frozen Issue/PR
handoff. Normal/full/SDK/Godot/emulator/H3/H4/screenshots/whole-world/old
recipes/comparisons are **SKIPPED**, not passes. New maintained parser,
schema/fixture/mutation-test and H3 callback obligations are N/A.

Preserve [#1192's complete retained record](./ending-credits-finale-and-bonus-admission.md#reproduction-retained-results-and-open-boundary):
Elis131 speed20 prose P2, original author/main direct PASS and CI omission,
focused correction/ACCEPT, common-map completed-flag clarification;
direct01's missing N in238 FAIL, focused01 boundary-newline FAIL,
IntroCutscene1 ValueError/partial and native/global-label truncation.
Carry #1190 orientation/FACING, #1188 search-kind/text/interaction-wrapper,
#1184 physical-timer/CURSE P2, prior PASS omissions and accepted focused
corrections, HP/rawchoice/RotateRight/native gaps and the twelve completed
comparison failures/Unknowns through their named owners. No old recipe,
capture, lowering, render, review or completed suite is rerun/reclassified.

**Unknown:** natural entry, live party/HP/status/flags/crop/aliases/queue,
RNG/debug/control input, concrete full boss choices, intermediate service
mutation/return/interruption, movement/scene/view/fade/text/audio completion,
decoded graphics/terrain and collision, VInt/DMA/hardware timing, replay/save
persistence through reset, source-ROM equality, original/remake playability
and full suspended restoration. None is silently filled by a supplied
static state or this complete battle-data/return account.

Freeze a clean committed/pushed Draft PR, exact scoped CI and complete
handoff; move Project to Review / Verify and notify main-gate, then stop
writer. Same task owns corrections; independent review/merge remains with
main-gate. No merge, cleanup, archive, ref deletion or next-slice dispatch.
