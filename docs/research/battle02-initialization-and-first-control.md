# Battle02 initialization, activation and first control

Evidence date: 2026-10-22. **Confirmed:** the pinned source connects Battle02's
complete selected formation to state-dependent fresh initialization, entity
loading, round preparation and individual-turn control routing. **Inferred:**
this is the intended early To Yeel encounter after the conditional
[Battle01 return/Jaha route](./battle01-to-battle02-content-route.md).
**Unknown:** natural arrival state, actual first actor, presentation/timing,
complete tactics and save durability. Static dispatch is not observed play.

## Scope and provenance

This is the Standard-rigor agreement for
[Issue1214](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1214),
within [Epic1053's coverage agreement](./story-content-coverage.md#scope-and-reading-rules).
The outcome lets content/engine maintainers and independent reviewers identify
which original initialization rules a conversion must preserve when valid party
state changes. It joins all selected declarations to their actual consumers;
it stops at the first individual-turn dispatch and its conditional player/AI
entry. It reuses the [admission](./battle01-to-battle02-content-route.md#battle-02-data-and-admission-semantics)
and [outcome/return](./battle02-to-battle03-content-route.md) owners without
reopening their castle, Jaha, Yeel or onward-route investigations.

All original paths below are relative to `disasm/` in
[SF2DISASM at c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The selected checkout was clean at that pin. The
[USA baseline](../../README.md#pinned-original-baseline) and existing fixtures
retain ROM identity; this slice makes no fresh ROM-equivalence or runtime claim.
Accepted repository base is `1c1876fb20fa02e2305613cfe47be1c739f31e4f`.

The following source-to-claim map is the semantic reproduction route. Read the
complete named functions, including their return tails, rather than treating
comments, symbols or a header count as behavior. Jump-interface `j_` entries
resolve to these implementations; the maintained source-form spriteset include
owns this selection, not the unused binary aggregate.

| Claim boundary | Pinned source and actual consumer |
| --- | --- |
| Selected data | `data/battles/spritesets/entries.asm` slot2 → `spriteset02.asm:BattleSpriteset02`, `0x1B3376..0x1B33F4`; `sf2macros.asm:allyCombatant`, `enemyCombatant`, `combatantAiAndItem`, `combatantBehavior`; `sf2enums.asm` constants. `code/gameflow/battle/getbattlespritesetsubsection.asm:GetBattleSpritesetSubsection` selects the current slot and advances by the declared subsection sizes. |
| Admission and metadata | `code/common/maps/getbattle.asm:CheckBattle`, `code/gameflow/mainloop.asm:MainLoop`; `data/battles/global/battlemapcoords.asm` row2, `terrainentries.asm` slot2; `battleloop/loadbattleterraindata.asm:LoadBattleTerrainData`. Global background/leader/EXP consumers are named below. |
| Entry and party | `code/gameflow/battle/battleloop_1.asm:BattleLoop` at `0x23A84`; `battleloop/heallivingandimmortalallies.asm`; `battleloop/initializecombatants.asm:InitializeAllAlliesBattlePositions` at `0x1B1272`; `code/common/stats/battleparty.asm:UpdateForce`, `JoinForce`, `LeaveBattleParty`. |
| Refreshed enemies | The complete enemy initialization/item/difficulty functions in `battleloop/initializecombatants.asm`; `battleloop/upgradeenemies.asm`; `data/stats/enemies/enemydefs.asm` records0/1; `code/common/stats/updatecombatantstats.asm:UpdateCombatantStats` and `InitializeCurrentStats`. |
| Entities and round | `battlefunctions/loadBattle.asm:LoadBattle` at `0x25610`; `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities`; `battleloop/clearaimemory.asm`, `activateenemies.asm`, `triggerregions.asm:TriggerRegionsAndActivateEnemies`, `populateenemyspawns.asm`; `battleloop/turnorderfunctions.asm` at `0x25544`. |
| First control and order meaning | `battlefunctions/executeindividualturn.asm:ExecuteIndividualTurn` at `0x23EB0`; `ai/startaicontrol.asm:StartAiControl`, `ai/executeaicommand.asm:ExecuteAiCommand`, `ai/command/moveorder.asm:aiCommand_MoveOrder`; `battlefield/getmoveorderposition.asm:GetMoveOrderPosition`; `code/common/stats/combatantstats_1.asm:GetMoveOrders`, `GetTriggerRegions`; `data/battles/global/aicommandsets.asm`. |

Unqualified battle code paths in that table resolve under
`code/gameflow/battle/`. The [spriteset](./battle-spriteset-data.md),
[global](./battle-global-data.md), [routing/terrain](./battle-routing-data.md),
[lifecycle](./battle-loop.md) and [AI](./battle-ai.md) owners retain their wider
inventories and bounded observations. Their existing H2 fixtures
`battle-spriteset-data-static-v1.json`, `battle-global-data-static-v1.json`,
`battle-routing-data-static-v1.json`, `battle-loop-static-v1.json`,
`battle-control-static-v1.json` and `battle-ai-static-v1.json` live under
`tests/fixtures/h2/`. These prove their declared shapes/guards, not a new
executable Battle02 composition. In particular, spriteset `rowContentTracked:
false` is not a per-row behavior assertion. No new parser, fixture, schema,
comparison platform, lowering, extraction or acquisition is part of this scope.

## Selection, metadata and empty hooks

**Confirmed:** source enum `BATTLE_TO_YEEL=2` is a table index, not chronology.
Row2 has Map66, area origin `(16,19)`, size `22×18` and wildcard triggers
`255/255`. `CheckBattle` scans table order, tests F402, then those trigger bytes,
and writes the area fields as outputs. The origin/size are not admission bounds.
If F502 is already set, it clears F402 but still returns2 on that call;
`MainLoop` passes the selection to `BattleLoop` before ordinary exploration.
Neither Map66's empty ordinary entity list nor its RTS init constructs this
battle roster. The entrance owner supplies the conditional unlock relationship;
this document does not prove a naturally walked continuation to it.

Slot2 selects the distinct `BattleTerrain02` compressed payload.
`LoadBattleTerrainData` uses `CURRENT_BATTLE*4` and Stack-decodes into the fixed
48×48 battle terrain array. The 22×18 operating area and 48×48 storage are
different dimensions. The global custom-background row is `OVERWORLD`:
`battlescenes/getbattlescenebackground.asm:GetBattlesceneBackground` interprets
that sentinel by reading the combatant's terrain and `table_TerrainBackgrounds`;
it does not force one uniform image. `enemyleaderpresence.asm` row2 is zero,
so `cutscenes/battleendcutscenesend.asm:loc_47C48` skips the leader-death sweep.
Enemy0's commandset name LEADER does not override that separate battle rule.
`global/halvedexpearnedbattles.asm` selects only Battle1; the actual scan in
`battleactions/giveexpandgold.asm` therefore supplies no Battle02 half-EXP rule.
These metadata consumers are not actions executed by this slice.

Before/start/after row2 all target `ms_Empty`; before/start slots are
`0x47A8C`/`0x47B30`. `code/common/scripting/map/ms_empty.asm` contains the
terminator, consumed by `ExecuteMapScript` in `mapscriptengine_2.asm` through
`loc_47234`: it can wait for view scrolling and clears scrolling speed. There
are no story commands in that target, but its wrapper is not empty:

- `cutscenes/beforebattlecutscenesstart.asm` tests F452 and skips when set;
  it does not set the flag. Its end file restores saved `d1` and returns.
- `cutscenes/battlestartcutscenesstart.asm` tests F452, sets it when clear,
  then dispatches the empty target; its end file restores `d1` and returns.
- `cutscenes/regionactivatedcutscenes.asm` scans the sparse built
  `data/battles/cutscenes/regionactivatedcutscenes.asm` table, which has only
  Battle32/40 rows and a terminator. Battle02 has no matching region program.
  This is an absent route, distinct from a selected empty script.

The after wrapper, join tail and returns remain with the
[outcome owner](./battle02-to-battle03-content-route.md). An empty after target
does not erase shared outcome/join work or imply automatic Battle03 entry.

## Complete declaration and order operands

**Confirmed:** header4/6/0/1 means four ally placement templates, six enemy
records, zero AI-region polygons and one AI point. Each combatant record is
12 bytes; points are coordinate pairs. All four ally rows declare HEALER1,
item NOTHING, primary/secondary NONE, region operands0/0, filler0 and STARTING.
Their ordered positions are `(13,12)`, `(12,12)`, `(13,13)`, `(12,13)`.
Their numeric identity fields0–3 do not select the original live allies:
`InitializeAllAlliesBattlePositions` reads actual member IDs from RAM and only
the X/Y fields from these records. It also does not copy the ally rows' AI,
item, orders, region or spawn fields into the allies. HEALER1 in a placement
does not turn a living force member into a healer or overwrite its inventory.

All six enemy rows have item NOTHING, secondary order NONE, primary/secondary
region operands15/15, filler0 and STARTING. The complete differences needed to
explain selection and command routing are:

| Enemy slot / combatant | Source type and start | Commandset | Primary order and actual referent |
| --- | --- | --- | --- |
| 0 / 128 | HUGE_RAT `(3,5)` | LEADER14 | MOVE_TO0 → the one AI point `(3,5)` |
| 1 / 129 | OOZE `(5,6)` | ATTACKER3/9 | FOLLOW_ENEMY0 → combatant128's current position |
| 2 / 130 | OOZE `(4,10)` | ATTACKER1/6 | NONE |
| 3 / 131 | HUGE_RAT `(5,14)` | ATTACKER1/6 | NONE |
| 4 / 132 | OOZE `(7,16)` | ATTACKER3/9 | FOLLOW_ENEMY3 → combatant131's current position |
| 5 / 133 | OOZE `(13,6)` | ATTACKER1/6 | NONE |

`combatantBehavior` encodes orders and region operands separately. NONE is
`0xFF`, MOVE_TO has bit6 (`0x40`), FOLLOW_ENEMY has bit7 (`0x80`), and region15
is the no-region sentinel, not polygon15 or a radius. `GetMoveOrders` extracts
the two stored order bytes. `GetMoveOrderPosition` resolves bit6 via the low
nibble into the AI-point subsection; otherwise it reads the target combatant's
current X/Y. The followers therefore track live rats, not immutable start
coordinates, an ally with index0/3, or a second AI point. There is no polygon
to infer from the point or these formation coordinates.

`StartAiControl` reads the dedicated commandset byte independently of the
activation word. Complete selected command sequences from
`global/aicommandsets.asm`, attempted in order until a command returns success,
are:

| Set | Ordered commands | Preparatory pathfinding mode |
| --- | --- | --- |
| HEALER1/0, declared only in ally templates | HEAL1, SUPPORT, ATTACK1, MOVE1, STAY | Regular; not installed by ally placement |
| ATTACKER1/6 | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY | Regular |
| ATTACKER3/9 | MOVE_ORDER3, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER4, MOVE2, STAY | Block non-movable |
| LEADER14, sharing set13's pointer | MOVE_ORDER5, ATTACK2, HEAL3, SUPPORT, MOVE_ORDER2, MOVE3, STAY | Block and carve |

`ExecuteAiCommand` maps MOVE_ORDER3 to follow mode/block-non-movable,
MOVE_ORDER4 to point mode/block-non-movable, MOVE_ORDER5 to point mode/block-and-carve,
and MOVE_ORDER2 to follow mode/block-and-carve. `aiCommand_MoveOrder` checks
MOV, order presence and a follow target's HP, tests attack opportunities before
constructing movement, and can return a movement string with action STAY.
An AI point equal to the rat's start is thus an order operand, not a guarantee
of permanent immobility. Before command dispatch, a dead primary follow target
causes promotion of the secondary order and clearing of that secondary; here
both become NONE. `StartAiControl` also applies/clears temporary terrain
restrictions. The [AI owner](./battle-ai.md#move-and-move-order-commands) retains
the deeper action/pathfinding predicates; this account selects the operands
and routing, not a complete six-enemy tactical simulation.

## Fresh and suspended entry lifetimes

**Confirmed:** `BattleLoop` first clears PLAYER_TYPE and sets F399 for either
entry. Its clear-F88 branch runs the following sequence:

1. Clear elapsed seconds; save incoming map/battle, install CURRENT_MAP and
   CURRENT_BATTLE, install base VInt functions, and call the before wrapper.
2. Restore the saved map/battle arguments and reinstall both current fields;
   clear region flags F90–F105 inclusively.
3. Call `HealLivingAndImmortalAllies`, then ally initialization (including
   `UpdateForce`), then all-enemy initialization, then `ClearAiMemory`.
4. Clear `d0` to choose combatant0 as the load focus; call `LoadBattle`, then
   the start wrapper, then round preparation.

F452 set skips both intro bodies but not healing, initialization, loading or
round preparation. F502 is a different completed/repeat flag; it is neither
the intro flag nor a suspended-state flag. `list_RandomBattles` in
`global/randombattles.asm` omits TO_YEEL2 (its first entry is TO_HAWEL_HOUSE3).
`UpgradeRandomBattleEnemies` therefore returns each declared enemy type even
on a selected completed repeat; F502 alone cannot cause Battle02 level upgrades.

Set-F88 entry instead restores SAVED_SECONDS_COUNTER, clears F88, clears AI
memory and VIEW_TARGET_ENTITY, and calls `LoadBattle` using incoming `d0` as
focus. It jumps directly to the individual-turn loop using the retained
CURRENT_BATTLE_TURN/queue. It does not install fresh map/battle arguments,
heal, reset placements/regions, rerun hooks or generate a new round first.
The caller must supply the saved current fields and queue. A retained sentinel
does enter round preparation; a retained actor goes directly to its dispatch.
The unreachable branch after that jump is not an alternate natural path.
This is source control flow, not proof of successful suspend/save restoration.

### Joined, active, dead and capacity

`HealLivingAndImmortalAllies` scans all30 ally slots, irrespective of joined
flags. It skips ordinary HP0 allies, but always handles Peter7/Lemon28. For a
handled ally it restores HP/MP to maxima, retains only STUN/POISON/CURSE
(`status & 7`) and refreshes derived stats. Thus ordinary death survives new
entry, and a living stunned member need not be able to act after placement.

`UpdateForce` scans IDs0–29 in ascending order. Only joined flag `0+id` enters
the force list; within it active flag `32+id` chooses battle party versus
reserve. It does not filter HP. `JoinForce` can set active while the rebuilt
active count is below FORCE_MAX_SIZE12; `JoinBattleParty` itself only sets the
flag. That general joining capacity is distinct from Battle02's four slots.

`InitializeAllAlliesBattlePositions` first sets every ally X to -1, then
rebuilds those lists. With a nonempty active party, it scans the first
`min(active count,4)` members. A living member gets the current template's X/Y
and advances the formation pointer; a dead member stays unplaced and does
not advance that pointer. **Both** consume a member-scan iteration: the
four-iteration counter decrements even on the dead branch. This is not
"scan until four living members have been placed." If active members remain
after those four iterations, `LeaveBattleParty` clears each remaining active
flag and sets X=-1; it does not clear joined membership. The RAM lists are
not rebuilt again inside that tail. The routine has no pre-loop zero-party
guard; arbitrary empty/corrupt entry state is not a supported natural premise.

Useful changed-state consequences follow directly from the branches:

| Supplied state | Source consequence |
| --- | --- |
| Joined but inactive Jaha, or any other member | In reserve; a template ID does not activate or recruit him. |
| Different joined/active IDs, at most four | Ascending member IDs consume the same templates; four named heroes are not required. |
| An ordinary dead member among the first four | Still active but unplaced; the next living member uses the unconsumed template. |
| Five active members, one of the first four dead | At most three live placements; fifth member still exits active party, rather than filling a fourth live slot. |
| Active Peter/Lemon at HP0 | Healing precedes the HP test, so the immortal rule can make that member placeable. This is not evidence either naturally joins this early encounter. |
| Intro already seen, completed repeat, or suspended entry | These are separate gates/lifetimes; only the fresh path applies this initialization and excess-party tail. |

## Enemy refresh, entity load and activation

**Confirmed:** `InitializeAllEnemiesBattlePositions` visits32 slots from128.
For Battle02, the first six have spawn mode0 and call `InitializeEnemyStats`;
the remaining slots receive max/current HP0 and X=-1. Battle32's enemy15/Jaro
exception does not apply. Fresh STARTING enemies are initialized directly;
the spawning reset helper's occupied-start carry check is not an admission
test for these six initial records.

`InitializeEnemyStats` selects the unchanged type, copies its complete enemy
definition, restores current HP/MP, assigns placement coordinates, combines
definition movement type with the row's commandset, stores enemy identity,
regions and both orders, then initializes the placement item. It preserves
only the definition activation word's high nibble when assembling the row's
spawn/filler word, adjusts base ATT for difficulty, and refreshes current
stats/status/equipment. Table declaration, copied stats and refreshed stats
are different stages.

Both selected enemy definitions have initial status0, no spells and four
NOTHING item slots; placement NOTHING adds no item. There is consequently no
equipped bonus to invent or apply twice. OOZE has max HP9, base ATT9/DEF6/AGI5/
MOV4 and REGULAR movement; HUGE_RAT has HP10, base ATT10/DEF8/AGI7/MOV6 and
STEALTH movement. Both have MP0 and base AI `PRIORITYMOD_2|BIT5`. The actual
initializer preserves `0x2000`, not BIT5, and merges zero spawn/filler into it.
No primary/secondary activation bit is required to be set at this stage.
`AdjustEnemyBaseAttForDifficulty` multiplies base ATT by5 and shifts right2
once on every difficulty; its adjacent equal branches do not provide the
suggested non-SUPER skip. SUPER applies the truncated operation a second time.
At difficulty0 this gives Ooze ATT11 and Rat ATT12. `UpdateCombatantStats`
rebuilds current values from base, then status and equipped effects; no selected
enemy effect changes those results. These facts are not source-derived ally
stats: allies retain their own state and equipment through the earlier refresh.

`ClearAiMemory` sets48 last-target bytes to -1 and48 memory bytes to0, not all
combatant data or RNG. `LoadBattle` fades/loads tilesets, rebuilds battle
entities, chooses its view focus, loads map/sprites, reinstalls VInts, loads
terrain/music and requests fade-in. `PositionBattleEntities` creates entities
from nonnegative combatant X/Y; enemy neutral-bit entries are skipped. It
uses live combatant mapsprites, not the ordinary Map66 entity list. The
completion-gated neutral table has no Battle02 entry. Loading reconstructs
entities from combatants; it does not rerun enemy-stat initialization.

Fresh round preparation calls `ActivateEnemies`, then the region-cutscene
dispatcher, spawn population/animation if any, then turn generation.
`ActivateEnemies` visits32 enemy slots and falls through to its update helper;
that tail is not an RTS to add by interpretation. The helper ignores negative-X
or HP0 entries before region processing. For this selected zero-region data,
`TriggerRegionsAndActivateEnemies` returns before testing or setting any
region/activation flags. The separate no-matching-region-program scan returns.
`PopulateTargetsListWithSpawningEnemies` inspects modes `0x0100`, `0x0200`,
`0x0300`; the fresh six mode0 entries and unused slots supply no candidate, so
there is no spawn animation at this first fresh boundary. A suspended queue
does not imply fresh combatants or this same initial state.

Crucially, StartAiControl tests both assigned regions against15 and bypasses
the inactive standby gate when both are NONE. All six enemies can therefore
reach ordinary AI routing despite zero region flags and zero low activation
bits. Declared, initialized, entity-loaded, region-activated, turn-eligible and
able-to-act are not interchangeable descriptions.

## First round and conditional control endpoint

**Confirmed:** `GenerateBattleTurnOrder` fills64 two-byte slots with -1, scans
all30 allies and the first30 enemy slots, and adds only nonnegative-X,
positive-HP candidates. This admission test does not check joined/active flags
again or require a primary-active AI bit. The six selected enemies are within
that enemy scan. It reads refreshed current AGI, masks the numerical low7 bits,
adds then subtracts main-RNG draws bounded by `AGI >> 3`, and adds `RNG(3)-1`.
An original AGI word at least128 adds a second score based on
`floor((AGI & 127)*5/6)` with its own plus/minus draws. The selected enemies
have no double-turn bit; changed ally state must not be frozen to a recording.

The routine performs62 bubble-sort passes using signed-byte score comparisons;
equal scores retain insertion order. It resets CURRENT_BATTLE_TURN to0.
`code/common/tech/randomnumbergenerator.asm:GenerateRandomNumber` updates
RANDOM_SEED even for range0; low-AGI candidates do not omit those first two
draws just because their returned values are zero.
The individual loop reads the combatant byte at that cursor; -1 restarts
round preparation, otherwise it calls `ExecuteIndividualTurn`. Actual party,
refreshed AGI and incoming RNG determine which actor reaches that call. There
is no fixed Bowie/Sarah/Jaha/enemy-first promise and no source seed reset here.
The suspended path may instead use an already generated queue/cursor.

`ExecuteIndividualTurn` masks the ID and clears the dead list, skips HP0,
then installs actor/target coordinates, cursor and view context. MUDDLE,
AI-controlled bit index2 (`0x0004`), an ordinary enemy without opponent-control,
or an ally with auto-battle selects `StartAiControl`; otherwise it enters
player preparation.
After common cursor/view waits, windows and movement-grid preparation, SLEEP
or STUN selects NoAction before the second AI/player action decision.
Otherwise the same control conditions select `ExecuteAiControl` or
`ProcessBattleEntityControlPlayerInput`. An AI-preparation call alone is not
proof an action was performed, and a dispatched ally can lose action to status.

This is the stopping boundary: the conditional first actor/control or no-action
route, not a completed turn, first guaranteed player input, victory script or
full battle. The existing [outcome/return account](./battle02-to-battle03-content-route.md)
owns defeat/escape/victory and the conditional onward Battle03 admission.

## Actual producer and production consumers

**Confirmed by code inspection, separate from original evidence:**
[`prepare_encounter`](../../src/sf2tool/remake_exploration_battles.py) already
admits2/3. It validates pinned inputs and selected source records, retains
all placement behaviors/AI points and selected metadata, and decodes terrain
into new ignored output. No new importer or regeneration is needed for this
account. `compile_routes` selects existing before/start targets and flags;
the selected production [trust](../../remake/docs/content/profiles-and-trust.md#selected-second-encounter)
and [Application](../../remake/docs/application/session-and-programs.md#continuous-second-encounter-boundary)
owners retain the admitted content and live-session boundary.

| Actual consumer | Expressed behavior and limit |
| --- | --- |
| [PrivateBattleScenarioReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs), including Assemble | Reads admitted definitions/start, resolves set0/6/7/9/14, NONE/MOVE_TO/FOLLOW_ENEMY, all four formation positions, AI points and source-combatant bindings. It explicitly admits region-program None for1/2/3 and STARTING spawn; original region0 is not invented. It matches controlled ally inputs to numeric template identities before constructing deployments and rejects unused input actors. For Battle02 that means IDs0–3, a narrower admission than the original template consumer's arbitrary joined/active IDs. The complete formation still reaches Domain rather than four permanent actor coordinates. |
| [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) | Selects the route, handles intro flags and before/load/start continuation, rebuilds joined/active lists, carries live ally resources/progress, gold and both seeds; only new members/enemies use defaults. It rejects absent member definitions and F88 suspended entry explicitly. It does not implement source saved queue/cursor restoration. |
| [BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) and [BattleTurnFlow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) | Require declared controlled provenance, already-refreshed allies, roster-only policy and difficulty0; preserve living/immortal healing, dead/unplaced state, packed supported loadouts and separate enemy commandset/activation. Living active members consume AllyFormation in order. Active count beyond formation capacity rejects `ally-formation` as Unsupported; it does not perform the original excess-party deactivation tail. General equipment/status refresh and other difficulty/control policies remain Unsupported. |
| [BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs), [BattleAdvancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs) | Zero-region early return, no region program, admitted spawn0, living/placed randomized rounds and signed sorting feed the actual queued actor. Advancer can process automatic work before player input; a retained seed or first actor is not an encounter rule. [BattleControlRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) checks reached player state, including missing activation words/status, rather than globally assuming every ally player-ready. |
| [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs) and [SourceMoveOrderAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) | Actual set6/9/14 support reads commandsets separately, bypasses standby for15/15, promotes dead follow targets, resolves live target/AI-point bindings and routes selected point/follow modes. Its supported unarmed/no-spell source actions fit these selected definitions; changed unsupported loadouts, status or missing bindings stop explicitly. The original HEAL/SUPPORT and other complete command families are not established by this bounded support. |

The source/production differences above are explicit admission/support limits,
not new implementation work or proof of arbitrary-party equivalence. No actual
consumer defect is repaired or silently converted into a new fixture here.
Production never supplies evidence about the original. The
[four frontiers](./story-content-coverage.md#four-different-frontiers) remain
unchanged: current remake support through selected Battle03 and accepted
original Battle01 observations are separate from this static account.

## Unknowns and verification boundary

**Unknown:** naturally supplied party/flags/status/RNG, exact first actor,
continuous travel from the accepted Battle01 terminal, original presentation,
all AI/action decisions, full tactical legality and save persistence. No
fixed four-character loadout or winning recording defines valid gameplay.
The original battle with excess active members and the production rejection
have different contracts; broader conversion would need separately declared
content/engine ownership. The existing #1047 twelve **completed unresolved
comparisons** remain recorded at the
[verification owner](../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03),
not interrupted runs or an automatic repair/acquisition queue. No earlier
Battle01 natural admission, victory, stable return or subsequent Down evidence
is downgraded.

Reproduction is bounded read-only inspection of the full selected source and
actual consumers above. Direct checks cover relative links/anchors, UTF-8/LF,
exact three-path ownership/diff, unchanged fixture counters/frontiers and
private boundary. The two retained map static outputs remain unchanged; new
small check receipts belong under ignored `local/issue1214/`. Mechanical PASS
does not establish semantic correctness; the handoff includes a claim-to-source/
consumer review and preserved inspection/check failures for independent review.

On a clean committed head, load the existing ignored private configuration in
the same process and run:

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify plan --base origin/main --head HEAD
```

Record that plan and actual exact-head scoped CI on the Issue/PR. This
documentation-only slice does not run normal/full, SDK/Godot/emulator/H3/H4,
screenshots, old comparisons, lowering, extraction or acquisition. Stop with a
clean pushed frozen Draft PR, Review / Verify handoff and stopped processes;
main-gate retains independent acceptance and integration authority.
