# Battle26 activation and first-turn dispatch

Evidence date: 2026-10-14. **Confirmed:** the unchanged fresh Battle26 roster
reaches round preparation with fifteen placed, living enemies, but no party
placement inside its three regions. Activation flags, spawn modes and turn
eligibility are separate consumers: these enemies need no active-region bit
to enter the turn list. The activation loop also falls through once with
combatant160, whose stat address aliases deals storage. Its possible write is
conditional, not an observed shop event. Stop at the first call to
ExecuteIndividualTurn, before its body. **Inferred:** the formation stages
contact before the party advances into the declared regions. **Unknown:**
natural reach, actual party/state/RNG, first actor, completed services,
presentation, control, persistence, actions, outcomes and recruitment.

## Agreement and evidence boundary

Standard rigor for [Issue #1009](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1009)
under [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
distinguish initialized AI fields, checked regions, persistent flags, spawn
admission and actual turn records. Accepted starting base is
`d369869d54d157d763e940df89396014a56edc6c`, including #1005/PR1008.
This document is the sole owned path. No shared index, schema, fixture,
contract, tooling, design, remake, glossary or Chinese changes; #1010 and
UI692 are independent and provide no evidence.

Start at the accepted [initialization endpoint](./battle26-initialization-and-roster.md#return-from-before-and-ordered-fresh-initialization),
`BattleLoop:@Start`, after LoadBattle and the start wrapper returned. The
[admission owner](./battle26-admission-and-geshp-confrontation.md) owns the
prior selection and confrontation. Consume [battle loop](../../battle/battle-loop.md),
[battle lifecycle contract](../../../design/contracts/battle/battle-control-lifecycle.md),
[Battle01 placement/activation](../grans-opening/battle01-placement.md),
[RNG and turn-order evidence](../../battle/runtime-rng-and-battle-math.md),
[cutscene routing](../../battle/battle-cutscenes.md) and its
[contract](../../../design/contracts/battle/battle-cutscene-routing.md). Their observations
prove only their named seams, not a natural Battle26 traversal.

All source paths below are relative to `disasm/` at
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The retained source is clean at that pin. The accepted USA retail ROM identity
is SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Constants/RAM are `sf2enums.asm` and `sf2const.asm`; Fnnn denotes flag nnn.
`battleloop/` and `cutscenes/` abbreviate `code/gameflow/battle/` children.
All results below are conditional instruction/data deductions, not new runtime
observations. No AI body, movement, attack, damage, turn completion, outcome,
afterbattle, Higins recruitment or later map is included.

Minimum evidence is the selected pinned source, accepted configuration and
existing fixtures/payloads. Reuse the unchanged 18,285,979-byte map-init/script
pair's 0.083s receipt; this file changes none of its dependencies. Selected
source/fixture joins are linear, under one minute/512 MiB, with small prose
and receipts. No extraction regeneration, acquisition, RNG injection, new
maintained parser/harness or generic AI/shop/turn-platform audit is needed.
Replan contradictory evidence or material expansion; Unknowns are not a
runtime queue.

## Ordered round prefix and caller state

**Confirmed:** `code/gameflow/battle/battleloop_1.asm:BattleLoop`
(`$23A84..$23BB4`) orders these operations at `@Start`:

1. `bsr.w ActivateEnemies`, including its fall-through described below.
2. `jsr j_ExecuteBattleRegionCutscene`.
3. Test DEBUG_MODE_TOGGLE as a byte. Zero skips PrintAllActivatedDefCons;
   nonzero calls it before spawn admission.
4. `jsr j_PopulateTargetsListWithSpawningEnemies`; read TARGETS_LIST_LENGTH
   as a word into d7. Zero bypasses the spawn loop. Otherwise decrement d7,
   point a0 to TARGETS_LIST, clear d0.w, read the next combatant byte and call
   SpawnEnemyWithCamera; DBF repeats in stored order.
5. `bsr.w GenerateBattleTurnOrder`, then enter `@IndividualTurns_Loop`.
6. Clear d0.w, read CURRENT_BATTLE_TURN.b into d0, address BATTLE_TURN_ORDER
   with that **byte offset**, and replace d0.b with the combatant byte.
   Compare that byte with `$FF`: equal branches back to `@Start`; otherwise
   `bsr.w ExecuteIndividualTurn` is the endpoint. No score is read here and
   no HP, activation, membership or control-type check intervenes.

The initial generated offset is0. Clear/read leaves d0.w zero-extended from
the selected ID; it does not clear d0's upper longword. a0 still addresses
the turn buffer at the call. This call is not proof that its body returns or
that a player/AI action begins. There is no end-of-battle check before it.

`battleloop/printdefcons.asm` calls PrintActivatedDefCon sixteen times from
F90 through105, then closes text. Each call checks its flag; held flags write
region index as DIALOGUE_NUMBER.l and request text463 before restoring and
incrementing d1.w. These are debug-dependent text-service calls, not region
activation or evidence of displayed diagnostics. Their completion is required
to reach the later prefix.

## Activation scan, geometry and exact row effects

**Confirmed:** `battleloop/activateenemies.asm` (`$2550C..$25544`) starts
d0.w=128 and d7=31. Each of32 iterations saves d7.w, calls
UpdateEnemyActivationBitfield, restores d7.w, increments d0.w and DBFs.
The helper first reads X and rejects a negative **signed byte**, then reads
current HP and rejects only a zero **word**, then rejects nonnegative d0.b.
It does not test Y, membership, the neutral bit, move orders or commandsets.
A supplied HP word with bit15 set is nonzero here; this is not a positive-HP
comparison. The selected ordinary enemies128–142 pass with their initialized
positive HP and X;143–158 fail X. Neutral159 has X=18/Y=14, activation `$0008`
and HP0 after the clear526 neutral load, so it fails HP, not a neutral-bit
predicate. With526 held, fresh159 has X=-1/HP0 instead.

`battleloop/triggerregions.asm:TriggerRegionsAndActivateEnemies` saves d0–a6
and the combatant index. GetBattleSpritesetSubsection selects the current
battle's region subsection. Zero regions returns before either region scans
or activation writes. For Battle26, count3 gives indices0,1,2. Each consults
NEWLY_TRIGGERED_BATTLE_REGIONS.w (`$FFB20C`): a held bit skips geometry;
an unset bit calls IsRegionTriggered and sets F90+index only on nonzero d1.b.
It then sets the checked bit **whether or not geometry triggered**. The word
is not cleared by ActivateEnemies or this `@Start` prefix. Persistent F90–105
are not cleared here either; the accepted fresh initialization cleared them.
The separately read `ai/startaicontrol.asm:@NonSwarmAi` contains a later
zero write to the checked word. That future body is outside this endpoint
and cannot be used to assume the incoming checked word was zero.

### Geometry and actual ally lookup

**Confirmed:** IsRegionTriggered uses twelve-byte region records and vertex
words at offsets2/4/6/8. Type3 selects triangle1/2/3; other types select
triangle1/2/4, then triangle3/2/4 only if the first contains no eligible ally.
The complete selected geometry is:

| Region / flag | Type | Vertices1,2,3,4 |
| --- | --- | --- |
|0 /90|4|`(21,0),(12,31),(35,31),(35,0)`|
|1 /91|3|`(4,0),(34,31),(34,0),(0,0)`|
|2 /92|4|`(0,0),(0,21),(35,21),(35,0)`|

AreAnyAlliesInTriangle scans all30 ally slots0–29, not the active-party list.
It requires current HP.w **positive signed**, X.b nonnegative and Y.b
nonnegative, then packs X/Y into a word for IsAllyInTriangle. The latter
returns zero for inside; the caller inverts this into d6.w=-1 for any match
or0 after the complete scan. IsRegionTriggered returns that word in d1.
Thus changed living placed nonmembers can trigger a region, while dead,
negative-HP or offscreen slots cannot. Neutral159 is not in this ally scan.

CheckTestPointPositionRelativeToSegment sign-extends byte coordinates,
chooses the larger absolute X/Y delta (horizontal on equality), and uses
signed multiplication/division with truncation toward zero to project onto
the edge. Collinearity then uses boundedness tests. IsAllyInTriangle accepts
an edge's result1 immediately, otherwise compares the ally's side with the
opposite vertex, with separate bit0-set exception branches. This is the
actual integer consumer, not an unrestricted floating-point polygon rule.
Selected coordinates are nonnegative and the declared triangles nondegenerate.
IsRegionTriggered's `$FF` index returns false; its count comparison is
`cmp.b d1,d7` / **ble**, not a strict index<count guard. For normal Battle26
iteration only0–2 are passed. Do not generalize safe arbitrary indexing from
the source comment or treat trigger nibble15 as a polygon lookup.

The twelve initialized placement ordinals are `(7,25),(6,25),(5,25),(4,25),
(6,26),(5,26),(4,26),(6,27),(5,27),(4,27),(7,26),(7,27)`. Every point is
outside all three selected regions, including the integer edge tests: region0
is to their right, region1's diagonal excludes this lower-left area, and
region2 ends at Y21. Fewer living party members only use a subset of these
positions; unused ally X=-1 is rejected. Therefore unchanged fresh
initialization leaves F90–92 clear regardless of preexisting checked bits.
The first passing enemy causes the low three checked bits to be set; other
bits remain supplied. Exact checked word7 requires the other bits clear.
This conclusion assumes completed initialization and no intervening actor
position/HP/flag mutation, not a fixed named party or an observed route.

### Per-row activation writes

**Confirmed:** GetTriggerRegions splits the stored byte into primary high
nibble d1 and secondary low nibble d2. After scanning regions, both15 returns
without writing activation. Otherwise the consumer checks the primary flag
first; held primary sets bit0 and returns, so secondary is not selected.
If primary is15 or its flag clear, a non15 held secondary sets bits0 and1.
A miss leaves the word unchanged; neither branch clears prior active state.
The primary `andi.w #($FFFF-AIBITFIELD_PRIMARY_ACTIVE)` followed by bset0
preserves bit1. The secondary expression plus bset0/bset1 also preserves
all other bits. Fresh values have both bits clear. No neutral-bit exclusion
occurs in this consumer.

All fifteen rows and their selected consequences are:

| Row / ID | Type | Trigger primary/secondary | Fresh word | If relevant flag becomes held |
| --- | --- | --- | --- | --- |
|0 /128|Lizardman|15/15|`2000`|no activation write|
|1 /129|Wizard|15/15|`2000`|no activation write|
|2 /130|Dark Bishop|15/15|`3000`|no activation write|
|3 /131|Lesser Demon|1/1|`2000`|F91 -> `2001`, primary wins|
|4 /132|Dark Soldier|1/15|`2000`|F91 -> `2001`|
|5 /133|Dark Sniper|1/15|`2000`|F91 -> `2001`|
|6 /134|Dark Sniper|2/2|`2000`|F92 -> `2001`, primary wins|
|7 /135|Dark Soldier|2/15|`2000`|F92 -> `2001`|
|8 /136|Lesser Demon|2/15|`2000`|F92 -> `2001`|
|9 /137|Dark Soldier|0/0|`2000`|F90 -> `2001`, primary wins|
|10 /138|Dark Soldier|0/15|`2000`|F90 -> `2001`|
|11 /139|Lesser Demon|0/15|`2000`|F90 -> `2001`|
|12 /140|Lesser Demon|15/15|`2000`|no activation write|
|13 /141|Dark Bishop|15/15|`3000`|no activation write|
|14 /142|Dark Sniper|15/15|`2000`|no activation write|

Words are hexadecimal, for the accepted unupgraded definitions. With fresh
flags and unchanged positions all words remain at their fresh values. No
selected row takes a secondary-only path: its secondary is either15 or equal
to primary. Changed trigger bytes/flags can take that branch; a previously
held secondary bit can survive a later primary write. Initialized priority,
NONE sentinels and HEALER/ATTACKER/LEADER names do not establish chosen tactics
or control ownership. Completion526-held upgraded definitions can change
priority/stats, but the initialized row triggers and STARTING spawn mode are
still the accepted row data, conditional on the upgrade service returning.

## The extra fall-through and its selected RAM alias

**Confirmed:** after the final DBF, d0.w is160 (`$00A0`) and d7.w=`$FFFF`.
There is **no RTS, branch or emitted data** before the immediately following
UpdateEnemyActivationBitfield label. Its final RTS therefore returns to the
original BattleLoop caller, after one extra helper execution for160.
`layout/sf2-05-0x020000-0x028000.asm` includes this whole file before
turnorderfunctions.asm; a source function-end comment emits no instruction.

The s02 jump aliases resolve the actual X/HP/trigger/activation getters and
setter. In `code/common/stats/combatantstats_3.asm:GetCombatantEntryAddress`,
`cmpi.b #COMBATANT_ENEMIES_SPACE_END,d0` followed by **bhi** rejects values
above160, not160 itself. Subtracting96 maps160 to storage index64;
`64*56 + $FFE800 = $FFF600`. This is CURRENT_GOLD, immediately before
DEALS_ITEMS at `$FFF604`, not an allocated enemy. The selected fields are:

| Pseudo-field | Width / address | Actual deals interpretation |
| --- | --- | --- |
|X +46|byte `$FFF62E`|packed quantities for item84/85; bit7 causes the signed-X rejection|
|current HP +14|word `$FFF60E`|four nibbles for Protect Ring20, Quick Ring21, Running Ring22, White Ring23; zero rejects|
|triggers +54|byte `$FFF636`|quantities for items100/101 become primary/secondary region nibbles|
|activation +52|word `$FFF634`|packed quantities for items96–99; its low bits belong to Goddess Staff99|

`code/common/stats/dealsinventory.asm:GetDealsItemInfo` confirms address
DEALS_ITEMS+floor(item/2), even items in the high nibble and odd items in the
low nibble. This is a field/address join, not a call to a shop add routine.
If pseudo-X is nonnegative, the ring word nonzero, and a non15 pseudo-trigger
references a held flag90–104, the normal activation setter ORs bit0 into the
Goddess Staff count, or bits0/1 for the secondary path. Other quantity bits
are preserved. An already-set bit need not change the count; this is **not**
unconditional addition of one staff, a saturating increment, or proof that
a shop was visited. The source's BUG comment alone does not establish it.

For unchanged fresh Battle26, F90–105 were cleared and no selected geometry
sets them. Even an eligible pseudo-combatant finds no held trigger flag and
performs no deals write. Changed positions/held flags/deals contents can alter
that result; the helper's earlier per-enemy flag writes are already visible
to this extra call. No unrelated shop behavior is audited or observed.

## Region hook and initial spawn admission

**Confirmed:** the complete `data/battles/cutscenes/regionactivatedcutscenes.asm`
table has four eight-byte records and a word `$FFFF` terminator:

| Battle | Region | Played flag | Script |
| --- | --- | --- | --- |
|TO_MOUN32|0|386|rbcs_battle32|
|TO_MOUN32|1|386|rbcs_battle32|
|VERSUS_ODD_EYE40|0|387|rbcs_battle40_1|
|VERSUS_ODD_EYE40|1|388|rbcs_battle40_2|

`cutscenes/regionactivatedcutscenes.asm:ExecuteBattleRegionCutscene`
(`$47E82`) preserves d0/d1/a0, advances by8, checks the terminator, then
compares CURRENT_BATTLE.b. With26 it skips all four rows and returns without
checking played/region flags, setting a flag or dispatching MAPSCRIPT.
This proves the Battle26 disposition from the complete table, not from a
missing per-battle file. For changed current battle32/40, the wrapper checks
played flag then region, sets the played flag **before** loading the script
pointer/trap, and returns after that selected script rather than continuing
the scan. Interruption can leave the played flag set before script completion.

### Spawn list and changed-state conditions

**Confirmed:** `battleloop/populateenemyspawns.asm` (`$1ACF30`) preserves
d0–a6, scans32 slots128–159 in order, and starts its list count d5.w at0.
It reads each activation word and masks `$0F00`. Zero branches directly to
Next, without HP, region, position, item or reset calls. All fifteen Battle26
rows are STARTING0; initialization keeps only definition priority `$F000`
and row initialization bits, and the activation consumer changes only bits0/1.
Thus these fresh rows still have masked mode0 even if regions fire or the
definitions upgrade. Absent143–158 have0;159 has0 or neutral8, also masked0.
All32 therefore skip reset and append, TARGETS_LIST_LENGTH.w becomes0,
and BattleLoop skips SpawnEnemyWithCamera. Old list bytes are not cleared,
but the zero length makes them unconsumed. No item, respawn counter, camera,
animation, sound or appearance text is executed by this skipped branch.

A row's name alone would not prove this: supplied mode bits can differ.
For changed state the actual branches are:

- `$0200` hidden: UpdateEnemyActivationIfDead requires current HP.w=0 and a
  held non15 primary or secondary flag. It sets primary bit0, or both bits,
  and returns d0.w=0 on success/-1 otherwise. Then max HP.w must be0 before
  ResetSpawningEnemyStats is called.
- `$0100` respawn: current HP.w=0 admits reset without a region requirement.
- `$0300` hidden+respawn: the same dead/trigger helper must succeed; no
  additional max-HP test follows.
- Other nonzero masked values match none of these arms and append nothing.

ResetSpawningEnemyStats in `battleloop/initializecombatants.asm` requires a
real row: for Battle26 the subsection count15 OR `$80` gives bound143;
combatant>=143 fails. It reads that row's starting coordinates and calls
IsEnemyStartingPositionOccupied, which compares X/Y of all30 allies and32
enemies without an HP/neutral filter or self-exclusion. A dead slot still
at its own start can therefore block its reset. Invalid row/occupied start
writes max/current HP0 and X=-1, sets carry and is not appended. Success
calls InitializeEnemyStats (including upgrade, current stats and item setup),
then restores the saved activation word. The successful source path returns
clear carry through the existing setter path; the caller appends d4.b and
increments d5.w only after a clear-carry return. Scanning order is ascending
slot order; list length is a word, each appended ID a byte. The source's later
mode comparisons use returned d1, not a fresh mode-mask load. Reset preserves
that full activation word. After a successful hidden reset its mode remains
$0200, so it matches neither later $0100 nor $0300; after a respawn reset
it remains $0100 and does not match $0300. Active/priority bits do not change
that distinction. The append is therefore once per successful selected slot,
not a second insertion caused by source fall-through.

A successful changed-state reset reuses the accepted initializer: row2's
Black Ring insertion strips equip/AI-use/drop bits through AddItem; ordinary
NOTHING rows leave copied equipment. No separate spawn-specific item award
or monotonically incremented spawn counter exists in this selected chain.
These possible writes are excluded for the unchanged fresh roster by mode0.

If the list is nonempty, SpawnEnemyWithCamera clears d1.w and falls into
SpawnEnemy (`battleloop/spawnenemy.asm`). It saves d7/a0, stores the combatant,
calls j_SpawnEnemyEntity, then the zero camera option clears CURSOR_RADIUS.b,
sets VIEW_TARGET_ENTITY.b from lookup, requests cursor destination and waits
for cursor/view completion before hiding the cursor. Twelve iterations
(d7=11 through0) request orientation `(d7+3)&3` with d2=d3=-1 and Sleep3,
then request SFX_SPAWN, write DIALOGUE_NAME_INDEX_1.w, request text397 and
close text. d7/a0 restoration preserves the outer list traversal. These are
called-service and counter boundaries only; no simulated animation, timing,
entity completion, visible text or camera result is claimed.

## Turn buffer, randomized agility and first dispatch

**Confirmed:** `battleloop/turnorderfunctions.asm:GenerateBattleTurnOrder`
starts at `$25544`, AddCombatantAndRandomizedAgiToTurnOrder at `$255A4`.
The buffer is `$FFF71A`,64 two-byte records,128 bytes; CURRENT_BATTLE_TURN.b
is the next address `$FFF79A`. Every record is first written `$FFFF`
(ID255, score255/signed -1). a0 is restored to its start for appends.

The ally loop is0–29 (counter29); the enemy loop is **128–157** because it
uses BATTLE_ENEMY_ENTITIES_COUNTER29, not COMBATANT_ENEMIES_COUNTER31.
Thus158/159 are never offered to this constructor, independently of HP or
neutral flags. AddCombatantAndRandomizedAgiToTurnOrder tests signed-byte X
nonnegative, then HP.w nonzero; no Y, active-region, neutral, active-party,
commandset or membership test follows. Consequently every selected fresh
enemy128–142 qualifies even while region flags and active bits are clear.
Fresh143–157 fail X. Neutral159 both has HP0 and lies outside this loop;
it supplies no turn record. A separately joined/active Higins19 would be
an ally candidate under the ordinary ally predicates, not this neutral actor.

The constructor uses **current** AGI, read as an unsigned byte into d1.w;
d3 retains the raw value. Let A=rawAGI&`$7F` and R(n) be the successive
GenerateRandomNumber result for range n:

- First score: `A + R(A>>3) - R(A>>3) + R(3) - 1`.
- If rawAGI>=`$80`, immediately append a second record for the same ID with
  `B=floor(A*5/6)` and score `B + R(B>>3) - R(B>>3)`.
- Store only each result's low byte after word arithmetic. The second turn
  has no final R(3)-1. Rejected candidates consume no draws; admitted singles
  consume3 draws and doubles5, in ally-then-enemy slot order.

`code/common/tech/randomnumbergenerator.asm` (`$1600..$1628`) advances the
word seed with `(seed*13+7)&$FFFF` on **every** call, including range0.
Its doubled-range multiply/high-word/shift yields floor(seed*n/65536) for
these small ranges; d6.w is restored. The calls here use that direct helper,
not the debug-direction override. One cannot independently choose the three
results or infer one actual actor without the incoming seed/current stats.

The complete fresh, unupgraded enemy join is:

| ID | Type | Current HP | Current AGI | A>>3 | Records |
| --- | --- | --- | --- | --- | --- |
|128|Lizardman|40|38|4|1|
|129|Wizard|37|44|5|1|
|130|Dark Bishop|39|27|3|1|
|131|Lesser Demon|40|32|4|1|
|132|Dark Soldier|35|34|4|1|
|133|Dark Sniper|38|35|4|1|
|134|Dark Sniper|38|35|4|1|
|135|Dark Soldier|35|34|4|1|
|136|Lesser Demon|40|32|4|1|
|137|Dark Soldier|35|34|4|1|
|138|Dark Soldier|35|34|4|1|
|139|Lesser Demon|40|32|4|1|
|140|Lesser Demon|40|32|4|1|
|141|Dark Bishop|39|27|3|1|
|142|Dark Sniper|38|35|4|1|

These current AGIs follow the predecessor's selected definitions, clear
initial status and equipped-item effects; difficulty changes ATT, not this
AGI join. They are not universal upgraded/supplied values. All fifteen first
scores remain positive and below128 for any permitted draws; there are45
enemy draws, after actual ally draws. With A_count eligible initialized
allies and T of them at rawAGI>=128, record count is `15+A_count+T`, at most39
for the accepted twelve-position roster. Changed party stats can add second
ally records or negative wrapped scores; no fixed party name/order follows.

Sorting performs62 outer passes (COMBATANTS_ALL_COUNTER61) of63 adjacent
comparisons (TURN_ORDER_ENTRIES_MINUS_ONE_COUNTER62). It reads whole words,
compares their **low bytes** with `cmp.b d0,d1` and signed `ble`, swapping
words only when the right score is strictly greater. Equal scores retain
insertion order, including padding ties; lower ally ID precedes later ally
ID, and allies precede enemies on equal first-entry scores because of append
order. A doubled actor's second entry is appended immediately after its first.
Unsigned135 sorts as -121, and padding255 sorts as -1; sorting is over the
whole64-record buffer, not a filtered living prefix. The accepted boundary
fixture demonstrates rawAGI128's scores0/255 and rawAGI127's score135.

There is no append-capacity guard. The fixed buffer64 is not permission for
arbitrary60 candidates to contribute120 records, and62 passes are not a
general proof of full sorting for every possible64-entry arrangement. The
selected fresh Battle26 maximum39 fits; its positive enemy scores ensure a
non-sentinel first entry after these passes. Upgraded definitions/supplied
large parties require re-evaluating record count and score conditions, not
assuming this bound. No overflow experiment or generic platform fix is made.

Finally CURRENT_BATTLE_TURN.b is cleared and RTS returns. BattleLoop's byte
read above selects offset0. A completely empty eligible set leaves `$FF`
there and loops to `@Start`, repeating preparation rather than dispatching a
turn or declaring victory/defeat. Padding can also sort ahead of scores below
-1 in changed states. For the selected fresh positive-enemy case a first call
exists conditional on all earlier services returning, but the actual actor
still depends on the party, current AGIs and sequential RNG. Commandset names
do not prove player-first, enemy-first or AI ownership at this boundary.

## Supplied continuation, interruption and limits

**Confirmed:** the F88 branch restores saved seconds, clears88/AI memory,
clears VIEW_TARGET_ENTITY and calls LoadBattle, then branches directly to
`@IndividualTurns_Loop`. Its subsequent branch to `@Start` is unreachable.
It initially bypasses fresh activation, region/debug/spawn preparation and
turn generation, consuming supplied CURRENT_BATTLE_TURN and buffer bytes.
No record alignment/range validation occurs in the byte-offset read. A
supplied non-FF ID reaches the call without rebuilding eligibility; a supplied
FF entry returns to `@Start`, where preparation uses actual CURRENT_BATTLE,
flags/positions/HP/modes/AGI. LoadBattle's neutral writes mean this is not
byte-for-byte preservation. Incoming map/battle arguments do not override
supplied CURRENT_BATTLE on this branch.

| Changed premise / interruption | Boundary retained |
| --- | --- |
|Party membership/HP/positions differ|Initialization and geometry scan predicates differ; region flags need not follow the nominal formation.|
|Checked-region word supplied|Held bits suppress new geometry; fresh reset of F90–105 alone does not clear that word.|
|Persistent flags or active bits held|This prefix does not clear them; primary-first and secondary writes preserve prior unrelated bits.|
|Completion526 held/upgraded type returned|Row triggers/mode survive initialization; priority, HP/AGI/items and turn capacity require actual definition/state.|
|Neutral or absent slot changed|No universal neutral filter in activation/spawn/turn helper; concrete HP, mode, bounds and positions control each consumer.|
|Deals bytes/trigger flags changed|The slot160 alias can modify Goddess Staff count bits under the exact gates above.|
|Debug byte nonzero|Diagnostic text services intervene before spawning/generation; returning is a separate premise.|
|Spawn mode changed|Dead/flag/max-HP/row/occupancy gates can write state before append or reject with HP0/X=-1.|
|RNG/current AGI changed|Scores, extra records, signed ordering and first actor change; seed is consumed even for range0.|
|F88 or current-turn/buffer altered|Initial supplied dispatch bypasses preparation; sentinel re-enters it using supplied current battle/state.|
|Interrupted before first call|Earlier flag/deals/initializer/list/seed/buffer writes are not rolled back. No atomic round-preparation guarantee.|

**Unknown:** natural Battle26 reach, actual selected party/class/HP/items,
flags/deals/checked word, saved/current map/battle/turn state and seed; shared
service return, asynchronous display/audio/control and save persistence.
No ExecuteIndividualTurn body, selected tactic, movement/attack, resolved
turn, battle outcome or Higins recruitment is established.

## Reproduction and review boundary

Load ignored `local/private-inputs.ps1` in the same process as controlled
commands. Reuse the clean pinned checkout and accepted #1005 configuration,
complete spriteset/definition joins and retained pair receipt. Do not
regenerate the pair or launch H3 for this documentation-only slice.

| Named join | What it establishes / limit |
| --- | --- |
|`battle-loop-static-v1.json` / `_build_lifecycle_facts`; `battle-control-static-v1.json` / `_build_control_facts`|Existing source/fixture lifecycle and top-level ordering; full selected bodies are still read for bounds, fall-through and widths.|
|Retained `battle-spriteset-data-static.json`, public fixture and complete spriteset26/definitions|Header/source identity plus all selected rows/regions/current-stat join; public `rowContentTracked=false` does not supply row bodies.|
|`battle-cutscenes-static-v1.json` / `_cutscene_facts`, actual complete region table|Wrapper order and the four-row/terminator join proving current26's no-match return.|
|Stat getters/setter/address helper, constants, s02/s07/s13 aliases, deals field decoder|Slot160 acceptance, exact overlapping addresses, branch predicates and bit writes; no shop observation.|
|Turn source, RNG source and `battle01-turn-order-v1.json`, `turn-order-boundaries-v1.json`, `battle01-region-activation-v1.json`|Actual arithmetic/filter/signed-sort/region consumer versus accepted seam facts; no new Battle26 runtime or seed selection.|
|Spawn/reset/occupancy bodies and complete initialization-mode join|Mode0 skips all fresh candidates; changed-state gates and called-service ordering, not completed animation.|

The ignored `local/issue1009/review.py` receipt compares selected actual
source/fixture joins and exact row/consumer operands without becoming a new
maintained parser. Reproduce with configured
`uv run --locked python -X utf8 local/issue1009/review.py <source-disasm-root>`.
Review the complete prose, changed states and cached diff, links/anchors,
UTF-8/LF, sole-path/private boundaries and `git diff --check`. On a clean
committed head run `uv run sf2 verify plan --base origin/main --head HEAD`;
record actual selection and exact-head CI in the Issue. Blanket local
normal/full/engine/Godot/H3/H4 suites are skipped under this Issue agreement.

Preserve [#1005 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1005#issuecomment-6181975872)
and [PR1008 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1008#issuecomment-6181990706):
head033ebbdd/tree2daf762b/basef1958d33, CI38663398489 (scope14s/research42s)
and originalCI38663279580, unchanged pair0.083s and completed source/data
checks. Retain the ignored inspect.py standard-library shadowing failure and
rename, exact pointer-expression assertion correction, earlier Higins ID
correction, neutral membership versus exact-source distinction and linked
#1001/#997/#993/#989 failures/Unknowns. No completed result is reset or called
interrupted. This slice's diagnostics/corrections and final objects/CI belong
in its Issue handoff. Freeze a clean pushed Draft PR for independent review;
no merge, cleanup, archive, next Issue or runtime.
