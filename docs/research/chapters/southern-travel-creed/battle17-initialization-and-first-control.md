# Battle17 initialization, repeat upgrades and first conditional control

Evidence date: 2026-10-22. **Confirmed:** the complete12/14/2/0 selection
connects actual party placement, six base definitions, repeated-entry selection,
two initially untriggered regions and live orders to first conditional control.
Arrow Launcher's BRASS_GUNNER selects the melee upgrade table; ordinary eligible
candidates fail its bounds and fall back. Gargoyle's FLYING path reaches the
unsafe count0 exclusion scan. Only135/136 have assigned region0; unassigned
region1 is still tested. **Inferred:** this describes the intended approach
encounter toward Taros's shrine. **Unknown:** natural route/state, substitutions,
party, first actor/action, service completion, presentation/timing and persistence.
This is static composition, not observed play or admitted production content.

## Agreement and provenance

This Standard-rigor agreement implements
[Issue1240](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1240)
under [Epic1053's finite scope](../../story-content-coverage.md#scope-and-reading-rules).
The user outcome is maintainable original content and future conversion review:
join every selected formation/definition/region/order to its actual initialization
and first-control consumer. Cross-owner party, repeated-definition and AI
lifetimes justify Standard rigor. Stop before a performed individual action.

The [admission/outcome owner](./hassan-departure-and-battle17-outcomes.md)
retains Hassan departure, walking-zone/random-request production, partial reload,
ordered admission and conditional victory/defeat/Egress returns. Do not duplicate
those scenes or the [Rohde choice](./hassan-ancestry-and-rohde-caravan-choice.md),
[raft/Luke](./battle16-outcomes-through-hassan-entry.md) and
[Luke join](../eastern-parmecia/polca-departure-through-luke-recruitment.md) owners. Reuse
[Battle09 repeat selection](../eastern-parmecia/battle09-initialization-and-first-control.md#f509-effective-level-and-actual-repeat-selection),
[Battle10 gear/drop distinctions](../eastern-parmecia/battle10-initialization-and-first-control.md),
[Battle12 healing AI](../eastern-parmecia/battle12-initialization-and-first-control.md),
[Battle13 actual party](../eastern-parmecia/battle13-initialization-and-first-control.md) and
[shared Battle02 lifecycles](../grans-opening/battle02-initialization-and-first-control.md).

Owned tracked writes are this document plus Battle17 consistency only in
`story-content-coverage.md` and `source-coverage.md`. There is no stacked or
shared-writer dependency; PR692 is unconsumed. Source, fixtures, schemas,
registries/counters, code, design and runtime frontiers remain read-only. No
parser/platform, allowlist, engine implementation, acquisition, comparator,
lowering, payload regeneration or bulk RNG census is added. Acceptance is complete
selected semantic/source/consumer review, direct document/link/private/scope
checks, clean committed planner and exact-head scoped CI, then a clean pushed
frozen Draft PR for independent main-gate review. No next slice is selected here.

Accepted repository base is `038f48fb5cf1021ed4fbb57aa202fcfb82ecd380`,
tree `0a5b95ed16466b704bdb6a7c3f4333e40e3ed4e1`. Original paths below resolve
under `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin; the
[USA baseline](../../../../README.md#pinned-original-baseline) retains ROM identity.
No fresh ROM-equivalence or runtime claim is made. Short battle-code paths below
have prefix `code/gameflow/battle/`; sibling filenames retain their preceding
directory. `j_` aliases resolve to named implementation bodies. Opcodes, operand
values and actual return/branch polarity control over comments and family names.

| Claim | Selected declaration and actual consumer |
| --- | --- |
| Complete12/14/2/0 | `data/battles/spritesets/entries.asm` slot17 → `spritesets/spriteset17.asm:BattleSpriteset17`, `0x1B43F2..0x1B4546`; `sf2macros.asm:allyCombatant/enemyCombatant/combatantAiAndItem/combatantBehavior`, `sf2enums.asm`; `getbattlespritesetsubsection.asm:GetBattleSpritesetSubsection`, `0x1B1630..0x1B169E`. |
| Selection/globals/hooks | `code/common/maps/getbattle.asm:CheckBattle`, `0x799C`; `code/gameflow/mainloop.asm`; complete `data/battles/global/battlemapcoords.asm`, `custombackgrounds.asm`, `enemyleaderpresence.asm`, `halvedexpearnedbattles.asm`, `afterbattlepositions.asm`, `battleneutralentities.asm`; terrain slot17 → `battleloop/loadbattleterraindata.asm`; `battlescenes/getbattlescenebackground.asm`; before/start table slots17 under `data/battles/cutscenes/` and their wrappers. |
| Actual party/refresh/load | `battleloop_1.asm:BattleLoop`, `0x23A84`; complete `battleloop/initializecombatants.asm`, `0x1B1272..0x1B15F8`; `heallivingandimmortalallies.asm`; `code/common/stats/battleparty.asm:UpdateForce/JoinForce/JoinBattleParty/LeaveBattleParty`, `updatecombatantstats.asm`; `battlefunctions/loadBattle.asm` → `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities`. |
| Six full bases/gear/spells/drop | `data/stats/enemies/enemydefs.asm` records9/11/43/60/69/78; `data/stats/items/itemdefs.asm` Steel Arrow47/Iron Rod88; `data/stats/spells/spelldefs.asm`; `code/common/stats/itemstats.asm`, `spellstats.asm:GetSpellDefinitionAddress`; complete `data/battles/global/enemyitemdrops.asm` → `battleactions/dropenemyitem.asm`. |
| Repeat selection | Complete `battleloop/upgradeenemies.asm`, `0x1B1780..0x1B1A28`; `data/battles/global/randombattles.asm`, `0x1B6DB0..0x1B6DDA`; `sf2macros.asm:battles/upgradeRange/excludedEnemies`; `code/common/tech/randomnumbergenerator.asm:GenerateRandomNumber`. |
| Regions/standby/orders | Complete `battleloop/activateenemies.asm`, `triggerregions.asm`, `0x1AC9FC..0x1ACF30`, `clearaimemory.asm`, `populateenemyspawns.asm`; code/data `cutscenes/regionactivatedcutscenes.asm`; `code/common/stats/combatantstats_1.asm:GetMoveOrders/GetTriggerRegions`, `_2.asm:SetTriggerRegions`; `battlefield/getmoveorderposition.asm`; `ai/startaicontrol.asm`, both `determineaistandbymovement_*.asm`. |
| Commands/round/control | `data/battles/global/aicommandsets.asm`, `0xE1AC..0xE25B`; `ai/executeaicommand.asm`, `command/moveorder.asm`, `command/moveorder/buildmovestringformoveorder.asm`, `command/attack.asm`, `command/move.asm`, `command/heal.asm`, `command/heal/determinehealingspelllevel.asm`, `command/support.asm`, `getnextuseableaiaction.asm`, `gethighestusablespelllevel.asm`; `battleloop/turnorderfunctions.asm`; `battlefunctions/executeindividualturn.asm`, `0x23EB0..0x24242`. |

The [spriteset](../../battle/battle-spriteset-data.md), [routing](../../battle/battle-routing-data.md),
[global](../../battle/battle-global-data.md), [loop](../../battle/battle-loop.md), [AI](../../battle/battle-ai.md)
and [cutscene](../../battle/battle-cutscenes.md) fixtures retain their existing boundaries.
`rowContentTracked:false` and `routeTargetsParsed:false` remain unchanged;
this prose adds no executable Battle17 fixture or indexed count.

## Ordered Map69 selection and entry lifetimes

**Confirmed:** `BATTLE_TO_TAROS_SHRINE=17` selects Map69 with output crop
`(0,31,30,32)` and wildcard triggers255/255. CheckBattle checks ordered row9
before17: supplied F409 wins, including with F509 set. With409 clear and417 set,
row17 can qualify independently of coordinates, F418/Rohde, raft or membership.
The crop is an output, not a containment/admission predicate. Completion F517
clears417 on an otherwise qualifying scan but **still returns17 on that call**.
F467 intro and F88 suspension have separate consumers. Numbered battle order
does not establish a natural route. The admission owner retains the complete
walking-zone versus actual raft producer and partial reload semantics.

Terrain slot17 selects `BattleTerrain17`, decoded into shared48×48 storage,
distinct from the30×32 crop. Background17 is OVERWORLD255: its consumer uses
map blocks rather than a fixed custom background. Leader-presence17 is0;
neither Soulsower128 nor set7 creates a leader sweep. Half-EXP selects only1,
post-battlescene position override only5, and the neutral table has no17 entry.
No selected neutral or Caravan actor is added at LoadBattle.

| Supplied state | Actual entry work |
| --- | --- |
| F88 clear/F467 clear | Clear PLAYER_TYPE/set399; clear seconds; save/install map/battle/base VInts; call empty before17; restore/reassert fields; clear90–105; heal; initialize actual allies and enemies including conditional upgrades/refresh; clear AI memory; `LoadBattle(0)`; start wrapper sets467 before its empty body; prepare round. |
| F88 clear/F467 set | Skip both hook bodies; fresh healing, placement, repeat selection, refresh/load and new round still run.517 remains independent. |
| F88 set | After mode0/set399, restore saved seconds, clear88/AI memory/view target, LoadBattle with incoming focus, then retained individual-turn queue/cursor. No fresh field installation, region reset, healing, formation, upgrade, before/start or new round precedes this loop. A reached sentinel later prepares a round. |

Both slot17 table targets are `ms_Empty`, not missing bespoke scene files.
Before only tests467; start tests/sets467. The shared terminator can still wait
for scrolling and clears scroll speed. Neither hook writes EGRESS_MAP, joins
Peter/Luke/Rohde, changes their AI or installs followers. A stored egress map
remains supplied to the savepoint/F399 consumer. F467-set entry is not suspension.

`LoadBattle` rebuilds physical entities/lookups from actual combatant positions
and mapsprites, then loads map/sprites/terrain/music/fade services. World
coordinates add the crop origin: template0 local`(24,23)` means world`(24,54)`,
not Hassan's posted`(26,54)` or the reload's residual X0. Local enemy128`(1,10)`
means world`(1,41)`. Earlier field/follower actors are not preserved roster
identities. Saved map/battle/area/roster/queue must agree on F88 entry; it does
not repair them or explicitly supply fresh d0=0. Service completion is Unknown.

## Twelve templates versus the actual party

**Confirmed:** header4 +26 combatants×12 +2 regions×12 +0 points×2 =340
(`0x154`), matching exclusive end`0x1B4546`. Every ally template declares
HEALER1/0, NOTHING127, primary/secondary NONE255 with regions15/15, filler0
and STARTING0. Complete local coordinates in source order are:

| Template | X,Y | Template | X,Y |
| --- | --- | --- | --- |
| 0 | `(24,23)` | 6 | `(25,22)` |
| 1 | `(24,24)` | 7 | `(26,22)` |
| 2 | `(24,22)` | 8 | `(26,23)` |
| 3 | `(24,25)` | 9 | `(27,23)` |
| 4 | `(25,24)` | 10 | `(27,22)` |
| 5 | `(25,23)` | 11 | `(26,21)` |

Initialization resets all30 ally Xs to-1, rebuilds ascending joined-and-active
lists from flags0–29/32–61, and copies **only X/Y** to actual living members.
Template IDs/AI/gear/spells do not install twelve fixed heroes. A dead ordinary
member consumes a list/loop iteration without advancing the placement cursor.
Twelve iterations bound capacity even when deaths leave positions unused;
remaining active members enter LeaveBattleParty (active/X cleared, joined kept),
without another list rebuild. The zero-template guard is not an empty-party
pre-loop guard. General invalid/supplied state is not normalized here.

`JoinForce` sets joined/rebuilds lists, then calls JoinBattleParty only below12
active members. At capacity a new inactive member stays reserve; an already-set
active bit remains. Direct JoinBattleParty has no such guard. Joining neither
revives ordinary dead allies nor establishes placed/queued/control state.

Healing scans all30 independently of membership: living allies plus immortal
Peter7/Lemon28 receive max HP/MP; other dead members remain dead. Intermediate
`status & 7` retains STUN/POISON/CURSE. Subsequent UpdateCombatantStats preserves
STUN/POISON and derives **final CURSE from equipped cursed gear**. Old CURSE alone
is not final curse. Current equipment/class/status affects refreshed stats.

Peter's prior joined/active/AI/follower producers, Luke's independent join/follow
and Rohde's F418/following choice keep their accepted owners. None is an automatic
Battle17 member or a required template identity. Peter can be revived yet remain
reserve/follower-only; a deployed Peter's live bit2/MUDDLE/auto-battle still matters.
Rohde following does not establish joined19/active51. A visible follower or field
actor does not fill a combatant slot. Ally AI uses set6, not template HEALER1.
ClearAiMemory resets48 last-target/memory bytes, not combatant AI-control bit2.

## All fourteen enemy rows and six complete bases

**Confirmed:** all enemy rows add NOTHING127, have secondary order NONE255,
secondary region15, filler0 and STARTING0. Slots128–141 initialize immediately;
undeclared142–159 have max/currentHP0 and X=-1. Mode0 fresh initialization does
not use the later spawn-reset occupancy gate. Complete row differences are:

| Row / slot | Base type / local X,Y | Commandset | Primary order | Primary region |
| --- | --- | --- | --- | --- |
| 0 /128 | SOULSOWER11 `(1,10)` | ATTACKER2/7 | FOLLOW_TARGET\|1 (`$01`) | 15 |
| 1 /129 | MASTER_MAGE69 `(4,15)` | ATTACKER1/6 | FOLLOW_TARGET\|1 | 15 |
| 2 /130 | BLACK_MONK78 `(3,19)` | HEALER1/0 | FOLLOW_TARGET\|1 | 15 |
| 3 /131 | GARGOYLE43 `(6,6)` | ATTACKER2/7 | FOLLOW_TARGET\|0 (`$00`) | 15 |
| 4 /132 | GARGOYLE43 `(6,17)` | ATTACKER2/7 | FOLLOW_TARGET\|0 | 15 |
| 5 /133 | GARGOYLE43 `(13,9)` | ATTACKER1/6 | NONE | 15 |
| 6 /134 | ARROW_LAUNCHER60 `(16,10)` | ATTACKER2/7 | NONE | 15 |
| 7 /135 | GOLEM9 `(11,19)` | ATTACKER1/6 | FOLLOW_TARGET\|0 | 0 |
| 8 /136 | SOULSOWER11 `(9,21)` | ATTACKER1/6 | FOLLOW_TARGET\|0 | 0 |
| 9 /137 | MASTER_MAGE69 `(18,15)` | ATTACKER1/6 | NONE | 15 |
| 10 /138 | GOLEM9 `(17,20)` | ATTACKER2/7 | NONE | 15 |
| 11 /139 | BLACK_MONK78 `(14,22)` | HEALER3/2 | FOLLOW_ENEMY\|13 (`$8D`) | 15 |
| 12 /140 | ARROW_LAUNCHER60 `(15,23)` | ATTACKER2/7 | NONE | 15 |
| 13 /141 | GARGOYLE43 `(15,26)` | ATTACKER1/6 | NONE | 15 |

All six full definitions have spellPower REGULAR, initialStatus NONE and raw
unknownByte equal to their base type ID. This byte is not a separate interpreted
AI behavior. “Rest NOTHING” below fills all unused slots to four items/spells.

| Definition | Level; HP/MP; base ATT/DEF/AGI/MOV | Four-slot gear / spells | Movement; original AI word |
| --- | --- | --- | --- |
| Soulsower11 | 16;27/0;35/32/25/4 | all NOTHING / all NOTHING | REGULAR; PRIORITYMOD_2\|BIT5 (`$2020`) |
| Master Mage69 | 14;28/14;11/18/22/5 | Iron Rod88 equipped, rest NOTHING / BLAZE2, rest NOTHING | MAGE; PRIORITYMOD_2\|BIT4 (`$2010`) |
| Black Monk78 | 14;30/19;27/18/21/6 | all NOTHING / HEAL2, BLAST1, NOTHING, NOTHING | HEALER; PRIORITYMOD_3 (`$3000`) |
| Gargoyle43 | 15;29/0;31/23/21/6 | all NOTHING / all NOTHING | FLYING; PRIORITYMOD_2\|BIT4\|BIT5 (`$2030`) |
| Arrow Launcher60 | 17;33/0;15/24/23/4 | Steel Arrow47 equipped, rest NOTHING / all NOTHING | BRASS_GUNNER; PRIORITYMOD_2\|BIT4 (`$2010`) |
| Golem9 | 12;24/0;28/24/19/4 | all NOTHING / all NOTHING | REGULAR; PRIORITYMOD_2\|BIT5 (`$2020`) |

Resistance is LIGHTNING_WEAKNESS for Soulsower; FIRE_MINOR for Mage;0 for Monk
and Gargoyle; LIGHTNING_WEAKNESS|STATUS_MINOR for Launcher; and
LIGHTNING_MINOR|ICE_MAJOR|FIRE_WEAKNESS|STATUS_IMMUNITY for Golem.
Soulsower prowess is CRITICAL_STUN/DOUBLE_1IN32/COUNTER_1IN32; Golem is
CRITICAL150_1IN8/DOUBLE_1IN32/COUNTER_1IN32; Monk is
CRITICAL150_1IN4/DOUBLE_1IN16/COUNTER_1IN16. Mage/Gargoyle/Launcher use
CRITICAL150_1IN32/DOUBLE_1IN32/COUNTER_1IN32. These operands do not prove
performed stun, resistance, critical, double, counter or casting.

InitializeEnemyStats selects the returned definition **before** copying its56
bytes. It restores HP/MP, applies row coordinates, combines selected movement
upper nibble with placement commandset, sets actual enemy ID/regions/orders,
handles placement item, reconstructs activation, adjusts difficulty and refreshes
current stats. NOTHING returns before AddItem; definition gear remains. Activation
retains priority upper nibble, with zero filler/mode replacing original BIT4/5.
The difficulty body's double-BEQ means every non-SUPER applies floor(ATT×5/4)
once, SUPER twice, before equipment. Difficulty0 unupgraded ATT is43 Soulsower,
25 Mage,33 Monk,38 Gargoyle,35 Launcher and35 Golem. This is ATT, not damage.

Iron Rod88 is WEAPON ATT+12/range1–1; Steel Arrow47 is WEAPON ATT+17/range2–2.
Both useSpell NOTHING63. Preserve the actual getter's erroneous255 no-use
comparison: fallback HEAL fails offensive TYPE_ATTACK filtering. These weapons
also lack AI-use bit13 and are not Healing Rain. Equipped stat/range support
does not make them automatic healing/attack items. No Battle17 row appears in
the **independent** special-drop table; its battle/enemy/item/once-bit/action
consumer cannot award a selected special drop here. NOTHING, definition gear
and table-authorized action loot are different mechanisms.

## F517 and actual repeat-definition selection

**Confirmed:**17 is the sixth member of the length-prefixed random-battle list.
IsBattleUpgradable succeeds; DetermineBattleUpgrade tests completion517,
independently of417/467. Clear517 returns base with no upgrade RNG. Set517
permits selection during fresh initialization, even after admission clears417.
F88 entry bypasses this entire fresh process. No per-round upgrade occurs.

Category reads the **original definition movement upper nibble**, before copy,
not the row's commandset, family name or a returned substitute's movement:

| Original movement / selected types | Table range R | Inclusive bounds | Exclusions |
| --- | --- | --- | --- |
| REGULAR /Soulsower11, Golem9; BRASS_GUNNER /Launcher60 | Melee5 | OOZE0..REAPER35 | KRAKEN_LEG10, PAWN13, KNIGHT14, RAT15, BURST_ROCK32, ZEON_GUARD38 |
| MAGE /Mage69 | Mage1 | GALAM_MAGE67..CHAOS_WIZARD74 | QUEEN71 |
| HEALER /Monk78 | Healer2 | DARK_CLERIC76..EVIL_BISHOP84 | BISHOP80 |
| FLYING /Gargoyle43 | Airborne2 | HUGE_BAT40..DEMON52 | count0; unsafe scan below |

Unrecognized movement takes the earlier direct-base default. All selected bases
have a recognized branch, including BRASS_GUNNER's explicit melee branch.
Bowie0's level supplies effective E; signed-byte class>11 adds20. Subtract17:
nonpositive values repeat that read once using shared D4, then return base.
Positive values divide by10 and keep quotient low byte q; q0 returns base.
Ordinary eligible selection begins at **E27**. Wrapped/corrupt values are not
a normalized level policy.

D3 starts at base; add`5q−5` for R5, otherwise q. D2 saves that center.
Direct GenerateRandomNumber with range R+1 supplies0..R and advances the16-bit
main seed `(13×seed+7) mod65536`. Signed-byte lower/upper bounds precede the
exclusion loop. A failure consumes the single D4 retry, restores center D2 and
draws again; a second failure returns original base. This is no clamp or
unlimited reroll. At q1 the pre-filter domains are Soulsower11..16,
Golem9..14, Mage70..71, Monk79..81 and Gargoyle44..46. Melee13/14/15/10,
Mage71 and Monk80 exclusions apply to the relevant domains. These are candidate
domains, not probabilities or guaranteed stronger results.

Launcher60 **does not use the ranged table**. At q1 it draws60..65, all above
melee35; both attempts reject and return60. More generally with an ordinary
unsigned level byte and the optional+20, eligible q is1..25: center60+5q−5
and roll0..5 produce60..185. Values60..127 fail signed upper35;128..185 fail
signed lower0. Thus these eligible Launcher calls return base after two RNG
draws even though no substitution occurs. Earlier ineligible branches return
base without those draws. This is a bounds fallback after recognized
classification, not the unknown-movement default or a ranged-family upgrade.

Gargoyle's excludedEnemies macro emits count0. The consumer reads it, subtracts1
without a zero guard, then executes DBF across **65,536 subsequent bytes** unless
an equality intervenes. An accidental match uses the shared retry/base fallback;
exhaustion can accept a candidate. The source comment is not a guaranteed safe
base return. Do not replace it with an empty-list algorithm or promise successful
44..46 substitutions. No out-of-table census or bulk seed enumeration is made.

All fourteen calls occur128–141 in order. Failed draws, Launcher fallback and
unsafe Gargoyle scans can affect later seeds/returns and round RNG. A returned
substitute changes definition identity, stats/HP/MP, movement, gear, spells,
resistance/prowess/priority before refresh; coordinates, commandsets, regions
and orders remain the row's operands. Base combat/action claims below are
conditional on unchanged selected definitions, not blanket repeated outcomes.

## Both regions, exact projections and assigned standby

**Confirmed:** both records are type4, byte1=0 and trailing0/0. Region0/F90
vertices are`(0,0),(0,31),(17,31),(17,0)`; region1/F91 uses
`(0,0),(0,31),(12,31),(12,0)`. Only135/136 assign primary0; every other
primary and all secondaries are15. Region1 is declared yet unassigned.

IsRegionTriggered tries triangles1/2/4 then3/2/4, scanning all30 allies with
positive signed HP and nonnegative signed-byte X/Y; joined/active/following
are not separate predicates. Segment projection uses signed MULS/DIVS truncated
toward zero, horizontal dominance on equal spans, projection versus actual
coordinate and bounded collinearity. Edge order is1→2,3→1,3→2 within each
triangle.01 immediately accepts; ordinary side flags must match the opposite
vertex. Bit0 exceptions skip either early edge but reject at the third.
Negative projection explicitly writes CCR=1→10 despite its misleading comment.
This is not assumed equivalent to arbitrary cross-product geometry.

All12 positions substituted against both triangles per region yield24
region-position/48 triangle misses. Complete first-rejection witnesses are:

| Region / triangle | Rejection edge in original vertex numbers | Ally projection / flag; opposite vertex projection / flag |
| --- | --- | --- |
| 0 /1,2,4 | Third,4→2 | X3–5 at Y21–25 /10; vertex1 X17 versus actual0 /00 |
| 0 /3,2,4 | Second,4→3 | X17 versus ally24–27 /10; vertex2 X17 versus actual0 /00 |
| 1 /1,2,4 | Third,4→2 | X2–3 at Y21–25 /10; vertex1 X12 versus actual0 /00 |
| 1 /3,2,4 | Second,4→3 | X12 versus ally24–27 /10; vertex2 X12 versus actual0 /00 |

For diagonal4→2 the exact projection is`0 + trunc(W×(Y−31)/−31)` for W17/12;
for edge4→3 it is W. The ordered table above plus all12 coordinates permits
reproduction without the ignored helper. Actual fresh living placements use a
prefix/subset; ordinary deaths do not advance coordinates and other Xs reset-1.
No fresh90/91 hit follows. Later movement/saved state is not covered by this
finite substitution.

ActivateEnemies scans128–159, then its absent outer RTS falls through once
for160; the [extra160 alias owner](../northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
retains that conditional defect. Region testing loops **all declared regions
before checking this enemy's assignments**. NEWLY_TRIGGERED_BATTLE_REGIONS is
a tested-word cache, including misses, separate from persistent90–105. Thus
unassigned1 can be tested/cached and can set91 under changed live ally positions.
With clear incoming tested bits and these fresh positions, tested becomes3
without90/91. Incoming tested bits can suppress checks. Fresh flag reset and
ClearAiMemory do not explicitly clear the word; normal enemy StartAiControl does.

Primary0 hit sets activation bit0 then returns, preserving existing bit1;
only a secondary hit sets bits0/1. Here no secondary region is assigned, and
no region15 exists. Activation does not exchange orders. Sparse region-program
tables contain only32/40, not17. All declared/undeclared fresh spawn modes0
give no hidden/respawn selection. F517 does not create spawning or a scene.

With flags90/91 clear,135/136 enter inactive standby. Their non-NONE ally0
orders cause ValidateAiStandbyEligibility to return nonzero d1, which its caller
treats as stop: STAY and an empty move string. Earlier thinking RNG can also stop.
The comment suggesting permission does not reverse that polarity. No NONE-based
local-wander case is assigned here. The other twelve enemies have15/15 and
bypass inactive standby irrespective of activation bit0. Region1 has no alternate
assigned order, enemy wave or activation role in this formation.

## Live referents, complete command streams and spells

**Confirmed:** `$01` on128–130 resolves **current ally1**, normally Sarah's
combatant slot, not template1 or AI point1. `$00` on131/132/135/136 resolves
current ally0/Bowie. Their positions/HP are live and need not equal the named
template coordinates; actual placement depends on party/dead/capacity state.
`$8D` on139 resolves current enemy141 (initial Gargoyle at`(15,26)`), not ally13,
enemy13 as an absolute ID or a fixed destination. Stored bit6 is clear for all;
there are zero fixed AI points. NONE255 must be handled before destination lookup.

On the activated/unconditional normal path, primary live-target HP0 promotes
secondary NONE and clears secondary, leaving both NONE. This includes129/130
and135/136 despite their commandsets lacking MOVE_ORDER; inactive standby delays
that mutation for135/136. Actual streams stop at the first d1=0 success:

| Set / actual enemies | Complete stream | Preparatory pathfinding |
| --- | --- | --- |
| HEALER1/0 /130 | HEAL1, SUPPORT, ATTACK1, MOVE1, STAY | Regular |
| HEALER3/2 /139 | HEAL2, MOVE_ORDER4, SUPPORT, ATTACK3, MOVE2, STAY | Block non-movable |
| ATTACKER1/6 /129,133,135,136,137,141; actual AI allies independently | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY | Regular |
| ATTACKER2/7 /128,131,132,134,138,140 | MOVE_ORDER1, ATTACK1, HEAL1, SUPPORT, MOVE1, STAY | Regular |

Set0/6 contain **no MOVE_ORDER**. Their stored follows can affect standby or
dead-target promotion but do not promise pursuit. Set7 has a real ordered
consumer for128/131/132;134/138/140 NONE fail MOVE_ORDER1 then continue.
Set2 can consume139's live141 order after failed HEAL2. MOVE_ORDER1 and4 both
use target type0;4 adds block-non-movable post-processing. “Target type0” does
not replace a live referent with point0; stored bit6 still selects the getter.

The command rejects MOV0/NONE/dead follow targets; type0 builds occupied grids,
tests current/adjacent attack stations, then uses `tst.b` on returned X: X0 fails,
whereas-1 is nonzero and can continue. It applies quadrant marking, tries ATTACK1
and conditionally builds ordered movement. Reverse search uses the selected
ordinary move-cost profile, budget128, preliminary MOV×2 and station radii0/1/2/3;
no Kraken-specific cost branch is selected by these base definitions. Success
can publish STAY plus a nonempty movement string. Failed commands/path tests
advance through the stream; command-local obstruction processing still occurs,
and top-level exit clears temporary obstruction bits. Ordinary MOVE1/2 scores
live opponents/path costs rather than guaranteeing the stored follow destination.

Base Mage BLAZE2 is silence-affected TYPE_ATTACK, MP6/range1–2/radius1/power10;
Monk BLAST1 is silence-affected TYPE_ATTACK, MP2/range1–1/radius0/power6.
GetNextUsableAttackSpell/GetHighestUsableSpellLevel filter actual learned level,
type/confusion and affordable MP; BLAZE may fall back to a lower known level.
Fresh14/19MP covers these costs but no target/casting station or performed cast
is established. Soulsower's stun prowess is not a learned spell; no base here
learns a support spell. SUPPORT does not create one. Attack candidates, priority,
physical range, occupancy, terrain and final path remain conditional.

Monk learns HEAL2: declared MP5/range0–2/radius0/power15; HEAL1 is
MP3/range0–1/radius0/power15. The actual healing path differs from ordinary
affordable-level selection: reject Muddle2, require minimum3MP and build living
friendly candidates with3×HP≤2×maxHP. MissingHP≤2 fails. With known level2
(`d5=1`), larger missingHP leaves returned d2=0/HEAL1; higher-threshold promotion
needs known level3. The helper's incorrect shift5/unmasked d4 causes this selected
entry`$40` to check **HEAL2's5MP** while returning HEAL1. MP3/4 passes the initial
check but fails that loop; MP≥5 may proceed as HEAL1/range0–1/power15. The caller's
nonself≥11MP promotion also requires known level3+. HEAL1/2 command suffixes do
not change this decision. Fresh enemies are full HP, so learned healing alone
supplies no injured friendly candidate; earlier successful commands can bypass it.
Changed upgraded spells/loadouts require their actual consumers, not these base
results or a silently repaired healing algorithm.

## Round generation and first conditional control

**Confirmed:** round prefix is activation → region program → spawn selection →
turn generation. The generator clears64 sentinels, scans30 allies then128–157,
and admits nonnegative-X/nonzero-HP actors independently of activation/standby.
All fourteen unchanged base enemies qualify with AGIs
`25,22,21,21,21,21,23,19,25,22,19,21,23,21`; none has bit128/extra turn.
They supply fourteen entries, including waiting135/136, on that unchanged state.
Repeated substitutions can change AGI and introduce other definition behavior;
no general repeated-entry queue is proven.

Ordinary score is AGI + RNG(AGI>>3) − RNG(AGI>>3) + RNG(3) −1. An actual
bit128 adds the distinct extra score using floor((AGI&127)×5/6) and two draws.
Range0 still advances the main seed.62 signed-byte stable-sort passes preserve
append-order ties; cursor becomes0. Earlier upgrades/fallback draws, actual
allies/refreshed gear and supplied seed affect the queue. Standby also uses a
separate thinking RNG; this does not imply every AI draw uses that generator.
No fixed first actor or player-ready seam follows from these definitions.

BattleLoop sentinel requests another round; ExecuteIndividualTurn masks the
actor and skips HP0. MUDDLE/AI bit2 selects AI preparation first. Otherwise enemy
**opponent-control=0** chooses AI, nonzero player preparation; ally auto-battle
independently chooses AI. Cursor/view/windows/grid work precedes SLEEP/STUN
NoAction, so preparation may already have occurred when status stops execution.
Dispatch repeats MUDDLE/bit2/control polarity before AI execution or player input.
F88 may supply a retained queue/cursor and changed status/orders/positions/cache.
Stop at this conditional control boundary, before movement/action/damage/terminal.

## Actual production consumers and conversion limits

**Confirmed by accepted implementation inspection, separate from original
evidence:**17 is not admitted/converted/played by the current private route.
[Trust](../../../../remake/docs/content/profiles-and-trust.md#selected-third-encounter)
and [Application](../../../../remake/docs/application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation)
retain selected2/3 consumption. No consumer is executed/changed here. Existing
mechanisms are distinguished from missing selected bindings and Unsupported
behavior; an enemy name alone is not a missing engine mechanism.

| Actual consumer | Reusable support and specific Battle17 limit |
| --- | --- |
| [prepare_encounter / compile_routes](../../../../src/sf2tool/remake_exploration_battles.py) / [scene producer](../../../../src/sf2tool/remake_battle_scene_content.py) | Encounter producer admits2/3, scene producer1/2/3. Wildcard routing/400–450–500 flags/before-load-start exist; no17 export/route/intro conversion is admitted. Random-zone admission remains at its prior owner. |
| [PrivateBattleEncounterReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) / [PrivateBattleDefinitionReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) | Generic complete formation/regions/zero-points/orders/metadata and definition/item/spell lookup exist; trusted pins admit1/2/3, not17. New names do not fail a family allowlist in generic definition lookup. No517/effective-level/category/exclusion/RNG substitution is implemented. |
| [PrivateBattleScenarioReader.Assemble](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs) / [PrivateBattleActionBindings](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Independent region-program admission1/2/3 rejects17. STARTING/NOTHING, sets0/6/7, live-follow/NONE, Regular/Healer/Flying exist. HEALER3/2 rejects; Mage/Gargoyle/Launcher BIT4 fail AI-word regex; MAGE/BRASS_GUNNER movers reject. Soulsower stun, Golem1IN8 and Monk critical/double/counter prowess reject. Gargoyle ordinary physical prowess fits but does not remove its BIT4 boundary. Rod adjacent-range metadata fits; Steel Arrow2–2 rejects physical binding. HEAL descriptions can bind; BLAZE/BLAST executable spells do not. Controlled classes0/1/2/4 omit arbitrary Peter/Luke/Rohde parties. |
| [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) / [BattleInitializationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) | Actual membership rebuilding, carried resources/seeds, already-refreshed formation, living/immortal healing, difficulty0 ATT/activation exist. Excess active count rejects instead of source tail eviction. General gear/final-CURSE refresh, other difficulty/control policies and arbitrary natural classes remain limited; enemy loadouts allow0/127, rejecting equipped Rod/Arrow. F88 rejects rather than restoring queue/cursor. Unarmed NOTHING bases need no new gear mechanism. |
| [BattleActivationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs) / [BattleTurnFlow](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) / [TurnOrderRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs) | Mode0/RegionProgram.None, formation, primary-first activation,15/15 and randomized/extra turns exist. Cross-product geometry/fresh per-round tested state is not proof of source signed projection/cache-lifetime equivalence; no17 observation validates this join. |
| [SourceEnemyAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs) / [SourceMoveOrderAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) / [BattleTerrainRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTerrainRules.cs) | Standby, NONE/live follow/dead-target promotion, temporary restrictions and selected6/7/9/14 streams exist. Activated AI rejects0/2, learned spells and equipped loadouts; automatic allies reject. Set6's absent MOVE_ORDER is not a missing-follow defect. Flying already exists; BRASS_GUNNER, exact0/2 healing/spell streams and general ranged/spell AI remain limits. Descriptive resistance/prowess is not executed damage/status behavior. |
| [BattleControlRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) / [capability owner](../../../../remake/docs/capability-status.md) | Classifiers expose unsupported status/automatic/control policies. Generic party/round/control data does not provide natural party producers, saved-state restoration, graphics/services or a17 adapter/content route. Prior admission/outcome owners retain their separate limits. |

These are explicit conversion/support requirements, not an implementation request
or original-game evidence inferred from remake behavior.

## Unknowns, retained failures and reproduction

**Unknown:** natural flags/arrival, actual joined/active/dead/capacity/follower/AI
history, levels/classes/gear/status/difficulty/seeds, Gargoyle scan/returned
substitutions, later region crossings/cache, targets/paths/tactics/first actor,
performed stun/spells/actions, saved queue/control, service completion,
presentation/audio/hardware timing and durable persistence. They are not an
automatic acquisition or implementation queue.

Accepted Battle01 admission/victory/stable-return/later-Down observations remain.
#1047's twelve **completed unresolved comparisons** remain at the
[verification owner](../../../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03),
not interrupted runs or passes. Preserve intermediate-status/final-CURSE and
opponent-control polarity corrections and
[PR1221's two P2 findings](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415).
[Battle12](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1234)
retains scene-identity/HEAL-helper corrections;
[Battle13](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1236)
retains same-region134/136→135 versus138 and neutral lookup31/63 corrections;
[Battle16](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1238)
retains the exact SWARM count/stream/cost corrections and completed helper
failures. A document-check PASS does not supersede semantic failures.

Reproduce at the pin by reading all26 rows, both regions and all six full bases.
Join Map69 row9-before17 and globals/hooks before fresh/saved lifetimes. Trace
517 through recognized movement, effective level, q/range, both bounds,
exclusions/retry/RNG and definition copy. Check Launcher melee60..65 at q1
versus upper35, and Gargoyle count0→DBF65535 without an out-of-table census.
Apply the48 finite triangle substitutions above; separately check unassigned1
testing/cache versus assigned0 standby. Resolve live1/0/141 before inspecting
actual streams0/2/6/7 and HEAL2's returned-HEAL1/cost5 operands. Recheck actual
HP/status/control and selected base versus repeated definitions before the
round endpoint. Read every named production binding/rejection body separately.

This is linear selected-source/consumer reading plus48 triangle substitutions,
one process and small receipts; arithmetic is budgeted below64 MiB/one minute.
It completed in0.001 seconds rounded; peak memory was not measured. The retained
18,285,979-byte map-init/script pair and `local/issue1005/pair.txt` remain
unchanged by length/mtime against retained metadata. No pair was regenerated,
copied or parsed. New helpers/receipts live only in ignored `local/issue1240/`;
old results remain preserved. Replan before concrete material scope/resource
expansion. Content readability determines length, not an arbitrary line ceiling.

Direct checks cover strict UTF-8/LF, local links/anchors with external references
counted separately, exact three-path committed diff, private/ownership boundary,
unchanged fixture metrics/four frontiers and retained input metadata. On clean
committed head load existing ignored selections in the same launching process:

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify plan --base origin/main --head HEAD
```

Record full semantic review, exact-head scoped CI, completed failures and stopped
process state in the Issue handoff. No normal/full/SDK/Godot/emulator/H3/H4,
screenshots, extraction, comparison or collection is selected for this docs-only
boundary. Stop at clean pushed frozen Draft PR, Review / Verify and successful
main-gate notification. Independent acceptance/merge, cleanup, archival and next
task remain with main-gate.
