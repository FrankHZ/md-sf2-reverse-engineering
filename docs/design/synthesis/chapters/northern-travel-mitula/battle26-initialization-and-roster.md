# Battle26 Initial Formation and Neutral Higins

**Confirmed:** after the Geshp confrontation's before wrapper returns, fresh
Battle26 entry heals eligible allies, places the actual active force and
fifteen enemies, replaces the scene population and loads the battlefield.
Completion-clear loading adds a separate neutral Higins actor. His appearance
does not recruit ally19. **Inferred:** this formation stages the encounter
outside Ketto. **Unknown:** natural arrival, supplied state, completed services,
visible/audible presentation, usable control and persistence.

This English guide continues [Battle26 admission and the Geshp confrontation](battle26-admission-and-geshp-confrontation.md),
using the complete accepted [initialization research](../../../../research/chapters/northern-travel-mitula/battle26-initialization-and-roster.md)
under [#1010](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1010)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
It ends at `BattleLoop:@Start`, immediately **before the first ActivateEnemies**.
It establishes no activated cast, first actor, movement, damage, drops, outcome
or Higins recruitment. The F88-supplied continuation is a separate contrast,
ending before its individual-turn loop is read or executed.

Fnnn denotes flag nnn; IDs and coordinates below are source operands or
conditional initializer results. They are not runtime observations. Source
identities and paths resolve through the research owner's
[agreement and provenance](../../../../research/chapters/northern-travel-mitula/battle26-initialization-and-roster.md#agreement-and-provenance):
USA retail baseline and SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`.
This synthesis consumes accepted Layer A; it adds no original evidence or remake rule.

## From the Confrontation to Fresh Initialization

**Confirmed:** the predecessor ends with the before wrapper restoring d1 and
returning. It has not healed the party, built combatants, set intro476 or joined
Higins. The caller has already cleared PLAYER_TYPE, set F399 and selected the
F88-clear branch. After before returns, the ordered suffix is:

1. Restore the saved incoming map/battle words and reassign their low bytes to
   current map72/battle26. This does not restore or recompute the battle crop.
2. Clear region flags90–105, including the three configured regions' F90–92.
3. Run HealLivingAndImmortalAllies, InitializeAllAlliesBattlePositions,
   InitializeAllEnemiesBattlePositions and ClearAiMemory, in that order.
4. Clear d0.w to zero and call LoadBattle for combatant0.
5. ExecuteBattleStartCutscene, then reach `@Start` before ActivateEnemies.

The [ordered suffix owner](../../../../research/chapters/northern-travel-mitula/battle26-initialization-and-roster.md#return-from-before-and-ordered-fresh-initialization)
retains the selected BattleLoop source and address. Unlock426, intro476,
completion526 and continuation88 have independent roles. Admission can select26
only after earlier Map72 candidates408/424 fail; completion-held cleanup clears
426 but still admits that selected call. Initialization does not repeat that
selection. Held476 skips before/start dispatch, while the fresh reset, healing,
rosters and load still run. Each step requires prior services to return;
interruption preserves completed prefix writes without rollback.

## Who Receives Healing and a Starting Position

**Confirmed:** healing visits all thirty ally slots, independently of force
membership. Living allies, and always Peter7/Lemon28, receive their maximum HP
and MP as current values. Other dead allies are skipped. The service masks
status to STUN, POISON and CURSE (`$0007`), then UpdateCombatantStats rebuilds
current stats with status and equipment effects and recomputes curse. The
intermediate mask does not guarantee final curse retention. No inventory,
class, membership or promotion is replaced. HP is word-sized; MP and positions
are bytes, even where a call supplies a word register.

All thirty ally X coordinates are first set to -1. UpdateForce rebuilds joined,
active and reserve lists from actual flags in ascending ally-ID order. The
placement table therefore supplies **ordinals in the rebuilt active list**,
not twelve named characters. Only a living processed member receives the next
X/Y row. Its placement row's AI/item fields do not overwrite the ally's state.

All twelve starting positions are battle-local tiles:

| Ordinal | X/Y | Ordinal | X/Y |
| --- | --- | --- | --- |
| 0 | `(7,25)` | 6 | `(4,26)` |
| 1 | `(6,25)` | 7 | `(6,27)` |
| 2 | `(5,25)` | 8 | `(5,27)` |
| 3 | `(4,25)` | 9 | `(4,27)` |
| 4 | `(6,26)` | 10 | `(7,26)` |
| 5 | `(5,26)` | 11 | `(7,27)` |

A dead listed member consumes one of the twelve processed-member slots but
does not advance the placement pointer. A later living member uses the next
unconsumed position. Fewer eligible members produce fewer actors. Surplus
active members go through LeaveBattleParty, clearing their active flag and X;
that helper does not immediately rebuild the list/count. An inconsistent zero
member count is decremented before the first read without an empty-list guard,
so arbitrary empty state is not established safe.

Changed class, HP, equipment and status affect eligibility, stats and sprites.
A stale supplied active list is not authoritative, and twelve rows do not imply
twelve living actors. If supplied joined/active flags already include Higins19,
he is processed independently as an ally; the later neutral actor does not add
him. The [actual ally consumer review](../../../../research/chapters/northern-travel-mitula/battle26-initialization-and-roster.md#actual-allies-healing-and-formation)
and [party/roster contract](../../../contracts/party/party-roster-state.md) retain the
membership, handler and persistence boundaries.

## The Fifteen Configured Enemies

**Confirmed:** BattleSpriteset26 declares **12 ally placements, 15 enemies,
3 regions and 1 point**. The initial unupgraded composition is one Lizardman,
one Wizard, two Dark Bishops, four Lesser Demons, four Dark Soldiers and three
Dark Snipers. The complete [row owner](../../../../research/chapters/northern-travel-mitula/battle26-initialization-and-roster.md#complete-spriteset-and-fifteen-enemy-rows)
resolves slot26, the 366-byte extent, all subsection offsets and macro widths.
This guide retains the meaningful rows rather than duplicating the encoding layout.

Enemy row r initializes combatant128+r. Every row uses STARTING=0 and filler0;
all row items are NOTHING except row2's encoded Black Ring. Region15 means
no trigger, not a fifteenth polygon. NONE and FOLLOW_ENEMY are configured
orders, not evidence of waiting, pursuit or an executed tactic.

| Row / combatant | Type / definition ID | X/Y | Commandset | Primary order / region | Secondary order / region |
| --- | --- | --- | --- | --- | --- |
| 0 / 128 | Lizardman / 19 | `(22,7)` | LEADER | MOVE_TO\|0 / 15 | NONE / 15 |
| 1 / 129 | Wizard / 72 | `(21,8)` | ATTACKER3 | FOLLOW_ENEMY\|0 / 15 | NONE / 15 |
| 2 / 130 | Dark Bishop / 81 | `(24,7)` | HEALER3 | FOLLOW_ENEMY\|0 / 15 | NONE / 15 |
| 3 / 131 | Lesser Demon / 45 | `(17,3)` | HEALER3 | FOLLOW_ENEMY\|0 / 1 | NONE / 1 |
| 4 / 132 | Dark Soldier / 18 | `(22,9)` | ATTACKER1 | NONE / 1 | NONE / 15 |
| 5 / 133 | Dark Sniper / 62 | `(23,11)` | ATTACKER1 | NONE / 1 | NONE / 15 |
| 6 / 134 | Dark Sniper / 62 | `(15,12)` | ATTACKER3 | FOLLOW_ENEMY\|7 / 2 | NONE / 2 |
| 7 / 135 | Dark Soldier / 18 | `(17,13)` | ATTACKER1 | NONE / 2 | NONE / 15 |
| 8 / 136 | Lesser Demon / 45 | `(4,6)` | ATTACKER1 | NONE / 2 | NONE / 15 |
| 9 / 137 | Dark Soldier / 18 | `(24,17)` | ATTACKER3 | FOLLOW_ENEMY\|10 / 0 | NONE / 0 |
| 10 / 138 | Dark Soldier / 18 | `(23,18)` | ATTACKER1 | NONE / 0 | NONE / 15 |
| 11 / 139 | Lesser Demon / 45 | `(24,27)` | ATTACKER1 | NONE / 0 | NONE / 15 |
| 12 / 140 | Lesser Demon / 45 | `(9,19)` | ATTACKER1 | NONE / 15 | NONE / 15 |
| 13 / 141 | Dark Bishop / 81 | `(13,20)` | HEALER4 | FOLLOW_ENEMY\|14 / 15 | FOLLOW_ENEMY\|12 / 15 |
| 14 / 142 | Dark Sniper / 62 | `(14,23)` | ATTACKER1 | NONE / 15 | NONE / 15 |

Initialization scans enemy slots128–159. These fifteen rows take the
InitializeEnemyStats branch: resolve the type, copy its 56-byte definition,
set current HP/MP from maxima, write position, pack movetype/commandset,
store type/triggers/orders and process the row item. Slots143–159 receive
activation0, maximum/current HP0 and X=-1. This is a partial absent-slot reset;
it does not clear every Y/MP/inventory field. Its trigger reads still use row0's
15/15 because the subsection pointer is unchanged. No extra enemy rows follow.
The Battle32/Jaro exception does not apply; load later repurposes159 as neutral.

The activation write keeps definition bits `$F000` and combines the rotated,
masked initialization word. For these unupgraded definitions and zero spawn
words, Bishops receive `$3000` and the other rows `$2000`. Definition BIT4/BIT5
are discarded; primary-active, secondary-active, neutral and spawn-mode bits
are clear. This is initialization of activation storage. The first activation
routine has not run.

### Definitions, Equipment and Initial Current Stats

**Confirmed:** with completion526 clear, the selected definitions have these
values. HP/MP maxima also become fresh current HP/MP. Each definition has four
item slots and four spell slots: only its first item is populated below; unlisted item and
spell slots are NOTHING. ATT/DEF/AGI/MOV here are copied **base** values before
difficulty/equipment processing.

| Type | Level | HP / MP | Base ATT / DEF / AGI / MOV | Movetype | Copied first item | Spells |
| --- | --- | --- | --- | --- | --- | --- |
| Dark Soldier18 | 23 | 35 / 17 | 31 / 46 / 34 / 4 | REGULAR1 | equipped Large Axe | ATTACK |
| Lizardman19 | 24 | 40 / 0 | 34 / 45 / 38 / 6 | REGULAR1 | equipped Large Axe | none |
| Lesser Demon45 | 22 | 40 / 21 | 49 / 37 / 32 / 6 | FLYING5 | NOTHING | BLAZE3, BOOST2 |
| Dark Sniper62 | 24 | 38 / 0 | 11 / 37 / 35 / 5 | STEALTH_ARCHER10 | equipped Hyper Cannon | none |
| Wizard72 | 26 | 37 / 37 | 32 / 40 / 44 / 5 | MAGE11 | equipped Flail | BLAZE3 |
| Dark Bishop81 | 23 | 39 / 38 | 28 / 38 / 27 / 5 | HEALER12 | equipped Flail | AURA, BLAST3 |

All six have initial status NONE and ENHANCED spell power. Lizardman has
ICE_WEAKNESS, Demon STATUS_MINOR and Wizard FIRE_MINOR resistance fields;
others have zero. Prowess fields retain critical150 chance1in32, double1in32
and counter1in16 for Soldier/Demon; Lizardman changes critical to1in8, Sniper
double to1in16 and Wizard/Bishop counter to1in32. Priority fields are
PRIORITYMOD_2 except Bishop's PRIORITYMOD_3. These remain copied fields,
without a damage, spell, initiative or balance conclusion.

The actual difficulty routine has consecutive `beq` branches after its first
SUPER comparison. SUPER branches to Continue; every other value falls through
the second untaken branch to Continue too. Consequently NORMAL0, HARD1 and
OUCH3 all apply floor(baseATT*5/4) once; SUPER2 applies it twice. The label
alone must not be used to claim that only SUPER receives an increase.
UpdateCombatantStats then copies base values into current stats and applies
equipped items. Large Axe adds 25 ATT, Hyper Cannon 40 and Flail 19. Current
DEF/AGI/MOV equal the table because these initial status/equipment definitions
add no relevant effect. Conditional ATT results are:

| Type | NORMAL / HARD / OUCH adjusted base ATT | Current ATT | SUPER adjusted base / current ATT |
| --- | --- | --- | --- |
| Dark Soldier | 38 | 63 | 47 / 72 |
| Lizardman | 42 | 67 | 52 / 77 |
| Lesser Demon | 61 | 61 | 76 / 76 |
| Dark Sniper | 13 | 53 | 16 / 56 |
| Wizard | 40 | 59 | 50 / 69 |
| Dark Bishop, both rows | 35 | 54 | 43 / 62 |

These are initialization results under the unupgraded premises, not immutable
stats for later turns. The [definition/stat owner](../../../../research/chapters/northern-travel-mitula/battle26-initialization-and-roster.md#selected-definitions-equipment-and-initialized-stats)
names the exact enemy/item definitions, stat setters and difficulty consumer.

### Why the Black Ring Does Not Add Attack

**Confirmed:** row2 encodes BLACK_RING with EQUIPPED, USABLE_BY_AI and
UNUSED_ITEM_DROP, giving `$6098`. The copied Bishop definition already has an
equipped Flail in slot0 and an empty slot1. On successful AddItem, the consumer
masks **both the stored value and d1** with `$807F`, inserting `$0018` in slot1.
The caller then tests that returned d1's equipped bit; it is clear, so
EquipItemBySlot is not called.

The resulting four items are `$00DA,$0018,$007F,$007F`: equipped Flail,
unequipped Black Ring, NOTHING, NOTHING. The encoded AI-use/drop bits have
also been stripped. The ring's declared +10 ATT does not contribute to the
initialized ATT above. BLAZE2 remains its definition's use-spell field; no later
use/drop/reward is established. Other rows' exact NOTHING operand returns before AddItem and
leaves copied equipment intact. A modified full definition inventory takes
AddItem's failure path, which the caller does not check. Successful insertion
is not a guarantee of normalization on that different branch.

### Completion Can Change the Returned Definitions

**Confirmed:** OUTSIDE_KETTO26 belongs to the eleven-entry random-battle list.
DetermineBattleUpgrade tests completion526: clear keeps the declared types;
held permits UpgradeRandomBattleEnemies before definition copy. This is
independent of intro476 and of526's separate neutral-population suppression.
A repeated intro is not the condition for upgrading enemies.

The helper selects melee for Soldier/Lizardman, airborne for Demon, ranged
for Sniper, mage for Wizard and healer for Bishop. It uses Bowie0's level,
adds20 for class>11, subtracts battle26 and requires a positive difference of
at least10. Smaller differences retain the original type. Category range
parameters5/2/2/1/2, RNG, bounds, exclusions and retry/fallback branches govern
the returned definition. Its stats, inventory, movetype, sprite and priority
can change; the Battle26 row's coordinates, commands, regions, orders and
item operand stay selected, and difficulty still follows the definition copy.

The airborne exclusion list is empty, but its consumer decrements the zero
count and enters a scan without a zero-length guard. No safe guaranteed
upgraded Demon follows from high level or the category label. This retained
uncertainty belongs to the [global-data upgrade owner](../../../../research/battle/battle-global-data.md#semantics-and-runtime-queue),
with the [selected Battle26 upgrade review](../../../../research/chapters/northern-travel-mitula/battle26-initialization-and-roster.md#completion-held-upgrade-alternative).
It does not justify a broader upgrade/combat audit or a deterministic repeat roster.

## Regions and a Destination Point Configure Later Work

**Confirmed:** the three battle-local regions are:

| Region / flag | Type | Vertices 1, 2, 3, 4 |
| --- | --- | --- |
| 0 / F90 | 4 | `(21,0)`, `(12,31)`, `(35,31)`, `(35,0)` |
| 1 / F91 | 3 | `(4,0)`, `(34,31)`, `(34,0)`, `(0,0)` |
| 2 / F92 | 4 | `(0,0)`, `(0,21)`, `(35,21)`, `(35,0)` |

Type3 uses vertices1/2/3; the other arm uses triangles1/2/4 and3/2/4.
Region1's fourth pair is not a geometry operand there. Each record's second
and trailing bytes are zero and are not geometry operands on this selected
path. Fresh reset clears the associated flags, not already-fired events.
Later region/activation work is outside this endpoint.

Point0 is `(23,7)`. Lizardman's MOVE_TO\|0 references that point through the
order's low nibble. FOLLOW_ENEMY operands0/7/10/14/12 refer to combatants
128/135/138/142/140 respectively. For example, row13 references rows14 and12,
not ally14 or ally12. These are configured point/combatant references; they
establish neither a selected tactical target nor completed movement. The
[region/point consumer owner](../../../../research/chapters/northern-travel-mitula/battle26-initialization-and-roster.md#three-regions-and-one-point)
retains record widths, units and selected read paths.

## Replacing the Scene with Physical Battle Actors

**Confirmed:** LoadBattle preserves its combatant argument, selects current
map72, requests fade-out, disables view tracking, loads tilesets, waits for
fade completion, clears VInts, waits one VInt and calls PositionBattleEntities.
ClearEntities resets 49 physical records and 64 lookup bytes, then walking
allocation state. It replaces the confrontation actors, their actions and
follower aliases. Scene NPC128 was Geshp; fresh combatant128 is Lizardman.
Reusing the number proves neither transformation nor defeat.

Ordinary allocation scans all 30 ally and 32 enemy slots. Nonnegative signed
X/Y allows declaration; a neutral bit excludes an enemy from this ordinary
pass. There is **no additional HP test**. Fresh initialization establishes the
usual HP/position relationship, while supplied continuation need not. Position
bytes become `(X & $3F)*384,(Y & $3F)*384`; actors start Down with eas_Standing.
Flying/hovering movetype selects layer1, others layer0. Declaration initializes
position/destination, sprite, action, velocity/travel/acceleration, flags and
counters; installed actions are not proof of their completed presentation.

Actual class governs ally sprite selection, including the Rohde exception;
battle mode does not use the exploration dead-ally BLUE_FLAME branch. Stored
enemy type selects the sprite. Unupgraded Soldier82, Lizardman83, Demon109,
Sniper126, Wizard136 and Bishop145 are below the special-sprite threshold240
and allocate sequentially. If N allies were declared, row r uses physical
N+r: with twelve allies, enemies occupy physical12–26. Missing/special/changed
slots require their actual branches instead of this conditional mapping.

Enemy IDs128–159 resolve to lookup offsets32–63, while script aliases A30/B31
are separate entries. Skipped ordinary lookups remain -1. The 49 physical reset
records and fixed scans are not an allocation guard; DeclareNewEntity has no
capacity check. Ordinary fresh entry uses at most 12+15+1=28 actors. Arbitrary
supplied positions cannot be promised safe for all 62 scanned combatant slots.
The [physical-entity owner](../../../../research/chapters/northern-travel-mitula/battle26-initialization-and-roster.md#battle-load-and-replacement-of-physical-entities)
retains exact fields, sentinel and sprite/lookup consumers.

### Neutral Higins Has Three Different Identities

**Confirmed:** after ordinary allocation, clear completion526 admits the
neutral table's sole OUTSIDE_KETTO row: `(18,14)`, **Down**, HIGINS_PROMO48,
eas_LyingLeft. Held526 skips the neutral scan. This fresh declaration does
not preserve the scene actor19.

| Identity | Fresh completion-clear result |
| --- | --- |
| Combatant state159 | Maximum/current HP0, status0, X18/Y14, neutral activation `$0008`; other fields are not all rewritten. |
| Physical actor | Next allocation, conditionally N+15 (27 for twelve allies), layer0 with the table's sprite/facing/action. |
| Script lookup B31 | Receives that physical index; combatant159's ordinary lookup offset63 remains -1. |

The neutral writer resets its lookup pointer to the enemy-list start and
predecrements once: the actual destination is offset31, B, rather than
combatant159's offset63. GetEntityIndexForCombatant(159) therefore does not
recover the neutral actor. A30 remains clear0; generic C158/D159 enemy aliases
do not become allies. State159, physical actor and script B31 must remain
distinct even though the visible character is named Higins.

No JoinForce, ally19 stat copy, ally healing or joined/active flag write occurs.
The HP0 neutral declaration does not prove Higins has died, joined, or changed
allegiance. eas_LyingLeft requests orientLeft/updateSprite, falls through
motion OFF and reaches eas_Idle. Left orientation differs from the initial
Down facing; action installation is not an observed lying pose. LoadBattle
reinstalls VInts later, so actions may progress before the start hook, but the
precise reached frame/action is Unknown.

With526 held, fresh159 stays absent with HP0/X=-1/activation0; B31 stays0 and
no neutral actor is added. With F88 supplied, a neutral-marked159 is skipped
by the ordinary pass before clear526 rewrites it. An inconsistent nonneutral159
with valid positions can allocate an ordinary actor first and then have its
state repurposed by the neutral pass, without that earlier declaration being
removed. No universal one-to-one mapping or supplied-state repair follows.
The [neutral row and lookup proof](../../../../research/chapters/northern-travel-mitula/battle26-initialization-and-roster.md#neutral-higins-slot159-script-b31-physical-n15)
is the owner for the table tuple and exact writes.

## The Battlefield Crop, View, Terrain and Music

**Confirmed:** row26 selects Map72 with crop origin `(28,0)`, size36x32 and
wildcard trigger coordinates255/255. This crop is separate from the map-area
descriptor and terrain. LoadBattle does not rerun CheckBattle; reassigning72/26
after the scene cannot repair changed BATTLE_AREA fields.

LoadBattle restores d0=0, resolves combatant0, writes VIEW_TARGET_ENTITY and
samples its physical tile position into BATTLE_ENTITY_CHOSEN_X/Y. A negative
lookup falls back to physical0 for coordinate reads, without creating Bowie
or repairing membership. It initializes sprites and calls LoadMap with
**d0=-1** for target-based view placement and preserved **d1=72** for a full
load. Negative d1 would mean the separate reload route; the arguments have
different purposes.

Map72 selects palette 0 and tilesets 106/102/107/108/105. Its sole descriptor
covers `(0,0)` to `(63,63)`, with foreground/background starts 0/0, parallax
pairs 256/256, zero autoscroll, layer type 254 and MUSIC_MAIN_THEME1. Descriptor
selection tests target internal coordinates against bounds scaled by 384 without
adding crop 28/0; fallback selects the first descriptor, also the only one here.
The [map/crop source owner](../../../../research/chapters/northern-travel-mitula/battle26-initialization-and-roster.md#map72-crop-descriptor-terrain-music-and-view)
retains the full source tuple, extent and exact load consumers.

Base blocks/layout receive flag copies and opened-chest overlays, then the
supplied battle rectangle is copied to working origin0/0. This crop copies
1152 words/2304 bytes through 64-wide word rows; it does not establish a new
36-wide storage stride. The battle area uses layer1 origin0/0 and endpoint
`(35*384,31*384)=(13440,11904)`, while remaining descriptor fields still apply.
All formation, enemy, neutral, region and point coordinates are already local;
initializers do not subtract28. Their source-layout correspondence is
`(localX+28,localY)`: placement0→`(35,25)`, Lizardman→`(50,7)`, point0→`(51,7)`
and neutral Higins→`(46,14)`. These relations establish no legal walking path.

After map load, LoadBattle waits a VInt, loads entity mapsprites, installs base
VInts, decompresses slot26's BattleTerrain26 into BATTLE_TERRAIN_ARRAY, then
requests map music and fade-in. The terrain pointer selects the existing
private payload; this guide adds no terrain corpus or movement comparison.
PlayMapMusic starts with descriptor music1, MAIN_THEME, which matches none of
its battle-substitution cases. It therefore submits1, without a presumed
battle-theme substitution. Battle44's timer branch does not apply. Ordered
load/fade/audio requests do not establish rendered output, audible timing,
completed asynchronous services or usable player input.

## The Empty Start Hook and Supplied Continuation

**Confirmed:** slot26 of the 48-entry start table selects ms_Empty, whose whole
body is the `$FFFF` terminator. The start wrapper checks476; when clear it
**sets476 before** saving d0/a0 and dispatching ExecuteMapScript. After the
interpreter returns it restores d0/a0, and the tail restores d1 and returns.
Held476 skips this dispatch and flag write, not the prior fresh setup.

Empty dispatch still saves interpreter state, initializes its work pointer
and clears text skip. The terminator conditionally waits for scrolling if a
dialogue window exists, clears VIEW_SCROLLING_SPEED and restores registers.
There is no local text, movement, choice, join or map load. A stalled shared
tail can leave476 set before return. Only completed wrapper return reaches
the fresh endpoint immediately before ActivateEnemies. The
[start/continuation owner](../../../../research/chapters/northern-travel-mitula/battle26-initialization-and-roster.md#empty-start-hook-supplied-continuation-and-changed-premises)
retains exact table/target addresses and the caller branches.

With F88 held, BattleLoop instead restores saved seconds, clears88 and AI
memory, clears VIEW_TARGET_ENTITY and calls LoadBattle without explicitly
clearing d0. It branches to `@IndividualTurns_Loop`; the following branch to
`@Start` is unreachable. Both AI clears write 48 last-target bytes255 and 48
memory bytes0. This supplied route does not assign incoming72/26, reset region
flags, heal/rebuild the force, initialize enemies or execute either hook.
Current map/battle/crop, HP/status/items, positions, activation and turn state
are supplied, subject to LoadBattle's entity rebuild and neutral writes.
Neither byte-for-byte preservation nor fresh-state repair is established.

| Changed state or interruption | Consequence that must remain visible |
| --- | --- |
| 426 absent, or earlier408/424 admitted | No unconditional Ketto-to-Battle26 initialization. |
| 476 clear / held | Hook dispatch changes; fresh reset/healing/formation/load still follows before return. |
| 526 clear / held | Base definitions and neutral versus possible upgrades and neutral suppression; independent of476. |
| Dead/fewer/surplus/empty or changed joined/active members | Actual sorted membership, processed-member limit and HP checks control placements; no fixed named twelve. |
| Changed class, items, status, maxima, Bowie level or RNG | Actual stat/sprite/upgrade consumers differ; fixed tables retain their stated premises. |
| Missing Bowie/lookup, changed positions or crop | View fallback and actual mappings/crop apply without membership/admission repair. |
| Changed159/neutral bit, completion or F88 | Ordinary allocation and neutral rewrite differ; state159 and B31 do not share a lookup. |
| F88 with inconsistent current/saved map/battle/turn | Incoming72/26 does not force fresh setup or first-round state. |
| Interrupted healing, load, neutral declaration, fade or start tail | Completed prefix writes remain;476 can be held before start returns. |

The endpoint establishes conditional initial state and a completed fresh start
wrapper, without activation execution or a control grant. Its continuation,
[Battle26 activation and the first turn handoff](battle26-activation-and-first-turn-dispatch.md),
consumes accepted #1009 from ActivateEnemies through the first individual-turn
call. It separates region flags, spawn modes and turn eligibility, including
unchanged fresh no-trigger/no-spawn and conditional deals-storage effects.
The actual actor is not guaranteed. Performed turns, outcomes, afterbattle
dialogue, Higins recruitment and later exploration remain outside this guide.

## Evidence Review and Retained Results

The complete accepted #1005 research and its changed-state matrix were reviewed,
including all 12 placement rows, all 15 enemy rows, six definitions/current-stat
results, successful item masking versus failure, upgrade uncertainty, all three
regions/one point, neutral159/B31 and map/start arguments. The accepted #1006
confrontation guide supplies continuity, without making scene actors combatants.
The [encounter contract](../../../contracts/battle/battle-encounter-definition.md),
[battle lifecycle contract](../../../contracts/battle/battle-control-lifecycle.md) and
[party/roster contract](../../../contracts/party/party-roster-state.md) retain their broader
unclosed behavior. Selected public fixtures, their schemas and maintained
consumers were also inspected:

| Surface / exact fixture ID | Reviewed proof and limit |
| --- | --- |
| `sf2-battle-spriteset-data-static-v1` | Header/pointer/source-identity structure; rowContentTracked=false cannot prove the27 row bodies, neutral tuple, regions or point. |
| `sf2-battle-global-data-static-v1` | Public factSummary is not private facts/Map72 rows; neutral group counts prove neither the Higins tuple nor B31 writes. |
| `sf2-battle-loop-static-v1` | Thirty/32 slots, healing/status mask,48-byte AI resets and terrain pointer units; not a fresh battle roster observation. |
| `sf2-battle-control-static-v1` | F88, region reset, twelve-byte entries, difficulty flag selection, VInt/music control facts; not runtime completion or encounter intent. |
| `sf2-battle-cutscenes-static-v1` | Shared before check/no-set and start set-before-script semantics; not selected slot26's target by itself. |
| `sf2-battle-routing-data-static-v1` | Terrain selection/aliases and route structure; routeTargetsParsed=false cannot establish ms_Empty. |
| `sf2-map-init-static-v1`, `sf2-map-script-engine-static-v1` | Selected load/population/placement/action/interpreter consumers and scalar units; not a natural cast, completed action, rendered pose or control. |

Maintained fixture/schema consumers are
[spriteset data](../../../../../src/sf2tool/h2/battle_spriteset_data.py),
[global data](../../../../../src/sf2tool/h2/battle_global_data.py),
[battle loop](../../../../../src/sf2tool/h2/battle_loop.py),
[battle control](../../../../../src/sf2tool/h2/battle_control.py),
[cutscenes](../../../../../src/sf2tool/h2/battle_cutscenes.py),
[routing](../../../../../src/sf2tool/h2/battle_routing_data.py),
[map init](../../../../../src/sf2tool/h2/map_init.py) and
[map script](../../../../../src/sf2tool/h2/map_script_engine.py). Their owning commands
are `uv run sf2 h2` with `battle-spriteset-data`, `battle-global-data`,
`battle-loop`, `battle-control`, `battle-cutscenes`, `battle-routing-data`,
`map-init` and `map-script-engine`. Schema/public review here is **not a fresh
comparison** of private source, rows, neutral tuple or targets. Source/pair
absence in the design environment remains accepted; no acquisition,
regeneration, runtime, environment change or new maintained machinery follows.
No in-scope contradiction was found; a source disagreement returns to research.

[#1005 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1005#issuecomment-6181975872)
and [PR1008 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1008#issuecomment-6181990706)
retain head `033ebbdd`, tree `2daf762b`, base `f1958d33`, CI38663398489
(scope14s/research42s), original CI38663279580, the0.083s retained-pair check
and full source/data/consumer joins. The corrected scene Higins ID, neutral
membership-versus-tuple proof, inspect.py standard-module shadowing and exact
before/start pointer-expression assertions remain in that handoff with their
original diagnostics and corrected receipts.

[#1006 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1006#issuecomment-6181920077)
and [PR1007 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1007#issuecomment-6181933475)
retain head `bed5ccec`, tree `ba76c6b2`, CI38663004252 (scope15s), eight
schemas/548 links, accepted source/pair absence and completed inspection
diagnostics. The [confrontation retained review](battle26-admission-and-geshp-confrontation.md#evidence-review-and-retained-results)
keeps #1001/#1002 and linked predecessor objects, pair/CI, factSummary-versus-row,
HTTPS, actor0/service and removed ignored Markdown-cap corrections. Linked
#997/#993/#989 completed failures and Unknowns remain with their owners;
a new task resets neither evidence nor completed suites.

The #1010 Issue/PR handoff records full prose/diff/navigation/links/anchors,
UTF-8/private/exact-five-path checks, git diff --check, configured clean committed
planner and actual exact-head CI. Documentation scope uses direct checks and
launches no blanket normal/full/Python/.NET/Godot/H3/H4/translation suite.
Natural reach, supplied state, service completion, presentation/control and
persistence remain **Unknown**. Independent acceptance and integration remain
with main-gate.
