# Battle36 activation and first-turn dispatch

Evidence date: 2026-10-20. **Confirmed (static):** all twelve declared
formation positions miss all three regions under the original integer
consumer. After unchanged completed fresh initialization, the twenty
STARTING enemies retain their activation words, no region script matches
Battle36 and no spawn is requested. All twenty still contribute turn
records, including the eight MOV0 Prism Flowers. Their 15/15 triggers bypass
the primary-active guard in StartAiControl; enemy kind93 then selects
ProcessLineAttackerAi before the ordinary HEALER1 commandset. This finding
stops at that entry and the corresponding first individual-turn control
interfaces. Natural reach, actual incoming state/RNG/first actor, completed
services, presentation, usable control and persistence remain **Unknown**.
This is not playable Battle36 acceptance.

The successor [Prism Flower laser targeting and results](./battle36-prism-flower-laser.md)
connects that entry to decision occupancy, action reconstruction, damage and
result interfaces; this owner's activation/first-dispatch stopping point remains.

## Agreement and evidence route

Standard rigor for [Issue #1130](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1130)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect the accepted [Battle36 initialization and opening](./battle36-initialization-and-opening.md)
to integer region activation, normal spawn candidates, live turn generation
and the first dispatch. Accepted base is
`627be12223e5f7c1b9aca901657ea36fd6d37171` (#1128/PR1129).
Own this document and navigation only in its predecessor; no shared
index/registry/schema/fixture/design/engine change or stacked dependency.
Individual targeting/pathfinding, laser ray/targetset/attack decision,
damage/animation, whole rounds, outcomes/drops/after scenes and Battle37
are excluded. Unit counts and this stopping point do not define legal play.

Reuse the [Battle35 shared flow](./battle35-activation-and-first-turn-dispatch.md),
[Battle30 integer derivation](./battle30-activation-and-first-turn-dispatch.md#geometry-checked-bits-and-persistent-flags),
[Battle26 alias/RNG derivations](./battle26-activation-and-first-turn-dispatch.md),
[battle loop](./battle-loop.md), [spritesets](./battle-spriteset-data.md),
[RNG/math](./runtime-rng-and-battle-math.md),
[lifecycle](../design/contracts/battle-control-lifecycle.md) and
[cutscene routing](../design/contracts/battle-cutscene-routing.md).
Shared evidence proves its named seam; selected36 operands, branch order,
integer examples and actual current consumers are checked directly here.

Original paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`sf2const.asm`/`sf2enums.asm` own constants and RAM. `battleloop/`, `ai/`
and `cutscenes/` abbreviate `code/gameflow/battle/` children; `stats/`
means `code/common/stats/`. Fnnn denotes flag nnn; ordinals are zero-based,
intervals half-open and activation words hexadecimal. The registered USA
ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new ROM/source equality, terrain decoding or runtime observation is claimed.

Minimum evidence is the selected table/helper/consumer join, 12×3 integer
results and small supplied-state deltas. Reuse the immutable18,285,979-byte
init/script pair and `local/issue1005/pair.txt` provenance (0.083s).
One pair load, linear selected source reads and constant-size deductions
have a one minute/512MiB budget per direct command; peak memory is
unmeasured. Small unique receipts stay in ignored `local/issue1130`.
No canonical reload is needed for this activation/turn investigation;
the predecessor owns its manifest-checked canonical/lowering result.
No re-export, seed enumeration, acquisition, new parser/fixture/platform,
tool tests, normal/full/old suite, SDK, Godot, emulator, H3/H4 or screenshots.
Reassess a material contradiction or supporting-tool expansion before scaling.

## Fresh call order and the retained F88 entrance

**Confirmed:** `code/gameflow/battle/battleloop_1.asm:BattleLoop`
(`$23A84..$23BB4`) reaches @Start after fresh LoadBattle and the empty
start-wrapper return. The predecessor owns Map53/Battle36/crop0,0/28×32,
flags436/486/536, scene replacement, all-ally healing and enemy initialization.
From that boundary the actual sequence is:

1. ActivateEnemies visits128–159 and falls through its helper once for160.
2. j_ExecuteBattleRegionCutscene samples live CURRENT_BATTLE.
3. Nonzero DEBUG_MODE_TOGGLE.b calls PrintAllActivatedDefCons.
4. PopulateTargetsListWithSpawningEnemies rebuilds TARGETS_LIST_LENGTH.w;
   nonzero length drives ascending byte IDs into SpawnEnemyWithCamera,
   with d0.w cleared before loading each ID.
5. GenerateBattleTurnOrder fills/sorts the buffer and clears the cursor byte.
6. @IndividualTurns_Loop clears d0.w, loads CURRENT_BATTLE_TURN.b as a
   **byte offset**, reads the actor byte at BATTLE_TURN_ORDER+offset and
   compares FF. FF returns to @Start; another byte calls ExecuteIndividualTurn.

Later phases require earlier services to return. No transaction rolls back
a reached flag, checked bit, activation word, stat or entity mutation.
Held486 alone proves none of load, opening completion, activation or input.
Actual living positions govern geometry; temporary Geshp/Caravan/four
scene flowers are not an extra combatant roster.

F88 restores seconds, clears88/AI memory/view target, calls LoadBattle
without clearing incoming d0, then branches directly to the individual-turn
entrance. It skips fresh activation/spawning/generation before that read.
The predecessor proves intact admitted map53 reads valid alias offset53
at `$FFB175`, rebuilt for enemy149; skipped149 gives FF/view fallback0.
Fresh reconstruction would set149 X−1, but F88 retains supplied positions
and neutral status. No safe alias/physical-slot/caller assumption follows.

Retained CURRENT_BATTLE_TURN, buffer, checked word, flags, crop, selectors
and RNG remain inputs. Odd offsets can read score bytes;128–255 exceed
the128-byte buffer. A retained FF can subsequently enter @Start with
then-current state. F88 is neither a new-round result nor a successful
restoration/control observation.

## Three regions, integer inclusion and checked state

**Confirmed:** `battleloop/activateenemies.asm` (`$2550C..$25544`)
rejects negative signed X.b, zero currentHP.w, then nonnegative d0.b.
It has no Y, membership, neutral, AI-order or active-bit gate. All twenty
unchanged STARTING enemies pass; absent148–159 fail X. High-bit HP is
nonzero here, unlike the ally geometry scan's positive-signed-HP gate.

The first passing enemy enters
`battleloop/triggerregions.asm:TriggerRegionsAndActivateEnemies`
(file `$1AC9FC..$1ACF30`). For each declared ID0–2, a clear bit in
NEWLY_TRIGGERED_BATTLE_REGIONS.w at `$FFB20C` permits IsRegionTriggered.
A hit sets F90+ID; **both a hit and a miss set its checked bit**. A checked
bit suppresses geometry, not activation from independently held flags.
Fresh entry clears flags90–105 separately. ClearAiMemory resets48 last
targets to FF at `$FFB1AC..$FFB1DC` and48 memory bytes to0 at
`$FFB1DC..$FFB20C`; it leaves the adjacent checked word untouched on both
fresh and F88 routes. Its name does not imply a checked reset.

`data/battles/spritesets/spriteset36.asm` (`$1B5F14..$1B60BE`)
has counts12/20/3/1. Each region is a twelve-byte type4 record, unused0,
these four ordered XY pairs and trailing0,0; point0 is `(8,27)`.

| Region / flag / type | Point1 | Point2 | Point3 | Point4 |
| --- | --- | --- | --- | --- |
|0 /90 /4|0,0|0,31|27,21|6,0|
|1 /91 /4|0,10|0,31|27,31|27,10|
|2 /92 /4|0,22|0,31|27,31|27,22|

The type4 consumer tests triangle1/2/4, then3/2/4 only after a miss.
Each scans all30 ally slots, requiring positive **signed** HP.w and
nonnegative signed X.b/Y.b, without joined/active membership checks.
A positioned living nonmember can trigger; party lists affect fresh
placement upstream, not this scan. HP0 or8000–FFFF and negative X/Y
cannot trigger. Crop, terrain collision and walkability are not predicates.
IsAllyInTriangle returns0 inside; the scan returns−1 for any hit,0 for none.

Coordinates are sign-extended bytes. The edge helper chooses dominant
absolute delta, horizontal on equality, uses signed MULS/DIVS with
truncation toward zero, then adds the projection to endpoint B. It tests
edges1→2,3→1,3→2; bounded collinearity result1 immediately accepts,
otherwise opposite-vertex side comparisons and bit0 exception branches
apply. Negative projection uses `move #1,ccr` and synthesizes2 despite
the comment's3. Region admission's signed byte `cmp.b d1,d7`/`ble`
admits equality with count; FF separately returns false. The normal loop
supplies0–2, not3. All selected vertices are nonnegative/nondegenerate.

| Formation | X,Y | R0 | R1 | R2 |
| --- | --- | --- | --- | --- |
|0|19,5|out|out|out|
|1|19,4|out|out|out|
|2|20,3|out|out|out|
|3|18,3|out|out|out|
|4|20,4|out|out|out|
|5|17,3|out|out|out|
|6|18,4|out|out|out|
|7|19,3|out|out|out|
|8|16,3|out|out|out|
|9|21,4|out|out|out|
|10|21,5|out|out|out|
|11|17,4|out|out|out|

The receipt retains both actual triangle results for all36 predicates.
For region0's upper edge6,0→27,21, y=x−6 gives10–15 at the initial
x16–21; initial y3–5 is above/outside. It is not an axis-aligned rectangle.
The small changed-coordinate table lists **all** hit IDs; it proves
supplied geometry only, without a natural movement or walkability claim.

| Point | Hit region IDs | Point | Hit region IDs |
| --- | --- | --- | --- |
|6,0|0|7,0|—|
|7,1|0|19,9|—|
|19,10|1|20,13|1|
|20,14|0,1|20,15|0,1|
|20,23|0,1,2|20,24|0,1,2|
|20,25|1,2|1,31|0,1,2|
|27,20|1|27,21|0,1|
|27,22|1,2|27,31|1,2|
|28,22|—|0,9|0|
|0,10|0,1|0,21|0,1|
|0,22|0,1,2|0,31|0,1,2|
|8,27|0,1,2| | |

At `(20,24)`, region0's lower edge27,21→0,31 computes
`31+trunc(−10×20/27)=24` and immediately accepts the bounded edge.
The exact line is637/27≈23.593; an exact polygon rejects y24. At `(1,31)`,
`31+trunc(−10/27)=31` likewise accepts while the exact line is below31.
The current Domain's cross-product test differs on these selected inputs.
These are integer truncation differences, independent of floating-point precision.

With unchanged completed fresh placement, flags90–105 remain clear for
**any incoming checked word**. The first passing enemy leaves
`incoming OR $0007`, not necessarily exactly0007. Moving an ally later
does not itself permit a checked miss to run again.

For example supply one eligible ally at `(20,14)`, the others at an
ordinary formation subset, clear flags and checked bits0/1 clear. Geometry
sets F90/F91; region2 misses. With incoming checked0007 and all flags clear,
that same moved ally causes no geometry or activation. With checked0007
and independently held F92, secondary activation still occurs on142/143.
At `(0,22)` with bits0–2 clear, all three flags set, but primary0 wins
over secondary2 for those two rows. Existing secondary bits are retained.
Changing the eligible ally to HP0, signed-negative HP, or negative X/Y
removes its contribution; another valid ally can still trigger.

### The later AI reset is a different boundary

`ai/startaicontrol.asm:StartAiControl` clears this checked word to0 at
@NonSwarmAi **before** testing its two trigger nibbles. The eight normal
flowers use HEALER1, not SWARM15, so their AI preparation reaches this reset.
It clears no F90–105 flag. A later @Start can therefore re-evaluate living
positions, while already held region flags persist. An ally AI branch goes
directly to ATTACKER1 handling and bypasses this enemy reset. Swarm early
return paths are not equivalent to @NonSwarmAi. This is a call-order fact,
not proof that a particular actor naturally reaches it before the next round.

## Complete activation rows and normal spawn candidates

**Confirmed:** `stats/combatantstats_1.asm:GetTriggerRegions` splits
the byte into primary high/secondary low nibbles. Both15 returns without
an activation write. Held non15 primary wins and ORs1; otherwise held
non15 secondary ORs3. Primary preserves an existing secondary bit;
misses preserve the whole word and no path deactivates. Orders do not
enable or disable these region reads. Initial words below come from
the predecessor's definition/STARTING initialization, not AI names.

| Row / ID | Definition | Commandset | Primary order / trigger | Secondary order / trigger | Initial = unchanged result | Changed held flag result |
| --- | --- | --- | --- | --- | --- | --- |
|0 /128|EXECUTIONER|LEADER|MOVE_TO\|0 /0|NONE /15|`2000`|F90→`2001`|
|1 /129|PRISM_FLOWER|HEALER1|NONE /15|NONE /15|`1000`|no activation write|
|2 /130|PRISM_FLOWER|HEALER1|NONE /15|NONE /15|`1000`|no activation write|
|3 /131|PRISM_FLOWER|HEALER1|NONE /15|NONE /15|`1000`|no activation write|
|4 /132|PRISM_FLOWER|HEALER1|NONE /15|NONE /15|`1000`|no activation write|
|5 /133|PRISM_FLOWER|HEALER1|NONE /15|NONE /15|`1000`|no activation write|
|6 /134|PRISM_FLOWER|HEALER1|NONE /15|NONE /15|`1000`|no activation write|
|7 /135|PRISM_FLOWER|HEALER1|NONE /15|NONE /15|`1000`|no activation write|
|8 /136|PRISM_FLOWER|HEALER1|NONE /15|NONE /15|`1000`|no activation write|
|9 /137|CHAOS_WIZARD|ATTACKER3|FOLLOW_ENEMY\|0 /15|NONE /15|`2000`|no activation write|
|10 /138|SHAMAN|HEALER5|FOLLOW_ENEMY\|0 /15|NONE /15|`3000`|no activation write|
|11 /139|DRAGONEWT|SENTRY|NONE /15|NONE /15|`2000`|no activation write|
|12 /140|GRIFFIN|SENTRY|NONE /15|NONE /15|`2000`|no activation write|
|13 /141|BOW_RIDER|SENTRY|NONE /15|NONE /15|`2000`|no activation write|
|14 /142|PURPLE_WORM|ATTACKER3|FOLLOW_ENEMY\|15 /0|FOLLOW_ENEMY\|15 /2|`2000`|F90→`2001`; else F92→`2003`|
|15 /143|BOW_RIDER|ATTACKER1|FOLLOW_TARGET\|0 /0|FOLLOW_TARGET\|0 /2|`2000`|F90→`2001`; else F92→`2003`|
|16 /144|MIST_DEMON|ATTACKER1|FOLLOW_TARGET\|0 /1|NONE /15|`2000`|F91→`2001`|
|17 /145|BOW_RIDER|ATTACKER3|144\|0 /1|NONE /15|`2000`|F91→`2001`|
|18 /146|BOW_RIDER|SENTRY|NONE /15|NONE /15|`2000`|no activation write|
|19 /147|MIST_DEMON|SENTRY|NONE /15|NONE /15|`2000`|no activation write|

MOVE_TO0 is point0 `(8,27)`, coinciding with Executioner128's start.
FOLLOW_ENEMY0 means128; FOLLOW_ENEMY15 means143. FOLLOW_TARGET0
uses ally0. On145, raw144|0 is `$90`, not `$80`: StartAiControl first
tests FF, then bit6, and passes the **whole order byte** to GetCurrentHp
for a nonpoint order. `battlefield/getmoveorderposition.asm` similarly
reads combatant X/Y with the whole byte; only point orders mask the low
nibble. Thus145 follows combatant144 at this interface, whereas `$80`
would address128. If the followed target is dead, ordinary AI preparation
promotes secondary order to primary and writes secondaryFF. No target
selection or movement is executed here.

Absent148–159 initialize activation/maxHP/currentHP0 and X−1 while
other fields can remain stale. Their trigger byte is0F from the unoffset
first enemy record, not twelve absent declarations. Changing only X/HP
can expose F90 activation, but does not initialize their other fields.
Neutral status is not an activation or turn-candidate exclusion; entity
packing independently checks neutral and both coordinates. A slot with
Y−1 can pass activation/turn scans while lacking a physical entity.
Nonnegative X beyond the crop also passes those scans. No safe alias or
runtime result follows from those malformed/supplied states.

After32 DBF iterations, d0.w=160/d7.w=FFFF falls directly into
UpdateEnemyActivationBitfield; its RTS returns to BattleLoop. Jump aliases
and GetCombatantEntryAddress's `bhi` admit160. The
[accepted RAM/deals derivation](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
gives `$FFE800+(160−96)×56=$FFF600`, CURRENT_GOLD. Pseudo-X `$FFF62E`
packs deals84/85, pseudo-HP `$FFF60E` packs20–23, trigger byte `$FFF636`
packs100/101 and activation word `$FFF634` packs96–99. Nonnegative pseudo-X,
nonzero pseudo-HP and a held non15 trigger flag90–104 can OR1/OR3 into
Goddess Staff99's count nibble. Unchanged36 has no held region flags,
so no activation write even if that guard passes. Changed flags differ.
This is not unconditional item delivery or persistence;160 is absent
from the normal spawn and turn scans.

### Hook table, debug requests and zero normal spawns

`data/battles/cutscenes/regionactivatedcutscenes.asm` has four eight-byte
rows followed by wordFFFF:

| Battle | Region | Played flag | Program |
| --- | --- | --- | --- |
|32|0|386|rbcs_battle32|
|32|1|386|rbcs_battle32|
|40|0|387|rbcs_battle40_1|
|40|1|388|rbcs_battle40_2|

`cutscenes/regionactivatedcutscenes.asm:ExecuteBattleRegionCutscene`
(`$47E82..$47EC8`) misses all rows while the live selector stays36,
before played/region checks or MAPSCRIPT. There is no empty Battle36
region story program to invent or lower. A changed32/40 can match;
played check precedes region check, SetFlag precedes MAPSCRIPT, and
the wrapper returns after one selected program. That is a changed
selector input, not a Battle36 hook; set played does not prove completion.

PrintAllActivatedDefCons tests all sixteen flags90–105 in order and
requests normal txt463 once per held flag, with DIALOGUE_NUMBER.l=index0–15,
then closes text. Full ordered tokens are `["{#}","{N}","{D3}"]`;
the short meaning is a diagnostic region number. No fixed scene speaker
or portrait is selected here. Unchanged36 requests none even with debug
enabled. The changed `(20,14)` case requests indices0,1; `(0,22)` requests
0,1,2. Other held flags can request undeclared region numbers. Requests
are not completed text/window/service observations.

`battleloop/populateenemyspawns.asm` (`$1ACF30..$1AD090`) scans128–159,
masks each activation word with0F00 and rebuilds TARGETS_LIST_LENGTH.w.
All twenty STARTING words1000/2000/3000, their OR1/OR3 variants, and
absent0 mask to0 and skip further work. There are **no HIDDEN rows**.
Length becomes0, stale list bytes are unconsumed and appearance is bypassed.
CurrentHP0 alone never converts a mode0 slot into a spawn candidate.

This is not a universal safe rejection of modified state. Mode0200
requires currentHP0, a non15 held trigger, then maxHP0;0100 requires
currentHP0 without a region check;0300 uses the dead/trigger helper
without0200's extra maxHP requirement. Other modes append nothing.
The dead helper has no X/Y/neutral test and can write active bits before
a later maxHP/occupancy refusal.

ResetSpawningEnemyStats uses live enemy count20 OR80 as the exclusive
bound148. A declared dead flower129 changed to mode0100/X−1, with its
start `(11,7)` unoccupied, can reach reset and append on success.
It is no longer the unchanged STARTING case. At/above148 the reset
rejects; this does not make arbitrary combatant/alias state safe.
Occupancy scans all30 allies/32 enemies without HP, neutral or self
exclusion, so a dead actor at its own start blocks itself. Invalid or
occupied reset sets max/currentHP0, X−1 and carry. Success initializes
the row/definition/current stats then restores the saved **full activation
word**; it does not only restore active bits. Later mode comparisons
use the returned full d1 rather than remasking.

No SpawnEnemyWithCamera call occurs in the normal selected case. The
shared max-alias+1 allocation, capacity/DMA/cursor/view/spin/audio/text
services remain conditional interfaces, not a Battle36 appearance proof.
No spawn story dialogue is added by the zero-length list.

## Live current AGI, sequential RNG and round storage

**Confirmed:** the predecessor owns full InitializeEnemyStats→refresh:
definition copy, HP/MP fill, placement/orders/triggers/items, activation,
difficulty ATT and UpdateCombatantStats. Battle36 is absent from the
random-battle list, so intact36 fails IsBattleUpgradable before level,
category or upgrade RNG, even with536 held. Its five equipped weapons
change ATT only; difficulty does not change AGI. All nine definitions
start status0. The following base current AGIs are therefore valid after
unchanged completed fresh initialization; later HP/status/equipment/current
state can differ. Prowess double-attack chances are not extra round records.

| Definition / IDs | Current HP | Current AGI byte | First range | Score envelope |
| --- | --- | --- | --- | --- |
|EXECUTIONER /128|64|45|5|40..50|
|PRISM_FLOWER /129,130,131,132,133,134,135,136|62|44|5|39..49|
|CHAOS_WIZARD /137|53|59|7|52..66|
|SHAMAN /138|56|40|5|35..45|
|DRAGONEWT /139|60|46|5|41..51|
|GRIFFIN /140|60|48|6|42..54|
|BOW_RIDER /141,143,145,146|61|44|5|39..49|
|PURPLE_WORM /142|58|43|5|38..48|
|MIST_DEMON /144,147|68|51|6|45..57|

These are algebraic score envelopes; correlated sequential draws need
not attain each extreme. They are not observed scores or probabilities.

Each flower starts AGI44/MOV0, HP62/MP0. MOV0 does not exclude a turn
or the special kind branch. STUN refresh would reduce AGI5; later SLOW,
equipment and live raw high-bit state must use actual refresh, not an
immutable opening-table lookup. Zero-status AGI helpers also preserve
the source's numerical/high-bit coupling; arbitrary high-bit refresh is
not established by a separate boolean ExtraRoundAction representation.

`battleloop/turnorderfunctions.asm` (`$25544..$25610`) initializes64
two-byte records at `$FFF71A` toFFFF. It offers allies0–29, then
enemies128–157 in ascending ID order;158/159/160 are excluded. Each
rejects negative signed X.b or zero HP.w, with no Y, membership, neutral,
activation, MOV or positive-signed-HP gate. Inactive128/142–145 participate,
and the fifteen15/15 rows need no active bit to contribute. Absent148–157
fail X; supplied changed slots can alter the candidate population.

Let A=currentAGI&127 and R(n) be the next **direct** GenerateRandomNumber
draw. First score is `A+R(A>>3)−R(A>>3)+R(3)−1`. Raw current byte>=128
immediately appends a second record using B=floor(A×5/6), score
`B+R(B>>3)−R(B>>3)`; these draws precede the next candidate. Word
arithmetic stores only low score bytes. For these small ranges the
generator updates `seed=(seed×13+7)&65535`, returning
floor(seed×n/65536), including a seed update when n=0. It bypasses the
debug override. Accepted RNG fixtures cover their named seams; they
do not identify Battle36's incoming seed.

Unchanged enemies append128..147: **20 records/60 draws**, all single.
For N eligible normally placed allies and T of those with rawAGI>=128,
the intact case has `20+N+T` records and `60+3N+2T` turn draws, at most
**44 records/120 draws** for the twelve-slot placement. These bounds
exclude earlier service/debug draws and say nothing about their seed.
For arbitrary changed eligible candidates C and high-bit candidates D,
use `C+D` records and `3C+2D` draws, not the opening count.

Minimal supplied-state checks isolate the next actor's draw stream:

| Candidate supplied alone at seed0 | Ranges in call order | Scores | Seed after |
| --- | --- | --- | --- |
|rawAGI44|5,5,3|43|1281|
|rawAGI172 (44 with bit7)|5,5,3,4,4|43,36|19979|
|rawAGI0|0,0,3|255 (word−1)|1281|
|HP0 or X−1, any rawAGI|none|no record|0|

The first three seeds are7,98,1281; the extra two are16660,19979.
Range0 returns0 while still consuming a draw. These are deterministic
supplied inputs, not natural-state probabilities or an enumerated seed
distribution. Removing an eligible flower removes its three draws and
changes later candidates' input seed; setting only its activation bit
does not affect turn admission. Equal AGI does not imply equal scores.

Sorting performs62 passes of63 adjacent **signed byte** comparisons
across all64 records, swapping only if the right score is strictly
greater. Equal scores retain append order: allies before enemies, lower
offered ID first, an actor's immediate extra record after its first on
equality. Padding255 is−1; wrapped135 is−121. Existing rawAGI128 score0/255
and rawAGI127 score135 fixtures prove those boundaries. A score255 tie
does not turn an actor byte into the FF sentinel. Original append has no
capacity guard and62 passes do not guarantee full sorting for arbitrary64
entries. Supplied30-allied/30-enemy high-bit states can overflow; the
twelve-slot intact bound is not a universal safety guarantee.

Generation clears CURRENT_BATTLE_TURN.b at `$FFF79A`, directly after
the128-byte buffer, so fresh dispatch reads offset0. Empty eligibility
leaves FF and repeats @Start; changed negative scores can sort behind
padding. In the unchanged base enemy set all scores are positive and
fit capacity. Chaos Wizard137's52..66 exceeds every base flower's39..49,
so a flower cannot be first among that unchanged enemy set. Griffin/Mist
Demon and actual ally scores/RNG still prevent naming the global first
actor from base AGI or LEADER/priority alone. Changed HP/AGI/eligibility
can remove that constraint; it is not a hard-coded first-actor rule.

## First individual-turn control and the flower kind branch

**Confirmed:** `battlefunctions/executeindividualturn.asm:ExecuteIndividualTurn`
(`$23EB0..$24242`) initializes the battlefield sprite frame counter,
clears speech SFX, creates its stack frame, masks actor with
COMBATANT_MASK_ALL and clears the dead list length. Its Taros/Bowie
special flag112 branch is not selected by intact36. It then reads **live**
currentHP.w: zero jumps to @Done before position/control work. A
previously queued actor can therefore be skipped after an intervening
HP change; generation did not reserve a future turn unconditionally.

For a living actor it stores X/Y into actor and target coordinates,
clears cursor radius, resolves its entity alias/view target and sets the
cursor destination. No new off-map/neutral guard repairs malformed turn
candidates here. Control checks then run in this order:

1. MUDDLE mask$30 or activation AI_CONTROLLED mask$0004 (bit2) routes to StartAiControl.
2. Otherwise an enemy with CONTROL_OPPONENT_TOGGLE=0 routes to AI;
   nonzero permits the player path. An ally with AUTO_BATTLE_TOGGLE
   nonzero routes to AI; otherwise it takes the player path.
3. StartAiControl returning falls through the common @PlayerControl setup:
   wait cursor/view, clear targeting, set MOVING_BATTLE_ENTITY_INDEX,
   open mini-status/land-effect windows, build movement range and blocks,
   hide cursor. The label is not proof of human input ownership.
4. Live SLEEP mask$C0 then STUN mask$01 (bit0) choose @NoAction before player/AI
   execution. The earlier AI preparation can already have occurred.
   The control predicates are read again afterward: player reaches
   ProcessBattleEntityControlPlayerInput, AI reaches ExecuteAiControl.

Those are the interface stops; input loops, movement planning, selected
actions, no-action cleanup, after-turn effects and battle scenes are not
executed here. Waits/windows/range setup remain requested services, not
completed player control. A changed status/control word between the two
reads can change dispatch; no immutable control snapshot is assumed.

In `ai/startaicontrol.asm` (`$DEFC..$E1AC`), an AI-controlled ally
selects ATTACKER1 and bypasses the enemy branch. An enemy first gets its
actual commandset; SWARM15 alone enters the full-HP/swarm special checks.
None of the twenty selected commandsets is SWARM. @NonSwarmAi clears
the checked word as above, then reads the two trigger regions:

| Actual input at that read | Preparation route |
| --- | --- |
|15/15, regardless of primary-active|@HandleActivatedCombatant|
|Either trigger non15, primary-active bit0 clear|DetermineAiStandbyMovement, request STAY, then @Done|
|Either trigger non15, primary-active bit0 held|@HandleActivatedCombatant|

Secondary-active alone is not sufficient for this guard; normal secondary
activation sets **both** bits. NONE orderFF and NONE trigger15 are
different interfaces. On an unchanged first AI dispatch,128/142–145
can take standby while the other fifteen rows bypass that activation
guard. This does not choose their later individual action.

@HandleActivatedCombatant tests enemy kind **before** ordinary move-order
promotion, pathfinding mode and commandset execution. Kind93 PRISM_FLOWER
calls ProcessLineAttackerAi, then @Done; ZEON_GUARD and BURST_ROCK have
separate subsequent kind branches. Thus unchanged flowers129–136, if
living and actually routed to AI, pass15/15 even with word1000 and reach
the line-attacker entry. The reason is kind93 and branch order, not
HEALER1's name, LASER in the spell list, MOV0 or active-bit fabrication.
Changed non15 triggers/clear primary-active can send even a kind93 slot
to standby before the kind test; changed enemy kind can bypass it.
Opponent player control can bypass StartAiControl entirely.

The predecessor owns the eight laser-facing table operands. This slice
stops **before the body of ProcessLineAttackerAi**: no ray targetset,
target/attack decision, attack/no-target result, damage, animation or
later scene/service claim is made. Other ordinary AI work likewise stops
at its preparation/control interface, without executing its commandset.

## Actual current consumers and missing Battle36 binding

**Confirmed about accepted code:** production consumes prepared Content;
disassembly is not an on-demand engine input. The
[capability owner](../../remake/docs/capability-status.md) governs runnable
scope. The predecessor retains selected53 opening/lowering, native
cameraSpeed/AnimSpeedx2, equipment and F88 boundaries. The following
checks read actual consumers without SDK/adapter/Godot execution.

| Actual owner | Existing expression or behavior | Selected gap or difference |
| --- | --- | --- |
|[Encounter preparation](../../src/sf2tool/remake_exploration_battles.py), [BattleEncounterDefinition](../../remake/src/Sf2.Remake.Application/Content/Scenarios/BattleEncounterDefinition.cs), [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs)|Placements, orders, regions, point, spawn and provenance shapes exist. All selected36 coordinates fit28×32; all three type4 records have distinct vertices/nondegenerate triangles.|Preparation admits2/3; Pin admits1/2/3 and rejects36. Shape predicates do not bind36 terrain, definitions, route, opening or materials.|
|[PrivateBattleScenarioReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|STARTING, HEALER1/ATTACKER1/ATTACKER3/LEADER, numeric AGI plus separate extra action and source order expressions have representations.|region-program-table admits1/2/3. SENTRY/HEALER5 fail source-commandset; MAGE/CENTAUR_ARCHER fail source-mover; all selected ENHANCED definitions fail REGULAR-only enemy-derived-state and selected BIT4 also fails source-ai-word. These are inspected guards, not observed36 runtime errors.|
|[BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs)|Primary/secondary OR precedence, mode0/None-program policies and activation-before-generation exist.|Exact cross products reject native region0 hits20,24 and1,31. Local tested=0 every round and initialization's checked0 differ from native retained checked until enemy AI reset. Validated actors omit raw160/deals, all30 nonroster slots and signed-HP/malformed-position distinctions. Nonzero spawn modes refuse spawn-mode; five equipped enemy loadouts refuse enemy-equipment-refresh.|
|[BattleTurnFlow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs), [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs), [BattleRandom](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleRandom.cs)|Numerical AGI<=127 plus extra flag, sequential draws,64 slots and signed62×63 sort exist. GenerateRound clears the managed cursor and preserves low half of MainSeed.|Queue uses actor Agility plus definition ExtraRoundAction; arbitrary live raw-high-bit refresh coupling is not established. Explicit overflow refusal and validated candidate state differ from unchecked native writes. No selected36 actual seed/queue is bound.|
|[RuleCompositions](../../remake/src/Sf2.Remake.Application/Gameplay/RuleCompositions.cs), [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs)|source-orders binds the actual SourceEnemyAi; it clears managed Tested, uses15/15 bypass and primary bit0, with admitted standby/order interfaces.|It has no Prism Flower kind93/ProcessLineAttackerAi dispatch. Activated commandset0 refuses source-commandset; admitted activated sets6/7/9/14 then require supported unequipped physical-only categories. A generic HEALER1 shape is not a laser implementation. Source-follow-binding is required for whole-byte orders such as `$90`.|
|[BattleAdvancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs), [BattleControlRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs), [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|BattleStartFinished invokes BattleAdvancer. It exposes candidates/draws/queue, skips dead entries and reaches player admission or automatic decisions. Player classification names muddle/AI/control/sleep/stun refusals.|Outcome check precedes generation; sleeping entries skip before source AI preparation, unlike the native service order. Deployment control is constrained, not arbitrary native cheat/control switching. F88 refuses suspended-battle-entry. region-program-none/spawn-modes-admitted observations describe policies, not original script/spawn execution; transactional failure does not reproduce partial native prefixes.|
|[BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs), [ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs)|Existing board, cursor/actor, sprite/text/audio/effect projection consumers exist.|No selected36 laser targeting/damage/visual binding follows. Crop-origin board selection differs from original actual view/alias fallback. Original cursor/view/DMA/text/audio timing, complete presentation, usable control and persistence are unproved.|

Battle01–03 support and offline scene compilation do not establish36
playability. Minimum integration decisions include selected content/materials,
integer/checked policy, actual current stats/equipment, supported control
and the separate special-kind behavior boundary. This research identifies
them without authorizing implementation or an acquisition platform.

## Reproduction, preserved results and Unknowns

The ignored recipe checks pinned clean source/unchanged pair, selected
12/20/3/1 table,36 formation predicates and23 changed coordinates,
activation/checked/160 joins, complete hook/debug tokens, normal spawn
absence/modified-mode boundaries, definitions/currentAGI, turn/RNG/sort,
F88/first control and special-kind branch, plus actual consumers. It also
checks two-path/nav-only scope, UTF8/LF/local links and the private boundary.
Load current ignored selections in the same PowerShell process, then
use an unused receipt name:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1130/review.py <configured-source-disasm-root> --receipt local/issue1130/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The source argument ends in disasm and contains sf2enums.asm, code/ and
data/. Review the cached diff and actual exact-head scoped CI, not suggested
commands as executed passes. ADR0004's applicable semantic/provenance,
branch/order/width/selector/state/private checklist applies; maintained
parser/schema/fixture/H3 requirements are N/A because none change.
Suggested normal verification is **SKIPPED** under explicit documentation
scope, not a passed execution. Normal/full/old comparisons, SDK/runtime,
screenshots and export remain excluded.

Preserve #1128's original4c672cb9 author/main/CI success that missed three
prose defects, and accepted4556d184 focused corrections/CI39542303146:
clear bits5/6 disable collision checks; HasSameDestinationAsOtherEntity
tests moving a0's bit5; global leader-presenceFF is nonzero. No full audit
rerun is implied. The predecessor and ignored `local/issue1128/handoff.md`
retain the exact original/focused results.

The [admission owner](./roft-to-galam-and-battle36-admission.md) retains
the completed event-enabled Map53 import UnboundLocalError/zero partial
programs and native item/flash/copy/reload boundaries. The
[Roft owner](./roft-town-interactions-and-zynk-recruitment.md) retains
#1124's missed alias0/focused correction; the
[Nazca/Roft owner](./nazca-to-roft-and-battle35-admission.md) retains
collision correction; [Dojo](./dojo-exploration-and-sheela-recruitment.md)
retains follow2=(−24,0)/local-action refusal;
[post-crash](./post-crash-ship-and-nazca-overworld-return.md) retains
complete-import/service/reload limits; the
[Battle34 outcomes owner](./battle34-outcomes-and-sky-orb-handoff.md)
retains the statistics correction. Twelve old comparison failures remain
completed discovery evidence with their named owners, not interrupted
work or an automatic rerun queue. No selected36 result closes them.

Freeze a clean committed/pushed Draft PR with exact base/head/tree,
checks/actual CI/failures/Unknowns and ignored handoff; move the Project
to Review / Verify, notify main-gate once and stop writer/processes.
Independent acceptance/merge remains main-gate-owned; no next Issue,
merge, cleanup, archive or private input publication follows.

**Unknown:** natural entry/region reach and actual party/flags/checked,
HP/MP/status/class/equipment/currentAGI/positions/aliases/deals; selector
or control changes and retained F88 caller/scratch/queue state; incoming
RNG, exact first actor and reached AI/control interface; decoded collision/
terrain/materials and source/ROM equality beyond existing evidence;
interrupted/interleaved and completed load/debug/cursor/view/window/
sprite/DMA/text/audio services, VInt/render timing, usable input and
save/load persistence. Laser body, individual actions/combat and outcomes
remain beyond this stop. No Unknown is a passed gate or acquisition queue.
