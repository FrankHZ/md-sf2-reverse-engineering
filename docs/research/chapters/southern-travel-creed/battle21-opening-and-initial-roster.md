# Battle21 opening and initial roster

Evidence date: 2026-10-14. **Confirmed:** admitted fresh Battle21 selects the
complete `bbcs_21` confrontation when F471 is clear, then heals and initializes
the supplied party and twelve enemy rows before loading battle and the empty
start program. The scene and deployment use the cropped battle layout, whose
source origin is Map67 `(21,0)`; Fairy Cave arrival coordinates do not deploy
combatants. F471, F521, follower selectors and saved F88 state have separate
consumers. **Inferred:** the scene stages a refusal followed by a threat of
attack. **Unknown:** natural reach, supplied state, service completion, actual
roster and presentation. No choice, played encounter or tactical result is
established by these static facts.

## Agreement and provenance

Standard rigor under [Issue #884](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/884)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
establish the selected complete before scene and fresh/suspended entry through
initial roster loading for the next content guide. Accepted starting base is
`05e2fd194be07c6581e4fd3fe1abf3b38854ae65`. This is the sole owned document;
no shared-path or stacked dependency, new verification machinery or runtime
acquisition. Fresh entry stops at `BattleLoop:@Start` **before ActivateEnemies**;
suspended entry stops before `@IndividualTurns_Loop`. Exclude turn generation,
activation/AI execution, damage, outcomes, after/defeated scenes, Creed entry,
terrain decoding and a whole renderer audit.

Consume accepted [Fairy Cave exploration and Devil's Tail return](./fairy-cave-exploration-and-devils-tail-return.md),
[battle routing](../../battle/battle-cutscenes.md), [lifecycle](../../battle/battle-loop.md),
[spriteset inventory](../../battle/battle-spriteset-data.md) and the shared
[Battle20 entry/roster consumers](./harpy-pond-approach-through-battle20-entry.md).
Source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The assigned read-only checkout is clean at that pin. Retained evidence binds
USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Fnnn means flag nnn; rows and command indices are zero-based. Coordinates and
wait operands are source values, not observed positions or measured durations.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine pair and small
selected source/fixture reads. Linear comparisons are budgeted below 512 MiB
and one minute with only small receipts/prose. The retained comparisons and
cutscene-source comparison passed in 0.084 seconds. No environment, source
checkout, capture or payload is created; reassess before material expansion.

## Admission, fresh entry and scene gates

**Confirmed:** the accepted predecessor owns Map32's two no-scroll exits to
Map67 `(51,23)` and `(57,24)`, Down, and the separate default random-battle and
packed-reload request paths. Preserve their pending-slot, producer and
consumption conditions. Neither exit proves traversal to a deployment tile.
`code/common/maps/getbattle.asm:CheckBattle` and the complete 45-row
`data/battles/global/battlemapcoords.asm` select the sole Map67 row, slot 21:
area `(21,0)`, size 32x42, trigger `(255,255)`. Supplied F421 admits the row
without checking rectangle containment. F521 set clears F421 **after selection**
and still returns battle 21; F521 clear retains F421. F471 and party/follower
state do not gate this scan. An absent F421 is not repaired by arrival or F471.

`code/gameflow/battle/battleloop_1.asm:BattleLoop` clears PLAYER_TYPE to 0 and
sets F399 before testing F88. On fresh F88-clear entry:

1. Clear seconds, save incoming map/battle words, write CURRENT_MAP=67 and
   CURRENT_BATTLE=21, install base VInts, execute the before wrapper.
2. Restore incoming words and rewrite current map/battle. Clear region flags
   90–105, heal living/immortal allies, initialize all ally then enemy positions.
3. Clear AI memory, set d0=0, LoadBattle, execute the start wrapper.
4. Reach `@Start`; stop before its ActivateEnemies call.

`beforebattlecutscenes.asm` slot 21 selects `bbcs_21` at `0x4B790` in
`data/battles/entries/battle21/cs_beforebattle.asm`. Slot 21 of
`data/battles/cutscenes/battlestartcutscenes.asm` selects `ms_Empty` at `0x47A4E`,
whose complete body in `code/common/scripting/map/ms_empty.asm` is `$FFFF`.
No Battle21-specific start file is inferred. The actual wrappers are
`code/gameflow/battle/cutscenes/beforebattlecutscenesstart.asm` and
`battlestartcutscenesstart.asm`, with their corresponding `*end.asm` returns.
Both add CURRENT_BATTLE to intro base 450 and check F471. Before does not set
it; start sets it **before** dispatching the empty script when clear. F471 set
skips both selected scripts but not the fresh healing/roster/load stages.
Empty still enters/terminates ExecuteMapScript, including its shared services.
The before program itself contains no flag write, recruitment, request or choice.

F88 set takes the saved-state branch after mode/F399: restore saved seconds,
clear F88 and AI memory, clear view target, LoadBattle, then branch directly to
`@IndividualTurns_Loop`. The following branch to `@Start` is unreachable.
Incoming map/battle assignment, before/start scripts, fresh healing and roster
initialization are bypassed; d0 is not explicitly cleared here. Current map,
battle, area, combatants and continuation state must already be coherent.
Incoming battle 21 is not a saved-state repair. This account stops before any
individual turn and makes no suspended-roster claim from the fresh table.

## Coordinate domains and the two map loads

**Confirmed:** the nonzero origin is consumed by the map loader, not by silently
subtracting 21 from every script/roster X. The required joins are:

| Consumer | Actual domain and operation |
| --- | --- |
| CheckBattle | Writes BATTLE_AREA_X/Y=`21,0` and WIDTH/HEIGHT=`32,42` from row 21. Arrival X/Y are trigger inputs; wildcards accept them without assigning a combatant position. |
| `mapscriptengine_1.asm:csc37_loadMapAndFadeIn` → `csc48_loadMap` | Starts fade-out, disables view tracking, loads tilesets, waits for fading, deactivates entity VInts and calls LoadMap with explicit Map67 and packed camera `(5,18)` multiplied by 3. It does not change CURRENT_BATTLE to 255. |
| `code/common/maps/mapload.asm:LoadMap` → LoadMapBlocksAndLayout | The explicit nonnegative map selects a full load. After base blocks/layout, flag copies and chest overlays, CURRENT_BATTLE!=255 copies the 32x42 rectangle at source `(21,0)` to destination `(0,0)`. |
| `code/gameflow/exploration/exploration.asm:CopyMapBlocks` | Packed high-byte X/low-byte Y and width/height address the 64-wide working word layout; destination d2=0 is the origin. This establishes the coordinate relation without decoding terrain. |
| `mapload.asm:LoadMapArea` | Battle branch sets layer-1 start X/Y to 0 and ends to `(width-1,height-1)*384`, hence `(31,41)*384`. Other layer parameters still come from the selected map area; no claim about their rendered result. |
| `mapfunctions.asm:InitializeMapEntities`, csc19 setPos, csc2D movement | Main/fixed positions become internal units using MAP_TILE_SIZE=384 (fixed rows mask X/Y with `$3F`). setPos multiplies its literal coordinates directly; inline moves install relative destinations. None applies BATTLE_AREA_X. |
| csc32 setCamDest → `battlefunctions_0.asm:SetCameraDestination` → `tech/graphics/display.asm:SetViewDestination` | Disables tracking, multiplies tile operands by 384, submits view destinations through map layer/parallax state and waits for scroll completion. No battle-origin subtraction/addition occurs here. |
| Fresh InitializeAllAllies/EnemiesBattlePositions → PositionBattleEntities | Store the selected roster X/Y directly, then declare battle actors from nonnegative coordinates, masked by `$3F` and multiplied by 384. Later LoadBattle uses the same current map/battle and cropped layout rule. |

`LoadMap` decodes the scene's camera packing into block units then shifts by 7;
for `(5,18)` this is the same `*384` domain. The later `setCamDest (23,18)` and
`(5,18)` are camera operands, not player warps or actor centers. Camera speed
`$30` writes VIEW_SCROLLING_SPEED directly. No frame-duration or final-screen
claim follows from these operands.

Thus scene Bowie `(27,23)` and ally placement row 0 `(27,23)` address the same
cropped layout coordinate. Its source-layout correspondence is `(48,23)` under
the actual copy, not a command to move Bowie to that global coordinate. The
exit `(57,24)` can admit battle even outside the source area's X=21..52 span;
there is no entry-coordinate-to-deployment conversion. The parked `(63,63)`
scene rows are beyond the battle rectangle, not valid deployment assertions.
Changes to supplied/saved battle geometry would affect this relationship;
scene loading alone does not recompute or repair the area fields.

## Complete scene population and actor identity

**Confirmed:** `ce_4B84C` (`0x4B84C..0x4B88C`) has a main header, seven fixed
rows and `cscEntitiesEnd`. `csc42_loadMapEntities` disables entity VInts/display,
reads the three header words, invokes InitializeMapEntities, loads mapsprites,
then reenables display/VInts. It does not call Map67's ordinary map-setup init.
InitializeMapEntities clears entities/lookups, declares flag-selected followers
starting at physical 1, processes these fixed rows, then declares Bowie at
physical 0 using the main header and `eas_Idle`.

| Declaration / logical lookup | X/Y, declared facing | Sprite / initial action |
| --- | --- | --- |
| main / Bowie 0 | `(27,23)`, Left | GetAllyMapsprite(0), eas_Idle |
| fixed ally / Peter 7 | `(28,24)`, Left | GetAllyMapsprite(7), eas_Init; duplicate check below |
| NPC 128 | `(8,22)`, Left | SKELETON 81, eas_Init |
| NPC 129 | `(9,21)`, Left | DARK_CLERIC 140, eas_Init |
| NPC 130 | `(9,23)`, Left | DARK_MADAM 134, eas_Init |
| NPC 131 | `(63,63)`, Right | ARROW_LAUNCHER 124, eas_Init |
| NPC 132 | `(63,63)`, Right | HARPY 108, eas_Init |
| NPC 133 | `(63,63)`, Right | ORC 76, eas_Init |

Fixed ally rows test the existing ally lookup byte, not HP or membership.
A nonzero Peter mapping skips his fixed declaration while consuming its action
pointer; zero declares/maps him before the six NPCs. The NPC lookup list starts
at offset 32 (logical 128 minus 96), independently of physical actor allocation.
These sprite IDs are not enemy type IDs. In particular, scene 129 is a Dark
Cleric sprite; later battle combatant 129 is a Dark Madam definition.

`followersfunctions_1.asm:InitializeFollowerEntities` uses
`data/scripting/entity/followers.asm`. Map67 is present in the complete
`data/maps/global/overworldmaps.asm`, so F65 selects the overworld prefix:
Caravan mapped as A=30, followed by the normal ordered rows. F67 selects Peter;
F68 selects Oddler mapped as B=31. With only F65/F67/F68 supplied, physical
1/2/3 are A/Caravan, Peter and B/Oddler; the fixed Peter row is skipped and
NPCs 128–133 become physical 4–9. With F67 clear but those other flags retained,
fixed Peter occupies the next physical slot instead. F66 adds Sarah/Chester;
other supplied rows can add actors and later overwrite A/B lookups. No story
assumption guarantees F65, F68 or the example mapping.

`ClearEntities` clears lookup bytes to zero. GetEntityAddressFromCharacter
masks the logical ID, subtracts 96 for an enemy ID, reads the lookup byte and
computes the physical address **without a missing-actor guard**. An absent A
or B therefore aliases physical 0 here; it is not a harmless skipped command.
A's setPos can move Bowie when no A exists. Fixed Peter does provide a mapping
when the follower did not, independent of whether Peter joined the battle party.
The scene does not rebuild followers after each individual command.

`getallymapsprite.asm:GetAllyMapsprite` only selects a dead ally's blue flame
when CURRENT_BATTLE=255. This scene already has battle 21, so Bowie/Peter use
the class-sensitive sprite route even at zero HP. Declaration is not revival.
In `mapscriptengine_1.asm`, setPos and setFacing use
AdjustScriptPointerByCharacterAliveStatus: zero-HP true allies (<30) skip the
operation; follower IDs 30/31 and NPC IDs bypass that HP check. SetActscriptWait,
inline entityActionsWait and these text commands have no equivalent HP guard.
A dead follower Peter may retain his follower-origin position when setPos is
skipped, then still receive the inline move and speech. Later healing is too
late to retroactively fix the scene. Missing/mismapped actors and stalled idle
waits are not converted into a guaranteed successful scene.

The shared `sub_44404` tail still runs. BattleLoop's mode clear prevents its
player-as-raft branch, but supplied F64 plus RAFT_MAP=67 can declare separate
raft physical 31 at masked saved X/Y. It does not convert those saved values
by the battle origin or rewrite them. No visible raft or sane arbitrary
follower-capacity configuration is asserted.

## Complete before program and text

**Confirmed:** the retained `programCorpus.programs` entry `bbcs_21` has commands
0–36. Direct source review also includes both inline action bodies, which are
not separate top-level command entries, and all of `ce_4B84C`.

| Commands | Ordered operation |
| --- | --- |
| 0–2 | Set text cursor 2576; load Map67 with camera `(5,18)`; rebuild scene entities from ce_4B84C. |
| 3–8 | Install eas_Init and wait for Bowie, then Peter; set Peter `(28,24)` Left; install eas_Init and wait for B; set B `(28,22)` Left, A `(28,23)` Left. A gets no new action script here. |
| 9–12 | fadeInB; camera speed `$30`; NPC128 emits 2576 via nextText, then 2577 via nextSingleText. |
| 13–18 | Face NPC128,129,130 Right in that order; csWait 30; camera `(23,18)` and scroll wait; NPC128 emits 2578. |
| 19–21 | Peter's complete inline body is moveLeft 2, endActions, with idle wait; Peter emits 2579 then 2580. |
| 22–25 | Camera `(5,18)` and scroll wait; NPC128's complete inline body is moveRight 1, endActions, with idle wait; NPC128 emits 2581; csWait 50. |
| 26–32 | Set NPC131 `(10,24)` Right; wait 40; NPC132 `(10,20)` Right; wait 30; NPC133 `(10,22)` Right; wait 20; NPC128 emits 2582. |
| 33–36 | Install eas_Jump on 131, then 132 without waiting; install it on 133 and wait for eas_Idle; csc_end. There is no separate wait for 131/132. |

For an admitted living Peter at `(28,24)`, moveLeft 2 requests `(26,24)`; for
NPC128 at `(8,22)`, moveRight 1 requests `(9,22)`. These are requested relative
moves, conditional on mapping/action progress. They do not deploy the later
combatant rows or prove unobstructed movement.

Literal text is from `data/scripting/text/gamescript.txt` keys `0A10`–`0A16`,
not just comments attached to scene commands:

| Decimal / hex | Speaker lookup | Literal text and controls |
| --- | --- | --- |
| 2576 / 0A10 | 128 | `(Sniff, sniff)...I smell{N}more invaders from the east.{W2}` |
| 2577 / 0A11 | 128 | `This is the territory of{N}Mr. Creed.{W1}` |
| 2578 / 0A12 | 128 | `Go away!{W1}` |
| 2579 / 0A13 | Peter 7 | `But we have to see him.{W2}` |
| 2580 / 0A14 | Peter 7 | `Please let us pass!{W1}` |
| 2581 / 0A15 | 128 | `No!  Leave now!{W1}` |
| 2582 / 0A16 | 128 | `Never mind.  Get 'em boys!{W1}` |

There is no YesNoPrompt, selection result or consent branch. Commands use
`$0` as the text operand's upper byte; the lower byte selects the speaker.
`mapscriptengine_2.asm:csc02/csc00` resolve portrait/speech through the mapped
actor's current sprite, wait for scrolling, display the cursor text, then
increment it. nextSingleText additionally closes portrait/text and Sleep(10).
`getentityportaitandspeechsfx.asm` owns sprite-to-portrait/speech lookup and its
default fallback; logical NPC128 alone is not a portrait identity.

`data/scripting/entity/eas_actions.asm:eas_Init` installs zero acceleration
factors, speed 32/32, disabled acceleration/deceleration, obstructable/collidable
settings, auto-facing, orientation-up, no immersion, size 24 and sprite update,
then jumps to eas_Idle. Orientation is a separate field from declared facing.
`eas_Jump` disables auto-facing, submits relative `(0,-1)`, waits 5, submits
`(0,1)`, waits 5, submits `(0,0)`, reenables auto-facing and jumps to eas_Idle.
No displacement duration, simultaneous completion or rendered jump is inferred.

SetActscriptWait and entityActionsWait spin on the mapped actor reaching
eas_Idle; the latter compiles its inline body and appends that idle jump.
csWait uses Sleep with its encoded low-byte count. ExecuteMapScript's debug
P2-Start/text-skip path can omit ordinary text and csWait services; the sequence
above describes the non-skipped path. Its terminator conditionally waits for
view scrolling when the dialogue-window index is nonzero, clears camera speed
and restores registers. Map/fade/display/VInt, text/window, movement and scroll
services must return for later steps to run. Earlier mutations are not rolled
back on a stalled service. F471 is only written at the later start wrapper,
so a partial before-script is not itself a completed intro flag transition.

## Complete spriteset21 and immediate initialization

**Confirmed:** `data/battles/spritesets/entries.asm` slot 21 points to
`BattleSpriteset21` in `spriteset21.asm` (`0x1B4950..0x1B4AA4`). Its complete
header is **12 allies, 12 enemies, 4 AI regions, 0 AI points**.
`GetBattleSpritesetSubsection` selects by CURRENT_BATTLE and advances by the
actual subsection counts/strides. `sf2macros.asm` defines each 12-byte combatant
record: type/index, X,Y, commandset byte, item word, primary order/region,
secondary order/region, filler and initialization byte.

Ally placement rows 0–11, in order, are `(27,23)`, `(27,24)`, `(27,25)`,
`(28,26)`, `(28,25)`, `(28,22)`, `(28,23)`, `(28,24)`, `(29,24)`, `(29,23)`,
`(30,23)`, `(30,24)`. Every row carries HEALER1/NOTHING, primary NONE/15,
secondary NONE/15, filler 0, STARTING. The numeric row indices 0–11 are not an
imposed named roster: InitializeAllAlliesBattlePositions consumes only their
positions for the supplied party, not their AI/item/order fields.

`battleloop/heallivingandimmortalallies.asm` restores HP/MP maxima for living
allies and immortal Peter 7/Lemon 28. Its intermediate status mask retains
STUN/POISON/CURSE; UpdateCombatantStats then preserves STUN/POISON and derives
final CURSE from equipped cursed items, rather than retaining an old CURSE bit
alone. It does not join anyone or revive other dead allies. The
[initialization/first-control owner](./battle21-initialization-and-first-control.md)
continues through regions, live orders, repeat selection and actual item custody.
`battleloop/initializecombatants.asm:InitializeAllAlliesBattlePositions` clears
all 30 ally X positions to -1, calls `code/common/stats/battleparty.asm:UpdateForce`
(joined/active flags, ascending ally IDs), then uses that list. A dead member
consumes a party/loop slot without advancing the placement row. Beyond twelve
processed party slots, remaining members leave the active party. Fewer eligible
members produce fewer positioned actors; reserves and scene followers are not
implicitly included. The first party entry is read before exhausted-count
checking, so an empty/inconsistent list is not guaranteed a safe empty result.

All enemy rows have secondary NONE/15, filler 0 and STARTING=0. The differing
operands are exact below; `FOLLOW_ENEMY|n` encodes `$80|n`, and NONE is `$FF`.
Commandsets HEALER1/HEALER3/ATTACKER1/ATTACKER3 are 0/2/6/9 respectively.

| Row / combatant | Base enemy type (ID) | X/Y | Commandset | Item word | Primary order / region |
| --- | --- | --- | --- | --- | --- |
| 0 /128 | SKELETON (17) | `(7,23)` | ATTACKER1 | MEDICAL_HERB\|USABLE_BY_AI (`$2000`) | NONE /1 |
| 1 /129 | DARK_MADAM (70) | `(8,22)` | ATTACKER3 | PROTECT_RING\|EQUIPPED\|UNUSED_ITEM_DROP (`$4094`) | FOLLOW_ENEMY\|0 /1 |
| 2 /130 | HIGH_PRIEST (79) | `(9,24)` | HEALER3 | NOTHING (`$007F`) | FOLLOW_ENEMY\|0 /1 |
| 3 /131 | DARK_MADAM (70) | `(13,18)` | ATTACKER1 | NOTHING | NONE /0 |
| 4 /132 | HIGH_PRIEST (79) | `(15,19)` | HEALER3 | NOTHING | FOLLOW_ENEMY\|3 /0 |
| 5 /133 | ORC (12) | `(23,14)` | ATTACKER1 | NOTHING | NONE /15 |
| 6 /134 | ORC (12) | `(19,16)` | ATTACKER3 | NOTHING | FOLLOW_ENEMY\|7 /15 |
| 7 /135 | SKELETON (17) | `(21,17)` | ATTACKER1 | MEDICAL_HERB\|USABLE_BY_AI (`$2000`) | NONE /15 |
| 8 /136 | HARPY (44) | `(15,25)` | HEALER1 | NOTHING | NONE /2 |
| 9 /137 | HARPY (44) | `(13,31)` | HEALER1 | NOTHING | NONE /3 |
| 10 /138 | ORC (12) | `(20,29)` | ATTACKER1 | NOTHING | NONE /15 |
| 11 /139 | ARROW_LAUNCHER (60) | `(26,33)` | ATTACKER1 | NOTHING | NONE /15 |

The four region records are each twelve bytes: leading `4,0`, four ordered
coordinate pairs, trailing `0,0`. Preserve their complete operands without
claiming activation or polygon reach:

| Region | Four coordinate pairs in stored order |
| --- | --- |
| 0 | `(0,0)`, `(0,40)`, `(31,15)`, `(31,0)` |
| 1 | `(0,21)`, `(9,32)`, `(20,23)`, `(11,13)` |
| 2 | `(0,15)`, `(0,41)`, `(23,41)`, `(23,15)` |
| 3 | `(0,1)`, `(0,41)`, `(31,41)`, `(31,37)` |

InitializeAllEnemiesBattlePositions scans slots 128–159. The twelve STARTING
rows enter InitializeEnemyStats; remaining slots take the absent/hidden path
with max/current HP=0 and X=-1. InitializeEnemyStats first resolves any random
upgrade, copies that enemy definition, fills HP/MP, installs row X/Y, combines
its movetype with the row commandset, stores enemy type/trigger/order fields,
handles the item word, constructs activation bits, applies shared difficulty
adjustment and updates stats. STARTING and these stored orders do not mean the
enemy has activated, followed, attacked, healed or consumed an item here.

### Item operands versus installed inventory

**Confirmed:** `InitializeEnemyItems` returns immediately for NOTHING; it does
not erase definition inventory. For other words it calls `j_AddItem`, whose
actual `s02_jumpinterface.asm` target is `itemstats.asm:AddItem`. That routine
finds the first free masked-127 slot among four, then masks **d1 itself** with
`ITEMENTRY_MASK_INDEX_AND_BROKEN_BIT=$807F`, writes it and returns d2=0. Full
inventory returns d2=1 without that mask/write. InitializeEnemyItems does not
branch on d2; it then tests d1's equipped bit 7 and only if set scans/equips a
matching item. Do not infer installed equipment from the row symbol alone.

For unchanged base definitions in `data/stats/enemies/enemydefs.asm`, Skeleton
has an equipped Broad Sword and three free slots; Dark Madam has an equipped
Power Stick and three free slots. Successful additions therefore install plain
Medical Herb `$0000` for rows 0/7 and plain Protect Ring `$0014` for row 1.
The successful AddItem mask clears `$2000`, `$4000` and equipped `$80` before
the caller's bit test, so this immediate path does not equip the added ring.
This is a source-level result, not an observed inventory or claim about later
AI/drop consumers of the original row. Changed/upgraded definitions and full
inventory must retain the actual success/failure branch, not assume addition.
No item use, reward/drop or derived-stat result is researched here.

### Completion-gated random upgrade and battle actors

**Confirmed:** the complete length-prefixed eleven-entry
`data/battles/global/randombattles.asm:list_RandomBattles` **includes DEVIL_TAIL
(21)**. `battleloop/upgradeenemies.asm:IsBattleUpgradable` admits it;
DetermineBattleUpgrade checks F500+21=F521. F521 clear returns the original
type. F521 set enters the upgrade path using the original definition's movetype,
Bowie's class-adjusted effective level relative to battle 21, the selected
upgrade range, RNG and exclusion/fallback logic. A zero quotient for the
positive level difference divided by ten returns the original type; admission
to the upgrade path alone does not guarantee a changed enemy.

The selected base types route Regular Skeleton/Orc and Brass Gunner Arrow
Launcher to the melee table, Mage Dark Madam to mage, Healer High Priest to
healer, and Flying Harpy to airborne. The airborne exclusion list is empty;
its source comment warns of upgrade failure, and the consumer subtracts one
from that zero length before its exclusion DBF loop. This document does not
turn it into a guaranteed successful empty-list case or predict a final RNG
result. The exact installed upgraded roster remains conditional on this code
returning. Row positions/commandsets/orders/items remain the row inputs even
when the copied definition changes. F421 having just been cleared by admission
does not disable F521's separate initialization consumer; F471 does not gate it.

`code/gameflow/battle/battlefunctions/loadBattle.asm:LoadBattle` fades out,
loads tilesets, clears VInts and invokes
`code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities`.
It clears old entity data/lookups, declares actors for nonnegative combatant
X/Y, excludes neutral-marked enemies from the ordinary enemy pass, and uses
standing actions and Down facing for ordinary actors. None of the old scene
physical mappings, parked/revealed actors or action scripts is that fresh
combat roster. The full `data/battles/global/battleneutralentities.asm` contains
no Battle21 entry: with F521 clear the neutral scan reaches its terminator;
with F521 set it is skipped. Neither path adds a Battle21 neutral or carries
scene Oddler/Caravan/Peter over as a neutral actor.

LoadBattle maps its supplied combatant to a view target (fresh d0=0); a missing
mapping falls back to physical 0 for coordinate reads, not a membership repair.
It initializes sprites, loads the current map/view, loads entity sprites,
installs base VInts, loads battle terrain through the existing loader, submits
map music and fades in. Terrain decoding and presentation remain outside this
account. The empty/omitted start script then reaches the stated fresh stop.

## Counterexamples, reproduction and limits

| Supplied case | Consequence inside this boundary |
| --- | --- |
| F421 clear, regardless of F471/F521 | No row-21 admission through CheckBattle. |
| F421 set, F521 clear, F471 clear, F88 clear | Full selected before scene, fresh initialization with base enemy types, empty start sets F471 if reached. |
| F471 set on a fresh admission | Both scene wrappers skip; fresh healing, roster and battle load still occur. |
| F521 set, F421 set, F471 clear | Admission clears F421; before scene still runs; enemy initialization may upgrade. A completion flag does not imply intro was already seen. |
| F65/F68 absent or other follower flags supplied | A/B may alias physical 0 or name a different last-written follower. Fixed Peter duplicate logic is separate. |
| Dead Peter/Bowie before the scene | Actor declaration and class sprite selection still occur; HP-guarded position/facing can skip while action/text commands do not. Healing occurs afterward. |
| Changed joined/active/dead party or capacity | Placement consumes the actual updated list; twelve rows do not force twelve living allies or any named party member. |
| F88 set with inconsistent saved map/area/roster | Saved branch loads that state without fresh assignment/repair; fresh table is not its resulting roster. |
| A required fade/text/movement/scroll service does not return | Only the completed prefix is established; no blanket rollback, F471 completion or usable control is guaranteed. |

Load `. ./local/private-inputs.ps1` in the same process as controlled tools and
verify the assigned upstream pin/clean status. Reproduce the existing
[retained comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init equality, `_verify_map_script_engine_fixture` and source
`_cutscene_facts` equality. Select `programCorpus.programs` by
`entryLabel == "bbcs_21"`, not guessed keys; compare ordered `macro`/`arguments`
with the complete named source after stripping comments. Read both inline
bodies and the entity table directly; the top-level corpus is not their full
semantic proof. Text keys are four-digit hexadecimal followed by `=`.

| Existing evidence / direct source | Review boundary |
| --- | --- |
| `map-init-static-v1.json`, `map-script-engine-static-v1.json` and unchanged retained pair | Existing equality and selected command-stream context; no fresh ROM/H1 or runtime observation. |
| `battle-cutscenes-static-v1.json`, actual before/start tables/wrappers and ms_Empty | Shared wrapper-source comparison, exact slot 21 and F471 polarity/write order. |
| `battle-loop-static-v1.json`, `battle-spriteset-data-static-v1.json`, `common-stats-static-v1.json` | Existing lifecycle/inventory context; complete spriteset21 and immediate source consumers reviewed directly. Aggregate inventory does not guard every selected row. |
| Named map-load/copy/area/camera and entity/follower helpers | Nonzero origin, domains, actual aliases, HP/lookup/duplicate rules and partial-return services. No whole renderer or terrain claim. |
| Complete bbcs_21/ce_4B84C, inline bodies, text, macros/enums, random/neutral tables and item consumers | Exact selected source-to-prose joins, including type versus sprite IDs and item masking after AddItem. |

Direct document/scope/provenance/link/private checks and `git diff --check`
apply. Run configured `uv run sf2 verify plan --base origin/main --head HEAD`
on the clean committed candidate and record actual final-head CI in the Issue
handoff. No blanket local normal/full/engine/Godot/H3/H4 suite is selected
solely for prose; no new fixture, parser, schema, index or verification program.

**Unknown:** natural admission and supplied flags/party/HP/classes, exact live
actor/follower/raft mappings, collision/layout behavior, completed action/fade/
scroll/text/window services, RNG-dependent upgrade result, actual initial roster,
view/sound/presentation, saved persistence and usable control. These are not an
automatic runtime queue under
[ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md).
Preserve [Issue880's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/880#issuecomment-6172113709)
and [PR882 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/882#issuecomment-6172145421),
including their retained comparisons, original/rebased CI and linked failures.
Current checks, corrections and exact candidate identities belong in Issue884's
handoff. Freeze a clean pushed Draft PR for independent main-gate review;
no integration, cleanup or successor selection.
