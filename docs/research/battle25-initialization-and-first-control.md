# Battle25 initialization and first conditional control

Evidence date: 2026-10-23. **Confirmed:** selected Battle25 joins twelve
formation templates, fifteen enemies, three initially missed regions and
live follow chains to conditional first dispatch. Dark Soldier's learned
ATTACK and Lesser Demon's BOOST2 pass the support getter but fail SUPPORT's
early command gate. **Unknown:** natural party, activation, first actor,
actions, loot, completed services, presentation and persistence. This is
static conversion guidance, not a played encounter or engine acceptance.

## Agreement and provenance

**Standard** rigor under [Issue1257](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1257)
and [continuation Epic1255](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1255)
fits composition of accepted consumers with selected geometry, follow-chain,
spell and item differences. Accepted project base is
`1db828c9bdd8fa2ad23c0a72625e0050800113b2`. Own this account and matching
selected25 scope in [story coverage](./story-content-coverage.md) and the
[source ledger](./source-coverage.md), plus continuation links and necessary
final-CURSE clarity in the [admission](./north-cliff-entry-through-battle25-admission.md)
and [outcome](./battle25-outcomes-through-ketto-entry.md) owners. Those owners
retain travel/request, Rohde, field scenes, opening, outcomes and return.
No code, schema/fixture/index, engine admission, runtime, screenshot, retained
payload parsing, bulk collection or new verification framework is included.

Reuse [Battle23 region/live-order consumers](./battle23-initialization-and-first-control.md),
[Battle24 spell/item consumers](./battle24-initialization-and-first-control.md)
and [candidate-local healing](./battle19-initialization-and-first-control.md).
Paths below are relative to `disasm/` at
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm),
whose read-only checkout is clean. Existing contracts identify USA retail
SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no fresh ROM equivalence is claimed. Constants use `sf2enums.asm`, RAM uses
`sf2const.asm`; Fnnn denotes flag nnn. Indices are zero-based and extents
half-open. Short consumer paths are under `code/gameflow/battle/` unless
qualified otherwise.

Minimum evidence is selected source and actual accepted production consumers.
The72 arithmetic cases are linear in twelve templates times six triangles,
with small ignored receipts, below one minute/64 MiB. The retained
18,285,979-byte map-init/script pair and metadata companion are checked only
for unchanged length/mtime; no payload is parsed, copied or regenerated.
Reassess material growth or contradiction before expanding the method.

## Admission, fresh initialization and supplied suspension

**Confirmed:** the complete Map48 matching set in
`data/battles/global/battlemapcoords.asm` is rows11/25, in that scan order:

| Battle | Crop X/Y and size | Exact trigger | Unlock /intro /completion |
| --- | --- | --- | --- |
| 11 Cave of Darkness | `(0,0)` /27×32 | `(15,26)` | 411 /461 /511 |
| 25 North Cave | `(28,0)` /32×32 | `(55,29)` | 425 /475 /525 |

`code/common/maps/getbattle.asm:CheckBattle` tests effective map, unlock and
trigger, not crop containment. At55,29 row11 fails even with411 held;25 needs425.
At15,26 only11 can match. At52,12 neither qualifies from rectangle membership.
With425/525 both held,25 is selected, its crop stored and425 cleared without
rescanning. Completion525 alone does not admit25. Request production and
consumption remain distinct; the admission owner retains mutable request/
high-byte, Map72 and Rohde boundaries. F475 and F88 are independent. Local
`(x,y)` corresponds to Map48 `(x+28,y)` for this selected crop.

`battleloop_1.asm:BattleLoop` clears PLAYER_TYPE and sets399. Fresh88-clear
entry clears seconds, saves/assigns incoming map/battle and installs base VInts;
before returns, then map/battle are restored, F90–105 cleared, living/immortal
allies healed, allies positioned, enemies initialized, AI memory cleared,
combatant0 loaded and start invoked, in that order. Slot25 in
`beforebattlecutscenes.asm` and `battlestartcutscenes.asm` selects `ms_Empty`,
the single `$FFFF` at `$47A4E..$47A50` (pointer words `$47ABA`/`$47B5E`).
Before tests475 without setting it; start sets475 before dispatch. Set475
skips both scripts, not fresh shared initialization. Empty return can still
depend on scrolling. Neither hook adds a scene, join or scripted move.

Healing visits all30 allies independently of membership. Living allies and
Peter7/Lemon28 receive HP/MP maxima and an intermediate status mask retaining
STUN/POISON/CURSE; other dead allies skip. The subsequent
`code/common/stats/updatecombatantstats.asm:UpdateCombatantStats` removes
incoming CURSE from its working mask, initializes current stats, applies
status/equipped effects and writes final status. Final ally CURSE follows
current equipped cursed gear, not the intermediate mask alone. STUN changes
MOV/AGI; actual classes, gear and other supplied state remain separate.

LoadBattle/LoadBattleTerrainData selects terrain pointer25 into the battle
terrain array independently of the crop. `global/custombackgrounds.asm`
slot25 is CAVE; `battlescenes/getbattlescenebackground.asm` takes this fixed
background rather than OVERWORLD's terrain-low-nibble lookup. Its actor
fallback/Zeon override remains separate, not selected by these base definitions.
`enemyleaderpresence.asm` slot25 is0. Half-EXP's list has only Battle1;
after-leader positioning only Battle5; timer special only44. None follows
from Soldier128 being the first enemy.

With88 set, BattleLoop restores saved seconds, clears88/AI memory, selects
view0, loads supplied current battle state and jumps into its supplied
queue/cursor. Incoming48/25 do not replace saved map/battle. Fresh hooks,
region clearing, healing and placement are skipped; the following @Start
branch is unreachable. No fresh account repairs saved state. Interrupted
services keep their reached prefix, without rollback or usable-control proof.

## Complete formation and original enemy definitions

**Confirmed:** `spritesets/entries.asm` slot25 selects `spriteset25.asm`,
`$1B4EA6..$1B5012`, 364 bytes. Header12/15/3/0 gives offsets4/148/328/364:
header4 +27 twelve-byte actors +3 twelve-byte regions +zero points. All actor
fillers/spawn modes are0/STARTING. Macros in `sf2macros.asm` encode identity,
X/Y/AI, item word, primary order/region, secondary order/region, filler/mode.
GetBattleSpritesetSubsection uses counts and strides, not enemy names.

All twelve ally records use HEALER1=0, NOTHING=`$007F`, orders NONE/NONE
and regions15/15. Complete placement indices/local coordinates:

| Index | Position | Index | Position |
| --- | --- | --- | --- |
| 0 | `(25,26)` | 6 | `(27,25)` |
| 1 | `(26,26)` | 7 | `(26,25)` |
| 2 | `(27,26)` | 8 | `(28,26)` |
| 3 | `(26,27)` | 9 | `(28,25)` |
| 4 | `(27,27)` | 10 | `(27,28)` |
| 5 | `(28,27)` | 11 | `(28,24)` |

`battleloop/initializecombatants.asm:InitializeAllAlliesBattlePositions`
resets30 ally X values to-1 and rebuilds joined/active members in ascending
ally-ID order. Twelve processed members bound placement. Dead members
consume a loop slot without advancing the template pointer; the next living
member takes that unconsumed coordinate. Surplus members LeaveBattleParty;
cached list/count are not immediately rebuilt. No initial empty-list guard
exists. Only X/Y copy: template index/item/AI do not replace actual identity,
inventory, status or AI. Rohde11's join, active43, follower84, health and
placement are distinct; his field actor does not guarantee participation.

Complete enemy records follow. AI numbers are HEALER3/2, ATTACKER1/6 and
ATTACKER3/9; NONE=`$FF`, FOLLOW_ENEMYn=`$80|n`. All secondary orders are NONE.
Item words are authored source values, not promised stored permissions.

| Row /combatant | Definition | Position | AI | Primary order | Regions primary/secondary | Item word |
| --- | --- | --- | --- | --- | --- | --- |
| 0 /128 | DARK_SOLDIER18 | `(24,7)` | 6 | NONE | 0/15 | `$007F` |
| 1 /129 | DARK_BISHOP81 | `(21,10)` | 2 | FOLLOW_ENEMY0 | 0/15 | `$007F` |
| 2 /130 | LESSER_DEMON45 | `(19,9)` | 2 | FOLLOW_ENEMY0 | 0/15 | `$007F` |
| 3 /131 | DARK_BISHOP81 | `(6,6)` | 2 | FOLLOW_ENEMY4 | 1/1 | `$007F` |
| 4 /132 | DARK_SNIPER62 | `(7,9)` | 9 | FOLLOW_ENEMY5 | 1/1 | `$007F` |
| 5 /133 | SKELETON17 | `(5,10)` | 6 | NONE | 1/15 | `$6000` Herb |
| 6 /134 | LESSER_DEMON45 | `(14,12)` | 6 | NONE | 1/15 | `$007F` |
| 7 /135 | SKELETON17 | `(10,16)` | 6 | NONE | 2/15 | `$2000` Herb |
| 8 /136 | DARK_SNIPER62 | `(8,17)` | 6 | NONE | 2/15 | `$007F` |
| 9 /137 | LESSER_DEMON45 | `(6,28)` | 6 | NONE | 2/15 | `$007F` |
| 10 /138 | DARK_SNIPER62 | `(13,28)` | 9 | FOLLOW_ENEMY11 | 15/15 | `$007F` |
| 11 /139 | SKELETON17 | `(14,27)` | 6 | NONE | 15/15 | `$2000` Herb |
| 12 /140 | SKELETON17 | `(15,19)` | 6 | NONE | 15/15 | `$2000` Herb |
| 13 /141 | DARK_SNIPER62 | `(18,21)` | 9 | FOLLOW_ENEMY14 | 15/15 | `$007F` |
| 14 /142 | SKELETON17 | `(20,22)` | 6 | NONE | 15/15 | `$6000` Herb |

Initialization scans128–159. Count15 admits128–142; absent143–159 receive
activation0, max/currentHP0 and X=-1. Their trigger-byte read is still through
the subsection pointer, not an extra row. Battle32/Jaro is unrelated.
STARTING0 calls InitializeEnemyStats: upgrade selection precedes the56-byte,
14-longword definition copy. Then currentHP/MP=max, row X/Y, selected mover/row
commandset, enemy ID, triggers/orders, row item, activation, difficulty ATT
and UpdateCombatantStats are applied. Orders pack primary<<8 plus secondary;
regions primary<<4 plus secondary. Fresh activation retains only definition
`$F000`, then filler/mode: Bishop`$3000`, others`$2000`, active/neutral clear.
Definition BIT4/BIT5 are not fresh activation bits.

Complete selected ordinal definitions in `data/stats/enemies/enemydefs.asm`
follow. Raw unknownByte equals the ordinal here; neither is logical128–142
or a physical entity index. All have initialStatus NONE and spellPower
ENHANCED99, stored in the class-read byte.

| Ordinal /raw byte | Level | HP/MP | Base ATT/DEF/AGI/MOV | Mover /raw AI |
| --- | --- | --- | --- | --- |
| Soldier18 /18 | 23 | 35/17 | 31/46/34/4 | REGULAR1 /PRIORITYMOD_2 `$2000` |
| Bishop81 /81 | 23 | 39/38 | 28/38/27/5 | HEALER12 /PRIORITYMOD_3 `$3000` |
| Demon45 /45 | 22 | 40/21 | 49/37/32/6 | FLYING5 /PRIORITYMOD_2\|BIT4\|BIT5 `$2030` |
| Sniper62 /62 | 24 | 38/0 | 11/37/35/5 | STEALTH_ARCHER10 /PRIORITYMOD_2\|BIT4 `$2010` |
| Skeleton17 /17 | 21 | 30/0 | 26/40/30/6 | REGULAR1 /PRIORITYMOD_2\|BIT5 `$2020` |

| Definition | Full resistance | Full prowess |
| --- | --- | --- |
| Soldier | 0 | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN16 |
| Bishop | 0 | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN32 |
| Demon | STATUS_MINOR | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN16 |
| Sniper | 0 | CRITICAL150_1IN32\|DOUBLE_1IN16\|COUNTER_1IN16 |
| Skeleton | WIND_WEAKNESS\|LIGHTNING_MINOR\|ICE_MAJOR\|FIRE_WEAKNESS\|STATUS_IMMUNITY | CRITICAL150_1IN8\|DOUBLE_1IN32\|COUNTER_1IN16 |

| Definition | All four item slots | All four learned spell slots |
| --- | --- | --- |
| Soldier | equipped LARGE_AXE38,127,127,127 | ATTACK1`$05`,63,63,63 |
| Bishop | equipped FLAIL90,127,127,127 | AURA1`$01`,BLAST3`$8E`,63,63 |
| Demon | 127,127,127,127 | BLAZE3`$8B`,BOOST2`$43`,63,63 |
| Sniper | equipped HYPER_CANNON53,127,127,127 | 63,63,63,63 |
| Skeleton | equipped BROAD_SWORD62,127,127,127 | 63,63,63,63 |

Difficulty's consecutive BEQs apply floor(baseATT×5/4) once at non-SUPER,
twice at SUPER2: Soldier38/47, Bishop35/43, Demon61/76, Sniper13/16,
Skeleton32/40. Copied equipped weapons add25/19/0/40/22, giving fresh
currentATT63/54/61/53/54 ordinarily and72/62/76/56/62 at SUPER. DEF/AGI/MOV
stay at listed bases for these fresh statuses/gear. Enemy copied equipment
is already equipped; ordinary ally class compatibility does not veto it.
Initialized stats/prowess metadata do not prove damage or performed effects.

UpgradeRandomBattleEnemies calls IsBattleUpgradable first. The eleven-entry
`global/randombattles.asm:list_RandomBattles` omits25, so no completion,
effective-level/category/retry or upgrade RNG selection follows here. F525,
high/promoted Bowie and altered seeds cannot turn25 into24's repeat path.
Definitions stay these five on fresh25; F88 skips their initialization.
This excludes upgrade draws, not subsequent AI/turn/action RNG.

## Three exact regions and all72 signed arithmetic cases

**Confirmed:** all regions are type4, unknown/trailing bytes0, with vertices:

| Region | Vertex1 | Vertex2 | Vertex3 | Vertex4 |
| --- | --- | --- | --- | --- |
| 0 | `(0,0)` | `(0,14)` | `(31,14)` | `(31,0)` |
| 1 | `(0,0)` | `(0,31)` | `(7,31)` | `(31,7)` |
| 2 | `(0,0)` | `(0,31)` | `(18,31)` | `(18,0)` |

`battleloop/triggerregions.asm:IsRegionTriggered` scans all30 allies with
positive signedHP and nonnegative signed-byte X/Y, without membership checks.
Type4 tries triangles(1,2,4), then(3,2,4); each tests A→B,C→A,C→B. Horizontal
is chosen when |dx|≥|dy|. Signed MULS/DIVS truncates toward zero:
`Yproj=Yb+trunc((Yb−Ya)×(X−Xb)/(Xb−Xa))`; vertical exchanges X/Y.
SUB.W borrow controls flags00/10; collinear/in-bounds01 accepts immediately,
out-of-bounds returns10. Ordinary flags must equal the opposite vertex's flag.
Bit0 exceptions skip either early edge but reject at the third. Negative
projection sets CCR=1 and returns10 despite the11 comment. These cases have
no word overflow or degenerate edge. Rectangle containment is not the algorithm.

These first-rejection witnesses cover all72 substitutions. Earlier tests,
where present, have the equal flags shown; rejection is test10/reference00.
Every formula is also substituted at its opposite reference vertex.

| Region /triangle | First rejection and preceding tests | Opposite-vertex witness | Projection formula and substitution check |
| --- | --- | --- | --- |
| 0 /(1,2,4) | edge4→2 after two equal10 tests | vertex1 `(0,0)`: Yproj14 | `Yproj=14+trunc(14×X/−31)`; X25/26→3,27/28→2; X0→14. |
| 0 /(3,2,4) | edge3→2 | vertex4 `(31,0)`: Yproj14 | `Yproj=14`; Y24–28 gives10, referenceY0 gives00. |
| 1 /(1,2,4) | edge4→2 after two equal10 tests | vertex1 `(0,0)`: Yproj31 | `Yproj=31+trunc(24×X/−31)`; X25→12,26/27→11,28→10; X0→31. |
| 1 /(3,2,4) | edge4→3 after one equal00 test | vertex2 `(0,31)`: Yproj38 | `Yproj=31+trunc(24×(X−7)/−24)`; X25/26/27/28→13/12/11/10; X0→38. |
| 2 /(1,2,4) | edge4→2 after two equal10 tests | vertex1 `(0,0)`: Xproj18 | `Xproj=trunc(−18×(Y−31)/31)`; Y24/25/26/27/28→4/3/2/2/1; Y0→18. |
| 2 /(3,2,4) | edge4→3 after one equal00 test | vertex2 `(0,31)`: Xproj18 | `Xproj=18`; X25–28 gives10, referenceX0 gives00. |

Complete per-template rejection projections, in those six columns:

| t /position | R0a Y | R0b Y | R1a Y | R1b Y | R2a X | R2b X |
| --- | --- | --- | --- | --- | --- | --- |
| 0 `(25,26)` | 3 | 14 | 12 | 13 | 2 | 18 |
| 1 `(26,26)` | 3 | 14 | 11 | 12 | 2 | 18 |
| 2 `(27,26)` | 2 | 14 | 11 | 11 | 2 | 18 |
| 3 `(26,27)` | 3 | 14 | 11 | 12 | 2 | 18 |
| 4 `(27,27)` | 2 | 14 | 11 | 11 | 2 | 18 |
| 5 `(28,27)` | 2 | 14 | 10 | 10 | 2 | 18 |
| 6 `(27,25)` | 2 | 14 | 11 | 11 | 3 | 18 |
| 7 `(26,25)` | 3 | 14 | 11 | 12 | 3 | 18 |
| 8 `(28,26)` | 2 | 14 | 10 | 10 | 2 | 18 |
| 9 `(28,25)` | 2 | 14 | 10 | 10 | 3 | 18 |
| 10 `(27,28)` | 2 | 14 | 11 | 11 | 1 | 18 |
| 11 `(28,24)` | 2 | 14 | 10 | 10 | 4 | 18 |

All three fresh regions miss for a coherent living placement subset. Fewer
living members use a prefix of the same stream. Moved/saved positions or a
living placed reserve among all30 require their actual premises.

At @Start, ActivateEnemies scans128–159 with X≥0/HP≠0/enemy identity; fresh
128–142 qualify. Missing RTS falls through for160; the BHI combatant bound
admits it, aliasing storage64 at `$FFF600` CURRENT_GOLD/deals. The
[alias owner](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
retains pseudo-X`$FFF62E`, HP`$FFF60E`, triggers`$FFF636`, activation`$FFF634`.
Eligible pseudo-state and held non15 trigger flags can OR Goddess Staff count
bits. This is not an unconditional staff award; fresh cleared flags/misses
supply no held trigger for that write.

TriggerRegionsAndActivateEnemies tests each region once per tested-word
lifetime. A miss also sets NEWLY_TRIGGERED_BATTLE_REGIONS; a hit sets F90/91/92.
Fresh flag clearing and ClearAiMemory's two48-byte arrays do not clear that
tested word. Supplied tested bits can skip initial tests; coherent fresh misses
leave F90–92 clear either way. Normal non-SWARM StartAiControl clears the tested
word before standby/active handling; persistent flags survive. This is a
service lifetime, not an assumed per-round reset.

Primary triggered region wins, ORing bit0 without clearing an old secondary
bit. Only absent/untriggered primary permits secondary to OR bits0+1. Rows131/
132 have equal1/1: setF91 always takes primary, never creates a secondary bit
through this test, and preserves any supplied sticky bit. All other assigned
rows have secondary15. A secondary NONE order does not suppress activation;
no order swap occurs here. The five138–142 rows are15/15 and bypass assigned-
region activation/standby;15 is a sentinel, not a region. The other ten
initially remain inactive. `cutscenes/regionactivatedcutscenes.asm` has no25
program. PopulateEnemySpawns' `$0F00` mode test rejects STARTING0 and absent
mode0; no hidden/spawned wave is declared. Saved modes remain conditional.

## Live follow chains, standby and exact2/6/9 streams

**Confirmed:** no selected row is MOVE_TO, LEADER or SWARM, and zero points
are declared. GetMoveOrderPosition tests bit6 before its point lookup; all
selected non-NONE orders take live combatant coordinates. There is no selected
out-of-table point read or missing-point defect to infer.

| Following actor(s) | Live target /initial coordinate | Further chain |
| --- | --- | --- |
| 129,130 | 128 Soldier `(24,7)` | 128 has NONE |
| 131 | 132 Sniper `(7,9)` | 132 follows133 |
| 132 | 133 Skeleton `(5,10)` | 133 has NONE |
| 138 | 139 Skeleton `(14,27)` | 139 has NONE |
| 141 | 142 Skeleton `(20,22)` | 142 has NONE |

These are logical combatant IDs, not physical entity slots or fixed points.
An order reads the immediate target's current position, not a transitive
ultimate leader or the listed starting coordinate after it moves.

Inactive assigned129–132 have non-NONE primaries with a declared region.
ValidateAiStandbyEligibility returns nonzero, which its caller treats as no
standby move despite the helper comment's polarity. Initial thinking-RNG
range8 precedes validation. Assigned128/133–137 have NONE primary and can
build local standby movement around their source start, subject to RNG,
AI memory, terrain, occupancy and path/station success. Both groups return
STAY action; that does not imply an empty movement string. Five15/15 actors
bypass standby despite clear active bits.

Normal active handling checks any non-NONE/non-MOVE_TO primary targetHP
before the stream. HP0 promotes secondary and clears it; here every secondary
is NONE. Inactive standby precedes promotion. This matters for129/130/131
even though HEAL is first, and for138/141 before MOVE_ORDER3. Dead133 clears
132's follow without automatically clearing131's follow of living132. HP
nonzero proves neither position nor path. Region flags and move orders are
separate state; sticky activation does not itself substitute the order.

`global/aicommandsets.asm` and its pathfinding table supply complete streams:

| Set /selected combatants | Preparatory mode | Complete ordered stream |
| --- | --- | --- |
| 2 HEALER3 /129,130,131 | BLOCK_NON_MOVABLE1 | HEAL2, MOVE_ORDER4, SUPPORT, ATTACK3, MOVE2, STAY |
| 6 ATTACKER1 /128,133–137,139,140,142 | REGULAR0 | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY |
| 9 ATTACKER3 /132,138,141 | BLOCK_NON_MOVABLE1 | MOVE_ORDER3, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER4, MOVE2, STAY |

StartAiControl stops on first d1=0 and clears temporary obstructions on exit.
Earlier HEAL/MOVE_ORDER/ATTACK success prevents later commands. No selected
BLOCK_AND_CARVE/LEADER prepass occurs. MOVE_ORDER3/4 pass targetType/pathMode
1/1 and0/1. MOV0, NONE and dead target reject ordered commands; type0's station
check tests returned X byte, so X0 fails while-1 can continue. Ordered movement
can attack internally or move-and-STAY. Set6 has no MOVE_ORDER consumer: a
stored order would not create one. Lesser Demon130's HEALER3 selects a stream,
not a healing spell or permission; BLAZE/BOOST has no TYPE_HEAL and it holds
no item. No named fixed tactic/action or first actor follows.

## Selected spell decisions and later effects

**Confirmed:** reuse full-key, getter/MP, category, candidate-local, station/
path and later-effect consumers in
[Battle24](./battle24-initialization-and-first-control.md#aura1-blast3-blaze3-and-boost2).
Selected definitions here are unchanged;25 has no repeat substitution.
`data/stats/spells/spelldefs.asm` supplies the exact keys below.
All listed learned spells have AFFECTEDBYSILENCE. AURA/BOOST/ATTACK target
teammates; BLAST/BLAZE target opponents.

| Learned spell /full key | MP | Range /radius | Power |
| --- | --- | --- | --- |
| ATTACK1 `$05` | 7 | 1–3 /0 | 15 |
| AURA1 `$01` | 7 | 0–3 /1 | 15 |
| BLAST3 `$8E` | 8 | 1–2 /1 | 12 |
| BLAST2 `$4E` /BLAST1 `$0E` | 5 /2 | 1–2 /1;1–1 /0 | 8 /6 |
| BLAZE3 `$8B` | 10 | 1–2 /1 | 15 |
| BLAZE2 `$4B` /BLAZE1 `$0B` | 6 /2 | 1–2 /1;1–2 /0 | 10 /6 |
| BOOST2 `$43` | 5 | 0–2 /2 | 15 |

### Dark Soldier ATTACK: getter success, command failure

`ai/getnextuseableaiaction.asm:GetNextSupportSpell` scans learned slots,
masks the base to skip NOTHING, reads TYPE_SUPPORT and returns the full
original key. Soldier's first slot ATTACK1`$05` qualifies. It is not TYPE_ATTACK:
GetNextUsableAttackSpell does not turn its name into an offensive category,
and GetNextHealingSpell does not admit it either.

`ai/command/support.asm` first requires enemy/nonconfused caster, then accepts
only full-word MUDDLE2`$47` or DISPEL`$06`. `$05` fails both and takes DoNothing:
STAY, empty move string and failure, before MP, movement arrays, targets or
station work. Soldier17MP could cover7 but that check is never reached here.
The later ATTACK prioritizer is likewise unreachable for selected ordinary
SUPPORT. Failure lets set6 continue to MOVE1 after earlier commands fail;
it does not prove the whole turn stays still. No automatic ATTACK buff follows
from learned name, stream token or ENHANCED. Demon BOOST2`$43` passes the same
getter but fails the same early exact gate; Bishops have no support spell.
Other callers/effect engines are separate.

### AURA healing and BLAST/BLAZE attack selection

Bishops129/131 have first healing spell AURA1. Heal rejects actual confusion,
tries Rain, then learned TYPE_HEAL and ordinary usable healing items. AURA
requires currentMP≥7; no base enemy starts injured. Friendly candidates need
`3×HP≤2×maxHP`. Full`$01` radius1 builds an affected group and sums each
CalculateHealTargetPriority+4; candidates sort descending, retaining ties.
Mover priorities here are Soldier2, Bishop12, Demon6, Sniper10, Skeleton2.
No selected commandset13/14 priority13 override applies. Area score, command
order and station/path remain necessary, not a fixed lowest-HP target.

DetermineHealingSpellLevel reads current local spellEntry. Incoming`$01`
rank0 returns-1 for missingHP≤2, otherwise d2=0; the buggy shift5/unmasked-key
cost expression still selects`$01`/7MP. Caller nonself promotion needs incoming
rank≥2 and cannot promote AURA1. Repacking keeps01, including after a failed
station when a later candidate is tried. It uses AURA range0–3, not HEAL3's
mutable local80→40/00 path. Learned slots are not rewritten. HEAL2 in the
stream is an unused command parameter, not learned rank2.

Attack-spell getter skips AURA and finds Bishop BLAST3; Demon finds BLAZE3.
GetHighestUsableSpellLevel reconstructs `(rank<<6)+base` downward and
unsigned-compares currentMP with actual costs. Bishop38/Demon21 admit rank3;
depleted MP can select a known lower rank or NOTHING. BLAZE/BLAST belong to
the confused/ally whitelist; IsCombatantConfused needs the actual MUDDLE/
MUDDLE2 combination, not every preparation MUDDLE branch.

Attack prioritization builds separate item/spell/physical reachable lists
with actual ranges, resistance, priority, difficulty, last-target and movement
state. Full returned word reaches ATTACK_COMMAND_SPELL: neither8E nor8B equals
exact AQUA1`$28`. Spell+physical without item uses main range6:2/4 spell,
otherwise physical; spell-only still draws, physical-only skips that draw.
Viable item+spell uses thinking range2 (1 item,0 spell); item+physical uses
main range6 (3/5 item, otherwise physical), with item-only fallback after the
draw. No category returns STAY. Target/tie, station/path and earlier stream
success remain separate from category choice.

Later initbattlesceneproperties gates silence for CAST_SPELL; castspell,
resistance/damage and PerformAnimation retain actual effects and MP charging.
AdjustSpellPower reads copied class/ENHANCED99 and applies5/4 to non-full
CAST_SPELL power before caps/resistance. AURA15 can become18 before missingHP
cap; USE_ITEM bypasses that enhancement and spell-MP charge. No performed
cast, full damage/prowess/follow-up or visible animation is claimed.

## Equipment, masked Herbs and independent action drops

**Confirmed:** selected physical/item metadata in `data/stats/items/itemdefs.asm`:

| Item | Equip /item range /price /type /use | Three equipment effects |
| --- | --- | --- |
| Large Axe38 | GLDT/BRN/RDBN /1–1 /2250 /WEAPON /NOTHING63 | ATT+25,NONE0,NONE0 |
| Flail90 | WIZ/SORC/VICR /1–1 /1490 /WEAPON /NOTHING63 | ATT+19,NONE0,NONE0 |
| Hyper Cannon53 | SNIP/BRGN/BWNT /2–3 /8700 /WEAPON\|RARE /NOTHING63 | ATT+40,NONE0,NONE0 |
| Broad Sword62 | HERO/BRN/BDBT/NINJ/RDBN /1–1 /1600 /WEAPON /NOTHING63 | ATT+22,NONE0,NONE0 |
| Herb0 | NONE /0–1 /10 /CONSUMABLE /HEALIN1 | NONE0,NONE0,NONE0 |

GetAttackRange reads equipped gear: Soldier/Bishop/Skeleton1–1, Sniper2–3.
Unarmed Demon defaults1–1 while its AI physical station branch uses0–1.
These differ from learned spell and item ranges.

InitializeEnemyItems skips exact NOTHING. AddItem finds first index&127=127
slot, masks successful input by`$807F`, stores/returns it and sets d2=0.
All five Skeleton Herbs (`$2000`/`$6000`) become`$0000` in slot1 after copied
equipped Sword`$BE`. AI-use13 and unused-drop14 are stripped; returned bit7
is clear, so the caller does not equip Herb. Copied Axe`$A6`, Flail`$DA` and
Cannon`$B5` stay equipped in their definitions. No25 row supplies Blizzard.
Saved inventory/capacity can alter addition/getter order.

IsItemUsableInBattle incorrectly rejects useSpell255 instead of63. Attack-item
getter can reach an equipped copied weapon, then its NOTHING63 use spell
falls back to first HEAL1 in GetSpellDefinitionAddress, fails TYPE_ATTACK and
returns NOTHING without advancing to a later slot. Fresh Herb has neither
equipped nor AI-use permission and fails ordinary healing-item selection too;
it is not Rain. Restoring Herb bit13 does not bypass an earlier weapon's
attack-getter termination, nor turn HEALIN into TYPE_ATTACK. Learned Soldier
ATTACK and weapon useSpell NOTHING remain different keys/categories.

If inventory permits the healing getter to reach Herb, HEALIN`$10` is MP0,
TYPE_HEAL|TARGET_TEAMMATES, range0–1/radius0/power10 without silence flag.
Heal uses actual friendly deficit/priority, item0–1 station/path and actual
slot, not learned AURA rank. USE_ITEM is still only a command. UseItem resolves
use spell and branches CastSpell; PerformAnimation chargesMP only for
CAST_SPELL, and initial silence distinguishes that action. After target loop,
BreakUsedItem removes the actual nonequipment Herb slot before its enemy-user
exemption for breakable weapons/rings. No break RNG, spare copy or enemy
consumable exemption follows. Early inaction/no-target/incomplete services
does not prove effect or consumption.

`global/enemyitemdrops.asm` independently declares exactly two selected25 rows:

| Target row /combatant | Required currently held index | Once flag /bank bit |
| --- | --- | --- |
| 5 /133 | MEDICAL_HERB0 | 22 /byte2 bit6 |
| 14 /142 | MEDICAL_HERB0 | 23 /byte2 bit7 |

`battleactions/dropenemyitem.asm` follows target effects, including actual
actor/target in double/counter contexts. It needs ally actor, enemy target,
local targetDies, matching battle/slot and held index found with mask127.
Authored UNUSED_ITEM_DROP, table shape, HP0 elsewhere or leader sweep grants
nothing. Herb0 branches directly to DropItem without drop RNG; only110/102/68
take range32/zero first. Later upgradable/range3 code is unreachable, not an
extra25 RNG gate.

DropItem BSETs once first and returns if already set. Otherwise it removes/
compacts target slot and refreshes stats, then checks actorHP. Living actor
gets AddItem at its first free slot. Dead actor/full inventory takes rare-deals
fallback; Herb is not RARE and is discarded. No alternate ally is chosen.
Once/removal can occur without receipt; receipt also needs later message/
services. Four-byte bank`$FFF7A4` differs from story F22/F23 and F525; fresh
region/AI initialization does not reset it. RAM lifetime is not persistence.
Unlike24 there is no selected rare Blizzard/deals increment. Changed held
items still must match the table; lost/used Herbs, held once bits and actual
killing actor remain conditions.

## Actors, rounds and first conditional control

**Confirmed:** LoadBattle/PositionBattleEntities replace field population,
clear49 physical records/64 lookup bytes and scan allies0–29 then ordinary
enemies. Nonnegative X/Y admits without an independentHP check; neutral enemies
skip that branch. With N declared allies, fresh128–142 map to physicalN..N+14.
Base mapsprites Soldier82/Bishop145/Demon109/Sniper126/Skeleton81 are below240,
using ordinary slots. A live follow resolves through current mappings, not
an old Rohde/scene actor. `global/battleneutralentities.asm` has no25 entry
under its completion scan; no159/158 neutral is declared, and saved neutral
fields are not repaired.

`battleloop/turnorderfunctions.asm` clears64 sentinel entries, scans30 allies
then128–157 and admits X≥0/HP≠0 independently of activation. Fresh unchanged
enemies supply fifteen ordinary entries with orderedAGIs
`34,27,32,27,35,30,32,30,35,32,35,30,30,35,30`; none has bit128.
Ordinary score is `A+RNG(A>>3)−RNG(A>>3)+RNG(3)−1`. Supplied currentAGI≥128
adds extra base `floor((AGI&127)×5/6)` scored with two draws, independently
of outcome leadership. Range0 still advances main seed. Stable signed-byte
descending sort preserves append-order ties and sets cursor0. Actual allies,
gear/status and earlier RNG determine order; inactive enemies still participate.
No first actor or player-ready state is fixed by formation/AGI.

`battlefunctions/executeindividualturn.asm` masks identity and skipsHP0.
MUDDLE or AI-control bit2 selects AI preparation; otherwise enemy opponent
control0 chooses AI and nonzero chooses player preparation. Ally auto-battle
is separate. Cursor/view/window/grid services precede SLEEP/STUN NoAction;
the same control polarity later selects ExecuteAiControl or player input.
Preparation can mutate cache/orders before status-based inaction. Fresh enemy
status0 does not constrain supplied allies/suspension. Queue sentinel returns
to @Start. Stop at these conditional branches, before accepted input/action,
completed presentation, outcome or persistence.

## Actual production consumers and conversion limits

**Confirmed by inspection of the accepted project base:** descriptive data,
trusted transport, assembled capability and execution have separate boundaries.
No selected25 content is produced, trusted, admitted or observed here.

| Actual owner | Reusable mechanism and selected25 limit |
| --- | --- |
| [prepare_encounter /compile_routes](../../src/sf2tool/remake_exploration_battles.py), [scene producer](../../src/sf2tool/remake_battle_scene_content.py) | Encounter preparation admits2/3; scene build1/2/3. Route compiler rejects coordinate-triggered selection, which25 needs. Existing flag/phase/wildcard mechanisms do not supply25 exact Map48 admission, request/field-party conversion or selected export. Opening/outcome conversions stay with their owners. |
| [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [PrivateBattleDefinitionReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) | Ordinal definitions, full gear/spell/resistance/prowess metadata, regions and live orders can describe all five bases. Trusted pins admit1/2/3 and reject25. A new enemy name alone does not need new infrastructure. |
| [PrivateBattleScenarioReader.Assemble](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs), [PrivateBattleActionBindings](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Region-program admission1/2/3 rejects25 despite no25 program. STARTING/NONE/live and6/9 bind generically; HEALER3/2 rejects. Placement accepts NOTHING or exact Herb6000, rejecting2000. All five ENHANCED definitions reject REGULAR-only state. BIT4 rejects Demon/Sniper; Soldier/Bishop/Skeleton raw AI expressions fit descriptively. Bishop ordinary prowess fits; other four counter/double/critical expressions reject. HEAL-only learned binding does not execute AURA/BLAST/BLAZE/BOOST/ATTACK. Axe/Flail/Sword adjacent ATT-only WEAPON metadata fits physical binding; Cannon WEAPON\|RARE/2–3 fails. |
| [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) | Membership/resources/seeds, living/immortal healing, formation, difficulty0 ATT and activation are reusable. Excess members reject instead of source eviction; actual party/classes beyond0/1/2/4, gear/final-CURSE refresh and other difficulty/control remain limited. Enemy equipment accepts only0/127, rejecting copied weapons. F88 rejects instead of restoring supplied queue/cursor. No25 upgrade algorithm is needed, because source selection omits25. |
| [BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs), [BattleControlRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) | Four-vertex regions, primary-first/sticky-secondary,15/15 and ordinary/extra randomized turns exist. Geometry uses cross products instead of signed projection/CCR, BeforeRound resets tested state locally instead of source service lifetime, and no160 deals alias is represented. Fresh misses do not prove later/saved equivalence or actual25 control. |
| [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs), [SourceMoveOrderAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs), [BattleTerrainRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTerrainRules.cs) | Standby, normal cache reset, dead-follow promotion,6/9 and live128/132/133/139/142, Flying/Regular/Healer and rounds are reusable. Content Mover admits STEALTH but not STEALTH_ARCHER; Domain has no Archer mover, so generic Stealth costs do not bind Sniper. Activated AI rejects set2, nonzero status, automatic allies and loadouts beyond0/127/no learned spells. Exact healing/offensive/item decisions, AURA area/local-rank and actual ATTACK/BOOST rejection need conversion. No selected point/LEADER/SWARM platform is missing here. |
| [Physical action](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2PhysicalAction.cs), [ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), Application/entity and adapter boundary | Generic physical/reward/control and logical projection do not implement selected133/142 held0/once22/23, consumption/remove/receipt/deals order, full resistance/prowess or source actor/service/presentation lifetime. No25 adapter observation, natural input, suspend or persistence acceptance exists. |

These are conversion requirements over valid states/content, not widening-only
admission edits or a mandate for speculative infrastructure. Original source
remains the evidence for original behavior; production code establishes only
its own implemented/rejected boundary.

## Unknowns, retained failures and reproduction

**Unknown:** natural route/flags/party/classes/gear/status/seeds, supplied
tested/deals state, moved/saved regions, follow/standby/targets/stations/paths,
AURA/BLAST/BLAZE/Herb choices, other-call ATTACK/BOOST effects, damage/prowess/
follow-ups, first actor/input/actions, death/loot, completed map/scroll/entity/
text/animation/sound/VInt services, presentation/hardware and persistence.
Completing this selected set is not all-action, special-battle, full-game or
engine completion and does not create a next research/runtime queue.

Preserve [#1047's twelve completed unresolved comparisons](../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03),
the [control correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415),
intermediate/final-CURSE, [Battle12 HEAL/identity](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1234)
and [Battle13 neutral](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1236)
findings/original failures. [PR1245's P2](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1245#issuecomment-6366643680)
retains initial16f4233a mechanical PASS versus acceptedc1893c7b candidate-local
HEAL correction. [PR1251](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1250#issuecomment-6368359671)
retains initial70fbb37c mechanical PASS versus accepted425d504e after-position44→5;
[PR1253](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1252#issuecomment-6368882658)
retains initial6dfd61b0 versus acceptedf171630a formula numerator−15→+15,
a prose formula failure rather than failed geometry.
[PR1256's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1254#issuecomment-6369333245)
retains pre-stage semantic corrections and two completed direct-check failures.
These records are not interrupted runs or semantic passes.

Reproduce from complete spriteset25/macros/enums, definitions18/81/45/62/17,
actual map/hook/global tables and direct consumers named above. Check364bytes/
27 actors/three exact regions/zero points and all72 substitutions with first-
rejection traces and independent substitution of every printed formula/value.
Follow tested versus persistent flags, equal1/1 priority,15/15, NONE versus
live-follow standby and exact2/6/9 streams/prepasses/dead promotion. Follow
full keys through getter/category/MP, ATTACK/BOOST's early gate, AURA local01/
area/station and later effect; masked inventory, getter termination, use and
133/142 drop/once/remove/receipt/deals order. Review actual producer/trust/
Content/Domain/Application boundaries separately.

Direct checks in `local/issue1257` cover UTF-8/LF, links/anchors/tables/private
boundary, exact five paths, unchanged retained metadata/fixture metrics/four
frontiers, selected source and arithmetic. On the clean committed head load
ignored `local/private-inputs.ps1` in the same process and run
`uv run sf2 verify plan --base origin/main --head HEAD`; inspect exact-head
scoped CI. No normal/full/SDK/Godot/H3/H4/historical reruns belong to this
documentation acceptance. Freeze a clean pushed Draft PR with exact identities,
source/production/check/failure handoff for independent main-gate review/merge.
