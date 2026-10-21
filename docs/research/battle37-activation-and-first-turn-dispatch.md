# Battle37 activation, double turns and first control dispatch

Evidence date: 2026-10-21. **Confirmed (static):** all twelve declared
formation points miss all three original integer regions. With unchanged
completed fresh initialization, eighteen STARTING enemies retain their
initial activation words, no selected region program matches and no spawn
is requested. All eighteen enemies enter turn generation; Red Baron's
live raw AGI190 produces two records with bases62 and51, consuming five
sequential draws. Neither LEADER nor that extra record fixes the first
actor. The first living entry reaches the live player/AI predicates and,
conditionally, ordinary commandset dispatch. Stop before individual AI
command bodies, player input execution or attack execution. Natural
reach, actual seed/state, completed services, usable control and persistence
remain **Unknown**. This is not playable Battle37 acceptance.

## Agreement and evidence route

Standard rigor for [Issue #1138](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1138)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect accepted [Battle37 initialization](./battle37-initialization-and-opening.md)
to integer activation, live turn generation and first control/commandset
handoff. Accepted base is `adfe8911a91244c80b843427aa43b6926998fa7a`
(#1136/PR1137). Own this document and navigation only in its predecessor;
no shared path, competing writer or stacked dependency. Red Baron
Desoul/targeting/attack results, movement execution, whole rounds,
outcomes/after scenes and engine implementation are excluded. Formation,
example seeds and receipt counts do not define legal gameplay.

Reuse [Battle36 shared activation/dispatch](./battle36-activation-and-first-turn-dispatch.md),
[Battle30 integer derivation](./battle30-activation-and-first-turn-dispatch.md#geometry-checked-bits-and-persistent-flags),
[Battle26 alias/RNG derivations](./battle26-activation-and-first-turn-dispatch.md),
[battle loop](./battle-loop.md), [RNG/math](./runtime-rng-and-battle-math.md),
[spritesets](./battle-spriteset-data.md) and
[lifecycle](../design/contracts/battle-control-lifecycle.md).
Shared mechanics retain those owners; selected37 operands, state deltas
and current consumers are checked here without repeating opening lowering.

Original paths are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battleloop/`, `ai/` and `cutscenes/` abbreviate
`code/gameflow/battle/` children; `stats/` means `code/common/stats/`.
sf2const.asm/sf2enums.asm and macros own constants, widths and encodings.
Fnnn means flag nnn; ordinals are zero-based, intervals half-open and
activation words hexadecimal. USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new source/ROM equality, decoded terrain or runtime observation is claimed.

Minimum evidence is selected source/tables/helpers and actual consumers,
36 formation predicates,24 changed coordinates and a few supplied live
states/seeds. Reuse the immutable18,285,979-byte init/script pair with
`local/issue1005/pair.txt` provenance. One pair load and linear selected
reads plus constant-size arithmetic have a one minute/512MiB budget per
direct command; peak memory is unmeasured. Unique small receipts belong
in ignored `local/issue1138/`. No source acquisition, canonical map import,
enemy export, new parser/fixture/platform, verifier tests, normal/full/old
suites, SDK/Godot/emulator/H3/H4 or screenshots. Reassess changed
prerequisites or material contradictions before expansion.

## Fresh call order and retained F88 state

**Confirmed:** `code/gameflow/battle/battleloop_1.asm:BattleLoop`
(`$23A84..$23BB4`) reaches @Start after fresh LoadBattle and the empty
start wrapper. The predecessor owns Map77/crop16,32/48×32, flags437/487/537,
fresh flag90–105 clearing, healing, positions, enemy/current-stat refresh
and LoadBattle. Its temporary Geshp128/Red Baron129 scene population is
replaced; combat Red Baron is128. From @Start the sequence is:

1. ActivateEnemies scans128–159 and falls through its helper once for160.
2. ExecuteBattleRegionCutscene reads the live battle selector.
3. Nonzero DEBUG_MODE_TOGGLE.b calls PrintAllActivatedDefCons.
4. PopulateTargetsListWithSpawningEnemies rebuilds the list length;
   nonzero length passes ascending byte IDs, with cleared d0.w, to
   SpawnEnemyWithCamera.
5. GenerateBattleTurnOrder fills/sorts the buffer and clears its cursor.
6. @IndividualTurns_Loop clears d0.w and loads CURRENT_BATTLE_TURN.b
   as a **byte offset** into BATTLE_TURN_ORDER. ActorFF returns to
   @Start; another actor byte calls ExecuteIndividualTurn.

Each later step requires earlier services to return. Reached flags,
checked bits, words and service mutations have no transactional rollback.
F487 alone proves neither initialization completion nor usable input.

Held F88 instead restores seconds, clears88/AI memory/view target, calls
LoadBattle without clearing incoming d0 and enters the individual-turn
loop directly. It skips this initial activation/spawn/generation chain.
The predecessor proves incoming77 selects offset77 beyond the64-byte
alias table, at `$FFB18D`, the low TEMP_ITEM_OR_SPELL byte. No safe
restoration follows. Retained selectors, crop, HP/status/positions, flags,
checked word, RNG, buffer and cursor remain inputs. Odd offsets read
score bytes;128–255 exceed the128-byte buffer. RetainedFF can later enter
@Start with then-current state. This extension follows that read to its
interface, without claiming a restored battle or natural F88 state.

## Three integer regions, live allies and checked flags

**Confirmed:** `battleloop/activateenemies.asm` (`$2550C..$25544`)
rejects signed-negative X.b, zero currentHP.w, then nonnegative d0.b.
There is no Y, membership, neutral, order or already-active gate. All
eighteen unchanged enemies pass; absent146–159 fail X. High-bit HP is
nonzero here, unlike the geometry scan's positive-signed-HP gate.

The first passing enemy reaches
`battleloop/triggerregions.asm:TriggerRegionsAndActivateEnemies`
(`$1AC9FC..$1ACF30`). For each declared region0–2, a clear checked bit
in NEWLY_TRIGGERED_BATTLE_REGIONS.w (`$FFB20C`) permits geometry.
A hit sets F90+ID; **hit and miss both set the checked bit**. Checked
suppresses geometry, not activation from independently held flags.
ClearAiMemory resets48 last-target bytes and48 memory bytes ending at
`$FFB20C`; neither fresh nor F88 clears this adjacent checked word.
Fresh flag90–105 clearing is a different operation.

`data/battles/spritesets/spriteset37.asm` has counts12/18/3/1 and
402 storage bytes. The three twelve-byte region records at `$1B622A`
each have type4, unused0, these ordered vertices and trailing0,0:

| Region / flag / type | Point1 | Point2 | Point3 | Point4 |
| --- | --- | --- | --- | --- |
|0 /90 /4|0,15|0,31|30,31|30,15|
|1 /91 /4|0,12|0,31|47,31|47,12|
|2 /92 /4|0,0|0,31|47,31|18,0|

Point0 is19,28. Coordinates are crop-relative; neither the crop's world
origin16,32 nor scene coordinates are added for geometry. Type4 tests
triangle1/2/4, then3/2/4 after a miss. Each scans **all30 ally slots**,
requiring signed HP.w>0 and signed X.b/Y.b>=0. There is no joined/active
party-list gate. A living positioned nonmember can trigger. HP0,
HP8000–FFFF or negative X/Y cannot contribute; another eligible ally can.
Terrain collision, walkability, neutral status and enemy positions are not
geometry predicates. Fresh placement supplies a subset of the following
formation points; the table is not an actual thirty-slot state snapshot.

| Formation | X,Y | R0 | R1 | R2 |
| --- | --- | --- | --- | --- |
|0|39,6|out|out|out|
|1|40,6|out|out|out|
|2|39,5|out|out|out|
|3|40,5|out|out|out|
|4|41,6|out|out|out|
|5|39,4|out|out|out|
|6|41,5|out|out|out|
|7|40,4|out|out|out|
|8|41,4|out|out|out|
|9|42,6|out|out|out|
|10|42,4|out|out|out|
|11|42,5|out|out|out|

Regions0/1 start at y15/12. Region2 is sloping: its18,0→47,31
edge uses the vertical dominant-axis branch and the B-anchored projection
`47+trunc(29×(y−31)/31)`. At y4–6 that is22–24, below the formation's
x39–42. The receipt checks both triangles for all36 predicates.

The shared edge helper sign-extends bytes, chooses horizontal when the
absolute deltas tie, uses signed MULS/DIVS with truncation toward zero
and adds the projection to endpoint B. Bounded collinearity result1
immediately accepts. Edges1→2,3→1,3→2 otherwise compare the opposite
vertex's side flags. A negative projection synthesizes2 despite the
source comment's3. The signed byte region-index `cmp.b`/`ble` admits
equality with count; FF separately returns false. Normal iteration passes
0–2, not3. Selected vertices are nonnegative and nondegenerate.

| Supplied X,Y | Hit region IDs | Supplied X,Y | Hit region IDs |
| --- | --- | --- | --- |
|0,14|1,2|0,15|0,1,2|
|30,15|0,1,2|31,15|1,2|
|47,11|—|47,12|1|
|48,12|—|18,0|2|
|19,0|—|19,1|2|
|39,21|1|39,22|1,2|
|39,23|1,2|40,22|1|
|40,23|1,2|40,24|1,2|
|46,29|1,2|46,30|1,2|
|47,30|1,2|47,31|1,2|
|1,31|0,1,2|19,28|0,1,2|
|30,31|0,1,2|31,31|1,2|

At `(39,22)`, projection is `47+trunc(−261/31)=39` and the edge
accepts. The exact line is1196/31≈38.581, so an exact polygon rejects.
The other selected region2 differences are19,1;40,23;46,29;47,30.
For47,30 the truncated projection47 admits a point outside the exact
46.065 boundary. These are integer division differences, independent
of floating-point precision. Supplied points prove geometry only;
they do not establish a naturally reachable move or walkable tile.

With unchanged completed fresh placement, all region flags stay clear
for **any incoming checked word**. The first passing enemy leaves
`incoming OR $0007`. Movement alone cannot re-evaluate an already checked
miss. Supply one eligible ally at39,22, the others at a formation subset,
clear flags and checked0: F91/F92 set. With checked0007 and clear flags,
the same coordinates cause no geometry or activation. With checked0007
and held F92, region2 activation still occurs. At19,28 all three flags
can set, but **no selected enemy references region0** in either trigger
nibble. Declaring/checking/debugging region0 does not give it an enemy
consumer. Absent slots and the extra160 seam are distinguished below.

At enemy AI @NonSwarmAi, StartAiControl clears the checked word **before**
its trigger/activation guard, without clearing flags90–105. None of the
selected commandsets is SWARM15. A later round can therefore recheck
positions if that enemy-AI preparation actually occurred; ally AI skips
this reset, as can special swarm early returns. This is conditional
call order, not proof of a naturally completed first turn.

## All eighteen activation words and conditional spawn branches

**Confirmed:** `stats/combatantstats_1.asm:GetTriggerRegions` splits
the packed byte into high primary/low secondary nibbles. Both15 returns
without writing activation. Held non15 primary wins and ORs1; otherwise
held non15 secondary ORs3. Primary retains an existing secondary bit;
misses preserve the whole word. There is no deactivation path.

The predecessor owns definition copy and STARTING initialization, which
keeps only the definition's upper priority nibble before inserting spawn/
filler. Definition BIT4 does not survive this fresh activation-word write.
Raw AI commandset is a separate byte nibble, not that priority/filler.
The complete selected rows are:

| Row / ID | Definition | Commandset | Primary order / trigger | Secondary order / trigger | Initial = unchanged result | Changed held flag result |
| --- | --- | --- | --- | --- | --- | --- |
|0 /128|RED_BARON|LEADER|MOVE_TO\|0 /15|NONE /15|`1000`|no activation write|
|1 /129|EVIL_BISHOP|HEALER5|FOLLOW_ENEMY\|0 /15|NONE /15|`3000`|no activation write|
|2 /130|CHAOS_WIZARD|ATTACKER3|FOLLOW_ENEMY\|0 /15|NONE /15|`2000`|no activation write|
|3 /131|MINOTAUR|SENTRY|NONE /15|NONE /15|`2000`|no activation write|
|4 /132|BOW_RIDER|ATTACKER3|FOLLOW_ENEMY\|3 /15|NONE /15|`2000`|no activation write|
|5 /133|HELL_HOUND|SENTRY|NONE /15|NONE /15|`2000`|no activation write|
|6 /134|CHAOS_WIZARD|ATTACKER1|FOLLOW_TARGET\|0 /2|NONE /15|`2000`|F92→`2001`|
|7 /135|EVIL_BISHOP|HEALER4|FOLLOW_ENEMY\|6 /2|FOLLOW_ENEMY\|0 /15|`3000`|F92→`3001`|
|8 /136|HELL_HOUND|ATTACKER3|FOLLOW_ENEMY\|6 /2|NONE /15|`2000`|F92→`2001`|
|9 /137|MIST_DEMON|ATTACKER2|FOLLOW_TARGET\|0 /1|NONE /15|`2000`|F91→`2001`|
|10 /138|HELL_HOUND|ATTACKER3|FOLLOW_ENEMY\|11 /1|NONE /15|`2000`|F91→`2001`|
|11 /139|MINOTAUR|ATTACKER2|FOLLOW_TARGET\|0 /1|NONE /15|`2000`|F91→`2001`|
|12 /140|MIST_DEMON|ATTACKER2|NONE /15|NONE /15|`2000`|no activation write|
|13 /141|EXECUTIONER|ATTACKER2|NONE /15|NONE /15|`2000`|no activation write|
|14 /142|BOW_RIDER|ATTACKER3|FOLLOW_ENEMY\|13 /15|NONE /15|`2000`|no activation write|
|15 /143|BOW_RIDER|ATTACKER1|NONE /15|NONE /15|`2000`|no activation write|
|16 /144|HELL_HOUND|ATTACKER3|FOLLOW_ENEMY\|15 /15|NONE /15|`2000`|no activation write|
|17 /145|BOW_RIDER|ATTACKER3|144\|0 /15|NONE /15|`2000`|no activation write|

All secondary triggers are15, so no intact row selects secondary
activation;135's secondary **order**$80 does not imply a secondary region.
Supply primary2/secondary1, held1 alone to get initial OR3; both held
select primary OR1. That is a changed row, not selected37 behavior.
Current word3003 with primary held remains3003, not3001.

MOVE_TO0=$40 selects point0 `(19,28)`, Red Baron's own start.
FOLLOW_ENEMY0/3/6/11/13/15=$80/83/86/8B/8D/8F selects
128/131/134/139/141/143. FOLLOW_TARGET0=$00 selects combatant0.
Enemy145's **raw144|0=$90 selects combatant144**, not128 or point0:
StartAiControl passes the whole nonpoint order to GetCurrentHp;
GetMoveOrderPosition passes it to current coordinate getters. Bit6
instead chooses a point and masks the low nibble. If the followed target
is HP0, preparation promotes secondary to primary and writes secondaryFF;
e.g.145's pair becomesFF/FF when144 is dead. Stop before executing orders.

Absent146–159 initialize max/currentHP0, activation0 and X−1 while other
fields may remain stale. The unoffset first enemy row gives triggerFF
(15/15), not fourteen nonexistent declarations or trigger0F. Changing
only HP/X therefore does not expose selected-region activation for them;
it also does not initialize their commandset, order, AGI or alias state.
They can enter the turn scan only through157;158/159 are excluded there.
Y−1, neutral or X outside the crop can pass activation/turn admission
while entity packing uses different guards. No safe entity follows.

After32 DBF iterations, d0.w=160 falls directly into the activation
helper. The [accepted RAM/deals seam](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
resolves accepted160 to CURRENT_GOLD `$FFF600`. Pseudo-X `$FFF62E`
packs deals84/85, pseudo-HP `$FFF60E` packs20–23, pseudo-trigger `$FFF636`
packs100/101 and pseudo-activation `$FFF634` packs96–99. A passing
nonnegative pseudo-X/nonzero pseudo-HP plus held non15 trigger90–104
can OR1/OR3 into the Goddess Staff99 count nibble. Unchanged formation
leaves no held region flags, so no such activation write. Changed F90
can matter here even though no selected enemy references region0.
This is neither unconditional item delivery nor persistence. Slot160
does not enter normal spawn or turn scans.

### Actual region hook, debug and spawn consumers

The complete `data/battles/cutscenes/regionactivatedcutscenes.asm`
table has32/region0/F386 and32/region1/F386→rbcs_battle32,
40/region0/F387→rbcs_battle40_1 and40/region1/F388→rbcs_battle40_2,
then wordFFFF. ExecuteBattleRegionCutscene (`$47E82..$47EC8`) misses
all four rows while live CURRENT_BATTLE=37, before played/region checks.
There is no selected37 program to lower. Changed32/40 can match;
played check precedes region check and SetFlag precedes MAPSCRIPT.
A held played flag proves no completed program/service.

PrintAllActivatedDefCons checks all sixteen flags90–105 in order,
requests txt463 for each held flag with DIALOGUE_NUMBER.l=0–15, then
closes text. Complete tokens are `["{#}","{N}","{D3}"]` (diagnostic
region number); no scene speaker/portrait is set. Unchanged fresh37
requests none even when debug is enabled. Changed39,22 requests1,2;
19,28 requests0,1,2. Held undeclared flags can request other numbers.
Requests do not establish completed window/text services.

`battleloop/populateenemyspawns.asm` (`$1ACF30..$1AD090`) scans128–159,
masks activation with0F00 and rebuilds TARGETS_LIST_LENGTH.w. All eighteen
STARTING words1000/2000/3000, their OR1/OR3 variants and absent0 mask
to0. Length becomes0; stale list bytes are not consumed and no appearance
call follows. There are no HIDDEN declarations. HP0 alone never changes
mode0 into a spawn candidate.

That does not make arbitrary modified modes safe. Reuse the shared
0100/0200/0300 branches:0100 requires HP0;0200 requires HP0, a held
non15 trigger and maxHP0;0300 uses the dead/trigger helper without the
extra maxHP0 gate. Other modes append nothing. The helper lacks X/Y/
neutral gates and can set active bits before later reset/occupancy refusal.
ResetSpawningEnemyStats uses live count18 OR80 as exclusive bound146;
it rejects146–159 and scans all30 allies/32 enemies for occupancy without
HP, neutral or self exclusion. A dead actor on its own start blocks itself.
Changed declared128, mode0100, HP0/X−1 and unoccupied19,28 can reach
reset/append on success. Reset restores the saved **full activation word**,
and subsequent mode comparisons use returned full d1, without remasking.
Invalid/occupied reset clears max/currentHP, sets X−1 and carry. Conditional
alias allocation, capacity/DMA/cursor/view/spin/audio/text services remain
interfaces owned by shared evidence; no normal37 spawn is observed.

## Red Baron live AGI, sequential draws and queue consumption

**Confirmed:** the predecessor owns definition/current-stat refresh and
five equipped item types. Intact37 fails random-battle membership before
upgrade RNG, even withF537; difficulty changes ATT, not AGI. Dark Sword's
effects increase ATT and decrease DEF5, without AGI change or enemy curse
status. Red Baron therefore starts HP150/status0/currentAGI190 ($BE).
Prowess DOUBLE_1IN4 is a separate within-action mechanic, not the cause
of the extra round record. Other selected fresh AGIs are:

| Definition / IDs | Current HP | Raw AGI | First range | First score envelope |
| --- | --- | --- | --- | --- |
|RED_BARON /128|150|190|7|55..69|
|EVIL_BISHOP /129,135|64|46|5|41..51|
|CHAOS_WIZARD /130,134|53|59|7|52..66|
|MINOTAUR /131,139|58|44|5|39..49|
|BOW_RIDER /132,142,143,145|61|44|5|39..49|
|HELL_HOUND /133,136,138,144|62|50|6|44..56|
|MIST_DEMON /137,140|68|51|6|45..57|
|EXECUTIONER /141|64|45|5|40..50|

These algebraic envelopes are not probabilities or independent attainable
extremes. Current HP/X/status/equipment/AGI may later differ; generation
reads current AGI rather than rebuilding an immutable definition table.
UpdateCombatantStats copies base bytes, applies statuses, then equipped
effects. IncreaseCurrentAgi uses IncreaseAndClamp7Bits (cap127, preserving
bit7), whereas DecreaseCurrentAgi uses DecreaseAndClampByte (cap200,
full-byte subtraction). STUN subtracts5, so otherwise unchanged190
becomes185, whose turn bases are57 and47; the later no-action guard
can still prevent executing it. SLOW/BOOST/equipment changes require their
actual refresh order and operands. There is no universal independent
numerical-AGI/extra-turn boolean in the original byte.

`battleloop/turnorderfunctions.asm` (`$25544..$25610`) initializes64
two-byte records at `$FFF71A` toFFFF. It offers allies0–29 followed by
enemies128–157, ascending within each group. Reject signed-negative X.b
or zero HP.w; do not reject Y−1, neutral, nonmember, inactive, MOV0,
sleep/stun or signed-negative nonzero HP. Absent146–157 fail X in the
unchanged case;158–160 are never offered. These are admission facts,
not safe alias or arbitrary changed-state support.

Let A=currentAGI&127. Direct GenerateRandomNumber draws R(n) produce
first score `A+R(A>>3)−R(A>>3)+R(3)−1`. Saved raw AGI>=128 immediately
appends a second record, basis `B=floor(A×5/6)`, score
`B+R(B>>3)−R(B>>3)`. Word arithmetic stores the low score byte.
For raw190, **A=62, B=51**; ranges are7,7,3,6,6, in that order,
and the second envelope is46..56. These extra two draws precede the
next candidate. For the small ranges here, each direct draw updates
`seed=(seed×13+7)&65535` and returns `floor(seed×n/65536)`.
Range0 still updates the seed. This calls GenerateRandomNumber itself,
not the direction/debug override or the thinking RNG copy.

Unchanged enemies append19 records/56 draws: seventeen single actors
and Red Baron twice. For N eligible normally placed allies, T with
rawAGI>=128, total records=`19+N+T`, draws=`56+3N+2T`, at most
**43 records/116 draws** at twelve eligible high-bit allies. Earlier
service/debug draws are excluded; no incoming seed is identified.
For changed eligible candidate count C and high-bit count D, use C+D
records and3C+2D draws. The formation count is not a universal bound.

| Supplied generation input | Ordered raw score records | Seed after |
| --- | --- | --- |
|128 alone,raw190,seed0|128:61;128:51|19979|
|128 alone,raw190,seed1|128:61;128:50|63592|
|128 alone,raw190,seed65535|128:63;128:52|41902|
|128 alone,raw190,seed7|128:61;128:47|63126|
|128 alone,raw62,seed0|128:61|1281|
|HP0 or X−1,raw190,seed0|none; no draws|0|
|Y−1 or HP8000,raw190,seed0|128:61;128:51|19979|
|ally0 raw50 then128 raw190,seed0|0:49;128:62;128:50|51560|
|ally0 alone,raw0,seed0|0:255 (word−1)|1281|

For128 alone at seed0, successive seeds are7,98,1281,16660,19979;
draws0,0,0,1,1 give61,51. Adding the preceding ally consumes its
three draws first. Removing an eligible actor or clearing Red Baron's
raw high bit changes the later stream; changing only active bits does
not change generation eligibility. These supplied cases do not enumerate
seeds or measure a natural distribution.

Sorting uses62 passes of63 adjacent **signed score-byte** comparisons
across all64 records, swapping only when right>left. Ties keep append
order: allies before enemies, earlier offered ID first, an actor's second
after its first when equal. Padding255 is−1; wrapped135 is−121. Actor0
with score255 is still actor0, not actorFF. Negative scores can sort
behind padding and a reached FF can stop consumption before them.
No native append-capacity guard exists; arbitrary30+30 high-bit candidates
can overflow64 slots, and62 passes do not fully sort arbitrary64-entry
inputs. The intact43 bound does not license those states.

Generation clears CURRENT_BATTLE_TURN.b at `$FFF79A`, immediately after
the128-byte buffer. Fresh dispatch therefore reads offset0; empty
eligibility seesFF and repeats @Start. With just the unchanged eighteen
enemies offered, supplied seed0 sorts128:61 first, while seed1 sorts
Chaos Wizard134:64 before128:61. Both cases generate19/56; they isolate
the generation seam, not a claimed fresh party state. Real ally scores,
eligibility and incoming RNG remain unknown. No first-actor claim follows
from LEADER, raw190 or the two records; no whole round was executed.

## First living entry, control and ordinary commandset handoff

**Confirmed:** `battlefunctions/executeindividualturn.asm`
(`$23EB0..$24242`) initializes the battlefield sprite-frame counter,
clears speech SFX, allocates its frame, masks the actor with
COMBATANT_MASK_ALL and clears the dead-list length. The Taros/Bowie/F112
branch is not selected by intact37. A live HP.w read then skips HP0
to @Done: a queued actor can die before consumption, including between
its two records. This is not a reservation of two completed actions.
@Done returns without moving the cursor. BattleLoop must still return
through its debug/death-processing/end checks and after-turn interface
before adding two to CURRENT_BATTLE_TURN.b and reading the next entry.
HP0 is therefore not an unconditional immediate jump to the next living
actor; those downstream bodies/results stay outside this slice.

For nonzero HP it publishes actor/target X/Y, clears cursor radius,
resolves entity alias/view target and sets cursor destination. No new
X/Y/neutral gate repairs malformed candidates. Control predicates run:

1. MUDDLE mask$30 or activation AI_CONTROLLED mask$0004 selects StartAiControl.
2. Otherwise an enemy with CONTROL_OPPONENT_TOGGLE=0 selects AI;
   nonzero permits player control. Ally AUTO_BATTLE_TOGGLE nonzero
   selects AI; zero permits player control.
3. Returning AI preparation joins @PlayerControl setup: cursor/view waits,
   targeting reset, moving entity, mini-status/land-effect windows,
   movement range/blocks and hiding the cursor. The label does not
   establish human input ownership.
4. Live SLEEP mask$C0, then STUN bit0, selects @NoAction. AI preparation
   may already have happened. Control predicates are read again afterward;
   player reaches ProcessBattleEntityControlPlayerInput, AI ExecuteAiControl.

Stop at these interfaces. No input loop, individual command body,
movement/target/attack, no-action cleanup, after-turn effect or battle
scene is executed. A changed status/control value between reads can alter
dispatch. Wait/window/range requests do not prove completed services.

In `ai/startaicontrol.asm` (`$DEFC..$E1AC`), ally AI goes directly to
ATTACKER1, bypassing the enemy checked reset. An enemy obtains its
dedicated commandset with GetAiCommandset: low nibble of
COMBATANT_OFFSET_MOVETYPE_AND_AI_COMMANDSET. None here is SWARM15.
After @NonSwarmAi clears checked,15/15 bypasses primary-active testing;
either trigger non15 with primary bit0 clear takes standby/STAY, otherwise
it reaches @HandleActivatedCombatant. Secondary bit1 alone does not pass
that guard; normal secondary activation sets both bits.

Thus unchanged134–139 take the standby interface if routed to AI;
128–133 and140–145 pass15/15 despite clear active bits. Kind checks for
PRISM_FLOWER93, ZEON_GUARD and BURST_ROCK precede ordinary order handling;
none of the eight selected definitions is one of those kinds. Red Baron94
has no dedicated kind branch here. After dead-follow promotion, current
commandset selects pathfinding mode and a pointer in
`data/battles/global/aicommandsets.asm` (`$E1AC..$E25B`):

| Selected set / number | Pathfinding mode | Ordered command list |
| --- | --- | --- |
|LEADER /14|BLOCK_AND_CARVE|MOVE_ORDER5,ATTACK2,HEAL3,SUPPORT,MOVE_ORDER2,MOVE3,STAY|
|HEALER5 /4|BLOCK_AND_CARVE|HEAL3,MOVE_ORDER5,SUPPORT,ATTACK2,MOVE3,STAY|
|HEALER4 /3|BLOCK_NON_MOVABLE|HEAL2,MOVE_ORDER4,SUPPORT,ATTACK3,MOVE1,STAY|
|ATTACKER1 /6|REGULAR|ATTACK1,HEAL1,SUPPORT,MOVE1,STAY|
|ATTACKER2 /7|REGULAR|MOVE_ORDER1,ATTACK1,HEAL1,SUPPORT,MOVE1,STAY|
|SENTRY /8|REGULAR|ATTACK1,HEAL1,SUPPORT,STAY|
|ATTACKER3 /9|BLOCK_NON_MOVABLE|MOVE_ORDER3,ATTACK3,HEAL2,SUPPORT,MOVE_ORDER4,MOVE2,STAY|

Pointer14 aliases AiCommandset13 (CRITICAL); it does not alias SWARM15
or the definition's PRIORITYMOD_1. Red Baron's15/15 passes the actual
activation guard even with word1000, then point$40 avoids dead-follow
promotion and selects this ordinary14 list. No leader-first turn rule or
region0 guard is added. StartAiControl reads count, advances past it,
and feeds each command byte to ExecuteAiCommand; result0 stops the list,
nonzero continues. `ai/executeaicommand.asm` (`$E294..$E3EE`) maps those
codes to their named parameterized command entries. Listing subsequent
commands proves neither their execution nor an action result. Red Baron
Desoul/item use, ATTACK2 choice and any targeting remain beyond this stop.

## Actual current consumers and selected37 binding gaps

**Confirmed about accepted code:** production consumes prepared Content,
not on-demand disassembly. The [capability owner](../../remake/docs/capability-status.md)
defines runnable scope. The predecessor owns37 opening/lowering and
equipment/F88/resource differences. These are direct source checks,
without running an SDK, adapter or host:

| Actual owner | Existing behavior and selected limitation |
| --- | --- |
|[Encounter preparation](../../src/sf2tool/remake_exploration_battles.py), [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs)|Placements/regions/point/order/spawn shapes exist; selected coordinates fit48×32 and all three region triangles are nondegenerate. Preparation admits2/3, Pin1/2/3;37 is absent. No selected terrain/definitions/world/route/opening/material binding follows.|
|[PrivateBattleScenarioReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Maps LEADER→14, ATTACKER1/2/3→6/7/9; raw AGI is split into low127 numerical speed and ExtraRoundAction. It refuses37 at region-program-table; selected HEALER4/5 and SENTRY fail source-commandset, MAGE/CENTAUR_ARCHER fail source-mover, ENHANCED definitions fail REGULAR-only enemy-derived-state and selected BIT4 fails source-ai-word. These are guard facts, not observed37 errors.|
|[BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs), [BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs)|Roster-only/already-refreshed/difficulty0/control policies and mode0/None-program are explicit. Five equipped item types refuse enemy-equipment-refresh. Cross-product geometry rejects the five listed native R2 boundary hits; tested starts0 each round and initialization clears it, unlike the native retained checked word. No all30 nonroster/raw160/deals or malformed signed-state model follows.|
|[BattleTurnFlow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs), [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs), [BattleRandom](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleRandom.cs)|Activation precedes generation; sequential draws, bases62/51 with supplied numerical62+extra flag,64 slots and signed62×63 sorting are expressible. Flow uses live actor.Agility plus definition.ExtraRoundAction, resets cursor and preserves the low MainSeed half. General native high-bit/status/equipment coupling is not established. Managed capacity refusal differs from native unchecked writes; no selected37 actual seed/queue is bound.|
|[RuleCompositions](../../remake/src/Sf2.Remake.Application/Gameplay/RuleCompositions.cs), [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs), [SourceMoveOrderAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs)|source-orders binds actual AI;15/15/primary guard and checked reset exist. The actual commandset guard **admits14** alongside6/7/9; SourceMoveOrderAi chooses14's mode2/first5/attack2 sequence. It is incorrect to call LEADER itself unsupported. Subsequent source-action-categories requires unequipped physical-only items/spells, refusing Red Baron's equipped Dark Sword; native Desoul use is not implemented by admitting14. Whole-byte$90 requires SourceCombatants binding for144.|
|[BattleAdvancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs), [BattleControlRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs), [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|BattleStartFinished advances; candidates/draws/queue, dead skips and player/automatic dispatch have consumers. Outcome checking precedes generation; sleep skips before source-AI preparation, unlike native order. Controlled deployment is constrained; F88 refuses suspended-battle-entry. Transactional errors do not reproduce native partial prefixes.|
|[BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs), [ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs)|Board/actor/cursor, sprite/text/audio/effect projection exist. They establish no selected37 bindings, completed original cursor/view/window/DMA services, VInt timing, usable input or save/load persistence.|

Generic shapes, admitted individual policies and Battle01–03 support
remain distinct from bound37 content and playable acceptance. No missing
binding is repaired or quietly reclassified by this research.
The separate `leader-defeat-program` start guard tests a dead actor's
Physical.Leader flag; it is not a LEADER commandset14 rejection.
The current private reader assigns enemy physical `leader:false`.

## Reproduction, retained evidence and Unknowns

The ignored direct recipe reuses accepted shared instruction checks,
then joins selected12/18/3/1 rows,36 formation predicates,24 coordinate
deltas, all activation words, hook/debug/spawn/absent160 boundaries,
current stats/turn/RNG/sort/control and command tables to actual consumers.
It checks two-path/nav-only scope, UTF8/LF/links and private publication.
Load the ignored configuration in the same PowerShell process and use
an unused receipt name:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1138/review.py <configured-source-disasm-root> --receipt local/issue1138/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The source root contains sf2enums.asm, code/ and data/. Review cached
diff and actual exact-head dependency-scoped CI; suggested commands are
not executed passes. ADR0004's applicable semantic/provenance, branch,
width, selector, ordering, consumer and private checklist applies. No
maintained parser/schema/fixture/test or H3 callback changes make those
specific duties applicable. Normal `uv run sf2 verify` is **SKIPPED**
under the explicit documentation scope; full/old comparisons remain excluded.

Preserve [#1136 initialization](./battle37-initialization-and-opening.md#reproduction-preserved-results-and-unknowns)'s
single0.1145s lowering and completed direct/CI results; no opening rerun.
Preserve [#1130 shared owner](./battle36-activation-and-first-turn-dispatch.md#reproduction-preserved-results-and-unknowns)'s
direct/source/consumer and changed-state results. The
[Map77/53 admission owner](./roft-to-galam-and-battle36-admission.md#actual-lowering-and-current-consumer-boundary)
retains Map53's event-enabled import UnboundLocalError/zero partial programs;
the linked predecessors retain twelve completed private comparison
failures and corrected collision/alias/follower/statistics evidence.
No completed failure is relabeled interrupted, erased or rerun here.

Freeze a clean committed/pushed Draft PR with head/tree, actual selected
commands/CI, failures/Unknowns and ignored handoff. Main-gate retains
independent acceptance/merge. Stop writer/processes at handoff; no next
Issue, merge, cleanup, archive or private upload follows.

**Unknown:** natural entry/region reach, all30 live slots/party/HP/status/
equipment/currentAGI/positions, actual flags/checked/aliases/deals, selector
and control changes, F88 scratch/caller/queue/cursor, incoming RNG and
natural first actor; original source/ROM equality beyond existing evidence,
terrain/collision/material bindings; interruption/interleaving and completed
load/debug/cursor/view/window/sprite/text/audio/DMA services, hardware timing,
usable input and save/load persistence. Individual Red Baron/Desoul,
command/action execution, whole rounds and outcomes stay beyond this stop.
Unknowns are neither passed gates nor an automatic acquisition queue.
