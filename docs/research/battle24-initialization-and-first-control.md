# Battle24 initialization and first conditional control

Evidence date: 2026-10-23. **Confirmed:** selected Battle24 joins twelve
formation templates, sixteen enemies, three initially missed regions, point0,
conditional repeat selection and ordinary AI to conditional first dispatch.
Dark Bishop's AURA1/BLAST3, Lesser Demon's BLAZE3/BOOST2 and held Blizzard
have different usability and command paths. **Unknown:** natural party,
repeat roster, activation, first actor, actions, loot, completed services,
presentation and persistence. This is static conversion guidance, not played
encounter or engine acceptance.

## Agreement and provenance

**Standard** rigor under [Issue1254](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1254)
and [continuation Epic1255](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1255)
fits composition of accepted consumers with selected geometry/spell/item
differences. Accepted project base is `d26987078f8cbeb0301f828d30348af6269250e4`.
Own this account and matching selected24 scope in the
[story coverage](./story-content-coverage.md) and [source ledger](./source-coverage.md),
plus continuation links/final-CURSE clarity in the
[admission](./new-granseal-departure-through-battle24-admission.md) and
[outcome](./battle24-outcomes-through-north-cliff-preparation.md) owners.
Those owners retain departure, cave scenes, outcomes and return. No code,
fixture/schema/index, engine admission, emulator, screenshot, acquisition,
retained-payload parsing or broad enumeration is included.

Reuse [Battle17 repeat selection](./battle17-initialization-and-first-control.md),
[Battle19 candidate-local healing/item custody](./battle19-initialization-and-first-control.md),
[Battle21 geometry/repeat/drop consumers](./battle21-initialization-and-first-control.md)
and [Battle23 region/live-order consumers](./battle23-initialization-and-first-control.md).
Paths below are relative to `disasm/` at
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm),
whose read-only checkout is clean. Existing contracts identify USA retail
SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no fresh ROM equivalence is claimed. Constants use `sf2enums.asm`, RAM uses
`sf2const.asm`; Fnnn denotes flag nnn. Indices are zero-based and extents
half-open. Short consumer paths are under `code/gameflow/battle/` unless
qualified otherwise.

Minimum evidence is the selected source and actual accepted production
consumers. The 72 tiny triangle substitutions are linear in twelve templates
times six triangles, with small local receipts, below one minute/64 MiB.
The retained 18,285,979-byte map-init/script pair and its metadata companion
are checked only for unchanged length/mtime; no payload is parsed, copied or
regenerated. Reassess material growth or contradiction before expanding work.

## Admission, fresh initialization and supplied suspension

**Confirmed:** the global scan visits rows8,11,24,26 in that order; row11
belongs to Map48 and is skipped on72, leaving the admission owner's matching
8/24/26. `code/common/maps/getbattle.asm:CheckBattle` tests map, unlock and wildcard trigger;
it does not require crop containment. A source default24 request writes424,
but retained408 can select8 first. With408 clear,424 selects24 before426;
completion524 clears424 after selection without rescanning. Request production,
surviving pending parameters and actual admission remain distinct.

Selected24 crop is Map72 origin `(1,32)`, size32x30, trigger `(255,255)`.
Local `(x,y)` corresponds to map `(x+1,y+32)`; placements are stored locally.
F424 unlock, F474 intro, F524 completion/repeat and F88 suspension are
independent. Janet/items/805/806 are not extra battle admission predicates.

`battleloop_1.asm:BattleLoop` clears PLAYER_TYPE and sets399. Fresh88-clear
entry clears seconds, saves/assigns incoming map/battle and installs base VInts;
before returns, then map/battle are restored, F90–105 cleared, living/immortal
allies healed, allies positioned, enemies initialized, AI memory cleared,
combatant0 loaded and start invoked, in that order. Both slot24 pointers in
`beforebattlecutscenes.asm`/`battlestartcutscenes.asm` select `ms_Empty`, the
single `$FFFF` at `$47A4E..$47A50`. Before tests474 without setting it;
start sets474 before dispatch. Set474 skips both scripts but not the fresh
shared initialization. Empty script return can still depend on scrolling;
there is no selected dialogue, join or scripted opening movement.

Healing visits all30 allies independently of membership. Living allies and
Peter7/Lemon28 receive HP/MP maxima and an intermediate status mask retaining
STUN/POISON/CURSE; other dead allies skip. The subsequent
`code/common/stats/updatecombatantstats.asm:UpdateCombatantStats` **removes
incoming CURSE** from its working mask, initializes current stats, applies
status/equipped effects and writes final status. Final ally CURSE follows
current equipped cursed items, not the intermediate mask alone. STUN changes
MOV/AGI; current gear/classes/resources remain supplied state.

`LoadBattle`/`LoadBattleTerrainData` selects terrain pointer24 from
`pt_BattleTerrainData` and decompresses into BATTLE_TERRAIN_ARRAY, independently
of the map crop. `custombackgrounds.asm` slot24 is OVERWORLD: the actual
`battlescenes/getbattlescenebackground.asm` then uses current combatant terrain's
low nibble with `terrainbackgrounds.asm`, rather than promising one background.
Its -1 actor fallback uses the retained actor word then0; Zeon override is not
selected by these base definitions. Half-EXP's list contains only Battle1;
after-leader positioning only Battle5; the timer special only Battle44.
None is selected24 or a consequence of LEADER14.

F88 set restores saved seconds, clears88/AI memory, selects view0, loads supplied
current battle state and jumps directly into the supplied queue/cursor. It
skips fresh assignment, hooks, region clearing, healing and placement; the
following branch to @Start is unreachable. No fresh-state account below
repairs or validates a saved state. Interrupted services retain their reached
prefix; no rollback or usable-control claim follows.

## Complete formation and original enemy definitions

**Confirmed:** `spritesets/entries.asm` slot24 points to `spriteset24.asm`,
`$1B4D2C..$1B4EA6`, 378 bytes. Header12/16/3/1 gives offsets4/148/340/376:
header4 +28 twelve-byte actors +3 twelve-byte regions +point2. All actor
fillers/spawn modes are0/STARTING. Macros in `sf2macros.asm` encode identity,
X/Y/AI, item word, primary order/region, secondary order/region, filler/mode.
`GetBattleSpritesetSubsection` uses the counts and strides, not enemy names.

All twelve ally records use HEALER1=0, item NOTHING=`$007F`, orders NONE/NONE
and regions15/15. Complete placement indices/local coordinates:

| Index | Position | Index | Position |
| --- | --- | --- | --- |
| 0 | `(4,18)` | 6 | `(2,19)` |
| 1 | `(3,17)` | 7 | `(2,16)` |
| 2 | `(3,18)` | 8 | `(3,16)` |
| 3 | `(2,18)` | 9 | `(1,16)` |
| 4 | `(3,19)` | 10 | `(1,17)` |
| 5 | `(2,17)` | 11 | `(1,18)` |

`battleloop/initializecombatants.asm:InitializeAllAlliesBattlePositions`
resets30 ally X values to-1 and rebuilds joined/active members in ascending
ally-ID order. Twelve **processed members** bound placement. Dead members
consume a loop slot without advancing the template pointer; the next living
member uses that unconsumed coordinate. Surplus members LeaveBattleParty;
cached list/count are not immediately rebuilt. There is no initial empty-list
guard. Only X/Y copy: template index/item/AI do not replace actual identity,
inventory, status or AI. Following and recruitment do not fix participation.

Complete enemy records follow; all secondary orders are NONE. AI numbers are
LEADER14, HEALER3/2, ATTACKER1/6, ATTACKER2/7 and ATTACKER3/9. MOVE_TO0=`$40`,
FOLLOW_ENEMYn=`$80|n`, FOLLOW_TARGET0=`$00`. Item words remain **authored**.

| Row /combatant | Definition | Position | AI | Primary order | Regions primary/secondary | Item word |
| --- | --- | --- | --- | --- | --- | --- |
| 0 /128 | DARK_BISHOP81 | `(25,7)` | 14 | MOVE_TO0 | 1/15 | `$007F` |
| 1 /129 | DARK_SNIPER62 | `(25,5)` | 9 | FOLLOW_ENEMY0 | 1/15 | `$007F` |
| 2 /130 | SKELETON17 | `(24,8)` | 9 | FOLLOW_ENEMY0 | 1/15 | `$2000` Herb |
| 3 /131 | LESSER_DEMON45 | `(28,10)` | 6 | NONE | 1/15 | `$007F` |
| 4 /132 | DARK_SNIPER62 | `(18,13)` | 6 | NONE | 1/0 | `$007F` |
| 5 /133 | SKELETON17 | `(21,14)` | 6 | NONE | 1/0 | `$6000` Herb |
| 6 /134 | SKELETON17 | `(17,18)` | 6 | NONE | 15/15 | `$2000` Herb |
| 7 /135 | DARK_BISHOP81 | `(18,19)` | 2 | FOLLOW_ENEMY6 | 15/15 | `$6011` Blizzard |
| 8 /136 | SKELETON17 | `(18,20)` | 2 | FOLLOW_ENEMY6 | 15/15 | `$2000` Herb |
| 9 /137 | LESSER_DEMON45 | `(22,25)` | 6 | NONE | 0/1 | `$007F` |
| 10 /138 | LESSER_DEMON45 | `(21,26)` | 6 | NONE | 0/1 | `$007F` |
| 11 /139 | SKELETON17 | `(11,21)` | 6 | NONE | 15/15 | `$2000` Herb |
| 12 /140 | SKELETON17 | `(9,25)` | 6 | NONE | 15/15 | `$6000` Herb |
| 13 /141 | DARK_SNIPER62 | `(8,24)` | 9 | FOLLOW_ENEMY12 | 15/15 | `$007F` |
| 14 /142 | LESSER_DEMON45 | `(14,3)` | 6 | NONE | 1/2 | `$007F` |
| 15 /143 | LESSER_DEMON45 | `(8,8)` | 7 | FOLLOW_TARGET0 | 1/2 | `$007F` |

Initialization scans128–159. Count16 admits128–143; absent144–159 receive
activation0, max/currentHP0 and X=-1. Their trigger-byte read is still through
the subsection pointer, not an extra valid actor row. Battle32/Jaro is unrelated.
STARTING0 calls InitializeEnemyStats; repeat selection precedes the56-byte,
14-longword definition copy. Then currentHP/MP=max, row X/Y, selected mover/row
commandset, selected enemy ID, trigger/order bytes, row item, activation,
difficulty ATT and UpdateCombatantStats are applied. Orders pack primary<<8
plus secondary, regions primary<<4 plus secondary. Activation retains only
definition `$F000`, then filler/mode: Bishop `$3000`, others `$2000`, with
active/neutral bits clear. Raw BIT4/BIT5 are not retained as fresh activation.

With F524 **clear**, the complete ordinal definitions in
`data/stats/enemies/enemydefs.asm` are:

| Ordinal /raw byte | spellPower /level | HP/MP | Base ATT/DEF/AGI/MOV | Mover /raw AI |
| --- | --- | --- | --- | --- |
| Bishop81 /81 | ENHANCED99 /23 | 39/38 | 28/38/27/5 | HEALER12 /PRIORITYMOD_3 `$3000` |
| Sniper62 /62 | ENHANCED99 /24 | 38/0 | 11/37/35/5 | STEALTH_ARCHER10 /PRIORITYMOD_2\|BIT4 `$2010` |
| Skeleton17 /17 | ENHANCED99 /21 | 30/0 | 26/40/30/6 | REGULAR1 /PRIORITYMOD_2\|BIT5 `$2020` |
| Demon45 /45 | ENHANCED99 /22 | 40/21 | 49/37/32/6 | FLYING5 /PRIORITYMOD_2\|BIT4\|BIT5 `$2030` |

| Definition | Full resistance | Full prowess |
| --- | --- | --- |
| Bishop | 0 | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN32 |
| Sniper | 0 | CRITICAL150_1IN32\|DOUBLE_1IN16\|COUNTER_1IN16 |
| Skeleton | WIND_WEAKNESS\|LIGHTNING_MINOR\|ICE_MAJOR\|FIRE_WEAKNESS\|STATUS_IMMUNITY | CRITICAL150_1IN8\|DOUBLE_1IN32\|COUNTER_1IN16 |
| Demon | STATUS_MINOR | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN16 |

All have initialStatus NONE. Bishop item slots are equipped FLAIL90,127,127,127;
Sniper equipped HYPER_CANNON53,127,127,127; Skeleton equipped BROAD_SWORD62,
127,127,127; Demon127×4. Spell slots are Bishop AURA1=`$01`, BLAST3=`$8E`,63,63;
Sniper63×4; Skeleton63×4; Demon BLAZE3=`$8B`, BOOST2=`$43`,63,63.
Spell-power99 occupies the class-read byte; it grants neither a spell nor an
automatic cast. For these four definitions the raw byte equals its ordinal;
logical enemy128–143 and physical entity slots remain separate identities.

Difficulty's consecutive BEQs apply floor(baseATT×5/4) once at non-SUPER,
twice at SUPER2: Bishop35/43, Sniper13/16, Skeleton32/40, Demon61/76. Copied
equipped weapons then add19/40/22/0, giving fresh currentATT54/53/54/61
ordinarily and62/56/62/76 at SUPER, with unchanged status/other effects.
DEF/AGI/MOV remain the listed bases here. Definition-copy equipment is already
equipped; ordinary ally class compatibility does not veto these enemy weapons.
These are initialized stats, not damage or executed prowess effects.

## F524 and actual repeat selection

**Confirmed:**24 is ninth in the eleven-entry `global/randombattles.asm` list.
`battleloop/upgradeenemies.asm:IsBattleUpgradable` and DetermineBattleUpgrade
require list membership and completion524, independently of424/474. Clear524
returns the base without upgrade RNG. F88 skips this fresh-copy path.

The original definition's mover chooses the category before any substitution:

| Original definition /category | Range R /inclusive bounds | Exclusions |
| --- | --- | --- |
| Bishop81 HEALER /Healer | 2 /DARK_CLERIC76..EVIL_BISHOP84 | BISHOP80 |
| Sniper62 STEALTH_ARCHER /Ranged | 2 /GALAM_ARCHER56..DARK_GUNNER65 | KRAKEN_ARM59, ROOK61 |
| Skeleton17 REGULAR /Melee | 5 /OOZE0..REAPER35 | KRAKEN_LEG10, PAWN13, KNIGHT14, RAT15, BURST_ROCK32, ZEON_GUARD38 |
| Demon45 FLYING /Airborne | 2 /HUGE_BAT40..DEMON52 | count0: unsafe exclusion scan |

Bowie0's level supplies E; signed-byte class>11 adds20. Subtract24: a
nonpositive result repeats the read once using shared D4 then returns base.
A positive result divides unsigned by10 and retains quotient low byte q.
q0 returns base; ordinary eligibility begins E34. Byte level plus optional20
gives eligible q1..25. Unknown mover categories default early; these four have
actual selected categories.

Center is base+5q−5 for R5, otherwise base+q. Main RNG range R+1 supplies0..R;
seed advances `(13×seed+7) mod65536`. Signed-byte lower/upper checks precede
the exclusion scan. A failure consumes the one D4 retry, restores center and
draws again; second failure returns original. No clamping or uniform draw over
valid definitions occurs.

| Base | q1 before filters | Later ordinary bounds |
| --- | --- | --- |
| Bishop81 | 82..84 | q2:83..85; q3:84..86; q≥4 all exceed84. |
| Sniper62 | 63..65, none excluded | q2:64..66; q3:65..67; q≥4 all exceed65. |
| Skeleton17 | 17..22 | q4:32..37, with32 excluded and36–37 out; q≥5 no in-bound result. |
| Demon45 | 46..48 before unsafe scan | q5:50..52; q6:51..53; q7:52..54; q≥8 all exceed52. |

For all these ordinary q ranges, later byte candidates cannot wrap back into
their valid positive bounds. Airborne's `excludedEnemies` count0 becomes
DBF counter-1: it scans up to65,536 following bytes unless a match intervenes.
A match takes retry/base fallback; exhaustion can accept. The comment claiming
failure does not prove safe termination or base selection. No surrounding-byte
census or natural repeat roster is asserted.

Calls occur128–143 in row order; their draws, retries and unsafe reads affect
later seeds. A returned definition replaces stats, gear, spells, resistance,
prowess, spellPower, mover and priority. Row position, AI, orders, regions,
item request and STARTING/filler survive. Use its actual inventory: a full
copied loadout can reject AddItem; initialization does not test its failure
before the equipped-bit check. The selected original row words lack equipped
bit7 even on that failure, so they do not initiate an equip scan. No universal
repeat stats, item slot, drop entitlement or guaranteed successful upgrade follows.

## Three regions and 72 signed projection substitutions

**Confirmed:** all three type4 records have byte1=0 and trailing0/0.
Point0 is declared `(25,7)`, separate from every follow order.

| Region /persistent flag | Ordered vertices1,2,3,4 |
| --- | --- |
| 0 /90 | `(9,18),(13,29),(31,29),(31,18)` |
| 1 /91 | `(19,0),(10,18),(31,18),(31,0)` |
| 2 /92 | `(0,0),(0,10),(10,17),(19,0)` |

`battleloop/triggerregions.asm:IsRegionTriggered` tests all30 allies with
positive signedHP and nonnegative signed-byte X/Y, without joined/active checks.
Type4 tries triangles(1,2,4), then(3,2,4); each triangle tests edges A→B,C→A,C→B.
Horizontal is chosen when |dx|≥|dy|. MULS/DIVS truncates toward zero:
`Yproj=Yb+trunc((Yb−Ya)×(X−Xb)/(Xb−Xa))`; vertical exchanges X/Y.
SUB.W borrow, rather than geometric orientation, controls side flags00/10.
Collinear/in-bounds returns01 and accepts immediately; out-of-bounds returns10.
Ordinary flags must equal the opposite vertex's flag. Bit0 exceptions skip
either early edge and reject at the third. Negative projection sets CCR=1,
yielding10 despite the11 comment. Selected arithmetic has no word overflow or
degenerate edge; no conventional polygon substitution replaces it.

The following first-rejection witnesses cover all72 substitutions. p is the
actual projected coordinate, t the template index. Flags are binary.

| Region /triangle | First rejection | Opposite-vertex witness | Projection formula and substitution check |
| --- | --- | --- | --- |
| 0 /(1,2,4) | edge1→2: template00 versus reference10 | vertex4 `(31,18)`: Xproj9 | `Xproj=13+trunc(4×(Y−29)/11)`; Y18→9,19→10. |
| 0 /(3,2,4) | edge4→2 after two equal00 tests: template00 versus10 | vertex3 `(31,29)`: Yproj18 | `Yproj=29+trunc(11×(X−13)/−18)`; X4→34,3→35,2→35,1→36. |
| 1 /(1,2,4) | edge1→2: template00 versus10 | vertex4 `(31,0)`: Xproj19 | `Xproj=10+trunc(−9×(Y−18)/18)`; Y16→11,17/18/19→10. |
| 1 /(3,2,4), t0/2/3/4/6/11 | first edge3→2: template10 versus00 | vertex4 `(31,0)`: Yproj18 | Y18 is collinear but X1–4 is outside10..31; Y19 is below18. |
| 1 /(3,2,4), t1/5/7/8/9/10 | edge4→2 after two equal00 tests: template00 versus10 | vertex3 `(31,18)`: Yproj0 | `Yproj=18+trunc(18×(X−10)/−21)`; X3/2→24,1→25. |
| 2 /(1,2,4) | edge4→2 after two equal10 tests: template10 versus00 | vertex1 `(0,0)`: Yproj10 | `Yproj=10+trunc(10×X/−19)`; X4→8,3/2→9,1→10. |
| 2 /(3,2,4) | first edge3→2: template10 versus00 | vertex4 `(19,0)`: Yproj23 | `Yproj=10+trunc(−7×X/−10)`; X4/3→12,2→11,1→10. |

The complete per-template rejection projections, in those six triangle columns,
make the repeated substitutions explicit. R1b entries marked e1 reject early;
the others reject e3. All other columns use the edge named above.

| t /position | R0a X | R0b Y | R1a X | R1b Y /edge | R2a Y | R2b Y |
| --- | --- | --- | --- | --- | --- | --- |
| 0 `(4,18)` | 9 | 34 | 10 | 18 e1 | 8 | 12 |
| 1 `(3,17)` | 9 | 35 | 10 | 24 e3 | 9 | 12 |
| 2 `(3,18)` | 9 | 35 | 10 | 18 e1 | 9 | 12 |
| 3 `(2,18)` | 9 | 35 | 10 | 18 e1 | 9 | 11 |
| 4 `(3,19)` | 10 | 35 | 10 | 18 e1 | 9 | 12 |
| 5 `(2,17)` | 9 | 35 | 10 | 24 e3 | 9 | 11 |
| 6 `(2,19)` | 10 | 35 | 10 | 18 e1 | 9 | 11 |
| 7 `(2,16)` | 9 | 35 | 11 | 24 e3 | 9 | 11 |
| 8 `(3,16)` | 9 | 35 | 11 | 24 e3 | 9 | 12 |
| 9 `(1,16)` | 9 | 36 | 11 | 25 e3 | 10 | 10 |
| 10 `(1,17)` | 9 | 36 | 10 | 25 e3 | 10 | 10 |
| 11 `(1,18)` | 9 | 36 | 10 | 18 e1 | 10 | 10 |

Thus all three fresh regions miss for a coherent living placement subset.
Fewer living members use a prefix of this same template stream. Moved/saved
positions or a living placed reserve among all30 slots require new premises.

At @Start, ActivateEnemies scans128–159, requiring X≥0/HP≠0/enemy identity.
Fresh128–143 qualify. Its missing RTS falls through for160; the BHI combatant
bound admits that index, aliasing storage64 at `$FFF600` CURRENT_GOLD/deals.
The [alias owner](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
retains pseudo-X`$FFF62E`, HP`$FFF60E`, triggers`$FFF636`, activation`$FFF634`.
Eligible pseudo-state plus held non15 trigger flags can OR Goddess Staff count
bits; this is not an unconditional staff award. Fresh cleared flags/misses
supply no held trigger for such a write.

TriggerRegionsAndActivateEnemies tests each region once per tested-word
lifetime. A miss also sets NEWLY_TRIGGERED_BATTLE_REGIONS; a hit sets F90/91/92.
Fresh flag clearing and ClearAiMemory's two48-byte arrays do not clear this
tested word. Supplied tested bits can skip initial tests; with coherent fresh
misses, F90–92 remain clear either way. Normal non-SWARM StartAiControl clears
the tested word before standby/active handling; persistent flags survive. This
is a service lifetime, not an assumed per-round reset.

Primary triggered region wins and ORs bit0, preserving any old secondary bit.
Only absent/untriggered primary allows secondary to OR bits0+1. Thus F90 can
activate132/133 secondarily when F91 is clear, F91 can activate137/138 secondarily
when F90 is clear, and F92 can activate142/143 secondarily when F91 is clear.
NONE secondary orders do not suppress these writes. Both flags set chooses
primary without clearing a sticky secondary bit; no order swap happens here.
134/135/136/139/140/141 are15/15 and bypass assigned-region activation/standby;
15 is a sentinel, not a polygon. At fresh misses, the other ten remain inactive.
Region script dispatch has no selected24 program; the actual spawning scan's
`$0F00` mode test rejects STARTING0 and absent mode0. No hidden spawn list or
performed crossing is established. Changed saved modes remain conditional.

## Orders, standby and complete command streams

**Confirmed:** no selected set is SWARM. Inactive assigned128/129/130/143
have non-NONE primaries with a declared region: ValidateAiStandbyEligibility
returns nonzero, which its caller treats as **no standby move**, despite the
comment's polarity. Its initial thinking-RNG range8 occurs before validation.
Inactive131/132/133/137/138/142 have NONE primary and a declared region and
can construct local standby movement around their source starting position,
subject to RNG, AI memory, terrain, occupancy and path/station success.
All return STAY action; that does not imply an empty movement string.
The six15/15 actors reach ordinary streams despite clear active bits.

`battlefield/getmoveorderposition.asm` uses bit6 before the low-nibble point
lookup: Bishop128 reads declared point0 `(25,7)`, coincident with its base
position but not an immutable standing command.129/130 follow live128,
135/136 follow live134 initially`(17,18)`,141 follows live140 initially`(9,25)`.
Raw0 on143 follows live ally0/Bowie, neither point0 nor enemy128. Logical
IDs, source positions and physical entity slots remain distinct.

Normal active handling tests any non-NONE/non-MOVE_TO primary's targetHP
before every commandset. HP0 promotes secondary and clears secondary; here
all secondaries are NONE. This includes HEALER3 follows and set6 in a changed
state even though set6 has no MOVE_ORDER. Inactive standby precedes this
promotion. Nonzero targetHP alone proves neither valid position nor path.

`global/aicommandsets.asm` and its pathfinding table give complete streams:

| Set /selected combatants | Preparatory mode | Complete ordered stream |
| --- | --- | --- |
| 2 HEALER3 /135,136 | BLOCK_NON_MOVABLE1 | HEAL2, MOVE_ORDER4, SUPPORT, ATTACK3, MOVE2, STAY |
| 6 ATTACKER1 /131–134,137–140,142 | REGULAR0 | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY |
| 7 ATTACKER2 /143 | REGULAR0 | MOVE_ORDER1, ATTACK1, HEAL1, SUPPORT, MOVE1, STAY |
| 9 ATTACKER3 /129,130,141 | BLOCK_NON_MOVABLE1 | MOVE_ORDER3, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER4, MOVE2, STAY |
| 14 LEADER /128 | BLOCK_AND_CARVE2 | MOVE_ORDER5, ATTACK2, HEAL3, SUPPORT, MOVE_ORDER2, MOVE3, STAY |

Set14 aliases13's stream. ExecuteAiCommand stops at first d1=0; earlier
MOVE_ORDER/ATTACK/HEAL success prevents later commands. Top-level exit clears
temporary obstructions. MOVE_ORDER1/2/3/4/5 pass targetType/pathMode0/0,2/2,
1/1,0/1,0/2. HEAL2/3 are command parameters, not learned ranks. MOV0/NONE/dead
target reject ordered commands; type0's station check tests returned X byte,
so X0 fails while-1 can continue. Ordered movement can attack internally or
move-and-STAY. Prepasses and command-local restrictions still need actual
order, last-target state, terrain, occupancy, current MOV and path success.
LEADER's center uses real point0; its carving radii depend on last-target state.
Set6's ordinary ATTACK1/MOVE1 does not implement a follow just because an order
is stored. Outcome leader-presence slot24 is0: LEADER14 does not make Bishop128
a leader-cleanup victory condition. No fixed pursuit, boss action or tactic follows.

## AURA1, BLAST3, BLAZE3 and BOOST2

**Confirmed:** `data/stats/spells/spelldefs.asm` supplies these exact keys;
all listed spells have AFFECTEDBYSILENCE. Healing/support target teammates;
attack spells target opponents.

| Learned spell /full key | MP | Range /radius | Power |
| --- | --- | --- | --- |
| AURA1 `$01` | 7 | 0–3 /1 | 15 |
| BLAST3 `$8E` | 8 | 1–2 /1 | 12 |
| BLAST2 `$4E` /BLAST1 `$0E` | 5 /2 | 1–2 /1;1–1 /0 | 8 /6 |
| BLAZE3 `$8B` | 10 | 1–2 /1 | 15 |
| BLAZE2 `$4B` /BLAZE1 `$0B` | 6 /2 | 1–2 /1;1–2 /0 | 10 /6 |
| BOOST2 `$43` | 5 | 0–2 /2 | 15 |

GetNextUsableAttackSpell masks the base, skips NOTHING and requires TYPE_ATTACK.
Bishop skips AURA then finds BLAST3; Demon finds BLAZE3. Both bases belong to
the confused/ally BLAZE/FREEZE/BOLT/BLAST/KATON/RAIJIN whitelist. Actual confusion
is IsCombatantConfused's MUDDLE/MUDDLE2 combination, not every preparation
MUDDLE branch. GetHighestUsableSpellLevel reconstructs `(rank<<6)+base` from
learned rank downward and unsigned-compares currentMP with definition cost.
BishopMP38 and DemonMP21 admit rank3; depletedMP can choose known lower ranks
or NOTHING. No learned rank, power byte or metadata forces a cast.

`command/attack/prioritizetargets.asm` builds separate item/spell/physical
reachable lists using movement arrays, actual ranges, target/resistance,
difficulty, priority and last-target state. Getter/list calls preserve the
full returned spell word written to ATTACK_COMMAND_SPELL. Neither `$8E` nor
`$8B` equals the exact AQUA1 `$28` shortcut in DetermineBattleactionForAttackAiCommand.
With spell and physical categories and no item, main range6 chooses spell on2/4,
physical otherwise. Spell-only still draws then selects spell; physical-only
skips that draw. With viable item **and** spell it uses thinking range2 (1 item,
0 spell); item versus physical uses main range6 (3/5 item, otherwise physical),
with item-only fallback after the draw. No categories returns STAY. Additional
priority/tie/class/movement and station/path consumers still choose the target.
`command/attack.asm` writes CAST_SPELL/actual key and constructs movement from
the selected spell range; an earlier order command can reach internal ATTACK1.

### AURA's actual healing selection

**Confirmed:** `command/heal.asm` rejects confusion, tries special Healing Rain,
then learned TYPE_HEAL and finally ordinary usable healing items. Bishop's
first healing spell is **AURA1**, not HEAL3. Its explicit AURA gate requires
currentMP≥7 (HEAL's gate is3). No base actor is initially injured.

The command builds movement arrays and living friendly candidates satisfying
DoesCombatantRequireHealing's `3×HP≤2×maxHP`. For each candidate it calls
PopulateTargetsList with full`$01`: AURA's radius1 can include multiple
friendlies. It sums each affected actor's CalculateHealTargetPriority+4,
sorts descending retaining ties, then tries candidate stations in that order.
Actual commandsets13/14 receive13, so leader128 has13; other base mover
priorities from `aipriority.asm` are Bishop12, Sniper10, Skeleton2, Demon6.
This area scoring precedes the per-candidate rank/range decision; it is not
a simple lowest-HP or single HEAL target rule.

DetermineHealingSpellLevel reads the **current local** spellEntry. For incoming
AURA1`$01`, rank0 cannot enter HEAL3/4 rank promotion: missingHP≤2 returns-1;
larger deficits select d2=0. Its shift5/unmasked-key calculation therefore
tests exactly`$01` cost7. With MP≥7 it returns0; the caller's nonself promotion
also requires incoming rank≥2 and cannot promote this AURA1. Repacking writes
`$01` again. A failed station followed by another candidate retains`$01`,
unlike the accepted mutable HEAL3 sequence. The command uses AURA range0–3
for DetermineAttackPosition, then builds the path and writes CAST_SPELL plus
that candidate. Healing-item paths instead use item range/actual slot.
Area priority, a valid station, earlier stream success and later dispatch are
separate from performing a heal; supplied/repeat higher AURA ranks need their
actual local-key path, not this rank0 derivation.

### BOOST name versus reachable SUPPORT

**Confirmed:** GetNextSupportSpell can return learned BOOST2`$43` as TYPE_SUPPORT.
But `command/support.asm` first requires an enemy/nonconfused caster, then
compares the **full word** only with MUDDLE2`$47` or DISPEL`$06`. BOOST2 fails
that early gate, writes STAY and returns failure before MP/targets/station.
The later BOOST2 prioritizer and ATTACK branch exist but are unreachable for
this selected ordinary call. AffordableMP21, teammate targeting and its name
do not establish automatic BOOST. Bishop has no learned support spell.

At the later effect seam `initbattlesceneproperties.asm` gates spell silence
for CAST_SPELL. `castspell.asm`, `calculatespelldamage.asm` and GetResistanceToSpell
retain actual recovery/damage/resistance; PerformAnimation charges actual
spellMP. AdjustSpellPower reads class, which the copied ENHANCED99 supplies,
and multiplies non-full spell power by5/4 for CAST_SPELL before later caps/
resistance. AURA's base15 can become18 before missingHP cap; this is conditional
effect arithmetic, not healing completed. Items keep USE_ITEM and bypass
that CAST_SPELL enhancement. No full damage, critical/double/counter or visible
animation audit is included; complete prowess metadata is not performed behavior.

## Equipment, masked items and independent drops

**Confirmed:** complete relevant `data/stats/items/itemdefs.asm` entries:

| Item | Equip /item range /price /type /use | Three equipment effects |
| --- | --- | --- |
| Herb0 | NONE /0–1 /10 /CONSUMABLE /HEALIN1 | NONE0,NONE0,NONE0 |
| Blizzard17 | NONE /0–1 /1200 /RARE\|CONSUMABLE /BREZAD1 | NONE0,NONE0,NONE0 |
| Flail90 | WIZ/SORC/VICR /1–1 /1490 /WEAPON /NOTHING63 | ATT+19,NONE0,NONE0 |
| Hyper Cannon53 | SNIP/BRGN/BWNT /2–3 /8700 /WEAPON\|RARE /NOTHING63 | ATT+40,NONE0,NONE0 |
| Broad Sword62 | HERO/BRN/BDBT/NINJ/RDBN /1–1 /1600 /WEAPON /NOTHING63 | ATT+22,NONE0,NONE0 |

Physical GetAttackRange reads equipped gear: Bishop/Skeleton1–1, Sniper2–3;
unarmed Demon defaults1–1 while its AI physical station branch uses0–1.
These are separate from spell and item ranges. Neither rank nor spell power
changes a weapon's range.

InitializeEnemyItems skips exact NOTHING. AddItem finds the first slot whose
index&127 is127, then masks successful input by`$807F`, stores/returns it and
sets d2=0. Thus authored herbs`$2000`/`$6000` become`$0000`; row135 Blizzard
`$6011` becomes`$0011`, removing AI-use bit13 and unused-drop bit14. Base copied
weapons stay equipped in slot0 (`$DA` Flail,`$B5` Cannon,`$BE` Sword);
additions use slot1. Returned bit7 is clear, so the caller does not equip them.
Repeat copied inventory/capacity can change both getter order and successful addition.

`itemstats.asm:IsItemUsableInBattle` wrongly rejects useSpell255, not63, so
equipped Flail/Sword/Cannon can reach GetNextUsableAttackItem. It accepts
equipped/AI-use words before resolving useSpell; their NOTHING63 falls back
to first HEAL1 in GetSpellDefinitionAddress, fails TYPE_ATTACK and returns
NOTHING **without advancing to slot1**. Fresh Blizzard also lacks both
permission bits. Restoring its bit13 alone does not bypass the earlier Flail.
With an inventory that actually reaches an allowed Blizzard, BREZAD`$26` is
TYPE_ATTACK, MP0, range1–1/radius0/power33, FREEZE|VARIATION3, no silence flag.
Ordinary enemy selection can admit it; confused/ally item selection's four-base
BLAZE/FREEZE/BOLT/BLAST whitelist excludes BREZAD. Fresh Herb is neither Rain
nor AI-enabled and fails healing-item selection. HEALIN`$10` is MP0,
TYPE_HEAL|TARGET_TEAMMATES, range0–1/radius0/power10, no silence flag.

If permitted healing Herb is reached, Heal uses friendly deficit/priority,
item0–1 station/path and actual slot; it does not inherit learned AURA rank.
If Blizzard is actually admitted, attack item targeting and station selection
use its use spell BREZAD's range1–1, not the item's declared0–1. The item branch
also records the actor itself in last-target memory after overwriting d0 during
inventory lookup, rather than the chosen target. A USE_ITEM command is not an
executed effect. UseItem unpacks
the use spell then branches CastSpell; PerformAnimation chargesMP only for
CAST_SPELL and the initial silence check distinguishes that action too.
After the target loop, BreakUsedItem removes the **actual used slot** for
nonequipment Herb/Blizzard before the enemy-user exemption for breakable
weapons/rings. Neither RARE nor CONSUMABLE supplies break RNG, a spare copy
or an enemy consumable exemption. Early inaction/no-target or unfinished services
do not prove consumption/effect.

`global/enemyitemdrops.asm` independently has exactly three selected24 records:

| Target row /combatant | Required currently held index | Once flag /bank bit |
| --- | --- | --- |
| 7 /135 | BLIZZARD17 | 9 /byte1 bit1 |
| 5 /133 | MEDICAL_HERB0 | 20 /byte2 bit4 |
| 12 /140 | MEDICAL_HERB0 | 21 /byte2 bit5 |

`battleactions/dropenemyitem.asm` is reached after target effects, including
double/counter contexts with their actual actor/target. It requires ally actor,
enemy target, local targetDies, matching battle/slot and a held index found
with mask127. Table shape, HP0 elsewhere, authored UNUSED_ITEM_DROP or a
leader sweep grants nothing. All three selected indices branch directly to
DropItem with **no drop RNG**; only110/102/68 take range32/zero before once
checking. The subsequent upgradable-battle/range3 code is unreachable.

DropItem BSETs the once bit first and returns if already set. Otherwise it
removes/compacts the target slot and refreshes stats, then checks actorHP;
living actor receives an AddItem attempt into its first free slot. Dead actor
or full inventory takes rare-deals fallback. Blizzard is RARE: deals nibble
increments unless already15. Herb is not RARE and is discarded. No alternate
ally is chosen. Once/removal can happen without receipt or deals increase.
The four-byte ENEMY_ITEM_DROPPED_FLAGS bank at`$FFF7A4` is separate from story
F9/F20/F21 and F524; fresh region/AI initialization does not reset it. Its RAM
lifetime is not save persistence. Repeat substitutions, lost/used items,
prior once flags, actual killing actor and completed services remain conditions.

## Actors, rounds and first conditional control

**Confirmed:** LoadBattle/PositionBattleEntities replace field population,
clear49 physical records/64 lookup bytes and scan allies0–29 then ordinary
enemies. Nonnegative X/Y admits actors without an independentHP guard; neutral
enemies skip that ordinary branch. With N declared allies, fresh128–143 map
to physicalN..N+15. Base mapsprites Bishop145/Sniper126/Skeleton81/Demon109
are below240, using ordinary slots. Follower logical identities and Janet's
old actor do not survive as the same combat population. No24 neutral group
exists; F524's neutral scan has no matching entry and F523 is not its gate.
No new158/159 neutral is declared, and saved neutral fields are not repaired.

`battleloop/turnorderfunctions.asm` clears64 sentinel entries, scans30 allies
then128–157 and admits X≥0/HP≠0 independently of activation. Unchanged fresh
enemies supply sixteen entries with orderedAGIs
`27,35,30,32,35,30,30,27,30,32,32,30,30,35,32,32`; none has bit128.
Ordinary score is `A+RNG(A>>3)−RNG(A>>3)+RNG(3)−1`. CurrentAGI≥128 would add
an extra base `floor((AGI&127)×5/6)` scored with two draws; it is independent
of LEADER14/outcome presence. Range0 still advances main seed. Stable signed-byte
descending sort preserves append-order ties and sets cursor0. Actual allies,
gear/status/repeat definitions and earlier RNG determine order; inactive
enemies still participate. No first actor or player-ready state is fixed.

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
No selected24 content is produced, trusted, admitted or observed here.

| Actual owner | Reusable mechanism and selected24 limit |
| --- | --- |
| [prepare_encounter /compile_routes](../../src/sf2tool/remake_exploration_battles.py), [scene producer](../../src/sf2tool/remake_battle_scene_content.py) | Encounter preparation admits2/3; scene build1/2/3. Wildcards,400/450/500 flags and before/load/start phases exist; no24 route/request/export conversion or actual field party is admitted. Departure/cave/outcome conversions stay with their owners. |
| [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [PrivateBattleDefinitionReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) | Ordinal definitions, complete gear/spell/resistance/prowess metadata, regions and points can describe the four bases. Trusted pins admit1/2/3 and reject24. Describing81/62/17/45 is not execution or a new-name infrastructure requirement. |
| [PrivateBattleScenarioReader.Assemble](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs), [PrivateBattleActionBindings](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Independent region-program admission1/2/3 rejects24 despite no24 program. STARTING, NONE/point/live and6/7/9/14 bind generically; HEALER3/2 rejects. Placement allowlist accepts NOTHING or exact Herb6000, rejecting Herb2000/Blizzard6011. All four ENHANCED definitions reject REGULAR-only state. BIT4 rejects Sniper/Demon; Skeleton/Bishop AI expressions fit descriptively. Bishop ordinary prowess fits; the other three counter/double/critical expressions reject. HEAL-only learned binding does not execute AURA/BLAST/BLAZE/BOOST. Flail/Sword adjacent ATT-only WEAPON metadata fits the physical binding; Cannon WEAPON\|RARE/2–3 fails. |
| [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) | Membership/resources/seeds, living/immortal healing, formation, difficulty0 ATT and activation are reusable. Excess members reject instead of source eviction; actual party/classes beyond0/1/2/4, gear/final-CURSE refresh and other difficulty/control remain limited. Enemy equipment accepts only0/127, rejecting copied weapons/Blizzard. No F524 effective-level/category/retry/unsafe scan substitution exists. F88 rejects rather than restoring supplied queue/cursor. |
| [BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs), [BattleControlRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) | Four-vertex regions, primary-first/sticky-secondary,15/15 and ordinary/extra randomized turns exist. Geometry uses cross products instead of signed projection/CCR, BeforeRound resets tested state locally instead of preserving the source service lifetime, and no160 deals alias is represented. Fresh misses alone cannot establish later/saved equivalence or actual24 control. |
| [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs), [SourceMoveOrderAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs), [BattleTerrainRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTerrainRules.cs) | Standby, normal cache reset, dead-follow promotion,6/7/9/14, valid point0/live0/128/134/140, LEADER prepass and Flying/Regular/Healer are reusable. Content's Mover switch admits STEALTH but **not STEALTH_ARCHER**; Domain has no corresponding Archer mover, and generic Stealth costs alone do not bind Sniper. Activated AI rejects set2, nonzero status, automatic allies and loadouts beyond0/127/no learned spells. Exact healing/offensive/item selection, AURA area/local-rank, BOOST early rejection and weapon ranges need actual conversion. No SWARM platform is selected. |
| [Physical action](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2PhysicalAction.cs), [ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), Application/entity and adapter boundary | Generic physical/reward/control and logical projection do not implement selected slot135/133/140, actual held17/0, once9/20/21, consumption/remove/receipt/deals, full resistance/prowess or source actor/service/presentation lifetime. No24 adapter observation, natural input, suspend or persistence acceptance exists. |

These are conversion requirements over actual valid states and content, not
widening-only admission edits or a mandate for speculative infrastructure.
Preserve generic support while resolving the exact missing behavior when
implementation is authorized.

## Unknowns, retained failures and reproduction

**Unknown:** natural route/flags/party/classes/gear/status/seeds, actual repeated
definitions/inventory, supplied tested state/deals alias, moved/saved regions,
standby/targets/stations/paths, AURA/BLAST/BLAZE/item choices, BOOST effects from
other callers, damage/prowess/follow-ups, first actor/input/actions, death
actor/loot/deals, completed map/scroll/entity/text/animation/sound/VInt services,
presentation/hardware and persistence. No Unknown is an automatic runtime queue.

Preserve [#1047's twelve completed unresolved comparisons](../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03),
the [control correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415),
intermediate/final-CURSE, [Battle12 HEAL/identity](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1234)
and [Battle13 neutral](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1236)
findings/original failures. [PR1245's P2](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1245#issuecomment-6366643680)
retains initial16f4233a mechanical PASS versus acceptedc1893c7b candidate-local
HEAL correction. [PR1251's record](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1250#issuecomment-6368359671)
retains initial70fbb37c mechanical PASS versus accepted425d504e after-position44→5.
[PR1253's record](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1252#issuecomment-6368882658)
retains initial6dfd61b0 mechanical PASS versus acceptedf171630a projection
formula numerator−15→+15. That was a prose formula failure, not a failed
geometry result. Completed failures are not interrupted or semantic passes.

Reproduce by joining complete spriteset24/macros/enums, definitions81/62/17/45,
actual map/hook/global tables and the direct consumers named above. Check
378bytes/28 records/three exact regions/point0, every field and all72 triangle
substitutions with first-rejection traces. Substitute the displayed formulas
against those results independently. Follow tested versus persistent flags,
primary/secondary/NONE/15 semantics, all selected streams/prepasses, repeat
category/ranges/defaults/unsafe scan and actual copy/item/stat order. Follow
full spell keys through usability/category, AURA priority/local rank/station,
BOOST's early gate, masked inventory/getter termination and actual conditional
drop/remove/deals order. Review production support and rejection independently.

Direct checks in `local/issue1254` cover UTF-8/LF, links/anchors/tables/private
boundary, exact five paths, unchanged retained metadata/fixture metrics/four
frontiers, selected source and arithmetic. On the clean committed head load
ignored `local/private-inputs.ps1` in the same process and run
`uv run sf2 verify plan --base origin/main --head HEAD`; inspect actual exact-head
scoped CI. No normal/full/SDK/Godot/H3/H4/historical reruns belong to this
documentation acceptance. Freeze a clean pushed Draft PR, with checks,
failures and exact identities handed to independent main-gate review/merge.
