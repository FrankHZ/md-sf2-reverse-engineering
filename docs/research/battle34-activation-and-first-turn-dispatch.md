# Battle34 activation and first-turn dispatch

Evidence date: 2026-10-20. **Confirmed (static):** all twelve declared
formation positions miss all six Battle34 regions under the original
integer consumer. With unchanged completed fresh initialization, region
flags remain clear, activation words retain their initial values, no
region scene is selected and the spawn list is empty. The seventeen
enemies nevertheless contribute eighteen turn records: Cameela's current
AGI byte186 represents numerical58 plus an extra round action. Stop at
the first ExecuteIndividualTurn call, before its body. **Unknown:** natural
reach, incoming party/checked word/RNG, first actor, completed services,
presentation, usable control and persistence. This is not a playable34 claim.

## Agreement, sources and reproduction scope

Standard rigor for [Issue #1102](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1102)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect accepted fresh initialization to activation, spawn admission,
current stats, round generation and first dispatch for later content and
engine decisions. Dependency is [Battle34 initialization and the
Cameela/Geshp opening](./battle34-initialization-and-opening.md), accepted
#1100/corrected PR1101, base `00e92ba7d06f9242549283da3da9ad54814a7552`.
Own this document and a navigation-only link there. No shared registry,
schema, fixture, design or implementation changes; no stacked dependency
or competing research writer. Individual-turn AI/movement/actions/combat,
outcomes and later Map55/ship exploration are excluded. Unknowns do not
authorize acquisition or a new comparison platform.

Reuse the [Battle33 shared flow](./battle33-activation-and-first-turn-dispatch.md),
[Battle30 integer derivation](./battle30-activation-and-first-turn-dispatch.md#geometry-checked-bits-and-persistent-flags),
[Battle26 alias/RNG derivations](./battle26-activation-and-first-turn-dispatch.md),
[battle loop](./battle-loop.md), [spritesets](./battle-spriteset-data.md),
[RNG/math](./runtime-rng-and-battle-math.md),
[lifecycle](../design/contracts/battle-control-lifecycle.md) and
[cutscene routing](../design/contracts/battle-cutscene-routing.md).
Shared fixtures establish only their named seams; selected34 declarations,
branches, item/status refresh and consumers below are checked directly.

Original paths are relative to `disasm/` at clean read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`sf2const.asm`/`sf2enums.asm` own constants, widths and RAM. `battleloop/`
and `cutscenes/` abbreviate children of `code/gameflow/battle/`; `stats/`
means `code/common/stats/`. Fnnn means flag nnn; ordinals are zero-based,
intervals half-open. USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new ROM/H1 equality or runtime observation is asserted.

Minimum evidence is complete selected source/table/consumer joins,
12×6 integer predicates and small changed-point/status examples. Reuse
the unchanged18,285,979-byte init/script pair and0.083s provenance at
`local/issue1005/pair.txt`. One pair load, bounded linear source reads and
constant-size arithmetic fit the estimate of one minute/512MiB per direct
audit, with small ignored receipts in `local/issue1102/`; no bulk scan,
seed enumeration, extraction, forced scene lowering, new environment,
runtime, SDK, Godot, emulator, screenshot or old comparison. A material
contradiction or supporting-tool expansion requires reassessment.

## Fresh phase order and the separate F88 handoff

**Confirmed:** `code/gameflow/battle/battleloop_1.asm:BattleLoop`
(`$23A84..$23BB4`) reaches @Start after fresh LoadBattle and the empty
start-wrapper return. The predecessor owns the before scene, independent
F76 reads, F484 wrappers, all30-ally healing, list/formation construction,
17-enemy initialization and Map55 reload. @Start then:

1. Calls ActivateEnemies for128–159, then its fall-through helper for160.
2. Calls j_ExecuteBattleRegionCutscene using the live battle selector.
3. Calls PrintAllActivatedDefCons if DEBUG_MODE_TOGGLE.b is nonzero.
4. Rebuilds TARGETS_LIST_LENGTH.w; zero skips spawning. Otherwise reads
   each byte ID with d0.w cleared and DBF-calls SpawnEnemyWithCamera.
5. Generates/sorts the turn buffer and clears CURRENT_BATTLE_TURN.b.
6. Clears d0.w, loads that **byte offset**, reads BATTLE_TURN_ORDER at it
   into d0.b, and tests FF. FF returns to @Start; another byte calls
   ExecuteIndividualTurn. Stop before that body's work.

Earlier services must return for later phases to run. There is no
transaction/rollback; held484 is not proof of completed opening/load/input.
Allies use rebuilt sorted joined/active lists, at most twelve list
iterations; dead members consume an iteration without a formation row.
Other ally X bytes are−1. Declarations do not prove twelve actual living
members or their identities. Empty/stale and oversized party behavior
retains the predecessor's explicit limits. Scene Zynk/Caravan coordinates
are not combatant placements; F76 does not recruit Zynk.

F88 restores seconds, clears88/AI memory/view target, calls LoadBattle
without clearing incoming d0, and branches directly to @IndividualTurns_Loop.
It skips fresh activation/spawning/generation before that first read. The
accepted intact d0.w=55 resolves retained combatant151's rebuilt alias,
not fresh combatant0; missing aliasFF selects the view fallback. Supplied
CURRENT_MAP/BATTLE, crop, roster, offset and queue govern. Odd offsets can
read score bytes;128–255 exceed the128-byte buffer. An FF read can later
enter @Start, with actual supplied state. No resumed round or safe arbitrary
restore is claimed here.

## Six regions, original integer geometry and checked state

**Confirmed:** `battleloop/activateenemies.asm` (`$2550C..$25544`)
rejects negative signed X.b, then zero currentHP.w, then nonnegative d0.b.
There is no Y, membership, neutral, AI command or activation-bit gate.
All17 fresh declared enemies pass; absent145–159 fail X. High-bit HP is
nonzero for this test. The first passing enemy invokes
`battleloop/triggerregions.asm:TriggerRegionsAndActivateEnemies`
(`$1AC9FC..$1ACF30` file) with the live battle's six regions.

For each unchecked bit of NEWLY_TRIGGERED_BATTLE_REGIONS.w at `$FFB20C`,
IsRegionTriggered can set F90+index. Both hit and miss set the checked
bit. Checked suppresses geometry, not activation from independently held
source flags. Fresh entry cleared90–105 separately. ClearAiMemory writes
48 last-target bytesFF at `$FFB1AC..$FFB1DC` and48 AI-memory bytes0 at
`$FFB1DC..$FFB20C`, with subq/BNE. It leaves the checked word at
`$FFB20C..$FFB20E` untouched, on both fresh and F88 routes.

`data/battles/spritesets/spriteset34.asm` has counts12/17/6/1. Each region
is twelve bytes: type, unused0, four ordered XY pairs, two trailing zeros:

| Region / flag / type | Point1 | Point2 | Point3 | Point4 |
| --- | --- | --- | --- | --- |
|0 /90 /4|31,0|0,31|15,31|31,15|
|1 /91 /4|31,0|0,31|7,31|31,7|
|2 /92 /4|0,0|0,31|9,31|25,0|
|3 /93 /3|0,0|0,31|31,0|0,0|
|4 /94 /4|0,0|0,19|8,19|8,0|
|5 /95 /4|8,0|8,19|22,5|22,0|

Type3 tests **1/2/3 once**, ignoring its repeated stored point4. Every
other type takes the type4 branch: triangle1/2/4, then3/2/4 only after a
miss. Each triangle scans all30 ally slots, requiring positive **signed**
HP.w and nonnegative signed X.b and Y.b, without joined/active tests.
IsAllyInTriangle returns0 inside; the scan returns−1 on any eligible hit,
0 otherwise. This differs from enemy activation and turn admission.

The checked integer consumer sign-extends coordinate bytes, uses the
dominant absolute delta (horizontal on equality), signed MULS/DIVS with
truncation toward zero, and adds projection to endpoint B. Edges are1→2,
3→1,3→2; bounded collinearity result1 immediately accepts. Otherwise the
point/opposite-vertex sides and bit0 exception branches apply. Negative
projection uses `move #1,ccr` and synthesizes2, despite the comment's3.
Index admission is `cmp.b d1,d7`/`ble`, so equality with count is admitted;
FF separately returns false. Normal six-region iteration supplies0–5,
not6. These finite coordinates are nonnegative and the tested triangles
are nondegenerate; no floating-point or ideal-polygon replacement is used.

| Formation | X,Y | R0 | R1 | R2 | R3 | R4 | R5 |
| --- | --- | --- | --- | --- | --- | --- | --- |
|0|28,29|out|out|out|out|out|out|
|1|29,29|out|out|out|out|out|out|
|2|29,30|out|out|out|out|out|out|
|3|30,29|out|out|out|out|out|out|
|4|30,30|out|out|out|out|out|out|
|5|28,30|out|out|out|out|out|out|
|6|27,30|out|out|out|out|out|out|
|7|27,29|out|out|out|out|out|out|
|8|27,28|out|out|out|out|out|out|
|9|28,28|out|out|out|out|out|out|
|10|29,28|out|out|out|out|out|out|
|11|30,28|out|out|out|out|out|out|

All72 results include the actual triangle ordering. Changed coordinate
predicates below list **all** hit region IDs; a dash means none. They
are arithmetic examples, not walkability, movement or natural-reach claims.

| Point | Hits | Point | Hits |
| --- | --- | --- | --- |
|0,0|2,3,4|0,31|0,1,2,3|
|31,0|0,1,3|15,16|0,1,2,3|
|15,17|0,1,2|15,23|0,1|
|15,24|0|8,19|2,3,4,5|
|9,19|2,3|9,31|0,2|
|10,31|0|25,0|2,3|
|26,0|3|22,5|2,3,5|
|22,6|3|8,20|2,3|
|31,15|0|31,16|—|
|17,16|0,1|17,15|0,1,2|
|10,30|0|10,29|0,2|

With unchanged completed fresh placement, any living subset still misses;
all other X bytes are−1. Thus F90–105 remain clear for **any incoming
checked word**. First passing enemy makes that word `incoming OR $003F`,
not necessarily exactly003F. A checked miss can suppress a later moved
ally; positioned living nonmembers can trigger; a held flag can activate
after skipped/missing geometry. The checked word and the source flags are
independent authorities with different lifetimes.

## Seventeen activation rows, absent slots and fall-through160

**Confirmed:** GetTriggerRegions splits its stored byte into primary high
and secondary low nibbles. Both15 returns with no activation write. Held
non15 primary wins and ORs1; otherwise held non15 secondary ORs3. A primary
write preserves preexisting bit1; misses preserve the whole word, and no
path deactivates. The source AND/BSET pairs do not select an action here.
All rows are STARTING/NOTHING/filler0; words below are hexadecimal.

| Row / ID | Definition | Primary order / trigger | Secondary order / trigger | Initial = fresh result | Changed held flag result |
| --- | --- | --- | --- | --- | --- |
|0 /128|CAMEELA|MOVE_TO\|0 /4|MOVE_TO\|0 /5|`1000`|F94→`1001`; else F95→`1003`|
|1 /129|NECROMANCER|FOLLOW_ENEMY\|0 /15|NONE /15|`2000`|no activation write|
|2 /130|SHAMAN|FOLLOW_ENEMY\|0 /15|NONE /15|`3000`|no activation write|
|3 /131|CERBERUS|FOLLOW_ENEMY\|4 /15|NONE /15|`2000`|no activation write|
|4 /132|PEGASUS_KNIGHT|NONE /15|NONE /15|`2000`|no activation write|
|5 /133|BOW_MASTER|FOLLOW_ENEMY\|4 /15|NONE /15|`2000`|no activation write|
|6 /134|MUD_MAN|FOLLOW_ENEMY\|7 /0|FOLLOW_ENEMY\|7 /3|`2000`|F90→`2001`; else F93→`2003`|
|7 /135|DRAGONEWT|FOLLOW_TARGET\|0 /0|FOLLOW_TARGET\|0 /3|`2000`|F90→`2001`; else F93→`2003`|
|8 /136|MUD_MAN|FOLLOW_ENEMY\|7 /0|FOLLOW_ENEMY\|7 /3|`2000`|F90→`2001`; else F93→`2003`|
|9 /137|MUD_MAN|FOLLOW_TARGET\|0 /1|FOLLOW_TARGET\|0 /3|`2000`|F91→`2001`; else F93→`2003`|
|10 /138|CERBERUS|FOLLOW_ENEMY\|9 /1|FOLLOW_ENEMY\|9 /3|`2000`|F91→`2001`; else F93→`2003`|
|11 /139|PEGASUS_KNIGHT|FOLLOW_ENEMY\|10 /1|FOLLOW_ENEMY\|10 /3|`2000`|F91→`2001`; else F93→`2003`|
|12 /140|BOW_MASTER|FOLLOW_TARGET\|0 /1|FOLLOW_TARGET\|0 /3|`2000`|F91→`2001`; else F93→`2003`|
|13 /141|DRAGONEWT|FOLLOW_ENEMY\|12 /1|FOLLOW_ENEMY\|12 /3|`2000`|F91→`2001`; else F93→`2003`|
|14 /142|PEGASUS_KNIGHT|FOLLOW_ENEMY\|13 /1|FOLLOW_ENEMY\|13 /3|`2000`|F91→`2001`; else F93→`2003`|
|15 /143|DRAGONEWT|NONE /15|NONE /15|`2000`|no activation write|
|16 /144|DRAGONEWT|FOLLOW_TARGET\|0 /2|NONE /15|`2000`|F92→`2001`|

MOVE_TO0 references the separate AI point0 `(7,7)` for both Cameela orders;
FOLLOW_TARGET0 is a target operand, not that point. FOLLOW_ENEMY operands
are local ordinals0=Cameela128,4=Pegasus132,7=Dragonewt135,9=Mud137,
10=Cerberus138,12=Bow140,13=Dragonewt141. Real orders with sentinel15 do
not create region triggers. LEADER, SENTRY and priority do not establish
tactics or first actor. No movement/order execution is claimed.

Absent145–159 initialize activation/maxHP/currentHP0 and X=−1, but retain
other Y/type/inventory state. Their triggers are45 from the unoffset first
enemy record, not fifteen new rows. Changing only X/HP can expose F94/F95
activation, without making them fully initialized declared enemies.

After32 DBF iterations d0.w=160/d7.w=FFFF falls directly into
UpdateEnemyActivationBitfield; its RTS returns to BattleLoop. The
[accepted RAM/deals derivation](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
and direct s02 aliases/GetCombatantEntryAddress `bhi` check admit160:
`$FFE800+(160−96)*56=$FFF600`, CURRENT_GOLD. Pseudo-X `$FFF62E` packs
deals84/85; pseudo-HP `$FFF60E` packs20–23; trigger `$FFF636` packs100/101;
activation word `$FFF634` packs96–99. Nonnegative pseudo-X, nonzero
pseudo-HP and a held non15 trigger flag90–104 can OR1/OR3 into Goddess
Staff99's count nibble. Fresh clear flags mean no activation write here,
even if its guard passes. This is not an unconditional item addition or
persistence proof. Slot160 is never offered by normal turn generation.

## Region hook, debug and spawning

**Confirmed:** the complete eight-byte-record table at
`data/battles/cutscenes/regionactivatedcutscenes.asm` is:

| Battle | Region | Played flag | Program |
| --- | --- | --- | --- |
|32|0|386|rbcs_battle32|
|32|1|386|rbcs_battle32|
|40|0|387|rbcs_battle40_1|
|40|1|388|rbcs_battle40_2|

WordFFFF terminates it. `cutscenes/regionactivatedcutscenes.asm`
ExecuteBattleRegionCutscene (`$47E82..$47EC8`) skips every row while the
live selector stays34, before any played/region-flag read or script trap.
No34 region program or nested action needs lowering. Changed32/40 can
match: played check precedes region check, SetFlag precedes MAPSCRIPT,
and it returns after one selected program. A held played flag does not
prove completed services; changing a selector does not turn that into34.

PrintAllActivatedDefCons then tests all sixteen flags90–105, requests
text463 once per held flag with DIALOGUE_NUMBER.l=index0–15, and closes
text. Complete ordered tokens are `["{#}","{N}","{D3}"]`; the authored
meaning is a diagnostic region number. Fresh34 requests no such text even
when debug is enabled. Source scope exceeds the six declared regions.

`battleloop/populateenemyspawns.asm` (`$1ACF30..$1AD090`) scans128–159
and masks each word with0F00. Initial1000/2000/3000, any added active bits
and absent0 all mask to0. All32 skip HP/flag/reset/item work; length.w=0,
stale list bytes remain unconsumed and SpawnEnemyWithCamera is bypassed.
This remains true if flags change while modes stay initialized.

Supplied nonzero modes have these branches:

- 0100: currentHP.w=0 permits reset without a region check.
- 0200: dead/trigger helper must pass, then maxHP.w=0 permits reset.
- 0300: dead/trigger helper must pass without that extra maxHP check.
- Other masked modes append nothing.

The dead helper has no X/Y test; primary wins, secondary sets both active
bits. These writes can precede a later maxHP/occupancy rejection. Both
triggers15 cannot pass a region requirement merely by changing flags.

ResetSpawningEnemyStats selects actual row count17 OR80 as exclusive
bound145:128–144 are real rows. It compares original starting coordinates
with all30 ally and32 enemy slots, without HP, neutral or self exclusion.
All17 selected starts are distinct, but a dead actor still at its own
start can block itself. Invalid/occupied writes max/currentHP0,X=−1 and
sets carry, preventing append. Success calls InitializeEnemyStats,
including actual definition/equipment/current-stat refresh, restores the
saved **full activation word** and clears carry. NOTHING preserves
definition equipment. Live selector changes can change rows/counts between
reads; later mode comparisons use returned full d1 rather than remasking.

Successful IDs append once each in ascending order (word length/byte IDs).
SpawnEnemyWithCamera clears d1 then enters SpawnEnemy. In
`code/common/scripting/entity/entityfunctions_1.asm`, SpawnEnemyEntity
reads actual X/Y/mover/mapsprite; masks coordinates3F, scales384, selects
the aerial layer for FLYING/HOVERING and facing3/eas_Idle. GetEntityEvent
scans64 mappings with a signed byte maximum, appends above it and calls
DeclareNewEntity without a separate capacity guard. Existing holes are
not a lowest-free-slot policy. Sprite DMA and its wait must return.

SpawnEnemy sets radius0, view/cursor target, waits for cursor and view,
hides cursor, then requests twelve orientation/property/Sleep3 iterations,
SFX_SPAWN, text397 and close. Its complete ordered text tokens are
`["{CLEAR}","{NAME}","{D3}"]`, announcing the named spawn. Outer d7/a0
are preserved. These are requests/return dependencies, not completed
camera/sprite/audio/control observations. No fresh34 slot reaches them;
an interruption does not roll back reached stats, flags or allocation.

## Current stats, Cameela's high bit and Iron Ball

**Confirmed:** `battleloop/initializecombatants.asm:InitializeEnemyStats`
copies each selected definition, fills HP/MP, applies placement/items,
rebuilds activation bits, adjusts base ATT for difficulty, then calls
`stats/updatecombatantstats.asm:UpdateCombatantStats` (`$89CE..$8BD0`).
Difficulty's consecutive BEQs apply floor(5×ATT/4) once at every difficulty
and twice, with truncation at each step, for SUPER. No AGI difficulty
mutation occurs. Refresh copies base→current, processes statuses, then
each of four equipped item slots in order and stores resulting status.

All eight selected definitions start NONE. Zero status still calls
IncreaseCurrentAgi with0 and DecreaseCurrentAgi with0. In
`stats/combatantstats_2.asm`/`combatantstats_3.asm`, increase masks numerical
seven bits, clamps at127 and restores bit7; decrease operates on the full
byte with unsigned borrow/min/max and maximum200. Thus Cameela186 remains
186 through the zero operations, not127,58 or200. Other AGIs are unchanged.
All six equipped weapon definitions have only INCREASE_ATT (Iron Ball44,
Heat Axe32, Flail19, Wish Staff26, Chrome Lance31, Assault Shell25); the
other two effects are NONE. No selected item changes AGI or prowess.

Iron Ball102 is CURSED, but ApplyItemOnStats tests the item curse bit only
for allies. Its enemy branch `clr.w d2` saves Z=1 in SR before effects;
RTR restores that Z, so UpdateCombatantStats's BEQ skips ORing CURSE.
Cameela's fresh status therefore stays0. The weapon is still equipped
and supplies ATT+44. Item type, stored curse status and an extra round
action are distinct; prowess DOUBLE_1IN4 is not another queue entry.

Current resources are maxima; ATT columns show non-SUPER / SUPER after
equipment. DEF/MOV match selected bases; status0 for every row:

| Definition / combatant IDs | HP/MP | Current ATT | DEF | AGI byte / numerical | MOV | Records each |
| --- | --- | --- | --- | --- | --- | --- |
|CAMEELA /128|99/0|96/109|52|186/58|6|2|
|NECROMANCER /129|47/42|75/89|44|51/51|6|1|
|SHAMAN /130|56/46|82/96|42|40/40|5|1|
|CERBERUS /131,138|51/0|87/108|44|45/45|6|1|
|PEGASUS_KNIGHT /132,139,142|65/0|81/93|37|47/47|7|1|
|BOW_MASTER /133,140|46/0|87/102|53|40/40|5|1|
|MUD_MAN /134,136,137|55/0|95/118|51|41/41|4|1|
|DRAGONEWT /135,141,143,144|60/0|87/100|42|46/46|6|1|

Changed status cannot be modeled by independently subtracting from58 and
retaining an immutable extra-action flag. For example, supplied SLOW=$0C00
with no other status makes the multiplier3: floor(186×3/8)=69; full-byte
decrease186−69=117, now a single-action numerical117. This is a bounded
refresh derivation, not a claim SLOW naturally reaches Cameela or survives
fresh initialization. Future content must bind actual current stats;
raw definition186 alone is insufficient for arbitrary changed states.

## Records, RNG, sorting and first dispatch

**Confirmed:** `battleloop/turnorderfunctions.asm` (`$25544..$25610`)
initializes64 two-byte records at `$FFF71A` toFFFF, then offers allies0–29
and enemies128–157 in ascending order. It excludes158/159 and160. Each
candidate rejects negative signed X.b and zero HP.w, with no Y,
membership, activation, neutral or positive-signed-HP predicate. All17
fresh enemies pass despite unchanged inactive words; absent145–157 fail X.

Let A=currentAGI &127 and R(n) be the next direct GenerateRandomNumber
draw with range n. First record score is
`A+R(A>>3)−R(A>>3)+R(3)−1`. Current raw byte>=128 appends an **immediate**
second record, with B=floor(A×5/6), score `B+R(B>>3)−R(B>>3)`; its two
draws precede the next candidate. Word arithmetic stores only low score
bytes. For these ranges, each draw updates
`seed=(seed×13+7)&65535`, then yields floor(seed×n/65536), even n=0.
This direct generator bypasses the debug override; earlier reached services
own the unknown incoming seed. Singles use3 draws, doubles5, rejects0.

| Definition | First range / conservative score | Second basis / range / score |
| --- | --- | --- |
|CAMEELA|7 /51..65|48 /6 /43..53|
|NECROMANCER|6 /45..57|none|
|SHAMAN|5 /35..45|none|
|CERBERUS|5 /40..50|none|
|PEGASUS_KNIGHT|5 /42..52|none|
|BOW_MASTER|5 /35..45|none|
|MUD_MAN|5 /36..46|none|
|DRAGONEWT|5 /41..51|none|

Bounds are algebraic envelopes, not independently attainable RNG extrema.
After the actual ally draws, enemy append order is128 first/second, then
129 through144 once each: **18 records/53 draws**. With N eligible fresh
allies and T of them having current rawAGI>=128, totals are `18+N+T`
records and `53+3N+2T` draws. Intact twelve-slot placement implies at most
**42 records/113 draws**. Dead/fewer members reduce N. F76, LEADER,
priority, equipped curse and prowess do not create further queue entries.

Sorting makes62 passes of63 adjacent signed-byte comparisons across all64
records, swapping only when the right score is strictly greater. Ties
preserve append order (allies before enemies, Cameela's first before her
second on equality). Padding255 is−1; a wrapped135 is−121. Existing rawAGI128
score0/255 and rawAGI127 score135 fixtures prove their seams, not34's seed.
Original appending has no capacity guard, and62 passes do not establish
complete sorting for arbitrary64 entries. Supplied changed actors can
exceed64 records; the fresh42 bound does not apply to all30 ally/30 enemy
offers or to arbitrary changed current HP/X/AGI. Buffer overrun can affect
adjacent state.

Fresh enemy scores are positive and the42 maximum fits, so a non-sentinel
reaches the front within those passes, conditional on earlier services
returning. Generation clears CURRENT_BATTLE_TURN.b at `$FFF79A`, immediately
after the128-byte buffer, and fresh dispatch reads offset0. The actual
first actor depends on party/current stats and sequential RNG; Cameela's
58/high bit/LEADER does not prove she is first. An empty eligible set
retains FF and repeats @Start; changed negative scores can sort behind
padding. No ExecuteIndividualTurn body or resulting action is claimed.

## Actual consumers and missing Battle34 binding

**Confirmed about accepted code:** production consumes prepared Content;
source shape, admission and actual execution remain separate. The
[capability owner](../../remake/docs/capability-status.md) governs runnable
scope. The predecessor owns opening/native/profile gaps. These selected
consumer findings are static, with no SDK/adapter/Godot run:

| Actual owner | Expressible or implemented | Selected gap or distinct boundary |
| --- | --- | --- |
|[Encounter preparation](../../src/sf2tool/remake_exploration_battles.py), [BattleEncounterDefinition](../../remake/src/Sf2.Remake.Application/Content/Scenarios/BattleEncounterDefinition.cs), [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs)|Placements, orders, regions, point, spawn expressions and provenance have shapes. All selected coordinates fit32×32; STARTING positions are distinct. Preparation stores the source type as vertexCount.|This preparation entry admits2/3 only. Read checks fixed export identities and Pin admits1/2/3, rejects34. Parse requires vertexCount4 and four distinct bounded vertices/two nondegenerate triangles: region3's type3 and repeated point fail those independent guards. EncounterRegion itself stores no triangle-type selector. Do not rewrite3→4 or invent a fourth vertex.|
|[PrivateBattleScenarioReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|STARTING/NOTHING, supported order expressions, numerical AGI and ExtraRoundAction have representations. A source186 can split58/true.|region-program-table admits1/2/3 only despite original34 having no hook. HEALER5 and SENTRY reject source-commandset; MAGE/ARCHER reject source-mover. All eight ENHANCED definitions fail enemy-derived-state's REGULAR requirement; selected BIT4 also fails source-ai-word. These inspected guards are not reached34 runtime errors.|
|[BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [BattleTurnFlow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs)|Activation before generation; primary/secondary OR precedence; None region program and mode0 policies.|Cross-product quadrilaterals and per-round tested=0 differ from original integer/type3/retained checked word. Domain also rejects the degenerate four-point region3. Validated actor sets omit raw160/deals alias and original signed-HP distinctions. Non-None program/nonzero mode refuse region-program/spawn-mode; no spawn reset/service consumer.|
|[BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs), [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs)|Controlled roster initialization/difficulty0, numerical AGI<=127 with separate extra action, sequential draws and signed62×63 sorting/64 slots.|Initialization sets checked state0. Enemy equipment requires words0/127 and refuses all six equipped weapons as enemy-equipment-refresh. General status/equipment/current-stat refresh and changed high-bit coupling are missing; numerical58/true is only the unchanged selected shape. Overflow refusal and validated actors/cursor differ from unsafe source append/byte offset. Missing34 definitions/party/class/loadout bindings remain.|
|[BattleAdvancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs), [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Round RNG/candidate/draw/queue observations, fresh-entry continuations and dispatch consumers exist.|region-program-none/spawn-modes-admitted report admitted policies, not executed native hooks/spawn services. F88 NewBattle refuses suspended-battle-entry. Application checks outcome before generation and continues into control/AI beyond this research stop; transactional refusal does not reproduce arbitrary original partial prefixes. No composed34 dispatch is established.|
|[ExplorationViewRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs), [BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs), [ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs)|Existing view/board/sprite/effect/text services; predecessor checks Map55 layer255 profile.|No selected34 region/round/spawn-service binding is supplied. Crop-origin area selection differs from original view-target selection. Terrain/materials, physical allocation, camera/cursor/DMA/text/audio completion, exact presentation/timing and usable control remain unassessed.|

Research completion does not admit34 as runnable. The minimum future
integration decision needs selected content/material/definition bindings,
an explicit type3/integer/checked-state policy and admitted current-stat/
equipment semantics. Those are identified boundaries, not authorized
engine work or a new acquisition queue.

## Verification, retained failures and Unknowns

From the owning worktree after loading current ignored selections, use
the bounded ignored reproduction recipe and clean committed planner:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1102/review.py <configured-source-disasm-root> --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The audit checks pinned clean source/unchanged pair; full selected
12/17/6/1 declarations/eight definitions/six items; original geometry/type3,
72 formation predicates and changed points; checked/flag precedence,
all activation rows/absent slots/160 aliases; complete hook/debug/text-token
and spawn/reset/occupancy/capacity/services; current stats/curse/high-bit
refresh; RNG/append/sort/buffer/sentinel/F88; and actual consumer guards.
Apply ADR0004's adversarial checklist to this documentation scope; no
parser/schema/fixture/test is added. Check exact two-path ownership,
navigation-only predecessor, cached diff, UTF8/LF, links and private boundary.
Raw game-text joins stay ignored; public content uses IDs, authored
paraphrases and complete ordered token arrays. Record actual exact-head
scoped CI. Normal/full Python/.NET/Godot/H3/H4 suites, old comparisons and
tests of audits are excluded; a planner suggestion is not an executed pass.

Retain #1100's pre-correction6df1f6c7/direct0.377s/CI39511030878 as historical
evidence, including its missed public-dialogue boundary. Corrected51ac0d0f
focused author0.239s/main-gate0.242s/CI39511617705 replaced public raw lines
with paraphrases/tokens; it did not correct static game semantics. Preserve
#1088's49→48 AI-clear correction and untouched checked word, #1096's
withdrawn Tristan attribution, #1098 diagnostics and all twelve completed
old comparison failures. No completed receipt is overwritten, called
interrupted or made an automatic broad-rerun queue.

Freeze a clean committed/pushed Draft PR with exact base/head/tree,
commands/CI/failures/Unknowns and ignored handoff, Project Review / Verify
and successful main-gate notification, then stop the writer/process.
Main-gate independently accepts/merges; no next Issue, cleanup or archive.

**Unknown:** natural Nazca admission, actual party/membership/flags/checked
word/current stats/positions/cached tails/selectors/deals/RNG/first actor;
changed occupancy/mappings/capacity and interrupted/interleaved services;
completed load/debug/scene/camera/cursor/sprite/DMA/text/audio waits,
presentation/timing, usable control and persistence. Individual-turn
movement/AI/combat, outcomes and later Map55/ship remain outside this
endpoint. None of these Unknowns is a passed gate or automatic runtime queue.
