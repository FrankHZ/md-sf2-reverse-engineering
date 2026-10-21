# Battle38 activation, hidden spawning and first control dispatch

Evidence date: 2026-10-21. **Confirmed (static):** all twelve unchanged
formation points miss all three integer regions. Eleven STARTING enemies
remain initialized; nine HIDDEN Burst Rocks remain off-map with HP0
until the dead/held-trigger/maxHP0 and unoccupied-start gates admit them.
Successful reset restores the full activation word0201, then queues an
appearance request. Geshp's current raw AGI191 produces two turn records,
using numerical bases63 and52 and five sequential draws. No fixed first
actor follows. Follow live control predicates to the ordinary command
interface, or a spawned Burst Rock's **ProcessExploderAi entry**, then
stop. Natural reach, original services, usable control and persistence
remain **Unknown**. This does not establish playable Battle38.

## Agreement, provenance and budget

Standard rigor for [Issue #1146](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1146)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
continue accepted [Battle38 initialization](./battle38-initialization-and-opening.md)
at base `ef0f2be59e9e261d6b6c9654ee86183c66388bfa` through activation,
hidden reset/appearance interfaces, turn generation and first control.
Own this document and navigation only in that predecessor. No shared
registry/index, parser/schema/fixture, design or engine changes; no
competing writer or stacked dependency. Explosion target selection,
explosion RNG/action/damage, individual command or movement execution,
whole rounds, outcomes and neighbouring story are later scopes.
Supplied positions, seeds and formation counts do not define legality.

Reuse [Battle37 activation/dispatch](./battle37-activation-and-first-turn-dispatch.md),
[Battle35 hidden spawning](./battle35-activation-and-first-turn-dispatch.md),
[Battle30 integer geometry](./battle30-activation-and-first-turn-dispatch.md#geometry-checked-bits-and-persistent-flags),
[Battle26 alias/RNG seams](./battle26-activation-and-first-turn-dispatch.md),
[battle loop](./battle-loop.md), [RNG/math](./runtime-rng-and-battle-math.md),
[spritesets](./battle-spriteset-data.md) and
[lifecycle](../design/contracts/battle-control-lifecycle.md).
Those owners retain shared mechanisms; this owner checks selected38
operands, changed states and actual consumer boundaries without rerunning
the opening lowering or exporting definitions.

Original paths are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battleloop/`, `ai/`, `battlefunctions/` and `cutscenes/` abbreviate
`code/gameflow/battle/` children; `stats/` means `code/common/stats/`.
sf2const.asm/sf2enums.asm and macros own constants, widths and encodings.
Fnnn means flag nnn, ordinals are zero-based, intervals half-open,
activation words hexadecimal. USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new source/ROM equality or original-runtime observation is claimed.

Minimum evidence is selected instructions/tables, their actual consumers,
36 formation predicates,22 changed coordinates, bounded spawn states
and supplied seeds. Reuse the immutable18,285,979-byte init/script pair
with `local/issue1005/pair.txt` provenance. One pair load, linear selected
reads and constant-size arithmetic target one minute/512MiB per command;
peak memory is unmeasured. Small unique receipts stay in ignored
`local/issue1146/`. No acquisition, map import, repeated opening lowering,
definition export, new verification platform/helper tests, normal/full/old
comparisons, SDK/Godot/emulator/H3/H4 or screenshots. Reassess changed
prerequisites or a material contradiction before expanding.

## Fresh order versus retained F88 entry

**Confirmed:** `code/gameflow/battle/battleloop_1.asm:BattleLoop`
(`$23A84..$23BB4`) reaches @Start after fresh LoadBattle and empty start.
The predecessor owns map66/crop17,0/32×32, flag488 hooks, flag90–105
clearing and reconstruction. Scene Geshp128 is replaced by combat128;
scene coordinates do not enter this geometry. From @Start:

1. ActivateEnemies completes its32-slot scan128–159 and falls into its
   helper once for160.
2. ExecuteBattleRegionCutscene rereads CURRENT_BATTLE.
3. Nonzero DEBUG_MODE_TOGGLE.b calls PrintAllActivatedDefCons.
4. PopulateTargetsListWithSpawningEnemies rebuilds the word length.
   Nonzero length sends ascending byte IDs, with cleared d0.w, to
   SpawnEnemyWithCamera; each call must return before the next.
5. GenerateBattleTurnOrder fills/sorts the buffer and clears its cursor.
6. @IndividualTurns_Loop clears d0.w, reads CURRENT_BATTLE_TURN.b
   as a byte offset into BATTLE_TURN_ORDER and reads the actor byte.
   ActorFF repeats @Start; another value calls ExecuteIndividualTurn.

Flags, checked bits, reset stats and service writes have no transaction
or rollback. A completed spawn-list build does not prove an appearance,
and F488 does not prove the earlier initialization/start services finished.

Held F88 instead restores seconds, clears88/AI memory/view target, calls
LoadBattle without clearing incoming d0 and jumps directly to the retained
individual-turn loop. It skips this initial activation/spawn/generation
chain. The predecessor proves incoming66 reads alias byte `$FFB182`,
beyond `$FFB140..$FFB180`, with no supported named ownership/value at182.
It is neither an installed selector nor safe restoration. Retained map/
battle/crop, combatants, flags, checked word, aliases, queue/cursor and RNG
remain inputs. Odd offsets read score bytes;128–255 are beyond the128-byte
turn buffer. RetainedFF may later reach @Start with then-current state.
Stop at its reached interface without inventing a valid retained battle.

## Three integer regions and checked state

**Confirmed:** `battleloop/activateenemies.asm` (`$2550C..$25544`)
rejects signed-negative X.b, zero currentHP.w and nonnegative actor d0.b.
It has no Y, membership, neutral, already-active or order gate. All eleven
unchanged STARTING enemies pass; hidden139–147 and absent148–159 fail X.
A nonzero HP8000 passes here, unlike the geometry's signed-positive test.

The first passing enemy reaches
`battleloop/triggerregions.asm:TriggerRegionsAndActivateEnemies`
(`$1AC9FC..$1ACF30`). For each declared region a clear bit in
NEWLY_TRIGGERED_BATTLE_REGIONS.w (`$FFB20C`) permits geometry; a hit sets
F90+ID. **Miss and hit both set the checked bit.** An already checked
region can still activate enemies through an independently held flag.
ClearAiMemory resets48 last-target bytes and48 memory bytes ending at
`$FFB20C`; neither fresh nor F88 clears that adjacent word. Fresh clearing
of flags90–105 is a separate operation.

`data/battles/spritesets/spriteset38.asm` is12/20/3/1,426 storage bytes.
Regions at relative388, absolute `$1B63D4`, each occupy12 bytes: type4,
unused0, four ordered vertices and trailing0,0. Point0 is7,27.

| Region / flag / type | Point1 | Point2 | Point3 | Point4 |
| --- | --- | --- | --- | --- |
|0 /90 /4|0,13|0,31|31,31|31,13|
|1 /91 /4|0,19|0,31|31,31|31,19|
|2 /92 /4|0,24|0,31|31,31|31,24|

Geometry uses crop-relative coordinates without adding world origin17,0.
Type4 tests triangle1/2/4 then3/2/4 after a miss. Each scans all30 ally
slots, requiring signed HP.w>0 and signed X.b/Y.b>=0. Joined/active force
membership, neutral state, terrain/collision and enemy occupancy are not
geometry predicates. A positioned living nonmember can trigger; HP0,
HP8000–FFFF and negative X/Y cannot, while another eligible slot can.
Fresh placement supplies a subset of these twelve declared points, not
an immutable twelve-actor snapshot:

| Formation | X,Y | R0 | R1 | R2 |
| --- | --- | --- | --- | --- |
|0|13,11|out|out|out|
|1|14,11|out|out|out|
|2|14,10|out|out|out|
|3|13,10|out|out|out|
|4|14,9|out|out|out|
|5|13,9|out|out|out|
|6|12,9|out|out|out|
|7|11,9|out|out|out|
|8|12,11|out|out|out|
|9|11,10|out|out|out|
|10|12,10|out|out|out|
|11|11,11|out|out|out|

The shared helper sign-extends coordinate bytes, chooses the horizontal
branch on equal absolute deltas, uses signed MULS/DIVS with truncation
toward zero and adds the projection to endpoint B. Bounded collinearity
result1 immediately accepts; otherwise triangle edges1→2,3→1,3→2 compare
the opposite vertex's side flags. Negative projection synthesizes2
although the source comment says3. Signed region-index cmp.b/ble admits
equality with count, while FF separately rejects; normal iteration uses
0–2, not3. Selected vertices are nonnegative and nondegenerate.

| Supplied X,Y | Hit region IDs | Supplied X,Y | Hit region IDs |
| --- | --- | --- | --- |
|0,12|—|0,13|0|
|31,13|0|32,13|—|
|10,12|—|10,13|0|
|14,15|0|0,18|0|
|0,19|0,1|31,19|0,1|
|32,19|—|12,20|0,1|
|0,23|0,1|0,24|0,1,2|
|31,24|0,1,2|32,24|—|
|9,24|0,1,2|10,26|0,1,2|
|0,31|0,1,2|31,31|0,1,2|
|31,32|—|13,24|0,1,2|

These probes agree with inclusive rectangles x0..31, y13/19/24..31.
The actual operation is still integer triangle code; no generic polygon
parity follows. Rock140's declared10,12 is outside its trigger0: the
trigger names admission, not containment of its own spawn position.
Coordinates above prove predicates only, not walkability or natural moves.

With unchanged completed fresh placement and clear region flags, any
incoming checked word yields no held selected flag and ends as
`incoming OR $0007`. Moving later does not alone recheck a missed bit.
Supply eligible ally13,24, other eligible allies at a formation subset:
checked0 permits flags90/91/92; checked0007 with flags clear permits none.
With checked0007 and held90, hidden group0 may still enter the dead helper.
At enemy StartAiControl @NonSwarmAi, checked clears **before** the
activation guard without clearing flags. None of the selected sets is
SWARM15. Ally AI bypasses this reset. A later recheck depends on actually
reaching enemy preparation, not merely advancing a nominal round.

## All activation rows, absent slots and160

**Confirmed:** `stats/combatantstats_1.asm:GetTriggerRegions` returns
primary high/secondary low nibbles. Both15 preserves the whole activation
word; held non15 primary wins and ORs1, otherwise held secondary ORs3.
An existing secondary bit survives primary activation. Misses preserve
the word and no branch deactivates. Definition priority/filler and the
dedicated AI-commandset nibble are separate fields.

The first eleven words are accepted STARTING results; BIT4 did not
survive that initialization. HIDDEN rows instead start0200 with HP/maxHP0
and X−1, so ActivateEnemies skips them. Their dead helper, below, can
set0201 from a held trigger even before reset is allowed:

| Row / ID | Definition | Commandset | Primary order / trigger | Secondary order / trigger | Initial = unchanged result | Changed held flag result |
| --- | --- | --- | --- | --- | --- | --- |
|0 /128|GESHP|LEADER|MOVE_TO\|0 /15|NONE /15|`1000`|no activation write|
|1 /129|DARK_GUNNER|ATTACKER3|FOLLOW_ENEMY\|0 /15|NONE /15|`2000`|no activation write|
|2 /130|WHITE_DRAGON|ATTACKER3|FOLLOW_ENEMY\|0 /15|NONE /15|`2000`|no activation write|
|3 /131|CYCLOPS|ATTACKER3|FOLLOW_ENEMY\|0 /15|NONE /15|`2000`|no activation write|
|4 /132|MIST_DEMON|HEALER3|FOLLOW_ENEMY\|5 /15|NONE /15|`2000`|no activation write|
|5 /133|CHAOS_WIZARD|SENTRY|NONE /15|NONE /15|`2000`|no activation write|
|6 /134|EVIL_BISHOP|HEALER4|FOLLOW_ENEMY\|5 /15|FOLLOW_ENEMY\|0 /15|`3000`|no activation write|
|7 /135|HELL_HOUND|ATTACKER1|NONE /15|NONE /15|`2000`|no activation write|
|8 /136|MINOTAUR|ATTACKER1|NONE /15|NONE /15|`2000`|no activation write|
|9 /137|DARK_GUNNER|ATTACKER1|NONE /15|NONE /15|`2000`|no activation write|
|10 /138|WHITE_DRAGON|ATTACKER1|NONE /15|NONE /15|`2000`|no activation write|
|11 /139|BURST_ROCK|ATTACKER1|FOLLOW_TARGET\|0 /0|NONE /15|`0200`|dead helper F90→`0201`|
|12 /140|BURST_ROCK|ATTACKER1|FOLLOW_TARGET\|0 /0|NONE /15|`0200`|dead helper F90→`0201`|
|13 /141|BURST_ROCK|ATTACKER1|FOLLOW_TARGET\|0 /0|NONE /15|`0200`|dead helper F90→`0201`|
|14 /142|BURST_ROCK|ATTACKER1|FOLLOW_TARGET\|0 /1|NONE /15|`0200`|dead helper F91→`0201`|
|15 /143|BURST_ROCK|ATTACKER1|FOLLOW_TARGET\|0 /1|NONE /15|`0200`|dead helper F91→`0201`|
|16 /144|BURST_ROCK|ATTACKER1|FOLLOW_TARGET\|0 /1|NONE /15|`0200`|dead helper F91→`0201`|
|17 /145|BURST_ROCK|ATTACKER1|FOLLOW_TARGET\|0 /2|NONE /15|`0200`|dead helper F92→`0201`|
|18 /146|BURST_ROCK|ATTACKER1|FOLLOW_TARGET\|0 /2|NONE /15|`0200`|dead helper F92→`0201`|
|19 /147|BURST_ROCK|ATTACKER1|FOLLOW_TARGET\|0 /2|NONE /15|`0200`|dead helper F92→`0201`|

All selected secondary triggers are15. Evil Bishop134's secondary
FOLLOW_ENEMY0 is an order, not a second region. Changed primary0/
secondary1 with only91 held yields wordOR3; both held choose primaryOR1.
Existing0203 with90 held remains0203, not0201. These are supplied row/
state changes, not selected38 secondary admission.

Point order$40 selects7,27, Geshp's own start. FOLLOW_ENEMY0/5=$80/85
select combatants128/133, not scene128 or physical entity offsets.
Ordinary nonpoint follow preparation reads the full order as the target
and promotes secondary to primary, secondaryFF, when its live HP is0.
For134 this can yield80/FF after133 dies. The rock's special branch
precedes ordinary follow handling; no rock move-order execution is inferred.

Absent148–159 have activation0000, HP/maxHP0 and X−1, with triggerFF
from the unoffset first declared row. Other fields may be stale. Merely
changing their HP/X neither defines their AI/stats/orders nor makes them
declared enemies.148–157 can enter the turn scan if those live gates pass;
158/159 do not. Y−1/neutral/out-of-crop positions can pass activation/
turn admission while packing and consumers use different guards.

After32 iterations, d0.w=160 falls into the helper. Reuse the accepted
[RAM/deals derivation](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias):
GetCombatantEntryAddress admits160 and resolves it to CURRENT_GOLD
`$FFF600`. Pseudo-X `$FFF62E` packs deals84/85; pseudo-HP `$FFF60E`
packs20–23; pseudo-trigger `$FFF636` packs100/101; pseudo-activation
`$FFF634` packs96–99. Nonnegative X/nonzero HP plus a held non15
trigger can OR1/OR3 into the Goddess Staff99 count nibble. Unchanged
fresh38 leaves selected flags clear, so does not cause that write;
changed held90–92 can matter. This is conditional alias arithmetic,
not item delivery, a safe combatant or persistence.160 never enters
normal spawn/turn scans.

## Hidden reset, retries and ascending appearance list

**Confirmed:** `battleloop/populateenemyspawns.asm`
(`$1ACF30..$1AD090`) scans128–159, masks activation0F00 and rebuilds
TARGETS_LIST_LENGTH.w. STARTING words1000/2000/3000 and their active
variants mask to0; unchanged hidden0200 takes the HIDDEN branch:

1. UpdateEnemyActivationIfDead requires HP0, then checks held non15
   primary, else secondary flag. It ORs1/3 into the **live full word**
   and returns d0=0, otherwise−1. It has no position/neutral gate.
2. Only then test maxHP.w=0. Thus a dead rock with maxHP45 can become
   active without a reset/append. HP>0 skips before maxHP; unheld
   triggers skip even if old active bits happen to be present.
3. Read the now-current activation word and call ResetSpawningEnemyStats
   in `battleloop/initializecombatants.asm`. Its live enemy count20 OR80
   gives **exclusive bound148**; IDs148–159 refuse before row lookup.
4. For declared IDs, fetch that row's starting X/Y. Occupancy compares
   both coordinates over30 allies and32 enemies, including the subject,
   without HP, neutral, membership or self exclusions. A dead subject
   still on its own start blocks itself.
5. A refused/occupied reset clears maxHP/currentHP, sets X−1 and carry;
   Y, already-set activation bits and other stale fields survive.
   Carry skips append. A later held-trigger scan may retry after the
   blocker moves; a self-blocked rock is now off-map and may retry too.
6. Success calls InitializeEnemyStats, restores the saved **full** word,
   then appends the ID byte and increments the word length. The returned
   d1 remains full0201 rather than remasked0200; following0100/0300
   comparisons do not append this selected row again.

Other modes remain distinct:0100 requires HP0 without the held-trigger/
maxHP0 pair;0300 requires dead+held-trigger without the one-time maxHP0
gate. They are not selected38 declarations. Unsupported modified modes,
absent slots and interrupted data are not granted a safe state.

Successful reset now copies56 definition bytes/14 longwords and fills
HP45/MP0, position, REGULAR/ATTACKER1, definition32, selected orders and
trigger0/1/2 with secondary15, initial status0/class99, four NOTHING
items/spells and refreshed current stats. The temporary initialized word
is2200 (priority2 plus mode2); final word is saved0201, not2201 or the
raw definition2020. Difficulty scales ATT0 to0; current DEF60/AGI43/MOV4
remain as declared with status0 and empty equipment. The earlier hidden
branch did not install any of these fields. No death or explosion follows
from initializing them.

| Hidden ID | Restored starting X,Y | Primary region / flag | Saved word in base success |
| --- | --- | --- | --- |
|139|14,15|0 /90|0201|
|140|10,12|0 /90|0201|
|141|11,14|0 /90|0201|
|142|12,16|1 /91|0201|
|143|15,18|1 /91|0201|
|144|12,20|1 /91|0201|
|145|12,22|2 /92|0201|
|146|9,24|2 /92|0201|
|147|10,26|2 /92|0201|

InitializeEnemyStats checks UpgradeRandomBattleEnemies before copy for
each reset. With live CURRENT_BATTLE=38, the complete11-entry random list
`3,8,9,10,14,17,19,21,24,26,35` misses38 before completed538/level/category/
upgrade RNG work. Holding538 alone does not change this. Selector changes
within services remain live inputs; no immutable roster/upgrade snapshot
is asserted.

Supply one eligible ally13,13, other eligible positions at a formation
subset, checked0/flags clear, and no other occupants at hidden starts:

| Held flags after geometry / supplied variation | Queued IDs |
| --- | --- |
|13,13 →90; group0 clear|139,140,141|
|13,19 →90,91; groups0/1 clear|139,140,141,142,143,144|
|13,24 →90,91,92; all starts clear|139,140,141,142,143,144,145,146,147|
|13,24; checked0007 and flags clear|none|
|Original formation; checked0007, independently held90|139,140,141|
|13,13; a dead/neutral slot occupies10,12|139,141;140 fails after0201|
|13,13; all three group0 starts occupied|none;139–141 retain0201/HP0/maxHP0/X−1|
|13,13;139 dead with maxHP45, other two fresh|140,141;139 can become0201 but fails one-time gate|
|13,13;139 already alive, other two fresh|140,141;139 skips dead helper|

These states do not simulate movement. Distinct9 starts avoid the eleven
STARTING coordinates and these triggering positions. After any successful
base reset, maxHP45 blocks another HIDDEN spawn even if HP later becomes0,
unless supplied state also returns maxHP to0. A blocked row keeps0 and
can retry. During a partially interrupted successful reset, fields may
have changed before activation restoration/list append. After full list
construction, stats for all admitted rows already exist before the first
appearance call; later presentation failure cannot roll them back.

SpawnEnemyWithCamera clears d1 and enters SpawnEnemy
(`battleloop/spawnenemy.asm`, `$2448A..$24512`).
`code/common/scripting/entity/entityfunctions_1.asm:SpawnEnemyEntity`
reads actual X/Y/mover/mapsprite, masks coordinates3F, scales by384,
sets facing3/eas_Idle and aerial layer only for FLYING/HOVERING. A base
rock uses REGULAR/mapsprite96, not Geshp159 or an earlier scene identity.

GetEntityEvent scans64 alias bytes with a signed-byte maximum, appends
max+1, writes enemy alias offset ID−96, then DeclareNewEntity writes the
physical record. It does not choose the lowest hole or enforce49-record
capacity. With A normally packed allies, exactly eleven initial enemies,
no extra entities/changed mappings, all nine successes allocate physical
A+11..A+19, alias offsets43..51; at A=12 the last is31, within49 records.
A partial list allocates consecutively in successful-ID order, without
holes for rejected IDs. Arbitrary retained/malformed mappings do not
inherit this capacity argument. Stats may already be reset when later
mapping/allocation fails. Sprite clear, ApplyVIntVramDma and
WaitForDmaQueueProcessing are subsequent required-return interfaces.

SpawnEnemy requests cursor radius0, view/cursor target, cursor/view waits,
hide cursor, twelve orientation/property/Sleep3 iterations, SFX_SPAWN,
text397 then close. txt397 uses DIALOGUE_NAME_INDEX_1=combatant; its
short authored role is named arrival, complete tokens
`["{CLEAR}","{NAME}","{D3}"]`. No scene portrait/speaker is fixed here.
Outer d7/a0 preserve the list loop. These requests do not prove completed
camera/sprite/DMA/audio/text, hardware duration or usable input.

ExecuteBattleRegionCutscene (`$47E82..$47EC8`) scans the complete table:
32/0/F386 and32/1/F386→rbcs_battle32;40/0/F387→rbcs_battle40_1;
40/1/F388→rbcs_battle40_2; FFFF terminates. Live38 misses before played/
region checks: no selected region program exists to lower. Changed32/40
can match; played check precedes region and SetFlag precedes MAPSCRIPT.
PrintAllActivatedDefCons checks flags90–105 in order, requests txt463
for held flags with DIALOGUE_NUMBER=0–15 and closes. Complete tokens
`["{#}","{N}","{D3}"]` express a diagnostic region number, not new
story dialogue. Unchanged fresh38 requests none; the all-group variant
requests0,1,2. Held undeclared flags may produce other numbers.

## Live AGI, sequential draws and turn consumption

**Confirmed:** accepted definition/current-stat refresh gives Geshp180HP,
status0/raw currentAGI191 ($BF). FLAIL/BUSTER_SHOT/GREAT_ROD/ATLAS_AXE
have ATT-only equipped effects and difficulty changes ATT, not AGI.
Prowess DOUBLE_1IN8 is a separate within-action mechanic. The current-stat
helper's increase clamps low seven bits and restores bit7; decrease uses
the full byte with cap200. General changed status/equipment can therefore
change numerical AGI and high-bit coupling. Generation reads currentAGI,
not an immutable definition table. Rock values below apply only **after**
a successful base reset; unspawned stale fields are not current stats.

| Definition / IDs | Current HP | Raw AGI | First range | First score envelope |
| --- | --- | --- | --- | --- |
|GESHP /128|180|191|7|56..70|
|DARK_GUNNER /129,137|60|45|5|40..50|
|WHITE_DRAGON /130,138|66|53|6|47..59|
|CYCLOPS /131|64|46|5|41..51|
|MIST_DEMON /132|68|51|6|45..57|
|CHAOS_WIZARD /133|53|59|7|52..66|
|EVIL_BISHOP /134|64|46|5|41..51|
|HELL_HOUND /135|62|50|6|44..56|
|MINOTAUR /136|58|44|5|39..49|
|BURST_ROCK /139,140,141,142,143,144,145,146,147|45|43|5|38..48|

`battleloop/turnorderfunctions.asm` (`$25544..$25610`) clears64 two-byte
records at `$FFF71A` toFFFF. It offers allies0–29 then enemies128–157,
ascending within each group. Reject signed-negative X.b or zero HP.w;
no Y, membership, neutral, active, sleep/stun or signed-positive-HP gate.
Successful hidden reset admits that rock regardless of independently
unobserved appearance completion. Hidden/absent X−1 reject;158–160 are
never offered. Mere HP/X changes do not repair malformed aliases.

Let A=currentAGI&127 and R(n) mean the next direct GenerateRandomNumber
draw. First score=`A+R(A>>3)−R(A>>3)+R(3)−1`. Raw byte>=128 appends
an immediate second record with B=floor(A×5/6), score=`B+R(B>>3)−R(B>>3)`.
Store the low score byte after word arithmetic. For Geshp **A63/B52**,
ranges7,7,3,6,6 occur in order; first envelope56..70, second47..57.
These are conservative algebraic bounds, not independent attainable RNG
extrema or probabilities. Extra draws precede the next candidate.

For these small ranges the direct generator updates
`seed=(seed×13+7)&65535` and returns floor(seed×n/65536). Range0 still
updates the seed. This bypasses direction/debug overrides and the
thinking RNG copy. Singles draw3, high-bit candidates5, rejected0.

Unchanged enemies yield **12 records/35 draws**: ten singles and Geshp's
two records. With S successful base rocks, N eligible normally placed
allies and T of those with rawAGI>=128, totals are `12+S+N+T` records
and `35+3S+3N+2T` draws. Twelve high-bit allies bound unchanged placement
at **36 records/95 draws**, or all-nine success at **45 records/122 draws**.
For arbitrary changed eligible count C/high-bit count D use C+D and
3C+2D instead. Earlier debug/spawn/service draws are outside these totals;
no incoming seed is established.

| Supplied generation seam | Appended records actor:score | Seed after |
| --- | --- | --- |
|128 alone/raw191/seed0|128:62;128:52|19979|
|128 alone/raw191/seed1|128:62;128:51|63592|
|128 alone/raw191/seed65535|128:64;128:53|41902|
|128 alone/raw191/seed7|128:62;128:48|63126|
|128 alone/raw63/seed0|128:62|1281|
|128 HP0 or X−1/raw191/seed0|none; no draws|0|
|128 Y−1 or HP8000/raw191/seed0|128:62;128:52|19979|
|ally0 raw50 then128 raw191/seed0|0:49;128:63;128:51|51560|
|ally0 raw80 then128 raw191/seed0|0:79;128:63;128:51|51560|
|ally0 alone/raw0/seed0|0:255 (word−1)|1281|

Geshp-alone seed0 advances7,98,1281,16660,19979, returning0,0,0,1,1.
The preceding ally changes Geshp's stream; active bits alone change no
turn admission. With unchanged enemies only, supplied seeds0/1/65535
produce12/35, first128:62/62/64. Successful group0 gives15/44; blocked140
with139/141 successful gives14/41; all-nine gives21/62, at those seeds
also first128. These isolate generation with a supplied incoming seed,
not actual fresh-party states or seeds preserved across spawn services.
The raw80 ally variant sorts0:79 first; no LEADER/191 rule fixes control.

Sorting performs62 passes of63 adjacent **signed score-byte** comparisons
over64 records, swapping only if right>left. Ties preserve append order:
allies before enemies, lower offered IDs first, an actor's first before
its immediate second when equal. Padding255 is−1; wrapped135 is−121.
Actor0 score255 is not actorFF. Negative scores may follow padding and
an encountered FF can stop consumption before them. No native capacity
guard exists; arbitrary30+30 high-bit candidates overflow64 slots, and
62 passes do not promise fully sorted arbitrary64-entry inputs. The
bounded selected45 maximum does not license those malformed states.

Generation clears CURRENT_BATTLE_TURN.b at `$FFF79A`, just after the128-byte
buffer. Fresh dispatch reads offset0. Empty eligibility seesFF and repeats
@Start. A queued actor's later HP is read again; two records are not two
reserved completed actions. Cursor advances by2 only after returning
through BattleLoop's downstream checks/after-turn interface, below.

## First living/control and special Burst Rock entrance

**Confirmed:** `battlefunctions/executeindividualturn.asm`
(`$23EB0..$24242`) sets the battlefield frame counter, clears speech SFX,
allocates its frame, masks actor with COMBATANT_MASK_ALL and clears the
dead list. Intact38 does not select the Taros/Bowie/F112 branch.
Live HP0 skips to @Done and returns without advancing the cursor.
BattleLoop still passes debug/death-processing/end checks and after-turn
interfaces before adding2; those bodies/results stay excluded. An actor
may die after generation or between its two entries.

Nonzero HP publishes actor/target X/Y, clears cursor radius, resolves
entity alias/view target and sets cursor destination. There is no new
position/neutral/capacity repair. The live dispatch predicates are:

1. MUDDLE$30 or AI_CONTROLLED$0004 selects StartAiControl.
2. Otherwise an enemy with CONTROL_OPPONENT_TOGGLE=0 selects AI;
   nonzero permits player. An ally with AUTO_BATTLE_TOGGLE nonzero
   selects AI; zero permits player.
3. Returning preparation joins @PlayerControl setup: cursor/view waits,
   targeting reset, moving entity, mini-status/land-effect windows,
   movement range/blocks and hide cursor. Its label does not prove
   human control.
4. Live SLEEP$C0 then STUN bit0 selects @NoAction, potentially after AI
   preparation. Control predicates are read again before
   ProcessBattleEntityControlPlayerInput or ExecuteAiControl.

At these interfaces stop before input/AI execution, movement/action,
no-action cleanup, after-turn effects or battle scenes. Intervening
status/control changes can alter dispatch; windows/waits/range requests
do not prove completed services.

`ai/startaicontrol.asm:StartAiControl` (`$DEFC..$E1AC`) sends ally AI
straight to ATTACKER1, bypassing the enemy checked reset. Enemy AI reads
the dedicated commandset nibble. Selected sets are not SWARM15, so
@NonSwarmAi clears checked, then15/15 bypasses active testing; any non15
trigger with primary active bit0 clear takes standby/STAY. Secondary bit1
alone does not pass; normal secondary activation sets both bits.
All eleven STARTING38 rows pass15/15 with their unchanged words. A
successfully triggered/reset rock normally passes with0201. A changed
unactivated rock can instead reach standby; no special-kind bypass of
that activation guard exists.

At @HandleActivatedCombatant, GetEnemy checks PRISM_FLOWER93, ZEON_GUARD,
then **BURST_ROCK32**. Rock32 calls **ProcessExploderAi** and branches
@Done before ordinary order/pathfinding/commandset handling. Stop at the
call's entry: no explosion targeting, RNG, selected action or damage is
investigated or simulated here. ATTACKER1/6 in the row does not make this
ordinary ATTACK1 dispatch. Opponent-control permits a player interface
only when the earlier forced-AI/status conditions do not override it.

Geshp95 has no special-kind branch and reaches ordinary LEADER14.
After dead-follow promotion, current commandset picks pathfinding mode
and `data/battles/global/aicommandsets.asm` (`$E1AC..$E25B`) pointers:

| Selected set / number | Pathfinding mode | Ordered command list |
| --- | --- | --- |
|HEALER3 /2|BLOCK_NON_MOVABLE|HEAL2,MOVE_ORDER4,SUPPORT,ATTACK3,MOVE2,STAY|
|HEALER4 /3|BLOCK_NON_MOVABLE|HEAL2,MOVE_ORDER4,SUPPORT,ATTACK3,MOVE1,STAY|
|ATTACKER1 /6|REGULAR|ATTACK1,HEAL1,SUPPORT,MOVE1,STAY|
|SENTRY /8|REGULAR|ATTACK1,HEAL1,SUPPORT,STAY|
|ATTACKER3 /9|BLOCK_NON_MOVABLE|MOVE_ORDER3,ATTACK3,HEAL2,SUPPORT,MOVE_ORDER4,MOVE2,STAY|
|LEADER /14|BLOCK_AND_CARVE|MOVE_ORDER5,ATTACK2,HEAL3,SUPPORT,MOVE_ORDER2,MOVE3,STAY|

Pointer14 aliases AiCommandset13/CRITICAL, not15. Geshp's point0 avoids
dead-follow promotion and its15/15 does not require any region flag.
The ordinary dispatcher reads count, advances over it and feeds command
bytes to ExecuteAiCommand (`ai/executeaicommand.asm`, `$E294..$E3EE`):
result0 stops, nonzero continues. Listing the list is not executing any
command or choosing an attack/spell/item. Geshp FREEZE4, Evil Bishop's
Fairy Tear/AURA and rock explosion remain separate later evidence.

## Actual consumers and selected38 gaps

**Confirmed about accepted code:** production consumes prepared Content,
not on-demand disassembly. The [capability owner](../../remake/docs/capability-status.md)
defines runnable scope. The predecessor owns missing38 terrain/content/
world/material bindings, native opening cameraSpeed/flash, omitted
Caravan65 follower prefix, HP-skip and F88 differences. Current direct
source checks add these boundaries without SDK/adapter/host execution:

| Actual consumer | Behavior and selected38 boundary |
| --- | --- |
|[Encounter preparation](../../src/sf2tool/remake_exploration_battles.py), [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs)|Placement/order/region/point/spawn shapes exist; all selected coordinates fit32×32 with nondegenerate type4 triangles. Preparation admits2/3; Pin admits1/2/3, excludes38. Shape validity does not bind38 terrain/definitions/route/materials.|
|[PrivateBattleScenarioReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|LEADER14/ATTACKER1/3 and split low127 AGI/ExtraRoundAction have representations. region-program-table admits1/2/3 only; HIDDEN refuses spawn-expression; HEALER3/4/SENTRY refuse source-commandset, MAGE refuses source-mover, ENHANCED refuses enemy-derived-state and selected BIT4 refuses source-ai-word. These are inspected guards, not executed38 errors.|
|[BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs)|Cross products agree with these selected rectangle probes, but use a different numerical mechanism. tested starts0 each round and initialization clears it, unlike the retained native word. Validated roster omits all30 nonroster/signed-HP/raw160 deals seams. Nonzero modes refuse spawn-mode; no hidden reset/maxHP/retry/list/appearance flow follows. Equipped items refuse enemy-equipment-refresh; controlled difficulty/party refresh limits remain.|
|[BattleTurnFlow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs), [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs), [BattleRandom](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleRandom.cs)|Activation before generation, sequential draws/bases63/52 via numerical63+extra flag,64 slots and signed62×63 sort exist. Flow reads actor.Agility plus definition.ExtraRoundAction, so arbitrary native raw high-bit refresh coupling is unproved. Managed overflow refusal differs from unchecked native writes. No actual38 seed/queue is bound.|
|[RuleCompositions](../../remake/src/Sf2.Remake.Application/Gameplay/RuleCompositions.cs), [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs), [SourceMoveOrderAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs)|source-orders binds actual AI; checked reset and15/15/primary guard exist. Commandset guard admits6/7/9/14 and14's mode2/first5/attack2 sequence. Subsequent source-action-categories refuses Geshp's equipped FLAIL/FREEZE4; admitting14 does not implement that loadout. SourceEnemyAi has no enemy-kind32/ProcessExploderAi dispatch; admitting ATTACKER1 cannot establish Burst Rock fidelity. Source follow targets need explicit combatant bindings.|
|[BattleAdvancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs), [BattleControlRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs), [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|BattleStartFinished advances through observed candidate/draw/queue/dead-skip/player/automatic interfaces. Outcome precedes generation; sleeping action skips before source-AI preparation, unlike native order. region-program-none/spawn-modes-admitted are restricted policies. F88 refuses suspended-battle-entry; transactional refusal does not reproduce interrupted original prefixes.|
|[SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs), [ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs)|Alias/population and board/cursor/sprite/text/audio/effect projection exist with capacity guards. They bind no selected38 hidden max+1 allocation, appearance/cursor/DMA/text lifecycle, original service timing, usable input or persistence.|

No missing consumer is repaired here. Shapes, restricted shared policies
and Battle01–03 support do not establish selected38 playability. Follow-up
decisions need the selected content/terrain/materials, checked-state
policy, hidden reset/appearance, actual loadout and special-kind boundary.
The separate leader-defeat-program guard concerns a dead actor's
Physical.Leader flag, not an unsupported LEADER14 commandset; the current
reader sets enemy physical leader:false.

## Reproduction, preserved results and Unknowns

The ignored recipe checks pinned clean source, unchanged pair metadata,
selected12/20/3/1 rows,36 predicates/22 coordinate cases, all activation
words/checked state/absent160 aliases, hidden gate/reset/occupancy/service
instructions, ten supplied spawn states, selected stats/currentAGI,
three retry/full-word/unheld-flag deltas, sequential supplied RNG/queue
variants, control/special entry and actual
consumers. It does not export, import or execute the opening/commands.
Run after current ignored configuration in the same PowerShell process,
using an unused receipt path:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1146/review.py <configured-source-disasm-root> --receipt local/issue1146/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The source root contains sf2enums.asm, code/ and data/. Check exact two-path
ownership, navigation-only predecessor, UTF8/LF/links/fences, cached diff
and private publication boundary. ADR0004's applicable evidence/date/
provenance/selector/width/branch/order/consumer/adversarial checklist applies;
maintained parser/schema/fixture/helper-test and H3 callback changes are
N/A. Use actual exact-head scoped CI; suggested commands are not passes.
Normal `uv run sf2 verify` is **SKIPPED** under the explicit Issue's
documentation scope; full/old comparisons and SDK/runtime remain excluded.

Preserve #1144's sole opening observation and direct/main-gate results,
#1142/#1140 handoffs and results, the Map53 event import UnboundLocalError/
zero partial programs, twelve completed old private comparison failures,
Battle37 y64 runner refusal and Map66 reload/cross-map scroll limitations.
The predecessor's accepted182/alias/collision/follower/item accounting
remains intact. No old failure is erased, relabeled interrupted or rerun.
Scratch-recipe construction failures and corrections remain in the ignored
handoff; no public source/golden/schema/policy is weakened to pass them.

Freeze a clean committed/pushed Draft PR with exact base/head/tree,
actual checks/CI, failures/Unknowns and ignored handoff. Independent
main-gate acceptance/merge remains pending; stop the writer/processes at
handoff. No next Issue, merge, cleanup, archive or private upload follows.

**Unknown:** natural admission/region reach, live all30 slots/party/
HP/status/class/equipment/AGI/positions, flags/checked/deals/aliases,
selector/control changes, F88 caller182/queue/cursor and incoming RNG/
first actor; source/ROM equality beyond existing evidence; decoded
terrain/collision/materials; interruption/interleaving and completed
load/debug/reset/appearance/cursor/view/window/sprite/text/audio/DMA
services, hardware timing, usable control and save/load persistence.
Exploder targeting/RNG/action/damage, ordinary individual actions/movement,
whole rounds and outcomes remain outside. Unknowns are not passed gates
or an automatic acquisition queue.
