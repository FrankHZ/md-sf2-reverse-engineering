# Battle16 Kraken initialization, SWARM waiting and first conditional control

Evidence date: 2026-10-22. **Confirmed:** complete12/11/0/0 connects to actual
party/definition refresh, eight SWARM versus three ordinary actors, exact
HP-only128–138 count, live arm orders and first conditional control. No polygon
or fixed point is declared; Head has two round entries. **Inferred:** the
composition supports the intended Kraken encounter. **Unknown:** natural state,
first actor/actions, service completion, presentation and persistence. It is
static content, not observed play or an admitted production encounter.

## Agreement and provenance

Standard rigor under [Issue1238](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1238)
and [Epic1053's finite scope](../../story-content-coverage.md#scope-and-reading-rules).
The outcome is maintainable conversion-review content connecting the complete
selected formation, Kraken definitions, SWARM thresholds, live orders and actual
consumers through first conditional control. The composition of party lifetime,
counting and command dispatch justifies Standard rigor. It is static research;
natural reach, performed actions and playable implementation remain outside scope.

Owned tracked paths are this document, `story-content-coverage.md` and
`source-coverage.md`, with only Battle16 consistency in the latter two. The
read-only ownership check found no competing writer or stacked dependency.
Accepted base is `81c713ad348cf1233b21da06a8509213de42cfa5`, tree
`4dc105f32d7cbf41a1891742c18c80eeb370717b`. PR692 is unconsumed. No fixture,
schema, registry, counter, code, allowlist, design or runtime frontier changes.
No acquisition, emulator, screenshot, comparator, lowering or implementation.

Original paths resolve under `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin; the
[USA baseline](../../../../README.md#pinned-original-baseline) retains input identity.
No fresh ROM-equivalence or runtime claim follows. Short battle-code paths use
prefix `code/gameflow/battle/`; `j_` aliases resolve to the named implementation.
Actual operands/opcodes and branch polarity control over comments and names.

Reuse the complete [entry and introduction](./south-parmecia-through-kraken-battle16-entry.md),
[outcomes/Hassan](./battle16-outcomes-through-hassan-entry.md),
[raft boarding](./polca-raft-boarding-through-south-parmecia-entry.md),
[Luke recruitment](../eastern-parmecia/polca-departure-through-luke-recruitment.md),
[actual-party lifetime](../grans-opening/battle02-initialization-and-first-control.md),
[Peter/neutral distinctions](../eastern-parmecia/battle13-initialization-and-first-control.md) and
[accepted AI semantics](../../battle/battle-ai.md). Full intro, vehicle and graphics-service
research is not duplicated. Battle14/15 keep their separate owners.

Acceptance is complete selected source/consumer review, UTF-8/LF/link/anchor/
private/exact-three-path checks, unchanged counters/four frontiers/retained input
metadata, clean committed planner and exact-head scoped CI, then clean pushed
frozen Draft PR, Review / Verify and independent main-gate acceptance. Small
linear text/arithmetic receipts are estimated below64 MiB and one minute; no
retained payload is parsed, copied or regenerated. New output is isolated in
`local/issue1238/`. Replan only for a concrete material scope/resource expansion.

## Selected source and consumer map

| Claim | Selected source and immediate consumer |
| --- | --- |
| Complete12/11/0/0 | `data/battles/spritesets/entries.asm` slot16 → `spritesets/spriteset16.asm:BattleSpriteset16`, `0x1B42DA..0x1B43F2` (exclusive end); `sf2macros.asm:allyCombatant/enemyCombatant/combatantAiAndItem/combatantBehavior`; `getbattlespritesetsubsection.asm:GetBattleSpritesetSubsection`, `0x1B1630..0x1B169E`; `sf2enums.asm`. |
| Map/globals/lifetime | `code/common/maps/getbattle.asm:CheckBattle`; `code/gameflow/mainloop.asm`; row16 of `data/battles/global/battlemapcoords.asm`, `custombackgrounds.asm`, `enemyleaderpresence.asm`; complete `battleneutralentities.asm`, `halvedexpearnedbattles.asm`, `afterbattlepositions.asm`; `data/battles/terrainentries.asm` slot16; `battleloop_1.asm`, `battlefunctions/loadBattle.asm`; cutscene before/start tables and wrappers. |
| Party/refresh/entity | `battleloop/heallivingandimmortalallies.asm`, `initializecombatants.asm`, `0x1B1272..0x1B15F8`; `code/common/stats/battleparty.asm`, `updatecombatantstats.asm`; `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities/DeclareNewEntity`, `getcombatantmapsprite.asm`; `data/stats/enemies/enemymapsprites.asm`, `enemybattlesprites.asm`; `battlefunctions/battlefunctions_0.asm:GetEntityIndexForCombatant`. |
| Definitions/spell/drop | `data/stats/enemies/enemydefs.asm` complete records10/59/87; complete `data/battles/global/randombattles.asm`, `enemyitemdrops.asm`; `battleloop/upgradeenemies.asm:UpgradeRandomBattleEnemies/IsBattleUpgradable`; `InitializeEnemyStats/InitializeEnemyItems/AdjustEnemyBaseAttForDifficulty`; `data/stats/spells/spelldefs.asm:AQUA`; `ai/getnextuseableaiaction.asm`, `gethighestusablespelllevel.asm`, `command/attack.asm`, `command/attack/prioritizetargets.asm`, `command/attack/determinebattleaction.asm`, `command/heal.asm`, `command/support.asm`; `battlefield/getactionrange.asm`. |
| Activation/SWARM/orders | `battleloop/activateenemies.asm`, `triggerregions.asm`, `populateenemyspawns.asm`, `clearaimemory.asm`; `data/battles/cutscenes/regionactivatedcutscenes.asm`; `ai/startaicontrol.asm:StartAiControl/CountDefeatedEnemies`, `0xDEFC..0xE1AC`; complete selected `data/battles/global/swarmbattles.asm:table_SwarmAiEnemyCounts0`, `0xE25B..0xE294`, and `aicommandsets.asm`, `0xE1AC..0xE25B`; `ai/executeaicommand.asm`; `code/common/stats/combatantstats_1.asm:GetMoveOrders/GetTriggerRegions`; `battlefield/getmoveorderposition.asm`, `0x1B15F8`. |
| Movement/turn/control | `ai/command/moveorder.asm`, `0xE98C..0xEBA4`, `moveorder/buildmovestringformoveorder.asm`, `move.asm`, `0xF1D4..0xF522`; `battlefield/getcurrentterraintypeformoveorder.asm`, `determineattackpositionformoveorder.asm`, `battlefieldengine.asm`; `data/battles/global/krakenmovecosts.asm`, `0xF9B4..0xF9C4`, `landeffectsettingsandmovecosts.asm`; `battleloop/turnorderfunctions.asm`; `battlefunctions/executeindividualturn.asm`, `0x23EB0..0x24242`. |

The [spriteset](../../battle/battle-spriteset-data.md), [routing](../../battle/battle-routing-data.md),
[global](../../battle/battle-global-data.md), [loop](../../battle/battle-loop.md), [AI](../../battle/battle-ai.md)
and [cutscene](../../battle/battle-cutscenes.md) executable inventories retain their own
limits. `rowContentTracked:false` and `routeTargetsParsed:false` do not change;
this composed prose is not a new Battle16 executable fixture or observation.

## Map49 and fresh versus suspended lifetime

**Confirmed:** the sole Map49 admission row selects battle16, wildcard trigger
`(255,255)`, output crop`(0,0,30,30)`, gated by F416. The crop is not a containment
test. F416 clear admits no16 even with F516 set. F416/F516 both set clears416
but still selects16 on this call. F466 controls intro/start and F88 suspension;
neither is the admission predicate. Raft mode/ownership, follower flags, Peter/
Luke membership and Battle14/15 completion are not predicates. The linked entry
owner retains the complete Map69 `(28,51)` → Map49 `(15,16)`, Up request and
its conditional pending/service boundary. Luke's prior scene supplies416 only
if reached; its join does not prove his active deployment.

Terrain slot16 selects `BattleTerrain16`, Stack-decoded into shared48×48
storage, separately from the30×30 operating crop. Background16 is RIVER14;
leader-presence16 is-1. The neutral table omits16, half-EXP selects only1 and
after-position overrides only5. Head128's HP/death-list terminal and all raft/
Hassan returns remain with the outcome owner; SWARM threshold5 is not the win
condition and commandset15 does not replace leader-presence semantics.

| Supplied state | Actual entry sequence |
| --- | --- |
| F88 clear/F466 clear | Clear PLAYER_TYPE/set399; clear seconds; save/install map49/battle16/base VInts; before16 runs the complete introduction; restore/reassert map/battle; clear90–105; heal; initialize actual allies and eleven enemies; clear AI memory; `LoadBattle(0)` rebuilds entities/map/terrain; start wrapper sets466 before empty start16; enter round preparation. |
| F88 clear/F466 set | Skip before and start bodies; fresh healing, formation/definition refresh, load and new round still run. F516 is separate. |
| F88 set | Clear PLAYER_TYPE/set399; restore saved seconds; clear88, AI memory and view target; load using incoming focus; enter retained queue/cursor directly. No fresh map/battle installation, hooks, healing, formation/definition refresh, region reset or new round precedes that loop. A retained sentinel later starts a round. |

Before16 points to `bbcs_16`; start16 selects `ms_Empty`, whose terminator may
still wait for scrolling and clears scroll speed. The normal before-scene's
map load, clone links, hidden limb actors, reveal/action/graphics/text waits and
raft normalization are requests with their own completion dependencies. They
are not combatant initialization. `LoadBattle` clears/rebuilds field actors from
placed combatants; intro follower/physical identities and clone links are not
copied into stats, membership or combat orders. Saved-state consistency is
required on the F88 path, not repaired by the new incoming warp tuple.

## Twelve placement templates and actual party

**Confirmed:** header4 +23 combatant records×12 =280 (`0x118`) bytes, matching
`0x1B42DA..0x1B43F2`. There are no region or point bytes. All twelve ally rows
have HEALER1/0, NOTHING127, NONE255/NONE255, regions15/15, filler0 and STARTING0.
Their complete ordered local coordinates are:

| Template | X,Y | Template | X,Y |
| --- | --- | --- | --- |
| 0 | `(15,15)` | 6 | `(15,16)` |
| 1 | `(15,17)` | 7 | `(14,13)` |
| 2 | `(14,18)` | 8 | `(15,18)` |
| 3 | `(13,17)` | 9 | `(13,18)` |
| 4 | `(13,14)` | 10 | `(15,13)` |
| 5 | `(15,14)` | 11 | `(13,13)` |

Healing scans all30 allies independently of membership; living allies and
immortal Peter7/Lemon28 receive max HP/MP, ordinary dead allies remain dead.
`status & 7` is intermediate STUN/POISON/CURSE; subsequent stat refresh retains
STUN/POISON and recomputes final CURSE from equipped cursed items. Neither a
supplied CURSE bit nor healing alone proves final curse or usable control.

`InitializeAllAlliesBattlePositions` sets all30 Xs to-1, calls `UpdateForce` to
rebuild ascending joined/active lists from flags0–29/32–61, and reads identities
from `BATTLE_PARTY_MEMBERS`. It copies only template X/Y for living members;
IDs, AI, items, spells and behavior fields are not copied. A dead member consumes
a list iteration but no position row. Twelve iterations still bound capacity;
a remaining active tail loses active flag/X via `LeaveBattleParty`, retaining
joined. Dead-created spare positions do not enlarge that capacity. The zero
formation-count guard is not a pre-loop empty-active-party guard.

Peter can be revived by immortal healing yet remain reserve/unjoined/unplaced.
His earlier Volcanon selector0 AI-bit2 clear remains conditional on reaching
that producer; intro actions neither clear that bit nor guarantee a player turn.
Luke's separate prior `JoinForce(10)` sets joined10 and capacity-dependent active42;
F75 following and the scene lookup are independent. Already-set42 is not cleared
by a full-capacity join, joining does not revive ordinary dead Luke, and F416
alone proves neither join nor deployment. No fixed twelve heroes, mandatory
Peter/Luke, neutral Luke or separate raft combatant follows from these rows.
The earlier Battle13 neutral159 belongs to that encounter, not this formation.

## All eleven enemy rows and three complete definitions

**Confirmed:** all selected rows add NOTHING127, have secondary NONE255,
regions15/15, filler0 and STARTING0. Fresh initialization scans128–159;
slots128–138 initialize immediately, without the spawn-occupancy check, and
undeclared139–159 get max/currentHP0 and X=-1. Battle32's Jaro exception does
not apply. Full row operands and the slot-indexed threshold table are:

| Row / slot | Type / local X,Y | Commandset | Primary order | Threshold |
| --- | --- | --- | --- | --- |
| 0 /128 | KRAKEN_HEAD87 `(26,9)` | SWARM15 | NONE | 5 |
| 1 /129 | KRAKEN_ARM59 `(25,17)` | SWARM15 | FOLLOW_ENEMY7 (`$87`) →135 | 4 |
| 2 /130 | KRAKEN_ARM59 `(6,11)` | SWARM15 | FOLLOW_ENEMY3 (`$83`) →131 | 2 |
| 3 /131 | KRAKEN_LEG10 `(6,15)` | SWARM15 | NONE | 2 |
| 4 /132 | KRAKEN_LEG10 `(7,18)` | SWARM15 | NONE | 3 |
| 5 /133 | KRAKEN_LEG10 `(10,20)` | ATTACKER1/6 | NONE | 0 |
| 6 /134 | KRAKEN_LEG10 `(21,11)` | ATTACKER1/6 | NONE | 0 |
| 7 /135 | KRAKEN_LEG10 `(20,15)` | SWARM15 | NONE | 4 |
| 8 /136 | KRAKEN_LEG10 `(23,20)` | SWARM15 | NONE | 1 |
| 9 /137 | KRAKEN_LEG10 `(13,5)` | SWARM15 | NONE | 3 |
| 10 /138 | KRAKEN_LEG10 `(16,26)` | ATTACKER1/6 | NONE | 0 |

This is eight SWARM actors (head, two arms, five legs) and three ordinary legs,
not eleven SWARM actors. Threshold entries for133/134/138 are present but their
set6 bypasses the SWARM gate entirely. These are one head/two arms/eight legs;
scene actor counts do not establish the rebuilt battle identities.

`InitializeEnemyStats` selects by declared enemy ID after the upgrade helper,
copies the full definition, fills HP/MP, sets coordinates/mover+commandset,
actual enemy ID, regions/orders/items, then activation/difficulty/stat refresh.
All three definitions use spellPower REGULAR and initialStatus NONE. Complete
stat/slot operands are:

| Definition | unknownByte; level; HP/MP | Base ATT/DEF/AGI/MOV | Four items / four spells | Mover; base AI word |
| --- | --- | --- | --- | --- |
| Leg10 | 10;14;24/0 | 31/26/19/6 | NOTHING×4 / NOTHING×4 | AQUATIC7; PRIORITYMOD_2\|BIT5 (`$2020`) |
| Arm59 | 59;16;30/0 | 34/28/22/6 | NOTHING×4 / NOTHING×4 | AQUATIC7; PRIORITYMOD_2\|BIT4 (`$2010`) |
| Head87 | 0;18;35/0 | 40/29/153/5 | NOTHING×4 / AQUA1, NOTHING, NOTHING, NOTHING | AQUATIC7; PRIORITYMOD_1 (`$1000`) |

Head's `unknownByte=0` is not its enemy ID and does not select definition0 or100.
The enum/placement/copied enemy-index consumer selects87. Leg resistance is
LIGHTNING_WEAKNESS|ICE_WEAKNESS|FIRE_MINOR; Arm0; Head adds STATUS_MAJOR to the
Leg set. Prowess is Leg CRITICAL150_1IN4|DOUBLE_1IN16|COUNTER_1IN16, Arm
CRITICAL150_1IN8|DOUBLE_1IN32|COUNTER_1IN32, Head
CRITICAL150_1IN4|DOUBLE_1IN8|COUNTER_1IN8. These are definition operands for their
actual damage/status/follow-up consumers, not performed effects or generic
remake support.

The complete random-battle list omits16, so `UpgradeRandomBattleEnemies` returns
the original ID before level/movement/substitution/random work even with F516
set. The Leg/Arm exclusions elsewhere in that file do not enable16. Separately,
the complete special-drop table omits16. No equipped gear exists in these base
records and placement NOTHING returns before AddItem/equip; neither absence is
inferred from the other table. Difficulty still applies: the two BEQ instructions
in `AdjustEnemyBaseAttForDifficulty` make every difficulty multiply ATT by5/4,
flooring; SUPER applies it twice. Leg31→38 (SUPER47), Arm34→42 (52), Head40→50
(62), before ordinary refresh. Init preserves only definition priority bits12–15
and combines STARTING/filler0: fresh words are limbs `$2000`, Head `$1000`.
The copied BIT4/BIT5 are not retained as fresh activation bits, nor is15 stored
in that word; commandset is a separate low nibble in mover/AI storage.

### Attack ranges and AQUA at the selected boundary

`GetAttackRange` gives unarmed Leg1–1, Arm1–2 and Head1–3. The AI physical action
construction separately uses Leg0–1, Arm1–2 and Head1–3 after `GetCombatantType`
byte comparisons; do not silently make these two min-range paths identical.
The accepted AI owner retains candidate scoring/position and the Claude alias
bug; no alternate definition or new general ranged algorithm is inferred here.

Head's only learned spell is AQUA1 (base40/`$28`): MP0, BUBBLE_BREATH,
TYPE_ATTACK, range1–3, radius0, power13. AQUA2 exists separately but is not
learned by this definition. The unconfused-enemy attack-spell filter admits
AQUA1 and `GetHighestUsableSpellLevel` accepts its zero MP cost with currentMP0.
A confused caster uses the six-spell allowlist, which excludes AQUA. Reachable
physical/spell/item candidate lists and priorities precede category
selection; the selected action then constructs its final position/path.
With physical plus AQUA and no item, the special AQUA branch chooses the spell
without the usual RNG6 roll; AQUA alone is likewise usable. This does not prove
an attack/cast target or first action. None of these records has healing/support
spells or held items; HEAL2/SUPPORT in the stream do not create abilities.

## Zero regions, exact defeated count and SWARM waiting

**Confirmed:** round preparation calls `ActivateEnemies`, sparse region program,
spawn selection, then turn generation. The activation loop scans128–159 and
falls through once for160 because the outer RTS is absent; the
[existing extra160 owner](../northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
retains that defect. `TriggerRegionsAndActivateEnemies` sees region count0 and
returns **before** polygon tests, tested-word writes or assigned-region activation.
There is no triangle sweep or manufactured flag hit. The region-program table
contains only32/40, not16. STARTING mode0 declares no hidden/respawn candidate;
fresh undeclared slots have mode0 too, so the spawn selection is empty.

15/15 means no assigned region, not disabled AI. After SWARM admission, normal
AI bypasses inactive standby directly to commandsets; it need not set activation
bit0. SWARM uses its own earlier gate in `StartAiControl`:

1. Only enemy set15 enters it. HP unequal to maxHP immediately takes normal AI.
2. Full HP scans the three-battle list16/20/22. Match16 selects pointer0, then
   table byte indexed by `combatant & $7F`, giving the eleven thresholds above.
3. Threshold0 goes directly to normal AI without counting. Otherwise call
   `CountDefeatedEnemies`; `cmp.b d1,d0; ble` enters normal AI when threshold
   is **at most** count. Count0–11 and thresholds1–5 fit signed-byte comparisons.
4. Below threshold writes action STAY and first move-string byte-1, then exits.
   Damage is a currentHP!=maxHP bypass, not a permanent activation latch: a later
   restoration to exactly full HP can reapply the wait if still below threshold.

The count helper really requests ALLIES, gets12, sets `d2=12−2=10`, starts
`d0=128` and executes its body before DBF. Values10 through0 therefore give
**11 iterations over128–138 inclusive**, exactly the eleven declared enemies
here. The generic wrong-subsection comment does not imply extra dead slots in
Battle16. Undeclared139–159 and allies never contribute to this count. It tests
**currentHP==0 only**: X/placement, active flags, joined/party lists, death lists,
region bits, sprite presence and maxHP are not consulted. A supplied living
unplaced enemy in that range is not counted; HP0 contributes without a completed
death animation. This is not `CountRemainingCombatants`'s different HP/X rule.

Fresh definitions set all eleven to positive full HP, so the first fresh count
is0. If those values are unchanged when AI preparation is reached, all eight
SWARM actors wait;133/134/138 instead try set6. Each later SWARM call rechecks
current HP/count: full-HP136 is admitted at count1,130/131 at2,132/137 at3,
129/135 at4 and128 at5. These are conditional thresholds, not a guaranteed
kill order, wave spawn, same-time transition or first enemy action. HP0 actors
are skipped before AI dispatch. Nonzero opponent control can bypass normal
enemy AI entirely, so a table row is not itself an observed wait.

All `StartAiControl` exits clear temporary terrain obstruction bits, including
SWARM waits. The wait exits **before** normal AI's clear of
`NEWLY_TRIGGERED_BATTLE_REGIONS`, order promotion and stream work. Normal paths
clear that tested word, separately from persistent90–105. `ClearAiMemory` resets
48 last-target/memory bytes, not that word; zero-region activation never needs
the polygon cache. Do not assert that fresh flag reset, swarm STAY or the memory
clear performs normal-path cache clearing. No thinking-RNG standby movement
is entered merely because SWARM waits.

## Complete streams, live arm orders and movement

**Confirmed:** commands are attempted in order; d1=0 stops with success, nonzero
advances. Actual enemy streams are:

| Set / actors | Complete stream | Preparatory pathfinding |
| --- | --- | --- |
| SWARM15 /128–132,135–137 after admission | MOVE_ORDER1, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER2, MOVE2, STAY | Regular |
| ATTACKER1/6 /133,134,138; AI allies independently | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY | Regular |

HEALER1/0 belongs only to ally templates and is not installed by placement.
An actual AI-controlled ally uses set6 irrespective of that declaration.
Set15's MOVE_ORDER1 is target-type0/regular; MOVE_ORDER2 is type2/block-and-carve;
MOVE2 adds block-non-movable. Set6 has no MOVE_ORDER. Numbered HEAL/ATTACK/MOVE
commands are command parameters, not spell levels or separate learned spells.

The primary live orders on129/130 are `$87`/`$83`. Bit6 is clear, so
`GetMoveOrderPosition` reads current enemy135/131 X/Y, initially`(20,15)`/`(6,15)`.
They are not fixed points7/3, allies Peter7/Slade3, physical slots, intro clone
sources or immutable coordinates. There are zero fixed points to select.
Both ordered commands in set15 can consume those stored arm orders: type0
chooses a code path, while the stored bit6 still chooses the live referent.
The declaration therefore has a real consumer, without guaranteeing following.

Before command dispatch, normal `StartAiControl` tests primary live-target HP;
HP0 promotes secondary NONE to primary and clears secondary, leaving both NONE.
SWARM wait delays this mutation. The command itself rejects MOV0, NONE and dead
live targets; NONE actors fail the first ordered command and continue to attack.
Point-path `DetermineAttackPositionForMoveOrder` tests current/adjacent stations
and the caller uses `tst.b` on returned X: X0 fails; the-1 failure value is
nonzero and can continue. Do not reinterpret that as a correct sentinel check.
It applies quadrant markings, tries ATTACK1, then builds movement if needed.
The follow-type fallback calls `GetCurrentTerrainTypeForMoveOrder`, which returns
-1 **when current terrain bit7 is set**,0 otherwise; the caller continues on
nonzero. Its saved target-type parameter is unused. Temporary obstruction state
and earlier failed-command post-processing can therefore affect that fallback.

The move-order builder uses the regular AQUATIC move-cost profile, reverse
budget128, preliminary MOV×2, then acceptable station radii0/1/2/3. AQUATIC's
ordinary profile obstructs terrain0–7 and9–15 and permits water8 at cost2.
It does **not** use the special Kraken cost table. Movement-only success is
STAY plus a nonempty move string; exhausted paths can keep failure and advance
through later commands. Block-and-carve is command-local post-processing even
though set15's preparatory mode is regular; top-level exit removes temporary bits.

If ordinary MOVE1/2 is reached, `aiCommand_Move` initially collects living,
nonnegative-X/Y opponents for an unconfused actor, scores/sorts unsigned byte
movement costs, and uses the accepted class-neighbor ordering where applicable.
Its confused path directly chooses its own side's first index without the same
HP/position validation; the ordinary empty-target-list guard is absent. The
selected actual target remains conditional on live state and thinking RNG.
For enemy definitions10/59/87, its later reverse search selects all16 bytes of
`table_KrakenMoveCosts` by terrain nibble:
`[15,2,2,3,4,3,3,-1,2,-1,-1,-1,-1,-1,-1,-1]`.
The initial15 is a cost, not a length prefix. This differs from ordinary water-
only AQUATIC movement and can guide an approach across other terrain. It builds
with reverse budget128 and a hardcoded preliminary move budget4, then rebuilds
the actual occupied movement grid and tests attack stations at radii0 then1.
Failure yields STAY/empty string; the command still returns success. Shared
scoring, terrain, occupancy and final-path limits remain at the AI owner. No
full path, pursuit, target or action simulation is performed here.

## Head sprite identity, round generation and control endpoint

**Confirmed:** `GetCombatantMapsprite` reads actual enemy ID into
`enemymapsprites.asm`: Leg10→74, Arm59→123, Head87→254. `PositionBattleEntities`
selects the special-sprite branch for Head: physical `ENTITY_SPECIAL_SPRITE=47`,
ENTNUM `ENTITY_ENEMY_START=32`, and writes physical47 into enemy128's ordinary
mapping at offset32. With crop0,0, initial head coordinates stay`(26,9)`.
It restores the ordinary physical cursor without incrementing it; arms/legs
consume ordinary slots after the actual placed allies. Their precise physical
numbers therefore depend on that party. `GetEntityIndexForCombatant(128)` uses
128−96=32 and reads47; special physical47/ENTNUM32 is not combatant47/32, a new
enemy definition, a different attack identity or a general mapsprite rewrite.
The separate battle-sprite table selects Head44/Arm33/Leg8 palette0. Neither
that selection nor rebuilding proves original rendering or service completion.

Turn generation clears64 sentinel entries, scans30 allies then128–157, and
admits nonnegative-X/nonzero-HP combatants independently of SWARM thresholds
and activation. All eleven fresh enemies qualify; waiting actors are in the
queue. Refreshed AGI is Leg19, Arm22 and Head153=`128+25`; only Head has the
extra-turn bit in these definitions. Its first score uses25, with the ordinary
`AGI + RNG(AGI>>3) − RNG(AGI>>3) + RNG(3) −1` rule. Its second uses
`floor(25×5/6)=20` plus/minus two RNG draws at20>>3. Thus the eleven enemies
supply12 entries on unchanged fresh state, not a guaranteed head-first round
or an exemption from checking SWARM again on each head turn. Actual allies and
supplied main seed still determine the stable signed-byte descending sort
(62 passes), append-order ties and first actor. Cursor becomes0. No selected
random-substitution draws precede this round. AI also has a separate thinking
RNG; this does not mean every AI draw uses it.

`ExecuteIndividualTurn` masks the selected actor and skips HP0. MUDDLE or AI
bit2 chooses AI preparation first. Otherwise an enemy with **opponent-control=0**
uses AI, nonzero chooses player preparation; ally auto-battle independently
chooses AI. Cursor/view/windows/grid work then precedes SLEEP/STUN NoAction.
The same MUDDLE/bit2/control polarity is rechecked before AI execution versus
player input. AI preparation may therefore have run before a status stops the
action. Stop at this conditional boundary, before performing movement/action,
damage or the leader terminal. Fresh roster, a threshold crossing, Head's two
entries, prior Peter bit-clear or Luke join does not prove a player-ready seam.
F88 can instead supply a retained queue/cursor and changed HP/positions/orders.

## Accepted production consumers and conversion limits

**Confirmed by accepted implementation inspection, separately from original
evidence:** Battle16 is not admitted/converted/played by the current private
route. [Trust](../../../../remake/docs/content/profiles-and-trust.md#selected-third-encounter)
and [Application](../../../../remake/docs/application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation)
retain selected2/3 consumption. No consumer is executed or changed here.

| Actual consumer | Reusable support and specific Battle16 boundary |
| --- | --- |
| [prepare_encounter / compile_routes](../../../../src/sf2tool/remake_exploration_battles.py) | Producer/CLI selects2/3. Wildcard admission,400/450/500 flags and before/load/start routing exist, but no16 encounter/export/intro conversion is admitted. [Scene producer](../../../../src/sf2tool/remake_battle_scene_content.py) also restricts1/2/3. |
| [PrivateBattleEncounterReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) / [PrivateBattleDefinitionReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) | Format can describe12/11/0/0, orders and scene metadata; trusted pins admit1/2/3 only. Generic code/ID lookup can select Kraken definitions/items/AQUA without a name prohibition. A retained description is not trusted16 conversion or execution. No random substitution or neutral construction is needed for this selected encounter. |
| [PrivateBattleScenarioReader.Assemble](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs) / [PrivateBattleActionBindings](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Independent region-program admission1/2/3 rejects16. STARTING/NOTHING, NONE/live orders and set6 bind; SWARM15 does not. AQUATIC mover is unsupported. Arm BIT4 fails the AI-word regex, while Leg BIT5/Head priority-only fit. All three prowess expressions reject. Unarmed physical binding always defaults1–1 and does not supply Arm1–2/Head1–3. AQUA is descriptive data, not supported attack-spell execution. |
| [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) / [BattleInitializationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) | Membership rebuilding, carried resources/seeds, already-refreshed roster/formation, living/immortal healing and difficulty0 ATT exist. Excess active members reject capacity instead of source eviction. General gear/CURSE refresh, natural Peter/Luke classes/join/control and other difficulty/control policies remain limits; F88 rejects rather than restoring saved queue/cursor. Enemy NOTHING fits existing loadout refresh; no new gear mechanism is implied by these three definitions. |
| [BattleActivationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs) / [BattleTurnFlow](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) / [TurnOrderRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs) | RegionProgram.None, mode0, zero regions, randomized/extra turns and15/15 need no new polygon mechanism. They do not implement the source's slot/HP SWARM count or waiting branch. No16 round/control observation validates composition. |
| [SourceEnemyAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs) / [SourceMoveOrderAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) / [BattleTerrainRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTerrainRules.cs) | NONE/live follow, dead-target promotion, temporary restrictions, station search and selected6/7/9/14 streams exist. Activated AI rejects15 and learned spells (Head AQUA); automatic allies reject. AQUATIC and the distinct Kraken cost profile, threshold/count/re-wait, exact15 stream and general ranged/spell AI are missing selected mechanisms. Definition resistance/prowess does not create damage/status/follow-up execution. New enemy names alone are not missing mechanisms. |
| [BattleControlRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) / [capability owner](../../../../remake/docs/capability-status.md) | Controlled admission exposes unsupported status/automatic/control policies. Existing party/round/control data does not provide arbitrary natural parties, saved-state restoration, Head special physical mapping/graphics services or a Battle16 adapter/content route. Prior intro/raft and outcome owners retain their own limits. |

These are conversion/support requirements, not an implementation request or
original-game evidence from remake code. Described, admitted, consumed and
naturally observed remain distinct.

## Unknowns, preserved failures and reproduction

**Inferred:** this selected composition supports the intended Kraken encounter.
**Unknown:** natural flags/route, joined/active/dead/capacity/AI/follower history,
actual Peter/Luke deployment, gear/classes/status/difficulty/seeds, saved queue/
orders/cache, targets/paths/tactics/first actor/action, full damage/spell/follow-up
behavior, service completion, rendered/audio timing, usable input and durable
persistence. These do not automatically authorize acquisition or implementation.

Accepted Battle01 admission/victory/stable-return/later-Down observations remain.
#1047's twelve **completed unresolved comparisons** remain at the
[verification owner](../../../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03),
not interrupted runs or passes. Preserve the intermediate-status/final-CURSE
and opponent-control polarity corrections and
[PR1221's two P2 findings](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415).
[Battle12](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1234)
retains its pre-stage scene-identity and HEAL-helper corrections;
[Battle13](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1236)
retains same-region134/136→135 versus138 and neutral mapping31/63 corrections.
No mechanical document PASS supersedes those semantic failures.

Reproduce all23 combatant records, zero region/point counts, table0's full11
threshold bytes and complete definitions10/59/87 at the pin. Join actual Map49
and16 selectors before selecting globals/hooks; inspect init/party/refresh and
special physical mapping separately from the complete intro owner. Follow
ALLIES12→subtract2→DBF10..0→128..138 and HP-only counting; compare fresh0,
threshold−1/equal/+1, damaged/rehealed actor, HP0 with either X, undeclared139
and a dead followed135/131. Check the complete15/6 streams, actual `$87`/`$83`
referents, type0 versus type2 terrain polarity, ordinary AQUATIC versus all16
Kraken costs, zero-cost AQUA/actual range/prowess, Head153/25/20 and conditional
control before performed action. Read every named producer/Content/Domain/
Application/AI boundary. These small selections are sufficient; no polygon
sweep, retained-payload parse or full simulation is needed.

The retained map-init/script payload pair totals18,285,979 bytes; it and
`local/issue1005/pair.txt` remain unchanged by length/mtime against the earlier
retained metadata. New small receipts live only in `local/issue1238/`, with
old results preserved. Direct document checks cover strict UTF-8/LF, links/
anchors with external references counted separately, exact three paths,
private boundary, unchanged fixture metrics/four frontiers and input metadata.
On a clean committed head load existing ignored selections in the same process:

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify plan --base origin/main --head HEAD
```

Record exact-head scoped CI, full semantic review, completed failures and stopped
process state in the Issue handoff. No normal/full/SDK/Godot/emulator/H3/H4 suite,
screenshot, extraction or comparison is selected for this documentation-only
boundary. Stop at frozen Draft PR and Review / Verify with successful main-gate
notification; independent merge, cleanup, archival and next task remain separate.
