# Battle22 initialization and initial roster

Evidence date: 2026-10-14. **Confirmed:** admitted fresh Chessboard/Battle22
selects empty before/start scripts, but still heals the eligible party,
initializes twelve placement slots and sixteen enemy records, and loads the
battle. F522-clear loading also declares Ponpei and the Maid as two neutral
entities. F422 admission, F472 intro, F522 completion and F88 suspension have
independent consumers. **Inferred:** the chess-piece names and formation stage
the King's challenge. **Unknown:** natural admission, supplied state, service
completion, actual live roster and presentation. Empty hooks do not establish
that initialization was skipped or that an encounter was played.

## Agreement and provenance

Standard rigor under [Issue #908](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/908)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
The uncertainty is the complete selected data-to-consumer join, including
saved-state contrast; the result supports the next story/combat content guide.
Accepted starting base is `ccf4a57360fea5b62160fe959a5d2dbca6dd8736`, without
stacking. This is the sole owned document. No shared index, registry, design,
contract, schema, fixture, tooling or remake change is needed.

Begin at [Desktop conversations through Battle22 admission](./desktop-kingdom-interactions-through-battle22-entry.md).
Reuse [Battle21's accepted entry consumers](./battle21-opening-and-initial-roster.md),
[battle-loop research](./battle-loop.md), the
[control/lifecycle contract](../design/contracts/battle-control-lifecycle.md),
[cutscene routing](./battle-cutscenes.md) and
[spriteset inventory](./battle-spriteset-data.md). The [first-control continuation](./battle22-initialization-and-first-control.md)
owns subsequent SWARM, orders and dispatch. Fresh entry here stops at
`BattleLoop:@Start`, **before ActivateEnemies**. Suspended entry stops before
`@IndividualTurns_Loop`. Exclude tactical/AI execution, damage, outcomes,
leader-death/after scenes, terrain decoding, a whole renderer audit, postbattle
Desktop, Cotton Balloon and Floor World. No runtime acquisition is admitted.

Paths below are relative to `disasm/` in
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The assigned read-only source checkout is clean at that pin. Retained evidence
binds USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Fnnn means flag nnn; all table/row indices are zero-based. Coordinates are
source operands, not observed positions. No new ROM extraction or playthrough
is represented by this document.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine pair and small
existing fixtures/source selections. Linear comparisons are budgeted below
512 MiB and one minute, with only small receipts/prose written locally. The
pair and shared cutscene-source comparison passed in 0.084 seconds. No new
payload, environment or capture; reassess before material expansion.

## Admission and exact fresh lifecycle

**Confirmed:** the predecessor owns the F777/F522 zone, `cs_5994E`, F422 write
and surviving Map22 `(0,0,Right)` warp. Reaching this boundary is conditional;
those conversations are not replayed by the battle hooks.
`code/common/maps/getbattle.asm:CheckBattle` and complete
`data/battles/global/battlemapcoords.asm` select sole Map22 row 22:
`22,9,0,30,30,255,255`. F422 must be set; both trigger coordinates are
wildcards, with no area-containment test. Selection writes area origin `(9,0)`
and size 30x30. F522 set then clears F422 but still returns 22 for this call;
F522 clear retains it. F472 and F88 do not gate this scan.

`code/gameflow/battle/battleloop_1.asm:BattleLoop` first clears PLAYER_TYPE
and sets F399, then tests F88. On F88-clear entry with incoming Map22/Battle22:

1. Clear SECONDS_COUNTER. Save incoming d0/d1 words, assign CURRENT_MAP=22
   and CURRENT_BATTLE=22, install base VInts and execute the before wrapper.
2. After its return restore the incoming words and reassign current map/battle.
   Clear region flags 90–105, even though this spriteset has zero regions.
3. Call HealLivingAndImmortalAllies, InitializeAllAlliesBattlePositions,
   InitializeAllEnemiesBattlePositions, then ClearAiMemory, in that order.
4. Clear d0 to zero, call LoadBattle, then execute the start wrapper.
5. Reach `@Start`; this document stops before its ActivateEnemies call.

This preserves the corrected shared lifecycle order: fresh healing and roster
initialization follow the before wrapper. The input map/battle overwrite is
explicit; neither earlier exploration actor positions nor a previously current
battle selects the fresh roster after that assignment.

### Empty does not remove wrapper effects

**Confirmed:** complete pointer tables
`data/battles/cutscenes/beforebattlecutscenes.asm` and
`battlestartcutscenes.asm` each contain 48 relative-word slots. Both slot22
entries select `ms_Empty`, whose complete body is the single `$FFFF` word at
`0x47A4E..0x47A50` in `code/common/scripting/map/ms_empty.asm`.
This is positive table/body evidence, not an inference from a missing directory.

The four actual wrapper parts in `code/gameflow/battle/cutscenes/` are
`beforebattlecutscenesstart.asm`, `beforebattlecutscenesend.asm`,
`battlestartcutscenesstart.asm`, and `battlestartcutscenesend.asm`:

| Wrapper | F472 clear | F472 set | Actual return |
| --- | --- | --- | --- |
| ExecuteBeforeBattleCutscene | Add CURRENT_BATTLE to intro base450; check; save d0/a0, select relative word at index22*2, ExecuteMapScript; no intro-flag write | Branch around script dispatch | Restore d0/a0 on dispatched path, then d1 at loc_47AE8, rts |
| ExecuteBattleStartCutscene | Same check; **set F472 before dispatch**, then save d0/a0 and execute selected empty script | Branch around flag write and script dispatch | Restore d0/a0 on dispatched path, then d1 at loc_47B8C, rts |

`mapscriptengine_2.asm:ExecuteMapScript` still initializes its work pointer,
saves registers, clears the text-skip byte and reads the terminator. Its
`loc_47234` tail waits for view scrolling if DIALOGUE_WINDOW_INDEX is nonzero,
clears VIEW_SCROLLING_SPEED, restores registers and returns. Thus the empty
hook has no local dialogue, choice, actor deployment, recruitment or map-load
command, but is not literally a bare wrapper rts. A stalled shared wait can
prevent return. F472 is already set if the start terminator stalls; it is not
written by the before wrapper. Supplied F472 set skips both scripts while
fresh region/heal/roster/load work remains. F522 does not replace this intro test.

## Complete spriteset and party placement

**Confirmed:** `data/battles/spritesets/entries.asm` has 45 longword pointers;
slot22 selects `BattleSpriteset22` in `spriteset22.asm`,
`0x1B4AA4..0x1B4BFA`. Header bytes are **12 allies, 16 enemies, 0 regions,
1 point**. `code/gameflow/battle/getbattlespritesetsubsection.asm` selects by
CURRENT_BATTLE*4; subsections use the actual preceding counts, with twelve-byte
combatant and region strides. The four-byte header, 28 combatant records and
two-byte point occupy 342 bytes. Subsection offsets from the start are ally 4,
enemy 148, region 340 and point 340; zero regions means no intervening region data.

`sf2macros.asm:allyCombatant/enemyCombatant`, `combatantAiAndItem` and
`combatantBehavior`, with `sf2enums.asm`, encode each combatant as:

| Offset | Width | Source field |
| --- | --- | --- |
| 0 | byte | Ally placement index or ENEMY_ type |
| 1,2 | byte each | Starting X,Y |
| 3 | byte | AICOMMANDSET_ value |
| 4 | word | ITEM_ bitfield |
| 6,7 | byte each | Primary AIORDER_ bitfield and trigger region |
| 8,9 | byte each | Secondary order and trigger region |
| 10,11 | byte each | Filler and SPAWN_ initialization type; consumer reads them as a word |

`defineShorthand` resolves the prefixed constant or numeric value;
`defineBitfield` ORs the prefixed/numeric terms, then emits the requested width.
Every ally row carries HEALER1=0, NOTHING=`$007F`, primary NONE/15,
secondary NONE/15, filler 0 and STARTING=0. Complete positions are:

| Placement row | X/Y | Placement row | X/Y |
| --- | --- | --- | --- |
| 0 | `(18,18)` | 6 | `(23,18)` |
| 1 | `(21,18)` | 7 | `(22,17)` |
| 2 | `(15,18)` | 8 | `(14,17)` |
| 3 | `(20,17)` | 9 | `(18,17)` |
| 4 | `(16,17)` | 10 | `(19,18)` |
| 5 | `(13,18)` | 11 | `(17,18)` |

These numeric row indices do not impose named allies. The immediate owner is
`code/gameflow/battle/battleloop/initializecombatants.asm`:
InitializeAllAlliesBattlePositions first sets all thirty ally X values to -1,
then `j_UpdateForce` rebuilds the list from joined and active flags in ascending
ally ID order (`code/common/stats/battleparty.asm`). An arbitrary preexisting
list order is overwritten. Each living entry takes the next placement X/Y;
the placement's type/index, item and AI fields are not copied into that ally.

Healing has already run: `battleloop/heallivingandimmortalallies.asm` restores
HP/MP maxima for living allies and always for Peter7/Lemon28, retains only
existing STUN/POISON/CURSE bits as an intermediate mask, then updates stats.
`code/common/stats/updatecombatantstats.asm:UpdateCombatantStats` removes incoming
CURSE and derives final ally CURSE from equipped cursed gear; stale CURSE with
uncursed gear is cleared, and cursed equipment can add it. STUN/POISON remain.
Other dead allies remain dead. It neither joins members nor changes their classes/inventories.
A dead listed member consumes one of the twelve party-loop slots without
advancing the placement record. Later living members therefore use earlier
unconsumed positions, not their numeric ally-ID row. Any members beyond twelve
processed slots have their active flags cleared and X=-1 by LeaveBattleParty;
that helper does not immediately rebuild the list/count. Fewer living active
members yield fewer placed allies. An empty list is not guarded before its
first entry read, so arbitrary empty/inconsistent state is not promised a safe
empty deployment. Reserves and exploration followers are not automatically added.

## Sixteen enemy records and immediate decoding

**Confirmed:** all sixteen rows have item NOTHING=`$007F`, primary region15,
secondary region15, filler 0 and STARTING=0. NONE=`$FF`, MOVE_TO=`$40`,
FOLLOW_ENEMY=`$80`; numeric suffixes are OR operands. Commandset values are
LEADER=14, ATTACKER3=9, HEALER3=2, HEALER4=3, SWARM=15 and ATTACKER2=7.
The full differing fields, including the only non-NONE secondary order, are:

| Row / combatant | Enemy type (ID) | X/Y | Commandset | Primary order (byte) | Secondary order (byte) |
| --- | --- | --- | --- | --- | --- |
| 0 /128 | KING (89) | `(15,5)` | LEADER | MOVE_TO\|0 (`$40`) | NONE (`$FF`) |
| 1 /129 | QUEEN (71) | `(17,5)` | ATTACKER3 | FOLLOW_ENEMY\|0 (`$80`) | NONE (`$FF`) |
| 2 /130 | BISHOP (80) | `(13,5)` | HEALER3 | FOLLOW_ENEMY\|0 (`$80`) | NONE (`$FF`) |
| 3 /131 | BISHOP (80) | `(19,5)` | HEALER4 | FOLLOW_ENEMY\|9 (`$89`) | FOLLOW_ENEMY\|0 (`$80`) |
| 4 /132 | KNIGHT (14) | `(11,5)` | SWARM | NONE (`$FF`) | NONE (`$FF`) |
| 5 /133 | KNIGHT (14) | `(21,5)` | SWARM | FOLLOW_ENEMY\|8 (`$88`) | NONE (`$FF`) |
| 6 /134 | ROOK (61) | `(9,5)` | ATTACKER2 | NONE (`$FF`) | NONE (`$FF`) |
| 7 /135 | ROOK (61) | `(23,5)` | SWARM | FOLLOW_ENEMY\|4 (`$84`) | NONE (`$FF`) |
| 8 /136 | QUEEN (71) | `(15,6)` | SWARM | NONE (`$FF`) | NONE (`$FF`) |
| 9 /137 | QUEEN (71) | `(17,6)` | SWARM | NONE (`$FF`) | NONE (`$FF`) |
| 10 /138 | PAWN (13) | `(13,6)` | SWARM | FOLLOW_ENEMY\|9 (`$89`) | NONE (`$FF`) |
| 11 /139 | PAWN (13) | `(19,6)` | SWARM | FOLLOW_ENEMY\|4 (`$84`) | NONE (`$FF`) |
| 12 /140 | PAWN (13) | `(11,6)` | ATTACKER2 | FOLLOW_ENEMY\|6 (`$86`) | NONE (`$FF`) |
| 13 /141 | PAWN (13) | `(21,6)` | SWARM | FOLLOW_ENEMY\|8 (`$88`) | NONE (`$FF`) |
| 14 /142 | PAWN (13) | `(9,6)` | SWARM | FOLLOW_ENEMY\|8 (`$88`) | NONE (`$FF`) |
| 15 /143 | PAWN (13) | `(23,6)` | ATTACKER2 | FOLLOW_ENEMY\|6 (`$86`) | NONE (`$FF`) |

There are no region records. The sole point0 is exactly `(15,5)` after the last
enemy record. MOVE_TO|0 and that point are stored inputs; the initialization
boundary does not execute a move, follow, heal, spell or tactical strategy.
HEALER/LEADER/SWARM names are commandset labels, not observed actions or victory
conditions. Region15 is a stored nibble, not evidence for a fifteenth polygon.

InitializeAllEnemiesBattlePositions visits combatants128–159. For rows0–15,
InitializeEnemyBattlePosition reads the initialization word's low nibble;
STARTING0 is below HIDDEN2 and invokes InitializeEnemyStats. Out-of-range
slots144–159 take the absent path: activation0, max/current HP0 and X=-1.
That path still reads trigger bytes through the subsection pointer; it does
not zero every combatant field. Load-time neutrals below subsequently repurpose
158/159 when F522 is clear.

InitializeEnemyStats resolves the type, copies its 56-byte enemy definition
(14 longwords), fills current HP/MP from maxima, sets row X/Y, combines the
definition movetype in the upper nibble with the masked commandset in the lower,
and stores enemy type, triggers, orders and inventory effects. The actual
`combatantstats_2.asm` setters pack trigger regions as primary<<4 | secondary
(`$FF` here) and move orders as primary<<8 | secondary (for row3, `$8980`).
The initial activation write preserves only the copied definition's `$F000`,
rotates the row's initialization word by eight and masks it with `$0FFF`,
then ORs them. Here the zero filler/STARTING word contributes zero: the write
is `$1000` for King, `$3000` for Bishops and `$2000` for the other pieces.
Definition low bits such as BIT4/BIT5 are not carried through that write;
primary/secondary-active and neutral bits are clear. Shared difficulty
adjustment and UpdateCombatantStats follow. This is initialization, before
ActivateEnemies; no activated enemy or executed tactic is claimed.

`battleloop/upgradeenemies.asm:UpgradeRandomBattleEnemies` returns the original
type when IsBattleUpgradable is false. Complete eleven-entry
`data/battles/global/randombattles.asm:list_RandomBattles` excludes CHESSBOARD22.
Thus F522 set does not upgrade these types; Battle21's F521/random-upgrade
case must not be copied here. The Battle32/Jaro special case also does not apply.

### Row NOTHING versus definition inventory

**Confirmed:** InitializeEnemyItems compares the complete word to ITEM_NOTHING
before calling AddItem; all sixteen exact `$007F` words return immediately.
NOTHING is not a request to erase the copied definition's inventory. The
selected `data/stats/enemies/enemydefs.asm` entries supply:

| Type | Copied HP/MP maxima | Equipped first item; remaining three slots | Movetype |
| --- | --- | --- | --- |
| King89 | 55/28 | FLAIL; NOTHING, NOTHING, NOTHING | MAGE |
| Queen71 | 30/26 | POWER_STICK; NOTHING, NOTHING, NOTHING | MAGE |
| Bishop80 | 33/36 | POWER_STICK; NOTHING, NOTHING, NOTHING | HEALER |
| Knight14 | 48/0 | POWER_SPEAR; NOTHING, NOTHING, NOTHING | REGULAR |
| Rook61 | 40/0 | ROBIN_ARROW; NOTHING, NOTHING, NOTHING | BRASS_GUNNER |
| Pawn13 | 36/0 | BATTLE_AXE; NOTHING, NOTHING, NOTHING | REGULAR |

The source comment calls entry80 EVIL BISHOP; the selected ENEMY_BISHOP enum
is 80 and its map-sprite table entry is BISHOP. Do not substitute ENEMY_EVIL_BISHOP
or a different type because of that comment. Copied definitions include their
spell fields; this boundary does not execute them or derive damage/rewards.

For the contrasting non-sentinel path, `j_AddItem` resolves through
`s02_jumpinterface.asm` to `code/common/stats/itemstats.asm:AddItem`. It searches
four slots using the index mask 127; successful insertion masks d1 itself with
`$807F` (index plus broken bit). That removes equipped bit7 and other high
item flags before InitializeEnemyItems tests the equipped bit. Full inventory
returns failure without that successful mask/write, and the caller does not
branch on the result. This accepted shared decoder is not invoked by Battle22's
sentinels. Neither the row NOTHING nor a symbolic item flag proves a later drop,
reward, use, or replacement of supplied ally equipment.

## LoadBattle, coordinate domains and new actor mappings

**Confirmed:** `code/gameflow/battle/battlefunctions/loadBattle.asm:LoadBattle`
starts with the current map in d1, fades out, disables view tracking, loads
that map's tilesets, waits for fading, clears VInts and calls
`code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities`.
That routine clears old actor data/lookups, scans all thirty allies then all
32 enemy slots, and declares actors only for nonnegative X/Y; its enemy pass
also excludes neutral-marked slots. It does not use an additional HP test.
For this fresh roster, initialization supplied the HP/position eligibility.
Saved state can differ.

Ordinary actors use `(X & $3F)*384`, `(Y & $3F)*384`, Down=3 and eas_Standing.
GetCombatantMapsprite uses GetAllyMapsprite for class-sensitive allies, or the
stored enemy type through `data/stats/enemies/enemymapsprites.asm`. King/Queen/
Bishop/Knight/Rook/Pawn map sprites are 153/135/144/78/125/77 respectively, all
below the special-sprite threshold 240. Thus these enemies follow the ordinary
sequential physical allocation. With N successfully declared allies, enemy
row r maps logical combatant128+r to physical N+r; skipped ally lookups are -1.
With twelve placed allies, the sixteen enemies are physical12–27.

The exploration chess NPCs137–152, NPC128 Caravan and old ORB153–155 are not
carried over. For example combatant128 is now King, while combatant137 is
Queen row9. Type89, map sprite153, logical128 and physical N are distinct IDs.
The cleared/rebuilt battle mapping is not a continuation of exploration's
physical allocation, actions, follower aliases or Right-facing warp.

### Completion-gated neutral population

**Confirmed:** the complete `data/battles/global/battleneutralentities.asm`
contains this CHESSBOARD group, terminated after two rows:

| Order | Combatant state slot / script lookup | Position | Facing / sprite / action |
| --- | --- | --- | --- |
| 0 | 159 / B31 | `(6,2)` | Down / KING_PONPEI188 / eas_Standing |
| 1 | 158 / A30 | `(5,3)` | Down / MAID208 / eas_Standing |

After the ordinary scans PositionBattleEntities tests F500+CURRENT_BATTLE,
hence F522. Clear admits the neutral-table scan; set skips it. Each matched
row sets that descending slot's max/current HP0 and status0, ORs neutral mask `$0008` (bit3)
into activation, writes row X/Y, then declares the next physical actor and
writes its lookup by predecrementing from ENTITY_INDEX_LIST_ENEMIES.
With the fresh absent-slot initialization above, these slots' activation
becomes `$0008`. With N placed allies and all sixteen ordinary enemies,
Ponpei/Maid occupy physical N+16/N+17 (28/29 when N=12).

The lookup distinction is literal: `sf2const.asm` places ENTITY_INDEX_LIST
at `$FFB140` and ENTITY_INDEX_LIST_ENEMIES at `$FFB160`. Predecrement writes
`$FFB15F` then `$FFB15E`, offsets 31/B and 30/A, **not** enemy offsets63/62.
The ordinary enemy lookups for 159/158 remain -1 from the skipped enemy pass.
GetEntityIndexForCombatant(159/158) therefore does not recover these neutral
actors; the script B/A lookup does. Do not conflate modified combatant state
with a matching combatant-to-actor mapping.

They are additional neutral actors, not additional enemy-row data
or additional allies, and not the old exploration NPC134/133 mapping.
F522 set leaves fresh 158/159 absent. No Caravan, ORB or other exploration
resident is added by this selected group. On resume, this same neutral loader
can rewrite those slots again; it is not a guarantee to preserve every saved
field unchanged.

### Full map load and battle-local coordinates

LoadBattle restores its supplied combatant d0 (fresh0), resolves the new lookup
with GetEntityIndexForCombatant, sets VIEW_TARGET_ENTITY and reads its tile
coordinates for BATTLE_ENTITY_CHOSEN_X/Y. A negative lookup falls back to
physical0 for those reads, without repairing membership or positioning Bowie.
It initializes sprites, sets **d0=-1**, and calls `code/common/maps/mapload.asm:LoadMap`.
The negative d0 selects target-based view placement. It is **d1** that selects
new map versus reload; this path retains the nonnegative CURRENT_MAP value
through the intervening helpers, so Map22 uses the full-load arm. Do not confuse
it with the predecessor's csc46 reloadMap negative-d1 reuse of the working layout.

LoadMapBlocksAndLayout loads the base layout and applies flag copies/chest
overlays, then, because CURRENT_BATTLE!=255, CopyMapBlocks copies area `(9,0)`
size 30x30 to working origin `(0,0)`. The copy uses 64-wide word rows.
LoadMapArea's battle arm sets layer1 start `(0,0)` and end `(29,29)*384`;
other layer values remain selected map-area operands. No terrain/renderer
result is inferred from this geometry.

Roster, point and neutral coordinates are already battle-local: no initialization
helper subtracts 9 from their X values. Their source-layout correspondence is
`(localX+9,localY)` after the crop. King `(15,5)` corresponds to full Map22
`(24,5)`; placement0 `(18,18)` corresponds to `(27,18)`; neutral Ponpei `(6,2)`
to `(15,2)`. This relation is not a warp or proof of traversal from `(0,0)`.
Changed supplied area state changes the crop; LoadBattle does not rerun CheckBattle.

LoadBattle then loads entity mapsprites, installs base VInts, invokes the
existing battle-terrain loader, submits map music and fades in. Battle22 does
not take the Battle44 timer-window branch. Its return permits the start wrapper,
whose return permits the declared fresh stop. Required fade, VInt, display,
map/sprite and other services must return; their call sequence does not prove
visible performance, exact timing or usable input. Earlier writes are not
rolled back by a later incomplete service.

## Supplied F88 continuation and counterexamples

**Confirmed:** after mode clear/F399, F88 set restores SAVED_SECONDS_COUNTER
into SECONDS_COUNTER, clears F88, clears AI memory, clears VIEW_TARGET_ENTITY,
calls LoadBattle, then branches directly to `@IndividualTurns_Loop`.
The following branch to `@Start` is unreachable. Both fresh and suspended
ClearAiMemory calls reset 48 last-target bytes to `$FF` and 48 memory bytes to 0;
this does not mean a fresh AI/turn simulation has run.

The suspended arm does not assign incoming map/battle, clear region flags,
heal, initialize allies/enemies, or execute either intro wrapper. It does not
explicitly clear d0 to 0 before LoadBattle. Current map/battle, battle area,
combatants, current turn/order, flags and the supplied view-target argument
must be distinguished from the incoming admission pair. LoadBattle rebuilds
entities from the existing positions, applies its current completion/neutral
selection and reloads the current map/terrain; it does not repair that state
from spriteset22. Saved HP, items, statuses and positions are therefore not
asserted to equal the fresh table. An independently changed CURRENT_BATTLE can
select other load data even when the incoming argument was 22. This is the
bounded branch, not evidence of cross-process save restoration or persistence.

| Changed supplied premise | Consequence within this boundary |
| --- | --- |
| F422 clear, Map22 | No row22 CheckBattle admission, regardless of intro/completion/suspension bits |
| F422 set, F472 clear, F522 clear, F88 clear | Empty before; fresh healing/roster; two neutral actors during load; start writes F472 before its empty dispatch |
| F472 set independently | Both scripts skipped; fresh initialization/load still runs; no previous played intro is proven |
| F522 set independently with F422 set | Selection clears F422 but admits this call; chess types do not upgrade; neutral loader skips; F472 still independently gates scripts |
| Changed active membership or supplied list order | UpdateForce rebuilds ascending joined/active IDs; first twelve processed members bound placement, not named row IDs or stale ordering |
| Dead non-immortal members, fewer or surplus members | Dead slots consume loop capacity without consuming positions; no fixed twelve-living-ally assertion; surplus active flags cleared |
| Changed ally class/item/HP | Current class-sensitive sprites/inventory remain relevant; fresh healing changes eligible HP/MP/status, not class/membership/item provenance |
| Different prior map/battle, F88 clear | Incoming22/22 is assigned before the hook and again afterward; fresh load uses that state |
| F88 set, inconsistent saved/current map/area/turn/roster | Fresh assignment/hooks/heal/roster/region reset bypassed; LoadBattle uses actual current state, including neutral effects; no automatic repair |
| Incomplete before/start termination or load service | Only completed prefix writes hold; F472 may already be set during unfinished start; no successful visible encounter or control guarantee |

## Reproduction, retained evidence and limits

Load `. ./local/private-inputs.ps1` in the same process as controlled tools and
verify the source pin/clean status. The existing
[retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
compares map-init equality, `_verify_map_script_engine_fixture` and
`_cutscene_facts` without regenerating the pair. Battle22 has no extra selected
before/start command corpus beyond the directly read ms_Empty terminator.

| Existing owner / direct join | What is checked, and its limit |
| --- | --- |
| `battle-cutscenes-static-v1.json`, actual complete before/start tables and wrappers | Shared source facts plus both slot22 targets, flag polarity/write order and actual end-label returns |
| `battle-routing-data-static-v1.json` | Existing aggregate topology; its routeTargetsParsed=false is not proof of individual slot22 targets |
| `battle-spriteset-data-static-v1.json` and retained spriteset output | Existing pointer/address/header inventory; rowContentTracked=false means all 28 complete records, macros/constants and point0 require the direct source join above |
| `battle-loop-static-v1.json`, `battle-control-static-v1.json`, accepted lifecycle owners | Existing source facts for roster/healing/AI reset and fresh/resumed control; selected complete entry/initialization bodies checked directly |
| `battleparty.asm`, enemydefs/enemymapsprites, itemstats and combatantstats setters | Actual membership order/capacity, six definition identities, sentinel/masks and packed writes, not symbolic AI execution or rewards |
| LoadBattle, PositionBattleEntities, GetCombatantMapsprite, GetEntityIndexForCombatant, map load/copy/area helpers | Full-load d1 versus view d0, rebuilt mapping, complete CHESSBOARD neutral group and coordinate relation |
| Complete random/neutral tables, pointer tables and relevant jump interfaces | Exclusion/inclusion established by full scans; `j_` calls resolved to the named implementation in s02/s07/s13 |

For direct reproduction read the named files/functions completely within the
selected boundary, enumerate the twelve ally and sixteen enemy triples of
macros in order, resolve constants from sf2enums, and compare source/header/
point/address facts to the retained and public shapes. Fixture aggregate
equality alone does not guard every new semantic claim. No new parser, fixture,
schema, verification program or runtime question acquisition is introduced.

Direct whole-document/source/provenance/link/private checks and `git diff --check`
apply. Run configured `uv run sf2 verify plan --base origin/main --head HEAD`
on the clean committed candidate; record actual final-head CI on Issue908.
No local blanket normal/full/engine/Godot/H3/H4 suite is selected solely for prose.

Preserve [#904 original](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/904#issuecomment-6174520666),
[rebased/final clarification](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/904#issuecomment-6174585157)
and [PR906 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/906#issuecomment-6174618486),
including distinct original/rebased/final identities, CI38608369060/38608730439/
38608904864 and unchanged 18,285,979-byte pair/0.080s. Also retain
[#903 original](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/903#issuecomment-6174446531),
[correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/903#issuecomment-6174502720)
and [PR905 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/905#issuecomment-6174512539).
Their linked #900/#899 failures and corrected results remain evidence:
csc2D clears FLAGS_A bits5/6 and bypasses competing-destination scanning,
reloadMap reuses the working layout, and RightLeft changes facing. These are
not replaced by an imagined fresh scene or rerun because a design base advances.

**Unknown:** natural approach/admission; supplied flags, membership, HP/class/
items/current/saved state; actual live roster and lookup/view; completion of
load, fade, VInt and shared script-return services; visible/heard performance,
usable control and cross-process persistence. Static counterexamples are not
executed scenarios or an automatic runtime queue under
[ADR0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../decisions/0016-remake-start-evidence-deferral.md).
