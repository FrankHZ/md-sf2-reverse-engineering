# Battle23 activation, AQUA2 and first conditional control

Evidence date: 2026-10-23. **Confirmed:** the fresh Battle23 formation has
twelve party placements, twelve enemies, one region and one point. All twelve
placement coordinates miss both source triangles. Six enemies assign region0;
the other six use15/15. Willard's point0, live ally0 follows and live enemy
follows have different consumers. Bubbling Ooze can select zero-cost AQUA2,
but its packed level2 byte does not take AQUA1's category shortcut. Round and
control dispatch remain conditional. **Inferred:** these declarations stage
the burrow encounter. **Unknown:** natural activation, party/state, first actor,
usable input, selected actions, presentation and persistence.

## Agreement and evidence boundary

Standard rigor under [Issue #1252](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1252)
and [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053).
The uncertainty is the selected data-to-activation/order/spell/control join and
its conversion limits. Accepted starting base is
`f84f5c0b16d5f1a89d18db2d63fd976c8bfa9006`, tree
`4b0bc2ddd77202a2a1df0a880a65affb552fe8ae`. This account supports conversion
planning and the English guide; it does not implement or admit an encounter.
Ownership is this primary, the two coverage summaries, and narrow continuation/
final-CURSE edits to the existing Battle23 initialization and outcome owners.
No dependency on unmerged UI work is consumed.

Begin with accepted [burrow admission](./willard-burrow-through-battle23-entry.md)
and [initialization/roster](./battle23-initialization-and-roster.md). Continue
through activation, round construction and conditional individual dispatch.
[Leader cleanup and return](./battle23-outcomes-and-burrow-return.md) retain
their endpoint, and the admission owner retains scenes/followers/warp identities.
Reuse [Battle16 AQUA1](./battle16-initialization-and-first-control.md#attack-ranges-and-aqua-at-the-selected-boundary),
[Battle21 projection](./battle21-initialization-and-first-control.md#four-regions-and-the-actual-projection-consumer)
and [Battle22 orders/rounds](./battle22-initialization-and-first-control.md).
No played encounter, full damage audit, runtime/emulator, screenshot, acquisition,
engine/schema/fixture/index/allowlist change or new framework is authorized.

All original paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
USA retail input identity remains SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Fnnn means flag nnn; indices/ordinals are zero-based. Coordinates/stat values
are source operands or derived initializer values, not observed state.

Selected text reads and24 small triangle substitutions are linear in this
bounded surface, budgeted below64MiB and one minute per check. Retained pair
18,285,979B and `local/issue1005/pair.txt` receive length/mtime checks only;
their payloads are not parsed, copied or regenerated. Receipts are isolated in
ignored `local/issue1252`. Reassess before material resource/scope expansion.

## Admission, actual party and saved-state contrast

**Confirmed:** `code/common/maps/getbattle.asm:CheckBattle` and the complete
`data/battles/global/battlemapcoords.asm` select the sole Map27 row23:
`27,23,14,27,16,255,255`. F423 unlocks it; both trigger coordinates are wildcards,
without crop containment. The crop is origin23,14/size27x16. F523 set clears
F423 while returning23 for this call; clear523 retains it. F473 gates the
intro wrappers independently; F88 selects current-state resume. Admission's
F781 and requested `(0,0,Right)` warp do not replace those four selectors or
normalize abnormal supplied coordinate high bytes.

Fresh `code/gameflow/battle/battleloop_1.asm:BattleLoop` clears PLAYER_TYPE,
sets F399 and, with F88 clear, resets seconds and assigns current27/23 around
the before wrapper. Both complete48-word before/start tables select ms_Empty
at slot23/offset46. Before does not set F473; start sets it before dispatch.
An empty script still uses the common script-engine save/wait/restore tail.
Set473 skips scripts, not fresh initialization. Region flags90–105 are cleared;
healing, ally placement, enemy initialization, ClearAiMemory and LoadBattle(0)
precede start. Required map/fade/VInt/script services must return before @Start.

`battleloop/heallivingandimmortalallies.asm` restores maxHP/MP for living allies
and Peter7/Lemon28 even when dead; ordinary dead allies are skipped. Its
STUN/POISON/CURSE mask `$0007` is intermediate. In
`code/common/stats/updatecombatantstats.asm:UpdateCombatantStats`, the next mask
excludes CURSE, current stats are rebuilt, and equipped non-NOTHING items call
ApplyItemOnStats. For an ally its saved cursed-item test can OR CURSE back in.
Thus final CURSE follows actual equipped cursed gear, not the incoming status
bit; retained STUN/POISON and gear effects also affect current stats. No fixed
class, inventory, membership or status follows from a placement template.

`initializecombatants.asm:InitializeAllAlliesBattlePositions` first sets all30
ally X bytes to-1; UpdateForce rebuilds ascending joined/active lists. Only X/Y
are consumed from the twelve templates. Dead listed members consume one of
the twelve processed-member slots without advancing the placement pointer;
later living members use earlier unconsumed positions. Excess members leave
the active party, without an immediate list/count rebuild in LeaveBattleParty.
Fewer eligible members give fewer placements. The unguarded initial read of a
supplied empty list is not safe-empty-roster support. Follower flags and named
scene actors are not party-admission or living-placement predicates.

With F88 set, BattleLoop restores seconds, clears88 and AI memory, reloads
current map/terrain/entities and branches to @IndividualTurns_Loop. It bypasses
fresh assignment/healing/roster/region clears/hooks and new round generation.
Current battle, area, HP/status/items, orders/flags, saved queue/cursor and
LoadBattle's view argument are supplied state. A sentinel can later reach
@Start, but no coherent fresh23 or cross-process restoration is established.

### Actual selected global consumers

| Table and direct consumer | Battle23 selection and limit |
| --- | --- |
| `data/battles/terrainentries.asm`; `battleloop/loadbattleterraindata.asm:LoadBattleTerrainData` | Slot23*4 selects BattleTerrain23/`entries/battle23/terrain.bin`, decompressed into BATTLE_TERRAIN_ARRAY. No new payload decoding or terrain route is claimed. |
| `data/battles/global/custombackgrounds.asm`; `battlescenes/getbattlescenebackground.asm:GetBattlesceneBackground` | Slot23 is BURROW, a non-OVERWORLD custom background; terrain-background fallback is not the selected ordinary23 arm. This is a selector, not rendered presentation. |
| `global/enemyleaderpresence.asm`; defeated wrapper | Slot23 is-1, enabling the existing leader cleanup after its Bowie/128 predicates. This is independent of commandset14 and AGI extra turns. |
| Complete `global/halvedexpearnedbattles.asm`; `battleactions/giveexpandgold.asm` | Only INSIDE_ANCIENT_TOWER=1, so23 does not take its EXP-halving branch. |
| Complete `global/afterbattlepositions.asm`; `cutscenes/afterenemyleaderdeathpositions.asm` | Only GALAM_CASTLE=5; no23 placement override. FAIRY_WOODS=44 is the separate timer branch in LoadBattle/individual turn/victory. |
| Complete `global/randombattles.asm`, `enemyitemdrops.asm`, `battleneutralentities.asm` | No23 repeat substitution, special-drop row or neutral group. F523 does not create an upgrade; NOTHING inventory is not a special-drop entitlement. |
| Complete `cutscenes/regionactivatedcutscenes.asm` | Only32/40; no23 region-program dispatch. |

## Complete selected formation and definitions

**Confirmed:** `data/battles/spritesets/entries.asm` slot23 selects
BattleSpriteset23 at`$1B4BFA..$1B4D2C`,306bytes. Header12/12/1/1 means four
header bytes,24 twelve-byte records,12 region bytes and2 point bytes. Subsection
offsets are4/148/292/304. `sf2macros.asm` and `sf2enums.asm` define record bytes:
identity0, X/Y1/2, commandset3, item word4/5, primary order/region6/7,
secondary order/region8/9, filler/spawn10/11.

All twelve ally records have HEALER1=0, NOTHING=`$007F`, primary/secondary
NONE=`$FF` with15/15, filler0 and STARTING=0. Every differing field is:

| Placement | X/Y | Placement | X/Y |
| --- | --- | --- | --- |
| 0 | `(4,5)` | 6 | `(2,6)` |
| 1 | `(4,4)` | 7 | `(4,6)` |
| 2 | `(3,5)` | 8 | `(4,3)` |
| 3 | `(3,4)` | 9 | `(2,7)` |
| 4 | `(3,6)` | 10 | `(3,8)` |
| 5 | `(3,7)` | 11 | `(3,3)` |

All twelve enemy records have NOTHING, secondary NONE/15, filler0 and STARTING.
These common fields plus the following rows cover all24 complete records:

| Row /combatant | Ordinal type | X/Y | Set | Primary byte /region |
| --- | --- | --- | --- | --- |
| 0 /128 | WILLARD90 | `(18,7)` | LEADER14 | MOVE_TO\|0=`$40` /15 |
| 1 /129 | RAT15 | `(20,6)` | ATTACKER3=9 | FOLLOW_ENEMY\|0=`$80` /15 |
| 2 /130 | RAT15 | `(20,8)` | ATTACKER3=9 | FOLLOW_ENEMY\|0=`$80` /15 |
| 3 /131 | BUBBLING_OOZE16 | `(17,1)` | ATTACKER2=7 | FOLLOW_TARGET\|0=`$00` /0 |
| 4 /132 | RAT15 | `(16,2)` | ATTACKER1=6 | FOLLOW_TARGET\|0=`$00` /0 |
| 5 /133 | BUBBLING_OOZE16 | `(15,2)` | ATTACKER1=6 | FOLLOW_TARGET\|0=`$00` /0 |
| 6 /134 | RAT15 | `(21,12)` | ATTACKER2=7 | FOLLOW_TARGET\|0=`$00` /0 |
| 7 /135 | BUBBLING_OOZE16 | `(22,13)` | ATTACKER1=6 | FOLLOW_TARGET\|0=`$00` /0 |
| 8 /136 | BUBBLING_OOZE16 | `(20,14)` | ATTACKER2=7 | NONE=`$FF` /0 |
| 9 /137 | RAT15 | `(10,11)` | ATTACKER1=6 | NONE=`$FF` /15 |
| 10 /138 | BUBBLING_OOZE16 | `(9,10)` | ATTACKER3=9 | FOLLOW_ENEMY\|9=`$89` /15 |
| 11 /139 | BUBBLING_OOZE16 | `(5,12)` | ATTACKER1=6 | NONE=`$FF` /15 |

`battleloop/initializecombatants.asm:InitializeEnemyStats` selects by type
ordinal*56 and copies14 longwords. Raw unknownByte is not the lookup key:
Willard90's raw byte is0. These are the complete three definitions in
`data/stats/enemies/enemydefs.asm` (ordinals90/15/16):

| Definition | Raw; spellPower; level | HP/MP | Base ATT/DEF/AGI/MOV | Resistance | Prowess | Mover /base AI |
| --- | --- | --- | --- | --- | --- | --- |
| Willard90 | 0; ENHANCED99;25 | 50/0 | 50/36/165/6 | STATUS_MAJOR=`$8000` | CRITICAL150_1IN4\|DOUBLE_1IN16\|COUNTER_1IN16 | STEALTH3 /PRIORITYMOD_0=`$0000` |
| Rat15 | 15; REGULAR0;22 | 41/0 | 45/32/30/6 | 0 | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN16 | STEALTH3 /PRIORITYMOD_2\|BIT5=`$2020` |
| Ooze16 | 16; REGULAR0;21 | 32/0 | 38/35/26/4 | ICE_MAJOR\|FIRE_WEAKNESS | CRITICAL_SILENCE\|DOUBLE_1IN32\|COUNTER_1IN16 | REGULAR1 /PRIORITYMOD_2\|BIT5=`$2020` |

Every definition has initialStatus NONE and exactly four NOTHING items.
Willard/Rat have four NOTHING spells; Ooze has AQUA\|LV2=`$68`, then three
NOTHING=`$3F`. All row items are exact NOTHING, so InitializeEnemyItems returns
before AddItem; no copied gear, equipped bonus, item-use spell or row-item
mask effect needs inventing. Willard's ENHANCED field does not grant a spell.

Initialization sets currentHP/MP from maxima, row X/Y, packed mover/commandset,
type, trigger nibbles and order words. Trigger bytes are`$0F` on131–136,
`$FF` elsewhere. Primary/secondary order words are`$40FF` on128,`$80FF` on129/130,
`$00FF` on131–135,`$89FF` on138 and`$FFFF` on136/137/139.
The activation write retains copied`$F000` and ORs rotated/masked initialization
`$0000`: Willard0, Rat/Ooze`$2000`. BIT5 is discarded; primary/secondary-active,
AI-control and neutral bits are clear. Slots140–159 take absent initialization:
activation0, max/currentHP0 and X=-1, with other fields not generally cleared.
STARTING declares no respawn/hidden wave;160 is not an allocated enemy row.

AdjustEnemyBaseAttForDifficulty's consecutive BEQs make every difficulty apply
floor(ATT*5/4); SUPER2 applies it twice. Current ordinary ATT after refresh is
62/56/47 for Willard/Rat/Ooze on0/1/3,77/70/58 on2. Selected status/gear add no
further modifier. DEF/AGI/MOV/resistance/prowess rebuild from base fields; raw
AGI165 remains165, with its turn interpretation below. These values are not damage.

## Region0: projection, tested cache and activation

**Confirmed:** the full record is
`4,0,9,0,9,15,26,15,26,0,0,0`: type4 and vertices1–4
`(9,0),(9,15),(26,15),(26,0)`, byte1/trailing0/0. Point0 is`(22,3)`.
Coordinates are battle-local; no23/14 subtraction occurs in these consumers.

`battleloop/triggerregions.asm:IsRegionTriggered` tries triangles1/2/4 and
3/2/4. AreAnyAlliesInTriangle scans all30 allies with positive signedHP and
nonnegative signed-byte X/Y, without joined/active tests. IsAllyInTriangle tests
edges first1→2, then3→1, then3→2 using each triangle's local vertex numbering.
CheckTestPointPositionRelativeToSegment chooses the dominant absolute span
(horizontal on ties), uses signed MULS/DIVS truncated toward zero, and compares
the projected coordinate with the test coordinate using SUB.W's unsigned borrow.
For a horizontal edge:
`Yproj=Yb+trunc((Yb−Ya)*(Xc−Xb)/(Xb−Xa))`; vertical exchanges X/Y.
Collinear in-bounds01 accepts immediately; ordinary00/10 must match the opposite
vertex. Negative projection writes CCR=1 and yields10 despite the11 comment;
bit0 exceptions skip early edges but reject at the third. A conventional polygon
test is not a substitute for this selected consumer.

All12 templates into both triangles give24 substitutions and zero hits:

| Triangle | First rejection, original vertices | Projection/flag for every template | Opposite vertex |
| --- | --- | --- | --- |
| 1,2,4 | First edge1→2, vertical X9 | X9 versus allyX2–4:00 | vertex4 X26, projection9:10 |
| 3,2,4 | Third edge4→2, horizontal Y19–21 | Y19–21 versus allyY3–8:00 | vertex3 Y15, projection0:10 |

For the second triangle, edges3→2 and4→3 pass before4→2 rejects.
On4→2, `Yproj=15+trunc(−15*(X−9)/−17)`; X2 gives21, X3 gives20,
X4 gives19. Thus the actual first-rejection witnesses cover each placement,
not just their rectangle bounding box. A smaller living party can use a prefix
of these positions and still miss; a supplied, moved or saved ally has a new
premise. All30 slots are tested, so a living placed reserve can matter at a
direct/saved call even without active membership.

At @Start, ActivateEnemies scans128–159; UpdateEnemyActivationBitfield requires
X≥0, HP≠0 and enemy identity before calling TriggerRegionsAndActivateEnemies.
Fresh128–139 qualify,140–159 do not. After DBF the outer body has no RTS and
falls through into the helper once more for160. GetCombatantEntryAddress's
BHI bound admits160, mapping it to storage64 at`$FFF600` (CURRENT_GOLD/deals),
not another enemy. The [existing alias owner](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
retains X`$FFF62E`, HP`$FFF60E`, triggers`$FFF636` and activation`$FFF634`.
Eligible pseudo-X/HP and held non15 trigger flags can OR Goddess Staff count
bits, not unconditionally add a staff. Fresh cleared flags and these misses
supply no held trigger for that write; changed flags/positions/deals are separate.

Each region's
NEWLY_TRIGGERED_BATTLE_REGIONS bit skips retesting when set; a miss also sets
that tested bit. A hit sets persistent F90. Assignment then reads persistent
flags: primary0 on131–136 ORs bit0 when F90 is set; secondary15 cannot select
another order. Primary wins; the generic secondary-hit path sets both bits,
but no selected secondary region exercises it.15/15 skips assignment; it is
not a region15 polygon or an activation bit written for unconditional enemies.

Fresh clearing F90–105 is not a write to the tested word. ClearAiMemory clears
48 last-target bytes/48 memory bytes, not this word. Initial @Start can therefore
reuse supplied tested state. With coherent fresh templates either a new test
misses or a pretested bit skips; cleared F90 stays clear. Normal StartAiControl
clears the tested word before standby/active handling, even on a tested miss;
persistent flags survive. The next reached activation scan can test changed
positions. Do not replace this service lifetime with an assumed per-round reset.
At @Start the region hook then spawn list precede round generation.
PopulateTargetsListWithSpawningEnemies scans128–159 and tests initialization
mask`$0F00`; selected STARTING and absent mode0 skip all hidden/respawn work,
yielding no fresh spawn list. A saved changed mode requires its own conditions.
No selected region script, hidden spawn or actual crossing is established.

## Live orders and complete command execution

**Confirmed:** `ai/startaicontrol.asm` selects no SWARM for these sets. Ordinary
handling clears the tested word, then15/15 bypasses standby. Assigned0/15 with
primary-active clear invokes DetermineAiStandbyMovement and returns STAY.
131–135's assigned FOLLOW_TARGET0 fails ValidateAiStandbyEligibility's non-NONE
primary/declared-region test: it produces no standby move.136 has NONE/0 and
can build conditional local standby movement around its source starting position,
using thinking RNG, memory, current terrain, occupancy and station/path success.
STAY action is not a promise of zero movement. Fresh128/129/130/137/138/139 use
ordinary AI despite clear active bits. Later active131–136 enter their streams.

`battlefield/getmoveorderposition.asm` tests bit6 before low-nibble point lookup.
Willard's`$40` reads declared point0`(22,3)`, different from fresh`(18,7)`.
Raw0 on131–135 instead calls combatant X/Y getters for live ally0/Bowie;
it is neither point0, enemy128 nor automatic opponent selection.129/130 read
live128, initially`(18,7)`;138 reads live137, initially`(10,11)`.
These are not physical actor indices or immutable formation coordinates.

Normal active handling checks any non-NONE non-MOVE_TO primary's targetHP.
HP0 promotes secondary and clears secondary, even for set6 which has no order
command. Here every secondary is NONE. Inactive standby occurs earlier, so it
does not perform that promotion. Target life and current position are separate:
nonzeroHP does not itself guarantee a valid mapped/on-map target or path.

The full16-entry pointer table in `data/battles/global/aicommandsets.asm`
selects these complete ordered streams;14 aliases AiCommandset13:

| Set | Preparatory pathfinding | Complete stream |
| --- | --- | --- |
| 6 ATTACKER1 | REGULAR0 | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY |
| 7 ATTACKER2 | REGULAR0 | MOVE_ORDER1, ATTACK1, HEAL1, SUPPORT, MOVE1, STAY |
| 9 ATTACKER3 | BLOCK_NON_MOVABLE1 | MOVE_ORDER3, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER4, MOVE2, STAY |
| 14 LEADER | BLOCK_AND_CARVE2 | MOVE_ORDER5, ATTACK2, HEAL3, SUPPORT, MOVE_ORDER2, MOVE3, STAY |

StartAiControl promotes dead follows, applies the preparatory restriction and
then ExecuteAiCommand dispatches until a command returns d1=0. Earlier success
short-circuits later commands; cleanup clears temporary obstructions on exit.
MOVE_ORDER1/2/3/4/5 pass targetType/pathMode0/0,2/2,1/1,0/1,0/2 respectively.
HEAL1/2/3 are command parameters, not newly learned spell ranks. Selected
enemies have neither healing/support spells nor held items; failed filters do
not grant an ability from a stream label.

`ai/command/moveorder.asm` rejects MOV0, NONE and dead live targets. Its type0
station test uses returned X via tst.b, retaining source-specific X0/-1 behavior.
A reached order can attack internally or move-and-STAY before later commands.
BLOCK_NON_MOVABLE and LEADER's block-and-carve center use the actual order;
the latter's radius depends on last-target state. Restrictions can still be
applied after a failed command. Movement arrays, current MOV/terrain, opposing
and allied occupancy, attack stations and BuildMoveStringForMoveOrder remain
conditions. Stored ally0 follows on set6 do not make its ATTACK1/MOVE1 follow0.
No fixed pursuit route, standing boss, target or action follows from the table.

## AQUA2 selection and the later effect boundary

**Confirmed:** `data/stats/spells/spelldefs.asm` entry AQUA|LV2 is packed`$68`
(base40/`$28`, LV2=`$40`): MP0, TYPE_ATTACK, range1–1, radius0, power15,
BUBBLE_BREATH|VARIATION2. It lacks AFFECTEDBYSILENCE. The four-slot learned
list, usable rank, chosen command/action and damage execution are separate.
Willard/Rat have no learned spells; ENHANCED on Willard does not create AQUA.

`ai/getnextuseableaiaction.asm:GetNextUsableAttackSpell` masks the base index,
skips NOTHING and accepts TYPE_ATTACK. Ordinary unconfused enemies can admit
AQUA2. Confused casters and ally casters use the explicit BLAZE/FREEZE/BOLT/
BLAST/KATON/RAIJIN whitelist, which excludes AQUA. IsCombatantConfused tests
the actual MUDDLE/MUDDLE2 combination; do not equate every AI-preparation MUDDLE
branch with that spell filter. GetHighestUsableSpellLevel starts at learned
rank1, reconstructs `(rank<<6)+base`, and unsigned-compares currentMP with cost.
At freshMP0, AQUA2 cost0 succeeds at rank1; it is not downgraded to AQUA1 or
rejected for lack of MP.

`command/attack/prioritizetargets.asm` builds movement arrays and separate item,
spell and physical reachable lists. Selected empty inventory supplies no item
category. Spell priorities use actual target lists, resistance, difficulty,
activation priority and prior-target state; physical potential also uses ATT,
DEF and land effect. BuildTargetsArrayWithAllCombatants and
GetTargetsReachableBySpell save/restore d1 (the latter through its shared
PopulateReachableTargetsList tail); GetSpellDefinitionAddress also preserves
the packed key. The spell list therefore stores the **full returned word**`$68` in
ATTACK_COMMAND_SPELL at lines134–136; no rank mask precedes the category test.

`command/attack/determinebattleaction.asm:84–109` compares that full word with
`#SPELL_AQUA=$28`. Unlike the accepted Kraken AQUA1, level2`$68` does **not**
take the no-RNG6 shortcut. With physical and spell categories available and no
item, main RNG range6 chooses spell on2 or4 and physical on the other values.
With only spell candidates it still makes that range6 draw, then selects the
spell regardless of the roll; physical-only skips this spell-category draw.
No candidates returns STAY. Additional target/prioritization draws and an
earlier MOVE_ORDER success remain separate; an order's internal ATTACK1 can
also reach this same selector. Neither MP0 nor TYPE_ATTACK fixes the action.

After category selection the highest priority/tie/class/movement consumers
choose a target. `ai/command/attack.asm` writes the full spell word, obtains its
actual range and constructs a station/path. A selected command result is not
proof that movement, action dispatch or an effect completed. Physical
GetAttackRange is1–1 for these three unarmed definitions; the AI's unarmed
station branch separately uses0–1. Identical displayed range does not merge
physical ATT/DEF and spell power/resistance calculations.

At the later execution seam, `battleactions/initbattlesceneproperties.asm`
checks AFFECTEDBYSILENCE before setting silencedActor; AQUA's properties do not
set that flag through this test. `castspell.asm`/`calculatespelldamage.asm` and
GetResistanceToSpell own actual cost/effect/resistance processing. The copied
spell-power byte is read as class by GetClass/AdjustSpellPower; selected Ooze
REGULAR is not Willard ENHANCED. No enhanced healing/damage or performed15
damage follows. `attack.asm`, `determinecriticalhit.asm`, `inflictailment.asm`,
`determinedoubleandcounter.asm` and `isabletocounterattack.asm` under battleactions
retain critical/silence/prowess and follow-up execution boundaries; source expressions above are not successful
status applications, counterattacks or extra hits. This is not a full damage audit.

## Actors, round generation and first conditional control

**Confirmed:** LoadBattle/PositionBattleEntities replace exploration actors and
clear49 physical records/64 lookup bytes. Ordinary nonnegative X/Y declares
actors, with neutral-marked enemies excluded and no separateHP check. With N
declared allies, fresh enemies128–139 occupy physical N..N+11; mapsprites are
Willard154, Rat79 and Ooze80, all ordinary below240. Enemy type90, logical128,
sprite154 and physicalN are different identities. No23 neutral group adds
158/159. F523 set skips neutral scanning; clear523 reaches its no-match end.
Supplied saved neutral fields are not repaired by the absence of a group.
The admission owner retains exploration128–130/follower/raft mapping details.

`battleloop/turnorderfunctions.asm` clears64 sentinel entries, scans30 allies
then enemy128–157 and admits X≥0/HP≠0 independently of region activation.
Fresh enemies supply13 entries: Willard165 gives ordinary A=`165 & 127=37`
and an extra; Rats30 and Oozes26 give one each. Slots140–157 have noHP/position;
158/159 are outside this round scan, and160 outside the combatant range.

Ordinary score is `A+RNG(A>>3)−RNG(A>>3)+RNG(3)−1`. Willard's extra base is
`floor(37*5/6)=30`, scored `30+RNG(30>>3)−RNG(30>>3)` without a third draw.
The criterion is currentAGI≥128, independent of leader presence/set14/point0.
Range0 still advances the main seed. Stable signed-byte descending sort keeps
append-order ties and sets cursor0. Actual allies/current AGIs and seeds, plus
earlier draws/services, determine the first actor. Neither leader status nor
thirteen enemy entries proves a first player-ready actor.

`battlefunctions/executeindividualturn.asm` masks identity and skipsHP0.
MUDDLE or AI-control bit2 selects AI preparation; otherwise enemies use AI when
CONTROL_OPPONENT_TOGGLE=0 and player preparation when nonzero. Ally auto-battle
is separate. After cursor/view/window/grid services, SLEEP/STUN goes NoAction;
the same control/status polarity then chooses ExecuteAiControl or player input.
AI preparation/order/cache mutations can precede status-based inaction.
Fresh enemy status0 does not constrain supplied allies or saved actors.
The queue sentinel returns to @Start. The account stops at this conditional
dispatch, without asserting accepted input, a natural action or its return.

## Accepted production mechanisms and conversion limits

**Confirmed:** inspection at the accepted project base separates producers,
trusted reading, descriptive records, assembled capability and execution.

| Actual owner | Reusable support and selected23 limit |
| --- | --- |
| [prepare_encounter / compile_routes](../../src/sf2tool/remake_exploration_battles.py), [scene producer](../../src/sf2tool/remake_battle_scene_content.py) | Encounter producer selects2/3, scene producer1/2/3. Wildcard/400–450–500 flags and before/load/start phases exist; selected23 route/export is not admitted. Existing burrow scene owners remain separate. |
| [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [PrivateBattleDefinitionReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) | Trusted pins/geometry select1/2/3 and reject23. Ordinal definitions can describe Willard90 with raw0, all three stats/resistance/prowess/spells and AQUA2. A new name/raw0 requires no new definition infrastructure. Descriptive support is not admission. |
| [PrivateBattleScenarioReader.Assemble](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs), [PrivateBattleActionBindings](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Independent region-program admission1/2/3 rejects23 even though source23 has no program. STARTING/NOTHING, sets6/7/9/14, NONE/point/live orders and REGULAR/STEALTH bind generically. Willard ENHANCED fails REGULAR-only derived-state admission; all three selected prowess expressions reject (including counter1IN16 and SILENCE). Rat/Ooze AI regex accepts PRIORITYMOD_2\|BIT5; initialization discardsBIT5. Healing-only spell binding does not execute AQUA2. Supported controlled classes0/1/2/4 do not cover arbitrary natural parties. |
| [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) | Membership/resources/seeds, living/immortal healing, formation and difficulty0 ATT/activation are reusable. Excess members reject instead of source eviction; general ally gear/final-CURSE refresh, actual status/class producers, other difficulties/control and F88 restoration remain limited. Selected enemy empty loadouts fit; no new enemy gear mechanism is required. |
| [BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs), [BattleControlRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) | Four-vertex regions, primary-first/sticky-secondary,15/15, randomized and extra rounds exist. Geometry uses cross products rather than original signed projection/CCR, and BeforeRound resets tested state locally rather than preserving the source service lifetime. Matching fresh misses does not prove later/saved-state equivalence. Generic round/control primitives do not admit or observe23. |
| [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs), [SourceMoveOrderAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs), [BattleTerrainRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTerrainRules.cs) | Standby, cache clear on normal AI, dead-follow promotion,6/7/9/14, LEADER prepass, valid point0 and live0/128/137 follow are reusable. STEALTH is supported through Content and terrain costs; Rat/Willard names do not require a mover addition. Activated AI rejects nonzero status, automatic allies and learned spells; Ooze AQUA2 needs actual category/priorities/RNG/station/effect support. No SWARM or missing-point mechanism is selected. |
| [ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs) and actor/adapter boundary | Generic entity/services and logical projection do not establish selected23 actor-load/physical lookup, completed services, natural control or saved-state fidelity. No admitted23 adapter observation exists here. |

These are bounded conversion requirements, not implementation/admission-only
changes or a demand for new geometry/AI infrastructure by enemy name. Preserve
the reusable mechanisms and assess their actual valid-state boundary when
implementation is authorized.

## Unknowns, failures and reproduction

**Unknown:** naturally supplied flags/party/gear/classes/status/seeds, moved/
saved geometry and cache state, actual activation/target/station/path, first
actor/player input, AQUA2 or physical action, damage/silence/counter/double,
completed services, presentation/timing/hardware and persistence. No runtime
question here is an automatic acquisition queue.

Preserve [#1047's twelve completed unresolved comparisons](../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03),
the [control correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415),
intermediate/final-CURSE, [Battle12 HEAL/identity](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1234)
and [Battle13 neutral](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1236)
findings/original failures. [PR1245's independent P2](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1245#issuecomment-6366643680)
and accepted`c1893c7b` corrected candidate-local HEAL missed by initial`16f4233a`
mechanical PASS. [PR1251's correction record](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1250#issuecomment-6368359671)
retains initial`70fbb37c` mechanical PASS versus accepted`425d504e`: the actual
after-leader table is Battle5, independently of timer44. No completed failure
is relabeled interrupted, unavailable or semantically passed by these checks.

Reproduce from complete spriteset23/macros/enums and ordinal90/15/16 definitions;
join actual map/hook/global tables to named direct consumers. Verify306bytes,
24 records, exact region/point and all24 signed substitutions with rejection
traces. Follow tested miss/hit versus persistent F90, normal cache reset,
inactive follow/NONE and15/15, dead-follow promotion, prepass and every selected
stream. Trace learned`$68`→affordable rank→full ATTACK_COMMAND_SPELL write→exact
`$28` comparison→RNG6 category→station/path. Review actual production admission,
rejection and reusable support independently, including STEALTH and prowess.

Direct checks in `local/issue1252` cover UTF-8/LF, links/anchors/tables/private
boundary, exact five paths, unchanged retained lengths/mtime, fixture metrics
and four frontiers. On a clean committed head load `local/private-inputs.ps1`
in the same process and run `uv run sf2 verify plan --base origin/main --head HEAD`;
inspect actual exact-head scoped CI. Normal/full/SDK/Godot/H3/H4/historical
reruns are outside this documentation acceptance. A frozen Draft PR and passing
mechanical checks still require independent main-gate semantic review/merge.
