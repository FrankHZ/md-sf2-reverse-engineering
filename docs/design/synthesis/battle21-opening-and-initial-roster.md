# Battle21 Opening and Initial Roster

At the admitted Devil's Tail encounter, a Skeleton tells the newcomers to leave Mr. Creed's
territory. Peter asks to pass; the Skeleton refuses, then calls for an attack as three more scene
actors are revealed. The subsequent battle setup replaces this cast with the supplied force and
selected enemy records. Neither the arrivals from Fairy Cave nor the confrontation's actor list
chooses the player's combat party.

**Confirmed:** accepted static research establishes the selected scene, independent flag gates,
coordinate domains and conditional initialization below. **Inferred:** the exchange stages a refusal
followed by a threat of attack. **Unknown:** natural reach, supplied state, completed services, actual
initial roster, presentation and usable control. This is content and setup explanation, not a played
encounter, tactical recommendation or claim that the threat executes successfully.

This guide continues [Fairy Cave exploration and Devil's Tail return](fairy-cave-exploration-and-devils-tail-return.md)
and consumes [accepted Battle21 research](../../research/battle21-opening-and-initial-roster.md).
Fresh entry stops at `BattleLoop:@Start` **before ActivateEnemies**; suspended entry stops before
`@IndividualTurns_Loop`. Combat execution, outcomes, Creed entry/interior and terrain reconstruction
remain outside this guide. Fnnn means flag nnn; rows are zero-based. Coordinates, waits and speeds
are source operands rather than observed positions or measured durations.

## Admission Does Not Decide Whether the Scene Runs

**Confirmed:** the predecessor owns both cave exits, the separate default-zone random request and
the packed reload's retained/replaced pending state. After a consumed request, CheckBattle selects
the sole Map67 row: battle 21, source area `(21,0)`, size 32x42, trigger `(255,255)`. Supplied F421
admits it without requiring arrival inside the rectangle. F521 set clears F421 after selection and
still returns battle 21; F521 clear leaves F421 set. Neither arrival nor F471 repairs an absent F421.

The relevant flags answer different questions:

| Supplied state | Consequence at this boundary |
| --- | --- |
| F421 clear | No row 21 admission through CheckBattle, whatever F471/F521 say. |
| F421 set | Admit battle 21; F521 controls whether admission then clears F421. |
| F88 clear | Fresh BattleLoop entry assigns the current map/battle, runs gated before/start seams and initializes combatants. |
| F88 set | Load supplied saved battle state and resume before individual turns; do not rerun the fresh scene or roster initialization. |
| F471 clear on fresh entry | Before wrapper selects bbcs_21; later start wrapper sets F471 before dispatching the empty start program. |
| F471 set on fresh entry | Skip both selected scripts, but retain fresh healing, initialization and battle loading. |
| F521 set during fresh enemy initialization | Admit the conditional random-upgrade path independently of F421/F471. |

BattleLoop first clears PLAYER_TYPE to walking mode 0 and sets F399, before the F88 test.
On the fresh path it clears seconds, saves the incoming map/battle words, assigns CURRENT_MAP=67
and CURRENT_BATTLE=21, installs base VInts and executes the before wrapper. After its return it
restores the incoming words and rewrites current map/battle, clears region flags 90–105, heals eligible
allies, initializes allies then enemies, clears AI memory, sets d0=0 and calls LoadBattle. Only then
does it execute the start wrapper and reach the fresh stop.

Slot 21 of the before table selects `bbcs_21` at `0x4B790`; slot 21 of the start table selects
`ms_Empty` at `0x47A4E`, whose complete body is `$FFFF`. Empty still enters and terminates
ExecuteMapScript, with shared services; no Battle21-specific start file is implied. Both wrappers
test F450+21=F471. Before never sets it, and bbcs_21 contains no flag write, recruitment, request or
choice. A partial before scene therefore does not establish a completed intro flag transition.

F88-set entry restores saved seconds, clears F88 and AI memory, clears the view target, calls
LoadBattle and branches directly to the individual-turn loop. Fresh assignment, before/start,
healing and placements are bypassed; d0 is not explicitly cleared there. The supplied current map,
battle, area, roster and continuation must already agree. An incoming battle 21 value does not repair
inconsistent saved state, and the fresh tables below are not a suspended-roster prediction.

## A Cropped Battlefield with Literal Scene Coordinates

**Confirmed:** source area `(21,0)` and working-layout origin `(0,0)` describe two different domains.
While CURRENT_BATTLE is 21, the full Map67 loader applies base layout, flag copies and chest overlays,
then CopyMapBlocks copies the 32x42 source rectangle to working origin 0,0. The working layout has
64-word row stride. LoadMapArea starts layer 1 at 0,0 and ends it at `(31,41)*384`; other layer
parameters retain their selected map-area values. This establishes a crop, not terrain or collision.

| Operand domain | How the accepted consumer uses it |
| --- | --- |
| Fairy Cave exits `(51,23)` / `(57,24)` | Arrival/trigger inputs, not combatant placements. Wildcards can admit even `(57,24)`, outside the source rectangle's X=21..52 span. |
| Scene load camera `(5,18)` | Packed coordinates become block units then shift by 7, equivalent to tile operands multiplied by 384. |
| Scene entity rows and setPos | Literal working coordinates; fixed row X/Y mask with `$3F`, then positions scale by MAP_TILE_SIZE=384. No origin offset is added or subtracted. |
| Inline actor movement | Relative destinations from the current mapped actor; no arrival-to-deployment conversion. |
| setCamDest `(23,18)` / `(5,18)` | Literal camera tile operands scaled by 384, submitted through layer/parallax view state and waited on for scroll completion. They are not actor warps. |
| Ally/enemy placement rows | Store literal roster X/Y; later PositionBattleEntities masks nonnegative positions with `$3F` and multiplies by 384. |

Thus scene Bowie `(27,23)` and ally placement row 0 `(27,23)` address the same cropped coordinate.
Under the copy it corresponds to source `(48,23)`; that correspondence is not another move command.
Parked scene rows `(63,63)` lie beyond the battle rectangle. Camera speed `$30` directly writes the
scrolling speed field, with no measured duration or final-screen guarantee. The later LoadBattle
uses the same map/battle crop rule; scene loading alone does not recompute or repair supplied geometry.

## The Confrontation Cast

**Confirmed:** `ce_4B84C` at `0x4B84C..0x4B88C` contains the main header, seven fixed rows and a
terminator. Loading it disables entity VInts/display, clears entities and lookups, declares
flag-selected followers, processes the fixed rows, then declares Bowie at physical 0 with eas_Idle.
It loads mapsprites and reenables display/VInts. It does not run Map67's ordinary setup init.

| Logical scene actor | Declared position / facing | Sprite and initial action |
| --- | --- | --- |
| Bowie 0, main | `(27,23)`, Left | Class-sensitive ally sprite; eas_Idle. |
| Peter 7, fixed ally | `(28,24)`, Left | Class-sensitive ally sprite; eas_Init, unless his existing lookup skips this duplicate. |
| NPC128 | `(8,22)`, Left | Skeleton sprite 81; eas_Init. |
| NPC129 | `(9,21)`, Left | Dark Cleric sprite 140; eas_Init. |
| NPC130 | `(9,23)`, Left | Dark Madam sprite 134; eas_Init. |
| NPC131 | `(63,63)`, Right | Arrow Launcher sprite 124; eas_Init. |
| NPC132 | `(63,63)`, Right | Harpy sprite 108; eas_Init. |
| NPC133 | `(63,63)`, Right | Orc sprite 76; eas_Init. |

Map67 is an overworld map. F65 selects Caravan as A=30, F67 selects Peter 7, and F68 selects Oddler
as B=31. With only those flags supplied, physical 1/2/3 are Caravan/Peter/Oddler; the fixed Peter row
is skipped and NPC128–133 occupy physical 4–9. With F67 clear but the other two retained, the fixed
row instead declares Peter at the next slot. F66 adds Sarah/Chester; other selectors can add actors
or later replace A/B lookups. No story assumption guarantees these flags or example physical slots.

The fixed Peter duplicate check tests a nonzero ally lookup, not membership or HP. Cleared missing
lookups are 0; GetEntityAddressFromCharacter resolves them to physical 0 without a missing-actor guard.
An absent A's setPos can therefore move Bowie; absent B commands also alias him. Fixed Peter gives
logical 7 a mapping when his follower was absent, without joining him to the battle party.

Battle sprites remain class-sensitive even at zero HP, because CURRENT_BATTLE is 21 rather than 255.
For true allies below 30, setPos/setFacing skip zero-HP actors; IDs 30/31 and NPCs bypass this guard.
Action-script waits, inline actions and text have no equivalent HP test. Dead Peter can retain his
follower-origin position when setPos skips, then still receive movement and speech. Healing after
the scene is too late to change that already executed prefix. Missing/mismapped actors and waits
that do not reach idle are not guaranteed successful scenes.

The shared raft tail remains: mode 0 excludes player-as-raft, but supplied F64 and RAFT_MAP=67 can
declare separate raft physical 31 at masked saved coordinates. It neither converts them by the battle
origin nor rewrites them. Arbitrary follower capacity, mapping collisions and visible raft state
remain Unknown.

## Refusal, Peter's Appeal and the Threat

**Confirmed:** bbcs_21's complete ordered command stream has 37 entries. The accepted research also
owns both complete inline action bodies and the entity table; a top-level command count alone does
not contain those bodies. The non-skipped path proceeds as follows:

| Commands | Scene progression |
| --- | --- |
| 0–2 | Select text 2576, load Map67 with camera `(5,18)`, rebuild the confrontation cast. |
| 3–8 | Install eas_Init and wait for Bowie, then Peter. Set Peter `(28,24)` Left. Install eas_Init and wait for B; set B `(28,22)` Left and A `(28,23)` Left. A receives no new action script here. |
| 9–12 | fadeInB, camera speed `$30`, then NPC128's text 2576 and single-text 2577. |
| 13–18 | Face NPC128, NPC129, NPC130 Right in order; wait 30; move camera to `(23,18)` and wait for scroll; NPC128 speaks 2578. |
| 19–21 | Peter's inline moveLeft 2, endActions and idle wait, then text 2579 and 2580. |
| 22–25 | Return camera to `(5,18)` and wait for scroll; NPC128's inline moveRight 1, endActions and idle wait; text 2581, then wait 50. |
| 26–32 | Reveal NPC131 at `(10,24)` Right; wait 40. Reveal NPC132 at `(10,20)` Right; wait 30. Reveal NPC133 at `(10,22)` Right; wait 20. NPC128 speaks 2582. |
| 33–36 | Install eas_Jump on NPC131 and NPC132 without waiting; install it on NPC133 with an idle wait, then csc_end. There is no separate completion wait for NPC131/NPC132. |

For a mapped living Peter at `(28,24)`, moveLeft 2 requests `(26,24)`; Skeleton 128 at `(8,22)` requests
`(9,22)`. These relative destinations depend on actual mapping/action progress and establish neither
unobstructed movement nor later combat deployment.

The seven source text keys `0A10`–`0A16` provide the whole exchange:

| Text ID | Speaker lookup | Content / controls |
| --- | --- | --- |
| 2576 /0A10 | NPC128 | `(Sniff, sniff)...I smell{N}more invaders from the east.{W2}` |
| 2577 /0A11 | NPC128 | `This is the territory of{N}Mr. Creed.{W1}` |
| 2578 /0A12 | NPC128 | `Go away!{W1}` |
| 2579 /0A13 | Peter 7 | `But we have to see him.{W2}` |
| 2580 /0A14 | Peter 7 | `Please let us pass!{W1}` |
| 2581 /0A15 | NPC128 | `No!  Leave now!{W1}` |
| 2582 /0A16 | NPC128 | `Never mind.  Get 'em boys!{W1}` |

**Inferred:** Peter's appeal and the refusal frame the encounter as denied passage followed by a
threat. There is no YesNoPrompt, selection result or consent branch, and these lines do not establish
Creed's own intentions, an interior visit or a player choice.

**Confirmed service boundary:** the text operand's upper byte is 0 and lower byte selects the actor.
nextText/nextSingleText resolve portrait and speech from its current sprite, wait for scroll, display
cursor text and advance the cursor. nextSingleText additionally closes portrait/text and sleeps 10.
NPC128 is a logical lookup, not an independently guaranteed portrait. `{N}` is newline; W1/W2 name
distinct input-wait services, not measured timing or acknowledged input.

eas_Init sets zero acceleration factors, speed 32/32, disabled acceleration/deceleration,
obstructable/collidable settings, auto-facing, orientation-up, no immersion, size 24 and sprite update,
then eas_Idle. Orientation is separate from facing. eas_Jump disables auto-facing, submits relative
`(0,-1)`, waits 5, submits `(0,1)`, waits 5, submits `(0,0)`, reenables auto-facing and enters idle.
The last actor's wait proves no simultaneous completion or visible jump for all three.

Action waits spin until the mapped actor reaches eas_Idle; inline actions append an idle jump.
csWait calls Sleep with its low-byte count. The debug P2-Start/text-skip path can omit ordinary text
and csWait services. The terminator conditionally waits for scroll when the dialogue-window index
is nonzero, clears camera speed and restores registers. Fade/load/display/VInt, movement, scroll and
text/window services must return to reach later commands. Earlier changes have no blanket rollback
on a stalled service, and F471 is written only at the later start wrapper.

## The Force Is Placed After the Scene

**Confirmed:** spriteset21 at `0x1B4950..0x1B4AA4` has 12 ally records, 12 enemy records, 4 AI regions
and 0 AI points. Its combatant and region records are 12 bytes. Every ally row has source fields
HEALER1/NOTHING, primary NONE/15, secondary NONE/15, filler 0 and STARTING. Ally initialization uses
only these positions, in order:

| Placement row | X/Y | Placement row | X/Y |
| --- | --- | --- | --- |
| 0 | `(27,23)` | 6 | `(28,23)` |
| 1 | `(27,24)` | 7 | `(28,24)` |
| 2 | `(27,25)` | 8 | `(29,24)` |
| 3 | `(28,26)` | 9 | `(29,23)` |
| 4 | `(28,25)` | 10 | `(30,23)` |
| 5 | `(28,22)` | 11 | `(30,24)` |

These row indices are not named allies. After the before scene, healing restores maximum HP/MP for
living allies and immortal Peter 7/Lemon 28, retains only existing STUN/POISON/CURSE bits and updates
stats. It joins nobody and revives no other dead ally. InitializeAllAlliesBattlePositions clears all
30 ally X values to -1; UpdateForce builds the supplied joined/active list in ascending ally ID order.
A dead member consumes a party/loop slot without advancing the placement row. Members beyond 12
processed party slots leave the active party. Fewer eligible members yield fewer positioned actors;
reserves and scene followers are not automatically included. An empty/inconsistent list is not a
guaranteed safe empty case because the first entry is read before exhausted-count checking.

## Twelve Enemy Records and Four Regions

**Confirmed:** the selected enemy inputs are below. All have secondary NONE/15, filler 0 and
STARTING=0. Commandsets HEALER1/HEALER3/ATTACKER1/ATTACKER3 encode 0/2/6/9. FOLLOW_ENEMY|n encodes
`$80|n`; NONE is `$FF`. Sprite IDs used in the scene are distinct from these enemy definition IDs:
scene NPC129 uses a Dark Cleric sprite, while combatant 129 has the base definition Dark Madam.

| Row / combatant | Base type / ID | X/Y | Commandset | Raw item word | Primary order / region |
| --- | --- | --- | --- | --- | --- |
| 0 /128 | Skeleton 17 | `(7,23)` | ATTACKER1 | MEDICAL_HERB\|USABLE_BY_AI `$2000` | NONE /1 |
| 1 /129 | Dark Madam 70 | `(8,22)` | ATTACKER3 | PROTECT_RING\|EQUIPPED\|UNUSED_ITEM_DROP `$4094` | FOLLOW_ENEMY\|0 /1 |
| 2 /130 | High Priest 79 | `(9,24)` | HEALER3 | NOTHING `$007F` | FOLLOW_ENEMY\|0 /1 |
| 3 /131 | Dark Madam 70 | `(13,18)` | ATTACKER1 | NOTHING | NONE /0 |
| 4 /132 | High Priest 79 | `(15,19)` | HEALER3 | NOTHING | FOLLOW_ENEMY\|3 /0 |
| 5 /133 | Orc 12 | `(23,14)` | ATTACKER1 | NOTHING | NONE /15 |
| 6 /134 | Orc 12 | `(19,16)` | ATTACKER3 | NOTHING | FOLLOW_ENEMY\|7 /15 |
| 7 /135 | Skeleton 17 | `(21,17)` | ATTACKER1 | MEDICAL_HERB\|USABLE_BY_AI `$2000` | NONE /15 |
| 8 /136 | Harpy 44 | `(15,25)` | HEALER1 | NOTHING | NONE /2 |
| 9 /137 | Harpy 44 | `(13,31)` | HEALER1 | NOTHING | NONE /3 |
| 10 /138 | Orc 12 | `(20,29)` | ATTACKER1 | NOTHING | NONE /15 |
| 11 /139 | Arrow Launcher 60 | `(26,33)` | ATTACKER1 | NOTHING | NONE /15 |

Each region has leading bytes 4,0, these four coordinate pairs in stored order, then trailing 0,0:

| Region | Ordered coordinates |
| --- | --- |
| 0 | `(0,0)`, `(0,40)`, `(31,15)`, `(31,0)` |
| 1 | `(0,21)`, `(9,32)`, `(20,23)`, `(11,13)` |
| 2 | `(0,15)`, `(0,41)`, `(23,41)`, `(23,15)` |
| 3 | `(0,1)`, `(0,41)`, `(31,41)`, `(31,37)` |

These stored orders, commandsets and region operands do not establish actual following, healing,
attack selection, polygon reach or activation. Enemy initialization scans 128–159: the 12 STARTING rows
enter InitializeEnemyStats; absent slots take max/current HP 0 and X=-1. The initializer resolves any
upgrade, copies the selected definition, fills HP/MP, writes row coordinates, combines definition
movetype with row commandset, stores type/trigger/orders, handles the item, constructs activation
bits, applies shared difficulty adjustment and updates stats. No derived-stat or AI result is claimed.

### Successful Addition Removes the Ring's Equipped Bit

**Confirmed:** a raw item word is not installed inventory. InitializeEnemyItems returns on NOTHING,
leaving definition inventory intact. Otherwise its actual j_AddItem target finds the first free
masked 127 slot among four. On success AddItem masks d1 itself with `$807F`, writes that word and
returns d2=0. Full inventory returns d2=1 without that mask or write. The caller does not branch on
d2; it next tests d1's equipped bit 7 and only if set scans/equips a matching item.

Unchanged base Skeleton has an equipped Broad Sword and three free slots; Dark Madam has an equipped
Power Stick and three free slots. Successful additions therefore install plain Medical Herb `$0000`
for rows 0/7 and plain Protect Ring `$0014` for row 1. The mask clears AI `$2000`, drop `$4000` and
equipped `$80` before the caller tests equipment, so the unchanged row 1 Dark Madam receives an
**unequipped ring** through this path. Upgraded/changed definitions or full inventory retain their
actual success/failure branch. No downstream AI use, drop/reward, item effect or derived-stat claim
follows from the row labels or this immediate initialization.

### F521 Allows an Upgrade, Not a Guaranteed Replacement

**Confirmed:** battle 21 is in the complete eleven-entry random-battle list. IsBattleUpgradable admits
it, and DetermineBattleUpgrade tests F500+21=F521. Clear F521 preserves original types. Set F521 uses
the original movetype, Bowie's class-adjusted effective level relative to battle 21, the selected
range, RNG and exclusion/fallback logic. A zero quotient of the positive level difference divided
by 10 still retains the original type; entering this path does not guarantee a changed enemy.

Regular Skeleton/Orc and Brass Gunner Arrow Launcher use the melee table; Mage Dark Madam uses mage,
Healer High Priest uses healer and Flying Harpy uses airborne. The airborne exclusion list is empty:
the consumer subtracts 1 from that zero length before its exclusion DBF loop, and the source comment warns
of upgrade failure. Preserve that boundary rather than promise successful empty-list handling or a
final RNG result. Installed upgraded roster remains conditional on the code returning; row positions,
commandsets, orders and items remain inputs even when the copied definition changes. Admission's
F421 clear does not disable F521 here; F471 does not gate this consumer.

## Battle Loading Replaces the Scene Cast

**Confirmed:** LoadBattle fades out, loads tilesets, clears VInts and calls PositionBattleEntities.
That clears old entities/lookups and declares actors for nonnegative combatant X/Y, excluding
neutral-marked enemies from the ordinary enemy pass. Ordinary actors use standing actions and Down
facing. Earlier physical mappings, parked/revealed NPCs and action scripts are not the new roster.
The complete neutral table has no Battle21 row: F521-clear scanning reaches its terminator, while
F521-set entry skips it. Neither adds a neutral or carries scene Oddler/Caravan/Peter into battle as one.

Fresh d0=0 selects the view target; a missing mapping falls back to physical 0 for coordinate reads,
without repairing membership. LoadBattle initializes sprites, loads the current map/view and entity
sprites, installs base VInts, invokes the existing terrain loader, submits map music and fades in.
The empty or skipped start seam then reaches the stated stop. Terrain decoding, visible/audible
results, responsive control and later activation/turns remain outside the proven boundary.

## Evidence and Remaining Questions

**Confirmed provenance:** this guide consumes the research accepted in main
`0a4e8c6980086ad712ce1bcde740583d21f69a87`, with
[SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The [research reproduction boundary](../../research/battle21-opening-and-initial-roster.md#counterexamples-reproduction-and-limits)
owns named source paths, addresses, complete selected commands/entities/text/placements and consumer
joins. No raw-source reinvestigation, regenerated comparison or runtime acquisition is performed here.

| Reviewed owner | What it supports and what remains outside it |
| --- | --- |
| Accepted Battle21 research, predecessor return and shared Battle20 entry consumers | Complete selected admission, before/start, cropped layout, follower/HP, source rows, random/neutral and actual item-mask joins. These accepted selected joins, not public aggregate counts, establish the local content. |
| `sf2-battle-cutscenes-static-v1`, [fixture](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), expected.cutsceneFacts.intro; [routing contract](../contracts/battle-cutscene-routing.md) | Shared intro check/write order. Older contract Unknowns for exact route targets/empty execution are aggregate limits; accepted Battle21 research supplies these selected joins, without closing other routes. |
| `sf2-battle-loop-static-v1`, [fixture](../../../tests/fixtures/h2/battle-loop-static-v1.json), expected.lifecycleFacts.rosters/betweenBattleHealing; [lifecycle contract](../contracts/battle-control-lifecycle.md#entry-and-round-scheduling) | Shared slot/healing boundary. The shared entry-order summaries agree with the accepted exact research trace: before → reset/heal/ally then enemy initialization/AI clear/load → start. Battle01 activation/turn observations do not prove Battle21 behavior. |
| `sf2-battle-spriteset-data-static-v1`, [fixture](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json), table.BattleSpriteset21 and expected.facts.rowContentTracked=false; [encounter contract](../contracts/battle-encounter-definition.md) | Address and aggregate shape only. Neither aggregate totals nor macro counts serialize all 24 selected combatant rows or 48 region bytes. |
| `sf2-map-script-engine-static-v1`, [fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json), expected.programSummary and command families | Shared script/consumer context. The accepted retained corpus selection entryLabel=bbcs_21 has 37 commands; inline bodies/entity rows and literal text remain separately research-owned. |

Schema/verifier boundaries inspected are the
[cutscene fixture schema](../../../schemas/h2-battle-cutscenes-static-fixture.schema.json) and
[battle_cutscenes.py](../../../src/sf2tool/h2/battle_cutscenes.py),
[loop fixture schema](../../../schemas/h2-battle-loop-static-fixture.schema.json) and
[battle_loop.py](../../../src/sf2tool/h2/battle_loop.py),
[spriteset fixture schema](../../../schemas/h2-battle-spriteset-data-static-fixture.schema.json) and
[battle_spriteset_data.py](../../../src/sf2tool/h2/battle_spriteset_data.py), and
[script fixture schema](../../../schemas/h2/map-script-engine-fixture.schema.json) with
[map_script_engine.py](../../../src/sf2tool/h2/map_script_engine.py).
Their existing source/provenance/model checks retain their own scope; no H2 payload is regenerated.

Preserve [PR886's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/884#issuecomment-6172444717),
[rebase handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/884#issuecomment-6172589433)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/886#issuecomment-6172605301),
including retained comparison receipts, original/final CI, resolved inspection failures and linked
predecessor results. Review records locate results; tracked research and exact accepted Git objects
own original-game claims. This guide's checks, planner and final CI belong to Issue 887's handoff.

**Unknown:** natural admission; actual flags, party, HP/classes, follower/raft mappings and collisions;
completed fade/load/text/window/scroll/action services; RNG-dependent upgrade result, including the
empty airborne-exclusion boundary; actual initial roster, presentation/sound, usable control and
saved persistence. F421 absent, F471 set with fresh initialization, F521 set with unseen intro,
missing A/B, dead Peter before later healing, changed party/capacity and inconsistent F88 state are
supplied-state counterexamples to static overclaims, not executed scenarios. Missing observations
create no automatic runtime queue and complete neither the campaign nor design epic.
