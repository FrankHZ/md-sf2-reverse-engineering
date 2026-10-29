# Battle03 initialization, region activation and first control

Evidence date: 2026-10-22. **Confirmed:** the selected source connects Battle03's
12/6/1/0 declaration to fresh initialization, a conditional Yeel egress assignment,
two different activation routes and individual-turn control dispatch. Completed
repeats can change enemy definitions before loading. **Inferred:** this is the
early encounter on the way to Hawel's house. **Unknown:** naturally supplied
party/flags/level/RNG, actual first actor, complete tactics, presentation/timing
and save durability. Static composition is not a played continuation.

## Scope and provenance

This is the Standard-rigor agreement for
[Issue1216](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1216),
under [Epic1053's finite coverage agreement](../../story-content-coverage.md#scope-and-reading-rules).
The outcome supports early-encounter content maintenance and independent review:
identify Battle03's actual differences and the conversion limits when valid
party/state changes. The stopping point is the first individual-turn dispatch
and conditional player/AI/no-action entry. The
[Battle02 account](./battle02-initialization-and-first-control.md) owns the shared
lifecycle detail; [Battle02 return/Yeel/admission](./battle02-to-battle03-content-route.md)
and [Battle03 outcomes/Hawel/Battle04](./battle03-to-battle04-content-route.md)
keep their separate conditional connections. No new fixture, parser, schema,
platform, implementation, extraction or acquisition belongs to this scope.

All original paths below are relative to `disasm/` at
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The selected checkout was clean at that pin. Accepted repository base is
`a17ce818aed8f674b41870f7e6060f2ed8ee89ad`. The
[USA baseline](../../../../README.md#pinned-original-baseline) and existing fixtures
retain ROM identity; no fresh ROM-equivalence or runtime claim is made here.

Read the complete named functions and selected records, including return tails.
The following claim-to-source map is the bounded reproduction route; `j_` jump
interfaces resolve to these implementations. Battle paths without a prefix
below resolve under `code/gameflow/battle/`.

| Claim | Pinned declaration and actual consumer |
| --- | --- |
| Selected formation | `data/battles/spritesets/entries.asm` slot3 → `spritesets/spriteset03.asm:BattleSpriteset03`, `0x1B33F4..0x1B34DC`; `sf2macros.asm:allyCombatant`, `enemyCombatant`, `combatantAiAndItem`, `combatantBehavior`; `sf2enums.asm`; `getbattlespritesetsubsection.asm:GetBattleSpritesetSubsection`. |
| Admission/metadata/hooks | `code/common/maps/getbattle.asm:CheckBattle`, `code/gameflow/mainloop.asm:MainLoop`; `data/battles/global/battlemapcoords.asm` row3, `terrainentries.asm` slot3, global background/leader/EXP tables; `battleloop/loadbattleterraindata.asm`, `battlescenes/getbattlescenebackground.asm`; before/start wrapper start/end files under `cutscenes/`, selected tables under `data/battles/cutscenes/`; `data/battles/entries/battle03/cs_beforebattle.asm:bbcs_03/csub_49816`. |
| Fresh/suspended party and enemies | `battleloop_1.asm:BattleLoop` at `0x23A84`; `battleloop/heallivingandimmortalallies.asm`; complete `battleloop/initializecombatants.asm` at `0x1B1272..0x1B15F8`; `code/common/stats/battleparty.asm`; `code/common/stats/updatecombatantstats.asm:UpdateCombatantStats/InitializeCurrentStats`. |
| Repeat definitions and item meaning | `battleloop/upgradeenemies.asm:UpgradeRandomBattleEnemies` and its helpers, `0x1B1780..0x1B1A28`; `data/battles/global/randombattles.asm`; `data/stats/enemies/enemydefs.asm` records0/1/40; `code/common/stats/itemstats.asm:AddItem`; `ai/getnextuseableaiaction.asm:GetNextUsableAttackItem/GetNextUsableHealingItem`. |
| Loaded entities, activation and spawning | `battlefunctions/loadBattle.asm:LoadBattle` at `0x25610`; `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities`; `battleloop/clearaimemory.asm`, `activateenemies.asm`, complete `triggerregions.asm` at `0x1AC9FC..0x1ACF30`, `populateenemyspawns.asm`; both `cutscenes/regionactivatedcutscenes.asm` and its built data table. |
| Round and control endpoint | `battleloop/turnorderfunctions.asm` at `0x25544`; `code/common/tech/randomnumbergenerator.asm:GenerateRandomNumber`; `battlefunctions/executeindividualturn.asm:ExecuteIndividualTurn`; `ai/startaicontrol.asm:StartAiControl`; both `ai/determineaistandbymovement_*.asm`; `ai/executeaicommand.asm`; `data/battles/global/aicommandsets.asm`; `code/common/stats/combatantstats_1.asm:GetMoveOrders/GetTriggerRegions`, `combatantstats_2.asm:SetTriggerRegions`. |

The [spriteset](../../battle/battle-spriteset-data.md), [global](../../battle/battle-global-data.md),
[terrain/routing](../../battle/battle-routing-data.md), [loop](../../battle/battle-loop.md),
[AI](../../battle/battle-ai.md) and [cutscene](../../battle/battle-cutscenes.md) owners retain shared
inventories and observations. Existing `tests/fixtures/h2/` contracts
`battle-spriteset-data-static-v1.json`, `battle-global-data-static-v1.json`,
`battle-routing-data-static-v1.json`, `battle-loop-static-v1.json`,
`battle-control-static-v1.json`, `battle-ai-static-v1.json` and
`battle-cutscenes-static-v1.json` do not become a new executable Battle03 proof.
In particular `rowContentTracked:false` and `routeTargetsParsed:false` retain
their structural limits; selected row/consumer relations here are direct review.

## Selection and entry gates

**Confirmed:** `BATTLE_TO_HAWEL_HOUSE=3` selects row3, Map66, area origin `(0,0)`,
size `22×30`, triggers255/255. `CheckBattle` tests map, unlock F403 and the trigger
bytes in table order. Area dimensions are outputs, not proximity predicates.
Unlocked row2 can win first. F403/F503 both set clears F403 but still returns3
on that call. The predecessor owns Map66's first/repeat request and Yeel's
nonmandatory guidance; neither a battle number nor this row proves chronology.

Slot3 selects `BattleTerrain03`; slot4 aliases it. `LoadBattleTerrainData`
indexes four-byte pointers by CURRENT_BATTLE and Stack-decodes the fixed48×48
terrain array. Shared terrain does not merge the two encounters or replace the
22×30 operating area. Global row3 background is OVERWORLD: the actual background
getter resolves terrain for the selected combatant, rather than one fixed image.
Leader-presence row3 is0, consumed by `cutscenes/battleendcutscenesend.asm:loc_47C48`
to skip the leader-death sweep. The EXP table scan in
`battleactions/giveexpandgold.asm` matches only Battle1, so supplies no Battle03
half-EXP rule. These metadata relations are not new action/outcome observations.

F453 is the intro gate, F503 the completed/repeat gate, and F88 the suspended
entry gate. The shared fresh sequence remains
[Battle02's lifecycle](./battle02-initialization-and-first-control.md#fresh-and-suspended-entry-lifetimes):
clear PLAYER_TYPE/set399; install current map/battle for before; restore the
supplied arguments; clear90–105; heal, initialize allies then enemies, clear AI
memory; load with focus0; start hook; round preparation. For Battle03 specifically:

- Before slot3 at `0x47A8E` selects `bbcs_03` at `0x4980E`. On F453 clear,
  `ExecuteBeforeBattleCutscene` runs it without setting453. Its subroutine
  `csub_49816` writes `EGRESS_MAP=MAP_YEEL`; there is no dialogue, join or unlock
  write. This assignment does not prove a Yeel visit or an actual Egress journey.
- Start slot3 at `0x47B32` selects `ms_Empty`. Its wrapper sets453 before
  dispatch on the clear branch. The shared terminator consumer can wait for
  scrolling and clears scrolling speed; empty content is still a script boundary.
  F453 set skips both hook bodies while fresh healing/initialization still runs.
- F88 set restores saved elapsed seconds, clears88/AI memory/view target,
  reloads with incoming focus and resumes the retained queue/cursor. It bypasses
  fresh map/battle installation, both hooks, region reset, healing, placements,
  upgrades and new-round generation until a retained sentinel requests a round.

Thus not every Battle03 call writes Yeel. Suspended state requires already
supplied current fields and queue; successful save restoration remains Unknown.
The empty after target and its shared join/return work stay with the
[outcome owner](./battle03-to-battle04-content-route.md).

## All formation operands and live membership

**Confirmed:** the header is12 ally templates,6 enemy records,1 region,0 AI
points. Combatant records and region records are12 bytes; header counts drive
the subsection getter's pointer arithmetic. The ally numeric identities0–11
are template declarations. All declare HEALER1, NOTHING, NONE/NONE orders,
15/15 regions, filler0 and STARTING. Their complete ordered positions are:

| Template | Position | Template | Position | Template | Position |
| --- | --- | --- | --- | --- | --- |
| 0 | `(17,20)` | 4 | `(16,22)` | 8 | `(15,21)` |
| 1 | `(18,20)` | 5 | `(16,23)` | 9 | `(15,22)` |
| 2 | `(17,21)` | 6 | `(16,21)` | 10 | `(15,23)` |
| 3 | `(18,21)` | 7 | `(17,19)` | 11 | `(16,24)` |

`UpdateForce` rebuilds ascending actual IDs0–29 from joined flags0+ID and active
flags32+ID, without an HP filter. `InitializeAllAlliesBattlePositions` first
clears every ally X, then reads actual IDs from that RAM list and only template
X/Y. It copies no template identity, item, AI, order or region into those allies.
Twelve templates do not recruit twelve members, include reserve members, install
HEALER1 or require early Bowie/Sarah/Chester/Jaha as the only legal roster.

The same [member-scan/cursor distinction](./battle02-initialization-and-first-control.md#joined-active-dead-and-capacity)
now has a12-iteration limit. A dead ordinary active member consumes a scan
iteration but not a placement; the next living member uses the unconsumed
template. After twelve iterations any remaining active members leave the battle
party and receive X=-1, retaining joined flags; the RAM lists are not rebuilt
inside that tail. With13 active and a death among the first12, the thirteenth
does not fill the gap. `JoinForce` normally limits active joining to12, but the
placement tail is a separate contract for supplied larger lists. The source
still lacks a pre-loop zero-party guard; corrupt/empty entry is not a natural
premise established here.

Healing scans all30 allies, not just the party: ordinary dead members stay dead;
Peter7/Lemon28 always receive HP/MP restoration before placement. This does not
establish their natural presence. For handled allies `status & 7` is intermediate:
the subsequent `UpdateCombatantStats` recomputes CURSE from equipped cursed items,
preserves STUN/POISON and refreshes derived stats. A copied CURSE bit alone is not
a guaranteed final curse. The accepted predecessor retains this complete rule.

All enemy records have ATTACKER1/set6, NONE/NONE orders, filler0 and STARTING.
Their complete differences are:

| Slot / combatant | Base type and start | Primary/secondary regions | Placement item |
| --- | --- | --- | --- |
| 0 / 128 | HUGE_RAT1 `(6,7)` | 0/15 | NOTHING |
| 1 / 129 | OOZE0 `(5,8)` | 0/15 | NOTHING |
| 2 / 130 | HUGE_BAT40 `(8,26)` | 15/15 | `MEDICAL_HERB\|USABLE_BY_AI\|UNUSED_ITEM_DROP` |
| 3 / 131 | HUGE_RAT1 `(13,12)` | 15/15 | NOTHING |
| 4 / 132 | OOZE0 `(9,14)` | 15/15 | NOTHING |
| 5 / 133 | OOZE0 `(14,16)` | 15/15 | NOTHING |

NONE is order255; region15 is a separate sentinel, not polygon15. Set/get region
consumers pack/unpack two nibbles; move orders remain separate bytes. There is
no AI point, follow target or MOVE_TO operand to resolve in this selection.

## Enemy refresh, the bat's item and completed repeats

**Confirmed:** fresh enemy initialization scans32 slots beginning128. These six
mode0 records initialize directly; unused slots get max/current HP0 and X=-1.
The occupied-start carry test belongs to spawning reset, not these initial rows.
`InitializeEnemyStats` upgrades first, copies the selected56-byte definition,
restores current HP/MP, installs X/Y and type, combines movement with the row's
commandset, stores regions/orders, adds the placement item, assembles activation,
adjusts base ATT for difficulty, then calls `UpdateCombatantStats`.

For an uncompleted encounter, the full selected definitions have initial status0,
MP0, no spells and four NOTHING item slots. The stats needed to distinguish the
three base types are:

| Type | HP | Base ATT/DEF/AGI/MOV | Movement / defining difference |
| --- | --- | --- | --- |
| OOZE0 | 9 | 9/6/5/4 | REGULAR; ice resistance/fire weakness |
| HUGE_RAT1 | 10 | 10/8/7/6 | STEALTH; no base resistance |
| HUGE_BAT40 | 11 | 12/8/8/6 | FLYING; wind weakness; CRITICAL_SLEEP prowess |

All have PRIORITYMOD_2; rats/oozes also declare BIT5. Assembly preserves only
the activation high nibble, so all six initially get `0x2000`, independent of
commandset6 and absent primary-active bit. `InitializeCurrentStats` copies base
ATT/DEF/AGI/MOV/resistance/prowess to current fields before status/equipment work.
Difficulty refresh always multiplies ATT by5 then shifts right2 once; SUPER
does it twice, with truncation each time. Thus ordinary current ATT is11/12/15
for Ooze/Rat/Bat, or13/15/18 on SUPER. These are refreshed base definitions,
not a promise for upgraded enemies, changed equipment or later status.

The bat's placement expression is word`0x6000` (Herb0 plus bits13/14).
`InitializeEnemyItems` calls `AddItem`, which masks with
`ITEMENTRY_MASK_INDEX_AND_BROKEN_BIT=0x807F` and writes the first free slot.
It therefore stores ordinary unequipped Herb0, clearing both declaration bits;
the equipped-item branch is not taken. The source AI attack-item getter requires
equipped or AI-use permission; the healing-item getter exempts Healing Rain8,
not Herb0, from the AI-use check. This bat's stored Herb cannot be selected by
either getter on that state. Do not infer self-healing or guaranteed drop from
the placement label. This distinction matches the existing selected production
trust owner; no newly discovered consumer repair is required here.

Unlike Battle02, Battle03 is in `list_RandomBattles`. `IsBattleUpgradable`
checks list membership and `DetermineBattleUpgrade` tests503. Clear503 keeps
each base type; set503 permits selection even after `CheckBattle` clears403.
F453 has no role in upgrades. The
[Battle14 upgrade owner](../southern-travel-creed/battle14-desert-encounter-and-outcomes.md#completed-repeats-and-actual-enemy-upgrade-selection)
owns the complete algorithm, category bounds/exclusions, retry and seed effects.
Here rats/oozes select melee (range5, types0–35 with its declared exclusions);
the bat selects airborne (range2, types40–52).

The actual consumer reads Bowie0's level/class, adds20 when signed class>11,
subtracts this battle's ID3, and for a positive difference divides by10 and
keeps the quotient's low byte q. q0 keeps base; ordinary inputs first reach
selection at effective level13. Melee offset is`5q−5`, airborne offset q;
each row makes its own range-inclusive main-RNG draw and can use one retry
from the saved offset baseline, then fall back after a second failure. A
successful new type changes stats, movement, equipment, spells and prowess;
placement coordinates, item addition, set6 and region/order operands remain.

The airborne exclusion macro emits count0, but the consumer subtracts1 without
a zero guard and can scan65,536 subsequent bytes unless an equality intervenes.
That accidental match takes the retry/fallback path; exhaustion can accept a
candidate. This is the known original defect, not a valid empty exclusion set
or a guaranteed safe base return. Actual bat substitutions and their natural
seed/level are **Unknown**. No out-of-table census, simulation or runtime
acquisition is used to invent an outcome. Upgrade draws also change the seed
later used by round generation; a repeat is not just the same roster reset.

## Region0, first-round activation and control endpoint

**Confirmed:** region0 is type4, byte1=0, vertices `(0,0)`, `(0,15)`,
`(19,15)`, `(19,0)`, final bytes0/0. The getter selects a12-byte record.
`IsRegionTriggered` splits it into triangles1/2/4 and3/2/4.
`AreAnyAlliesInTriangle` scans all30 ally slots, requires positive signed HP
and nonnegative X/Y, then calls the edge/side consumers, which include bounded
edges. It does not test enemies, template identities, map entities, joined flags
or the camera. These particular vertices form X0–19/Y0–15 inclusive.

After fresh loading, every living placed ally uses the templates above with
Y19–24; a dead/unplaced ally cannot trigger it. Therefore none can trigger0
at the first fresh round boundary, whatever valid joined IDs fill the templates.
Enemy128/129 being inside the rectangle does not activate them. This conclusion
joins the complete placements with the actual test subject/filters, rather than
guessing from polygon shape. It does not apply to retained suspended positions
or to later movement into the region.

`ActivateEnemies` scans32 slots and falls through to its update helper. The
helper skips negative-X/HP0 before invoking region processing. The region loop
tests not-yet-tested entries, sets F90 when0 succeeds and marks the tested bit
even when no ally triggered it. NEWLY_TRIGGERED_BATTLE_REGIONS is this tested
cache, distinct from persistent F90; `ClearAiMemory` does not clear it.
`StartAiControl` clears it on its ordinary enemy path. Fresh90–105 clear plus
the outside formation leaves enemy128/129's primary-active bit clear regardless
of whether an inherited tested bit skips that first check. Later a successful0
sets their primary-active bit; their secondary15 supplies no second trigger.

The built region-cutscene table contains only32/40 rows, so Battle03 has no
matching region program. This absent route is distinct from the selected empty
start script and from F453. All six spawn modes are0, and unused slots have no
spawn mode: the population pass supplies no fresh hidden/respawn candidate or
spawn animation. Declared, initialized, entity-loaded, activated and able to
act remain different states.

`LoadBattle` rebuilds entities from live combatant coordinates/mapsprites, with
enemy-neutral filtering, loads map/sprites/terrain/music and requests fades;
it does not rerun enemy stats or turn Map66's ordinary entity list into this
roster. No Battle03 neutral-entity table entry adds another actor.

At ordinary AI preparation,128/129 have0/15 and inactive primary bits, so enter
`DetermineAiStandbyMovement` and finish with action STAY. NONE orders plus a
configured primary region select the regular starting-position anchor when
the standby RNG/eligibility permits movement; STAY may retain a move string.
The bat and slots3–5 have15/15, bypass that gate and reach regular set6 even
with no activation bit set. Set6 attempts ATTACK1, HEAL1, SUPPORT, MOVE1, STAY
in order, with regular pathfinding, stopping on command success. The ally-row
HEALER1 declaration is not copied; AI-controlled allies use source set6 instead.
Base no-spell loadouts and the masked herb do not establish every later action;
an upgraded type can make other action families relevant. Deeper AI predicates
remain with the [AI owner](../../battle/battle-ai.md#dispatcher-and-standby-control).

Round generation and first control reuse
[the complete common rule](./battle02-initialization-and-first-control.md#first-round-and-conditional-control-endpoint):
64 sentinel slots, placed/living candidates from30 allies and the first30 enemy
slots, current numerical AGI and main-RNG draws, optional high-bit second turn,
62 signed-byte stable sort passes, cursor0. All six enemies are candidates
without requiring active-region bits. Base AGI5/7/8 has no double-turn flag;
changed ally state or upgraded definitions are not frozen to those values.
Range0 draws still advance the seed; repeat-upgrade draws precede the round.
No seed reset, first hero or first enemy is prescribed here.

A sentinel requests another round; otherwise the selected byte enters
`ExecuteIndividualTurn`. HP0 skips; MUDDLE, AI-controlled bit2, enemy control
mode or ally auto-battle choose AI preparation. After common cursor/view/grid
work, SLEEP/STUN can select NoAction; otherwise control conditions choose actual
AI execution or player input. This is the endpoint, not a completed action or
guaranteed first player-ready state. A suspended caller may use its retained
queue/cursor instead of this new round.

## Existing producer and production boundary

**Confirmed by current code inspection, separate from original evidence:**
the [trust](../../../../remake/docs/content/profiles-and-trust.md#selected-third-encounter)
and [Application](../../../../remake/docs/application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation)
owners already describe selected Battle03 consumption. The current implementation
does not prove original behavior or general Battle03 parity.

| Actual consumer | Expressed relation and retained limit |
| --- | --- |
| [prepare_encounter / compile_routes](../../../../src/sf2tool/remake_exploration_battles.py) | Already admits2/3, checks pinned source/ROM identity, retains complete placements/behavior, region0, no points, metadata and terrain. Route compilation selects before/start targets and intro453; the reached SetEgressMap instruction writes the shared story state. No preparation/lowering rerun is needed for this account. |
| [PrivateBattleScenarioReader.Assemble](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs) | Retains all12 formation positions, region geometry, STARTING mode, set6 and distinct0/15 versus15/15. Adds masked Herb0 into the copied bat loadout; binds Flying/sleep prowess through [action bindings](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs). Controlled ally inputs must match template numeric identities0–11, supported classes and definitions; unused actors reject. Original placement instead takes arbitrary live joined/active IDs0–29. RegionProgram.None is explicitly admitted for1/2/3, independently of geometry. |
| [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) | Orders eligible routes, before/load/start and453 gates, clears90–105, rebuilds actual membership, carries live allies/growth/accounting/gold/both seeds and defaults only absent actors/enemies. Missing member definitions and F88 entry reject. PrivateDefinitions policy permits completed admission, but neither this class nor the definition reader implements original503/category/level upgrade selection; repeat entry alone does not prove upgraded definitions. |
| [BattleInitializationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs), [BattleTurnFlow](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) | Controlled roster-only/already-refreshed policy, difficulty0, living/immortal healing, dead/unplaced state, formation cursor and enemy high-nibble activation are expressed. Enemy supported loadouts are only0/127. Excess active count rejects rather than applying the original excess-party tail. General equipment/status refresh, other difficulty/control policies and saved queue restoration remain Unsupported; the controlled policy does not establish arbitrary CURSE refresh equivalence. |
| [BattleActivationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [TurnOrderRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs) | Living/placed allies test region triangles; primary/secondary flags update each living placed enemy before randomized rounds. Region programs other than None and nonzero spawn modes reject. The source's general integer-projection geometry and inherited tested-cache lifetime are not claimed equivalent for arbitrary polygons/state by this selected rectangle. |
| [BattleAdvancer](../../../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs), [BattleControlRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) | Actual queue drives automatic work or player admission, with sleep continuation and explicit unsupported control/status boundaries. No recorded actor/seed chooses gameplay legality. A visible player stop may follow automatic work. |
| [SourceEnemyAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs) | Uses region assignment/activation independently of commandset, routes0/15 standby versus15/15 ordinary set6, and admits ordinary Herb0/no spells because source getters cannot select that herb. Activated sets6/7/9/14 and selected physical/pursuit behavior are supported; enabled AI-item bits, learned spells, other equipment/status and arbitrary upgraded loadouts remain Unsupported. |

These are declared admission/conversion/support limits, not authorization to
repair an engine, add fixtures or regenerate content. No candidate consumer
defect was found requiring a separate implementation change in this review.
The [four frontiers](../../story-content-coverage.md#four-different-frontiers),
selected production support and original Battle01 observations remain distinct.

## Unknowns and verification boundary

**Unknown:** natural arrival flags/party/HP/status/level/RNG, continuous journey,
actual first actor, bat repeat substitutions through the unsafe scan, full action
and tactical legality, presentation/hardware timing and saving. A complete
selected static account does not resolve these. The #1047 twelve **completed
unresolved comparisons** remain at their
[verification owner](../../../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03);
they are not interrupted runs or an automatic repair/acquisition queue. Accepted
Battle01 natural admission, victory, stable return and later Down observation
are not downgraded.

Reproduction is bounded read-only review of the named complete source/consumer
bodies and direct document checks: relative links/anchors, UTF-8/LF, three-path
ownership, full diff, private boundary and unchanged fixture counters/frontiers.
The retained map-init/map-script outputs and `local/issue1005/pair.txt` remain
unchanged; new small receipts belong under ignored `local/issue1216/`.
Mechanical PASS does not replace independent semantic acceptance.

On a clean committed head, load the existing ignored configuration in the same
process and record the plan and actual exact-head scoped CI on the Issue/PR:

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify plan --base origin/main --head HEAD
```

No normal/full, SDK/Godot/emulator/H3/H4, screenshots, comparisons, lowering,
extraction or acquisition is part of this documentation acceptance. Stop at a
clean pushed frozen Draft PR, Review / Verify handoff and stopped processes;
main-gate retains independent review and integration.
