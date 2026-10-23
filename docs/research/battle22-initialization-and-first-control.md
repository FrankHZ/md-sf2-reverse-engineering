# Battle22 chess roster, SWARM waiting and first conditional control

Evidence date: 2026-10-23. **Confirmed:** the admitted Chessboard encounter
initializes sixteen chess pieces, but only nine use SWARM. Their full-HP waiting
depends on an HP-only count of slots128–138, omitting five actual enemies.
King128 has a valid point0 order and an extra round entry; neither leadership
nor the chess formation fixes the first actor. Ponpei159 and Maid158 are separate
neutral state/actor declarations. **Inferred:** the formation and delayed pieces
stage the King's challenge. **Unknown:** natural state, targets/actions, first
usable input and presentation. This is a static account, not a played encounter.

## Agreement, scope and provenance

Standard rigor under [Issue1250](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1250)
and [Epic1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053).
The uncertainty is composition of accepted SWARM, HEAL, LEADER, neutral and round
consumers with the complete selected22 operands. Accepted base is
`c2a898796a357999dff0e1ee2597283346b1b4e0`, tree
`862a61b55e9c7011bd762f5336837304d4d70030`; no unmerged dependency is consumed.
This primary and Battle22 consistency in the two coverage owners are in scope;
the original initialization/outcome owners receive only final-CURSE clarification
and this route. No code, schema, fixture, index, admission or implementation changes.

Begin with [Desktop admission](./desktop-kingdom-interactions-through-battle22-entry.md).
The [initial roster owner](./battle22-initialization-and-roster.md) retains complete
before/load/start wrappers and coordinate/actor construction. The
[outcome owner](./battle22-outcomes-and-desktop-return.md) retains leader sweep,
victory/defeat/Egress and Desktop return; [Cotton Balloon](./cotton-balloon-through-floor-world-entry.md)
retains later reward custody and departure. Those scenes/services are not repeated.
Here fresh entry continues through ActivateEnemies, round generation and the
first conditional dispatch. Saved entry is contrasted without claiming restored
natural state. No all-action, whole renderer, traversal or runtime audit is added.

All original source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. The original input identity remains
USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM extraction is claimed. Fnnn means flag nnn; indexes are zero-based.
Source coordinates and supplied-state counterexamples are not observed positions.

Inspection is linear in these small source sections. Direct receipts/checks are
budgeted below64MiB and one minute; no geometry helper is needed for zero regions.
The retained18,285,979-byte map-init/map-script-engine pair and its `pair.txt` are
checked by existing lengths/mtime only: no payload read, copy or regeneration.
No emulator, acquisition, screenshot, historical comparison or new environment.
Reassess before a material expansion of that boundary.

## Selected source and consumer map

**Confirmed:** these named tables and functions, including the actual enum and
macro operands rather than comments, own the claims below.

| Concern | Complete selected surface / direct consumer |
| --- | --- |
| Admission/globals | `code/common/maps/getbattle.asm:CheckBattle`; Map22 row22 of `data/battles/global/battlemapcoords.asm`, `custombackgrounds.asm`, `enemyleaderpresence.asm`; complete `randombattles.asm`, `enemyitemdrops.asm`, `battleneutralentities.asm`, `halvedexpearnedbattles.asm`, `afterbattlepositions.asm`; `data/battles/terrainentries.asm` slot22. |
| Fresh/saved initialization | `code/gameflow/battle/battleloop_1.asm:BattleLoop`; `battleloop/heallivingandimmortalallies.asm`, `initializecombatants.asm`, `upgradeenemies.asm`, `clearaimemory.asm`; `code/common/stats/battleparty.asm`, `updatecombatantstats.asm`, `combatantstats_1.asm`, `combatantstats_2.asm`, `itemstats.asm`; before/start pointer tables and wrappers. |
| Formation/definitions | `data/battles/spritesets/entries.asm` slot22, complete `spriteset22.asm` (`0x1B4AA4..0x1B4BFA`); `sf2macros.asm`, `sf2enums.asm`, `sf2const.asm`; six ordinal definitions in `data/stats/enemies/enemydefs.asm`, their mapsprite entries, selected `data/stats/items/itemdefs.asm` and `data/stats/spells/spelldefs.asm`. |
| Activation/SWARM | `battleloop/activateenemies.asm`, `triggerregions.asm`, `populateenemyspawns.asm`; `data/battles/cutscenes/regionactivatedcutscenes.asm`; `ai/startaicontrol.asm:StartAiControl/CountDefeatedEnemies` (`0xDEFC..0xE1AC`); complete third table in `data/battles/global/swarmbattles.asm` (`0xE25B..0xE294`). |
| Commands/orders | `data/battles/global/aicommandsets.asm` (`0xE1AC..0xE25B`), `ai/executeaicommand.asm`, `ai/command/moveorder.asm`, `move.asm`, `movementhelperfunctions.asm`; `battlefield/getmoveorderposition.asm`, `determineattackpositionformoveorder.asm`; `combatantstats_1.asm:GetMoveOrders/GetTriggerRegions`. |
| Spell decisions/effects | `ai/getnextuseableaiaction.asm`, `gethighestusablespelllevel.asm`, `command/attack.asm` and its `attack/determinebattleaction.asm`; `command/heal.asm`, `heal/doescombatantrequirehealing.asm`, `determinehealingspelllevel.asm`, `calculatehealtargetpriority.asm`; `command/support.asm`; `code/common/stats/spellstats.asm`; `battleactions/calculatespelldamage.asm:AdjustSpellPower`. |
| Actors/round/control | `battlefunctions/loadBattle.asm`, `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities/DeclareNewEntity`, entity-index getter; `battleloop/turnorderfunctions.asm`; `battlefunctions/executeindividualturn.asm` and its preparation/dispatch callees; [battle-loop](./battle-loop.md) and [control contract](../design/contracts/battle-control-lifecycle.md). |

Reuse [Battle16 SWARM](./battle16-initialization-and-first-control.md),
[Battle18 LEADER/neutral](./battle18-initialization-and-first-control.md),
[Battle19 corrected HEAL](./battle19-initialization-and-first-control.md#learned-heal3-blast2-and-freeze2),
[Battle20 zero regions/neutral/SWARM](./battle20-initialization-and-first-control.md)
and [Battle21 refresh](./battle21-initialization-and-first-control.md). Their
different selected counts, points, upgrades and actions must not be copied as22 facts.

## Map22, actual party and fresh versus saved lifetime

**Confirmed:** the sole Map22 row22 is `22,9,0,30,30,255,255`. CheckBattle requires
F422, accepts both wildcard triggers without a containment test, and writes crop
origin9,0/size30×30. If F522 is set it clears F422 but still admits this call.
F472 and F88 do not gate the scan. The Desktop owner retains F777/F522 zone
admission, F422 write and surviving `(22,0,0,Right)` request.

Background22 is CHESSBOARD17, not the terrain-dependent OVERWORLD255 selector;
terrain slot22 points to `entries/battle22/terrain.bin`. Leader-presence22 is-1,
independent of commandset14 and AGI161. Half-EXP selects only1. The complete
`data/battles/global/afterbattlepositions.asm` selects only BATTLE_GALAM_CASTLE5
and `abp_Battle5`, consumed by `ApplyPositionsAfterEnemyLeaderDies` in
`code/gameflow/battle/cutscenes/afterenemyleaderdeathpositions.asm`; neither table
selects22. Battle44's timer special is separate. The random-battle list and
special-drop table omit22. Completion does not install Battle21's random upgrades
or a special chess item drop. Generic action item rules remain separate.

BattleLoop clears PLAYER_TYPE, sets F399 and tests F88. Fresh entry sets current
map/battle22, executes the selected empty before wrapper, reassigns22 after it,
clears persistent region flags90–105, heals, initializes allies then enemies,
clears AI memory, loads battle with d0=0, and executes the empty start wrapper.
Both hook slots select the actual `ms_Empty` terminator; F472 set skips both.
The start wrapper sets F472 before dispatch when clear. Empty scripts retain
their wrapper/shared waits and do not replay Desktop dialogue. Required services
must return; their calls alone do not establish a displayed or usable encounter.

Healing scans actual allies: living allies and always Peter7/Lemon28 receive
maxHP/MP, an intermediate STUN/POISON/CURSE mask, then UpdateCombatantStats.
That refresh removes incoming CURSE and derives final ally CURSE from equipped
cursed items; an uncursed loadout loses stale CURSE, and equipped cursed gear
can add it. STUN/POISON remain and affect later behavior. Other dead allies are
not refreshed. This does not recruit, equip or assign classes to the party.

InitializeAllAlliesBattlePositions clears all thirty X values, then UpdateForce
rebuilds joined/active IDs in ascending order. The twelve rows below are placement
templates, not named members. A dead nonimmortal consumes a party-loop slot but
does not advance the placement record; surplus members have active flags cleared
and X=-1 without immediate list rebuild. Empty/inconsistent state has no general
safe-empty guarantee. Class, inventory and saved provenance remain supplied inputs.

Roster/point/neutral coordinates are already battle-local. Crop9,0 relates King
`(15,5)` to full-map `(24,5)`, not a subtraction during initialization or a warp.
Actors use `(coordinate &63)×384`. LoadBattle rebuilds mappings, loads current map/
terrain, sets its view from the selected lookup (physical0 fallback on a negative
lookup), and invokes the normal services. Current full load differs from a negative-d1
reloadMap working-layout reuse; the original owner retains those exact consumers.

F88 set instead restores saved seconds, clears F88/AI memory/view target, calls
LoadBattle and jumps directly to `@IndividualTurns_Loop`. It skips incoming22
assignment, wrappers, healing/roster, region reset and round generation. Current
map/battle/crop/queue/cursor/HP/items/status/activation/orders can therefore differ.
LoadBattle still applies its current completion-gated neutral tail; it does not
repair saved state from this formation. This is a branch description, not proven
cross-process restoration, persistence or a legal arbitrary suspend image.

## All28 records and six complete definitions

**Confirmed:** header12/16/0/1 plus28 twelve-byte combatant records and two-byte
point0`(15,5)` occupy342 bytes, half-open `0x1B4AA4..0x1B4BFA`. Subsections start
at offsets4/148/340/340. Combatant fields are type, X/Y, commandset, item word,
primary order/region, secondary order/region, filler and spawn. All28 rows use
NOTHING127, regions15/15, filler0 and STARTING0. All ally templates use HEALER1/0
and NONE255/NONE255; only X/Y are consumed as ally placement.

| Placement row | X/Y | Placement row | X/Y |
| --- | --- | --- | --- |
| 0 | `(18,18)` | 6 | `(23,18)` |
| 1 | `(21,18)` | 7 | `(22,17)` |
| 2 | `(15,18)` | 8 | `(14,17)` |
| 3 | `(20,17)` | 9 | `(18,17)` |
| 4 | `(16,17)` | 10 | `(19,18)` |
| 5 | `(13,18)` | 11 | `(17,18)` |

The full third SWARM table is `0,0,0,6,1,3,0,1,3,6,6,1,0,3,3,0`.
The last column is raw slot data; it gates only actual set15 actors.

| Row /combatant | TypeID /X,Y | Set | Primary byte → live target or point | Secondary | Table byte |
| --- | --- | --- | --- | --- | --- |
| 0 /128 | KING89 `(15,5)` | LEADER14 | MOVE_TO0 `$40` →point0`(15,5)` | NONE255 | 0 |
| 1 /129 | QUEEN71 `(17,5)` | ATTACKER3/9 | FOLLOW_ENEMY0 `$80` →128 | NONE255 | 0 |
| 2 /130 | BISHOP80 `(13,5)` | HEALER3/2 | FOLLOW_ENEMY0 `$80` →128 | NONE255 | 0 |
| 3 /131 | BISHOP80 `(19,5)` | HEALER4/3 | FOLLOW_ENEMY9 `$89` →137 | FOLLOW_ENEMY0 `$80` →128 | 6 |
| 4 /132 | KNIGHT14 `(11,5)` | SWARM15 | NONE255 | NONE255 | 1 |
| 5 /133 | KNIGHT14 `(21,5)` | SWARM15 | FOLLOW_ENEMY8 `$88` →136 | NONE255 | 3 |
| 6 /134 | ROOK61 `(9,5)` | ATTACKER2/7 | NONE255 | NONE255 | 0 |
| 7 /135 | ROOK61 `(23,5)` | SWARM15 | FOLLOW_ENEMY4 `$84` →132 | NONE255 | 1 |
| 8 /136 | QUEEN71 `(15,6)` | SWARM15 | NONE255 | NONE255 | 3 |
| 9 /137 | QUEEN71 `(17,6)` | SWARM15 | NONE255 | NONE255 | 6 |
| 10 /138 | PAWN13 `(13,6)` | SWARM15 | FOLLOW_ENEMY9 `$89` →137 | NONE255 | 6 |
| 11 /139 | PAWN13 `(19,6)` | SWARM15 | FOLLOW_ENEMY4 `$84` →132 | NONE255 | 1 |
| 12 /140 | PAWN13 `(11,6)` | ATTACKER2/7 | FOLLOW_ENEMY6 `$86` →134 | NONE255 | 0 |
| 13 /141 | PAWN13 `(21,6)` | SWARM15 | FOLLOW_ENEMY8 `$88` →136 | NONE255 | 3 |
| 14 /142 | PAWN13 `(9,6)` | SWARM15 | FOLLOW_ENEMY8 `$88` →136 | NONE255 | 3 |
| 15 /143 | PAWN13 `(23,6)` | ATTACKER2/7 | FOLLOW_ENEMY6 `$86` →134 | NONE255 | 0 |

Initialization visits128–159. Declared STARTING rows copy their ordinal56-byte
definitions, fill HP/MP, set position/type/orders/regions/mover/commandset, process
row inventory, adjust difficulty and refresh. Undeclared144–159 get activation0,
max/currentHP0 and X=-1; not every residual field is erased. F522-clear load later
repurposes158/159. No selected row is hidden or respawning.

The complete base scalars follow. Raw unknownByte is stored data, not a key:
King89's is0. Definition80's comment says EVIL BISHOP, but ENEMY_BISHOP=80;
ENEMY_EVIL_BISHOP=84 is a different record. All six initial statuses are NONE.

| Type | raw unknownByte /spellPower | Level | HP/MP | ATT/DEF/AGI/MOV | Movement /copied AI expression |
| --- | --- | --- | --- | --- | --- |
| King89 | 0 /ENHANCED | 24 | 55/28 | 29/40/161/6 | MAGE /PRIORITYMOD_1 |
| Queen71 | 71 /ENHANCED | 20 | 30/26 | 25/36/35/6 | MAGE /PRIORITYMOD_2\|BIT4 |
| Bishop80 | 80 /ENHANCED | 19 | 33/36 | 26/37/25/5 | HEALER /PRIORITYMOD_3 |
| Knight14 | 14 /ENHANCED | 20 | 48/0 | 25/34/29/7 | REGULAR /PRIORITYMOD_2\|BIT5 |
| Rook61 | 61 /REGULAR | 19 | 40/0 | 22/31/27/4 | BRASS_GUNNER /PRIORITYMOD_2\|BIT4 |
| Pawn13 | 13 /ENHANCED | 19 | 36/0 | 26/37/25/6 | REGULAR /PRIORITYMOD_2\|BIT5 |

| Type | Full resistance expression | Base prowess expression | First equipped item | Learned slots, in order |
| --- | --- | --- | --- | --- |
| King89 | WIND_MINOR\|LIGHTNING_MINOR\|ICE_MINOR\|FIRE_MINOR\|STATUS_IMMUNITY | CRITICAL125_1IN4\|DOUBLE_1IN8\|COUNTER_1IN16 | FLAIL90 | BLAZE2, NOTHING, NOTHING, NOTHING |
| Queen71 | ICE_MINOR\|STATUS_IMMUNITY | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN32 | POWER_STICK89 | FREEZE2, NOTHING, NOTHING, NOTHING |
| Bishop80 | 0 | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN32 | POWER_STICK89 | HEAL3, BLAST2, NOTHING, NOTHING |
| Knight14 | STATUS_IMMUNITY | CRITICAL150_1IN4\|DOUBLE_1IN16\|COUNTER_1IN16 | POWER_SPEAR76 | NOTHING, NOTHING, NOTHING, NOTHING |
| Rook61 | LIGHTNING_WEAKNESS\|STATUS_IMMUNITY | CRITICAL150_1IN32\|DOUBLE_1IN16\|COUNTER_1IN32 | ROBIN_ARROW48 | NOTHING, NOTHING, NOTHING, NOTHING |
| Pawn13 | STATUS_IMMUNITY | CRITICAL150_1IN8\|DOUBLE_1IN32\|COUNTER_1IN16 | BATTLE_AXE37 | NOTHING, NOTHING, NOTHING, NOTHING |

Each first item carries EQUIPPED128; all other three slots are NOTHING127.
Each learned NOTHING is spell63, distinct from item127. Row item NOTHING returns
before AddItem, retaining that copied inventory. It does not unequip these pieces.
The activation write preserves only copied `$F000`, combines the rotated/masked
zero filler/STARTING word, giving King`$1000`, Bishops`$3000`, all others`$2000`.
Copied BIT4/BIT5 and low activation/neutral bits do not survive this write.
The commandset remains its separate byte; regions pack to`$FF`, and Bishop131's
primary/secondary orders pack to`$8980`.

| Selected weapon | Range | Equip effects, including remaining slots |
| --- | --- | --- |
| FLAIL90 | 1–1 | INCREASE_ATT19, NONE0, NONE0 |
| POWER_STICK89 | 1–1 | INCREASE_ATT15, NONE0, NONE0 |
| POWER_SPEAR76 | 1–2 | INCREASE_ATT20, NONE0, NONE0 |
| ROBIN_ARROW48 | 2–3 | INCREASE_ATT21, INCREASE_CRITICAL1, NONE0 |
| BATTLE_AXE37 | 1–1 | INCREASE_ATT21, NONE0, NONE0 |

All five are exact WEAPON, useSpell NOTHING63. Refresh applies equipped effects
to enemies without ally class equip eligibility. Rook's critical nibble0+1 becomes
CRITICAL125_1IN32, retaining DOUBLE_1IN16/COUNTER_1IN32; its base prowess is not
the final prowess. The adjacent BEQ defect makes the first truncated5/4 ATT
adjustment run at every difficulty; SUPER applies a second. At difficulty0,
fresh effective ATT King/Queen/Bishop/Knight/Rook/Pawn is55/46/47/51/48/53 after
gear, rather than base ATT or damage. Enemy refresh does not add ally CURSE.
DEF/AGI/MOV remain their fresh bases absent other effects;161 is kept for turns.

## Zero regions, SWARM count and re-waiting

**Confirmed:** round preparation orders ActivateEnemies → region cutscene →
spawn selection → GenerateBattleTurnOrder. Activation scans128–159 and falls
through once for160; the [existing defect owner](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
retains that boundary. With count0, TriggerRegionsAndActivateEnemies returns
before geometry, tested-word writes or assigned activation. Region programs
select32/40 only. STARTING0 and absent-mode0 slots select no hidden/respawn wave.
Fresh flags90–105 reset, tested cache and AI memory have separate lifetimes:
ClearAiMemory resets48 last-target bytes to-1 and48 memory bytes to0.

All sixteen have15/15, so normal AI enters active handling without requiring
primary-active bit0. Before that normal path, StartAiControl checks:

1. Only enemy set15 takes SWARM. CurrentHP unequal to maxHP bypasses waiting.
   At full HP, list16/20/22 selects third pointer/table2 for22, indexed by
   `combatant &127`. Threshold0 bypasses counting.
2. Actual SWARM132/133/135/136/137/138/139/141/142 use thresholds1/3/1/3/6/6/1/3/3.
   Bishop131's table6 is never a gate for its HEALER4/3. The other six non-SWARM
   rows likewise bypass SWARM regardless of their table bytes.
3. CountDefeatedEnemies requests **ALLIES**, obtains12 and subtracts2. The body
   runs before DBF10..0: eleven currentHP tests, **128–138 inclusive**. Actual
   enemies139–143 and neutral158/159 are omitted. This is not16 tests, twelve
   tests, or the outcome helper's living/placed enemy count.
4. Threshold≤count proceeds normally; below threshold writes STAY and move-string
   first byte-1, then exits before cache clear, order promotion and commands.
   Every exit still clears temporary obstruction flags.

The count uses currentHP==0 alone, without X, maxHP, activation, membership,
sprite or completed death-effects checks. HP0 on139–143 contributes nothing;
HP0 on an unplaced counted slot contributes one. Fresh declared HP is positive,
so count0 would leave all nine full-HP SWARM actors waiting if that state reaches
their AI preparation. Seven ordinary actors can attempt their own streams.
Waiting actors still have round entries. HP0 actors skip individual control.

For a supplied-state counterexample, all five omitted enemies can haveHP0 while
128–138 remain positive: count0 still blocks a full-HP132. Conversely one counted
HP0 admits threshold1 actors132/135/139, three admit133/136/141/142, six admit137/138.
These are count premises, not a natural kill sequence or simultaneous wave.
Damage bypasses waiting only while HP differs from maxHP; healing back to full
can reintroduce waiting below threshold. No permanent activation latch is set.
Opponent control/status may select a different preparation path entirely.

Normal AI clears NEWLY_TRIGGERED_BATTLE_REGIONS before15/15 handling and dead
target promotion. SWARM wait occurs before this clear; neither zero-region
activation nor ClearAiMemory substitutes for it. Persistent flags, tested cache,
temporary obstruction cleanup and order mutation must not be merged into one
activation state or inferred from an unchanged screen.

## Exact streams, point0 and stored live targets

**Confirmed:** dispatch stops at the first command returning d1=0; nonzero tries
the next. Command numbers are parameters, not learned spell levels.

| Set /actual actors | Complete ordered stream | Preparatory pathfinding |
| --- | --- | --- |
| HEALER3/2 /130 | HEAL2, MOVE_ORDER4, SUPPORT, ATTACK3, MOVE2, STAY | Block non-movable |
| HEALER4/3 /131 | HEAL2, MOVE_ORDER4, SUPPORT, ATTACK3, MOVE1, STAY | Block non-movable |
| ATTACKER2/7 /134,140,143 | MOVE_ORDER1, ATTACK1, HEAL1, SUPPORT, MOVE1, STAY | Regular |
| ATTACKER3/9 /129 | MOVE_ORDER3, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER4, MOVE2, STAY | Block non-movable |
| LEADER14 /128, pointer aliases stream13 | MOVE_ORDER5, ATTACK2, HEAL3, SUPPORT, MOVE_ORDER2, MOVE3, STAY | Block and carve |
| SWARM15 /nine admitted actors above | MOVE_ORDER1, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER2, MOVE2, STAY | Regular |

HEALER1/0 appears only on placement templates; it is not installed on actual
allies. Automatic allies use ATTACKER1/6 through their separate source branch.
Preparatory restrictions run before the ordered command stream. MOVE_ORDER1/2/3/4/5
use respectively targetType/pathMode0/0,2/2,1/1,0/1,0/2. Thus HEALER4's later
MOVE1 fallback differs from HEALER3's MOVE2 even though both begin with the same
healing/order commands and block-non-movable preparation.

King's bit6 selects the declared point0`(15,5)`; the low nibble selects its two
bytes. The point is valid and equal to the fresh position. Battle18's absent-point
read does not apply. LEADER's block-and-carve prepass still centers on the order;
being at point0 does not promise immobility, a physical attack or a first turn.
Live bytes`$80/$89/$88/$84/$86` instead read **current**128/137/136/132/134 X/Y,
initially`(15,5)/(17,6)/(15,6)/(11,5)/(9,5)`. They are not AI points, physical
actor indices or immutable formation destinations.

Normal preparation promotes secondary to primary and clears secondary when the
current primary follow target hasHP0. Bishop131 can change137→128; if that new
target is also dead the order command rejects it, with a subsequent normal call
able to promote NONE. Other selected live followers promote NONE. SWARM waiting
delays this mutation; MOVE_TO is not checked as a live target.

MoveOrder rejects MOV0, NONE and dead live targets. Its type0 branch uses the
returned X via `tst.b`, so X0 and-1 have the source-specific outcomes retained
by the accepted mechanism. A successful order may attack internally or move-and-
STAY, short-circuiting later HEAL/SUPPORT/ATTACK. A failed command may still apply
its path restrictions. Live positions, terrain/costs, current MOV, occupancy,
station search, path construction and prior command success remain conditions.

## Learned spells, actual usability and candidate-local HEAL

**Confirmed:** the selected active spells all carry AFFECTEDBYSILENCE. Learned
slots, available spell levels, AI command selection and later effect execution
are different consumers. King learns BLAZE2, Queens FREEZE2, Bishops HEAL3/BLAST2;
other pieces have no learned spells despite Knight/Pawn ENHANCED metadata.

| Selected definition | MP /range /radius /declared power | Animation |
| --- | --- | --- |
| BLAZE2 | 6 /1–2 /1 /10 | BLAZE\|VARIATION2 |
| FREEZE2 | 7 /1–2 /1 /12 | FREEZE\|VARIATION2 |
| BLAST2 | 5 /1–2 /1 /8 | BLAST\|VARIATION2 |
| HEAL3 | 10 /0–3 /0 /30 | HEALING_FAIRY\|VARIATION3 |

Attack lookup selects TYPE_ATTACK and, for confused/ally casters, the explicit
whitelist including BLAZE/FREEZE/BLAST. GetHighestUsableSpellLevel uses the proper
rank shift6 and currentMP: level1 alternatives cost2/3/2 respectively. Current
MP, reachable lists, physical/spell/item priorities and category RNG, stations
and earlier commands still decide whether an attack is returned. HEAL is TYPE_HEAL,
not an offensive candidate. SUPPORT does not turn these learned attack/heal
spells into DISPEL or MUDDLE2; neither support spell is learned by a chess piece.

ENHANCED occupies the copied class/spell-power byte used by GetClass and
AdjustSpellPower. For an actual CAST_SPELL, its promoted-class comparison applies
truncated5/4 power before resistance/critical/damage processing. Thus declared
BLAZE2/FREEZE2/BLAST2 powers10/12/8 become12/15/10 at that adjustment seam, not
fixed inflicted damage. This attack consumer is not proof that every spell or
healing amount is multiplied, nor that any selected spell was performed.

Bishops' HEAL2 command is not a HEAL2 learned slot. Nonconfused healing initially
requires3MP for HEAL; candidates must live and satisfy3×HP≤2×maxHP. Heal priority
13 belongs to CRITICAL13/LEADER14, **not SWARM15** despite the source comment.
King128 therefore has13; Bishops12, Queens11, Rooks5 and regular Knight/Pawn2.
Sorting, current injury and a reachable station remain necessary.

A new healing command reads learned HEAL3 into local`$80`. For one candidate
with deficitM, the helper incorrectly forms `incoming + (rank<<5)`, then converts
returned rank1 to0. On current`$80`, M≤2 fails; M3–14 checks`$80`/HEAL3 cost10
and returns HEAL1 atMP≥10. M≥15 first checks`$C0`/HEAL4 cost20, then`$A0`/absent
ATLAS3 (getter defaults to HEAL1 cost3), then`$80` as needed: atMP≥3 it yields
HEAL1 for3–19MP or HEAL3 at≥20MP. The caller promotes a returned HEAL1 to HEAL2
for a nonself target atMP≥11 only if the **current local rank** is≥2. Actual
casting cost/range follows the selected spell, not the erroneous checked key.

The caller writes selected local rank **before** station selection. If that
station fails, the next candidate keeps the modified local without rereading
learned HEAL3. Incoming`$40` selects HEAL1/check5MP forM>2, fails atMP3/4, has no
nonself promotion, and writes`$00` before its station test. Incoming`$00` selects
HEAL1/check3MP forM>2 without promotion. Helper failure retains the prior byte;
failed attempts spend noMP. A new independent command reads learned`$80` again.
Fresh Bishop36MP therefore does not guarantee HEAL3 on a later candidate. The
[corrected Battle19 table/counterexample](./battle19-initialization-and-first-control.md#learned-heal3-blast2-and-freeze2)
retains exact helper and local-entry provenance; no permanent unlearning is implied.

Item NOTHING in formation does not prove no item admissibility work. All copied
weapons have useSpell63. IsItemUsableInBattle compares against255, so its carry
alone can say usable; the spell getter defaults missing63 to HEAL1. Attack-item
filter then rejects its TYPE_HEAL, while healing-item selection requires Healing
Rain or AI-use bit13, neither present here. Equipped bit7 alone does not admit a
healing item. No chess weapon supplies an automatic attack/heal item action from
its name, equip effect or that defective carry result.

## Two neutral actors and the first conditional dispatch

**Confirmed:** PositionBattleEntities clears old actors/lookups and declares
ordinary actors for nonnegativeX/Y, excluding neutral-marked enemy slots, without
a separateHP test. WithN declared allies, sixteen fresh enemies occupy physical
N..N+15, using King/Queen/Bishop/Knight/Rook/Pawn mapsprites153/135/144/78/125/77.
Type89, combatant128, sprite153 and physicalN are different identity domains.
Exploration chess NPCs137–152 and Caravan/ORB actors are not retained as members.

After ordinary scans, F522 clear admits the two complete neutral rows; set skips
the tail. Each writes max/currentHP0, status0, activation OR8, X/Y and an actual
physical ENTNUM via DeclareNewEntity. Fresh absent initialization makes activation8.

| Declared state slot | Position /sprite /facing/action | Physical cursor | Lookup write |
| --- | --- | --- | --- |
| 159 Ponpei | `(6,2)` /KING_PONPEI188 /Down3 /eas_Standing | N+16 (28 ifN=12) | predecrement31/B at`$FFB15F` |
| 158 Maid | `(5,3)` /MAID208 /Down3 /eas_Standing | N+17 (29 ifN=12) | predecrement30/A at`$FFB15E` |

The declaration supplies layer0, clears velocity/travel/acceleration state,
writes FLAGS_A word`$E040`, and seeds animation/wait counters from the physical
cursor (animation counter then increments). In `data/scripting/entity/eas_main.asm`,
eas_Standing sets speed32/32, acceleration factors0/0, acceleration/deceleration
off, entity-obstructable off, map-uncollidable on and entity-uncollidable off,
then loops wait1/branch. These are requested actor flags/actions, not measured
motion, collision or elapsed time; combatant activation8 is a different field.

ENTITY_INDEX_LIST starts`$FFB140`, enemies`$FFB160`. Ordinary159/158 readers use
offsets63/62 (`$FFB17F/$FFB17E`), left-1 by the fresh skipped ordinary scan;
the neutral writes31/30 do not repair them. Physical ENTNUM remains the actual
cursor, not159/158 or lookup31/30. This repeats the existing owner's correct
distinction, without inventing a renderer repair. Ponpei is neither King128 nor
a joined ally. F522 set leaves fresh158/159 absent; on F88 load, skipping this
tail does not erase arbitrary saved neutral fields. No whole service audit follows.

Round generation clears64 sentinel entries, scans30 allies and enemy128–157,
admitting X≥0 and currentHP≠0 independently of activation and SWARM. Both neutrals
are outside that enemy range and haveHP0; SWARM's separate count excludes them
because its bounds end138. These are distinct reasons, not a generic sprite rule.
Fresh sixteen enemies supply17 entries: King161 yields ordinaryAGI33 plus extra,
while remaining AGIs are35,25,25,29,29,27,27,35,35,25,25,25,25,25,25.

Ordinary score is A + RNG(A>>3) − RNG(A>>3) + RNG(3) −1. King's extra uses
B=floor(33×5/6)=27 and B + RNG(B>>3) − RNG(B>>3), with no third draw. The extra
criterion is currentAGI≥128, independent of leader flag, set14, point0 and SWARM.
Range0 still advances the main seed. The stable signed-byte descending sort
keeps append-order ties and sets cursor0. Actual party/current stats and incoming
or earlier seeds determine order; neither King's name nor161 fixes first actor.

ExecuteIndividualTurn masks identity and skipsHP0. MUDDLE/AI-bit2 first selects
AI preparation; otherwise an enemy with opponent-control0 uses AI, nonzero uses
player preparation. Ally auto-battle is a separate selector. Cursor/view/windows/
grid preparation precedes SLEEP/STUN NoAction; preparation may mutate AI orders
before a status prevents action. Dispatch repeats status/control polarity before
AI execution or player input. Fresh source enemy statuses are0, but supplied
allies/saved state need not be. F88 can resume a different queue/cursor without
new generation. This boundary proves conditional dispatch only, not a fixed
first player-ready seam, accepted input, natural action or completed effect.

## Accepted production mechanisms and precise conversion limits

**Confirmed:** inspection at the accepted project base distinguishes descriptive
data from trusted admission, assembled capability and execution. No selected22
encounter or original observation is admitted merely because reusable mechanisms exist.

| Actual owner | Reusable behavior and selected22 limit |
| --- | --- |
| [prepare_encounter / compile_routes](../../src/sf2tool/remake_exploration_battles.py), [scene producer](../../src/sf2tool/remake_battle_scene_content.py) | Encounter producer admits2/3; scene producer1/2/3. Generic wildcard/400–450–500 flags and before/load/start phases exist. No22 export/route is admitted; Desktop scenes remain with their owners. |
| [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [PrivateBattleDefinitionReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) | Trusted source/digests and selected geometries bind1/2/3, rejecting22. Definition lookup uses ordinal ID and preserves raw unknownByte/spellPower, gear/spells/resistance/prowess descriptively; King raw0 is not a new key. Widening an allowlist does not establish converted capability. |
| [PrivateBattleScenarioReader.Assemble](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs), [PrivateBattleActionBindings](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Independent region-program admission1/2/3 rejects22. STARTING/NOTHING, sets7/9/14, NONE/live/valid point and REGULAR/HEALER exist. Sets2/3/15, MAGE/BRASS_GUNNER reject. King/Queen/Bishop/Knight/Pawn ENHANCED rejects REGULAR-only derived state; Queen/Rook BIT4 reject AI regex. King/Knight/Rook/Pawn base prowess reject; Queen/Bishop ordinary prowess can fit. Flail/Stick/Axe adjacent WEAPON effects can fit descriptively; Spear1–2 and Arrow2–3 reject range, Arrow critical effect rejects the equipment-effects binding. HEAL definitions bind healing; BLAZE/FREEZE/BLAST descriptions do not bind offensive execution. Controlled classes0/1/2/4 omit arbitrary natural parties. |
| [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) | Membership/resource/seed carry, formation, living/immortal healing and difficulty0 ATT/activation exist. Excess active members reject rather than source eviction. General gear/final-CURSE refresh remains limited; enemy loadout0/127 rejects copied equipped weapons. F88 rejects instead of restoring queue. Other difficulty/control policies, actual party/status/class producers and selected neutral load remain conversion limits. |
| [BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs), [BattleControlRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) | RegionProgram.None, zero-region/15/15, randomized and extra-round primitives are reusable. They do not implement the original HP-only128–138 count, omitted139–143, thresholds/re-wait or original cache/service lifetime. Generic control classification is not an observation of22. |
| [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs), [SourceMoveOrderAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) | Standby,6/7/9/14 streams, LEADER prepass, valid point and live-follow/dead-target promotion exist. Point0 fits the generic bounds; no Battle18 missing-point exception is needed. Activated AI rejects2/3/15, learned spells and loadouts beyond0/127; automatic allies reject. Current normal AI clears tested state before supported streams, so selected SWARM's earlier wait/count/re-wait requires explicit support. Exact15, HEALER3/4 fallback difference, candidate-local HEAL, attack/spell/item choices and selected gear remain gaps. |
| [ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs) and actual actor/adapter boundary | Generic standing/entity/services and logical projection do not bind22's two F522-gated neutrals, predecrement31/30 versus readers63/62, physical ENTNUM or saved-state load effects. No admitted22 content/adapter observation proves this composition. |

These are scoped conversion/support gaps, not a request for new infrastructure,
geometry, scene duplication or admission-only edits. Preserve the reusable point,
live-order, zero-region and round mechanisms when future implementation is authorized.

## Unknowns, retained failures and reproduction

**Unknown:** naturally supplied route/flags/party/classes/items/status/seeds,
HP-count transitions, targets/stations/paths, healing/attack choices, first actor
and usable input, damage/prowess, saved-state validity, completed services,
presentation/hardware timing and persistence. Static data/control flow closes
none of those runtime questions and creates no automatic acquisition queue.

Preserve [#1047's twelve completed unresolved comparisons](../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03),
the [control correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415),
intermediate/final-CURSE, [Battle12 HEAL/identity](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1234)
and [Battle13 neutral](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1236)
findings and their original failures. PR1245 initial`16f4233a` mechanical direct/CI
PASS missed candidate-local HEAL; its [independent P2](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1245#issuecomment-6366643680)
and accepted correction`c1893c7b` remain distinct. No completed failure becomes
interrupted, unavailable or semantically passed through this documentation.

Reproduce with complete spriteset22/macros/enums, six **ordinal** definitions,
selected item/spell definitions, exact globals and every named direct consumer.
Verify28 records/342bytes/point0; compare all16 table bytes to actual commandsets;
derive ALLIES12−2/DBF10..0 and substitute independentHP/X states. Follow full-HP
gate → wait or normal cache clear → promotion → prep restrictions → complete
streams → cleanup. Trace HEAL local`$80→$40/$00` across failed stations, item63
fallback/filtering, neutral lookup writes/readers/physical ENTNUM, King ordinary/
extra scoring and both control polarities. Review actual production rejection
and reusable mechanisms separately. Mechanical equality alone is not this audit.

Ignored reproduction receipts belong to `local/issue1250`: bounded selected-source
checks, semantic claim-to-consumer audit, document checks and configured clean-head
`uv run sf2 verify plan --base origin/main --head HEAD`. Use fresh receipt names;
inspect exact-head scoped CI. Direct checks cover UTF-8/LF, links/anchors/tables,
private boundary, exact five paths, unchanged retained metadata, fixture counters
and four frontiers. Normal/full suites, SDK/Godot/H3/H4, old comparisons and runtime
acquisition are outside this documentation acceptance. Independent main-gate
review/merge remains separate from the executor's frozen Draft PR.
