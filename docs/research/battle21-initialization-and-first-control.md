# Battle21 regions, repeat upgrades, item custody and first control

Evidence date: 2026-10-23. **Confirmed:** complete12/12/4/0 connects actual
party/definition refresh, four regions, live orders, item custody and first
conditional control. All96 fresh triangle substitutions miss under the actual
signed-projection consumer. F521 enables level-dependent repeat selection,
including the unsafe airborne exclusion scan. AddItem strips the added Herbs'
AI-use bit and Ring's equipped bit; the independent drop table selects Broad
Sword135 and Protect Ring129. **Inferred:** the declarations support the intended
Devil's Tail encounter. **Unknown:** natural state/reach, installed repeated
roster, first actor/actions, loot receipt, services, presentation and persistence.
This is static conversion guidance, not played21 or admitted production content.

## Agreement and provenance

Standard rigor under [Issue1248](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1248)
and [Epic1053's scope](./story-content-coverage.md#scope-and-reading-rules).
Party, geometry, upgrade, AI and inventory lifetimes cross existing owners; the
outcome is one usable account through conditional individual control. The
[opening owner](./battle21-opening-and-initial-roster.md) retains before21,
scene/follower identity and crop loading. The
[outcome owner](./battle21-outcomes-and-devils-tail-return.md) retains leader
death, victory, after21 and the Devil's Tail return. The
[Fairy Cave return](./fairy-cave-exploration-and-devils-tail-return.md) and
[mansion entry](./creed-mansion-entry-and-first-goliath-scene.md) retain their
separate requests/consumers. Their links do not establish a natural itinerary.

Owned paths are this primary, Battle21 consistency only in `story-content-coverage.md`
and `source-coverage.md`, and the opening/outcome owners' narrow final-CURSE and
relationship corrections. Read-only Git/worktree/open-topic checks found no
competing writer or stacked dependency. Accepted base is
`72ff32ffc5a5e65aba00a3e890052024ff7303a9`, tree
`e0ca549b70e11e4773e44b9c103c42a0c1782eae`; PR692 is unconsumed.
No code, schema, fixture, index, allowlist, counter, design or runtime frontier
changes; no simulation, acquisition, emulator, screenshot, dataset/comparator
work or implementation.

Original paths resolve under `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at this pin; the
[USA baseline](../../README.md#pinned-original-baseline) owns ROM identity.
No fresh ROM-equivalence or runtime observation is claimed. Short battle-code
paths below use `code/gameflow/battle/`. Fnnn means flag nnn; rows/templates/slots
are zero-based. Resolve `j_` aliases and `sf2enums.asm` operands at the actual
consumer; comments and enemy names do not override instructions.

Reuse [Battle17 upgrades](./battle17-initialization-and-first-control.md),
[Battle18 items](./battle18-initialization-and-first-control.md),
[corrected Battle19 HEAL](./battle19-initialization-and-first-control.md),
[Battle20 DISPEL](./battle20-initialization-and-first-control.md) and
[battle AI](./battle-ai.md), checking the selected consumers directly. Existing
[spriteset](./battle-spriteset-data.md), [routing](./battle-routing-data.md),
[global](./battle-global-data.md), [loop](./battle-loop.md) and
[cutscene](./battle-cutscenes.md) fixtures retain their boundaries:
`rowContentTracked:false` and `routeTargetsParsed:false` remain. This is not an
executable Battle21 fixture.

| Claim | Pinned declaration and actual consumer |
| --- | --- |
| Complete selection | `data/battles/spritesets/entries.asm` slot21 → `spritesets/spriteset21.asm:BattleSpriteset21`, `0x1B4950..0x1B4AA4`; `sf2macros.asm:allyCombatant/enemyCombatant/combatantAiAndItem/combatantBehavior`; `getbattlespritesetsubsection.asm`, `0x1B1630..0x1B169E`; enums. |
| Map/global/hook selection | `code/common/maps/getbattle.asm:CheckBattle`, `0x799C`; `code/gameflow/mainloop.asm`; complete `data/battles/global/{battlemapcoords,custombackgrounds,enemyleaderpresence,battleneutralentities,halvedexpearnedbattles,afterbattlepositions}.asm`; terrain21; actual before/start slots21, wrappers and `code/common/scripting/map/ms_empty.asm`. |
| Party/definitions/repeat | `battleloop_1.asm:BattleLoop`, `0x23A84`; complete `battleloop/initializecombatants.asm`, `0x1B1272..0x1B15F8`, `heallivingandimmortalallies.asm`; `code/common/stats/{battleparty,updatecombatantstats}.asm`; complete `data/stats/enemies/enemydefs.asm` records17/12/44/60/70/79; `battleloop/upgradeenemies.asm`, `0x1B1780..0x1B1A28`; `data/battles/global/randombattles.asm`, `0x1B6DB0..0x1B6DDA`. |
| Actors/regions/orders | `battlefunctions/loadBattle.asm`; `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities/DeclareNewEntity`; `battlefunctions/battlefunctions_0.asm:GetEntityIndexForCombatant`; enemy mapsprite/battlesprite tables; `battleloop/{activateenemies,triggerregions,populateenemyspawns,clearaimemory}.asm`; code/data `cutscenes/regionactivatedcutscenes.asm`; `ai/startaicontrol.asm`, `0xDEFC..0xE1AC`; `data/battles/global/aicommandsets.asm`, `0xE1AC..0xE25B`; `battlefield/getmoveorderposition.asm`, `0x1B15F8`; stats `GetMoveOrders/GetTriggerRegions`. |
| Actual commands/spells | `ai/executeaicommand.asm`; complete selected `command/{moveorder,attack,heal,support}.asm`, `move.asm`, movement/attack/healing/support helpers; `getnextuseableaiaction.asm`, `gethighestusablespelllevel.asm`; `command/heal/{doescombatantrequirehealing,calculatehealtargetpriority,determinehealingspelllevel}.asm`; `command/support/prioritizetargetsforsupportspells.asm`; `data/battles/global/aipriority.asm`; `data/stats/spells/spelldefs.asm`; `code/common/stats/spellstats.asm:GetSpellDefinitionAddress`; `code/common/tech/randomnumbergenerator.asm`. |
| Items/drop/control | `code/common/stats/itemstats.asm:AddItem/IsItemUsableInBattle/IsItemUsableByCombatant/GetItemSlotContainingIndex/RemoveItemBySlot`; `dealsinventory.asm:AddItemToDeals`, `0x99DE..0x9A3C`; `data/stats/items/itemdefs.asm`; `battleactions/{useitem,breakuseditem,dropenemyitem,battleactionsengine_1,initbattlesceneproperties,createbattlesceneanimation,castspell}.asm`; independent `data/battles/global/enemyitemdrops.asm`, `0xBE52..0xBECC`; `sf2const.asm`; `battleloop/turnorderfunctions.asm`; `battlefunctions/executeindividualturn.asm`, `0x23EB0..0x24242`. |

## Map67 and fresh versus retained state

**Confirmed:** the sole Map67 battle-map row is21, origin`(21,0)`, size32×42,
triggers255/255. CheckBattle requires F421, treating triggers as wildcards rather
than testing containment in the crop. Supplied F521 clears421 after selection
without canceling admission. F471, party and followers are not row predicates.
The crop is copied to working-layout origin; scene/formation operands are already
in that cropped domain. The opening owner retains the full coordinate join.

Before21 selects `bbcs_21` at`0x4B790`; start21 selects `ms_Empty` at`0x47A4E`,
the complete terminator. Both wrappers check F471; before does not set it, start
sets it before dispatch. Empty still uses common script termination/services.

| Supplied state | Ordered entry work |
| --- | --- |
| F88 clear/F471 clear | Mode0/F399; clear seconds; save/install Map67/Battle21/base VInts; selected before21; restore/reassert incoming map/battle; clear90–105; heal/refresh eligible allies; place actual party; initialize enemies; clear AI memory; LoadBattle(0); set471/empty start; activation/region-program/spawn/round. |
| F88 clear/F471 set | Skip both hook bodies but perform fresh reset/healing/placement/definition refresh/load and new round. F521 independently controls repeat substitution. |
| F88 set | After mode0/F399 restore seconds; clear88/AI memory/view target; LoadBattle with incoming focus; enter retained queue/cursor. No fresh installation, hooks,90–105 reset, healing/placement or new round precedes individual turns. A later sentinel requests a round. |

Terrain21 loads into shared48×48 storage, distinct from crop32×42. Background21
is OVERWORLD255 (the custom-background sentinel). Leader-presence21 is-1;
it is not the Skeleton's commandset6,
AI-priority word, scene NPC128 or a direct item-drop trigger. The outcome owner
retains the actual128 death append/sweep and ordered terminal counts. The complete
neutral table has no21 row; no pond159 or scene cast is retained as a neutral.
Half-EXP selects only1 and post-battlescene position override only5.

Saved map/battle/area, resources, orders, positions, tested cache and queue must
already agree. LoadBattle rebuilds actors but does not make this saved path a
fresh initialization; incoming21 does not repair inconsistent saved state.

## All twenty-four records and actual party refresh

**Confirmed:** header4 +24×12-byte records +4×12-byte regions +0 points =340
(`0x154`) bytes, ending exclusively at`0x1B4AA4`. All ally templates declare
HEALER1/0, NOTHING127, primary/secondary NONE255 with15/15, filler0, STARTING0.
Only their coordinates are copied into actual eligible party members:

| Template | X,Y | Template | X,Y |
| --- | --- | --- | --- |
| 0 | `(27,23)` | 6 | `(28,23)` |
| 1 | `(27,24)` | 7 | `(28,24)` |
| 2 | `(27,25)` | 8 | `(29,24)` |
| 3 | `(28,26)` | 9 | `(29,23)` |
| 4 | `(28,25)` | 10 | `(30,23)` |
| 5 | `(28,22)` | 11 | `(30,24)` |

Healing scans all30 independently of membership: living allies and immortal
Peter7/Lemon28 receive maxHP/MP; other dead allies stay dead. Intermediate
`status & 7` retains STUN/POISON/CURSE. UpdateCombatantStats then preserves
STUN/POISON and derives **final CURSE from equipped cursed items**, so an old
CURSE bit alone does not survive refresh. The before scene has already run;
this later refresh does not retroactively repair its actor/HP conditions.

Initialization sets all30 ally Xs to-1, rebuilds ascending joined/active lists
from flags0–29/32–61, then traverses BATTLE_PARTY_MEMBERS. A dead member consumes
a list/loop iteration without advancing the coordinate row. Capacity is twelve
iterations; remaining members LeaveBattleParty (active/X cleared, joined retained).
Unused coordinates after deaths do not enlarge capacity. The zero-template guard
is not a pre-loop empty-list guard. Actual IDs/AI/items/spells/behavior do not
come from the ally declarations.

Exact enums include Bowie0, Sarah1, Peter7, Rohde11, Elric13 and Lemon28;
joined/active flags0/32,7/39,11/43,13/45,28/60 are separate from421/471/521.
JoinForce's independent below12 check can leave a new member reserve; it does
not clear a supplied active bit at capacity or revive ordinary dead members.
Peter may revive without being joined/active. Scene/follower selectors do not
establish this deployed party. Automatic allies independently use set6.

Every enemy row has secondary NONE255/15, filler0 and STARTING0. Complete
differing operands are:

| Row /slot | Definition /X,Y | Set | Raw added item | Primary order /region |
| --- | --- | --- | --- | --- |
| 0 /128 | SKELETON17 `(7,23)` | ATTACKER1/6 | MEDICAL_HERB\|USABLE_BY_AI `$2000` | NONE255 /1 |
| 1 /129 | DARK_MADAM70 `(8,22)` | ATTACKER3/9 | PROTECT_RING\|EQUIPPED\|UNUSED_ITEM_DROP `$4094` | FOLLOW_ENEMY0 `$80` →128 /1 |
| 2 /130 | HIGH_PRIEST79 `(9,24)` | HEALER3/2 | NOTHING127 | FOLLOW_ENEMY0 `$80` →128 /1 |
| 3 /131 | DARK_MADAM70 `(13,18)` | ATTACKER1/6 | NOTHING127 | NONE255 /0 |
| 4 /132 | HIGH_PRIEST79 `(15,19)` | HEALER3/2 | NOTHING127 | FOLLOW_ENEMY3 `$83` →131 /0 |
| 5 /133 | ORC12 `(23,14)` | ATTACKER1/6 | NOTHING127 | NONE255 /15 |
| 6 /134 | ORC12 `(19,16)` | ATTACKER3/9 | NOTHING127 | FOLLOW_ENEMY7 `$87` →135 /15 |
| 7 /135 | SKELETON17 `(21,17)` | ATTACKER1/6 | MEDICAL_HERB\|USABLE_BY_AI `$2000` | NONE255 /15 |
| 8 /136 | HARPY44 `(15,25)` | HEALER1/0 | NOTHING127 | NONE255 /2 |
| 9 /137 | HARPY44 `(13,31)` | HEALER1/0 | NOTHING127 | NONE255 /3 |
| 10 /138 | ORC12 `(20,29)` | ATTACKER1/6 | NOTHING127 | NONE255 /15 |
| 11 /139 | ARROW_LAUNCHER60 `(26,33)` | ATTACKER1/6 | NOTHING127 | NONE255 /15 |

InitializeAllEnemies visits128–159. Declared modes0 pass the `<SPAWN_HIDDEN2`
test and initialize immediately; undeclared140–159 take hidden initialization
with HP0/X-1. LoadBattle clears field actors/lookups, then PositionBattleEntities
declares placed allies/enemies Down/standing with flying/hovering handling.
GetEntityIndexForCombatant's ordinary mapping uses actual placed allies followed
by enemies; physical cursor values depend on party. All six enemy mapsprites
use ordinary table entries. The opening's six NPCs and parked coordinates are
not battle combatants, immutable physical indices or new roster members.

## Six complete bases, gear and actual added slots

**Confirmed:** all six initialStatus values are NONE. Every named equipped item
below occupies definition slot0, remaining three slots NOTHING; Harpy has four
NOTHING items. Each spell list is explicitly padded to four:

| Definition | Level; HP/MP; base ATT/DEF/AGI/MOV | Gear /four learned slots | Mover; original AI word |
| --- | --- | --- | --- |
| Skeleton17 | 21;30/0;26/40/30/6 | Broad Sword62 equipped /NOTHING×4 | REGULAR1; PRIORITYMOD_2\|BIT5 `$2020` |
| Orc12 | 17;30/0;22/37/27/5 | Steel Lance75 equipped /NOTHING×4 | REGULAR1; PRIORITYMOD_2\|BIT5 `$2020` |
| Harpy44 | 17;32/10;39/38/29/7 | NOTHING×4 /DISPEL1, NOTHING, NOTHING, NOTHING | FLYING5; PRIORITYMOD_2\|BIT4\|BIT5 `$2030` |
| Arrow Launcher60 | 17;33/0;15/24/23/4 | Steel Arrow47 equipped /NOTHING×4 | BRASS_GUNNER4; PRIORITYMOD_2\|BIT4 `$2010` |
| Dark Madam70 | 17;29/17;19/31/29/6 | Power Stick89 equipped /FREEZE2, NOTHING, NOTHING, NOTHING | MAGE11; PRIORITYMOD_2\|BIT4 `$2010` |
| High Priest79 | 17;30/29;20/31/23/5 | Power Stick89 equipped /HEAL3, BLAST2, NOTHING, NOTHING | HEALER12; PRIORITYMOD_3 `$3000` |

Skeleton spellPower is **ENHANCED99**, despite no learned spells and MP0; all
five others are REGULAR0. No casting claim follows from that field.

| Definition | Complete resistance | Complete prowess |
| --- | --- | --- |
| Skeleton | WIND_WEAKNESS\|LIGHTNING_MINOR\|ICE_MAJOR\|FIRE_WEAKNESS\|STATUS_IMMUNITY | CRITICAL150_1IN8\|DOUBLE_1IN32\|COUNTER_1IN16 |
| Orc | 0 | CRITICAL150_1IN8\|DOUBLE_1IN32\|COUNTER_1IN16 |
| Harpy | WIND_WEAKNESS\|FIRE_WEAKNESS | CRITICAL_POISON\|DOUBLE_1IN32\|COUNTER_1IN32 |
| Launcher | LIGHTNING_WEAKNESS\|STATUS_MINOR | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN32 |
| Madam | ICE_MINOR | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN32 |
| Priest | 0 | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN32 |

InitializeEnemyStats asks repeat selection before copying the selected56 bytes,
restoresHP/MP, applies row position/type/orders/regions and combines selected
mover with row commandset. Row item handling precedes activation construction,
difficulty and UpdateCombatantStats. Activation keeps priority upper nibble;
mode0/filler0 replaces raw BIT4/5, rather than treating them as fresh activation
or AI-control. Difficulty's second BEQ makes non-SUPER apply floor(baseATT×5/4)
once, SUPER twice, before equipped bonuses. With unchanged bases/difficulty0,
currentATT is54 Skeleton,43 Orc,48 Harpy,35 Launcher,38 Madam,40 Priest.
These are stats, not damage, criticals or observed resisted effects.

| Item | Complete relevant definition |
| --- | --- |
| Medical Herb0 | equip NONE; range0–1; price10; CONSUMABLE; use HEALIN1; three NONE0 effects. |
| Protect Ring20 | equip ALL; item range0–0; price3000; RING\|RARE\|BREAKABLE; use BOOST1; DEF+5, NONE0, NONE0. |
| Broad Sword62 | HERO/BRN/BDBT/NINJ/RDBN;1–1; price1600; WEAPON; use NOTHING63; ATT+22, NONE0, NONE0. |
| Steel Lance75 | KNTE/PLDN/PGNT;1–1; price810; WEAPON; use NOTHING63; ATT+16, NONE0, NONE0. |
| Power Stick89 | MAGE/PRST/WIZ/SORC/VICR;1–1; price1050; WEAPON; use NOTHING63; ATT+15, NONE0, NONE0. |
| Steel Arrow47 | ACHR/RNGR/SNIP/BRGN/BWNT;2–2; price1270; WEAPON; use NOTHING63; ATT+17, NONE0, NONE0. |

NOTHING returns before AddItem, retaining copied gear. For each base Skeleton,
AddItem finds slot1 after equipped Broad Sword`$00BE`; `$2000 & $807F = $0000`
both stores and returns ordinary Herb0. For base Madam129, slot0 remains equipped
Power Stick`$00D9`; slot1 receives `$4094 & $807F = $0014`. The caller tests
equipped bit7 **after AddItem**; it is clear in both returned words, so no equip
search runs. Ring remains unequipped and adds no automatic DEF+5: Madam DEF31
does not become36. UNUSED_ITEM_DROP bit14 is stripped too and does not govern
the independent drop table.

Definition-copy equipment is already equipped without checking ordinary class
compatibility. Physical GetAttackRange defaults1–1 for unarmed Harpy; AI physical
station construction separately uses0–1. Launcher retains2–2. The raw row item,
copied inventory, masked addition, equipped effect and action-use spell are
separate relationships.

## F521 and conditional repeat selection

**Confirmed:**21 is eighth in the complete eleven-entry random-battle list.
IsBattleUpgradable/DetermineBattleUpgrade require F521 independently of421/471.
Clear521 returns base without upgrade RNG. F88 bypasses fresh definition copy;
this is not a per-round substitution mechanism.

Category reads the original definition mover upper nibble before copy, not the
enemy name, row commandset or eventual substitute's mover:

| Original bases | Category/range R | Inclusive bounds | Exclusions |
| --- | --- | --- | --- |
| Skeleton17/Orc12 REGULAR; Launcher60 BRASS_GUNNER | Melee5 | OOZE0..REAPER35 | KRAKEN_LEG10, PAWN13, KNIGHT14, RAT15, BURST_ROCK32, ZEON_GUARD38 |
| Madam70 MAGE | Mage1 | GALAM_MAGE67..CHAOS_WIZARD74 | QUEEN71 |
| Priest79 HEALER | Healer2 | DARK_CLERIC76..EVIL_BISHOP84 | BISHOP80 |
| Harpy44 FLYING | Airborne2 | HUGE_BAT40..DEMON52 | Count0, unsafe scan below |

Bowie's level supplies E, adding20 when signed-byte class>11. Subtract21;
nonpositive difference repeats the read once using shared D4 then returns base.
Positive difference divided by10 retains quotient low byte q; q0 returns base.
Ordinary eligibility begins at **E31**; ordinary byte-level plus optional20 gives
eligible q1..25. Unknown movement categories return base early, but these six
original bases have selected categories.

Center is base+`5q−5` for R5, otherwise base+q. GenerateRandomNumber(range R+1)
returns0..R and advances main16-bit seed `(13×seed+7) mod65536`. Signed-byte
lower/upper bounds precede exclusions. One failure consumes D4, restores center
and draws once more; a second failure returns original. No clamp or unlimited
retry is present.

| Base | q1 pre-filter domain | Later ordinary boundary |
| --- | --- | --- |
| Skeleton17 | 17..22 | q4:32..37 with32 excluded/36–37 out; q≥5 has no in-bound candidate. |
| Orc12 | 12..17 with13/14/15 excluded | q5:32..37 with32 excluded/36–37 out; q≥6 all out. |
| Launcher60 | 60..65, all above35 | q1..25 spans60..185:60..127 fail signed upper35,128..185 fail signed lower0. |
| Madam70 | 71..72 with71 excluded | q2:72..73; q3:73..74; q4:74..75 with75 out; q≥5 all out. |
| Priest79 | 80..82 with80 excluded | q2:81..83; q3:82..84; q4:83..85; q5:84..86; q≥6 all out. |
| Harpy44 | 45..47 before unsafe exclusion | Bounds precede the same unsafe scan; no installed result is asserted. |

Eligible Launcher therefore returns60 after two draws, rather than acquiring
a ranged-family substitute. Ineligible calls return60 without these draws.
Airborne's blank excludedEnemies emits count0; subtract1 then DBF scans65,536
following bytes unless a match intervenes. Match takes retry/base fallback;
exhaustion can accept. The comment does not establish safe failure. No out-of-table
byte census or natural repeated roster is claimed.

Calls occur128–139 in row order. Draws, fallback and unsafe scans can affect later
seeds and round order. A returned substitute supplies actual HP/MP/stats/mover,
gear/spells/resistance/prowess/spellPower/priority; the row retains coordinates,
commandset, orders, regions and item request. Use that copied inventory, not the
base table above: successful additions use its first NOTHING slot and strip the
same bits. Full inventory returns failure without AddItem's success-path mask;
InitializeEnemyItems does not separately check that failure before its equipped
test/search. Do not promise an added Ring/Herb or base slot1 on such a state.
In particular row135's upgrade may remove Broad Sword from the definition even
though its independent drop-table entry is unchanged.

## Four regions and the actual projection consumer

**Confirmed:** all four records have type4, byte1=0 and trailing0/0.

| Region /flag | Ordered vertices1,2,3,4 | Assigned primary slots |
| --- | --- | --- |
| 0 /90 | `(0,0),(0,40),(31,15),(31,0)` | 131/132 |
| 1 /91 | `(0,21),(9,32),(20,23),(11,13)` | 128/129/130 |
| 2 /92 | `(0,15),(0,41),(23,41),(23,15)` | 136 |
| 3 /93 | `(0,1),(0,41),(31,41),(31,37)` | 137 |

IsRegionTriggered scans all30 allies with positive signedHP and nonnegative
signed-byte X/Y; joined/active/following are not extra predicates. Type4 tries
triangles1/2/4 then3/2/4; triangle edge order is1→2,3→1,3→2. Projection chooses
horizontal on equal absolute spans, signed MULS/DIVS truncated toward zero:
`Yproj=Yb+trunc((Yb−Ya)×(Xc−Xb)/(Xb−Xa))`; vertical exchanges X/Y.
SUB.W uses unsigned borrow for side flags. Collinear/bounded returns01 and
immediately accepts; out-of-bounds returns10. Ordinary flags must match the
opposite vertex. Bit0 exceptions skip either early edge but reject at the third.
Negative projection writes CCR=1 and returns10 despite the misleading11 comment.

All12 coordinates into all8 triangles give48 region-position misses/96 triangle
substitutions. Complete first-rejection witnesses use original vertex numbers:

| Region /triangle | First rejecting edge | Ally projection/flag; opposite projection/flag |
| --- | --- | --- |
| 0 /1,2,4 | Third,4→2 | X10..13 /10; vertex1 X31 /00 |
| 0 /3,2,4 | First,3→2 | Y16..19 /10; vertex4 Y15 /00 |
| 1 /1,2,4 | Third,4→2 | X9..10 /10; vertex1 X10 /00 |
| 1 /3,2,4 | First,3→2 | Y15..18 /10; vertex4 Y31 /00 |
| 2 /1,2,4 | Third,4→2 | X13..16 /10; vertex1 X23 /00 |
| 2 /3,2,4 | Second,4→3 | X23 /10; vertex2 X23 /00 |
| 3 /1,2,4 | Second,4→1 | X18..21 /10; vertex2 X34 /00 |
| 3 /3,2,4 | Third,4→2 | Y38 /00; vertex3 Y37 /10 |

Flags compare projection to the corresponding actual coordinate (X27..30 or
Y22..26), not to the other column's projection. Region2's X0–23 rectangle
does not authorize a conventional polygon replacement: its first triangle uses
`Xproj=trunc(−23×(Y−41)/26)` on4→2; the second fails at vertical X23 versus
the allies' X27..30. Region3's second triangle has horizontally dominant4→2:
`Yproj=41+trunc(4×X/−31)` gives38 for every template, while vertex3 gives37.
These are arithmetic witnesses, not naturally observed positions.

Living fresh placements use a prefix/subset; dead ordinary members do not
advance the formation cursor and other ally Xs reset-1. No fresh90–93 hit follows.
Later/saved movement or arbitrary supplied geometry is outside this finite check.
With clear incoming tested bits the first full evaluation leaves cache15 and
persistent90–93 clear. NEWLY_TRIGGERED_BATTLE_REGIONS caches misses too; existing
tested bits can suppress evaluation. Fresh90–105 reset and ClearAiMemory do not
explicitly clear this word. Normal enemy StartAiControl clears it separately.

ActivateEnemies scans128–159 then falls through once for160 because its outer
body lacks RTS; the [alias owner](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
retains that defect. Every declared region is tested before assignments. Primary
hit sets bit0, retaining bit1, and returns; only secondary hit sets0/1. Here
secondaries are15, not a fifth region. Activation does not exchange orders.
Region programs select only32/40, not21. STARTING/undeclared fresh modes0
select no hidden or respawn wave.

Inactive129/130/132 have live follows with assigned regions: standby eligibility
returns nonzero d1 and the caller stops with STAY/empty movement. Earlier
thinking RNG may also stop them. Inactive128/131/136/137 have NONE orders and
can try local standby movement about their starting positions, subject to
memory/terrain/occupancy/RNG. Unconditional133/134/135/138/139 have15/15 and
bypass inactive standby without needing bit0. Harpy136/137 use HEALER1/0;
neither name, fullHP nor the Battle20 thresholds installs SWARM15 here.

## Live orders, complete streams and learned spells

**Confirmed:**129/130 follow live128 initially`(7,23)`,132 follows live131
`(13,18)`,134 follows live135`(21,17)`. These are combatants, not fixed points,
templates, allies or physical indices. There are zero AI points and no selected
MOVE_TO. NONE255 is handled before lookup. Active/unconditional preparation
promotes secondary NONE when the primary live referent hasHP0, leaving both NONE;
inactive standby delays this mutation. It precedes stream selection even for
sets without MOVE_ORDER. Commands stop at first d1=0 success:

| Set /slots | Complete stream | Preparatory pathfinding |
| --- | --- | --- |
| HEALER1/0 /136/137 | HEAL1, SUPPORT, ATTACK1, MOVE1, STAY | Regular |
| HEALER3/2 /130/132 | HEAL2, MOVE_ORDER4, SUPPORT, ATTACK3, MOVE2, STAY | Block non-movable |
| ATTACKER1/6 /128/131/133/135/138/139; actual automatic allies | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY | Regular |
| ATTACKER3/9 /129/134 | MOVE_ORDER3, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER4, MOVE2, STAY | Block non-movable |

MOVE_ORDER3 uses follow-type1/block-non-movable;4 uses type0/block-non-movable.
Stored bit6 selects point versus live position; type0 does not invent a point.
MOV0/NONE/dead-target guards, actual target position/terrain/occupancy, reverse
search and move-string construction remain conditions. Type0's station test
uses `tst.b` on returned X (X0 fails, -1 is nonzero); follow-type paths test
temporary obstruction. Success can attack internally or move-and-STAY, excluding
later commands; failure may still change restrictions. Sets0/6 have no order
command. Every top-level AI exit clears temporary obstructions.

Attack selection tries physical, offensive spell and item categories using
actual reachable targets/priorities/RNG/stations. FREEZE2 (MP7, range1–2,
radius1, power12) and BLAST2 (MP5,1–2,1,8) pass TYPE_ATTACK and confusion's
whitelist. Affordable lower FREEZE1/BLAST1 cost3/2; actual rank selection uses
proper shift6. Learned DISPEL1 is TYPE_SUPPORT (MP5,1–2,1,0), not offensive.
All these learned definitions are AFFECTEDBYSILENCE. No base resource fixes a
cast or damage result.

SUPPORT uses learned spells, not a Protect Ring item scan. Nonconfused enemies
can admit exactly DISPEL1 or MUDDLE2. Harpy's actual DISPEL path checks5MP,
scores area centers by units with an admitted attack spell, otherwise a healing
spell (at most one point per unit), and retains score≥2. Attack scoring uses
usability/MP while healing lookup lacks that same MP test. MOVE/CLR between
targeting BTST and branches changes CCR: the selected path marks allied
occupancy then builds ally targets. Highest priority, target/station and path
must succeed; earlier command success can prevent SUPPORT entirely. Reuse
the [full DISPEL account](./battle20-initialization-and-first-control.md#actual-spell-filters-dispel-and-candidate-local-heal),
without converting Harpy's known spell into a performed action.

Priest HEAL3 starts a new aiCommand_Heal with local spellEntry`$80`. HEAL1/2
command parameters are unused for learned rank. Muddle2 rejects healing;
ordinary HEAL needs at least3MP and living candidates with3×HP≤2×maxHP.
MissingHP M≤2 fails. HEAL levels1/2/3/4 cost3/5/10/20, ranges0–1/0–2/0–3/0–1,
radius0, powers15/15/30/255. Priorities test sets13/14, not15; selected priorities
are Priest12, Madam11, Harpy6, Launcher5, Skeleton/Orc2. FullHP bases initially
need no healing. Candidate/AOE sorting and available stations still govern.

For **current incoming local`$80`**, M3–14 checks HEAL3/key`$80` cost10 before
returning HEAL1. M≥15 checks key`$C0` HEAL4 cost20, then faulty key`$A0`
(absent ATLAS3, getter defaults to HEAL1 cost3), then`$80` if needed. WithMP≥3
this gives HEAL1 at3–19 and HEAL3 at≥20. Nonself returnedHEAL1 promotes toHEAL2
atMP≥11 only while incoming rank is still2. Self does not promote.

The caller writes selected local rank **before** station search. A failed
station advances to another candidate without rereading learnedHEAL3: incoming
`$40` withM>2 checks5MP, returnsHEAL1 atMP≥5, fails3/4, and has no nonself
promotion; a successful rank choice writes`$00` even if that station fails.
Incoming`$00` checks3MP, returnsHEAL1 and does not promote. Helper failure leaves
the prior local; failed attempts spend noMP. A new command rereads`$80` and no
learned slot is lost. Preserve [Battle19's corrected lifetime](./battle19-initialization-and-first-control.md#learned-heal3-blast2-and-freeze2),
rather than asserting HEAL3 for every later candidate at fresh29MP.

## Item selection, conditional use and independent drops

**Confirmed:** GetNextUsableHealingItem requires bit13 except Healing Rain.
Actual Herb0 at Skeleton slot1 has that bit stripped, so it does not enable AI
healing. Ring20 lacks it too and its BOOST spell is SUPPORT rather than HEAL.
The offensive item iterator tests equipped/AI permission, then TYPE_ATTACK,
and can terminate with NOTHING at an earlier item instead of continuing.
Copied equipped Broad Sword/Power Stick use NOTHING63; IsItemUsableInBattle
incorrectly compares255, so63 enters the spell getter's defaultHEAL and fails
TYPE_ATTACK. Thus the earlier equipped slot0 can end that item search before
the Herb/Ring. An unequipped item without permission also ends that search.
No automatic Herb use, Ring BOOST or fresh DEF+5 follows from row metadata.

Separately supplied player/control-selected USE_ITEM must pass its menu/target
consumers and actual held slot. IsItemUsableByCombatant permits a nonequipment
Herb; equipment requires equippable class and usable spell, not necessarily an
already equipped bit. The actual menu in `battlefunctions/battlefunctions_2.asm`
calls `battlefield/buildactionrangegrids.asm:BuildSpellRangeGridForItemUse`,
which fetches the use-spell and builds its spell range, not the item's range
bytes. It requires a target list and confirmed cursor choice. The item action's
`battlefield/populatetargetslist.asm:PopulateTargetsListForItemUse` builds the
use-spell's AOE about the selected target. UseItem fetches that same spell and
enters CastSpell. Herb HEALIN1
is MP0/range0–1/radius0/power10; Ring BOOST1 is MP2/range0–1/radius1/power15
in the spell definition (separate from Ring's item range0–0). BOOST sets status
and uses its actual refresh/effect path, rather than applying Ring's equip DEF+5.

PerformAnimation deducts MP only for CAST_SPELL action; USE_ITEM remains that
action while sharing effects, so neither item spends spell MP. The silence
property check belongs to CAST_SPELL, not this item branch. After the
target loop, BreakUsedItem removes nonequipment Herb. Breakable Ring on
an **enemy actor** exits without break/destroy RNG; on an ally, an already
broken Ring is removed, otherwise range4 RNG zero breaks it. Skipped/no-target,
curse/stun/silence early exits and unfinished services do not establish completed
use. This is conditional consumer behavior, not a natural selection observation.

The independent drop table has exactly two21 entries; enemyEntity adds128:

| Target row /combatant | Required actual held item | Once flag |
| --- | --- | --- |
| 7 /135 | BROAD_SWORD62 | 7 → dropped-flags byte0 bit7 |
| 1 /129 | PROTECT_RING20 | 8 → byte1 bit0 |

This matches battle/slot, not Skeleton/Madam type, raw UNUSED_ITEM_DROP or the
Herb on row0/7. WriteBattlesceneScript calls DropEnemyItem after target effects,
also on double/counter paths with their actual actor/target. It requires an ally
actor, enemy target, local targetDies and a matching held index found through
GetItemSlotContainingIndex's127 mask. HP0 elsewhere, a table record or leader
death sweep is insufficient; common leader cleanup does not invoke this loot
helper for every swept enemy.

Both selected62 (weapon) and20 (nonweapon Ring) branch directly to DropItem with
**no drop RNG**. Only Taros Sword110, Iron Ball102 and Counter Sword68 take the
hardcoded range32/zero random branch, before the once check. No general weapon
versus nonweapon chance exists here. The subsequent IsBattleUpgradable/range3
code is unreachable; F521 does not activate it.

DropItem sets the selected once bit with BSET and returns immediately if already
set. Otherwise it removes/compacts the target's actual slot and refreshes stats,
then tests actorHP and attempts AddItem into that actor's first free slot.
Receipt loses source equipment/metadata via the item-index path; the receiver
is not another ally chosen for capacity. Dead actor or full inventory takes
rare-deals fallback: Ring20 is RARE and AddItemToDeals increments its nibble
unless already15; Broad Sword62 is not RARE and is discarded. Once bit/removal
have already happened even if no inventory receipt or deals increase follows.

Dropped flags at`$FFF7A4` are a separate four-byte bank, not F7/F8 or F521.
Fresh initialization clears90–105 and AI memory, not these once bits; repeat
selection does not reset them. RAM lifetime is not proof of save persistence.
Upgraded row135 may not hold62; full/changed copied inventory may prevent the
row129 addition. Player/supplied Ring use/removal, independent prior flags,
actual killing actor and services affect loot. Keep the accepted victory/
scene/return at the outcome owner; static held items do not guarantee loot.

## Round generation and first conditional control

**Confirmed:** generation clears64 sentinels, scans30 allies then128–157,
admitting nonnegative-X/nonzero-HP independently of activation. Unchanged fresh
enemies contribute12 entries with orderedAGIs
`30,29,23,29,23,27,27,30,29,29,27,23`; none has bit128/extra turn.
Ordinary score is AGI +RNG(AGI>>3) −RNG(AGI>>3) +RNG(3) −1. Actual bit128 would
add floor((AGI&127)×5/6) with two draws. Range0 still advances main seed.
62 signed-byte stable-sort passes preserve append-order ties and set cursor0.
Party/gear, repeat/standby/earlier seeds affect order; no first actor is fixed.

ExecuteIndividualTurn masks identity and skipsHP0. MUDDLE/AI-bit2 chooses AI
preparation first; otherwise enemy opponent-control0 selects AI and nonzero
selects player preparation; ally auto-battle is independent. Cursor/view/window/
grid services precede SLEEP/STUN NoAction, so preparation can already mutate AI
before a status prevents action. Dispatch repeats the same control polarity
before AI execution or input. F88 may retain a changed queue/cursor. Stop at
these conditional branches before performed movement/action or usable control.

## Production consumers and conversion limits

**Confirmed by accepted implementation inspection, separately from original-game
evidence:**21 is not produced/trusted/admitted/played. The
[trust owner](../../remake/docs/content/profiles-and-trust.md#selected-third-encounter)
and [capability owner](../../remake/docs/capability-status.md) retain their
selected2/3 boundary. No production consumer is changed or executed here.

| Actual consumer | Reusable mechanism and selected21 limit |
| --- | --- |
| [prepare_encounter / compile_routes](../../src/sf2tool/remake_exploration_battles.py), [scene producer](../../src/sf2tool/remake_battle_scene_content.py) | Preparation admits2/3, scene build1/2/3. Wildcard routes,400/450/500 flags and before/load/start phases exist; no21 selection/export/route or before21 conversion is admitted. |
| [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [PrivateBattleDefinitionReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) | Complete formations, region/point arrays and definition/item/spell/resistance/prowess metadata exist; trust pins admit1/2/3. Four vertices can be described; trusted transport alone is not selected21 behavior. |
| [PrivateBattleScenarioReader.Assemble](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs), [PrivateBattleActionBindings](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Independent region-program admission1/2/3 rejects21. Sets0/6/9, STARTING, NONE/live and REGULAR/HEALER/FLYING exist; set2 rejects. Herb `$2000` and Ring `$4094` expressions reject the current exact placement allowlist; existing Herb `$6000` lowering does not admit them automatically. Skeleton ENHANCED rejects even with no spells. Harpy/Madam/Launcher BIT4 and MAGE/BRASS_GUNNER reject; Skeleton/Orc prowess and Harpy poison reject. Ordinary Priest/Madam prowess and adjacent ATT-only WEAPON metadata can fit descriptively; Arrow2–2 and equipped Ring effects do not. HEAL/Herb effects have bindings, not executable enemy healing/support/offensive-spell decisions. Arbitrary party classes beyond0/1/2/4 reject. |
| [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) | Membership rebuild, carried resources/seeds, formation/living-immortal healing, difficulty0 ATT and activation exist. Excess active count rejects instead of eviction; general gear/final-CURSE refresh is limited. Enemy equipment rejects loadout0/127 admission, including copied Sword/Stick/Lance/Arrow and added Ring. No F521 level/substitution/unsafe exclusion path or full-inventory equip behavior is implemented. F88 rejects instead of restoring queue; other difficulty/control and actual party producers remain limits. |
| [BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs) | Four-vertex regions, primary-first activation,15/15, STARTING/RegionProgram.None and ordinary/extra randomized turns exist. Geometry uses cross-products rather than the original signed projection/CCR; fresh tested-state lifecycle differs. Matching these fresh misses alone would not prove equivalence. No21 observation validates this join. |
| [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs), [SourceMoveOrderAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) | Standby, live-follow/dead-target promotion,6/7/9/14 and Flying movement/priority are reusable; no selected point or SWARM platform is needed. Activated AI rejects0/2, automatic allies, nonzero status and loadouts beyond0/127/no learned spells. Exact0/2 streams, candidate-local HEAL3, DISPEL area scoring and offensive spell/item selection remain gaps. |
| [Physical action](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2PhysicalAction.cs), Application/entity services and adapter boundary | Existing physical/control/reward primitives do not bind21's independent slot135/129 held62/20, once7/8, removal/receipt/deals, Ring BOOST/break versus Herb consumption, full resistance/prowess or original services/presentation. No21 content/adapter, natural party, suspend or usable-control acceptance follows from generic projection. Opening/outcome actor and pending-request conversions remain at their owners. |

These limits call for scoped reuse/conversion decisions, not admission-only
edits or new infrastructure. Descriptions, trust, assembled capability and
played behavior remain distinct.

## Unknowns, retained failures and reproduction

**Unknown:** natural flags/route/party/classes/gear/status/seeds, actual repeated
definitions/inventory, later regions/standby/targets/paths, HEAL/DISPEL/item/attack
choices, death actor/loot recipient/deals, first actor/actions, prowess effects,
saved validity, completed map/entity/camera/text/animation/input services,
presentation/hardware timing and persistence. These create no automatic runtime
queue under ADR0014/0016.

Preserve #1047's twelve **completed unresolved comparisons** at the
[verification owner](../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03),
the [control/summary corrections](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415),
intermediate/final-CURSE, [Battle12 HEAL/identity](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1234),
[Battle13 neutral](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1236)
and Battle16/18/20 findings/Unknowns. PR1245's initial`16f4233a` mechanical PASS
missed HEAL's candidate-local lifetime; its
[independent P2](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1245#issuecomment-6366643680)
and corrected`c1893c7b` remain accepted evidence. No completed failure becomes
interrupted or passed here.

Reproduce from the pin/base: read all24 records, four regions and six full
definitions; resolve selected map/global/hooks/macros/enums, then their actual
callers. Substitute the12 coordinate pairs into triangles1/2/4 and3/2/4 using
the projection/CCR rules and rejection table above (96 substitutions). Follow
tested-cache/flags/standby/dead-target/command ordering. Re-derive E31/categories/
q/bounds/exclusions/retry, then track the selected copy and actual inventory
before item decisions. Resolve `$2000/$4094` through807F, add/equip order, earlier
item termination, learned SUPPORT and candidate-local HEAL. For drops inspect
actual ally actor/enemy death/held index, reachable RNG branches, once7/8,
removal/capacity/deals and independent leader sweep. Inspect current production
rejections independently of original declarations.

Bounded reads and small direct receipts under ignored `local/issue1248/` are
linear in selected text/records, budgeted below64MiB/one minute for check work;
peak memory is not measured. Geometry is96 substitutions, not simulation or
fixed-point analysis. Retained18,285,979-byte pair and pair.txt are checked only
by existing length/mtime metadata against `local/issue1230/retained-before.json`;
no payload is parsed, copied or regenerated. Reassess material scope/resource
growth before expanding.

Direct checks cover UTF-8/LF, links/anchors/fences/tables, private boundary,
exact five paths, unchanged fixture metrics/four frontiers and retained metadata.
On a clean committed candidate load `. ./local/private-inputs.ps1` in the same
process and run `uv run sf2 verify plan --base origin/main --head HEAD`, then
exact-head scoped CI. Docs-only policy controls generic suite suggestions:
no normal/full/SDK/Godot/H3/H4 or historical comparison reruns. Actual commands,
inspection failures, Git objects and stopped processes belong in the Issue/PR
handoff. Stop at clean pushed frozen Draft PR, Project Review / Verify and
successful main-gate notification; no merge/cleanup/archive/next task here.
