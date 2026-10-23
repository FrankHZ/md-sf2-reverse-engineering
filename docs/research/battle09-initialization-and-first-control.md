# Battle09 initialization, repeat upgrades and first control

Evidence date: 2026-10-22. **Confirmed:** TO_RIBBLE's complete12/11/2/0
selection supplies twelve ally placement templates, eleven starting enemies,
two ordered regions and no points. All twelve fresh positions miss both regions
under the original integer projection/CCR branches. F509 can enable actual
per-enemy type selection, including the unsafe airborne exclusion scan for both
Vampire Bats. Peter's joined, active, AI-control and map-follower states have
separate consumers. **Inferred:** the surrounding route connects the departure
from New Granseal toward Ribble. **Unknown:** natural arrival, actual party,
levels/status/difficulty/RNG, substitutions, first actor, complete actions,
presentation/timing and durable continuation. This is selected static evidence.

## Scope and provenance

This is the **Standard**-rigor agreement for
[Issue1228](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1228),
under [Epic1053's finite denominator](./story-content-coverage.md#scope-and-reading-rules).
The outcome supports content maintenance and future conversion review: connect
the complete selected formation, initialization, repeat upgrades and activation
to the first conditional player/AI/no-action dispatch. Cross-owner state and
consumer distinctions justify Standard rigor. Stop before an individual action.
The [admission owner](./new-granseal-departure-through-battle09.md)
retains departure, scrolling versus ordinary entry, partial reload, ordered
Map69 admission and empty hooks. The
[outcome owner](./battle09-outcomes-through-ribble-welcome.md)
retains victory/defeat/escape and the conditional Ribble welcome. Shared party,
refresh and round mechanisms remain at [Battle02](./battle02-initialization-and-first-control.md).

Owned writes are this document and Battle09-related synchronization in the two
coverage owners. Source, code, fixtures, schemas, registry/counters and runtime
frontiers are read-only. No parser, platform, engine implementation, allowlist,
export, acquisition or comparison is added. Acceptance is selected complete
source/consumer review, direct document/link/private/scope checks, clean committed
planner and exact-head scoped CI, then clean pushed frozen Draft PR and independent
main-gate acceptance. Existing adjacent content is reused rather than reopened.

Original paths resolve under `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at this pin. Accepted repository base is
`44a2cf974a0fe0fcd7d49ee33c9173030bd4ead4`, tree
`c5f993780ea7381b0d73f5307b8c77a23552e00e`. The
[USA baseline](../../README.md#pinned-original-baseline) retains input identity;
no new ROM-equivalence or runtime observation is claimed. Short battle paths
below have prefix `code/gameflow/battle/`; `j_` interfaces resolve to the named
implementations. Instruction bodies and return tails, not comments alone,
supply the claims.

| Claim | Declaration and actual consumer |
| --- | --- |
| Complete12/11/2/0 | `data/battles/spritesets/entries.asm` slot9 → `spritesets/spriteset09.asm:BattleSpriteset09`, `0x1B3978..0x1B3AA8`; `sf2macros.asm:allyCombatant/enemyCombatant/combatantAiAndItem/combatantBehavior`, `sf2enums.asm`; `getbattlespritesetsubsection.asm:GetBattleSpritesetSubsection`, `0x1B1630..0x1B169E`. |
| Admission, terrain and globals | `code/common/maps/getbattle.asm:CheckBattle`, `0x799C`; `code/gameflow/mainloop.asm`; `data/battles/global/battlemapcoords.asm` row9 and `global/custombackgrounds.asm`, `enemyleaderpresence.asm`, `halvedexpearnedbattles.asm`, `afterbattlepositions.asm`; `data/battles/terrainentries.asm` slot9 → `battleloop/loadbattleterraindata.asm`; `battlescenes/getbattlescenebackground.asm`. |
| Entry, empty hooks and load | `battleloop_1.asm:BattleLoop`, `0x23A84`; before/start wrappers under `cutscenes/` and selected row9 under `data/battles/cutscenes/`; `code/common/scripting/map/ms_empty.asm`, `mapscriptengine_2.asm` return; `battlefunctions/loadBattle.asm` → `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities`, `data/battles/global/battleneutralentities.asm`. |
| Actual party/Peter and refresh | `battleloop/heallivingandimmortalallies.asm`; complete `initializecombatants.asm`, `0x1B1272..0x1B15F8`; `code/common/stats/battleparty.asm:UpdateForce/JoinForce/JoinBattleParty/LeaveBattleParty`, `0x9900..0x99DE`; `updatecombatantstats.asm`; `code/common/scripting/map/mapscriptengine_1.asm:csc54_joinForceAi`, `followersfunctions_1.asm:InitializeFollowerEntities`, `data/scripting/entity/followers.asm`. |
| Definitions, equipment, spells | `data/stats/enemies/enemydefs.asm` records4/5/41/57/68; `data/stats/items/itemdefs.asm` Short Axe33/Iron Arrow46/Bronze Rod87; `data/stats/spells/spelldefs.asm:BLAZE/FREEZE`; item/spell/stat getters and refresh under `code/common/stats/`. |
| Repeat selection | Complete `battleloop/upgradeenemies.asm`, `0x1B1780..0x1B1A28`; `data/battles/global/randombattles.asm`, `0x1B6DB0..0x1B6DDA`; `sf2macros.asm:battles/upgradeRange/excludedEnemies`; `code/common/tech/randomnumbergenerator.asm:GenerateRandomNumber`. |
| Activation/orders/standby | Complete `battleloop/activateenemies.asm`, `triggerregions.asm`, `0x1AC9FC..0x1ACF30`, `clearaimemory.asm`, `populateenemyspawns.asm`; code/data `cutscenes/regionactivatedcutscenes.asm`; `code/common/stats/combatantstats_1.asm:GetMoveOrders/GetTriggerRegions`, `_2.asm:SetTriggerRegions`; `battlefield/getmoveorderposition.asm`; `ai/startaicontrol.asm`, both `determineaistandbymovement_*.asm`. |
| First commands/control | `ai/executeaicommand.asm`, `command/moveorder.asm`, `command/moveorder/buildmovestringformoveorder.asm`, `command/attack.asm`, `getnextuseableaiaction.asm`, `gethighestusablespelllevel.asm`; `data/battles/global/aicommandsets.asm`; `battleloop/turnorderfunctions.asm`; `battlefunctions/executeindividualturn.asm`, `0x23EB0..0x24242`. |

Existing [spriteset](./battle-spriteset-data.md), [global](./battle-global-data.md),
[routing](./battle-routing-data.md), [loop](./battle-loop.md),
[AI](./battle-ai.md) and [cutscene](./battle-cutscenes.md) inventories and their
`tests/fixtures/h2/battle-*-static-v1.json` owners remain unchanged.
Spriteset `rowContentTracked:false` and routing `routeTargetsParsed:false` remain
fixture limits; no new executable Battle09 composition follows from those fixtures.

## Admission, metadata and three entry lifetimes

**Confirmed:** `BATTLE_TO_RIBBLE=9` selects Map69, area`(0,0,48,32)`, wildcard
triggers255/255. The existing admission owner binds F409 and row9 before row17;
area bounds are outputs, not admission containment. F409/F509 both set clears409
but still selects9 on that call. The default Map69 zone sets409 only with509
clear; a carried409 can still select9 when509 is set. F459 is the intro gate,
not admission409, completion509 or suspension88.

Terrain slot9 selects `BattleTerrain09`, decoded into the shared48×48 terrain
array; this does not make the encounter area48×48. Background9 is OVERWORLD255,
whose consumer falls back to map blocks. Leader-presence9 is0; there is no
leader-victory sweep. Halved-EXP contains only1 and post-battlescene coordinate
overrides only5; neither supplies9 an override. The neutral table has no9 row.
`PositionBattleEntities` therefore adds no selected neutral to this formation.
Terrain/collision, scene rendering and natural traversal are not newly decoded.

| Supplied state | Actual entry sequence |
| --- | --- |
| F88 clear, F459 clear | Clear PLAYER_TYPE/set399; clear seconds; save incoming map/battle, install current fields/base VInts; call empty before9; restore/reassert fields; clear90–105; heal; initialize actual allies/enemies; clear AI memory; `LoadBattle(0)`; set459 before dispatching empty start9; prepare round. |
| F88 clear, F459 set | Skip both hook bodies, while fresh healing, placements, upgrades, refresh, load and round still run. F509 remains independent. |
| F88 set | Restore saved seconds; clear88/AI memory/view target; load with incoming focus; enter retained queue/cursor. No fresh field installation, region reset, healing, formation, upgrade, before/start or new round precedes this loop. A retained sentinel later requests round preparation; valid saved state is a supplied input. |

Both selected hooks are `ms_Empty`; the before wrapper only checks459, while
the start wrapper sets it before the body. The terminator can still wait for
view scrolling and clears scroll speed. Neither hook writes EGRESS_MAP, joins
Peter, changes AI control or installs followers. A supplied stored egress map
is consumed by the existing savepoint table with F399; this slice fixes no
egress tuple. F459-set entry and suspension are distinct lifetimes.
`LoadBattle` rebuilds entities from actual combatant positions/mapsprites and
loads map/sprites/terrain/music/fade services; visible exploration followers
are not an alternate battle roster. Service completion/timing stays Unknown.

## Twelve templates and Peter's independent states

**Confirmed:** four header bytes +23 twelve-byte combatant records +two
twelve-byte regions +zero point bytes =304 (`0x130`), matching
`0x1B3978..0x1B3AA8`. The subsection getter advances by the actual count/stride.
Every ally template has HEALER1/0, NOTHING127, NONE255/region15 twice,
filler0 and STARTING0. The complete ordered positions are:

| Template | Local X,Y | Template | Local X,Y |
| --- | --- | --- | --- |
| 0 | `(6,2)` | 6 | `(5,3)` |
| 1 | `(6,1)` | 7 | `(7,1)` |
| 2 | `(5,1)` | 8 | `(4,3)` |
| 3 | `(5,2)` | 9 | `(3,2)` |
| 4 | `(4,2)` | 10 | `(2,3)` |
| 5 | `(4,1)` | 11 | `(3,3)` |

`InitializeAllAlliesBattlePositions` resets all30 ally X values, calls
`UpdateForce`, then uses its ascending joined-and-active list (flags0–29 and
32–61). It copies only X/Y to actual living members; template identities, AI,
items and spells are not installed on those members. An ordinary dead member
consumes an iteration/member but does not advance the placement cursor. Twelve
iterations are the local capacity; a thirteenth active member enters
`LeaveBattleParty` even if deaths left positions unused. That tail clears
active flag/X, keeps joined and does not rebuild RAM lists again. Empty-party
state has no pre-loop guard proving a valid natural start.

The [shared membership rule](./battle02-initialization-and-first-control.md#joined-active-dead-and-capacity)
keeps `JoinForce` distinct: set joined, rebuild lists, call `JoinBattleParty`
only with active count **below12**. At12 or above, a newly joined inactive ally
stays reserve while an already-active ally remains active. Direct
`JoinBattleParty` has no such capacity test. Twelve templates do not select
twelve fixed natural characters or establish their classes/levels/status.

The [audience owner](./peter-castle-audience-through-town-return.md) binds the
actual `joinForceAI ALLY_PETER,$FFFF` producer: the handler reads Peter7's
activation word, ORs bit2 into its saved value for the nonzero selector, calls
`JoinForce`, then writes that activation word. F7 is joined; F39 depends
on capacity/history. Its later caller writes F702 then F67. The follower row
`follower 67, ALLY_PETER, $FF, 1` is consumed by `InitializeFollowerEntities`
without using F7/F39/AI as its admission test; supplied F65 also affects the
overworld follower prefix. Battle09's empty hooks repair none of these states.
`ClearAiMemory` resets48 last-target bytes to-1 and48 memory bytes to0, not
Peter's combatant AI bit. If actual F7/F39 select him, fresh healing restores
immortal Peter's HP even from0; his placement then uses the actual list cursor.
If his live AI bit remains set, his reached turn takes conditional ally AI set6,
not template HEALER1 and not guaranteed player control. Reserve or follower-only
Peter is no proof of a placed/queued Peter; no fixed first Peter action follows.

Healing scans all30 slots independently of membership: living allies and
immortal Peter7/Lemon28 receive max HP/MP; other dead allies remain dead.
Intermediate `status & 7` retains existing STUN/POISON/CURSE. Subsequent
`UpdateCombatantStats` preserves STUN/POISON and derives final CURSE from
currently equipped cursed gear; an old CURSE bit alone proves no final curse.
Actual equipment/status determines refreshed movement/agility/action eligibility.

## Eleven enemies, definitions and available action operands

**Confirmed:** every enemy row has NOTHING127, filler0 and STARTING0;
all secondary orders are NONE255. Slots128–138 initialize immediately;
undeclared139–159 have max/current HP0 and X=-1. Ordinary mode0 initialization
has no spawn-reset occupied-position gate. These are battle IDs, not map aliases.

| Row / slot | Base type / local X,Y | Commandset | Primary order / region | Secondary region |
| --- | --- | --- | --- | --- |
| 0 /128 | VAMPIRE_BAT41 `(30,13)` | ATTACKER1/6 | NONE /1 | 15 |
| 1 /129 | WITCH68 `(29,15)` | ATTACKER3/9 | FOLLOW_ENEMY\|0 (`$80`) /1 | 1 |
| 2 /130 | GOBLIN4 `(27,10)` | ATTACKER1/6 | NONE /1 | 15 |
| 3 /131 | WITCH68 `(21,19)` | ATTACKER1/6 | NONE /1 | 15 |
| 4 /132 | GREEN_OOZE5 `(19,21)` | ATTACKER1/6 | NONE /1 | 15 |
| 5 /133 | VAMPIRE_BAT41 `(19,11)` | ATTACKER1/6 | FOLLOW_TARGET\|0 (`$00`) /0 | 15 |
| 6 /134 | GOBLIN4 `(14,15)` | ATTACKER1/6 | FOLLOW_TARGET\|0 (`$00`) /0 | 15 |
| 7 /135 | HUNTER_GOBLIN57 `(13,16)` | ATTACKER1/6 | FOLLOW_TARGET\|0 (`$00`) /0 | 15 |
| 8 /136 | HUNTER_GOBLIN57 `(15,7)` | ATTACKER1/6 | NONE /15 | 15 |
| 9 /137 | GOBLIN4 `(13,10)` | ATTACKER1/6 | NONE /15 | 15 |
| 10 /138 | GREEN_OOZE5 `(9,11)` | ATTACKER1/6 | NONE /15 | 15 |

`Set/GetTriggerRegions` packs/unpacks nibbles independently of move-order bytes.
`GetMoveOrders` returns primary/secondary unchanged. `GetMoveOrderPosition`
resolves `$80` to live enemy128 and `$00` to live ally0/Bowie. No MOVE_TO is
selected and there is **no point0**. NONE and region15 are distinct sentinels;
NONE must be checked before a destination getter, not decoded as a point.

| Base definition | Level; HP/MP; base ATT/DEF/AGI/MOV | Movement; equipped item; resistance; original AI word |
| --- | --- | --- |
| VAMPIRE_BAT41 | 7;20/8;21/16/16/6 | FLYING; none;0; PRIORITYMOD_2 |
| WITCH68 | 7;21/10;9/12/15/6 | MAGE; Bronze Rod87; ICE_MINOR; PRIORITYMOD_2\|BIT4 |
| GOBLIN4 | 6;18/0;14/13/13/5 | REGULAR; Short Axe33;0; PRIORITYMOD_2\|BIT5 |
| GREEN_OOZE5 | 5;17/0;18/12/12/4 | REGULAR; none; ICE_MAJOR\|FIRE_WEAKNESS; PRIORITYMOD_2\|BIT5 |
| HUNTER_GOBLIN57 | 7;24/0;8/12/14/5 | STEALTH_ARCHER; Iron Arrow46;0; PRIORITYMOD_2\|BIT4 |

All five have REGULAR spell power, initialStatus NONE and raw unknownByte equal
to type ID. Equipped definitions have three additional NOTHING items; unarmed
ones four. Bat has BLAZE2 and Witch FREEZE1, followed by three NOTHING spell
slots; the others four NOTHING. Bat prowess is CRITICAL_POISON/DOUBLE_1IN32/
COUNTER_1IN32; Hunter uses CRITICAL150_1IN32/DOUBLE_1IN16/COUNTER_1IN16;
Goblin/Ooze/Witch use CRITICAL150_1IN32/DOUBLE_1IN32/COUNTER_1IN32. None of
these base AGIs contains the128 extra-turn bit. These are action operands,
not proof of an actual poison, cast, double attack or counter.

`InitializeEnemyStats` selects the type before copying its56-byte definition,
restores HP/MP, applies placement X/Y, combines definition movement upper nibble
with placement commandset, stores selected ID/orders/regions, handles added
item, reconstructs activation, adjusts difficulty and refreshes current stats.
NOTHING returns before AddItem, retaining definition gear. Activation retains
priority upper nibble$2000; zero filler/mode remove original BIT4/BIT5 from
initial active bits. Stat refresh applies floor(baseATT×5/4) on every non-SUPER
difficulty and twice on SUPER before equipment. At difficulty0 unupgraded
current ATT is26 Bat,19 Witch,22 Goblin,22 Ooze,22 Hunter. These are refreshed
ATT values, not damage or natural difficulty.

Short Axe is WEAPON ATT+5/range1–1; Bronze Rod ATT+8/range1–1;
Iron Arrow ATT+12/range2–2. All have useSpell NOTHING. Retain the
[actual item-getter caveat](./battle05-initialization-and-first-control.md#eight-enemies-equipment-and-available-action-operands):
NOTHING63 differs from the erroneous255 no-use comparison; fallback HEAL fails
the attack-type test, and these non-Rain weapons lack AI-use bit13 for healing
selection. Equipment still supplies physical stats/range, not usable potions.

BLAZE2 is silence-affected TYPE_ATTACK, MP6, range1–2, radius1, power10;
FREEZE1 has MP3, range1–2, radius0, power10. Bat8MP/Witch10MP cover those costs
at fresh base initialization. `GetNextUsableAttackSpell` and
`GetHighestUsableSpellLevel` select attack type/confusion eligibility and an affordable known level; attack AI
still needs a valid target, priority and reachable casting station before
publishing CAST_SPELL. Learned BLAZE2 can permit a lower usable level under
changed MP. Standby or earlier move-order success can bypass attack selection.
AI HEAL1/2 command suffixes are parameters, not newly installed HEAL spells.
Complete spell/poison/damage/action execution is outside this endpoint.

## F509, effective level and actual repeat selection

**Confirmed:** TO_RIBBLE9 is the third member of the length-prefixed
`list_RandomBattles`. `IsBattleUpgradable` succeeds;
`DetermineBattleUpgrade` tests **F509**, not459/409. F509 clear returns base
without upgrade RNG. Set509 permits per-row selection even after admission
clears409. The [Battle14 shared algorithm](./battle14-desert-encounter-and-outcomes.md#completed-repeats-and-actual-enemy-upgrade-selection),
[Battle03 airborne account](./battle03-initialization-and-first-control.md)
and [Battle08 category account](./battle08-initialization-and-first-control.md#f508-and-actual-repeated-entry-upgrades)
retain common implementation; Battle09 selects all four categories below.

| Original movement / selected base types | Range R | Inclusive bounds | Actual exclusions |
| --- | --- | --- | --- |
| FLYING /Bat41 | 2 | HUGE_BAT40..DEMON52 | count0; unsafe consumer below |
| REGULAR /Goblin4, Ooze5 | 5 | OOZE0..REAPER35 | KRAKEN_LEG10, PAWN13, KNIGHT14, RAT15, BURST_ROCK32, ZEON_GUARD38 |
| STEALTH_ARCHER /Hunter57 | 2 | GALAM_ARCHER56..DARK_GUNNER65 | KRAKEN_ARM59, ROOK61 |
| MAGE /Witch68 | 1 | GALAM_MAGE67..CHAOS_WIZARD74 | QUEEN71 |

Category comes from the **original** definition's movement upper nibble, not
the placement commandset or substituted movement. Bowie0's level supplies E;
signed-byte class>11 adds20. Compute `E−9`; nonpositive values repeat the same
read once using shared D4, then return base. Positive values divide by10 and
retain quotient low byte q; q0 returns base. Ordinary selection first becomes
possible at **E19**. Corrupt/wrapped class/level values are not normalized.

D3 begins at base; offset is`5q−5` for R5 and q for R1/2. D2 saves base+offset.
`GenerateRandomNumber` with range R+1 supplies0..R. Signed-byte bounds and the
exclusion scan then accept or consume the one D4 retry: restore D3=D2 and draw
again. Second failure returns base; it is neither clamping nor unlimited reroll.
Failed draws also advance RANDOM_SEED. At q1, pre-filter candidates are42..44
for Bat,4..9 Goblin,5..10 Ooze,58..60 Hunter,69..70 Witch; Ooze10 and Hunter59
are declared exclusions. These are candidate domains, not selected results,
probabilities or a guarantee of stronger statistics.

The actual airborne `excludedEnemies` macro emits count0. Its consumer adds3
to the table pointer, reads that byte, subtracts1 without a zero guard, and
executes DBF over **65,536 subsequent bytes** unless an equality intervenes.
An accidental match takes the shared retry/fallback path; exhaustion can accept
the candidate. The comment claiming failure is not a guaranteed safe base
return. Do not replace this with a safe empty-list algorithm or promise Bat
substitutions. No out-of-table census, ROM scan or runtime simulation resolves
those outcomes here. This defect is directly relevant to slots128/133, unlike
Battle08's selected bases, which never use airborne.

All eleven initialization calls occur in slot order128–138. Earlier draws,
including accidental-scan retries/fallbacks, affect later selections and round
RNG. A returned substitute changes stats/HP/MP, gear, spells, movement,
resistance/prowess/priority before copy and refresh; placement coordinates,
commandset, orders and regions remain this row's operands. This is no in-place
level increment or per-round upgrade. F88 skips fresh selection entirely.
Natural levels/seeds, actual Bat returns and all upgraded action families stay Unknown.

## Two ordered regions and activation lifetimes

**Confirmed:** both records are type4, byte1=0, trailing0/0:

| Region / persistent flag | Vertex1 /2 /3 /4 |
| --- | --- |
| 0 /90 | `(0,20)` /`(14,31)` /`(38,0)` /`(18,0)` |
| 1 /91 | `(0,26)` /`(41,26)` /`(41,0)` /`(26,0)` |

`IsRegionTriggered` tries triangles1/2/4 then3/2/4. It scans all30 allies with
positive signed HP and nonnegative signed-byte X/Y, not enemies, joined flags,
template IDs or followers. `CheckTestPointPositionRelativeToSegment` uses
horizontal dominance on equal spans, signed MULS/DIVS truncated toward zero and
projection versus actual coordinate, followed by Z/C flag synthesis. Bounded
collinearity01 is an immediate triangle hit; other ordinary flags must equal
the opposite vertex for each edge. A bit0 exception skips either early edge
but rejects at the third. Negative projection writes CCR=1 and yields10,
despite the misleading11 comment. This is not arbitrary cross-product geometry.

Direct substitution of **all12** fresh positions gives the following complete
rejection witnesses (binary flags shown with two digits):

| Region / triangle | First rejection for every template | Integer projection witness |
| --- | --- | --- |
| 0 /1,2,4 | Second edge4→1: ally00 versus vertex2's10 | Ally X projection15–17 at Y1–3; opposite vertex2 gives-9, forcing CCR=1/10. |
| 0 /3,2,4 | Third edge4→2: ally00 versus vertex3's10 | Ally X projection17; vertex3 gives18 below actual X38. |
| 1 /1,2,4 | Second edge4→1: ally00 versus vertex2's10 | Ally Y projection19–24 at X2–7; vertex2 gives-15, forcing CCR=1/10. |
| 1 /3,2,4 | Third edge4→2: ally00 versus vertex3's10 | Ally X projection27–28; vertex3 gives26 below actual X41. |

All placed living allies use a prefix/subset of these twelve positions even
when dead members consume iterations; unplaced X resets to-1. Thus no fresh
F90/91 hit follows from this selection. Suspended or later supplied positions
remain separate. This finite result proves no natural crossing or arbitrary
integer/cross-product equivalence.

`ActivateEnemies` scans32 enemy slots, then its missing outer RTS falls through
once for160. The helper skips negative X/HP0/nonenemy before region processing;
the [extra160/deals alias owner](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
retains that conditional defect. Do not erase it because Battle09 has empty hooks.
The tested word NEWLY_TRIGGERED_BATTLE_REGIONS differs from persistent F90–105;
every tested region sets its cache bit even on a miss. Fresh flag clearing and
`ClearAiMemory` do not explicitly clear this word. Non-swarm enemy
`StartAiControl` clears it later; inherited bits can skip checks. Clear incoming
bits with unchanged fresh placement yield tested3 and no90/91.

A primary hit sets bit0 and returns before secondary, preserving an old bit1.
Secondary-only hit sets bits0/1. Witch129 has1/1: a held91 takes primary, not a
new secondary activation; slots128/130–132 have1/15 and133–135 have0/15.
The remaining136–138 have15/15. No region15 declaration exists. Activation does
not swap primary/secondary orders. On the active command path, `StartAiControl`
promotes secondary only when a non-NONE non-point primary's referenced current
HP is0:129 changes `$80/NONE` to NONE/NONE if enemy128 dies;133–135 do likewise
for ally0. Primary-region priority/sticky secondary state is separate from this
HP-dependent order promotion. It supplies no nonexistent point destination.

## Standby, commands, round and first control

**Confirmed:** the sparse region-program table has only32/40 rows, so9 has no
region program. All declared spawn modes are0 and undeclared entries have no
hidden/respawn mode; the fresh spawn-population pass selects no such candidate.
No neutral, region scene or spawn operation adds a twelfth live enemy.

With fresh flags clear,128–135 are inactive, but their actual standby differs:

| Selected slots | Actual initial AI path |
| --- | --- |
| 128/130/131/132: NONE with primary region1 | `ValidateAiStandbyEligibility` returns d1=0/d2=0; if earlier thinking-RNG stop is avoided, use actual starting position as anchor and conditionally build a local move string. Final action STAY can still move. NONE does not mean guaranteed immobility. |
| 129: follow enemy128/region1;133–135: follow ally0/region0 | Non-NONE primary plus assigned region returns d1=1. The caller treats nonzero as stop, writes empty move string/STAY. Earlier thinking-RNG stop also stops; its helper comment does not reverse this polarity. |
| 136–138:15/15 | `StartAiControl` bypasses inactive standby and enters the placement commandset regardless of activation bit0. |

All ten ATTACKER1/6 enemies attempt ATTACK1, HEAL1, SUPPORT, MOVE1, STAY in
order with regular preparation. Witch129's ATTACKER3/9 attempts MOVE_ORDER3,
ATTACK3, HEAL2, SUPPORT, MOVE_ORDER4, MOVE2, STAY with block-non-movable
preparation. First successful command stops the scan. MOVE_ORDER3 is follow
type1 and4 target type0, both block-non-movable; their destination still resolves
stored `$80` to live128. Set6 has no MOVE_ORDER command;133–135's ally0 reference
still matters to standby and dead-follow promotion, but
must not be converted into a promised MOVE_ORDER pursuit. Ally AI, including
conditionally placed AI-controlled Peter, uses set6 independently of template
HEALER1/0 (HEAL1, SUPPORT, ATTACK1, MOVE1, STAY). Occupancy, terrain, refreshed
movement, targets, live HP/status and RNG determine actual results; no first
attack/cast/path or winning tactic is asserted.

At `BattleLoop/@Start`, activation → region program → spawn selection precedes
`GenerateBattleTurnOrder`. The [shared round rule](./battle02-initialization-and-first-control.md#first-round-and-conditional-control-endpoint)
clears64 sentinel slots, scans30 allies then first30 enemy slots, admits living
placed actors independently of activation and uses refreshed AGI/main RNG.
It performs62 signed-byte stable-sort passes and sets cursor0. Eleven base
enemies supply eleven ordinary entries, not twelve template turns or a follower
turn. Each ordinary score is AGI + RNG(AGI>>3) − RNG(AGI>>3) + RNG(3) −1;
AGI bit128 adds the distinct two-draw second score. Range0 still advances RNG.
No seed reset occurs. Upgrade draws precede this round; standby uses the
separate thinking RNG. Changed allies/substitutions can change the queue.

A sentinel requests a round; HP0 actor skips. `ExecuteIndividualTurn` gives
MUDDLE/AI-control bit2 priority to AI preparation. Otherwise enemy
**opponent-control=0** selects AI, nonzero player preparation; ally auto-battle
independently selects AI. Shared cursor/view/status-window/grid work follows;
SLEEP/STUN select NoAction before execution. Dispatch repeats the same
MUDDLE/bit2 priority and opponent-control polarity before AI execution or player
input. Stop at this conditional player/AI/no-action boundary, before an executed
individual action. No fixed first actor or guaranteed initial player-ready state
is established; F88 may start at a retained queue position.

## Current conversion and production support

**Confirmed by accepted production inspection, separate from original-game
evidence:** Battle09 is not admitted, converted or supported by the current
private encounter route. [Trust](../../remake/docs/content/profiles-and-trust.md#selected-third-encounter)
and [Application](../../remake/docs/application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation)
retain selected2/3 consumption. PR692 is unconsumed. No preparation/lowering,
comparison or engine execution is performed here.

| Actual consumer | Reusable mechanism and explicit Battle09 boundary |
| --- | --- |
| [prepare_encounter / compile_routes](../../src/sf2tool/remake_exploration_battles.py) | Producer/CLI admit2/3 only with selected source/ROM checks. Hook/load and400/450/500 route flags are reusable; no9 is exported. |
| [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) / [PrivateBattleDefinitionReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) | Generic parsing retains placements, regions, zero points and metadata; trusted pins admit1/2/3, not9. Definition selection loads declared base types/referenced items/spells; it implements no F509/effective-level/category/RNG substitution. |
| [PrivateBattleScenarioReader.Assemble](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs) / [PrivateBattleActionBindings](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Independent region-program admission1/2/3 rejects9. Sets0/6/9 and live-follow/NONE are expressible. Flying and Hovering movers exist; STEALTH_ARCHER/MAGE do not. Bat CRITICAL_POISON and Hunter double/counter prowess reject; Hunter/Witch BIT4 fails the AI-word regex. Iron Arrow2–2 fails adjacent weapon binding. Short Axe/Bronze Rod range alone is expressible; initialization/AI still reject equipped loadouts. BLAZE2/FREEZE1 data are not executable attack-spell support: assembled executable spells bind HEAL/healing items. Controlled classes0/1/2/4 omit Peter and arbitrary natural classes/party state. |
| [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) | Before/load/start flags, actual membership rebuilding and carried resources/growth/gold/both seeds are reusable; missing member definitions reject. F88 rejects rather than restoring the saved queue/cursor. Completed admission policy supplies no original509 upgrade selection; no admitted9 content reaches it. |
| [BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) / [BattleTurnFlow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) | Controlled already-refreshed allies, roster-only/difficulty0, living/immortal healing, formation and enemy activation/ATT calculation exist. Excess active count rejects instead of source tail eviction. General equipment/CURSE refresh, other difficulty/control policies and suspension remain Unsupported. Enemy items admit0/127, rejecting selected equipped weapons. |
| [BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs) / [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs) | Mode0/RegionProgram.None, living placed ally tests, primary-first/sticky-secondary activation and randomized/extra entries are reusable. Cross-product geometry and fresh per-round tested word do not prove equality to original integer projection/cache lifetimes. No Battle09 equivalence observation exists. |
| [BattleControlRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) / [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs) / [SourceMoveOrderAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) | Control classifiers expose unsupported automatic-ally/status/control policies; enemy AI rejects ally actors, so Peter's original set6 branch is not production support. Enemy standby/NONE/15/15, sets6/9, live-follow and dead-target promotion exist. Activated AI permits only0/127 items and no learned spells, rejecting equipped Hunter/Goblin/Witch and Bat/Witch spells. Adjacent physical station search is not general ranged/attack-spell AI. Plain Ooze does not admit the whole encounter or arbitrary substitutes. |

These are bounded support requirements at actual consumers, not an allowlist
patch, engine-defect acceptance claim or new implementation task. Existing
mechanisms are not reported missing. Remake code proves no original-game fact.

## Unknowns, retained failures and reproduction

**Unknown:** natural arrival/request/flags, joined/active/dead/capacity history,
Peter's actual battle/follower/control state, classes/levels/equipment/status,
difficulty/RNG, actual substitutions through the airborne defect, region crossings,
full action/target/tactical legality, continuous continuation, presentation/hardware
timing and save durability. Twelve templates and a map follower prove no fixed
party or first actor. Surrounding admission/outcome owners retain their scope.

Accepted Battle01 original admission/victory/stable-return/later-Down observations
remain intact. The #1047 twelve **completed unresolved comparisons** stay at the
[verification owner](../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03),
not interrupted suites or an automatic acquisition/repair queue. Preserve the
intermediate-status/equipment-CURSE correction. PR1221's mechanical PASS missed
the stale coverage summary/denominator and opponent-control polarity; its
**two P2** findings and correction remain in the
[handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415).
Mechanical checks cannot replace semantic review.

Reproduce from the complete named source/consumer bodies at the pin: all23
combatant rows, two ordered regions, zero points and five complete definitions;
follow509/category/level/exclusion/retry into actual definition copy/refresh.
Substitute all12 positions into both triangles of each region using signed
MULS/DIVS, truncation and CCR/edge branch order. Separately read production
admission/binding/rejection bodies. Existing fixture counts/frontiers stay fixed.

Direct checks cover UTF-8/LF, relative links/anchors separately from external
references, explicit three-path `origin/main` diff including committed head,
private/ownership and retained metadata. Retained map-init/map-script payloads
total18,285,979 bytes; these and `local/issue1005/pair.txt` stay unchanged.
New small methods/fresh receipts stay under `local/issue1228/`, preserving old
results. Work is linear selected reads plus24 region-position/48 triangle
substitutions, one process, small memory/output. No bulk run or artificial prose
line ceiling is selected. Replan only for concrete required acquisition or
cross-system expansion.

On a clean committed head, load ignored selections in the same process:

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify plan --base origin/main --head HEAD
```

Record exact-head scoped CI and completed failures in the handoff. No normal/full,
SDK/Godot/emulator/H3/H4, screenshots, comparison/lowering or new extraction is
selected. Stop at clean pushed frozen Draft PR, Review / Verify handoff, stopped
writer/processes and successful notification to the assigned main-gate;
independent acceptance and merge remain with it.
