# Battle40 regions, terrain changes and first control

Evidence date: 2026-10-21. **Confirmed (static):** Battle40 has four
regions, twenty STARTING enemies and two independently guarded layout
scenes. Region checks, held region flags, scene flags and turn admission
are distinct state. Odd Eye's current rawAGI195 supplies two queue
records, with numerical bases67 and55. No fixed first actor follows.
Natural region reach, completed presentation and usable control remain
**Unknown**; this is not runnable Battle40 acceptance.

## Agreement, provenance and method

Standard rigor for [Issue #1160](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1160)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053),
from accepted base `06e1fad521af275af4301fff8ea01be2664dee7b`,
tree `6d3c5bd2e21a1a1601970999b02a328c7b348a60`.
Continue [opening and initialization](./battle40-initialization-and-opening.md)
at BattleLoop @Start. Own this document and predecessor navigation only.
The result supports future content's region/state/layout/turn contract;
no index, schema, fixture, extractor, engine or shared registry change.
Stop at player-input, AI-standby or ordinary AI-command interfaces before
individual action algorithms, damage, whole rounds, outcomes or afterbattle.

Reuse [Battle38 activation](./battle38-activation-and-first-turn-dispatch.md),
[Battle37 turn dispatch](./battle37-activation-and-first-turn-dispatch.md),
[Battle30 integer geometry](./battle30-activation-and-first-turn-dispatch.md),
[Battle26 extra-slot derivation](./battle26-activation-and-first-turn-dispatch.md),
[battle loop](./battle-loop.md), [scripting](./common-scripting.md),
[maps](./common-maps.md) and [RNG](./runtime-rng-and-battle-math.md).
Selected40 operands and actual consumers are checked here; earlier battle
conclusions are not substituted for this selection.

Original paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battle/` means `code/gameflow/battle/`; `map/` means
`code/common/scripting/map/`. sf2const.asm/sf2enums.asm and macros own
constants, widths and encodings. Fnnn denotes flag nnn; ordinals are
zero-based; intervals are half-open; activation words are hexadecimal.
USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new source/ROM equality or original-runtime observation is claimed.

Minimum evidence is selected source, four region/twenty behavior records,
both complete scenes and actual consumer boundaries. Reuse the immutable
18,285,979-byte init/script pair and `local/issue1005/pair.txt` provenance.
One pair read lowers only the two region scenes; no opening rerun, layout
decode, world import, asset export, full-map/state enumeration or runtime.
Processing is linear in selected text and pair size, with finite geometry
probes. Ignored `local/issue1160/` holds recipes/receipts: selected evidence
estimate below2MiB, total including coordination below4MiB, seconds per
command, peak-memory target512MiB **unmeasured**. Reassess expansion or
cross-system runtime needs before changing method.

## Fresh round order and retained entry

**Confirmed:** `battle/battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`)
reaches @Start after fresh LoadBattle and the empty start hook:

1. ActivateEnemies scans128..159 and falls through once for160.
2. ExecuteBattleRegionCutscene rereads CURRENT_BATTLE and may run one scene.
3. Nonzero DEBUG_MODE_TOGGLE requests PrintAllActivatedDefCons.
4. PopulateTargetsListWithSpawningEnemies rebuilds the target-list length.
   If nonzero, ascending appended IDs call SpawnEnemyWithCamera in order.
5. GenerateBattleTurnOrder builds/sorts the queue and clears its cursor.
6. @IndividualTurns_Loop clears d0.w, reads CURRENT_BATTLE_TURN.b as a
   byte offset and reads the actor byte. FF goes back to @Start; any
   other value enters ExecuteIndividualTurn.

The predecessor owns fresh flag90..105 clearing, map54/crop0,0/32×44,
intro490, formation/stat reconstruction and services. It does not clear
387/388 or the checked word discussed below. None of these operations is
a transaction; interrupted prefixes can retain flags or partial layout.

Held F88 instead clears88/AI memory/view target, restores seconds, calls
LoadBattle with incoming d0 unchanged and jumps to the retained individual
turn loop. Initial activation, scene dispatch, spawn and generation are
bypassed. The predecessor's concrete incoming54 reads alias22/logical150
at `$FFB176`, inside the alias table, after rebuilding retained entities.
Retained selectors, crop, slots, flags, checked word, queue/cursor, aliases
and RNG remain inputs. An odd cursor reads a score byte as actor; offsets
128..255 exceed the128-byte turn buffer. A retained FF can later reach
@Start, with then-current state. This is not a validated suspend restore.

## Region geometry and the checked word

**Confirmed:** `battle/battleloop/activateenemies.asm` (`$2550C..$25544`)
rejects signed-negative X.b, zero currentHP.w, then nonnegative d0.b.
It does not test Y, membership, neutral state, active bits or orders.
All twenty unchanged initialized enemies pass. Absent148..159 fail X;
HP8000 is nonzero here, unlike geometry's positive-signed-HP test.

The first passing enemy enters TriggerRegionsAndActivateEnemies in
`battle/battleloop/triggerregions.asm` (`$1AC9FC..$1ACF30`). For each
declared region a clear bit in NEWLY_TRIGGERED_BATTLE_REGIONS.w
(`$FFB20C`) permits geometry; a hit sets F90+ID. Both hit and miss
then set that checked bit. Held flags can activate enemies even when
geometry is suppressed. This word is neither an event queue nor a
snapshot of which regions have triggered.

ClearAiMemory writes48 last-target bytesFF and48 memory bytes0 ending
immediately before `$FFB20C`; it does not clear this adjacent word.
Fresh flag90..105 clearing is separate. The source's named references
to the checked word are the trigger helper's read/write and enemy AI's
@NonSwarmAi word0 write. Broad RAM initialization is not a per-round
clear. Ally AI skips that enemy branch; selected40 has no SWARM15 set.
Reaching ordinary enemy AI clears checked state before testing activation,
without clearing flags. Merely moving or nominally advancing a round
does not establish that this reset happened.

`data/battles/spritesets/spriteset40.asm` is12/20/4/1,438 storage bytes
at `$1B6538..$1B66EE`. Regions begin at relative388, `$1B66BC`;
four12-byte records occupy48 bytes, followed by point0=(16,5) at
`$1B66EC`. Each raw row is type, unused, X1,Y1..X4,Y4, trailing bytes:

| Region / flag | Complete raw12 bytes, decimal |
| --- | --- |
|0 /90|`4,0,0,24,0,32,31,32,31,24,0,0`|
|1 /91|`4,0,0,0,0,23,31,23,31,0,0,0`|
|2 /92|`4,0,0,0,0,16,31,16,31,0,0,0`|
|3 /93|`4,0,0,0,0,22,17,22,27,0,0,0`|

IsRegionTriggered's type4 path tests triangle1/2/4, then3/2/4 after
a miss. Each scans all30 ally slots, requiring signed HP.w>0 and
signed X.b/Y.b>=0. It does not require joined/active membership, inspect
enemy occupants or consult terrain/collision. A positioned living
nonmember can trigger. HP0, HP8000..FFFF or negative X/Y exclude that
ally; another eligible ally can still hit. Crop-relative coordinates
are used directly, with no scene-position or world-origin addition.

The shared edge helper sign-extends bytes, chooses horizontal on equal
absolute deltas, uses signed MULS/DIVS truncating toward zero, and adds
the projection to endpoint B. Bounded collinearity flag1 immediately
accepts; otherwise edges1→2,3→1,3→2 compare the opposite vertex's side.
Negative projection synthesizes2 despite the source comment's3.
The signed-byte index cmp/ble admits equality with region count; FF
separately rejects. Ordinary iteration passes0..3, not4; out-of-contract
indices do not inherit safety from this finite geometry review.

All twelve declared formation points (x14..17,y35..38, with the exact
rows retained in the predecessor) miss both triangles of every region.
This is a predicate on supplied points, not a natural first-trigger
observation. Fresh placement uses a party-dependent subset; arbitrary
live slots are not constrained by that table.

| Supplied eligible ally X,Y | Hit region IDs | Supplied X,Y | Hit region IDs |
| --- | --- | --- | --- |
|0,23|1|0,24|0|
|31,32|0|31,33|none|
|32,24|none|0,0|1,2,3|
|31,16|1,2|31,17|1|
|31,23|1|28,0|1,2|
|27,0|1,2,3|27,1|1,2|
|26,1|1,2,3|18,21|1|
|18,22|1|17,22|1,3|
|17,23|1|16,5|1,2,3|

R0–2 agree with inclusive rectangles x0..31 and y24..32,0..23,0..16
for these probes. R3's tested sloping edge27,0→17,22 uses B-anchored
projection `17+trunc(-10*(y-22)/22)`. At27,1 it projects26 and rejects;
at18,21 it projects17 and rejects. Reversing the endpoints would change
truncation and incorrectly accept both. These finite probes agree with
the exact trapezoid; they do not establish general cross-product parity,
a walkable route or a tile type.

With only eligible formation positions, clear flags and any incoming
checked word C, the first passing enemy leaves `C OR 000F` and no held
selected flag. Checked000F plus clear flags still suppresses a later
move to16,5. Checked0 plus an eligible16,5 can set91/92/93; checked0
plus0,24 can set90. Independently held90..93 continue to apply when
checked bits are set. If neither128..159 nor the extra160 candidate
passes the outer guards, this helper is unreached: no geometry,
checked-bit write or selected activation write.
No valid allies with a passing enemy produces misses and checked bits;
it does not erase already held flags.

## All twenty activation connections and spawn distinction

**Confirmed:** GetTriggerRegions in `code/common/stats/combatantstats_1.asm`
returns the packed high primary and low secondary nibbles. Both15
preserves activation. Otherwise held primary wins and ORs1 (PRIMARY
bit0); only a primary miss permits held secondary to OR3 (PRIMARY bit0
and SECONDARY bit1). Priority1000/2000/3000 is not PRIMARY. Existing
secondary survives primary; misses do not deactivate or clear either bit.

The predecessor owns full definitions, items, current stats and positions.
Here FF means NONE order,40 MOVE_TO point0,80/85/8A/8B/8F follow enemy
128/133/138/139/143, and00 FOLLOW_TARGET0. These are combatant/order
identities, not scene aliases or physical slots. All secondary orders
areFF even where a secondary trigger is declared.

| Row / ID | Primary order | Primary/secondary region | Initial word | Any selected held trigger result |
| --- | --- | --- | --- | --- |
|0 /128|40|2/15|1000|F92→1001|
|1 /129|80|2/15|2000|F92→2001|
|2 /130|80|2/15|3000|F92→3001|
|3 /131|85|1/1|2000|F91→2001|
|4 /132|85|1/1|2000|F91→2001|
|5 /133|FF|1/15|2000|F91→2001|
|6 /134|FF|2/15|2000|F92→2001|
|7 /135|FF|2/15|2000|F92→2001|
|8 /136|FF|0/15|2000|F90→2001|
|9 /137|8A|3/3|2000|F93→2001|
|10 /138|8B|3/3|2000|F93→2001|
|11 /139|FF|3/15|2000|F93→2001|
|12 /140|FF|0/15|2000|F90→2001|
|13 /141|8F|0/0|2000|F90→2001|
|14 /142|8F|0/0|3000|F90→3001|
|15 /143|00|0/15|2000|F90→2001|
|16 /144|00|0/15|2000|F90→2001|
|17 /145|00|0/15|2000|F90→2001|
|18 /146|FF|15/15|2000|unchanged|
|19 /147|FF|15/15|2000|unchanged|

Same primary/secondary indices cannot select secondary while primary
misses under ordinary unchanged flags. They therefore OR1, not3.
Supply primary0/secondary1 with only91 held: wordOR3. Both held:
wordOR1. Existing2003 with primary held stays2003. These are legal
changed-state distinctions, not selected40's initialized secondary path.
Rows146/147 need no activation flag for their later ordinary AI path.

Absent148..159 retain the predecessor's X−1/HP0/activation0000 and
trigger2/15 read from the unoffset first row; other fields may be stale.
Changing only HP/X does not install a definition or valid AI/loadout.
148..157 can enter the queue if its live gates pass;158/159 never enter
the ordinary turn scan. Y−1 or neutral state can pass activation/queue
while entity packing/other consumers reject or interpret them differently.

After its32 explicit iterations ActivateEnemies falls into the helper
with d0.w160. The accepted Battle26 RAM/deals derivation maps this to
CURRENT_GOLD `$FFF600`: pseudo-X `$FFF62E` is deals84/85, pseudo-HP
`$FFF60E` deals20..23, pseudo-trigger `$FFF636` deals100/101 and
pseudo-activation `$FFF634` deals96..99. Nonnegative pseudo-X/nonzero
pseudo-HP plus a held non15 trigger can OR1/3 into the Goddess Staff99
count nibble. Formation-only fresh40 sets no selected flag, so does not
produce that write. Changed held flags can matter. This is conditional
alias arithmetic, not safe actor160, item delivery or persistence.

`battle/battleloop/populateenemyspawns.asm` (`$1ACF30..$1AD090`) masks
the live activation with0F00. All selected STARTING words, including
OR1/3 variants, mask to0 and skip reset/append; absent words do too.
The rebuilt target-list length is0, so no selected40 appearance request.
Activation is not spawn and is not queue admission. Changed0100,0200,
0300 modes invoke separate dead/held-trigger/maxHP/occupancy reset rules
owned by Battle38; region scenes do not install those modes or enemies.

## Dispatcher, flags and interruption

**Confirmed:** `battle/cutscenes/regionactivatedcutscenes.asm`
(`$47E82..$47EC8`) scans8-byte rows; complete data at `$47EC8..$47EEA`:

| Row | Battle / region / scene flag | Program |
| --- | --- | --- |
|0|32 /0 /386|rbcs_battle32|
|1|32 /1 /386|rbcs_battle32|
|2|40 /0 /387|rbcs_battle40_1|
|3|40 /1 /388|rbcs_battle40_2|
|end|FFFF word|return|

Battle match precedes scene-flag guard; held scene flag skips before
checking region. On a clear guard it loads region in d0, calls
CheckTriggerRegionFlag, then SetFlag on a hit. Crucially the helper
saves/restores d0–a6: its temporary90+region in d1 does not escape.
The original guard387/388 remains in d1. CheckFlag also preserves d1;
SetFlag/GetFlag consume d1, so the dispatcher sets387/388, not90/91.
It then loads the program pointer and traps MAPSCRIPT, falls to @Done
and returns. It never resumes the table after that scene in this call.

| Supplied40 state at reads | This dispatcher call |
| --- | --- |
|90 and91 held,387/388 clear|set387; execute scene1 only|
|same region flags;387 held,388 clear|skip scene1; set388; execute scene2|
|90 absent,91 held,388 clear|scene2 can execute before scene1|
|387/388 held|neither scene, regardless of regions|
|clear guard but its region flag absent|skip that row|
|guard cleared again with region still held|that scene can repeat on a later call|

Each scene ends by setting its guard again. This is redundant on an
uninterrupted ordinary call, but can restore a guard changed during
the scene. Guard set before the first camera/copy means interruption
can leave guard held with zero or only a prefix of layout copies; its
tail setF is not proof all earlier display services completed.

The effective caller audit resolves j_ExecuteBattleRegionCutscene to
this implementation and finds one call instruction, at BattleLoop
@Start; direct implementation calls0, AI calls0. Similarly activation's
j_TriggerRegionsAndActivateEnemies resolves to its implementation at
the one helper call, not a zero-caller claim. Enemy StartAiControl
clears checked state and reads held flags/activation as preparation;
it does not run geometry or this scene dispatcher. A player move,
AI turn, or two newly held regions therefore does not prove immediate
scene execution. A later @Start normally provides another opportunity;
this owner does not simulate an entire round to establish it.

## Both complete layout sequences

**Confirmed:** scene1 spans `$4E8A8..$4E9B2` (266 encoded bytes,
53 top-level commands); scene2 `$4E9B2..$4EA94` (226 bytes,45 commands).
Both start cameraSpeed `$30` (word48). csc45 writes VIEW_SCROLLING_SPEED;
setCamDest uses csc32, clears view target to−1, supplies tile coordinates
to SetCameraDestination and calls WaitForViewScrollEnd. Scene1 chooses
10,17; scene2 chooses13,8. These are view requests, not actor positions.

Every copy below is `setBlocks 11,11,1,1,destX,destY`, reading one live
working-layout word from11,11. Rows are exact source order; the right
column is the following csWait operand; dash means no following wait.

| Copy ordinal | Scene1 destination | Wait | Scene2 destination | Wait |
| --- | --- | --- | --- | --- |
|0|15,21|5|19,10|5|
|1|17,20|5|19,11|5|
|2|13,19|5|20,12|5|
|3|11,22|5|21,11|5|
|4|16,21|5|21,12|5|
|5|15,22|4|22,12|4|
|6|14,20|4|19,12|4|
|7|15,20|4|18,11|4|
|8|16,22|4|20,13|4|
|9|16,23|4|19,13|4|
|10|14,22|3|18,10|3|
|11|10,23|3|18,12|3|
|12|16,20|3|18,13|3|
|13|13,20|3|17,12|3|
|14|14,21|3|19,14|3|
|15|17,21|2|20,14|2|
|16|18,22|2|21,14|2|
|17|16,19|2|17,10|2|
|18|17,22|2|16,12|2|
|19|17,23|2|17,13|2|
|20|12,19|1|13,14|—|
|21|18,20|1|—|—|
|22|13,22|1|—|—|
|23|15,19|1|—|—|
|24|19,22|—|—|—|

Scene1 then setF387/end; scene2 setF388/end. There is no dialogue,
entity move/spawn, damage, SetTerrain, saved-layout restore, fade or
music command in either scene. Scene1 has24 waits, operand sum74;
scene2 has20, sum70. ExecuteMapScript treats encoded negative wait
words as Sleep(low8) unless SKIP_CUTSCENE_TEXT skips them, including
P2 START with debug enabled. These sums are requested Sleep operands,
not measured scene durations. Normal script termination clears view
speed and conditionally waits for scrolling if a dialogue window is open.

csc34 reads three packed words (sourceXY,sizeWH,destinationXY), calls
CopyMapBlocks, then sets both view-plane update bits. CopyMapBlocks in
`code/gameflow/exploration/exploration.asm` copies live16-bit words at
FF0000_RAM_START, row-major, stride64 words/128 bytes, using forward
reads/writes rather than a detached snapshot. These1×1 copies do not
overlap source11,11, nor does either scene overwrite that source.
The words are copied with their flags; the operation does not decode
or invent a terrain type. Repaint requests do not prove DMA/render finish.

### Reload behavior and the independent terrain array

**Confirmed:** `data/maps/entries/map54/3-flag-events.asm`
(`$B71D8..$B71EA`,two8-byte rows plus terminator) declares:

| Guard / load order | Source X,Y | Size W,H | Destination X,Y |
| --- | --- | --- | --- |
|387 /first|11,53|13,8|7,18|
|388 /second|0,52|10,7|13,10|

LoadMapBlocksAndLayout in `code/common/maps/mapload.asm` decodes the
base layout first, walks every flag-copy row in order with independent
CheckFlag reads and live CopyMapBlocks, then item-layout updates and,
when in battle, crop→0,0 copy. Selected crop0,0/32×44 changes no origin.
Unlike the one-scene dispatcher, both held load guards apply both rows
in one load. Their destination rectangles y18..25 and10..16 do not
overlap; each contains its corresponding scene's destinations. Template
sources differ from11,11 and remain outside those destination areas.
Their decoded word equality to the scene result has not been measured.

| Supplied load flags | Fresh load's flag-copy behavior |
| --- | --- |
|neither|neither rectangle copied|
|387 only|13×8=104 words/208 bytes copied|
|388 only|10×7=70 words/140 bytes copied|
|both|first104 then70 words,174 words/348 bytes total|

These are transfer sizes, not encoded table storage or a count of
visually different tiles. Repeated load decodes base again and reapplies
the then-held rows. Clearing a flag alone does not undo an already
mutated in-memory layout; a subsequent fresh load omits its row.
Interrupted scene plus held guard can load an entire template rectangle
later, even if that scene's copies never finished. Flag mutation between
reads remains live state; there is no atomic two-flag snapshot.

The predecessor's319-word638-byte temporary save/replace/restore is a
different scene operation. These later scripts have no restore and
do not clear or set the battle-region90..93 flags. Flag-driven reload
reconstructs from map data; it is not a serialized copy of the animated
working layout, and does not prove save-file persistence.

`battle/battleloop/loadbattleterraindata.asm` (`$1AD0DC..$1AD104`)
selects pt_BattleTerrainData[CURRENT_BATTLE] and decompresses it into
BATTLE_TERRAIN_ARRAY (`$FF5F00`,2304 bytes/48×48). LoadBattle requests
this separately from map loading. GetTerrain/GetCurrentTerrainType in
`battle/battlefield/battlefieldengine.asm` read that array;
InitializeMovementArrays supplies it with mover-derived costs to the
battle range builder, and GetLandEffectSetting reads its low terrain
nibble through the mover table. Neither selected script nor
CopyMapBlocks writes this terrain array or calls SetTerrain.

Thus these layout changes do not by themselves change battle terrain,
move costs, land effect or HP. Working layout also feeds UpdateVdpPlane
and entity map services. In `code/common/scripting/entity/entityscriptengine_2.asm`,
esc02_controlCharacter's flag6-guarded map check calls
ConvertMapPixelCoordinatesToOffset, reads the live layout word and
masks C000 for directional block behavior; debug+B can bypass that
check. This is a field/entity consumer, separate from battle range costs.
Passability needs its actual state and flags, not a visual label such
as a collapsing tile. Decoded Map54 words,
precise field collision at these cells and actual display completion
remain **Unknown**. No new terrain decode or collision experiment was
needed to establish the separate memory/call ownership.

## Live turn generation and first dispatch

**Confirmed:** `battle/battleloop/turnorderfunctions.asm`
(`$25544..$25610`) clears64 two-byte records at `$FFF71A` toFFFF,
offers allies0..29 then enemies128..157 in ascending order, and rejects
only signed-negative X.b or zero HP.w. Y, membership, neutral state,
activation, sleep/stun and signed-positive-HP are not queue gates.
The twenty selected enemies enter even with PRIMARY clear.148..157
normally fail;158..160 are never offered. Queue admission does not
prove a valid entity alias, later action or completed appearance.

Read current rawAGI, not an immutable definition. Let A=raw&127 and
R(n) denote the next direct GenerateRandomNumber draw:

- First score: `A+R(A>>3)-R(A>>3)+R(3)-1`.
- Raw>=128 appends a second record immediately, with B=floor(5A/6)
  and score `B+R(B>>3)-R(B>>3)`.
- Low score bytes are stored after word arithmetic. Skips draw0,
  single candidates3 and high-bit candidates5 times.

The direct generator (`code/common/tech/randomnumbergenerator.asm`,
`$1600..$16BE`) updates `seed=(seed*13+7)&65535`; for these small
ranges output is floor(seed*n/65536), in0..n−1. Range0 still advances
seed. This bypasses debug/direction overrides and the thinking RNG copy.
Every ally's eligibility/high bit/AGI affects later enemy draws.

The predecessor's status0/equipment/difficulty analysis gives these
unchanged current values; no selected effect changes AGI. Odd Eye's
prowess DOUBLE encoding is a separate within-action mechanic.

| Kind | Raw AGI / A | First draw range | First score envelope |
| --- | --- | --- | --- |
|Odd Eye|195 /67|8|59..75|
|Demon Master|65 /65|8|57..73|
|Evil Bishop, Cyclops|46 /46|5|41..51|
|Dark Gunner|45 /45|5|40..50|
|Hydra|50 /50|6|44..56|
|Chaos Warrior|54 /54|6|48..60|
|Demon|55 /55|6|49..61|
|White Dragon|53 /53|6|47..59|
|Chaos Wizard|59 /59|7|52..66|

Odd Eye uses ranges8,8,3,6,6 in that order; B55's second envelope
is50..60. These are conservative algebraic bounds, not independent
attainable extrema, probabilities or a fixed first actor. With twenty
unchanged enemies,21 records and62 draws are appended. N eligible
normally placed allies, T with high-bit rawAGI, make totals21+N+T and
62+3N+2T; N≤12 gives at most45 records/122 draws when T12. Other valid
live states use C eligible and D high-bit candidates: C+D,3C+2D. Source
has no capacity check for arbitrary changed slots;120 potential turns
from sixty high-bit candidates exceeds64. The maintained engine rejects
overcapacity rather than reproducing that unsafe buffer overwrite.

Sorting performs62 outer passes and63 adjacent signed-byte comparisons
per pass. Swap only when the next score is signed-greater; ties preserve
insertion order (allies before enemies, lower IDs, first then second).
FFFF sentinels participate as score−1. Wrapped negative scores can
remain behind a sentinel and never execute; stable order is not a
guarantee every admitted record is consumed. Odd Eye's two records are
independent queue entries and need not be consecutive after sorting.

CURRENT_BATTLE_TURN.b is reset0 and advances by2 after a completed
individual turn path. ExecuteIndividualTurn (`$23EB0..$24242`) tests
live HP again; HP0 skips. It sets actor/target coordinates, alias/view,
cursor destination, then chooses AI preparation if MUDDLE, AI_CONTROLLED
bit2, ordinary enemy without control-opponent toggle, or ally auto-battle.
Ordinary allies otherwise take the player branch; an enemy with control
opponent enabled can too. Source predicates are reread later, not one
permanently cached control classification.

AI preparation precedes the shared cursor/view waits, windows, movement
range/grid and hidden cursor. After those services SLEEP/STUN skips
action; otherwise live MUDDLE/AI word/toggles select ExecuteAiControl
or ProcessBattleEntityControlPlayerInput. Thus sleeping/stunned AI may
already have performed preparation. Queue generation, PRIMARY and raw
AGI high bit alone do not grant player control.

For selected enemy AI, @NonSwarmAi first clears checked state. Both
trigger15 bypasses activation testing; otherwise PRIMARY bit0 selects
activated handling. A clear bit goes to DetermineAiStandbyMovement,
then installs STAY; standby can still have a movement interface and is
not synonymous with absent actor or skipped queue entry. None of the
selected kinds takes Prism Flower/Zeon Guard/Burst Rock special branches.
Activated ordinary handling can promote a dead follow target's secondary
order to primary, secondaryFF, then uses the live AI commandset/pathfinding
interface. This owner stops before that command algorithm or concrete
movement/attack/damage. No natural seed, first actor, completed wait or
usable input is established by these static branches.

## Actual importer, engine and adapter boundaries

**Confirmed about current code:** the sole two-scene lowering completed
in0.1186s with no exception; full45387-byte receipt includes programs,
source corpus and error fields. It emits54/46 instructions (including
the extra CameraWait presentation after each camera target), with26/22
native instructions: cameraSpeed
plus25/21 setBlocks. Camera destination/waits/setF/end have typed forms;
there are no actions. Each program's instruction0 is native cameraSpeed.
If explicitly bound and reached, ProgramRunner refuses program-opcode
before advancing that instruction. No40 scene was executed, and lowering
does not provide region dispatcher registration or lifecycle support.

| Owner | Actual support and selected40 boundary |
| --- | --- |
|[preparation](../../src/sf2tool/remake_exploration_battles.py), [program compiler](../../src/sf2tool/remake_exploration_programs.py)|Preparation admits2/3. Explicit registration allowed this selected lowering only. Native camera/layout remains; a compiler result is not encounter admission.|
|[map compiler](../../src/sf2tool/remake_exploration_maps.py)|Accepted Map54 behavior remains: route-absent non-event setup is finally overwritten to end; conditional387/388 wrappers retain native flag-layout-copy; both clear reaches end. eventMaps54 requires an unsupplied setup binding. No fallback population proves battle40 entry.|
|[PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [PrivateBattleScenarioReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Pin and region-program admission1/2/3. Four-vertex geometry/placement shapes exist, but40's region scenes are unadmitted. Selected HEALER3/5/SENTRY, Fairy Tear placement refresh, ENHANCED/BIT4, MAGE/BRASS_GUNNER and copied equipment guards from the predecessor remain; no full import/failure ordering claimed.|
|[BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs)|Assigned OR1/3 and sticky flags exist. BeforeRound starts local tested0 each round, uses represented living positioned allies and exact cross-product triangles. Original checked-word retention and all30/raw signed-byte slots differ; cross products do not implement native B-anchored truncation. RegionProgram other than None and nonzero spawn modes refuse before RNG/publication.|
|[TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs), [BattleTurnFlow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs)|Sequential word RNG, numerical AGI plus separate ExtraRoundAction, stable signed sorting/sentinels and64-slot capacity validation exist. GenerateRound calls activation first. Actor model/processing order require content binding; production does not scan arbitrary native RAM slots.|
|[SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs), [BattleControlRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs), [BattleAdvancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs)|Supported source AI clears represented tested state and branches on PRIMARY/unconditional15; commandsets/loadouts remain guarded. Application emits regions-tested/region-program-none/spawn-modes-admitted/round-rng, advances to validated player or automatic control, and preserves last committed state on refusal. It does not invoke these40 region scenes or reproduce original partial prefixes. Control-side/status/position restrictions are explicit; this is not arbitrary toggle/muddled-state parity.|
|[WorkingMapLayout](../../remake/src/Sf2.Remake.Domain/Maps/WorkingMapLayout.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs)|64×64 word copy and generic scene/map/terrain projections exist. Native-call maps to UnsupportedInstruction and is refused before cursor advancement. These scenes, Map54 assets/templates, collision and387/388 original persistence are unlinked. Battle terrain remains separately bound content, not a consequence of a display copy.|

**Inferred:** future content can express separate region predicates,
sticky activation, one-scene dispatch, guard-before-effects, mutable layout
and flag-driven reconstruction, then turn generation/control. Existing
typed primitives can help after explicit bindings and admitted semantics;
they do not make40 engine-ready. The smallest follow-up owner would be
an explicitly scoped Content/Domain/Application admission task addressing
the named refusal and geometry/state/scene lifecycle differences, without
turning this documentation slice into an engine repair.

## Reproduction and stopping boundary

The ignored direct recipe records selected source/function sequences,
complete region/behavior/scene data, finite integer probes, supplied-seed
turn arithmetic, actual consumer text and document/private boundaries.
It reuses only the small accepted integer geometry functions, not an old
review framework. No maintained checker or verifier tests were added.
One-shot lowering is retained and not repeated by direct review.
Use fresh receipts and load private configuration in the launching process:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1160/review.py <configured-source-disasm-root> --receipt local/issue1160/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

Applicable ADR0004 checks cover provenance, widths, selectors, branches,
mutation/call order, aliases, changed valid states and publication. New
maintained parser/schema/fixture/helper-test and H3 callback items are
N/A. Normal verify is **SKIPPED** under the explicit documentation-only
Issue scope; full/old comparisons, SDK/Godot/emulator/H3/H4/screenshots
are not run. The exact committed planner, actual scoped CI, resource and
process results, local recipe failures and frozen identities belong to
the handoff. Main-gate independently reviews/accepts/merges.

Preserve #1158's completed direct/main-gate/CI results, sole opening
lowering and local recipe failures; #1156's missed final Map54 setup
overwrite and accepted correction; #1154 autoFacing/HP, #1152 PRIMARY/
Mover corrections; existing no-clamp and aftermath-native findings;
Map53 UnboundLocalError/zero partial, twelve completed old comparison
failures and F88/natural-service limits. No previous result is rerun,
erased, relabeled interrupted or silently upgraded by this slice.

**Unknown:** natural incoming membership/HP/positions/aliases/flags/
checked word/RNG, actual first actor, interrupted/interleaved source
services, decoded Map54 layout/terrain and collision, template equality
to completed animation, source/ROM equality beyond accepted evidence,
camera/render/DMA/VInt/text/input completion, hardware timing, save/load
persistence and playable40 support. These limits are not an automatic
acquisition or repair queue. Freeze a clean pushed Draft PR; no merge,
cleanup, archive or next Issue follows this handoff.
