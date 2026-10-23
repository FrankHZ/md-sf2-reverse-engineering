# Battle05 initialization, activation and first control

Evidence date: 2026-10-22. **Confirmed:** Battle05's complete6/8/1/1 selection
connects six placement templates to the actual party, equipped Galam enemies,
one initially untriggered region and conditional individual control. Both entry
scripts are empty; the arrest dialogue precedes battle in a field script.
**Inferred:** this is the castle escape encounter's opening combat arrangement.
**Unknown:** natural entry/party/status/RNG, the first actor, every action,
presentation/timing and persistence. Static composition is not a played escape.

## Scope and provenance

This is the **Standard**-rigor agreement for
[Issue1220](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220),
under [Epic1053's finite denominator](./story-content-coverage.md#scope-and-reading-rules).
The outcome supports content maintenance and conversion review: explain the
complete selected encounter through its first player/AI/no-action dispatch,
including changed valid party state. Reuse the
[Battle02 lifecycle](./battle02-initialization-and-first-control.md) and
[Battle03](./battle03-initialization-and-first-control.md)/
[Battle04](./battle04-initialization-and-first-control.md) differences. The
[captivity/admission owner](./battle04-to-battle05-content-route.md) keeps the
field scenes; the [outcome owner](./battle05-to-battle06-content-route.md) keeps
the defeated-position program, Jewel, Slade recruitment and onward escape.

Owned writes are this document and Battle05 narratives in the two coverage
owners. Source, consumers, fixtures, registries, counters and runtime frontiers
are read-only. No parser, fixture, schema, implementation, allowlist, platform,
extraction or acquisition is added. Acceptance is direct semantic/document/
private-boundary review, committed-head planner and actual exact-head scoped CI,
then a clean pushed frozen Draft PR for independent acceptance.

Original paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The selected checkout was clean at that pin. Accepted repository base is
`ff5f45afaef53a4c7ca626624c1688259783c147`. The
[USA baseline](../../README.md#pinned-original-baseline) and existing contracts
retain input identity; this account makes no fresh ROM-equivalence claim.
Read the complete selected functions and return tails; `j_` interfaces resolve
to the named implementations. Short battle paths resolve under
`code/gameflow/battle/`.

| Claim | Declaration and actual consumer |
| --- | --- |
| Complete formation and behavior | `data/battles/spritesets/entries.asm` slot5 → `spritesets/spriteset05.asm:BattleSpriteset05`, `0x1B35A2..0x1B365C`; `sf2macros.asm:allyCombatant/enemyCombatant/combatantAiAndItem/combatantBehavior`, `sf2enums.asm`; `getbattlespritesetsubsection.asm:GetBattleSpritesetSubsection`, `0x1B1630`. |
| Admission and metadata | `code/common/maps/getbattle.asm:CheckBattle`, `0x799C`; `code/gameflow/mainloop.asm:MainLoop`; `data/battles/global/battlemapcoords.asm` row5, `data/battles/terrainentries.asm` slot5, `global/custombackgrounds.asm`, `enemyleaderpresence.asm`, `halvedexpearnedbattles.asm`; `battleloop/loadbattleterraindata.asm`, `battlescenes/getbattlescenebackground.asm`, `battleactions/giveexpandgold.asm`. |
| Empty hooks and fresh/suspended life | Before/start wrapper start/end files under `cutscenes/`, corresponding tables under `data/battles/cutscenes/`; `code/common/scripting/map/ms_empty.asm` and `mapscriptengine_2.asm:ExecuteMapScript/loc_47234`; `battleloop_1.asm:BattleLoop`, `0x23A84`. |
| Actual party, refresh and entities | `battleloop/heallivingandimmortalallies.asm`, complete `initializecombatants.asm`, `0x1B1272..0x1B15F8`; `code/common/stats/battleparty.asm:UpdateForce/LeaveBattleParty`, `updatecombatantstats.asm`; `data/stats/enemies/enemydefs.asm` records2/3/56/67, `data/stats/items/itemdefs.asm` records33/45/72/86; `battlefunctions/loadBattle.asm:LoadBattle`, `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities`. |
| Action selection and orders | `data/stats/spells/spelldefs.asm:BLAZE`, `code/common/stats/spellstats.asm:GetSpellDefinitionAddress`, `itemstats.asm:IsItemUsableInBattle`; `ai/startaicontrol.asm`, `executeaicommand.asm`, `command/attack.asm`, `command/moveorder.asm`, `getnextuseableaiaction.asm`, `gethighestusablespelllevel.asm`; `data/battles/global/aicommandsets.asm`; `battlefield/getmoveorderposition.asm`; `code/common/stats/combatantstats_1.asm:GetMoveOrders/GetTriggerRegions`, `combatantstats_2.asm:SetTriggerRegions`. |
| Upgrade and region preparation | `battleloop/upgradeenemies.asm:IsBattleUpgradable/UpgradeRandomBattleEnemies`, `data/battles/global/randombattles.asm`; `battleloop/clearaimemory.asm`, `activateenemies.asm`, complete `triggerregions.asm`, `populateenemyspawns.asm`; both `cutscenes/regionactivatedcutscenes.asm` and its data table; `data/battles/global/battleneutralentities.asm`; both `ai/determineaistandbymovement_*.asm`. |
| Round and stopping boundary | `battleloop/turnorderfunctions.asm`, `0x25544`; `code/common/tech/randomnumbergenerator.asm:GenerateRandomNumber`; complete `battlefunctions/executeindividualturn.asm`, `0x23EB0..0x24242`. |

The shared [spriteset](./battle-spriteset-data.md),
[global](./battle-global-data.md), [routing](./battle-routing-data.md),
[loop](./battle-loop.md), [AI](./battle-ai.md) and
[cutscene](./battle-cutscenes.md) owners retain their inventories and observations.
Existing `tests/fixtures/h2/` contracts `battle-spriteset-data-static-v1.json`,
`battle-global-data-static-v1.json`, `battle-routing-data-static-v1.json`,
`battle-loop-static-v1.json`, `battle-control-static-v1.json`,
`battle-ai-static-v1.json` and `battle-cutscenes-static-v1.json` remain shared
evidence. `rowContentTracked:false` and `routeTargetsParsed:false` do not become
executable proofs of this selected composition. The table above supplies its
read-only source-review route.

## Selection, empty hooks and the three lifetimes

**Confirmed:** `BATTLE_GALAM_CASTLE=5` selects row5 (`0x7A59`): Map16,
origin`(0,14)`, area24×14, wildcard triggers255/255. `CheckBattle` tests map,
unlock and trigger coordinates in table order; Map16 has no competing battle
row. F405/F505 both set clears405 but still returns5 on that call. The area
is output, not an inside-rectangle test. F455 is the intro gate, F505 completion,
and F88 suspended entry; they are not interchangeable.

Terrain slot5 selects `BattleTerrain05`, a Stack-decoded48×48 terrain array
independent of the24×14 operating area. Background row5 is CASTLE8; unlike the
OVERWORLD sentinel in Battles02–04, the ordinary getter returns that fixed
background without terrain-based selection. Leader-presence row5 is0, so the
defeated wrapper does not perform its leader-death sweep. Commandset LEADER14
does not itself define victory. The separate Battle05 battlescene-return
position listener can zero enemy HP after128 dies with Bowie alive; its exact
caller boundary remains at the [outcome owner](./battle05-to-battle06-content-route.md#battle05s-defeated-consumer-and-outcomes).
The zero table must not erase that later consumer. Half-EXP selection matches
Battle1 only; outcomes are not reopened here.

| Supplied state | Selected behavior |
| --- | --- |
| F88 clear, F455 clear | Clear PLAYER_TYPE/set399, clear elapsed time, install incoming current map/battle for before, dispatch empty before, restore supplied map/battle/current fields, clear90–105, heal, initialize allies/enemies, clear AI memory, load focused on0, run empty start, then prepare a round. Before slot5 (`0x47A92`) checks455 without setting it; start slot5 (`0x47B36`) sets455 before dispatch. |
| F88 clear, F455 set | Both hook bodies skip. Fresh healing, placements, enemy refresh, loading and round preparation still run. Completion505 neither skips these steps nor supplies enemy upgrades. |
| F88 set | Restore saved seconds, clear88/AI memory/view target, load with incoming focus and enter the retained queue/cursor. No fresh current-field installation, region reset, healing, placements, hooks or new round precedes dispatch. A retained sentinel subsequently enters round preparation. Saved fields and queue are prerequisites, not demonstrated persistence. |

Both table entries select `ms_Empty` (`0x47A4E`), a terminator whose interpreter
can wait for scrolling and clears scroll speed. There is no
`entries/battle05/cs_beforebattle.asm` to interpret as the arrest scene.
The predecessor's Map16 `Map16_ZoneEvent1` supplies `EGRESS_MAP=16` only when
F505 is clear; `cs_521BA` supplies405 and the field arrest dialogue, then shares
the Map16 request tail with the F664-set branch. F664-set/F405-clear does not
rearm5. Battle05's empty hooks do not rewrite egress or recruit Slade.

`LoadBattle` fades, rebuilds combatant-derived entities, loads the current map,
mapsprites and terrain, then requests music/fade-in. `PositionBattleEntities`
clears/rebuilds the entity lookup from live coordinates, with no Battle05
neutral-table row. The preceding field's145/146/147 actors therefore do not
become combatants of those IDs. Combatants128–135 below and actual allies are
the new population; numeric6 in the header does not preserve six field actors.
After-battlescene reload and the Battle05-specific defeated-position listener
are later consumers owned by the outcome account, outside this first dispatch.

## Six templates and the actual force

**Confirmed:** four header bytes precede14 combatant records of12 bytes,
one12-byte region and one2-byte point:186 bytes total, matching the selected
half-open source range. Ordered ally templates0–5 place at`(3,5)`, `(4,5)`,
`(2,5)`, `(4,4)`, `(3,4)`, `(2,4)`. Each declares HEALER1/0, NOTHING,
NONE/NONE orders,15/15 regions, filler0 and STARTING0.

`InitializeAllAlliesBattlePositions` resets all30 ally X coordinates, calls
`UpdateForce`, and copies **only X/Y** from those templates to ascending actual
joined+active IDs0–29. Template IDs0–5 do not require Bowie through Slade;
template HEALER1 does not overwrite actual AI, inventory or spells. Slade's
following flag73, joined flag5 and active flag37 are separate. The earlier
tunnel makes him follow; the later after-program owns recruitment. Neither
six templates nor his field presence proves he enters this fight as a member.

Reuse the accepted
[member-scan/cursor rule](./battle02-initialization-and-first-control.md#joined-active-dead-and-capacity)
with a six-iteration limit. A dead ordinary member consumes a member iteration
but no template; a seventh active member leaves the active party even if an
earlier death left a template unused. Joined flags remain; the tail does not
rebuild the RAM lists again. Reserves stay unplaced. This is not scan-until-six-
living, and JoinForce's general capacity12 is a separate rule. The source has
no pre-loop zero-party guard; empty/corrupt entry is not a natural premise.

Healing scans all30 slots; ordinary HP0 stays dead, Peter7/Lemon28 always get
HP/MP restoration. For handled allies `status & 7` is intermediate:
`UpdateCombatantStats` preserves STUN/POISON and recomputes CURSE from equipped
cursed gear. An old CURSE bit alone does not establish final curse. Equipment
and STUN can change current movement/agility; caller values are not fixed by
the spriteset or by the earlier prison reset.

## Eight enemies, equipment and available action operands

**Confirmed:** every enemy has placement item NOTHING, secondary order NONE,
secondary region15, filler0 and STARTING0. These are all differing operands;
region15 is a sentinel, separate from order NONE255.

| Slot / combatant | Base type / position | Commandset | Primary order | Primary region |
| --- | --- | --- | --- | --- |
| 0 /128 | GALAM_KNIGHT3 `(17,4)` | LEADER14 | MOVE_TO\|0 (`$40`) | 0 |
| 1 /129 | GALAM_ARCHER56 `(14,5)` | SENTRY8 | FOLLOW_ENEMY\|0 (`$80`) | 15 |
| 2 /130 | GALAM_MAGE67 `(15,7)` | SENTRY8 | FOLLOW_ENEMY\|0 | 15 |
| 3 /131 | GALAM_SOLDIER2 `(11,4)` | ATTACKER1/6 | NONE | 15 |
| 4 /132 | GALAM_ARCHER56 `(10,7)` | SENTRY8 | NONE | 15 |
| 5 /133 | GALAM_SOLDIER2 `(9,9)` | ATTACKER1/6 | NONE | 15 |
| 6 /134 | GALAM_SOLDIER2 `(19,2)` | ATTACKER3/9 | FOLLOW_ENEMY\|0 | 0 |
| 7 /135 | GALAM_MAGE67 `(17,1)` | ATTACKER3/9 | FOLLOW_ENEMY\|0 | 0 |

Point0 is`(17,4)`. `GetMoveOrderPosition` tests bit6 and uses the low nibble
to index points; otherwise it reads the specified combatant's live X/Y.
128 targets point0, while129/130/134/135 bind enemy128, not ally0, point0 or
an immutable knight start. `GetMoveOrders` returns separate bytes and
`Set/GetTriggerRegions` pack/unpack region nibbles. Secondary15 adds no polygon.

Fresh initialization scans32 enemy slots from128. These eight mode0 records
initialize directly; unused slots receive max/current HP0 and X=-1. The occupied
spawn-position check is not a mode0 entry gate. `InitializeEnemyStats` calls
upgrade selection, copies the selected56-byte definition, restores HP/MP,
coordinates, movement/commandset, type/regions/orders, processes the placement
item, reconstructs the activation word, adjusts base ATT and refreshes stats.
NOTHING makes `InitializeEnemyItems` return without AddItem; it does not erase
the definition's equipment or supply a Herb/AI-use/drop addition.

| Definition | Level; HP/MP; base ATT/DEF/AGI/MOV | Movement / equipped item / resistance / AI declaration |
| --- | --- | --- |
| GALAM_KNIGHT3 | 3;16/0;9/11/12/7 | CENTAUR; SHORT_SPEAR72; resistance0; PRIORITYMOD_2\|BIT5 |
| GALAM_ARCHER56 | 3;15/0;7/10/10/5 | ARCHER; WOODEN_ARROW45; resistance0; PRIORITYMOD_2\|BIT4 |
| GALAM_MAGE67 | 3;14/5;8/9/11/5 | MAGE; SHORT_ROD86; FIRE_MINOR; PRIORITYMOD_2\|BIT4 |
| GALAM_SOLDIER2 | 2;11/0;8/10/8/6 | REGULAR; SHORT_AXE33; resistance0; PRIORITYMOD_2\|BIT5 |

Each has three other empty item slots, status NONE, REGULAR spell power and
CRITICAL150_1IN32/DOUBLE_1IN32/COUNTER_1IN32 prowess. No selected AGI has the
second-turn bit. Only mages know BLAZE at level1, followed by three NOTHING
slots; the other types have four NOTHING spells. The initializer keeps only
the AI definition word's high nibble and installs mode/filler0: initial word
is`$2000`; BIT4/BIT5 do not survive. This word is not the dedicated commandset.

Equipment refresh starts from base stats and applies each equipped item's
effects. Short Spear has ATT+6/range1–2, Wooden Arrow ATT+7/range2–2, Short Rod
ATT+5/range1–1 and Short Axe ATT+5/range1–1. All four declare useSpell NOTHING
and WEAPON; physical reach is separate from a spell's range. The actual
difficulty branch applies floor(baseATT×5/4) once on every non-SUPER difficulty and
twice on SUPER, before equipment. Thus difficulty0 final ATT is17/15/15/15
for knight/archer/mage/soldier, not9/7/8/8. This is static derivation, not
damage or a naturally selected difficulty.

The mage's BLAZE1 definition is attack/silence-affected, MP2, range1–2, radius0,
power6. `GetNextUsableAttackSpell` finds the actual slot/type and
`GetHighestUsableSpellLevel` checks current MP; fresh MP5 can admit level1.
`aiCommand_Attack` can select spell, physical attack or item through its actual
category/target consumers. BLAZE availability does not prove casting first,
range access, damage6 or a completed spell action. Silence, targets, movement
and later resource changes still matter. No healing/support spell is declared.
For these selected weapons, `IsItemUsableInBattle`'s erroneous comparison
against255 must not be treated as a reliable no-use filter: NOTHING63 falls
back to the first HEAL definition in `GetSpellDefinitionAddress`. The attack-
item getter rejects that non-attack type; healing-item selection skips these
non-Rain weapons without AI bit13. No enabled healing item follows merely
from equipment. This is selected action admission, not a full spell/item audit.

`IsBattleUpgradable` scans the actual length-prefixed random list, which omits5.
`UpgradeRandomBattleEnemies` restores the declared type and returns before
completion/level/category/RNG selection. F505 cannot upgrade these enemies or
consume upgrade RNG. Battle03's airborne exclusion defect remains at its owner;
it is not entered here.

## Region0, standby and the first conditional dispatch

**Confirmed:** region0 is type4, second byte0, ordered vertices`(12,0)`,
`(12,13)`, `(23,13)`, `(23,0)`, trailing bytes0/0. The source tests triangles
1/2/4 and3/2/4 using its integer projection/CCR rules. Test subjects are all30
allies with positive HP and nonnegative X/Y, not the enemy that invokes the
test, field actors or template identities. All fresh possible placed allies
have X2–4/Y4–5, left of the X12 boundary: the first triangle's vertical edge
separates them from vertex4, and the second triangle excludes them as well.
None can trigger0 from these fresh placements. Enemy131 atX11 or enemies
inside the rectangle are not the tested party. No natural saved-state position
or arbitrary-polygon equivalence is inferred from this rectangle.

`ActivateEnemies` visits placed living enemies, then
`TriggerRegionsAndActivateEnemies` tests all declared regions subject to
`NEWLY_TRIGGERED_BATTLE_REGIONS`, and sets persistent F90 on a hit. That word
is a tested cache, not the persistent flags: fresh90–105 clearing and
`ClearAiMemory` do not explicitly clear it; non-swarm `StartAiControl` clears
it later. An inherited cache can suppress a test, but fresh F90 remains clear
here even with a real test. Assigned primary0 success sets active bit0; primary
wins over secondary, whose success would set bits0/1. All selected secondary
regions are15. A later hit retains F90 until a clearing consumer; suspended
entry must not be assigned fresh flags/coordinates.

No Battle05 region-program row exists (the sparse table selects32/40), and
there is no neutral row. All mode0 enemies are already initialized; the spawn
collector supplies no hidden/respawn candidate or spawn animation here. No
extra actor or dialogue is implied by region activation.

At ordinary enemy AI entry, five15/15 enemies129–133 bypass standby regardless
of active bit0. The three0/15 enemies128/134/135 enter standby while0 is clear.
Their actual special-order/non15 combination makes
`ValidateAiStandbyEligibility` return nonzero; the caller treats nonzero as
stop. Along with its separate early random-stop branches, this yields an empty
move string and STAY for those unactivated entries, not local wandering or
an attack. Comments calling the nonzero result “allow movement” do not reverse
the caller. A later activated entry uses its ordinary commandset.

| Set / selected users | Commands attempted in order | Preparatory mode |
| --- | --- | --- |
| HEALER1/0, ally templates only | HEAL1, SUPPORT, ATTACK1, MOVE1, STAY | Regular; not installed by placement |
| SENTRY8,129/130/132 | ATTACK1, HEAL1, SUPPORT, STAY | Regular |
| ATTACKER1/6,131/133 | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY | Regular |
| ATTACKER3/9,134/135 after activation | MOVE_ORDER3, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER4, MOVE2, STAY | Block non-movable |
| LEADER14,128 after activation; shares set13 pointer | MOVE_ORDER5, ATTACK2, HEAL3, SUPPORT, MOVE_ORDER2, MOVE3, STAY | Block and carve |

`StartAiControl` promotes a dead followed target to the secondary order before
command selection; here that makes both orders NONE. SENTRY has no MOVE_ORDER
or MOVE command: its stored FOLLOW_ENEMY0 does not itself move129/130 after
the knight. ATTACK1 can still construct an attack position/path; SENTRY is
not a universal no-movement predicate. MOVE_ORDER3/4/5/2 dispatch follow/point
and obstruction modes independently. Move-order execution checks MOV/order/
target HP, can try an attack, and can build movement followed by STAY. AI
allies select set6, not the templates' HEALER1. Every later tactic is outside
this stopping boundary.

Rounds reuse the [complete common rule](./battle02-initialization-and-first-control.md#first-round-and-conditional-control-endpoint):
64 sentinel slots, living placed candidates from30 allies/first30 enemies,
current numerical AGI and main-RNG draws, optional high-bit second turn,
62 signed-byte stable sort passes and cursor0. All eight selected enemies
can enter independently of activation. No upgrade draws precede the round;
no seed reset or fixed first actor follows. Even range0 draws advance the
main seed; standby uses the separate thinking RNG.

A sentinel requests round preparation. A reached actor enters
`ExecuteIndividualTurn`: HP0 skips; MUDDLE, AI-controlled bit2, opponent-control
and ally auto-battle choose AI preparation. After cursor/view/status windows
and movement-grid work, SLEEP/STUN choose NoAction; otherwise the control
predicates choose actual AI execution or player input. Stop at that dispatch,
not an executed action or guaranteed first player-ready state. Suspended entry
can consume its saved queue instead.

## Current conversion and production consumers

**Confirmed by accepted production code inspection, separate from original
evidence:** Battle05 is not admitted, converted or supported by the existing
private encounter path. The [trust](../../remake/docs/content/profiles-and-trust.md#selected-third-encounter)
and [Application](../../remake/docs/application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation)
owners describe selected2/3 consumption. No preparation, lowering or runtime
comparison was performed; PR692 supplies no accepted dependency here.

| Actual consumer | Reusable mechanism and explicit Battle05 boundary |
| --- | --- |
| [prepare_encounter / compile_routes](../../src/sf2tool/remake_exploration_battles.py) | Producer/CLI admit2/3 only, with exact selected headers and source/ROM checks. Generic route compilation binds hooks/load and400/450/500 flags for a supplied selection; it does not export5 or its castle content. |
| [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) | Reads complete placements, regions/points, metadata and pinned terrain/ranges. `Pin` admits1/2/3, rejects5; no5 pin/export exists. |
| [PrivateBattleScenarioReader.Assemble](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs) / [PrivateBattleActionBindings](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Region-program admission independently allows1/2/3. Commandset switch allows0/6/7/9/14, rejecting SENTRY8. Mover lacks ARCHER/MAGE; enemy AI expression rejects BIT4; physical binding rejects non1/1 weapons (Short Spear1/2 and Arrow2/2). The controlled class switch lacks class3/MAGE and Slade's THIF10; six templates do not require either character. Assembled action definitions bind HEAL/healing items, not BLAZE. A transport shape or stored spell does not establish executable spell support. |
| [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) | Route/before/load/start/intro continuations, live membership rebuild and carried resources/growth/gold/both seeds are reusable. Missing ally definitions and suspended F88 entry reject; original saved queue/cursor restoration is absent. No admitted5 content reaches this path. |
| [BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) / [BattleTurnFlow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) | Controlled already-refreshed allies/roster-only/difficulty0, living/immortal healing and formation placement exist. Excess active count rejects instead of the original excess-party tail; enemy item admission allows0/127 only, rejecting all four equipped weapons. General equipment/CURSE refresh, other difficulty/control policies and saved-state restoration remain Unsupported. |
| [BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs) / [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs) | Living placed ally tests, primary/secondary activation, RegionProgram.None, spawn0 and randomized rounds are reusable. Cross-product geometry and a fresh per-round tested word do not prove equality with the original projection/cache lifetime for arbitrary state. No Battle05 equivalence observation is claimed. |
| [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs) / [SourceMoveOrderAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) | Standby,15/15, points/live follow and dead-target promotion exist. Activated commandsets admit6/7/9/14, not8; loadout checks allow only0/127 items and no learned spells, rejecting these weapons and BLAZE. Existing adjacent physical searches do not supply general ranged/mage behavior. |
| [BattleAdvancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs) / [BattleControlRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) | Queue-driven automatic progression and reached-player admission exist with explicit control/status rejection. A visible player stop may follow automatic work; no fixed hero or reference recording defines legality. This consumer boundary is not original evidence. |

These are current admission/support limits, not a newly observed Battle05 run
or authorization to broaden allowlists or repair the engine. No new in-scope
consumer defect requiring adjacent implementation was identified. The
[four frontiers](./story-content-coverage.md#four-different-frontiers) stay distinct.

## Unknowns and verification boundary

**Unknown:** naturally reached405/455/505/88 and egress/following state, actual
party/HP/status/level/difficulty/RNG, first actor, continuous travel, every
AI/action and full tactical legality, presentation/hardware timing and save
durability. No fixed six-member party, Slade participation, first cast or
winning strategy is established. The #1047 twelve **completed unresolved
comparisons** remain at the [verification owner](../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03);
they are not interrupted runs or an automatic repair/acquisition queue.
Accepted Battle01 natural admission, victory, stable return and later Down
observations are not downgraded. The earlier mechanical PASS that missed the
intermediate-status/equipment-CURSE distinction remains a semantic miss in the
prior handoff; the accepted correction is retained above.

Reproduction is direct reading of the complete named source/consumer bodies.
Direct checks cover links/anchors, UTF-8/LF, explicit three-path base diff,
ownership/private boundary, unchanged fixture counters and four frontiers.
Retained map-init/map-script outputs total18,285,979 bytes and remain unchanged,
as does `local/issue1005/pair.txt`. Small fresh receipts belong under ignored
`local/issue1220/`; reuse the simple document recipe with explicit `origin/main`
diff, including committed head. A mechanical PASS is not independent semantic
acceptance. On a clean committed head load current ignored configuration in
the same process, then record the plan and actual exact-head scoped CI:

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify plan --base origin/main --head HEAD
```

No normal/full, SDK/Godot/emulator/H3/H4, screenshots, comparison, lowering,
extraction or acquisition is selected. Stop with a clean pushed frozen Draft
PR, Review / Verify handoff and stopped processes; main-gate retains independent
acceptance and merge authority.
