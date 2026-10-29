# Battle35 activation and first-turn dispatch

Continue with [Battle35 outcomes and Roft return](./battle35-outcomes-and-roft-return.md).

Evidence date: 2026-10-20. **Confirmed (static):** all twelve declared
formation positions miss all four regions under the original integer
consumer. After unchanged completed fresh initialization, thirteen
STARTING enemies retain their activation words, five HIDDEN Mud Men stay
HP0/X−1, no region scene is selected and no spawn is requested. With clear535
and the base definitions, thirteen enemies still contribute thirteen turn
records. Changed eligible ally
coordinates or held region flags can admit the five hidden rows; their
initially missing stats are then initialized before appearance and round
generation. Stop at the first ExecuteIndividualTurn call, **before its body**.
Natural reach, actual state/RNG/first actor, completed services, presentation,
usable control and persistence remain **Unknown**. This is not a playable35 claim.

## Agreement, sources and bounded method

Standard rigor for [Issue #1120](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1120)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect accepted initialization to region activation, actual hidden-spawn
admission, current stats, round generation and first dispatch so subsequent
content integration can distinguish initial misses from changed state.
Accepted dependency is [Battle35 initialization and the Geshp opening](./battle35-initialization-and-opening.md),
#1118/PR1119, base `18baedb997987011940a02eb117971d58d40269b`.
Own this document and a navigation-only link there. No competing writer,
stacked dependency, shared registry/index/schema/fixture/design or engine
change. AI movement/actions/combat, outcomes and Roft interaction are excluded.

Reuse the [Battle34 shared flow](./battle34-activation-and-first-turn-dispatch.md),
[Battle30 integer derivation](../northern-travel-mitula/battle30-activation-and-first-turn-dispatch.md#geometry-checked-bits-and-persistent-flags),
[Battle26 alias/RNG derivations](../northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md),
[battle loop](../../battle/battle-loop.md), [spritesets](../../battle/battle-spriteset-data.md),
[RNG/math](../../battle/runtime-rng-and-battle-math.md),
[lifecycle](../../../design/contracts/battle/battle-control-lifecycle.md) and
[cutscene routing](../../../design/contracts/battle/battle-cutscene-routing.md).
Shared fixtures prove their named seams; selected35 rows, arithmetic,
spawn callers, refresh and actual current consumers are checked directly.

Original paths are relative to clean read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`sf2const.asm`/`sf2enums.asm` own constants and RAM. `battleloop/` and
`cutscenes/` abbreviate `code/gameflow/battle/` children; `stats/` means
`code/common/stats/`. Fnnn denotes flag nnn, ordinals are zero-based,
address intervals half-open, activation words hexadecimal. The USA ROM
SHA-256 remains `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new ROM/source equality, decoding or runtime observation is asserted.

Minimum evidence is complete selected source/table/consumer joins,
12×4 integer results and small changed-coordinate/status examples.
Reuse the immutable18,285,979-byte init/script pair and its0.083s provenance
at `local/issue1005/pair.txt`. One pair load, linear selected reads and
constant-size arithmetic have a one minute/512MiB budget per direct audit;
peak memory is unmeasured. Unique small receipts stay in `local/issue1120/`.
No extraction, acquisition, seed enumeration, forced scene lowering,
new environment/platform or old comparison. A material contradiction or
supporting-tool expansion requires reassessment before increasing scope.

## Fresh phases and the separate retained F88 route

**Confirmed:** `code/gameflow/battle/battleloop_1.asm:BattleLoop`
(`$23A84..$23BB4`) reaches @Start after fresh LoadBattle and the empty
start-wrapper return. The predecessor owns Map76/Battle35/crop16,12/48×32,
independent flags435/485/535, the scene population, all30-ally healing,
formation/list construction, enemy initialization and map reload. Then:

1. ActivateEnemies visits128–159 and falls through its helper once for160.
2. j_ExecuteBattleRegionCutscene selects against the live battle byte.
3. Nonzero DEBUG_MODE_TOGGLE.b calls PrintAllActivatedDefCons.
4. PopulateTargetsListWithSpawningEnemies rebuilds length.w. Nonzero length
   drives ascending byte IDs through SpawnEnemyWithCamera, with d0.w cleared.
5. GenerateBattleTurnOrder fills/sorts the buffer and clears the cursor byte.
6. Individual-turn entrance clears d0.w, loads CURRENT_BATTLE_TURN.b as a
   **byte offset**, reads the actor byte at BATTLE_TURN_ORDER+offset, and
   compares FF. FF returns to @Start; another value calls ExecuteIndividualTurn.

Every later phase depends on earlier services returning. There is no
transaction or rollback. A held485 is not evidence that the opening,
load, input or later activation completed. Actual party membership and
living positions govern the geometry, not scene Peter/Zynk/Caravan or
the four temporary scene Mud Men. The predecessor's empty/stale, dead,
oversized and all30-ally boundaries remain; declarations do not prove a
particular twelve-member party. Any normally placed living subset uses
the same formation prefix and misses the regions below.

F88 restores seconds, clears88/AI memory/view target, calls LoadBattle
without clearing incoming d0 and branches directly to the individual-turn
entrance. It skips fresh activation/spawning/generation before the first
read. With intact admitted d0.w=76, the predecessor proves alias address
`$FFB140+76=$FFB18C`, outside the64-byte alias table, in TEMP_ITEM_OR_SPELL.
It is not a rebuilt combatant76 alias. Unknown scratch can further select
fallback or physical49–63 beyond49 records. Retained CURRENT_BATTLE_TURN,
queue, selectors, crop, roster and RNG remain supplied state. Odd offsets
can read score bytes,128–255 exceed the128-byte queue, and a retained FF
can subsequently enter @Start using then-current state. No safe restore
or fresh-round result is assigned to this route.

## Four regions, integer inclusion and the retained checked word

**Confirmed:** `battleloop/activateenemies.asm` (`$2550C..$25544`)
rejects negative signed X.b, zero currentHP.w, then nonnegative d0.b.
There is no Y, neutral, membership, AI-order or active-bit gate. The thirteen
fresh STARTING enemies pass; hidden141–145 and absent146–159 fail X.
High-bit HP is nonzero here, unlike the positive-signed-HP ally geometry gate.

The first passing enemy invokes
`battleloop/triggerregions.asm:TriggerRegionsAndActivateEnemies`
(`$1AC9FC..$1ACF30` file). For region IDs0–3, an unchecked bit of
NEWLY_TRIGGERED_BATTLE_REGIONS.w at `$FFB20C` permits IsRegionTriggered.
A hit sets F90+ID; **both hit and miss set the checked bit**. Checked
suppresses geometry only, not activation from independently held flags.
Fresh entry clears flags90–105 separately. ClearAiMemory writes48 bytesFF
at `$FFB1AC..$FFB1DC` and48 bytes0 at `$FFB1DC..$FFB20C`; the adjacent
checked word `$FFB20C..$FFB20E` is untouched on fresh and F88 routes.

`data/battles/spritesets/spriteset35.asm` (`$1B5D76..$1B5F14`) has counts
12/18/4/1. All regions are twelve-byte type4 records, unused0, these four
ordered XY pairs and trailing0,0:

| Region / flag / type | Point1 | Point2 | Point3 | Point4 |
| --- | --- | --- | --- | --- |
|0 /90 /4|20,20|20,31|31,31|31,20|
|1 /91 /4|0,0|0,31|21,31|21,0|
|2 /92 /4|21,0|22,31|47,31|47,25|
|3 /93 /4|19,21|19,28|29,28|29,21|

Point0 is23,25. The type4 consumer tests triangle1/2/4, then3/2/4 only
after a miss. Each triangle scans all30 ally slots, requiring positive
**signed** HP.w and nonnegative signed X.b/Y.b, with no joined/active
test. A positioned living nonmember can therefore trigger. IsAllyInTriangle
returns0 inside; the scan returns−1 for any hit and0 for no hit.

Coordinates are sign-extended bytes. The edge consumer chooses dominant
absolute delta, horizontal on equality; signed MULS/DIVS truncates toward
zero and adds the projection to endpoint B. It tests edges1→2,3→1,3→2;
bounded collinearity result1 immediately accepts, otherwise opposite-vertex
side comparisons and bit0 exception branches apply. Negative projection
uses `move #1,ccr` and synthesizes2, despite the comment's3. Region index
admission uses signed byte `cmp.b d1,d7`/`ble`, admitting equality with
count; FF returns false separately. The normal loop supplies0–3, not4.
These selected coordinates are nonnegative and triangles nondegenerate.

| Formation | X,Y | R0 | R1 | R2 | R3 |
| --- | --- | --- | --- | --- | --- |
|0|45,8|out|out|out|out|
|1|45,7|out|out|out|out|
|2|44,7|out|out|out|out|
|3|43,7|out|out|out|out|
|4|45,6|out|out|out|out|
|5|44,6|out|out|out|out|
|6|43,6|out|out|out|out|
|7|44,8|out|out|out|out|
|8|43,8|out|out|out|out|
|9|42,6|out|out|out|out|
|10|42,7|out|out|out|out|
|11|42,8|out|out|out|out|

All48 results retain both actual triangles. In each initial region2
test, triangle1/2/4 rejects on its second edge and3/2/4 on its third.
For the upper sloping edge from47,25 to21,0, the B-anchored projection is
`trunc(25×(x−21)/26)`: initial x42–45 projects20–23 and rejects y6–8.
An axis-aligned rectangle would incorrectly contain those allies. The following
small changed predicates list **all** hits; they establish neither
walkability nor a naturally reached movement path:

| Point | Hit region IDs | Point | Hit region IDs |
| --- | --- | --- | --- |
|21,0|1,2|21,1|1|
|22,1|2|22,30|0,2|
|21,30|0,1|22,31|0,2|
|47,24|—|47,25|2|
|46,24|2|45,23|2|
|45,22|—|19,21|1,3|
|20,21|0,1,3|29,28|0,2,3|
|30,28|0,2|26,21|0,2,3|
|29,21|0,2,3|31,24|0,2|
|28,28|0,2,3|21,29|0,1|
|19,20|1|20,20|0,1|

At45,23 the projected23 immediately accepts the bounded edge, while an
exact cross-product polygon rejects it: the mathematical edge is at
300/13, slightly above23. The current Domain's polygon test therefore
differs on a concrete selected coordinate, not only hypothetically.

With unchanged completed fresh placement, all source flags remain clear
for **any incoming checked word**. The first passing enemy leaves
`incoming OR $000F`, not necessarily exactly000F. A later move alone
cannot recheck an already checked miss: a changed-state example must name
the checked bits it permits, or an independently held source flag.

## Complete activation rows, absent slots and fall-through160

**Confirmed:** GetTriggerRegions splits its byte into primary high and
secondary low nibbles. Both15 returns without activation write. A held
non15 primary wins and ORs1; otherwise a held non15 secondary ORs3.
Primary preserves an existing secondary bit, misses preserve the entire
word, and no path deactivates. The table's fresh values assume the base
definitions under clear535; an upgrade can change the initialized upper
priority nibble. Hidden values are the earlier hidden branch, not copied
Mud Man definitions. Orders/point operands are retained without executing AI.

| Row / ID | Definition | Primary order / trigger | Secondary order / trigger | Initial = unchanged result | Changed held flag result |
| --- | --- | --- | --- | --- | --- |
|0 /128|BOW_RIDER|MOVE_TO\|0 /0|NONE /15|`2000`|F90→`2001`|
|1 /129|CHAOS_WIZARD|FOLLOW_ENEMY\|0 /0|NONE /15|`2000`|F90→`2001`|
|2 /130|SHAMAN|FOLLOW_ENEMY\|0 /0|NONE /15|`3000`|F90→`3001`|
|3 /131|DRAGONEWT|FOLLOW_TARGET\|0 /15|NONE /15|`2000`|no activation write|
|4 /132|PURPLE_WORM|FOLLOW_ENEMY\|3 /15|NONE /15|`2000`|no activation write|
|5 /133|DRAGONEWT|FOLLOW_ENEMY\|7 /15|NONE /15|`2000`|no activation write|
|6 /134|PURPLE_WORM|FOLLOW_ENEMY\|7 /15|NONE /15|`2000`|no activation write|
|7 /135|GRIFFIN|FOLLOW_TARGET\|0 /15|FOLLOW_TARGET\|0 /15|`2000`|no activation write|
|8 /136|DRAGONEWT|NONE /1|NONE /2|`2000`|F91→`2001`; else F92→`2003`|
|9 /137|MIST_DEMON|FOLLOW_ENEMY\|8 /1|FOLLOW_ENEMY\|8 /2|`2000`|F91→`2001`; else F92→`2003`|
|10 /138|GRIFFIN|FOLLOW_TARGET\|0 /1|FOLLOW_TARGET\|0 /2|`2000`|F91→`2001`; else F92→`2003`|
|11 /139|MIST_DEMON|FOLLOW_ENEMY\|10 /1|FOLLOW_ENEMY\|10 /2|`2000`|F91→`2001`; else F92→`2003`|
|12 /140|PURPLE_WORM|FOLLOW_ENEMY\|10 /1|FOLLOW_ENEMY\|10 /2|`2000`|F91→`2001`; else F92→`2003`|
|13 /141|MUD_MAN|FOLLOW_TARGET\|0 /3|NONE /15|`0200`|skips X; dead helper F93→`0201`|
|14 /142|MUD_MAN|FOLLOW_TARGET\|0 /3|NONE /15|`0200`|skips X; dead helper F93→`0201`|
|15 /143|MUD_MAN|FOLLOW_TARGET\|0 /3|NONE /15|`0200`|skips X; dead helper F93→`0201`|
|16 /144|MUD_MAN|FOLLOW_TARGET\|0 /3|NONE /15|`0200`|skips X; dead helper F93→`0201`|
|17 /145|MUD_MAN|FOLLOW_TARGET\|0 /3|NONE /15|`0200`|skips X; dead helper F93→`0201`|

MOVE_TO0 refers to point0 `(23,25)`, coinciding with Bow Rider's start;
FOLLOW_TARGET0 is an ally-target operand. FOLLOW_ENEMY local ordinals are
0=Bow Rider128,3=Dragonewt131,7=Griffin135,8=Dragonewt136,10=Griffin138.
NONE/1 and NONE/2 on136 do not disable its activation triggers, just as a
real order with trigger15 does not create one. Geshp is no combatant here.

Absent146–159 initialize activation/maxHP/currentHP0 and X−1, retaining
other fields. Their trigger byte is0F from the **unoffset first enemy
record**, not fourteen absent records. Changing only X/HP can expose F90
activation, but does not copy a definition, make a declared row or permit
reset past the exclusive146 bound. Unchanged slots fail X.

After32 DBF iterations d0.w=160/d7.w=FFFF falls directly into
UpdateEnemyActivationBitfield; its RTS returns to BattleLoop. Pinned
jump aliases and GetCombatantEntryAddress's `bhi` admit160. The
[accepted RAM/deals derivation](../northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
gives `$FFE800+(160−96)×56=$FFF600`, CURRENT_GOLD. Pseudo-X `$FFF62E`
packs deals84/85, pseudo-HP `$FFF60E` packs20–23, triggers `$FFF636`
pack100/101, activation word `$FFF634` packs96–99. Nonnegative pseudo-X,
nonzero pseudo-HP and a held non15 trigger flag90–104 can OR1/OR3 into
Goddess Staff99's count nibble. Unchanged35 has no held region flags and
therefore no activation write even if this guard passes. Changed flags
can differ. This is neither unconditional item addition nor persistence
evidence;160 is excluded from ordinary spawn and turn scans.

## Region selector, debug text and five actual hidden-spawn paths

**Confirmed:** the complete eight-byte-record table
`data/battles/cutscenes/regionactivatedcutscenes.asm` is:

| Battle | Region | Played flag | Program |
| --- | --- | --- | --- |
|32|0|386|rbcs_battle32|
|32|1|386|rbcs_battle32|
|40|0|387|rbcs_battle40_1|
|40|1|388|rbcs_battle40_2|

WordFFFF terminates it. `cutscenes/regionactivatedcutscenes.asm`
ExecuteBattleRegionCutscene (`$47E82..$47EC8`) skips all rows while the
live selector stays35, before played/region checks or MAPSCRIPT. No35
region program is selected and none needs lowering. A changed32/40 can
match; played check precedes region check, SetFlag precedes MAPSCRIPT,
and it returns after one selected program. Such a changed selector is
not a Battle35 hook, and a set played flag does not prove completion.

PrintAllActivatedDefCons tests all sixteen flags90–105, requests text463
once per held flag with DIALOGUE_NUMBER.l=index0–15, then closes text.
Its complete ordered tokens are `["{#}","{N}","{D3}"]`; the authored
meaning is a diagnostic region number. Its scope exceeds four declared
regions. Unchanged35 requests none even with debug enabled. In the
changed20,21 example below it requests indices0,1,3 in that order.

`battleloop/populateenemyspawns.asm` (`$1ACF30..$1AD090`) scans128–159,
masks each activation word with0F00, and rebuilds TARGETS_LIST_LENGTH.w.
STARTING2000/3000 and absent0 mask to0 and skip further work; adding
active bits does not change that. HIDDEN141–145 mask to0200 and enter
UpdateEnemyActivationIfDead. With original triggers3/15 and clear F93,
each fails the trigger check even though HP/maxHP0. Length becomes0;
stale list bytes are unconsumed and appearance is bypassed.

The modes and return dependencies are:

- 0200: currentHP.w must be0 and a non15 trigger flag held; then maxHP.w
  must also be0 before reset. Primary wins; secondary sets both active bits.
- 0100: currentHP.w=0 admits reset without a region check.
- 0300: dead/trigger helper must pass, without the extra maxHP requirement.
- Other masked modes append nothing. The dead helper has no X/Y test and
  can write active bits before a later maxHP/occupancy rejection.

ResetSpawningEnemyStats reads the live battle's enemy count18 OR80 as
exclusive bound146;128–145 are real rows. It resolves the chosen row's
original start coordinates and compares them with all30 allies and32
enemies, with no HP, neutral or self exclusion. A dead actor still at its
own start blocks itself. Invalid/occupied resets max/currentHP0, X−1 and
sets carry; there is no append. Other fields need not be initialized.
Success calls full InitializeEnemyStats, then restores the saved **full
activation word**, not just active bits, and returns clear carry. The
later mode comparisons use returned full d1 rather than remasking it;
successful0201 does not append again as0100/0300.

### Bounded changed state that reaches all five rows

Supply one eligible ally at `(20,21)`, all other placed allies at an
ordinary formation subset, clear region flags, and checked bits0,1,3
clear before the first passing enemy's geometry. This is a supplied-state
derivation, not a natural movement/runtime observation. Geometry sets
F90/F91/F93; if bit3 were already checked with F93 clear, the same moved
ally would not reveal the Mud Men. Independently held F93 can instead
admit their dead helper even when geometry is skipped.

The five starts are distinct from each other, every STARTING enemy and
this changed ally/remaining formation. With no other occupant, stable
clear535 and services returning, all five reset/append:

| ID | Row start | Reset resources / definition / mover | Restored activation |
| --- | --- | --- | --- |
|141|26,21|55/0 HP/MP, MUD_MAN25, REGULAR/ATTACKER1|`0201`|
|142|29,21|55/0 HP/MP, MUD_MAN25, REGULAR/ATTACKER2|`0201`|
|143|31,24|55/0 HP/MP, MUD_MAN25, REGULAR/ATTACKER2|`0201`|
|144|28,28|55/0 HP/MP, MUD_MAN25, REGULAR/ATTACKER1|`0201`|
|145|21,29|55/0 HP/MP, MUD_MAN25, REGULAR/ATTACKER1|`0201`|

They receive the row X/Y, enemy index25, FOLLOW_TARGET0/NONE, trigger3/15,
definition status0, four empty items/spells, difficulty-adjusted/current
stats below. NOTHING skips additions; it never empties a copied loadout.
InitializeEnemyStats temporarily derives2200 from Mud Man priority2 and
mode2, but ResetSpawningEnemyStats restores saved0201. Do not substitute
2201. Hidden fields had not been copied or upgraded during the predecessor.
After success maxHP55 prevents another0200 spawn even if that unit later
has HP0, unless supplied state also restores maxHP0. Occupied starts
instead leave their slot off-map; the held flag permits a later retry.

An eligible ally at26,21 still triggers region3 but blocks141's start;
under the same other conditions the list is142,143,144,145. Occupancy by
a dead or neutral-positioned slot also blocks. Active bits can already
be0201 on a rejected slot. These variations distinguish trigger admission,
stat initialization, list append and visible appearance.

Successful IDs append in ascending order as bytes with a word length;
the full list is141,142,143,144,145. SpawnEnemyWithCamera clears d1 and
enters SpawnEnemy in `battleloop/spawnenemy.asm` (`$2448A..$24512`).
`code/common/scripting/entity/entityfunctions_1.asm:SpawnEnemyEntity`
reads actual X/Y/mover/mapsprite, masks coordinates3F, scales384, sets
facing3/eas_Idle, and selects the aerial layer only for FLYING/HOVERING.
It uses the newly selected definition/mapsprite, not the earlier four
scene Mud Men or a guessed Geshp alias.

GetEntityEvent scans64 alias bytes using a signed byte maximum, appends
above it, writes the combatant mapping (enemy ID−96), then DeclareNewEntity
writes the physical record. It does not reuse the lowest hole or guard
49-record capacity. With A normally placed allies, intact initial13
enemy mappings and no extra entities, these five allocate physical
A+13..A+17 (maximum29 when A=12), logical alias offsets45..49. Thus this
particular case fits; arbitrary changed mappings/capacity do not inherit
that safety. Stats can be reset even if a later allocation/service fails.
Sprite clearing/DMA and WaitForDmaQueueProcessing must return.

SpawnEnemy then sets cursor radius0, view/cursor target, waits for cursor
and view, hides the cursor, requests twelve orientation/property/Sleep3
iterations, SFX_SPAWN, text397 and close. Text397 is the normal dialogue
request with DIALOGUE_NAME_INDEX_1 set to the combatant; its authored
meaning announces the named arrival. Complete ordered tokens are
`["{CLEAR}","{NAME}","{D3}"]`; no fixed scene speaker/portrait is selected
by these call sites. Text463 similarly uses the ordinary txt request
with the numeric modifier above. Outer d7/a0 are preserved across spawn.
No selected story dialogue or region scene is added at this boundary.
Requests and waits are not completed camera/sprite/audio/text/control
observations. Interruption preserves the reached prefix rather than
rolling back flags, stats, mappings or earlier appearances.

## Actual current stats and completed-battle upgrades

**Confirmed:** `battleloop/initializecombatants.asm:InitializeEnemyStats`
selects/possibly upgrades the definition **before** copy, HP/MP fill,
position/type/orders/triggers/items, activation, difficulty and refresh.
`stats/updatecombatantstats.asm:UpdateCombatantStats` (`$89CE..$8BD0`)
copies base→current, applies statuses, visits four equipped item slots
in order and stores resulting status. Difficulty multiplies baseATT by5/4
with truncation once for ordinary values and again for SUPER; no AGI
difficulty mutation occurs. All selected base definitions start NONE.

The four equipped weapons have only INCREASE_ATT, followed by two NONE
effects: Hyper Cannon40, Great Rod28, Wish Staff26, Heat Axe32. They do not
change currentAGI or prowess. Zero-status refresh still calls AGI increase/
decrease with0: increase clamps numerical seven bits at127 and restores
bit7, decrease uses the full byte with cap200. All selected AGIs below128
remain unchanged. Prowess double-attack probabilities are not extra round
records. The table is conditional on selecting the original definitions:
clear535 for fresh STARTING rows and later successful Mud Man resets.
It does **not** describe the still-uninitialized hidden slots.

| Definition / combatant IDs | HP/MP | Current ATT ordinary/SUPER | DEF | Current AGI byte | MOV | First range / score envelope |
| --- | --- | --- | --- | --- | --- | --- |
|BOW_RIDER /128|61/0|95/108|55|44|7|5 /39..49|
|CHAOS_WIZARD /129|53/49|86/100|48|59|5|7 /52..66|
|SHAMAN /130|56/46|82/96|42|40|5|5 /35..45|
|DRAGONEWT /131,133,136|60/0|87/100|42|46|6|5 /41..51|
|PURPLE_WORM /132,134,140|58/0|97/121|46|43|5|5 /38..48|
|GRIFFIN /135,138|60/0|96/120|45|48|7|6 /42..54|
|MIST_DEMON /137,139|68/28|100/125|41|51|6|6 /45..57|
|MUD_MAN /141,142,143,144,145|55/0|95/118|51|41|4|5 /36..46|

Changed status/equipment/current state needs actual refresh, not a base
AGI lookup. For example supplied Mud Man SLOW=$0C00 gives multiplier3,
floor(41×3/8)=15 decrease and currentAGI26; STUN alone decreases5 to36.
Fresh enemy copy starts status0, so these are later/supplied-state examples,
not conditions naturally present at this stop. General raw high-bit
coupling retains the accepted current-stat owner; an independent immutable
ExtraRoundAction flag is not sufficient for arbitrary source refresh.

### F535 is read again for each later reset

TO_ROFT35 is in the complete accepted random-battle list. Reuse the
predecessor's [upgrade boundary](./battle35-initialization-and-opening.md#to_roft-and-completed-battle-upgrades):
F535, Bowie0's live effective level/class, live battle byte, movement
category, bounded candidate draws/exclusions and one-retry fallback govern
definition selection. Clear535 returns the original without level/RNG
upgrade. With535 held, E44 retains it; E45 can select a different permitted
definition, not necessarily stronger/currently observed. The airborne
empty excluded table has a zero-length DBF underflow and reached behavior
remains Unknown; no safe fixed Griffin/Mist Demon result is inferred.

The new caller chain here is ResetSpawningEnemyStats→InitializeEnemyStats
→UpgradeRandomBattleEnemies. Each admitted hidden reset reads535 and the
current level/class/selector afresh, before copying a definition or
appearance. It does not reuse an upgrade snapshot from STARTING128–140.
Even if thirteen initial rows used one state, later hidden rows can sample
another. Mud Man's REGULAR category uses the existing melee rules; its
selected definition/loadout/AGI/mapsprite can therefore differ from the
base table. F485 only gated the earlier opening and does not fix upgrades.
Upgrade/service RNG precedes turn generation's unknown incoming seed.
No full repeated-battle roster, upgrade draw or completed execution is
claimed. F88 remains the retained queue path above, not an upgrade run.

## Turn records, RNG, signed sorting and first dispatch

**Confirmed:** `battleloop/turnorderfunctions.asm` (`$25544..$25610`)
initializes64 two-byte records at `$FFF71A` toFFFF. It offers allies0–29,
then enemies128–157 in ascending order, excluding158/159/160. Each rejects
negative signed X.b or zero HP.w; there is no Y, membership, activation,
neutral or positive-signed-HP test. Inactive STARTING enemies participate;
unspawned141–145 and absent146–157 fail X. Successfully reset hidden rows
participate once their actual X/HP pass, irrespective of appearance timing
being independently unobserved.

Let A=currentAGI&127 and R(n) be the next **direct** GenerateRandomNumber
draw. First score is `A+R(A>>3)−R(A>>3)+R(3)−1`. Raw current byte>=128
appends an immediate second record using B=floor(A×5/6), score
`B+R(B>>3)−R(B>>3)`; both draws precede the next candidate. Word arithmetic
stores only low score bytes. For these ranges each draw updates
`seed=(seed×13+7)&65535` and returns floor(seed×n/65536), even when n=0.
The direct generator bypasses the debug override. Singles use3 draws,
doubles5, rejected candidates0. Score bounds above are conservative
algebraic envelopes, not independently attainable correlated RNG extrema.

With unchanged clear535 definitions, no selected enemy has bit7: enemy
append order128..140 yields **13 records/39 draws**. After all five
successful base Mud Man resets it is128..145: **18 records/54 draws**.
If S of these five spawn, N eligible normally placed allies and T of
those allies have raw currentAGI>=128, totals are
`13+S+N+T` records and `39+3S+3N+2T` turn draws. Thus the intact twelve-slot
placement bound is **37 records/99 draws** initially, or
**42 records/114 draws** with all five. These counts exclude earlier
upgrade/debug/spawn/service draws and do not establish the incoming seed.
For upgraded/changed enemies use actual eligibility and high bits:
`C+D` records and `3C+2D` draws for C eligible candidates, D with extra
actions. The base thirteen/eighteen counts are not universal F535 results.

Sorting performs62 passes of63 adjacent **signed byte** comparisons across
all64 records, swapping only when the right score is strictly greater.
Ties preserve append order: allies before enemies, lower offered ID first,
an actor's first record before its immediate second on equality. Padding255
is−1; a wrapped135 is−121. Existing rawAGI128 score0/255 and rawAGI127
score135 fixtures prove those seams, not Battle35's actual RNG. Original
appending has no capacity guard and62 passes do not promise fully sorted
arbitrary64 entries. Supplied30-allied/30-enemy high-bit states can overflow
the buffer; the twelve-slot bound above is not a safety claim for them.

For the bounded base cases all enemy scores are positive and capacity
fits; a non-sentinel reaches the front conditional on earlier return.
Generation clears CURRENT_BATTLE_TURN.b at `$FFF79A`, directly after
the128-byte buffer. Fresh dispatch reads offset0. The first actor still
depends on actual ally/current stats and sequential RNG; LEADER/AI
priority/Bow Rider or Chaos Wizard's AGI does not establish it. Empty
eligibility leaves FF and repeats @Start; changed negative scores can
sort behind padding. Stop at ExecuteIndividualTurn's call, before its body,
without claiming any selected action or usable player turn.

## Current consumers and missing Battle35 binding

**Confirmed about accepted code:** production consumes prepared Content;
disassembly is not an on-demand engine input. The
[capability owner](../../../../remake/docs/capability-status.md) governs runnable
scope. The predecessor retains Map76 metadata/profile, missing65 follower
prefix, native cameraSpeed and F88 gaps. These are direct static consumer
checks, without SDK, adapter, Godot or runtime execution:

| Actual owner | Existing expression or behavior | Selected gap or difference |
| --- | --- | --- |
|[Encounter preparation](../../../../src/sf2tool/remake_exploration_battles.py), [BattleEncounterDefinition](../../../../remake/src/Sf2.Remake.Application/Content/Scenarios/BattleEncounterDefinition.cs), [PrivateBattleEncounterReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs)|Placements/orders/regions/point/spawn expressions and provenance have shapes. All35 coordinates fit48×32; all four type4 records have distinct vertices and nondegenerate triangles.|Preparation admits2/3; fixed input identities and Pin admit1/2/3, reject35. Passing these geometric shape predicates alone does not bind35 terrain, definitions, route or materials.|
|[PrivateBattleScenarioReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Supported orders, numeric AGI and separate ExtraRoundAction have representations.|region-program-table admits1/2/3 only. HIDDEN rejects spawn-expression; HEALER5/HEALER3 reject source-commandset; MAGE/CENTAUR_ARCHER reject source-mover. ENHANCED definitions fail REGULAR-only enemy-derived-state; selected BIT4 also fails source-ai-word. These are inspected guards, not observed35 runtime errors.|
|[BattleActivationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [BattleTurnFlow](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs)|Activation before generation, primary/secondary OR precedence, None program and mode0 policies.|Exact cross products reject selected45,23 that original truncation accepts. Local tested=0 each round and initialization's checked0 differ from retained native checked word. Validated actors omit raw160/deals and signed-HP distinctions. Nonzero modes refuse spawn-mode; non-None programs refuse region-program. No hidden reset/list/appearance flow is admitted.|
|[BattleInitializationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs), [TurnOrderRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs)|Controlled roster/difficulty0 initialization; numerical AGI<=127 plus extra action, sequential RNG, signed62×63 sort and64 slots.|Four equipped weapons refuse enemy-equipment-refresh (requires item words0/127); NOTHING must not erase them. General equipment/status refresh, live535 definition upgrades and airborne underflow are unrepresented. Queue uses actor state AGI plus definition extra flag, which is insufficient for arbitrary changed raw high-bit refresh. Overflow refusal and validated actors differ from unsafe native writes.|
|[BattleAdvancer](../../../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs), [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|Fresh-entry continuations, round/candidate/draw/queue observations and dispatch exist; BattleStartFinished invokes BattleAdvancer.|region-program-none/spawn-modes-admitted describe admitted policies, not original spawn execution. F88 refuses suspended-battle-entry. Application checks outcome before generation and proceeds into control/AI beyond this research stop; transactional refusal does not reproduce arbitrary partial original prefixes. No composed35 dispatch is proved.|
|[SceneEntities](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [ExplorationViewRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs), [BattleMapViewport](../../../../remake/game/src/Battles/BattleMapViewport.cs), [ExplorationPresentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs)|Existing alias/population, view, board, sprite/effect/text/audio consumers;49-slot guards exist.|Original unchecked max+1 spawn allocation has no selected35 binding. Missing65 Caravan population and cameraSpeed native refusal remain. Crop-origin area selection differs from original actual view target. No selected hidden-spawn cursor/DMA/text/audio lifecycle, presentation/timing, usable input or persistence is established.|

Battle01–03 support does not establish Battle35 playability. Minimum
future integration decisions concern selected content/terrain/materials,
integer/checked-state policy, hidden reset/appearance, equipment/current
stats/upgrades and retained-entry boundaries. This research identifies
them without authorizing implementation or an acquisition platform.

## Reproduction, preserved failures and Unknowns

After loading the owning worktree's current ignored selections in the
same PowerShell process, run the bounded recipe with an unused receipt:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1120/review.py <configured-source-disasm-root> --receipt local/issue1120/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

It checks clean pinned source/unchanged pair, complete12/18/4/1 data,
48 formation predicates and22 changed coordinates, activation rows/checked
state/160 aliases, complete hook/debug/tokens, hidden reset/occupancy/
allocation/service boundaries, selected definitions/items/currentAGI,
live upgrade call chain, turn/RNG/sort/sentinel/F88 and actual consumers.
Inspect exact two-path ownership, navigation-only predecessor, UTF8/LF,
links, cached diff and private boundary. Apply ADR0004's applicable
adversarial semantics/provenance/order/selector/width/state checklist;
new parser/schema/fixture/H3 obligations are N/A. Public text is authored
paraphrase/ID/complete ordered tokens; raw joins stay ignored. Use actual
exact-head scoped CI, not suggested commands as executed passes.
Normal/full Python, SDK/.NET/Godot/emulator/H3/H4, screenshots, old
comparisons and tests of verification tools are excluded by this scope.

Preserve #1118's F88 out-of-table lookup, omitted scene65 Caravan prefix,
native cameraSpeed, equipment/upgrade/airborne boundaries. #1116's original
e26968a9 author0.500s/CI39529528549 PASS missed collision polarity;
accepted c3f0c96a has focused correction/CI39530664106, without rerunning
the whole original recipe. The [Dojo owner](./dojo-exploration-and-sheela-recruitment.md)
retains Lemon28, follow offset2=(−24,0), eas_5FFC4/5FFC8 and compile_maps45
refusals. The [post-crash owner](./post-crash-ship-and-nazca-overworld-return.md)
retains complete-import @Return, crew/RNG/mapLoad/ReloadScene boundaries;
other named predecessor failures remain with their owners. Twelve old
comparison failures remain completed discovery evidence, not interrupted
work or an automatic rerun queue. This selected35 result closes none of
those separate boundaries.

Freeze a clean committed/pushed Draft PR with exact base/head/tree,
checks/actual CI/failures/Unknowns and ignored handoff; move the Project
to Review / Verify and notify main-gate, then stop the writer/process.
Independent acceptance/merge remains main-gate's responsibility. No next
Issue, cleanup, archive or private input publication follows.

**Unknown:** natural route/admission/region reach; actual party, flags,
checked word, HP/classes/status/items, positions/aliases/deals, selector
changes, repeated-battle selections and airborne out-of-list behavior;
incoming RNG/first actor/retained F88 queue and scratch; decoded collision/
terrain/resources; interrupted/interleaved services and completed
load/debug/camera/cursor/sprite/DMA/text/audio scheduling; presentation,
VInt/render timing, usable control and save/load persistence. Individual
AI/actions/combat, outcomes and Roft interactions remain beyond the stop.
No Unknown is a passed gate or an automatic acquisition queue.
