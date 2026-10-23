# Battle04 opening, initialization and first control

Evidence date: 2026-10-22. **Confirmed:** Battle04's complete5/8/3/1 selection
connects a separate scene population to live-party initialization, three region
tests, distinct standby/ordinary AI paths and individual-turn control. It is not
a random-upgrade encounter. **Inferred:** the dialogue and staging express the
Galam ambush following Hawel's house. **Unknown:** natural entry/party/status/RNG,
the first actor, every action, presentation/timing and save durability. Static
composition is not a played continuation or forced loss.

## Scope and provenance

This is the **Standard**-rigor agreement for
[Issue1218](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1218),
under [Epic1053's finite denominator](./story-content-coverage.md#scope-and-reading-rules).
The outcome supports content maintenance and independent conversion review:
explain the complete selected encounter through its first conditional
player/AI/no-action dispatch, including changed valid party state. Reuse the
[Battle02 lifecycle](./battle02-initialization-and-first-control.md) and
[Battle03 differences](./battle03-initialization-and-first-control.md); the
[Hawel/admission owner](./battle03-to-battle04-content-route.md) and
[outcome/captivity owner](./battle04-to-battle05-content-route.md) keep their
existing field scenes and outcome connections.

Owned writes are this document and Battle04 narratives in the two coverage
owners. Source, consumers, fixtures, registries, counters and runtime frontiers
remain read-only. No new parser, fixture, schema, implementation, allowlist,
platform, extraction or acquisition belongs to this slice. Acceptance is direct
semantic/document/private-boundary review, committed-head planner and actual
exact-head scoped CI, then a clean pushed frozen Draft PR for independent review.

All original paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The selected checkout was clean at that pin. Accepted repository base is
`7fc195abc26bc67cb80115b9faddf657a0784109`. The
[USA baseline](../../README.md#pinned-original-baseline) and existing fixtures
retain input identity; this account makes no fresh ROM-equivalence claim.
Read complete selected functions and return tails; `j_` interfaces resolve to
the named implementations. Short battle paths resolve under
`code/gameflow/battle/`.

| Claim | Selected declaration and actual consumer |
| --- | --- |
| All formation/behavior operands | `data/battles/spritesets/entries.asm` slot4 → `spritesets/spriteset04.asm:BattleSpriteset04`, `0x1B34DC..0x1B35A2`; `sf2macros.asm:allyCombatant/enemyCombatant/combatantAiAndItem/combatantBehavior`, `sf2enums.asm`; `getbattlespritesetsubsection.asm:GetBattleSpritesetSubsection`, `0x1B1630`. |
| Admission, metadata and hooks | `code/common/maps/getbattle.asm:CheckBattle`, `code/gameflow/mainloop.asm:MainLoop`; `data/battles/global/battlemapcoords.asm` row4, `terrainentries.asm` slot4, `global/custombackgrounds.asm`, `enemyleaderpresence.asm`, `halvedexpearnedbattles.asm`; `battleloop/loadbattleterraindata.asm`, `battlescenes/getbattlescenebackground.asm`, `battleactions/giveexpandgold.asm`; before/start wrapper start/end files under `cutscenes/` and tables under `data/battles/cutscenes/`. |
| Complete opening | `data/battles/entries/battle04/cs_beforebattle.asm:bbcs_04/ce_4989E`, `0x4981E..0x498EE`; `sf2cutscenemacros.asm`; `code/common/scripting/map/mapscriptengine_1.asm:csc37/csc48/csc42/csc15/csc19/csc32/csc23/csc26/csc2D/GetEntityAddressFromCharacter/AdjustScriptPointerByCharacterAliveStatus`, `mapscriptengine_2.asm:ExecuteMapScript/csc00/csc02/csc04`; `mapfunctions.asm:InitializeMapEntities`, `followersfunctions_1.asm:InitializeFollowerEntities`; `data/scripting/entity/followers.asm`, `eas_actions.asm`; `data/scripting/text/gamescript.txt` keys0907–090E. |
| Party/enemy refresh and entity replacement | `battleloop_1.asm:BattleLoop`, `0x23A84`; `battleloop/heallivingandimmortalallies.asm`, complete `initializecombatants.asm`, `0x1B1272..0x1B15F8`; `code/common/stats/battleparty.asm`, `updatecombatantstats.asm`, `itemstats.asm:AddItem`; `data/stats/enemies/enemydefs.asm` records2/40/56, `data/stats/items/itemdefs.asm` records0/33/45; `battlefunctions/loadBattle.asm:LoadBattle`, `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities`. |
| Upgrade, activation and AI | `battleloop/upgradeenemies.asm:IsBattleUpgradable/UpgradeRandomBattleEnemies`, `data/battles/global/randombattles.asm`; `battleloop/clearaimemory.asm`, `activateenemies.asm`, complete `triggerregions.asm`, `populateenemyspawns.asm`; both `cutscenes/regionactivatedcutscenes.asm` and its data table; `data/battles/global/battleneutralentities.asm`; `ai/startaicontrol.asm`, both `determineaistandbymovement_*.asm`, `executeaicommand.asm`, `command/moveorder.asm`, `getnextuseableaiaction.asm`; `battlefield/getmoveorderposition.asm`; `data/battles/global/aicommandsets.asm`; `code/common/stats/combatantstats_1.asm:GetMoveOrders/GetTriggerRegions`, `combatantstats_2.asm:SetTriggerRegions`. |
| Round and stopping point | `battleloop/turnorderfunctions.asm`, `0x25544`; `code/common/tech/randomnumbergenerator.asm:GenerateRandomNumber`; complete `battlefunctions/executeindividualturn.asm`, `0x23EB0..0x24242`. |

Shared [spriteset](./battle-spriteset-data.md), [global](./battle-global-data.md),
[routing](./battle-routing-data.md), [loop](./battle-loop.md),
[AI](./battle-ai.md) and [cutscene](./battle-cutscenes.md) owners retain their
inventories and observations. Existing `tests/fixtures/h2/` contracts
`battle-spriteset-data-static-v1.json`, `battle-global-data-static-v1.json`,
`battle-routing-data-static-v1.json`, `battle-loop-static-v1.json`,
`battle-control-static-v1.json`, `battle-ai-static-v1.json` and
`battle-cutscenes-static-v1.json` remain shared-scope evidence. In particular
`rowContentTracked:false` and `routeTargetsParsed:false` do not become executable
proofs of these composed selected rows. The table above is the read-only
reproduction route.

## Selection and the three entry lifetimes

**Confirmed:** `BATTLE_AMBUSHED_BY_GALAM_SOLDIERS=4` selects Map66, origin`(0,0)`,
area22×30, wildcard triggers255/255 at row4 (`0x7A52`). `CheckBattle` tests
map/unlock/trigger in table order: F402/F403 can select earlier66 rows first.
F404/F504 both set clears404 but still returns4 on that call. Dimensions are
outputs, not an inside-rectangle admission test. The predecessor owns the
conditional house producer of404 and its map request, not a mandatory played route.

Terrain slot4 aliases `BattleTerrain03`; its four-byte pointer selects a
Stack-decoded48×48 array, independent of the22×30 operating area. The global
background row is OVERWORLD; the actual getter uses the selected combatant's
terrain rather than one fixed image. Leader-presence row4 is0, so the defeated
wrapper's leader-death sweep is skipped. LEADER commandset14 does not override
that separate table. The halved-EXP scan matches Battle1 only, not4.

| Supplied gate state | Actual lifecycle |
| --- | --- |
| F88 clear, F454 clear | Shared fresh sequence: clear PLAYER_TYPE/set399, clear elapsed time, install incoming current map/battle for before, run `bbcs_04`, restore supplied arguments/current fields, clear90–105, heal, initialize allies then enemies, clear AI memory, load with focus0, run start wrapper, prepare round. Before slot4 (`0x47A90`) selects `bbcs_04`; it checks454 without setting it. Start slot4 (`0x47B34`) selects `ms_Empty` and its wrapper sets454 before dispatch. |
| F88 clear, F454 set | Both hook bodies skip; fresh healing/placements/enemy stats/loading/round still run. No before population/text/motion is reconstructed on this branch. F504 is completion, not the intro gate. |
| F88 set | Restore saved elapsed seconds, clear88/AI memory/view target, reload with incoming focus, go directly to retained queue/cursor. No fresh current-field installation, hooks, healing, region reset, placement or new-round generation precedes dispatch. A retained sentinel subsequently requests round preparation. Saved fields/positions/queue must already exist; successful persistence is Unknown. |

Empty start termination can wait for scrolling and clears scroll speed. No
Battle04 before instruction sets EGRESS_MAP: the predecessor's conditional
Hawel assignment is carried state, not an unconditional Battle04 effect.

## Opening scene, text keys and actual state effects

**Confirmed:** `bbcs_04` sets text cursor2311, requests Map66 camera`(1,5)`
through load/fade, then loads `ce_4989E`. `csc37` falls into `csc48`'s map load;
`csc42` reads the three-word main tuple and invokes `InitializeMapEntities`,
then mapsprite loading. This is an in-script scene load, not another battle
selection or the later combatant-derived `LoadBattle`.

The complete authored scene declaration is main Bowie`(5,9,Down)` and these
nine `eas_Init` entries. Logical identities and mapsprite declarations differ
from combatant slots128–135 below.

| Authored entity / logical key | Declared position/facing | Declaration meaning |
| --- | --- | --- |
| Kazin4 | `(6,9,Down)` | Ally mapsprite lookup, not JoinForce |
| Sarah1 | `(5,8,Down)` | Ally mapsprite lookup |
| Chester2 | `(5,7,Down)` | Ally mapsprite lookup |
| Jaha3 | `(6,8,Down)` | Ally mapsprite lookup |
| Lemon28 | `(13,17,Up)` | Ally mapsprite lookup; scene commander, not the selected enemy leader |
| 128 | `(14,17,Up)` | MAPSPRITE_GALAM_ARCHER |
| 129 | `(11,18,Up)` | MAPSPRITE_GALAM_SOLDIER |
| 130 | `(12,18,Up)` | MAPSPRITE_GALAM_SOLDIER |
| 131 | `(14,16,Up)` | MAPSPRITE_GALAM_SOLDIER |

These are declarations, not a guaranteed ten-entity natural snapshot.
`InitializeMapEntities` clears entities and declares flag-selected followers
before the explicit stream. Existing ally mappings suppress duplicate authored
ally declarations; extra followers can exist. The follower table includes
F66→Sarah/Chester,72→Kazin and71→Lemon, independently of joined/active flags;
F65/overworld selection can also change following. Main0 is declared separately.
The subsequent named commands resolve the rebuilt lookup. Incoming follower/HP
state can therefore affect choreography even on a clear454 branch. Being drawn
does not prove joining, health, active membership or turn eligibility.

The rest of the complete before program is ordered as follows:

1. Wait for Bowie's `eas_Init`; set Kazin's scene position/destination/facing
   to`(6,9,Down)`; fade in; wait for Kazin's `eas_Jump`. Init configures entity
   movement/flags/size and returns to idle; Jump returns to idle after relative
   vertical motion. Neither initializes combatant stats.
2. Display2311, move camera to`(7,13)`, then display2312–2317. The table below
   retains speakers, modifiers and keys without publishing a transcript.
3. Face Lemon Down, script wait30, face Lemon Right and scene128 Left, wait20,
   display2318, then nod scene128. These are script/Sleep operands, not measured
   elapsed presentation.
4. Two waited Lemon action sequences: Down1/Right2/Down2/Right1/Down1, then
   Right2/Down1; terminate. For the declared Lemon origin and completed motion
   this yields`(18,22)`; an inherited follower origin is not that premise.

`csc19`/`csc23` first call `AdjustScriptPointerByCharacterAliveStatus`: HP0
ally targets skip their position/facing instruction. Before runs **before**
immortal healing, so later healing of Lemon28 does not justify unconditional
before facing/position claims. Action-script/sequence/nod handlers operate on
entities, not their battle HP or roster flags.

| Decimal / source hex key | Speaker / modifier | Minimal content and tokens |
| --- | --- | --- |
| 2311 /0907 | Kazin /`$00` | Identify strangers; `{W1}` |
| 2312 /0908 | Kazin /`$00` | Recognize Galam soldiers; `{W1}` |
| 2313 /0909 | Lemon /`$C0` | Hostile greeting; `{W1}` |
| 2314 /090A | Kazin /`$00` | Ask about Hawel's death; `{N}` twice, `{W1}` |
| 2315 /090B | Lemon /`$C0` | Hesitation; `{W1}` |
| 2316 /090C | Kazin /`$00` | Demand an answer; `{W1}`; the sole `nextText` here |
| 2317 /090D | Kazin /`$00` | Address `{LEADER}` and propose fighting for the truth; `{N}` twice, `{W1}` |
| 2318 /090E | Lemon /`$C0` | Order capture alive; `{W1}` |

`textCursor` writes CUTSCENE_DIALOG_INDEX; ordinary `csc00`/`csc02` display
and increment it. Single-text calls also close portrait/text and sleep10;
`nextText` leaves the windows for continuation. `$C0` combines right-side and
mirror bits; `$00` adds neither. The interpreter clears SKIP_CUTSCENE_TEXT on
entry, but debug/P2 Start can set it during execution; skipped00/02 consume
operands without displaying or incrementing, and script waits also skip.
The keyed sequence above describes normal text dispatch, not debug playback.

There is no before join, unlock/completion/region write, item/gold operation,
combatant HP-zero write, capture status, warp request or new victory predicate.
The capture sentence is dialogue; it does not make Battle04 unwinnable. The
[outcome owner](./battle04-to-battle05-content-route.md#battle04-outcome-branches)
already distinguishes victory, special defeat and escape, including different
returns and the completed-repeat capture gate.

## All selected combatants and refresh

**Confirmed:** header5/8/3/1 drives subsection arithmetic: four header bytes,
13 combatant records of12 bytes, three region records of12 and one2-byte
point. Ally templates0–4 are`(6,6)`, `(6,7)`, `(5,6)`, `(5,7)`, `(4,7)`.
Every ally row declares HEALER1/0, NOTHING, NONE/NONE orders,15/15 regions,
filler0 and STARTING0. Ally placement copies **only X/Y** to actual ascending
joined+active IDs0–29 from `UpdateForce`, not those template identities or AI.

Apply the accepted
[member-scan/cursor rule](./battle02-initialization-and-first-control.md#joined-active-dead-and-capacity)
with a five-iteration limit. Ordinary dead active members consume a member
iteration but no template; a sixth active member still leaves the active party
after the first five scans, even if an earlier death left a placement unused.
Joined flags remain and the tail does not rebuild RAM lists. Reserve members
stay unplaced. This is neither five fixed heroes nor scan-until-five-live.
The source lacks a pre-loop zero-party guard; empty/corrupt entry is not a
natural premise. General JoinForce capacity12 remains a different limit.

Healing scans all30 allies; ordinary HP0 stays dead, Peter7/Lemon28 always
receive HP/MP restoration. For handled allies `status & 7` is intermediate:
`UpdateCombatantStats` preserves STUN/POISON and recomputes CURSE from equipped
cursed gear. The previously corrected rule is retained; an old copied CURSE bit
alone does not establish final curse. Scene Lemon and an actually joined/active
Lemon are separate consumers of the same numeric identity.

All eight enemy rows have secondary order NONE, filler0 and STARTING0.
These are the complete differing operands; slots refer to combatant identities,
not the scene lookup above. Region15 is a sentinel, separate from order255.

| Slot / combatant | Base definition / position | Commandset | Primary order | Primary/secondary region | Placement item |
| --- | --- | --- | --- | --- | --- |
| 0 /128 | GALAM_ARCHER56 `(17,21)` | LEADER14 | MOVE_TO0 (`$40`) | 2/15 | NOTHING |
| 1 /129 | GALAM_SOLDIER2 `(8,12)` | ATTACKER1/6 | NONE (`$FF`) | 15/15 | NOTHING |
| 2 /130 | HUGE_BAT40 `(7,14)` | ATTACKER1/6 | NONE | 15/15 | MEDICAL_HERB\|USABLE_BY_AI\|UNUSED_ITEM_DROP |
| 3 /131 | HUGE_BAT40 `(8,23)` | ATTACKER3/9 | FOLLOW_TARGET0 (`$00`) | 1/15 | MEDICAL_HERB\|USABLE_BY_AI\|UNUSED_ITEM_DROP |
| 4 /132 | GALAM_SOLDIER2 `(14,10)` | ATTACKER1/6 | NONE | 0/2 | NOTHING |
| 5 /133 | GALAM_ARCHER56 `(13,12)` | ATTACKER1/6 | NONE | 0/2 | NOTHING |
| 6 /134 | GALAM_SOLDIER2 `(11,18)` | ATTACKER1/6 | NONE | 2/15 | NOTHING |
| 7 /135 | HUGE_BAT40 `(15,20)` | ATTACKER3/9 | FOLLOW_ENEMY0 (`$80`) | 0/1 | NOTHING |

Point0 is`(17,21)`. `GetMoveOrderPosition` tests bit6, indexes two-byte points
by low nibble, otherwise reads target combatant X/Y. Thus128 targets point0;
131 follows actual ally0;135 follows enemy128. The latter two track live
coordinates, not point0 or template0. `GetMoveOrders` returns separate bytes;
`Set/GetTriggerRegions` pack/unpack two nibbles. No secondary order or extra
point is implied by secondary activation.

Fresh initialization visits32 enemy slots from128; these eight mode0 rows
initialize directly, and unused slots get max/current HP0 and X=-1. The occupied
spawn-position carry check is not an initial mode0 admission rule.
`InitializeEnemyStats` calls upgrade selection, copies the selected56-byte
definition, restores HP/MP maxima and coordinates, combines movement's high
nibble with the declared commandset, installs type/regions/orders, adds the
placement item, keeps the definition AI word's high nibble while installing
spawn/filler, adjusts base ATT for difficulty, then refreshes current stats.

| Definition | Level; HP/MP; base ATT/DEF/AGI/MOV | Movement / base loadout / other relevant fields |
| --- | --- | --- |
| GALAM_SOLDIER2 | 2;11/0;8/10/8/6 | REGULAR; equipped SHORT_AXE33, three empty items; resistance0; critical150 prowess; AI PRIORITYMOD_2\|BIT5 |
| HUGE_BAT40 | 0;11/0;12/8/8/6 | FLYING; four empty items; WIND_WEAKNESS; sleep-critical prowess; AI PRIORITYMOD_2 |
| GALAM_ARCHER56 | 3;15/0;7/10/10/5 | ARCHER; equipped WOODEN_ARROW45, three empty items; resistance0; critical150 prowess; AI PRIORITYMOD_2\|BIT4 |

All have status NONE, spellPower REGULAR, four NOTHING spells, double/counter
prowess1-in32 and no AGI high-bit second turn. Soldier/archer critical150 has
chance1-in32. The initializer preserves only the AI word's high nibble:
BIT4/BIT5 declarations do not survive as initial low bits. They are not the
dedicated commandset or leader-presence flag.

Short Axe has range1 and equip ATT+5; Wooden Arrow has range2 exactly and ATT+7.
`UpdateCombatantStats` resets derived fields from base, then applies equipment;
base ATT is not final ATT. The source's redundant difficulty branch applies
floor(baseATT×5/4) once for ordinary difficulty and twice for SUPER, before
weapon bonuses. For difficulty0 these selected final ATTs are15/15/15 for
soldier/bat/archer; AGI remains8/8/10. This is source derivation, not a natural
save's difficulty or combat-damage observation.

Both bat placement items pass `InitializeEnemyItems`→`AddItem`, which masks
with`$807F`: declared`$6000` becomes ordinary unequipped Herb0 in the first
empty slot; equip handling does not run. Neither AI-use nor unused-drop bit
survives. Attack-item selection cannot use it; healing-item selection skips it
without AI bit13 (the Healing Rain exception is item8, not0). A declared item
bit is not proof of AI healing or an awarded drop.

`IsBattleUpgradable` scans the length-prefixed random list, which includes3
but omits4. `UpgradeRandomBattleEnemies` therefore restores each original type
and returns **before** completion, level/category or RNG selection. F504 set
does not upgrade these eight types, consume upgrade RNG or enter Battle03's
unsafe airborne exclusion scan. That retained original defect remains at the
[Battle03 owner](./battle03-initialization-and-first-control.md#enemy-refresh-the-bats-item-and-completed-repeats).
This is a selector result, not a correction to the original.

## Three regions, first activation and control endpoint

**Confirmed:** all three regions are type4, byte1=0, final bytes0/0; complete
ordered vertices are:

| Region / flag | Vertices1,2,3,4 | Selected geometry |
| --- | --- | --- |
| 0 /F90 | `(7,0)`, `(7,29)`, `(19,29)`, `(19,0)` | Inclusive rectangleX7–19/Y0–29 |
| 1 /F91 | `(0,13)`, `(0,29)`, `(21,29)`, `(21,13)` | Inclusive rectangleX0–21/Y13–29 |
| 2 /F92 | `(0,20)`, `(0,29)`, `(20,29)`, `(20,0)` | Slanted upper edge from`(0,20)` to`(20,0)`; not rectangleY20–29 |

`IsRegionTriggered` tests triangles1/2/4 and3/2/4. The edge consumer uses
signed integer projection/division, bounded-edge inclusion and side comparisons,
not a polygon bounding box. `AreAnyAlliesInTriangle` scans all30 ally slots,
requires positive signed HP and nonnegative X/Y, and tests their actual combatant
positions. It does not test enemies, scene actors, template IDs or camera location.

After fresh placement, living allies can occupy only the five positions above;
all other ally X values were cleared. Every position has X≤6 and Y≤7: outside0
and1. For2 their X+Y≤13 is strictly before the slanted edgeX+Y=20; both source
triangle tests exclude them, with no near-edge rounding ambiguity at these
coordinates. Thus no region triggers at the first fresh boundary for any valid
membership filling those templates. Dead/unplaced allies cannot change that
result. Enemies inside regions do not activate them. This conclusion does not
apply to suspended retained positions or later moves.

`ActivateEnemies` scans32 slots and falls through to its update helper;
negative X/HP0 skip. Region processing sets90–92 on successful tests and
marks each tested bit even on failure. NEWLY_TRIGGERED_BATTLE_REGIONS is that
cache, not persistent flags; `ClearAiMemory` does not clear it, while ordinary
enemy `StartAiControl` does. Whether a stale tested bit skips a first check or
an outside formation is tested, fresh90–105 clear means no selected activation
bit becomes set here. Later primary success sets bit0; if primary does not
succeed, secondary success sets bits0/1. Primary takes precedence when both
flags are set; activation does not itself replace either order byte.

Battle04 has no region-cutscene row (the built table selects32/40 only), and
no neutral-entity row. All selected spawn modes are0; unused slots also have
no spawn mode. There is no hidden/respawn candidate or spawn animation from
this pass. `LoadBattle` clears/rebuilds entities from live combatant coordinates/
mapsprites with neutral filtering, then loads map/sprites/terrain/music and
requests fades. It does not retain the declared scene actors as a combat roster
or rerun stats. Declared, initialized, entity-loaded, activated and able to act differ.

At ordinary first AI dispatch, only129/130 have15/15 and bypass the active
bit gate to regular set6. The other six enter standby with STAY action.
`ValidateAiStandbyEligibility` and its caller's actual `tst.b d1`/nonzero-stop
branch, rather than the misleading allow/disallow comments, give two cases:
128/131/135 have special primary orders with configured regions and stop with
move string`-1`;132/133/134 have NONE orders and configured primary regions,
so can attempt local movement around their declared starting positions after
the standby RNG/eligibility tests. STAY need not mean no movement.

Once activated, exact set6/9/14 command sequences and regular/block-non-movable/
block-and-carve modes are the
[accepted common matrix](./battle02-initialization-and-first-control.md#complete-declaration-and-order-operands).
Set14 aliases set13's command pointer, not the leader-death table. Follow
preparation promotes a dead target's secondary order (NONE here) before command
dispatch. Move-order commands check MOV/order/target HP, may attempt an attack
before movement, and can build movement followed by STAY. AI-controlled allies
use set6, not the templates' HEALER1. This selects operands and routing without
simulating every later tactic/action.

Rounds reuse the
[complete common rule](./battle02-initialization-and-first-control.md#first-round-and-conditional-control-endpoint):
64 sentinel slots, placed/living candidates from30 allies and the first30 enemy
slots, current numerical AGI/main-RNG draws, optional AGI-high-bit second turn,
62 signed-byte stable sort passes and cursor0. All eight selected enemies can
enter the queue independently of active-region bits. No upgrade draws precede
this round, but no seed reset or fixed first actor follows. Range0 draws still
advance the main seed; standby uses the separate thinking RNG.

A sentinel requests another round. A reached actor enters `ExecuteIndividualTurn`:
HP0 skips; MUDDLE, AI-controlled bit2, enemy/opponent-control mode and ally
auto-battle select AI preparation. After cursor/view/status windows/grid work,
SLEEP/STUN select NoAction; otherwise control predicates select actual AI
execution or player input. Stop at that dispatch, not an executed action or
guaranteed first player-ready state. Suspended entry may use the saved queue.

## Current conversion and production consumers

**Confirmed by current accepted code inspection, separate from original evidence:**
Battle04 is not admitted, converted or supported by the existing private encounter
path. The [trust](../../remake/docs/content/profiles-and-trust.md#selected-third-encounter)
and [Application](../../remake/docs/application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation)
owners describe selected2/3 consumption; primitive reuse cannot be extrapolated
to4. No preparation, lowering or implementation run was performed.

| Actual consumer | Reusable mechanism and explicit Battle04 boundary |
| --- | --- |
| [prepare_encounter / compile_routes](../../src/sf2tool/remake_exploration_battles.py) | Producer and CLI admit2/3 only, with exact selected headers and source/ROM checks. Route compilation can bind before/load/start programs and flags for a supplied selection; this does not produce Battle04 or prove its full before conversion. |
| [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) | Validates complete placements, geometry, points, metadata and pinned inputs/ranges. `Pin` admits1/2/3 only and rejects4. No4 pin/export was added. |
| [PrivateBattleScenarioReader.Assemble](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs) / [PrivateBattleActionBindings](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Requires region-program admission for1/2/3 independently of geometry. Already expresses sets6/9/14, point/follow orders, complete formation and masked Herb0. Further limits would matter even after a separately authorized admission: controlled class3/MAGE is absent from the class switch (0/1/2/4); ARCHER is absent from Mover; enemy AI expression allows PRIORITYMOD_n with optional BIT5, rejecting the archer's BIT4; Physical rejects weapon ranges other than1/1, unlike Wooden Arrow2/2. Class3 and ally Kazin4 are different numbers; the original templates do not require Kazin. These are independent rejections, not an observed Battle04 run or repaired defects. |
| [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) | Route/before/load/start/intro continuations, membership rebuild and carried resources/growth/gold/both seeds are reusable. Missing ally definitions and F88 suspended entry reject; original saved queue/cursor restoration is absent. It has no admitted4 content from the producer/reader above. |
| [BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) / [BattleTurnFlow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) | Controlled already-refreshed allies/roster-only/difficulty0 policy, living/immortal healing and formation placement exist. Excess active count rejects instead of applying the original excess-party tail. Enemy item admission allows0/127 only, so equipped Short Axe/Wooden Arrow need actual refresh support. General equipment/CURSE refresh, other difficulty/control policies and saved-state restoration remain Unsupported. |
| [BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs) / [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs) | Living placed allies, primary/secondary activation, RegionProgram.None, spawn0 and randomized rounds are reusable. Domain triangle cross-products do not establish equality with original integer-projection edge behavior or inherited tested-cache lifetime for arbitrary polygons/state. No Battle04 equivalence observation is claimed. |
| [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs) / [SourceMoveOrderAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) | Already distinguish commandset/order/activation, standby and15/15, live point/follow bindings and dead-target promotion. Activated AI admits only0/127 items and no learned spells; selected physical searches do not provide general ranged/equipped support. Primitive sets6/9/14 do not make this encounter playable. |
| [BattleAdvancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs) / [BattleControlRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) | Queue-driven automatic progression and reached player-state admission exist, with explicit control/status rejection. A visible player stop can follow automatic work; no fixed hero or recording defines legality. This consumer boundary is not original evidence. |

These are current admission/support limits, not newly discovered runnable
Battle04 failures or authorization for engine work. No candidate defect is
silently repaired, fixture invented or allowlist broadened. The
[four frontiers](./story-content-coverage.md#four-different-frontiers) stay distinct.

## Unknowns and verification boundary

**Unknown:** naturally reached404/454/504/88 and follower flags, party/HP/status/
level/difficulty/RNG, actual first actor, continuous travel, every AI/action and
full tactical legality, presentation/hardware timing and persistence. No fixed
five-character roster, seed, winning strategy or unavoidable loss is established.
The #1047 twelve **completed unresolved comparisons** remain at the
[verification owner](../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03);
they are not interrupted runs or an automatic repair/acquisition queue. Accepted
Battle01 natural admission, victory, stable return and later Down observations
are not downgraded. Earlier mechanical PASS that missed the intermediate-status/
equipment-CURSE distinction remains a semantic miss in the prior handoff; its
accepted correction is reused above, not erased by this document's checks.

Reproduction is bounded direct review of the complete named source/consumer
bodies. Direct checks cover relative links/anchors, UTF-8/LF, exact three-path
diff/ownership/private boundary, unchanged fixture counters and four frontiers.
Retained map-init/map-script outputs total18,285,979 bytes and remain unchanged,
as does `local/issue1005/pair.txt`. New small receipts belong under ignored
`local/issue1218/`; reuse the simple document recipe with explicit `origin/main`
diff and a fresh output receipt, including after commit. Mechanical PASS is
not independent semantic acceptance.

On a clean committed head load current ignored configuration in the same
process, then record the plan and actual exact-head scoped CI on Issue/PR:

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify plan --base origin/main --head HEAD
```

No normal/full, SDK/Godot/emulator/H3/H4, screenshots, comparison, lowering,
extraction or acquisition is part of this documentation acceptance. Stop with
a clean pushed frozen Draft PR, Review / Verify handoff and stopped processes;
main-gate retains independent acceptance and merge authority.
