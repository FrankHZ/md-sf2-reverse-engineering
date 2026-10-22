# Battle43 activation, round queue and first control

Continue with [Battle43 Zeon Guard laser action and results](./battle43-zeon-guard-laser.md).

Evidence date: 2026-10-22. **Confirmed (static):** all twelve fresh formation
points miss Battle43's sole region. Three enemies use that region; the other
thirteen bypass its PRIMARY gate through 15/15 assignments. The sixteen
STARTING enemies offer seventeen queue records using Zeon's refreshed
currentAGI200. The two Zeon Guards enter line-attacker preparation before
ordinary LEADER commands. These are conditional source contracts, not an
observed first actor or playable Battle43 acceptance. Stop before concrete
movement, attack, spell, Healing Rain consumption or other action execution.

## Agreement, provenance and method

Standard rigor for [Issue #1178](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1178)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053).
Accepted base `57a4b2b577e02214fad31f02c9abec8700a3d0a4`, tree
`04d601f504ae8550f4862ab8d814edb641e1ca65`. The outcome is a complete
selected activation, queue and first-control contract for future design and
content integration. Own this document and successor navigation only in
[Battle43 initialization](./battle43-initialization.md). No shared path,
unmerged dependency, engine, extractor, schema, fixture or index change.
Full Guard laser legality/targets/damage/animation, outcomes and ending are
separate outcomes. Preparation side effects reached before that boundary
are included here.

Reuse [Battle42 activation and first control](./battle42-activation-and-first-control.md),
[Battle36 laser](./battle36-prism-flower-laser.md), [battle loop](./battle-loop.md),
[battle AI](./battle-ai.md#dispatcher-and-standby-control) and
[stats](./common-stats.md). Initialization owns all definitions, loadouts,
fresh/F88 load, hooks, special47 and neutral159 reconstruction. Selected43
operands and actual producer/consumer conditions are independently checked here.

Original paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battle/` abbreviates `code/gameflow/battle/`; constants and RAM addresses
come from sf2enums.asm/sf2const.asm, encoding from sf2macros.asm. Combatant
IDs are byte IDs, ordinals zero-based, intervals half-open, packed words and
addresses hexadecimal. Fnnn denotes a story flag. Registered USA ROM SHA-256:
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new source-ROM equality, decoded terrain or runtime claim is made.

Method: linear selected source/consumer inspection, small changed valid states
and one fixed 128×128=16384 geometry enumeration. Retain counts and boundary
probes, not a full matrix or arbitrary-state Cartesian product. Estimate
seconds per direct command, evidence below2MiB and total including CI/
coordination below4MiB; peak-memory target512MiB is **unmeasured**. Reassess
material growth or coupling. Ignored recipe/receipts are in `local/issue1178/`.
The unchanged18,285,979-byte static pair and `local/issue1005/pair.txt`
provenance are not read or regenerated. Empty hooks need no lowering;
no producer, map/terrain export, acquisition, emulator or new verification framework.

## Entry to the round prefix

**Confirmed:** after fresh LoadBattle and the F493-controlled Empty start
wrapper return, `battle/battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`)
reaches @Start in this order:

1. ActivateEnemies updates128..159, then falls through into its helper
   once more with160; there is no RTS between loop and helper.
2. ExecuteBattleRegionCutscene rereads CURRENT_BATTLE.b.
3. Nonzero DEBUG_MODE_TOGGLE.b requests PrintAllActivatedDefCons.
4. PopulateTargetsListWithSpawningEnemies rebuilds TARGETS_LIST_LENGTH.w.
   A nonempty list would widen each byte ID from a cleared d0.w and call
   SpawnEnemyWithCamera in scan order, waiting for each return.
5. GenerateBattleTurnOrder clears, fills and sorts the queue, then clears
   CURRENT_BATTLE_TURN.b.
6. The individual loop clears d0.w, loads that cursor byte as a **byte
   offset**, then reads an actor byte from the buffer. FF returns to @Start;
   another byte calls ExecuteIndividualTurn with its own live HP check.

The complete region dispatcher is
`battle/cutscenes/regionactivatedcutscenes.asm` (`$47E82..$47EC8`), with
data at `$47EC8..$47EEA`:32/0/F386 and32/1/F386→rbcs_battle32;
40/0/F387→rbcs_battle40_1;40/1/F388→rbcs_battle40_2; FFFF terminates.
Live43 misses every battle comparison before played/region flags: there
is **no Battle43 region scene**. A changed32/40 selector follows played
check→region check→SetFlag→MAPSCRIPT. No selector snapshot or rollback
protects earlier mutations. Debug printing visits held90..105 and requests
txt463 plus region number, then closes; it is not a story scene.

Held F88 follows the initialization owner's retained path: restore seconds,
clear88/AI memory/view target, LoadBattle with incoming d0, then branch
directly to the individual loop. It bypasses this initial activation,
collector and generation. Retained queue/cursor/RNG/flags/checked word and
combatants remain inputs; a later sentinel may reach @Start. Incoming59
reads alias offset59 at FFB17B, enemy155, with negative-alias fallback
physical0. Neither a valid installed43 battle nor safe saved state follows.

## Region0 and the exact integer predicate

**Confirmed:** spriteset43 at `$1B6A5A..$1B6BBE` declares12/16/1/2.
Region0 at `$1B6BAE..$1B6BBA` is all twelve bytes:
`4,0;2,4;10,12;16,12;19,4;0,0`. Its flag is90. Ordered vertices are
V1=(2,4),V2=(10,12),V3=(16,12),V4=(19,4). Points at1B6BBA/1B6BBC
are (16,13)/(15,8), separately from region vertices.

`battle/battleloop/triggerregions.asm` (`$1AC9FC..$1ACF30`) evaluates
triangle1/2/4, then3/2/4 only on a miss. AreAnyAlliesInTriangle scans
all30 slots0..29, requiring **signed HP.w>0**, signed X.b≥0 and Y.b≥0.
It tests no membership, active party, neutral, occupancy, terrain, crop or
scene alias. A living positioned nonmember can trigger; HP0,8000..FFFF
or either coordinate128..255 cannot. The twelve formation ordinals are
capacity/positions, not fixed character identities or proof all are alive.

For segment A→B and point C, CheckTestPointPositionRelativeToSegment
sign-extends bytes, chooses horizontal when |Bx−Ax|≥|By−Ay| (including
equality), and projects from **B**:

- Horizontal: Yproj=By+trunc0((By−Ay)×(Cx−Bx)/(Bx−Ax)). A zero
  horizontal denominator goes to vertical.
- Vertical: Xproj=Bx+trunc0((Bx−Ax)×(Cy−By)/(By−Ay)).
- MULS uses signed word operands and a long product; DIVS truncates
  toward zero, retaining the quotient word. The intervening EXT.L of d6
  before division sign-extends its low word. For these selected vertices
  and test bytes0..127, products/quotients fit, so that step loses nothing.
- Negative projection returns2 through `move #1,ccr`, despite the comment's3.
  Equality inside inclusive dominant-axis bounds returns1 and immediately
  accepts the triangle. Equality outside bounds returns2; otherwise return2
  when projection<test coordinate,0 when greater.
- Each triangle tests edges1→2,3→1,3→2 against C, comparing with the
  opposite vertex unless a bounded edge already accepts. The second
  triangle uses its own ordered vertices, not a reordered polygon.

The index check rejects FF separately, then signed cmp.b/ble against
count1 admits equality1 and other signed cases. Ordinary iteration only
requests0; this is not a safe arbitrary table-index API.

| Formation ordinal | X,Y | Region0 |
| --- | --- | --- |
|0|13,16|miss|
|1|14,16|miss|
|2|12,16|miss|
|3|13,17|miss|
|4|14,17|miss|
|5|12,17|miss|
|6|11,16|miss|
|7|11,17|miss|
|8|11,18|miss|
|9|13,18|miss|
|10|12,18|miss|
|11|14,18|miss|

The fixed domain gives **109 native hits,109 inclusive cross-product
hits,zero differences in16384 queries**. Thus no mismatch counterexample
exists in this enumerated domain for this region. This does not identify
the two algorithms generally; the earlier Battle40/42 slope differences
remain valid. It also does not validate collision or reachable movement.

| Supplied eligible point | Native result | Neighbor | Native result |
| --- | --- | --- | --- |
|2,4 (V1)|hit|1,4 /2,3 /2,5|miss /miss /miss|
|3,4 /3,5|hit /hit|3,6|miss|
|10,12 (V2)|hit|9,12 /10,13|miss /miss|
|11,12|hit|13,8 (interior)|hit|
|16,12 (V3)|hit|17,12 /16,13|miss /miss|
|19,4 (V4)|hit|20,4 /19,3 /19,5|miss /miss /miss|
|18,5 /18,6|hit /hit|11,15 (Elis backing point)|miss|

For triangle3/2/4's V4→V3 edge at(18,5), vertical projection is
16+trunc0((−3)×(−7)/8)=18. The bounded edge accepts it although the exact
edge X is18.625. Adjacent(19,5) misses. Basing that projection at V4 instead
would round differently; the source's actual edge direction matters.
These supplied valid positions are arithmetic controls, not natural moves.

## Checked word, flags and all sixteen assignments

**Confirmed:** ActivateEnemies (`$2550C..$25544`) and its helper require
signed X.b≥0, HP.w nonzero and negative actor d0.b. Y, membership,
NEUTRAL8, priority and existing PRIMARY are not gates. HP8000 passes this
enemy gate although it fails ally geometry. All sixteen fresh STARTING
enemies pass; fresh144..159 fail X−1, except neutral159 is repositioned
but fails HP0.

TriggerRegionsAndActivateEnemies reads NEWLY_TRIGGERED_BATTLE_REGIONS.w
at `$FFB20C`. Clear bit0 permits geometry; a hit sets90. **Both hit and
miss set checked bit0.** Held flags survive misses. Later enemies reuse
the word, while each activation assignment rereads the flags. Fresh
initialization clears90..105 but not this checked word. ClearAiMemory
resets48 last-target bytes atFFB1AC and48 memory bytes atFFB1DC, ending
atFFB20C without writing it. F88's memory clear also leaves it intact.
Enemy StartAiControl @NonSwarmAi clears the whole checked word before
the trigger/PRIMARY gate; ally AI does not. None of the selected sets is
SWARM15. A player move or a nominal round boundary does not itself clear it.

GetTriggerRegions splits primary high nibble/secondary low nibble. For
current activation W: held non15 primary wins and produces W OR1;
otherwise held non15 secondary produces W OR3; otherwise W is unchanged.
The source masks then sets those bits, with no clearing of a previously
held SECONDARY. 15/15 preserves W and bypasses the later PRIMARY gate;
it does not mean PRIMARY was set. Priority occupies the upper nibble and
the AI commandset a separate byte; neither supplies activation bit0.

All rows have secondary orderFF,secondary region15,filler0,STARTING0.
The following is the complete assignment list; order values are hex.

| ID | Kind /commandset | Primary order /region | Fresh activation /effect |
| --- | --- | --- | --- |
|128|ZEON /LEADER|FF /15|1000 unchanged;15/15 bypass|
|129|DEMON_MASTER /ATTACKER3|80 /0|2000;90→2001|
|130|PYROHYDRA /ATTACKER3|80 /0|2000;90→2001|
|131|HORSEMAN /ATTACKER3|84 /15|2000 unchanged;15/15 bypass|
|132|ZEON_GUARD /LEADER|FF /15|2000 unchanged;special-kind preparation|
|133|DEMON_MASTER /ATTACKER3|84 /15|2000 unchanged;15/15 bypass|
|134|PYROHYDRA /ATTACKER3|84 /15|2000 unchanged;15/15 bypass|
|135|ZEON_GUARD /LEADER|FF /15|2000 unchanged;special-kind preparation|
|136|PYROHYDRA /ATTACKER3|87 /15|2000 unchanged;15/15 bypass|
|137|ARCH_DEMON /ATTACKER3|87 /15|2000 unchanged;15/15 bypass|
|138|DEMON_MASTER /LEADER|40 /15|2000 unchanged;15/15 bypass|
|139|PYROHYDRA /ATTACKER3|8A /15|2000 unchanged;15/15 bypass|
|140|PYROHYDRA /ATTACKER1|FF /15|2000 unchanged;15/15 bypass|
|141|ARCH_DEMON /ATTACKER1|FF /15|2000 unchanged;15/15 bypass|
|142|DEVIL_GRIFFIN /ATTACKER1|FF /15|2000 unchanged;15/15 bypass|
|143|BLUE_SHAMAN_0 /HEALER5|41 /0|3000;90→3001|

| Supplied state at prefix | Conditional result |
| --- | --- |
|Only eligible formation points,90 clear,checked C|No hit;checked becomes C OR1;129/130/143 stay inactive.|
|Eligible ally at13,8,checked bit0 clear|Sets90 then bit0;129/130 become2001,143 becomes3001.|
|Same point,checked bit0 held,90 clear|Geometry skipped;three region rows remain inactive.|
|90 already held with checked bit0 held|Geometry skipped;three rows still activate.|
|Move supplied ally outside after90 held|A later miss never clears90 or PRIMARY.|
|No eligible ally but a passing enemy|Miss still sets checked;does not clear held flags.|
|No passing128..159 or extra160 caller|No geometry or per-row activation update.|
|An eligible nonmember at13,8|Can trigger;membership is not consulted.|
|Only ally at13,8 has HP0/8000 or X/Y negative|Cannot trigger;other eligible allies remain independently relevant.|
|Changed assignment primary0/secondary0,W=2000,90 held|Primary wins→2001. With primary15/secondary0 instead,it gives2003. Neither combination is a selected43 row.|
|W=2002 on a primary0 row,90 held|Becomes2003;the existing SECONDARY bit is sticky.|

Flags91..105 have no selected43 region or assignment. Independently held
ones can still debug-print or affect a changed assignment; their existence
does not create undeclared regions. Source cache reset and sticky flag
effects must not be replaced by an imagined per-round geometry snapshot.

## Collector, absent slots and distinct identities

**Confirmed:** `battle/battleloop/populateenemyspawns.asm`
(`$1ACF30..$1AD090`) scans all32 IDs128..159 and masks activation0F00.
Every fresh selected activation masks0, including after OR1/3. The collector
therefore sets TARGETS_LIST_LENGTH.w atFFB63E to0; it does not clear old
TARGETS_LIST bytes atFFB652. No reset, stat refresh, append, spawn camera,
appearance or spawn RNG runs. This battle declares no HIDDEN/RESPAWN row;
death does not turn STARTING into respawn. The generic100/200/300 paths
remain owned by [Battle38](./battle38-activation-and-first-turn-dispatch.md).
Stale/unbound absent fields are not new declared spawn definitions.

Fresh144..159 received HP/maxHP0,X−1,activation0,triggers15/15 from
the unadvanced Zeon row. Other fields may be stale; clear543 subsequently
changes159 alone. A retained valid HP/X change can admit144..157 to the
queue without proving definition refresh;158/159 are never offered there.
Changing Y or neutral can change packing without changing activation or
queue admission. No arbitrary RAM fuzz or changed spawn-bit corpus is used.

| Consumer | Logical159/Elis,script31,special Zeon |
| --- | --- |
|Ally geometry|Only0..29;159 and script31 never supply ally coordinates. Physical47 is not an extra ally.|
|Enemy activation|159 is visited. Clear543 gives HP0 at11,15,activation0008,so fails HP;neutral itself is no gate. Zeon128 is visited once by logical ID,independent of physical47.|
|Spawn collector|159's0008 masks0;all selected STARTING words also mask0. Alias31 and physical47 are not collector entries.|
|Queue|Only128..157 are offered.159 is excluded even if later HP/X change;script31 and physical47 are not offered IDs. Zeon128 enters with currentAGI200.|
|Individual view lookup|Logical128→enemy alias0 atFFB160→physical47;ENTNUM32 remains distinct. Logical159 reads offset63 atFFB17F=FF fresh. Logical31 uses alias31 atFFB15F→Elis physical A+15 with clear543;held543 leaves alias31 fresh0.|
|Entity packing inherited from load|Signed X/Y nonnegative and enemy neutral clear,without HP/PRIMARY gate. Fifteen ordinary enemies pack A..A+14;special Zeon47 does not increment that counter;Elis then usesA+15.|

Here A counts eligible packed allies,0≤A≤12 for the bounded fresh party.
With Elis,occupied records=A+17≤29,highest ordinary/neutral index≤27,
plus special47 within49 cleared slots. Without Elis,count=A+16≤28.
The zero collector adds none. A counts packing, N below counts queue
admission; a changed live state need not make them equal. Retained extra
population or multiple special sprites lacks this capacity/collision proof.

The accepted [extra160/deals alias](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
applies unchanged. GetCombatantEntryAddress admits160 and resolves
CURRENT_GOLD atFFF600: pseudo-X FFF62E packs deals84/85,pseudo-HP
FFF60E deals20..23,triggersFFF636 deals100/101,activationFFF634
deals96..99. If the helper gates and a non15 held trigger permit, OR1/3
can change Goddess Staff99 count bits. With only fresh formation points
there is no new90; independently held90 or changed positions can matter.
160 is not a combat actor, spawn/queue candidate, item award or save claim.

## Refreshed AGI, ordered RNG and the queue

**Confirmed:** `battle/battleloop/turnorderfunctions.asm`
(`$25544..$25610`) clears64 two-byte records atFFF71A..FFF79A toFFFF.
It offers allies0..29 then enemies128..157 ascending, filtering only
signed-negative X.b or currentHP.w=0. Y,membership,neutral,PRIMARY,
sleep/stun and positive-signed HP are not filters. Every fresh selected
enemy enters, including inactive129/130/143.158..160 never enter.

Generation calls GetCurrentAgi, not UpdateCombatantStats. The accepted
refresh is raw203=CB→zero Boost preserves high80/low4B→zero Slow clamps
to200=C8 even with decrement0→no Zeon equipment change. Numerical low
bits are72,not75. Staff/shot do not change the other selected AGI values.
Live status/item/stat changes after refresh remain inputs; raw definitions
cannot replace those current bytes.

For G=currentAGI AND127 and R(n) the **next** direct GenerateRandomNumber
draw, the first score is G+R(G>>3)−R(G>>3)+R(3)−1. Current byte≥128
immediately appends a second record: B=floor(5G/6),score
B+R(B>>3)−R(B>>3). Store the low score byte. Skipped actors draw0,
single-record actors3,two-record actors5. `code/common/tech/randomnumbergenerator.asm`
at1600 uses seed=(13×seed+7) AND65535 and returns floor(seed×n/65536)
for these small ranges; even n0 advances the word seed. The calls are
sequential, before offering the next actor, not thinking/debug RNG.

| ID | Current AGI byte | G /range | First algebraic envelope | Second B /range /envelope |
| --- | --- | --- | --- | --- |
|128|200|72 /9|63..81|60 /7 /54..66|
|129|65|65 /8|57..73|none|
|130|55|55 /6|49..61|none|
|131|55|55 /6|49..61|none|
|132|64|64 /8|56..72|none|
|133|65|65 /8|57..73|none|
|134|55|55 /6|49..61|none|
|135|64|64 /8|56..72|none|
|136|55|55 /6|49..61|none|
|137|61|61 /7|54..68|none|
|138|65|65 /8|57..73|none|
|139|55|55 /6|49..61|none|
|140|55|55 /6|49..61|none|
|141|61|61 /7|54..68|none|
|142|59|59 /7|52..66|none|
|143|51|51 /6|45..57|none|

The envelopes are bounds, not independently drawn samples, joint extrema,
probabilities or a natural winner. Eligible allies draw first. Prowess
DOUBLE affects a later action, not this high-bit queue rule.

With unchanged sixteen living enemies:17 entries and50 draws. For N
eligible allies and T of them high-bit, totals are **17+N+T** entries and
**50+3N+2T** draws. With0≤N≤12 and0≤T≤N,the bounds are17..41
entries/50..110 draws. If k of these enemies remain eligible and z is1
when Zeon is among them (else0),enemy totals=k+z and3k+2z;
add N+T and3N+2T. For general offered live candidates C and high-bit D,
use C+D and3C+2D. Absent/stale HP/X changes need not satisfy the fresh
formation bound. Native code has no capacity guard:60 offered high-bit
candidates could write120 records into64 slots. These arithmetic examples
do not legalize an arbitrary retained population.

Sort is descending **signed score byte**,62 passes with63 adjacent
comparisons each. Swap only when the next score is greater; ties preserve
insertion order (allies before enemies,lower ID,first before second).
FFFF's score−1 participates. Wrapped negative scores can stay behind a
sentinel and never dispatch; queue admission alone does not ensure a turn.
CURRENT_BATTLE_TURN.b atFFF79A resets0 and advances2 after the ordinary
post-turn path. Odd retained offsets read score bytes as actors;128..255
exceed this128-byte buffer. No safe arbitrary cursor or F88 restoration
API follows from the normal fresh bound.

## First individual control and ordinary AI interfaces

**Confirmed:** `battle/battlefunctions/executeindividualturn.asm`
(`$23EB0..$24242`) initializes sprite frame counting/speech state, masks
actor ID toFF, clears dead-list length and rechecks HP.w=0. A dead actor
returns without preparation. A live actor supplies current X/Y to actor
and target words,alias to VIEW_TARGET_ENTITY.b,then cursor destination.
There is no new universal HP/X/Y/alias validity check at this seam.
Zeon128's view goes to47; that does not select Guard behavior or script31.

MUDDLE,AI_CONTROLLED bit2,enemy versus CONTROL_OPPONENT_TOGGLE,and ally
AUTO_BATTLE_TOGGLE choose StartAiControl preparation. Otherwise ordinary
allies (or controlled opponents) use the player route. After preparation
returns: wait cursor/view→clear targeting→set moving actor→open mini-status
and land-effect windows→BuildMovementRangeGrid→pulsating blocks→hide cursor.
Then **Sleep first,Stun second** can branch NoAction. The same control
predicates are reread before ExecuteAiControl or
ProcessBattleEntityControlPlayerInput. A sleeping/stunned AI may already
have mutated preparation scratch. A control toggle change can prepare AI
and later hand off to player,or take the reverse route; no immutable
classification is proved. Requested services/input are not completed play.

At enemy @NonSwarmAi,the checked word clears before the region gate.
With only fresh formation state and90 clear,129/130/143 take
DetermineAiStandbyMovement then install STAY. This prepares a movement
string/standby interface; it does not exclude their queue entries or prove
they cannot later move. Held90 permits ordinary commands. The thirteen
15/15 rows bypass PRIMARY; Guards then take the special path below.
Zeon kind98 itself is not kind38,93 or32 and uses ordinary LEADER14.

For ordinary activated AI,primaryFF skips follow promotion;bit40 means
a point,so138's40 reads point0=(16,13),143's41 point1=(15,8).
Follow80→128,84→132,87→135,8A→138 reads that logical combatant's
**current HP**. HP0 promotes secondary to primary and writes secondaryFF;
all selected secondaries areFF,so the pair becomesFF/FF. Alive HP keeps
the order. This is not a scene/physical lookup,death-list or region-flag
selection. `battle/battlefield/getmoveorderposition.asm`
(`$1B15F8..$1B1630`) reads current target X/Y or point low-nibble×2,
without a generic point-count guard.

The complete selected command interfaces in
`data/battles/global/aicommandsets.asm` (`$E1AC..$E25B`) are:

| Set | Ordered commands | Temporary pathfinding mode |
| --- | --- | --- |
|HEALER1=0 (ally formation metadata)|HEAL1,SUPPORT,ATTACK1,MOVE1,STAY|REGULAR|
|HEALER5=4|HEAL3,MOVE_ORDER5,SUPPORT,ATTACK2,MOVE3,STAY|BLOCK_AND_CARVE|
|ATTACKER1=6 (also AI-controlled allies)|ATTACK1,HEAL1,SUPPORT,MOVE1,STAY|REGULAR|
|ATTACKER3=9|MOVE_ORDER3,ATTACK3,HEAL2,SUPPORT,MOVE_ORDER4,MOVE2,STAY|BLOCK_NON_MOVABLE|
|LEADER14→list13|MOVE_ORDER5,ATTACK2,HEAL3,SUPPORT,MOVE_ORDER2,MOVE3,STAY|BLOCK_AND_CARVE|

An AI-controlled ally is forced to set6 by StartAiControl,not formation
HEALER1. Ordinary commands execute until one returns success (d1.b=0),
then temporary obstruction flags clear. These are command interfaces,not
selected actions. No concrete movement/heal/attack,Rain use or resource
consumption is resolved in this slice.

## Guard preparation and the action boundary

**Confirmed:** StartAiControl tests enemy kind before follow promotion,
pathfinding modes and commandsets. Kind38 Guards132/135 call
ProcessLineAttackerAi (`battle/ai/startaicontrol.asm`, `$DEFC..$E1AC`),
then @Done clears temporary obstruction flags and restores registers.
Their LEADER14 byte does **not** execute Zeon128's ordinary command list.
15/15 permits this branch even with PRIMARY clear. A changed region-bound
Guard with PRIMARY clear would take standby before the special-kind test.

Preparation is already stateful: BuildTargetsArrayWithAllies clears2304
occupancy bytes atFF5600 toFF,then scans ally0..29 ascending. Its insertion
requires HP.w nonzero,neutral bit3 clear,and unsigned sign-extended X/Y<48.
It does not test membership or signed-positive HP;8000 therefore differs
from region geometry. Later eligible same-square slots overwrite earlier
ones. No enemy,Elis159,script31 or physical47 enters that ally array.

GetLaserFacing (`$1AC8A0..$1AC9B8`) saves/restores d0–a6; its outputs
are RAM writes,not returned facing registers. The full battle list is36,43,0.
The selected43 sixteen-byte table is
`FF,FF,FF,FF,00,FF,FF,02,FF,FF,FF,FF,FF,FF,FF,FF`.
It indexes combatant byte AND7F;132/ordinal4 gets Right0,135/ordinal7
Left2,independent of stored mapsprite facing (initially Down3).
There is no per-table count guard;the selected Guard ordinals are within
this table,not evidence for safe arbitrary retained actor indexes.

A valid direction reads current actor X/Y,clears both2304-byte total-cost
and movable grids atFF4400/FF4D00 toFF,marks the ray squares0,and builds
TARGETS_LIST in spatial order with a final word length. Initial132 at7,9
therefore prepares8,9 through47,9;135 at19,7 prepares18,7 through0,7.
No eligible fresh formation point lies on either line. With only those
ally slots,each Guard conditionally prepares STAY. An eligible ally6 at8,9
instead makes132 prepare action6 PRISM_LASER and item-or-spell word6.
Moving that supplied ally to8,10,setting HP0 or neutral removes it from
this decision;an enemy alone on8,9 cannot trigger it. These are changed
valid-state controls,not complete ray legality or a predicted reached turn.

No matching battle or facingFF writes length0 without rebuilding the ray
grids/target bytes. In the normal Guard branch,ProcessLineAttackerAi tests
the resulting length: nonzero writes action.type.w=6 and zero-extends the
first target byte into offset2;zero writes STAY3 and leaves offset2 stale.
Both write FF to the first move-string byte. Offset4 and other action
fields are not assigned here;no decision RNG is drawn. The list/grids/action
are preparation side effects,not consumed damage targets or animation.

After StartAiControl returns,the caller's common BuildMovementRangeGrid
rebuilds movement costs/grid before Sleep/Stun and final control dispatch.
Thus its eventual grid is not simply the earlier ray grid. The prepared
action/move string remains pending. This slice stops **before ExecuteAiControl**
or the player input implementation. That later AI function itself performs
move-string services and X/Y writes before its STAY/action checks;its laser
branch calls GetLaserFacing again and reads the earlier offset2 word for
target focus. The preparation call is therefore not the last target/grid
authority. Full Guard target reconstruction,legality,damage,reactions and
presentation belong to a later selected action result;the Battle36 owner
is reused without promoting its flower-specific damage to Guard damage.

## Actual producer and consumer support

**Confirmed about production code at this accepted base:** the following
mechanisms exist,but none binds or runs a Battle43 package here. Supported
generic rules,explicit refusals and selected unbound state stay distinct.

| Actual owner | Mechanism and selected43 boundary |
| --- | --- |
|[Encounter preparation](../../src/sf2tool/remake_exploration_battles.py),[final producer](../../src/sf2tool/remake_exploration_content.py),[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py)|prepare_encounter admits2/3;route primitives can express before/load/start. Final prepare compiles/maps/binds before writing the serialized world. The retained after42 special251 refusal/five-op partial is not a successful producer/world. Empty43 hooks remove none of those gates.|
|[Encounter reader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs),[definition reader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs),[scenario assembly](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Pinned reader/assembly admits1/2/3.43 STARTING fits that spawn form,but ENHANCED99,HEALER5,BIT4 forms,equipped refresh and6008 Rain placement do not fit actual admission. Retained definition metadata alone does not execute behavior. No43 terrain/definition/material package is bound.|
|Same assembly,actual derived state|Splits raw BaseAgility directly into low7/high80;widening an allowlist would give Zeon75+extra rather than refreshed72+extra. Admitted Medical Herb placement becomes item0;it cannot substitute for native Shaman00E1,0008,0008,007F or prove Rain consumption.|
|[Initialization](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs)|Roster-only,already-refreshed allies,difficulty0,bounded status/control/loadout and STARTING priority formula exist. Enemy items outside0/127 refuse. No all-slot native stale fields,Zeon special47/ENTNUM32 or neutral159→alias31 reconstruction.|
|[Activation](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs)|Held flags and OR1/OR3 have consumers. This region's fixed nonnegative domain agrees with cross-product Includes,but native uses integer projection. tested starts0 each BeforeRound and initialization clears it;native checked word persists until named AI clears. Uses supplied roster/Position/HP>0,not native30/32 signed slots or160 alias. Nonzero spawn modes refuse.|
|[Turn order](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs),[turn flow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs)|Sequential RNG,extra record,signed stable sort/sentinel are consumed. Numerical Agility≤127 and explicit ExtraRoundAction replace native current byte;deployment processing order replaces fixed offered IDs. Capacity>64 refuses;cursor counts records rather than bytes. No selected Zeon refreshed value is bound.|
|[SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs),[orders](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs),[standby](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/AiStandbyRules.cs)|Consumes15/15 bypass,checked clear,standby,follow promotion/points and active sets6/7/9/14. Nonzero status and unsupported spells/items refuse;HEALER5 is not active support. There is no kind38 line-attacker branch. A generic LEADER primitive cannot stand in for Guard preparation;neutral target words also refuse.|
|[Control](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs),[advancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs)|SleepC0 skips before automatic AI preparation. Stun1 has no same skip;with StartPolicy player admission reports Stunned Unsupported,while SourceEnemyAi refuses nonzero status. EnterPlayer without StartPolicy returns unchanged before classification. Original toggle/F88/control-service ordering is unbound.|
|[BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs),[ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|Ordered entry/round/control continuation exists. Direct Start with NewBattle throws on initial control refusal before returning a session;field entry commits initialization/load and then intro/start prefixes separately,with BattleStartFinished calling Advance. A later refusal retains those prefixes and any published round;it does not undo the entire field entry.|
|[Presentation callback](../../remake/game/src/Exploration/ExplorationPresentation.cs),[battle viewport](../../remake/game/src/Battles/BattleMapViewport.cs)|BattleLoad prepares the active supported battle,and viewport consumes session selection/projection. No43 binding,Guard ray prep,special47/neutral alias fidelity,DMA/music/view completion or playable first-control observation was run.|

BeforeRound validation and GenerateRound compute replacements before publication;
failure there retains the previous committed state/RNG. Failure at a later
control/AI decision does not erase a published round or field prefix. This
is an actual local publication contract,not native instruction-prefix parity.
The earlier accepted Sleep/Stun and direct Start/field-publication corrections
remain binding here.

**Inferred:** future43 integration needs selected content/material admission,
current-stat/loadout conversion,persistent checked state/native slot semantics,
special combat identities and Guard preparation/control support. The reusable
mechanisms above reduce that work but do not make43 ready. This is a dependency
assessment,not an engine repair,acquisition or comparison-platform authorization.

## Reproduction, retained failures and open boundary

The ignored one-off recipe checks selected pinned source/actual consumers,
complete formation/assignments/current values,call/mutation order,widths and
branch polarity,finite geometry,full prose/links/anchors/UTF8/LF,private boundary
and exact owned diff. The initial snapshots are compared directly on later
runs,which retain any newly selected source files separately. It is not a
maintained parser/schema/fixture framework or a test
of a verification program. Tracked prose and exact Git objects own the findings.

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1178/review.py <configured-source-disasm-root> --receipt local/issue1178/direct-reproduction-01.json --snapshots local/issue1178/analysis-01.json --committed
git diff --check origin/main
uv run sf2 verify plan --base origin/main --head HEAD
```

Each receipt must use a **fresh and nonexistent** output path;change the example
after use. The source argument ends in disasm. Load private configuration in
the same process for each uv invocation. Analysis-only receipts omit document
acceptance. Clean committed planner,actual scoped CI,head/tree,process state
and measured resources belong to the frozen Issue/PR handoff. Normal/full
Python,old comparisons,SDK/Godot,emulator/H3/H4 and screenshots are **SKIPPED**
by documentation-only scope. No unfinished inherited suite is restarted.

Preserve [the initialization owner's failures and Unknowns](./battle43-initialization.md#reproduction-retained-failures-and-stopping-boundary).
#1174/#1176 original author/main-gate direct PASS,reviews and CI missed the
predecessor's Zeon ENTNUM128→**ENTNUM32** prose error and the scene coordinate
distinction: declaration62,62 precedes later13,6;combat reconstruction13,6
is another producer. Those passing receipts do not cover the missed claims;
the accepted corrections do not erase their limits. Also preserve after42's
single special251 refusal/five-op partial and14-action eas_Init cache with
no published world;#1172's Sleep/Stun and field-publication missed-finding
PASS and correction;#1170's fresh-receipt command correction and old-head
CI/watcher propagation;#1162's followEntity high-byte/BowieHP missed-finding
PASS and correction;earlier geometry/read failures,Map54 overwrite,Map53
failed import/zero partial and twelve completed old comparison failures.
No old direct/lowering/suite is rerun without dependency invalidation.

ADR0004 review covers provenance,date,full selected shape,widths,aliases,
order/polarity,changed valid states and actual consumers. New production
parser/schema/fixture/mutation-test/H3-callback obligations are N/A. One-off
guards do not claim maintained mutation coverage. This task's initial
source-path lookup failures and self-review corrections remain in its local
handoff;they are not missing source,toolchain or runtime blockers.

**Unknown:** natural party/HP/status/items/positions/flags,checked word,
aliases/deals/cursor/queue/RNG,incoming F88 state,actual first actor and
natural Guard preparation/control reach;walkability/terrain/materials,
interleaved service mutation and interruption,completed view/sprite/DMA/
fade/text/audio/input,hardware timing,save persistence,source-ROM equality
and original/remake playability. Full Guard actions and ending are outside
this result. Supplied arithmetic controls close none of these and create
no automatic runtime/repair queue.

Freeze a clean committed/pushed Draft PR with direct checks,planner,actual
CI and handoff. Main-gate retains independent source/consumer review and
merge. Stop at author delivery;no merge,cleanup,archive or next Issue.
