# Battle20 Harpy SWARM, neutral Elric and first conditional control

Evidence date: 2026-10-23. **Confirmed:** complete12/12/0/0 connects actual
party/definition refresh, three SWARM versus two ordinary Harpies, live orders
and first conditional control. The HP-only defeated count tests128–138 and
omits actual enemy139. Neutral Elric159 has HP0 and a physical actor, but its
lookup is written at31 while the159 reader uses63. Harpy DISPEL has a real
SUPPORT command path, subject to earlier stream success and target selection.
**Inferred:** these declarations support the intended pond encounter.
**Unknown:** natural arrival/state, first actor/actions, service completion,
presentation and persistence. This is static conversion guidance, not played
Battle20 or admitted production content.

## Agreement and provenance

Standard rigor under [Issue1246](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1246)
and [Epic1053's finite scope](../../story-content-coverage.md#scope-and-reading-rules).
Party, HP-count, order, spell and entity lifetimes cross several existing owners;
the outcome is one source-backed account through conditional individual control.
The [approach/entry owner](./harpy-pond-approach-through-battle20-entry.md) retains
the confrontation, warning and request publication. The
[outcome/Elric owner](./battle20-outcomes-and-elric-recruitment.md) retains
resolution, recruitment and return; the
[departure owner](./harpy-pond-departure-through-fairy-cave-entry.md) retains
F751/passage/travel. Their conditional connections are not a natural itinerary.

Owned tracked paths are this new primary, Battle20 consistency only in
`story-content-coverage.md` and `source-coverage.md`, and the entry owner's narrow
final-CURSE/neutral correction and primary link. The read-only worktree/open-topic
check found no competing writer or stacked dependency. Accepted base is
`6477dcc25b04177924293453b1f81f9da4e0b3c8`, tree
`a2b6e0e41e0ec7bb8173b5f1a33a0a4a0a7c9f47`; PR692 is unconsumed.
No source, code, fixture, schema, index, allowlist, counter, design or runtime
frontier changes. No battle simulation, acquisition, emulator, screenshot,
dataset processing, comparator or implementation is included.

Original paths below resolve under `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin; the
[USA baseline](../../../../README.md#pinned-original-baseline) owns ROM identity.
No fresh ROM equivalence or runtime observation is claimed. Short battle-code
paths use `code/gameflow/battle/`; sibling names retain their directory. Fnnn
denotes flag nnn; row/template/slot indices are zero-based. Resolve `j_` aliases
to their actual implementations and symbolic operands through `sf2enums.asm`.
Instruction operands and branch polarity control over comments and names.

Reuse [Battle16 SWARM](./battle16-initialization-and-first-control.md),
[Battle18 neutral/refresh](./battle18-initialization-and-first-control.md),
[corrected Battle19 healing](./battle19-initialization-and-first-control.md)
and the [AI owner](../../battle/battle-ai.md), while reading the actual selected consumers.
Battle16's count happens to cover all its enemies; that conclusion does not
transfer to20. Existing [spriteset](../../battle/battle-spriteset-data.md),
[routing](../../battle/battle-routing-data.md), [global](../../battle/battle-global-data.md),
[loop](../../battle/battle-loop.md) and [cutscene](../../battle/battle-cutscenes.md) fixtures keep their
boundaries. `rowContentTracked:false` and `routeTargetsParsed:false` remain;
this prose is not an executable Battle20 fixture or an observation.

| Claim | Pinned declaration and actual consumer |
| --- | --- |
| Complete selection | `data/battles/spritesets/entries.asm` slot20 → `spritesets/spriteset20.asm:BattleSpriteset20`, `0x1B482C..0x1B4950`; `sf2macros.asm:allyCombatant/enemyCombatant/combatantAiAndItem/combatantBehavior`; `getbattlespritesetsubsection.asm`, `0x1B1630..0x1B169E`; enums. |
| Map/globals/hooks | `code/common/maps/getbattle.asm:CheckBattle`, `0x799C`; `code/gameflow/mainloop.asm`; complete `data/battles/global/{battlemapcoords,custombackgrounds,enemyleaderpresence,battleneutralentities,halvedexpearnedbattles,afterbattlepositions}.asm`; terrain20; actual before/start slots20, wrappers and `code/common/scripting/map/ms_empty.asm`. |
| Actual party/definitions | `battleloop_1.asm:BattleLoop`, `0x23A84`; complete `battleloop/initializecombatants.asm`, `0x1B1272..0x1B15F8`, `heallivingandimmortalallies.asm`; `code/common/stats/{battleparty,updatecombatantstats}.asm`; full `data/stats/enemies/enemydefs.asm` records12/44/60/70/79; `battleloop/upgradeenemies.asm`, random-battle and independent item-drop tables. |
| Actors/neutral | `battlefunctions/loadBattle.asm`; `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities/DeclareNewEntity`; `battlefunctions/battlefunctions_0.asm:GetEntityIndexForCombatant`; `data/stats/enemies/{enemymapsprites,enemybattlesprites}.asm`; `data/scripting/entity/eas_battleneutralentities.asm:eas_WhirlsInWater`; `sf2const.asm`. |
| Activation/count/orders | `battleloop/{activateenemies,triggerregions,populateenemyspawns,clearaimemory}.asm`; code/data `cutscenes/regionactivatedcutscenes.asm`; `ai/startaicontrol.asm:StartAiControl/CountDefeatedEnemies`, `0xDEFC..0xE1AC`; `data/battles/global/swarmbattles.asm:table_SwarmAiEnemyCounts1`, `0xE25B..0xE294`; `aicommandsets.asm`, `0xE1AC..0xE25B`; stats `GetMoveOrders/GetTriggerRegions`; `battlefield/getmoveorderposition.asm`, `0x1B15F8`. |
| Commands/spells/control | `ai/executeaicommand.asm`; full `command/{moveorder,attack,heal,support}.asm`, `move.asm`, selected movement/attack/healing/support helpers; `getnextuseableaiaction.asm`, `gethighestusablespelllevel.asm`; `command/heal/{doescombatantrequirehealing,calculatehealtargetpriority,determinehealingspelllevel}.asm`; `command/support/prioritizetargetsforsupportspells.asm`; `data/battles/global/aipriority.asm`; item/spell definitions and `code/common/stats/spellstats.asm:GetSpellDefinitionAddress`; `code/common/tech/randomnumbergenerator.asm`; `battleloop/turnorderfunctions.asm`; `battlefunctions/executeindividualturn.asm`, `0x23EB0..0x24242`. |

## Map51 and fresh versus saved entry

**Confirmed:** the sole Map51 battle-map row is20, crop origin`(0,0)`, size18×26,
triggers255/255. CheckBattle requires F420 and treats those triggers as wildcards;
the crop is an output, not a containment check. F520 set clears420 after selection
without canceling this admission. F470, Elric membership/HP, approach completion
and party composition are not predicates in this row. The entry owner retains
the actual F420 → pending Map51`(0,0)`, Right → script return → F470 ordering.

Before20 and start20 select `ms_Empty` at`0x47A4E`, the complete terminator.
Both wrappers check F470; before does not set it, start sets it before execution.
A completed approach therefore skips them. Direct supplied420/clear470 runs
the empty before and later sets470 before the empty start; it does not replay
the confrontation. Empty script execution still uses common termination/services.

| Supplied state | Ordered entry work |
| --- | --- |
| F88 clear/F470 clear | PLAYER_TYPE0/F399; clear seconds; save/install Map51/Battle20/base VInts; empty before; restore/reassert map/battle; clear90–105; heal/refresh eligible allies; place actual party; initialize enemies; clear AI memory; `LoadBattle(0)`; set470/empty start; activation/region-program/spawn/round. |
| F88 clear/F470 set | Skip both hook bodies while still performing fresh clearing/healing/placement/definition refresh/load and a new round. F520 remains independent. |
| F88 set | After mode0/F399, restore saved seconds; clear88/AI memory/view target; LoadBattle with incoming focus; enter retained queue/cursor. No fresh map/battle installation, hooks,90–105 reset, healing, placement or new round precedes individual turns. A sentinel later requests a round. |

Terrain slot20 selects `BattleTerrain20`, loaded into shared48×48 storage,
distinct from18×26 crop. Background20 is VALLEY16. Leader-presence20 is0:
actor128 is neither a leader-kill shortcut nor the authority for SWARM admission.
The neutral table has the separate completion-gated Elric below. Half-EXP
selects only1 and post-battlescene position override only5. No special20 row
is inferred from those tables. The outcome owner retains terminal counts,
Elric recruitment, F420/F520 writes and returns.

Saved map/battle/area, resources, orders, positions, tested cache and queue must
already agree. LoadBattle rebuilds actors even on this saved path, but does not
make it fresh initialization. Incoming20 is not a repair of inconsistent saved
fields; F520 controls the neutral tail using the actual current battle.

## All twenty-four records and the actual party

**Confirmed:** 4-byte header +24 records×12 =292 (`0x124`) bytes, ending
exclusively at`0x1B4950`. There are no region/point bytes. Every ally template
has HEALER1/0, NOTHING127, primary/secondary NONE255 with15/15, filler0 and
STARTING0. Only these X/Y coordinates are copied:

| Template | X,Y | Template | X,Y |
| --- | --- | --- | --- |
| 0 | `(6,8)` | 6 | `(5,9)` |
| 1 | `(7,8)` | 7 | `(4,9)` |
| 2 | `(5,8)` | 8 | `(5,10)` |
| 3 | `(6,9)` | 9 | `(6,10)` |
| 4 | `(8,8)` | 10 | `(8,7)` |
| 5 | `(7,9)` | 11 | `(9,7)` |

Healing scans all30 allies independently of membership: living allies and
immortal Peter7/Lemon28 receive max HP/MP; other dead allies stay dead. Its
intermediate `status & 7` keeps STUN/POISON/CURSE. UpdateCombatantStats then
preserves STUN/POISON and derives **final CURSE from equipped cursed items**;
an old CURSE bit alone does not survive refresh. This happens after the earlier
approach, so it does not retroactively repair scene actor/HP problems.

Initialization sets all30 ally Xs to-1, rebuilds ascending joined/active lists
from flags0–29/32–61, and traverses `BATTLE_PARTY_MEMBERS`. A dead member consumes
a list/loop iteration without advancing the coordinate row. Twelve iterations
bound capacity; remaining members lose active flag/X via LeaveBattleParty,
retaining joined. Unused positions after deaths do not enlarge capacity. The
zero-template guard is not a pre-loop empty-list check. Ally IDs/AI/items/spells
and behavior are not overwritten by template declarations.

Exact enums include Bowie0, Peter7, Rohde11, Elric13 and Lemon28. Elric's joined13
and active45 are distinct from F470/F520/F751, scene/current lookup13 and neutral
combatant159. Neither admission, empty hooks nor initialization joins him or
promises a deployed living Elric. JoinForce's separate capacity check may leave
a newly joined member in reserve; it does not clear an already-set active bit
at full capacity or revive ordinary dead allies. Peter may revive without being
joined/active; his earlier AI-bit2 clear and all follower selectors retain their
own producers. Actual automatic allies use set6, irrespective of HEALER1 here.

All enemy rows add NOTHING127, have secondary NONE255, regions15/15, filler0 and
STARTING0. Their complete differing operands, live referents and swarm bytes are:

| Row /slot | Definition /X,Y | Commandset | Primary order | Swarm table byte |
| --- | --- | --- | --- | --- |
| 0 /128 | HARPY44 `(15,23)` | SWARM15 | NONE255 | 3 |
| 1 /129 | DARK_MADAM70 `(9,22)` | ATTACKER3/9 | FOLLOW_ENEMY3 (`$83`) →131 | 0 |
| 2 /130 | HIGH_PRIEST79 `(10,21)` | HEALER3/2 | FOLLOW_ENEMY1 (`$81`) →129 | 0 |
| 3 /131 | ORC12 `(9,19)` | ATTACKER1/6 | NONE255 | 0 |
| 4 /132 | ARROW_LAUNCHER60 `(4,20)` | ATTACKER3/9 | FOLLOW_ENEMY5 (`$85`) →133 | 0 |
| 5 /133 | ORC12 `(5,19)` | ATTACKER1/6 | NONE255 | 0 |
| 6 /134 | HARPY44 `(1,16)` | SWARM15 | NONE255 | 2 |
| 7 /135 | HIGH_PRIEST79 `(6,15)` | HEALER3/2 | FOLLOW_ENEMY8 (`$88`) →136 | 0 |
| 8 /136 | DARK_MADAM70 `(8,13)` | ATTACKER1/6 | NONE255 | 0 |
| 9 /137 | HARPY44 `(11,17)` | ATTACKER1/6 | NONE255 | 0 |
| 10 /138 | HARPY44 `(14,3)` | ATTACKER1/6 | NONE255 | 0 |
| 11 /139 | HARPY44 `(4,1)` | SWARM15 | NONE255 | 1 |

There are five Harpies, of which only128/134/139 are SWARM;137/138 use ordinary
set6. Fresh initialization scans128–159: declared128–139 initialize immediately,
without a spawn-occupancy test, while undeclared140–159 get max/currentHP0 and
X=-1. The later neutral tail changes159 separately. No hidden wave follows.

## Five complete definitions and actual refresh

**Confirmed:** all five have spellPower REGULAR, initialStatus NONE and raw
unknownByte equal to their definition ID. Every item/spell array has four slots;
“rest NOTHING” below completes the array, not an unspecified remainder.

| Definition | Level; HP/MP; base ATT/DEF/AGI/MOV | Four items /four spells | Movement; original AI word |
| --- | --- | --- | --- |
| Harpy44 | 17;32/10;39/38/29/7 | NOTHING×4 /DISPEL1, NOTHING, NOTHING, NOTHING | FLYING5; PRIORITYMOD_2\|BIT4\|BIT5 (`$2030`) |
| Dark Madam70 | 17;29/17;19/31/29/6 | Power Stick89 equipped, rest NOTHING /FREEZE2, rest NOTHING | MAGE11; PRIORITYMOD_2\|BIT4 (`$2010`) |
| High Priest79 | 17;30/29;20/31/23/5 | Power Stick89 equipped, rest NOTHING /HEAL3, BLAST2, NOTHING, NOTHING | HEALER12; PRIORITYMOD_3 (`$3000`) |
| Orc12 | 17;30/0;22/37/27/5 | Steel Lance75 equipped, rest NOTHING /NOTHING×4 | REGULAR1; PRIORITYMOD_2\|BIT5 (`$2020`) |
| Arrow Launcher60 | 17;33/0;15/24/23/4 | Steel Arrow47 equipped, rest NOTHING /NOTHING×4 | BRASS_GUNNER4; PRIORITYMOD_2\|BIT4 (`$2010`) |

| Definition | Complete resistance | Complete prowess |
| --- | --- | --- |
| Harpy | WIND_WEAKNESS\|FIRE_WEAKNESS | CRITICAL_POISON\|DOUBLE_1IN32\|COUNTER_1IN32 |
| Dark Madam | ICE_MINOR | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN32 |
| High Priest | 0 | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN32 |
| Orc | 0 | CRITICAL150_1IN8\|DOUBLE_1IN32\|COUNTER_1IN16 |
| Arrow Launcher | LIGHTNING_WEAKNESS\|STATUS_MINOR | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN32 |

InitializeEnemyStats asks UpgradeRandomBattleEnemies before copying the full
56-byte definition. The complete eleven-battle random list omits20, so F520
does not admit substitution or the unsafe airborne repeat path. The independent
special-drop table also omits20: row NOTHING is not the reason for that absence.
The copy fills HP/MP, sets row position/type/orders/regions, combines base mover
with row commandset and handles the item. NOTHING returns before AddItem,
retaining copied definition gear. Activation construction keeps the priority
upper nibble while mode0/filler0 replaces raw BIT4/5; these bits are not fresh
active/AI-controlled flags. Harpy's fresh word is`$2000`, Priest's`$3000`.

Difficulty's second BEQ makes non-SUPER apply floor(baseATT×5/4) once; SUPER
applies it twice before stat/equipment refresh. At difficulty0 with fresh status,
current ATT is Harpy48, Madam38, Priest40, Orc43 and Launcher35. These are stats,
not damage. No selected gear changes their DEF/AGI/MOV. Resistance/prowess data
does not establish performed poison, double/counter, resisted spell or timing.

| Copied item | Complete relevant definition | Consumer consequence |
| --- | --- | --- |
| Power Stick89 | MAGE/PRST/WIZ/SORC/VICR;1–1; price1050; WEAPON; use NOTHING63; ATT+15, NONE0, NONE0 | Madam/Priest adjacent physical range and refresh. |
| Steel Lance75 | KNTE/PLDN/PGNT;1–1; price810; WEAPON; use NOTHING63; ATT+16, NONE0, NONE0 | Orc adjacent physical range and refresh. |
| Steel Arrow47 | ACHR/RNGR/SNIP/BRGN/BWNT;2–2; price1270; WEAPON; use NOTHING63; ATT+17, NONE0, NONE0 | Launcher physical range2–2, separate from its movement/order. |

Unarmed Harpy's `battlefield/getactionrange.asm:GetAttackRange` is1–1;
AI physical station construction uses0–1, the shared distinction already owned
by Battle16/18. FLYING uses the
ordinary flying movement/priority consumers, not Kraken AQUATIC costs. All held
weapons are equipped but have no attack use-spell: the item getter's incorrect
255 absence comparison lets63 fall back to HEAL, whose type fails TYPE_ATTACK.
They lack AI-use bit13 and are not Healing Rain, so do not become healing items.

## Neutral Elric: stats, physical identity and lookup mismatch

**Confirmed:** LoadBattle calls PositionBattleEntities, clearing old field data
and lookups. Ordinary placed allies/enemies are rebuilt Down/standing, with
flying/hovering layer handling; the three exploration enemy sprites128–130 are
not retained as battle identities. Battle129/130 are Madam/Priest, regardless
of the earlier Soulsower scene sprites. Physical cursor values depend on the
actual placed allies. These five enemy definitions use ordinary mapsprites.

After ordinary placement, F520 clear selects the complete neutral row:
`(6,6)`, Down, ELRIC_BASE mapsprite30, `eas_WhirlsInWater`. Combatant159 receives
max/currentHP0, status0, activation OR8 and X/Y6,6. This does not assign ally13's
stats, definition or membership. A physical actor is declared despite HP0.

The mapping instruction is `move.b d0,-(a1)` after resetting a1 to
`ENTITY_INDEX_LIST_ENEMIES=$FFB160`: the physical index is written at`$FFB15F`,
lookup31 relative to`ENTITY_INDEX_LIST=$FFB140`. DeclareNewEntity stores the
physical d0/d6 as ENTNUM and does not repair lookup63. The ordinary159 getter
subtracts`$60`, reading offset63/`$FFB17F`, which this fresh path leaves`$FF`
from skipped undeclared159. The neutral's actual physical ENTNUM is the next
ordinary cursor after allies plus twelve enemies; no fixed physical index or
working159→sprite binding follows. B31's lookup is also not ally13's lookup.

The complete action enables immersion, enables double animation speed, then
loops Right → refresh → wait10 → Up → refresh → wait10 → Left → refresh →
wait10 → Down → refresh → wait10 → branch to Right. The flags are set before
the loop, not reset each cycle. Initial Down is not a permanently held facing;
these source animation/wait requests do not prove rendered drowning or elapsed
time. No screenshot or runtime diagnosis is made.

F520 set skips the entire neutral tail without erasing the twelve selected
enemies. F520 clear with independently joined/active Elric may produce both a
normal ally and this separate neutral; no reconciliation is performed. On F88
load the same tail is gated, but fresh enemy clearing did not run; set520 does
not establish that supplied saved neutral fields were erased. HP0 excludes this
neutral from living activation/count, and the ordinary round enemy scan ends157.
Its absent contribution to SWARM count follows independently from128–138 bounds.

## Zero regions and the exact SWARM admission rule

**Confirmed:** round preparation orders ActivateEnemies → region cutscene →
spawn selection → turn generation. Activation scans128–159 and, lacking an outer
RTS, falls through once for160; the
[extra160 owner](../northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
retains that defect. TriggerRegionsAndActivateEnemies sees count0 and returns
before polygon tests, tested-word writes or assignment activation. No triangle
calculation is needed. Region-program tables select32/40 only, not20. Fresh
mode0 on declared/undeclared slots yields no hidden/respawn selection.

All selected enemies have15/15. After the earlier SWARM gate, normal AI bypasses
inactive standby directly to command handling without requiring bit0 activation.
Zero regions does not disable AI or make a waiting Harpy a future spawn.

1. Only enemy commandset15 enters the swarm test. CurrentHP unequal to maxHP
   bypasses it into normal AI. At full HP the16/20/22 list match selects pointer1
   for20, then indexes threshold bytes by `combatant & $7F`.
2. The complete twelve bytes are`3,0,0,0,0,0,2,0,0,0,0,1`. Threshold0 skips
   counting. The actual selected SWARM slots128/134/139 use3/2/1;137/138 never
   enter this gate because their set is6.
3. CountDefeatedEnemies requests **ALLIES**, obtains12, subtracts2 and executes
   the loop before DBF. d2 values10..0 mean **eleven HP tests128–138 inclusive**.
   Actual twelfth enemy139 is omitted; neutral159 and all allies are outside.
   The Battle16 statement “all declared enemies” is false for Battle20.
4. `cmp.b d1,d0; ble` takes normal AI when threshold≤count. Below threshold
   writes STAY and first move-string byte-1, then exits. These small nonnegative
   counts/thresholds have no signed-byte ambiguity.

Count tests currentHP==0 only: no X/placement, maxHP, membership, activation,
death-list, sprite or completed death-effect predicate. A living unplaced128–138
does not count; HP0 there does, even before a completed death animation. HP0 on
139 alone does not increase it. Killing139 plus two counted enemies gives2,
so full-HP128 still waits; this is a supplied-state counterexample, not a kill
route. This helper differs from CountRemainingCombatants' HP/X terminal rule.

Fresh definitions give all twelve positive full HP, hence initial count0. If
unchanged on reaching AI preparation,128/134/139 wait and the ordinary nine
attempt their own streams. Later full-HP139 admits at counted1,134 at2,128 at3.
Damage is a current-value bypass, not a permanent latch; healing to exactly full
HP may restore waiting below threshold. HP0 actors skip individual control, and
nonzero opponent control can bypass enemy AI. No natural wave, kill order,
simultaneous activation or performed action is established.

Every StartAiControl exit clears temporary obstruction flags. SWARM waiting
exits **before** normal-path clearing of NEWLY_TRIGGERED_BATTLE_REGIONS,
dead-follow promotion and commands. Persistent90–105 reset, tested-word lifetime
and ClearAiMemory's48 last-target/-1 plus48 memory/0 bytes are distinct. Neither
fresh flag reset, memory clear nor SWARM STAY performs that normal cache clear.
Zero-region activation leaves the tested word alone; no polygon cache is needed.

## Complete streams and actual live orders

**Confirmed:** commands run in order; returned d1=0 short-circuits, nonzero tries
the next command. Numbered commands are parameters, not learned spell levels.

| Set /actual actors | Complete stream | Preparatory pathfinding |
| --- | --- | --- |
| HEALER3/2 /130,135 | HEAL2, MOVE_ORDER4, SUPPORT, ATTACK3, MOVE2, STAY | Block non-movable |
| ATTACKER1/6 /131,133,136,137,138 | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY | Regular |
| ATTACKER3/9 /129,132 | MOVE_ORDER3, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER4, MOVE2, STAY | Block non-movable |
| SWARM15 /128,134,139 after admission | MOVE_ORDER1, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER2, MOVE2, STAY | Regular |

For129/130/132/135, bit6 is clear in`$83/$81/$85/$88`, so GetMoveOrderPosition
reads current combatants131/129/133/136, initially`(9,19)/(9,22)/(5,19)/(8,13)`.
These are neither fixed points3/1/5/8, allies, physical indices nor immutable
coordinates. There are zero declared points and no selected MOVE_TO operand;
Battle18's absent-point read is not needed here.

Normal preparation tests the primary live target's HP and, if zero, promotes
secondary NONE to primary and clears secondary before dispatch. SWARM wait
precedes that step. MoveOrder rejects MOV0, NONE and dead live targets. Set9's
MOVE_ORDER3 uses follow-type1/block-non-movable, while MOVE_ORDER4 uses type0/
block-non-movable. Set15's MOVE_ORDER1 is type0/regular and MOVE_ORDER2 type2/
block-and-carve; all three selected swarm orders are NONE, so those commands
fail before selecting a point or live position. Sets6 have no MOVE_ORDER at all.

A successful order command can attack internally or move-and-STAY and prevent
later healing/support/attack attempts. A failed command can still apply its
pathfinding restrictions. The shared type0 station check uses `tst.b` on returned
X, distinguishing X0 from the-1 failure sentinel; follow-type commands have their
own terrain-marker test. Terrain, occupancy, current target position, current MOV,
path construction and earlier success remain conditions, not a scripted route.

## Actual spell filters, DISPEL and candidate-local HEAL

**Confirmed:** fresh definitions learn Harpy DISPEL1, Madam FREEZE2 and Priest
HEAL3/BLAST2. All use AFFECTEDBYSILENCE; learned entries, AI selection and action
execution are separate. Attack selection filters TYPE_ATTACK; BLAST/FREEZE pass
the confused-caster whitelist too, while DISPEL is TYPE_SUPPORT and never becomes
an attack spell. GetHighestUsableSpellLevel uses the proper rank shift6 and
current MP for attack spells: FREEZE2 costs7 (FREEZE1 costs3), BLAST2 costs5
(BLAST1 costs2). Reachable candidate lists, physical/spell/item priorities and
category RNG precede final target/station/path selection. No base MP or power
operand fixes a cast, damage result or first action.

| Learned selected spell | MP; range; radius; power | Definition animation |
| --- | --- | --- |
| DISPEL1 | 5;1–2;1;0 | DEBUFF1 |
| FREEZE2 | 7;1–2;1;12 | FREEZE\|VARIATION2 |
| BLAST2 | 5;1–2;1;8 | BLAST\|VARIATION2 |
| HEAL3 | 10;0–3;0;30 | HEALING_FAIRY\|VARIATION3 |

Harpy's complete stream contains SUPPORT after earlier attack/heal attempts
(and after swarm admission for set15). GetNextSupportSpell finds DISPEL;
aiCommand_Support rejects allies/confused casters, accepts exactly DISPEL1 or
MUDDLE2, and checks DISPEL's actual5MP cost. The DISPEL helper builds reachable
area centers and scores affected units: one point for an admitted attack spell,
otherwise one for a known healing spell, never two for both on one unit. It
retains centers with priority≥2. Attack-spell scoring itself uses its usability/
MP filter; healing-spell lookup does not impose the same MP test. Thus one lone
qualifying caster is insufficient for this area score, even though Harpy knows
DISPEL and has10MP. Highest priority, target/station availability and move-string
construction must still succeed before CAST_SPELL/DISPEL is returned. Earlier
successful commands prevent reaching SUPPORT. The support routine's intervening
MOVE/CLR instructions overwrite the targeting BTST condition codes; the actual
branches mark allied occupancy then build ally targets on this path, rather
than a generic spell-property-driven targeting choice. No executed silence or
natural support target follows from the learned slot.

Priests' HEAL2 command parameter is not HEAL2 learned rank. The learned HEAL3
slot is local`$80` at a new aiCommand_Heal invocation. Nonconfused healing starts
with an actual3MP gate; candidates must live and satisfy3×HP≤2×maxHP. Priority
uses commandsets **CRITICAL13/LEADER14**, not SWARM15 despite the source comment;
these selected Harpies use FLYING priority6, Priests HEALER12, Madams MAGE11,
Launchers5 and Orcs REGULAR2. Candidate/AOE sorting and available stations still
control which candidate is attempted and whether an action is admitted.

For each candidate with deficit M, DetermineHealingSpellLevel wrongly checks
`(incoming entry + (rank << 5))`, then converts returned rank1 to0. With current
input`$80`, M≤2 fails. M3–14 starts rank0/check`$80` HEAL3 cost10 and returns
HEAL1 atMP≥10. M≥15 starts rank2/check`$C0` HEAL4 cost20; if unaffordable it
tries rank1/key`$A0` (absent ATLAS3, getter defaults to HEAL1 cost3), then rank0/
key`$80` cost10 as needed. ForMP≥3 this gives HEAL1 at3–19 and HEAL3 at≥20.
On this same`$80` input, a nonself returned HEAL1 is promoted to HEAL2 atMP≥11;
self is not. Actual casting uses the selected spell's cost/range, not the wrong
lookup's checked cost. The
[complete corrected table](./battle19-initialization-and-first-control.md#learned-heal3-blast2-and-freeze2)
retains the single-candidate boundary and provenance.

The caller writes selected local rank **before** DetermineAttackPosition. A
failed station advances to the next candidate without rereading the learned
slot. Incoming`$40` then selects HEAL1/check5MP for any M>2, fails atMP3/4,
and cannot receive nonself promotion because its current rank is1. A successful
rank choice changes that local to`$00` even if its station later fails.
Incoming`$00` selects HEAL1/check3MP for M>2 and also has no promotion. Helper
failure leaves the previous local unchanged. Failed attempts spend no MP and
do not unlearn HEAL3; a new independent HEAL command reads`$80` again. The
current29MP alone cannot establish HEAL3 for a later candidate. Preserve this
candidate-local lifetime rather than copying an unconditional deficit/MP rule.

## Round generation and first conditional control

**Confirmed:** the generator clears64 sentinel entries, scans30 allies then
128–157 and admits nonnegative-X/nonzero-HP actors independently of activation
or SWARM waiting. Fresh selected enemies all qualify; ordered AGIs are
`29,29,23,27,23,27,29,23,29,29,29,29`. None has bit128, so they supply twelve
entries, including the three waiting swarm actors. Neutral159 supplies none.

Ordinary score is AGI + RNG(AGI>>3) − RNG(AGI>>3) + RNG(3) −1. Actual bit128
would add the separate floor((AGI&127)×5/6) score with two draws; no selected
base has it. Range0 still advances the main seed.62 signed-byte stable-sort
passes preserve append-order ties and set cursor0. Ally current gear/stats,
actual party and initial/earlier seeds affect order. No fixed first actor or
first player-ready seam follows from formation, HP count or learned spells.

ExecuteIndividualTurn masks actor identity and skips HP0. MUDDLE/AI-bit2
chooses AI preparation first; otherwise enemy opponent-control=0 selects AI,
nonzero player preparation. Ally auto-battle is a separate AI selector.
Cursor/view/windows/grid preparation precedes SLEEP/STUN NoAction; preparation
may already have changed AI state before status prevents action. Dispatch
repeats MUDDLE/bit2/control polarity before AI execution or player input.
F88 instead can retain a changed queue/cursor and caller state. Stop at these
conditional branches before claiming performed movement/action or usable input.

## Accepted production consumers and conversion limits

**Confirmed by accepted implementation inspection, separately from original-game
evidence:**20 is not exported/trusted/admitted/played by the current route.
[Trust](../../../../remake/docs/content/profiles-and-trust.md#selected-third-encounter)
and the [capability owner](../../../../remake/docs/capability-status.md) retain the
selected2/3 boundary. No consumer is changed or executed in this slice.

| Actual consumer | Reusable mechanism and selected20 limit |
| --- | --- |
| [prepare_encounter / compile_routes](../../../../src/sf2tool/remake_exploration_battles.py), [scene producer](../../../../src/sf2tool/remake_battle_scene_content.py) | Encounter preparation admits2/3, scene build1/2/3. Wildcard routing,400/450/500 flags and before/load/start phases exist; no20 selection/export/route or pond producer conversion is admitted. |
| [PrivateBattleEncounterReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [PrivateBattleDefinitionReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) | Generic complete formations, zero region/point arrays, selected definitions/items/spells/resistance/prowess metadata exist; trust pins admit1/2/3. Descriptive lookup needs no new enemy-name mechanism. No20 trusted transport is produced here. |
| [PrivateBattleScenarioReader.Assemble](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs), [PrivateBattleActionBindings](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Independent region-program admission1/2/3 excludes20. Sets6/9, NONE/live orders and FLYING/REGULAR/HEALER exist; HEALER3/2 and SWARM15 reject. Harpy/Madam/Launcher BIT4 fails the AI-word binding; MAGE/BRASS_GUNNER movers reject. Harpy poison and Orc prowess reject; Priest/Madam ordinary prowess and adjacent ATT-only weapon metadata can fit, but Arrow2–2 rejects physical binding. HEAL can bind player healing actions; FREEZE/BLAST/DISPEL descriptions do not bind executable spell/support AI. Controlled classes0/1/2/4 do not cover arbitrary actual parties/Elric. |
| [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [BattleInitializationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) | Membership rebuild, carried resources/seeds, formation/living-immortal healing and difficulty0 ATT/activation exist. Excess active count rejects instead of source eviction. General gear/final-CURSE refresh remains limited; equipped enemy weapons reject loadout0/127 admission. F88 rejects instead of restoring queue; other difficulty/control policies and natural membership producers remain limits. |
| [BattleActivationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [TurnOrderRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs), [BattleControlRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) | STARTING/RegionProgram.None/zero-region return/15/15 and randomized/extra-round primitives exist. They do not implement the HP-only128–138 bound,20's omitted139, thresholds/re-wait or original state/cache/service lifecycle. Generic control classification is not a20 observation. |
| [SourceEnemyAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs), [SourceMoveOrderAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) | Standby,6/7/9/14, live-follow/dead-target promotion and flying priority/movement are reusable. Activated AI rejects2/15 and learned spells/loadouts beyond0/127; automatic allies reject. SWARM waiting must precede normal cache clear/order work, whereas current AI clears tested state before supported active streams. Exact15, candidate-local healing, DISPEL area score/selection and offensive-spell decisions remain gaps. No point mechanism is required for these selected live/NONE orders. |
| [ExplorationContentReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs) entity/services and adapter boundary | Generic face/wait/flags/refresh/jump actions are expressible; native calls retain Unsupported forms. No admitted20 route binds F520-gated neutral159, lookup31/read63, physical ENTNUM or WhirlsInWater services. Generic logical actors and ordinary battle projection do not establish that original mismatch or its presentation. Elric13 join/active45, later passage, actual party/status/control/suspend and20 adapter/content remain separate conversion requirements. |

These are specific support/conversion limits, not a request for replacement
infrastructure or admission-only edits. Existing Flying/order/round mechanisms
should be reused when a future scoped implementation is authorized. Descriptive
content, trusted admission, assembled capability and played behavior differ.

## Unknowns, retained failures and reproduction

**Unknown:** natural flags/route/party/capacity/classes/gear/status/seeds,
actual HP-count transitions, targets/paths/HEAL/DISPEL/attack choices, first
actor/action, damage/prowess, saved-state validity, completed entity/map/text/
camera/animation services, presentation/hardware timing and persistence. These
do not create an automatic runtime queue.

Preserve #1047's twelve **completed unresolved comparisons** at the
[verification owner](../../../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03).
They are neither passed nor interrupted. Existing
[control/summary corrections](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415),
intermediate/final-CURSE findings, [Battle12 HEAL/identity](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1234),
[Battle13 neutral](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1236),
Battle16/18 findings and Unknowns remain. PR1245's initial`16f4233a` direct/CI
mechanical PASS missed the candidate-local HEAL lifetime; its
[independent P2](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1245#issuecomment-6366643680)
and corrected`c1893c7b` are retained, already accepted in this base. No original
failure receipt is overwritten or turned into semantic PASS here.

Reproduce at the pin/base by reading all24 selected records and all five full
definitions, actual map/global/hook selections, then each named caller/consumer
above. Re-derive12−2/DBF10..0 and compare128–138 to actual128–139; substitute HP
states independently of placement/death effects. Trace full-HP/threshold/normal
cache/order/cleanup ordering and all four complete streams. Resolve the neutral
predecrement write against the ordinary getter and physical ENTNUM, then the
complete action loop. Trace actual TYPE/MP/SUPPORT scoring and HEAL local writes
across failed stations before reporting selected actions. Review production
rejections independently of the source descriptions.

This uses bounded source/consumer reads and small direct receipts under ignored
`local/issue1246/`, linear in selected text/records, budgeted below64 MiB and
one minute for check work; peak memory is not measured. There are zero regions,
so no triangle arithmetic or empty verification machinery is added. The retained
18,285,979-byte pair and pair.txt are checked by existing length/mtime metadata
against `local/issue1230/retained-before.json` only; no payload is copied, parsed,
regenerated or processed. Reassess material scope/resource growth.

Direct checks cover UTF-8/LF, links/anchors/fences/tables, private boundary,
exact four paths, unchanged fixture metrics/four frontiers and retained metadata.
On a clean committed candidate, load `. ./local/private-inputs.ps1` in the same
process and run `uv run sf2 verify plan --base origin/main --head HEAD`, then
exact-head scoped CI. Docs-only policy controls generic suite suggestions:
no normal/full/SDK/Godot/H3/H4, screenshots, collection or historical comparison
reruns. Actual checks, inspection errors, Git objects and stopped process state
belong in the Issue/PR handoff. Stop at the clean pushed frozen Draft PR,
Project Review / Verify and successful main-gate notification; independent
acceptance/merge remain with main-gate, with no cleanup/archive/next task here.
