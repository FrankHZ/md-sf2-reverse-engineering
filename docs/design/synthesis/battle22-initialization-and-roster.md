# Chessboard: Battle22 Initialization and Initial Roster

**Confirmed:** admitted fresh Battle22 has no local before/start scene, but still heals the
eligible party, assigns its starting positions, initializes sixteen chess enemies and loads the
battle map. With F522 clear, King Ponpei and the Maid are added as neutral actors.
**Inferred:** the names and formation stage the King's chessboard challenge.
**Unknown:** natural admission, the supplied party/current state, completed services and the
actual visible encounter. An empty scene does not mean initialization was skipped.

This English guide continues [Desktop conversations and the Chessboard challenge](desktop-kingdom-interactions-through-battle22-entry.md)
at its conditional BattleLoop admission. It consumes
[accepted Battle22 research](../../research/battle22-initialization-and-roster.md) and the
[battle control/lifecycle contract](../contracts/battle-control-lifecycle.md), under
[Issue #911](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/911) and the
[current Standard agreement](../documentation-roadmap.md#current-baseline-and-near-term-synthesis).
It stops **before ActivateEnemies on fresh entry**, or **before IndividualTurns_Loop on
suspended entry**. Tactics, AI execution, damage, outcomes, postbattle Desktop, Cotton Balloon
and Floor World are outside this guide. Continue with
[Chessboard outcomes and Desktop return](battle22-outcomes-and-desktop-return.md) for the
accepted outcome, rebuilt population, announcement and changed conversations; this guide's
pre-activation/current-state stop remains unchanged. Coordinates below are source tile operands;
rows are
zero-based. They are not observed movement or a prescribed party lineup.

## Four Independent Gates

**Confirmed:** the predecessor's surviving Map22 warp supplies `(0,0,Right)`. CheckBattle
selects row22 only when the map is 22 and F422 is set. Both trigger bytes are 255 wildcards;
the party need not be inside the battle-area rectangle for this source check. Selection writes
area origin `(9,0)` and size 30×30, then checks F522. F522 set clears F422 but still admits
Battle22 for that call. The predecessor retains the permission/zone/request conditions that
must precede this particular warp; a supplied request is not proof of a natural itinerary.

| Flag | Selected consumer | Consequence within this guide |
| --- | --- | --- |
| F422, unlocked | CheckBattle | Required for row22 selection; independent of intro/suspension. |
| F472, intro | Before/start wrappers | Set skips both scripts; clear permits their dispatch. |
| F522, completed | CheckBattle and PositionBattleEntities | Selection may clear F422; clear admits the two neutral actors, set skips them. It does not upgrade the chess types. |
| F88, suspended | BattleLoop | Selects current-state continuation instead of fresh initialization. |

F777 permission belongs to the earlier conversation/zone, rather than these battle gates.
F472 set does not prove the player saw an intro, F522 set does not prevent this already selected
call, and F88 set does not promise that the incoming map/battle pair matches current state.
None of these flags is a substitute for the other three.

## An Empty Opening Still Builds the Encounter

**Confirmed:** BattleLoop first clears PLAYER_TYPE and sets F399 before testing F88.
With F88 clear and incoming Map22/Battle22, the fresh sequence is:

1. Clear elapsed seconds, save the incoming map/battle words, assign CURRENT_MAP and
   CURRENT_BATTLE to 22, install base VInts and run the before wrapper.
2. After its return restore the incoming words and reassign map/battle. Clear region flags
   90–105, despite this roster having no region records.
3. Heal living/immortal allies; initialize ally positions, then enemies; clear AI memory.
4. Supply combatant0 to LoadBattle, then run the start wrapper.
5. Reach BattleLoop's Start label. **Stop before its ActivateEnemies call.**

Both complete slot22 before/start table entries select `ms_Empty`, the single `$FFFF`
terminator. There is no local dialogue, choice, recruitment, actor-deployment or map-load command
in either hook. The wrapper and shared script engine still matter:

| Wrapper with F472 clear | Flag effect | Completion boundary |
| --- | --- | --- |
| Before | Check F472; no write | Dispatch empty script, restore saved registers and return. |
| Start | Set F472 **before** dispatch | Dispatch empty script, restore saved registers and return. |

With F472 set, both skip script dispatch while the fresh healing/roster/load sequence remains.
The empty script still initializes script state, clears text-skip state and consumes its
terminator. Its tail conditionally waits for view scrolling when DIALOGUE_WINDOW_INDEX is
nonzero, then clears VIEW_SCROLLING_SPEED and returns. A shared wait can prevent return;
F472 may already be set during an unfinished start dispatch. Empty hooks establish neither
instant completion nor usable input. They do not replay Ponpei's earlier encounter scene.

## Which Allies Receive Starting Positions

**Confirmed:** the selected spriteset declares twelve ally placements. The
[complete placement table](../../research/battle22-initialization-and-roster.md#complete-spriteset-and-party-placement)
gives their ordered coordinates, from placement0 `(18,18)` to placement11 `(17,18)`.
These rows are positions for the current battle party, rather than named character assignments.

Healing runs first. Living allies, plus Peter7 and Lemon28 even when dead, receive maximum
HP/MP, retain only existing STUN/POISON/CURSE status bits and have stats refreshed. Other dead
allies stay dead. This step does not recruit anyone or change class/inventory.

Placement first marks all thirty ally X values absent (`-1`). UpdateForce rebuilds the battle
list from joined **and active** flags in ascending ally ID order, replacing a supplied stale
list order. Each living listed member receives the next placement X/Y. The row's numeric
index, item and AI fields are not copied into the ally.

The limit is **twelve processed party slots**, rather than twelve living placements. A dead
listed member consumes loop capacity without advancing the placement record; a later living
member uses that unconsumed position. Fewer living active members produce fewer placed allies.
Surplus members beyond those twelve processed slots have active flags cleared and X set to
`-1`; LeaveBattleParty does not immediately rebuild the list/count. Reserves and exploration
followers are not automatically included. The first list read has no empty-list guard, so
arbitrary empty/inconsistent state is not promised a safe empty deployment.

## The Sixteen Chess Pieces

**Confirmed:** spriteset22 declares twelve ally records, sixteen enemy records, **zero regions
and one AI point**. Its sixteen enemy rows become logical combatants128–143. The formation
contains one King, three Queens, two Bishops, two Knights, two Rooks and six Pawns:

| Row / logical combatant | Enemy type | Starting X/Y | Stored commandset |
| --- | --- | --- | --- |
| 0 /128 | King89 | `(15,5)` | LEADER |
| 1 /129 | Queen71 | `(17,5)` | ATTACKER3 |
| 2 /130 | Bishop80 | `(13,5)` | HEALER3 |
| 3 /131 | Bishop80 | `(19,5)` | HEALER4 |
| 4 /132 | Knight14 | `(11,5)` | SWARM |
| 5 /133 | Knight14 | `(21,5)` | SWARM |
| 6 /134 | Rook61 | `(9,5)` | ATTACKER2 |
| 7 /135 | Rook61 | `(23,5)` | SWARM |
| 8 /136 | Queen71 | `(15,6)` | SWARM |
| 9 /137 | Queen71 | `(17,6)` | SWARM |
| 10 /138 | Pawn13 | `(13,6)` | SWARM |
| 11 /139 | Pawn13 | `(19,6)` | SWARM |
| 12 /140 | Pawn13 | `(11,6)` | ATTACKER2 |
| 13 /141 | Pawn13 | `(21,6)` | SWARM |
| 14 /142 | Pawn13 | `(9,6)` | SWARM |
| 15 /143 | Pawn13 | `(23,6)` | ATTACKER2 |

The [accepted exact enemy table and decoder](../../research/battle22-initialization-and-roster.md#sixteen-enemy-records-and-immediate-decoding)
owns every primary/secondary order, numeric commandset and packed byte. King row0 stores
MOVE_TO point0; that sole point is `(15,5)`. Follow orders name enemy rows, not map-sprite or
physical actor IDs. Bishop row3 is the only row with a non-NONE secondary order: primary
FOLLOW_ENEMY9, secondary FOLLOW_ENEMY0. All rows store trigger region15 in both fields,
but there are no region records; 15 does not establish a fifteenth polygon.

All sixteen rows use STARTING initialization. Enemy definitions supply HP/MP maxima, copied
inventory and movement type; initialization fills current HP/MP, assigns row positions/type,
and combines movement/commandset, triggers, orders and inventory effects. Shared difficulty/stat
adjustment follows. Before activation, the initial activation words are
`$1000` for King, `$3000` for Bishops and `$2000` for the other pieces, with primary/secondary
active and neutral bits clear. Slots144–159 take the absent path: activation0, max/current HP0
and X=`-1`, without a promise to zero every field. The neutral loader can subsequently reuse
158/159. LEADER, HEALER and SWARM are stored labels, not executed tactics, healing or an outcome
rule. Nothing in this guide executes the point/follow orders or a spell.

### NOTHING Leaves Copied Equipment Intact

**Confirmed:** each enemy row stores the exact item word NOTHING (`$007F`). The item initializer
returns on that complete-word sentinel before AddItem; it does **not** erase the copied enemy
definition's inventory. King carries an equipped FLAIL; Queens/Bishops an equipped POWER_STICK;
Knights POWER_SPEAR; Rooks ROBIN_ARROW; Pawns BATTLE_AXE. Each definition's other three item slots
are NOTHING. The [definition inventory table](../../research/battle22-initialization-and-roster.md#row-nothing-versus-definition-inventory)
retains HP/MP maxima, movement types and the non-sentinel decoder's separate limits.

Enemy type Bishop80 is selected even though its definition comment says EVIL BISHOP; that comment
does not select a different enum. Battle22 is absent from the complete random-battle list, so
supplied F522 set does not upgrade these types. Battle21's random-upgrade behavior and opening
scene are not transferred here. Copied items/spells establish neither use, damage nor later drops
or rewards; supplied ally equipment is also not replaced by placement rows.

## Loading the Board Replaces Exploration Actors

**Confirmed:** LoadBattle uses CURRENT_MAP for tilesets/map selection, fades out, disables view
tracking, waits for fading and clears VInts. PositionBattleEntities clears old actor data and
lookups, then scans thirty allies and thirty-two enemy state slots. Nonnegative X/Y admits an
ordinary actor; the enemy pass also excludes neutral-marked slots. There is no additional HP
test in this actor scan. Fresh initialization supplied eligibility; existing resumed state may
therefore produce a different actor population.

Ordinary actors start Down with eas_Standing, using `(X & $3F)*384` and `(Y & $3F)*384`.
Allies resolve class-sensitive map sprites; enemies resolve sprites from their stored types.
The six chess types' sprites are King153, Queen135, Bishop144, Knight78, Rook125 and Pawn77,
all using ordinary sequential allocation. If N allies were successfully declared, fresh enemy
row r has physical index N+r; twelve placed allies put the sixteen enemies at physical12–27.
Skipped ally lookups are `-1`.

Exploration's chess NPCs137–152, Caravan128 and ORB153–155 are not carried over. Logical
combatant128 now means the King, while combatant137 means Queen row9. **Enemy type89, sprite153,
logical actor128 and physical actor N are different identifiers.** Previous follower aliases,
exploration physical indices/actions and Right-facing warp do not survive as battle mappings.

### Ponpei and the Maid Are Completion-Gated Neutrals

**Confirmed:** after ordinary allocation, F522 clear selects exactly two CHESSBOARD neutral rows:

| Actor | Combatant state slot | Script lookup | Local X/Y | Facing / sprite / action |
| --- | --- | --- | --- | --- |
| King Ponpei | 159 | B, offset31 | `(6,2)` | Down / KING_PONPEI188 / eas_Standing |
| Maid | 158 | A, offset30 | `(5,3)` | Down / MAID208 / eas_Standing |

Their loader writes max/current HP0, status0 and row X/Y, and ORs neutral mask `$0008` (bit3)
into activation. Fresh absent-slot initialization makes that activation exactly `$0008`.
With N placed allies and sixteen ordinary enemies, physical allocation is N+16/N+17,
or 28/29 with twelve placed allies. These are two additional neutral actors, rather than
enemy spriteset rows or party members.

The lookup writes predecrement from ENTITY_INDEX_LIST_ENEMIES, landing at offsets31/30
in the overall list: **B/A**, rather than enemy lookup offsets63/62. Ordinary combatant159/158
lookups remain `-1`; GetEntityIndexForCombatant(159/158) does not recover these actors. State
slots and script lookup keys must stay distinct. Nor are these the old exploration Ponpei134
and Maid133 mappings. F522 set skips the group and leaves fresh slots158/159 absent. No Caravan,
ORB or other resident is added. On resume the same loader may rewrite these neutral state slots;
it does not promise to preserve every saved field.

### Full Map Load, Cropped Board and View Target

**Confirmed:** LoadBattle restores its combatant argument (fresh0), resolves that actor for
VIEW_TARGET_ENTITY and reads chosen battle tile coordinates. A missing lookup falls back to
physical0 for those reads, without restoring membership or positioning Bowie. It then sets
**d0=-1** for target-based view placement and calls LoadMap with the nonnegative **d1=Map22**.
The latter selects a full map load. This differs from the predecessor's negative-d1 reloadMap,
which reuses its working layout; negative view d0 is not the map-reload selector.

The full load applies base layout, flag copies and chest overlays, then crops area `(9,0)`
size30×30 to working origin `(0,0)` for the battle. Layer1 spans local `(0,0)` through `(29,29)`.
Roster, neutral and point coordinates are already **battle-local**; initialization does not
subtract 9 from their X. The cropped board's source-layout correspondence is `(localX+9,localY)`:

| Selected position | Battle-local | Full Map22 correspondence |
| --- | --- | --- |
| King / point0 | `(15,5)` | `(24,5)` |
| Ally placement0 | `(18,18)` | `(27,18)` |
| Neutral Ponpei | `(6,2)` | `(15,2)` |

This relation is neither a warp nor evidence of walking from `(0,0)` onto the board. Changed
supplied area state changes the crop; LoadBattle does not rerun CheckBattle. Entity mapsprites,
base VInts, the existing terrain loader, music submission and fade-in follow; Battle22 has no
Battle44 timer-window branch. Required services must return before the start wrapper and fresh
stop can be reached. Earlier writes are not rolled back by a later unfinished service. Layout
geometry and service calls do not prove rendered terrain, heard music, exact timing or usable input.

## Suspended Entry Uses Current State

**Confirmed:** after the common mode clear/F399 write, F88 set restores saved elapsed seconds,
clears F88 and AI memory, clears VIEW_TARGET_ENTITY, calls LoadBattle and branches directly to
IndividualTurns_Loop. **Stop before that loop.** It bypasses fresh map/battle assignment, region
reset, healing, ally/enemy initialization and both intro wrappers. It does not explicitly supply
fresh combatant0 to LoadBattle. Both entries clear 48 last-target bytes to `$FF` and 48 AI-memory
bytes to zero; this reset is not execution of a turn or tactical simulation.

The loader rebuilds actors and loads the actual current map/area/combatants, with its applicable
completion/neutral effects. It does not reconstruct spriteset22's fresh roster. Existing HP, items,
status, positions, current turn/order and view argument may differ; an incoming22/22 pair does not
override an independently changed CURRENT_BATTLE on this arm. This source continuation does not
prove cross-process save restoration or persistence.

| Changed supplied premise | Conditional consequence |
| --- | --- |
| F422 clear at CheckBattle | No row22 admission, regardless of F472/F522/F88. |
| F472 set, F88 clear | Skip both scripts; fresh party/enemy/load work still runs. |
| F522 set with F422 set | Admit this selection and clear F422; no type upgrade; no selected neutrals. |
| Changed membership, dead or surplus members | Rebuild ascending active/joined list; twelve processed slots do not guarantee twelve living actors. |
| Changed ally class/items | Class-sensitive sprites and supplied inventory remain relevant after fresh healing. |
| F88 set with inconsistent current/saved state | Use that current state without fresh repair, including applicable neutral rewrites. |
| Unfinished wrapper/load service | Only completed prefix writes are established; no completed presentation/control claim. |

## Evidence Review and Remaining Limits

**Confirmed provenance:** accepted main `5ef473acf19a3c47cbccc71078e0e3bbfc65f047` contains
[#908/PR910](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/910) and the
[#907/PR909 predecessor acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/909#issuecomment-6174944499).
The research owner pins [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This synthesis reviews accepted joins without source reacquisition, extraction or runtime.

Pre-synthesis adversarial review checked the complete Battle22 owner, predecessor, lifecycle
contract and the following bounded executable surfaces, including fixture IDs, provenance,
schemas and verifier selection. The narrow commands are reproduction owners, not new runs:

| Fixture / command owner | Reviewed boundary |
| --- | --- |
| [sf2-battle-loop-static-v1](../../../tests/fixtures/h2/battle-loop-static-v1.json), `uv run sf2 h2 battle-loop` | Roster sizes, eligible healing and AI-memory reset; the lifecycle contract and accepted Battle22 direct join provide placement/dead/surplus details. |
| [sf2-battle-control-static-v1](../../../tests/fixtures/h2/battle-control-static-v1.json), `uv run sf2 h2 battle-control` | Fresh/suspended controller facts and subsection units; accepted selected control trace provides exact assignments, bypasses and stopping labels. |
| [sf2-battle-cutscenes-static-v1](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), `uv run sf2 h2 battle-cutscenes` | Shared intro check/write order; complete slot22 tables, terminator and actual returns belong to the accepted direct join. |
| [sf2-battle-routing-data-static-v1](../../../tests/fixtures/h2/battle-routing-data-static-v1.json), `uv run sf2 h2 battle-routing-data` | Aggregate topology has routeTargetsParsed=false; it does not independently prove the two slot22 targets. |
| [sf2-battle-spriteset-data-static-v1](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json), `uv run sf2 h2 battle-spriteset-data` | Header/macro/address inventory has rowContentTracked=false; the accepted twelve/sixteen records, point, definitions and load/mapping join supply exact local semantics. |

The corresponding fixture schemas are
[loop](../../../schemas/h2-battle-loop-static-fixture.schema.json),
[control](../../../schemas/h2-battle-control-static-fixture.schema.json),
[cutscenes](../../../schemas/h2-battle-cutscenes-static-fixture.schema.json),
[routing](../../../schemas/h2-battle-routing-data-static-fixture.schema.json) and
[spritesets](../../../schemas/h2-battle-spriteset-data-static-fixture.schema.json).
Their existing verifiers are
[battle_loop.py](../../../src/sf2tool/h2/battle_loop.py),
[battle_control.py](../../../src/sf2tool/h2/battle_control.py),
[battle_cutscenes.py](../../../src/sf2tool/h2/battle_cutscenes.py),
[battle_routing_data.py](../../../src/sf2tool/h2/battle_routing_data.py) and
[battle_spriteset_data.py](../../../src/sf2tool/h2/battle_spriteset_data.py).
Equality of their aggregates cannot guard every novel roster/load operand or prove natural
reach. The accepted owner's [reproduction boundary](../../research/battle22-initialization-and-roster.md#reproduction-retained-evidence-and-limits)
records complete macro/constants, initialization, item, neutral, actor and map-load consumer joins.
No conflicting accepted semantic owner was found. Older battle-loop/spriteset “runtime queue”
wording remains a deferred ambiguity register under ADR0014/0016, rather than an acquisition
instruction or a reason to downgrade Battle22's accepted static joins.

Preserve [#908's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/908#issuecomment-6174872245):
head `0a7048f0fbfa38a69d6470a73e40445cebc55acd`, tree `eb68c08e958a868dd163f9e128edffffa5147f14`,
base `ccf4a57360fea5b62160fe959a5d2dbca6dd8736`,
[CI38610992302](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38610992302).
Its [pure-rebase handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/908#issuecomment-6174994479)
retains head `9b054f217a07b1ab8926dcf90f30ac5fd90f8668`, tree `6da75413c07be7587231e10e6659f3dde40c7eca`,
base `2c13623a3720199d2e92b13f9b353bcf054cc756`,
[CI38612004333](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38612004333).
Both CI runs succeeded in scope/research-public with engine/adapter/H4 scoped skips; the
[independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/910#issuecomment-6175006823)
applies to the unchanged rebased document. The retained map-init/map-script pair is unchanged:
18,285,979 bytes, compared in 0.084s. No comparison is rerun or regenerated for this guide.

Those handoffs preserve bounded-read/path-search failures, the neutral state versus B/A lookup
correction, full-load d1 versus view d0 clarification and CRLF checks. Their linked #904/#903/
#900/#899 receipts retain distinct heads, CI and completed failures/corrections. The
[predecessor evidence section](desktop-kingdom-interactions-through-battle22-entry.md#evidence-and-review-boundary)
keeps those links, including corrected csc2D FLAGS_A5/6 destination-scan bypass, reloadMap's
working-layout reuse and RightLeft facing. [#907 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/909#issuecomment-6174944499)
retains CI38610983007 scope-only success. These results are not replaced by this synthesis.

Direct acceptance checks complete owned prose/diff, all sixteen enemy rows and placement/flag/
item/mapping/coordinate contrasts, provenance, links/fragments, scope, English/private boundary
and whitespace. Use the configured clean committed dependency planner and actual final-head CI.
No local normal/full/engine/Godot/H3/H4 or translation suite is selected solely for prose.

**Unknown:** natural approach/admission; supplied flags, membership, HP/class/items/current/saved
state; actual live roster, lookups and view; completed load/fade/VInt/shared script-return services;
rendered/heard performance, usable control and cross-process persistence. The changed-state
examples are static counterexamples, not executed scenarios or an automatic runtime queue under
[ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) and
[ADR0016](../../decisions/0016-remake-start-evidence-deferral.md). The separate outcome guide
consumes accepted #912; this initialization guide retains its own stop.
This guide completes neither the campaign nor
[epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
