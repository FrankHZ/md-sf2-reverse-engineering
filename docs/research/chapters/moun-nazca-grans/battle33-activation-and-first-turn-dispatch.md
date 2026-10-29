# Battle33 activation and first-turn dispatch

Continue with [Battle33 outcomes and the Zynk handoff](./battle33-outcomes-and-zynk-handoff.md).

Evidence date: 2026-10-20. **Confirmed (static):** all twelve declared
formation positions miss Battle33's sole region under the original integer
consumer. With unchanged fresh construction, flags90–105 remain clear,
activation words keep their initialized values and the spawn list is empty.
All sixteen initialized enemies nevertheless enter turn generation, each
with one record. Stop at the first ExecuteIndividualTurn call, before its
body. **Unknown:** natural reach, actual incoming state/RNG, first actor,
completed services, presentation, control and persistence. No playable
Battle33 or engine-readiness claim follows.

## Agreement, sources and evidence boundary

Standard rigor for [Issue #1090](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1090)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect region activation, spawn admission and first dispatch for later
content/engine decisions. Accepted dependency is [Battle33 initialization
and opening](./battle33-initialization-and-opening.md), #1088/PR1089,
base `e732c369b2d8441b56c9adc13ab17b02e51138a0`. Owned writes are this
document and one navigation-only link there. No shared-index write or
stacked dependency; UI692 is independent. No implementation, new
schema/fixture/tool, extraction/decoding, acquisition, runtime, seed hunt,
individual-turn AI/action/combat/outcome, Moun exploration, design-guide,
UX or translation work.

Reuse [Battle32 activation](./battle32-activation-jaro-and-first-turn-dispatch.md),
[Battle31 activation](../northern-travel-mitula/battle31-activation-and-first-turn-dispatch.md),
[Battle30 integer geometry](../northern-travel-mitula/battle30-activation-and-first-turn-dispatch.md#geometry-checked-bits-and-persistent-flags),
[Battle26 shared derivations](../northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md),
[battle loop](../../battle/battle-loop.md), [spritesets](../../battle/battle-spriteset-data.md),
[RNG/math](../../battle/runtime-rng-and-battle-math.md), [lifecycle](../../../design/contracts/battle/battle-control-lifecycle.md)
and [cutscene routing](../../../design/contracts/battle/battle-cutscene-routing.md).
Their fixtures prove named shared seams, not a Battle33 observation.

Original paths below are relative to `disasm/` in clean read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The accepted USA retail ROM identity is SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM/H1/runtime equality is claimed. `sf2const.asm`/`sf2enums.asm`
own constants/RAM. Fnnn means flag nnn; ordinals are zero-based, intervals
half-open. `battleloop/` and `cutscenes/` abbreviate the corresponding
children of `code/gameflow/battle/`.

Minimum evidence is complete selected source/table/fixture/consumer joins,
twelve formation-region calculations and bounded changed states. Reuse the
unchanged18,285,979-byte init/script pair and accepted0.083s provenance in
`local/issue1005/pair.txt`. Bounded linear reads and arithmetic budget less
than one minute/512MiB per direct run, with small ignored receipts under
`local/issue1090/`. No new corpus or comparison platform. Reassess
contradictions or material supporting machinery before expansion.

## Fresh prefix and supplied-state entry

**Confirmed:** `code/gameflow/battle/battleloop_1.asm:BattleLoop`
(`$23A84..$23BB4`) reaches @Start after fresh LoadBattle and the empty start
wrapper return. The predecessor owns F483, the Frayja scene and replacement,
healing, placement and Map31 reconstruction. At @Start the order is:

1. ActivateEnemies scans128–159, then falls through once for160.
2. j_ExecuteBattleRegionCutscene scans its live selector table.
3. Nonzero DEBUG_MODE_TOGGLE.b calls PrintAllActivatedDefCons.
4. j_PopulateTargetsListWithSpawningEnemies rewrites TARGETS_LIST_LENGTH.w.
   Zero skips spawning; otherwise subtract1, traverse byte IDs with d0.w
   cleared on each read and DBF around SpawnEnemyWithCamera.
5. GenerateBattleTurnOrder constructs/sorts the buffer and clears the offset.
6. @IndividualTurns_Loop clears d0.w, reads CURRENT_BATTLE_TURN.b as a
   **byte offset** into BATTLE_TURN_ORDER, then replaces d0.b with that byte.
   FF returns to @Start; another byte calls ExecuteIndividualTurn. Stop here.

There is no transaction or rollback across these phases. Every earlier
service must return before later work. F483 is set before start dispatch,
not a receipt for completed opening, Frayja membership or usable input.

The accepted Frayja sequence can replace the first tested dead active
member, else the greatest-ID active member; it is not a fixed character
replacement. Selection occurs before fresh healing. Healing visits all30
allies: Peter7/Lemon28 even at HP0, other living allies to maxima, other
dead allies remain dead; STUN/POISON/CURSE survive before stat refresh.
Ally placement resets all30 X=-1, rebuilds sorted lists and considers up to
twelve active entries; dead entries consume an iteration but no formation
row. Excess entries lose active flags/X. Frayja22 uses his resulting sorted
slot if active, living and reached. Scene coordinates are not formation
coordinates; final cached scene lists do not replace placement's rebuild.
The predecessor retains the unguarded stale-byte/DBF behavior for an empty
list; a safe empty-party initialization must not be assumed.

F88 instead restores seconds, clears88/AI memory/view target, calls
LoadBattle and branches directly to @IndividualTurns_Loop; the following
bra @Start is unreachable. It does not clear region flags, heal, place,
activate, spawn or generate before that first read, and keeps the supplied
offset/buffer. Odd offsets read score bytes; offsets128–255 exceed the
128-byte buffer. Supplied FF can re-enter @Start using actual live state.
Natural suspend state, arbitrary-state safety and saved persistence remain
Unknown.

## Integer region test, checked word and flags

**Confirmed:** `battleloop/activateenemies.asm` (`$2550C..$25544`) uses
32 iterations. UpdateEnemyActivationBitfield rejects negative signed X.b,
then zero current HP.w, then nonnegative d0.b. There is no Y, membership,
neutral, commandset, order or active-bit test. All sixteen initialized
enemies pass; absent144–159 fail X. High-bit HP is nonzero for this guard.

`battleloop/triggerregions.asm:TriggerRegionsAndActivateEnemies`
(`$1AC9FC..$1ACF30` file) saves d0–a6 and selects the **live** battle's region
subsection. Count0 returns before geometry or activation. Selected33 has
count1. For each unchecked NEWLY_TRIGGERED_BATTLE_REGIONS.w (`$FFB20C`)
bit, IsRegionTriggered is called; a hit sets F90+index. Both hit and miss
set the checked bit. A held checked bit suppresses geometry, not later
activation from an independently held flag. The prefix clears neither
checked word nor persistent flags; fresh entry separately cleared90–105.

The accepted ClearAiMemory correction matters here: its word counter starts
48 and uses subq.w/BNE after each pair of byte writes. It writes offsets0–47,
48 last-target bytes at `$FFB1AC..$FFB1DC` toFF and48 AI-memory bytes at
`$FFB1DC..$FFB20C` to0. The checked word at `$FFB20C..$FFB20E` is untouched.
Neither fresh nor F88 clearing proves that word zero. A later AI-body clear
is outside this endpoint and cannot supply an incoming value.

`data/battles/spritesets/spriteset33.asm` declares one12-byte region:
type4, unknown0, ordered vertices `(0,0),(0,38),(18,38),(18,0)`, two trailing
zero bytes. Point0 `(10,11)` is a separate AI-order destination, not another
region. The original geometry does not clip vertices to battle crop32×38.

IsRegionTriggered tries triangle1/2/4, then3/2/4 only if the first finds no
eligible ally. AreAnyAlliesInTriangle scans slots0–29, requiring **positive
signed HP.w** and nonnegative signed X.b/Y.b, without membership tests.
IsAllyInTriangle returns0 inside; the wrapper returns d1.w=-1 for a match,
0 for none. Living positioned nonmembers can trigger, whereas dead,
negative-HP or negative-coordinate allies cannot. This HP/Y predicate
differs from enemy activation and turn admission.

The reused integer derivation is checked against the actual branches:
sign-extended coordinate bytes; dominant absolute delta, horizontal on
equality; signed multiplication/division truncating toward zero; projection
added to endpoint B. Triangle edges are1->2,3->1,3->2. Result1 immediately
accepts a bounded collinear point; otherwise side/opposite-vertex comparisons
and bit0 exception branches apply. A negative projection synthesizes2
through `move #1,ccr`, despite the comment's3. No floating-point or
cross-product substitute is used. Index validation is `cmp.b d1,d7`/`ble`,
not strict index<count; FF separately returns false. Normal iteration here
supplies only0; trigger15 is a sentinel.

| Formation ordinal | X/Y | Region0 |
| --- | --- | --- |
|0|`(28,28)`|outside|
|1|`(28,29)`|outside|
|2|`(27,29)`|outside|
|3|`(28,30)`|outside|
|4|`(27,30)`|outside|
|5|`(26,30)`|outside|
|6|`(27,28)`|outside|
|7|`(26,29)`|outside|
|8|`(25,30)`|outside|
|9|`(29,30)`|outside|
|10|`(25,29)`|outside|
|11|`(26,28)`|outside|

For every point, triangle1/2/4 rejects at its third edge4->2: vertical
`Xproj=trunc(-18*(Y-38)/38)` is3 or4, below every selected X, and the
opposite vertex has the other side. Triangle3/2/4 rejects at its second
edge4->3: Xproj18 is below every selected X, opposite vertex2 is to its
left. Earlier edges do not immediately accept. At `(28,28)` the first
projection is4; all arithmetic fits and triangles are nondegenerate.

Changed-point checks: `(18,28)` hits, `(19,28)` misses; `(10,38)` hits,
`(10,39)` misses; `(0,0)` and `(18,38)` hit, `(19,38)` misses. The diagonal
neighbors `(4,28)`/`(5,28)` both hit via the union, despite different first
triangle decisions. These are coordinate predicates, not walkability or
natural-reach evidence; row38 can trigger even though the crop ends before38.

Thus an unchanged completed fresh placement uses only misses or a living
subset of them, with other ally X=-1. F90–105 remain clear for any incoming
checked word. The first passing enemy produces `incoming OR $0001`, not
necessarily exactly1. Empty/dead eligible geometry produces no hit. A
checked miss can suppress a later moved ally; changed live positioned allies
can hit even without joined/active membership. Independently held flags
can activate enemies when geometry is skipped or now misses.

## Sixteen activation rows and extra invocation160

**Confirmed:** GetTriggerRegions reads a byte and splits primary high /
secondary low nibbles. Both15 returns without an activation write. Held
non15 primary wins and ORs bit0; only absent/clear primary allows held
non15 secondary to OR bits0/1. Misses preserve the word. Primary writes
preserve preexisting bit1; unrelated bits survive both branches. There is
no deactivation or chosen-action result.

All rows are NOTHING/filler0/STARTING0; words are hexadecimal. Orders and
definitions are rejoined to the predecessor, not inferred from actor names.

| Row / ID | Definition | X,Y | Commandset | Primary order / trigger | Secondary / trigger | Initial word | Held relevant flag |
| --- | --- | --- | --- | --- | --- | --- | --- |
|0 /128|SHAMAN|10,11|LEADER|MOVE_TO\|0 /0|NONE /15|`3000`|F90 -> `3001`|
|1 /129|DEVIL_SOLDIER|9,10|ATTACKER2|FOLLOW_TARGET\|0 /0|NONE /15|`2000`|F90 -> `2001`|
|2 /130|DEVIL_SOLDIER|11,10|ATTACKER2|FOLLOW_TARGET\|0 /0|NONE /15|`2000`|F90 -> `2001`|
|3 /131|MUD_MAN|10,17|ATTACKER3|FOLLOW_ENEMY\|0 /0|NONE /15|`2000`|F90 -> `2001`|
|4 /132|MUD_MAN|7,19|ATTACKER3|FOLLOW_ENEMY\|0 /0|NONE /15|`2000`|F90 -> `2001`|
|5 /133|CERBERUS|5,18|ATTACKER3|FOLLOW_ENEMY\|3 /0|NONE /15|`2000`|F90 -> `2001`|
|6 /134|DEVIL_SOLDIER|18,26|ATTACKER1|NONE /15|NONE /15|`2000`|no activation write|
|7 /135|NECROMANCER_0|16,31|ATTACKER3|FOLLOW_ENEMY\|8 /15|NONE /15|`2000`|no activation write|
|8 /136|BOW_MASTER|16,32|ATTACKER1|NONE /15|NONE /15|`2000`|no activation write|
|9 /137|BOW_MASTER|14,14|SENTRY|NONE /15|NONE /15|`2000`|no activation write|
|10 /138|CERBERUS|13,15|SENTRY|NONE /15|NONE /15|`2000`|no activation write|
|11 /139|PEGASUS_KNIGHT|14,15|SENTRY|NONE /15|NONE /15|`2000`|no activation write|
|12 /140|PEGASUS_KNIGHT|19,14|ATTACKER1|NONE /15|NONE /15|`2000`|no activation write|
|13 /141|NECROMANCER|21,16|ATTACKER3|FOLLOW_ENEMY\|14 /15|NONE /15|`2000`|no activation write|
|14 /142|BOW_MASTER|22,16|ATTACKER3|FOLLOW_ENEMY\|12 /15|NONE /15|`2000`|no activation write|
|15 /143|CERBERUS|12,22|ATTACKER1|FOLLOW_TARGET\|0 /0|NONE /15|`2000`|F90 -> `2001`|

MOVE_TO0 references point0 `(10,11)`; FOLLOW_ENEMY operands are local
ordinals (0=Shaman128,3=Mud131,8=Bow136,14=Bow142,12=Pegasus140).
FOLLOW_TARGET0 is not that AI point. Rows7/13/14 have real stored follow
orders but sentinel triggers; no selected row has a secondary-only path.
Changed trigger bytes can take that path. LEADER, priority and SENTRY names
do not choose first actor, tactics or control.

All sixteen declared rows initialize in33; Battle32's Jaro143 skip requires
selector32. Absent144–159 have activation0,max/currentHP0,X=-1 and trigger0F
from the **unoffset first row**, with Y/type/items/orders otherwise retained.
Changing only X/HP can expose primary0/F90; these are not hidden declarations
or safely initialized extra enemies. Fresh flags leave all words unchanged.

After32 iterations d0.w=160/d7.w=FFFF falls through into the helper; no RTS,
branch or emitted data separates it. Its RTS returns to BattleLoop.
The checked [RAM/deals derivation](../northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
resolves s02 aliases and GetCombatantEntryAddress's `bhi` admission of160:
`(160-96)*56+$FFE800=$FFF600`, CURRENT_GOLD. Pseudo-X byte `$FFF62E` packs
counts84/85; pseudo-HP word `$FFF60E` packs20–23; trigger byte `$FFF636`
packs100/101; activation word `$FFF634` packs96–99. Nonnegative pseudo-X,
nonzero pseudo-HP and a held non15 trigger flag90–104 can OR1/OR3 into
Goddess Staff99's count nibble, preserving other bits. Clear fresh flags
imply no write here; earlier actual flag writes would already be visible.
This is no unconditional shop addition, visible inventory or persistence
proof.

## Region hook, debug display and spawn boundary

**Confirmed:** the complete `data/battles/cutscenes/regionactivatedcutscenes.asm`
table contains four8-byte records followed by word FFFF:

| Battle | Region | Played flag | Script |
| --- | --- | --- | --- |
|32|0|386|rbcs_battle32|
|32|1|386|rbcs_battle32|
|40|0|387|rbcs_battle40_1|
|40|1|388|rbcs_battle40_2|

`cutscenes/regionactivatedcutscenes.asm:ExecuteBattleRegionCutscene`
(`$47E82..$47EC8`) advances8, tests terminator, then compares live
CURRENT_BATTLE.b. With33 throughout it skips every row without checking
played/region flags, setting played flags or dispatching MAPSCRIPT. F90
does not import Battle32's Jaro scene. Changed32/40 can match: played flag
check precedes region check; played flag write precedes the trap; the wrapper
returns after one script, without rescanning. Interrupted services need not
complete despite that flag. No new selected33 region-scene inventory exists.

Debug display follows the wrapper. PrintAllActivatedDefCons starts at90
and calls PrintActivatedDefCon sixteen times, then closes text. Each held
flag writes its index0–15 to DIALOGUE_NUMBER.l and requests text463; clear
flags request none. This scans all90–105, not only the one declared region.
Unchanged fresh state therefore requests no region text even when debug
is enabled. Display/control/timing were not observed.

`battleloop/populateenemyspawns.asm` (`$1ACF30..$1AD090`) scans128–159,
masking each activation word with0F00. Mode0 skips HP/region/reset/item
work. Initial3000/2000, any new active bits0/1 and absent0 all mask to0.
Thus all32 skip, TARGETS_LIST_LENGTH.w becomes0 and SpawnEnemyWithCamera
is bypassed. Stale list bytes remain but are not consumed. This result
does not depend on F90 staying clear if initialization/modes stay unchanged.

For supplied changed modes, the actual branches are:

- 0100: HP.w=0 admits reset without a region test.
- 0200: HP.w=0 and a held non15 trigger must pass the dead helper; maxHP.w
  must then be0 before reset.
- 0300: dead/trigger helper must pass, with no extra maxHP test.
- Other nonzero masked modes append nothing.

The dead helper has no X/Y predicate. Primary wins; secondary sets both
active bits. These writes can precede a maxHP/reset rejection. A row with
FF triggers cannot pass a region requirement by changing flags alone.

ResetSpawningEnemyStats in `battleloop/initializecombatants.asm` uses the
live enemy count16 OR80 as exclusive bound144. Only128–143 are real selected
rows. It compares that row's original starting coordinates against all30
allies/32 enemies without HP/neutral/self exclusion. A dead actor still at
its own start can block itself; all sixteen declared starts are distinct,
but actual changed occupants matter. Invalid row or occupancy writes
max/currentHP0,X=-1 and carry, preventing append. Success calls
InitializeEnemyStats directly, including selected definition/current stats/
equipment, then restores the saved **full activation word** and clear carry.
Selected NOTHING preserves definition equipment; no extra item award follows.
Live selector changes can change row/count/upgrade selection between reads.

Each successful slot appends once, in ascending ID order (word length,
byte IDs). Later mode comparisons use returned full d1, not a remasked
mode. For an actual nonempty list, SpawnEnemyWithCamera clears d1 and
calls SpawnEnemy. SpawnEnemyEntity (`code/common/scripting/entity/entityfunctions_1.asm`)
reads actual coordinates/mover/mapsprite, masks coordinates3F and scales384,
uses aerial layer for FLYING/HOVERING, facing3/eas_Idle, and allocates above
the highest signed mapping in the64-byte lookup scan, without a separate
capacity guard. Sprite/DMA and its wait must return. No fixed physical slot
is deduced for changed occupancy/mappings.

SpawnEnemy then sets radius0/view target/cursor, waits for cursor and view,
hides cursor, requests twelve orientation/property/Sleep3 iterations,
SFX_SPAWN and text397/close; outer d7/a0 are preserved. These are service
requests and return dependencies, not completed animation/audio/control.
An interrupted service does not roll back reached reset/list/stat writes.
No unchanged fresh33 list reaches this path.

## Current AGI, records, RNG and first call

**Confirmed:** `battleloop/turnorderfunctions.asm` (`$25544..$25610`)
clears64 two-byte records at `$FFF71A` toFFFF, offers allies0–29 then
enemies128–157 in ascending order. It excludes158/159. Admission rejects
negative signed X.b or zero HP.w only: no Y, membership, neutral, activation,
region, commandset or LEADER test. All sixteen initialized enemies qualify,
including those with clear active bits; absent144–157 fail X.

All eight definitions are checked in `data/stats/enemies/enemydefs.asm`
against the actual refresh in `code/common/stats/updatecombatantstats.asm`.
InitializeCurrentStats copies base AGI into current AGI; status NONE passes
zero increase/decrease and skips STUN's extra decrease. IncreaseAndClamp7Bits
preserves80, masks7F and clamps the low part; DecreaseAndClampByte then caps
at200. Every selected byte below128 stays unchanged. Difficulty's ATT
transformation does not change AGI; no33 random upgrade is selected. Itemdefs
effects for Wish Staff94(+26), Great Axe39(+28), Mage Staff93(+27), Assault
Shell49(+25), Chrome Lance79(+31), Flail90(+19) are ATT-only. Placement
NOTHING leaves those copied equipped items present. Changed current status,
equipment or refresh requires an actual current-byte calculation.

| Definition / initialized IDs | HP | Base = current raw AGI | A = raw&127 | A>>3 | Records each | Conservative score bounds |
| --- | --- | --- | --- | --- | --- | --- |
|SHAMAN83 /128|56|40|40|5|1|35..45|
|DEVIL_SOLDIER23 /129,130,134|60|43|43|5|1|38..48|
|MUD_MAN25 /131,132|55|41|41|5|1|36..46|
|CERBERUS24 /133,138,143|51|45|45|5|1|40..50|
|NECROMANCER_0 101 /135|47|51|51|6|1|45..57|
|BOW_MASTER63 /136,137,142|46|40|40|5|1|35..45|
|PEGASUS_KNIGHT48 /139,140|65|47|47|5|1|42..52|
|NECROMANCER73 /141|47|51|51|6|1|45..57|

NECROMANCER_0 is definition101, distinct from73 despite equal stats,
spells and mapsprite137: Mage Staff versus Flail, unknownByte0 versus73,
priority-only versus definition BIT4. Initialization keeps upper priority
nibble and replaces lower AI bits from placement; both get2000 before
activation. Those differences are not a second-turn bit.

Reuse the [RNG/score/sort derivation](../northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#turn-buffer-randomized-agility-and-first-dispatch).
First score is `A+R(A>>3)-R(A>>3)+R(3)-1`. Current **raw** AGI>=128 adds
an immediate second record using `B=floor(A*5/6)` and
`B+R(B>>3)-R(B>>3)`. Word arithmetic stores only the low score byte.
Each direct draw updates `seed=(seed*13+7)&65535`, even range0; selected
ranges yield `R(n)=floor(seed*n/65536)`. Singles consume3 sequential draws,
doubles5, rejects0; direct generation bypasses the debug RNG override.
Earlier reached services determine the actual incoming seed. Coupled draws
do not guarantee independently attainable conservative extrema.

Unchanged enemies contribute **16 records/48 draws**, after actual ally
draws. With N eligible placed fresh allies and T having current rawAGI>=128,
total records are `16+N+T`, draws `48+3N+2T`; twelve formation slots imply
at most40 records/108 draws under those assumptions. Fewer/dead members
reduce N; Frayja uses his own current AGI. No extra record follows from
Shaman's LEADER/priority, spell power, two spells or Frayja's forced admission.
Changed positions/modes/HP/AGI do not inherit the twelve-member bound:
all30 ally slots and30 enemy slots are offered, irrespective of membership.

Sorting uses62 passes of63 adjacent signed-byte comparisons across64
slots (`cmp.b`/`ble`), swapping only when the right score is strictly greater.
Ties preserve append order, including allies before enemies. Padding255 is-1;
wrapped135 is-121. Existing rawAGI128 score0/255 and rawAGI127 score135
fixtures prove their seam, not incoming33 state. There is no original
append-capacity guard; changed doubled candidates can overrun64, and62
passes do not establish complete sorting for arbitrary64 entries. Fresh
maximum40 fits; positive selected enemy scores put a non-sentinel at the
front within these passes, conditional on earlier services returning.

Generation clears CURRENT_BATTLE_TURN.b (`$FFF79A`, immediately after the
buffer), so fresh dispatch reads offset0 and reaches the first call.
Actor identity depends on actual party/current stats and sequential RNG;
NECROMANCER's51 or Shaman's LEADER is no proof of first actor. Empty
eligible sets retain FF and repeat @Start; changed scores below-1 can sort
behind padding. No ExecuteIndividualTurn body or resulting turn is inspected.

## Accepted consumers and missing Battle33 binding

**Confirmed about accepted code:** production consumes prepared Content;
source representation, admission, execution and fidelity are separate. The
[capability owner](../../../../remake/docs/capability-status.md) governs runnable
scope. The predecessor owns opening lowering and Map31's profiles; this
slice checks the actual round consumers without running them.

| Actual consumer | Representable / implemented | Specific gap, refusal or different boundary |
| --- | --- | --- |
|[BattleEncounterDefinition](../../../../remake/src/Sf2.Remake.Application/Content/Scenarios/BattleEncounterDefinition.cs), [PrivateBattleEncounterReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs)|Coordinates, orders, items, regions, point and spawn expressions have shapes; STARTING placements here are distinct and within32×38.|Pin admits1/2/3, rejects33. Independently, Parse requires every vertex X<width/Y<height: the two Y38 vertices fail at height38 (`regions`). Domain [BattleActivationRules.Validate](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs) also requires definition.Contains for vertices. Do not silently clip them or enlarge crop to claim admission.|
|[PrivateBattleScenarioReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Selected STARTING/NOTHING, HEALER1/ATTACKER1/2/3/LEADER and order forms are expressible. Numerical AGI and ExtraRoundAction are separate.|region-program-table admits1/2/3 only despite original33 having no hook. SENTRY rows reject source-commandset; ARCHER/MAGE reject source-mover (HEALER/REGULAR/STEALTH/FLYING are admitted). All eight ENHANCED definitions reject enemy-derived-state's REGULAR requirement; BIT4 definitions also exceed source-ai-word. These inspected guards are separate from first Pin rejection, not reached33 runtime errors.|
|[BattleActivationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [BattleTurnFlow](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs)|Activation before generation; primary/secondary OR precedence; RegionProgram.None and SpawnMode0. Original33's absent hook and initialized modes fit those policies in isolation.|Geometry uses cross products and local tested=0 each round, without supplied checked-word suppression or raw-slot160/deals alias. Non-None program rejects region-program; nonzero spawn rejects spawn-mode; no reset/entity-spawn service. Fresh misses alone prove no changed-state fidelity.|
|[BattleInitializationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs), [TurnOrderRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs)|Numerical AGI<=127 plus extra action, sequential draws, signed62×63 sort/64 slots exist. Selected numerical AGIs need no extra action.|Enemy equipment refresh requires source items0/127 and rejects the selected equipped weapons as enemy-equipment-refresh. Party/classes/current status and all eight definitions need binding. Overflow refusal and validated actors/queue cursor differ from original unsafe slots/byte offset; matching formulas do not select the first33 actor.|
|[BattleAdvancer](../../../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs), [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Round observations and fresh-entry continuations exist.|region-program-none is no executed original scene. F88 NewBattle refuses suspended-battle-entry. Missing33 encounter/party/Frayja replacement/start/world bindings precede a composed dispatch claim. Generic Join is not csc51 replacement.|
|[ExplorationViewRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs), [BattleMapViewport](../../../../remake/game/src/Battles/BattleMapViewport.cs), [ExplorationPresentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs)|Existing view, battle-map, sprite/effect/window consumers; predecessor checks Map31 layer0, unit parallax and foreground32 profile.|No selected33 deployment/region/round or spawn-service binding is supplied. Native opening operations remain separate gaps. Selected terrain/materials, actual geometry transitions, physical allocation, text/audio/presentation/timing, usable control and persistence are unassessed. No screenshot, adapter, Godot or emulator run.|

Research completion does not make33 runnable. Missing33 bindings also do
not erase supported general turn, view, text or entity mechanisms.

## Reproduction, retained results and stopping condition

Load ignored selections in the launching shell. Reproduce the bounded
source/document/consumer audit on clean committed HEAD:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1090/review.py <configured-source-disasm-root> --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The direct review checks selected rows/eight definitions/equipment/current
AGI, twelve integer results and changed points; actual full phase guards,
widths, branches, order, aliases, checked/flag/activation distinctions,
extra160, complete hook table, debug, spawn/reset/occupancy/services and
turn/RNG/sentinel/F88; existing fixtures and real Content/Domain/Application/
Godot consumers. Separately check exact accepted-base-to-head two paths,
clean status, navigation-only predecessor, UTF8/LF/links/private boundary
and cached diff. Retain exact-head scoped CI and every direct receipt in
the Issue/PR handoff. Current documentation policy skips blanket normal/full
Python/.NET/Godot/H3/H4, old comparisons and tests of audit programs.

Preserve #1088 original276d1ef0 author0.250s/main-gate0.249s/CI39498534706:
those passed while missing the erroneous49-count sentence. The accepted
9d157737 correction is48 bytes/table and untouched checked word; focused
main-gate0.196s/CI39499322697 passed. Retain earlier inventory-NOTHING,
clean-head, nested-action, LEADER, selector-width, HP, geometry and N==V
findings/receipts and their corrections through the predecessor chain.
Twelve old completed comparison failures remain discovery evidence, not
a rerun queue. Preserve each new receipt before a focused correction or
rerun; do not simulate clean committed scope with dirt.

Stop with the exact owned diff, clean committed/pushed frozen Draft PR,
Project Review / Verify, exact head/tree/base/checks/CI/handoff and successful
main-gate notification; writer/process stopped. Main-gate independently
accepts and integrates. No merge, environment cleanup/archive or next Issue.

**Unknown:** natural admission/reach, actual incoming flags/checked word,
party/membership/classes/equipment/current HP/AGI/positions/cached tails,
selectors/deals/RNG/first actor, changed occupancy/mappings/capacity,
interrupted prefixes and service interleavings, completed load/debug/text/
camera/cursor/sprite/DMA/audio/waits, presentation/timing, usable control
and persistence. These are not an automatic runtime queue. Individual-turn
AI/actions/combat, battle outcomes and later Moun exploration remain deferred.
